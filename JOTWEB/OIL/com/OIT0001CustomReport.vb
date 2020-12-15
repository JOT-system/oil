Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 空回日報個別帳票作成クラス
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）</remarks>
Public Class OIT0001CustomReport : Implements IDisposable
    ''' <summary>
    ''' エクセルアプリケーションオブジェクト
    ''' </summary>
    Private ExcelAppObj As Excel.Application
    ''' <summary>
    ''' エクセルブックコレクション
    ''' </summary>
    Private ExcelBooksObj As Excel.Workbooks
    ''' <summary>
    ''' エクセルブックオブジェクト
    ''' </summary>
    Private ExcelBookObj As Excel.Workbook
    ''' <summary>
    ''' エクセルシートコレクション
    ''' </summary>
    Private ExcelWorkSheets As Excel.Sheets
    ''' <summary>
    ''' エクセルシートオブジェクト
    ''' </summary>
    Private ExcelWorkSheet As Excel.Worksheet
    ''' <summary>
    ''' 一時作業シート
    ''' </summary>
    Private ExcelTempSheet As Excel.Worksheet

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private xlProcId As Integer

    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                      "PRINTFORMAT",
                                                      C_DEFAULT_DATAKEY,
                                                      mapId, excelFileName)
            Me.UploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                   "PRINTWORK",
                                                   CS0050SESSION.USERID)
            'ディレクトリが存在しない場合は生成
            If IO.Directory.Exists(Me.UploadRootPath) = False Then
                IO.Directory.CreateDirectory(Me.UploadRootPath)
            End If
            '前日プリフィックスのアップロードファイルが残っていた場合は削除
            Dim targetFiles = IO.Directory.GetFiles(Me.UploadRootPath, "*.*")
            Dim keepFilePrefix As String = Now.ToString("yyyyMMdd")
            For Each targetFile In targetFiles
                Dim fileName As String = IO.Path.GetFileName(targetFile)
                '今日の日付が先頭のファイル名の場合は残す
                If fileName.StartsWith(keepFilePrefix) Then
                    Continue For
                End If
                Try
                    IO.File.Delete(targetFile)
                Catch ex As Exception
                    '削除時のエラーは無視
                End Try
            Next targetFile
            'URLのルートを表示
            Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

            'Excelアプリケーションオブジェクトの生成
            Me.ExcelAppObj = New Excel.Application
            ExcelAppObj.DisplayAlerts = False
            Dim xlHwnd As IntPtr = CType(Me.ExcelAppObj.Hwnd, IntPtr)
            GetWindowThreadProcessId(xlHwnd, Me.xlProcId)
            'Excelワークブックオブジェクトの生成
            Me.ExcelBooksObj = Me.ExcelAppObj.Workbooks
            Me.ExcelBookObj = Me.ExcelBooksObj.Open(Me.ExcelTemplatePath,
                                                UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                                [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
            Me.ExcelWorkSheets = Me.ExcelBookObj.Sheets
            Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("空回日報"), Excel.Worksheet)
            'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)

        Catch ex As Exception
            If Me.xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData(ByVal I_officeCode As String, Optional ByVal repPtn As String = Nothing) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Select Case repPtn
                '★受注一覧(帳票)よりダウンロード
                Case "KUUKAI_SODEGAURA"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーと明細の設定
                    EditHeaderDetailArea(I_officeCode)
                    '***** TODO処理 ここまで *****
                    'ExcelTempSheet.Delete() '雛形シート削除

                    '★空回日報画面よりダウンロード
                Case Else
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditHeaderArea(I_officeCode)
                    '◯明細の設定
                    EditDetailArea(I_officeCode)
                    '***** TODO処理 ここまで *****
                    'ExcelTempSheet.Delete() '雛形シート削除
            End Select

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
            End SyncLock
            Me.ExcelBookObj.Close(False)

            'ストリーム生成
            Using fs As New IO.FileStream(tmpFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                Dim binaryLength = Convert.ToInt32(fs.Length)
                ReDim retByte(binaryLength)
                fs.Read(retByte, 0, binaryLength)
                fs.Flush()
            End Using
            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        Finally
            ExcelMemoryRelease(rngWrite)
        End Try

    End Function

    ''' <summary>
    ''' 帳票のヘッダー(共通)設定
    ''' </summary>
    Private Sub EditHeaderCommonArea(ByVal I_officeCode As String,
                                     ByVal I_rngHeaderArea As Excel.Range,
                                     ByVal PrintDatarow As DataRow,
                                     ByVal I_column As Integer)

        Dim iHeader(,) As Integer = {{3, 7, 9, 41, 4}, {45, 49, 51, 83, 46}, {87, 91, 93, 125, 88}, {129, 133, 135, 167, 130}}
        Dim strTrainNo() As String = {"5461", "5972"}
        Dim i As Integer = 0

        '◯ 営業所名
        I_rngHeaderArea = Me.ExcelWorkSheet.Range("E" + Convert.ToString(iHeader(I_column, i)))
        I_rngHeaderArea.Value = PrintDatarow("OFFICENAME")
        ExcelMemoryRelease(I_rngHeaderArea)
        '◯ 向い先(着駅)
        I_rngHeaderArea = Me.ExcelWorkSheet.Range("E" + Convert.ToString(iHeader(I_column, i + 1)))
        I_rngHeaderArea.Value = PrintDatarow("ARRSTATIONNAME")
        ExcelMemoryRelease(I_rngHeaderArea)
        '### 20201019 START 指摘票対応(No177) ####################################
        '◎袖ヶ浦営業所の場合
        If I_officeCode = BaseDllConst.CONST_OFFICECODE_011203 Then
            '★列車No(5461⇒5972へ変更)
            If Convert.ToString(PrintDatarow("TRAINNO")) = strTrainNo(0) Then
                '◯ 列車No
                I_rngHeaderArea = Me.ExcelWorkSheet.Range("N" + Convert.ToString(iHeader(I_column, i + 1)))
                I_rngHeaderArea.Value = strTrainNo(1)
                ExcelMemoryRelease(I_rngHeaderArea)
                I_rngHeaderArea = Me.ExcelWorkSheet.Range("M" + Convert.ToString(iHeader(I_column, i + 3)))
                I_rngHeaderArea.Value = strTrainNo(1)
                ExcelMemoryRelease(I_rngHeaderArea)
            Else
                '◯ 列車No
                I_rngHeaderArea = Me.ExcelWorkSheet.Range("N" + Convert.ToString(iHeader(I_column, i + 1)))
                I_rngHeaderArea.Value = PrintDatarow("TRAINNO")
                ExcelMemoryRelease(I_rngHeaderArea)
                I_rngHeaderArea = Me.ExcelWorkSheet.Range("M" + Convert.ToString(iHeader(I_column, i + 3)))
                I_rngHeaderArea.Value = PrintDatarow("TRAINNO")
                ExcelMemoryRelease(I_rngHeaderArea)
            End If
            '◯ 日付
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("R" + Convert.ToString(iHeader(I_column, i + 4)))
            I_rngHeaderArea.Value = Now.ToString("yyyy年MM月dd日")
            ExcelMemoryRelease(I_rngHeaderArea)
            '◯ 積込日（予定）
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("E" + Convert.ToString(iHeader(I_column, i + 2)))
            I_rngHeaderArea.Value = PrintDatarow("LODDATE")
            ExcelMemoryRelease(I_rngHeaderArea)
            '◯ 発日（予定）
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("J" + Convert.ToString(iHeader(I_column, i + 2)))
            I_rngHeaderArea.Value = PrintDatarow("DEPDATE")
            ExcelMemoryRelease(I_rngHeaderArea)
            '◯ 積車着日（予定）
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("M" + Convert.ToString(iHeader(I_column, i + 2)))
            I_rngHeaderArea.Value = PrintDatarow("ARRDATE")
            ExcelMemoryRelease(I_rngHeaderArea)
            '◯ 受入日（予定）
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("P" + Convert.ToString(iHeader(I_column, i + 2)))
            I_rngHeaderArea.Value = PrintDatarow("ACCDATE")
            ExcelMemoryRelease(I_rngHeaderArea)

        Else
            '◯ 列車No
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("M" + Convert.ToString(iHeader(I_column, i + 1)))
            I_rngHeaderArea.Value = PrintDatarow("TRAINNO")
            ExcelMemoryRelease(I_rngHeaderArea)
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("K" + Convert.ToString(iHeader(I_column, i + 3)))
            I_rngHeaderArea.Value = PrintDatarow("TRAINNO")
            ExcelMemoryRelease(I_rngHeaderArea)

            '◯ 積込日（予定）
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("E" + Convert.ToString(iHeader(I_column, i + 2)))
            I_rngHeaderArea.Value = PrintDatarow("LODDATE")
            ExcelMemoryRelease(I_rngHeaderArea)
            '◯ 発日（予定）
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("J" + Convert.ToString(iHeader(I_column, i + 2)))
            I_rngHeaderArea.Value = PrintDatarow("DEPDATE")
            ExcelMemoryRelease(I_rngHeaderArea)
            '◯ 積車着日（予定）
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("L" + Convert.ToString(iHeader(I_column, i + 2)))
            I_rngHeaderArea.Value = PrintDatarow("ARRDATE")
            ExcelMemoryRelease(I_rngHeaderArea)
            '◯ 受入日（予定）
            I_rngHeaderArea = Me.ExcelWorkSheet.Range("N" + Convert.ToString(iHeader(I_column, i + 2)))
            I_rngHeaderArea.Value = PrintDatarow("ACCDATE")
            ExcelMemoryRelease(I_rngHeaderArea)
        End If
        '### 20201019 END   指摘票対応(No177) ####################################

    End Sub

    ''' <summary>
    ''' 帳票の明細(共通)設定
    ''' </summary>
    Private Sub EditDetailCommonArea(ByVal I_officeCode As String,
                                     ByVal I_rngDetailArea As Excel.Range,
                                     ByVal PrintDatarow As DataRow,
                                     ByVal I_column As Integer,
                                     ByRef O_OtOilName As String)

        '◯ 車数
        I_rngDetailArea = Me.ExcelWorkSheet.Range("B" + I_column.ToString())
        I_rngDetailArea.Value = PrintDatarow("LINECNT")
        ExcelMemoryRelease(I_rngDetailArea)
        '◯ 荷主名
        I_rngDetailArea = Me.ExcelWorkSheet.Range("C" + I_column.ToString())
        I_rngDetailArea.Value = PrintDatarow("SHIPPERSNAME")
        ExcelMemoryRelease(I_rngDetailArea)
        '◯ 在庫発駅(発駅)
        I_rngDetailArea = Me.ExcelWorkSheet.Range("D" + I_column.ToString())
        I_rngDetailArea.Value = PrintDatarow("DEPSTATIONNAME")
        ExcelMemoryRelease(I_rngDetailArea)
        '◯ 油種(OT油種)
        I_rngDetailArea = Me.ExcelWorkSheet.Range("E" + I_column.ToString())
        I_rngDetailArea.Value = PrintDatarow("OTOILNAME")
        ExcelMemoryRelease(I_rngDetailArea)
        '◯ 車(OT油種毎の件数)
        If O_OtOilName <> PrintDatarow("OTOILNAME").ToString() Then
            I_rngDetailArea = Me.ExcelWorkSheet.Range("F" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("OTOILCTCNT")
            ExcelMemoryRelease(I_rngDetailArea)
        End If
        O_OtOilName = PrintDatarow("OTOILNAME").ToString()

        '★袖ヶ浦営業所の場合
        If I_officeCode = BaseDllConst.CONST_OFFICECODE_011203 Then
            '◯ タンク車番号
            I_rngDetailArea = Me.ExcelWorkSheet.Range("G" + I_column.ToString())
            If Convert.ToString(PrintDatarow("MODEL")) = BaseDllConst.CONST_MODEL_1000 Then
                I_rngDetailArea.Value = "1-" + Convert.ToString(PrintDatarow("TANKNO"))
            Else
                I_rngDetailArea.Value = PrintDatarow("TANKNO")
            End If
            ExcelMemoryRelease(I_rngDetailArea)
            '◯ 前回油種(OT油種)
            I_rngDetailArea = Me.ExcelWorkSheet.Range("I" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("LASTOTOILNAME")
            ExcelMemoryRelease(I_rngDetailArea)
            '### 20201008 START 指摘票対応(No156)全体 ###################################################
            '◯ 順位
            I_rngDetailArea = Me.ExcelWorkSheet.Range("K" + I_column.ToString())
            'I_rngDetailArea = Me.ExcelWorkSheet.Range("I" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("SHIPORDER")
            ExcelMemoryRelease(I_rngDetailArea)
            '### 20201008 END   指摘票対応(No156)全体 ###################################################
            '◯ 次回交検日
            I_rngDetailArea = Me.ExcelWorkSheet.Range("L" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("JRINSPECTIONDATE")
            ExcelMemoryRelease(I_rngDetailArea)
            '◯ 返送日列車
            I_rngDetailArea = Me.ExcelWorkSheet.Range("M" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("RETURNDATETRAIN")
            ExcelMemoryRelease(I_rngDetailArea)
            '### 20200917 START 指摘票対応(No138)全体 ###################################################
            '◯ FOC入線順
            I_rngDetailArea = Me.ExcelWorkSheet.Range("N" + I_column.ToString())
            'I_rngDetailArea = Me.ExcelWorkSheet.Range("L" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("LINEORDER")
            ExcelMemoryRelease(I_rngDetailArea)
            '◯ 託送用コード
            I_rngDetailArea = Me.ExcelWorkSheet.Range("P" + I_column.ToString())
            'I_rngDetailArea = Me.ExcelWorkSheet.Range("M" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("DELIVERYCODE")
            ExcelMemoryRelease(I_rngDetailArea)
            '### 20200917 END   指摘票対応(No138)全体 ###################################################
            '### 20201008 START 指摘票対応(No157)全体 ###################################################
            '◯ 記事
            I_rngDetailArea = Me.ExcelWorkSheet.Range("R" + I_column.ToString())
            'I_rngDetailArea = Me.ExcelWorkSheet.Range("N" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("KUUKAICONSIGNEENAME")
            ExcelMemoryRelease(I_rngDetailArea)
            '### 20201008 END   指摘票対応(No157)全体 ###################################################
        Else
            '◯ タンク車番号
            I_rngDetailArea = Me.ExcelWorkSheet.Range("G" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("TANKNO")
            ExcelMemoryRelease(I_rngDetailArea)
            '◯ 前回油種
            I_rngDetailArea = Me.ExcelWorkSheet.Range("H" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("PREORDERINGOILNAME")
            ExcelMemoryRelease(I_rngDetailArea)
            '◯ 順位
            '### 未使用項目 ###########################################
            '◯ 次回交検日
            I_rngDetailArea = Me.ExcelWorkSheet.Range("J" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("JRINSPECTIONDATE")
            ExcelMemoryRelease(I_rngDetailArea)
            '◯ 返送日列車
            I_rngDetailArea = Me.ExcelWorkSheet.Range("K" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("RETURNDATETRAIN")
            ExcelMemoryRelease(I_rngDetailArea)
            '◯ ジョイント先
            I_rngDetailArea = Me.ExcelWorkSheet.Range("L" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("JOINT")
            ExcelMemoryRelease(I_rngDetailArea)
            '◯ 割当元
            '### 未使用項目 ###########################################
            '◯ 記事
            I_rngDetailArea = Me.ExcelWorkSheet.Range("N" + I_column.ToString())
            I_rngDetailArea.Value = PrintDatarow("REMARK")
            ExcelMemoryRelease(I_rngDetailArea)
        End If

    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(空回日報画面)
    ''' </summary>
    Private Sub EditHeaderArea(ByVal I_officeCode As String)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            Dim j As Integer = 0                            '次明細切り替え時用

            For Each PrintDatarow As DataRow In PrintData.Rows

                '○ 帳票のヘッダー(共通)設定
                EditHeaderCommonArea(I_officeCode, rngHeaderArea, PrintDatarow, j)

                Exit For
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(空回日報画面)
    ''' </summary>
    Private Sub EditDetailArea(ByVal I_officeCode As String)
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim i As Integer = 12
            Dim strOtOilNameSave As String = ""
            For Each PrintDatarow As DataRow In PrintData.Rows

                '○帳票の明細(共通)設定
                EditDetailCommonArea(I_officeCode, rngDetailArea, PrintDatarow, i, strOtOilNameSave)

                i += 1
            Next

            '◯ 合計
            rngDetailArea = Me.ExcelWorkSheet.Range("G41")
            rngDetailArea.Value = PrintData.Rows.Count.ToString() + "車"
            ExcelMemoryRelease(rngDetailArea)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

    ''' <summary>
    ''' 帳票のヘッダーと明細設定(受注一覧(帳票)画面)
    ''' </summary>
    Private Sub EditHeaderDetailArea(ByVal I_officeCode As String)
        Dim rngHeaderArea As Excel.Range = Nothing
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim j As Integer = 0                            '次明細切り替え時用
            Dim iTate() As Integer = {12, 54, 96, 138}      '明細の開始行
            Dim i As Integer = iTate(j)
            Dim iFooter() As Integer = {41, 83, 125, 167}   'フッター行(配列)
            Dim z As Integer = 0                            '明細の合計
            Dim strOtOilNameSave As String = ""
            Dim strTrainNoSave As String = ""
            For Each PrintDatarow As DataRow In PrintData.Rows

                If strTrainNoSave = "" Then
                    '○ 帳票のヘッダー(共通)設定(初回)
                    EditHeaderCommonArea(I_officeCode, rngHeaderArea, PrintDatarow, j)
                End If
                '★列車が変わった場合
                If strTrainNoSave <> "" AndAlso strTrainNoSave <> Convert.ToString(PrintDatarow("TRAINNO")) Then
                    '◯ 合計
                    rngDetailArea = Me.ExcelWorkSheet.Range("G" + Convert.ToString(iFooter(j)))
                    rngDetailArea.Value = Convert.ToString(z) + "車"
                    ExcelMemoryRelease(rngDetailArea)
                    '★次明細用として合計,油種(保存)を初期化
                    z = 0
                    strOtOilNameSave = ""

                    '★次明細の行設定
                    j += 1
                    i = iTate(j)

                    '○ 帳票のヘッダー(共通)設定(２列車目以降)
                    EditHeaderCommonArea(I_officeCode, rngHeaderArea, PrintDatarow, j)
                End If

                '○帳票の明細(共通)設定
                EditDetailCommonArea(I_officeCode, rngDetailArea, PrintDatarow, i, strOtOilNameSave)

                '○列車Noの保存
                strTrainNoSave = Convert.ToString(PrintDatarow("TRAINNO"))

                '○次の行へカウント
                i += 1
                z += 1
            Next

            '◯ 合計
            rngDetailArea = Me.ExcelWorkSheet.Range("G" + Convert.ToString(iFooter(j)))
            rngDetailArea.Value = Convert.ToString(z) + "車"
            ExcelMemoryRelease(rngDetailArea)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

    ''' <summary>
    ''' Excelオブジェクトの解放
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="objCom"></param>
    Private Sub ExcelMemoryRelease(Of T As Class)(ByRef objCom As T)

        'ランタイム実行対象がComObjectのアンマネージコードの場合、メモリ開放
        If objCom Is Nothing Then
            Return
        Else
            Try
                If Marshal.IsComObject(objCom) Then
                    Dim count As Integer = Marshal.FinalReleaseComObject(objCom)
                End If
            Finally
                objCom = Nothing
            End Try
        End If

    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 重複する呼び出しを検出するには

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: マネージド状態を破棄します (マネージド オブジェクト)。
            End If
            'Excel 作業シートオブジェクトの解放
            ExcelMemoryRelease(ExcelTempSheet)
            'Excel Sheetオブジェクトの解放
            ExcelMemoryRelease(ExcelWorkSheet)
            'Excel Sheetコレクションの解放
            ExcelMemoryRelease(ExcelWorkSheets)
            'Excel Bookオブジェクトを閉じる
            If ExcelBookObj IsNot Nothing Then
                Try
                    'ExcelBookObj.Close(Excel.XlSaveAction.xlDoNotSaveChanges)
                    ExcelBookObj.Close(False)
                Catch ex As Exception
                End Try
            End If

            ExcelMemoryRelease(ExcelBookObj)
            'Excel Bookコレクションの解放
            ExcelMemoryRelease(ExcelBooksObj)
            'Excel Appの終了
            If ExcelAppObj IsNot Nothing Then
                Try
                    ExcelAppObj.Quit()
                Catch ex As Exception
                End Try
            End If
            ExcelMemoryRelease(ExcelAppObj)

        End If
        ExcelProcEnd()
        disposedValue = True
    End Sub
    ''' <summary>
    ''' Excelプロセスの終了
    ''' </summary>
    Private Sub ExcelProcEnd()
        ExcelMemoryRelease(ExcelAppObj)
        Try
            '念のため当処理で起動したプロセスが残っていたらKill
            Dim xproc As Process = Process.GetProcessById(Me.xlProcId)
            System.Threading.Thread.Sleep(200) 'Waitかけないとプロセスが終了しきらない為
            If Not xproc.HasExited Then
                xproc.Kill()
            End If
        Catch ex As Exception
        End Try
    End Sub
    ' TODO: 上の Dispose(disposing As Boolean) にアンマネージド リソースを解放するコードが含まれる場合にのみ Finalize() をオーバーライドします。
    'Protected Overrides Sub Finalize()
    '    ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(disposing As Boolean) に記述します。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(disposing As Boolean) に記述します。
        Dispose(True)
        ' TODO: 上の Finalize() がオーバーライドされている場合は、次の行のコメントを解除してください。
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class

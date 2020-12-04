Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 貨車連結順序表(運用指示書)個別帳票作成クラス
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）</remarks>
Public Class OIT0002CustomReport : Implements IDisposable
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
            Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("ポラリス投入用"), Excel.Worksheet)
            'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData(ByVal I_OFFICENAME As String) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            '***** TODO処理 ここから *****
            '◯ヘッダーの設定
            EditHeaderArea(I_OFFICENAME)
            '◯明細の設定
            EditDetailArea()
            '***** TODO処理 ここまで *****
            'ExcelTempSheet.Delete() '雛形シート削除

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
    ''' 帳票のヘッダー設定
    ''' </summary>
    Private Sub EditHeaderArea(ByVal I_OFFICENAME As String)
        Dim rngTitleArea As Excel.Range = Nothing
        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 営業所名
                rngTitleArea = Me.ExcelWorkSheet.Range("A2")
                rngTitleArea.Value = I_OFFICENAME
                ExcelMemoryRelease(rngTitleArea)
                '◯ 列車No
                rngTitleArea = Me.ExcelWorkSheet.Range("E2")
                If PrintDatarow("CONVENTIONAL").ToString() = "" Then
                    rngTitleArea.Value = PrintDatarow("TRAINNO")
                Else
                    rngTitleArea.Value = PrintDatarow("CONVENTIONAL")
                End If
                ExcelMemoryRelease(rngTitleArea)
                '◯ 登録日
                rngTitleArea = Me.ExcelWorkSheet.Range("D4")
                rngTitleArea.Value = PrintDatarow("REGISTRATIONDATE")
                ExcelMemoryRelease(rngTitleArea)
                '◯ 現車
                rngTitleArea = Me.ExcelWorkSheet.Range("C39")
                rngTitleArea.Value = PrintDatarow("CURRENTCARTOTAL")
                ExcelMemoryRelease(rngTitleArea)
                '◯ 延長
                rngTitleArea = Me.ExcelWorkSheet.Range("E39")
                rngTitleArea.Value = PrintDatarow("EXTEND")
                ExcelMemoryRelease(rngTitleArea)
                '◯ 換算
                rngTitleArea = Me.ExcelWorkSheet.Range("H39")
                rngTitleArea.Value = PrintDatarow("CONVERSIONTOTAL")
                ExcelMemoryRelease(rngTitleArea)
                Exit For
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngTitleArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定
    ''' </summary>
    Private Sub EditDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim i As Integer = 9
            Dim strOtOilNameSave As String = ""
            For Each PrintDatarow As DataRow In PrintData.Rows

                If PrintDatarow("TRUCKSYMBOL").ToString() = "" Then Continue For

                '◯ 貨車(記号及び符号)
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TRUCKSYMBOL")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 貨車(番　号)
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TRUCKNO")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 発　駅
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("DEPSTATIONNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 着　駅
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ARRSTATIONNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 品名
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ARTICLENAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 換算数量
                rngDetailArea = Me.ExcelWorkSheet.Range("G" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONVERSIONAMOUNT")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 記事
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ARTICLE")
                ExcelMemoryRelease(rngDetailArea)
                'If PrintDatarow("ORDERTRKBN").ToString() = BaseDllConst.CONST_TRKBN_M _
                '    AndAlso PrintDatarow("OTTRANSPORTFLG").ToString() = "2" Then
                '    rngDetailArea.Value = PrintDatarow("ARTICLE").ToString() + "JOT"
                'End If
                '◯ 交検日
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                rngDetailArea.Value = PrintDatarow("INSPECTIONDATE")
                ExcelMemoryRelease(rngDetailArea)
                '### 20201021 START 指摘票対応(No189)全体 #############################################
                '◯ 前回油種
                rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
                rngDetailArea.Value = PrintDatarow("PREORDERINGOILNAME")
                ExcelMemoryRelease(rngDetailArea)
                '### 20201021 START 指摘票対応(No189)全体 #############################################

                '### 運　用　指　示 ###########################################
                '### 20201111 START 指摘票対応(No190)全体 #####################
                '◯ タンク車指示(指示内容)
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                rngDetailArea.Value = PrintDatarow("OBJECTIVENAME")
                ExcelMemoryRelease(rngDetailArea)
                '### 20201111 END   指摘票対応(No190)全体 #####################
                '◯ 充 填 線(油　種)
                rngDetailArea = Me.ExcelWorkSheet.Range("L" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("RINKAIOILKANA")
                rngDetailArea.Value = PrintDatarow("ORDERINGOILNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 充 填 線(回　転)
                rngDetailArea = Me.ExcelWorkSheet.Range("M" + i.ToString())
                rngDetailArea.Value = PrintDatarow("LINE")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 充 填 線(位　置)
                rngDetailArea = Me.ExcelWorkSheet.Range("N" + i.ToString())
                rngDetailArea.Value = PrintDatarow("FILLINGPOINT")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 入　線 　列　車(選択 OR 入力)
                rngDetailArea = Me.ExcelWorkSheet.Range("O" + i.ToString())
                If Convert.ToString(PrintDatarow("LOADINGIRILINETRAINNO")) <> "" Then
                    rngDetailArea.Value = PrintDatarow("LOADINGIRILINETRAINNO")
                End If
                ExcelMemoryRelease(rngDetailArea)
                '### 20201204 START 指摘票対応(No231)全体 #######################
                '◯ ＯＴ輸送(選択 OR 入力)
                rngDetailArea = Me.ExcelWorkSheet.Range("P" + i.ToString())
                If Convert.ToString(PrintDatarow("OTTRANSPORTFLG")) = "1" Then
                    rngDetailArea.Value = "OT輸送"
                End If
                ExcelMemoryRelease(rngDetailArea)
                '### 20201204 START 指摘票対応(No231)全体 #######################
                '◯ 着駅(本線列車)※自動設定のため未設定
                'rngDetailArea = Me.ExcelWorkSheet.Range("P" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("LOADINGARRSTATIONNAME")
                'ExcelMemoryRelease(rngDetailArea)

                '◯ ポラリス受注登録必須項目(本線列車)
                rngDetailArea = Me.ExcelWorkSheet.Range("S" + i.ToString())
                'rngDetailArea = Me.ExcelWorkSheet.Range("R" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ORDERTRAINNO")
                ExcelMemoryRelease(rngDetailArea)
                '◯ ポラリス受注登録必須項目(積込日)
                rngDetailArea = Me.ExcelWorkSheet.Range("T" + i.ToString())
                'rngDetailArea = Me.ExcelWorkSheet.Range("S" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ORDERLODDATE")
                ExcelMemoryRelease(rngDetailArea)
                '◯ ポラリス受注登録必須項目(発日)
                rngDetailArea = Me.ExcelWorkSheet.Range("U" + i.ToString())
                'rngDetailArea = Me.ExcelWorkSheet.Range("T" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ORDERDEPDATE")
                ExcelMemoryRelease(rngDetailArea)
                '### 20201111 START 指摘票対応(No190)全体 #####################
                '◯ ポラリス受注登録必須項目(回送(着駅))
                rngDetailArea = Me.ExcelWorkSheet.Range("V" + i.ToString())
                'rngDetailArea = Me.ExcelWorkSheet.Range("U" + i.ToString())
                rngDetailArea.Value = PrintDatarow("FORWARDINGARRSTATION")
                ExcelMemoryRelease(rngDetailArea)
                '◯ ポラリス受注登録必須項目(回送(その他))
                rngDetailArea = Me.ExcelWorkSheet.Range("W" + i.ToString())
                'rngDetailArea = Me.ExcelWorkSheet.Range("V" + i.ToString())
                rngDetailArea.Value = PrintDatarow("FORWARDINGCONFIGURE")
                '### 20201111 END   指摘票対応(No190)全体 #####################
                ExcelMemoryRelease(rngDetailArea)
                i += 1
            Next

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

            ' TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、下の Finalize() をオーバーライドします。
            ' TODO: 大きなフィールドを null に設定します。
        End If
        'Excel Sheetオブジェクトの解放
        ExcelMemoryRelease(ExcelWorkSheet)
        ExcelMemoryRelease(ExcelTempSheet)
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

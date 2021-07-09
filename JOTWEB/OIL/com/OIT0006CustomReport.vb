Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 回送個別帳票作成クラス
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）</remarks>
Public Class OIT0006CustomReport : Implements IDisposable
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

    Private CMNPTS As New CmnParts                                  '共通関数

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
            If excelFileName = "OIT0006L_TRANSPORT.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("貨物運送状(交検)"), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("貨物運送状(全検)"), Excel.Worksheet)
            End If
        Catch ex As Exception
            If Me.xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try

    End Sub

#Region "(帳票)三重塩浜営業所"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(三重塩浜(運送状))URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintMieShiohamaData(ByVal repPtn As String, ByVal depDate As String) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"

        '○帳票名取得
        Dim tmpGetFileName As String = CMNPTS.SetReportFileName(repPtn, BaseDllConst.CONST_OFFICECODE_012402, depDate, "")
        If tmpGetFileName <> "" Then
            tmpFileName = tmpGetFileName
        End If

        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Select Case repPtn
                '運送状(交検), 運送状(全検)
                Case "TRANSPORT_INSP",
                     "TRANSPORT_AINS"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditTransportHeaderArea(depDate, repPtn)
                    '◯明細の設定
                    EditTransportDetailArea(depDate, repPtn)
                    '◯フッターの設定
                    EditTransportFooterArea(depDate, repPtn)
                    '***** TODO処理 ここまで *****

                    '★不要シート削除
                    Select Case repPtn
                        '運送状(交検)
                        Case "TRANSPORT_INSP"
                            ExcelTempSheet.Delete() '「運送状(全検)」シート削除
                            ExcelMemoryRelease(ExcelTempSheet)
                        '運送状(全検)
                        Case "TRANSPORT_AINS"
                            ExcelWorkSheet.Delete() '「運送状(交検)」シート削除
                            ExcelMemoryRelease(ExcelWorkSheet)
                    End Select
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
    ''' 帳票のヘッダー設定(運送状(共通))
    ''' </summary>
    Private Sub EditTransportHeaderArea(ByVal depDate As String, ByVal repPtn As String)
        Dim rngHeaderArea As Excel.Range = Nothing
        'シートを選択
        Dim insExcelWorkSheet As Excel.Worksheet = Me.ExcelWorkSheet
        If repPtn = "TRANSPORT_AINS" Then insExcelWorkSheet = Me.ExcelTempSheet

        Try
            '◯ 申込日
            Dim sTodayYYYY As String = Now.ToString("yyyy", New Globalization.CultureInfo("ja-JP"))
            Dim sTodayMM As String = Now.ToString("MM", New Globalization.CultureInfo("ja-JP"))
            Dim sTodayDD As String = Now.ToString("dd", New Globalization.CultureInfo("ja-JP"))
            '　年
            rngHeaderArea = insExcelWorkSheet.Range("AE1")
            rngHeaderArea.Value = sTodayYYYY
            ExcelMemoryRelease(rngHeaderArea)
            '　月
            rngHeaderArea = insExcelWorkSheet.Range("AJ1")
            rngHeaderArea.Value = sTodayMM
            ExcelMemoryRelease(rngHeaderArea)
            '　日
            rngHeaderArea = insExcelWorkSheet.Range("AO1")
            rngHeaderArea.Value = sTodayDD
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 発送日
            Dim sDepDate As String = depDate
            '　年
            rngHeaderArea = insExcelWorkSheet.Range("AJ2")
            rngHeaderArea.Value = Date.Parse(sDepDate).ToString("yyyy", New Globalization.CultureInfo("ja-JP"))
            ExcelMemoryRelease(rngHeaderArea)
            '　月
            rngHeaderArea = insExcelWorkSheet.Range("AO2")
            rngHeaderArea.Value = Date.Parse(sDepDate).ToString("MM", New Globalization.CultureInfo("ja-JP"))
            ExcelMemoryRelease(rngHeaderArea)
            '　日
            rngHeaderArea = insExcelWorkSheet.Range("AT2")
            rngHeaderArea.Value = Date.Parse(sDepDate).ToString("dd", New Globalization.CultureInfo("ja-JP"))
            ExcelMemoryRelease(rngHeaderArea)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(運送状(共通))
    ''' </summary>
    Private Sub EditTransportDetailArea(ByVal depDate As String, ByVal repPtn As String)
        Dim rngDetailArea As Excel.Range = Nothing
        'シートを選択
        Dim insExcelWorkSheet As Excel.Worksheet = Me.ExcelWorkSheet
        If repPtn = "TRANSPORT_AINS" Then insExcelWorkSheet = Me.ExcelTempSheet

        Try
            Dim i As Integer = 13
            Dim iProductName As Integer() = {13, 16, 19, 22, 25}
            Dim iProductNum As Integer = iProductName(0)
            Dim aProductName As String() = {"H", "J", "L", "N"}             '品目コード(配置場所)
            Dim aJRTankType As String() = {"AN", "AP", "AR"}                '車種コード(配置場所)
            Dim aTankNo As String() = {"AT", "AV", "AX", "AZ", "BB", "BD"}  '貨車番号(配置場所)
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 品名・荷造, 品目コード, 運賃計算トン数
                If iProductNum = i Then
                    '◯ 品名・荷造
                    rngDetailArea = insExcelWorkSheet.Range("C" + i.ToString())
                    rngDetailArea.Value = "私有タンク車"
                    ExcelMemoryRelease(rngDetailArea)

                    Dim ItemCd As String = "3011"
                    '◯ 品目コード
                    For j = 0 To 3
                        rngDetailArea = insExcelWorkSheet.Range(aProductName(j) + i.ToString())
                        rngDetailArea.Value = ItemCd.Substring(j, 1)
                        ExcelMemoryRelease(rngDetailArea)
                    Next

                    '◯ 運賃計算トン数
                    rngDetailArea = insExcelWorkSheet.Range("V" + i.ToString())
                    rngDetailArea.Value = "4"
                    ExcelMemoryRelease(rngDetailArea)

                    iProductNum += 1
                End If

                '◯ 車種コード
                Dim JRTankType As String = Convert.ToString(PrintDatarow("JRTANKTYPE"))
                For j = 0 To 2
                    rngDetailArea = insExcelWorkSheet.Range(aJRTankType(j) + i.ToString())
                    'rngDetailArea = Me.ExcelWorkSheet.Range(aJRTankType(j) + i.ToString())
                    rngDetailArea.Value = JRTankType.Substring(j, 1)
                    ExcelMemoryRelease(rngDetailArea)
                Next j

                '◯ 貨車番号
                Dim TankNo As String = Convert.ToString(PrintDatarow("TANKNO")).PadLeft(6)
                For j = 0 To 5
                    rngDetailArea = insExcelWorkSheet.Range(aTankNo(j) + i.ToString())
                    'rngDetailArea = Me.ExcelWorkSheet.Range(aTankNo(j) + i.ToString())
                    rngDetailArea.Value = TankNo.Substring(j, 1)
                    ExcelMemoryRelease(rngDetailArea)
                Next j

                i += 1
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のフッター設定(運送状(共通))
    ''' </summary>
    Private Sub EditTransportFooterArea(ByVal depDate As String, ByVal repPtn As String)
        Dim rngFooterArea As Excel.Range = Nothing
        Dim insExcelWorkSheet As Excel.Worksheet = Me.ExcelWorkSheet
        If repPtn = "TRANSPORT_AINS" Then insExcelWorkSheet = Me.ExcelTempSheet

        Try
            '◯ 運賃計算トン数(計)
            rngFooterArea = insExcelWorkSheet.Range("V28")
            rngFooterArea.Value = PrintData.Rows.Count * 4
            ExcelMemoryRelease(rngFooterArea)

            '◯ 総車数
            rngFooterArea = insExcelWorkSheet.Range("AN29")
            rngFooterArea.Value = PrintData.Rows.Count
            ExcelMemoryRelease(rngFooterArea)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngFooterArea)
        End Try
    End Sub

#End Region

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

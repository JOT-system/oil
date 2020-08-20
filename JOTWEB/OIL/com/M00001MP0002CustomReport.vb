Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' メニュー画面の月間輸送量ペインダウンロード機能
''' </summary>
Public Class M00001MP0002CustomReport : Implements IDisposable
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

    '''' <summary>
    '''' 一時作業シート
    '''' </summary>
    'Private ExcelTempSheet As Excel.Worksheet
    ''' <summary>
    ''' 画面展開している内容と同等のデータテーブル
    ''' </summary>
    Private DispData As List(Of DataTable)
    ''' <summary>
    ''' 画面展開している選択した表示種別
    ''' </summary>
    Private ViewId As String = ""
    ''' <summary>
    ''' 画面展開している選択した表示種別文言
    ''' </summary>
    Private ViewIdName As String = ""
    ''' <summary>
    ''' 画面展開している選択した営業所名
    ''' </summary>
    Private OfficeName As String = ""
    ''' <summary>
    ''' ExcelプロセスID
    ''' </summary>
    Private xlProcId As Integer
    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintFilePath As String = ""
    ''' <summary>
    ''' WindowハンドルよりProcessIDを取得
    ''' </summary>
    ''' <param name="hwnd"></param>
    ''' <param name="lpdwProcessId"></param>
    ''' <returns></returns>
    ''' <remarks>ExcelのWindowハンドルを探しプロセスIDを取得
    ''' 当処理で使用したExcelのプロセスIDが残っていた場合KILLする為使用</remarks>
    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId"></param>
    ''' <param name="excelFileName"></param>
    ''' <param name="dispData"></param>
    ''' <param name="viewId"></param>
    Public Sub New(mapId As String, excelFileName As String, dispData As List(Of DataTable), viewId As String, viewName As String, officeName As String)
        Dim CS0050SESSION As New CS0050SESSION
        Me.DispData = dispData
        Me.ViewId = viewId
        Me.ViewIdName = viewName
        Me.OfficeName = officeName
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
        Me.UrlRoot = String.Format("{0}://{1}/PRINT/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID)
        'Excelアプリケーションオブジェクトの生成
        Me.ExcelAppObj = New Excel.Application
        ExcelAppObj.DisplayAlerts = False
        ExcelAppObj.ScreenUpdating = False

        Dim xlHwnd As IntPtr = CType(Me.ExcelAppObj.Hwnd, IntPtr)
        GetWindowThreadProcessId(xlHwnd, Me.xlProcId)
        'Excelワークブックオブジェクトの生成
        Me.ExcelBooksObj = Me.ExcelAppObj.Workbooks
        Me.ExcelBookObj = Me.ExcelBooksObj.Open(Me.ExcelTemplatePath,
                                                UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                                [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
        ExcelAppObj.Calculation = Excel.XlCalculation.xlCalculationManual
        Me.ExcelWorkSheets = Me.ExcelBookObj.Sheets
        Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("月間輸送量"), Excel.Worksheet)

    End Sub
    ''' <summary>
    ''' ExcelデータをPrintフォルダに格納しURLを作成
    ''' </summary>
    ''' <returns></returns>
    Public Function CreateExcelPrintData() As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Try
            'タイトル文言の設定
            CreateHeader()

            '保存処理実行
            ExcelAppObj.ScreenUpdating = True
            ExcelAppObj.Calculation = Excel.XlCalculation.xlCalculationAutomatic
            ExcelAppObj.Calculate()
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
            End SyncLock
            Me.ExcelBookObj.Close(False)
            Me.PrintFilePath = tmpFilePath
            Return UrlRoot & tmpFileName
        Catch ex As Exception
            Throw
        End Try

    End Function
    ''' <summary>
    ''' 作成＋ダウンロード処理実行
    ''' </summary>
    ''' <param name="currentPage"></param>
    Public Sub CreateExcelFileStream(currentPage As Page, Optional dlFileName As String = "")
        Dim url = CreateExcelPrintData()
        If Me.PrintFilePath = "" OrElse
            IO.File.Exists(Me.PrintFilePath) = False Then
            Return
        End If

        Dim fileName As String = IO.Path.GetFileName(Me.PrintFilePath)
        If dlFileName <> "" Then
            fileName = ""
        End If

        Dim fi = New IO.FileInfo(Me.PrintFilePath)
        Dim encodeFileName As String = HttpUtility.UrlEncode(fileName)
        encodeFileName = encodeFileName.Replace("+", "%20")
        With currentPage
            .Response.ContentType = "application/octet-stream"
            .Response.AddHeader("Content-Disposition", String.Format("attachment;filename*=utf-8''{0}", encodeFileName))
            .Response.AddHeader("Content-Length", fi.Length.ToString())
            .Response.AddHeader("Pragma", "no-cache")
            .Response.AddHeader("Cache-Control", "no-cache")
            .Response.WriteFile(Me.PrintFilePath)
            .Response.End()
        End With

    End Sub
    ''' <summary>
    ''' ヘッダー文言の設定
    ''' </summary>
    Private Sub CreateHeader()
        Dim rngWork As Excel.Range = Nothing

        Try
            rngWork = Me.ExcelWorkSheet.Range("B1")
            rngWork.Value = String.Format("{0:M月d日}時点 月間輸送数量", Now)
            ExcelMemoryRelease(rngWork)
            rngWork = Me.ExcelWorkSheet.Range("B2")
            If Me.ViewId.Equals("VIEW001") Then
                rngWork.Value = String.Format("{0} - {1}", Me.ViewIdName, Me.OfficeName)
            Else
                rngWork.Value = String.Format("{0}", Me.ViewIdName)
            End If
            ExcelMemoryRelease(rngWork)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngWork)
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
            ''Excel 作業シートオブジェクトの解放
            'ExcelMemoryRelease(ExcelTempSheet)
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
            ExcelProcEnd()
        End If
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

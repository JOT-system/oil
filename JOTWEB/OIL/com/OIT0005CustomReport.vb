Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices

''' <summary>
''' カスタムレポート作成Factory
''' </summary>
''' <remarks>
''' Usingを利用しなくてもいいようFactoryパターンを使用
''' </remarks>
Public Class OIT0005CustomReport

    ''' <summary>
    ''' 複数ファイルダウンロード用
    ''' </summary>
    ''' <param name="urlList"></param>
    ''' <returns></returns>
    Public Shared Function CreateUrlJson(ByVal urlList As List(Of String)) As String
        If urlList IsNot Nothing AndAlso urlList.Any() Then
            Return String.Format("[{0}]", String.Join(",", urlList.Select(Function(url) String.Format("{{""url"": ""{0}""}}", url)).ToArray()))
        End If
        Return ""
    End Function

    ''' <summary>
    ''' 交検一覧作成
    ''' </summary>
    ''' <param name="mapId"></param>
    ''' <param name="officeCodeDic"></param>
    ''' <param name="beginDate"></param>
    ''' <param name="endDate"></param>
    ''' <param name="printDataClass"></param>
    ''' <returns></returns>
    Public Shared Function CreateKoukenList(ByVal mapId As String, ByVal officeCodeDic As Dictionary(Of String, String), ByVal beginDate As Date, ByVal endDate As Date, ByVal printDataClass As DataTable) As String
        Dim url As String
        Using repCbj = New KoukenList(mapId, printDataClass)
            Try
                url = repCbj.CreatePrintData(officeCodeDic, beginDate, endDate)
            Catch ex As Exception
                Throw
            End Try
        End Using
        Return url
    End Function

End Class

''' <summary>
''' カスタムレポート作成ベースクラス（共通処理）
''' </summary>
Public MustInherit Class OIT0005CustomReportBase : Implements IDisposable

    ''' <summary>
    ''' エクセルアプリケーションオブジェクト
    ''' </summary>
    Protected ExcelAppObj As Excel.Application
    ''' <summary>
    ''' エクセルブックコレクション
    ''' </summary>
    Protected ExcelBooksObj As Excel.Workbooks
    ''' <summary>
    ''' エクセルブックオブジェクト
    ''' </summary>
    Protected ExcelBookObj As Excel.Workbook
    ''' <summary>
    ''' エクセルシートコレクション
    ''' </summary>
    Protected ExcelWorkSheets As Excel.Sheets
    ''' <summary>
    ''' エクセル作業シート
    ''' </summary>
    Protected ReadOnly Property ExcelWorkSheet As Excel.Worksheet
    ''' <summary>
    ''' エクセル一時作業シート
    ''' </summary>
    Protected ReadOnly Property ExcelTempSheet As Excel.Worksheet

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Protected ExcelTemplatePath As String = ""
    Protected UploadRootPath As String = ""
    Protected UrlRoot As String = ""

    ''' <summary>
    ''' エクセルアプリケーションのプロセスID
    ''' </summary>
    Protected xlProcId As Integer

    ''' <summary>
    ''' 出力対象のシート名
    ''' </summary>
    Protected OutputSheetNames As New List(Of String)

    ''' <summary>
    ''' プロセスID取得
    ''' </summary>
    ''' <param name="hwnd"></param>
    ''' <param name="lpdwProcessId"></param>
    ''' <returns></returns>
    Protected Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId"></param>
    ''' <param name="excelFileName"></param>
    Protected Sub New(mapId As String, excelFileName As String)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                          "PRINTFORMAT",
                                                          C_DEFAULT_DATAKEY,
                                                          mapId, excelFileName)
            UploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                       "PRINTWORK",
                                                       CS0050SESSION.USERID)
            'ディレクトリが存在しない場合は生成
            If IO.Directory.Exists(UploadRootPath) = False Then
                IO.Directory.CreateDirectory(UploadRootPath)
            End If
            '前日プリフィックスのアップロードファイルが残っていた場合は削除
            Dim targetFiles = IO.Directory.GetFiles(UploadRootPath, "*.*")
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
            UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

            'Excelアプリケーションオブジェクトの生成
            ExcelAppObj = New Excel.Application
            ExcelAppObj.DisplayAlerts = False
            ExcelAppObj.SheetsInNewWorkbook = 1
            Dim xlHwnd As IntPtr = CType(ExcelAppObj.Hwnd, IntPtr)
            GetWindowThreadProcessId(xlHwnd, xlProcId)

            ExcelBooksObj = ExcelAppObj.Workbooks

            'Excelワークブックオブジェクトの生成
            ExcelBookObj = ExcelBooksObj.Open(ExcelTemplatePath,
                                                UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                                [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
            ExcelWorkSheets = ExcelBookObj.Worksheets

        Catch ex As Exception
            If xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Excel作業シート設定
    ''' </summary>
    ''' <param name="sheetName"></param>
    Protected Function TrySetExcelWorkSheet(ByVal sheetName As String, Optional ByVal templateSheetName As String = Nothing) As Boolean
        Dim result As Boolean = False
        Try
            ExcelMemoryRelease(_ExcelWorkSheet)
            ExcelMemoryRelease(_ExcelTempSheet)
            Dim allSeetName As New Dictionary(Of String, Integer)
            For Each sheet As Excel.Worksheet In ExcelWorkSheets
                allSeetName.Add(sheet.Name, sheet.Index)
                ExcelMemoryRelease(sheet)
            Next
            If Not String.IsNullOrWhiteSpace(templateSheetName) AndAlso allSeetName.ContainsKey(templateSheetName) Then
                _ExcelWorkSheet = DirectCast(ExcelWorkSheets(allSeetName.Item(templateSheetName)), Excel.Worksheet)
                _ExcelTempSheet = DirectCast(ExcelWorkSheets(allSeetName.Last.Value), Excel.Worksheet)
                _ExcelWorkSheet.Copy(After:=_ExcelTempSheet)
                ExcelMemoryRelease(_ExcelWorkSheet)
                ExcelMemoryRelease(_ExcelTempSheet)
                _ExcelWorkSheet = DirectCast(ExcelWorkSheets(ExcelWorkSheets.Count), Excel.Worksheet)

                Dim newSheetName As String = sheetName
                Dim sheetCount As Integer = 1
                While allSeetName.ContainsKey(newSheetName)
                    sheetCount += 1
                    newSheetName = String.Format("{0} ({1})", sheetName, sheetCount)
                End While
                _ExcelWorkSheet.Name = newSheetName

                result = True

            ElseIf Not String.IsNullOrWhiteSpace(sheetName) AndAlso allSeetName.ContainsKey(sheetName) Then
                _ExcelWorkSheet = DirectCast(ExcelWorkSheets(allSeetName.Item(sheetName)), Excel.Worksheet)
                result = True
            End If
        Catch ex As Exception
            Throw
        Finally
            If Not result Then
                ExcelMemoryRelease(_ExcelWorkSheet)
            End If
            ExcelMemoryRelease(_ExcelTempSheet)
        End Try
        Return result
    End Function

    ''' <summary>
    ''' 出力シートのみ残す
    ''' </summary>
    ''' <param name="isReverse">シート順反転</param>
    Protected Sub LeaveOnlyOutputSheets(Optional ByVal isReverse As Boolean = False)
        Try

            '○出力シートのみ残す
            If OutputSheetNames IsNot Nothing AndAlso OutputSheetNames.Any() Then
                ExcelMemoryRelease(ExcelWorkSheet)
                ExcelMemoryRelease(ExcelTempSheet)
                Dim allSeetName As New Dictionary(Of String, Integer)
                For Each sheet As Excel.Worksheet In ExcelWorkSheets
                    allSeetName.Add(sheet.Name, sheet.Index)
                    ExcelMemoryRelease(sheet)
                Next
                For Each sheetName As String In allSeetName.
                    Where(Function(x) Not OutputSheetNames.Contains(x.Key)).
                    OrderBy(Function(x) x.Value).
                    Select(Function(x) x.Key).ToList()

                    If TrySetExcelWorkSheet(sheetName) Then
                        ExcelWorkSheet.Delete()
                    End If
                Next

                '○シート順反転
                If isReverse Then
                    allSeetName.Clear()
                    For Each sheet As Excel.Worksheet In ExcelWorkSheets
                        allSeetName.Add(sheet.Name, sheet.Index)
                        ExcelMemoryRelease(sheet)
                    Next
                    For i As Integer = allSeetName.Count() To 2 Step -1
                        Dim moveSheet As Excel.Worksheet = DirectCast(ExcelWorkSheets(i), Excel.Worksheet)
                        Dim beforSheet As Excel.Worksheet = DirectCast(ExcelWorkSheets(1), Excel.Worksheet)
                        moveSheet.Move(Before:=beforSheet)
                        ExcelMemoryRelease(moveSheet)
                        ExcelMemoryRelease(beforSheet)
                    Next
                End If
            End If

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try
    End Sub

    ''' <summary>
    ''' Excel保存処理
    ''' </summary>
    ''' <param name="filePath"></param>
    ''' <param name="uploadFilePath"></param>
    Protected Sub ExcelSaveAs(filePath As String, Optional uploadFilePath As String = Nothing)
        Try

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                If UCase(Right(filePath, 3)) = "XLS" Then
                    ExcelBookObj.SaveAs(filePath, Excel.XlFileFormat.xlExcel8)
                Else
                    ExcelBookObj.SaveAs(filePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
                End If
            End SyncLock

            '★別名が設定されている場合
            If Not String.IsNullOrEmpty(uploadFilePath) AndAlso filePath <> uploadFilePath Then
                '作成したファイルを指定パスに配置する。
                System.IO.File.Copy(filePath, uploadFilePath)
            End If

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try
    End Sub

    ''' <summary>
    ''' Excelオブジェクトの解放
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="objCom"></param>
    Protected Sub ExcelMemoryRelease(Of T As Class)(ByRef objCom As T)

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
        ExcelProcEnd()
        disposedValue = True
    End Sub
    ''' <summary>
    ''' Excelプロセスの終了
    ''' </summary>
    Protected Sub ExcelProcEnd()
        ExcelMemoryRelease(ExcelAppObj)
        Try
            'プロセスの状態を確認
            '（待機時間が短すぎるとプロセス終了されているか判断できないためある程度確保）
            Dim xproc As Process = Process.GetProcessById(xlProcId)
            For index = 1 To 50
                If Not xproc.HasExited Then
                    xproc.Refresh()
                    System.Threading.Thread.Sleep(200)
                Else
                    Exit For
                End If
            Next

            '念のため当処理で起動したプロセスが残っていたらKill
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

''' <summary>
''' 交検一覧
''' </summary>
Public Class KoukenList : Inherits OIT0005CustomReportBase

    ''' <summary>
    ''' テンプレートファイル名称
    ''' </summary>
    Private Const TEMP_XLS_FILE_NAME As String = "KOUKENLIST.xlsx"

    ''' <summary>
    ''' 明細ヘッダー定義
    '''     TEMPLATE_A：タンク車番号
    '''     TEMPLATE_B：全検チェック、タンク車番号、前回油種名称
    ''' </summary>
    Private Const A_DETAIL_HEADER_COLS As Integer = 1
    Private Const B_DETAIL_HEADER_COLS As Integer = 3

    ''' <summary>
    ''' 明細データ数
    ''' </summary>
    Private Const A_DETAIL_DATA_COUNT As Integer = 7
    Private Const B_DETAIL_DATA_COUNT As Integer = 20

    ''' <summary>
    ''' 折り返し日数
    ''' </summary>
    Private Const A_WRAPPING_DAYS As Integer = 16
    Private Const B_WRAPPING_DAYS As Integer = 11

    ''' <summary>
    ''' 明細データ構造
    ''' </summary>
    Private Class DetailItem
        Public OfficeCode As String
        Public TargetDate As Date
        Public TankNumber As String
        Public PreorderingOilName As String
        Public JRAllInspectionDate As Date
    End Class

    Protected PrintData As DataTable

    Public Sub New(ByVal mapId As String, ByVal printDataClass As DataTable)
        MyBase.New(mapId, TEMP_XLS_FILE_NAME)
        Me.PrintData = printDataClass
    End Sub

    Public Function CreatePrintData(ByVal officeCodeDic As Dictionary(Of String, String), ByVal beginDate As Date, ByVal endDate As Date) As String

        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)

        Try

            For Each officeCodePair As KeyValuePair(Of String, String) In officeCodeDic

                '○作業シート設定
                Select Case officeCodePair.Key
                    Case BaseDllConst.CONST_OFFICECODE_011402
                        TrySetExcelWorkSheet(String.Format("交検一覧表({0})", officeCodePair.Value), "TEMPLATE_B")
                    Case Else
                        TrySetExcelWorkSheet(String.Format("交検一覧表({0})", officeCodePair.Value), "TEMPLATE_A")
                End Select

                '○出力シート設定
                If ExcelWorkSheet IsNot Nothing AndAlso OutputSheetNames IsNot Nothing AndAlso Not OutputSheetNames.Contains(ExcelWorkSheet.Name) Then
                    OutputSheetNames.Add(ExcelWorkSheet.Name)
                End If

                Select Case officeCodePair.Key
                    Case BaseDllConst.CONST_OFFICECODE_011402
                        '○ヘッダーの設定
                        EditHeaderAreaB(officeCodePair.Value, beginDate, endDate)
                        '○明細の設定
                        EditDetailAreaB(officeCodePair.Key, beginDate, endDate)
                    Case Else
                        '○ヘッダーの設定
                        EditHeaderAreaA(officeCodePair.Value, beginDate, endDate)
                        '○明細の設定
                        EditDetailAreaA(officeCodePair.Key, beginDate, endDate)
                End Select

            Next

            '○出力シートのみ残す
            LeaveOnlyOutputSheets()

            '保存処理実行
            ExcelSaveAs(tmpFilePath)
            ExcelBookObj.Close(False)

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try

    End Function

#Region "TEMPLATE_A"

    ''' <summary>
    ''' ヘッダー部の設定（TEMPLATE_A）
    ''' </summary>
    Private Sub EditHeaderAreaA(ByVal officeName As String, ByVal beginDate As Date, ByVal endDate As Date)

        Dim rngHeaderArea As Excel.Range = Nothing

        Try

            'タイトル(営業所名)
            rngHeaderArea = ExcelWorkSheet.Range("A1")
            rngHeaderArea.Value = String.Format("交検一覧表({0})", officeName)
            ExcelMemoryRelease(rngHeaderArea)

            '出力月（出力開始日）
            rngHeaderArea = ExcelWorkSheet.Range("A2")
            rngHeaderArea.Value = beginDate.ToShortDateString()
            ExcelMemoryRelease(rngHeaderArea)

            '出力開始日
            rngHeaderArea = ExcelWorkSheet.Range("C2")
            rngHeaderArea.Value = beginDate.ToShortDateString()
            ExcelMemoryRelease(rngHeaderArea)

            '出力終了日
            rngHeaderArea = ExcelWorkSheet.Range("F2")
            rngHeaderArea.Value = endDate.ToShortDateString()
            ExcelMemoryRelease(rngHeaderArea)

            Dim nBeginDate As Date = New Date(beginDate.Year, beginDate.AddMonths(1).Month, 1)
            Dim nEndDate As Date = nBeginDate.AddMonths(1).AddDays(-1)

            '出力月（出力開始日）
            rngHeaderArea = ExcelWorkSheet.Range("A20")
            rngHeaderArea.Value = nBeginDate.ToShortDateString()
            ExcelMemoryRelease(rngHeaderArea)

            '出力開始日
            rngHeaderArea = ExcelWorkSheet.Range("C20")
            rngHeaderArea.Value = nBeginDate.ToShortDateString()
            ExcelMemoryRelease(rngHeaderArea)

            '出力終了日
            rngHeaderArea = ExcelWorkSheet.Range("F20")
            rngHeaderArea.Value = nEndDate.ToShortDateString()
            ExcelMemoryRelease(rngHeaderArea)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try

    End Sub

    ''' <summary>
    ''' 明細部分の編集（TEMPLATE_A）
    ''' </summary>
    Private Sub EditDetailAreaA(ByVal officeCode As String, ByVal beginDate As Date, ByVal endDate As Date)
        Try

            Dim baseDate As Date = Nothing
            Dim printRows As List(Of DetailItem) = PrintData.AsEnumerable.
                Select(Function(r) New DetailItem With {
                       .OfficeCode = r("OFFICECODE").ToString(),
                       .TargetDate = CDate(r("JRINSPECTIONDATE").ToString()),
                       .TankNumber = r("TANKNUMBER").ToString()
                       }).
                Where(Function(r) r.OfficeCode = officeCode).ToList()

            '当月
            baseDate = New Date(beginDate.Year, beginDate.Month, 1)
            SetDetailDataA("A3", baseDate, printRows)
            baseDate = baseDate.AddDays(A_WRAPPING_DAYS)
            SetDetailDataA("A11", baseDate, printRows)

            '次月
            baseDate = New Date(beginDate.Year, beginDate.AddMonths(1).Month, 1)
            SetDetailDataA("A21", baseDate, printRows)
            baseDate = baseDate.AddDays(A_WRAPPING_DAYS)
            SetDetailDataA("A29", baseDate, printRows)

        Catch ex As Exception
            Throw
        End Try

    End Sub

    ''' <summary>
    '''  明細データ設定（TEMPLATE_A）
    ''' </summary>
    ''' <param name="basePoint"></param>
    ''' <param name="beginDate"></param>
    ''' <param name="printRows"></param>
    Private Sub SetDetailDataA(ByVal basePoint As String, ByVal beginDate As Date, ByVal printRows As List(Of DetailItem))
        Dim rngDateAreaBase As Excel.Range = Nothing
        Dim rngDetailAreaBase As Excel.Range = Nothing
        Dim rngWorkArea As Excel.Range = Nothing
        Try
            '基本位置
            rngDateAreaBase = ExcelWorkSheet.Range(basePoint)
            rngDetailAreaBase = rngDateAreaBase.Offset(RowOffset:=1)
            For dayOffset As Integer = 0 To A_WRAPPING_DAYS - 1

                Dim thisDate As Date = beginDate.AddDays(dayOffset)
                If beginDate.Month <> thisDate.Month Then Continue For

                rngWorkArea = rngDateAreaBase.Offset(ColumnOffset:=dayOffset)
                rngWorkArea.Value = thisDate.ToShortDateString()
                ExcelMemoryRelease(rngWorkArea)

                Dim query = printRows.Where(Function(r) r.TargetDate.ToShortDateString() = thisDate.ToShortDateString())
                If query.Any() Then
                    For Each item In query.Take(A_DETAIL_DATA_COUNT).Select(Function(r, idx) New With {.row = r, .rowOffset = idx})
                        'タンク車番号
                        rngWorkArea = rngDetailAreaBase.Offset(RowOffset:=item.rowOffset, ColumnOffset:=dayOffset)
                        rngWorkArea.Value = item.row.TankNumber
                        ExcelMemoryRelease(rngWorkArea)
                    Next
                End If
            Next
            ExcelMemoryRelease(rngDetailAreaBase)
            ExcelMemoryRelease(rngDateAreaBase)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngWorkArea)
            ExcelMemoryRelease(rngDetailAreaBase)
            ExcelMemoryRelease(rngDateAreaBase)
        End Try
    End Sub

#End Region

#Region "TEMPLATE_B"

    ''' <summary>
    ''' ヘッダー部の設定（TEMPLATE_B）
    ''' </summary>
    Private Sub EditHeaderAreaB(ByVal officeName As String, ByVal beginDate As Date, ByVal endDate As Date)

        Dim rngHeaderArea As Excel.Range = Nothing

        Try

            'タイトル(営業所名)
            rngHeaderArea = ExcelWorkSheet.Range("A1")
            rngHeaderArea.Value = String.Format("交検一覧表({0})", officeName)
            ExcelMemoryRelease(rngHeaderArea)

            '出力月（出力開始日）
            rngHeaderArea = ExcelWorkSheet.Range("B2")
            rngHeaderArea.Value = beginDate.ToShortDateString()
            ExcelMemoryRelease(rngHeaderArea)

            '出力開始日
            rngHeaderArea = ExcelWorkSheet.Range("D2")
            rngHeaderArea.Value = beginDate.ToShortDateString()
            ExcelMemoryRelease(rngHeaderArea)

            '出力終了日
            rngHeaderArea = ExcelWorkSheet.Range("G2")
            rngHeaderArea.Value = endDate.ToShortDateString()
            ExcelMemoryRelease(rngHeaderArea)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try

    End Sub

    ''' <summary>
    ''' 明細部分の編集（TEMPLATE_B）
    ''' </summary>
    Private Sub EditDetailAreaB(ByVal officeCode As String, ByVal beginDate As Date, ByVal endDate As Date)
        Try

            Dim baseDate As Date = Nothing
            Dim printRows As List(Of DetailItem) = PrintData.AsEnumerable.
                Select(Function(r) New DetailItem With {
                       .OfficeCode = r("OFFICECODE").ToString(),
                       .TargetDate = CDate(r("JRINSPECTIONDATE").ToString()),
                       .TankNumber = r("TANKNUMBER").ToString(),
                       .PreorderingOilName = r("PREORDERINGOILNAME").ToString(),
                       .JRAllInspectionDate = CDate(r("JRALLINSPECTIONDATE").ToString())
                       }).
                Where(Function(r) r.OfficeCode = officeCode).ToList()

            '当月
            baseDate = New Date(beginDate.Year, beginDate.Month, 1)
            SetDetailDataB("B3", baseDate, printRows)
            baseDate = baseDate.AddDays(B_WRAPPING_DAYS)
            SetDetailDataB("B25", baseDate, printRows)
            baseDate = baseDate.AddDays(B_WRAPPING_DAYS)
            SetDetailDataB("B47", baseDate, printRows)

        Catch ex As Exception
            Throw
        End Try

    End Sub

    ''' <summary>
    '''  明細データ設定（TEMPLATE_B）
    ''' </summary>
    ''' <param name="basePoint"></param>
    ''' <param name="beginDate"></param>
    ''' <param name="printRows"></param>
    Private Sub SetDetailDataB(ByVal basePoint As String, ByVal beginDate As Date, ByVal printRows As List(Of DetailItem))
        Dim rngDateAreaBase As Excel.Range = Nothing
        Dim rngDetailAreaBase As Excel.Range = Nothing
        Dim rngWorkArea As Excel.Range = Nothing
        Try
            '基本位置
            rngDateAreaBase = ExcelWorkSheet.Range(basePoint)
            rngDetailAreaBase = rngDateAreaBase.Offset(RowOffset:=1)
            For dayOffset As Integer = 0 To B_WRAPPING_DAYS - 1

                Dim colOffset As Integer = dayOffset * B_DETAIL_HEADER_COLS
                Dim thisDate As Date = beginDate.AddDays(dayOffset)
                If beginDate.Month <> thisDate.Month Then Continue For

                rngWorkArea = rngDateAreaBase.Offset(ColumnOffset:=IIf(dayOffset > 1, colOffset - B_DETAIL_HEADER_COLS + 1, dayOffset))
                rngWorkArea.Value = thisDate.ToShortDateString()
                ExcelMemoryRelease(rngWorkArea)

                Dim query = printRows.Where(Function(r) r.TargetDate.ToShortDateString() = thisDate.ToShortDateString())
                If query.Any() Then
                    For Each item In query.Take(B_DETAIL_DATA_COUNT).Select(Function(r, idx) New With {.row = r, .rowOffset = idx})
                        '全検チェック
                        rngWorkArea = rngDetailAreaBase.Offset(RowOffset:=item.rowOffset, ColumnOffset:=colOffset)
                        rngWorkArea.Value = IIf((item.row.JRAllInspectionDate - thisDate).Days <= 30, "☆", "")
                        ExcelMemoryRelease(rngWorkArea)
                        'タンク車番号
                        rngWorkArea = rngDetailAreaBase.Offset(RowOffset:=item.rowOffset, ColumnOffset:=colOffset + 1)
                        rngWorkArea.Value = item.row.TankNumber
                        ExcelMemoryRelease(rngWorkArea)
                        '前回油種名称
                        rngWorkArea = rngDetailAreaBase.Offset(RowOffset:=item.rowOffset, ColumnOffset:=colOffset + 2)
                        rngWorkArea.Value = item.row.PreorderingOilName
                        ExcelMemoryRelease(rngWorkArea)
                    Next
                End If
            Next
            ExcelMemoryRelease(rngDetailAreaBase)
            ExcelMemoryRelease(rngDateAreaBase)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngWorkArea)
            ExcelMemoryRelease(rngDetailAreaBase)
            ExcelMemoryRelease(rngDateAreaBase)
        End Try
    End Sub
#End Region

End Class

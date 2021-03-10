Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices

''' <summary>
''' カスタムレポート作成Factory
''' </summary>
''' <remarks>
''' Usingを利用しなくてもいいようFactoryパターンを使用
''' </remarks>
Public Class OIT0003CustomMultiReport

    Public Shared Function CreateUrlJson(urlList As List(Of String)) As String
        If urlList IsNot Nothing AndAlso urlList.Any() Then
            Return String.Format("[{0}]", String.Join(",", urlList.Select(Function(url) String.Format("{{""url"": ""{0}""}}", url)).ToArray()))
        End If
        Return ""
    End Function

    Public Shared Function CreateActualShip(mapId As String, officeCode As String, printDataClass As DataTable, ByVal lodDate As String, ByVal trainNo As String) As String
        Dim url As String
        Using repCbj = New ActualShip(mapId, officeCode, printDataClass)
            Try
                url = repCbj.CreatePrintData(lodDate, trainNo)
            Catch ex As Exception
                Throw
            End Try
        End Using
        Return url
    End Function

    Public Shared Function CreateTankDispatch(mapId As String, officeCode As String, printDataClass As DataTable, ByVal lodDate As String, ByVal consigneeCode As String, ByVal trainNo As String) As String
        Dim url As String
        Using repCbj = New TankDispatch(mapId, officeCode, printDataClass)
            Try
                url = repCbj.CreatePrintData(lodDate, {trainNo}, consigneeCode)
            Catch ex As Exception
                Throw
            End Try
        End Using
        Return url
    End Function

    Public Shared Function CreateTankDispatch(mapId As String, officeCode As String, printDataClass As DataTable, ByVal lodDate As String, ByVal consigneeCode As String, ByVal trainNo As String()) As String
        Dim url As String
        Using repCbj = New TankDispatch(mapId, officeCode, printDataClass)
            Try
                url = repCbj.CreatePrintData(lodDate, trainNo, consigneeCode)
            Catch ex As Exception
                Throw
            End Try
        End Using
        Return url
    End Function

    Public Shared Function CreateContactOrder(mapId As String, officeCode As String, printDataClass As DataTable, ByVal lodDate As String, ByVal trainNo As String) As String
        Dim url As String
        Using repCbj = New ContactOrder(mapId, officeCode, printDataClass)
            Try
                url = repCbj.CreatePrintData(lodDate, trainNo)
            Catch ex As Exception
                Throw
            End Try
        End Using
        Return url
    End Function

    Public Shared Function CreateOrderDetail(mapId As String, printDataClass As DataTable) As String
        Dim url As String
        Using repCbj = New OrderDetail(mapId, printDataClass)
            Try
                url = repCbj.CreatePrintData()
            Catch ex As Exception
                Throw
            End Try
        End Using
        Return url
    End Function

End Class

''' <summary>
''' カスタムレポート作成ベースクラス
''' </summary>
Public MustInherit Class OIT0003CustomMultiReportBase : Implements IDisposable

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

    Protected Sub ExcelSaveAs(filePath As String)
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
''' タンク車発送フォーマット
''' </summary>
Public Class ActualShip : Inherits OIT0003CustomMultiReportBase

    Private Const TEMP_XLS_FILE_NAME As String = "ACTUALSHIP.xls"
    Private Const DETAIL_AREA_BEGIN_ROW_INDEX As Integer = 9
    Private Const DETAIL_AREA_ROWS_COUNT As Integer = 20

    Protected OfficeCode As String
    Protected PrintData As DataTable

    Public Sub New(mapId As String, ByVal officeCode As String, printDataClass As DataTable)
        MyBase.New(mapId, TEMP_XLS_FILE_NAME)
        Me.OfficeCode = officeCode
        Me.PrintData = printDataClass

        '○作業シート設定
        TrySetExcelWorkSheet("出荷実績表", "TEMPLATE")
    End Sub

    ''' <summary>
    ''' 帳票作成処理
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreatePrintData(ByVal lodDate As String, ByVal trainNo As String) As String

        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xls"
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)

        Try

            Dim rowIndex As Integer = 0
            Dim maxRowIndex As Integer = CInt(IIf(PrintData Is Nothing, 0, PrintData.Rows.Count))
            Do
                If rowIndex > 0 Then
                    '○作業シート設定
                    TrySetExcelWorkSheet("出荷実績表", "TEMPLATE")
                End If

                '○出力シート設定
                If ExcelWorkSheet IsNot Nothing AndAlso OutputSheetNames IsNot Nothing AndAlso Not OutputSheetNames.Contains(ExcelWorkSheet.Name) Then
                    OutputSheetNames.Add(ExcelWorkSheet.Name)
                End If

                '◯ヘッダーの設定
                EditHeaderArea(lodDate, trainNo)

                '◯明細の設定
                EditDetailArea(rowIndex)

                rowIndex += DETAIL_AREA_ROWS_COUNT
            Loop While rowIndex < maxRowIndex

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
                    OrderByDescending(Function(x) x.Value).
                    Select(Function(x) x.Key).ToList()

                    If TrySetExcelWorkSheet(sheetName) Then
                        ExcelWorkSheet.Delete()
                    End If
                Next
            End If

            '保存処理実行
            ExcelSaveAs(tmpFilePath)
            ExcelBookObj.Close(False)

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try
    End Function

    ''' <summary>
    ''' ヘッダー部の設定
    ''' </summary>
    Private Sub EditHeaderArea(ByVal lodDate As String, ByVal trainNo As String)

        Dim rngHeaderArea As Excel.Range = Nothing

        Try

            '列車番号
            rngHeaderArea = ExcelWorkSheet.Range("G31")
            If trainNo = "5282" Then
                trainNo = "5875"
            ElseIf trainNo = "8072" Then
                trainNo = "8081"
            End If
            rngHeaderArea.Value = String.Format("{0}列車", trainNo)
            ExcelMemoryRelease(rngHeaderArea)

            '出荷日(積込日)
            rngHeaderArea = ExcelWorkSheet.Range("C3")
            rngHeaderArea.Value = String.Format("{0}月", CDate(lodDate).Month)
            ExcelMemoryRelease(rngHeaderArea)
            rngHeaderArea = ExcelWorkSheet.Range("D3")
            rngHeaderArea.Value = String.Format("{0}日", CDate(lodDate).Day)
            ExcelMemoryRelease(rngHeaderArea)

            '出荷基地名
            rngHeaderArea = ExcelWorkSheet.Range("C4")
            Select Case OfficeCode
                Case CONST_OFFICECODE_011203
                    rngHeaderArea.Value = "富士石油"
                Case CONST_OFFICECODE_012402
                    rngHeaderArea.Value = "昭和四日市石油"
            End Select
            ExcelMemoryRelease(rngHeaderArea)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try

    End Sub

    ''' <summary>
    ''' 明細部分の編集
    ''' </summary>
    Private Sub EditDetailArea(ByVal rowIndex As Integer)
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim printRows = PrintData.AsEnumerable.
                Skip(rowIndex).
                Take(DETAIL_AREA_ROWS_COUNT).
                Select(Function(r, i) New With {.row = r, .index = i + DETAIL_AREA_BEGIN_ROW_INDEX}).ToList()

            For Each r In printRows
                '油種名
                rngDetailArea = ExcelWorkSheet.Range("B" + r.index.ToString())
                Dim oilCode As String = r.row("OILCODE").ToString()
                Dim orderingType As String = r.row("ORDERINGTYPE").ToString()

                Select Case oilCode
                    Case "1001"
                        rngDetailArea.Value = "ﾌﾟﾚﾐｱﾑ"
                    Case "1101"
                        rngDetailArea.Value = "ﾚｷﾞｭﾗｰG"
                    Case "1301"
                        rngDetailArea.Value = "ﾄｳﾕ"
                    Case "1401"
                        rngDetailArea.Value = "ｹｲﾕ"
                    Case "1404"
                        Select Case orderingType
                            Case "A"
                                rngDetailArea.Value = "3ｺﾞｳｹｲﾕ"
                            Case "E"
                                rngDetailArea.Value = "ｶﾝﾚｲｹｲﾕ"
                        End Select
                    Case "2101"
                        Select Case orderingType
                            Case "B"
                                rngDetailArea.Value = "0.5AFO"
                            Case "C"
                                rngDetailArea.Value = "ｶﾝﾚｲAFO"
                        End Select
                    Case "2201"
                        rngDetailArea.Value = "0.1AFO"
                End Select
                ExcelMemoryRelease(rngDetailArea)

                '積載実数量
                rngDetailArea = ExcelWorkSheet.Range("C" + r.index.ToString())
                rngDetailArea.Value = CDec(r.row("CARSAMOUNT")).ToString("#.##0")
                ExcelMemoryRelease(rngDetailArea)

                'ﾀﾝｸ車番号
                rngDetailArea = ExcelWorkSheet.Range("D" + r.index.ToString())
                rngDetailArea.Value = r.row("TANKNO")
                ExcelMemoryRelease(rngDetailArea)
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

End Class

''' <summary>
''' タンク車発送実績
''' </summary>
Public Class TankDispatch : Inherits OIT0003CustomMultiReportBase

    Private Const TEMP_XLS_FILE_NAME As String = "TANKDISPATCH.xlsx"
    Private Const DETAIL_AREA_BEGIN_ROW_INDEX As Integer = 9
    Private Const DETAIL_AREA_ROWS_COUNT As Integer = 20

    Protected OfficeCode As String
    Protected PrintData As DataTable


    Public Class CONSIGNEECODE
        Public Const KOUSYOUTAKASAKI As String = "30"
        Public Const JONETMATSUMOTO As String = "40"
        Public Const OTMORIOKA As String = "51"
        Public Const OTTAKASAKI As String = "54"
    End Class


    Public Sub New(mapId As String, ByVal officeCode As String, printDataClass As DataTable)
        MyBase.New(mapId, TEMP_XLS_FILE_NAME)
        Me.OfficeCode = officeCode
        Me.PrintData = printDataClass

        '○作業シート設定
        TrySetExcelWorkSheet("出荷実績表", "TEMPLATE")
    End Sub

    Public Function CreatePrintData(ByVal lodDate As String, ByVal trainNo As String(), ByVal consigneeCode As String) As String

        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)

        Try
            '○作業シート設定
            TrySetExcelWorkSheet("タンク車発送実績", String.Format("TEMPLATE_{0}", OfficeCode))

            Dim rowIndex As Integer = 0
            Dim maxRowIndex As Integer = CInt(IIf(PrintData Is Nothing, 0, PrintData.Rows.Count))
            Do

                '○NextPage
                If rowIndex > 0 Then
                    '○作業シート設定
                    TrySetExcelWorkSheet("タンク車発送実績", String.Format("TEMPLATE_{0}", OfficeCode))
                End If

                '○出力シート設定
                If ExcelWorkSheet IsNot Nothing AndAlso OutputSheetNames IsNot Nothing AndAlso Not OutputSheetNames.Contains(ExcelWorkSheet.Name) Then
                    OutputSheetNames.Add(ExcelWorkSheet.Name)
                End If

                '◯ヘッダーの設定
                EditHeaderArea(lodDate, trainNo, consigneeCode)

                '◯明細の設定
                EditDetailArea(rowIndex)

                rowIndex += DETAIL_AREA_ROWS_COUNT
            Loop While rowIndex < maxRowIndex

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
                    OrderByDescending(Function(x) x.Value).
                    Select(Function(x) x.Key).ToList()

                    If TrySetExcelWorkSheet(sheetName) Then
                        ExcelWorkSheet.Delete()
                    End If
                Next
            End If

            '保存処理実行
            ExcelSaveAs(tmpFilePath)
            ExcelBookObj.Close(False)

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try

    End Function

    ''' <summary>
    ''' ヘッダー部の設定
    ''' </summary>
    Private Sub EditHeaderArea(ByVal lodDate As String, ByVal trainNo() As String, ByVal consigneeCode As String)

        Dim rngHeaderArea As Excel.Range = Nothing

        Try

            'タイトル(列車番号)
            rngHeaderArea = ExcelWorkSheet.Range("B1")
            rngHeaderArea.Value = String.Format("出荷実績表({0}列車)", String.Join(",", trainNo))
            ExcelMemoryRelease(rngHeaderArea)

            '出荷日(積込日)
            rngHeaderArea = ExcelWorkSheet.Range("C3")
            rngHeaderArea.Value = CDate(lodDate).ToString("yyyyMMdd")
            ExcelMemoryRelease(rngHeaderArea)

            '出荷基地
            Select Case OfficeCode
                Case CONST_OFFICECODE_010402
                    rngHeaderArea = ExcelWorkSheet.Range("C4")
                    rngHeaderArea.Value = "ENEOS仙台"
                    ExcelMemoryRelease(rngHeaderArea)
                    rngHeaderArea = ExcelWorkSheet.Range("D4")
                    rngHeaderArea.Value = "P061"
                Case CONST_OFFICECODE_011203
                    rngHeaderArea = ExcelWorkSheet.Range("C4")
                    rngHeaderArea.Value = "富士石油"
                    ExcelMemoryRelease(rngHeaderArea)
                    rngHeaderArea = ExcelWorkSheet.Range("D4")
                    rngHeaderArea.Value = "P055"
            End Select
            ExcelMemoryRelease(rngHeaderArea)

            '受入基地
            Select Case OfficeCode
                Case CONST_OFFICECODE_010402
                    rngHeaderArea = ExcelWorkSheet.Range("G4")
                    rngHeaderArea.Value = "JOT盛岡"
                    ExcelMemoryRelease(rngHeaderArea)
                    rngHeaderArea = ExcelWorkSheet.Range("H4")
                    rngHeaderArea.Value = "ZP310"
                Case CONST_OFFICECODE_011203
                    Select Case consigneeCode
                        Case "53"
                            rngHeaderArea = ExcelWorkSheet.Range("G4")
                            rngHeaderArea.Value = "宇都宮"
                            ExcelMemoryRelease(rngHeaderArea)
                            rngHeaderArea = ExcelWorkSheet.Range("H4")
                            rngHeaderArea.Value = "ZP342"
                        Case "54"
                            rngHeaderArea = ExcelWorkSheet.Range("G4")
                            rngHeaderArea.Value = "JOT高崎"
                            ExcelMemoryRelease(rngHeaderArea)
                            rngHeaderArea = ExcelWorkSheet.Range("H4")
                            rngHeaderArea.Value = "ZP343"
                        Case "30"
                            rngHeaderArea = ExcelWorkSheet.Range("G4")
                            rngHeaderArea.Value = "高崎"
                            ExcelMemoryRelease(rngHeaderArea)
                            rngHeaderArea = ExcelWorkSheet.Range("H4")
                            rngHeaderArea.Value = "ZP154"
                    End Select
            End Select
            ExcelMemoryRelease(rngHeaderArea)

            '取扱営業所名
            Select Case OfficeCode
                Case CONST_OFFICECODE_010402
                    rngHeaderArea = ExcelWorkSheet.Range("C6")
                    rngHeaderArea.Value = "日本石油輸送㈱"
                    ExcelMemoryRelease(rngHeaderArea)
                    rngHeaderArea = ExcelWorkSheet.Range("D6")
                    rngHeaderArea.Value = "仙台新港営業所"
                    ExcelMemoryRelease(rngHeaderArea)
                    rngHeaderArea = ExcelWorkSheet.Range("F6")
                    rngHeaderArea.Value = "1286"
                Case CONST_OFFICECODE_011203
                    rngHeaderArea = ExcelWorkSheet.Range("C6")
                    rngHeaderArea.Value = "日本石油輸送㈱"
                    ExcelMemoryRelease(rngHeaderArea)
                    rngHeaderArea = ExcelWorkSheet.Range("D6")
                    rngHeaderArea.Value = "袖ケ浦営業"
                    ExcelMemoryRelease(rngHeaderArea)
                    rngHeaderArea = ExcelWorkSheet.Range("F6")
                    rngHeaderArea.Value = "1286"
            End Select
            ExcelMemoryRelease(rngHeaderArea)

            '輸送経路
            Select Case OfficeCode
                Case CONST_OFFICECODE_010402
                    'ENEOS仙台～JOT盛岡
                    rngHeaderArea = ExcelWorkSheet.Range("H6")
                    rngHeaderArea.Value = "T00016"
                Case CONST_OFFICECODE_011203
                    Select Case consigneeCode
                        Case "53"
                            '富士石油～宇都宮
                            rngHeaderArea = ExcelWorkSheet.Range("H6")
                            rngHeaderArea.Value = "T00026"
                        Case "54"
                            '富士石油～JOT高崎
                            rngHeaderArea = ExcelWorkSheet.Range("H6")
                            rngHeaderArea.Value = "T00022"
                        Case "30"
                            '富士石油～高崎
                            rngHeaderArea = ExcelWorkSheet.Range("H6")
                            rngHeaderArea.Value = "T00021"
                    End Select
            End Select
            ExcelMemoryRelease(rngHeaderArea)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try

    End Sub

    ''' <summary>
    ''' 明細部分の編集
    ''' </summary>
    Private Sub EditDetailArea(ByVal rowIndex As Integer)
        Dim rngDetailArea As Excel.Range = Nothing
        Try

            Dim printRows = PrintData.AsEnumerable.
                Skip(rowIndex).
                Take(DETAIL_AREA_ROWS_COUNT).
                Select(Function(r, i) New With {.row = r, .index = i + DETAIL_AREA_BEGIN_ROW_INDEX}).ToList()

            For Each r In printRows
                'コード
                rngDetailArea = ExcelWorkSheet.Range("C" + r.index.ToString())
                rngDetailArea.Value = r.row("OILCODE")
                ExcelMemoryRelease(rngDetailArea)

                '積載実数量
                rngDetailArea = ExcelWorkSheet.Range("D" + r.index.ToString())
                rngDetailArea.Value = CDec(r.row("CARSAMOUNT")).ToString("#.##0")
                ExcelMemoryRelease(rngDetailArea)

                'ﾀﾝｸ車番号
                rngDetailArea = ExcelWorkSheet.Range("E" + r.index.ToString())
                rngDetailArea.Value = r.row("TANKNUMBER")
                ExcelMemoryRelease(rngDetailArea)
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

End Class

''' <summary>
''' 連結順序票
''' </summary>
Public Class ContactOrder : Inherits OIT0003CustomMultiReportBase

    Private Const TEMP_XLS_FILE_NAME As String = "CONTACTORDER.xlsx"
    Private Const DETAIL_AREA_BEGIN_ROW_INDEX As Integer = 6
    Private Const DETAIL_AREA_ROWS_COUNT As Integer = 23

    Protected OfficeCode As String
    Protected PrintData As DataTable

    Public Sub New(mapId As String, ByVal officeCode As String, printDataClass As DataTable)
        MyBase.New(mapId, TEMP_XLS_FILE_NAME)
        Me.OfficeCode = officeCode
        Me.PrintData = printDataClass

        '○作業シート設定
        TrySetExcelWorkSheet("連結順序票", "TEMPLATE")
    End Sub

    Public Function CreatePrintData(ByVal lodDate As String, ByVal trainNo As String) As String

        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)

        Try

            Dim rowIndex As Integer = 0
            Dim maxRowIndex As Integer = CInt(IIf(PrintData Is Nothing, 0, PrintData.Rows.Count))
            Do
                '○NextPage
                If rowIndex > 0 Then
                    '○作業シート設定
                    TrySetExcelWorkSheet("連結順序票", "TEMPLATE")
                End If

                '○出力シート設定
                If ExcelWorkSheet IsNot Nothing AndAlso OutputSheetNames IsNot Nothing AndAlso Not OutputSheetNames.Contains(ExcelWorkSheet.Name) Then
                    OutputSheetNames.Add(ExcelWorkSheet.Name)
                End If

                '◯ヘッダーの設定
                EditHeaderArea(lodDate, trainNo)

                '◯明細の設定
                EditDetailArea(rowIndex)

                rowIndex += DETAIL_AREA_ROWS_COUNT
            Loop While rowIndex < maxRowIndex

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
                    OrderByDescending(Function(x) x.Value).
                    Select(Function(x) x.Key).ToList()

                    If TrySetExcelWorkSheet(sheetName) Then
                        ExcelWorkSheet.Delete()
                    End If
                Next
            End If

            '保存処理実行
            ExcelSaveAs(tmpFilePath)
            ExcelBookObj.Close(False)

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try

    End Function

    ''' <summary>
    ''' ヘッダー部の設定
    ''' </summary>
    Private Sub EditHeaderArea(ByVal lodDate As String, ByVal trainNo As String)

        Dim rngHeaderArea As Excel.Range = Nothing

        Try

            '出荷日(積込日)
            rngHeaderArea = Me.ExcelWorkSheet.Range("AA4")
            rngHeaderArea.Value = CDate(lodDate).ToString("yyyy") & "年"
            ExcelMemoryRelease(rngHeaderArea)
            rngHeaderArea = Me.ExcelWorkSheet.Range("AB4")
            rngHeaderArea.Value = CDate(lodDate).ToString("MM")
            ExcelMemoryRelease(rngHeaderArea)
            rngHeaderArea = Me.ExcelWorkSheet.Range("AD4")
            rngHeaderArea.Value = CDate(lodDate).ToString("dd")
            ExcelMemoryRelease(rngHeaderArea)

            'タイトル(列車番号)
            rngHeaderArea = Me.ExcelWorkSheet.Range("AG4")
            rngHeaderArea.Value = trainNo
            ExcelMemoryRelease(rngHeaderArea)

            '明細データから取得
            If Me.PrintData IsNot Nothing AndAlso Me.PrintData.Rows.Count > 0 Then
                Dim query = Me.PrintData.AsEnumerable

                'タイトル(タンク車数)
                Dim tankCount As Integer = query.Count
                rngHeaderArea = Me.ExcelWorkSheet.Range("AI4")
                rngHeaderArea.Value = IIf(tankCount > 0, String.Format("{0}車", tankCount.ToString()), "車")
                ExcelMemoryRelease(rngHeaderArea)

                'PG
                tankCount = query.Where(Function(x As DataRow) x.Item("OILCODE").ToString().Equals("1001")).Count
                rngHeaderArea = Me.ExcelWorkSheet.Range("AH13")
                rngHeaderArea.Value = IIf(tankCount > 0, tankCount.ToString(), "")
                ExcelMemoryRelease(rngHeaderArea)

                'RG
                tankCount = query.Where(Function(x As DataRow) x.Item("OILCODE").ToString().Equals("1101")).Count
                rngHeaderArea = Me.ExcelWorkSheet.Range("AH14")
                rngHeaderArea.Value = IIf(tankCount > 0, tankCount.ToString(), "")
                ExcelMemoryRelease(rngHeaderArea)

                'KR
                tankCount = query.Where(Function(x As DataRow) x.Item("OILCODE").ToString().Equals("1301")).Count
                rngHeaderArea = Me.ExcelWorkSheet.Range("AH15")
                rngHeaderArea.Value = IIf(tankCount > 0, tankCount.ToString(), "")
                ExcelMemoryRelease(rngHeaderArea)

                'GO
                tankCount = query.Where(Function(x As DataRow) x.Item("OILCODE").ToString().Equals("1401")).Count
                rngHeaderArea = Me.ExcelWorkSheet.Range("AH16")
                rngHeaderArea.Value = IIf(tankCount > 0, tankCount.ToString(), "")
                ExcelMemoryRelease(rngHeaderArea)

                'AFO
                tankCount = query.Where(Function(x As DataRow) x.Item("OILCODE").ToString().Equals("2101")).Count
                rngHeaderArea = Me.ExcelWorkSheet.Range("AH17")
                rngHeaderArea.Value = IIf(tankCount > 0, tankCount.ToString(), "")
                ExcelMemoryRelease(rngHeaderArea)

                'LSA
                tankCount = query.Where(Function(x As DataRow) x.Item("OILCODE").ToString().Equals("2201")).Count
                rngHeaderArea = Me.ExcelWorkSheet.Range("AH18")
                rngHeaderArea.Value = IIf(tankCount > 0, tankCount.ToString(), "")
                ExcelMemoryRelease(rngHeaderArea)

            End If

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try

    End Sub

    ''' <summary>
    ''' 明細部分の編集
    ''' </summary>
    Private Sub EditDetailArea(ByVal rowIndex As Integer)
        Dim rngDetailArea As Excel.Range = Nothing
        Try

            Dim printRows = PrintData.AsEnumerable.
                Skip(rowIndex).
                Take(DETAIL_AREA_ROWS_COUNT).
                Select(Function(r, i) New With {.row = r, .index = i + DETAIL_AREA_BEGIN_ROW_INDEX}).ToList()

            For Each r In printRows
                '油種名
                rngDetailArea = Me.ExcelWorkSheet.Range("X" + r.index.ToString())
                Dim oilCode As String = r.row("OILCODE").ToString()
                Dim orderingType As String = r.row("ORDERINGTYPE").ToString()

                Select Case oilCode
                    Case "1001"
                        rngDetailArea.Value = "ﾌﾟﾚﾐｱﾑ"
                    Case "1101"
                        rngDetailArea.Value = "ﾚｷﾞｭﾗｰG"
                    Case "1301"
                        rngDetailArea.Value = "ﾄｳﾕ"
                    Case "1401"
                        rngDetailArea.Value = "ｹｲﾕ"
                    Case "1404"
                        Select Case orderingType
                            Case "A"
                                rngDetailArea.Value = "3ｺﾞｳｹｲﾕ"
                            Case "E"
                                rngDetailArea.Value = "ｶﾝﾚｲｹｲﾕ"
                        End Select
                    Case "2101"
                        Select Case orderingType
                            Case "B"
                                rngDetailArea.Value = "0.5AFO"
                            Case "C"
                                rngDetailArea.Value = "ｶﾝﾚｲAFO"
                        End Select
                    Case "2201"
                        rngDetailArea.Value = "0.1AFO"
                End Select
                ExcelMemoryRelease(rngDetailArea)

                'ﾀﾝｸ車番号
                rngDetailArea = Me.ExcelWorkSheet.Range("Z" + r.index.ToString())
                rngDetailArea.Value = r.row("TANKNO")
                ExcelMemoryRelease(rngDetailArea)
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

End Class

Public Class OrderDetail : Inherits OIT0003CustomMultiReportBase

    Private Const TEMP_XLS_FILE_NAME As String = "ORDERDETAIL.xlsx"
    Private Const DETAIL_AREA_BEGIN_ROW_INDEX As Integer = 7
    Private Const DETAIL_AREA_ROWS_COUNT As Integer = 22
    Private Const DETAIL_AREA_PAGE_COUNT As Integer = 25

    Protected PrintData As DataTable

    Public Sub New(mapId As String, printDataClass As DataTable)
        MyBase.New(mapId, TEMP_XLS_FILE_NAME)

        Me.PrintData = printDataClass

        '○作業シート設定
        TrySetExcelWorkSheet("受注明細", "TEMPLATE")
    End Sub

    Public Function CreatePrintData(Optional ByVal trainNo As String = Nothing, Optional ByVal lodDate As String = Nothing, Optional ByVal depDate As String = Nothing) As String

        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)

        Try

            Dim query = Me.PrintData.AsEnumerable().
                GroupBy(Function(r) New With {
                    Key .officeCode = r("OFFICECODE").ToString(),
                    Key .trainNo = r("TRAINNO").ToString(),
                    Key .lodDate = r("LODDATE").ToString(),
                    Key .depDate = r("DEPDATE").ToString()
                }).
                Select(Function(g) New With {
                    g.Key.officeCode,
                    g.Key.trainNo,
                    g.Key.lodDate,
                    g.Key.depDate,
                    .rows = g.Select(Function(r) r).ToArray()
                }).
                Where(Function(p)
                          Dim selectFlg As Boolean = True
                          If selectFlg AndAlso Not String.IsNullOrEmpty(trainNo) Then
                              selectFlg = (p.trainNo = trainNo)
                          End If
                          If selectFlg AndAlso Not String.IsNullOrEmpty(lodDate) Then
                              selectFlg = (p.lodDate = lodDate)
                          End If
                          If selectFlg AndAlso Not String.IsNullOrEmpty(lodDate) Then
                              selectFlg = (p.depDate = depDate)
                          End If
                          Return selectFlg
                      End Function).ToList()

            If query.Any() Then

                Dim pageIndex As Integer = 0
                Do
                    '○NextPage
                    If pageIndex > 0 Then
                        '○作業シート設定
                        TrySetExcelWorkSheet("受注明細", "TEMPLATE")
                    End If

                    '○出力シート設定
                    If ExcelWorkSheet IsNot Nothing AndAlso OutputSheetNames IsNot Nothing AndAlso Not OutputSheetNames.Contains(ExcelWorkSheet.Name) Then
                        OutputSheetNames.Add(ExcelWorkSheet.Name)
                        HiddenColumn(query.Item(pageIndex).officeCode)
                    End If

                    '◯ヘッダーの設定
                    EditHeaderArea(query.Item(pageIndex).trainNo, query.Item(pageIndex).lodDate, query.Item(pageIndex).depDate)

                    '◯明細の設定
                    EditDetailArea(query.Item(pageIndex).rows, query.Item(pageIndex).officeCode)

                    pageIndex += 1
                Loop While pageIndex < query.Count()
            Else
                '○出力シート設定
                If ExcelWorkSheet IsNot Nothing AndAlso OutputSheetNames IsNot Nothing AndAlso Not OutputSheetNames.Contains(ExcelWorkSheet.Name) Then
                    OutputSheetNames.Add(ExcelWorkSheet.Name)
                    HiddenColumn(Nothing)
                End If

                '◯ヘッダーの設定
                EditHeaderArea(trainNo, lodDate, depDate)
            End If

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
                    OrderByDescending(Function(x) x.Value).
                    Select(Function(x) x.Key).ToList()

                    If TrySetExcelWorkSheet(sheetName) Then
                        ExcelWorkSheet.Delete()
                    End If
                Next
            End If

            '保存処理実行
            ExcelSaveAs(tmpFilePath)
            ExcelBookObj.Close(False)

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try

    End Function

    ''' <summary>
    ''' ヘッダー部の設定
    ''' </summary>
    Private Sub EditHeaderArea(ByVal trainNo As String, ByVal lodDate As String, ByVal depDate As String)

        Dim rngHeaderArea As Excel.Range = Nothing

        Try

            '列車番号
            rngHeaderArea = Me.ExcelWorkSheet.Range("B3")
            rngHeaderArea.Value = trainNo
            ExcelMemoryRelease(rngHeaderArea)

            '積込予定日
            rngHeaderArea = Me.ExcelWorkSheet.Range("D3")
            rngHeaderArea.Value = lodDate
            ExcelMemoryRelease(rngHeaderArea)

            '発予定日
            rngHeaderArea = Me.ExcelWorkSheet.Range("F3")
            rngHeaderArea.Value = depDate
            ExcelMemoryRelease(rngHeaderArea)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try

    End Sub

    ''' <summary>
    ''' 明細部分の編集
    ''' </summary>
    Private Sub EditDetailArea(ByVal printRows As DataRow(), ByVal officeCode As String)
        Dim rngDetailArea As Excel.Range = Nothing
        Try

            Select Case officeCode
                Case BaseDllConst.CONST_OFFICECODE_011402
                    printRows = printRows.
                        OrderBy(Function(r) r("ORDERNO").ToString()).
                        ThenBy(Function(r) IIf(r("ACTUALLODDATE").ToString() <> "", r("ACTUALLODDATE").ToString(), r("LODDATE").ToString())).
                        ThenBy(Function(r)
                                   Dim result As Integer = 2
                                   Select Case r("OILCODE").ToString()
                                       Case BaseDllConst.CONST_HTank,
                                            BaseDllConst.CONST_RTank,
                                            BaseDllConst.CONST_TTank,
                                            BaseDllConst.CONST_MTTank,
                                            BaseDllConst.CONST_KTank1,
                                            BaseDllConst.CONST_K3Tank1,
                                            BaseDllConst.CONST_LTank1,
                                            BaseDllConst.CONST_ATank
                                           result = 1
                                       Case Else
                                           result = 2
                                   End Select
                                   Return result
                               End Function
                        ).
                        ThenBy(Function(r) r("TANKNO").ToString().PadLeft(8, "0"c)).
                        ThenBy(Function(r) r("LINEORDER").ToString().PadLeft(2, "0"c)).
                        ThenBy(Function(r) r("SHIPORDER").ToString().PadLeft(2, "0"c)).
                        ToArray()
                Case BaseDllConst.CONST_OFFICECODE_012402
                    printRows = printRows.
                        OrderBy(Function(r) r("ORDERNO").ToString()).
                        ThenBy(Function(r) r("DETAILNO").ToString()).
                        ToArray()
                Case Else
                    printRows = printRows.
                        OrderBy(Function(r) r("ORDERNO").ToString()).
                        ThenBy(Function(r) IIf(r("ACTUALLODDATE").ToString() <> "", r("ACTUALLODDATE").ToString(), r("LODDATE").ToString())).
                        ThenBy(Function(r) r("PRIORITYNO").ToString()).
                        ThenBy(Function(r) r("TANKNO").ToString().PadLeft(8, "0"c)).
                        ThenBy(Function(r) r("LINEORDER").ToString().PadLeft(2, "0"c)).
                        ThenBy(Function(r) r("SHIPORDER").ToString().PadLeft(2, "0"c)).
                        ToArray()
            End Select

            Dim orderNo As String = ""
            Dim pageCount As Integer = 1
            Dim startRowIndex As Integer = DETAIL_AREA_BEGIN_ROW_INDEX
            Dim rowIndex As Integer = 0
            For Each r In printRows.Select(Function(x, i) New With {.row = x, .index = i + DETAIL_AREA_BEGIN_ROW_INDEX}).ToList()

                If orderNo <> "" AndAlso orderNo <> r.row("ORDERNO").ToString() Then
                    pageCount += 1
                    startRowIndex = pageCount * DETAIL_AREA_ROWS_COUNT + DETAIL_AREA_BEGIN_ROW_INDEX
                    rowIndex = 0
                End If

                'Excel行No
                Dim rIdx As String = (startRowIndex + rowIndex).ToString()

                '発送順
                rngDetailArea = Me.ExcelWorkSheet.Range("A" + rIdx)
                rngDetailArea.Value = r.row("SHIPORDER")
                ExcelMemoryRelease(rngDetailArea)

                '車番
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + rIdx)
                rngDetailArea.Value = r.row("TANKNO")
                ExcelMemoryRelease(rngDetailArea)

                '油種
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + rIdx)
                rngDetailArea.Value = r.row("OILNAME")
                ExcelMemoryRelease(rngDetailArea)

                '数量
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + rIdx)
                rngDetailArea.Value = r.row("CARSAMOUNT")
                ExcelMemoryRelease(rngDetailArea)

                '荷主
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + rIdx)
                rngDetailArea.Value = r.row("SHIPPERSNAME")
                ExcelMemoryRelease(rngDetailArea)

                'ジョイント
                rngDetailArea = Me.ExcelWorkSheet.Range("G" + rIdx)
                rngDetailArea.Value = r.row("JOINT")
                ExcelMemoryRelease(rngDetailArea)

                '構内取
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + rIdx)
                rngDetailArea.Value = r.row("SECONDCONSIGNEENAME")
                ExcelMemoryRelease(rngDetailArea)

                '積置
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + rIdx)
                rngDetailArea.Value = r.row("STACKINGFLG")
                ExcelMemoryRelease(rngDetailArea)

                '先返し
                rngDetailArea = Me.ExcelWorkSheet.Range("J" + rIdx)
                rngDetailArea.Value = r.row("FIRSTRETURNFLG")
                ExcelMemoryRelease(rngDetailArea)

                '後返し
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + rIdx)
                rngDetailArea.Value = r.row("AFTERRETURNFLG")
                ExcelMemoryRelease(rngDetailArea)

                'OT輸送
                rngDetailArea = Me.ExcelWorkSheet.Range("L" + rIdx)
                rngDetailArea.Value = r.row("OTTRANSPORTFLG")
                ExcelMemoryRelease(rngDetailArea)

                '格上げ
                rngDetailArea = Me.ExcelWorkSheet.Range("M" + rIdx)
                rngDetailArea.Value = r.row("UPGRADEFLG")
                ExcelMemoryRelease(rngDetailArea)

                '格下げ
                rngDetailArea = Me.ExcelWorkSheet.Range("N" + rIdx)
                rngDetailArea.Value = r.row("DOWNGRADEFLG")
                ExcelMemoryRelease(rngDetailArea)

                '積込日(実)
                rngDetailArea = Me.ExcelWorkSheet.Range("O" + rIdx)
                rngDetailArea.Value = r.row("ACTUALLODDATE")
                ExcelMemoryRelease(rngDetailArea)

                '発日(実)
                rngDetailArea = Me.ExcelWorkSheet.Range("P" + rIdx)
                rngDetailArea.Value = r.row("ACTUALDEPDATE")
                ExcelMemoryRelease(rngDetailArea)

                '積車着日(実)
                rngDetailArea = Me.ExcelWorkSheet.Range("Q" + rIdx)
                rngDetailArea.Value = r.row("ACTUALARRDATE")
                ExcelMemoryRelease(rngDetailArea)

                '受入日(実)
                rngDetailArea = Me.ExcelWorkSheet.Range("R" + rIdx)
                rngDetailArea.Value = r.row("ACTUALACCDATE")
                ExcelMemoryRelease(rngDetailArea)

                '空車着日日(実)
                rngDetailArea = Me.ExcelWorkSheet.Range("S" + rIdx)
                rngDetailArea.Value = r.row("ACTUALEMPARRDATE")
                ExcelMemoryRelease(rngDetailArea)

                '積込入線列車
                rngDetailArea = Me.ExcelWorkSheet.Range("T" + rIdx)
                rngDetailArea.Value = r.row("LOADINGIRILINEORDER")
                ExcelMemoryRelease(rngDetailArea)

                '積込入線順
                rngDetailArea = Me.ExcelWorkSheet.Range("U" + rIdx)
                rngDetailArea.Value = r.row("LINEORDER")
                ExcelMemoryRelease(rngDetailArea)

                '回線
                rngDetailArea = Me.ExcelWorkSheet.Range("V" + rIdx)
                rngDetailArea.Value = r.row("LINE")
                ExcelMemoryRelease(rngDetailArea)

                '充填ポイント
                rngDetailArea = Me.ExcelWorkSheet.Range("W" + rIdx)
                rngDetailArea.Value = r.row("FILLINGPOINT")
                ExcelMemoryRelease(rngDetailArea)

                orderNo = r.row("ORDERNO").ToString()
                rowIndex += 1
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

    Private Sub HiddenColumn(ByVal officeCode As String)
        '列非表示処理構築
        Dim funcHiddenColumn As Func(Of String, Boolean, Boolean) =
            Function(ByVal strRange As String, ByVal hidden As Boolean)
                Dim rngColumnArea As Excel.Range = Nothing
                Dim rngEntryColumn As Excel.Range = Nothing
                Try
                    rngColumnArea = Me.ExcelWorkSheet.Range(strRange)
                    rngEntryColumn = rngColumnArea.EntireColumn
                    rngEntryColumn.Hidden = hidden
                    ExcelMemoryRelease(rngColumnArea)
                    ExcelMemoryRelease(rngEntryColumn)
                Catch ex As Exception
                    Throw
                Finally
                    ExcelMemoryRelease(rngColumnArea)
                    ExcelMemoryRelease(rngEntryColumn)
                End Try
                Return True
            End Function

        '************
        '列非表示設定
        '************

        '共通
        '積込日(実)、発日(実)、積車着日(実)、受入日(実)、空車着日日(実)、積込入線列車、積込入線順
        funcHiddenColumn("O:S", True)

        '営業所別
        Select Case officeCode
            Case BaseDllConst.CONST_OFFICECODE_010402
                '○仙台
                'ジョイント
                funcHiddenColumn("G:G", False)
                '構内取
                funcHiddenColumn("H:H", True)
                '積置
                funcHiddenColumn("I:I", False)
                '先返し
                funcHiddenColumn("J:J", True)
                '後返し
                funcHiddenColumn("K:K", True)
                'OT輸送
                funcHiddenColumn("L:L", True)
                '積込入線列車
                funcHiddenColumn("T:T", True)
                '積込入線順
                funcHiddenColumn("U:U", True)
                '回線
                funcHiddenColumn("V:V", True)
                '充填ポイント
                funcHiddenColumn("W:W", True)
            Case BaseDllConst.CONST_OFFICECODE_011201
                '○五井
                'ジョイント
                funcHiddenColumn("G:G", True)
                '構内取
                funcHiddenColumn("H:H", True)
                '積置
                funcHiddenColumn("I:I", True)
                '先返し
                funcHiddenColumn("J:J", True)
                '後返し
                funcHiddenColumn("K:K", True)
                'OT輸送
                funcHiddenColumn("L:L", False)
                '積込入線列車
                funcHiddenColumn("T:T", True)
                '積込入線順
                funcHiddenColumn("U:U", True)
                '回線
                funcHiddenColumn("V:V", False)
                '充填ポイント
                funcHiddenColumn("W:W", False)
            Case BaseDllConst.CONST_OFFICECODE_011202
                '○甲子
                'ジョイント
                funcHiddenColumn("G:G", True)
                '構内取
                funcHiddenColumn("H:H", True)
                '積置
                funcHiddenColumn("I:I", True)
                '先返し
                funcHiddenColumn("J:J", True)
                '後返し
                funcHiddenColumn("K:K", True)
                'OT輸送
                funcHiddenColumn("L:L", True)
                '積込入線列車
                funcHiddenColumn("T:T", True)
                '積込入線順
                funcHiddenColumn("U:U", True)
                '回線
                funcHiddenColumn("V:V", False)
                '充填ポイント
                funcHiddenColumn("W:W", False)
            Case BaseDllConst.CONST_OFFICECODE_011203
                '○袖ヶ浦
                'ジョイント
                funcHiddenColumn("G:G", True)
                '構内取
                funcHiddenColumn("H:H", False)
                '積置
                funcHiddenColumn("I:I", True)
                '先返し
                funcHiddenColumn("J:J", True)
                '後返し
                funcHiddenColumn("K:K", True)
                'OT輸送
                funcHiddenColumn("L:L", True)
                '積込入線列車
                funcHiddenColumn("T:T", False)
                '積込入線順
                funcHiddenColumn("U:U", False)
                '回線
                funcHiddenColumn("V:V", True)
                '充填ポイント
                funcHiddenColumn("W:W", True)
            Case BaseDllConst.CONST_OFFICECODE_011402
                '○根岸
                'ジョイント
                funcHiddenColumn("G:G", True)
                '構内取
                funcHiddenColumn("H:H", True)
                '積置
                funcHiddenColumn("I:I", False)
                '先返し
                funcHiddenColumn("J:J", False)
                '後返し
                funcHiddenColumn("K:K", False)
                'OT輸送
                funcHiddenColumn("L:L", True)
                '積込入線列車
                funcHiddenColumn("T:T", True)
                '積込入線順
                funcHiddenColumn("U:U", True)
                '回線
                funcHiddenColumn("V:V", True)
                '充填ポイント
                funcHiddenColumn("W:W", True)
            Case Else
                '○その他
                'ジョイント
                funcHiddenColumn("G:G", True)
                '構内取
                funcHiddenColumn("H:H", True)
                '積置
                funcHiddenColumn("I:I", True)
                '先返し
                funcHiddenColumn("J:J", True)
                '後返し
                funcHiddenColumn("K:K", True)
                'OT輸送
                funcHiddenColumn("L:L", True)
                '積込入線列車
                funcHiddenColumn("T:T", True)
                '積込入線順
                funcHiddenColumn("U:U", True)
                '回線
                funcHiddenColumn("V:V", True)
                '充填ポイント
                funcHiddenColumn("W:W", True)
        End Select
    End Sub

End Class
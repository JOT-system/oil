﻿Option Strict On
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

    Public Shared Function CreateActualShip(mapId As String, officeCode As String, printDataClass As DataTable, ByVal lodDate As String, ByVal trainNo As String) As List(Of String)
        Dim urlList As New List(Of String)
        Using repCbj = New ActualShip(mapId, officeCode, printDataClass)
            Dim url As String
            Try
                url = repCbj.CreatePrintData(lodDate, trainNo)
            Catch ex As Exception
                Throw
            End Try
            If Not String.IsNullOrWhiteSpace(url) Then
                urlList.Add(url)
            End If
        End Using
        Return urlList
    End Function

    Public Shared Function CreateTankDispatch(mapId As String, officeCode As String, printDataClass As DataTable, ByVal lodDate As String, ByVal trainNo As String) As List(Of String)
        Dim urlList As New List(Of String)


        If printDataClass IsNot Nothing AndAlso printDataClass.Rows.Count > 0 Then
            'グループ化（油層所毎）
            Dim group = printDataClass.AsEnumerable.
                    GroupBy(Function(g As DataRow) Tuple.Create(g.Item("CONSIGNEECODE").ToString())).
                    Select(Function(g) New With {.consigneeCode = g.Key.Item1, .dataTable = g.CopyToDataTable}).ToList()
            'グループ毎に作成
            Try
                For Each item In group
                    Using repCbj = New TankDispatch(mapId, officeCode, item.dataTable)
                        Dim url As String
                        url = repCbj.CreatePrintData(lodDate, trainNo, item.consigneeCode)
                        If Not String.IsNullOrWhiteSpace(url) Then
                            urlList.Add(url)
                        End If
                    End Using
                Next
            Catch ex As Exception
                Throw
            End Try
        Else
            Using repCbj = New TankDispatch(mapId, officeCode, printDataClass)
                Dim url As String
                Try
                    url = repCbj.CreatePrintData(lodDate, trainNo, Nothing)
                Catch ex As Exception
                    Throw
                End Try
                If Not String.IsNullOrWhiteSpace(url) Then
                    urlList.Add(url)
                End If
            End Using
        End If

        Return urlList
    End Function

    Public Shared Function CreateContactOrder(mapId As String, officeCode As String, printDataClass As DataTable, ByVal lodDate As String, ByVal trainNo As String) As List(Of String)
        Dim urlList As New List(Of String)
        Using repCbj = New ContactOrder(mapId, officeCode, printDataClass)
            Dim url As String
            Try
                url = repCbj.CreatePrintData(lodDate, trainNo)
            Catch ex As Exception
                Throw
            End Try
            If Not String.IsNullOrWhiteSpace(url) Then
                urlList.Add(url)
            End If
        End Using
        Return urlList
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
    ''' エクセルシートオブジェクト
    ''' </summary>
    Protected ExcelWorkSheet As Excel.Worksheet

    ''' <summary>
    ''' エクセルブックオブジェクト
    ''' </summary>
    Protected ExcelTempBookObj As Excel.Workbook
    ''' <summary>
    ''' エクセルシートコレクション
    ''' </summary>
    Protected ExcelTempSheets As Excel.Sheets
    ''' <summary>
    ''' 一時作業シート
    ''' </summary>
    Protected ExcelTempSheet As Excel.Worksheet

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Protected ExcelTemplatePath As String = ""
    Protected UploadRootPath As String = ""
    Protected UrlRoot As String = ""
    Protected xlProcId As Integer
    Protected OfficeCode As String
    Protected PrintData As DataTable

    Protected Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer

    Protected Sub Init(mapId As String, excelFileName As String)
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
            ExcelTempBookObj = ExcelBooksObj.Open(ExcelTemplatePath,
                                                UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                                [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
            ExcelTempSheets = ExcelTempBookObj.Worksheets

            Dim searchList As New List(Of String)
            searchList.Add(String.Format("TEMPLATE_{0}", OfficeCode))
            searchList.Add("TEMPLATE")

            Dim templateSheetIndex As Integer = 1
            For Each searchName As String In searchList
                If templateSheetIndex > 1 Then
                    Exit For
                End If
                For Each sheet As Excel.Worksheet In ExcelTempSheets
                    If sheet.Name.Equals(searchName) Then
                        templateSheetIndex = sheet.Index
                        ExcelMemoryRelease(sheet)
                        Exit For
                    End If
                    ExcelMemoryRelease(sheet)
                Next
            Next
            ExcelMemoryRelease(ExcelTempSheet)
            ExcelTempSheet = DirectCast(ExcelTempSheets(templateSheetIndex), Excel.Worksheet)

            ExcelMemoryRelease(ExcelBookObj)
            ExcelBookObj = ExcelBooksObj.Add()
            ExcelMemoryRelease(ExcelWorkSheets)
            ExcelWorkSheets = ExcelBookObj.Worksheets
            ExcelMemoryRelease(ExcelWorkSheet)
            ExcelWorkSheet = DirectCast(ExcelWorkSheets.Item(1), Excel.Worksheet)
            ExcelTempSheet.Copy(After:=ExcelWorkSheet)
            ExcelWorkSheet.Delete()
            ExcelMemoryRelease(ExcelWorkSheet)
            ExcelWorkSheet = DirectCast(ExcelWorkSheets.Item(1), Excel.Worksheet)


        Catch ex As Exception
            If xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try
    End Sub

    Protected Sub SetTemplateSheet(ByVal sheetName As String, Optional ByVal defaultSheetName As String = "TEMPLATE")
        Try
            Dim searchList As New List(Of String)
            searchList.Add(sheetName)
            searchList.Add(defaultSheetName)

            Dim templateSheetIndex As Integer = 1
            For Each searchName As String In searchList
                If templateSheetIndex > 1 Then
                    Exit For
                End If
                For Each sheet As Excel.Worksheet In ExcelWorkSheets
                    If sheet.Name.Equals(searchName) Then
                        templateSheetIndex = sheet.Index
                        ExcelMemoryRelease(sheet)
                        Exit For
                    End If
                    ExcelMemoryRelease(sheet)
                Next
            Next
            ExcelTempSheet = DirectCast(ExcelWorkSheets(templateSheetIndex), Excel.Worksheet)
        Catch ex As Exception
            Throw
        End Try
    End Sub

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
        'Excel Sheetコレクションの解放
        ExcelMemoryRelease(ExcelTempSheets)
        'Excel Bookオブジェクトを閉じる
        If ExcelTempBookObj IsNot Nothing Then
            Try
                'ExcelBookObj.Close(Excel.XlSaveAction.xlDoNotSaveChanges)
                ExcelTempBookObj.Close(False)
            Catch ex As Exception
            End Try
        End If
        ExcelMemoryRelease(ExcelTempBookObj)

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

    Public Sub New(ByVal mapId As String, ByVal officeCode As String, printDataClass As DataTable)
        Try
            Me.PrintData = printDataClass
            Me.OfficeCode = officeCode
            Init(mapId, TEMP_XLS_FILE_NAME)
        Catch ex As Exception
            If xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' 帳票作成処理
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreatePrintData(ByVal lodDate As String, ByVal trainNo As String) As String

        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xls"
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)

        Try

            ExcelWorkSheet.Name = "出荷実績表"

            Dim rowIndex As Integer = 0
            Dim maxRowIndex As Integer = CInt(IIf(PrintData Is Nothing, 0, PrintData.Rows.Count))
            Do
                '○テンプレートシート複製
                If rowIndex > 0 Then
                    ExcelTempSheet.Copy(After:=ExcelWorkSheet)
                    ExcelMemoryRelease(ExcelWorkSheet)
                    ExcelWorkSheet = DirectCast(ExcelBookObj.ActiveSheet, Excel.Worksheet)
                    ExcelWorkSheet.Name = String.Format("出荷実績表({0})", CInt(rowIndex / DETAIL_AREA_ROWS_COUNT) + 1)
                End If

                '◯ヘッダーの設定
                EditHeaderArea(lodDate, trainNo)

                '◯明細の設定
                EditDetailArea(rowIndex)

                rowIndex += DETAIL_AREA_ROWS_COUNT
            Loop While rowIndex < maxRowIndex

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
                                rngDetailArea.Value = "LTA"
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

    Public Sub New(ByVal mapId As String, ByVal officeCode As String, printDataClass As DataTable)
        Try
            PrintData = printDataClass
            Me.OfficeCode = officeCode
            Init(mapId, TEMP_XLS_FILE_NAME)
        Catch ex As Exception
            If xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try
    End Sub

    Public Function CreatePrintData(ByVal lodDate As String, ByVal trainNo As String, ByVal consigneeCode As String) As String

        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)

        Try
            ExcelWorkSheet.Name = "タンク車発送実績"

            Dim rowIndex As Integer = 0
            Dim maxRowIndex As Integer = CInt(IIf(PrintData Is Nothing, 0, PrintData.Rows.Count))
            Do

                '○NextPage
                If rowIndex > 0 Then
                    ExcelTempSheet.Copy(After:=ExcelWorkSheet)
                    ExcelMemoryRelease(ExcelWorkSheet)
                    ExcelWorkSheet = DirectCast(ExcelBookObj.ActiveSheet, Excel.Worksheet)
                    ExcelWorkSheet.Name = String.Format("タンク車発送実績({0})", CInt(rowIndex / DETAIL_AREA_ROWS_COUNT) + 1)
                End If

                '◯ヘッダーの設定
                EditHeaderArea(lodDate, trainNo, consigneeCode)

                '◯明細の設定
                EditDetailArea(rowIndex)

                rowIndex += DETAIL_AREA_ROWS_COUNT
            Loop While rowIndex < maxRowIndex

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
    Private Sub EditHeaderArea(ByVal lodDate As String, ByVal trainNo As String, ByVal consigneeCode As String)

        Dim rngHeaderArea As Excel.Range = Nothing

        Try

            'タイトル(列車番号)
            rngHeaderArea = ExcelWorkSheet.Range("B1")
            rngHeaderArea.Value = String.Format("出荷実績表({0}列車)", trainNo)
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

    Public Sub New(ByVal mapId As String, ByVal officeCode As String, printDataClass As DataTable)
        Try
            PrintData = printDataClass
            Me.OfficeCode = officeCode
            Init(mapId, TEMP_XLS_FILE_NAME)
        Catch ex As Exception
            If xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try
    End Sub

    Public Function CreatePrintData(ByVal lodDate As String, ByVal trainNo As String) As String

        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)

        Try

            ExcelWorkSheet.Name = "連結順序票"

            Dim rowIndex As Integer = 0
            Dim maxRowIndex As Integer = CInt(IIf(PrintData Is Nothing, 0, PrintData.Rows.Count))
            Do
                '○NextPage
                If rowIndex > 0 Then
                    ExcelTempSheet.Copy(After:=ExcelWorkSheet)
                    ExcelMemoryRelease(ExcelWorkSheet)
                    ExcelWorkSheet = DirectCast(ExcelBookObj.ActiveSheet, Excel.Worksheet)
                    ExcelWorkSheet.Name = String.Format("連結順序票({0})", CInt(rowIndex / DETAIL_AREA_ROWS_COUNT) + 1)
                End If

                '◯ヘッダーの設定
                EditHeaderArea(lodDate, trainNo)

                '◯明細の設定
                EditDetailArea(rowIndex)

                rowIndex += DETAIL_AREA_ROWS_COUNT
            Loop While rowIndex < maxRowIndex

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
                                rngDetailArea.Value = "LTA"
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

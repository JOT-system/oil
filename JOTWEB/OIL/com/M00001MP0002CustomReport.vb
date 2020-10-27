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
    ''' 表と表の縦間隔(ExcelのnPT値で設定)
    ''' </summary>
    Private Const TABLE_TO_TABLE_MARGIN As Integer = 12
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
            Dim margeFieldLenFieldName As new Dictionary(Of String, String)
            'データ部生成
            Select Case Me.ViewId
                Case "VIEW001"
                    CreateView001(margeFieldLenFieldName)
                Case "VIEW002"
                    margeFieldLenFieldName.Add("BIGOILNAME", "ROWSPANFIELD1")
                    margeFieldLenFieldName.Add("TRAINCLASSNAME", "ROWSPANFIELD2")
                    CreateView002(margeFieldLenFieldName)
                Case "VIEW003"
                    margeFieldLenFieldName.Add("BIGOILNAME", "ROWSPANFIELD1")
                    margeFieldLenFieldName.Add("TRAINCLASSNAME", "ROWSPANFIELD1")
                    CreateView003(margeFieldLenFieldName)
                Case "VIEW004"
                    margeFieldLenFieldName.Add("BIGOILNAME", "ROWSPANFIELD1")
                    margeFieldLenFieldName.Add("TRAINCLASSNAME", "ROWSPANFIELD1")
                    CreateView004(margeFieldLenFieldName)
                Case "VIEW005"
                    margeFieldLenFieldName.Add("TRAINCLASSNAME", "ROWSPANFIELD1")
                    CreateView005(margeFieldLenFieldName)
                Case "VIEW006"
                    margeFieldLenFieldName.Add("BIGOILNAME", "ROWSPANFIELD1")
                    margeFieldLenFieldName.Add("TRAINCLASSNAME", "ROWSPANFIELD2")
                    CreateView006(margeFieldLenFieldName)
            End Select
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
            fileName = dlFileName
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
    Private Sub CreateView001(margeFieldLenFieldName As Dictionary(Of String, String))
        Dim rngWork As Excel.Range = Nothing
        Dim rngHeaderTmp As Excel.Range = Nothing
        Dim rngHeader As Excel.Range = Nothing
        Dim dicPrintField As New Dictionary(Of String, String) From {
             {"B", "OILNAME"},
             {"C", "MAERUIKEIVOLUME"},
             {"D", "RUIKEIVOLUME"},
             {"E", "VOLUME"},
             {"F", "VOLUMECHANGE"},
             {"G", "VOLUMERATIO"},
             {"H", "LYVOLUMECHANGE"},
             {"I", "LYVOLUMERATIO"}
            }
        Const STARTROW As Integer = 4

        Try
            Dim currentRow As Integer = STARTROW
            Dim tableStartRow As Integer = 0
            For Each dt As DataTable In DispData
                'ヘッダー行のレンジ設定
                If rngHeader Is Nothing Then
                    rngHeaderTmp = Me.ExcelWorkSheet.Range("RNG_TABLE_HEADER")
                    rngHeader = rngHeaderTmp.Rows
                Else
                    '空行追加
                    Dim empRowTmp As Excel.Range
                    Dim empRow As Excel.Range
                    empRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    empRow = empRowTmp.Rows
                    empRow.RowHeight = TABLE_TO_TABLE_MARGIN
                    ExcelMemoryRelease(empRow)
                    ExcelMemoryRelease(empRowTmp)
                    currentRow = currentRow + 1
                    Dim pasteTabHeaderRowTmp As Excel.Range
                    Dim pasteTabHeaderRow As Excel.Range
                    pasteTabHeaderRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    pasteTabHeaderRow = pasteTabHeaderRowTmp.Rows
                    rngHeader.Copy(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRowTmp)
                    currentRow = currentRow + 1
                End If
                tableStartRow = currentRow - 1

                '行データのループ
                For Each dr As DataRow In dt.Rows

                    Dim rngValset As Excel.Range = Nothing
                    For Each colField In dicPrintField
                        rngValset = Me.ExcelWorkSheet.Range(String.Format("{0}{1}", colField.Key, currentRow))

                        '値の設定
                        rngValset.Value = dr(colField.Value)

                        ExcelMemoryRelease(rngValset)

                    Next colField

                    currentRow = currentRow + 1
                Next
                Dim tableEndRow = currentRow - 1

                Dim rngTableArea As Excel.Range = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", dicPrintField.Keys(1), tableStartRow, dicPrintField.Last.Key, tableStartRow))
                TableBorderEdit(rngTableArea)
                ExcelMemoryRelease(rngTableArea)

                rngTableArea = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", dicPrintField.First.Key, tableStartRow + 1, dicPrintField.Last.Key, tableEndRow))
                TableBorderEdit(rngTableArea)
                ExcelMemoryRelease(rngTableArea)
                '油種名行ヘッダーの色を前日(累計)の列ヘッダーの色に合わせる
                Dim rowHeaderArea As Excel.Range = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", dicPrintField.First.Key, tableStartRow + 1, dicPrintField.First.Key, tableEndRow))
                Dim headerSample As Excel.Range = DirectCast(rngHeaderTmp(1, 3), Excel.Range)
                Dim colHeaderInterior As Excel.Interior = headerSample.Interior

                Dim rowHeaderInterior As Excel.Interior = rowHeaderArea.Interior
                rowHeaderInterior.Color = colHeaderInterior.Color

                ExcelMemoryRelease(rowHeaderInterior)
                ExcelMemoryRelease(colHeaderInterior)
                ExcelMemoryRelease(headerSample)
                ExcelMemoryRelease(rowHeaderArea)
                'グラフの範囲変更
                Dim chartsObj As Excel.ChartObjects = DirectCast(Me.ExcelWorkSheet.ChartObjects, Excel.ChartObjects)
                Dim chartObj As Excel.ChartObject = DirectCast(chartsObj(0), Excel.ChartObject)
                Dim chart As Excel.Chart = chartObj.Chart
                Dim rngChartAreaXVal = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", "B", tableStartRow + 1, "B", tableEndRow))
                Dim rngChartAreaTodayVal = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", "D", tableStartRow + 1, "D", tableEndRow))
                Dim rngChartAreaYesterdayVal = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", "C", tableStartRow + 1, "C", tableEndRow))

                Dim fullSeriesCollectionObj As Excel.SeriesCollection

                fullSeriesCollectionObj = DirectCast(chart.SeriesCollection, Excel.SeriesCollection)
                Dim seriesTodayObj As Excel.Series
                Dim seriesYesterdayObj As Excel.Series
                seriesTodayObj = DirectCast(fullSeriesCollectionObj.Item(1), Excel.Series)
                seriesYesterdayObj = DirectCast(fullSeriesCollectionObj.Item(2), Excel.Series)
                seriesTodayObj.Values = rngChartAreaTodayVal
                seriesTodayObj.XValues = rngChartAreaXVal
                seriesYesterdayObj.Values = rngChartAreaYesterdayVal
                seriesYesterdayObj.XValues = rngChartAreaXVal
                'chart.SetSourceData(rngChartArea)
                ExcelMemoryRelease(rngChartAreaXVal)
                ExcelMemoryRelease(rngChartAreaTodayVal)
                ExcelMemoryRelease(rngChartAreaYesterdayVal)

                ExcelMemoryRelease(seriesTodayObj)
                ExcelMemoryRelease(seriesYesterdayObj)
                ExcelMemoryRelease(fullSeriesCollectionObj)
                ExcelMemoryRelease(chart)
                ExcelMemoryRelease(chartObj)
                ExcelMemoryRelease(chartsObj)
                'グラフの位置移動
                Dim rngBottomTable As Excel.Range = Me.ExcelWorkSheet.Range("B" & (tableEndRow + 2).ToString)
                Dim shapesObj As Excel.Shapes = Me.ExcelWorkSheet.Shapes
                Dim shapeObj As Excel.Shape = DirectCast(shapesObj(0), Excel.Shape)
                shapeObj.Top = CSng(rngBottomTable.Top)
                shapeObj.Left = CSng(rngBottomTable.Left)
                ExcelMemoryRelease(rngBottomTable)
                ExcelMemoryRelease(shapeObj)
                ExcelMemoryRelease(shapesObj)
            Next dt
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        Catch ex As Exception
            Throw '呼出し元に返却
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        End Try

    End Sub
    ''' <summary>
    ''' 支店別帳票出力
    ''' </summary>
    ''' <param name="margeFieldLenFieldName"></param>
    Private Sub CreateView002(margeFieldLenFieldName As Dictionary(Of String, String))
        Dim rngWork As Excel.Range = Nothing
        Dim rngHeaderTmp As Excel.Range = Nothing
        Dim rngHeader As Excel.Range = Nothing
        Dim dicPrintField As New Dictionary(Of String, String) From {
             {"B", "BIGOILNAME"},
             {"C", "TRAINCLASSNAME"},
             {"D", "ORGNAME"},
             {"E", "MAERUIKEIVOLUME"},
             {"F", "RUIKEIVOLUME"},
             {"G", "VOLUME"},
             {"H", "VOLUMECHANGE"},
             {"I", "VOLUMERATIO"},
             {"J", "LYVOLUMECHANGE"},
             {"K", "LYVOLUMERATIO"}
            }
        Const STARTROW As Integer = 4

        Try
            Dim currentRow As Integer = STARTROW
            Dim tableStartRow As Integer = 0
            For Each dt As DataTable In DispData
                'ヘッダー行のレンジ設定
                If rngHeader Is Nothing Then
                    rngHeaderTmp = Me.ExcelWorkSheet.Range("RNG_TABLE_HEADER")
                    rngHeader = rngHeaderTmp.Rows
                Else
                    '空行追加
                    Dim empRowTmp As Excel.Range
                    Dim empRow As Excel.Range
                    empRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    empRow = empRowTmp.Rows
                    empRow.RowHeight = TABLE_TO_TABLE_MARGIN
                    ExcelMemoryRelease(empRow)
                    ExcelMemoryRelease(empRowTmp)
                    currentRow = currentRow + 1
                    Dim pasteTabHeaderRowTmp As Excel.Range
                    Dim pasteTabHeaderRow As Excel.Range
                    pasteTabHeaderRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    pasteTabHeaderRow = pasteTabHeaderRowTmp.Rows
                    rngHeader.Copy(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRowTmp)
                    currentRow = currentRow + 1
                End If
                tableStartRow = currentRow - 1

                '行データのループ
                For Each dr As DataRow In dt.Rows

                    Dim rngValset As Excel.Range = Nothing
                    For Each colField In dicPrintField
                        rngValset = Me.ExcelWorkSheet.Range(String.Format("{0}{1}", colField.Key, currentRow))
                        '結合済のセルの場合次の列にスキップ
                        If Convert.ToBoolean(rngValset.MergeCells) Then
                            ExcelMemoryRelease(rngValset)
                            Continue For
                        End If

                        '結合対象セルか判定
                        If margeFieldLenFieldName.ContainsKey(colField.Value) Then
                            Dim lengthRowName As String = margeFieldLenFieldName(colField.Value)
                            Dim rowLengthStr As String = Convert.ToString(dr(lengthRowName))
                            If IsNumeric(rowLengthStr) Then
                                Dim rowLength As Integer = CInt(dr(lengthRowName))
                                MargeRowRange(rngValset, rowLength)
                            End If
                        End If

                        '値の設定
                        rngValset.Value = dr(colField.Value)

                        ExcelMemoryRelease(rngValset)

                    Next colField

                    '合計行の場合ハイライト
                    Dim rowSumVal As String = Convert.ToString(dr("ORGNAME"))
                    If rowSumVal.Equals("計") Then
                        Dim sumRow As Excel.Range = Nothing
                        sumRow = Me.ExcelWorkSheet.Range(String.Format("{0}{2}:{1}{2}", "D", "K", currentRow))
                        HilightSummary(sumRow)
                        ExcelMemoryRelease(sumRow)
                    End If
                    currentRow = currentRow + 1
                Next
                Dim tableEndRow = currentRow - 1

                Dim rngTableArea As Excel.Range = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", dicPrintField.First.Key, tableStartRow, dicPrintField.Last.Key, tableEndRow))
                TableBorderEdit(rngTableArea)
                ExcelMemoryRelease(rngTableArea)
            Next dt
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        Catch ex As Exception
            Throw '呼出し元に返却
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        End Try

    End Sub
    ''' <summary>
    ''' 荷主別　請負輸送OT輸送合算帳票出力
    ''' </summary>
    ''' <param name="margeFieldLenFieldName"></param>
    Private Sub CreateView003(margeFieldLenFieldName As Dictionary(Of String, String))
        Dim rngWork As Excel.Range = Nothing
        Dim rngHeaderTmp As Excel.Range = Nothing
        Dim rngHeader As Excel.Range = Nothing
        Dim dicPrintField As New Dictionary(Of String, String) From {
             {"B", "BIGOILNAME"},
             {"C", "TRAINCLASSNAME"},
             {"D", "SHIPPERNAME"},
             {"E", "MAERUIKEIVOLUME"},
             {"F", "RUIKEIVOLUME"},
             {"G", "VOLUME"},
             {"H", "VOLUMECHANGE"},
             {"I", "VOLUMERATIO"},
             {"J", "LYVOLUMECHANGE"},
             {"K", "LYVOLUMERATIO"}
            }
        Const STARTROW As Integer = 4

        Try
            Dim currentRow As Integer = STARTROW
            Dim tableStartRow As Integer = 0
            For Each dt As DataTable In DispData
                'ヘッダー行のレンジ設定
                If rngHeader Is Nothing Then
                    rngHeaderTmp = Me.ExcelWorkSheet.Range("RNG_TABLE_HEADER")
                    rngHeader = rngHeaderTmp.Rows
                Else
                    '空行追加
                    Dim empRowTmp As Excel.Range
                    Dim empRow As Excel.Range
                    empRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    empRow = empRowTmp.Rows
                    empRow.RowHeight = TABLE_TO_TABLE_MARGIN
                    ExcelMemoryRelease(empRow)
                    ExcelMemoryRelease(empRowTmp)
                    currentRow = currentRow + 1
                    Dim pasteTabHeaderRowTmp As Excel.Range
                    Dim pasteTabHeaderRow As Excel.Range
                    pasteTabHeaderRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    pasteTabHeaderRow = pasteTabHeaderRowTmp.Rows
                    rngHeader.Copy(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRowTmp)
                    currentRow = currentRow + 1
                End If
                tableStartRow = currentRow - 1

                '行データのループ
                For Each dr As DataRow In dt.Rows

                    Dim rngValset As Excel.Range = Nothing
                    For Each colField In dicPrintField
                        rngValset = Me.ExcelWorkSheet.Range(String.Format("{0}{1}", colField.Key, currentRow))
                        '結合済のセルの場合次の列にスキップ
                        If Convert.ToBoolean(rngValset.MergeCells) Then
                            ExcelMemoryRelease(rngValset)
                            Continue For
                        End If

                        '結合対象セルか判定
                        If margeFieldLenFieldName.ContainsKey(colField.Value) Then
                            Dim lengthRowName As String = margeFieldLenFieldName(colField.Value)
                            Dim rowLengthStr As String = Convert.ToString(dr(lengthRowName))
                            If IsNumeric(rowLengthStr) Then
                                Dim rowLength As Integer = CInt(dr(lengthRowName))
                                MargeRowRange(rngValset, rowLength)
                            End If
                        End If

                        '値の設定
                        rngValset.Value = dr(colField.Value)

                        ExcelMemoryRelease(rngValset)

                    Next colField

                    '合計行の場合ハイライト
                    Dim rowSumVal As String = Convert.ToString(dr("SHIPPERNAME"))
                    If rowSumVal.Equals("計") Then
                        Dim sumRow As Excel.Range = Nothing
                        sumRow = Me.ExcelWorkSheet.Range(String.Format("{0}{2}:{1}{2}", "D", "K", currentRow))
                        HilightSummary(sumRow)
                        ExcelMemoryRelease(sumRow)
                    End If
                    currentRow = currentRow + 1
                Next
                Dim tableEndRow = currentRow - 1

                Dim rngTableArea As Excel.Range = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", dicPrintField.First.Key, tableStartRow, dicPrintField.Last.Key, tableEndRow))
                TableBorderEdit(rngTableArea)
                ExcelMemoryRelease(rngTableArea)
            Next dt
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        Catch ex As Exception
            Throw '呼出し元に返却
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        End Try

    End Sub
    ''' <summary>
    ''' 荷受人別帳票出力
    ''' </summary>
    ''' <param name="margeFieldLenFieldName"></param>
    Private Sub CreateView004(margeFieldLenFieldName As Dictionary(Of String, String))
        Dim rngWork As Excel.Range = Nothing
        Dim rngHeaderTmp As Excel.Range = Nothing
        Dim rngHeader As Excel.Range = Nothing
        Dim dicPrintField As New Dictionary(Of String, String) From {
             {"B", "BIGOILNAME"},
             {"C", "TRAINCLASSNAME"},
             {"D", "CONSIGNEENAME"},
             {"E", "MAERUIKEIVOLUME"},
             {"F", "RUIKEIVOLUME"},
             {"G", "VOLUME"},
             {"H", "VOLUMECHANGE"},
             {"I", "VOLUMERATIO"},
             {"J", "LYVOLUMECHANGE"},
             {"K", "LYVOLUMERATIO"}
            }
        Const STARTROW As Integer = 4

        Try
            Dim currentRow As Integer = STARTROW
            Dim tableStartRow As Integer = 0
            For Each dt As DataTable In DispData
                'ヘッダー行のレンジ設定
                If rngHeader Is Nothing Then
                    rngHeaderTmp = Me.ExcelWorkSheet.Range("RNG_TABLE_HEADER")
                    rngHeader = rngHeaderTmp.Rows
                Else
                    '空行追加
                    Dim empRowTmp As Excel.Range
                    Dim empRow As Excel.Range
                    empRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    empRow = empRowTmp.Rows
                    empRow.RowHeight = TABLE_TO_TABLE_MARGIN
                    ExcelMemoryRelease(empRow)
                    ExcelMemoryRelease(empRowTmp)
                    currentRow = currentRow + 1
                    Dim pasteTabHeaderRowTmp As Excel.Range
                    Dim pasteTabHeaderRow As Excel.Range
                    pasteTabHeaderRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    pasteTabHeaderRow = pasteTabHeaderRowTmp.Rows
                    rngHeader.Copy(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRowTmp)
                    currentRow = currentRow + 1
                End If
                tableStartRow = currentRow - 1

                '行データのループ
                For Each dr As DataRow In dt.Rows

                    Dim rngValset As Excel.Range = Nothing
                    For Each colField In dicPrintField
                        rngValset = Me.ExcelWorkSheet.Range(String.Format("{0}{1}", colField.Key, currentRow))
                        '結合済のセルの場合次の列にスキップ
                        If Convert.ToBoolean(rngValset.MergeCells) Then
                            ExcelMemoryRelease(rngValset)
                            Continue For
                        End If

                        '結合対象セルか判定
                        If margeFieldLenFieldName.ContainsKey(colField.Value) Then
                            Dim lengthRowName As String = margeFieldLenFieldName(colField.Value)
                            Dim rowLengthStr As String = Convert.ToString(dr(lengthRowName))
                            If IsNumeric(rowLengthStr) Then
                                Dim rowLength As Integer = CInt(dr(lengthRowName))
                                MargeRowRange(rngValset, rowLength)
                            End If
                        End If

                        '値の設定
                        rngValset.Value = dr(colField.Value)

                        ExcelMemoryRelease(rngValset)

                    Next colField

                    '合計行の場合ハイライト
                    Dim rowSumVal As String = Convert.ToString(dr("CONSIGNEENAME"))
                    If rowSumVal.Equals("計") Then
                        Dim sumRow As Excel.Range = Nothing
                        sumRow = Me.ExcelWorkSheet.Range(String.Format("{0}{2}:{1}{2}", "D", "K", currentRow))
                        HilightSummary(sumRow)
                        ExcelMemoryRelease(sumRow)
                    End If
                    currentRow = currentRow + 1
                Next
                Dim tableEndRow = currentRow - 1

                Dim rngTableArea As Excel.Range = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", dicPrintField.First.Key, tableStartRow, dicPrintField.Last.Key, tableEndRow))
                TableBorderEdit(rngTableArea)
                ExcelMemoryRelease(rngTableArea)
            Next dt
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        Catch ex As Exception
            Throw '呼出し元に返却
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        End Try

    End Sub
    ''' <summary>
    ''' 油種別（中分類）帳票出力
    ''' </summary>
    ''' <param name="margeFieldLenFieldName"></param>
    Private Sub CreateView005(margeFieldLenFieldName As Dictionary(Of String, String))
        Dim rngWork As Excel.Range = Nothing
        Dim rngHeaderTmp As Excel.Range = Nothing
        Dim rngHeader As Excel.Range = Nothing
        Dim dicPrintField As New Dictionary(Of String, String) From {
             {"B", "TRAINCLASSNAME"},
             {"C", "OILNAME"},
             {"D", "MAERUIKEIVOLUME"},
             {"E", "RUIKEIVOLUME"},
             {"F", "VOLUME"},
             {"G", "VOLUMECHANGE"},
             {"H", "VOLUMERATIO"},
             {"I", "LYVOLUMECHANGE"},
             {"J", "LYVOLUMERATIO"}
            }
        Const STARTROW As Integer = 4

        Try
            Dim currentRow As Integer = STARTROW
            Dim tableStartRow As Integer = 0
            For Each dt As DataTable In DispData
                'ヘッダー行のレンジ設定
                If rngHeader Is Nothing Then
                    rngHeaderTmp = Me.ExcelWorkSheet.Range("RNG_TABLE_HEADER")
                    rngHeader = rngHeaderTmp.Rows
                Else
                    '空行追加
                    Dim empRowTmp As Excel.Range
                    Dim empRow As Excel.Range
                    empRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    empRow = empRowTmp.Rows
                    empRow.RowHeight = TABLE_TO_TABLE_MARGIN
                    ExcelMemoryRelease(empRow)
                    ExcelMemoryRelease(empRowTmp)
                    currentRow = currentRow + 1
                    Dim pasteTabHeaderRowTmp As Excel.Range
                    Dim pasteTabHeaderRow As Excel.Range
                    pasteTabHeaderRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    pasteTabHeaderRow = pasteTabHeaderRowTmp.Rows
                    rngHeader.Copy(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRowTmp)
                    currentRow = currentRow + 1
                End If
                tableStartRow = currentRow - 1

                '行データのループ
                For Each dr As DataRow In dt.Rows

                    Dim rngValset As Excel.Range = Nothing
                    For Each colField In dicPrintField
                        rngValset = Me.ExcelWorkSheet.Range(String.Format("{0}{1}", colField.Key, currentRow))
                        '結合済のセルの場合次の列にスキップ
                        If Convert.ToBoolean(rngValset.MergeCells) Then
                            ExcelMemoryRelease(rngValset)
                            Continue For
                        End If

                        '結合対象セルか判定
                        If margeFieldLenFieldName.ContainsKey(colField.Value) Then
                            Dim lengthRowName As String = margeFieldLenFieldName(colField.Value)
                            Dim rowLengthStr As String = Convert.ToString(dr(lengthRowName))
                            If IsNumeric(rowLengthStr) Then
                                Dim rowLength As Integer = CInt(dr(lengthRowName))
                                MargeRowRange(rngValset, rowLength)
                            End If
                        End If

                        '値の設定
                        rngValset.Value = dr(colField.Value)

                        ExcelMemoryRelease(rngValset)

                    Next colField

                    '合計行の場合ハイライト
                    Dim rowSumVal As String = Convert.ToString(dr("OILNAME"))
                    If rowSumVal.Equals("合計") OrElse rowSumVal.EndsWith("油計") Then
                        Dim sumRow As Excel.Range = Nothing
                        sumRow = Me.ExcelWorkSheet.Range(String.Format("{0}{2}:{1}{2}", "C", "J", currentRow))
                        Dim bgCol As Integer = -99999
                        If rowSumVal.EndsWith("油計") Then
                            bgCol = RGB(240, 230, 140)
                        End If
                        HilightSummary(sumRow, bgCol)
                        '油種名エリアのフォントを太字
                        Dim rngBoldArea As Excel.Range = DirectCast(sumRow(1, 1), Excel.Range)
                        Dim fonObj = rngBoldArea.Font
                        fonObj.Bold = True
                        ExcelMemoryRelease(fonObj)
                        ExcelMemoryRelease(rngBoldArea)
                        ExcelMemoryRelease(sumRow)
                    End If
                    currentRow = currentRow + 1
                Next
                Dim tableEndRow = currentRow - 1

                Dim rngTableArea As Excel.Range = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", dicPrintField.First.Key, tableStartRow, dicPrintField.Last.Key, tableEndRow))
                TableBorderEdit(rngTableArea)
                ExcelMemoryRelease(rngTableArea)
            Next dt
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        Catch ex As Exception
            Throw '呼出し元に返却
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        End Try

    End Sub
    ''' <summary>
    ''' 荷主別票出力
    ''' </summary>
    ''' <param name="margeFieldLenFieldName"></param>
    Private Sub CreateView006(margeFieldLenFieldName As Dictionary(Of String, String))
        Dim rngWork As Excel.Range = Nothing
        Dim rngHeaderTmp As Excel.Range = Nothing
        Dim rngHeader As Excel.Range = Nothing
        Dim dicPrintField As New Dictionary(Of String, String) From {
             {"B", "BIGOILNAME"},
             {"C", "TRAINCLASSNAME"},
             {"D", "SHIPPERNAME"},
             {"E", "MAERUIKEIVOLUME"},
             {"F", "RUIKEIVOLUME"},
             {"G", "VOLUME"},
             {"H", "VOLUMECHANGE"},
             {"I", "VOLUMERATIO"},
             {"J", "LYVOLUMECHANGE"},
             {"K", "LYVOLUMERATIO"}
            }
        Const STARTROW As Integer = 4

        Try
            Dim currentRow As Integer = STARTROW
            Dim tableStartRow As Integer = 0
            For Each dt As DataTable In DispData
                'ヘッダー行のレンジ設定
                If rngHeader Is Nothing Then
                    rngHeaderTmp = Me.ExcelWorkSheet.Range("RNG_TABLE_HEADER")
                    rngHeader = rngHeaderTmp.Rows
                Else
                    '空行追加
                    Dim empRowTmp As Excel.Range
                    Dim empRow As Excel.Range
                    empRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    empRow = empRowTmp.Rows
                    empRow.RowHeight = TABLE_TO_TABLE_MARGIN
                    ExcelMemoryRelease(empRow)
                    ExcelMemoryRelease(empRowTmp)
                    currentRow = currentRow + 1
                    Dim pasteTabHeaderRowTmp As Excel.Range
                    Dim pasteTabHeaderRow As Excel.Range
                    pasteTabHeaderRowTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", currentRow))
                    pasteTabHeaderRow = pasteTabHeaderRowTmp.Rows
                    rngHeader.Copy(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRow)
                    ExcelMemoryRelease(pasteTabHeaderRowTmp)
                    currentRow = currentRow + 1
                End If
                tableStartRow = currentRow - 1

                '行データのループ
                For Each dr As DataRow In dt.Rows

                    Dim rngValset As Excel.Range = Nothing
                    For Each colField In dicPrintField
                        rngValset = Me.ExcelWorkSheet.Range(String.Format("{0}{1}", colField.Key, currentRow))
                        '結合済のセルの場合次の列にスキップ
                        If Convert.ToBoolean(rngValset.MergeCells) Then
                            ExcelMemoryRelease(rngValset)
                            Continue For
                        End If

                        '結合対象セルか判定
                        If margeFieldLenFieldName.ContainsKey(colField.Value) Then
                            Dim lengthRowName As String = margeFieldLenFieldName(colField.Value)
                            Dim rowLengthStr As String = Convert.ToString(dr(lengthRowName))
                            If IsNumeric(rowLengthStr) Then
                                Dim rowLength As Integer = CInt(dr(lengthRowName))
                                MargeRowRange(rngValset, rowLength)
                            End If
                        End If

                        '値の設定
                        rngValset.Value = dr(colField.Value)

                        ExcelMemoryRelease(rngValset)

                    Next colField

                    '合計行の場合ハイライト
                    Dim rowSumVal As String = Convert.ToString(dr("SHIPPERNAME"))
                    If rowSumVal.Equals("計") Then
                        Dim sumRow As Excel.Range = Nothing
                        sumRow = Me.ExcelWorkSheet.Range(String.Format("{0}{2}:{1}{2}", "D", "K", currentRow))
                        HilightSummary(sumRow)
                        ExcelMemoryRelease(sumRow)
                    End If
                    '総計行の場合ハイライト
                    rowSumVal = Convert.ToString(dr("TRAINCLASSNAME"))
                    If rowSumVal.Equals("総計") Then
                        Dim sumRow As Excel.Range = Nothing
                        sumRow = Me.ExcelWorkSheet.Range(String.Format("{0}{2}:{1}{2}", "C", "K", currentRow))
                        HilightSummary(sumRow)
                        ExcelMemoryRelease(sumRow)
                    End If
                    currentRow = currentRow + 1
                Next
                Dim tableEndRow = currentRow - 1

                Dim rngTableArea As Excel.Range = Me.ExcelWorkSheet.Range(String.Format("{0}{1}:{2}{3}", dicPrintField.First.Key, tableStartRow, dicPrintField.Last.Key, tableEndRow))
                TableBorderEdit(rngTableArea)
                ExcelMemoryRelease(rngTableArea)
            Next dt
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        Catch ex As Exception
            Throw '呼出し元に返却
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngHeader)
            ExcelMemoryRelease(rngHeaderTmp)
        End Try

    End Sub
    ''' <summary>
    ''' 縦行の結合
    ''' </summary>
    ''' <param name="margeStartRng">開始セル</param>
    ''' <param name="rowLength">伸ばす長さ</param>
    Private Sub MargeRowRange(margeStartRng As Excel.Range, rowLength As Integer)
        '0や1の場合結合の意味が無いのでスキップ
        If rowLength = 0 OrElse rowLength = 1 Then
            Return
        End If
        Dim rngMarge As Excel.Range = Nothing
        Try
            rngMarge = margeStartRng.Resize(RowSize:=rowLength)
            rngMarge.Merge()
            ExcelMemoryRelease(rngMarge)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngMarge)
        End Try
    End Sub
    ''' <summary>
    ''' 合計行のハイライト設定
    ''' </summary>
    ''' <param name="rngHightLightArea"></param>
    ''' <param name="bgColor">特に指定が無ければデフォルト</param>
    Private Sub HilightSummary(rngHightLightArea As Excel.Range, Optional bgColor As Integer = -99999)
        If bgColor = -99999 Then
            bgColor = RGB(238, 232, 170) '240 230 140
        End If
        Dim intrColor As Excel.Interior = Nothing
        Try
            intrColor = rngHightLightArea.Interior
            intrColor.Color = bgColor
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(intrColor)
        End Try

    End Sub
    ''' <summary>
    ''' テーブルのボーダー生成
    ''' </summary>
    ''' <param name="rngTargetTable"></param>
    Private Sub TableBorderEdit(rngTargetTable As Excel.Range)
        Dim allBorder = rngTargetTable.Borders
        For Each borderIdx In {Excel.XlBordersIndex.xlEdgeBottom, Excel.XlBordersIndex.xlEdgeLeft,
                               Excel.XlBordersIndex.xlEdgeRight, Excel.XlBordersIndex.xlEdgeTop,
                               Excel.XlBordersIndex.xlInsideHorizontal, Excel.XlBordersIndex.xlInsideVertical}

            Dim targetBorder = allBorder(borderIdx)

            targetBorder.Color = RGB(100, 100, 100)
            targetBorder.LineStyle = Excel.XlLineStyle.xlContinuous
            targetBorder.Weight = Excel.XlBorderWeight.xlThin
            ExcelMemoryRelease(targetBorder)
        Next

        ExcelMemoryRelease(allBorder)
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

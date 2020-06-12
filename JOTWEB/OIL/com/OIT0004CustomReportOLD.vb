Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 在庫管理表個別帳票作成クラス(2020/06/01 三種レイアウト前版）
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）
''' 現状未使用、別帳票で指標する可能性がある為保持</remarks>
Public Class OIT0004CustomReportOLD : Implements IDisposable
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
    Private PrintData As OIT0004OilStockCreate.DispDataClass
    Private xlProcId As Integer

    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As OIT0004OilStockCreate.DispDataClass)
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
        Me.UrlRoot = String.Format("{0}://{1}/PRINT/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID)

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
        Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("在庫管理表"), Excel.Worksheet)
        Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
    End Sub
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData() As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte
        Try
            '***** TODO処理 ここから *****
            'rngWrite = Me.ExcelWorkSheet.Range("A1")
            'rngWrite.Value = "test"
            '〇〇月度＋油槽所名の設定
            EditDateConsignee()
            '日付の設定
            EditDateArea()
            '提案表部分の作成
            EditSuggestArea()
            '在庫表部分の作成
            EditStockArea()
            '***** TODO処理 ここまで *****
            ExcelTempSheet.Delete() '雛形シート削除
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
    ''' 帳票の日付、タイトル設定
    ''' </summary>
    Private Sub EditDateConsignee()
        Dim targetDate As String = Me.PrintData.StockDate.Values(6).ItemDate.ToString("yyyy年M月度")
        Dim consignee As String = Me.PrintData.ConsigneeName
        Dim title As String = String.Format("{0} ({1})", targetDate, consignee)
        Dim rngTitleArea As Excel.Range = Nothing
        Try
            rngTitleArea = Me.ExcelWorkSheet.Range("B2")
            rngTitleArea.Value = title
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngTitleArea)
        End Try

    End Sub
    ''' <summary>
    ''' 出力帳票日付部分の設定
    ''' </summary>
    Private Sub EditDateArea()
        Dim startDate As Date = DateSerial(Now.Year, Now.Month, Now.Day)
        startDate = (From itm In Me.PrintData.StockDate.Values Select itm.ItemDate).FirstOrDefault
        Dim rngCell As Excel.Range = Nothing
        Try
            rngCell = Me.ExcelWorkSheet.Range("RNG_DATE_FIRST")
            rngCell.Value = startDate
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngCell)
        End Try
    End Sub
    ''' <summary>
    ''' 提案エリアの生成
    ''' </summary>
    Private Sub EditSuggestArea()
        Dim rngWork As Excel.Range = Nothing
        Dim rngTmp As Excel.Range = Nothing
        Dim rngSummary As Excel.Range = Nothing
        Dim rngData As Excel.Range = Nothing
        Dim rngCell As Excel.Range = Nothing
        Dim baseSummaryAdrs As String = ""
        Dim maxTrainCnt As Integer = 5
        Dim rngInterSect As Excel.Range = Nothing
        Dim rngDataArea As Excel.Range = Nothing
        Dim brds As Excel.Borders = Nothing
        Dim brdItem As Excel.Border = Nothing
        Dim exlErrs As Excel.Errors = Nothing
        Dim exlErr As Excel.Error = Nothing
        Dim cell As Excel.Range = Nothing
        Try
            '*************
            '提案欄非表示の場合はテンプレートより対象行削除し終了
            '*************
            If Me.PrintData.ShowSuggestList = False Then
                rngWork = Me.ExcelWorkSheet.Range("3:8")
                rngWork.Delete(Excel.XlDeleteShiftDirection.xlShiftUp)
                Return
            End If
            '*************
            '列車情報の転記(意図的に数式で値を設定し「数字が文字」のビックリマークを抑止)
            '*************
            rngTmp = Me.ExcelWorkSheet.Range("RNG_TRAIN")
            maxTrainCnt = rngTmp.Count
            Dim trainList(0, maxTrainCnt - 1) As Object
            For i = 0 To maxTrainCnt - 1
                If i > Me.PrintData.TrainList.Count - 1 Then
                    trainList(0, i) = "="""""
                Else
                    trainList(0, i) = "=""" & Me.PrintData.TrainList.Values(i).TrainNo & """ & """""
                End If
            Next
            rngTmp.Value = trainList
            Erase trainList
            trainList = Nothing
            ExcelMemoryRelease(rngTmp)
            '*************
            '油種数の退避
            '*************
            Dim oilTypeRowCnt = Me.PrintData.SuggestOilNameList.Count
            '******************************
            '提案欄油種行の拡張（罫線維持）
            '******************************
            rngInterSect = Me.ExcelWorkSheet.Range("RNG_INTERSECT")
            rngTmp = Me.ExcelWorkSheet.Range(String.Format("5:{0}", oilTypeRowCnt + 2))
            Dim interSectAdrs As String = Me.ExcelAppObj.Intersect(rngTmp, rngInterSect).Address
            rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
            rngDataArea = Me.ExcelWorkSheet.Range(interSectAdrs)
            brds = rngDataArea.Borders
            brdItem = brds(Excel.XlBordersIndex.xlInsideHorizontal)
            brdItem.LineStyle = Excel.XlLineStyle.xlContinuous
            brdItem.ColorIndex = 0
            brdItem.TintAndShade = 0
            brdItem.Weight = Excel.XlBorderWeight.xlThin
            ExcelMemoryRelease(brdItem)

            brdItem = brds(Excel.XlBordersIndex.xlEdgeBottom)
            brdItem.LineStyle = Excel.XlLineStyle.xlContinuous
            brdItem.ColorIndex = 0
            brdItem.TintAndShade = 0
            brdItem.Weight = Excel.XlBorderWeight.xlThin
            ExcelMemoryRelease(brdItem)
            ExcelMemoryRelease(brds)
            ExcelMemoryRelease(rngDataArea)
            ExcelMemoryRelease(rngTmp)
            '*************
            '合計行の追記
            '*************
            rngSummary = Me.ExcelTempSheet.Range("RNG_SUG_SUMMARY")
            rngTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", oilTypeRowCnt + 3))
            rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
            rngSummary.Copy(rngTmp)
            rngCell = CType(rngTmp(1, 2), Excel.Range)
            baseSummaryAdrs = rngCell.Address
            ExcelMemoryRelease(rngCell)
            ExcelMemoryRelease(rngTmp)
            '*************
            '油種名の設定
            '*************
            Dim oilNameList = (From oilItm In PrintData.SuggestOilNameList.Values Where oilItm.OilCode <> OIT0004OilStockCreate.DispDataClass.SUMMARY_CODE Select oilItm.OilName).ToList
            Dim oilNameArray(oilNameList.Count - 1, 0) As String
            Dim oilIdx As Integer = 0
            For Each oilNameItm In oilNameList
                oilNameArray(oilIdx, 0) = oilNameItm
                oilIdx = oilIdx + 1
            Next
            rngTmp = Me.ExcelWorkSheet.Range("RNG_SUG_OILNAME_START")
            rngWork = rngTmp.Offset(1)
            Dim trainNumStartRow = rngWork.Address(True, False).Split("$"c)(1)
            rngCell = rngWork.Resize(RowSize:=oilNameList.Count)
            rngCell.Value = oilNameArray
            ExcelMemoryRelease(rngCell)
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngTmp)
            '******************
            '車数の設定
            '******************
            EditTrainNum("F" & trainNumStartRow, maxTrainCnt, Me.PrintData)
            '******************
            '提案欄の値を展開
            '******************
            If Me.PrintData.HasMoveInsideItem Then
                '構内取りではないほうの合計文言変更
                rngCell = Me.ExcelWorkSheet.Range(baseSummaryAdrs)
                rngCell.Value = "中計(両)"
                ExcelMemoryRelease(rngCell)
                'データ表のレイアウト取得
                rngData = Me.ExcelTempSheet.Range("RNG_SUG_DATAROW")
                'データ行の追加
                Dim miStartRow = oilTypeRowCnt + 5
                oilTypeRowCnt = Me.PrintData.MiDispData.SuggestOilNameList.Count
                rngTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", miStartRow, miStartRow + oilTypeRowCnt - 1))

                rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
                ExcelMemoryRelease(rngTmp)
                '罫線情報をペースト
                rngTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", miStartRow, miStartRow + oilTypeRowCnt - 1))
                rngData.Copy(rngTmp)
                ExcelMemoryRelease(rngTmp)
                '合計欄のペースト
                rngTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", miStartRow + oilTypeRowCnt - 1))
                rngSummary.Copy(rngTmp)
                ExcelMemoryRelease(rngTmp)
                '油種名の設定
                Dim miOilStartRow = oilNameList.Count + 2
                oilNameList = (From oilItm In PrintData.MiDispData.SuggestOilNameList.Values Where oilItm.OilCode <> OIT0004OilStockCreate.DispDataClass.SUMMARY_CODE Select oilItm.OilName).ToList
                ReDim oilNameArray(oilNameList.Count - 1, 0)
                oilIdx = 0
                For Each oilNameItm In oilNameList
                    oilNameArray(oilIdx, 0) = oilNameItm
                    oilIdx = oilIdx + 1
                Next
                rngTmp = Me.ExcelWorkSheet.Range("RNG_SUG_OILNAME_START")
                rngWork = rngTmp.Offset(miOilStartRow)
                Dim miTrainNumStartRow = rngWork.Address(True, False).Split("$"c)(1)
                rngCell = rngWork.Resize(RowSize:=oilNameList.Count)
                rngCell.Value = oilNameArray
                ExcelMemoryRelease(rngCell)
                ExcelMemoryRelease(rngWork)
                ExcelMemoryRelease(rngTmp)
                '******************
                '提案欄の値を展開
                '******************
                EditTrainNum("F" & miTrainNumStartRow, maxTrainCnt, Me.PrintData.MiDispData, True)
            End If

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngTmp)
            ExcelMemoryRelease(rngSummary)
            ExcelMemoryRelease(rngData)
            ExcelMemoryRelease(rngCell)
            ExcelMemoryRelease(rngInterSect)
            ExcelMemoryRelease(brdItem)
            ExcelMemoryRelease(brds)
            ExcelMemoryRelease(rngDataArea)
            ExcelMemoryRelease(exlErr)
            ExcelMemoryRelease(exlErrs)
            ExcelMemoryRelease(cell)
        End Try

    End Sub
    ''' <summary>
    ''' 一覧表（上部）の両数を格納
    ''' </summary>
    ''' <param name="startAddrs">開始位置</param>
    ''' <param name="maxTrainCnt">最大車両数</param>
    ''' <param name="suggestList">提案表</param>
    Private Sub EditTrainNum(startAddrs As String, maxTrainCnt As Integer, suggestList As OIT0004OilStockCreate.DispDataClass, Optional isMiArea As Boolean = False)
        Dim rngTmp1 As Excel.Range = Nothing
        Dim rngTmp2 As Excel.Range = Nothing
        Dim rngTmp3 As Excel.Range = Nothing

        Dim rngAveTmp1 As Excel.Range = Nothing
        Dim rngAveTmp2 As Excel.Range = Nothing

        rngTmp1 = Me.ExcelWorkSheet.Range(startAddrs)
        rngTmp2 = rngTmp1.Resize(suggestList.OilTypeList.Count + 1, maxTrainCnt)
        rngTmp3 = rngTmp2.Offset(0)
        rngAveTmp1 = rngTmp1.Resize(suggestList.OilTypeList.Count)
        rngAveTmp2 = rngAveTmp1.Offset(ColumnOffset:=-3)
        Try
            Dim oilAve = (From oilItm In suggestList.OilTypeList.Values Select oilItm.LastSendAverage).ToList
            Dim oilAveArray(oilAve.Count - 1, 0) As Object
            For oilAveIdx = 0 To oilAve.Count - 1
                oilAveArray(oilAveIdx, 0) = oilAve(oilAveIdx)
            Next
            rngAveTmp2.Value = oilAveArray
            rngAveTmp2.NumberFormat = "#,##0"
            Dim trainNumList(suggestList.OilTypeList.Count, maxTrainCnt - 1) As Object
            Dim dateIdx = 1
            For Each sugItm In suggestList.SuggestList.Values
                For i = 0 To maxTrainCnt - 1
                    If i > suggestList.TrainList.Count - 1 Then
                        For rowCnt = 0 To suggestList.OilTypeList.Count - 1
                            trainNumList(rowCnt, i) = "="""""
                        Next
                    Else
                        With sugItm.SuggestOrderItem.Values(i)
                            For rowCnt = 0 To suggestList.OilTypeList.Count - 1
                                trainNumList(rowCnt, i) = .SuggestValuesItem.Values(rowCnt).ItemValue
                            Next
                            If isMiArea Then
                                trainNumList(suggestList.OilTypeList.Count, i) = String.Format("=SUM(R[-{0}]C:R[-1]C)", suggestList.OilTypeList.Count + 1)
                            Else
                                trainNumList(suggestList.OilTypeList.Count, i) = String.Format("=SUM(R[-{0}]C:R[-1]C)", suggestList.OilTypeList.Count)
                            End If

                        End With

                    End If
                Next i
                rngTmp3.Value = trainNumList
                rngTmp3.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight
                ExcelMemoryRelease(rngTmp3)
                rngTmp3 = rngTmp2.Offset(ColumnOffset:=maxTrainCnt * dateIdx)
                dateIdx = dateIdx + 1
            Next sugItm '日付のループ
            ExcelMemoryRelease(rngTmp3)
        Catch ex As Exception
            Throw
        Finally

            ExcelMemoryRelease(rngTmp3)
            ExcelMemoryRelease(rngTmp2)
            ExcelMemoryRelease(rngAveTmp2)
            ExcelMemoryRelease(rngAveTmp1)
            ExcelMemoryRelease(rngTmp1)
        End Try
    End Sub
    ''' <summary>
    ''' 在庫表部分の生成
    ''' </summary>
    Private Sub EditStockArea()
        Dim rngTmp As Excel.Range = Nothing
        Dim rngWork As Excel.Range = Nothing
        Dim rngCell As Excel.Range = Nothing
        Dim rngInterSect As Excel.Range = Nothing
        Dim rngDataArea As Excel.Range = Nothing

        Dim brds As Excel.Borders
        Dim brdItem As Excel.Border
        Try
            '******************************
            'データ開始行の取得
            '******************************
            rngTmp = Me.ExcelWorkSheet.Range("RNG_STC_OILNAME_START")
            rngWork = rngTmp.Offset(1)
            Dim startRow = rngWork.Address(True, False).Split("$"c)(1)
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngTmp)
            '******************************
            '提案欄油種行の拡張（罫線維持）
            '******************************
            Dim oilTypeRowCnt = Me.PrintData.StockList.Values.Count
            rngInterSect = Me.ExcelWorkSheet.Range("RNG_INTERSECT")
            rngTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", startRow, CInt(startRow) + oilTypeRowCnt - 2))
            Dim interSectAdrs As String = Me.ExcelAppObj.Intersect(rngTmp, rngInterSect).Address
            rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromRightOrBelow)
            rngDataArea = Me.ExcelWorkSheet.Range(interSectAdrs)
            brds = rngDataArea.Borders
            brdItem = brds(Excel.XlBordersIndex.xlInsideHorizontal)
            brdItem.LineStyle = Excel.XlLineStyle.xlContinuous
            brdItem.ColorIndex = 0
            brdItem.TintAndShade = 0
            brdItem.Weight = Excel.XlBorderWeight.xlThin
            ExcelMemoryRelease(brdItem)

            brdItem = brds(Excel.XlBordersIndex.xlEdgeBottom)
            brdItem.LineStyle = Excel.XlLineStyle.xlContinuous
            brdItem.ColorIndex = 0
            brdItem.TintAndShade = 0
            brdItem.Weight = Excel.XlBorderWeight.xlThin
            ExcelMemoryRelease(brdItem)


            brdItem = brds(Excel.XlBordersIndex.xlEdgeTop)
            brdItem.LineStyle = Excel.XlLineStyle.xlContinuous
            brdItem.ColorIndex = 0
            brdItem.TintAndShade = 0
            brdItem.Weight = Excel.XlBorderWeight.xlMedium
            ExcelMemoryRelease(brdItem)

            ExcelMemoryRelease(brds)
            ExcelMemoryRelease(rngDataArea)
            ExcelMemoryRelease(rngTmp)

            '******************************
            '油種名の設定
            '******************************
            Dim oilNameList = (From oilItm In PrintData.SuggestOilNameList.Values Where oilItm.OilCode <> OIT0004OilStockCreate.DispDataClass.SUMMARY_CODE Select oilItm.OilName).ToList
            Dim oilNameArray(oilNameList.Count - 1, 0) As String
            Dim oilIdx As Integer = 0
            For Each oilNameItm In oilNameList
                oilNameArray(oilIdx, 0) = oilNameItm
                oilIdx = oilIdx + 1
            Next
            rngTmp = Me.ExcelWorkSheet.Range("RNG_STC_OILNAME_START")
            rngWork = rngTmp.Offset(1)
            rngCell = rngWork.Resize(RowSize:=oilNameList.Count)
            rngCell.Value = oilNameArray
            ExcelMemoryRelease(rngCell)
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngTmp)
            '******************************
            '容量在庫下限の設定
            '******************************
            '3列、n(油種分)行の配列生成
            Dim qOilInfo = (From oilInfoItm In Me.PrintData.StockList.Values Select oilInfoItm)
            Dim oilInfo(qOilInfo.Count - 1, 2) As Object
            oilIdx = 0
            For Each oilItm In qOilInfo
                oilInfo(oilIdx, 0) = oilItm.TankCapacity  '1列目(タンク容量)
                oilInfo(oilIdx, 1) = oilItm.TargetStock  '2列目(適正在庫)
                oilInfo(oilIdx, 2) = "" '3列目(下限)※一旦ブランク
                oilIdx = oilIdx + 1
            Next
            rngTmp = Me.ExcelWorkSheet.Range("RNG_STC_OILNAME_START")
            rngWork = rngTmp.Offset(1, 1)
            rngCell = rngWork.Resize(RowSize:=oilNameList.Count, ColumnSize:=3)
            rngCell.Value = oilInfo
            ExcelMemoryRelease(rngCell)
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngTmp)
            '******************************
            '各日付のデータ設定
            '******************************
            oilIdx = 0
            '横軸の最大数(5(朝在庫～適正比の５カラム） × 日付）
            Dim maxHolizonalColumns = (5 * Me.PrintData.StockDate.Count) - 1
            Dim oilCnt As Integer = qOilInfo.Count
            Dim oilSummaryFormula As String = String.Format("=SUM(R[-{0}]C:R[-1]C)", oilCnt)
            Dim pasteVal(oilCnt, maxHolizonalColumns) As Object
            Dim daysIdx = 0
            For Each oilItm In qOilInfo
                For Each stckItm In oilItm.StockItemList.Values
                    pasteVal(oilIdx, daysIdx) = stckItm.MorningStock
                    pasteVal(oilIdx, daysIdx + 1) = stckItm.Send
                    pasteVal(oilIdx, daysIdx + 2) = stckItm.SummaryReceive
                    pasteVal(oilIdx, daysIdx + 3) = stckItm.EveningStock
                    pasteVal(oilIdx, daysIdx + 4) = "=IF(RC[-1]<RC4,RC[-1]-RC4,IF(RC[-1]<RC5,RC[-1]-RC5,""""))"
                    If oilIdx = 0 Then
                        pasteVal(oilCnt, daysIdx) = oilSummaryFormula
                        pasteVal(oilCnt, daysIdx + 1) = oilSummaryFormula
                        pasteVal(oilCnt, daysIdx + 2) = oilSummaryFormula
                        pasteVal(oilCnt, daysIdx + 3) = oilSummaryFormula
                        pasteVal(oilCnt, daysIdx + 4) = oilSummaryFormula
                    End If
                    daysIdx = daysIdx + 5

                Next
                oilIdx = oilIdx + 1
                daysIdx = 0
            Next
            rngTmp = Me.ExcelWorkSheet.Range("RNG_STC_OILNAME_START")
            rngWork = rngTmp.Offset(1, 4)
            rngCell = rngWork.Resize(RowSize:=oilNameList.Count + 1, ColumnSize:=maxHolizonalColumns + 1)
            rngCell.Value = pasteVal
            ExcelMemoryRelease(rngCell)
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngTmp)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngCell)
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngTmp)
            ExcelMemoryRelease(rngDataArea)
            ExcelMemoryRelease(rngInterSect)
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
            Try
                '念のため当処理で起動したプロセスが残っていたらKill
                Dim xproc As Process = Process.GetProcessById(Me.xlProcId)
                If Not xproc.HasExited Then
                    xproc.Kill()
                End If
            Catch ex As Exception
            End Try

        End If
        disposedValue = True
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

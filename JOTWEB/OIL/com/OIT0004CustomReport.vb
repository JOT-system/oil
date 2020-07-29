Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 在庫管理表個別帳票作成クラス
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）</remarks>
Public Class OIT0004CustomReport : Implements IDisposable
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

    Private xlProcId As Integer
    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As OIT0004OilStockCreate.DispDataClass
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
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <param name="printDataClass"></param>
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
        Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("在庫管理表"), Excel.Worksheet)
        Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
    End Sub
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成するメソッド
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData() As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Try
            '***** 生成処理群ここから *****
            '* 油種（行）、日付（列）を元に雛形の罫線を拡張し体裁を整える
            Dim posInfo As ExcelPositions = ExtentDisplayFormat()
            '* 数値埋め処理
            EditNumberArea(posInfo)
            '***** 生成処理群ここまで *****

            ExcelTempSheet.Delete() '雛形シート削除
            '保存処理実行
            ExcelAppObj.ScreenUpdating = True
            ExcelAppObj.Calculation = Excel.XlCalculation.xlCalculationAutomatic
            ExcelAppObj.Calculate()
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
            End SyncLock
            Me.ExcelBookObj.Close(False)
            Return UrlRoot & tmpFileName
        Catch ex As Exception
            Throw '呼出し元にThrow
        Finally
            ExcelMemoryRelease(rngWrite)
        End Try
    End Function
    ''' <summary>
    ''' 油種、日付に応じ罫線書式等を雛形より拡張する
    ''' </summary>
    ''' <returns>表数値の開始行・終了行を保持</returns>
    Private Function ExtentDisplayFormat() As ExcelPositions
        Dim retPosInfo As New ExcelPositions
        Dim rngSingleFlame As Excel.Range = Nothing
        Dim rngPageteBase As Excel.Range = Nothing
        Dim rngPasteOffset As Excel.Range = Nothing
        Dim rngValueSet As Excel.Range = Nothing
        Dim rngFooter As Excel.Range = Nothing
        Dim rngFooterRow As Excel.Range = Nothing
        Dim fntColor As Excel.Font = Nothing
        Dim intrColor As Excel.Interior = Nothing
        '油種色定義
        Dim colorSettings = New OilTypeColorSettings
        '表の先頭行、最終行保持用
        Dim firstRowNum As Integer = 0
        Dim lastRowNum As Integer = 0
        Dim miFirstRowNum As Integer = 0
        Dim miLastRowNum As Integer = 0
        '
        Dim footerRowNum As Integer = 0
        '******************************
        '油種の行拡張
        '******************************
        Try
            rngPageteBase = Me.ExcelWorkSheet.Range("RNG_LEFTHEADER")
            rngSingleFlame = Me.ExcelWorkSheet.Range("RNG_LEFTHEADER")
            rngFooter = Me.ExcelTempSheet.Range("RNG_CONSIGNEEFOOTER")

            rngFooterRow = rngFooter.Rows()
            footerRowNum = rngFooterRow.Count
            ExcelMemoryRelease(rngFooterRow)
            ExcelMemoryRelease(rngFooter)
            Dim pasteOffset = 0
            Dim lastOilCode As String = Me.PrintData.StockList.Last.Value.OilInfo.OilCode
            Dim trainNumNameTop As String = ""
            Dim trainNumNameBottom As String = ""
            If Me.PrintData.PrintTrainNums.Count > 0 AndAlso Me.PrintData.PrintTrainNums.Values.First.PrintTrainNumList.Values.Count > 0 Then
                trainNumNameTop = Me.PrintData.PrintTrainNums.Values.First.PrintTrainNumList.First.Value.OfficeName
                If Me.PrintData.PrintTrainNums.Values.First.PrintTrainNumList.Values.Count > 1 Then
                    trainNumNameBottom = Me.PrintData.PrintTrainNums.Values.First.PrintTrainNumList.Values(1).OfficeName
                End If
            End If

            Dim miTrainNumNameTop As String = ""
            Dim miTrainNumNameBottom As String = ""
            If Me.PrintData.HasMoveInsideItem AndAlso
                Me.PrintData.MiDispData.PrintTrainNums.Count > 0 AndAlso
                Me.PrintData.MiDispData.PrintTrainNums.Values.First.PrintTrainNumList.Values.Count > 0 Then
                miTrainNumNameTop = Me.PrintData.MiDispData.PrintTrainNums.Values.First.PrintTrainNumList.First.Value.OfficeName
                If Me.PrintData.MiDispData.PrintTrainNums.Values.First.PrintTrainNumList.Values.Count > 1 Then
                    miTrainNumNameBottom = Me.PrintData.MiDispData.PrintTrainNums.Values.First.PrintTrainNumList.Values(1).OfficeName
                End If
            End If

            For Each stkItm In Me.PrintData.StockList.Values
                rngPasteOffset = rngPageteBase.Offset(RowOffset:=pasteOffset * 10)
                If pasteOffset <> 0 Then
                    rngSingleFlame.Copy(rngPasteOffset)
                End If
                '***********************
                '各油種毎の値を設定する
                '***********************
                '油種名
                rngValueSet = DirectCast(rngPasteOffset(1, 1), Excel.Range)
                rngValueSet.Value = stkItm.OilInfo.OilName
                fntColor = rngValueSet.Font
                fntColor.Color = colorSettings.GetColor(stkItm.OilInfo.OilCode).FontColor
                intrColor = rngValueSet.Interior
                intrColor.Color = colorSettings.GetColor(stkItm.OilInfo.OilCode).BackGroundColor
                ExcelMemoryRelease(fntColor)
                ExcelMemoryRelease(intrColor)
                ExcelMemoryRelease(rngValueSet)
                '総容量
                rngValueSet = DirectCast(rngPasteOffset(2, 2), Excel.Range)
                rngValueSet.Value = stkItm.OilInfo.MaxTankCap
                ExcelMemoryRelease(rngValueSet)
                'D/S
                rngValueSet = DirectCast(rngPasteOffset(3, 2), Excel.Range)
                rngValueSet.Value = stkItm.OilInfo.DS
                ExcelMemoryRelease(rngValueSet)
                ''出荷可能 → 総容量 - D/S テンプレートの数式設定の為不要
                'rngValueSet = DirectCast(rngPasteOffset(4, 2), Excel.Range)
                'rngValueSet.Value = stkItm.OilInfo.MaxTankCap
                'ExcelMemoryRelease(rngValueSet)
                '目標在庫
                rngValueSet = DirectCast(rngPasteOffset(5, 2), Excel.Range)
                rngValueSet.Value = stkItm.TargetStock
                ExcelMemoryRelease(rngValueSet)
                '平均積高
                rngValueSet = DirectCast(rngPasteOffset(9, 2), Excel.Range)
                rngValueSet.Value = stkItm.OilInfo.PrintStockAmountAverage 'ここ不明なので一旦前週
                ExcelMemoryRelease(rngValueSet)
                '車数列ヘッダー上
                rngValueSet = DirectCast(rngPasteOffset(7, 4), Excel.Range)
                rngValueSet.Value = trainNumNameTop
                ExcelMemoryRelease(rngValueSet)
                '車数列ヘッダー下
                rngValueSet = DirectCast(rngPasteOffset(8, 4), Excel.Range)
                rngValueSet.Value = trainNumNameBottom
                ExcelMemoryRelease(rngValueSet)

                '最終行まで到達時の処理
                If lastOilCode = stkItm.OilInfo.OilCode Then
                    '先頭行と最終行の保持
                    firstRowNum = rngPageteBase.Row
                    rngValueSet = DirectCast(rngPasteOffset(10, 2), Excel.Range)
                    lastRowNum = rngValueSet.Row
                    ExcelMemoryRelease(rngValueSet)
                    '最左列に油槽所名を縦書きで設定
                    rngValueSet = Me.ExcelWorkSheet.Range(String.Format("B{0}:B{1}", firstRowNum, lastRowNum))
                    rngValueSet.Value = Me.PrintData.ConsigneeName
                    rngValueSet.MergeCells = True
                    rngValueSet.Orientation = Excel.XlOrientation.xlVertical
                    rngValueSet.BorderAround2(Excel.XlLineStyle.xlContinuous,
                                              Excel.XlBorderWeight.xlMedium)
                    rngValueSet.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    ExcelMemoryRelease(rngValueSet)
                    '最下部の罫線を太く
                    Dim bdrs As Excel.Borders
                    Dim bdrBtm As Excel.Border
                    bdrs = rngPasteOffset.Borders
                    bdrBtm = bdrs(Excel.XlBordersIndex.xlEdgeBottom)
                    bdrBtm.Weight = Excel.XlBorderWeight.xlMedium
                    bdrBtm.LineStyle = Excel.XlLineStyle.xlContinuous
                    ExcelMemoryRelease(bdrBtm)
                    ExcelMemoryRelease(bdrs)
                End If

                pasteOffset = pasteOffset + 1
                ExcelMemoryRelease(rngPasteOffset)
            Next
            '構内取りが無ければ油種の設定はしない
            If Me.PrintData.HasMoveInsideItem = False Then
                Exit Try '油種設定のTry Catchスコープの脱出
            End If
            '構内取り分の油種設定
            Dim pasteOffsetStart As Integer = pasteOffset

            For Each stkItm In Me.PrintData.MiDispData.StockList.Values
                rngPasteOffset = rngPageteBase.Offset(RowOffset:=(pasteOffset * 10) + (footerRowNum))
                If pasteOffsetStart = pasteOffset Then
                    miFirstRowNum = rngPasteOffset.Row
                End If
                rngSingleFlame.Copy(rngPasteOffset)

                '***********************
                '各油種毎の値を設定する
                '***********************
                '油種名
                rngValueSet = DirectCast(rngPasteOffset(1, 1), Excel.Range)
                rngValueSet.Value = stkItm.OilInfo.OilName
                fntColor = rngValueSet.Font
                fntColor.Color = colorSettings.GetColor(stkItm.OilInfo.OilCode).FontColor
                intrColor = rngValueSet.Interior
                intrColor.Color = colorSettings.GetColor(stkItm.OilInfo.OilCode).BackGroundColor
                ExcelMemoryRelease(fntColor)
                ExcelMemoryRelease(intrColor)
                ExcelMemoryRelease(rngValueSet)
                '総容量
                rngValueSet = DirectCast(rngPasteOffset(2, 2), Excel.Range)
                rngValueSet.Value = stkItm.OilInfo.MaxTankCap
                ExcelMemoryRelease(rngValueSet)
                'D/S
                rngValueSet = DirectCast(rngPasteOffset(3, 2), Excel.Range)
                rngValueSet.Value = stkItm.OilInfo.DS
                ExcelMemoryRelease(rngValueSet)
                ''出荷可能 → 総容量 - D/S テンプレートの数式設定の為不要
                'rngValueSet = DirectCast(rngPasteOffset(4, 2), Excel.Range)
                'rngValueSet.Value = stkItm.OilInfo.MaxTankCap
                'ExcelMemoryRelease(rngValueSet)
                '目標在庫
                rngValueSet = DirectCast(rngPasteOffset(5, 2), Excel.Range)
                rngValueSet.Value = stkItm.TargetStock
                ExcelMemoryRelease(rngValueSet)
                '平均積高
                rngValueSet = DirectCast(rngPasteOffset(9, 2), Excel.Range)
                rngValueSet.Value = stkItm.OilInfo.PrintStockAmountAverage 'ここ不明なので一旦前週
                ExcelMemoryRelease(rngValueSet)
                '車数列ヘッダー上
                rngValueSet = DirectCast(rngPasteOffset(7, 4), Excel.Range)
                rngValueSet.Value = miTrainNumNameTop
                ExcelMemoryRelease(rngValueSet)
                '車数列ヘッダー下
                rngValueSet = DirectCast(rngPasteOffset(8, 4), Excel.Range)
                rngValueSet.Value = miTrainNumNameBottom
                ExcelMemoryRelease(rngValueSet)
                '最終行まで到達時の処理
                If lastOilCode = stkItm.OilInfo.OilCode Then
                    '先頭行と最終行の保持

                    rngValueSet = DirectCast(rngPasteOffset(10, 2), Excel.Range)
                    miLastRowNum = rngValueSet.Row
                    ExcelMemoryRelease(rngValueSet)
                    '最左列に油槽所名を縦書きで設定
                    rngValueSet = Me.ExcelWorkSheet.Range(String.Format("B{0}:B{1}", miFirstRowNum, miLastRowNum))
                    rngValueSet.Value = Me.PrintData.MiConsigneeName
                    rngValueSet.MergeCells = True
                    ExcelMemoryRelease(intrColor)

                    rngValueSet.Orientation = Excel.XlOrientation.xlVertical
                    rngValueSet.BorderAround2(Excel.XlLineStyle.xlContinuous,
                                              Excel.XlBorderWeight.xlMedium)
                    rngValueSet.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter

                    ExcelMemoryRelease(rngValueSet)
                    '最下部の罫線を太く
                    Dim bdrs As Excel.Borders
                    Dim bdrBtm As Excel.Border
                    bdrs = rngPasteOffset.Borders
                    bdrBtm = bdrs(Excel.XlBordersIndex.xlEdgeBottom)
                    bdrBtm.Weight = Excel.XlBorderWeight.xlMedium
                    bdrBtm.LineStyle = Excel.XlLineStyle.xlContinuous
                    ExcelMemoryRelease(bdrBtm)
                    ExcelMemoryRelease(bdrs)
                End If

                pasteOffset = pasteOffset + 1
                ExcelMemoryRelease(rngPasteOffset)

            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngPasteOffset)
            ExcelMemoryRelease(rngSingleFlame)
            ExcelMemoryRelease(rngPageteBase)
            ExcelMemoryRelease(rngFooterRow)
            ExcelMemoryRelease(rngFooter)
        End Try
        '******************************
        '日付の列拡張
        '******************************
        Dim dataColString = "G"
        Dim rngColumns As Excel.Range = Nothing
        Dim rngTmpPasteStartColumn As Excel.Range = Nothing
        Dim rngPasteTargetColumns As Excel.Range = Nothing
        Dim rngDateSet As Excel.Range = Nothing
        Try
            rngColumns = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", dataColString))
            rngTmpPasteStartColumn = rngColumns.Offset(ColumnOffset:=1)
            rngPasteTargetColumns = rngTmpPasteStartColumn.Resize(ColumnSize:=Me.PrintData.StockDate.Count - 1)
            rngColumns.Copy(rngPasteTargetColumns)

            ''最下部の罫線を太く
            Dim rowsItems = {New With {.startRow = 2, .lastRow = lastRowNum},
                            New With {.startRow = miFirstRowNum, .lastRow = miLastRowNum}}
            For Each rowsItem In rowsItems
                If rowsItem.lastRow = 0 Then
                    Continue For
                End If
                Dim rngRow As Excel.Range
                rngRow = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", rowsItem.startRow, rowsItem.lastRow))
                Dim rngBorderArea As Excel.Range
                rngBorderArea = Me.ExcelAppObj.Intersect(rngPasteTargetColumns, rngRow)

                Dim bdrs As Excel.Borders
                Dim bdrRgt As Excel.Border
                bdrs = rngBorderArea.Borders
                bdrRgt = bdrs(Excel.XlBordersIndex.xlEdgeRight)
                bdrRgt.Weight = Excel.XlBorderWeight.xlMedium
                bdrRgt.LineStyle = Excel.XlLineStyle.xlContinuous
                ExcelMemoryRelease(bdrRgt)
                ExcelMemoryRelease(bdrs)
                ExcelMemoryRelease(rngBorderArea)
                ExcelMemoryRelease(rngRow)
            Next


            '最左に日付を設定※他はExcelテンプレートにて左隣の日付 + 1日となる
            rngDateSet = DirectCast(rngColumns(3, 1), Excel.Range)
            rngDateSet.Value = Me.PrintData.StockDate.Values.First.ItemDate
            ExcelMemoryRelease(rngDateSet)
            '休日の印を付ける
            rngDateSet = DirectCast(rngColumns(2, 1), Excel.Range)
            Dim rngHollidaySet As Excel.Range = Nothing
            rngHollidaySet = rngDateSet.Resize(ColumnSize:=Me.PrintData.StockDate.Count)
            Dim holydaysArray = (From dayItm In Me.PrintData.StockDate.Values Select If(dayItm.IsHoliday OrElse dayItm.WeekNum = "0", "1", ""))
            rngHollidaySet.Value = holydaysArray.ToArray
            ExcelMemoryRelease(rngHollidaySet)
            ExcelMemoryRelease(rngDateSet)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngTmpPasteStartColumn)
            ExcelMemoryRelease(rngPasteTargetColumns)
            ExcelMemoryRelease(rngColumns)
        End Try
        '******************************
        '表フッターの設定（構内取りが在ればその上）
        '******************************
        Try
            rngFooter = Me.ExcelTempSheet.Range("RNG_CONSIGNEEFOOTER")
            Dim rngPasteFooter As Excel.Range
            rngPasteFooter = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", lastRowNum + 1, lastRowNum + footerRowNum))
            rngFooter.Copy(rngPasteFooter)
            ExcelMemoryRelease(rngFooter)
            ExcelMemoryRelease(rngPasteFooter)
        Catch ex As Exception
            ExcelMemoryRelease(rngFooter)
        End Try
        retPosInfo.FirstRowNum = firstRowNum
        retPosInfo.LastRowNum = lastRowNum
        retPosInfo.MiFirstRowNum = miFirstRowNum
        retPosInfo.MiLastRowNum = miLastRowNum
        '******************************
        '過去実績の列を非表示
        '******************************
        Dim rngHiddenColumns As Excel.Range = Nothing
        Dim rngHiddenColumnObj As Excel.Range = Nothing
        Try
            'From入力値-7日分は過去平均の為に設定している為当該列非表示
            rngHiddenColumns = Me.ExcelWorkSheet.Range("G:M")
            rngHiddenColumnObj = rngHiddenColumns.Columns
            rngHiddenColumnObj.Hidden = True
            ExcelMemoryRelease(rngHiddenColumnObj)
            ExcelMemoryRelease(rngHiddenColumns)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHiddenColumnObj)
            ExcelMemoryRelease(rngHiddenColumns)
        End Try

        Return retPosInfo
    End Function
    ''' <summary>
    ''' 一覧表の数字部分の設定
    ''' </summary>
    ''' <param name="posInfo"></param>
    Private Sub EditNumberArea(posInfo As ExcelPositions)
        Dim rngAllCell As Excel.Range = Nothing
        Dim rngBasePasteArea As Excel.Range = Nothing
        Dim rngPasteArea As Excel.Range = Nothing
        Try
            Dim targetDataList As New List(Of OIT0004OilStockCreate.DispDataClass)
            targetDataList.Add(Me.PrintData)
            If Me.PrintData.HasMoveInsideItem Then
                targetDataList.Add(Me.PrintData.MiDispData)
            End If
            Dim loopCnt As Integer = 0
            For Each prnItm In targetDataList
                '一括貼り付け用の領域定義
                '在庫部分
                Dim morningStock(,) As Object '初日のみ設定他は数式の為1セル
                Dim pastDaysCnt As Integer = 0 '朝在庫保持日数
                Dim qPastDaysCnt = (From daysItm In prnItm.StockDate.Values Where daysItm.IsPastDay)
                If qPastDaysCnt.Any Then
                    pastDaysCnt = qPastDaysCnt.Count - 1
                End If
                ReDim morningStock(0, pastDaysCnt)
                Dim ukrireHaraiDasiNums As Object(,) '受入払出
                Dim hoyuNums As Object(,) '保有日数

                '車数部分
                Dim syaSu As Object(,)
                'ローリー部分
                Dim lorryNum As Object(,)
                Dim oilCnt As Integer = 0
                Dim daysMax = prnItm.StockDate.Count - 1
                rngAllCell = Me.ExcelWorkSheet.Cells
                Dim firstRowNum As Integer = posInfo.FirstRowNum
                If loopCnt = 1 Then
                    firstRowNum = posInfo.MiFirstRowNum
                End If
                Dim rngStartCell As Excel.Range = DirectCast(rngAllCell(firstRowNum, 7), Excel.Range)
                Dim rngEndCell As Excel.Range = DirectCast(rngAllCell(firstRowNum + 10 - 1, 7 + daysMax), Excel.Range)
                rngBasePasteArea = Me.ExcelWorkSheet.Range(rngStartCell, rngEndCell)
                ExcelMemoryRelease(rngStartCell)
                ExcelMemoryRelease(rngEndCell)
                ExcelMemoryRelease(rngAllCell)
                'ExcelMemoryRelease(rngAllCell)
                '通常部油種別ループ
                For Each oilItem In prnItm.StockList.Values
                    ReDim ukrireHaraiDasiNums(1, daysMax)
                    ReDim hoyuNums(0, daysMax)
                    ReDim syaSu(1, daysMax)
                    ReDim lorryNum(0, daysMax)
                    Dim trainNumList As OIT0004OilStockCreate.PrintTrainNumCollection = Nothing
                    If prnItm.PrintTrainNums.ContainsKey(oilItem.OilInfo.OilCode) Then
                        trainNumList = prnItm.PrintTrainNums(oilItem.OilInfo.OilCode)
                    Else
                        trainNumList = Nothing
                    End If
                    '日付別ループ
                    Dim daysCnt As Integer = 0
                    For Each dateItem In oilItem.StockItemList.Values

                        If daysCnt <= pastDaysCnt Then
                            morningStock(0, daysCnt) = CDec(dateItem.MorningStock)
                        End If
                        '受入数
                        ukrireHaraiDasiNums(0, daysCnt) = CDec(dateItem.Receive)
                        '払出数
                        ukrireHaraiDasiNums(1, daysCnt) = CDec(dateItem.Send)
                        '保有日数
                        If daysCnt <= 6 Then
                            '緑のエラーポップマークが付くので無意味ですが同じ数式
                            hoyuNums(0, daysCnt) = "=IFERROR(R[-4]C/(SUMIFS(R[-2]C[-6]:R[-2]C,R4C[-6]:R4C,""<>(日)"") / 6),"""")" '''"="""""
                        Else
                            '出荷可能数量 / ([日曜を含まない当日含む払出数] / 6) ※エラーの場合ブランク
                            '上記コメント通りの数式を設定
                            hoyuNums(0, daysCnt) = "=IFERROR(R[-4]C/(SUMIFS(R[-2]C[-6]:R[-2]C,R4C[-6]:R4C,""<>(日)"") / 6),"""")"
                        End If
                        'hoyuNums(0, daysCnt) = dateItem.Retentiondays
                        If trainNumList IsNot Nothing AndAlso trainNumList.PrintTrainNumList.Count > 0 Then
                            syaSu(0, daysCnt) = trainNumList.PrintTrainNumList.Values(0).PrintTrainItems.Values(daysCnt).TrainNum
                            If trainNumList.PrintTrainNumList.Count > 1 Then
                                syaSu(1, daysCnt) = trainNumList.PrintTrainNumList.Values(1).PrintTrainItems.Values(daysCnt).TrainNum
                            Else
                                syaSu(1, daysCnt) = ""
                            End If
                        Else
                            syaSu(0, daysCnt) = ""
                            syaSu(1, daysCnt) = ""
                        End If
                        'ローリー受入

                        lorryNum(0, daysCnt) = CDec(dateItem.ReceiveFromLorry)
                        daysCnt = daysCnt + 1
                    Next dateItem
                    'Excelに貼り付け
                    rngPasteArea = rngBasePasteArea.Offset(RowOffset:=10 * oilCnt)
                    Dim rngRowsObj As Excel.Range = Nothing
                    Dim rngPasteRow As Excel.Range = Nothing
                    Dim rngPasteResized As Excel.Range = Nothing
                    Try
                        '朝在庫
                        rngPasteRow = DirectCast(rngPasteArea(1, 1), Excel.Range)
                        rngPasteResized = rngPasteRow.Resize(ColumnSize:=pastDaysCnt + 1)
                        rngPasteResized.Value = morningStock
                        ExcelMemoryRelease(rngPasteResized)
                        ExcelMemoryRelease(rngPasteRow)
                        '受入払出
                        rngRowsObj = rngPasteArea.Rows
                        rngPasteRow = DirectCast(rngRowsObj("3:4"), Excel.Range)
                        rngPasteRow.Value = ukrireHaraiDasiNums
                        ExcelMemoryRelease(rngPasteRow)
                        '保有日数
                        'targetRowObj = DirectCast(rngPasteArea.Rows("6:6"), Excel.Range)
                        rngPasteRow = DirectCast(rngRowsObj("6:6"), Excel.Range)
                        rngPasteRow.FormulaR1C1 = hoyuNums
                        ExcelMemoryRelease(rngPasteRow)
                        '車数
                        rngPasteRow = DirectCast(rngRowsObj("7:8"), Excel.Range)
                        rngPasteRow.Value = syaSu
                        ExcelMemoryRelease(rngPasteRow)
                        'ローリー受入数
                        rngPasteRow = DirectCast(rngRowsObj("10:10"), Excel.Range)
                        rngPasteRow.Value = lorryNum
                        ExcelMemoryRelease(rngPasteRow)
                        'Rowsオブジェクトの解放
                        ExcelMemoryRelease(rngRowsObj)
                    Catch ex As Exception
                        Throw
                    Finally
                        ExcelMemoryRelease(rngPasteResized)
                        ExcelMemoryRelease(rngPasteRow)
                        ExcelMemoryRelease(rngPasteArea)
                    End Try
                    oilCnt = oilCnt + 1
                    ExcelMemoryRelease(rngPasteArea)
                Next oilItem
                ExcelMemoryRelease(rngBasePasteArea)
                ExcelMemoryRelease(rngAllCell)
                loopCnt = loopCnt + 1
            Next prnItm
            ExcelMemoryRelease(rngBasePasteArea)
            ExcelMemoryRelease(rngAllCell)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngBasePasteArea)
            ExcelMemoryRelease(rngAllCell)
        End Try

    End Sub
    ''' <summary>
    ''' 帳票色設定クラス
    ''' </summary>
    Private Class OilTypeColorSettings
        Private ColorSettings As Dictionary(Of String, OilTypeColorSetting)
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            Me.ColorSettings = New Dictionary(Of String, OilTypeColorSetting)
            'ハイオク
            Me.ColorSettings.Add("1001", New OilTypeColorSetting(RGB(255, 255, 0), RGB(20, 23, 26)))
            'レギュラー
            Me.ColorSettings.Add("1101", New OilTypeColorSetting(RGB(255, 192, 0), RGB(255, 255, 255)))
            '灯油
            Me.ColorSettings.Add("1301", New OilTypeColorSetting(RGB(255, 255, 255), RGB(20, 23, 26)))
            '未添加灯油
            Me.ColorSettings.Add("1302", New OilTypeColorSetting(RGB(221, 245, 253), RGB(20, 23, 26)))
            '軽油
            Me.ColorSettings.Add("1401", New OilTypeColorSetting(RGB(0, 176, 80), RGB(255, 255, 255)))
            '3号軽油
            Me.ColorSettings.Add("1404", New OilTypeColorSetting(RGB(146, 208, 80), RGB(255, 255, 255)))
            'A重油
            Me.ColorSettings.Add("2101", New OilTypeColorSetting(RGB(0, 112, 192), RGB(255, 255, 255)))
            'LSA
            Me.ColorSettings.Add("2201", New OilTypeColorSetting(RGB(0, 176, 240), RGB(255, 255, 255)))
        End Sub
        ''' <summary>
        ''' 油種別の色情報取得
        ''' </summary>
        ''' <param name="oilTypeCode"></param>
        ''' <returns></returns>
        Public Function GetColor(oilTypeCode As String) As OilTypeColorSetting
            If Me.ColorSettings.ContainsKey(oilTypeCode) Then
                Return Me.ColorSettings(oilTypeCode)
            Else
                Return New OilTypeColorSetting(RGB(51, 152, 109), RGB(255, 255, 255))
            End If
        End Function

    End Class
    ''' <summary>
    ''' 色設定クラス
    ''' </summary>
    Private Class OilTypeColorSetting
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New(backGroundColor As Integer, fontColor As Integer)
            Me.BackGroundColor = backGroundColor
            Me.FontColor = fontColor
        End Sub

        ''' <summary>
        ''' 背景色
        ''' </summary>
        ''' <returns></returns>
        Public Property BackGroundColor As Integer
        ''' <summary>
        ''' 文字色
        ''' </summary>
        ''' <returns></returns>
        Public Property FontColor As Integer
    End Class
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
    ''' <summary>
    ''' エクセルの座標保持クラス
    ''' </summary>
    Private Class ExcelPositions
        ''' <summary>
        ''' 通常の開始位置
        ''' </summary>
        ''' <returns></returns>
        Public Property FirstRowNum As Integer
        ''' <summary>
        ''' 通常の終了位置
        ''' </summary>
        ''' <returns></returns>
        Public Property LastRowNum As Integer
        ''' <summary>
        ''' 構内取り開始位置
        ''' </summary>
        ''' <returns></returns>
        Public Property MiFirstRowNum As Integer
        ''' <summary>
        ''' 構内取り終了位置
        ''' </summary>
        ''' <returns></returns>
        Public Property MiLastRowNum As Integer

    End Class
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
            ExcelProcEnd()
        End If
        disposedValue = True
    End Sub
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

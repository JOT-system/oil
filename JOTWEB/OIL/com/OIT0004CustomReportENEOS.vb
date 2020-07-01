Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 在庫管理表ENEOS個別帳票作成クラス
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）</remarks>
Public Class OIT0004CustomReportENEOS : Implements IDisposable
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
    ''' 休日設定シート
    ''' </summary>
    Private ExcelHolidaysSheet As Excel.Worksheet

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
    Private HokushinData As OIT0004OilStockCreate.DispDataClass
    Private KouhuData As OIT0004OilStockCreate.DispDataClass
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
    ''' <param name="hokushinData"></param>
    ''' <param name="kouhuData"></param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, hokushinData As OIT0004OilStockCreate.DispDataClass, kouhuData As OIT0004OilStockCreate.DispDataClass)
        Dim CS0050SESSION As New CS0050SESSION
        Me.HokushinData = hokushinData
        Me.KouhuData = kouhuData
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
        Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("ENEOS報告用"), Excel.Worksheet)
        Me.ExcelHolidaysSheet = DirectCast(Me.ExcelWorkSheets("祝日リスト"), Excel.Worksheet)
        'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
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
            '* 祝祭日の設定
            SetHolidaysSheet()
            ' 値の設定
            EditValuesArea()

            '* 油種（行）、日付（列）を元に雛形の罫線を拡張し体裁を整える
            'Dim posInfo As ExcelPositions = ExtentDisplayFormat()
            ''* 数値埋め処理
            'EditNumberArea(posInfo)
            '***** 生成処理群ここまで *****
            '休日シート非表示
            Me.ExcelHolidaysSheet.Visible = Excel.XlSheetVisibility.xlSheetHidden

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
    ''' 祝祭日の設定
    ''' </summary>
    Private Sub SetHolidaysSheet()
        Dim holidaysList = (From dateItm In Me.KouhuData.StockDate.Values
                            Where dateItm.IsHoliday
                            Select dateItm).ToList
        '範囲に祝祭日が存在しない場合は何もせず終了
        If holidaysList.Count = 0 Then
            Return
        End If
        '祝祭日の情報を貼り付け
        Dim pasteValue(holidaysList.Count - 1, 1) As Object
        Dim loopCnt As Integer = 0
        For Each holidayItm In holidaysList
            pasteValue(loopCnt, 0) = holidayItm.ItemDate
            pasteValue(loopCnt, 1) = holidayItm.HolidayName
            loopCnt = loopCnt + 1
        Next

        Dim rngPaste As Excel.Range = Nothing
        Try
            rngPaste = Me.ExcelHolidaysSheet.Range(String.Format("A1:B{0}", holidaysList.Count))
            rngPaste.Value = pasteValue
            ExcelMemoryRelease(rngPaste)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngPaste)
        End Try

    End Sub
    ''' <summary>
    ''' 値の設定を埋める
    ''' </summary>
    Private Sub EditValuesArea()
        Dim rngDateStart As Excel.Range = Nothing
        'Dim rowSettings As New Dictionary(Of String, Dictionary(Of String, String))
        Dim rowSettings As New Dictionary(Of String, String) From
               {{"RNG_{0}_HG_ROWNAME", "1001"},
                {"RNG_{0}_RG_ROWNAME", "1101"},
                {"RNG_{0}_KE_ROWNAME", "1302"},
                {"RNG_{0}_GO_ROWNAME", "1401"},
                {"RNG_{0}_3GO_ROWNAME", "1404"},
                {"RNG_{0}_A_ROWNAME", "2101"},
                {"RNG_{0}_LSA_ROWNAME", "2201"}}

        Try
            '起点年月日の設定、他の日付はExcel数式で+1日・・・と設定しているので不要
            rngDateStart = Me.ExcelWorkSheet.Range("RNG_FIRSTDATE")
            rngDateStart.Value = Me.HokushinData.StockDate.Values.First.ItemDate
            For Each printObj In {Me.HokushinData, Me.KouhuData}
                If printObj.Consignee = "10" Then

                End If
            Next printObj
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDateStart)
        End Try
    End Sub
    ''' <summary>
    ''' 一覧表の数字部分の設定
    ''' </summary>
    ''' <param name="posInfo"></param>
    Private Sub EditNumberArea(posInfo As ExcelPositions)
        'Dim rngAllCell As Excel.Range = Nothing
        'Dim rngBasePasteArea As Excel.Range = Nothing
        'Dim rngPasteArea As Excel.Range = Nothing
        'Try
        '    Dim targetDataList As New List(Of OIT0004OilStockCreate.DispDataClass)
        '    targetDataList.Add(Me.PrintData)
        '    If Me.PrintData.HasMoveInsideItem Then
        '        targetDataList.Add(Me.PrintData.MiDispData)
        '    End If
        '    Dim loopCnt As Integer = 0
        '    For Each prnItm In targetDataList
        '        '一括貼り付け用の領域定義
        '        '在庫部分
        '        Dim morningStock(,) As Object '初日のみ設定他は数式の為1セル
        '        Dim pastDaysCnt As Integer = 0 '朝在庫保持日数
        '        Dim qPastDaysCnt = (From daysItm In prnItm.StockDate.Values Where daysItm.IsPastDay)
        '        If qPastDaysCnt.Any Then
        '            pastDaysCnt = qPastDaysCnt.Count - 1
        '        End If
        '        ReDim morningStock(0, pastDaysCnt)
        '        Dim ukrireHaraiDasiNums As Object(,) '受入払出
        '        Dim hoyuNums As Object(,) '保有日数

        '        '車数部分
        '        Dim syaSu As Object(,)
        '        'ローリー部分
        '        Dim lorryNum As Object(,)
        '        Dim oilCnt As Integer = 0
        '        Dim daysMax = prnItm.StockDate.Count - 1
        '        rngAllCell = Me.ExcelWorkSheet.Cells
        '        Dim firstRowNum As Integer = posInfo.FirstRowNum
        '        If loopCnt = 1 Then
        '            firstRowNum = posInfo.MiFirstRowNum
        '        End If
        '        Dim rngStartCell As Excel.Range = DirectCast(rngAllCell(firstRowNum, 7), Excel.Range)
        '        Dim rngEndCell As Excel.Range = DirectCast(rngAllCell(firstRowNum + 10 - 1, 7 + daysMax), Excel.Range)
        '        rngBasePasteArea = Me.ExcelWorkSheet.Range(rngStartCell, rngEndCell)
        '        ExcelMemoryRelease(rngStartCell)
        '        ExcelMemoryRelease(rngEndCell)
        '        ExcelMemoryRelease(rngAllCell)
        '        'ExcelMemoryRelease(rngAllCell)
        '        '通常部油種別ループ
        '        For Each oilItem In prnItm.StockList.Values
        '            ReDim ukrireHaraiDasiNums(1, daysMax)
        '            ReDim hoyuNums(0, daysMax)
        '            ReDim syaSu(1, daysMax)
        '            ReDim lorryNum(0, daysMax)
        '            Dim trainNumList As OIT0004OilStockCreate.PrintTrainNumCollection = Nothing
        '            If prnItm.PrintTrainNums.ContainsKey(oilItem.OilInfo.OilCode) Then
        '                trainNumList = prnItm.PrintTrainNums(oilItem.OilInfo.OilCode)
        '            Else
        '                trainNumList = Nothing
        '            End If
        '            '日付別ループ
        '            Dim daysCnt As Integer = 0
        '            For Each dateItem In oilItem.StockItemList.Values

        '                If daysCnt <= pastDaysCnt Then
        '                    morningStock(0, daysCnt) = CDec(dateItem.MorningStock)
        '                End If
        '                '受入数
        '                ukrireHaraiDasiNums(0, daysCnt) = CDec(dateItem.Receive)
        '                '払出数
        '                ukrireHaraiDasiNums(1, daysCnt) = CDec(dateItem.Send)
        '                '保有日数
        '                If daysCnt <= 6 Then
        '                    '緑のエラーポップマークが付くので無意味ですが同じ数式
        '                    hoyuNums(0, daysCnt) = "=IFERROR(R[-4]C/(SUMIFS(R[-2]C[-6]:R[-2]C,R4C[-6]:R4C,""<>(日)"") / 6),"""")" '''"="""""
        '                Else
        '                    '出荷可能数量 / ([日曜を含まない当日含む払出数] / 6) ※エラーの場合ブランク
        '                    '上記コメント通りの数式を設定
        '                    hoyuNums(0, daysCnt) = "=IFERROR(R[-4]C/(SUMIFS(R[-2]C[-6]:R[-2]C,R4C[-6]:R4C,""<>(日)"") / 6),"""")"
        '                End If
        '                'hoyuNums(0, daysCnt) = dateItem.Retentiondays
        '                If trainNumList IsNot Nothing AndAlso trainNumList.PrintTrainNumList.Count > 0 Then
        '                    syaSu(0, daysCnt) = trainNumList.PrintTrainNumList.Values(0).PrintTrainItems.Values(daysCnt).TrainNum
        '                    If trainNumList.PrintTrainNumList.Count > 1 Then
        '                        syaSu(1, daysCnt) = trainNumList.PrintTrainNumList.Values(1).PrintTrainItems.Values(daysCnt).TrainNum
        '                    Else
        '                        syaSu(1, daysCnt) = ""
        '                    End If
        '                Else
        '                    syaSu(0, daysCnt) = ""
        '                    syaSu(1, daysCnt) = ""
        '                End If
        '                'ローリー受入

        '                lorryNum(0, daysCnt) = CDec(dateItem.ReceiveFromLorry)
        '                daysCnt = daysCnt + 1
        '            Next dateItem
        '            'Excelに貼り付け
        '            rngPasteArea = rngBasePasteArea.Offset(RowOffset:=10 * oilCnt)
        '            Dim rngRowsObj As Excel.Range = Nothing
        '            Dim rngPasteRow As Excel.Range = Nothing
        '            Dim rngPasteResized As Excel.Range = Nothing
        '            Try
        '                '朝在庫
        '                rngPasteRow = DirectCast(rngPasteArea(1, 1), Excel.Range)
        '                rngPasteResized = rngPasteRow.Resize(ColumnSize:=pastDaysCnt + 1)
        '                rngPasteResized.Value = morningStock
        '                ExcelMemoryRelease(rngPasteResized)
        '                ExcelMemoryRelease(rngPasteRow)
        '                '受入払出
        '                rngRowsObj = rngPasteArea.Rows
        '                rngPasteRow = DirectCast(rngRowsObj("3:4"), Excel.Range)
        '                rngPasteRow.Value = ukrireHaraiDasiNums
        '                ExcelMemoryRelease(rngPasteRow)
        '                '保有日数
        '                'targetRowObj = DirectCast(rngPasteArea.Rows("6:6"), Excel.Range)
        '                rngPasteRow = DirectCast(rngRowsObj("6:6"), Excel.Range)
        '                rngPasteRow.FormulaR1C1 = hoyuNums
        '                ExcelMemoryRelease(rngPasteRow)
        '                '車数
        '                rngPasteRow = DirectCast(rngRowsObj("7:8"), Excel.Range)
        '                rngPasteRow.Value = lorryNum
        '                ExcelMemoryRelease(rngPasteRow)
        '                '車数
        '                rngPasteRow = DirectCast(rngRowsObj("10:10"), Excel.Range)
        '                rngPasteRow.Value = lorryNum
        '                ExcelMemoryRelease(rngPasteRow)
        '                'Rowsオブジェクトの解放
        '                ExcelMemoryRelease(rngRowsObj)
        '            Catch ex As Exception
        '                Throw
        '            Finally
        '                ExcelMemoryRelease(rngPasteResized)
        '                ExcelMemoryRelease(rngPasteRow)
        '                ExcelMemoryRelease(rngPasteArea)
        '            End Try
        '            oilCnt = oilCnt + 1
        '            ExcelMemoryRelease(rngPasteArea)
        '        Next oilItem
        '        ExcelMemoryRelease(rngBasePasteArea)
        '        ExcelMemoryRelease(rngAllCell)
        '        loopCnt = loopCnt + 1
        '    Next prnItm
        '    ExcelMemoryRelease(rngBasePasteArea)
        '    ExcelMemoryRelease(rngAllCell)
        'Catch ex As Exception
        '    Throw
        'Finally
        '    ExcelMemoryRelease(rngBasePasteArea)
        '    ExcelMemoryRelease(rngAllCell)
        'End Try

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
            'Excel Sheetオブジェクトの解放(休日設定)
            ExcelMemoryRelease(ExcelHolidaysSheet)
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

Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 油槽所在庫管理帳票作成クラス
''' </summary>
''' <remarks>
''' 根岸営業所の北信・甲府油槽所限定の想定
''' </remarks>
Public Class OIT0004CustomReportConsignee : Implements IDisposable
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
    Private xlProcId As Integer
    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As OIT0004OilStockCreate.DispDataClass
    ''' <summary>
    ''' 油種の行マップリスト
    ''' </summary>
    Private RerativeRowMapList As Dictionary(Of String, Integer)
    ''' <summary>
    ''' 列車Noの列マップリスト
    ''' </summary>
    Private RerativeTrainColMapList As Dictionary(Of String, Integer)
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
            Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("入力画面"), Excel.Worksheet)
            Me.RerativeRowMapList = New Dictionary(Of String, Integer)
            With Me.RerativeRowMapList
                .Add("1001", 1) 'ハイオク
                .Add("1101", 2) 'レギュラー
                .Add("1301", 3) '灯油
                .Add("1302", 4) '未添加灯油
                .Add("1401", 5) '軽油
                .Add("1404", 6) '３号軽油
                .Add("2101", 8) 'A重油
                .Add("2201", 9) 'LSA
            End With
            Me.RerativeTrainColMapList = New Dictionary(Of String, Integer)
            If Me.PrintData.Consignee = "10" Then
                '北信の列車列マップ
                With RerativeTrainColMapList
                    .Add("5463", 1)
                    .Add("2085", 2)
                    .Add("8471", 4)
                End With

            Else
                '甲府の列車列マップ
                With RerativeTrainColMapList
                    .Add("81", 1)
                    .Add("83", 2)
                End With
            End If
        Catch ex As Exception
            If Me.xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
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
            'ヘッダーの調整
            EditHeaderArea()
            '車数表の値展開
            EditTrainNumList()
            '下段在庫表
            EditAmountList()
            ''***** 生成処理群ここから *****
            ''* 油種（行）、日付（列）を元に雛形の罫線を拡張し体裁を整える
            'Dim posInfo As ExcelPositions = ExtentDisplayFormat()
            ''* 数値埋め処理
            'EditNumberArea(posInfo)
            ''***** 生成処理群ここまで *****

            'ExcelTempSheet.Delete() '雛形シート削除
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
    ''' ヘッダー部分に値を転記
    ''' </summary>
    Private Sub EditHeaderArea()
        Dim rngTmp As Excel.Range = Nothing
        Dim rngDelete As Excel.Range = Nothing
        Dim colDelete As Excel.Range = Nothing
        Dim rngHideRow As Excel.Range = Nothing
        Dim rowHide As Excel.Range = Nothing
        Try
            '*********************************
            '開始日の設定
            '*********************************
            Dim startDate As String = Me.PrintData.StockDateDisplay.First.Value.ItemDate.ToString("yyyy/MM/dd")
            Dim endDate As String = Me.PrintData.StockDateDisplay.Last.Value.ItemDate.ToString("yyyy/MM/dd")
            rngTmp = Me.ExcelWorkSheet.Range("RNG_START_DATE")
            rngTmp.Value = startDate
            ExcelMemoryRelease(rngTmp)
            Dim rowNumList As List(Of String) = Nothing
            '*********************************
            '日付範囲の削除(6日表示なら7～12日分の枠を削除)
            '*********************************
            If Me.PrintData.StockDateDisplay.Count = 6 Then
                rngDelete = Me.ExcelWorkSheet.Range("AJ:BM")
                colDelete = rngDelete.Columns
                colDelete.Delete()
                ExcelMemoryRelease(colDelete)
                ExcelMemoryRelease(rngDelete)
            End If
            '*********************************
            '三号・軽油切替範囲設定
            '*********************************
            If Me.PrintData.OilTypeList.ContainsKey("1404") Then
                Dim ordDate3goFrom As String = Me.PrintData.OilTypeList("1404").OrderFromDate
                Dim ordDate3goTo As String = Me.PrintData.OilTypeList("1404").OrderToDate
                Dim q3goDateList = (From dispDateItm In Me.PrintData.StockDateDisplay Where dispDateItm.Value.ItemDate.ToString("yyyy/MM/dd") >= ordDate3goFrom AndAlso dispDateItm.Value.ItemDate.ToString("yyyy/MM/dd") <= ordDate3goTo Select dispDateItm.Value.ItemDate.ToString("yyyy/MM/dd"))
                If q3goDateList.Any = False Then
                    '全てが３号期間に含まれない
                    '３号の行を非表示にする
                    rowNumList = New List(Of String) From {"15", "44"}

                ElseIf q3goDateList.Count = Me.PrintData.StockDateDisplay.Count Then
                    '全てが３号期間
                    '軽油の行を非表示
                    rowNumList = New List(Of String) From {"14", "43"}
                End If
                '****************************
                '３号or軽油の対象行の非表示
                '****************************
                For Each rowNum As String In rowNumList
                    rngHideRow = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", rowNum))
                    rowHide = rngHideRow.Rows
                    rowHide.Hidden = True
                    ExcelMemoryRelease(rowHide)
                    ExcelMemoryRelease(rngHideRow)
                Next
                '****************************
                '非表示行に三号期間のフラグを設定
                '(合計の計算に含めない）
                '****************************
                If q3goDateList.Any Then
                    Dim l3goDate = q3goDateList.ToList
                    Dim shiftNum = 0
                    Dim rng3GouFlag As Excel.Range = Nothing
                    Dim rng3GouFlagBase As Excel.Range = Nothing
                    Try
                        rng3GouFlagBase = Me.ExcelWorkSheet.Range("RNG_3GOUFLG")
                        For Each dateItm In Me.PrintData.StockDateDisplay.Values
                            If l3goDate.Contains(dateItm.ItemDate.ToString("yyyy/MM/dd")) Then
                                rng3GouFlag = rng3GouFlagBase.Offset(ColumnOffset:=(shiftNum * 5))
                                rng3GouFlag.Value = 1
                                ExcelMemoryRelease(rng3GouFlag)
                            End If
                            shiftNum = shiftNum + 1
                        Next dateItm
                    Catch ex As Exception
                        Throw
                    Finally

                        ExcelMemoryRelease(rng3GouFlag)
                        ExcelMemoryRelease(rng3GouFlagBase)
                    End Try

                End If
            End If
            '****************************
            '下段表の見出し列の値を格納
            '(タンク容量とD/S）
            '****************************
            Dim maxCapArea As Excel.Range = Nothing
            Dim rngMaxCap As Excel.Range = Nothing
            Dim dsArea As Excel.Range = Nothing
            Dim rngDs As Excel.Range = Nothing
            Try
                maxCapArea = Me.ExcelWorkSheet.Range("RNG_TANKMAX")
                dsArea = Me.ExcelWorkSheet.Range("RNG_DS")
                For Each oilItem In Me.PrintData.OilTypeList.Values
                    If Me.RerativeRowMapList.ContainsKey(oilItem.OilCode) = False Then
                        Continue For
                    End If

                    rngDs = DirectCast(dsArea(Me.RerativeRowMapList(oilItem.OilCode), 1), Excel.Range)
                    rngMaxCap = DirectCast(maxCapArea(Me.RerativeRowMapList(oilItem.OilCode), 1), Excel.Range)
                    rngDs.Value = oilItem.DS
                    rngMaxCap.Value = oilItem.MaxTankCap
                    ExcelMemoryRelease(rngMaxCap)
                    ExcelMemoryRelease(rngDs)
                Next
            Catch ex As Exception
                Throw
            Finally
                ExcelMemoryRelease(rngMaxCap)
                ExcelMemoryRelease(rngDs)

                ExcelMemoryRelease(maxCapArea)
                ExcelMemoryRelease(dsArea)
            End Try

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngTmp)
            ExcelMemoryRelease(colDelete)
            ExcelMemoryRelease(rngDelete)
            ExcelMemoryRelease(rowHide)
            ExcelMemoryRelease(rngHideRow)
        End Try
    End Sub
    ''' <summary>
    ''' 列車数一覧表の数値をマッピング
    ''' </summary>
    Private Sub EditTrainNumList()
        Dim rngBaseArea As Excel.Range = Nothing
        Dim rngCurrentArea As Excel.Range = Nothing
        Dim rngCurrentValue As Excel.Range = Nothing
        Dim rngBaseUkeireArea As Excel.Range = Nothing
        Dim rngCurrentUkeireArea As Excel.Range = Nothing
        Dim rngCurrentUkeireValue As Excel.Range = Nothing
        Try
            rngBaseArea = Me.ExcelWorkSheet.Range("RNG_TRAINNUMAREA")
            rngBaseUkeireArea = Me.ExcelWorkSheet.Range("RNG_UNKOUAREA")
            Dim shiftNum As Integer = 0
            Dim hasAnyValue As Boolean = False
            For Each suggestItm In Me.PrintData.SuggestListDisplay.Values
                '日付の枠を翌日(最初は当日のまま）にシフトする
                rngCurrentArea = rngBaseArea.Offset(ColumnOffset:=shiftNum * 5)
                rngCurrentUkeireArea = rngBaseUkeireArea.Offset(ColumnOffset:=shiftNum * 5)
                '列車別のループ
                For Each trainItm In suggestItm.SuggestOrderItem.Values
                    hasAnyValue = False
                    '帳票に展開すべき列車番号が無い場合はスキップ
                    If Me.RerativeTrainColMapList.ContainsKey(trainItm.TrainInfo.TrainNo) = False Then
                        Continue For
                    End If
                    Dim trainCol = Me.RerativeTrainColMapList(trainItm.TrainInfo.TrainNo)
                    '油種をループし対象の行に値を展開
                    For Each oilItm In trainItm.SuggestValuesItem.Values
                        '帳票に展開すべき油種が無い場合はスキップ
                        If Me.RerativeRowMapList.ContainsKey(oilItm.OilInfo.OilCode) = False Then
                            Continue For
                        End If
                        rngCurrentValue = DirectCast(rngCurrentArea(Me.RerativeRowMapList(oilItm.OilInfo.OilCode), trainCol), Excel.Range)
                        If IsNumeric(oilItm.ItemValue) = False OrElse Not CInt(oilItm.ItemValue) = 0 Then
                            hasAnyValue = True
                        End If

                        If IsNumeric(oilItm.ItemValue) Then
                            rngCurrentValue.Value = CInt(oilItm.ItemValue)
                        End If

                        ExcelMemoryRelease(rngCurrentValue)

                    Next oilItm '油種ループ
                    '運転日設定(どの油種にも車数が無い場合は0）
                    rngCurrentUkeireValue = DirectCast(rngCurrentUkeireArea(1, trainCol), Excel.Range)
                    rngCurrentUkeireValue.Value = 0
                    If hasAnyValue Then
                        Dim accDaysVal = 0
                        If IsNumeric(trainItm.AccAddDays) Then
                            accDaysVal = CInt(trainItm.AccAddDays) + 1
                        Else
                            accDaysVal = CInt(trainItm.TrainInfo.AccDays) + 1
                        End If
                        rngCurrentUkeireValue.Value = accDaysVal
                    End If
                    ExcelMemoryRelease(rngCurrentUkeireValue)

                Next trainItm '列車ループ
                shiftNum = shiftNum + 1
                ExcelMemoryRelease(rngCurrentArea)
                ExcelMemoryRelease(rngCurrentUkeireArea)
            Next suggestItm '日付ループ
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngCurrentValue)
            ExcelMemoryRelease(rngCurrentUkeireValue)
            ExcelMemoryRelease(rngCurrentArea)
            ExcelMemoryRelease(rngCurrentUkeireArea)
            ExcelMemoryRelease(rngBaseArea)
            ExcelMemoryRelease(rngBaseUkeireArea)
        End Try
    End Sub
    ''' <summary>
    ''' 下表の設定
    ''' </summary>
    Private Sub EditAmountList()
        Dim baseArea As Excel.Range = Nothing
        Dim currentArea As Excel.Range = Nothing
        Dim rngVal As Excel.Range = Nothing
        Try
            baseArea = Me.ExcelWorkSheet.Range("RNG_VALUEAREA")
            For Each oilItem In Me.PrintData.StockList.Values
                '展開すべき油種が帳票に無い場合はスキップ
                If Me.RerativeRowMapList.ContainsKey(oilItem.OilInfo.OilCode) = False Then
                    Continue For
                End If

                Dim rowNum As Integer = Me.RerativeRowMapList(oilItem.OilInfo.OilCode)

                Dim shiftVal = 0
                For Each dateItem In oilItem.StockItemListDisplay.Values
                    currentArea = baseArea.Offset(ColumnOffset:=shiftVal * 5)
                    '朝在庫
                    rngVal = DirectCast(currentArea(rowNum, 1), Excel.Range)
                    If IsNumeric(dateItem.MorningStock) Then
                        rngVal.Value = CDec(dateItem.MorningStock)
                    End If

                    ExcelMemoryRelease(rngVal)
                    '払出
                    rngVal = DirectCast(currentArea(rowNum, 2), Excel.Range)
                    If IsNumeric(dateItem.Send) Then
                        rngVal.Value = CDec(dateItem.Send)
                    End If
                    ExcelMemoryRelease(rngVal)
                    '受入
                    rngVal = DirectCast(currentArea(rowNum, 3), Excel.Range)
                    If IsNumeric(dateItem.Receive) Then
                        rngVal.Value = CDec(dateItem.Receive)
                    End If
                    ExcelMemoryRelease(rngVal)

                    ExcelMemoryRelease(currentArea)
                    shiftVal = shiftVal + 1
                Next
                shiftVal = 0

            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngVal)
            ExcelMemoryRelease(currentArea)
            ExcelMemoryRelease(baseArea)
        End Try
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

        ' TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、下の Finalize() をオーバーライドします。
        ' TODO: 大きなフィールドを null に設定します。
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
    'AJから消す
End Class

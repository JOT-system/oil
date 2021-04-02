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
        Try
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
            Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("ENEOS報告用"), Excel.Worksheet)
            Me.ExcelHolidaysSheet = DirectCast(Me.ExcelWorkSheets("祝日リスト"), Excel.Worksheet)
            'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
        Catch ex As Exception
            If Me.xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try

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
               {{"1001", "RNG_{0}_HG_ROWNAME"},
                {"1101", "RNG_{0}_RG_ROWNAME"},
                {"1302", "RNG_{0}_KE_ROWNAME"},
                {"1401", "RNG_{0}_GO_ROWNAME"},
                {"1404", "RNG_{0}_3GO_ROWNAME"},
                {"2101", "RNG_{0}_A_ROWNAME"},
                {"2201", "RNG_{0}_LSA_ROWNAME"}}
        Dim rngPrefix As String = "H"

        Try
            '起点年月日の設定、他の日付はExcel数式で+1日・・・と設定しているので不要
            rngDateStart = Me.ExcelWorkSheet.Range("RNG_FIRSTDATE")
            rngDateStart.Value = Me.HokushinData.StockDate.Values.First.ItemDate
            For Each printObj In {Me.HokushinData, Me.KouhuData}
                If printObj.Consignee <> "10" Then
                    rngPrefix = "K"
                End If
                Dim rngTargetRow As Excel.Range = Nothing
                Dim rngOffset As Excel.Range = Nothing
                Dim rngResize As Excel.Range = Nothing
                Dim rngKeiOffset As Excel.Range = Nothing
                Dim targetRowName As String
                Dim pasteColHeader(0, 4) As Object
                Dim pasteColUkeire(0, 2) As Object
                For Each oilItm In printObj.StockList.Values
                    If rowSettings.ContainsKey(oilItm.OilInfo.OilCode) = False Then
                        Continue For
                    End If
                    targetRowName = rowSettings(oilItm.OilInfo.OilCode)
                    targetRowName = String.Format(targetRowName, rngPrefix)
                    rngTargetRow = Me.ExcelWorkSheet.Range(targetRowName)
                    '列見出し部分の数値格納
                    rngOffset = rngTargetRow.Offset(ColumnOffset:=1)
                    rngResize = rngOffset.Resize(ColumnSize:=5)
                    pasteColHeader(0, 0) = oilItm.TankCapacity '安全容量
                    pasteColHeader(0, 1) = oilItm.DS 'D/S
                    pasteColHeader(0, 2) = oilItm.Stock80  '80%在庫
                    pasteColHeader(0, 3) = oilItm.LastShipmentAve
                    '初日は朝在庫設定
                    pasteColHeader(0, 4) = oilItm.StockItemList.Values.First.MorningStockWithoutDS
                    rngResize.Value = pasteColHeader

                    ExcelMemoryRelease(rngResize)
                    ExcelMemoryRelease(rngOffset)
                    '日付毎のループ
                    Dim loopcnt = 0
                    For Each daysItm In oilItm.StockItemList.Values
                        '受入数量（３カラム分の設定）
                        rngOffset = rngTargetRow.Offset(ColumnOffset:=7 + (7 * loopcnt))
                        rngResize = rngOffset.Resize(ColumnSize:=3)
                        pasteColUkeire(0, 0) = daysItm.Print1stPositionVal
                        pasteColUkeire(0, 1) = daysItm.Print2ndPositionVal
                        pasteColUkeire(0, 2) = daysItm.Print3rdPositionVal
                        rngResize.Value = pasteColUkeire
                        ExcelMemoryRelease(rngResize)
                        ExcelMemoryRelease(rngOffset)
                        '払出数量の設定
                        rngOffset = rngTargetRow.Offset(ColumnOffset:=11 + (7 * loopcnt))
                        rngOffset.Value = CDec(daysItm.Send)
                        ExcelMemoryRelease(rngOffset)
                        '軽油の場合は計算範囲情報を付与
                        If {"1404"}.Contains(oilItm.OilInfo.OilCode) Then
                            rngOffset = rngTargetRow.Offset(RowOffset:=2, ColumnOffset:=6 + (7 * loopcnt))
                            rngResize = rngOffset
                            If oilItm.OilInfo.OrderFromDate <= daysItm.DaysItem.ItemDate.ToString("yyyy/MM/dd") AndAlso
                               oilItm.OilInfo.OrderToDate >= daysItm.DaysItem.ItemDate.ToString("yyyy/MM/dd") Then
                                rngResize.Value = "1"
                            Else
                                rngResize.Value = "0"
                            End If

                            ExcelMemoryRelease(rngResize)
                            ExcelMemoryRelease(rngOffset)
                        End If
                        loopcnt = loopcnt + 1
                    Next
                    ExcelMemoryRelease(rngTargetRow)


                Next
                ExcelMemoryRelease(rngTargetRow)
            Next printObj
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDateStart)
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

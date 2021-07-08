Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 輸送量・輸送費関連帳票作成クラス
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）</remarks>
Public Class OIT0008CustomReport : Implements IDisposable
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

    '輸送費明細
    '1ページ辺りの縦長さ
    Private Const TRANSPORT_COST_DETAIL_1PAGE_VLENGTH As Double = 902.25

    'タンク車輸送実績表
    '1ページ辺りの縦長さ(列車別)
    Private Const TANK_TRANSPORT_RESULT_1PAGE_VLENGTH As Double = 628.5
    '1ページ辺りの縦長さ(着駅別)
    Private Const TANK_TRANSPORT_RESULT_ARR_1PAGE_VLENGTH As Double = 624
    '1ページ辺りの明細数(列車別)
    Private Const TANK_TRANSPORT_RESULT_1PAGE_DETAIL_COUNT As Integer = 42
    '1ページ辺りの明細数(着駅別)
    Private Const TANK_TRANSPORT_RESULT_ARR_1PAGE_DETAIL_COUNT As Integer = 28

    '輸送実績表
    '1ページ辺りの明細数
    Private Const TRANSPORT_RESULT_1PAGE_DETAIL_COUNT As Integer = 48

    Private Const TRANSPORT_RESULT_1PAGE_DETAIL_COUNT_011201 As Integer = 53

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private xlProcId As Integer

    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable)
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
            Dim xlHwnd As IntPtr = CType(Me.ExcelAppObj.Hwnd, IntPtr)
            GetWindowThreadProcessId(xlHwnd, Me.xlProcId)
            'Excelワークブックオブジェクトの生成
            Me.ExcelBooksObj = Me.ExcelAppObj.Workbooks
            Me.ExcelBookObj = Me.ExcelBooksObj.Open(Me.ExcelTemplatePath,
                                                    UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                                    [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
            Me.ExcelWorkSheets = Me.ExcelBookObj.Sheets

            If CONST_TEMPNAME_TRANSPORT_COST_DETAIL.Equals(excelFileName) Then
                '輸送費明細
                Me.ExcelWorkSheet = DirectCast(
                    Me.ExcelWorkSheets(CONST_REPORTNAME_TRANSPORT_COST_DETAIL), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf CONST_TEMPNAME_TANK_TRANSPORT_RESULT.Equals(excelFileName) OrElse
                CONST_TEMPNAME_TANK_TRANSPORT_RESULT_010402.Equals(excelFileName) Then
                'タンク車運賃実績表
                Me.ExcelWorkSheet = DirectCast(
                    Me.ExcelWorkSheets(CONST_REPORTNAME_TANK_TRANSPORT_RESULT), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf CONST_TEMPNAME_TANK_TRANSPORT_RESULT_ARR.Equals(excelFileName) OrElse
                CONST_TEMPNAME_TANK_TRANSPORT_RESULT_ARR_010402.Equals(excelFileName) Then
                'タンク車運賃実績表（着駅別）
                Me.ExcelWorkSheet = DirectCast(
                    Me.ExcelWorkSheets(CONST_REPORTNAME_TANK_TRANSPORT_RESULT_ARR), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf CONST_TEMPNAME_TRANSPORT_RESULT.Equals(excelFileName) OrElse
                CONST_TEMPNAME_TRANSPORT_RESULT_010402.Equals(excelFileName) OrElse
                CONST_TEMPNAME_TRANSPORT_RESULT_011201.Equals(excelFileName) Then
                '輸送実績表
                Me.ExcelWorkSheet = DirectCast(
                    Me.ExcelWorkSheets(CONST_REPORTNAME_TRANSPORT_RESULT), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf CONST_TEMPNAME_FINANCE_COOPERATION_IF.Equals(excelFileName) Then
                '経理連携IF
                Me.ExcelWorkSheet = DirectCast(
                    Me.ExcelWorkSheets(CONST_REPORTNAME_FINANCE_COOPERATION_IF), Excel.Worksheet)
            ElseIf CONST_TEMPNAME_ACCOUNT_BRANCH_BUSINESS_INCOME.Equals(excelFileName) Then
                '科目別一覧
                Me.ExcelWorkSheet = DirectCast(
                        Me.ExcelWorkSheets(CONST_REPORTNAME_ACCOUNT_BRANCH_BUSINESS_INCOME), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            End If
        Catch ex As Exception
            If Me.xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try

    End Sub

#Region "ダウンロード(輸送費明細)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(輸送費明細)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_TransportCostDetail(ByVal KEIJYO_YM As Date) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".pdf"
        'Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            '固定帳票(輸送費明細)作成処理
            Dim lastRow As DataRow = Nothing
            Dim idx As Int32 = 1
            Dim srcRange As Excel.Range = Nothing
            Dim destRange As Excel.Range = Nothing
            Dim PageNum As Int32 = 1
            Dim pixel As Double = 0.0
            Dim row_cnt As Int32 = 0
            Dim nowdate As DateTime = DateTime.Now

            For Each row As DataRow In PrintData.Rows

                row_cnt += 1

                '最終レコードの場合
                If row_cnt = PrintData.Rows.Count Then

                    '〇明細の設定(請求先計)
                    'テンプレート⑩をコピーする
                    srcRange = ExcelTempSheet.Cells.Range("I44:CJ45")
                    destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    ExcelMemoryRelease(srcRange)
                    ExcelMemoryRelease(destRange)
                    '値出力(転送販売/着駅/荷受人/油種をスキップし、請求先を出力)
                    EditTransportCostDetail_DetailArea(idx, row, 8)
                    'ピクセル加算
                    pixel += 18
                    '2行目の高さを調整
                    destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                    destRange.RowHeight = 6
                    ExcelMemoryRelease(destRange)
                    idx += 1
                    pixel += 6

                    '〇改頁処理
                    ChangeTansportCostDetailPage(idx, pixel, 1)

                    Exit For
                End If

                '1行目
                If lastRow Is Nothing Then
                    '◯ヘッダーの設定
                    '値出力
                    EditTransportCostDetail_HeaderArea(idx, row, KEIJYO_YM)
                    'ピクセル加算
                    pixel += 150
                    '◯明細の設定
                    '値出力(全項目)
                    EditTransportCostDetail_DetailArea(idx, row)
                    'ピクセル加算
                    pixel += 18
                Else '2行目以降
                    '前行と輸送形態、請求先会社、請求先部門、出荷場所、荷主、扱支店、荷受人が一致する場合 START
                    If lastRow("TRKBN").ToString().Equals((row("TRKBN").ToString())) AndAlso
                        lastRow("INVOICECODE").ToString().Equals((row("INVOICECODE").ToString())) AndAlso
                        lastRow("INVOICEDEPTNAME").ToString().Equals((row("INVOICEDEPTNAME").ToString())) AndAlso
                        lastRow("BASECODE").ToString().Equals((row("BASECODE").ToString())) AndAlso
                        lastRow("SHIPPERSCODE").ToString().Equals((row("SHIPPERSCODE").ToString())) AndAlso
                        lastRow("MANAGEBRANCHCODE").ToString().Equals((row("MANAGEBRANCHCODE").ToString())) AndAlso
                        lastRow("CONSIGNEECODE").ToString().Equals((row("CONSIGNEECODE").ToString())) Then

                        '現在のレコードが荷受人計の場合
                        If "9999".Equals(row("OILCODE").ToString()) Then

                            '〇明細の設定(荷受人計)
                            'テンプレート③をコピーする
                            srcRange = ExcelTempSheet.Cells.Range("I19:CJ19")
                            destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                            '明細出力(油種のみスキップ)
                            EditTransportCostDetail_DetailArea(idx, row, 2)
                            'ピクセル加算
                            pixel += 18

                            '空行を差し込む
                            destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                            destRange.RowHeight = 6
                            ExcelMemoryRelease(destRange)
                            idx += 1
                            pixel += 6

                            '〇明細の設定(転送販売/着駅計)
                            'テンプレート⑤をコピーする
                            srcRange = ExcelTempSheet.Cells.Range("I23:CJ23")
                            destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                            '値出力(荷受人/油種スキップ)
                            EditTransportCostDetail_DetailArea(idx, row, 3)
                            'ピクセル加算
                            pixel += 18

                            '空行を差し込む
                            destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                            destRange.RowHeight = 6
                            ExcelMemoryRelease(destRange)
                            idx += 1
                            pixel += 6

                            '基地コードが出光昭和四日市又はコスモ四日市の場合
                            '転送販売計は荷受人計と同値なので、転送販売計を出力する
                            If "2401".Equals(row("BASECODE").ToString()) OrElse
                                "2402".Equals(row("BASECODE").ToString()) Then
                                '〇明細の設定(転送販売計)
                                'テンプレート⑥をコピーする
                                srcRange = ExcelTempSheet.Cells.Range("I27:CJ27")
                                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                srcRange.Copy(destRange)
                                ExcelMemoryRelease(srcRange)
                                ExcelMemoryRelease(destRange)
                                '値出力(着駅/荷受人/油種をスキップ)
                                EditTransportCostDetail_DetailArea(idx, row, 4)
                                'ピクセル加算
                                pixel += 18

                                '空行を差し込む
                                destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                                destRange.RowHeight = 6
                                ExcelMemoryRelease(destRange)
                                idx += 1
                                pixel += 6
                            End If
                        Else
                            '〇明細の設定(油種計)
                            'テンプレート③をコピーする
                            srcRange = ExcelTempSheet.Cells.Range("I16:CJ16")
                            destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                            '明細出力(転送販売/着駅/荷受人スキップ)
                            EditTransportCostDetail_DetailArea(idx, row, 1)
                            'ピクセル加算
                            pixel += 18
                        End If
                    Else
                        '出荷場所が不一致の場合 START
                        If Not lastRow("BASECODE").ToString().Equals((row("BASECODE").ToString())) Then
                            If "9998".Equals(row("BASECODE").ToString()) Then
                                '〇明細の設定(請求先部門計)
                                'テンプレート⑨をコピーする
                                srcRange = ExcelTempSheet.Cells.Range("I40:CJ40")
                                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                srcRange.Copy(destRange)
                                ExcelMemoryRelease(srcRange)
                                ExcelMemoryRelease(destRange)
                                '値出力(着駅/荷受人/油種をスキップし、請求先部門を出力)
                                EditTransportCostDetail_DetailArea(idx, row, 7)
                                'ピクセル加算
                                pixel += 18

                                '空行を差し込む
                                destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                                destRange.RowHeight = 6
                                ExcelMemoryRelease(destRange)
                                idx += 1
                                pixel += 6
                            ElseIf "9999".Equals(row("BASECODE").ToString()) Then
                                '〇明細の設定(請求先計)
                                'テンプレート⑨をコピーする
                                srcRange = ExcelTempSheet.Cells.Range("I44:CJ45")
                                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                srcRange.Copy(destRange)
                                ExcelMemoryRelease(srcRange)
                                ExcelMemoryRelease(destRange)
                                '値出力(着駅/荷受人/油種をスキップし、請求先を出力)
                                EditTransportCostDetail_DetailArea(idx, row, 8)
                                'ピクセル加算
                                pixel += 18
                                '2行目の高さを調整
                                destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                                destRange.RowHeight = 6
                                ExcelMemoryRelease(destRange)
                                idx += 1
                                pixel += 6
                            Else
                                '〇改頁処理
                                ChangeTansportCostDetailPage(idx, pixel)

                                '◯ヘッダーの設定
                                '値出力
                                EditTransportCostDetail_HeaderArea(idx, row, KEIJYO_YM)
                                'ピクセル加算
                                pixel += 150

                                '〇明細の設定
                                'テンプレート②をコピーする
                                srcRange = ExcelTempSheet.Cells.Range("I13:CJ13")
                                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                srcRange.Copy(destRange)
                                ExcelMemoryRelease(srcRange)
                                ExcelMemoryRelease(destRange)
                                '値出力(全項目)
                                EditTransportCostDetail_DetailArea(idx, row)
                                'ピクセル加算
                                pixel += 18
                            End If
                        Else
                            '出荷場所コードが同一だが、出荷場所名に「請求先部門」が入っている場合 START
                            If "請求先部門計".Equals(row("BASENAME").ToString()) Then
                                '〇明細の設定(請求先部門計)
                                'テンプレート⑨をコピーする
                                srcRange = ExcelTempSheet.Cells.Range("I40:CJ40")
                                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                srcRange.Copy(destRange)
                                ExcelMemoryRelease(srcRange)
                                ExcelMemoryRelease(destRange)
                                '値出力(着駅/荷受人/油種をスキップし、請求先部門を出力)
                                EditTransportCostDetail_DetailArea(idx, row, 7)
                                'ピクセル加算
                                pixel += 18

                                '空行を差し込む
                                destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                                destRange.RowHeight = 6
                                ExcelMemoryRelease(destRange)
                                idx += 1
                                pixel += 6
                            Else
                                '扱支店が不一致の場合 START
                                If Not lastRow("MANAGEBRANCHCODE").ToString().Equals((row("MANAGEBRANCHCODE").ToString())) Then
                                    '荷主計の場合 START
                                    If "99".Equals(row("MANAGEBRANCHCODE").ToString()) Then

                                        '〇明細の設定(荷主計)
                                        'テンプレート⑧をコピーする
                                        srcRange = ExcelTempSheet.Cells.Range("I36:CJ36")
                                        destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                        srcRange.Copy(destRange)
                                        ExcelMemoryRelease(srcRange)
                                        ExcelMemoryRelease(destRange)
                                        '値出力(着駅/荷受人/油種をスキップし、荷主を出力)
                                        EditTransportCostDetail_DetailArea(idx, row, 6)
                                        'ピクセル加算
                                        pixel += 18

                                        '空行を差し込む
                                        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                                        destRange.RowHeight = 6
                                        ExcelMemoryRelease(destRange)
                                        idx += 1
                                        pixel += 6
                                    Else
                                        '〇改頁処理
                                        ChangeTansportCostDetailPage(idx, pixel)

                                        '◯ヘッダーの設定
                                        '値出力
                                        EditTransportCostDetail_HeaderArea(idx, row, KEIJYO_YM)
                                        'ピクセル加算
                                        pixel += 150

                                        '〇明細の設定
                                        'テンプレート②をコピーする
                                        srcRange = ExcelTempSheet.Cells.Range("I13:CJ13")
                                        destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                        srcRange.Copy(destRange)
                                        ExcelMemoryRelease(srcRange)
                                        ExcelMemoryRelease(destRange)
                                        '値出力(全項目)
                                        EditTransportCostDetail_DetailArea(idx, row)
                                        'ピクセル加算
                                        pixel += 18
                                    End If
                                    '荷主計の場合 END
                                Else
                                    '荷受人が不一致の場合 START
                                    If Not lastRow("CONSIGNEECODE").ToString().Equals((row("CONSIGNEECODE").ToString())) Then
                                        '扱支店計の場合
                                        If "99".Equals(row("CONSIGNEECODE").ToString()) Then

                                            '基地コードが出光昭和四日市又はコスモ四日市以外の場合
                                            '転送販売計は扱支店計と同値なので、転送販売計を出力する
                                            If Not "2401".Equals(row("BASECODE").ToString()) AndAlso
                                                Not "2402".Equals(row("BASECODE").ToString()) Then
                                                '〇明細の設定(転送販売計)
                                                'テンプレート⑥をコピーする
                                                srcRange = ExcelTempSheet.Cells.Range("I27:CJ27")
                                                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                                srcRange.Copy(destRange)
                                                ExcelMemoryRelease(srcRange)
                                                ExcelMemoryRelease(destRange)
                                                '値出力(着駅/荷受人/油種をスキップ)
                                                EditTransportCostDetail_DetailArea(idx, row, 4)
                                                'ピクセル加算
                                                pixel += 18

                                                '空行を差し込む
                                                destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                                                destRange.RowHeight = 6
                                                ExcelMemoryRelease(destRange)
                                                idx += 1
                                                pixel += 6
                                            End If

                                            '〇明細の設定(扱支店計)
                                            'テンプレート⑦をコピーする
                                            srcRange = ExcelTempSheet.Cells.Range("I31:CJ32")
                                            destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                            srcRange.Copy(destRange)
                                            ExcelMemoryRelease(srcRange)
                                            ExcelMemoryRelease(destRange)
                                            '値出力(転送販売/着駅/荷受人/油種をスキップし、扱支店を出力)
                                            EditTransportCostDetail_DetailArea(idx, row, 5)
                                            'ピクセル加算
                                            pixel += 18
                                            '2行目の高さを調整
                                            destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                                            destRange.RowHeight = 6
                                            ExcelMemoryRelease(destRange)
                                            idx += 1
                                            pixel += 6

                                            '空行を差し込む
                                            destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                                            destRange.RowHeight = 6
                                            ExcelMemoryRelease(destRange)
                                            idx += 1
                                            pixel += 6
                                        Else
                                            '〇明細の設定(荷受人替わり)
                                            'テンプレート②をコピーする
                                            srcRange = ExcelTempSheet.Cells.Range("I13:CJ13")
                                            destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                                            srcRange.Copy(destRange)
                                            ExcelMemoryRelease(srcRange)
                                            ExcelMemoryRelease(destRange)
                                            '値出力(全項目)
                                            EditTransportCostDetail_DetailArea(idx, row)
                                            'ピクセル加算
                                            pixel += 18
                                        End If
                                    End If
                                    '荷受人が不一致の場合 END
                                End If
                                '扱支店が不一致の場合 END
                            End If
                            '出荷場所コードが同一だが、出荷場所名に「請求先部門」が入っている場合 END
                        End If
                        '出荷場所が不一致の場合 END
                    End If
                    '前行と輸送形態、請求先会社、請求先部門、出荷場所、荷主、扱支店、荷受人が一致する場合 END
                End If

                '最後に出力した行を保存
                lastRow = row
            Next

            '***** TODO処理 ここまで *****
            ExcelTempSheet.Delete() '雛形シート削除
            ExcelMemoryRelease(ExcelTempSheet)

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                'Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
                Me.ExcelBookObj.ExportAsFixedFormat(
                    Type:=0,
                    Filename:=tmpFilePath,
                    Quality:=0,
                    IncludeDocProperties:=True,
                    IgnorePrintAreas:=False,
                    OpenAfterPublish:=False)
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
    ''' 帳票のヘッダー設定(輸送費明細)
    ''' </summary>
    Private Sub EditTransportCostDetail_HeaderArea(ByRef idx As Int32, ByVal row As DataRow, ByVal KEIJYO_YM As Date)
        Dim rngHeaderArea As Excel.Range = Nothing
        Dim idxStr As String = ""

        Try
            '◯ 請求先名
            idxStr = String.Format("C{0}", 1 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = row("INVOICENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 請求先部門名
            idxStr = String.Format("C{0}", 2 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = row("INVOICEDEPTNAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 荷主名
            idxStr = String.Format("E{0}", 3 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            If row("SHIPPERSNAME").ToString().Length > 7 Then
                rngHeaderArea.Value = row("SHIPPERSNAME").ToString().Substring(0, 7)
            Else
                rngHeaderArea.Value = row("SHIPPERSNAME")
            End If
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 基地名
            idxStr = String.Format("F{0}", 5 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = row("BASENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 扱支店
            idxStr = String.Format("P{0}", 3 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = row("MANAGEBRANCHNAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 計上年月
            idxStr = String.Format("AG{0}", 3 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = KEIJYO_YM.ToString("yyyy")
            ExcelMemoryRelease(rngHeaderArea)
            idxStr = String.Format("AP{0}", 3 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = KEIJYO_YM.ToString("yyyy")
            ExcelMemoryRelease(rngHeaderArea)
            idxStr = String.Format("AK{0}", 3 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = KEIJYO_YM.ToString("MM")
            ExcelMemoryRelease(rngHeaderArea)
            idxStr = String.Format("AT{0}", 3 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = KEIJYO_YM.ToString("MM")
            ExcelMemoryRelease(rngHeaderArea)

            '〇 輸送形態
            idxStr = String.Format("AM{0}", 5 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = String.Format("{0}輸送", row("TRKBNNAME"))
            ExcelMemoryRelease(rngHeaderArea)

            '〇 支店名
            idxStr = String.Format("BR{0}", 5 + idx)
            rngHeaderArea = Me.ExcelWorkSheet.Range(idxStr)
            rngHeaderArea.Value = row("BRANCHNAME")
            ExcelMemoryRelease(rngHeaderArea)

            'ヘッダ行部分の行数を加算する
            idx += 11

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(輸送費明細)
    ''' </summary>
    Private Sub EditTransportCostDetail_DetailArea(ByRef idx As Int32, ByVal row As DataRow, Optional type As Int32 = 0)
        Dim rngDetailArea As Excel.Range = Nothing
        Dim total As Long = 0

        Try
            '◯ 転送/販売
            If type = 0 OrElse type = 2 OrElse type = 3 OrElse type = 4 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + idx.ToString())
                If "5421".Equals(row("ARRSTATION")) Then
                    rngDetailArea.Value = "販売"
                Else
                    rngDetailArea.Value = "転送"
                End If
                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 着駅
            If type = 0 OrElse type = 2 OrElse type = 3 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + idx.ToString())
                If row("ARRSTATIONNAME") IsNot DBNull.Value Then
                    Dim wkArrStationName As String = row("ARRSTATIONNAME").ToString()
                    '()（）を取り除く
                    wkArrStationName = wkArrStationName.Replace("(", "")
                    wkArrStationName = wkArrStationName.Replace(")", "")
                    wkArrStationName = wkArrStationName.Replace("（", "")
                    wkArrStationName = wkArrStationName.Replace("）", "")
                    rngDetailArea.Value = wkArrStationName
                Else
                    rngDetailArea.Value = ""
                End If
                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 荷受人
            If type = 0 OrElse type = 2 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + idx.ToString())
                If row("CONSIGNEENAME") IsNot DBNull.Value Then
                    rngDetailArea.Value = row("CONSIGNEENAME")
                Else
                    rngDetailArea.Value = ""
                End If
                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 油種
            If type = 0 OrElse type = 1 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("M" + idx.ToString())
                If row("ORDERINGOILNAME") IsNot DBNull.Value Then
                    rngDetailArea.Value = row("ORDERINGOILNAME")
                Else
                    rngDetailArea.Value = ""
                End If
                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 扱支店計の場合、扱支店を出力
            If type = 5 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + idx.ToString())
                rngDetailArea.Value = row("MANAGEBRANCHNAME")
                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 荷主計の場合、荷主を出力
            If type = 6 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + idx.ToString())
                If row("SHIPPERSNAME").ToString().Length > 11 Then
                    rngDetailArea.Value = row("SHIPPERSNAME").ToString().Substring(0, 11)
                Else
                    rngDetailArea.Value = row("SHIPPERSNAME")
                End If
                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 請求先部門計の場合、請求先部門を出力
            If type = 7 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + idx.ToString())

                Dim wkInvoiceDeptName As String = row("INVOICEDEPTNAME").ToString()
                If "物流管理部物流企画グループ".Equals(wkInvoiceDeptName) Then
                    wkInvoiceDeptName = "物流管理部物流企画"
                End If

                If wkInvoiceDeptName.Length > 11 Then
                    rngDetailArea.Value = wkInvoiceDeptName.Substring(0, 11)
                Else
                    rngDetailArea.Value = wkInvoiceDeptName
                End If

                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 請求先会社計の場合、請求先を出力
            If type = 8 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + idx.ToString())

                Dim wkInvoiceName As String = row("INVOICENAME").ToString()
                If "大阪国際石油精製株式会社".Equals(wkInvoiceName) Then
                    wkInvoiceName = "大阪国際石油精製株"
                ElseIf "日本オイルターミナル株式会社".Equals(wkInvoiceName) Then
                    wkInvoiceName = "日本オイルターミナル"
                End If

                If wkInvoiceName.Length > 11 Then
                    rngDetailArea.Value = wkInvoiceName.Substring(0, 11)
                Else
                    rngDetailArea.Value = wkInvoiceName
                End If

                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 数量
            rngDetailArea = Me.ExcelWorkSheet.Range("Q" + idx.ToString())
            If row("CARSAMOUNT") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.000}", row("CARSAMOUNT"))
            Else
                rngDetailArea.Value = "0.000"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 車数
            rngDetailArea = Me.ExcelWorkSheet.Range("V" + idx.ToString())
            If row("CARSNUMBER") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("CARSNUMBER"))
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 屯数
            rngDetailArea = Me.ExcelWorkSheet.Range("Y" + idx.ToString())
            If row("LOAD") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("LOAD"))
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 使用料(料率)
            rngDetailArea = Me.ExcelWorkSheet.Range("AC" + idx.ToString())
            If row("USAGE_FEE_RATE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.0}", row("USAGE_FEE_RATE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 使用料
            rngDetailArea = Me.ExcelWorkSheet.Range("AF" + idx.ToString())
            If row("USAGE_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("USAGE_FEE"))
                total += Long.Parse(row("USAGE_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 往路運賃
            rngDetailArea = Me.ExcelWorkSheet.Range("AK" + idx.ToString())
            If row("OUTBOUND_FARE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("OUTBOUND_FARE"))
                total += Long.Parse(row("OUTBOUND_FARE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 返路運賃
            rngDetailArea = Me.ExcelWorkSheet.Range("AP" + idx.ToString())
            If row("RETURN_FARE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("RETURN_FARE"))
                total += Long.Parse(row("RETURN_FARE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 運転科入換料(単価)
            rngDetailArea = Me.ExcelWorkSheet.Range("AU" + idx.ToString())
            If row("DRIVE_FEE_UPRICE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.00}", row("DRIVE_FEE_UPRICE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 運転科入換料
            rngDetailArea = Me.ExcelWorkSheet.Range("AX" + idx.ToString())
            If row("DRIVE_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("DRIVE_FEE"))
                total += Long.Parse(row("DRIVE_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 業務科管理料(単価)
            rngDetailArea = Me.ExcelWorkSheet.Range("BB" + idx.ToString())
            If row("BUSINESS_FEE_UPRICE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.00}", row("BUSINESS_FEE_UPRICE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 業務科管理料
            rngDetailArea = Me.ExcelWorkSheet.Range("BE" + idx.ToString())
            If row("BUSINESS_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("BUSINESS_FEE"))
                total += Long.Parse(row("BUSINESS_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 取扱料(単価)
            rngDetailArea = Me.ExcelWorkSheet.Range("BI" + idx.ToString())
            If row("HANDLING_FEE_UPRICE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.00}", row("HANDLING_FEE_UPRICE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 取扱料
            rngDetailArea = Me.ExcelWorkSheet.Range("BL" + idx.ToString())
            If row("HANDLING_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("HANDLING_FEE"))
                total += Long.Parse(row("HANDLING_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ OT運賃手数料(単価)
            rngDetailArea = Me.ExcelWorkSheet.Range("BP" + idx.ToString())
            If row("OT_FARE_FEE_UPRICE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.00}", row("OT_FARE_FEE_UPRICE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ OT運賃手数料
            rngDetailArea = Me.ExcelWorkSheet.Range("BS" + idx.ToString())
            If row("OT_FARE_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("OT_FARE_FEE"))
                total += Long.Parse(row("OT_FARE_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 計
            rngDetailArea = Me.ExcelWorkSheet.Range("BW" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:#,##0}", total)
            ExcelMemoryRelease(rngDetailArea)

            '行数を加算
            idx += 1

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

    ''' <summary>
    ''' 輸送費明細改頁処理
    ''' </summary>
    ''' <param name="idx">行インデックス</param>
    ''' <param name="pixel">出力済みPixel数</param>
    Private Sub ChangeTansportCostDetailPage(ByRef idx As Int32, ByRef pixel As Double, Optional type As Int32 = 0)
        Dim srcRange As Excel.Range = Nothing
        Dim destRange As Excel.Range = Nothing

        '出力済みPixel数が最大に達してない場合、ページ埋め処理
        While (pixel < TRANSPORT_COST_DETAIL_1PAGE_VLENGTH)
            '明細1行分(18)以上
            If TRANSPORT_COST_DETAIL_1PAGE_VLENGTH - pixel > 18 Then
                '高さの調整のみ
                destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                destRange.RowHeight = 18
                ExcelMemoryRelease(destRange)
                pixel += 18
            Else
                '1行以下（フッター行）の場合、MAX - 出力済みPixel数分の高さにして、下罫線を引く
                destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                destRange.RowHeight = TRANSPORT_COST_DETAIL_1PAGE_VLENGTH - pixel
                ExcelMemoryRelease(destRange)

                destRange = ExcelWorkSheet.Range(String.Format("B{0}:CA{0}", idx))
                destRange.Borders(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous
                ExcelMemoryRelease(destRange)

                pixel += TRANSPORT_COST_DETAIL_1PAGE_VLENGTH - pixel
            End If
            idx += 1
        End While

        '出力済みPixcel数をリセット
        pixel = 0

        '最終行の場合はヘッダーテンプレートコピー処理をせずに終了
        If type = 1 Then Exit Sub

        'テンプレートのコピー
        srcRange = ExcelTempSheet.Cells.Range("I1:CJ11")
        destRange = ExcelWorkSheet.Range("A" + idx.ToString())
        srcRange.Copy(destRange)
        ExcelMemoryRelease(srcRange)
        ExcelMemoryRelease(destRange)
        '行の高さ設定
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx)))
        destRange.RowHeight = 18
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 1)))
        destRange.RowHeight = 18
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 2)))
        destRange.RowHeight = 18
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 3)))
        destRange.RowHeight = 18
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 4)))
        destRange.RowHeight = 6
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 5)))
        destRange.RowHeight = 18
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 6)))
        destRange.RowHeight = 6
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 7)))
        destRange.RowHeight = 6
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 8)))
        destRange.RowHeight = 18
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 9)))
        destRange.RowHeight = 18
        ExcelMemoryRelease(destRange)
        destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 10)))
        destRange.RowHeight = 6
        ExcelMemoryRelease(destRange)
    End Sub
#End Region

#Region "ダウンロード(経理連携IF)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(経理連携IF)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_FinanceCooperationIF(ByVal username As String) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = "im_SW.xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim ridx As Integer = 2
            '固定帳票(経理連携IF)作成処理
            For Each row As DataRow In PrintData.Rows
                '明細出力
                EditFinanceCooperationIFDetail(ridx, row, username)
                ridx += 1
            Next

            '保存処理実行
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
    ''' 帳票の明細設定(経理連携IF)
    ''' </summary>
    Private Sub EditFinanceCooperationIFDetail(ByVal idx As Int32, ByVal row As DataRow, ByVal username As String)
        Dim rngDetailArea As Excel.Range = Nothing
        Dim total As Long = 0

        Try
            '◯ データ基準
            rngDetailArea = Me.ExcelWorkSheet.Range("A" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("データ基準"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 仕訳入力形式
            rngDetailArea = Me.ExcelWorkSheet.Range("B" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0000}", row("仕訳入力形式"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 入力画面番号
            rngDetailArea = Me.ExcelWorkSheet.Range("C" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:00}", row("入力画面番号"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 伝票日付
            rngDetailArea = Me.ExcelWorkSheet.Range("D" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("伝票日付"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 決算月区分
            rngDetailArea = Me.ExcelWorkSheet.Range("E" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("決算月区分"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 証憑番号
            rngDetailArea = Me.ExcelWorkSheet.Range("F" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("証憑番号"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 伝票番号
            rngDetailArea = Me.ExcelWorkSheet.Range("G" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:00000000}", row("伝票番号"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 伝票No
            rngDetailArea = Me.ExcelWorkSheet.Range("H" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("伝票No"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 明細行番号
            rngDetailArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:000}", row("明細行番号"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方科目
            rngDetailArea = Me.ExcelWorkSheet.Range("J" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:00000000}", row("借方科目"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方部門
            rngDetailArea = Me.ExcelWorkSheet.Range("K" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:000000}", row("借方部門"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方銀行
            rngDetailArea = Me.ExcelWorkSheet.Range("L" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0000}", row("借方銀行"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方取引先
            rngDetailArea = Me.ExcelWorkSheet.Range("M" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0000000000}", row("借方取引先"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方汎用補助1
            rngDetailArea = Me.ExcelWorkSheet.Range("N" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("借方汎用補助1"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方セグメント1
            rngDetailArea = Me.ExcelWorkSheet.Range("O" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:00000}", row("借方セグメント1"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方セグメント2
            rngDetailArea = Me.ExcelWorkSheet.Range("P" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:000}", row("借方セグメント2"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方セグメント3
            rngDetailArea = Me.ExcelWorkSheet.Range("Q" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("借方セグメント3"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方番号1
            rngDetailArea = Me.ExcelWorkSheet.Range("R" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("借方番号1"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方番号2
            rngDetailArea = Me.ExcelWorkSheet.Range("S" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("借方番号2"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方消費税区分
            rngDetailArea = Me.ExcelWorkSheet.Range("T" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("借方消費税区分"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方消費税コード
            rngDetailArea = Me.ExcelWorkSheet.Range("U" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:00}", row("借方消費税コード"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方消費税率区分
            rngDetailArea = Me.ExcelWorkSheet.Range("V" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("借方消費税率区分"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方外税同時入力区分
            rngDetailArea = Me.ExcelWorkSheet.Range("W" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("借方外税同時入力区分"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方金額
            rngDetailArea = Me.ExcelWorkSheet.Range("X" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("借方金額"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方消費税額
            rngDetailArea = Me.ExcelWorkSheet.Range("Y" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("借方消費税額"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方外貨金額
            rngDetailArea = Me.ExcelWorkSheet.Range("Z" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("借方外貨金額"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方外貨レート
            rngDetailArea = Me.ExcelWorkSheet.Range("AA" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("借方外貨レート"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 借方外貨取引区分
            rngDetailArea = Me.ExcelWorkSheet.Range("AB" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("借方外貨取引区分"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方科目
            rngDetailArea = Me.ExcelWorkSheet.Range("AC" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:00000000}", row("貸方科目"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方部門
            rngDetailArea = Me.ExcelWorkSheet.Range("AD" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:000000}", row("貸方部門"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方銀行
            rngDetailArea = Me.ExcelWorkSheet.Range("AE" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0000}", row("貸方銀行"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方取引先
            rngDetailArea = Me.ExcelWorkSheet.Range("AF" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0000000000}", row("貸方取引先"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方汎用補助1
            rngDetailArea = Me.ExcelWorkSheet.Range("AG" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("貸方汎用補助1"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方セグメント1
            rngDetailArea = Me.ExcelWorkSheet.Range("AH" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:00000}", row("貸方セグメント1"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方セグメント2
            rngDetailArea = Me.ExcelWorkSheet.Range("AI" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:000}", row("貸方セグメント2"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方セグメント3
            rngDetailArea = Me.ExcelWorkSheet.Range("AJ" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("貸方セグメント3"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方番号1
            rngDetailArea = Me.ExcelWorkSheet.Range("AK" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("貸方番号1"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方番号2
            rngDetailArea = Me.ExcelWorkSheet.Range("AL" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("貸方番号2"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方消費税区分
            rngDetailArea = Me.ExcelWorkSheet.Range("AM" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("貸方消費税区分"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方消費税コード
            rngDetailArea = Me.ExcelWorkSheet.Range("AN" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:00}", row("貸方消費税コード"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方消費税率区分
            rngDetailArea = Me.ExcelWorkSheet.Range("AO" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("貸方消費税率区分"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方外税同時入力区分
            rngDetailArea = Me.ExcelWorkSheet.Range("AP" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("貸方外税同時入力区分"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方金額
            rngDetailArea = Me.ExcelWorkSheet.Range("AQ" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("貸方金額"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方消費税額
            rngDetailArea = Me.ExcelWorkSheet.Range("AR" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:0}", row("貸方消費税額"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方外貨金額
            rngDetailArea = Me.ExcelWorkSheet.Range("AS" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("貸方外貨金額"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方外貨レート
            rngDetailArea = Me.ExcelWorkSheet.Range("AT" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("貸方外貨レート"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 貸方外貨取引区分
            rngDetailArea = Me.ExcelWorkSheet.Range("AU" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("貸方外貨取引区分"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 期日
            rngDetailArea = Me.ExcelWorkSheet.Range("AV" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("期日"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 摘要
            rngDetailArea = Me.ExcelWorkSheet.Range("AW" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("摘要"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 摘要コード1
            rngDetailArea = Me.ExcelWorkSheet.Range("AX" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("摘要コード1"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 作成日
            rngDetailArea = Me.ExcelWorkSheet.Range("AY" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:00000000}", row("作成日"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 作成時間
            rngDetailArea = Me.ExcelWorkSheet.Range("AZ" + idx.ToString())
            rngDetailArea.Value = String.Format("{0:000000}", row("作成時間"))
            ExcelMemoryRelease(rngDetailArea)

            '◯ 作成者
            rngDetailArea = Me.ExcelWorkSheet.Range("BA" + idx.ToString())
            rngDetailArea.Value = username
            ExcelMemoryRelease(rngDetailArea)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

#End Region

#Region "ダウンロード(タンク車運賃実績表-列車別-仙台以外)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(タンク車輸送実績表)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_TankTansportResult(
        ByVal STYMD As Date,
        ByVal EDYMD As Date,
        ByVal type As Integer
    ) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim e_idx As Integer = 1        'EXCEL行インデックス
            Dim pageVLength As Double = 0.0 '1ページ出力縦長
            Dim officeCode As String = ""

            For dr_idx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

                Dim srcRange As Excel.Range = Nothing
                Dim destRange As Excel.Range = Nothing

                Dim writeDetailCnt As Integer = 0       '出力明細数
                Dim trainChangeCnt As Integer = 0       '列車切替わり前の明細数
                Dim mergeCellsFlg As Boolean = False    'セル結合フラグ
                Dim mergeCellsStIdx As Integer = 0      'セル結合行

                Dim lastShippersCode As String = PrintData.Rows(dr_idx)("SHIPPERSCODE").ToString()
                Dim lastBaseCode As String = PrintData.Rows(dr_idx)("BASECODE").ToString()
                Dim lastConsigneeCode As String = PrintData.Rows(dr_idx)("CONSIGNEECODE").ToString()
                Dim lastTrainNo As String = PrintData.Rows(dr_idx)("TRAINNO").ToString()
                Dim lastOtTransportFlg As String = ""

                If dr_idx = 0 Then
                    officeCode = PrintData.Rows(dr_idx)("OFFICECODE").ToString()
                End If

                If CONST_OFFICECODE_011201.Equals(officeCode) Then
                    lastOtTransportFlg = PrintData.Rows(dr_idx)("OTTRANSPORTFLG").ToString()
                End If

                '出力する明細行数のカウント
                For pidx As Integer = dr_idx To PrintData.Rows.Count - 1 Step 3
                    Dim nrow As DataRow = PrintData.Rows(pidx)

                    If (
                            CONST_OFFICECODE_011201.Equals(officeCode) AndAlso                  '対象の営業所が五井で
                            Not lastOtTransportFlg.Equals(nrow("OTTRANSPORTFLG").ToString())    '前行とOT輸送フラグが異なる
                        ) OrElse
                        Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) OrElse     '前行と荷主が異なる
                        Not lastBaseCode.Equals(nrow("BASECODE").ToString()) OrElse             '前行と出荷元が異なる
                        Not lastConsigneeCode.Equals(nrow("CONSIGNEECODE").ToString()) Then     '前行と荷受人が異なる
                        Exit For
                    End If
                    '前行と列車番号が異なる場合は、レコード数をセーブしておく
                    If Not lastTrainNo.Equals(nrow("TRAINNO").ToString()) Then
                        trainChangeCnt = writeDetailCnt
                    End If
                    writeDetailCnt += 3

                    If CONST_OFFICECODE_011201.Equals(nrow("OFFICECODE").ToString()) Then
                        lastOtTransportFlg = nrow("OTTRANSPORTFLG").ToString()
                    End If
                    lastShippersCode = nrow("SHIPPERSCODE").ToString()
                    lastBaseCode = nrow("BASECODE").ToString()
                    lastConsigneeCode = nrow("CONSIGNEECODE").ToString()
                    lastTrainNo = nrow("TRAINNO").ToString()
                Next

                '出力明細数が1ページあたりの明細数の閾値を超えた場合は、直前の列車までの明細数に変更する
                If writeDetailCnt > TANK_TRANSPORT_RESULT_1PAGE_DETAIL_COUNT Then
                    writeDetailCnt = trainChangeCnt
                End If

                '◎ヘッダー出力処理
                If dr_idx = 0 Then                                                           '先頭レコード
                    '〇ヘッダー出力
                    EditTankTansportResult_HeaderArea(e_idx, PrintData.Rows(dr_idx), STYMD, EDYMD, type)

                    'ページ縦長加算
                    pageVLength += 93
                Else
                    Dim pageVLengthPlan = pageVLength + 7.5 + writeDetailCnt * 12.75
                    Dim nrow As DataRow = PrintData.Rows(dr_idx)
                    Dim lrow As DataRow = PrintData.Rows(dr_idx - 1)

                    '〇改頁条件
                    '①出力済み縦長＋空行＋明細行が、1ページ当たりの閾値に達した場合
                    '②五井営業所の場合のみ、OT輸送フラグが切り替わったタイミング
                    If pageVLengthPlan >= TANK_TRANSPORT_RESULT_1PAGE_VLENGTH OrElse
                        (
                            CONST_OFFICECODE_011201.Equals(officeCode) AndAlso
                            Not nrow("OTTRANSPORTFLG").ToString().Equals(lrow("OTTRANSPORTFLG").ToString())
                        ) Then
                        '〇改頁処理
                        While (pageVLength < TANK_TRANSPORT_RESULT_1PAGE_VLENGTH)
                            destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", e_idx))
                            '〇行高さの調整
                            '明細1行分(12.75)以上の場合
                            If TANK_TRANSPORT_RESULT_1PAGE_VLENGTH - pageVLength > 12.75 Then
                                destRange.RowHeight = 12.75
                                pageVLength += 12.75
                            Else
                                '明細1行分未満の場合、「閾値 - 出力済み縦長」
                                destRange.RowHeight = TANK_TRANSPORT_RESULT_1PAGE_VLENGTH - pageVLength
                                pageVLength += TANK_TRANSPORT_RESULT_1PAGE_VLENGTH - pageVLength
                            End If
                            ExcelMemoryRelease(destRange)
                            e_idx += 1
                        End While

                        'ページ縦長リセット
                        pageVLength = 0

                        '〇ヘッダーセルコピー
                        srcRange = ExcelTempSheet.Cells.Range("K1:DB7")
                        destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)

                        '〇行高さ調整
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", e_idx, e_idx + 3))
                        srcRange.RowHeight = 15
                        ExcelMemoryRelease(srcRange)
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", e_idx + 4))
                        srcRange.RowHeight = 7.5
                        ExcelMemoryRelease(srcRange)
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", e_idx + 5, e_idx + 6))
                        srcRange.RowHeight = 12.75
                        ExcelMemoryRelease(srcRange)

                        '〇ヘッダー出力
                        EditTankTansportResult_HeaderArea(e_idx, PrintData.Rows(dr_idx), STYMD, EDYMD, type)

                        'ページ縦長加算
                        pageVLength += 93
                    Else
                        '〇空行の差し込み
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", e_idx))
                        srcRange.RowHeight = 7.5
                        ExcelMemoryRelease(srcRange)
                        pageVLength += 7.5
                        e_idx += 1
                    End If

                End If

                '◎明細出力処理
                lastTrainNo = ""
                For i As Integer = 0 To writeDetailCnt - 1 Step 3

                    Dim nrow As DataRow = PrintData.Rows(dr_idx)

                    '〇明細セルコピー
                    If i = 0 Then
                        If "9999".Equals(nrow("TRAINNO").ToString()) Then
                            '荷受人計レコードの場合、テンプレート⑤をコピー
                            srcRange = ExcelTempSheet.Cells.Range("K21:DB23")
                            destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)

                            '〇 着駅計
                            Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
                            '()（）を取り除く
                            wkArrStationName = wkArrStationName.Replace("(", "")
                            wkArrStationName = wkArrStationName.Replace(")", "")
                            wkArrStationName = wkArrStationName.Replace("（", "")
                            wkArrStationName = wkArrStationName.Replace("）", "")
                            srcRange = Me.ExcelWorkSheet.Range("B" + e_idx.ToString())
                            srcRange.Value = wkArrStationName + "計"
                            ExcelMemoryRelease(srcRange)

                            mergeCellsFlg = True
                            mergeCellsStIdx = e_idx
                        Else
                            '列車レコードの先頭の場合、テンプレート②をコピー
                            srcRange = ExcelTempSheet.Cells.Range("K9:DB11")
                            destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)

                            '〇 着駅
                            Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
                            '()（）を取り除く
                            wkArrStationName = wkArrStationName.Replace("(", "")
                            wkArrStationName = wkArrStationName.Replace(")", "")
                            wkArrStationName = wkArrStationName.Replace("（", "")
                            wkArrStationName = wkArrStationName.Replace("）", "")
                            srcRange = Me.ExcelWorkSheet.Range("B" + e_idx.ToString())
                            srcRange.Value = wkArrStationName
                            ExcelMemoryRelease(srcRange)

                            '〇 荷受人
                            Dim wkConsigneeName As String = nrow("CONSIGNEENAME").ToString()
                            'ENEOS北信油槽所、ENEOS甲府油槽所の場合、ENEOSを取り除く
                            If "10".Equals(nrow("CONSIGNEECODE").ToString()) OrElse
                            "20".Equals(nrow("CONSIGNEECODE").ToString()) Then
                                wkConsigneeName = wkConsigneeName.Replace("ENEOS", "")
                                wkConsigneeName = wkConsigneeName.Replace("ＥＮＥＯＳ", "")
                            End If
                            srcRange = Me.ExcelWorkSheet.Range("G" + e_idx.ToString())
                            srcRange.Value = wkConsigneeName
                            ExcelMemoryRelease(srcRange)

                            '〇 車番
                            srcRange = Me.ExcelWorkSheet.Range("M" + e_idx.ToString())
                            srcRange.Value = nrow("TRAINNO").ToString()
                            ExcelMemoryRelease(srcRange)

                            mergeCellsFlg = False
                            mergeCellsStIdx = 0
                        End If
                    Else
                        '同一の列車レコードの場合
                        If lastTrainNo.Equals(nrow("TRAINNO").ToString()) Then
                            '列車レコードの中間・末尾の場合、テンプレート③をコピー
                            srcRange = ExcelTempSheet.Cells.Range("K13:DB15")
                            destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                        Else    '列車レコードが切り替わった場合
                            If "9999".Equals(nrow("TRAINNO").ToString()) Then
                                '荷受人計レコードの場合、テンプレート⑤をコピー
                                srcRange = ExcelTempSheet.Cells.Range("K21:DB23")
                                destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                                srcRange.Copy(destRange)
                                ExcelMemoryRelease(srcRange)
                                ExcelMemoryRelease(destRange)

                                '〇 着駅計
                                Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
                                '()（）を取り除く
                                wkArrStationName = wkArrStationName.Replace("(", "")
                                wkArrStationName = wkArrStationName.Replace(")", "")
                                wkArrStationName = wkArrStationName.Replace("（", "")
                                wkArrStationName = wkArrStationName.Replace("）", "")
                                srcRange = Me.ExcelWorkSheet.Range("B" + e_idx.ToString())
                                srcRange.Value = wkArrStationName + "計"
                                ExcelMemoryRelease(srcRange)

                                mergeCellsFlg = True
                                mergeCellsStIdx = e_idx
                            Else
                                '別列車レコードの場合、テンプレート④をコピー
                                srcRange = ExcelTempSheet.Cells.Range("K17:DB19")
                                destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                                srcRange.Copy(destRange)
                                ExcelMemoryRelease(srcRange)
                                ExcelMemoryRelease(destRange)

                                '〇 車番
                                srcRange = Me.ExcelWorkSheet.Range("M" + e_idx.ToString())
                                srcRange.Value = nrow("TRAINNO").ToString()
                                ExcelMemoryRelease(srcRange)

                                mergeCellsFlg = False
                                mergeCellsStIdx = 0
                            End If
                        End If
                    End If

                    '〇 車型(荷重)
                    srcRange = Me.ExcelWorkSheet.Range("Q" + e_idx.ToString())
                    If "1".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "43ｔ"
                    ElseIf "2".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "45ｔ"
                    ElseIf "3".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "43ｔ＋45ｔ"
                    ElseIf "4".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "44ｔ"
                    ElseIf "5".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "総　計"
                    End If
                    ExcelMemoryRelease(srcRange)

                    '〇行高さ調整
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", e_idx, e_idx + 2))
                    srcRange.RowHeight = 12.75
                    ExcelMemoryRelease(srcRange)

                    '〇明細出力
                    '危険品
                    EditTankTansportResult_DetailArea(e_idx, PrintData.Rows(dr_idx))
                    'その他
                    EditTankTansportResult_DetailArea(e_idx, PrintData.Rows(dr_idx + 1))
                    '計
                    EditTankTansportResult_DetailArea(e_idx, PrintData.Rows(dr_idx + 2))

                    'データ行index加算
                    dr_idx += 3

                    '出力縦長加算
                    pageVLength += 12.75 * 3

                    lastTrainNo = nrow("TRAINNO").ToString()
                Next

                'セル結合フラグONの場合
                If mergeCellsFlg Then
                    destRange = ExcelWorkSheet.Range(String.Format("B{0}:P{1}", mergeCellsStIdx, e_idx - 1))
                    destRange.MergeCells = True
                    destRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    destRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    ExcelMemoryRelease(destRange)
                End If

                '〇罫線を引く
                destRange = ExcelWorkSheet.Range(String.Format("B{0}:CQ{0}", e_idx))
                destRange.Borders(Excel.XlBordersIndex.xlEdgeTop).LineStyle = Excel.XlLineStyle.xlContinuous
                ExcelMemoryRelease(destRange)

            Next

            ExcelTempSheet.Delete() '雛形シート削除
            ExcelMemoryRelease(ExcelTempSheet)

            '保存処理実行
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
    ''' 帳票のヘッダー設定(タンク車輸送実績表)
    ''' </summary>
    Private Sub EditTankTansportResult_HeaderArea(
        ByRef idx As Integer,   'EXCEL行インデックス
        ByVal row As DataRow,   'データ行
        ByVal STYMD As Date,    '期間開始日
        ByVal EDYMD As Date,    '期間終了日
        ByVal type As Integer   '種別(1:往路所定 2:往路割引)
    )
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            '行加算
            idx += 2

            '◯ 出荷場所
            rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
            rngHeaderArea.Value = row("BASENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '五井営業所の場合のみ、輸送形態を表示
            rngHeaderArea = Me.ExcelWorkSheet.Range("AN" + idx.ToString())
            If CONST_OFFICECODE_011201.Equals(row("OFFICECODE").ToString()) Then
                If Integer.Parse(row("OTTRANSPORTFLG").ToString()) = 1 Then
                    rngHeaderArea.Value = "ＯＴ輸送"
                ElseIf Integer.Parse(row("OTTRANSPORTFLG").ToString()) = 2 Then
                    rngHeaderArea.Value = "請負輸送"
                Else
                    rngHeaderArea.Value = "全輸送計"
                End If
            End If
            ExcelMemoryRelease(rngHeaderArea)

            '行加算
            idx += 1

            '◯ 荷主
            rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
            rngHeaderArea.Value = row("SHIPPERSNAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 出力期間
            rngHeaderArea = Me.ExcelWorkSheet.Range("AN" + idx.ToString())
            rngHeaderArea.Value = String.Format("{0} ～ {1}", STYMD.ToString("yyyy年 MM月 dd日"), EDYMD.ToString("yyyy年 MM月 dd日"))
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 営業所
            rngHeaderArea = Me.ExcelWorkSheet.Range("CF" + idx.ToString())
            rngHeaderArea.Value = row("OFFICENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '行加算
            idx += 3

            If type = 2 Then
                rngHeaderArea = Me.ExcelWorkSheet.Range("AO" + idx.ToString())
                rngHeaderArea.Value = "往路割引"
                ExcelMemoryRelease(rngHeaderArea)
                rngHeaderArea = Me.ExcelWorkSheet.Range("BT" + idx.ToString())
                rngHeaderArea.Value = "往路割引"
                ExcelMemoryRelease(rngHeaderArea)
            End If

            '行加算
            idx += 1

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(タンク車輸送実績表)
    ''' </summary>
    Private Sub EditTankTansportResult_DetailArea(ByRef idx As Integer, ByVal row As DataRow, Optional ByVal arrFlg As Boolean = False)
        Dim rngDetailArea As Excel.Range = Nothing
        Dim total As Long = 0

        Try
            '〇 車数(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AC" + idx.ToString())
            rngDetailArea.Value = row("DAILY_CARSNUMBER")
            ExcelMemoryRelease(rngDetailArea)

            '〇 標屯(日計)
            If arrFlg Then
                rngDetailArea = Me.ExcelWorkSheet.Range("AF" + idx.ToString())
            Else
                rngDetailArea = Me.ExcelWorkSheet.Range("AG" + idx.ToString())
            End If
            rngDetailArea.Value = row("DAILY_LOAD")
            ExcelMemoryRelease(rngDetailArea)

            '〇 運屯(日計)
            Dim dailyLoad As Double = Double.Parse(row("DAILY_LOAD").ToString())
            Dim dailyCarsNumber As Integer = Integer.Parse(row("DAILY_CARSNUMBER").ToString())
            If arrFlg Then
                rngDetailArea = Me.ExcelWorkSheet.Range("AJ" + idx.ToString())
            Else
                rngDetailArea = Me.ExcelWorkSheet.Range("AK" + idx.ToString())
            End If
            rngDetailArea.Value = dailyLoad - (2.0 * dailyCarsNumber)
            ExcelMemoryRelease(rngDetailArea)

            '〇 往路所定(日計)
            If arrFlg Then
                rngDetailArea = Me.ExcelWorkSheet.Range("AN" + idx.ToString())
            Else
                rngDetailArea = Me.ExcelWorkSheet.Range("AO" + idx.ToString())
            End If
            rngDetailArea.Value = row("DAILY_OUTBOUND")
            ExcelMemoryRelease(rngDetailArea)

            '〇 返路所定(日計)
            If arrFlg Then
                rngDetailArea = Me.ExcelWorkSheet.Range("AT" + idx.ToString())
            Else
                rngDetailArea = Me.ExcelWorkSheet.Range("AU" + idx.ToString())
            End If
            rngDetailArea.Value = row("DAILY_RETURN")
            ExcelMemoryRelease(rngDetailArea)

            '〇 往返計(日計)
            Dim dailyOutBound As Double = Double.Parse(row("DAILY_OUTBOUND").ToString())
            Dim dailyReturn As Double = Double.Parse(row("DAILY_RETURN").ToString())
            rngDetailArea = Me.ExcelWorkSheet.Range("AZ" + idx.ToString())
            rngDetailArea.Value = dailyOutBound + dailyReturn
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("BF" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_CARSNUMBER")
            ExcelMemoryRelease(rngDetailArea)

            '〇 標屯(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("BJ" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_LOAD")
            ExcelMemoryRelease(rngDetailArea)

            '〇 運屯(日計)
            Dim monthlyLoad As Double = Double.Parse(row("MONTHLY_LOAD").ToString())
            Dim monthlyCarsNumber As Integer = Integer.Parse(row("MONTHLY_CARSNUMBER").ToString())
            rngDetailArea = Me.ExcelWorkSheet.Range("BO" + idx.ToString())
            rngDetailArea.Value = monthlyLoad - (2.0 * monthlyCarsNumber)
            ExcelMemoryRelease(rngDetailArea)

            '〇 往路所定(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("BT" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_OUTBOUND")
            ExcelMemoryRelease(rngDetailArea)

            '〇 返路所定(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("CB" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_RETURN")
            ExcelMemoryRelease(rngDetailArea)

            '〇 往返計(日計)
            Dim monthlyOutBound As Double = Double.Parse(row("MONTHLY_OUTBOUND").ToString())
            Dim monthlyReturn As Double = Double.Parse(row("MONTHLY_RETURN").ToString())
            rngDetailArea = Me.ExcelWorkSheet.Range("CJ" + idx.ToString())
            rngDetailArea.Value = monthlyOutBound + monthlyReturn
            ExcelMemoryRelease(rngDetailArea)

            '行加算
            idx += 1
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

#End Region

#Region "ダウンロード(タンク車運賃実績表-列車別-仙台以外-20210513前)"
    '''' <summary>
    '''' テンプレートを元に帳票を作成しダウンロード(タンク車輸送実績表)URLを生成する
    '''' </summary>
    '''' <returns>ダウンロード先URL</returns>
    '''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    'Public Function CreateExcelPrintData_TankTansportResult(
    '    ByVal STYMD As Date,
    '    ByVal EDYMD As Date,
    '    ByVal type As Integer
    ') As String
    '    Dim rngWrite As Excel.Range = Nothing
    '    Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
    '    Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

    '    Try
    '        Dim lastOtTransportFlg As String = ""
    '        Dim lastShippersCode As String = ""
    '        Dim lastBaseCode As String = ""
    '        Dim lastConsigneeCode As String = ""
    '        Dim lastTrainNo As String = ""
    '        Dim putDetail As Integer = 0
    '        Dim idx As Integer = 1

    '        For ridx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

    '            Dim nrow As DataRow = PrintData.Rows(ridx)
    '            Dim srcRange As Excel.Range = Nothing
    '            Dim destRange As Excel.Range = Nothing

    '            '◎ヘッダー出力処理
    '            If ridx = 0 Then                                                           '先頭レコード
    '                '〇ヘッダー出力
    '                EditTankTansportResult_HeaderArea(idx, nrow, STYMD, EDYMD, type)
    '            ElseIf (CONST_OFFICECODE_011201.Equals(nrow("OFFICECODE").ToString()) AndAlso   '対象の営業所が五井で
    '                Not lastOtTransportFlg.Equals(nrow("OTTRANSPORTFLG").ToString())) OrElse    '前行とOT輸送フラグが異なる
    '                Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) OrElse         '前行と荷主が異なる
    '                Not lastBaseCode.Equals(nrow("BASECODE").ToString()) OrElse                 '前行と出荷元が異なる
    '                Not lastConsigneeCode.Equals(nrow("CONSIGNEECODE").ToString()) OrElse       '前行と荷受人が異なる
    '                putDetail = 2 Then                                                          '出力済み明細数が2

    '                '〇改頁処理
    '                If putDetail = 1 Then
    '                    idx += 22 '明細1つ分＋2行飛ばす
    '                Else
    '                    idx += 2 '2行飛ばす
    '                End If
    '                'フッター行の高さ調整
    '                srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
    '                srcRange.RowHeight = 3
    '                ExcelMemoryRelease(srcRange)
    '                idx += 1

    '                '〇ヘッダーセルコピー
    '                srcRange = ExcelTempSheet.Cells.Range("K1:DB7")
    '                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
    '                srcRange.Copy(destRange)
    '                ExcelMemoryRelease(srcRange)
    '                ExcelMemoryRelease(destRange)

    '                '〇ヘッダー行高さ調整
    '                srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx, idx + 3))
    '                srcRange.RowHeight = 15
    '                ExcelMemoryRelease(srcRange)
    '                srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx + 4, idx + 4))
    '                srcRange.RowHeight = 4.5
    '                ExcelMemoryRelease(srcRange)

    '                '〇ヘッダー出力
    '                EditTankTansportResult_HeaderArea(idx, nrow, STYMD, EDYMD, type)

    '                '出力済み明細数初期化
    '                putDetail = 0
    '            End If
    '            '◎明細出力処理
    '            '〇明細セルコピー
    '            If "9999".Equals(nrow("TRAINNO").ToString()) Then
    '                '荷受人計の場合、テンプレート④をコピー
    '                srcRange = ExcelTempSheet.Cells.Range("K51:DB70")
    '                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
    '                srcRange.Copy(destRange)
    '                ExcelMemoryRelease(srcRange)
    '                ExcelMemoryRelease(destRange)

    '                '〇 着駅計
    '                Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
    '                '()（）を取り除く
    '                wkArrStationName = wkArrStationName.Replace("(", "")
    '                wkArrStationName = wkArrStationName.Replace(")", "")
    '                wkArrStationName = wkArrStationName.Replace("（", "")
    '                wkArrStationName = wkArrStationName.Replace("）", "")
    '                srcRange = Me.ExcelWorkSheet.Range("B" + idx.ToString())
    '                srcRange.Value = wkArrStationName + "計"
    '                ExcelMemoryRelease(srcRange)
    '            Else
    '                If putDetail = 0 Then
    '                    '出力明細数0の場合、テンプレート②をコピー
    '                    srcRange = ExcelTempSheet.Cells.Range("K9:DB28")
    '                    destRange = ExcelWorkSheet.Range("A" + idx.ToString())
    '                    srcRange.Copy(destRange)
    '                    ExcelMemoryRelease(srcRange)
    '                    ExcelMemoryRelease(destRange)

    '                    '〇 着駅
    '                    Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
    '                    '()（）を取り除く
    '                    wkArrStationName = wkArrStationName.Replace("(", "")
    '                    wkArrStationName = wkArrStationName.Replace(")", "")
    '                    wkArrStationName = wkArrStationName.Replace("（", "")
    '                    wkArrStationName = wkArrStationName.Replace("）", "")
    '                    srcRange = Me.ExcelWorkSheet.Range("B" + idx.ToString())
    '                    srcRange.Value = wkArrStationName
    '                    ExcelMemoryRelease(srcRange)

    '                    '〇 荷受人
    '                    Dim wkConsigneeName As String = nrow("CONSIGNEENAME").ToString()
    '                    'ENEOS北信油槽所、ENEOS甲府油槽所の場合、ENEOSを取り除く
    '                    If "10".Equals(nrow("CONSIGNEECODE").ToString()) OrElse
    '                        "20".Equals(nrow("CONSIGNEECODE").ToString()) Then
    '                        wkConsigneeName = wkConsigneeName.Replace("ENEOS", "")
    '                        wkConsigneeName = wkConsigneeName.Replace("ＥＮＥＯＳ", "")
    '                    End If
    '                    srcRange = Me.ExcelWorkSheet.Range("G" + idx.ToString())
    '                    srcRange.Value = wkConsigneeName
    '                    ExcelMemoryRelease(srcRange)
    '                Else
    '                    '荷受人計以外の場合、テンプレート③をコピー
    '                    srcRange = ExcelTempSheet.Cells.Range("K30:DB49")
    '                    destRange = ExcelWorkSheet.Range("A" + idx.ToString())
    '                    srcRange.Copy(destRange)
    '                    ExcelMemoryRelease(srcRange)
    '                    ExcelMemoryRelease(destRange)
    '                End If

    '                '〇 車番
    '                srcRange = Me.ExcelWorkSheet.Range("M" + idx.ToString())
    '                srcRange.Value = nrow("TRAINNO").ToString()
    '                ExcelMemoryRelease(srcRange)
    '            End If
    '            '〇明細出力ループ
    '            For i As Integer = 0 To 4 Step 1
    '                '揮発
    '                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx))
    '                '灯軽
    '                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 1))
    '                '黒油
    '                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 2))
    '                '計
    '                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 3))
    '                'データ行index加算
    '                ridx += 4
    '            Next

    '            If CONST_OFFICECODE_011201.Equals(nrow("OFFICECODE").ToString()) Then
    '                lastOtTransportFlg = nrow("OTTRANSPORTFLG").ToString()
    '            End If
    '            lastShippersCode = nrow("SHIPPERSCODE").ToString()
    '            lastBaseCode = nrow("BASECODE").ToString()
    '            lastConsigneeCode = nrow("CONSIGNEECODE").ToString()

    '            '出力済み明細数
    '            putDetail += 1
    '        Next

    '        ExcelTempSheet.Delete() '雛形シート削除
    '        ExcelMemoryRelease(ExcelTempSheet)

    '        '保存処理実行
    '        Dim saveExcelLock As New Object
    '        SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
    '            Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
    '        End SyncLock
    '        Me.ExcelBookObj.Close(False)

    '        Return UrlRoot & tmpFileName

    '    Catch ex As Exception
    '        Throw '呼出し元にThrow
    '    Finally
    '        ExcelMemoryRelease(rngWrite)
    '    End Try

    'End Function

    '''' <summary>
    '''' 帳票のヘッダー設定(タンク車輸送実績表)
    '''' </summary>
    'Private Sub EditTankTansportResult_HeaderArea(
    '    ByRef idx As Integer,   'EXCEL行インデックス
    '    ByVal row As DataRow,   'データ行
    '    ByVal STYMD As Date,    '期間開始日
    '    ByVal EDYMD As Date,    '期間終了日
    '    ByVal type As Integer   '種別(1:往路所定 2:往路割引)
    ')
    '    Dim rngHeaderArea As Excel.Range = Nothing

    '    Try
    '        '行加算
    '        idx += 2

    '        '◯ 出荷場所
    '        rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
    '        rngHeaderArea.Value = row("BASENAME")
    '        ExcelMemoryRelease(rngHeaderArea)

    '        '五井営業所の場合のみ、輸送形態を表示
    '        rngHeaderArea = Me.ExcelWorkSheet.Range("AN" + idx.ToString())
    '        If CONST_OFFICECODE_011201.Equals(row("OFFICECODE").ToString()) Then
    '            If Integer.Parse(row("OTTRANSPORTFLG").ToString()) = 1 Then
    '                rngHeaderArea.Value = "ＯＴ輸送"
    '            ElseIf Integer.Parse(row("OTTRANSPORTFLG").ToString()) = 2 Then
    '                rngHeaderArea.Value = "請負輸送"
    '            Else
    '                rngHeaderArea.Value = "全輸送計"
    '            End If
    '        End If
    '        ExcelMemoryRelease(rngHeaderArea)

    '        '行加算
    '        idx += 1

    '        '◯ 荷主
    '        rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
    '        rngHeaderArea.Value = row("SHIPPERSNAME")
    '        ExcelMemoryRelease(rngHeaderArea)

    '        '◯ 出力期間
    '        rngHeaderArea = Me.ExcelWorkSheet.Range("AN" + idx.ToString())
    '        rngHeaderArea.Value = String.Format("{0} ～ {1}", STYMD.ToString("yyyy年 MM月 dd日"), EDYMD.ToString("yyyy年 MM月 dd日"))
    '        ExcelMemoryRelease(rngHeaderArea)

    '        '◯ 営業所
    '        rngHeaderArea = Me.ExcelWorkSheet.Range("CF" + idx.ToString())
    '        rngHeaderArea.Value = row("OFFICENAME")
    '        ExcelMemoryRelease(rngHeaderArea)

    '        '行加算
    '        idx += 3

    '        If type = 2 Then
    '            rngHeaderArea = Me.ExcelWorkSheet.Range("AO" + idx.ToString())
    '            rngHeaderArea.Value = "往路割引"
    '            ExcelMemoryRelease(rngHeaderArea)
    '            rngHeaderArea = Me.ExcelWorkSheet.Range("BT" + idx.ToString())
    '            rngHeaderArea.Value = "往路割引"
    '            ExcelMemoryRelease(rngHeaderArea)
    '        End If

    '        '行加算
    '        idx += 1

    '    Catch ex As Exception
    '        Throw
    '    Finally
    '        ExcelMemoryRelease(rngHeaderArea)
    '    End Try
    'End Sub

    '''' <summary>
    '''' 帳票の明細設定(タンク車輸送実績表)
    '''' </summary>
    'Private Sub EditTankTansportResult_DetailArea(ByRef idx As Integer, ByVal row As DataRow)
    '    Dim rngDetailArea As Excel.Range = Nothing
    '    Dim total As Long = 0

    '    Try
    '        '〇 車数(日計)
    '        rngDetailArea = Me.ExcelWorkSheet.Range("AC" + idx.ToString())
    '        rngDetailArea.Value = row("DAILY_CARSNUMBER")
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 標屯(日計)
    '        rngDetailArea = Me.ExcelWorkSheet.Range("AG" + idx.ToString())
    '        rngDetailArea.Value = row("DAILY_LOAD")
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 運屯(日計)
    '        Dim dailyLoad As Double = Double.Parse(row("DAILY_LOAD").ToString())
    '        Dim dailyCarsNumber As Integer = Integer.Parse(row("DAILY_CARSNUMBER").ToString())
    '        rngDetailArea = Me.ExcelWorkSheet.Range("AK" + idx.ToString())
    '        rngDetailArea.Value = dailyLoad - (2.0 * dailyCarsNumber)
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 往路所定(日計)
    '        rngDetailArea = Me.ExcelWorkSheet.Range("AO" + idx.ToString())
    '        rngDetailArea.Value = row("DAILY_OUTBOUND")
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 返路所定(日計)
    '        rngDetailArea = Me.ExcelWorkSheet.Range("AU" + idx.ToString())
    '        rngDetailArea.Value = row("DAILY_RETURN")
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 往返計(日計)
    '        Dim dailyOutBound As Double = Double.Parse(row("DAILY_OUTBOUND").ToString())
    '        Dim dailyReturn As Double = Double.Parse(row("DAILY_RETURN").ToString())
    '        rngDetailArea = Me.ExcelWorkSheet.Range("AZ" + idx.ToString())
    '        rngDetailArea.Value = dailyOutBound + dailyReturn
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 車数(日計)
    '        rngDetailArea = Me.ExcelWorkSheet.Range("BF" + idx.ToString())
    '        rngDetailArea.Value = row("MONTHLY_CARSNUMBER")
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 標屯(日計)
    '        rngDetailArea = Me.ExcelWorkSheet.Range("BJ" + idx.ToString())
    '        rngDetailArea.Value = row("MONTHLY_LOAD")
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 運屯(日計)
    '        Dim monthlyLoad As Double = Double.Parse(row("MONTHLY_LOAD").ToString())
    '        Dim monthlyCarsNumber As Integer = Integer.Parse(row("MONTHLY_CARSNUMBER").ToString())
    '        rngDetailArea = Me.ExcelWorkSheet.Range("BO" + idx.ToString())
    '        rngDetailArea.Value = monthlyLoad - (2.0 * monthlyCarsNumber)
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 往路所定(日計)
    '        rngDetailArea = Me.ExcelWorkSheet.Range("BT" + idx.ToString())
    '        rngDetailArea.Value = row("MONTHLY_OUTBOUND")
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 返路所定(日計)
    '        rngDetailArea = Me.ExcelWorkSheet.Range("CB" + idx.ToString())
    '        rngDetailArea.Value = row("MONTHLY_RETURN")
    '        ExcelMemoryRelease(rngDetailArea)

    '        '〇 往返計(日計)
    '        Dim monthlyOutBound As Double = Double.Parse(row("MONTHLY_OUTBOUND").ToString())
    '        Dim monthlyReturn As Double = Double.Parse(row("MONTHLY_RETURN").ToString())
    '        rngDetailArea = Me.ExcelWorkSheet.Range("CJ" + idx.ToString())
    '        rngDetailArea.Value = monthlyOutBound + monthlyReturn
    '        ExcelMemoryRelease(rngDetailArea)

    '        '行加算
    '        idx += 1
    '    Catch ex As Exception
    '        Throw
    '    Finally
    '        ExcelMemoryRelease(rngDetailArea)
    '    End Try

    'End Sub

#End Region

#Region "ダウンロード(タンク車運賃実績表-列車別-仙台)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(タンク車輸送実績表)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_TankTansportResult_010402(
        ByVal STYMD As Date,
        ByVal EDYMD As Date,
        ByVal type As Integer
    ) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim lastOfficeCode As String = ""
            Dim lastShippersCode As String = ""
            Dim lastBaseCode As String = ""
            Dim lastConsigneeCode As String = ""
            Dim putRow As Integer = 0
            Dim idx As Integer = 1

            For ridx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

                Dim nrow As DataRow = PrintData.Rows(ridx)
                Dim srcRange As Excel.Range = Nothing
                Dim destRange As Excel.Range = Nothing

                '◎ヘッダー出力処理
                If ridx = 0 Then                                                            '先頭レコード
                    '〇ヘッダー出力
                    EditTankTansportResult_HeaderArea(idx, nrow, STYMD, EDYMD, type)
                ElseIf Not lastOfficeCode.Equals(nrow("OFFICECODE").ToString()) OrElse      '前行と営業所が異なる
                    Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) OrElse     '前行と荷主が異なる
                    Not lastBaseCode.Equals(nrow("BASECODE").ToString()) OrElse             '前行と出荷元が異なる
                    TANK_TRANSPORT_RESULT_1PAGE_DETAIL_COUNT - putRow < 3 Then              '1ページ辺りの最大行数 - 出力済み行数が4（明細行数）以下                                                    '出力済み明細数が10

                    '〇改頁処理
                    For i As Integer = putRow To TANK_TRANSPORT_RESULT_1PAGE_DETAIL_COUNT - 1 Step 1
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                        srcRange.RowHeight = 12.75
                        ExcelMemoryRelease(srcRange)
                        idx += 1 '出力済み明細数
                    Next

                    '〇ヘッダーセルコピー
                    srcRange = ExcelTempSheet.Cells.Range("K1:DB7")
                    destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    ExcelMemoryRelease(srcRange)
                    ExcelMemoryRelease(destRange)

                    '〇ヘッダー行高さ調整
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", idx, idx + 3))
                    srcRange.RowHeight = 15
                    ExcelMemoryRelease(srcRange)
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx + 4))
                    srcRange.RowHeight = 7.5
                    ExcelMemoryRelease(srcRange)
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", idx + 5, idx + 6))
                    srcRange.RowHeight = 12.75
                    ExcelMemoryRelease(srcRange)

                    '〇ヘッダー出力
                    EditTankTansportResult_HeaderArea(idx, nrow, STYMD, EDYMD, type)

                    '出力済み明細数初期化
                    putRow = 0
                End If
                '◎明細出力処理
                '〇明細セルコピー
                If "9999".Equals(nrow("TRAINNO").ToString()) Then
                    '荷受人計の場合、テンプレート④をコピー
                    srcRange = ExcelTempSheet.Cells.Range("K17:DB19")
                    destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    ExcelMemoryRelease(srcRange)
                    ExcelMemoryRelease(destRange)

                    '〇 着駅計
                    Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
                    '()（）を取り除く
                    wkArrStationName = wkArrStationName.Replace("(", "")
                    wkArrStationName = wkArrStationName.Replace(")", "")
                    wkArrStationName = wkArrStationName.Replace("（", "")
                    wkArrStationName = wkArrStationName.Replace("）", "")
                    srcRange = Me.ExcelWorkSheet.Range("B" + idx.ToString())
                    srcRange.Value = wkArrStationName + "計"
                    ExcelMemoryRelease(srcRange)
                Else
                    'ヘッダー出力後か、前行と荷主が異なる場合
                    If putRow = 0 OrElse
                        Not lastConsigneeCode.Equals(nrow("CONSIGNEECODE").ToString()) Then
                        'ページ内で荷受人が変わる場合
                        If Not putRow = 0 AndAlso
                            Not lastConsigneeCode.Equals(nrow("CONSIGNEECODE").ToString()) Then

                            srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                            srcRange.RowHeight = 12.75
                            ExcelMemoryRelease(srcRange)
                            idx += 1
                            putRow += 1
                        End If

                        '出力明細数0の場合、テンプレート②をコピー
                        srcRange = ExcelTempSheet.Cells.Range("K9:DB11")
                        destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)

                        '〇 着駅
                        Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
                        '()（）を取り除く
                        wkArrStationName = wkArrStationName.Replace("(", "")
                        wkArrStationName = wkArrStationName.Replace(")", "")
                        wkArrStationName = wkArrStationName.Replace("（", "")
                        wkArrStationName = wkArrStationName.Replace("）", "")
                        srcRange = Me.ExcelWorkSheet.Range("B" + idx.ToString())
                        srcRange.Value = wkArrStationName
                        ExcelMemoryRelease(srcRange)

                        '〇 荷受人
                        Dim wkConsigneeName As String = nrow("CONSIGNEENAME").ToString()
                        'ENEOS北信油槽所、ENEOS甲府油槽所の場合、ENEOSを取り除く
                        If "10".Equals(nrow("CONSIGNEECODE").ToString()) OrElse
                            "20".Equals(nrow("CONSIGNEECODE").ToString()) Then
                            wkConsigneeName = wkConsigneeName.Replace("ENEOS", "")
                            wkConsigneeName = wkConsigneeName.Replace("ＥＮＥＯＳ", "")
                        End If
                        srcRange = Me.ExcelWorkSheet.Range("G" + idx.ToString())
                        srcRange.Value = wkConsigneeName
                        ExcelMemoryRelease(srcRange)
                    Else
                        '荷受人計以外の場合、テンプレート③をコピー
                        srcRange = ExcelTempSheet.Cells.Range("K13:DB15")
                        destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)
                    End If

                    '〇 車番
                    srcRange = Me.ExcelWorkSheet.Range("M" + idx.ToString())
                    srcRange.Value = nrow("TRAINNO").ToString()
                    ExcelMemoryRelease(srcRange)
                End If

                srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", idx, idx + 2))
                srcRange.RowHeight = 12.75
                ExcelMemoryRelease(srcRange)

                '〇明細出力
                '危険品
                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx))
                'その他
                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 1))
                '計
                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 2))
                'データ行index加算
                ridx += 3

                lastOfficeCode = nrow("OFFICECODE").ToString()
                lastShippersCode = nrow("SHIPPERSCODE").ToString()
                lastBaseCode = nrow("BASECODE").ToString()
                lastConsigneeCode = nrow("CONSIGNEECODE").ToString()

                '出力済み行数
                putRow += 3
            Next

            ExcelTempSheet.Delete() '雛形シート削除
            ExcelMemoryRelease(ExcelTempSheet)

            '保存処理実行
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

#End Region

#Region "ダウンロード(タンク車運賃実績表-着駅別-仙台以外)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(タンク車輸送実績表)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_TankTansportResult_Arr(
        ByVal STYMD As Date,
        ByVal EDYMD As Date,
        ByVal type As Integer
    ) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim e_idx As Integer = 1        'EXCEL行インデックス
            Dim pageVLength As Double = 0.0 '1ページ出力縦長
            Dim officeCode As String = ""

            For dr_idx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

                Dim srcRange As Excel.Range = Nothing
                Dim destRange As Excel.Range = Nothing

                Dim writeDetailCnt As Integer = 0       '出力明細数
                Dim mergeCellsFlg As Boolean = False    'セル結合フラグ
                Dim mergeCellsStIdx As Integer = 0      'セル結合行

                Dim lastShippersCode As String = PrintData.Rows(dr_idx)("SHIPPERSCODE").ToString()
                Dim lastBaseCode As String = PrintData.Rows(dr_idx)("BASECODE").ToString()
                Dim lastConsigneeCode As String = PrintData.Rows(dr_idx)("CONSIGNEECODE").ToString()
                Dim lastOtTransportFlg As String = ""

                If dr_idx = 0 Then
                    officeCode = PrintData.Rows(dr_idx)("OFFICECODE").ToString()
                End If

                If CONST_OFFICECODE_011201.Equals(officeCode) Then
                    lastOtTransportFlg = PrintData.Rows(dr_idx)("OTTRANSPORTFLG").ToString()
                End If

                '出力する明細行数のカウント
                For pidx As Integer = dr_idx To PrintData.Rows.Count - 1 Step 3
                    Dim nrow As DataRow = PrintData.Rows(pidx)

                    If (
                            CONST_OFFICECODE_011201.Equals(officeCode) AndAlso                  '対象の営業所が五井で
                            Not lastOtTransportFlg.Equals(nrow("OTTRANSPORTFLG").ToString())    '前行とOT輸送フラグが異なる
                        ) OrElse
                        Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) OrElse     '前行と荷主が異なる
                        Not lastBaseCode.Equals(nrow("BASECODE").ToString()) OrElse             '前行と出荷元が異なる
                        Not lastConsigneeCode.Equals(nrow("CONSIGNEECODE").ToString()) Then     '前行と荷受人が異なる
                        Exit For
                    End If

                    writeDetailCnt += 3

                    If CONST_OFFICECODE_011201.Equals(nrow("OFFICECODE").ToString()) Then
                        lastOtTransportFlg = nrow("OTTRANSPORTFLG").ToString()
                    End If
                    lastShippersCode = nrow("SHIPPERSCODE").ToString()
                    lastBaseCode = nrow("BASECODE").ToString()
                    lastConsigneeCode = nrow("CONSIGNEECODE").ToString()
                Next

                '◎ヘッダー出力処理
                If dr_idx = 0 Then                                                                '先頭レコード
                    '〇ヘッダー出力
                    EditTankTansportResult_HeaderArea(e_idx, PrintData.Rows(dr_idx), STYMD, EDYMD, type)

                    'ページ縦長加算
                    pageVLength += 97.5
                Else
                    Dim pageVLengthPlan = pageVLength + 7.5 + writeDetailCnt * 18.75
                    Dim nrow As DataRow = PrintData.Rows(dr_idx)
                    Dim lrow As DataRow = PrintData.Rows(dr_idx - 1)

                    '〇改頁条件
                    '①出力済み縦長＋空行＋明細行が、1ページ当たりの閾値に達した場合
                    '②五井営業所の場合のみ、OT輸送フラグが切り替わったタイミング
                    If pageVLengthPlan >= TANK_TRANSPORT_RESULT_ARR_1PAGE_VLENGTH OrElse
                        (
                            CONST_OFFICECODE_011201.Equals(officeCode) AndAlso
                            Not nrow("OTTRANSPORTFLG").ToString().Equals(lrow("OTTRANSPORTFLG").ToString())
                        ) Then
                        '〇改頁処理
                        While (pageVLength < TANK_TRANSPORT_RESULT_ARR_1PAGE_VLENGTH)
                            destRange = ExcelWorkSheet.Range(String.Format("{0}:{0}", e_idx))
                            '〇行高さの調整
                            '明細1行分(18.75)以上の場合
                            If TANK_TRANSPORT_RESULT_ARR_1PAGE_VLENGTH - pageVLength > 18.75 Then
                                destRange.RowHeight = 18.75
                                pageVLength += 18.75
                            Else
                                '明細1行分未満の場合、「閾値 - 出力済み縦長」
                                destRange.RowHeight = TANK_TRANSPORT_RESULT_ARR_1PAGE_VLENGTH - pageVLength
                                pageVLength += TANK_TRANSPORT_RESULT_ARR_1PAGE_VLENGTH - pageVLength
                            End If
                            ExcelMemoryRelease(destRange)
                            e_idx += 1
                        End While

                        'ページ縦長リセット
                        pageVLength = 0

                        '〇ヘッダーセルコピー
                        srcRange = ExcelTempSheet.Cells.Range("K1:DB7")
                        destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)

                        '〇行高さ調整
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", e_idx, e_idx + 3))
                        srcRange.RowHeight = 15
                        ExcelMemoryRelease(srcRange)
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", e_idx + 4))
                        srcRange.RowHeight = 7.5
                        ExcelMemoryRelease(srcRange)
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", e_idx + 5, e_idx + 6))
                        srcRange.RowHeight = 15
                        ExcelMemoryRelease(srcRange)

                        '〇ヘッダー出力
                        EditTankTansportResult_HeaderArea(e_idx, PrintData.Rows(dr_idx), STYMD, EDYMD, type)

                        'ページ縦長加算
                        pageVLength += 97.5
                    Else
                        '〇空行の差し込み
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", e_idx))
                        srcRange.RowHeight = 7.5
                        ExcelMemoryRelease(srcRange)
                        pageVLength += 7.5
                        e_idx += 1
                    End If

                End If

                '◎明細出力処理
                For i As Integer = 0 To writeDetailCnt - 1 Step 3

                    Dim nrow As DataRow = PrintData.Rows(dr_idx)

                    '〇明細セルコピー
                    If i = 0 Then   '先頭レコードの場合
                        mergeCellsStIdx = e_idx
                        '基地計レコードの場合
                        If "9999999".Equals(nrow("ARRSTATION").ToString()) AndAlso
                            "99".Equals(nrow("CONSIGNEECODE").ToString()) Then
                            'テンプレート④をコピー
                            srcRange = ExcelTempSheet.Cells.Range("K17:DB19")
                            destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)

                            '〇 「基地名」+計
                            Dim wkBaseTotalName As String = nrow("BASENAME").ToString() + "計"
                            srcRange = Me.ExcelWorkSheet.Range("B" + e_idx.ToString())
                            srcRange.Value = wkBaseTotalName
                            ExcelMemoryRelease(srcRange)

                            mergeCellsFlg = True
                        Else    '基地計以外のレコードの場合
                            'テンプレート②をコピー
                            srcRange = ExcelTempSheet.Cells.Range("K9:DB11")
                            destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)

                            '〇 着駅
                            Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
                            '()（）を取り除く
                            wkArrStationName = wkArrStationName.Replace("(", "")
                            wkArrStationName = wkArrStationName.Replace(")", "")
                            wkArrStationName = wkArrStationName.Replace("（", "")
                            wkArrStationName = wkArrStationName.Replace("）", "")
                            srcRange = Me.ExcelWorkSheet.Range("B" + e_idx.ToString())
                            srcRange.Value = wkArrStationName
                            ExcelMemoryRelease(srcRange)

                            '〇 荷受人
                            Dim wkConsigneeName As String = nrow("CONSIGNEENAME").ToString()
                            'ENEOS北信油槽所、ENEOS甲府油槽所の場合、ENEOSを取り除く
                            If "10".Equals(nrow("CONSIGNEECODE").ToString()) OrElse
                                "20".Equals(nrow("CONSIGNEECODE").ToString()) Then
                                wkConsigneeName = wkConsigneeName.Replace("ENEOS", "")
                                wkConsigneeName = wkConsigneeName.Replace("ＥＮＥＯＳ", "")
                            End If
                            srcRange = Me.ExcelWorkSheet.Range("I" + e_idx.ToString())
                            srcRange.Value = wkConsigneeName
                            ExcelMemoryRelease(srcRange)

                            mergeCellsFlg = False
                        End If
                    Else
                        'テンプレート③をコピー
                        srcRange = ExcelTempSheet.Cells.Range("K13:DB15")
                        destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)
                    End If

                    '〇 車型(荷重)
                    srcRange = Me.ExcelWorkSheet.Range("Q" + e_idx.ToString())
                    If "1".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "43ｔ"
                    ElseIf "2".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "45ｔ"
                    ElseIf "3".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "43ｔ＋45ｔ"
                    ElseIf "4".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "44ｔ"
                    ElseIf "5".Equals(nrow("LOADCODE").ToString()) Then
                        srcRange.Value = "総　計"
                    End If
                    ExcelMemoryRelease(srcRange)

                    '〇行高さ調整
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", e_idx, e_idx + 2))
                    srcRange.RowHeight = 18.75
                    ExcelMemoryRelease(srcRange)

                    '〇明細出力
                    '危険品
                    EditTankTansportResult_DetailArea(e_idx, PrintData.Rows(dr_idx), True)
                    'その他
                    EditTankTansportResult_DetailArea(e_idx, PrintData.Rows(dr_idx + 1), True)
                    '計
                    EditTankTansportResult_DetailArea(e_idx, PrintData.Rows(dr_idx + 2), True)

                    'データ行index加算
                    dr_idx += 3

                    '出力縦長加算
                    pageVLength += 18.75 * 3

                Next

                If mergeCellsFlg Then   'セル結合フラグON(基地計)の場合
                    destRange = ExcelWorkSheet.Range(String.Format("B{0}:P{1}", mergeCellsStIdx, e_idx - 1))
                    destRange.MergeCells = True
                    destRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    destRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    ExcelMemoryRelease(destRange)
                Else                    'セル結合フラグON(着駅計)の場合
                    '着駅
                    destRange = ExcelWorkSheet.Range(String.Format("B{0}:H{1}", mergeCellsStIdx, e_idx - 1))
                    destRange.MergeCells = True
                    destRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    destRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    ExcelMemoryRelease(destRange)
                    '荷受人
                    destRange = ExcelWorkSheet.Range(String.Format("I{0}:P{1}", mergeCellsStIdx, e_idx - 1))
                    destRange.MergeCells = True
                    destRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    destRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    ExcelMemoryRelease(destRange)
                End If

                '〇罫線を引く
                destRange = ExcelWorkSheet.Range(String.Format("B{0}:P{0}", e_idx))
                destRange.Borders(Excel.XlBordersIndex.xlEdgeTop).LineStyle = Excel.XlLineStyle.xlContinuous
                ExcelMemoryRelease(destRange)

            Next

            ExcelTempSheet.Delete() '雛形シート削除
            ExcelMemoryRelease(ExcelTempSheet)

            '保存処理実行
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

#End Region

#Region "ダウンロード(タンク車運賃実績表-着駅別-仙台以外-20210514前)"
    '''' <summary>
    '''' テンプレートを元に帳票を作成しダウンロード(タンク車輸送実績表)URLを生成する
    '''' </summary>
    '''' <returns>ダウンロード先URL</returns>
    '''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    'Public Function CreateExcelPrintData_TankTansportResult_Arr(
    '    ByVal STYMD As Date,
    '    ByVal EDYMD As Date,
    '    ByVal type As Integer
    ') As String
    '    Dim rngWrite As Excel.Range = Nothing
    '    Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
    '    Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

    '    Try
    '        Dim lastOtTransportFlg As String = ""
    '        Dim lastShippersCode As String = ""
    '        Dim lastBaseCode As String = ""
    '        Dim putDetail As Integer = 0
    '        Dim idx As Integer = 1

    '        For ridx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

    '            Dim nrow As DataRow = PrintData.Rows(ridx)
    '            Dim srcRange As Excel.Range = Nothing
    '            Dim destRange As Excel.Range = Nothing

    '            '------------------
    '            ' ヘッダー出力処理
    '            '------------------
    '            If ridx = 0 Then                                                           '先頭レコード
    '                '〇ヘッダー出力
    '                EditTankTansportResult_HeaderArea(idx, nrow, STYMD, EDYMD, type)
    '            ElseIf (CONST_OFFICECODE_011201.Equals(nrow("OFFICECODE").ToString()) AndAlso   '対象の営業所が五井で
    '                Not lastOtTransportFlg.Equals(nrow("OTTRANSPORTFLG").ToString())) OrElse    '前行とOT輸送フラグが異なる
    '                Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) OrElse         '前行と荷主が異なる
    '                Not lastBaseCode.Equals(nrow("BASECODE").ToString()) OrElse                 '前行と出荷元が異なる
    '                putDetail = 2 Then                                                          '出力済み明細数が2

    '                '〇改頁処理
    '                If putDetail = 1 Then
    '                    idx += 22 '明細1つ分＋2行飛ばす
    '                Else
    '                    idx += 2 '2行飛ばす
    '                End If
    '                'フッター行の高さ調整
    '                srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
    '                srcRange.RowHeight = 3
    '                ExcelMemoryRelease(srcRange)
    '                idx += 1

    '                '〇ヘッダーセルコピー
    '                srcRange = ExcelTempSheet.Cells.Range("K1:DB7")
    '                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
    '                srcRange.Copy(destRange)
    '                ExcelMemoryRelease(srcRange)
    '                ExcelMemoryRelease(destRange)

    '                '〇ヘッダー行高さ調整
    '                srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx, idx + 3))
    '                srcRange.RowHeight = 15
    '                ExcelMemoryRelease(srcRange)
    '                srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx + 4, idx + 4))
    '                srcRange.RowHeight = 4.5
    '                ExcelMemoryRelease(srcRange)

    '                '〇ヘッダー出力
    '                EditTankTansportResult_HeaderArea(idx, nrow, STYMD, EDYMD, type)

    '                '出力済み明細数初期化
    '                putDetail = 0
    '            End If

    '            '--------------
    '            ' 明細出力処理 
    '            '--------------
    '            '〇明細セルコピー
    '            '基地計の場合
    '            If "9999999".Equals(nrow("ARRSTATION").ToString()) AndAlso
    '                "99".Equals(nrow("CONSIGNEECODE").ToString()) Then

    '                'テンプレート③をコピー
    '                srcRange = ExcelTempSheet.Cells.Range("K30:DB49")
    '                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
    '                srcRange.Copy(destRange)
    '                ExcelMemoryRelease(srcRange)
    '                ExcelMemoryRelease(destRange)

    '                '〇 「基地名」+計
    '                Dim wkBaseTotalName As String = nrow("BASENAME").ToString() + "計"
    '                srcRange = Me.ExcelWorkSheet.Range("B" + idx.ToString())
    '                srcRange.Value = wkBaseTotalName
    '                ExcelMemoryRelease(srcRange)
    '            Else    '基地計以外の場合

    '                'テンプレート②をコピー
    '                srcRange = ExcelTempSheet.Cells.Range("K9:DB28")
    '                destRange = ExcelWorkSheet.Range("A" + idx.ToString())
    '                srcRange.Copy(destRange)
    '                ExcelMemoryRelease(srcRange)
    '                ExcelMemoryRelease(destRange)

    '                '〇 着駅
    '                Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
    '                '()（）を取り除く
    '                wkArrStationName = wkArrStationName.Replace("(", "")
    '                wkArrStationName = wkArrStationName.Replace(")", "")
    '                wkArrStationName = wkArrStationName.Replace("（", "")
    '                wkArrStationName = wkArrStationName.Replace("）", "")
    '                srcRange = Me.ExcelWorkSheet.Range("B" + idx.ToString())
    '                srcRange.Value = wkArrStationName
    '                ExcelMemoryRelease(srcRange)

    '                '〇 荷受人
    '                Dim wkConsigneeName As String = nrow("CONSIGNEENAME").ToString()
    '                'ENEOS北信油槽所、ENEOS甲府油槽所の場合、ENEOSを取り除く
    '                If "10".Equals(nrow("CONSIGNEECODE").ToString()) OrElse
    '                    "20".Equals(nrow("CONSIGNEECODE").ToString()) Then
    '                    wkConsigneeName = wkConsigneeName.Replace("ENEOS", "")
    '                    wkConsigneeName = wkConsigneeName.Replace("ＥＮＥＯＳ", "")
    '                End If
    '                srcRange = Me.ExcelWorkSheet.Range("I" + idx.ToString())
    '                srcRange.Value = wkConsigneeName
    '                ExcelMemoryRelease(srcRange)
    '            End If

    '            '〇明細出力ループ
    '            For i As Integer = 0 To 4 Step 1
    '                '揮発
    '                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx))
    '                '灯軽
    '                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 1))
    '                '黒油
    '                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 2))
    '                '計
    '                EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 3))
    '                'データ行index加算
    '                ridx += 4
    '            Next

    '            If CONST_OFFICECODE_011201.Equals(nrow("OFFICECODE").ToString()) Then
    '                lastOtTransportFlg = nrow("OTTRANSPORTFLG").ToString()
    '            End If
    '            lastShippersCode = nrow("SHIPPERSCODE").ToString()
    '            lastBaseCode = nrow("BASECODE").ToString()

    '            '出力済み明細数
    '            putDetail += 1
    '        Next

    '        ExcelTempSheet.Delete() '雛形シート削除
    '        ExcelMemoryRelease(ExcelTempSheet)

    '        '保存処理実行
    '        Dim saveExcelLock As New Object
    '        SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
    '            Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
    '        End SyncLock
    '        Me.ExcelBookObj.Close(False)

    '        Return UrlRoot & tmpFileName

    '    Catch ex As Exception
    '        Throw '呼出し元にThrow
    '    Finally
    '        ExcelMemoryRelease(rngWrite)
    '    End Try

    'End Function

#End Region

#Region "ダウンロード(タンク車運賃実績表-着駅別-仙台)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(タンク車輸送実績表)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_TankTansportResult_Arr_010402(
        ByVal STYMD As Date,
        ByVal EDYMD As Date,
        ByVal type As Integer
    ) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim lastOfficeCode As String = ""
            Dim lastShippersCode As String = ""
            Dim lastBaseCode As String = ""
            Dim putRow As Integer = 0
            Dim e_idx As Integer = 1

            For dr_idx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

                Dim nrow As DataRow = PrintData.Rows(dr_idx)
                Dim srcRange As Excel.Range = Nothing
                Dim destRange As Excel.Range = Nothing

                '------------------
                ' ヘッダー出力処理
                '------------------
                If dr_idx = 0 Then '先頭レコード
                    '〇ヘッダー出力
                    EditTankTansportResult_HeaderArea_Arr_010402(e_idx, nrow, STYMD, EDYMD, type)
                ElseIf Not lastOfficeCode.Equals(nrow("OFFICECODE").ToString()) OrElse  '前行と営業所が異なる
                    Not lastBaseCode.Equals(nrow("BASECODE").ToString()) OrElse         '前行と出荷元が異なる
                    TANK_TRANSPORT_RESULT_1PAGE_DETAIL_COUNT - putRow < 3 Then          '1ページ辺りの最大行数 - 出力済み行数が4（明細行数）以下                                                    '出力済み明細数が10

                    '〇改頁処理
                    For i As Integer = putRow To TANK_TRANSPORT_RESULT_1PAGE_DETAIL_COUNT Step 1
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", e_idx))
                        srcRange.RowHeight = 12.75
                        ExcelMemoryRelease(srcRange)
                        e_idx += 1 '出力済み明細数
                    Next

                    '〇ヘッダーセルコピー
                    srcRange = ExcelTempSheet.Cells.Range("K1:DB7")
                    destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                    srcRange.Copy(destRange)
                    ExcelMemoryRelease(srcRange)
                    ExcelMemoryRelease(destRange)

                    '〇ヘッダー行高さ調整
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", e_idx, e_idx + 3))
                    srcRange.RowHeight = 15
                    ExcelMemoryRelease(srcRange)
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", e_idx + 4))
                    srcRange.RowHeight = 7.5
                    ExcelMemoryRelease(srcRange)
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", e_idx + 5, e_idx + 6))
                    srcRange.RowHeight = 12.75
                    ExcelMemoryRelease(srcRange)

                    '〇ヘッダー出力
                    EditTankTansportResult_HeaderArea_Arr_010402(e_idx, nrow, STYMD, EDYMD, type)

                    '出力済み明細数初期化
                    putRow = 0
                End If

                '--------------
                ' 明細出力処理 
                '--------------
                '〇明細セルコピー
                If putRow = 0 OrElse Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) Then

                    'ページ内で荷主が変わる場合
                    If Not putRow = 0 AndAlso
                        Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) Then
                        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", e_idx))
                        srcRange.RowHeight = 12.75
                        ExcelMemoryRelease(srcRange)
                        e_idx += 1
                        putRow += 1
                    End If

                    'テンプレート②をコピー
                    srcRange = ExcelTempSheet.Cells.Range("K9:DB11")
                    destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                    srcRange.Copy(destRange)
                    ExcelMemoryRelease(srcRange)
                    ExcelMemoryRelease(destRange)

                    '〇 荷主
                    srcRange = Me.ExcelWorkSheet.Range("B" + e_idx.ToString())
                    srcRange.Value = nrow("SHIPPERSNAME").ToString()
                    ExcelMemoryRelease(srcRange)

                Else
                    'テンプレート③をコピー
                    srcRange = ExcelTempSheet.Cells.Range("K13:DB15")
                    destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                    srcRange.Copy(destRange)
                    ExcelMemoryRelease(srcRange)
                    ExcelMemoryRelease(destRange)
                End If

                'コピーしたテンプレートにより出力インデックスを変える
                Dim eidx As Integer = e_idx
                If putRow = 0 OrElse Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) Then
                    eidx += 1
                End If

                '〇 着駅
                Dim wkArrStationName As String = nrow("ARRSTATIONNAME").ToString()
                '()（）を取り除く
                wkArrStationName = wkArrStationName.Replace("(", "")
                wkArrStationName = wkArrStationName.Replace(")", "")
                wkArrStationName = wkArrStationName.Replace("（", "")
                wkArrStationName = wkArrStationName.Replace("）", "")
                srcRange = Me.ExcelWorkSheet.Range("B" + eidx.ToString())
                srcRange.Value = wkArrStationName
                ExcelMemoryRelease(srcRange)

                '〇 荷受人
                Dim wkConsigneeName As String = nrow("CONSIGNEENAME").ToString()
                'ENEOS北信油槽所、ENEOS甲府油槽所の場合、ENEOSを取り除く
                If "10".Equals(nrow("CONSIGNEECODE").ToString()) OrElse
                    "20".Equals(nrow("CONSIGNEECODE").ToString()) Then
                    wkConsigneeName = wkConsigneeName.Replace("ENEOS", "")
                    wkConsigneeName = wkConsigneeName.Replace("ＥＮＥＯＳ", "")
                End If
                srcRange = Me.ExcelWorkSheet.Range("I" + eidx.ToString())
                srcRange.Value = wkConsigneeName
                ExcelMemoryRelease(srcRange)

                '〇明細出力ループ
                '危険品
                EditTankTansportResult_DetailArea(e_idx, PrintData.Rows(dr_idx))
                'その他
                EditTankTansportResult_DetailArea(e_idx, PrintData.Rows(dr_idx + 1))
                '計
                EditTankTansportResult_DetailArea(e_idx, PrintData.Rows(dr_idx + 2))

                lastOfficeCode = nrow("OFFICECODE").ToString()
                lastShippersCode = nrow("SHIPPERSCODE").ToString()
                lastBaseCode = nrow("BASECODE").ToString()

                'データ行index加算
                dr_idx += 3
                '出力済み明細数
                putRow += 3
            Next

            ExcelTempSheet.Delete() '雛形シート削除
            ExcelMemoryRelease(ExcelTempSheet)

            '保存処理実行
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
    ''' 帳票のヘッダー設定(タンク車輸送実績表-着駅別-仙台)
    ''' </summary>
    Private Sub EditTankTansportResult_HeaderArea_Arr_010402(
        ByRef idx As Integer,   'EXCEL行インデックス
        ByVal row As DataRow,   'データ行
        ByVal STYMD As Date,    '期間開始日
        ByVal EDYMD As Date,    '期間終了日
        ByVal type As Integer   '種別(1:往路所定 2:往路割引)
    )
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            '行加算
            idx += 3

            '◯ 出荷場所
            rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
            rngHeaderArea.Value = row("BASENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 出力期間
            rngHeaderArea = Me.ExcelWorkSheet.Range("AN" + idx.ToString())
            rngHeaderArea.Value = String.Format("{0} ～ {1}", STYMD.ToString("yyyy年 MM月 dd日"), EDYMD.ToString("yyyy年 MM月 dd日"))
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 営業所
            rngHeaderArea = Me.ExcelWorkSheet.Range("CF" + idx.ToString())
            rngHeaderArea.Value = row("OFFICENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '行加算
            idx += 3

            If type = 2 Then
                rngHeaderArea = Me.ExcelWorkSheet.Range("AO" + idx.ToString())
                rngHeaderArea.Value = "往路割引"
                ExcelMemoryRelease(rngHeaderArea)
                rngHeaderArea = Me.ExcelWorkSheet.Range("BT" + idx.ToString())
                rngHeaderArea.Value = "往路割引"
                ExcelMemoryRelease(rngHeaderArea)
            End If

            '行加算
            idx += 1

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

#End Region

#Region "ダウンロード(輸送実績表-仙台・五井以外)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(輸送実績表)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_TansportResult(
        ByVal stYmd As Date,
        ByVal edYmd As Date
    ) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim e_idx As Integer = 1        'EXCEL行INDEX

            For dr_idx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

                Dim writeDetailCnt As Integer = 0           '出力明細数                   
                Dim srcRange As Excel.Range = Nothing
                Dim destRange As Excel.Range = Nothing

                Dim lastShippersCode As String = PrintData.Rows(dr_idx)("SHIPPERSCODE").ToString()
                Dim lastBaseCode As String = PrintData.Rows(dr_idx)("BASECODE").ToString()
                Dim lastConsigneeCode As String = PrintData.Rows(dr_idx)("CONSIGNEECODE").ToString()

                '出力する明細行数のカウント
                For i As Integer = dr_idx To PrintData.Rows.Count - 1 Step 1
                    Dim nrow As DataRow = PrintData.Rows(i)

                    If Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) OrElse  '前行と荷主が異なる
                        Not lastBaseCode.Equals(nrow("BASECODE").ToString()) OrElse         '前行と出荷元が異なる
                        Not lastConsigneeCode.Equals(nrow("CONSIGNEECODE").ToString()) Then '前行と荷受人が異なる
                        Exit For
                    End If

                    writeDetailCnt += 1

                    lastShippersCode = nrow("SHIPPERSCODE").ToString()
                    lastBaseCode = nrow("BASECODE").ToString()
                    lastConsigneeCode = nrow("CONSIGNEECODE").ToString()
                Next

                '◎ヘッダー部出力処理
                If dr_idx = 0 Then
                    '〇ヘッダー出力
                    EditTansportResult_HeaderArea(e_idx, PrintData.Rows(dr_idx), stYmd, edYmd)
                End If

                '◎明細部出力
                Dim mergeStIdx As Integer = e_idx
                Dim baseTotalFlg As Boolean = False
                Dim lastBigOilCode As String = ""

                For i As Integer = 0 To writeDetailCnt - 1
                    '出力行
                    Dim prow As DataRow = PrintData.Rows(dr_idx + i)
                    If i = 0 Then
                        If "9999999".Equals(prow("ARRSTATION").ToString()) Then
                            baseTotalFlg = True
                            '〇明細部4テンプレートセルコピー
                            srcRange = ExcelTempSheet.Cells.Range("K13:BZ13")
                            destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                            '〇明細出力
                            EditTansportResult_DetailArea(e_idx, prow, 4)
                        Else
                            '〇明細部1テンプレートセルコピー
                            srcRange = ExcelTempSheet.Cells.Range("K7:BZ7")
                            destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                            '〇明細出力
                            EditTansportResult_DetailArea(e_idx, prow)
                        End If
                    ElseIf Not "9999".Equals(prow("OILCODE").ToString()) Then
                        '〇明細部2テンプレートセルコピー
                        srcRange = ExcelTempSheet.Cells.Range("K9:BZ9")
                        destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)

                        '〇罫線
                        If ("W".Equals(lastBigOilCode) AndAlso
                            "B".Equals(prow("BIGOILCODE").ToString())) OrElse
                            "8888".Equals(prow("OILCODE").ToString()) Then
                            '白油から黒油へ切り替わる場合
                            '又は出力レコードが「白(黒)油計」の場合は、明細行の上に罫線を引く
                            destRange = ExcelWorkSheet.Range(String.Format("V{0}:BO{0}", e_idx))
                            destRange.Borders(Excel.XlBordersIndex.xlEdgeTop).LineStyle = Excel.XlLineStyle.xlContinuous

                            '出力レコードが「白(黒)油計」の場合
                            If "8888".Equals(prow("OILCODE").ToString()) Then
                                '背景色を薄い青(197, 217, 241)に設定
                                destRange.Interior.Color = RGB(197, 217, 241)
                            End If

                            ExcelMemoryRelease(destRange)
                        End If

                        '〇明細出力
                        EditTansportResult_DetailArea(e_idx, prow, 2)
                    Else
                        '〇明細部3テンプレートセルコピー
                        srcRange = ExcelTempSheet.Cells.Range("K11:BZ11")
                        destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)

                        ''基地計の「計」の場合、背景色を塗りつぶしなしにする
                        'If "9999999".Equals(prow("ARRSTATION").ToString()) Then
                        '    destRange = ExcelWorkSheet.Range(String.Format("V{0}:BO{0}", eridx))
                        '    destRange.Interior.ColorIndex = 0
                        '    ExcelMemoryRelease(destRange)
                        'End If

                        '〇明細出力
                        EditTansportResult_DetailArea(e_idx, prow, 3)
                    End If
                    lastBigOilCode = prow("BIGOILCODE").ToString()
                Next

                '◎明細部のセル結合
                If baseTotalFlg Then
                    '〇基地計の結合
                    srcRange = ExcelWorkSheet.Range(String.Format("B{0}:U{1}", mergeStIdx, mergeStIdx + writeDetailCnt - 1))
                    srcRange.MergeCells = True
                    srcRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    srcRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    '下罫線を引く
                    srcRange.Borders(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous
                    ExcelMemoryRelease(srcRange)
                Else
                    '〇着駅の結合
                    srcRange = ExcelWorkSheet.Range(String.Format("B{0}:J{1}", mergeStIdx, mergeStIdx + writeDetailCnt - 1))
                    srcRange.MergeCells = True
                    srcRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    srcRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    '下罫線を引く
                    srcRange.Borders(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous
                    ExcelMemoryRelease(srcRange)
                    '〇荷受人の結合
                    srcRange = ExcelWorkSheet.Range(String.Format("K{0}:U{1}", mergeStIdx, mergeStIdx + writeDetailCnt - 1))
                    srcRange.MergeCells = True
                    srcRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    srcRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    '下罫線を引く
                    srcRange.Borders(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous
                    ExcelMemoryRelease(srcRange)
                End If

                '読み込み済み行数を加算
                dr_idx += writeDetailCnt
            Next

            ExcelTempSheet.Delete() '雛形シート削除
            ExcelMemoryRelease(ExcelTempSheet)

            '保存処理実行
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
    ''' 帳票のヘッダー設定(輸送実績表-仙台・五井以外)
    ''' </summary>
    Private Sub EditTansportResult_HeaderArea(ByRef idx As Integer, ByVal row As DataRow, ByVal stYmd As Date, ByVal edYmd As Date)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            '行加算
            idx += 1

            '◯ 出荷場所
            rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
            rngHeaderArea.Value = row("BASENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '行加算
            idx += 1

            '◯ 荷主
            rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
            rngHeaderArea.Value = row("SHIPPERSNAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 出力期間
            rngHeaderArea = Me.ExcelWorkSheet.Range("W" + idx.ToString())
            rngHeaderArea.Value = String.Format("{0} ～ {1}", stYmd.ToString("yyyy年 MM月 dd日"), edYmd.ToString("yyyy年 MM月 dd日"))
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 営業所
            rngHeaderArea = Me.ExcelWorkSheet.Range("BG" + idx.ToString())
            rngHeaderArea.Value = row("OFFICENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '行加算
            idx += 4

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(輸送実績表-仙台以外)
    ''' </summary>
    Private Sub EditTansportResult_DetailArea(ByRef idx As Integer, ByVal row As DataRow, Optional ByVal type As Integer = 1)
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            If type = 1 Then '明細部1の場合
                '◯ 着駅
                Dim wkArrStationName As String = row("ARRSTATIONNAME").ToString()
                '()（）を取り除く
                wkArrStationName = wkArrStationName.Replace("(", "")
                wkArrStationName = wkArrStationName.Replace(")", "")
                wkArrStationName = wkArrStationName.Replace("（", "")
                wkArrStationName = wkArrStationName.Replace("）", "")
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + idx.ToString())
                rngDetailArea.Value = wkArrStationName
                ExcelMemoryRelease(rngDetailArea)

                '〇 荷受人
                Dim wkConsigneeName As String = row("CONSIGNEENAME").ToString()
                'ENEOS北信油槽所、ENEOS甲府油槽所の場合、ENEOSを取り除く
                If "10".Equals(row("CONSIGNEECODE").ToString()) OrElse
                    "20".Equals(row("CONSIGNEECODE").ToString()) Then
                    wkConsigneeName = wkConsigneeName.Replace("ENEOS", "")
                    wkConsigneeName = wkConsigneeName.Replace("ＥＮＥＯＳ", "")
                End If
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + idx.ToString())
                rngDetailArea.Value = wkConsigneeName
                ExcelMemoryRelease(rngDetailArea)
            End If

            If type = 4 Then '明細部4の場合
                '◯ 着駅 +「計」
                Dim wkBaseName As String = row("BASENAME").ToString() + "計"
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + idx.ToString())
                rngDetailArea.Value = wkBaseName
                ExcelMemoryRelease(rngDetailArea)
            End If

            If Not type = 3 Then    '明細部3以外の場合
                '◯ 油種名
                rngDetailArea = Me.ExcelWorkSheet.Range("V" + idx.ToString())
                rngDetailArea.Value = row("SEGMENTOILNAME").ToString()
                ExcelMemoryRelease(rngDetailArea)
            End If

            '〇 車数(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AD" + idx.ToString())
            rngDetailArea.Value = row("DAILY_CARSNUMBER")
            ExcelMemoryRelease(rngDetailArea)

            '〇 標記屯(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AH" + idx.ToString())
            rngDetailArea.Value = row("DAILY_LOAD")
            ExcelMemoryRelease(rngDetailArea)

            '〇 数量(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AM" + idx.ToString())
            rngDetailArea.Value = row("DAILY_CARSAMOUNT")
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(累計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AT" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_CARSNUMBER")
            ExcelMemoryRelease(rngDetailArea)

            '〇 標記屯(累計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AZ" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_LOAD")
            ExcelMemoryRelease(rngDetailArea)

            '〇 数量(累計)
            rngDetailArea = Me.ExcelWorkSheet.Range("BG" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_CARSAMOUNT")
            ExcelMemoryRelease(rngDetailArea)

            '行加算
            idx += 1
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

#End Region

#Region "ダウンロード(輸送実績表-五井)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(輸送実績表)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_TansportResult_011201(
        ByVal stYmd As Date,
        ByVal edYmd As Date
    ) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim e_idx As Integer = 1                'EXCEL行INDEX
            Dim pageDetailCnt As Integer = 0        '1ページ明細数
            Dim lastOTTRANSPORTFLG As String = ""

            For dr_idx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

                Dim writeDetailCnt As Integer = 0           '出力明細数                   
                Dim srcRange As Excel.Range = Nothing
                Dim destRange As Excel.Range = Nothing

                Dim lrowShippersCode As String = PrintData.Rows(dr_idx)("SHIPPERSCODE").ToString()
                Dim lrowBaseCode As String = PrintData.Rows(dr_idx)("BASECODE").ToString()
                Dim lrowConsigneeCode As String = PrintData.Rows(dr_idx)("CONSIGNEECODE").ToString()
                Dim lrowOTTRANSPORTFLG As String = PrintData.Rows(dr_idx)("OTTRANSPORTFLG").ToString()

                '出力する明細行数のカウント
                For i As Integer = dr_idx To PrintData.Rows.Count - 1 Step 1
                    Dim nrow As DataRow = PrintData.Rows(i)

                    If Not lrowShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) OrElse      '前行と荷主が異なる
                        Not lrowBaseCode.Equals(nrow("BASECODE").ToString()) OrElse             '前行と出荷元が異なる
                        Not lrowConsigneeCode.Equals(nrow("CONSIGNEECODE").ToString()) OrElse   '前行と荷受人が異なる
                        Not lrowOTTRANSPORTFLG.Equals(nrow("OTTRANSPORTFLG").ToString()) Then   '前行とOT輸送フラグが異なる
                        Exit For
                    End If

                    writeDetailCnt += 1

                    lrowShippersCode = nrow("SHIPPERSCODE").ToString()
                    lrowBaseCode = nrow("BASECODE").ToString()
                    lrowConsigneeCode = nrow("CONSIGNEECODE").ToString()
                    lrowOTTRANSPORTFLG = nrow("OTTRANSPORTFLG").ToString()
                Next

                '◎ヘッダー部出力処理
                If dr_idx = 0 Then
                    '〇ヘッダー出力(五井)
                    EditTansportResult_HeaderArea_011201(e_idx, PrintData.Rows(dr_idx), stYmd, edYmd)
                ElseIf Not lastOTTRANSPORTFLG.Equals(PrintData.Rows(dr_idx)("OTTRANSPORTFLG").ToString()) Then
                    '〇改頁処理(五井)
                    ChangeTansportResultPage_011201(e_idx, pageDetailCnt)
                    '〇ヘッダー出力(五井)
                    EditTansportResult_HeaderArea_011201(e_idx, PrintData.Rows(dr_idx), stYmd, edYmd)
                End If

                '◎明細部出力
                Dim mergeStIdx As Integer = e_idx
                Dim baseTotalFlg As Boolean = False
                Dim lastBigOilCode As String = ""

                For i As Integer = 0 To writeDetailCnt - 1
                    '出力行
                    Dim prow As DataRow = PrintData.Rows(dr_idx + i)
                    If i = 0 Then
                        If "9999999".Equals(prow("ARRSTATION").ToString()) Then
                            baseTotalFlg = True
                            '〇明細部4テンプレートセルコピー
                            srcRange = ExcelTempSheet.Cells.Range("K14:BZ14")
                            destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                            '〇明細出力
                            EditTansportResult_DetailArea(e_idx, prow, 4)
                        Else
                            '〇明細部1テンプレートセルコピー
                            srcRange = ExcelTempSheet.Cells.Range("K8:BZ8")
                            destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                            '〇明細出力
                            EditTansportResult_DetailArea(e_idx, prow)
                        End If
                    ElseIf Not "9999".Equals(prow("OILCODE").ToString()) Then
                        '〇明細部2テンプレートセルコピー
                        srcRange = ExcelTempSheet.Cells.Range("K10:BZ10")
                        destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)

                        '〇罫線
                        If ("W".Equals(lastBigOilCode) AndAlso
                            "B".Equals(prow("BIGOILCODE").ToString())) OrElse
                            "8888".Equals(prow("OILCODE").ToString()) Then
                            '白油から黒油へ切り替わる場合
                            '又は出力レコードが「白(黒)油計」の場合は、明細行の上に罫線を引く
                            destRange = ExcelWorkSheet.Range(String.Format("V{0}:BO{0}", e_idx))
                            destRange.Borders(Excel.XlBordersIndex.xlEdgeTop).LineStyle = Excel.XlLineStyle.xlContinuous

                            '出力レコードが「白(黒)油計」の場合
                            If "8888".Equals(prow("OILCODE").ToString()) Then
                                '背景色を薄い青(197, 217, 241)に設定
                                destRange.Interior.Color = RGB(197, 217, 241)
                            End If

                            ExcelMemoryRelease(destRange)
                        End If

                        '〇明細出力
                        EditTansportResult_DetailArea(e_idx, prow, 2)
                    Else
                        '〇明細部3テンプレートセルコピー
                        srcRange = ExcelTempSheet.Cells.Range("K12:BZ12")
                        destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)

                        ''基地計の「計」の場合、背景色を塗りつぶしなしにする
                        'If "9999999".Equals(prow("ARRSTATION").ToString()) Then
                        '    destRange = ExcelWorkSheet.Range(String.Format("V{0}:BO{0}", e_idx))
                        '    destRange.Interior.ColorIndex = 0
                        '    ExcelMemoryRelease(destRange)
                        'End If

                        '〇明細出力
                        EditTansportResult_DetailArea(e_idx, prow, 3)
                    End If
                    lastBigOilCode = prow("BIGOILCODE").ToString()
                Next

                '◎明細部のセル結合
                If baseTotalFlg Then
                    '〇基地計の結合
                    srcRange = ExcelWorkSheet.Range(String.Format("B{0}:U{1}", mergeStIdx, mergeStIdx + writeDetailCnt - 1))
                    srcRange.MergeCells = True
                    srcRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    srcRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    '下罫線を引く
                    srcRange.Borders(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous
                    ExcelMemoryRelease(srcRange)
                Else
                    '〇着駅の結合
                    srcRange = ExcelWorkSheet.Range(String.Format("B{0}:J{1}", mergeStIdx, mergeStIdx + writeDetailCnt - 1))
                    srcRange.MergeCells = True
                    srcRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    srcRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    '下罫線を引く
                    srcRange.Borders(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous
                    ExcelMemoryRelease(srcRange)
                    '〇荷受人の結合
                    srcRange = ExcelWorkSheet.Range(String.Format("K{0}:U{1}", mergeStIdx, mergeStIdx + writeDetailCnt - 1))
                    srcRange.MergeCells = True
                    srcRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    srcRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter
                    '下罫線を引く
                    srcRange.Borders(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous
                    ExcelMemoryRelease(srcRange)
                End If

                '最終OT輸送フラグを保存
                lastOTTRANSPORTFLG = PrintData.Rows(dr_idx)("OTTRANSPORTFLG").ToString()
                '読み込み済み行数を加算
                dr_idx += writeDetailCnt
                'ページ明細数を加算
                pageDetailCnt += writeDetailCnt
            Next

            ExcelTempSheet.Delete() '雛形シート削除
            ExcelMemoryRelease(ExcelTempSheet)

            '保存処理実行
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
    ''' 帳票のヘッダー設定(輸送実績表-五井)
    ''' </summary>
    Private Sub EditTansportResult_HeaderArea_011201(ByRef idx As Integer, ByVal row As DataRow, ByVal stYmd As Date, ByVal edYmd As Date)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            '行加算
            idx += 2

            '◯ 出荷場所
            rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
            rngHeaderArea.Value = row("BASENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 出力期間
            rngHeaderArea = Me.ExcelWorkSheet.Range("W" + idx.ToString())
            rngHeaderArea.Value = String.Format("{0} ～ {1}", stYmd.ToString("yyyy年 MM月 dd日"), edYmd.ToString("yyyy年 MM月 dd日"))
            ExcelMemoryRelease(rngHeaderArea)

            '行加算
            idx += 1

            '◯ 荷主
            rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
            rngHeaderArea.Value = row("SHIPPERSNAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 輸送形態
            rngHeaderArea = Me.ExcelWorkSheet.Range("W" + idx.ToString())
            If "0".Equals(row("OTTRANSPORTFLG").ToString()) Then
                rngHeaderArea.Value = "全輸送計"
            ElseIf "1".Equals(row("OTTRANSPORTFLG").ToString()) Then
                rngHeaderArea.Value = "ＯＴ輸送"
            Else
                rngHeaderArea.Value = "請負輸送"
            End If
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 営業所
            rngHeaderArea = Me.ExcelWorkSheet.Range("BG" + idx.ToString())
            rngHeaderArea.Value = row("OFFICENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '行加算
            idx += 4

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    '''' <summary>
    '''' 輸送実績表(五井)改ページ処理
    '''' </summary>
    '''' <param name="eridx">EXCEL行インデックス</param>
    Private Sub ChangeTansportResultPage_011201(ByRef eridx As Integer, ByRef putDetailCnt As Integer)

        Dim srcRange As Excel.Range = Nothing
        Dim destRange As Excel.Range = Nothing

        '〇改ページ処理
        For i As Integer = putDetailCnt To TRANSPORT_RESULT_1PAGE_DETAIL_COUNT_011201 - 1
            eridx += 1  '1ページ辺りの明細行数の上限に達するまで行を進める
        Next
        '〇フッター行高さ調整
        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", eridx, eridx))
        srcRange.RowHeight = 7.5
        ExcelMemoryRelease(srcRange)
        eridx += 1
        '〇出力済み明細数を初期化する
        putDetailCnt = 0

        '〇ヘッダーテンプレートセルコピー
        srcRange = ExcelTempSheet.Cells.Range("K1:BZ7")
        destRange = ExcelWorkSheet.Range("A" + eridx.ToString())
        srcRange.Copy(destRange)
        ExcelMemoryRelease(srcRange)
        ExcelMemoryRelease(destRange)
        '〇ヘッダー行高さ調整
        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", eridx, eridx + 3))
        srcRange.RowHeight = 15
        ExcelMemoryRelease(srcRange)
        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", eridx + 4, eridx + 4))
        srcRange.RowHeight = 6
        ExcelMemoryRelease(srcRange)
        srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", eridx + 5, eridx + 6))
        srcRange.RowHeight = 15
        ExcelMemoryRelease(srcRange)
    End Sub
#End Region

#Region "ダウンロード(輸送実績表-仙台)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(輸送実績表)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_TansportResult_010402(
        ByVal stYmd As Date,
        ByVal edYmd As Date
    ) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") _
                                    & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim eridx As Integer = 1                'EXCEL行INDEX

            For idx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

                Dim nrow As DataRow = PrintData.Rows(idx)   '現在行
                Dim writeDetailCnt As Integer = 0           '出力明細数                   
                Dim srcRange As Excel.Range = Nothing
                Dim destRange As Excel.Range = Nothing

                '出力する明細行数のカウント
                For ridx As Integer = idx To PrintData.Rows.Count - 1 Step 1
                    Dim nextrow As DataRow = PrintData.Rows(ridx)
                    writeDetailCnt += 1
                    If "9999".Equals(nextrow("OILCODE").ToString()) Then
                        Exit For
                    End If
                Next

                '◎ヘッダー部出力処理
                If idx = 0 Then
                    '〇ヘッダー出力
                    EditTansportResult_HeaderArea_010402(eridx, nrow, stYmd, edYmd)
                End If

                '◎明細部出力
                Dim mergeStIdx As Integer = eridx
                Dim baseTotalFlg As Boolean = False

                For i As Integer = 0 To writeDetailCnt - 1
                    '出力行
                    Dim prow As DataRow = PrintData.Rows(idx + i)
                    If i = 0 Then
                        If "9999999".Equals(prow("ARRSTATION").ToString()) Then
                            baseTotalFlg = True
                            '〇明細部4テンプレートセルコピー
                            srcRange = ExcelTempSheet.Cells.Range("K16:CV16")
                            destRange = ExcelWorkSheet.Range("A" + eridx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                            '〇明細出力
                            EditTansportResult_DetailArea_010402(eridx, prow, 4)
                        Else
                            '〇明細部1テンプレートセルコピー
                            srcRange = ExcelTempSheet.Cells.Range("K10:CV10")
                            destRange = ExcelWorkSheet.Range("A" + eridx.ToString())
                            srcRange.Copy(destRange)
                            ExcelMemoryRelease(srcRange)
                            ExcelMemoryRelease(destRange)
                            '〇明細出力
                            EditTansportResult_DetailArea_010402(eridx, prow)
                        End If
                    ElseIf Not "9999".Equals(prow("OILCODE").ToString()) Then
                        '〇明細部2テンプレートセルコピー
                        srcRange = ExcelTempSheet.Cells.Range("K12:CV12")
                        destRange = ExcelWorkSheet.Range("A" + eridx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)
                        '〇明細出力
                        EditTansportResult_DetailArea_010402(eridx, prow, 2)
                    Else
                        '〇明細部3テンプレートセルコピー
                        srcRange = ExcelTempSheet.Cells.Range("K14:CV14")
                        destRange = ExcelWorkSheet.Range("A" + eridx.ToString())
                        srcRange.Copy(destRange)
                        ExcelMemoryRelease(srcRange)
                        ExcelMemoryRelease(destRange)
                        '〇明細出力
                        EditTansportResult_DetailArea_010402(eridx, prow, 3)
                    End If
                Next
                '読み込み済み行数を加算
                idx += writeDetailCnt

                '◎明細部のセル結合
                If baseTotalFlg Then
                    '〇基地計の結合
                    srcRange = ExcelWorkSheet.Range(String.Format("B{0}:O{1}", mergeStIdx, mergeStIdx + writeDetailCnt - 1))
                    srcRange.MergeCells = True
                    srcRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    srcRange.VerticalAlignment = Excel.XlVAlign.xlVAlignTop
                    ExcelMemoryRelease(srcRange)
                Else
                    '〇着駅の結合
                    srcRange = ExcelWorkSheet.Range(String.Format("B{0}:H{1}", mergeStIdx, mergeStIdx + writeDetailCnt - 1))
                    srcRange.MergeCells = True
                    srcRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    srcRange.VerticalAlignment = Excel.XlVAlign.xlVAlignTop
                    ExcelMemoryRelease(srcRange)
                    '〇荷受人の結合
                    srcRange = ExcelWorkSheet.Range(String.Format("I{0}:O{1}", mergeStIdx, mergeStIdx + writeDetailCnt - 1))
                    srcRange.MergeCells = True
                    srcRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter
                    srcRange.VerticalAlignment = Excel.XlVAlign.xlVAlignTop
                    ExcelMemoryRelease(srcRange)
                End If

            Next

            ExcelTempSheet.Delete() '雛形シート削除
            ExcelMemoryRelease(ExcelTempSheet)

            '保存処理実行
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
    ''' 帳票のヘッダー設定(輸送実績表-仙台)
    ''' </summary>
    Private Sub EditTansportResult_HeaderArea_010402(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByVal stYmd As Date,
        ByVal edYmd As Date
    )
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            '行加算
            idx += 2

            '◯ 出荷場所
            rngHeaderArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
            rngHeaderArea.Value = row("BASENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 出力期間
            rngHeaderArea = Me.ExcelWorkSheet.Range("AG" + idx.ToString())
            rngHeaderArea.Value = String.Format("{0} ～ {1}", stYmd.ToString("yyyy年 MM月 dd日"), edYmd.ToString("yyyy年 MM月 dd日"))
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 営業所
            rngHeaderArea = Me.ExcelWorkSheet.Range("CD" + idx.ToString())
            rngHeaderArea.Value = row("OFFICENAME")
            ExcelMemoryRelease(rngHeaderArea)

            '行加算
            idx += 6

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(輸送実績表-仙台)
    ''' </summary>
    Private Sub EditTansportResult_DetailArea_010402(
        ByRef idx As Integer,
        ByVal row As DataRow,
        Optional ByVal type As Integer = 1
    )
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            If type = 1 Then '明細部1の場合
                '◯ 着駅
                Dim wkArrStationName As String = row("ARRSTATIONNAME").ToString()
                '()（）を取り除く
                wkArrStationName = wkArrStationName.Replace("(", "")
                wkArrStationName = wkArrStationName.Replace(")", "")
                wkArrStationName = wkArrStationName.Replace("（", "")
                wkArrStationName = wkArrStationName.Replace("）", "")
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + idx.ToString())
                rngDetailArea.Value = wkArrStationName
                ExcelMemoryRelease(rngDetailArea)

                '〇 荷受人
                Dim wkConsigneeName As String = row("CONSIGNEENAME").ToString()
                'ENEOS北信油槽所、ENEOS甲府油槽所の場合、ENEOSを取り除く
                If "10".Equals(row("CONSIGNEECODE").ToString()) OrElse
                    "20".Equals(row("CONSIGNEECODE").ToString()) Then
                    wkConsigneeName = wkConsigneeName.Replace("ENEOS", "")
                    wkConsigneeName = wkConsigneeName.Replace("ＥＮＥＯＳ", "")
                End If
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + idx.ToString())
                rngDetailArea.Value = wkConsigneeName
                ExcelMemoryRelease(rngDetailArea)
            End If

            If type = 4 Then '明細部4の場合
                '◯ 着駅 +「計」
                Dim wkBaseName As String = row("BASENAME").ToString() + "計"
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + idx.ToString())
                rngDetailArea.Value = wkBaseName
                ExcelMemoryRelease(rngDetailArea)
            End If

            If Not type = 3 Then    '明細部3以外の場合
                '◯ 油種名
                rngDetailArea = Me.ExcelWorkSheet.Range("P" + idx.ToString())
                rngDetailArea.Value = row("SEGMENTOILNAME").ToString()
                ExcelMemoryRelease(rngDetailArea)
            End If

            '〇 数量(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("X" + idx.ToString())
            rngDetailArea.Value = row("DAILY_CARSAMOUNT")
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AF" + idx.ToString())
            rngDetailArea.Value = row("DAILY_CARSNUMBER")
            ExcelMemoryRelease(rngDetailArea)

            '〇 数量(累計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AJ" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_CARSAMOUNT")
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(累計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AR" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_CARSNUMBER")
            ExcelMemoryRelease(rngDetailArea)

            '〇 数量(ENEOS)
            rngDetailArea = Me.ExcelWorkSheet.Range("AW" + idx.ToString())
            If Double.Parse(row("E_MONTHLY_CARSAMOUNT").ToString()) > 0.0 Then
                rngDetailArea.Value = row("E_MONTHLY_CARSAMOUNT")
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(ENEOS)
            rngDetailArea = Me.ExcelWorkSheet.Range("BE" + idx.ToString())
            If Integer.Parse(row("E_MONTHLY_CARSNUMBER").ToString()) > 0 Then
                rngDetailArea.Value = row("E_MONTHLY_CARSNUMBER")
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 数量(他荷主)
            rngDetailArea = Me.ExcelWorkSheet.Range("BI" + idx.ToString())
            If Double.Parse(row("O_MONTHLY_CARSAMOUNT").ToString()) > 0.0 Then
                rngDetailArea.Value = row("O_MONTHLY_CARSAMOUNT")
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(他荷主)
            rngDetailArea = Me.ExcelWorkSheet.Range("BP" + idx.ToString())
            If Integer.Parse(row("O_MONTHLY_CARSNUMBER").ToString()) > 0 Then
                rngDetailArea.Value = row("O_MONTHLY_CARSNUMBER")
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 数量(コスモ)
            rngDetailArea = Me.ExcelWorkSheet.Range("BS" + idx.ToString())
            If Double.Parse(row("C_MONTHLY_CARSAMOUNT").ToString()) > 0.0 Then
                rngDetailArea.Value = row("C_MONTHLY_CARSAMOUNT")
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(コスモ)
            rngDetailArea = Me.ExcelWorkSheet.Range("BZ" + idx.ToString())
            If Integer.Parse(row("C_MONTHLY_CARSNUMBER").ToString()) > 0 Then
                rngDetailArea.Value = row("C_MONTHLY_CARSNUMBER")
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 数量(出光)
            rngDetailArea = Me.ExcelWorkSheet.Range("CC" + idx.ToString())
            If Double.Parse(row("I_MONTHLY_CARSAMOUNT").ToString()) > 0.0 Then
                rngDetailArea.Value = row("I_MONTHLY_CARSAMOUNT")
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(出光)
            rngDetailArea = Me.ExcelWorkSheet.Range("CJ" + idx.ToString())
            If Integer.Parse(row("I_MONTHLY_CARSNUMBER").ToString()) > 0 Then
                rngDetailArea.Value = row("I_MONTHLY_CARSNUMBER")
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '行加算
            idx += 1
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

#End Region

#Region "ダウンロード(科目別一覧)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(科目別一覧)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_AccountBranchBusinessIncome(ByVal KeijyoYM As String) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") _
                                    & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim e_idx As Integer = 9
            Dim srcRange As Excel.Range = Nothing
            Dim destRange As Excel.Range = Nothing

            Dim keijyoYmd As DateTime = DateTime.Parse(KeijyoYM + "/01")
            srcRange = ExcelWorkSheet.Range("AO3")
            srcRange.Value = keijyoYmd.ToString("yyyy年MM月")
            ExcelMemoryRelease(srcRange)

            '固定帳票(科目別一覧)作成処理
            For Each row As DataRow In PrintData.Rows

                If row("SEGMENTCODE").ToString.Equals("ALL") Then
                    srcRange = ExcelTempSheet.Cells.Range("A4:CN4")
                Else
                    srcRange = ExcelTempSheet.Cells.Range("A2:CN2")
                End If
                destRange = ExcelWorkSheet.Range("A" + e_idx.ToString())
                srcRange.Copy(destRange)
                ExcelMemoryRelease(srcRange)
                ExcelMemoryRelease(destRange)

                '明細出力
                EditAccountBranchBusinessIncome(e_idx, row)
                e_idx += 1
            Next

            '保存処理実行
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
    ''' 帳票の明細設定(科目別一覧)
    ''' </summary>
    Private Sub EditAccountBranchBusinessIncome(ByVal idx As Int32, ByVal row As DataRow)
        Dim rngDetailArea As Excel.Range = Nothing
        Dim total As Long = 0

        Try
            '◯ セグメントコード
            rngDetailArea = Me.ExcelWorkSheet.Range("A" + idx.ToString())
            If row("SEGMENTCODE").ToString.Equals("ALL") Then
                rngDetailArea.Value = "41?101"
            Else
                rngDetailArea.Value = String.Format("{0:000000}", row("SEGMENTCODE"))
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ セグメント名
            rngDetailArea = Me.ExcelWorkSheet.Range("F" + idx.ToString())
            rngDetailArea.Value = String.Format("{0}", row("SEGMENTNAME"))
            ExcelMemoryRelease(rngDetailArea)

            If Not row("SEGMENTCODE").ToString.Equals("ALL") Then
                '◯ 勘定科目コード
                rngDetailArea = Me.ExcelWorkSheet.Range("S" + idx.ToString())
                rngDetailArea.Value = String.Format("{0:00000000}", row("ACCOUNTCODE"))
                ExcelMemoryRelease(rngDetailArea)

                '◯ 勘定科目名
                rngDetailArea = Me.ExcelWorkSheet.Range("Y" + idx.ToString())
                rngDetailArea.Value = String.Format("{0:00000000}", row("ACCOUNTNAME"))
                ExcelMemoryRelease(rngDetailArea)
            End If

            '〇 全社計
            rngDetailArea = Me.ExcelWorkSheet.Range("AM" + idx.ToString())
            If Double.Parse(row("ALL_AM").ToString) <> 0 Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("ALL_AM"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 本社
            rngDetailArea = Me.ExcelWorkSheet.Range("AV" + idx.ToString())
            If Double.Parse(row("HONSYA_AM").ToString) <> 0 Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("HONSYA_AM"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 東北
            rngDetailArea = Me.ExcelWorkSheet.Range("BE" + idx.ToString())
            If Double.Parse(row("TOHOKU_AM").ToString) <> 0 Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("TOHOKU_AM"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 関東
            rngDetailArea = Me.ExcelWorkSheet.Range("BN" + idx.ToString())
            If Double.Parse(row("KANTO_AM").ToString) <> 0 Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("KANTO_AM"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 中部
            rngDetailArea = Me.ExcelWorkSheet.Range("BW" + idx.ToString())
            If Double.Parse(row("CHUBU_AM").ToString) <> 0 Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("CHUBU_AM"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '〇 ＭＣ
            rngDetailArea = Me.ExcelWorkSheet.Range("CF" + idx.ToString())
            If Double.Parse(row("MC_AM").ToString) <> 0 Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("MC_AM"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

#End Region

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

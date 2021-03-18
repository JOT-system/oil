﻿Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 受注個別帳票作成クラス
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

    '輸送費明細の1ページ辺りの縦長さ
    Const TRANSPORT_COST_DETAIL_1PAGE_VERTICAL_LENGTH As Double = 705.0

    'タンク車輸送実績表の1ページ辺りの縦長さ
    Const TANK_TRANSPORT_RESULT_1PAGE_VERTICAL_LENGTH As Double = 628.5

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

            If excelFileName = "OIT0008M_TRASPORT_COST_DETAIL.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("輸送費明細"), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf excelFileName = "OIT0008M_TANK_TRASPORT_RESULT.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("タンク車輸送実績表"), Excel.Worksheet)
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
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
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

            'フッターの設定
            ExcelWorkSheet.PageSetup.LeftFooter = String.Format(
                                                    "&L{0}                {1}",
                                                    Format(nowdate, "yyyy年M月d日"),
                                                    Format(nowdate, "H:mm"))
            ExcelWorkSheet.PageSetup.RightFooter = "&R&P ページ     "

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
                    pixel += 14.25
                    '2行目の高さを調整
                    ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                    idx += 1
                    pixel += 3.75

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
                    pixel += 117.75
                    '◯明細の設定
                    '値出力(全項目)
                    EditTransportCostDetail_DetailArea(idx, row)
                    'ピクセル加算
                    pixel += 14.25
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
                            pixel += 14.25

                            '空行を差し込む
                            ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                            idx += 1
                            pixel += 3.75

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
                            pixel += 14.25

                            '空行を差し込む
                            ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                            idx += 1
                            pixel += 3.75

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
                                pixel += 14.25

                                '空行を差し込む
                                ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                                idx += 1
                                pixel += 3.75
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
                            pixel += 14.25
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
                                pixel += 14.25

                                '空行を差し込む
                                ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                                idx += 1
                                pixel += 3.75
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
                                pixel += 14.25
                                '2行目の高さを調整
                                ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                                idx += 1
                                pixel += 3.75
                            Else
                                '〇改頁処理
                                ChangeTansportCostDetailPage(idx, pixel)

                                '◯ヘッダーの設定
                                '値出力
                                EditTransportCostDetail_HeaderArea(idx, row, KEIJYO_YM)
                                'ピクセル加算
                                pixel += 117.75

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
                                pixel += 14.25
                            End If
                        Else
                            '出荷場所コードが同一だが、出荷場所名に「請求先部門」が入っている場合
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
                                pixel += 14.25

                                '空行を差し込む
                                ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                                idx += 1
                                pixel += 3.75
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
                                        pixel += 14.25

                                        '空行を差し込む
                                        ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                                        idx += 1
                                        pixel += 3.75
                                    Else
                                        '前行の扱支店＝04かつ現在行の扱支店＝05(関東第2)の場合以外は改頁処理
                                        If Not ("04".Equals(lastRow("MANAGEBRANCHCODE").ToString()) And
                                        "05".Equals(row("MANAGEBRANCHCODE").ToString())) Then
                                            '〇改頁処理
                                            ChangeTansportCostDetailPage(idx, pixel)

                                            '◯ヘッダーの設定
                                            '値出力
                                            EditTransportCostDetail_HeaderArea(idx, row, KEIJYO_YM)
                                            'ピクセル加算
                                            pixel += 117.75
                                        End If

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
                                        pixel += 14.25
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
                                                pixel += 14.25

                                                '空行を差し込む
                                                ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                                                idx += 1
                                                pixel += 3.75
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
                                            pixel += 14.25
                                            '2行目の高さを調整
                                            ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                                            idx += 1
                                            pixel += 3.75

                                            '空行を差し込む
                                            ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 3.75
                                            idx += 1
                                            pixel += 3.75
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
                                            pixel += 14.25
                                        End If
                                    End If
                                    '荷受人が不一致の場合 END
                                End If
                                '扱支店が不一致の場合 END
                            End If

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
                    If wkArrStationName.Length > 4 Then
                        rngDetailArea.Value = wkArrStationName.Substring(0, 4)
                    Else
                        rngDetailArea.Value = wkArrStationName
                    End If
                Else
                    rngDetailArea.Value = ""
                End If
                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 荷受人
            If type = 0 OrElse type = 2 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + idx.ToString())
                If row("CONSIGNEENAME") IsNot DBNull.Value Then
                    If row("CONSIGNEENAME").ToString().Length > 6 Then
                        rngDetailArea.Value = row("CONSIGNEENAME").ToString().Substring(0, 6)
                    Else
                        rngDetailArea.Value = row("CONSIGNEENAME")

                    End If
                Else
                    rngDetailArea.Value = ""
                End If
                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 油種
            If type = 0 OrElse type = 1 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("L" + idx.ToString())
                If row("ORDERINGOILNAME") IsNot DBNull.Value Then
                    If row("ORDERINGOILNAME").ToString().Length > 5 Then
                        rngDetailArea.Value = row("ORDERINGOILNAME").ToString().Substring(0, 5)
                    Else
                        rngDetailArea.Value = row("ORDERINGOILNAME")
                    End If
                Else
                    rngDetailArea.Value = ""
                End If
                ExcelMemoryRelease(rngDetailArea)
            End If

            '◯ 扱支店計の場合、扱支店を出力
            If type = 5 Then
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + idx.ToString())
                If row("MANAGEBRANCHNAME").ToString().Length > 11 Then
                    rngDetailArea.Value = row("MANAGEBRANCHNAME").ToString().Substring(0, 11)
                Else
                    rngDetailArea.Value = row("MANAGEBRANCHNAME")
                End If
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
            rngDetailArea = Me.ExcelWorkSheet.Range("P" + idx.ToString())
            If row("CARSAMOUNT") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.000}", row("CARSAMOUNT"))
            Else
                rngDetailArea.Value = "0.000"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 車数
            rngDetailArea = Me.ExcelWorkSheet.Range("T" + idx.ToString())
            If row("CARSNUMBER") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("CARSNUMBER"))
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 屯数
            rngDetailArea = Me.ExcelWorkSheet.Range("W" + idx.ToString())
            If row("LOAD") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("LOAD"))
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 使用料(料率)
            rngDetailArea = Me.ExcelWorkSheet.Range("Z" + idx.ToString())
            If row("USAGE_FEE_RATE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.0}", row("USAGE_FEE_RATE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 使用料
            rngDetailArea = Me.ExcelWorkSheet.Range("AC" + idx.ToString())
            If row("USAGE_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("USAGE_FEE"))
                total += Long.Parse(row("USAGE_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 往路運賃
            rngDetailArea = Me.ExcelWorkSheet.Range("AG" + idx.ToString())
            If row("OUTBOUND_FARE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("OUTBOUND_FARE"))
                total += Long.Parse(row("OUTBOUND_FARE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 返路運賃
            rngDetailArea = Me.ExcelWorkSheet.Range("AK" + idx.ToString())
            If row("RETURN_FARE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("RETURN_FARE"))
                total += Long.Parse(row("RETURN_FARE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 運転科入換料(単価)
            rngDetailArea = Me.ExcelWorkSheet.Range("AO" + idx.ToString())
            If row("DRIVE_FEE_UPRICE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.00}", row("DRIVE_FEE_UPRICE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 運転科入換料
            rngDetailArea = Me.ExcelWorkSheet.Range("AR" + idx.ToString())
            If row("DRIVE_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("DRIVE_FEE"))
                total += Long.Parse(row("DRIVE_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 業務科管理料(単価)
            rngDetailArea = Me.ExcelWorkSheet.Range("AV" + idx.ToString())
            If row("BUSINESS_FEE_UPRICE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.00}", row("BUSINESS_FEE_UPRICE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 業務科管理料
            rngDetailArea = Me.ExcelWorkSheet.Range("AY" + idx.ToString())
            If row("BUSINESS_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("BUSINESS_FEE"))
                total += Long.Parse(row("BUSINESS_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 取扱料(単価)
            rngDetailArea = Me.ExcelWorkSheet.Range("BC" + idx.ToString())
            If row("HANDLING_FEE_UPRICE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.00}", row("HANDLING_FEE_UPRICE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 取扱料
            rngDetailArea = Me.ExcelWorkSheet.Range("BF" + idx.ToString())
            If row("HANDLING_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("HANDLING_FEE"))
                total += Long.Parse(row("HANDLING_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ OT運賃手数料(単価)
            rngDetailArea = Me.ExcelWorkSheet.Range("BJ" + idx.ToString())
            If row("OT_FARE_FEE_UPRICE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0.00}", row("OT_FARE_FEE_UPRICE"))
            Else
                rngDetailArea.Value = ""
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ OT運賃手数料
            rngDetailArea = Me.ExcelWorkSheet.Range("BM" + idx.ToString())
            If row("OT_FARE_FEE") IsNot DBNull.Value Then
                rngDetailArea.Value = String.Format("{0:#,##0}", row("OT_FARE_FEE"))
                total += Long.Parse(row("OT_FARE_FEE").ToString(), Globalization.NumberStyles.Number)
            Else
                rngDetailArea.Value = "0"
            End If
            ExcelMemoryRelease(rngDetailArea)

            '◯ 積卸料(単価)
            rngDetailArea = Me.ExcelWorkSheet.Range("BQ" + idx.ToString())
            rngDetailArea.Value = ""
            ExcelMemoryRelease(rngDetailArea)

            '◯ 積卸料
            rngDetailArea = Me.ExcelWorkSheet.Range("BT" + idx.ToString())
            rngDetailArea.Value = "0"
            ExcelMemoryRelease(rngDetailArea)

            '◯ 計
            rngDetailArea = Me.ExcelWorkSheet.Range("BX" + idx.ToString())
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
        '出力済みPixel数が最大に達してない場合、ページ埋め処理
        While (pixel < TRANSPORT_COST_DETAIL_1PAGE_VERTICAL_LENGTH)
            '明細1行分(14.25)以上
            If TRANSPORT_COST_DETAIL_1PAGE_VERTICAL_LENGTH - pixel > 14.25 Then
                '高さの調整のみ
                ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight = 14.25
                pixel += 14.25
            Else
                '1行以下（フッター行）の場合、MAX - 出力済みPixel数分の高さにして、下罫線を引く
                ExcelWorkSheet.Range(String.Format("{0}:{0}", idx)).RowHeight =
                    TRANSPORT_COST_DETAIL_1PAGE_VERTICAL_LENGTH - pixel
                ExcelWorkSheet.Range(String.Format("B{0}:CA{0}", idx)) _
                .Borders(Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Excel.XlLineStyle.xlContinuous

                pixel += TRANSPORT_COST_DETAIL_1PAGE_VERTICAL_LENGTH - pixel
            End If
            idx += 1
        End While

        '出力済みPixcel数をリセット
        pixel = 0

        '最終行の場合はヘッダーテンプレートコピー処理をせずに終了
        If type = 1 Then Exit Sub

        'テンプレートのコピー
        Dim srcRange As Excel.Range = ExcelTempSheet.Cells.Range("I1:CJ11")
        Dim destRange As Excel.Range = ExcelWorkSheet.Range("A" + idx.ToString())
        srcRange.Copy(destRange)
        ExcelMemoryRelease(srcRange)
        ExcelMemoryRelease(destRange)
        '行の高さ設定
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx))).RowHeight = 14.25
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 1))).RowHeight = 15.75
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 2))).RowHeight = 15.75
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 3))).RowHeight = 14.25
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 4))).RowHeight = 3.75
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 5))).RowHeight = 14.25
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 6))).RowHeight = 3.75
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 7))).RowHeight = 3.75
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 8))).RowHeight = 14.25
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 9))).RowHeight = 14.25
        ExcelWorkSheet.Range(String.Format("{0}:{0}", (idx + 10))).RowHeight = 3.75
    End Sub
#End Region

#Region "ダウンロード(タンク車輸送実績表)"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(タンク車輸送実績表)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData_TankTansportResult(ByVal STYMD As Date, ByVal EDYMD As Date) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim lastOfficeCode As String = ""
            Dim lastShippersCode As String = ""
            Dim lastBaseCode As String = ""
            Dim lastConsigneeCode As String = ""
            Dim lastTrainNo As String = ""
            Dim putDetail As Integer = 0
            Dim idx As Integer = 1

            For ridx As Integer = 0 To PrintData.Rows.Count - 1 Step 0

                Dim nrow As DataRow = PrintData.Rows(ridx)
                Dim srcRange As Excel.Range = Nothing
                Dim destRange As Excel.Range = Nothing

                '◎ヘッダー出力処理
                If ridx = 0 Then                                                           '先頭レコード
                    '〇ヘッダー出力
                    EditTankTansportResult_HeaderArea(idx, nrow, STYMD, EDYMD)
                ElseIf Not lastOfficeCode.Equals(nrow("OFFICECODE").ToString()) OrElse     '前行と営業所が異なる
                    Not lastShippersCode.Equals(nrow("SHIPPERSCODE").ToString()) OrElse    '前行と荷主が異なる
                    Not lastBaseCode.Equals(nrow("BASECODE").ToString()) OrElse            '前行と出荷元が異なる
                    Not lastConsigneeCode.Equals(nrow("CONSIGNEECODE").ToString()) OrElse  '前行と荷受人が異なる
                    putDetail = 2 Then                                                     '出力済み明細数が2

                    '〇改頁処理
                    If putDetail = 1 Then
                        idx += 22 '明細1つ分＋2行飛ばす
                    Else
                        idx += 2 '2行飛ばす
                    End If
                    'フッター行の高さ調整
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx))
                    srcRange.RowHeight = 3
                    ExcelMemoryRelease(srcRange)
                    idx += 1

                    '〇ヘッダーセルコピー
                    srcRange = ExcelTempSheet.Cells.Range("K1:DB7")
                    destRange = ExcelWorkSheet.Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    ExcelMemoryRelease(srcRange)
                    ExcelMemoryRelease(destRange)

                    '〇ヘッダー行高さ調整
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx, idx + 3))
                    srcRange.RowHeight = 15
                    ExcelMemoryRelease(srcRange)
                    srcRange = Me.ExcelWorkSheet.Range(String.Format("{0}:{0}", idx + 4, idx + 4))
                    srcRange.RowHeight = 4.5
                    ExcelMemoryRelease(srcRange)

                    '〇ヘッダー出力
                    EditTankTansportResult_HeaderArea(idx, nrow, STYMD, EDYMD)

                    '出力済み明細数初期化
                    putDetail = 0
                End If
                '◎明細出力処理
                '〇明細セルコピー
                If "9999".Equals(nrow("TRAINNO").ToString()) Then
                    '荷受人計の場合、テンプレート④をコピー
                    srcRange = ExcelTempSheet.Cells.Range("K51:DB70")
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
                    If putDetail = 0 Then
                        '出力明細数0の場合、テンプレート②をコピー
                        srcRange = ExcelTempSheet.Cells.Range("K9:DB28")
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
                        srcRange = ExcelTempSheet.Cells.Range("K30:DB49")
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
                '〇明細出力ループ
                For i As Integer = 0 To 4 Step 1
                    '揮発
                    EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx))
                    '灯軽
                    EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 1))
                    '黒油
                    EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 2))
                    '計
                    EditTankTansportResult_DetailArea(idx, PrintData.Rows(ridx + 3))
                    'データ行index加算
                    ridx += 4
                Next

                lastOfficeCode = nrow("OFFICECODE").ToString()
                lastShippersCode = nrow("SHIPPERSCODE").ToString()
                lastBaseCode = nrow("BASECODE").ToString()
                lastConsigneeCode = nrow("CONSIGNEECODE").ToString()

                '出力済み明細数
                putDetail += 1
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
    Private Sub EditTankTansportResult_HeaderArea(ByRef idx As Integer, ByVal row As DataRow, ByVal STYMD As Date, ByVal EDYMD As Date)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            '行加算
            idx += 2

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
            rngHeaderArea = Me.ExcelWorkSheet.Range("AN" + idx.ToString())
            rngHeaderArea.Value = String.Format("{0} ～ {1}", STYMD.ToString("yyyy年 MM月 dd日"), EDYMD.ToString("yyyy年 MM月 dd日"))
            ExcelMemoryRelease(rngHeaderArea)

            '◯ 営業所
            rngHeaderArea = Me.ExcelWorkSheet.Range("CF" + idx.ToString())
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
    ''' 帳票の明細設定(タンク車輸送実績表)
    ''' </summary>
    Private Sub EditTankTansportResult_DetailArea(ByRef idx As Integer, ByVal row As DataRow)
        Dim rngDetailArea As Excel.Range = Nothing
        Dim total As Long = 0

        Try
            '〇 数量(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AC" + idx.ToString())
            rngDetailArea.Value = row("DAILY_CARSAMOUNT")
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AH" + idx.ToString())
            rngDetailArea.Value = row("DAILY_CARSNUMBER")
            ExcelMemoryRelease(rngDetailArea)

            '〇 標屯(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AK" + idx.ToString())
            rngDetailArea.Value = row("DAILY_LOAD")
            ExcelMemoryRelease(rngDetailArea)

            '〇 運屯(日計)
            Dim dailyLoad As Double = Double.Parse(row("DAILY_LOAD").ToString())
            Dim dailyCarsNumber As Integer = Integer.Parse(row("DAILY_CARSNUMBER").ToString())
            rngDetailArea = Me.ExcelWorkSheet.Range("AN" + idx.ToString())
            rngDetailArea.Value = dailyLoad - (2.0 * dailyCarsNumber)
            ExcelMemoryRelease(rngDetailArea)

            '〇 往路所定(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AQ" + idx.ToString())
            rngDetailArea.Value = row("DAILY_OUTBOUND")
            ExcelMemoryRelease(rngDetailArea)

            '〇 返路所定(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("AV" + idx.ToString())
            rngDetailArea.Value = row("DAILY_RETURN")
            ExcelMemoryRelease(rngDetailArea)

            '〇 往返計(日計)
            Dim dailyOutBound As Double = Double.Parse(row("DAILY_OUTBOUND").ToString())
            Dim dailyReturn As Double = Double.Parse(row("DAILY_RETURN").ToString())
            rngDetailArea = Me.ExcelWorkSheet.Range("BA" + idx.ToString())
            rngDetailArea.Value = dailyOutBound + dailyReturn
            ExcelMemoryRelease(rngDetailArea)

            '〇 数量(月計)
            rngDetailArea = Me.ExcelWorkSheet.Range("BF" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_CARSAMOUNT")
            ExcelMemoryRelease(rngDetailArea)

            '〇 車数(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("BL" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_CARSNUMBER")
            ExcelMemoryRelease(rngDetailArea)

            '〇 標屯(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("BO" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_LOAD")
            ExcelMemoryRelease(rngDetailArea)

            '〇 運屯(日計)
            Dim monthlyLoad As Double = Double.Parse(row("MONTHLY_LOAD").ToString())
            Dim monthlyCarsNumber As Integer = Integer.Parse(row("MONTHLY_CARSNUMBER").ToString())
            rngDetailArea = Me.ExcelWorkSheet.Range("BS" + idx.ToString())
            rngDetailArea.Value = monthlyLoad - (2.0 * monthlyCarsNumber)
            ExcelMemoryRelease(rngDetailArea)

            '〇 往路所定(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("BW" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_OUTBOUND")
            ExcelMemoryRelease(rngDetailArea)

            '〇 返路所定(日計)
            rngDetailArea = Me.ExcelWorkSheet.Range("CD" + idx.ToString())
            rngDetailArea.Value = row("MONTHLY_RETURN")
            ExcelMemoryRelease(rngDetailArea)

            '〇 往返計(日計)
            Dim monthlyOutBound As Double = Double.Parse(row("MONTHLY_OUTBOUND").ToString())
            Dim monthlyReturn As Double = Double.Parse(row("MONTHLY_RETURN").ToString())
            rngDetailArea = Me.ExcelWorkSheet.Range("CK" + idx.ToString())
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

    '''' <summary>
    '''' タンク車輸送実績表改頁処理
    '''' </summary>
    '''' <param name="idx">行インデックス</param>
    '''' <param name="pixel">出力済みPixel数</param>
    'Private Sub ChangeTankTansportResultPage(ByRef idx As Int32, ByRef pixel As Double, Optional type As Int32 = 0)

    'End Sub
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

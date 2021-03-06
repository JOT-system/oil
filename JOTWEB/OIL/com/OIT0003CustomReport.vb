﻿Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 受注個別帳票作成クラス
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）</remarks>
Public Class OIT0003CustomReport : Implements IDisposable
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
    Private PrintData As DataTable
    Private xlProcId As Integer

    Private KinoeneYusoujyoName As String = "OIREC(大阪国際石油精製)"

    Private CMNPTS As New CmnParts                                  '共通関数

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
            If excelFileName = "OIT0003L_DELIVERYPLAN.xlsx" _
                OrElse excelFileName = "OIT0003D_DELIVERYPLAN.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("運送状"), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf excelFileName = "OIT0003L_LOADPLAN.xlsx" _
                OrElse excelFileName = "OIT0003L_OTLOADPLAN.xlsx" _
                OrElse excelFileName = "OIT0003L_YOKKAICHI_LOADPLAN.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("積込指示書"), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf excelFileName = "OIT0003L_MIESHIOHAMA_LOADPLAN.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("積込指示書"), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("出荷数量(昭四分)"), Excel.Worksheet)
            ElseIf excelFileName = "OIT0003L_NEGISHI_SHIPPLAN.xlsx" _
                OrElse excelFileName = "OIT0003L_GOI_SHIPPLAN.xlsx" _
                OrElse excelFileName = "OIT0003L_KINOENE_SHIPPLAN.xlsx" _
                OrElse excelFileName = "OIT0003L_SODEGAURA_SHIPPLAN.xlsx" _
                OrElse excelFileName = "OIT0003L_MIESHIOHAMA_SHIPCONTACT.xlsx" _
                OrElse excelFileName = "OIT0003L_MIESHIOHAMA_SHIPPLAN.xlsx" _
                OrElse excelFileName = "OIT0003L_YOKKAICHI_SHIPPLAN.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("入出力画面"), Excel.Worksheet)
                'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf excelFileName = "OIT0003L_GOI_FILLINGPOINT.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("五井明細データ"), Excel.Worksheet)
                'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf excelFileName = "OIT0003L_NEGISHI_LOADPLAN.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("回線別積込"), Excel.Worksheet)
                'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf excelFileName = "OIT0003L_SODEGAURA_LINEPLAN_401.xlsx" _
                OrElse excelFileName = "OIT0003L_SODEGAURA_LINEPLAN_501.xlsx" _
                OrElse excelFileName = "OIT0003L_SODEGAURA_LINEPLAN.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("入線方"), Excel.Worksheet)
                'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            ElseIf excelFileName = "OIT0003L_KINOENE_LOADPLAN.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("出力"), Excel.Worksheet)
                'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            End If
        Catch ex As Exception
            If Me.xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try

    End Sub

#Region "ダウンロード(積込指示書(共通))"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(積込指示書(共通))URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData(ByVal tyohyoType As String, ByVal officeCode As String, Optional ByVal lodDate As String = Nothing) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"

        '○帳票名取得
        Dim tmpGetFileName As String = CMNPTS.SetReportFileName(tyohyoType, officeCode, lodDate, "")
        If tmpGetFileName <> "" Then
            tmpFileName = tmpGetFileName
        End If

        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Select Case tyohyoType
                '固定帳票(積込予定(共通))作成処理
                Case "LOADPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditLoadHeaderArea(lodDate, officeCode)
                    '◯明細の設定
                    EditLoadDetailArea(officeCode)
                    '***** TODO処理 ここまで *****
                    ExcelTempSheet.Delete() '雛形シート削除
                    ExcelMemoryRelease(ExcelTempSheet)
                '固定帳票(OT積込予定(共通))作成処理
                Case "OTLOADPLAN"
                    '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditOTLoadHeaderArea(lodDate)
                    '◯明細の設定
                    EditOTLoadDetailArea(officeCode, lodDate)
                    '***** TODO処理 ここまで *****
                    ExcelTempSheet.Delete() '雛形シート削除
                    ExcelMemoryRelease(ExcelTempSheet)
                    '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################

            End Select

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
            End SyncLock
            Me.ExcelBookObj.Close(False)

            ''ストリーム生成
            'Using fs As New IO.FileStream(tmpFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
            '    Dim binaryLength = Convert.ToInt32(fs.Length)
            '    ReDim retByte(binaryLength)
            '    fs.Read(retByte, 0, binaryLength)
            '    fs.Flush()
            'End Using
            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        Finally
            ExcelMemoryRelease(rngWrite)
        End Try

    End Function

    ''' <summary>
    ''' 帳票のヘッダー設定(積込指示書(共通))
    ''' </summary>
    Private Sub EditLoadHeaderArea(ByVal lodDate As String, ByVal officeCode As String)
        Dim rngHeaderArea As Excel.Range = Nothing
        'Dim value As String = Now.AddDays(1).ToString("yyyy年MM月dd日（ddd）", New Globalization.CultureInfo("ja-JP"))

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 基地名
                rngHeaderArea = Me.ExcelWorkSheet.Range("B1")
                If officeCode = BaseDllConst.CONST_OFFICECODE_011202 Then
                    rngHeaderArea.Value = KinoeneYusoujyoName
                Else
                    rngHeaderArea.Value = PrintDatarow("BASENAME")
                End If
                ExcelMemoryRelease(rngHeaderArea)
                ''◯ 積込日
                'Dim value As String = PrintDatarow("LODDATE").ToString
                'rngHeaderArea = Me.ExcelWorkSheet.Range("E1")
                'rngHeaderArea.Value = Date.Parse(value).ToString("MM月dd日分", New Globalization.CultureInfo("ja-JP"))

                Exit For
            Next

            '◯ 積込日
            Dim value As String = lodDate
            rngHeaderArea = Me.ExcelWorkSheet.Range("E1")
            rngHeaderArea.Value = Date.Parse(value).ToString("MM月dd日分", New Globalization.CultureInfo("ja-JP"))
            ExcelMemoryRelease(rngHeaderArea)
            '◯ 作成日(当日)
            rngHeaderArea = Me.ExcelWorkSheet.Range("P1")
            'rngHeaderArea = Me.ExcelWorkSheet.Range("O1")
            rngHeaderArea.Value = Now.AddDays(0).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
            ExcelMemoryRelease(rngHeaderArea)

            '★不要項目を非表示
            Select Case officeCode
                '○五井営業所
                Case BaseDllConst.CONST_OFFICECODE_011201
                    Me.ExcelWorkSheet.Range("F:F").Columns.Hidden = True
                    Me.ExcelWorkSheet.Range("G:G").Columns.Hidden = True
                    Me.ExcelWorkSheet.Range("K:K").Columns.Hidden = True
                '○甲子営業所
                Case BaseDllConst.CONST_OFFICECODE_011202
                    Me.ExcelWorkSheet.Range("E:E").ColumnWidth = 15.25
                    Me.ExcelWorkSheet.Range("F:F").Columns.Hidden = True
                    Me.ExcelWorkSheet.Range("G:G").Columns.Hidden = True
                '○袖ヶ浦営業所
                Case BaseDllConst.CONST_OFFICECODE_011203
                    Me.ExcelWorkSheet.Range("F:F").Columns.Hidden = True
                    Me.ExcelWorkSheet.Range("G:G").Columns.Hidden = True
                '○四日市営業所
                Case BaseDllConst.CONST_OFFICECODE_012401
                    Me.ExcelWorkSheet.Range("F:F").Columns.Hidden = True
                    Me.ExcelWorkSheet.Range("L:L").Columns.Hidden = True
                    'Me.ExcelWorkSheet.Range("N:N").Columns.Hidden = True
                    Me.ExcelWorkSheet.Range("O:O").Columns.Hidden = True
            End Select

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(積込指示書(共通))
    ''' </summary>
    Private Sub EditLoadDetailArea(ByVal officeCode As String)
        Dim rngDetailArea As Excel.Range = Nothing
        Dim rngTmp As Excel.Range = Nothing
        Dim rngSummary As Excel.Range = Nothing
        Dim strTrainNoSave As String = ""
        Dim strTrainNameSave As String = ""
        Dim strTotalTankSave As String = ""
        Dim strOTTransportSave As String = ""
        Dim blnNewLine As Boolean = False

        Try
            Dim i As Integer = 5
            Dim iTotalCnt As Integer = 0
            For Each PrintDatarow As DataRow In PrintData.Rows

                '★ 五井営業所の場合のみ合計車数を表示
                If officeCode = BaseDllConst.CONST_OFFICECODE_011201 AndAlso strTrainNameSave <> "" Then
                    '○前回の列車名と今回の列車名が不一致
                    If strTrainNameSave <> PrintDatarow("TRAINNAME").ToString() Then
                        blnNewLine = True

                        '○8883列車でOT⇒請負になった時点で合計を表示
                    ElseIf strTrainNoSave = "8883" AndAlso PrintDatarow("TRAINNO").ToString() = "8883" _
                           AndAlso strOTTransportSave <> PrintDatarow("OTTRANSPORTFLG").ToString() Then
                        blnNewLine = True

                        '○8877列車でOT⇒請負になった時点で合計を表示
                    ElseIf strTrainNoSave = "8877" AndAlso PrintDatarow("TRAINNO").ToString() = "8877" _
                           AndAlso strOTTransportSave <> PrintDatarow("OTTRANSPORTFLG").ToString() Then
                        blnNewLine = True

                    End If

                    '★合計表示対象の場合
                    If blnNewLine = True Then
                        '★tmpシートより合計行をコピーして値を設定
                        rngSummary = Me.ExcelTempSheet.Range("B1:P1")
                        rngTmp = Me.ExcelWorkSheet.Range("B" + i.ToString(), "P" + i.ToString())
                        'rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
                        rngSummary.Copy(rngTmp)
                        ExcelMemoryRelease(rngSummary)
                        ExcelMemoryRelease(rngTmp)
                        '◯ 合計車数
                        rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                        rngDetailArea.Value = Convert.ToString(iTotalCnt) + "両"
                        'rngDetailArea.Value = strTotalTankSave + "両"
                        ExcelMemoryRelease(rngDetailArea)
                        i += 1
                        iTotalCnt = 0
                        blnNewLine = False
                    End If

                End If

                '◯ No
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = PrintDatarow("LINECNT")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 荷主
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPPERSNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 着駅
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ARRSTATIONNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 荷受人
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONSIGNEENAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 積込回数
                '### 出力項目（空白） #####################################
                '◯ 積込ポイント
                '### 出力項目（空白） #####################################

                '    ★四日市営業所以外の場合
                If officeCode <> BaseDllConst.CONST_OFFICECODE_012401 Then
                    '◯ 油種
                    rngDetailArea = Me.ExcelWorkSheet.Range("H" + i.ToString())
                    rngDetailArea.Value = PrintDatarow("ORDERINGOILNAME")
                    ExcelMemoryRelease(rngDetailArea)
                    '◯ 型式
                    rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                    rngDetailArea.Value = PrintDatarow("MODEL")
                    ExcelMemoryRelease(rngDetailArea)
                    '◯ 車番
                    rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
                    rngDetailArea.Value = PrintDatarow("TANKNUMBER")
                    ExcelMemoryRelease(rngDetailArea)

                    '★四日市営業所の場合
                Else
                    '◯ 油種
                    rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
                    rngDetailArea.Value = PrintDatarow("ORDERINGOILNAME")
                    ExcelMemoryRelease(rngDetailArea)
                    '◯ 型式
                    rngDetailArea = Me.ExcelWorkSheet.Range("H" + i.ToString())
                    rngDetailArea.Value = PrintDatarow("MODEL")
                    ExcelMemoryRelease(rngDetailArea)
                    '◯ 車番
                    rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                    rngDetailArea.Value = PrintDatarow("TANKNUMBER")
                    ExcelMemoryRelease(rngDetailArea)
                End If

                '◯ 予約数量
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                rngDetailArea.Value = PrintDatarow("RESERVEAMOUNT")
                ExcelMemoryRelease(rngDetailArea)
                ''◯ 交検
                'rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("JRINSPECTIONDATE")
                'ExcelMemoryRelease(rngDetailArea)
                '◯ 積置
                rngDetailArea = Me.ExcelWorkSheet.Range("L" + i.ToString())
                rngDetailArea.Value = PrintDatarow("STACKING").ToString().Replace("　", "")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 列車№
                rngDetailArea = Me.ExcelWorkSheet.Range("M" + i.ToString())
                If officeCode = BaseDllConst.CONST_OFFICECODE_011201 Then
                    rngDetailArea.Value = PrintDatarow("OTTRAINNO")
                Else
                    rngDetailArea.Value = PrintDatarow("TRAINNO")
                End If
                ExcelMemoryRelease(rngDetailArea)

                '    ★四日市営業所以外の場合
                If officeCode <> BaseDllConst.CONST_OFFICECODE_012401 Then
                    '◯ 発日(予定)
                    rngDetailArea = Me.ExcelWorkSheet.Range("N" + i.ToString())
                    rngDetailArea.Value = PrintDatarow("DEPDATE")
                    ExcelMemoryRelease(rngDetailArea)
                    ''### 20201203 START 指摘票対応(No247) ######################
                    '○ 受入日(予定)
                    rngDetailArea = Me.ExcelWorkSheet.Range("O" + i.ToString())
                    rngDetailArea.Value = PrintDatarow("ACCDATE")
                    ExcelMemoryRelease(rngDetailArea)
                    ''### 20201203 END   指摘票対応(No247) ######################

                    '★四日市営業所の場合
                Else
                    '◯ 連結順序
                    rngDetailArea = Me.ExcelWorkSheet.Range("N" + i.ToString())
                    rngDetailArea.Value = PrintDatarow("SHIPORDER")
                    ExcelMemoryRelease(rngDetailArea)
                End If

                '◯ 備考
                '### 20201014 START 備考欄への表示対応 #####################
                rngDetailArea = Me.ExcelWorkSheet.Range("P" + i.ToString())
                'rngDetailArea = Me.ExcelWorkSheet.Range("O" + i.ToString())
                Dim Remark As String = ""
                '★ジョイント
                If PrintDatarow("JOINT").ToString <> "" Then
                    Remark = "『" + PrintDatarow("JOINT").ToString + "』"
                End If
                ''★積込
                'If PrintDatarow("STACKING").ToString <> "" Then
                '    Remark &= "『" + PrintDatarow("STACKING").ToString + "』"
                'End If
                '★交検
                If PrintDatarow("INSPECTION").ToString <> "" Then
                    Remark &= "『" + PrintDatarow("INSPECTION").ToString + "』"
                End If
                '★格上
                If PrintDatarow("UPGRADE").ToString <> "" Then
                    Remark &= "『" + PrintDatarow("UPGRADE").ToString + "(端切)" + "』"
                    'Remark &= "『" + PrintDatarow("UPGRADE").ToString + "（端切）" + "』"
                End If

                '### 20201105 START 指摘票対応(No210)全体 ##################
                '★「５０％運行分」※甲子営業所(2685列車)対応
                If officeCode = BaseDllConst.CONST_OFFICECODE_011202 _
                    AndAlso Convert.ToString(PrintDatarow("TRAINNO")) = "2685" Then
                    If Remark = "" Then
                        Remark &= "「５０％運行分」"
                    Else
                        Remark &= vbCrLf + "「５０％運行分」"
                    End If
                End If
                '### 20201105 END   指摘票対応(No210)全体 ##################

                '★五井営業所
                If officeCode = BaseDllConst.CONST_OFFICECODE_011201 Then
                    '★「請負」倉賀野向け列車(8883, 8877列車)対応※請負のみ
                    If (Convert.ToString(PrintDatarow("TRAINNO")) = "8883" _
                             OrElse Convert.ToString(PrintDatarow("TRAINNO")) = "8877") _
                        AndAlso PrintDatarow("OTTRANSPORTFLG").ToString() = "2" Then
                        If Remark = "" Then
                            Remark &= "「請負」"
                        Else
                            Remark &= vbCrLf + "「請負」"
                        End If
                        '★「請負」南松本向け列車(2081, 5972, 9672列車)対応
                    ElseIf Convert.ToString(PrintDatarow("TRAINNO")) = "2081" _
                             OrElse Convert.ToString(PrintDatarow("TRAINNO")) = "5972" _
                             OrElse Convert.ToString(PrintDatarow("TRAINNO")) = "9672" Then
                        If Remark = "" Then
                            Remark &= "「請負」"
                        Else
                            Remark &= vbCrLf + "「請負」"
                        End If
                    End If
                End If

                '★備考
                If PrintDatarow("REMARK").ToString <> "" Then
                    If Remark = "" Then
                        Remark &= PrintDatarow("REMARK").ToString
                    Else
                        Remark &= vbCrLf + PrintDatarow("REMARK").ToString
                    End If
                End If
                rngDetailArea.Value = Remark
                ExcelMemoryRelease(rngDetailArea)
                '### 20201014 END   備考欄への表示対応 #####################

                '★ 列車名・合計車数を退避
                strTrainNoSave = PrintDatarow("TRAINNO").ToString()
                strTrainNameSave = PrintDatarow("TRAINNAME").ToString()
                strTotalTankSave = PrintDatarow("TOTALTANK").ToString()
                strOTTransportSave = PrintDatarow("OTTRANSPORTFLG").ToString()

                i += 1
                iTotalCnt += 1
            Next

            '★ 五井営業所の場合のみ合計車数を表示
            If officeCode = BaseDllConst.CONST_OFFICECODE_011201 Then
                '★tmpシートより合計行をコピーして値を設定
                rngSummary = Me.ExcelTempSheet.Range("B1:P1")
                rngTmp = Me.ExcelWorkSheet.Range("B" + i.ToString(), "O" + i.ToString())
                'rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
                rngSummary.Copy(rngTmp)
                ExcelMemoryRelease(rngSummary)
                ExcelMemoryRelease(rngTmp)
                '◯ 合計車数
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                rngDetailArea.Value = Convert.ToString(iTotalCnt) + "両"
                'rngDetailArea.Value = strTotalTankSave + "両"
                ExcelMemoryRelease(rngDetailArea)
            End If

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(OT積込指示書(共通))
    ''' </summary>
    Private Sub EditOTLoadHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing
        'Dim value As String = Now.AddDays(1).ToString("yyyy年MM月dd日（ddd）", New Globalization.CultureInfo("ja-JP"))

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 基地名
                rngHeaderArea = Me.ExcelWorkSheet.Range("B1")
                rngHeaderArea.Value = PrintDatarow("BASENAME")
                ExcelMemoryRelease(rngHeaderArea)
                '◯ 積込日
                Dim value As String = Date.Parse(lodDate).ToString("MM月dd日分", New Globalization.CultureInfo("ja-JP")).ToString()
                value &= "　" + PrintDatarow("TRAINNO").ToString() + "列車"
                rngHeaderArea = Me.ExcelWorkSheet.Range("G1")
                rngHeaderArea.Value = value
                ExcelMemoryRelease(rngHeaderArea)
                Exit For
            Next

            ''◯ 積込日
            'Dim value As String = lodDate
            'rngHeaderArea = Me.ExcelWorkSheet.Range("G1")
            'rngHeaderArea.Value = Date.Parse(value).ToString("MM月dd日分", New Globalization.CultureInfo("ja-JP"))
            'ExcelMemoryRelease(rngHeaderArea)
            ''◯ 作成日(当日)
            'rngHeaderArea = Me.ExcelWorkSheet.Range("O1")
            'rngHeaderArea.Value = Now.AddDays(0).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
            'ExcelMemoryRelease(rngHeaderArea)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(OT積込指示書(共通))
    ''' </summary>
    Private Sub EditOTLoadDetailArea(ByVal officeCode As String, ByVal lodDate As String)
        Dim rngDetailArea As Excel.Range = Nothing
        Dim rngTmp As Excel.Range = Nothing
        Dim rngSummary As Excel.Range = Nothing
        Dim strTrainNameSave As String = ""
        Dim strTotalTankSave As String = ""

        Try
            '### 20201112 START 行数追加に伴い開始行を変更 ##########
            'Dim z() As Integer = {5, 32, 59, 86}
            Dim z() As Integer = {5, 49, 93, 137}
            '### 20201112 END   行数追加に伴い開始行を変更 ##########
            Dim j As Integer = 0
            Dim i As Integer = z(j)
            Dim lineNo As Integer = 1
            For Each PrintDatarow As DataRow In PrintData.Rows

                '★ 前回の列車名と今回の列車名が不一致
                If strTrainNameSave <> "" _
                    AndAlso strTrainNameSave <> PrintDatarow("TRAINNAME").ToString() Then

                    ''★tmpシートより合計行をコピーして値を設定
                    'rngSummary = Me.ExcelTempSheet.Range("A1:H3")
                    'rngTmp = Me.ExcelWorkSheet.Range("A" + i.ToString(), "H" + (i + 2).ToString())
                    ''rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
                    'rngTmp.PageBreak = Excel.XlPageBreak.xlPageBreakManual
                    'rngSummary.Copy(rngTmp)

                    j += 1
                    i = z(j)
                    '◯ヘッダーの設定
                    '◯ 基地名
                    rngDetailArea = Me.ExcelWorkSheet.Range("B" + (i - 4).ToString())
                    rngDetailArea.Value = PrintDatarow("BASENAME")
                    ExcelMemoryRelease(rngDetailArea)
                    '◯ 積込日
                    Dim value As String = Date.Parse(lodDate).ToString("MM月dd日分", New Globalization.CultureInfo("ja-JP")).ToString()
                    value &= "　" + PrintDatarow("TRAINNO").ToString() + "列車"
                    rngDetailArea = Me.ExcelWorkSheet.Range("G" + (i - 4).ToString())
                    rngDetailArea.Value = value
                    ExcelMemoryRelease(rngDetailArea)
                    'i += 3
                    lineNo = 1
                End If

                '◯ No
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("LINECNT")
                rngDetailArea.Value = lineNo
                ExcelMemoryRelease(rngDetailArea)
                '◯ 荷主
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPPERSNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 着駅
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ARRSTATIONNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 荷受人
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONSIGNEENAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 油種
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ORDERINGOILNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 車番
                rngDetailArea = Me.ExcelWorkSheet.Range("G" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TANKNUMBER")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 備考
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + i.ToString())
                Dim Remark As String = ""
                '### 20201209 START OT積込指示書(翌月発送対応) #########################
                '★翌月発送
                If PrintDatarow("NEXTMONTH").ToString <> "" Then
                    Remark &= "『" + PrintDatarow("NEXTMONTH").ToString + "』"
                End If
                '### 20201209 END   OT積込指示書(翌月発送対応) #########################
                '★ジョイント
                If PrintDatarow("JOINT").ToString <> "" Then
                    Remark &= "『" + PrintDatarow("JOINT").ToString + "』"
                End If
                '★積込
                If PrintDatarow("STACKING").ToString <> "" Then
                    '郡山向け５０９０列車については『積置』の記載は不要
                    If PrintDatarow("TRAINNO").ToString <> "5090" Then
                        '★翌月発送については『積置』の記載は不要
                        If PrintDatarow("NEXTMONTH").ToString = "" Then
                            Remark &= "『" + PrintDatarow("STACKING").ToString + "』"
                        End If
                    End If
                End If
                '★交検
                If PrintDatarow("INSPECTION").ToString <> "" Then
                    Remark &= "『" + PrintDatarow("INSPECTION").ToString + "』"
                End If
                '★留置
                If PrintDatarow("DETENTION").ToString <> "" Then
                    Remark &= "『" + PrintDatarow("DETENTION").ToString + "』"
                End If
                '★格上
                If PrintDatarow("UPGRADE").ToString <> "" Then
                    Remark &= "『" + PrintDatarow("UPGRADE").ToString + "』"
                    'Remark &= "『" + PrintDatarow("UPGRADE").ToString + "（端切）" + "』"
                End If
                '★備考
                If PrintDatarow("REMARK").ToString <> "" Then
                    If Remark = "" Then
                        Remark &= PrintDatarow("REMARK").ToString
                    Else
                        Remark &= vbCrLf + PrintDatarow("REMARK").ToString
                    End If
                End If
                rngDetailArea.Value = Remark
                ExcelMemoryRelease(rngDetailArea)
                '★ 列車名・合計車数を退避
                strTrainNameSave = PrintDatarow("TRAINNAME").ToString()
                strTotalTankSave = PrintDatarow("TOTALTANK").ToString()

                i += 1
                lineNo += 1
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

#End Region

#Region "ダウンロード(出荷予定表(五井))"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(出荷予定表(五井))URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintGoiData(ByVal repPtn As String, ByVal lodDate As String) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"

        '○帳票名取得
        Dim tmpGetFileName As String = CMNPTS.SetReportFileName(repPtn, BaseDllConst.CONST_OFFICECODE_011201, lodDate, "")
        If tmpGetFileName <> "" Then
            tmpFileName = tmpGetFileName
        End If

        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Select Case repPtn
                '充填ポイント表
                Case "FILLINGPOINT"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditGoiFillingPointHeaderArea(lodDate)
                    '◯明細の設定
                    EditGoiFillingPointDetailArea()
                    '***** TODO処理 ここまで *****
                    'ExcelTempSheet.Delete() '雛形シート削除
                Case Else
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditGoiShipHeaderArea(lodDate)
                    '◯明細の設定
                    EditGoiShipDetailArea()
                    '***** TODO処理 ここまで *****
                    'ExcelTempSheet.Delete() '雛形シート削除
            End Select

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
    ''' 帳票のヘッダー設定(充填ポイント表(五井))
    ''' </summary>
    Private Sub EditGoiFillingPointHeaderArea(ByVal lodDate As String)
        'Dim rngHeaderArea As Excel.Range = Nothing

        'Try

        'Catch ex As Exception
        '    Throw
        'Finally
        '    ExcelMemoryRelease(rngHeaderArea)
        'End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(充填ポイント表(五井))
    ''' </summary>
    Private Sub EditGoiFillingPointDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim i As Integer = 3

            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 積込日
                rngDetailArea = Me.ExcelWorkSheet.Range("A" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("LODDATE")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 入線列車
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("LOADINGIRILINETRAINNO")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 積込車数
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("TOTALCNT")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 回線
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("LINE")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 充填ポイント
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("LOADINGPOINT")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 油種
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("OILCODE")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 油種区分
                rngDetailArea = Me.ExcelWorkSheet.Range("G" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("ORDERINGTYPE")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 車型
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("MODEL")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 車番
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("TANKNO")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 着駅
                rngDetailArea = Me.ExcelWorkSheet.Range("J" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("ARRSTATION")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 着駅名
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("ARRSTATIONNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 発列車
                rngDetailArea = Me.ExcelWorkSheet.Range("L" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("TRAINNO")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 発列車名
                rngDetailArea = Me.ExcelWorkSheet.Range("M" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("TRAINNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 所在
                rngDetailArea = Me.ExcelWorkSheet.Range("N" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("RETURNDATETRAIN")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 出荷口
                rngDetailArea = Me.ExcelWorkSheet.Range("R" + Convert.ToString(i))
                rngDetailArea.Value = PrintDatarow("SHIPPINGGATE")
                ExcelMemoryRelease(rngDetailArea)
                i += 1
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(出荷予定表(五井))
    ''' </summary>
    Private Sub EditGoiShipHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 積込日
                'Dim value As String = PrintDatarow("LODDATE").ToString
                Dim value As String = lodDate
                rngHeaderArea = Me.ExcelWorkSheet.Range("D3")
                rngHeaderArea.Value = Date.Parse(value).ToString("yyyy", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)
                rngHeaderArea = Me.ExcelWorkSheet.Range("F3")
                rngHeaderArea.Value = Date.Parse(value).ToString("MM", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)
                rngHeaderArea = Me.ExcelWorkSheet.Range("H3")
                rngHeaderArea.Value = Date.Parse(value).ToString("dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)
                Exit For
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(出荷予定表(五井))
    ''' </summary>
    Private Sub EditGoiShipDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing
        Dim svTrainNo As String = ""
        Dim strYoko As String() = {"F", "G", "H", "I", "J", "K", "L", "N", "O", "P", "Q", "R"}
        Dim iOilCnt As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim iYoko As Integer = 0

        Try
            Dim i As Integer = 8
            For Each PrintDatarow As DataRow In PrintData.Rows

                '★列車(着駅)が変更となった場合
                If svTrainNo <> "" AndAlso svTrainNo <> PrintDatarow("OTTRAINNO").ToString() Then
                    '行を１つ下に移動
                    i += 1
                    '油種数を初期化
                    iOilCnt = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                End If

                '油種が未設定の場合は次のデータへ
                If PrintDatarow("OILCODE").ToString() = "" Then
                    '★列車番号を退避
                    svTrainNo = PrintDatarow("OTTRAINNO").ToString()
                    Continue For
                End If

                Select Case PrintDatarow("REPORTOILNAME").ToString()
                    '◯白油 
                    '　HI-G(ハイオク)
                    Case BaseDllConst.CONST_COSMO_HIG
                        iYoko = 0
                    '　RE-G(レギュラー)
                    Case BaseDllConst.CONST_COSMO_REG
                        iYoko = 1
                    '　WKO(灯油)
                    Case BaseDllConst.CONST_COSMO_WKO
                        iYoko = 2
                    '　DGO(軽油)
                    Case BaseDllConst.CONST_COSMO_DGO
                        iYoko = 3
                    '　DGO.10(軽油１０)
                    Case BaseDllConst.CONST_COSMO_DGO10
                        iYoko = 4
                    '　DGO.3(３号軽油)
                    Case BaseDllConst.CONST_COSMO_DGO3
                        iYoko = 5
                    '　DGO.5(軽油５)
                    Case BaseDllConst.CONST_COSMO_DGO5
                        iYoko = 6

                    '◯黒油
                    '　LA-1(LSA-1, LSA-1(山岳))
                    Case BaseDllConst.CONST_COSMO_LSA
                        iYoko = 7
                    '　LAブ(LSA-ブレンド, LSA-ブレンド(山岳))
                    Case BaseDllConst.CONST_COSMO_LSABU
                        iYoko = 8
                    '　AFO(AFO, AFO(山岳))
                    Case BaseDllConst.CONST_COSMO_AFO
                        iYoko = 9
                    '　A-SP(AFOーSP, AFOーSP(山岳))
                    Case BaseDllConst.CONST_COSMO_AFOSP
                        iYoko = 10
                    '　A(ブ(AFOーブレンド(山岳))
                    Case BaseDllConst.CONST_COSMO_AFOBU
                        iYoko = 11

                    Case Else
                        Continue For
                End Select

                '★帳票に値を設定
                rngDetailArea = Me.ExcelWorkSheet.Range(strYoko(iYoko) + i.ToString())
                iOilCnt(iYoko) += Integer.Parse(Convert.ToString(PrintDatarow("CNT")))
                rngDetailArea.Value = iOilCnt(iYoko)
                ExcelMemoryRelease(rngDetailArea)
                '★列車番号を退避
                svTrainNo = PrintDatarow("OTTRAINNO").ToString()

            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

#End Region

#Region "ダウンロード(出荷予定、積込予定表(甲子))"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(出荷予定、積込予定表(甲子))URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintKinoeneData(ByVal repPtn As String, ByVal lodDate As String) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"

        '○帳票名取得
        Dim tmpGetFileName As String = CMNPTS.SetReportFileName(repPtn, BaseDllConst.CONST_OFFICECODE_011202, lodDate, "")
        If tmpGetFileName <> "" Then
            tmpFileName = tmpGetFileName
        End If

        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Select Case repPtn
                '出荷予定(甲子)
                Case "SHIPPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditKinoeneShipHeaderArea(lodDate)
                    '◯明細の設定
                    EditkinoeneShipDetailArea()
                    '***** TODO処理 ここまで *****
                    'ExcelTempSheet.Delete() '雛形シート削除

                '積込予定(甲子)
                Case "KINOENE_LOADPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditKinoeneLineHeaderArea(lodDate)
                    '◯明細の設定
                    EditKinoeneLineDetailArea()
                    '***** TODO処理 ここまで *****
                    'ExcelTempSheet.Delete() '雛形シート削除
            End Select

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
    ''' 帳票のヘッダー設定(出荷予定表(甲子))
    ''' </summary>
    Private Sub EditKinoeneShipHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 出荷日（積込日）
                Dim value As String = lodDate

                '◯ 作成日
                rngHeaderArea = Me.ExcelWorkSheet.Range("AG1")
                rngHeaderArea.Value = Date.Now.ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)

                '　月月日
                rngHeaderArea = Me.ExcelWorkSheet.Range("D5")
                rngHeaderArea.Value = Date.Parse(value).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)

                '　ADO3TCH出荷期間（FROM）
                rngHeaderArea = Me.ExcelWorkSheet.Range("B20")
                rngHeaderArea.Value = PrintDatarow("ORDERFROMDATE").ToString()
                ExcelMemoryRelease(rngHeaderArea)

                '　ADO3TCH出荷期間（TO）
                rngHeaderArea = Me.ExcelWorkSheet.Range("B21")
                rngHeaderArea.Value = PrintDatarow("ORDERTODATE").ToString()
                ExcelMemoryRelease(rngHeaderArea)
                Exit For
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(出荷予定表(甲子))
    ''' </summary>
    Private Sub EditKinoeneShipDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing
        Dim svTrainNo As String = ""
        Dim svShippersCode As String = ""
        Dim strYoko As String() = {"E", "H", "K", "N", "Q", "T", "Z", "AC"}
        Dim iOilCnt As Integer() = {0, 0, 0, 0, 0, 0, 0, 0}
        Dim iTate As Integer() = {8, 9, 10, 11, 12, 13}
        Dim iYoko As Integer = 0

        Try
            Dim i As Integer = 8
            For Each PrintDatarow As DataRow In PrintData.Rows

                '★列車(着駅)、荷主が変更となった場合
                If svTrainNo <> "" AndAlso
                   (svTrainNo <> PrintDatarow("OTTRAINNO").ToString() OrElse
                    svShippersCode <> PrintDatarow("SHIPPERSCODE").ToString()) Then
                    '行を１つ下に移動
                    i += 1
                    '油種数を初期化
                    iOilCnt = {0, 0, 0, 0, 0, 0, 0, 0}
                End If

                '油種が未設定の場合は次のデータへ
                If PrintDatarow("OILCODE").ToString() = "" Then
                    '★列車番号を退避
                    svTrainNo = PrintDatarow("OTTRAINNO").ToString()
                    svShippersCode = PrintDatarow("SHIPPERSCODE").ToString()
                    Continue For
                End If

                '荷主の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPPERSNAME").ToString()
                ExcelMemoryRelease(rngDetailArea)
                'OT列車番号の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("OTTRAINNO").ToString()
                ExcelMemoryRelease(rngDetailArea)
                '荷受人の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONSIGNEENAME").ToString()
                ExcelMemoryRelease(rngDetailArea)

                Select Case PrintDatarow("OILCODE").ToString()
                    '◯白油 
                    '　GPRE(ハイオク)
                    Case BaseDllConst.CONST_HTank
                        iYoko = 0
                    '　GREG(レギュラー)
                    Case BaseDllConst.CONST_RTank
                        iYoko = 1
                    '　KER(灯油)
                    Case BaseDllConst.CONST_TTank
                        iYoko = 2
                    '　ADO(軽油)
                    Case BaseDllConst.CONST_KTank1
                        iYoko = 3
                    '　ADO3(３号軽油)
                    Case BaseDllConst.CONST_K3Tank1
                        iYoko = 4
                    '　ADO3TCH(３号軽油＋灯油)
                    Case BaseDllConst.CONST_K3Tank2
                        iYoko = 5

                    '◯黒油
                    '　FOA10(Ａ重油)
                    Case BaseDllConst.CONST_ATank
                        iYoko = 6
                    '　FOA01(ＬＳＡ)
                    Case BaseDllConst.CONST_LTank1
                        iYoko = 7

                    Case Else
                        Continue For
                End Select

                '★帳票に値を設定
                rngDetailArea = Me.ExcelWorkSheet.Range(strYoko(iYoko) + i.ToString())
                iOilCnt(iYoko) += Integer.Parse(Convert.ToString(PrintDatarow("CNT")))
                rngDetailArea.Value = iOilCnt(iYoko)
                ExcelMemoryRelease(rngDetailArea)
                '★列車番号、荷主を退避
                svTrainNo = PrintDatarow("OTTRAINNO").ToString()
                svShippersCode = PrintDatarow("SHIPPERSCODE").ToString()
            Next

            '空白行の削除（合計が0（ゼロ）の行を削除する）
            For rowCnt As Integer = iTate(iTate.Count - 1) To iTate(0) Step -1
                rngDetailArea = Me.ExcelWorkSheet.Range("AI" & rowCnt)
                If rngDetailArea.Value.ToString = "0" Then
                    rngDetailArea.EntireRow.Delete()
                End If
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(積込予定表(甲子))
    ''' </summary>
    Private Sub EditKinoeneLineHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows

                '◯ 積込日
                Dim value As String = lodDate
                '　月
                rngHeaderArea = Me.ExcelWorkSheet.Range("J3")
                rngHeaderArea.Value = Date.Parse(value).ToString("MM", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)
                '　日
                rngHeaderArea = Me.ExcelWorkSheet.Range("K3")
                rngHeaderArea.Value = Date.Parse(value).ToString("dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)
                Exit For
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(積込予定表(甲子))
    ''' </summary>
    Private Sub EditKinoeneLineDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim i As Integer = 0
            Dim iST As Integer = 0
            Dim i1ST As Integer = 6
            Dim i2ST As Integer = 17
            Dim i3ST As Integer = 28
            Dim i4ST As Integer = 39
            Dim sPointy As String = ""
            Dim sOdd() As String = {"B", "C", "D", "E", "F"}
            Dim sEven() As String = {"H", "I", "J", "K", "L"}
            Dim sPointx() As String = {"", "", "", "", ""}
            For Each PrintDatarow As DataRow In PrintData.Rows

                If sPointy = "" OrElse sPointy <> PrintDatarow("LOADINGPOINT").ToString() Then

                    '積込回線により列の箇所を設定
                    Select Case PrintDatarow("LOADINGPOINT").ToString()
                        '★[Ｘ－１回目]
                        Case "1", "3", "5", "7"
                            sPointx = sOdd
                        '★[Ｘ－２回目]
                        Case "2", "4", "6", "8"
                            sPointx = sEven
                    End Select

                    '積込回線により行の箇所を設定
                    Select Case PrintDatarow("LOADINGPOINT").ToString()
                        '★[１－１回目], [１－２回目]
                        Case "1", "2"
                            iST = i1ST
                        '★[２－１回目], [２－２回目]
                        Case "3", "4"
                            iST = i2ST
                        '★[３－１回目], [３－２回目]
                        Case "5", "6"
                            iST = i3ST
                        '★[４－１回目], [４－２回目]
                        Case "7", "8"
                            iST = i4ST
                    End Select
                End If

                '★スポットの位置を設定
                i = iST + (Integer.Parse(PrintDatarow("SPOTNO").ToString()) - 1)
                '◯ 注
                'rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(0) + iST.ToString())
                rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(0) + i.ToString())
                rngDetailArea.Value = PrintDatarow("ATTENTION")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 車両番号
                'rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(1) + iST.ToString())
                rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(1) + i.ToString())
                rngDetailArea.Value = PrintDatarow("SYARYONUMBER")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 油種名
                'rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(2) + iST.ToString())
                rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(2) + i.ToString())
                rngDetailArea.Value = PrintDatarow("REPORTOILNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 予約数量
                'rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(3) + iST.ToString())
                rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(3) + i.ToString())
                rngDetailArea.Value = PrintDatarow("RESERVEDQUANTITY")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 納入先
                'rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(4) + iST.ToString())
                rngDetailArea = Me.ExcelWorkSheet.Range(sPointx(4) + i.ToString())
                rngDetailArea.Value = PrintDatarow("DELIVERYFIRST")
                ExcelMemoryRelease(rngDetailArea)

                'iST += 1
                sPointy = PrintDatarow("LOADINGPOINT").ToString()

            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub
#End Region

#Region "ダウンロード(入線方(袖ヶ浦))"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(入線方(袖ヶ浦))URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintSodegauraData(ByVal repPtn As String, ByVal lodDate As String, ByVal rTrainNo As String, Optional ByVal bSameTimeLine As Boolean = False) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"

        '○帳票名取得
        Dim tmpGetFileName As String = CMNPTS.SetReportFileName(repPtn, BaseDllConst.CONST_OFFICECODE_011203, lodDate, rTrainNo)
        If tmpGetFileName <> "" Then
            tmpFileName = tmpGetFileName
        End If

        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            '○帳票の種類
            Select Case repPtn
                '出荷予定(袖ケ浦)
                Case "SHIPPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditSodegauraShipHeaderArea(lodDate)
                    '◯明細の設定
                    EditSodegauraShipDetailArea()
                    '***** TODO処理 ここまで *****
                    'ExcelTempSheet.Delete() '雛形シート削除

                '★入線方(袖ヶ浦)
                Case "LINEPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditSodegauraLineHeaderArea(lodDate, rTrainNo, bSameTimeLine)
                    '◯明細の設定
                    EditSodegauraLineDetailArea()
                    '◯フッターの設定
                    EditSodegauraLineFooterArea(rTrainNo, bSameTimeLine)
                    '***** TODO処理 ここまで *****
                    'ExcelTempSheet.Delete() '雛形シート削除
            End Select

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
    ''' 帳票のヘッダー設定(出荷予定表(袖ヶ浦))
    ''' </summary>
    Private Sub EditSodegauraShipHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 出荷日（積込日）
                Dim value As String = lodDate

                '◯ 作成日
                rngHeaderArea = Me.ExcelWorkSheet.Range("AG1")
                rngHeaderArea.Value = Date.Now.ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)

                '　月月日
                rngHeaderArea = Me.ExcelWorkSheet.Range("D5")
                rngHeaderArea.Value = Date.Parse(value).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)
                Exit For
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(出荷予定表(袖ヶ浦))
    ''' </summary>
    Private Sub EditSodegauraShipDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing
        Dim svTrainNo As String = ""
        Dim svShippersCode As String = ""
        Dim svConsigneeCode As String = ""
        Dim strYoko As String() = {"E", "H", "K", "N", "Q", "W", "Z", "AC"}
        Dim iOilCnt As Integer() = {0, 0, 0, 0, 0, 0, 0, 0}
        Dim iTate As Integer() = {8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22}
        Dim iYoko As Integer = 0

        Try
            Dim i As Integer = 8
            For Each PrintDatarow As DataRow In PrintData.Rows

                '★列車(着駅)、荷主が変更となった場合
                If svTrainNo <> "" AndAlso
                   (svTrainNo <> PrintDatarow("OTTRAINNO").ToString() OrElse
                    svConsigneeCode <> PrintDatarow("CONSIGNEECODE").ToString() OrElse
                    svShippersCode <> PrintDatarow("SHIPPERSCODE").ToString()) Then

                    If PrintDatarow("CONSIGNEECODE").ToString() = "30" Then
                        'LTA出荷期間コウショウ高崎（FROM）
                        rngDetailArea = Me.ExcelWorkSheet.Range("B29")
                        rngDetailArea.Value = PrintDatarow("ORDERFROMDATE").ToString()
                        'LTA出荷期間コウショウ高崎（TO）
                        rngDetailArea = Me.ExcelWorkSheet.Range("B30")
                        rngDetailArea.Value = PrintDatarow("ORDERTODATE").ToString()
                    End If

                    If PrintDatarow("CONSIGNEECODE").ToString() = "40" Then
                        'LTA出荷期間JONET松本（FROM）
                        rngDetailArea = Me.ExcelWorkSheet.Range("B32")
                        rngDetailArea.Value = PrintDatarow("ORDERFROMDATE").ToString()
                        'LTA出荷期間JONET松本（TO）
                        rngDetailArea = Me.ExcelWorkSheet.Range("B33")
                        rngDetailArea.Value = PrintDatarow("ORDERTODATE").ToString()
                    End If

                    If PrintDatarow("CONSIGNEECODE").ToString() = "54" Then
                        'LTA出荷期間OT高崎（FROM）
                        rngDetailArea = Me.ExcelWorkSheet.Range("B35")
                        rngDetailArea.Value = PrintDatarow("ORDERFROMDATE").ToString()
                        'LTA出荷期間OT高崎（TO）
                        rngDetailArea = Me.ExcelWorkSheet.Range("B36")
                        rngDetailArea.Value = PrintDatarow("ORDERTODATE").ToString()
                    End If

                    '行を１つ下に移動
                    i += 1
                    '油種数を初期化
                    iOilCnt = {0, 0, 0, 0, 0, 0, 0, 0}
                End If

                '油種が未設定の場合は次のデータへ
                If PrintDatarow("OILCODE").ToString() = "" Then
                    '★列車番号を退避
                    svTrainNo = PrintDatarow("OTTRAINNO").ToString()
                    svConsigneeCode = PrintDatarow("CONSIGNEECODE").ToString()
                    svShippersCode = PrintDatarow("SHIPPERSCODE").ToString()
                    Continue For
                End If

                '荷主の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPPERSNAME").ToString()
                ExcelMemoryRelease(rngDetailArea)
                'OT列車番号の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("OTTRAINNO").ToString()
                ExcelMemoryRelease(rngDetailArea)
                '荷受人の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONSIGNEENAME").ToString()
                ExcelMemoryRelease(rngDetailArea)

                Select Case PrintDatarow("OILCODE").ToString()
                    '◯白油 
                    '　プレミアム(ハイオク)
                    Case BaseDllConst.CONST_HTank
                        iYoko = 0
                    '　レギュラー
                    Case BaseDllConst.CONST_RTank
                        iYoko = 1
                    '　灯油
                    Case BaseDllConst.CONST_TTank
                        iYoko = 2
                    '　軽油
                    Case BaseDllConst.CONST_KTank1
                        iYoko = 3
                    '　３号軽油
                    Case BaseDllConst.CONST_K3Tank1
                        iYoko = 4

                    '◯黒油
                    Case BaseDllConst.CONST_ATank
                        If PrintDatarow("ORDERINGTYPE").ToString() = "C" Then
                            '　LTA
                            iYoko = 5
                        Else
                            '　0.5A重油
                            iYoko = 6
                        End If
                    '　0.1A重油
                    Case BaseDllConst.CONST_LTank1
                        iYoko = 7

                    Case Else
                        Continue For
                End Select

                '★帳票に値を設定
                rngDetailArea = Me.ExcelWorkSheet.Range(strYoko(iYoko) + i.ToString())
                iOilCnt(iYoko) += Integer.Parse(Convert.ToString(PrintDatarow("CNT")))
                rngDetailArea.Value = iOilCnt(iYoko)
                ExcelMemoryRelease(rngDetailArea)
                '★列車番号、荷主を退避
                svTrainNo = PrintDatarow("OTTRAINNO").ToString()
                svConsigneeCode = PrintDatarow("CONSIGNEECODE").ToString()
                svShippersCode = PrintDatarow("SHIPPERSCODE").ToString()
            Next

            '空白行の削除（合計が0（ゼロ）の行を削除する）
            For rowCnt As Integer = iTate(iTate.Count - 1) To iTate(0) Step -1
                rngDetailArea = Me.ExcelWorkSheet.Range("AI" & rowCnt)
                If rngDetailArea.Value.ToString = "0" Then
                    rngDetailArea.EntireRow.Delete()
                End If
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(入線方(袖ヶ浦))
    ''' </summary>
    Private Sub EditSodegauraLineHeaderArea(ByVal lodDate As String, ByVal rTrainNo As String, ByVal bSameTimeLine As Boolean)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 現日付
                rngHeaderArea = Me.ExcelWorkSheet.Range("L1")
                rngHeaderArea.Value = Now.ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)
                '◯ 積込日
                'Dim value As String = PrintDatarow("ACTUALLODDATE").ToString
                Dim value As String = PrintDatarow("LODDATE").ToString
                '　月
                rngHeaderArea = Me.ExcelWorkSheet.Range("E6")
                rngHeaderArea.Value = Date.Parse(value).ToString("MM", New Globalization.CultureInfo("ja-JP")) + "月"
                ExcelMemoryRelease(rngHeaderArea)
                '　日
                rngHeaderArea = Me.ExcelWorkSheet.Range("F6")
                rngHeaderArea.Value = Date.Parse(value).ToString("dd", New Globalization.CultureInfo("ja-JP")) + "日"
                ExcelMemoryRelease(rngHeaderArea)
                '　曜日
                rngHeaderArea = Me.ExcelWorkSheet.Range("G6")
                rngHeaderArea.Value = Date.Parse(value).ToString("(ddd)", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)
                '◯ 入線列車名(臨海鉄道)
                rngHeaderArea = Me.ExcelWorkSheet.Range("I6")
                rngHeaderArea.Value = PrintDatarow("LOADINGIRILINETRAINNAME")
                ExcelMemoryRelease(rngHeaderArea)
                '◯ 入線列車No(臨海鉄道)
                rngHeaderArea = Me.ExcelWorkSheet.Range("C10")
                rngHeaderArea.Value = PrintDatarow("LOADINGIRILINETRAINNO")
                ExcelMemoryRelease(rngHeaderArea)
                '◯ 出線列車No(臨海鉄道)
                rngHeaderArea = Me.ExcelWorkSheet.Range("F10")
                rngHeaderArea.Value = PrintDatarow("LOADINGOUTLETTRAINNO")
                '★ 入線方(501同時入線タイプ)の場合
                If Convert.ToString(PrintDatarow("LOADINGIRILINETRAINNO")) = BaseDllConst.CONST_RTRAIN_I01_501_011203 _
                    AndAlso bSameTimeLine = True Then
                    rngHeaderArea.Value = BaseDllConst.CONST_RTRAIN_O02_404_011203
                End If
                ExcelMemoryRelease(rngHeaderArea)
                '★501専用入線方の場合
                If rTrainNo = BaseDllConst.CONST_RTRAIN_I01_501_011203 AndAlso bSameTimeLine = False Then
                    '◯ 受入日
                    rngHeaderArea = Me.ExcelWorkSheet.Range("M17")
                    rngHeaderArea.Value = PrintDatarow("ACCDATE")
                    ExcelMemoryRelease(rngHeaderArea)

                    If Convert.ToString(PrintDatarow("JRTRAINNO1")) = "9672" Then
                        rngHeaderArea = Me.ExcelWorkSheet.Range("F24")
                        rngHeaderArea.Value = PrintDatarow("JRTRAINNO1")
                        ExcelMemoryRelease(rngHeaderArea)
                    End If

                End If

                Exit For
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(入線方(袖ヶ浦))
    ''' </summary>
    Private Sub EditSodegauraLineDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim i As Integer = 12
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 発送列車
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = PrintDatarow("JRTRAINNO1")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 入線順
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("LOADINGIRILINEORDER")
                rngDetailArea.Value = PrintDatarow("NYUSENNO")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 油種名
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + i.ToString())
                rngDetailArea.Value = PrintDatarow("REPORTOILNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 車両番号
                rngDetailArea = Me.ExcelWorkSheet.Range("G" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CARSNUMBER")
                ExcelMemoryRelease(rngDetailArea)
                '◯ ☆入荷記
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                rngDetailArea.Value = PrintDatarow("NYUUKA")
                ExcelMemoryRelease(rngDetailArea)
                '◯ OT順位
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("LOADINGOUTLETORDER")
                rngDetailArea.Value = PrintDatarow("OTRANK")
                ExcelMemoryRelease(rngDetailArea)
                i += 1
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のフッター設定(入線方(袖ヶ浦))
    ''' </summary>
    Private Sub EditSodegauraLineFooterArea(ByVal rTrainNo As String, ByVal bSameTimeLine As Boolean)
        Dim rngFooterArea As Excel.Range = Nothing

        Try
            '荷受人(比較用)
            Dim svConsigneeCode As String = ""
            '開始行
            Dim j As Integer = 48               '401専用入線方
            If rTrainNo = BaseDllConst.CONST_RTRAIN_I01_501_011203 _
                AndAlso bSameTimeLine = False Then j = 26     '501専用入線方
            Dim i As Integer = j

            '★油種合計(列)
            Dim clnTrain() As String = {"C", "E", "G", "I", "K", "M"}
            Dim svTrain As String = ""
            '★油種数(HR, RG, 灯油, 軽油, 3号軽油, LSA, A重油)
            Dim oilCnt() As Integer = {0, 0, 0, 0, 0, 0, 0}
            For Each PrintDatarow As DataRow In PrintData.Select(Nothing, "JRTRAINNO1, OILCODE")

                '### ２列車存在する場合の対応
                If svConsigneeCode <> "" AndAlso svConsigneeCode <> PrintDatarow("CONSIGNEECODE").ToString() Then
                    '◯荷受人
                    Select Case svConsigneeCode
                    '# JONET松本
                        Case BaseDllConst.CONST_CONSIGNEECODE_40
                            svTrain = clnTrain(5)                               '401専用入線方
                            If rTrainNo = BaseDllConst.CONST_RTRAIN_I01_501_011203 _
                                AndAlso bSameTimeLine = False Then svTrain = clnTrain(2)      '501専用入線方
                    '# OT宇都宮
                        Case BaseDllConst.CONST_CONSIGNEECODE_53
                            svTrain = clnTrain(4)
                        Case Else
                            svTrain = ""
                    End Select

                    '# 油種合計用の荷受人の場合
                    If svTrain <> "" Then
                        '◯HR
                        rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                        rngFooterArea.Value = oilCnt(0)
                        ExcelMemoryRelease(rngFooterArea)
                        i += 1
                        '◯RG
                        rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                        rngFooterArea.Value = oilCnt(1)
                        i += 1
                        '◯灯油
                        rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                        rngFooterArea.Value = oilCnt(2)
                        ExcelMemoryRelease(rngFooterArea)
                        i += 1
                        '◯軽油
                        rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                        rngFooterArea.Value = oilCnt(3)
                        ExcelMemoryRelease(rngFooterArea)
                        i += 1
                        '◯3号軽油
                        rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                        rngFooterArea.Value = oilCnt(4)
                        ExcelMemoryRelease(rngFooterArea)
                        i += 1
                        '◯LSA
                        rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                        rngFooterArea.Value = oilCnt(5)
                        ExcelMemoryRelease(rngFooterArea)
                        i += 1
                        '◯A重油
                        rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                        rngFooterArea.Value = oilCnt(6)
                        ExcelMemoryRelease(rngFooterArea)
                    End If

                    '★★★初期化
                    i = j
                    oilCnt = {0, 0, 0, 0, 0, 0, 0}
                End If

                Select Case PrintDatarow("OILCODE").ToString()
                    '# ハイオク
                    Case BaseDllConst.CONST_HTank
                        oilCnt(0) += 1
                    '# レギュラー
                    Case BaseDllConst.CONST_RTank
                        oilCnt(1) += 1
                    '# 灯油
                    Case BaseDllConst.CONST_TTank
                        oilCnt(2) += 1
                    '# 軽油
                    Case BaseDllConst.CONST_KTank1
                        oilCnt(3) += 1
                    '# ３号軽油
                    Case BaseDllConst.CONST_K3Tank1
                        oilCnt(4) += 1
                    '# LSA
                    Case BaseDllConst.CONST_LTank1
                        oilCnt(5) += 1
                    '# A重油
                    Case BaseDllConst.CONST_ATank
                        oilCnt(6) += 1
                End Select

                '荷受人(保存)
                svConsigneeCode = PrintDatarow("CONSIGNEECODE").ToString()
            Next

            '◯荷受人
            Select Case svConsigneeCode
                    '# JONET松本
                Case BaseDllConst.CONST_CONSIGNEECODE_40
                    svTrain = clnTrain(5)                               '401専用入線方
                    If rTrainNo = BaseDllConst.CONST_RTRAIN_I01_501_011203 _
                        AndAlso bSameTimeLine = False Then svTrain = clnTrain(2)      '501専用入線方
                    '# OT宇都宮
                Case BaseDllConst.CONST_CONSIGNEECODE_53
                    svTrain = clnTrain(4)
                Case Else
                    svTrain = ""
            End Select

            '# 油種合計用の荷受人の場合
            If svTrain <> "" Then
                '◯HR
                rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                rngFooterArea.Value = oilCnt(0)
                ExcelMemoryRelease(rngFooterArea)
                i += 1
                '◯RG
                rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                rngFooterArea.Value = oilCnt(1)
                ExcelMemoryRelease(rngFooterArea)
                i += 1
                '◯灯油
                rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                rngFooterArea.Value = oilCnt(2)
                ExcelMemoryRelease(rngFooterArea)
                i += 1
                '◯軽油
                rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                rngFooterArea.Value = oilCnt(3)
                ExcelMemoryRelease(rngFooterArea)
                i += 1
                '◯3号軽油
                rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                rngFooterArea.Value = oilCnt(4)
                ExcelMemoryRelease(rngFooterArea)
                i += 1
                '◯LSA
                rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                rngFooterArea.Value = oilCnt(5)
                ExcelMemoryRelease(rngFooterArea)
                i += 1
                '◯A重油
                rngFooterArea = Me.ExcelWorkSheet.Range(svTrain + i.ToString())
                rngFooterArea.Value = oilCnt(6)
                ExcelMemoryRelease(rngFooterArea)
            End If

            '受注オーダーにLTA油種が含まれているか確認
            Dim iLTACnt As Integer = PrintData.Select("ORDERINGOILNAME='" + BaseDllConst.CONST_2101C + "'").Count
            If iLTACnt >= 1 AndAlso (rTrainNo = BaseDllConst.CONST_RTRAIN_I02_401_011203 OrElse bSameTimeLine = True) Then
                Dim clnLTA() As String = {"B", "D", "F", "H", "J", "L"}
                For Each strLTA As String In clnLTA
                    '○LTA油種が含まれている場合
                    '　フッターの「A重油」⇒「ＬＴＡ」へ書き換える
                    rngFooterArea = Me.ExcelWorkSheet.Range(strLTA + "54")
                    rngFooterArea.Value = BaseDllConst.CONST_2101C
                    ExcelMemoryRelease(rngFooterArea)
                Next
            End If

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngFooterArea)
        End Try
    End Sub
#End Region

#Region "ダウンロード(出荷・積込予定表(根岸))"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(出荷・積込予定表(根岸))URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintNegishiData(ByVal repPtn As String,
                                                ByVal lodDate As String,
                                                Optional ByVal dtFT As DataTable = Nothing,
                                                Optional ByVal dtPF As DataTable = Nothing) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"

        '○帳票名取得
        Dim tmpGetFileName As String = CMNPTS.SetReportFileName(repPtn, BaseDllConst.CONST_OFFICECODE_011402, lodDate, "")
        If tmpGetFileName <> "" Then
            tmpFileName = tmpGetFileName
        End If

        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            '***** TODO処理 ここから *****
            If repPtn = "SHIPPLAN" Then
                '◯ヘッダーの設定
                EditNegishiShipHeaderArea(lodDate, dtFT, dtPF)
                '◯明細の設定
                EditNegishiShipDetailArea()
            ElseIf repPtn = "LOADPLAN" Then
                '◯ヘッダーの設定
                EditNegishiLoadHeaderArea(lodDate)
                '◯明細の設定
                EditNegishiLoadDetailArea()
            End If
            '***** TODO処理 ここまで *****
            'ExcelTempSheet.Delete() '雛形シート削除

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
    ''' 帳票のヘッダー設定(出荷予定表(根岸))
    ''' </summary>
    Private Sub EditNegishiShipHeaderArea(ByVal lodDate As String, ByVal dtFT As DataTable, ByVal dtPF As DataTable)
        Dim rngHeaderArea As Excel.Range = Nothing
        'Dim valueYear As String = Now.AddDays(1).ToString("yyyy", New Globalization.CultureInfo("ja-JP"))
        'Dim valueMonth As String = Now.AddDays(1).ToString("MM", New Globalization.CultureInfo("ja-JP"))
        'Dim valueDay As String = Now.AddDays(1).ToString("dd", New Globalization.CultureInfo("ja-JP"))
        Dim valueYear As String = Date.Parse(lodDate).ToString("yyyy", New Globalization.CultureInfo("ja-JP"))
        Dim valueMonth As String = Date.Parse(lodDate).ToString("MM", New Globalization.CultureInfo("ja-JP"))
        Dim valueDay As String = Date.Parse(lodDate).ToString("dd", New Globalization.CultureInfo("ja-JP"))

        Try
            '年
            rngHeaderArea = Me.ExcelWorkSheet.Range("D3")
            rngHeaderArea.Value = valueYear
            ExcelMemoryRelease(rngHeaderArea)
            '月
            rngHeaderArea = Me.ExcelWorkSheet.Range("F3")
            rngHeaderArea.Value = valueMonth
            ExcelMemoryRelease(rngHeaderArea)
            '日
            rngHeaderArea = Me.ExcelWorkSheet.Range("H3")
            rngHeaderArea.Value = valueDay
            ExcelMemoryRelease(rngHeaderArea)
            '### 20201105 START 指摘票対応(No193) ####################################################################
            '#枠(計画)
            Dim iAmount As Decimal = 0
            Dim iTTank As Integer = 0
            For Each dtPFrow As DataRow In dtPF.Rows
                Select Case Convert.ToString(dtPFrow("OILCODE"))
                    'HG
                    Case BaseDllConst.CONST_HTank
                        rngHeaderArea = Me.ExcelWorkSheet.Range("D61")
                        rngHeaderArea.Value = dtPFrow("AVERAGELOADAMOUNT")
                        ExcelMemoryRelease(rngHeaderArea)

                        rngHeaderArea = Me.ExcelWorkSheet.Range("E61")
                        rngHeaderArea.Value = dtPFrow("SHIPPINGPLAN")
                        ExcelMemoryRelease(rngHeaderArea)
                    'RG
                    Case BaseDllConst.CONST_RTank
                        rngHeaderArea = Me.ExcelWorkSheet.Range("D62")
                        rngHeaderArea.Value = dtPFrow("AVERAGELOADAMOUNT")
                        ExcelMemoryRelease(rngHeaderArea)

                        rngHeaderArea = Me.ExcelWorkSheet.Range("E62")
                        rngHeaderArea.Value = dtPFrow("SHIPPINGPLAN")
                        ExcelMemoryRelease(rngHeaderArea)
                    '灯油、未添加灯油
                    Case BaseDllConst.CONST_TTank,
                         BaseDllConst.CONST_MTTank
                        rngHeaderArea = Me.ExcelWorkSheet.Range("D63")
                        iAmount += Decimal.Parse(Convert.ToString(dtPFrow("AVERAGELOADAMOUNT")))
                        rngHeaderArea.Value = iAmount
                        ExcelMemoryRelease(rngHeaderArea)

                        rngHeaderArea = Me.ExcelWorkSheet.Range("E63")
                        iTTank += Integer.Parse(Convert.ToString(dtPFrow("SHIPPINGPLAN")))
                        rngHeaderArea.Value = iTTank
                        ExcelMemoryRelease(rngHeaderArea)
                    '軽油
                    Case BaseDllConst.CONST_KTank1
                        rngHeaderArea = Me.ExcelWorkSheet.Range("D65")
                        rngHeaderArea.Value = dtPFrow("AVERAGELOADAMOUNT")
                        ExcelMemoryRelease(rngHeaderArea)

                        rngHeaderArea = Me.ExcelWorkSheet.Range("E65")
                        rngHeaderArea.Value = dtPFrow("SHIPPINGPLAN")
                        ExcelMemoryRelease(rngHeaderArea)
                    '3号軽油
                    Case BaseDllConst.CONST_K3Tank1
                        rngHeaderArea = Me.ExcelWorkSheet.Range("D66")
                        rngHeaderArea.Value = dtPFrow("AVERAGELOADAMOUNT")
                        ExcelMemoryRelease(rngHeaderArea)

                        rngHeaderArea = Me.ExcelWorkSheet.Range("E66")
                        rngHeaderArea.Value = dtPFrow("SHIPPINGPLAN")
                        ExcelMemoryRelease(rngHeaderArea)
                    'A重油
                    Case BaseDllConst.CONST_ATank
                        rngHeaderArea = Me.ExcelWorkSheet.Range("D67")
                        rngHeaderArea.Value = dtPFrow("AVERAGELOADAMOUNT")
                        ExcelMemoryRelease(rngHeaderArea)

                        rngHeaderArea = Me.ExcelWorkSheet.Range("E67")
                        rngHeaderArea.Value = dtPFrow("SHIPPINGPLAN")
                        ExcelMemoryRelease(rngHeaderArea)
                    'LSA
                    Case BaseDllConst.CONST_LTank1
                        rngHeaderArea = Me.ExcelWorkSheet.Range("D68")
                        rngHeaderArea.Value = dtPFrow("AVERAGELOADAMOUNT")
                        ExcelMemoryRelease(rngHeaderArea)

                        rngHeaderArea = Me.ExcelWorkSheet.Range("E68")
                        rngHeaderArea.Value = dtPFrow("SHIPPINGPLAN")
                        ExcelMemoryRelease(rngHeaderArea)
                End Select
            Next
            '### 20201105 START 指摘票対応(No193) ####################################################################

            '### 20201105 START 指摘票対応(No191) ####################################################################
            '### 20201126 START 指摘票対応(No230) ####################################################################
            '積込日
            rngHeaderArea = Me.ExcelWorkSheet.Range("E71")
            rngHeaderArea.Value = lodDate
            ExcelMemoryRelease(rngHeaderArea)
            For Each dtFTrow As DataRow In dtFT.Rows
                Select Case Convert.ToString(dtFTrow("CONSIGNEECODE"))
                    'JXTG北信油槽所
                    Case BaseDllConst.CONST_CONSIGNEECODE_10
                        '出荷開始日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("E72")
                        rngHeaderArea.Value = dtFTrow("ORDERFROMDATE")
                        ExcelMemoryRelease(rngHeaderArea)
                        '出荷終了日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("E73")
                        rngHeaderArea.Value = dtFTrow("ORDERTODATE")
                        ExcelMemoryRelease(rngHeaderArea)
                    'JXTG甲府油槽所
                    Case BaseDllConst.CONST_CONSIGNEECODE_20
                        '出荷開始日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("F72")
                        rngHeaderArea.Value = dtFTrow("ORDERFROMDATE")
                        ExcelMemoryRelease(rngHeaderArea)
                        '出荷終了日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("F73")
                        rngHeaderArea.Value = dtFTrow("ORDERTODATE")
                        ExcelMemoryRelease(rngHeaderArea)
                    'OT宇都宮
                    Case BaseDllConst.CONST_CONSIGNEECODE_53
                        '出荷開始日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("G72")
                        rngHeaderArea.Value = dtFTrow("ORDERFROMDATE")
                        ExcelMemoryRelease(rngHeaderArea)
                        '出荷終了日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("G73")
                        rngHeaderArea.Value = dtFTrow("ORDERTODATE")
                        ExcelMemoryRelease(rngHeaderArea)
                    'OT高崎
                    Case BaseDllConst.CONST_CONSIGNEECODE_54
                        '出荷開始日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("H72")
                        rngHeaderArea.Value = dtFTrow("ORDERFROMDATE")
                        ExcelMemoryRelease(rngHeaderArea)
                        '出荷終了日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("H73")
                        rngHeaderArea.Value = dtFTrow("ORDERTODATE")
                        ExcelMemoryRelease(rngHeaderArea)
                    'OT八王子
                    Case BaseDllConst.CONST_CONSIGNEECODE_55
                        '出荷開始日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("I72")
                        rngHeaderArea.Value = dtFTrow("ORDERFROMDATE")
                        ExcelMemoryRelease(rngHeaderArea)
                        '出荷終了日(3号軽油)
                        rngHeaderArea = Me.ExcelWorkSheet.Range("I73")
                        rngHeaderArea.Value = dtFTrow("ORDERTODATE")
                        ExcelMemoryRelease(rngHeaderArea)
                End Select
            Next
            ''出荷開始日(3号軽油)
            'rngHeaderArea = Me.ExcelWorkSheet.Range("E72")
            'rngHeaderArea.Value = dtFT.Rows(0)("ORDERFROMDATE")
            ''出荷終了日(3号軽油)
            'rngHeaderArea = Me.ExcelWorkSheet.Range("E73")
            'rngHeaderArea.Value = dtFT.Rows(0)("ORDERTODATE")
            '### 20201126 END   指摘票対応(No230) ####################################################################
            '### 20201105 END   指摘票対応(No191) ####################################################################

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(出荷予定表(根岸))
    ''' </summary>
    Private Sub EditNegishiShipDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            '○帳票の明細共通処理(出荷予定表(根岸))
            Dim strTate As Integer() = {6, 12}
            EditNegishiShipCmn(rngDetailArea, strTate, "RNUM=1")

            '### 20201020 START 指摘票対応(No174)全体 ##################################################
            '○帳票の明細共通処理(出荷予定表(根岸))※予備枠の設定
            Dim strTateYobi As Integer() = {9, 17}
            EditNegishiShipCmn(rngDetailArea, strTateYobi, "RNUM=2")
            '### 20201020 END   指摘票対応(No174)全体 ##################################################

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細共通処理(出荷予定表(根岸))
    ''' </summary>
    Private Sub EditNegishiShipCmn(ByVal I_rngDetailArea As Excel.Range,
                                   ByVal I_TATE() As Integer,
                                   ByVal I_CONDITION As String)

        Dim i As Integer = 0
        For Each PrintDatarow As DataRow In PrintData.Select(I_CONDITION)

            '★行位置決め（列車別の固定行を設定）
            Select Case PrintDatarow("TRAINNO").ToString()
                    '着駅(坂城)
                Case "5463", "2085", "8471"
                    i = I_TATE(0)
                    If PrintDatarow("TRAINNO").ToString() = "5463" Then i = i
                    If PrintDatarow("TRAINNO").ToString() = "2085" Then i = i + 1
                    If PrintDatarow("TRAINNO").ToString() = "8471" Then i = i + 2
                    '着駅(竜王)
                Case "81", "83"
                    i = I_TATE(1)
                    If PrintDatarow("TRAINNO").ToString() = "81" Then i = i
                    If PrintDatarow("TRAINNO").ToString() = "83" Then i = i + 1
                    '着駅(宇都宮)
                Case "4091", "2091", "8571", "8569", "2569"
                    i = 22
                    If PrintDatarow("TRAINNO").ToString() = "4091" Then i = i
                    If PrintDatarow("TRAINNO").ToString() = "2091" Then i = i + 1
                    If PrintDatarow("TRAINNO").ToString() = "8571" Then i = i + 2
                    If PrintDatarow("TRAINNO").ToString() = "8569" Then i = i + 3
                    If PrintDatarow("TRAINNO").ToString() = "2569" Then i = i + 4
                    '着駅(倉賀野)
                Case "3093", "3091", "8777", "2777"
                    i = 27
                    If PrintDatarow("TRAINNO").ToString() = "3093" Then i = i
                    'If PrintDatarow("TRAINNO").ToString() = "5166" Then i = i + 1
                    If PrintDatarow("TRAINNO").ToString() = "3091" Then i = i + 2
                    If PrintDatarow("TRAINNO").ToString() = "8777" Then i = i + 3
                    'If PrintDatarow("TRAINNO").ToString() = "8099" Then i = i + 4
                    If PrintDatarow("TRAINNO").ToString() = "2777" Then i = i + 5
                    '着駅(八王子)
                Case "85", "87", "8097", "5692"
                    i = 33
                    If PrintDatarow("TRAINNO").ToString() = "85" Then i = i
                    If PrintDatarow("TRAINNO").ToString() = "87" Then i = i + 1
                    If PrintDatarow("TRAINNO").ToString() = "8097" Then i = i + 2
                    If PrintDatarow("TRAINNO").ToString() = "5692" Then i = i + 3
                    '着駅(南松本)
                Case "5474", "5160"
                    i = 39
                    If PrintDatarow("TRAINNO").ToString() = "5474" Then i = i
                    If PrintDatarow("TRAINNO").ToString() = "5160" Then i = i + 1
            End Select

            '★列位置決め（油種別の固定列を設定）
            Select Case PrintDatarow("OILCODE").ToString()
                    '◯ 油種(ＨＧ)
                Case BaseDllConst.CONST_HTank
                    I_rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                    '◯ 油種(ＲＧ)
                Case BaseDllConst.CONST_RTank
                    I_rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                    '◯ 油種(クト)灯油？
                Case BaseDllConst.CONST_TTank
                    I_rngDetailArea = Me.ExcelWorkSheet.Range("M" + i.ToString())
                    '◯ 油種(未ト)未添加灯油？
                Case BaseDllConst.CONST_MTTank
                    I_rngDetailArea = Me.ExcelWorkSheet.Range("N" + i.ToString())
                    '◯ 油種(軽)
                Case BaseDllConst.CONST_KTank1
                    I_rngDetailArea = Me.ExcelWorkSheet.Range("O" + i.ToString())
                    '◯ 油種(軽３)
                Case BaseDllConst.CONST_K3Tank1
                    I_rngDetailArea = Me.ExcelWorkSheet.Range("P" + i.ToString())
                    '◯ 油種(Ａ)
                Case BaseDllConst.CONST_ATank
                    I_rngDetailArea = Me.ExcelWorkSheet.Range("R" + i.ToString())
                    '◯ 油種(ＬＡ)
                Case BaseDllConst.CONST_LTank1
                    I_rngDetailArea = Me.ExcelWorkSheet.Range("V" + i.ToString())
                Case Else
                    Continue For
            End Select
            I_rngDetailArea.Value = PrintDatarow("TOTALTANK")
            ExcelMemoryRelease(I_rngDetailArea)
        Next

    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(積込予定表(根岸))
    ''' </summary>
    Private Sub EditNegishiLoadHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing
        'Dim value As String = Now.AddDays(1).ToString("yyyy年MM月dd日（ddd）", New Globalization.CultureInfo("ja-JP"))
        Dim value As String = Date.Parse(lodDate).ToString("yyyy年MM月dd日（ddd）", New Globalization.CultureInfo("ja-JP"))

        Try
            'タイトル
            rngHeaderArea = Me.ExcelWorkSheet.Range("B1")
            rngHeaderArea.Value = value + "タンク車回線別出荷予定表"
            ExcelMemoryRelease(rngHeaderArea)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(積込予定表(根岸))
    ''' </summary>
    Private Sub EditNegishiLoadDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim strYoko As String() = {"E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W"}
            'Dim strYoko As String() = {"E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V"}
            '○帳票の明細共通処理(積込予定表(根岸))
            EditNegishiLoadCmn(rngDetailArea, strYoko, "RNUM=1")

            '### 20201020 START 指摘票対応(No174)全体 ##################################################
            Dim strYokoYobi As String() = {"X", "Y", "Z", "AA", "AB"}
            'Dim strYokoYobi As String() = {"W", "X", "Y", "Z", "AA"}
            '○帳票の明細共通処理(積込予定表(根岸))※予備枠の設定
            EditNegishiLoadCmn(rngDetailArea, strYokoYobi, "RNUM=2")
            '### 20201020 END   指摘票対応(No174)全体 ##################################################

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub

    ''' <summary>
    ''' 帳票の明細共通処理(積込予定表(根岸))
    ''' </summary>
    Private Sub EditNegishiLoadCmn(ByVal I_rngDetailArea As Excel.Range,
                                   ByVal I_YOKO() As String,
                                   ByVal I_CONDITION As String)

        Dim iYoko As Integer = 0
        'Dim strYoko As String() = {"E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V"}
        Dim iTate As Integer = 0
        Dim intTate As Integer() = {6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46, 48}
        Dim iTateJyogai As Integer = 0
        Dim intTateJyogai As Integer() = {50, 52, 54, 56, 58}
        Dim jTate As Integer = 0
        Dim svTrainNo As String = ""

        For Each PrintDatarow As DataRow In PrintData.Select(I_CONDITION)

            '★列車(着駅)が変更となった場合
            If svTrainNo <> "" AndAlso svTrainNo <> PrintDatarow("TRAINNO").ToString() Then
                '列を１つ右に移動
                iYoko += 1
                '除外枠の行を初期化
                iTateJyogai = 0
            End If

            '◯ 充填ポイント
            If PrintDatarow("FILLINGPOINT").ToString() = "" Then
                '### 2020/06/25 START 充填ポイントにはまらない油種は除外枠に表示 ########################################
                If PrintDatarow("OILKANA").ToString() <> "" Then
                    '列車名(着駅)
                    I_rngDetailArea = Me.ExcelWorkSheet.Range(I_YOKO(iYoko) + intTateJyogai(iTateJyogai).ToString())
                    I_rngDetailArea.Value = PrintDatarow("TRAINNAME").ToString().Substring(0, 1)
                    ExcelMemoryRelease(I_rngDetailArea)
                    '油種名
                    I_rngDetailArea = Me.ExcelWorkSheet.Range(I_YOKO(iYoko) + (intTateJyogai(iTateJyogai) + 1).ToString())
                    I_rngDetailArea.Value = PrintDatarow("OILKANA")
                    ExcelMemoryRelease(I_rngDetailArea)
                    iTateJyogai += 1
                End If
                '### 2020/06/25 END   充填ポイントにはまらない油種は除外枠に表示 ########################################

                svTrainNo = PrintDatarow("TRAINNO").ToString()
                Continue For
            End If
            jTate = Integer.Parse(PrintDatarow("FILLINGPOINT").ToString()) - 1

            '列車名(着駅)
            I_rngDetailArea = Me.ExcelWorkSheet.Range(I_YOKO(iYoko) + intTate(jTate).ToString())
            'rngDetailArea.Value = PrintDatarow("TRAINNAME")
            I_rngDetailArea.Value = PrintDatarow("TRAINNAME").ToString().Substring(0, 1)
            If Convert.ToString(PrintDatarow("TRAINNAME")) = "南松本" Then I_rngDetailArea.Value = "松"
            ExcelMemoryRelease(I_rngDetailArea)
            '油種名
            I_rngDetailArea = Me.ExcelWorkSheet.Range(I_YOKO(iYoko) + (intTate(jTate) + 1).ToString())
            'rngDetailArea.Value = PrintDatarow("OILNAME")
            I_rngDetailArea.Value = PrintDatarow("OILKANA")
            ExcelMemoryRelease(I_rngDetailArea)
            '★列車名(着駅)を退避
            svTrainNo = PrintDatarow("TRAINNO").ToString()
        Next
    End Sub
#End Region

#Region "ダウンロード(四日市(託送指示, 出荷予定))"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(四日市(託送指示, 出荷予定表))URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintYokkaichiData(ByVal repPtn As String, ByVal lodDate As String,
                                                  Optional ByVal dt As DataTable = Nothing) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"

        '○帳票名取得
        Dim tmpGetFileName As String = CMNPTS.SetReportFileName(repPtn, BaseDllConst.CONST_OFFICECODE_012401, lodDate, "")
        If tmpGetFileName <> "" Then
            tmpFileName = tmpGetFileName
        End If

        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Select Case repPtn
                Case "DELIVERYPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditDeliveryHeaderArea(lodDate)
                    '◯明細の設定
                    EditDeliveryDetailArea()
                    '***** TODO処理 ここまで *****
                    ExcelTempSheet.Delete() '雛形シート削除
                    ExcelMemoryRelease(ExcelTempSheet)

                '出荷予定表
                Case "SHIPPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditYokkaichiShipHeaderArea(lodDate)
                    '◯油種出荷期間の設定
                    EditOilDurationArea(BaseDllConst.CONST_OFFICECODE_012401, dt)
                    '◯明細の設定
                    EditYokkaichiShipDetailArea()
                    '***** TODO処理 ここまで *****

            End Select
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
#End Region

#Region "ダウンロード(三重塩浜(託送指示, 出荷予定, 積込指示書, タンク車出荷連絡書))"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(三重塩浜(託送指示, 出荷予定表, 積込指示書, タンク車出荷連絡書))URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintMieShiohamaData(ByVal repPtn As String, ByVal lodDate As String,
                                                    Optional ByVal dt As DataTable = Nothing) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"

        '○帳票名取得
        Dim tmpGetFileName As String = CMNPTS.SetReportFileName(repPtn, BaseDllConst.CONST_OFFICECODE_012402, lodDate, "")
        If tmpGetFileName <> "" Then
            tmpFileName = tmpGetFileName
        End If

        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Select Case repPtn
                '託送指示
                Case "DELIVERYPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditDeliveryHeaderArea(lodDate)
                    '◯明細の設定
                    EditDeliveryDetailArea()
                    '***** TODO処理 ここまで *****
                    ExcelTempSheet.Delete() '雛形シート削除
                    ExcelMemoryRelease(ExcelTempSheet)
                '出荷予定
                Case "SHIPPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditMieShiohamaShipHeaderArea(lodDate)
                    '◯油種出荷期間の設定
                    EditOilDurationArea(BaseDllConst.CONST_OFFICECODE_012402, dt)
                    '◯明細の設定
                    EditMieShiohamaShipDetailArea()
                    '***** TODO処理 ここまで *****

                '積込指示書
                Case "LOADPLAN"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditLoadPlanHeaderArea(lodDate)
                    '◯明細の設定
                    EditLoadPlanDetailArea()
                    '◯予約数量の設定
                    EditReserveAmountArea(dt)
                    '***** TODO処理 ここまで *****
                'タンク車出荷連絡書
                Case "SHIPCONTACT"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditShipContactHeaderArea(lodDate)
                    '◯明細の設定
                    EditShipContactDetailArea()
                    '***** TODO処理 ここまで *****
            End Select
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
#End Region

    ''' <summary>
    ''' 帳票のヘッダー設定(託送指示)
    ''' </summary>
    Private Sub EditDeliveryHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing
        Dim value As String = Now.AddDays(0).ToString("yyyy年MM月dd日", New Globalization.CultureInfo("ja-JP"))

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '発駅名
                rngHeaderArea = Me.ExcelWorkSheet.Range("C3")
                rngHeaderArea.Value = PrintDatarow("DEPSTATIONNAME")
                ExcelMemoryRelease(rngHeaderArea)
                '発行日
                rngHeaderArea = Me.ExcelWorkSheet.Range("M2")
                rngHeaderArea.Value = value
                ExcelMemoryRelease(rngHeaderArea)
                Exit For
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(託送指示)
    ''' </summary>
    Private Sub EditDeliveryDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim i As Integer = 7
            For Each PrintDatarow As DataRow In PrintData.Rows
                '固定NO
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")
                ExcelMemoryRelease(rngDetailArea)
                '協定コード
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")
                ExcelMemoryRelease(rngDetailArea)
                '割増・割引C
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")
                ExcelMemoryRelease(rngDetailArea)
                '品目コード
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")
                ExcelMemoryRelease(rngDetailArea)
                '車種コード
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")
                ExcelMemoryRelease(rngDetailArea)
                '貨車番号
                rngDetailArea = Me.ExcelWorkSheet.Range("G" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TANKNO")
                ExcelMemoryRelease(rngDetailArea)
                '列車番号
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TRAINNO")
                ExcelMemoryRelease(rngDetailArea)
                '着駅名
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ARRSTATIONNAME")
                ExcelMemoryRelease(rngDetailArea)
                '荷受人名
                rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONSIGNEENAME")
                ExcelMemoryRelease(rngDetailArea)
                '運送状番号
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")
                ExcelMemoryRelease(rngDetailArea)
                '屯数
                rngDetailArea = Me.ExcelWorkSheet.Range("L" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")
                ExcelMemoryRelease(rngDetailArea)
                '運賃
                rngDetailArea = Me.ExcelWorkSheet.Range("M" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")
                ExcelMemoryRelease(rngDetailArea)
                '受領印
                rngDetailArea = Me.ExcelWorkSheet.Range("N" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")
                ExcelMemoryRelease(rngDetailArea)
                i += 1
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(出荷予定表(四日市営業所))
    ''' </summary>
    Private Sub EditYokkaichiShipHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 出荷日（積込日）
                Dim value As String = lodDate

                '◯ 作成日
                rngHeaderArea = Me.ExcelWorkSheet.Range("AJ1")
                rngHeaderArea.Value = Date.Now.ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)

                '　出荷日
                rngHeaderArea = Me.ExcelWorkSheet.Range("D5")
                rngHeaderArea.Value = Date.Parse(value).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)

                Exit For
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(出荷予定表(四日市営業所))
    ''' </summary>
    Private Sub EditYokkaichiShipDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing
        Dim svTrainNo As String = ""
        Dim svShippersCode As String = ""
        Dim strYoko As String() = {"E", "H", "K", "N", "Q", "T", "W", "AC", "AF"}
        Dim iOilCnt As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim iTate As Integer() = {8, 9, 10}
        Dim iYoko As Integer = 0

        Try
            Dim i As Integer = 8
            For Each PrintDatarow As DataRow In PrintData.Rows
                '★列車(着駅)、荷主が変更となった場合
                If svTrainNo <> "" AndAlso
                   (svTrainNo <> PrintDatarow("TRAINNO").ToString() OrElse
                    svShippersCode <> PrintDatarow("SHIPPERSCODE").ToString()) Then
                    '行を１つ下に移動
                    i += 1
                    '油種数を初期化
                    iOilCnt = {0, 0, 0, 0, 0, 0, 0, 0, 0}
                End If

                '油種が未設定の場合は次のデータへ
                If PrintDatarow("OILCODE").ToString() = "" Then
                    '★列車番号を退避
                    svTrainNo = PrintDatarow("TRAINNO").ToString()
                    svShippersCode = PrintDatarow("SHIPPERSCODE").ToString()
                    Continue For
                End If

                '荷主の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPPERSNAME").ToString()
                ExcelMemoryRelease(rngDetailArea)
                '列車番号の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TRAINNO").ToString()
                ExcelMemoryRelease(rngDetailArea)
                '荷受人の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONSIGNEENAME").ToString()
                ExcelMemoryRelease(rngDetailArea)

                Select Case PrintDatarow("OILCODE").ToString()
                    '◯白油 
                    '　ハイオク
                    Case BaseDllConst.CONST_HTank
                        iYoko = 0
                    '　レギュラー
                    Case BaseDllConst.CONST_RTank
                        iYoko = 1
                    '　灯油
                    Case BaseDllConst.CONST_TTank
                        iYoko = 2
                    '　軽油
                    Case BaseDllConst.CONST_KTank1,
                         BaseDllConst.CONST_K5Tank,
                         BaseDllConst.CONST_K10Tank
                        iYoko = 3
                    '　３号軽油, ３号軽油20, ３号軽油50
                    Case BaseDllConst.CONST_K3Tank1
                        Select Case PrintDatarow("ORDERINGTYPE").ToString()
                            '　３号軽油
                            Case "A"
                                iYoko = 4
                            '　３号軽油20
                            Case "B"
                                iYoko = 5
                            '　３号軽油50
                            Case "C"
                                iYoko = 6
                            Case Else
                                Continue For
                        End Select

                    '◯黒油
                    '　LSA重油5号(Ａ重油)
                    Case BaseDllConst.CONST_ATank
                        iYoko = 7
                    '　LSA1(ＬＳＡ)
                    Case BaseDllConst.CONST_LTank1
                        iYoko = 8

                    Case Else
                        Continue For
                End Select

                '★帳票に値を設定
                rngDetailArea = Me.ExcelWorkSheet.Range(strYoko(iYoko) + i.ToString())
                iOilCnt(iYoko) += Integer.Parse(Convert.ToString(PrintDatarow("CNT")))
                rngDetailArea.Value = iOilCnt(iYoko)
                ExcelMemoryRelease(rngDetailArea)
                '★列車番号、荷主を退避
                svTrainNo = PrintDatarow("TRAINNO").ToString()
                svShippersCode = PrintDatarow("SHIPPERSCODE").ToString()
            Next

            '空白行の削除（合計が0（ゼロ）の行を削除する）
            For rowCnt As Integer = iTate(iTate.Count - 1) To iTate(0) Step -1
                rngDetailArea = Me.ExcelWorkSheet.Range("AL" & rowCnt)
                If rngDetailArea.Value.ToString = "0" Then
                    rngDetailArea.EntireRow.Delete()
                End If
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(出荷予定表(三重塩浜営業所))
    ''' </summary>
    Private Sub EditMieShiohamaShipHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 出荷日（積込日）
                Dim value As String = lodDate

                '◯ 作成日
                rngHeaderArea = Me.ExcelWorkSheet.Range("AG1")
                rngHeaderArea.Value = Date.Now.ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)

                '　出荷日
                rngHeaderArea = Me.ExcelWorkSheet.Range("D5")
                rngHeaderArea.Value = Date.Parse(value).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                ExcelMemoryRelease(rngHeaderArea)

                Exit For
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(出荷予定表(三重塩浜営業所))
    ''' </summary>
    Private Sub EditMieShiohamaShipDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing
        Dim svTrainNo As String = ""
        Dim svShippersCode As String = ""
        Dim strYoko As String() = {"E", "H", "K", "N", "Q", "W", "Z", "AC"}
        Dim iOilCnt As Integer() = {0, 0, 0, 0, 0, 0, 0, 0}
        Dim iTate As Integer() = {8, 9, 10}
        Dim iYoko As Integer = 0

        Try
            Dim i As Integer = 8
            For Each PrintDatarow As DataRow In PrintData.Rows
                '★列車(着駅)、荷主が変更となった場合
                If svTrainNo <> "" AndAlso
                   (svTrainNo <> PrintDatarow("TRAINNO").ToString() OrElse
                    svShippersCode <> PrintDatarow("SHIPPERSCODE").ToString()) Then
                    '行を１つ下に移動
                    i += 1
                    '油種数を初期化
                    iOilCnt = {0, 0, 0, 0, 0, 0, 0, 0}
                End If

                '油種が未設定の場合は次のデータへ
                If PrintDatarow("OILCODE").ToString() = "" Then
                    '★列車番号を退避
                    svTrainNo = PrintDatarow("TRAINNO").ToString()
                    svShippersCode = PrintDatarow("SHIPPERSCODE").ToString()
                    Continue For
                End If

                '荷主の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPPERSNAME").ToString()
                ExcelMemoryRelease(rngDetailArea)
                '列車番号の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TRAINNO").ToString()
                ExcelMemoryRelease(rngDetailArea)
                '荷受人の出力
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONSIGNEENAME").ToString()
                ExcelMemoryRelease(rngDetailArea)

                Select Case PrintDatarow("OILCODE").ToString()
                    '◯白油 
                    '　プレミアム(ハイオク)
                    Case BaseDllConst.CONST_HTank
                        iYoko = 0
                    '　レギュラー
                    Case BaseDllConst.CONST_RTank
                        iYoko = 1
                    '　灯油
                    Case BaseDllConst.CONST_TTank
                        iYoko = 2
                    '　軽油
                    Case BaseDllConst.CONST_KTank1
                        iYoko = 3
                    '　３号軽油(寒冷軽油)
                    Case BaseDllConst.CONST_K3Tank1
                        iYoko = 4

                    '◯黒油
                    '　LTA(Ａ重油), 0.5A重油(Ａ重油)
                    Case BaseDllConst.CONST_ATank
                        Select Case PrintDatarow("ORDERINGTYPE").ToString()
                            '　LTA(Ａ重油)
                            Case "C"
                                iYoko = 5
                            '　0.5A重油(Ａ重油)
                            Case "B"
                                iYoko = 6
                            Case Else
                                Continue For
                        End Select
                    '　0.1A重油(ＬＳＡ)
                    Case BaseDllConst.CONST_LTank1
                        iYoko = 7

                    Case Else
                        Continue For
                End Select

                '★帳票に値を設定
                rngDetailArea = Me.ExcelWorkSheet.Range(strYoko(iYoko) + i.ToString())
                iOilCnt(iYoko) += Integer.Parse(Convert.ToString(PrintDatarow("CNT")))
                rngDetailArea.Value = iOilCnt(iYoko)
                ExcelMemoryRelease(rngDetailArea)
                '★列車番号、荷主を退避
                svTrainNo = PrintDatarow("TRAINNO").ToString()
                svShippersCode = PrintDatarow("SHIPPERSCODE").ToString()
            Next

            '空白行の削除（合計が0（ゼロ）の行を削除する）
            For rowCnt As Integer = iTate(iTate.Count - 1) To iTate(0) Step -1
                rngDetailArea = Me.ExcelWorkSheet.Range("AI" & rowCnt)
                If rngDetailArea.Value.ToString = "0" Then
                    rngDetailArea.EntireRow.Delete()
                End If
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 油種期間の設定(出荷予定表)
    ''' </summary>
    Private Sub EditOilDurationArea(ByVal I_OFFICECODE As String, ByVal dtOilDuration As DataTable)
        Dim rngOilDurationArea As Excel.Range = Nothing
        Try
            Select Case I_OFFICECODE
            '○四日市営業所
                Case BaseDllConst.CONST_OFFICECODE_012401
                    Dim Condition As String = ""
                    '○荷受人(OT松本)
                    Condition = "CONSIGNEECODE='" + BaseDllConst.CONST_CONSIGNEECODE_56 + "' "
                    For Each OilDurationrow As DataRow In dtOilDuration.Select(Condition)
                        '○油種
                        Select Case Convert.ToString(OilDurationrow("OILCODE")) + Convert.ToString(OilDurationrow("SEGMENTOILCODE"))
                            '★3号軽油
                            Case BaseDllConst.CONST_K3Tank1 + "A"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B16")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B17")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                            '★3号軽油20
                            Case BaseDllConst.CONST_K3Tank1 + "B"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B19")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B20")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                            '★3号軽油50
                            Case BaseDllConst.CONST_K3Tank1 + "C"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B22")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B23")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                        End Select
                    Next

                    '○荷受人(愛知機関区)
                    Condition = "CONSIGNEECODE='" + BaseDllConst.CONST_CONSIGNEECODE_70 + "' "
                    For Each OilDurationrow As DataRow In dtOilDuration.Select(Condition)
                        '○油種
                        Select Case Convert.ToString(OilDurationrow("OILCODE")) + Convert.ToString(OilDurationrow("SEGMENTOILCODE"))
                            '★3号軽油
                            Case BaseDllConst.CONST_K3Tank1 + "A"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B25")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B26")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                            '★3号軽油20
                            Case BaseDllConst.CONST_K3Tank1 + "B"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B28")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B29")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                            '★3号軽油50
                            Case BaseDllConst.CONST_K3Tank1 + "C"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B31")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B32")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                        End Select
                    Next

            '○三重塩浜営業所
                Case BaseDllConst.CONST_OFFICECODE_012402
                    Dim Condition As String = ""
                    '○荷受人(JONET松本)
                    Condition = "CONSIGNEECODE='" + BaseDllConst.CONST_CONSIGNEECODE_40 + "' "
                    For Each OilDurationrow As DataRow In dtOilDuration.Select(Condition)
                        '○油種
                        Select Case Convert.ToString(OilDurationrow("OILCODE")) + Convert.ToString(OilDurationrow("SEGMENTOILCODE"))
                            '★3号軽油(寒冷軽油)
                            Case BaseDllConst.CONST_K3Tank1 + "E"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B16")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B17")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                            '★LTA(Ａ重油)
                            Case BaseDllConst.CONST_ATank + "C"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B22")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B23")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                        End Select
                    Next
                    '○荷受人(愛知機関区)
                    Condition = "CONSIGNEECODE='" + BaseDllConst.CONST_CONSIGNEECODE_70 + "' "
                    For Each OilDurationrow As DataRow In dtOilDuration.Select(Condition)
                        '○油種
                        Select Case Convert.ToString(OilDurationrow("OILCODE")) + Convert.ToString(OilDurationrow("SEGMENTOILCODE"))
                            '★3号軽油(寒冷軽油)
                            Case BaseDllConst.CONST_K3Tank1 + "E"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B19")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B20")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                            '★LTA(Ａ重油)
                            Case BaseDllConst.CONST_ATank + "C"
                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B25")
                                rngOilDurationArea.Value = OilDurationrow("ORDERFROMDATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                                rngOilDurationArea = Me.ExcelWorkSheet.Range("B26")
                                rngOilDurationArea.Value = OilDurationrow("ORDERTODATE").ToString()
                                ExcelMemoryRelease(rngOilDurationArea)

                        End Select
                    Next

            End Select
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngOilDurationArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(積込指示書)
    ''' </summary>
    Private Sub EditLoadPlanHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            '積込日
            rngHeaderArea = Me.ExcelWorkSheet.Range("H1")
            rngHeaderArea.Value = lodDate
            ExcelMemoryRelease(rngHeaderArea)

            '○シート[出荷数量(昭四分)]
            '　積込日
            rngHeaderArea = Me.ExcelTempSheet.Range("V1")
            rngHeaderArea.Value = lodDate
            ExcelMemoryRelease(rngHeaderArea)

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(積込指示書)
    ''' </summary>
    Private Sub EditLoadPlanDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim i As Integer = 4
            For Each PrintDatarow As DataRow In PrintData.Rows
                '固定NO
                rngDetailArea = Me.ExcelWorkSheet.Range("A" + i.ToString())
                rngDetailArea.Value = PrintDatarow("LINECNT")
                ExcelMemoryRelease(rngDetailArea)
                '荷主名
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPPERSNAME")
                ExcelMemoryRelease(rngDetailArea)
                '着駅名
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ARRSTATIONNAME")
                ExcelMemoryRelease(rngDetailArea)
                '荷受人名
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONSIGNEENAME")
                ExcelMemoryRelease(rngDetailArea)
                '油種名
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + i.ToString())
                rngDetailArea.Value = PrintDatarow("OILNAME")
                ExcelMemoryRelease(rngDetailArea)
                '貨車番号
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TANKNO")
                ExcelMemoryRelease(rngDetailArea)
                i += 1
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の予約数量設定(三重塩浜営業所(積込指示))
    ''' </summary>
    Private Sub EditReserveAmountArea(ByVal dtReserveAmount As DataTable)
        If dtReserveAmount.Rows.Count = 0 Then Exit Sub
        Dim rngReserveAmountArea As Excel.Range = Nothing

        Try
            For Each ReserveAmountrow As DataRow In dtReserveAmount.Rows
                '○油種
                Select Case Convert.ToString(ReserveAmountrow("OILCODE")) + Convert.ToString(ReserveAmountrow("SEGMENTOILCODE"))
                    'SG(ハイオク)
                    Case BaseDllConst.CONST_HTank + "A"
                        rngReserveAmountArea = Me.ExcelTempSheet.Range("F4")
                    'RG(レギュラー)
                    Case BaseDllConst.CONST_RTank + "A"
                        rngReserveAmountArea = Me.ExcelTempSheet.Range("F5")
                    'DK(灯油)
                    Case BaseDllConst.CONST_TTank + "A"
                        rngReserveAmountArea = Me.ExcelTempSheet.Range("F6")
                    'GO(軽油)
                    Case BaseDllConst.CONST_KTank1 + "A"
                        rngReserveAmountArea = Me.ExcelTempSheet.Range("F7")
                    '3GO(寒冷軽油)
                    Case BaseDllConst.CONST_K3Tank1 + "E"
                        rngReserveAmountArea = Me.ExcelTempSheet.Range("F8")
                    '0.5AFO(0.5A重油)
                    Case BaseDllConst.CONST_ATank + "B"
                        rngReserveAmountArea = Me.ExcelTempSheet.Range("F9")
                    'LTA
                    Case BaseDllConst.CONST_ATank + "C"
                        rngReserveAmountArea = Me.ExcelTempSheet.Range("F10")
                    '0.1AFO(0.1A重油)
                    Case BaseDllConst.CONST_LTank1 + "B"
                        rngReserveAmountArea = Me.ExcelTempSheet.Range("F11")
                End Select
                rngReserveAmountArea.Value = ReserveAmountrow("RESERVEDQUANTITY")
                ExcelMemoryRelease(rngReserveAmountArea)
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngReserveAmountArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票のヘッダー設定(タンク車出荷連絡書)
    ''' </summary>
    Private Sub EditShipContactHeaderArea(ByVal lodDate As String)
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '荷主名
                rngHeaderArea = Me.ExcelWorkSheet.Range("A1")
                rngHeaderArea.Value = PrintDatarow("CONSIGNEENAME")
                ExcelMemoryRelease(rngHeaderArea)
                '発日
                rngHeaderArea = Me.ExcelWorkSheet.Range("Z1")
                rngHeaderArea.Value = PrintDatarow("DEPDATE")
                ExcelMemoryRelease(rngHeaderArea)
                '列車（JR最終列車番号）
                If PrintDatarow("TUMIOKIFLG").ToString = "0" Then
                    '当日発
                    rngHeaderArea = Me.ExcelWorkSheet.Range("AH1")
                    rngHeaderArea.Value = PrintDatarow("JRTRAINNO3")
                Else
                    '積置
                    rngHeaderArea = Me.ExcelWorkSheet.Range("AH1")
                    rngHeaderArea.Value = PrintDatarow("TRAINNO_TUMIOKI")
                End If
                ExcelMemoryRelease(rngHeaderArea)
                Exit For
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(タンク車出荷連絡書)
    ''' </summary>
    Private Sub EditShipContactDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing

        Try
            Dim i As Integer = 4
            Dim DtlEnd As Integer = 23
            '----------------------------------
            '明細行の編集出力
            '----------------------------------
            For Each PrintDatarow As DataRow In PrintData.Rows
                '固定NO
                rngDetailArea = Me.ExcelWorkSheet.Range("A" + i.ToString())
                rngDetailArea.Value = PrintDatarow("LINECNT")
                ExcelMemoryRelease(rngDetailArea)
                '貨車番号
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                'タキ1000の場合だけ型式+スペース + 車番
                'タキ243000タキ43000は車番のみ出力
                If PrintDatarow("MODEL").ToString = "タキ1000" Then
                    rngDetailArea.Value = PrintDatarow("MODEL").ToString & " " & PrintDatarow("TANKNO").ToString
                Else
                    rngDetailArea.Value = PrintDatarow("TANKNO")
                End If
                ExcelMemoryRelease(rngDetailArea)
                '### 20210705 START 指摘票No518(タンク車出荷連絡票対応) #######################################
                '交検
                rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
                rngDetailArea.Value = PrintDatarow("JRINSPECTIONDATE")
                ExcelMemoryRelease(rngDetailArea)
                '### 20210705 END   指摘票No518(タンク車出荷連絡票対応) #######################################
                '油種名
                rngDetailArea = Me.ExcelWorkSheet.Range("Q" + i.ToString())
                'rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
                rngDetailArea.Value = PrintDatarow("REPORTOILNAME")
                ExcelMemoryRelease(rngDetailArea)
                '数量
                rngDetailArea = Me.ExcelWorkSheet.Range("V" + i.ToString())
                'rngDetailArea = Me.ExcelWorkSheet.Range("O" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CARSAMOUNT")
                ExcelMemoryRelease(rngDetailArea)
                i += 1
            Next

            '----------------------------------
            '油種別合計行の編集（HIDDEN=0をキーとして利用、SELECT=1を列車数として利用）
            '----------------------------------
            Dim viw As New DataView(PrintData)
            Dim isDistinct As Boolean = True
            Dim cols() As String = {"HIDDEN", "OILCODE", "ORDERINGTYPE", "REPORTOILNAME"}
            viw.Sort = "HIDDEN, OILCODE, ORDERINGTYPE"
            Dim dtFilter As DataTable = viw.ToTable(isDistinct, cols)
            dtFilter.Columns.Add("Select", GetType(Integer))
            dtFilter.Columns.Add("CARSAMOUNT", GetType(Double))
            For Each row As DataRow In dtFilter.Rows
                Dim expr As String = String.Format("HIDDEN = '{0}' AND REPORTOILNAME = '{1}'", row("HIDDEN"), row("REPORTOILNAME"))
                row("SELECT") = PrintData.Compute("SUM(SELECT)", expr)
                row("CARSAMOUNT") = PrintData.Compute("SUM(CARSAMOUNT)", expr)
            Next

            Dim TtlRowCnt As Integer = 25
            Dim TtlRowMax As Integer = 28
            Dim TtlCol() As String = {"C", "I", "K", "L", "R", "X", "Z", "AA"}
            Dim ColIdx As Integer = 0
            For Each row As DataRow In dtFilter.Rows
                '油種
                rngDetailArea = Me.ExcelWorkSheet.Range(TtlCol(ColIdx) + TtlRowCnt.ToString)
                rngDetailArea.Value = row("REPORTOILNAME")
                ExcelMemoryRelease(rngDetailArea)
                'タンク数
                rngDetailArea = Me.ExcelWorkSheet.Range(TtlCol(ColIdx + 1) + TtlRowCnt.ToString)
                rngDetailArea.Value = row("SELECT")
                ExcelMemoryRelease(rngDetailArea)
                '固定値
                rngDetailArea = Me.ExcelWorkSheet.Range(TtlCol(ColIdx + 2) + TtlRowCnt.ToString)
                rngDetailArea.Value = "車"
                ExcelMemoryRelease(rngDetailArea)
                '数量
                rngDetailArea = Me.ExcelWorkSheet.Range(TtlCol(ColIdx + 3) + TtlRowCnt.ToString)
                rngDetailArea.Value = row("CARSAMOUNT")
                ExcelMemoryRelease(rngDetailArea)
                TtlRowCnt += 1

                If TtlRowCnt > TtlRowMax Then
                    TtlRowCnt = 25
                    ColIdx = 4
                End If
            Next

            '----------------------------------
            '合計行の編集
            '----------------------------------
            TtlRowCnt = 29
            Dim cols2() As String = {"HIDDEN"}
            dtFilter = viw.ToTable(isDistinct, cols2)
            dtFilter.Columns.Add("SELECT", GetType(Integer))
            dtFilter.Columns.Add("CARSAMOUNT", GetType(Double))
            For Each row As DataRow In dtFilter.Rows
                Dim expr As String = String.Format("HIDDEN = '{0}'", row("HIDDEN"))
                row("SELECT") = PrintData.Compute("SUM(SELECT)", expr)
                row("CARSAMOUNT") = PrintData.Compute("SUM(CARSAMOUNT)", expr)
            Next

            For Each row As DataRow In dtFilter.Rows
                'タンク数
                rngDetailArea = Me.ExcelWorkSheet.Range("X" + TtlRowCnt.ToString)
                rngDetailArea.Value = row("SELECT")
                ExcelMemoryRelease(rngDetailArea)
                '固定値
                rngDetailArea = Me.ExcelWorkSheet.Range("Z" + TtlRowCnt.ToString)
                rngDetailArea.Value = "車"
                ExcelMemoryRelease(rngDetailArea)
                '数量
                rngDetailArea = Me.ExcelWorkSheet.Range("AA" + TtlRowCnt.ToString)
                rngDetailArea.Value = row("CARSAMOUNT")
                ExcelMemoryRelease(rngDetailArea)
            Next

            '----------------------------------
            '空白行の削除
            '----------------------------------
            '合計行の空白行を削除（28行目から25行目まで値が入ってない行を削除する）
            For rowCnt As Integer = TtlRowMax To TtlRowMax - 3 Step -1
                rngDetailArea = Me.ExcelWorkSheet.Range("C" & rowCnt)
                If IsNothing(rngDetailArea.Value) Then
                    rngDetailArea.EntireRow.Delete()
                End If
            Next

            '明細の空白行を削除（23行目から値が入っている行まで削除する）
            For rowCnt As Integer = DtlEnd To i Step -1
                rngDetailArea = Me.ExcelWorkSheet.Range("C" & rowCnt)
                If IsNothing(rngDetailArea.Value) Then
                    rngDetailArea.EntireRow.Delete()
                End If
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
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

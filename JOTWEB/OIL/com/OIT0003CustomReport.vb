Option Strict On
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

    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable)
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
        If excelFileName = "OIT0003L_LOADPLAN.xlsx" Then
            Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("積込指示書"), Excel.Worksheet)
            Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
        ElseIf excelFileName = "OIT0003L_NEGISHI_SHIPPLAN.xlsx" _
            OrElse excelFileName = "OIT0003L_GOI_SHIPPLAN.xlsx" Then
            Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("入出力画面"), Excel.Worksheet)
            'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
        ElseIf excelFileName = "OIT0003L_NEGISHI_LOADPLAN.xlsx" Then
            Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("回線別積込"), Excel.Worksheet)
            'Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
        End If
    End Sub

#Region "ダウンロード(積込予定表(根岸以外))"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(出荷・積込予定表(根岸))URLを生成する
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
            '◯ヘッダーの設定
            EditLoadHeaderArea()
            '◯明細の設定
            EditLoadDetailArea()
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
    ''' 帳票のヘッダー設定(積込予定表(根岸以外))
    ''' </summary>
    Private Sub EditLoadHeaderArea()
        Dim rngHeaderArea As Excel.Range = Nothing
        'Dim value As String = Now.AddDays(1).ToString("yyyy年MM月dd日（ddd）", New Globalization.CultureInfo("ja-JP"))

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 基地名
                rngHeaderArea = Me.ExcelWorkSheet.Range("B1")
                rngHeaderArea.Value = PrintDatarow("BASENAME")

                '◯ 積込日
                Dim value As String = PrintDatarow("LODDATE").ToString
                rngHeaderArea = Me.ExcelWorkSheet.Range("E1")
                rngHeaderArea.Value = Date.Parse(value).ToString("MM月dd日分", New Globalization.CultureInfo("ja-JP"))

                '◯ 作成日(当日)
                rngHeaderArea = Me.ExcelWorkSheet.Range("O1")
                'rngHeaderArea.Value = Date.Parse(value).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))
                rngHeaderArea.Value = Now.AddDays(0).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP"))

                Exit For
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定(積込予定表(根岸以外))
    ''' </summary>
    Private Sub EditLoadDetailArea()
        Dim rngDetailArea As Excel.Range = Nothing
        Dim rngTmp As Excel.Range = Nothing
        Dim rngSummary As Excel.Range = Nothing
        Dim strTrainNameSave As String = ""
        Dim strTotalTankSave As String = ""

        Try
            Dim i As Integer = 5
            For Each PrintDatarow As DataRow In PrintData.Rows

                ''★ 前回の列車名と今回の列車名が不一致
                'If strTrainNameSave <> "" _
                '    AndAlso strTrainNameSave <> PrintDatarow("TRAINNAME").ToString() Then

                '    '★tmpシートより合計行をコピーして値を設定
                '    rngSummary = Me.ExcelTempSheet.Range("B1:P1")
                '    rngTmp = Me.ExcelWorkSheet.Range("B" + i.ToString(), "P" + i.ToString())
                '    'rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
                '    rngSummary.Copy(rngTmp)

                '    '◯ 合計車数
                '    rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                '    rngDetailArea.Value = strTotalTankSave + "両"

                '    i += 1
                'End If

                '◯ No
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = PrintDatarow("LINECNT")
                '◯ 荷主
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPPERSNAME")
                '◯ 着駅
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ARRSTATIONNAME")
                '◯ 荷受人
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CONSIGNEENAME")
                '◯ 積込ポイント
                '### 出力項目（空白） #####################################
                '◯ 油種
                rngDetailArea = Me.ExcelWorkSheet.Range("G" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ORDERINGOILNAME")
                '◯ 型式
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + i.ToString())
                rngDetailArea.Value = PrintDatarow("MODEL")
                '◯ 車番
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TANKNUMBER")
                '◯ 予約数量
                '### 出力項目（空白） #####################################
                ''◯ 交検
                'rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("JRINSPECTIONDATE")
                '◯ 積置
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                rngDetailArea.Value = PrintDatarow("STACKING")
                '◯ 列車№
                rngDetailArea = Me.ExcelWorkSheet.Range("L" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TRAINNO")
                '◯ 積込回数
                '### 出力項目（空白） #####################################
                '◯ 発日(予定)
                rngDetailArea = Me.ExcelWorkSheet.Range("N" + i.ToString())
                rngDetailArea.Value = PrintDatarow("DEPDATE")
                '◯ 予備
                '### 出力項目（空白） #####################################

                '★ 列車名・合計車数を退避
                strTrainNameSave = PrintDatarow("TRAINNAME").ToString()
                strTotalTankSave = PrintDatarow("TOTALTANK").ToString()

                i += 1
            Next

            ''★tmpシートより合計行をコピーして値を設定
            'rngSummary = Me.ExcelTempSheet.Range("B1:O1")
            'rngTmp = Me.ExcelWorkSheet.Range("B" + i.ToString(), "O" + i.ToString())
            ''rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
            'rngSummary.Copy(rngTmp)

            ''◯ 合計車数
            'rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
            'rngDetailArea.Value = strTotalTankSave + "両"

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
    Public Function CreateExcelPrintGoiData(ByVal repPtn As String) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            '***** TODO処理 ここから *****
            '◯ヘッダーの設定
            EditGoiShipHeaderArea()
            '◯明細の設定
            EditGoiShipDetailArea()
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
    ''' 帳票のヘッダー設定(出荷予定表(五井))
    ''' </summary>
    Private Sub EditGoiShipHeaderArea()
        Dim rngHeaderArea As Excel.Range = Nothing

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 積込日
                Dim value As String = PrintDatarow("LODDATE").ToString
                rngHeaderArea = Me.ExcelWorkSheet.Range("D3")
                rngHeaderArea.Value = Date.Parse(value).ToString("yyyy", New Globalization.CultureInfo("ja-JP"))
                rngHeaderArea = Me.ExcelWorkSheet.Range("F3")
                rngHeaderArea.Value = Date.Parse(value).ToString("MM", New Globalization.CultureInfo("ja-JP"))
                rngHeaderArea = Me.ExcelWorkSheet.Range("H3")
                rngHeaderArea.Value = Date.Parse(value).ToString("dd", New Globalization.CultureInfo("ja-JP"))

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

        Try
            Dim i As Integer = 8
            For Each PrintDatarow As DataRow In PrintData.Rows

                '◯ HI-G(ハイオク)
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ RE-G(レギュラー)
                rngDetailArea = Me.ExcelWorkSheet.Range("G" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ WKO(灯油)
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ DGO(軽油)
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ DGO.10(軽油１０)
                rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ DGO.3(３号軽油)
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ DGO.5(軽油５)
                rngDetailArea = Me.ExcelWorkSheet.Range("L" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ LA-1(ＬＳＡ－１, ＬＳＡ-1（山岳）)
                rngDetailArea = Me.ExcelWorkSheet.Range("N" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ LAブ(ＬＳＡーブレンド, ＬＳＡーブレンド（山岳）)
                rngDetailArea = Me.ExcelWorkSheet.Range("O" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ AFO(AFO, AFO（山岳）)
                rngDetailArea = Me.ExcelWorkSheet.Range("P" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ A-SP(AFOーSP, AFOーSP（山岳）)
                rngDetailArea = Me.ExcelWorkSheet.Range("Q" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '◯ A(ブ(AFOーブレンド（山岳）)
                rngDetailArea = Me.ExcelWorkSheet.Range("R" + i.ToString())
                'rngDetailArea.Value = PrintDatarow("")

                '★列車名(着駅)を退避
                svTrainNo = PrintDatarow("TRAINNAME").ToString()

                i += 1
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub

#End Region

#Region "ダウンロード(出荷・積込予定表(根岸))"
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(出荷・積込予定表(根岸))URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintNegishiData(ByVal repPtn As String) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            '***** TODO処理 ここから *****
            If repPtn = "SHIPPLAN" Then
                '◯ヘッダーの設定
                EditNegishiShipHeaderArea()
                '◯明細の設定
                EditNegishiShipDetailArea()
            ElseIf repPtn = "LOADPLAN" Then
                '◯ヘッダーの設定
                EditNegishiLoadHeaderArea()
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

#Region "出荷予定表(根岸)帳票設定"
    ''' <summary>
    ''' 帳票のヘッダー設定(出荷予定表(根岸))
    ''' </summary>
    Private Sub EditNegishiShipHeaderArea()
        Dim rngHeaderArea As Excel.Range = Nothing
        Dim valueYear As String = Now.AddDays(1).ToString("yyyy", New Globalization.CultureInfo("ja-JP"))
        Dim valueMonth As String = Now.AddDays(1).ToString("MM", New Globalization.CultureInfo("ja-JP"))
        Dim valueDay As String = Now.AddDays(1).ToString("dd", New Globalization.CultureInfo("ja-JP"))

        Try
            '年
            rngHeaderArea = Me.ExcelWorkSheet.Range("D3")
            rngHeaderArea.Value = valueYear
            '月
            rngHeaderArea = Me.ExcelWorkSheet.Range("F3")
            rngHeaderArea.Value = valueMonth
            '日
            rngHeaderArea = Me.ExcelWorkSheet.Range("H3")
            rngHeaderArea.Value = valueDay
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
            Dim i As Integer = 0
            For Each PrintDatarow As DataRow In PrintData.Rows

                '★行位置決め（列車別の固定行を設定）
                Select Case PrintDatarow("TRAINNO").ToString()
                    '着駅(坂城)
                    Case "5463", "2085", "8471"
                        i = 6
                        If PrintDatarow("TRAINNO").ToString() = "5463" Then i = i
                        If PrintDatarow("TRAINNO").ToString() = "2085" Then i = i + 1
                        If PrintDatarow("TRAINNO").ToString() = "8471" Then i = i + 2
                    '着駅(竜王)
                    Case "81", "83"
                        i = 10
                        If PrintDatarow("TRAINNO").ToString() = "81" Then i = i
                        If PrintDatarow("TRAINNO").ToString() = "83" Then i = i + 1
                    '着駅(宇都宮)
                    Case "4091", "2091", "8571", "8569", "2569"
                        i = 20
                        If PrintDatarow("TRAINNO").ToString() = "4091" Then i = i
                        If PrintDatarow("TRAINNO").ToString() = "2091" Then i = i + 1
                        If PrintDatarow("TRAINNO").ToString() = "8571" Then i = i + 2
                        If PrintDatarow("TRAINNO").ToString() = "8569" Then i = i + 3
                        If PrintDatarow("TRAINNO").ToString() = "2569" Then i = i + 4
                    '着駅(倉賀野)
                    Case "3093", "3091", "8777", "2777"
                        i = 25
                        If PrintDatarow("TRAINNO").ToString() = "3093" Then i = i
                        If PrintDatarow("TRAINNO").ToString() = "3091" Then i = i + 1
                        If PrintDatarow("TRAINNO").ToString() = "8777" Then i = i + 2
                        If PrintDatarow("TRAINNO").ToString() = "2777" Then i = i + 3
                    '着駅(八王子)
                    Case "85", "87", "8097", "5692"
                        i = 31
                        If PrintDatarow("TRAINNO").ToString() = "85" Then i = i
                        If PrintDatarow("TRAINNO").ToString() = "87" Then i = i + 1
                        If PrintDatarow("TRAINNO").ToString() = "8097" Then i = i + 2
                        If PrintDatarow("TRAINNO").ToString() = "5692" Then i = i + 3
                End Select

                '★列位置決め（油種別の固定列を設定）
                Select Case PrintDatarow("OILCODE").ToString()
                    '◯ 油種(ＨＧ)
                    Case BaseDllConst.CONST_HTank
                        rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                    '◯ 油種(ＲＧ)
                    Case BaseDllConst.CONST_RTank
                        rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                    '◯ 油種(クト)灯油？
                    Case BaseDllConst.CONST_TTank
                        rngDetailArea = Me.ExcelWorkSheet.Range("M" + i.ToString())
                    '◯ 油種(未ト)未添加灯油？
                    Case BaseDllConst.CONST_MTTank
                        rngDetailArea = Me.ExcelWorkSheet.Range("N" + i.ToString())
                    '◯ 油種(軽)
                    Case BaseDllConst.CONST_KTank1
                        rngDetailArea = Me.ExcelWorkSheet.Range("O" + i.ToString())
                    '◯ 油種(軽３)
                    Case BaseDllConst.CONST_K3Tank1
                        rngDetailArea = Me.ExcelWorkSheet.Range("P" + i.ToString())
                    '◯ 油種(Ａ)
                    Case BaseDllConst.CONST_ATank
                        rngDetailArea = Me.ExcelWorkSheet.Range("R" + i.ToString())
                    '◯ 油種(ＬＡ)
                    Case BaseDllConst.CONST_LTank1
                        rngDetailArea = Me.ExcelWorkSheet.Range("V" + i.ToString())
                    Case Else
                        Continue For
                End Select
                rngDetailArea.Value = PrintDatarow("TOTALTANK")
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try
    End Sub
#End Region
#Region "積込予定表(根岸)帳票設定"
    ''' <summary>
    ''' 帳票のヘッダー設定(積込予定表(根岸))
    ''' </summary>
    Private Sub EditNegishiLoadHeaderArea()
        Dim rngHeaderArea As Excel.Range = Nothing
        Dim value As String = Now.AddDays(1).ToString("yyyy年MM月dd日（ddd）", New Globalization.CultureInfo("ja-JP"))

        Try
            'タイトル
            rngHeaderArea = Me.ExcelWorkSheet.Range("B1")
            rngHeaderArea.Value = value + "タンク車回線別出荷予定表"
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
            Dim iYoko As Integer = 0
            Dim strYoko As String() = {"E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V"}
            Dim iTate As Integer = 0
            Dim intTate As Integer() = {6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46, 48}
            Dim jTate As Integer = 0
            Dim svTrainNo As String = ""

            For Each PrintDatarow As DataRow In PrintData.Rows

                '★列車(着駅)が変更となった場合
                If svTrainNo <> "" AndAlso svTrainNo <> PrintDatarow("TRAINNO").ToString() Then
                    iYoko += 1
                End If

                '◯ 充填ポイント
                If PrintDatarow("FILLINGPOINT").ToString() = "" Then
                    svTrainNo = PrintDatarow("TRAINNO").ToString()
                    Continue For
                End If
                jTate = Integer.Parse(PrintDatarow("FILLINGPOINT").ToString()) - 1

                '列車名(着駅)
                rngDetailArea = Me.ExcelWorkSheet.Range(strYoko(iYoko) + intTate(jTate).ToString())
                'rngDetailArea.Value = PrintDatarow("TRAINNAME")
                rngDetailArea.Value = PrintDatarow("TRAINNAME").ToString().Substring(0, 1)

                '油種名
                rngDetailArea = Me.ExcelWorkSheet.Range(strYoko(iYoko) + (intTate(jTate) + 1).ToString())
                'rngDetailArea.Value = PrintDatarow("OILNAME")
                rngDetailArea.Value = PrintDatarow("OILKANA")

                '★列車名(着駅)を退避
                svTrainNo = PrintDatarow("TRAINNO").ToString()
            Next

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

    End Sub
#End Region

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

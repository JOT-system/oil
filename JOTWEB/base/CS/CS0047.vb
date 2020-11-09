Option Strict On
Imports System.Web
Imports System.IO
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices

''' <summary>
''' 帳票マージ
''' </summary>
''' <remarks></remarks>
Public Structure CS0047XLSMERGE

    ''' <summary>
    ''' Excelディレクトリ
    ''' </summary>
    ''' <value>Excelディレクトリ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DIR() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value></value>
    ''' <returns>エラーコード</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 出力Dir＋ファイル名
    ''' </summary>
    ''' <value></value>
    ''' <returns>出力Dir＋ファイル名</returns>
    ''' <remarks></remarks>
    Public Property FILEpath() As String

    ''' <summary>
    ''' 全出力フラグ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property REMOVALLENGTH() As Integer

    ''' <summary>
    ''' 出力URL＋ファイル名
    ''' </summary>
    ''' <value></value>
    ''' <returns>出力URL＋ファイル名</returns>
    ''' <remarks></remarks>
    Public Property URL() As String

    ''' <summary>
    ''' 指定フォルダー内の複数Excelを出力Excel内複数Sheetへ格納
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0047XLSMERGE()

        Dim CS0011LOGWRITE As New CS0011LOGWrite        'ログ出力
        Dim CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

        Dim W_ExcelApp As Excel.Application = Nothing
        Dim W_ExcelBooks As Excel.Workbooks = Nothing
        Dim W_InExcelBook As Excel.Workbook = Nothing
        Dim W_InExcelSheets As Excel.Sheets = Nothing
        Dim W_InExcelSheet As Excel.Worksheet = Nothing
        Dim W_OutExcelBook As Excel.Workbook = Nothing
        Dim W_OutExcelSheets As Excel.Sheets = Nothing
        Dim W_OutExcelSheet As Excel.Worksheet = Nothing

        Dim W_ExcelLIST As New List(Of String)
        Dim WW_datetime As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString
        Dim W_SheetName As String = ""

        '●In PARAMチェック
        '○ 入力フォルダ存在確認＆Excelファイル名抽出 (C:\apple\files\TEXTWORK)
        If Directory.Exists(DIR) Then
            'ファイル格納フォルダ内不要ファイル削除(すべて削除)
            For Each tempFile As String In Directory.GetFiles(DIR, "*.*")
                If InStr(tempFile, ".XLS") > 0 Or InStr(tempFile, ".xls") > 0 Then
                    W_ExcelLIST.Add(tempFile)
                End If
            Next
        Else
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "InParamチェック"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "Excel処理に失敗しました"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '○ 入力フォルダー内のExcelファイルが存在しない場合はエラー
        If W_ExcelLIST.Count = 0 Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "InParamチェック"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "Excel処理に失敗しました"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '■Excelデータ処理
        Try
            '○初期処理 (Excel起動)
            W_ExcelApp = New Excel.Application
            'Excel非表示に設定
            W_ExcelApp.Visible = False
            '保存時の問合せのダイアログを非表示に設定
            W_ExcelApp.DisplayAlerts = False
            W_ExcelApp.SheetsInNewWorkbook = 1

            W_ExcelBooks = W_ExcelApp.Workbooks

            '○出力Excelファイルを開く(新規)
            W_OutExcelBook = W_ExcelBooks.Add

            '○一旦、保存形式（.xlsx)を指定して保存する（保存形式が異なると下記のシートコピーで異常終了するため）
            FILEpath = CS0050SESSION.UPLOAD_PATH & "\" & "PRINTWORK" & "\" & CS0050SESSION.USERID & "\" & WW_datetime & ".XLSX"
            'W_OutExcelBook.SaveAs(FILEpath, FileFormat:=Excel.XlFileFormat.xlOpenXMLWorkbook)
            'ExcelMemoryRelease(W_OutExcelBook)

            ''○出力Excelファイルを再度開く
            'W_OutExcelBook = W_ExcelBooks.Open(FILEpath)
            W_OutExcelSheets = W_OutExcelBook.Worksheets

            For i As Integer = 0 To W_ExcelLIST.Count - 1
                Try
                    '○入力Excelファイルを開く
                    W_InExcelBook = W_ExcelBooks.Open(W_ExcelLIST(i).ToString)
                    'W_InExcelSheet = DirectCast(W_InExcelBook.Worksheets.Item(1), Excel.Worksheet)
                    W_InExcelSheets = W_InExcelBook.Worksheets
                    W_InExcelSheet = CType(W_InExcelSheets.Item(1), Excel.Worksheet)

                    '○Sheet書式(文字形式)指定 … Sheet間参照防止
                    'W_InExcelSheet.Cells.NumberFormatLocal = "@"

                    '○Sheet名指定
                    W_SheetName = Mid(W_ExcelLIST(i).ToString, 1, InStr(W_ExcelLIST(i).ToString, ".") - 1)
                    Do Until InStr(W_SheetName, "\") = 0
                        W_SheetName = Mid(W_SheetName, InStrRev(W_SheetName, "\") + 1, 100)
                    Loop

                    '〇ソート用の名前を削除
                    If Not IsNothing(REMOVALLENGTH) AndAlso REMOVALLENGTH <> 0 Then
                        If W_SheetName.Length > REMOVALLENGTH Then
                            W_SheetName = W_SheetName.Remove(0, REMOVALLENGTH)
                        End If
                    End If

                    W_InExcelSheet.Name = W_SheetName

                    '○Sheetコピー
                    '出力先の最終Sheetを設定
                    'W_OutExcelSheet = DirectCast(W_OutExcelBook.Worksheets.Item(W_OutExcelBook.Worksheets.Count), Excel.Worksheet)
                    W_OutExcelSheet = CType(W_OutExcelSheets.Item(W_OutExcelSheets.Count), Excel.Worksheet)
                    W_InExcelSheet.Copy(Before:=W_OutExcelSheet)

                    'Excel終了＆リリース
                    If Not W_InExcelBook Is Nothing Then
                        W_ExcelApp.DisplayAlerts = False
                        W_InExcelBook.Close(False)
                        W_ExcelApp.DisplayAlerts = True
                    End If

                    ExcelMemoryRelease(W_InExcelSheet)
                    ExcelMemoryRelease(W_InExcelSheets)
                    ExcelMemoryRelease(W_InExcelBook)

                Catch ex As Exception
                    ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                    CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"               'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "Excel_Merge"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                    'Excel終了＆リリース
                    CloseExcel(W_ExcelApp, W_ExcelBooks, W_OutExcelBook, W_OutExcelSheets, W_OutExcelSheet)
                    Exit Sub
                End Try
            Next

            '○Excelファイル保存準備

            Try
                Dim WW_Dir As String

                ' 印刷用フォルダ作成
                WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "PRINTWORK"
                ' 格納フォルダ存在確認＆作成(...\PRINTWORK)
                If Directory.Exists(WW_Dir) Then
                Else
                    Directory.CreateDirectory(WW_Dir)
                End If

                ' 格納フォルダ存在確認＆作成(...\PRINTWORK\ユーザーID)
                WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "PRINTWORK" & "\" & CS0050SESSION.USERID
                If Directory.Exists(WW_Dir) Then
                Else
                    Directory.CreateDirectory(WW_Dir)
                End If

            Catch ex As Exception
                ERR = C_MESSAGE_NO.FILE_IO_ERROR

                CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_Folder"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                'Excel終了＆リリース
                CloseExcel(W_ExcelApp, W_ExcelBooks, W_OutExcelBook, W_OutExcelSheets, W_OutExcelSheet)
                Exit Sub
            End Try

            '○Excelファイル保存
            Try
                'URL = "http://" & Dns.GetHostName & "/PRINT/" & ユーザーID & "/" & WW_datetime & ".XLSX"
                URL = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/" & CS0050SESSION.PRINT_ROOT_URL_NAME & "/" & CS0050SESSION.USERID & "/" & WW_datetime & ".XLSX"
                W_OutExcelBook.SaveAs(FILEpath)

            Catch ex As Exception
                ERR = C_MESSAGE_NO.FILE_IO_ERROR

                CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_Save"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                'Excel終了＆リリース
                CloseExcel(W_ExcelApp, W_ExcelBooks, W_OutExcelBook, W_OutExcelSheets, W_OutExcelSheet)
                Exit Sub
            End Try

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Open"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            'Excel終了＆リリース
            CloseExcel(W_ExcelApp, W_ExcelBooks, W_OutExcelBook, W_OutExcelSheets, W_OutExcelSheet)
            Exit Sub
        End Try

        'Excel終了＆リリース
        CloseExcel(W_ExcelApp, W_ExcelBooks, W_OutExcelBook, W_OutExcelSheets, W_OutExcelSheet)

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' Excel操作のメモリ開放
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="objCom"></param>
    ''' <remarks></remarks>
    Public Sub ExcelMemoryRelease(Of T As Class)(ByRef objCom As T)

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
    ''' Excel終了＆リリース
    ''' </summary>
    ''' <param name="W_ExcelApp"></param>
    ''' <param name="W_ExcelBooks"></param>
    ''' <param name="W_ExcelBook"></param>
    ''' <param name="W_ExcelSheets"></param>
    ''' <param name="W_ExcelSheet"></param>
    ''' <remarks></remarks>
    Public Sub CloseExcel(W_ExcelApp As Excel.Application, W_ExcelBooks As Excel.Workbooks, W_ExcelBook As Excel.Workbook, W_ExcelSheets As Excel.Sheets, W_ExcelSheet As Excel.Worksheet)

        'Excel終了＆リリース
        If Not W_ExcelBook Is Nothing Then
            W_ExcelApp.DisplayAlerts = False
            W_ExcelBook.Close(False)
            W_ExcelApp.DisplayAlerts = True
        End If

        ExcelMemoryRelease(W_ExcelSheet)        'ExcelSheet の解放
        ExcelMemoryRelease(W_ExcelSheets)       'ExcelSheets の解放
        ExcelMemoryRelease(W_ExcelBook)         'ExcelBook の解放
        ExcelMemoryRelease(W_ExcelBooks)        'ExcelBooks の解放

        Try
            W_ExcelApp.Visible = True
        Catch ex As Exception
        End Try

        Try
            W_ExcelApp.Quit()
        Catch ex As Exception
        End Try

        ExcelMemoryRelease(W_ExcelApp)          'ExcelApp を解放

    End Sub

End Structure

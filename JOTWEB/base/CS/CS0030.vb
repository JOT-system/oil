Option Strict On
Imports System.Web
Imports System.IO
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices

''' <summary>
''' 帳票出力
''' </summary>
''' <remarks></remarks>
Public Structure CS0030REPORT

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' プロファイルID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PROFID() As String

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' 帳票ID
    ''' </summary>
    ''' <value>帳票ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property REPORTID() As String

    ''' <summary>
    ''' 出力ファイル形式
    ''' </summary>
    ''' <value>出力ファイル形式</value>
    ''' <returns></returns>
    ''' <remarks>pdf, csv, xlsx, xlsm</remarks>
    Public Property FILEtyp() As String

    ''' <summary>
    ''' データ参照tabledata
    ''' </summary>
    ''' <value>データ参照tabledata</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TBLDATA() As DataTable

    ''' <summary>
    ''' 出力Dir＋ファイル名
    ''' </summary>
    ''' <value></value>
    ''' <returns>出力Dir＋ファイル名</returns>
    ''' <remarks></remarks>
    Public Property FILEpath() As String

    ''' <summary>
    ''' 出力URL＋ファイル名
    ''' </summary>
    ''' <value></value>
    ''' <returns>出力URL＋ファイル名</returns>
    ''' <remarks></remarks>
    Public Property URL() As String

    ''' <summary>
    ''' 対象日付
    ''' </summary>
    ''' <value>対象日付</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TARGETDATE() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value></value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String

    Private Const METHOD_NAME = "CS0030REPORT"

    Public Sub CS0030REPORT()

        '■共通宣言
        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
        Dim CS0021PROFXLS As New CS0021PROFXLS                  'プロファイル（XLS）取得
        Dim CS0028STRUCT As New CS0028STRUCT                    '構造取得
        Dim CS0050SESSION As New CS0050SESSION                  'セッション情報操作処理

        Dim W_ExcelApp As Excel.Application = Nothing
        Dim W_ExcelBooks As Excel.Workbooks = Nothing
        Dim W_ExcelBook As Excel.Workbook = Nothing
        Dim W_ExcelSheets As Excel.Sheets = Nothing
        Dim W_ExcelSheet As Excel.Worksheet = Nothing
        Dim PROFCODE As String = String.Empty
        '●In PARAMチェック
        'CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
        End If

        'MAPID
        If IsNothing(MAPID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        'REPORTID
        If IsNothing(REPORTID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "REPORTID"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If
        'REPORTID のコード
        Dim GS0032 As New GS0032FIXVALUElst
        GS0032.CAMPCODE = CAMPCODE
        GS0032.CLAS = "CO0004_RPRTPROFID"
        GS0032.STDATE = Date.Now
        GS0032.ENDDATE = Date.Now
        GS0032.GS0032FIXVALUElst()
        If Not isNormal(GS0032.ERR) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "REPORTID-CODE NOT EXIST"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If
        PROFCODE = If(IsNothing(GS0032.VALUE1.Items.FindByText(PROFID)), C_DEFAULT_DATAKEY, GS0032.VALUE1.Items.FindByText(PROFID).Value)
        If String.IsNullOrEmpty(PROFCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "REPORTID-CODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
        End If
        'FILEtyp
        If IsNothing(FILEtyp) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "FILEtyp"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        'TBLDATA
        If IsNothing(TBLDATA) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TBLDATA"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        '■ユーザーID
        If String.IsNullOrEmpty(CS0050SESSION.USERID) Then
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "APSRVname ERR"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = "APSRVname ERR"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        '■対象日付
        If IsNothing(TARGETDATE) OrElse TARGETDATE = "" Then
            TARGETDATE = Date.Now.ToString("yyyy/MM/dd")
        End If

        '■出力レイアウト取得
        CS0021PROFXLS.CAMPCODE = CAMPCODE
        CS0021PROFXLS.PROFID = PROFID
        CS0021PROFXLS.MAPID = MAPID
        CS0021PROFXLS.REPORTID = REPORTID
        CS0021PROFXLS.TARGETDATE = TARGETDATE
        CS0021PROFXLS.CS0021PROFXLS()
        If Not isNormal(CS0021PROFXLS.ERR) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        '■繰返データ見出し取得　…　I_DataKeyの指定・構造よりクリア絵師見出しを作成する。
        Dim WW_CELL_KEY As List(Of String)                              '列見出位置決め用のKEY

        WW_CELL_KEY = New List(Of String)
        For i As Integer = 0 To CS0021PROFXLS.STRUCT.Count - 1
            If CS0021PROFXLS.STRUCT(i) <> "" Then
                CS0028STRUCT.CAMPCODE = CAMPCODE
                CS0028STRUCT.STRUCT = CS0021PROFXLS.STRUCT(i)
                CS0028STRUCT.CS0028STRUCT()
                If isNormal(CS0028STRUCT.ERR) Then

                    If WW_CELL_KEY.Count = 0 Then
                        For CNT As Integer = 0 To CS0028STRUCT.CODE.Count - 1
                            WW_CELL_KEY.Add(CS0028STRUCT.CODE(CNT))
                        Next
                    Else
                        '複数定義された構造の列数が全て一致
                        If WW_CELL_KEY.Count = CS0028STRUCT.CODE.Count Then
                            For CNT As Integer = 0 To WW_CELL_KEY.Count - 1
                                WW_CELL_KEY(CNT) = WW_CELL_KEY(CNT) & "_" & CS0028STRUCT.CODE(CNT)
                            Next
                        Else
                            ERR = C_MESSAGE_NO.EXCEL_COLUMNS_FORMAT_ERROR

                            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS"
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                            CS0011LOGWRITE.TEXT = "Excel書式(列構造定義)不良"
                            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                            Exit Sub
                        End If
                    End If
                Else
                    ERR = C_MESSAGE_NO.EXCEL_COLUMNS_FORMAT_ERROR

                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                    CS0011LOGWRITE.TEXT = "Excel書式(列構造定義)不良"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End If
            End If
        Next

        '■明細データソート・性能対策

        'ソート用Cell追加
        TBLDATA.Columns.Add("ROWKEY", GetType(String))             '行SORT・マッチング用Key
        TBLDATA.Columns.Add("CELLNO", GetType(Integer))            '繰返データ列番号
        TBLDATA.Columns.Add("ROWCNT", GetType(Integer))            '(I_DataKey+列番号)内順番

        '性能対策用(明細)       …　前提：CS0021PROFXLS出力パラListは、SORT順に格納されている
        Dim WW_I_TITLEKBN As List(Of String) = New List(Of String)
        Dim WW_I_FIELD As List(Of String) = New List(Of String)
        Dim WW_I_FIELDNAME As List(Of String) = New List(Of String)
        Dim WW_I_STRUCT As List(Of String) = New List(Of String)
        Dim WW_I_POSIX As List(Of Integer) = New List(Of Integer)
        Dim WW_I_POSIY As List(Of Integer) = New List(Of Integer)
        Dim WW_I_WIDTH As List(Of Integer) = New List(Of Integer)
        Dim WW_I_EFFECT As List(Of String) = New List(Of String)
        Dim WW_I_SORT As List(Of Integer) = New List(Of Integer)

        '性能対策用(明細データ)
        Dim WW_R_TITLEKBN As List(Of String) = New List(Of String)
        Dim WW_R_FIELD As List(Of String) = New List(Of String)
        Dim WW_R_FIELDNAME As List(Of String) = New List(Of String)
        Dim WW_R_STRUCT As List(Of String) = New List(Of String)
        Dim WW_R_POSIX As List(Of Integer) = New List(Of Integer)
        Dim WW_R_POSIY As List(Of Integer) = New List(Of Integer)
        Dim WW_R_WIDTH As List(Of Integer) = New List(Of Integer)
        Dim WW_R_EFFECT As List(Of String) = New List(Of String)

        For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
            If CS0021PROFXLS.TITLEKBN(i) = "I" AndAlso CS0021PROFXLS.EFFECT(i) = "Y" AndAlso CS0021PROFXLS.POSIY(i) > 0 AndAlso CS0021PROFXLS.POSIX(i) > 0 Then
                WW_I_TITLEKBN.Add(CS0021PROFXLS.TITLEKBN(i))
                WW_I_FIELD.Add(CS0021PROFXLS.FIELD(i))
                WW_I_FIELDNAME.Add(CS0021PROFXLS.FIELDNAME(i))
                WW_I_STRUCT.Add(CS0021PROFXLS.STRUCT(i))
                WW_I_POSIX.Add(CS0021PROFXLS.POSIX(i))
                WW_I_POSIY.Add(CS0021PROFXLS.POSIY(i))
                WW_I_WIDTH.Add(CS0021PROFXLS.WIDTH(i))
                WW_I_EFFECT.Add(CS0021PROFXLS.EFFECT(i))
                WW_I_SORT.Add(CS0021PROFXLS.SORT(i))
            End If

            If ((CS0021PROFXLS.TITLEKBN(i) = "I_DataKey") Or (CS0021PROFXLS.TITLEKBN(i) = "I_Data")) And CS0021PROFXLS.EFFECT(i) = "Y" Then
                WW_R_TITLEKBN.Add(CS0021PROFXLS.TITLEKBN(i))
                WW_R_FIELD.Add(CS0021PROFXLS.FIELD(i))
                WW_R_FIELDNAME.Add(CS0021PROFXLS.FIELDNAME(i))
                WW_R_STRUCT.Add(CS0021PROFXLS.STRUCT(i))
                WW_R_POSIX.Add(CS0021PROFXLS.POSIX(i))
                WW_R_POSIY.Add(CS0021PROFXLS.POSIY(i))
                WW_R_WIDTH.Add(CS0021PROFXLS.WIDTH(i))
                WW_R_EFFECT.Add(CS0021PROFXLS.EFFECT(i))
            End If
        Next

        'データソート準備
        For i As Integer = 0 To TBLDATA.Rows.Count - 1

            '行SORT Key編集
            Dim WW_RowKey As String = ""
            For CNT As Integer = 0 To WW_I_TITLEKBN.Count - 1
                If WW_I_SORT(CNT) <> 0 And WW_I_TITLEKBN(CNT) = "I" Then
                    If WW_RowKey = "" Then
                        WW_RowKey = TBLDATA.Rows(i).Item(WW_I_FIELD(CNT)).ToString
                    Else
                        WW_RowKey = WW_RowKey & "_" & TBLDATA.Rows(i).Item(WW_I_FIELD(CNT)).ToString
                    End If
                End If
            Next
            TBLDATA.Rows(i).Item("ROWKEY") = WW_RowKey

            '列位置決め用Key編集
            Dim WW_CellKey As String = ""
            For CNT As Integer = 0 To WW_R_TITLEKBN.Count - 1
                If WW_R_TITLEKBN(CNT) = "I_DataKey" Then
                    If WW_CellKey = "" Then
                        WW_CellKey = TBLDATA.Rows(i).Item(WW_R_FIELD(CNT)).ToString
                    Else
                        WW_CellKey = WW_CellKey & "_" & TBLDATA.Rows(i).Item(WW_R_FIELD(CNT)).ToString
                    End If
                End If
            Next

            '列位置決め
            '  初期値を枠直後に設定
            Dim WW_CellPosi As Integer = WW_CELL_KEY.Count

            If WW_CellKey <> "" Then
                For k As Integer = 0 To WW_CELL_KEY.Count - 1
                    If WW_CellKey = WW_CELL_KEY(k) Then
                        WW_CellPosi = k
                        Exit For
                    End If
                Next
            End If
            TBLDATA.Rows(i).Item("CELLNO") = WW_CellPosi

            TBLDATA.Rows(i).Item("ROWCNT") = 0
        Next

        'ソート
        Dim WW_TBLDATA_View As DataView = New DataView(TBLDATA)
        Dim WW_TBLDATA_SORTstr As String = "ROWKEY , CELLNO"
        WW_TBLDATA_View.Sort = WW_TBLDATA_SORTstr
        TBLDATA = WW_TBLDATA_View.ToTable

        WW_TBLDATA_View.Dispose()
        WW_TBLDATA_View = Nothing

        'ROWCNT設定
        Dim WW_BreakKey As String = ""
        Dim WW_ROWCNT As Integer = 0
        For i As Integer = 0 To TBLDATA.Rows.Count - 1
            If TBLDATA.Rows(i).Item("ROWKEY").ToString & TBLDATA.Rows(i).Item("CELLNO").ToString = WW_BreakKey Then
                WW_ROWCNT = WW_ROWCNT + 1
            Else
                WW_BreakKey = TBLDATA.Rows(i).Item("ROWKEY").ToString & TBLDATA.Rows(i).Item("CELLNO").ToString
                WW_ROWCNT = 0
            End If

            TBLDATA.Rows(i).Item("ROWCNT") = WW_ROWCNT
        Next

        Dim WW_View As DataView = New DataView(TBLDATA)
        Dim WW_SORTstr As String = "ROWKEY , ROWCNT"
        WW_View.Sort = WW_SORTstr


        '■Excel起動



        Dim WW_ExcelExist As String = ""

        Try
            W_ExcelApp = New Excel.Application
            W_ExcelBooks = W_ExcelApp.Workbooks

            If CS0021PROFXLS.EXCELFILE = "" OrElse Not File.Exists(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & PROFCODE & "\" & MAPID & "\" & CS0021PROFXLS.EXCELFILE) Then
                If File.Exists(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & C_DEFAULT_DATAKEY & "\" & MAPID & "\" & CS0021PROFXLS.EXCELFILE) Then
                    '既存のファイルを開く場合
                    W_ExcelBook = W_ExcelBooks.Open(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & C_DEFAULT_DATAKEY & "\" & MAPID & "\" & CS0021PROFXLS.EXCELFILE)
                    W_ExcelSheets = W_ExcelBook.Worksheets
                    'シート名の取得
                    Dim W_FIND As String = "OFF"
                    For Each xlElement As Excel.Worksheet In W_ExcelSheets
                        If xlElement.Name = "出力" Then
                            W_ExcelSheet = CType(xlElement, Excel.Worksheet)
                            W_FIND = "ON"
                            Exit For
                        End If
                        ExcelMemoryRelease(xlElement)
                    Next

                    If W_FIND = "OFF" Then
                        For Each xlElement As Excel.Worksheet In W_ExcelSheets
                            If xlElement.Name = "入出力" Then
                                W_ExcelSheet = CType(xlElement, Excel.Worksheet)
                                W_FIND = "ON"
                                Exit For
                            End If
                            ExcelMemoryRelease(xlElement)
                        Next
                    End If

                    If W_FIND = "OFF" Then
                        W_ExcelSheet = CType(W_ExcelSheets.Item(1), Excel.Worksheet)
                    End If
                    WW_ExcelExist = "ON"
                Else
                    '新規のファイルを開く場合
                    W_ExcelBook = W_ExcelBooks.Add(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & C_DEFAULT_DATAKEY & "\COMMON\書式無.xlsx")
                    W_ExcelSheets = W_ExcelBook.Worksheets
                    W_ExcelSheet = CType(W_ExcelSheets.Item(1), Excel.Worksheet)
                    WW_ExcelExist = ""
                End If
            Else
                '既存のファイルを開く場合
                W_ExcelBook = W_ExcelBooks.Open(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & PROFCODE & "\" & MAPID & "\" & CS0021PROFXLS.EXCELFILE)
                W_ExcelSheets = W_ExcelBook.Worksheets
                'シート名の取得
                Dim xlElement As Excel.Worksheet = Nothing
                Dim W_FIND As String = "OFF"
                For Each xlElement In W_ExcelSheets
                    If xlElement.Name = "出力" Then
                        W_ExcelSheet = CType(xlElement, Excel.Worksheet)
                        W_FIND = "ON"
                        Exit For
                    End If
                    ExcelMemoryRelease(xlElement)
                Next

                If W_FIND = "OFF" Then
                    For Each xlElement In W_ExcelSheets
                        If xlElement.Name = "入出力" Then
                            W_ExcelSheet = CType(xlElement, Excel.Worksheet)
                            W_FIND = "ON"
                            Exit For
                        End If
                        ExcelMemoryRelease(xlElement)
                    Next
                End If

                If W_FIND = "OFF" Then
                    W_ExcelSheet = CType(W_ExcelSheets.Item(1), Excel.Worksheet)
                End If

                '2016/07/22 add miyake
                Dim WW_STR As String = CS0021PROFXLS.EXCELFILE.ToUpper()
                If WW_STR Like "*.XLSM" Then
                    FILEtyp = "XLSM"
                End If
                WW_ExcelExist = "ON"
            End If

            W_ExcelApp.Visible = False

            '自動計算を止める
            W_ExcelApp.Calculation = Excel.XlCalculation.xlCalculationManual

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Open"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
            Exit Sub
        End Try

        '■Excelデータ処理
        Dim WW_Cells As Excel.Range = Nothing
        Dim WW_STARTpoint As Excel.Range = Nothing
        Dim WW_ENDpoint As Excel.Range = Nothing
        Dim WW_EXCELrange As Excel.Range = Nothing
        Dim WW_HENSYUrange(,) As Object = Nothing

        '～～～～～ データ設定 (開始) ～～～～～～～～～～～～～～～～

        If CS0021PROFXLS.POSISTART = 0 Then
            CS0021PROFXLS.POSISTART = 1
        End If
        If CS0021PROFXLS.POSI_T_X_MAX = 0 Then
            CS0021PROFXLS.POSI_T_X_MAX = 2
        End If
        If CS0021PROFXLS.POSI_T_Y_MAX = 0 Then
            CS0021PROFXLS.POSI_T_Y_MAX = 1
        End If

        If CS0021PROFXLS.POSI_I_X_MAX = 0 Then
            CS0021PROFXLS.POSI_I_X_MAX = 1
        End If
        If CS0021PROFXLS.POSI_I_Y_MAX = 0 Then
            CS0021PROFXLS.POSI_I_Y_MAX = 1
        End If

        '○Excel(タイトル)表示
        ' ******************************************************
        ' *    タイトル                                        *
        ' ******************************************************

        '    タイトル区分(=H)の場合

        '異常終了用
        Dim WW_Range_str As String = ""

        Try
            WW_Range_str = "MaxX:" & CS0021PROFXLS.POSI_T_Y_MAX.ToString & "_MaxY:" & CS0021PROFXLS.POSI_T_X_MAX.ToString
            'Dim WW_HENSYUrange(CS0021UPROFXLS.POSI_T_Y_MAX - 1, CS0021UPROFXLS.POSI_T_X_MAX - 1) As Object          '行編集領域　　※開始位置(0,0) …　object

            '　タイトル(1行目)範囲指定
            WW_Cells = W_ExcelSheet.Cells
            WW_STARTpoint = DirectCast(WW_Cells.Item(1, 1), Excel.Range)        'A1
            WW_ENDpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSI_T_Y_MAX, CS0021PROFXLS.POSI_T_X_MAX), Excel.Range)
            WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)      'データの入力セル範囲  ※開始位置(1,1)　…　Excel

            WW_HENSYUrange = CType(WW_EXCELrange.Value, Object(,))

            ''　書式Excel内文字の退避
            'Dim WW_DEFULTrange(CS0021UPROFXLS.POSI_T_Y_MAX - 1, CS0021UPROFXLS.POSI_T_X_MAX - 1) As Object          '行編集領域　　※開始位置(1,1) …　object
            'WW_DEFULTrange = WW_EXCELrange.Value

            'If IsNothing(WW_EXCELrange.Value) Then
            'Else
            '    For i As Integer = 1 To (CS0021UPROFXLS.POSI_T_Y_MAX)
            '        For CNT As Integer = 1 To (CS0021UPROFXLS.POSI_T_X_MAX)
            '            WW_HENSYUrange(i - 1, CNT - 1) = WW_DEFULTrange(i, CNT)
            '        Next
            '    Next
            'End If

            '　タイトル設定(明細と同一レイアウトで明細タイトルを設定する)
            '    ※タイトルは、Ecel・セル位置(A1)を基準として、指定された位置に項目をセット
            '    ※タイトルに表示する項目指定値(Field)は、GridViewの１行目情報を表示する
            For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                If CS0021PROFXLS.TITLEKBN(i) = "T" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIX(i) > 0 And CS0021PROFXLS.POSIY(i) > 0 Then
                    Select Case CS0021PROFXLS.FIELD(i)
                        Case "EXCELTITOL"                'CS0021UPROFXLSパラメータ(FIELDNAME)をセット
                            'WW_HENSYUrange(CS0021UPROFXLS.POSIY(i) - 1, CS0021UPROFXLS.POSIX(i) - 1) = CS0021UPROFXLS.FIELDNAME(i)
                            WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)) = CS0021PROFXLS.FIELDNAME(i)
                        Case "REPORTID"                  'CS0021UPROFXLSパラメータ(REPORTID)をセット
                            'WW_HENSYUrange(CS0021UPROFXLS.POSIY(i) - 1, CS0021UPROFXLS.POSIX(i) - 1) = "ID:" & CS0021UPROFXLS.REPORTID
                            WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)) = "ID:" & CS0021PROFXLS.REPORTID & ";" & CS0021PROFXLS.PROFID
                        Case Else                        'Tableの1行目の該当項目値をセット
                            Try
                                'WW_HENSYUrange(CS0021UPROFXLS.POSIY(i) - 1, CS0021UPROFXLS.POSIX(i) - 1) = TBLDATA.Rows(0).Item(CS0021UPROFXLS.FIELD(i)).ToString
                                WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)) = TBLDATA.Rows(0).Item(CS0021PROFXLS.FIELD(i)).ToString
                            Catch ex As Exception
                                '項目名が無い場合、無視
                            End Try
                    End Select
                End If
            Next

            'セルへデータの入力
            WW_EXCELrange.Value = WW_HENSYUrange

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Titol_Range"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = WW_Range_str
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
            Exit Sub
        Finally
            'Excel.Range 解放
            ExcelMemoryRelease(WW_HENSYUrange)
            ExcelMemoryRelease(WW_Cells)
            ExcelMemoryRelease(WW_STARTpoint)
            ExcelMemoryRelease(WW_ENDpoint)
            ExcelMemoryRelease(WW_EXCELrange)
        End Try

        '○画面選択明細(Table)からExcel(明細ヘッダー)へ表示
        ' ******************************************************
        ' *    明細ヘッダー(I)                                 *
        ' ******************************************************

        Try
            WW_Range_str = "MaxX:" & CS0021PROFXLS.POSI_I_Y_MAX.ToString & "_MaxY:" & CS0021PROFXLS.POSI_I_X_MAX.ToString
            'Dim WW_HENSYUrange(CS0021UPROFXLS.POSI_I_Y_MAX - 1, CS0021UPROFXLS.POSI_I_X_MAX - 1) As Object                                      '行編集領域　　※開始位置(0,0) …　object

            '　明細タイトル範囲指定
            WW_Cells = W_ExcelSheet.Cells
            WW_STARTpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART, 1), Excel.Range)         '指定された行開始のA列
            WW_ENDpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART + CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_I_X_MAX), Excel.Range)       '指定された行+明細行
            WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)                              'データの入力セル範囲

            WW_HENSYUrange = CType(WW_EXCELrange.Value, Object(,))

            ''　書式Excel内文字の退避
            'Dim WW_DEFULTrange(CS0021UPROFXLS.POSI_I_Y_MAX - 1, CS0021UPROFXLS.POSI_I_X_MAX - 1) As Object
            'WW_DEFULTrange = WW_EXCELrange.Value

            'If IsNothing(WW_EXCELrange.Value) Then
            'Else
            '    For i As Integer = 1 To (CS0021UPROFXLS.POSI_I_Y_MAX)
            '        For CNT As Integer = 1 To (CS0021UPROFXLS.POSI_I_X_MAX)
            '            WW_HENSYUrange(i - 1, CNT - 1) = WW_DEFULTrange(i, CNT)
            '        Next
            '    Next
            'End If

            '　明細タイトル設定
            For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                '有効行を対象とする
                If CS0021PROFXLS.TITLEKBN(i) = "I" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIX(i) > 0 And CS0021PROFXLS.POSIY(i) > 0 Then
                    'WW_HENSYUrange(CS0021UPROFXLS.POSIY(i) - 1, CS0021UPROFXLS.POSIX(i) - 1) = CS0021UPROFXLS.FIELDNAME(i)
                    WW_HENSYUrange(CS0021PROFXLS.POSIY(i), CS0021PROFXLS.POSIX(i)) = CS0021PROFXLS.FIELDNAME(i)
                End If
            Next

            'セルへデータの入力
            WW_EXCELrange.Value = WW_HENSYUrange

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_DetailHeader_Range"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = WW_Range_str
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
            Exit Sub
        Finally
            'Excel.Range 解放
            ExcelMemoryRelease(WW_HENSYUrange)
            ExcelMemoryRelease(WW_Cells)
            ExcelMemoryRelease(WW_STARTpoint)
            ExcelMemoryRelease(WW_ENDpoint)
            ExcelMemoryRelease(WW_EXCELrange)
        End Try

        ' ******************************************************
        ' *    明細ヘッダー(I_DataKey)                         *
        ' ******************************************************

        If WW_CELL_KEY.Count > 0 Then
            Try
                '繰返明細部タイトル印刷有無判定
                ' And CS0021UPROFXLS.POSIY(i) > 0 And CS0021UPROFXLS.POSIX(i) > 0 
                Dim WW_DataKeyPrint As String = "Print NG"
                For i As Integer = 0 To WW_R_TITLEKBN.Count - 1
                    If WW_R_TITLEKBN(i) = "I_DataKey" And WW_R_POSIY(i) <> 0 And WW_R_POSIX(i) <> 0 Then
                        'WW_DataKeyPrint = "Print OK"
                        Exit For
                    End If
                Next

                If WW_DataKeyPrint = "Print OK" Then
                    WW_Range_str = "MaxX:" & CS0021PROFXLS.POSI_R_Y_MAX.ToString & "_MaxY:" & CS0021PROFXLS.POSI_R_X_MAX.ToString
                    'Dim WW_HENSYUrange(0, WW_CELL_KEY.Count * CS0021UPROFXLS.POSI_R_X_MAX - 1) As Object                                      '行編集領域　　※開始位置(0,0) …　object

                    '　明細タイトル範囲指定
                    WW_Cells = W_ExcelSheet.Cells
                    WW_STARTpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART, CS0021PROFXLS.POSI_I_X_MAX + 1), Excel.Range)       '指定された行開始のA列
                    WW_ENDpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART, WW_CELL_KEY.Count * CS0021PROFXLS.POSI_R_X_MAX + CS0021PROFXLS.POSI_I_X_MAX), Excel.Range)       '指定された行+明細行
                    WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)                                                          'データの入力セル範囲

                    '　明細タイトル設定
                    For i As Integer = 0 To WW_CELL_KEY.Count - 1
                        WW_HENSYUrange(0, i * CS0021PROFXLS.POSI_R_X_MAX) = WW_CELL_KEY(i)
                    Next

                    WW_EXCELrange.Value = WW_HENSYUrange                                                                                          'セルへデータの入力

                End If
            Catch ex As Exception
                ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_DetailHeader_Range"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = WW_Range_str
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                'Excel終了＆リリース
                CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
                Exit Sub
            Finally
                'Excel.Range 解放
                ExcelMemoryRelease(WW_HENSYUrange)
                ExcelMemoryRelease(WW_Cells)
                ExcelMemoryRelease(WW_STARTpoint)
                ExcelMemoryRelease(WW_ENDpoint)
                ExcelMemoryRelease(WW_EXCELrange)
            End Try
        End If

        If WW_CELL_KEY.Count <= 0 Then
            '○画面選択明細(Table)からExcel(明細)へ表示
            ' ******************************************************
            ' *    明細                                            *
            ' ******************************************************

            '　明細範囲指定(全Excel明細範囲を指定する)

            'ERR表示用文字編集
            WW_Range_str = "MaxX:" & CS0021PROFXLS.POSI_I_Y_MAX.ToString & "_MaxY:" & CS0021PROFXLS.POSI_I_X_MAX.ToString

            Try
                'メモリー不足対策として3000件づつ処理する(Range操作レスポンスは悪いので１件づつ処理できない)
                For wLoopCNT As Integer = 0 To CInt((Math.Ceiling(WW_View.Count / 3000) - 1))

                    If wLoopCNT = (Math.Ceiling(WW_View.Count / 3000) - 1) Then
                        '〇出力編集
                        '3000件の編集Work領域定義         ※開始位置(0,0)
                        ReDim WW_HENSYUrange((WW_View.Count Mod 3000) * CS0021PROFXLS.POSI_I_Y_MAX - 1, CS0021PROFXLS.POSI_I_X_MAX - 1)       '行編集領域　　※開始位置(0,0) …　object

                        '編集処理(WW_View-->WW_HENSYUrange)
                        For i As Integer = (wLoopCNT * 3000) To WW_View.Count - 1
                            For CNT As Integer = 0 To WW_I_TITLEKBN.Count - 1
                                Try
                                    WW_HENSYUrange((i - wLoopCNT * 3000) * CS0021PROFXLS.POSI_I_Y_MAX + WW_I_POSIY(CNT) - 1, WW_I_POSIX(CNT) - 1) = WW_View(i)(WW_I_FIELD(CNT)).ToString
                                Catch ex As Exception
                                    Dim iiii As Integer = 0
                                End Try
                            Next
                        Next

                        '〇Excel貼付
                        WW_Cells = W_ExcelSheet.Cells
                        WW_STARTpoint = DirectCast(WW_Cells.Item(((wLoopCNT * 3000) + 1) * CS0021PROFXLS.POSI_I_Y_MAX + CS0021PROFXLS.POSISTART, 1), Excel.Range)
                        WW_ENDpoint = DirectCast(WW_Cells.Item(((wLoopCNT * 3000 + WW_View.Count Mod 3000) + 1) * CS0021PROFXLS.POSI_I_Y_MAX + CS0021PROFXLS.POSISTART - 1, CS0021PROFXLS.POSI_I_X_MAX), Excel.Range)
                        WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)

                        WW_EXCELrange.NumberFormatLocal = "@"                           '明細範囲の書式(文字形式)指定　…　文字化け対策
                        WW_EXCELrange.Value = WW_HENSYUrange                            'セルへデータの入力
                    Else
                        '〇出力編集
                        '3000件の編集Work領域定義         ※開始位置(0,0)
                        ReDim WW_HENSYUrange(3000 * CS0021PROFXLS.POSI_I_Y_MAX - 1, CS0021PROFXLS.POSI_I_X_MAX - 1)       '行編集領域　　※開始位置(0,0) …　object

                        '編集処理(WW_View-->WW_HENSYUrange)
                        For i As Integer = (wLoopCNT * 3000) To ((wLoopCNT + 1) * 3000 - 1)
                            For CNT As Integer = 0 To WW_I_TITLEKBN.Count - 1
                                WW_HENSYUrange((i - wLoopCNT * 3000) * CS0021PROFXLS.POSI_I_Y_MAX + WW_I_POSIY(CNT) - 1, WW_I_POSIX(CNT) - 1) = WW_View(i)(WW_I_FIELD(CNT)).ToString
                            Next
                        Next

                        '〇Excel貼付
                        WW_Cells = W_ExcelSheet.Cells
                        WW_STARTpoint = DirectCast(WW_Cells.Item(((wLoopCNT) * 3000 + 1) * CS0021PROFXLS.POSI_I_Y_MAX + CS0021PROFXLS.POSISTART, 1), Excel.Range)
                        WW_ENDpoint = DirectCast(WW_Cells.Item(((wLoopCNT + 1) * 3000 + 1) * CS0021PROFXLS.POSI_I_Y_MAX + CS0021PROFXLS.POSISTART - 1, CS0021PROFXLS.POSI_I_X_MAX), Excel.Range)
                        WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)

                        WW_EXCELrange.NumberFormatLocal = "@"                           '明細範囲の書式(文字形式)指定　…　文字化け対策
                        WW_EXCELrange.Value = WW_HENSYUrange                            'セルへデータの入力
                    End If

                    'Excel.Range 解放
                    ExcelMemoryRelease(WW_HENSYUrange)
                    ExcelMemoryRelease(WW_Cells)
                    ExcelMemoryRelease(WW_STARTpoint)
                    ExcelMemoryRelease(WW_ENDpoint)
                    ExcelMemoryRelease(WW_EXCELrange)
                Next
            Catch ex As Exception
                ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_Detail_Range"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = WW_Range_str
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                'Excel終了＆リリース
                CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
                Exit Sub
            Finally
                'Excel.Range 解放
                ExcelMemoryRelease(WW_HENSYUrange)
                ExcelMemoryRelease(WW_Cells)
                ExcelMemoryRelease(WW_STARTpoint)
                ExcelMemoryRelease(WW_ENDpoint)
                ExcelMemoryRelease(WW_EXCELrange)
            End Try
        Else

            '○画面選択明細(Table)からExcel(明細・繰返)へ表示
            ' ******************************************************
            ' *    明細＋明細繰返データ                            *
            ' ******************************************************
            Try
                'ERR表示用文字編集
                WW_Range_str = "MaxY:" & CS0021PROFXLS.POSI_R_Y_MAX.ToString & "_MaxX:" & CS0021PROFXLS.POSI_I_X_MAX.ToString

                '繰返明細(全体)の行編集Work領域定義(行：見出 + データ - 1 、列：行ヘッダー+1)        WW_CELL_KEY.Count ※開始位置(0,0)
                'Dim WW_HENSYUrange((TBLDATA.Rows.Count + 1) * CS0021UPROFXLS.POSI_I_Y_MAX - 1 + 1, _
                '                   CS0021UPROFXLS.POSI_I_X_MAX + (CS0021UPROFXLS.POSI_R_X_MAX * WW_CELL_KEY.Count) - 1 + 1) As Object
                ReDim WW_HENSYUrange((TBLDATA.Rows.Count + 1) * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) - 1 + 1, _
                                   CS0021PROFXLS.POSI_I_X_MAX + (CS0021PROFXLS.POSI_R_X_MAX * WW_CELL_KEY.Count) - 1 + 1)

                'Excel操作対象領域定義
                WW_Cells = W_ExcelSheet.Cells
                '・行：開始＋見出、列：１
                WW_STARTpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART + CS0021PROFXLS.POSI_I_Y_MAX, 1), Excel.Range)
                '・行：開始＋見出＋データ、列：行ヘッダー＋データ＋エラー表示行
                'WW_ENDpoint = _
                '    W_ExcelSheet.Cells.Item(CS0021UPROFXLS.POSISTART + CS0021UPROFXLS.POSI_I_Y_MAX + (TBLDATA.Rows.Count + 1) * CS0021UPROFXLS.POSI_I_Y_MAX, _
                '                            CS0021UPROFXLS.POSI_I_X_MAX + CS0021UPROFXLS.POSI_R_X_MAX * WW_CELL_KEY.Count + 1)
                WW_ENDpoint = DirectCast(WW_Cells.Item(CS0021PROFXLS.POSISTART + CS0021PROFXLS.POSI_I_Y_MAX + (TBLDATA.Rows.Count + 1) * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX), _
                                                      CS0021PROFXLS.POSI_I_X_MAX + CS0021PROFXLS.POSI_R_X_MAX * WW_CELL_KEY.Count + 1), Excel.Range)
                'データの入力セル範囲
                WW_EXCELrange = W_ExcelSheet.Range(WW_STARTpoint, WW_ENDpoint)

                '明細範囲の書式(文字形式)指定　…　文字化け対策
                WW_EXCELrange.NumberFormatLocal = "@"

                '行ブレイク用ワーク
                Dim WW_LineCNT As Integer = 0
                Dim WW_LineKEY As String = ""

                '明細・繰返設定
                For i As Integer = 0 To WW_View.Count - 1

                    If WW_LineKEY = "" Then
                        WW_LineKEY = WW_View(i)("ROWKEY").ToString & "_" & WW_View(i)("ROWCNT").ToString
                    End If

                    If WW_LineKEY <> (WW_View(i)("ROWKEY").ToString & "_" & WW_View(i)("ROWCNT").ToString) Then
                        WW_LineKEY = WW_View(i)("ROWKEY").ToString & "_" & WW_View(i)("ROWCNT").ToString
                        WW_LineCNT = WW_LineCNT + 1
                    End If

                    '明細セット
                    For CNT As Integer = 0 To WW_I_TITLEKBN.Count - 1
                        Try
                            'WW_HENSYUrange(WW_LineCNT * CS0021UPROFXLS.POSI_I_Y_MAX + WW_I_POSIY(CNT) - 1, WW_I_POSIX(CNT) - 1) = WW_View(i)(WW_I_FIELD(CNT)).ToString
                            WW_HENSYUrange(WW_LineCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) + WW_I_POSIY(CNT) - 1, WW_I_POSIX(CNT) - 1) = WW_View(i)(WW_I_FIELD(CNT)).ToString
                        Catch ex As Exception
                            '項目名が無い場合、無視
                        End Try
                    Next

                    '繰返データセット
                    If CInt(WW_View(i)("CELLNO")) > (WW_CELL_KEY.Count - 1) Then
                        '列位置決め用KeyにHitしない場合、メッセージをセット
                        'WW_HENSYUrange(WW_LineCNT * CS0021UPROFXLS.POSI_I_Y_MAX, CS0021UPROFXLS.POSI_I_X_MAX + WW_CELL_KEY.Count * CS0021UPROFXLS.POSI_R_X_MAX) = "★表示出来ないデータ有(該当列無)"
                        WW_HENSYUrange(WW_LineCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX), CS0021PROFXLS.POSI_I_X_MAX + WW_CELL_KEY.Count * CS0021PROFXLS.POSI_R_X_MAX) = "★表示出来ないデータ有(該当列無)"
                    Else
                        'I_Dataは表示。I_DataKeyは表示しない(表題に存在する為)。
                        For CNT As Integer = 0 To WW_R_TITLEKBN.Count - 1
                            If (WW_R_TITLEKBN(CNT) = "I_Data") And WW_R_POSIY(CNT) > 0 And WW_R_POSIX(CNT) > 0 Then
                                Try
                                    'WW_HENSYUrange(WW_LineCNT * CS0021UPROFXLS.POSI_I_Y_MAX + WW_R_POSIY(CNT) - 1, CS0021UPROFXLS.POSI_I_X_MAX + WW_View(i)("CELLNO") * CS0021UPROFXLS.POSI_R_X_MAX + WW_R_POSIX(CNT) - 1) = WW_View(i)(WW_R_FIELD(CNT)).ToString
                                    WW_HENSYUrange(WW_LineCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) + WW_R_POSIY(CNT) - 1, CS0021PROFXLS.POSI_I_X_MAX + CInt(WW_View(i)("CELLNO")) * CS0021PROFXLS.POSI_R_X_MAX + WW_R_POSIX(CNT) - 1) = WW_View(i)(WW_R_FIELD(CNT)).ToString
                                Catch ex As Exception
                                    '項目名が無い場合、無視
                                End Try
                            End If
                        Next
                    End If
                Next

                WW_EXCELrange.Value = WW_HENSYUrange          'セルへデータの入力

            Catch ex As Exception
                ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_Detail_Range"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = WW_Range_str
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                'Excel終了＆リリース
                CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
                Exit Sub
            Finally
                'Excel.Range 解放
                ExcelMemoryRelease(WW_HENSYUrange)
                ExcelMemoryRelease(WW_Cells)
                ExcelMemoryRelease(WW_STARTpoint)
                ExcelMemoryRelease(WW_ENDpoint)
                ExcelMemoryRelease(WW_EXCELrange)
            End Try
        End If


        '○Excel書式設定
        Dim WW_Columns As Excel.Range = Nothing
        Dim WW_ColumnsA As Excel.Range = Nothing
        Dim WW_PageSetup As Excel.PageSetup = Nothing

        Try
            '列幅設定
            If WW_ExcelExist = "ON" Then
                'Excel書式ありの場合、書式設定しない
            Else
                For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                    If CS0021PROFXLS.POSIX(i) <> 0 And CS0021PROFXLS.WIDTH(i) <> 0 Then
                        'Dim WW_Columns As Integer = CS0021UPROFXLS.POSIX(i)
                        'W_ExcelSheet.Columns(WW_Columns).ColumnWidth = CS0021UPROFXLS.WIDTH(i)

                        WW_Columns = W_ExcelSheet.Columns
                        WW_ColumnsA = DirectCast(WW_Columns(CS0021PROFXLS.POSIX(i)), Excel.Range)
                        WW_ColumnsA.ColumnWidth = CS0021PROFXLS.WIDTH(i)

                        ExcelMemoryRelease(WW_Columns)
                        ExcelMemoryRelease(WW_ColumnsA)
                    End If
                Next

                ' EXCEL印刷書式設定
                'W_ExcelSheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape
                'W_ExcelSheet.PageSetup.TopMargin = 20
                'W_ExcelSheet.PageSetup.BottomMargin = 20
                'W_ExcelSheet.PageSetup.LeftMargin = 20
                'W_ExcelSheet.PageSetup.RightMargin = 20
                'W_ExcelSheet.PageSetup.Zoom = False
                'W_ExcelSheet.PageSetup.FitToPagesWide = 1 '横を1ページに収める
                'W_ExcelSheet.PageSetup.PrintTitleRows = "$1:$" & (CS0021UPROFXLS.POSISTART + CS0021UPROFXLS.POSI_I_Y_MAX - 1).ToString  'ページタイトル固定
                WW_PageSetup = W_ExcelSheet.PageSetup
                With WW_PageSetup
                    .Orientation = Excel.XlPageOrientation.xlLandscape
                    .TopMargin = 20
                    .BottomMargin = 20
                    .LeftMargin = 20
                    .RightMargin = 20
                    .Zoom = False
                    .FitToPagesWide = 1     '横を1ページに収める
                    .PrintTitleRows = "$1:$" & (CS0021PROFXLS.POSISTART + CS0021PROFXLS.POSI_I_Y_MAX - 1).ToString    'ページタイトル固定
                End With
                ExcelMemoryRelease(WW_PageSetup)
            End If

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_OverLay"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
            Exit Sub
        Finally
            ExcelMemoryRelease(WW_Columns)
            ExcelMemoryRelease(WW_ColumnsA)
            ExcelMemoryRelease(WW_PageSetup)
        End Try

        '～～～～～ データ設定 (終了) ～～～～～～～～～～～～～～～～

        '○EXCEL保存
        Dim WW_Dir As String = ""

        Try
            '　印刷用フォルダ作成
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK"
            '　格納フォルダ存在確認＆作成(...\PRINTWORK)
            If Directory.Exists(WW_Dir) Then
            Else
                Directory.CreateDirectory(WW_Dir)
            End If

            '　格納フォルダ存在確認＆作成(...\PRINTWORK\ユーザーID)
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID
            If Directory.Exists(WW_Dir) Then
            Else
                Directory.CreateDirectory(WW_Dir)
            End If

            '　印刷用フォルダ内不要ファイル削除(当日以外のファイルは削除)
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID
            For Each FileName As String In Directory.GetFiles(WW_Dir, "*.*")
                ' ファイルパスからファイル名を取得
                Do
                    FileName = Mid(FileName, InStr(FileName, "\") + 1, 100)
                Loop Until InStr(FileName, "\") = 0

                If FileName = "" Then
                Else
                    If IsNumeric(Mid(FileName, 1, 8)) And Mid(FileName, 1, 8) = Date.Now.ToString("yyyyMMdd") Then
                    Else
                        For Each tempFile As String In Directory.GetFiles(WW_Dir)
                            File.Delete(tempFile)
                        Next
                        Exit For
                    End If
                End If
            Next
        Catch ex As Exception
            ERR = C_MESSAGE_NO.FILE_IO_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Folder"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            'Excel終了＆リリース
            CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
            Exit Sub
        End Try

        '○保存時の問合せのダイアログを非表示に設定
        W_ExcelApp.DisplayAlerts = False

        '自動計算する
        W_ExcelApp.Calculation = Excel.XlCalculation.xlCalculationAutomatic

        '○ファイル(PDF,CSV)保存
        Dim WW_datetime As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString
        FILEtyp = FILEtyp.ToLower()

        Try
            Select Case FILEtyp
                Case "pdf"
                    FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".pdf"
                    'URL = "http://" & Dns.GetHostName & "/PRINT/" & WW_Term & "/" & WW_datetime & ".pdf"
                    URL = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/PRINT/" & CS0050SESSION.USERID & "/" & WW_datetime & ".pdf"
                    W_ExcelBook.ExportAsFixedFormat(Type:=0,
                         Filename:=FILEpath,
                         Quality:=0,
                         IncludeDocProperties:=True,
                         IgnorePrintAreas:=False,
                         OpenAfterPublish:=False)
                Case "csv"
                    FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".CSV"
                    'URL = "http://" & Dns.GetHostName & "/PRINT/" & WW_Term & "/" & WW_datetime & ".CSV"
                    URL = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/PRINT/" & CS0050SESSION.USERID & "/" & WW_datetime & ".CSV"
                    W_ExcelApp.DisplayAlerts = False
                    W_ExcelSheet.SaveAs(Filename:=FILEpath, FileFormat:=Excel.XlFileFormat.xlCSV)
                Case "xls"
                    FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".XLS"
                    'URL = "http://" & Dns.GetHostName & "/PRINT/" & WW_Term & "/" & WW_datetime & ".XLS"
                    URL = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/PRINT/" & CS0050SESSION.USERID & "/" & WW_datetime & ".XLS"
                    W_ExcelBook.SaveAs(FILEpath)
                Case "xlsx"
                    FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".XLSX"
                    'URL = "http://" & Dns.GetHostName & "/PRINT/" & WW_Term & "/" & WW_datetime & ".XLSX"
                    URL = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/PRINT/" & CS0050SESSION.USERID & "/" & WW_datetime & ".XLSX"
                    W_ExcelBook.SaveAs(FILEpath)
                Case "xlsm"
                    FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".XLSM"
                    'URL = "http://" & Dns.GetHostName & "/PRINT/" & WW_Term & "/" & WW_datetime & ".XLSM"
                    URL = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/PRINT/" & CS0050SESSION.USERID & "/" & WW_datetime & ".XLSM"
                    'W_ExcelBook.SaveAs(FILEpath)
                    W_ExcelBook.SaveAs(Filename:=FILEpath, FileFormat:=Excel.XlFileFormat.xlOpenXMLWorkbookMacroEnabled)
            End Select

        Catch ex As Exception
            ERR = C_MESSAGE_NO.FILE_IO_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Save"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            'Excel終了＆リリース
            CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)
            Exit Sub
        End Try

        '○1秒間表示して終了処理へ
        'System.Threading.Thread.Sleep(1000)

        '○Excel終了＆リリース
        CloseExcel(WW_View, W_ExcelApp, W_ExcelBooks, W_ExcelBook, W_ExcelSheets, W_ExcelSheet)

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
    ''' メモリ開放＆Excel終了＆リリース
    ''' </summary>
    ''' <param name="WW_View"></param>
    ''' <param name="W_ExcelApp"></param>
    ''' <param name="W_ExcelBooks"></param>
    ''' <param name="W_ExcelBook"></param>
    ''' <param name="W_ExcelSheets"></param>
    ''' <param name="W_ExcelSheet"></param>
    ''' <remarks></remarks>
    Public Sub CloseExcel(WW_View As DataView, W_ExcelApp As Excel.Application, W_ExcelBooks As Excel.Workbooks, W_ExcelBook As Excel.Workbook, W_ExcelSheets As Excel.Sheets, W_ExcelSheet As Excel.Worksheet)

        'メモリ開放
        Try
            WW_View.Dispose()
            WW_View = Nothing
        Catch ex As Exception
        End Try

        'Excel終了＆リリース
        If Not W_ExcelBook Is Nothing Then
            W_ExcelApp.DisplayAlerts = False
            W_ExcelBook.Close(False)            '保存する必要は無い
            W_ExcelApp.DisplayAlerts = True
        End If

        ExcelMemoryRelease(W_ExcelSheet)        'ExcelSheet の解放
        ExcelMemoryRelease(W_ExcelSheets)       'ExcelSheets の解放
        ExcelMemoryRelease(W_ExcelBook)         'ExcelBook の解放
        ExcelMemoryRelease(W_ExcelBooks)        'ExcelBooks の解放

        Try
            '自動計算する
            W_ExcelApp.Calculation = Excel.XlCalculation.xlCalculationAutomatic
        Catch err As Exception
        End Try

        Try
            W_ExcelApp.Visible = True
        Catch err As Exception
        End Try

        Try
            W_ExcelApp.Quit()
        Catch err As Exception
        End Try

        ExcelMemoryRelease(W_ExcelApp)          'ExcelApp を解放

    End Sub

End Structure

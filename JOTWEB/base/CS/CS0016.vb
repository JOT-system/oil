Imports System.Data.SqlClient

''' <summary>
''' プロファイル変数取得
''' </summary>
''' <remarks></remarks>
Public Class CS0016ProfMValue
    ''' <summary>
    ''' 値タイプの一覧
    ''' </summary>
    Public Class C_VALUE_TYPE
        ''' <summary>
        ''' 当日日付
        ''' </summary>
        Public Const DATE_NOW As String = "DATENOW"
        ''' <summary>
        ''' 月初
        ''' </summary>
        Public Const DATE_BEGINING_MONTH As String = "DATES"
        ''' <summary>
        ''' 固定日
        ''' </summary>
        Public Const DATE_FIX_VALUE As String = "DATEFIX"
        ''' <summary>
        ''' 固定値
        ''' </summary>
        Public Const VALUE_FIX As String = "FIX"
    End Class

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' プロファイルID
    ''' </summary>
    ''' <value>プロファイルID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PROFID As String

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' 変数
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VARI() As String

    ''' <summary>
    ''' 項目
    ''' </summary>
    ''' <value>項目</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FIELD() As String

    ''' <summary>
    ''' 設定値
    ''' </summary>
    ''' <value>設定値</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE() As String

    ''' <summary>
    ''' 対象年月
    ''' </summary>
    ''' <value>対象年月</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TARGETDATE() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "getInfo"

    ''' <summary>
    ''' プロファイルの変数設定値を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getInfo()
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '●In PARAMチェック
        'PARAM01: MAPID
        If IsNothing(MAPID) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: CAMPCODE

        'PARAM03: VARI
        If IsNothing(VARI) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "VARI"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM04: FIELD
        If IsNothing(FIELD) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "FIELD"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '■対象日付
        If IsNothing(TARGETDATE) OrElse TARGETDATE = "" Then
            TARGETDATE = Date.Now.ToString("yyyy/MM/dd")
        End If

        '●変数情報取得
        '○ DB(S0007_UPROFVARI)検索
        Try
            '○指定ﾊﾟﾗﾒｰﾀで検索
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'I_CAMPCODE検索SQL文
            Dim SQL_Str As String = ""
            If CAMPCODE = "" Then
                SQL_Str =
                     "SELECT rtrim(PROFID) as PROFID " _
                        & ", rtrim(MAPID) as MAPID " _
                        & ", rtrim(CAMPCODE) as CAMPCODE" _
                        & ", rtrim(VARIANT) as VARIANT " _
                        & ", rtrim(TITLEKBN) as TITLEKBN " _
                        & ", SEQ " _
                        & ", rtrim(FIELD) as FIELD " _
                        & ", STYMD " _
                        & ", ENDYMD " _
                        & ", rtrim(VARIANTNAMES) as VARIANTNAMES " _
                        & ", rtrim(TITLENAMES) as TITLENAMES " _
                        & ", rtrim(VALUETYPE) as VALUETYPE " _
                        & ", rtrim(VALUE) as VALUE " _
                        & ", VALUEADDYY " _
                        & ", VALUEADDMM " _
                        & ", VALUEADDDD " _
                        & ", rtrim(DELFLG) as DELFLG " _
                        & " FROM  com.S0023_PROFMVARI " _
                        & " Where PROFID   IN (@P1 ,'" & C_DEFAULT_DATAKEY & "') " _
                        & "   and MAPID    = @P2 " _
                        & "   and VARIANT  = @P4 " _
                        & "   and TITLEKBN = 'I' " _
                        & "   and FIELD    = @P5 " _
                        & "   and STYMD   <= @P6 " _
                        & "   and ENDYMD  >= @P7 " _
                        & "   and DELFLG  <> @P8 " _
                        & " ORDER BY CASE PROFID WHEN '" & C_DEFAULT_DATAKEY & "' THEN 2 ELSE 1 END" _
                        & "         ,CASE CAMPCODE WHEN '" & C_DEFAULT_DATAKEY & "' THEN 2 ELSE 1 END"
            Else
                SQL_Str =
                     "SELECT rtrim(PROFID) as PROFID " _
                        & ", rtrim(MAPID) as MAPID " _
                        & ", rtrim(CAMPCODE) as CAMPCODE" _
                        & ", rtrim(VARIANT) as VARIANT " _
                        & ", rtrim(TITLEKBN) as TITLEKBN " _
                        & ", SEQ " _
                        & ", rtrim(FIELD) as FIELD " _
                        & ", STYMD " _
                        & ", ENDYMD " _
                        & ", rtrim(VARIANTNAMES) as VARIANTNAMES " _
                        & ", rtrim(TITLENAMES) as TITLENAMES " _
                        & ", rtrim(VALUETYPE) as VALUETYPE " _
                        & ", rtrim(VALUE) as VALUE " _
                        & ", VALUEADDYY " _
                        & ", VALUEADDMM " _
                        & ", VALUEADDDD " _
                        & ", rtrim(DELFLG) as DELFLG " _
                        & " FROM  com.S0023_PROFMVARI " _
                        & " Where PROFID   IN (@P1 ,'" & C_DEFAULT_DATAKEY & "') " _
                        & "   and MAPID    = @P2 " _
                        & "   and CAMPCODE IN (@P3 ,'" & C_DEFAULT_DATAKEY & "') " _
                        & "   and VARIANT  = @P4 " _
                        & "   and TITLEKBN = 'I' " _
                        & "   and FIELD    = @P5 " _
                        & "   and STYMD   <= @P6 " _
                        & "   and ENDYMD  >= @P7 " _
                        & "   and DELFLG  <> @P8 " _
                        & " ORDER BY CASE PROFID WHEN '" & C_DEFAULT_DATAKEY & "' THEN 2 ELSE 1 END" _
                        & "         ,CASE CAMPCODE WHEN '" & C_DEFAULT_DATAKEY & "' THEN 2 ELSE 1 END"
            End If

            Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = PROFID
            PARA2.Value = MAPID
            PARA3.Value = CAMPCODE
            PARA4.Value = VARI
            PARA5.Value = FIELD
            PARA6.Value = TARGETDATE
            PARA7.Value = TARGETDATE
            PARA8.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            VALUE = ""
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim WW_DATE As Date = Date.Now
            If SQLdr.Read Then
                Select Case SQLdr("VALUETYPE")
                    Case C_VALUE_TYPE.DATE_BEGINING_MONTH
                        WW_DATE = New DateTime(Date.Now.Year, Date.Now.Month, 1)
                        If SQLdr("VALUEADDYY") <> 0 Then
                            WW_DATE = WW_DATE.AddYears(SQLdr("VALUEADDYY"))
                        End If
                        If SQLdr("VALUEADDMM") <> 0 Then
                            WW_DATE = WW_DATE.AddMonths(SQLdr("VALUEADDMM"))
                        End If
                        If SQLdr("VALUEADDDD") <> 0 Then
                            WW_DATE = WW_DATE.AddDays(SQLdr("VALUEADDDD"))
                        End If
                        VALUE = WW_DATE.ToString("yyyy/MM/dd")
                        ERR = C_MESSAGE_NO.NORMAL
                    Case C_VALUE_TYPE.DATE_NOW
                        WW_DATE = Date.Now
                        If SQLdr("VALUEADDYY") <> 0 Then
                            WW_DATE = WW_DATE.AddYears(SQLdr("VALUEADDYY"))
                        End If
                        If SQLdr("VALUEADDMM") <> 0 Then
                            WW_DATE = WW_DATE.AddMonths(SQLdr("VALUEADDMM"))
                        End If
                        If SQLdr("VALUEADDDD") <> 0 Then
                            WW_DATE = WW_DATE.AddDays(SQLdr("VALUEADDDD"))
                        End If
                        VALUE = WW_DATE.ToString("yyyy/MM/dd")
                        ERR = C_MESSAGE_NO.NORMAL
                    Case C_VALUE_TYPE.DATE_FIX_VALUE
                        Try
                            Date.TryParse(SQLdr("VALUE"), WW_DATE)
                        Catch ex As Exception
                            Exit Sub
                        End Try
                        If SQLdr("VALUEADDYY") <> 0 Then
                            WW_DATE = WW_DATE.AddYears(SQLdr("VALUEADDYY"))
                        End If
                        If SQLdr("VALUEADDMM") <> 0 Then
                            WW_DATE = WW_DATE.AddMonths(SQLdr("VALUEADDMM"))
                        End If
                        If SQLdr("VALUEADDDD") <> 0 Then
                            WW_DATE = WW_DATE.AddDays(SQLdr("VALUEADDDD"))
                        End If
                        VALUE = WW_DATE.ToString("yyyy/MM/dd")
                        ERR = C_MESSAGE_NO.NORMAL
                    Case C_VALUE_TYPE.VALUE_FIX
                        VALUE = SQLdr("VALUE")
                        ERR = C_MESSAGE_NO.NORMAL
                    Case Else
                        VALUE = ""
                        ERR = C_MESSAGE_NO.NORMAL
                End Select

            End If

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0007_UPROFVARI Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try


    End Sub

End Class

Imports System.Data.SqlClient

''' <summary>
''' 配送受注ＤＢ存在チェック
''' </summary>
''' <remarks></remarks>
Public Structure CS0037HORDERchk
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 出荷部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHIPORG() As String
    ''' <summary>
    ''' 受注番号
    ''' </summary>
    ''' <value>受注番号</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORDERNO() As String
    ''' <summary>
    ''' 受注明細
    ''' </summary>
    ''' <value>受注明細</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DETAILNO() As String
    ''' <summary>
    ''' トリップ番号
    ''' </summary>
    ''' <value>トリップ番号</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TRIPNO() As String
    ''' <summary>
    ''' ドロップ番号
    ''' </summary>
    ''' <value>ドロップ番号</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DROPNO() As String
    ''' <summary>
    ''' 出庫年月日
    ''' </summary>
    ''' <value>出庫年月日</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHUKODATE() As Date
    ''' <summary>
    ''' 業務車番
    ''' </summary>
    ''' <value>業務車番</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GSHABAN() As String
    ''' <summary>
    ''' 乗務員コード
    ''' </summary>
    ''' <value>社員コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFCODE() As String
    ''' <summary>
    ''' 乗務区分
    ''' </summary>
    ''' <value>乗務区分</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CREWKBN() As String
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
    Public Const METHOD_NAME As String = "CS0037HORDERchk"
    ''' <summary>
    ''' 配送受注ＤＢ存在チェック
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0037HORDERchk()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: SHIPORG
        If IsNothing(SHIPORG) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "SHIPORG"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End If

        'PARAM03: ORDERNO
        If IsNothing(ORDERNO) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ORDERNO"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End If

        'PARAM04: DETAILNO
        If IsNothing(DETAILNO) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DETAILNO"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End If

        'PARAM05: TRIPNO
        If IsNothing(TRIPNO) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TRIPNO"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End If

        'PARAM06: DROPNO
        If IsNothing(DROPNO) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DROPNO"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End If

        'PARAM07: SHUKODATE
        If IsNothing(SHUKODATE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "SHUKODATE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        Else
            If SHUKODATE < C_DEFAULT_YMD Then
                ERR = C_MESSAGE_NO.DLL_IF_ERROR

                Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "SHUKODATE"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
                Exit Sub
            Else
            End If
        End If

        'PARAM08: GSHABAN
        If IsNothing(GSHABAN) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "GSHABAN"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End If

        'PARAM09: STAFFCODE
        If IsNothing(STAFFCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "STAFFCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End If

        'PARAM10: CREWKBN
        If IsNothing(CREWKBN) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite              'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CREWKBN"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'T0004_HORDER検索SQL文
            Dim SQLStr As String = ""

            If CREWKBN = "1" Then
                SQLStr =
                           " SELECT CAMPCODE " _
                         & "     FROM COM.T0004_HORDER " _
                         & "     WHERE    CAMPCODE        = @P01 " _
                         & "       and    SHIPORG         = @P02 " _
                         & "       and    ORDERNO         = @P03 " _
                         & "       and    DETAILNO        = @P04 " _
                         & "       and    TRIPNO          = @P05 " _
                         & "       and    DROPNO          = @P06 " _
                         & "       and    SHUKODATE       = @P07 " _
                         & "       and    GSHABAN         = @P08 " _
                         & "       and    STAFFCODE       = @P09 " _
                         & "       and    DELFLG         <> '1' "
            Else
                SQLStr =
                           " SELECT CAMPCODE " _
                         & "     FROM COM.T0004_HORDER " _
                         & "     WHERE    CAMPCODE        = @P01 " _
                         & "       and    SHIPORG         = @P02 " _
                         & "       and    ORDERNO         = @P03 " _
                         & "       and    DETAILNO        = @P04 " _
                         & "       and    TRIPNO          = @P05 " _
                         & "       and    DROPNO          = @P06 " _
                         & "       and    SHUKODATE       = @P07 " _
                         & "       and    GSHABAN         = @P08 " _
                         & "       and    SUBSTAFFCODE    = @P09 " _
                         & "       and    DELFLG         <> '1' "
            End If

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)
            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)

            PARA1.Value = CAMPCODE
            PARA2.Value = SHIPORG
            PARA3.Value = ORDERNO
            PARA4.Value = DETAILNO
            PARA5.Value = TRIPNO
            PARA6.Value = DROPNO
            PARA7.Value = SHUKODATE
            PARA8.Value = GSHABAN
            PARA9.Value = STAFFCODE

            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            While SQLdr.Read
                ERR = C_MESSAGE_NO.NORMAL

            End While

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

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub
End Structure

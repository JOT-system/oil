Imports System.Data.SqlClient

''' <summary>
''' 権限チェック（組織 APSRVチェック有）
''' </summary>
''' <remarks></remarks>
Public Structure CS0012AUTHORorg

    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value>ユーザID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
    ''' <summary>
    ''' 端末ID
    ''' </summary>
    ''' <value>端末ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID As String
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' 組織
    ''' </summary>
    ''' <value>組織コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As String

    ''' <summary>
    ''' 有効日付(開始)
    ''' </summary>
    ''' <value>有効日付(開始)</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STYMD() As Date

    ''' <summary>
    ''' 有効日付(終了)
    ''' </summary>
    ''' <value>有効日付(終了)</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDYMD() As Date

    ''' <summary>
    ''' 権限結果
    ''' </summary>
    ''' <value>権限コード</value>
    ''' <returns>0：権限無　１：参照権限　２：参照更新権限</returns>
    ''' <remarks></remarks>
    Public Property PERMITCODE() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(Customize),ERR:00003(DBerr),ERR:10003(権限エラー)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0012AUTHORorg"

    ''' <summary>
    ''' 権限情報を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0012AUTHORorg()
        Dim sm As New CS0050SESSION
        '●In PARAMチェック
        'PARAM01: CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"                    '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                           '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                 'ログ出力
            Exit Sub
        End If

        ERR = C_MESSAGE_NO.DLL_IF_ERROR 'PARAM02: ORGCODE
        If IsNothing(ORGCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ORGCODE"                    '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                          '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM03: I_STYMD
        If IsNothing(STYMD) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "STYMD"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                              '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        Else
            If STYMD < C_DEFAULT_YMD Then
                ERR = C_MESSAGE_NO.DLL_IF_ERROR

                Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "STYMD"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                              '
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
                Exit Sub
            End If
        End If

        'PARAM04: I_ENDYMD
        If IsNothing(ENDYMD) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ENDYMD"                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                              '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        Else
            If ENDYMD < C_DEFAULT_YMD Then
                ERR = C_MESSAGE_NO.DLL_IF_ERROR

                Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "ENDYMD"                    '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                              '
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
                Exit Sub
            Else
            End If
        End If
        'PARAM EXTRA01 USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02 TERMID
        If IsNothing(TERMID) Then
            TERMID = sm.APSV_ID
        End If

        Dim WW_USER_PERMIT As String = " "
        Dim WW_SRV_PERMIT As String = " "
        PERMITCODE = ""
        ERR = C_MESSAGE_NO.AUTHORIZATION_ERROR

        '●権限情報取得　…　ユーザIDに対する組織権限を取得
        Try
            'DataBase接続文字
            Dim SQLcon As SqlConnection = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
                 "SELECT rtrim(B.PERMITCODE) as PERMITCODE  " _
               & " FROM  COM.OIS0004_USER              A          " _
               & " INNER JOIN COM.OIS0009_ROLE         B       ON " _
               & "       B.CAMPCODE = A.CAMPCODE            " _
               & "   and B.OBJECT   = @P3                   " _
               & "   and B.ROLE     = A.ORGROLE             " _
               & "   and B.CODE     = @P7 " _
               & "   and B.STYMD   <= @P4 " _
               & "   and B.ENDYMD  >= @P5 " _
               & "   and B.DELFLG  <> @P6 " _
               & " Where A.USERID   = @P1 " _
               & "   and A.CAMPCODE = @P2 " _
               & "   and A.STYMD   <= @P4 " _
               & "   and A.ENDYMD  >= @P5 " _
               & "   and A.DELFLG  <> @P6 " _
               & "ORDER BY B.SEQ "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
            PARA1.Value = USERID
            PARA2.Value = CAMPCODE
            PARA3.Value = C_ROLE_VARIANT.USER_ORG
            PARA4.Value = ENDYMD
            PARA5.Value = STYMD
            PARA6.Value = C_DELETE_FLG.DELETE
            PARA7.Value = ORGCODE

            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                WW_USER_PERMIT = SQLdr("PERMITCODE")
                ERR = C_MESSAGE_NO.NORMAL
                Exit While
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

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0010_AUTHOR Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '●権限情報取得　…　セッション情報(APサーバ)に対する組織権限を取得
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
                 "SELECT rtrim(B.PERMITCODE) as PERMITCODE " _
               & " FROM  COM.OIS0011_SRVAUTHOR A " _
               & " INNER JOIN COM.OIS0009_ROLE B " _
               & "   ON  B.CAMPCODE = A.CAMPCODE " _
               & "   and B.OBJECT   = A.OBJECT " _
               & "   and B.ROLE     = A.ROLE " _
               & "   and B.CODE     = @P7 " _
               & "   and B.STYMD   <= @P4 " _
               & "   and B.ENDYMD  >= @P5 " _
               & "   and B.DELFLG  <> @P6 " _
               & " Where A.CAMPCODE = @P1 " _
               & "   and A.TERMID   = @P2 " _
               & "   and A.OBJECT   = @P3 " _
               & "   and A.STYMD   <= @P4 " _
               & "   and A.ENDYMD  >= @P5 " _
               & "   and A.DELFLG  <> @P6 " _
               & "ORDER BY B.SEQ "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
            PARA1.Value = CAMPCODE
            PARA2.Value = TERMID
            PARA3.Value = C_ROLE_VARIANT.SERV_ORG
            PARA4.Value = ENDYMD
            PARA5.Value = STYMD
            PARA6.Value = C_DELETE_FLG.DELETE
            PARA7.Value = ORGCODE

            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                WW_SRV_PERMIT = SQLdr("PERMITCODE")
                ERR = C_MESSAGE_NO.NORMAL
                Exit While
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

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0011_SRVAUTHOR Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        If isNormal(ERR) Then
            PERMITCODE = C_PERMISSION.REFERLANCE
            Select Case (WW_SRV_PERMIT & WW_USER_PERMIT)
                Case "00"
                    PERMITCODE = C_PERMISSION.INVALID
                Case "01"
                    PERMITCODE = C_PERMISSION.INVALID
                Case "02"
                    PERMITCODE = C_PERMISSION.INVALID
                Case "10"
                    PERMITCODE = C_PERMISSION.INVALID
                Case "11"
                    PERMITCODE = C_PERMISSION.REFERLANCE
                Case "12"
                    PERMITCODE = C_PERMISSION.REFERLANCE
                Case "20"
                    PERMITCODE = C_PERMISSION.INVALID
                Case "21"
                    PERMITCODE = C_PERMISSION.REFERLANCE
                Case "22"
                    PERMITCODE = C_PERMISSION.UPDATE
            End Select
        End If
    End Sub

End Structure

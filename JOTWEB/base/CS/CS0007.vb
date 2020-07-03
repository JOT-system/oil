Option Strict On
Imports System.Data.SqlClient

''' <summary>
''' 更新権限チェック（画面 APSRVチェック有）
''' </summary>
''' <remarks></remarks>
Public Class CS0007CheckAuthority

    ''' <summary>
    ''' 権限チェックを行う画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' 権限チェックを行うメニュー表示のロール
    ''' </summary>
    ''' <value>ROLECODE(MENU)</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE_MENU As String

    ''' <summary>
    ''' 権限チェックを行う画面のロール
    ''' </summary>
    ''' <value>ROLECODE(MAP)</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE_MAP As String

    ''' <summary>
    ''' 権限チェックを行う画面表示項目のロール
    ''' </summary>
    ''' <value>ROLECODE(VIEWPROF)</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE_VIEWPROF As String

    ''' <summary>
    ''' 権限チェックを行うエクセル出力のロール
    ''' </summary>
    ''' <value>ROLECODE(RPRTPROF)</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE_RPRTPROF As String

    ''' <summary>
    ''' 権限チェックを行う会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property COMPCODE() As String

    ''' <summary>
    ''' 権限チェックを行う会社のロール
    ''' </summary>
    ''' <value>ROLECODE(MAP)</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE_COMP As String

    ''' <summary>
    ''' 権限チェックを行う端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>未設定時はセッションから取得する</remarks>
    Public Property TERMID As String
    ''' <summary>
    ''' 権限結果
    ''' </summary>
    ''' <value>権限コード</value>
    ''' <returns>0：権限無　１：参照権限　２：参照更新権限</returns>
    ''' <remarks></remarks>
    Public Property MAPPERMITCODE() As String

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
    Public Const METHOD_NAME As String = "CS0007CheckAuthority"

    ''' <summary>
    ''' 各画面の更新権限情報を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub check()
        Dim sm As CS0050SESSION = New CS0050SESSION()
        '●In PARAMチェック
        'PARAM01: MAPID
        If IsNothing(MAPID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If
        'PARAM 02: ROLECODE_MAP
        If IsNothing(ROLECODE_MAP) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ROLECODE_MAP"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM EXTRA 01: TERMID
        If IsNothing(TERMID) Then
            TERMID = sm.APSV_ID
        End If
        'PARAM EXTRA 02 ROLECODE_COMP
        If IsNothing(ROLECODE_COMP) Then
            ROLECODE_COMP = String.Empty
        End If
        Dim WW_USER_MAP_PERMIT As Integer = CInt(C_PERMISSION.INVALID)
        Dim WW_SRV_PERMIT As Integer = CInt(C_PERMISSION.INVALID)
        Dim WW_USER_COMP_PERMIT As Integer = CInt(C_PERMISSION.INVALID)
        '●権限チェック（画面）　…　ユーザ操作権限取得

        MAPPERMITCODE = C_PERMISSION.INVALID

        Try
            'DataBase接続文字
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)
                SqlConnection.ClearPool(SQLcon)
                '●権限チェック（画面）　…　ユーザ操作権限取得
                WW_USER_MAP_PERMIT = checkUserPermission(SQLcon, ROLECODE_MAP, C_ROLE_VARIANT.USER_PERTMIT, MAPID)
                ''●権限チェック（画面）　…　サーバ操作権限取得
                'WW_SRV_PERMIT = checkTermPermission(SQLcon, C_ROLE_VARIANT.SERV_PERTMIT)
                ''●権限チェック（会社）　…　ユーザ操作権限取得
                'If String.IsNullOrEmpty(ROLECODE_COMP) Then
                '    WW_USER_COMP_PERMIT = CInt(C_PERMISSION.UPDATE)
                'Else
                '    WW_USER_COMP_PERMIT = checkUserPermission(SQLcon, ROLECODE_COMP, C_ROLE_VARIANT.USER_COMP, COMPCODE)
                'End If
            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0009_ROLE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '権限コード判定
        If isNormal(ERR) Then
            ''一番小さい権限を採用する
            'MAPPERMITCODE = Math.Min(WW_SRV_PERMIT, Math.Min(WW_USER_MAP_PERMIT, WW_USER_COMP_PERMIT))
            MAPPERMITCODE = WW_USER_MAP_PERMIT.ToString
        End If

    End Sub
    ''' <summary>
    ''' ユーザ権限の権限コードを取得する
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="ROLECODE"></param>
    ''' <param name="OBJCODE"></param>
    ''' <param name="CODE" ></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function checkUserPermission(ByVal SQLcon As SqlConnection, ByVal ROLECODE As String, ByVal OBJCODE As String, ByVal CODE As String) As Integer
        Dim WW_PERMIT As Integer = CInt(C_PERMISSION.INVALID)
        '検索SQL文
        Try
            Dim SQLStr As String =
                 " SELECT " _
               & "              rtrim(A.PERMITCODE)    AS PERMITCODE   " _
               & " FROM        COM.OIS0009_ROLE                A             " _
               & " WHERE                                               " _
               & "           A.ROLE        = @P1                       " _
               & "       and A.OBJECT      = @P2                       " _
               & "       and A.CODE        = @P3                       " _
               & "       and A.STYMD      <= @P4                       " _
               & "       and A.ENDYMD     >= @P5                       " _
               & "       and A.DELFLG     <> @P6                       " _
               & " ORDER BY A.SEQ                                      "

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = ROLECODE
                    .Add("@P2", SqlDbType.NVarChar, 20).Value = OBJCODE
                    .Add("@P3", SqlDbType.NVarChar, 20).Value = CODE
                    .Add("@P4", SqlDbType.Date).Value = Date.Now
                    .Add("@P5", SqlDbType.Date).Value = Date.Now
                    .Add("@P6", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '権限コード初期値(権限なし)設定

                ERR = C_MESSAGE_NO.AUTHORIZATION_ERROR

                If SQLdr.Read Then
                    WW_PERMIT = CInt(SQLdr("PERMITCODE"))
                    ERR = C_MESSAGE_NO.NORMAL
                End If

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0009_ROLE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            checkUserPermission = Nothing
            Exit Function
        End Try
        checkUserPermission = WW_PERMIT
    End Function
    '''' <summary>
    '''' 端末権限の権限コードを取得する
    '''' </summary>
    '''' <param name="SQLcon"></param>
    '''' <param name="OBJCODE"></param>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Protected Function checkTermPermission(ByVal SQLcon As SqlConnection, ByVal OBJCODE As String) As Integer
    '    Dim WW_PERMIT As Integer = C_PERMISSION.INVALID
    '    '検索SQL文
    '    Try
    '        '検索SQL文
    '        Dim SQLStr As String =
    '             "SELECT rtrim(B.PERMITCODE) as PERMITCODE " _
    '           & " FROM  COM.OIS0011_SRVAUTHOR A " _
    '           & " INNER JOIN COM.OIS0009_ROLE B " _
    '           & "   ON  B.OBJECT   = A.OBJECT " _
    '           & "   and B.ROLE     = A.ROLE " _
    '           & "   and B.STYMD   <= @P4 " _
    '           & "   and B.ENDYMD  >= @P5 " _
    '           & "   and B.DELFLG  <> @P6 " _
    '           & " Where A.TERMID   = @P1 " _
    '           & "   and A.OBJECT   = @P2 " _
    '           & "   and B.CODE     = @P3 " _
    '           & "   and A.STYMD   <= @P4 " _
    '           & "   and A.ENDYMD  >= @P5 " _
    '           & "   and A.DELFLG  <> @P6 " _
    '           & "ORDER BY B.SEQ "

    '        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
    '            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 30)
    '            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
    '            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
    '            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 1)
    '            PARA1.Value = TERMID
    '            PARA2.Value = OBJCODE
    '            PARA3.Value = MAPID
    '            PARA4.Value = Date.Now
    '            PARA5.Value = Date.Now
    '            PARA6.Value = C_DELETE_FLG.DELETE
    '            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

    '            '権限コード初期値(権限なし)設定

    '            ERR = C_MESSAGE_NO.AUTHORIZATION_ERROR

    '            If SQLdr.Read Then
    '                WW_PERMIT = SQLdr("PERMITCODE")
    '                ERR = C_MESSAGE_NO.NORMAL
    '            End If

    '            'Close
    '            SQLdr.Close() 'Reader(Close)
    '            SQLdr = Nothing
    '        End Using

    '    Catch ex As Exception
    '        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

    '        CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "DB:OIS0011_SRVAUTHOR Select"
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWRITE.TEXT = ex.ToString()
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

    '        ERR = C_MESSAGE_NO.DB_ERROR
    '        checkTermPermission = WW_PERMIT
    '        Exit Function
    '    End Try
    '    checkTermPermission = WW_PERMIT
    'End Function
End Class

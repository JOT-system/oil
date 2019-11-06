Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' タイトル会社取得
''' </summary>
''' <remarks>CAMP権限によりDB(OIS0009_ROLE)とDB(OIS0011_SRVAUTHOR)を検索して両方許可のある会社コードを取得する。</remarks>
Public Class CS0015TITLEcamp

    ''' <summary>
    ''' 会社コード一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property List() As Object

    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value>ユーザID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

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
    Public Const METHOD_NAME As String = "CS0015TITLEcamp"

    ''' <summary>
    ''' タイトルに設定する会社の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0015TITLEcamp()

        '●In PARAMチェック
        'PARAM01: List
        If IsNothing(List) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "List"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        Dim sm As New CS0050SESSION

        'PARAM EXTRA01 USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        Dim W_OBJ As ListBox = List
        Dim W_OBJ_USER_CAMPCODE As New List(Of String)
        Dim W_OBJ_USER_NAMES As New List(Of String)
        Dim W_OBJ_USER_PERMIT As New List(Of String)

        Dim W_OBJ_SRV_CAMPCODE As New List(Of String)
        Dim W_OBJ_SRV_NAMES As New List(Of String)
        Dim W_OBJ_SRV_PERMIT As New List(Of String)

        Using SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)
            '●タイトル会社取得
            '○ User権限によりDB(OIS0009_ROLE)検索
            Try
                'DataBase接続文字


                '検索SQL文
                Dim SQLStr As String =
                     "SELECT rtrim(A.CAMPCODE) as CAMPCODE " _
                   & "     , rtrim(A.NAME) as NAME  " _
                   & "     , rtrim(MAX( B.PERMITCODE )) as PERMITCODE " _
                   & " FROM  OIL.OIM0001_CAMP A " _
                   & " INNER JOIN COM.OIS0009_ROLE B ON " _
                   & "       B.PERMITCODE >= 1 " _
                   & "   and B.DELFLG  <> @P5 " _
                   & " INNER JOIN COM.OIS0004_USER C  ON " _
                   & "       C.USERID   = @P1 " _
                   & "   and C.CAMPCODE = A.CAMPCODE " _
                   & "   and C.MENUROLE = B.ROLE " _
                   & "   and C.MAPID = B.CODE " _
                   & "   and C.STYMD   <= @P3 " _
                   & "   and C.ENDYMD  >= @P4 " _
                   & "   and C.DELFLG  <> @P5 " _
                   & " WHERE A.DELFLG  <> @P5 " _
                   & "GROUP BY A.CAMPCODE , A.NAME " _
                   & "ORDER BY A.CAMPCODE "

                '  "SELECT rtrim(A.CAMPCODE) as CAMPCODE " _
                '& "     , rtrim(A.NAMES) as NAMES  " _
                '& "     , rtrim(MAX( B.PERMITCODE )) as PERMITCODE " _
                '& " FROM  OIL.OIM0001_CAMP A " _
                '& " INNER JOIN COM.OIS0009_ROLE B ON " _
                '& "       B.CODE     = A.CAMPCODE " _
                '& "   and B.OBJECT   = @P2 " _
                '& "   and B.PERMITCODE >= 1 " _
                '& "   and B.STYMD   <= @P3 " _
                '& "   and B.ENDYMD  >= @P4 " _
                '& "   and B.DELFLG  <> @P5 " _
                '& " INNER JOIN COM.OIS0004_USER C  ON " _
                '& "       C.USERID   = @P1 " _
                '& "   and C.CAMPROLE = B.ROLE " _
                '& "   and C.STYMD   <= @P3 " _
                '& "   and C.ENDYMD  >= @P4 " _
                '& "   and C.DELFLG  <> @P5 " _
                '& " WHERE A.STYMD   <= @P3 " _
                '& "   and A.ENDYMD  >= @P4 " _
                '& "   and A.DELFLG  <> @P5 " _
                '& "GROUP BY A.CAMPCODE , A.NAMES " _
                '& "ORDER BY A.CAMPCODE "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                '                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = USERID
                '                PARA2.Value = C_ROLE_VARIANT.USER_COMP
                PARA3.Value = Date.Now
                PARA4.Value = Date.Now
                PARA5.Value = C_DELETE_FLG.DELETE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                Dim i As Integer = 0
                While SQLdr.Read
                    i = i + 1
                    W_OBJ_USER_CAMPCODE.Add(SQLdr("CAMPCODE"))
                    W_OBJ_USER_NAMES.Add(SQLdr("NAME"))
                    W_OBJ_USER_PERMIT.Add(SQLdr("PERMITCODE"))
                End While

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

            Catch ex As Exception
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:OIS0010_AUTHOR Select"           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            '○ 端末権限によりDB(OIS0011_SRVAUTHOR)検索
            'Try
            '    '検索SQL文
            '    Dim SQLStr As String =
            '         "SELECT rtrim(C.CAMPCODE) as CAMPCODE , rtrim(C.NAMES) as NAMES , rtrim(MAX( B.PERMITCODE )) as PERMITCODE " _
            '       & " FROM  COM.OIS0011_SRVAUTHOR A " _
            '       & " INNER JOIN COM.OIS0009_ROLE B " _
            '       & "   ON  B.CAMPCODE = A.CAMPCODE " _
            '       & "   and B.OBJECT   = @P2 " _
            '       & "   and B.ROLE     = A.ROLE " _
            '       & "   and B.PERMITCODE >= 1 " _
            '       & "   and B.STYMD   <= @P3 " _
            '       & "   and B.ENDYMD  >= @P4 " _
            '       & "   and B.DELFLG  <> @P5 " _
            '       & " INNER JOIN oil.OIM0001_CAMP C " _
            '       & "   ON  C.CAMPCODE = B.CODE " _
            '       & "   and C.STYMD   <= @P3 " _
            '       & "   and C.ENDYMD  >= @P4 " _
            '       & "   and C.DELFLG  <> @P5 " _
            '       & " Where A.TERMID   = @P1 " _
            '       & "   and A.OBJECT   = @P2 " _
            '       & "   and A.STYMD   <= @P3 " _
            '       & "   and A.ENDYMD  >= @P4 " _
            '       & "   and A.DELFLG  <> @P5 " _
            '       & "GROUP BY C.CAMPCODE , C.NAMES " _
            '       & "ORDER BY C.CAMPCODE "

            '    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            '    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            '    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            '    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            '    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            '    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
            '    PARA1.Value = sm.APSV_ID
            '    PARA2.Value = C_ROLE_VARIANT.SERV_COMP
            '    PARA3.Value = Date.Now
            '    PARA4.Value = Date.Now
            '    PARA5.Value = C_DELETE_FLG.DELETE
            '    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '    Dim i As Integer = 0
            '    While SQLdr.Read
            '        i = i + 1
            '        W_OBJ_SRV_CAMPCODE.Add(SQLdr("CAMPCODE"))
            '        W_OBJ_SRV_NAMES.Add(SQLdr("NAMES"))
            '        W_OBJ_SRV_PERMIT.Add(SQLdr("PERMITCODE"))

            '    End While

            '    'Close
            '    SQLdr.Close() 'Reader(Close)
            '    SQLdr = Nothing

            '    SQLcmd.Dispose()
            '    SQLcmd = Nothing

            'Catch ex As Exception
            '    Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            '    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            '    CS0011LOGWRITE.INFPOSI = "DB:OIS0011_SRVAUTHOR Select"        '
            '    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            '    CS0011LOGWRITE.TEXT = ex.ToString()
            '    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            '    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            '    Exit Sub
            'End Try
        End Using
        '○出力編集(I_List)
        For i As Integer = 0 To W_OBJ_USER_CAMPCODE.Count - 1
            'For j As Integer = 0 To W_OBJ_SRV_CAMPCODE.Count - 1
            '    If W_OBJ_USER_CAMPCODE(i) = W_OBJ_SRV_CAMPCODE(j) Then
            W_OBJ.Items.Add(New ListItem(W_OBJ_USER_NAMES(i), W_OBJ_USER_CAMPCODE(i)))
            '    End If
            'Next j
        Next i

        'デフォルト選択位置設定
        For i As Integer = 0 To W_OBJ.Items.Count - 1
            If W_OBJ.Items(i).Value = CAMPCODE OrElse i = 0 Then
                W_OBJ.SelectedIndex = i
            End If
        Next

        List = W_OBJ
        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class

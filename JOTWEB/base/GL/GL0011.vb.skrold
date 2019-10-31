Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 端末情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0011TermList
    Inherits GL0000

    ''' <summary>
    ''' 取得条件
    ''' </summary>
    Public Enum LC_TERM_TYPE
        ''' <summary>
        ''' 全取得
        ''' </summary>
        ALL
        ''' <summary>
        ''' 端末指定
        ''' </summary>
        TERMINAL
        ''' <summary>
        ''' 端末種別指定
        ''' </summary>
        SELECT_CLASS
    End Enum
    ''' <summary>
    ''' 取得条件
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TYPEMODE As String
    ''' <summary>
    ''' ROLECODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE() As String
    ''' <summary>
    ''' 端末種別
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLASSCODE() As String
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "getList"


    ''' <summary>
    ''' 会社情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理

        'PARAM EXTRA02: STYMD
        If STYMD < C_DEFAULT_YMD Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
            ENDYMD = Date.Now
        End If

        Try
            If IsNothing(LIST) Then
                LIST = New ListBox
            Else
                LIST.Items.Clear()
            End If
        Catch ex As Exception
        End Try

        'DataBase接続文字
        Using SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)
            Select Case TYPEMODE
                Case LC_TERM_TYPE.SELECT_CLASS
                    getTermClassList(SQLcon)
                Case LC_TERM_TYPE.TERMINAL
                    getTermList(SQLcon)
                Case LC_TERM_TYPE.ALL
                    getAllList(SQLcon)
            End Select

        End Using
    End Sub
    ''' <summary>
    ''' 端末一覧取得
    ''' </summary>
    Protected Sub getTermList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用会社取得
        '○ User権限によりDB(OIS0010_AUTHOR)検索
        Try

            Dim SQLStr As String =
                    " SELECT TERMID, TERMNAME " &
                    " FROM COM.OIS0001_TERM " &
                    " WHERE TERMCLASS     =  '1' " &
                    " AND   STYMD        <= getdate() " &
                    " AND   ENDYMD       >= getdate() " &
                    " AND   DELFLG       <> '1' "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY TERMID, TERMNAME "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY TERMNAME, TERMID "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY TERMID, TERMNAME "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr, "TERMID", "TERMNAME")

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0011"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0001_TERM Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub
    ''' <summary>
    ''' 端末種別別一覧取得
    ''' </summary>
    Protected Sub getTermClassList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用端末一覧取得
        '○ User権限によりDB(OIS0010_AUTHOR)検索
        Try

            Dim SQLStr As String =
                    " SELECT TERMID, TERMNAME   " &
                    " FROM COM.OIS0001_TERM           " &
                    " WHERE                     " &
                    "       STYMD        <= @P2 " &
                    " AND   ENDYMD       >= @P1 " &
                    " AND   TERMCLASS   　= @P3 " &
                    " AND   DELFLG       <> '1' "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY TERMID, TERMNAME "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY TERMNAME, TERMID "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY TERMID, TERMNAME "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 1)

                PARA1.Value = STYMD
                PARA2.Value = ENDYMD
                PARA3.Value = CLASSCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr, "TERMID", "TERMNAME")

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0011"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0001_TERM Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub
    ''' <summary>
    ''' 端末一覧取得
    ''' </summary>
    Protected Sub getAllList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用端末一覧取得
        '○ User権限によりDB(OIS0010_AUTHOR)検索
        Try

            Dim SQLStr As String =
                    " SELECT TERMID, TERMNAME   " &
                    " FROM COM.OIS0001_TERM           " &
                    " WHERE                     " &
                    "       STYMD        <= @P2 " &
                    " AND   ENDYMD       >= @P1 " &
                    " AND   DELFLG       <> '1' "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY TERMID, TERMNAME "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY TERMNAME, TERMID "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY TERMID, TERMNAME "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)

                PARA1.Value = STYMD
                PARA2.Value = ENDYMD
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr, "TERMID", "TERMNAME")

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0011"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0001_TERM Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub
End Class


Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' MAPID/URL情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0013URLList
    Inherits GL0000

    Public Enum LC_URL_TYPE
        URL
        MAPID
    End Enum
    ''' <summary>
    ''' TYPECODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TYPECODE() As String

    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "getList"


    ''' <summary>
    ''' MAPID/URL情報取得
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
            Select Case TYPECODE
                Case LC_URL_TYPE.URL
                    getURLList(SQLcon)
                Case LC_URL_TYPE.MAPID
                    getMapList(SQLcon)
                Case Else
            End Select

        End Using
    End Sub
    ''' <summary>
    ''' URL一覧取得
    ''' </summary>
    Protected Sub getURLList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用MAPID取得
        '○ User権限によりDB(S0009_URL)検索
        Try

            Dim SQLStr As String =
                    " SELECT rtrim(URL)             as CODE  , " &
                    "        rtrim(NAMES)           as NAMES   " &
                    " FROM COM.S0009_URL " &
                    " WHERE   STYMD        <= @P2 " &
                    "   AND   ENDYMD       >= @P3 " &
                    "   AND   DELFLG       <> '1' "

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY URL, NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY NAMES, URL "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY URL, NAMES "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)

                PARA2.Value = STYMD
                PARA3.Value = ENDYMD

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0012"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0009_URL Select"
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
    ''' MAPID一覧取得
    ''' </summary>
    Protected Sub getMapList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用MAPID取得
        '○ User権限によりDB(S0009_URL)検索
        Try

            Dim SQLStr As String =
                    " SELECT rtrim(MAPID)           as CODE  , " &
                    "        rtrim(NAMES)           as NAMES   " &
                    " FROM COM.S0009_URL " &
                    " WHERE   STYMD        <= @P2 " &
                    "   AND   ENDYMD       >= @P3 " &
                    "   AND   DELFLG       <> '1' "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY MAPID, NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY NAMES, MAPID "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY MAPID, NAMES "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)

                PARA2.Value = STYMD
                PARA3.Value = ENDYMD

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0012"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0009_URL Select"
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


Imports System.Data.SqlClient

''' <summary>
''' 遷移先URL取得
''' </summary>
''' <remarks>遷移するのURIを取得する</remarks>
Public Structure CS0017ForwardURL

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' 変数
    ''' </summary>
    ''' <value>変数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VARI() As String

    ''' <summary>
    ''' 遷移先URL
    ''' </summary>
    ''' <value>URL</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URL() As String

    ''' <summary>
    ''' 遷移先ID
    ''' </summary>
    ''' <value>画面戻先変数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAP_RETURN() As String

    ''' <summary>
    ''' 遷移先変数
    ''' </summary>
    ''' <value>画面戻先変数</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VARI_RETURN() As String

    ''' <summary>
    ''' ボタン名称
    ''' </summary>
    ''' <value>ボタン名称</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAMES() As String

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
    Public Const METHOD_NAME As String = "CS0017RETURNURLget"

    ''' <summary>
    ''' URL取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getPreviusURL()

        '●In PARAMチェック
        'PARAM01: MAPID
        If IsNothing(MAPID) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: VARI …任意項目
        'PARAM03: CAMPCODE …任意項目（本来は任意ではない）

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●画面戻先URL取得
        '○ DB(S0024_PROFMMAP-S0009_URL)検索

        Try
            'DataBase接続文字
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                     " SELECT " _
                   & "      rtrim(A.MAPIDP)   as MAPIDP   , " _
                   & "      rtrim(A.VARIANTP) as VARIANTP , " _
                   & "      rtrim(A.MAPNAMES) as NAMES    , " _
                   & "      rtrim(B.URL)      as URL        " _
                   & " FROM  com.OIS0008_PROFMMAP A " _
                   & " INNER JOIN com.OIS0009_URL B " _
                   & "   ON  B.MAPID     = A.MAPIDP " _
                   & "   and B.STYMD    <= @P4 " _
                   & "   and B.ENDYMD   >= @P3 " _
                   & "   and B.DELFLG   <> @P5 " _
                   & " Where " _
                   & "       A.MAPID     = @P1 " _
                   & "   and A.VARIANT   = @P2 " _
                   & "   and A.TITLEKBN  = 'I' " _
                   & "   and A.STYMD    <= @P4 " _
                   & "   and A.ENDYMD   >= @P3 " _
                   & "   and A.DELFLG   <> @P5 "
                If Not String.IsNullOrEmpty(CAMPCODE) Then
                    SQLStr += "   and A.CAMPCODE   = @P6 "
                End If
                SQLStr += "ORDER BY A.POSIROW "
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                    PARA1.Value = MAPID
                    PARA2.Value = VARI
                    PARA3.Value = Date.Now
                    PARA4.Value = Date.Now
                    PARA5.Value = C_DELETE_FLG.DELETE
                    If Not String.IsNullOrEmpty(CAMPCODE) Then
                        Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 50)
                        PARA6.Value = CAMPCODE
                    End If
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    ERR = C_MESSAGE_NO.DLL_IF_ERROR
                    If SQLdr.Read Then
                        ERR = C_MESSAGE_NO.NORMAL
                        URL = SQLdr("URL")
                        VARI_RETURN = SQLdr("VARIANTP")
                        MAP_RETURN = SQLdr("MAPIDP")
                        NAMES = SQLdr("NAMES")
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME
            CS0011LOGWRITE.INFPOSI = "S0024_PROFMMAP SELECT (" & MAPID & " " & VARI & ")"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' URL取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getNextURL()
        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●In PARAMチェック
        'PARAM01: MAPID
        If IsNothing(MAPID) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPIDP"                         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: VARIP …任意項目
        '●変数情報取得
        '○ DB(S0024_PROFMMAP-S0009_URL)検索

        'PARAM03: CAMPCODE …任意項目（本来は任意ではない）

        Try
            'DataBase接続文字
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                 "SELECT " _
               & "     rtrim(B.URL)      as URL    , " _
               & "     rtrim(A.MAPNAMES) as NAMES  , " _
               & "     rtrim(A.MAPID)    as MAPID  , " _
               & "     rtrim(A.VARIANT)  as VARIANT  " _
               & " FROM  com.OIS0008_PROFMMAP A " _
               & " LEFT JOIN com.OIS0009_URL B " _
               & "   ON  B.MAPID    = A.MAPID " _
               & "   and B.STYMD   <= @P4 " _
               & "   and B.ENDYMD  >= @P3 " _
               & "   and B.DELFLG  <> @P5 " _
               & " Where " _
               & "       A.MAPIDP   = @P1 " _
               & "   and A.VARIANTP = @P2 " _
               & "   and A.TITLEKBN = 'I' " _
               & "   and A.STYMD   <= @P4 " _
               & "   and A.ENDYMD  >= @P3 " _
               & "   and A.DELFLG   <> @P5 "
                If Not String.IsNullOrEmpty(CAMPCODE) Then
                    SQLStr += "   and A.CAMPCODE   = @P6 "
                End If
                SQLStr += "ORDER BY A.POSIROW "
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                    PARA1.Value = MAPID
                    PARA2.Value = VARI
                    PARA3.Value = Date.Now
                    PARA4.Value = Date.Now
                    PARA5.Value = C_DELETE_FLG.DELETE
                    If Not String.IsNullOrEmpty(CAMPCODE) Then
                        Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 50)
                        PARA6.Value = CAMPCODE
                    End If

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    ERR = C_MESSAGE_NO.DLL_IF_ERROR
                    If SQLdr.Read Then
                        ERR = C_MESSAGE_NO.NORMAL
                        URL = SQLdr("URL")
                        NAMES = SQLdr("NAMES")
                        MAP_RETURN = SQLdr("MAPID")
                        VARI_RETURN = SQLdr("VARIANT")
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using
                SQLcon.Close() 'DataBase接続(Close)
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME
            CS0011LOGWRITE.INFPOSI = "S0024_PROFMMAP SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub
End Structure

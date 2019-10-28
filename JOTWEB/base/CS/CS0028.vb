Imports System.Data.SqlClient

''' <summary>
''' 構造取得（帳票用）
''' </summary>
''' <remarks>ユーザとかがDEFAULT見てるので検討が必要</remarks>
Public Structure CS0028STRUCT

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value>ユーザID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String
    ''' <summary>
    ''' 構造コード
    ''' </summary>
    ''' <value>構造コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STRUCT() As String
    ''' <summary>
    ''' CODE
    ''' </summary>
    ''' <value></value>
    ''' <returns>CODE</returns>
    ''' <remarks></remarks>
    Public Property CODE() As List(Of String)
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
    Public Const METHOD_NAME As String = "CS0028STRUCT"

    ''' <summary>
    ''' 構造モデルの取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0028STRUCT()

        '●In PARAMチェック
        'PARAM01: CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"                       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: STRUCT
        If IsNothing(STRUCT) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "STRUCT"                         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        'PARAM EXTRA01:USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If

        '●構造（帳票）取得
        '○ 帳票IDよりDB(M0006_STRUCT)検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
                 "SELECT SEQ , rtrim(CODE) as CODE " _
               & " FROM  oil.M0006_STRUCT " _
               & " Where ((USERID    = @P1) or (USERID    = 'Default')) " _
               & "   and ((CAMPCODE  = @P2) or (CAMPCODE  = 'Default')) " _
               & "   and OBJECT      = 'REPORT' " _
               & "   and STRUCT      = @P3 " _
               & "   and STYMD      <= @P4 " _
               & "   and ENDYMD     >= @P4 " _
               & "   and DELFLG     <> '1' " _
               & " ORDER BY SEQ "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            PARA1.Value = USERID
            PARA2.Value = CAMPCODE
            PARA3.Value = STRUCT
            PARA4.Value = Date.Now
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            CODE = New List(Of String)
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            While SQLdr.Read
                CODE.Add(SQLdr("CODE"))

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
            ERR = C_MESSAGE_NO.DB_ERROR
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:M0006_STRUCT Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

End Structure


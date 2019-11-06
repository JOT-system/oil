Imports System.Data.SqlClient

''' <summary>
''' グループ指定コード取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0027GROUPget
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
    ''' オブジェクトコード
    ''' </summary>
    ''' <value>オブジェクト</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OBJCODE() As String
    ''' <summary>
    ''' 指定グループ１
    ''' </summary>
    ''' <value>グループコード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GROUP1() As String
    ''' <summary>
    ''' 指定グループ２
    ''' </summary>
    ''' <value>グループコード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GROUP2() As String
    ''' <summary>
    ''' 指定グループ３
    ''' </summary>
    ''' <value>グループコード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GROUP3() As String
    ''' <summary>
    ''' 指定グループ４
    ''' </summary>
    ''' <value>グループコード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GROUP4() As String
    ''' <summary>
    ''' 指定グループ５
    ''' </summary>
    ''' <value>グループコード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GROUP5() As String
    ''' <summary>
    ''' コード一覧
    ''' </summary>
    ''' <value>取得コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CODE() As List(Of String)

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
    Public Const METHOD_NAME As String = "CS0027GROUPget"

    ''' <summary>
    ''' グループ内容取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0027GROUPget()
        '<< エラー説明 >>
        'O_ERR = 

        '●In PARAMチェック
        'PARAM01: USERID
        If IsNothing(USERID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "USERID"                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM02: CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"               '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM03: OBJCODE
        If IsNothing(OBJCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "OBJCODE"                '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●グループ指定コード取得

        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
                 "SELECT rtrim(CODE) as CODE , rtrim(GRCODE01) as GRCODE01 , rtrim(GRCODE02) as GRCODE02 , rtrim(GRCODE03) as GRCODE03 , rtrim(GRCODE04) as GRCODE04 , rtrim(GRCODE05) as GRCODE05 , rtrim(GRCODE06) as GRCODE06 , rtrim(GRCODE07) as GRCODE07 , rtrim(GRCODE08) as GRCODE08 , rtrim(GRCODE09) as GRCODE09 , rtrim(GRCODE10) as GRCODE10 " _
               & " FROM  oil.M0006_STRUCT " _
               & " Where USERID   = @P1 " _
               & "   and CAMPCODE = @P2 " _
               & "   and OBJECT   = @P3 " _
               & "   and STYMD   <= @P4 " _
               & "   and ENDYMD  >= @P4 " _
               & "   and DELFLG  <> '1' " _
               & "ORDER BY SEQ "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            PARA1.Value = USERID
            PARA2.Value = CAMPCODE
            PARA3.Value = OBJCODE
            PARA4.Value = Date.Now
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            ERR = C_MESSAGE_NO.AUTHORIZATION_ERROR
            CODE = New List(Of String)

            While SQLdr.Read

                Dim WW_SW As String = ""

                If GROUP1 <> "" Then
                    If SQLdr("GRCODE01") = GROUP1 OrElse
                       SQLdr("GRCODE02") = GROUP1 OrElse
                       SQLdr("GRCODE03") = GROUP1 OrElse
                       SQLdr("GRCODE04") = GROUP1 OrElse
                       SQLdr("GRCODE05") = GROUP1 OrElse
                       SQLdr("GRCODE06") = GROUP1 OrElse
                       SQLdr("GRCODE07") = GROUP1 OrElse
                       SQLdr("GRCODE08") = GROUP1 OrElse
                       SQLdr("GRCODE09") = GROUP1 OrElse
                       SQLdr("GRCODE10") = GROUP1 Then
                        WW_SW = "ON"
                    End If
                End If

                If GROUP2 <> "" Then
                    If SQLdr("GRCODE01") = GROUP2 OrElse
                       SQLdr("GRCODE02") = GROUP2 OrElse
                       SQLdr("GRCODE03") = GROUP2 OrElse
                       SQLdr("GRCODE04") = GROUP2 OrElse
                       SQLdr("GRCODE05") = GROUP2 OrElse
                       SQLdr("GRCODE06") = GROUP2 OrElse
                       SQLdr("GRCODE07") = GROUP2 OrElse
                       SQLdr("GRCODE08") = GROUP2 OrElse
                       SQLdr("GRCODE09") = GROUP2 OrElse
                       SQLdr("GRCODE10") = GROUP2 Then
                        WW_SW = "ON"
                    End If
                End If

                If GROUP3 <> "" Then
                    If SQLdr("GRCODE01") = GROUP3 OrElse
                       SQLdr("GRCODE02") = GROUP3 OrElse
                       SQLdr("GRCODE03") = GROUP3 OrElse
                       SQLdr("GRCODE04") = GROUP3 OrElse
                       SQLdr("GRCODE05") = GROUP3 OrElse
                       SQLdr("GRCODE06") = GROUP3 OrElse
                       SQLdr("GRCODE07") = GROUP3 OrElse
                       SQLdr("GRCODE08") = GROUP3 OrElse
                       SQLdr("GRCODE09") = GROUP3 OrElse
                       SQLdr("GRCODE10") = GROUP3 Then
                        WW_SW = "ON"
                    End If
                End If

                If GROUP4 <> "" Then
                    If SQLdr("GRCODE01") = GROUP4 OrElse
                       SQLdr("GRCODE02") = GROUP4 OrElse
                       SQLdr("GRCODE03") = GROUP4 OrElse
                       SQLdr("GRCODE04") = GROUP4 OrElse
                       SQLdr("GRCODE05") = GROUP4 OrElse
                       SQLdr("GRCODE06") = GROUP4 OrElse
                       SQLdr("GRCODE07") = GROUP4 OrElse
                       SQLdr("GRCODE08") = GROUP4 OrElse
                       SQLdr("GRCODE09") = GROUP4 OrElse
                       SQLdr("GRCODE10") = GROUP4 Then
                        WW_SW = "ON"
                    End If
                End If

                If GROUP5 <> "" Then
                    If SQLdr("GRCODE01") = GROUP5 OrElse
                       SQLdr("GRCODE02") = GROUP5 OrElse
                       SQLdr("GRCODE03") = GROUP5 OrElse
                       SQLdr("GRCODE04") = GROUP5 OrElse
                       SQLdr("GRCODE05") = GROUP5 OrElse
                       SQLdr("GRCODE06") = GROUP5 OrElse
                       SQLdr("GRCODE07") = GROUP5 OrElse
                       SQLdr("GRCODE08") = GROUP5 OrElse
                       SQLdr("GRCODE09") = GROUP5 OrElse
                       SQLdr("GRCODE10") = GROUP5 Then
                        WW_SW = "ON"
                    End If
                End If

                If WW_SW = "ON" Then
                    CODE.Add(SQLdr("CODE"))
                End If

            End While

            ERR = C_MESSAGE_NO.NORMAL

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
            CS0011LOGWRITE.INFPOSI = "DB:M0006_STRUCT Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Structure

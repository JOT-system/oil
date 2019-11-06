Imports System.Data.SqlClient

''' <summary>
''' 乗務員取得
''' </summary>
''' <remarks>SQLに会社コードを追加するか検討</remarks>
Public Structure CS0043STAFFORGget
    ''' <summary>
    ''' 乗務員保管テーブル
    ''' </summary>
    ''' <value>テーブルデータ</value>
    ''' <returns>テーブルデータ</returns>
    ''' <remarks></remarks>
    Public Property TBL() As DataTable
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns>会社コード</returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 乗務員コード
    ''' </summary>
    ''' <value>社員コード</value>
    ''' <returns>社員コード</returns>
    ''' <remarks></remarks>
    Public Property STAFFCODE() As String
    ''' <summary>
    ''' 作業部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property SORG() As String
    ''' <summary>
    ''' 適用開始年月日
    ''' </summary>
    ''' <value>開始年月日</value>
    ''' <returns>開始年月日</returns>
    ''' <remarks></remarks>
    Public Property STYMD() As Date
    ''' <summary>
    ''' 適用終了年月日
    ''' </summary>
    ''' <value>終了年月日</value>
    ''' <returns>終了年月日</returns>
    ''' <remarks></remarks>
    Public Property ENDYMD() As Date
    ''' <summary>
    ''' 職務区分
    ''' </summary>
    ''' <value>職務区分</value>
    ''' <returns>職務区分</returns>
    ''' <remarks></remarks>
    Public Property STAFFKBN() As String
    ''' <summary>
    ''' 管理組織コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MORG() As String
    ''' <summary>
    ''' 配属組織コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property HORG() As String
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
    Public Const METHOD_NAME As String = "CS0043STAFFORGget"
    ''' <summary>
    ''' 乗務員情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0043STAFFORGget()

        '●In PARAMチェック
        'PARAM01: TBL
        If IsNothing(TBL) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TBL"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        If IsNothing(CAMPCODE) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: STAFFCODE
        If IsNothing(STAFFCODE) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "SHARYOTYPE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM03: SORG
        If IsNothing(SORG) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "SORG"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Exit Sub
        End If

        'PARAM04: STYMD
        If STYMD < C_DEFAULT_YMD Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "STYMD"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Exit Sub
        End If

        'PARAM05: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ENDYMD"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Exit Sub
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        Try
            If TBL.Columns.Count = 0 Then
                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String = ""
                SQLStr = _
                         "SELECT isnull(rtrim(A.STAFFKBN),'') as STAFFKBN " _
                       & "      ,isnull(rtrim(A.MORG),'') as MORG " _
                       & "      ,isnull(rtrim(A.HORG),'') as HORG " _
                       & "      ,isnull(rtrim(A.CAMPCODE),'') as CAMPCODE " _
                       & "      ,isnull(rtrim(A.STAFFCODE),'') as STAFFCODE " _
                       & "      ,isnull(rtrim(A.STYMD),'') as STYMD " _
                       & "      ,isnull(rtrim(A.ENDYMD),'') as ENDYMD " _
                       & " FROM  MB002_STAFFORG O " _
                       & " INNER JOIN MB001_STAFF A " _
                       & "   ON    A.CAMPCODE   = O.CAMPCODE   " _
                       & "   and   A.STAFFCODE  = O.STAFFCODE  " _
                       & "   and   A.CAMPCODE   = @P1 " _
                       & "   and   A.DELFLG    <> '1' " _
                       & " Where   O.SORG       = @P2 " _
                       & "   and   O.DELFLG    <> '1' "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = SORG

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                'SELECT結果をテンポラリに保存
                TBL.Load(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
                SQLcon.Dispose()
                SQLcon = Nothing

            End If

            STAFFKBN = ""
            MORG = ""
            HORG = ""

            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            For Each TBLrow As DataRow In TBL.Rows
                If TBLrow("CAMPCODE") = CAMPCODE And
                   TBLrow("STAFFCODE") = STAFFCODE And
                   TBLrow("STYMD") <= ENDYMD And
                   TBLrow("ENDYMD") >= STYMD Then
                    STAFFKBN = TBLrow("STAFFKBN")
                    MORG = TBLrow("MORG")
                    HORG = TBLrow("HORG")
                    ERR = C_MESSAGE_NO.NORMAL
                    Exit For
                End If
            Next

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Structure

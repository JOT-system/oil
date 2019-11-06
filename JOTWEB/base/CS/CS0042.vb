Imports System.Data.SqlClient

''' <summary>
''' 車両部署取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0042SHARYOORGget
    ''' <summary>
    ''' 車両部署保管テーブル
    ''' </summary>
    ''' <value>テーブルデータ</value>
    ''' <returns>テーブルデータ</returns>
    ''' <remarks></remarks>
    Public Property TBL() As DataTable
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 統一車番（上）
    ''' </summary>
    ''' <value>車両タイプ</value>
    ''' <returns>車両タイプ</returns>
    ''' <remarks></remarks>
    Public Property SHARYOTYPE() As String
    ''' <summary>
    ''' 統一車番（下）
    ''' </summary>
    ''' <value>統一車番</value>
    ''' <returns>統一車番</returns>
    ''' <remarks></remarks>
    Public Property TSHABAN() As String
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
    ''' 車両管理部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MANGMORG() As String
    ''' <summary>
    ''' 車両設置部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MANGSORG() As String
    ''' <summary>
    ''' 車両運用部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MANGUORG() As String
    ''' <summary>
    ''' 車両所有
    ''' </summary>
    ''' <value>車両所有</value>
    ''' <returns>車両所有</returns>
    ''' <remarks></remarks>
    Public Property BASELEASE() As String
    ''' <summary>
    ''' 車両登録油種コード
    ''' </summary>
    ''' <value>油種コード</value>
    ''' <returns>油種コード</returns>
    ''' <remarks></remarks>
    Public Property MANGOILTYPE() As String
    ''' <summary>
    ''' 庸車会社コード
    ''' </summary>
    ''' <value>取引先コード</value>
    ''' <returns>取引先コード</returns>
    ''' <remarks></remarks>
    Public Property MANGSUPPL() As String
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
    Public Const METHOD_NAME As String = "CS0042SHARYOORGget"
    ''' <summary>
    ''' 車両部署情報取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0042SHARYOORGget()

        '●In PARAMチェック
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

        'PARAM01: CAMPCODE
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

        'PARAM02: SHARYOTYPE
        If IsNothing(SHARYOTYPE) Then
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

        'PARAM03: TSHABAN
        If IsNothing(TSHABAN) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TSHABAN"
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
        '●車両部署マスタ取得
        Try
            If TBL.Columns.Count = 0 Then
                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                     "SELECT " _
                   & "       isnull(rtrim(B.CAMPCODE),'') as CAMPCODE " _
                   & "     , isnull(rtrim(B.SHARYOTYPE),'') as SHARYOTYPE " _
                   & "     , isnull(rtrim(B.TSHABAN),'') as TSHABAN " _
                   & "     , isnull(rtrim(B.STYMD),'') as STYMD " _
                   & "     , isnull(rtrim(B.ENDYMD),'') as ENDYMD " _
                   & "     , isnull(rtrim(A.MANGMORG),'') as MANGMORG " _
                   & "     , isnull(rtrim(A.MANGSORG),'') as MANGSORG " _
                   & "     , isnull(rtrim(O.MANGUORG),'') as MANGUORG " _
                   & "     , isnull(rtrim(A.BASELEASE),'') as BASELEASE " _
                   & "     , isnull(rtrim(A.MANGOILTYPE),'') as MANGOILTYPE " _
                   & "     , isnull(rtrim(A.MANGSUPPL),'') as MANGSUPPL " _
                   & " FROM  OIL.MA003_SHARYOB B " _
                   & " INNER JOIN OIL.MA002_SHARYOA A " _
                   & "   ON    A.CAMPCODE        = B.CAMPCODE                                                                " _
                   & "   and   A.SHARYOTYPE      = B.SHARYOTYPE                                                              " _
                   & "   and   A.TSHABAN         = B.TSHABAN                                                                 " _
                   & "   and   A.STYMD          <= B.ENDYMD                                                                  " _
                   & "   and   A.ENDYMD         >= B.STYMD                                                                   " _
                   & "   and   A.DELFLG         <> '1'                                                                       " _
                   & " LEFT  JOIN OIL.MA006_SHABANORG O                                                                          " _
                   & "   ON    O.CAMPCODE        = B.CAMPCODE                                                                " _
                   & "   and ((O.SHARYOTYPEF     = B.SHARYOTYPE and O.TSHABANF        = B.TSHABAN) or                        " _
                   & "        (O.SHARYOTYPEB     = B.SHARYOTYPE and O.TSHABANB        = B.TSHABAN) or                        " _
                   & "        (O.SHARYOTYPEB2    = B.SHARYOTYPE and O.TSHABANB2       = B.TSHABAN))                          " _
                   & "   and   O.DELFLG         <> '1'                                                                       " _
                   & " Where B.CAMPCODE    = @P1 " _
                   & "   and B.DELFLG     <> '1' "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE

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

            MANGMORG = ""
            MANGSORG = ""
            MANGUORG = ""
            BASELEASE = ""
            MANGOILTYPE = ""
            MANGSUPPL = ""
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            For Each TBLrow As DataRow In TBL.Rows
                If TBLrow("CAMPCODE") = CAMPCODE And
                   TBLrow("SHARYOTYPE") = SHARYOTYPE And
                   TBLrow("TSHABAN") = TSHABAN And
                   TBLrow("STYMD") <= ENDYMD And
                   TBLrow("ENDYMD") >= STYMD Then
                    MANGMORG = TBLrow("MANGMORG")
                    MANGSORG = TBLrow("MANGSORG")
                    MANGUORG = TBLrow("MANGUORG")
                    BASELEASE = TBLrow("BASELEASE")
                    MANGOILTYPE = TBLrow("MANGOILTYPE")
                    MANGSUPPL = TBLrow("MANGSUPPL")

                    ERR = C_MESSAGE_NO.NORMAL
                    Exit For
                End If
            Next

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA003_SHARYOB Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Structure

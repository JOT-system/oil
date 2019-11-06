Imports System.Data.SqlClient

''' <summary>
''' 車番部署取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0045GSHABANORGget
    ''' <summary>
    ''' 車番部署保管テーブル
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
    ''' 運用部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property UORG() As String
    ''' <summary>
    ''' 業務車番
    ''' </summary>
    ''' <value>業務車番</value>
    ''' <returns>業務車番</returns>
    ''' <remarks></remarks>
    Public Property GSHABAN() As String
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
    ''' 統一車番 前（上）
    ''' </summary>
    ''' <value>車両タイプ</value>
    ''' <returns>車両タイプ</returns>
    ''' <remarks></remarks>
    Public Property SHARYOTYPEF() As String
    ''' <summary>
    ''' 統一車番 前（下）
    ''' </summary>
    ''' <value>統一車番</value>
    ''' <returns>統一車番</returns>
    ''' <remarks></remarks>
    Public Property TSHABANF() As String
    ''' <summary>
    ''' 車両管理部署コード 前
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MANGMORGF() As String
    ''' <summary>
    ''' 車両設置部署コード 前
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MANGSORGF() As String
    ''' <summary>
    ''' 車両所有 前
    ''' </summary>
    ''' <value>車両所有</value>
    ''' <returns>車両所有</returns>
    ''' <remarks></remarks>
    Public Property BASELEASEF() As String
    ''' <summary>
    ''' 車両登録油種
    ''' </summary>
    ''' <value>油種</value>
    ''' <returns>油種</returns>
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
    ''' 統一車番 後1（上）
    ''' </summary>
    ''' <value>車両タイプ</value>
    ''' <returns>車両タイプ</returns>
    ''' <remarks></remarks>
    Public Property SHARYOTYPEB() As String
    ''' <summary>
    ''' 統一車番 後1（下）
    ''' </summary>
    ''' <value>統一車番</value>
    ''' <returns>統一車番</returns>
    ''' <remarks></remarks>
    Public Property TSHABANB() As String
    ''' <summary>
    ''' 車両管理部署コード 後1
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MANGMORGB() As String
    ''' <summary>
    ''' 車両設置部署コード 後1
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MANGSORGB() As String
    ''' <summary>
    ''' 車両所有 後1
    ''' </summary>
    ''' <value>車両所有</value>
    ''' <returns>車両所有</returns>
    ''' <remarks></remarks>
    Public Property BASELEASEB() As String
    ''' <summary>
    ''' 統一車番 後2（上）
    ''' </summary>
    ''' <value>車両タイプ</value>
    ''' <returns>車両タイプ</returns>
    ''' <remarks></remarks>
    Public Property SHARYOTYPEB2() As String
    ''' <summary>
    ''' 統一車番 後2（下）
    ''' </summary>
    ''' <value>統一車番</value>
    ''' <returns>統一車番</returns>
    ''' <remarks></remarks>
    Public Property TSHABANB2() As String
    ''' <summary>
    ''' 車両管理部署コード 後2
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MANGMORGB2() As String
    ''' <summary>
    ''' 車両設置部署コード 後2
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>部署コード</returns>
    ''' <remarks></remarks>
    Public Property MANGSORGB2() As String
    ''' <summary>
    ''' 車両所有 後2
    ''' </summary>
    ''' <value>車両所有</value>
    ''' <returns>車両所有</returns>
    ''' <remarks></remarks>
    Public Property BASELEASEB2() As String
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
    Public Const METHOD_NAME As String = "CS0045GSHABANORGget"
    ''' <summary>
    ''' 車番部署情報取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0045GSHABANORGget()

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

        'PARAM02: UORG
        If IsNothing(UORG) Then
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

        'PARAM03: GSHABAN
        If IsNothing(GSHABAN) Then
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
                        " SELECT isnull(rtrim(O.SHARYOTYPEF),'')  as SHARYOTYPEF     " _
                      & "      , isnull(rtrim(O.TSHABANF),'')     as TSHABANF        " _
                      & "      , isnull(rtrim(A1.MANGMORG),'')    as MANGMORGF       " _
                      & "      , isnull(rtrim(A1.MANGSORG),'')    as MANGSORGF       " _
                      & "      , isnull(rtrim(A1.BASELEASE),'')   as BASELEASEF      " _
                      & "      , isnull(rtrim(A1.MANGOILTYPE),'') as MANGOILTYPE     " _
                      & "      , isnull(rtrim(A1.MANGSUPPL),'')   as MANGSUPPL       " _
                      & "      , isnull(rtrim(O.SHARYOTYPEB),'')  as SHARYOTYPEB     " _
                      & "      , isnull(rtrim(O.TSHABANB),'')     as TSHABANB        " _
                      & "      , isnull(rtrim(A2.MANGMORG),'')    as MANGMORGB       " _
                      & "      , isnull(rtrim(A2.MANGSORG),'')    as MANGSORGB       " _
                      & "      , isnull(rtrim(A2.BASELEASE),'')   as BASELEASEB      " _
                      & "      , isnull(rtrim(O.SHARYOTYPEB2),'') as SHARYOTYPEB2    " _
                      & "      , isnull(rtrim(O.TSHABANB2),'')    as TSHABANB2       " _
                      & "      , isnull(rtrim(A3.MANGMORG),'')    as MANGMORGB2      " _
                      & "      , isnull(rtrim(A3.MANGSORG),'')    as MANGSORGB2      " _
                      & "      , isnull(rtrim(A3.BASELEASE),'')   as BASELEASEB2     " _
                      & "      , isnull(rtrim(O.CAMPCODE),'')     as CAMPCODE        " _
                      & "      , isnull(rtrim(O.MANGUORG),'')     as MANGUORG        " _
                      & "      , isnull(rtrim(O.GSHABAN),'')      as GSHABAN         " _
                      & "   FROM OIL.MA006_SHABANORG  O                             " _
                      & "   LEFT JOIN OIL.MA002_SHARYOA A1 						    " _
                      & "     ON A1.CAMPCODE   	= O.CAMPCODE 				    " _
                      & "    and A1.SHARYOTYPE  = O.SHARYOTYPEF 		        " _
                      & "    and A1.TSHABAN     = O.TSHABANF 		            " _
                      & "    and A1.STYMD      <= @P3                           " _
                      & "    and A1.ENDYMD     >= @P4                           " _
                      & "    and A1.DELFLG     <> '1' 						    " _
                      & "   LEFT JOIN OIL.MA002_SHARYOA A2 						    " _
                      & "     ON A2.CAMPCODE   	= O.CAMPCODE 				    " _
                      & "    and A2.SHARYOTYPE  = O.SHARYOTYPEB 		        " _
                      & "    and A2.TSHABAN     = O.TSHABANB 		            " _
                      & "    and A2.STYMD      <= @P3                           " _
                      & "    and A2.ENDYMD     >= @P4                           " _
                      & "    and A2.DELFLG     <> '1' 						    " _
                      & "   LEFT JOIN OIL.MA002_SHARYOA A3 						    " _
                      & "     ON A3.CAMPCODE   	= O.CAMPCODE 				    " _
                      & "    and A3.SHARYOTYPE  = O.SHARYOTYPEB2 		        " _
                      & "    and A3.TSHABAN     = O.TSHABANB2 		            " _
                      & "    and A3.STYMD      <= @P3                           " _
                      & "    and A3.ENDYMD     >= @P4                           " _
                      & "    and A3.DELFLG     <> '1' 						    " _
                      & "  Where O.CAMPCODE    =  @P1                           " _
                      & "    and O.MANGUORG    =  @P2                           " _
                      & "    and O.DELFLG      <> '1'                           "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                PARA1.Value = CAMPCODE
                PARA2.Value = UORG
                PARA3.Value = ENDYMD
                PARA4.Value = STYMD
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

            SHARYOTYPEF = ""
            TSHABANF = ""
            MANGMORGF = ""
            MANGSORGF = ""
            BASELEASEF = ""
            SHARYOTYPEB = ""
            TSHABANB = ""
            MANGMORGB = ""
            MANGSORGB = ""
            BASELEASEB = ""
            SHARYOTYPEB2 = ""
            TSHABANB2 = ""
            MANGMORGB2 = ""
            MANGSORGB2 = ""
            BASELEASEB2 = ""
            MANGOILTYPE = ""
            MANGSUPPL = ""
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            For Each TBLrow As DataRow In TBL.Rows
                If TBLrow("CAMPCODE") = CAMPCODE And
                   TBLrow("MANGUORG") = UORG And
                   TBLrow("GSHABAN") = GSHABAN Then
                    SHARYOTYPEF = TBLrow("SHARYOTYPEF")
                    TSHABANF = TBLrow("TSHABANF")
                    MANGMORGF = TBLrow("MANGMORGF")
                    MANGSORGF = TBLrow("MANGSORGF")
                    BASELEASEF = TBLrow("BASELEASEF")
                    SHARYOTYPEB = TBLrow("SHARYOTYPEB")
                    TSHABANB = TBLrow("TSHABANB")
                    MANGMORGB = TBLrow("MANGMORGB")
                    MANGSORGB = TBLrow("MANGSORGB")
                    BASELEASEB = TBLrow("BASELEASEB")
                    SHARYOTYPEB2 = TBLrow("SHARYOTYPEB2")
                    TSHABANB2 = TBLrow("TSHABANB2")
                    MANGMORGB2 = TBLrow("MANGMORGB2")
                    MANGSORGB2 = TBLrow("MANGSORGB2")
                    BASELEASEB2 = TBLrow("BASELEASEB2")
                    MANGOILTYPE = TBLrow("MANGOILTYPE")
                    MANGSUPPL = TBLrow("MANGSUPPL")

                    ERR = C_MESSAGE_NO.NORMAL
                    Exit For
                End If
            Next

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME          'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA006_SHABANORG Select"        '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Structure

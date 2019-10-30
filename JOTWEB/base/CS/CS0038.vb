Imports System.Data.SqlClient

''' <summary>
''' 勘定科目取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0038ACCODEget
    ''' <summary>
    ''' 保管用勘定科目判定テーブル
    ''' </summary>
    ''' <value>テーブルデータ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TBL() As DataTable
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 適用開始日
    ''' </summary>
    ''' <value>開始日</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STYMD() As Date
    ''' <summary>
    ''' 適用終了日
    ''' </summary>
    ''' <value>終了日</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDYMD() As Date
    ''' <summary>
    ''' 元帳
    ''' </summary>
    ''' <value>元帳</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MOTOCHO() As String
    ''' <summary>
    ''' 伝票タイプ
    ''' </summary>
    ''' <value>伝票タイプ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DENTYPE() As String
    ''' <summary>
    ''' 勘定科目判定コード
    ''' </summary>
    ''' <value>勘定科目判定コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ACHANTEI() As String
    ''' <summary>
    ''' 荷主コード
    ''' </summary>
    ''' <value>取引先コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String
    ''' <summary>
    ''' 取引タイプ01
    ''' </summary>
    ''' <value>取引タイプ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORITYPE01() As String
    ''' <summary>
    ''' 取引タイプ02
    ''' </summary>
    ''' <value>取引タイプ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORITYPE02() As String
    ''' <summary>
    ''' 取引タイプ03
    ''' </summary>
    ''' <value>取引タイプ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORITYPE03() As String
    ''' <summary>
    ''' 取引タイプ04
    ''' </summary>
    ''' <value>取引タイプ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORITYPE04() As String
    ''' <summary>
    ''' 取引タイプ05
    ''' </summary>
    ''' <value>取引タイプ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORITYPE05() As String
    ''' <summary>
    ''' 売上計上基準
    ''' </summary>
    ''' <value>売掛区分</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URIKBN() As String
    ''' <summary>
    ''' 販売店コード
    ''' </summary>
    ''' <value>取引先コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STORICODE() As String
    ''' <summary>
    ''' 油種
    ''' </summary>
    ''' <value>油種</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OILTYPE() As String
    ''' <summary>
    ''' 品名１
    ''' </summary>
    ''' <value>品名コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PRODUCT1() As String
    ''' <summary>
    ''' 社有・庸車区分
    ''' </summary>
    ''' <value>社有・庸車区分</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SUPPLIERKBN() As String
    ''' <summary>
    ''' 車両設置部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MANGSORG() As String
    ''' <summary>
    ''' 車両運用部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MANGUORG() As String
    ''' <summary>
    ''' 車両所有
    ''' </summary>
    ''' <value>車両所有</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property BASELEASE() As String
    ''' <summary>
    ''' 社員区分
    ''' </summary>
    ''' <value>社員区分</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFKBN() As String
    ''' <summary>
    ''' 配属部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HORG() As String
    ''' <summary>
    ''' 作業部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SORG() As String
    ''' <summary>
    ''' 勘定科目コード
    ''' </summary>
    ''' <value>勘定科目コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ACCODE() As String
    ''' <summary>
    ''' 補助科目コード
    ''' </summary>
    ''' <value>勘定科目コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SUBACCODE() As String
    ''' <summary>
    ''' 照会区分
    ''' </summary>
    ''' <value>照会区分</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property INQKBN() As String
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
    Public Const METHOD_NAME As String = "CS0038ACCODEget"

    ''' <summary>
    ''' 勘定科目取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0038ACCODEget()

        '●In PARAMチェック

        'PARAM00: TBL
        If IsNothing(TBL) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
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
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: STYMD
        If STYMD < C_DEFAULT_YMD Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "STYMD"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM03: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ENDYMD"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM04: MOTOCHO
        If IsNothing(MOTOCHO) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MOTOCHO"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM05: DENTYPE
        If IsNothing(DENTYPE) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DENTYPE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM06: ACHANTEI
        If IsNothing(ACHANTEI) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "ACHANTEI"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM07～23: 指定なしＯＫ
        If IsNothing(TORICODE) Then
            TORICODE = ""
        End If
        If IsNothing(TORITYPE01) Then
            TORITYPE01 = ""
        End If
        If IsNothing(TORITYPE02) Then
            TORITYPE02 = ""
        End If
        If IsNothing(TORITYPE03) Then
            TORITYPE03 = ""
        End If
        If IsNothing(TORITYPE04) Then
            TORITYPE04 = ""
        End If
        If IsNothing(TORITYPE05) Then
            TORITYPE05 = ""
        End If
        If IsNothing(URIKBN) Then
            URIKBN = ""
        End If
        If IsNothing(STORICODE) Then
            STORICODE = ""
        End If
        If IsNothing(OILTYPE) Then
            OILTYPE = ""
        End If
        If IsNothing(PRODUCT1) Then
            PRODUCT1 = ""
        End If
        If IsNothing(SUPPLIERKBN) Then
            SUPPLIERKBN = ""
        End If
        If IsNothing(MANGSORG) Then
            MANGSORG = ""
        End If
        If IsNothing(MANGUORG) Then
            MANGUORG = ""
        End If
        If IsNothing(BASELEASE) Then
            BASELEASE = ""
        End If
        If IsNothing(STAFFKBN) Then
            STAFFKBN = ""
        End If
        If IsNothing(HORG) Then
            HORG = ""
        End If
        If IsNothing(SORG) Then
            SORG = ""
        End If

        '●項目情報取得
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        Try
            '初回の呼び出しのみSELECTする（保管用のテーブルを受渡しすることで初回を判定）

            If TBL.Columns.Count = 0 Then

                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQL_Str As String =
                      "SELECT rtrim(CAMPCODE)               as CAMPCODE    " _
                    & "      ,SEQ                           as SEQ         " _
                    & "      ,STYMD                         as STYMD       " _
                    & "      ,ENDYMD                        as ENDYMD      " _
                    & "      ,isnull(rtrim(MOTOCHO),'')     as MOTOCHO     " _
                    & "      ,isnull(rtrim(DENTYPE),'')     as DENTYPE     " _
                    & "      ,isnull(rtrim(ACHANTEI),'')    as ACHANTEI    " _
                    & "      ,isnull(rtrim(TORICODE),'')    as TORICODE    " _
                    & "      ,isnull(rtrim(TORITYPE01),'')  as TORITYPE01  " _
                    & "      ,isnull(rtrim(TORITYPE02),'')  as TORITYPE02  " _
                    & "      ,isnull(rtrim(TORITYPE03),'')  as TORITYPE03  " _
                    & "      ,isnull(rtrim(TORITYPE04),'')  as TORITYPE04  " _
                    & "      ,isnull(rtrim(TORITYPE05),'')  as TORITYPE05  " _
                    & "      ,isnull(rtrim(URIKBN),'')      as URIKBN      " _
                    & "      ,isnull(rtrim(STORICODE),'')   as STORICODE   " _
                    & "      ,isnull(rtrim(OILTYPE),'')     as OILTYPE     " _
                    & "      ,isnull(rtrim(PRODUCT1),'')    as PRODUCT1    " _
                    & "      ,isnull(rtrim(SUPPLIERKBN),'') as SUPPLIERKBN " _
                    & "      ,isnull(rtrim(MANGSORG),'')    as MANGSORG    " _
                    & "      ,isnull(rtrim(MANGUORG),'')    as MANGUORG    " _
                    & "      ,isnull(rtrim(BASELEASE),'')   as BASELEASE   " _
                    & "      ,isnull(rtrim(STAFFKBN),'')    as STAFFKBN    " _
                    & "      ,isnull(rtrim(HORG),'')        as HORG        " _
                    & "      ,isnull(rtrim(SORG),'')        as SORG        " _
                    & "      ,isnull(rtrim(ACCODE),'')      as ACCODE      " _
                    & "      ,isnull(rtrim(SUBACCODE),'')   as SUBACCODE   " _
                    & "      ,isnull(rtrim(INQKBN),'')      as INQKBN      " _
                    & " FROM  OIL.ML002_ACHANTEI " _
                    & " WHERE CAMPCODE = @P1 " _
                    & "   and DENTYPE  = @P2 " _
                    & "   and DELFLG  <> @P3 " _
                    & " ORDER BY SEQ "

                Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = CAMPCODE
                PARA2.Value = DENTYPE
                PARA3.Value = C_DELETE_FLG.DELETE

                'SELECT実行
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

            '出力パラメータ初期設定
            ACCODE = ""
            SUBACCODE = ""
            INQKBN = ""
            ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR

            '勘定科目判定（最初にヒットした勘定科目を設定する）
            Dim WW_CNT As Integer = 0
            For Each WW_ROW As DataRow In TBL.Rows
                If WW_ROW("MOTOCHO") = MOTOCHO And
                   WW_ROW("ACHANTEI") = ACHANTEI And
                   WW_ROW("STYMD") <= STYMD And
                   WW_ROW("ENDYMD") >= ENDYMD Then
                Else
                    WW_CNT += 1
                    Continue For
                End If

                '荷主コード
                If Not startWith(WW_ROW("TORICODE"), TORICODE) Then
                    Continue For
                End If

                '取引タイプ01
                If Not startWith(WW_ROW("TORITYPE01"), TORITYPE01) Then
                    Continue For
                End If

                '取引タイプ02
                If Not startWith(WW_ROW("TORITYPE02"), TORITYPE02) Then
                    Continue For
                End If

                '取引タイプ03
                If Not startWith(WW_ROW("TORITYPE03"), TORITYPE03) Then
                    Continue For
                End If

                '取引タイプ04
                If Not startWith(WW_ROW("TORITYPE04"), TORITYPE04) Then
                    Continue For
                End If

                '取引タイプ05
                If Not startWith(WW_ROW("TORITYPE05"), TORITYPE05) Then
                    Continue For
                End If

                '売上計上基準
                If Not startWith(WW_ROW("URIKBN"), URIKBN) Then
                    Continue For
                End If

                '販売店コード
                If Not startWith(WW_ROW("STORICODE"), STORICODE) Then
                    Continue For
                End If

                '油種
                If Not startWith(WW_ROW("OILTYPE"), OILTYPE) Then
                    Continue For
                End If

                '品名１
                If Not startWith(WW_ROW("PRODUCT1"), PRODUCT1) Then
                    Continue For
                End If

                '社有・庸車区分
                If Not startWith(WW_ROW("SUPPLIERKBN"), SUPPLIERKBN) Then
                    Continue For
                End If

                '車両設置部署
                If Not startWith(WW_ROW("MANGSORG"), MANGSORG) Then
                    Continue For
                End If

                '車両運用部署
                If Not startWith(WW_ROW("MANGUORG"), MANGUORG) Then
                    Continue For
                End If

                '車両所有
                If Not startWith(WW_ROW("BASELEASE"), BASELEASE) Then
                    Continue For
                End If

                '乗務員・社員区分
                If Not startWith(WW_ROW("STAFFKBN"), STAFFKBN) Then
                    Continue For
                End If

                '乗務員・配属部署
                If Not startWith(WW_ROW("HORG"), HORG) Then
                    Continue For
                End If

                '乗務員・作業部署
                If Not startWith(WW_ROW("SORG"), SORG) Then
                    Continue For
                End If

                '返却値設定（勘定科目、補助科目）
                ACCODE = WW_ROW("ACCODE")
                SUBACCODE = WW_ROW("SUBACCODE")
                INQKBN = WW_ROW("INQKBN")

                ERR = C_MESSAGE_NO.NORMAL
                '最初にヒットした時点で処理（ループ）終了
                Exit For
            Next

            '勘定科目判定テーブルが0件の場合、エラーを返却
            If TBL.Rows.Count = WW_CNT Then
                Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                 'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:ML002_ACHANTEI Select"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = "勘定科目判定マスタ（ML002_ACHANTEI）に存在しません。"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力

                ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            End If

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:ML002_ACHANTEI Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 前方一致検索処理
    ''' </summary>
    ''' <param name="value">検索対象文字列</param>
    ''' <param name="search">検索文字</param>
    ''' <returns>TRUE:存在する　FALSE：存在しない</returns>
    ''' <remarks><para>検索文字列が前方一致検索した場合に存在するか判定する</para>
    ''' <para>ただし検索対象文字列がワイルドカード（＊）の場合は存在していることとする</para>
    ''' </remarks>
    Private Function startWith(ByVal value As String, ByVal search As String)
        Const WILD_CARD As String = "*"
        '検索対象がワイルドカードなら対象
        If value = WILD_CARD Then
            startWith = True
            ' 検索文字が存在し、検索対象に含まれる（前方一致）なら対象
        ElseIf search.Length > 0 And value Like search & WILD_CARD Then
            startWith = True
        Else
            startWith = False
        End If
    End Function
End Structure

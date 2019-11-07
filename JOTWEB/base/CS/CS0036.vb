Imports System.Data.SqlClient

''' <summary>
''' FIELDDATAによる項目のチェック処理
''' </summary>
''' <remarks></remarks>
Public Class CS0036FCHECK
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value> 会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' チェック対象画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String
    ''' <summary>
    ''' チェック対象項目名
    ''' </summary>
    ''' <value>項目名</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FIELD() As String
    ''' <summary>
    ''' チェック対象の値
    ''' </summary>
    ''' <value>項目値</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE() As String
    ''' <summary>
    ''' DATAFIELD格納テーブル
    ''' </summary>
    ''' <value>テーブルデータ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TBL() As DataTable
    ''' <summary>
    ''' 結果LIST
    ''' </summary>
    ''' <value></value>
    ''' <returns>結果LIST</returns>
    ''' <remarks></remarks>
    Public Property CHECKREPORT() As String
    ''' <summary>
    ''' 編集後の項目値
    ''' </summary>
    ''' <value></value>
    ''' <returns>項目値</returns>
    ''' <remarks></remarks>
    Public Property VALUEOUT() As String
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String
    ''' <summary>
    ''' 固定桁数かチェックするフラグ
    ''' </summary>
    ''' <value>フラグ</value>
    ''' <returns>TRUE;実施、それ以外：未実施</returns>
    Public Property SAMEFLG() As Boolean
    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0036FCHECK"

    'セッション制御宣言
    Private sm As New CS0050SESSION

    '''' <summary>
    '''' S00013に対応するチェック内容が存在するか確認する
    '''' </summary>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Function existsCheckField() As Boolean
    '    '●In PARAMチェック
    '    Dim WW_SINGLECHECK As Boolean = False
    '    'PARAM01: CAMPCODE
    '    If IsNothing(CAMPCODE) Then
    '        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

    '        CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "CAMPCODE"                          '
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
    '        CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
    '        Return False
    '    End If

    '    'PARAM02: MAPID
    '    If IsNothing(MAPID) Then
    '        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

    '        CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "MAPID"                          '
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
    '        CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
    '        Return False
    '    End If

    '    'PARAM03: FIELD
    '    If IsNothing(FIELD) Then
    '        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

    '        CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "FIELD"                          '
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
    '        CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
    '        Return False
    '    End If
    '    'EXTRAPARAM01: TBL
    '    If IsNothing(TBL) Then
    '        WW_SINGLECHECK = True
    '        TBL = New DataTable
    '    End If
    '    'PARAM04: I_VALUE  空白を認める

    '    '●項目情報取得
    '    Try
    '        createFieldDataTbl(WW_SINGLECHECK)
    '        If Not isNormal(ERR) Then Return False


    '        If TBL.Rows.Count = 0 Then
    '            If Not WW_SINGLECHECK Then
    '                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

    '                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
    '                CS0011LOGWRITE.INFPOSI = "DB:OIS0016_DATAFIELD Select"             '
    '                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                     '
    '                CS0011LOGWRITE.TEXT = "データフィールドマスタ（OIS0016_DATAFIELD）に存在しません。"
    '                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
    '                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

    '                ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
    '            End If
    '            Return False
    '        End If
    '        Dim WW_row() As DataRow = TBL.Select("FIELD='" & FIELD & "'")
    '        If WW_row.Count = 0 Then Return False

    '        Return True

    '    Catch ex As Exception
    '        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get

    '        CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "DB:OIS0016_DATAFIELD Select"
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWRITE.TEXT = ex.ToString()
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

    '        ERR = C_MESSAGE_NO.DB_ERROR
    '        Return False

    '    Finally
    '        '〇単独処理の場合TBLを除去する
    '        If WW_SINGLECHECK Then
    '            TBL.Dispose()
    '            TBL = Nothing
    '        End If
    '    End Try

    'End Function

    ''' <summary>
    ''' FIELDDATAによる項目のチェック処理
    ''' </summary>
    ''' <remarks>既存の保証</remarks>
    Public Sub CS0036FCHECK()
        Me.check()
    End Sub
    ''' <summary>
    ''' FIELDDATAによる項目のチェック処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub check()
        '●In PARAMチェック
        Dim WW_SINGLECHECK As Boolean = False
        'PARAM01: CAMPCODE
        If IsNothing(CAMPCODE) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: MAPID
        If IsNothing(MAPID) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM03: FIELD
        If IsNothing(FIELD) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "FIELD"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        'EXTRAPARAM01: TBL
        If IsNothing(TBL) Then
            WW_SINGLECHECK = True
            TBL = New DataTable
        End If
        'PARAM04: I_VALUE  空白を認める

        '●項目情報取得
        Try
            createFieldDataTbl(WW_SINGLECHECK)
            If Not isNormal(ERR) Then Exit Sub

            If TBL.Rows.Count = 0 Then
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:OIS0016_DATAFIELD Select"             '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                     '
                CS0011LOGWRITE.TEXT = "データフィールドマスタ（OIS0016_DATAFIELD）に存在しません。"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End If

            Dim WW_row() As DataRow = TBL.Select("FIELD='" & FIELD & "'")

            '出力パラメータ初期設定
            CHECKREPORT = ""
            ERR = C_MESSAGE_NO.NORMAL
            VALUEOUT = VALUE

            Dim WW_DATE As Date = Date.Now
            Dim WW_TIME As DateTime
            Dim WW_VALUE_SAVE As String = VALUE
            Dim i As Integer = 0

            If WW_row.Count > 0 Then
                '○必須チェック
                If WW_row(i)("MUST") = CONST_FLAG_YES Then
                    If String.IsNullOrEmpty(VALUE) Then
                        CHECKREPORT = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT & "(" & VALUE & ")"
                        ERR = C_MESSAGE_NO.PREREQUISITE_ERROR
                        Exit Sub
                    End If
                End If

                '○項目属性別チェック
                Select Case WW_row(i)("FIELDTYPE")
                    Case "NUM"
                        If String.IsNullOrEmpty(VALUE) Then
                            '空欄は、0を設定
                            VALUE = "0"
                            VALUEOUT = "0"
                        End If

                        '有効桁数チェック
                        Dim WW_VALUE As Double = 0
                        Dim WW_INT_SIDE As String = ""
                        Dim WW_DEC_SIDE As String = ""
                        Dim WW_I_VALUE As String = Replace(VALUE, ",", "")

                        '項目属性チェック
                        If Double.TryParse(WW_I_VALUE, WW_VALUE) Then
                        Else
                            CHECKREPORT = C_MESSAGE_TEXT.NUMERIC_ERROR_TEXT & "(" & VALUE & ")"
                            ERR = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                            VALUEOUT = "0"
                            Exit Sub
                        End If

                        '桁数チェック準備
                        Try
                            If InStr(WW_I_VALUE, ".") = 0 Then
                                WW_INT_SIDE = WW_I_VALUE
                                WW_DEC_SIDE = ""
                            Else
                                WW_INT_SIDE = Mid(WW_I_VALUE, 1, InStr(WW_I_VALUE, ".") - 1)
                                WW_DEC_SIDE = Mid(WW_I_VALUE, InStr(WW_I_VALUE, ".") + 1, 100)
                            End If
                        Catch ex As Exception
                            CHECKREPORT = C_MESSAGE_TEXT.NUMERIC_ERROR_TEXT & "(" & VALUE & ")"
                            ERR = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                            VALUEOUT = "0"
                            Exit Sub
                        End Try

                        '　整数部チェック
                        If WW_row(i)("INTLENG") <> 0 Then            'データフィールドマスタ(OIS0016_DATAFIELD)　桁数未設定
                            Try
                                If WW_INT_SIDE.Length > WW_row(i)("INTLENG") Then
                                    CHECKREPORT = C_MESSAGE_TEXT.INTEGER_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                    ERR = C_MESSAGE_NO.INTEGER_LENGTH_OVER_ERROR
                                    VALUEOUT = "0"
                                    Exit Sub
                                End If
                            Catch ex As Exception
                                CHECKREPORT = C_MESSAGE_TEXT.NUMERIC_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                                VALUEOUT = "0"
                                Exit Sub
                            End Try
                        End If

                        '　小数部チェック
                        If WW_row(i)("DECLENG") = 0 Then            'データフィールドマスタ(OIS0016_DATAFIELD)　桁数未設定　
                            If WW_DEC_SIDE.Length > 0 Then
                                CHECKREPORT = C_MESSAGE_TEXT.DECIMAL_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DECIMAL_LENGTH_OVER_ERROR
                                VALUEOUT = "0"
                                Exit Sub
                            End If
                        Else
                            Try
                                If WW_DEC_SIDE.Length > WW_row(i)("DECLENG") Then
                                    CHECKREPORT = C_MESSAGE_TEXT.DECIMAL_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                    ERR = C_MESSAGE_NO.DECIMAL_LENGTH_OVER_ERROR
                                    VALUEOUT = "0"
                                    Exit Sub
                                End If
                            Catch ex As Exception
                                CHECKREPORT = C_MESSAGE_TEXT.DECIMAL_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DECIMAL_LENGTH_OVER_ERROR
                                VALUEOUT = "0"
                                Exit Sub
                            End Try
                        End If

                        '有効桁数編集
                        If WW_row(i)("INTLENG") <> 0 And WW_row(i)("DECLENG") = 0 Then
                            VALUEOUT = Right("0000000000" & WW_I_VALUE.ToString, WW_row(i)("INTLENG"))
                        Else
                            VALUEOUT = WW_I_VALUE
                        End If


                    Case "DATE"
                        ' 項目属性チェック
                        If VALUE <> "" Then
                            Try
                                Date.TryParse(VALUE, WW_DATE)
                            Catch ex As Exception
                                CHECKREPORT = C_MESSAGE_TEXT.DATE_FORMAT_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                Exit Sub
                            End Try

                            If WW_DATE < C_DEFAULT_YMD Then
                                CHECKREPORT = C_MESSAGE_TEXT.DATE_FORMAT_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                Exit Sub
                            End If

                            '2018/04/24 追加
                            If WW_DATE > C_MAX_YMD Then
                                CHECKREPORT = C_MESSAGE_TEXT.DATE_MAX_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                Exit Sub
                            End If

                            VALUEOUT = WW_DATE.ToString("yyyy/MM/dd")
                        Else
                            VALUEOUT = ""
                        End If

                    Case "TIME"
                        ' 項目属性チェック
                        If VALUE <> "" Then
                            VALUE = StrConv(VALUE, VbStrConv.Narrow)
                            Try
                            If VALUE.Contains(":") Then
                                    WW_TIME = VALUE
                                Else
                                    WW_TIME = VALUE.PadLeft(4, "0").Insert(2, ":")
                                End If

                                VALUEOUT = WW_TIME.ToString("H:mm")
                            Catch ex As Exception
                                CHECKREPORT = C_MESSAGE_TEXT.TIME_FORMAT_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                Exit Sub
                            End Try

                            '　整数部チェック
                            If WW_row(i)("INTLENG") <> 0 Then            'データフィールドマスタ(OIS0016_DATAFIELD)　桁数未設定
                                Dim WW_MINUTE As Integer = WW_TIME.Hour * 60 + WW_TIME.Minute

                                Try
                                    If WW_MINUTE Mod WW_row(i)("INTLENG") <> 0 Then
                                        CHECKREPORT = WW_row(i)("INTLENG") & C_MESSAGE_TEXT.TIME_FORMAT_SPLIT_ERROR_TEXT & "(" & VALUE & ")"
                                        ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                        Exit Sub
                                    End If
                                Catch ex As Exception
                                    CHECKREPORT = C_MESSAGE_TEXT.TIME_FORMAT_ERROR_TEXT & "(" & VALUE & ")"
                                    ERR = C_MESSAGE_NO.DATE_FORMAT_ERROR
                                    VALUEOUT = ""
                                    Exit Sub
                                End Try
                            End If
                        Else
                            VALUEOUT = ""
                        End If

                    Case "STR"
                        ' 有効桁数チェック
                        If WW_row(i)("INTLENG") <> 0 Then
                            '桁数判断
                            If VALUE.Length > WW_row(i)("INTLENG") Then
                                CHECKREPORT = C_MESSAGE_TEXT.STRING_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                                ERR = C_MESSAGE_NO.STRING_LENGTH_OVER_ERROR
                                Exit Sub
                            End If
                        End If

                        VALUEOUT = VALUE

                End Select

                '固定値マスタ存在チェック
                If WW_row(i)("FVCHECK") = CONST_FLAG_YES Then
                    Dim WW_FINDrow() As DataRow = TBL.Select("FIELD='" & FIELD & "' and KEYCODE='" & WW_VALUE_SAVE & "'")
                    If WW_FINDrow.Count = 0 Then
                        CHECKREPORT = C_MESSAGE_TEXT.SELECT_INVALID_VALUE_ERROR & "(" & VALUE & ")"
                        ERR = C_MESSAGE_NO.INVALID_SELECTION_DATA
                        'Exit Sub
                    End If
                End If

                '固定桁数かチェック
                If SAMEFLG Then
                    If WW_row(i)("INTLENG") <> VALUEOUT.Length Then
                        CHECKREPORT = C_MESSAGE_TEXT.INTEGER_LENGTH_OVER_ERROR_TEXT & "(" & VALUE & ")"
                        ERR = C_MESSAGE_NO.INTEGER_LENGTH_OVER_ERROR
                        'Exit Sub
                    End If
                End If
            Else
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:OIS0016_DATAFIELD Select"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = "OIS0016_DATAFIELDに" & FIELD & "が存在しません。"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR
            End If
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0016_DATAFIELD Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        Finally
            '〇単独処理の場合TBLを除去する
            If WW_SINGLECHECK Then
                TBL.Dispose()
                TBL = Nothing
            End If

        End Try
    End Sub
    ''' <summary>
    ''' OIS0016_DATAFIELD
    ''' </summary>
    ''' <param name="I_SINGLE"></param>
    ''' <remarks></remarks>
    Protected Sub createFieldDataTbl(ByVal I_SINGLE As Boolean)
        '○ DB(OIS0016_DATAFIELD)検索
        ERR = C_MESSAGE_NO.NORMAL
        Try

            Dim S0013row As DataRow

            If TBL.Columns.Count = 0 Then
                TBL.Clear()
                TBL.Columns.Add("FIELD", GetType(String))
                TBL.Columns.Add("FIELDTYPE", GetType(String))
                TBL.Columns.Add("INTLENG", GetType(String))
                TBL.Columns.Add("DECLENG", GetType(String))
                TBL.Columns.Add("MUST", GetType(String))
                TBL.Columns.Add("FVCHECK", GetType(String))
                TBL.Columns.Add("KEYCODE", GetType(String))
                'インデックス作成
                TBL.DefaultView.Sort = "FIELD,KEYCODE"

                'テンポラリDB項目作成
                '○指定ﾊﾟﾗﾒｰﾀで検索
                'DataBase接続文字
                Using SQLcon = sm.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    'CAMPCODE検索SQL文
                    Dim SQL_Str As String =
                                 " SELECT " _
                               & "          FIELD                                     , " _
                               & "          FIELDTYPE                                 , " _
                               & "          INTLENG                                   , " _
                               & "          DECLENG                                   , " _
                               & "          MUST                                      , " _
                               & "          FVCHECK                                   , " _
                               & "          KEYCODE                                     " _
                               & " FROM                                                 " _
                               & "  (                                            " _
                               & "   SELECT                                             " _
                               & "            A.FIELD                                 , " _
                               & "            A.FIELDTYPE                             , " _
                               & "            A.INTLENG                               , " _
                               & "            A.DECLENG                               , " _
                               & "            A.MAST               AS MUST            , " _
                               & "            A.FVCHECK                               , " _
                               & "            B.KEYCODE                               , " _
                               & "            RANK() OVER(                        " _
                               & "                 PARTITION BY                         " _
                               & "                        A.FIELD                     , " _
                               & "                        A.MAPID                       " _
                               & "                 ORDER BY                             " _
                               & "                        CASE A.CAMPCODE               " _
                               & "                        WHEN '" & C_DEFAULT_DATAKEY & "' THEN 2         " _
                               & "                        ELSE 1 END                    " _
                               & "                       ) AS RNK                       " _
                               & "   FROM                                               " _
                               & "             COM.OIS0016_DATAFIELD             A            " _
                               & "   LEFT JOIN COM.OIS0015_FIXVALUE              B       ON   " _
                               & "            B.CLASS      = A.FIELD                    " _
                               & "        and B.STYMD     <= @P3                        " _
                               & "        and B.ENDYMD    >= @P4                        " _
                               & "        and B.DELFLG    <> @P5                        " _
                               & "   Where                                              " _
                               & "            A.CAMPCODE IN (@P1,'" & C_DEFAULT_DATAKEY & "') " _
                               & "        and A.MAPID      = @P2                        " _
                               & "        and A.STYMD     <= @P3                        " _
                               & "        and A.ENDYMD    >= @P4                        " _
                               & "        and A.DELFLG    <> @P5                        " _
                               & "  ) MAIN                                              " _
                               & " WHERE                                                " _
                               & "           RNK = 1                                    "
                    If I_SINGLE Then
                        SQL_Str = SQL_Str & String.Format(" and FIELD = '{0}' ", Me.FIELD)
                    End If

                    Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                        PARA1.Value = CAMPCODE
                        PARA2.Value = MAPID
                        PARA3.Value = Date.Now
                        PARA4.Value = Date.Now
                        PARA5.Value = C_DELETE_FLG.DELETE
                        Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            S0013row = TBL.NewRow
                            S0013row("FIELD") = SQLdr("FIELD")
                            S0013row("FIELDTYPE") = SQLdr("FIELDTYPE")
                            S0013row("INTLENG") = SQLdr("INTLENG")
                            S0013row("DECLENG") = SQLdr("DECLENG")
                            S0013row("MUST") = SQLdr("MUST")
                            S0013row("FVCHECK") = SQLdr("FVCHECK")
                            S0013row("KEYCODE") = SQLdr("KEYCODE")
                            TBL.Rows.Add(S0013row)
                        End While
                        'Close
                        SQLdr.Close() 'Reader(Close)
                        SQLdr = Nothing
                    End Using
                End Using
            End If
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0016_DATAFIELD Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
        End Try
    End Sub
End Class

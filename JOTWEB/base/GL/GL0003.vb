Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 取引先情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0003CustomerList
    Inherits GL0000
    ''' <summary>
    ''' 取得条件
    ''' </summary>
    Public Enum LC_CUSTOMER_TYPE
        ''' <summary>
        ''' 全取得
        ''' </summary>
        ALL
        ''' <summary>
        ''' 全取得
        ''' </summary>
        WITHTERM
        ''' <summary>
        ''' 荷主
        ''' </summary> 
        OWNER
        ''' <summary>
        ''' 端末権限参照の荷主
        ''' </summary>
        OWNER_WITHTERM
        ''' <summary>
        ''' 庸車
        ''' </summary> 
        CARRIDE
        ''' <summary>
        ''' 端末権限参照の庸車
        ''' </summary>
        CARRIDE_WITHTERM
    End Enum
    ''' <summary>
    ''' 取引先タイプ一覧
    ''' </summary>
    ''' <remarks></remarks>
    Protected Class C_TORITYPE
        Public Const TYPE_01_GROUP As String = "02"
        ''' <summary>
        ''' 荷主　（TYPECODE2）
        ''' </summary>
        Public Const TYPE_02_OWNER As String = "NI"
        ''' <summary>
        ''' 庸車　（TYPECODE3)
        ''' </summary>
        Public Const TYPE_03_RIDECAR As String = "YO"

    End Class
    ''' <summary>
    '''　取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TYPE() As LC_CUSTOMER_TYPE
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As String
    ''' <summary>
    ''' ROLECODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE() As String
    ''' <summary>
    ''' 権限フラグ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PERMISSION() As String
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "getList"


    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理


        'PARAM 01: TYPE
        If checkParam(METHOD_NAME, TYPE) Then
            Exit Sub
        End If
        'PARAM 01: CAMPCODE
        If checkParam(METHOD_NAME, CAMPCODE) Then
            Exit Sub
        End If
        'PARAM EXTRA01: ORGCODE
        If IsNothing(ORGCODE) Then
            ORGCODE = String.Empty
        End If
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
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Select Case TYPE
            Case LC_CUSTOMER_TYPE.OWNER
                getOwnerList(SQLcon)
            Case LC_CUSTOMER_TYPE.OWNER_WITHTERM
                getOwnerTermList(SQLcon)
            Case LC_CUSTOMER_TYPE.CARRIDE
                getRideCarList(SQLcon)
            Case LC_CUSTOMER_TYPE.CARRIDE_WITHTERM
                getRideCarTermList(SQLcon)
            Case LC_CUSTOMER_TYPE.WITHTERM
                getCustomerTermList(SQLcon)
            Case Else
                getCustomerList(SQLcon)
        End Select

        SQLcon.Close() 'DataBase接続(Close)
        SQLcon.Dispose()
        SQLcon = Nothing

    End Sub
    ''' <summary>
    ''' 取引先一覧取得
    ''' </summary>
    Protected Sub getCustomerList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用荷主取得
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            Dim SQLStr As String
            '部署コード未設定時
            If String.IsNullOrEmpty(ORGCODE) Then
                '検索SQL文
                SQLStr = _
                      " SELECT                                          " _
                    & "            rtrim(A.TORICODE) as CODE ,          " _
                    & "            rtrim(A.NAMES)    as NAMES           " _
                    & " FROM       MC002_TORIHIKISAKI     A             " _
                    & " WHERE                                           " _
                    & "              A.CAMPCODE = @COMPCODE             " _
                    & "        and   A.STYMD   <= @ENDYMD               " _
                    & "        and   A.ENDYMD  >= @STYMD                " _
                    & "        and   A.DELFLG  <> '1'                   "
                '〇ソート条件追加
                Select Case DEFAULT_SORT
                    Case C_DEFAULT_SORT.CODE, String.Empty
                        SQLStr = SQLStr & " ORDER BY A.TORICODE, A.NAMES "
                    Case C_DEFAULT_SORT.NAMES
                        SQLStr = SQLStr & " ORDER BY A.NAMES, A.TORICODE "
                    Case C_DEFAULT_SORT.SEQ
                        SQLStr = SQLStr & " ORDER BY A.TORICODE, A.NAMES "
                    Case Else
                End Select
            Else
                SQLStr =
                      "   SELECT                                            " _
                    & "            rtrim(A.TORICODE)      as CODE ,         " _
                    & "            rtrim(A.NAMES) 	      as NAMES ,		" _
                    & "            rtrim(B.SEQ) 	      as SEQ  		    " _
                    & "         FROM MC002_TORIHIKISAKI as A 			    " _
                    & "   INNER JOIN MC003_TORIORG      as B ON  	        " _
                    & "              B.CAMPCODE          = A.CAMPCODE 		" _
                    & "          and B.TORICODE          = A.TORICODE 		" _
                    & "          and B.UORG              = @ORGCODE         " _
                    & "          and B.DELFLG           <> '1' 				" _
                    & "   WHERE                              				" _
                    & "              A.CAMPCODE          = @COMPCODE		" _
                    & "          and A.STYMD            <= @ENDYMD 			" _
                    & "          and A.ENDYMD           >= @STYMD 			" _
                    & "          and A.DELFLG           <> '1' 				" _
                    & "   GROUP BY A.TORICODE , A.NAMES , B.SEQ             "
                '〇ソート条件追加
                Select Case DEFAULT_SORT
                    Case C_DEFAULT_SORT.CODE
                        SQLStr = SQLStr & " ORDER BY A.TORICODE, A.NAMES , B.SEQ "
                    Case C_DEFAULT_SORT.NAMES
                        SQLStr = SQLStr & " ORDER BY A.NAMES, A.TORICODE , B.SEQ "
                    Case C_DEFAULT_SORT.SEQ, String.Empty
                        SQLStr = SQLStr & " ORDER BY B.SEQ, A.TORICODE , A.NAMES "
                    Case Else
                End Select
            End If

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_COMPCODE As SqlParameter = SQLcmd.Parameters.Add("@COMPCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                Dim P_ORGCODE As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar, 20)

                P_COMPCODE.Value = CAMPCODE
                P_STYMD.Value = STYMD
                P_ENDYMD.Value = ENDYMD
                P_ORGCODE.Value = ORGCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using


        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "getCustomerList"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC002_TORIHIKISAKI Select"
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
    ''' 取引先一覧取得
    ''' </summary>
    Protected Sub getCustomerTermList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用荷主取得
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            ORGCODE = sm.APSV_ORG

            Dim SQLStr As String = _
            "   SELECT " _
                & "            rtrim(A.TORICODE)      as CODE        ,  " _
                & "            rtrim(A.NAMES) 	      as NAMES       ,  " _
                & "            rtrim(B.SEQ)           as SEQ            " _
                & "         FROM MC002_TORIHIKISAKI as A 			    " _
                & "   INNER JOIN MC003_TORIORG      as B ON  	        " _
                & "              B.CAMPCODE          = A.CAMPCODE 		" _
                & "          and B.TORICODE          = A.TORICODE 		" _
                & "          and B.UORG              = @ORGCODE         " _
                & "          and B.DELFLG           <> '1' 				" _
                & "   WHERE                              				" _
                & "              A.CAMPCODED        <= @COMPCODE		" _
                & "          and A.STYMD            <= @ENDYMD 			" _
                & "          and A.ENDYMD           >= @STYMD 			" _
                & "          and A.DELFLG           <> '1' 				" _
                & "   GROUP BY A.TORICODE , A.NAMES , B.SEQ             "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.TORICODE, A.NAMES , B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES, A.TORICODE , B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ, A.TORICODE , A.NAMES "
                Case Else
            End Select

            Using SQLcmd = New SqlCommand(SQLStr, SQLcon)
                Dim P_COMPCODE As SqlParameter = SQLcmd.Parameters.Add("@COMPCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                Dim P_ORGCODE As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar, 20)
                P_COMPCODE.Value = CAMPCODE
                P_STYMD.Value = STYMD
                P_ENDYMD.Value = ENDYMD
                P_ORGCODE.Value = ORGCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力編集
                addListData(SQLdr)
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GS0010OWNCODEget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC002_TORIHIKISAKI Select"
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
    ''' 荷主一覧取得
    ''' </summary>
    Protected Sub getOwnerList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用荷主取得
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            Dim SQLStr As String = String.Empty
            '部署コード未設定時
            If String.IsNullOrEmpty(ORGCODE) Then
                '検索SQL文
                SQLStr =
                          " SELECT " _
                        & "            rtrim(A.TORICODE)      as CODE        ,  " _
                        & "            rtrim(A.NAMES) 	      as NAMES       ,  " _
                        & "            rtrim(B.SEQ)           as SEQ            " _
                        & " FROM       MC002_TORIHIKISAKI    A                  " _
                        & " INNER JOIN MC003_TORIORG         B         ON       " _
                        & "            B.CAMPCODE = A.CAMPCODE                  " _
                        & "        and B.TORICODE = A.TORICODE                  " _
                        & "        and B.TORITYPE02 = @TORITYPE                 " _
                        & "        and B.DELFLG  <> '1'                         " _
                        & " INNER JOIN S0006_ROLE C ON                          " _
                        & "            C.CAMPCODE = B.CAMPCODE                  " _
                        & "        and C.OBJECT   = @OBJECT                     " _
                        & "        and C.CODE     = B.UORG                      " _
                        & "        and C.ROLE     = @ROLE                       " _
                        & "        and C.PERMITCODE >= @PERMIT                  " _
                        & "        and C.STYMD   <= @ENDYMD                     " _
                        & "        and C.ENDYMD  >= @STYMD                      " _
                        & "        and C.DELFLG  <> '1'                         " _
                        & " Where                                               " _
                        & "       A.CAMPCODE    = @COMPCODE                     " _
                        & "   and A.STYMD      <= @ENDYMD                       " _
                        & "   and A.ENDYMD     >= @STYMD                        " _
                        & "   and A.DELFLG     <> '1'                           " _
                        & "GROUP BY A.TORICODE , A.NAMES , B.SEQ                "

            Else
                SQLStr =
                        " SELECT " _
                      & "            rtrim(A.TORICODE)      as CODE        ,    " _
                      & "            rtrim(A.NAMES) 	    as NAMES       ,    " _
                      & "            rtrim(B.SEQ)           as SEQ              " _
                      & " FROM  MC002_TORIHIKISAKI A                            " _
                      & " INNER JOIN MC003_TORIORG B                   ON       " _
                      & "            B.CAMPCODE    = A.CAMPCODE                 " _
                      & "        and B.TORICODE    = A.TORICODE                 " _
                      & "        and B.TORITYPE02  = @TORITYPE                  " _
                      & "        and B.UORG　　    = @ORGCODE                   " _
                      & "        and B.DELFLG     <> '1'                        " _
                      & " INNER JOIN S0006_ROLE C                      ON       " _
                      & "            C.CAMPCODE    = B.CAMPCODE                 " _
                      & "        and C.OBJECT      = @OBJECT                    " _
                      & "        and C.CODE        = B.UORG                     " _
                      & "        and C.ROLE        = @ROLE                      " _
                      & "        and C.PERMITCODE >= @PERMIT                    " _
                      & "        and C.STYMD      <= @ENDYMD                    " _
                      & "        and C.ENDYMD     >= @STYMD                     " _
                      & "        and C.DELFLG     <> '1'                        " _
                      & " Where                                                 " _
                      & "            A.CAMPCODE    = @COMPCODE                  " _
                      & "        and A.STYMD      <= @ENDYMD                    " _
                      & "        and A.ENDYMD     >= @STYMD                     " _
                      & "        and A.DELFLG     <> '1'                        " _
                      & "GROUP BY A.TORICODE , A.NAMES , B.SEQ                  "
            End If
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.TORICODE, A.NAMES , B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES, A.TORICODE , B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ, A.TORICODE , A.NAMES "
                Case Else
            End Select

            Using SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim P_COMPCODE As SqlParameter = SQLcmd.Parameters.Add("@COMPCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_ROLE As SqlParameter = SQLcmd.Parameters.Add("@ROLE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                Dim P_TORITYPE As SqlParameter = SQLcmd.Parameters.Add("@TORITYPE", System.Data.SqlDbType.NVarChar, 3)
                Dim P_OBJECT As SqlParameter = SQLcmd.Parameters.Add("@OBJECT", System.Data.SqlDbType.NVarChar, 20)
                Dim P_PERMIT As SqlParameter = SQLcmd.Parameters.Add("@PERMIT", System.Data.SqlDbType.Int, 1)
                Dim P_ORGCODE As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar, 20)
                P_COMPCODE.Value = CAMPCODE
                P_ROLE.Value = ROLECODE
                P_STYMD.Value = STYMD
                P_ENDYMD.Value = ENDYMD
                P_TORITYPE.Value = C_TORITYPE.TYPE_02_OWNER
                P_OBJECT.Value = C_ROLE_VARIANT.USER_ORG
                P_PERMIT.Value = PERMISSION
                P_ORGCODE.Value = ORGCODE

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力編集
                addListData(SQLdr)
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GS0010OWNCODEget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC002_TORIHIKISAKI Select"
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
    ''' 荷主一覧取得
    ''' </summary>
    Protected Sub getOwnerTermList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用荷主取得
        '○ User権限によりDB(S0005_AUTHOR)検索


        Try
            ORGCODE = sm.APSV_ORG

            Dim SQLStr As String = _
                  "   SELECT                                                " _
                & "            rtrim(A.TORICODE)      as CODE            ,  " _
                & "            rtrim(A.NAMES) 	      as NAMES           ,  " _
                & "            rtrim(B.SEQ)           as SEQ                " _
                & "         FROM MC002_TORIHIKISAKI as A 			        " _
                & "   INNER JOIN MC003_TORIORG      as B ON  	            " _
                & "              B.CAMPCODE          = A.CAMPCODE 	    	" _
                & "          and B.TORICODE          = A.TORICODE 	    	" _
                & "          and B.TORITYPE02        = @TORITYPE 			" _
                & "          and B.UORG              = @ORGCODE          	" _
                & "          and B.DELFLG           <> '1' 				    " _
                & "   WHERE                              				    " _
                & "              A.CAMPCODE          = @COMPCODE            " _
                & "          and A.STYMD            <= @ENDYMD 				" _
                & "          and A.ENDYMD           >= @STYMD 				" _
                & "          and A.DELFLG           <> '1' 				    " _
                & "   GROUP BY A.TORICODE , A.NAMES , B.SEQ                 "
            '〇ソート条件追加
            Select DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.TORICODE, A.NAMES , B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES, A.TORICODE , B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ, A.TORICODE , A.NAMES "
                Case Else
            End Select

            Using SQLcmd = New SqlCommand(SQLStr, SQLcon)
                Dim P_COMPCODE As SqlParameter = SQLcmd.Parameters.Add("@COMPCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                Dim P_TORITYPE As SqlParameter = SQLcmd.Parameters.Add("@TORITYPE", System.Data.SqlDbType.NVarChar, 3)
                Dim P_OBJECT As SqlParameter = SQLcmd.Parameters.Add("@OBJECT", System.Data.SqlDbType.NVarChar, 20)
                Dim P_PERMIT As SqlParameter = SQLcmd.Parameters.Add("@PERMIT", System.Data.SqlDbType.Int, 1)
                Dim P_ORGCODE As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar, 20)

                P_COMPCODE.Value = CAMPCODE
                P_STYMD.Value = STYMD
                P_ENDYMD.Value = ENDYMD
                P_TORITYPE.Value = C_TORITYPE.TYPE_02_OWNER
                P_ORGCODE.Value = ORGCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "getOwnerTermList"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC002_TORIHIKISAKI Select"
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
    ''' 庸車一覧取得
    ''' </summary>
    Protected Sub getRideCarList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用荷主取得
        '○ User権限によりDB(S0005_AUTHOR)検索
        Try
            Dim SQLStr As String = String.Empty
            '部署コード未設定時
            If String.IsNullOrEmpty(ORGCODE) Then
                '検索SQL文
                SQLStr =
                                  " SELECT " _
                                & "            rtrim(A.TORICODE)      as CODE       ,    " _
                                & "            rtrim(A.NAMES)         as NAMES      ,    " _
                                & "            rtrim(B.SEQ)           as SEQ             " _
                                & " FROM       MC002_TORIHIKISAKI A                      " _
                                & " INNER JOIN MC003_TORIORG B               ON          " _
                                & "       B.CAMPCODE     = A.CAMPCODE                    " _
                                & "   and B.TORICODE        = A.TORICODE                 " _
                                & "   and B.TORITYPE03      = @TORITYPE                  " _
                                & "   and B.DELFLG         <> '1'                        " _
                                & " INNER JOIN S0006_ROLE C                  ON          " _
                                & "       C.CAMPCODE        = B.CAMPCODE                 " _
                                & "   and C.OBJECT          = @OBJECT                    " _
                                & "   and C.CODE            = B.UORG                     " _
                                & "   and C.ROLE            = @ROLE                      " _
                                & "   and C.PERMITCODE     >= @PERMIT                    " _
                                & "   and C.STYMD          <= @ENDYMD                    " _
                                & "   and C.ENDYMD         >= @STYMD                     " _
                                & "   and C.DELFLG         <> '1'                        " _
                                & " Where                                                " _
                                & "            A.CAMPCODE      = @COMPCODE               " _
                                & "   and A.STYMD             <= @ENDYMD                 " _
                                & "   and A.ENDYMD            >= @STYMD                  " _
                                & "   and A.DELFLG            <> '1'                     " _
                                & " GROUP BY A.TORICODE , A.NAMES , B.SEQ                "

            Else
                SQLStr =
                                " SELECT " _
                                & "            rtrim(A.TORICODE)      as CODE       ,    " _
                                & "            rtrim(A.NAMES)         as NAMES      ,    " _
                                & "            rtrim(B.SEQ)           as SEQ             " _
                                & " FROM       MC002_TORIHIKISAKI       A                " _
                                & " INNER JOIN MC003_TORIORG            B             ON " _
                                & "       B.CAMPCODE      = A.CAMPCODE                   " _
                                & "   and B.TORICODE      = A.TORICODE                   " _
                                & "   and B.TORITYPE03    = @TORITYPE                    " _
                                & "   and B.UORG　　      = @ORGCODE                     " _
                                & "   and B.DELFLG       <> '1'                          " _
                                & " INNER JOIN S0006_ROLE C                           ON " _
                                & "       C.CAMPCODE      = B.CAMPCODE                   " _
                                & "   and C.OBJECT        = @OBJECT                      " _
                                & "   and C.CODE          = B.UORG                       " _
                                & "   and C.ROLE          = @ROLE                        " _
                                & "   and C.PERMITCODE   >= @PERMIT                      " _
                                & "   and C.STYMD        <= @ENDYMD                      " _
                                & "   and C.ENDYMD       >= @STYMD                       " _
                                & "   and C.DELFLG       <> '1'                          " _
                                & " Where                                                " _
                                & "       A.CAMPCODE      = @COMPCODE                    " _
                                & "   and A.STYMD        <= @ENDYMD                      " _
                                & "   and A.ENDYMD       >= @STYMD                       " _
                                & "   and A.DELFLG       <> '1'                          " _
                                & " GROUP BY A.TORICODE , A.NAMES , B.SEQ                "
            End If
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.TORICODE, A.NAMES , B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES, A.TORICODE , B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ, A.TORICODE , A.NAMES "
                Case Else
            End Select

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)

                Dim P_COMPCODE As SqlParameter = SQLcmd.Parameters.Add("@COMPCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_ROLE As SqlParameter = SQLcmd.Parameters.Add("@ROLE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                Dim P_TORITYPE As SqlParameter = SQLcmd.Parameters.Add("@TORITYPE", System.Data.SqlDbType.NVarChar, 3)
                Dim P_OBJECT As SqlParameter = SQLcmd.Parameters.Add("@OBJECT", System.Data.SqlDbType.NVarChar, 20)
                Dim P_PERMIT As SqlParameter = SQLcmd.Parameters.Add("@PERMIT", System.Data.SqlDbType.Int, 1)
                Dim P_ORGCODE As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar, 20)
                P_COMPCODE.Value = CAMPCODE
                P_ROLE.Value = ROLECODE
                P_STYMD.Value = STYMD
                P_ENDYMD.Value = ENDYMD
                P_TORITYPE.Value = C_TORITYPE.TYPE_03_RIDECAR
                P_OBJECT.Value = C_ROLE_VARIANT.USER_ORG
                P_PERMIT.Value = PERMISSION
                P_ORGCODE.Value = ORGCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力編集
                addListData(SQLdr)
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GS0010OWNCODEget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC002_TORIHIKISAKI Select"
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
    ''' 庸車一覧取得
    ''' </summary>
    Protected Sub getRideCarTermList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用荷主取得
        '○ User権限によりDB(S0005_AUTHOR)検索


        Try
            ORGCODE = sm.APSV_ORG

            Dim SQLStr As String = _
                  "   SELECT " _
                & "            rtrim(A.TORICODE)      as CODE         , " _
                & "            rtrim(A.NAMES)         as NAMES        , " _
                & "            rtrim(B.SEQ)           as SEQ            " _
                & "         FROM MC002_TORIHIKISAKI as A 			    " _
                & "   INNER JOIN MC003_TORIORG      as B ON  	        " _
                & "              B.CAMPCODE          = A.CAMPCODE       " _
                & "              B.TORICODE          = A.TORICODE 		" _
                & "          and B.TORITYPE03        = @TORITYPE 		" _
                & "          and B.UORG              = @ORGCODE         " _
                & "          and B.DELFLG           <> '1' 				" _
                & "   WHERE                              				" _
                & "              A.CAMPCODE          = @COMPCODE        " _
                & "          and A.STYMD            <= @ENDYMD 			" _
                & "          and A.ENDYMD           >= @STYMD 			" _
                & "          and A.DELFLG           <> '1' 				" _
                & "   GROUP BY A.TORICODE , A.NAMES , B.SEQ             "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.TORICODE, A.NAMES , B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES, A.TORICODE , B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ, A.TORICODE , A.NAMES "
                Case Else
            End Select

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)
                Dim P_COMPCODE As SqlParameter = SQLcmd.Parameters.Add("@COMPCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                Dim P_TORITYPE As SqlParameter = SQLcmd.Parameters.Add("@TORITYPE", System.Data.SqlDbType.NVarChar, 3)
                Dim P_ORGCODE As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar, 20)

                P_COMPCODE.Value = CAMPCODE
                P_STYMD.Value = STYMD
                P_ENDYMD.Value = ENDYMD
                P_TORITYPE.Value = C_TORITYPE.TYPE_03_RIDECAR
                P_ORGCODE.Value = ORGCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力編集
                addListData(SQLdr)
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GS0010OWNCODEget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC002_TORIHIKISAKI Select"
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


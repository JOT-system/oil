Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 車両情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0007CarList
    Inherits GL0000
    ''' <summary>
    ''' 前方車両コード
    ''' </summary>
    Private Const C_FRONT_CODE As String = "前"
    ''' <summary>
    ''' 後方車両コード
    ''' </summary>
    Private Const C_REAR_CODE As String = "後"
    ''' <summary>
    ''' 取得条件
    ''' </summary>
    Public Enum LC_LORRY_TYPE
        ''' <summary>
        ''' 全取得
        ''' </summary>
        ALL
        ''' <summary>
        ''' 前方車両
        ''' </summary>
        FRONT
        ''' <summary>
        ''' 後方車両
        ''' </summary>
        REAR

    End Enum

    ''' <summary>
    '''  取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TYPE() As LC_LORRY_TYPE
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
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
    ''' 部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As String
    ''' <summary>
    ''' テーブル表示エリア
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AREA() As Object
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

        'PARAM EXTRA01: ORGCODE
        If IsNothing(ORGCODE) Then
            ORGCODE = ""
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
            Case LC_LORRY_TYPE.FRONT
                getFrontLorryList(SQLcon)
            Case LC_LORRY_TYPE.REAR
                getRearLorryList(SQLcon)
            Case Else
                getLorryList(SQLcon)
        End Select

        SQLcon.Close() 'DataBase接続(Close)
        SQLcon.Dispose()
        SQLcon = Nothing

    End Sub

    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getTable()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理


        'PARAM 01: TYPE
        If checkParam(METHOD_NAME, TYPE) Then
            Exit Sub
        End If

        'PARAM EXTRA01: ORGCODE
        If IsNothing(ORGCODE) Then
            ORGCODE = ""
        End If
        'PARAM EXTRA02: STYMD
        If STYMD < C_DEFAULT_YMD Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
            ENDYMD = Date.Now
        End If

        If IsNothing(AREA) Then
            Exit Sub
        End If
        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Select Case TYPE
            Case LC_LORRY_TYPE.FRONT
                getFrontLorryTbl(SQLcon)
            Case LC_LORRY_TYPE.REAR
                getRearLorryTbl(SQLcon)
            Case Else
                getLorryTbl(SQLcon)
        End Select


        SQLcon.Close() 'DataBase接続(Close)
        SQLcon.Dispose()
        SQLcon = Nothing

    End Sub
    ''' <summary>
    ''' 一覧登録時の追加チェック処理
    ''' </summary>
    ''' <param name="I_SQLDR"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overloads Function extracheck(ByVal I_SQLDR As SqlDataReader)
        Return String.IsNullOrEmpty(Me.ORGCODE) OrElse ORGCODE = I_SQLDR("MANGSORG")
    End Function

    ''' <summary>
    ''' 統一車番取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Sub getLorryList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用車両取得
        '○ User権限によりDB(MA003_SHARYOB)検索
        Try

            '検索SQL文
            Dim SQLStr As String = _
                 " SELECT DISTINCT                                                  " _
               & " isnull(rtrim(B.SHARYOTYPE),'') +                  		        " _
               & "        isnull(rtrim(B.TSHABAN),'') 	    as CODE ,               " _
               & "        isnull(rtrim(A.MANGSORG),'') 	    as MANGSORG ,           " _
               & "        isnull(rtrim(C.LICNPLTNO1),'') +                          " _
               & "        isnull(rtrim(C.LICNPLTNO2),'')    as NAMES                " _
               & " FROM       MA003_SHARYOB                   B                     " _
               & " INNER JOIN MA002_SHARYOA                   A                  ON " _
               & "            A.CAMPCODE   	= B.CAMPCODE 				            " _
               & "        and A.SHARYOTYPE  = B.SHARYOTYPE  		                " _
               & "        and A.TSHABAN     = B.TSHABAN 		                    " _
               & "        and A.STYMD      <= @P3                                   " _
               & "        and A.ENDYMD     >= @P2                                   " _
               & "        and A.DELFLG     <> '1' 						            " _
               & " LEFT  JOIN MA004_SHARYOC                   C                  ON " _
               & "            C.CAMPCODE    = A.CAMPCODE 				            " _
               & "        and C.SHARYOTYPE  = A.SHARYOTYPE 		                    " _
               & "        and C.TSHABAN     = A.TSHABAN 			                " _
               & "        and C.STYMD      <= @P3                                   " _
               & "        and C.ENDYMD     >= @P2                                   " _
               & "        and C.DELFLG     <> '1' 						            " _
               & " WHERE      B.CAMPCODE    = @P1                                   " _
               & "        and B.STYMD      <= @P3                                   " _
               & "        and B.ENDYMD     >= @P2                                   " _
               & "        and B.DELFLG     <> '1'                                   "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY CODE, MANGSORG , NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY NAMES,  CODE, MANGSORG "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY CODE, MANGSORG , NAMES "
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力編集
                addListData(SQLdr)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA003_SHARYOB Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 統一車番取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Sub getLorryTbl(ByVal SQLcon As SqlConnection)
        '●Leftボックス用車両取得
        '○ User権限によりDB(MA003_SHARYOB)検索
        Try

            Dim profTbl As String(,) = { _
                      {"TSHABAN", "統一車番", "10"} _
                    , {"LICNPLTNO", "登録車番", "8"} _
                    , {"MANGSORG", "設置部署", "6"} _
                    , {"MANGSORGNAME", "設置部署名称", "8"}
            }
            '検索SQL文
            Dim SQLStr As String = _
                 " SELECT isnull(rtrim(A.SHARYOTYPE),'') +                  		" _
               & "        isnull(rtrim(A.TSHABAN),'') 	    as TSHABAN ,            " _
               & "        isnull(rtrim(B.MANGSORG),'') 	    as MANGSORG ,           " _
               & "        isnull(rtrim(E.NAMES),'') 	    as MANGSORGNAME ,       " _
               & "        isnull(rtrim(D.LICNPLTNO1),'') +                          " _
               & "        isnull(rtrim(D.LICNPLTNO2),'')    as LICNPLTNO            " _
               & " FROM       MA003_SHARYOB                   A                     " _
               & " INNER JOIN MA002_SHARYOA                   B                  ON " _
               & "            B.CAMPCODE   	= A.CAMPCODE 				            " _
               & "        and B.SHARYOTYPE  = A.SHARYOTYPE  		                " _
               & "        and B.TSHABAN     = A.TSHABAN 		                    " _
               & "        and B.STYMD      <= @P3                                   " _
               & "        and B.ENDYMD     >= @P2                                   " _
               & "        and B.DELFLG     <> '1' 						            " _
               & " LEFT  JOIN MA004_SHARYOC                   D                  ON " _
               & "            D.CAMPCODE    = A.CAMPCODE 				            " _
               & "        and D.SHARYOTYPE  = A.SHARYOTYPE 		                    " _
               & "        and D.TSHABAN     = A.TSHABAN 			                " _
               & "        and D.STYMD      <= @P3                                   " _
               & "        and D.ENDYMD     >= @P2                                   " _
               & "        and D.DELFLG     <> '1' 						            " _
               & " LEFT JOIN  M0002_ORG                       E                  ON " _
               & "            E.CAMPCODE    = A.CAMPCODE                            " _
               & "        and E.ORGCODE     = B.MANGSORG                            " _
               & "        and E.STYMD      <= @P3                                   " _
               & "        and E.ENDYMD     >= @P2                                   " _
               & "        and E.DELFLG     <> '1'                                   " _
               & " WHERE      A.CAMPCODE    = @P1                                   " _
               & "        and A.DELFLG     <> '1'                                   " _
               & "        and A.STYMD      <= @P3                                   " _
               & "        and A.ENDYMD     >= @P2                                   " _
               & " ORDER BY   A.SHARYOTYPE ,A.TSHABAN                               "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            PARA1.Value = CAMPCODE
            PARA2.Value = STYMD
            PARA3.Value = ENDYMD
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
            Dim srcData As New DataTable
            srcData.Load(SQLdr)

            MakeTableObject(profTbl, srcData, AREA)
            ERR = C_MESSAGE_NO.NORMAL
            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA003_SHARYOB Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 統一車番(前)取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Sub getFrontLorryList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用車両取得
        '○ User権限によりDB(MA003_SHARYOB)検索
        Try

            '検索SQL文
            Dim SQLStr As String = _
                 " SELECT DISTINCT                                                  " _
               & "        isnull(rtrim(B.SHARYOTYPE),'') +                  		" _
               & "        isnull(rtrim(B.TSHABAN),'') 	    as CODE ,               " _
               & "        isnull(rtrim(A.MANGSORG),'') 	    as MANGSORG ,           " _
               & "        isnull(rtrim(C.LICNPLTNO1),'') +                          " _
               & "        isnull(rtrim(C.LICNPLTNO2),'')    as NAMES                " _
               & " FROM       MA003_SHARYOB                   B                     " _
               & " INNER JOIN MA002_SHARYOA                   A                  ON " _
               & "            A.CAMPCODE   	= B.CAMPCODE 				            " _
               & "        and A.SHARYOTYPE  = B.SHARYOTYPE  		                " _
               & "        and A.TSHABAN     = B.TSHABAN 		                    " _
               & "        and A.STYMD      <= @P3                                   " _
               & "        and A.ENDYMD     >= @P2                                   " _
               & "        and A.DELFLG     <> '1' 						            " _
               & " LEFT  JOIN MA004_SHARYOC                   C                  ON " _
               & "            C.CAMPCODE    = A.CAMPCODE 				            " _
               & "        and C.SHARYOTYPE  = A.SHARYOTYPE 		                    " _
               & "        and C.TSHABAN     = A.TSHABAN 			                " _
               & "        and C.STYMD      <= @P3                                   " _
               & "        and C.ENDYMD     >= @P2                                   " _
               & "        and C.DELFLG     <> '1' 						            " _
               & " WHERE      B.CAMPCODE    = @P1                                   " _
               & "        and B.SHARYOTYPE IN (                                     " _
               & "           SELECT RTRIM(S.KEYCODE)                                " _
               & "           FROM   MC001_FIXVALUE                S                 " _
               & "           WHERE  S.VALUE2         = @P4                          " _
               & "             and  S.CLASS          = 'SHARYOTYPE'                 " _
               & "             and  S.CAMPCODE       = A.CAMPCODE                   " _
               & "             and  S.STYMD         <= @P3                          " _
               & "             and  S.ENDYMD        >= @P2                          " _
               & "             and  S.DELFLG        <> '1'                          " _
               & "           )                                                      " _
               & "        and B.DELFLG     <> '1'                                   " _
               & "        and B.STYMD      <= @P3                                   " _
               & "        and B.ENDYMD     >= @P2                                   "

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY CODE, MANGSORG , NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY NAMES,  CODE, MANGSORG "
                Case C_DEFAULT_SORT.SEQ
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = C_FRONT_CODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力編集
                addListData(SQLdr)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA003_SHARYOB Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 統一車番(前)取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Sub getFrontLorryTbl(ByVal SQLcon As SqlConnection)
        '●Leftボックス用車両取得
        '○ User権限によりDB(MA003_SHARYOB)検索
        Try

            Dim profTbl As String(,) = { _
                      {"TSHABAN", "統一車番", "20"} _
                    , {"LICNPLTNO", "登録車番", "8"} _
                    , {"MANGSORG", "設置部署", "6"} _
                    , {"MANGSORGNAME", "設置部署名称", "8"} _
            }
            '検索SQL文
            Dim SQLStr As String = _
                 " SELECT isnull(rtrim(A.SHARYOTYPE),'') +                  		" _
               & "        isnull(rtrim(A.TSHABAN),'') 	    as TSHABAN ,            " _
               & "        isnull(rtrim(B.MANGSORG),'') 	    as MANGSORG ,           " _
               & "        isnull(rtrim(E.NAMES),'') 	    as MANGSORGNAME ,       " _
               & "        isnull(rtrim(D.LICNPLTNO1),'') +                          " _
               & "        isnull(rtrim(D.LICNPLTNO2),'')    as LICNPLTNO            " _
               & " FROM       MA003_SHARYOB                   A                     " _
               & " INNER JOIN MA002_SHARYOA                   B                  ON " _
               & "            B.CAMPCODE   	= A.CAMPCODE 				            " _
               & "        and B.SHARYOTYPE  = A.SHARYOTYPE  		                " _
               & "        and B.TSHABAN     = A.TSHABAN 		                    " _
               & "        and B.STYMD      <= @P3                                   " _
               & "        and B.ENDYMD     >= @P2                                   " _
               & "        and B.DELFLG     <> '1' 						            " _
               & " LEFT  JOIN MA004_SHARYOC                   D                  ON " _
               & "            D.CAMPCODE    = A.CAMPCODE 				            " _
               & "        and D.SHARYOTYPE  = A.SHARYOTYPE 		                    " _
               & "        and D.TSHABAN     = A.TSHABAN 			                " _
               & "        and D.STYMD      <= @P3                                   " _
               & "        and D.ENDYMD     >= @P2                                   " _
               & "        and D.DELFLG     <> '1' 						            " _
               & " LEFT JOIN  M0002_ORG                       E                  ON " _
               & "            E.CAMPCODE    = A.CAMPCODE                            " _
               & "        and E.ORGCODE     = B.MANGSORG                            " _
               & "        and E.STYMD      <= @P3                                   " _
               & "        and E.ENDYMD     >= @P2                                   " _
               & "        and E.DELFLG     <> '1'                                   " _
               & " WHERE      A.CAMPCODE    = @P1                                   " _
               & "        and A.SHARYOTYPE IN (                                     " _
               & "           SELECT RTRIM(S.KEYCODE)                                " _
               & "           FROM   MC001_FIXVALUE                S                 " _
               & "           WHERE  S.VALUE2         = @P4                          " _
               & "             and  S.CLASS          = 'SHARYOTYPE'                 " _
               & "             and  S.CAMPCODE       = A.CAMPCODE                   " _
               & "             and  S.STYMD         <= @P3                          " _
               & "             and  S.ENDYMD        >= @P2                          " _
               & "             and  S.DELFLG        <> '1'                          " _
               & "           )                                                      " _
               & "        and A.DELFLG     <> '1'                                   " _
               & "        and A.STYMD      <= @P3                                   " _
               & "        and A.ENDYMD     >= @P2                                   " _
               & " ORDER BY   A.SHARYOTYPE ,A.TSHABAN                               "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
            PARA1.Value = CAMPCODE
            PARA2.Value = STYMD
            PARA3.Value = ENDYMD
            PARA4.Value = C_FRONT_CODE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
            Dim srcData As New DataTable
            srcData.Load(SQLdr)

            MakeTableObject(profTbl, srcData, AREA)
            ERR = C_MESSAGE_NO.NORMAL
            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA003_SHARYOB Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 統一車番(前)取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Sub getRearLorryList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用車両取得
        '○ User権限によりDB(MA003_SHARYOB)検索
        Try

            '検索SQL文
            Dim SQLStr As String = _
                 " SELECT DISTINCT                                                  " _
               & "        isnull(rtrim(B.SHARYOTYPE),'') +                  		" _
               & "        isnull(rtrim(B.TSHABAN),'') 	    as CODE ,               " _
               & "        isnull(rtrim(A.MANGSORG),'') 	    as MANGSORG ,           " _
               & "        isnull(rtrim(C.LICNPLTNO1),'') +                          " _
               & "        isnull(rtrim(C.LICNPLTNO2),'')    as NAMES                " _
               & " FROM       MA003_SHARYOB                   B                     " _
               & " INNER JOIN MA002_SHARYOA                   A                  ON " _
               & "            A.CAMPCODE   	= B.CAMPCODE 				            " _
               & "        and A.SHARYOTYPE  = B.SHARYOTYPE  		                " _
               & "        and A.TSHABAN     = B.TSHABAN 		                    " _
               & "        and A.STYMD      <= @P3                                   " _
               & "        and A.ENDYMD     >= @P2                                   " _
               & "        and A.DELFLG     <> '1' 						            " _
               & " LEFT  JOIN MA004_SHARYOC                   C                  ON " _
               & "            C.CAMPCODE    = A.CAMPCODE 				            " _
               & "        and C.SHARYOTYPE  = A.SHARYOTYPE 		                    " _
               & "        and C.TSHABAN     = A.TSHABAN 			                " _
               & "        and C.STYMD      <= @P3                                   " _
               & "        and C.ENDYMD     >= @P2                                   " _
               & "        and C.DELFLG     <> '1' 						            " _
               & " WHERE      B.CAMPCODE    = @P1                                   " _
               & "        and B.SHARYOTYPE IN (                                     " _
               & "           SELECT RTRIM(S.KEYCODE)                                " _
               & "           FROM   MC001_FIXVALUE                S                 " _
               & "           WHERE  S.VALUE2         = @P4                          " _
               & "             and  S.CLASS          = 'SHARYOTYPE'                 " _
               & "             and  S.CAMPCODE       = A.CAMPCODE                   " _
               & "             and  S.STYMD         <= @P3                          " _
               & "             and  S.ENDYMD        >= @P2                          " _
               & "             and  S.DELFLG        <> '1'                          " _
               & "           )                                                      " _
               & "        and B.DELFLG     <> '1'                                   " _
               & "        and B.STYMD      <= @P3                                   " _
               & "        and B.ENDYMD     >= @P2                                   "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY CODE, MANGSORG , NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY NAMES,  CODE, MANGSORG "
                Case C_DEFAULT_SORT.SEQ
                Case Else
            End Select
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = C_REAR_CODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA003_SHARYOB Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 統一車番(後)取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Sub getRearLorryTbl(ByVal SQLcon As SqlConnection)
        '●Leftボックス用車両取得
        '○ User権限によりDB(MA003_SHARYOB)検索
        Try

            Dim profTbl As String(,) = { _
                      {"TSHABAN", "統一車番", "20"} _
                    , {"LICNPLTNO", "登録車番", "8"} _
                    , {"MANGSORG", "設置部署", "6"} _
                    , {"MANGSORGNAME", "設置部署名称", "8"} _
            }
            '検索SQL文
            Dim SQLStr As String = _
                 " SELECT isnull(rtrim(A.SHARYOTYPE),'') +                  		" _
               & "        isnull(rtrim(A.TSHABAN),'') 	    as TSHABAN ,            " _
               & "        isnull(rtrim(B.MANGSORG),'') 	    as MANGSORG ,           " _
               & "        isnull(rtrim(E.NAMES),'') 	    as MANGSORGNAME ,       " _
               & "        isnull(rtrim(D.LICNPLTNO1),'') +                          " _
               & "        isnull(rtrim(D.LICNPLTNO2),'')    as LICNPLTNO            " _
               & " FROM       MA003_SHARYOB                   A                     " _
               & " INNER JOIN MA002_SHARYOA                   B                  ON " _
               & "            B.CAMPCODE   	= A.CAMPCODE 				            " _
               & "        and B.SHARYOTYPE  = A.SHARYOTYPE  		                " _
               & "        and B.TSHABAN     = A.TSHABAN 		                    " _
               & "        and B.STYMD      <= @P3                                   " _
               & "        and B.ENDYMD     >= @P2                                   " _
               & "        and B.DELFLG     <> '1' 						            " _
               & " LEFT  JOIN MA004_SHARYOC                   D                  ON " _
               & "            D.CAMPCODE    = A.CAMPCODE 				            " _
               & "        and D.SHARYOTYPE  = A.SHARYOTYPE 		                    " _
               & "        and D.TSHABAN     = A.TSHABAN 			                " _
               & "        and D.STYMD      <= @P3                                   " _
               & "        and D.ENDYMD     >= @P2                                   " _
               & "        and D.DELFLG     <> '1' 						            " _
               & " LEFT JOIN  M0002_ORG                       E                  ON " _
               & "            E.CAMPCODE    = A.CAMPCODE                            " _
               & "        and E.ORGCODE     = B.MANGSORG                            " _
               & "        and E.STYMD      <= @P3                                   " _
               & "        and E.ENDYMD     >= @P2                                   " _
               & "        and E.DELFLG     <> '1'                                   " _
               & " WHERE      A.CAMPCODE    = @P1                                   " _
               & "        and A.SHARYOTYPE IN (                                     " _
               & "           SELECT RTRIM(S.KEYCODE)                                " _
               & "           FROM   MC001_FIXVALUE                S                 " _
               & "           WHERE  S.VALUE2         = @P4                          " _
               & "             and  S.CLASS          = 'SHARYOTYPE'                 " _
               & "             and  S.CAMPCODE       = A.CAMPCODE                   " _
               & "             and  S.STYMD         <= @P3                          " _
               & "             and  S.ENDYMD        >= @P2                          " _
               & "             and  S.DELFLG        <> '1'                          " _
               & "           )                                                      " _
               & "        and A.DELFLG     <> '1'                                   " _
               & "        and A.STYMD      <= @P3                                   " _
               & "        and A.ENDYMD     >= @P2                                   " _
               & " ORDER BY   A.SHARYOTYPE ,A.TSHABAN                               "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
            PARA1.Value = CAMPCODE
            PARA2.Value = STYMD
            PARA3.Value = ENDYMD
            PARA4.Value = C_REAR_CODE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
            Dim srcData As New DataTable
            srcData.Load(SQLdr)

            MakeTableObject(profTbl, srcData, AREA)
            ERR = C_MESSAGE_NO.NORMAL
            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA003_SHARYOB Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
End Class


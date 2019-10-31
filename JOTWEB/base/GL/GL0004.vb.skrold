Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 届先情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0004DestinationList
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
        ''' JX,COSMO
        ''' </summary>
        JXCOSMO
    End Enum
    ''' <summary>
    ''' 取引先タイプ一覧
    ''' </summary>
    ''' <remarks></remarks>
    Protected Class C_TORITYPE
        Public Const TYPE_01_GROUP As String = "02"
        Public Const TYPE_02_OWNER As String = "NI"
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
    ''' 取引先コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String
    ''' <summary>
    ''' 分類コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLASSCODE() As String
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

        'PARAM EXTRA02: CLASSCODE
        If IsNothing(CLASSCODE) Then
            CLASSCODE = ""
        End If
        'PARAM EXTRA03: TORICODE
        If IsNothing(TORICODE) Then
            TORICODE = ""
        End If
        'PARAM EXTRA03: ORGCODE
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
        Using SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Select Case TYPE
                Case LC_CUSTOMER_TYPE.ALL
                    getDistinationList(SQLcon)

                Case LC_CUSTOMER_TYPE.WITHTERM
                    getDistinationTermList(SQLcon)

                Case LC_CUSTOMER_TYPE.JXCOSMO
                    getDistinationJXCOSMOList(SQLcon)

            End Select

        End Using

    End Sub
    ''' <summary>
    ''' 届先一覧取得
    ''' </summary>
    Protected Sub getDistinationList(ByVal SQLcon As SqlConnection)

        Try
            Dim SQLStr As String
            '●Leftボックス用届先取得
            If String.IsNullOrEmpty(CLASSCODE) Then          '[分類]に値が存在しない場合
                If String.IsNullOrEmpty(TORICODE) Then           '[取引先コード]に値が存在しない場合
                    SQLStr =
                          "  SELECT                                       " _
                        & "         rtrim(A.TODOKECODE) as CODE ,         " _
                        & "         rtrim(A.NAMES)      as NAMES          " _
                        & "    FROM OIL.MC006_TODOKESAKI    A             " _
                        & "   Where A.CAMPCODE     = @P1                  " _
                        & "     and substring(A.TODOKECODE,1,2)  <> 'JX'  " _
                        & "     and substring(A.TODOKECODE,1,5)  <> 'COSMO'  " _
                        & "     and A.STYMD       <= @P3                  " _
                        & "     and A.ENDYMD      >= @P2                  " _
                        & "     and A.DELFLG      <> '1'                  "
                Else                            '[取引先コード]に値が存在する場合
                    SQLStr =
                          "  SELECT                                       " _
                        & "         rtrim(A.TODOKECODE) as CODE  ,        " _
                        & "         rtrim(A.NAMES)      as NAMES          " _
                        & "    FROM OIL.MC006_TODOKESAKI    A             " _
                        & "   Where A.CAMPCODE     = @P1                  " _
                        & "     and A.TORICODE     = @P4                  " _
                        & "     and substring(A.TODOKECODE,1,2)  <> 'JX'  " _
                        & "     and substring(A.TODOKECODE,1,5)  <> 'COSMO'  " _
                        & "     and A.STYMD       <= @P3                  " _
                        & "     and A.ENDYMD      >= @P2                  " _
                        & "     and A.DELFLG      <> '1'                  "
                End If
                '〇ソート条件追加
                Select Case DEFAULT_SORT
                    Case C_DEFAULT_SORT.CODE, String.Empty
                        SQLStr = SQLStr & " ORDER BY A.TODOKECODE, A.NAMES "
                    Case C_DEFAULT_SORT.NAMES
                        SQLStr = SQLStr & " ORDER BY A.NAMES, A.TODOKECODE "
                    Case C_DEFAULT_SORT.SEQ
                        SQLStr = SQLStr & " ORDER BY A.TODOKECODE, A.NAMES "
                    Case Else
                End Select
            Else                              '[分類]に値が存在する場合
                If String.IsNullOrEmpty(ORGCODE) Then            '[組織CODE]に値が存在しない場合、
                    If String.IsNullOrEmpty(TORICODE) Then        '[取引先コード]に値が存在しない場合
                        SQLStr =
                              "  SELECT rtrim(A.TODOKECODE) as CODE ,       " _
                            & "         rtrim(A.NAMES)      as NAMES ,      " _
                            & "         rtrim(A.ADDR1) +                    " _
                            & "         rtrim(A.ADDR2) +                    " _
                            & "         rtrim(A.ADDR3) +                    " _
                            & "         rtrim(A.ADDR4)      as ADDR ,       " _
                            & "         rtrim(A.NOTES1)     as NOTES1 ,     " _
                            & "         rtrim(A.NOTES2)     as NOTES2 ,     " _
                            & "         rtrim(A.NOTES3)     as NOTES3 ,     " _
                            & "         rtrim(A.NOTES4)     as NOTES4 ,     " _
                            & "         rtrim(A.NOTES5)     as NOTES5 ,     " _
                            & "         rtrim(B.ARRIVTIME)  as ARRIVTIME ,  " _
                            & "         rtrim(B.DISTANCE)   as DISTANCE     " _
                            & "    FROM OIL.MC006_TODOKESAKI A              " _
                            & "   INNER JOIN OIL.MC007_TODKORG B ON         " _
                            & "         B.CAMPCODE      = A.CAMPCODE        " _
                            & "     and B.TORICODE 　　 = A.TORICODE        " _
                            & "     and B.TODOKECODE 　 = A.TODOKECODE      " _
                            & "     and B.DELFLG       <> '1'               " _
                            & "   INNER JOIN COM.OIS0009_ROLE C               " _
                            & "      ON C.CAMPCODE      = B.CAMPCODE        " _
                            & "     and C.CODE          = B.UORG            " _
                            & "     and C.OBJECT        = @P6               " _
                            & "     and C.ROLE          = @P7               " _
                            & "     and C.PERMITCODE   >= @P8               " _
                            & "     and C.STYMD        <= @P3               " _
                            & "     and C.ENDYMD       >= @P2               " _
                            & "     and C.DELFLG       <> '1'               " _
                            & "   Where                                     " _
                            & "         A.CAMPCODE      = @P1               " _
                            & "     and A.CLASS 　     IN (@P5,'')          " _
                            & "     and A.STYMD        <= @P3               " _
                            & "     and A.ENDYMD       >= @P2               " _
                            & "     and A.DELFLG       <> '1'               "
                    Else                         '[取引先コード]に値が存在する場合
                        SQLStr =
                              "  SELECT rtrim(A.TODOKECODE) as CODE ,       " _
                            & "         rtrim(A.NAMES)      as NAMES ,      " _
                            & "         rtrim(A.ADDR1) +                    " _
                            & "         rtrim(A.ADDR2) +                    " _
                            & "         rtrim(A.ADDR3) +                    " _
                            & "         rtrim(A.ADDR4)      as ADDR ,       " _
                            & "         rtrim(A.NOTES1)     as NOTES1 ,     " _
                            & "         rtrim(A.NOTES2)     as NOTES2 ,     " _
                            & "         rtrim(A.NOTES3)     as NOTES3 ,     " _
                            & "         rtrim(A.NOTES4)     as NOTES4 ,     " _
                            & "         rtrim(A.NOTES5)     as NOTES5 ,     " _
                            & "         rtrim(B.ARRIVTIME)  as ARRIVTIME ,  " _
                            & "         rtrim(B.DISTANCE)   as DISTANCE     " _
                            & "   INNER JOIN MC007_TODKORG B ON             " _
                            & "         B.CAMPCODE      = A.CAMPCODE        " _
                            & "     and B.TORICODE 　　 = A.TORICODE        " _
                            & "     and B.TODOKECODE 　 = A.TODOKECODE      " _
                            & "     and B.DELFLG       <> '1'               " _
                            & "   INNER JOIN OIS0009_ROLE C                   " _
                            & "      ON C.CAMPCODE      = B.CAMPCODE        " _
                            & "     and C.CODE          = B.UORG            " _
                            & "     and C.OBJECT        = @P6               " _
                            & "     and C.ROLE          = @P7               " _
                            & "     and C.PERMITCODE   >= @P8               " _
                            & "     and C.STYMD        <= @P3               " _
                            & "     and C.ENDYMD       >= @P2               " _
                            & "     and C.DELFLG       <> '1'               " _
                            & "   Where                                     " _
                            & "         A.CAMPCODE      = @P1               " _
                            & "     and A.TORICODE      = @P4               " _
                            & "     and A.CLASS 　     IN (@P5,'')          " _
                            & "     and A.STYMD        <= @P3               " _
                            & "     and A.ENDYMD       >= @P2               " _
                            & "     and A.DELFLG       <> '1'               "
                    End If
                Else                            '[組織CODE]に値が存在する場合、
                    If TORICODE = "" Then        '[取引先コード]に値が存在しない場合
                        SQLStr =
                              "  SELECT rtrim(A.TODOKECODE) as CODE  ,      " _
                            & "         rtrim(A.NAMES)      as NAMES ,      " _
                            & "         rtrim(A.ADDR1) +                    " _
                            & "         rtrim(A.ADDR2) +                    " _
                            & "         rtrim(A.ADDR3) +                    " _
                            & "         rtrim(A.ADDR4)      as ADDR ,       " _
                            & "         rtrim(A.NOTES1)     as NOTES1 ,     " _
                            & "         rtrim(A.NOTES2)     as NOTES2 ,     " _
                            & "         rtrim(A.NOTES3)     as NOTES3 ,     " _
                            & "         rtrim(A.NOTES4)     as NOTES4 ,     " _
                            & "         rtrim(A.NOTES5)     as NOTES5 ,     " _
                            & "         rtrim(B.ARRIVTIME)  as ARRIVTIME ,  " _
                            & "         rtrim(B.DISTANCE)   as DISTANCE     " _
                            & "    FROM OIL.MC006_TODOKESAKI A              " _
                            & "   INNER JOIN OIL.MC007_TODKORG B ON         " _
                            & "         B.CAMPCODE      = A.CAMPCODE        " _
                            & "     and B.TORICODE 　　 = A.TORICODE        " _
                            & "     and B.TODOKECODE 　 = A.TODOKECODE      " _
                            & "     and B.UORG          = @P9               " _
                            & "     and B.DELFLG       <> '1'               " _
                            & "   INNER JOIN COM.OIS0009_ROLE C               " _
                            & "      ON C.CAMPCODE      = B.CAMPCODE        " _
                            & "     and C.CODE          = B.UORG            " _
                            & "     and C.OBJECT        = @P6               " _
                            & "     and C.ROLE          = @P7               " _
                            & "     and C.PERMITCODE   >= @P8               " _
                            & "     and C.STYMD        <= @P3               " _
                            & "     and C.ENDYMD       >= @P2               " _
                            & "     and C.DELFLG       <> '1'               " _
                            & "   Where                                     " _
                            & "         A.CAMPCODE      = @P1               " _
                            & "     and A.CLASS 　     IN (@P5,'')          " _
                            & "     and A.STYMD        <= @P3               " _
                            & "     and A.ENDYMD       >= @P2               " _
                            & "     and A.DELFLG       <> '1'               "
                    Else                         '[取引先コード]に値が存在する場合
                        SQLStr =
                              "  SELECT rtrim(A.TODOKECODE) as CODE  ,      " _
                            & "         rtrim(A.NAMES)      as NAMES ,      " _
                            & "         rtrim(A.ADDR1) +                    " _
                            & "         rtrim(A.ADDR2) +                    " _
                            & "         rtrim(A.ADDR3) +                    " _
                            & "         rtrim(A.ADDR4)      as ADDR ,       " _
                            & "         rtrim(A.NOTES1)     as NOTES1 ,     " _
                            & "         rtrim(A.NOTES2)     as NOTES2 ,     " _
                            & "         rtrim(A.NOTES3)     as NOTES3 ,     " _
                            & "         rtrim(A.NOTES4)     as NOTES4 ,     " _
                            & "         rtrim(A.NOTES5)     as NOTES5 ,     " _
                            & "         rtrim(B.ARRIVTIME)  as ARRIVTIME ,  " _
                            & "         rtrim(B.DISTANCE)   as DISTANCE     " _
                            & "    FROM OIL.MC006_TODOKESAKI A              " _
                            & "   INNER JOIN OIL.MC007_TODKORG B ON         " _
                            & "         B.CAMPCODE      = A.CAMPCODE        " _
                            & "     and B.TORICODE      = A.TORICODE        " _
                            & "     and B.TODOKECODE    = A.TODOKECODE      " _
                            & "     and B.UORG          = @P9               " _
                            & "     and B.DELFLG       <> '1'               " _
                            & "   INNER JOIN COM.OIS0009_ROLE C               " _
                            & "      ON C.CAMPCODE      = B.CAMPCODE        " _
                            & "     and C.CODE          = B.UORG            " _
                            & "     and C.OBJECT        = @P6               " _
                            & "     and C.ROLE          = @P7               " _
                            & "     and C.PERMITCODE   >= @P8               " _
                            & "     and C.STYMD        <= @P3               " _
                            & "     and C.ENDYMD       >= @P2               " _
                            & "     and C.DELFLG       <> '1'               " _
                            & "   Where                                     " _
                            & "         A.CAMPCODE      = @P1               " _
                            & "     and A.TORICODE      = @P4               " _
                            & "     and A.CLASS        IN (@P5,'')          " _
                            & "     and A.STYMD        <= @P3               " _
                            & "     and A.ENDYMD       >= @P2               " _
                            & "     and A.DELFLG       <> '1'               "
                    End If
                End If
                '〇ソート条件追加
                Select Case DEFAULT_SORT
                    Case C_DEFAULT_SORT.CODE
                        SQLStr = SQLStr & " ORDER BY A.TODOKECODE, A.NAMES ,B.SEQ"
                    Case C_DEFAULT_SORT.NAMES
                        SQLStr = SQLStr & " ORDER BY A.NAMES, A.TODOKECODE ,B.SEQ"
                    Case C_DEFAULT_SORT.SEQ, String.Empty
                        SQLStr = SQLStr & " ORDER BY B.SEQ, A.TODOKECODE, A.NAMES "
                    Case Else
                End Select
            End If


            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.Int, 1)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = TORICODE
                PARA5.Value = CLASSCODE
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = ROLECODE
                PARA8.Value = PERMISSION
                PARA9.Value = ORGCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0004"          'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub

    ''' <summary>
    ''' 届先一覧取得
    ''' </summary>
    Protected Sub getDistinationTermList(ByVal SQLcon As SqlConnection)
        '○ セッション変数（APSRVOrg）に紐付くデータ取得
        ORGCODE = sm.APSV_ORG
        '●Leftボックス用届先取得（APSRVOrg）
        Try
            Dim SQLStr As String
            If CLASSCODE = "" Then '[分類]に値が存在しない場合
                If TORICODE = "" Then '[取引先コード]に値が存在しない場合
                    SQLStr =
                              "       SELECT isnull(rtrim(A.TODOKECODE),'')    as CODE ,       " _
                            & "              isnull(rtrim(A.NAMES),'')         as NAMES ,      " _
                            & "              rtrim(B.SEQ)                      as SEQ   ,      " _
                            & "              isnull(rtrim(A.ADDR1),'') +                       " _
                            & "              isnull(rtrim(A.ADDR2),'') +                       " _
                            & "              isnull(rtrim(A.ADDR3),'') +                       " _
                            & "              isnull(rtrim(A.ADDR4),'')         as ADDR ,       " _
                            & "              isnull(rtrim(A.NOTES1),'')        as NOTES1 ,     " _
                            & "              isnull(rtrim(A.NOTES2),'')        as NOTES2 ,     " _
                            & "              isnull(rtrim(A.NOTES3),'')        as NOTES3 ,     " _
                            & "              isnull(rtrim(A.NOTES4),'')        as NOTES4 ,     " _
                            & "              isnull(rtrim(A.NOTES5),'')        as NOTES5 ,     " _
                            & "              isnull(rtrim(A.NOTES6),'')        as NOTES6 ,     " _
                            & "              isnull(rtrim(A.NOTES7),'')        as NOTES7 ,     " _
                            & "              isnull(rtrim(A.NOTES8),'')        as NOTES8 ,     " _
                            & "              isnull(rtrim(A.NOTES9),'')        as NOTES9 ,     " _
                            & "              isnull(rtrim(A.NOTES10),'')       as NOTES10 ,    " _
                            & "              rtrim(B.ARRIVTIME)                as ARRIVTIME ,  " _
                            & "              isnull(rtrim(B.DISTANCE),'')      as DISTANCE     " _
                            & "         FROM OIL.MC006_TODOKESAKI as A                         " _
                            & "   INNER JOIN OIL.MC007_TODKORG    as B ON                      " _
                            & "              B.CAMPCODE     = A.CAMPCODE                       " _
                            & "          and B.TORICODE     = A.TORICODE                       " _
                            & "          and B.TODOKECODE   = A.TODOKECODE                     " _
                            & "          and B.UORG         = @P9                              " _
                            & "          and B.STYMD       <= @P3                              " _
                            & "          and B.ENDYMD      >= @P2                              " _
                            & "          and B.DELFLG      <> '1'                              " _
                            & "        Where A.CAMPCODE     = @P1                              " _
                            & "          and A.STYMD       <= @P3                              " _
                            & "          and A.ENDYMD      >= @P2                              " _
                            & "          and A.DELFLG      <> '1'                              " _
                            & "     GROUP BY B.SEQ ,  			                               " _
                            & "              A.TODOKECODE ,                                    " _
                            & "              A.NAMES ,                                         " _
                            & "              A.ADDR1 ,                                         " _
                            & "              A.ADDR2 ,                                         " _
                            & "              A.ADDR3 ,                                         " _
                            & "              A.ADDR4 ,                                         " _
                            & "              A.NOTES1 ,                                        " _
                            & "              A.NOTES2 ,                                        " _
                            & "              A.NOTES3 ,                                        " _
                            & "              A.NOTES4 ,                                        " _
                            & "              A.NOTES5 ,                                        " _
                            & "              A.NOTES6 ,                                        " _
                            & "              A.NOTES7 ,                                        " _
                            & "              A.NOTES8 ,                                        " _
                            & "              A.NOTES9 ,                                        " _
                            & "              A.NOTES10 ,                                       " _
                            & "              B.ARRIVTIME ,                                     " _
                            & "              B.DISTANCE                                        "
                Else '[取引先コード]に値が存在する場合
                    SQLStr =
                                "     SELECT isnull(rtrim(A.TODOKECODE),'')    as CODE  ,      " _
                            & "              isnull(rtrim(A.NAMES),'')         as NAMES ,      " _
                            & "              rtrim(B.SEQ)                      as SEQ   ,      " _
                            & "              isnull(rtrim(A.ADDR1),'') +                       " _
                            & "              isnull(rtrim(A.ADDR2),'') +                       " _
                            & "              isnull(rtrim(A.ADDR3),'') +                       " _
                            & "              isnull(rtrim(A.ADDR4),'')         as ADDR ,       " _
                            & "              isnull(rtrim(A.NOTES1),'')        as NOTES1 ,     " _
                            & "              isnull(rtrim(A.NOTES2),'')        as NOTES2 ,     " _
                            & "              isnull(rtrim(A.NOTES3),'')        as NOTES3 ,     " _
                            & "              isnull(rtrim(A.NOTES4),'')        as NOTES4 ,     " _
                            & "              isnull(rtrim(A.NOTES5),'')        as NOTES5 ,     " _
                            & "              isnull(rtrim(A.NOTES6),'')        as NOTES6 ,     " _
                            & "              isnull(rtrim(A.NOTES7),'')        as NOTES7 ,     " _
                            & "              isnull(rtrim(A.NOTES8),'')        as NOTES8 ,     " _
                            & "              isnull(rtrim(A.NOTES9),'')        as NOTES9 ,     " _
                            & "              isnull(rtrim(A.NOTES10),'')       as NOTES10 ,    " _
                            & "              rtrim(B.ARRIVTIME)                as ARRIVTIME ,  " _
                            & "              isnull(rtrim(B.DISTANCE),'')      as DISTANCE     " _
                            & "         FROM OIL.MC006_TODOKESAKI as A                         " _
                            & "   INNER JOIN OIL.MC007_TODKORG    as B ON                      " _
                            & "              B.CAMPCODE     = A.CAMPCODE                       " _
                            & "          and B.TORICODE     = A.TORICODE                       " _
                            & "          and B.TODOKECODE   = A.TODOKECODE                     " _
                            & "          and B.UORG         = @P9                              " _
                            & "          and B.STYMD       <= @P3                              " _
                            & "          and B.ENDYMD      >= @P2                              " _
                            & "          and B.DELFLG      <> '1'                              " _
                            & "        Where A.CAMPCODE     = @P1                              " _
                            & "          and A.TORICODE     = @P4                              " _
                            & "          and A.STYMD       <= @P3                              " _
                            & "          and A.ENDYMD      >= @P2                              " _
                            & "          and A.DELFLG      <> '1'                              " _
                            & "     GROUP BY B.SEQ ,  			                               " _
                            & "              A.TODOKECODE ,                                    " _
                            & "              A.NAMES ,                                         " _
                            & "              A.ADDR1 ,                                         " _
                            & "              A.ADDR2 ,                                         " _
                            & "              A.ADDR3 ,                                         " _
                            & "              A.ADDR4 ,                                         " _
                            & "              A.NOTES1 ,                                        " _
                            & "              A.NOTES2 ,                                        " _
                            & "              A.NOTES3 ,                                        " _
                            & "              A.NOTES4 ,                                        " _
                            & "              A.NOTES5 ,                                        " _
                            & "              A.NOTES6 ,                                        " _
                            & "              A.NOTES7 ,                                        " _
                            & "              A.NOTES8 ,                                        " _
                            & "              A.NOTES9 ,                                        " _
                            & "              A.NOTES10 ,                                       " _
                            & "              B.ARRIVTIME ,                                     " _
                            & "              B.DISTANCE                                        "
                End If
            Else ' [分類] に値が存在する場合
                If TORICODE = "" Then ' [取引先コード]に値が存在しない場合

                    SQLStr =
                                "     SELECT isnull(rtrim(A.TODOKECODE),'')    as CODE  ,      " _
                            & "              isnull(rtrim(A.NAMES),'')         as NAMES ,      " _
                            & "              rtrim(B.SEQ)                      as SEQ   ,      " _
                            & "              isnull(rtrim(A.ADDR1),'') +                       " _
                            & "              isnull(rtrim(A.ADDR2),'') +                       " _
                            & "              isnull(rtrim(A.ADDR3),'') +                       " _
                            & "              isnull(rtrim(A.ADDR4),'')         as ADDR ,       " _
                            & "              isnull(rtrim(A.NOTES1),'')        as NOTES1 ,     " _
                            & "              isnull(rtrim(A.NOTES2),'')        as NOTES2 ,     " _
                            & "              isnull(rtrim(A.NOTES3),'')        as NOTES3 ,     " _
                            & "              isnull(rtrim(A.NOTES4),'')        as NOTES4 ,     " _
                            & "              isnull(rtrim(A.NOTES5),'')        as NOTES5 ,     " _
                            & "              isnull(rtrim(A.NOTES6),'')        as NOTES6 ,     " _
                            & "              isnull(rtrim(A.NOTES7),'')        as NOTES7 ,     " _
                            & "              isnull(rtrim(A.NOTES8),'')        as NOTES8 ,     " _
                            & "              isnull(rtrim(A.NOTES9),'')        as NOTES9 ,     " _
                            & "              isnull(rtrim(A.NOTES10),'')       as NOTES10 ,    " _
                            & "              rtrim(B.ARRIVTIME)                as ARRIVTIME ,  " _
                            & "              isnull(rtrim(B.DISTANCE),'')      as DISTANCE     " _
                            & "         FROM OIL.MC006_TODOKESAKI as A                         " _
                            & "   INNER JOIN OIL.MC007_TODKORG    as B ON                      " _
                            & "              B.CAMPCODE     = A.CAMPCODE                       " _
                            & "          and B.TORICODE     = A.TORICODE                       " _
                            & "          and B.TODOKECODE   = A.TODOKECODE                     " _
                            & "          and B.UORG         = @P9                              " _
                            & "          and B.STYMD       <= @P3                              " _
                            & "          and B.ENDYMD      >= @P2                              " _
                            & "          and B.DELFLG      <> '1'                              " _
                            & "        Where A.CAMPCODE     = @P1                              " _
                            & "          and A.CLASS        = @P5                              " _
                            & "          and A.STYMD       <= @P3                              " _
                            & "          and A.ENDYMD      >= @P2                              " _
                            & "          and A.DELFLG      <> '1'                              " _
                            & "     GROUP BY B.SEQ ,  			                               " _
                            & "              A.TODOKECODE ,                                    " _
                            & "              A.NAMES ,                                         " _
                            & "              A.ADDR1 ,                                         " _
                            & "              A.ADDR2 ,                                         " _
                            & "              A.ADDR3 ,                                         " _
                            & "              A.ADDR4 ,                                         " _
                            & "              A.NOTES1 ,                                        " _
                            & "              A.NOTES2 ,                                        " _
                            & "              A.NOTES3 ,                                        " _
                            & "              A.NOTES4 ,                                        " _
                            & "              A.NOTES5 ,                                        " _
                            & "              A.NOTES6 ,                                        " _
                            & "              A.NOTES7 ,                                        " _
                            & "              A.NOTES8 ,                                        " _
                            & "              A.NOTES9 ,                                        " _
                            & "              A.NOTES10 ,                                       " _
                            & "              B.ARRIVTIME ,                                     " _
                            & "              B.DISTANCE                                        "
                Else '[取引先コード]に値が存在する場合
                    SQLStr =
                                "     SELECT isnull(rtrim(B.TODOKECODE),'')    as CODE  ,      " _
                            & "              isnull(rtrim(B.NAMES),'')         as NAMES ,      " _
                            & "              rtrim(B.SEQ)                      as SEQ   ,      " _
                            & "              isnull(rtrim(B.ADDR1),'') +                       " _
                            & "              isnull(rtrim(B.ADDR2),'') +                       " _
                            & "              isnull(rtrim(B.ADDR3),'') +                       " _
                            & "              isnull(rtrim(B.ADDR4),'')         as ADDR ,       " _
                            & "              isnull(rtrim(B.NOTES1),'')        as NOTES1 ,     " _
                            & "              isnull(rtrim(B.NOTES2),'')        as NOTES2 ,     " _
                            & "              isnull(rtrim(B.NOTES3),'')        as NOTES3 ,     " _
                            & "              isnull(rtrim(B.NOTES4),'')        as NOTES4 ,     " _
                            & "              isnull(rtrim(B.NOTES5),'')        as NOTES5 ,     " _
                            & "              isnull(rtrim(B.NOTES6),'')        as NOTES6 ,     " _
                            & "              isnull(rtrim(B.NOTES7),'')        as NOTES7 ,     " _
                            & "              isnull(rtrim(B.NOTES8),'')        as NOTES8 ,     " _
                            & "              isnull(rtrim(B.NOTES9),'')        as NOTES9 ,     " _
                            & "              isnull(rtrim(B.NOTES10),'')       as NOTES10 ,    " _
                            & "              rtrim(A.ARRIVTIME)                as ARRIVTIME ,  " _
                            & "              isnull(rtrim(A.DISTANCE),'')      as DISTANCE     " _
                            & "         FROM OIL.MC006_TODOKESAKI as A                         " _
                            & "   INNER JOIN OIL.MC007_TODKORG    as B ON                      " _
                            & "              B.CAMPCODE     = A.CAMPCODE                       " _
                            & "          and B.TORICODE     = A.TORICODE                       " _
                            & "          and B.TODOKECODE   = A.TODOKECODE                     " _
                            & "          and B.UORG         = @P9                              " _
                            & "          and B.STYMD       <= @P3                              " _
                            & "          and B.ENDYMD      >= @P2                              " _
                            & "          and B.DELFLG      <> '1'                              " _
                            & "        Where A.CAMPCODE     = @P1                              " _
                            & "          and A.TORICODE     = @P4                              " _
                            & "          and A.CLASS        = @P5                              " _
                            & "          and A.STYMD       <= @P3                              " _
                            & "          and A.ENDYMD      >= @P2                              " _
                            & "          and A.DELFLG      <> '1'                              " _
                            & "     GROUP BY B.SEQ ,  			                               " _
                            & "              A.TODOKECODE ,                                    " _
                            & "              A.NAMES ,                                         " _
                            & "              A.ADDR1 ,                                         " _
                            & "              A.ADDR2 ,                                         " _
                            & "              A.ADDR3 ,                                         " _
                            & "              A.ADDR4 ,                                         " _
                            & "              A.NOTES1 ,                                        " _
                            & "              A.NOTES2 ,                                        " _
                            & "              A.NOTES3 ,                                        " _
                            & "              A.NOTES4 ,                                        " _
                            & "              A.NOTES5 ,                                        " _
                            & "              A.NOTES6 ,                                        " _
                            & "              A.NOTES7 ,                                        " _
                            & "              A.NOTES8 ,                                        " _
                            & "              A.NOTES9 ,                                        " _
                            & "              A.NOTES10 ,                                       " _
                            & "              B.ARRIVTIME ,                                     " _
                            & "              B.DISTANCE                                        "
                End If
            End If
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.TODOKECODE, A.NAMES, B.SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES, A.TODOKECODE, B.SEQ "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ, A.TODOKECODE, A.NAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.Int, 1)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = CAMPCODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = TORICODE
                PARA5.Value = CLASSCODE
                PARA6.Value = ROLECODE
                PARA7.Value = C_ROLE_VARIANT.USER_ORG
                PARA8.Value = PERMISSION
                PARA9.Value = ORGCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                addListData(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

            ERR = C_MESSAGE_NO.NORMAL
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC007_TODKORG Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try


    End Sub

    ''' <summary>
    ''' 届先一覧(JX,COSMO)取得
    ''' </summary>
    Protected Sub getDistinationJXCOSMOList(ByVal SQLcon As SqlConnection)

        Try
            Dim SQLStr As String
            '●Leftボックス用届先取得
            If String.IsNullOrEmpty(TORICODE) Then          '[取引先コード]に値が存在しない場合
                SQLStr =
                        "  SELECT                                           " _
                    & "         rtrim(A.TODOKECODE) as CODE ,               " _
                    & "         rtrim(A.NAMES)      as NAMES                " _
                    & "    FROM OIL.MC006_TODOKESAKI    A                   " _
                    & "   Where A.CAMPCODE     = @P1                        " _
                    & "     and (substring(A.TODOKECODE,1,2)  = 'JX'        " _
                    & "      or  substring(A.TODOKECODE,1,5)  = 'COSMO')    " _
                    & "     and A.STYMD       <= @P3                        " _
                    & "     and A.ENDYMD      >= @P2                        " _
                    & "     and A.DELFLG      <> '1'                        "
            Else                                            '[取引先コード]に値が存在する場合
                SQLStr =
                        "  SELECT                                           " _
                    & "         rtrim(A.TODOKECODE) as CODE  ,              " _
                    & "         rtrim(A.NAMES)      as NAMES                " _
                    & "    FROM OIL.MC006_TODOKESAKI    A                   " _
                    & "   Where A.CAMPCODE     = @P1                        " _
                    & "     and A.STYMD       <= @P3                        " _
                    & "     and A.ENDYMD      >= @P2                        " _
                    & "     and A.DELFLG      <> '1'                        "

                If TORICODE = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX Then
                    SQLStr = SQLStr & " and substring(A.TODOKECODE,1,2)  = 'JX' "
                ElseIf TORICODE = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO Then
                    SQLStr = SQLStr & " and substring(A.TODOKECODE,1,5)  = 'COSMO' "
                End If

            End If
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.TODOKECODE, A.NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAMES, A.TODOKECODE "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.TODOKECODE, A.NAMES "
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

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0004"                       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI_JC Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub
End Class


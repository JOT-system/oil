Option Strict On
Imports System.Data.SqlClient
''' <summary>
''' 部署情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0002OrgList
    Inherits GL0000
    ''' <summary>
    ''' 権限チェックの要否
    ''' </summary>
    Public Enum LS_AUTHORITY_WITH
        ''' <summary>
        ''' 権限確認無
        ''' </summary>
        NO_AUTHORITY
        ''' <summary>
        ''' 権限確認無/自分の部署に関連するもののみ
        ''' </summary>
        NO_AUTHORITY_WITH_ORG
        ''' <summary>
        ''' 権限確認無/全部署
        ''' </summary>
        NO_AUTHORITY_WITH_ALL
        ''' <summary>
        ''' 権限確認無/会社内全部署
        ''' </summary>
        NO_AUTHORITY_WITH_CMPORG
        ''' <summary>
        ''' ユーザ権限確認
        ''' </summary>
        USER
        ''' <summary>
        ''' 端末権限確認
        ''' </summary>
        MACHINE
        ''' <summary>
        ''' 両権限確認
        ''' </summary>
        BOTH
    End Enum

    ''' <summary>
    ''' 部署レベルのカテゴリ
    ''' </summary>
    Public Class C_CATEGORY_LIST
        ''' <summary>
        ''' 営業部門 作業部署 設置部署 車庫
        ''' </summary>
        Public Const CARAGE As String = "車庫"
        ''' <summary>
        ''' 営業部門 管理部門 管理部署 支店
        ''' </summary>　
        Public Const BRANCH_OFFICE As String = "支店"
        ''' <summary>
        ''' 管理部門 所属部署 事業所
        ''' </summary>
        Public Const OFFICE_PLACE As String = "事業所"
        ''' <summary>
        ''' 管理部門 管理部署 受託 部
        ''' </summary>
        Public Const DEPARTMENT As String = "部"
        ''' <summary>
        ''' 管理部門 管理部署　役員
        ''' </summary>
        Public Const OFFICER As String = "役員"
    End Class
    ''' <summary>
    '''　権限チェック区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AUTHWITH() As LS_AUTHORITY_WITH
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID() As String
    ''' <summary>
    ''' 所属部署コード
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
    ''' 部署取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>"</remarks>
    Public Property Categorys() As String()
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "GL0002OrgLst"


    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理

        'PARAM 01: Categorys
        If checkParam(METHOD_NAME, Categorys) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM EXTRA01: STYMD
        If STYMD < CDate(C_DEFAULT_YMD) Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA02: ENDYMD
        If ENDYMD < CDate(C_DEFAULT_YMD) Then
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

        Select Case AUTHWITH
            'Case LS_AUTHORITY_WITH.USER
            '    getOrgListWithUserAuth(SQLcon)
            'Case LS_AUTHORITY_WITH.MACHINE
            '    getOrgListWithTermAuth(SQLcon)
            'Case LS_AUTHORITY_WITH.BOTH
            '    getOrgListWithBothAuth(SQLcon)
            'Case LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_ORG
            '    getOrgRelationList(SQLcon)
            Case LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_ALL
                getOrgAllList(SQLcon, "1")
            Case LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG
                getOrgAllList(SQLcon, "2")
            Case Else
                getOrgList(SQLcon)
        End Select

        SQLcon.Close() 'DataBase接続(Close)
        SQLcon.Dispose()
        SQLcon = Nothing

    End Sub

    ''' <summary>
    ''' 部署一覧取得
    ''' </summary>
    Protected Sub getOrgList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用部署取得
        '○ User権限によりDB(OIM0002_ORG)検索
        Try
            '検索SQL文
            Dim SQLStr As String =
                  " SELECT                                 " _
                & "   rtrim(A.ORGCODE)     as CODE      ,  " _
                & "   rtrim(A.NAME)        as NAMES     ,  " _
                & "   rtrim(A.ORGCODE)     as CATEGORY  ,  " _
                & "   ''                   as SEQ          " _
                & " FROM       OIL.OIM0002_ORG A           " _
                & " Where                                  " _
                & "         A.ORGCODE  = @P2               " _
                & "   and   A.STYMD   <= @P3               " _
                & "   and   A.ENDYMD  >= @P4               " _
                & "   and   A.DELFLG  <> @P5               "
            If Not String.IsNullOrEmpty(CAMPCODE) Then SQLStr = SQLStr & " and A.CAMPCODE = @P1 "
            SQLStr = SQLStr & " GROUP BY A.ORGCODE , A.NAME , A.ORGCODE "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.ORGCODE , A.NAME "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAME, A.ORGCODE "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.ORGCODE , A.NAME "
                Case Else
            End Select

            '  " SELECT                                 " _
            '& "   rtrim(A.CODE)        as CODE      ,  " _
            '& "   rtrim(B.NAMES)       as NAMES     ,  " _
            '& "   rtrim(A.GRCODE01)    as CATEGORY  ,  " _
            '& "   rtrim(A.SEQ)         as SEQ          " _
            '& " FROM       OIL.M0006_STRUCT A          " _
            '& " INNER JOIN OIL.OIM0002_ORG B ON          " _
            '& "         A.CAMPCODE = B.CAMPCODE        " _
            '& "   and   A.CODE     = B.ORGCODE         " _
            '& "   and   B.STYMD   <= @P5               " _
            '& "   and   B.ENDYMD  >= @P4               " _
            '& "   and   B.DELFLG  <> @P8               " _
            '& " Where                                  " _
            '& "         A.OBJECT   = @P1               " _
            '& "   and   A.STRUCT   = @P2               " _
            '& "   and   A.STYMD   <= @P5               " _
            '& "   and   A.ENDYMD  >= @P4               " _
            '& "   and   A.DELFLG  <> @P8               "
            'If Not String.IsNullOrEmpty(CAMPCODE) Then SQLStr = SQLStr & " and A.CAMPCODE = @P1 "
            'SQLStr = SQLStr & " GROUP BY A.CODE , B.NAMES , A.GRCODE01 , A.SEQ "
            ''〇ソート条件追加
            'Select Case DEFAULT_SORT
            '    Case C_DEFAULT_SORT.CODE
            '        SQLStr = SQLStr & " ORDER BY A.CODE , B.NAMES , A.SEQ "
            '    Case C_DEFAULT_SORT.NAMES
            '        SQLStr = SQLStr & " ORDER BY B.NAMES, A.CODE , A.SEQ "
            '    Case C_DEFAULT_SORT.SEQ, String.Empty
            '        SQLStr = SQLStr & " ORDER BY A.SEQ, A.CODE , B.NAMES "
            '    Case Else
            'End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                'Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", System.Data.SqlDbType.NVarChar, 20)
                'Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                'Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                'Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                'Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                'Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 1)

                'PARA0.Value = CAMPCODE
                'PARA1.Value = C_ROLE_VARIANT.USER_ORG
                'PARA2.Value = C_STRUCT_CODE.ORG_LIST_CODE
                'PARA4.Value = STYMD
                'PARA5.Value = ENDYMD
                'PARA8.Value = C_DELETE_FLG.DELETE
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = CAMPCODE
                    .Add("@P2", SqlDbType.NVarChar, 6).Value = ORGCODE
                    .Add("@P3", SqlDbType.Date).Value = STYMD
                    .Add("@P4", SqlDbType.Date).Value = ENDYMD
                    .Add("@P5", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○出力編集
                    addListData(SQLdr)
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0002"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIM0002_ORG Select"
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
    ''' 全部署一覧取得
    ''' </summary>
    Protected Sub getOrgAllList(ByVal SQLcon As SqlConnection, ByVal COMPANYCODE_FLG As String)
        '●Leftボックス用部署取得
        '○ User権限によりDB(OIM0002_ORG)検索
        Try
            '検索SQL文
            Dim SQLStr As String =
                  " SELECT                                 " _
                & "   rtrim(A.ORGCODE)     as CODE      ,  " _
                & "   rtrim(A.NAME)        as NAMES     ,  " _
                & "   rtrim(A.ORGCODE)     as CATEGORY  ,  " _
                & "   ''                   as SEQ          " _
                & " FROM       OIL.OIM0002_ORG A           " _
                & " Where                                  " _
                & "         A.STYMD   <= @P1               " _
                & "   and   A.ENDYMD  >= @P2               " _
                & "   and   A.DELFLG  <> @P3               "

            If COMPANYCODE_FLG <> "1" Then
                If Not String.IsNullOrEmpty(CAMPCODE) Then SQLStr &= " and A.CAMPCODE = @P0 "
            End If
            SQLStr = SQLStr & " GROUP BY A.ORGCODE , A.NAME , A.ORGCODE "
                '〇ソート条件追加
                Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.ORGCODE , A.NAME "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.NAME, A.ORGCODE "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.ORGCODE , A.NAME "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P0", SqlDbType.NVarChar, 20).Value = CAMPCODE
                    .Add("@P1", SqlDbType.Date).Value = STYMD
                    .Add("@P2", SqlDbType.Date).Value = ENDYMD
                    .Add("@P3", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○出力編集
                    addListData(SQLdr)
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0002"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIM0002_ORG Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

    '''' <summary>
    '''' 指定部署の関連する部署一覧取得
    '''' </summary>
    'Protected Sub getOrgRelationList(ByVal SQLcon As SqlConnection)
    '    '●Leftボックス用部署取得
    '    '○ User権限によりDB(OIM0002_ORG)検索
    '    Try
    '        '検索SQL文
    '        Dim SQLStr As String =
    '              " SELECT                                  " _
    '            & "   rtrim(A.CODE)        as CODE        , " _
    '            & "   rtrim(B.NAMES)       as NAMES       , " _
    '            & "   rtrim(A.GRCODE01)    as CATEGORY    , " _
    '            & "   rtrim(A.SEQ)         as SEQ           " _
    '            & " FROM       OIL.M0006_STRUCT A           " _
    '            & " INNER JOIN OIL.OIM0002_ORG B ON           " _
    '            & "         A.CAMPCODE = B.CAMPCODE         " _
    '            & "   and   A.CODE     = B.ORGCODE          " _
    '            & "   and   B.STYMD   <= @P5                " _
    '            & "   and   B.ENDYMD  >= @P4                " _
    '            & "   and   B.DELFLG  <> @P8                " _
    '            & " INNER JOIN OIL.M0006_STRUCT C ON        " _
    '            & "         A.CODE     = C.CODE             " _
    '            & "   and   A.CAMPCODE = C.CAMPCODE         " _
    '            & "   and   A.OBJECT   = C.OBJECT           " _
    '            & "   and   C.STYMD   <= @P5                " _
    '            & "   and   C.ENDYMD  >= @P4                " _
    '            & "   and   C.DELFLG  <> @P8                " _
    '            & "   and   C.STRUCT  IN (                  " _
    '            & "    SELECT @P2 + '_' + C2.GRCODE02       " _
    '            & "    FROM OIL.M0006_STRUCT C2             " _
    '            & "    WHERE                                " _
    '            & "           C2.OBJECT   = @P1             " _
    '            & "     and   C2.STRUCT   = @P2             " _
    '            & "     and   C2.CODE     = @P10            " _
    '            & "     and   C2.STYMD   <= @P5             " _
    '            & "     and   C2.ENDYMD  >= @P4 " _
    '            & "     and   C2.DELFLG  <> @P8 " _
    '            & "    ) " _
    '            & " WHERE " _
    '            & "         A.OBJECT   = @P1 " _
    '            & "   and   A.STRUCT   = @P2 " _
    '            & "   and   A.STYMD   <= @P5 " _
    '            & "   and   A.ENDYMD  >= @P4 " _
    '            & "   and   A.DELFLG  <> @P8 "
    '        If Not String.IsNullOrEmpty(CAMPCODE) Then SQLStr = SQLStr & " and A.CAMPCODE = @P0 "
    '        SQLStr = SQLStr & " GROUP BY A.CODE , B.NAMES , A.GRCODE01 , A.SEQ "
    '        '〇ソート条件追加
    '        Select Case DEFAULT_SORT
    '            Case C_DEFAULT_SORT.CODE
    '                SQLStr = SQLStr & " ORDER BY A.CODE , B.NAMES , A.SEQ "
    '            Case C_DEFAULT_SORT.NAMES
    '                SQLStr = SQLStr & " ORDER BY B.NAMES, A.CODE , A.SEQ "
    '            Case C_DEFAULT_SORT.SEQ, String.Empty
    '                SQLStr = SQLStr & " ORDER BY A.SEQ, A.CODE , B.NAMES "

    '            Case Else
    '        End Select

    '        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
    '            Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
    '            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
    '            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
    '            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 1)
    '            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)

    '            PARA0.Value = CAMPCODE
    '            PARA1.Value = C_ROLE_VARIANT.USER_ORG
    '            PARA2.Value = C_STRUCT_CODE.ORG_LIST_CODE
    '            PARA4.Value = STYMD
    '            PARA5.Value = ENDYMD
    '            PARA8.Value = C_DELETE_FLG.DELETE
    '            PARA10.Value = ORGCODE
    '            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

    '            '○出力編集
    '            addListData(SQLdr)

    '            'Close
    '            SQLdr.Close() 'Reader(Close)
    '            SQLdr = Nothing

    '        End Using

    '    Catch ex As Exception
    '        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    '        CS0011LOGWRITE.INFSUBCLASS = "GL0002"                'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "DB:OIM0002_ORG Select"
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWRITE.TEXT = ex.ToString()
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
    '        ERR = C_MESSAGE_NO.DB_ERROR
    '        Exit Sub
    '    End Try

    '    ERR = C_MESSAGE_NO.NORMAL

    'End Sub

    '''' <summary>
    '''' 部署一覧取得
    '''' </summary>
    'Protected Sub getOrgListWithUserAuth(ByVal SQLcon As SqlConnection)
    '    '●Leftボックス用会社取得
    '    '○ User権限によりDB(OIM0002_ORG)検索
    '    Try
    '        '検索SQL文
    '        Dim SQLStr As String =
    '                "SELECT " _
    '            & " rtrim(A.CODE)        as CODE ," _
    '            & " rtrim(B.NAMES)       as NAMES ," _
    '            & " rtrim(A.GRCODE01)    as CATEGORY    , " _
    '            & " rtrim(A.SEQ)         as SEQ " _
    '            & " FROM  OIL.M0006_STRUCT A " _
    '            & " INNER JOIN OIL.OIM0002_ORG B ON " _
    '            & "         A.CAMPCODE = B.CAMPCODE " _
    '            & "   and   A.CODE     = B.ORGCODE " _
    '            & "   and   B.STYMD   <= @P5 " _
    '            & "   and   B.ENDYMD  >= @P4 " _
    '            & "   and   B.DELFLG  <> @P8 " _
    '            & " INNER JOIN COM.OIS0009_ROLE C ON " _
    '            & "         C.CAMPCODE = A.CAMPCODE " _
    '            & "   and   C.OBJECT   = A.OBJECT " _
    '            & "   and   C.ROLE     = @P6 " _
    '            & "   and   C.PERMITCODE >= @P7 " _
    '            & "   and   C.STYMD   <= @P5 " _
    '            & "   and   C.ENDYMD  >= @P4 " _
    '            & "   and   C.DELFLG  <> @P8 " _
    '            & " Where " _
    '            & "         A.OBJECT   = @P1 " _
    '            & "   and   A.STRUCT   = @P2 " _
    '            & "   and   A.STYMD   <= @P5 " _
    '            & "   and   A.ENDYMD  >= @P4 " _
    '            & "   and   A.DELFLG  <> @P8 "
    '        If Not String.IsNullOrEmpty(CAMPCODE) Then SQLStr = SQLStr & " and A.CAMPCODE = @P0 "
    '        SQLStr = SQLStr & " GROUP BY A.CODE , B.NAMES , A.GRCODE01 , A.SEQ "
    '        '〇ソート条件追加
    '        Select Case DEFAULT_SORT
    '            Case C_DEFAULT_SORT.CODE
    '                SQLStr = SQLStr & " ORDER BY A.CODE , B.NAMES , A.SEQ  "
    '            Case C_DEFAULT_SORT.NAMES
    '                SQLStr = SQLStr & " ORDER BY B.NAMES, A.CODE , A.SEQ  "
    '            Case C_DEFAULT_SORT.SEQ, String.Empty
    '                SQLStr = SQLStr & " ORDER BY A.SEQ, A.CODE , B.NAMES  "
    '            Case Else
    '        End Select

    '        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
    '            Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
    '            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
    '            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
    '            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Int, 1)
    '            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 1)

    '            PARA0.Value = CAMPCODE
    '            PARA1.Value = C_ROLE_VARIANT.USER_ORG
    '            PARA2.Value = C_STRUCT_CODE.ORG_LIST_CODE
    '            PARA4.Value = STYMD
    '            PARA5.Value = ENDYMD
    '            PARA6.Value = ROLECODE
    '            PARA7.Value = PERMISSION
    '            PARA8.Value = C_DELETE_FLG.DELETE
    '            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

    '            '○出力編集
    '            addListData(SQLdr)

    '            'Close
    '            SQLdr.Close() 'Reader(Close)
    '            SQLdr = Nothing

    '        End Using

    '    Catch ex As Exception
    '        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    '        CS0011LOGWRITE.INFSUBCLASS = "GL0002"                'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "DB:OIM0002_ORG Select"
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWRITE.TEXT = ex.ToString()
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
    '        ERR = C_MESSAGE_NO.DB_ERROR
    '        Exit Sub
    '    End Try

    '    ERR = C_MESSAGE_NO.NORMAL

    'End Sub

    '''' <summary>
    '''' 部署一覧取得
    '''' </summary>
    'Protected Sub getOrgListWithTermAuth(ByVal SQLcon As SqlConnection)
    '    '●Leftボックス用会社取得
    '    '○ User権限によりDB(OIM0002_ORG)検索
    '    Try
    '        '検索SQL文
    '        Dim SQLStr As String =
    '                "SELECT " _
    '            & " rtrim(A.CODE) as CODE ," _
    '            & " rtrim(B.NAMES) as NAMES ," _
    '            & " rtrim(A.GRCODE01)    as CATEGORY    , " _
    '            & " rtimr(A.SEQ)   as SEQ " _
    '            & " FROM  OIL.M0006_STRUCT A " _
    '            & " INNER JOIN OIL.OIM0002_ORG B ON " _
    '            & "         A.CAMPCODE = B.CAMPCODE " _
    '            & "   and   A.CODE     = B.ORGCODE " _
    '            & "   and   B.STYMD   <= @P5 " _
    '            & "   and   B.ENDYMD  >= @P4 " _
    '            & "   and   B.DELFLG  <> @P8 " _
    '            & " INNER JOIN COM.OIS0011_SRVAUTHOR C ON " _
    '            & "         C.CAMPCODE = A.CAMPCODE " _
    '            & "   and   C.OBJECT   = @P10 " _
    '            & "   and   C.TERMID   = @P9 " _
    '            & "   and   C.ROLE     = @P6 " _
    '            & "   and   C.STYMD   <= @P5 " _
    '            & "   and   C.ENDYMD  >= @P4 " _
    '            & "   and   C.DELFLG  <> @P8 " _
    '            & " INNER JOIN COM.OIS0009_ROLE D ON " _
    '            & "         D.CAMPCODE = A.CAMPCODE " _
    '            & "   and   D.OBJECT   = C.OBJECT " _
    '            & "   and   D.ROLE     = C.ROLE " _
    '            & "   and   D.PERMITCODE >= @P7 " _
    '            & "   and   D.STYMD   <= @P5 " _
    '            & "   and   D.ENDYMD  >= @P4 " _
    '            & "   and   D.DELFLG  <> @P8 " _
    '            & " Where " _
    '            & "         A.OBJECT   = @P1 " _
    '            & "   and   A.STRUCT   = @P2 " _
    '            & "   and   A.STYMD   <= @P5 " _
    '            & "   and   A.ENDYMD  >= @P4 " _
    '            & "   and   A.DELFLG  <> @P8 "
    '        If Not String.IsNullOrEmpty(CAMPCODE) Then SQLStr = SQLStr & " and A.CAMPCODE = @P0 "
    '        SQLStr = SQLStr & " GROUP BY A.CODE , B.NAMES , A.GRCODE01 , A.SEQ "
    '        '〇ソート条件追加
    '        Select Case DEFAULT_SORT
    '            Case C_DEFAULT_SORT.CODE
    '                SQLStr = SQLStr & " ORDER BY A.CODE , B.NAMES , A.SEQ "
    '            Case C_DEFAULT_SORT.NAMES
    '                SQLStr = SQLStr & " ORDER BY A.NAMES, A.CODE , A.SEQ "
    '            Case C_DEFAULT_SORT.SEQ, String.Empty
    '                SQLStr = SQLStr & " ORDER BY A.SEQ , A.CODE , B.NAMES "
    '            Case Else
    '        End Select

    '        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
    '            Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
    '            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
    '            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
    '            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Int, 1)
    '            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 1)
    '            Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar, 30)
    '            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
    '            PARA0.Value = CAMPCODE
    '            PARA1.Value = C_ROLE_VARIANT.USER_ORG
    '            PARA2.Value = C_STRUCT_CODE.ORG_LIST_CODE
    '            PARA4.Value = STYMD
    '            PARA5.Value = ENDYMD
    '            PARA6.Value = ROLECODE
    '            PARA7.Value = PERMISSION
    '            PARA8.Value = C_DELETE_FLG.DELETE
    '            PARA9.Value = TERMID
    '            PARA10.Value = C_ROLE_VARIANT.SERV_ORG
    '            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

    '            '○出力編集
    '            addListData(SQLdr)

    '            'Close
    '            SQLdr.Close() 'Reader(Close)
    '            SQLdr = Nothing

    '        End Using

    '    Catch ex As Exception
    '        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    '        CS0011LOGWRITE.INFSUBCLASS = "GL0002"                'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "DB:OIM0002_ORG Select"
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWRITE.TEXT = ex.ToString()
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
    '        ERR = C_MESSAGE_NO.DB_ERROR
    '        Exit Sub
    '    End Try

    '    ERR = C_MESSAGE_NO.NORMAL

    'End Sub

    '''' <summary>
    '''' 部署一覧取得
    '''' </summary>
    'Protected Sub getOrgListWithBothAuth(ByVal SQLcon As SqlConnection)
    '    '●Leftボックス用会社取得
    '    '○ User権限によりDB(OIM0002_ORG)検索
    '    Try
    '        '検索SQL文
    '        Dim SQLStr As String =
    '                "SELECT " _
    '            & " rtrim(A.CODE) as CODE ," _
    '            & " rtrim(B.NAMES) as NAMES  , " _
    '            & " rtrim(A.GRCODE01)    as CATEGORY    , " _
    '            & " rtrim(A.SEQ)  as SEQ " _
    '            & " FROM  OIL.M0006_STRUCT A " _
    '            & " INNER JOIN OIL.OIM0002_ORG B ON " _
    '            & "         A.CAMPCODE = B.CAMPCODE " _
    '            & "   and   A.CODE     = B.ORGCODE " _
    '            & "   and   B.STYMD   <= @P5 " _
    '            & "   and   B.ENDYMD  >= @P4 " _
    '            & "   and   B.DELFLG  <> @P8 " _
    '            & " INNER JOIN COM.OIS0011_SRVAUTHOR C ON " _
    '            & "         C.CAMPCODE = A.CAMPCODE " _
    '            & "   and   C.OBJECT   = @P10 " _
    '            & "   and   C.TERMID   = @P9 " _
    '            & "   and   C.ROLE     = @P6 " _
    '            & "   and   C.STYMD   <= @P5 " _
    '            & "   and   C.ENDYMD  >= @P4 " _
    '            & "   and   C.DELFLG  <> @P8 " _
    '            & " INNER JOIN COM.OIS0009_ROLE D ON " _
    '            & "         D.CAMPCODE = A.CAMPCODE " _
    '            & "   and   D.OBJECT   = C.OBJECT " _
    '            & "   and   D.ROLE     = C.ROLE " _
    '            & "   and   D.PERMITCODE >= @P7 " _
    '            & "   and   D.STYMD   <= @P5 " _
    '            & "   and   D.ENDYMD  >= @P4 " _
    '            & "   and   D.DELFLG  <> @P8 " _
    '            & " INNER JOIN COM.OIS0009_ROLE E ON " _
    '            & "         E.CAMPCODE = A.CAMPCODE " _
    '            & "   and   E.OBJECT   = A.OBJECT " _
    '            & "   and   E.ROLE     = @P6 " _
    '            & "   and   E.PERMITCODE >= @P7 " _
    '            & "   and   E.STYMD   <= @P5 " _
    '            & "   and   E.ENDYMD  >= @P4 " _
    '            & "   and   E.DELFLG  <> @P8 " _
    '            & " Where " _
    '            & "         A.OBJECT   = @P1 " _
    '            & "   and   A.STRUCT   = @P2 " _
    '            & "   and   A.STYMD   <= @P5 " _
    '            & "   and   A.ENDYMD  >= @P4 " _
    '            & "   and   A.DELFLG  <> @P8 "
    '        If Not String.IsNullOrEmpty(CAMPCODE) Then SQLStr = SQLStr & " and A.CAMPCODE = @P0 "
    '        SQLStr = SQLStr & " GROUP BY A.CODE , B.NAMES , A.GRCODE01 , A.SEQ "
    '        '〇ソート条件追加
    '        Select Case DEFAULT_SORT
    '            Case C_DEFAULT_SORT.CODE
    '                SQLStr = SQLStr & " ORDER BY A.CODE, B.NAMES, A.SEQ "
    '            Case C_DEFAULT_SORT.NAMES
    '                SQLStr = SQLStr & " ORDER BY B.NAMES, A.CODE, A.SEQ "
    '            Case C_DEFAULT_SORT.SEQ, String.Empty
    '                SQLStr = SQLStr & " ORDER BY A.SEQ, A.CODE, B.NAMES "

    '            Case Else
    '        End Select

    '        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
    '            Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
    '            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
    '            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
    '            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
    '            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Int, 1)
    '            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 1)
    '            Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar, 30)
    '            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
    '            PARA0.Value = CAMPCODE
    '            PARA1.Value = C_ROLE_VARIANT.USER_ORG
    '            PARA2.Value = C_STRUCT_CODE.ORG_LIST_CODE
    '            PARA4.Value = STYMD
    '            PARA5.Value = ENDYMD
    '            PARA6.Value = ROLECODE
    '            PARA7.Value = PERMISSION
    '            PARA8.Value = C_DELETE_FLG.DELETE
    '            PARA9.Value = TERMID
    '            PARA10.Value = C_ROLE_VARIANT.SERV_ORG
    '            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

    '            '○出力編集
    '            addListData(SQLdr)

    '            'Close
    '            SQLdr.Close() 'Reader(Close)
    '            SQLdr = Nothing
    '        End Using

    '    Catch ex As Exception
    '        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    '        CS0011LOGWRITE.INFSUBCLASS = "GL0002"                'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "DB:OIM0002_ORG Select"
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWRITE.TEXT = ex.ToString()
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
    '        ERR = C_MESSAGE_NO.DB_ERROR
    '        Exit Sub
    '    End Try

    '    ERR = C_MESSAGE_NO.NORMAL

    'End Sub

    ''' <summary>
    ''' 一覧登録時のチェック処理
    ''' </summary>
    ''' <param name="I_SQLDR"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overrides Function extracheck(ByVal I_SQLDR As SqlDataReader) As Boolean
        Return (IsNothing(Me.Categorys) OrElse Categorys.Contains(Convert.ToString(I_SQLDR("CATEGORY"))))

    End Function
End Class


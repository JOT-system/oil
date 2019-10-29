Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 社員情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0005StaffList
    Inherits GL0000
    ''' <summary>
    ''' 取得条件
    ''' </summary>
    Public Enum LC_STAFF_TYPE
        ''' <summary>
        ''' 全取得
        ''' </summary>
        ALL
        ''' <summary>
        ''' 社員 (未実装)
        ''' </summary>
        EMPLOYEE
        ''' <summary>
        ''' 従業員 (未実装)
        ''' </summary>
        WORKER
        ''' <summary>
        ''' 乗務員
        ''' </summary>
        DRIVER
        ''' <summary>
        ''' 事務員員
        ''' </summary>
        CLERK
        ''' <summary>
        ''' 社員
        ''' </summary>
        EMPLOYEE_IN_ORG
        ''' <summary>
        ''' 従業員
        ''' </summary>
        WORKER_IN_ORG
        ''' <summary>
        ''' 全取得(勤怠用）
        ''' </summary>
        ATTENDANCE_FOR_ALL
        ''' <summary>
        ''' 乗務員(勤怠用）
        ''' </summary>
        ATTENDANCE_FOR_DRIVER
        ''' <summary>
        ''' 事務員(勤怠用）
        ''' </summary>
        ATTENDANCE_FOR_CLERK
        ''' <summary>
        ''' 全取得(勤怠用）
        ''' </summary>
        ATTENDANCE_FOR_ALL_IN_AORG
        ''' <summary>
        ''' 乗務員(勤怠用）
        ''' </summary>
        ATTENDANCE_FOR_DRIVER_IN_AORG
        ''' <summary>
        ''' 事務員(勤怠用）
        ''' </summary>
        ATTENDANCE_FOR_CLERK_IN_AORG
        ''' <summary>
        ''' 全取得(傭車用）
        ''' </summary>
        EMPLOY_ALL
        ''' <summary>
        ''' 全取得(傭車以外）
        ''' </summary>
        EMPLOY_EXCEPT_ALL

    End Enum

    ''' <summary>
    '''　取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TYPE() As LC_STAFF_TYPE
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
    ''' 社員コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFCODE() As String
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
    ''' 従業員区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAFFKBN() As List(Of String)
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
    ''' 従業員コード桁数
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const STAFF_LENGTH As String = "5"
    ''' <summary>
    ''' 傭車従業員コード桁数
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const STAFF_EMPLOY_LENGTH As String = "7"

    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        'PARAM 01: TYPE
        If checkParam(METHOD_NAME, TYPE) Then Exit Sub
        'PARAM EXTRA01: ORGCODE
        If String.IsNullOrEmpty(ORGCODE) Then ORGCODE = ""
        'PARAM EXTRA02: STYMD
        If STYMD < C_DEFAULT_YMD Then STYMD = Date.Now
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then ENDYMD = Date.Now
        'PARAM EXTRA04: TERMID
        If String.IsNullOrEmpty(TERMID) Then TERMID = sm.APSV_ID
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
                Case LC_STAFF_TYPE.EMPLOYEE_IN_ORG
                    getEmpInOrgList(SQLcon)
                Case LC_STAFF_TYPE.WORKER_IN_ORG
                    getWrkerInOrgList(SQLcon)
                Case LC_STAFF_TYPE.DRIVER
                    getDriverList(SQLcon)
                Case LC_STAFF_TYPE.CLERK
                    getClerkList(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_ALL
                    getAttendanceList(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER
                    getAttendanceDriverList(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK
                    getAttendanceClerkList(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_ALL_IN_AORG
                    getAttendanceInAPOrgList(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER_IN_AORG
                    getAttendanceDriverInAPOrgList(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK_IN_AORG
                    getAttendanceClerkInAPOrgList(SQLcon)
                Case LC_STAFF_TYPE.EMPLOY_ALL
                    getStaffList(SQLcon, STAFF_EMPLOY_LENGTH)
                Case LC_STAFF_TYPE.EMPLOY_EXCEPT_ALL
                    getStaffList(SQLcon, STAFF_LENGTH)
                Case Else
                    getStaffList(SQLcon)
            End Select

        End Using

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
        If checkParam(METHOD_NAME, TYPE) Then Exit Sub
        'PARAM EXTRA01: ORGCODE
        If String.IsNullOrEmpty(ORGCODE) Then ORGCODE = ""
        'PARAM EXTRA02: STYMD
        If STYMD < C_DEFAULT_YMD Then STYMD = Date.Now
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then ENDYMD = Date.Now
        'PARAM EXTRA04: TERMID
        If String.IsNullOrEmpty(TERMID) Then TERMID = sm.APSV_ID

        If IsNothing(AREA) Then
            Exit Sub
        End If
        'DataBase接続文字
        Using SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Select Case TYPE
                Case LC_STAFF_TYPE.EMPLOYEE_IN_ORG
                    getEmpInOrgTbl(SQLcon)
                Case LC_STAFF_TYPE.WORKER_IN_ORG
                    getWrkerInOrgTbl(SQLcon)
                Case LC_STAFF_TYPE.DRIVER
                    getDriverTbl(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_ALL
                    getAttendanceTbl(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER
                    getAttendanceDriverTbl(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK
                    getAttendanceClerkTbl(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_ALL_IN_AORG
                    getAttendanceInAPOrgTbl(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER_IN_AORG
                    getAttendanceDriverInAPOrgTbl(SQLcon)
                Case LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK_IN_AORG
                    getAttendanceClerkInAPOrgTbl(SQLcon)
                Case LC_STAFF_TYPE.EMPLOY_ALL
                    getStaffTbl(SQLcon, STAFF_EMPLOY_LENGTH)
                Case LC_STAFF_TYPE.EMPLOY_EXCEPT_ALL
                    getStaffTbl(SQLcon, STAFF_LENGTH)
                Case Else
                    getStaffTbl(SQLcon)
            End Select

        End Using

    End Sub
    ''' <summary>
    ''' 社員取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getStaffList(ByVal SQLcon As SqlConnection, Optional ByVal length As String = "")
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try

            '検索SQL文
            Dim SQLStr As String =
                          " SELECT " _
                        & "        rtrim(A.STAFFCODE)  as CODE     " _
                        & "      , rtrim(A.STAFFNAMES) as NAMES    " _
                        & "      , rtrim(A.STAFFKBN)   as KBN      " _
                        & " FROM                                   " _
                        & "        OIL.MB001_STAFF        A        " _
                        & " WHERE                                  " _
                        & "            A.STYMD      <= @P3         " _
                        & "        and A.ENDYMD     >= @P2         " _
                        & "        and A.DELFLG     <> '1'         "

            If (String.IsNullOrEmpty(Me.ORGCODE) = False) Then SQLStr &= String.Format(" and A.HORG = '{0}' ", Me.ORGCODE)
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            If (String.IsNullOrEmpty(length) = False) Then SQLStr &= String.Format(" and LEN(A.STAFFCODE) = '{0}' ", length)

            SQLStr &= " GROUP BY A.STAFFCODE , A.STAFFNAMES ,A.STAFFKBN "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFNAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE  "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.STAFFKBN, A.STAFFCODE   "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 従業員取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getEmpInOrgList(ByVal SQLcon As SqlConnection)

        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try

            '検索SQL文
            Dim SQLStr As String =
                          " SELECT " _
                        & "        rtrim(A.STAFFCODE)  as CODE     " _
                        & "      , rtrim(A.STAFFNAMES) as NAMES    " _
                        & "      , rtrim(A.STAFFKBN)   as KBN      " _
                        & " FROM                                   " _
                        & "        OIL.MB001_STAFF        A        " _
                        & " INNER JOIN OIL.M0002_ORG      B     ON " _
                        & "            B.CAMPCODE    = A.CAMPCODE  " _
                        & "        and B.ORGCODE     = A.MORG      " _
                        & "        and B.DELFLG     <> '1'         " _
                        & "        and B.STYMD      <= @P3         " _
                        & "        and B.ENDYMD     >= @P2         " _
                        & " INNER JOIN COM.S0006_ROLE     C     ON " _
                        & "            C.CAMPCODE    = B.CAMPCODE  " _
                        & "        and C.CODE        = B.ORGCODE   " _
                        & "        and C.OBJECT      = @P4         " _
                        & "        and C.ROLE        = @P1         " _
                        & "        and C.PERMITCODE >= @P5         " _
                        & "        and C.STYMD      <= @P3         " _
                        & "        and C.ENDYMD     >= @P2         " _
                        & "        and C.DELFLG     <> '1'         " _
                        & " INNER JOIN OIL.M0002_ORG      S     ON " _
                        & "            S.CAMPCODE    = A.CAMPCODE  " _
                        & "        and S.ORGCODE     = A.HORG      " _
                        & "        and S.DELFLG     <> '1'         " _
                        & "        and S.STYMD      <= @P3         " _
                        & "        and S.ENDYMD     >= @P2         " _
                        & " INNER JOIN COM.S0006_ROLE     T     ON " _
                        & "            T.CAMPCODE    = B.CAMPCODE  " _
                        & "        and T.CODE        = B.ORGCODE   " _
                        & "        and T.OBJECT      = @P4         " _
                        & "        and T.ROLE        = @P1         " _
                        & "        and T.PERMITCODE >= @P5         " _
                        & "        and T.STYMD      <= @P3         " _
                        & "        and T.ENDYMD     >= @P2         " _
                        & "        and T.DELFLG     <> '1'         " _
                        & " WHERE                                  " _
                        & "            A.STYMD      <= @P3         " _
                        & "        and A.ENDYMD     >= @P2         " _
                        & "        and A.DELFLG     <> '1'         "

            If (String.IsNullOrEmpty(Me.ORGCODE) = False) Then SQLStr &= String.Format(" and A.HORG = '{0}' ", Me.ORGCODE)
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)

            SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE , A.STAFFKBN  "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case Else
            End Select

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Int, 1)
            PARA1.Value = ROLECODE
            PARA2.Value = STYMD
            PARA3.Value = ENDYMD
            PARA4.Value = C_ROLE_VARIANT.USER_ORG
            PARA5.Value = PERMISSION
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()


            '○出力編集
            addListData(SQLdr)

            ERR = C_MESSAGE_NO.NORMAL
            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の社員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                     "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & "      , isnull(C.SEQ, 0)               as SEQ   " _
                   & " FROM       OIL.MB001_STAFF  A                  " _
                   & " INNER JOIN OIL.M0006_STRUCT B               ON " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C             ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " WHERE                                          " _
                   & "       A.STYMD     <= @P3                       " _
                   & "   and A.ENDYMD    >= @P2                       " _
                   & "   and A.DELFLG    <> '1'                       "
            Else
                SQLStr =
                     "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & " FROM       OIL.MB001_STAFF A                   " _
                   & " INNER JOIN OIL.M0006_STRUCT B              ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " Where                                          " _
                   & "       A.STYMD     <= @P3                       " _
                   & "   and A.ENDYMD    >= @P2                       " _
                   & "   and A.DELFLG    <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            '検索SQL文
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE , A.STAFFKBN "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN, C.SEQ "
                    SQLStr = SQLStr & " ORDER BY C.SEQ , A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now

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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の乗務員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceDriverList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                 "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & "      , isnull(C.SEQ,0)                as SEQ   " _
                   & " FROM       OIL.MB001_STAFF A                   " _
                   & " INNER JOIN OIL.M0006_STRUCT B              ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C             ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E           " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2   = '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                 "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
               & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
               & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
               & " FROM       OIL.MB001_STAFF A                   " _
               & " INNER JOIN OIL.M0006_STRUCT B              ON  " _
               & "        B.CAMPCODE = A.CAMPCODE                 " _
               & "   and  B.OBJECT    = @P6                       " _
               & "   and  B.STRUCT    = @P7                       " _
               & "   and  B.GRCODE01  = @P8                       " _
               & "   and  B.STYMD    <= @P9                       " _
               & "   and  B.ENDYMD   >= @P9                       " _
               & "   and  B.DELFLG   <> '1'                       " _
               & "   and  B.CODE      = A.HORG                    " _
               & " WHERE                                          " _
               & "         A.STAFFKBN IN (                        " _
               & "            SELECT                              " _
               & "                     KEYCODE                    " _
               & "            FROM OIL.MC001_FIXVALUE E           " _
               & "            WHERE                               " _
               & "                    E.CAMPCODE = A.CAMPCODE     " _
               & "              and   E.CLASS    = 'STAFFKBN'     " _
               & "              and   E.VALUE2   = '1'            " _
               & "              and   E.STYMD   <= @P9            " _
               & "              and   E.ENDYMD  >= @P9            " _
               & "              and   E.DELFLG  <> '1'            " _
               & "         )                                      " _
               & "   and   A.STYMD   <= @P3                       " _
               & "   and   A.ENDYMD  >= @P2                       " _
               & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES  "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE , A.STAFFKBN  "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN ,C.SEQ"
                    SQLStr = SQLStr & " ORDER BY C.SEQ , A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now

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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の事務員員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceClerkList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                 "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & "      , isnull(C.SEQ,0)                as SEQ   " _
                   & " FROM       OIL.MB001_STAFF A                   " _
                   & " INNER JOIN OIL.M0006_STRUCT B              ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C             ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E           " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2  <> '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                 "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
               & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
               & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
               & " FROM       OIL.MB001_STAFF A                   " _
               & " INNER JOIN OIL.M0006_STRUCT B              ON  " _
               & "        B.CAMPCODE = A.CAMPCODE                 " _
               & "   and  B.OBJECT    = @P6                       " _
               & "   and  B.STRUCT    = @P7                       " _
               & "   and  B.GRCODE01  = @P8                       " _
               & "   and  B.STYMD    <= @P9                       " _
               & "   and  B.ENDYMD   >= @P9                       " _
               & "   and  B.DELFLG   <> '1'                       " _
               & "   and  B.CODE      = A.HORG                    " _
               & " WHERE                                          " _
               & "         A.STAFFKBN IN (                        " _
               & "            SELECT                              " _
               & "                     KEYCODE                    " _
               & "            FROM OIL.MC001_FIXVALUE E           " _
               & "            WHERE                               " _
               & "                    E.CAMPCODE = A.CAMPCODE     " _
               & "              and   E.CLASS    = 'STAFFKBN'     " _
               & "              and   E.VALUE2  <> '1'            " _
               & "              and   E.STYMD   <= @P9            " _
               & "              and   E.ENDYMD  >= @P9            " _
               & "              and   E.DELFLG  <> '1'            " _
               & "         )                                      " _
               & "   and   A.STYMD   <= @P3                       " _
               & "   and   A.ENDYMD  >= @P2                       " _
               & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES  "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE , A.STAFFKBN  "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN ,C.SEQ"
                    SQLStr = SQLStr & " ORDER BY C.SEQ , A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now

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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の社員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceInAPOrgList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                     "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & "      , isnull(C.SEQ, 0)               as SEQ   " _
                   & " FROM       OIL.MB001_STAFF  A                      " _
                   & " INNER JOIN OIL.M0006_STRUCT B                   ON " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C                 ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " WHERE                                          " _
                   & "       A.STYMD     <= @P3                       " _
                   & "   and A.ENDYMD    >= @P2                       " _
                   & "   and A.DELFLG    <> '1'                       "
            Else
                SQLStr =
                     "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " Where                                          " _
                   & "       A.STYMD     <= @P3                       " _
                   & "   and A.ENDYMD    >= @P2                       " _
                   & "   and A.DELFLG    <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            '検索SQL文
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE , A.STAFFKBN "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN, C.SEQ "
                    SQLStr = SQLStr & " ORDER BY C.SEQ , A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now
                PARA10.Value = C_ROLE_VARIANT.SERV_ORG
                PARA11.Value = TERMID

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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の乗務員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceDriverInAPOrgList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                 "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & "      , isnull(C.SEQ,0)                as SEQ   " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C                 ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2   = '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                 "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2   = '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES  "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE , A.STAFFKBN  "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN ,C.SEQ"
                    SQLStr = SQLStr & " ORDER BY C.SEQ , A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now
                PARA10.Value = C_ROLE_VARIANT.SERV_ORG
                PARA11.Value = TERMID


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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の事務員員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceClerkInAPOrgList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                 "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & "      , isnull(C.SEQ,0)                as SEQ   " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C                 ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM COM.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2  <> '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                 "SELECT  isnull(rtrim(A.STAFFCODE),'')  as CODE  " _
                   & "      , isnull(rtrim(A.STAFFNAMES),'') as NAMES " _
                   & "      , isnull(rtrim(A.STAFFKBN),'')   as KBN   " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2  <> '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES  "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE , A.STAFFKBN  "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN ,C.SEQ"
                    SQLStr = SQLStr & " ORDER BY C.SEQ , A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now
                PARA10.Value = C_ROLE_VARIANT.SERV_ORG
                PARA11.Value = TERMID


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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 組織内の社員一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub getWrkerInOrgList(ByVal SQLcon As SqlConnection)

        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            '検索SQL文
            Dim SQLStr As String =
                          " SELECT " _
                        & "        rtrim(A.STAFFCODE)  as CODE     " _
                        & "      , rtrim(A.STAFFNAMES) as NAMES    " _
                        & "      , rtrim(A.STAFFKBN)   as KBN      " _
                        & " FROM                                   " _
                        & "            OIL.MB001_STAFF        A        " _
                        & " INNER JOIN OIL.M0002_ORG          B     ON " _
                        & "            B.CAMPCODE    = A.CAMPCODE  " _
                        & "        and B.ORGCODE     = A.MORG      " _
                        & "        and B.ORGLEVEL   IN             " _
                        & "             ('01000', '00100')         " _
                        & "        and B.DELFLG     <> '1'         " _
                        & "        and B.STYMD      <= @P3         " _
                        & "        and B.ENDYMD     >= @P2         " _
                        & " INNER JOIN COM.S0006_ROLE         C     ON " _
                        & "            C.CAMPCODE    = B.CAMPCODE  " _
                        & "        and C.CODE        = B.ORGCODE   " _
                        & "        and C.OBJECT      = @P4         " _
                        & "        and C.ROLE        = @P1         " _
                        & "        and C.PERMITCODE >= @P5         " _
                        & "        and C.STYMD      <= @P3         " _
                        & "        and C.ENDYMD     >= @P2         " _
                        & "        and C.DELFLG     <> '1'         " _
                        & " INNER JOIN OIL.M0002_ORG          S     ON " _
                        & "            S.CAMPCODE    = A.CAMPCODE  " _
                        & "        and S.ORGCODE     = A.HORG      " _
                        & "        and S.ORGLEVEL    = '00010'     " _
                        & "        and S.DELFLG     <> '1'         " _
                        & "        and S.STYMD      <= @P3         " _
                        & "        and S.ENDYMD     >= @P2         " _
                        & " INNER JOIN COM.S0006_ROLE         T     ON " _
                        & "            T.CAMPCODE    = B.CAMPCODE  " _
                        & "        and T.CODE        = B.ORGCODE   " _
                        & "        and T.OBJECT      = @P4         " _
                        & "        and T.ROLE        = @P1         " _
                        & "        and T.PERMITCODE >= @P5         " _
                        & "        and T.STYMD      <= @P3         " _
                        & "        and T.ENDYMD     >= @P2         " _
                        & "        and T.DELFLG     <> '1'         " _
                        & " WHERE                                  " _
                        & "            A.STYMD      <= @P3         " _
                        & "        and A.ENDYMD     >= @P2         " _
                        & "        and A.DELFLG     <> '1'         "

            If (String.IsNullOrEmpty(Me.ORGCODE) = False) Then SQLStr &= String.Format(" and A.HORG = '{0}' ", Me.ORGCODE)
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)

            SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN  "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN  "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES , A.STAFFCODE , A.STAFFKBN "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY  A.STAFFCODE , A.STAFFKBN , A.STAFFNAMES "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Int, 1)
                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = C_ROLE_VARIANT.USER_ORG
                PARA5.Value = PERMISSION
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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 乗務員一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getDriverList(ByVal SQLcon As SqlConnection)

        '●Leftボックス用乗務員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim SQLStr As String = ""
            If String.IsNullOrEmpty(ORGCODE) Then
                ORGCODE = ""
                SQLStr =
                          " SELECT                                         " _
                        & "       rtrim(A.STAFFCODE)    as CODE            " _
                        & "      ,rtrim(A.STAFFNAMES)   as NAMES           " _
                        & "      ,rtrim(A.STAFFKBN)     as KBN             " _
                        & "      ,B.SEQ                 as SEQ             " _
                        & " FROM                                           " _
                        & "            OIL.MB001_STAFF        A                " _
                        & " INNER JOIN OIL.MB002_STAFFORG     B            ON  " _
                        & "         B.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   B.STAFFCODE          = A.STAFFCODE     " _
                        & "   and   B.DELFLG            <> '1'             " _
                        & " INNER JOIN COM.S0006_ROLE         C            ON  " _
                        & "         C.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   C.CODE               = B.SORG          " _
                        & "   and   C.OBJECT             = @P4             " _
                        & "   and   C.ROLE               = @P1             " _
                        & "   and   C.PERMITCODE        >= @P5             " _
                        & "   and   C.STYMD             <= @P3             " _
                        & "   and   C.ENDYMD            >= @P2             " _
                        & "   and   C.DELFLG            <> '1'             " _
                        & " Where                                          " _
                        & "         A.STAFFKBN IN (                        " _
                        & "            SELECT                              " _
                        & "                     KEYCODE                    " _
                        & "            FROM OIL.MC001_FIXVALUE E               " _
                        & "            WHERE                               " _
                        & "                    E.CAMPCODE = A.CAMPCODE     " _
                        & "              and   E.CLASS    = 'STAFFKBN'     " _
                        & "              and   E.VALUE2   = '1'            " _
                        & "              and   E.STYMD   <= @P3            " _
                        & "              and   E.ENDYMD  >= @P2            " _
                        & "              and   E.DELFLG  <> '1'            " _
                        & "         )                                      " _
                        & "   and   A.STYMD   <= @P3                       " _
                        & "   and   A.ENDYMD  >= @P2                       " _
                        & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                          " SELECT                                         " _
                        & "       rtrim(A.STAFFCODE)    as CODE            " _
                        & "      ,rtrim(A.STAFFNAMES)   as NAMES           " _
                        & "      ,rtrim(A.STAFFKBN)     as KBN             " _
                        & "      ,B.SEQ                 as SEQ             " _
                        & " FROM                                           " _
                        & "            OIL.MB001_STAFF        A                " _
                        & " INNER JOIN OIL.MB002_STAFFORG     B            ON  " _
                        & "         B.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   B.STAFFCODE          = A.STAFFCODE     " _
                        & "   and   B.SORG               = @P6             " _
                        & "   and   B.DELFLG            <> '1'             " _
                        & " INNER JOIN COM.S0006_ROLE         C            ON  " _
                        & "         C.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   C.CODE               = B.SORG          " _
                        & "   and   C.OBJECT             = @P4             " _
                        & "   and   C.ROLE               = @P1             " _
                        & "   and   C.PERMITCODE        >= @P5             " _
                        & "   and   C.STYMD             <= @P3             " _
                        & "   and   C.ENDYMD            >= @P2             " _
                        & "   and   C.DELFLG            <> '1'             " _
                        & " Where                                          " _
                        & "         A.STAFFKBN IN (                        " _
                        & "            SELECT                              " _
                        & "                     KEYCODE                    " _
                        & "            FROM OIL.MC001_FIXVALUE E               " _
                        & "            WHERE                               " _
                        & "                    E.CAMPCODE = A.CAMPCODE     " _
                        & "              and   E.CLASS    = 'STAFFKBN'     " _
                        & "              and   E.VALUE2   = '1'            " _
                        & "              and   E.STYMD   <= @P3            " _
                        & "              and   E.ENDYMD  >= @P2            " _
                        & "              and   E.DELFLG  <> '1'            " _
                        & "         )                                      " _
                        & "   and   A.STYMD   <= @P3                       " _
                        & "   and   A.ENDYMD  >= @P2                       " _
                        & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)

            SQLStr &= " GROUP BY A.CAMPCODE , B.SEQ , A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFNAMES , B.SEQ , A.STAFFKBN "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES , A.STAFFCODE , B.SEQ , A.STAFFKBN "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ , A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Int, 1)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = C_ROLE_VARIANT.USER_ORG
                PARA5.Value = PERMISSION
                PARA6.Value = ORGCODE
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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 事務員一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getClerkList(ByVal SQLcon As SqlConnection)

        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim SQLStr As String = ""
            If String.IsNullOrEmpty(ORGCODE) Then
                ORGCODE = ""
                SQLStr =
                          " SELECT                                         " _
                        & "       rtrim(A.STAFFCODE)    as CODE            " _
                        & "      ,rtrim(A.STAFFNAMES)   as NAMES           " _
                        & "      ,rtrim(A.STAFFKBN)     as KBN             " _
                        & "      ,B.SEQ                 as SEQ             " _
                        & " FROM                                           " _
                        & "            OIL.MB001_STAFF        A                " _
                        & " INNER JOIN OIL.MB002_STAFFORG     B            ON  " _
                        & "         B.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   B.STAFFCODE          = A.STAFFCODE     " _
                        & "   and   B.DELFLG            <> '1'             " _
                        & " INNER JOIN COM.S0006_ROLE         C            ON  " _
                        & "         C.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   C.CODE               = B.SORG          " _
                        & "   and   C.OBJECT             = @P4             " _
                        & "   and   C.ROLE               = @P1             " _
                        & "   and   C.PERMITCODE        >= @P5             " _
                        & "   and   C.STYMD             <= @P3             " _
                        & "   and   C.ENDYMD            >= @P2             " _
                        & "   and   C.DELFLG            <> '1'             " _
                        & " Where                                          " _
                        & "         A.STAFFKBN IN (                        " _
                        & "            SELECT                              " _
                        & "                     KEYCODE                    " _
                        & "            FROM OIL.MC001_FIXVALUE E               " _
                        & "            WHERE                               " _
                        & "                    E.CAMPCODE = A.CAMPCODE     " _
                        & "              and   E.CLASS    = 'STAFFKBN'     " _
                        & "              and   E.VALUE2  <> '1'            " _
                        & "              and   E.STYMD   <= @P3            " _
                        & "              and   E.ENDYMD  >= @P2            " _
                        & "              and   E.DELFLG  <> '1'            " _
                        & "         )                                      " _
                        & "   and   A.STYMD   <= @P3                       " _
                        & "   and   A.ENDYMD  >= @P2                       " _
                        & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                          " SELECT                                         " _
                        & "       rtrim(A.STAFFCODE)    as CODE            " _
                        & "      ,rtrim(A.STAFFNAMES)   as NAMES           " _
                        & "      ,rtrim(A.STAFFKBN)     as KBN             " _
                        & "      ,B.SEQ                 as SEQ             " _
                        & " FROM                                           " _
                        & "            OIL.MB001_STAFF        A                " _
                        & " INNER JOIN OIL.MB002_STAFFORG     B            ON  " _
                        & "         B.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   B.STAFFCODE          = A.STAFFCODE     " _
                        & "   and   B.SORG               = @P6             " _
                        & "   and   B.DELFLG            <> '1'             " _
                        & " INNER JOIN COM.S0006_ROLE         C            ON  " _
                        & "         C.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   C.CODE               = B.SORG          " _
                        & "   and   C.OBJECT             = @P4             " _
                        & "   and   C.ROLE               = @P1             " _
                        & "   and   C.PERMITCODE        >= @P5             " _
                        & "   and   C.STYMD             <= @P3             " _
                        & "   and   C.ENDYMD            >= @P2             " _
                        & "   and   C.DELFLG            <> '1'             " _
                        & " Where                                          " _
                        & "         A.STAFFKBN IN (                        " _
                        & "            SELECT                              " _
                        & "                     KEYCODE                    " _
                        & "            FROM OIL.MC001_FIXVALUE E               " _
                        & "            WHERE                               " _
                        & "                    E.CAMPCODE = A.CAMPCODE     " _
                        & "              and   E.CLASS    = 'STAFFKBN'     " _
                        & "              and   E.VALUE2  <> '1'            " _
                        & "              and   E.STYMD   <= @P3            " _
                        & "              and   E.ENDYMD  >= @P2            " _
                        & "              and   E.DELFLG  <> '1'            " _
                        & "         )                                      " _
                        & "   and   A.STYMD   <= @P3                       " _
                        & "   and   A.ENDYMD  >= @P2                       " _
                        & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)

            SQLStr &= " GROUP BY A.CAMPCODE , B.SEQ , A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFNAMES , B.SEQ , A.STAFFKBN "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES , A.STAFFCODE , B.SEQ , A.STAFFKBN "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ , A.STAFFCODE , A.STAFFNAMES , A.STAFFKBN "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Int, 1)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = C_ROLE_VARIANT.USER_ORG
                PARA5.Value = PERMISSION
                PARA6.Value = ORGCODE
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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub


    ''' <summary>
    ''' 社員テーブル取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getStaffTbl(ByVal SQLcon As SqlConnection, Optional ByVal length As String = "")
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try

            Dim profTbl As String(,) = {
                      {"CODE", "社員コード", "6"} _
                    , {"NAMES", "社員名称", "8"} _
                    , {"HORG", "配属部署", "6"} _
                    , {"HORGNAMES", "配属部署名称", "8"}
            }
            '検索SQL文
            Dim SQLStr As String =
                          " SELECT " _
                        & "        rtrim(A.STAFFCODE)  as CODE     " _
                        & "      , rtrim(A.STAFFNAMES) as NAMES    " _
                        & "      , rtrim(A.HORG)       as HORG     " _
                        & "      , rtrim(B.NAMES)      as HORGNAMES" _
                        & " FROM                                   " _
                        & "            OIL.MB001_STAFF       A         " _
                        & " INNER JOIN OIL.M0002_ORG         B     ON  " _
                        & "            A.HORG        = B.ORGCODE   " _
                        & "        and A.CAMPCODE    = B.CAMPCODE  " _
                        & "        and B.STYMD      <= @P3         " _
                        & "        and B.ENDYMD     >= @P2         " _
                        & "        and B.DELFLG     <> '1'         " _
                        & " WHERE                                  " _
                        & "            A.STYMD      <= @P3         " _
                        & "        and A.ENDYMD     >= @P2         " _
                        & "        and A.DELFLG     <> '1'         "
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            If (String.IsNullOrEmpty(Me.ORGCODE) = False) Then SQLStr &= String.Format(" and A.HORG = '{0}' ", Me.ORGCODE)
            If (String.IsNullOrEmpty(length) = False) Then SQLStr &= String.Format(" and LEN(A.STAFFCODE) = '{0}' ", length)

            SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,B.NAMES "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE, A.STAFFNAMES , A.HORG "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE , A.HORG "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.HORG, B.NAMES , A.STAFFCODE "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
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

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 従業員取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getEmpInOrgTbl(ByVal SQLcon As SqlConnection)

        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim profTbl As String(,) = {
          {"CODE", "社員コード", "6"} _
        , {"NAMES", "社員名称", "8"} _
        , {"HORG", "配属部署", "6"} _
        , {"HORGNAMES", "配属部署名称", "8"}
}
            '検索SQL文
            Dim SQLStr As String =
                          " SELECT " _
                        & "        rtrim(A.STAFFCODE)  as CODE     " _
                        & "      , rtrim(A.STAFFNAMES) as NAMES    " _
                        & "      , rtrim(A.HORG)       as HORG     " _
                        & "      , rtrim(S.NAMES)      as HORGNAMES" _
                        & " FROM                                   " _
                        & "            OIL.MB001_STAFF        A        " _
                        & " INNER JOIN OIL.M0002_ORG          B     ON " _
                        & "            B.CAMPCODE    = A.CAMPCODE  " _
                        & "        and B.ORGCODE     = A.MORG      " _
                        & "        and B.DELFLG     <> '1'         " _
                        & "        and B.STYMD      <= @P3         " _
                        & "        and B.ENDYMD     >= @P2         " _
                        & " INNER JOIN COM.S0006_ROLE         C     ON " _
                        & "            C.CAMPCODE    = B.CAMPCODE  " _
                        & "        and C.CODE        = B.ORGCODE   " _
                        & "        and C.OBJECT      = @P4         " _
                        & "        and C.ROLE        = @P1         " _
                        & "        and C.PERMITCODE >= @P5         " _
                        & "        and C.STYMD      <= @P3         " _
                        & "        and C.ENDYMD     >= @P2         " _
                        & "        and C.DELFLG     <> '1'         " _
                        & " INNER JOIN OIL.M0002_ORG          S     ON " _
                        & "            S.CAMPCODE    = A.CAMPCODE  " _
                        & "        and S.ORGCODE     = A.HORG      " _
                        & "        and S.DELFLG     <> '1'         " _
                        & "        and S.STYMD      <= @P3         " _
                        & "        and S.ENDYMD     >= @P2         " _
                        & " INNER JOIN COM.S0006_ROLE         T     ON " _
                        & "            T.CAMPCODE    = B.CAMPCODE  " _
                        & "        and T.CODE        = B.ORGCODE   " _
                        & "        and T.OBJECT      = @P4         " _
                        & "        and T.ROLE        = @P1         " _
                        & "        and T.PERMITCODE >= @P5         " _
                        & "        and T.STYMD      <= @P3         " _
                        & "        and T.ENDYMD     >= @P2         " _
                        & "        and T.DELFLG     <> '1'         " _
                        & " WHERE                                  " _
                        & "            A.STYMD      <= @P3         " _
                        & "        and A.ENDYMD     >= @P2         " _
                        & "        and A.DELFLG     <> '1'         "
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then
                SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            End If
            If (String.IsNullOrEmpty(Me.ORGCODE) = False) Then
                SQLStr &= String.Format(" and A.HORG = '{0}' ", Me.ORGCODE)
            End If
            If Not IsNothing(Me.STAFFKBN) Then
                Dim KBN_IN As String = String.Empty

                For Each kbn As String In Me.STAFFKBN
                    KBN_IN = If(String.IsNullOrEmpty(KBN_IN), "", KBN_IN & ",") & "'" & kbn & "'"
                Next
                SQLStr &= String.Format(" and A.STAFFKBN IN ({0}) ", KBN_IN)
            End If
            SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE, A.STAFFNAMES , A.HORG "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES, A.STAFFCODE , A.HORG "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY A.HORG, B.NAMES , A.STAFFCODE "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Int, 1)
                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = C_ROLE_VARIANT.USER_ORG
                PARA5.Value = PERMISSION
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                Dim srcData As New DataTable
                srcData.Load(SQLdr)

                MakeTableObject(profTbl, srcData, AREA)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' （管理部署・所属部署）組織内の社員一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getWrkerInOrgTbl(ByVal SQLcon As SqlConnection)

        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim profTbl As String(,) = {
                  {"CODE", "社員コード", "6"} _
                , {"NAMES", "社員名称", "8"} _
                , {"HORG", "配属部署", "6"} _
                , {"HORGNAMES", "配属部署名称", "8"}
            }
            '検索SQL文
            Dim SQLStr As String =
                          " SELECT " _
                        & "        rtrim(A.STAFFCODE)  as CODE     " _
                        & "      , rtrim(A.STAFFNAMES) as NAMES    " _
                        & "      , rtrim(A.HORG)       as HORG     " _
                        & "      , rtrim(S.NAMES)      as HORGNAMES" _
                        & " FROM                                   " _
                        & "            OIL.MB001_STAFF        A        " _
                        & " INNER JOIN OIL.M0002_ORG          B     ON " _
                        & "            B.CAMPCODE    = A.CAMPCODE  " _
                        & "        and B.ORGCODE     = A.MORG      " _
                        & "        and B.ORGLEVEL   IN             " _
                        & "             ('01000', '00100')         " _
                        & "        and B.DELFLG     <> '1'         " _
                        & "        and B.STYMD      <= @P3         " _
                        & "        and B.ENDYMD     >= @P2         " _
                        & " INNER JOIN COM.S0006_ROLE         C     ON " _
                        & "            C.CAMPCODE    = B.CAMPCODE  " _
                        & "        and C.CODE        = B.ORGCODE   " _
                        & "        and C.OBJECT      = @P4         " _
                        & "        and C.ROLE        = @P1         " _
                        & "        and C.PERMITCODE >= @P5         " _
                        & "        and C.STYMD      <= @P3         " _
                        & "        and C.ENDYMD     >= @P2         " _
                        & "        and C.DELFLG     <> '1'         " _
                        & " INNER JOIN OIL.M0002_ORG          S     ON " _
                        & "            S.CAMPCODE    = A.CAMPCODE  " _
                        & "        and S.ORGCODE     = A.HORG      " _
                        & "        and S.ORGLEVEL    = '00010'     " _
                        & "        and S.DELFLG     <> '1'         " _
                        & "        and S.STYMD      <= @P3         " _
                        & "        and S.ENDYMD     >= @P2         " _
                        & " INNER JOIN COM.S0006_ROLE         T     ON " _
                        & "            T.CAMPCODE    = B.CAMPCODE  " _
                        & "        and T.CODE        = B.ORGCODE   " _
                        & "        and T.OBJECT      = @P4         " _
                        & "        and T.ROLE        = @P1         " _
                        & "        and T.PERMITCODE >= @P5         " _
                        & "        and T.STYMD      <= @P3         " _
                        & "        and T.ENDYMD     >= @P2         " _
                        & "        and T.DELFLG     <> '1'         " _
                        & " WHERE                                  " _
                        & "            A.STYMD      <= @P3         " _
                        & "        and A.ENDYMD     >= @P2         " _
                        & "        and A.DELFLG     <> '1'         "

            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then
                SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            End If
            If (String.IsNullOrEmpty(Me.ORGCODE) = False) Then
                SQLStr &= String.Format(" and A.HORG = '{0}' ", Me.ORGCODE)
            End If
            If Not IsNothing(Me.STAFFKBN) Then
                Dim KBN_IN As String = String.Empty

                For Each kbn As String In Me.STAFFKBN
                    KBN_IN = If(String.IsNullOrEmpty(KBN_IN), "", KBN_IN & ",") & "'" & kbn & "'"
                Next
                SQLStr &= String.Format(" and A.STAFFKBN IN ({0}) ", KBN_IN)
            End If
            SQLStr &= "GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
            SQLStr &= "ORDER BY A.STAFFCODE, A.STAFFNAMES , A.HORG "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Int, 1)
            PARA1.Value = ROLECODE
            PARA2.Value = STYMD
            PARA3.Value = ENDYMD
            PARA4.Value = C_ROLE_VARIANT.USER_ORG
            PARA5.Value = PERMISSION
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
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 組織内の社員一覧取得(作業部署内の運転手）
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getDriverTbl(ByVal SQLcon As SqlConnection)

        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim profTbl As String(,) = {
                  {"CODE", "社員コード", "6"} _
                , {"NAMES", "社員名称", "8"} _
                , {"HORG", "配属部署", "6"} _
                , {"HORGNAMES", "配属部署名称", "8"}
            }
            Dim SQLStr As String = ""
            If String.IsNullOrEmpty(ORGCODE) Then
                ORGCODE = ""
                SQLStr =
                          " SELECT                                         " _
                        & "        rtrim(A.STAFFCODE)  as CODE             " _
                        & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                        & "      , rtrim(A.HORG)       as HORG             " _
                        & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                        & "      , B.SEQ               as SEQ              " _
                        & " FROM                                           " _
                        & "            OIL.MB001_STAFF        A                " _
                        & " INNER JOIN OIL.MB002_STAFFORG     B            ON  " _
                        & "         B.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   B.STAFFCODE          = A.STAFFCODE     " _
                        & "   and   B.DELFLG            <> '1'             " _
                        & " INNER JOIN COM.S0006_ROLE         C            ON  " _
                        & "         C.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   C.CODE               = B.SORG          " _
                        & "   and   C.OBJECT             = @P4             " _
                        & "   and   C.ROLE               = @P1             " _
                        & "   and   C.PERMITCODE        >= @P5             " _
                        & "   and   C.STYMD             <= @P3             " _
                        & "   and   C.ENDYMD            >= @P2             " _
                        & "   and   C.DELFLG            <> '1'             " _
                        & " INNER JOIN COM.M0002_ORG          S            ON  " _
                        & "            S.CAMPCODE        = A.CAMPCODE      " _
                        & "        and S.ORGCODE         = A.HORG          " _
                        & "        and S.DELFLG         <> '1'             " _
                        & "        and S.STYMD          <= @P3             " _
                        & "        and S.ENDYMD         >= @P2             " _
                        & " Where                                          " _
                        & "         A.STAFFKBN IN (                        " _
                        & "            SELECT                              " _
                        & "                     KEYCODE                    " _
                        & "            FROM COM.MC001_FIXVALUE E               " _
                        & "            WHERE                               " _
                        & "                    E.CAMPCODE = A.CAMPCODE     " _
                        & "              and   E.CLASS    = 'STAFFKBN'     " _
                        & "              and   E.VALUE2   = '1'            " _
                        & "              and   E.STYMD   <= @P3            " _
                        & "              and   E.ENDYMD  >= @P2            " _
                        & "              and   E.DELFLG  <> '1'            " _
                        & "         )                                      " _
                        & "   and   A.STYMD   <= @P3                       " _
                        & "   and   A.ENDYMD  >= @P2                       " _
                        & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                          " SELECT                                         " _
                        & "        rtrim(A.STAFFCODE)  as CODE             " _
                        & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                        & "      , rtrim(A.HORG)       as HORG             " _
                        & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                        & "      , B.SEQ               as SEQ              " _
                        & " FROM                                           " _
                        & "            OIL.MB001_STAFF        A                " _
                        & " INNER JOIN OIL.MB002_STAFFORG     B            ON  " _
                        & "         B.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   B.STAFFCODE          = A.STAFFCODE     " _
                        & "   and   B.SORG               = @P6             " _
                        & "   and   B.DELFLG            <> '1'             " _
                        & " INNER JOIN COM.S0006_ROLE         C            ON  " _
                        & "         C.CAMPCODE           = A.CAMPCODE      " _
                        & "   and   C.CODE               = B.SORG          " _
                        & "   and   C.OBJECT             = @P4             " _
                        & "   and   C.ROLE               = @P1             " _
                        & "   and   C.PERMITCODE        >= @P5             " _
                        & "   and   C.STYMD             <= @P3             " _
                        & "   and   C.ENDYMD            >= @P2             " _
                        & "   and   C.DELFLG            <> '1'             " _
                        & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                        & "            S.CAMPCODE        = A.CAMPCODE      " _
                        & "        and S.ORGCODE         = A.HORG          " _
                        & "        and S.DELFLG         <> '1'             " _
                        & "        and S.STYMD          <= @P3             " _
                        & "        and S.ENDYMD         >= @P2             " _
                        & " Where                                          " _
                        & "         A.STAFFKBN IN (                        " _
                        & "            SELECT                              " _
                        & "                     KEYCODE                    " _
                        & "            FROM OIL.MC001_FIXVALUE E               " _
                        & "            WHERE                               " _
                        & "                    E.CAMPCODE = A.CAMPCODE     " _
                        & "              and   E.CLASS    = 'STAFFKBN'     " _
                        & "              and   E.VALUE2   = '1'            " _
                        & "              and   E.STYMD   <= @P3            " _
                        & "              and   E.ENDYMD  >= @P2            " _
                        & "              and   E.DELFLG  <> '1'            " _
                        & "         )                                      " _
                        & "   and   A.STYMD   <= @P3                       " _
                        & "   and   A.ENDYMD  >= @P2                       " _
                        & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then
                SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            End If
            If Not IsNothing(Me.STAFFKBN) Then
                Dim KBN_IN As String = String.Empty

                For Each kbn As String In Me.STAFFKBN
                    KBN_IN = If(String.IsNullOrEmpty(KBN_IN), "", KBN_IN & ",") & "'" & kbn & "'"
                Next
                SQLStr &= String.Format(" and A.STAFFKBN IN ({0}) ", KBN_IN)
            End If

            SQLStr &= "GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES ,B.SEQ "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE , A.STAFFNAMES , B.SEQ , A.HORG "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES , A.STAFFCODE , B.SEQ , A.HORG "
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    SQLStr = SQLStr & " ORDER BY B.SEQ , A.STAFFCODE , A.STAFFNAMES , A.HORG "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Int, 1)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA4.Value = C_ROLE_VARIANT.USER_ORG
                PARA5.Value = PERMISSION
                PARA6.Value = ORGCODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                Dim srcData As New DataTable
                srcData.Load(SQLdr)

                MakeTableObject(profTbl, srcData, AREA)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の社員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceTbl(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim profTbl As String(,) = {
                  {"CODE", "社員コード", "6"} _
                , {"NAMES", "社員名称", "8"} _
                , {"HORG", "配属部署", "6"} _
                , {"HORGNAMES", "配属部署名称", "8"}
            }
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & "      , isnull(C.SEQ, 0)    as CSEQ             " _
                   & "      , isnull(B.SEQ, 0)    as BSEQ             " _
                   & " FROM       OIL.MB001_STAFF  A                      " _
                   & " INNER JOIN OIL.M0006_STRUCT B                   ON " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C                 ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " WHERE                                          " _
                   & "       A.STYMD     <= @P3                       " _
                   & "   and A.ENDYMD    >= @P2                       " _
                   & "   and A.DELFLG    <> '1'                       "
            Else
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "        S.CAMPCODE  = A.CAMPCODE                " _
                   & "   and S.ORGCODE    = A.HORG                    " _
                   & "   and S.DELFLG    <> '1'                       " _
                   & "   and S.STYMD     <= @P3                       " _
                   & "   and S.ENDYMD    >= @P2                       " _
                   & " Where                                          " _
                   & "       A.STYMD     <= @P3                       " _
                   & "   and A.ENDYMD    >= @P2                       " _
                   & "   and A.DELFLG    <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            If Not IsNothing(Me.STAFFKBN) Then
                Dim KBN_IN As String = String.Empty

                For Each kbn As String In Me.STAFFKBN
                    KBN_IN = If(String.IsNullOrEmpty(KBN_IN), "", KBN_IN & ",") & "'" & kbn & "'"
                Next
                SQLStr &= String.Format(" and A.STAFFKBN IN ({0}) ", KBN_IN)
            End If
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES,S.NAMES , A.STAFFCOD , A.HORG  "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.HORG ,S.NAMES, C.SEQ ,  B.SEQ "
                    SQLStr = SQLStr & " ORDER BY B.SEQ , C.SEQ , A.STAFFCODE , A.HORG "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                Dim srcData As New DataTable
                srcData.Load(SQLdr)

                MakeTableObject(profTbl, srcData, AREA)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の乗務員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceDriverTbl(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim profTbl As String(,) = {
                  {"CODE", "社員コード", "6"} _
                , {"NAMES", "社員名称", "8"} _
                , {"HORG", "配属部署", "6"} _
                , {"HORGNAMES", "配属部署名称", "8"}
            }
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & "      , isnull(C.SEQ, 0)    as CSEQ             " _
                   & "      , isnull(B.SEQ, 0)    as BSEQ             " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C                 ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2   = '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2   = '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            If Not IsNothing(Me.STAFFKBN) Then
                Dim KBN_IN As String = String.Empty

                For Each kbn As String In Me.STAFFKBN
                    KBN_IN = If(String.IsNullOrEmpty(KBN_IN), "", KBN_IN & ",") & "'" & kbn & "'"
                Next
                SQLStr &= String.Format(" and A.STAFFKBN IN ({0}) ", KBN_IN)
            End If
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES,S.NAMES , A.STAFFCOD , A.HORG  "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.HORG ,S.NAMES, C.SEQ ,  B.SEQ "
                    SQLStr = SQLStr & " ORDER BY B.SEQ , C.SEQ , A.STAFFCODE , A.HORG "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                Dim srcData As New DataTable
                srcData.Load(SQLdr)

                MakeTableObject(profTbl, srcData, AREA)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の事務員員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceClerkTbl(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim profTbl As String(,) = {
                  {"CODE", "社員コード", "6"} _
                , {"NAMES", "社員名称", "8"} _
                , {"HORG", "配属部署", "6"} _
                , {"HORGNAMES", "配属部署名称", "8"}
            }
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & "      , isnull(C.SEQ, 0)    as CSEQ             " _
                   & "      , isnull(B.SEQ, 0)    as BSEQ             " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C                 ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2  <> '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2  <> '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            If Not IsNothing(Me.STAFFKBN) Then
                Dim KBN_IN As String = String.Empty

                For Each kbn As String In Me.STAFFKBN
                    KBN_IN = If(String.IsNullOrEmpty(KBN_IN), "", KBN_IN & ",") & "'" & kbn & "'"
                Next
                SQLStr &= String.Format(" and A.STAFFKBN IN ({0}) ", KBN_IN)
            End If

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES,S.NAMES , A.STAFFCOD , A.HORG  "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.HORG ,S.NAMES, C.SEQ ,  B.SEQ "
                    SQLStr = SQLStr & " ORDER BY B.SEQ , C.SEQ , A.STAFFCODE , A.HORG "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                Dim srcData As New DataTable
                srcData.Load(SQLdr)

                MakeTableObject(profTbl, srcData, AREA)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の社員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceInAPOrgTbl(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim profTbl As String(,) = {
                  {"CODE", "社員コード", "6"} _
                , {"NAMES", "社員名称", "8"} _
                , {"HORG", "配属部署", "6"} _
                , {"HORGNAMES", "配属部署名称", "8"}
            }
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & "      , isnull(C.SEQ, 0)    as CSEQ             " _
                   & "      , isnull(B.SEQ, 0)    as BSEQ             " _
                   & " FROM       OIL.MB001_STAFF  A                      " _
                   & " INNER JOIN OIL.M0006_STRUCT B                   ON " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C                 ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " WHERE                                          " _
                   & "       A.STYMD     <= @P3                       " _
                   & "   and A.ENDYMD    >= @P2                       " _
                   & "   and A.DELFLG    <> '1'                       "
            Else
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " Where                                          " _
                   & "       A.STYMD     <= @P3                       " _
                   & "   and A.ENDYMD    >= @P2                       " _
                   & "   and A.DELFLG    <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            If Not IsNothing(Me.STAFFKBN) Then
                Dim KBN_IN As String = String.Empty

                For Each kbn As String In Me.STAFFKBN
                    KBN_IN = If(String.IsNullOrEmpty(KBN_IN), "", KBN_IN & ",") & "'" & kbn & "'"
                Next
                SQLStr &= String.Format(" and A.STAFFKBN IN ({0}) ", KBN_IN)
            End If

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES,S.NAMES , A.STAFFCOD , A.HORG  "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.HORG ,S.NAMES, C.SEQ ,  B.SEQ "
                    SQLStr = SQLStr & " ORDER BY B.SEQ , C.SEQ , A.STAFFCODE , A.HORG "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now
                PARA10.Value = C_ROLE_VARIANT.SERV_ORG
                PARA11.Value = TERMID

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                Dim srcData As New DataTable
                srcData.Load(SQLdr)

                MakeTableObject(profTbl, srcData, AREA)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の乗務員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceDriverInAPOrgTbl(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim profTbl As String(,) = {
                  {"CODE", "社員コード", "6"} _
                , {"NAMES", "社員名称", "8"} _
                , {"HORG", "配属部署", "6"} _
                , {"HORGNAMES", "配属部署名称", "8"}
            }
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & "      , isnull(C.SEQ, 0)    as CSEQ             " _
                   & "      , isnull(B.SEQ, 0)    as BSEQ             " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C                 ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2   = '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2   = '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            If Not IsNothing(Me.STAFFKBN) Then
                Dim KBN_IN As String = String.Empty

                For Each kbn As String In Me.STAFFKBN
                    KBN_IN = If(String.IsNullOrEmpty(KBN_IN), "", KBN_IN & ",") & "'" & kbn & "'"
                Next
                SQLStr &= String.Format(" and A.STAFFKBN IN ({0}) ", KBN_IN)
            End If

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES,S.NAMES , A.STAFFCOD , A.HORG  "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.HORG ,S.NAMES, C.SEQ ,  B.SEQ "
                    SQLStr = SQLStr & " ORDER BY B.SEQ , C.SEQ , A.STAFFCODE , A.HORG "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now
                PARA10.Value = C_ROLE_VARIANT.SERV_ORG
                PARA11.Value = TERMID


                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                Dim srcData As New DataTable
                srcData.Load(SQLdr)

                MakeTableObject(profTbl, srcData, AREA)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 勤怠対象の事務員一覧取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub getAttendanceClerkInAPOrgTbl(ByVal SQLcon As SqlConnection)
        '●Leftボックス用従業員取得
        '○ User権限によりDB(MB001_STAFF)検索
        Try
            Dim profTbl As String(,) = {
                  {"CODE", "社員コード", "6"} _
                , {"NAMES", "社員名称", "8"} _
                , {"HORG", "配属部署", "6"} _
                , {"HORGNAMES", "配属部署名称", "8"}
            }
            Dim SQLStr As String = String.Empty
            If DEFAULT_SORT = C_DEFAULT_SORT.SEQ Then
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & "      , isnull(C.SEQ, 0)    as CSEQ             " _
                   & "      , isnull(B.SEQ, 0)    as BSEQ             " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                    ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " LEFT  JOIN OIL.MB002_STAFFORG C                 ON " _
                   & "        C.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  C.STAFFCODE = A.STAFFCODE               " _
                   & "   and  C.SORG      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2  <> '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            Else
                SQLStr =
                     "SELECT                                          " _
                   & "        rtrim(A.STAFFCODE)  as CODE             " _
                   & "      , rtrim(A.STAFFNAMES) as NAMES            " _
                   & "      , rtrim(A.HORG)       as HORG             " _
                   & "      , rtrim(S.NAMES)      as HORGNAMES        " _
                   & " FROM       OIL.MB001_STAFF A                       " _
                   & " INNER JOIN OIL.M0006_STRUCT B                  ON  " _
                   & "        B.CAMPCODE = A.CAMPCODE                 " _
                   & "   and  B.OBJECT    = @P6                       " _
                   & "   and  B.STRUCT    = @P7                       " _
                   & "   and  B.GRCODE01  = @P8                       " _
                   & "   and  B.STYMD    <= @P9                       " _
                   & "   and  B.ENDYMD   >= @P9                       " _
                   & "   and  B.DELFLG   <> '1'                       " _
                   & "   and  B.CODE      = A.HORG                    " _
                   & " INNER JOIN COM.S0012_SRVAUTHOR X                ON " _
                   & "        X.CAMPCODE  = A.CAMPCODE                " _
                   & "   and  X.OBJECT    = @P10                      " _
                   & "   and  X.TERMID    = @P11                      " _
                   & "   and  X.STYMD    <= @P9                       " _
                   & "   and  X.ENDYMD   >= @P9                       " _
                   & "   and  X.DELFLG   <> '1'                       " _
                   & " INNER JOIN COM.S0006_ROLE      Y                ON " _
                   & "         Y.CAMPCODE = X.CAMPCODE                " _
                   & "   and   Y.OBJECT   = X.OBJECT                  " _
                   & "   and   Y.ROLE     = X.ROLE                    " _
                   & "   and   Y.STYMD   <= @P9                       " _
                   & "   and   Y.ENDYMD  >= @P9                       " _
                   & "   and   Y.CODE     = A.HORG                    " _
                   & "   and   Y.DELFLG  <> '1'                       " _
                   & " INNER JOIN OIL.M0002_ORG          S            ON  " _
                   & "            S.CAMPCODE        = A.CAMPCODE      " _
                   & "        and S.ORGCODE         = A.HORG          " _
                   & "        and S.DELFLG         <> '1'             " _
                   & "        and S.STYMD          <= @P3             " _
                   & "        and S.ENDYMD         >= @P2             " _
                   & " WHERE                                          " _
                   & "         A.STAFFKBN IN (                        " _
                   & "            SELECT                              " _
                   & "                     KEYCODE                    " _
                   & "            FROM OIL.MC001_FIXVALUE E               " _
                   & "            WHERE                               " _
                   & "                    E.CAMPCODE = A.CAMPCODE     " _
                   & "              and   E.CLASS    = 'STAFFKBN'     " _
                   & "              and   E.VALUE2  <> '1'            " _
                   & "              and   E.STYMD   <= @P9            " _
                   & "              and   E.ENDYMD  >= @P9            " _
                   & "              and   E.DELFLG  <> '1'            " _
                   & "         )                                      " _
                   & "   and   A.STYMD   <= @P3                       " _
                   & "   and   A.ENDYMD  >= @P2                       " _
                   & "   and   A.DELFLG  <> '1'                       "
            End If
            If (String.IsNullOrEmpty(Me.CAMPCODE) = False) Then SQLStr &= String.Format(" and A.CAMPCODE = '{0}' ", Me.CAMPCODE)
            If Not IsNothing(Me.STAFFKBN) Then
                Dim KBN_IN As String = String.Empty

                For Each kbn As String In Me.STAFFKBN
                    KBN_IN = If(String.IsNullOrEmpty(KBN_IN), "", KBN_IN & ",") & "'" & kbn & "'"
                Next
                SQLStr &= String.Format(" and A.STAFFKBN IN ({0}) ", KBN_IN)
            End If

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr &= " GROUP BY A.STAFFCODE, A.STAFFNAMES , A.HORG ,S.NAMES "
                    SQLStr = SQLStr & " ORDER BY A.STAFFNAMES,S.NAMES , A.STAFFCOD , A.HORG  "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr &= "GROUP BY  A.STAFFCODE , A.STAFFNAMES , A.HORG ,S.NAMES, C.SEQ ,  B.SEQ "
                    SQLStr = SQLStr & " ORDER BY B.SEQ , C.SEQ , A.STAFFCODE , A.HORG "
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.Date)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = ROLECODE
                PARA2.Value = STYMD
                PARA3.Value = ENDYMD
                PARA6.Value = C_ROLE_VARIANT.USER_ORG
                PARA7.Value = C_STRUCT_CODE.ATTENDANCE_CODE
                PARA8.Value = Me.ORGCODE
                PARA9.Value = Date.Now
                PARA10.Value = C_ROLE_VARIANT.SERV_ORG
                PARA11.Value = TERMID


                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                Dim srcData As New DataTable
                srcData.Load(SQLdr)

                MakeTableObject(profTbl, srcData, AREA)
                ERR = C_MESSAGE_NO.NORMAL
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 一覧登録時のチェック処理
    ''' </summary>
    ''' <param name="I_SQLDR"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Overrides Function extracheck(ByVal I_SQLDR As SqlDataReader)
        Return (IsNothing(Me.STAFFKBN) OrElse STAFFKBN.Contains(I_SQLDR("KBN"))) AndAlso
               (String.IsNullOrEmpty(Me.STAFFCODE) OrElse STAFFCODE = I_SQLDR("CODE"))

    End Function
End Class


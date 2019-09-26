Imports System.Data.SqlClient

Public Class GRTA0006WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "TA0006S"                     'MAPID(選択)
    Public Const MAPID As String = "TA0006"                       'MAPID(実行)

    Public Const C_KINTAI_ALL_CODE As String = "*_勤怠ALL"        '勤怠入力（全体／個別）の判定

    Private T0007COM As New GRT0007COM                            '勤怠共通
    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

    End Sub

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="FIXCODE">固定値コード</param>
    ''' <returns>検索条件テーブル</returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        Return prmData
    End Function
    ''' <summary>
    ''' 部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="PRMIT">権限区分</param>
    ''' <returns>検索条件テーブル</returns>
    ''' <remarks></remarks>
    Function CreateHORGParam(ByVal COMPCODE As String, ByVal PRMIT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE, GL0002OrgList.C_CATEGORY_LIST.OFFICE_PLACE, GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT}
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = PRMIT
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        Return prmData
    End Function
    ''' <summary>
    ''' 乗務員一覧取得
    ''' </summary>
    ''' <param name="CAMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="TAISHOYM">対象年月</param>
    ''' <param name="USERID">ユーザID</param>
    ''' <param name="MAPVARI">画面表示用文字列</param>
    ''' <returns>検索条件テーブル</returns>
    ''' <remarks></remarks>
    Function GetStaffCodeList(ByVal CAMPCODE As String, ByVal ORGCODE As String, ByVal TAISHOYM As String, ByVal USERID As String, ByVal MAPVARI As String) As Hashtable
        Dim prmData As New Hashtable
        Dim wDATE As Date
        Dim WW_ORG As String = String.Empty
        Dim WW_RTN As String = String.Empty
        Dim WW_CODE As String = String.Empty
        '袖２の場合、袖１に変換
        Dim orgCngCode As String = ""
        Dim retCode As String = ""
        T0007COM.ConvORGCODE(CAMPCODE, ORGCODE, orgCngCode, retCode)
        If retCode = C_MESSAGE_NO.NORMAL Then
            ORGCODE = orgCngCode
        End If

        '勤怠ＡＬＬまたは、共通ユーザーＩＤ以外は自分以外除去
        If WF_SEL_ALL_ATTEND.Text <> "ALL" Then GetSTAFFCODE(CAMPCODE, USERID, WW_ORG, WW_CODE, WW_RTN)

        Try
            wDATE = TAISHOYM & "/01"
        Catch ex As Exception
            wDATE = Date.Now
        End Try
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = CAMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK_IN_AORG
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_STYMD) = TAISHOYM & "/01"
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ENDYMD) = TAISHOYM & "/" & DateTime.DaysInMonth(wDATE.Year, wDATE.Month).ToString("00")
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_SELECTED_CODE) = WW_CODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_DEFAULT_SORT) = GL0005StaffList.C_DEFAULT_SORT.SEQ
        Return prmData

    End Function
    ''' <summary>
    ''' 乗務員一覧取得
    ''' </summary>
    ''' <param name="CAMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="TAISHOYM">対象年月</param>
    ''' <returns>検索条件テーブル</returns>
    ''' <remarks></remarks>
    Function GetStaffCodeAllList(ByVal CAMPCODE As String, ByVal ORGCODE As String, ByVal TAISHOYM As String) As Hashtable

        Dim prmData As New Hashtable
        Dim wDATE As Date

        Try
            wDATE = TAISHOYM & "/01"
        Catch ex As Exception
            wDATE = Date.Now
        End Try
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = CAMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_STYMD) = TAISHOYM & "/01"
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ENDYMD) = TAISHOYM & "/" & DateTime.DaysInMonth(wDATE.Year, wDATE.Month).ToString("00")
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_DEFAULT_SORT) = GL0005StaffList.C_DEFAULT_SORT.SEQ
        Return prmData

    End Function
    ''' <summary>
    ''' 従業員番号取得処理
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="USERID">ユーザID</param>
    ''' <param name="O_ORG">取得した部署コード</param>
    ''' <param name="O_STAFFCODE">取得した社員コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Public Sub GetSTAFFCODE(ByVal COMPCODE As String, ByVal USERID As String, ByRef O_ORG As String, ByRef O_STAFFCODE As String, ByRef O_RTN As String)

        O_ORG = ""
        O_STAFFCODE = ""
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim CS0050Session As New CS0050SESSION

        Try

            '○　従業員ListBox設定                
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                Dim PARA(10) As SqlParameter

                SQLcon.Open() 'DataBase接続(Open)
                '検索SQL文
                Dim SQLStr As String =
                     "SELECT isnull(rtrim(B.GRCODE01),'')  as ORG       " _
                   & "      ,isnull(rtrim(A.STAFFCODE),'') as STAFFCODE " _
                   & " FROM       S0004_USER   A                        " _
                   & " INNER JOIN M0006_STRUCT B                        " _
                   & "    ON   B.CAMPCODE     = @P3                     " _
                   & "   and   B.OBJECT       = 'ORG'                   " _
                   & "   and   B.STRUCT       = '勤怠管理組織'          " _
                   & "   and   B.CODE         = A.ORG                   " _
                   & "   and   B.STYMD       <= @P2                     " _
                   & "   and   B.ENDYMD      >= @P2                     " _
                   & "   and   B.DELFLG      <> '1'                     " _
                   & " WHERE   A.USERID       = @P1                     " _
                   & "   and   A.STYMD       <= @P2                     " _
                   & "   and   A.ENDYMD      >= @P2                     " _
                   & "   and   A.DELFLG      <> '1'                     "

                Using SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    PARA(1).Value = USERID
                    PARA(2).Value = Date.Now
                    PARA(3).Value = COMPCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            O_ORG = SQLdr("ORG")
                            O_STAFFCODE = SQLdr("STAFFCODE")
                        End While
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Dim CS0011LOGWrite As New CS0011LOGWrite
            O_RTN = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0004_USER Select"             '
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = O_RTN
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

End Class
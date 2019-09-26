Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRTA0002WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "TA0002S"                       'MAPID(選択)
    Public Const MAPID As String = "TA0002"                         'MAPID(実行)
    Public Const MAPIDNJS As String = "TA0002NJS"                   'MAPID(実行)
    Public Const MAPIDKNK As String = "TA0002KNK"                   'MAPID(実行)
    Public Const MAPIDJKT As String = "TA0002JKT"                   'MAPID(実行)

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

    End Sub

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        CreateFIXParam = prmData
    End Function

    ''' <summary>
    ''' 配属部署パラメーター
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    Function CreateHORGParam(ByVal COMPCODE As String, ByVal PRMIT As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = PRMIT
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateHORGParam = prmData

    End Function

    ''' <summary>
    ''' 職務区分パラメーター
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateStaffKbnParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        Dim StaffKbnList As New ListBox
        Dim PARA(10) As SqlParameter
        Dim SQLStr As New StringBuilder
        Using SQLcon = CS0050SESSION.getConnection()
            SQLcon.Open()

            '○ 職務区分リストボックス作成
            Try

                '検索SQL文
                SQLStr.AppendLine(" SELECT rtrim(KEYCODE) as KEYCODE    ")
                SQLStr.AppendLine("       ,rtrim(VALUE1)  as VALUE1     ")
                SQLStr.AppendLine(" FROM  MC001_FIXVALUE                ")
                SQLStr.AppendLine(" Where CAMPCODE  = @P1               ")
                SQLStr.AppendLine("   and CLASS     = @P2               ")
                SQLStr.AppendLine("   and STYMD    <= @P3               ")
                SQLStr.AppendLine("   and ENDYMD   >= @P4               ")
                SQLStr.AppendLine("   and DELFLG   <> @P5               ")
                SQLStr.AppendLine("   and KEYCODE LIKE '03%'            ")
                SQLStr.AppendLine("ORDER BY KEYCODE                     ")

                Using SQLcmd = New SqlCommand(SQLStr.ToString, SQLcon)
                    PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                    PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    PARA(4) = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    PARA(5) = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar)
                    PARA(1).Value = I_COMPCODE
                    PARA(2).Value = "STAFFKBN"
                    PARA(3).Value = Date.Now
                    PARA(4).Value = Date.Now
                    PARA(5).Value = C_DELETE_FLG.DELETE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            StaffKbnList.Items.Add(New ListItem(SQLdr("VALUE1"), SQLdr("KEYCODE")))
                        End While
                    End Using
                    prmData.Item(C_PARAMETERS.LP_LIST) = StaffKbnList
                End Using
            Finally

            End Try
        End Using
        CreateStaffKbnParam = prmData

    End Function

    ''' <summary>
    ''' 乗務員一覧取得
    ''' </summary>
    ''' <param name="CAMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="TAISHOYM">対象年月</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function GetStaffCodeList(ByVal CAMPCODE As String, ByVal TAISHOYM As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        Dim wDATE As Date
        Try
            wDATE = TAISHOYM & "/01"
        Catch ex As Exception
            wDATE = Date.Now
        End Try
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = CAMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER_IN_AORG
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_STYMD) = TAISHOYM & "/" & "01"
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ENDYMD) = TAISHOYM & "/" & DateTime.DaysInMonth(wDATE.Year, wDATE.Month).ToString("00")
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_DEFAULT_SORT) = GL0000.C_DEFAULT_SORT.SEQ
        Return prmData
    End Function
End Class
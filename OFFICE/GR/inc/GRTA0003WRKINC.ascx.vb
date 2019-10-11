Imports System.Data.SqlClient

Public Class GRTA0003WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDS As String = "TA0003S"                        'MAPID(選択)
    Public Const MAPID As String = "TA0003"                          'MAPID(実行)

    Public Const CONST_CAMP_ENEX As String = "02"                          '会社コード（エネックス）
    Public Const CONST_CAMP_KNK As String = "03"                           '会社コード（近石）
    Public Const CONST_CAMP_NJS As String = "04"                           '会社コード（NJS）
    Public Const CONST_CAMP_JKT As String = "05"                           '会社コード（JKT）

    Protected STAFFTbl As New Hashtable

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
    ''' <returns></returns>
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
    ''' <param name="PARMIT">権限区分</param>
    ''' <param name="SHUKADATE">出荷日</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateHORGParam(ByVal COMPCODE As String, ByVal PARMIT As String, ByVal SHUKADATE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = PARMIT
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_STYMD) = SHUKADATE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ENDYMD) = SHUKADATE
        Return prmData
    End Function
    ''' <summary>
    ''' 社員一覧取得
    ''' </summary>
    ''' <param name="CAMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="TAISHOYM">対象年月</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function GetStaffCodeList(ByVal CAMPCODE As String, ByVal ORGCODE As String, ByVal TAISHOYM As String) As Hashtable
        Dim prmData As New Hashtable
        Dim wDATE As Date
        Try
            wDATE = TAISHOYM & "/01"
        Catch ex As Exception
            wDATE = Date.Now
        End Try
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = CAMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_ALL
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_STYMD) = TAISHOYM & "/01"
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ENDYMD) = TAISHOYM & "/" & DateTime.DaysInMonth(wDATE.Year, wDATE.Month).ToString("00")
        Return prmData
    End Function

    ''' <summary>
    ''' 会社毎の平日残業時の丸め方を返す
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="O_SPLIT_MINUITE">丸め時間</param>
    ''' <param name="O_ROUND_TYPE">
    ''' <para>丸め方</para>
    ''' <para>FLOOR:切下げ</para>
    ''' <para>CEILING：切上げ</para>
    ''' <para>ROUND：五捨六入</para>
    ''' <para>ROUND_AWAY：四捨五入</para></param>
    ''' <remarks></remarks>
    Public Sub GetRoundType(ByVal COMPCODE As String, ByRef O_SPLIT_MINUITE As Integer, ByRef O_ROUND_TYPE As GRT00009TIMEFORMAT.EM_ROUND_TYPE)
        Select Case COMPCODE
            Case CONST_CAMP_ENEX, CONST_CAMP_KNK 'ENX, KNK
                O_SPLIT_MINUITE = 10
                O_ROUND_TYPE = GRT00009TIMEFORMAT.EM_ROUND_TYPE.CEILING
            Case CONST_CAMP_NJS 'NJS
                O_SPLIT_MINUITE = 5
                O_ROUND_TYPE = GRT00009TIMEFORMAT.EM_ROUND_TYPE.CEILING
            Case CONST_CAMP_JKT 'JKT
                O_SPLIT_MINUITE = 10
                O_ROUND_TYPE = GRT00009TIMEFORMAT.EM_ROUND_TYPE.ROUND_AWAY
            Case Else
                O_SPLIT_MINUITE = 1
                O_ROUND_TYPE = GRT00009TIMEFORMAT.EM_ROUND_TYPE.ROUND
        End Select
    End Sub
End Class
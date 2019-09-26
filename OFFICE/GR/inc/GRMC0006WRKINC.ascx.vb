Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRMC0006WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "MC0006S"           'MAPID(選択)
    Public Const MAPID As String = "MC0006"             'MAPID(実行)
    Public Const MAPIDS_R As String = "MC0006S_R"       'MAPID(選択)照会
    Public Const MAPID_R As String = "MC0006_R"         'MAPID(実行)照会
    Public Const MAPIDS_JC As String = "MC0006S_JC"     'MAPID(選択)JX,COSMO
    Public Const MAPID_JC As String = "MC0006_JC"       'MAPID(実行)JX,COSMO

    ''' <summary>
    ''' 取引先一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateTORIParam(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateTORIParam = prmData
    End Function

    ''' <summary>
    ''' 届先一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="TORICODE"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateTODOKEParam(ByVal COMPCODE As String, Optional ByVal TORICODE As String = Nothing) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        If Not String.IsNullOrEmpty(TORICODE) Then
            prmData.Item(C_PARAMETERS.LP_CUSTOMER) = TORICODE
        End If
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateTODOKEParam = prmData
    End Function

    ''' <summary>
    ''' 届先一覧(JX,COSMO)の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="TORICODE"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateTODOKEJCParam(ByVal COMPCODE As String, Optional ByVal TORICODE As String = Nothing) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        If Not String.IsNullOrEmpty(TORICODE) Then
            prmData.Item(C_PARAMETERS.LP_CUSTOMER) = TORICODE
        End If
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0004DestinationList.LC_CUSTOMER_TYPE.JXCOSMO
        CreateTODOKEJCParam = prmData
    End Function

    ''' <summary>
    ''' 管理部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>部・支店</remarks>
    Function CreateMORGParam(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT,
            GL0002OrgList.C_CATEGORY_LIST.BRANCH_OFFICE}
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        CreateMORGParam = prmData
    End Function

    ''' <summary>
    ''' 運用部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>部・支店</remarks>
    Function CreateUORGParam(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        CreateUORGParam = prmData
    End Function

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        CreateFIXParam = prmData
    End Function

End Class

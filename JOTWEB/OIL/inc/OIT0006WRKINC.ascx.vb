Imports JOTWEB.GRIS0005LeftBox
Public Class OIT0006WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDS As String = "OIT0006S"       'MAPID(検索)
    Public Const MAPIDL As String = "OIT0006L"       'MAPID(一覧)
    Public Const MAPIDD As String = "OIT0006D"       'MAPID(明細)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 運用部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateUORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateUORGParam = prmData

    End Function

    ''' <summary>
    ''' 営業所の取得
    ''' </summary>
    ''' <param name="I_SALESOFFICEPT"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateSALESOFFICEParam(ByVal I_COMPCODE As String, ByVal I_SALESOFFICEPT As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_SALESOFFICE) = I_SALESOFFICEPT
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateSALESOFFICEParam = prmData

    End Function

    ''' <summary>
    ''' 状態の取得
    ''' </summary>
    ''' <param name="I_ORDERSTATUSPT"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateORDERSTATUSParam(ByVal I_COMPCODE As String, ByVal I_ORDERSTATUSPT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORDERSTATUS) = I_ORDERSTATUSPT
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateORDERSTATUSParam = prmData
    End Function

    ''' <summary>
    ''' 貨物車パターンの取得
    ''' </summary>
    ''' <param name="I_STATIONPT"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateSTATIONPTParam(ByVal I_COMPCODE As String, ByVal I_STATIONPT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_STATIONCODE) = I_STATIONPT
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateSTATIONPTParam = prmData
    End Function

End Class
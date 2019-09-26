Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRMC0007WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "MC0007S"       'MAPID(条件)
    Public Const MAPID As String = "MC0007"         'MAPID(実行)

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 運用部署パラメーター
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    Function CreateUORGParam(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        CreateUORGParam = prmData
    End Function

    ''' <summary>
    ''' 取引先一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="ORGCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateTORIParam(ByVal COMPCODE As String, Optional ByVal ORGCODE As String = Nothing) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.OWNER
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
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

End Class

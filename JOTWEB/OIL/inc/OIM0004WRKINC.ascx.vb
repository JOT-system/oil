Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0004WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIM0004S"       'MAPID(条件)
    Public Const MAPIDL As String = "OIM0004L"       'MAPID(実行)
    Public Const MAPIDC As String = "OIM0004C"       'MAPID(実行)

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

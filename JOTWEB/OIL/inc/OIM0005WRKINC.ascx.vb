Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0005WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIM0005S"       'MAPID(条件)
    Public Const MAPIDL As String = "OIM0005L"       'MAPID(実行)
    Public Const MAPIDC As String = "OIM0005C"       'MAPID(更新)

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    '' <summary>
    '' 運用部署パラメーター
    '' </summary>
    '' <param name="I_COMPCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Public Function CreateUORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateUORGParam = prmData

    End Function

    '' <summary>
    '' タンク車パターンの取得
    '' </summary>
    '' <param name="I_TANKNUMBERPT"></param>
    '' <returns></returns>
    '' <remarks>全て</remarks>
    Function CreateTANKNUMBERParam(ByVal I_COMPCODE As String, ByVal I_TANKNUMBERPT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TANKNUMBER) = I_TANKNUMBERPT
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateTANKNUMBERParam = prmData
    End Function

    '' <summary>
    '' 型式の取得
    '' </summary>
    '' <param name="I_TANKNUMBERPT"></param>
    '' <returns></returns>
    '' <remarks>全て</remarks>
    Function CreateTANKMODELParam(ByVal I_COMPCODE As String, ByVal I_TANKMODELPT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TANKMODEL) = I_TANKMODELPT
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateTANKMODELParam = prmData
    End Function

    '' <summary>
    '' 固定値マスタから一覧の取得
    '' </summary>
    '' <param name="COMPCODE"></param>
    '' <param name="FIXCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        CreateFIXParam = prmData
    End Function

End Class

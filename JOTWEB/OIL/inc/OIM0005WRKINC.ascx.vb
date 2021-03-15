Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0005WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIM0005S"       'MAPID(条件)
    Public Const MAPIDL As String = "OIM0005L"       'MAPID(実行)
    Public Const MAPIDC As String = "OIM0005C"       'MAPID(更新)
    Public Const MAPIDLK As String = "OIM0005LK"     'MAPID(甲子)

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    '' <summary>
    '' 固定値マスタから一覧の取得
    '' </summary>
    '' <param name="COMPCODE"></param>
    '' <param name="FIXCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Function CreateFIXParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE
        CreateFIXParam = prmData
    End Function

    '' <summary>
    '' 運用部署パラメーター
    '' </summary>
    '' <param name="I_COMPCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Public Function CreateORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        Return prmData
    End Function

    '' <summary>
    '' タンク車関連パターンの取得
    '' </summary>
    '' <param name="I_TANKNUMBERPT"></param>
    '' <returns></returns>
    '' <remarks>全て</remarks>
    Function CreateTankParam(ByVal I_COMPCODE As String, ByVal I_TANK As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_TANK
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        Return prmData
    End Function

    '' <summary>
    '' 基地マスタから一覧の取得
    '' </summary>
    '' <param name="I_BASERPT"></param>
    '' <returns></returns>
    '' <remarks>全て</remarks>
    Function CreateBaseParam(ByVal I_COMPCODE As String, ByVal I_BASE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_BASE) = I_BASE
        Return prmData
    End Function

    '' <summary>
    '' 原籍所有者コードの取得
    '' </summary>
    '' <param name="COMPCODE"></param>
    '' <param name="FIXCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Function CreateOriginOwnercodeParam(ByVal I_COMPCODE As String, Optional ByVal I_ORIGINOWNERCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORIGINOWNERCODE) = I_ORIGINOWNERCODE
        Return prmData
    End Function

    Function CreateOwnercodeParam(ByVal I_COMPCODE As String, Optional ByVal I_OWNERCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_OWNERCODE) = I_OWNERCODE
        Return prmData
    End Function



End Class

Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0011WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIM0011S"       'MAPID(条件)
    Public Const MAPIDL As String = "OIM0011L"       'MAPID(実行)
    Public Const MAPIDC As String = "OIM0011C"       'MAPID(更新)

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

End Class

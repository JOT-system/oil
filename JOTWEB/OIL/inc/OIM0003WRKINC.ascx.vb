﻿Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0003WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIM0003S"       'MAPID(条件)
    Public Const MAPIDL As String = "OIM0003L"       'MAPID(実行)
    Public Const MAPIDC As String = "OIM0003C"       'MAPID(登録)

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

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

        CreateORGParam = prmData

    End Function
    ''' <summary>
    ''' 営業所の取得
    ''' </summary>
    ''' <param name="I_SALESOFFICEPT"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateSALESOFFICEParam(ByVal I_COMPCODE As String, Optional ByVal I_SALESOFFICEPT As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_SALESOFFICE) = I_SALESOFFICEPT
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateSALESOFFICEParam = prmData
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

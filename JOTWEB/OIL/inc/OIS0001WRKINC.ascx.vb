﻿'Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIS0001WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIS0001S"       'MAPID(検索)
    Public Const MAPIDL As String = "OIS0001L"       'MAPID(一覧)
    Public Const MAPIDC As String = "OIS0001C"       'MAPID(更新)

    ''○ 共通関数宣言(BASEDLL)
    'Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    '' <summary>
    '' 組織コードパラメーター
    '' </summary>
    '' <param name="I_COMPCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Public Function CreateORGParam(ByVal I_COMPCODE As String, ByVal AUTHORITYALL_FLG As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        If AUTHORITYALL_FLG = "1" Then
            prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_ALL
        ElseIf AUTHORITYALL_FLG = "2" Then
            prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG
        Else
            prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        End If
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateORGParam = prmData

    End Function

    '' <summary>
    '' ロールマスタから一覧の取得
    '' </summary>
    '' <param name="COMPCODE"></param>
    '' <param name="FIXCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Function CreateRoleList(ByVal I_COMPCODE As String, ByVal I_OBJCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_CLASSCODE) = I_OBJCODE
        'prmData.Item(C_PARAMETERS.LP_STYMD) = WF_SEL_STYMD2.Text
        'prmData.Item(C_PARAMETERS.LP_ENDYMD) = WF_SEL_ENDYMD2.Text
        CreateRoleList = prmData
    End Function

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

End Class
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0008WRKINC
    Inherits UserControl

    Public Const MAPIDM As String = "OIT0008M"       'MAPID(費用管理)
    Public Const MAPIDD As String = "OIT0008D"       'MAPID(費用管理明細表示)

    Public Const MAPIDMC As String = "OIT0008MC"     'MAPID(費用管理)
    Public Const MAPIDC As String = "OIT0008C"       'MAPID(費用管理明細入力)

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

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
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_FIXCODE"></param>
    ''' <param name="I_ADDITIONALCONDITION"></param>
    ''' <returns></returns>
    Function CreateFIXParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "", Optional ByVal I_ADDITIONALCONDITION As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE
        If Not String.IsNullOrEmpty(I_ADDITIONALCONDITION) Then
            prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = I_ADDITIONALCONDITION
        End If
        CreateFIXParam = prmData
    End Function

End Class

Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0008WRKINC
    Inherits UserControl

    Public Const MAPIDM As String = "OIT0008M"       'MAPID(費用管理)
    Public Const MAPIDD As String = "OIT0008D"       'MAPID(費用管理詳細)

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

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

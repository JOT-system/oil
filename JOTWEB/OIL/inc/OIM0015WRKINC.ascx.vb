Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0015WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIM0015S"       'MAPID(条件)
    Public Const MAPIDL As String = "OIM0015L"       'MAPID(実行)
    Public Const MAPIDC As String = "OIM0015C"       'MAPID(更新)

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

Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRCO0007WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "CO0007S"       'MAPID(条件)
    Public Const MAPID As String = "CO0007"         'MAPID(実行)

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 画面IDパラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateMAPIDParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = C_ROLE_VARIANT.USER_PERTMIT

        CreateMAPIDParam = prmData

    End Function

End Class

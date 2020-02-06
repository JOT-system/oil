Option Strict On
Public Class GRM00001WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPID As String = "M00001"                          'MAPID
    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

    End Sub

    ''' <summary>
    ''' 前画面からデータを再取得する
    ''' </summary>
    ''' <param name="W_PrePage"></param>
    ''' <remarks></remarks>
    Public Sub Copy(ByVal W_PrePage As UserControl)

        WF_SEL_CAMPCODE.Text = DirectCast(W_PrePage.FindControl("WF_SEL_CAMPCODE"), TextBox).Text

    End Sub

End Class
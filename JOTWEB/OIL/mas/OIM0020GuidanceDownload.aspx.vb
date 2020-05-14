''' <summary>
''' ガイダンスダウンロードクラス(画面は提供せずファイルストリームを転送する)
''' </summary>
Public Class OIM0020GuidanceDownload
    Inherits System.Web.UI.Page
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'パラメータが無ければ404
        If Request.Params Is Nothing OrElse Request.Params.Count = 0 _
           OrElse Not Request.Params.AllKeys.Contains("id") Then
            Response.Redirect("~/OIL/ex/page_404.html")
            Return
        End If
        Dim paramStr = Request.Params("id")
        Dim decParam As List(Of String) = OIM0020WRKINC.DecodeParamString(paramStr)
    End Sub

End Class
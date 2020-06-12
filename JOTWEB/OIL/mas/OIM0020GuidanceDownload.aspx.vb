Option Strict On
Imports System.Data.SqlClient
''' <summary>
''' ガイダンスダウンロードクラス(画面は提供せずファイルストリームを転送する)
''' </summary>
Public Class OIM0020GuidanceDownload
    Inherits System.Web.UI.Page
    Private CS0050SESSION As New CS0050SESSION
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
        Dim filePath As String = ""
        Dim fileName As String = ""
        If decParam(2) = "0" Then
            filePath = GetFilePath(decParam(1))
            fileName = decParam(1)
        Else
            filePath = GetFilePath(decParam(0), decParam(1))
            fileName = IO.Path.GetFileName(filePath)
        End If

        If filePath = "" Then
            Response.Redirect("~/OIL/ex/page_404.html")
            Return
        End If

        Dim fi = New IO.FileInfo(filePath)
        Dim encodeFileName As String = HttpUtility.UrlEncode(fileName)
        encodeFileName = encodeFileName.Replace("+", "%20")
        Response.ContentType = "application/octet-stream"
        Response.AddHeader("Content-Disposition", String.Format("attachment;filename*=utf-8''{0}", encodeFileName))
        Response.AddHeader("Content-Length", fi.Length.ToString())
        Response.WriteFile(filePath)
        Response.End()
    End Sub
    ''' <summary>
    ''' ファイルパス生成（作業フォルダ）
    ''' </summary>
    ''' <param name="fileName"></param>
    ''' <returns></returns>
    Private Function GetFilePath(fileName As String) As String
        Dim guidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, OIM0020WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not IO.Directory.Exists(guidanceWorkDir) Then
            Return ""
        End If
        Dim retFilePath As String = IO.Path.Combine(guidanceWorkDir, fileName)
        If Not IO.File.Exists(retFilePath) Then
            Return ""
        End If
        Return retFilePath
    End Function
    ''' <summary>
    ''' ファイルパス生成（正式フォルダ）
    ''' </summary>
    ''' <param name="GuidanceNo"></param>
    ''' <param name="fileNo"></param>
    ''' <returns></returns>
    Private Function GetFilePath(guidanceNo As String, fileNo As String) As String
        Dim fileName As String = ""

        If fileNo = "" Then
            Return ""
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendFormat("SELECT GD.FAILE{0}", fileNo).AppendLine()
        sqlStat.AppendLine("  FROM OIL.OIM0020_GUIDANCE GD")
        sqlStat.AppendLine(" WHERE GD.GUIDANCENO = @GUIDANCENO ")
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection,
              SQLCmd As New SqlCommand(sqlStat.ToString, SQLcon)
            SQLcon.Open()
            With SQLCmd.Parameters
                .Add("@GUIDANCENO", SqlDbType.NVarChar).Value = guidanceNo
            End With
            Dim fileNameObj = SQLCmd.ExecuteScalar
            fileName = Convert.ToString(fileNameObj)
        End Using

        If fileName = "" Then
            Return ""
        End If
        Dim guidanceDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, OIM0020WRKINC.GUIDANCEROOT, guidanceNo)
        Dim filePath As String = IO.Path.Combine(guidanceDir, fileName)
        If IO.File.Exists(filePath) = False Then
            Return ""
        End If

        Return filePath
    End Function

End Class
Imports BASEDLL

Public Class GRCO0108CONFIRM
    Inherits Page

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If Not IsPostBack Then
            '○ 初期メッセージ表示
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' 初期メッセージ表示
    ''' </summary>
    Protected Sub Initialize()

        Dim WW_MSGs As String = ""
        Dim WW_TABLE As New DataTable
        WW_TABLE.Columns.Add("MESSAGE", GetType(String))

        If Not String.IsNullOrEmpty(Request.QueryString("MSGbtn")) Then
            WF_ParentButton.Value = Request.QueryString("MSGbtn")
        End If

        If Not String.IsNullOrEmpty(Request.QueryString("MSGtext")) Then
            WW_MSGs = Request.QueryString("MSGtext")
        End If

        For Each WW_MSG As String In WW_MSGs.Split("\n")
            Dim WW_ROW As DataRow = WW_TABLE.NewRow
            WW_ROW("MESSAGE") = WW_MSG
            WW_TABLE.Rows.Add(WW_ROW)
        Next

        WF_DViewRep1.DataSource = WW_TABLE
        WF_DViewRep1.DataBind()

        For i As Integer = 0 To WF_DViewRep1.Items.Count - 1
            'メッセージ
            CType(WF_DViewRep1.Items(i).FindControl("WF_Rep_MESSAGE"), Label).Text = WW_TABLE.Rows(i)("MESSAGE")
        Next

    End Sub

End Class

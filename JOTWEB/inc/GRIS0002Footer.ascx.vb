Imports System.Drawing

Public Class GRIS0002Footer
    Inherits UserControl

    Protected MEGID As String = String.Empty
    Protected MSGTYPE As String = String.Empty
    Protected PARAM01 As String = String.Empty
    Protected PARAM02 As String = String.Empty
    Protected NeedsPopUp As Boolean = False '追加
    Protected MessageTitle As String = ""   '追加
    ''' <summary>
    ''' ページロード処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>コンテンツページのロード処理後に実行される</remarks >
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If Not String.IsNullOrEmpty(MEGID) Then
            outputMessage()
        Else
            clear()
        End If
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
        WF_MESSAGE.Text = ""
        WF_MESSAGE.ForeColor = Color.Black
        WF_MESSAGE.Font.Bold = False
        WF_HELPIMG.Visible = True
        'WF_HELPIMG.ImageUrl = ResolveUrl("~/img/ヘルプ.jpg")
        MEGID = String.Empty
        MSGTYPE = String.Empty
        PARAM01 = String.Empty
        PARAM02 = String.Empty
        NeedsPopUp = False
        MessageTitle = ""
    End Sub
    ''' <summary>
    ''' メッセージの初期化
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Clear()
        WF_MESSAGE.Text = ""
        WF_MESSAGE.ForeColor = Color.Black
        WF_MESSAGE.Font.Bold = False
        MEGID = String.Empty
        MSGTYPE = String.Empty
        PARAM01 = String.Empty
        PARAM02 = String.Empty
        NeedsPopUp = False
        MessageTitle = ""
    End Sub
    ''' <summary>
    ''' メッセージの設定処理
    ''' </summary>
    ''' <param name="msgNo"></param>
    ''' <param name="msgType"></param>
    ''' <param name="I_PARA01"></param>
    ''' <param name="I_PARA02"></param>
    ''' <remarks></remarks>
    Public Sub Output(ByVal msgNo As String,
                      ByVal msgType As String,
                      Optional ByVal I_PARA01 As String = "",
                      Optional ByVal I_PARA02 As String = "",
                      Optional ByVal needsPopUp As Boolean = False,
                      Optional ByVal messageTitle As String = "メッセージ")

        Me.MEGID = msgNo
        Me.MSGTYPE = msgType
        Me.PARAM01 = I_PARA01
        Me.PARAM02 = I_PARA02
        Me.NeedsPopUp = needsPopUp
        Me.MessageTitle = messageTitle

    End Sub
    ''' <summary>
    ''' ヘルプボタンを非表示にする
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub DisabledHelp()
        WF_HELPIMG.Visible = False
    End Sub
    ''' <summary>
    ''' ヘルプボタンを表示にする
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub EnabledHelp()
        WF_HELPIMG.Visible = True
    End Sub
    ''' <summary>
    ''' ヘルプ画面表示
    ''' </summary>
    ''' <param name="I_MAPID">ヘルプを表示する画面ID</param>
    ''' <remarks></remarks>
    Public Sub ShowHelp(ByVal I_MAPID As String, ByVal I_USERID As String)
        ShowHelp(I_MAPID, String.Empty, I_USERID)
    End Sub
    ''' <summary>
    ''' ヘルプ画面表示
    ''' </summary>
    ''' <param name="I_MAPID">ヘルプを表示する画面ID</param>
    ''' <param name="I_COMPCODE">ヘルプを表示する会社コード</param>
    ''' <remarks></remarks>
    Public Sub ShowHelp(ByVal I_MAPID As String, ByVal I_COMPCODE As String, ByVal I_USERID As String)
        Dim CS0050Session As New CS0050SESSION
        '■■■ 画面遷移実行 ■■■
        Dim WW_SCRIPT As String = "<script language=""javascript"">window.open('" _
                        & ResolveUrl(C_URL.HELP) & "?HELPid=" & I_MAPID & "&HELPcomp=" & I_COMPCODE & "&HELPuserid=" & I_USERID _
                        & "', '_blank', 'directories=0, menubar=1, location=1, status=1, scrollbars=1, resizable=1, width=900, height=400');</script>"
        CS0050Session.HELP_ID = I_MAPID
        CS0050Session.HELP_COMP = I_COMPCODE
        CS0050Session.USERID = I_USERID
        Parent.Page.ClientScript.RegisterStartupScript(Parent.Page.GetType, "OpenNewWindow", WW_SCRIPT)

    End Sub
    '''' <summary>
    '''' メッセージの設定処理
    '''' </summary>
    '''' <remarks></remarks>
    'Public Sub OutputMessage()
    '    Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out

    '    CS0009MESSAGEout.MESSAGENO = Me.MEGID
    '    CS0009MESSAGEout.NAEIW = Me.MSGTYPE
    '    CS0009MESSAGEout.MESSAGEBOX = WF_MESSAGE
    '    If Not String.IsNullOrEmpty(PARAM01) Then CS0009MESSAGEout.PARA01 = PARAM01
    '    If Not String.IsNullOrEmpty(PARAM02) Then CS0009MESSAGEout.PARA02 = PARAM02
    '    CS0009MESSAGEout.CS0009MESSAGEout()

    '    If isNormal(CS0009MESSAGEout.ERR) Then
    '        WF_MESSAGE.Text = CS0009MESSAGEout.MESSAGEBOX.text
    '    End If

    'End Sub
    ''' <summary>
    ''' メッセージの設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub OutputMessage()
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        CS0009MESSAGEout.MESSAGENO = Me.MEGID
        CS0009MESSAGEout.NAEIW = Me.MSGTYPE
        CS0009MESSAGEout.MESSAGEBOX = WF_MESSAGE
        If Not String.IsNullOrEmpty(PARAM01) Then CS0009MESSAGEout.PARA01 = PARAM01
        If Not String.IsNullOrEmpty(PARAM02) Then CS0009MESSAGEout.PARA02 = PARAM02
        CS0009MESSAGEout.CS0009MESSAGEout()

        If isNormal(CS0009MESSAGEout.ERR) Then
            WF_MESSAGE.Text = DirectCast(CS0009MESSAGEout.MESSAGEBOX, Label).Text
        Else
            'baseDllエラーは現行でも放置なのでそのまま終了
            Return
        End If
        '疑似ポップアップ処理(TargetPageが設定されていなければポップアップ生成処理しない)
        Dim messageText As String = WF_MESSAGE.Text '場合によってはHTMLエンコード
        If Me.NeedsPopUp = False Then
            Return
        End If
        'ありえないが念のためポップアップ表示オブジェクトが存在していた場合クリア
        If Me.FindControl("pnlCommonMessageWrapper") IsNot Nothing Then
            Dim removeObj = Me.FindControl("pnlCommonMessageWrapper")
            Me.Controls.Remove(removeObj)
        End If
        'ポップアップオブジェクトの生成
        Dim pnlWrapper As New Panel With {.ID = "pnlCommonMessageWrapper", .ViewStateMode = ViewStateMode.Disabled}
        Dim pnlMessageBox As New Panel With {.ID = "pnlCommonMessageContents", .ViewStateMode = ViewStateMode.Disabled}
        Dim pnlMessageBoxTitle As New Panel With {.ID = "pnlCommonMessageTitle", .ViewStateMode = ViewStateMode.Disabled}
        Dim btnMessageBoxOkButton As New HtmlInputButton With {.ID = "btnCommonMessageOk", .ViewStateMode = ViewStateMode.Disabled,
                                                               .Value = "OK"}
        'ポップアップOKボタン押下時の動作
        Dim onClickScriptText As New StringBuilder
        'モーダル状態をOFFにする共通関数実行
        onClickScriptText.AppendLine("commonCloseModal('pnlCommonMessageWrapper');")
        'ポップアップ自身を非表示
        onClickScriptText.AppendLine("document.getElementById('pnlCommonMessageWrapper').style.display = 'none';")
        '本来ポップアップを表示させなかった際ASPが画面表示時にフォーカスをあてる動作をするスクリプトを実行
        onClickScriptText.AppendLine("var docLastElms = document.forms[0].querySelectorAll('script');")
        onClickScriptText.AppendLine("if (docLastElms !== null) {")
        onClickScriptText.AppendLine("    var lastScript = docLastElms[docLastElms.length -1];")
        onClickScriptText.AppendLine("    if (lastScript.innerHTML.indexOf('WebForm_Auto') >= 0) {")
        onClickScriptText.AppendLine("        var s = document.createElement('script');")
        onClickScriptText.AppendLine("        s.innerHTML = lastScript.innerHTML;")
        onClickScriptText.AppendLine("        lastScript.innerHTML = '';")
        onClickScriptText.AppendLine("        document.body.appendChild(s);")
        onClickScriptText.AppendLine("    }")
        onClickScriptText.AppendLine("}")
        btnMessageBoxOkButton.Attributes.Add("onclick", onClickScriptText.ToString)
        Dim lblMessageBoxTitleLabel As New Label With {.ID = "lblCommonMessageTitle", .ViewStateMode = ViewStateMode.Disabled,
                                                           .Text = Me.MessageTitle}
        Dim pnlMessageBoxText As New Panel With {.ID = "pnlCommonMessageText", .ViewStateMode = ViewStateMode.Disabled}
        Dim lblMessageBoxText As New Label With {.ID = "lblCommonMessageText", .ViewStateMode = ViewStateMode.Disabled,
                                                           .Text = messageText}
        lblMessageBoxText.Attributes.Add("data-naeiw", Me.MSGTYPE)
        'メッセージボックスオブジェクトの組み立て
        pnlMessageBoxTitle.Controls.Add(btnMessageBoxOkButton)
        pnlMessageBoxTitle.Controls.Add(lblMessageBoxTitleLabel)
        pnlMessageBoxText.Controls.Add(lblMessageBoxText)

        pnlMessageBox.Controls.Add(pnlMessageBoxTitle)
        pnlMessageBox.Controls.Add(pnlMessageBoxText)

        pnlWrapper.Controls.Add(pnlMessageBox)

        Me.Controls.Add(pnlWrapper)
    End Sub
End Class
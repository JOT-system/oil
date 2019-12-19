Imports System.Drawing

Public Class GRIS0002Footer
    Inherits UserControl

    Protected MEGID As String = String.Empty
    Protected MSGTYPE As String = String.Empty
    Protected PARAM01 As String = String.Empty
    Protected PARAM02 As String = String.Empty

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
        WF_HELPIMG.ImageUrl = ResolveUrl("~/img/ヘルプ.jpg")
        MEGID = String.Empty
        MSGTYPE = String.Empty
        PARAM01 = String.Empty
        PARAM02 = String.Empty
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
    End Sub
    ''' <summary>
    ''' メッセージの設定処理
    ''' </summary>
    ''' <param name="msgNo"></param>
    ''' <param name="msgType"></param>
    ''' <param name="I_PARA01"></param>
    ''' <param name="I_PARA02"></param>
    ''' <remarks></remarks>
    Public Sub Output(ByVal msgNo As String, ByVal msgType As String, Optional ByVal I_PARA01 As String = "", Optional ByVal I_PARA02 As String = "")
        Me.MEGID = msgNo
        Me.MSGTYPE = msgType
        Me.PARAM01 = I_PARA01
        Me.PARAM02 = I_PARA02

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
            WF_MESSAGE.Text = CS0009MESSAGEout.MESSAGEBOX.text
        End If

    End Sub

    ''' <summary>
    ''' メッセージNoを元にフッターラベル＋オプションでポップアップを表示する
    ''' </summary>
    ''' <param name="messageNo">[IN]メッセージNo</param>
    ''' <param name="lblObject">[IN/OUT]対象ラベルオブジェクト</param>
    ''' <param name="naeiw">[IN]省略可 エラーレベル：C_NAEIW.xxxxxを指定(未指定時は'A')</param>
    ''' <param name="pageObject">[IN/OUT]省略可 対象ページオブジェクト、指定した場合はメッセージ表示</param>
    ''' <param name="messageBoxTitle">[IN]省略可 メッセージボックスのタイトルバー文言(省略時は"Message")</param>
    ''' <param name="messagePrefix">[IN]省略可 取得したメッセージの頭につける文言</param>
    ''' <param name="messageSuffix">[IN]省略可 取得したメッセージの末尾につける文言</param>
    ''' <param name="messageParams">メッセージにしていした「?01」を置換するためのリスト</param>
    ''' <param name="messageBoxOnly">[IN]省略可 ラベル設定なしでメッセージボックスのみ表示True:メッセージボックスのみ、False:両方 (未指定はFalse)</param>
    ''' <param name="isThrowBaseDllError">[IN]省略可 BaseDllエラー時上位にスローするか？デフォルトはしない</param>
    Public Shared Sub ShowMessage(ByVal messageNo As String,
                                  ByRef lblObject As String,
                                  Optional naeiw As String = "",
                                  Optional pageObject As Page = Nothing,
                                  Optional messageBoxTitle As String = "Message",
                                  Optional messagePrefix As String = "",
                                  Optional messageSuffix As String = "",
                                  Optional messageParams As List(Of String) = Nothing,
                                  Optional messageBoxOnly As Boolean = False,
                                  Optional isThrowBaseDllError As Boolean = False,
                                  <System.Runtime.CompilerServices.CallerMemberName> Optional callerMemberName As String = Nothing,
                                  <System.Runtime.CompilerServices.CallerFilePath> Optional callerFilePath As String = Nothing,
                                  <System.Runtime.CompilerServices.CallerLineNumber> Optional callerLineNumber As Integer = 0)
        '一旦初期化
        'lblObject = ""

        If naeiw = "" Then
            naeiw = C_MESSAGE_TYPE.ABORT
        End If
        '置換文言パラメータの設定
        Dim messageParamFull As New List(Of String)
        If messageParams IsNot Nothing AndAlso messageParams.Count > 0 Then
            For i As Integer = 0 To messageParams.Count - 1
                messageParamFull.Add(messageParams(i))
                If i = 9 Then
                    Exit For
                End If
            Next
        End If

        For i = messageParamFull.Count To 9
            messageParamFull.Add("")
        Next

        ''メッセージの取得
        'Dim tmpLabel As New Label
        'Dim COA0004LableMessage As New COA0004LableMessage With
        '    {.messageNo = messageNo,
        '        .MESSAGEBOX = tmpLabel,
        '        .naeiw = naeiw,
        '        .PARA01 = messageParamFull(0), .PARA02 = messageParamFull(1),
        '        .PARA03 = messageParamFull(2), .PARA04 = messageParamFull(3),
        '        .PARA05 = messageParamFull(4), .PARA06 = messageParamFull(5),
        '        .PARA07 = messageParamFull(6), .PARA08 = messageParamFull(7),
        '        .PARA09 = messageParamFull(8), .PARA10 = messageParamFull(9)
        '    }
        ''メッセージ取得時のエラーはスローし呼出し元に任せる
        'COA0004LableMessage.COA0004getMessage()
        'If COA0004LableMessage.ERR <> C_MESSAGE_NO.NORMAL Then
        '    If isThrowBaseDllError Then
        '        Throw New Exception(String.Format("COA0004LableMessage.GetMessageError:Member={0},LineNo={1}", callerMemberName, callerLineNumber))
        '    Else
        '        Return '上位にスローしない場合は無反応で終了
        '    End If
        'End If
        'tmpLabel = COA0004LableMessage.MESSAGEBOX
        'Dim retMsg As String = messagePrefix & tmpLabel.Text & messageSuffix

        'If messageBoxOnly = False Then
        '    lblObject.Font.ClearDefaults() '余計な個別文字設定をクリア
        '    lblObject.ForeColor = Drawing.Color.FromName("0") '余計な文字色をクリア
        '    lblObject.Style.Clear()        '余計なスタイル設定をクリア
        '    lblObject.CssClass = tmpLabel.CssClass
        '    lblObject.Text = retMsg
        'End If

        Dim retMsg As String = lblObject

        'メッセーボックス生成
        If pageObject IsNot Nothing Then


            If pageObject.FindControl("pnlCommonMessageWrapper") IsNot Nothing Then
                Dim removeObj = pageObject.FindControl("pnlCommonMessageWrapper")
                pageObject.Controls.Remove(removeObj)
            End If
            Dim pnlWrapper As New Panel With {.ID = "pnlCommonMessageWrapper", .ViewStateMode = ViewStateMode.Disabled}
            Dim pnlMessageBox As New Panel With {.ID = "pnlCommonMessageContents", .ViewStateMode = ViewStateMode.Disabled}
            Dim pnlMessageBoxTitle As New Panel With {.ID = "pnlCommonMessageTitle", .ViewStateMode = ViewStateMode.Disabled}
            Dim btnMessageBoxOkButton As New HtmlInputButton With {.ID = "btnCommonMessageOk", .ViewStateMode = ViewStateMode.Disabled,
                                                                   .Value = "OK"}
            Dim onClickScriptText As New StringBuilder

            onClickScriptText.AppendLine("commonCloseModal('pnlCommonMessageWrapper');")
            onClickScriptText.AppendLine("document.getElementById('pnlCommonMessageWrapper').style.display = 'none';")
            'onClickScriptText.AppendLine("focusAfterChange();")
            onClickScriptText.AppendLine("var docLastElms = document.querySelectorAll('script');")
            onClickScriptText.AppendLine("if (docLastElms !== null) {")
            onClickScriptText.AppendLine("    var lastScript = docLastElms[docLastElms.length -1];")
            onClickScriptText.AppendLine("    if (lastScript.innerHTML.indexOf('WebForm_Auto') === 0) {")
            onClickScriptText.AppendLine("        var s = document.createElement('script');")
            onClickScriptText.AppendLine("        s.innerHTML = lastScript.innerHTML;")
            onClickScriptText.AppendLine("        lastScript.innerHTML = '';")
            onClickScriptText.AppendLine("        document.body.appendChild(s);")
            onClickScriptText.AppendLine("    }")
            onClickScriptText.AppendLine("}")
            btnMessageBoxOkButton.Attributes.Add("onclick", onClickScriptText.ToString)
            Dim lblMessageBoxTitleLabel As New Label With {.ID = "lblCommonMessageTitle", .ViewStateMode = ViewStateMode.Disabled,
                                                           .Text = messageBoxTitle}
            Dim pnlMessageBoxText As New Panel With {.ID = "pnlCommonMessageText", .ViewStateMode = ViewStateMode.Disabled}
            Dim lblMessageBoxText As New Label With {.ID = "lblCommonMessageText", .ViewStateMode = ViewStateMode.Disabled,
                                                           .Text = retMsg}
            lblMessageBoxText.Attributes.Add("data-naeiw", naeiw)
            'メッセージボックスオブジェクトの組み立て
            pnlMessageBoxTitle.Controls.Add(btnMessageBoxOkButton)
            pnlMessageBoxTitle.Controls.Add(lblMessageBoxTitleLabel)
            pnlMessageBoxText.Controls.Add(lblMessageBoxText)

            pnlMessageBox.Controls.Add(pnlMessageBoxTitle)
            pnlMessageBox.Controls.Add(pnlMessageBoxText)

            pnlWrapper.Controls.Add(pnlMessageBox)

            pageObject.Form.Parent.Controls.Add(pnlWrapper)
        End If

    End Sub
End Class
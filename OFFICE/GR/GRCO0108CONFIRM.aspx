<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GRCO0108CONFIRM.aspx.vb" Inherits="OFFICE.GRCO0108CONFIRM" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
<title>確認</title>
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/CO0108.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/CO0108.js")%>"></script>
</head>

<body onblur="focus();">
    <form id="CO0108" runat="server" target="_blank">
        <div>
            <span style="position:absolute;top:1.5em;left:2.5em;height:7em;width:20em;overflow-x:auto;">
                <asp:Label ID="WF_name" runat="server" text="  確認" Width="10em" Font-Bold="True" Font-Size="X-Large"></asp:Label>
            </span>
            <br />
            <span style="position:absolute;top:3.5em;left:1.3em;height:6em;width:25em;overflow-x:hidden;overflow-y:auto;background-color:white;border:1px solid black;">
                <asp:Repeater ID="WF_DViewRep1" runat="server" >
                    <HeaderTemplate>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table id="WF_MESSAGELIST">
                            <tr>
                                <td style="height:1.0em;width:25em;color:blue;">
                                    <!-- ■　確認メッセージ　■ -->
                                    <a></a>
                                    <asp:Label ID="WF_Rep_MESSAGE" runat="server" Text="" Height="1.0em" Width="20em"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                    <FooterTemplate>
                    </FooterTemplate>
                </asp:Repeater>
            </span>
            <a style="position:fixed; top:10em; left:7.5em;">
                <input type="button" id="WF_ButtonOK" value="ＯＫ" style="Width:5em;" onclick="ButtonClick('OK');" />
            </a>
            <a style="position:fixed; top:10em; left:15.5em;">
                <input type="button" id="WF_ButtonNG" value="ｷｬﾝｾﾙ" style="Width:5em;" onclick="ButtonClick('FALSE');" />
            </a>
        </div>
        
        <!-- イベント用 -->
        <div hidden="hidden">
            <input id="WF_ParentButton" runat="server" value="" type="text" />          <!-- 親画面ボタン押下 -->
            <input id="WF_Confirm" runat="server" value="" type="text" />               <!-- ボタン押下 -->
        </div>
    </form>
</body>
</html>

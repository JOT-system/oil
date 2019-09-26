<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GRCO0105HELP.aspx.vb" Inherits="OFFICE.GRCO0105HELP" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
<title>ヘルプ一覧</title>
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/CO0105.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/CO0105.js")%>"></script>
</head>
<body>
    <form id="CO0105" runat="server" target="_blank">
        <div>
            <span style="position:absolute;top:1.5em;left:2.5em;height:15em;width:20em;overflow-x:auto;">
                <asp:Label ID="WF_name" runat="server" text="  ヘルプ一覧" Width="10em" Font-Bold="True" Font-Size="X-Large"></asp:Label>
            </span>
            <br />
            <span style="position:absolute;top:3.5em;left:1.3em;height:15em;width:50em;overflow-x:hidden;overflow-y:auto;background-color:white;border:1px solid black;">
                <asp:Repeater ID="WF_DViewRepPDF" runat="server" >
                    <HeaderTemplate>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table id="WF_HELPFILELIST">
                            <tr>
                                <td style="height:1.0em;width:40em;color:blue;">
                                    <!-- ■　ファイル記号名称　■ -->
                                    <a>　</a>
                                    <asp:Label ID="WF_Rep_FILENAME" runat="server" Text="" Height="1.0em" Width="30em" ></asp:Label>
                                </td>
                                <td style="height:1.0em;width:10em;" hidden="hidden">
                                    <!-- ■　FILEPATH　■ -->
                                    <asp:Label ID="WF_Rep_FILEPATH" runat="server" Height="1.0em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                    <FooterTemplate>
                    </FooterTemplate>
                </asp:Repeater>
            </span>
        </div>
        <div hidden="hidden">
            <input id="WF_FileDisplay" runat="server" value="" type="text"/>                <!-- ファイル表示 -->
            <input id="WF_HELPURL" runat="server" value=""  type="text" />                  <!-- Textbox HelpURL -->
            <input id="WF_USERID" runat="server" value=""  type="text" />                   <!-- Textbox USERID -->
        </div>
    </form>
</body>
</html>
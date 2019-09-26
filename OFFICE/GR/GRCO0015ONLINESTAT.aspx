<%@ Page Title="CO0015" Language="vb" AutoEventWireup="false" CodeBehind="GRCO0015ONLINESTAT.aspx.vb" Inherits="OFFICE.GRCO0015ONLINESTAT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Register Src="~/GR/inc/GRCO0015WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<asp:Content ID="GRCO0015H" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/CO0015.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/CO0015.js")%>"></script>
</asp:Content> 
<asp:Content ID="GRCO0015" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　Menuheaderbox -->
        <div  class="Menuheaderbox" id="Menuheaderbox">

            <span class="Operation" style="margin-left:3em;margin-top:0.5em;">
                <a>　　　　　　</a>
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:49em;">
                    <input type="button" id="WF_ButtonUPDATE" value="更新"  style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
                </a>
                <a style="position:fixed;top:2.8em;left:53.5em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
            </span>

            <span style="position:fixed;top:7em;left:12em;text-align:left;">
                <asp:Label ID="Label1" runat="server" Text="運用ガイダンス" Width="12em" Font-Bold="True"></asp:Label>
            </span>
 
            <span style="position:fixed;top:8em;left:8em;text-align:left;vertical-align:text-top;">
                <asp:TextBox ID="WF_Guidance" class="WF_Guidance" runat="server" Text="" TextMode="MultiLine"></asp:TextBox><br />
            </span>


        </div>

        <!-- Work レイアウト -->
        <MSINC:wrklist id="work" runat="server" />

        <div hidden="hidden">
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
        </div>


</asp:Content>

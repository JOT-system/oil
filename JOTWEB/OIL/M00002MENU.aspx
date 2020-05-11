<%@ Page Title="M00002" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="M00002MENU.aspx.vb" Inherits="JOTWEB.M00002MENU" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>
<%@ register src="~/OIL/inc/GRM00002WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="MC0002H" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/OIL/css/M00002.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/OIL/script/M00001.js")%>"></script>

</asp:Content> 
<asp:Content ID="MC0002" ContentPlaceHolderID="contents1" runat="server">

    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonBackToMenu" class="btn-sticky" value="メニューへ" onclick="ButtonClick('WF_ButtonBackToMenu');" />
            </div>
        </div> <!-- End actionButtonBox -->
    </div>

    <!-- AWSサーバーとGITの接続確認 !-->
    <!-- 全体レイアウト　Menuheaderbox -->
    <div  class="Menuheaderbox" id="Menuheaderbox">

        <!-- 在庫管理メニュー表示 --------------------------------------------------------->
        <a  class="Menu_L" id="Menu_L"  >
        <asp:Repeater ID="Repeater_Menu_L" runat="server" >
            <HeaderTemplate>
                <div>
            </HeaderTemplate>
            <ItemTemplate>
                <%#If(Container.ItemIndex = 0, "<div class='title'>", "")  %>
                <div class='<%# If(Eval("NAMES").Equals(""), "noname", "") %>'>
                    <asp:Label ID="WF_MenuLabe_L" runat="server" CssClass="WF_MenuLabel_L"></asp:Label>
                    <asp:Label ID="WF_MenuURL_L" runat="server" Visible="False"></asp:Label>
                    <asp:Label ID="WF_MenuVARI_L" runat="server" Visible="False"></asp:Label>
                    <asp:Label ID="WF_MenuMAP_L" runat="server" Visible="False"></asp:Label>
                    <asp:Button ID="WF_MenuButton_L" runat="server" CssClass="WF_MenuButton_L" OnClientClick="commonDispWait();"/>
                </div>
                <%#If(Container.ItemIndex = 0, "</div><div class='buttons'>", "")  %>
            </ItemTemplate>
            <FooterTemplate>
                </div>
                </div>
            </FooterTemplate>
        </asp:Repeater>
        </a>

        <!-- 回送管理メニュー表示 --------------------------------------------------------->
        <a class="Menu_R" id="Menu_R" >
        <asp:Repeater ID="Repeater_Menu_R" runat="server" >
            <HeaderTemplate>
                <div>
            </HeaderTemplate>
            <ItemTemplate>
                <%#If(Container.ItemIndex = 0, "<div class='title'>", "")  %>
                    <div class='<%# If(Eval("NAMES").Equals(""), "noname", "") %>'>
                        <asp:Label ID="WF_MenuLabe_R" runat="server" CssClass="WF_MenuLabel_R"></asp:Label>
                        <asp:Label ID="WF_MenuURL_R" runat="server"  Visible="False" ></asp:Label>
                        <asp:Label ID="WF_MenuVARI_R" runat="server"  Visible="False" ></asp:Label>
                        <asp:Label ID="WF_MenuMAP_R" runat="server" Visible="False"></asp:Label>
                        <asp:Button ID="WF_MenuButton_R" runat="server" CssClass="WF_MenuButton_R"  OnClientClick="commonDispWait();"/> 
                    </div>
                <%#If(Container.ItemIndex = 0, "</div><div class='buttons'>", "")  %>
            </ItemTemplate>
            <FooterTemplate>
                </div>
                </div>
            </FooterTemplate>
        </asp:Repeater>
        </a>

        <!------- ボタン押下 ------->
        <a hidden="hidden">
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />        
        </a>
    </div>
        <!-- Work レイアウト -->
    <LSINC:work id="work" runat="server" />
   
</asp:Content> 
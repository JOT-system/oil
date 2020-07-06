<%@ Page Title="M00001" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="M00001MENU_V2.aspx.vb" Inherits="JOTWEB.M00001MENU_V2" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %> 

<%@ register src="~/OIL/inc/GRM00001WRKINC.ascx" tagname="work" tagprefix="LSINC" %>
<asp:Content ID="MC0001H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/OIL/css/M00001V2.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/OIL/script/M00001V2.js")%>"></script>
</asp:Content>
<asp:Content ID="MC0001" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　Menuheaderbox -->
    <div  class="Menuheaderbox" id="Menuheaderbox">
        <div class="guidance">

        </div>
        <div class="menuMain">
            <div class= "sideMenu">

            </div>
            <div class= "parsonalParts">

            </div>            
        </div>
        <!-- ***** ボタン押下 ***** -->
        <a hidden="hidden">
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />        
            <asp:HiddenField ID="WF_HdnGuidanceUrl" visible="false" runat="server" />
        </a>
    </div>
    <!-- Work レイアウト -->
    <LSINC:work id="work" runat="server" />
</asp:Content>


﻿<%@ Page Title="M00001" Language="vb" AutoEventWireup="true" CodeBehind="M00001MENU.aspx.vb" Inherits="OFFICE.M00001MENU"  %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ register src="inc/GRM00001WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="MC0001H" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/css/M00001.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/script/M00001.js")%>"></script>

</asp:Content> 
<asp:Content ID="MC0001" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　Menuheaderbox -->
        <div  class="Menuheaderbox" id="Menuheaderbox">

          <a  class="Menu_L" id="Menu_L"  >
            <asp:Repeater ID="Repeater_Menu_L" runat="server" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>

                    <tr>
                        <td >
                            <asp:Label ID="WF_MenuLabe_L" runat="server" CssClass="WF_MenuLabel_L"></asp:Label>
                            <asp:Label ID="WF_MenuURL_L" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuVARI_L" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuMAP_L" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_L" runat="server" CssClass="WF_MenuButton_L" onmouseover="this.style.background='blue';this.style.color='white'" onmouseout="this.style.background='gray';this.style.color='black'"/> 
                        </td>
                    </tr>

                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
             
            </asp:Repeater>
          </a>

          <a class="Menu_R" id="Menu_R" >
            <asp:Repeater ID="Repeater_Menu_R" runat="server" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td >
                            <asp:Label ID="WF_MenuLabe_R" runat="server" CssClass="WF_MenuLabel_R"></asp:Label>
                            <asp:Label ID="WF_MenuURL_R" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuVARI_R" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuMAP_R" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_R" runat="server" CssClass="WF_MenuButton_R" onmouseover="this.style.background='blue';this.style.color='white'" onmouseout="this.style.background='gray';this.style.color='black'"/> 
                        </td>
                    </tr>
                </ItemTemplate>

                <FooterTemplate>
                    </table>
                </FooterTemplate>
             
            </asp:Repeater>
          </a>

            <a hidden="hidden">
                <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
            </a>
        </div>
            <!-- Work レイアウト -->
        <LSINC:work id="work" runat="server" />

</asp:Content> 
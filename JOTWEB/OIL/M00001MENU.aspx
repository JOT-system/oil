<%@ Page Title="M00001" Language="vb" AutoEventWireup="true" CodeBehind="~/OIL/M00001MENU.aspx.vb" Inherits="JOTWEB.M00001MENU"  %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %> 

<%@ register src="~/OIL/inc/GRM00001WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="MC0001H" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/OIL/css/M00001.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/OIL/script/M00001.js")%>"></script>

</asp:Content> 
<asp:Content ID="MC0001" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　Menuheaderbox -->
        <div  class="Menuheaderbox" id="Menuheaderbox">


          <!-- 在庫管理メニュー表示 --------------------------------------------------------->
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
                            <asp:Button ID="WF_MenuButton_L" runat="server" CssClass="WF_MenuButton_L" onmouseover="this.style.background='blue';this.style.color='white'" onmouseout="this.style.background='gray';this.style.color='black'" OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>

                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
             
            </asp:Repeater>
          </a>

          <!-- 受注管理メニュー表示 --------------------------------------------------------->
          <a  class="Menu_L2" id="Menu_L2"  >
            <asp:Repeater ID="Repeater_Menu_L2" runat="server" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>

                    <tr>
                        <td >
                            <!-- 受注管理メニュー表示 -->
                            <asp:Label ID="WF_MenuLabe_L2" runat="server" CssClass="WF_MenuLabel_L2"></asp:Label>
                            <asp:Label ID="WF_MenuURL_L2" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuVARI_L2" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuMAP_L2" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_L2" runat="server" CssClass="WF_MenuButton_L2" onmouseover="this.style.background='blue';this.style.color='white'" onmouseout="this.style.background='gray';this.style.color='black'"/> 
                        </td>
                    </tr>

                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
             
            </asp:Repeater>
          </a>



          <!-- 回送管理メニュー表示 --------------------------------------------------------->
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


          <!-- マスタ管理メニュー表示 --------------------------------------------------------->
          <a class="Menu_R2" id="Menu_R2" >
            <asp:Repeater ID="Repeater_Menu_R2" runat="server" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td >
                            <asp:Label ID="WF_MenuLabe_R2" runat="server" CssClass="WF_MenuLabel_R2"></asp:Label>
                            <asp:Label ID="WF_MenuURL_R2" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuVARI_R2" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuMAP_R2" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_R2" runat="server" CssClass="WF_MenuButton_R2" onmouseover="this.style.background='blue';this.style.color='white'" onmouseout="this.style.background='gray';this.style.color='black'"/> 
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
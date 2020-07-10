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
        <div class="menuMain">
            <div id="divLeftNav" class= "sideMenu">
                <asp:Repeater ID="repLeftNav" runat="server" ClientIDMode="Predictable">
                    <HeaderTemplate>
                    </HeaderTemplate>
                    <ItemTemplate>
                        
                        <div class="parentMenu <%# DirectCast(Container.DataItem, MenuItem).Title %> <%# If(DirectCast(Container.DataItem, MenuItem).HasChild, "hasChild", "") %> <%# If(DirectCast(Container.DataItem, MenuItem).IsMenu2Link, "menu2Link", "") %>" 
                             data-posicol='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).PosiCol) %>'
                             data-rowline='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).RowLine) %>'
                             data-hasnext='<%# if(Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).HasNextPageInfo), "1", "") %>'
                            >
                            <asp:CheckBox ID="chkTopItem" 
                                          runat="server"
                                          Text='<%# DirectCast(Container.DataItem, MenuItem).Names %>' />
                            
                            <asp:Repeater ID="repLeftNavChild" 
                                            runat="server" 
                                            DataSource='<%# DirectCast(Container.DataItem, MenuItem).ChildMenuItem %>'>
                                <HeaderTemplate>
                                    <div class="childMenu">
                                </HeaderTemplate>  
                                <ItemTemplate>
                                    <div data-posicol='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).PosiCol) %>'
                                         data-rowline='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).RowLine) %>'
                                         data-hasnext='<%# if(Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).HasNextPageInfo), "1", "") %>'
                                        >
                                        <label><%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).Names) %></label>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </div>
                                </FooterTemplate>
                            </asp:Repeater>
                            
                        </div>
                    </ItemTemplate>
                    <FooterTemplate>

                    </FooterTemplate>
                </asp:Repeater>
            </div>
            <div class="guidance_parsonalArea">
                <div class="guidance">

                </div>
                <div class= "parsonalParts">
                    <p style="font-size:20px;">正式メニューIDではないので</p>
                    <p style="font-size:20px;">遷移先（次画面）でメニューと判定されずにあらぬ初期処理されるので含みおきを</p>
                </div>            
            </div>
        </div>
        <!-- ***** ボタン押下 ***** -->
        <a hidden="hidden">
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />
            <!-- 左ナビでクリックしたボタンにつきサーバー保持の遷移先情報を特定するためのキーを格納 -->
            <asp:HiddenField ID="hdnPosiCol" runat="server" Value="" />
            <asp:HiddenField ID="hdnRowLine" runat="server" Value="" /> 
            <asp:HiddenField ID="WF_HdnGuidanceUrl" visible="false" runat="server" />
        </a>
    </div>
    <!-- Work レイアウト -->
    <LSINC:work id="work" runat="server" />
</asp:Content>


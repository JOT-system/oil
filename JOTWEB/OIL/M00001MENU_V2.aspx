﻿<%@ Page Title="M00001" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="M00001MENU_V2.aspx.vb" Inherits="JOTWEB.M00001MENU_V2" %>
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
            <!-- 左ナビゲーション -->
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
            <!-- ガイダンス・個人ペインエリア -->
            <div class="guidance_parsonalArea">
                <!-- ガイダンスエリア -->
                <div ID="guidanceArea" class="guidance" runat="server">
                    <div id="guidanceList">
                        <asp:Repeater ID="repGuidance" runat="server" ClientIDMode="Predictable">
                            <HeaderTemplate>
                                <table class="guidanceTable">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td class="guidanceType"><div class='<%# Eval("TYPE") %>'></div></td>
                                    <td class="entryDate"><%# Eval("ENTRYDATE") %></td>
                                    <td class="title"><a href="#" onclick="ButtonClick('WF_ButtonShowGuidance<%# Eval("GUIDANCENO") %>'); return false;"><%# Eval("TITLE") %></a></td>
                                    <td class="naiyo"><%# Eval("NAIYOU") %></td>
                                    <td class="attachFile1"><a href='<%# ResolveUrl("~/OIL/mas/OIM0020GuidanceDownload.aspx") & "?id=" & JOTWEB.OIM0020WRKINC.GetParamString(Eval("GUIDANCENO"), "1") %>' target="_blank"><%# Eval("FILE1") %></a></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                <%#If(DirectCast(DirectCast(Container.Parent, Repeater).DataSource, System.Data.DataTable).Rows.Count = 0,
                                                                                                            "<tr><td class='empty'>ガイダンスはありません</td></tr>",
                                                                                                            "") %>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                    <!-- 個別エリア -->
                    <div id="guidanceOpenCloseWrapper">
                        <div id="guidanceOpenClose">
                        <span id="guidanceOpenCloseButton">＋ ガイダンス表示</span>
                        </div>
                    </div>
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


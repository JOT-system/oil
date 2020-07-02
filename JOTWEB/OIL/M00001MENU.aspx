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
          <a id="guidanceArea" class="guidance" runat="server">
              <div id="guidanceList">
                  <asp:Repeater ID="repGuidance" runat="server">
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
              <div id="guidanceOpenCloseWrapper">
                  <div id="guidanceOpenClose">
                    <span id="guidanceOpenCloseButton">＋ ガイダンス表示</span>
                  </div>
              </div>

          </a>
          <!-- 在庫管理メニュー表示 ****************************************************** -->
          <a  class="Menu_L" id="Menu_L"  >
            <asp:Repeater ID="Repeater_Menu_L" runat="server" ClientIDMode="Predictable" >
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
                            <asp:Button ID="WF_MenuButton_L" runat="server" CssClass="WF_MenuButton_L" OnClientClick="commonDispWait();"/>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
          </a>

          <!-- 受注管理メニュー表示 ****************************************************** -->
          <a  class="Menu_L2" id="Menu_L2"  >
            <asp:Repeater ID="Repeater_Menu_L2" runat="server" ClientIDMode="Predictable">
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
                            <asp:Button ID="WF_MenuButton_L2" runat="server" CssClass="WF_MenuButton_L2"  OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
          </a>

          <!-- 回送管理メニュー表示 ****************************************************** -->
          <a class="Menu_R" id="Menu_R" >
            <asp:Repeater ID="Repeater_Menu_R" runat="server" ClientIDMode="Predictable" >
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
                            <asp:Button ID="WF_MenuButton_R" runat="server" CssClass="WF_MenuButton_R"  OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
          </a>

          <!-- マスタ管理メニュー表示 ****************************************************** -->
          <a class="Menu_R2" id="Menu_R2" >
            <asp:Repeater ID="Repeater_Menu_R2" runat="server" ClientIDMode="Predictable" >
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
                            <asp:Button ID="WF_MenuButton_R2" runat="server" CssClass="WF_MenuButton_R2" OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
          </a>

          <!-- 請求支払管理メニュー表示 ****************************************************** -->
          <a  class="Menu_L3" id="Menu_L3"  >
            <asp:Repeater ID="Repeater_Menu_L3" runat="server" ClientIDMode="Predictable" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td >
                            <!-- 受注管理メニュー表示 -->
                            <asp:Label ID="WF_MenuLabe_L3" runat="server" CssClass="WF_MenuLabel_L3"></asp:Label>
                            <asp:Label ID="WF_MenuURL_L3" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuVARI_L3" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuMAP_L3" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_L3" runat="server" CssClass="WF_MenuButton_L3"  OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
          </a>

          <!-- タンク車所在管理メニュー表示 ****************************************************** -->
          <a  class="Menu_L4" id="Menu_L4"  >
            <asp:Repeater ID="Repeater_Menu_L4" runat="server" ClientIDMode="Predictable" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td >
                            <!-- 受注管理メニュー表示 -->
                            <asp:Label ID="WF_MenuLabe_L4" runat="server" CssClass="WF_MenuLabel_L4"></asp:Label>
                            <asp:Label ID="WF_MenuURL_L4" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuVARI_L4" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="WF_MenuMAP_L4" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_L4" runat="server" CssClass="WF_MenuButton_L4" OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
          </a>

          <!-- 実績・統計メニュー表示 ****************************************************** -->
          <a class="Menu_R3" id="Menu_R3" >
            <asp:Repeater ID="Repeater_Menu_R3" runat="server" ClientIDMode="Predictable" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td >
                            <asp:Label ID="WF_MenuLabe_R3" runat="server" CssClass="WF_MenuLabel_R3"></asp:Label>
                            <asp:Label ID="WF_MenuURL_R3" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuVARI_R3" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuMAP_R3" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_R3" runat="server" CssClass="WF_MenuButton_R3"  OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
          </a>

          <!-- データ連携メニュー表示 ****************************************************** -->
          <a class="Menu_R4" id="Menu_R4" >
            <asp:Repeater ID="Repeater_Menu_R4" runat="server" ClientIDMode="Predictable" >
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td >
                            <asp:Label ID="WF_MenuLabe_R4" runat="server" CssClass="WF_MenuLabel_R4"></asp:Label>
                            <asp:Label ID="WF_MenuURL_R4" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuVARI_R4" runat="server"  Visible="False" ></asp:Label>
                            <asp:Label ID="WF_MenuMAP_R4" runat="server" Visible="False"></asp:Label>
                            <asp:Button ID="WF_MenuButton_R4" runat="server" CssClass="WF_MenuButton_R4" OnClientClick="commonDispWait();"/> 
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
          </a>

         <!-- ***** ボタン押下 ***** -->
         <a hidden="hidden">
                <input id="WF_ButtonClick" runat="server" value=""  type="text" />        
                <asp:HiddenField ID="WF_HdnGuidanceUrl" visible="false" runat="server" />
          </a>
        </div>
            <!-- Work レイアウト -->
        <LSINC:work id="work" runat="server" />

</asp:Content> 
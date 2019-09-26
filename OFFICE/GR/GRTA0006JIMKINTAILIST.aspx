<%@ Page Title="TA0006" Language="vb" AutoEventWireup="false" CodeBehind="GRTA0006JIMKINTAILIST.aspx.vb" Inherits="OFFICE.GRTA0006JIMKINTAILIST" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>

<%@ register src="inc/GRTA0006WRKINC.ascx" tagname="work" tagprefix="LSINC" %>
<asp:Content ID="GRTA0006H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/TA0006.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/TA0006.js")%>"></script>
</asp:Content>
<asp:Content ID="GRTA0006" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　headerbox -->
        <div  class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <!-- ■　条件　■ -->
                <a style="position:fixed;top:3.0em;left:15em;width:15.0em;" >
                    <asp:Label runat="server" Text="対象年月：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_DATE" runat="server"  Font-Bold="True"></asp:Label>
                </a>
                <a style="position:fixed;top:3.0em;left:30em;width:15.0em;" >
                    <asp:Label runat="server" Text="配属部署：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_ORG" runat="server"  Font-Bold="True"></asp:Label>
                </a>
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:53.5em;">
                    <input type="button" id="WF_ButtonPDF" value="全印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPDF');" />
                </a>
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonXLS" value="Excel取得"  style="Width:5em" onclick="ButtonClick('WF_ButtonXLS');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonZIP" value="全ZIP取得"  style="Width:5em" onclick="ButtonClick('WF_ButtonZIP');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.2em;left:75em;">
                    <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
            </div>
            <div id="leftMenubox" class="leftMenubox">
                <div style="overflow-y:auto;height:1.8em;width:12.0em;text-align:center;vertical-align:central;color:white;background-color:rgb(22,54,92);font-weight:bold;border: solid black;border-width:1.5px;">
                    <!-- ■　照会選択　■ -->
                    <a style="width:12.0em;font-size:medium;overflow:hidden;color:white;background-color:rgb(22,54,92);text-align:center;">照会選択</a>
                </div>

                <div id="STAFFSelect" style="overflow-y:auto;min-height:30em;width:12.0em;color:black;background-color: white;border: solid;border-width:1.5px;">
                    <!-- ■　セレクター　■ -->
                    <asp:Repeater ID="WF_SELECTOR" runat="server">
                        <HeaderTemplate> 
                            <table style="border-width:1px;margin:0.1em 0.1em 0.1em 0.1em;">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr> 
                                <!-- 非表示項目(左Box処理用・Repeater内行位置)　-->
                                <td hidden="hidden">
                                    <asp:Label ID="WF_SELECTOR_VALUE" runat="server"></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="WF_SELECTOR_TEXT" runat="server" Text="" Height="1.3em" Width="11.8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>
                            </tr> 
                        </ItemTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>

                    </asp:Repeater>
                </div>
            </div>
            <!-- 一覧レイアウト -->
            <div id="divListArea">
                <asp:panel id="pnlListArea" runat="server" ></asp:panel>
            </div>
        </div>
            <!-- 全体レイアウト　detailbox -->
        <div  class="detailbox" id="detailbox" hidden="hidden">
        </div>
        <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>         <!-- GridViewダブルクリック -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>        <!-- GridView表示位置フィールド -->
            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />             <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>            <!-- Textbox DBクリックフィールド -->
            <input id="WF_RightViewChange" runat="server" value="" type="text"/>            <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />             <!-- Rightbox 開閉 -->

            <input id="WF_REP_LINECNT"  runat="server" value=""  type="text" />             <!-- Repeater 行位置 -->
            <input id="WF_REP_POSITION"  runat="server" value=""  type="text" />            <!-- Repeater 行位置 -->
            <input id="WF_REP_ROWSCNT" runat="server" value=""  type="text" />              <!-- Repeaterの１明細の行数 -->

            <input id="WF_SELECTOR_SW" runat="server" value=""  type="text" />              <!-- Repeaterの選択値 -->
            <input id="WF_SELECTOR_Posi" runat="server" value=""  type="text" />            <!-- Repeaterの選択値 -->

            <input id="WF_SaveSX"  runat="server" value=""  type="text" />                  <!-- セレクタ 変更位置X軸 -->
            <input id="WF_SaveSY"  runat="server" value=""  type="text" />                  <!-- セレクタ 変更位置Y軸 -->

            <input id="WF_PrintURL" runat="server" value=""  type="text" />              <!-- Textbox Print URL -->
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->

        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>
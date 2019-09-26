<%@ Page Title="TA0007" Language="vb" AutoEventWireup="false" CodeBehind="GRTA0007JIMKYUYOLIST.aspx.vb" Inherits="OFFICE.GRTA0007JIMKYUYOLIST" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>

<%@ register src="inc/GRTA0007WRKINC.ascx" tagname="work" tagprefix="LSINC" %>
<asp:Content ID="GRTA0007H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/TA0007.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/TA0007.js")%>"></script>
</asp:Content>
<asp:Content ID="GRTA0007" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　headerbox -->
        <div  class="headerboxOnly" id="headerbox">
            <div class="Operation" id="Operation">
                <!-- ■　条件　■ -->
                <a style="position:fixed;top:3.0em;left:5em;width:15.0em;">
                    <asp:Label runat="server" Text="対象年月：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_DATE" runat="server"  Font-Bold="True"></asp:Label>
                </a>
                <a style="position:fixed;top:3.0em;left:20em;width:15.0em;">
                    <asp:Label runat="server" Text="配属部署：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_ORG" runat="server"  Font-Bold="True"></asp:Label>
                </a>
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonPDF" value="印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPDF');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonXLS" value="Excel取得"  style="Width:5em" onclick="ButtonClick('WF_ButtonXLS');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.2em;left:75em;">
                    <asp:Image ID="WF_ButtonFIRST2" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST2" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
            </div>
            <!-- 一覧レイアウト -->
            <div id="divListArea">
                <asp:panel id="pnlListArea" runat="server" ></asp:panel>
            </div>
        </div>

        <!-- 全体レイアウト　detailbox -->
        <div  class="detailbox" id="detailbox">
        </div>  

        <div hidden="hidden">
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>        <!-- GridView表示位置フィールド -->
            
            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />         <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>        <!-- Textbox DBクリックフィールド -->
            <input id="WF_RightViewChange" runat="server" value="" type="text"/>        <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />         <!-- Rightbox 開閉 -->

            <input id="WF_PrintURL" runat="server" value=""  type="text" />             <!-- Textbox Print URL -->            
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->
        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>

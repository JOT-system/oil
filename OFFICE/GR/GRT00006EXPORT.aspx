<%@ Page Title="T00006" Language="vb" AutoEventWireup="false" CodeBehind="GRT00006EXPORT.aspx.vb" Inherits="OFFICE.GRT00006EXPORT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00006WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00006H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00006.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00006.js")%>"></script>
</asp:Content> 

<asp:Content ID="T00006" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerboxOnly" id="headerbox">
        <div class="Operation">
            <!-- 全選択 -->
            <a style="position:fixed;top:2.8em;left:1.0em;">
                <input type="button" id="WF_ButtonALLSELECT" value="全選択"  style="Width:5em" onclick="ButtonClick('WF_ButtonALLSELECT');" />
            </a>
            <!-- 全解除 -->
            <a style="position:fixed;top:2.8em;left:6.0em;">
                <input type="button" id="WF_ButtonALLCANCEL" value="全解除"  style="Width:5em" onclick="ButtonClick('WF_ButtonALLCANCEL');" />
            </a>

            <!-- ■　ボタン　■ -->
            <a style="position:fixed;top:2.8em;left:53.5em;">
                <input type="button" id="WF_ButtonPut" value="光英送信"  style="Width:5em" onclick="ButtonClick('WF_ButtonPut');" />
            </a>
            <a style="position:fixed;top:2.8em;left:58em;">
                <input type="button" id="WF_ButtonCSV" value="光英CSV"  style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
            </a>
            <a style="position:fixed;top:2.8em;left:62.5em;">
                <input type="button" id="WF_ButtonLOCAL" value="矢崎ZIP"  style="Width:5em" onclick="ButtonClick('WF_ButtonLOCAL');" />
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
        <div id="divListArea">
            <asp:panel id="pnlListArea" runat="server"></asp:panel>
        </div>
    </div>


    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <div hidden="hidden">
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>        <!-- GridView表示位置フィールド -->

        <input id="WF_ButtonClick" runat="server" value=""  type="text" />              <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />            <!-- 権限 -->

        <input id="WF_RightViewChange" runat="server" value="" type="text"/>            <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value=""  type="text" />             <!-- Rightbox 開閉 -->

        <input id="WF_IsHideKoueiButton"  runat="server" value="0" type="text" />       <!-- 光栄受信ボタン非表示フラグ -->

        <input id="WF_ZipName" runat="server" value=""  type="text" />                  <!-- Textbox Zipファイル名称 -->
        <input id="WF_ZipURL" runat="server" value=""  type="text" />                   <!-- Textbox Zip URL -->

        <input id="WF_PrintURL" runat="server" value=""  type="text" />                 <!-- Textbox Print URL -->
    </div>

    <!-- Work レイアウト -->
    <LSINC:work id="work" runat="server" />
           
</asp:Content>

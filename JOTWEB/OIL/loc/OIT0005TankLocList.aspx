<%@ Page Title="OIT0005L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0005TankLocList.aspx.vb" Inherits="JOTWEB.OIT0005TankLocList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>
<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0005LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0005L.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0005L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
<asp:Content ID="OIT0005L" ContentPlaceHolderID="contents1" runat="server">
        <div class="headerboxOnly" id="headerbox">
            <div class="actionButtonBox">
                <div class="leftSide">
<%--                    <input type="button" id="WF_ButtonSelectAll"           class="btn-sticky" value="全選択" onclick="ButtonClick('WF_ButtonSelectAll');" />
                    <input type="button" id="WF_ButtonSelectOff"           class="btn-sticky" value="選択解除" onclick="ButtonClick('WF_ButtonSelectOff');" />--%>
                </div>
                <div class="rightSide">
                    <input type="button" id="WF_ButtonCSV"           class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonCSV');" />
                    <input type="button" id="WF_ButtonUpdateList"    class="btn-sticky" value="明細更新" onclick="ButtonClick('WF_ButtonUpdateList');" />
                    <input type="button" id="WF_ButtonEND"           class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                    <div                 id="WF_ButtonFIRST"         class="firstPage"  runat="server"   onclick="ButtonClick('WF_ButtonFIRST');"></div>
                    <div                 id="WF_ButtonLAST"          class="lastPage"   runat="server"   onclick="ButtonClick('WF_ButtonLAST');"></div>
                </div>
            </div> <!-- End class=actionButtonBox -->
            <div class="filterArea">
                <div class="grc0001Wrapper" onchange="ButtonClick('chklGroupFilter');">
                    <asp:CheckBoxList ID="chklGroupFilter" runat="server" RepeatLayout="UnorderedList"></asp:CheckBoxList>
                </div>
            </div>
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
        </div> <!-- End class=headerboxOnly -->
    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
        <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>
        <!-- GridView表示位置フィールド -->

        <input id="WF_FIELD" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />
        <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->

        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
        <!-- LeftBox Mview切替 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
        <!-- LeftBox 開閉 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />
        <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />
        <!-- Rightbox 開閉 -->

        <input id="WF_PrintURL" runat="server" value="" type="text" />
        <!-- Textbox Print URL -->

        <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />
        <!-- 一覧・詳細画面切替用フラグ -->

        <input id="WF_ButtonClick" runat="server" value="" type="text" />
        <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
        <!-- 権限 -->
    </div>
</asp:Content>
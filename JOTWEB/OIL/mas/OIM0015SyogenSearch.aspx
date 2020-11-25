<%@ Page Title="OIM0015S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0015SyogenSearch.aspx.vb" Inherits="JOTWEB.OIM0015SyogenSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0015WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIM0015SH" contentplaceholderid="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0015S.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0015S.js")%>'></script>
</asp:content>

<asp:Content ID="OIM0015S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO"  class="btn-sticky" value="検索"  onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 荷受人コード -->
            <div class="inputItem">
                <a id="WF_CONSIGNEECODE_LABEL">荷受人コード</a>
                <span ondblclick="Field_DBclick('WF_CONSIGNEECODE', <%=LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST%>);" onchange="TextBox_change('WF_CONSIGNEECODE');">
                    <asp:TextBox  ID="WF_CONSIGNEECODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
                <asp:Label ID="WF_CONSIGNEECODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>
            <!-- 荷主コード -->
            <div class="inputItem">
                <a id="WF_SHIPPERSCODE_LABEL">荷主コード</a>
                <span ondblclick="Field_DBclick('WF_SHIPPERSCODE', <%=LIST_BOX_CLASSIFICATION.LC_JOINTLIST%>);" onchange="TextBox_change('WF_SHIPPERSCODE');">
                    <asp:TextBox  ID="WF_SHIPPERSCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
                <asp:Label ID="WF_SHIPPERSCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>
            <!-- 油種コード -->
            <div class="inputItem">
                <a id="WF_OILCODE_LABEL">油種コード</a>
                <span ondblclick="Field_DBclick('WF_OILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OILCODE');">
                    <asp:TextBox  ID="WF_OILCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </span>
                <asp:Label ID="WF_OILCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>
        </div> <!-- End inputBox -->
    </div> <!-- End searchbox -->

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
    </div>
</asp:Content>

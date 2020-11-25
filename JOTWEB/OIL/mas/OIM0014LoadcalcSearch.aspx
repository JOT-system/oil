<%@ Page Title="OIM0014S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0014LoadcalcSearch.aspx.vb" Inherits="JOTWEB.OIM0014LoadcalcSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0014WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIM0014SH" contentplaceholderid="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0014S.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0014S.js")%>'></script>
</asp:content>

<asp:Content ID="OIM0014S" ContentPlaceHolderID="contents1" runat="server">
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
            <!-- 基地コード -->
            <div class="inputItem">
                <a id="WF_PLANTCODE_LABEL">基地コード</a>
                <span ondblclick="Field_DBclick('WF_PLANTCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_PLANTCODE');">
                    <asp:TextBox  ID="WF_PLANTCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </span>
                <asp:Label ID="WF_PLANTCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>
            <!-- 油種大分類コード -->
            <div class="inputItem">
                <a id="WF_BIGOILCODE_LABEL">油種大分類コード</a>
                <span ondblclick="Field_DBclick('WF_BIGOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_BIGOILCODE');">
                    <asp:TextBox  ID="WF_BIGOILCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </span>
                <asp:Label ID="WF_BIGOILCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>
            <!-- 積込チェック用油種コード -->
            <div class="inputItem">
                <a id="WF_CHECKOILCODE_LABEL">積込チェック用油種コード</a>
                <span ondblclick="Field_DBclick('WF_CHECKOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_CHECKOILCODE');">
                    <asp:TextBox  ID="WF_CHECKOILCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </span>
                <asp:Label ID="WF_CHECKOILCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
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

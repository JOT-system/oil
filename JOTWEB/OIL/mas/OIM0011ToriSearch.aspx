<%@ Page Title="OIM0011S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0011ToriSearch.aspx.vb" Inherits="JOTWEB.OIM0011TORISearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0011WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIM0011SH" contentplaceholderid="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0011S.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0011S.js")%>'></script>
</asp:content>

<asp:Content ID="OIM0011S" ContentPlaceHolderID="contents1" runat="server">
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
            <!-- 取引先コード -->
            <div class="inputItem">
                <a id="WF_TORICODE_LABEL">取引先コード</a>
                <span ondblclick="Field_DBclick('WF_TORICODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_TORICODE');">
                    <asp:TextBox  ID="WF_TORICODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10" ></asp:TextBox>
                </span>
                <asp:Label ID="WF_TORICODE_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>
            <!-- 有効年月日(開始） -->
            <div class="inputItem">
                <a id="WF_STYMD_LABEL">有効年月日（開始）</a>
                <span ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_STYMD" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
            </div>
            <!-- 有効年月日(終了） -->
            <div class="inputItem">
                <a id="WF_ENDYMD_LABEL" >有効年月日（終了）</a>
                <span ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_ENDYMD" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
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

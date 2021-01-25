<%@ Page Title="OIM0019S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0019AccountSearch.aspx.vb" Inherits="JOTWEB.OIM0019AccountSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0019WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIM0019SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIM0019S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0019S.js")%>'></script>
</asp:content>

<asp:Content ID="OIM0019S" ContentPlaceHolderID="contents1" runat="server">
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
            <!-- 適用開始年月日 -->
            <div class="inputItem">
                <a id="WF_FROMYMD_LABEL">適用開始年月日</a>
                <span ondblclick="Field_DBclick('WF_FROMYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_FROMYMD" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
            </div>
            <!-- 適用終了年月日 -->
            <div class="inputItem">
                <a id="WF_ENDYMD_LABEL">適用終了年月日</a>
                <span ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_ENDYMD" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
            </div>
            <!-- 科目コード -->
            <div class="inputItem">
                <a id="WF_ACCOUNTCODE_LABEL">科目コード</a>
                <span ondblclick="Field_DBclick('WF_ACCOUNTCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ACCOUNTCODE');">
                    <asp:TextBox  ID="WF_ACCOUNTCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                </span>
                <a id="WF_ACCOUNTCODE_NAME">
                    <asp:Label ID="WF_ACCOUNTCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- セグメント -->
            <div class="inputItem">
                <a id="WF_SEGMENTCODE_LABEL">セグメント</a>
                <span ondblclick="Field_DBclick('WF_SEGMENTCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_SEGMENTCODE');">
                    <asp:TextBox  ID="WF_SEGMENTCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" onblur="MsgClear();" MaxLength="5"></asp:TextBox>
                </span>
                <a id="WF_SEGMENTCODE_NAME">
                    <asp:Label ID="WF_SEGMENTCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- セグメント枝番 -->
            <div class="inputItem">
                <a id="WF_SEGMENTBRANCHCODE_LABEL">セグメント枝番</a>
                <asp:TextBox  ID="WF_SEGMENTBRANCHCODE" runat="server" CssClass="WF_TEXTBOX_CSS" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
            </div>
            <!-- 科目区分 -->
            <div class="inputItem">
                <a id="WF_ACCOUNTTYPE_LABEL">科目区分</a>
                <span ondblclick="Field_DBclick('WF_ACCOUNTTYPE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ACCOUNTTYPE');">
                    <asp:TextBox  ID="WF_ACCOUNTTYPE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </span>
                <a id="WF_ACCOUNTTYPE_NAME">
                    <asp:Label ID="WF_ACCOUNTTYPE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
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

<%@ Page Title="OIM0021S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0021LoadReserveSearch.aspx.vb" Inherits="JOTWEB.OIM0021LoadReserveSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0021WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIM0021SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIM0021S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0021S.js")%>'></script>
</asp:content>

<asp:Content ID="OIM0021S" ContentPlaceHolderID="contents1" runat="server">
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
            <!-- 管轄営業所 -->
            <div class="inputItem">
                <a id="WF_OFFICECODE_LABEL">管轄営業所</a>
                <span ondblclick="Field_DBclick('WF_OFFICECODE', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('WF_OFFICECODE');">
                    <asp:TextBox  ID="WF_OFFICECODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </span>
                <a id="WF_OFFICECODE_NAME">
                    <asp:Label ID="WF_OFFICECODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 適用開始年月日 -->
            <div class="inputItem">
                <a id="WF_FROMYMD_LABEL">適用開始年月日</a>
                <span ondblclick="Field_DBclick('WF_FROMYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_FROMYMD" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
            </div>
            <!-- 適用終了年月日 -->
            <div class="inputItem">
                <a id="WF_TOYMD_LABEL">適用終了年月日</a>
                <span ondblclick="Field_DBclick('WF_TOYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_TOYMD" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
            </div>
            <!-- 荷重 -->
            <!--
            <div class="inputItem">
                <a id="WF_LOAD_LABEL">荷重</a>
                <asp:TextBox ID="WF_LOAD" runat="server" CssClass="WF_TEXTBOX_CSS" onblur="MsgClear();" MaxLength="5"></asp:TextBox>
            </div>
            -->
            <!-- 油種コード -->
            <div class="inputItem">
                <a id="WF_OILCODE_LABEL">油種コード</a>
                <span ondblclick="Field_DBclick('WF_OILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OILCODE');">
                    <asp:TextBox  ID="WF_OILCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </span>
                <a id="WF_OILCODE_NAME">
                    <asp:Label ID="WF_OILCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 油種細分コード -->
            <div class="inputItem">
                <a id="WF_SEGMENTOILCODE_LABEL">油種細分コード</a>
                <span ondblclick="Field_DBclick('WF_SEGMENTOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_SEGMENTOILCODE');">
                    <asp:TextBox  ID="WF_SEGMENTOILCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </span>
                <a id="WF_SEGMENTOILCODE_NAME">
                    <asp:Label ID="WF_SEGMENTOILCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
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

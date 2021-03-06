﻿<%@ Page Title="OIM0017S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0017TrainOperationSearch.aspx.vb" Inherits="JOTWEB.OIM0017TrainOperationSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0017WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIM0017SH" contentplaceholderid="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0017S.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0017S.js")%>'></script>
</asp:content>

<asp:Content ID="OIM0017S" ContentPlaceHolderID="contents1" runat="server">
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
            <!-- 管轄受注営業所 -->
            <div class="inputItem">
                <a id="WF_OFFICECODE_LABEL">営業所</a>
                <a class="ef" id="WF_OFFICECODE_CODE" ondblclick="Field_DBclick('WF_OFFICECODE', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('WF_OFFICECODE');">
                    <asp:TextBox ID="WF_OFFICECODE" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_OFFICECODE_NAME">
                    <asp:Label ID="WF_OFFICECODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- JOT列車番号 -->
            <div class="inputItem">
                <a id="WF_TRAINNO_LABEL">列車番号</a>
                <a class="ef" id="WF_TRAINNO_CODE" ondblclick="Field_DBclick('WF_TRAINNO', <%=LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER%>);" onchange="TextBox_change('WF_TRAINNO');">
                    <asp:TextBox ID="WF_TRAINNO" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </a>
                <a id="WF_TRAINNO_NAME">
                    <asp:Label ID="WF_TRAINNO_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 運行日 -->
            <div class="inputItem">
                <a id="WF_WORKINGDATE_LABEL">運行日</a>
                <span ondblclick="Field_DBclick('WF_WORKINGDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_WORKINGDATE" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
                <a id="WF_WORKINDATE_NAME">
                    <span class="WF_TEXT">&nbsp;以降</span>
                </a>
            </div>
            <!-- 積置フラグ -->
            <!--
            <div class="inputItem">
                <a id="WF_TSUMI_LABEL" >積置フラグ</a>
                <a class="ef" id="WF_TSUMI_CODE" ondblclick="Field_DBclick('WF_TSUMI', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_TSUMI');">
                    <asp:TextBox ID="WF_TSUMI" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </a>
                <a id="WF_TSUMI_NAME">
                    <asp:Label ID="WF_TSUMI_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            -->
            <!-- 発駅コード -->
            <!--
            <div class="inputItem">
                <a id="WF_DEPSTATION_LABEL">発駅コード</a>
                <a class="ef" id="WF_DEPSTATION_CODE" ondblclick="Field_DBclick('WF_DEPSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_DEPSTATION');">
                    <asp:TextBox ID="WF_DEPSTATION" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="7"></asp:TextBox>
                </a>
                <a id="WF_DEPSTATION_NAME">
                    <asp:Label ID="WF_DEPSTATION_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            -->
            <!-- 着駅コード -->
            <!--
            <div class="inputItem">
                <a id="WF_ARRSTATION_LABEL">着駅コード</a>
                <a class="ef" id="WF_ARRSTATION_CODE" ondblclick="Field_DBclick('WF_ARRSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_ARRSTATION');">
                    <asp:TextBox ID="WF_ARRSTATION" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="7"></asp:TextBox>
                </a>
                <a id="WF_ARRSTATION_NAME">
                    <asp:Label ID="WF_ARRSTATION_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            -->
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

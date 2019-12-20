﻿<%@ Page Title="OIS0001S" Language="vb" AutoEventWireup="false" CodeBehind="OIS0001UserSearch.aspx.vb" Inherits="JOTWEB.OIS0001UserSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIS0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIS0001SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIS0001S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIS0001S.js")%>'></script>
</asp:content>

<asp:Content ID="OIS0001S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <a style="position:fixed;top:2.8em;left:62.5em;">
            <input type="button" id="WF_ButtonDO" value="検索"  style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
        </a>
        <a style="position:fixed;top:2.8em;left:67em;">
            <input type="button" id="WF_ButtonEND" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
        </a>

        <!-- ○ 変動項目 ○ -->
        <!-- 会社コード -->
        <a id="WF_CAMPCODE_LABEL" >会社コード</a>

        <a class="ef" id="WF_CAMPCODE" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox CssClass="BoxIcon" ID="WF_CAMPCODE_CODE" runat="server"  onblur="MsgClear();"></asp:TextBox>
        </a>
        <a id="WF_CAMPCODE_TEXT">
            <asp:Label ID="WF_CAMPCODE_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 有効年月日(開始） -->
        <a id="WF_STYMD_LABEL" >有効年月日（開始）</a>

        <a class="ef" id="WF_STYMD" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:TextBox CssClass="CalendarIcon" ID="WF_STYMD_CODE" runat="server"  onblur="MsgClear();"></asp:TextBox>
        </a>

        <!-- 有効年月日(終了） -->
        <a id="WF_ENDYMD_LABEL" >有効年月日（終了）</a>

        <a class="ef" id="WF_ENDYMD" ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:TextBox CssClass="CalendarIcon" ID="WF_ENDYMD_CODE" runat="server" onblur="MsgClear();"></asp:TextBox>
        </a>

        <!-- 組織コード -->
        <a id="WF_ORG_LABEL" >組織コード</a>

        <a class="ef" id="WF_ORG" ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
            <asp:TextBox CssClass="BoxIcon" ID="WF_ORG_CODE" runat="server" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a id="WF_ORG_TEXT">
            <asp:Label ID="WF_ORG_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
        </a>
    </div>

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

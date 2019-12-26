<%@ Page Title="OIT0001S" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0001EmptyTurnDairySearch.aspx.vb" Inherits="JOTWEB.OIT0001EmptyTurnDairySearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content id="OIT0001SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0001S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0001S.js")%>'></script>
</asp:Content>

<asp:Content ID="OIT0001S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <a style="position:fixed;top:2.8em;left:62.5em;">
            <input type="button" id="WF_ButtonDO" class="btn-sticky" value="検索"  style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
        </a>
        <a style="position:fixed;top:2.8em;left:67em;">
            <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
        </a>

        <!-- ○ 変動項目 ○ -->
        <!-- 会社コード -->
        <a style="position:fixed; top:7.7em; left:4em; font-weight:bold; text-decoration:underline;display:none">会社コード</a>

        <a class="ef" style="position:fixed; top:7.7em; left:18em;display:none" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:7.7em; left:27em;display:none">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        <!-- 運用部署 -->
        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;display:none">運用部署</a>

        <a class="ef" style="position:fixed; top:9.9em; left:18em;display:none" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
            <asp:TextBox ID="WF_UORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:9.9em; left:27em;display:none">
            <asp:Label ID="WF_UORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        <!-- 営業所 -->
        <a id="WF_OFFICECODE_LABEL" class="requiredMark">営業所</a>
        <a id="WF_OFFICECODE_ICON" onclick="Field_DBclick('TxtSalesOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);">
            <asp:Image runat="server" ImageUrl="../img/leftbox.png"/>
        </a>
        <a class="ef" id="WF_OFFICECODE" ondblclick="Field_DBclick('TxtSalesOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtSalesOffice');">
            <%--<asp:TextBox CssClass="BoxIcon" ID="TxtSalesOffice" runat="server" onblur="MsgClear();" MaxLength="6"></asp:TextBox>--%>
            <asp:TextBox ID="TxtSalesOffice" runat="server" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
        </a>
        <a id="WF_OFFICECODE_TEXT" >
            <asp:Label ID="LblSalesOfficeName" runat="server" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 積込日 -->
        <a id="WF_LOADINGDATE_LABEL" class="requiredMark">積込日</a>
        <a id="WF_LOADINGDATE_ICON" onclick="Field_DBclick('TxtLoadingDateStart', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:Image runat="server" ImageUrl="../img/calendar.png"/>
        </a>
        <a class="ef" id="WF_LOADINGDATE" ondblclick="Field_DBclick('TxtLoadingDateStart', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <%--<asp:TextBox CssClass="CalendarIcon" ID="TxtLoadingDateStart" runat="server" onblur="MsgClear();"></asp:TextBox>--%>
            <asp:TextBox ID="TxtLoadingDateStart" runat="server" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a id="WF_LOADINGDATE_SYMBOL">～</a>

        <!-- 列車番号 -->
        <a id="WF_TRAINNO_LABEL">列車番号</a>
        <a class="ef" id="WF_TRAINNO">
            <asp:TextBox ID="TxtTrainNumber" runat="server" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
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

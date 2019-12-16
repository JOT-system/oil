<%@ Page Title="OIM0005S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0005TankSearch.aspx.vb" Inherits="JOTWEB.OIM0005TankSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIM0005SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIM0005S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0005S.js")%>'></script>
</asp:content>

<asp:Content ID="OIM0005S" ContentPlaceHolderID="contents1" runat="server">
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
        <a style="display:none; position:fixed; top:3.3em; left:4em; font-weight:bold; text-decoration:underline;">会社コード</a>

        <a  class="ef" style="display:none; position:fixed; top:5.5em; left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="display:none; position:fixed; top:3.3em; left:27em;">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 組織コード -->
        <a style="display:none; position:fixed; top:5.5em; left:4em; font-weight:bold; text-decoration:underline;">組織コード</a>

        <a  class="ef" style="display:none; position:fixed; top:5.5em; left:18em;" ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
            <asp:TextBox ID="WF_ORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="display:none; position:fixed; top:5.5em; left:27em;">
            <asp:Label ID="WF_ORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- JOT車番 -->
        <a id="WF_TANKNUMBER_LABEL">JOT車番</a>

        <a class="ef" id="WF_TANKNUMBER" ondblclick="Field_DBclick('WF_TANKNUMBER', <%=LIST_BOX_CLASSIFICATION.LC_TANKNUMBER%>);" onchange="TextBox_change('WF_TANKNUMBER');">
            <asp:TextBox CssClass="BoxIcon" ID="WF_TANKNUMBER_CODE" runat="server" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a id="WF_TANKNUMBER_TEXT">
            <asp:Label ID="WF_TANKNUMBER_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 型式 -->
        <a id="WF_MODEL_LABEL">型式</a>

        <a class="ef"  id="WF_MODEL" ondblclick="Field_DBclick('WF_MODEL', <%=LIST_BOX_CLASSIFICATION.LC_TANKMODEL%>);" onchange="TextBox_change('WF_MODEL');">
            <asp:TextBox CssClass="BoxIcon" ID="WF_MODEL_CODE" runat="server" onblur="MsgClear();"></asp:TextBox>
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

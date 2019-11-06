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
            <input type="button" id="WF_ButtonDO" value="実行"  style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
        </a>
        <a style="position:fixed;top:2.8em;left:67em;">
            <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
        </a>

        <!-- ○ 変動項目 ○ -->
        <!-- 会社コード -->
        <a style="display:none; position:fixed; top:5.5em; left:4em; font-weight:bold; text-decoration:underline;">会社コード</a>

        <a style="display:none; position:fixed; top:5.5em; left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="display:none; position:fixed; top:5.5em; left:27em;">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        <!-- JOT車番 -->
        <a style="position:fixed; top:7.7em; left:4em; font-weight:bold;">JOT車番</a>

        <a style="position:fixed; top:7.7em; left:18em;">
            <asp:TextBox ID="WF_TANKNUMBER" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:7.7em; left:27em;">
            <asp:Label ID="WF_TANKNUMBER_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        <!-- 型式 -->
        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold;">型式</a>

        <a style="position:fixed; top:9.9em; left:18em;">
            <asp:TextBox ID="WF_MODEL" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:9.9em; left:27em;">
            <asp:Label ID="WF_MODEL_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
<%--        <!-- JOT車番 -->
        <a style="position:fixed; top:7.7em; left:4em; font-weight:bold; text-decoration:underline;">JOT車番</a>

        <a style="position:fixed; top:7.7em; left:18em;" ondblclick="Field_DBclick('WF_TANKNUMBER', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_TANKNUMBER');">
            <asp:TextBox ID="TextBox1" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:7.7em; left:27em;">
            <asp:Label ID="Label1" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        <!-- 型式 -->
        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;">型式</a>

        <a style="position:fixed; top:9.9em; left:18em;" ondblclick="Field_DBclick('WF_MODEL ', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_MODEL');">
            <asp:TextBox ID="TextBox2" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:9.9em; left:27em;">
            <asp:Label ID="Label2" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>--%>
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

<%@ Page Title="T00007S" Language="vb" AutoEventWireup="false" CodeBehind="GRT00007SELECT.aspx.vb" Inherits="OFFICE.GRT00007SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRT00007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="T00007SH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/T00007S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/T00007S.js")%>'></script>
</asp:Content>

<asp:Content ID="T00007S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <a style="position:fixed;top:2.8em;left:58em;">
            <input type="button" id="WF_ButtonRESTART" value="再開"  style="Width:5em" onclick="ButtonClick('WF_ButtonRESTART');" />
        </a>
        <a style="position:fixed; top:2.8em; left:62.5em;">
            <input type="button" id="WF_ButtonDO" value="実行" style="Width:5em;" onclick="ButtonClick('WF_ButtonDO');" />
        </a>
        <a style="position:fixed; top:2.8em; left:67em;">
            <input type="button" id="WF_ButtonEND" value="終了" style="Width:5em;" onclick="ButtonClick('WF_ButtonEND');" />
        </a>

        <!-- ○ 変動項目 ○ -->
        <!-- 会社コード -->
        <a style="position:fixed; top:7.7em; left:4em; font-weight:bold; text-decoration:underline;">会社コード</a>

        <a style="position:fixed; top:7.7em; left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:7.7em; left:27em;">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 対象年月 -->
        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;">対象年月</a>

        <a style="position:fixed; top:9.9em; left:18em;" ondblclick="Field_DBclick('WF_TAISHOYM', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:TextBox ID="WF_TAISHOYM" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>

        <!-- 配属部署 -->
        <a style="position:fixed; top:12.1em; left:4em; font-weight:bold; text-decoration:underline;">配属部署</a>

        <a style="position:fixed; top:12.1em; left:18em;" ondblclick="Field_DBclick('WF_HORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_HORG');">
            <asp:TextBox ID="WF_HORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:12.1em; left:27em;">
            <asp:Label ID="WF_HORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 職務区分 -->
        <a style="position:fixed; top:14.3em; left:4em; font-weight:bold; text-decoration:underline;">職務区分</a>

        <a style="position:fixed; top:14.3em; left:18em;" ondblclick="Field_DBclick('WF_STAFFKBN', <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>);" onchange="TextBox_change('WF_STAFFKBN');">
            <asp:TextBox ID="WF_STAFFKBN" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:14.3em; left:27em;">
            <asp:Label ID="WF_STAFFKBN_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 　従業員コード　 -->
        <a style="position:fixed;top:16.5em;left:4em;font-weight:bold;text-decoration:underline">従業員</a>
        <a style="position:fixed;top:16.5em;left:11.5em;">コード</a>

        <a style="position:fixed;top:16.5em;left:18em;" ondblclick="Field_DBclick('WF_STAFFCODE', <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>);" onchange="TextBox_change('WF_STAFFCODE')">
            <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
        </a>
        <a style="position:fixed;top:16.5em;left:27em;">
            <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Text="" Width="17em" Height="1.2em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 　従業員名称　 -->
        <a style="position:fixed;top:18.7em;left:11.5em;">名称</a>

        <a style="position:fixed;top:18.7em;left:18em;" onchange="TextBox_change('WF_STAFFNAME')">
            <asp:TextBox ID="WF_STAFFNAME" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
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
        <input id="WF_Restart" runat="server" value=""  type="text" />              <!-- 一時保管 -->
    </div>
</asp:Content>

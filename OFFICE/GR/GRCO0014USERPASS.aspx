<%@ Page Title="CO0014" Language="vb" AutoEventWireup="false" CodeBehind="GRCO0014USERPASS.aspx.vb" Inherits="OFFICE.GRCO0014USERPASS" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Register Src="~/GR/inc/GRCO0014WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="CO0014H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/CO0014.css")%>' rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="CO0014" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <a style="position:fixed; top:2.8em; left:62.5em;">
            <input type="button" id="WF_ButtonUPDATE" value="更新" style="Width:5em;" onclick="ButtonClick('WF_ButtonUPDATE');" />
        </a>
        <a style="position:fixed; top:2.8em; left:67em;">
            <input type="button" id="WF_ButtonEND" value="終了" style="Width:5em;" onclick="ButtonClick('WF_ButtonEND');" />
        </a>

        <!-- ○ 変動項目 ○ -->
        <!-- 新パスワード -->
        <a style="position:fixed; top:7.7em; left:7.7em; font-weight:bold;">新しいパスワード</a>

        <a style="position:fixed; top:7.7em; left:18em;">
            <asp:TextBox ID="WF_PASSWORD" runat="server" Height="1.4em" Width="10em" TextMode="Password"></asp:TextBox>
        </a>

        <!-- (再)新パスワード -->
        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold;">(再入力)新しいパスワード</a>

        <a style="position:fixed; top:9.9em; left:18em;">
            <asp:TextBox ID="WF_PASSWORD_R" runat="server" Height="1.4em" Width="10em" TextMode="Password"></asp:TextBox>
        </a>

        <a style="position:fixed; top:16.5em; left:4em; font-weight:bold;">
            <asp:Label ID="WF_INFO" runat="server" font-size="X-Large" ForeColor="Red"></asp:Label>
        </a>
    </div>

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_ButtonClick" runat="server" value="" type="text" />       <!-- ボタン押下 -->
    </div>
</asp:Content>

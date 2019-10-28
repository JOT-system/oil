<%@ Page Title="OIM0004S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0004StationSearch.aspx.vb" Inherits="OFFICE.OIM0004StationSearch" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/OIM0004WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIM0004SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/OIM0004S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/OIM0004S.js")%>'></script>
</asp:content>

<asp:Content ID="OIM0004S" ContentPlaceHolderID="contents1" runat="server">
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
        <a style="position:fixed; top:7.7em; left:4em; font-weight:bold; text-decoration:underline;">会社コード</a>

        <a style="position:fixed; top:7.7em; left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:7.7em; left:27em;">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        <!-- 運用部署 -->
        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;">運用部署</a>

        <a style="position:fixed; top:9.9em; left:18em;" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
            <asp:TextBox ID="WF_UORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:9.9em; left:27em;">
            <asp:Label ID="WF_UORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        <!-- 貨物駅コード -->
        <a style="position:fixed; top:12.1em; left:4em; font-weight:bold; text-decoration:underline;">貨物駅コード</a>

        <a style="position:fixed; top:12.1em; left:18em;" ondblclick="Field_DBclick('TxtStationCode', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtStationCode');">
            <asp:TextBox ID="TxtStationCode" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:12.1em; left:27em;">
            <asp:Label ID="LblStationCode" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        <!-- 貨物コード枝番 -->
        <a style="position:fixed; top:14.3em; left:4em; font-weight:bold;">貨物コード枝番</a>
        <a style="position:fixed; top:14.3em; left:18em;" onchange="TextBox_change('TxtStationCode');">
            <asp:TextBox ID="TxtBranch" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:14.3em; left:27em;">
            <asp:Label ID="LblBranch" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
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

<%@ Page Title="OIM0004S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0004StationSearch.aspx.vb" Inherits="OFFICE.OIM0004StationSearch" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>貨物駅マスタメンテナンス（検索）</title>

    <%--全画面共通のスタイルシート --%>
    <link href="~/css/common.css" rel="stylesheet" type="text/css" />

    <%--共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/common.js") %>' type="text/javascript" charset="utf-8"></script>
    <%-- 左ボックスカレンダー使用の場合のスクリプト --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/script/calendar.js") %>' charset="utf-8"></script>

</head>
<body>
    <form id="FORM_OIM0004" runat="server">

        <!--ヘッダーボックス -->
        <table class="titlebox" id="tblTitlebox">
            <tr>
                <td class="WF_TITLEID">
                    <%= IIf(Me.lblTitleId.Text <> "", "ID:", "") %>
                    <asp:Label ID="lblTitleId" runat ="server" Text="">0001</asp:Label>
                </td>
                <td class="WF_TITLETEXT">
                    <asp:Label ID="lblTitleText" runat="server" Text="">BREAKER SEARCH</asp:Label>
                </td>
                <td class="WF_TITLECAMP" style="text-align:right;">
                    <asp:Label ID="lblTitleCompany" runat="server" Text="">会社</asp:Label>
                </td>
                <td rowspan="2">
                    <div id="divShowRightBoxBg"><div id="divShowRightBox" ></div></div>
                </td>
            </tr>
            <tr>
                <td class="WF_TITLEID">
                    <asp:Label ID="lblTitleOffice" runat="server" Text="">OFFICE</asp:Label>
                </td>
                <td></td>
                <td class="WF_TITLEDATE" style="text-align:right;">
                    <asp:Label ID="lblTitleDate" runat="server" Text="">2019/10/10</asp:Label>
                </td>
            </tr>
        </table>
        <%-- メインボックス --%>
        <table class="searchbox" id="tblmainbox">
            <tr>
                <td colspan="2" style="text-align:right;">
                    <asp:Button ID="BtnSearch" runat="server" Text="検索" />
                    <asp:Button ID="BtnEnd" runat="server" Text="終了" />
                </td>
            </tr>
            <tr>
                <td style="width:15%">
                    <asp:Label ID="LblGoodsStationCode" runat="server" Text="Label">貨物駅コード</asp:Label>
                </td>
                <td class="WF_TEXT_LEFT"  style="width:85%">
                    <asp:TextBox ID="TxtGoodsStationCode" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="LblGoodsStationCodeBranch" runat="server" Text="Label">貨物コード枝番</asp:Label>
                </td>
                <td class="WF_TEXT_LEFT">
                    <asp:TextBox ID="TxtGoodsStationCodeBranch" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr></tr>
        </table>
    </form>
</body>
</html>

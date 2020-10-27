<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0007WRKINC.ascx.vb" Inherits="JOTWEB.OIT0007WRKINC" %>
<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>
    <!-- 営業所コード -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODE" runat="server"></asp:TextBox>
    <!-- 営業所名 -->
    <asp:TextBox ID="WF_SEL_SALESOFFICE" runat="server"></asp:TextBox>
    <!-- 検索画面スキップフラグ 一覧から戻る際に使用,（1:検索画面スキップ、それ以外：検索画面維持） -->
    <asp:TextBox ID="WF_SEL_CAN_BYPASS_SERACH" runat="server"></asp:TextBox>
</div>
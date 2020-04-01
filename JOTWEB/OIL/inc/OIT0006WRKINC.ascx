<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0006WRKINC.ascx.vb" Inherits="JOTWEB.OIT0006WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- ■回送検索用 -->
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>
    <!-- 営業所コード(検索退避用) -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODEMAP" runat="server"></asp:TextBox>
    <!-- 営業所コード -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODE" runat="server"></asp:TextBox>
    <!-- 営業所名 -->
    <asp:TextBox ID="WF_SEL_SALESOFFICE" runat="server"></asp:TextBox>
    <!-- 年月日 -->
    <asp:TextBox ID="WF_SEL_DATE" runat="server"></asp:TextBox>
    <!-- 列車番号 -->
    <asp:TextBox ID="WF_SEL_TRAINNUMBER" runat="server"></asp:TextBox>
    <!-- 状態(コード) -->
    <asp:TextBox ID="WF_SEL_STATUSCODE" runat="server"></asp:TextBox>
    <!-- 状態(名) -->
    <asp:TextBox ID="WF_SEL_STATUS" runat="server"></asp:TextBox>
    <!-- 目的(コード) -->
    <asp:TextBox ID="WF_SEL_OBJECTIVECODE" runat="server"></asp:TextBox>
    <!-- 目的(名) -->
    <asp:TextBox ID="WF_SEL_OBJECTIVENAME" runat="server"></asp:TextBox>
    <!-- 着駅(コード(検索退避用)) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATIONMAP" runat="server"></asp:TextBox>
    <!-- 着駅(コード) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATION" runat="server"></asp:TextBox>
    <!-- 着駅(名) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATIONNM" runat="server"></asp:TextBox>

</div>

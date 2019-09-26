<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRMB0007WRKINC.ascx.vb" Inherits="OFFICE.GRMB0007WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>            <!-- 有効年月日(From) -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>           <!-- 有効年月日(To) -->
    <asp:TextBox ID="WF_SEL_MORG" runat="server"></asp:TextBox>             <!-- 管理部署 -->
    <asp:TextBox ID="WF_SEL_HORG" runat="server"></asp:TextBox>             <!-- 配属部署 -->
    <asp:TextBox ID="WF_SEL_STAFFKBN" runat="server"></asp:TextBox>         <!-- 職務区分 -->
    <asp:TextBox ID="WF_SEL_STAFFCODE" runat="server"></asp:TextBox>        <!-- 従業員 -->
</div>

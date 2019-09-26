<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRMC0012WRKINC.ascx.vb" Inherits="OFFICE.GRMC0012WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>             <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_TORICODES" runat="server"></asp:TextBox>        <!-- 取引先コード（出荷） -->
    <asp:TextBox ID="WF_SEL_TORICODET" runat="server"></asp:TextBox>        <!-- 取引先コード（届先） -->
</div>

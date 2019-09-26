<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRMC0001WRKINC.ascx.vb" Inherits="OFFICE.GRMC0001WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>            <!-- 有効年月日(From) -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>           <!-- 有効年月日(To) -->
    <asp:TextBox ID="WF_SEL_BUNRUIF" runat="server"></asp:TextBox>          <!-- 分類(From) -->
    <asp:TextBox ID="WF_SEL_BUNRUIT" runat="server"></asp:TextBox>          <!-- 分類(To) -->
</div>

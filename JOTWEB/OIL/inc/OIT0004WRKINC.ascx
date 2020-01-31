<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0004WRKINC.ascx.vb" Inherits="JOTWEB.OIT0004WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>         <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODEMAP" runat="server"></asp:TextBox> <!-- 営業所コード(検索退避用) -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODE" runat="server"></asp:TextBox> <!-- 営業所コード -->
    <asp:TextBox ID="WF_SEL_SALESOFFICE" runat="server"></asp:TextBox> <!-- 営業所名 -->
    <asp:TextBox ID="WF_SEL_CONSIGNEE" runat="server"></asp:TextBox>         <!-- 空車着駅（発駅）コード -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>         <!-- 開始年月日 -->
    <asp:TextBox ID="WF_SEL_OWNERCODE" runat="server"></asp:TextBox>         <!-- 名義所有者C -->
</div>

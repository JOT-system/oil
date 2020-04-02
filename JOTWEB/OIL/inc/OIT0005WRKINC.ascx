<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0005WRKINC.ascx.vb" Inherits="JOTWEB.OIT0005WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>         <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODEMAP" runat="server"></asp:TextBox> <!-- 営業所コード(検索退避用) -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODE" runat="server"></asp:TextBox> <!-- 営業所コード -->
    <asp:TextBox ID="WF_SEL_SALESOFFICE" runat="server"></asp:TextBox> <!-- 営業所名 -->
    <asp:TextBox ID="WF_SEL_OWNERCODE" runat="server"></asp:TextBox>         <!-- 名義所有者C -->
    <!-- 状況画面で設定 -->
    <asp:TextBox ID="WF_COND_DETAILTYPE" runat="server"></asp:TextBox> <!-- 選択された詳細タイプ -->
    <asp:TextBox ID="WF_COND_DETAILTYPENAME" runat="server"></asp:TextBox> <!-- 選択された詳細タイプ名 -->
    <!-- 一覧画面用で設定 -->
    <asp:TextBox ID="WF_LISTSEL_TANKNUMBER" runat="server"></asp:TextBox>
</div>

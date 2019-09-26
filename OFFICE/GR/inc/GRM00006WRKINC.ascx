<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRM00006WRKINC.ascx.vb" Inherits="OFFICE.GRM00006WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>            <!-- 有効年月日(From) -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>           <!-- 有効年月日(To) -->
    <asp:TextBox ID="WF_SEL_USERID" runat="server"></asp:TextBox>           <!-- ユーザーID -->
    <asp:TextBox ID="WF_SEL_OBJECT" runat="server"></asp:TextBox>           <!-- オブジェクト -->
    <asp:TextBox ID="WF_SEL_STRUCT" runat="server"></asp:TextBox>           <!-- 構造コード -->
</div>

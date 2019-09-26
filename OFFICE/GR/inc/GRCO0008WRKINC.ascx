<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRCO0008WRKINC.ascx.vb" Inherits="OFFICE.GRCO0008WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>            <!-- 有効年月日(From) -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>           <!-- 有効年月日(To) -->
    <asp:TextBox ID="WF_SEL_MAPIDPF" runat="server"></asp:TextBox>          <!-- 親画面ID(From) -->
    <asp:TextBox ID="WF_SEL_MAPIDPT" runat="server"></asp:TextBox>          <!-- 親画面ID(To) -->
    <asp:TextBox ID="WF_SEL_MAPIDF" runat="server"></asp:TextBox>           <!-- 子画面ID(From) -->
    <asp:TextBox ID="WF_SEL_MAPIDT" runat="server"></asp:TextBox>           <!-- 子画面ID(To) -->
</div>

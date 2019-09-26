<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRMC0010WRKINC.ascx.vb" Inherits="OFFICE.GRMC0010WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>            <!-- 有効年月日(From) -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>           <!-- 有効年月日(To) -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>         <!-- 取引先 -->
    <asp:TextBox ID="WF_SEL_ORDERORG" runat="server"></asp:TextBox>         <!-- 受注組織 -->
    <asp:TextBox ID="WF_SEL_OILTYPE" runat="server"></asp:TextBox>          <!-- 油種 -->
</div>

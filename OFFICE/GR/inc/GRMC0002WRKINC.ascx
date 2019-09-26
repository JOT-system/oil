<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRMC0002WRKINC.ascx.vb" Inherits="OFFICE.GRMC0002WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>            <!-- 有効年月日(From) -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>           <!-- 有効年月日(To) -->
    <asp:TextBox ID="WF_SEL_TORICODEF" runat="server"></asp:TextBox>        <!-- 取引先(From) -->
    <asp:TextBox ID="WF_SEL_TORICODET" runat="server"></asp:TextBox>        <!-- 取引先(To) -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>         <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_POSTNUM" runat="server"></asp:TextBox>          <!-- 郵便番号 -->
    <asp:TextBox ID="WF_SEL_ADDR" runat="server"></asp:TextBox>             <!-- 住所 -->
    <asp:TextBox ID="WF_SEL_TEL" runat="server"></asp:TextBox>              <!-- 電話番号 -->
    <asp:TextBox ID="WF_SEL_FAX" runat="server"></asp:TextBox>              <!-- FAX番号 -->
</div>

<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0008WRKINC.ascx.vb" Inherits="JOTWEB.OIT0008WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LAST_KEIJYO_YM" runat="server"></asp:TextBox>           <!-- 最終表示計上年月 -->
    <asp:TextBox ID="WF_SEL_LAST_OFFICECODE" runat="server"></asp:TextBox>          <!-- 最終表示営業所コード -->
    <asp:TextBox ID="WF_SEL_INIT_KEIJYO_YM" runat="server"></asp:TextBox>           <!-- 締め済み計上年月 -->
    <!-- 明細画面 -->
    <asp:TextBox ID="WF_SEL_OFFICENAME" runat="server"></asp:TextBox>               <!-- 営業所名 -->
    <asp:TextBox ID="WF_SEL_LINE" runat="server"></asp:TextBox>                     <!-- # -->
    <asp:TextBox ID="WF_SEL_ACCOUNTCODE" runat="server"></asp:TextBox>              <!-- 勘定科目コード -->
    <asp:TextBox ID="WF_SEL_ACCOUNTNAME" runat="server"></asp:TextBox>              <!-- 勘定科目名 -->
    <asp:TextBox ID="WF_SEL_SEGMENTCODE" runat="server"></asp:TextBox>              <!-- セグメント -->
    <asp:TextBox ID="WF_SEL_SEGMENTNAME" runat="server"></asp:TextBox>              <!-- セグメント名 -->
    <asp:TextBox ID="WF_SEL_SEGMENTBRANCHCODE" runat="server"></asp:TextBox>        <!-- セグメント枝番 -->
    <asp:TextBox ID="WF_SEL_SEGMENTBRANCHNAME" runat="server"></asp:TextBox>        <!-- セグメント枝番名 -->
    <asp:TextBox ID="WF_SEL_SHIPPERSCODE" runat="server"></asp:TextBox>             <!-- 荷受人コード -->
    <asp:TextBox ID="WF_SEL_SHIPPERSNAME" runat="server"></asp:TextBox>             <!-- 荷受人名 -->
    <asp:TextBox ID="WF_SEL_INVOICECODE" runat="server"></asp:TextBox>              <!-- 請求先コード -->
    <asp:TextBox ID="WF_SEL_INVOICENAME" runat="server"></asp:TextBox>              <!-- 請求先名 -->
    <asp:TextBox ID="WF_SEL_INVOICEDEPTNAME" runat="server"></asp:TextBox>          <!-- 請求先部門 -->
    <asp:TextBox ID="WF_SEL_PAYEECODE" runat="server"></asp:TextBox>                <!-- 支払先コード -->
    <asp:TextBox ID="WF_SEL_PAYEENAME" runat="server"></asp:TextBox>                <!-- 支払先名 -->
    <asp:TextBox ID="WF_SEL_PAYEEDEPTNAME" runat="server"></asp:TextBox>            <!-- 支払先部門 -->
    <asp:TextBox ID="WF_SEL_TEKIYOU" runat="server"></asp:TextBox>                  <!-- 摘要 -->
</div>

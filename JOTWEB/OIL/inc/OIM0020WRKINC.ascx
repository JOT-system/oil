<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0020WRKINC.ascx.vb" Inherits="JOTWEB.OIM0020WRKINC" %>
<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>                      <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_FROMYMD" runat="server"></asp:TextBox>                  <!-- 掲載開始日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>                   <!-- 掲載終了日 -->
    <asp:TextBox ID="WF_SEL_DISPFLAGS_LIST" runat="server"></asp:TextBox>           <!-- 掲載フラグ -->
    <asp:TextBox ID="WF_LIST_GUIDANCENO" runat="server"></asp:TextBox>              <!-- 選択行 -->
</div>

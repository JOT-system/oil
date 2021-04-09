<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0002WRKINC.ascx.vb" Inherits="JOTWEB.OIM0002WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_SEL_CAMPCODE2" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_SEL_CAMPCODE_L" runat="server"></asp:TextBox>
    <!-- 会社名称 -->
    <asp:TextBox ID="WF_SEL_CAMPNAME" runat="server"></asp:TextBox>
    <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_SEL_ORGCODE2" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_SEL_ORGCODE_L" runat="server"></asp:TextBox>
    <!-- 組織名 -->
    <asp:TextBox ID="WF_SEL_ORGNAME" runat="server"></asp:TextBox>
    <!-- 組織名（短） -->
    <asp:TextBox ID="WF_SEL_ORGNAMES" runat="server"></asp:TextBox>
    <!-- 組織名カナ -->
    <asp:TextBox ID="WF_SEL_ORGNAMEKANA" runat="server"></asp:TextBox>
    <!-- 組織名カナ（短） -->
    <asp:TextBox ID="WF_SEL_ORGNAMEKANAS" runat="server"></asp:TextBox>
    <!-- 開始年月日 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>
    <!-- 終了年月日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>
    <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_SELECT" runat="server"></asp:TextBox>
    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>
    <!-- 詳細画面更新 -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>
</div>

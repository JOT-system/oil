<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0004WRKINC.ascx.vb" Inherits="JOTWEB.OIM0004WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>
    <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>
    <!-- 貨物駅コード -->
    <asp:TextBox ID="WF_SEL_STATIONCODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_SEL_STATIONCODE2" runat="server"></asp:TextBox>
    <!-- 貨物コード枝番 -->
    <asp:TextBox ID="WF_SEL_BRANCH" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_SEL_BRANCH2" runat="server"></asp:TextBox>
    <!-- 貨物駅名称 -->
    <asp:TextBox ID="WF_SEL_STATONNAME" runat="server"></asp:TextBox>
    <!-- 貨物駅名称カナ -->
    <asp:TextBox ID="WF_SEL_STATIONNAMEKANA" runat="server"></asp:TextBox>
    <!-- 貨物駅種別名称 -->
    <asp:TextBox ID="WF_SEL_TYPENAME" runat="server"></asp:TextBox>
    <!-- 貨物駅種別名称カナ -->
    <asp:TextBox ID="WF_SEL_TYPENAMEKANA" runat="server"></asp:TextBox>
    <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>
    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>

</div>

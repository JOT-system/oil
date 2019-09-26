<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRT00010WRKINC.ascx.vb" Inherits="OFFICE.GRT00010WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>             <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_TAISHOYM" runat="server"></asp:TextBox>             <!-- 申請年月 -->
    <asp:TextBox ID="WF_SEL_HORG" runat="server"></asp:TextBox>                 <!-- 配属部署 -->
    <asp:TextBox ID="WF_SEL_APPROVALDISPTYPE" runat="server"></asp:TextBox>     <!-- 承認区分 -->
    <asp:TextBox ID="WF_SEL_GRIDPOSITION" runat="server"></asp:TextBox>         <!-- グリッドポジション保管　 -->
            
    <asp:TextBox ID="WF_T09_CAMPCODE" runat="server"></asp:TextBox>             <!-- 会社 -->
    <asp:TextBox ID="WF_T09_TAISHOYM" runat="server"></asp:TextBox>             <!-- 配属部署 -->
    <asp:TextBox ID="WF_T09_HORG" runat="server"></asp:TextBox>                 <!-- 対象年月 -->
    <asp:TextBox ID="WF_T09_STAFFCODE" runat="server"></asp:TextBox>            <!-- 従業員 -->
    <asp:TextBox ID="WF_T09_STAFFNAME" runat="server"></asp:TextBox>            <!-- 従業員名 -->
    <asp:TextBox ID="WF_T09_STAFFKBN" runat="server"></asp:TextBox>             <!-- 職種区分 -->
    <asp:TextBox ID="WF_T09_MAPID" runat="server"></asp:TextBox>                <!-- 画面ID -->
    <asp:TextBox ID="WF_T09_MAPVARIANT" runat="server"></asp:TextBox>           <!-- MAP変数 -->
</div>

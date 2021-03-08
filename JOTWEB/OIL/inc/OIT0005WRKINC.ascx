<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0005WRKINC.ascx.vb" Inherits="JOTWEB.OIT0005WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>         <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_OWNERCODE" runat="server"></asp:TextBox>         <!-- 名義所有者C -->
    <asp:TextBox ID="WF_SEL_SALESOFFICE_TILES" runat="server"></asp:TextBox> <!-- 営業リスト -->
    <!-- 状況画面で設定 -->
    <asp:TextBox ID="WF_COND_DETAILTYPE" runat="server"></asp:TextBox> <!-- 選択された詳細タイプ -->
    <asp:TextBox ID="WF_COND_DETAILTYPENAME" runat="server"></asp:TextBox> <!-- 選択された詳細タイプ名 -->
    <!-- 一覧画面用で設定 -->
    <asp:TextBox ID="WF_LISTSEL_TANKNUMBER" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_LISTSEL_INPTBL" runat="server"></asp:TextBox> <!-- 一覧画面のデータファイルパス -->
    <!-- メニュー画面より遷移した場合用で設定 -->
    <asp:TextBox ID="WF_MAIN_OFFICECODE" runat="server"></asp:TextBox>  <!-- 対象の営業所コード -->
    <asp:TextBox ID="WF_MAIN_VIEWTABLE" runat="server"></asp:TextBox>   <!-- 対象のVIEWテーブル -->
    <asp:TextBox ID="WF_MAIN_VIEWSORT" runat="server"></asp:TextBox>    <!-- 対象のソートKEY -->

</div>

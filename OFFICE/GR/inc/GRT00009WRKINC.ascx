<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRT00009WRKINC.ascx.vb" Inherits="OFFICE.GRT00009WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_TAISHOYM" runat="server"></asp:TextBox>                 <!-- 対象年月 -->
    <asp:TextBox ID="WF_SEL_HORG" runat="server"></asp:TextBox>                     <!-- 配属部署 -->
    <asp:TextBox ID="WF_SEL_STAFFKBN" runat="server"></asp:TextBox>                 <!-- 社員区分 -->
    <asp:TextBox ID="WF_SEL_STAFFCODE" runat="server"></asp:TextBox>                <!-- 従業員(コード) -->
    <asp:TextBox ID="WF_SEL_STAFFNAMES" runat="server"></asp:TextBox>               <!-- 従業員(名称) -->

    <asp:TextBox ID="WF_SEL_ONLY" runat="server"></asp:TextBox>                     <!-- 勤怠個人 -->
    <asp:TextBox ID="WF_SEL_ONLY_ORG" runat="server"></asp:TextBox>                 <!-- 勤怠個人 部署 -->
    <asp:TextBox ID="WF_SEL_ONLY_STAFF" runat="server"></asp:TextBox>               <!-- 勤怠個人 従業員 -->
    <asp:TextBox ID="WF_SEL_LIMITFLG" runat="server"></asp:TextBox>                 <!-- 締フラグ -->
    <asp:TextBox ID="WF_SEL_PERMITCODE" runat="server"></asp:TextBox>               <!-- 権限コード -->
    <asp:TextBox ID="WF_SEL_XMLsaveTMP" runat="server"></asp:TextBox>               <!-- 一時保存ファイル -->
    <asp:TextBox ID="WF_SEL_RESTARTFLG" runat="server"></asp:TextBox>               <!-- 再開フラグ -->

    <!-- 承認画面(T0010) -->
    <asp:TextBox ID="WF_T09_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_T09_TAISHOYM" runat="server"></asp:TextBox>                 <!-- 対象年月 -->
    <asp:TextBox ID="WF_T09_HORG" runat="server"></asp:TextBox>                     <!-- 配属部署 -->
    <asp:TextBox ID="WF_T09_STAFFKBN" runat="server"></asp:TextBox>                 <!-- 社員区分 -->
    <asp:TextBox ID="WF_T09_STAFFCODE" runat="server"></asp:TextBox>                <!-- 従業員(コード) -->
    <asp:TextBox ID="WF_T09_STAFFNAME" runat="server"></asp:TextBox>                <!-- 従業員(名称) -->
    <asp:TextBox ID="WF_T09_MAPID" runat="server"></asp:TextBox>                    <!-- 画面ID -->
    <asp:TextBox ID="WF_T09_MAPVARIANT" runat="server"></asp:TextBox>               <!-- MAP変数 -->

    <asp:TextBox ID="WF_T10_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_T10_TAISHOYM" runat="server"></asp:TextBox>                 <!-- 申請年月 -->
    <asp:TextBox ID="WF_T10_HORG" runat="server"></asp:TextBox>                     <!-- 配属部署 -->
    <asp:TextBox ID="WF_SEL_APPROVALDISPTYPE" runat="server"></asp:TextBox>         <!-- 承認区分 -->
    <asp:TextBox ID="WF_SEL_GRIDPOSITION" runat="server"></asp:TextBox>             <!-- 明細行位置 -->
    <asp:TextBox ID="WF_T10_VIEWID" runat="server"></asp:TextBox>                   <!-- 画面ID -->
</div>

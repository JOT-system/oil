<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0015WRKINC.ascx.vb" Inherits="JOTWEB.OIM0015WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_CONSIGNEECODE" runat="server"></asp:TextBox>            <!-- 荷受人コード -->
    <asp:TextBox ID="WF_SEL_SHIPPERSCODE" runat="server"></asp:TextBox>             <!-- 荷主コード -->
    <asp:TextBox ID="WF_SEL_OILCODE" runat="server"></asp:TextBox>                  <!-- 油種コード -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_CONSIGNEECODE2" runat="server"></asp:TextBox>           <!-- 荷受人コード -->
    <asp:TextBox ID="WF_SEL_SHIPPERSCODE2" runat="server"></asp:TextBox>            <!-- 荷主コード -->
    <asp:TextBox ID="WF_SEL_FROMMD" runat="server"></asp:TextBox>                   <!-- 開始月日 -->
    <asp:TextBox ID="WF_SEL_TOMD" runat="server"></asp:TextBox>                     <!-- 終了月日 -->
    <asp:TextBox ID="WF_SEL_OILCODE2" runat="server"></asp:TextBox>                 <!-- 油種コード -->
    <asp:TextBox ID="WF_SEL_TANKCAP" runat="server"></asp:TextBox>                  <!-- タンク容量 -->
    <asp:TextBox ID="WF_SEL_TARGETCAPRATE" runat="server"></asp:TextBox>            <!-- 目標在庫率 -->
    <asp:TextBox ID="WF_SEL_DS" runat="server"></asp:TextBox>                       <!-- Ｄ／Ｓ -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>               <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                   <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                  <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>               <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_UPDTIMSTP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->

    <!-- 詳細画面更新 -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>
</div>

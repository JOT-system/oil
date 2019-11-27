<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0001WRKINC.ascx.vb" Inherits="JOTWEB.OIT0001WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>
    <!-- 営業所 -->
    <asp:TextBox ID="WF_SEL_SALESOFFICE" runat="server"></asp:TextBox>
    <!-- 拠点 -->
    <asp:TextBox ID="WF_SEL_BASE" runat="server"></asp:TextBox>
    <!-- 列車番号 -->
    <asp:TextBox ID="WF_SEL_TRAINNUMBER" runat="server"></asp:TextBox>

    <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>
    <!-- 受注№ -->
    <asp:TextBox ID="WF_SEL_ORDERNUMBER" runat="server"></asp:TextBox>
    <!-- 登録日 -->
    <asp:TextBox ID="WF_SEL_REGISTRATIONDATE" runat="server"></asp:TextBox>
    <!-- ステータス -->
    <asp:TextBox ID="WF_SEL_STATUS" runat="server"></asp:TextBox>
    <!-- 情報 -->
    <asp:TextBox ID="WF_SEL_INFORMATION" runat="server"></asp:TextBox>
    <!-- 受注営業所 -->
    <asp:TextBox ID="WF_SEL_ORDERSALESOFFICE" runat="server"></asp:TextBox>
    <!-- 列車 -->
    <asp:TextBox ID="WF_SEL_TRAIN" runat="server"></asp:TextBox>
    <!-- 発駅 -->
    <asp:TextBox ID="WF_SEL_DEPARTURESTATION" runat="server"></asp:TextBox>
    <!-- 向先 -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATION" runat="server"></asp:TextBox>
    <!-- 積込日 -->
    <asp:TextBox ID="WF_SEL_LOADINGDATE" runat="server"></asp:TextBox>
    <!-- 積車発日 -->
    <asp:TextBox ID="WF_SEL_LOADINGCAR_DEPARTUREDATE" runat="server"></asp:TextBox>
    <!-- 積車着日 -->
    <asp:TextBox ID="WF_SEL_LOADINGCAR_ARRIVALDATE" runat="server"></asp:TextBox>
    <!-- 受入日 -->
    <asp:TextBox ID="WF_SEL_RECEIPTDATE" runat="server"></asp:TextBox>
    <!-- レギュラー(タンク車数) -->
    <asp:TextBox ID="WF_SEL_REGULAR_TANKCAR" runat="server"></asp:TextBox>
    <!-- ハイオク(タンク車数) -->
    <asp:TextBox ID="WF_SEL_HIGHOCTANE_TANKCAR" runat="server"></asp:TextBox>
    <!-- 灯油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_KEROSENE_TANKCAR" runat="server"></asp:TextBox>
    <!-- 未添加灯油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_NOTADDED_KEROSENE_TANKCAR" runat="server"></asp:TextBox>
    <!-- 軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- 3号軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_NUM3DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- 5号軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_NUM5DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- 10号軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_NUM10DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- LSA(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LSA_TANKCAR" runat="server"></asp:TextBox>
    <!-- A重油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_AHEAVY_TANKCAR" runat="server"></asp:TextBox>
    <!-- タンク車合計 -->
    <asp:TextBox ID="WF_SEL_TANKCARTOTAL" runat="server"></asp:TextBox>
    <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>
    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>

</div>
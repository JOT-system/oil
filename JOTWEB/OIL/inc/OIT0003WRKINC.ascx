﻿<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0003WRKINC.ascx.vb" Inherits="JOTWEB.OIT0003WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>
    <!-- 営業所コード -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODE" runat="server"></asp:TextBox>
    <!-- 営業所名 -->
    <asp:TextBox ID="WF_SEL_SALESOFFICE" runat="server"></asp:TextBox>
    <!-- 年月日 -->
    <asp:TextBox ID="WF_SEL_DATE" runat="server"></asp:TextBox>
    <!-- 列車番号 -->
    <asp:TextBox ID="WF_SEL_TRAINNUMBER" runat="server"></asp:TextBox>
    <!-- 荷卸地コード -->
    <asp:TextBox ID="WF_SEL_UNLOADINGCODE" runat="server"></asp:TextBox>
    <!-- 荷卸地名 -->
    <asp:TextBox ID="WF_SEL_UNLOADING" runat="server"></asp:TextBox>
    <!-- 状態コード -->
    <asp:TextBox ID="WF_SEL_STATUSCODE" runat="server"></asp:TextBox>
    <!-- 状態名 -->
    <asp:TextBox ID="WF_SEL_STATUS" runat="server"></asp:TextBox>

    <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>
    <!-- 受注№ -->
    <asp:TextBox ID="WF_SEL_ORDERNUMBER" runat="server"></asp:TextBox>
    <!-- 登録日 -->
    <asp:TextBox ID="WF_SEL_REGISTRATIONDATE" runat="server"></asp:TextBox>
    <!-- ステータス -->
    <asp:TextBox ID="WF_SEL_STATUS1" runat="server"></asp:TextBox>

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
    <!-- 作成フラグ -->
    <asp:TextBox ID="WF_SEL_CREATEFLG" runat="server"></asp:TextBox>

    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>
    <!-- 荷主コード -->
    <asp:TextBox ID="WF_SEL_SHIPPERSCODE" runat="server"></asp:TextBox>
    <!-- 荷主名 -->
    <asp:TextBox ID="WF_SEL_SHIPPERSNAME" runat="server"></asp:TextBox>
    <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_BASECODE" runat="server"></asp:TextBox>
    <!-- 基地名 -->
    <asp:TextBox ID="WF_SEL_BASENAME" runat="server"></asp:TextBox>
    <!-- 荷受人コード -->
    <asp:TextBox ID="WF_SEL_CONSIGNEECODE" runat="server"></asp:TextBox>
    <!-- 荷受人名 -->
    <asp:TextBox ID="WF_SEL_CONSIGNEENAME" runat="server"></asp:TextBox>

</div>
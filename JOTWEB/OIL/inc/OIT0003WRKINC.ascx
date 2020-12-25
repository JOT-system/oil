﻿<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0003WRKINC.ascx.vb" Inherits="JOTWEB.OIT0003WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- ■受注検索用 -->
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>
    <!-- 営業所コード(検索退避用) -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODEMAP" runat="server"></asp:TextBox>
    <!-- 営業所コード -->
    <asp:TextBox ID="WF_SEL_SALESOFFICECODE" runat="server"></asp:TextBox>
    <!-- 営業所名 -->
    <asp:TextBox ID="WF_SEL_SALESOFFICE" runat="server"></asp:TextBox>
    <!-- 年月日 -->
    <asp:TextBox ID="WF_SEL_DATE" runat="server"></asp:TextBox>
    <!-- 発日 -->
    <asp:TextBox ID="WF_SEL_SEARCH_DEPDATE" runat="server"></asp:TextBox>
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
    <!-- 受注キャンセルフラグ -->
    <asp:TextBox ID="WF_SEL_ORDERCANCELFLG" runat="server"></asp:TextBox>

    <!-- ■受注一覧用 -->
    <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>
    <!-- 登録日 -->
    <asp:TextBox ID="WF_SEL_REGISTRATIONDATE" runat="server"></asp:TextBox>
    <!-- 受注営業所(コード) -->
    <asp:TextBox ID="WF_SEL_ORDERSALESOFFICECODE" runat="server"></asp:TextBox>
    <!-- 受注営業所(名) -->
    <asp:TextBox ID="WF_SEL_ORDERSALESOFFICE" runat="server"></asp:TextBox>
    <!-- ステータス(コード) -->
    <asp:TextBox ID="WF_SEL_ORDERSTATUS" runat="server"></asp:TextBox>
    <!-- ステータス(名) -->
    <asp:TextBox ID="WF_SEL_ORDERSTATUSNM" runat="server"></asp:TextBox>
    <!-- 情報(コード) -->
    <asp:TextBox ID="WF_SEL_INFORMATION" runat="server"></asp:TextBox>
    <!-- 情報(名) -->
    <asp:TextBox ID="WF_SEL_INFORMATIONNM" runat="server"></asp:TextBox>
    <!-- 空回日報可否フラグ -->
    <asp:TextBox ID="WF_SEL_EMPTYTURNFLG" runat="server"></asp:TextBox>
    <!-- 積置可否フラグ -->
    <asp:TextBox ID="WF_SEL_STACKINGFLG" runat="server"></asp:TextBox>
    <!-- 利用可否フラグ -->
    <asp:TextBox ID="WF_SEL_USEPROPRIETYFLG" runat="server"></asp:TextBox>
    <!-- 受注№ -->
    <asp:TextBox ID="WF_SEL_ORDERNUMBER" runat="server"></asp:TextBox>
    <!-- 列車 -->
    <asp:TextBox ID="WF_SEL_TRAIN" runat="server"></asp:TextBox>
    <!-- 列車名 -->
    <asp:TextBox ID="WF_SEL_TRAINNAME" runat="server"></asp:TextBox>
    <!-- OT列車 -->
    <asp:TextBox ID="WF_SEL_OTTRAIN" runat="server"></asp:TextBox>
    <!-- パターンコード -->
    <asp:TextBox ID="WF_SEL_PATTERNCODE" runat="server"></asp:TextBox>
    <!-- パターン名 -->
    <asp:TextBox ID="WF_SEL_PATTERNNAME" runat="server"></asp:TextBox>
    <!-- 荷主(コード) -->
    <asp:TextBox ID="WF_SEL_SHIPPERSCODE" runat="server"></asp:TextBox>
    <!-- 荷主(名) -->
    <asp:TextBox ID="WF_SEL_SHIPPERSNAME" runat="server"></asp:TextBox>
    <!-- 荷受人(コード) -->
    <asp:TextBox ID="WF_SEL_CONSIGNEECODE" runat="server"></asp:TextBox>
    <!-- 荷受人(名) -->
    <asp:TextBox ID="WF_SEL_CONSIGNEENAME" runat="server"></asp:TextBox>
    <!-- 発駅(コード) -->
    <asp:TextBox ID="WF_SEL_DEPARTURESTATION" runat="server"></asp:TextBox>
    <!-- 発駅(名) -->
    <asp:TextBox ID="WF_SEL_DEPARTURESTATIONNM" runat="server"></asp:TextBox>
    <!-- 着駅(コード) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATION" runat="server"></asp:TextBox>
    <!-- 着駅(名) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATIONNM" runat="server"></asp:TextBox>
    <!-- 戻着駅(コード) -->
    <asp:TextBox ID="WF_SEL_CHANGERETSTATION" runat="server"></asp:TextBox>
    <!-- 戻着駅(名) -->
    <asp:TextBox ID="WF_SEL_CHANGERETSTATIONNM" runat="server"></asp:TextBox>

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

    <!-- レギュラー(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_REGULARCH_TANKCAR" runat="server"></asp:TextBox>
    <!-- ハイオク(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_HIGHOCTANECH_TANKCAR" runat="server"></asp:TextBox>
    <!-- 灯油(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_KEROSENECH_TANKCAR" runat="server"></asp:TextBox>
    <!-- 未添加灯油(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_NOTADDED_KEROSENECH_TANKCAR" runat="server"></asp:TextBox>
    <!-- 軽油(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_DIESELCH_TANKCAR" runat="server"></asp:TextBox>
    <!-- 3号軽油(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_NUM3DIESELCH_TANKCAR" runat="server"></asp:TextBox>
    <!-- 5号軽油(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_NUM5DIESELCH_TANKCAR" runat="server"></asp:TextBox>
    <!-- 10号軽油(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_NUM10DIESELCH_TANKCAR" runat="server"></asp:TextBox>
    <!-- LSA(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_LSACH_TANKCAR" runat="server"></asp:TextBox>
    <!-- A重油(タンク車数)割当 -->
    <asp:TextBox ID="WF_SEL_AHEAVYCH_TANKCAR" runat="server"></asp:TextBox>
    <!-- タンク車合計(割当) -->
    <asp:TextBox ID="WF_SEL_TANKCARTOTALCH" runat="server"></asp:TextBox>

    <!-- 積込日(予定) -->
    <asp:TextBox ID="WF_SEL_LODDATE" runat="server"></asp:TextBox>
    <!-- 積車発日(予定) -->
    <asp:TextBox ID="WF_SEL_DEPDATE" runat="server"></asp:TextBox>
    <!-- 積車着日(予定) -->
    <asp:TextBox ID="WF_SEL_ARRDATE" runat="server"></asp:TextBox>
    <!-- 受入日(予定) -->
    <asp:TextBox ID="WF_SEL_ACCDATE" runat="server"></asp:TextBox>
    <!-- 空車着日(予定) -->
    <asp:TextBox ID="WF_SEL_EMPARRDATE" runat="server"></asp:TextBox>

    <!-- 積込日(実績) -->
    <asp:TextBox ID="WF_SEL_ACTUALLODDATE" runat="server"></asp:TextBox>
    <!-- 積車発日(実績) -->
    <asp:TextBox ID="WF_SEL_ACTUALDEPDATE" runat="server"></asp:TextBox>
    <!-- 積車着日(実績) -->
    <asp:TextBox ID="WF_SEL_ACTUALARRDATE" runat="server"></asp:TextBox>
    <!-- 受入日(実績) -->
    <asp:TextBox ID="WF_SEL_ACTUALACCDATE" runat="server"></asp:TextBox>
    <!-- 空車着日(実績) -->
    <asp:TextBox ID="WF_SEL_ACTUALEMPARRDATE" runat="server"></asp:TextBox>
    <!-- 貨車連結順序表№ -->
    <asp:TextBox ID="WF_SEL_LINKNO" runat="server"></asp:TextBox>
    <!-- 貨車連結順序表№(受注　→　貨車連結順序表(作成用)) -->
    <asp:TextBox ID="WF_SEL_LINKNO_ORDER" runat="server"></asp:TextBox>

    <!-- 計上年月日 -->
    <asp:TextBox ID="WF_SEL_KEIJYOYMD" runat="server"></asp:TextBox>
    <!-- 売上金額 -->
    <asp:TextBox ID="WF_SEL_SALSE" runat="server"></asp:TextBox>
    <!-- 売上消費税額 -->
    <asp:TextBox ID="WF_SEL_SALSETAX" runat="server"></asp:TextBox>
    <!-- 売上合計金額 -->
    <asp:TextBox ID="WF_SEL_TOTALSALSE" runat="server"></asp:TextBox>
    <!-- 支払金額 -->
    <asp:TextBox ID="WF_SEL_PAYMENT" runat="server"></asp:TextBox>
    <!-- 支払消費税額 -->
    <asp:TextBox ID="WF_SEL_PAYMENTTAX" runat="server"></asp:TextBox>
    <!-- 支払合計金額 -->
    <asp:TextBox ID="WF_SEL_TOTALPAYMENT" runat="server"></asp:TextBox>

    <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>

    <!-- 帳票ポップアップ(営業所(コード)) -->
    <asp:TextBox ID="WF_SEL_TH_ORDERSALESOFFICECODE" runat="server"></asp:TextBox>
    <!-- 帳票ポップアップ(営業所(名)) -->
    <asp:TextBox ID="WF_SEL_TH_ORDERSALESOFFICENAME" runat="server"></asp:TextBox>

    <!-- ■受注タンク車割当用 -->
    <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_LINK_LINECNT" runat="server"></asp:TextBox>
    <!-- 貨車連結順序表№ -->
    <asp:TextBox ID="WF_SEL_LINK_LINKNO" runat="server"></asp:TextBox>
    <!-- 登録日 -->
    <asp:TextBox ID="WF_SEL_LINK_REGISTRATIONDATE" runat="server"></asp:TextBox>
    <!-- ステータス -->
    <asp:TextBox ID="WF_SEL_LINK_ORDERSTATUS" runat="server"></asp:TextBox>
    <!-- 情報 -->
    <asp:TextBox ID="WF_SEL_LINK_INFORMATION" runat="server"></asp:TextBox>
    <!-- 前回オーダー№ -->
    <asp:TextBox ID="WF_SEL_LINK_PREORDERNO" runat="server"></asp:TextBox>
    <!-- 列車 -->
    <asp:TextBox ID="WF_SEL_LINK_TRAIN" runat="server"></asp:TextBox>
    <!-- 列車名 -->
    <asp:TextBox ID="WF_SEL_LINK_TRAINNAME" runat="server"></asp:TextBox>
    <!-- 担当営業所 -->
    <asp:TextBox ID="WF_SEL_LINK_ORDERSALESOFFICE" runat="server"></asp:TextBox>
    <!-- 空車発駅(名) -->
    <asp:TextBox ID="WF_SEL_LINK_DEPARTURESTATION" runat="server"></asp:TextBox>
    <!-- 空車着駅(名) -->
    <asp:TextBox ID="WF_SEL_LINK_ARRIVALSTATION" runat="server"></asp:TextBox>
    <!-- レギュラー(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_REGULAR_TANKCAR" runat="server"></asp:TextBox>
    <!-- ハイオク(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_HIGHOCTANE_TANKCAR" runat="server"></asp:TextBox>
    <!-- 灯油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_KEROSENE_TANKCAR" runat="server"></asp:TextBox>
    <!-- 未添加灯油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_NOTADDED_KEROSENE_TANKCAR" runat="server"></asp:TextBox>
    <!-- 軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- 3号軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_NUM3DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- 5号軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_NUM5DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- 10号軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_NUM10DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- LSA(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_LSA_TANKCAR" runat="server"></asp:TextBox>
    <!-- A重油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LINK_AHEAVY_TANKCAR" runat="server"></asp:TextBox>
    <!-- タンク車合計 -->
    <asp:TextBox ID="WF_SEL_LINK_TANKCARTOTAL" runat="server"></asp:TextBox>
    <!-- 空車着日（予定） -->
    <asp:TextBox ID="WF_SEL_LINK_EMPARRDATE" runat="server"></asp:TextBox>
    <!-- 空車着日（実績） -->
    <asp:TextBox ID="WF_SEL_LINK_ACTUALEMPARRDATE" runat="server"></asp:TextBox>

    <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_LINK_DELFLG" runat="server"></asp:TextBox>

    <!-- (根岸営業所)「灯油＋３号軽油＝１０両以上」フラグ -->
    <asp:TextBox ID="WG_SEL_KEROSENE_3DIESEL_FLG" runat="server"></asp:TextBox>

    <!-- ■受注手配（費用入力）用 -->
    <!-- 支払請求№ -->
    <asp:TextBox ID="WF_SEL_BILLINGNO" runat="server"></asp:TextBox>
    <!-- 消費税 -->
    <asp:TextBox ID="WF_SEL_CONSUMPTIONTAX" runat="server"></asp:TextBox>

    <!-- ■共通 -->
    <!-- 作成フラグ -->
    <asp:TextBox ID="WF_SEL_CREATEFLG" runat="server"></asp:TextBox>
    <!-- 作成(貨車連結用)フラグ -->
    <asp:TextBox ID="WF_SEL_CREATELINKFLG" runat="server"></asp:TextBox>

    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>
    <!-- 貨車連結用更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPLINKTBL" runat="server"></asp:TextBox>
    <!-- OT連携用更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPOTLINKAGETBL" runat="server"></asp:TextBox>

    <!-- 明細画面(タブ１)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB1TBL" runat="server"></asp:TextBox>
    <!-- 明細画面(タブ２)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB2TBL" runat="server"></asp:TextBox>
    <!-- 明細画面(タブ３)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB3TBL" runat="server"></asp:TextBox>
    <!-- 明細画面(タブ４)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB4TBL" runat="server"></asp:TextBox>

    <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_BASECODE" runat="server"></asp:TextBox>
    <!-- 基地名 -->
    <asp:TextBox ID="WF_SEL_BASENAME" runat="server"></asp:TextBox>

    <!-- MAPID退避(受注明細画面への遷移制御のため) -->
    <asp:TextBox ID="WF_SEL_MAPIDBACKUP" runat="server"></asp:TextBox>

    <!-- 実績日訂正フラグ -->
    <asp:TextBox ID="WF_SEL_CORRECTIONDATEFLG" runat="server"></asp:TextBox>
    <!-- 手配連絡フラグ -->
    <asp:TextBox ID="WF_SEL_CONTACTFLG" runat="server"></asp:TextBox>
    <!-- 結果受理フラグ -->
    <asp:TextBox ID="WF_SEL_RESULTFLG" runat="server"></asp:TextBox>
    <!-- 託送指示フラグ -->
    <asp:TextBox ID="WF_SEL_DELIVERYFLG" runat="server"></asp:TextBox>

    <!-- 発送順区分 -->
    <asp:TextBox ID="WF_SEL_SHIPORDERCLASS" runat="server"></asp:TextBox>

    <!-- ***** ↓OT外部連携専用↓ ***** -->
    <!-- 外部連携対象営業所コード -->
    <asp:TextBox ID="WF_SEL_OTS_SALESOFFICECODE" runat="server"></asp:TextBox>
    <!-- 外部連携対象営業所名 -->
    <asp:TextBox ID="WF_SEL_OTS_SALESOFFICE" runat="server"></asp:TextBox>
    <!-- 検索画面スキップフラグ 一覧から戻る際に使用,（1:検索画面スキップ、それ以外：検索画面維持） -->
    <asp:TextBox ID="WF_SEL_CAN_BYPASS_SERACH" runat="server"></asp:TextBox>
    <!-- ***** ↑OT外部連携専用↑ ***** -->
</div>
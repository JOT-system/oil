<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0006WRKINC.ascx.vb" Inherits="JOTWEB.OIT0006WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- ■回送検索用 -->
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
    <!-- 列車番号 -->
    <asp:TextBox ID="WF_SEL_TRAINNUMBER" runat="server"></asp:TextBox>
    <!-- 状態(コード) -->
    <asp:TextBox ID="WF_SEL_STATUSCODE" runat="server"></asp:TextBox>
    <!-- 状態(名) -->
    <asp:TextBox ID="WF_SEL_STATUS" runat="server"></asp:TextBox>
    <!-- 目的(コード)(検索退避用) -->
    <asp:TextBox ID="WF_SEL_OBJECTIVECODEMAP" runat="server"></asp:TextBox>
    <!-- 着駅(コード(検索退避用)) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATIONMAP" runat="server"></asp:TextBox>

    <!-- ■回送一覧用 -->
    <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>
    <!-- 登録日 -->
    <asp:TextBox ID="WF_SEL_REGISTRATIONDATE" runat="server"></asp:TextBox>
    <!-- 回送営業所(コード) -->
    <asp:TextBox ID="WF_SEL_KAISOUSALESOFFICECODE" runat="server"></asp:TextBox>
    <!-- 回送営業所(名) -->
    <asp:TextBox ID="WF_SEL_KAISOUSALESOFFICE" runat="server"></asp:TextBox>
    <!-- ステータス(コード) -->
    <asp:TextBox ID="WF_SEL_KAISOUSTATUS" runat="server"></asp:TextBox>
    <!-- ステータス(名) -->
    <asp:TextBox ID="WF_SEL_KAISOUSTATUSNM" runat="server"></asp:TextBox>
    <!-- 情報(コード) -->
    <asp:TextBox ID="WF_SEL_INFORMATION" runat="server"></asp:TextBox>
    <!-- 情報(名) -->
    <asp:TextBox ID="WF_SEL_INFORMATIONNM" runat="server"></asp:TextBox>

    <!-- 回送№ -->
    <asp:TextBox ID="WF_SEL_KAISOUNUMBER" runat="server"></asp:TextBox>
    <!-- 目的(コード) -->
    <asp:TextBox ID="WF_SEL_OBJECTIVECODE" runat="server"></asp:TextBox>
    <!-- 目的(名) -->
    <asp:TextBox ID="WF_SEL_OBJECTIVENAME" runat="server"></asp:TextBox>
    <!-- 列車 -->
    <asp:TextBox ID="WF_SEL_TRAIN" runat="server"></asp:TextBox>
    <!-- 列車名 -->
    <asp:TextBox ID="WF_SEL_TRAINNAME" runat="server"></asp:TextBox>
    <!-- パターンコード -->
    <asp:TextBox ID="WF_SEL_PATTERNCODE" runat="server"></asp:TextBox>
    <!-- パターン名 -->
    <asp:TextBox ID="WF_SEL_PATTERNNAME" runat="server"></asp:TextBox>
    <!-- 運賃フラグ -->
    <asp:TextBox ID="WF_SEL_FAREFLG" runat="server"></asp:TextBox>
    <!-- 発駅(コード) -->
    <asp:TextBox ID="WF_SEL_DEPARTURESTATION" runat="server"></asp:TextBox>
    <!-- 発駅(名) -->
    <asp:TextBox ID="WF_SEL_DEPARTURESTATIONNM" runat="server"></asp:TextBox>
    <!-- 着駅(コード) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATION" runat="server"></asp:TextBox>
    <!-- 着駅(名) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATIONNM" runat="server"></asp:TextBox>
    <!-- タンク車合計 -->
    <asp:TextBox ID="WF_SEL_TANKCARTOTAL" runat="server"></asp:TextBox>

    <!-- 目的(修理) -->
    <asp:TextBox ID="WF_SEL_REPAIR" runat="server"></asp:TextBox>
    <!-- 目的(ＭＣ) -->
    <asp:TextBox ID="WF_SEL_MC" runat="server"></asp:TextBox>
    <!-- 目的(交検) -->
    <asp:TextBox ID="WF_SEL_INSPECTION" runat="server"></asp:TextBox>
    <!-- 目的(全検) -->
    <asp:TextBox ID="WF_SEL_ALLINSPECTION" runat="server"></asp:TextBox>
    <!-- 目的(疎開留置) -->
    <asp:TextBox ID="WF_SEL_INDWELLING" runat="server"></asp:TextBox>
    <!-- 目的(移動) -->
    <asp:TextBox ID="WF_SEL_MOVE" runat="server"></asp:TextBox>

    <!-- 発日(予定) -->
    <asp:TextBox ID="WF_SEL_DEPDATE" runat="server"></asp:TextBox>
    <!-- 着日(予定) -->
    <asp:TextBox ID="WF_SEL_ARRDATE" runat="server"></asp:TextBox>
    <!-- 受入日(予定) -->
    <asp:TextBox ID="WF_SEL_ACCDATE" runat="server"></asp:TextBox>
    <!-- 空車着日(予定) -->
    <asp:TextBox ID="WF_SEL_EMPARRDATE" runat="server"></asp:TextBox>
    <!-- 発日(実績) -->
    <asp:TextBox ID="WF_SEL_ACTUALDEPDATE" runat="server"></asp:TextBox>
    <!-- 着日(実績) -->
    <asp:TextBox ID="WF_SEL_ACTUALARRDATE" runat="server"></asp:TextBox>
    <!-- 受入日(実績) -->
    <asp:TextBox ID="WF_SEL_ACTUALACCDATE" runat="server"></asp:TextBox>
    <!-- 空車着日(実績) -->
    <asp:TextBox ID="WF_SEL_ACTUALEMPARRDATE" runat="server"></asp:TextBox>

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

    <!-- 荷主(コード) -->
    <asp:TextBox ID="WF_SEL_SHIPPERSCODE" runat="server"></asp:TextBox>
    <!-- 荷主(名) -->
    <asp:TextBox ID="WF_SEL_SHIPPERSNAME" runat="server"></asp:TextBox>
    <!-- 荷受人(コード) -->
    <asp:TextBox ID="WF_SEL_CONSIGNEECODE" runat="server"></asp:TextBox>
    <!-- 荷受人(名) -->
    <asp:TextBox ID="WF_SEL_CONSIGNEENAME" runat="server"></asp:TextBox>
    <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_BASECODE" runat="server"></asp:TextBox>
    <!-- 基地名 -->
    <asp:TextBox ID="WF_SEL_BASENAME" runat="server"></asp:TextBox>
    <!-- 受注№ -->
    <asp:TextBox ID="WF_SEL_ORDERNUMBER" runat="server"></asp:TextBox>

    <!-- ■共通 -->
    <!-- 作成フラグ -->
    <asp:TextBox ID="WF_SEL_CREATEFLG" runat="server"></asp:TextBox>

    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>

    <!-- 明細画面(タブ１)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB1TBL" runat="server"></asp:TextBox>
    <!-- 明細画面(タブ２)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB2TBL" runat="server"></asp:TextBox>

    <!-- MAPID退避(受注明細画面への遷移制御のため) -->
    <asp:TextBox ID="WF_SEL_MAPIDBACKUP" runat="server"></asp:TextBox>

    <!-- 回送訂正フラグ -->
    <asp:TextBox ID="WF_SEL_CORRECTIONFLG" runat="server"></asp:TextBox>
    <!-- 託送指示フラグ -->
    <asp:TextBox ID="WF_SEL_DELIVERYFLG" runat="server"></asp:TextBox>
    <!-- タンク車状況コード -->
    <asp:TextBox ID="WF_SEL_TANKSITUATION" runat="server"></asp:TextBox>

</div>

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
    <!-- 目的(コード) -->
    <asp:TextBox ID="WF_SEL_OBJECTIVECODE" runat="server"></asp:TextBox>
    <!-- 目的(名) -->
    <asp:TextBox ID="WF_SEL_OBJECTIVENAME" runat="server"></asp:TextBox>
    <!-- 着駅(コード(検索退避用)) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATIONMAP" runat="server"></asp:TextBox>
    <!-- 着駅(コード) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATION" runat="server"></asp:TextBox>
    <!-- 着駅(名) -->
    <asp:TextBox ID="WF_SEL_ARRIVALSTATIONNM" runat="server"></asp:TextBox>

    <!-- ■共通 -->
    <!-- 作成フラグ -->
    <asp:TextBox ID="WF_SEL_CREATEFLG" runat="server"></asp:TextBox>

    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>

    <!-- 明細画面(タブ１)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB1TBL" runat="server"></asp:TextBox>
    <!-- 明細画面(タブ２)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB2TBL" runat="server"></asp:TextBox>

    <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_BASECODE" runat="server"></asp:TextBox>
    <!-- 基地名 -->
    <asp:TextBox ID="WF_SEL_BASENAME" runat="server"></asp:TextBox>

    <!-- MAPID退避(受注明細画面への遷移制御のため) -->
    <asp:TextBox ID="WF_SEL_MAPIDBACKUP" runat="server"></asp:TextBox>

</div>

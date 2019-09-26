<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRT00007WRKINC.ascx.vb" Inherits="OFFICE.GRT00007WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_T7SEL_CAMPCODE" runat="server"></asp:TextBox>             <!-- 会社コード -->
    <asp:TextBox ID="WF_T7SEL_TAISHOYM" runat="server"></asp:TextBox>             <!-- 対象年月 -->
    <asp:TextBox ID="WF_T7SEL_HORG" runat="server"></asp:TextBox>                 <!-- 運用部署 -->
    <asp:TextBox ID="WF_T7SEL_STAFFKBN" runat="server"></asp:TextBox>             <!-- 職種区分 -->
    <asp:TextBox ID="WF_T7SEL_STAFFCODE" runat="server"></asp:TextBox>            <!-- 従業員コード -->
    <asp:TextBox ID="WF_T7SEL_STAFFNAME" runat="server"></asp:TextBox>            <!-- 従業員名称 -->

    <asp:TextBox ID="WF_SEL_VIEWID" runat="server"></asp:TextBox>               <!-- 画面ID　 -->
    <asp:TextBox ID="WF_SEL_VIEWID_DTL" runat="server"></asp:TextBox>           <!-- 画面ID（個別）　 -->
    <asp:TextBox ID="WF_SEL_MAPvariant" runat="server"></asp:TextBox>           <!-- MAP変数 -->
    <asp:TextBox ID="WF_SEL_MAPpermitcode" runat="server"></asp:TextBox>        <!-- MAP権限 -->

    <asp:TextBox ID="WF_T7SEL_BUTTON" runat="server"></asp:TextBox>               <!-- 押下ボタン　 -->
    <asp:TextBox ID="WF_T7SEL_LIMITFLG" runat="server"></asp:TextBox>             <!-- 締状態　 -->
    <asp:TextBox ID="WF_T7SEL_PERMITCODE" runat="server"></asp:TextBox>           <!-- 権限　 -->
    <asp:TextBox ID="WF_T7SEL_SRVSTAT" runat="server"></asp:TextBox>              <!-- サーバー状態　 -->
    <asp:TextBox ID="WF_T7SEL_XMLsaveTmp" runat="server"></asp:TextBox>           <!-- 画面一覧保存パス　 -->
    <asp:TextBox ID="WF_T7SEL_XMLsavePARM" runat="server"></asp:TextBox>          <!-- 抽出条件保存パス　 -->
    <!--  自画面でのみ必要な情報（次画面（個別画面）で保持して戻してもらう必要あり）  -->
    <asp:TextBox ID="WF_T7I_XMLsaveF" runat="server"></asp:TextBox>               <!-- 画面一覧保存パス　 -->
    <asp:TextBox ID="WF_T7I_GridPosition" runat="server"></asp:TextBox>           <!-- GridView位置　 -->
    <asp:TextBox ID="WF_T7I_Head_STAFFCODE" runat="server"></asp:TextBox>         <!-- ヘッダの従業員　 -->
    <asp:TextBox ID="WF_T7I_Head_WORKDATE" runat="server"></asp:TextBox>          <!-- ヘッダの日付　 -->
    <asp:TextBox ID="WF_T7I_Head_NIPPO_FROM" runat="server"></asp:TextBox>        <!-- ヘッダの日報日FROM　 -->
    <asp:TextBox ID="WF_T7I_Head_NIPPO_TO" runat="server"></asp:TextBox>          <!-- ヘッダの日報日TO　 -->
    <!--  次画面（個別画面）用  -->
    <asp:TextBox ID="WF_T7KIN_LINECNT" runat="server"></asp:TextBox>              <!-- 選択番号　 -->
    <asp:TextBox ID="WF_T7KIN_WORKDATE" runat="server"></asp:TextBox>             <!-- 明細の日付　 -->
    <asp:TextBox ID="WF_T7KIN_STAFFCODE" runat="server"></asp:TextBox>            <!-- 明細の従業員　 -->
    <asp:TextBox ID="WF_T7KIN_RECODEKBN" runat="server"></asp:TextBox>            <!-- 明細のレコード区分　 -->
    <asp:TextBox ID="WF_T7KIN_XMLsaveF" runat="server"></asp:TextBox>             <!-- 画面一覧保存パス　 -->
    <asp:TextBox ID="WF_T7KIN_XMLsaveF2" runat="server"></asp:TextBox>            <!-- モデル保存パス　 -->

    <!--  日報画面で必要な情報（次画面で保持して戻してもらう必要あり）  -->
    <asp:TextBox ID="WF_T5I_XMLsaveF" runat="server"></asp:TextBox>             <!-- 画面一覧保存パス　 -->
    <asp:TextBox ID="WF_T5I_XMLsaveF9" runat="server"></asp:TextBox>            <!-- 画面一覧保存パス　 -->
    <asp:TextBox ID="WF_T5I_LINECNT" runat="server"></asp:TextBox>              <!-- 選択番号　 -->
    <asp:TextBox ID="WF_T5I_GridPosition" runat="server"></asp:TextBox>         <!-- GridView位置　 -->

    <%--<asp:TextBox ID="WF_T5_CAMPCODE" runat="server"></asp:TextBox>--%>              <!-- 会社　 -->
    <asp:TextBox ID="WF_T5_STYMD" runat="server"></asp:TextBox>                 <!-- 出庫年月日開始　 -->
    <asp:TextBox ID="WF_T5_ENDYMD" runat="server"></asp:TextBox>                <!-- 出庫年月日終了　 -->
    <asp:TextBox ID="WF_T5_UORG" runat="server"></asp:TextBox>                  <!-- 運用部署　 -->
    <asp:TextBox ID="WF_T5_STAFFCODE" runat="server"></asp:TextBox>             <!-- 従業員　 -->
    <asp:TextBox ID="WF_T5_STAFFNAME" runat="server"></asp:TextBox>             <!-- 従業員名　 -->
    <asp:TextBox ID="WF_T5_VIEWID" runat="server"></asp:TextBox>                <!-- 画面ID　 -->
    <asp:TextBox ID="WF_T5_VIEWID_DTL" runat="server"></asp:TextBox>            <!-- 画面ID（個別）　 -->
    <asp:TextBox ID="WF_T5_MAPvariant" runat="server"></asp:TextBox>            <!-- MAP変数 -->
    <asp:TextBox ID="WF_T5_MAPpermitcode" runat="server"></asp:TextBox>         <!-- MAP権限 -->
    <asp:TextBox ID="WF_T5_BUTTON" runat="server"></asp:TextBox>                <!-- 押下ボタン　 -->
    <asp:TextBox ID="WF_T5_XMLsaveTmp" runat="server"></asp:TextBox>            <!-- 画面一覧保存パス　 -->
    <asp:TextBox ID="WF_T5_XMLsaveTmp9" runat="server"></asp:TextBox>           <!-- 画面一覧保存パス　 -->
    <asp:TextBox ID="WF_T5_XMLsavePARM" runat="server"></asp:TextBox>           <!-- 抽出条件保存パス　 -->
    <asp:TextBox ID="WF_T5_XMLsaveF2" runat="server"></asp:TextBox>             <!-- 画面一覧保存パス　 -->
    <asp:TextBox ID="WF_T5_YMD" runat="server"></asp:TextBox>                   <!-- ヘッダの日付　 -->
    <asp:TextBox ID="WF_T5_FROMMAPID" runat="server"></asp:TextBox>             <!-- 呼出元MAPID　 -->
    <asp:TextBox ID="WF_T5_FROMMAPVARIANT" runat="server"></asp:TextBox>        <!-- 呼出元MAPVARI　 -->

    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>             <!-- 会社　 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>                <!-- 出庫年月日開始　 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>               <!-- 出庫年月日終了　 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>                 <!-- 運用部署　 -->
    <asp:TextBox ID="WF_SEL_XMLsaveF" runat="server"></asp:TextBox>             <!-- 画面一覧保存パス　 -->
    <asp:TextBox ID="WF_SEL_XMLsaveF9" runat="server"></asp:TextBox>            <!-- 画面一覧保存パス　 -->

</div>

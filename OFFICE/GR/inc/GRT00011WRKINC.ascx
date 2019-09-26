<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRT00011WRKINC.ascx.vb" Inherits="OFFICE.GRT00011WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
            <!--  画面（条件選択）  -->
            <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>           <!-- 会社　 -->
            <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>              <!-- 出庫年月日開始　 -->
            <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>             <!-- 出庫年月日終了　 -->
            <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>               <!-- 運用部署　 -->
            <asp:TextBox ID="WF_SEL_STAFFCODE" runat="server"></asp:TextBox>          <!-- 従業員　 -->
            <asp:TextBox ID="WF_SEL_STAFFNAME" runat="server"></asp:TextBox>          <!-- 従業員名　 -->
            <asp:TextBox ID="WF_SEL_VIEWID" runat="server"></asp:TextBox>             <!-- 画面ID　 -->
            <asp:TextBox ID="WF_SEL_VIEWID_DTL" runat="server"></asp:TextBox>         <!-- 画面ID（個別）　 -->
            <asp:TextBox ID="WF_SEL_MAPvariant" runat="server"></asp:TextBox>         <!-- MAP変数 -->
            <asp:TextBox ID="WF_SEL_MAPpermitcode" runat="server"></asp:TextBox>      <!-- MAP権限 -->
            <asp:TextBox ID="WF_SEL_PERMIT_ORG" runat="server"></asp:TextBox>         <!-- 更新権限部署 -->
            <!--  自画面でのみ必要な情報（次画面（個別画面）へ連携し、そのまま戻す）  -->
            <asp:TextBox ID="WF_T5I_LINECNT" runat="server"></asp:TextBox>              <!-- 選択番号　 -->
            <asp:TextBox ID="WF_T5I_GridPosition" runat="server"></asp:TextBox>         <!-- GridView位置　 -->
            <asp:TextBox ID="WF_T5I_YMD" runat="server"></asp:TextBox>                   <!-- ヘッダの日付　 -->
            <asp:TextBox ID="WF_T5I_STAFFCODE" runat="server"></asp:TextBox>             <!-- 従業員　 -->

            <!--  自画面でのみ必要な情報（次画面（個別画面）で保持して戻してもらう必要あり）  -->
            <asp:TextBox ID="WF_T5_YMD" runat="server"></asp:TextBox>                   <!-- ヘッダの日付　 -->
            <asp:TextBox ID="WF_T5_STAFFCODE" runat="server"></asp:TextBox>             <!-- 従業員　 -->
            <asp:TextBox ID="WF_T5_STAFFNAME" runat="server"></asp:TextBox>             <!-- 従業員名　 -->
            <asp:TextBox ID="WF_T5_ERRMSG" runat="server"></asp:TextBox>                <!-- エラーメッセージ　 -->

            <asp:TextBox ID="WF_T5_FROMMAPID" runat="server"></asp:TextBox>             <!-- 呼出元MAPID　 -->
            <asp:TextBox ID="WF_T5_FROMMAPVARIANT" runat="server"></asp:TextBox>        <!-- 呼出元MAPVARI　 -->

            <asp:TextBox ID="WF_SEL_XMLsaveF" runat="server"></asp:TextBox>             <!-- 画面一覧保存パス　 -->
            <asp:TextBox ID="WF_SEL_XMLsaveF2" runat="server"></asp:TextBox>            <!-- 抽出条件保存パス　 -->
            <asp:TextBox ID="WF_SEL_XMLsaveF9" runat="server"></asp:TextBox>            <!-- 画面一覧保存パス　 -->
        </div>
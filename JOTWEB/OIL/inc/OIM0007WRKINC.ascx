<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0007WRKINC.ascx.vb" Inherits="JOTWEB.OIM0007WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_OFFICECODE" runat="server"></asp:TextBox>               <!-- 管轄受注営業所 -->
    <asp:TextBox ID="WF_SEL_TRAINNO" runat="server"></asp:TextBox>                  <!-- 本線列車番号 -->
    <asp:TextBox ID="WF_SEL_TSUMI" runat="server"></asp:TextBox>                    <!-- 積置フラグ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- Grid情報保存先のファイル名 -->

    <!-- 編集用 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <asp:TextBox ID="WF_SEL_OFFICECODE2" runat="server"></asp:TextBox>              <!-- 管轄受注営業所 -->
    <asp:TextBox ID="WF_SEL_TRAINNO2" runat="server"></asp:TextBox>                 <!-- 本線列車番号 -->
    <asp:TextBox ID="WF_SEL_TRAINNAME" runat="server"></asp:TextBox>                <!-- 本線列車番号名 -->
    <asp:TextBox ID="WF_SEL_TSUMI2" runat="server"></asp:TextBox>                   <!-- 積置フラグ -->
    <asp:TextBox ID="WF_SEL_OTTRAINNO" runat="server"></asp:TextBox>                <!-- OT列車番号 -->
    <asp:TextBox ID="WF_SEL_OTFLG" runat="server"></asp:TextBox>                    <!-- OT発送日報送信フラグ -->
    <asp:TextBox ID="WF_SEL_DEPSTATION" runat="server"></asp:TextBox>               <!-- 発駅コード -->
    <asp:TextBox ID="WF_SEL_ARRSTATION" runat="server"></asp:TextBox>               <!-- 着駅コード -->
    <asp:TextBox ID="WF_SEL_JRTRAINNO1" runat="server"></asp:TextBox>               <!-- JR発列車番号 -->
    <asp:TextBox ID="WF_SEL_MAXTANK1" runat="server"></asp:TextBox>                 <!-- JR発列車牽引車数 -->
    <asp:TextBox ID="WF_SEL_JRTRAINNO2" runat="server"></asp:TextBox>               <!-- JR中継列車番号 -->
    <asp:TextBox ID="WF_SEL_MAXTANK2" runat="server"></asp:TextBox>                 <!-- JR中継列車牽引車数 -->
    <asp:TextBox ID="WF_SEL_JRTRAINNO3" runat="server"></asp:TextBox>               <!-- JR最終列車番号 -->
    <asp:TextBox ID="WF_SEL_MAXTANK3" runat="server"></asp:TextBox>                 <!-- JR最終列車牽引車数 -->
    <asp:TextBox ID="WF_SEL_TRAINCLASS" runat="server"></asp:TextBox>               <!-- 列車区分 -->
    <asp:TextBox ID="WF_SEL_SPEEDCLASS" runat="server"></asp:TextBox>               <!-- 高速列車区分 -->
    <asp:TextBox ID="WF_SEL_SHIPORDERCLASS" runat="server"></asp:TextBox>           <!-- 発送順区分 -->
    <asp:TextBox ID="WF_SEL_DEPDAYS" runat="server"></asp:TextBox>                  <!-- 発日日数 -->
    <asp:TextBox ID="WF_SEL_MARGEDAYS" runat="server"></asp:TextBox>                <!-- 特継日数 -->
    <asp:TextBox ID="WF_SEL_ARRDAYS" runat="server"></asp:TextBox>                  <!-- 積車着日数 -->
    <asp:TextBox ID="WF_SEL_ACCDAYS" runat="server"></asp:TextBox>                  <!-- 受入日数 -->
    <asp:TextBox ID="WF_SEL_EMPARRDAYS" runat="server"></asp:TextBox>               <!-- 空車着日数 -->
    <asp:TextBox ID="WF_SEL_USEDAYS" runat="server"></asp:TextBox>                  <!-- 当日利用日数 -->
    <asp:TextBox ID="WF_SEL_FEEKBN" runat="server"></asp:TextBox>                   <!-- 料金マスタ区分 -->
    <asp:TextBox ID="WF_SEL_RUN" runat="server"></asp:TextBox>                      <!-- 稼働フラグ -->
    <asp:TextBox ID="WF_SEL_ZAIKOSORT" runat="server"></asp:TextBox>                <!-- 在庫管理表表示ソート区分 -->
    <asp:TextBox ID="WF_SEL_BIKOU" runat="server"></asp:TextBox>                    <!-- 備考 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                  <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                 <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>               <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                   <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                  <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>               <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_UPDTIMSTP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->

    <!-- 詳細画面更新 -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>
</div>

<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0019WRKINC.ascx.vb" Inherits="JOTWEB.OIM0019WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_FROMYMD" runat="server"></asp:TextBox>                  <!-- 適用開始年月日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>                   <!-- 適用終了年月日 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTCODE" runat="server"></asp:TextBox>              <!-- 科目コード -->
    <asp:TextBox ID="WF_SEL_SEGMENTCODE" runat="server"></asp:TextBox>              <!-- セグメント -->
    <asp:TextBox ID="WF_SEL_SEGMENTBRANCHCODE" runat="server"></asp:TextBox>        <!-- セグメント枝番 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTTYPE" runat="server"></asp:TextBox>              <!-- 科目区分 -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <asp:TextBox ID="WF_SEL_FROMYMD2" runat="server"></asp:TextBox>                 <!-- 適用開始年月日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD2" runat="server"></asp:TextBox>                  <!-- 適用終了年月日 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTCODE2" runat="server"></asp:TextBox>             <!-- 科目コード -->
    <asp:TextBox ID="WF_SEL_ACCOUNTNAME" runat="server"></asp:TextBox>              <!-- 科目名 -->
    <asp:TextBox ID="WF_SEL_SEGMENTCODE2" runat="server"></asp:TextBox>             <!-- セグメント -->
    <asp:TextBox ID="WF_SEL_SEGMENTNAME" runat="server"></asp:TextBox>              <!-- セグメント名 -->
    <asp:TextBox ID="WF_SEL_SEGMENTBRANCHCODE2" runat="server"></asp:TextBox>       <!-- セグメント枝番 -->
    <asp:TextBox ID="WF_SEL_SEGMENTBRANCHNAME" runat="server"></asp:TextBox>        <!-- セグメント枝番名 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTTYPE2" runat="server"></asp:TextBox>             <!-- 科目区分 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTTYPENAME" runat="server"></asp:TextBox>          <!-- 科目区分名 -->
    <asp:TextBox ID="WF_SEL_TAXTYPE" runat="server"></asp:TextBox>                  <!-- 税区分 -->

    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                  <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                 <!-- 登録ユーザーＩＤ -->
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
<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0017WRKINC.ascx.vb" Inherits="JOTWEB.OIM0017WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_OFFICECODE" runat="server"></asp:TextBox>               <!-- 管轄受注営業所 -->
    <asp:TextBox ID="WF_SEL_TRAINNO" runat="server"></asp:TextBox>                  <!-- JOT列車番号 -->
    <asp:TextBox ID="WF_SEL_WORKINGDATE" runat="server"></asp:TextBox>              <!-- 運行日 -->
    <!--<asp:TextBox ID="WF_SEL_TSUMI" runat="server"></asp:TextBox>-->                    <!-- 積置フラグ -->
    <!--<asp:TextBox ID="WF_SEL_DEPSTATION" runat="server"></asp:TextBox>-->               <!-- 発駅コード -->
    <!--<asp:TextBox ID="WF_SEL_ARRSTATION" runat="server"></asp:TextBox>-->               <!-- 着駅コード -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- Grid情報保存先のファイル名 -->

    <!-- 編集用 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <asp:TextBox ID="WF_SEL_OFFICECODE2" runat="server"></asp:TextBox>              <!-- 管轄受注営業所 -->
    <asp:TextBox ID="WF_SEL_TRAINNO2" runat="server"></asp:TextBox>                 <!-- JOT列車番号 -->
    <asp:TextBox ID="WF_SEL_TRAINNAME" runat="server"></asp:TextBox>                <!-- 列車名 -->
    <asp:TextBox ID="WF_SEL_WORKINGDATE2" runat="server"></asp:TextBox>             <!-- 運行日 -->
    <asp:TextBox ID="WF_SEL_TSUMI2" runat="server"></asp:TextBox>                   <!-- 積置フラグ -->
    <asp:TextBox ID="WF_SEL_DEPSTATION2" runat="server"></asp:TextBox>              <!-- 発駅コード -->
    <asp:TextBox ID="WF_SEL_ARRSTATION2" runat="server"></asp:TextBox>              <!-- 着駅コード -->
    <asp:TextBox ID="WF_SEL_TRAINCLASS" runat="server"></asp:TextBox>               <!-- 列車区分 -->
    <asp:TextBox ID="WF_SEL_RUN" runat="server"></asp:TextBox>                      <!-- 稼働フラグ -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                  <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                 <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>               <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                   <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                  <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>               <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_UPDTIMSTP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->

</div>

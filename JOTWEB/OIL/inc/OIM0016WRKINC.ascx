<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0016WRKINC.ascx.vb" Inherits="JOTWEB.OIM0016WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_OFFICECODE" runat="server"></asp:TextBox>               <!-- 管轄受注営業所 -->
    <asp:TextBox ID="WF_SEL_IOKBN" runat="server"></asp:TextBox>                    <!-- 入線出線区分 -->
    <asp:TextBox ID="WF_SEL_LINE" runat="server"></asp:TextBox>                     <!-- 回線 -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- Grid情報保存先のファイル名 -->

    <!-- 編集用 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <asp:TextBox ID="WF_SEL_OFFICECODE2" runat="server"></asp:TextBox>              <!-- 管轄受注営業所 -->
    <asp:TextBox ID="WF_SEL_IOKBN2" runat="server"></asp:TextBox>                   <!-- 入線出線区分 -->
    <asp:TextBox ID="WF_SEL_TRAINNO" runat="server"></asp:TextBox>                  <!-- 入線出線列車番号 -->
    <asp:TextBox ID="WF_SEL_TRAINNAME" runat="server"></asp:TextBox>                <!-- 入線出線列車名 -->
    <asp:TextBox ID="WF_SEL_DEPSTATION" runat="server"></asp:TextBox>               <!-- 発駅コード -->
    <asp:TextBox ID="WF_SEL_ARRSTATION" runat="server"></asp:TextBox>               <!-- 着駅コード -->
    <asp:TextBox ID="WF_SEL_PLANTCODE" runat="server"></asp:TextBox>                <!-- プラントコード -->
    <asp:TextBox ID="WF_SEL_LINE2" runat="server"></asp:TextBox>                    <!-- 回線 -->

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

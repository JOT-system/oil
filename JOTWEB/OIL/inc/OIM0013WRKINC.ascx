<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0013WRKINC.ascx.vb" Inherits="JOTWEB.OIM0013WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_PLANTCODE" runat="server"></asp:TextBox>                <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_LOADINGPOINT" runat="server"></asp:TextBox>             <!-- 積込ポイント -->
    <asp:TextBox ID="WF_SEL_OILCODE" runat="server"></asp:TextBox>                  <!-- 油種コード -->
    <asp:TextBox ID="WF_SEL_SEGMENTOILCODE" runat="server"></asp:TextBox>           <!-- 油種細分コード -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_PLANTCODE2" runat="server"></asp:TextBox>               <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_MAXLINECNT" runat="server"></asp:TextBox>               <!-- 最大回線数 -->
    <asp:TextBox ID="WF_SEL_LOADINGPOINT2" runat="server"></asp:TextBox>            <!-- 積込ポイント -->
    <asp:TextBox ID="WF_SEL_OILCODE2" runat="server"></asp:TextBox>                 <!-- 油種コード -->
    <asp:TextBox ID="WF_SEL_SEGMENTOILCODE2" runat="server"></asp:TextBox>          <!-- 油種細分コード -->
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

</div>

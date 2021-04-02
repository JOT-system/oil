<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0014WRKINC.ascx.vb" Inherits="JOTWEB.OIM0014WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_PLANTCODE" runat="server"></asp:TextBox>                <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_BIGOILCODE" runat="server"></asp:TextBox>               <!-- 油種大分類コード -->
    <asp:TextBox ID="WF_SEL_CHECKOILCODE" runat="server"></asp:TextBox>             <!-- 積込チェック用油種コード -->
    <asp:TextBox ID="WF_SEL_TANKCOUNT" runat="server"></asp:TextBox>                <!-- 車数 -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_PLANTCODE2" runat="server"></asp:TextBox>               <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_BIGOILCODE2" runat="server"></asp:TextBox>              <!-- 油種大分類コード -->
    <asp:TextBox ID="WF_SEL_CHECKOILCODE2" runat="server"></asp:TextBox>            <!-- 積込チェック用油種コード -->
    <asp:TextBox ID="WF_SEL_TANKCOUNT2" runat="server"></asp:TextBox>               <!-- 車数 -->
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

<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0024WRKINC.ascx.vb" Inherits="JOTWEB.OIM0024WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_OFFICECODE" runat="server"></asp:TextBox>               <!-- 管轄営業所コード -->
    <asp:TextBox ID="WF_SEL_OILCODE" runat="server"></asp:TextBox>                  <!-- 油種コード -->
    <asp:TextBox ID="WF_SEL_SEGMENTOILCODE" runat="server"></asp:TextBox>           <!-- 油種細分コード -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <asp:TextBox ID="WF_SEL_OFFICECODE2" runat="server"></asp:TextBox>              <!-- 管轄営業所コード -->
    <asp:TextBox ID="WF_SEL_OILCODE2" runat="server"></asp:TextBox>                 <!-- 油種コード -->
    <asp:TextBox ID="WF_SEL_SEGMENTOILCODE2" runat="server"></asp:TextBox>          <!-- 油種細分コード -->
    <asp:TextBox ID="WF_SEL_PRIORITYNO" runat="server"></asp:TextBox>               <!-- 優先順 -->
    <asp:TextBox ID="WF_SEL_STARTPOINT" runat="server"></asp:TextBox>               <!-- 開始位置 -->

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

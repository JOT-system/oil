<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRTA0006WRKINC.ascx.vb" Inherits="OFFICE.GRTA0006WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
            <!-- マルチウィンドウ　自画面情報 -->
            <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>        <!-- 会社　 -->
            <asp:TextBox ID="WF_SEL_TAISHOYM" runat="server"></asp:TextBox>        <!-- 対象年月　 -->
            <asp:TextBox ID="WF_SEL_HORG" runat="server"></asp:TextBox>            <!-- 配属部署　 -->
            <asp:TextBox ID="WF_SEL_STAFFKBN" runat="server"></asp:TextBox>        <!-- 職務区分　 -->
            <asp:TextBox ID="WF_SEL_STAFFCODE" runat="server"></asp:TextBox>       <!-- 従業員　 -->
            <asp:TextBox ID="WF_SEL_STAFFNAME" runat="server"></asp:TextBox>       <!-- 従業員名　 -->
            <asp:TextBox ID="WF_SEL_ALL_ATTEND" runat="server"></asp:TextBox>      <!-- 勤怠ALLフラグ　 -->
        </div>
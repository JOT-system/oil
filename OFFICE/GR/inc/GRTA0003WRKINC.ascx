<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRTA0003WRKINC.ascx.vb" Inherits="OFFICE.GRTA0003WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
                <!-- 　2018/11/13　マルチウィンドウ　自画面情報 -->
                <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>        <!-- 会社　 -->
                <asp:TextBox ID="WF_SEL_TAISHOYM" runat="server"></asp:TextBox>        <!-- 対象年月　 -->
                <asp:TextBox ID="WF_SEL_HORG" runat="server"></asp:TextBox>            <!-- 配属部署　 -->
                <asp:TextBox ID="WF_IsHideDetailBox" runat="server"></asp:TextBox>       <!-- 画面一覧保存パス -->
        </div>
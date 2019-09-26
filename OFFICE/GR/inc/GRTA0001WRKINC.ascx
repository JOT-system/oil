<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRTA0001WRKINC.ascx.vb" Inherits="OFFICE.GRTA0001WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
                <!-- 　マルチウィンドウ　自画面情報 -->
                <asp:TextBox ID="WF_SEL_DRIVERS" runat="server"></asp:TextBox>         <!-- ZIPボタン用　 -->
                <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>        <!-- 会社　 -->
                <asp:TextBox ID="WF_SEL_SHUKODATEF" runat="server"></asp:TextBox>      <!-- 出庫日　 -->
                <asp:TextBox ID="WF_SEL_SHIPORG" runat="server"></asp:TextBox>         <!-- 出荷部署　 -->
                <asp:TextBox ID="WF_SEL_SHIPORGNAME" runat="server"></asp:TextBox>     <!-- 出荷部署名　 -->
                <asp:TextBox ID="WF_SEL_FUNCSEL" runat="server"></asp:TextBox>         <!-- 機能選択　 -->
                <asp:TextBox ID="WF_IsHideDetailBox" runat="server"></asp:TextBox>       <!-- 画面一覧保存パス -->
        </div>
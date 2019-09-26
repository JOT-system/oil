<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRT00006WRKINC.ascx.vb" Inherits="OFFICE.GRT00006WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
            <!-- 　2018/9/10　マルチウィンドウ　自画面情報 -->
            <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server" ></asp:TextBox>              <!-- 会社コード　 -->
            <asp:TextBox ID="WF_SEL_SHUKODATEF" runat="server"></asp:TextBox>             <!-- 出庫日FROM　 -->
            <asp:TextBox ID="WF_SEL_SHUKODATET" runat="server"></asp:TextBox>             <!-- 出庫日TO　 -->
            <asp:TextBox ID="WF_SEL_SHIPORG" runat="server"></asp:TextBox>                <!-- 出荷部署　 -->
        </div>
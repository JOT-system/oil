<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRT00004WRKINC.ascx.vb" Inherits="OFFICE.GRT00004WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
            <!-- 　2018/9/10　マルチウィンドウ　自画面情報 -->
            <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server" ></asp:TextBox>              <!-- 会社コード　 -->
            <asp:TextBox ID="WF_SEL_SHUKODATEF" runat="server"></asp:TextBox>             <!-- 出庫日FROM　 -->
            <asp:TextBox ID="WF_SEL_SHUKODATET" runat="server"></asp:TextBox>             <!-- 出庫日TO　 -->
            <asp:TextBox ID="WF_SEL_SHUKADATEF" runat="server"></asp:TextBox>             <!-- 出荷日FROM　 -->
            <asp:TextBox ID="WF_SEL_SHUKADATET" runat="server"></asp:TextBox>             <!-- 出荷日TO　 -->
            <asp:TextBox ID="WF_SEL_TODOKEDATEF" runat="server"></asp:TextBox>            <!-- 届日FROM　 -->
            <asp:TextBox ID="WF_SEL_TODOKEDATET" runat="server"></asp:TextBox>            <!-- 届日TO　 -->
            <asp:TextBox ID="WF_SEL_ORDERORG" runat="server"></asp:TextBox>               <!-- 受注部署　 -->
            <asp:TextBox ID="WF_SEL_SHIPORG" runat="server"></asp:TextBox>                <!-- 出荷部署　 -->
            <asp:TextBox ID="WF_SEL_OILTYPE" runat="server"></asp:TextBox>                <!-- 油種　 -->
            <asp:TextBox ID="WF_SEL_KOUEILOADFILE" runat="server"></asp:TextBox>          <!-- 光英読込中ファイル　 -->

            <asp:TextBox ID="WF_SEL_RESTART" runat="server"></asp:TextBox>                <!-- 再開　 -->
            <asp:TextBox ID="WF_SEL_XMLsavePARM" runat="server"></asp:TextBox>            <!-- 抽出条件保存パス　 -->
            <asp:TextBox ID="WF_SEL_XMLsaveTmp" runat="server"></asp:TextBox>             <!-- 画面一覧保存パス　 -->

        </div>
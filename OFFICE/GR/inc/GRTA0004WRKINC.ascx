<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRTA0004WRKINC.ascx.vb" Inherits="OFFICE.GRTA0004WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
                <!-- 　マルチウィンドウ　自画面情報 -->
                <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>             <!-- 会社　 -->
                <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>                <!-- 開始年月日　 -->
                <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>               <!-- 終了年月日　 -->
                <asp:TextBox ID="WF_SEL_FIELDSEL" runat="server"></asp:TextBox>             <!-- 日付選択　 -->
                <asp:TextBox ID="WF_SEL_FUNC" runat="server"></asp:TextBox>                 <!-- 機能（副乗務員含む）　 -->
                <asp:TextBox ID="WF_SEL_FUNCBRK" runat="server"></asp:TextBox>              <!-- 機能（休憩含む）　 -->
                <asp:TextBox ID="WF_SEL_XMLsaveF" runat="server"></asp:TextBox>             <!-- 一覧保存ファイル -->
                <asp:TextBox ID="WF_SEL_XMLsaveF2" runat="server"></asp:TextBox>            <!-- 一覧保存ファイル -->

        </div>
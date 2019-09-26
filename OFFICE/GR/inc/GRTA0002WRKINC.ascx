<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRTA0002WRKINC.ascx.vb" Inherits="OFFICE.GRTA0002WRKINC" %>
        <!-- Work レイアウト -->
        <div hidden="hidden">
                <!-- 　マルチウィンドウ　自画面情報 -->
                <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>             <!-- 会社コード　 -->
                <asp:TextBox ID="WF_SEL_TAISHOYM" runat="server"></asp:TextBox>             <!-- 申請年月　 -->
                <asp:TextBox ID="WF_SEL_HORG" runat="server"></asp:TextBox>                 <!-- 配属部署　 -->
                <asp:TextBox ID="WF_SEL_STAFFKBN" runat="server"></asp:TextBox>             <!-- 職務区分　 -->
                <asp:TextBox ID="WF_SEL_STAFFCODE" runat="server"></asp:TextBox>            <!-- 従業員コード　 -->
                <asp:TextBox ID="WF_SEL_STAFFNAME" runat="server"></asp:TextBox>            <!-- 従業員名称　 -->
                <asp:TextBox ID="WF_SEL_XMLsaveF" runat="server"></asp:TextBox>             <!-- 一覧保存ファイル -->
                <asp:TextBox ID="WF_IsHideDetailBox" runat="server"></asp:TextBox>          <!-- 画面一覧保存パス -->
                <asp:TextBox ID="WF_DTL_XMLsaveF" runat="server"></asp:TextBox>             <!-- 画面一覧保存パス　 -->

        </div>
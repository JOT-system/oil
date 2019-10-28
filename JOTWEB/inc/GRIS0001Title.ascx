<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0001Title.ascx.vb" Inherits="JOTWEB.GRIS0001Title" %>

        <!-- 全体レイアウト　titlebox -->
        <div class="titlebox" id="titlebox" runat="server">
            <asp:Label ID="WF_TITLEID" class="WF_TITLEID" runat="server" Text=""></asp:Label>
            <asp:Label ID="WF_TITLETEXT" class="WF_TITLETEXT" runat="server" Text=""></asp:Label>
            <asp:Label ID="WF_TITLECAMP" class="WF_TITLECAMP" runat="server" Text=""></asp:Label>
            <asp:Label ID="WF_TITLEDATE" class="WF_TITLEDATE" runat="server" Text=""></asp:Label>
            <img class="WF_rightboxSW" src="<%=ResolveUrl("~/img/透明R.png")%>" style="z-index:30" ondblclick="r_boxDisplay();" alt=""/>
        </div>
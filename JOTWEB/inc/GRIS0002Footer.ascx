<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0002Footer.ascx.vb" Inherits="JOTWEB.GRIS0002Footer" %>

        <!-- 全体レイアウト　footerbox -->
        <div class="footerbox" id="footerbox">
            <asp:Label ID="WF_MESSAGE" runat="server" Text="" CssClass="WF_MESSAGE" ondblclick="r_boxDisplay();"></asp:Label>
            <a style="position:fixed;right:0.2em;">
                <asp:Image ID="WF_HELPIMG" runat ="server" ImageUrl ="~/img/ヘルプ.jpg" style="z-index:30" ondblclick="HelpDisplay();" alt=""/>
            </a>
        </div>
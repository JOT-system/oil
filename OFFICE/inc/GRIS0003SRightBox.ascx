<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0003SRightBox.ascx.vb" Inherits="OFFICE.GRIS0003SRightBox" %>

        <!-- 全体レイアウト　rightbox -->
        <div class="rightbox" id="RF_RIGHTBOX">
            <span style="position:relative;left:1em;top:1em;">
                <a >メモ</a>
            </span>
            <br />

            <span id="RF_MEMOTITLE" style="position:relative;left:1em;top:1.2em;" onchange="MEMOChange()">
                <asp:TextBox ID="RF_MEMO" runat="server" Width="28.4em" Height="16.9em" CssClass="WF_MEMO" TextMode="MultiLine"/>
            </span>
            <br />

            <span id="RF_VIEWTITLE" style="position:relative;left:1em;top:2em;">
                <a ><asp:Label ID="RF_RIGHT_VIEW_NAME" runat="server" Text="" CssClass="WF_RIGHT_VIEW_NAME"></asp:Label></a>
            </span>
            <br />

            <span id="RF_VIEWLIST" style="position:relative;left:1em;top:2.3em;">
                <asp:ListBox ID="RF_VIEW" runat="server" Width="28.4em" Height="15em" style="border: 2px solid blue;background-color: rgb(220,230,240);" ></asp:ListBox>
            </span>
            <br />
            
            <span id="RF_VIEWDTLTITLE" class="RF_DTLDISP" style="position:relative;left:1em;top:3em;">
                <a ><asp:Label ID="RF_RIGHT_VIEW_DTL_NAME" runat="server" Text="" CssClass="WF_RIGHT_VIEW_NAME"></asp:Label></a>
            </span>
            <br />

            <span id="RF_VIEWDTLLIST" class="RF_DTLDISP" style="position:relative;left:1em;right:1em;top:3.3em;">
                <asp:ListBox ID="RF_VIEW_DTL" runat="server" Width="28.4em" Height="10em" style="border: 2px solid blue;background-color: rgb(220,230,240);" ></asp:ListBox>
            </span><br />

            <div id="RF_HIDDEN_LIS">
                <asp:HiddenField ID="RF_COMPCODE" runat="server" value="" />
                <asp:HiddenField ID="RF_MAPID" runat="server"  value="" />
                <asp:HiddenField ID="RF_MAPID_DTL" runat="server"  value="" />
                <asp:HiddenField ID="RF_MAPIDS" runat="server"  value="" />
                <asp:HiddenField ID="RF_PROFID" runat="server"  value="" />
                <asp:HiddenField ID="RF_MAPVARI" runat="server"  value="" />
            </div>
        </div>
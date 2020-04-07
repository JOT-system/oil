<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRC0001TILESELECTORWRKINC.ascx.vb" Inherits="JOTWEB.GRC0001TILESELECTORWRKINC"  %>
<div class="grc0001Wrapper">
    <asp:CheckBoxList ID="chklGrc0001SelectionBox" runat="server" ClientIDMode="Predictable" RepeatLayout="UnorderedList">
    </asp:CheckBoxList>
    <div class="grc0001Settings">
        <asp:TextBox ID="txtGrc0001SelectionMode" data-id="SelectionMode" runat="server" Enabled="false"></asp:TextBox>
        <asp:TextBox ID="txtGrc0001ListClass" data-id="ListClass" runat="server" Enabled="false"></asp:TextBox>
        <asp:TextBox ID="txtGrc0001NeedsAfterPostBack" data-id="NeedsAfterPostBack" runat="server" Enabled="false"></asp:TextBox>
    </div>
</div>
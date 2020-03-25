<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0001Title.ascx.vb" Inherits="JOTWEB.GRIS0001Title" %>

        <!-- 全体レイアウト　titlebox -->
        <div class="titlebox" id="titlebox" runat="server">
            <table id="tblTitlebox">
                <tr>
                    <td>
                        <asp:Label ID="WF_TITLEID" class="WF_TITLEID" runat="server" Text=""></asp:Label>
                    </td>
                    <td rowspan="2">
                        <asp:Label ID="WF_TITLETEXT" class="WF_TITLETEXT" runat="server" Text=""></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="WF_TITLECAMP" class="WF_TITLECAMP" runat="server" Text=""></asp:Label>
                    </td>
                    <td rowspan="2">
                        <div id="rightb" ondblclick="r_boxDisplay();">
                            <div id="divShowRightBox"></div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                       <%=If(Parent.Parent.FindControl("contents1").Page.Title = "M00001", "<span id='spnOpenNewTab' onclick='commonOpenNewTab(""" & ResolveUrl(Parent.Parent.FindControl("contents1").Page.Form.Page.AppRelativeVirtualPath) & """);return false;'>新しいタブを開く</span>", "&nbsp;") %>
                        <asp:Label ID="lblCommonHeaderLeftBottom" runat="server" Text=""></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="WF_TITLEDATE" class="WF_TITLEDATE" runat="server" Text=""></asp:Label>
                    </td>
                </tr>
            </table>

<%--                <img id="rightb" class="WF_rightboxSW" src="<%=ResolveUrl("~/img/right.png")%>" style="z-index:30" ondblclick="r_boxDisplay();" alt=""/>--%>
        </div>
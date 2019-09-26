<%@ Page Title="M00000" Language="vb" AutoEventWireup="true" CodeBehind="M00000LOGON.aspx.vb" Inherits="OFFICE.M00000LOGON"  %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<asp:Content ID="M00000H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/css/M00000.css")%>"/>  
    <script type="text/javascript" src="<%=ResolveUrl("~/script/M00000.js")%>"></script>

</asp:Content> 
<asp:Content ID="M00000" ContentPlaceHolderID="contents1" runat="server">
        <!-- LOGON　TOPbox -->
        <div id="logonbox" class="logonbox">
            <div class="Operation" >
                <span>
                    <input type="button" id="OK" value="実行"  style="Width:5em" onclick="ButtonClick('WF_ButtonOK');" />
                </span>
            </div>
            <div id="logonkeybox" class="logonkeybox">
                <p class="LINE_1">
                    <span>
                        <asp:Label ID="UserName" runat="server" Text="ユーザＩＤ" Width="100px" Font-Bold="True" BorderStyle="NotSet"></asp:Label>
                    </span>
                    <span>
                        <asp:TextBox ID="UserID" runat="server" Width="120pt"></asp:TextBox>
                    </span>
                </p>
                <p class="LINE_2">
                <span>
                    <asp:Label ID="PassName" runat="server" Text="パスワード" Width="100px" Font-Bold="True"></asp:Label>
                </span>
                <span>
                    <asp:TextBox ID="PassWord" runat="server" Width="120pt" TextMode="Password"></asp:TextBox>
                </span>
                </p>
            </div>
            <div id="guidancebox" class="guidancebox">
                <span>
                    <asp:Label ID="WF_Guidance" runat="server" Text=""></asp:Label><br />
                </span>
            </div> 
        </div>

        <div hidden="hidden">
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />      <!-- ボタン押下 -->
            <asp:TextBox ID="WF_TERMID" runat="server"></asp:TextBox>               <!-- 端末ID　 -->
            <asp:TextBox ID="WF_TERMCAMP" runat="server"></asp:TextBox>             <!-- 端末会社　 -->

        </div>

</asp:Content> 

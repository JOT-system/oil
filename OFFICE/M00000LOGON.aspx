<%@ Page Title="M00000" Language="vb" AutoEventWireup="true" CodeBehind="M00000LOGON.aspx.vb" Inherits="OFFICE.M00000LOGON"  %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<asp:Content ID="M00000H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/css/M00000.css")%>"/>  
    <script type="text/javascript" src="<%=ResolveUrl("~/script/M00000.js")%>"></script>

</asp:Content> 
<asp:Content ID="M00000" ContentPlaceHolderID="contents1" runat="server">
        <!-- LOGON　TOPbox -->
        <div id="logonbox" class="logonbox" >
            <div id="logonkeybox" class="logonkeybox">
                      <asp:Image ID="WF_LOGO" runat ="server" ImageUrl ="~/logo.png" alt=""/>
  
                <p class="LINE_0">
                    <span>
                        <asp:Label ID="LoginLabel" runat="server" Text=" Login" Width="100px" Font-Bold="True" BorderStyle="NotSet"></asp:Label>
                    </span>
                </p>
                <p class="LINE_1">
                    <span>
                        <asp:TextBox ID="UserID" runat="server" Width="300px" placeholder="UserID"></asp:TextBox>
                    </span>
                </p>
                <p class="LINE_2">
                <span>
                    <asp:TextBox ID="PassWord" runat="server" Width="300px" TextMode="Password" placeholder="Password"></asp:TextBox>
                </span>
                </p>
                <div class="Operation" >
                 <span>
                    <input type="button" id="OK" value="LOGIN"  style="Width:300px" onclick="ButtonClick('WF_ButtonOK');" />
                 </span>
                </div>      
            </div>
            
            <!--
            <div id="guidancebox" class="guidancebox">
                <span>
                    <asp:Label ID="WF_Guidance" runat="server" Text=""></asp:Label><br />
                </span>
            </div>
            -->
            
        </div>

        <div hidden="hidden">
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />      <!-- ボタン押下 -->
            <asp:TextBox ID="WF_TERMID" runat="server"></asp:TextBox>               <!-- 端末ID　 -->
            <asp:TextBox ID="WF_TERMCAMP" runat="server"></asp:TextBox>             <!-- 端末会社　 -->

        </div>

</asp:Content> 

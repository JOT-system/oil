
<%@ Page Title="M00000" Language="vb" AutoEventWireup="true" CodeBehind="M00000LOGON.aspx.vb" Inherits="JOTWEB.M00000LOGON"  %>

<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %> 



<asp:Content ID="M00000H" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/css/M00000.css")%>"/>  

    <script type="text/javascript" src="<%=ResolveUrl("~/script/M00000.js")%>"></script>

    <!--  ここにGAのスクリプト記載予定 -->

</asp:Content> 

<asp:Content ID="M00000" ContentPlaceHolderID="contents1" runat="server">
       <!--  画像　-->        

        <!-- LOGON　TOPbox -->
        <div id="logonbox" class="logonbox" >
            <div id="logonkeybox" class="logonkeybox">
                <div id="Waku" class="Waku">
                    <div id="LogInImage" class="LogInImage">
                        <asp:Image ID="WF_LOGO" runat ="server" ImageUrl ="~/img/logo.png" alt=""/>
                    </div>
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
                        <input type="button" id="OK" value="LOGIN"  style="Width:300px" onclick="ButtonClick('WF_ButtonOK'); " />
                        </span>
                    </div>
                    <p class="LINE_3">
                    </p>
                 </div>   
            </div>
            <!-- ガイダンス表示エリア -->
            <div id="guidanceBoxWrapper" class="guidanceboxWrapper">
                <div id="guidanceBox" class="guidancebox">
                    <div id="guidanceTitle">◆お知らせ</div>
                    <asp:Repeater ID="repGuidance" runat="server">
                        <HeaderTemplate >
                            <ul class="ulGuidance">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <div class="titleLine">
                                    <div class="entryDate"><%# Eval("ENTRYDATE") %></div>
                                    <div class="title"><%# Eval("TITTLE") %></div>
                                </div>
                                <div class="otherLine">
                                    <div class="naiyo"><%# Eval("NAIYOU") %></div>
                                    <div class="attachFile1"><a href="#"><%# Eval("FAILE1") %></a></div>
                                </div>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate>
                            </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <div hidden="hidden">
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />      <!-- ボタン押下 -->
            <asp:TextBox ID="WF_TERMID" runat="server"></asp:TextBox>               <!-- 端末ID　 -->
            <asp:TextBox ID="WF_TERMCAMP" runat="server"></asp:TextBox>             <!-- 端末会社　 -->
        </div>

</asp:Content> 
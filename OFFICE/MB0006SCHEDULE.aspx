<%@ Page Title="MB0006" Language="vb" AutoEventWireup="true" CodeBehind="MB0006SCHEDULE.aspx.vb" Inherits="OFFICE.MB0006SCHEDULE"  %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<asp:Content ID="MB0006H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/css/MB0006.css")%>"/>  
    <script type="text/javascript" src="<%=ResolveUrl("~/script/MB0006.js")%>"></script>

</asp:Content> 
<asp:Content ID="MB0006" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　headerbox -->
        <div  class="headerboxOnly" id="headerbox">
            <div class="Operation" style="margin-left:3em;margin-top:0.5em;">
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:67em;">
                    <asp:Button ID="WF_ButtonEND" runat="server" Text="終了"  Width="5em" />
                </a>
            </div>

            <div style="overflow-y:auto;position:fixed;top:4.8em;left:1em;height:1.8em;width:10.3em;text-align:center;vertical-align:central;color:white;background-color:rgb(22,54,92);font-weight:bold;border: solid black;border-width:1.5px;">
                <!-- ■　照会選択　■ -->
                <a style="position:fixed;top:5.0em;left:1em;width:10.0em;font-size:medium;overflow:hidden;color:white;background-color:rgb(22,54,92);text-align:center;">照会選択</a>
            </div>

            <div class="AREASelect" hidden="hidden" style="overflow-y:auto;position:fixed;top:7.0em;height:18.0em;left:1em;width:10.3em;color:black;background-color: white;border: solid;border-width:1.5px;">
                <!-- ■　エリアセレクター　■ -->
                <asp:Repeater ID="WF_AREAselector" runat="server">
                    <HeaderTemplate> 
                    </HeaderTemplate>

                    <ItemTemplate>
                        <table style="border-width:1px;margin:0.06em 0.6em 0.05em 1.0em;">
                            <tr> 

                                <!-- 非表示項目(左Box処理用・Repeater内行位置)　-->
                                <td hidden="hidden">
                                    <asp:Label ID="WF_SELarea_VALUE" runat="server"></asp:Label>
                                </td>

                                <td>
                                    <asp:Label ID="WF_SELarea_TEXT" runat="server" Text="" Width="7.8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                            </tr> 
                        </table>

                    </ItemTemplate>

                    <FooterTemplate>
                    </FooterTemplate>

                </asp:Repeater>
            </div>

            <div class="DateSelect" style="overflow-y:auto;position:fixed;top:7.0em;bottom:25px;left:1em;width:10.3em;color:black;background-color: white;border: solid;border-width:1.5px;">
                <!-- ■　日付セレクター　■ -->
                <asp:Repeater ID="WF_DATEselector" runat="server">
                    <HeaderTemplate> 
                    </HeaderTemplate>

                    <ItemTemplate>
                        <table style="border-width:1px;margin:0.06em 0.6em 0.05em 1.0em;">
                            <tr> 

                                <!-- 非表示項目(左Box処理用・Repeater内行位置)　-->
                                <td hidden="hidden">
                                    <asp:Label ID="WF_SELdate_VALUE" runat="server"></asp:Label>
                                </td>

                                <td>
                                    <asp:Label ID="WF_SELdate_TEXT" runat="server" Text="" Width="7.8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                            </tr> 
                        </table>

                    </ItemTemplate>

                    <FooterTemplate>
                    </FooterTemplate>

                </asp:Repeater>

            </div>

            <div class="WF_RepeaterSCHDULE" style="position:fixed;top:4.8em;bottom:25px;left:12.7em;right:1.0em;border:solid 1px black;overflow:auto;background-color:white;" >

                <!-- ■■■　列ヘッダー　■■■ -->
                <span id="RepHeaderC" style="position:fixed;top:4.9em;left:24.8em;right:1.8em;height:5.0em;border:solid 1px white;overflow:auto;background-color:white;">
                <asp:Repeater ID="WF_RepeaterHC" runat="server" >

                    <HeaderTemplate>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <!-- ■　ユーザー欄（01～50）　■ -->
                        <table style="height:1.8em;table-layout: fixed;border:solid 1px white;">
                        <tr style="">

                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_01" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_01" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_02" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_02" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_03" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_03" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_04" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_04" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_05" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_05" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_06" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_06" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_07" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_07" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_08" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_08" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_09" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_09" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_10" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_10" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_11" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_11" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_12" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_12" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_13" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_13" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_14" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_14" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_15" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_15" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_16" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_16" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_17" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_17" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_18" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_18" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_19" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_19" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_20" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_20" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_21" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_21" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_22" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_22" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_23" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_23" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_24" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_24" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_25" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_25" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_26" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_26" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_27" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_27" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_28" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_28" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_29" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_29" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_30" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_30" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_31" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_31" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_32" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_32" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_33" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_33" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_34" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_34" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_35" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_35" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_36" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_36" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_37" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_37" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_38" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_38" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_39" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_39" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_40" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_40" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_41" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_41" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_42" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_42" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_43" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_43" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_44" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_44" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_45" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_45" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_46" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_46" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_47" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_47" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_48" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_48" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_49" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_49" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_USER_50" runat="server" hidden="hidden"></asp:TextBox>
                            <asp:TextBox id="WF_Rep_USERNM_50" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="9.7em" Font-Bold="True" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                        </td>

                        </tr>
                        </table>                    
                    </ItemTemplate>

                    <FooterTemplate>
                    </FooterTemplate>
             
                </asp:Repeater>
                </span>

                <!-- ■■■　行ヘッダー　■■■ -->
                <span id="RepHeaderL" style="position:fixed;top:6.7em;bottom:30px;left:13.0em;width:20em;border:solid 1px white;overflow-x:hidden;overflow-y:auto;background-color:white;">
                <asp:Repeater ID="WF_RepeaterHL" runat="server">

                    <HeaderTemplate>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <table style="table-layout:fixed;border:solid 1px white;">
                        <tr style="">

                        <!-- ■　日付　■ -->
                        <td style="height:1.8em;width:5.0em;vertical-align:middle;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox ID="WF_Rep_GDATE" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="5em" Font-Bold="True" CssClass="WF_TEXT_CENTER" TextMode="MultiLine"></asp:TextBox>
                        </td>
                        <td style="height:1.8em;width:6.5em;vertical-align:middle;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox ID="WF_Rep_GDATE_TEXT" runat="server" ReadOnly="true" BorderWidth="1px"  Borderstyle="Solid" BorderColor="white" Width="6em" CssClass="WF_TEXT_LEFT" TextMode="MultiLine"></asp:TextBox>
                        </td>

                        </tr>
                        </table>

                    </ItemTemplate>

                    <FooterTemplate>
                    </FooterTemplate>
             
                </asp:Repeater>
                </span>

                <!-- ■■■　明　細　■■■ -->
                <span id="RepDetail"  onscroll="f_Scroll();" style="position:fixed;top:6.7em;bottom:30px;left:24.8em;right:1.8em;border:solid 1px white;overflow:auto;background-color:white;z-index:10">
                <asp:Repeater ID="WF_Repeater" runat="server">

                    <HeaderTemplate>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <table style="height:1.8em;margin:0px 0px 0px 0px;table-layout: fixed;border:solid 1px white;">
                        <tr style="height:1.8em;">

                        <!-- ■　スケジュール欄（01～50）　■ -->
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_01" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_01_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_02" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_02_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_03" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_03_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_04" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_04_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_05" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_05_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_06" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_06_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_07" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_07_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_08" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_08_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_09" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_09_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_10" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_10_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_11" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_11_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_12" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_12_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_13" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_13_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_14" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_14_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_15" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_15_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_16" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_16_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_17" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_17_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_18" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_18_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_19" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_19_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_20" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_20_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_21" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_21_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_22" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_22_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_23" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_23_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_24" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_24_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_25" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_25_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_26" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_26_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_27" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_27_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_28" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_28_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_29" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_29_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_30" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_30_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_31" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_31_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_32" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_32_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_33" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_33_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_34" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_34_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_35" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_35_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_36" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_36_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_37" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_37_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_38" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_38_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_39" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_39_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_40" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_40_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_41" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_41_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_42" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_42_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_43" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_43_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_44" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_44_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_45" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_45_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_46" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_46_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_47" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_47_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_48" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_48_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_49" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_49_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>
                        <td style="height:1.8em;width:240px;vertical-align:top;border:solid 1px white;overflow:hidden;">
                            <asp:TextBox id="WF_Rep_CHEDULE_50" runat="server" BorderWidth="1px"  Borderstyle="Solid" BorderColor="black" Width="9.7em" CssClass="WF_TEXT_LEFT2" TextMode="MultiLine"></asp:TextBox>
                            <asp:Label ID="WF_Rep_CHEDULE_50_STP" hidden="hidden" runat="server" Text="Label"></asp:Label>
                        </td>

                        </tr>
                        </table>
                    </ItemTemplate>

                    <FooterTemplate>
                    </FooterTemplate>
             
                </asp:Repeater>
                </span>
            </div>

        </div>

        <!-- 全体レイアウト　detailbox -->
        <div  class="detailbox" id="detailbox" hidden="hidden">
        </div>

        <div hidden="hidden">
            <input id="Selected_STYYMMDD" runat="server" value=""  type="text" />           <!-- 対象開始日 -->
            <input id="Selected_ENDYYMMDD" runat="server" value=""  type="text" />         <!-- 対象終了日 -->

            <input id="WF_SELECTYYMM" runat="server" value=""  type="text" />           <!-- 処理年月(yyyy年MM月) -->
            <input id="WF_SELECTYYMMDD" runat="server" value=""  type="text" />         <!-- 処理日(yyyy/MM/dd) -->
            <input id="WF_SELECTAREA" runat="server" value=""  type="text" />           <!-- 処理地域 -->

            <asp:ListBox ID="WF_HEADuser" runat="server"></asp:ListBox>                 <!-- 情報退避() -->
            <asp:ListBox ID="WF_HEADdate_YMD" runat="server"></asp:ListBox>             <!-- 情報退避() -->
            <asp:ListBox ID="WF_HEADdate_WEEKKBN" runat="server"></asp:ListBox>         <!-- 情報退避() -->
            <asp:ListBox ID="WF_HEADdate_TEXT" runat="server"></asp:ListBox>            <!-- 情報退避() -->

            <input id="WF_REP_LineCnt"  runat="server" value=""  type="text" />         <!-- 明細 変更位置 -->
            <input id="WF_REP_ColCnt"  runat="server" value=""  type="text" />          <!-- 明細 変更位置 -->
            <input id="WF_SaveX"  runat="server" value=""  type="text" />               <!-- 明細 変更位置X軸 -->
            <input id="WF_SaveY"  runat="server" value=""  type="text" />               <!-- 明細 変更位置Y軸 -->
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />      <!-- ボタン押下 -->
            <asp:TextBox ID="WF_TERMID" runat="server"></asp:TextBox>               <!-- 端末ID　 -->
            <asp:TextBox ID="WF_TERMCAMP" runat="server"></asp:TextBox>             <!-- 端末会社　 -->

        </div>

</asp:Content> 

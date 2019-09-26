<%@ Page Title="CO0106" Language="vb" AutoEventWireup="false" CodeBehind="GRCO0106LIBSEND.aspx.vb" Inherits="OFFICE.GRCO0106LIBSEND" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRCO0106WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<asp:Content ID="GRCO0106H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/CO0106.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/CO0106.js")%>"></script>
</asp:Content> 
<asp:Content ID="GRCO0106" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　headerbox -->
        <div  class="headerbox" id="headerbox">
            <div class="Operation" style="margin-left:3em;margin-top:0.5em;">
                <a style="position:fixed;top:2.8em;left:1.0em;"  onclick="ButtonClick('WF_ALLSELECT');">
                    <asp:Button ID="WF_ButtonALLSELECT" runat="server" Text="全選択" Width="5em"  />
                </a>
                <a style="position:fixed;top:2.8em;left:6.0em;" onclick="ButtonClick('WF_ALLCANCEL');">
                    <asp:Button ID="WF_ButtonALLCANCEL" runat="server" Text="全解除"  Width="5em"   />
                </a>

                <a style="position:fixed;top:2.8em;left:11.0em;">
                    <asp:Label ID="Label5"  runat="server" Text="配信Ver." Height="1.1em" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
                <a style="position:fixed;top:2.8em;left:15.0em;">
                    <asp:TextBox ID="WF_VER" runat="server" Text=""  Width="10em" />
                </a>

                <a style="position:fixed;top:2.8em;left:27.0em;" onclick="ButtonClick('PSTAT');" >
                    <asp:Button ID="WF_ButtonPSTAT" runat="server" Text="ping確認"  Width="5em" />
                </a>

                <a style="position:fixed;top:2.8em;left:31.5em;" onclick="ButtonClick('SSTAT');" >
                    <asp:Button ID="WF_ButtonSSTAT" runat="server" Text="ｻｰﾋﾞｽ確認"  Width="5em"  />
                </a>

                <a style="position:fixed;top:2.8em;left:36.0em;" onclick="ButtonClick('OSTAT');" >
                    <asp:Button ID="WF_ButtonOSTAT" runat="server" Text="ｵﾝﾗｲﾝ確認"  Width="5em" />
                </a>

                <a style="position:fixed;top:2.8em;left:42.0em;" onclick="ButtonClick('OSTOP');" >
                    <asp:Button ID="WF_ButtonOSTOP" runat="server" Text="ｵﾝﾗｲﾝ停止"  Width="5em" />
                </a>

                <a style="position:fixed;top:2.8em;left:46.5em;"  onclick="ButtonClick('OSTART');">
                    <asp:Button ID="WF_ButtonOSTART" runat="server" Text="ｵﾝﾗｲﾝ開始"  Width="5em" />
                </a>

                <a style="position:fixed;top:2.8em;left:52.5em;"  onclick="ButtonClick('SSTOP');">
                    <asp:Button ID="WF_ButtonSSTOP" runat="server" Text="ｻｰﾋﾞｽ停止"  Width="5em" />
                </a>

                <a style="position:fixed;top:2.8em;left:57em;" onclick="ButtonClick('SSTART');">
                    <asp:Button ID="WF_ButtonSSTART" runat="server" Text="ｻｰﾋﾞｽ開始"  Width="5em" />
                </a>

                <a style="position:fixed;top:2.8em;left:62.5em;">
                   <asp:checkbox ID="WF_ALLsned" text="ソースも配信" runat="server" Width="8.0em" Checked="false" />
                </a>
                <a style="position:fixed;top:2.8em;left:70em;"  onclick="ButtonClick('SEND')">
                   <asp:Button ID="WF_ButtonSEND" runat="server" Text="配信"  Width="5em" />
                </a>

                <a style="position:fixed;top:2.8em;left:75em;" onclick="ButtonClick('WF_ButtonEND');">
                    <asp:Button ID="WF_ButtonEND" runat="server" Text="終了"  Width="5em"  />
                </a>
            </div>
        </div>

        <!-- 全体レイアウト　detailbox -->
        <div  class="detailbox" id="detailbox">

            <!-- ■■■　明細ヘッダ　■■■ -->
            <table style="position:fixed;top:5em; bottom:35.8em;left:1.0em;width:68.0em;margin:0px 0px 0px 0px;border-collapse:collapse;">
                <tr>          
                    <td style="height:1.3em;width:3.0em;" rowspan="2">
                        <!-- ■　チェックボックス　■ -->
                        <asp:Label ID="Label1"  runat="server" Text="選択" Height="1.3em" Width="3.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="height:1.3em;width:5.0em;" rowspan="2">
                        <!-- ■　端末ＩＤ　■ -->
                        <asp:Label ID="Label2"  runat="server" Text="端末ＩＤ" Height="1.3em" Width="8.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="height:1.3em;width:13.0em;" rowspan="2">
                        <!-- ■　設置会社　■ -->
                        <asp:Label ID="LabeL12"  runat="server" Text="設置会社" Height="1.3em" Width="13.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="height:1.3em;width:11.0em;" rowspan="2">
                        <!-- ■　設置場所　■ -->
                        <asp:Label ID="LabeL3"  runat="server" Text="設置場所" Height="1.3em" Width="11.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="height:1.3em;width:8.0em;" rowspan="2">
                        <!-- ■　ＩＰアドレス　■ -->
                        <asp:Label ID="Label4"  runat="server" Text="ＩＰアドレス" Height="1.3em" Width="8.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="height:1.3em;width:10.5em;" colspan="3">
                        <!-- ■　接続、サービス、オンライン状態　■ -->
                        <asp:Label ID="LabeL10"  runat="server" Text="　状　　　態" Height="1.3em" Width="10.5em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="height:1.3em;width:12.0em;" colspan="2">
                        <!-- ■　配信状態　■ -->
                        <asp:Label ID="LabeL11"  runat="server" Text="　配　　信" Height="1.3em" Width="12.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="height:1.3em;width:20.0em;" rowspan="2">
                        <!-- ■　備考　■ -->
                        <asp:Label ID="Label8"  runat="server" Text="備　　考" Height="1.3em" Width="20em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td style="height:1.3em;width:3.0em;">
                        <!-- ■　接続状態　■ -->
                        <asp:Label ID="LabeL17"  runat="server" Text="ping" Height="1.3em" Width="3.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                     <td style="height:1.3em;width:5.5em;">
                        <!-- ■　サービス状態　■ -->
                        <asp:Label ID="LabeL7"  runat="server" Text="ｻｰﾋﾞｽ" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                   <td style="height:1.3em;width:5.0em;">
                        <!-- ■　オンライン状態　■ -->
                        <asp:Label ID="LabeL19"  runat="server" Text="ｵﾝﾗｲﾝ" Height="1.3em" Width="5.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="height:1.3em;width:7.0em;" rowspan="2">
                        <!-- ■　配信Ver.　■ -->
                        <asp:Label ID="Label6"  runat="server" Text="Ver." Height="1.3em" Width="7.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="height:1.3em;width:5.0em;text-wrap:normal;" rowspan="2">
                        <!-- ■　配信状態　■ -->
                        <asp:Label ID="Label9"  runat="server" Text="状態" Height="1.3em" Width="5.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                </tr>
            </table>

            <!-- ■■■　明　細　■■■ -->
           <span class="WF_Repeater" style="position:fixed;top:7.5em; bottom:2em;left:1.0em;width:82.0em;overflow:auto;background-color:white;table-layout:fixed;margin:0px 0px 0px 0px;" >

                <asp:Repeater ID="WF_Repeater" runat="server" >
                    <HeaderTemplate>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <table style="border:solid;border-width:thin;margin:0px 0px 0px 0px;">
                           <tr style="<%# DataBinder.Eval(Container.DataItem, "COLOR") %>">
                                <!-- ■　選択　■ -->
                                <td style="height:1.3em;width:2.0em;">
                                    <asp:checkbox ID="WF_Rep_CheckBox" runat="server" Width="2.0em" Checked="True" />
                                </td>

                                <!-- ■　端末ＩＤ　■ -->
                                <td style="height:1.3em;width:5.0em;white-space: nowrap;">
                                    <asp:label ID="WF_Rep_TERMID" runat="server" Height="1.1em" Width="8.0em" CssClass="WF_TEXT_LEFT"></asp:label>
                                </td>

                                <!-- ■　設置会社　■ -->
                                <td style="height:1.3em;width:10.0em;white-space: nowrap;">
                                    <asp:label ID="WF_Rep_CAMPNAME" runat="server" Height="1.1em" Width="13.0em" CssClass="WF_TEXT_LEFT"></asp:label>
                                </td>

                                <!-- ■　設置場所　■ -->
                                <td style="height:1.3em;width:7.0em;white-space: nowrap;">
                                    <asp:label ID="WF_Rep_TERMNAME" runat="server" Height="1.1em" Width="12.0em" CssClass="WF_TEXT_LEFT"></asp:label>
                                </td>

                                <!-- ■　IPアドレス　■ -->
                                <td style="height:1.3em;width:7.0em;white-space: nowrap;">
                                    <asp:label ID="WF_Rep_IPADDR" runat="server" Height="1.1em" Width="7.0em" CssClass="WF_TEXT_LEFT"></asp:label>
                                </td>

                                <!-- ■　接続状態　■ -->
                                <td style="height:1.3em;width:3.0em;white-space: nowrap;">
                                    <asp:label ID="WF_Rep_NETSTAT" runat="server" Height="1.1em" Width="3.0em" CssClass="WF_TEXT_LEFT"></asp:label>
                                </td>

                                <!-- ■　サービス状態　■ -->
                                <td style="height:1.3em;width:5.0em;white-space: nowrap;">
                                    <asp:label ID="WF_Rep_SERVICESTAT" runat="server" Height="1.1em" Width="5.0em" CssClass="WF_TEXT_LEFT"></asp:label>
                                </td>

                                <!-- ■　オンライン状態　■ -->
                                <td style="height:1.3em;width:5.0em;white-space: nowrap;">
                                    <asp:label ID="WF_Rep_ONLINESTAT" runat="server" Height="1.1em" Width="5.0em" CssClass="WF_TEXT_LEFT"></asp:label>
                                </td>

                                <!-- ■　配信日時　■ -->
                                <td style="height:1.3em;width:8.0em;white-space: nowrap;">
                                    <asp:label ID="WF_Rep_SENDTIME" runat="server" Height="1.1em" Width="8.0em" CssClass="WF_TEXT_LEFT"></asp:label>
                                </td>

                                <!-- ■　配信状態　■ -->
                                <td style="height:1.3em;width:3.0em;white-space: nowrap;">
                                    <asp:label ID="WF_Rep_SENDSTAT" runat="server" Height="1.1em" Width="3.0em" CssClass="WF_TEXT_CENTER"></asp:label>
                                </td>

                                <!-- ■　備考　■ -->
                                <td style="height:1.3em;width:30em;white-space: nowrap;">
                                    <asp:textbox ID="WF_Rep_NOTES" runat="server" Height="1.1em" Width="30em" CssClass="WF_TEXT_LEFT" Enabled="True" ReadOnly="True" BorderStyle="None" ForeColor="Red"></asp:textbox>
                                </td>

                           </tr>
                        </table>
                    </ItemTemplate>

                    <FooterTemplate>
                    </FooterTemplate>
             
                </asp:Repeater>
            </span>

        </div>

        <div hidden="hidden">
                <input id="WF_RightViewChange" runat="server" value="" type="text"/>        <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />         <!-- Rightbox 開閉 -->
                <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
                <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />      <!-- 権限 -->
                <input id="WF_PrintURL" runat="server" value=""  type="text" />        <!-- Textbox Print URL -->

                <input id="WF_DSP_INIT" runat="server" value="" type="text"/>               <!-- 画面初期表示 -->
                <input id="WF_STAT_BTN" runat="server" value="" type="text"/>               <!-- 状態確認ボタン押下 -->
                <input id="WF_PSTAT_BTN" runat="server" value="" type="text"/>              <!-- ping確認ボタン押下 -->
                <input id="WF_SSTAT_BTN" runat="server" value="" type="text"/>              <!-- サービス確認ボタン押下 -->
                <input id="WF_OSTAT_BTN" runat="server" value="" type="text"/>              <!-- オンライン確認ボタン押下 -->
                <input id="WF_OSTART_BTN" runat="server" value="" type="text"/>             <!-- オンライン開始ボタン押下 -->
                <input id="WF_OSTOP_BTN" runat="server" value="" type="text"/>              <!-- オンライン停止ボタン押下 -->
                <input id="WF_SSTART_BTN" runat="server" value="" type="text"/>             <!-- サービス開始ボタン押下 -->
                <input id="WF_SSTOP_BTN" runat="server" value="" type="text"/>              <!-- サービス停止ボタン押下 -->
                <input id="WF_SEND_BTN" runat="server" value="" type="text"/>               <!-- 配信ボタン押下 -->

        </div>
    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />
</asp:Content>
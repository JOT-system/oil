<%@ Page Title="TA0002" Language="vb" AutoEventWireup="false" CodeBehind="GRTA0002KINTAILIST.aspx.vb" Inherits="OFFICE.GRTA0002KINTAILIST" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>

<%@ register src="~/GR/inc/GRTA0002WRKINC.ascx" tagname="work" tagprefix="LSINC" %>
<asp:Content ID="GRTA0002H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/TA0002.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/TA0002.js")%>"></script>
</asp:Content>
<asp:Content ID="GRTA0002" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　headerbox -->
        <div  class="headerboxOnly" id="headerbox">
            <div class="Operation" id ="Operation">
                <!-- ■　条件　■ -->
                <a style="position:fixed;top:3.0em;left:15em;width:15.0em;">
                    <asp:Label runat="server" Text="対象年月：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_DATE" runat="server"  Font-Bold="True"></asp:Label>
                </a>
                <a style="position:fixed;top:3.0em;left:30em;width:15.0em;">
                    <asp:Label runat="server" Text="配属部署：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_ORG" runat="server"  Font-Bold="True"></asp:Label>
                </a>
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:53.5em;">
                    <input type="button" id="WF_ButtonPDF" value="全印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPDF');" />
                </a>
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonXLS" value="Excel取得"  style="Width:5em" onclick="ButtonClick('WF_ButtonXLS');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonZIP" value="全ZIP取得"  style="Width:5em" onclick="ButtonClick('WF_ButtonZIP');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.2em;left:75em;">
                    <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" Height="1em" ImageAlign="AbsMiddle" onclick="ButtonClick('WF_ButtonFIRST');" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" Height="1em" ImageAlign="AbsMiddle" onclick="ButtonClick('WF_ButtonLAST');" />
                </a>
            </div>
            <div id="leftMenubox" class="leftMenubox">
                <div style="overflow-y:auto;height:1.8em;width:12.0em;text-align:center;vertical-align:central;color:white;background-color:rgb(22,54,92);font-weight:bold;border: solid black;border-width:1.5px;">
                    <!-- ■　照会選択　■ -->
                    <a style="width:12.0em;font-size:medium;overflow:hidden;color:white;background-color:rgb(22,54,92);text-align:center;">照会選択</a>
                </div>

                <div id="STAFFSelect" style="overflow-y:auto;height:497px;width:12.0em;color:black;background-color: white;border: solid;border-width:1.5px;">
                    <!-- ■　セレクター　■ -->
                    <asp:Repeater ID="WF_SELECTOR" runat="server">
                        <HeaderTemplate> 
                            <table style="border-width:1px;margin:0.1em 0.1em 0.1em 0.1em;">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <!-- 非表示項目(左Box処理用・Repeater内行位置)　-->
                                <td style="height:1.3em;width:11.8em;" hidden="hidden">
                                    <asp:Label ID="WF_SELECTOR_VALUE" runat="server"></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="WF_SELECTOR_TEXT" runat="server" Text="" Height="1.3em" Width="11.8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>
                            </tr> 
                        </ItemTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
            <!-- 一覧レイアウト -->
            <div id="divListArea">
                <asp:panel id="pnlListArea" runat="server" ></asp:panel>
            </div>
        </div>

        <!-- 全体レイアウト　detailbox -->
        <div  class="Detail" id="detailbox" runat="server">
                <a style="position:fixed;top:3.2em;left:67em;">
                    <input type="button" id="WF_BACK" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_BACK');" />
                </a>
            <!-- ■　選択No　■ -->
            <a style="position:fixed;top:3.7em;left:3em; width:32em;" hidden="hidden">
                <asp:Label ID="WF_Head_LINECNT_L" runat="server" Text="選択No" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false"></asp:Label>
                <asp:TextBox ID="WF_Head_LINECNT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            </a>

            <!-- ■　従業員　■ -->
            <a style="position:fixed;top:5.1em;left:3em; width:32em;" >
                <asp:Label ID="WF_STAFFCODE_L" runat="server" Text="従業員" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                <b>
                <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true"></asp:TextBox>
                </b>
                <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>
            <!-- ■　配属部署　■ -->
            <a style="position:fixed;top:6.2em;left:3em; width:32em;">
                <asp:Label ID="WF_HORG_L" runat="server" Text="配属部署" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                <b>
                <asp:TextBox ID="WF_HORG" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true"></asp:TextBox>
                </b>
                <asp:Label ID="WF_HORG_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>

            <!-- ■　対象年月日　■ -->
            <a style="position:fixed;top:5.1em;left:20em; width:32em;" >
                <asp:TextBox ID="WF_WORKDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                <asp:Label ID="WF_WORKINGWEEK_TEXT" text="月曜日" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>

            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <!-- エネックス用                                                                                                                                   -->
            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <span class="ENEX">

                <!-- ■　見出し　■ -->
                <a style="position:fixed;top:7.4em;left:26em; width:32em;">
                    <asp:Label ID="Label20" runat="server" Text="日付" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:32em; width:32em;">
                    <asp:Label ID="Label22" runat="server" Text="時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:43em; width:32em;">
                    <asp:Label ID="Label21" runat="server" Text="時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:54.5em; width:32em;">
                    <asp:Label ID="Label23" runat="server" Text="業務手当" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:67em; width:32em;">
                    <asp:Label ID="Label24" runat="server" Text="残業" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:73em; width:32em;">
                    <asp:Label ID="Label25" runat="server" Text="深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <!-- ■　休日区分　■ -->
                <a style="position:fixed;top:8.5em;left:3em; width:32em;" >
                    <asp:Label ID="WF_HOLIDAYKBN_L" runat="server" Text="休日区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false" readonly="true"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HOLIDAYKBN" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_HOLIDAYKBN_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　勤怠区分　■ -->
                <a style="position:fixed;top:9.6em;left:3em; width:32em;" >
                    <asp:Label ID="WF_PAYKBN_L" runat="server" Text="勤怠区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_PAYKBN" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_PAYKBN_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　宿直区分　■ -->
                <a style="position:fixed;top:10.7em;left:3em; width:32em;" >
                    <asp:Label ID="WF_SHUKCHOKKBN_L" runat="server" Text="宿直区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKKBN" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_SHUKCHOKKBN_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>


                <!-- ■　出社日　■ -->
                <a style="position:fixed;top:8.5em;left:20em; width:32em;">
                    <asp:Label ID="WF_STDATE_L" runat="server" Text="出社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　出社時間　■ -->
                <a style="position:fixed;top:8.5em;left:30em; width:32em;">
                    <asp:TextBox ID="WF_STTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                </a>
                <!-- ■　拘束開始　■ -->
                <a style="position:fixed;top:9.6em;left:20em; width:32em;">
                    <asp:Label ID="WF_BINDSTDATE_L" runat="server" Text="拘束開始" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:9.6em;left:30em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDSTDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　拘束時間　■ -->
                <a style="position:fixed;top:10.7em;left:20em; width:32em;">
                    <asp:Label ID="WF_BINDTIME_L" runat="server" Text="拘束時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:10.7em;left:30em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社日　■ -->
                <a style="position:fixed;top:11.8em;left:20em; width:32em;">
                    <asp:Label ID="WF_ENDDATE_L" runat="server" Text="退社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ENDDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社時間　■ -->
                <a style="position:fixed;top:11.8em;left:30em; width:32em;">
                    <asp:TextBox ID="WF_ENDTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                </a>


                <!-- ■　日報休憩　■ -->
                <a style="position:fixed;top:8.5em;left:37em; width:32em;">
                    <asp:Label ID="WF_NIPPOBREAKTIME_L" runat="server" Text="日報休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NIPPOBREAKTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休憩　■ -->
                <a style="position:fixed;top:9.6em;left:37em; width:32em;">
                    <asp:Label ID="WF_BREAKTIME_L" runat="server" Text="休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_BREAKTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　特作Ｉ　■ -->
                <a style="position:fixed;top:10.7em;left:37em; width:32em;">
                    <asp:Label ID="WF_TOKUSA1TIME_L" runat="server" Text="特作Ⅰ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■　保安検査　■ -->
                <a style="position:fixed;top:11.8em;left:37em; width:25em;">
                    <asp:Label ID="WF_HOANTIME_L" runat="server" Text="保安検査" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HOANTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　高圧作業　■ -->
                <a style="position:fixed;top:12.9em;left:37em; width:25em;">
                    <asp:Label ID="WF_KOATUTIME_L" runat="server" Text="高圧作業" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KOATUTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　早出補填　■ -->
                <a style="position:fixed;top:14.0em;left:37em; width:25em;">
                    <asp:Label ID="WF_HAYADETIME_L" runat="server" Text="早出補填" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAYADETIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>


                <!-- ■　作業手当Ａ　■ -->
                <a style="position:fixed;top:8.5em;left:49em; width:32em;">
                    <asp:Label ID="WF_TOKSAAKAISU_L" runat="server" Text="　　　　Ａ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKSAAKAISU" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　作業手当Ｂ　■ -->
                <a style="position:fixed;top:9.6em;left:49em; width:32em;">
                    <asp:Label ID="WF_TOKSABKAISU_L" runat="server" Text="　　　　Ｂ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKSABKAISU" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　作業手当Ｃ　■ -->
                <a style="position:fixed;top:10.7em;left:49em; width:32em;">
                    <asp:Label ID="WF_TOKSACKAISU_L" runat="server" Text="　　　　Ｃ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKSACKAISU" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　点呼回数　■ -->
                <a style="position:fixed;top:11.8em;left:49em; width:32em;">
                    <asp:Label ID="WF_TENKOKAISU_L" runat="server" Text="点呼回数" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TENKOKAISU" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>



                <!-- ■　平日残業　■ -->
                <a style="position:fixed;top:8.5em;left:61em; width:32em;">
                    <asp:Label ID="WF_ORVERTIME_L" runat="server" Text="平　　　日" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　平日深夜　■ -->
                <a style="position:fixed;top:8.5em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日出勤　■ -->
                <a style="position:fixed;top:9.6em;left:61em; width:32em;">
                    <asp:Label ID="WF_HWORKTIME_L" runat="server" Text="休日出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日深夜　■ -->
                <a style="position:fixed;top:9.6em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜出勤　■ -->
                <a style="position:fixed;top:10.7em;left:61em; width:32em;">
                    <asp:Label ID="WF_SWORKTIME_L" runat="server" Text="日曜出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜深夜　■ -->
                <a style="position:fixed;top:10.7em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　所定深夜　■ -->
                <a style="position:fixed;top:11.8em;left:61em; width:32em;">
                    <asp:Label ID="WF_NIGHTTIME_L" runat="server" Text="所定深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:11.8em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_NIGHTTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>



                <!-- ■　荷卸回数　■ -->
                <a style="position:fixed;top:14.5em;left:49em; width:32em;">
                    <asp:Label ID="WF_UNLOADCNT_L" runat="server" Text="荷卸回数" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　走行距離　■ -->
                <a style="position:fixed;top:13.4em;left:67em; width:32em;">
                    <asp:Label ID="Label16" runat="server" Text="配送" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:13.4em;left:73em; width:32em;">
                    <asp:Label ID="Label33" runat="server" Text="回送" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:14.5em;left:61em; width:32em;">
                    <asp:Label ID="WF_SOUDISTANCE_L" runat="server" Text="距　　　離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14.5em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_KAIDISTANCE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
            </span>

            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <!-- 近石用                                                                                                                                         -->
            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <span class="KNK">
                <!-- ■　見出し　■ -->
                <a style="position:fixed;top:7.4em;left:26em; width:32em;">
                    <asp:Label ID="Label3" runat="server" Text="日付" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:32em; width:32em;">
                    <asp:Label ID="Label4" runat="server" Text="時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:43em; width:32em;">
                    <asp:Label ID="Label5" runat="server" Text="時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:67em; width:32em;">
                    <asp:Label ID="Label6" runat="server" Text="残業" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:73em; width:32em;">
                    <asp:Label ID="Label7" runat="server" Text="深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <!-- ■　休日区分　■ -->
                <a style="position:fixed;top:8.5em;left:3em; width:32em;">
                    <asp:Label ID="WF_HOLIDAYKBN_KNK_L" runat="server" Text="休日区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HOLIDAYKBN_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="false" ></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_HOLIDAYKBN_TEXT_KNK" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　勤怠区分　■ -->
                <a style="position:fixed;top:9.6em;left:3em; width:32em;" >
                    <asp:Label ID="WF_PAYKBN_KNK_L" runat="server" Text="勤怠区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_PAYKBN_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" ></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_PAYKBN_TEXT_KNK" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　宿直区分　■ -->
                <a style="position:fixed;top:10.7em;left:3em; width:32em;" >
                    <asp:Label ID="WF_SHUKCHOKKBN_KNK_L" runat="server" Text="宿直区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKKBN_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" ></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_SHUKCHOKKBN_TEXT_KNK" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>


                <!-- ■　出社日　■ -->
                <a style="position:fixed;top:8.5em;left:20em; width:32em;">
                    <asp:Label ID="WF_STDATE_KNK_L" runat="server" Text="出社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STDATE_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　出社時間　■ -->
                <a style="position:fixed;top:8.5em;left:30em; width:32em;">
                    <asp:TextBox ID="WF_STTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                </a>
                <!-- ■　拘束開始　■ -->
                <a style="position:fixed;top:9.6em;left:20em; width:32em;">
                    <asp:Label ID="WF_BINDSTDATE_KNK_L" runat="server" Text="拘束開始" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:9.6em;left:30em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDSTDATE_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社日　■ -->
                <a style="position:fixed;top:10.7em;left:20em; width:32em;">
                    <asp:Label ID="WF_ENDDATE_KNK_L" runat="server" Text="退社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ENDDATE_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社時間　■ -->
                <a style="position:fixed;top:10.7em;left:30em; width:32em;">
                    <asp:TextBox ID="WF_ENDTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                </a>
                <!-- ■　所定内計　■ -->
                <a style="position:fixed;top:11.8em;left:20em; width:32em;">
                    <asp:Label ID="WF_WWORKTIME_KNK_L" runat="server" Text="所定内計" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:11.8em;left:30em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_WWORKTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　乗務日計　■ -->
                <a style="position:fixed;top:12.9em;left:20em; width:32em;">
                    <asp:Label ID="WF_JYOMUTIME_KNK_L" runat="server" Text="乗務日計" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:12.9em;left:30em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_JYOMUTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>



                <!-- ■　日報休憩　■ -->
                <a style="position:fixed;top:8.5em;left:37em; width:32em;">
                    <asp:Label ID="WF_NIPPOBREAKTIME_KNK_L" runat="server" Text="日報休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NIPPOBREAKTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休憩　■ -->
                <a style="position:fixed;top:9.6em;left:37em; width:32em;">
                    <asp:Label ID="WF_BREAKTIME_KNK_L" runat="server" Text="休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_BREAKTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　特作Ｉ　■ -->
                <a style="position:fixed;top:10.7em;left:37em; width:32em;">
                    <asp:Label ID="WF_TOKUSA1TIME_KNK_L" runat="server" Text="特作Ⅰ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■　回転　■ -->
                <a style="position:fixed;top:8.5em;left:49em; width:32em;">
                    <asp:Label ID="WF_KAITENCNT_KNK_L" runat="server" Text="回　　転" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　荷卸回数　■ -->
                <a style="position:fixed;top:9.6em;left:49em; width:32em;">
                    <asp:Label ID="WF_UNLOADCNT_KNK_L" runat="server" Text="届　　数" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　走行距離　■ -->
                <a style="position:fixed;top:10.7em;left:49em; width:32em;">
                    <asp:Label ID="WF_HAIDISTANCE_KNK_L" runat="server" Text="配送距離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.8em;left:49em; width:32em;">
                    <asp:Label ID="WF_KAIDISTANCE_L" runat="server" Text="回送距離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAIDISTANCE_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■　平日残業　■ -->
                <a style="position:fixed;top:8.5em;left:61em; width:32em;">
                    <asp:Label ID="WF_ORVERTIME_KNK_L" runat="server" Text="平　　　日" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　平日深夜　■ -->
                <a style="position:fixed;top:8.5em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日出勤　■ -->
                <a style="position:fixed;top:9.6em;left:61em; width:32em;">
                    <asp:Label ID="WF_HWORKTIME_KNK_L" runat="server" Text="休日出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日深夜　■ -->
                <a style="position:fixed;top:9.6em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　代休出勤　■ -->
                <a style="position:fixed;top:10.7em;left:61em; width:32em;">
                    <asp:Label ID="WF_HDAIWORKTIME_KNK_L" runat="server" Text="代休出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HDAIWORKTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　代休深夜　■ -->
                <a style="position:fixed;top:10.7em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_HDAINIGHTTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜出勤　■ -->
                <a style="position:fixed;top:11.8em;left:61em; width:32em;">
                    <asp:Label ID="WF_SWORKTIME_KNK_L" runat="server" Text="日曜出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜深夜　■ -->
                <a style="position:fixed;top:11.8em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜代休出勤　■ -->
                <a style="position:fixed;top:12.9em;left:61em; width:32em;">
                    <asp:Label ID="WF_SDAIWORKTIME_KNK_L" runat="server" Text="日曜代休" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SDAIWORKTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜代休深夜　■ -->
                <a style="position:fixed;top:12.9em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_SDAINIGHTTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　所定深夜　■ -->
                <a style="position:fixed;top:14.0em;left:61em; width:32em;">
                    <asp:Label ID="WF_NIGHTTIME_KNK_L" runat="server" Text="所定深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:14.0em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_NIGHTTIME_KNK" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
            </span>

            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <!-- NJS用                                                                                                                                          -->
            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <span class="NJS">
                <!-- ■　見出し　■ -->
                <a style="position:fixed;top:7.4em;left:26em; width:32em;">
                    <asp:Label ID="Label54" runat="server" Text="日付" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:32em; width:32em;">
                    <asp:Label ID="Label55" runat="server" Text="時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:43em; width:32em;">
                    <asp:Label ID="Label56" runat="server" Text="時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:53.5em; width:32em;">
                    <b>
                    <asp:CheckBox ID="WF_MODIFY_NJS" runat="server" Height="1.2em" Width="1em"  Readonly="true"></asp:CheckBox>
                    </b>
                    <asp:Label ID="Label57" runat="server" Text="ﾓﾃﾞﾙ距離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:67em; width:32em;">
                    <asp:Label ID="Label58" runat="server" Text="残業" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:73em; width:32em;">
                    <asp:Label ID="Label59" runat="server" Text="深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <!-- ■　休日区分　■ -->
                <a style="position:fixed;top:8.5em;left:3em; width:32em;">
                    <asp:Label ID="WF_HOLIDAYKBN_NJS_L" runat="server" Text="休日区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HOLIDAYKBN_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" ></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_HOLIDAYKBN_TEXT_NJS" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　勤怠区分　■ -->
                <a style="position:fixed;top:9.6em;left:3em; width:32em;" >
                    <asp:Label ID="WF_PAYKBN_NJS_L" runat="server" Text="勤怠区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_PAYKBN_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" ></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_PAYKBN_TEXT_NJS" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　宿直区分　■ -->
                <a style="position:fixed;top:10.7em;left:3em; width:32em;" >
                    <asp:Label ID="WF_SHUKCHOKKBN_NJS_L" runat="server" Text="宿直区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKKBN_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" ></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_SHUKCHOKKBN_TEXT_NJS" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>


                <!-- ■　出社日　■ -->
                <a style="position:fixed;top:8.5em;left:20em; width:32em;">
                    <asp:Label ID="WF_STDATE_NJS_L" runat="server" Text="出社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STDATE_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　出社時間　■ -->
                <a style="position:fixed;top:8.5em;left:30em; width:32em;">
                    <asp:TextBox ID="WF_STTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                </a>
                <!-- ■　拘束開始　■ -->
                <a style="position:fixed;top:9.6em;left:20em; width:32em;">
                    <asp:Label ID="WF_BINDSTDATE_NJS_L" runat="server" Text="拘束開始" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:9.6em;left:30em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDSTDATE_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　拘束時間　■ -->
                <a style="position:fixed;top:10.7em;left:20em; width:32em;">
                    <asp:Label ID="WF_BINDTIME_NJS_L" runat="server" Text="拘束時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:10.7em;left:30em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社日　■ -->
                <a style="position:fixed;top:11.8em;left:20em; width:32em;">
                    <asp:Label ID="WF_ENDDATE_NJS_L" runat="server" Text="退社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ENDDATE_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社時間　■ -->
                <a style="position:fixed;top:11.8em;left:30em; width:32em;">
                    <asp:TextBox ID="WF_ENDTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                </a>



                <!-- ■　日報休憩　■ -->
                <a style="position:fixed;top:8.5em;left:37em; width:32em;">
                    <asp:Label ID="WF_NIPPOBREAKTIME_NJS_L" runat="server" Text="日報休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NIPPOBREAKTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休憩　■ -->
                <a style="position:fixed;top:9.6em;left:37em; width:32em;">
                    <asp:Label ID="WF_BREAKTIME_NJS_L" runat="server" Text="休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_BREAKTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　特作Ｉ　■ -->
                <a style="position:fixed;top:10.7em;left:37em; width:32em;">
                    <asp:Label ID="WF_TOKUSA1TIME_NJS_L" runat="server" Text="特作" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　配送時間　■ -->
                <a style="position:fixed;top:11.8em;left:37em; width:32em;">
                    <asp:Label ID="WF_HAISOTIME_NJS_L" runat="server" Text="配送時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAISOTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　車h中伯　■ -->
                <a style="position:fixed;top:12.9em;left:37em; width:32em;">
                    <asp:Label ID="WF_SHACHUHAKKBN_NJS_L" runat="server" Text="車中泊" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:CheckBox ID="WF_SHACHUHAKKBN_NJS" runat="server" Height="1.2em" Width="6em"  Readonly="true"></asp:CheckBox>
                    </b>
                </a>


                <!-- ■　単車ラテックス　■ -->
                <a style="position:fixed;top:8.5em;left:49em; width:32em;">
                    <asp:Label ID="WF_MODELDISTANCE109_NJS_L" runat="server" Text="単・ﾗﾃｯｸｽ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE109_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true"  style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　ﾄﾚｰﾗラテックス　■ -->
                <a style="position:fixed;top:9.6em;left:49em; width:32em;">
                    <asp:Label ID="WF_MODELDISTANCE209_NJS_L" runat="server" Text="ﾄﾚ・ﾗﾃｯｸｽ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE209_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　ﾄﾚｰﾗラLNG　■ -->
                <a style="position:fixed;top:10.7em;left:49em; width:32em;">
                    <asp:Label ID="WF_MODELDISTANCE204_NJS_L" runat="server" Text="ﾄﾚ・LNG" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE204_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>


                <!-- ■　平日残業　■ -->
                <a style="position:fixed;top:8.5em;left:61em; width:32em;">
                    <asp:Label ID="WF_ORVERTIME_NJS_L" runat="server" Text="平　　　日" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　平日深夜　■ -->
                <a style="position:fixed;top:8.5em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日出勤　■ -->
                <a style="position:fixed;top:9.6em;left:61em; width:32em;">
                    <asp:Label ID="WF_HWORKTIME_NJS_L" runat="server" Text="休日出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日深夜　■ -->
                <a style="position:fixed;top:9.6em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜出勤　■ -->
                <a style="position:fixed;top:10.7em;left:61em; width:32em;">
                    <asp:Label ID="WF_SWORKTIME_NJS_L" runat="server" Text="日曜出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜深夜　■ -->
                <a style="position:fixed;top:10.7em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　所定深夜　■ -->
                <a style="position:fixed;top:11.8em;left:61em; width:32em;">
                    <asp:Label ID="WF_NIGHTTIME_NJS_L" runat="server" Text="所定深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:11.8em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_NIGHTTIME_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>



                <!-- ■　走行距離　■ -->
                <a style="position:fixed;top:13.4em;left:67em; width:32em;">
                    <asp:Label ID="Label78" runat="server" Text="走行" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:13.4em;left:73em; width:32em;">
                    <asp:Label ID="Label79" runat="server" Text="回送" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:14.5em;left:61em; width:32em;">
                    <asp:Label ID="Label80" runat="server" Text="距離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14.5em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_KAIDISTANCE_NJS" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

            </span>

            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <!-- JKT用                                                                                                                                          -->
            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <span class="JKT">
                <!-- ■　見出し　■ -->
                <a style="position:fixed;top:7.4em;left:26em; width:32em;">
                    <asp:Label ID="Label81" runat="server" Text="日付" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:32em; width:32em;">
                    <asp:Label ID="Label82" runat="server" Text="時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:42.5em; width:32em;">
                    <asp:Label ID="Label83" runat="server" Text="時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:54.5em; width:32em;">
                    <asp:Label ID="Label84" runat="server" Text="回数" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:65.5em; width:32em;">
                    <asp:Label ID="Label85" runat="server" Text="残業" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:70.8em; width:32em;">
                    <asp:Label ID="Label86" runat="server" Text="深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <!-- ■　休日区分　■ -->
                <a style="position:fixed;top:8.5em;left:3em; width:32em;">
                    <asp:Label ID="WF_HOLIDAYKBN_JKT_L" runat="server" Text="休日区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HOLIDAYKBN_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" ></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_HOLIDAYKBN_TEXT_JKT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　勤怠区分　■ -->
                <a style="position:fixed;top:9.6em;left:3em; width:32em;" >
                    <asp:Label ID="WF_PAYKBN_JKT_L" runat="server" Text="勤怠区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_PAYKBN_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" ></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_PAYKBN_TEXT_JKT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　宿直区分　■ -->
                <a style="position:fixed;top:10.7em;left:3em; width:32em;" >
                    <asp:Label ID="WF_SHUKCHOKKBN_JKT_L" runat="server" Text="宿直区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKKBN_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" ></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_SHUKCHOKKBN_TEXT_JKT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>


                <!-- ■　出社日　■ -->
                <a style="position:fixed;top:8.5em;left:20em; width:32em;">
                    <asp:Label ID="WF_STDATE_JKT_L" runat="server" Text="出社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STDATE_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　出社時間　■ -->
                <a style="position:fixed;top:8.5em;left:30em; width:32em;">
                    <asp:TextBox ID="WF_STTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                </a>
                <!-- ■　拘束開始　■ -->
                <a style="position:fixed;top:9.6em;left:20em; width:32em;">
                    <asp:Label ID="WF_BINDSTDATE_JKT_L" runat="server" Text="拘束開始" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:9.6em;left:30em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDSTDATE_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　拘束時間　■ -->
                <a style="position:fixed;top:10.7em;left:20em; width:32em;">
                    <asp:Label ID="WF_BINDTIME_JKT_L" runat="server" Text="拘束時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:10.7em;left:30em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社日　■ -->
                <a style="position:fixed;top:11.8em;left:20em; width:32em;">
                    <asp:Label ID="WF_ENDDATE_JKT_L" runat="server" Text="退社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ENDDATE_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社時間　■ -->
                <a style="position:fixed;top:11.8em;left:30em; width:32em;">
                    <asp:TextBox ID="WF_ENDTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                </a>



                <!-- ■　日報休憩　■ -->
                <a style="position:fixed;top:8.5em;left:36em; width:32em;">
                    <asp:Label ID="WF_NIPPOBREAKTIME_JKT_L" runat="server" Text="日報休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NIPPOBREAKTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休憩　■ -->
                <a style="position:fixed;top:9.6em;left:36em; width:32em;">
                    <asp:Label ID="WF_BREAKTIME_JKT_L" runat="server" Text="休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_BREAKTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　特作Ｉ　■ -->
                <a style="position:fixed;top:10.7em;left:36em; width:32em;">
                    <asp:Label ID="WF_TOKUSA1TIME_JKT_L" runat="server" Text="特作Ⅰ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　車中泊　■ -->
                <a style="position:fixed;top:12.0em;left:36em; width:32em;">
                    <asp:Label ID="WF_SHACHUHAKKBN_JKT_L" runat="server" Text="車中泊" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:CheckBox ID="WF_SHACHUHAKKBN_JKT" runat="server" Height="1.2em" Width="6em"  Readonly="true"></asp:CheckBox>
                    </b>
                </a>
                <!-- ■　洗浄回数　■ -->
                <a style="position:fixed;top:13.1em;left:36em; width:25em;">
                    <asp:Label ID="WF_SENJYOCNT_JKT_L" runat="server" Text="洗浄回数" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SENJYOCNT_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■　危険品回数100（荷卸時加算単価）　■ -->
                <a style="position:fixed;top:8.5em;left:47em; width:32em;">
                    <asp:Label ID="WF_UNLOADADDCNT1_JKT_L" runat="server" Text="卸危険品100" Height="1.2em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADADDCNT1_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　危険品回数200（荷卸時加算単価）　■ -->
                <a style="position:fixed;top:9.6em;left:47em; width:32em;">
                    <asp:Label ID="WF_UNLOADADDCNT2_JKT_L" runat="server" Text="卸危険品200" Height="1.2em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADADDCNT2_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　危険品回数800（荷卸時加算単価）　■ -->
                <a style="position:fixed;top:10.7em;left:47em; width:32em;">
                    <asp:Label ID="WF_UNLOADADDCNT3_JKT_L" runat="server" Text="卸危険品800" Height="1.2em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADADDCNT3_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　危険品回数1000（荷卸時加算単価）　■ -->
                <a style="position:fixed;top:11.8em;left:47em; width:32em;">
                    <asp:Label ID="WF_LOADINGCNT1_JKT_L" runat="server" Text="積危険品1000" Height="1.2em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_LOADINGCNT1_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■　短距離手当1回数　■ -->
                <a style="position:fixed;top:12.9em;left:47em; width:32em;">
                    <asp:Label ID="WF_SHORTDISTANCE1_JKT_L" runat="server" Text="短距離1000" Height="1.2em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHORTDISTANCE1_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　短距離手当2回数　■ -->
                <a style="position:fixed;top:14em;left:47em; width:32em;">
                    <asp:Label ID="WF_SHORTDISTANCE2_JKT_L" runat="server" Text="短距離2000" Height="1.2em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHORTDISTANCE2_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■　平日残業　■ -->
                <a style="position:fixed;top:8.5em;left:59em; width:32em;">
                    <asp:Label ID="WF_ORVERTIME_JKT_L" runat="server" Text="平　　　日" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　平日深夜　■ -->
                <a style="position:fixed;top:8.5em;left:69.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日出勤　■ -->
                <a style="position:fixed;top:9.6em;left:59em; width:32em;">
                    <asp:Label ID="WF_HWORKTIME_JKT_L" runat="server" Text="休日出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日深夜　■ -->
                <a style="position:fixed;top:9.6em;left:69.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜出勤　■ -->
                <a style="position:fixed;top:10.7em;left:59em; width:32em;">
                    <asp:Label ID="WF_SWORKTIME_JKT_L" runat="server" Text="日曜出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜深夜　■ -->
                <a style="position:fixed;top:10.7em;left:69.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　所定深夜　■ -->
                <a style="position:fixed;top:11.8em;left:59em; width:32em;">
                    <asp:Label ID="WF_NIGHTTIME_JKT_L" runat="server" Text="所定深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:11.8em;left:69.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_NIGHTTIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　時給者所定内　■ -->
                <a style="position:fixed;top:12.9em;left:59em; width:32em;">
                    <asp:Label ID="WF_JIKYUSHATIME_JKT_L" runat="server" Text="時給者所定" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_JIKYUSHATIME_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>


                <!-- ■　荷卸回数　■ -->
                <a style="position:fixed;top:15.5em;left:47em; width:32em;">
                    <asp:Label ID="WF_UNLOADCNT_JKT_L" runat="server" Text="荷卸回数" Height="1.2em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　走行距離　■ -->
                <a style="position:fixed;top:14.4em;left:65.5em; width:32em;">
                    <asp:Label ID="Label111" runat="server" Text="配送" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:14.4em;left:70.5em; width:32em;">
                    <asp:Label ID="Label112" runat="server" Text="回送" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:15.5em;left:59em; width:32em;">
                    <asp:Label ID="WF_HAIDISTANCE_JKT_L" runat="server" Text="距　　　離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:15.5em;left:69.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_KAIDISTANCE_JKT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

            </span>

            <div id="divListArea2">
                <asp:panel id="pnlListArea2" runat="server" ></asp:panel>
            </div>
        </div>

        <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>     <!-- GridViewダブルクリック -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>    <!-- GridView表示位置フィールド -->
            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />         <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>        <!-- Textbox DBクリックフィールド -->
            <input id="WF_RightViewChange" runat="server" value="" type="text"/>        <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />         <!-- Rightbox 開閉 -->

            <input id="WF_BOXChange"  runat="server" value="headerbox" type="text" />   <!-- 画面(一覧・詳細)切替用フラグ -->
            <input id="WF_REP_LINECNT"  runat="server" value=""  type="text" />         <!-- Repeater 行位置 -->
            <input id="WF_REP_POSITION"  runat="server" value=""  type="text" />        <!-- Repeater 行位置 -->
            <input id="WF_REP_ROWSCNT" runat="server" value=""  type="text" />          <!-- Repeaterの１明細の行数 -->
            
            <input id="WF_DownURL" runat="server" value=""  type="text" />              <!-- Textbox Print URL -->
            <input id="WF_SELECTOR_SW" runat="server" value=""  type="text" />          <!-- Repeaterの選択値 -->
            <input id="WF_SELECTOR_Posi" runat="server" value=""  type="text" />        <!-- Repeaterの選択値 -->

            <input id="WF_SaveSX"  runat="server" value=""  type="text" />              <!-- セレクタ 変更位置X軸 -->
            <input id="WF_SaveSY"  runat="server" value=""  type="text" />              <!-- セレクタ 変更位置Y軸 -->

            <input id="WF_PrintURL" runat="server" value=""  type="text" />             <!-- Textbox Print URL -->
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->
        </div>

        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>




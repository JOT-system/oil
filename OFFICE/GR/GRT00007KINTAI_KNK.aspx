<%@ Page Title="T00007" Language="vb" AutoEventWireup="false" CodeBehind="GRT00007KINTAI_KNK.aspx.vb" Inherits="OFFICE.GRT00007KINTAI_KNK" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRT00007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="T00007H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/T00007_KNK.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/T00007_KNK.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        var EXTRALIST = '<%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>';
    </script>
</asp:Content>

<asp:Content ID="T00007" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <div class="Operation" style="margin-left:3em;margin-top:0.5em;">
            <!-- ■　ボタン　■ -->
            <a style="position:fixed;top:3.1em;left:40em;">
                <input type="button" id="WF_ButtonNIPPOEDIT" value="日報修正" runat="server" style="Width:5em" onclick="ButtonClick('WF_ButtonNIPPOEDIT');" />
            </a>
            <a style="position:fixed;top:3.1em;left:47.0em;">
                <input type="button" id="WF_ButtonNIPPO" value="日報取込" runat="server" style="Width:5em" onclick="ButtonClick('WF_ButtonNIPPO');" />
            </a>
            <a style="position:fixed;top:3.1em;left:53.5em;">
                <input type="button" id="WF_ButtonDOWN" value="前頁(更新)" runat="server" style="Width:5em" onclick="ButtonClick('WF_ButtonDOWN');" />
            </a>
            <a style="position:fixed;top:3.1em;left:58.0em;">
                <input type="button" id="WF_ButtonUP" value="次頁(更新)" runat="server" style="Width:5em" onclick="ButtonClick('WF_ButtonUP');" />
            </a>
            <a style="position:fixed;top:3.1em;left:62.5em;">
                <input type="button" id="WF_ButtonUPDATE" value="更新" runat="server" style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
            <a style="position:fixed;top:3.1em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了" runat="server" style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
        </div>

        <div style="position:fixed;top:4.55em;left:0em; right:0em; bottom:1.2em">
        <asp:MultiView ID="WF_DetailMView" runat="server">

            <!--◆◆◆◆◆◆ Tab No1　（指定日） ◆◆◆◆◆◆◆-->
            <asp:View ID="WF_DView1" runat="server" >
                <a style="position:fixed;top:3.1em;left:3em;" >
                    <asp:Label ID="WF_STATUS_LABEL" runat="server" Text="状態：" Height="1.3em" Width="4em" CssClass="WF_TEXT_LEFT" Font-Size="Medium" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_STATUS" runat="server" Text="" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT" Font-Size="Medium" Font-Bold="true" ForeColor="Red"></asp:Label>
                </a>

                <a style="position:fixed;top:3.7em;left:3em; width:32em;" hidden="hidden">
                    <asp:Label ID="WF_CAMPCODE_LABEL" runat="server" Text="会社" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false"></asp:Label>
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>

                <!-- ■　選択No　■ -->
                <a style="position:fixed;top:3.7em;left:3em; width:32em;" hidden="hidden">
                    <asp:Label ID="WF_Head_LINECNT_LABEL" runat="server" Text="選択No" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false"></asp:Label>
                    <asp:TextBox ID="WF_Head_LINECNT" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>

                <!-- ■　従業員　■ -->
                <a style="position:fixed;top:5.1em;left:3em; width:32em;" >
                    <asp:Label ID="WF_STAFFCODE_LABEL" runat="server" Text="従業員" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　配属部署　■ -->
                <a style="position:fixed;top:6.2em;left:3em; width:32em;">
                    <asp:Label ID="WF_HORG_LABEL" runat="server" Text="配属部署" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HORG" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_HORG_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　対象年月日　■ -->
                <a style="position:fixed;top:5.1em;left:18em; width:32em;" >
                    <asp:TextBox ID="WF_WORKDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="text-align: center; "></asp:TextBox>
                    <asp:Label ID="WF_LEFT_PARENTHESES_LABEL" text="（" runat="server" Height="1.2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    <asp:Label ID="WF_WORKINGWEEK_TEXT" text="" runat="server" Height="1.2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    <asp:Label ID="WF_RIGHT_PARENTHESES_LABEL" text="）" runat="server" Height="1.2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　見出し　■ -->
                <a style="position:fixed;top:7.4em;left:24.5em; width:32em;">
                    <asp:Label ID="WF_DATE_LABEL" runat="server" Text="日付" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:29em; width:32em;">
                    <asp:Label ID="WF_TIMEOFDAY_LABEL" runat="server" Text="時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:38.5em; width:32em;">
                    <asp:Label ID="WF_TIME_LABEL" runat="server" Text="時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:48.1em; width:32em;">
                    <asp:Label ID="WF_KAITENCNT_SINGLE_LABEL" runat="server" Text="回　転" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:57.6em; width:32em;">
                    <asp:Label ID="WF_KAITENCNT_TRAILER_LABEL" runat="server" Text="回　転" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:68.1em; width:32em;">
                    <asp:Label ID="WF_OVERTIME_LABEL" runat="server" Text="残業" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:71.9em; width:32em;">
                    <asp:Label ID="WF_MIDNIGHT_LABEL" runat="server" Text="深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <!-- ■　休日区分　■ -->
                <a style="position:fixed;top:8.5em;left:3em; width:32em;">
                    <asp:Label ID="WF_HOLIDAYKBN_LABEL" runat="server" Text="休日区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HOLIDAYKBN" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" Enabled="false" onchange="ItemChange('WF_HOLIDAYKBN')"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_HOLIDAYKBN_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　勤怠区分　■ -->
                <a style="position:fixed;top:9.6em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_PAYKBN' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                    <asp:Label ID="WF_PAYKBN_LABEL" runat="server" Text="勤怠区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="true"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_PAYKBN" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_PAYKBN')"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_PAYKBN_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　宿直区分　■ -->
                <a style="position:fixed;top:10.7em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_SHUKCHOKKBN' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                    <asp:Label ID="WF_SHUKCHOKKBN_LABEL" runat="server" Text="宿直区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="true"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKKBN" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHUKCHOKKBN')"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_SHUKCHOKKBN_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　出社日　■ -->
                <a style="position:fixed;top:8.5em;left:18em; width:32em;">
                    <asp:Label ID="WF_STDATE_LABEL" runat="server" Text="出社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_STDATE')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　出社時間　■ -->
                <a style="position:fixed;top:8.5em;left:28.1em; width:32em;">
                    <asp:TextBox ID="WF_STTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_STTIME')" style="text-align: center; "></asp:TextBox>
                </a>
                <!-- ■　拘束開始　■ -->
                <a style="position:fixed;top:9.6em;left:18em; width:32em;">
                    <asp:Label ID="WF_BINDSTDATE_LABEL" runat="server" Text="拘束開始" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:9.6em;left:28.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDSTDATE" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_BINDSTDATE')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社日　■ -->
                <a style="position:fixed;top:10.7em;left:18em; width:32em;">
                    <asp:Label ID="WF_ENDDATE_LABEL" runat="server" Text="退社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ENDDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_ENDDATE')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社時間　■ -->
                <a style="position:fixed;top:10.7em;left:28.1em; width:32em;">
                    <asp:TextBox ID="WF_ENDTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_ENDTIME')" style="text-align: center; "></asp:TextBox>
                </a>
                <!-- ■　所定内計　■ -->
                <a style="position:fixed;top:11.8em;left:18em; width:32em;">
                    <asp:Label ID="WF_WWORKTIME_LABEL" runat="server" Text="所定内計" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:11.8em;left:28.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_WWORKTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false"  style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　乗務日計　■ -->
                <a style="position:fixed;top:12.9em;left:18em; width:32em;">
                    <asp:Label ID="WF_JYOMUTIME_LABEL" runat="server" Text="乗務日計" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:12.9em;left:28.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_JYOMUTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日報休憩　■ -->
                <a style="position:fixed;top:8.5em;left:33em; width:32em;">
                    <asp:Label ID="WF_BREAKTIME_L" runat="server" Text="日報休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NIPPOBREAKTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休憩　■ -->
                <a style="position:fixed;top:9.6em;left:33em; width:32em;">
                    <asp:Label ID="WF_BREAKTIME_LABEL" runat="server" Text="休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_BREAKTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_BREAKTIME')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　特作Ｉ　■ -->
                <a style="position:fixed;top:10.7em;left:33em; width:32em;">
                    <asp:Label ID="WF_TOKUSA1TIME_LABEL" runat="server" Text="特作Ⅰ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_TOKUSA1TIME')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　荷卸回数　■ -->
                <a style="position:fixed;top:14em;left:33em; width:32em;">
                    <asp:Label ID="WF_UNLOADCNT_LABEL" runat="server" Text="届　　数" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　走行距離　■ -->
                <a style="position:fixed;top:14em;left:43em; width:32em;">
                    <asp:Label ID="WF_SOUDISTANCE_L" runat="server" Text="配送距離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14em;left:52.5em; width:32em;">
                    <asp:Label ID="WF_KAIDISTANCE_L" runat="server" Text="回送距離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAIDISTANCE" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　平日残業　■ -->
                <a style="position:fixed;top:8.5em;left:62.6em; width:32em;">
                    <asp:Label ID="WF_ORVERTIME_LABEL" runat="server" Text="平 　 　 日" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　平日深夜　■ -->
                <a style="position:fixed;top:8.5em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日出勤　■ -->
                <a style="position:fixed;top:9.6em;left:62.6em; width:32em;">
                    <asp:Label ID="WF_HWORKTIME_LABEL" runat="server" Text="休日出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日深夜　■ -->
                <a style="position:fixed;top:9.6em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　代休出勤　■ -->
                <a style="position:fixed;top:10.7em;left:62.6em; width:32em;">
                    <asp:Label ID="WF_HDAIWORKTIME_LABEL" runat="server" Text="代休出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HDAIWORKTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　代休深夜　■ -->
                <a style="position:fixed;top:10.7em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_HDAINIGHTTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜出勤　■ -->
                <a style="position:fixed;top:11.8em;left:62.6em; width:32em;">
                    <asp:Label ID="WF_SWORKTIME_LABEL" runat="server" Text="日曜出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜深夜　■ -->
                <a style="position:fixed;top:11.8em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜代休出勤　■ -->
                <a style="position:fixed;top:12.9em;left:62.6em; width:32em;">
                    <asp:Label ID="WF_SDAIWORKTIME_LABEL" runat="server" Text="日曜代休" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SDAIWORKTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜代休深夜　■ -->
                <a style="position:fixed;top:12.9em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_SDAINIGHTTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　所定深夜　■ -->
                <a style="position:fixed;top:14.0em;left:62.6em; width:32em;">
                    <asp:Label ID="WF_NIGHTTIME_LABEL" runat="server" Text="所定深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:14.0em;left:71em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_NIGHTTIME" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　回転　■ -->
                <a style="position:fixed;top:8.5em;left:49em; width:32em;" hidden="hidden">
                    <asp:Label ID="WF_KAITENCNT_LABEL" runat="server" Text="回　　転" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■　油種別（卸回数、走行距離）　■ -->
                <a style="position:fixed;top:8.5em;left:43em; width:32em;">
                    <asp:Label ID="WF_OIL_WHITE1_EACH_LABEL" runat="server" Text="白油単車" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_WHITE1_EACH" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" style="text-align: right; " onchange="ItemChange('WF_KAITENCNT_WHITE1_EACH')"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:9.6em;left:43em; width:32em;">
                    <asp:Label ID="WF_OIL_BLACK1_EACH_LABEL" runat="server" Text="黒油単車" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_BLACK1_EACH" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" style="text-align: right; " onchange="ItemChange('WF_KAITENCNT_BLACK1_EACH')"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.7em;left:43em; width:32em;">
                    <asp:Label ID="WF_LPG1_EACH_LABEL" runat="server" Text="LPG単車" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_LPG1_EACH" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" style="text-align: right; " onchange="ItemChange('WF_KAITENCNT_LPG1_EACH')"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.8em;left:43em; width:32em;">
                    <asp:Label ID="WF_LNG1_EACH_LABEL" runat="server" Text="LNG単車" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_LNG1_EACH" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" style="text-align: right; " onchange="ItemChange('WF_KAITENCNT_LNG1_EACH')"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.5em;left:52.5em; width:32em;">
                    <asp:Label ID="WF_TRAILER_WHITE2_EACH_LABEL" runat="server" Text="白油ﾄﾚｰﾗ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_WHITE2_EACH" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" style="text-align: right; " onchange="ItemChange('WF_KAITENCNT_WHITE2_EACH')"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:9.6em;left:52.5em; width:32em;">
                    <asp:Label ID="WF_TRAILER_BLACK2_EACH_LABEL" runat="server" Text="黒油ﾄﾚｰﾗ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_BLACK2_EACH" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" style="text-align: right; " onchange="ItemChange('WF_KAITENCNT_BLACK2_EACH')"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.7em;left:52.5em; width:32em;">
                    <asp:Label ID="WF_LPG2_EACH_LABEL" runat="server" Text="LPGﾄﾚｰﾗ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_LPG2_EACH" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" style="text-align: right; " onchange="ItemChange('WF_KAITENCNT_LPG2_EACH')"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.8em;left:52.5em; width:32em;">
                    <asp:Label ID="WF_LNG2_EACH_LABEL" runat="server" Text="LNGﾄﾚｰﾗ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_LNG2_EACH" runat="server" Height="1.2em" Width="4em" CssClass="WF_TEXTBOX_CSS" enabled="true" style="text-align: right; " onchange="ItemChange('WF_KAITENCNT_LNG2_EACH')"></asp:TextBox>
                    </b>
                </a>

                <!-- 全体レイアウト　detailbox -->
                <div class="detailbox" id="detailbox">
                    <div style="height:1em;"></div>
                    <div id="divListArea">
                        <asp:panel id="pnlListArea" runat="server"></asp:panel>
                    </div>
                </div>
            </asp:View>

            <!--◆◆◆◆◆◆ Tab No2　（月調整） ◆◆◆◆◆◆◆-->
                <asp:View ID="WF_DView2" runat="server" >
                    <!-- ■■■　１行目　■■■ -->
                    <!-- ■　従業員　■ -->
                    <a style="position:fixed;top:5.1em;left:3em; width:32em;" >
                        <asp:Label ID="WF_STAFFCODETTL_LABEL" runat="server" Text="従業員" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_STAFFCODETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                        </b>
                        <asp:Label ID="WF_STAFFCODETTL_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■■■　２行目　■■■ -->
                    <!-- ■　所属部署　■ -->
                    <a style="position:fixed;top:6.3em;left:3em; width:32em;" >
                        <asp:Label ID="WF_HORGTTL_LABEL" runat="server" Text="配属部署" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_HORGTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                        </b>
                        <asp:Label ID="WF_HORGTTL_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■■■　勤怠区分関連１　■■■ -->
                    <a style="position:fixed;top:7.8em;left:3em; width:32em;" >
                        <asp:Label ID="WF_WORKNISSUTTL_LABEL" runat="server" Text="所 　 　 労" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_WORKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:7.8em;left:14em; width:32em;" >
                        <asp:Label ID="WF_SHOUKETUNISSUTTL_LABEL" runat="server" Text="傷 　 　 欠" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SHOUKETUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:7.8em;left:25em; width:32em;" >
                        <asp:Label ID="WF_NENKYUNISSUTTL_LABEL" runat="server" Text="年 　 　 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_NENKYUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　勤怠区分関連２　■■■ -->
                    <a style="position:fixed;top:8.9em;left:3em; width:32em;" >
                        <asp:Label ID="WF_CHIKOKSOTAINISSUTTL_LABEL" runat="server" Text="遅 　 　 早" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_CHIKOKSOTAINISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:8.9em;left:14em; width:32em;" >
                        <asp:Label ID="WF_KUMIKETUNISSUTTL_LABEL" runat="server" Text="組 　 　 欠" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KUMIKETUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:8.9em;left:25em; width:32em;" >
                        <asp:Label ID="WF_TOKUKYUNISSUTTL_LABEL" runat="server" Text="特 　 　 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_TOKUKYUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　勤怠区分関連３　■■■ -->
                    <a style="position:fixed;top:10.0em;left:14em; width:32em;" >
                        <asp:Label ID="WF_ETCKETUNISSUTTL_LABEL" runat="server" Text="他 　 　 欠" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_ETCKETUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:10.0em;left:25em; width:32em;" >
                        <asp:Label ID="WF_STOCKNISSUTTL_LABEL" runat="server" Text="ｽﾄｯｸ休暇" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_STOCKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　勤怠区分関連４　■■■ -->
                    <a style="position:fixed;top:11.1em;left:25em; width:32em;" >
                        <asp:Label ID="WF_KYOTEIWEEKNISSUTTL_LABEL" runat="server" Text="協 約 週 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KYOTEIWEEKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <!-- ■■■　勤怠区分関連５　■■■ -->
                    <a style="position:fixed;top:12.2em;left:25em; width:32em;" >
                        <asp:Label ID="WF_WEEKNISSUTTL_LABEL" runat="server" Text="週 　 　 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_WEEKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <!-- ■■■　勤怠区分関連６　■■■ -->
                    <a style="position:fixed;top:13.3em;left:25em; width:32em;" >
                        <asp:Label ID="WF_DAIKYUNISSUTTL_LABEL" runat="server" Text="代 　 　 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_DAIKYUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　手当関連１　■■■ -->
                    <a style="position:fixed;top:15.5em;left:3em; width:32em;" >
                        <asp:Label ID="WF_NENSHINISSUTTL_LABEL" runat="server" Text="年始出勤日数" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_NENSHINISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:15.5em;left:14em; width:32em;" >
                        <asp:Label ID="WF_SHUKCHOKNNISSUTTL_LABEL" runat="server" Text="宿日直年始" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SHUKCHOKNNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　手当関連２　■■■ -->
                    <a style="position:fixed;top:16.6em;left:3em; width:32em;" >
                        <asp:Label ID="WF_HWORKNISSUTTL_LABEL" runat="server" Text="休日出勤日数" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_HWORKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:16.6em;left:14em; width:32em;" >
                        <asp:Label ID="WF_SHUKCHOKNISSUTTL_LABEL" runat="server" Text="宿日直通常" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SHUKCHOKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　残業関連１　■■■ -->
                    <a style="position:fixed;top:6.7em;left:43.2em; width:32em;" >
                        <asp:Label ID="WF_ORVERTIMETTL_LABEL" runat="server" Text="残　業" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:6.7em;left:48.5em; width:32em;" >
                        <asp:Label ID="WF_WNIGHTTIMETTL_LABEL" runat="server" Text="深　夜" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:7.8em;left:37em; width:32em;" >
                        <asp:Label ID="WF_WEEKDAY_LABEL" runat="server" Text="平 　 　 日" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_ORVERTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_WNIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:8.9em;left:37em; width:32em;" >
                        <asp:Label ID="WF_HWORKTIMETTL_LABEL" runat="server" Text="休 日 出 勤" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_HWORKTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HNIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:10.0em;left:37em; width:32em;" >
                        <asp:Label ID="WF_HDAIWORKTIMETTL_LABEL" runat="server" Text="代 休 出 勤" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_HDAIWORKTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HDAINIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:11.1em;left:37em; width:32em;" >
                        <asp:Label ID="WF_SWORKTIMETTL_LABEL" runat="server" Text="日 曜 出 勤" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SWORKTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_SNIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:12.2em;left:37em; width:32em;" >
                        <asp:Label ID="WF_SDAIWORKTIMETTL_LABEL" runat="server" Text="日 曜 代 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SDAIWORKTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_SDAINIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:13.3em;left:37em; width:32em;" >
                        <asp:Label ID="WF_NIGHTTIMETTL_LABEL" runat="server" Text="所 定 深 夜" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:13.3em;left:47.5em; width:32em;" >
                        <b>
                        <asp:TextBox ID="WF_NIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>

                    <a style="position:fixed;top:14.4em;left:37em; width:32em;" >
                        <asp:Label ID="WF_TOKUSA1TIMETTL_LABEL" runat="server" Text="特 作 Ｉ" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_TOKUSA1TIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>

                    <a style="position:fixed;top:15.5em;left:37em; width:32em;" >
                        <asp:Label ID="WF_WWORKTIMETTL_LABEL" runat="server" Text="所定内計" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_WWORKTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>

                    <a style="position:fixed;top:16.6em;left:37em; width:32em;" >
                        <asp:Label ID="WF_JYOMUTIMETTL_LABEL" runat="server" Text="乗務日計" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_JYOMUTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　油種別（卸回数、走行距離）　■■■ -->
                    <a style="position:fixed;top:6.7em;left:61em; width:32em;" >
                        <asp:Label ID="WF_KAITENCNT_WHITE1_LABEL" runat="server" Text="回　転" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:6.7em;left:66em; width:32em;" >
                        <asp:Label ID="WF_HAIDISTANCE_WHITE1_LABEL" runat="server" Text="走行㎞" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>

                    <a style="position:fixed;top:7.8em;left:55em; width:32em;" >
                        <asp:Label ID="WF_OIL_WHITE1_LABEL" runat="server" Text="白油単車" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KAITENCNT_WHITE1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HAIDISTANCE_WHITE1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:8.9em;left:55em; width:32em;" >
                        <asp:Label ID="WF_OIL_BLACK1_LABEL" runat="server" Text="黒油単車" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KAITENCNT_BLACK1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HAIDISTANCE_BLACK1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:10.0em;left:55em; width:32em;" >
                        <asp:Label ID="WF_LPG1_LABEL" runat="server" Text="LPG単車" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KAITENCNT_LPG1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HAIDISTANCE_LPG1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:11.1em;left:55em; width:32em;" >
                        <asp:Label ID="WF_LNG1_LABEL" runat="server" Text="LNG単車" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KAITENCNT_LNG1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HAIDISTANCE_LNG1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:12.2em;left:55em; width:32em;" >
                        <asp:Label ID="WF_TRAILER_WHITE2_LABEL" runat="server" Text="白油ﾄﾚｰﾗ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KAITENCNT_WHITE2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HAIDISTANCE_WHITE2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:13.3em;left:55em; width:32em;" >
                        <asp:Label ID="WF_TRAILER_BLACK2_LABEL" runat="server" Text="黒油ﾄﾚｰﾗ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KAITENCNT_BLACK2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HAIDISTANCE_BLACK2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:14.4em;left:55em; width:32em;" >
                        <asp:Label ID="WF_LPG2_LABEL" runat="server" Text="LPGﾄﾚｰﾗ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KAITENCNT_LPG2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HAIDISTANCE_LPG2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:15.5em;left:55em; width:32em;" >
                        <asp:Label ID="WF_LNG2_LABEL" runat="server" Text="LNGﾄﾚｰﾗ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KAITENCNT_LNG2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HAIDISTANCE_LNG2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:17.0em;left:55em; width:32em;" >
                        <asp:Label ID="WF_SUM_LABEL" runat="server" Text="合計" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KAITENCNTTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="text-align: right; "></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HAIDISTANCETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                </asp:View>

            </asp:MultiView>
        </div>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>      <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>     <!-- GridView表示位置フィールド -->

        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->

        <input id="WF_NIPPObtn" runat="server" value=""  type="text" />             <!-- 日報ボタン制御 -->
        <input id="WF_DTABChange" runat="server" value="" type="text"/>             <!-- DetailBox Mview切替 -->

        <input id="WF_DISP_SaveX" runat="server" value="" type="text" />            <!-- 明細位置X軸 -->
        <input id="WF_DISP_SaveY" runat="server" value="" type="text" />            <!-- 明細位置Y軸 -->
        <input id="WF_SelectLine" runat="server" value="" type="text" />            <!-- リスト変更行数 -->

        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />       <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->

        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->

        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />         <!-- 権限 -->

        <input id="WF_KAITEN" runat="server" value="" type="text" />                <!-- 回転数 -->
        <input id="WF_HAIDIS" runat="server" value="" type="text" />                <!-- 距離 -->

    </div>
</asp:Content>

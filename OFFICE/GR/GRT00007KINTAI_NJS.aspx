<%@ Page Title="T00007" Language="vb" AutoEventWireup="false" CodeBehind="GRT00007KINTAI_NJS.aspx.vb" Inherits="OFFICE.GRT00007KINTAI_NJS" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRT00007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="T00007H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/T00007_NJS.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/T00007_NJS.js")%>'></script>
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
            <a style="position:fixed;top:3.1em;left:33em;">
                <input type="button" id="WF_ButtonBREAKTIME" value="休憩不足分" runat="server" style="Width:6em" onclick="ButtonClick('WF_ButtonBREAKTIME');" />
            </a>
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
                    <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　配属部署　■ -->
                <a style="position:fixed;top:6.2em;left:3em; width:32em;">
                    <asp:Label ID="WF_HORG_LABEL" runat="server" Text="配属部署" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HORG" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_HORG_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　対象年月日　■ -->
                <a style="position:fixed;top:5.1em;left:20em; width:32em;" >
                    <asp:TextBox ID="WF_WORKDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="text-align: center; "></asp:TextBox>
                    <asp:Label ID="WF_LEFT_PARENTHESES_LABEL" text="（" runat="server" Height="1.2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    <asp:Label ID="WF_WORKINGWEEK_TEXT" text="" runat="server" Height="1.2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    <asp:Label ID="WF_RIGHT_PARENTHESES_LABEL" text="）" runat="server" Height="1.2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　見出し　■ -->
                <a style="position:fixed;top:7.4em;left:26em; width:32em;">
                    <asp:Label ID="WF_DATE_LABEL" runat="server" Text="日付" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:32em; width:32em;">
                    <asp:Label ID="WF_TIMEOFDAY_LABEL" runat="server" Text="時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:43em; width:32em;">
                    <asp:Label ID="WF_TIME_LABEL" runat="server" Text="時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:53.5em; width:32em;">
                    <b>
                    <asp:CheckBox ID="WF_MODIFY" runat="server" Height="1.2em" Width="1em"  enabled="false"></asp:CheckBox>
                    </b>
                    <asp:Label ID="WF_MODELDISTANCE_LABEL" runat="server" Text="ﾓﾃﾞﾙ距離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:67em; width:32em;">
                    <asp:Label ID="WF_OVERTIME_LABEL" runat="server" Text="残業" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.4em;left:73em; width:32em;">
                    <asp:Label ID="WF_MIDNIGHT_LABEL" runat="server" Text="深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <!-- ■　休日区分　■ -->
                <a style="position:fixed;top:8.5em;left:3em; width:32em;">
                    <asp:Label ID="WF_HOLIDAYKBN_LABEL" runat="server" Text="休日区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HOLIDAYKBN" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false" onchange="ItemChange('WF_HOLIDAYKBN')"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_HOLIDAYKBN_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　勤怠区分　■ -->
                <a style="position:fixed;top:9.6em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_PAYKBN' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                    <asp:Label ID="WF_PAYKBN_LABEL" runat="server" Text="勤怠区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="true"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_PAYKBN" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_PAYKBN')"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_PAYKBN_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　宿直区分　■ -->
                <a style="position:fixed;top:10.7em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_SHUKCHOKKBN' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                    <asp:Label ID="WF_SHUKCHOKKBN_LABEL" runat="server" Text="宿直区分" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="true"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKKBN" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHUKCHOKKBN')"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_SHUKCHOKKBN_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　出社日　■ -->
                <a style="position:fixed;top:8.5em;left:20em; width:32em;">
                    <asp:Label ID="WF_STDATE_LABEL" runat="server" Text="出社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_STDATE')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　出社時間　■ -->
                <a style="position:fixed;top:8.5em;left:30.1em; width:32em;">
                    <asp:TextBox ID="WF_STTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_STTIME')" style="text-align: center; "></asp:TextBox>
                </a>
                <!-- ■　拘束開始　■ -->
                <a style="position:fixed;top:9.6em;left:20em; width:32em;">
                    <asp:Label ID="WF_BINDSTDATE_LABEL" runat="server" Text="拘束開始" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:9.6em;left:30.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDSTDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_BINDSTDATE')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　拘束時間　■ -->
                <a style="position:fixed;top:10.7em;left:20em; width:32em;">
                    <asp:Label ID="WF_BINDTIME_LABEL" runat="server" Text="拘束時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:10.7em;left:30.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_BINDTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_BINDTIME')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社日　■ -->
                <a style="position:fixed;top:11.8em;left:20em; width:32em;">
                    <asp:Label ID="WF_ENDDATE_LABEL" runat="server" Text="退社時刻" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ENDDATE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_ENDDATE')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　退社時間　■ -->
                <a style="position:fixed;top:11.8em;left:30.1em; width:32em;">
                    <asp:TextBox ID="WF_ENDTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_ENDTIME')" style="text-align: center; "></asp:TextBox>
                </a>
                <!-- ■　日報休憩　■ -->
                <a style="position:fixed;top:8.5em;left:37em; width:32em;">
                    <asp:Label ID="WF_BREAKTIME_L" runat="server" Text="日報休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NIPPOBREAKTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休憩　■ -->
                <a style="position:fixed;top:9.6em;left:37em; width:32em;">
                    <asp:Label ID="WF_BREAKTIME_LABEL" runat="server" Text="休憩" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_BREAKTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_BREAKTIME')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　特作Ｉ　■ -->
                <a style="position:fixed;top:10.7em;left:37em; width:32em;">
                    <asp:Label ID="WF_TOKUSA1TIME_LABEL" runat="server" Text="特作" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" onchange="ItemChange('WF_TOKUSA1TIME')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　配送時間　■ -->
                <a style="position:fixed;top:11.8em;left:37em; width:32em;">
                    <asp:Label ID="WF_HAISOTIME_LABEL" runat="server" Text="配送時間" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAISOTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" onchange="ItemChange('WF_HAISOTIME')" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　車中伯　■ -->
                <a style="position:fixed;top:12.9em;left:37em; width:32em;">
                    <asp:Label ID="WF_SHACHUHAKKBN_LABEL" runat="server" Text="車中泊" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:CheckBox ID="WF_SHACHUHAKKBN" runat="server" Height="1.2em" Width="6em"  enabled="true"></asp:CheckBox>
                    </b>
                </a>
                <!-- ■　単車ラテックス　■ -->
                <a style="position:fixed;top:8.5em;left:49em; width:32em;">
                    <asp:Label ID="WF_MODELDISTANCE0109_LABEL" runat="server" Text="単・ﾗﾃｯｸｽ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE0109" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" readonly="true" ondblclick="DtabChange('2')" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　ﾄﾚｰﾗラテックス　■ -->
                <a style="position:fixed;top:9.6em;left:49em; width:32em;">
                    <asp:Label ID="WF_MODELDISTANCE0209_LABEL" runat="server" Text="ﾄﾚ・ﾗﾃｯｸｽ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE0209" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" readonly="true" ondblclick="DtabChange('2')" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　ﾄﾚｰﾗラLNG　■ -->
                <a style="position:fixed;top:10.7em;left:49em; width:32em;">
                    <asp:Label ID="WF_MODELDISTANCE0204_LABEL" runat="server" Text="ﾄﾚ・LNG" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE0204" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="true" readonly="true" ondblclick="DtabChange('2')" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　平日残業　■ -->
                <a style="position:fixed;top:8.5em;left:61em; width:32em;">
                    <asp:Label ID="WF_ORVERTIME_LABEL" runat="server" Text="平 　 　 日" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　平日深夜　■ -->
                <a style="position:fixed;top:8.5em;left:71.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日出勤　■ -->
                <a style="position:fixed;top:9.6em;left:61em; width:32em;">
                    <asp:Label ID="WF_HWORKTIME_LABEL" runat="server" Text="休日出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　休日深夜　■ -->
                <a style="position:fixed;top:9.6em;left:71.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜出勤　■ -->
                <a style="position:fixed;top:10.7em;left:61em; width:32em;">
                    <asp:Label ID="WF_SWORKTIME_LABEL" runat="server" Text="日曜出勤" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　日曜深夜　■ -->
                <a style="position:fixed;top:10.7em;left:71.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　所定深夜　■ -->
                <a style="position:fixed;top:11.8em;left:61em; width:32em;">
                    <asp:Label ID="WF_NIGHTTIME_LABEL" runat="server" Text="所定深夜" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:11.8em;left:71.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_NIGHTTIME" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■　走行距離　■ -->
                <a style="position:fixed;top:13.4em;left:67em; width:32em;">
                    <asp:Label ID="WF_RUN_LABEL" runat="server" Text="走行" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:13.4em;left:73em; width:32em;">
                    <asp:Label ID="WF_FORWARDING_LABEL" runat="server" Text="回送" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:14.5em;left:61em; width:32em;">
                    <asp:Label ID="WF_SOUDISTANCE_LABEL" runat="server" Text="距離" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14.5em;left:71.1em; width:32em;">
                    <b>
                    <asp:TextBox ID="WF_KAIDISTANCE" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" style="text-align: right; "></asp:TextBox>
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
                        <asp:Label ID="WF_NENKYUNISSUTTL_LABEL" runat="server" Text="年 　 　 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_NENKYUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:7.8em;left:25em; width:32em;" >
                        <asp:Label ID="WF_KYOTEIWEEKNISSUTTL_LABEL" runat="server" Text="協 約 週 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KYOTEIWEEKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　勤怠区分関連２　■■■ -->
                    <a style="position:fixed;top:8.9em;left:3em; width:32em;" >
                        <asp:Label ID="WF_SHOUKETUNISSUTTL_LABEL" runat="server" Text="傷 　 　 欠" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SHOUKETUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:8.9em;left:14em; width:32em;" >
                        <asp:Label ID="WF_TOKUKYUNISSUTTL_LABEL" runat="server" Text="特 　 　 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_TOKUKYUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:8.9em;left:25em; width:32em;" >
                        <asp:Label ID="WF_WEEKNISSUTTL_LABEL" runat="server" Text="週 　 　 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_WEEKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　勤怠区分関連３　■■■ -->
                    <a style="position:fixed;top:10.0em;left:3em; width:32em;" >
                        <asp:Label ID="WF_KUMIKETUNISSUTTL_LABEL" runat="server" Text="組 　 　 欠" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KUMIKETUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:10.0em;left:14em; width:32em;" >
                        <asp:Label ID="WF_CHIKOKSOTAINISSUTTL_LABEL" runat="server" Text="遅 　 　 早" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_CHIKOKSOTAINISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:10.0em;left:25em; width:32em;" >
                        <asp:Label ID="WF_DAIKYUNISSUTTL_LABEL" runat="server" Text="代 　 　 休" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_DAIKYUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　勤怠区分関連４　■■■ -->
                    <a style="position:fixed;top:11.1em;left:3em; width:32em;" >
                        <asp:Label ID="WF_ETCKETUNISSUTTL_LABEL" runat="server" Text="他 　 　 欠" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_ETCKETUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:11.1em;left:14em; width:32em;" >
                        <asp:Label ID="WF_STOCKNISSUTTL_LABEL" runat="server" Text="ｽﾄｯｸ休暇" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_STOCKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　手当関連１　■■■ -->
                    <a style="position:fixed;top:13.3em;left:3em; width:32em;" >
                        <asp:Label ID="WF_NENMATUNISSUTTL_LABEL" runat="server" Text="年末出勤日数" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_NENMATUNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:14.4em;left:3em; width:32em;" >
                        <asp:Label ID="WF_NENSHINISSUTTL_LABEL" runat="server" Text="年始出勤日数" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_NENSHINISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:15.5em;left:3em; width:32em;" >
                        <asp:Label ID="WF_SHACHUHAKNISSUTTL_LABEL" runat="server" Text="車中泊日数" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SHACHUHAKNISSUTTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■■■　残業関連１　■■■ -->
                    <a style="position:fixed;top:20.0em;left:8.5em; width:32em;" >
                        <asp:Label ID="WF_OVERTIME1_LABEL" runat="server" Text="残　業" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:20.0em;left:14em; width:32em;" >
                        <asp:Label ID="WF_MIDNIGHT1_LABEL" runat="server" Text="深　夜" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:21.1em;left:3em; width:32em;" >
                        <asp:Label ID="WF_WEEKDAY1_LABEL" runat="server" Text="平 　 　 日" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_ORVERTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_WNIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:22.2em;left:3em; width:32em;" >
                        <asp:Label ID="WF_HWORKTIMETTL_LABEL" runat="server" Text="休 日 出 勤" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_HWORKTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_HNIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:23.3em;left:3em; width:32em;" >
                        <asp:Label ID="WF_SWORKTIMETTL_LABEL" runat="server" Text="日 曜 出 勤" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SWORKTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                        <b>
                        <asp:TextBox ID="WF_SNIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:24.4em;left:3em; width:32em;" >
                        <asp:Label ID="WF_NIGHTTIMETTL_LABEL" runat="server" Text="所 定 深 夜" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:24.4em;left:13.53em; width:32em;" >
                        <b>
                        <asp:TextBox ID="WF_NIGHTTIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>

                    <a style="position:fixed;top:21.1em;left:25em; width:32em;" >
                        <asp:Label ID="WF_TOKUSA1TIMETTL_LABEL" runat="server" Text="特 作" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_TOKUSA1TIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:22.2em;left:25em; width:32em;" >
                        <asp:Label ID="WF_JIKYUSHATIMETTLL_LABEL" runat="server" Text="時給者作業" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_JIKYUSHATIMETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                        </b>
                    </a>



                    <!-- ■■■　油種別（卸回数、走行距離）　■■■ -->
                    <a style="position:fixed;top:6.7em;left:42em; width:32em;" >
                        <asp:Label ID="WF_MOTORCYCLE_LABEL" runat="server" Text="【単車】" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:6.7em;left:47em; width:32em;" >
                        <asp:Label ID="WF_MODELDISTANCE1_LABEL" runat="server" Text="ﾓﾃﾞﾙ距離" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:6.7em;left:55em; width:32em;" >
                        <asp:Label ID="WF_TRAILER_LABEL" runat="server" Text="【ﾄﾚｰﾗ】" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:6.7em;left:60em; width:32em;" >
                        <asp:Label ID="WF_MODELDISTANCE2_LABEL" runat="server" Text="ﾓﾃﾞﾙ距離" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>

                    <a style="position:fixed;top:7.8em;left:42em; width:32em;" >
                        <asp:Label ID="WF_MODELDISTANCE_RATE1_LABEL" runat="server" Text="ﾗﾃｯｸｽ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_MODELDISTANCE_RATE1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:7.8em;left:55em; width:32em;" >
                        <asp:Label ID="WF_MODELDISTANCE_RATE2_LABEL" runat="server" Text="ﾗﾃｯｸｽ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_MODELDISTANCE_RATE2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:8.9em;left:42em; width:32em;" hidden="hidden">
                        <asp:Label ID="WF_MODELDISTANCE_LNG1_LABEL" runat="server" Text="ＬＮＧ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_MODELDISTANCE_LNG1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:8.9em;left:55em; width:32em;" >
                        <asp:Label ID="WF_MODELDISTANCE_LNG2_LABEL" runat="server" Text="ＬＮＧ" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_MODELDISTANCE_LNG2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " OnChange="TTL_SUM()"></asp:TextBox>
                        </b>
                    </a>
                    <a style="position:fixed;top:11.1em;left:55em; width:32em;" >
                        <asp:Label ID="WF_MODELDISTANCETTL_LABEL" runat="server" Text="合計" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_MODELDISTANCETTL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="text-align: right; "></asp:TextBox>
                        </b>
                    </a>

                </asp:View>

                <!--◆◆◆◆◆◆ Tab No3　（モデル距離） ◆◆◆◆◆◆◆-->
                <asp:View ID="WF_DView3" runat="server" >
                    <a style="position:fixed;top:3.1em;left:52.5em;">
                        <input type="button" id="WF_ButtonRESET" value="モデル再取得"  style="Width:10em" onclick="ButtonClick('WF_ButtonRESET');" />
                    </a>
                    <a style="position:fixed;top:3.1em;left:62.5em;">
                        <input type="button" id="WF_ButtonUPDATEMDL" value="更新"  style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATEMDL');" />
                    </a>
                    <a style="position:fixed;top:3.1em;left:67em;">
                        <input type="button" id="WF_ButtonENDMDL" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonENDMDL');" />
                    </a>
                    <!-- ■■■　１行目　■■■ -->
                    <!-- ■　従業員　■ -->
                    <a style="position:fixed;top:5.1em;left:3em; width:32em;" >
                        <asp:Label ID="WF_STAFFCODEMDL_LABEL" runat="server" Text="従業員" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_STAFFCODEMDL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                        </b>
                        <asp:Label ID="WF_STAFFCODEMDL_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■■■　２行目　■■■ -->
                    <!-- ■　所属部署　■ -->
                    <a style="position:fixed;top:6.2em;left:3em; width:32em;" >
                        <asp:Label ID="WF_HORGMDL_LABEL" runat="server" Text="配属部署" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_HORGMDL" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                        </b>
                        <asp:Label ID="WF_HORGMDL_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <a style="position:fixed;top:5.1em;left:20em; width:32em;" >
                        <asp:TextBox ID="WF_WORKDATE2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="text-align: center; "></asp:TextBox>
                        <asp:Label ID="WF_LEFT_PARENTHESES2_LABEL" text="（" runat="server" Height="1.2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        <asp:Label ID="WF_WORKINGWEEK2_TEXT" text="" runat="server" Height="1.2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        <asp:Label ID="WF_RIGHT_PARENTHESES2_LABEL" text="）" runat="server" Height="1.2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■■■　勤怠区分関連１　■■■ -->
                    <a style="position:fixed;top:7.8em;left:3em; width:32em;" >
                        <asp:Label ID="WF_NO_LABEL" runat="server" Text="№" Height="1.2em" Width="2em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:7.8em;left:5em; width:32em;" >
                        <asp:Label ID="WF_MODELCAR_LABEL" runat="server" Text="単車・ﾄﾚｰﾗ" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:7.8em;left:12em; width:32em;" >
                        <asp:Label ID="WF_OILTYPE_LABEL" runat="server" Text="油種" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:7.8em;left:19.5em; width:32em;" >
                        <asp:Label ID="WF_SHIPPINGLOCATION_LABEL" runat="server" Text="出荷場所" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:7.8em;left:37em; width:32em;" >
                        <asp:Label ID="WF_SHIPPINGADDRESS_LABEL" runat="server" Text="届　　先" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:7.8em;left:54.5em; width:32em;" >
                        <asp:Label ID="WF_MODELDISTANCE3_LABEL" runat="server" Text="モデル距離" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:7.8em;left:60em; width:32em;" >
                        <asp:Label ID="WF_FIX_LABEL" runat="server" Text="修正" Height="1.2em" Width="5.3em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    </a>
                    <a style="position:fixed;top:8.9em;left:3em; width:100em;" >
                        <b>
                        <asp:Label ID="WF_ONE_LABEL" runat="server" Text="１．" Height="1.2em" Width="2em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        </b>
                    </a>
                    <a style="position:fixed;top:8.9em;left:5em; width:32em;" ondblclick="Field_DBclick('WF_SHARYOKBN1' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_SHARYOKBN1" runat="server" Height="1.2em" Width="2em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHARYOKBN1')"></asp:TextBox>
                        <asp:Label ID="WF_SHARYOKBN1_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:8.9em;left:12em; width:32em;" ondblclick="Field_DBclick('WF_OILPAYKBN1' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_OILPAYKBN1" runat="server" Height="1.2em" Width="3em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_OILPAYKBN1')"></asp:TextBox>
                        <asp:Label ID="WF_OILPAYKBN1_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:8.9em;left:19.5em; width:32em;" ondblclick="Field_DBclick('WF_SHUKABASHO1' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)"  >
                        <asp:TextBox ID="WF_SHUKABASHO1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHUKABASHO1')"></asp:TextBox>
                        <asp:Label ID="WF_SHUKABASHO1_TEXT" runat="server" Height="1.2em" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:8.9em;left:37em; width:32em;" ondblclick="Field_DBclick('WF_TODOKECODE1' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_TODOKECODE1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_TODOKECODE1')"></asp:TextBox>
                        <asp:Label ID="WF_TODOKECODE1_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:8.9em;left:54.5em; width:32em;" >
                        <asp:TextBox ID="WF_MODELDISTANCE1" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " onchange="ItemChange('WF_MODELDISTANCE1')"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:9.1em;left:60.5em; width:10em;" >
                        <asp:CheckBox ID="WF_MODIFYKBN1" runat="server" Height="1.2em" enabled="true"></asp:CheckBox>
                    </a>
                    <a style="position:fixed;top:10.0em;left:3em; width:100em;" >
                        <b>
                        <asp:Label ID="WF_TWO_LABEL" runat="server" Text="２．" Height="1.2em" Width="2em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        </b>
                    </a>
                    <a style="position:fixed;top:10.0em;left:5em; width:32em;" ondblclick="Field_DBclick('WF_SHARYOKBN2' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_SHARYOKBN2" runat="server" Height="1.2em" Width="2em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHARYOKBN2')"></asp:TextBox>
                        <asp:Label ID="WF_SHARYOKBN2_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:10.0em;left:12em; width:32em;" ondblclick="Field_DBclick('WF_OILPAYKBN2' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_OILPAYKBN2" runat="server" Height="1.2em" Width="3em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_OILPAYKBN2')"></asp:TextBox>
                        <asp:Label ID="WF_OILPAYKBN2_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:10.0em;left:19.5em; width:32em;" ondblclick="Field_DBclick('WF_SHUKABASHO2' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_SHUKABASHO2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHUKABASHO2')"></asp:TextBox>
                        <asp:Label ID="WF_SHUKABASHO2_TEXT" runat="server" Height="1.2em" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:10.0em;left:37em; width:32em;" ondblclick="Field_DBclick('WF_TODOKECODE2' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_TODOKECODE2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_TODOKECODE2')"></asp:TextBox>
                        <asp:Label ID="WF_TODOKECODE2_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:10.0em;left:54.5em; width:32em;" >
                        <asp:TextBox ID="WF_MODELDISTANCE2" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " onchange="ItemChange('WF_MODELDISTANCE2')"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:10.2em;left:60.5em; width:10em;" >
                        <asp:CheckBox ID="WF_MODIFYKBN2" runat="server" Height="1.2em" enabled="true"></asp:CheckBox>
                    </a>

                    <a style="position:fixed;top:11.1em;left:3em; width:100em;" >
                        <b>
                        <asp:Label ID="WF_THREE_LABEL" runat="server" Text="３．" Height="1.2em" Width="2em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        </b>
                    </a>
                    <a style="position:fixed;top:11.1em;left:5em; width:32em;" ondblclick="Field_DBclick('WF_SHARYOKBN3' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_SHARYOKBN3" runat="server" Height="1.2em" Width="2em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHARYOKBN3')"></asp:TextBox>
                        <asp:Label ID="WF_SHARYOKBN3_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:11.1em;left:12em; width:32em;" ondblclick="Field_DBclick('WF_OILPAYKBN3' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_OILPAYKBN3" runat="server" Height="1.2em" Width="3em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_OILPAYKBN3')"></asp:TextBox>
                        <asp:Label ID="WF_OILPAYKBN3_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:11.1em;left:19.5em; width:32em;" ondblclick="Field_DBclick('WF_SHUKABASHO3' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_SHUKABASHO3" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHUKABASHO3')"></asp:TextBox>
                        <asp:Label ID="WF_SHUKABASHO3_TEXT" runat="server" Height="1.2em" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:11.1em;left:37em; width:32em;" ondblclick="Field_DBclick('WF_TODOKECODE3' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_TODOKECODE3" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_TODOKECODE3')"></asp:TextBox>
                        <asp:Label ID="WF_TODOKECODE3_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:11.1em;left:54.5em; width:32em;" >
                        <asp:TextBox ID="WF_MODELDISTANCE3" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " onchange="ItemChange('WF_MODELDISTANCE3')"></asp:TextBox>
                    </a>

                    <a style="position:fixed;top:11.3em;left:60.5em; width:10em;" >
                        <asp:CheckBox ID="WF_MODIFYKBN3" runat="server" Height="1.2em" enabled="true"></asp:CheckBox>
                    </a>

                    <a style="position:fixed;top:12.2em;left:3em; width:100em;" >
                        <b>
                        <asp:Label ID="WF_FOUR_LABEL" runat="server" Text="４．" Height="1.2em" Width="2em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        </b>
                    </a>
                    <a style="position:fixed;top:12.2em;left:5em; width:32em;" ondblclick="Field_DBclick('WF_SHARYOKBN4' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_SHARYOKBN4" runat="server" Height="1.2em" Width="2em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHARYOKBN4')"></asp:TextBox>
                        <asp:Label ID="WF_SHARYOKBN4_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:12.2em;left:12em; width:32em;" ondblclick="Field_DBclick('WF_OILPAYKBN4' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_OILPAYKBN4" runat="server" Height="1.2em" Width="3em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_OILPAYKBN4')"></asp:TextBox>
                        <asp:Label ID="WF_OILPAYKBN4_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:12.2em;left:19.5em; width:32em;" ondblclick="Field_DBclick('WF_SHUKABASHO4' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_SHUKABASHO4" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHUKABASHO4')"></asp:TextBox>
                        <asp:Label ID="WF_SHUKABASHO4_TEXT" runat="server" Height="1.2em" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:12.2em;left:37em; width:32em;"  ondblclick="Field_DBclick('WF_TODOKECODE4' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_TODOKECODE4" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_TODOKECODE4')"></asp:TextBox>
                        <asp:Label ID="WF_TODOKECODE4_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:12.2em;left:54.5em; width:32em;" >
                        <asp:TextBox ID="WF_MODELDISTANCE4" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " onchange="ItemChange('WF_MODELDISTANCE4')"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:12.4em;left:60.5em; width:10em;" >
                        <asp:CheckBox ID="WF_MODIFYKBN4" runat="server" Height="1.2em" enabled="true"></asp:CheckBox>
                    </a>

                    <a style="position:fixed;top:13.3em;left:3em; width:100em;" >
                        <b>
                        <asp:Label ID="WF_FIVE_LABEL" runat="server" Text="５．" Height="1.2em" Width="2em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        </b>
                    </a>
                    <a style="position:fixed;top:13.3em;left:5em; width:32em;" ondblclick="Field_DBclick('WF_SHARYOKBN5' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_SHARYOKBN5" runat="server" Height="1.2em" Width="2em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHARYOKBN5')"></asp:TextBox>
                        <asp:Label ID="WF_SHARYOKBN5_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:13.3em;left:12em; width:32em;" ondblclick="Field_DBclick('WF_OILPAYKBN5' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_OILPAYKBN5" runat="server" Height="1.2em" Width="3em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_OILPAYKBN5')"></asp:TextBox>
                        <asp:Label ID="WF_OILPAYKBN5_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:13.3em;left:19.5em; width:32em;" ondblclick="Field_DBclick('WF_SHUKABASHO5' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_SHUKABASHO5" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHUKABASHO5')"></asp:TextBox>
                        <asp:Label ID="WF_SHUKABASHO5_TEXT" runat="server" Height="1.2em" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:13.3em;left:37em; width:32em;" ondblclick="Field_DBclick('WF_TODOKECODE5' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_TODOKECODE5" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_TODOKECODE5')"></asp:TextBox>
                        <asp:Label ID="WF_TODOKECODE5_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:13.3em;left:54.5em; width:32em;" >
                        <asp:TextBox ID="WF_MODELDISTANCE5" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " onchange="ItemChange('WF_MODELDISTANCE5')"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:13.5em;left:60.5em; width:10em;" >
                        <asp:CheckBox ID="WF_MODIFYKBN5" runat="server" Height="1.2em" enabled="true"></asp:CheckBox>
                    </a>

                    <a style="position:fixed;top:14.4em;left:3em; width:100em;" >
                        <b>
                        <asp:Label ID="WF_SIX_LABEL" runat="server" Text="６．" Height="1.2em" Width="2em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        </b>
                    </a>
                    <a style="position:fixed;top:14.4em;left:5em; width:32em;" ondblclick="Field_DBclick('WF_SHARYOKBN6' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_SHARYOKBN6" runat="server" Height="1.2em" Width="2em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHARYOKBN6')"></asp:TextBox>
                        <asp:Label ID="WF_SHARYOKBN6_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:14.4em;left:12em; width:32em;" ondblclick="Field_DBclick('WF_OILPAYKBN6' , <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" >
                        <asp:TextBox ID="WF_OILPAYKBN6" runat="server" Height="1.2em" Width="3em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_OILPAYKBN6')"></asp:TextBox>
                        <asp:Label ID="WF_OILPAYKBN6_TEXT" runat="server" Height="1.2em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:14.4em;left:19.5em; width:32em;" ondblclick="Field_DBclick('WF_SHUKABASHO6' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_SHUKABASHO6" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_SHUKABASHO6')"></asp:TextBox>
                        <asp:Label ID="WF_SHUKABASHO6_TEXT" runat="server" Height="1.2em" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:14.4em;left:37em; width:32em;" ondblclick="Field_DBclick('WF_TODOKECODE6' , <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" >
                        <asp:TextBox ID="WF_TODOKECODE6" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" onchange="ItemChange('WF_TODOKECODE6')"></asp:TextBox>
                        <asp:Label ID="WF_TODOKECODE6_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <a style="position:fixed;top:14.4em;left:54.5em; width:32em;" >
                        <asp:TextBox ID="WF_MODELDISTANCE6" runat="server" Height="1.2em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: right; " onchange="ItemChange('WF_MODELDISTANCE6')"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:14.6em;left:60.5em; width:10em;" >
                        <asp:CheckBox ID="WF_MODIFYKBN6" runat="server" Height="1.2em" enabled="true"></asp:CheckBox>
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
    </div>
</asp:Content>

<%@ Page Title="T00009" Language="vb" AutoEventWireup="false" CodeBehind="GRT00009JIMKINTAI.aspx.vb" Inherits="OFFICE.GRT00009JIMKINTAI" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRT00009WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="T00009H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/T00009.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/T00009.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var pnlListTotalAreaId = '<%=Me.pnlListTotalArea.ClientID%>';
        var IsPostBack = '<%=if(IsPostBack = True, "1", "0")%>';
        var EXTRALIST = '<%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>';
    </script>
</asp:Content>

<asp:Content ID="T00009" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerboxOnly" id="headerbox">
        <div class="Operation">
            <!-- 絞込従業員 -->
            <a>
                <asp:Label ID="WF_SELSTAFFCODE_L" runat="server" Text="絞込従業員" Height="1.5em" Font-Bold="true" Font-Underline="true"></asp:Label>
            </a>
            <a ondblclick="Field_DBclick('WF_SELSTAFFCODE', <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>)">
                <asp:TextBox ID="WF_SELSTAFFCODE" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_SELSTAFFCODE_TEXT" runat="server" Width="30em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ボタン -->
            <a style="position:fixed; top:2.8em; left:22em;">
                <input type="button" id="WF_ButtonCALC" runat="server" value="残業再計算" style="Width:7em" onclick="ButtonClick('WF_ButtonCALC');" />
            </a>
            <a style="position:fixed; top:2.8em; left:30em;">
                <input type="button" id="WF_ButtonDOWN" value="前頁" style="Width:5em" onclick="ButtonClick('WF_ButtonDOWN');" />
            </a>
            <a style="position:fixed; top:2.8em; left:34.5em;">
                <input type="button" id="WF_ButtonUP" value="次頁" style="Width:5em" onclick="ButtonClick('WF_ButtonUP');" />
            </a>
            <a style="position:fixed; top:2.8em; left:42.5em;">
                <input type="button" id="WF_ButtonSAVE" value="一時保存" style="Width:5em" onclick="ButtonClick('WF_ButtonSAVE');" />
            </a>

            <a style="position:fixed; top:2.8em; left:49em;">
                <input type="button" id="WF_ButtonExtract" value="絞り込み" style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
            </a>
            <a style="position:fixed; top:2.8em; left:53.5em;">
                <input type="button" id="WF_ButtonUPDATE" value="DB更新" style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
            <a style="position:fixed; top:2.8em; left:58em;">
                <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ" style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
            </a>
            <a style="position:fixed; top:2.8em; left:62.5em;">
                <input type="button" id="WF_ButtonPrint" value="一覧印刷" style="Width:5em" onclick="ButtonClick('WF_ButtonPrint');" />
            </a>
            <a style="position:fixed; top:2.8em; left:62.5em;">
                <input type="button" id="WF_ButtonUPDATE2" value="更新" style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE2');" />
            </a>
            <a style="position:fixed; top:2.8em; left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了" style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
        </div>
        
        <div id="detailkeybox">
            <p>
                <!-- 対象年月 -->
                <a>
                    <asp:Label ID="WF_TAISHOYM_L" runat="server" Text="対象年月" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_TAISHOYM" runat="server" Width="4em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                </a>

                <!-- 従業員 -->
                <a>
                    <asp:Label ID="WF_STAFFCODE_L" runat="server" Text="従業員" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_STAFFCODE" runat="server" Width="4em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
                </a>

                <!-- 配属部署 -->
                <a>
                    <asp:Label ID="WF_HORG_L" runat="server" Text="配属部署" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_HORG" runat="server" Width="4em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_HORG_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
                </a>

                <!-- インフォメーション -->
                <a>
                    <asp:Label ID="WF_INFO" runat="server" Text="" Width="30em" CssClass="WF_TEXT_LEFT" ForeColor="Red" Font-Bold="true"></asp:Label>
                </a>
            </p>
        </div>
        
        <!-- 明細行 -->
        <div id="divListArea">
            <asp:Panel id="pnlListArea" runat="server"></asp:Panel>
            <asp:Panel ID="pnlListTotalArea" runat="server"></asp:Panel>
        </div>

        <div id="divAdjustArea">
            <table id="tblAdjustArea">
                <!-- 25行08列 -->
                <tbody id="tblAdjucstArea_tbody">
                    <tr id="LINE_01">
                        <td id="WORKNISSU_L">
                            <!-- 所労 -->
                            <asp:Label ID="WF_WORKNISSUTTL_L" runat="server" Text="所 　 　 労" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="WORKNISSU">
                            <asp:TextBox ID="WF_WORKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>

                        <td id="NENKYUNISSU_L">
                            <!-- 年休 -->
                            <asp:Label ID="WF_NENKYUNISSUTTL_L" runat="server" Text="年 　 　 休" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="NENKYUNISSU">
                            <asp:TextBox ID="WF_NENKYUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                    
                        <td id="KYOTEIWEEKNISSU_L">
                            <!-- 協約週休 -->
                            <asp:Label ID="WF_KYOTEIWEEKNISSUTTL_L" runat="server" Text="協 約 週 休" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="KYOTEIWEEKNISSU">
                            <asp:TextBox ID="WF_KYOTEIWEEKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_02">
                        <td id="SHOUKETUNISSU_L">
                            <!-- 傷欠 -->
                            <asp:Label ID="WF_SHOUKETUNISSUTTL_L" runat="server" Text="傷 　 　 欠" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="SHOUKETUNISSU">
                            <asp:TextBox ID="WF_SHOUKETUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                    
                        <td id="TOKUKYUNISSU_L">
                            <!-- 特休 -->
                            <asp:Label ID="WF_TOKUKYUNISSUTTL_L" runat="server" Text="特 　 　 休" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="TOKUKYUNISSU">
                            <asp:TextBox ID="WF_TOKUKYUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                    
                        <td id="WEEKNISSU_L">
                            <!-- 週休 -->
                            <asp:Label ID="WF_WEEKNISSUTTL_L" runat="server" Text="週 　 　 休" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="WEEKNISSU">
                            <asp:TextBox ID="WF_WEEKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_03">
                        <td id="KUMIKETUNISSU_L">
                            <!-- 組欠 -->
                            <asp:Label ID="WF_KUMIKETUNISSUTTL_L" runat="server" Text="組 　 　 欠" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="KUMIKETUNISSU">
                            <asp:TextBox ID="WF_KUMIKETUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>

                        <td id="CHIKOKSOTAINISSU_L">
                            <!-- 遅早 -->
                            <asp:Label ID="WF_CHIKOKSOTAINISSUTTL_L" runat="server" Text="遅 　 　 早" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="CHIKOKSOTAINISSU">
                            <asp:TextBox ID="WF_CHIKOKSOTAINISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>

                        <td id="DAIKYUNISSU_L">
                            <!-- 代休 -->
                            <asp:Label ID="WF_DAIKYUNISSUTTL_L" runat="server" Text="代 　 　 休" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="DAIKYUNISSU">
                            <asp:TextBox ID="WF_DAIKYUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_04">
                        <td id="ETCKETUNISSU_L">
                            <!-- 他欠 -->
                            <asp:Label ID="WF_ETCKETUNISSUTTL_L" runat="server" Text="他 　 　 欠" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="ETCKETUNISSU">
                            <asp:TextBox ID="WF_ETCKETUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>

                        <td id="STOCKNISSU_L">
                            <!-- ｽﾄｯｸ休暇 -->
                            <asp:Label ID="WF_STOCKNISSUTTL_L" runat="server" Text="ｽﾄ ｯｸ 休 暇" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="STOCKNISSU">
                            <asp:TextBox ID="WF_STOCKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_05">
                        <!-- 空白行 -->
                        <td colspan="8">
                            <asp:Label runat="server" Text="" Width="0em"></asp:Label>
                        </td>
                    </tr>
                    <tr id="LINE_06">
                        <td id="NENMATUNISSU_L">
                            <!-- 年末出勤日 -->
                            <asp:Label ID="WF_NENMATUNISSUTTL_L" runat="server" Text="年末出勤日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="NENMATUNISSU">
                            <asp:TextBox ID="WF_NENMATUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_07">
                        <td id="NENSHINISSU_L">
                            <!-- 年始出勤日 -->
                            <asp:Label ID="WF_NENSHINISSUTTL_L" runat="server" Text="年始出勤日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="NENSHINISSU">
                            <asp:TextBox ID="WF_NENSHINISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>

                        <td id="SHUKCHOKHLDNISSU_L">
                            <!-- 宿日直通常（翌日休み） -->
                            <asp:Label ID="WF_SHUKCHOKHLDNISSUTTL_L" runat="server" Text="宿日直通常（翌日休み）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="SHUKCHOKHLDNISSU">
                            <asp:TextBox ID="WF_SHUKCHOKHLDNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_08">
                        <td id="HWORKNISSU_L">
                            <!-- 休日出勤日数 -->
                            <asp:Label ID="WF_HWORKNISSUTTL_L" runat="server" Text="休日出勤日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="HWORKNISSU">
                            <asp:TextBox ID="WF_HWORKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>

                        <td id="SHUKCHOKNISSU_L">
                            <!-- 宿日直通常 -->
                            <asp:Label ID="WF_SHUKCHOKNISSUTTL_L" runat="server" Text="宿日直通常" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="SHUKCHOKNISSU">
                            <asp:TextBox ID="WF_SHUKCHOKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_09">
                        <td></td>
                        <td></td>
                        <td id="SHUKCHOKNHLDNISSU_L">
                        <!-- 宿日直年始（翌日休み） -->
                            <asp:Label ID="WF_SHUKCHOKNHLDNISSUTTL_L" runat="server" Text="宿日直年始（翌日休み）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="SHUKCHOKNHLDNISSU">
                            <asp:TextBox ID="WF_SHUKCHOKNHLDNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_10">
                        <td></td>
                        <td></td>
                        <td id="SHUKCHOKNNISSU_L">
                            <!-- 宿日直年始 -->
                            <asp:Label ID="WF_SHUKCHOKNNISSUTTL_L" runat="server" Text="宿日直年始" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="SHUKCHOKNNISSU">
                            <asp:TextBox ID="WF_SHUKCHOKNNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_RIGHT"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_11">
                        <!-- 空白行 -->
                        <td colspan="8">
                            <asp:Label runat="server" Text="" Width="0em"></asp:Label>
                        </td>
                    </tr>
                    <tr id="LINE_12">
                        <td id="HAYADETIME_L">
                            <!-- 早出補填 -->
                            <asp:Label ID="WF_HAYADETIMETTL_L" runat="server" Text="早 出 補 填" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="HAYADETIME">
                            <asp:TextBox ID="WF_HAYADETIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_13">
                        <!-- 空白行 -->
                        <td colspan="8">
                            <asp:Label runat="server" Text="" Width="0em"></asp:Label>
                        </td>
                    </tr>
                    <tr id="LINE_14">
                        <!-- 空白行 -->
                        <td colspan="8">
                            <asp:Label runat="server" Text="" Width="0em"></asp:Label>
                        </td>
                    </tr>
                    <tr id="LINE_15">
                        <td></td>
                        <td id="OVERTIME_L">
                            <!-- 残業　深夜 -->
                            <asp:Label ID="WF_OVERTIME_L" runat="server" Text="残　業" Width="6em" CssClass="WF_TEXT_CENTER"></asp:Label>
                        </td>
                        <td id="NIGHT_L">
                            <asp:Label ID="WF_NIGHT_L" runat="server" Text="深　夜" Width="6em" CssClass="WF_TEXT_CENTER"></asp:Label>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_16">
                        <td id="WEEKDAY_L">
                            <!-- 平日 -->
                            <asp:Label ID="WF_WEEKDAY_L" runat="server" Text="平 　 　 日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="ORVERTIME">
                            <asp:TextBox ID="WF_ORVERTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td id="WNIGHTTIME">
                            <asp:TextBox ID="WF_WNIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td></td>
                        <td id="TOKUSA1TIME_L">
                            <!-- 特作Ⅰ -->
                            <asp:Label ID="WF_TOKUSA1TIMETTL_L" runat="server" Text="特作Ⅰ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="TOKUSA1TIME">
                            <asp:TextBox ID="WF_TOKUSA1TIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_17">
                        <td id="HOLIDAY_L">
                            <!-- 休日出勤 -->
                            <asp:Label ID="WF_HOLIDAY_L" runat="server" Text="休 日 出 勤" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="HWORKTIME">
                            <asp:TextBox ID="WF_HWORKTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td id="HNIGHTTIME">
                            <asp:TextBox ID="WF_HNIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td></td>
                        <td id="WWORKTIME_L">
                            <!-- 所定内時間 -->
                            <asp:Label ID="WF_WWORKTIMETTL_L" runat="server" Text="所定内計" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="WWORKTIME">
                            <asp:TextBox ID="WF_WWORKTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td id="JIKYUSHATIME_L">
                            <!-- 時給者時間 -->
                            <asp:Label ID="WF_JIKYUSHATIMETTL_L" runat="server" Text="時給者作業" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="JIKYUSHATIME">
                            <asp:TextBox ID="WF_JIKYUSHATIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                    </tr>
                    <tr id="LINE_18">
                        <td id="HDAI_L">
                            <!-- 代休出勤 -->
                            <asp:Label ID="WF_HDAI_L" runat="server" Text="代 休 出 勤" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="HDAIWORKTIME">
                            <asp:TextBox ID="WF_HDAIWORKTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td id="HDAINIGHTTIME">
                            <asp:TextBox ID="WF_HDAINIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_19">
                        <td id="SUNDAY_L">
                            <!-- 日曜出勤 -->
                            <asp:Label ID="WF_SUNDAY_L" runat="server" Text="日 曜 出 勤" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="SWORKTIME">
                            <asp:TextBox ID="WF_SWORKTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td id="SNIGHTTIME">
                            <asp:TextBox ID="WF_SNIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_20">
                        <td id="SDAI_L">
                            <!-- 日曜代休 -->
                            <asp:Label ID="WF_SDAI_L" runat="server" Text="日 曜 代 休" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td id="SDAIWORKTIME">
                            <asp:TextBox ID="WF_SDAIWORKTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td id="SDAINIGHTTIME">
                            <asp:TextBox ID="WF_SDAINIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_21">
                        <td id="NIGHTTIME_L">
                            <!-- 所定深夜 -->
                            <asp:Label ID="WF_NIGHTTIME_L" runat="server" Text="所 定 深 夜" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td></td>
                        <td id="NIGHTTIME">
                            <asp:TextBox ID="WF_NIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_22">
                        <!-- 空白行 -->
                        <td colspan="8">
                            <asp:Label runat="server" Text="" Width="0em"></asp:Label>
                        </td>
                    </tr>
                    <tr id="LINE_23">
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_24">
                        <td>
                            <asp:Label ID="WF_JIKYUSHATIMETTL_L2" runat="server" Text="所定内時間" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr id="LINE_25">
                        <!-- 空白行 -->
                        <td colspan="8">
                            <asp:Label runat="server" Text="" Width="0em"></asp:Label>
                        </td>
                    </tr>
                </tbody>
            </table>
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

        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        
        <input id="WF_DISP_SaveX" runat="server" value="" type="text" />            <!-- 明細位置X軸 -->
        <input id="WF_DISP_SaveY" runat="server" value="" type="text" />            <!-- 明細位置Y軸 -->
        
        <input id="WF_DISP" runat="server" value="" type="text" />                  <!-- 画面表示切替 -->
        <input id="WF_BEFORE_MAPID" runat="server" value="" type="text" />          <!-- 前画面ID -->
        <input id="WF_ONLY" runat="server" value="" type="text" />                  <!-- 勤怠個人 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />       <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->

        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->
        
        <input id="WF_XMLsaveF" runat="server" value="" type="text" />              <!-- 保存先TblURL -->
        <input id="WF_XMLsaveF_INP" runat="server" value="" type="text" />          <!-- 保存先INPTblURL -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />         <!-- 権限 -->
    </div>
</asp:Content>

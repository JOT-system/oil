<%@ Page Title="TA0003" Language="vb" AutoEventWireup="false" CodeBehind="GRTA0003KYUYOLIST.aspx.vb" Inherits="OFFICE.GRTA0003KYUYOLIST" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>

<%@ register src="inc/GRTA0003WRKINC.ascx" tagname="work" tagprefix="LSINC" %>
<asp:Content ID="GRTA0003H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/TA0003.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/TA0003.js")%>"></script>
</asp:Content>
<asp:Content ID="GRTA0003" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　headerbox -->
        <div  class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <!-- ■　条件　■ -->
                <a style="position:fixed;top:3.0em;left:5em;width:15.0em;">
                    <asp:Label runat="server" Text="対象年月：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_DATE" runat="server"    Font-Bold="True"></asp:Label>
                </a>
                <a style="position:fixed;top:3.0em;left:20em;width:15.0em;">
                    <asp:Label runat="server" Text="配属部署：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_ORG" runat="server"     Font-Bold="True"></asp:Label>
                </a>
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonPDF" value="印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPDF');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonXLS" value="Excel取得"  style="Width:5em" onclick="ButtonClick('WF_ButtonXLS');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.2em;left:75em;">
                    <asp:Image ID="WF_ButtonFIRST2" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST2" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
            </div>
            <!-- 一覧レイアウト -->
            <div id="divListArea">
                <asp:panel id="pnlListArea" runat="server" ></asp:panel>
            </div>
        </div>

        <!-- 全体レイアウト　detailbox -->
        <div  class="Detail detailboxOnly" id="detailbox" runat="server">
           <div class="Operation_Detail" >
                <a>
                    <input type="button" id="WF_BACK" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_BACK');" />
                </a>
            </div> 
            <!-- ■■■　１行目　■■■ -->
            <!-- ■　従業員　■ -->
            <a style="position:fixed;top:5.1em;left:3em; width:32em;" >
                <asp:Label ID="Label4" runat="server" Text="従業員" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                <b>
                <asp:TextBox ID="WF_STAFFCODETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true"></asp:TextBox>
                </b>
                <asp:Label ID="WF_STAFFCODETTL_TEXT" runat="server" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>

            <!-- ■■■　２行目　■■■ -->
            <!-- ■　所属部署　■ -->
            <a style="position:fixed;top:6.3em;left:3em; width:32em;" >
                <asp:Label ID="Label3" runat="server" Text="配属部署" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                <b>
                <asp:TextBox ID="WF_HORGTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true"></asp:TextBox>
                </b>
                <asp:Label ID="WF_HORGTTL_TEXT" runat="server" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>

            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <!-- エネックス用                                                                                                                                   -->
            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <span class="ENEX">

                <!-- ■■■　勤怠区分関連１　■■■ -->
                <a style="position:fixed;top:7.8em;left:3em; width:32em;" >
                    <asp:Label ID="Label5" runat="server" Text="所　　　　労" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_WORKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;" ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:14em; width:32em;" >
                    <asp:Label ID="Label6" runat="server" Text="年　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NENKYUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:25em; width:32em;" >
                    <asp:Label ID="Label7" runat="server" Text="協 約 週 休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KYOTEIWEEKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連２　■■■ -->
                <a style="position:fixed;top:8.9em;left:3em; width:32em;" >
                    <asp:Label ID="Label8" runat="server" Text="傷　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHOUKETUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:14em; width:32em;" >
                    <asp:Label ID="Label9" runat="server" Text="特　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUKYUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:25em; width:32em;" >
                    <asp:Label ID="Label10" runat="server" Text="週　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_WEEKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連３　■■■ -->
                <a style="position:fixed;top:10.0em;left:3em; width:32em;" >
                    <asp:Label ID="Label14" runat="server" Text="組　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KUMIKETUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:14em; width:32em;" >
                    <asp:Label ID="Label15" runat="server" Text="遅　　　　早" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_CHIKOKSOTAINISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:25em; width:32em;" >
                    <asp:Label ID="Label19" runat="server" Text="代　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_DAIKYUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連４　■■■ -->
                <a style="position:fixed;top:11.1em;left:3em; width:32em;" >
                    <asp:Label ID="Label48" runat="server" Text="他　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ETCKETUNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:14em; width:32em;" >
                    <asp:Label ID="Label49" runat="server" Text="ｽﾄｯｸ休暇" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STOCKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連１　■■■ -->
                <a style="position:fixed;top:13.3em;left:3em; width:32em;" >
                    <asp:Label ID="Label11" runat="server" Text="年始出勤日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NENSHINISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:14em; width:32em;" >
                    <asp:Label ID="Label12" runat="server" Text="宿日直年始" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKNNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:25em; width:32em;" >
                    <asp:Label ID="Label13" runat="server" Text="ポンプ日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_PONPNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連２　■■■ -->
                <a style="position:fixed;top:14.4em;left:14em; width:32em;" >
                    <asp:Label ID="Label51" runat="server" Text="宿日直通常" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14.4em;left:25em; width:32em;" >
                    <asp:Label ID="Label52" runat="server" Text="バルク日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_BULKNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連３　■■■ -->
                <a style="position:fixed;top:15.5em;left:25em; width:32em;" >
                    <asp:Label ID="Label68" runat="server" Text="トレーラ日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TRAILERNISSUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連３　■■■ -->
                <a style="position:fixed;top:16.6em;left:25em; width:32em;" >
                    <asp:Label ID="Label56" runat="server" Text="Ｂ勤務回数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_BKINMUKAISUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連４　■■■ -->
                <a style="position:fixed;top:18em;left:3em; width:32em;" >
                    <asp:Label ID="Label50" runat="server" Text="【 特 作 手 当 日 数 】" Height="1.3em" Width="16em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <!-- ■■■　手当関連５　■■■ -->
                <a style="position:fixed;top:19.5em;left:3em; width:32em;" >
                    <asp:Label ID="Label54" runat="server" Text="　　　　　Ａ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKSAAKAISUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:19.5em;left:14em; width:32em;" >
                    <asp:Label ID="Label55" runat="server" Text="　　　　　Ｂ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKSABKAISUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:19.5em;left:25em; width:32em;" >
                    <asp:Label ID="Label57" runat="server" Text="　　　　　Ｃ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKSACKAISUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連６　■■■ -->
                <a style="position:fixed;top:21.9em;left:3em; width:32em;" >
                    <asp:Label ID="Label95" runat="server" Text="【 点 呼 手 当 回 数 】" Height="1.3em" Width="16em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <a style="position:fixed;top:23.4em;left:3em; width:32em;" >
                    <asp:Label ID="Label94" runat="server" Text="　　　点 呼" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TENKOKAISUTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　残業関連１　■■■ -->
                <a style="position:fixed;top:26.2em;left:8.5em; width:32em;" >
                    <asp:Label ID="Label58" runat="server" Text="残　業" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:26.2em;left:14em; width:32em;" >
                    <asp:Label ID="Label59" runat="server" Text="深　夜" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:27.3em;left:3em; width:32em;" >
                    <asp:Label ID="Label61" runat="server" Text="平　　　　日" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:28.4em;left:3em; width:32em;" >
                    <asp:Label ID="Label60" runat="server" Text="休 日 出 勤" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:29.5em;left:3em; width:32em;" >
                    <asp:Label ID="Label64" runat="server" Text="日 曜 出 勤" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:30.6em;left:3em; width:32em;" >
                    <asp:Label ID="Label66" runat="server" Text="所 定 深 夜" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:30.6em;left:14.1em; width:32em;" >
                    <b>
                    <asp:TextBox ID="WF_NIGHTTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center;"></asp:TextBox>
                    </b>
                </a>

                <a style="position:fixed;top:27.3em;left:25em; width:32em;" >
                    <asp:Label ID="Label62" runat="server" Text="特 作 Ｉ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:28.4em;left:25em; width:32em;" >
                    <asp:Label ID="Label63" runat="server" Text="保 安 検 査" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HOANTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:29.5em;left:25em; width:32em;" >
                    <asp:Label ID="Label65" runat="server" Text="高 圧 作 業" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KOATUTIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: center;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:30.6em;left:25em; width:32em;" >
                    <asp:Label ID="WF_HAYADETIMETTL_LABEL" runat="server" Text="早 出 補 填" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HAYADETIMETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Enabled="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　油種別（卸回数、走行距離）　■■■ -->
                <a style="position:fixed;top:6.7em;left:37em; width:32em;" >
                    <asp:Label ID="Label53" runat="server" Text="【単車】" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:42.5em; width:32em;" >
                    <asp:Label ID="Label67" runat="server" Text="荷卸回数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:48em; width:32em;" >
                    <asp:Label ID="Label69" runat="server" Text="走行㎞" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:55em; width:32em;" >
                    <asp:Label ID="Label70" runat="server" Text="【ﾄﾚｰﾗ】" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:60.5em; width:32em;" >
                    <asp:Label ID="Label71" runat="server" Text="荷卸回数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:66em; width:32em;" >
                    <asp:Label ID="Label72" runat="server" Text="走行㎞" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <a style="position:fixed;top:7.8em;left:37em; width:32em;" >
                    <asp:Label ID="Label73" runat="server" Text="一般" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_IPPAN1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_IPPAN1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:55em; width:32em;" >
                    <asp:Label ID="Label74" runat="server" Text="一般" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_IPPAN2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_IPPAN2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:37em; width:32em;" >
                    <asp:Label ID="Label75" runat="server" Text="潤滑油" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_JUN1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_JUN1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:55em; width:32em;" >
                    <asp:Label ID="Label76" runat="server" Text="潤滑油" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_JUN2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_JUN2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:37em; width:32em;" >
                    <asp:Label ID="Label77" runat="server" Text="ＬＰＧ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_LPG1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LPG1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:55em; width:32em;" >
                    <asp:Label ID="Label78" runat="server" Text="ＬＰＧ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_LPG2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LPG2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:37em; width:32em;" >
                    <asp:Label ID="Label79" runat="server" Text="ＬＮＧ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_LNG1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LNG1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:55em; width:32em;" >
                    <asp:Label ID="Label80" runat="server" Text="ＬＮＧ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_LNG2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LNG2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:12.2em;left:37em; width:32em;" >
                    <asp:Label ID="Label81" runat="server" Text="コンテナ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_CONT1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_CONT1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:12.2em;left:55em; width:32em;" >
                    <asp:Label ID="Label82" runat="server" Text="コンテナ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_CONT2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_CONT2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:37em; width:32em;" >
                    <asp:Label ID="Label83" runat="server" Text="酸素" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_SANS1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_SANS1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:55em; width:32em;" >
                    <asp:Label ID="Label84" runat="server" Text="酸素" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_SANS2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_SANS2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14.4em;left:37em; width:32em;" >
                    <asp:Label ID="Label85" runat="server" Text="窒素" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_CHIS1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_CHIS1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14.4em;left:55em; width:32em;" >
                    <asp:Label ID="Label86" runat="server" Text="窒素" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_CHIS2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_CHIS2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:15.5em;left:37em; width:32em;" >
                    <asp:Label ID="Label87" runat="server" Text="水素" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_SUIS1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_SUIS1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:15.5em;left:55em; width:32em;" >
                    <asp:Label ID="Label88" runat="server" Text="水素" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_SUIS2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_SUIS2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:16.6em;left:37em; width:32em;" >
                    <asp:Label ID="Label89" runat="server" Text="ﾒﾀｰﾉｰﾙ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_META1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_META1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:16.6em;left:55em; width:32em;" >
                    <asp:Label ID="Label90" runat="server" Text="ﾒﾀｰﾉｰﾙ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_META2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_META2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:17.7em;left:37em; width:32em;" >
                    <asp:Label ID="Label91" runat="server" Text="ﾗﾃｯｸｽ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_RATE1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_RATE1" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:17.7em;left:55em; width:32em;" >
                    <asp:Label ID="Label92" runat="server" Text="ﾗﾃｯｸｽ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_RATE2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_RATE2" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:19.5em;left:55em; width:32em;" >
                    <asp:Label ID="Label93" runat="server" Text="合計" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNTTTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCETTL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" readonly="true" style="text-align: right;"></asp:TextBox>
                    </b>
                </a>
            </span>

            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <!-- 近石用                                                                                                                                   -->
            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <span class="KNK">
                <!-- ■■■　勤怠区分関連１　■■■ -->
                <a style="position:fixed;top:7.8em;left:3em; width:32em;" >
                    <asp:Label ID="Label1" runat="server" Text="所　　　　労" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_WORKNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:14em; width:32em;" >
                    <asp:Label ID="Label23" runat="server" Text="傷　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHOUKETUNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:25em; width:32em;" >
                    <asp:Label ID="Label2" runat="server" Text="年　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NENKYUNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連２　■■■ -->
                <a style="position:fixed;top:8.9em;left:3em; width:32em;" >
                    <asp:Label ID="Label26" runat="server" Text="遅　　　　早" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_CHIKOKSOTAINISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:14em; width:32em;" >
                    <asp:Label ID="Label16" runat="server" Text="組　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KUMIKETUNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:25em; width:32em;" >
                    <asp:Label ID="Label17" runat="server" Text="特　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUKYUNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連３　■■■ -->
                <a style="position:fixed;top:10.0em;left:14em; width:32em;" >
                    <asp:Label ID="Label18" runat="server" Text="他　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ETCKETUNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:25em; width:32em;" >
                    <asp:Label ID="Label20" runat="server" Text="ｽﾄｯｸ休暇" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STOCKNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連４　■■■ -->
                <a style="position:fixed;top:11.1em;left:25em; width:32em;" >
                    <asp:Label ID="Label21" runat="server" Text="協 約 週 休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KYOTEIWEEKNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■■■　勤怠区分関連５　■■■ -->
                <a style="position:fixed;top:12.2em;left:25em; width:32em;" >
                    <asp:Label ID="Label22" runat="server" Text="週　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_WEEKNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <!-- ■■■　勤怠区分関連６　■■■ -->
                <a style="position:fixed;top:13.3em;left:25em; width:32em;" >
                    <asp:Label ID="Label24" runat="server" Text="代　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_DAIKYUNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連１　■■■ -->
                <a style="position:fixed;top:15.5em;left:3em; width:32em;" >
                    <asp:Label ID="Label25" runat="server" Text="年始出勤日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NENSHINISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:15.5em;left:14em; width:32em;" >
                    <asp:Label ID="Label27" runat="server" Text="宿日直年始" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKNNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連２　■■■ -->
                <a style="position:fixed;top:16.6em;left:3em; width:32em;" >
                    <asp:Label ID="Label37" runat="server" Text="休日出勤日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:16.6em;left:14em; width:32em;" >
                    <asp:Label ID="Label28" runat="server" Text="宿日直通常" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKNISSUTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　残業関連１　■■■ -->
                <a style="position:fixed;top:6.7em;left:43.2em; width:32em;" >
                    <asp:Label ID="Label29" runat="server" Text="残　業" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:48.5em; width:32em;" >
                    <asp:Label ID="Label30" runat="server" Text="深　夜" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:7.8em;left:37em; width:32em;" >
                    <asp:Label ID="Label31" runat="server" Text="平　　　　日" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:37em; width:32em;" >
                    <asp:Label ID="Label32" runat="server" Text="休 日 出 勤" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:37em; width:32em;" >
                    <asp:Label ID="Label33" runat="server" Text="代 休 出 勤" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HDAIWORKTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HDAINIGHTTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:37em; width:32em;" >
                    <asp:Label ID="Label34" runat="server" Text="日 曜 出 勤" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:12.2em;left:37em; width:32em;" >
                    <asp:Label ID="Label35" runat="server" Text="日 曜 代 休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SDAIWORKTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_SDAINIGHTTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:37em; width:32em;" >
                    <asp:Label ID="Label36" runat="server" Text="所 定 深 夜" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:13.3em;left:48.1em; width:32em;" >
                    <b>
                    <asp:TextBox ID="WF_NIGHTTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <a style="position:fixed;top:14.4em;left:37em; width:32em;" >
                    <asp:Label ID="Label38" runat="server" Text="特 作 Ｉ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <a style="position:fixed;top:15.5em;left:37em; width:32em;" >
                    <asp:Label ID="Label41" runat="server" Text="所定内計" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_WWORKTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <a style="position:fixed;top:16.6em;left:37em; width:32em;" >
                    <asp:Label ID="Label39" runat="server" Text="乗務日計" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_JYOMUTIMETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　油種別（卸回数、走行距離）　■■■ -->
                <a style="position:fixed;top:6.7em;left:61em; width:32em;" >
                    <asp:Label ID="Label40" runat="server" Text="回　転" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:66em; width:32em;" >
                    <asp:Label ID="Label42" runat="server" Text="走行㎞" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <a style="position:fixed;top:7.8em;left:55em; width:32em;" >
                    <asp:Label ID="Label43" runat="server" Text="白油単車" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_WHITE1_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_WHITE1_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:55em; width:32em;" >
                    <asp:Label ID="Label44" runat="server" Text="黒油単車" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_BLACK1_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_BLACK1_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:55em; width:32em;" >
                    <asp:Label ID="Label45" runat="server" Text="LPG単車" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_LPG1_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LPG1_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:55em; width:32em;" >
                    <asp:Label ID="Label46" runat="server" Text="LNG単車" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_LNG1_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LNG1_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:12.2em;left:55em; width:32em;" >
                    <asp:Label ID="Label47" runat="server" Text="白油ﾄﾚｰﾗ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_WHITE2_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_WHITE2_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:55em; width:32em;" >
                    <asp:Label ID="Label96" runat="server" Text="黒油ﾄﾚｰﾗ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_BLACK2_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_BLACK2_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14.4em;left:55em; width:32em;" >
                    <asp:Label ID="Label97" runat="server" Text="LPGﾄﾚｰﾗ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_LPG2_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LPG2_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:15.5em;left:55em; width:32em;" >
                    <asp:Label ID="Label98" runat="server" Text="LNGﾄﾚｰﾗ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNT_LNG2_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LNG2_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:17.0em;left:55em; width:32em;" >
                    <asp:Label ID="Label99" runat="server" Text="合計" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KAITENCNTTTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCETTL_KNK" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
            </span>

            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <!-- ＮＪＳ用                                                                                                                                       -->
            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <span class="NJS">
                <!-- ■■■　勤怠区分関連１　■■■ -->
                <a style="position:fixed;top:7.8em;left:3em; width:32em;" >
                    <asp:Label ID="Label100" runat="server" Text="所　　　　労" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_WORKNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:14em; width:32em;" >
                    <asp:Label ID="Label101" runat="server" Text="年　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NENKYUNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:25em; width:32em;" >
                    <asp:Label ID="Label102" runat="server" Text="協 約 週 休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KYOTEIWEEKNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連２　■■■ -->
                <a style="position:fixed;top:8.9em;left:3em; width:32em;" >
                    <asp:Label ID="Label103" runat="server" Text="傷　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHOUKETUNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:14em; width:32em;" >
                    <asp:Label ID="Label104" runat="server" Text="特　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUKYUNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:25em; width:32em;" >
                    <asp:Label ID="Label105" runat="server" Text="週　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_WEEKNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連３　■■■ -->
                <a style="position:fixed;top:10.0em;left:3em; width:32em;" >
                    <asp:Label ID="Label106" runat="server" Text="組　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KUMIKETUNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:14em; width:32em;" >
                    <asp:Label ID="Label107" runat="server" Text="遅　　　　早" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_CHIKOKSOTAINISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:25em; width:32em;" >
                    <asp:Label ID="Label108" runat="server" Text="代　　　　休" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_DAIKYUNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連４　■■■ -->
                <a style="position:fixed;top:11.1em;left:3em; width:32em;" >
                    <asp:Label ID="Label109" runat="server" Text="他　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ETCKETUNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:14em; width:32em;" >
                    <asp:Label ID="Label110" runat="server" Text="ｽﾄｯｸ休暇" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STOCKNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連１　■■■ -->
                <a style="position:fixed;top:13.3em;left:3em; width:32em;" >
                    <asp:Label ID="Label111" runat="server" Text="年末出勤日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NENMATUNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14.4em;left:3em; width:32em;" >
                    <asp:Label ID="Label112" runat="server" Text="年始出勤日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NENSHINISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:15.5em;left:3em; width:32em;" >
                    <asp:Label ID="Label113" runat="server" Text="車中泊日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHACHUHAKNISSUTTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　残業関連１　■■■ -->
                <a style="position:fixed;top:20.0em;left:8.5em; width:32em;" >
                    <asp:Label ID="Label114" runat="server" Text="残　業" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:20.0em;left:14em; width:32em;" >
                    <asp:Label ID="Label115" runat="server" Text="深　夜" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:21.1em;left:3em; width:32em;" >
                    <asp:Label ID="Label116" runat="server" Text="平　　　　日" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIMETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIMETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:22.2em;left:3em; width:32em;" >
                    <asp:Label ID="Label117" runat="server" Text="休 日 出 勤" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIMETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIMETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:23.3em;left:3em; width:32em;" >
                    <asp:Label ID="Label118" runat="server" Text="日 曜 出 勤" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIMETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIMETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:24.4em;left:3em; width:32em;" >
                    <asp:Label ID="Label119" runat="server" Text="所 定 深 夜" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:24.4em;left:14.1em; width:32em;" >
                    <b>
                        <asp:TextBox ID="WF_NIGHTTIMETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <a style="position:fixed;top:21.1em;left:25em; width:32em;" >
                    <asp:Label ID="Label120" runat="server" Text="特 作" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIMETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:22.2em;left:25em; width:32em;" >
                    <asp:Label ID="Label121" runat="server" Text="時給者作業" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_JIKYUSHATIMETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>



                <!-- ■■■　油種別（卸回数、走行距離）　■■■ -->
                <a style="position:fixed;top:6.7em;left:42em; width:32em;" >
                    <asp:Label ID="Label122" runat="server" Text="【単車】" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:47em; width:32em;" >
                    <asp:Label ID="Label123" runat="server" Text="ﾓﾃﾞﾙ距離" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:55em; width:32em;" >
                    <asp:Label ID="Label124" runat="server" Text="【ﾄﾚｰﾗ】" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:60em; width:32em;" >
                    <asp:Label ID="Label125" runat="server" Text="ﾓﾃﾞﾙ距離" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <a style="position:fixed;top:7.8em;left:42em; width:32em;" >
                    <asp:Label ID="Label126" runat="server" Text="ﾗﾃｯｸｽ" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE_RATE1_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:55em; width:32em;" >
                    <asp:Label ID="Label127" runat="server" Text="ﾗﾃｯｸｽ" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE_RATE2_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:42em; width:32em;" hidden="hidden">
                    <asp:Label ID="Label128" runat="server" Text="ＬＮＧ" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE_LNG1_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:55em; width:32em;" >
                    <asp:Label ID="Label129" runat="server" Text="ＬＮＧ" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCE_LNG2_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:55em; width:32em;" >
                    <asp:Label ID="Label130" runat="server" Text="合計" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_MODELDISTANCETTL_NJS" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="false" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
            </span>

            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <!-- ＪＫトランス用                                                                                                                                 -->
            <!---------------------------------------------------------------------------------------------------------------------------------------------------->
            <span class="JKT">
                <!-- ■■■　勤怠区分関連１　■■■ -->
                <a style="position:fixed;top:7.8em;left:3em; width:32em;" >
                    <asp:Label ID="Label131" runat="server" Text="所　　　　労" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_WORKNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:14.5em; width:32em;" >
                    <asp:Label ID="Label132" runat="server" Text="年　　　　休" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NENKYUNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:26em; width:32em;" >
                    <asp:Label ID="Label133" runat="server" Text="協 約 週 休" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KYOTEIWEEKNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連２　■■■ -->
                <a style="position:fixed;top:8.9em;left:3em; width:32em;" >
                    <asp:Label ID="Label134" runat="server" Text="傷　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHOUKETUNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:14.5em; width:32em;" >
                    <asp:Label ID="Label135" runat="server" Text="特　　　　休" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUKYUNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連３　■■■ -->
                <a style="position:fixed;top:10.0em;left:3em; width:32em;" >
                    <asp:Label ID="Label136" runat="server" Text="休 業 日 数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_KUMIKETUNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:14.5em; width:32em;" >
                    <asp:Label ID="Label137" runat="server" Text="遅　　　　早" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_CHIKOKSOTAINISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:26em; width:32em;" >
                    <asp:Label ID="Label138" runat="server" Text="代　　　　休" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_DAIKYUNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　勤怠区分関連４　■■■ -->
                <a style="position:fixed;top:11.1em;left:3em; width:32em;" >
                    <asp:Label ID="Label139" runat="server" Text="他　　　　欠" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ETCKETUNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:14.5em; width:32em;" >
                    <asp:Label ID="Label140" runat="server" Text="ｽﾄｯｸ休暇" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_STOCKNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連１　■■■ -->
                <a style="position:fixed;top:13.3em;left:3em; width:32em;" >
                    <asp:Label ID="Label141" runat="server" Text="年始出勤日数" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_NENSHINISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:14.5em; width:32em;" >
                    <asp:Label ID="Label142" runat="server" Text="宿日直年始" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKNNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:26em; width:32em;" >
                    <asp:Label ID="Label143" runat="server" Text="車中泊回数" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHACHUHAKNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　手当関連２　■■■ -->
                <a style="position:fixed;top:14.4em;left:14.5em; width:32em;" >
                    <asp:Label ID="Label144" runat="server" Text="宿日直通常" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHUKCHOKNISSUTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:14.4em;left:26em; width:32em;" >
                    <asp:Label ID="Label145" runat="server" Text="洗浄回数" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SENJYOCNTTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>


                <!-- ■■■　残業関連１　■■■ -->
                <a style="position:fixed;top:17.9em;left:10em; width:32em;" >
                    <asp:Label ID="Label146" runat="server" Text="残　業" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:17.9em;left:15.5em; width:32em;" >
                    <asp:Label ID="Label147" runat="server" Text="深　夜" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:19.0em;left:3em; width:32em;" >
                    <asp:Label ID="Label148" runat="server" Text="平　　　　日" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_ORVERTIMETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_WNIGHTTIMETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:20.1em;left:3em; width:32em;" >
                    <asp:Label ID="Label149" runat="server" Text="休 日 出 勤" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_HWORKTIMETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HNIGHTTIMETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:21.2em;left:3em; width:32em;" >
                    <asp:Label ID="Label150" runat="server" Text="日 曜 出 勤" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SWORKTIMETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_SNIGHTTIMETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:22.3em;left:3em; width:32em;" >
                    <asp:Label ID="Label151" runat="server" Text="所 定 深 夜" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:22.3em;left:14.15em; width:32em;" >
                    <b>
                    <asp:TextBox ID="WF_NIGHTTIMETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>


                <a style="position:fixed;top:24.0em;left:3em; width:32em;" >
                    <asp:Label ID="Label152" runat="server" Text="時間給者" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_JIKYUSHATIMETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <a style="position:fixed;top:25.1em;left:3em; width:32em;" >
                    <asp:Label ID="Label153" runat="server" Text="所定内時間" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <!-- ■■■　手当関連３　■■■ -->
                <a style="position:fixed;top:19.0em;left:26em; width:30em;" >
                    <asp:Label ID="Label154" runat="server" Text="特 作 Ｉ" Height="1.3em" Width="5.5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TOKUSA1TIMETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: center; "></asp:TextBox>
                    </b>
                </a>

                <a style="position:fixed;top:17.9em;left:44em; width:32em;" >
                    <asp:Label ID="Label155" runat="server" Text="荷卸時" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:19.0em;left:37em; width:32em;" >
                    <asp:Label ID="Label156" runat="server" Text="卸危険品100" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADADDCNT1TTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:20.1em;left:37em; width:32em;" >
                    <asp:Label ID="Label157" runat="server" Text="卸危険品200" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADADDCNT2TTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:21.2em;left:37em; width:32em;" >
                    <asp:Label ID="Label158" runat="server" Text="卸危険品800" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADADDCNT3TTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:22.3em;left:37em; width:32em;" >
                    <asp:Label ID="Label180" runat="server" Text="積危険品1000" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_LOADINGCNT1TTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <a style="position:fixed;top:24.0em;left:37em; width:32em;" >
                    <asp:Label ID="Label160" runat="server" Text="短距離1000" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHORTDISTANCE1TTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:25.1em;left:37em; width:32em;" >
                    <asp:Label ID="Label159" runat="server" Text="短距離2000" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_SHORTDISTANCE2TTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>

                <!-- ■■■　油種別（卸回数、走行距離）　■■■ -->
                <a style="position:fixed;top:6.7em;left:37em; width:32em;" >
                    <asp:Label ID="Label161" runat="server" Text="【単車】" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:43.5em; width:32em;" >
                    <asp:Label ID="Label162" runat="server" Text="荷卸回数" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:49.5em; width:32em;" >
                    <asp:Label ID="Label163" runat="server" Text="走行㎞" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:55em; width:32em;" >
                    <asp:Label ID="Label164" runat="server" Text="【ﾄﾚｰﾗ】" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:61.5em; width:32em;" >
                    <asp:Label ID="Label165" runat="server" Text="荷卸回数" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>
                <a style="position:fixed;top:6.7em;left:67.5em; width:32em;" >
                    <asp:Label ID="Label166" runat="server" Text="走行㎞" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                </a>

                <a style="position:fixed;top:7.8em;left:37em; width:32em;" >
                    <asp:Label ID="Label167" runat="server" Text="燃料油" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_IPPAN1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_IPPAN1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:7.8em;left:55em; width:32em;" >
                    <asp:Label ID="Label168" runat="server" Text="燃料油" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_IPPAN2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_IPPAN2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:37em; width:32em;" >
                    <asp:Label ID="Label169" runat="server" Text="太陽油脂" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_TAIYO1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_TAIYO1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:8.9em;left:55em; width:32em;" >
                    <asp:Label ID="Label170" runat="server" Text="太陽油脂" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_TAIYO2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_TAIYO2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:37em; width:32em;" >
                    <asp:Label ID="Label171" runat="server" Text="ｲﾝﾌｨﾆｱﾑ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_INF1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_INF1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:10.0em;left:55em; width:32em;" >
                    <asp:Label ID="Label172" runat="server" Text="ｲﾝﾌｨﾆｱﾑ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_INF2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_INF2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:37em; width:32em;" >
                    <asp:Label ID="Label173" runat="server" Text="化成・潤滑" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_JUN1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_JUN1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:11.1em;left:55em; width:32em;" >
                    <asp:Label ID="Label174" runat="server" Text="化成・潤滑" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_JUN2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_JUN2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:12.2em;left:37em; width:32em;" >
                    <asp:Label ID="Label175" runat="server" Text="コンテナ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_CONT1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_CONT1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:12.2em;left:55em; width:32em;" >
                    <asp:Label ID="Label176" runat="server" Text="コンテナ" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_CONT2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_CONT2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:37em; width:32em;" >
                    <asp:Label ID="Label177" runat="server" Text="高圧" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_LPG1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LPG1_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:13.3em;left:55em; width:32em;" >
                    <asp:Label ID="Label178" runat="server" Text="高圧" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNT_LPG2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCE_LPG2_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; " ></asp:TextBox>
                    </b>
                </a>
                <a style="position:fixed;top:15.0em;left:55em; width:32em;" >
                    <asp:Label ID="Label179" runat="server" Text="合計" Height="1.3em" Width="6em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_UNLOADCNTTTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                    <b>
                    <asp:TextBox ID="WF_HAIDISTANCETTL_JKT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" Readonly="true" style="text-align: right; "></asp:TextBox>
                    </b>
                </a>
            </span>
        </div>  

        <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>         <!-- GridViewダブルクリック -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>        <!-- GridView表示位置フィールド -->
            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />             <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>            <!-- Textbox DBクリックフィールド -->
            <input id="WF_RightViewChange" runat="server" value="" type="text"/>            <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />             <!-- Rightbox 開閉 -->

            <input id="WF_REP_LINECNT"  runat="server" value=""  type="text" />             <!-- Repeater 行位置 -->
            <input id="WF_REP_POSITION"  runat="server" value=""  type="text" />            <!-- Repeater 行位置 -->
            <input id="WF_REP_ROWSCNT" runat="server" value=""  type="text" />              <!-- Repeaterの１明細の行数 -->

            <input id="WF_PrintURL" runat="server" value=""  type="text" />                  <!-- Textbox Print URL -->
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />              <!-- ボタン押下 -->
        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>
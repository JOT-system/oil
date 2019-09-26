<%@ Page Title="T00004" Language="vb" AutoEventWireup="false" CodeBehind="GRT00004HORDER.aspx.vb" Inherits="OFFICE.GRT00004HORDER" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00004WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00004H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00004.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00004.js")%>"></script>
</asp:Content> 

<asp:Content ID="T00004" ContentPlaceHolderID="contents1" runat="server">

    <!-- 全体レイアウト　headerbox -->
    <div  class="headerboxOnly" id="headerbox">
        <div class="Operation">

            <!-- ■　取引先　■ -->
            <a ondblclick="Field_DBclick('WF_SELTORICODE', <%= LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)">
                <asp:Label ID="WF_SELTORICODE_LABEL" runat="server" Text="取引先" Width="3.2em" Font-Bold="True" Font-Underline="True"></asp:Label>
                <asp:TextBox ID="WF_SELTORICODE" runat="server" Width="6em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                <asp:Label ID="WF_SELTORICODE_TEXT" runat="server" Width="12em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>
            <!-- ■　受注部署　■ -->
            <a ondblclick="Field_DBclick('WF_SELORDERORG', <%= LIST_BOX_CLASSIFICATION.LC_ORG%>)">
                <asp:Label ID="WF_SELORDERORG_LABEL" runat="server" Text="受注部署" Width="4.2em" Font-Bold="True" Font-Underline="True"></asp:Label>
                <asp:TextBox ID="WF_SELORDERORG" runat="server" Width="4em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                <asp:Label ID="WF_SELORDERORG_TEXT" runat="server" Width="8em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>

            <!-- ■　ボタン　■ -->
            <a style="position:fixed;top:2.8em;left:34.5em;">
                <input type="button" id="WF_ButtonGet" value="光英受信"  style="Width:5em" onclick="ButtonClick('WF_ButtonGet');" />
            </a>
            <a style="position:fixed;top:2.8em;left:39em;">
                <input type="button" id="WF_ButtonSAVE" value="一時保存"  style="Width:5em" onclick="ButtonClick('WF_ButtonSAVE');" />
            </a>
            <a style="position:fixed;top:2.8em;left:44.5em;">
                <input type="button" id="WF_ButtonExtract" value="絞り込み"  style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
            </a>
            <a style="position:fixed;top:2.8em;left:49em;">
                <input type="button" id="WF_ButtonNEW" value="新規"  style="Width:5em" onclick="ButtonClick('WF_ButtonNEW');" />
            </a>
            <a style="position:fixed;top:2.8em;left:53.5em;">
                <input type="button" id="WF_ButtonUPDATE" value="DB更新"  style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
            <a style="position:fixed;top:2.8em;left:58em;">
                <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ"  style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
            </a>
            <a style="position:fixed;top:2.8em;left:62.5em;">
                <input type="button" id="WF_ButtonPrint" value="一覧印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPrint');" />
            </a>
            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
            <a style="position:fixed;top:3.2em;left:75em;">
                <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>
            <a style="position:fixed;top:3.2em;left:77em;">
                <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>
        </div>
        <!-- 一覧レイアウト -->
        <div id="divListArea">
            <asp:panel id="pnlListArea" runat="server" ></asp:panel>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div  class="detailboxOnly" id="detailbox">
        <div id="detailbuttonbox" class="detailbuttonbox">

        <a style="position:fixed;top:2.8em;left:58em;">
            <input type="button" id="WF_UPDATE" value="表更新"  style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
        </a>
        <a style="position:fixed;top:2.8em;left:62.5em;">
            <input type="button" id="WF_CLEAR" value="クリア"  style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
        </a>
        <a style="position:fixed;top:2.8em;left:67em;">
            <input type="button" id="WF_BACK" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_BACK');" />
        </a><br />
        </div> 

        <div id="detailkeybox"  onchange="f_Rep1_Change(1)">

        <p id="KEY_LINE_1">
        <!-- ■　選択No　■ -->
        <a style="position:fixed;top:3.0em;left:3em; width:32em;">
            <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
            <asp:TextBox ID="WF_Sel_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
        </a>
        </p>
            
        <p id="KEY_LINE_2">
        <!-- ■　出庫日　■ -->
        <a style="position:fixed;top:4.4em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_SHUKODATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
            <asp:Label ID="WF_SHUKODATE_L" runat="server" Text="出庫日" Height="1.3em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_SHUKODATE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>

        <!-- ■　帰庫日　■ -->
        <a style="position:fixed;top:4.4em;left:30.5em; width:32em;" ondblclick="Field_DBclick('WF_KIKODATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
            <asp:Label ID="WF_KIKODATE_L" runat="server" Text="帰庫日" Height="1.3em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_KIKODATE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>

        <!-- ■　受注番号　■ -->
        <a style="position:fixed;top:4.4em;left:59.5em; width:32em;">
            <asp:Label ID="WF_ORDERNO_L" runat="server" Text="受注番号" Height="1.3em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
            <asp:TextBox ID="WF_ORDERNO" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
        </a>
        </p>

        <p id="KEY_LINE_3">
        <!-- ■　出荷日　■ -->
        <a style="position:fixed;top:5.8em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_SHUKADATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
            <asp:Label ID="WF_SHUKADATE_L" runat="server" Text="出荷日" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_SHUKADATE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>

        <!-- ■　届日　■ -->
        <a style="position:fixed;top:5.8em;left:30.5em; width:32em;" ondblclick="Field_DBclick('WF_TODOKEDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
            <asp:Label ID="WF_TODOKEDATE_L" runat="server" Text="届日" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_TODOKEDATE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>

        <!-- ■　両目　■ -->
        <a style="position:fixed;top:5.8em;left:59.5em; width:32em;">
            <asp:Label ID="WF_RYOME_L" runat="server" Text="両目" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
            <asp:TextBox ID="WF_RYOME" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>

        <!-- ■　明細番号　■ -->
        <a style="position:fixed;top:5.8em;left:59.5em; width:32em;" hidden="hidden">
            <asp:Label ID="WF_DETAILNO_L" runat="server" Text="明細No" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
            <asp:TextBox ID="WF_DETAILNO" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
        </a>

        </p>

        <p id="KEY_LINE_4">
        <!-- ■　油種　■ -->
        <a style="position:fixed;top:7.2em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_OILTYPE', <%=LIST_BOX_CLASSIFICATION.LC_OILTYPE  %>)">
            <asp:Label ID="WF_OILTYPE_L" runat="server" Text="油種" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_OILTYPE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            <asp:Label ID="WF_OILTYPE_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　受注部署　■ -->
        <a style="position:fixed;top:7.2em;left:30.5em; width:40em;" ondblclick="Field_DBclick('WF_ORDERORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG  %>)">
            <asp:Label ID="WF_ORDERORG_L" runat="server" Text="受注部署" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_ORDERORG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            <asp:Label ID="WF_ORDERORG_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　出荷部署　■ -->
        <a style="position:fixed;top:7.2em;left:59.5em; width:40em;" ondblclick="Field_DBclick('WF_SHIPORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG  %>)">
            <asp:Label ID="WF_SHIPORG_L" runat="server" Text="出荷部署" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_SHIPORG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            <asp:Label ID="WF_SHIPORG_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>
        </p>

        <p id="KEY_LINE_5">
        <!-- ■　取引先　■ -->
        <a style="position:fixed;top:8.6em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_TORICODE',  <%=LIST_BOX_CLASSIFICATION.LC_CUSTOMER  %>)">
            <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先CD" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_TORICODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            <asp:Label ID="WF_TORICODE_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　販売店　■ -->
        <a style="position:fixed;top:8.6em;left:30.5em; width:32em;" ondblclick="Field_DBclick('WF_STORICODE',  <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE  %>)">
            <asp:Label ID="WF_STORICODE_L" runat="server" Text="販売店" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_STORICODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            <asp:Label ID="WF_STORICODE_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　売上計上基準　■ -->
        <a style="position:fixed;top:8.6em;left:59.5em; width:32em;" ondblclick="Field_DBclick('WF_URIKBN',  <%=LIST_BOX_CLASSIFICATION.LC_URIKBN  %>)">
            <asp:Label ID="WF_URIKBN_L" runat="server" Text="売上計上基準" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="False"></asp:Label>
            <asp:TextBox ID="WF_URIKBN" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
            <asp:Label ID="WF_URIKBN_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>
        </p>

        <p id="KEY_LINE_6">
        <!-- ■　業務車番　■ -->
        <a style="position:fixed;top:10.0em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_GSHABAN',  <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>)">
            <asp:Label ID="WF_GSHABAN_L" runat="server" Text="業務車番" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_GSHABAN" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>

        <!-- ■　コンテナシャーシ　■ -->
        <a style="position:fixed;top:10.0em;left:30.5em; width:32em;" ondblclick="Field_DBclick('WF_CONTCHASSIS',  <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>)">
            <asp:Label ID="WF_CONTCHASSIS_L" runat="server" Text="ｺﾝﾃﾅｼｬｰｼ" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_CONTCHASSIS" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            <asp:Label ID="WF_CONTCHASSIS_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　車腹　■ -->
        <a style="position:fixed;top:10.0em;left:59.5em; width:32em;">
            <asp:Label ID="WF_SHAFUKU_L" runat="server" Text="車腹" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="False"></asp:Label>
            <asp:TextBox ID="WF_SHAFUKU" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>
        </p>

        <p id="KEY_LINE_7">
        <!-- ■　統一車番（前）　■ -->
        <a style="position:fixed;top:11.4em;left:3em; width:32em;">
            <asp:Label ID="WF_TSHABANF_L" runat="server" Text="統一車番（前）" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
            <asp:TextBox ID="WF_TSHABANF" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
            <asp:Label ID="WF_TSHABANF_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　統一車番（後）　■ -->
        <a style="position:fixed;top:11.4em;left:30.5em; width:32em;">
            <asp:Label ID="WF_TSHABANB_L" runat="server" Text="統一車番（後）" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
            <asp:TextBox ID="WF_TSHABANB" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
            <asp:Label ID="WF_TSHABANB_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　統一車番（後）２　■ -->
        <a style="position:fixed;top:11.4em;left:59.5em; width:32em;">
            <asp:Label ID="WF_TSHABANB2_L" runat="server" Text="統一車番（後）２" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
            <asp:TextBox ID="WF_TSHABANB2" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
            <asp:Label ID="WF_TSHABANB2_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>
        </p>

        <p id="KEY_LINE_8">
        <!-- ■　積置区分　■ -->
        <a style="position:fixed;top:12.8em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_TUMIOKIKBN',  <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE  %>)">
            <asp:Label ID="WF_TUMIOKIKBN_L" runat="server" Text="積置区分" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_TUMIOKIKBN" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            <asp:Label ID="WF_TUMIOKIKBN_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　トリップ　■ -->
        <a style="position:fixed;top:12.8em;left:30.5em; width:32em;">
            <asp:Label ID="WF_TRIPNO_L" runat="server" Text="トリップ" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="False"></asp:Label>
            <asp:TextBox ID="WF_TRIPNO" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>

        <!-- ■　ドロップ　■ -->
        <a style="position:fixed;top:12.8em;left:59.5em; width:32em;">
            <asp:Label ID="WF_DROPNO_L" runat="server" Text="ドロップ" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="False"></asp:Label>
            <asp:TextBox ID="WF_DROPNO" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>
        </p>

        <p id="KEY_LINE_9">
        <!-- ■　乗務員　■ -->
        <a style="position:fixed;top:14.2em;left:3em; width:32em;" ondblclick="Field_DBclick('WF_STAFFCODE',  <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>)">
            <asp:Label ID="WF_STAFFCODE_L" runat="server" Text="乗務員" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　副乗務員　■ -->
        <a style="position:fixed;top:14.2em;left:30.5em; width:32em;" ondblclick="Field_DBclick('WF_SUBSTAFFCODE',  <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>)">
            <asp:Label ID="WF_SUBSTAFFCODE_L" runat="server" Text="副乗務員" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
            <asp:TextBox ID="WF_SUBSTAFFCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            <asp:Label ID="WF_SUBSTAFFCODE_TEXT" runat="server" Height="1.2em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
        </a>

        <!-- ■　出勤時間　■ -->
        <a style="position:fixed;top:14.2em;left:59.5em; width:32em;">
            <asp:Label ID="WF_STTIME_L" runat="server" Text="出勤時間" Height="1.2em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="False"></asp:Label>
            <asp:TextBox ID="WF_STTIME" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
        </a>
        </p>
        </div>

        <!-- DETAIL画面 -->
        <asp:MultiView ID="WF_DetailMView" runat="server">
            <asp:View ID="WF_DView1" runat="server"  >

                <span class="WF_DViewRep1_Area" id="WF_DViewRep1_Area">
                    <asp:Repeater ID="WF_DViewRep1" runat="server">
                        <HeaderTemplate>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <table>
                            <tr>
                            <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                            <td>
                                <asp:TextBox ID="WF_Rep1_MEISAINO" runat="server"></asp:TextBox>  
                                <asp:TextBox ID="WF_Rep1_LINEPOSITION" runat="server"></asp:TextBox>  
                            </td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_4" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_4"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_4"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_4"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_4"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_4" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_4"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_5" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_5"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_5"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_5"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_5"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_5" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_5"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_6" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_6"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_6"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_6"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_6"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_6" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_6"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            </tr>
                            </table>
                            <asp:Label ID="WF_Rep1_LINE" runat="server" Height="1px" Width="184em" style="display:none; border-bottom:solid; border-width:2px; border-color:blue;"></asp:Label>
                        </ItemTemplate>
                        <FooterTemplate>
                        </FooterTemplate>
                    </asp:Repeater>
                </span>
            </asp:View>
        </asp:MultiView>
    </div>  

    <%-- rightview --%>
    <MSINC:rightview id="rightview" runat="server" />
    <%-- leftview --%>
    <MSINC:leftview id="leftview" runat="server" />

    <!-- leftview 画面独自 -->
    <div class="leftbox" id="leftbox">
        <div class="button" id="button" style="position:relative;left:0.5em;top:0.8em;">
            <input type="button" id="WF_ButtonSel" value="　選　択　"  onclick="ButtonClick('WF_ButtonSel');" />
            <input type="button" id="WF_ButtonCan" value="キャンセル"  onclick="ButtonClick('WF_ButtonCan');" />
        </div><br />
            
        <asp:MultiView ID="WF_LeftMView" runat="server">

            <!-- 　業務車番　 -->
            <asp:View id="LeftView1" runat="server" >
                <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="TableDBclick()">
                    <span class="WF_TableArea">
                        <asp:Repeater ID="WF_GSHABAN_Rep" runat="server">
                            <HeaderTemplate>
                                    <asp:Table ID="WF_GSHABAN_HeadTable" runat="server" cellspacing="0" rules="all" border="1" CssClass="WF_HeaderArea">
                                    <asp:TableRow  runat="server" CssClass="WF_TEXT_CENTER">
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell1"   runat="server" style="width:8.4em;" Text='業務車番' RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell2"   runat="server" style="width:8.4em;" Text='油種'     RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell3"   runat="server" style="width:8.4em;" Text='車腹'     RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell4"   runat="server" style="width:8.4em;" Text='荷主'     RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH"   runat="server" style="width:8.4em;" Text='配車状況' ColumnSpan="8"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCell5"   runat="server" style="width:8.4em;" Text='運休'     RowSpan="2"></asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow  runat="server" CssClass="WF_TEXT_CENTER">
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_1" runat="server" style="width:1.8em;" Text='1'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_2" runat="server" style="width:1.8em;" Text='2'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_3" runat="server" style="width:1.8em;" Text='3'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_4" runat="server" style="width:1.8em;" Text='4'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_5" runat="server" style="width:1.8em;" Text='5'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_6" runat="server" style="width:1.8em;" Text='6'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_7" runat="server" style="width:1.8em;" Text='7'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_HeadCellH_8" runat="server" style="width:1.8em;" Text='8'></asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                            </HeaderTemplate>

                            <ItemTemplate>
                                    <asp:Table ID="WF_GSHABAN_ItemTable" runat="server" cellspacing="0" rules="all" border="1" CssClass="WF_DetialArea">
                                    <asp:TableRow ID="WF_GSHABAN_Items" runat="server">
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell6"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOSTATUS")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell7"   runat="server" style="width:7.0em;" Text='<%# Eval("LICNPLTNOF")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell8"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO1")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell9"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO2")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell10"  runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO3")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell11"  runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO4")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell12"  runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO5")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell13"  runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOINFO6")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell14"  runat="server" style="width:7.0em;" Text='<%# Eval("SHAFUKU")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell5"  runat="server" style="width:7.0em;" Text='<%# Eval("TSHABANF")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell6"  runat="server" style="width:7.0em;" Text='<%# Eval("TSHABANB")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell7"  runat="server" style="width:7.0em;" Text='<%# Eval("TSHABANB2")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell8"   runat="server" style="width:7.0em;" Text='<%# Eval("LICNPLTNOB")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_TableCell9"   runat="server" style="width:7.0em;" Text='<%# Eval("LICNPLTNOB2")%>' hidden="hidden"></asp:TableCell>

                                        <asp:TableCell ID="WF_GSHABAN_ItemCell1"   runat="server" style="width:7.0em;" Text='<%# Eval("GSHABAN")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell2"   runat="server" style="width:7.0em;" Text='<%# Eval("OILTYPENAME")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell3"   runat="server" style="width:7.0em;" Text='<%# Eval("SHAFUKU")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell4"   runat="server" style="width:7.0em;" Text='<%# Eval("OWNCODENAME")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_1" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS1")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_2" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS2")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_3" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS3")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_4" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS4")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_5" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS5")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_6" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS6")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_7" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS7")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCellH_8" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS8")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_GSHABAN_ItemCell5"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOSTATUSNAME")%>'></asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                            </ItemTemplate>
                            <FooterTemplate>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>
                </a>
            </asp:View>

            <!-- 　コンテナシャーシ　 -->
                <asp:View id="LeftView2" runat="server" >
                <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="TableDBclick()">
                    <span class="WF_TableArea">
                        <asp:Repeater ID="WF_CONTCHASSIS_Rep" runat="server">
                            <HeaderTemplate>
                                    <asp:Table ID="WF_CONTCHASSIS_HeadTable" runat="server" cellspacing="0" rules="all" border="1" CssClass="WF_HeaderArea">
                                    <asp:TableRow  runat="server" CssClass="WF_TEXT_CENTER">
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCell1"   runat="server" style="width:8.4em;" Text='業務車番' RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCell2"   runat="server" style="width:8.4em;" Text='油種'     RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCell3"   runat="server" style="width:8.4em;" Text='車腹'     RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCell4"   runat="server" style="width:8.4em;" Text='荷主'     RowSpan="2"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCellH"   runat="server" style="width:8.4em;" Text='配車状況' ColumnSpan="8"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCell5"   runat="server" style="width:8.4em;" Text='運休'     RowSpan ="2"></asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow  runat="server" CssClass="WF_TEXT_CENTER">
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCellH_1" runat="server" style="width:1.8em;" Text='1'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCellH_2" runat="server" style="width:1.8em;" Text='2'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCellH_3" runat="server" style="width:1.8em;" Text='3'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCellH_4" runat="server" style="width:1.8em;" Text='4'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCellH_5" runat="server" style="width:1.8em;" Text='5'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCellH_6" runat="server" style="width:1.8em;" Text='6'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCellH_7" runat="server" style="width:1.8em;" Text='7'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_HeadCellH_8" runat="server" style="width:1.8em;" Text='8'></asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                            </HeaderTemplate>

                            <ItemTemplate>
                                    <asp:Table ID="WF_CONTCHASSIS_ItemTable" runat="server" cellspacing="0" rules="all" border="1" CssClass="WF_DetialArea">
                                    <asp:TableRow ID="WF_CONTCHASSIS_Items" runat="server">
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCell6"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOSTATUS")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCell7"   runat="server" style="width:7.0em;" Text='<%# Eval("LICNPLTNO")%>' hidden="hidden"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCell1"   runat="server" style="width:7.0em;" Text='<%# Eval("GSHABAN")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCell2"   runat="server" style="width:7.0em;" Text='<%# Eval("OILTYPENAME")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCell3"   runat="server" style="width:7.0em;" Text='<%# Eval("SHAFUKU")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCell4"   runat="server" style="width:7.0em;" Text='<%# Eval("OWNCODENAME")%>'></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCellH_1" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS1")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCellH_2" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS2")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCellH_3" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS3")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCellH_4" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS4")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCellH_5" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS5")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCellH_6" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS6")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCellH_7" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS7")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCellH_8" runat="server" style="width:1.8em;" Text='<%# Eval("HSTATUS8")%>' CssClass="WF_TEXT_CENTER"></asp:TableCell>
                                        <asp:TableCell ID="WF_CONTCHASSIS_ItemCell5"   runat="server" style="width:7.0em;" Text='<%# Eval("SHARYOSTATUSNAME")%>'></asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                            </ItemTemplate>
                            <FooterTemplate>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>
                </a>
            </asp:View>

        </asp:MultiView>

    </div>

    <div hidden="hidden">
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>         <!-- GridViewダブルクリック -->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>        <!-- GridView表示位置フィールド -->

        <input id="WF_ButtonClick" runat="server" value=""  type="text" />              <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />            <!-- 権限 -->

            
        <asp:ListBox ID="WF_ListGSHABAN" runat="server"></asp:ListBox>                  <!-- List業務車番 -->
        <asp:ListBox ID="WF_ListKSHABAN" runat="server"></asp:ListBox>                  <!-- List光英車番 -->
        <asp:ListBox ID="WF_ListSHARYOINFO1" runat="server"></asp:ListBox>              <!-- List車両情報１ -->
        <asp:ListBox ID="WF_ListSHARYOINFO2" runat="server"></asp:ListBox>              <!-- List車両情報２ -->
        <asp:ListBox ID="WF_ListSHARYOINFO3" runat="server"></asp:ListBox>              <!-- List車両情報３ -->
        <asp:ListBox ID="WF_ListSHARYOINFO4" runat="server"></asp:ListBox>              <!-- List車両情報４ -->
        <asp:ListBox ID="WF_ListSHARYOINFO5" runat="server"></asp:ListBox>              <!-- List車両情報５ -->
        <asp:ListBox ID="WF_ListSHARYOINFO6" runat="server"></asp:ListBox>              <!-- List車両情報６ -->
        <asp:ListBox ID="WF_ListOILTYPE" runat="server"></asp:ListBox>                  <!-- List車両油種 -->
        <asp:ListBox ID="WF_ListOILTYPENAME" runat="server"></asp:ListBox>              <!-- List車両油種名 -->
        <asp:ListBox ID="WF_ListSHAFUKU" runat="server"></asp:ListBox>                  <!-- List車腹 -->
        <asp:ListBox ID="WF_ListOWNCODE" runat="server"></asp:ListBox>                  <!-- List車両荷主 -->
        <asp:ListBox ID="WF_ListOWNCODENAME" runat="server"></asp:ListBox>              <!-- List車両荷主名称 -->
        <asp:ListBox ID="WF_ListSHARYOSTATUS" runat="server"></asp:ListBox>             <!-- List車両状態 -->
        <asp:ListBox ID="WF_ListSHARYOSTATUSNAME" runat="server"></asp:ListBox>         <!-- List車両状態名称 -->
        <asp:ListBox ID="WF_ListLICNPLTNOF" runat="server"></asp:ListBox>               <!-- List登録車番前 -->
        <asp:ListBox ID="WF_ListLICNPLTNOB" runat="server"></asp:ListBox>               <!-- List登録車番後 -->
        <asp:ListBox ID="WF_ListLICNPLTNOB2" runat="server"></asp:ListBox>              <!-- List登録車番後２ -->
        <asp:ListBox ID="WF_ListTSHABANF" runat="server"></asp:ListBox>                 <!-- List統一車番前 -->
        <asp:ListBox ID="WF_ListTSHABANB" runat="server"></asp:ListBox>                 <!-- List統一車番後 -->
        <asp:ListBox ID="WF_ListTSHABANB2" runat="server"></asp:ListBox>                <!-- List統一車番後２ -->
        <asp:ListBox ID="WF_ListHPRSINSNYMDF" runat="server"></asp:ListBox>             <!-- List統一次回容器検査年月日前 -->
        <asp:ListBox ID="WF_ListHPRSINSNYMDB" runat="server"></asp:ListBox>             <!-- List統一次回容器検査年月日後 -->
        <asp:ListBox ID="WF_ListHPRSINSNYMDB2" runat="server"></asp:ListBox>            <!-- List統一次回容器検査年月日後2 -->
        <asp:ListBox ID="WF_ListLICNYMDF" runat="server"></asp:ListBox>                 <!-- List統一車検有効年月日前 -->
        <asp:ListBox ID="WF_ListLICNYMDB" runat="server"></asp:ListBox>                 <!-- List統一車検有効年月日後 -->
        <asp:ListBox ID="WF_ListLICNYMDB2" runat="server"></asp:ListBox>                <!-- List統一車検有効年月日後2 -->

        <asp:ListBox ID="WF_ListGSHABAN_CONT" runat="server"></asp:ListBox>             <!-- Listコンテナ業務車番 -->
        <asp:ListBox ID="WF_ListOILTYPE_CONT" runat="server"></asp:ListBox>             <!-- Listコンテナ油種 -->
        <asp:ListBox ID="WF_ListOILTYPENAME_CONT" runat="server"></asp:ListBox>         <!-- Listコンテナ油種名称 -->
        <asp:ListBox ID="WF_ListSHAFUKU_CONT" runat="server"></asp:ListBox>             <!-- Listコンテナ車腹 -->
        <asp:ListBox ID="WF_ListOWNCODE_CONT" runat="server"></asp:ListBox>             <!-- Listコンテナ車両荷主 -->
        <asp:ListBox ID="WF_ListOWNCODENAME_CONT" runat="server"></asp:ListBox>         <!-- Listコンテナ車両荷主名称 -->
        <asp:ListBox ID="WF_ListSHARYOSTATUS_CONT" runat="server"></asp:ListBox>        <!-- Listコンテナ車両状態 -->
        <asp:ListBox ID="WF_ListSHARYOSTATUSNAME_CONT" runat="server"></asp:ListBox>    <!-- Listコンテナ車両状態名称 -->
        <asp:ListBox ID="WF_ListLICNPLTNOF_CONT" runat="server"></asp:ListBox>          <!-- Listコンテナ登録車番前 -->
        <asp:ListBox ID="WF_ListLICNPLTNOB_CONT" runat="server"></asp:ListBox>          <!-- Listコンテナ登録車番後 -->


        <asp:TextBox ID="WF_DEFORG" runat="server"></asp:TextBox>                       <!-- 所属部署　 -->
        <asp:TextBox ID="WF_JXORDERID" runat="server"></asp:TextBox>                    <!-- JXオーダー識別ID　 -->

        <input id="WF_FIELD"  runat="server" value=""  type="text" />                   <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD_REP"  runat="server" value=""  type="text" />               <!-- Textbox(Repeater) DBクリックフィールド -->

        <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>            <!-- Leftbox Mview切替 -->
        <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />             <!-- Leftbox 開閉 -->

        <input id="WF_RightViewChange" runat="server" value="" type="text"/>            <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value=""  type="text" />             <!-- Rightbox 開閉 -->

        <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />           <!-- Leftbox DBクリックフィールド(行位置) -->

        <input id="WF_REP_LINECNT"  runat="server" value=""  type="text" />             <!-- Repeater 行位置 -->
        <input id="WF_REP_POSITION"  runat="server" value=""  type="text" />            <!-- Repeater 行位置 -->
        <input id="WF_REP_Change"  runat="server" value=""  type="text" />              <!-- Repeater 変更監視 -->
        <input id="WF_REP_ROWSCNT" runat="server" value=""  type="text" />              <!-- Repeaterの１明細の行数 -->
        <input id="WF_REP_COLSCNT" runat="server" value=""  type="text" />              <!-- Repeaterの列数 -->
            
        <input id="WF_IsHideDetailBox"  runat="server" value="1" type="text" />         <!-- 詳細画面非表示フラグ -->
        <input id="WF_IsHideKoueiButton"  runat="server" value="0" type="text" />       <!-- 光英受信ボタン非表示フラグ -->
        <input id="WF_IsKoueiData"  runat="server" value="0" type="text" />             <!-- 詳細画面表示光英データフラグ -->
        <asp:ListBox ID="WF_KoueiLoadFile" runat="server"></asp:ListBox>                <!-- List光栄読込中ファイル -->
    
        <input id="WF_PrintURL" runat="server" value=""  type="text" />                 <!-- Textbox Print URL -->

    </div>

    <!-- Work レイアウト -->
    <LSINC:work id="work" runat="server" />

</asp:Content>
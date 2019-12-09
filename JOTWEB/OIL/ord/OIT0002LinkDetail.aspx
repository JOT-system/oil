<%@ Page Title="OIT0002D" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0002LinkDetail.aspx.vb" Inherits="JOTWEB.OIT0002LinlDetail" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0002DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0002D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0002D.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIT0002D" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <div class="Operation" style="margin-left: 3em; margin-top: 0.5em; height: 1.8em;">

            <!-- ○ 固定項目 ○ -->
            <!-- ボタン -->
            <a style="position:fixed;top:2.8em;left:0.5em;">
                <input type="button" id="WF_ButtonRegister" value="登録"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>

            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>

            <!-- ○ 変動項目 ○ -->
            <!-- 会社コード -->
            <a style="position:fixed; top:7.7em; left:4em; font-weight:bold; text-decoration:underline;display:none">会社コード</a>

            <a class="ef" style="position:fixed; top:7.7em; left:18em;display:none" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a style="position:fixed; top:7.7em; left:27em;display:none">
                <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>
            <!-- 運用部署 -->
            <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;display:none">運用部署</a>

            <a class="ef" style="position:fixed; top:9.9em; left:18em;display:none" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
                <asp:TextBox ID="WF_UORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a style="position:fixed; top:9.9em; left:27em;display:none">
                <asp:Label ID="WF_UORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ■　受注営業所　■ -->
            <a style="position:fixed; top:4.4em; left:4em; font-weight:bold;">受注営業所</a>
            <a class="ef" style="position:fixed; top:4.4em; left:11em;" ondblclick="Field_DBclick('TxtOrderOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtOrderOffice');">
                <asp:TextBox ID="TxtOrderOffice" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　本社列車　■ -->
            <a style="position:fixed; top:7.7em; left:4em; font-weight:bold; text-decoration:underline;">本社列車</a>
            <a class="ef" style="position:fixed; top:7.7em; left:11em;" ondblclick="Field_DBclick('TxtHeadOfficeTrain', <%=LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER%>);" onchange="TextBox_change('TxtHeadOfficeTrain');">
                <asp:TextBox ID="TxtHeadOfficeTrain" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　発駅　■ -->
            <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;">発駅</a>
            <a class="ef" style="position:fixed; top:9.9em; left:11em;" ondblclick="Field_DBclick('TxtDepstation', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtDepstation');">
                <asp:TextBox ID="TxtDepstation" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a style="position:fixed; top:9.9em; left:20em;">
                <asp:Label ID="LblDepstationName" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>
            <!-- ■　着駅　■ -->
            <a style="position:fixed; top:9.9em; left:38em; font-weight:bold; text-decoration:underline;">着駅</a>
            <a class="ef" style="position:fixed; top:9.9em; left:43em;" ondblclick="Field_DBclick('TxtArrstation', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtArrstation');">
                <asp:TextBox ID="TxtArrstation" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a style="position:fixed; top:9.9em; left:52em;">
                <asp:Label ID="LblArrstationName" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ■　(予定)積込日　■ -->
            <a style="position:fixed; top:12.1em; left:4em; font-weight:bold; text-decoration:underline;">(予定)積込日</a>
            <a class="ef" style="position:fixed; top:12.1em; left:11em;" ondblclick="Field_DBclick('TxtLoadingDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtLoadingDate" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)発日　■ -->
            <a style="position:fixed; top:12.1em; left:24em; font-weight:bold; text-decoration:underline;">発日</a>
            <a class="ef" style="position:fixed; top:12.1em; left:27em;" ondblclick="Field_DBclick('TxtDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtDepDate" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)積車着日　■ -->
            <a style="position:fixed; top:12.1em; left:38em; font-weight:bold; text-decoration:underline;">積車着日</a>
            <a class="ef" style="position:fixed; top:12.1em; left:43em;" ondblclick="Field_DBclick('TxtArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtArrDate" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)受入日　■ -->
            <a style="position:fixed; top:12.1em; left:55em; font-weight:bold; text-decoration:underline;">受入日</a>
            <a class="ef" style="position:fixed; top:12.1em; left:59em;" ondblclick="Field_DBclick('TxtAccDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtAccDate" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>

            <!-- ■　油種別タンク車数(車)　■ -->
            <a style="position:fixed; top:16.5em; left:4em; font-weight:bold;">油種別タンク車数(車)</a>

            <!-- ■　合計車数　■ -->
            <a style="position:fixed; top:15.0em; left:19em; font-weight:bold;">合計</a>
            <a class="ef" style="position:fixed; top:16.5em; left:19em;">
                <asp:TextBox ID="TxtTotalTank" runat="server" Height="5.6em" Width="5em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（ハイオク）　■ -->
            <a style="position:fixed; top:15.0em; left:23.3em; font-weight:bold;">ハイオク</a>
            <a class="ef" style="position:fixed; top:16.5em; left:23.3em;">
                <asp:TextBox ID="TxtHTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（レギュラー）　■ -->
            <a style="position:fixed; top:18.5em; left:23.3em; font-weight:bold;">レギュラー</a>
            <a class="ef" style="position:fixed; top:20.0em; left:23.3em;">
                <asp:TextBox ID="TxtRTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（灯油）　■ -->
            <a style="position:fixed; top:15.0em; left:29.3em; font-weight:bold;">灯油</a>
            <a class="ef" style="position:fixed; top:16.5em; left:29.3em;">
                <asp:TextBox ID="TxtTTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（未添加灯油）　■ -->
            <a style="position:fixed; top:18.5em; left:29.3em; font-weight:bold;">未添加灯油</a>
            <a class="ef" style="position:fixed; top:20.0em; left:29.3em;">
                <asp:TextBox ID="TxtMTTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（軽油）　■ -->
            <a style="position:fixed; top:15.0em; left:35.0em; font-weight:bold;">軽油</a>
            <a class="ef" style="position:fixed; top:16.5em; left:35.0em;">
                <asp:TextBox ID="TxtKTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（３号軽油）　■ -->
            <a style="position:fixed; top:18.5em; left:35.0em; font-weight:bold;">３号軽油</a>
            <a class="ef" style="position:fixed; top:20.0em; left:35.0em;">
                <asp:TextBox ID="TxtK3Tank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（５号軽油）　■ -->
            <a style="position:fixed; top:15.0em; left:40.7em; font-weight:bold;">５号軽油</a>
            <a class="ef" style="position:fixed; top:16.5em; left:40.7em;">
                <asp:TextBox ID="TxtK5Tank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（１０号軽油）　■ -->
            <a style="position:fixed; top:18.5em; left:40.7em; font-weight:bold;">１０号軽油</a>
            <a class="ef" style="position:fixed; top:20.0em; left:40.7em;">
                <asp:TextBox ID="TxtK10Tank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（LSA）　■ -->
            <a style="position:fixed; top:15.0em; left:46.4em; font-weight:bold;">ＬＳＡ</a>
            <a class="ef" style="position:fixed; top:16.5em; left:46.4em;">
                <asp:TextBox ID="TxtLTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（A重油）　■ -->
            <a style="position:fixed; top:18.5em; left:46.4em; font-weight:bold;">Ａ重油</a>
            <a class="ef" style="position:fixed; top:20.0em; left:46.4em;">
                <asp:TextBox ID="TxtATank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
        </div>
    </div>

        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox" style="overflow-y: auto;">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <a>
                    <input type="button" id="WF_UPDATE" value="表更新" style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
                </a>
                <a>
                    <input type="button" id="WF_CLEAR" value="クリア" style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
                </a>
            </div>

            <div class="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- 選択No -->
                    <a>
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 削除フラグ -->
                    <a class="ef" ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_2">
                    <!-- 貨車連結順序表№ -->
                    <a class="ef">
                        <asp:Label ID="WF_LINKNO_L" runat="server" Text="貨車連結順序表№" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_LINKNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LINKNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 貨車連結順序表明細№ -->
                    <a class="ef">
                        <asp:Label ID="WF_LINKDETAILNO_L" runat="server" Text="貨車連結順序表明細№" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LINKDETAILNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LINKDETAILNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_3">
                    <!-- ステータス -->
                    <a class="ef">
                        <asp:Label ID="WF_STATUS_L" runat="server" Text="ステータス" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_STATUS" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_STATUS_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 情報 -->
                    <a class="ef">
                        <asp:Label ID="WF_INFO_L" runat="server" Text="情報" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_INFO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_INFO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 前回オーダー№ -->
                    <a class="ef">
                        <asp:Label ID="WF_PREORDERNO_L" runat="server" Text="前回オーダー№" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_PREORDERNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_PREORDERNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 本線列車 -->
                    <a class="ef">
                        <asp:Label ID="WF_TRAINNO_L" runat="server" Text="本線列車" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_TRAINNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_TRAINNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 登録営業所コード -->
                    <a class="ef">
                        <asp:Label ID="WF_OFFICECODE_L" runat="server" Text="登録営業所コード" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OFFICECODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OFFICECODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 空車発駅コード -->
                    <a class="ef">
                        <asp:Label ID="WF_DEPSTATION_L" runat="server" Text="空車発駅コード" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_DEPSTATION" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DEPSTATION_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 空車発駅名 -->
                    <a class="ef">
                        <asp:Label ID="WF_DEPSTATIONNAME_L" runat="server" Text="空車発駅名" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_DEPSTATIONNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DEPSTATIONNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 空車着駅コード -->
                    <a class="ef">
                        <asp:Label ID="WF_RETSTATION_L" runat="server" Text="空車着駅コード" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RETSTATION" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_RETSTATION_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 空車着駅名 -->
                    <a class="ef">
                        <asp:Label ID="WF_RETSTATIONNAME_L" runat="server" Text="空車着駅名" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RETSTATIONNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_RETSTATIONNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 空車着日（予定） -->
                    <a class="ef">
                        <asp:Label ID="WF_EMPARRDATE_L" runat="server" Text="空車着日（予定）" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_EMPARRDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_EMPARRDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 空車着日（実績） -->
                    <a class="ef">
                        <asp:Label ID="WF_ACTUALEMPARRDATE_L" runat="server" Text="空車着日（実績）" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ACTUALEMPARRDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_ACTUALEMPARRDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 入線列車番号 -->
                    <a class="ef">
                        <asp:Label ID="WF_LINETRAINNO_L" runat="server" Text="入線列車番号" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LINETRAINNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LINETRAINNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 入線順 -->
                    <a class="ef">
                        <asp:Label ID="WF_LINEORDER_L" runat="server" Text="入線順" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LINEORDER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LINEORDER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- タンク車№ -->
                    <a class="ef">
                        <asp:Label ID="WF_TANKNUMBER_L" runat="server" Text="タンク車№" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_TANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_TANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 前回油種 -->
                    <a class="ef">
                        <asp:Label ID="WF_PREOILCODE_L" runat="server" Text="前回油種" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_PREOILCODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_PREOILCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
            </div>
        </div>

        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

        <!-- イベント用 -->
        <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
            <!-- GridView DBクリック-->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>
            <!-- GridView表示位置フィールド -->

            <input id="WF_FIELD" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD_REP" runat="server" value="" type="text" />
            <!-- Textbox(Repeater) DBクリックフィールド -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->

            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
            <!-- LeftBox Mview切替 -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
            <!-- LeftBox 開閉 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text" />
            <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value="" type="text" />
            <!-- Rightbox 開閉 -->

            <input id="WF_PrintURL" runat="server" value="" type="text" />
            <!-- Textbox Print URL -->

            <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />
            <!-- 一覧・詳細画面切替用フラグ -->

            <input id="WF_ButtonClick" runat="server" value="" type="text" />
            <!-- ボタン押下 -->
            <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
            <!-- 権限 -->
        </div>
 
</asp:Content>

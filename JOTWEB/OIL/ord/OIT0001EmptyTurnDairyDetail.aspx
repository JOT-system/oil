<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0001EmptyTurnDairyDetail.aspx.vb" Inherits="JOTWEB.OIT0001EmptyTurnDairyDetail" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0001DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0001D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0001D.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="OIT0001D" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <!-- 全体レイアウト　headerbox -->
    <div class="headerboxOnly" id="headerbox">
        <div class="Operation" style="margin-left: 3em; margin-top: 0.5em; height: 1.8em;">
            <!-- 会社 -->
            <asp:Label ID="WF_SEL_CAMPCODE" runat="server" Text="会社" Font-Bold="True" Font-Underline="false" Visible="false"></asp:Label>
            <asp:Label ID="WF_SEL_CAMPNAME" runat="server" Width="12em" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>

            <!-- 運用部署 -->
            <asp:Label ID="WF_SELUORG_L" runat="server" Text="運用部署" Font-Bold="True" Font-Underline="false" Visible="false"></asp:Label>
            <asp:Label ID="WF_SELUORG_TEXT" runat="server" Width="12em" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>

            <!-- ボタン -->
            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>

            <!-- ■　受注営業所　■ -->
            <a style="position:fixed; top:4.4em; left:4em; font-weight:bold;">受注営業所</a>
            <a style="position:fixed; top:4.4em; left:11em;">
                <asp:TextBox ID="TxtOrderOffice" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　本社列車　■ -->
            <a style="position:fixed; top:7.7em; left:4em; font-weight:bold;">本社列車</a>
            <a style="position:fixed; top:7.7em; left:11em;">
                <asp:TextBox ID="TxtHeadOfficeTrain" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　発駅　■ -->
            <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;">発駅</a>
            <a style="position:fixed; top:9.9em; left:11em;" ondblclick="Field_DBclick('TxtDepstation', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtDepstation');">
                <asp:TextBox ID="TxtDepstation" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a style="position:fixed; top:9.9em; left:20em;">
                <asp:Label ID="LblDepstationName" runat="server" Width="17em" CssClass="WF_TEXT">発発発発発発発発発発発発発発発発発発発発発発発発発</asp:Label>
            </a>
            <!-- ■　着駅　■ -->
            <a style="position:fixed; top:9.9em; left:38em; font-weight:bold; text-decoration:underline;">着駅</a>
            <a style="position:fixed; top:9.9em; left:43em;" ondblclick="Field_DBclick('TxtArrstation', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtArrstation');">
                <asp:TextBox ID="TxtArrstation" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a style="position:fixed; top:9.9em; left:52em;">
                <asp:Label ID="LblArrstationName" runat="server" Width="17em" CssClass="WF_TEXT">着着着着着着着着着着着着着着着着着着着着着着着着着</asp:Label>
            </a>

            <!-- ■　(予定)積込日　■ -->
            <a style="position:fixed; top:12.1em; left:4em; font-weight:bold; text-decoration:underline;">(予定)積込日</a>
            <a style="position:fixed; top:12.1em; left:11em;" ondblclick="Field_DBclick('TxtLoadingDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtLoadingDate" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)発日　■ -->
            <a style="position:fixed; top:12.1em; left:24em; font-weight:bold; text-decoration:underline;">発日</a>
            <a style="position:fixed; top:12.1em; left:27em;" ondblclick="Field_DBclick('TxtDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtDepDate" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)積車着日　■ -->
            <a style="position:fixed; top:12.1em; left:38em; font-weight:bold; text-decoration:underline;">積車着日</a>
            <a style="position:fixed; top:12.1em; left:43em;" ondblclick="Field_DBclick('TxtArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtArrDate" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)受入日　■ -->
            <a style="position:fixed; top:12.1em; left:55em; font-weight:bold; text-decoration:underline;">受入日</a>
            <a style="position:fixed; top:12.1em; left:59em;" ondblclick="Field_DBclick('TxtAccDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtAccDate" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>

            <!-- ■　油種別タンク車数(車)　■ -->
            <a style="position:fixed; top:16.5em; left:4em; font-weight:bold;">油種別タンク車数(車)</a>

            <!-- ■　合計車数　■ -->
            <a style="position:fixed; top:15.0em; left:19em; font-weight:bold;">合計</a>
            <a style="position:fixed; top:16.5em; left:19em;">
                <asp:TextBox ID="TxtTotalTank" runat="server" Height="5.6em" Width="5em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（ハイオク）　■ -->
            <a style="position:fixed; top:15.0em; left:23.3em; font-weight:bold;">ハイオク</a>
            <a style="position:fixed; top:16.5em; left:23.3em;">
                <asp:TextBox ID="TxtHTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（レギュラー）　■ -->
            <a style="position:fixed; top:18.5em; left:23.3em; font-weight:bold;">レギュラー</a>
            <a style="position:fixed; top:20.0em; left:23.3em;">
                <asp:TextBox ID="TxtRTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（灯油）　■ -->
            <a style="position:fixed; top:15.0em; left:29.3em; font-weight:bold;">灯油</a>
            <a style="position:fixed; top:16.5em; left:29.3em;">
                <asp:TextBox ID="TxtTTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（未添加灯油）　■ -->
            <a style="position:fixed; top:18.5em; left:29.3em; font-weight:bold;">未添加灯油</a>
            <a style="position:fixed; top:20.0em; left:29.3em;">
                <asp:TextBox ID="TxtMTTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（軽油）　■ -->
            <a style="position:fixed; top:15.0em; left:35.0em; font-weight:bold;">軽油</a>
            <a style="position:fixed; top:16.5em; left:35.0em;">
                <asp:TextBox ID="TxtKTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（３号軽油）　■ -->
            <a style="position:fixed; top:18.5em; left:35.0em; font-weight:bold;">３号軽油</a>
            <a style="position:fixed; top:20.0em; left:35.0em;">
                <asp:TextBox ID="TxtK3Tank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（５号軽油）　■ -->
            <a style="position:fixed; top:15.0em; left:40.7em; font-weight:bold;">５号軽油</a>
            <a style="position:fixed; top:16.5em; left:40.7em;">
                <asp:TextBox ID="TxtK5Tank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（１０号軽油）　■ -->
            <a style="position:fixed; top:18.5em; left:40.7em; font-weight:bold;">１０号軽油</a>
            <a style="position:fixed; top:20.0em; left:40.7em;">
                <asp:TextBox ID="TxtK10Tank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（LSA）　■ -->
            <a style="position:fixed; top:15.0em; left:46.4em; font-weight:bold;">ＬＳＡ</a>
            <a style="position:fixed; top:16.5em; left:46.4em;">
                <asp:TextBox ID="TxtLTank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（A重油）　■ -->
            <a style="position:fixed; top:18.5em; left:46.4em; font-weight:bold;">Ａ重油</a>
            <a style="position:fixed; top:20.0em; left:46.4em;">
                <asp:TextBox ID="TxtATank" runat="server" Height="1.4em" Width="7em" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
        </div>


    </div>

        <!-- 全体レイアウト　detailbox -->
    <div class="detailbox" id="detailbox">
        <div style="height:1em;"></div>
        <div id="divListArea">
            <asp:panel id="pnlListArea" runat="server"></asp:panel>
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
            <!-- GridView DBクリック-->
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
            <!-- GridView表示位置フィールド -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>

            <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD" runat="server" value="" type="text" />
            <!-- Textbox(Repeater) DBクリックフィールド -->
            <input id="WF_FIELD_REP" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text" />

            <!-- LeftBox Mview切替 -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
            <!-- LeftBox 開閉 -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
            <!-- Rightbox Mview切替 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text" />
            <!-- Rightbox 開閉 -->
            <input id="WF_RightboxOpen" runat="server" value="" type="text" />

            <!-- Textbox Print URL -->
            <input id="WF_PrintURL" runat="server" value="" type="text" />

            <!-- 一覧・詳細画面切替用フラグ -->
            <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />

            <!-- ボタン押下 -->
            <input id="WF_ButtonClick" runat="server" value="" type="text" />
            <!-- 権限 -->
            <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
        </div>
</asp:Content>



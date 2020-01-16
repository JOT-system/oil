<%@ Page Title="OIT0002D" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0002LinkDetail.aspx.vb" Inherits="JOTWEB.OIT0002LinkDetail" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0002DH" ContentPlaceHolderID="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIT0002D.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0002D.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
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
            <a style="position:fixed;top:2.8em;left:62.5em;">
                <input type="button" id="WF_ButtonRegister" value="登録"  class="btn-sticky" style="Width:5em" onclick="ButtonClick('WF_ButtonRegister');" />
            </a>

            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="戻る"  class="btn-sticky" style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
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

            <a class="ef" style="position:fixed; top:9.9em; left:18em;display:none" ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
                <asp:TextBox ID="WF_ORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a style="position:fixed; top:9.9em; left:27em;display:none">
                <asp:Label ID="WF_ORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ■　登録営業所　■ -->
            <a id="WF_OFFICECODE_LABEL" class="requiredMark">登録営業所</a>
            <a id="WF_OFFICECODE_ICON" onclick="Field_DBclick('TxtOrderOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);">
                <asp:Image ID="imgOrderOffice" runat="server" ImageUrl="../img/leftbox.png"/>
            </a>
            <a id="WF_OFFICECODE_DUMMY">
                <asp:Label ID="lblOrderOffice_dummy" runat="server"></asp:Label>
            </a>
            <a class="ef" id="WF_OFFICECODE" ondblclick="Field_DBclick('TxtOrderOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtOrderOffice');">
                <asp:TextBox ID="TxtOrderOffice" runat="server" onblur="MsgClear();" MaxLength="20"></asp:TextBox>
            </a>

            <!-- ■　本線列車　■ -->
            <a id="WF_TRAIN_LABEL" class="requiredMark">本線列車</a>
            <a id="WF_TRAIN_ICON" onclick="Field_DBclick('TxtHeadOfficeTrain', <%=LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER%>);">
                <asp:Image runat="server" ImageUrl="../img/leftbox.png"/>
            </a>
            <a class="ef" id="WF_TRAINCODE" ondblclick="Field_DBclick('TxtHeadOfficeTrain', <%=LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER%>);" onchange="TextBox_change('TxtHeadOfficeTrain');">
                <asp:TextBox ID="TxtHeadOfficeTrain" runat="server" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
            </a>
            <!-- ■　利用可能日　■ -->
            <a id="WF_AVAILABLEYMD_LABEL" class="requiredMark">利用可能日</a>
            <a id="WF_AVAILABLEYMD_ICON" onclick="Field_DBclick('AvailableYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:Image runat="server" ImageUrl="../img/calendar.png"/>
            </a>
            <a class="ef" id="WF_AVAILABLEYMD" ondblclick="Field_DBclick('AvailableYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="AvailableYMD" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>

            <!-- ■　空車発駅（着駅）　■ -->
            <a id="WF_DEPSTATION_LABEL" class="requiredMark">空車発駅</a>
            <a id="WF_DEPSTATION_ICON" onclick="Field_DBclick('TxtDepstation', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);">
                <asp:Image runat="server" ImageUrl="../img/leftbox.png"/>
            </a>
            <a class="ef" id="WF_DEPSTATIONCODE" ondblclick="Field_DBclick('TxtDepstation', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtDepstation');">
                <asp:TextBox ID="TxtDepstation" runat="server" onblur="MsgClear();" MaxLength="7"></asp:TextBox>
            </a>
            <a id="WF_DEPSTATIONNAME">
                <asp:Label ID="LblDepstationName" runat="server" CssClass="WF_TEXT"></asp:Label>
            </a>
            <!-- ■　空車着駅（発駅）　■ -->
            <a id="WF_RETSTATION_LABEL" class="requiredMark">空車着駅</a>
            <a id="WF_RETSTATION_ICON" onclick="Field_DBclick('TxtRetstation', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);">
                <asp:Image runat="server" ImageUrl="../img/leftbox.png"/>
            </a>
            <a class="ef" id="WF_RETSTATIONCODE" ondblclick="Field_DBclick('TxtRetstation', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtRetstation');">
                <asp:TextBox ID="TxtRetstation" runat="server" onblur="MsgClear();" MaxLength="7"></asp:TextBox>
            </a>
            <a id="WF_RETSTATIONNAME">
                <asp:Label ID="LblRetstationName" runat="server" CssClass="WF_TEXT"></asp:Label>
            </a>
            
            <!-- ■　(予定)空車着日　■ -->
            <a id="WF_EMPARRDATE_LABEL" class="requiredMark">(予定)空車着日</a>
            <a id="WF_EMPARRDATE_ICON" onclick="Field_DBclick('TxtEmpDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:Image runat="server" ImageUrl="../img/calendar.png"/>
            </a>
            <a class="ef" id="WF_EMPARRDATE" ondblclick="Field_DBclick('TxtEmpDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtEmpDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(実績)空車着日　■ -->
            <a id="WF_ACTUALEMPARRDATE_LABEL">(実績)空車着日</a>
            <a id="WF_ACTUALEMPARRDATE_ICON" onclick="Field_DBclick('TxtActEmpDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:Image runat="server" ImageUrl="../img/calendar.png"/>
            </a>
            <a class="ef" id="WF_ACTUALEMPARRDATE" ondblclick="Field_DBclick('TxtActEmpDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="TxtActEmpDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
        </div>

        <div class="Operation2" style="margin-left: 3em; margin-top: 0.5em; height: 1.8em;">
            <!-- ■　油種別タンク車数(車)　■ -->
            <a id="WF_OILTANKCNT_LABEL">油種別タンク車数(車)</a>

            <!-- ■　合計車数　■ -->
            <a id="WF_TOTALTANK_LABEL">合計</a>
            <a class="ef" id="WF_TOTALTANK">
                <asp:TextBox ID="TxtTotalTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（ハイオク）　■ -->
            <a id="WF_HTANK_LABEL">ハイオク</a>
            <a class="ef" id="WF_HTANK">
                <asp:TextBox ID="TxtHTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（レギュラー）　■ -->
            <a id="WF_RTANK_LABEL">レギュラー</a>
            <a class="ef" id="WF_RTANK">
                <asp:TextBox ID="TxtRTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（灯油）　■ -->
            <a id="WF_TTANK_LABEL">灯油</a>
            <a class="ef" id="WF_TTANK">
                <asp:TextBox ID="TxtTTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（未添加灯油）　■ -->
            <a id="WF_MTTANK_LABEL">未添加灯油</a>
            <a class="ef" id="WF_MTTANK">
                <asp:TextBox ID="TxtMTTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（軽油）　■ -->
            <a id="WF_KTANK_LABEL">軽油</a>
            <a class="ef" id="WF_KTANK">
                <asp:TextBox ID="TxtKTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（３号軽油）　■ -->
            <a id="WF_K3TANK_LABEL">３号軽油</a>
            <a class="ef" id="WF_K3TANK">
                <asp:TextBox ID="TxtK3Tank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（５号軽油）　■ -->
            <a id="WF_K5TANK_LABEL">５号軽油</a>
            <a class="ef" id="WF_K5TANK">
                <asp:TextBox ID="TxtK5Tank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（１０号軽油）　■ -->
            <a id="WF_K10TANK_LABEL">１０号軽油</a>
            <a class="ef" id="WF_K10TANK">
                <asp:TextBox ID="TxtK10Tank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>

            <!-- ■　車数（LSA）　■ -->
            <a id="WF_LTANK_LABEL">ＬＳＡ</a>
            <a class="ef" id="WF_LTANK">
                <asp:TextBox ID="TxtLTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
            <!-- ■　車数（A重油）　■ -->
            <a id="WF_ATANK_LABEL">Ａ重油</a>
            <a class="ef" id="WF_ATANK">
                <asp:TextBox ID="TxtATank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
            </a>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div class="detailbox" id="detailbox">
        <div style="height:3em;">
        <div id="detailbuttonbox" class="detailbuttonbox">
            <!-- ボタン -->
            <a style="left:0.5em;">
                <input type="button" id="WF_ButtonALLSELECT"     class="btn-sticky" value="全選択"   style="Width:5em" onclick="ButtonClick('WF_ButtonALLSELECT');" />
            </a>
            <a style="left:0.5em;">
                <input type="button" id="WF_ButtonSELECT_LIFTED" class="btn-sticky" value="選択解除" style="Width:5em" onclick="ButtonClick('WF_ButtonSELECT_LIFTED');" />
            </a>
            <a style="left:0.5em;">
                <input type="button" id="WF_ButtonLINE_LIFTED"   class="btn-sticky" value="行削除"   style="Width:5em" onclick="ButtonClick('WF_ButtonLINE_LIFTED');" />
            </a>
            <a style="left:0.5em;">
                <input type="button" id="WF_ButtonLINE_ADD"      class="btn-sticky" value="行追加"   style="Width:5em" onclick="ButtonClick('WF_ButtonLINE_ADD');" />
            </a>
            <a style="left:0.5em;">
                <input type="button" id="WF_ButtonCSV"           class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
            </a>
            <a style="left:43.5em;">
                <input type="button" id="WF_ButtonUPDATE"        class="btn-sticky" value="明細更新" style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
        </div>
        </div>
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
        <div style="display:none;">
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

            <!-- 画面表示切替 -->
            <input id="WF_DISP" runat="server" value="" type="text" />
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

            <!-- 新規・更新切替用フラグ -->
            <input id="WF_CREATEFLG" runat="server" value="" type="text" />

            <!-- パネルロック・解除フラグ -->
            <input id="WF_PANELFLG" runat="server" value="" type="text" />

            <!-- TBLアップデートエラーフラグ -->
            <input id="WF_UPDERRFLG" runat="server" value="" type="text" />
        </div>
</asp:Content>

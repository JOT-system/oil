<%@ Page Title="OIT0001D" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0001EmptyTurnDairyDetail.aspx.vb" Inherits="JOTWEB.OIT0001EmptyTurnDairyDetail" %>
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
    <div class="headerbox" id="headerbox">
        <!-- ○ 固定項目 ○ -->
        <!-- ボタン -->
        <div class="actionButtonBox">
            <div class="leftSide">
            </div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="油種登録" onclick="ButtonClick('WF_ButtonINSERT');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div>
        
        <div style="display:none;"> <!-- 非表示にしてるようなので余計なスタイルサイズ指定を取って
                                         一旦この枠に退避 -->
            <!-- 会社コード -->
            <a>会社コード</a>
            <a class="ef" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                <asp:TextBox ID="WF_CAMPCODE" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </a>
            <!-- 運用部署 -->
            <a>運用部署</a>

            <a class="ef" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
                <asp:TextBox ID="WF_UORG" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_UORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ↓いらん -->
            <a id="WF_OFFICECODE_DUMMY">
                <asp:Label ID="lblOrderOffice_dummy" runat="server"></asp:Label>
            </a>
        </div>
        <!-- ○ 変動項目 ○ -->
        <div class="commonHeaderInput"> <!-- 共通ヘッダー入力部 1行4列設定 -->
            <!-- ■　受注営業所　■ -->
            <span>
                <a id="WF_OFFICECODE_LABEL" class="requiredMark">受注営業所</a>
                <a class="ef" id="WF_OFFICECODE" ondblclick="Field_DBclick('TxtOrderOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtOrderOffice');">
                    <asp:TextBox ID="TxtOrderOffice" runat="server" onblur="MsgClear();" CssClass="boxIcon" MaxLength="20"></asp:TextBox>
                </a>
            </span>
            <span></span><span></span><span></span><span></span>
            <!-- ■　本線列車　■ -->
            <span>
                <a id="WF_HEADOFFICETRAIN_LABEL" class="requiredMark">本線列車</a>
                <a class="ef" id="WF_HEADOFFICETRAINCODE" ondblclick="Field_DBclick('TxtHeadOfficeTrain', <%=LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER%>);" onchange="TextBox_change('TxtHeadOfficeTrain');">
                    <asp:TextBox ID="TxtHeadOfficeTrain" runat="server" onblur="MsgClear();" CssClass="boxIcon iconOnly" readonly="true" MaxLength="4"></asp:TextBox>
                    <asp:TextBox ID="TxtHeadOfficeTrainName" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                </a>
            </span>
            <span></span><span></span><span></span><span></span>
            <!-- ■　発駅　■ -->
            <span>
                <a id="WF_DEPSTATION_LABEL" class="requiredMark">発駅</a>
                <a class="ef" id="WF_DEPSTATIONCODE" ondblclick="Field_DBclick('TxtDepstation', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtDepstation');">
                    <asp:TextBox ID="TxtDepstation" runat="server" onblur="MsgClear();" CssClass="boxIcon iconOnly" readonly="true" MaxLength="7"></asp:TextBox>
                </a>

            </span>
            <span>
                <a id="WF_DEPSTATIONNAME">
                    <asp:Label ID="LblDepstationName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </span>
            <!-- ■　着駅　■ -->
            <span>
                <a id="WF_ARRSTATION_LABEL" class="requiredMark">着駅</a>
                <a class="ef" id="WF_ARRSTATION" ondblclick="Field_DBclick('TxtArrstation', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtArrstation');">
                    <asp:TextBox ID="TxtArrstation" runat="server" onblur="MsgClear();" CssClass="boxIcon iconOnly" readonly="true" MaxLength="7"></asp:TextBox>
                </a>

            </span>
            <span>
                <a id="WF_ARRSTATIONNAME">
                    <asp:Label ID="LblArrstationName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </span>
            <span></span>
            <!-- ■　(予定)積込日　■ -->
            <span>
                <a id="WF_LOADINGDATE_LABEL" class="requiredMark">(予定)積込日</a>
                <a class="ef" id="WF_LOADINGDATE" ondblclick="Field_DBclick('TxtLoadingDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtLoadingDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                </a>
            </span>
            <!-- ■　(予定)発日　■ -->
            <span>
                <a id="WF_DEPDATE_LABEL" class="requiredMark">発日</a>
                <a class="ef" id="WF_DEPDATE" ondblclick="Field_DBclick('TxtDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtDepDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                </a>
            </span>
            <!-- ■　(予定)積車着日　■ -->
            <span>
                <a id="WF_ARRDATE_LABEL" class="requiredMark">積車着日</a>
                <a class="ef" id="WF_ARRDATE" ondblclick="Field_DBclick('TxtArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtArrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                </a>
            </span>
            <!-- ■　(予定)受入日　■ -->
            <span>
                <a id="WF_ACCDATE_LABEL" class="requiredMark">受入日</a>
                <a class="ef" id="WF_ACCDATE" ondblclick="Field_DBclick('TxtAccDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtAccDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                </a>
            </span>
            <!-- ■　(予定)空車着日　■ -->
            <span>
                <a id="WF_EMPARRDATE_LABEL" class="requiredMark">空車着日</a>
                <a class="ef" id="WF_EMPARRDATE" ondblclick="Field_DBclick('TxtEmparrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtEmparrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                </a>
            </span>
            
        </div> <!-- End commonHeaderInput -->
        <div class="summaryAreaWrapper">
            <div class="summaryArea">
                <!-- ■　油種別タンク車数(車)　■ -->
                <span>
                    <a>&nbsp;</a>
                    <a id="WF_OILTANKCNT_LABEL" class="requiredMark">油種別タンク車数(車)</a>
                </span>
                <!-- ■　合計車数　■ -->
                <span>
                    <a id="WF_TOTALTANK_LABEL">合計</a>
                    <a class="ef" id="WF_TOTALTANK">
                        <asp:TextBox ID="TxtTotalTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　車数（ハイオク）　■ -->
                <span>
                    <a id="WF_HTANK_LABEL">ハイオク</a>
                    <a class="ef" id="WF_HTANK">
                        <asp:TextBox ID="TxtHTank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（レギュラー）　■ -->
                <span>
                    <a id="WF_RTANK_LABEL">レギュラー</a>
                    <a class="ef" id="WF_RTANK">
                        <asp:TextBox ID="TxtRTank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　車数（灯油）　■ -->
                <span>
                    <a id="WF_TTANK_LABEL">灯油</a>
                    <a class="ef" id="WF_TTANK">
                        <asp:TextBox ID="TxtTTank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>
                <span>
                    <!-- ■　車数（未添加灯油）　■ -->
                    <a id="WF_MTTANK_LABEL">未添加灯油</a>
                    <a class="ef" id="WF_MTTANK">
                        <asp:TextBox ID="TxtMTTank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（軽油）　■ -->
                <span>
                    <a id="WF_KTANK_LABEL">軽油</a>
                    <a class="ef" id="WF_KTANK">
                        <asp:TextBox ID="TxtKTank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（３号軽油）　■ -->
                <span>
                    <a id="WF_K3TANK_LABEL">３号軽油</a>
                    <a class="ef" id="WF_K3TANK">
                        <asp:TextBox ID="TxtK3Tank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　車数（５号軽油）　■ -->
                <span>
                    <a id="WF_K5TANK_LABEL">５号軽油</a>
                    <a class="ef" id="WF_K5TANK">
                        <asp:TextBox ID="TxtK5Tank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（１０号軽油）　■ -->
                <span>
                    <a id="WF_K10TANK_LABEL">１０号軽油</a>
                    <a class="ef" id="WF_K10TANK">
                        <asp:TextBox ID="TxtK10Tank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　車数（LSA）　■ -->
                <span>
                    <a id="WF_LTANK_LABEL">ＬＳＡ</a>
                    <a class="ef" id="WF_LTANK">
                        <asp:TextBox ID="TxtLTank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（A重油）　■ -->
                <span>
                    <a id="WF_ATANK_LABEL">Ａ重油</a>
                    <a class="ef" id="WF_ATANK">
                        <asp:TextBox ID="TxtATank" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2"></asp:TextBox>
                    </a>
                </span>
                <!-- ここに↑と同じ規則性で追加 -->
<%--            <span> ←こんな感じで項目を増やすと画面上では 上1下2 右移動 上3下4 ・・・の順で追加される、ワザワザposition fixed top leftは使用しない
                    <a>項目名</a>
                    <a>テキストボックス</a>
                </span>  --%>

            </div> <!-- End summaryArea -->
            <div class="rightSpace"></div>
        </div> <!-- End summaryAreaWrapper -->

    </div> <!-- End headerbox -->

    <!-- 全体レイアウト　detailbox -->
    <div class="detailbox" id="detailbox">
        <!-- ボタン -->
        <div class="actionButtonBox">
            <div class="leftSide">
                <input type="button" id="WF_ButtonALLSELECT" class="btn-sticky" value="全選択"  onclick="ButtonClick('WF_ButtonALLSELECT');" />
                <input type="button" id="WF_ButtonSELECT_LIFTED" class="btn-sticky" value="選択解除" onclick="ButtonClick('WF_ButtonSELECT_LIFTED');" />
                <input type="button" id="WF_ButtonLINE_LIFTED" class="btn-sticky" value="行削除"  onclick="ButtonClick('WF_ButtonLINE_LIFTED');" />
                <input type="button" id="WF_ButtonLINE_ADD" class="btn-sticky" value="行追加" onclick="ButtonClick('WF_ButtonLINE_ADD');" />
                <input type="button" id="WF_ButtonCSV" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ"  onclick="ButtonClick('WF_ButtonCSV');" />
            </div>
            <div class="rightSide">
                <%--<input type="button" id="WF_ButtonUPDATE" class="btn-sticky" value="空回日報確定" onclick="ButtonClick('WF_ButtonUPDATE');" />--%>
                <input type="button" id="WF_ButtonUPDATE" class="btn-sticky" value="受注登録" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </div>
        </div>
        <div id="listWrapper" class="listWrapper">
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
            <!-- 油種数登録ボタン押下フラグ(True:有効, False：無効) -->
            <input id="WF_ButtonInsertFLG" runat="server" value="" type="text" />
            <!-- 受注進行ステータス制御フラグ -->
            <input id="WF_OrderStatusFLG" runat="server" value="" type="text" />
        </div>
</asp:Content>



<%@ Page Title="OIT0002D" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0002LinkDetail.aspx.vb" Inherits="JOTWEB.OIT0002LinkDetail" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0002DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0002D.css")%>' rel="stylesheet" type="text/css" />
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
        <!-- ボタン -->
        <div class="actionButtonBox">
            <div class="leftSide">
            </div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonRegister" value="登録"  class="btn-sticky" style="Width:5em" onclick="ButtonClick('WF_ButtonRegister');" />
                <input type="button" id="WF_ButtonEND" value="戻る"  class="btn-sticky" style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
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

            <a class="ef" ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
                <asp:TextBox ID="WF_ORG" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_ORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </a>
        </div>
        <!-- ○ 変動項目 ○ -->
        <div class="commonHeaderInput"> <!-- 共通ヘッダー入力部 1行4列設定 -->
            <!-- ■　登録営業所　■ -->
            <span>
                <a id="WF_OFFICECODE_LABEL" class="requiredMark">登録営業所</a>
                <a class="ef" id="WF_OFFICECODE" ondblclick="Field_DBclick('TxtOrderOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtOrderOffice');">
                    <asp:TextBox ID="TxtOrderOffice" runat="server" onblur="MsgClear();" ReadOnly="true" CssClass="boxIcon iconOnly" MaxLength="20"></asp:TextBox>
                </a>
            </span>
            <span></span><span></span><span></span>
            <!-- ■　返送列車　■ -->
            <span>
                <a id="WF_BTRAIN_LABEL" class="requiredMark">返送列車</a>
                <a class="ef" id="WF_BTRAINCODE" ondblclick="Field_DBclick('TxtBTrainNo', <%=LIST_BOX_CLASSIFICATION.LC_BTRAINNUMBER%>);">
                    <asp:TextBox ID="TxtBTrainNo" runat="server" onblur="MsgClear();" ReadOnly="true" CssClass="boxIcon iconOnly" MaxLength="4"></asp:TextBox>
                    <asp:TextBox ID="TxtBTrainName" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                </a>
            </span>
            <span>
                <a id="WF_BTRAINNAME">
                    <asp:Label ID="LblBTrainName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </span>
            <!-- ■　空車着日　■ -->
            <span>
                <a id="WF_EMPARRDATE_LABEL" class="requiredMark">空車着日</a>
                <a class="ef" id="WF_EMPARRDATE" ondblclick="Field_DBclick('txtEmparrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="txtEmparrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                </a>
            </span>
            <span></span>
        </div>
        <div class="summaryAreaWrapper">
            <div class="summaryArea">
                 <!-- ■　油種別タンク車数(車)　■ -->
                <span>
                    <a>&nbsp;</a>
                    <a id="WF_OILTANKCNT_LABEL">油種別タンク車数(車)</a>
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
                        <asp:TextBox ID="TxtHTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（レギュラー）　■ -->
                <span>
                    <a id="WF_RTANK_LABEL">レギュラー</a>
                    <a class="ef" id="WF_RTANK">
                        <asp:TextBox ID="TxtRTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（灯油）　■ -->
                <span>
                    <a id="WF_TTANK_LABEL">灯油</a>
                    <a class="ef" id="WF_TTANK">
                        <asp:TextBox ID="TxtTTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（未添加灯油）　■ -->
                <span>
                    <a id="WF_MTTANK_LABEL">未添加灯油</a>
                    <a class="ef" id="WF_MTTANK">
                        <asp:TextBox ID="TxtMTTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（軽油）　■ -->
                <span>
                    <a id="WF_KTANK_LABEL">軽油</a>
                    <a class="ef" id="WF_KTANK">
                        <asp:TextBox ID="TxtKTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（３号軽油）　■ -->
                <span>
                    <a id="WF_K3TANK_LABEL">３号軽油</a>
                    <a class="ef" id="WF_K3TANK">
                        <asp:TextBox ID="TxtK3Tank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（５号軽油）　■ -->
                <span>
                    <a id="WF_K5TANK_LABEL">５号軽油</a>
                    <a class="ef" id="WF_K5TANK">
                        <asp:TextBox ID="TxtK5Tank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（１０号軽油）　■ -->
                <span>
                    <a id="WF_K10TANK_LABEL">１０号軽油</a>
                    <a class="ef" id="WF_K10TANK">
                        <asp:TextBox ID="TxtK10Tank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（LSA）　■ -->
                <span>
                    <a id="WF_LTANK_LABEL">ＬＳＡ</a>
                    <a class="ef" id="WF_LTANK">
                        <asp:TextBox ID="TxtLTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　車数（A重油）　■ -->
                <span>
                    <a id="WF_ATANK_LABEL">Ａ重油</a>
                    <a class="ef" id="WF_ATANK">
                        <asp:TextBox ID="TxtATank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <div class="rightSpace"></div>
            </div>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div class="detailbox" id="detailbox">
        <div class="actionButtonBox">
            <div class="leftSide">
                <div style="display:none">
                    <input type="button" id="WF_ButtonALLSELECT"     class="btn-sticky" value="全選択"   onclick="ButtonClick('WF_ButtonALLSELECT');" />
                    <input type="button" id="WF_ButtonSELECT_LIFTED" class="btn-sticky" value="選択解除" onclick="ButtonClick('WF_ButtonSELECT_LIFTED');" />
                    <input type="button" id="WF_ButtonLINE_LIFTED"   class="btn-sticky" value="行削除"   onclick="ButtonClick('WF_ButtonLINE_LIFTED');" />
                    <input type="button" id="WF_ButtonLINE_ADD"      class="btn-sticky" value="行追加"   onclick="ButtonClick('WF_ButtonLINE_ADD');" />
                </div>
                <input type="button" id="WF_ButtonCSV" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonCSV');" />
            </div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonUPDATE" class="btn-sticky" value="明細更新" onclick="ButtonClick('WF_ButtonUPDATE');" />
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

            <!-- 油種数登録ボタン押下フラグ(True:有効, False：無効) -->
            <input id="WF_ButtonInsertFLG" runat="server" value="" type="text" />

        </div>
</asp:Content>

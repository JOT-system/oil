<%@ Page Title="OIT0003L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0003OrderList.aspx.vb" Inherits="JOTWEB.OIT0003OrderList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC"  %>

<asp:Content ID="OIT0003LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0003L.css")%>' rel="stylesheet" type="text/css" /> 
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0003L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        //共通ポップアップボタン名
        var customPopUpOkButtonName = 'ﾀﾞｳﾝﾛｰﾄﾞ';
    </script>
</asp:Content>
 
<asp:Content ID="OIT0003L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <!-- 会社 -->
                        <asp:Label ID="WF_SEL_CAMPCODE" runat="server" Text="会社" Visible="false"></asp:Label>
                        <asp:Label ID="WF_SEL_CAMPNAME" runat="server" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>

                        <!-- 運用部署 -->
                        <asp:Label ID="WF_SELUORG_L" runat="server" Text="運用部署" Visible="false"></asp:Label>
                        <asp:Label ID="WF_SELUORG_TEXT" runat="server" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>
                        <!-- 左ボタン -->
                        <input type="button" id="WF_ButtonALLSELECT" class="btn-sticky" value="全選択"  onclick="ButtonClick('WF_ButtonALLSELECT');" />
                        <input type="button" id="WF_ButtonSELECT_LIFTED" class="btn-sticky" value="選択解除"  onclick="ButtonClick('WF_ButtonSELECT_LIFTED');" />
                        <input type="button" id="WF_ButtonORDER_CANCEL" class="btn-sticky" value="キャンセル"  onclick="ButtonClick('WF_ButtonORDER_CANCEL');" />←選択した受注のキャンセル
                    </div>
                    <div class="rightSide">
                        <!-- 右ボタン -->
                        <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="受注新規作成" style="width:7em;"  onclick="ButtonClick('WF_ButtonINSERT');" />
                        <input type="button" id="WF_ButtonOTLinkageINSERT" class="btn-sticky" value="社外連携" style="width:7em;"  onclick="ButtonClick('WF_ButtonOTLinkageINSERT');" />
                        <a style="display:none;">
                            <input type="button" id="WF_ButtonLinkINSERT" class="btn-sticky" value="貨車連結選択" style="width:7em;"  onclick="ButtonClick('WF_ButtonLinkINSERT');" />
                            <input type="button" id="WF_ButtonCSV" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ"   onclick="ButtonClick('WF_ButtonCSV');" />
                            <input type="button" id="WF_ButtonSendaiLOADCSV" class="btn-sticky" value="積込予定" onclick="ButtonClick('WF_ButtonSendaiLOADCSV');" />
                            <input type="button" id="WF_ButtonNegishiSHIPCSV" class="btn-sticky" value="出荷予定" onclick="ButtonClick('WF_ButtonNegishiSHIPCSV');" />
                            <input type="button" id="WF_ButtonNegishiLOADCSV" class="btn-sticky" value="積込予定" onclick="ButtonClick('WF_ButtonNegishiLOADCSV');" />
                        </a>
                        <input type="button" id="WF_ButtonTyohyo" class="btn-sticky" value="帳票" onclick="commonShowCustomPopup();" />
                        <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"   onclick="ButtonClick('WF_ButtonEND');" />
                        <!-- 先頭行・末尾行ボタンを表示させる場合は divの括りを無くして WF_ButtonXXXを外だしにすれば出ます -->
                        <div style="display:none;">
                            <div id="WF_ButtonFIRST" class="firstPage" runat="server"                    onclick="ButtonClick('WF_ButtonFIRST');"></div>
                            <div id="WF_ButtonLAST" class="lastPage" runat="server"                      onclick="ButtonClick('WF_ButtonLAST');"></div>
                        </div>
                    </div>
                </div>
 
            </div>
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
        </div>

        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

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
            <!-- ボタン権限 -->
            <!-- 0 : 石油部/情報システム部  -->
            <!-- 1 : 東北支店/仙台  -->
            <!-- 2 : 関東支店/五井/甲子/袖ヶ浦/根岸  -->
            <!-- 3 : 中部支店/四日市/三重塩浜  -->
            <input id="WF_BUTTONpermitcode" runat="server" value="" type="text" />
            <input id="WF_BUTTONofficecode" runat="server" value="" type="text" />
        </div>
 
</asp:Content>

<%--ポップアップタイトルバーの文字--%>
<asp:Content ID="ctCostumPopUpTitle" ContentPlaceHolderID ="contentsPopUpTitle" runat="server">
    帳票
</asp:Content>
<%--ポップアップタイトルバーの内容--%>
<asp:Content ID="ctCostumPopUp" ContentPlaceHolderID ="contentsPopUpInside" runat="server">
<%--    <div>
        <div class="grc0001Wrapper">
            <ul>
                <li>
                    <asp:CheckBox ID="chkPrintJXTG" runat="server" Text="JXTG用帳票" />
                </li>
            </ul>
        </div>
    </div>--%>
<%--    <div>
        <span id="spnDownloadMonth" style="display:none;">
                <asp:Label ID="Label1" runat="server" Text="帳票年月"></asp:Label>
                <asp:TextBox ID="txtDownloadMonth" runat="server" data-monthpicker="1"></asp:TextBox>
        </span>
    </div>--%>
    <div>
        <span id="spnLodDate">
            <asp:Label ID="lblReportLodDate" runat="server" Text="積込日"></asp:Label>
            <a class="ef" id="aReportLodDate" ondblclick="Field_DBclick('txtReportLodDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="txtReportLodDate" runat="server" CssClass="calendarIcon"  onblur="MsgClear();"></asp:TextBox>
            </a>
        </span>
    </div>
    <br/>
    <div>
        <MSINC:tilelist ID="tileSalesOffice" runat="server" />
    </div>
    <br/>
    <div class="grc0001Wrapper">
        <ul>
            <li>
                <asp:RadioButton ID="rbDeliveryBtn" runat="server" GroupName="WF_SW" Text="託送指示" onclick="reportRadioButton();" />
            </li>
            <li>
                <asp:RadioButton ID="rbDeliveryCSVBtn" runat="server" GroupName="WF_SW" Text="託送指示(CSV)" onclick="reportRadioButton();" />
            </li>
            <li>
                <asp:RadioButton ID="rbLoadBtn" runat="server" GroupName="WF_SW" Text="積込指示" onclick="reportRadioButton();" />
            </li>
            <li>
                <asp:RadioButton ID="rbOTLoadBtn" runat="server" GroupName="WF_SW" Text="OT積込指示" onclick="reportRadioButton();" />
            </li>
            <li>
                <asp:RadioButton ID="rbShipBtn" runat="server" GroupName="WF_SW" Text="出荷予定" onclick="reportRadioButton();" />
            </li>
            <li>
                <asp:RadioButton ID="rbLineBtn" runat="server" GroupName="WF_SW" Text="入線方" onclick="reportRadioButton();" />
            </li>
            <li>
                <asp:RadioButton ID="rbKinoeneLoadBtn" runat="server" GroupName="WF_SW" Text="回線別指示書<br>(甲子)" onclick="reportRadioButton();" />
            </li>
            <li>
                <asp:RadioButton ID="rbNegishiLoadBtn" runat="server" GroupName="WF_SW" Text="回線別(根岸)" onclick="reportRadioButton();" />
            </li>
            <li>
                <asp:RadioButton ID="rbKuukaiBtn" runat="server" GroupName="WF_SW" Text="空回日報" onclick="reportRadioButton();" />
            </li>
        </ul>
    </div>
    <div id="divRTrainNo">
        <span id="spnRTrainNo">
            <asp:Label ID="lblReportRTrainNo" runat="server" Text="列車番号(臨海)"></asp:Label>
            <a class="ef" id="aReportRTrainNo" ondblclick="Field_DBclick('txtReportRTrainNo', <%=LIST_BOX_CLASSIFICATION.LC_RINKAITRAIN_INLIST%>);">
                <asp:TextBox ID="txtReportRTrainNo" runat="server" CssClass="boxIcon iconOnly"  onblur="MsgClear();"></asp:TextBox>
            </a>
        </span>
    </div>
    <div id="divEndMonthChk">
        <span id="spnEndMonthChk">
            <asp:Label ID="lblEndMonthChkDmy" runat="server" Text="　　　　　　　　　"></asp:Label>
            <a id="aEndMonthChk">
                <asp:CheckBox ID="ChkEndMonthChk" runat="server" Text=" 当月積込、翌月発分を含める" />
            </a>
        </span>
    </div>

</asp:Content>

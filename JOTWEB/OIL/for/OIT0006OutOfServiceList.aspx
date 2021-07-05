<%@ Page Title="OIT0006L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0006OutOfServiceList.aspx.vb" Inherits="JOTWEB.OIT0006OutOfServiceList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0006WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC"  %>

<asp:Content ID="OIT0006LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0006L.css")%>' rel="stylesheet" type="text/css" /> 
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0006L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        //共通ポップアップボタン名
        var customPopUpOkButtonName = 'ﾀﾞｳﾝﾛｰﾄﾞ';
    </script>
</asp:Content>

<asp:Content ID="OIT0006L" ContentPlaceHolderID="contents1" runat="server">

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
                    <input type="button" id="WF_ButtonOUTOFSERVICE_CANCEL" class="btn-sticky" value="キャンセル"  onclick="ButtonClick('WF_ButtonOUTOFSERVICE_CANCEL');" />←選択した回送のキャンセル
                </div>
                <div class="rightSide">
                    <!-- 右ボタン -->
                    <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="回送新規作成" style="width:7em;"  onclick="ButtonClick('WF_ButtonINSERT');" />
                    <a style="display:none;">
                        <input type="button" id="WF_ButtonCSV" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonCSV');" />
                    <input type="button" id="WF_ButtonTyohyo" class="btn-sticky" value="帳票" onclick="commonShowCustomPopup();" />
                    </a>
                    <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"   onclick="ButtonClick('WF_ButtonEND');" />
                    <!-- 先頭行・末尾行ボタンを表示させる場合は divの括りを無くして WF_ButtonXXXを外だしにすれば出ます -->
                    <div style="display:none;">
                        <div id="WF_ButtonFIRST" class="firstPage" runat="server" onclick="ButtonClick('WF_ButtonFIRST');"></div>
                        <div id="WF_ButtonLAST" class="lastPage" runat="server" onclick="ButtonClick('WF_ButtonLAST');"></div>
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
        <!-- 帳票フラグ -->
        <input id="WF_TYOHYOFLG" runat="server" value="" type="text" />

        <!-- ボタン押下 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />
        <!-- 権限 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
    </div>
</asp:Content>

<%--ポップアップタイトルバーの文字--%>
<asp:Content ID="ctCostumPopUpTitle" ContentPlaceHolderID ="contentsPopUpTitle" runat="server">
    帳票
</asp:Content>
<%--ポップアップタイトルバーの内容--%>
<asp:Content ID="ctCostumPopUp" ContentPlaceHolderID ="contentsPopUpInside" runat="server">
    <div class="grc0001Wrapper">
        <span id="spnReportDateNowChk">
            <asp:Label ID="lblReportDateNowChk" runat="server" Text=""></asp:Label>
            <a id="aReportDateNowChk" onclick="reportDatrNowButton();" >
                <asp:CheckBox ID="chkReportDateNowChk" runat="server" Text="当日" />
            </a>
        </span>
        <span id="spnDepDate">
            <asp:Label ID="lblReportDepDate" runat="server" Text="発送日"></asp:Label>
            <a class="ef" id="aReportDepDate" ondblclick="Field_DBclick('txtReportDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox ID="txtReportDepDate" runat="server" CssClass="calendarIcon"  onblur="MsgClear();"></asp:TextBox>
            </a>
        </span>
    </div>
    <br/>
    <div>
        <MSINC:tilelist ID="tileSalesOffice" runat="server" />
    </div>

    <div class="grc0001Wrapper">
        <ul>
            <li>
                <asp:RadioButton ID="rbStationInspectionDateBtn" runat="server" GroupName="WF_SW" Text="貨物運送状(交検)" onclick="reportRadioButton();" />
            </li>
            <li>
                <asp:RadioButton ID="rbStationALLInspectionDateBtn" runat="server" GroupName="WF_SW" Text="貨物運送状(全検)" onclick="reportRadioButton();" />
            </li>
        </ul>
    </div>
</asp:Content>

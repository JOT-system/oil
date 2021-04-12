<%@ Page Title="OIT0009TA" Language="vb" AutoEventWireup="false" CodeBehind="OIT0009TransportAnalysis.aspx.vb" Inherits="JOTWEB.OIT0009TransportAnalysis" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0009WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC"  %>

<asp:Content id="OIT0009TAH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0009TA.css")%>' rel="stylesheet" type="text/css" /> 
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0009TA.js")%>'></script>
</asp:Content>

<asp:Content ID="OIT0009TA" ContentPlaceHolderID="contents1" runat="server">
    <!-- 〇ヘッダー部 headerbox -->
    <div class="headerbox" id="headerbox">
        <div class="Operation">
            <div class="actionButtonBox">
                <div class="leftSide"></div>
                <div class="rightSide">
                    <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
                </div>
            </div> <!-- End actionButtonBox -->
        </div> <!-- End Operation -->
    </div> <!-- End headerbox -->
    <!-- 〇明細部 detailbox -->
    <div class="detailbox" id="detailbox">
        <div class="selectionButtonBox">
            <input type="button" class="btn-sticky" id="btnChkAll" value="全選択" onclick="checkAll();" />
            <input type="button" class="btn-sticky" id="btnUnChk" value="選択解除" onclick="unCheck();" />
        </div>
        <MSINC:tilelist ID="tileSalesOffice" runat="server" />
        <div class="titleArea">
            <a>実績データ取得（数量、車数、屯数）</a>
        </div>
        <div class="downLoadArea" id="downLoadArea">
            <!-- 日報 -->
            <div class="downLoadCondLine">
                <p class="condHeader"></p>
                <div class="condDetail">
                    <p class="reportTitle">日報</p>
                    <input type="button" class="btn-sticky" id="btnDailyOnTheDay" value="当日分" onclick="clickBtnDateChange('Daily', 1);" />
                    <input type="button" class="btn-sticky" id="btnDailyToOnTheDay" value="当日迄" onclick="clickBtnDateChange('Daily', 2);" />
                    <input type="button" class="btn-sticky" id="btnDailyPreviousDay" value="前日分" onclick="clickBtnDateChange('Daily', 3);" />
                    <input type="button" class="btn-sticky" id="btnDailyToPreviousDay" value="前日迄" onclick="clickBtnDateChange('Daily', 4);" />
                    <a>期間</a>
                    <div class="termInputArea">
                        <span ondblclick="Field_DBclick('txtDailyStYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                            <asp:TextBox ID="txtDailyStYmd" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                        </span>
                        ～
                        <span ondblclick="Field_DBclick('txtDailyEdYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                            <asp:TextBox ID="txtDailyEdYmd" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                        </span>
                    </div>
                    <input id="btnDownLoadDailyReport" type="button" runat="server" class="btn-sticky btnDownload" value="ダウンロード" onclick="ButtonClick('WF_DownLoadDailyReport');"  />
                </div>
            </div>
            <!-- 月報 -->
            <div class="downLoadCondLine">
                <p class="condHeader"></p>
                <div class="condDetail">
                    <p class="reportTitle">月報</p>
                    <input type="button" class="btn-sticky" id="btnMonthlyOnTheDay" value="当日分" onclick="clickBtnDateChange('Monthly', 1);" />
                    <input type="button" class="btn-sticky" id="btnMonthlyToOnTheDay" value="当日迄" onclick="clickBtnDateChange('Monthly', 2);" />
                    <input type="button" class="btn-sticky" id="btnMonthlyPreviousDay" value="前日分" onclick="clickBtnDateChange('Monthly', 3);" />
                    <input type="button" class="btn-sticky" id="btnMonthlyToPreviousDay" value="前日迄" onclick="clickBtnDateChange('Monthly', 4);" />
                    <a>期間</a>
                    <div class="termInputArea">
                        <span ondblclick="Field_DBclick('txtMonthlyStYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                            <asp:TextBox ID="txtMonthlyStYmd" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                        </span>
                        ～
                        <span ondblclick="Field_DBclick('txtMonthlyEdYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                            <asp:TextBox ID="txtMonthlyEdYmd" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                        </span>
                    </div>
                    <input id="btnDownLoadMonthlyReport" type="button" runat="server" class="btn-sticky btnDownload" value="ダウンロード" onclick="ButtonClick('WF_DownLoadMonthlyReport');"  />
                </div>
            </div>
        </div>
    </div> <!-- End detailbox -->

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
    </div>
</asp:Content>

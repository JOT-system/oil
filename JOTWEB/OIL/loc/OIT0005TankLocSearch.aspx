<%@ Page Title="OIT0005S" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0005TankLocSearch.aspx.vb" Inherits="JOTWEB.OIT0005TankLocSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>
<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC"  %>

<asp:Content ID="OIT0005SH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0005S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0005S.js")%>'></script>
</asp:Content>
<asp:Content ID="OIT0005S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO"  class="btn-sticky" value="検索" onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->
        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 会社コード -->
            <div class="inputItem" style="display:none;">
                <a id="WF_CAMPCODE_LABEL">会社コード</a>
                <a class="ef" id="WF_CAMPCODE_CODE" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_CAMPCODE_NAME" >
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 組織コード -->
            <div class="inputItem" style="display:none;">
                <a id="WF_ORG_LABEL">組織コード</a>
                <a class="ef" id="WF_ORG_CODE" ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
                    <asp:TextBox ID="WF_ORG" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_ORG_NAME" >
                    <asp:Label ID="WF_ORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 所属先 -->
            <div class="inputItem" >
                <a id="WF_Test" >
                    <div>
                    <span class="requiredMark" style="display:inline-block;margin-bottom:5px;">タンク車を管轄する支店・営業所を選ぶ</span>
                    <input type="button" class="btn-sticky" id="btnChkAll" value="全選択" onclick="checkAll();" />
                    <input type="button" class="btn-sticky" id="btnUnChk" value="選択解除" onclick="unCheck();" />
                    </div>
                </a>
                <a>
                    <MSINC:tilelist ID="tileSalesOffice" runat="server" />
                </a>
                <a></a>
            </div>
            <%-- 交検一覧ダウンロード --%>
            <div class="inputItem">
                <div>
                    交検一覧
<%--                    <a class="ef" id="WF_STYMD_CODE" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);" onchange="TextBox_change('WF_STYMD');" style="display:inline-block;width:10em;">
                        <asp:TextBox ID="WF_STYMD" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                    </a>--%>
                    <asp:DropDownList ID="ddlKoukenListYearMonth" runat="server" ClientIDMode="Predictable" CssClass="yearMonthDdl"></asp:DropDownList>
                    ～
                    <input type="button" id="WF_ButtonKoukenList"  class="btn-sticky" value="ダウンロード" onclick="ButtonClick('WF_ButtonKoukenList');" style="margin-left:1em;" />
                </div>
            </div>
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
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->
    </div>
</asp:Content>

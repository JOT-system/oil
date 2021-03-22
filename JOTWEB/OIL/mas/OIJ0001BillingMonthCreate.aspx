<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIJ0001BillingMonthCreate.aspx.vb" Inherits="JOTWEB.OIJ0001BillingMonthCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIJ0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content id="OIJ0001CH" contentplaceholderid="head" runat="server">
    <!-- <link href='<%=ResolveUrl("~/OIL/css/OIJ0001C.css")%>' rel="stylesheet" type="text/css" /> -->
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIJ0001C.js")%>'></script>
</asp:Content>


<asp:Content ID="OIJ0001C" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 会社コード -->
            <div class="inputItem" style="display:none">
                <a id="WF_CAMPCODE_LABEL" class="requiredMark">会社コード</a>
                <a class="ef" id="WF_CAMPCODE_CODE" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_CAMPCODE_NAME">
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 運用部署 -->
            <div class="inputItem">
                <a id="WF_UORG_LABEL" class="requiredMark" style="display:none">運用部署</a>
                <a class="ef" id="WF_UORG_CODE" style="display:none" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
                    <asp:TextBox ID="WF_UORG" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_UORG_NAME" style="display:none">
                    <asp:Label ID="WF_UORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 計上年月 -->
            <div class="inputItem">
                <a id="WF_KEIJYOYM_LABEL">計上年月</a>
                <a class="ef" id="WF_KEIJYOYM" ondblclick="Field_DBclick('TxtKeijyoYM', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);" onchange="TextBox_change('TxtKeijyoYM');">
                    <asp:TextBox ID="TxtKeijyoYM" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a id="WF_KEIJYOYM_BUTTON">
                    <input type="button" class="btn-sticky" id="btnKeijyoYM" value="受注費用作成"  onclick="ButtonClick('WF_ButtonINSERT');" />
                </a>
            </div>
    </div> <!-- End searchbox -->

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
    </div>
</asp:Content>

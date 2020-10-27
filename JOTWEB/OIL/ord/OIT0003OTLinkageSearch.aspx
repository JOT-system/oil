<%@ Page Title="OIT0003OTS" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0003OTLinkageSearch.aspx.vb" Inherits="JOTWEB.OIT0003OTLinkageSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0003OTSH" ContentPlaceHolderID="head" runat="server">
    <!-- <link href='<%=ResolveUrl("~/OIL/css/OIT0003S.css")%>' rel="stylesheet" type="text/css" /> -->
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0003OTS.js")%>'></script>
</asp:Content>
<asp:Content ID="OIT0003OTS" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO" class="btn-sticky" value="検索"  onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 会社コード -->
            <div class="inputItem" style="display:none;">
                <a>会社コード</a>
                <a class="ef" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 運用部署 -->
            <div class="inputItem" style="display:none;">
                <a>運用部署</a>
                <a class="ef" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
                    <asp:TextBox ID="WF_UORG" runat="server" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_UORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 営業所 -->
            <div class="inputItem">
                <a id="WF_OFFICECODE_LABEL" class="requiredMark">営業所</a>
                <a class="ef" id="WF_OFFICECODE" ondblclick="Field_DBclick('TxtSalesOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtSalesOffice');">
                    <asp:TextBox ID="TxtSalesOffice" runat="server"  CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_OFFICECODE_TEXT" >
                    <asp:Label ID="LblSalesOfficeName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
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
        <input id="WF_LoadAfterBackOrForward" runat="server" value="" type="text" />
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentsPopUpTitle" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="contentsPopUpInside" runat="server">
</asp:Content>

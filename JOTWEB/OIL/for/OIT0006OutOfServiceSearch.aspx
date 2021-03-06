﻿<%@ Page Title="OIT0006S" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0006OutOfServiceSearch.aspx.vb" Inherits="JOTWEB.OIT0006OutOfServiceSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0006WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content id="OIT0006SH" contentplaceholderid="head" runat="server">
    <!-- <link href='<%=ResolveUrl("~/OIL/css/OIT0006S.css")%>' rel="stylesheet" type="text/css" /> -->
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0006S.js")%>'></script>
</asp:Content>

<asp:Content ID="OIT0006S" ContentPlaceHolderID="contents1" runat="server">
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
                <a id="WF_OFFICECODE_LABEL">営業所</a>
                <a class="ef" id="WF_OFFICECODE" ondblclick="Field_DBclick('TxtSalesOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE_KAISOU%>);" onchange="TextBox_change('TxtSalesOffice');">
                    <asp:TextBox ID="TxtSalesOffice" runat="server"  CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_OFFICECODE_TEXT" >
                    <asp:Label ID="LblSalesOfficeName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 年月日(登録日) -->
            <div class="inputItem">
                <a id="WF_DATE_LABEL" class="requiredMark">登録日</a>
                <a class="ef" id="WF_DATE" ondblclick="Field_DBclick('TxtDateStart', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtDateStart" runat="server" CssClass="calendarIcon" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a id="WF_DATE_SYMBOL"><span>～</span></a>
            </div>

            <!-- 列車番号 -->
            <div class="inputItem" style="display:none">
                <a id="WF_TRAINNO_LABEL">列車番号</a>
                <a class="ef" id="WF_TRAINNO">
                    <asp:TextBox ID="TxtTrainNumber" runat="server" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </a>
            </div>

            <!-- 状態 -->
            <div class="inputItem">
                <a id="WF_STATUS_LABEL">状態</a>
                <a class="ef" id="WF_STATUS" ondblclick="Field_DBclick('TxtStatus', <%=LIST_BOX_CLASSIFICATION.LC_KAISOUSTATUS%>);" onchange="TextBox_change('TxtStatus');">
                    <asp:TextBox ID="TxtStatus" runat="server"  CssClass="boxIcon" onblur="MsgClear();" MaxLength="3"></asp:TextBox>
                </a>
                <a id="WF_STATUS_TEXT" >
                    <asp:Label ID="LblStatusName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 目的 -->
            <div class="inputItem" style="display:none">
                <a id="WF_OBJECTIVE_LABEL">目的</a>
                <a class="ef" id="WF_OBJECTIVE" ondblclick="Field_DBclick('TxtObjective', <%=LIST_BOX_CLASSIFICATION.LC_OBJECTIVECODE%>);" onchange="TextBox_change('TxtObjective');">
                    <asp:TextBox ID="TxtObjective" runat="server"  CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </a>
                <a id="WF_OBJECTIVE_TEXT" >
                    <asp:Label ID="LblObjective" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 着駅 -->
            <div class="inputItem" style="display:none">
                <a id="WF_ARRSTATION_LABEL">着駅</a>
                <a class="ef" id="WF_ARRSTATION" ondblclick="Field_DBclick('TxtArrstationCode', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtArrstationCode');">
                    <asp:TextBox ID="TxtArrstationCode" runat="server"  CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_ARRSTATION_TEXT" >
                    <asp:Label ID="LblArrstationName" runat="server" CssClass="WF_TEXT"></asp:Label>
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
    </div>

</asp:Content>

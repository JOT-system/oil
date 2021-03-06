﻿<%@ Page Title="OIM0004S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0004StationSearch.aspx.vb" Inherits="JOTWEB.OIM0004StationSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0004WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content id="OIM0004SH" contentplaceholderid="head" runat="server">
    <!-- <link href='<%=ResolveUrl("~/OIL/css/OIM0004S.css")%>' rel="stylesheet" type="text/css" /> -->
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0004S.js")%>'></script>
</asp:Content>

<asp:Content ID="OIM0004S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO" class="btn-sticky" value="検索" onclick="ButtonClick('WF_ButtonDO');" />
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

            <!-- 貨物駅コード -->
            <div class="inputItem">
                <a id="WF_STATIONCODE_LABEL">貨物駅コード</a>
                <a class="ef" id="WF_STATIONCODE" ondblclick="Field_DBclick('TxtStationCode', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtStationCode');">
                    <asp:TextBox ID="TxtStationCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="4" placeholder="前方一致で検索"></asp:TextBox>
                </a>
                <a id="WF_STATIONCODE_TEXT">
                    <asp:Label ID="LblStationCode" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 貨物コード枝番 -->
            <div class="inputItem">
                <a id="WF_BRANCH_LABEL">貨物コード枝番</a>
                <a class="ef" id="WF_BRANCH">
                    <asp:TextBox ID="TxtBranch" runat="server" CssClass="" onblur="MsgClear();" MaxLength="3"></asp:TextBox>
                </a>
                <a id="WF_BRANCH_TEXT">
                    <asp:Label ID="LblBranch" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 発着駅フラグ -->
            <div class="inputItem">
                <a id="WF_DEPARRSTATION_LABEL">発着駅フラグ</a>
                <a class="ef" id="WF_DEPARRSTATION" ondblclick="Field_DBclick('TxtDepArrStation', <%=LIST_BOX_CLASSIFICATION.LC_DEPARRSTATIONLIST%>);" onchange="TextBox_change('TxtDepArrStation');">
                    <asp:TextBox ID="TxtDepArrStation" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </a>
                <a id="WF_DEPARRSTATION_TEXT">
                    <asp:Label ID="LblDepArrStation" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

        </div> <!-- End inputBox -->

        <!-- 会社コード -->
<%--        <a style="position:fixed; top:7.7em; left:4em; font-weight:bold; text-decoration:underline;display:none">会社コード</a>
        <a style="position:fixed; top:7.7em; left:18em;display:none" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:7.7em; left:27em;display:none">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>--%>
        <!-- 運用部署 -->
<%--        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;display:none">運用部署</a>

        <a style="position:fixed; top:9.9em; left:18em;display:none" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
            <asp:TextBox ID="WF_UORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:9.9em; left:27em;display:none">
            <asp:Label ID="WF_UORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>--%>
        <!-- 貨物駅コード -->
<%--        <a id="WF_STATIONCODE_LABEL" class="requiredMark">貨物駅コード</a>

        <a id="WF_STATIONCODE_ICON" onclick="Field_DBclick('TxtStationCode', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);">
            <asp:Image runat="server" ImageUrl="../img/leftbox.png"/>
        </a>
        <a class="ef" id="WF_STATIONCODE" ondblclick="Field_DBclick('TxtStationCode', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtStationCode');">
            <asp:TextBox ID="TxtStationCode" runat="server" onblur="MsgClear();" MaxLength="4" placeholder="前方一致で検索"></asp:TextBox>
        </a>
        <a  id="WF_STATIONCODE_TEXT">
            <asp:Label ID="LblStationCode" runat="server" CssClass="WF_TEXT"></asp:Label>
        </a>--%>
        <!-- 貨物コード枝番 -->
<%--        <a id="WF_BRANCH_LABEL">貨物コード枝番</a>
        <a class="ef" id="WF_BRANCH" onchange="TextBox_change('TxtStationCode');">
            <asp:TextBox ID="TxtBranch" runat="server" onblur="MsgClear();" MaxLength="3"></asp:TextBox>
        </a>
        <a id="WF_BRANCH_TEXT">
            <asp:Label ID="LblBranch" runat="server" CssClass="WF_TEXT"></asp:Label>
        </a>--%>
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
</asp:Content>

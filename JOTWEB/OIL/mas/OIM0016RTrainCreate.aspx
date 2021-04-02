<%@ Page Title="OIM0016C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0016RTrainCreate.aspx.vb" Inherits="JOTWEB.OIM0016RTrainCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0016WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0016CH" ContentPlaceHolderID="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0016C.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0016C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0016C" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="表更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR"  class="btn-sticky" value="クリア" onclick="ButtonClick('WF_CLEAR');" />
                    </div>
                </div>
            </div>

            <div id="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- 選択No -->
                    <span>
                        <asp:Label ID="WF_SEL_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:Label ID="WF_SEL_LINECNT" runat="server" CssClass="WF_TEXT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_2">
                    <!-- 削除フラグ -->
                    <span class="ef">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除フラグ" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('WF_DELFLG');">
                            <asp:TextBox ID="WF_DELFLG" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_3">
                    <!-- 管轄受注営業所 -->
                    <span class="ef">
                        <asp:Label ID="WF_OFFICECODE_L" runat="server" Text="管轄受注営業所" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OFFICECODE', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('WF_OFFICECODE');">
                            <asp:TextBox ID="WF_OFFICECODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OFFICECODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 入線出線区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_IOKBN_L" runat="server" Text="入線出線区分" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_IOKBN', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_IOKBN');">
                            <asp:TextBox ID="WF_IOKBN" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_IOKBN_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 入線出線列車番号 -->
                    <span class="ef">
                        <asp:Label ID="WF_TRAINNO_L" runat="server" Text="入線出線列車番号" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_TRAINNO" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_TRAINNO_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 入線出線列車名 -->
                    <span class="ef">
                        <asp:Label ID="WF_TRAINNAME_L" runat="server" Text="入線出線列車名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_TRAINNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_TRAINNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 発駅コード -->
                    <span class="ef">
                        <asp:Label ID="WF_DEPSTATION_L" runat="server" Text="発駅コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_DEPSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_DEPSTATION');">
                            <asp:TextBox ID="WF_DEPSTATION" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="7"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_DEPSTATION_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 着駅コード -->
                    <span class="ef">
                        <asp:Label ID="WF_ARRSTATION_L" runat="server" Text="着駅コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ARRSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_ARRSTATION');">
                            <asp:TextBox ID="WF_ARRSTATION" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="7"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ARRSTATION_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- プラントコード -->
                    <span class="ef">
                        <asp:Label ID="WF_PLANTCODE_L" runat="server" Text="プラントコード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_PLANTCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_PLANTCODE');">
                            <asp:TextBox ID="WF_PLANTCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_PLANTCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 回線 -->
                    <span class="ef">
                        <asp:Label ID="WF_LINE_L" runat="server" Text="回線" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_LINE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_LINE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

            </div>
        </div>

        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

        <!-- イベント用 -->
        <div style="display:none;">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
            <!-- GridView DBクリック-->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>
            <!-- GridView表示位置フィールド -->

            <input id="WF_FIELD" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD_REP" runat="server" value="" type="text" />
            <!-- Textbox(Repeater) DBクリックフィールド -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->

            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
            <!-- LeftBox Mview切替 -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
            <!-- LeftBox 開閉 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text" />
            <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value="" type="text" />
            <!-- Rightbox 開閉 -->

            <input id="WF_PrintURL" runat="server" value="" type="text" />
            <!-- Textbox Print URL -->

            <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />
            <!-- 一覧・詳細画面切替用フラグ -->

            <input id="WF_ButtonClick" runat="server" value="" type="text" />
            <!-- ボタン押下 -->
            <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
            <!-- 権限 -->
        </div>
 
</asp:Content>

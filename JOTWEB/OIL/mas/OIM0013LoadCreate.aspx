<%@ Page Title="OIM0013C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0013LoadCreate.aspx.vb" Inherits="JOTWEB.OIM0013LoadCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0013WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0013CH" ContentPlaceHolderID="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0013C.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0013C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0013C" ContentPlaceHolderID="contents1" runat="server">
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
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" CssClass="WF_TEXT_LABEL"></asp:Label>
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

                    <!-- 基地コード -->
                    <span class="ef">
                        <asp:Label ID="WF_PLANTCODE_L" runat="server" Text="基地コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_PLANTCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_PLANTCODE');">
                            <asp:TextBox ID="WF_PLANTCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_PLANTCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_3">
                    <!-- 最大回線数 -->
                    <span class="ef">
                        <asp:Label ID="WF_MAXLINECNT_L" runat="server" Text="最大回線数" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_MAXLINECNT" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_MAXLINECNT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 積込ポイント -->
                    <span class="ef">
                        <asp:Label ID="WF_LOADINGPOINT_L" runat="server" Text="積込ポイント" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_LOADINGPOINT" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LOADINGPOINT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 油種コード -->
                    <span class="ef">
                        <asp:Label ID="WF_OILCODE_L" runat="server" Text="油種コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OILCODE');">
                            <asp:TextBox ID="WF_OILCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 油種細分コード -->
                    <span class="ef">
                        <asp:Label ID="WF_SEGMENTOILCODE_L" runat="server" Text="油種細分コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_SEGMENTOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_SEGMENTOILCODE');">
                            <asp:TextBox ID="WF_SEGMENTOILCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_SEGMENTOILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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

<%@ Page Title="OIM0002C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0002OrgCreate.aspx.vb" Inherits="JOTWEB.OIM0002OrgCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0002CH" ContentPlaceHolderID="head" runat="server">
    <%--<link href='<%=ResolveUrl("~/OIL/css/OIM0002C.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0002C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0002C" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_CLEAR');" />
                    </div>
                </div>
            </div>

            <div id="detailkeybox">

                <p id="KEY_LINE_1">
                    <!-- 選択No -->
                    <span>
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_2">
                    <!-- 削除フラグ -->
                    <span class="ef">
                        <asp:Label ID="LblDelFlg" runat="server" Text="削除フラグ" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('WF_DELFLG');">
                        <asp:TextBox ID="TxtDelFlg" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="1"></asp:TextBox></span>
                        <asp:Label ID="Label1" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_3">
                    <!-- 会社コード -->
                    <span class="ef" id="WF_CAMPCODE">
                        <asp:Label ID="LblCampCode" runat="server" Text="会社コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                        <asp:TextBox ID="TxtCampCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox></span>
                        <asp:Label ID="Label2" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_4">
                    <!-- 組織コード -->
                    <span class="ef" id="WF_ORGCODE">
                        <asp:Label ID="LblOrgCode" runat="server" Text="組織コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ORGCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORGCODE');">
                        <asp:TextBox ID="TxtOrgCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox></span>
                        <asp:Label ID="Label3" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_5">
                    <!-- 開始年月日 -->
                    <span class="ef" id="WF_STYMD">
                        <asp:Label ID="LblStYmd" runat="server" Text="開始年月日" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtStYmd" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox></span>
                        <asp:Label ID="Label4" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_6">
                    <!-- 終了年月日 -->
                    <span class="ef" id="WF_ENDYMD">
                        <asp:Label ID="LblEndYmd" runat="server" Text="終了年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtEndYmd" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox></span>
                        <asp:Label ID="Label5" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_7">
                    <!-- 組織名称 -->
                    <span class="ef" id="WF_ORGNAME">
                        <asp:Label ID="LblOrgName" runat="server" Text="組織名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOrgName" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label6" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_8">
                    <!-- 組織名称（短） -->
                    <span class="ef" id="WF_ORGNAMES">
                        <asp:Label ID="LblOrgNameS" runat="server" Text="組織名称（短）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOrgNameS" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="20"></asp:TextBox>
                        <asp:Label ID="Label7" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_9">
                    <!-- 組織名称カナ -->
                    <span class="ef" id="WF_ORGNAMEKANA">
                        <asp:Label ID="LblOegNameKana" runat="server" Text="組織名称カナ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOrgNameKana" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label8" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_10">
                    <!-- 組織名称カナ（短） -->
                    <span class="ef" id="WF_ORGNAMEKANAS">
                        <asp:Label ID="LblOrgNameKanaS" runat="server" Text="組織名称カナ（短）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOrgNameKanaS" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="20"></asp:TextBox>
                        <asp:Label ID="Label9" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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
        <div hidden="hidden">
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


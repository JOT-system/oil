<%@ Page Title="OIM0002S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0002OrgSearch.aspx.vb" Inherits="JOTWEB.OIM0002OrgSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="OIM0002SH" contentplaceholderid="head" runat="server">
    <!-- <link href='<%=ResolveUrl("~/OIL/css/OIM0002S.css")%>' rel="stylesheet" type="text/css" /> -->
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0002S.js")%>'></script>
</asp:Content>

<asp:Content ID="OIM0002S" ContentPlaceHolderID="contents1" runat="server">
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
                <a id="LblCampCodeMy" class="requiredMark">会社コード</a>
                <a class="ef" id="WF_CAMPCODE_MY" ondblclick="Field_DBclick('WF_CAMPCODE_MY', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE_MY');">
                    <asp:TextBox ID="TxtCampCodeMy" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_CAMPNAME_MY">
                    <asp:Label ID="txtCampNameMy" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 運用部署 -->
            <div class="inputItem" style="display:none">
                <a id="LblOrgCodeMy" class="requiredMark">運用部署</a>
                <a class="ef" id="WF_ORGCODE_MY" style="display:none" ondblclick="Field_DBclick('WF_ORGCODE_MY', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORGCODE_MY');">
                    <asp:TextBox ID="TxtOrgCodeMy" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>TxtCampCode
                <a id="WF_ORGNAME_MY" style="display:none">
                    <asp:Label ID="txtOrgNameMy" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 会社コード2 -->
            <div class="inputItem">
                <a id="LblCampCode">会社コード</a><br/>&nbsp;
                    <a class="ef" id="WF_CAMPCODE" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                        <asp:TextBox ID="TxtCampCode" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_CAMPNAME">
                    <asp:Label ID="txtCampName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 組織コード2 -->
            <div class="inputItem">
                <a id="LblOrgCode" >組織コード</a><br/>&nbsp;
                    <a class="ef" id="WF_ORGCODE" ondblclick="Field_DBclick('WF_ORGCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORGCODE');">
                        <asp:TextBox ID="TxtOrgCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_ORGNAME">
                    <asp:Label ID="txtOrgName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 削除フラグ -->
            <div class="inputItem">
                <a id="LblSearch">検索条件</a><br/>&nbsp;
                <a id="WF_SW">
                    <asp:RadioButton ID="RdBSearch1" runat="server" GroupName="RdBSearch" Text="削除除く" /><br/>
                    <asp:RadioButton ID="RdBSearch2" runat="server" GroupName="RdBSearch" Text="削除のみ" />
                </a>
            </div>

            <!-- 削除フラグ -->
            <div class="inputItem">
                <a id="LblWord">※会社コード、組織コードの条件指定がない時は全件表示</a><br/>
            </div>

        </div> <!-- End inputBox -->
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

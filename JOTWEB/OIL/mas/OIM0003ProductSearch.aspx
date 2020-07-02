<%@ Page Title="OIM0003S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0003ProductSearch.aspx.vb" Inherits="JOTWEB.OIM0003ProductSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="OIM0003SH" contentplaceholderid="head" runat="server">
    <!-- <link href='<%=ResolveUrl("~/OIL/css/OIM0003S.css")%>' rel="stylesheet" type="text/css" /> -->
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0003S.js")%>'></script>
</asp:Content>

<asp:Content ID="OIM0003S" ContentPlaceHolderID="contents1" runat="server">
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
                    <asp:TextBox ID="TxtCampCodeMy" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox></a>
                <a id="WF_CAMPNAME_MY">
                    <asp:Label ID="txtCampNameMy" runat="server" CssClass="WF_TEXT"></asp:Label></a>
            </div>

            <!-- 運用部署 -->
            <div class="inputItem" style="display:none">
                <a id="LblOrgCodeMy" class="requiredMark">運用部署</a>
                <a class="ef" id="WF_ORGCODE_MY" style="display:none" ondblclick="Field_DBclick('WF_ORGCODE_MY', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORGCODE_MY');">
                    <asp:TextBox ID="TxtOrgCodeMy" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox></a>
                <a id="WF_ORGNAME_MY" style="display:none">
                    <asp:Label ID="txtOrgNameMy" runat="server" CssClass="WF_TEXT"></asp:Label></a>
            </div>

            <!-- 営業所コード -->
            <div class="inputItem">
                <a id="LblOfficeCode">営業所コード</a><br/>&nbsp;
                    <a class="ef" id="WF_SEL_OFFICECODE" ondblclick="Field_DBclick('WF_OFFICECODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_SEL_OFFICECODE');">
                        <asp:TextBox ID="TxtOfficeCode" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox></a>
                <a id="WF_OFFICECODE">
                    <asp:Label ID="TxtOfficeName" runat="server" CssClass="WF_TEXT"></asp:Label></a>
            </div>

            <!-- 荷主コード -->
            <div class="inputItem">
                <a id="LblShipperCode" >荷主コード</a><br/>&nbsp;
                    <a class="ef" id="WF_SEL_SHIPPERCODE" ondblclick="Field_DBclick('WF_SHIPPERCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_SEL_SHIPPERCODE');">
                        <asp:TextBox ID="TxtShipperCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox></a>
                <a id="WF_SHIPPERCODE">
                    <asp:Label ID="TxtShipperName" runat="server" CssClass="WF_TEXT"></asp:Label></a>
            </div>
            
            <!-- 基地コード -->
            <div class="inputItem">
                <a id="LblPlantCode" >基地コード</a><br/>&nbsp;
                    <a class="ef" id="WF_SEL_PLANTCODE" ondblclick="Field_DBclick('WF_PLANTCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_SEL_PLANTCODE');">
                        <asp:TextBox ID="TxtPlantCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox></a>
                <a id="WF_PLANTCODE">
                    <asp:Label ID="TxtPlantName" runat="server" CssClass="WF_TEXT"></asp:Label></a>
            </div>

            <!-- 油種大分類コード -->
            <div class="inputItem">
                <a id="LblBigoilCode" >油種大分類コード</a><br/>&nbsp;
                    <a class="ef" id="WF_SEL_BIGOILCODE" ondblclick="Field_DBclick('WF_BIGOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_SEL_BIGOILCODE');">
                        <asp:TextBox ID="TxtBigoilCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox></a>
                <a id="WF_BIGOILCODE">
                    <asp:Label ID="TxtBigoilName" runat="server" CssClass="WF_TEXT"></asp:Label></a>
            </div>
            
            <!-- 油種中分類コード -->
            <div class="inputItem">
                <a id="LblMiddleoilCode" >油種中分類コード</a><br/>&nbsp;
                    <a class="ef" id="WF_SEL_MIDDLEOILCODE" ondblclick="Field_DBclick('WF_MIDDLEOILNAME', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_SEL_MIDDLEOILCODE');">
                        <asp:TextBox ID="TxtMiddleoilCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox></a>
                <a id="WF_MIDDLEOILNAME">
                    <asp:Label ID="TxtMiddleoilName" runat="server" CssClass="WF_TEXT"></asp:Label></a>
            </div>
            
            <!-- 油種コード -->
            <div class="inputItem">
                <a id="LblOilCode" >油種コード</a><br/>&nbsp;
                    <a class="ef" id="WF_ORGCODE" ondblclick="Field_DBclick('WF_OILCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORGCODE');">
                        <asp:TextBox ID="TxtOilCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox></a>
                <a id="WF_OILCODE">
                    <asp:Label ID="TxtOilName" runat="server" CssClass="WF_TEXT"></asp:Label></a>
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
                <a id="LblWord">※条件指定がない時は全件表示</a><br/>
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

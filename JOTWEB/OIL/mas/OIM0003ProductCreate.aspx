<%@ Page Title="OIM0003C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0003ProductCreate.aspx.vb" Inherits="JOTWEB.OIM0003ProductCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0003CH" ContentPlaceHolderID="head" runat="server">
    <%--<link href='<%=ResolveUrl("~/OIL/css/OIM0003C.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0003C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0003C" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="表更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR" class="btn-sticky" value="クリア"  onclick="ButtonClick('WF_CLEAR');" />
                    </div>
                </div>
            </div>

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
                    <!-- 営業所コード -->
                    <span class="ef" id="WF_OFFICECODE">
                        <asp:Label ID="LblOfficeCode" runat="server" Text="営業所コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OFFICECODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_OFFICECODE');">
                        <asp:TextBox ID="TxtOfficeCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox></span>
                        <asp:Label ID="Label2" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_4">
                    <!-- 荷主コード -->
                    <span class="ef" id="WF_SHIPPERCODE">
                        <asp:Label ID="LblShipperCode" runat="server" Text="荷主コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_SHIPPERCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_SHIPPERCODE');">
                        <asp:TextBox ID="TxtShipperCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox></span>
                        <asp:Label ID="Label3" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_5">
                    <!-- 基地コード -->
                    <span class="ef" id="WF_PLANTCODE">
                        <asp:Label ID="LblPlantCode" runat="server" Text="基地コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_PLANTCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_PLANTCODE');">
                        <asp:TextBox ID="TxtPlantCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox></span>
                        <asp:Label ID="Label4" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_6">
                    <!-- 油種大分類コード -->
                    <span class="ef" id="WF_BIGOILCODE">
                        <asp:Label ID="LblBigoilCode" runat="server" Text="油種大分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_BIGOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_BIGOILCODE');">
                        <asp:TextBox ID="TxtBigoilCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox></span>
                        <asp:Label ID="Label5" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_7">
                    <!-- 油種大分類名 -->
                    <span class="ef" id="WF_BIGOILNAME">
                        <asp:Label ID="LblBigoilName" runat="server" Text="油種大分類名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtBigoilName" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label6" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_8">
                    <!-- 油種大分類名カナ -->
                    <span class="ef" id="WF_BIGOILKANA">
                        <asp:Label ID="LblBigoilKana" runat="server" Text="油種大分類名カナ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtBigoilKana" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label7" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_9">
                    <!-- 油種中分類コード -->
                    <span class="ef" id="WF_MIDDLEOILCODE">
                        <asp:Label ID="LblMiddleoilCode" runat="server" Text="油種中分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_MIDDLEOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_MIDDLEOILCODE');">
                        <asp:TextBox ID="TxtMiddleoilCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox></span>
                        <asp:Label ID="Label18" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_10">
                    <!-- 油種中分類名 -->
                    <span class="ef" id="WF_MIDDLEOILNAME">
                        <asp:Label ID="LblMiddleoilName" runat="server" Text="油種中分類名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtMiddleoilName" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label9" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_11">
                    <!-- 油種中分類名カナ -->
                    <span class="ef" id="WF_MIDDLEOILKANA">
                        <asp:Label ID="LblMiddleoilKana" runat="server" Text="油種中分類名カナ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtMiddleoilKana" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label10" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_12">
                    <!-- 油種コード -->
                    <span class="ef" id="WF_OILCODE">
                        <asp:Label ID="LblOilCode" runat="server" Text="油種コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OILCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_OILCODE');">
                        <asp:TextBox ID="TxtOilCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox></span>
                        <asp:Label ID="Label11" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_13">
                    <!-- 油種名 -->
                    <span class="ef" id="WF_OILNAME">
                        <asp:Label ID="LblOilName" runat="server" Text="油種名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOilName" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label12" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_14">
                    <!-- 油種名カナ -->
                    <span class="ef" id="WF_OILKANA">
                        <asp:Label ID="LblOilKana" runat="server" Text="油種名カナ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOilKana" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label13" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_15">
                    <!-- 油種細分コード -->
                    <span class="ef" id="WF_SEGMENTOILCODE">
                        <asp:Label ID="LblSegmentoilCode" runat="server" Text="油種細分コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_SEL_SEGMENTOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_SEL_SEGMENTOILCODE');">
                        <asp:TextBox ID="TxtSegmentoilCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox></span>
                        <asp:Label ID="Label14" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_16">
                    <!-- 油種名（細分） -->
                    <span class="ef" id="WF_SEGMENTOILNAME">
                        <asp:Label ID="LblSegmentoilName" runat="server" Text="油種名（細分）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtSegmentoilName" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label15" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_17">
                    <!-- OT油種コード -->
                    <span class="ef" id="WF_OTOILCODE">
                        <asp:Label ID="LblOtoilCode" runat="server" Text="OT油種コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOtoilCode" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label16" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_18">
                    <!-- OT油種名 -->
                    <span class="ef" id="WF_OTOILNAME">
                        <asp:Label ID="LblOtoilName" runat="server" Text="OT油種名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOtoilName" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label17" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_19">
                    <!-- 荷主油種コード -->
                    <span class="ef" id="WF_SHIPPEROILCODE">
                        <asp:Label ID="LblShipperoilCode" runat="server" Text="荷主油種コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtShipperoilCode" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label18" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_20>
                    <!-- 荷主油種名 -->
                    <span class="ef" id="WF_SHIPPEROILNAME">
                        <asp:Label ID="LblShipperoilName" runat="server" Text="荷主油種名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtShipperoilName" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label19" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_21">
                    <!-- 積込チェック用油種コード -->
                    <span class="ef" id="WF_CHECKOILCODE">
                        <asp:Label ID="LblCheckoilCode" runat="server" Text="積込チェック用油種コード " CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_CHECKOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_CHECKOILCODE');">
                        <asp:TextBox ID="TxtCheckoilCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox></span>
                        <asp:Label ID="Label20" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_22">
                    <!-- 積込チェック用油種名 -->
                    <span class="ef" id="WF_CHECKOILNAME">
                        <asp:Label ID="LblCheckoilName" runat="server" Text="積込チェック用油種名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtCheckoilName" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <asp:Label ID="Label21" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_23">
                    <!-- 在庫管理対象フラグ -->
                    <span class="ef" id="WF_STOCKFLG">
                        <asp:Label ID="LblStockFlg" runat="server" Text="在庫管理対象フラグ" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_STOCKFLG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_STOCKFLG');">
                        <asp:TextBox ID="TxtStockFlg" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox></span>
                        <asp:Label ID="Label22" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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


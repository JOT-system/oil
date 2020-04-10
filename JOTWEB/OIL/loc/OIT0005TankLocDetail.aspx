<%@ Page Title="OIT0005D" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0005TankLocDetail.aspx.vb" Inherits="JOTWEB.OIT0005TankLocDetail" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>
<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0005DH" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0005D.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
<asp:Content ID="OIT0005D" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="DB更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_CLEAR');" />
                    </div>
                </div>
            </div>

            <div id="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- タンク車番号 -->
                    <span>
                        <asp:Label ID="WF_Sel_TANKNUMBER_L" runat="server" Text="タンク車番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:Label ID="WF_Sel_TANKNUMBER" runat="server" CssClass="WF_TEXT"></asp:Label>
                        <span></span>
                    </span>

                </p>
                <p id="KEY_LINE_2">
                    <!-- 管轄支店コード -->
                    <span class="ef" id="WF_BRANCHCODE">
                        <asp:Label ID="LblBranchCode" runat="server" Text="管轄支店" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtBranchCode', <%=LIST_BOX_CLASSIFICATION.LC_BRANCH%>)" onchange="TextBox_change('TxtBranchCode');">
                            <asp:TextBox ID="TxtBranchCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6" onkeypress="CheckNum();"></asp:TextBox>
                        </span>
                        <span>
                            <asp:Label ID="LblBranchCodeText" runat="server" Text="" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                    </span>
                </p>
                <p id="KEY_LINE_3">
                    <!-- 所属営業所コード -->
                    <span class="ef" id="WF_OFFICECODE">
                        <asp:Label ID="LblOfficeCode" runat="server" Text="所属営業所" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtOfficeCode', <%=LIST_BOX_CLASSIFICATION.LC_BELONGTOOFFICE %>)" onchange="TextBox_change('TxtOfficeCode');">
                            <asp:TextBox ID="TxtOfficeCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6" onkeypress="CheckNum();"></asp:TextBox>                        
                        </span>
                        <span>
                            <asp:Label ID="LblOfficeCodeText" runat="server" Text="" CssClass="WF_TEXT_LEFT_LABEL" ></asp:Label>
                        </span>
                    </span>

                    <!-- 所在地コード -->
                    <span class="ef" id="WF_LOCATIONCODE">
                        <asp:Label ID="LblLocationCode" runat="server" Text="所在地" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtLocationCode', <%=LIST_BOX_CLASSIFICATION.LC_BRANCHOFFICESTATION%>)" onchange="TextBox_change('TxtLocationCode');">
                            <asp:TextBox ID="TxtLocationCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6" onkeypress="CheckNum();" ></asp:TextBox>
                        </span>
                        <span>
                            <asp:Label ID="LblLocationCodeText" runat="server" Text="" CssClass="WF_TEXT_LEFT_LABEL" ></asp:Label>
                        </span>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- タンク車状態コード -->
                    <span class="ef" id="WF_TANKSTATUS">
                        <asp:Label ID="LblTankStatus" runat="server" Text="タンク車状態" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtTankStatus', <%=LIST_BOX_CLASSIFICATION.LC_TANKSTATUS %>)" onchange="TextBox_change('TxtTankStatus');">
                            <asp:TextBox ID="TxtTankStatus" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1" onkeypress="CheckNum();"></asp:TextBox>
                        </span>
                        <span>
                            <asp:Label ID="LblTankStatusText" runat="server" Text="" CssClass="WF_TEXT_LEFT_LABEL" ></asp:Label>
                        </span>
                    </span>

                    <!-- 積車区分 -->
                    <span class="ef" id="WF_LOADINGKBN">
                        <asp:Label ID="LblLoadingKbn" runat="server" Text="積車区分" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtLoadingKbn', <%=LIST_BOX_CLASSIFICATION.LC_LOADINGKBN %>)" onchange="TextBox_change('TxtLoadingKbn');">
                            <asp:TextBox ID="TxtLoadingKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1" ></asp:TextBox>
                        </span>
                        <span>
                            <asp:Label ID="LblLoadingKbnText" runat="server" Text="" CssClass="WF_TEXT_LEFT_LABEL" ></asp:Label>
                        </span>
                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 空車着日（予定） -->
                    <span class="ef" id="WF_EMPARRDATE">
                        <asp:Label ID="LblEmpArrDate" runat="server" Text="空車着日（予定）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtEmpArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR %>)">
                            <asp:TextBox ID="TxtEmpArrDate" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon" MaxLength="10" onkeypress="CheckCalendar();" ></asp:TextBox>
                        </span>
                        <span></span>
                    </span>

                    <!-- 空車着日（実績） -->
                    <span class="ef" id="WF_ACTUALEMPARRDATE">
                        <asp:Label ID="LblActualEmpArrDate" runat="server" Text="空車着日（実績）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtActualEmpArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR %>)">
                            <asp:TextBox ID="TxtActualEmpArrDate" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon" MaxLength="10" onkeypress="CheckCalendar();" ></asp:TextBox>
                        </span>
                        <span></span>
                    </span>
                </p>
                <p id="KEY_LINE_6">
                    <!-- 積車油種コード -->
                    <span class="ef" id="WF_OILCODE">
                        <asp:Label ID="LblOilCode" runat="server" Text="積車油種" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('dummy', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('dummy');">
                            <asp:TextBox ID="TxtOilCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" ></asp:TextBox>
                        </span>
                        <asp:Label ID="LblOilCodeText" runat="server" Text="" maxlength="4" onkeypress="CheckNum();" ></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_7">
                    <!-- 油種区分(受発注用) -->
                    <span class="ef" id="WF_ORDERINGTYPE">
                        <asp:Label ID="LblOrderingType" runat="server" Text="油種区分(受発注用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOrderingType" runat="server" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                        <span></span>
                    </span>
                    <!-- 油種名(受発注用) -->
                    <span class="ef" id="WF_ORDERINGOILNAME">
                        <asp:Label ID="LblOrderingOilName" runat="server" Text="油種名(受発注用) " CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOrderingOilName" runat="server" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                        <span></span>
                    </span>


                </p>
                <p id="KEY_LINE_8">
                    <!-- 前回油種 -->
                    <span class="ef" id="WF_LASTOILCODE">
                        <asp:Label ID="LblLastOilCode" runat="server" Text="前回油種" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('dummy', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('dummy');">
                            <asp:TextBox ID="TxtLastOilCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" ></asp:TextBox>
                        </span>
                        <asp:Label ID="LblLastOilCodeText" runat="server" Text="" maxlength="4" onkeypress="CheckNum();" ></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_9">
                    <!-- 前回油種区分(受発注用) -->
                    <span class="ef" id="WF_PreOrderingType">
                        <asp:Label ID="LblPreOrderingType" runat="server" Text="前回油種区分(受発注用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPreOrderingType" runat="server" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                        <span></span>
                    </span>                    <!-- 前回油種名(受発注用) -->
                    <span class="ef" id="WF_PREORDERINGOILNAME">
                        <asp:Label ID="LblPreOrderingOilName" runat="server" Text="前回油種名(受発注用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPreOrderingOilName" runat="server" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                        <span></span>
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

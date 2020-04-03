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
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR" class="btn-sticky" value="クリア"  onclick="ButtonClick('WF_CLEAR');" />
                    </div>
                </div>
            </div>

            <div id="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- タンク車番号 -->
                    <span>
                        <asp:Label ID="WF_Sel_TANKNUMBER_L" runat="server" Text="タンク車番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:Label ID="WF_Sel_TANKNUMBER" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </span>

                </p>
                <p id="KEY_LINE_2">
                    <!-- 管轄支店コード -->
                    <span class="ef" id="WF_BRANCHCODE">
                        <asp:Label ID="LblBranchCode" runat="server" Text="管轄支店コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtBranchCode" runat="server" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                    </span>
                </p>
                <p id="KEY_LINE_3">
                    <!-- 所属営業所コード -->
                    <span class="ef" id="WF_OFFICECODE">
                        <asp:Label ID="LblOfficeCode" runat="server" Text="所属営業所コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOfficeCode" runat="server" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                    </span>

                    <!-- 所在地コード -->
                    <span class="ef" id="WF_LOCATIONCODE">
                        <asp:Label ID="LblLocationCode" runat="server" Text="所在地コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtLocationCode" runat="server" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 貨物駅名称 -->
                    <span class="ef" id="WF_STATIONNAME">

                    </span>

                    <!-- 貨物駅名称カナ -->
                    <span class="ef" id="WF_STATIONNAMEKANA">

                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 貨物駅種別名称 -->
                    <span class="ef" id="WF_STATIONTYPENAME">

                    </span>

                    <!-- 貨物駅種別名称カナ -->
                    <span class="ef" id="WF_STATIONTYPENAMEKANA">
                        <asp:Label ID="LblTypeNameKana" runat="server" Text="貨物駅種別名称カナ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtTypeNameKana" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="LblTypeNameKanaText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_6">
                    <!-- 発着駅フラグ -->
                    <span class="ef" id="WF_DEPARRSTATION">
                        <asp:Label ID="LblDepArrStation" runat="server" Text="発着駅フラグ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtDepArrStation', <%=LIST_BOX_CLASSIFICATION.LC_DEPARRSTATIONLIST%>)" onchange="TextBox_change('TxtDepArrStation');">
                            <asp:TextBox ID="TxtDepArrStation" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDepArrStationName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <span></span>
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

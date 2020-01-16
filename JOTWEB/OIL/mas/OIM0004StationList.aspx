<%@ Page Title="OIM0004L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0004StationList.aspx.vb" Inherits="JOTWEB.OIM0004StationList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0004WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0004LH" ContentPlaceHolderID="head" runat="server">
    <%--<link href='<%=ResolveUrl("~/OIL/css/OIM0004L.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0004L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=if(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0004L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <!-- ↓ これも使ってないなら消す！ ↓ --> 
                        <!-- 会社コード -->
                        <a style="display:none;">
                            <asp:Label ID="WF_SEL_CAMPCODE" runat="server" Text="会社" Font-Bold="True" Font-Underline="false"></asp:Label>
                            <asp:Label ID="WF_SEL_CAMPNAME" runat="server" Width="12em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </a>

                        <!-- 組織コード -->
                        <a style="display:none;">
                            <asp:Label ID="WF_SELUORG_L" runat="server" Text="運用組織" Font-Bold="True" Font-Underline="false"></asp:Label>
                            <asp:Label ID="WF_SELUORG_TEXT" runat="server" Width="12em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </a>
                        <!-- ↑ これも使ってないなら消す！ ↑ --> 

                        <!-- 一覧件数 -->
                        <asp:Label ID="WF_ListCNT" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </div>

                    <div class="rightSide">
                        <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="追加"    onclick ="ButtonClick('WF_ButtonINSERT');" />
                        <input type="button" id="WF_ButtonUPDATE" class="btn-sticky" value="DB更新"  onclick="ButtonClick('WF_ButtonUPDATE');" />
                        <input type="button" id="WF_ButtonCSV" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ"   onclick="ButtonClick('WF_ButtonCSV');" />
                        <input type="button" id="WF_ButtonPrint" class="btn-sticky" value="一覧印刷" onclick="ButtonClick('WF_ButtonPrint');" />
                        <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"       onclick="ButtonClick('WF_ButtonEND');" />
                        <div id="WF_ButtonFIRST" class="firstPage" runat="server"                    onclick="ButtonClick('WF_ButtonFIRST');"></div>
                        <div id="WF_ButtonLAST" class="lastPage" runat="server"                      onclick="ButtonClick('WF_ButtonLAST');"></div>
                    </div>

                </div> <!-- End class=actionButtonBox -->

            </div> <!-- End class="Operation" -->
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
        </div>

        <!--↓↓(20200109- 消す予定だが一旦画面映らなくする) 全体レイアウト　detailbox -->
        <!-- 全体レイアウト　detailbox -->
<%--        <div class="detailboxOnly" id="detailbox" style="display:none;" >
            <div id="detailbuttonbox" class="detailbuttonbox">
                <a>
                    <input type="button" id="WF_UPDATE" value="表更新" style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
                </a>
                <a>
                    <input type="button" id="WF_CLEAR" value="クリア" style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
                </a>
            </div>
            <div id="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- 選択No -->
                    <a>
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" Width="15em" CssClass="WF_TEXT_LABEL"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_2">
                    <!-- 削除フラグ -->
                    <a ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_DELFLG" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_3">
                    <!-- 貨物車コード -->
                    <a>
                        <asp:Label ID="LblStationCode" runat="server" Text="貨物車コード" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="TxtStationCode" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="LblStationCodeText" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 貨物コード枝番 -->
                    <a>
                        <asp:Label ID="LblBranch" runat="server" Text="貨物コード枝番" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="TxtBranch" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="LblBranchText" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_4">
                    <!-- 貨物駅名称 -->
                    <a>
                        <asp:Label ID="LblStationName" runat="server" Text="貨物駅名称" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="TxtStationName" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="LblStationNameText" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 貨物駅名称カナ -->
                    <a>
                        <asp:Label ID="LblStationNameKana" runat="server" Text="貨物駅名称カナ" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="TxtStationNameKana" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="LblStationNameKanaText" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_5">
                    <!-- 貨物駅種別名称 -->
                    <a>
                        <asp:Label ID="LblTypeName" runat="server" Text="貨物駅種別名称" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="TxtTypeName" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="LblTypeNameText" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 貨物駅種別名称カナ -->
                    <a>
                        <asp:Label ID="LblTypeNameKana" runat="server" Text="貨物駅種別名称" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="TxtTypeNameKana" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="LblTypeNameKanaText" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
            </div>
        </div>--%>

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


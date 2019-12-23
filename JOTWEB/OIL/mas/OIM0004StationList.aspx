<%@ Page Title="OIM0004L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0004StationList.aspx.vb" Inherits="JOTWEB.OIM0004StationList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0004WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0004LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIM0004L.css")%>' rel="stylesheet" type="text/css" />
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
            <div class="Operation" style="margin-left: 3em; margin-top: 0.5em; height: 1.8em;">
                <!-- 会社 -->
                <asp:Label ID="WF_SEL_CAMPCODE" runat="server" Text="会社" Font-Bold="True" Font-Underline="false"></asp:Label>
                <asp:Label ID="WF_SEL_CAMPNAME" runat="server" Width="12em" CssClass="WF_TEXT_LEFT"></asp:Label>

                <!-- 運用部署 -->
                <asp:Label ID="WF_SELUORG_L" runat="server" Text="運用部署" Font-Bold="True" Font-Underline="false"></asp:Label>
                <asp:Label ID="WF_SELUORG_TEXT" runat="server" Width="12em" CssClass="WF_TEXT_LEFT"></asp:Label>

                <!-- ボタン -->
                <a style="position:fixed;top:2.8em;left:49em;">
                    <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="追加"  style="Width:5em" onclick="ButtonClick('WF_ButtonINSERT');" />
                </a>
                <a style="position:fixed;top:2.8em;left:53.5em;">
                    <input type="button" id="WF_ButtonUPDATE" class="btn-sticky" value="DB更新"  style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
                </a>
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonCSV" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ"  style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonPrint" class="btn-sticky" value="一覧印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPrint');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" class="btn-sticky" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.2em;left:75em;">
                    <asp:Image ID="WF_ButtonFIRST2" runat="server" ImageUrl="~/img/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST2" runat="server" ImageUrl="~/img/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
            </div>
            <div id="divListArea">
                <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
            </div>
        </div>

        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
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


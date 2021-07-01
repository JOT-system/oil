<%@ Page Title="OIT0001L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0001EmptyTurnDairyList.aspx.vb" Inherits="JOTWEB.OIT0001EmptyTurnDairyList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0001LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0001L.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0001L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIT0001L" ContentPlaceHolderID="contents1" runat="server">
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
                        <%--<asp:Label ID="WF_ListCNT" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>--%>

                        <input type="button" id="WF_ButtonALLSELECT" class="btn-sticky" value="全選択"       onclick  ="ButtonClick('WF_ButtonALLSELECT');" />
                        <input type="button" id="WF_ButtonSELECT_LIFTED" class="btn-sticky" value="選択解除" onclick ="ButtonClick('WF_ButtonSELECT_LIFTED');" />
                        <input type="button" id="WF_ButtonLINE_LIFTED" class="btn-sticky" value="行削除"     onclick ="ButtonClick('WF_ButtonLINE_LIFTED');" />

                    </div>

                    <div class="rightSide">
                        <div style="display:none;">
                        </div>
                        <input type="button" id="WF_ButtonOTCOMPARE" class="btn-sticky" value="OT取込結果"  onclick ="ButtonClick('WF_ButtonOTCOMPARE');" />
                        <input type="button" id="WF_ButtonOTINSERT" class="btn-sticky" value="OT空回日報取込"  onclick ="ButtonClick('WF_ButtonOTINSERT');" />
                        <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="新規登録"        onclick ="ButtonClick('WF_ButtonINSERT');" />
                        <input type="button" id="WF_ButtonCSV" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ"           onclick="ButtonClick('WF_ButtonCSV');" />
                        <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"               onclick="ButtonClick('WF_ButtonEND');" />
                        <div style="display:none;" id="WF_ButtonFIRST" class="firstPage" runat="server"      onclick="ButtonClick('WF_ButtonFIRST');"></div>
                        <div style="display:none;" id="WF_ButtonLAST" class="lastPage" runat="server"        onclick="ButtonClick('WF_ButtonLAST');"></div>
                    </div>
                </div> <!-- End class=actionButtonBox -->
            </div> <!-- End class="Operation" -->
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>

            <%--<div class="Operation" style="margin-left: 3em; margin-top: 0.5em; height: 1.8em;">--%>

                <!-- 会社 -->
<%--                <asp:Label ID="WF_SEL_CAMPCODE" runat="server" Text="会社" Font-Bold="True" Font-Underline="false" Visible="false"></asp:Label>
                <asp:Label ID="WF_SEL_CAMPNAME" runat="server" Width="12em" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>--%>

                <!-- 運用部署 -->
<%--                <asp:Label ID="WF_SELUORG_L" runat="server" Text="運用部署" Font-Bold="True" Font-Underline="false" Visible="false"></asp:Label>
                <asp:Label ID="WF_SELUORG_TEXT" runat="server" Width="12em" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>--%>

                <!-- ボタン -->
<%--                <a style="position:fixed;top:2.8em;left:0.5em;">
                    <input type="button" id="WF_ButtonALLSELECT" class="btn-sticky" value="全選択"  style="Width:5em" onclick="ButtonClick('WF_ButtonALLSELECT');" />
                </a>
                <a style="position:fixed;top:2.8em;left:5em;">
                    <input type="button" id="WF_ButtonSELECT_LIFTED" class="btn-sticky" value="選択解除"  style="Width:5em" onclick="ButtonClick('WF_ButtonSELECT_LIFTED');" />
                </a>
                <a style="position:fixed;top:2.8em;left:9.5em;">
                    <input type="button" id="WF_ButtonLINE_LIFTED" class="btn-sticky" value="行削除"  style="Width:5em" onclick="ButtonClick('WF_ButtonLINE_LIFTED');" />
                </a>
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="新規登録"  style="Width:5em" onclick="ButtonClick('WF_ButtonINSERT');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonCSV" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ"  style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.2em;left:75em;display:none;">
                    <asp:Image ID="WF_ButtonFIRST2" runat="server" ImageUrl="~/img/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;display:none;">
                    <asp:Image ID="WF_ButtonLAST2" runat="server" ImageUrl="~/img/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>--%>
            <%--</div>--%>
<%--            <div id="divListArea">
                <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
            </div>--%>
        </div>

        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

        <!-- イベント用 -->
        <div hidden="hidden">
            <!-- GridView DBクリック-->
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
            <!-- GridView表示位置フィールド -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>

            <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD" runat="server" value="" type="text" />
            <!-- Textbox(Repeater) DBクリックフィールド -->
            <input id="WF_FIELD_REP" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text" />

            <!-- LeftBox Mview切替 -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
            <!-- LeftBox 開閉 -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
            <!-- Rightbox Mview切替 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text" />
            <!-- Rightbox 開閉 -->
            <input id="WF_RightboxOpen" runat="server" value="" type="text" />

            <!-- Textbox Print URL -->
            <input id="WF_PrintURL" runat="server" value="" type="text" />

            <!-- 一覧・詳細画面切替用フラグ -->
            <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />

            <!-- ボタン押下 -->
            <input id="WF_ButtonClick" runat="server" value="" type="text" />
            <!-- 権限 -->
            <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
            <!-- 空回日報取込ボタン押下フラグ(True:有効, False：無効) -->
            <input id="WF_OTReceiveFLG" runat="server" value="FALSE" type="text" />
        </div>
 
</asp:Content>


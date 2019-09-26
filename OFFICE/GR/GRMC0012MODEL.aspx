<%@ Page Title="MC0012" Language="vb" AutoEventWireup="false" CodeBehind="GRMC0012MODEL.aspx.vb" Inherits="OFFICE.GRMC0012MODEL" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRMC0012WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="MC0012H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/MC0012.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/MC0012.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=if(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="MC0012" ContentPlaceHolderID="contents1" runat="server">
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
                <a style="position:fixed;top:2.8em;left:53.5em;">
                    <input type="button" id="WF_ButtonUPDATE" value="DB更新"  style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
                </a>
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ"  style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonPrint" value="一覧印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPrint');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.2em;left:75em;">
                    <asp:Image ID="WF_ButtonFIRST2" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST2" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
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
                    <!-- ポイントパターン -->
                    <a ondblclick="Field_DBclick('WF_MODELPT', <%=LIST_BOX_CLASSIFICATION.LC_MODELPT%>)">
                        <asp:Label ID="WF_MODELPT_L" runat="server" Text="モデル距離パターン" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_MODELPT" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_MODELPT_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_4">
                    <!-- 取引先（出荷場所） -->
                    <a ondblclick="Field_DBclick('WF_TORICODES', <%=LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)">
                        <asp:Label ID="WF_TORICODES_L" runat="server" Text="取引先（出荷場所）" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_TORICODES" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_TORICODES_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 出荷場所 -->
                    <a ondblclick="Field_DBclick('WF_SHUKABASHO', <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" onchange="TextBox_change('WF_SHUKABASHO')">
                        <asp:Label ID="WF_SHUKABASHO_L" runat="server" Text="出荷場所" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_SHUKABASHO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_SHUKABASHO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_5">
                    <!-- 取引先（届先） -->
                    <a ondblclick="Field_DBclick('WF_TORICODET', <%=LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)">
                        <asp:Label ID="WF_TORICODET_L" runat="server" Text="取引先（届先）" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_TORICODET" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_TORICODET_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 届先 -->
                    <a ondblclick="Field_DBclick('WF_TODOKECODE', <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)"  onchange="TextBox_change('WF_TODOKECODE')">
                        <asp:Label ID="WF_TODOKECODE_L" runat="server" Text="届先" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_TODOKECODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_TODOKECODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_6">
                    <!-- ポイント -->
                    <a>
                        <asp:Label ID="WF_MODEL_L" runat="server" Text="モデル距離" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="WF_MODEL" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_MODEL_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
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

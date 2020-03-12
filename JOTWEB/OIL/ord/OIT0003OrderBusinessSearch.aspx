<%@ Page Title="OIT0003B" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0003OrderBusinessSearch.aspx.vb" Inherits="JOTWEB.OIT0003OrderBusinessSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content id="OIT0003BH" contentplaceholderid="head" runat="server">
    <!-- <link href='<%=ResolveUrl("~/OIL/css/OIT0003B.css")%>' rel="stylesheet" type="text/css" /> -->
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0003B.js")%>'></script>
</asp:Content>

<asp:Content ID="OIT0003B" ContentPlaceHolderID="contents1" runat="server">

    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO" class="btn-sticky" value="検索"  onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 会社コード -->
            <div class="inputItem" style="display:none;">
                <a>会社コード</a>
                <a class="ef" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 運用部署 -->
            <div class="inputItem" style="display:none;">
                <a>運用部署</a>
                <a class="ef" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
                    <asp:TextBox ID="WF_UORG" runat="server" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_UORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <div class="inputItem">
                <a id="WF_OFFICECODE_LABEL">営業所</a>
                <a  class="inline-radio" id="WF_OFFICECODE">
                    <div>
                        <asp:RadioButton ID="rbTohokuSendai" runat="server" GroupName="WF_OFFICECODE_SW" Text="仙台新港" />
                    </div>
                    <div>
                        <asp:RadioButton ID="rbKantoGoi" runat="server" GroupName="WF_OFFICECODE_SW" Text="五井" />
                        <asp:RadioButton ID="rbKantoKinoene" runat="server" GroupName="WF_OFFICECODE_SW" Text="甲子" />
                        <asp:RadioButton ID="rbKantoSodegaura" runat="server" GroupName="WF_OFFICECODE_SW" Text="袖ヶ浦" />
                        <asp:RadioButton ID="rbKantoNegishi" runat="server" GroupName="WF_OFFICECODE_SW" Text="根岸" />
                    </div>
                    <div>
                        <asp:RadioButton ID="rbChubuYokkaichi" runat="server" GroupName="WF_OFFICECODE_SW" Text="四日市" />
                        <asp:RadioButton ID="rbChubuMieShiohama" runat="server" GroupName="WF_OFFICECODE_SW" Text="三重塩浜" />
                    </div>
                </a>
            </div>

            <div class="inputItem">
                <a id="WF_ORDER_LABEL">オーダー</a>
                <a  class="inline-radio" id="WF_ORDER">
                    <div>
                        <asp:RadioButton ID="rbRunDay" runat="server" GroupName="WF_ORDER_SW" Text="当日運行" />
                        <asp:RadioButton ID="rbRunNextDay" runat="server" GroupName="WF_ORDER_SW" Text="翌日運行" />
                        <asp:RadioButton ID="rbRunTwoDayLater" runat="server" GroupName="WF_ORDER_SW" Text="翌々日以降" />
                    </div>
                </a>
            </div>
        </div> <!-- End inputBox -->
    </div>

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

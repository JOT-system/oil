<%@ Page Title="MB0002" Language="vb" AutoEventWireup="false" CodeBehind="GRMB0002STAFFORG.aspx.vb" Inherits="OFFICE.GRMB0002STAFFORG" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRMB0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="MB0002H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/MB0002.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/MB0002.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=if(IsPostBack = True, "1", "0")%>';
        var EXTRALIST = '<%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>';
    </script>
</asp:Content>

<asp:Content ID="MB0002" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <div class="Operation">
            <p>
                <!-- 作業部署 -->
                <a>
                    <asp:Label ID="WF_SORG_L" runat="server" Text="作業部署" Height="1.5em" Width="5em" Font-Bold="true"></asp:Label>
                </a>
                <a>
                    <asp:Label ID="WF_SORG" runat="server" Width="7em"></asp:Label>
                </a>
                <a>
                    <asp:Label ID="WF_SORG_TEXT" runat="server" Width="12em"></asp:Label>
                </a>
            </p>
            <p>
                <!-- 会社 -->
                <a>
                    <asp:Label ID="WF_SELCAMPCODE_L" runat="server" Text="会社" Height="1.5em" Font-Bold="true" Font-Underline="true"></asp:Label>
                </a>
                <a ondblclick="Field_DBclick('WF_SELCAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>)">
                    <asp:TextBox ID="WF_SELCAMPCODE" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_SELCAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
                </a>

                <!-- 管理部署 -->
                <a>
                    <asp:Label ID="WF_SELMORG_L" runat="server" Text="管理部署" Height="1.5em" Font-Bold="true" Font-Underline="true"></asp:Label>
                </a>
                <a ondblclick="Field_DBclick('WF_SELMORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>)">
                    <asp:TextBox ID="WF_SELMORG" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_SELMORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
                </a>
            </p>
            
            <!-- ボタン -->
            <a style="position:fixed; top:2.8em; left:53.5em;">
                <input type="button" id="WF_ButtonExtract" value="絞り込み" style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
            </a>
            <a style="position:fixed; top:2.8em; left:58em;">
                <input type="button" id="WF_ButtonUPDATE" value="DB更新" style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
            <a style="position:fixed; top:2.8em; left:62.5em;">
                <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ" style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
            </a>
            <a style="position:fixed; top:2.8em; left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了" style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div class="detailbox" id="detailbox">
        <div style="height:1em;"></div>
        <div id="divListArea">
            <asp:panel id="pnlListArea" runat="server"></asp:panel>
        </div>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>      <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>     <!-- GridView表示位置フィールド -->

        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        
        <input id="WF_DISP_SaveX" runat="server" value="" type="text" />            <!-- 明細位置X軸 -->
        <input id="WF_DISP_SaveY" runat="server" value="" type="text" />            <!-- 明細位置Y軸 -->
        <input id="WF_SelectLine" runat="server" value="" type="text" />            <!-- リスト変更行数 -->
        <input id="WF_SelectFIELD" runat="server" value="" type="text" />            <!-- リスト変更行数 -->

        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />       <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->

        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->

        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />         <!-- 権限 -->
    </div>
</asp:Content>

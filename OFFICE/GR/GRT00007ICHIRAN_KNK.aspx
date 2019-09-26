<%@ Page Title="T00007I" Language="vb" AutoEventWireup="false" CodeBehind="GRT00007ICHIRAN_KNK.aspx.vb" Inherits="OFFICE.GRT00007ICHIRAN_KNK" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRT00007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="T00007H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/T00007I_KNK.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/T00007I_KNK.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        var EXTRALIST = '<%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>';
    </script>
</asp:Content>

<asp:Content ID="T00007I" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <div class="Operation" style="margin-left:3em; margin-top:0.5em; height:1.8em;">
            <!-- ■　選択　■ -->
            <a style="position:fixed;top:2.8em;left:3.0em;">
                <asp:Label ID="WF_DATECODE_LABEL" runat="server" Text="絞込日付" Height="1.5em" Font-Bold="True" Font-Underline="True"></asp:Label>
            </a>
            <a style="position:fixed;top:2.8em;left:7.5em;" ondblclick="Field_DBclick('WF_WORKDATE' , <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                <asp:TextBox ID="WF_WORKDATE" runat="server" Height="1.2em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet" ></asp:TextBox>
            </a>
            <a style="position:fixed;top:2.8em;left:13.5em;">
                <asp:Label ID="WF_WORKDATE_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>

            <a style="position:fixed;top:2.9em;left:16.5em;">
                <asp:Label ID="WF_STAFFCODE_LABEL" runat="server" Text="絞込従業員" Height="1.5em" Font-Bold="True" Font-Underline="True"></asp:Label>
            </a>
            <a style="position:fixed;top:2.8em;left:22em;" ondblclick="Field_DBclick('WF_STAFFCODE' , <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>)" onchange="TextBox_change('WF_STAFFCODE');">
                <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.2em" Width="7em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            </a>
            <a style="position:fixed;top:2.8em;left:28.5em;">
                <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Height="1.2em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
            </a>

            <!-- ■　日報一括取込期間　■ -->
            <a style="position:fixed;top:4.3em;left:3em;">
                <asp:Label ID="WF_NIPPO_LABEL" runat="server" Text="対象期間" Height="1.5em" Font-Bold="True" Font-Underline="false"></asp:Label>
            </a>
            <a style="position:fixed;top:4.2em;left:7.5em;"">
                <asp:TextBox ID="WF_NIPPO_FROM" runat="server" Height="1.2em" Width="7em" CssClass="WF_TEXTBOX_CSS" style="text-align: right; "></asp:TextBox>
            </a>
            <a style="position:fixed;top:4.3em;left:14em;">
                <asp:Label ID="WF_NIPPO_FROM_LABEL" runat="server" Text="日～" Height="1.0em"  Font-Underline="false"></asp:Label>
            </a>
            <a style="position:fixed;top:4.2em;left:16.5em;">
                <asp:TextBox ID="WF_NIPPO_TO" runat="server" Height="1.2em" Width="7em" CssClass="WF_TEXTBOX_CSS" style="text-align: right; "></asp:TextBox>
            </a>
            <a style="position:fixed;top:4.3em;left:23em;">
                <asp:Label ID="WF_NIPPO_TO_LABEL" runat="server" Text="日" Height="1.0em"  Font-Underline="false"></asp:Label>
            </a>
            <a style="position:fixed;top:4.3em;left:28.5em;">
                <input type="button" id="WF_ButtonNIPPO" value="日報一括取込" style="width:7em" onclick="ButtonClick('WF_ButtonNIPPO');" />
            </a>
            <a style="position:fixed;top:4.3em;left:35em;">
                <input type="button" id="WF_ButtonCALC" value="一括残業計算"  style="width:7em" onclick="ButtonClick('WF_ButtonCALC');" />
            </a>

            <!-- ■　ボタン　■ -->
            <a style="position:fixed;top:3.1em;left:42.5em;">
                <input type="button" id="WF_ButtonSAVE" value="一時保存"  style="width:5em" onclick="ButtonClick('WF_ButtonSAVE');" />
            </a>
            <a style="position:fixed;top:3.1em;left:49em;">
                <input type="button" id="WF_ButtonExtract" value="絞り込み"  style="width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
            </a>
            <a style="position:fixed;top:3.1em;left:53.5em;">
                <input type="button" id="WF_ButtonUPDATE" value="DB更新"  style="width:5em" disabled="disabled" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
            <a style="position:fixed;top:3.1em;left:58em;">
                <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ"  style="width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
            </a>
            <a style="position:fixed;top:3.1em;left:62.5em;">
                <input type="button" id="WF_ButtonPrint" value="一覧印刷"  style="width:5em" onclick="ButtonClick('WF_ButtonPrint');" />
            </a>
            <a style="position:fixed;top:3.1em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了"  style="width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>

            <a style="position:fixed; top:4.4em; left:75em;">
                <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>
            <a style="position:fixed; top:4.4em; left:77em;">
                <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div class="detailbox" id="detailbox">
        <div style="height:0.25em;"></div>
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

        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />       <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->

        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->

        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />         <!-- 権限 -->
    </div>
</asp:Content>

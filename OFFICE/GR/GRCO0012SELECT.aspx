<%@ Page Title="CO0012S" Language="vb" AutoEventWireup="false" CodeBehind="GRCO0012SELECT.aspx.vb" Inherits="OFFICE.GRCO0012SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRCO0012WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="CO0012SH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/CO0012S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/CO0012S.js")%>'></script>
</asp:Content>

<asp:Content ID="CO0012S" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　searchbox -->
        <div  class="searchbox" id="searchbox" >
             <!-- ○ 固定項目 ○ -->
            <a style="position:fixed;top:2.8em;left:62.5em;">
                <input type="button" id="WF_ButtonDO" value="実行"  style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
            </a>
            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
            
            <!-- ○ 変動項目 ○ -->
            <a style="position:fixed;top:5em;left:4em;font-weight:bold;text-decoration:underline">有効年月日</a> 
            <a style="position:fixed;top:5em;left:11.5em;">範囲指定</a>
            <a style="position:fixed;top:5em;left:18em;" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
                <asp:TextBox ID="WF_STYMD" runat="server" Height="1.4em" Width="10em"></asp:TextBox>
            </a>
            <a style="position:fixed;top:5em;left:42.5em;">～</a>
            <a style="position:fixed;top:5em;left:44em;" ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
                <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.4em" Width="10em"></asp:TextBox>
            </a>
            
            <!-- 　端末ＩＤ(From-To)　 -->
            <a style="position:fixed;top:7.7em;left:4em;font-weight:bold;text-decoration:underline">端末ID</a>
            <a style="position:fixed;top:7.7em;left:11.5em;">範囲指定</a>
            <a style="position:fixed;top:7.7em;left:18em;" ondblclick="Field_DBclick('WF_TERMIDF', <%=LIST_BOX_CLASSIFICATION.LC_TERM   %>)">
                <asp:TextBox ID="WF_TERMIDF" runat="server" Height="1.4em" Width="10em"></asp:TextBox>
            </a>
            <a style="position:fixed;top:7.7em;left:42.5em;" >～</a>
            <a style="position:fixed;top:7.7em;left:44em;" ondblclick="Field_DBclick('WF_TERMIDT',<%=LIST_BOX_CLASSIFICATION.LC_TERM   %>)">
                <asp:TextBox ID="WF_TERMIDT" runat="server" Height="1.4em" Width="10em"></asp:TextBox>
            </a>

            <a hidden="hidden">
                <input id="WF_FIELD"  runat="server" value=""  type="text" />          <!-- Textbox DBクリックフィールド -->
                <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />  <!-- Textbox DBクリックフィールド -->
                <input id="Text1" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
                <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
                <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
                <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->

                <input id="WF_TextBoxchange" runat="server" value="" type="text"/>     <!-- TextBox変更フィールド -->
                <input id="WF_TERMIDF_Text" runat="server" value="" type="text"/>        <!-- MEMO変更フィールド -->
                <input id="WF_TERMIDT_Text" runat="server" value="" type="text"/>        <!-- MEMO変更フィールド -->
                <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
            </a>
        </div>
    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />
</asp:Content>
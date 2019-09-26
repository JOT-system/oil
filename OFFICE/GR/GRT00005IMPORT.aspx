<%@ Page Title="T00005I" Language="vb" AutoEventWireup="false" CodeBehind="GRT00005IMPORT.aspx.vb" Inherits="OFFICE.GRT00005IMPORT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00005WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00005IH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00005I.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00005I.js")%>"></script>
</asp:Content> 
<asp:Content ID="T00005I" ContentPlaceHolderID="contents1" runat="server">

       <!-- 全体レイアウト　headerbox -->
        <div  class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <!-- ■　選択　■ -->
                <a style="position:fixed;top:2.9em;left:3em;">
                    <asp:Label ID="WF_STAFFCODE_LABEL" runat="server" Text="乗務員" Height="1.3em" Font-Bold="True" Font-Underline="True"></asp:Label>
                </a>
                <a style="position:fixed;top:2.8em;left:7em;" ondblclick="Field_DBclick('WF_STAFFCODE' ,  <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>)">
                    <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                </a>
                <a style="position:fixed;top:2.8em;left:13.5em;">
                    <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Height="1.3em" Width="7em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <a style="position:fixed;top:2.9em;left:18em;">
                    <asp:Label ID="WF_YMD_LABEL" runat="server" Text="出庫日" Height="1.3em" Font-Bold="True" Font-Underline="True"></asp:Label>
                </a>
                <a style="position:fixed;top:2.8em;left:23.5em;" ondblclick="Field_DBclick('WF_YMD' ,  <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
                    <asp:TextBox ID="WF_YMD" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>
                <a style="position:fixed;top:2.8em;left:30em;" hidden="hidden">
                    <asp:Label ID="WF_YMD_TEXT" runat="server" Height="1.1em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:32em;">
                    <input type="button" id="WF_ButtonDownload" value="光英受信"  style="Width:5em" onclick="ButtonClick('WF_ButtonDownload');" />
                </a>

                <a style="position:fixed;top:2.8em;left:37em;">
                    <input type="button" id="WF_ButtonSAVE" value="一時保存"  style="Width:5em" onclick="ButtonClick('WF_ButtonSAVE');" />
                </a>
                <a style="position:fixed;top:2.8em;left:44.5em;">
                    <input type="button" id="WF_ButtonExtract" value="絞り込み"  style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
                </a>
                <a style="position:fixed;top:2.8em;left:49em;">
                    <input type="button" id="WF_ButtonNEW" value="新規"  style="Width:5em" onclick="ButtonClick('WF_ButtonNEW');" />
                </a>
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
                    <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
            </div>
            <!-- 一覧レイアウト -->
            <div id="divListArea">
                <asp:panel id="pnlListArea" runat="server" ></asp:panel>
            </div>
        </div>  

        <!-- Work レイアウト -->
        <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>     <!-- GridViewダブルクリック -->

            <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->
            <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />        <!-- 権限 -->

            <input id="WF_FIELD"  runat="server" value=""  type="text" />               <!-- Textbox DBクリックフィールド -->
            
            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>        <!-- Leftbox Mview切替 -->
            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />         <!-- Leftbox 開閉 -->

            <input id="WF_SelectedIndex" runat="server" value="" type="text"/>          <!-- Textbox DBクリックフィールド -->

            <input id="WF_RightViewChange" runat="server" value="" type="text"/>        <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />         <!-- Rightbox 開閉 -->
            
            <input id="WF_PrintURL" runat="server" value=""  type="text" />             <!-- Textbox Print URL -->
            <asp:ListBox ID="WF_KoueiLoadFile" runat="server"></asp:ListBox>            <!-- List光栄読込中ファイル -->
        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <!-- Work レイアウト -->
        <LSINC:work id="work" runat="server" />

</asp:Content>

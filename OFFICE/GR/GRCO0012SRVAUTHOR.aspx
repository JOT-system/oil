<%@ Page Title="CO0012" Language="vb" AutoEventWireup="false" CodeBehind="GRCO0012SRVAUTHOR.aspx.vb" Inherits="OFFICE.GRCO0012SRVAUTHOR" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRCO0012WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="CO0011H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/CO0012.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=if(IsPostBack = True, "1", "0")%>';
    </script>
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/CO0012.js")%>'></script>

</asp:Content>

<asp:Content ID="CO0012" ContentPlaceHolderID="contents1" runat="server">


        <!-- 全体レイアウト　headerbox -->
        <div  class="headerbox" id="headerbox">
            <div class="Operation" style="margin-left:3em;margin-top:0.5em;">
                <!-- ■　サーバＩＤ　■ -->
                <a ondblclick="Field_DBclick('WF_SELTERM', <%=LIST_BOX_CLASSIFICATION.LC_TERM  %>)">
                    <asp:Label ID="WF_LabelTERM" runat="server" Text="端末ID" Height="1.5em" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <asp:TextBox ID="WF_SELTERM" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                </a>
                <a>　　</a>
                <!-- ■　オブジェクト　■ -->
                <a ondblclick="Field_DBclick('WF_SELOBJECT',  911)">
                    <asp:Label ID="WF_LabelOBJECT" runat="server" Text="オブジェクト" Height="1.5em" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <asp:TextBox ID="WF_SELOBJECT" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                    <asp:Label ID="WF_SELOBJECT_TEXT" runat="server" Width="12em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                <a>　　　　　　</a>
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:49em;">
                    <input type="button" id="WF_ButtonExtract" value="絞り込み"  style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
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
                    <asp:Image ID="WF_ButtonFIRST2" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST2" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
            </div>
            <!-- 一覧レイアウト -->
            <div id="divListArea">
                <asp:panel id="pnlListArea" runat="server" ></asp:panel>
            </div>
        </div>

        <!-- 全体レイアウト　detailbox -->
        <div  class="detailbox" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <a style="position:relative;top:0.5em;left:49em;">
                    <input type="button" id="WF_UPDATE" value="表更新"  style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
                </a>
                <a style="position:relative;top:0.5em;left:49em;margin: 0em 0em 0em 0.2em;">
                    <input type="button" id="WF_CLEAR" value="クリア"  style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
                </a>
            </div>
            <div id="detailkeybox">
                <!-- ■　選択No　■ -->
                <a style="position:absolute;top:0.5em;left:3em; width:60em;">
                    <asp:Label ID="Label2" runat="server" Text="選択No" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_Sel_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>

                <!-- ■　サーバＩＤ　■ -->
                <a style="position:absolute;top:1.8em;left:3em; width:60em;" ondblclick="Field_DBclick('WF_TERMID', <%=LIST_BOX_CLASSIFICATION.LC_TERM  %>)">
                    <asp:Label ID="WF_TERMID_L" runat="server" Text="端末ID" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT" Font-Underline="True"></asp:Label>
                    <asp:TextBox ID="WF_TERMID" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                </a>

                <!-- ■　会社　■ -->
                <a style="position:absolute;top:3.1em;left:3em; width:60em;" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>)">
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT" Font-Underline="True"></asp:Label>
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　オブジェクト　■ -->
                <a style="position:absolute;top:4.4em;left:3em; width:60em;" ondblclick="Field_DBclick('WF_OBJECT', 901)">
                    <asp:Label ID="WF_OBJECT_L" runat="server" Text="オブジェクト" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT"  Font-Underline="True"></asp:Label>
                    <asp:TextBox ID="WF_OBJECT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT" ></asp:TextBox>
                    <asp:Label ID="WF_OBJECT_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　ロール　■ -->
                <a style="position:absolute;top:5.7em;left:3em; width:60em;" ondblclick="Field_DBclick('WF_ROLE', <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>)">
                    <asp:Label ID="WF_ROLE_L" runat="server" Text="ロール" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT"  Font-Underline="True"></asp:Label>
                    <asp:TextBox ID="WF_ROLE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT" ></asp:TextBox>
                    <asp:Label ID="WF_ROLE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　有効年月日　■ -->
                <a style="position:absolute;top:7.0em;left:3em; width:60em;">
                    <asp:Label ID="WF_YMD_L" runat="server" Text="有効年月日" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT" Font-Underline="True"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_STYMD',  <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_STYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                    </b>
                    <asp:Label ID="Label1" runat="server" Text=" ～ " CssClass="WF_TEXT_LEFT"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                    </b>
                </a>

                <!-- ■　ロール名称（短）　■ -->
                <a style="position:absolute;top:8.3em;left:3em; width:60em;">
                    <asp:Label ID="WF_CODENAMES_L" runat="server" Text="ロール名称（短）" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_CODENAMES" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT" ></asp:TextBox>
                </a>

                <!-- ■　ロール名称（長）　■ -->
                <a style="position:absolute;top:9.6em;left:3em; width:60em;">
                    <asp:Label ID="WF_CODENAMEL_L" runat="server" Text="ロール名称（長）" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_CODENAMEL" runat="server" Height="1.1em" Width="30em" CssClass="WF_TEXT_LEFT" ></asp:TextBox>
                </a>

                <!-- ■　削除フラグ　■ -->
                <a style="position:absolute;top:10.9em;left:3em; width:60em;" ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT" Font-Underline="True"></asp:Label>
                    <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT" ></asp:TextBox>
                    <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
             </div> 
        </div>

        <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>     <!-- GridViewダブルクリック -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>    <!-- GridView表示位置フィールド -->

            <input id="WF_FIELD"  runat="server" value=""  type="text" />               <!-- Textbox DBクリックフィールド -->
            <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />       <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text" />       <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->

            <input id="WF_UPLOAD" runat="server" value="" type="text"/>                 <!-- ドロップ処理結果格納フィールド -->

            <input id="WF_PrintURL" runat="server" value=""  type="text" />             <!-- Textbox Print URL -->

            <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->
            <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />        <!-- 権限 -->
        </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />
</asp:Content>

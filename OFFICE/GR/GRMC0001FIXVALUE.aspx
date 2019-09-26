<%@ Page Title="MC0001" Language="vb" AutoEventWireup="false" CodeBehind="GRMC0001FIXVALUE.aspx.vb" Inherits="OFFICE.GRMC0001FIXVALUE" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="~/GR/inc/GRMC0001WRKINC.ascx" tagname="work" tagprefix="MSINC" %>

<asp:Content ID="GRMC0001H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/MC0001.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/MC0001.js")%>"></script>
</asp:Content> 
<asp:Content ID="GRMC0001" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div  class="headerbox" id="headerbox">
        <div class="Operation">
            <!-- ■　分類　■ -->
            <a>
                <asp:Label ID="WF_LabelBUNRUI" runat="server" Text="分類" Height="1.5em" Font-Bold="true" Font-Underline="true"></asp:Label>
            </a>
            <a ondblclick="Field_DBclick('WF_SELBUNRUI',  <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                <asp:TextBox ID="WF_SELBUNRUI" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_SELBUNRUI_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
            </a>

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
            <asp:panel id="pnlListArea" runat="server" />
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div  class="detailbox" id="detailbox">
        <div id="detailbuttonbox" class="detailbuttonbox">
            <a>
                <input type="button" id="WF_UPDATE" value="表更新"  style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
            </a>
            <a>
                <input type="button" id="WF_CLEAR" value="クリア"  style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
            </a>
        </div>
        <div id="detailkeybox">
            <p id="KEY_LINE_1">
                <!-- ■　項番　■ -->
                <a>
                    <asp:Label ID="Label4" runat="server" Text="項番" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_2">
                <!-- ■　会社　■ -->
                <a name="KEY_2" >
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE" runat="server" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_3">
                <!-- ■　分類　■ -->
                <a name="KEY_3" >
                    <asp:Label ID="label3" runat="server" Text="分類" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_BUNRUI',  <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                        <asp:TextBox ID="WF_BUNRUI" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_BUNRUI_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　マスタキー　■ -->
                <a name="KEY_4" >
                    <asp:Label ID="WF_KEYCODE_L" runat="server" Text="マスタキー" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_KEYCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_KEYCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_4">
                <!-- ■　有効年月日　■ -->
                <a name="KEY_5" >
                    <asp:Label ID="WF_STYMD_L" runat="server" Text="有効年月日" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_STYMD',  <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_STYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_ENDYMD_L" runat="server" Text="～" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <b ondblclick="Field_DBclick( 'WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                </a>

                <!-- ■　削除フラグ　■ -->
                <a name="KEY_5" >
                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_DELFLG',  <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                        <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_5">
                <!-- ■　値１～５　■ -->
                <a>
                    <asp:Label ID="WF_VALUE1_L" runat="server" Text="値（１～５）" Width="7em"  CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_VALUE1" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                </a>
                <a>
                    <asp:TextBox ID="WF_VALUE2" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>
                <a>
                    <asp:TextBox ID="WF_VALUE3" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                </a>
                <a>
                    <asp:TextBox ID="WF_VALUE4" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>
                <a>
                    <asp:TextBox ID="WF_VALUE5" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>
            </p>    

            <p id="KEY_LINE_6">
                <!-- ■　画面名称(短)　■ -->
                <a>
                    <asp:Label ID="WF_NAMES_L" runat="server" Text="画面名称（短）" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_NAMES" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>

                <!-- ■　画面名称(長)　■ -->
                <a>
                    <asp:Label ID="WF_NAMEL_L" runat="server" Text="画面名称（長）" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_NAMEL" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>
            </p>
            <p id="KEY_LINE_7">
                <!-- ■　システムキーフラグ　■ -->
                <a>
                    <asp:Label ID="WF_SYSTEMFLG_L" runat="server" Text="システム用" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_SYSTEMFLG" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>
            </p>
        </div>

        <!-- DETAIL画面 -->
        <span class="WF_DViewRep1_Area" id="WF_DViewRep1_Area"></span>
    </div>

    <%-- rightview --%>
    <MSINC:rightview id="rightview" runat="server" />
    <%-- leftview --%>
    <MSINC:leftview id="leftview" runat="server" />

    <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>   <!-- GridViewダブルクリック -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>  <!-- GridView表示位置フィールド -->
 
            <input id="WF_FIELD"  runat="server" value=""  type="text" />             <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD_REP"  runat="server" value=""  type="text" />         <!-- Textbox(Repeater) DBクリックフィールド -->

            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>      <!-- Leftbox Mview切替 -->
            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />       <!-- Leftbox 開閉 -->

            <input id="WF_RightViewChange" runat="server" value="" type="text"/>      <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />       <!-- Rightbox 開閉 -->

            <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />     <!-- Textbox DBクリックフィールド -->

            <input id="WF_PrintURL" runat="server" value=""  type="text" />           <!-- Textbox Print URL -->

            <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
            <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />      <!-- 権限 -->
    </div>

    <%-- Work --%>
    <MSINC:work id="work" runat="server" />

</asp:Content>
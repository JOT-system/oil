<%@ Page Title="CO0008" Language="vb" AutoEventWireup="false" CodeBehind="GRCO0008PROFMMAP.aspx.vb" Inherits="OFFICE.GRCO0008PROFMMAP" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRCO0008WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="CO0008SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/CO0008.css")%>"/>
    <script type="text/javascript">
            var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
            var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/CO0008.js")%>"></script>
</asp:Content> 

<asp:Content ID="CO0008S" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　headerbox -->
        <div  class="headerbox" id="headerbox">
            <div class="Operation">
                <!-- 画面ＩＤ -->
                <a>
                    <asp:Label ID="WF_LabelMAP" runat="server" Text="画面ID" Height="1.5em" Font-Bold="true" Font-Underline="true"></asp:Label>
                </a>
                <a ondblclick="Field_DBclick('WF_SELMAP', <%=LIST_BOX_CLASSIFICATION.LC_URL%>)">
                    <asp:TextBox ID="WF_SELMAP" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_SELMAP_TEXT" runat="server" Height="1.5em" Width="30em" CssClass="WF_TEXT"></asp:Label>
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
                <asp:panel id="pnlListArea" runat="server" ></asp:panel>
            </div>
        </div>

        <!-- 全体レイアウト　detailbox -->
        <div  class="detailbox" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <a >
                    <input type="button" id="WF_UPDATE" value="表更新" style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
                </a>
                <a >
                    <input type="button" id="WF_CLEAR" value="クリア" style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
                </a>
            </div>
            <div id="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- ■　選択No　■ -->
                    <a>
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </a>
                </p> 
                <p id="KEY_LINE_2">
                    <!-- ■　会社コード　■ -->
                    <a>
                        <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社コード" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                        <asp:Label ID="WF_CAMPCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_3">
                    <!-- ■　親画面ＩＤ　■ -->
                    <a>
                        <asp:Label ID="WF_MAPIDP_L" runat="server" Text="親画面ID" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                        <asp:Label ID="WF_MAPIDP" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:Label ID="WF_MAPIDP_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■　親変数・名称　■ -->
                    <a ondblclick="Field_DBclick('WF_VARIANTP', <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>)">
                        <asp:Label ID="WF_VARIANTP_L" runat="server" Text="親変数・名称" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_VARIANTP" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                        <asp:Label ID="WF_VARIANTP_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_4">
                    <!-- ■　有効年月日　■ -->
                    <a>
                        <asp:Label ID="WF_YMD_L" runat="server" Text="有効年月日" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                        <b ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_STYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </b>
                        <asp:Label ID="Label1" runat="server" Text=" ～ " CssClass="WF_TEXT_LEFT"></asp:Label>
                        <b ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■　削除フラグ　■ -->
                    <a ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>
            </div>
            <div id="detailItems">
            <!-- ■■■　明細ヘッダ　■■■ -->
            <table id="detailheader" >
                <tr>
                    <td style="width:2em;">
                        <!-- ■　左右位置　■ -->
                        <asp:Label ID="Label17" runat="server" Text="位置" Width="2.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="width:2em;">
                        <!-- ■　項番　■ -->
                        <asp:Label ID="Label13" runat="server" Text="項番" Width="2.0em" style="margin:1px 0px 0px 0px;" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="width:9em;">
                        <!-- ■　切替　■ -->
                        <asp:Label ID="Label3" runat="server" Text="定義内容" Width="9.0em" style="margin:1px 0px 0px 0px;" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="width:10.3em;">
                        <!-- ■　見出名称　■ -->
                        <asp:Label ID="Label15" runat="server" Text="見出名称" Width="10.0em" style="margin:1px 0px 0px 0px;" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="width:10.3em;">
                        <!-- ■　ボタン名称　■ -->
                        <asp:Label ID="Label19" runat="server" Text="ボタン名称" Width="10.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="width:10.3em;">
                        <!-- ■　画面ＩＤ　■ -->
                        <asp:Label ID="Label20" runat="server" Text="画面ＩＤ" Width="10.0em" CssClass="WF_TEXT_CENTER" Font-Underline="True"></asp:Label>
                    </td>

                    <td style="width:10.2em; text-align:center;">
                        <!-- ■　画面名称　■ -->
                        <asp:Label ID="Label21" runat="server" Text="画面名称" Width="10.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>

                    <td style="width:10.2em; text-align:center;">
                        <!-- ■　変数　■ -->
                        <asp:Label ID="Label22" runat="server" Text="変数" Width="10.0em" CssClass="WF_TEXT_CENTER" Font-Underline="True"></asp:Label>
                    </td>

                    <td style="width:10.2em; text-align:center;">
                        <!-- ■　変数名称　■ -->
                        <asp:Label ID="Label23" runat="server" Text="変数名称" Width="10.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </td>
                </tr>
            </table>

            <!-- ■■■　明　細　■■■ -->
            <span class="WF_Repeater" style="position:fixed;height:9.5em;left:3em;overflow:auto;background-color:white;table-layout: fixed" >

                <asp:Repeater ID="WF_Repeater" runat="server" >
                    <HeaderTemplate>
                        <table id="detailRepeater">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                        <td style="width:2em;">
                            <!-- ■　左右位置　■ -->
                            <asp:Label ID="WF_Rep_POSICOL" runat="server" Text="" Width="2.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
                        </td>

                        <td style="width:2em;">
                            <!-- ■　項番　■ -->
                            <asp:Label ID="WF_Rep_POSIROW" runat="server" Text="" Width="2.0em" CssClass="WF_TEXT_CENTER" ></asp:Label>
                        </td>

                            <!-- ■　切替　■ -->
                        <td style="width:9em;">
                            <asp:RadioButton ID="WF_Rep_SW1" runat="server" GroupName="WF_Rep_SW" Text=" 見出し　" Width="4em" />
                            <asp:RadioButton ID="WF_Rep_SW2" runat="server" GroupName="WF_Rep_SW" Text=" ボタン" Width="4em" />
                        </td>

                        <td style="width:10em;">
                            <!-- ■　見出名称　■ -->
                            <asp:TextBox ID="WF_Rep_TITLE" runat="server" Text="" Height="1.1em" Width="11.5em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                        </td>

                        <td style="width:10em;">
                            <!-- ■　ボタン名称　■ -->
                            <asp:TextBox ID="WF_Rep_NAMES" runat="server" Height="1.1em" Width="11.5em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                        </td>

                        <td style="width:10em;">
                            <!-- ■　画面ＩＤ　■ -->
                            <asp:TextBox ID="WF_Rep_MAPID" runat="server" Height="1.1em" Width="11.5em" CssClass="WF_TEXT_LEFT"></asp:TextBox>
                        </td>

                        <td style="width:10em;">
                            <!-- ■　画面名称　■ -->
                            <asp:Label ID="WF_Rep_MAPID_TEXT" runat="server" Width="10.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </td>

                        <td style="width:10em;">
                            <!-- ■　変数　■ -->
                            <asp:TextBox ID="WF_Rep_VARIANT" runat="server" Height="1.1em" Width="11.5em" CssClass="WF_TEXT_LEFT"></asp:TextBox>

                        </td>
                        <td style="width:10em;">
                            <!-- ■　変数名称　■ -->
                            <asp:Label ID="WF_Rep_VARIANT_TEXT" runat="server" Width="10.0em" CssClass="WF_TEXT_LEFT" ></asp:Label>

                        </td>
                    </tr>
                    </ItemTemplate>

                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
             
                </asp:Repeater>
            </span>
            </div>
        </div>

  
 
        <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>         <!-- GridViewダブルクリック -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>        <!-- GridView表示位置フィールド -->
            <input id="WF_FIELD"  runat="server" value=""  type="text" />                   <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD_REP"  runat="server" value=""  type="text" />               <!-- Textbox(Repeater) DBクリックフィールド -->

            <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />           <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>            <!-- Leftbox Mview切替 -->
            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />             <!-- Leftbox 開閉 -->

            <input id="WF_RightViewChange" runat="server" value="" type="text"/>            <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />             <!-- Rightbox 開閉 -->

            <input id="WF_UPLOAD" runat="server" value="" type="text"/>                     <!-- ドロップ処理結果格納フィールド -->

            <input id="WF_REP_POSITION"  runat="server" value=""  type="text" />            <!-- Repeater 行位置 -->
            <input id="WF_REP_SW"  runat="server" value=""  type="text" />                  <!-- Repeater ラジオボタン -->
            <input id="WF_PrintURL" runat="server" value=""  type="text" />                 <!-- Textbox Print URL -->

            <input id="WF_ButtonClick" runat="server" value=""  type="text" />              <!-- ボタン押下 -->
            <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />            <!-- 権限 -->
        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>

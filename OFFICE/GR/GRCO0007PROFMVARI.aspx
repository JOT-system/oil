<%@ Page Title="CO0007" Language="vb" AutoEventWireup="false" CodeBehind="GRCO0007PROFMVARI.aspx.vb" Inherits="OFFICE.GRCO0007PROFMVARI" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRCO0007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="CO0007H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/CO0007.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/CO0007.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=if(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="CO0007" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerboxOnly" id="headerbox">
        <div class="Operation">
            <!-- 画面ID -->
            <a>
                <asp:Label ID="WF_SELMAPID_L" runat="server" Text="画面ID" Height="1.5em" Font-Bold="true" Font-Underline="true"></asp:Label>
            </a>
            <a ondblclick="Field_DBclick('WF_SELMAPID', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>)">
                <asp:TextBox ID="WF_SELMAPID" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_SELMAPID_TEXT" runat="server" Width="30em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ボタン -->
            <a style="position:fixed; top:2.8em; left:49em;">
                <input type="button" id="WF_ButtonExtract" value="絞り込み" style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
            </a>
            <a style="position:fixed; top:2.8em; left:53.5em;">
                <input type="button" id="WF_ButtonUPDATE" value="DB更新" style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
            <a style="position:fixed; top:2.8em; left:58em;">
                <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ" style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
            </a>
            <a style="position:fixed; top:2.8em; left:62.5em;">
                <input type="button" id="WF_ButtonPrint" value="一覧印刷" style="Width:5em" onclick="ButtonClick('WF_ButtonPrint');" />
            </a>
            <a style="position:fixed; top:2.8em; left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了" style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
            <a style="position:fixed; top:3.2em; left:75em;">
                <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>
            <a style="position:fixed; top:3.2em; left:77em;">
                <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>
        </div>
        <div id="divListArea">
            <asp:panel id="pnlListArea" runat="server"></asp:panel>
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
                    <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_Sel_LINECNT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_2">
                <!-- 会社コード -->
                <a>
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE" runat="server" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- プロフID -->
                <a>
                    <asp:Label ID="WF_PROFID_L" runat="server" Text="プロフID" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_PROFID" runat="server" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_3">
                <!-- 画面ID -->
                <a>
                    <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ID" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_MAPID" runat="server" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_MAPID_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
                
                <!-- 変数・名称 -->
                <a>
                    <asp:Label ID="WF_VARIANT_L" runat="server" Text="変数･名称" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_VARIANT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:TextBox ID="WF_VARIANTNAMES" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>
            </p>
            <p id="KEY_LINE_4">
                <!-- 有効年月日 -->
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
                
                <!-- 削除フラグ -->
                <a ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
        </div>

        <table style="position:fixed; top:9.5em; left:3em;">
            <tr>
                <!-- 項番 -->
                <td style="height:1.3em; width:3em;">
                    <asp:Label ID="WF_Rep_SEQ_L" runat="server" Text="項番" Width="3em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </td>
                <!-- 項目(名称) -->
                <td style="height:1.3em; width:18.5em;">
                    <asp:Label ID="WF_Rep_TITLENAMES_L" runat="server" Text="項目(名称)" Width="18em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </td>
                <!-- 項目(記号名) -->
                <td style="height:1.3em; width:18.5em;">
                    <asp:Label ID="WF_Rep_FIELD_L" runat="server" Text="項目(記号名)" Width="18em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </td>
                <!-- 値タイプ -->
                <td style="height:1.3em; width:10em;">
                    <asp:Label ID="WF_Rep_VALUETYPE_L" runat="server" Text="値タイプ" Width="10em" CssClass="WF_TEXT_CENTER" Font-Underline="true"></asp:Label>
                </td>
                <!-- 値 -->
                <td style="height:1.3em; width:10em;">
                    <asp:Label ID="WF_Rep_VALUE_L" runat="server" Text="値" Width="10em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </td>
                <!-- 値加算(年) -->
                <td style="height:1.3em; width:5.2em;">
                    <asp:Label ID="WF_Rep_VALUEADDYY_L" runat="server" Text="値加算(年)" Width="5em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </td>
                <!-- 値加算(月) -->
                <td style="height:1.3em; width:5.2em;">
                    <asp:Label ID="WF_Rep_VALUEADDMM_L" runat="server" Text="値加算(月)" Width="5em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </td>
                <!-- 値加算(日) -->
                <td style="height:1.3em; width:5.2em;">
                    <asp:Label ID="WF_Rep_VALUEADDDD_L" runat="server" Text="値加算(日)" Width="5em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </td>
            </tr>
        </table>

        <!-- DETAIL画面 -->
        <span class="WF_Repeater">
            <asp:Repeater ID="WF_Repeater" runat="server">
                <HeaderTemplate></HeaderTemplate>
                <ItemTemplate>
                    <table style="border:solid; border-width:1px;">
                        <tr>
                            <!-- 項番 -->
                            <td style="height:1.3em; width:3em;">
                                <asp:label ID="WF_Rep_SEQ" runat="server" Width="3em" CssClass="WF_TEXT_RIGHT"></asp:label>
                            </td>
                            <!-- 項目(名称) -->
                            <td style="height:1.3em; width:18.5em;">
                                <asp:label ID="WF_Rep_TITLENAMES" runat="server" Width="18em" CssClass="WF_TEXT_LABEL"></asp:label>
                            </td>
                            <!-- 項目(記号名) -->
                            <td style="height:1.3em; width:18.5em;">
                                <asp:label ID="WF_Rep_FIELD" runat="server" Width="18em" CssClass="WF_TEXT_LABEL"></asp:label>
                            </td>
                            <!-- 値タイプ -->
                            <td style="height:1.3em; width:10em;">
                                <asp:TextBox ID="WF_Rep_VALUETYPE" runat="server" Width="10em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                            </td>
                            <!-- 値 -->
                            <td style="height:1.3em; width:10em;">
                                <asp:TextBox ID="WF_Rep_VALUE" runat="server" Width="10em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                            </td>
                            <!-- 値加算(年) -->
                            <td style="height:1.3em; width:5em;">
                                <asp:TextBox ID="WF_Rep_VALUEADDYY" runat="server" Width="5em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                            </td>
                            <!-- 値加算(月) -->
                            <td style="height:1.3em; width:5em;">
                                <asp:TextBox ID="WF_Rep_VALUEADDMM" runat="server" Width="5em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                            </td>
                            <!-- 値加算(日) -->
                            <td style="height:1.3em; width:5em;">
                                <asp:TextBox ID="WF_Rep_VALUEADDDD" runat="server" Width="5em" CssClass="WF_TEXTBOX_CENTER"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
                <FooterTemplate></FooterTemplate>
            </asp:Repeater>
        </span>
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
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />             <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        
        <input id="WF_DISP" runat="server" value="" type="text" />                  <!-- 画面表示切替 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />       <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->

        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->

        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />         <!-- 権限 -->
    </div>
</asp:Content>

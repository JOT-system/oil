<%@ Page Title="M00006" Language="vb" AutoEventWireup="false" CodeBehind="GRM00006STRUCT.aspx.vb" Inherits="OFFICE.GRM00006STRUCT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRM00006WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="GRM00006H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/M00006.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/M00006.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=if(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="GRM00006" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <div class="Operation">
            <!-- オブジェクト -->
            <a>
                <asp:Label ID="WF_SELOBJECT_L" runat="server" Text="オブジェクト" Height="1.5em" Font-Bold="true" Font-Underline="true"></asp:Label>
            </a>
            <a ondblclick="Field_DBclick('WF_SELOBJECT', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                <asp:TextBox ID="WF_SELOBJECT" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_SELOBJECT_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
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
    <div class="detailbox" id="detailbox">
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
                    <asp:Label ID="WF_Sel_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_2">
                <!-- ユーザーID -->
                <a ondblclick="Field_DBclick('WF_USERID', <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>)">
                    <asp:Label ID="WF_USERID_L" runat="server" Text="ユーザーID" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_USERID" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_USERID_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- 会社コード -->
                <a ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>)">
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_3">
                <!-- オブジェクト -->
                <a ondblclick="Field_DBclick('WF_OBJECT', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                    <asp:Label ID="WF_OBJECT_L" runat="server" Text="オブジェクト" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_OBJECT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_OBJECT_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- 構造コード -->
                <a>
                    <asp:Label ID="WF_STRUCT_L" runat="server" Text="構造コード" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_STRUCT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
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
        
        <table style="position:fixed; top:28em; left:2em;">
            <tr>
                <!-- 項番 -->
                <td style="height:1.3em; width:4.5em;">
                    <asp:Label ID="WF_LINECNT_L" runat="server" Text="項番" Width="4em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </td>
                <!-- SEQ -->
                <td style="height:1.3em; width:5em;">
                    <asp:Label ID="WF_SEQ_L" runat="server" Text="SEQ" Width="7em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </td>
                <!-- コード -->
                <td style="height:1.3em; width:30em;">
                    <asp:Label ID="WF_CODE_L" runat="server" Text="コード" Width="12em" CssClass="WF_TEXT_CENTER" Font-Underline="true"></asp:Label>
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
                            <td style="height:1.3em; width:4.5em;">
                                <asp:label ID="WF_Rep_LINECNT" runat="server" Height="1.1em" Width="4.5em" CssClass="WF_TEXT_LEFT"></asp:label>
                            </td>
                            <!-- SEQ -->
                            <td style="height:1.3em; width:5em;">
                                <asp:TextBox ID="WF_Rep_SEQ" runat="server" Height="1.1em" Width="5em"></asp:TextBox>
                            </td>
                            <!-- コード -->
                            <td style="height:1.3em; width:30em;">
                                <asp:TextBox ID="WF_Rep_CODE" runat="server" Width="14.5em"></asp:TextBox>
                                <asp:label ID="WF_Rep_CODE_TEXT" runat="server" Width="14.5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:label>
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

        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />       <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->

        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->

        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />         <!-- 権限 -->
    </div>
</asp:Content>

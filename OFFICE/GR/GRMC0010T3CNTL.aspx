<%@ Page Title="MC0010" Language="vb" AutoEventWireup="false" CodeBehind="GRMC0010T3CNTL.aspx.vb" Inherits="OFFICE.GRMC0010T3CNTL" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRMC0010WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="GRMC0010H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/MC0010.css")%>"/>
    <script type="text/javascript">
            var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
            var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
        <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/MC0010.js")%>"></script>
</asp:Content>

<asp:Content ID="GRMC0010" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　headerbox -->
        <div  class="headerbox" id="headerbox">
            <div class="Operation">
                <!-- ■　取引先　■ -->
                <a>
                    <asp:Label ID="WF_LabelSORG" runat="server" Text="取引先" Height="1.5em" Font-Bold="true" Font-Underline="True"></asp:Label>
                </a>
                <a ondblclick="Field_DBclick('WF_SELTORI', <%=LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)">
                    <asp:TextBox ID="WF_SELTORI" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_SELTORI_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
                </a>
                
                <!-- ■　受注部署　■ -->
                <a>
                    <asp:Label ID="WF_LabelHORG" runat="server" Text="受注部署" Height="1.5em" Font-Bold="true" Font-Underline="True"></asp:Label>
                </a>
                <a ondblclick="Field_DBclick('WF_SELOORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>)">
                    <asp:TextBox ID="WF_SELOORG" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_SELOORG_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
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
                    <input type="button" id="WF_UPDATE" value="表更新"  style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
                </a>
                <a >
                    <input type="button" id="WF_CLEAR" value="クリア"  style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
                </a>
            </div> 
            <div id="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- ■　選択No　■ -->
                    <a>
                        <asp:Label ID="Label2" runat="server" Text="選択No" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_2">
                    <!-- ■　会社　■ -->
                    <a name="KEY_2" >
                        <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                        <asp:Label ID="WF_CAMPCODE" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■　受注部署　■ -->
                    <a name="KEY_2" ondblclick="Field_DBclick('WF_OORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>)">
                        <asp:Label ID="WF_OORG_L" runat="server" Text="受注部署" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_OORG" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OORG_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■　削除フラグ　■ -->
                    <a name="KEY_2" ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>    
                </p>
                <p id="KEY_LINE_3">
                    <!-- ■　取引先　■ -->
                    <a name="KEY_3" ondblclick="Field_DBclick('WF_TORICODE', <%=LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)">
                        <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_TORICODE" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_TORICODE_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■　油種　■ -->
                    <a name="KEY_3" ondblclick="Field_DBclick('WF_OILTYPE',  <%=LIST_BOX_CLASSIFICATION.LC_OILTYPE%>)">
                        <asp:Label ID="WF_OILTYPE_L" runat="server" Text="油種" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_OILTYPE" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OILTYPE_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■　有効年月日　■ -->
                   <a name="KEY_3" >
                        <asp:Label ID="WF_STYMD_L" runat="server" Text="有効年月日" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <b ondblclick="Field_DBclick('WF_STYMD',  <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_STYMD" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </b>
                        <asp:Label ID="WF_ENDYMD_L" runat="server" Text=" ～ " CssClass="WF_TEXT_LEFT"></asp:Label>
                        <b ondblclick="Field_DBclick( 'WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </b>
                    </a>
                </p>
            </div>
           <!-- DETAIL画面 -->
        <asp:MultiView ID="WF_DetailMView" runat="server">
            <asp:View ID="WF_DView1" runat="server"  >

                <span class="WF_DViewRep1_Area" id="WF_DViewRep1_Area">
                    <asp:Repeater ID="WF_DViewRep1" runat="server">
                        <HeaderTemplate>
                            <table>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                            <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                            <td>
                                <asp:TextBox ID="WF_Rep1_MEISAINO" runat="server"></asp:TextBox>  
                                <asp:TextBox ID="WF_Rep1_LINEPOSITION" runat="server"></asp:TextBox>  
                            </td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            </tr>
<%--                        <asp:Label ID="WF_Rep1_LINE" runat="server" Height="1px" Width="100%" style="display:none; border-bottom:solid; border-width:2px; border-color:blue;"></asp:Label>--%>
                        </ItemTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </span>
            </asp:View>
        </asp:MultiView>

        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%--　イベント用 --%>
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

        <!-- Work レイアウト -->
        <LSINC:work id="work" runat="server" />

</asp:Content>
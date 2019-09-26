<%@ Page Title="T00005" Language="vb" AutoEventWireup="false" CodeBehind="GRT00005NIPPO.aspx.vb" Inherits="OFFICE.GRT00005NIPPO" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00005WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00005H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00005.css")%>"/>
        <script type="text/javascript">
            var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
            var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00005.js")%>"></script>
</asp:Content> 
<asp:Content ID="T00005" ContentPlaceHolderID="contents1" runat="server">


        <!-- 全体レイアウト　headerbox -->
        <div  class="headerbox" id="headerbox">
            <div class="Operation">
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:53.5em;">
                    <input type="button" id="WF_ButtonUPDATE" value="更新"  style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
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
                <a></a>
                <a></a>
            </div>

            <!-- ■■■　１行目　■■■ -->
            <div id="headerkeybox">
                <p id ="LINE_1">
                    <!-- ■　選択No　■ -->
                    <a style="position:fixed;top:4.4em;left:0em;">
                        <asp:Label ID="Label2" runat="server" Text="選択No" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false"></asp:Label>
                        <asp:TextBox ID="WF_Head_LINECNT" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_Head_LINECNT_TEXT" runat="server" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    </a>
                    <!-- ■　始業日　■ -->
                    <a style="position:fixed;top:4.4em;left:22.3em;">
                        <asp:Label ID="WF_STDATE_L" runat="server" Text="始業日" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_STDATE" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
                        </b>
                    </a>
                    <!-- ■　始業時間　■ -->
                    <a style="position:fixed;top:4.4em;left:32.9em;">
                        <asp:Label ID="WF_STTIME_L" runat="server" Text="始業時間" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_STTIME" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
                        </b>
                    </a>
                    <!-- ■　走行距離　■ -->
                    <a style="position:fixed;top:4.4em;left:43.5em;">
                        <asp:Label ID="WF_SOUDISTANCE_L" runat="server" Text="走行距離" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SOUDISTANCE" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
                        </b>
                    </a>
                </p>
                <!-- ■■■　２行目　■■■ -->
                <p id ="LINE_2">
                    <!-- ■　出庫日　■ -->
                    <a style="position:fixed;top:5.6em;left:0em;" ondblclick="Field_DBclick( 'WF_YMD' ,  <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR %>)">
                        <asp:Label ID="WF_YMD_L" runat="server" Text="出庫日" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="true"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_YMD" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </b>
                        <asp:Label ID="WF_YMD_TEXT" runat="server" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    </a>
                    <!-- ■　終業日　■ -->
                    <a style="position:fixed;top:5.6em;left:22.3em;">
                        <asp:Label ID="WF_ENDDATE_L" runat="server" Text="終業日" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_ENDDATE" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
                        </b>
                    </a>
                    <!-- ■　終業時間　■ -->
                    <a style="position:fixed;top:5.6em;left:32.9em;">
                        <asp:Label ID="WF_ENDTIME_L" runat="server" Text="終業時間" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_ENDTIME" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
                        </b>
                    </a>
                    <!-- ■　通行料　■ -->
                    <a style="position:fixed;top:5.6em;left:43.5em;">
                        <asp:Label ID="WF_TOTALTOLL_L" runat="server" Text="通行料" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_TOTALTOLL" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
                        </b>
                    </a>
                </p> 
                    <!-- ■■■　３行目　■■■ -->
                <p id ="LINE_3">
                    <!-- ■　乗務員　■ -->
                    <a style="position:fixed;top:6.8em;left:0em;" ondblclick="Field_DBclick( 'WF_STAFFCODE' ,  <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>)">
                        <asp:Label ID="WF_STAFFCODE_L" runat="server" Text="乗務員" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="true"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </b>
                        <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <!-- ■　乗務区分　■ -->
                    <a hidden="hidden" ondblclick="Field_DBclick( 'WF_CREWKBN' ,  902)">
                        <asp:Label ID="WF_CREWKBN_L" runat="server" Text="乗務区分" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="true"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_CREWKBN" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS"  ></asp:TextBox>
                        </b>
                        <asp:Label ID="WF_CREWKBN_TEXT" runat="server" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                    <!-- ■　休憩時間　■ -->
                    <a style="position:fixed;top:6.8em;left:22.3em;">
                        <asp:Label ID="WF_BREAKTIME_L" runat="server" Text="休憩時間" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_BREAKTIME" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
                        </b>
                    </a>
                    <!-- ■　稼働時間　■ -->
                    <a style="position:fixed;top:6.8em;left:32.9em;">
                        <asp:Label ID="WF_WORKTIME_L" runat="server" Text="稼働時間" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_WORKTIME" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false"></asp:TextBox>
                        </b>
                    </a>
                    <!-- ■　給油　■ -->
                    <a style="position:fixed;top:6.8em;left:43.5em;">
                        <asp:Label ID="WF_KYUYU_L" runat="server" Text="給油" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="false"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_KYUYU" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS" enabled="false" ></asp:TextBox>
                        </b>
                    </a>
                    <!-- ■　削除フラグ　■ -->
                    <a style="position:fixed;top:6.8em;left:54.1em;" ondblclick="Field_DBclick('WF_DELFLG_H' ,  <%=LIST_BOX_CLASSIFICATION.LC_DELFLG  %>)">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除フラグ" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="false" Font-Underline="true"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_DELFLG_H" runat="server" Height="1.1em" Width="6em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </b>
                        <asp:Label ID="WF_DELFLG_H_TEXT" runat="server" Height="1.3em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <a style="position:fixed;top:6.8em;left:75em;">
                        <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                    </a>
                    <a style="position:fixed;top:6.8em;left:77em;">
                        <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                    </a>
                    </p>
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
                    <!-- ■　明細行番号　■ -->
                    <a>
                        <asp:Label ID="Label3" runat="server" Text="項番" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                        <asp:TextBox ID="WF_SEQ" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </a>
                    <!-- ■　削除フラグ　■ -->
                    <a ondblclick="Field_DBclick( 'WF_DELFLG' ,  <%=LIST_BOX_CLASSIFICATION.LC_DELFLG  %>)">
                        <asp:Label ID="Label12" runat="server" Text="削除フラグ" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="4em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■　選択No　非表示 ■ -->
                    <a hidden="hidden">
                        <asp:Label ID="Label1" runat="server" Text="項番" Height="1.3em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                        <asp:Label ID="WF_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </a>

                    <!-- ■　油種　非表示■ -->
                    <a ondblclick="Field_DBclick('WF_OILTYPE' ,  <%=LIST_BOX_CLASSIFICATION.LC_OILTYPE  %>)")" hidden="hidden">
                        <asp:Label ID="Label13" runat="server" Text="油種" Height="1.3em" Width="3em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_OILTYPE" runat="server" Height="1.1em" Width="4em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OILTYPE_TEXT" runat="server" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■　品名１ 非表示　■ -->
                    <a ondblclick="Field_DBclick( 'WF_PRODUCT1' ,  <%=LIST_BOX_CLASSIFICATION.LC_GOODS  %>)")" hidden="hidden">
                        <asp:Label ID="Label14" runat="server" Text="品名１" Height="1.3em" Width="3em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_PRODUCT1" runat="server" Height="1.1em" Width="4em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_PRODUCT1_TEXT" runat="server" Height="1.3em" Width="5em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>
            </div>
           <!-- ■ 明細　■ -->        
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
                        </ItemTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </span>
            </asp:View>
        </asp:MultiView>

        </div>

        <!-- HIDDEN項目 -->
        <div hidden="hidden">
                <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>   <!-- GridViewダブルクリック -->
                <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>  <!-- GridView表示位置フィールド -->

                <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
                <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />      <!-- 権限 -->

                <input id="WF_FIELD"  runat="server" value=""  type="text" />             <!-- Textbox DBクリックフィールド -->
                <input id="WF_FIELD_REP"  runat="server" value=""  type="text" />         <!-- Textbox(Repeater) DBクリックフィールド -->

                <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>      <!-- Leftbox Mview切替 -->
                <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />       <!-- Leftbox 開閉 -->

                <input id="WF_RightViewChange" runat="server" value="" type="text"/>      <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />       <!-- Rightbox 開閉 -->

                <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />     <!-- Textbox DBクリックフィールド -->

                <input id="WF_WORKKBNChange" runat="server" value="" type="text"/>        <!-- 作業区分変更 -->

                <input id="WF_PrintURL" runat="server" value=""  type="text" />           <!-- Textbox Print URL -->

        </div>

        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <!-- Work レイアウト -->
        <LSINC:work id="work" runat="server" />

</asp:Content>
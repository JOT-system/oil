<%@ Page Title="TA0001" Language="vb" AutoEventWireup="false" CodeBehind="GRTA0001HAISHA.aspx.vb" Inherits="OFFICE.GRTA0001HAISHA" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>

<%@ register src="inc/GRTA0001WRKINC.ascx" tagname="work" tagprefix="LSINC" %>
<asp:Content ID="GRTA0001H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/TA0001.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/TA0001.js")%>"></script>
</asp:Content>
<asp:Content ID="GRTA0001" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　headerbox -->
        <div  class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <!-- ■　条件　■ -->
                <a style="position:fixed;top:3.0em;left:15em;width:15.0em;">
                    <asp:Label runat="server" Text="出庫日：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_DATE" runat="server"  Font-Bold="True"></asp:Label>
                </a>
                <a style="position:fixed;top:3.0em;left:30em;width:15.0em;">
                    <asp:Label runat="server" Text="出荷部署：　" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_SEL_ORG" runat="server"  Font-Bold="True"></asp:Label>
                </a>
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:53.5em;">
                    <input type="button" id="WF_ButtonPDF" value="全印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPDF');" />
                </a>
                <a style="position:fixed;top:2.8em;left:58.0em;">
                    <input type="button" id="WF_ButtonXLS" value="Excel取得"  style="Width:5em" onclick="ButtonClick('WF_ButtonXLS');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonZIP" value="全ZIP取得"  style="Width:5em" onclick="ButtonClick('WF_ButtonZIP');" />
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
            <div id="leftMenubox"  class="leftMenubox">
                <div style="overflow-y:auto;width:12.0em;text-align:center;vertical-align:middle;color:white;background-color:rgb(22,54,92);font-weight:bold;border: solid black;border-width:1.5px;">
                    <!-- ■　照会選択　■ -->
                    <a style="width:12.0em;font-size:medium;overflow:hidden;color:white;background-color:rgb(22,54,92);text-align:center;">照会選択</a>
                </div>

                <div style="overflow-y:auto;width:12.0em;min-height:30em;max-height:32em;color:black;background-color: white;border: solid;border-width:1.5px;">
                    <!-- ■　セレクター　■ -->
                    <asp:Repeater ID="WF_SELECTOR" runat="server">
                        <HeaderTemplate> 
                            <table style="border-width:1px;margin:0.1em 0.1em 0.1em 0.1em;">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr> 

                            <!-- 非表示項目(左Box処理用・Repeater内行位置)　-->
                            <td style="height:1.5em;width:11.8em;" hidden="hidden">
                                <asp:Label ID="WF_SELECTOR_VALUE" runat="server"></asp:Label>
                            </td>

                            <td>
                                <asp:Label ID="WF_SELECTOR_TEXT" runat="server" Text="" Height="1.3em" Width="11.8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                            </td>
                            </tr> 
                        </ItemTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
            <!-- 一覧レイアウト -->
            <div id="divListArea">
                <asp:panel id="pnlListArea" runat="server" ></asp:panel>
            </div>
        </div>

        <!-- 全体レイアウト　detailbox -->
        <div  class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">

            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_BACK" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_BACK');" />
            </a><br />
            </div> 
            <div class="detailkeybox" id ="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- ■　選択No　■ -->
                    <a style="position:fixed;top:3.0em;left:3em; width:32em;">
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                        <asp:TextBox ID="WF_Sel_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                </p>
                <p id="KEY_LINE_2">
            
                    <!-- ■　出庫日　■ -->
                    <a style="position:fixed;top:4.4em;left:3em; width:25.0em;" >
                        <asp:Label ID="WF_SHUKODATE_L" runat="server" Text="出庫日" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_SHUKODATE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　帰庫日　■ -->
                    <a style="position:fixed;top:4.4em;left:24.6em; width:25.0em;">
                        <asp:Label ID="WF_KIKODATE_L" runat="server" Text="帰庫日" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_KIKODATE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                </p>
                <p id="KEY_LINE_3">
                    <!-- ■　出荷日　■ -->
                    <a style="position:fixed;top:5.8em;left:3em; width:25.0em;">
                        <asp:Label ID="WF_SHUKADATE_L" runat="server" Text="出荷日" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_SHUKADATE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　届日　■ -->
                    <a style="position:fixed;top:5.8em;left:24.6em; width:25.0em;">
                        <asp:Label ID="WF_TODOKEDATE_L" runat="server" Text="届日" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_TODOKEDATE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　両目　■ -->
                    <a style="position:fixed;top:5.8em;left:46.2em; width:25.0em;">
                        <asp:Label ID="WF_RYOME_L" runat="server" Text="両目" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="WF_RYOME" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                </p>
                <p id="KEY_LINE_4">
                    <!-- ■　油種　■ -->
                    <a style="position:fixed;top:7.2em;left:3em; width:25.0em;">
                        <asp:Label ID="WF_OILTYPE_L" runat="server" Text="油種" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_OILTYPE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　受注部署　■ -->
                    <a style="position:fixed;top:7.2em;left:24.6em; width:25.0em;">
                        <asp:Label ID="WF_ORDERORG_L" runat="server" Text="受注部署" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_ORDERORG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>

                    <!-- ■　出荷部署　■ -->
                    <a style="position:fixed;top:7.2em;left:46.2em; width:25.0em;">
                        <asp:Label ID="WF_SHIPORG_L" runat="server" Text="出荷部署" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_SHIPORG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                </p>
                <p id="KEY_LINE_5">
                    <!-- ■　取引先　■ -->
                    <a style="position:fixed;top:8.6em;left:3em; width:25.0em;">
                        <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先CD" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_TORICODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　販売店　■ -->
                    <a style="position:fixed;top:8.6em;left:24.6em; width:25.0em;">
                        <asp:Label ID="WF_STORICODE_L" runat="server" Text="販売店" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_STORICODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　売上計上基準　■ -->
                    <a style="position:fixed;top:8.6em;left:46.2em; width:25.0em;">
                        <asp:Label ID="WF_URIKBN_L" runat="server" Text="売上計上基準" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_URIKBN" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                </p>
                <p id="KEY_LINE_6">
                    <!-- ■　業務車番　■ -->
                    <a style="position:fixed;top:10.0em;left:3em; width:25.0em;">
                        <asp:Label ID="WF_GSHABAN_L" runat="server" Text="業務車番" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_GSHABAN" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　コンテナシャーシ　■ -->
                    <a style="position:fixed;top:10.0em;left:24.6em; width:25.0em;">
                        <asp:Label ID="WF_CONTCHASSIS_L" runat="server" Text="ｺﾝﾃﾅｼｬｰｼ" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_CONTCHASSIS" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　車腹　■ -->
                    <a style="position:fixed;top:10.0em;left:46.2em; width:25.0em;">
                        <asp:Label ID="WF_SHAFUKU_L" runat="server" Text="車腹" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="False"></asp:Label>
                        <asp:TextBox ID="WF_SHAFUKU" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                </p>
                <p id="KEY_LINE_7">
                    <!-- ■　統一車番（前）　■ -->
                    <a style="position:fixed;top:11.4em;left:3em; width:25.0em;">
                        <asp:Label ID="WF_TSHABANF_L" runat="server" Text="統一車番（前）" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="WF_TSHABANF" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　統一車番（後）　■ -->
                    <a style="position:fixed;top:11.4em;left:24.6em; width:25.0em;">
                        <asp:Label ID="WF_TSHABANB_L" runat="server" Text="統一車番（後）" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="WF_TSHABANB" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                    <!-- ■　統一車番（後）２　■ -->
                    <a style="position:fixed;top:11.4em;left:46.2em; width:25.0em;">
                        <asp:Label ID="WF_TSHABANB2_L" runat="server" Text="統一車番（後）２" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="false"></asp:Label>
                        <asp:TextBox ID="WF_TSHABANB2" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                </p>
                <p id="KEY_LINE_8">
                    <!-- ■　積置区分　■ -->
                    <a style="position:fixed;top:12.8em;left:3em; width:25.0em;">
                        <asp:Label ID="WF_TUMIOKIKBN_L" runat="server" Text="積置区分" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_TUMIOKIKBN" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>

                    <!-- ■　トリップ　■ -->
                    <a style="position:fixed;top:12.8em;left:24.6em; width:25.0em;">
                        <asp:Label ID="WF_TRIPNO_L" runat="server" Text="トリップ" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="False"></asp:Label>
                        <asp:TextBox ID="WF_TRIPNO" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>

                    <!-- ■　ドロップ　■ -->
                    <a style="position:fixed;top:12.8em;left:46.2em; width:25.0em;">
                        <asp:Label ID="WF_DROPNO_L" runat="server" Text="ドロップ" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="False"></asp:Label>
                        <asp:TextBox ID="WF_DROPNO" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                </p>
                <p id="KEY_LINE_9">
                    <!-- ■　乗務員　■ -->
                    <a style="position:fixed;top:14.2em;left:3em; width:25.0em;">
                        <asp:Label ID="WF_STAFFCODE_L" runat="server" Text="乗務員" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>

                    <!-- ■　副乗務員　■ -->
                    <a style="position:fixed;top:14.2em;left:24.6em; width:25.0em;">
                        <asp:Label ID="WF_SUBSTAFFCODE_L" runat="server" Text="副乗務員" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_SUBSTAFFCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>

                    <!-- ■　出勤時間　■ -->
                    <a style="position:fixed;top:14.2em;left:46.2em; width:25.0em;">
                        <asp:Label ID="WF_STTIME_L" runat="server" Text="出勤時間" Height="1.3em" Width="8.1em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="False"></asp:Label>
                        <asp:TextBox ID="WF_STTIME" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_BORDER" ReadOnly="true"></asp:TextBox>
                    </a>
                </p>
            </div>
            <!-- DETAIL画面 -->
            <asp:MultiView ID="WF_DetailMView" runat="server">
                <asp:View ID="WF_DView1" runat="server"  >
                <span class="WF_DViewRep1_Area" id="WF_DViewRep1_Area">
                        <asp:Repeater ID="WF_DViewRep1" runat="server">
                            <HeaderTemplate>

                            </HeaderTemplate>
                            <ItemTemplate>
                            <table>
                                <tr>
                                    <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                    <td>
                                        <asp:TextBox ID="WF_Rep1_MEISAINO" runat="server"></asp:TextBox>  
                                        <asp:TextBox ID="WF_Rep1_LINEPOSITION" runat="server"></asp:TextBox>  
                                    </td>
                                    <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　1 --%>
                                    <td><asp:Label   ID="WF_Rep1_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:TextBox ID="WF_Rep1_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                    <td><asp:Label   ID="WF_Rep1_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　2 --%>
                                    <td><asp:Label   ID="WF_Rep1_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:TextBox ID="WF_Rep1_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                    <td><asp:Label   ID="WF_Rep1_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　3 --%>
                                    <td><asp:Label   ID="WF_Rep1_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:TextBox ID="WF_Rep1_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                    <td><asp:Label   ID="WF_Rep1_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　4 --%>
                                    <td><asp:Label   ID="WF_Rep1_FIELDNM_4" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label1_4"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_FIELD_4"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:TextBox ID="WF_Rep1_VALUE_4"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                    <td><asp:Label   ID="WF_Rep1_Label2_4"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_4" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label3_4"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　5 --%>
                                    <td><asp:Label   ID="WF_Rep1_FIELDNM_5" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label1_5"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_FIELD_5"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:TextBox ID="WF_Rep1_VALUE_5"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                    <td><asp:Label   ID="WF_Rep1_Label2_5"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_5" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label3_5"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　6 --%>
                                    <td><asp:Label   ID="WF_Rep1_FIELDNM_6" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label1_6"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_FIELD_6"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:TextBox ID="WF_Rep1_VALUE_6"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                    <td><asp:Label   ID="WF_Rep1_Label2_6"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_6" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                    <td><asp:Label   ID="WF_Rep1_Label3_6"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                                </table>
                                <asp:Label ID="WF_Rep1_LINE" runat="server" Height="1px" Width="100%" style="display:none; border-bottom:solid; border-width:2px; border-color:blue;"></asp:Label>

                            </ItemTemplate>
                            <FooterTemplate>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>
                </asp:View>
            </asp:MultiView>
        </div> 

        <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>         <!-- GridViewダブルクリック -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>        <!-- GridView表示位置フィールド -->
            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />             <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>            <!-- Textbox DBクリックフィールド -->
            <input id="WF_RightViewChange" runat="server" value="" type="text"/>            <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />             <!-- Rightbox 開閉 -->

            <input id="WF_PrintURL" runat="server" value=""  type="text" />                 <!-- Textbox Print URL -->
            <input id="WF_SELECTOR_SW" runat="server" value=""  type="text" />              <!-- Repeaterの選択値 -->
            <input id="WF_SELECTOR_Posi" runat="server" value=""  type="text" />            <!-- Repeaterの選択値 -->
            
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
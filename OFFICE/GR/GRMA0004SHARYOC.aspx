<%@ Page Title="MA0004" Language="vb" AutoEventWireup="false" CodeBehind="GRMA0004SHARYOC.aspx.vb" Inherits="OFFICE.GRMA0004SHARYOC" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRMA0004WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="GRMA0004H" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/MA0004.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
        <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/MA0004.js")%>"></script>
</asp:Content> 
<asp:Content ID="GRMA0004" ContentPlaceHolderID="contents1" runat="server">

        <!-- 全体レイアウト　headerbox -->
        <div  class="headerbox" id="headerbox">
            <div class="Operation">
                <!-- ■　選択　■ -->
                <a>
                    <asp:Label ID="WF_SELSHARYOTYPE_LABEL" runat="server" Text="統一車番" Height="1.5em" Font-Bold="True" Font-Underline="True"></asp:Label>
                </a>
                <a ondblclick="Field_DBclick( 'WF_SELSHARYOTYPE' ,  910)">
                    <asp:TextBox ID="WF_SELSHARYOTYPE" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_SELSHARYOTYPE_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
                </a>

                <a>
                    <asp:Label ID="Label3" runat="server" Text="管理組織" Height="1.5em" Font-Bold="True" Font-Underline="True"></asp:Label>
                </a>
                <a ondblclick="Field_DBclick( 'WF_SELMORG' ,  <%=LIST_BOX_CLASSIFICATION.LC_ORG %>)">
                    <asp:TextBox ID="WF_SELMORG" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_SELMORG_TEXT" runat="server" Width="12em" CssClass="WF_TEXT"></asp:Label>
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
                <a>
                    <input type="button" id="WF_UPDATE" value="表更新"  style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
                </a>
                <a>
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
                    <a>
                        <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                        <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- ■　統一車番　■ -->
                    <a >
                        <asp:Label ID="WF_TSHABAN_L" runat="server" Text="統一車番" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                        <b>
                        <asp:TextBox ID="WF_SHARYOTYPE" runat="server" Height="1.1em" Width="4em" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                        </b>
                        <asp:TextBox ID="WF_TSHABAN" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                        <asp:Label ID="WF_TSHABAN1_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_3">
                    <!-- ■　有効年月日　■ -->
                    <a>
                        <asp:Label ID="WF_YMD_L" runat="server" Text="有効年月日" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <b  ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_STYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"  onchange="STYMDChange();"></asp:TextBox>
                        </b>
                        <asp:Label ID="Label1" runat="server" Text=" ～ " CssClass="WF_TEXT_LEFT"></asp:Label>
                        <b  ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </b>
                    </a>

                    <!-- ■　削除フラグ　■ -->
                    <a  ondblclick="Field_DBclick('WF_DELFLG' , <%=LIST_BOX_CLASSIFICATION.LC_DELFLG  %>)">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                        <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" ></asp:TextBox>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>
                <p id="KEY_LINE_4">
                    <!-- ■　Dタブ　■ -->
                    <a onclick="DtabChange('0')">
                        <asp:Label ID="WF_Dtab01" runat="server" Text="管理" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('1')">
                        <asp:Label ID="WF_Dtab02" runat="server" Text="連結車番" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('2')">
                        <asp:Label ID="WF_Dtab03" runat="server" Text="車両緒元" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('3')">
                        <asp:Label ID="WF_Dtab04" runat="server" Text="石油タンク" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('4')">
                        <asp:Label ID="WF_Dtab05" runat="server" Text="高圧タンク" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('5')">
                        <asp:Label ID="WF_Dtab06" runat="server" Text="化成品タンク" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('6')">
                        <asp:Label ID="WF_Dtab07" runat="server" Text="コンテナ" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('7')">
                        <asp:Label ID="WF_Dtab08" runat="server" Text="車両その他" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('8')">
                        <asp:Label ID="WF_Dtab09" runat="server" Text="経理" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('9')">
                        <asp:Label ID="WF_Dtab10" runat="server" Text="申請" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>

                    <a  onclick="DtabChange('10')">
                        <asp:Label ID="WF_Dtab11" runat="server" Text="申請書類（PDF）" Height="1.3em" Width="7.5em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                    </a>
                </p> 
            </div> 
            <asp:MultiView ID="WF_DetailMView" runat="server">

                <!-- ■ Tab No1　管理　■ -->
                <asp:View ID="WF_DView1" runat="server" >
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

                 <!-- ■ Tab No2　連結車番　■ -->
                <asp:View ID="WF_DView2" runat="server">
                     <span class="WF_DViewRep2_Area" id="WF_DViewRep2_Area">
                        <asp:Repeater ID="WF_DViewRep2" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                <td>
                                    <asp:TextBox ID="WF_Rep2_MEISAINO" runat="server"></asp:TextBox>  
                                    <asp:TextBox ID="WF_Rep2_LINEPOSITION" runat="server"></asp:TextBox>  
                                </td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                                <td><asp:Label   ID="WF_Rep2_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep2_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep2_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                                <td><asp:Label   ID="WF_Rep2_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep2_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep2_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                                <td><asp:Label   ID="WF_Rep2_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep2_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep2_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep2_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>
                </asp:View>

                <!-- ■ Tab No3　車両緒元　■ -->
                <asp:View ID="WF_DView3" runat="server">
                     <span class="WF_DViewRep3_Area" id="WF_DViewRep3_Area">
                        <asp:Repeater ID="WF_DViewRep3" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                <td>
                                    <asp:TextBox ID="WF_Rep3_MEISAINO" runat="server"></asp:TextBox>  
                                    <asp:TextBox ID="WF_Rep3_LINEPOSITION" runat="server"></asp:TextBox>  
                                </td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                                <td><asp:Label   ID="WF_Rep3_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep3_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep3_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                                <td><asp:Label   ID="WF_Rep3_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep3_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep3_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                                <td><asp:Label   ID="WF_Rep3_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep3_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep3_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep3_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>
                </asp:View>

                <!-- ■ Tab No4　石油タンク　■ -->
                <asp:View ID="WF_DView4" runat="server">
                     <span class="WF_DViewRep4_Area" id="WF_DViewRep4_Area">
                        <asp:Repeater ID="WF_DViewRep4" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                <td>
                                    <asp:TextBox ID="WF_Rep4_MEISAINO" runat="server"></asp:TextBox>  
                                    <asp:TextBox ID="WF_Rep4_LINEPOSITION" runat="server"></asp:TextBox>  
                                </td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                                <td><asp:Label   ID="WF_Rep4_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep4_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep4_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                                <td><asp:Label   ID="WF_Rep4_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep4_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep4_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                                <td><asp:Label   ID="WF_Rep4_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep4_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep4_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep4_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>

                </asp:View>

                <!-- ■ Tab No5　高圧タンク　■ -->
                <asp:View ID="WF_DView5" runat="server">
                     <span class="WF_DViewRep5_Area" id="WF_DViewRep5_Area">
                        <asp:Repeater ID="WF_DViewRep5" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                <td>
                                    <asp:TextBox ID="WF_Rep5_MEISAINO" runat="server"></asp:TextBox>  
                                    <asp:TextBox ID="WF_Rep5_LINEPOSITION" runat="server"></asp:TextBox>  
                                </td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                                <td><asp:Label   ID="WF_Rep5_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep5_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep5_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                                <td><asp:Label   ID="WF_Rep5_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep5_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep5_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                                <td><asp:Label   ID="WF_Rep5_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep5_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep5_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep5_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>

                </asp:View>

                <!-- ■ Tab No6　化成品タンク　■ -->
                <asp:View ID="WF_DView6" runat="server">
                     <span class="WF_DViewRep6_Area" id="WF_DViewRep6_Area">
                        <asp:Repeater ID="WF_DViewRep6" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                <td>
                                    <asp:TextBox ID="WF_Rep6_MEISAINO" runat="server"></asp:TextBox>  
                                    <asp:TextBox ID="WF_Rep6_LINEPOSITION" runat="server"></asp:TextBox>  
                                </td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                                <td><asp:Label   ID="WF_Rep6_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep6_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep6_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                                <td><asp:Label   ID="WF_Rep6_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep6_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep6_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                                <td><asp:Label   ID="WF_Rep6_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep6_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep6_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep6_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>

                </asp:View>

                <!-- ■ Tab No7　コンテナ　■ -->
                <asp:View ID="WF_DView7" runat="server">
                     <span class="WF_DViewRep7_Area" id="WF_DViewRep7_Area">
                        <asp:Repeater ID="WF_DViewRep7" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                <td>
                                    <asp:TextBox ID="WF_Rep7_MEISAINO" runat="server"></asp:TextBox>  
                                    <asp:TextBox ID="WF_Rep7_LINEPOSITION" runat="server"></asp:TextBox>  
                                </td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                                <td><asp:Label   ID="WF_Rep7_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep7_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep7_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                                <td><asp:Label   ID="WF_Rep7_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep7_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep7_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                                <td><asp:Label   ID="WF_Rep7_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep7_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep7_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep7_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>
                </asp:View>

                <!-- ■ Tab No8　車両その他　■ -->
                <asp:View ID="WF_DView8" runat="server">
                     <span class="WF_DViewRep8_Area" id="WF_DViewRep8_Area">
                        <asp:Repeater ID="WF_DViewRep8" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                <td>
                                    <asp:TextBox ID="WF_Rep8_MEISAINO" runat="server"></asp:TextBox>  
                                    <asp:TextBox ID="WF_Rep8_LINEPOSITION" runat="server"></asp:TextBox>  
                                </td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                                <td><asp:Label   ID="WF_Rep8_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep8_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep8_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                                <td><asp:Label   ID="WF_Rep8_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep8_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep8_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                                <td><asp:Label   ID="WF_Rep8_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep8_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep8_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep8_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>

                </asp:View>

                <!-- ■ Tab No9　経理　■ -->
                <asp:View ID="WF_DView9" runat="server">
                     <span class="WF_DViewRep9_Area" id="WF_DViewRep9_Area">
                        <asp:Repeater ID="WF_DViewRep9" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                <td>
                                    <asp:TextBox ID="WF_Rep9_MEISAINO" runat="server"></asp:TextBox>  
                                    <asp:TextBox ID="WF_Rep9_LINEPOSITION" runat="server"></asp:TextBox>  
                                </td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                                <td><asp:Label   ID="WF_Rep9_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep9_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep9_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                                <td><asp:Label   ID="WF_Rep9_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep9_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep9_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                                <td><asp:Label   ID="WF_Rep9_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep9_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep9_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep9_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>

                </asp:View>

                <!-- ■ Tab No10　申請　■ -->
                <asp:View ID="WF_DView10" runat="server">
                     <span class="WF_DViewRep10_Area" id="WF_DViewRep10_Area">
                        <asp:Repeater ID="WF_DViewRep10" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                <td>
                                    <asp:TextBox ID="WF_Rep10_MEISAINO" runat="server"></asp:TextBox>  
                                    <asp:TextBox ID="WF_Rep10_LINEPOSITION" runat="server"></asp:TextBox>  
                                </td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                                <td><asp:Label   ID="WF_Rep10_FIELDNM_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_Label1_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_FIELD_1"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep10_VALUE_1"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep10_Label2_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_VALUE_TEXT_1" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_Label3_1"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　中央 --%>
                                <td><asp:Label   ID="WF_Rep10_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep10_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep10_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　右 --%>
                                <td><asp:Label   ID="WF_Rep10_FIELDNM_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_Label1_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_FIELD_3"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:TextBox ID="WF_Rep10_VALUE_3"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                                <td><asp:Label   ID="WF_Rep10_Label2_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_VALUE_TEXT_3" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                <td><asp:Label   ID="WF_Rep10_Label3_3"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </span>

                </asp:View>
                
                <!-- ■ Tab No11　申請書類(PDF)　■ -->
                <asp:View ID="WF_DView11" runat="server">

                    <span class="WF_DViewRep11_Area">
                        
                        <!-- PDF表示選択 -->
                        <span style="position:relative;top:0.2em;left:1.3em;">
                            <asp:Label ID="Label12" runat="server" Text="PDF表示選択" Width="8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>

                        <span style="position:relative;top:0.5em;left:0.5em;" onchange="PDFselectChange()">
                            <asp:ListBox ID="WF_Rep11_PDFselect" runat="server" Height="1.3em" Width="15em" rows="1"  CssClass="WF_ListBoxPDF"></asp:ListBox>
                        </span>
                        <br />

                        <!-- PDF明細ヘッダー -->
                        <span style="position:relative;top:0.5em;left:5.0em;display:inline;">
                            <asp:Label ID="Label13" runat="server" Text="ファイル名" Height="1.3em" Width="8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>

                        <span style="position:relative;top:0.5em;left:34.3em;display:inline;">
                            <asp:Label ID="Label14" runat="server" Text="削 除" Height="1.3em" Width="8em" CssClass="WF_TEXT_CENTER"></asp:Label>
                        </span>
                        <br />

                        <span style="position:absolute;top:3.4em;left:1.3em;height:7.3em;width:50em;overflow-x:hidden;overflow-y:auto;background-color:white;border:1px solid black;">
                        <asp:Repeater ID="WF_DViewRepPDF" runat="server" >
                            <HeaderTemplate>
                            </HeaderTemplate>

                            <ItemTemplate>
                                <table >
                                <tr>

                                <td style="height:1.0em;width:37em;color:blue;display:inline-block;">
                                <!-- ■　ファイル記号名称　■ -->
                                <asp:Label ID="WF_Rep_FILENAME" runat="server" Text="" Width="30em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.0em;width:10em;display:inline-block;">
                                <!-- ■　削除　■ -->
                                <asp:TextBox ID="WF_Rep_DELFLG" runat="server" Height="1.0em" Width="10em" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                                </td>

                                <td style="height:1.0em;width:10em;" hidden="hidden">
                                <!-- ■　FILEPATH　■ -->
                                <asp:Label ID="WF_Rep_FILEPATH" runat="server" Height="1.0em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                </tr>
                                </table>
                            </ItemTemplate>

                            <FooterTemplate>
                            </FooterTemplate>
             
                        </asp:Repeater>
                        </span>

                    </span>

                </asp:View>
                
            </asp:MultiView>

        </div>
        <div hidden="hidden">
                <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>   <!-- GridViewダブルクリック -->
                <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>  <!-- GridView表示位置フィールド -->
                <asp:ListBox ID="WF_ListBoxMANGPROD1ALL" runat="server" ></asp:ListBox>   <!-- ListBox品名１（全て） -->
                <asp:ListBox ID="WF_ListBoxMANGPROD2ALL" runat="server" ></asp:ListBox>   <!-- ListBox品名２（全て） -->
                <asp:ListBox ID="WF_ListBoxSHARYOTYPE2" runat="server"></asp:ListBox>     <!-- 車両タイプ分類ListBox（前、後の分類）　-->

                <input id="WF_DTAB_CHANGE_NO" runat="server" value="" type="text"/>       <!-- DetailBox Mview切替 -->
                <input id="WF_DTAB_PDF_DISP_FILE" runat="server" value="" type="text"/>   <!-- DetailBox PDF表示 -->
                <input id="WF_DTABPDFchange" runat="server" value="" type="text"/>        <!-- DetailBox PDF表示内容切替 -->
                <input id="WF_STYMDChange" runat="server" value="" type="text"/>          <!-- DetailBox 有効年月日変更 -->
           

                <input id="WF_FIELD"  runat="server" value=""  type="text" />             <!-- Textbox DBクリックフィールド -->
                <input id="WF_FIELD_REP"  runat="server" value=""  type="text" />         <!-- Textbox(Repeater) DBクリックフィールド -->

                <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>      <!-- Leftbox Mview切替 -->
                <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />       <!-- Leftbox 開閉 -->

                <input id="WF_RightViewChange" runat="server" value="" type="text"/>      <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />       <!-- Rightbox 開閉 -->

                <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />     <!-- Textbox DBクリックフィールド -->

                <input id="WF_EXCEL_UPLOAD"  runat="server" value=""  type="text" />      <!-- Excel アップロードフィールド -->

                <asp:ListBox ID="WF_ListBoxPDF" runat="server"></asp:ListBox>             <!-- PDF アップロード一覧 -->
            
                <input id="WF_PrintURL" runat="server" value=""  type="text" />           <!-- Textbox Print URL -->
            
                <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
                <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />      <!-- 権限 -->
        </div>

        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <!-- Work レイアウト -->
        <LSINC:work id="work" runat="server" />

</asp:Content>
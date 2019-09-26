<%@ Page Title="MC0006" Language="vb" AutoEventWireup="false" CodeBehind="GRMC0006TODOKESAKI.aspx.vb" Inherits="OFFICE.GRMC0006TODOKESAKI" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="~/GR/inc/GRMC0006WRKINC.ascx" tagname="work" tagprefix="MSINC" %>

<asp:Content ID="GRMC0006H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/MC0006.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/MC0006.js")%>"></script>
</asp:Content> 
<asp:Content ID="GRMC0006" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div  class="headerbox" id="headerbox">
        <div class="Operation">
            <!-- ■　選択　■ -->
            <a>
                <asp:Label ID="WF_TORINAME_LABEL" runat="server" Text="取引先名称" Height="1.5em" Font-Bold="True"></asp:Label>
            </a>
            <a>
                <asp:TextBox ID="WF_TORINAME" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_TODOKENAME_LABEL" runat="server" Text="届先名称" Height="1.5em" Font-Bold="True"></asp:Label>
            </a>
            <a>
                <asp:TextBox ID="WF_TODOKENAME" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_CLASS_LABEL" runat="server" Text="分類" Height="1.5em" Font-Bold="True" Font-Underline="True"></asp:Label>
            </a>
            <a ondblclick="Field_DBclick('WF_CLASS',  <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                <asp:TextBox ID="WF_CLASS" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_CLASS_TEXT" runat="server" Width="10em" CssClass="WF_TEXT"></asp:Label>
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
            <a>
                <input type="button" id="WF_MAP" value="地図表示"  style="Width:5em" onclick="ButtonClick('WF_MAP');" />
            </a>
            <a>
                <input type="button" id="WF_COORDINATE" value="緯度経度"  style="Width:5em" onclick="ButtonClick('WF_COORDINATE');" />
            </a>
        </div>
        <div id="detailkeybox">
            <p id="KEY_LINE_1">
                <!-- ■　選択No　■ -->
                <a>
                    <asp:Label ID="Label2" runat="server" Text="選択No" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_Sel_LINECNT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_2">
                <!-- ■　会社　■ -->
                <a name="KEY_2" >
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE" runat="server" Width="8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　取引先　■ -->
                <a name="KEY_2" >
                    <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_TORICODE',  <%=LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)">
                        <asp:TextBox ID="WF_TORICODE" runat="server" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_TORICODE_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
          
            <p id="KEY_LINE_3">
                <!-- ■　届先　■ -->
                <a name="KEY_3" >
                    <asp:Label ID="WF_TODOKECODE_L" runat="server" Text="届先CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_TODOKECODE',  <%=LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)">
                    <asp:TextBox ID="WF_TODOKECODE" runat="server" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_TODOKECODE_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　有効年月日　■ -->
                <a name="KEY_3" >
                    <asp:Label ID="WF_STYMD_L" runat="server" Text="有効年月日" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_STYMD',  <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_STYMD" runat="server" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_ENDYMD_L" runat="server" Text=" ～ " CssClass="WF_TEXT_LEFT"></asp:Label>
                    <b ondblclick="Field_DBclick( 'WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_ENDYMD" runat="server" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                </a>

                <!-- ■　削除フラグ　■ -->
                <a name="KEY_3" >
                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_DELFLG',  <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                        <asp:TextBox ID="WF_DELFLG" runat="server" Width="8em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>    

            <p id="KEY_LINE_4">
                <!-- ■　Dタブ　■ -->
                <a name="KEY_4" onclick="DtabChange('0')">
                    <asp:Label ID="WF_Dtab01" runat="server" Text="届先情報" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                </a>
                <asp:Label ID="Label4" runat="server" Width="2em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                <a name="KEY_4" onclick="DtabChange('1')">
                    <asp:Label ID="WF_Dtab02" runat="server" Text="書類（PDF or EXCEL）" Height="1.3em" Width="11em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                </a>
            </p>
        </div>

        <!-- DETAIL画面 -->
        <asp:MultiView ID="WF_DetailMView" runat="server">
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
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
                            <td><asp:Label   ID="WF_Rep1_FIELDNM_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label1_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_FIELD_2"   runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:TextBox ID="WF_Rep1_VALUE_2"   runat="server" Text="" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox></td>
                            <td><asp:Label   ID="WF_Rep1_Label2_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_VALUE_TEXT_2" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <td><asp:Label   ID="WF_Rep1_Label3_2"  runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label></td>
                            <%-- 項目(名称・必須表記・項目・値・スペース・フィールド・スペース)　左Side --%>
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
            <!-- ■ PDF選択　■ -->

            <asp:View ID="WF_DView2" runat="server">

                <span class="WF_DViewRep2_Area"
                    ondragstart="f_dragEventCancel(event)"
                    ondrag="f_dragEventCancel(event)"
                    ondragend="f_dragEventCancel(event)" 
                    ondragenter="f_dragEventCancel(event)"
                    ondragleave="f_dragEventCancel(event)" 
                    ondragover="f_dragEventCancel(event)"  
                    ondrop="f_dragEvent(event,'FILE_UP')">    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
                        
                    <!-- PDF表示選択 -->
                    <span style="position:relative;top:0.2em;left:1.3em;">
                        <asp:Label ID="Label12" runat="server" Text="表示選択" Width="6em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </span>

                    <span style="position:relative;top:0.5em;left:0.5em;" onchange="PDFselectChange()">
                        <asp:ListBox ID="WF_Rep2_PDFselect" runat="server" Height="1.5em" Rows="1" Width="13em"></asp:ListBox>
                    </span>

                    <span style="position:relative;top:0.2em;left:3.0em;">
                        <asp:Label ID="Label3" runat="server" Text="添付書類(届先台帳EXCEL)を登録する場合は、ここにドロップすること" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Size="Medium"></asp:Label>
                    </span>
                    <span style="position:absolute;top:1.6em;left:30.5em;">
                        <asp:Label ID="Label15" runat="server" Text="↓↓↓" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Size="Medium"></asp:Label>
                    </span>
                    <br />

                    <!-- PDF明細ヘッダー -->
                    <span style="position:relative;top:0.3em;left:5.0em;display:inline;">
                        <asp:Label ID="Label13" runat="server" Text="ファイル名" Height="1.3em" Width="8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </span>

                    <span style="position:relative;top:0.3em;left:34.3em;display:inline;">
                        <asp:Label ID="Label14" runat="server" Text="削 除" Height="1.3em" Width="8em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    </span>
                    <br />

                    <span style="position:absolute;top:3.4em;left:1.3em;height:7.3em;width:50em;overflow-x:hidden;overflow-y:auto;background-color:white;border:1px solid black;">
                    <asp:Repeater ID="WF_DViewRepPDF" runat="server" >
                        <HeaderTemplate>
                        </HeaderTemplate>

                        <ItemTemplate>
                            <table style="">
                            <tr style="">

                            <td style="height:1.0em;width:37em;color:blue;display:inline-block;">
                            <!-- ■　ファイル記号名称　■ -->
                            <a>　</a>
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

    <%-- rightview --%>
    <MSINC:rightview id="rightview" runat="server" />
    <%-- leftview --%>
    <MSINC:leftview id="leftview" runat="server" />

    <div hidden="hidden">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>   <!-- GridViewダブルクリック -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>  <!-- GridView表示位置フィールド -->
            
            <input id="WF_DTAB_CHANGE_NO" runat="server" value="" type="text"/>           <!-- DetailBox Mview切替 -->
            <input id="WF_DTAB_PDF_DISP_FILE" runat="server" value="" type="text"/>  <!-- DetailBox PDF内容表示 -->

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

    <%-- Work --%>
    <MSINC:work id="work" runat="server" />

</asp:Content>
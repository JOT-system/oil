<%@ Page Title="TA0005" Language="vb" AutoEventWireup="false" CodeBehind="GRTA0005LMKINTAI.aspx.vb" Inherits="OFFICE.GRTA0005LMKINTAI" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>

<%@ register src="inc/GRTA0005WRKINC.ascx" tagname="work" tagprefix="LSINC" %>
<asp:Content ID="GRTA0005H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/TA0005.css")%>"/>
    <script type="text/javascript">
        var pnlListAreaId = '<%= Me.pnlListArea.ClientId %>';
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/TA0005.js")%>"></script>
</asp:Content>
<asp:Content ID="GRTA0005" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　headerbox -->
        <div  class="headerboxOnly" id="headerbox">

            <!-- ■　ボタン　■ -->
            <div class="Operation">
                <a style="position:fixed;top:2.8em;left:58em;" hidden="hidden">
                    <input type="button" id="WF_ButtonPDF" value="印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPDF');" />
                </a>
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonINQ" value="照会"  style="Width:5em" onclick="ButtonClick('WF_ButtonINQ');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonXLS" value="Excel取得"  style="Width:5em" onclick="ButtonClick('WF_ButtonXLS');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.0em;left:75em;">
                    <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" Height="1em" ImageAlign="AbsMiddle" onclick="ButtonClick('WF_ButtonFIRST');" />
                </a>
                <a style="position:fixed;top:3.0em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" Height="1em" ImageAlign="AbsMiddle" onclick="ButtonClick('WF_ButtonLAST');" />
                </a>
            </div>
            <div class="leftMenubox">
                <!-- ■　照会選択タイトル　■ -->
                <div style="overflow-y:auto;height:3.5em;width:11.3em;text-align:left;vertical-align:middle;color:white;background-color:rgb(22,54,92);font-weight:bold;border: solid black;border-width:1.5px;">
                    <a style="width:10.0em;overflow:hidden;text-align:left;">
                        <asp:RadioButton ID="WF_ORG_SW" runat="server" GroupName="selector" Text="　組織選択" Width="8em" onclick="selectorChange('0')" Checked="True" />
                        <asp:RadioButton ID="WF_STAFF_SW" runat="server" GroupName="selector" Text="　従業員選択" Width="8em" onclick="selectorChange('1')"/>
                    </a>
                </div>

                <asp:MultiView ID="WF_SelectorMView" runat="server">
                    <asp:View ID="WF_DView1" runat="server" >
                        <!-- ■　組織セレクター　■ -->
                        <div id="ORGSelect" style="overflow-y:auto;width:11.3em;min-height:16em;max-height:18.5em;color:black;background-color: white;border: solid;border-width:1.5px;">
                            <asp:Repeater ID="WF_ORGselector" runat="server">
                                <HeaderTemplate> 
                                     <table style="border-width:1px;margin:0.1em 0.1em 0.1em 0.1em;white-space:nowrap;">
                               </HeaderTemplate>
                                <ItemTemplate>
                                    <tr> 
                                        <!-- 非表示項目(左Box処理用・Repeater内行位置)　-->
                                        <td hidden="hidden">
                                            <asp:Label ID="WF_SELorg_VALUE" runat="server"></asp:Label>
                                        </td>

                                        <td>
                                            <asp:Label ID="WF_SELorg_TEXT" runat="server" Text="" Width="11.3em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        </td>
                                    </tr> 
                                </ItemTemplate>
                                <FooterTemplate>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>
                    </asp:View>
                    <asp:View ID="WF_DView2" runat="server" >
                        <!-- ■　従業員セレクター　■ -->
                        <div id="STAFFSelect" style="overflow-y:auto;width:11.3em;min-height:16em;max-height:18.5em;color:black;background-color: white;border: solid;border-width:1.5px;">
                            <asp:Repeater ID="WF_STAFFselector" runat="server">
                                <HeaderTemplate> 
                                    <table style="border-width:1px;margin:0.1em 0.1em 0.1em 0.1em;white-space:nowrap;">
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr> 
                                        <!-- 非表示項目(左Box処理用・Repeater内行位置)　-->
                                        <td hidden="hidden">
                                            <asp:Label ID="WF_SELstaff_VALUE" runat="server"></asp:Label>
                                        </td>

                                        <td>
                                            <asp:Label ID="WF_SELstaff_TEXT" runat="server" Text="" Width="11.3em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        </td>
                                    </tr> 
                                </ItemTemplate>
                                <FooterTemplate>
                                     </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>
                    </asp:View>
                    <asp:View ID="View1" runat="server" >
                        <!-- ■　日別・月別セレクター　■ -->
                        <div id="DATESelect" style="overflow-y:auto;width:11.3em;min-height:16em;max-height:18.5em;color:black;background-color: white;border: solid;border-width:1.5px;">
                           <asp:Repeater ID="Repeater1" runat="server">
                                <HeaderTemplate> 
                                    <table style="border-width:1px;margin:0.1em 0.1em 0.1em 0.1em;">
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr> 
                                        <!-- 非表示項目(左Box処理用・Repeater内行位置)　-->
                                        <td hidden="hidden">
                                            <asp:Label ID="WF_SELstaff_VALUE" runat="server"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:Label ID="WF_SELstaff_TEXT" runat="server" Text="" Width="11.3em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        </td>
                                    </tr> 
                                </ItemTemplate>

                                <FooterTemplate>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>
                    </asp:View>
                </asp:MultiView>

                <!-- ■　集計指定タイトル　■ -->
                <div style="height:1.8em;width:11.3em;text-align:center;color:white;background-color:rgb(22,54,92);font-weight:bold;border: solid black;border-width:1.5px;">
                    <a style="width:11.0em;font-size:medium;overflow:hidden;color:white;background-color:rgb(22,54,92);text-align:center;">
                        <asp:Label ID="Label3" runat="server" Text="集計指定"></asp:Label>
                    </a>
                </div>
                <!-- ■　集計指定　■ -->
                <div id="SUMaction" class="SUMaction" style="overflow:auto;height:10.5em;width:11.3em;color:black;background-color: white;border: solid;border-width:1.5px;">
                <!-- 　集計指定　 -->
                    <asp:CheckBox ID="WF_CBOX_SW1" Checked="True" runat="server" Text=" 配属部署別" Width="8em" /><br/>
                    <asp:CheckBox ID="WF_CBOX_SW2" Checked="True" runat="server" Text=" 作業日別" Width="8em" /><br/>
                    <asp:CheckBox ID="WF_CBOX_SW3" Checked="True" runat="server" Text=" 従業員別" Width="8em" /><br/>
                </div>
            </div>
            <!-- 一覧レイアウト -->
            <div id="divListArea">
                <asp:panel id="pnlListArea" runat="server" ></asp:panel>
            </div>

        </div>

        <!-- 全体レイアウト　detailbox -->
        <div  class="detailbox" id="detailbox" hidden="hidden">
        </div>

        <div hidden="hidden">
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>    <!-- GridView表示位置フィールド -->

            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />         <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>        <!-- Textbox DBクリックフィールド -->
            <input id="WF_RightViewChange" runat="server" value="" type="text"/>        <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />         <!-- Rightbox 開閉 -->
            
            <input id="WF_SELECTOR_Chg" runat="server" value=""  type="text" />         <!-- "":未選択/"ON":選択 -->

            <input id="WF_SELECT_SW" runat="server" value=""  type="text" />            <!-- "":未選択/"ON":選択 -->
            <input id="WF_SELECTYYMM" runat="server" value=""  type="text" />           <!-- 処理年月(yyyy年MM月) -->
            <input id="WF_SELECTYYMMDD" runat="server" value=""  type="text" />         <!-- 処理日(yyyy/MM/dd) -->
            <input id="WF_SELECTAREA" runat="server" value=""  type="text" />           <!-- 処理地域 -->
            <input id="WF_SaveX"  runat="server" value=""  type="text" />               <!-- 明細 変更位置X軸 -->
            <input id="WF_SaveY"  runat="server" value=""  type="text" />               <!-- 明細 変更位置Y軸 -->
            <input id="WF_SaveSX"  runat="server" value=""  type="text" />              <!-- セレクタ 変更位置X軸 -->
            <input id="WF_SaveSY"  runat="server" value=""  type="text" />              <!-- セレクタ 変更位置Y軸 -->

            <input id="WF_SELECTOR_SW" runat="server" value=""  type="text" />          <!-- セレクタの選択値 -->
            <input id="WF_SELECTOR_PosiORG" runat="server" value=""  type="text" />     <!-- セレクタの選択値（部署選択行）-->
            <input id="WF_SELECTOR_PosiSTAFF" runat="server" value=""  type="text" />   <!-- セレクタの選択値（乗務員選択行）-->
            <input id="WF_SELECTOR_PosiYM" runat="server" value=""  type="text" />      <!-- セレクタの選択値（年月選択行）-->
            <input id="WF_PrintURL" runat="server" value=""  type="text" />              <!-- Textbox Print URL -->

            <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->
        </div>


        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>




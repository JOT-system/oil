<%@ Page Title="MB0003" Language="vb" AutoEventWireup="false" CodeBehind="GRMB0003HSTAFF.aspx.vb" Inherits="OFFICE.GRMB0003HSTAFF" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>
    
<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRMB0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="MB0003H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/MB0003.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/MB0003.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=if(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="MB0003" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <div class="Operation">
            <!-- ボタン -->
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
            <p>
                <!-- 選択No -->
                <a>
                    <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_Sel_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
            </p>
            <p>
                <!-- 会社コード -->
                <a>
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
            <p>
                <!-- 年月日 -->
                <a>
                    <asp:Label ID="WF_YMD_L" runat="server" Text="年月日" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_YMD" runat="server" Height="1.1em" width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_WEEK" runat="server" Width="17em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
            </p>
        </div>

        <table style="position:fixed; bottom:13.5em; left:1em;">
            <tr>
                <!-- 乗務員 -->
                <td style="height:1.3em; width:8em;">
                    <asp:Label ID="WF_STAFFCODE_L" runat="server" Text="乗務員" Width="17em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </td>
                <!-- 職務区分 -->
                <td style="height:1.3em; width:12em;">
                    <asp:Label ID="WF_STAFFKBN_L" runat="server" Text="職務区分" Width="9em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </td>
                <!-- 勤務状況 -->
                <td style="height:1.3em; width:18.5em;">
                    <asp:Label ID="WF_HOLIDAY_L" runat="server" Text="勤務状況" Width="10em" CssClass="WF_TEXT_CENTER"></asp:Label>
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
                            <!-- 乗務員 -->
                            <td style="height:1.3em; width:14em;">
                                <asp:label ID="WF_Rep_STAFFCODE" runat="server" Height="1.1em" Width="4em" CssClass="WF_TEXT_LABEL"></asp:label>
                                <asp:label ID="WF_Rep_STAFFCODE_TEXT" runat="server" Height="1.1em" Width="10em" CssClass="WF_TEXT_LABEL"></asp:label>
                            </td>
                            <!-- 勤務区分 -->
                            <td style="height:1.3em; width:10em;">
                                <asp:label ID="WF_Rep_STAFFKBN_TEXT" runat="server" Text="" Height="1.1em" Width="10em" CssClass="WF_TEXT_LABEL"></asp:label>
                            </td>
                            <!-- 勤務状況の切替 -->
                            <td style="height:1.3em; width:16em; white-space:nowrap;">
                                <asp:RadioButton ID="WF_Rep_SW1" runat="server" GroupName="WF_Rep_SW" Text=" 出社" Width="6em" />
                                <asp:RadioButton ID="WF_Rep_SW2" runat="server" GroupName="WF_Rep_SW" Text=" 休み" Width="6em" />
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

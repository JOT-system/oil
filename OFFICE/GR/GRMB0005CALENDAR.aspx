<%@ Page Title="MB0005" Language="vb" AutoEventWireup="false" CodeBehind="GRMB0005CALENDAR.aspx.vb" Inherits="OFFICE.GRMB0005CALENDAR" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRMB0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="MB0005H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/MB0005.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/MB0005.js")%>'></script>
</asp:Content>

<asp:Content ID="MB0005" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerboxOnly" id="headerbox">
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
        </div>

        <!-- 年月日 -->
        <div class="DATE_TREE">
            <asp:TreeView ID="WF_DATE_TREE" runat="server"></asp:TreeView>
        </div>

        <table style="position:fixed; top:4.5em; left:20em;">
            <tr>
                <!-- 年月 -->
                <td style="height:1.3em;">
                    <asp:Label ID="WF_YYMM_L" runat="server" Width="14em"></asp:Label>
                </td>
                <!-- テキスト -->
                <td style="height:1.3em;">
                    <asp:Label ID="WF_WORKINGTEXT_L" runat="server" Text="テキスト" Width="16em"></asp:Label>
                </td>
                <!-- 日付区分 -->
                <td style="height:1.3em;">
                    <asp:Label ID="WF_WORKINGKBN_L" runat="server" Text="法廷・法定外区分" Width="18.5em"></asp:Label>
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
                            <td hidden="hidden">
                                <asp:Label id="WF_Rep_DATE" runat="server"></asp:Label>
                            </td>
                            <!-- 日付 -->
                            <td style="height:1.3em;">
                                <asp:Label ID="WF_Rep_DAY" runat="server" Height="1.1em" Width="4em"></asp:Label>
                            </td>
                            <!-- 曜日 -->
                            <td style="height:1.3em;">
                                <asp:Label ID="WF_Rep_WEEK" runat="server" Text="" Height="1.1em" Width="4em"></asp:Label>
                            </td>
                            <!-- テキスト -->
                            <td style="height:1.3em; width:20em;">
                                <asp:TextBox ID="WF_Rep_TEXT" runat="server" Width="18.5em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                            </td>
                            <!-- 勤務状況の切替 -->
                            <td style="height:1.3em; white-space:nowrap;">
                                <asp:RadioButton ID="WF_Rep_SW1" runat="server" GroupName="WF_Rep_SW" Text=" 平日" Width="6em" />
                                <asp:RadioButton ID="WF_Rep_SW2" runat="server" GroupName="WF_Rep_SW" Text=" 法定" Width="6em" />
                                <asp:RadioButton ID="WF_Rep_SW3" runat="server" GroupName="WF_Rep_SW" Text=" 法定外" Width="6em" />
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

        <input id="WF_DISP_DATE" runat="server" value="" type="text" />             <!-- 画面表示用日付 -->
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

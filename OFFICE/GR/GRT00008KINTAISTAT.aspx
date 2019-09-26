<%@ Page Title="T00008" Language="vb" AutoEventWireup="false" CodeBehind="GRT00008KINTAISTAT.aspx.vb" Inherits="OFFICE.GRT00008KINTAISTAT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0004RightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00008WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00008H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00008.css")%>"/>
    <script type="text/javascript">
        var IsPostBack = '<%= if(IsPostBack = True, "1", "0") %>';
    </script>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00008.js")%>"></script>
</asp:Content>

<asp:Content ID="T00008" ContentPlaceHolderID="contents1" runat="server">

    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <div class="Operation">
            <asp:Label ID="Label3"  runat="server" Text="締年月:" Height="1.1em" Width="4em" CssClass="WF_TEXT_LEFT" Font-Bold="True" ForeColor="Red" Font-Size="Large"></asp:Label>
            <asp:Label ID="WF_LIMITYY"  runat="server"  Height="1.1em" Width="2.0em" CssClass="WF_TEXT_LEFT" Font-Bold="True" ForeColor="Red" Font-Size="Large"></asp:Label>
            <asp:Label ID="Label4"  runat="server" Text="年" Height="1.1em" Width="1.5em" CssClass="WF_TEXT_LEFT" Font-Bold="True" ForeColor="Red" Font-Size="Large"></asp:Label>
            <asp:Label ID="WF_LIMITMM"  runat="server"  Height="1.1em" Width="1.0em" CssClass="WF_TEXT_LEFT" Font-Bold="True" ForeColor="Red" Font-Size="Large"></asp:Label>
            <asp:Label ID="Label5"  runat="server" Text="月" Height="1.1em" Width="2em" CssClass="WF_TEXT_LEFT" Font-Bold="True" ForeColor="Red" Font-Size="Large"></asp:Label>
            <a style="position:fixed;top:2.8em;left:23em;">
                <input type="button" id="WF_ButtonALLSELECT" value="全選択"  style="Width:5em" onclick="ButtonClick('WF_ButtonALLSELECT');" />
            </a>
            <a style="position:fixed;top:2.8em;left:27.5em;">
                <input type="button" id="WF_ButtonALLCANCEL" value="全解除"  style="Width:5em" onclick="ButtonClick('WF_ButtonALLCANCEL');" />
            </a>

            <a style="position:fixed;top:2.8em;left:35em;">
                <input type="button" id="WF_ButtonLIMIT" value="勤怠締"  style="Width:9em" onclick="ButtonClick('WF_ButtonLIMIT');" />
            </a>

            <a style="position:fixed;top:2.8em;left:43em;">
                <input type="button" id="WF_ButtonRELEASE" value="勤怠締 解除"  style="Width:9em" onclick="ButtonClick('WF_ButtonRELEASE');" />
            </a>

            <a style="position:fixed;top:2.8em;left:51em;">
                <input type="button" id="WF_ButtonJOURNAL" value="給与ｼﾞｬｰﾅﾙ作成"  style="Width:9em" onclick="ButtonClick('WF_ButtonJOURNAL');" />
            </a>

            <a style="position:fixed;top:2.8em;left:59em;">
                <input type="button" id="WF_ButtonBUMON" value="部門別按分ﾀﾞｳﾝﾛｰﾄﾞ"  style="Width:9em" onclick="ButtonClick('WF_ButtonBUMON');" />
            </a>

            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div  class="detailbox" id="detailbox">

        <!-- ■■■　明細ヘッダ　■■■ -->
        <table style="position:fixed;top:5em; bottom:35.8em;left:10.0em;width:30em;margin:0px 0px 0px 0px;">
            <tr>

            <td style="height:1.3em;width:2em;">
                <!-- ■　項番　■ -->
                <asp:Label ID="Label6"  runat="server" Text="項番" Height="1.1em" Width="2em" CssClass="WF_TEXT_CENTER"></asp:Label>
            </td>

            <td style="height:1.3em;width:2.0em;" rowspan="2">
                <!-- ■　チェックボックス　■ -->
                <asp:Label ID="Label9"  runat="server" Text="選択" Height="1.1em" Width="2.0em" CssClass="WF_TEXT_CENTER"></asp:Label>
            </td>

            <td style="height:1.3em;width:12em;">
                <!-- ■　配属部署　■ -->
                <asp:Label ID="Label2"  runat="server" Text="配属部署" Height="1.1em" Width="18em" CssClass="WF_TEXT_CENTER"></asp:Label>
            </td>

            <td style="height:1.3em;width:12em;">
                <!-- ■　締フラグ　■ -->
                <asp:Label ID="Label1"  runat="server" Text="勤怠締 状況" Height="1.1em" Width="13em" CssClass="WF_TEXT_CENTER"></asp:Label>
            </td>

            </tr>
        </table>

        <!-- ■■■　明　細　■■■ -->
        <span class="WF_Repeater" style="position:fixed;top:6.6em; bottom:2em;left:10.0em;width:30em;overflow:auto;background-color:white;table-layout:fixed;margin:0px 0px 0px 0px;" >

            <asp:Repeater ID="WF_Repeater" runat="server" >
                <HeaderTemplate>
                </HeaderTemplate>

                <ItemTemplate>
                    <table style="border:solid;border-width:thin;margin:0px 0px 0px 0px;">
                    <tr style="<%# DataBinder.Eval(Container.DataItem, "COLOR") %>;">
                    <!-- ■　項番　■ -->
                    <td style="height:1.3em;width:2.0em;white-space: nowrap;">
                        <asp:label       ID="WF_Rep_LINECNT"   runat="server" Height="1.1em" Width="1.5em"  CssClass="WF_TEXT_RIGHT"></asp:label>
                    </td>

                    <!-- ■　選択　■ -->
                    <td style="height:1.3em;width:2.0em;">
                        <asp:checkbox ID="WF_Rep_CheckBox" runat="server" Width="1.5em" Checked="True" />
                    </td>

                    <!-- ■　配属部署　■ -->
                    <td style="height:1.3em;width:17.0em;white-space: nowrap;">
                        <asp:label       ID="WF_Rep_ORGCODE"   runat="server" Height="1.3em" Width="3.5em"  CssClass="WF_TEXT_LEFT"></asp:label>
                        <asp:label       ID="WF_Rep_ORGCODENAME"   runat="server" Height="1.3em" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:label>
                    </td>

                    <!-- ■　締フラグ　■ -->
                    <td style="height:1.3em;width:7.0em;white-space: nowrap;">
                        <asp:label       ID="WF_Rep_LIMITFLGNAME"   runat="server" Height="1.3em" Width="7.0em" CssClass="WF_TEXT_CENTER"></asp:label>
                    </td>

                    <!-- ■　未承認件数１（非表示）　■ -->
                    <td style="height:1.3em;white-space: nowrap;">
                        <asp:label       ID="WF_Rep_UNAPPROVED1" runat="server" Height="1.3em" Width="5.0em" CssClass="WF_TEXT_CENTER"></asp:label>
                    </td>
                    <!-- ■　未承認件数２（非表示）　■ -->
                    <td style="height:1.3em;white-space: nowrap;">
                        <asp:label       ID="WF_Rep_UNAPPROVED2" runat="server" Height="1.3em" Width="5.0em"  CssClass="WF_TEXT_CENTER"></asp:label>
                    </td>
                    <!-- ■　未申請件数（非表示）　■ -->
                    <td style="height:1.3em;white-space: nowrap;">
                        <asp:label       ID="WF_Rep_UNAPPLIED"   runat="server" Height="1.3em" Width="5.0em"  CssClass="WF_TEXT_CENTER"></asp:label>
                    </td>
                    <!-- ■　NGレコード件数（非表示）　■ -->
                    <td style="height:1.3em;white-space: nowrap;">
                        <asp:label       ID="WF_Rep_NGREC"       runat="server" Height="1.3em" Width="5.0em"  CssClass="WF_TEXT_CENTER"></asp:label>
                    </td>
                    <!-- ■　締区分（非表示）　■ -->
                    <td style="height:1.3em;white-space: nowrap;">
                        <asp:label       ID="WF_Rep_LIMITKBN"    runat="server" Height="1.3em" Width="5.0em"  CssClass="WF_TEXT_CENTER"></asp:label>
                    </td>
                    </tr>
                    </table>
                </ItemTemplate>

                <FooterTemplate>
                </FooterTemplate>
             
            </asp:Repeater>
        </span>

    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <div hidden="hidden">
        <input id="WF_RightViewChange" runat="server" value="" type="text"/>            <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value=""  type="text" />             <!-- Rightbox 開閉 -->
        <input id="WF_REP_POSITION"  runat="server" value=""  type="text" />        <!-- Repeater 行位置 -->
        <input id="WF_DownURL" runat="server" value=""  type="text" />              <!-- Textbox Print URL -->
        <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />        <!-- 権限 -->
        <input id="WF_MAPpermitcode2" runat="server" value=""  type="text" />       <!-- 権限 -->
        <input id="WF_UNAPPROVED" runat="server" value=""  type="text" />           <!-- 未承認 -->
        <input id="WF_UNAPPLIED" runat="server" value=""  type="text" />            <!-- 未申請 -->
        <input id="WF_NGREC" runat="server" value=""  type="text" />                <!-- NGレコード -->
        <input id="WF_LIMITKBN" runat="server" value=""  type="text" />             <!-- 締区分 -->
        <input id="WF_FORCE" runat="server" value=""  type="text" />                <!-- 強制実行 -->
    </div>

    <!-- Work レイアウト -->
    <LSINC:work id="work" runat="server" />
           
</asp:Content>

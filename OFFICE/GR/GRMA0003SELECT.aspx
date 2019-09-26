<%@ Page  Title="MA0003S" Language="vb" AutoEventWireup="false" CodeBehind="GRMA0003SELECT.aspx.vb" Inherits="OFFICE.GRMA0003SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0003SRightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRMA0003WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="MA0003SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/MA0003S.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/MA0003S.js")%>"></script>
</asp:Content>

<asp:Content ID="MA0003S" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　searchbox -->
        <div  class="searchbox" id="searchbox" >
            <!-- ○ 固定項目 ○ -->
            <a style="position:fixed;top:2.8em;left:62.5em;">
                <input type="button" id="WF_ButtonDO" value="実行"  style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
            </a>
            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>

            <!-- ○ 変動項目 ○ -->
            <!-- 　会社コード　 -->
            <a style="position:fixed;top:7.7em;left:4em;font-weight:bold;text-decoration:underline">会社コード</a>

            <a style="position:fixed;top:7.7em;left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE' ,  <%= LIST_BOX_CLASSIFICATION.LC_COMPANY%>)" onchange="TextBox_change('WF_CAMPCODE')">
                <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:7.7em;left:27em;">
                <asp:Label ID="WF_CAMPCODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- 　年度　 -->
            <a style="position:fixed;top:9.9em;left:4em;font-weight:bold;text-decoration:underline">有効年月日</a>
            <a style="position:fixed;top:9.9em;left:11.5em;">範囲指定</a>
            <a style="position:fixed;top:9.9em;left:42.5em;">～</a>

            <a style="position:fixed;top:9.9em;left:18em;" onchange="TextBox_change('WF_STYMD')" ondblclick="Field_DBclick('WF_STYMD', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                <asp:TextBox ID="WF_STYMD" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:9.9em;left:44em;" onchange="TextBox_change('WF_ENDYMD')" ondblclick="Field_DBclick('WF_ENDYMD', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            
            <!-- 　管理部署　 -->
            <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">管理部署</a>

            <a style="position:fixed;top:12.1em;left:18em;" ondblclick="Field_DBclick('WF_MORG' ,  <%= LIST_BOX_CLASSIFICATION.LC_ORG%>)" onchange="TextBox_change('WF_MORG')">
                <asp:TextBox ID="WF_MORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:12.1em;left:27em;">
                <asp:Label ID="WF_MORG_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- 　設置部署　 -->
            <a style="position:fixed;top:14.3em;left:4em;font-weight:bold;text-decoration:underline">設置部署</a>

            <a style="position:fixed;top:14.3em;left:18em;" ondblclick="Field_DBclick('WF_SORG' ,  <%= LIST_BOX_CLASSIFICATION.LC_ORG%>)" onchange="TextBox_change('WF_SORG')">
                <asp:TextBox ID="WF_SORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:14.3em;left:27em;">
                <asp:Label ID="WF_SORG_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- 　油種　 -->
            <a style="position:fixed;top:16.5em;left:4em;font-weight:bold;text-decoration:underline">油種</a>
            <a style="position:fixed;top:16.5em;left:11.5em;"></a>
            <a style="position:fixed;top:16.5em;left:42.5em;"></a>

            <a style="position:fixed;top:16.5em;left:18em;" ondblclick="Field_DBclick('WF_OILTYPE1' ,  <%= LIST_BOX_CLASSIFICATION.LC_OILTYPE%>)" onchange="TextBox_change('WF_OILTYPE1')">
                <asp:TextBox ID="WF_OILTYPE1" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:16.5em;left:27em;">
                <asp:Label ID="WF_OILTYPE1_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <a style="position:fixed;top:16.5em;left:44em;" ondblclick="Field_DBclick('WF_OILTYPE2' ,  <%= LIST_BOX_CLASSIFICATION.LC_OILTYPE%>)" onchange="TextBox_change('WF_OILTYPE2')">
                <asp:TextBox ID="WF_OILTYPE2" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:16.5em;left:53em;">
                <asp:Label ID="WF_OILTYPE2_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- 　荷主　 -->
            <a style="position:fixed;top:18.7em;left:4em;font-weight:bold;text-decoration:underline">荷主</a>
            <a style="position:fixed;top:18.7em;left:11.5em;">範囲指定</a>
            <a style="position:fixed;top:18.7em;left:42.5em;">～</a>

            <a style="position:fixed;top:18.7em;left:18em;" ondblclick="Field_DBclick('WF_OWNCODEF' ,  <%= LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)" onchange="TextBox_change('WF_OWNCODEF')">
                <asp:TextBox ID="WF_OWNCODEF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:18.7em;left:27em;">
                <asp:Label ID="WF_OWNCODEF_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed;top:18.7em;left:44em;" ondblclick="Field_DBclick('WF_OWNCODET' ,  <%= LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)" onchange="TextBox_change('WF_OWNCODET')">
                <asp:TextBox ID="WF_OWNCODET" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:18.7em;left:53em;">
                <asp:Label ID="WF_OWNCODET_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- 　車両タイプ　 -->
            <a style="position:fixed;top:20.9em;left:4em;font-weight:bold;text-decoration:underline">車両タイプ</a>

            <a style="position:fixed;top:20.9em;left:18em;" ondblclick="Field_DBclick('WF_SHARYOTYPE1' ,  999)" onchange="TextBox_change('WF_SHARYOTYPE1')">
                <asp:TextBox ID="WF_SHARYOTYPE1" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:20.9em;left:27em;">
                <asp:Label ID="WF_SHARYOTYPE1_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed;top:20.9em;left:44em;" ondblclick="Field_DBclick('WF_SHARYOTYPE2' ,  999)" onchange="TextBox_change('WF_SHARYOTYPE2')">
                <asp:TextBox ID="WF_SHARYOTYPE2" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:20.9em;left:53em;">
                <asp:Label ID="WF_SHARYOTYPE2_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <a style="position:fixed;top:23.1em;left:18em;" ondblclick="Field_DBclick('WF_SHARYOTYPE3' ,  999)" onchange="TextBox_change('WF_SHARYOTYPE3')">
                <asp:TextBox ID="WF_SHARYOTYPE3" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:23.1em;left:27em;">
                <asp:Label ID="WF_SHARYOTYPE3_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed;top:23.1em;left:44em;" ondblclick="Field_DBclick('WF_SHARYOTYPE4' ,  999)" onchange="TextBox_change('WF_SHARYOTYPE4')">
                <asp:TextBox ID="WF_SHARYOTYPE4" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:23.1em;left:53em;">
                <asp:Label ID="WF_SHARYOTYPE4_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <a style="position:fixed;top:25.3em;left:18em;" ondblclick="Field_DBclick('WF_SHARYOTYPE5' ,  999)" onchange="TextBox_change('WF_SHARYOTYPE5')">
                <asp:TextBox ID="WF_SHARYOTYPE5" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:25.3em;left:27em;">
                <asp:Label ID="WF_SHARYOTYPE5_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <a hidden="hidden">
                <input id="WF_FIELD"  runat="server" value=""  type="text" />          <!-- Textbox DBクリックフィールド -->
                <input id="WF_SelectedIndex" runat="server" value="" type="text"/>     <!-- Textbox DBクリックフィールド -->

                <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />    <!-- Textbox DBクリックフィールド -->
                <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>   <!-- Textbox DBクリックフィールド -->

                <input id="WF_RightViewChange" runat="server" value="" type="text"/>      <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />       <!-- Rightbox 開閉 -->

                <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
            </a>

        </div>

        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>
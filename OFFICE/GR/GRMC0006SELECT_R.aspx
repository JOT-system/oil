<%@ Page Title="MC0006S_R" Language="vb" AutoEventWireup="false" CodeBehind="GRMC0006SELECT_R.aspx.vb" Inherits="OFFICE.GRMC0006SELECT_R" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0003SRightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="~/GR/inc/GRMC0006WRKINC.ascx" tagname="work" tagprefix="MSINC" %>

<asp:Content ID="MC0006SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/MC0006S.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/MC0006S.js")%>"></script>
</asp:Content>
<asp:Content ID="MC0006S" ContentPlaceHolderID="contents1" runat="server">
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

        <a style="position:fixed;top:9.9em;left:18em;" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
            <asp:TextBox ID="WF_STYMD" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>
        <a style="position:fixed;top:9.9em;left:44em;" ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
            <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>
            
        <!-- 　取引先コード　 -->
        <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">取引先</a>
        <a style="position:fixed;top:12.1em;left:11.5em;">範囲指定</a>
        <a style="position:fixed;top:12.1em;left:42.5em;">～</a>

        <a style="position:fixed;top:12.1em;left:18em;" ondblclick="Field_DBclick('WF_TORICODEF' ,  <%= LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)" onchange="TextBox_change('WF_TORICODEF')">
            <asp:TextBox ID="WF_TORICODEF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>
        <a style="position:fixed;top:12.1em;left:27em;">
            <asp:Label ID="WF_TORICODEF_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <a style="position:fixed;top:12.1em;left:44em;" ondblclick="Field_DBclick('WF_TORICODET' ,  <%= LIST_BOX_CLASSIFICATION.LC_CUSTOMER%>)" onchange="TextBox_change('WF_TORICODET')">
            <asp:TextBox ID="WF_TORICODET" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>
        <a style="position:fixed;top:12.1em;left:53em;">
            <asp:Label ID="WF_TORICODET_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 　届先コード　 -->
        <a style="position:fixed;top:14.3em;left:4em;font-weight:bold;text-decoration:underline">届先</a>

        <a style="position:fixed;top:14.3em;left:18em;" ondblclick="Field_DBclick('WF_TODOKECODE' ,   <%= LIST_BOX_CLASSIFICATION.LC_DISTINATION%>)" onchange="TextBox_change('WF_TODOKECODE')">
            <asp:TextBox ID="WF_TODOKECODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>
        <a style="position:fixed;top:14.3em;left:27em;">
            <asp:Label ID="WF_TODOKECODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 　届先名称　 -->
        <a style="position:fixed;top:16.5em;left:11.5em;">名称</a>

        <a style="position:fixed;top:16.5em;left:18em;">
            <asp:TextBox ID="WF_TODOKENAME" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>
            
        <!-- 　郵便番号　 -->
        <a style="position:fixed;top:18.7em;left:4em;font-weight:bold;">郵便番号</a>
        <a style="position:fixed;top:18.7em;left:11.5em;"></a>
        <a style="position:fixed;top:18.7em;left:42.5em;"></a>

        <a style="position:fixed;top:18.7em;left:18em;">
            <asp:TextBox ID="WF_POSTNUM" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>

        <!-- 　住所　 -->
        <a style="position:fixed;top:20.9em;left:4em;font-weight:bold;">住所</a>
        <a style="position:fixed;top:20.9em;left:11.5em;"></a>
        <a style="position:fixed;top:20.9em;left:42.5em;"></a>

        <a style="position:fixed;top:20.9em;left:18em;">
            <asp:TextBox ID="WF_ADDR" runat="server" Height="1.4em" Width="41em" onblur="MsgClear()"></asp:TextBox>
        </a>

        <!-- 　電話番号　 -->
        <a style="position:fixed;top:23.1em;left:4em;font-weight:bold;">電話番号</a>

        <a style="position:fixed;top:23.1em;left:18em;">
            <asp:TextBox ID="WF_TEL" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>

        <!-- 　FAX番号　 -->
        <a style="position:fixed;top:25.3em;left:4em;font-weight:bold;">FAX番号</a>

        <a style="position:fixed;top:25.3em;left:18em;">
            <asp:TextBox ID="WF_FAX" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>

        <!-- 　市町村コード　 -->
        <a style="position:fixed;top:27.5em;left:4em;font-weight:bold;text-decoration:underline">市町村コード</a>

        <a style="position:fixed;top:27.5em;left:18em;" ondblclick="Field_DBclick('WF_CITIES' ,   <%= LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('WF_CITIES')">
            <asp:TextBox ID="WF_CITIES" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>
        <a style="position:fixed;top:27.5em;left:27em;">
            <asp:Label ID="WF_CITIES_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 　分類　 -->
        <a style="position:fixed;top:29.7em;left:4em;font-weight:bold;text-decoration:underline">分類</a>

        <a style="position:fixed;top:29.7em;left:18em;" ondblclick="Field_DBclick('WF_CLASS' ,   <%= LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('WF_CLASS')">
            <asp:TextBox ID="WF_CLASS" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
        </a>
        <a style="position:fixed;top:29.7em;left:27em;">
            <asp:Label ID="WF_CLASS_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <a hidden="hidden">
            <input id="WF_FIELD"  runat="server" value=""  type="text" />          <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />    <!-- Leftbox 開閉 -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>   <!-- Leftbox Mview切替 -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text"/>     <!-- Leftbox Mview選択表示 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text"/>   <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value=""  type="text" />    <!-- Rightbox 開閉 -->

            <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
        </a>
    </div>

    <%-- leftview --%>
    <MSINC:leftview id="leftview" runat="server" />
    <%-- rightview --%>
    <MSINC:rightview id="rightview" runat="server" />
    <%-- Work --%>
    <MSINC:work id="work" runat="server" />

</asp:Content>

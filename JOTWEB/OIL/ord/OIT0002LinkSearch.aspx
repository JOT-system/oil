<%@ Page Title="OIT0002S" Language="vb" AutoEventWireup="false" CodeBehind="OIT0002LinkSearch.aspx.vb" Inherits="JOTWEB.OIT0002LinkSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIT0002SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0002S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0002S.js")%>'></script>
</asp:content>

<asp:Content ID="OIT0002S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <a style="position:fixed;top:2.8em;left:62.5em;">
            <input type="button" id="WF_ButtonDO" value="検索"  style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
        </a>
        <a style="position:fixed;top:2.8em;left:67em;">
            <input type="button" id="WF_ButtonEND" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
        </a>

        <!-- ○ 変動項目 ○ -->
        <!-- 会社コード -->
        <a style="display:none; position:fixed; top:5.5em; left:4em; font-weight:bold; text-decoration:underline;">会社コード</a>

        <a style="display:none; position:fixed; top:5.5em; left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="display:none; position:fixed; top:5.5em; left:27em;">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 発駅コード -->
        <a style="position:fixed; top:7.7em; left:5em; font-weight:bold; text-decoration:underline;">★発駅</a>

        <a style="position:fixed; top:7.7em; left:10em;" ondblclick="Field_DBclick('WF_DEPSTATION', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_OILTANKCODE');">
            <asp:TextBox ID="WF_DEPSTATION" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:7.7em; left:27em;">
            <asp:Label ID="WF_DEPSTATION_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 年月日 -->
        <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;">★年月日</a>
        <a style="position:fixed; top:9.9em; left:10em;" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:TextBox ID="WF_STYMD" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:9.9em; left:19em;">～</a>
        <a style="position:fixed; top:9.9em; left:20.5em;" ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>

        <!-- 本線列車番号 -->
        <a style="position:fixed; top:12.1em; left:4em; font-weight:bold; text-decoration:underline;">列車番号</a>

        <a style="position:fixed; top:12.1em; left:10em;" ondblclick="Field_DBclick('WF_TRAINNO', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_TRAINNO');">
            <asp:TextBox ID="WF_TRAINNO" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="position:fixed; top:12.1em; left:27em;">
            <asp:Label ID="WF_TRAINNO_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>
        
        <!-- ステータス選択 -->
        <a style="position:fixed; top:14.3em; left:1em;">ステータス選択</a>
       
        <a class="inline-radio" id="checkbox" style="position:fixed; top:14.3em; left:10em;">
            <div><asp:RadioButton ID="WF_SW1" runat="server" GroupName="WF_SW" Text="利用可のみ表示" /></div>
            <div><asp:RadioButton ID="WF_SW2" runat="server" GroupName="WF_SW" Text="全て表示" /></div>
        </a>

<%--        <a class="inline-radio" id="checkbox" style="position:fixed; top:14.3em; left:10em;">
	        <div><input type="radio" name="title" checked="checked" onclick="ButtonClick('WF_CheckBox');"/><label>利用可のみ表示</label></div>
	        <div><input type="radio" name="title" onclick="ButtonClick('WF_CheckBox');"/><label>全て表示</label></div>
        </a>--%>

<%--        <a style="position:fixed; top:14.3em; left:10em;">
            <asp:RadioButton ID="WF_SW1" runat="server" GroupName="WF_SW" Text=" 利用可のみ表示" Width="9em" />
            <asp:RadioButton ID="WF_SW2" runat="server" GroupName="WF_SW" Text=" 全て表示" Width="9em" />
        </a>--%>
<%--        <!-- 利用済含む -->
        <a style="position:fixed; top:14.3em; left:4em;">利用済含む</a>

        <a style="position:fixed;top:14.3em;left:10em;">
            <asp:CheckBox id="WF_INCLUDUSED" runat="server"  onclick="ButtonClick('WF_CheckBox');"/>
        </a>
        <a style="position:fixed; top:14.3em; left:27em;">
            <asp:Label ID="WF_INCLUDUSED_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>--%>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
    </div>
</asp:Content>

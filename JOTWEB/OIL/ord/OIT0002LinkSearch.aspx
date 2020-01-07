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
            <input type="button" id="WF_ButtonDO" value="検索"  class="btn-sticky" style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
        </a>
        <a style="position:fixed;top:2.8em;left:67em;">
            <input type="button" id="WF_ButtonEND" value="戻る"  class="btn-sticky" style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
        </a>

        <!-- ○ 変動項目 ○ -->
        <!-- 会社コード -->
        <a style="display:none; position:fixed; top:3.3em; left:4em; font-weight:bold; text-decoration:underline;">会社コード</a>

        <a  class="ef" style="display:none; position:fixed; top:5.5em; left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
            <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="display:none; position:fixed; top:3.3em; left:27em;">
            <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 組織コード -->
        <a style="display:none; position:fixed; top:5.5em; left:4em; font-weight:bold; text-decoration:underline;">組織コード</a>

        <a  class="ef" style="display:none; position:fixed; top:5.5em; left:18em;" ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
            <asp:TextBox ID="WF_ORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
        </a>
        <a style="display:none; position:fixed; top:5.5em; left:27em;">
            <asp:Label ID="WF_ORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 空車着駅（発駅）コード -->
        <a id="WF_RETSTATION_LABEL" class="requiredMark">空車着駅</a>
        <a id="WF_RETSTATION_ICON" onclick="Field_DBclick('WF_RETSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);">
            <asp:Image runat="server" ImageUrl="../img/leftbox.png"/>
        </a>

        <a class="ef" id="WF_RETSTATION" ondblclick="Field_DBclick('WF_RETSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('WF_RETSTATION');">
            <asp:TextBox ID="WF_RETSTATION_CODE" runat="server" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
        </a>
        <a id="WF_RETSTATION_TEXT">
            <asp:Label ID="WF_RETSTATION_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 空車発駅（着駅）コード -->
        <a style="display:none;" id ="WF_DEPSTATION_LABEL" class="requiredMark">空車発駅</a>
        <a style="display:none;" id="WF_DEPSTATION_ICON" onclick="Field_DBclick('WF_RETSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);">
            <asp:Image runat="server" ImageUrl="../img/leftbox.png"/>
        </a>

        <a style="display:none;" class="ef" id="WF_DEPSTATION" ondblclick="Field_DBclick('WF_DEPSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('WF_DEPSTATION');">
            <asp:TextBox ID="WF_DEPSTATION_CODE" runat="server" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
        </a>
        <a style="display:none;" id="WF_DEPSTATION_TEXT">
            <asp:Label ID="WF_DEPSTATION_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
        </a>

        <!-- 利用可能日 -->
        <a id="WF_STYMD_LABEL" class="requiredMark">利用可能日</a>
        <a id="WF_STYMD_ICON" onclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:Image runat="server" ImageUrl="../img/calendar.png"/>
        </a>

        <a class="ef" id="WF_STYMD" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:TextBox ID="WF_STYMD_CODE" runat="server" onblur="MsgClear();"></asp:TextBox>
        </a>

        <a id="WF_ENDYMD_LABEL">～</a>
        <a id="WF_ENDYMD_ICON" onclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:Image runat="server" ImageUrl="../img/calendar.png"/>
        </a>

        <a class="ef" id="WF_ENDYMD"  ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
            <asp:TextBox ID="WF_ENDYMD_CODE" runat="server" onblur="MsgClear();"></asp:TextBox>
        </a>

        <!-- 本線列車番号 -->
        <a id="WF_TRAINNO_LABEL">列車番号</a>

        <%--<a class="ef" id="WF_TRAINNO" ondblclick="Field_DBclick('WF_TRAINNO', <%=LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER%>);" onchange="TextBox_change('WF_TRAINNO');">--%>
        <a class="ef" id="WF_TRAINNO">
            <asp:TextBox ID="WF_TRAINNO_CODE" runat="server" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
        </a>

        <a id="WF_TRAINNO_TEXT">
            <asp:Label ID="WF_TRAINNO_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
        </a>
        
        <!-- ステータス選択 -->
        <a id="WF_SW_LABEL">ステータス選択</a>
       
        <a class="inline-radio" id="checkbox">
            <div><asp:RadioButton ID="WF_SW1" runat="server" GroupName="WF_SW" Text="利用可のみ表示" /></div>
            <div><asp:RadioButton ID="WF_SW2" runat="server" GroupName="WF_SW" Text="全て表示" /></div>
        </a>
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

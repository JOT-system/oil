<%@ Page Title="OIS0002PC" Language="vb" AutoEventWireup="false" CodeBehind="OIS0002PasswordChange.aspx.vb" Inherits="JOTWEB.OIS0002PasswordChange" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>

<%@ Register Src="~/OIL/inc/OIS0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIS0002PCH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIS0002PC.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIS0002PC.js")%>'></script>
</asp:content>

<asp:Content ID="OIS0002PC" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO" class="btn-sticky" value="変更" onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />

            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 現在パスワード -->
            <div class="inputItem">
                <a id="WF_CURRENTPASSWORD_LABEL" class="requiredMark">現在のパスワード</a>
                <asp:TextBox ID="WF_CURRENTPASSWORD" runat="server" CssClass="WF_TEXTBOX_CSS" onblur="MsgClear();" MaxLength="20" TextMode="Password"></asp:TextBox>
            </div>
            <!-- 新パスワード -->
            <div class="inputItem">
                <a id="WF_NEWPASSWORD_LABEL" class="requiredMark">新しいパスワード</a>
                <asp:TextBox ID="WF_NEWPASSWORD" runat="server" CssClass="WF_TEXTBOX_CSS" onblur="MsgClear();" MaxLength="20" TextMode="Password"></asp:TextBox>
            </div>
            <!-- 新パスワード(確認用) -->
            <div class="inputItem">
                <a id="WF_NEWPASSWORDCONF_LABEL" class="requiredMark">新しいパスワード（確認用）</a>
                <asp:TextBox ID="WF_NEWPASSWORDCONF" runat="server" CssClass="WF_TEXTBOX_CSS" onblur="MsgClear();" MaxLength="20" TextMode="Password"></asp:TextBox>
            </div>

        </div> <!-- End inputBox -->
    </div> <!-- End searchbox -->

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
    </div>
</asp:Content>

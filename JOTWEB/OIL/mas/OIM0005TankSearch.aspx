<%@ Page Title="OIM0005S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0005TankSearch.aspx.vb" Inherits="JOTWEB.OIM0005TankSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIM0005SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIM0005S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0005S.js")%>'></script>
</asp:content>

<asp:Content ID="OIM0005S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonKINOENE"  class="btn-sticky" value="甲子貨車マスタメンテ"  runat="server" onclick="ButtonClick('WF_ButtonKINOENE');" />
                <input type="button" id="WF_ButtonDO"  class="btn-sticky" value="検索"  onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 会社コード -->
            <div class="inputItem">
                <a id="WF_CAMPCODE_LABEL" style="display:none">会社コード</a>
                <a class="ef" id="WF_CAMPCODE_CODE" style="display:none" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_CAMPCODE_NAME" style="display:none">
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 組織コード -->
            <div class="inputItem">
                <a id="WF_ORG_LABEL" style="display:none">組織コード</a>
                <a class="ef" id="WF_ORG_CODE" style="display:none" ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
                    <asp:TextBox ID="WF_ORG" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_ORG_NAME" style="display:none">
                    <asp:Label ID="WF_ORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- JOT車番 -->
            <div class="inputItem">
                <a id="WF_TANKNUMBER_LABEL">JOT車番</a>
                <a class="ef" id="WF_TANKNUMBER" ondblclick="Field_DBclick('WF_TANKNUMBER', <%=LIST_BOX_CLASSIFICATION.LC_TANKNUMBER%>);" onchange="TextBox_change('WF_TANKNUMBER');">
                    <asp:TextBox  ID="WF_TANKNUMBER_CODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="8"  placeholder="未入力は全件表示"></asp:TextBox>
                </a>
                <a id="WF_TANKNUMBER_TEXT">
                    <asp:Label ID="WF_TANKNUMBER_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 型式 -->
            <div class="inputItem">
                <a id="WF_MODEL_LABEL">型式</a>
                <a class="ef"  id="WF_MODEL"  ondblclick="Field_DBclick('WF_MODEL', <%=LIST_BOX_CLASSIFICATION.LC_TANKMODEL%>);" onchange="TextBox_change('WF_MODEL');">
                    <asp:TextBox  ID="WF_MODEL_CODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="20"></asp:TextBox>
                </a>
                <a id="WF_MODEL_TEXT">
                    <asp:Label ID="WF_MODEL_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 利用フラグ -->
            <div class="inputItem">
                <a id="WF_USEDFLG_LABEL">利用フラグ</a>
                <a class="ef"  id="WF_USEDFLG"  ondblclick="Field_DBclick('WF_USEDFLG', <%=LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY%>);" onchange="TextBox_change('WF_USEDFLG');">
                    <asp:TextBox  ID="WF_USEDFLG_CODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </a>
                <a id="WF_USEDFLG_TEXT">
                    <asp:Label ID="WF_USEDFLG_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 運用基地コード -->
            <div class="inputItem">
                <a id="WF_OPERATIONBASE_LABEL">運用基地コード</a>
                <a class="ef"  id="WF_OPERATIONBASE"  ondblclick="Field_DBclick('WF_OPERATIONBASECODE', <%=LIST_BOX_CLASSIFICATION.LC_BASE%>);" onchange="TextBox_change('WF_OPERATIONBASECODE');">
                    <asp:TextBox  ID="WF_OPERATIONBASECODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_OPERATIONBASE_TEXT">
                    <asp:Label ID="WF_OPERATIONBASENAME" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- リース先チェックボックス -->
            <div class="inputItem">
                <a id="WF_LEASECODE_LABEL">リース先</a>
                <asp:RadioButtonList ID="WF_LEASECODE_LIST" runat="server" RepeatColumns="3" RepeatDirection="Vertical" RepeatLayout="Table">
                    <asp:ListItem Value="">なし　</asp:ListItem>
                    <asp:ListItem Value="11">日本ＯＴ　</asp:ListItem>
                    <asp:ListItem Value="71">在日米軍　</asp:ListItem>
                </asp:RadioButtonList>
            </div>
        </div> <!-- End inputBox -->
    </div> <!-- End searchbox -->

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

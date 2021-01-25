<%@ Page Title="OIM0003S" Language="vb" AutoEventWireup="false" CodeBehind="OIM0003ProductSearch.aspx.vb" Inherits="JOTWEB.OIM0003ProductSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content id="OIM0003SH" contentplaceholderid="head" runat="server">
    <%-- <link href='<%=ResolveUrl("~/OIL/css/OIM0003S.css")%>' rel="stylesheet" type="text/css" /> --%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0003S .js")%>'></script>
</asp:Content>

<asp:Content ID="OIM0003S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO" class="btn-sticky" value="検索" onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />

            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">

            <!-- 営業所コード -->
            <div class="inputItem">
                <a id="LblOfficeCode">営業所コード</a>
                <span ondblclick="Field_DBclick('WF_OFFICECODE', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('WF_OFFICECODE');">
                    <asp:TextBox ID="WF_OFFICECODE" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </span>
                <asp:Label ID="WF_OFFICECODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>

            <!-- 荷主コード -->
            <div class="inputItem">
                <a id="WF_SHIPPERCODE_LABEL" >荷主コード</a>
                <span ondblclick="Field_DBclick('WF_SHIPPERCODE', <%=LIST_BOX_CLASSIFICATION.LC_JOINTLIST%>);" onchange="TextBox_change('WF_SHIPPERCODE');">
                    <asp:TextBox ID="WF_SHIPPERCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </span>
                <asp:Label ID="WF_SHIPPERCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>
            
            <!-- 基地コード -->
            <div class="inputItem">
                <a id="WF_PLANTCODE_LABEL" >基地コード</a>
                <span ondblclick="Field_DBclick('WF_PLANTCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_PLANTCODE');">
                    <asp:TextBox ID="WF_PLANTCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </span>
                <asp:Label ID="WF_PLANTCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>

            <!-- 油種大分類コード -->
            <div class="inputItem">
                <a id="WF_BIGOILCODE_LABEL" >油種大分類コード</a>
                <span ondblclick="Field_DBclick('WF_BIGOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_BIGOILCODE');">
                    <asp:TextBox ID="WF_BIGOILCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </span>
                <asp:Label ID="WF_BIGOILCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>
            
            <!-- 油種中分類コード -->
            <div class="inputItem">
                <a id="WF_MIDDLEOILCODE_LABEL" >油種中分類コード</a>
                <span ondblclick="Field_DBclick('WF_MIDDLEOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_MIDDLEOILCODE');">
                    <asp:TextBox ID="WF_MIDDLEOILCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </span>
                <asp:Label ID="WF_MIDDLEOILCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>
            
            <!-- 油種コード -->
            <div class="inputItem">
                <a id="WF_OILCODE_LABEL" >油種コード</a>
                <span ondblclick="Field_DBclick('WF_OILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OILCODE');">
                    <asp:TextBox ID="WF_OILCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </span>
                <asp:Label ID="WF_OILCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
            </div>

            <!-- 削除フラグ -->
            <div class="inputItem">
                <a id="WF_DELFLG_LABEL">検索条件</a>
                <span id="WF_DELFLG">
                    <asp:RadioButton ID="WF_DELFLG_NOTDELETED" runat="server" GroupName="WF_DELFLG" Text="削除除く" /><br/>
                    <asp:RadioButton ID="WF_DELFLG_DELETED" runat="server" GroupName="WF_DELFLG" Text="削除のみ" />
                </span>
            </div>
            
            <div class="inputItem">
                <a id="WF_DELFLG_ANNOTATION">※条件指定がない時は全件表示</a>
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

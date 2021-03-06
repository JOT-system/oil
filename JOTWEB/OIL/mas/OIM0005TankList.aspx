﻿<%@ Page Title="OIM0005L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0005TankList.aspx.vb" Inherits="JOTWEB.OIM0005TankList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0005LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIM0005L.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0005L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0005L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <!-- 一覧件数 -->
                        <asp:Label ID="WF_ListCNT" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span id="filter_LENGTHFLG" runat="server" style="margin-left:20px;">
                            <span class="ef">
                                <asp:Label ID="WF_FIL_LENGTHFLG_L" runat="server" Text="長さフラグ" CssClass="WF_TEXT_LABEL"></asp:Label>
                                <span ondblclick="Field_DBclick('WF_FIL_LENGTHFLG', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_FIL_LENGTHFLG');">
                                    <asp:TextBox ID="WF_FIL_LENGTHFLG" runat="server" MaxLength="1" Height="22px"  Width="50px"></asp:TextBox>
                                </span>
                                <asp:Label ID="WF_FIL_LENGTHFLG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </span>
                    </div>
                    <div class="rightSide">
                        <!-- ボタン -->
                        <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="追加"     onclick="ButtonClick('WF_ButtonINSERT');" />
                        <input type="button" id="WF_ButtonUPDATE" class="btn-sticky" value="DB更新"   onclick="ButtonClick('WF_ButtonUPDATE');" />
                        <input type="button" id="WF_ButtonCSV"    class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonCSV');" />
                        <input type="button" id="WF_ButtonPrint"  class="btn-sticky" value="一覧印刷" onclick="ButtonClick('WF_ButtonPrint');" />
                        <input type="button" id="WF_ButtonEND"    class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                        <div                 id="WF_ButtonFIRST"  class="firstPage"  runat="server"   onclick="ButtonClick('WF_ButtonFIRST');"></div>
                        <div                 id="WF_ButtonLAST"   class="lastPage"   runat="server"   onclick="ButtonClick('WF_ButtonLAST');"></div>
                    </div>
                </div> <!-- End class=actionButtonBox -->
            </div> <!-- End class="Operation" -->
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
        </div>

        <!-- 削除フラグ表示位置 -->
        <asp:HiddenField ID="WF_DELFLG_INDEX" runat="server" />

        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

        <!-- イベント用 -->
        <div style="display:none;">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
            <!-- GridView DBクリック-->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>
            <!-- GridView表示位置フィールド -->

            <input id="WF_FIELD" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD_REP" runat="server" value="" type="text" />
            <!-- Textbox(Repeater) DBクリックフィールド -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->

            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
            <!-- LeftBox Mview切替 -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
            <!-- LeftBox 開閉 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text" />
            <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value="" type="text" />
            <!-- Rightbox 開閉 -->

            <input id="WF_PrintURL" runat="server" value="" type="text" />
            <!-- Textbox Print URL -->

            <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />
            <!-- 一覧・詳細画面切替用フラグ -->

            <input id="WF_ButtonClick" runat="server" value="" type="text" />
            <!-- ボタン押下 -->
            <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
            <!-- 権限 -->
        </div>
 
</asp:Content>

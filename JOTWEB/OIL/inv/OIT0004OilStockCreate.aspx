﻿<%@ Page Title="OIT0004C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0004OilStockCreate.aspx.vb" Inherits="JOTWEB.OIT0004OilStockCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0004WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0004CH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0004C.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0004C.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIT0004L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">

                        <a >在庫維持日数</a>
                        <a class="ef">
                            <asp:TextBox ID="WF_INVENTORYDAYS" runat="server"onblur="MsgClear();"></asp:TextBox>
                        </a>

                        <!-- ボタン -->
                        <input type="button" id="WF_ButtonAUTOSUGGESTION" class="btn-sticky" value="自動提案"     onclick="ButtonClick('WF_ButtonAUTOSUGGESTION');" />
                        <input type="button" id="WF_ButtonORDERLIST"      class="btn-sticky" value="受注作成"     onclick="ButtonClick('WF_ButtonORDERLIST');" />
                        <input type="button" id="WF_ButtonINPUTCLEAR"     class="btn-sticky" value="入力値クリア" onclick="ButtonClick('WF_ButtonINPUTCLEAR');" />
                    </div>

                    <div class="rightSide">
                        <input type="button" id="WF_ButtonUPDATE"        class="btn-sticky" value="更新"     onclick="ButtonClick('WF_ButtonUPDATE');" />
                        <input type="button" id="WF_ButtonCSV"           class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonCSV');" />
                        <input type="button" id="WF_ButtonINSERT"        class="btn-sticky" value="新規登録" onclick="ButtonClick('WF_ButtonINSERT');" />
                        <input type="button" id="WF_ButtonEND"           class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                        <div                 id="WF_ButtonFIRST"         class="firstPage"  runat="server"   onclick="ButtonClick('WF_ButtonFIRST');"></div>
                        <div                 id="WF_ButtonLAST"          class="lastPage"   runat="server"   onclick="ButtonClick('WF_ButtonLAST');"></div>
                    </div>
                </div> <!-- End class=actionButtonBox -->
            </div> <!-- End class="Operation" -->
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
        </div>

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

<%@ Page Title="OIT0005C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0005TankLocCondition.aspx.vb" Inherits="JOTWEB.OIT0005TankLocCondition" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>
<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0005CH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0005C.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0005C.js")%>'></script>
</asp:Content>
<asp:Content ID="OIT0005C" ContentPlaceHolderID="contents1" runat="server">
        <div class="headerboxOnly" id="headerbox">
            <div class="actionButtonBox">
                <div class="leftSide">
                </div>
                <div class="rightSide">
                    <input type="button" id="WF_ButtonEND"           class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                    <div                 id="WF_ButtonFIRST"         class="firstPage"  runat="server"   visible="false" onclick="ButtonClick('WF_ButtonFIRST');"></div>
                    <div                 id="WF_ButtonLAST"          class="lastPage"   runat="server"   visible="false" onclick="ButtonClick('WF_ButtonLAST');"></div>
                </div>
            </div> <!-- End class=actionButtonBox -->
            <div id="divConditionArea">
                <asp:Repeater ID="repCondition" runat="server" ClientIDMode="Predictable">
                    <HeaderTemplate >
                        <div class="conditionWrapper">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <div class="conditionItem">
                            <div class="conditionName"><span><%# Eval("ConditionName") %></span></div>
                            <div class="valueName"><span><%# Eval("Value1Name") %></span></div>
                            <div class="value"><span><%# CDec(Eval("Value1")).ToString("#,##0 両") %></span></div>
                            <div class="valueName"><span><%# Eval("Value2Name") %></span></div>
                            <div class="value"><span><%# CDec(Eval("Value2")).ToString("#,##0 両") %></span></div>
                            <div class="button">
                            <input id="btnShowList" type="button" value="内訳を見る" class="btn-sticky" runat="server" onclick="ButtonClick('WF_ButtonShowList');" />
                            </div>
                        </div>
                    </ItemTemplate>
                    <FooterTemplate >
                        </div>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div> <!-- End class=headerboxOnly -->
    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

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

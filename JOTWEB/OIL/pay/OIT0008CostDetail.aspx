<%@ Page Title="OIT0008D" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0008CostDetail.aspx.vb" Inherits="JOTWEB.OIT0008CostDetail" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0008WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0008DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0008D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0008D.js")%>'></script>
    <script type="text/javascript">
<%--        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';--%>
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIT0008D" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerbox" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <!-- 一覧件数 -->
                        <asp:Label ID="WF_ListCNT" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </div>
                    <div class="rightSide">
                    <!-- ボタン -->
                        <input type="button" id="WF_ButtonCSV"    class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonCSV');" />
                        <input type="button" id="WF_ButtonPrint"  class="btn-sticky" value="一覧印刷" onclick="ButtonClick('WF_ButtonPrint');" />
                        <input type="button" id="WF_ButtonEND"    class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                    </div>
                </div> <!-- End class=actionButtonBox -->
            </div> <!-- End class="Operation" -->
            <div id="headerDispArea">
                <asp:Panel ID="pnlHeader" CssClass="headerDetail" runat="server">
                    <!-- ■　行番号　■ -->
                    <span class="left">
                        <a id="WF_LINE_LABEL">#</a>
                        <a class="ef" id="WF_LINE">
                            <asp:TextBox ID="TxtLine" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span></span><span></span><span></span><span></span>

                    <!-- ■　勘定科目コード　■ -->
                    <span class="left">
                        <a id="WF_ACCOUNTCODE_LABEL">勘定科目コード</a>
                        <a class="ef" id="WF_ACCOUNTCODE">
                            <asp:TextBox ID="TxtAccountCode" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_ACCOUNTNAME_LABEL">勘定科目名</a>
                        <a class="ef" id="WF_ACCOUNTNAME">
                            <asp:TextBox ID="TxtAccountName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span></span><span></span>

                    <!-- ■　セグメント　■ -->
                    <span class="left">
                        <a id="WF_SEGMENTCODE_LABEL">セグメント</a>
                        <a class="ef" id="WF_SEGMENTCODE">
                            <asp:TextBox ID="TxtSegmentCode" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_SEGMENTNAME_LABEL">セグメント名</a>
                        <a class="ef" id="WF_SEGMENTNAME">
                            <asp:TextBox ID="TxtSegmentName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span></span><span></span>

                    <!-- ■　セグメント枝番　■ -->
                    <span class="left">
                        <a id="WF_SEGMENTBRANCHCODE_LABEL">セグメント枝番</a>
                        <a class="ef" id="WF_SEGMENTBRANCHCODE">
                            <asp:TextBox ID="TxtSegmentBranchCode" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_SEGMENTBRANCHNAME_LABEL">セグメント枝番名</a>
                        <a class="ef" id="WF_SEGMENTBRANCHNAME">
                            <asp:TextBox ID="TxtSegmentBranchName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span></span><span></span>

                    <!-- ■　荷主　■ -->
                    <span class="left">
                        <a id="WF_SHIPPERSCODE_LABEL">荷主コード</a>
                        <a class="ef" id="WF_SHIPPERSCODE">
                            <asp:TextBox ID="TxtShippersCode" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_SHIPPERSNAME_LABEL">荷主名</a>
                        <a class="ef" id="WF_SHIPPERSNAME">
                            <asp:TextBox ID="TxtShippersName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span></span><span></span>

                    <!-- ■　請求先コード　■ -->
                    <span class="left">
                        <a id="WF_INVOICECODE_LABEL">請求先コード</a>
                        <a class="ef" id="WF_INVOICECODE">
                            <asp:TextBox ID="TxtInvoiceCode" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_INVOICENAME_LABEL">請求先名</a>
                        <a class="ef" id="WF_INVOICENAME">
                            <asp:TextBox ID="TxtInvoiceName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_INVOICEDEPTNAME_LABEL">請求先部門</a>
                        <a class="ef" id="WF_INVOICEDEPTNAME">
                            <asp:TextBox ID="TxtInvoiceDeptName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
 
                    <!-- ■　支払先コード　■ -->
                    <span class="left">
                        <a id="WF_PAYEECODE_LABEL">支払先コード</a>
                        <a class="ef" id="WF_PAYEECODE">
                            <asp:TextBox ID="TxtPayeeCode" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_PAYEENAME_LABEL">支払先名</a>
                        <a class="ef" id="WF_PAYEENAME">
                            <asp:TextBox ID="TxtPayeeName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_PAYEEDEPTNAME_LABEL">支払先部門</a>
                        <a class="ef" id="WF_PAYEEDEPTNAME">
                            <asp:TextBox ID="TxtPayeeDeptName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </span>
                </asp:Panel>
            </div>
        </div>
        <div class="detailbox" id="detailbox">
            <asp:GridView ID="WF_CONSIGNEELIST" runat="server" AllowPaging="false" AutoGenerateColumns="false" ShowHeader="true" ShowHeaderWhenEmpty="true" Visible="true" ShowFooter="true" CssClass="" GridLines="None" BorderWidth="0">
                <Columns>
                    <asp:TemplateField HeaderStyle-CssClass="AllBorder" ItemStyle-CssClass="CONSIGNEENAME codeText NoneTopAndBottomBorder" FooterStyle-CssClass="NoneRightBorder">
                        <HeaderTemplate>荷受人</HeaderTemplate>
                        <ItemTemplate>
                            <asp:HiddenField ID="WF_NINUKELIST_CONSIGNEECODE" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "CONSIGNEECODE")%>' />
                            <asp:Label ID="WF_NINUKELIST_CONSIGNEENAME" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "CONSIGNEENAME")%>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="AllBorder" ItemStyle-CssClass="OILNAME codeText AllBorder" FooterStyle-CssClass="footerText NoneLeftBorder">
                        <HeaderTemplate>油種</HeaderTemplate>
                        <ItemTemplate>
                            <asp:HiddenField ID="WF_NINUKELIST_OILCODE" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "OILCODE")%>' />
                            <asp:Label ID="WF_NINUKELIST_OILNAME" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "OILNAME")%>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="AllBorder" ItemStyle-CssClass="AMOUNT moneyText NoneRightBorder" FooterStyle-CssClass="moneyText AllBorder">
                        <HeaderTemplate>数量</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="WF_NINUKELIST_QUANTITY" runat="server" Text='<%# String.Format("{0:#,##0.000}", DataBinder.Eval(Container.DataItem, "QUANTITY"))%>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="AllBorder" ItemStyle-CssClass="AMOUNT moneyText NoneRightBorder" FooterStyle-CssClass="moneyText AllBorder">
                        <HeaderTemplate>車数</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="WF_NINUKELIST_CARSNUMBER" runat="server" Text='<%# String.Format("{0:#,##0}", DataBinder.Eval(Container.DataItem, "CARSNUMBER"))%>' />
                        </ItemTemplate>
                        <FooterTemplate>
                            <span class="footerColName">合計額</span>
                        </FooterTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="AllBorder" ItemStyle-CssClass="AMOUNT moneyText NoneRightBorder" FooterStyle-CssClass="moneyText AllBorder">
                        <HeaderTemplate>請求額</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="WF_NINUKELIST_AMOUNT" runat="server" Text='<%# String.Format("{0:#,##0}", DataBinder.Eval(Container.DataItem, "AMOUNT"))%>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="AllBorder" ItemStyle-CssClass="TAX moneyText NoneRightBorder" FooterStyle-CssClass="moneyText AllBorder">
                        <HeaderTemplate>税額</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="WF_NINUKELIST_TAX" runat="server" Text='<%# String.Format("{0:#,##0}", DataBinder.Eval(Container.DataItem, "TAX")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="AllBorder" ItemStyle-CssClass="TOTAL moneyText AllBorder" FooterStyle-CssClass="moneyText AllBorder">
                        <HeaderTemplate>請求額合計</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="WF_NINUKELIST_TOTAL" runat="server" Text='<%# String.Format("{0:#,##0}", DataBinder.Eval(Container.DataItem, "TOTAL")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
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

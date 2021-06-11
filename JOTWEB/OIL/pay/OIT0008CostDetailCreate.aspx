<%@ Page Title="OIT0008D" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0008CostDetailCreate.aspx.vb" Inherits="JOTWEB.OIT0008CostDetailCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0008WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0008CH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0008C.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0008C.js")%>'></script>
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
                        <div class="costHeaderBox">
                            <!-- 営業所 -->
                            <p id="WF_OFFICENAME_LABEL">
                                <asp:Label ID="WF_OFFICENAME" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                            </p>
                            <!-- 計上年月 -->
                            <p id="WF_KEIJYOYM_LABEL">
                                <asp:Label ID="WF_KEIJYOYM" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                            </p>
                        </div>
                    </div>
                    <div class="rightSide">
                    <!-- ボタン -->
                        <input type="button" id="WF_ButtonUPATE"  class="btn-sticky" value="更新"     onclick="ButtonClick('WF_ButtonUPDATE');" />
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
                            <span ondblclick="Field_DBclick('TxtAccountCode', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE %>)">
                                <asp:TextBox ID="TxtAccountCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" onblur="MsgClear();" ReadOnly="true"></asp:TextBox>
                            </span>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_ACCOUNTNAME_LABEL">勘定科目名</a>
                        <a class="ef" id="WF_ACCOUNTNAME">
                            <asp:TextBox ID="TxtAccountName" runat="server" onblur="MsgClear();" ReadOnly="true"></asp:TextBox>
                        </a>
                    </span>
                    <span></span><span></span>

                    <!-- ■　セグメント　■ -->
                    <span class="left">
                        <a id="WF_SEGMENTCODE_LABEL">セグメント</a>
                        <a class="ef" id="WF_SEGMENTCODE">
                            <asp:TextBox ID="TxtSegmentCode" runat="server" onblur="MsgClear();" ReadOnly="true"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_SEGMENTNAME_LABEL">セグメント名</a>
                        <a class="ef" id="WF_SEGMENTNAME">
                            <asp:TextBox ID="TxtSegmentName" runat="server" onblur="MsgClear();" ReadOnly="true"></asp:TextBox>
                        </a>
                    </span>
                    <span></span><span></span>

                    <!-- ■　セグメント枝番　■ -->
                    <span class="left">
                        <a id="WF_SEGMENTBRANCHCODE_LABEL">セグメント枝番</a>
                        <a class="ef" id="WF_SEGMENTBRANCHCODE">
                            <asp:TextBox ID="TxtSegmentBranchCode" runat="server" onblur="MsgClear();" ReadOnly="true"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_SEGMENTBRANCHNAME_LABEL">セグメント枝番名</a>
                        <a class="ef" id="WF_SEGMENTBRANCHNAME">
                            <asp:TextBox ID="TxtSegmentBranchName" runat="server" onblur="MsgClear();" ReadOnly="true"></asp:TextBox>
                        </a>
                    </span>
                    <span>
                        <asp:HiddenField ID="HdnTaxKbn" runat="server" Value="" />
                    </span><span></span>

                    <!-- ■　荷主　■ -->
                    <span class="left">
                        <a id="WF_SHIPPERSCODE_LABEL">荷主コード</a>
                        <a class="ef" id="WF_SHIPPERSCODE">
                            <span ondblclick="Field_DBclick('TxtShippersCode', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPERSLIST %>)">
                                <asp:TextBox ID="TxtShippersCode" runat="server" onblur="MsgClear();" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" ></asp:TextBox>
                            </span>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_SHIPPERSNAME_LABEL">荷主名</a>
                        <a class="ef" id="WF_SHIPPERSNAME">
                            <asp:TextBox ID="TxtShippersName" runat="server" onblur="MsgClear();" ReadOnly="true"></asp:TextBox>
                        </a>
                    </span>
                    <span></span><span></span>

                    <!-- ■　請求先コード　■ -->
                    <span class="left">
                        <a id="WF_INVOICECODE_LABEL">請求先コード</a>
                        <a class="ef" id="WF_INVOICECODE">
                            <span ondblclick="Field_DBclick('TxtInvoiceCode', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtInvoiceCode');">
                                <asp:TextBox ID="TxtInvoiceCode" runat="server" onblur="MsgClear();" CssClass="WF_TEXTBOX_CSS boxIcon"></asp:TextBox>
                            </span>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_INVOICENAME_LABEL">請求先名</a>
                        <a class="ef" id="WF_INVOICENAME">
                            <asp:TextBox ID="TxtInvoiceName" runat="server" onblur="MsgClear();"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_INVOICEDEPTNAME_LABEL">請求先部門</a>
                        <a class="ef" id="WF_INVOICEDEPTNAME">
                            <asp:TextBox ID="TxtInvoiceDeptName" runat="server" onblur="MsgClear();"></asp:TextBox>
                        </a>
                    </span>
 
                    <!-- ■　支払先コード　■ -->
                    <span class="left">
                        <a id="WF_PAYEECODE_LABEL">支払先コード</a>
                        <a class="ef" id="WF_PAYEECODE">
                            <span ondblclick="Field_DBclick('TxtPayeeCode', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtPayeeCode');">
                                <asp:TextBox ID="TxtPayeeCode" runat="server" onblur="MsgClear();" CssClass="WF_TEXTBOX_CSS boxIcon"></asp:TextBox>
                            </span>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_PAYEENAME_LABEL">支払先名</a>
                        <a class="ef" id="WF_PAYEENAME">
                            <asp:TextBox ID="TxtPayeeName" runat="server" onblur="MsgClear();"></asp:TextBox>
                        </a>
                    </span>
                    <span class="doubleItem">
                        <a id="WF_PAYEEDEPTNAME_LABEL">支払先部門</a>
                        <a class="ef" id="WF_PAYEEDEPTNAME">
                            <asp:TextBox ID="TxtPayeeDeptName" runat="server" onblur="MsgClear();"></asp:TextBox>
                        </a>
                    </span>

                    <!-- ■　摘要　■ -->
                    <span class="full">
                        <a id="WF_TEKIYOU_LABEL">摘要</a>
                        <a class="ef" id="WF_TEKIYOU">
                            <asp:TextBox ID="TxtTekiyou" runat="server" onblur="MsgClear();"></asp:TextBox>
                        </a>
                    </span>
                </asp:Panel>
            </div>
        </div>
        <div class="detailbox" id="detailbox">
            <div class="actionButtonBox" id="RowOperationBox">
                <div class="leftSide">
                    <asp:Button ID="WF_ALLSELECT" runat="server" CssClass="btn-sticky" Text="全選択" OnClientClick="selectAll(true); return false;"></asp:Button>
                    <asp:Button ID="WF_ALLRELEACE" runat="server" CssClass="btn-sticky" Text="選択解除" OnClientClick="selectAll(false); return false;"></asp:Button>
                    <asp:Button ID="WF_DELETEROW" runat="server" CssClass="btn-sticky" Text="行削除" OnClientClick="ButtonClick('WF_ButtonDELETEROW');"></asp:Button>
                    <asp:Button ID="WF_ADDROW" runat="server" CssClass="btn-sticky" Text="行追加" OnClientClick="ButtonClick('WF_ButtonADDROW');"></asp:Button>
                </div>
            </div>
            <asp:GridView ID="WF_COSTDETAILTBL" runat="server" AllowPaging="false" AutoGenerateColumns="false" ShowHeader="true" ShowHeaderWhenEmpty="true" Visible="true" ShowFooter="false">
                <Columns>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH1" ItemStyle-CssClass="WF_COSTDETAILTBL_TD1">
                        <HeaderTemplate>#</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="WF_COSTDETAILTBL_DETAILNO" runat="server" Text='<%# String.Format("{0,3:#0}", DataBinder.Eval(Container.DataItem, "DETAILNO"))%>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH2" ItemStyle-CssClass="WF_COSTDETAILTBL_TD2">
                        <HeaderTemplate>選択</HeaderTemplate>
                        <ItemTemplate>
                            <!-- チェックボックス -->
                            <span class="WF_COSTDETAILTBL_CHECKFLG">
                                <asp:CheckBox ID="WF_COSTDETAILTBL_CHECKFLG" runat="server" Enabled='True' Checked='False' />
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH3" ItemStyle-CssClass="WF_COSTDETAILTBL_TD3">
                        <HeaderTemplate>輸送形態区分</HeaderTemplate>
                        <ItemTemplate>
                            <asp:DropDownList ID="WF_COSTDETAILTBL_TRKBNLIST" runat="server" />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_TRKBN" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "TRKBN")%>' />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_TRKBNNAME" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "TRKBNNAME")%>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH4" ItemStyle-CssClass="WF_COSTDETAILTBL_TD4">
                        <HeaderTemplate>計上営業所</HeaderTemplate>
                        <ItemTemplate>
                            <asp:DropDownList ID="WF_COSTDETAILTBL_POSTOFFICENAMELIST" runat="server" />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_POSTOFFICECODE" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "POSTOFFICECODE")%>' />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_POSTOFFICENAME" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "POSTOFFICENAME")%>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH5" ItemStyle-CssClass="WF_COSTDETAILTBL_TD5">
                        <HeaderTemplate>荷受人</HeaderTemplate>
                        <ItemTemplate>
                            <asp:DropDownList ID="WF_COSTDETAILTBL_CONSIGNEENAMELIST" runat="server" />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_CONSIGNEECODE" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "CONSIGNEECODE")%>' />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_CONSIGNEENAME" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "CONSIGNEENAME")%>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH10" ItemStyle-CssClass="WF_COSTDETAILTBL_TD10" FooterStyle-CssClass="WF_COSTDETAILTBL_TF10">
                        <HeaderTemplate>金額</HeaderTemplate>
                        <ItemTemplate>
                            <asp:TextBox ID="WF_COSTDETAILTBL_AMOUNT" runat="server" placeholder="0" Value='<%# String.Format("{0:#,##0}", DataBinder.Eval(Container.DataItem, "AMOUNT")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH11" ItemStyle-CssClass="WF_COSTDETAILTBL_TD11" FooterStyle-CssClass="WF_COSTDETAILTBL_TF11">
                        <HeaderTemplate>税額</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="WF_COSTDETAILTBL_TAX" runat="server" Text='<%# String.Format("{0:#,##0}", DataBinder.Eval(Container.DataItem, "TAX")) %>' />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_CONSUMPTIONTAX" runat="server" Value='<%# String.Format("{0:0.00}", DataBinder.Eval(Container.DataItem, "CONSUMPTIONTAX")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH12" ItemStyle-CssClass="WF_COSTDETAILTBL_TD12" FooterStyle-CssClass="WF_COSTDETAILTBL_TF12">
                        <HeaderTemplate>総額</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Label ID="WF_COSTDETAILTBL_TOTAL" runat="server" Text='<%# String.Format("{0:#,##0}", DataBinder.Eval(Container.DataItem, "TOTAL")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH13" ItemStyle-CssClass="WF_COSTDETAILTBL_TD13" FooterStyle-CssClass="WF_COSTDETAILTBL_TF13">
                        <HeaderTemplate>摘要</HeaderTemplate>
                        <ItemTemplate>
                            <asp:TextBox ID="WF_COSTDETAILTBL_TEKIYOU" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "TEKIYOU") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
<!--
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH5" ItemStyle-CssClass="WF_COSTDETAILTBL_TD5">
                        <HeaderTemplate>油種</HeaderTemplate>
                        <ItemTemplate>
                            <asp:DropDownList ID="WF_COSTDETAILTBL_ORDERINGOILNAMELIST" runat="server" />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_OILCODE" runat="server" Value='<!--%# DataBinder.Eval(Container.DataItem, "OILCODE")%' />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_OILNAME" runat="server" Value='<!--%# DataBinder.Eval(Container.DataItem, "OILNAME")%>' />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_ORDERINGTYPE" runat="server" Value='<!--%# DataBinder.Eval(Container.DataItem, "ORDERINGTYPE")%>' />
                            <asp:HiddenField ID="WF_COSTDETAILTBL_ORDERINGOILNAME" runat="server" Value='<!--%# DataBinder.Eval(Container.DataItem, "ORDERINGOILNAME")%>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH6" ItemStyle-CssClass="WF_COSTDETAILTBL_TD6" FooterStyle-CssClass="WF_COSTDETAILTBL_TF6">
                        <HeaderTemplate>数量</HeaderTemplate>
                        <ItemTemplate>
                            <asp:TextBox ID="WF_COSTDETAILTBL_CARSAMOUNT" runat="server" placeholder="0.000" Value='<!--%# String.Format("{0:#,##0.000}", DataBinder.Eval(Container.DataItem, "CARSAMOUNT")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH7" ItemStyle-CssClass="WF_COSTDETAILTBL_TD7" FooterStyle-CssClass="WF_COSTDETAILTBL_TF7">
                        <HeaderTemplate>車数</HeaderTemplate>
                        <ItemTemplate>
                            <asp:TextBox ID="WF_COSTDETAILTBL_CARSNUMBER" runat="server" placeholder="0" Value='<!--%# String.Format("{0:#,##0}", DataBinder.Eval(Container.DataItem, "CARSNUMBER")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH8" ItemStyle-CssClass="WF_COSTDETAILTBL_TD8" FooterStyle-CssClass="WF_COSTDETAILTBL_TF8">
                        <HeaderTemplate>屯数</HeaderTemplate>
                        <ItemTemplate>
                            <asp:TextBox ID="WF_COSTDETAILTBL_LOADAMOUNT" runat="server" placeholder="0" Value='<!--%# String.Format("{0:#,##0}", DataBinder.Eval(Container.DataItem, "LOADAMOUNT")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderStyle-CssClass="WF_COSTDETAILTBL_TH9" ItemStyle-CssClass="WF_COSTDETAILTBL_TD9" FooterStyle-CssClass="WF_COSTDETAILTBL_TF9">
                        <HeaderTemplate>単価</HeaderTemplate>
                        <ItemTemplate>
                            <asp:TextBox ID="WF_COSTDETAILTBL_UNITPRICE" runat="server" placeholder="0.00" Value='<!--%# String.Format("{0:#,##0.00}", DataBinder.Eval(Container.DataItem, "UNITPRICE")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
-->
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

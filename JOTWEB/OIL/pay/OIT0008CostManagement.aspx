<%@ Page Title="OIT0008M" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0008CostManagement.aspx.vb" Inherits="JOTWEB.OIT0008CostManagement" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0008WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>


<asp:Content ID="OIT0008MH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0008M.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0008M.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIT0008M" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerbox" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <asp:Label ID="WF_OFFICE_L" runat="server" CssClass="WF_TEXT_LEFT" Text="【表示する営業所】"></asp:Label>
                    </div>
                    <div class="rightSide">
                    <!-- ボタン -->
                        <input type="button" id="WF_ButtonEND"    class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                    </div>
                </div> <!-- End class=actionButtonBox -->
            </div> <!-- End class="Operation" -->
        </div>
        <div class="detailbox" id="detailbox">
            <div class="actionButtonBox" id="OfficeSelection">
                <asp:HiddenField ID="WF_OFFICEHDN_ID" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_1" runat="server" CssClass="btn-office" Text="営業所1" />
                <asp:HiddenField ID="WF_OFFICEHDN_1" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_2" runat="server" CssClass="btn-office" Text="営業所2" />
                <asp:HiddenField ID="WF_OFFICEHDN_2" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_3" runat="server" CssClass="btn-office" Text="営業所3" />
                <asp:HiddenField ID="WF_OFFICEHDN_3" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_4" runat="server" CssClass="btn-office" Text="営業所4" />
                <asp:HiddenField ID="WF_OFFICEHDN_4" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_5" runat="server" CssClass="btn-office" Text="営業所5" />
                <asp:HiddenField ID="WF_OFFICEHDN_5" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_6" runat="server" CssClass="btn-office" Text="営業所6" />
                <asp:HiddenField ID="WF_OFFICEHDN_6" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_7" runat="server" CssClass="btn-office" Text="営業所7" />
                <asp:HiddenField ID="WF_OFFICEHDN_7" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_8" runat="server" CssClass="btn-office"  Text="営業所8" />
                <asp:HiddenField ID="WF_OFFICEHDN_8" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_9" runat="server" CssClass="btn-office"  Text="営業所9" />
                <asp:HiddenField ID="WF_OFFICEHDN_9" runat="server" Value="" />
                <asp:Button ID="WF_OFFICEBTN_10" runat="server" CssClass="btn-office last" Text="営業所10" />
                <asp:HiddenField ID="WF_OFFICEHDN_10" runat="server" Value="" />
            </div>
            <div class="detail_keijoym">
                <asp:Label ID="WF_KEIJOYM_L" runat="server" CssClass="WF_TEXT_CENTER" Text="【計上年月】" />
                <span ondblclick="Field_DBclick('WF_KEIJOYM', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);" onchange="TextBox_change('WF_KEIJOYM');">
                    <asp:TextBox ID="WF_KEIJOYM" runat="server" CssClass="calendarIcon"></asp:TextBox>
                </span>
                <asp:Button ID="WF_RELOAD_BTN" runat="server" CssClass="btn-sticky" OnClientClick="OfficeButtonClick();" Text="表示する"></asp:Button>
                <asp:Label ID="WF_MEMO_L" runat="server" CssClass="WF_TEXT_RIGHT" Text="メモ" />
                <asp:TextBox ID="WF_MEMO" runat="server" TextMode="MultiLine" CssClass="memo_text" Rows="3"></asp:TextBox>
            </div>
            <div class="actionButtonBox" id="RowOperationBox">
                <div class="leftSide">
                    <input type="button" name="WF_ALLSELECT" class="btn-sticky" value="全選択" onclick="selectAll(true);" />
                    <input type="button" name="WF_ALLRELEACE" class="btn-sticky" value="選択解除" onclick="selectAll(false);" />
                    <asp:Button ID="WF_DELETEROW" runat="server" CssClass="btn-sticky" Text="行削除" OnClientClick="ButtonClick('WF_ButtonDELETEROW');"></asp:Button>
                    <asp:Button ID="WF_ADDROW" runat="server" CssClass="btn-sticky" Text="行追加" OnClientClick="ButtonClick('WF_ButtonADDROW');"></asp:Button>
                </div>
                <div class="rightSide">
                    <asp:Button ID="WF_UPDATE" runat="server" CssClass="btn-sticky" Text="保存する" OnClientClick="ButtonClick('WF_ButtonUPDATE');"></asp:Button>
                </div>
            </div>
            <div class="detail_costlist">
                <asp:GridView ID="WF_COSTLISTTBL" runat="server" AllowPaging="false" AutoGenerateColumns="false" ShowHeader="true" ShowHeaderWhenEmpty="true" Visible="true" ShowFooter="true" CssClass="WF_COSTLISTTBL">
                    <Columns>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH1" ItemStyle-CssClass="WF_COSTLISTTBL_TD1">
                            <HeaderTemplate>#</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="WF_COSTLISTTBL_LINECNT" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "LINECNT")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH2" ItemStyle-CssClass="WF_COSTLISTTBL_TD2">
                            <HeaderTemplate>選択</HeaderTemplate>
                            <ItemTemplate>
                                <!-- チェックボックス -->
                                <asp:CheckBox ID="WF_COSTLISTTBL_CHECK" runat="server" Checked='<%# GetCheckVal(DataBinder.Eval(Container.DataItem, "CHECK")) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH3" ItemStyle-CssClass="WF_COSTLISTTBL_TD3" ControlStyle-CssClass="btn-detail" >
                            <HeaderTemplate>確認</HeaderTemplate>
                            <ItemTemplate>
                                <!-- 詳細ボタン -->
                                <asp:Button ID="WF_COSTLISTTBL_DETAIL" runat="server" Text="明細を見る" Enabled='<%# GetCheckVal(DataBinder.Eval(Container.DataItem, "DETAIL")) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH4" ItemStyle-CssClass="WF_COSTLISTTBL_TD4" ControlStyle-CssClass="WF_TEXTBOX_CSS boxIcon">
                            <HeaderTemplate>勘定科目</HeaderTemplate>
                            <ItemTemplate>
                                <span ondblclick="Field_DBclick('WF_COSTLISTTBL_ACCOUNTCODE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>', <%=LIST_BOX_CLASSIFICATION.LC_ACCOUNTLIST%>)" onchange="TextBox_change('WF_COSTLISTTBL_ACCOUNTCODE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>');">
                                    <!-- 勘定科目コード OIM0010_PATTERN.ACCOUNTCODE -->
                                    <asp:TextBox ID="WF_COSTLISTTBL_ACCOUNTCODE" runat="server" ReadOnly='<%# GetCheckVal(DataBinder.Eval(Container.DataItem, "DETAIL")) %>' Text='<%# Bind("ACCOUNTCODE")%>' MaxLength="10" />
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH5" ItemStyle-CssClass="WF_COSTLISTTBL_TD5" ControlStyle-CssClass="WF_TEXTBOX_CSS boxIcon">
                            <HeaderTemplate>セグメント</HeaderTemplate>
                            <ItemTemplate>
                                <!-- セグメント OIM0010_PATTERN.SEGMENTCODE -->
                                <asp:Label ID="WF_COSTLISTTBL_SEGMENTCODE" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "SEGMENTCODE") %>' />
                                <!-- セグメント枝番 OIM0010_PATTERN.SEGMENTBRANCHCODE -->
                                <asp:HiddenField ID="WF_COSTLISTTBL_SEGMENTBRANCHCODE" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "SEGMENTBRANCHCODE") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH6" ItemStyle-CssClass="WF_COSTLISTTBL_TD6" ControlStyle-CssClass="WF_TEXTBOX_CSS">
                            <HeaderTemplate>金額</HeaderTemplate>
                            <ItemTemplate>
                                <asp:TextBox ID="WF_COSTLISTTBL_AMMOUNT" runat="server" ReadOnly='<%# GetCheckVal(DataBinder.Eval(Container.DataItem, "DETAIL")) %>' Text='<%# Bind("AMMOUNT")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH7" ItemStyle-CssClass="WF_COSTLISTTBL_TD7" ControlStyle-CssClass="WF_TEXTBOX_CSS">
                            <HeaderTemplate>税額</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="WF_COSTLISTTBL_TAX" runat="server" Text='<%# Bind("TAX")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH8" ItemStyle-CssClass="WF_COSTLISTTBL_TD8" ControlStyle-CssClass="WF_TEXTBOX_CSS boxIcon">
                            <HeaderTemplate>請求先コード</HeaderTemplate>
                            <ItemTemplate>
                                <span ondblclick="Field_DBclick('WF_COSTLISTTBL_INVOICECODE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>', <%=LIST_BOX_CLASSIFICATION.LC_TORILIST%>)" onchange="TextBox_change('WF_COSTLISTTBL_INVOICECODE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>');">
                                    <asp:TextBox ID="WF_COSTLISTTBL_INVOICECODE" runat="server" ReadOnly='<%# GetCheckVal(DataBinder.Eval(Container.DataItem, "DETAIL")) %>' Text='<%# Bind("INVOICECODE")%>' />
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH9" ItemStyle-CssClass="WF_COSTLISTTBL_TD9" ControlStyle-CssClass="WF_TEXTBOX_CSS">
                            <HeaderTemplate>請求先名</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="WF_COSTLISTTBL_INVOICENAME" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "INVOICENAME")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH10" ItemStyle-CssClass="WF_COSTLISTTBL_TD10" ControlStyle-CssClass="WF_TEXTBOX_CSS">
                            <HeaderTemplate>請求先部門</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="WF_COSTLISTTBL_INVOICEDEPT" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "INVOICEDEPT")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH11" ItemStyle-CssClass="WF_COSTLISTTBL_TD11" ControlStyle-CssClass="WF_TEXTBOX_CSS boxIcon">
                            <HeaderTemplate>支払先コード</HeaderTemplate>
                            <ItemTemplate>
                                <span ondblclick="Field_DBclick('WF_COSTLISTTBL_PAYEECODE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>', <%=LIST_BOX_CLASSIFICATION.LC_TORILIST%>)" onchange="TextBox_change('WF_COSTLISTTBL_PAYEECODE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>');">
                                    <asp:TextBox ID="WF_COSTLISTTBL_PAYEECODE" runat="server" ReadOnly='<%# GetCheckVal(DataBinder.Eval(Container.DataItem, "DETAIL")) %>' Text='<%# Bind("PAYEECODE")%>' />
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH12" ItemStyle-CssClass="WF_COSTLISTTBL_TD12" ControlStyle-CssClass="WF_TEXTBOX_CSS">
                            <HeaderTemplate>支払先名</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="WF_COSTLISTTBL_PAYEENAME" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "PAYEENAME")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH13" ItemStyle-CssClass="WF_COSTLISTTBL_TD13" ControlStyle-CssClass="WF_TEXTBOX_CSS">
                            <HeaderTemplate>支払先部門</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="WF_COSTLISTTBL_PAYEEDEPT" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "PAYEEDEPT")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-CssClass="WF_COSTLISTTBL_TH14" ItemStyle-CssClass="WF_COSTLISTTBL_TD14" ControlStyle-CssClass="WF_TEXTBOX_CSS">
                            <HeaderTemplate>摘要</HeaderTemplate>
                            <ItemTemplate>
                                <asp:TextBox ID="WF_COSTLISTTBL_ABSTRACT" runat="server" Text='<%# Bind("ABSTRACT")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
        <div class="footerbox" id="footerbox">
            <!-- ワークフロー -->
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

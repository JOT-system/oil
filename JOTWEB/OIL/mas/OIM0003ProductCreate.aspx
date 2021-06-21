<%@ Page Title="OIM0003C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0003ProductCreate.aspx.vb" Inherits="JOTWEB.OIM0003ProductCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0003CH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIM0003C.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0003C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0003C" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_CLEAR');" />
                    </div>
                </div>
            </div>

            <div id="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- 選択No -->
                    <span>
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_2">
                    <!-- 削除フラグ -->
                    <span class="ef">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除フラグ" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('WF_DELFLG');">
                            <asp:TextBox ID="WF_DELFLG" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 営業所コード -->
                    <span class="ef">
                        <asp:Label ID="WF_OFFICECODE_L" runat="server" Text="営業所コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OFFICECODE', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('WF_OFFICECODE');">
                            <asp:TextBox ID="WF_OFFICECODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OFFICECODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_3">
                    <!-- 荷主コード -->
                    <span class="ef">
                        <asp:Label ID="WF_SHIPPERCODE_L" runat="server" Text="荷主コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_SHIPPERCODE', <%=LIST_BOX_CLASSIFICATION.LC_JOINTLIST%>);" onchange="TextBox_change('WF_SHIPPERCODE');">
                        <asp:TextBox ID="WF_SHIPPERCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_SHIPPERCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 基地コード -->
                    <span class="ef">
                        <asp:Label ID="WF_PLANTCODE_L" runat="server" Text="基地コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_PLANTCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_PLANTCODE');">
                            <asp:TextBox ID="WF_PLANTCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_PLANTCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_4">
                    <!-- 油種大分類コード -->
                    <span class="ef">
                        <asp:Label ID="WF_BIGOILCODE_L" runat="server" Text="油種大分類コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_BIGOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_BIGOILCODE');">
                            <asp:TextBox ID="WF_BIGOILCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_BIGOILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 油種大分類名 -->
                    <span class="ef">
                        <asp:Label ID="WF_BIGOILNAME_L" runat="server" Text="油種大分類名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_BIGOILNAME" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="10"></asp:TextBox>
                        <asp:Label ID="WF_BIGOILNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_5">
                    <!-- 油種大分類名カナ -->
                    <span class="ef">
                        <asp:Label ID="WF_BIGOILKANA_L" runat="server" Text="油種大分類名カナ" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_BIGOILKANA" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="10"></asp:TextBox>
                        <asp:Label ID="WF_BIGOILKANA_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 油種中分類コード -->
                    <span class="ef">
                        <asp:Label ID="WF_MIDDLEOILCODE_L" runat="server" Text="油種中分類コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_MIDDLEOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_MIDDLEOILCODE');">
                            <asp:TextBox ID="WF_MIDDLEOILCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_MIDDLEOILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_6">
                    <!-- 油種中分類名 -->
                    <span class="ef">
                        <asp:Label ID="WF_MIDDLEOILNAME_L" runat="server" Text="油種中分類名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MIDDLEOILNAME" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_MIDDLEOILNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 油種中分類名カナ -->
                    <span class="ef">
                        <asp:Label ID="WF_MIDDLEOILKANA_L" runat="server" Text="油種中分類名カナ" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MIDDLEOILKANA" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_MIDDLEOILKANA_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_7">
                    <!-- 油種コード -->
                    <span class="ef">
                        <asp:Label ID="WF_OILCODE_L" runat="server" Text="油種コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_OILCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_OILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 油種名 -->
                    <span class="ef">
                        <asp:Label ID="WF_OILNAME_L" runat="server" Text="油種名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OILNAME" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="40"></asp:TextBox>
                        <asp:Label ID="WF_OILNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_8">
                    <!-- 油種名カナ -->
                    <span class="ef">
                        <asp:Label ID="WF_OILKANA_L" runat="server" Text="油種名カナ" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OILKANA" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="40"></asp:TextBox>
                        <asp:Label ID="WF_OILKANA_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 油種細分コード -->
                    <span class="ef">
                        <asp:Label ID="WF_SEGMENTOILCODE_L" runat="server" Text="油種細分コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_SEGMENTOILCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                        <asp:Label ID="WF_SEGMENTOILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_9">
                    <!-- 油種名（細分） -->
                    <span class="ef">
                        <asp:Label ID="WF_SEGMENTOILNAME_L" runat="server" Text="油種名（細分）" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SEGMENTOILNAME" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="40"></asp:TextBox>
                        <asp:Label ID="WF_SEGMENTOILNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- OT油種コード -->
                    <span class="ef">
                        <asp:Label ID="WF_OTOILCODE_L" runat="server" Text="OT油種コード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OTOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OTOILCODE');">
                            <asp:TextBox ID="WF_OTOILCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OTOILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_10">
                    <!-- OT油種名 -->
                    <span class="ef">
                        <asp:Label ID="WF_OTOILNAME_L" runat="server" Text="OT油種名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OTOILNAME" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="40"></asp:TextBox>
                        <asp:Label ID="WF_OTOILNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 荷主油種コード -->
                    <span class="ef">
                        <asp:Label ID="WF_SHIPPEROILCODE_L" runat="server" Text="荷主油種コード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SHIPPEROILCODE" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_SHIPPEROILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_11">
                    <!-- 荷主油種名 -->
                    <span class="ef">
                        <asp:Label ID="WF_SHIPPEROILNAME_L" runat="server" Text="荷主油種名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SHIPPEROILNAME" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="40"></asp:TextBox>
                        <asp:Label ID="WF_SHIPPEROILNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 積込チェック用油種コード -->
                    <span class="ef">
                        <asp:Label ID="WF_CHECKOILCODE_L" runat="server" Text="積込チェック用油種コード " CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_CHECKOILCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_CHECKOILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_12">
                    <!-- 積込チェック用油種名 -->
                    <span class="ef">
                        <asp:Label ID="WF_CHECKOILNAME_L" runat="server" Text="積込チェック用油種名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_CHECKOILNAME" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="40"></asp:TextBox>
                        <asp:Label ID="WF_CHECKOILNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 在庫管理対象フラグ -->
                    <span class="ef">
                        <asp:Label ID="WF_STOCKFLG_L" runat="server" Text="在庫管理対象フラグ" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_STOCKFLG', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_STOCKFLG');">
                            <asp:TextBox ID="WF_STOCKFLG" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_STOCKFLG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_13">
                    <!-- 受注登録可能期間FROM -->
                    <span class="ef">
                        <asp:Label ID="WF_ORDERFROMDATE_L" runat="server" Text="受注登録可能期間FROM" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ORDERFROMDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);" onchange="TextBox_change('WF_ORDERFROMDATE');">
                            <asp:TextBox ID="WF_ORDERFROMDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ORDERFROMDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 受注登録可能期間TO -->
                    <span class="ef">
                        <asp:Label ID="WF_ORDERTODATE_L" runat="server" Text="受注登録可能期間TO" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ORDERTODATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);" onchange="TextBox_change('WF_ORDERTODATE');">
                            <asp:TextBox ID="WF_ORDERTODATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ORDERTODATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_14">
                    <!-- 帳票用油種名 -->
                    <span class="ef">
                        <asp:Label ID="WF_REPORTOILNAME_L" runat="server" Text="帳票用油種名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_REPORTOILNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="40"></asp:TextBox>
                        <asp:Label ID="WF_REPORTOILNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- JR油種区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_JROILTYPE_L" runat="server" Text="JR油種区分" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_JROILTYPE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_JROILTYPE');">
                            <asp:TextBox ID="WF_JROILTYPE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_JROILTYPENAME" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_15">
                    <!-- 出荷口 -->
                    <span class="ef">
                        <asp:Label ID="WF_SHIPPINGGATE_L" runat="server" Text="出荷口(充填ポイント表用)" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_SHIPPINGGATE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_SHIPPINGGATE');">
                            <asp:TextBox ID="WF_SHIPPINGGATE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="40"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_SHIPPINGGATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 平均積込数量 -->
                    <span class="ef">
                        <asp:Label ID="WF_AVERAGELOADAMOUNT_L" runat="server" Text="平均積込数量" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_AVERAGELOADAMOUNT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_AVERAGELOADAMOUNT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_16">
                    <!-- 出荷計画枠 -->
                    <span class="ef">
                        <asp:Label ID="WF_SHIPPINGPLAN_L" runat="server" Text="出荷計画枠" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SHIPPINGPLAN" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                        <asp:Label ID="WF_SHIPPINGPLAN_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_17">
                    <asp:Label ID="WF_OILTERMTBL_L" runat="server" Text="品種出荷期間" CssClass="WF_TEXT_LABEL"></asp:Label>
                    <asp:GridView ID="WF_OILTERMTBL" runat="server" AllowPaging="false" AutoGenerateColumns="false" ShowHeader="true" Visible="true" ShowFooter="false" CssClass="WF_OILTERMTBL" ClientIDMode="Predictable">
                        <Columns>
                            <asp:TemplateField Visible="False">
                                <ItemTemplate>
                                    <asp:Label ID="WF_OILTERMTBL_CONSIGNEECODE" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "CONSIGNEECODE") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="WF_OILTERMTBL_TH1" ItemStyle-CssClass="WF_OILTERMTBL_TD1">
                                <HeaderTemplate>荷受人</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="WF_OILTERMTBL_CONSIGNEENAME" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "CONSIGNEENAME")%>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="WF_OILTERMTBL_TH2" ItemStyle-CssClass="WF_OILTERMTBL_TD2" ControlStyle-CssClass="WF_TEXTBOX_CSS calendarIcon">
                                <HeaderTemplate>受注登録可能期間FROM</HeaderTemplate>
                                <ItemTemplate>
                                    <span ondblclick="Field_DBclick('WF_OILTERMTBL_ORDERFROMDATE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>' , <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);" onchange="TextBox_change('WF_OILTERMTBL_ORDERFROMDATE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>');">
                                        <asp:TextBox ID="WF_OILTERMTBL_ORDERFROMDATE" runat="server" Text='<%# Bind("ORDERFROMDATE")%>' />
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="WF_OILTERMTBL_TH3" ItemStyle-CssClass="WF_OILTERMTBL_TD3" ControlStyle-CssClass="WF_TEXTBOX_CSS calendarIcon">
                                <HeaderTemplate>受注登録可能期間TO</HeaderTemplate>
                                <ItemTemplate>
                                    <span ondblclick="Field_DBclick('WF_OILTERMTBL_ORDERTODATE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);" onchange="TextBox_change('WF_OILTERMTBL_ORDERTODATE<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>');">
                                        <asp:TextBox ID="WF_OILTERMTBL_ORDERTODATE" runat="server" Text='<%# Bind("ORDERTODATE")%>' />
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderStyle-CssClass="WF_OILTERMTBL_TH4" ItemStyle-CssClass="WF_OILTERMTBL_TD4" ControlStyle-CssClass="WF_TEXTBOX_CSS boxIcon iconOnly">
                                <HeaderTemplate>削除フラグ</HeaderTemplate>
                                <ItemTemplate>
                                    <span ondblclick="Field_DBclick('WF_OILTERMTBL_DELFLG<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('WF_OILTERMTBL_DELFLG<%# String.Format("{0:000}", DirectCast(Container, GridViewRow).RowIndex + 1) %>');">
                                        <asp:TextBox ID="WF_OILTERMTBL_DELFLG" ReadOnly="true" runat="server" Text='<%# Bind("DELFLG")%>' />
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </p>
            </div>
        </div>

        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

        <!-- イベント用 -->
        <div hidden="hidden">
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


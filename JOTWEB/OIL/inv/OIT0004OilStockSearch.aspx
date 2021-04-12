<%@ Page Title="OIT0004S" Language="vb" AutoEventWireup="false" CodeBehind="OIT0004OilStockSearch.aspx.vb" Inherits="JOTWEB.OIT0004OilStockSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0004WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="OIT0004SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0004S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0004S.js")%>'></script>
</asp:content>

<asp:Content ID="OIT0004S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO"  class="btn-sticky" value="検索" onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 会社コード -->
            <div class="inputItem" style="display:none;">
                <a id="WF_CAMPCODE_LABEL">会社コード</a>
                <a class="ef" id="WF_CAMPCODE_CODE" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_CAMPCODE_NAME" >
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 組織コード -->
            <div class="inputItem" style="display:none;">
                <a id="WF_ORG_LABEL">組織コード</a>
                <a class="ef" id="WF_ORG_CODE" ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
                    <asp:TextBox ID="WF_ORG" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_ORG_NAME" >
                    <asp:Label ID="WF_ORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 営業所 -->
            <div class="inputItem">
                <a id="WF_OFFICECODE_LABEL"  class="requiredMark">営業所</a>
                <a class="ef" id="WF_OFFICECODE" ondblclick="Field_DBclick('TxtSalesOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtSalesOffice');">
                    <asp:TextBox ID="TxtSalesOffice" runat="server"  CssClass="boxIcon" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a id="WF_OFFICECODE_TEXT" >
                    <asp:Label ID="LblSalesOfficeName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 荷主 -->
            <div class="inputItem">
                <a id="WF_SHIPPER_LABEL"  class="requiredMark">荷主</a>
                <a class="ef" id="WF_SHIPPERCODE" ondblclick="Field_DBclick('TxtShipper', <%=LIST_BOX_CLASSIFICATION.LC_JOINTLIST%>);" onchange="TextBox_change('TxtShipper');">
                    <asp:TextBox ID="TxtShipper" runat="server"  CssClass="boxIcon" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a id="WF_SHIPPERCODE_TEXT" >
                    <asp:Label ID="LblShipperName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 油槽所 -->
            <div class="inputItem">
                <a id="WF_CONSIGNEE_LABEL" class="requiredMark">油槽所</a>
                <a class="ef" id="WF_CONSIGNEE" ondblclick="Field_DBclick('WF_CONSIGNEE', <%=LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST%>);" onchange="TextBox_change('WF_CONSIGNEE');">
                    <asp:TextBox  ID="WF_CONSIGNEE_CODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                </a>
                <a id="WF_CONSIGNEE_TEXT">
                    <asp:Label ID="WF_CONSIGNEE_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 年月日 -->
            <div class="inputItem">
                <a id="WF_STYMD_LABEL" class="requiredMark">年月日</a>
                <a class="ef" id="WF_STYMD" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);" onchange="TextBox_change('WF_STYMD');">
                    <asp:TextBox ID="WF_STYMD_CODE" runat="server" CssClass="calendarIcon"  onblur="MsgClear();"></asp:TextBox>
                </a>
            </div>
            <div class="inputItem" id="divUpdateInfo" runat="server" enableviewstate="true" visible="false">
                <a id="WF_LASTUPDATE_LABEL">更新情報</a>
                <a class="ef" id="WF_LASTUPDATE" >
                    <table class="tblLastupdate">
                        <tr>
                            <th class="userName">更新者</th>
                            <th class="update">更新日時</th>
                        </tr>
                        <tr >
                            <td>
                                <asp:Label ID="WF_UpdateUser" runat="server" Text=""></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="WF_UpdateDtm" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>

                    </table>

                </a>
            </div>
            <div class="inputItem"  id="divConsigneeUpdateInfo10" runat="server" enableviewstate="true" visible="false">
               <a id="WF_CONSIGNEE_UPDINFO_LABEL10">北信油槽所更新情報</a>
               <a class="ef" id="WF_CONSIGNEE_UPDINFO10" >
                    <table class="tblLastupdate consignee code10">
                        <tr>
                            <th class="userName">更新者</th>
                            <th class="update">更新日時</th>
                            <th class="fixed">オーダー確定(<%=System.DateTime.Now.ToString("M月d日") %>)</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="WF_ConsigneeUser10" runat="server" Text="&nbsp;"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="WF_ConsigneeUpdateDtm10" runat="server" Text="&nbsp;"></asp:Label>
                            </td>
                            <td>
                                 <asp:Label ID="WF_ConsigneeFixStatus10" runat="server" Text="&nbsp;"></asp:Label>
                            </td>
                        </tr>
                    </table>
               </a>

            </div>
            <div class="inputItem"  id="divConsigneeUpdateInfo20" runat="server" enableviewstate="true" visible="false">
               <a id="WF_CONSIGNEE_UPDINFO_LABEL20">甲府油槽所更新情報</a>
               <a class="ef" id="WF_CONSIGNEE_UPDINFO20" >
                    <table class="tblLastupdate consignee code20">
                        <tr>
                            <th class="userName">更新者</th>
                            <th class="update">更新日時</th>
                            <th class="fixed">オーダー確定(<%=System.DateTime.Now.ToString("M月d日") %>)</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="WF_ConsigneeUser20" runat="server" Text="&nbsp;"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="WF_ConsigneeUpdateDtm20" runat="server" Text="&nbsp;"></asp:Label>
                            </td>
                            <td>
                                 <asp:Label ID="WF_ConsigneeFixStatus20" runat="server" Text="&nbsp;"></asp:Label>
                            </td>
                        </tr>
                    </table>
               </a>

            </div>
        </div>
    </div>
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

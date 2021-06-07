<%@ Page Title="OIM0019C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0019AccountCreate.aspx.vb" Inherits="JOTWEB.OIM0019AccountCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0019WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0019CH" ContentPlaceHolderID="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0019C.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0019C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0019C" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR"  class="btn-sticky" value="戻る" onclick="ButtonClick('WF_CLEAR');" />
                    </div>
                </div>
            </div>

            <div id="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- 選択No -->
                    <span>
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" CssClass="WF_TEXT_LABEL"></asp:Label>
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
                </p>

                <p id="KEY_LINE_3">
                    <!-- 適用開始年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_FROMYMD_L" runat="server" Text="適用開始年月日" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_FROMYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_FROMYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_FROMYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                    
                <p id="KEY_LINE_4">
                    <!-- 適用終了年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_ENDYMD_L" runat="server" Text="適用終了年月日" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_ENDYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ENDYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                    
                <p id="KEY_LINE_5">
                    <!-- 科目コード -->
                    <span class="ef">
                        <asp:Label ID="WF_ACCOUNTCODE_L" runat="server" Text="科目コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ACCOUNTCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ACCOUNTCODE');">
                            <asp:TextBox ID="WF_ACCOUNTCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="8"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ACCOUNTCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 科目名 -->
                    <span class="ef">
                        <asp:Label ID="WF_ACCOUNTNAME_L" runat="server" Text="科目名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ACCOUNTNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="40" Width="700"></asp:TextBox>
                        <asp:Label ID="WF_ACCOUNTNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- セグメント -->
                    <span class="ef">
                        <asp:Label ID="WF_SEGMENTCODE_L" runat="server" Text="セグメント" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_SEGMENTCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_SEGMENTCODE');">
                            <asp:TextBox ID="WF_SEGMENTCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_SEGMENTCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- セグメント名 -->
                    <span class="ef">
                        <asp:Label ID="WF_SEGMENTNAME_L" runat="server" Text="セグメント名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SEGMENTNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="40" Width="700"></asp:TextBox>
                        <asp:Label ID="WF_SEGMENTNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="KEY_LINE_9">
                    <!-- セグメント枝番 -->
                    <span class="ef">
                        <asp:Label ID="WF_SEGMENTBRANCHCODE_L" runat="server" Text="セグメント枝番" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_SEGMENTBRANCHCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_SEGMENTBRANCHCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="KEY_LINE_10">
                    <!-- セグメント枝番名 -->
                    <span class="ef">
                        <asp:Label ID="WF_SEGMENTBRANCHNAME_L" runat="server" Text="セグメント枝番名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SEGMENTBRANCHNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="40" Width="700"></asp:TextBox>
                        <asp:Label ID="WF_SEGMENTBRANCHNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="KEY_LINE_11">
                    <!-- 科目区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_ACCOUNTTYPE_L" runat="server" Text="科目区分" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ACCOUNTTYPE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ACCOUNTTYPE');">
                            <asp:TextBox ID="WF_ACCOUNTTYPE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ACCOUNTTYPE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="KEY_LINE_12">
                    <!-- 科目区分名 -->
                    <span class="ef">
                        <asp:Label ID="WF_ACCOUNTTYPENAME_L" runat="server" Text="科目区分名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ACCOUNTTYPENAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="40" Width="700"></asp:TextBox>
                        <asp:Label ID="WF_ACCOUNTTYPENAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 税区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_TAXTYPE_L" runat="server" Text="税区分" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_TAXTYPE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_TAXTYPE');">
                            <asp:TextBox ID="WF_TAXTYPE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_TAXTYPE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
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

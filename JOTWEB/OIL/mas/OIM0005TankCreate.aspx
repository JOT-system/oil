<%@ Page Title="OIM0005C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0005TankCreate.aspx.vb" Inherits="JOTWEB.OIM0005TankCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0005CH" ContentPlaceHolderID="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0005C.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0005C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0005C" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="表更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR"  class="btn-sticky" value="クリア" onclick="ButtonClick('WF_CLEAR');" />
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

                    <!-- JOT車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_TANKNUMBER_L" runat="server" Text="JOT車番" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_TANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="8" onchange="TextBox_change('WF_TANKNUMBER');"></asp:TextBox>
                        <asp:Label ID="WF_TANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_3">
                    <!-- 形式 -->
                    <span class="ef">
                        <asp:Label ID="WF_MODEL_L" runat="server" Text="形式" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MODEL" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20" onchange="TextBox_change('WF_TANKNUMBER');"></asp:TextBox>
                        <asp:Label ID="WF_MODEL_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 形式カナ -->
                    <span class="ef">
                        <asp:Label ID="WF_MODELKANA_L" runat="server" Text="形式カナ" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MODELKANA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_MODELKANA_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 荷重 -->
                    <span class="ef">
                        <asp:Label ID="WF_LOAD_L" runat="server" Text="荷重" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LOAD" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LOAD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 荷重単位 -->
                    <span class="ef">
                        <asp:Label ID="WF_LOADUNIT_L" runat="server" Text="荷重単位" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_LOADUNIT', <%=LIST_BOX_CLASSIFICATION.LC_UNIT%>);" onchange="TextBox_change('WF_LOADUNIT');">
                            <asp:TextBox ID="WF_LOADUNIT" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LOADUNIT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 容積 -->
                    <span class="ef">
                        <asp:Label ID="WF_VOLUME_L" runat="server" Text="容積" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_VOLUME" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_VOLUME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 容積単位 -->
                    <span class="ef">
                        <asp:Label ID="WF_VOLUMEUNIT_L" runat="server" Text="容積単位" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_VOLUMEUNIT', <%=LIST_BOX_CLASSIFICATION.LC_UNIT%>);" onchange="TextBox_change('WF_VOLUMEUNIT');">
                            <asp:TextBox ID="WF_VOLUMEUNIT" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_VOLUMEUNIT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5.5">
                    <!-- 自重 -->
                    <span class="ef">
                        <asp:Label ID="WF_MYWEIGHT_L" runat="server" Text="自重" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MYWEIGHT" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_MYWEIGHT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 原籍所有者C -->
                    <span class="ef">
                        <asp:Label ID="WF_ORIGINOWNERCODE_L" runat="server" Text="原籍所有者C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ORIGINOWNERCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORIGINOWNERCODE%>);" onchange="TextBox_change('WF_ORIGINOWNERCODE');">
                            <asp:TextBox ID="WF_ORIGINOWNERCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ORIGINOWNERCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 名義所有者C -->
                    <span class="ef">
                        <asp:Label ID="WF_OWNERCODE_L" runat="server" Text="名義所有者C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OWNERCODE', <%=LIST_BOX_CLASSIFICATION.LC_OWNER%>);" onchange="TextBox_change('WF_OWNERCODE');">
                            <asp:TextBox ID="WF_OWNERCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OWNERCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- リース先C -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASECODE_L" runat="server" Text="リース先C" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span class="ef" ondblclick="Field_DBclick('WF_LEASECODE', <%=LIST_BOX_CLASSIFICATION.LC_LEASE%>);" onchange="TextBox_change('WF_LEASECODE');">
                            <asp:TextBox ID="WF_LEASECODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LEASECODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- リース区分C -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASECLASS_L" runat="server" Text="リース区分C" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_LEASECLASS', <%=LIST_BOX_CLASSIFICATION.LC_LEASECLASS%>);" onchange="TextBox_change('WF_LEASECLASS');">
                            <asp:TextBox ID="WF_LEASECLASS" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LEASECLASS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 自動延長 -->
                    <span class="ef">
                        <asp:Label ID="WF_AUTOEXTENTION_L" runat="server" Text="自動延長"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_AUTOEXTENTION', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_AUTOEXTENTION');">
                            <asp:TextBox ID="WF_AUTOEXTENTION" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_AUTOEXTENTION_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- リース開始年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASESTYMD_L" runat="server" Text="リース開始年月日"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_LEASESTYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_LEASESTYMD" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LEASESTYMD_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- リース満了年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASEENDYMD_L" runat="server" Text="リース満了年月日"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_LEASEENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_LEASEENDYMD" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LEASEENDYMD_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 第三者使用者C -->
                    <span class="ef">
                        <asp:Label ID="WF_USERCODE_L" runat="server" Text="第三者使用者C" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_USERCODE', <%=LIST_BOX_CLASSIFICATION.LC_THIRDUSER%>);" onchange="TextBox_change('WF_USERCODE');">
                            <asp:TextBox ID="WF_USERCODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_USERCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>                    
                    <!-- 第三者使用期限 -->
                    <span class="ef">
                        <asp:Label ID="WF_USERLIMIT_L" runat="server" Text="第三者使用期限"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_USERLIMIT', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_USERLIMIT" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_USERLIMIT_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 原常備駅C -->
                    <span class="ef">
                        <asp:Label ID="WF_CURRENTSTATIONCODE_L" runat="server" Text="原常備駅C"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_CURRENTSTATIONCODE', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_CURRENTSTATIONCODE');">
                            <asp:TextBox ID="WF_CURRENTSTATIONCODE" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_CURRENTSTATIONCODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 原専用種別C -->
                    <span class="ef">
                        <asp:Label ID="WF_DEDICATETYPECODE_L" runat="server" Text="原専用種別C"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_DEDICATETYPECODE', <%=LIST_BOX_CLASSIFICATION.LC_DEDICATETYPE%>);" onchange="TextBox_change('WF_DEDICATETYPECODE');">
                            <asp:TextBox ID="WF_DEDICATETYPECODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_DEDICATETYPECODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_12">
                    <!-- 臨時常備駅C -->
                    <span class="ef">
                        <asp:Label ID="WF_EXTRADINARYSTATIONCODE_L" runat="server" Text="臨時常備駅C" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_EXTRADINARYSTATIONCODE', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_EXTRADINARYSTATIONCODE');">
                            <asp:TextBox ID="WF_EXTRADINARYSTATIONCODE" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_EXTRADINARYSTATIONCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>                    
                    <!-- 臨時常備駅期限 -->
                    <span class="ef">
                        <asp:Label ID="WF_LIMITTEXTRADIARYSTATION_L" runat="server" Text="臨時常備駅期限"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_LIMITTEXTRADIARYSTATION', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_LIMITTEXTRADIARYSTATION" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LIMITTEXTRADIARYSTATION_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 臨時専用種別C -->
                    <span class="ef">
                        <asp:Label ID="WF_EXTRADINARYTYPECODE_L" runat="server" Text="臨時専用種別C" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_EXTRADINARYTYPECODE', <%=LIST_BOX_CLASSIFICATION.LC_EXTRADINARYTYPE%>);" onchange="TextBox_change('WF_DEDICATETYPECODE');">
                            <asp:TextBox ID="WF_EXTRADINARYTYPECODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_EXTRADINARYTYPECODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 臨時専用期限 -->
                    <span class="ef">
                        <asp:Label ID="WF_EXTRADINARYLIMIT_L" runat="server" Text="臨時専用期限"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_EXTRADINARYLIMIT', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_EXTRADINARYLIMIT" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_EXTRADINARYLIMIT_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_14">
                    <!-- 油種大分類コード -->
                    <span class="ef">
                        <asp:Label ID="WF_BIGOILCODE_L" runat="server" Text="油種大分類コード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_BIGOILCODE', <%=LIST_BOX_CLASSIFICATION.LC_BIGOILCODE%>);" onchange="TextBox_change('WF_BIGOILCODE');">
                            <asp:TextBox ID="WF_BIGOILCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_BIGOILCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 運用基地C -->
                    <span class="ef">
                        <asp:Label ID="WF_OPERATIONBASECODE_L" runat="server" Text="運用基地C"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OPERATIONBASECODE', <%=LIST_BOX_CLASSIFICATION.LC_BASE%>);" onchange="TextBox_change('WF_OPERATIONBASECODE');">
                            <asp:TextBox ID="WF_OPERATIONBASECODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OPERATIONBASECODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_15">
                    <!-- 塗色C -->
                    <span class="ef">
                        <asp:Label ID="WF_COLORCODE_L" runat="server" Text="塗色C"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_COLORCODE', <%=LIST_BOX_CLASSIFICATION.LC_COLOR%>);" onchange="TextBox_change('WF_COLORCODE');">
                            <asp:TextBox ID="WF_COLORCODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_COLORCODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- マークコード-->
                    <span class="ef">
                        <asp:Label ID="WF_MARKCODE_L" runat="server" Text="マークコード"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_MARKCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_MARKCODE');">
                            <asp:TextBox ID="WF_MARKCODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_MARKCODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_16">
                    <!-- JXTG仙台タグコード -->
                    <span class="ef">
                        <asp:Label ID="WF_JXTGTAGCODE1_L" runat="server" Text="JXTG仙台タグコード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JXTGTAGCODE1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JXTGTAGCODE1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- JXTG千葉タグコード -->
                    <span class="ef">
                        <asp:Label ID="WF_JXTGTAGCODE2_L" runat="server" Text="JXTG千葉タグコード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_JXTGTAGCODE2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_JXTGTAGCODE2');">
                            <asp:TextBox ID="WF_JXTGTAGCODE2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_JXTGTAGCODE2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_17">
                    <!-- JXT川崎タグコード -->
                    <span class="ef">
                        <asp:Label ID="WF_JXTGTAGCODE3_L" runat="server" Text="JXT川崎タグコード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JXTGTAGCODE3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JXTGTAGCODE3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- JXTG根岸タグコード -->
                    <span class="ef">
                        <asp:Label ID="WF_JXTGTAGCODE4_L" runat="server" Text="JXTG根岸タグコード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JXTGTAGCODE4" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JXTGTAGCODE4_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_18">
                    <!-- 出光昭シタグコード -->
                    <span class="ef">
                        <asp:Label ID="WF_IDSSTAGCODE_L" runat="server" Text="出光昭シタグコード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_IDSSTAGCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_IDSSTAGCODE');">
                            <asp:TextBox ID="WF_IDSSTAGCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_IDSSTAGCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- コスモタグコード -->
                    <span class="ef">
                        <asp:Label ID="WF_COSMOTAGCODE_L" runat="server" Text="コスモタグコード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_COSMOTAGCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_COSMOTAGCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_19">
                    <!-- 予備1 -->
                    <span class="ef">
                        <asp:Label ID="WF_RESERVE1_L" runat="server" Text="予備1" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RESERVE1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_RESERVE1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 予備2 -->
                    <span class="ef">
                        <asp:Label ID="WF_RESERVE2_L" runat="server" Text="予備2" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RESERVE2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_RESERVE2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_20">
                    <!-- 次回交検年月日(JR） -->
                    <span class="ef">
                        <asp:Label ID="WF_JRINSPECTIONDATE_L" runat="server" Text="次回交検年月日(JR）" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_JRINSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_JRINSPECTIONDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_JRINSPECTIONDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 次回交検年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_INSPECTIONDATE_L" runat="server" Text="次回交検年月日" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_INSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_INSPECTIONDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_INSPECTIONDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_21">
                    <!-- 次回指定年月日(JR) -->
                    <span class="ef">
                        <asp:Label ID="WF_JRSPECIFIEDDATE_L" runat="server" Text="次回指定年月日(JR)" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_JRSPECIFIEDDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_JRSPECIFIEDDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_JRSPECIFIEDDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 次回指定年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_SPECIFIEDDATE_L" runat="server" Text="次回指定年月日" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_SPECIFIEDDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_SPECIFIEDDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_SPECIFIEDDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_22">                    <!-- 次回全検年月日(JR)  -->
                    <span class="ef">
                        <asp:Label ID="WF_JRALLINSPECTIONDATE_L" runat="server" Text="次回全検年月日(JR) " CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_JRALLINSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_JRALLINSPECTIONDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_JRALLINSPECTIONDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 次回全検年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_ALLINSPECTIONDATE_L" runat="server" Text="次回全検年月日" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ALLINSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_ALLINSPECTIONDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ALLINSPECTIONDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_23">
                    <!-- 前回全検年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_PREINSPECTIONDATE_L" runat="server" Text="前回全検年月日" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_PREINSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_PREINSPECTIONDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_PREINSPECTIONDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>                    
                    <!-- 取得年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_GETDATE_L" runat="server" Text="取得年月日"  CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_GETDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_GETDATE" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_GETDATE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_24">
                    <!-- 車籍編入年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_TRANSFERDATE_L" runat="server" Text="車籍編入年月日"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_TRANSFERDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_TRANSFERDATE" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_TRANSFERDATE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 取得先C -->
                    <span class="ef">
                        <asp:Label ID="WF_OBTAINEDCODE_L" runat="server" Text="取得先C" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OBTAINEDCODE', <%=LIST_BOX_CLASSIFICATION.LC_OBTAINED%>);" onchange="TextBox_change('WF_OBTAINEDCODE');">
                            <asp:TextBox ID="WF_OBTAINEDCODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OBTAINEDCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_25">
                    <!-- 現在経年 -->
                    <span class="ef">
                        <asp:Label ID="WF_PROGRESSYEAR_L" runat="server" Text="現在経年" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_PROGRESSYEAR" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_PROGRESSYEAR_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 次回全検時経年 -->
                    <span class="ef">
                        <asp:Label ID="WF_NEXTPROGRESSYEAR_L" runat="server" Text="次回全検時経年" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_NEXTPROGRESSYEAR" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_NEXTPROGRESSYEAR_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_26">
                    <!-- 車籍除外年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_EXCLUDEDATE_L" runat="server" Text="車籍除外年月日" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_EXCLUDEDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_EXCLUDEDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon" MaxLength="0"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_EXCLUDEDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 資産除却年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_RETIRMENTDATE_L" runat="server" Text="資産除却年月日" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_RETIRMENTDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_RETIRMENTDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon" MaxLength="0"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_RETIRMENTDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_27">
                    <!-- JR車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_JRTANKNUMBER_L" runat="server" Text="JR車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JRTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JRTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- JR車種コード -->
                    <span class="ef">
                        <asp:Label ID="WF_JRTANKTYPE_L" runat="server" Text="JR車種コード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JRTANKTYPE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JRTANKTYPE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_28">
                    <!-- 旧JOT車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_OLDTANKNUMBER_L" runat="server" Text="旧JOT車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OLDTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_OLDTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- OT車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_OTTANKNUMBER_L" runat="server" Text="OT車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OTTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_OTTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_29">
                    <!-- JXTG仙台車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_JXTGTANKNUMBER1_L" runat="server" Text="JXTG仙台車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JXTGTANKNUMBER1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JXTGTANKNUMBER1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- JXTG千葉車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_JXTGTANKNUMBER2_L" runat="server" Text="JXTG千葉車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JXTGTANKNUMBER2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JXTGTANKNUMBER2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_30">
                    <!-- JXTG川崎車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_JXTGTANKNUMBER3_L" runat="server" Text="JXTG川崎車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JXTGTANKNUMBER3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JXTGTANKNUMBER3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- JXTG根岸車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_JXTGTANKNUMBER4_L" runat="server" Text="JXTG根岸車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JXTGTANKNUMBER4" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JXTGTANKNUMBER4_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_31">
                    <!-- コスモ車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_COSMOTANKNUMBER_L" runat="server" Text="コスモ車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_COSMOTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_COSMOTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 富士石油車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_FUJITANKNUMBER_L" runat="server" Text="富士石油車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_FUJITANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_FUJITANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_32">
                    <!-- 出光昭シ車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_SHELLTANKNUMBER_L" runat="server" Text="出光昭シ車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SHELLTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_SHELLTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 出光昭シSAP車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_SAPSHELLTANKNUMBER_L" runat="server" Text="出光昭シSAP車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SAPSHELLTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_SAPSHELLTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_33">
                    <!-- 予備 -->
                    <span class="ef">
                        <asp:Label ID="WF_RESERVE3_L" runat="server" Text="予備" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RESERVE3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_RESERVE3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 利用フラグ -->
                    <span class="ef">
                        <asp:Label ID="WF_USEDFLG_L" runat="server" Text="利用フラグ" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_USEDFLG', <%=LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY%>);" onchange="TextBox_change('WF_USEDFLG');">
                            <asp:TextBox ID="WF_USEDFLG" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_USEDFLG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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

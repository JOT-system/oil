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
                        <span ondblclick="Field_DBclick('WF_TANKNUMBER', <%=LIST_BOX_CLASSIFICATION.LC_TANKNUMBER%>)" onchange="TextBox_change('WF_TANKNUMBER');">
                            <asp:TextBox ID="WF_TANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="8"></asp:TextBox>
                        </span>
                    </span>
                </p>

                <p id="KEY_LINE_3">
                    <!-- 原籍所有者C -->
                    <span class="ef">
                        <asp:Label ID="WF_ORIGINOWNERCODE_L" runat="server" Text="原籍所有者C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ORIGINOWNERCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORIGINOWNER%>);" onchange="TextBox_change('WF_ORIGINOWNERCODE');">
                            <asp:TextBox ID="WF_ORIGINOWNERCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ORIGINOWNERCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 名義所有者C -->
                    <span class="ef">
                        <asp:Label ID="WF_OWNERCODE_L" runat="server" Text="名義所有者C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OWNERCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORIGINOWNER%>);" onchange="TextBox_change('WF_OWNERCODE');">
                            <asp:TextBox ID="WF_OWNERCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OWNERCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 原籍所有者 -->
                    <span class="ef">
                        <asp:Label ID="WF_ORIGINOWNERNAME_L" runat="server" Text="原籍所有者" CssClass="WF_TEXT_LABEL"></asp:Label>
                      <asp:TextBox ID="WF_ORIGINOWNERNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_ORIGINOWNERNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 名義所有者 -->
                    <span class="ef">
                        <asp:Label ID="WF_OWNERNAME_L" runat="server" Text="名義所有者" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OWNERNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_OWNERNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>


                <p id="KEY_LINE_5">
                    <!-- リース先C -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASECODE_L" runat="server" Text="リース先C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span class="ef" ondblclick="Field_DBclick('WF_LEASECODE', <%=LIST_BOX_CLASSIFICATION.LC_LEASE%>);" onchange="TextBox_change('WF_LEASECODE');">
                            <asp:TextBox ID="WF_LEASECODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LEASECODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- リース区分C -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASECLASS_L" runat="server" Text="リース区分C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_LEASECLASS', <%=LIST_BOX_CLASSIFICATION.LC_LEASECLASS%>);" onchange="TextBox_change('WF_LEASECLASS');">
                            <asp:TextBox ID="WF_LEASECLASS" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LEASECLASS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 自動延長 -->
                    <span class="ef">
                        <asp:Label ID="WF_AUTOEXTENTION_L" runat="server" Text="自動延長"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_AUTOEXTENTION" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_AUTOEXTENTION_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- リース開始年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASESTYMD_L" runat="server" Text="リース開始年月日"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_LEASESTYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_LEASESTYMD" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LEASESTYMD_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- リース満了年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASEENDYMD_L" runat="server" Text="リース満了年月日"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_LEASEENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_LEASEENDYMD" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LEASEENDYMD_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 第三者使用者C -->
                    <span class="ef">
                        <asp:Label ID="WF_USERCODE_L" runat="server" Text="第三者使用者C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_USERCODE', <%=LIST_BOX_CLASSIFICATION.LC_THIRDUSER%>);" onchange="TextBox_change('WF_USERCODE');">
                            <asp:TextBox ID="WF_USERCODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_USERCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 原常備駅C -->
                    <span class="ef">
                        <asp:Label ID="WF_CURRENTSTATIONCODE_L" runat="server" Text="原常備駅C"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_CURRENTSTATIONCODE', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_CURRENTSTATIONCODE');">
                            <asp:TextBox ID="WF_CURRENTSTATIONCODE" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_CURRENTSTATIONCODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 臨時常備駅C -->
                    <span class="ef">
                        <asp:Label ID="WF_EXTRADINARYSTATIONCODE_L" runat="server" Text="臨時常備駅C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_EXTRADINARYSTATIONCODE', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_EXTRADINARYSTATIONCODE');">
                            <asp:TextBox ID="WF_EXTRADINARYSTATIONCODE" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_EXTRADINARYSTATIONCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 第三者使用期限 -->
                    <span class="ef">
                        <asp:Label ID="WF_USERLIMIT_L" runat="server" Text="第三者使用期限"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_USERLIMIT', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_USERLIMIT" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_USERLIMIT_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 臨時常備駅期限 -->
                    <span class="ef">
                        <asp:Label ID="WF_LIMITTEXTRADIARYSTATION_L" runat="server" Text="臨時常備駅期限"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_LIMITTEXTRADIARYSTATION', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_LIMITTEXTRADIARYSTATION" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_LIMITTEXTRADIARYSTATION_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 原専用種別C -->
                    <span class="ef">
                        <asp:Label ID="WF_DEDICATETYPECODE_L" runat="server" Text="原専用種別C"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_DEDICATETYPECODE', <%=LIST_BOX_CLASSIFICATION.LC_DEDICATETYPE%>);" onchange="TextBox_change('WF_DEDICATETYPECODE');">
                            <asp:TextBox ID="WF_DEDICATETYPECODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_DEDICATETYPECODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 臨時専用種別C -->
                    <span class="ef">
                        <asp:Label ID="WF_EXTRADINARYTYPECODE_L" runat="server" Text="臨時専用種別C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_EXTRADINARYTYPECODE', <%=LIST_BOX_CLASSIFICATION.LC_EXTRADINARYTYPE%>);" onchange="TextBox_change('WF_DEDICATETYPECODE');">
                            <asp:TextBox ID="WF_EXTRADINARYTYPECODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_EXTRADINARYTYPECODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 臨時専用期限 -->
                    <span class="ef">
                        <asp:Label ID="WF_EXTRADINARYLIMIT_L" runat="server" Text="臨時専用期限"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_EXTRADINARYLIMIT', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_EXTRADINARYLIMIT" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_EXTRADINARYLIMIT_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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

                <p id="KEY_LINE_12">
                    <!-- 塗色C -->
                    <span class="ef">
                        <asp:Label ID="WF_COLORCODE_L" runat="server" Text="塗色C"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_COLORCODE', <%=LIST_BOX_CLASSIFICATION.LC_COLOR%>);" onchange="TextBox_change('WF_COLORCODE');">
                            <asp:TextBox ID="WF_COLORCODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_COLORCODE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- マークコード-->
                    <span class="ef">
                        <asp:Label ID="WF_MARK_L" runat="server" Text="マークコード"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_MARK" runat="server"  CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_MARK_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- マーク名 -->
                    <span class="ef">
                        <asp:Label ID="WF_MARKNAME_L" runat="server" Text="エコレール"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_MARKNAME" runat="server"  CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_MARKNAME_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 取得年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_GETDATE_L" runat="server" Text="取得年月日"  CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_GETDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_GETDATE" runat="server"  CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_GETDATE_TEXT" runat="server"  CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_14">
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
                        <asp:Label ID="WF_OBTAINEDCODE_L" runat="server" Text="取得先C" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OBTAINEDCODE', <%=LIST_BOX_CLASSIFICATION.LC_OBTAINED%>);" onchange="TextBox_change('WF_OBTAINEDCODE');">
                            <asp:TextBox ID="WF_OBTAINEDCODE" runat="server"  CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OBTAINEDCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_15">
                    <!-- 形式 -->
                    <span class="ef">
                        <asp:Label ID="WF_MODEL_L" runat="server" Text="形式" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_MODEL', <%=LIST_BOX_CLASSIFICATION.LC_TANKMODEL%>);" onchange="TextBox_change('WF_MODEL');">
                            <asp:TextBox ID="WF_MODEL" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_MODEL_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 形式カナ -->
                    <span class="ef">
                        <asp:Label ID="WF_MODELKANA_L" runat="server" Text="形式カナ" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MODELKANA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                        <asp:Label ID="WF_MODELKANA_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_16">
                    <!-- 荷重 -->
                    <span class="ef">
                        <asp:Label ID="WF_LOAD_L" runat="server" Text="荷重" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LOAD" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LOAD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 荷重単位 -->
                    <span class="ef">
                        <asp:Label ID="WF_LOADUNIT_L" runat="server" Text="荷重単位" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LOADUNIT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_LOADUNIT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_17">
                    <!-- 容積 -->
                    <span class="ef">
                        <asp:Label ID="WF_VOLUME_L" runat="server" Text="容積" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_VOLUME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                        <asp:Label ID="WF_VOLUME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 容積単位 -->
                    <span class="ef">
                        <asp:Label ID="WF_VOLUMEUNIT_L" runat="server" Text="容積単位" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_VOLUMEUNIT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_VOLUMEUNIT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>



                <p id="KEY_LINE_18">
                    <!-- リース先 -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASENAME_L" runat="server" Text="リース先" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LEASENAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_LEASENAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- リース区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_LEASECLASSNEMAE_L" runat="server" Text="リース区分" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LEASECLASSNEMAE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_LEASECLASSNEMAE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_19">
                    <!-- 第三者使用者 -->
                    <span class="ef">
                        <asp:Label ID="WF_USERNAME_L" runat="server" Text="第三者使用者" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_USERNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_USERNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 原常備駅 -->
                    <span class="ef">
                        <asp:Label ID="WF_CURRENTSTATIONNAME_L" runat="server" Text="原常備駅" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_CURRENTSTATIONNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_CURRENTSTATIONNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_20">
                    <!-- 臨時常備駅 -->
                    <span class="ef">
                        <asp:Label ID="WF_EXTRADINARYSTATIONNAME_L" runat="server" Text="臨時常備駅" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_EXTRADINARYSTATIONNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_EXTRADINARYSTATIONNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 原専用種別 -->
                    <span class="ef">
                        <asp:Label ID="WF_DEDICATETYPENAME_L" runat="server" Text="原専用種別" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_DEDICATETYPENAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_DEDICATETYPENAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_21">
                    <!-- 臨時専用種別 -->
                    <span class="ef">
                        <asp:Label ID="WF_EXTRADINARYTYPENAME_L" runat="server" Text="臨時専用種別" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_EXTRADINARYTYPENAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_EXTRADINARYTYPENAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 運用場所 -->
                    <span class="ef">
                        <asp:Label ID="WF_OPERATIONBASENAME_L" runat="server" Text="運用場所" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OPERATIONBASENAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_OPERATIONBASENAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_22">
                    <!-- 塗色 -->
                    <span class="ef">
                        <asp:Label ID="WF_COLORNAME_L" runat="server" Text="塗色" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_COLORNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_COLORNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 予備1 -->
                    <span class="ef">
                        <asp:Label ID="WF_RESERVE1_L" runat="server" Text="予備1" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RESERVE1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_RESERVE1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_23">
                    <!-- 予備2 -->
                    <span class="ef">
                        <asp:Label ID="WF_RESERVE2_L" runat="server" Text="予備2" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RESERVE2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_RESERVE2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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

                <p id="KEY_LINE_24">
                    <!-- 次回全検年月日(JR)  -->
                    <span class="ef">
                        <asp:Label ID="WF_JRALLINSPECTIONDATE_L" runat="server" Text="次回全検年月日(JR) " CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_JRALLINSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_JRALLINSPECTIONDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_JRALLINSPECTIONDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 現在経年 -->
                    <span class="ef">
                        <asp:Label ID="WF_PROGRESSYEAR_L" runat="server" Text="現在経年" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_PROGRESSYEAR" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_PROGRESSYEAR_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_25">
                    <!-- 次回全検時経年 -->
                    <span class="ef">
                        <asp:Label ID="WF_NEXTPROGRESSYEAR_L" runat="server" Text="次回全検時経年" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_NEXTPROGRESSYEAR" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_NEXTPROGRESSYEAR_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 次回交検年月日(JR） -->
                    <span class="ef">
                        <asp:Label ID="WF_JRINSPECTIONDATE_L" runat="server" Text="次回交検年月日(JR）" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_JRINSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_JRINSPECTIONDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_JRINSPECTIONDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_26">
                    <!-- 次回交検年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_INSPECTIONDATE_L" runat="server" Text="次回交検年月日" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_INSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_INSPECTIONDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_INSPECTIONDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 次回指定年月日(JR) -->
                    <span class="ef">
                        <asp:Label ID="WF_JRSPECIFIEDDATE_L" runat="server" Text="次回指定年月日(JR)" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_JRSPECIFIEDDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_JRSPECIFIEDDATE" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_JRSPECIFIEDDATE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_27">
                    <!-- JR車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_JRTANKNUMBER_L" runat="server" Text="JR車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JRTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JRTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 旧JOT車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_OLDTANKNUMBER_L" runat="server" Text="旧JOT車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OLDTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_OLDTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_28">
                    <!-- OT車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_OTTANKNUMBER_L" runat="server" Text="OT車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OTTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_OTTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- JXTG仙台車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_JXTGTANKNUMBER1_L" runat="server" Text="JXTG仙台車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JXTGTANKNUMBER1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_JXTGTANKNUMBER1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_29">
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

                <p id="KEY_LINE_30">
                    <!-- 出光昭シ車番 -->
                    <span class="ef">
                        <asp:Label ID="WF_SHELLTANKNUMBER_L" runat="server" Text="出光昭シ車番" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SHELLTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_SHELLTANKNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 予備 -->
                    <span class="ef">
                        <asp:Label ID="WF_RESERVE3_L" runat="server" Text="予備" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RESERVE3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_RESERVE3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_31">
                    <!-- 利用フラグ -->
                    <span class="ef">
                        <asp:Label ID="WF_USEDFLG_L" runat="server" Text="利用フラグ" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_USEDFLG', <%=LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY%>);" onchange="TextBox_change('WF_USEDFLG');">
                            <asp:TextBox ID="WF_USEDFLG" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_USEDFLG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <span></span>
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

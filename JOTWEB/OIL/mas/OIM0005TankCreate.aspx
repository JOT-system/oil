﻿<%@ Page Title="OIM0005C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0005TankCreate.aspx.vb" Inherits="JOTWEB.OIM0005TankCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0005WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0005CH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIM0005C.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0005C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0005C" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox" style="overflow-y: auto;">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <a>
                    <input type="button" id="WF_UPDATE" value="更新／追加" style="Width:7em" onclick="ButtonClick('WF_UPDATE');" />
                </a>
                <a>
                    <input type="button" id="WF_CLEAR" value="クリア" style="Width:7em" onclick="ButtonClick('WF_CLEAR');" />
                </a>
            </div>
            <div id="detailkeybox">
                <p id="KEY_LINE_1">
                    <!-- 選択No -->
                    <a>
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" Width="15em" CssClass="WF_TEXT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_2">
                    <!-- 削除フラグ -->
                    <a class="ef" ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('WF_DELFLG');">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除フラグ" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_DELFLG" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS BoxIcon"></asp:TextBox>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- JOT車番 -->
                    <a  class="ef" ondblclick="Field_DBclick('WF_TANKNUMBER', <%=LIST_BOX_CLASSIFICATION.LC_TANKNUMBER%>);" onchange="TextBox_change('WF_TANKNUMBER');">
                        <asp:Label ID="WF_TANKNUMBER_L" runat="server" Text="JOT車番" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_TANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS BoxIcon"></asp:TextBox>
                        <asp:Label ID="WF_TANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_3">
                    <!-- 原籍所有者C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_ORIGINOWNERCODE', <%=LIST_BOX_CLASSIFICATION.LC_ORIGINOWNER%>);" onchange="TextBox_change('WF_ORIGINOWNERCODE');">
                        <asp:Label ID="WF_ORIGINOWNERCODE_L" runat="server" Text="原籍所有者C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_ORIGINOWNERCODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_ORIGINOWNERCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 名義所有者C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_OWNERCODE', <%=LIST_BOX_CLASSIFICATION.LC_OWNER%>);" onchange="TextBox_change('WF_OWNERCODE');">
                        <asp:Label ID="WF_OWNERCODE_L" runat="server" Text="名義所有者C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_OWNERCODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OWNERCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_4">
                    <!-- リース先C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_LEASECODE', <%=LIST_BOX_CLASSIFICATION.LC_LEASE%>);" onchange="TextBox_change('WF_LEASECODE');">
                        <asp:Label ID="WF_LEASECODE_L" runat="server" Text="リース先C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_LEASECODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LEASECODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- リース区分C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_LEASECLASS', <%=LIST_BOX_CLASSIFICATION.LC_LEASECLASS%>);" onchange="TextBox_change('WF_LEASECLASS');">
                        <asp:Label ID="WF_LEASECLASS_L" runat="server" Text="リース区分C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_LEASECLASS" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LEASECLASS_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 自動延長 -->
                    <a class="ef">
                        <asp:Label ID="WF_AUTOEXTENTION_L" runat="server" Text="自動延長" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_AUTOEXTENTION" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_AUTOEXTENTION_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- リース開始年月日 -->
                    <a class="ef" ondblclick="Field_DBclick('WF_LEASESTYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_LEASESTYMD_L" runat="server" Text="リース開始年月日" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_LEASESTYMD" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_LEASESTYMD_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_6">
                    <!-- リース満了年月日 -->
                    <a class="ef" ondblclick="Field_DBclick('WF_LEASEENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_LEASEENDYMD_L" runat="server" Text="リース満了年月日" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_LEASEENDYMD" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_LEASEENDYMD_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 第三者使用者C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_USERCODE', <%=LIST_BOX_CLASSIFICATION.LC_THIRDUSER%>);" onchange="TextBox_change('WF_USERCODE');">
                        <asp:Label ID="WF_USERCODE_L" runat="server" Text="第三者使用者C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_USERCODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_USERCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 原常備駅C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_CURRENTSTATIONCODE', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_CURRENTSTATIONCODE');">
                        <asp:Label ID="WF_CURRENTSTATIONCODE_L" runat="server" Text="原常備駅C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_CURRENTSTATIONCODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_CURRENTSTATIONCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 臨時常備駅C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_EXTRADINARYSTATIONCODE', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_EXTRADINARYSTATIONCODE');">
                        <asp:Label ID="WF_EXTRADINARYSTATIONCODE_L" runat="server" Text="臨時常備駅C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_EXTRADINARYSTATIONCODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_EXTRADINARYSTATIONCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 第三者使用期限 -->
                    <a class="ef" ondblclick="Field_DBclick('WF_USERLIMIT', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_USERLIMIT_L" runat="server" Text="第三者使用期限" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_USERLIMIT" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_USERLIMIT_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 臨時常備駅期限 -->
                    <a class="ef" ondblclick="Field_DBclick('WF_LIMITTEXTRADIARYSTATION', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_LIMITTEXTRADIARYSTATION_L" runat="server" Text="臨時常備駅期限" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_LIMITTEXTRADIARYSTATION" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_LIMITTEXTRADIARYSTATION_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 原専用種別C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_DEDICATETYPECODE', <%=LIST_BOX_CLASSIFICATION.LC_DEDICATETYPE%>);" onchange="TextBox_change('WF_DEDICATETYPECODE');">
                        <asp:Label ID="WF_DEDICATETYPECODE_L" runat="server" Text="原専用種別C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_DEDICATETYPECODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DEDICATETYPECODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 臨時専用種別C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_EXTRADINARYTYPECODE', <%=LIST_BOX_CLASSIFICATION.LC_EXTRADINARYTYPE%>);" onchange="TextBox_change('WF_DEDICATETYPECODE');">
                        <asp:Label ID="WF_EXTRADINARYTYPECODE_L" runat="server" Text="臨時専用種別C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_EXTRADINARYTYPECODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_EXTRADINARYTYPECODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 臨時専用期限 -->
                    <a class="ef" ondblclick="Field_DBclick('WF_EXTRADINARYLIMIT', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_EXTRADINARYLIMIT_L" runat="server" Text="臨時専用期限" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_EXTRADINARYLIMIT" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_EXTRADINARYLIMIT_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 運用基地C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_OPERATIONBASECODE', <%=LIST_BOX_CLASSIFICATION.LC_BASE%>);" onchange="TextBox_change('WF_OPERATIONBASECODE');">
                        <asp:Label ID="WF_OPERATIONBASECODE_L" runat="server" Text="運用基地C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_OPERATIONBASECODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OPERATIONBASECODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 塗色C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_COLORCODE', <%=LIST_BOX_CLASSIFICATION.LC_COLOR%>);" onchange="TextBox_change('WF_COLORCODE');">
                        <asp:Label ID="WF_COLORCODE_L" runat="server" Text="塗色C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_COLORCODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_COLORCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- エネオス -->
                    <a class="ef">
                        <asp:Label ID="WF_ENEOS_L" runat="server" Text="エネオス" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_ENEOS" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_ENEOS_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_12">
                    <!-- エコレール -->
                    <a class="ef">
                        <asp:Label ID="WF_ECO_L" runat="server" Text="エコレール" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_ECO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_ECO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 取得年月日 -->
                    <a class="ef" ondblclick="Field_DBclick('WF_ALLINSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_ALLINSPECTIONDATE_L" runat="server" Text="取得年月日" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_ALLINSPECTIONDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_ALLINSPECTIONDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 車籍編入年月日 -->
                    <a class="ef" ondblclick="Field_DBclick('WF_TRANSFERDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_TRANSFERDATE_L" runat="server" Text="車籍編入年月日" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_TRANSFERDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_TRANSFERDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 取得先C -->
                    <a class="ef" ondblclick="Field_DBclick('WF_OBTAINEDCODE', <%=LIST_BOX_CLASSIFICATION.LC_OBTAINED%>);" onchange="TextBox_change('WF_OBTAINEDCODE');">
                        <asp:Label ID="WF_OBTAINEDCODE_L" runat="server" Text="取得先C" Width="10.0em" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_OBTAINEDCODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OBTAINEDCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_14">
                    <!-- 形式 -->
                    <a  class="ef" ondblclick="Field_DBclick('WF_MODEL', <%=LIST_BOX_CLASSIFICATION.LC_TANKMODEL%>);" onchange="TextBox_change('WF_MODEL');">
                        <asp:Label ID="WF_MODEL_L" runat="server" Text="形式" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_MODEL" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS BoxIcon"></asp:TextBox>
                        <asp:Label ID="WF_MODEL_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 形式カナ -->
                    <a class="ef">
                        <asp:Label ID="WF_MODELKANA_L" runat="server" Text="形式カナ" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MODELKANA" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_MODELKANA_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_15">
                    <!-- 荷重 -->
                    <a class="ef">
                        <asp:Label ID="WF_LOAD_L" runat="server" Text="荷重" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LOAD" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LOAD_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 荷重単位 -->
                    <a class="ef">
                        <asp:Label ID="WF_LOADUNIT_L" runat="server" Text="荷重単位" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LOADUNIT" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LOADUNIT_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_16">
                    <!-- 容積 -->
                    <a class="ef">
                        <asp:Label ID="WF_VOLUME_L" runat="server" Text="容積" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_VOLUME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_VOLUME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 容積単位 -->
                    <a class="ef">
                        <asp:Label ID="WF_VOLUMEUNIT_L" runat="server" Text="容積単位" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_VOLUMEUNIT" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_VOLUMEUNIT_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_17">
                    <!-- 原籍所有者 -->
                    <a class="ef">
                        <asp:Label ID="WF_ORIGINOWNERNAME_L" runat="server" Text="原籍所有者" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ORIGINOWNERNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_ORIGINOWNERNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 名義所有者 -->
                    <a class="ef">
                        <asp:Label ID="WF_OWNERNAME_L" runat="server" Text="名義所有者" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OWNERNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OWNERNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_18">
                    <!-- リース先 -->
                    <a class="ef">
                        <asp:Label ID="WF_LEASENAME_L" runat="server" Text="リース先" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LEASENAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LEASENAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- リース区分 -->
                    <a class="ef">
                        <asp:Label ID="WF_LEASECLASSNEMAE_L" runat="server" Text="リース区分" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LEASECLASSNEMAE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LEASECLASSNEMAE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_19">
                    <!-- 第三者使用者 -->
                    <a class="ef">
                        <asp:Label ID="WF_USERNAME_L" runat="server" Text="第三者使用者" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_USERNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_USERNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 原常備駅 -->
                    <a class="ef">
                        <asp:Label ID="WF_CURRENTSTATIONNAME_L" runat="server" Text="原常備駅" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_CURRENTSTATIONNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_CURRENTSTATIONNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_20">
                    <!-- 臨時常備駅 -->
                    <a class="ef">
                        <asp:Label ID="WF_EXTRADINARYSTATIONNAME_L" runat="server" Text="臨時常備駅" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_EXTRADINARYSTATIONNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_EXTRADINARYSTATIONNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 原専用種別 -->
                    <a class="ef">
                        <asp:Label ID="WF_DEDICATETYPENAME_L" runat="server" Text="原専用種別" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_DEDICATETYPENAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DEDICATETYPENAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_21">
                    <!-- 臨時専用種別 -->
                    <a class="ef">
                        <asp:Label ID="WF_EXTRADINARYTYPENAME_L" runat="server" Text="臨時専用種別" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_EXTRADINARYTYPENAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_EXTRADINARYTYPENAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 運用場所 -->
                    <a class="ef">
                        <asp:Label ID="WF_OPERATIONBASENAME_L" runat="server" Text="運用場所" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OPERATIONBASENAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OPERATIONBASENAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_22">
                    <!-- 塗色 -->
                    <a class="ef">
                        <asp:Label ID="WF_COLORNAME_L" runat="server" Text="塗色" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_COLORNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_COLORNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 予備1 -->
                    <a class="ef">
                        <asp:Label ID="WF_RESERVE1_L" runat="server" Text="予備1" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RESERVE1" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_RESERVE1_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_23">
                    <!-- 予備2 -->
                    <a class="ef">
                        <asp:Label ID="WF_RESERVE2_L" runat="server" Text="予備2" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RESERVE2" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_RESERVE2_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 次回指定年月日 -->
                    <a class="ef" ondblclick="Field_DBclick('WF_SPECIFIEDDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_SPECIFIEDDATE_L" runat="server" Text="次回指定年月日" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_SPECIFIEDDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_SPECIFIEDDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_24">
                    <!-- 次回全検年月日(JR)  -->
                    <a class="ef" ondblclick="Field_DBclick('WF_JRALLINSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_JRALLINSPECTIONDATE_L" runat="server" Text="次回全検年月日(JR) " Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_JRALLINSPECTIONDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_JRALLINSPECTIONDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 現在経年 -->
                    <a class="ef">
                        <asp:Label ID="WF_PROGRESSYEAR_L" runat="server" Text="現在経年" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_PROGRESSYEAR" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_PROGRESSYEAR_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_25">
                    <!-- 次回全検時経年 -->
                    <a class="ef">
                        <asp:Label ID="WF_NEXTPROGRESSYEAR_L" runat="server" Text="次回全検時経年" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_NEXTPROGRESSYEAR" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_NEXTPROGRESSYEAR_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 次回交検年月日(JR） -->
                    <a class="ef" ondblclick="Field_DBclick('WF_JRINSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_JRINSPECTIONDATE_L" runat="server" Text="次回交検年月日(JR）" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_JRINSPECTIONDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_JRINSPECTIONDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_26">
                    <!-- 次回交検年月日 -->
                    <a class="ef" ondblclick="Field_DBclick('WF_INSPECTIONDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_INSPECTIONDATE_L" runat="server" Text="次回交検年月日" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_INSPECTIONDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_INSPECTIONDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 次回指定年月日(JR) -->
                    <a class="ef" ondblclick="Field_DBclick('WF_JRSPECIFIEDDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:Label ID="WF_JRSPECIFIEDDATE_L" runat="server" Text="次回指定年月日(JR)" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_JRSPECIFIEDDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS CalendarIcon"></asp:TextBox>
                        <asp:Label ID="WF_JRSPECIFIEDDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_27">
                    <!-- JR車番 -->
                    <a class="ef">
                        <asp:Label ID="WF_JRTANKNUMBER_L" runat="server" Text="JR車番" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JRTANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_JRTANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 旧JOT車番 -->
                    <a class="ef">
                        <asp:Label ID="WF_OLDTANKNUMBER_L" runat="server" Text="旧JOT車番" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OLDTANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OLDTANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_28">
                    <!-- OT車番 -->
                    <a class="ef">
                        <asp:Label ID="WF_OTTANKNUMBER_L" runat="server" Text="OT車番" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OTTANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OTTANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- JXTG車番 -->
                    <a class="ef">
                        <asp:Label ID="WF_JXTGTANKNUMBER_L" runat="server" Text="JXTG車番" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JXTGTANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_JXTGTANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_29">
                    <!-- コスモ車番 -->
                    <a class="ef">
                        <asp:Label ID="WF_COSMOTANKNUMBER_L" runat="server" Text="コスモ車番" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_COSMOTANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_COSMOTANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 富士石油車番 -->
                    <a class="ef">
                        <asp:Label ID="WF_FUJITANKNUMBER_L" runat="server" Text="富士石油車番" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_FUJITANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_FUJITANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_30">
                    <!-- 出光昭シ車番 -->
                    <a class="ef">
                        <asp:Label ID="WF_SHELLTANKNUMBER_L" runat="server" Text="出光昭シ車番" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_SHELLTANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_SHELLTANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>

                    <!-- 予備 -->
                    <a class="ef">
                        <asp:Label ID="WF_RESERVE3_L" runat="server" Text="予備" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RESERVE3" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_RESERVE3_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
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

<%@ Page Title="OIS0001C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIS0001UserCreate.aspx.vb" Inherits="JOTWEB.OIS0001UserCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIS0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIS0001CH" ContentPlaceHolderID="head" runat="server">
    <!-- <link href='<%=ResolveUrl("~/OIL/css/OIS0001C.css")%>' rel="stylesheet" type="text/css" /> -->
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIS0001C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIS0001C" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox" >
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="表更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR" class="btn-sticky" value="クリア"  onclick="ButtonClick('WF_CLEAR');" />
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
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('WF_DELFLG');">
                            <asp:TextBox ID="WF_DELFLG" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 画面ＩＤ -->
                    <span class="ef" style="display:none;">
                        <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ＩＤ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_MAPID" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_MAPID_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_3">
                    <!-- ユーザID -->
                    <span class="ef">
                        <asp:Label ID="WF_USERID_L" runat="server" Text="ユーザID" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_USERID" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_USERID_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 社員名（短） -->
                    <span class="ef">
                        <asp:Label ID="WF_STAFFNAMES_L" runat="server" Text="社員名（短）" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_STAFFNAMES" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_STAFFNAMES_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 社員名（長） -->
                    <span class="ef">
                        <asp:Label ID="WF_STAFFNAMEL_L" runat="server" Text="社員名（長）" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_STAFFNAMEL" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                        <asp:Label ID="WF_STAFFNAMEL_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 誤り回数 -->
                    <span class="ef">
                        <asp:Label ID="WF_MISSCNT_L" runat="server" Text="誤り回数" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_MISSCNT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                        <asp:Label ID="WF_MISSCNT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- パスワード -->
                    <span class="ef">
                        <asp:Label ID="WF_PASSWORD_L" runat="server" Text="パスワード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_PASSWORD" runat="server" CssClass="WF_TEXTBOX_CSS" TextMode="Password" MaxLength="200"></asp:TextBox>
                        <asp:Label ID="WF_PASSWORD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                   </span>

                    <!-- パスワード有効期限 -->
                    <span class="ef">
                        <asp:Label ID="WF_PASSENDYMD_L" runat="server" Text="パスワード有効期限" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_PASSENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_PASSENDYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_PASSENDYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 開始年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_STYMD_L" runat="server" Text="開始年月日" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_STYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_STYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 終了年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_ENDYMD_L" runat="server" Text="終了年月日" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_ENDYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ENDYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 会社コード -->
                    <span class="ef" >
                        <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                            <asp:TextBox ID="WF_CAMPCODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    </span>

                    <!-- 組織コード -->
                    <span class="ef">
                        <asp:Label ID="WF_ORG_L" runat="server" Text="組織コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
                            <asp:TextBox ID="WF_ORG" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ORG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- メールアドレス -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_EMAIL_L" runat="server" Text="メールアドレス" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_EMAIL" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="128"></asp:TextBox>
                        <!-- <asp:Label ID="WF_EMAIL_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label> -->
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- メニュー表示制御ロール -->
                    <span class="ef" >
                        <asp:Label ID="WF_MENUROLE_L" runat="server" Text="メニュー表示制御ロール" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_MENUROLE', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_MENUROLE');">
                            <asp:TextBox ID="WF_MENUROLE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_MENUROLE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 画面参照更新制御ロール -->
                    <span class="ef" >
                        <asp:Label ID="WF_MAPROLE_L" runat="server" Text="画面参照更新制御ロール"  CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_MAPROLE', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_MAPROLE');">
                            <asp:TextBox ID="WF_MAPROLE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_MAPROLE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 画面表示項目制御ロール -->
                    <span class="ef">
                        <asp:Label ID="WF_VIEWPROFID_L" runat="server" Text="画面表示項目制御ロール" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_VIEWPROFID', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_VIEWPROFID');">
                            <asp:TextBox ID="WF_VIEWPROFID" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>

                        <asp:Label ID="WF_VIEWPROFID_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- エクセル出力制御ロール -->
                    <span class="ef">
                        <asp:Label ID="WF_RPRTPROFID_L" runat="server" Text="エクセル出力制御ロール" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_RPRTPROFID', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_RPRTPROFID');">
                            <asp:TextBox ID="WF_RPRTPROFID" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_RPRTPROFID_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 画面初期値ロール -->
                    <span class="ef">
                        <asp:Label ID="WF_VARIANT_L" runat="server" Text="画面初期値ロール" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_VARIANT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_VARIANT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 承認権限ロール -->
                    <span class="ef">
                        <asp:Label ID="WF_APPROVALID_L" runat="server" Text="承認権限ロール" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_APPROVALID', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_APPROVALID');">
                            <asp:TextBox ID="WF_APPROVALID" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_APPROVALID_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
            
                <p id="12">
                    <!-- 情報出力ID1 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID1_L" runat="server" Text="情報出力ID1" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID1', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID1');">
                            <asp:TextBox ID="WF_OUTPUTID1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="13">
                    <!-- 表示フラグ1 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF1_L" runat="server" Text="表示フラグ1" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF1', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF1');">
                            <asp:TextBox ID="WF_ONOFF1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順1 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO1_L" runat="server" Text="表示順1" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="14">
                    <!-- 情報出力ID2 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID2_L" runat="server" Text="情報出力ID2" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID2');">
                            <asp:TextBox ID="WF_OUTPUTID2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="15">
                    <!-- 表示フラグ2 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF2_L" runat="server" Text="表示フラグ2" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF2');">
                            <asp:TextBox ID="WF_ONOFF2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順2 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO2_L" runat="server" Text="表示順2" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="16">
                    <!-- 情報出力ID3 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID3_L" runat="server" Text="情報出力ID3" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID3', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID3');">
                            <asp:TextBox ID="WF_OUTPUTID3" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="17">
                    <!-- 表示フラグ3 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF3_L" runat="server" Text="表示フラグ3" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF3', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF3');">
                            <asp:TextBox ID="WF_ONOFF3" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順3 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO3_L" runat="server" Text="表示順3" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="18">
                    <!-- 情報出力ID4 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID4_L" runat="server" Text="情報出力ID4" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID4', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID4');">
                            <asp:TextBox ID="WF_OUTPUTID4" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID4_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="19">
                    <!-- 表示フラグ4 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF4_L" runat="server" Text="表示フラグ4" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF4', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF4');">
                            <asp:TextBox ID="WF_ONOFF4" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF4_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順4 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO4_L" runat="server" Text="表示順4" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO4" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="20">
                    <!-- 情報出力ID5 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID5_L" runat="server" Text="情報出力ID5" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID5', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID5');">
                            <asp:TextBox ID="WF_OUTPUTID5" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID5_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="21">
                    <!-- 表示フラグ5 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF5_L" runat="server" Text="表示フラグ5" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF5', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF5');">
                            <asp:TextBox ID="WF_ONOFF5" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF5_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順5 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO5_L" runat="server" Text="表示順5" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO5" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="22">
                    <!-- 情報出力ID6 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID6_L" runat="server" Text="情報出力ID6" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID6', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID6');">
                            <asp:TextBox ID="WF_OUTPUTID6" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID6_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="23">
                    <!-- 表示フラグ6 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF6_L" runat="server" Text="表示フラグ6" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF6', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF6');">
                            <asp:TextBox ID="WF_ONOFF6" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF6_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順6 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO6_L" runat="server" Text="表示順6" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO6" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="24">
                    <!-- 情報出力ID7 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID7_L" runat="server" Text="情報出力ID7" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID7', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID7');">
                            <asp:TextBox ID="WF_OUTPUTID7" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID7_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="25">
                    <!-- 表示フラグ7 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF7_L" runat="server" Text="表示フラグ7" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF7', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF7');">
                            <asp:TextBox ID="WF_ONOFF7" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF7_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順7 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO7_L" runat="server" Text="表示順7" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO7" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="26">
                    <!-- 情報出力ID8 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID8_L" runat="server" Text="情報出力ID8" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID8', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID8');">
                            <asp:TextBox ID="WF_OUTPUTID8" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID8_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="27">
                    <!-- 表示フラグ8 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF8_L" runat="server" Text="表示フラグ8" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF8', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF8');">
                            <asp:TextBox ID="WF_ONOFF8" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF8_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順8 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO8_L" runat="server" Text="表示順8" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO8" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="28">
                    <!-- 情報出力ID9 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID9_L" runat="server" Text="情報出力ID9" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID9', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID9');">
                            <asp:TextBox ID="WF_OUTPUTID9" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID9_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="29">
                    <!-- 表示フラグ9 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF9_L" runat="server" Text="表示フラグ9" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF9', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF9');">
                            <asp:TextBox ID="WF_ONOFF9" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF9_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順9 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO9_L" runat="server" Text="表示順9" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO9" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="30">
                    <!-- 情報出力ID10 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID10_L" runat="server" Text="情報出力ID10" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID10', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID10');">
                            <asp:TextBox ID="WF_OUTPUTID10" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID10_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="31">
                    <!-- 表示フラグ10 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF10_L" runat="server" Text="表示フラグ10" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF10', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF10');">
                            <asp:TextBox ID="WF_ONOFF10" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF10_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順10 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO10_L" runat="server" Text="表示順10" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO10" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="32">
                    <!-- 情報出力ID11 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID11_L" runat="server" Text="情報出力ID11" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID11', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID11');">
                            <asp:TextBox ID="WF_OUTPUTID11" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID11_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="33">
                    <!-- 表示フラグ11 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF11_L" runat="server" Text="表示フラグ11" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF11', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF11');">
                            <asp:TextBox ID="WF_ONOFF11" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF11_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順11 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO11_L" runat="server" Text="表示順11" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO11" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="34">
                    <!-- 情報出力ID12 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID12_L" runat="server" Text="情報出力ID12" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID12', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID12');">
                            <asp:TextBox ID="WF_OUTPUTID12" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID12_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="35">
                    <!-- 表示フラグ12 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF12_L" runat="server" Text="表示フラグ12" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF12', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF12');">
                            <asp:TextBox ID="WF_ONOFF12" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF12_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順12 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO12_L" runat="server" Text="表示順12" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO12" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="36">
                    <!-- 情報出力ID13 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID13_L" runat="server" Text="情報出力ID13" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID13', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID13');">
                            <asp:TextBox ID="WF_OUTPUTID13" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID13_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="37">
                    <!-- 表示フラグ13 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF13_L" runat="server" Text="表示フラグ13" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF13', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF13');">
                            <asp:TextBox ID="WF_ONOFF13" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF13_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順13 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO13_L" runat="server" Text="表示順13" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO13" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="38">
                    <!-- 情報出力ID14 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID14_L" runat="server" Text="情報出力ID14" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID14', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID14');">
                            <asp:TextBox ID="WF_OUTPUTID14" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID14_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="39">
                    <!-- 表示フラグ14 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF14_L" runat="server" Text="表示フラグ14" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF14', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF14');">
                            <asp:TextBox ID="WF_ONOFF14" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF14_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順14 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO14_L" runat="server" Text="表示順14" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO14" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="40">
                    <!-- 情報出力ID15 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID15_L" runat="server" Text="情報出力ID15" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID15', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID15');">
                            <asp:TextBox ID="WF_OUTPUTID15" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID15_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="41">
                    <!-- 表示フラグ15 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF15_L" runat="server" Text="表示フラグ15" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF15', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF15');">
                            <asp:TextBox ID="WF_ONOFF15" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF15_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順15 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO15_L" runat="server" Text="表示順15" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO15" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="42">
                    <!-- 情報出力ID16 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID16_L" runat="server" Text="情報出力ID16" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID16', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID16');">
                            <asp:TextBox ID="WF_OUTPUTID16" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID16_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="43">
                    <!-- 表示フラグ16 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF16_L" runat="server" Text="表示フラグ16" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF16', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF16');">
                            <asp:TextBox ID="WF_ONOFF16" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF16_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順16 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO16_L" runat="server" Text="表示順16" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO16" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="44">
                    <!-- 情報出力ID17 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID17_L" runat="server" Text="情報出力ID17" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID17', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID17');">
                            <asp:TextBox ID="WF_OUTPUTID17" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID17_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="45">
                    <!-- 表示フラグ17 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF17_L" runat="server" Text="表示フラグ17" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF17', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF17');">
                            <asp:TextBox ID="WF_ONOFF17" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF17_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順17 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO17_L" runat="server" Text="表示順17" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO17" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="46">
                    <!-- 情報出力ID18 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID18_L" runat="server" Text="情報出力ID18" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID18', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID18');">
                            <asp:TextBox ID="WF_OUTPUTID18" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID18_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="47">
                    <!-- 表示フラグ18 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF18_L" runat="server" Text="表示フラグ18" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF18', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF18');">
                            <asp:TextBox ID="WF_ONOFF18" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF18_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順18 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO18_L" runat="server" Text="表示順18" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO18" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="48">
                    <!-- 情報出力ID19 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID19_L" runat="server" Text="情報出力ID19" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID19', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID19');">
                            <asp:TextBox ID="WF_OUTPUTID19" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID19_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="49">
                    <!-- 表示フラグ19 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF19_L" runat="server" Text="表示フラグ19" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF19', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF19');">
                            <asp:TextBox ID="WF_ONOFF19" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF19_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順19 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO19_L" runat="server" Text="表示順19" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO19" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="50">
                    <!-- 情報出力ID20 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID20_L" runat="server" Text="情報出力ID20" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID20', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID20');">
                            <asp:TextBox ID="WF_OUTPUTID20" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID20_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="51">
                    <!-- 表示フラグ20 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF20_L" runat="server" Text="表示フラグ20" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF20', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF20');">
                            <asp:TextBox ID="WF_ONOFF20" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF20_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順20 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO20_L" runat="server" Text="表示順20" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO20" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="52">
                    <!-- 情報出力ID21 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID21_L" runat="server" Text="情報出力ID21" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID21', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID21');">
                            <asp:TextBox ID="WF_OUTPUTID21" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID21_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="53">
                    <!-- 表示フラグ21 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF21_L" runat="server" Text="表示フラグ21" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF21', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF21');">
                            <asp:TextBox ID="WF_ONOFF21" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF21_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順21 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO21_L" runat="server" Text="表示順21" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO21" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="54">
                    <!-- 情報出力ID22 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID22_L" runat="server" Text="情報出力ID22" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID22', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID22');">
                            <asp:TextBox ID="WF_OUTPUTID22" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID22_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="55">
                    <!-- 表示フラグ22 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF22_L" runat="server" Text="表示フラグ22" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF22', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF22');">
                            <asp:TextBox ID="WF_ONOFF22" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF22_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順22 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO22_L" runat="server" Text="表示順22" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO22" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="56">
                    <!-- 情報出力ID23 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID23_L" runat="server" Text="情報出力ID23" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID23', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID23');">
                            <asp:TextBox ID="WF_OUTPUTID23" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID23_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="57">
                    <!-- 表示フラグ23 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF23_L" runat="server" Text="表示フラグ23" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF23', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF23');">
                            <asp:TextBox ID="WF_ONOFF23" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF23_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順23 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO23_L" runat="server" Text="表示順23" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO23" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="58">
                    <!-- 情報出力ID24 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID24_L" runat="server" Text="情報出力ID24" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID24', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID24');">
                            <asp:TextBox ID="WF_OUTPUTID24" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID24_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="59">
                    <!-- 表示フラグ24 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF24_L" runat="server" Text="表示フラグ24" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF24', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF24');">
                            <asp:TextBox ID="WF_ONOFF24" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF24_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順24 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO24_L" runat="server" Text="表示順24" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO24" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>
                <p id="60">
                    <!-- 情報出力ID25 -->
                    <span class="ef">
                        <asp:Label ID="WF_OUTPUTID25_L" runat="server" Text="情報出力ID25" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OUTPUTID25', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OUTPUTID25');">
                            <asp:TextBox ID="WF_OUTPUTID25" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OUTPUTID25_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="61">
                    <!-- 表示フラグ25 -->
                    <span class="ef">
                        <asp:Label ID="WF_ONOFF25_L" runat="server" Text="表示フラグ25" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ONOFF25', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ONOFF25');">
                            <asp:TextBox ID="WF_ONOFF25" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ONOFF25_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 表示順25 -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_SORTNO25_L" runat="server" Text="表示順25" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORTNO25" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
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

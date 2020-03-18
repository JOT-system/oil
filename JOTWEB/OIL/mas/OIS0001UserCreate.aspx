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

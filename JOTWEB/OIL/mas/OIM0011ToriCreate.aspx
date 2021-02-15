<%@ Page Title="OIM0011C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0011ToriCreate.aspx.vb" Inherits="JOTWEB.OIM0011TORICreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0011WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0011CH" ContentPlaceHolderID="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0011C.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0011C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0011C" ContentPlaceHolderID="contents1" runat="server">
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

                    <!-- 取引先コード -->
                    <span class="ef">
                        <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_TORICODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                        <asp:Label ID="WF_TORICODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_3">
                    <!-- 開始年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_STYMD_L" runat="server" Text="開始年月日" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_STYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_STYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 終了年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_ENDYMD_L" runat="server" Text="終了年月日" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="WF_ENDYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ENDYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 取引先名称 -->
                    <span class="ef">
                        <asp:Label ID="WF_TORINAME_L" runat="server" Text="取引先名称" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_TORINAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="100"></asp:TextBox>
                        <asp:Label ID="WF_TORINAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 取引先略称 -->
                    <span class="ef">
                        <asp:Label ID="WF_TORINAMES_L" runat="server" Text="取引先略称" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_TORINAMES" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                        <asp:Label ID="WF_TORINAMES_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 取引先カナ名称 -->
                    <span class="ef">
                        <asp:Label ID="WF_TORINAMEKANA_L" runat="server" Text="取引先カナ名称" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_TORINAMEKANA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="100"></asp:TextBox>
                        <asp:Label ID="WF_TORINAMEKANA_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 部門名称 -->
                    <span class="ef">
                        <asp:Label ID="WF_DEPTNAME_L" runat="server" Text="部門名称" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_DEPTNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_DEPTNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 郵便番号（上） -->
                    <span class="ef">
                        <asp:Label ID="WF_POSTNUM1_L" runat="server" Text="郵便番号（上）" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_POSTNUM1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                        <asp:Label ID="WF_POSTNUM1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 郵便番号（下） -->
                    <span class="ef">
                        <asp:Label ID="WF_POSTNUM2_L" runat="server" Text="郵便番号（下）" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_POSTNUM2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_POSTNUM2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 住所１ -->
                    <span class="ef">
                        <asp:Label ID="WF_ADDR1_L" runat="server" Text="住所１" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ADDR1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="120"></asp:TextBox>
                        <asp:Label ID="WF_ADDR1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 住所２ -->
                    <span class="ef">
                        <asp:Label ID="WF_ADDR2_L" runat="server" Text="住所２" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ADDR2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="120"></asp:TextBox>
                        <asp:Label ID="WF_ADDR2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 住所３ -->
                    <span class="ef">
                        <asp:Label ID="WF_ADDR3_L" runat="server" Text="住所３" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ADDR3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="120"></asp:TextBox>
                        <asp:Label ID="WF_ADDR3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 住所４ -->
                    <span class="ef">
                        <asp:Label ID="WF_ADDR4_L" runat="server" Text="住所４" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ADDR4" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="120"></asp:TextBox>
                        <asp:Label ID="WF_ADDR4_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 電話番号 -->
                    <span class="ef">
                        <asp:Label ID="WF_TEL_L" runat="server" Text="電話番号" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_TEL" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="15"></asp:TextBox>
                        <asp:Label ID="WF_TEL_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- ＦＡＸ番号 -->
                    <span class="ef">
                        <asp:Label ID="WF_FAX_L" runat="server" Text="ＦＡＸ番号" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_FAX" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="15"></asp:TextBox>
                        <asp:Label ID="WF_FAX_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- メールアドレス -->
                    <span class="ef">
                        <asp:Label ID="WF_MAIL_L" runat="server" Text="メールアドレス" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MAIL" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="128"></asp:TextBox>
                        <asp:Label ID="WF_MAIL_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 石油利用フラグ -->
                    <span class="ef">
                        <asp:Label ID="WF_OILUSEFLG_L" runat="server" Text="石油利用フラグ" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OILUSEFLG', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OILUSEFLG');">
                            <asp:TextBox ID="WF_OILUSEFLG" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OILUSEFLG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 銀行コード -->
                    <span class="ef">
                        <asp:Label ID="WF_BANKCODE_L" runat="server" Text="銀行コード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_BANKCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_BANKCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 支店コード -->
                    <span class="ef">
                        <asp:Label ID="WF_BANKBRANCHCODE_L" runat="server" Text="支店コード" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_BANKBRANCHCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_BANKBRANCHCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_12">
                    <!-- 口座種別 -->
                    <span class="ef">
                        <asp:Label ID="WF_ACCOUNTTYPE_L" runat="server" Text="口座種別" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ACCOUNTTYPE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_ACCOUNTTYPE');">
                            <asp:TextBox ID="WF_ACCOUNTTYPE" runat="server" readOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ACCOUNTTYPE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 口座番号 -->
                    <span class="ef">
                        <asp:Label ID="WF_ACCOUNTNUMBER_L" runat="server" Text="口座番号" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ACCOUNTNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                        <asp:Label ID="WF_ACCOUNTNUMBER_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 口座名義 -->
                    <span class="ef">
                        <asp:Label ID="WF_ACCOUNTNAME_L" runat="server" Text="口座名義" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ACCOUNTNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="30"></asp:TextBox>
                        <asp:Label ID="WF_ACCOUNTNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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

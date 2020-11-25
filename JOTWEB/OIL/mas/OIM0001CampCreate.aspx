<%@ Page Title="OIM0001C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0001CampCreate.aspx.vb" Inherits="JOTWEB.OIM0001CampCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0001CH" ContentPlaceHolderID="head" runat="server">
    <%--<link href='<%=ResolveUrl("~/OIL/css/OIM0001C.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0001C.js")%>'></script>
    <script type="text/javascript">
       var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID = "OIM0001C" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id = "detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="表更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR" class="btn-sticky" value="クリア"  onclick="ButtonClick('WF_CLEAR');" />
                    </div>
                </div>
            </div>

            <!-- 会社コード -->
            <div class="inputItem" style="display:none">
                <a id="LblCampCodeMy" class="requiredMark">会社コード</a>
                <a class="ef" id="WF_CAMPCODE_MY" ondblclick="Field_DBclick('WF_CAMPCODE_MY', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE_MY');">
                    <asp:TextBox ID="TxtCampCodeMy" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_CAMPNAME_MY">
                    <asp:Label ID="txtCampNameMy" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 運用部署 -->
            <div class="inputItem" style="display:none">
                <a id="LblOrgCodeMy" class="requiredMark">運用部署</a>
                <a class="ef" id="WF_ORGCODE_MY" style="display:none" ondblclick="Field_DBclick('WF_ORGCODE_MY', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORGCODE_MY');">
                    <asp:TextBox ID="TxtOrgCodeMy" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>TxtCampCode
                <a id="WF_ORGNAME_MY" style="display:none">
                    <asp:Label ID="txtOrgNameMy" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <div id="detailkeybox">


                <p id="KEY_LINE_1">
                    <!-- 選択No -->
                    <span>
                        <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LEFT  requiredMark"></asp:Label>
                        <asp:Label ID="WF_Sel_LINECNT" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_2">
                    <!-- 削除フラグ -->
                    <span class="ef">
                        <asp:Label ID="LblDelFlg" runat="server" Text="削除フラグ" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('WF_DELFLG');">
                        <asp:TextBox ID="TxtDelFlg" runat="server" ReadOnly="true" CssClass="WF_TEXTBOX_CSS boxIcon iconOnly" MaxLength="1"></asp:TextBox></span>
                        <asp:Label ID="LblDelFlgText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_3">
                    <!-- 会社コード -->
                    <span class="ef" id="WF_CAMPCODE">
                        <asp:Label ID="LblCampCode" runat="server" Text="会社コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                        <asp:TextBox ID="TxtCampCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox></span>
                        <asp:Label ID="LblCampCodetext" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 開始年月日 -->
                    <span class="ef" id="WF_STYMD">
                        <asp:Label ID="LblStymd" runat="server" Text="開始年月日" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtStymd" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="LblStymdText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 終了年月日 -->
                    <span class="ef" id="WF_ENDYMD">
                        <asp:Label ID="LblEndymd" runat="server" Text="終了年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtEndymd" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="LblEndymdText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>


                <p id="KEY_LINE_6">
                    <!-- 会社名称 -->
                    <span class="ef" id="WF_NAME">
                        <asp:Label ID="LblName" runat="server" Text="会社名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                        <asp:Label ID="LblNameText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>


                <p id="KEY_LINE_6">
                    <!-- 会社名称（短） -->
                    <span class="ef" id="WF_NAMES">
                        <asp:Label ID="LblNames" runat="server" Text="会社名称（短）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtNames" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="LblTypeNameText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 会社カナ名称 -->
                    <span class="ef" id="WF_NAMEKANA">
                        <asp:Label ID="LblNameKana" runat="server" Text="会社カナ名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtNameKana" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                        <asp:Label ID="LblNameKanaText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 会社カナ名称（短） -->
                    <span class="ef" id="WF_NAMEKANAS">
                        <asp:Label ID="LblNameKanas" runat="server" Text="会社カナ名称（短）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtNameKanas" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="LblNameKanasText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 郵便番号（上） -->
                    <span class="ef" id="WF_POSTNAME1">
                        <asp:Label ID="LblPostName1" runat="server" Text="郵便番号（上）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span>
                        <asp:TextBox ID="TxtPostName1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                        <asp:Label ID="LblPostName1Text" runat="server" Text="―" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                     <!-- 郵便番号（下） -->
                        <asp:TextBox ID="TxtPostName2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
            　          </span>
                    </span>
                </p>


                <p id="KEY_LINE_10">
                    <!-- 住所１ -->
                    <span class="ef" id="WF_ADDR1">
                        <asp:Label ID="LblAddr1" runat="server" Text="住所" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtAddr1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="120"></asp:TextBox>
                        <asp:Label ID="LblAddr1Text" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 住所２ -->
                    <span class="ef" id="WF_ADDR2">
                        <asp:Label ID="LblAddr2" runat="server" Text="住所２" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtAddr2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="120"></asp:TextBox>
                        <asp:Label ID="LblAddr2Text" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_12">
                    <!-- 住所３ -->
                    <span class="ef" id="WF_ADDR3">
                        <asp:Label ID="LblAddr3" runat="server" Text="住所３" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtAddr3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="120"></asp:TextBox>
                        <asp:Label ID="LblAddr3Text" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 住所４ -->
                    <span class="ef" id="WF_ADDR4">
                        <asp:Label ID="LblAddr4" runat="server" Text="住所４" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtAddr4" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="120"></asp:TextBox>
                        <asp:Label ID="LblAddr4Text" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_14">
                    <!-- 電話番号 -->
                    <span class="ef" id="WF_TEL">
                        <asp:Label ID="LblTel" runat="server" Text="電話番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtTel" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="15"></asp:TextBox>
                        <asp:Label ID="LblTelText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- FAX番号 -->
                    <span class="ef" id="WF_FAX">
                        <asp:Label ID="LblFax" runat="server" Text="ＦＡＸ番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtFax" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="15"></asp:TextBox>
                        <asp:Label ID="LblFaxText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>


                <p id="KEY_LINE_15">
                    <!-- メールアドレス -->
                    <span class="ef" id="WF_MAIL">
                        <asp:Label ID="LblMail" runat="server" Text="メールアドレス" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtMail" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="128"></asp:TextBox>
                        <asp:Label ID="LblMailText" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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
 
        </span>
 
</asp:Content>

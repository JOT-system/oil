<%@ Page Title="OIT0002L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0002LinkList.aspx.vb" Inherits="JOTWEB.OIT0002UserList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0002LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0002L.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0002L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIT0002L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="Operation" style="margin-left: 3em; margin-top: 0.5em; height: 1.8em;">
                <!-- 会社コード -->
                <a style="display:none;">
                    <asp:Label ID="WF_SEL_CAMPCODE" runat="server" Text="会社" Font-Bold="True" Font-Underline="false"></asp:Label>
                    <asp:Label ID="WF_SEL_CAMPNAME" runat="server" Width="12em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>

                <!-- 組織コード -->
                <a style="display:none;">
                    <asp:Label ID="WF_SEL_ORG" runat="server" Text="運用組織" Font-Bold="True" Font-Underline="false"></asp:Label>
                    <asp:Label ID="WF_SEL_ORGNAME" runat="server" Width="12em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>

                <!-- ボタン -->
                <a style="position:fixed;top:2.8em;left:23em;">
                    <input type="button" id="WF_ButtonALLSELECT" value="全選択"  style="Width:5em" onclick="ButtonClick('WF_ButtonALLSELECT');" />
                </a>
                <a style="position:fixed;top:2.8em;left:27.5em;">
                    <input type="button" id="WF_ButtonALLCANCEL" value="選択解除"  style="Width:5em" onclick="ButtonClick('WF_ButtonALLCANCEL');" />
                </a>
                <a style="position:fixed;top:2.8em;left:49em;">
                    <input type="button" id="WF_ButtonINSERT" value="行削除"  style="Width:5em" onclick="ButtonClick('WF_ButtonINSERT');" />
                </a>
                <a style="position:fixed;top:2.8em;left:53.5em;">
                    <input type="button" id="WF_ButtonUPDATE" value="DB更新"  style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
                </a>
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ"  style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.2em;left:75em;">
                    <asp:Image ID="WF_ButtonFIRST2" runat="server" ImageUrl="~/img/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST2" runat="server" ImageUrl="~/img/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
            </div>
                <div id="divListArea">
                    <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
                </div>
        </div>

        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <a>
                    <input type="button" id="WF_UPDATE" value="表更新" style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
                </a>
                <a>
                    <input type="button" id="WF_CLEAR" value="クリア" style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
                </a>
            </div>

            <div class="detailkeybox">
            <p id="KEY_LINE_1">
                <!-- 選択No -->
                <a>
                    <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_Sel_LINECNT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_2">
                <!-- 削除フラグ -->
                <a ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_3">
                <!-- ユーザID -->
                <a>
                    <asp:Label ID="WF_USERID_L" runat="server" Text="ユーザID" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_USERID" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_USERID_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- 社員名（短） -->
                <a>
                    <asp:Label ID="WF_STAFFNAMES_L" runat="server" Text="社員名（短）" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_STAFFNAMES" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_STAFFNAMES_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_4">
                <!-- 社員名（長） -->
                <a>
                    <asp:Label ID="WF_STAFFNAMEL_L" runat="server" Text="社員名（長）" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_STAFFNAMEL" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_STAFFNAMEL_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- 画面ＩＤ -->
                <a>
                    <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ＩＤ" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_MAPID" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_MAPID_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_5">
                <!-- パスワード -->
                <a>
                    <asp:Label ID="WF_PASSWORD_L" runat="server" Text="パスワード" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_PASSWORD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" TextMode="Password"></asp:TextBox>
                    <asp:Label ID="WF_PASSWORD_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    
               </a>

                <!-- 誤り回数 -->
                <a>
                    <asp:Label ID="WF_MISSCNT_L" runat="server" Text="誤り回数" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_MISSCNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_MISSCNT_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_6">
                <!-- パスワード有効期限 -->
                <a ondblclick="Field_DBclick('WF_PASSENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                    <asp:Label ID="WF_PASSENDYMD_L" runat="server" Text="パスワード有効期限" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_PASSENDYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_PASSENDYMD_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_7">
                <!-- 開始年月日 -->
                <a ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                    <asp:Label ID="WF_STYMD_L" runat="server" Text="開始年月日" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_STYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_STYMD_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- 終了年月日 -->
                <a ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                    <asp:Label ID="WF_ENDYMD_L" runat="server" Text="終了年月日" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_ENDYMD_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_8">
                <!-- 会社コード -->
                <a>
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社コード" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- 組織コード -->
                <a ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
                    <asp:Label ID="WF_ORG_L" runat="server" Text="組織コード" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_ORG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_ORG_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_9">
                <!-- メールアドレス -->
                <a>
                    <asp:Label ID="WF_EMAIL_L" runat="server" Text="メールアドレス" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_EMAIL" runat="server" Height="1.1em" Width="29.5em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_EMAIL_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_10">
                <!-- メニュー表示制御ロール -->
                <a ondblclick="Field_DBclick('WF_MENUROLE', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_MENUROLE');">
                    <asp:Label ID="WF_MENUROLE_L" runat="server" Text="メニュー表示制御ロール" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_MENUROLE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_MENUROLE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- 画面参照更新制御ロール -->
                <a ondblclick="Field_DBclick('WF_MAPROLE', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_MAPROLE');">
                    <asp:Label ID="WF_MAPROLE_L" runat="server" Text="画面参照更新制御ロール" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_MAPROLE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_MAPROLE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_11">
                <!-- 画面表示項目制御ロール -->
                <a ondblclick="Field_DBclick('WF_VIEWPROFID', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_VIEWPROFID');">
                    <asp:Label ID="WF_VIEWPROFID_L" runat="server" Text="画面表示項目制御ロール" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_VIEWPROFID" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_VIEWPROFID_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- エクセル出力制御ロール -->
                <a ondblclick="Field_DBclick('WF_RPRTPROFID', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_RPRTPROFID');">
                    <asp:Label ID="WF_RPRTPROFID_L" runat="server" Text="エクセル出力制御ロール" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_RPRTPROFID" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_RPRTPROFID_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>

            <p id="KEY_LINE_12">
                <!-- 画面初期値ロール -->
                <a>
                    <asp:Label ID="WF_VARIANT_L" runat="server" Text="画面初期値ロール" Width="15.0em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="WF_VARIANT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_VARIANT_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- 承認権限ロール -->
                <a ondblclick="Field_DBclick('WF_APPROVALID', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('WF_APPROVALID');">
                    <asp:Label ID="WF_APPROVALID_L" runat="server" Text="承認権限ロール" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_APPROVALID" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_APPROVALID_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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

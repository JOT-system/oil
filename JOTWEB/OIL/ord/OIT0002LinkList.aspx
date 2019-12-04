<%@ Page Title="OIT0002L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0002LinkList.aspx.vb" Inherits="JOTWEB.OIT0002LinkList" %>
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

                    <!-- 削除フラグ -->
                    <a class="ef" ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="15.0em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_2">
                    <!-- 貨車連結順序表№ -->
                    <a class="ef">
                        <asp:Label ID="WF_LINKNO_L" runat="server" Text="貨車連結順序表№" Width="10.0em" CssClass="WF_TEXT_LABEL" Font-Underline="true"></asp:Label>
                        <asp:TextBox ID="WF_LINKNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LINKNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 貨車連結順序表明細№ -->
                    <a class="ef">
                        <asp:Label ID="WF_LINKDETAILNO_L" runat="server" Text="貨車連結順序表明細№" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LINKDETAILNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LINKDETAILNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_3">
                    <!-- ステータス -->
                    <a class="ef">
                        <asp:Label ID="WF_STATUS_L" runat="server" Text="ステータス" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_STATUS" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_STATUS_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 情報 -->
                    <a class="ef">
                        <asp:Label ID="WF_INFO_L" runat="server" Text="情報" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_INFO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_INFO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 前回オーダー№ -->
                    <a class="ef">
                        <asp:Label ID="WF_PREORDERNO_L" runat="server" Text="前回オーダー№" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_PREORDERNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_PREORDERNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 本線列車 -->
                    <a class="ef">
                        <asp:Label ID="WF_TRAINNO_L" runat="server" Text="本線列車" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_TRAINNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_TRAINNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 登録営業所コード -->
                    <a class="ef">
                        <asp:Label ID="WF_OFFICECODE_L" runat="server" Text="登録営業所コード" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OFFICECODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_OFFICECODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 空車発駅コード -->
                    <a class="ef">
                        <asp:Label ID="WF_DEPSTATION_L" runat="server" Text="空車発駅コード" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_DEPSTATION" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DEPSTATION_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 空車発駅名 -->
                    <a class="ef">
                        <asp:Label ID="WF_DEPSTATIONNAME_L" runat="server" Text="空車発駅名" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_DEPSTATIONNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_DEPSTATIONNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 空車着駅コード -->
                    <a class="ef">
                        <asp:Label ID="WF_RETSTATION_L" runat="server" Text="空車着駅コード" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RETSTATION" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_RETSTATION_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 空車着駅名 -->
                    <a class="ef">
                        <asp:Label ID="WF_RETSTATIONNAME_L" runat="server" Text="空車着駅名" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_RETSTATIONNAME" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_RETSTATIONNAME_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 空車着日（予定） -->
                    <a class="ef">
                        <asp:Label ID="WF_EMPARRDATE_L" runat="server" Text="空車着日（予定）" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_EMPARRDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_EMPARRDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 空車着日（実績） -->
                    <a class="ef">
                        <asp:Label ID="WF_ACTUALEMPARRDATE_L" runat="server" Text="空車着日（実績）" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ACTUALEMPARRDATE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_ACTUALEMPARRDATE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- 入線列車番号 -->
                    <a class="ef">
                        <asp:Label ID="WF_LINETRAINNO_L" runat="server" Text="入線列車番号" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LINETRAINNO" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LINETRAINNO_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 入線順 -->
                    <a class="ef">
                        <asp:Label ID="WF_LINEORDER_L" runat="server" Text="入線順" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_LINEORDER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_LINEORDER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>

                    <!-- タンク車№ -->
                    <a class="ef">
                        <asp:Label ID="WF_TANKNUMBER_L" runat="server" Text="タンク車№" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_TANKNUMBER" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_TANKNUMBER_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 前回油種 -->
                    <a class="ef">
                        <asp:Label ID="WF_PREOILCODE_L" runat="server" Text="前回油種" Width="10.0em" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_PREOILCODE" runat="server" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_PREOILCODE_TEXT" runat="server" Width="15em" CssClass="WF_TEXT"></asp:Label>
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

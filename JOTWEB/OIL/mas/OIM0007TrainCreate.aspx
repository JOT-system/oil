<%@ Page Title="OIM0007C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0007TrainCreate.aspx.vb" Inherits="JOTWEB.OIM0007TrainCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0007CH" ContentPlaceHolderID="head" runat="server">
<%--    <link href='<%=ResolveUrl("~/OIL/css/OIM0007C.css")%>' rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0007C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="OIM0007C" ContentPlaceHolderID="contents1" runat="server">
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
                        <asp:Label ID="WF_SEL_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:Label ID="WF_SEL_LINECNT" runat="server" CssClass="WF_TEXT_LABEL"></asp:Label>
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
                    <!-- 管轄受注営業所 -->
                    <span class="ef">
                        <asp:Label ID="WF_OFFICECODE_L" runat="server" Text="管轄受注営業所" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OFFICECODE', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('WF_OFFICECODE');">
                            <asp:TextBox ID="WF_OFFICECODE" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OFFICECODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 本線列車番号 -->
                    <span class="ef">
                        <asp:Label ID="WF_TRAINNO_L" runat="server" Text="本線列車番号" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <asp:TextBox ID="WF_TRAINNO" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_TRAINNO_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 本線列車番号名 -->
                    <span class="ef">
                        <asp:Label ID="WF_TRAINNAME_L" runat="server" Text="本線列車番号名" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_TRAINNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_TRAINNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 積置フラグ -->
                    <span class="ef">
                        <asp:Label ID="WF_TSUMI_L" runat="server" Text="積置フラグ" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_TSUMI', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_TSUMI');">
                            <asp:TextBox ID="WF_TSUMI" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_TSUMI_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 発駅コード -->
                    <span class="ef">
                        <asp:Label ID="WF_DEPSTATION_L" runat="server" Text="発駅コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_DEPSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_DEPSTATION');">
                            <asp:TextBox ID="WF_DEPSTATION" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="7"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_DEPSTATION_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 着駅コード -->
                    <span class="ef">
                        <asp:Label ID="WF_ARRSTATION_L" runat="server" Text="着駅コード" CssClass="WF_TEXT_LABEL requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_ARRSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('WF_ARRSTATION');">
                            <asp:TextBox ID="WF_ARRSTATION" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="7"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_ARRSTATION_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- OT列車番号 -->
                    <span class="ef">
                        <asp:Label ID="WF_OTTRAINNO_L" runat="server" Text="OT列車番号" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_OTTRAINNO" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_OTTRAINNO_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- OT発送日報送信フラグ -->
                    <span class="ef">
                        <asp:Label ID="WF_OTFLG_L" runat="server" Text="OT発送日報送信フラグ" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_OTFLG', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_OTFLG');">
                            <asp:TextBox ID="WF_OTFLG" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_OTFLG_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- JR発列車番号 -->
                    <span class="ef">
                        <asp:Label ID="WF_JRTRAINNO1_L" runat="server" Text="JR発列車番号" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JRTRAINNO1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_JRTRAINNO1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- JR発列車牽引車数 -->
                    <span class="ef">
                        <asp:Label ID="WF_MAXTANK1_L" runat="server" Text="JR発列車牽引車数" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MAXTANK1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_MAXTANK1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- JR中継列車番号 -->
                    <span class="ef">
                        <asp:Label ID="WF_JRTRAINNO2_L" runat="server" Text="JR中継列車番号" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JRTRAINNO2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_JRTRAINNO2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- JR中継列車牽引車数 -->
                    <span class="ef">
                        <asp:Label ID="WF_MAXTANK2_L" runat="server" Text="JR中継列車牽引車数" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MAXTANK2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_MAXTANK2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- JR最終列車番号 -->
                    <span class="ef">
                        <asp:Label ID="WF_JRTRAINNO3_L" runat="server" Text="JR最終列車番号" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_JRTRAINNO3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="WF_JRTRAINNO3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- JR最終列車牽引車数 -->
                    <span class="ef">
                        <asp:Label ID="WF_MAXTANK3_L" runat="server" Text="JR最終列車牽引車数" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MAXTANK3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_MAXTANK3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 列車区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_TRAINCLASS_L" runat="server" Text="列車区分" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_TRAINCLASS', <%=LIST_BOX_CLASSIFICATION.LC_TRAINCLASS%>);" onchange="TextBox_change('WF_TRAINCLASS');">
                            <asp:TextBox ID="WF_TRAINCLASS" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_TRAINCLASS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 高速列車区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_SPEEDCLASS_L" runat="server" Text="高速列車区分" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_SPEEDCLASS', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_SPEEDCLASS');">
                            <asp:TextBox ID="WF_SPEEDCLASS" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_SPEEDCLASS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_12">
                    <!-- 発送順区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_SHIPORDERCLASS_L" runat="server" Text="発送順区分" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_SHIPORDERCLASS', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_SHIPORDERCLASS');">
                            <asp:TextBox ID="WF_SHIPORDERCLASS" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_SHIPORDERCLASS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 発日日数 -->
                    <span class="ef">
                        <asp:Label ID="WF_DEPDAYS_L" runat="server" Text="発日日数" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_DEPDAYS" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_DEPDAYS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 特継日数 -->
                    <span class="ef">
                        <asp:Label ID="WF_MARGEDAYS_L" runat="server" Text="特継日数" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_MARGEDAYS" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_MARGEDAYS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_14">
                    <!-- 積車着日数 -->
                    <span class="ef">
                        <asp:Label ID="WF_ARRDAYS_L" runat="server" Text="積車着日数" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ARRDAYS" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_ARRDAYS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 受入日数 -->
                    <span class="ef">
                        <asp:Label ID="WF_ACCDAYS_L" runat="server" Text="受入日数" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ACCDAYS" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_ACCDAYS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_15">
                    <!-- 空車着日数 -->
                    <span class="ef">
                        <asp:Label ID="WF_EMPARRDAYS_L" runat="server" Text="空車着日数" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_EMPARRDAYS" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_EMPARRDAYS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    
                    <!-- 当日利用日数 -->
                    <span class="ef">
                        <asp:Label ID="WF_USEDAYS_L" runat="server" Text="当日利用日数" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_USEDAYS', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_USEDAYS');">
                            <asp:TextBox ID="WF_USEDAYS" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_USEDAYS_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_16">
                    <!-- 料金マスタ区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_FEEKBN_L" runat="server" Text="料金マスタ区分" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_FEEKBN" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                        <asp:Label ID="WF_FEEKBN_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_17">
                    <!-- 稼働フラグ -->
                    <span class="ef">
                        <asp:Label ID="WF_RUN_L" runat="server" Text="稼働フラグ" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <span ondblclick="Field_DBclick('WF_RUN', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_RUN');">
                            <asp:TextBox ID="WF_RUN" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="WF_RUN_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_18">
                    <!-- 在庫管理表表示ソート区分 -->
                    <span class="ef">
                        <asp:Label ID="WF_ZAIKOSORT_L" runat="server" Text="在庫管理表表示ソート区分" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_ZAIKOSORT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="WF_ZAIKOSORT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_19">
                    <!-- 備考 -->
                    <span class="ef">
                        <asp:Label ID="WF_BIKOU_L" runat="server" Text="備考" CssClass="WF_TEXT_LABEL"></asp:Label>
                        <asp:TextBox ID="WF_BIKOU" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="200"></asp:TextBox>
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

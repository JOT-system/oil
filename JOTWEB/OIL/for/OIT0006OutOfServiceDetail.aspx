<%@ Page Title="OIT0006D" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0006OutOfServiceDetail.aspx.vb" Inherits="JOTWEB.OIT0006OutOfServiceDetail" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0006WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0006DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0006D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0006D.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId1 = '<%=Me.pnlListArea1.ClientID%>';
        var pnlListAreaId2 = '<%=Me.pnlListArea2.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="OIT0006D" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide">
            </div>
            <div class="rightSide">
                <!-- ボタン -->
                <input type="button" id="WF_ButtonDELIVERY" style="display:none" class="btn-sticky" value="託送指示" onclick="ButtonClick('WF_ButtonDELIVERY');" />
                <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="回送登録" onclick="ButtonClick('WF_ButtonINSERT');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div>

        <div style="display:none;" data-comment="わからないので退避">
            <!-- 会社コード -->
            <div style="display:none">
                <a>会社コード</a>
                <a class="ef" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 運用部署 -->
            <div style="display:none">
                <a>運用部署</a>

                <a class="ef" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
                    <asp:TextBox ID="WF_UORG" runat="server" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_UORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
        </div>

        <!-- ○ 変動項目 ○ -->
        <div id="headerDispArea"> <!-- このdivで括られた領域を表示非表示する -->
            <asp:Panel ID="pnlHeaderInput" CssClass="commonHeaderInput" runat="server">

                <!-- ■　オーダー№　■ -->
                <span class="left">
                    <a id="WF_KAISOUORDERNO_LABEL" class="requiredMark">オーダー№</a>
                    <a class="ef" id="WF_KAISOUORDERNO">
                        <asp:TextBox ID="TxtKaisouOrderNo" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　ステータス　■ -->
                <span>
                    <a id="WF_KAISOUSTATUS_LABEL">ステータス</a>
                    <a class="ef" id="KAISOUSTATUS">
                        <asp:TextBox ID="TxtKaisouStatus" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　目的　■ -->
                <span style="display:none">
                    <a id="WF_OBJECTIVE_LABEL">目的</a>
                    <a class="ef" id="WF_OBJECTIVE" ondblclick="Field_DBclick('TxtObjective', <%=LIST_BOX_CLASSIFICATION.LC_OBJECTIVECODE%>);" onchange="TextBox_change('TxtObjective');">
                        <asp:TextBox ID="TxtObjective" runat="server" onblur="MsgClear();" ReadOnly="true" CssClass="boxIcon iconOnly" MaxLength="2"></asp:TextBox>
                    </a>
                </span>
                <span class ="left">
                    <span>
                        <a id="WF_OBJECTIVENAME">
                            <asp:Label ID="LblObjective" runat="server"></asp:Label>
                        </a>
                    </span>
                </span>                <span></span>

                <!-- ■　回送登録営業所　■ -->
                <span class="left">
                    <a id="WF_OFFICECODE_LABEL" class="requiredMark">回送営業所</a>
                    <a class="ef" id="WF_OFFICECODE" ondblclick="Field_DBclick('TxtKaisouOrderOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE_KAISOU%>);" onchange="TextBox_change('TxtKaisouOrderOffice');">
                        <asp:TextBox ID="TxtKaisouOrderOffice" runat="server" onblur="MsgClear();" ReadOnly="true" CssClass="boxIcon iconOnly" MaxLength="20"></asp:TextBox>
                        <asp:TextBox ID="TxtKaisouOrderOfficeCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　本線列車　■ -->
                <span style="display:none">
                    <a id="WF_TRAINNO_LABEL" class="requiredMark">本線列車</a>
                    <%-- 20200911 START 本線列車の入力を自由入力に変更(指摘票No130) --%>
                    <%--<a class="ef" id="WF_TRAINNO" ondblclick="Field_DBclick('TxtTrainNo', <%=LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER%>);" onchange="TextBox_change('TxtTrainNo');">--%>
                        <%--<asp:TextBox ID="TxtTrainNo" runat="server" onblur="MsgClear();" CssClass="boxIcon" MaxLength="4"></asp:TextBox>--%>
                    <a class="ef" id="WF_TRAINNO">
                        <asp:TextBox ID="TxtTrainNo" runat="server" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                    <%-- 20200911 END   本線列車の入力を自由入力に変更(指摘票No130) --%>
                        <asp:TextBox ID="TxtTrainName" runat="server" onblur="MsgClear();" ReadOnly="true" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　タンク車数　■ -->
                <span style="display:none">
                    <a id="WF_TANKCNT_LABEL" class="requiredMark">タンク車数</a>
                    <a class="ef" id="WF_TANKCNT">
                        <asp:TextBox ID="TxtTankCnt" runat="server" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                    </a>
                    <%--<input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="明細を作る" onclick="ButtonClick('WF_ButtonINSERT');" />--%>
                </span>
                <span></span>
                <span></span>

                <!-- ■　回送パターン　■ -->
                <span class="doubleItem" style="display:none">
                    <a id="WF_KAISOUTYPE_LABEL" class="requiredMark">回送パターン</a>
                    <a class="ef" id="WF_KAISOUTYPE" ondblclick="Field_DBclick('TxtKaisouType', <%=LIST_BOX_CLASSIFICATION.LC_KAISOUTYPE%>);" onchange="TextBox_change('TxtKaisouType');">
                        <asp:TextBox ID="TxtKaisouType" runat="server" ReadOnly="true" CssClass="boxIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                        <asp:TextBox ID="TxtKaisouTypeCode" runat="server" onblur="MsgClear();" ReadOnly="true" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <span></span>
                <!-- ■　運賃フラグ　■ -->
                <span style="display:none">
                    <a id="WF_FAREFLG_LABEL" class="requiredMark">片道</a>
                    <a id="WF_FAREFLG" onchange="ButtonClick('WF_CheckBoxSELECTFAREFLG');">
                        <asp:CheckBox ID="ChkFareFlg" runat="server" />
                    </a>
                </span>
                <span></span>
                <span></span>

                <!-- ■　発駅　■ -->
                <span class="left" style="display:none">
                    <a id="WF_DEPSTATION_LABEL" class="requiredMark">発駅</a>
                    <a class="ef" id="WF_DEPSTATIONCODE" ondblclick="Field_DBclick('TxtDepstationCode', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE_FOCUSON%>);" onchange="TextBox_change('TxtDepstationCode');">
                        <asp:TextBox ID="TxtDepstationCode" runat="server"  ReadOnly="true" CssClass="boxIcon iconOnly" onblur="MsgClear();" MaxLength="7"></asp:TextBox>
                    </a>
                </span>
                <span>
                    <span>
                        <a id="WF_DEPSTATIONNAME">
                            <asp:Label ID="LblDepstationName" runat="server"></asp:Label>
                        </a>
                    </span>
                </span>
                <!-- ■　着駅　■ -->
                <span style="display:none">
                    <a id="WF_ARRSTATION_LABEL" class="requiredMark">着駅</a>
                    <a class="ef" id="WF_ARRSTATIONCODE" ondblclick="Field_DBclick('TxtArrstationCode', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE_FOCUSON%>);" onchange="TextBox_change('TxtArrstationCode');">
                        <asp:TextBox ID="TxtArrstationCode" runat="server"  ReadOnly="true" CssClass="boxIcon iconOnly" onblur="MsgClear();" MaxLength="7"></asp:TextBox>
                    </a>
                </span>
                <span class ="left">
                    <span>
                        <a id="WF_ARRSTATIONNAME">
                            <asp:Label ID="LblArrstationName" runat="server"></asp:Label>
                        </a>
                    </span>
                </span>
                <span></span>
                <!-- ■　(予定)発日　■ -->
                <span class="left" style="display:none">
                    <a id="WF_DEPDATE_LABEL" class="requiredMark">(予定)発日</a>
                    <a class="ef" id="WF_DEPDATE" ondblclick="Field_DBclick('TxtDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtDepDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(予定)着日　■ -->
                <span style="display:none">
                    <a id="WF_ARRDATE_LABEL" class="requiredMark">着日</a>
                    <a class="ef" id="WF_ARRDATE" ondblclick="Field_DBclick('TxtArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtArrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(予定)受入日　■ -->
                <span style="display:none">
                    <a id="WF_ACCDATE_LABEL" class="requiredMark">受入日</a>
                    <a class="ef" id="WF_ACCDATE" ondblclick="Field_DBclick('TxtAccDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtAccDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(予定)発駅戻り日　■ -->
                <span style="display:none">
                    <a id="WF_EMPARRDATE_LABEL" class="requiredMark">発駅戻り日</a>
                    <a class="ef" id="WF_EMPARRDATE" ondblclick="Field_DBclick('TxtEmparrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtEmparrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>
                <span></span>

                <!-- ■　(実績)発日　■ -->
                <span class="left" style="display:none">
                    <a id="WF_ACTUALDEPDATE_LABEL" class="requiredMark">(実績)発日</a>
                    <a class="ef" id="WF_ACTUALDEPDATE" ondblclick="Field_DBclick('TxtActualDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtActualDepDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(実績)着日　■ -->
                <span style="display:none">
                    <a id="WF_ACTUALARRDATE_LABEL" class="requiredMark">着日</a>
                    <a class="ef" id="WF_ACTUALARRDATE" ondblclick="Field_DBclick('TxtActualArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtActualArrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(実績)受入日　■ -->
                <span style="display:none">
                    <a id="WF_ACTUALACCDATE_LABEL" class="requiredMark">受入日</a>
                    <a class="ef" id="WF_ACTUALACCDATE" ondblclick="Field_DBclick('TxtActualAccDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtActualAccDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(実績)発駅戻り日　■ -->
                <span style="display:none">
                    <a id="WF_ACTUALEMPARRDATE_LABEL" class="requiredMark">発駅戻り日</a>
                    <a class="ef" id="WF_ACTUALEMPARRDATE" ondblclick="Field_DBclick('TxtActualEmparrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtActualEmparrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>
                <span></span>
            </asp:Panel> <!-- END pnlHeaderInput -->
            <!-- 回送パターン表示エリア -->
            <div class="summaryAreaWrapper">
                <!-- 回送パターン数 -->
                <div class="summaryArea">
                    <!-- ■　回送パターン　■ -->
                    <span>
                        <a>&nbsp;</a>
                        <a id="WF_OILTANKCNT_LABEL" class="requiredMark">回送パターン</a>
                    </span>
                    <!-- ■　目的（修理）　■ -->
                    <span>
                        <a id="WF_REPAIR_LABEL">修理</a>
                        <a class="ef" id="WF_REPAIR">
                            <asp:TextBox ID="TxtRepair" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                    </span>
                    <!-- ■　目的（ＭＣ）　■ -->
                    <span>
                        <a id="WF_MC_LABEL">ＭＣ</a>
                        <a class="ef" id="WF_MC">
                            <asp:TextBox ID="TxtMC" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                    </span>
                    <!-- ■　目的（交検）　■ -->
                    <span>
                        <a id="WF_INSPECTION_LABEL">交検</a>
                        <a class="ef" id="WF_INSPECTION">
                            <asp:TextBox ID="TxtInspection" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                    </span>
                    <!-- ■　目的（全検）　■ -->
                    <span>
                        <a id="WF_ALLINSPECTION_LABEL">全検</a>
                        <a class="ef" id="WF_ALLINSPECTION">
                            <asp:TextBox ID="TxtALLInspection" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                    </span>
                    <!-- ■　目的（留置）　■ -->
                    <span>
                        <a id="WF_INDWELLING_LABEL">疎開留置</a>
                        <a class="ef" id="WF_INDWELLING">
                            <asp:TextBox ID="TxtIndwelling" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                    </span>
                    <!-- ■　目的（移動）　■ -->
                    <span>
                        <a id="WF_MOVE_LABEL">移動</a>
                        <a class="ef" id="WF_MOVE">
                            <asp:TextBox ID="TxtMove" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                    </span>
                </div>
            </div>

        </div> <!-- END headerDispArea -->
    </div> <!-- END headerbox -->

    <!-- 全体レイアウト　detailbox -->
    <div  class="detailbox" id="detailbox">
        <!-- タブボックス -->
        <div id="tabBox">
            <div class="leftSide">
                <!-- ■　Dタブ　■ -->
                <asp:Label ID="WF_Dtab01" runat="server" Text="タンク車割当" data-itemelm="tab" onclick="DtabChange('0')" ></asp:Label>
                <asp:Label ID="WF_Dtab02" runat="server" Text="費用入力" data-itemelm="tab" onclick="DtabChange('1')"></asp:Label>
            </div>
            <div class="rightSide">
                <span id="hideHeader">
                </span>
            </div>
        </div> <!-- END tabBox -->

        <asp:MultiView ID="WF_DetailMView" runat="server">
            <!-- ■ Tab No1　タンク車割当　■ -->
            <asp:View ID="WF_DView1" runat="server" >
                <!-- ボタン -->
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <input type="button" id="WF_ButtonALLSELECT_TAB1" class="btn-sticky" value="全選択" onclick="ButtonClick('WF_ButtonALLSELECT_TAB1');" />
                        <input type="button" id="WF_ButtonSELECT_LIFTED_TAB1" class="btn-sticky" value="選択解除"  onclick="ButtonClick('WF_ButtonSELECT_LIFTED_TAB1');" />
                        <input type="button" id="WF_ButtonLINE_LIFTED_TAB1" class="btn-sticky" value="行削除"  onclick="ButtonClick('WF_ButtonLINE_LIFTED_TAB1');" />
                        <input type="button" id="WF_ButtonLINE_ADD_TAB1" class="btn-sticky" value="行追加"  onclick="ButtonClick('WF_ButtonLINE_ADD_TAB1');" />
                    </div>
                    <div class="detail_tab"">
                    <%--<div class="detail_tab" style="display:none">--%>
                        <a class="ef" id="WF_BULKINSPECTION" ondblclick="Field_DBclick('TxtBulkInspection', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                            <asp:TextBox ID="TxtBulkInspection" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" Text="2021/4/23" onblur="MsgClear();"></asp:TextBox>
                        </a>
                        <input type="button" id="WF_ButtonINSPECTION_TAB1" class="btn-sticky" value="交検日一括反映" onclick="ButtonClick('WF_ButtonINSPECTION_TAB1');" />
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonUPDATE_KARI_TAB1" class="btn-sticky" value="一時保存"  onclick="ButtonClick('WF_ButtonUPDATE_KARI_TAB1');" />
                        <input type="button" id="WF_ButtonUPDATE_TAB1" class="btn-sticky" value="確定"  onclick="ButtonClick('WF_ButtonUPDATE_TAB1');" />
                        <input type="button" id="WF_ButtonUPDATE_MEISAI_TAB1" class="btn-sticky" value="明細更新"  onclick="ButtonClick('WF_ButtonUPDATE_MEISAI_TAB1');" />
                    </div>
                </div> <!-- END actionButtonBox -->
                <!-- 一覧レイアウト -->
                <asp:panel id="pnlListArea1" runat="server" ></asp:panel>
            </asp:View> <!-- END WF_DView1 -->

            <!-- ■ Tab No2　費用入力　■ -->
            <asp:View ID="WF_DView2" runat="server">
                <!-- 一覧レイアウト -->
                <asp:panel id="pnlListArea2" runat="server" ></asp:panel>
            </asp:View>
        </asp:MultiView> <!-- END WF_DetailMView -->
    </div> <!-- END detailbox -->

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
        <!-- GridView表示位置フィールド -->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>

        <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD" runat="server" value="" type="text" />
        <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />

        <!-- 画面表示切替 -->
        <input id="WF_DISP" runat="server" value="" type="text" />
        <!-- LeftBox Mview切替 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
        <!-- LeftBox 開閉 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
        <!-- Rightbox Mview切替 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />
        <!-- Rightbox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />

        <!-- Textbox Print URL -->
        <input id="WF_PrintURL" runat="server" value="" type="text" />

        <!-- 一覧・詳細画面切替用フラグ -->
        <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />

        <!-- ボタン押下 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />
        <!-- 権限 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
        <!-- 新規・更新切替用フラグ -->
        <input id="WF_CREATEFLG" runat="server" value="" type="text" />
        <!-- 託送指示フラグ -->
        <input id="WF_DELIVERYFLG" runat="server" value="" type="text" />
        <!-- 画面ボタン制御 -->
        <input id="WF_MAPButtonControl" runat="server" value="0" type="text" />
        <!-- DetailBox Mview切替 -->
        <input id="WF_DTAB_CHANGE_NO" runat="server" value="" type="text"/>
        <!-- ヘッダーを表示するか保持、"1"(表示:初期値),"0"(非表示)  -->
        <asp:HiddenField ID="hdnDispHeaderItems" runat="server" Value="1" />
    </div>

</asp:Content>

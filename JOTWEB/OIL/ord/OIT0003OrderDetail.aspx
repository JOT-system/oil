<%@ Page Title="OIT0003D" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0003OrderDetail.aspx.vb" Inherits="JOTWEB.OIT0003OrderDetail" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0003DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0003D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0003D.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId1 = '<%=Me.pnlListArea1.ClientID%>';
        var pnlListAreaId2 = '<%=Me.pnlListArea2.ClientID%>';
        var pnlListAreaId3 = '<%=Me.pnlListArea3.ClientID%>';
        var pnlListAreaId4 = '<%=Me.pnlListArea4.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="OIT0003D" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide">
            </div>
            <div class="rightSide">
                <!-- ボタン -->
                <input type="button" id="WF_ButtonCONTACT" class="btn-sticky" value="手配連絡" onclick="ButtonClick('WF_ButtonCONTACT');" />
                <input type="button" id="WF_ButtonRESULT" class="btn-sticky" value="結果受理" onclick="ButtonClick('WF_ButtonRESULT');" />
                <input type="button" id="WF_ButtonDELIVERY" class="btn-sticky" value="託送指示" onclick="ButtonClick('WF_ButtonDELIVERY');" />
                <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="油種数登録" onclick="ButtonClick('WF_ButtonINSERT');" />
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
            <asp:Panel ID="pnlHeaderInput" CssClass="headerInput" runat="server">
                <!-- ■　オーダー№　■ -->
                <span class="left">
                    <a id="WF_ORDERNO_LABEL" class="requiredMark">オーダー№</a>
                    <a class="ef" id="WF_ORDERNO">
                        <asp:TextBox ID="TxtOrderNo" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　ステータス　■ -->
                <span>
                    <a id="WF_ORDERSTATUS_LABEL">ステータス</a>
                    <a class="ef" id="ORDERSTATUS">
                        <asp:TextBox ID="TxtOrderStatus" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　情報　■ -->
                <span>
                    <a></a>
                    <a class="ef" id="WF_ORDERINFO">
                        <%--<asp:TextBox ID="TxtOrderInfo" runat="server" onblur="MsgClear();" Enabled="true"></asp:TextBox>--%>
                        <asp:CheckBox ID="chkOrderInfo" runat="server" Text=" " Checked="true" Enabled ="false"/>
                        <asp:CheckBox ID="chkOrderDetailInfo" runat="server" Text=" " Checked="true" Enabled ="false" Visible="false" />
                    </a>
                </span>
                <!-- ■　受注パターン　■ -->
                <span class="doubleItem">
                    <a id="WF_ORDERTYPE_LABEL" class="requiredMark">受注パターン</a>
                    <a class="ef" id="WF_ORDERTYPE">
                        <asp:TextBox ID="TxtOrderType" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtOrderTrkKbn" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <!-- <span></span> -->

                <!-- ■　受注営業所　■ -->
                <span class="left">
                    <a id="WF_OFFICECODE_LABEL" class="requiredMark">受注営業所</a>
                    <a class="ef" id="WF_OFFICECODE" ondblclick="Field_DBclick('TxtOrderOffice', <%=LIST_BOX_CLASSIFICATION.LC_SALESOFFICE%>);" onchange="TextBox_change('TxtOrderOffice');">
                        <asp:TextBox ID="TxtOrderOffice" runat="server" onblur="MsgClear();" ReadOnly="true" CssClass="boxIcon iconOnly" MaxLength="20"></asp:TextBox>
                        <asp:TextBox ID="TxtOrderOfficeCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　本線列車　■ -->
                <span>
                    <a id="WF_TRAINNO_LABEL" class="requiredMark">本線列車</a>
                    <a class="ef" id="WF_TRAINNO" ondblclick="Field_DBclick('TxtTrainNo', <%=LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER%>);" onchange="TextBox_change('TxtTrainNo');">
                        <asp:TextBox ID="TxtTrainNo" runat="server" onblur="MsgClear();" ReadOnly="true" CssClass="boxIcon iconOnly" MaxLength="4"></asp:TextBox>
                        <asp:TextBox ID="TxtTrainName" runat="server" onblur="MsgClear();" ReadOnly="true" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <span>
                    <a id="WF_OTTRAINNO_LABEL">積置列車</a>
                    <a class="ef" id="WF_OTTRAINNO" ondblclick="Field_DBclick('TxtOTTrainNo', <%=LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER%>);" onchange="TextBox_change('TxtOTTrainNo');">
                        <asp:TextBox ID="TxtOTTrainNo" runat="server" onblur="MsgClear();" ReadOnly="true" CssClass="boxIcon iconOnly" MaxLength="4" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtOTTrainName" runat="server" onblur="MsgClear();" ReadOnly="true" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <span></span><span></span>

                <!-- ■　荷主　■ -->
                <span class="left">
                    <a id="WF_SHIPPERS_LABEL" class="requiredMark">荷主</a>
                    <a class="ef" id="WF_SHIPPERSCODE" ondblclick="Field_DBclick('TxtShippersCode', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPERSLIST%>);" onchange="TextBox_change('TxtShippersCode');">
                        <asp:TextBox ID="TxtShippersCode" runat="server"  ReadOnly="true" CssClass="boxIcon iconOnly" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                    </a>
                </span>
                <span>
                    <span>
                        <a id="WF_SHIPPERSNAME">
                            <asp:Label ID="LblShippersName" runat="server"></asp:Label>
                        </a>
                    </span>
                </span>
                <!-- ■　荷受人　■ -->
                <span>
                    <a id="WF_CONSIGNEE_LABEL" class="requiredMark">荷受人</a>
                    <a class="ef" id="WF_CONSIGNEECODE" ondblclick="Field_DBclick('TxtConsigneeCode', <%=LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST%>);" onchange="TextBox_change('TxtConsigneeCode');">
                        <asp:TextBox ID="TxtConsigneeCode" runat="server"  ReadOnly="true" CssClass="boxIcon iconOnly" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                    </a>
                </span>
                <span class ="left">
                    <span>
                        <a id="WF_CONSIGNEENAME">
                            <asp:Label ID="LblConsigneeName" runat="server"></asp:Label>
                        </a>
                    </span>
                </span>
                <span></span>
                <!-- ■　発駅　■ -->
                <span class="left">
                    <a id="WF_DEPSTATION_LABEL" class="requiredMark">発駅</a>
                    <a class="ef" id="WF_DEPSTATIONCODE" ondblclick="Field_DBclick('TxtDepstationCode', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtDepstationCode');">
                        <asp:TextBox ID="TxtDepstationCode" runat="server"  ReadOnly="true" CssClass="boxIcon iconOnly" onblur="MsgClear();" MaxLength="7"></asp:TextBox>
                    </a>
                </span>
                <span>
                    <span>
                        <a id="WF_DEPSTATIONNAME">
                            <asp:Label ID="LblDepstationName" runat="server">JXTG</asp:Label>
                        </a>
                    </span>
                </span>
                <!-- ■　着駅　■ -->
                <span>
                    <a id="WF_ARRSTATION_LABEL" class="requiredMark">着駅</a>
                    <a class="ef" id="WF_ARRSTATIONCODE" ondblclick="Field_DBclick('TxtArrstationCode', <%=LIST_BOX_CLASSIFICATION.LC_STATIONCODE%>);" onchange="TextBox_change('TxtArrstationCode');">
                        <asp:TextBox ID="TxtArrstationCode" runat="server"  ReadOnly="true" CssClass="boxIcon iconOnly" onblur="MsgClear();" MaxLength="7"></asp:TextBox>
                    </a>
                </span>
                <span class ="left">
                    <span>
                        <a id="WF_ARRSTATIONNAME">
                            <asp:Label ID="LblArrstationName" runat="server">JXTG</asp:Label>
                        </a>
                    </span>
                </span>
                <span></span>
                <!-- ■　(予定)積込日　■ -->
                <span class="left">
                    <a id="WF_LOADINGDATE_LABEL" class="requiredMark">(予定)積込日</a>
                    <a class="ef" id="WF_LOADINGDATE" ondblclick="Field_DBclick('TxtLoadingDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtLoadingDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(予定)発日　■ -->
                <span>
                    <a id="WF_DEPDATE_LABEL" class="requiredMark">発日</a>
                    <a class="ef" id="WF_DEPDATE" ondblclick="Field_DBclick('TxtDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtDepDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(予定)積車着日　■ -->
                <span>
                    <a id="WF_ARRDATE_LABEL" class="requiredMark">積車着日</a>
                    <a class="ef" id="WF_ARRDATE" ondblclick="Field_DBclick('TxtArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtArrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(予定)受入日　■ -->
                <span class ="left">
                    <a id="WF_ACCDATE_LABEL" class="requiredMark">受入日</a>
                    <a class="ef" id="WF_ACCDATE" ondblclick="Field_DBclick('TxtAccDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtAccDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(予定)空車着日　■ -->
                <span>
                    <a id="WF_EMPARRDATE_LABEL" class="requiredMark">空車着日</a>
                    <a class="ef" id="WF_EMPARRDATE" ondblclick="Field_DBclick('TxtEmparrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtEmparrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　(実績)積込日　■ -->
                <span class="left">
                    <a id="WF_ACTUALLOADINGDATE_LABEL" class="requiredMark">(実績)積込日</a>
                    <a class="ef" id="WF_ACTUALLOADINGDATE" ondblclick="Field_DBclick('TxtActualLoadingDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtActualLoadingDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(実績)発日　■ -->
                <span>
                    <a id="WF_ACTUALDEPDATE_LABEL" class="requiredMark">発日</a>
                    <a class="ef" id="WF_ACTUALDEPDATE" ondblclick="Field_DBclick('TxtActualDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtActualDepDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(実績)積車着日　■ -->
                <span>
                    <a id="WF_ACTUALARRDATE_LABEL" class="requiredMark">積車着日</a>
                    <a class="ef" id="WF_ACTUALARRDATE" ondblclick="Field_DBclick('TxtActualArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtActualArrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(実績)受入日　■ -->
                <span class ="left">
                    <a id="WF_ACTUALACCDATE_LABEL" class="requiredMark">受入日</a>
                    <a class="ef" id="WF_ACTUALACCDATE" ondblclick="Field_DBclick('TxtActualAccDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtActualAccDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>

                <!-- ■　(実績)空車着日　■ -->
                <span>
                    <a id="WF_ACTUALEMPARRDATE_LABEL" class="requiredMark">空車着日</a>
                    <a class="ef" id="WF_ACTUALEMPARRDATE" ondblclick="Field_DBclick('TxtActualEmparrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtActualEmparrDate" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>
            </asp:Panel>
            <!-- タンク車数、積込数量　計上月表示エリア -->
            <asp:Panel ID="pnlSummaryArea" runat="server">
                <!-- タンク車数、積込数量 -->
                <div class="summaryTable">
                    <div class="headerRow">
                        <a></a>
                        <!-- ■　車数（ハイオク）　■ -->
                        <a id="WF_HTANK_LABEL_C">ハイオク</a>
                        <!-- ■　車数（レギュラー）　■ -->
                        <a id="WF_RTANK_LABEL_C">レギュラー</a>
                        <!-- ■　車数（灯油）　■ -->
                        <a id="WF_TTANK_LABEL_C">　灯油　</a>
                        <!-- ■　車数（未添加灯油）　■ -->
                        <a id="WF_MTTANK_LABEL_C">未添加灯油</a>
                        <!-- ■　車数（軽油）　■ -->
                        <a id="WF_KTANK_LABEL_C">　軽油　</a>
                        <!-- ■　車数（３号軽油）　■ -->
                        <a id="WF_K3TANK_LABEL_C">３号軽油</a>
                        <!-- ■　車数（５号軽油）　■ -->
                        <a id="WF_K5TANK_LABEL_C">５号軽油</a>
                        <!-- ■　車数（１０号軽油）　■ -->
                        <a id="WF_K10TANK_LABEL_C">１０号軽油</a>
                        <!-- ■　車数（LSA）　■ -->
                        <a id="WF_LTANK_LABEL_C">ＬＳＡ　　</a>
                        <!-- ■　車数（A重油）　■ -->
                        <a id="WF_ATANK_LABEL_C">Ａ重油</a>
                        <!-- ■　車数（合計）　■ -->
                        <a id="WF_TOTAL_LABEL_C">合計</a>
                    </div>
                    <div class="itemRow">
                        <!-- ■　油種別タンク車数(車)　■ -->
                        <a id="WF_OILTANKCNT_LABEL_C">油種別タンク車数(車)</a>
                        <!-- ■　車数（ハイオク）　■ -->
                        <a class="ef" id="WF_HTANK_C">
                            <asp:TextBox ID="TxtHTank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（レギュラー）　■ -->
                        <a class="ef" id="WF_RTANK_C">
                            <asp:TextBox ID="TxtRTank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（灯油）　■ -->
                        <a class="ef" id="WF_TTANK_C">
                            <asp:TextBox ID="TxtTTank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（未添加灯油）　■ -->
                        <a class="ef" id="WF_MTTANK_C">
                            <asp:TextBox ID="TxtMTTank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（軽油）　■ -->
                        <a class="ef" id="WF_KTANK_C">
                            <asp:TextBox ID="TxtKTank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（３号軽油）　■ -->
                        <a class="ef" id="WF_K3TANK_C">
                            <asp:TextBox ID="TxtK3Tank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（５号軽油）　■ -->
                        <a class="ef" id="WF_K5TANK_C">
                            <asp:TextBox ID="TxtK5Tank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（１０号軽油）　■ -->
                        <a class="ef" id="WF_K10TANK_C">
                            <asp:TextBox ID="TxtK10Tank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（LSA）　■ -->
                        <a class="ef" id="WF_LTANK_C">
                            <asp:TextBox ID="TxtLTank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（A重油）　■ -->
                        <a class="ef" id="WF_ATANK_C">
                            <asp:TextBox ID="TxtATank_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（合計）　■ -->
                        <a class="ef" id="WF_TOTAL_C">
                            <asp:TextBox ID="TxtTotal_c" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </div>
                    <div class="itemRow">
                        <!-- ■　積込数量(kl)　■ -->
                        <a id="WF_OILTANKCNT_LABEL_C2">　　　　積込数量(kl)</a>
                        <!-- ■　数量（ハイオク）　■ -->
                        <a class="ef" id="WF_HTANK_C2">
                            <asp:TextBox ID="TxtHTank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（レギュラー）　■ -->
                        <a class="ef" id="WF_RTANK_C2">
                            <asp:TextBox ID="TxtRTank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（灯油）　■ -->
                        <a class="ef" id="WF_TTANK_C2">
                            <asp:TextBox ID="TxtTTank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（未添加灯油）　■ -->
                        <a class="ef" id="WF_MTTANK_C2">
                            <asp:TextBox ID="TxtMTTank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（軽油）　■ -->
                        <a class="ef" id="WF_KTANK_C2">
                            <asp:TextBox ID="TxtKTank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（３号軽油）　■ -->
                        <a class="ef" id="WF_K3TANK_C2">
                            <asp:TextBox ID="TxtK3Tank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（５号軽油）　■ -->
                        <a class="ef" id="WF_K5TANK_C2">
                            <asp:TextBox ID="TxtK5Tank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（１０号軽油）　■ -->
                        <a class="ef" id="WF_K10TANK_C2">
                            <asp:TextBox ID="TxtK10Tank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（LSA）　■ -->
                        <a class="ef" id="WF_LTANK_C2">
                            <asp:TextBox ID="TxtLTank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（A重油）　■ -->
                        <a class="ef" id="WF_ATANK_C2">
                            <asp:TextBox ID="TxtATank_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　数量（合計）　■ -->
                        <a class="ef" id="WF_TOTALCNT_C2">
                            <asp:TextBox ID="TxtTotalCnt_c2" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </div>
                </div>
                <!-- 合計金額エリア -->
                <div class="summaryAmount" style="display:none">
                    <!-- ■　計上月　■ -->
                    <a id="WF_BUDGETMONTH_LABEL">計上月</a>
                    <a class="ef" id="WF_BUDGETMONTH">
                        <asp:TextBox ID="TxtBudgetMonth" runat="server" onblur="MsgClear();"></asp:TextBox>
                    </a>

                    <!-- ■　売上合計金額(税抜)　■ -->
                    <a id="WF_TOTALSALES_LABEL">売上合計金額(税抜)</a>
                    <a class="ef" id="WF_TOTALSALES">
                        <asp:TextBox ID="TxtTotalSales" runat="server" onblur="MsgClear();"></asp:TextBox>
                    </a>

                    <!-- ■　支払合計金額(税抜)　■ -->
                    <a id="WF_TOTALPAYMENT_LABEL">支払合計金額(税抜)</a>
                    <a class="ef" id="WF_TOTALPAYMENT">
                        <asp:TextBox ID="TxtTitalPayment" runat="server" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </div>
                <div class="summaryAmount" style="display:none">
                    <a></a>
                    <a></a>
                    <!-- ■　売上合計金額(税額)　■ -->
                    <a id="WF_TOTALSALES2_LABEL">　　　　　　(税額)</a>
                    <a class="ef" id="WF_TOTALSALES2">
                        <asp:TextBox ID="TxtTotalSales2" runat="server" onblur="MsgClear();"></asp:TextBox>
                    </a>

                    <!-- ■　支払合計金額(税額)　■ -->
                    <a id="WF_TOTALPAYMENT2_LABEL">　　　　　　(税額)</a>
                    <a class="ef" id="WF_TOTALPAYMENT2">
                        <asp:TextBox ID="TxtTitalPayment2" runat="server" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </div>
            </asp:Panel>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div  class="detailbox" id="detailbox">
        <!-- タブボックス -->
        <div id="tabBox">
            <div class="leftSide">
                <!-- ■　Dタブ　■ -->
                <asp:Label ID="WF_Dtab01" runat="server" Text="タンク車割当" data-itemelm="tab" onclick="DtabChange('0')" ></asp:Label>
                <asp:Label ID="WF_Dtab02" runat="server" Text="入換・積込指示" data-itemelm="tab" onclick="DtabChange('1')" ></asp:Label>
                <asp:Label ID="WF_Dtab03" runat="server" Text="タンク車明細" data-itemelm="tab" onclick="DtabChange('2')"></asp:Label>
                <asp:Label ID="WF_Dtab04" runat="server" Text="費用入力" data-itemelm="tab" onclick="DtabChange('3')"></asp:Label>
            </div>
            <div class="rightSide">
                <span id="hideHeader">
                </span>
            </div>
        </div>

        <asp:MultiView ID="WF_DetailMView" runat="server">
            <!-- ■ Tab No1　タンク車割当　■ -->
            <asp:View ID="WF_DView1" runat="server" >
                <div class="summaryTable wariate">
                    <div class="headerRow">
                        <a></a>
                        <!-- ■　車数（ハイオク）　■ -->
                        <a id="WF_HTANK_LABEL">ハイオク</a>
                        <!-- ■　車数（レギュラー）　■ -->
                        <a id="WF_RTANK_LABEL">レギュラー</a>
                        <!-- ■　車数（灯油）　■ -->
                        <a id="WF_TTANK_LABEL">　灯油　</a>
                        <!-- ■　車数（未添加灯油）　■ -->
                        <a id="WF_MTTANK_LABEL">未添加灯油</a>
                        <!-- ■　車数（軽油）　■ -->
                        <a id="WF_KTANK_LABEL">　軽油　</a>
                        <!-- ■　車数（３号軽油）　■ -->
                        <a id="WF_K3TANK_LABEL">３号軽油</a>
                        <!-- ■　車数（５号軽油）　■ -->
                        <a id="WF_K5TANK_LABEL">５号軽油</a>
                        <!-- ■　車数（１０号軽油）　■ -->
                        <a id="WF_K10TANK_LABEL">１０号軽油</a>
                        <!-- ■　車数（LSA）　■ -->
                        <a id="WF_LTANK_LABEL">ＬＳＡ　　</a>
                        <!-- ■　車数（A重油）　■ -->
                        <a id="WF_ATANK_LABEL">Ａ重油</a>
                        <!-- ■　合計　■ -->
                        <a id="WF_TOTALCNT_LABEL">合計</a>
                    </div>
                    <div class="itemRow">
                        <!-- ■　油種別タンク車数(車)　■ -->
                        <a id="WF_OILTANKCNT_LABEL">油種別タンク車数(車)</a>
                        <!-- ■　車数（ハイオク）　■ -->
                        <a class="ef" id="WF_HTANK">
                            <asp:TextBox ID="TxtHTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（レギュラー）　■ -->
                        <a class="ef" id="WF_RTANK">
                            <asp:TextBox ID="TxtRTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（灯油）　■ -->
                        <a class="ef" id="WF_TTANK">
                            <asp:TextBox ID="TxtTTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（未添加灯油）　■ -->
                        <a class="ef" id="WF_MTTANK">
                            <asp:TextBox ID="TxtMTTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（軽油）　■ -->
                        <a class="ef" id="WF_KTANK">
                            <asp:TextBox ID="TxtKTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（３号軽油）　■ -->
                        <a class="ef" id="WF_K3TANK">
                            <asp:TextBox ID="TxtK3Tank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（５号軽油）　■ -->
                        <a class="ef" id="WF_K5TANK">
                            <asp:TextBox ID="TxtK5Tank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（１０号軽油）　■ -->
                        <a class="ef" id="WF_K10TANK">
                            <asp:TextBox ID="TxtK10Tank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（LSA）　■ -->
                        <a class="ef" id="WF_LTANK">
                            <asp:TextBox ID="TxtLTank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　車数（A重油）　■ -->
                        <a class="ef" id="WF_ATANK">
                            <asp:TextBox ID="TxtATank" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                        <!-- ■　合計　■ -->
                        <a class="ef" id="WF_TOTALCNT">
                            <asp:TextBox ID="TxtTotalCnt" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>

                    </div>
                    <div class="itemRow">
                        <!-- #############################-->
                        <!-- 割当後                       -->
                        <!-- #############################-->
                        <!-- ■　油種別タンク車数(車)　■ -->
                        <a id="WF_OILTANKCNT_W_LABEL">割当後　油種別タンク車数(車)</a>
                        <!-- ■　車数（ハイオク）　■ -->
                        <a class="ef" id="WF_HTANK_W">
                            <asp:TextBox ID="TxtHTank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　車数（レギュラー）　■ -->
                        <a class="ef" id="WF_RTANK_W">
                            <asp:TextBox ID="TxtRTank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　車数（灯油）　■ -->
                        <a class="ef" id="WF_TTANK_W">
                            <asp:TextBox ID="TxtTTank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　車数（未添加灯油）　■ -->
                        <a class="ef" id="WF_MTTANK_W">
                            <asp:TextBox ID="TxtMTTank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　車数（軽油）　■ -->
                        <a class="ef" id="WF_KTANK_W">
                            <asp:TextBox ID="TxtKTank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　車数（３号軽油）　■ -->
                        <a class="ef" id="WF_K3TANK_W">
                            <asp:TextBox ID="TxtK3Tank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　車数（５号軽油）　■ -->
                        <a class="ef" id="WF_K5TANK_W">
                            <asp:TextBox ID="TxtK5Tank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　車数（１０号軽油）　■ -->
                        <a class="ef" id="WF_K10TANK_W">
                            <asp:TextBox ID="TxtK10Tank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　車数（LSA）　■ -->
                        <a class="ef" id="WF_LTANK_W">
                            <asp:TextBox ID="TxtLTank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　車数（A重油）　■ -->
                        <a class="ef" id="WF_ATANK_W">
                            <asp:TextBox ID="TxtATank_w" runat="server" onblur="MsgClear();" Enabled="false" MaxLength="2">0</asp:TextBox>
                        </a>
                        <!-- ■　合計　■ -->
                        <a class="ef" id="WF_TOTALCNT_W">
                            <asp:TextBox ID="TxtTotalCnt_w" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        </a>
                    </div>
                </div>
                <!-- ボタン -->
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <input type="button" id="WF_ButtonALLSELECT_TAB1" class="btn-sticky" value="全選択" onclick="ButtonClick('WF_ButtonALLSELECT_TAB1');" />
                        <input type="button" id="WF_ButtonSELECT_LIFTED_TAB1" class="btn-sticky" value="選択解除"  onclick="ButtonClick('WF_ButtonSELECT_LIFTED_TAB1');" />
                        <input type="button" id="WF_ButtonLINE_LIFTED_TAB1" class="btn-sticky" value="行削除"  onclick="ButtonClick('WF_ButtonLINE_LIFTED_TAB1');" />
                        <input type="button" id="WF_ButtonLINE_ADD_TAB1" class="btn-sticky" value="行追加"  onclick="ButtonClick('WF_ButtonLINE_ADD_TAB1');" />
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonUPDATE_TAB1" class="btn-sticky" value="割当確定"  onclick="ButtonClick('WF_ButtonUPDATE_TAB1');" />
                    </div>
                </div>
                <!-- 一覧レイアウト -->
                <asp:panel id="pnlListArea1" runat="server" ></asp:panel>
            </asp:View>

            <!-- ■ Tab No2　入換・積込指示　■ -->
            <asp:View ID="WF_DView2" runat="server">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonUPDATE_TAB2" class="btn-sticky" value="入力内容登録" onclick="ButtonClick('WF_ButtonUPDATE_TAB2');" />
                        <input type="button" id="WF_ButtonFILLINGALL_TAB2" class="btn-sticky" value="充填ポイントの全体を見る"  onclick="ButtonClick('WF_ButtonUPDATE_TAB2');" style="display:none"/>
                    </div>
                </div>
                <!-- 一覧レイアウト -->
                <asp:panel id="pnlListArea2" runat="server" ></asp:panel>
            </asp:View>

            <!-- ■ Tab No3　タンク車明細　■ -->
            <asp:View ID="WF_DView3" runat="server">
                <!-- ボタン -->
                <div class="actionButtonBox">
                    <div class="leftSide">
<%-- 20200219_タンク車明細での明細の追加・削除などは想定しないため削除
                        <input type="button" id="WF_ButtonALLSELECT_TAB3" class="btn-sticky" value="全選択"  onclick="ButtonClick('WF_ButtonALLSELECT_TAB3');" />
                        <input type="button" id="WF_ButtonSELECT_LIFTED_TAB3" class="btn-sticky" value="選択解除"  onclick="ButtonClick('WF_ButtonSELECT_LIFTED_TAB3');" />
                        <input type="button" id="WF_ButtonLINE_LIFTED_TAB3" class="btn-sticky" value="行削除" onclick="ButtonClick('WF_ButtonLINE_LIFTED_TAB3');" />
                        <input type="button" id="WF_ButtonLINE_ADD_TAB3" class="btn-sticky" value="行追加"  onclick="ButtonClick('WF_ButtonLINE_ADD_TAB3');" />
--%>
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonUPDATE_TAB3" class="btn-sticky" value="明細更新"  onclick="ButtonClick('WF_ButtonUPDATE_TAB3');" />
                    </div>
                </div>
                <!-- 一覧レイアウト -->
                <asp:panel id="pnlListArea3" runat="server" ></asp:panel>
            </asp:View>

            <!-- ■ Tab No4　費用入力　■ -->
            <asp:View ID="WF_DView4" runat="server">
                <!-- ボタン -->
                <div class="actionButtonBox">
                    <div class="leftSide">
                        選択した明細の<input type="button" id="WF_ButtonLINE_LIFTED_TAB4" class="btn-sticky" value="行削除"  onclick="ButtonClick('WF_ButtonLINE_LIFTED_TAB4');" />
                        <input type="button" id="WF_ButtonLINE_ADD_TAB4" class="btn-sticky" value="行追加"  onclick="ButtonClick('WF_ButtonLINE_ADD_TAB4');" />
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonUPDATE_TAB4" class="btn-sticky" value="訂正更新"  onclick="ButtonClick('WF_ButtonUPDATE_TAB4');" />
                    </div>
                </div>
                <!-- 一覧レイアウト -->
                <asp:panel id="pnlListArea4" runat="server" ></asp:panel>
            </asp:View>
        </asp:MultiView>
        <!-- <div class="detailBottom"></div> -->
    </div>

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
        <!-- 貨車連結切替用フラグ -->
        <input id="WF_CREATELINKFLG" runat="server" value="" type="text" />
        <!-- 手配連絡フラグ -->
        <input id="WF_CONTACTFLG" runat="server" value="" type="text" />
        <!-- 結果受理フラグ -->
        <input id="WF_RESULTFLG" runat="server" value="" type="text" />
        <!-- 託送指示フラグ -->
        <input id="WF_DELIVERYFLG" runat="server" value="" type="text" />
        <!-- 画面ボタン制御 -->
        <input id="WF_MAPButtonControl" runat="server" value="0" type="text" />
        <!-- DetailBox Mview切替 -->
        <input id="WF_DTAB_CHANGE_NO" runat="server" value="" type="text"/>
        <!-- ヘッダーを表示するか保持、"1"(表示:初期値),"0"(非表示)  -->
        <asp:HiddenField ID="hdnDispHeaderItems" runat="server" Value="1" />
        <!-- 油種数登録ボタン押下フラグ(True:有効, False：無効) -->
        <input id="WF_ButtonInsertFLG" runat="server" value="" type="text" />
        <!-- 選択(チェックボックス)押下フラグ(True:有効, False：無効) -->
        <input id="WF_CheckBoxFLG" runat="server" value="" type="text" />
    </div>
</asp:Content>

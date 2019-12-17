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
        <div class="Operation" style="margin-left: 3em; margin-top: 0.5em; height: 1.8em;">

            <!-- ○ 固定項目 ○ -->
            <!-- ボタン -->
            <a style="position:fixed;top:2.8em;left:62.5em;">
                <input type="button" id="WF_ButtonINSERT" value="登録"  style="Width:5em" onclick="ButtonClick('WF_ButtonINSERT');" />
            </a>
            <a style="position:fixed;top:2.8em;left:67em;">
                <input type="button" id="WF_ButtonEND" value="戻る"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>

            <!-- ○ 変動項目 ○ -->
            <!-- 会社コード -->
            <a style="position:fixed; top:7.7em; left:4em; font-weight:bold; text-decoration:underline;display:none">会社コード</a>

            <a class="ef" style="position:fixed; top:7.7em; left:18em;display:none" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a style="position:fixed; top:7.7em; left:27em;display:none">
                <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>
            <!-- 運用部署 -->
            <a style="position:fixed; top:9.9em; left:4em; font-weight:bold; text-decoration:underline;display:none">運用部署</a>

            <a class="ef" style="position:fixed; top:9.9em; left:18em;display:none" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
                <asp:TextBox ID="WF_UORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a style="position:fixed; top:9.9em; left:27em;display:none">
                <asp:Label ID="WF_UORG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ■　ステータス　■ -->
            <a id="WF_ORDERSTATUS_LABEL">ステータス</a>
            <a class="ef" id="ORDERSTATUS">
                <asp:TextBox ID="TxtOrderStatus" runat="server" onblur="MsgClear();" Enabled="true"></asp:TextBox>
            </a>

            <!-- ■　情報　■ -->
            <a class="ef" id="WF_ORDERINFO">
                <asp:TextBox ID="TxtOrderInfo" runat="server" onblur="MsgClear();" Enabled="true"></asp:TextBox>
            </a>

            <!-- ■　受注パターン　■ -->
            <a id="WF_ORDERTYPE_LABEL">受注パターン</a>
            <a class="ef" id="WF_ORDERTYPE">
                <asp:TextBox CssClass="NoIcon" ID="TxtOrderType" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>

            <!-- ■　オーダー№　■ -->
            <a id="WF_ORDERNO_LABEL">オーダー№</a>
            <a class="ef" id="WF_ORDERNO">
                <asp:TextBox CssClass="NoIcon" ID="TxtOrderNo" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　荷主　■ -->
            <a id="WF_SHIPPERS_LABEL">荷主</a>
            <a class="ef" id="WF_SHIPPERSCODE">
                <asp:TextBox CssClass="NoIcon" ID="TxtShippersCode" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a id="WF_SHIPPERSNAME">
                <asp:Label ID="LblShippersName" runat="server" Width="17em" CssClass="WF_TEXT">JXTG</asp:Label>
            </a>
            <!-- ■　荷受人　■ -->
            <a id="WF_CONSIGNEE_LABEL">荷受人</a>
            <a class="ef" id="WF_CONSIGNEECODE">
                <asp:TextBox CssClass="NoIcon" ID="TxtConsigneeCode" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a id="WF_CONSIGNEENAME">
                <asp:Label ID="LblConsigneeName" runat="server" Width="17em" CssClass="WF_TEXT">JXTG</asp:Label>
            </a>

            <!-- ■　本線列車　■ -->
            <a id="WF_TRAINNO_LABEL">本線列車</a>
            <a class="ef" id="WF_TRAINNO">
                <asp:TextBox CssClass="NoIcon" ID="TxtTrainNo" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　発駅　■ -->
            <a id="WF_DEPSTATION_LABEL">発駅</a>
            <a class="ef" id="WF_DEPSTATIONCODE">
                <asp:TextBox CssClass="NoIcon" ID="TxtDepstationCode" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a id="WF_DEPSTATIONNAME">
                <asp:Label ID="LblDepstationName" runat="server" Width="17em" CssClass="WF_TEXT">JXTG</asp:Label>
            </a>
            <!-- ■　着駅　■ -->
            <a id="WF_ARRSTATION_LABEL">着駅</a>
            <a class="ef" id="WF_ARRSTATIONCODE">
                <asp:TextBox CssClass="NoIcon" ID="TxtArrstationCode" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <a id="WF_ARRSTATIONNAME">
                <asp:Label ID="LblArrstationName" runat="server" Width="17em" CssClass="WF_TEXT">JXTG</asp:Label>
            </a>

            <!-- ■　(予定)積込日　■ -->
            <a id="WF_LOADINGDATE_LABEL">(予定)積込日</a>
            <a class="ef" id="WF_LOADINGDATE" ondblclick="Field_DBclick('TxtLoadingDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtLoadingDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)発日　■ -->
            <a id="WF_DEPDATE_LABEL">発日</a>
            <a class="ef" id="WF_DEPDATE" ondblclick="Field_DBclick('TxtDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtDepDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)積車着日　■ -->
            <a id="WF_ARRDATE_LABEL">積車着日</a>
            <a class="ef" id="WF_ARRDATE" ondblclick="Field_DBclick('TxtArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtArrDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)受入日　■ -->
            <a id="WF_ACCDATE_LABEL">受入日</a>
            <a class="ef" id="WF_ACCDATE" ondblclick="Field_DBclick('TxtAccDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtAccDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(予定)空車着日　■ -->
            <a id="WF_EMPARRDATE_LABEL">空車着日</a>
            <a class="ef" id="WF_EMPARRDATE" ondblclick="Field_DBclick('TxtEmparrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtEmparrDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>

            <!-- ■　(実績)積込日　■ -->
            <a id="WF_ACTUALLOADINGDATE_LABEL">(実績)積込日</a>
            <a class="ef" id="WF_ACTUALLOADINGDATE" ondblclick="Field_DBclick('TxtActualLoadingDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtActualLoadingDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(実績)発日　■ -->
            <a id="WF_ACTUALDEPDATE_LABEL">発日</a>
            <a class="ef" id="WF_ACTUALDEPDATE" ondblclick="Field_DBclick('TxtActualDepDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtActualDepDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(実績)積車着日　■ -->
            <a id="WF_ACTUALARRDATE_LABEL">積車着日</a>
            <a class="ef" id="WF_ACTUALARRDATE" ondblclick="Field_DBclick('TxtActualArrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtActualArrDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(実績)受入日　■ -->
            <a id="WF_ACTUALACCDATE_LABEL">受入日</a>
            <a class="ef" id="WF_ACTUALACCDATE" ondblclick="Field_DBclick('TxtActualAccDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtActualAccDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
            <!-- ■　(実績)空車着日　■ -->
            <a id="WF_ACTUALEMPARRDATE_LABEL">空車着日</a>
            <a class="ef" id="WF_ACTUALEMPARRDATE" ondblclick="Field_DBclick('TxtActualEmparrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                <asp:TextBox CssClass="CalendarIcon" ID="TxtActualEmparrDate" runat="server" onblur="MsgClear();"></asp:TextBox>
            </a>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div  class="detailbox" id="detailbox">
        <div id="detailkeybox">
            <p id="KEY_LINE_1">
                <!-- ■　Dタブ　■ -->
                <a onclick="DtabChange('0')">
                    <asp:Label BackColor="LightSkyBlue" BorderStyle="Groove" ID="WF_Dtab01" runat="server" Text="タンク車割当" Height="1.3em" Width="6.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                </a>

                <a onclick="DtabChange('1')">
                    <asp:Label BackColor="LightGray" BorderStyle="Groove" ID="WF_Dtab02" runat="server" Text="タンク車明細" Height="1.3em" Width="6.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                </a>

                <a onclick="DtabChange('2')">
                    <asp:Label BackColor="LightGray" BorderStyle="Groove" ID="WF_Dtab03" runat="server" Text="入換・積込指示" Height="1.3em" Width="7.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                </a>

                <a onclick="DtabChange('3')">
                    <asp:Label BackColor="LightGray" BorderStyle="Groove" ID="WF_Dtab04" runat="server" Text="費用入力" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="small"></asp:Label>
                </a>
            </p> 
        </div> 

        <asp:MultiView ID="WF_DetailMView" runat="server">

            <!-- ■ Tab No1　タンク車割当　■ -->
            <asp:View ID="WF_DView1" runat="server" >
                <!-- 一覧レイアウト -->
                <div id="divListArea1">
                    <asp:panel id="pnlListArea1" runat="server" ></asp:panel>
                </div>
            </asp:View>

            <!-- ■ Tab No2　タンク車明細　■ -->
            <asp:View ID="WF_DView2" runat="server">
                <!-- 一覧レイアウト -->
                <div id="divListArea2">
                    <asp:panel id="pnlListArea2" runat="server" ></asp:panel>
                </div>
            </asp:View>

            <!-- ■ Tab No3　入換・積込指示　■ -->
            <asp:View ID="WF_DView3" runat="server">
                <!-- 一覧レイアウト -->
                <div id="divListArea3">
                    <asp:panel id="pnlListArea3" runat="server" ></asp:panel>
                </div>
            </asp:View>

            <!-- ■ Tab No4　費用入力　■ -->
            <asp:View ID="WF_DView4" runat="server">
                <!-- 一覧レイアウト -->
                <div id="divListArea4">
                    <asp:panel id="pnlListArea4" runat="server" ></asp:panel>
                </div>
            </asp:View>
        </asp:MultiView>

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
        <!-- DetailBox Mview切替 -->
        <input id="WF_DTAB_CHANGE_NO" runat="server" value="" type="text"/>

    </div>
</asp:Content>

<%@ Page Title="OIT0002S" Language="vb" AutoEventWireup="false" CodeBehind="OIT0002LinkSearch.aspx.vb" Inherits="JOTWEB.OIT0002LinkSearch" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content id="OIT0002SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0002S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0002S.js")%>'></script>
</asp:Content>

<asp:Content ID="OIT0002S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO"  class="btn-sticky" value="検索" onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 会社コード -->
            <div class="inputItem">
                <a id="WF_CAMPCODE_LABEL" style="display:none">会社コード</a>
                <a class="ef" id="WF_CAMPCODE_CODE" style="display:none" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_CAMPCODE_NAME" style="display:none">
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 組織コード -->
            <div class="inputItem">
                <a id="WF_ORG_LABEL" style="display:none">組織コード</a>
                <a class="ef" id="WF_ORG_CODE" style="display:none" ondblclick="Field_DBclick('WF_ORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_ORG');">
                    <asp:TextBox ID="WF_ORG" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_ORG_NAME" style="display:none">
                    <asp:Label ID="WF_ORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 返送列車番号 -->
            <div class="inputItem">
                <a id="WF_BTRAINNO_LABEL">返送列車番号</a>
                <a class="ef" id="WF_BTRAINNO"  ondblclick="Field_DBclick('TxtBTrainNo', <%=LIST_BOX_CLASSIFICATION.LC_BTRAINNUMBER%>);" onchange="TextBox_change('TxtBTrainNo');">
                    <asp:TextBox ID="TxtBTrainNo" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </a>
                <a id="WF_BTRAINNO_TEXT">
                    <asp:Label ID="LblBTrainNo" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 空車着日 -->
            <div class="inputItem">
                <a id="WF_EMPARRDATE_LABEL" class="requiredMark">空車着日</a>
                <a class="ef" id="WF_EMPARRDATE" ondblclick="Field_DBclick('TxtEmparrDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtEmparrDate" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </a>
                <a id="WF_EMPARRDATE_SYMBOL_LABEL" >～</a>
            </div>

<%-- ### 20200722 START 貨車連結順序表(検索)の見直しに伴い削除 ##########
            <!-- 空車着駅（発駅）コード -->
            <div class="inputItem">
                <a id="WF_RETSTATION_LABEL" class="requiredMark">空車着駅</a>
                <a class="ef" id="WF_RETSTATION" ondblclick="Field_DBclick('WF_RETSTATION', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('WF_RETSTATION');">
                    <asp:TextBox ID="WF_RETSTATION_CODE" runat="server" CssClass="boxIcon"  onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </a>
                <a id="WF_RETSTATION_TEXT">
                    <asp:Label ID="WF_RETSTATION_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 利用可能日 -->
            <div class="inputItem">
                <a id="WF_STYMD_LABEL" class="requiredMark">利用可能日</a>
                <a class="ef" id="WF_STYMD" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_STYMD_CODE" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </a>
                <a id="WF_ENDYMD_LABEL" >～</a>
            </div>

            <!-- 本線列車番号 -->
            <div class="inputItem">
                <a id="WF_TRAINNO_LABEL">列車番号</a>
                <a class="ef" id="WF_TRAINNO">
                    <asp:TextBox ID="WF_TRAINNO_CODE" runat="server" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                </a>
                <a id="WF_TRAINNO_TEXT">
                    <asp:Label ID="WF_TRAINNO_NAME" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
        
            <!-- ステータス選択 -->
            <div class="inputItem">
                <a id="WF_SW_LABEL">ステータス選択</a>
       
                <a  class="inline-radio" id="checkbox">
                    <div class="grc0001Wrapper">
                        <ul>
                            <li>
                                <asp:RadioButton ID="WF_SW1" runat="server" GroupName="WF_SW" Text="利用可のみ表示" />
                            </li>
                            <li>
                                <asp:RadioButton ID="WF_SW2" runat="server" GroupName="WF_SW" Text="全て表示" />
                            </li>
                        </ul>
                    </div>
                </a>
            </div>
     ### 20200722 END   貨車連結順序表(検索)の見直しに伴い削除 ########## --%>
        </div> <!-- End inputBox -->
    </div> <!-- End searchbox -->

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
    </div>
</asp:Content>

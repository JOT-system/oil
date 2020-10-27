<%@ Page Title="OIT0007L" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0007FileInputList.aspx.vb" Inherits="JOTWEB.OIT0007FileInputList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>
<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>
<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<asp:Content ID="OIT0007LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0007L.css")%>' rel="stylesheet" type="text/css" /> 
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0007L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        // 添付許可拡張子
        var acceptExtentions = [<%= Me.AcceptExtentions %>];
        var acceptExtentionsStr = "許可ファイル種類(" + acceptExtentions.join(',') + ")";
        // Uploadハンドラー
        var handlerUrl = '<%=ResolveUrl("~/OIL/inc/OIM0020FILEUPLOAD.ashx")%>';
    </script>
</asp:Content>
<asp:Content ID="OIT0007L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <!-- 会社 -->
                        <asp:Label ID="WF_SEL_CAMPCODE" runat="server" Text="会社" Visible="false"></asp:Label>
                        <asp:Label ID="WF_SEL_CAMPNAME" runat="server" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>

                        <!-- 運用部署 -->
                        <asp:Label ID="WF_SELUORG_L" runat="server" Text="運用部署" Visible="false"></asp:Label>
                        <asp:Label ID="WF_SELUORG_TEXT" runat="server" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>
                        <!-- 左ボタン -->
                        <asp:FileUpload ID="fupAttachment" runat="server" />
                        <input type="button" id="btnFileUpload" class="btn-sticky" value="取込ファイル選択" runat="server"  />
                        <div class="filterDateFiledWrapper">
                        <asp:RadioButtonList ID="rblFilterDateFiled" runat="server"  RepeatDirection="Horizontal">
                            <asp:ListItem Text="積込日" Value="LODDATE" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="発日" Value="DEPDATE"></asp:ListItem>
                        </asp:RadioButtonList>
                        </div>
                        <a class="ef" id="WF_FILTERDATE" ondblclick="Field_DBclick('WF_FILTERDATE', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                            <asp:TextBox ID="WF_FILTERDATE_TEXT" CssClass="calendarIcon" runat="server"></asp:TextBox>
                        </a>
                       
                        <input type="button" id="WF_ButtonFilter" class="btn-sticky" value="絞込" title="選択は解除されます"  onclick="ButtonClick('WF_ButtonFilter');" />
                        <input type="button" id="WF_ButtonFilterClear" class="btn-sticky" value="絞込解除"   onclick="ButtonClick('WF_ButtonFilterClear');" />
                    </div>
                    <div class="rightSide">
                        <!-- 右ボタン -->
                        <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"   onclick="ButtonClick('WF_ButtonEND');" />
                        <!-- 先頭行・末尾行ボタンを表示させる場合は divの括りを無くして WF_ButtonXXXを外だしにすれば出ます -->
                        <div style="display:none;">
                            <div id="WF_ButtonFIRST" class="firstPage" runat="server" onclick="ButtonClick('WF_ButtonFIRST');"></div>
                            <div id="WF_ButtonLAST" class="lastPage" runat="server" onclick="ButtonClick('WF_ButtonLAST');"></div>
                        </div>
                    </div>
                </div>
 
            </div>
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
            <div id="divUpdList">

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
            <!-- ファイル名一覧 -->
            <input id="WF_FILENAMELIST" runat="server" value="" type="text" />
        </div>
</asp:Content>
<asp:Content ID="ctCostumPopUpTitle" ContentPlaceHolderID="contentsPopUpTitle" runat="server">
</asp:Content>
<asp:Content ID="ctCostumPopUp" ContentPlaceHolderID="contentsPopUpInside" runat="server">
</asp:Content>

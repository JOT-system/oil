<%@ Page Title="OIM0020C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIM0020GuidanceCreate.aspx.vb" Inherits="JOTWEB.OIM0020GuidanceCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>
<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIM0020WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIM0020CH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIM0020C.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIM0020C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        // 添付許可拡張子
        var acceptExtentions = ["xlsx", "docx", "pptx", "jpg", "png","bmp" , "zip", "gif", "csv", "txt", "pdf","lzh"];
    </script>
</asp:Content>
<asp:Content ID="OIM0020C" ContentPlaceHolderID="contents1" runat="server">
    <div class="detailboxOnly" id="detailbox">
        <div id="detailbuttonbox" class="detailbuttonbox">
            <div class="actionButtonBox">
                <div class="leftSide">
                </div>
                <div class="rightSide">
                    <input type="button" id="WF_UPDATE" class="btn-sticky" value="更新" onclick="ButtonClick('WF_UPDATE');" />
                    <input type="button" id="WF_CLEAR"  class="btn-sticky" value="戻る" onclick="ButtonClick('WF_CLEAR');" />
                </div>
            </div>
        </div>
        <table class="input">
            <colgroup>
                <col /><col /><col /><col />
            </colgroup>
            <tbody>
                <tr>
                    <th>ガイダンス登録日</th>
                    <td>
                        <asp:Label ID="lblGuidanceEntryDate" runat="server" Text=""></asp:Label>
                    </td>
                    <th>種類</th>
                    <td>
                        <div class="grc0001Wrapper type">
                            <asp:RadioButtonList ID="rblType" runat="server"  ClientIDMode="Predictable" RepeatLayout="UnorderedList"></asp:RadioButtonList>
                        </div>
                    </td>
                </tr>
                <tr>
                    <th>掲載開始日</th>
                    <td>
                        <a class="ef" id="WF_FROMYMD" ondblclick="Field_DBclick('WF_FROMYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                            <asp:TextBox ID="txtFromYmd" CssClass="calendarIcon" runat="server"></asp:TextBox>
                        </a>
                    </td>
                    <th>掲載終了日</th>
                    <td>
                        <a class="ef"  id="WF_ENDYMD"  ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                            <asp:TextBox ID="txtEndYmd" CssClass="calendarIcon" runat="server"></asp:TextBox>
                        </a>
                    </td>
                </tr>
                <tr>
                    <th>タイトル</th>
                    <td colspan="3"><asp:TextBox ID="txtTitle" runat="server"></asp:TextBox></td>
                </tr>
                <tr>
                    <th class="top">対象</th>
                    <td colspan="3">
                        <div class="grc0001Wrapper flags">
                            <asp:CheckBoxList ID="chklFlags" runat="server"  ClientIDMode="Predictable" RepeatLayout="UnorderedList"></asp:CheckBoxList>
                        </div>
                    </td>
                </tr>
                <tr>
                    <th class="top">内容</th>
                    <td colspan="3">
                        <asp:TextBox ID="txtNaiyou" runat="server" TextMode="MultiLine"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <th class="top">添付</th>
                    <td class="attachmentCell" colspan="3">
                        <div id="divAttachmentArea" class="fileDrag">
                            <div class="uploadLine">
                                <input type="button" class="btn-sticky" value="ファイル追加" />
                                <span>ボタンクリック、またはここにファイルをドラッグ＆ドロップ</span>
                                <hr />
                            </div>
                            <asp:Repeater ID="repAttachments" runat="server" ClientIDMode="Predictable">
                                <ItemTemplate >
                                    <div><span class="delAttachment" title="削除">×</span><span><%# Eval("FileName") %></span></div>
                                </ItemTemplate>
                            </asp:Repeater>

                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
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

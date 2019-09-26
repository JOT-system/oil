<%@ Page Title="TA0002" Language="vb" AutoEventWireup="false" CodeBehind="GRTA0002SELECT.aspx.vb" Inherits="OFFICE.GRTA0002SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0003SRightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="~/GR/inc/GRTA0002WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="TA0002SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/TA0002S.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/TA0002S.js")%>"></script>
</asp:Content>
<asp:Content ID="TA0002S" ContentPlaceHolderID="contents1" runat="server">
       <!-- 全体レイアウト　headerbox -->
        <div  class="searchbox" id="searchbox">
            <!-- ○ 固定項目 ○ -->
            <div id="searchbuttonbox" class="searchbuttonbox">
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonDO" value="実行"  style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
            </div>
            <div id="searchkeybox">
                <p class="LINE_1">
                <!-- 　会社コード　 -->
                    <a style="position:fixed;top:7.7em;left:4em;font-weight:bold;text-decoration:underline">会社コード</a>
                    <a style="position:fixed;top:7.7em;left:11.5em;"></a>
                    <a style="position:fixed;top:7.7em;left:18em;"  ondblclick="Field_DBclick('WF_CAMPCODE' ,  <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>)" onchange="TextBox_change('WF_CAMPCODE');">
                        <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:7.7em;left:27em;">
                        <asp:Label ID="WF_CAMPCODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_2">
                    <!-- 　対象年月　 -->
                    <a style="position:fixed;top:9.9em;left:4em;font-weight:bold;text-decoration:underline">対象年月</a>
                    <a style="position:fixed;top:9.9em;left:11.5em;"></a>
                    <a style="position:fixed;top:9.9em;left:18em;" ondblclick="Field_DBclick('WF_TAISHOYM',  <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_TAISHOYM" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                </p>
                <p class="LINE_3"> 
                    <!-- 　配属部署　 -->
                    <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">配属部署</a>
                    <a style="position:fixed;top:12.1em;left:11.5em;"></a>
                    <a style="position:fixed;top:12.1em;left:18em;" ondblclick="Field_DBclick('WF_HORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>)" onchange="TextBox_change('WF_HORG');">
                        <asp:TextBox ID="WF_HORG" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:12.1em;left:27em;">
                        <asp:Label ID="WF_HORG_TEXT" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_4">             
                    <!-- 　職務区分　 -->
                    <a style="position:fixed;top:14.3em;left:4em;font-weight:bold;text-decoration:underline">職務区分</a>
                    <a style="position:fixed;top:14.3em;left:11.5em;"></a>
                    <a style="position:fixed;top:14.3em;left:18em;" ondblclick="Field_DBclick('WF_STAFFKBN' , <%=LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST%>)" onchange="TextBox_change('WF_STAFFKBN');">
                        <asp:TextBox ID="WF_STAFFKBN" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:14.3em;left:27em;">
                        <asp:Label ID="WF_STAFFKBN_TEXT" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_5">             
                    <!-- 　従業員コード　 -->
                    <a style="position:fixed;top:16.5em;left:4em;font-weight:bold;text-decoration:underline">従業員</a>
                    <a style="position:fixed;top:16.5em;left:11.5em;">コード</a>
                    <a style="position:fixed;top:16.5em;left:18em;" ondblclick="Field_DBclick('WF_STAFFCODE' ,  <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE%>)" onchange="TextBox_change('WF_STAFFCODE');">
                        <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:16.5em;left:27em;">
                        <asp:Label ID="WF_STAFFCODE_TEXT" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>

                <p class="LINE_6">             
                    <!-- 　従業員名称　 -->
                    <a style="position:fixed;top:18.7em;left:4em;"></a>
                    <a style="position:fixed;top:18.7em;left:11.5em;">名称</a>
                    <a style="position:fixed;top:18.7em;left:18em;">
                        <asp:TextBox ID="WF_STAFFNAME" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                </p>
            </div>

            <a hidden="hidden">
                <input id="WF_FIELD"  runat="server" value=""  type="text" />          <!-- Textbox DBクリックフィールド -->
                <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />  <!-- Textbox DBクリックフィールド -->
                <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />    <!-- Textbox DBクリックフィールド -->
                <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>   <!-- Textbox DBクリックフィールド -->
                <input id="WF_RightViewChange" runat="server" value="" type="text"/>   <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />    <!-- Rightbox 開閉 -->
                <input id="WF_ButtonClick" runat="server" value=""  type="text" />     <!-- ボタン押下 -->
           </a>
        </div>

        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>

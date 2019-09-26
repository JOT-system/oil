<%@ Page Title="T00005S" Language="vb" AutoEventWireup="false" CodeBehind="GRT00005SELECT.aspx.vb" Inherits="OFFICE.GRT00005SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0003SRightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00005WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00005SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00005S.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00005S.js")%>"></script>
</asp:Content>
<asp:Content ID="T00005S" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　searchbox -->
        <div  class="searchbox" id="searchbox">
            <!-- ○ 固定項目 ○ -->
            <div id="searchbuttonbox" class="searchbuttonbox" >
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonRESTART" value="再開"  style="Width:5em" onclick="ButtonClick('WF_ButtonRESTART');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonDO" value="実行"  style="Width:5em" onclick="ButtonClick('WF_ButtonDO');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
            </div>
            <div id="searchkeybox" class="searchkeybox">
                <p class="LINE_1">
                    <!-- 　会社コード　 -->
                    <a style="position:fixed;top:7.7em;left:4em;font-weight:bold;text-decoration:underline">会社コード</a>
                    <a style="position:fixed;top:7.7em;left:11.5em;"></a>
                    <a style="position:fixed;top:7.7em;left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE' ,  <%=LIST_BOX_CLASSIFICATION.LC_COMPANY  %>)" onchange="TextBox_change('WF_CAMPCODE')">
                        <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:7.7em;left:27em;">
                        <asp:Label ID="WF_CAMPCODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_2">
                    <!-- 　年度　 -->
                    <a style="position:fixed;top:9.9em;left:4em;font-weight:bold;text-decoration:underline">出庫年月日</a>
                    <a style="position:fixed;top:9.9em;left:11.5em;">範囲指定</a>
                    <a style="position:fixed;top:9.9em;left:18em;" ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
                        <asp:TextBox ID="WF_STYMD" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:9.9em;left:42.5em;">～</a>
                    <a style="position:fixed;top:9.9em;left:44em;" ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
                        <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                </p>
                <p class="LINE_3">
                    <!-- 　運用部署　 -->
                    <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">運用部署</a>
                    <a style="position:fixed;top:12.1em;left:11.5em;"></a>
                    <a style="position:fixed;top:12.1em;left:18em;" ondblclick="Field_DBclick('WF_UORG' ,  <%=LIST_BOX_CLASSIFICATION.LC_ORG%>)" onchange="TextBox_change('WF_UORG')">
                        <asp:TextBox ID="WF_UORG" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:12.1em;left:27em;">
                        <asp:Label ID="WF_UORG_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_4">
                    <!-- 　従業員コード　 -->
                    <a style="position:fixed;top:14.3em;left:4em;font-weight:bold;text-decoration:underline">従業員</a>
                    <a style="position:fixed;top:14.3em;left:11.5em;">コード</a>

                    <a style="position:fixed;top:14.3em;left:18em;" ondblclick="Field_DBclick('WF_STAFFCODE' ,  <%=LIST_BOX_CLASSIFICATION.LC_STAFFCODE %>)" onchange="TextBox_change('WF_STAFFCODE')">
                        <asp:TextBox ID="WF_STAFFCODE" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:14.3em;left:27em;">
                        <asp:Label ID="WF_STAFFCODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_5">
                    <!-- 　従業員名称　 -->
                    <a style="position:fixed;top:16.5em;left:4em;"></a>
                    <a style="position:fixed;top:16.5em;left:11.5em;">名称</a>

                    <a style="position:fixed;top:16.5em;left:18em;" onchange="TextBox_change('WF_STAFFNAME')">
                        <asp:TextBox ID="WF_STAFFNAME" runat="server" Height="1.4em" Width="10em" ></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:16.5em;left:27em;"></a>
                </p> 
            </div> 
            <div id="calendarbox" class="calendarbox">
                <div id="calendarkeybox" class="calendarkeybox">
                    <a class="arrow-left" onclick="ButtonClick('WF_BEFORE');" ></a>
                    <asp:Label ID="WF_IMPYM" runat="server" Text="" Width="6em" CssClass="WF_TEXT_CENTER"></asp:Label>
                    <a class="arrow-right" onclick="ButtonClick('WF_AFTER');" ></a>
                    <a>
                        <input type="button" id="WF_IMPORT" value="確認"  style="Width:5em" onclick="ButtonClick('WF_ButtonCHECK');" />
                    </a>
                </div>
                <asp:Panel ID="WF_NIPPO_CALENDAR" runat="server" ></asp:Panel>
            </div>
            <a hidden="hidden">
                <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>     <!-- GridViewダブルクリック -->
                <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>    <!-- GridView表示位置フィールド -->

                <input id="WF_FIELD"  runat="server" value=""  type="text" />               <!-- Textbox DBクリックフィールド -->
                <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />         <!-- Textbox DBクリックフィールド -->
                <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>        <!-- Textbox DBクリックフィールド -->

                <input id="WF_SelectedIndex" runat="server" value="" type="text"/>          <!-- Textbox DBクリックフィールド -->

                <input id="WF_RightViewChange" runat="server" value="" type="text"/>        <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />         <!-- Rightbox 開閉 -->

                <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->
                <input id="WF_Restart" runat="server" value=""  type="text" />              <!-- 一時保管 -->
                
                <input id="WF_LISTDAY" runat="server" value ="" type="text" />
            </a>
        </div>
   
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <!-- Work レイアウト -->
        <LSINC:work id="work" runat="server" />

</asp:Content>


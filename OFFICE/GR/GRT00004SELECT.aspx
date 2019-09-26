<%@ Page Title="T00004S" Language="vb" AutoEventWireup="false" CodeBehind="GRT00004SELECT.aspx.vb" Inherits="OFFICE.GRT00004SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0003SRightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRT00004WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="T00004SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/T00004S.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/T00004S.js")%>"></script>
</asp:Content> 
<asp:Content ID="T00004S" ContentPlaceHolderID="contents1" runat="server">
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
                        <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:7.7em;left:27em;">
                        <asp:Label ID="WF_CAMPCODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_2">
                    <!-- 　出庫日　 -->
                    <a style="position:fixed;top:9.9em;left:4em;font-weight:bold;text-decoration:underline">出庫日</a>
                    <a style="position:fixed;top:9.9em;left:11.5em;">範囲指定</a>
                    <a style="position:fixed;top:9.9em;left:18em;" ondblclick="Field_DBclick('WF_SHUKODATEF', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_SHUKODATEF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:9.9em;left:42.5em;">～</a>
                    <a style="position:fixed;top:9.9em;left:44em;" ondblclick="Field_DBclick('WF_SHUKODATET', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_SHUKODATET" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                </p>
                <p class="LINE_3">
                    <!-- 　出荷日　 -->
                    <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">出荷日</a>
                    <a style="position:fixed;top:12.1em;left:11.5em;">範囲指定</a>
                    <a style="position:fixed;top:12.1em;left:18em;" ondblclick="Field_DBclick('WF_SHUKADATEF', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_SHUKADATEF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:12.1em;left:42.5em;">～</a>
                    <a style="position:fixed;top:12.1em;left:44em;"" ondblclick="Field_DBclick('WF_SHUKADATET', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_SHUKADATET" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                </p>
                <p class="LINE_4">
                    <!-- 　届日　 -->
                    <a style="position:fixed;top:14.3em;left:4em;font-weight:bold;text-decoration:underline">届日</a>
                    <a style="position:fixed;top:14.3em;left:11.5em;">範囲指定</a>
                    <a style="position:fixed;top:14.3em;left:18em;" ondblclick="Field_DBclick('WF_TODOKEDATEF', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_TODOKEDATEF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:14.3em;left:42.5em;">～</a>
                    <a style="position:fixed;top:14.3em;left:44em;" ondblclick="Field_DBclick('WF_TODOKEDATET', <%= LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_TODOKEDATET" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                </p>
                <p class="LINE_5">
                    <!-- 　受注部署　 -->
                    <a style="position:fixed;top:16.5em;left:4em;font-weight:bold;text-decoration:underline">受注部署</a>
                    <a style="position:fixed;top:16.5em;left:11.5em;"></a>
                    <a style="position:fixed;top:16.5em;left:18em;" ondblclick="Field_DBclick('WF_ORDERORG' ,<%= LIST_BOX_CLASSIFICATION.LC_ORG%>)" onchange="TextBox_change('WF_ORDERORG')">
                        <asp:TextBox ID="WF_ORDERORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:16.5em;left:27em;">
                        <asp:Label ID="WF_ORDERORG_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_6">
                    <!-- 　出荷部署　 -->
                    <a style="position:fixed;top:18.7em;left:4em;font-weight:bold;text-decoration:underline">出荷部署</a>
                    <a style="position:fixed;top:18.7em;left:11.5em;"></a>
                    <a style="position:fixed;top:18.5em;left:18em;" ondblclick="Field_DBclick('WF_SHIPORG' ,<%= LIST_BOX_CLASSIFICATION.LC_ORG%>)" onchange="TextBox_change('WF_SHIPORG')">
                        <asp:TextBox ID="WF_SHIPORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:18.7em;left:27em;">
                        <asp:Label ID="WF_SHIPORG_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_7">
                    <!-- 　油種　 -->
                    <a style="position:fixed;top:20.9em;left:4em;font-weight:bold;text-decoration:underline">油種</a>
                    <a style="position:fixed;top:20.9em;left:11.5em;"></a>
                    <a style="position:fixed;top:20.9em;left:18em;" ondblclick="Field_DBclick('WF_OILTYPE' ,<%= LIST_BOX_CLASSIFICATION.LC_OILTYPE%>)" onchange="TextBox_change('WF_OILTYPE')">
                        <asp:TextBox ID="WF_OILTYPE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:20.9em;left:27em;">
                        <asp:Label ID="WF_OILTYPE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
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

                <input id="WF_Restart" runat="server" value=""  type="text" />         <!-- 一時保管 -->
            </a>
        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>

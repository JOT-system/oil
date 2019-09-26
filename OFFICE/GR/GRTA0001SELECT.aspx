<%@ Page Title="TA0001" Language="vb" AutoEventWireup="false" CodeBehind="GRTA0001SELECT.aspx.vb" Inherits="OFFICE.GRTA0001SELECT" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %> 

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ register src="~/inc/GRIS0003SRightBox.ascx" tagname="rightview" tagprefix="MSINC" %>
<%@ register src="~/inc/GRIS0005LeftBox.ascx" tagname="leftview" tagprefix="MSINC" %>
<%@ register src="inc/GRTA0001WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="TA0001SH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/GR/css/TA0001S.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/GR/script/TA0001S.js")%>"></script>
</asp:Content>
<asp:Content ID="TA0001S" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　headerbox -->
        <div  class="searchbox" id="searchbox" >
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
            <!-- ○ 変動項目 ○ -->
                <p class="LINE_1">
                    <!-- 　会社コード　 -->
                    <a style="position:fixed;top:7.7em;left:4em;font-weight:bold;text-decoration:underline">会社コード</a>
                    <a style="position:fixed;top:7.7em;left:18em;" ondblclick="Field_DBclick('WF_CAMPCODE' ,  <%=LIST_BOX_CLASSIFICATION.LC_COMPANY  %>)">
                        <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:7.7em;left:27em;">
                        <asp:Label ID="WF_CAMPCODE_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p> 
                <p class="LINE_2">
                    <!-- 　出庫日　 -->
                    <a style="position:fixed;top:9.9em;left:4em;font-weight:bold;text-decoration:underline">出庫日</a>
                    <a style="position:fixed;top:9.9em;left:18em;" ondblclick="Field_DBclick('WF_SHUKODATEF', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR  %>)">
                        <asp:TextBox ID="WF_SHUKODATEF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:9.9em;left:11.5em;"></a>
                </p>
                <p class="LINE_3">
                    <!-- 　出荷部署　 -->
                    <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">出荷部署</a>
                    <a style="position:fixed;top:12.1em;left:18em;" ondblclick="Field_DBclick('WF_SHIPORG' ,  <%=LIST_BOX_CLASSIFICATION.LC_ORG  %>)">
                        <asp:TextBox ID="WF_SHIPORG" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
                    </a>
                    <a style="position:fixed;top:12.1em;left:27em;">
                        <asp:Label ID="WF_SHIPORG_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </p>
                <p class="LINE_4">
                    <!-- 　機能選択　 -->
                    <a style="position:fixed;top:14.3em;left:4em;font-weight:bold;">機能選択</a>
                    <a style="position:fixed;top:14.4em;left:18em;">
                        <span style="position:relative;left:0.2em;top:0.2em;" >
                            <asp:RadioButton ID="WF_right_SW1" runat="server" GroupName="rightbox" Text=" 乗務員指定(副乗務員指定含む)" Width="20em" Checked="true" />
                        </span><br/>
                        <span style="position:relative;left:0.2em;top:0.2em;">
                            <asp:RadioButton ID="WF_right_SW2" runat="server" GroupName="rightbox" Text=" 車番指定" Width="20em"/>
                        </span><br/>
                        <span  style="position:relative;left:0.2em;top:0.2em;">
                            <asp:RadioButton ID="WF_right_SW3" runat="server" GroupName="rightbox" Text=" 出荷場所指定(積配除く)" Width="20em"/>
                        </span>
                    </a>
                    <a></a>
                </p>
            </div>

            <a hidden="hidden">
                <input id="WF_FIELD"  runat="server" value=""  type="text" />               <!-- Textbox DBクリックフィールド -->
                <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />  <!-- Textbox DBクリックフィールド -->
                <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />    <!-- Textbox DBクリックフィールド -->
                <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>   <!-- Textbox DBクリックフィールド -->
                <input id="WF_RightViewChange" runat="server" value="" type="text"/>      <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />       <!-- Rightbox 開閉 -->

                <input id="WF_ButtonClick" runat="server" value=""  type="text" />          <!-- ボタン押下 -->
            </a>
        </div>
        <%-- rightview --%>
        <MSINC:rightview id="rightview" runat="server" />
        <%-- leftview --%>
        <MSINC:leftview id="leftview" runat="server" />
        <%-- Work --%>
        <LSINC:work id="work" runat="server" />
</asp:Content>

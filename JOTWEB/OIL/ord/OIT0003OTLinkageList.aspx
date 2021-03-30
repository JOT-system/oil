<%@ Page Title="OIT0003OTL" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0003OTLinkageList.aspx.vb" Inherits="JOTWEB.OIT0003OTLinkageList" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content id="OIT0003OTLH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0003OTL.css")%>' rel="stylesheet" type="text/css" /> 
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0003OTL.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="OIT0003OTL" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly <%= If(Me.ShowReserveModifiedMode, "showModMode", "") %>" id="headerbox" >
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
                        <input type="button" id="WF_ButtonALLSELECT" class="btn-sticky" value="全選択"  onclick="ButtonClick('WF_ButtonALLSELECT');" />
                        <input type="button" id="WF_ButtonSELECT_LIFTED" class="btn-sticky" value="選択解除"  onclick="ButtonClick('WF_ButtonSELECT_LIFTED');" />
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
                        <input type="button" id="WF_ButtonOtSend" class="btn-sticky" value="OT発送日報送信" runat="server" onclick="ButtonClick('WF_ButtonOtSend');" />
                        <input type="button" id="WF_ButtonReserved" class="btn-sticky" value="製油所出荷予約" runat="server" onclick="ButtonClick('WF_ButtonReserved');" />
                        <input type="button" id="WF_ButtonTakusou" class="btn-sticky" value="託送指示" runat="server" onclick="ButtonClick('WF_ButtonTakusou');" />
                        <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"   onclick="ButtonClick('WF_ButtonEND');" />
                        <!-- 先頭行・末尾行ボタンを表示させる場合は divの括りを無くして WF_ButtonXXXを外だしにすれば出ます -->
                        <div style="display:none;">
                            <div id="WF_ButtonFIRST" class="firstPage" runat="server" onclick="ButtonClick('WF_ButtonFIRST');"></div>
                            <div id="WF_ButtonLAST" class="lastPage" runat="server" onclick="ButtonClick('WF_ButtonLAST');"></div>
                        </div>
                    </div>
                </div>
                 <asp:Panel ID="pnlReserveModActionBox" runat="server" Visible='<%# Me.ShowReserveModifiedMode %>'>
                    <input type="button" id="WF_ButtonReserveMod" class="btn-sticky" value="出荷予約訂正指示" runat="server" onclick="ButtonClick('WF_ButtonReserveMod');" />
                    <span id="showCanceldOrder" onchange="ButtonClick('WF_ButtonFilter_Mod');">
                        <asp:CheckBox ID="chkShowCanceldOrder" runat="server" Text="削除した列車を表示" />
                    </span>
                </asp:Panel>
            </div>
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
            <div id="divModFileDlList" class='<%= Me.ShowModFileDlChkConfirm  %>'>
                <asp:HiddenField ID="hdnModFileDlChkConfirmIsActive" runat="server" Value="" />
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <span>選択した変更・削除予約ファイルをダウンロードします。</span>
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonModDownLoad" class="btn-sticky" value="ダウンロード"   onclick="ButtonClick('WF_ButtonReserveModDownload');" />
                        <input type="button" id="btnCloseModDownLoadConfirm" class="btn-sticky" value="閉じる"  onclick="closeModDownLoadConfirm();" />
                    </div>
                </div>
                <div id="divInputResultWrapper">
                    <asp:Repeater ID="repUpdateList" runat="server" ItemType="JOTWEB.OIT0003OTLinkageList.OutputOrdedrInfo" >
                        <HeaderTemplate>
                            <div class="updateList">
                                <table class="tblUpdList">
                                    <tr>
                                        <th rowspan="2" class="headerLine1 modChk">指示</th>
                                        <th colspan="2" class="headerLine1 file">予約情報</th>
                                        <th colspan="8" class="headerLine1 db">受注情報</th>
                                    </tr>
                                    <tr>
                                        <th class="headerLine2 file trnNo">車番</th>
                                        <th class="headerLine2 file oilName">油種名</th>
                                        <th class="headerLine2 db dbReservedNo">予約番号</th>
                                        <th class="headerLine2 db trnNo">車番</th>
                                        <th class="headerLine2 db oilName">油種名</th>
                                        <th class="headerLine2 db amount">数量</th>
                                        <th class="headerLine2 db trainNo">列車番号</th>
                                        <th class="headerLine2 db lodDate">積込日</th>
                                        <th class="headerLine2 db depDate">発日</th>
                                        <th class="headerLine2 db delDate">削除日</th>
                                    </tr>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr class='delFalg<%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).DeleteFlag %>'>
                                <td class="modChk">
                                    <asp:DropDownList ID="ddlModFlag" runat="server" SelectedValue='<%# DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).ModifiedFlag %>'>
                                        <asp:ListItem Text ="" Value=""></asp:ListItem>
                                        <asp:ListItem Text ="新規" Value="1"></asp:ListItem>
                                        <asp:ListItem Text ="変更" Value="2"></asp:ListItem>
                                        <asp:ListItem Text ="削除" Value="3"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:HiddenField ID="hdnModIndex" runat="server" Value='<%# DirectCast(Container, RepeaterItem).ItemIndex %>' />
                                </td>
                                <td class="trnNo"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OutTankNo %></td>
                                <td class="oilName"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OutOilType   %></td>
                                <td class="dbReservedNo"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).DispReservedNo  %></td>
                                <td class="trnNo"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).TankNo  %></td>
                                <td class="oilName"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OilName  %></td>
                                <td class="amount"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).CarsAmount %></td>
                                <td class="trainNo"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).TrainNo %></td>
                                <td class="lodDate"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).LodDate %></td>
                                <td class="depDate"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).DepDate %></td>
                                <td class="delDate"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).DelUpdDate %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </table>
                            </div>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
            <div id="divOTLinkageSendList" class='<%= Me.ShowOTLinkageSendChkConfirm  %>'>
                <asp:HiddenField ID="hdnOTLinkageSendChkConfirmIsActive" runat="server" Value="" />
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <span>表示されている内容で発送日報を送信しますよろしいですか？</span>
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonOTLinkageSend" class="btn-sticky" value="送信"   onclick="ButtonClick('WF_ButtonReserveOTLinkageSend');" />
                        <input type="button" id="btnCloseOTLinkageSendConfirm" class="btn-sticky" value="閉じる"  onclick="closeOTLinkageSendConfirm();" />
                    </div>
                </div>
                <div id="divInputOTResultWrapper">
                    <asp:Repeater ID="Repeater1" runat="server" ItemType="JOTWEB.OIT0003OTLinkageList.OutputOrdedrInfo" >
                        <HeaderTemplate>
                            <div class="updateList">
                                <table class="tblUpdList">
                                    <tr>
                                        <th colspan="2" class="headerLine1 officeName">OT営業所</th>
                                        <th colspan="2" class="headerLine1 sendYMD">発送年月日</th>
                                        <th colspan="2" class="headerLine1 trainNo">列車№</th>
                                        <th colspan="2" class="headerLine1 shipOrder">連結順位</th>
                                        <th colspan="2" class="headerLine1 depStation">発駅</th>
                                        <th colspan="2" class="headerLine1 shippersName">荷主</th>
                                        <th colspan="2" class="headerLine1 oilName">油種</th>
                                        <th colspan="2" class="headerLine1 tankNo">車号</th>
                                        <th colspan="2" class="headerLine1 amount">数量</th>
                                    </tr>
                                </table>
                            </div>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr class='delFalg<%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).DeleteFlag %>'>
                                <td class="officeName"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OTOfficeName %></td>
                                <td class="sendYMD"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OTSendYMD   %></td>
                                <td class="trainNo"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OTTrainNo  %></td>
                                <td class="shipOrder"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OTShipOrder  %></td>
                                <td class="depStation"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OTDepStationName  %></td>
                                <td class="shippersName"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OTShippersName %></td>
                                <td class="oilName"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OTOilName %></td>
                                <td class="tankNo"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OTTankNo %></td>
                                <td class="amount"><%#DirectCast(DirectCast(Container, RepeaterItem).DataItem, OutputOrdedrInfo).OTAmount %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
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
        </div>

</asp:Content>
<asp:Content ID="ctCostumPopUpTitle" ContentPlaceHolderID="contentsPopUpTitle" runat="server">
</asp:Content>
<asp:Content ID="ctCostumPopUp" ContentPlaceHolderID="contentsPopUpInside" runat="server">
</asp:Content>

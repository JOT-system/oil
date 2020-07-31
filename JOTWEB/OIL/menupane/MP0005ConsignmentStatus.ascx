<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="MP0005ConsignmentStatus.ascx.vb" Inherits="JOTWEB.MP0005ConsignmentStatus" %>
<asp:Panel ID="contentPane" CssClass="menuPaneItem paneWidth1" ClientIDMode="Predictable" runat="server">
    <div style="width:100%;height:100%;">
        <!-- ペインのタイトル設定 -->
        <div class="paneTitle">
            <div class="paneTitleLeft">
                <asp:Label ID="lblPaneTitle" runat="server" Text="" ClientIDMode="Predictable"></asp:Label>
            </div>
            <div class="paneTitleRight">
                <div class="paneTitleRefresh" onclick="refreshPane('<%= Me.hdnRefreshCall.ClientId %>');" title="最新化" ><div class="paneRefreshImg"></div></div>
                <!-- 上記ボタン内容更新のアイコンボタンを押下された時の呼出しに"1"を設定 -->
                <asp:HiddenField ID="hdnRefreshCall" runat="server" Value="" ClientIDMode="Predictable" />
            </div>
        </div> 
        <!-- ペインの内部コンテンツ -->
        <div class="paneContent">
            <!-- 営業所選択 -->
            <div class="importConsignmentStatusDdl" onchange="refreshPane('<%= Me.hdnRefreshCall.ClientId %>');">
                表示する営業所 
                <asp:DropDownList ID="ddlConsignmentOffice" runat="server" ClientIDMode="Predictable" CssClass="officeDdl"></asp:DropDownList>
            </div>
            <div class="paneConsignmentItems trainStatList">
                <asp:Repeater ID="repConsignmentItems" runat="server" ClientIDMode="Predictable">
                    <ItemTemplate>
                        <div class="importConsignmentItem trainItem" >
                            <div class="consignmentTrainNo trainNo">
                                <asp:Label ID="lblTrainNo" runat="server" Text='<%# Eval("TrainNo") %>' ClientIDMode="Predictable"></asp:Label>
                            </div>
                            <div class="consignmentStatus trainStatus">
                                <asp:Label ID="lblStatus" runat="server" Text='<%#  "<span class=""st" & Convert.ToString(Eval("Status")) & """></span>" %>' ClientIDMode="Predictable"></asp:Label>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>        
            </div>
        </div>
    </div>
    <asp:HiddenField ID="hdnPaneOrder" runat="server" Visible="false" />
</asp:Panel>
<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="MP0003OTLoadingSendStatus.ascx.vb" Inherits="JOTWEB.MP0003OTLoadingSendStatus" %>
<!-- OT発送日報送信状況ペイン カスタムコントロール ここより外側でcontentPaneを括らない事 -->
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
            <div class="loadingSendStatusDdl" onchange="refreshPane('<%= Me.hdnRefreshCall.ClientId %>');">
                表示する営業所 
                <asp:DropDownList ID="ddlLoadingSendStatusOffice" runat="server" ClientIDMode="Predictable" CssClass="officeDdl"></asp:DropDownList>
            </div>
            <div class="paneLoadingSendStatusItems trainStatList">
                <asp:Repeater ID="repLoadingSendStatusItems" runat="server" ClientIDMode="Predictable">
                    <ItemTemplate>
                        <div class="loadingSendStatusItem trainItem" >
                            <div class="loadingSendStatusTrainNo trainNo">
                                <asp:Label ID="lblTrainNo" runat="server" Text='<%# Eval("TrainNo") %>' ClientIDMode="Predictable"></asp:Label>
                            </div>
                            <div class="loadingSendStatusTrainStatus trainStatus">
                                <asp:Label ID="lblStatus" runat="server" Text='<%#  "<span class=""st" & Convert.ToString(Eval("Status")) & """></span>" %>' ClientIDMode="Predictable"></asp:Label>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>        
            </div>
            <asp:Panel ID="pnlSysError" CssClass="nodataArea" runat="server" ClientIDMode="Predictable" Visible="false">
                システムエラーが発生しOT発送日報送信状況を表示出来ませんでした。
            </asp:Panel>
        </div>

    </div>
    <asp:HiddenField ID="hdnPaneOrder" runat="server" Visible="false" ClientIDMode="Predictable" />
</asp:Panel>
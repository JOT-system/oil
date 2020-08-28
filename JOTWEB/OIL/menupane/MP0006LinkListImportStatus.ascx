<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="MP0006LinkListImportStatus.ascx.vb" Inherits="JOTWEB.MP0006LinkListImportStatus" %>
<asp:Panel ID="contentPane" CssClass="menuPaneItem paneWidth3" ClientIDMode="Predictable" runat="server">
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
            <div class="paneLinkListImportItems">
                <asp:Repeater ID="repLinkListImportItems" runat="server" ClientIDMode="Predictable">
                    <ItemTemplate>
                        <div class="linkListImportItem" >
                            <div class="linkListTrainNo">
                                <asp:Label ID="lblTrainNo" runat="server" Text='<%# Eval("TrainNo") %>' ClientIDMode="Predictable"></asp:Label>
                            </div>
                            <div class="linkListImported">
                                <asp:Label ID="lblStatus" runat="server" Text='<%# If(DirectCast(Eval("Imported"), Boolean) = True, "<span class=""imported""></span>", "<span class=""notProceed""></span>") %>' ClientIDMode="Predictable"></asp:Label>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>        
            </div>
            <asp:Panel ID="pnlSysError" CssClass="nodataArea" runat="server" ClientIDMode="Predictable" Visible="false">
                システムエラーが発生し貨車連結順序表取込状況を表示出来ませんでした。
            </asp:Panel>
        </div>

    </div>
    <asp:HiddenField ID="hdnPaneOrder" runat="server" Visible="false" ClientIDMode="Predictable" />
</asp:Panel>

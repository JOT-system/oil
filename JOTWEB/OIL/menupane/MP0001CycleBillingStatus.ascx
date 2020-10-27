<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="MP0001CycleBillingStatus.ascx.vb" Inherits="JOTWEB.MP0001CycleBillingStatus" %>
<!-- 月締め状況ペイン カスタムコントロール ここより外側でcontentPaneを括らない事 -->
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
            <asp:HiddenField ID="hdnTargetMonth" runat="server" ClientIDMode="Predictable" Visible="false" />
            <div class="cycleBillingStatusWrapper">
                <div class="cycleBillingStatusDeptBranch">
                    <asp:Repeater ID="repBranch" runat="server" ClientIDMode="Predictable">
                        <ItemTemplate>
                            <div>
                                <asp:Repeater ID="repDept" runat="server" ClientIDMode="Predictable" DataSource='<%# DirectCast(Eval("Value"), ClosingItem).ChildItem %>' >
                                    <HeaderTemplate>
                                        <div class="deptList">
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="deptItem">
                                             <asp:Label ID="lbldept" runat="server" Text='<%# Eval("Name") %>' data-isclosed='<%# Eval("IsClosed") %>' ClientIDMode="Predictable"></asp:Label>
                                            <div class="underArrow">
                                            </div>
                                        </div>
                                  
                                    </ItemTemplate>
                                    <FooterTemplate >
                                        </div>
                                    </FooterTemplate>
                                </asp:Repeater>
                                <div class="branchItemItem">
                                    <asp:Label ID="lblBranch" runat="server" Text='<%# DirectCast(Eval("Value"), ClosingItem).Name %>' data-isclosed='<%# DirectCast(Eval("Value"), ClosingItem).IsClosed %>' ClientIDMode="Predictable"></asp:Label>
                                    <div class="underArrow">
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <div class="cycleBillingStatusBottom">
                    <div class="bottomItem">
                        <asp:Label ID="lblBottomItem" runat="server" Text="" ClientIDMode="Predictable"></asp:Label>
                    </div>
                </div>
            </div>
            <asp:Panel ID="pnlSysError" CssClass="nodataArea" runat="server" ClientIDMode="Predictable" Visible="false">
                システムエラーが発生し月締状況を表示出来ませんでした。
            </asp:Panel>
        </div>
    </div>
    <asp:HiddenField ID="hdnPaneOrder" runat="server" Visible="false" />
</asp:Panel>

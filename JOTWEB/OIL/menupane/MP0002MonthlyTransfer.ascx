<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="MP0002MonthlyTransfer.ascx.vb" Inherits="JOTWEB.MP0002MonthlyTransfer" %>
<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>
<!-- 月間輸送数量ペイン カスタムコントロール ここより外側でcontentPaneを括らない事 -->
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
            <!-- 表エリア -->
            <div class="monthTransLeft">
                <!-- 営業所選択 -->
                <div class="monthTransDdl" onchange="refreshPane('<%= Me.hdnRefreshCall.ClientId %>');">
                    表示する営業所 
                    <asp:DropDownList ID="ddlMonthTransOffice" runat="server" ClientIDMode="Predictable"></asp:DropDownList>
                </div>
                <!-- 一覧表 -->
                <div class="monthTransTable">
                    <asp:Repeater ID="repMonthTrans" runat="server" ClientIDMode="Predictable">
                        <HeaderTemplate>
                            <table class="tblMonthTrans">
                                <tr>
                                    <th class="oilType">&nbsp;</th>
                                    <th class="yesterday">前日(累計)</th>
                                    <th class="today">当日(累計)</th>
                                    <th class="todayTrans">当日輸送分</th>
                                </tr>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <th class="oilNameData">
                                    <asp:Label ID="lblOilType" runat="server" Text='<%# Eval("OILNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                </th>
                                <td>
                                    <asp:Label ID="lblYesterday" runat="server" Text='<%# CDec(Eval("YESTERDAYVAL")).ToString("#,##0.00(kl)") %>' ClientIDMode="Predictable"></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lblToday" runat="server" Text='<%# CDec(Eval("TODAYVAL")).ToString("#,##0.00(kl)") %>' ClientIDMode="Predictable"></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lblTodayTrans" runat="server" Text='<%# CDec(Eval("TODAYTRANS")).ToString("#,##0.00(kl)") %>' ClientIDMode="Predictable"></asp:Label>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
            <!-- グラフエリア -->
            <div class="monthTransRight">
                <!-- グラフコントロール -->
                <asp:Chart ID="chtMonthTrans" runat="server" 
                    Width="620"
                    BackColor="Transparent">
                    <Series>
                        <%-- 当日分のデータ設定 --%>
                        <asp:Series Name="serToday" 
                            ChartArea="carMonthTrans" 
                            ChartType="Bar" 
                            Color="#2F5197" 
                            XValueMember="OILNAME" 
                            YValueMembers="TODAYVAL"
                            LegendText="当日"
                            Legend="legHan"
                            >
                        </asp:Series>
                        <%--前日分のデータ設定 --%>
                        <asp:Series Name="serYesterday" 
                            ChartArea="carMonthTrans" 
                            ChartType="Bar" 
                            Color="#A6A6A6"
                            XValueMember="OILNAME" 
                            YValueMembers="YESTERDAYVAL"
                            LegendText="前日"
                            Legend="legHan"
                            >
                        </asp:Series>

                    </Series>
                    <ChartAreas>
                        <asp:ChartArea Name="carMonthTrans" 
                             >
                            
                            <AxisX LabelAutoFitMaxFontSize="12"
                                   LineColor="Gray">
                                <%-- 油種名フォント --%>
                                <LabelStyle Font="ms pgothic, 6pt, style=Bold"  />
                                <%-- 横軸文言とつなぐメモリ線（表示しない） --%>
                                <MajorTickMark Enabled="false" />
                                <%-- 横軸のグリッド線 --%>
                                <MajorGrid Enabled="false" />
                            </AxisX>
                            <AxisY LineColor="Gray">
                                <%-- 縦軸メモリ線を消す --%>
                                <MajorTickMark Enabled="false" />
                                <%-- 数値フォント --%>
                                <LabelStyle Font="ms pgothic, 6pt, style=Regular" Format="#,##0"  />
                                <%-- 縦軸のグリッド線 --%>
                                <MajorGrid LineColor="Gray"   />
                            </AxisY>
                        </asp:ChartArea>
                    </ChartAreas>
                    <Legends>
                        <asp:Legend Name="legHan" 
                            LegendStyle="row" 
                            Docking="Top"  
                            Alignment="Far" 
                            BackColor="Transparent" ></asp:Legend>
	                </Legends>
                </asp:Chart>
            </div>
        </div>
    </div>
    <asp:HiddenField ID="hdnPaneOrder" runat="server" Visible="false" ClientIDMode="Predictable" />
</asp:Panel>

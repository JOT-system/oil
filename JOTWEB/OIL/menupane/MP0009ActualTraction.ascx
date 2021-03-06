﻿<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="MP0009ActualTraction.ascx.vb" Inherits="JOTWEB.MP0009ActualTraction" %>
<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>
<asp:Panel ID="contentPane" CssClass="menuPaneItem paneWidth3" ClientIDMode="Predictable" runat="server">
    <div style="width:100%;height:100%;">
        <!-- ペインのタイトル設定 -->
        <div class="paneTitle">
            <div class="paneTitleLeft">
                <asp:Label ID="lblPaneTitle" runat="server" Text="" ClientIDMode="Predictable"></asp:Label>
            </div>
            <div class="paneTitleMiddle">
                <asp:Label ID="lblPaneDownloadTitle" runat="server" Text="" ClientIDMode="Predictable"></asp:Label>
            </div>
            <div class="paneTitleRight">
                <div class="paneTitleRefresh" onclick="refreshPane('<%= Me.hdnRefreshCall.ClientId %>');" title="最新化" ><div class="paneRefreshImg"></div></div>
                <!-- 上記ボタン内容更新のアイコンボタンを押下された時の呼出しに"1"を設定 -->
                <asp:HiddenField ID="hdnRefreshCall" runat="server" Value="" ClientIDMode="Predictable" />
            </div>
        </div> 
        <!-- ペインの内部コンテンツ -->
        <div class="paneContent">
            <div class="flexCondition">
                <!-- 営業所選択 -->
                <div class="actualTractionDdl" onchange="refreshPane('<%= Me.hdnRefreshCall.ClientId %>');">
                    <span>
                        表示する営業所 
                        <asp:DropDownList ID="ddlActualTractionOffice" runat="server" ClientIDMode="Predictable" CssClass="officeDdl"></asp:DropDownList>
                    </span>
                    <span>
                        着駅
                        <asp:DropDownList ID="ddlActualTractionArrStation" runat="server" ClientIDMode="Predictable" CssClass="stationDdl"></asp:DropDownList>
                    </span>
                </div>
                <!-- ダウンロード -->
                <div class="actualTravtionDownloadDbl" >
                    <span>
                        年月
                        <asp:DropDownList ID="ddlActualTractionYearMonth" runat="server" ClientIDMode="Predictable" CssClass="yearMonthDdl"></asp:DropDownList>
                    </span>
                    <span>
                        基準日
                        <asp:DropDownList ID="ddlActualTractionDayStandard" runat="server" ClientIDMode="Predictable" CssClass="dayStandardDdl"></asp:DropDownList>
                    </span>
                    <span>
                        <input id="btnDownload" type="button" runat="server" ClientIDMode="Predictable" class="btn-sticky btnDownload" value="ダウンロード" onclick='downloadPaneData(this.id);'  />
                        <asp:HiddenField ID="hdnDownloadCall" runat="server" ClientIDMode="Predictable" EnableViewState="false"  />
                    </span>
                </div>
            </div>
            <!-- グラフコントロール -->
            <asp:Chart ID="chtActualTraction" runat="server" EnableViewState="true"
                Width="1070"
                BackColor="Transparent">
                <Series>

                </Series>
                <ChartAreas>
                    <asp:ChartArea Name="carActualTraction" 
                            >
                            
                        <AxisX LabelAutoFitMaxFontSize="12"
                                LineColor="Gray">
                            <%-- 油種名フォント --%>
                            <LabelStyle Font="ms pgothic, 6pt, style=Bold" />
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
                        Font="ms pgothic, 8pt, style=Regular"
                        Docking="Top"  
                        Alignment="Far" 
                        BackColor="Transparent" ></asp:Legend>
	            </Legends>
            </asp:Chart>
            <asp:Panel ID="pnlSysError" CssClass="nodataArea" runat="server" ClientIDMode="Predictable" Visible="false">
                システムエラーが発生し営業所別　列車牽引実績を表示出来ませんでした。
            </asp:Panel>
        </div>

    </div>
    <asp:HiddenField ID="hdnPaneOrder" runat="server" Visible="false" ClientIDMode="Predictable" />
    <asp:HiddenField ID="hdnCurrentOfficeCode" runat="server" Visible="false" ClientIDMode="Predictable" /> 
    <%--<asp:HiddenField ID="hdnYearMonth" runat="server" Visible="false" ClientIDMode="Predictable" />--%> 
</asp:Panel>

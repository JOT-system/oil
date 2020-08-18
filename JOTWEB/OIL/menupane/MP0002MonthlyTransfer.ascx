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
            <div class="divDdlArea" onchange="refreshPane('<%= Me.hdnRefreshCall.ClientId %>');">
                表示種別
                <asp:DropDownList ID="ddlListPattern" runat="server" ClientIDMode="Predictable" CssClass="officeDdl"></asp:DropDownList>
                <div style="display:inline-block" runat="server" id="divMonthlyTransOffice" visible="false" ClientIDMode="Predictable">
                    営業所
                    <asp:DropDownList ID="ddlMonthTransOffice" runat="server" ClientIDMode="Predictable" CssClass="officeDdl"></asp:DropDownList>
                </div>
                <div style="display:inline-block">
                    <input id="btnDownload" type="button" runat="server" ClientIDMode="Predictable" class="btn-sticky btnDownload" value="ダウンロード" onclick='downloadPaneData(this.id);' style="display:none;" />
                    <asp:HiddenField ID="hdnDownloadCall" runat="server" ClientIDMode="Predictable" EnableViewState="false"  />
                </div>
                <div class="measureText">
                    単位(kl)
                </div>
            </div>
            <!-- 表示種別で切り替えるビュー「asp:View」のIDはFIXVALUEのCLASS='MENUMONTHTRPAT'のKEYCODEと連動 -->
            <asp:MultiView ID="mvwMonthlyTransfer" runat="server" ClientIDMode="Predictable">
                <!-- 営業所別VIEW -->
                <asp:View ID="VIEW001" runat="server" ClientIDMode="Predictable">
                    <!-- 表エリア -->
                    <div class="monthTransLeft">
                        <!-- 一覧表 -->
                        <div class="monthTransTable">
                            <asp:Repeater ID="repMonthTrans" runat="server" ClientIDMode="Predictable">
                                <HeaderTemplate>
                                    <table class="tblMonthTrans">
                                        <tr>
                                            <th class="oilType erase">&nbsp;</th>
                                            <th class="yesterday">前日(累計)</th>
                                            <th class="today">当日(累計)</th>
                                            <th class="todayTrans">当日輸送分</th>
                                            <th class="volumeChange">対予算増減</th>
                                            <th class="volumeRatio">対予算比率</th>
                                            <th class="lyVolumeChange">対前年増減</th>
                                            <th class="lyVolumeRatio">対前年比率</th>
                                        </tr>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <th class="oilNameData">
                                            <asp:Label ID="lblOilType" runat="server" Text='<%# Eval("OILNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                        </th>
                                        <td>
                                            <asp:Label ID="lblYesterday" runat="server" Text='<%# CDec(Eval("MAERUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:Label ID="lblToday" runat="server" Text='<%# CDec(Eval("RUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:Label ID="lblTodayTrans" runat="server" Text='<%# CDec(Eval("VOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:Label ID="lblVolumeChange" runat="server" Text='<%# CDec(Eval("VOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:Label ID="lblVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("VOLUMERATIO")) < 0, "minus", If(CDec(Eval("VOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("VOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:Label ID="lblLyVolumeChange" runat="server" Text='<%# CDec(Eval("LYVOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:Label ID="lblLyVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("LYVOLUMERATIO")) < 0, "minus", If(CDec(Eval("LYVOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("LYVOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
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
                        <asp:Chart ID="chtMonthTrans" runat="server" EnableViewState="true"
                            Width="620"
                            BackColor="Transparent">
                            <Series>
                                <%-- 当日分のデータ設定 --%>
                                <asp:Series Name="serToday" 
                                    ChartArea="carMonthTrans" 
                                    ChartType="Bar" 
                                    Color="#2F5197" 
                                    XValueMember="OILNAME" 
                                    YValueMembers="RUIKEIVOLUME"
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
                                    YValueMembers="MAERUIKEIVOLUME"
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
                </asp:View>
                <!-- 支店別VIEW -->
                <asp:View ID="VIEW002" runat="server" ClientIDMode="Predictable">
                    <div class="monthTransTable view002">
                        <asp:Repeater ID="repMonthTrans002" runat="server" ClientIDMode="Predictable">
                            <ItemTemplate>
                                <asp:Repeater ID="repMonthTransSub002" runat="server" DataSource='<%# Container.DataItem %>' ClientIDMode="Predictable">
                                    <HeaderTemplate>
                                        <table>
                                            <tr>
                                                <th class="bigOilCode">白黒区分</th>
                                                <th class="trainClass">輸送区分</th>
                                                <th class="orgCode">支店</th>
                                                <th class="yesterday">前日(累計)</th>
                                                <th class="today">当日(累計)</th>
                                                <th class="todayTrans">当日輸送分</th>
                                                <th class="volumeChange">対予算増減</th>
                                                <th class="volumeRatio">対予算比率</th>
                                                <th class="lyVolumeChange">対前年増減</th>
                                                <th class="lyVolumeRatio">対前年比率</th>
                                            </tr>
                                    </HeaderTemplate>                                    
                                    <ItemTemplate>
                                        <tr>
                                            <td class="bigOilCode center" id="tdBigOilCode" runat="server" ClientIDMode="Predictable" rowspan='<%#If(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", Eval("ROWSPANFIELD1"), "0") %>' visible='<%# if(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", "True", "False") %>' >
                                                <asp:Label ID="lblBigOilName" runat="server" Text='<%# Eval("BIGOILNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class="trainClass center" id="tdTrainClass" runat="server" ClientIDMode="Predictable" rowspan='<%#If(Convert.ToString(Eval("ROWSPANFIELD2")) <> "", Eval("ROWSPANFIELD2"), "0") %>' visible='<%# if(Convert.ToString(Eval("ROWSPANFIELD2")) <> "", "True", "False") %>' >
                                                <asp:Label ID="lblTrainClassName" runat="server" Text='<%# Eval("TRAINCLASSNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='orgCode center <%# If(Convert.ToString(Eval("ORGNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblOrgName" runat="server" Text='<%# Eval("ORGNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("ORGNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblYesterday" runat="server" Text='<%# CDec(Eval("MAERUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("ORGNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblToday" runat="server" Text='<%# CDec(Eval("RUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("ORGNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblTodayTrans" runat="server" Text='<%# CDec(Eval("VOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("ORGNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblVolumeChange" runat="server" Text='<%# CDec(Eval("VOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("ORGNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("VOLUMERATIO")) < 0, "minus", If(CDec(Eval("VOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("VOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("ORGNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblLyVolumeChange" runat="server" Text='<%# CDec(Eval("LYVOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("ORGNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblLyVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("LYVOLUMERATIO")) < 0, "minus", If(CDec(Eval("LYVOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("LYVOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:View>
                <!-- 荷主別　請負輸送OT輸送合算VIEW -->
                <asp:View ID="VIEW003" runat="server" ClientIDMode="Predictable">
                    <div class="monthTransTable view003">
                        <asp:Repeater ID="repMonthTrans003" runat="server" ClientIDMode="Predictable">
                            <ItemTemplate>
                                <asp:Repeater ID="repMonthTransSub003" runat="server" DataSource='<%# Container.DataItem %>' ClientIDMode="Predictable">
                                    <HeaderTemplate>
                                        <table>
                                            <tr>
                                                <th class="bigOilCode">白黒区分</th>
                                                <th class="trainClass">輸送区分</th>
                                                <th class="shpCode">荷主</th>
                                                <th class="yesterday">前日(累計)</th>
                                                <th class="today">当日(累計)</th>
                                                <th class="todayTrans">当日輸送分</th>
                                                <th class="volumeChange">対予算増減</th>
                                                <th class="volumeRatio">対予算比率</th>
                                                <th class="lyVolumeChange">対前年増減</th>
                                                <th class="lyVolumeRatio">対前年比率</th>
                                            </tr>
                                    </HeaderTemplate>                                    
                                    <ItemTemplate>
                                        <tr>
                                            <td class="bigOilCode center" id="tdBigOilCode" runat="server" ClientIDMode="Predictable" rowspan='<%#If(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", Eval("ROWSPANFIELD1"), "0") %>' visible='<%# if(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", "True", "False") %>' >
                                                <asp:Label ID="lblBigOilName" runat="server" Text='<%# Eval("BIGOILNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class="trainClass center" id="tdTrainClass" runat="server" ClientIDMode="Predictable" rowspan='<%#If(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", Eval("ROWSPANFIELD1"), "0") %>' visible='<%# if(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", "True", "False") %>' >
                                                <asp:Label ID="lblTrainClassName" runat="server" Text='<%# Eval("TRAINCLASSNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='shpCode center <%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblOrgName" runat="server" Text='<%# Eval("SHIPPERNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblYesterday" runat="server" Text='<%# CDec(Eval("MAERUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblToday" runat="server" Text='<%# CDec(Eval("RUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblTodayTrans" runat="server" Text='<%# CDec(Eval("VOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblVolumeChange" runat="server" Text='<%# CDec(Eval("VOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("VOLUMERATIO")) < 0, "minus", If(CDec(Eval("VOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("VOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblLyVolumeChange" runat="server" Text='<%# CDec(Eval("LYVOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblLyVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("LYVOLUMERATIO")) < 0, "minus", If(CDec(Eval("LYVOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("LYVOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:View>
                <!-- 荷受人別VIEW -->
                <asp:View ID="VIEW004" runat="server" ClientIDMode="Predictable">
                    <div class="monthTransTable view004">
                        <asp:Repeater ID="repMonthTrans004" runat="server" ClientIDMode="Predictable">
                            <ItemTemplate>
                                <asp:Repeater ID="repMonthTransSub004" runat="server" DataSource='<%# Container.DataItem %>' ClientIDMode="Predictable">
                                    <HeaderTemplate>
                                        <table>
                                            <tr>
                                                <th class="bigOilCode">白黒区分</th>
                                                <th class="trainClass">輸送区分</th>
                                                <th class="cnsCode">着基地</th>
                                                <th class="yesterday">前日(累計)</th>
                                                <th class="today">当日(累計)</th>
                                                <th class="todayTrans">当日輸送分</th>
                                                <th class="volumeChange">対予算増減</th>
                                                <th class="volumeRatio">対予算比率</th>
                                                <th class="lyVolumeChange">対前年増減</th>
                                                <th class="lyVolumeRatio">対前年比率</th>
                                            </tr>
                                    </HeaderTemplate>                                    
                                    <ItemTemplate>
                                        <tr>
                                            <td class="bigOilCode center" id="tdBigOilCode" runat="server" ClientIDMode="Predictable" rowspan='<%#If(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", Eval("ROWSPANFIELD1"), "0") %>' visible='<%# if(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", "True", "False") %>' >
                                                <asp:Label ID="lblBigOilName" runat="server" Text='<%# Eval("BIGOILNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class="trainClass center" id="tdTrainClass" runat="server" ClientIDMode="Predictable" rowspan='<%#If(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", Eval("ROWSPANFIELD1"), "0") %>' visible='<%# if(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", "True", "False") %>' >
                                                <asp:Label ID="lblTrainClassName" runat="server" Text='<%# Eval("TRAINCLASSNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='cnsCode center <%# If(Convert.ToString(Eval("CONSIGNEENAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblOrgName" runat="server" Text='<%# Eval("CONSIGNEENAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("CONSIGNEENAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblYesterday" runat="server" Text='<%# CDec(Eval("MAERUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("CONSIGNEENAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblToday" runat="server" Text='<%# CDec(Eval("RUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("CONSIGNEENAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblTodayTrans" runat="server" Text='<%# CDec(Eval("VOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("CONSIGNEENAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblVolumeChange" runat="server" Text='<%# CDec(Eval("VOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("CONSIGNEENAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("VOLUMERATIO")) < 0, "minus", If(CDec(Eval("VOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("VOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("CONSIGNEENAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblLyVolumeChange" runat="server" Text='<%# CDec(Eval("LYVOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("CONSIGNEENAME")) = "計", "summary", "") %>'>
                                                <asp:Label ID="lblLyVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("LYVOLUMERATIO")) < 0, "minus", If(CDec(Eval("LYVOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("LYVOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:View>
                <!-- 油種別（中分類）別VIEW -->
                <asp:View ID="VIEW005" runat="server" ClientIDMode="Predictable">
                    <div class="monthTransTable view005">
                        <asp:Repeater ID="repMonthTrans005" runat="server" ClientIDMode="Predictable">
                            <HeaderTemplate>
                                <table>
                                    <tr>
                                        <th class="trainClass">輸送区分</th>
                                        <th class="oilCode">油種</th>
                                        <th class="yesterday">前日(累計)</th>
                                        <th class="today">当日(累計)</th>
                                        <th class="todayTrans">当日輸送分</th>
                                        <th class="volumeChange">対予算増減</th>
                                        <th class="volumeRatio">対予算比率</th>
                                        <th class="lyVolumeChange">対前年増減</th>
                                        <th class="lyVolumeRatio">対前年比率</th>
                                    </tr>
                            </HeaderTemplate>                                    
                            <ItemTemplate>
                                <tr>
                                    <td class="trainClass center" id="tdTrainClass" runat="server" ClientIDMode="Predictable" rowspan='<%#If(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", Eval("ROWSPANFIELD1"), "0") %>' visible='<%# if(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", "True", "False") %>' >
                                        <asp:Label ID="lblTrainClass" runat="server" Text='<%# Eval("TRAINCLASSNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                    </td>
                                    <td class='oilCode center <%# If(Convert.ToString(Eval("OILNAME")) = "合計", "summary", If(Convert.ToString(Eval("OILNAME")).EndsWith("計"), "midSummary", "")) %>'>
                                        <asp:Label ID="lblOrgName" runat="server" Text='<%# Eval("OILNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                    </td>
                                    <td class='<%# If(Convert.ToString(Eval("OILNAME")) = "合計", "summary", If(Convert.ToString(Eval("OILNAME")).EndsWith("計"), "midSummary", "")) %>'>
                                        <asp:Label ID="lblYesterday" runat="server" Text='<%# CDec(Eval("MAERUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                    </td>
                                    <td class='<%# If(Convert.ToString(Eval("OILNAME")) = "合計", "summary", If(Convert.ToString(Eval("OILNAME")).EndsWith("計"), "midSummary", "")) %>'>
                                        <asp:Label ID="lblToday" runat="server" Text='<%# CDec(Eval("RUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                    </td>
                                    <td class='<%# If(Convert.ToString(Eval("OILNAME")) = "合計", "summary", If(Convert.ToString(Eval("OILNAME")).EndsWith("計"), "midSummary", "")) %>'>
                                        <asp:Label ID="lblTodayTrans" runat="server" Text='<%# CDec(Eval("VOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                    </td>
                                    <td class='<%# If(Convert.ToString(Eval("OILNAME")) = "合計", "summary", If(Convert.ToString(Eval("OILNAME")).EndsWith("計"), "midSummary", "")) %>'>
                                        <asp:Label ID="lblVolumeChange" runat="server" Text='<%# CDec(Eval("VOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                    </td>
                                    <td class='<%# If(Convert.ToString(Eval("OILNAME")) = "合計", "summary", If(Convert.ToString(Eval("OILNAME")).EndsWith("計"), "midSummary", "")) %>'>
                                        <asp:Label ID="lblVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("VOLUMERATIO")) < 0, "minus", If(CDec(Eval("VOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("VOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                    </td>
                                    <td class='<%# If(Convert.ToString(Eval("OILNAME")) = "合計", "summary", If(Convert.ToString(Eval("OILNAME")).EndsWith("計"), "midSummary", "")) %>'>
                                        <asp:Label ID="lblLyVolumeChange" runat="server" Text='<%# CDec(Eval("LYVOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                    </td>
                                    <td class='<%# If(Convert.ToString(Eval("OILNAME")) = "合計", "summary", If(Convert.ToString(Eval("OILNAME")).EndsWith("計"), "midSummary", "")) %>'>
                                        <asp:Label ID="lblLyVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("LYVOLUMERATIO")) < 0, "minus", If(CDec(Eval("LYVOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("LYVOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </asp:View>
                <!-- 荷主別VIEW -->
                <asp:View ID="VIEW006" runat="server" ClientIDMode="Predictable">
                    <div class="monthTransTable view006">
                        <asp:Repeater ID="repMonthTrans006" runat="server" ClientIDMode="Predictable">
                            <ItemTemplate>
                                <asp:Repeater ID="repMonthTransSub006" runat="server" DataSource='<%# Container.DataItem %>' ClientIDMode="Predictable">
                                    <HeaderTemplate>
                                        <table>
                                            <tr>
                                                <th class="bigOilCode">白黒区分</th>
                                                <th class="trainClass">輸送区分</th>
                                                <th class="shpCode">荷主</th>
                                                <th class="yesterday">前日(累計)</th>
                                                <th class="today">当日(累計)</th>
                                                <th class="todayTrans">当日輸送分</th>
                                                <th class="volumeChange">対予算増減</th>
                                                <th class="volumeRatio">対予算比率</th>
                                                <th class="lyVolumeChange">対前年増減</th>
                                                <th class="lyVolumeRatio">対前年比率</th>
                                            </tr>
                                    </HeaderTemplate>                                    
                                    <ItemTemplate>
                                        <tr>
                                            <td class="bigOilCode center" id="tdBigOilCode" runat="server" ClientIDMode="Predictable" rowspan='<%#If(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", Eval("ROWSPANFIELD1"), "0") %>' visible='<%# if(Convert.ToString(Eval("ROWSPANFIELD1")) <> "", "True", "False") %>' >
                                                <asp:Label ID="lblBigOilName" runat="server" Text='<%# Eval("BIGOILNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td Class="trainClass center" data-ttlsum='<%# If(Convert.ToString(Eval("TRAINCLASSNAME")) = "総計", "1", "") %>'  id="tdTrainClass" runat="server" ClientIDMode="Predictable" rowspan='<%#If(Convert.ToString(Eval("ROWSPANFIELD2")) <> "", Eval("ROWSPANFIELD2"), "0") %>' visible='<%# if(Convert.ToString(Eval("ROWSPANFIELD2")) <> "", "True", "False") %>' >
                                                <asp:Label ID="lblTrainClassName" runat="server" Text='<%# Eval("TRAINCLASSNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='shpCode center <%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>' data-ttlsum='<%# If(Convert.ToString(Eval("TRAINCLASSNAME")) = "総計", "1", "") %>'>
                                                <asp:Label ID="lblOrgName" runat="server" Text='<%# Eval("SHIPPERNAME") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>' data-ttlsum='<%# If(Convert.ToString(Eval("TRAINCLASSNAME")) = "総計", "1", "") %>'>
                                                <asp:Label ID="lblYesterday" runat="server" Text='<%# CDec(Eval("MAERUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>' data-ttlsum='<%# If(Convert.ToString(Eval("TRAINCLASSNAME")) = "総計", "1", "") %>'>
                                                <asp:Label ID="lblToday" runat="server" Text='<%# CDec(Eval("RUIKEIVOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>' data-ttlsum='<%# If(Convert.ToString(Eval("TRAINCLASSNAME")) = "総計", "1", "") %>'>
                                                <asp:Label ID="lblTodayTrans" runat="server" Text='<%# CDec(Eval("VOLUME")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>' data-ttlsum='<%# If(Convert.ToString(Eval("TRAINCLASSNAME")) = "総計", "1", "") %>'>
                                                <asp:Label ID="lblVolumeChange" runat="server" Text='<%# CDec(Eval("VOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>' data-ttlsum='<%# If(Convert.ToString(Eval("TRAINCLASSNAME")) = "総計", "1", "") %>'>
                                                <asp:Label ID="lblVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("VOLUMERATIO")) < 0, "minus", If(CDec(Eval("VOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("VOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>' data-ttlsum='<%# If(Convert.ToString(Eval("TRAINCLASSNAME")) = "総計", "1", "") %>'>
                                                <asp:Label ID="lblLyVolumeChange" runat="server" Text='<%# CDec(Eval("LYVOLUMECHANGE")).ToString("#,##0.000") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                            <td class='<%# If(Convert.ToString(Eval("SHIPPERNAME")) = "計", "summary", "") %>' data-ttlsum='<%# If(Convert.ToString(Eval("TRAINCLASSNAME")) = "総計", "1", "") %>'>
                                                <asp:Label ID="lblLyVolumeRatio" runat="server" CssClass='<%# If(CDec(Eval("LYVOLUMERATIO")) < 0, "minus", If(CDec(Eval("LYVOLUMERATIO")) = 0, "zero", "plus")) %>' Text='<%# CDec(Eval("LYVOLUMERATIO")).ToString("0.0%;0.0%") %>' ClientIDMode="Predictable"></asp:Label>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:View>
                <!-- ありえないが未定義の切り口が来た場合VIEW -->
                <asp:View ID="UNDEFINE" runat="server" ClientIDMode="Predictable">
                    <div style="color:red;margin:5px;font-size:20px;">選択した表示種別は実装されていません。</div>
                </asp:View>
            </asp:MultiView>
            <!-- 各切り口においてデータが無い場合はこちら↓を表示 -->
            <asp:Panel ID="pnlNoData" CssClass="nodataArea" runat="server" ClientIDMode="Predictable" Visible="false">
                集計対象無し
            </asp:Panel>
        </div>
    </div>
    <asp:HiddenField ID="hdnPaneOrder" runat="server" Visible="false" ClientIDMode="Predictable" />
    <asp:HiddenField ID="hdnCurrentListPattern" runat="server" Visible="false" ClientIDMode="Predictable"  />
</asp:Panel>

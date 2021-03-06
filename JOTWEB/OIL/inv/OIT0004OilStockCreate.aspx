﻿<%@ Page Title="OIT0004C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0004OilStockCreate.aspx.vb" Inherits="JOTWEB.OIT0004OilStockCreate" %>
<%@ MasterType VirtualPath="~/OIL/OILMasterPage.Master" %>

<%@ Import Namespace="JOTWEB.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/OIL/inc/OIT0004WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="OIT0004CH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/OIL/css/OIT0004C.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/OIL/script/OIT0004C.js")%>'></script>
    <script type="text/javascript">
        
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        //共通ポップアップボタン名
        var customPopUpOkButtonName = 'ﾀﾞｳﾝﾛｰﾄﾞ';
    </script>
</asp:Content>
 
<asp:Content ID="OIT0004L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="actionButtonBox">
                <div class="leftSide">
                    <span id="spnInventoryDays" runat="server">
                        <a >在庫維持日数</a>
                        <a class="ef">
                            <asp:TextBox ID="WF_INVENTORYDAYS" runat="server" onblur="MsgClear();" ></asp:TextBox>
                        </a>
                    </span>
                    <!-- ボタン -->
                    <input type="button" id="WF_ButtonAUTOSUGGESTION" runat="server" class="btn-sticky" value="自動提案"     onclick="ButtonClick('WF_ButtonAUTOSUGGESTION');" />
                    <input type="button" id="WF_ButtonORDERLIST"      runat="server" class="btn-sticky" value="受注作成"     onclick="ButtonClick('WF_ButtonORDERLIST');" />
                    <input type="button" id="WF_ButtonINPUTCLEAR"     runat="server" class="btn-sticky" value="入力値クリア" onclick="ButtonClick('WF_ButtonINPUTCLEAR');" />
                    <input type="button" id="WF_ButtonGETEMPTURN"     runat="server" class="btn-sticky" value="空回日報取込" onclick="ButtonClick('WF_ButtonGETEMPTURN');" />
                    <input type="button" id="WF_ButtonFixedStock"     runat="server" class="btn-sticky" value="オーダー確定"     onclick="setFixPopUp();commonShowCustomPopup();" />
                </div>

                <div class="rightSide">
                    <input type="button" id="WF_ButtonRECULC"        class="btn-sticky" value="在庫表再計算"     onclick="ButtonClick('WF_ButtonRECULC');" />
                    <input type="button" id="WF_ButtonUPDATE"        class="btn-sticky" value="在庫表保存"     onclick="ButtonClick('WF_ButtonUPDATE');" />

                    <input type="button" id="WF_ButtonReport"           class="btn-sticky" value="帳票" onclick="setPrintPopUp();commonShowCustomPopup();" />
                    <input type="button" id="WF_ButtonEND"           class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                    <div                 id="WF_ButtonFIRST"         class="firstPage"  runat="server"   visible="false" onclick="ButtonClick('WF_ButtonFIRST');"></div>
                    <div                 id="WF_ButtonLAST"          class="lastPage"   runat="server"   visible="false" onclick="ButtonClick('WF_ButtonLAST');"></div>
                </div>
            </div> <!-- End class=actionButtonBox -->
            <%-- マスタページ上でClientIDMode=Staticを設定し継承されるためリピーターには個別で設定
                 設定しないとHTMLのルールであるIDがユニークではなく重複する --%>
            <!-- 受注提案タンク車数 (vbコード内で pnlSuggestList.Visible=Falseで消せる -->
            <asp:Panel ID="pnlSuggestList" runat="server"  >
            <div class="listTitle">受注提案タンク車数</div>
            <asp:FormView ID="frvSuggest" runat="server" RenderOuterTable="false" ClientIDMode="Predictable">
                <HeaderTemplate>
                    <div id="divSuggestList" style='height:calc(<%# Eval("OilTypeCount") + 3 + 1  %> * 24px)' data-hasmiitem='<%# Eval("HasMoveInsideItem") %>'>
                </HeaderTemplate>
                <ItemTemplate>
                    <%--  一列目 --%>
                    <div class="leftColumn">
                        <div>
                            <span>内訳</span>
                        </div>
                        <div id="suggestLeftRecvTitle" style='height:calc(<%# Eval("SuggestOilNameList").Count %> * 24px)'>
                            <span>積込車数</span>
                        </div>
                        <%-- 構内取り用の見出し --%>
                        <asp:PlaceHolder ID="phmiSuggestLeftRectTitle" runat="server" Visible='<%# Eval("HasMoveInsideItem") %>'>
                            <div id="miSuggestLeftRecvTitle" style='height:calc(<%#  If(Eval("HasMoveInsideItem"), DirectCast(Eval("MiDispData"), DispDataClass).SuggestOilNameList.Count, "0") %> * 24px)'>
                                <span data-tiptext='<%# String.Format("構内取り {6}営業所:{0}({1}) {6}荷主:{2}({3}) {6}油槽所:{4}({5}) ",
                                                                                                                                   Eval("MiSalesOfficeName"), Eval("MiSalesOffice"),
                                                                                                                                   Eval("MiShippersName"), Eval("MiShippersCode"),
                                                                                                                                   Eval("MiConsigneeName"), Eval("MiConsignee"),
                                                                                                                                   ControlChars.CrLf) %>'
                                    >構内取り</span>
                            </div>
                        </asp:PlaceHolder>
<%--                        <asp:Repeater runat="server" ID="repOilTypeNameListEmpty" DataSource='<%# Eval("SuggestOilNameList") %>' >
                            <ItemTemplate >
                                <div></div>
                            </ItemTemplate>
                        </asp:Repeater>--%>
                        <%--  積置きの画面表示なし？ --%>
                        <div class="suggestHDays">
                            <span class="suggestHdaysText">受入日</span>
                        </div>
                    </div>
                    <%--  二列目 --%>
                    <div class="oilTypeColumn" >
                        <div><span>日付</span></div>
                        <div><span>列車</span></div>
                        <div><span>受注作成</span></div>
                        <asp:Repeater runat="server" ID="repOilTypeNameList" DataSource='<%# Eval("SuggestOilNameList") %>' >
                            <ItemTemplate >
                                <div data-title="suggestValue"
                                     data-oilcode='<%# DirectCast(Eval("Value"), OilItem).OilCode %>'
                                     data-bigoilcode='<%# DirectCast(Eval("Value"), OilItem).BigOilCode %>'
                                     data-midoilcode='<%# DirectCast(Eval("Value"), OilItem).MiddleOilCode %>'>
                                    <span>
                                        <%# DirectCast(Eval("Value"), OilItem).OilName %>
                                    </span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Repeater runat="server" ID="repMiOilTypeNameList" DataSource='<%# If(DirectCast(Eval("MiDispData"), DispDataClass) Is Nothing, Nothing, DirectCast(Eval("MiDispData"), DispDataClass).SuggestOilNameList) %>' >
                            <ItemTemplate >
                                <div data-title="suggestValue" data-mi="1"
                                     data-oilcode='<%# DirectCast(Eval("Value"), OilItem).OilCode %>'
                                     data-bigoilcode='<%# DirectCast(Eval("Value"), OilItem).BigOilCode %>'
                                     data-midoilcode='<%# DirectCast(Eval("Value"), OilItem).MiddleOilCode %>'>
                                    <span>
                                        <%# DirectCast(Eval("Value"), OilItem).OilName %>
                                    </span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <%-- 構内取り用の油種 --%>
                        <div>
                            <span>&nbsp;</span>
                        </div>
                    </div>
                    <%-- 三列目以降 --%>
                    <asp:Repeater ID="repSuggestItem" runat="server"  DataSource='<%# Eval("SuggestListDisplay") %>' >
                        <ItemTemplate>
                            <div class='dataColumn has<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem).SuggestOrderItem.Count %>Col week<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem).DayInfo.WeekNum %> holiday<%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem).DayInfo.IsHoliday, "1", "0") %>'  >
                            <%-- 日付部分 --%>
                            <div class='suggestDate week<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem).DayInfo.WeekNum %>'>
                                <!-- -->
                                <span <%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem).DayInfo.IsHoliday, "data-tiptext='" & DirectCast(Eval("Value"), DispDataClass.SuggestItem).DayInfo.HolidayName & "'", "") %>>
                                    <%# DirectCast(Eval("Value"), DispDataClass.SuggestItem).DayInfo.DispDate %>
                                </span>
                                <span class="suggestDayAddRemove" data-listkey='<%# Eval("Key") %>' > <%# "＋" %> </span>
                                <asp:HiddenField ID="hdnSuggestListKey" runat="server" Value='<%# Eval("Key") %>' Visible="false" />
                            </div>
                            <%--列車・チェック・値のリピーター--%> 
                            <asp:Repeater ID="repSuggestTrainItem" runat="server"  
                                DataSource='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem).SuggestOrderItem %>' >
                                <ItemTemplate>
                                    <div class="values">
                                    <%--  列車 --%>
                                    <div class="trainNo"
                                         data-ispastday='<%#If(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).SuggestValuesItem.First.Value.DayInfo.IsPastDay = True,
                                                                                                                                                                "True",
                                                                                                                                                                "False") %>'>
                                        <div class="lockImgArea <%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).TrainLock, "Locked", "Unlocked") %>">
                                            <asp:HiddenField ID="hdnTrainLock" runat="server" 
                                                Value='<%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).TrainLock, "Locked", "Unlocked") %>'
                                                 />
                                        </div>
                                        <span data-tiptext='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).TrainInfo.TrainName %>'><%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).TrainInfo.DispTrainNo %>
                                            <asp:HiddenField ID="hdnTrainId" runat="server" Value='<%# Eval("Key") %>' Visible="false" />
                                        </span>
                                    </div>
                                    <%--  チェック --%>
                                    <div>
                                        <span>
                                            <asp:CheckBox ID="chkSuggest" runat="server" 
                                            Checked='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).CheckValue %>'
                                            Enabled ='<%# if(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).SuggestValuesItem.First.Value.DayInfo.IsPastDay = True,
                                                                                                                                                    "False",
                                                                                                                                                    "True") %>'    />
                                        </span>
                                    </div>
                                    <%--  各種値 --%>
                                    <asp:Repeater ID="repSuggestValueItem" runat="server"  
                                        DataSource='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).SuggestValuesItem %>' >
                                        <ItemTemplate>
                                            <%--  油種に紐づいた値 --%>
                                            <div class="num" data-oilcode='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.OilCode %>'
                                                             data-midoilcode='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.MiddleOilCode %>'
                                                             data-bigoilcode='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.BigOilCode %>'
                                                             <%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.OilCode = DispDataClass.SUMMARY_CODE,
                                                                                                                                           "data-tiptext='最大牽引車数:" & DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).TrainInfo.MaxVolume & "'",
                                                                                                                                           "") %> >
                                                <asp:HiddenField ID="hdnOilTypeCode" runat="server" Visible="false" Value='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.OilCode %>'  />
                                                <asp:TextBox ID="txtSuggestValue" runat="server" 
                                                    CssClass='<%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).ModFlg = "1", "modified", "") %>'
                                                    Text='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).ItemValue %>' 
                                                    Enabled='<%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.OilCode = DispDataClass.SUMMARY_CODE _
                                                                                            OrElse DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).DayInfo.IsPastDay = True,
                                                                                                "False",
                                                                                                "True") %>'></asp:TextBox>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    <%-- 構内取り用の値 --%>
                                    <asp:Repeater ID="repMiSuggestValueItem" runat="server"  
                                        DataSource='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).MiSuggestValuesItem %>' >
                                        <ItemTemplate>
                                            <%--  油種に紐づいた値 --%>
                                            <div class="num mi" data-mi="1"
                                                             data-oilcode='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.OilCode %>'
                                                             data-midoilcode='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.MiddleOilCode %>'
                                                             data-bigoilcode='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.BigOilCode %>'
                                                             <%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.OilCode = DispDataClass.SUMMARY_CODE,
                                                                                                                                           "data-tiptext='最大牽引車数:" & DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).TrainInfo.MaxVolume & "'",
                                                                                                                                           "") %> >
                                                <asp:HiddenField ID="hdnOilTypeCode" runat="server" Visible="false" Value='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.OilCode %>'  />
                                                <asp:TextBox ID="txtSuggestValue" runat="server" data-mi="1" 
                                                    CssClass='<%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).ModFlg = "1", "modified", "") %>'
                                                    Text='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).ItemValue %>' 
                                                    Enabled='<%# If(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).OilInfo.OilCode = DispDataClass.SUMMARY_CODE _
                                                                                                                            OrElse DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValue).DayInfo.IsPastDay = True,
                                                                                                                            "False",
                                                                                                                            "True") %>'></asp:TextBox>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    <div>
                                        <span>
                                            <asp:DropDownList ID="ddlSuggestAddDays" runat="server" 
                                                CssClass="ddlSuggestAccdays" 
                                                data-hideval='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).TrainInfo.AccDays %>' 
                                                data-loddate='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).SuggestValuesItem.First.Value.DayInfo.ItemDate.ToString("yyyy/MM/dd") %>'
                                                SelectedValue='<%# DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).AccAddDays %>'  
                                                Enabled='<%# if(DirectCast(Eval("Value"), DispDataClass.SuggestItem.SuggestValues).SuggestValuesItem.First.Value.DayInfo.IsPastDay = True,
                                                                                                                                                                            "False",
                                                                                                                                                                            "True") %>'
                                                >
                                                <asp:ListItem Text="" Value=""></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="0"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="1"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="2"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="3"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="4"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="5"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="6"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="7"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="8"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="9"></asp:ListItem>
                                                <asp:ListItem Text="&ensp;" Value="10"></asp:ListItem>
                                            </asp:DropDownList>
                                        </span>
                                    </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </ItemTemplate>
                <FooterTemplate>
                    <div></div>
                    </div>
                </FooterTemplate>
            </asp:FormView>
            </asp:Panel>  <!-- End 受注提案タンク車数 -->
            <!-- 比重一覧 これ非表示 （いづれ消す） -->
            <asp:Panel ID="pnlWeightList" runat="server">
                <div class="listTitle">比重</div>
                <asp:Repeater ID="repWeightList" runat="server" ClientIDMode="Predictable">
                    <HeaderTemplate>
                        <div id="weightListContainer">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <div class="weightListItem">
                            <div class="weightListOilType">
                                <span><%# DirectCast(Eval("Value"), OilItem).OilName %></span>
                            </div>
                            <div class="weightListValue">
                                <span><%# DirectCast(Eval("Value"), OilItem).Weight %></span>
                            </div>
                        </div>
                    </ItemTemplate>
                    <FooterTemplate>
                        <div></div>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>
            </asp:Panel>　<!-- End 比重一覧 -->
            <!-- 在庫表 -->
            <asp:Panel ID="pnlStockList" runat="server" >
                <div class="listTitle">在庫表</div>
                <div id="divStockList" class="full">
                    <!-- 1・2行目のヘッダー -->
                    <div class="header"> 
                        <div id="divEmptyBox" class="emptyBox">
                            <div id="dispLorry"><span id="spnDispLorry"></span></div>
                        </div>
                        <!-- 動的日付部の生成 -->
                        <asp:Repeater ID="repStockDate" runat="server">
                            <HeaderTemplate>
                                <div class="headerDate">
                                    <div class="colStockInfoTopRow"><span>日付</span></div>
                                    <div class="dateItem">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div class='colStockInfo date week<%# DirectCast(Eval("Value"), DaysItem).WeekNum %>  holiday<%# If(DirectCast(Eval("Value"), DaysItem).IsHoliday, "1", "0") %>'>
                                    <span <%# If(DirectCast(Eval("Value"), DaysItem).IsHoliday, "data-tiptext='" & DirectCast(Eval("Value"), DaysItem).HolidayName & "'", "") %>> <%# DirectCast(Eval("Value"), DaysItem).DispDate %></span>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                </div> <!-- End class="dateItem" -->
                                </div> <!-- End class="headerDate" -->
                            </FooterTemplate>
                        </asp:Repeater>
                        <div class="lastMargin"></div>
                    </div> <!-- End 1・2行目のヘッダー -->
                    <!-- 油種ごとのデータ生成部 -->
                    <asp:Repeater ID="repStockOilTypeItem" runat="server" ClientIDMode="Predictable">
                        <ItemTemplate>
                            <div class="oilTypeData" data-oilcode='<%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).OilTypeCode %>'
                                                     data-midoilcode='<%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).OilInfo.MiddleOilCode %>'
                                                     data-bigoilcode='<%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).OilInfo.BigOilCode %>'>
                                <div class="col1">
                                    <div><span><%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).OilTypeName %></span></div>
                                    <asp:HiddenField ID="hdnOilTypeCode" runat="server" Visible="false" Value='<%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).OilTypeCode %>' />
                                </div>
                                <div class="col2">
                                    <div><span>タンク容量</span></div>
                                    <div><span>目標在庫</span></div>
                                    <div><span>目標在庫率</span></div>
                                    <div><span>80%在庫</span></div>
                                    <div><span>D/S</span></div>
                                    <div><span>前週出荷平均</span></div>
                                </div>

                                <div class="col3">
                                    <div> <%--タンク容量値 --%>
                                        <span><%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).TankCapacity.ToString("#,##0") %></span>
                                    </div>
                                    <div> <%--目標在庫値 --%>
                                        <span><%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).TargetStock.ToString("#,##0") %></span>
                                    </div>
                                    <div> <%--目標在庫率値 --%>
                                        <span><%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).TargetStockRate.ToString("P1") %></span>
                                    </div>
                                    <div> <%--80%在庫 --%>
                                        <span><%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).Stock80.ToString("#,##0") %></span>
                                    </div>
                                    <div> <%-- D/S --%>
                                        <span><%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).DS.ToString("#,##0") %></span>
                                    </div>
                                    <div> <%--前週出荷平均 --%>
                                        <span><%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).LastShipmentAve.ToString("#,##0") %></span>
                                    </div>
                                </div>

                                <div class="col6">
                                    <div>朝在庫</div>
                                    <div>朝在庫D/S除</div>
                                    <div>保有日数</div>
                                    <div>在庫率</div>
                                    <div>受入</div>
                                    <div class="receiveFromLorry">ﾛｰﾘｰ受入</div>
                                    <div class="receiveSummary">受入計</div>
                                    <div>払出</div>
                                </div>

                                <%-- 日付毎の各値 --%>
                                <asp:Repeater ID="repStockValues" runat="server" DataSource='<%# DirectCast(Eval("Value"), DispDataClass.StockListCollection).StockItemListDisplay %>'>
                                    <ItemTemplate>
                                        <div class='colStockValue week<%# DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.WeekNum %> holiday<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsHoliday, "1", "0") %>' >
                                            <asp:HiddenField ID="hdnDateKey" runat="server" Visible="false" Value='<%# DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.KeyString %>' />
                                            <div><%--朝在庫--%>
                                                <span class='morningStockIdx<%# Container.ItemIndex %> <%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStock.Contains("-"), "minus", "") %>'>
                                                    <%-- 初日のみテキストボックス表示 --%>
                                                    <asp:TextBox ID="txtMorningStock" runat="server" 
                                                        CssClass='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStockModFlg = "1", "modified", "")  %>'
                                                        Text='<%# If(IsNumeric(DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStock),
                                                                                                         Decimal.Parse(DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStock).ToString("#,##0"),
                                                                                                         DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStock) %>'
                                                        Visible='<%# If(Container.ItemIndex = 0, True, True) %>'
                                                        data-textfield="MorningStock"
                                                        Enabled='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsBeforeToday OrElse
                                                                                                (DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsTomorrow AndAlso DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsTomorrowMorningStockActive),
                                                                                                                                                "True",
                                                                                                                                                "False") %>'
                                                        date-test1 ='<%# DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsTomorrow %>' 
                                                        date-test2 ='<%# DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsTomorrowMorningStockActive  %>'>
                                                    </asp:TextBox>
                                                    <asp:Label ID="lblMorningStock" runat="server" 
                                                        Text='<%# Decimal.Parse(DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStock).ToString("#,##0") %>'
                                                        Visible='<%# If(Container.ItemIndex = 0, False, False) %>'></asp:Label>
                                                </span>
                                            </div>
                                            <div><%--朝在庫D/S除--%>
                                                <span class='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStockWithoutDS < 0, "minus", "") %>'>
                                                    <%# DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStockWithoutDS.ToString("#,##0") %>
                                                </span>
                                            </div>
                                            <div><%--保持日数--%>
                                                <span class='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).Retentiondays < 0, "minus", "") %>'>
                                                    <%# DirectCast(Eval("Value"), DispDataClass.StockListItem).Retentiondays %>
                                                </span>
                                            </div>
                                            <div><%--在庫率--%>
                                                <span class='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).StockRate < 0, "minus", "") %>'>
                                                    <%# DirectCast(Eval("Value"), DispDataClass.StockListItem).StockRate.ToString("P1") %>
                                                </span>
                                            </div>
                                            <div><%--受入--%>
                                                <span class='<%# If(IsNumeric(DirectCast(Eval("Value"), DispDataClass.StockListItem).Receive) AndAlso DirectCast(Eval("Value"), DispDataClass.StockListItem).Receive < 0, "minus", "") %>'>
                                                    <asp:TextBox ID="txtReceive" runat="server" 
                                                        Text='<%# If(IsNumeric(DirectCast(Eval("Value"), DispDataClass.StockListItem).Receive),
                                                                                                                     Decimal.Parse(DirectCast(Eval("Value"), DispDataClass.StockListItem).Receive).ToString("#,##0"),
                                                                                                                     DirectCast(Eval("Value"), DispDataClass.StockListItem).Receive) %>'
                                                        Visible='<%# If(pnlSuggestList.Visible, True, True) %>'
                                                        data-textfield="Receive"
                                                        Enabled='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsBeforeToday,
                                                                                                                                    "True",
                                                                                                                                    "False") %>' >
                                                    </asp:TextBox>
                                                    <asp:Label ID="lblReceive" runat="server" 
                                                        Text='<%# Decimal.Parse(DirectCast(Eval("Value"), DispDataClass.StockListItem).Receive).ToString("#,##0") %>'
                                                        Visible='<%# If(pnlSuggestList.Visible, False, False) %>'></asp:Label>
                                                </span>
                                            </div>
                                            <div class="receiveFromLorry"><%-- ﾛｰﾘｰ受入 --%>
                                                <span class="stockinputtext">
                                                    <asp:TextBox ID="txtReceiveFromLorry" runat="server" Text='<%# If(IsNumeric(DirectCast(Eval("Value"), DispDataClass.StockListItem).ReceiveFromLorry),
                                                                                                                                             Decimal.Parse(DirectCast(Eval("Value"), DispDataClass.StockListItem).ReceiveFromLorry).ToString("#,##0"),
                                                                                                                                             DirectCast(Eval("Value"), DispDataClass.StockListItem).ReceiveFromLorry) %>'
                                                        Enabled='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsBeforeToday,
                                                                                                                        "True",
                                                                                                                        "True") %>'>
                                                    </asp:TextBox>
                                                </span>
                                            </div>
                                            <div class="receiveSummary"><%--受入計--%>
                                                <span class='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).SummaryReceive < 0, "minus", "") %>'>
                                                    <%# DirectCast(Eval("Value"), DispDataClass.StockListItem).SummaryReceive.ToString("#,##0") %>
                                                </span>
                                            </div>
                                            <div><%--払出--%>
                                                <span class="stockinputtext">
                                                    <asp:TextBox ID="txtSend" runat="server" Text='<%# If(IsNumeric(DirectCast(Eval("Value"), DispDataClass.StockListItem).Send),
                                                                                                                     Decimal.Parse(DirectCast(Eval("Value"), DispDataClass.StockListItem).Send).ToString("#,##0"),
                                                                                                                     DirectCast(Eval("Value"), DispDataClass.StockListItem).Send) %>'
                                                        CssClass='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).SendModFlg = "1", "modified", "")  %>'
                                                        Enabled='<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsBeforeToday,
                                                                                                                        "True",
                                                                                                                        "True") %>'>
                                                    </asp:TextBox>
                                                </span>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <div class="lastMargin"></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <div class="footer">
                        <asp:Repeater ID="repStockSummary" runat="server">
                            <HeaderTemplate>
                                <%--<div class="footerColHeader">--%>
                                    <div class="col1To3s">
                                        <div><span>合計</span></div>
                                    </div>
                                    <div class="col4s">
                                        <div class="colStockInfoBottomRow"><span>朝在庫</span></div>
                                        <div class="colStockInfoBottomRow"><span>受入</span></div>
                                        <div class="colStockInfoBottomRow"><span>払出</span></div>
                                    </div>
                                    <%--<div class="summaryItem">--%>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div class='colStockInfo date week<%# DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.WeekNum  %>  holiday<%# If(DirectCast(Eval("Value"), DispDataClass.StockListItem).DaysItem.IsHoliday, "1", "0") %>'>
                                    <!-- 朝在庫 -->
                                    <div>
                                        <span class='<%# If(CDec(DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStock) < 0, "minus", "") %>'>
                                            <%# DirectCast(Eval("Value"), DispDataClass.StockListItem).MorningStock %>
                                        </span>
                                    </div>
                                    <!-- 受入 -->
                                    <div>
                                        <span class='<%# If(CDec(DirectCast(Eval("Value"), DispDataClass.StockListItem).Receive) < 0, "minus", "") %>'>
                                            <%# DirectCast(Eval("Value"), DispDataClass.StockListItem).Receive  %>
                                        </span>
                                    </div>
                                    <!-- 払出 -->
                                    <div>
                                        <span class='<%# If(CDec(DirectCast(Eval("Value"), DispDataClass.StockListItem).Send) < 0, "minus", "") %>'>
                                            <%# DirectCast(Eval("Value"), DispDataClass.StockListItem).Send  %>
                                        </span>
                                    </div>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <%--</div>--%> <!-- End summaryItem -->
                                <%--</div>--%>
                            </FooterTemplate>
                        
                        </asp:Repeater> 
                        <div class="lastMargin"></div>
                    </div>
                </div> <!-- End id="divStockList" -->

            </asp:Panel> <!-- End 在庫表 -->
        </div> <!-- end class="headerboxOnly" id="headerbox" -->
        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

        <!-- イベント用 -->
        <div style="display:none;">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
            <!-- GridView DBクリック-->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>
            <!-- GridView表示位置フィールド -->

            <input id="WF_FIELD" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD_REP" runat="server" value="" type="text" />
            <!-- Textbox(Repeater) DBクリックフィールド -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->

            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
            <!-- LeftBox Mview切替 -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
            <!-- LeftBox 開閉 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text" />
            <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value="" type="text" />
            <!-- Rightbox 開閉 -->

            <input id="WF_PrintURL" runat="server" value="" type="text" />
            <!-- Textbox Print URL -->

            <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />
            <!-- 一覧・詳細画面切替用フラグ -->

            <input id="WF_ButtonClick" runat="server" value="" type="text" />
            <!-- ボタン押下 -->
            <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
            <!-- 在庫表で表示する油種保持用 -->
            <asp:ListBox ID="lstDispStockOilType" runat="server" SelectionMode="Multiple"></asp:ListBox>
            <!-- ローリー表示・非表示状態保持用 full or hideLorry -->
            <asp:HiddenField ID="hdnDispLorry" runat="server" Value="full" />
            <!-- 油槽所変更値保持用 -->
            <asp:HiddenField ID="hdnChgConsignee" runat="server" />
            <asp:HiddenField ID="hdnChgConsigneeName" runat="server" />
            <!-- 現在使用油槽所保持 -->
            <asp:HiddenField ID="hdnCurConsignee" runat="server" />
            <!-- 油槽所変更時初回ロード 0:通常、1:初回 -->
            <asp:HiddenField ID="hdnChgConsigneeFirstLoad" runat="server" Value="0" />
            <!-- 帳票設定表示状態保持用 -->
            <asp:HiddenField ID="hdnDispReportSettings" runat="server" value="0"/>
            <!-- 提案表の日付追加・削除ボタン押下元情報保持 -->
            <asp:HiddenField ID="hdnSuggestDateAddRemoveKey" runat="server" value="" EnableViewState ="false" />
            <!-- スクロール位置の保持 -->
            <asp:HiddenField ID="hdnThisScrollLeft" runat="server" Value="" />
            <asp:HiddenField ID="hdnThisScrollTop" runat="server" Value="" />
            <!-- 権限 -->
        </div>
 
</asp:Content>
<%--ポップアップタイトルバーの文字--%>
<asp:Content ID="ctCostumPopUpTitle" ContentPlaceHolderID ="contentsPopUpTitle" runat="server">
    <span id="popUpPrintTitle">帳票設定</span>
    <span id="popUpFixTitle">オーダー確定</span>
</asp:Content>
<%--ポップアップタイトルバーの内容--%>
<asp:Content ID="ctCostumPopUp" ContentPlaceHolderID ="contentsPopUpInside" runat="server">
    <asp:HiddenField ID="hdnPopUpType" runat="server" Value="print" />
    <div id="popUpPrintSettings">
        <div id="divChkEneos" runat="server">
            <div class="grc0001Wrapper">
                <ul>
                    <li>
                        <asp:CheckBox ID="chkPrintENEOS" runat="server" Text="ENEOS用帳票" />
                    </li>
                    <li>
                        <asp:CheckBox ID="chkPrintConsigneeRep" runat="server" Text="油槽所在庫" /> 
                    </li>
                </ul>
            </div>
        </div>
        <div>
            <span id="spnDownloadMonth" <%= If(hdnShowPnlToDate.Value = "1", "", "style='display:none;'") %> >
                <asp:HiddenField ID="hdnShowPnlToDate" runat="server" Value="1" />
                <asp:Label ID="Label1" runat="server" Text="帳票年月"></asp:Label>
                <asp:TextBox ID="txtDownloadMonth" runat="server" data-monthpicker="1"></asp:TextBox>
            </span>
        </div>
        <div>
            <span id="spnFromDate" <%= If(hdnShowPnlToDate.Value = "1", "style='display:none;'", "") %>>
                <asp:Label ID="lblReportFromDate" runat="server" Text="開始日"></asp:Label>
                <a class="ef" id="aReportFromDate" ondblclick="Field_DBclick('txtReportFromDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="txtReportFromDate" runat="server" CssClass="calendarIcon"  onblur="MsgClear();"></asp:TextBox>
                    <asp:CheckBox ID="chkConsigneeRepDoubleSpan" runat="server" text="12日分" />
                </a>
            </span>
        </div>
    <%--    <div id="pnlToDate" <%= If(hdnShowPnlToDate.Value = "1", "", "style='display:none;'") %>>
            <asp:HiddenField ID="hdnShowPnlToDate" runat="server" Value="1" />
            <span id="spnToDate">
                <asp:Label ID="lblReportToDate" runat="server" Text="終了日"></asp:Label>
                <a class="ef" id="aReportToDate" ondblclick="Field_DBclick('txtReportToDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="txtReportToDate" runat="server" CssClass="calendarIcon"  onblur="MsgClear();"></asp:TextBox>
                </a>
            </span>
        </div>--%>
    </div>
    <div id="popUpFixSettings" class="">
        <div>
            <span>以下の日付のオーダー確定をします。よろしいですか？</span>
        </div>
        <div class="divFixDateArea">
            <span >
                <asp:Label ID="lblFixDateLabel" runat="server" Text="オーダー確定日"></asp:Label>
                <a class="ef" id="aFixDate" ondblclick="Field_DBclick('txtFixDate', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);"">
                    <asp:TextBox ID="txtFixDate" runat="server" CssClass="calendarIcon" Text=""></asp:TextBox>
                </a>
                <asp:Label ID="lblFixStatus" runat="server" Text=""></asp:Label>
            </span>

        </div>
    </div>
</asp:Content>
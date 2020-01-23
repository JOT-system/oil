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
    </script>
</asp:Content>
 
<asp:Content ID="OIT0004L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="actionButtonBox">
                <div class="leftSide">

                    <a >在庫維持日数</a>
                    <a class="ef">
                        <asp:TextBox ID="WF_INVENTORYDAYS" runat="server" onblur="MsgClear();"></asp:TextBox>
                    </a>

                    <!-- ボタン -->
                    <input type="button" id="WF_ButtonAUTOSUGGESTION" class="btn-sticky" value="自動提案"     onclick="ButtonClick('WF_ButtonAUTOSUGGESTION');" />
                    <input type="button" id="WF_ButtonORDERLIST"      class="btn-sticky" value="受注作成"     onclick="ButtonClick('WF_ButtonORDERLIST');" />
                    <input type="button" id="WF_ButtonINPUTCLEAR"     class="btn-sticky" value="入力値クリア" onclick="ButtonClick('WF_ButtonINPUTCLEAR');" />
                </div>

                <div class="rightSide">
                    <input type="button" id="WF_ButtonUPDATE"        class="btn-sticky" value="更新"     onclick="ButtonClick('WF_ButtonUPDATE');" />
                    <input type="button" id="WF_ButtonCSV"           class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonCSV');" />
                    <input type="button" id="WF_ButtonINSERT"        class="btn-sticky" value="新規登録" onclick="ButtonClick('WF_ButtonINSERT');" />
                    <input type="button" id="WF_ButtonEND"           class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                    <div                 id="WF_ButtonFIRST"         class="firstPage"  runat="server"   visible="false" onclick="ButtonClick('WF_ButtonFIRST');"></div>
                    <div                 id="WF_ButtonLAST"          class="lastPage"   runat="server"   visible="false" onclick="ButtonClick('WF_ButtonLAST');"></div>
                </div>
            </div> <!-- End class=actionButtonBox -->
            <!-- 受注提案タンク車数 (vbコード内で pnlSuggestList.Visible=Falseで消せる -->
            <asp:Panel ID="pnlSuggestList" runat="server" ViewStateMode="Disabled" >
            <div class="listTitle">受注提案タンク車数</div>
            <asp:FormView ID="frvSuggest" runat="server" ViewStateMode="Disabled" RenderOuterTable="false">
                <HeaderTemplate>
                    <div id="divSuggestList" style='height:calc(<%# Eval("SuggestOilNameList").Count + 3  %> * 24px)'>
                </HeaderTemplate>
                <ItemTemplate>
                    <%--  一列目 --%>
                    <div class="leftColumn">
                        <div>
                            <span>内訳</span>
                        </div>
                        <div>
                            <span>受入数</span>
                        </div>
                        <asp:Repeater runat="server" ID="repOilTypeNameListEmpty" DataSource='<%# Eval("SuggestOilNameList") %>' ViewStateMode="Disabled">
                            <ItemTemplate >
                                <div></div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <%--  積置きの画面表示なし？ --%>
                    </div>
                    <%--  二列目 --%>
                    <div class="oilTypeColumn">
                        <div><span>日付</span></div>
                        <div><span>列車</span></div>
                        <div><span>油種</span></div>
                        <asp:Repeater runat="server" ID="repOilTypeNameList" DataSource='<%# Eval("SuggestOilNameList") %>' ViewStateMode="Disabled">
                            <ItemTemplate >
                                <div><span><%# Eval("Value") %></span></div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <%-- 三列目以降 --%>
                    <asp:Repeater ID="repSuggestItem" runat="server"  DataSource='<%# Eval("SuggestList") %>' ViewStateMode="Disabled">
                        <ItemTemplate>
                            <div class='dataColumn has<%# DirectCast(Eval("Value"), DemoDispDataClass.SuggestItem).SuggestOrderItem.Count %>Col'  >
                            <!-- 日付部分 -->
                            <div class="suggestDate">
                                <!-- -->
                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.SuggestItem).DispDate %></span>
                            </div>
                            <%--列車・チェック・値のリピーター--%> 
                            <asp:Repeater ID="repSuggestItem" runat="server"  
                                DataSource='<%# DirectCast(Eval("Value"), DemoDispDataClass.SuggestItem).SuggestOrderItem %>' ViewStateMode="Disabled">
                                <ItemTemplate>
                                    <div class="values">
                                    <%--  列車 --%>
                                    <div>
                                        <span>
                                        <%# Eval("Key") %>
                                        </span>
                                    </div>
                                    <%--  チェック --%>
                                    <div>
                                        <span>
                                            <asp:CheckBox ID="chkSuggest" runat="server" 
                                            Checked='<%# DirectCast(Eval("Value"), DemoDispDataClass.SuggestItem.SuggestValues).CheckValue %>' />
                                        </span>
                                    </div>
                                    <%--  各種値 --%>
                                    <asp:Repeater ID="repSuggestItem" runat="server"  
                                        DataSource='<%# DirectCast(Eval("Value"), DemoDispDataClass.SuggestItem.SuggestValues).SuggestValuesItem %>' ViewStateMode="Disabled">
                                        <ItemTemplate>
                                            <%--  油種に紐づいた値 --%>
                                            <div class="num" data-oilcode='<%# DirectCast(Eval("Value"), DemoDispDataClass.SuggestItem.SuggestValue).OilCode %>'>
                                                <asp:TextBox ID="txtSuggestValue" runat="server" ViewStateMode="Disabled"
                                                    Text='<%# DirectCast(Eval("Value"), DemoDispDataClass.SuggestItem.SuggestValue).ItemValue %>' 
                                                    Enabled='<%# If(DirectCast(Eval("Value"), DemoDispDataClass.SuggestItem.SuggestValue).OilCode = DemoDispDataClass.SUMMARY_CODE, "False", "True") %>'></asp:TextBox>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
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
            <!-- 比重一覧 -->
            <asp:Panel ID="pnlWeightList" runat="server">
                <div class="listTitle">比重</div>
                <asp:Repeater ID="repWeightList" runat="server" ViewStateMode="Disabled">
                    <HeaderTemplate>
                        <div id="weightListContainer">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <div class="weightListItem">
                            <div class="weightListOilType">
                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.WeightListItem).OilTypeName %></span>
                            </div>
                            <div class="weightListValue">
                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.WeightListItem).Weight %></span>
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
            <asp:Panel ID="pnlStockList" runat="server" ViewStateMode="Disabled">
                <div class="listTitle">在庫表</div>
                <div id="divStockList">
                    <!-- 1・2行目のヘッダー -->
                    <div class="header"> 
                        <div class="emptyBox"></div>
                        <!-- 動的日付部の生成 -->
                        <asp:Repeater ID="repStockDate" runat="server">
                            <HeaderTemplate>
                                <div class="headerDate">
                                    <div class="colStockInfoTopRow"><span>日付</span></div>
                                    <div class="dateItem">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div class="colStockInfo date">
                                    <span><%# Eval("Key") %></span>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                </div> <!-- End class="dateItem" -->
                                </div> <!-- End class="headerDate" -->
                            </FooterTemplate>
                        </asp:Repeater>
                        <div class="lastMargin"></div>
                    </div> <!-- End 1・2行目のヘッダー -->
                    <!-- End 油種ごとのデータ生成部 -->
                    <asp:Repeater ID="repStockOilTypeItem" runat="server" ViewStateMode="Disabled">
                        <ItemTemplate>
                            <div class="oilTypeData">
                                <div class="col1">
                                    <div><span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).OilTypeName %></span></div>
                                </div>
                                <div class="col2">
                                    <div><span>タンク容量</span></div>
                                    <div><span>目標在庫</span></div>
                                    <div><span>目標在庫率</span></div>
                                </div>

                                <div class="col3">
                                    <div> <%--タンク容量値 --%>
                                        <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).TankCapacity %></span>
                                    </div>
                                    <div> <%--目標在庫値 --%>
                                        <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).TargetStock %></span>
                                    </div>
                                    <div> <%--目標在庫率値 --%>
                                        <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).TargetStockRate %></span>
                                    </div>
                                </div>
                                <div class="col4">
                                    <div><span>80%在庫</span></div>
                                    <div><span>D/S</span></div>
                                    <div><span>前週出荷 平均</span></div>
                                </div>

                                <div class="col5">
                                    <div> <%--80%在庫 --%>
                                        <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).Stock80 %></span>
                                    </div>
                                    <div> <%-- D/S --%>
                                        <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).DS %></span>
                                    </div>
                                    <div> <%--前週出荷平均 --%>
                                        <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).LastShipmentAve %></span>
                                    </div>
                                </div>
                                <div class="col6">
                                    <div>前日夕在庫</div>
                                    <div>保持日数</div>
                                    <div>朝在庫</div>
                                    <div>受入</div>
                                    <div>払出</div>
                                    <div>夕在庫</div>
                                    <div>夕在庫D/S除</div>
                                    <div>空容量</div>
                                    <div>在庫率</div>
                                </div>

                                <%-- 日付毎の各値 --%>
                                <asp:Repeater ID="repStockValues" runat="server" ViewStateMode="Disabled" DataSource='<%# DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).StockItemList %>'>
                                    <ItemTemplate>
                                        <div class="colStockValue">
                                            <div><%--前日夕在庫--%>
                                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListItem).LastEveningStock.ToString("#,###") %></span>
                                            </div>
                                            <div><%--保持日数--%>
                                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListItem).Retentiondays %></span>
                                            </div>
                                            <div><%--朝在庫--%>
                                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListItem).MorningStock %></span>
                                            </div>
                                            <div><%--受入--%>
                                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListItem).Receive %></span>
                                            </div>
                                            <div><%--払出--%>
                                                <span>
                                                    <asp:TextBox ID="txtSend" runat="server" Text='<%# DirectCast(Eval("Value"), DemoDispDataClass.StockListItem).Send %>'></asp:TextBox>
                                                </span>
                                            </div>
                                            <div><%--夕在庫--%>
                                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListItem).EveningStock %></span>
                                            </div>
                                            <div><%--夕在庫D/S除--%>
                                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListItem).EveningStockWithoutDS %></span>
                                            </div>
                                            <div><%--空容量--%>
                                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListItem).FreeSpace %></span>
                                            </div>
                                            <div><%--在庫率--%>
                                                <span><%# DirectCast(Eval("Value"), DemoDispDataClass.StockListItem).StockRate %></span>
                                            </div>
                                        </div>

                                    </ItemTemplate>
                                </asp:Repeater>
                                <div class="lastMargin"></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
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
            <!-- 権限 -->
        </div>
 
</asp:Content>

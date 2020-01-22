<%@ Page Title="OIT0004C" Language="vb" AutoEventWireup="false" MasterPageFile="~/OIL/OILMasterPage.Master" CodeBehind="OIT0004OilStockCreate.aspx.vb" Inherits="JOTWEB.OIT0004OilStockCreate" %>
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
                        <div class="col1"></div>
                        <div class="col1"></div>
                        <div class="col2"></div>
                        <div class="col2"></div>
                        <div class="col3"></div>
                        <div class="col3"></div>
                        <div class="col4"></div>
                        <div class="col4"></div>
                        <div class="col5"></div>
                        <div class="col5"></div>
                        <div class="col6"></div>
                        <div class="col6"></div>
                        <!-- 動的日付部の生成 -->
                        <asp:Repeater ID="repStockDate" runat="server">
                            <ItemTemplate>
                                <div class="colStockInfo">
                                    <span><%# If(Container.ItemIndex = 0, "日付", "")  %></span>
                                </div>
                                <div class="colStockInfo">
                                    <span><%# Eval("Key") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div> <!-- End 1・2行目のヘッダー -->
                    <!-- End 油種ごとのデータ生成部 -->
                    <asp:Repeater ID="repStockOilTyleItem" runat="server" ViewStateMode="Disabled">
                        <ItemTemplate>
                            <div class="oilTypeData">
                                <div class="col1">
                                    <span><%#DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).OilTypeName %></span>
                                </div>
                                <div class="col2"><span>タンク容量</span></div>
                                <div class="col2"><span>目標在庫</span></div>
                                <div class="col2"><span>目標在庫率</span></div>
                                <div class="col3"> <%--タンク容量値 --%>
                                    <span><%#DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).TankCapacity %></span>
                                </div>
                                <div class="col3"> <%--目標在庫値 --%>
                                    <span><%#DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).TargetStock %></span>
                                </div>
                                <div class="col3"> <%--目標在庫率値 --%>
                                    <span><%#DirectCast(Eval("Value"), DemoDispDataClass.StockListCollection).TargetStockRate %></span>
                                </div>
                                <div class="col4"><span>タンク容量</span></div>
                                <div class="col4"><span>目標在庫</span></div>
                                <div class="col4"><span>目標在庫率</span></div>
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

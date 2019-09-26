<%@ Page Title="CO0011" Language="vb" AutoEventWireup="false" CodeBehind="GRCO0011PROFMXLS.aspx.vb" Inherits="OFFICE.GRCO0011PROFMXLS" %>
<%@ MasterType VirtualPath="~/GR/GRMasterPage.Master" %>

<%@ Import Namespace="OFFICE.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/GR/inc/GRCO0011WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="CO0011H" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/GR/css/CO0011.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/GR/script/CO0011.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=if(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="CO0011" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　headerbox -->
    <div class="headerboxOnly" id="headerbox">
        <div class="Operation">
            <!-- 画面ID -->
            <a>
                <asp:Label ID="WF_SELMAPID_L" runat="server" Text="画面ID" Height="1.5em" Font-Bold="true" Font-Underline="true"></asp:Label>
            </a>
            <a ondblclick="Field_DBclick('WF_SELMAPID', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)">
                <asp:TextBox ID="WF_SELMAPID" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS" BorderStyle="NotSet"></asp:TextBox>
            </a>
            <a>
                <asp:Label ID="WF_SELMAPID_TEXT" runat="server" Width="30em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- ボタン -->
            <a style="position:fixed; top:2.8em; left:58em;">
                <input type="button" id="WF_ButtonExtract" value="絞り込み" style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
            </a>
            <a style="position:fixed; top:2.8em; left:62.5em;">
                <input type="button" id="WF_ButtonUPDATE" value="DB更新" style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
            </a>
            <a style="position:fixed; top:2.8em; left:67em;">
                <input type="button" id="WF_ButtonEND" value="終了" style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
            </a>
            <a style="position:fixed; top:3.2em; left:75em;">
                <asp:Image ID="WF_ButtonFIRST" runat="server" ImageUrl="~/先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>
            <a style="position:fixed; top:3.2em; left:77em;">
                <asp:Image ID="WF_ButtonLAST" runat="server" ImageUrl="~/最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
            </a>
        </div>
        <div id="divListArea">
            <asp:panel id="pnlListArea" runat="server"></asp:panel>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div class="detailboxOnly" id="detailbox">
        <div id="detailbuttonbox" class="detailbuttonbox">
            <a>
                <input type="button" id="WF_UPDATE" value="表更新" style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
            </a>
            <a>
                <input type="button" id="WF_CLEAR" value="クリア" style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
            </a>
            <a>
                <input type="button" id="WF_BACK" value="戻る" style="Width:5em" onclick="ButtonClick('WF_BACK');" />
            </a>
        </div>
        
        <div id="detailkeybox">
            <p id="KEY_LINE_1">
                <!-- 選択No -->
                <a>
                    <asp:Label ID="WF_Sel_LINECNT_L" runat="server" Text="選択No" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_Sel_LINECNT" runat="server" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>

                <!-- 会社コード -->
                <a>
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE" runat="server" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Width="14em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_2">
                <!-- プロフID -->
                <a>
                    <asp:Label ID="WF_PROFID_L" runat="server" Text="プロフID" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_PROFID" runat="server" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>

                <!-- 画面ID -->
                <a>
                    <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ID" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:Label ID="WF_MAPID" runat="server" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="WF_MAPID_TEXT" runat="server" Width="30em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
            <p id="KEY_LINE_3">
                <!-- レポートID -->
                <a>
                    <asp:Label ID="WF_REPORTID_L" runat="server" Text="レポートID" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_REPORTID" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>

                <!-- レポート名称 -->
                <a>
                    <asp:Label ID="WF_REPORTNAMES_L" runat="server" Text="レポート名称" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_REPORTNAMES" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>

                <!-- 有効年月日 -->
                <a>
                    <asp:Label ID="WF_YMD_L" runat="server" Text="有効年月日" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_STYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_STYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                    <asp:Label runat="server" Text=" ～ " CssClass="WF_TEXT_LEFT"></asp:Label>
                    <b ondblclick="Field_DBclick('WF_ENDYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                        <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                </a>
            </p>
            <p id="KEY_LINE_4">
                <!-- EXCELファイル名 -->
                <a>
                    <asp:Label ID="WF_EXCELFILE_L" runat="server" Text="EXCELﾌｧｲﾙ名" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_EXCELFILE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>

                <!-- 明細開始行 -->
                <a>
                    <asp:Label ID="WF_POSISTART_L" runat="server" Text="明細開始行" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                    <asp:TextBox ID="WF_POSISTART" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>

                <!-- 削除フラグ -->
                <a ondblclick="Field_DBclick('WF_DELFLG', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)">
                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Underline="true"></asp:Label>
                    <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_DELFLG_TEXT" runat="server" Width="17em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            </p>
        </div>

        <!-- 項目選択 -->
        <div id="nouselist">
            <p style="position:relative; height:5%;">
                <asp:Label runat="server" Text="項目選択" Width="14em" CssClass="WF_TEXT_CENTER" Font-Bold="true"></asp:Label>
            </p>
            <div id="WF_RepExcelList" class="WF_Repeater" style="background-color:rgb(220, 230, 240);" onmousedown="ExcelMouseDown(0, 0, '');" onmouseup="ExcelMouseUp(0, 0);">
                <asp:Table ID="WF_EXCEL_LIST" runat="server"></asp:Table>
            </div>
            
            <!-- 追加・削除ボタン -->
            <div id="editbuttonbox">
                <p>
                    <a>
                        <asp:RadioButton ID="WF_ROW" runat="server" GroupName="WF_SHIFT" Text=" 行" Width="3em" />
                    </a>
                    <a>
                        <input type="button" id="WF_INSERT" value="＋" style="width:2em;height:2em;" onclick="ButtonClick('WF_INSERT');" />
                    </a>
                </p>
                <p>
                    <a>
                        <asp:RadioButton ID="WF_COL" runat="server" GroupName="WF_SHIFT" Text=" 列" Width="3em" />
                    </a>
                    <a>
                        <input type="button" id="WF_DELETE" value="－" style="width:2em;height:2em;" onclick="ButtonClick('WF_DELETE');" />
                    </a>
                </p>
            </div>
        </div>

        <div id="uselist">
            <!-- Excel切替 -->
            <p style="position:relative; height:5%; left:1em;">
                <a>
                    <asp:RadioButton ID="WF_TITLEKBN_TITLE" runat="server" GroupName="WF_Excel_SW" Text=" Excelタイトル" Width="9em" Font-Bold="true" OnClick="ExcelSelectChange()"/>
                </a>
                <a>
                    <asp:RadioButton ID="WF_TITLEKBN_ITEM" runat="server" GroupName="WF_Excel_SW" Text=" Excel行明細" Width="9em" Font-Bold="true" OnClick="ExcelSelectChange()" />
                </a>
            </p>

            <!-- DETAIL画面 -->
            <div id="WF_RepDetail" class="WF_Repeater">
                <asp:Table ID="WF_EXCEL" runat="server"></asp:Table>
            </div>

            <!-- 選択項目詳細 -->
            <p style="position:relative; height:5%;">
                <a style="margin-right:0.5em;">
                    <asp:Label runat="server" Text="詳細情報(Excelイメージ選択箇所)" Width="15em" CssClass="WF_TEXT_LEFT" Font-Bold="true"></asp:Label>
                </a>
                <a>
                    <asp:Label runat="server" Text="選択行列(" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
                <a>
                    <asp:Label ID="WF_POSIROW" runat="server" Width="2em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </a>
                <a>
                    <asp:Label runat="server" Text="," CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
                <a>
                    <asp:Label ID="WF_POSICOL" runat="server" Width="2em" CssClass="WF_TEXT_CENTER"></asp:Label>
                </a>
                <a style="margin-right:0.5em;">
                    <asp:Label runat="server" Text=")" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
                <a>
                    <asp:Label runat="server" Text="※選択操作（青表示）　：　Excelイメージ箇所をダブルクリックする。" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>
            </p>

            <!-- 追加機能 -->
            <div id="addfunction">
                <p>
                    <!-- 幅 -->
                    <a>
                        <asp:Label ID="WF_WIDTH_L" runat="server" Text="幅" Width="2.5em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_WIDTH" runat="server" Height="1.1em" Width="3.5em" style="background-color:rgb(255, 255, 170)"></asp:TextBox>
                    </a>

                    <!-- ソート -->
                    <a>
                        <asp:Label ID="WF_SORT_L" runat="server" Text="ソート" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_SORT" runat="server" Height="1.1em" Width="3.5em" style="background-color:rgb(255, 255, 170)"></asp:TextBox>
                    </a>

                    <!-- 項目名 -->
                    <a>
                        <asp:Label ID="WF_FIELDNAMES_L" runat="server" Text="項目名" Width="4em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="WF_FIELDNAMES" runat="server" Height="1.1em" Width="11em" style="background-color:rgb(255, 255, 170)"></asp:TextBox>
                    </a>
                </p>
            </div>
        </div>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>      <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>     <!-- GridView表示位置フィールド -->

        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />             <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD_EXCEL" runat="server" value="" type="text" />           <!-- エクセル明細選択項目 -->
        
        <input id="WF_DISP" runat="server" value="" type="text" />                  <!-- 画面表示切替 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />       <!-- Rightbox Mview切替 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_List_Top" runat="server" value="" type="text" />              <!-- 項目選択リストY軸 -->
        <input id="WF_Scroll_Left" runat="server" value="" type="text" />           <!-- スクロール位置X軸 -->
        <input id="WF_Scroll_Top" runat="server" value="" type="text" />            <!-- スクロール位置Y軸 -->

        <input id="WF_EXCEL_SELECT" runat="server" value="" type="text" />          <!-- 項目選択リスト -->
        <input id="WF_EXCEL_ROW" runat="server" value="" type="text" />             <!-- エクセル表示行位置 -->
        <input id="WF_EXCEL_COL" runat="server" value="" type="text" />             <!-- エクセル表示列位置 -->
        <input id="WF_DELCOL" runat="server" value="" type="text" />                <!-- エクセル削除列 -->

        <input id="WF_DRAG_START" runat="server" value="" type="text" />            <!-- 移動開始判定 -->
        <input id="WF_EXCEL_START_ROW" runat="server" value="" type="text" />       <!-- 移動前行位置 -->
        <input id="WF_EXCEL_START_COL" runat="server" value="" type="text" />       <!-- 移動前列位置 -->
        <input id="WF_EXCEL_END_ROW" runat="server" value="" type="text" />         <!-- 移動先行位置 -->
        <input id="WF_EXCEL_END_COL" runat="server" value="" type="text" />         <!-- 移動先列位置 -->

        <input id="WF_XMLsaveF" runat="server" value="" type="text" />              <!-- 保存先TblURL -->
        <input id="WF_XMLsaveF_INP" runat="server" value="" type="text" />          <!-- 保存先INPTblURL -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />         <!-- 権限 -->
    </div>
</asp:Content>

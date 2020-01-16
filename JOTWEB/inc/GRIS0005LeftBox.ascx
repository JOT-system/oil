<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0005LeftBox.ascx.vb" Inherits="JOTWEB.GRIS0005LeftBox" %>

         <!-- 全体レイアウト　leftbox -->
        <script type="text/javascript">
        //左ボックスの拡張機能
        // 拡張機能を紐づけるリスト及び機能のフラグの配列 
        // {コントロールのID, ソート機能フラグ, フィルタ機能フラグ}
        //  ※ソート機能フラグ(0:無し, 1:名称のみ, 2:コードのみ, 3:両方)
        //  ※フィルタ機能フラグ(0:無し, 1:設定)
        var leftListExtentionTarget = [
            ['<%= WF_LeftListBox.ClientID%>', '<%= LF_SORTING_CODE%>', '<%= LF_FILTER_CODE%>','']
            , ['<%= pnlLeftList.ClientID%>', '<%= LF_SORTING_CODE%>', '<%= LF_FILTER_CODE%>','<%=LF_PARAM_DATA%>']
        ];
       </script>
        <div class="LF_LEFTBOX" id="LF_LEFTBOX">
            <div class="button" id="button">
                <input type="hidden" id="WF_ButtonSel" class="btn-sticky" value="　選　択　"  onclick="ButtonClick('WF_ButtonSel');" />
                <input type="button" id="WF_ButtonCan" class="btn-sticky" value="閉 じ る"  onclick="ButtonClick('WF_ButtonCan');" />
            </div>
            <asp:MultiView ID="WF_LEFTMView" runat="server">
                <!-- 　リストボックス　 -->
                <asp:View id="tabL" runat="server" >
                    <a id="LF_ListArea" class="LF_ListArea" ondblclick="ListboxDBclick()">
                        <asp:ListBox ID="WF_LeftListBox" runat="server" CssClass="WF_ListBoxArea" />
                    </a>
                </asp:View>
                <!-- 　カレンダー　 -->
                <asp:View id="tabC" runat="server" >
                <div id="calWrap" class="calWrap">
                    <a>
                        <asp:textbox ID="WF_Calendar" runat="server" type="hidden"/>
                        <div id="dValue">

                        </div> 
                        <table border="0">
                            <tr>
                                <td colspan="3">
                                    <table border="0" >
                                        <tr>
                                            <td>
                                                <div id="carenda" ></div>
                                                 <script type="text/JavaScript">
                                                    <!--    
                                                        carenda(0,'<%=WF_Calendar.ClientID%>');
                                                    //-->
                                                    </script>
                                                
                                            </td> 
                                        </tr>
                                        <tr>
                                            <td id="altMsg">
                                                <script type="text/JavaScript">
                                                <!--    
                                                    setAltMsg(firstAltYMD, firstAltMsg);
                                                //-->
                                                </script>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </a>

                </div>
                </asp:View>
                <!-- 　テーブル　 -->
                <asp:View id="tabT" runat="server" >
                    <asp:panel id="pnlLeftList" runat="server" ></asp:panel>
                    <asp:textbox ID="WF_TBL_SELECT" runat="server" type="hidden"/>
                </asp:View>
            </asp:MultiView>
    </div>
<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0004RightBox.ascx.vb" Inherits="OFFICE.GRIS0004RightBox" %>

        <!-- 全体レイアウト　rightbox -->
        <div class="rightbox" id="RF_RIGHTBOX">
            <div id="RF_ERR_MEMO">
                <a>
                    <span style="position:relative;left:1.5em;right:1em;top:1.2em; width:80px; font-size:1.4rem;" >
                            <asp:RadioButton ID="RF_RIGHT_SW1" runat="server" GroupName="rightbox" Text=" エラー詳細表示" Width="9em" Onclick="rightboxChange('0')" Checked="True"/>
                            <asp:RadioButton ID="RF_RIGHT_SW2" runat="server" GroupName="rightbox" Text=" メモ表示" Width="9em" Onclick="rightboxChange('1')"  />
                    </span>
                </a>
                <br />

                <asp:MultiView ID="RF_RIGHTVIEW" runat="server">
                    <!-- 　エラー　 --> 
                    <asp:View id="RF_VIEW1" runat="server" >
                        <a id="RF_RIGHTBOX_ERROR_REPORT">
                            <span id="RF_ERROR_REPORT" style="position:relative;left:1em;top:2em;" >
                            <asp:TextBox ID="RF_ERR_REPORT" runat="server" Width="28.4em" Height="16.9em" TextMode="MultiLine"></asp:TextBox>
                            </span>
                            <br />
                        </a>
                    </asp:View>
                    <!-- 　メモ　 -->
                    <asp:View id="RF_VIEW2" runat="server" >
                        <a id="RF_RIGHTBOX_MEMO">
                            <span id="RF_MEMOTITLE" style="position:relative;left:1em;top:2em;" onchange="MEMOChange()">
                            <asp:TextBox ID="RF_MEMO" runat="server" Width="28.4em" Height="16.9em" CssClass="WF_MEMO" TextMode="MultiLine"></asp:TextBox>
                            </span><br />
                        </a>
                    </asp:View>
                </asp:MultiView>
            </div>
            <div id="RF_REPORT_LIST">
                <span style="position:relative;left:1em;top:2.0em;">印刷・インポート設定</span><br />

                <span style="position:relative;left:1em;top:2.0em;">
                    <asp:ListBox ID="RF_REPORTID" runat="server" Width="28.4em" Height="15em" style="border: 2px solid blue;background-color: #ccffff;"></asp:ListBox>
                </span>
            </div>
            
            <div id="RF_HIDDEN_LIS">
                <asp:HiddenField ID="RF_COMPCODE" runat="server" />
                <asp:HiddenField ID="RF_MAPID_REPORT" runat="server" />
                <asp:HiddenField ID="RF_MAPID_MEMO" runat="server" />
                <asp:HiddenField ID="RF_PROFID" runat="server" />
                <asp:HiddenField ID="RF_MAPVARI" runat="server" />
                <asp:HiddenField ID="RF_TARGETDATE" runat="server" />
            </div>
        </div>

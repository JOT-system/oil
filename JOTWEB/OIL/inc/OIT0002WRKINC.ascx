<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0002WRKINC.ascx.vb" Inherits="JOTWEB.OIT0002WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>              <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_INCLUDUSED" runat="server"></asp:TextBox>       <!-- 利用済含む -->

    <asp:TextBox ID="WF_SEL_SEARCH_BTRAINNO" runat="server"></asp:TextBox>   <!-- 返送列車番号 -->
    <asp:TextBox ID="WF_SEL_SEARCH_BTRAINNAME" runat="server"></asp:TextBox> <!-- 返送列車名 -->
    <asp:TextBox ID="WF_SEL_SEARCH_EMPARRDATE" runat="server"></asp:TextBox> <!-- 空車着日 -->

    <asp:TextBox ID="WF_SEL_SELECT" runat="server"></asp:TextBox>           <!-- ステータス選択 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>          <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>           <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_RLINKNO" runat="server"></asp:TextBox>          <!-- 貨車連結順序表(臨海)№ -->
    <asp:TextBox ID="WF_SEL_LINKNO" runat="server"></asp:TextBox>           <!-- 貨車連結順序表№ -->
    <asp:TextBox ID="WF_SEL_INFO" runat="server"></asp:TextBox>             <!-- 情報 -->
    <asp:TextBox ID="WF_SEL_INFONOW" runat="server"></asp:TextBox>          <!-- 情報名 -->
    <asp:TextBox ID="WF_SEL_BTRAINNO" runat="server"></asp:TextBox>         <!-- 返送列車番号 -->
    <asp:TextBox ID="WF_SEL_BTRAINNAME" runat="server"></asp:TextBox>       <!-- 返送列車名 -->
    <asp:TextBox ID="WF_SEL_OFFICECODE" runat="server"></asp:TextBox>       <!-- 登録営業所コード -->
    <asp:TextBox ID="WF_SEL_OFFICENAME" runat="server"></asp:TextBox>       <!-- 登録営業所名 -->
    <asp:TextBox ID="WF_SEL_DEPSTATION" runat="server"></asp:TextBox>       <!-- 空車発駅（着駅）コード -->
    <asp:TextBox ID="WF_SEL_DEPSTATIONNAME" runat="server"></asp:TextBox>   <!-- 空車発駅（着駅）名 -->
    <asp:TextBox ID="WF_SEL_RETSTATION" runat="server"></asp:TextBox>       <!-- 空車着駅（発駅）コード -->
    <asp:TextBox ID="WF_SEL_RETSTATIONNAME" runat="server"></asp:TextBox>   <!-- 空車着駅（発駅）名 -->
    <asp:TextBox ID="WF_SEL_EMPARRDATE" runat="server"></asp:TextBox>       <!-- 空車着日（予定） -->
    <asp:TextBox ID="WF_SEL_ACTUALEMPARRDATE" runat="server"></asp:TextBox> <!-- 空車着日（実績） -->

    <!-- タンク車合計 -->
    <asp:TextBox ID="WF_SEL_TANKCARTOTAL" runat="server"></asp:TextBox>
    <!-- ハイオク(タンク車数) -->
    <asp:TextBox ID="WF_SEL_HIGHOCTANE_TANKCAR" runat="server"></asp:TextBox>
    <!-- レギュラー(タンク車数) -->
    <asp:TextBox ID="WF_SEL_REGULAR_TANKCAR" runat="server"></asp:TextBox>
    <!-- 灯油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_KEROSENE_TANKCAR" runat="server"></asp:TextBox>
    <!-- 未添加灯油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_NOTADDED_KEROSENE_TANKCAR" runat="server"></asp:TextBox>
    <!-- 軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- 3号軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_NUM3DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- 5号軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_NUM5DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- 10号軽油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_NUM10DIESEL_TANKCAR" runat="server"></asp:TextBox>
    <!-- LSA(タンク車数) -->
    <asp:TextBox ID="WF_SEL_LSA_TANKCAR" runat="server"></asp:TextBox>
    <!-- A重油(タンク車数) -->
    <asp:TextBox ID="WF_SEL_AHEAVY_TANKCAR" runat="server"></asp:TextBox>

    <!-- 作成フラグ -->
    <asp:TextBox ID="WF_SEL_CREATEFLG" runat="server"></asp:TextBox>        
    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>           

    <!-- パネルロック・解除切替用フラグ -->
    <input id="WF_SEL_PANEL" runat="server" value="" type="text" />

</div>

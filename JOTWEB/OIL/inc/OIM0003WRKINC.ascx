<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0003WRKINC.ascx.vb" Inherits="JOTWEB.OIM0003WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>             <!-- 会社コード -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_OFFICECODE" runat="server"></asp:TextBox>           <!-- 営業所コード -->
    <asp:TextBox ID="WF_SEL_SHIPPERCODE" runat="server"></asp:TextBox>          <!-- 荷主コード -->
    <asp:TextBox ID="WF_SEL_PLANTCODE" runat="server"></asp:TextBox>            <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_BIGOILCODE" runat="server"></asp:TextBox>           <!-- 油種大分類コード -->
    <asp:TextBox ID="WF_SEL_MIDDLEOILCODE" runat="server"></asp:TextBox>        <!-- 油種中分類コード -->
    <asp:TextBox ID="WF_SEL_OILCODE" runat="server"></asp:TextBox>              <!-- 油種コード -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>               <!-- 削除フラグ -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>              <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_OFFICECODE2" runat="server"></asp:TextBox>          <!-- 営業所コード -->
    <asp:TextBox ID="WF_SEL_SHIPPERCODE2" runat="server"></asp:TextBox>         <!-- 荷主コード -->
    <asp:TextBox ID="WF_SEL_PLANTCODE2" runat="server"></asp:TextBox>           <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_BIGOILCODE2" runat="server"></asp:TextBox>          <!-- 油種大分類コード -->
    <asp:TextBox ID="WF_SEL_BIGOILNAME" runat="server"></asp:TextBox>           <!-- 油種大分類名 -->
    <asp:TextBox ID="WF_SEL_BIGOILKANA" runat="server"></asp:TextBox>           <!-- 油種大分類名カナ -->
    <asp:TextBox ID="WF_SEL_MIDDLEOILCODE2" runat="server"></asp:TextBox>       <!-- 油種中分類コード -->
    <asp:TextBox ID="WF_SEL_MIDDLEOILNAME" runat="server"></asp:TextBox>        <!-- 油種中分類名 -->
    <asp:TextBox ID="WF_SEL_MIDDLEOILKANA" runat="server"></asp:TextBox>        <!-- 油種中分類名カナ -->
    <asp:TextBox ID="WF_SEL_OILCODE2" runat="server"></asp:TextBox>             <!-- 油種コード -->
    <asp:TextBox ID="WF_SEL_OILNAME" runat="server"></asp:TextBox>              <!-- 油種名 -->
    <asp:TextBox ID="WF_SEL_OILKANA" runat="server"></asp:TextBox>              <!-- 油種名カナ -->
    <asp:TextBox ID="WF_SEL_SEGMENTOILCODE" runat="server"></asp:TextBox>       <!-- 油種細分コード -->
    <asp:TextBox ID="WF_SEL_SEGMENTOILNAME" runat="server"></asp:TextBox>       <!-- 油種名（細分） -->
    <asp:TextBox ID="WF_SEL_OTOILCODE" runat="server"></asp:TextBox>            <!-- OT油種コード -->
    <asp:TextBox ID="WF_SEL_OTOILNAME" runat="server"></asp:TextBox>            <!-- OT油種名 -->
    <asp:TextBox ID="WF_SEL_SHIPPEROILCODE" runat="server"></asp:TextBox>       <!-- 荷主油種コード -->
    <asp:TextBox ID="WF_SEL_SHIPPEROILNAME" runat="server"></asp:TextBox>       <!-- 荷主油種名 -->
    <asp:TextBox ID="WF_SEL_CHECKOILCODE" runat="server"></asp:TextBox>         <!-- 積込チェック用油種コード -->
    <asp:TextBox ID="WF_SEL_CHECKOILNAME" runat="server"></asp:TextBox>         <!-- 積込チェック用油種名 -->
    <asp:TextBox ID="WF_SEL_STOCKFLG" runat="server"></asp:TextBox>             <!-- 在庫管理対象フラグ -->
    <asp:TextBox ID="WF_SEL_ORDERFROMDATE" runat="server"></asp:TextBox>        <!-- 受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_ORDERTODATE" runat="server"></asp:TextBox>          <!-- 受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_DELFLG2" runat="server"></asp:TextBox>              <!-- 削除フラグ -->
    <!-- 品種出荷期間マスタ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_01" runat="server"></asp:TextBox> <!-- 品種出荷期間01.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_01" runat="server"></asp:TextBox> <!-- 品種出荷期間01.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_01" runat="server"></asp:TextBox> <!-- 品種出荷期間01.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_01" runat="server"></asp:TextBox>   <!-- 品種出荷期間01.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_01" runat="server"></asp:TextBox>        <!-- 品種出荷期間01.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_02" runat="server"></asp:TextBox> <!-- 品種出荷期間02.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_02" runat="server"></asp:TextBox> <!-- 品種出荷期間02.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_02" runat="server"></asp:TextBox> <!-- 品種出荷期間02.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_02" runat="server"></asp:TextBox>   <!-- 品種出荷期間02.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_02" runat="server"></asp:TextBox>        <!-- 品種出荷期間02.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_03" runat="server"></asp:TextBox> <!-- 品種出荷期間03.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_03" runat="server"></asp:TextBox> <!-- 品種出荷期間03.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_03" runat="server"></asp:TextBox> <!-- 品種出荷期間03.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_03" runat="server"></asp:TextBox>   <!-- 品種出荷期間03.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_03" runat="server"></asp:TextBox>        <!-- 品種出荷期間03.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_04" runat="server"></asp:TextBox> <!-- 品種出荷期間04.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_04" runat="server"></asp:TextBox> <!-- 品種出荷期間04.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_04" runat="server"></asp:TextBox> <!-- 品種出荷期間04.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_04" runat="server"></asp:TextBox>   <!-- 品種出荷期間04.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_04" runat="server"></asp:TextBox>        <!-- 品種出荷期間04.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_05" runat="server"></asp:TextBox> <!-- 品種出荷期間05.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_05" runat="server"></asp:TextBox> <!-- 品種出荷期間05.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_05" runat="server"></asp:TextBox> <!-- 品種出荷期間05.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_05" runat="server"></asp:TextBox>   <!-- 品種出荷期間05.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_05" runat="server"></asp:TextBox>        <!-- 品種出荷期間05.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_06" runat="server"></asp:TextBox> <!-- 品種出荷期間06.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_06" runat="server"></asp:TextBox> <!-- 品種出荷期間06.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_06" runat="server"></asp:TextBox> <!-- 品種出荷期間06.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_06" runat="server"></asp:TextBox>   <!-- 品種出荷期間06.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_06" runat="server"></asp:TextBox>        <!-- 品種出荷期間06.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_07" runat="server"></asp:TextBox> <!-- 品種出荷期間07.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_07" runat="server"></asp:TextBox> <!-- 品種出荷期間07.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_07" runat="server"></asp:TextBox> <!-- 品種出荷期間07.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_07" runat="server"></asp:TextBox>   <!-- 品種出荷期間07.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_07" runat="server"></asp:TextBox>        <!-- 品種出荷期間07.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_08" runat="server"></asp:TextBox> <!-- 品種出荷期間08.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_08" runat="server"></asp:TextBox> <!-- 品種出荷期間08.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_08" runat="server"></asp:TextBox> <!-- 品種出荷期間08.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_08" runat="server"></asp:TextBox>   <!-- 品種出荷期間08.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_08" runat="server"></asp:TextBox>        <!-- 品種出荷期間08.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_09" runat="server"></asp:TextBox> <!-- 品種出荷期間09.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_09" runat="server"></asp:TextBox> <!-- 品種出荷期間09.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_09" runat="server"></asp:TextBox> <!-- 品種出荷期間09.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_09" runat="server"></asp:TextBox>   <!-- 品種出荷期間09.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_09" runat="server"></asp:TextBox>        <!-- 品種出荷期間09.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_10" runat="server"></asp:TextBox> <!-- 品種出荷期間10.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_10" runat="server"></asp:TextBox> <!-- 品種出荷期間10.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_10" runat="server"></asp:TextBox> <!-- 品種出荷期間10.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_10" runat="server"></asp:TextBox>   <!-- 品種出荷期間10.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_10" runat="server"></asp:TextBox>        <!-- 品種出荷期間10.削除フラグ -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEECODE_11" runat="server"></asp:TextBox> <!-- 品種出荷期間11.荷受人コード -->
    <asp:TextBox ID="WF_SEL_OILTERM_CONSIGNEENAME_11" runat="server"></asp:TextBox> <!-- 品種出荷期間11.荷受人名 -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERFROMDATE_11" runat="server"></asp:TextBox> <!-- 品種出荷期間11.受注登録可能期間FROM -->
    <asp:TextBox ID="WF_SEL_OILTERM_ORDERTODATE_11" runat="server"></asp:TextBox>   <!-- 品種出荷期間11.受注登録可能期間TO -->
    <asp:TextBox ID="WF_SEL_OILTERM_DELFLG_11" runat="server"></asp:TextBox>        <!-- 品種出荷期間11.削除フラグ -->

    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>               <!-- 更新データ(退避用) -->

    <!-- DB更新メッセージ表示用 -->
    <asp:TextBox ID="WF_SEL_DBUPDATE_MESSAGE" runat="server"></asp:TextBox>     <!-- DB更新メッセージ -->
</div>

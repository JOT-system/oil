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
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>               <!-- 更新データ(退避用) -->

</div>

﻿<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0021WRKINC.ascx.vb" Inherits="JOTWEB.OIM0021WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_OFFICECODE" runat="server"></asp:TextBox>               <!-- 管轄営業所コード -->
    <asp:TextBox ID="WF_SEL_FROMYMD" runat="server"></asp:TextBox>                  <!-- 適用開始年月日 -->
    <asp:TextBox ID="WF_SEL_TOYMD" runat="server"></asp:TextBox>                    <!-- 適用終了年月日 -->
    <asp:TextBox ID="WF_SEL_LOAD" runat="server"></asp:TextBox>                     <!-- 荷重 -->
    <asp:TextBox ID="WF_SEL_OILCODE" runat="server"></asp:TextBox>                  <!-- 油種コード -->
    <asp:TextBox ID="WF_SEL_SEGMENTOILCODE" runat="server"></asp:TextBox>           <!-- 油種細分コード -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <asp:TextBox ID="WF_SEL_OFFICECODE2" runat="server"></asp:TextBox>              <!-- 管轄営業所コード -->
    <asp:TextBox ID="WF_SEL_FROMYMD2" runat="server"></asp:TextBox>                 <!-- 適用開始年月日 -->
    <asp:TextBox ID="WF_SEL_TOYMD2" runat="server"></asp:TextBox>                   <!-- 適用終了年月日 -->
    <asp:TextBox ID="WF_SEL_MODEL" runat="server"></asp:TextBox>                    <!-- 型式 -->
    <asp:TextBox ID="WF_SEL_LOAD2" runat="server"></asp:TextBox>                    <!-- 荷重 -->
    <asp:TextBox ID="WF_SEL_OILCODE2" runat="server"></asp:TextBox>                 <!-- 油種コード -->
    <asp:TextBox ID="WF_SEL_SEGMENTOILCODE2" runat="server"></asp:TextBox>          <!-- 油種細分コード -->
    <asp:TextBox ID="WF_SEL_RESERVEDQUANTITY" runat="server"></asp:TextBox>         <!-- 予約数量 -->

    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                  <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                 <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>               <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                   <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                  <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>               <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_UPDTIMSTP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->

    <!-- 詳細画面更新 -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>
</div>
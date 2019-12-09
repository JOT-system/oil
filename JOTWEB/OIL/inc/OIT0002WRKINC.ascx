<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0002WRKINC.ascx.vb" Inherits="JOTWEB.OIT0002WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>         <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_INCLUDUSED" runat="server"></asp:TextBox>         <!-- 利用済含む -->
    <asp:TextBox ID="WF_SEL_DEPSTATION" runat="server"></asp:TextBox>         <!-- 空車発駅コード -->
    <asp:TextBox ID="WF_SEL_DEPSTATION2" runat="server"></asp:TextBox>         <!-- 空車発駅コード2 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>         <!-- 開始年月日 -->
    <asp:TextBox ID="WF_SEL_STYMD2" runat="server"></asp:TextBox>         <!-- 開始年月日2 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>         <!-- 終了年月日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD2" runat="server"></asp:TextBox>         <!-- 終了年月日2 -->
    <asp:TextBox ID="WF_SEL_TRAINNO" runat="server"></asp:TextBox>         <!-- 本線列車 -->
    <asp:TextBox ID="WF_SEL_TRAINNO2" runat="server"></asp:TextBox>         <!-- 本線列車2 -->
    <asp:TextBox ID="WF_SEL_SELECT" runat="server"></asp:TextBox>           <!-- ステータス選択 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>        <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>         <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_CREATEFLG" runat="server"></asp:TextBox>         <!-- 作成フラグ -->
    <asp:TextBox ID="WF_SEL_LINKNO" runat="server"></asp:TextBox>         <!-- 貨車連結順序表№ -->
    <asp:TextBox ID="WF_SEL_LINKDETAILNO" runat="server"></asp:TextBox>         <!-- 貨車連結順序表明細№ -->
    <asp:TextBox ID="WF_SEL_STATUS" runat="server"></asp:TextBox>         <!-- ステータス -->
    <asp:TextBox ID="WF_SEL_INFO" runat="server"></asp:TextBox>         <!-- 情報 -->
    <asp:TextBox ID="WF_SEL_PREORDERNO" runat="server"></asp:TextBox>         <!-- 前回オーダー№ -->
    <asp:TextBox ID="WF_SEL_OFFICECODE" runat="server"></asp:TextBox>         <!-- 登録営業所コード -->
    <asp:TextBox ID="WF_SEL_DEPSTATIONNAME" runat="server"></asp:TextBox>         <!-- 空車発駅名 -->
    <asp:TextBox ID="WF_SEL_RETSTATION" runat="server"></asp:TextBox>         <!-- 空車着駅コード -->
    <asp:TextBox ID="WF_SEL_RETSTATIONNAME" runat="server"></asp:TextBox>         <!-- 空車着駅名 -->
    <asp:TextBox ID="WF_SEL_EMPARRDATE" runat="server"></asp:TextBox>         <!-- 空車着日（予定） -->
    <asp:TextBox ID="WF_SEL_ACTUALEMPARRDATE" runat="server"></asp:TextBox>         <!-- 空車着日（実績） -->
    <asp:TextBox ID="WF_SEL_LINETRAINNO" runat="server"></asp:TextBox>         <!-- 入線列車番号 -->
    <asp:TextBox ID="WF_SEL_LINEORDER" runat="server"></asp:TextBox>         <!-- 入線順 -->
    <asp:TextBox ID="WF_SEL_TANKNUMBER" runat="server"></asp:TextBox>         <!-- タンク車№ -->
    <asp:TextBox ID="WF_SEL_PREOILCODE" runat="server"></asp:TextBox>         <!-- 前回油種 -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>         <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>         <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>         <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>         <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>         <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>         <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>         <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>         <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>        <!-- 更新データ(退避用) -->
</div>

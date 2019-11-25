<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIT0002WRKINC.ascx.vb" Inherits="JOTWEB.OIT0002WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE2" runat="server"></asp:TextBox>         <!-- 会社コード2 -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>              <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_ORG2" runat="server"></asp:TextBox>              <!-- 組織コード2 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>         <!-- 開始年月日 -->
    <asp:TextBox ID="WF_SEL_STYMD2" runat="server"></asp:TextBox>         <!-- 開始年月日2 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>         <!-- 終了年月日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD2" runat="server"></asp:TextBox>         <!-- 終了年月日2 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>        <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>         <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_USERID" runat="server"></asp:TextBox>         <!-- ユーザID -->
    <asp:TextBox ID="WF_SEL_STAFFNAMES" runat="server"></asp:TextBox>         <!-- 社員名（短） -->
    <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>         <!-- 社員名（短） -->
    <asp:TextBox ID="WF_SEL_STAFFNAMEL" runat="server"></asp:TextBox>         <!-- 社員名（長） -->
    <asp:TextBox ID="WF_SEL_EMAIL" runat="server"></asp:TextBox>         <!-- メールアドレス -->
    <asp:TextBox ID="WF_SEL_MENUROLE" runat="server"></asp:TextBox>         <!-- メニュー表示制御ロール -->
    <asp:TextBox ID="WF_SEL_MAPROLE" runat="server"></asp:TextBox>         <!-- 画面参照更新制御ロール -->
    <asp:TextBox ID="WF_SEL_VIEWPROFID" runat="server"></asp:TextBox>         <!-- 画面表示項目制御ロール -->
    <asp:TextBox ID="WF_SEL_RPRTPROFID" runat="server"></asp:TextBox>         <!-- エクセル出力制御ロール -->
    <asp:TextBox ID="WF_SEL_MAPID" runat="server"></asp:TextBox>         <!-- 画面ＩＤ -->
    <asp:TextBox ID="WF_SEL_VARIANT" runat="server"></asp:TextBox>         <!-- 画面初期値ロール -->
    <asp:TextBox ID="WF_SEL_APPROVALID" runat="server"></asp:TextBox>         <!-- 承認権限ロール -->
    <asp:TextBox ID="WF_SEL_PASSWORD" runat="server"></asp:TextBox>         <!-- パスワード -->
    <asp:TextBox ID="WF_SEL_MISSCNT" runat="server"></asp:TextBox>         <!-- 誤り回数 -->
    <asp:TextBox ID="WF_SEL_PASSENDYMD" runat="server"></asp:TextBox>         <!-- パスワード有効期限 -->
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

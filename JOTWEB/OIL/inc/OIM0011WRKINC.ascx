<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0011WRKINC.ascx.vb" Inherits="JOTWEB.OIM0011WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>                      <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                 <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>                    <!-- 開始年月日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>                   <!-- 終了年月日 -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_TORICODE2" runat="server"></asp:TextBox>                <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_STYMD2" runat="server"></asp:TextBox>                   <!-- 開始年月日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD2" runat="server"></asp:TextBox>                  <!-- 終了年月日 -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>                 <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_TORINAMES" runat="server"></asp:TextBox>                <!-- 取引先略称 -->
    <asp:TextBox ID="WF_SEL_TORINAMEKANA" runat="server"></asp:TextBox>             <!-- 取引先カナ名称 -->
    <asp:TextBox ID="WF_SEL_DEPTNAME" runat="server"></asp:TextBox>                 <!-- 部門名称 -->
    <asp:TextBox ID="WF_SEL_POSTNUM1" runat="server"></asp:TextBox>                 <!-- 郵便番号（上） -->
    <asp:TextBox ID="WF_SEL_POSTNUM2" runat="server"></asp:TextBox>                 <!-- 郵便番号（下） -->
    <asp:TextBox ID="WF_SEL_ADDR1" runat="server"></asp:TextBox>                    <!-- 住所１ -->
    <asp:TextBox ID="WF_SEL_ADDR2" runat="server"></asp:TextBox>                    <!-- 住所２ -->
    <asp:TextBox ID="WF_SEL_ADDR3" runat="server"></asp:TextBox>                    <!-- 住所３ -->
    <asp:TextBox ID="WF_SEL_ADDR4" runat="server"></asp:TextBox>                    <!-- 住所４ -->
    <asp:TextBox ID="WF_SEL_TEL" runat="server"></asp:TextBox>                      <!-- 電話番号 -->
    <asp:TextBox ID="WF_SEL_FAX" runat="server"></asp:TextBox>                      <!-- ＦＡＸ番号 -->
    <asp:TextBox ID="WF_SEL_MAIL" runat="server"></asp:TextBox>                     <!-- メールアドレス -->
    <asp:TextBox ID="WF_SEL_OILUSEFLG" runat="server"></asp:TextBox>                <!-- 石油利用フラグ -->

    <asp:TextBox ID="WF_SEL_INVOICEBANKOUTSIDECODE" runat="server"></asp:TextBox>   <!-- 請求先銀行外部コード -->
    <asp:TextBox ID="WF_SEL_PAYEEBANKOUTSIDECODE" runat="server"></asp:TextBox>     <!-- 支払先銀行外部コード -->

    <asp:TextBox ID="WF_SEL_BANKCODE" runat="server"></asp:TextBox>                 <!-- 銀行コード -->
    <asp:TextBox ID="WF_SEL_BANKBRANCHCODE" runat="server"></asp:TextBox>           <!-- 支店コード -->
    <asp:TextBox ID="WF_SEL_ACCOUNTTYPE" runat="server"></asp:TextBox>              <!-- 口座種別 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTNUMBER" runat="server"></asp:TextBox>            <!-- 口座番号 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTNAME" runat="server"></asp:TextBox>              <!-- 口座名義 -->

    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                  <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                 <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>               <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                   <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                  <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>               <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->

    <!-- 詳細画面更新 -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>
</div>

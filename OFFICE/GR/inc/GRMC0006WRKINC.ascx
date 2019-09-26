<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRMC0006WRKINC.ascx.vb" Inherits="OFFICE.GRMC0006WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>         <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>            <!-- 有効年月日(From) -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>           <!-- 有効年月日(To) -->
    <asp:TextBox ID="WF_SEL_TORICODEF" runat="server"></asp:TextBox>        <!-- 取引先(From) -->
    <asp:TextBox ID="WF_SEL_TORICODET" runat="server"></asp:TextBox>        <!-- 取引先(To) -->
    <asp:TextBox ID="WF_SEL_TODOKECODE" runat="server"></asp:TextBox>       <!-- 届先 -->
    <asp:TextBox ID="WF_SEL_TODOKENAME" runat="server"></asp:TextBox>       <!-- 届先名称 -->
    <asp:TextBox ID="WF_SEL_POSTNUM" runat="server"></asp:TextBox>          <!-- 郵便番号 -->
    <asp:TextBox ID="WF_SEL_ADDR" runat="server"></asp:TextBox>             <!-- 住所 -->
    <asp:TextBox ID="WF_SEL_TEL" runat="server"></asp:TextBox>              <!-- 電話番号 -->
    <asp:TextBox ID="WF_SEL_FAX" runat="server"></asp:TextBox>              <!-- FAX番号 -->
    <asp:TextBox ID="WF_SEL_CITIES" runat="server"></asp:TextBox>           <!-- 市町村コード -->
    <asp:TextBox ID="WF_SEL_CLASS" runat="server"></asp:TextBox>            <!-- 分類 -->
    <asp:TextBox ID="WF_SEL_TORIRADIO" runat="server"></asp:TextBox>        <!-- 取引先(JX,COSMO) -->
    <asp:TextBox ID="WF_SEL_PREFECTURES1" runat="server"></asp:TextBox>     <!-- 都道府県１ -->
    <asp:TextBox ID="WF_SEL_PREFECTURES2" runat="server"></asp:TextBox>     <!-- 都道府県２ -->
    <asp:TextBox ID="WF_SEL_PREFECTURES3" runat="server"></asp:TextBox>     <!-- 都道府県３ -->
    <asp:TextBox ID="WF_SEL_PREFECTURES4" runat="server"></asp:TextBox>     <!-- 都道府県４ -->
    <asp:TextBox ID="WF_SEL_PREFECTURES5" runat="server"></asp:TextBox>     <!-- 都道府県５ -->
</div>

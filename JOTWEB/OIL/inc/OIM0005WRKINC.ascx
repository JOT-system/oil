<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="OIM0005WRKINC.ascx.vb" Inherits="JOTWEB.OIM0005WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>                      <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_TANKNUMBER" runat="server"></asp:TextBox>               <!-- JOT車番 -->
    <asp:TextBox ID="WF_SEL_MODEL" runat="server"></asp:TextBox>                    <!-- 形式 -->
    <asp:TextBox ID="WF_SEL_USEDFLG" runat="server"></asp:TextBox>                  <!-- 利用フラグ -->
    <asp:TextBox ID="WF_SEL_LENGTHFLG" runat="server"></asp:TextBox>                <!-- 長さフラグ -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_TANKNUMBER2" runat="server"></asp:TextBox>              <!-- JOT車番 -->
    <asp:TextBox ID="WF_SEL_MODEL2" runat="server"></asp:TextBox>                   <!-- 形式 -->
    <asp:TextBox ID="WF_SEL_MODELKANA" runat="server"></asp:TextBox>                <!-- 形式カナ -->
    <asp:TextBox ID="WF_SEL_LOAD" runat="server"></asp:TextBox>                     <!-- 荷重 -->
    <asp:TextBox ID="WF_SEL_LOADUNIT" runat="server"></asp:TextBox>                 <!-- 荷重単位 -->
    <asp:TextBox ID="WF_SEL_VOLUME" runat="server"></asp:TextBox>                   <!-- 容積 -->
    <asp:TextBox ID="WF_SEL_VOLUMEUNIT" runat="server"></asp:TextBox>               <!-- 容積単位 -->
    <asp:TextBox ID="WF_SEL_MYWEIGHT" runat="server"></asp:TextBox>                 <!-- 自重 -->
    <asp:TextBox ID="WF_SEL_LENGTH" runat="server"></asp:TextBox>                   <!-- タンク車長 -->
    <asp:TextBox ID="WF_SEL_TANKLENGTH" runat="server"></asp:TextBox>               <!-- タンク車体長 -->
    <asp:TextBox ID="WF_SEL_MAXCALIBER" runat="server"></asp:TextBox>               <!-- 最大口径 -->
    <asp:TextBox ID="WF_SEL_MINCALIBER" runat="server"></asp:TextBox>               <!-- 最大口径 -->
    <asp:TextBox ID="WF_SEL_LENGTHFLG2" runat="server"></asp:TextBox>                <!-- 長さフラグ -->
    <asp:TextBox ID="WF_SEL_ORIGINOWNERCODE" runat="server"></asp:TextBox>          <!-- 原籍所有者C -->
    <asp:TextBox ID="WF_SEL_ORIGINOWNERNAME" runat="server"></asp:TextBox>          <!-- 原籍所有者 -->
    <asp:TextBox ID="WF_SEL_OWNERCODE" runat="server"></asp:TextBox>                <!-- 名義所有者C -->
    <asp:TextBox ID="WF_SEL_OWNERNAME" runat="server"></asp:TextBox>                <!-- 名義所有者 -->
    <asp:TextBox ID="WF_SEL_LEASECODE" runat="server"></asp:TextBox>                <!-- リース先C -->
    <asp:TextBox ID="WF_SEL_LEASENAME" runat="server"></asp:TextBox>                <!-- リース先 -->
    <asp:TextBox ID="WF_SEL_LEASECLASS" runat="server"></asp:TextBox>               <!-- リース区分C -->
    <asp:TextBox ID="WF_SEL_LEASECLASSNEMAE" runat="server"></asp:TextBox>          <!-- リース区分 -->
    <asp:TextBox ID="WF_SEL_AUTOEXTENTION" runat="server"></asp:TextBox>            <!-- 自動延長 -->
    <asp:TextBox ID="WF_SEL_AUTOEXTENTIONNAME" runat="server"></asp:TextBox>        <!-- 自動延長名 -->
    <asp:TextBox ID="WF_SEL_LEASESTYMD" runat="server"></asp:TextBox>               <!-- リース開始年月日 -->
    <asp:TextBox ID="WF_SEL_LEASEENDYMD" runat="server"></asp:TextBox>              <!-- リース満了年月日 -->
    <asp:TextBox ID="WF_SEL_USERCODE" runat="server"></asp:TextBox>                 <!-- 第三者使用者C -->
    <asp:TextBox ID="WF_SEL_USERNAME" runat="server"></asp:TextBox>                 <!-- 第三者使用者 -->
    <asp:TextBox ID="WF_SEL_CURRENTSTATIONCODE" runat="server"></asp:TextBox>       <!-- 原常備駅C -->
    <asp:TextBox ID="WF_SEL_CURRENTSTATIONNAME" runat="server"></asp:TextBox>       <!-- 原常備駅 -->
    <asp:TextBox ID="WF_SEL_EXTRADINARYSTATIONCODE" runat="server"></asp:TextBox>   <!-- 臨時常備駅C -->
    <asp:TextBox ID="WF_SEL_EXTRADINARYSTATIONNAME" runat="server"></asp:TextBox>   <!-- 臨時常備駅 -->
    <asp:TextBox ID="WF_SEL_USERLIMIT" runat="server"></asp:TextBox>                <!-- 第三者使用期限 -->
    <asp:TextBox ID="WF_SEL_LIMITTEXTRADIARYSTATION" runat="server"></asp:TextBox>  <!-- 臨時常備駅期限 -->
    <asp:TextBox ID="WF_SEL_DEDICATETYPECODE" runat="server"></asp:TextBox>         <!-- 原専用種別C -->
    <asp:TextBox ID="WF_SEL_DEDICATETYPENAME" runat="server"></asp:TextBox>         <!-- 原専用種別 -->
    <asp:TextBox ID="WF_SEL_EXTRADINARYTYPECODE" runat="server"></asp:TextBox>      <!-- 臨時専用種別C -->
    <asp:TextBox ID="WF_SEL_EXTRADINARYTYPENAME" runat="server"></asp:TextBox>      <!-- 臨時専用種別 -->
    <asp:TextBox ID="WF_SEL_EXTRADINARYLIMIT" runat="server"></asp:TextBox>         <!-- 臨時専用期限 -->
    <asp:TextBox ID="WF_SEL_BIGOILCODE" runat="server"></asp:TextBox>               <!-- 油種大分類コード -->
    <asp:TextBox ID="WF_SEL_BIGOILNAME" runat="server"></asp:TextBox>               <!-- 油種大分類名 -->

    <asp:TextBox ID="WF_SEL_MIDDLEOILCODE" runat="server"></asp:TextBox>            <!-- 油種中分類コード -->
    <asp:TextBox ID="WF_SEL_MIDDLEOILNAME" runat="server"></asp:TextBox>            <!-- 油種中分類名 -->

    <asp:TextBox ID="WF_SEL_OPERATIONBASECODE" runat="server"></asp:TextBox>        <!-- 運用基地C -->
    <asp:TextBox ID="WF_SEL_OPERATIONBASENAME" runat="server"></asp:TextBox>        <!-- 運用場所 -->
    <asp:TextBox ID="WF_SEL_COLORCODE" runat="server"></asp:TextBox>                <!-- 塗色C -->
    <asp:TextBox ID="WF_SEL_COLORNAME" runat="server"></asp:TextBox>                <!-- 塗色 -->
    <asp:TextBox ID="WF_SEL_MARKCODE" runat="server"></asp:TextBox>                 <!-- マークコード -->
    <asp:TextBox ID="WF_SEL_MARKNAME" runat="server"></asp:TextBox>                 <!-- マーク名 -->
    <asp:TextBox ID="WF_SEL_JXTGTAGCODE1" runat="server"></asp:TextBox>             <!-- JXTG仙台タグコード -->
    <asp:TextBox ID="WF_SEL_JXTGTAGNAME1" runat="server"></asp:TextBox>             <!-- JXTG仙台タグ名 -->
    <asp:TextBox ID="WF_SEL_JXTGTAGCODE2" runat="server"></asp:TextBox>             <!-- JXTG千葉タグコード -->
    <asp:TextBox ID="WF_SEL_JXTGTAGNAME2" runat="server"></asp:TextBox>             <!-- JXTG千葉タグ名 -->
    <asp:TextBox ID="WF_SEL_JXTGTAGCODE3" runat="server"></asp:TextBox>             <!-- JXTG川崎タグコード -->
    <asp:TextBox ID="WF_SEL_JXTGTAGNAME3" runat="server"></asp:TextBox>             <!-- JXTG川崎タグ名 -->
    <asp:TextBox ID="WF_SEL_JXTGTAGCODE4" runat="server"></asp:TextBox>             <!-- JXTG根岸タグコード -->
    <asp:TextBox ID="WF_SEL_JXTGTAGNAME4" runat="server"></asp:TextBox>             <!-- JXTG根岸タグ名 -->
    <asp:TextBox ID="WF_SEL_IDSSTAGCODE" runat="server"></asp:TextBox>              <!-- 出光昭シタグコード -->
    <asp:TextBox ID="WF_SEL_IDSSTAGNAME" runat="server"></asp:TextBox>              <!-- 出光昭シタグ名 -->
    <asp:TextBox ID="WF_SEL_COSMOTAGCODE" runat="server"></asp:TextBox>             <!-- コスモタグコード -->
    <asp:TextBox ID="WF_SEL_COSMOTAGNAME" runat="server"></asp:TextBox>             <!-- コスモタグ名 -->
    <asp:TextBox ID="WF_SEL_RESERVE1" runat="server"></asp:TextBox>                 <!-- 予備1 -->
    <asp:TextBox ID="WF_SEL_RESERVE2" runat="server"></asp:TextBox>                 <!-- 予備2 -->
    <asp:TextBox ID="WF_SEL_JRINSPECTIONDATE" runat="server"></asp:TextBox>         <!-- 次回交検年月日(JR） -->
    <asp:TextBox ID="WF_SEL_INSPECTIONDATE" runat="server"></asp:TextBox>           <!-- 次回交検年月日 -->
    <asp:TextBox ID="WF_SEL_JRSPECIFIEDDATE" runat="server"></asp:TextBox>          <!-- 次回指定年月日(JR) -->
    <asp:TextBox ID="WF_SEL_SPECIFIEDDATE" runat="server"></asp:TextBox>            <!-- 次回指定年月日 -->
    <asp:TextBox ID="WF_SEL_JRALLINSPECTIONDATE" runat="server"></asp:TextBox>      <!-- 次回全検年月日(JR)  -->
    <asp:TextBox ID="WF_SEL_ALLINSPECTIONDATE" runat="server"></asp:TextBox>        <!-- 次回全検年月日 -->
    <asp:TextBox ID="WF_SEL_PREINSPECTIONDATE" runat="server"></asp:TextBox>        <!-- 前回全検年月日 -->
    <asp:TextBox ID="WF_SEL_GETDATE" runat="server"></asp:TextBox>                  <!-- 取得年月日 -->
    <asp:TextBox ID="WF_SEL_TRANSFERDATE" runat="server"></asp:TextBox>             <!-- 車籍編入年月日 -->
    <asp:TextBox ID="WF_SEL_OBTAINEDCODE" runat="server"></asp:TextBox>             <!-- 取得先C -->
    <asp:TextBox ID="WF_SEL_OBTAINEDNAME" runat="server"></asp:TextBox>             <!-- 取得先名 -->
    <asp:TextBox ID="WF_SEL_PROGRESSYEAR" runat="server"></asp:TextBox>             <!-- 現在経年 -->
    <asp:TextBox ID="WF_SEL_NEXTPROGRESSYEAR" runat="server"></asp:TextBox>         <!-- 次回全検時経年 -->
    <asp:TextBox ID="WF_SEL_EXCLUDEDATE" runat="server"></asp:TextBox>              <!-- 車籍除外年月日 -->
    <asp:TextBox ID="WF_SEL_RETIRMENTDATE" runat="server"></asp:TextBox>            <!-- 資産除却年月日 -->
    <asp:TextBox ID="WF_SEL_JRTANKNUMBER" runat="server"></asp:TextBox>             <!-- JR車番 -->
    <asp:TextBox ID="WF_SEL_JRTANKTYPE" runat="server"></asp:TextBox>               <!-- JR車種コード -->
    <asp:TextBox ID="WF_SEL_OLDTANKNUMBER" runat="server"></asp:TextBox>            <!-- 旧JOT車番 -->
    <asp:TextBox ID="WF_SEL_OTTANKNUMBER" runat="server"></asp:TextBox>             <!-- OT車番 -->
    <asp:TextBox ID="WF_SEL_JXTGTANKNUMBER1" runat="server"></asp:TextBox>          <!-- JXTG仙台車番 -->
    <asp:TextBox ID="WF_SEL_JXTGTANKNUMBER2" runat="server"></asp:TextBox>          <!-- JXTG千葉車番 -->
    <asp:TextBox ID="WF_SEL_JXTGTANKNUMBER3" runat="server"></asp:TextBox>          <!-- JXTG川崎車番 -->
    <asp:TextBox ID="WF_SEL_JXTGTANKNUMBER4" runat="server"></asp:TextBox>          <!-- JXTG根岸車番 -->
    <asp:TextBox ID="WF_SEL_COSMOTANKNUMBER" runat="server"></asp:TextBox>          <!-- コスモ車番 -->
    <asp:TextBox ID="WF_SEL_FUJITANKNUMBER" runat="server"></asp:TextBox>           <!-- 富士石油車番 -->
    <asp:TextBox ID="WF_SEL_SHELLTANKNUMBER" runat="server"></asp:TextBox>          <!-- 出光昭シ車番 -->
    <asp:TextBox ID="WF_SEL_SAPSHELLTANKNUMBER" runat="server"></asp:TextBox>       <!-- 出光昭シSAP車番 -->
    <asp:TextBox ID="WF_SEL_RESERVE3" runat="server"></asp:TextBox>                 <!-- 予備 -->
    <asp:TextBox ID="WF_SEL_USEDFLG2" runat="server"></asp:TextBox>                 <!-- 利用フラグ -->

    <asp:TextBox ID="WF_SEL_INTERINSPECTYM" runat="server"></asp:TextBox>           <!-- 中間点検年月 -->
    <asp:TextBox ID="WF_SEL_INTERINSPECTSTATION" runat="server"></asp:TextBox>      <!-- 中間点検場所 -->
    <asp:TextBox ID="WF_SEL_INTERINSPECTORGCODE" runat="server"></asp:TextBox>      <!-- 中間点検実施者 -->
    <asp:TextBox ID="WF_SEL_SELFINSPECTYM" runat="server"></asp:TextBox>            <!-- 自主点検年月 -->
    <asp:TextBox ID="WF_SEL_SELFINSPECTSTATION" runat="server"></asp:TextBox>       <!-- 自主点検場所 -->
    <asp:TextBox ID="WF_SEL_SELFINSPECTORGCODE" runat="server"></asp:TextBox>       <!-- 自主点検実施者 -->

    <asp:TextBox ID="WF_SEL_INSPECTMEMBERNAME" runat="server"></asp:TextBox>        <!-- 点検実施者(社員名) -->

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

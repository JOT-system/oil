'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0003OrderDetail
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0003tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0003INPtbl As DataTable                              'チェック用テーブル
    Private OIT0003UPDtbl As DataTable                              '更新用テーブル
    Private OIT0003WKtbl As DataTable                               '作業用テーブル
    Private OIT0003Fixvaltbl As DataTable                           '作業用テーブル(固定値マスタ取得用)

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 7                  'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部タブID

    'Private Const CONST_DSPROWCOUNT As Integer = 45                '１画面表示対象
    'Private Const CONST_SCROLLROWCOUNT As Integer = 10              'マウススクロール時の増分
    'Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID
    Private Const CONST_MAX_TABID As Integer = 4                    '詳細タブ数

    '〇タンク車割当状況
    Private Const CONST_TANKNO_STATUS_WARI As String = "割当"
    Private Const CONST_TANKNO_STATUS_MIWARI As String = "未割当"
    Private Const CONST_TANKNO_STATUS_FUKA As String = "不可"
    Private Const CONST_TANKNO_STATUS_ZAN As String = "残車"

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0052DetailView As New CS0052DetailView                'Repeterオブジェクト作成

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    'サブ用リターンコード

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0003tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '油種数登録ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonEND"                   '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"               'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_CheckBoxSELECT"              'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_LeftBoxSelectClick"          'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"                   '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                   '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"              '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_ButtonALLSELECT_TAB1",       '全選択ボタン押下
                             "WF_ButtonALLSELECT_TAB2",
                             "WF_ButtonALLSELECT_TAB3",
                             "WF_ButtonALLSELECT_TAB4"
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED_TAB1",   '選択解除ボタン押下
                             "WF_ButtonSELECT_LIFTED_TAB2",
                             "WF_ButtonSELECT_LIFTED_TAB3",
                             "WF_ButtonSELECT_LIFTED_TAB4"
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonLINE_LIFTED_TAB1",     '行削除ボタン押下
                             "WF_ButtonLINE_LIFTED_TAB2",
                             "WF_ButtonLINE_LIFTED_TAB3",
                             "WF_ButtonLINE_LIFTED_TAB4"
                            WF_ButtonLINE_LIFTED_Click()
                        Case "WF_ButtonLINE_ADD_TAB1",        '行追加ボタン押下
                             "WF_ButtonLINE_ADD_TAB2",
                             "WF_ButtonLINE_ADD_TAB3",
                             "WF_ButtonLINE_ADD_TAB4"
                            WF_ButtonLINE_ADD_Click()
                        'Case "WF_ButtonUPDATE"          '割当確定ボタン押下
                        '    WF_ButtonUPDATE_Click()
                        Case "WF_MouseWheelUp"                'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"              'マウスホイール(Down)
                            WF_Grid_Scroll()
                        'Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                        '    WF_FILEUPLOAD()
                        Case "WF_RadioButonClick"             '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"                  '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "WF_ListChange"                  'リスト変更
                            WF_ListChange()
                        Case "WF_DTAB_Click"                  '○DetailTab切替処理
                            WF_Detail_TABChange()
                            'TAB_DisplayCTRL()
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

            '○ 作成モード(１：新規登録, ２：更新)設定
            If work.WF_SEL_CREATEFLG.Text = "1" Then
                WF_CREATEFLG.Value = "1"
            Else
                WF_CREATEFLG.Value = "2"
            End If

            '○ 作成モード(１：貨車連結未使用, ２：貨車連結使用)設定
            If work.WF_SEL_CREATELINKFLG.Text = "1" Then
                WF_CREATELINKFLG.Value = "1"
            Else
                WF_CREATELINKFLG.Value = "2"
            End If
        Finally
            '○ 格納Table Close
            If Not IsNothing(OIT0003tbl) Then
                OIT0003tbl.Clear()
                OIT0003tbl.Dispose()
                OIT0003tbl = Nothing
            End If

            If Not IsNothing(OIT0003INPtbl) Then
                OIT0003INPtbl.Clear()
                OIT0003INPtbl.Dispose()
                OIT0003INPtbl = Nothing
            End If

            If Not IsNothing(OIT0003UPDtbl) Then
                OIT0003UPDtbl.Clear()
                OIT0003UPDtbl.Dispose()
                OIT0003UPDtbl = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0003WRKINC.MAPIDD
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.MENU Then
            'Master.MAPID = OIT0003WRKINC.MAPIDD
            work.WF_SEL_MAPIDBACKUP.Text = OIT0003WRKINC.MAPIDD
        Else
            'Master.MAPID = OIT0003WRKINC.MAPIDD + "MAIN"
            work.WF_SEL_MAPIDBACKUP.Text = OIT0003WRKINC.MAPIDD + "MAIN"
        End If

        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        GridViewInitialize()

        '○ 詳細-画面初期設定
        'Repeater_INIT()
        'WF_DTAB_CHANGE_NO.Value = "0"

        '〇 タブ切替
        WF_Detail_TABChange()

        ''〇 タブ指定時表示判定処理
        'TAB_DisplayCTRL()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        'ステータス
        If work.WF_SEL_ORDERSTATUSNM.Text = "" Then
            work.WF_SEL_ORDERSTATUS.Text = "100"
            work.WF_SEL_ORDERSTATUSNM.Text = "受注受付"
        End If
        TxtOrderStatus.Text = work.WF_SEL_ORDERSTATUSNM.Text

        '情報
        TxtOrderInfo.Text = work.WF_SEL_INFORMATIONNM.Text
        '###################################################
        '受注パターン
        CODENAME_get("ORDERTYPE", work.WF_SEL_PATTERNCODE.Text, work.WF_SEL_PATTERNNAME.Text, WW_DUMMY)
        TxtOrderType.Text = work.WF_SEL_PATTERNNAME.Text
        '###################################################
        'オーダー№
        If work.WF_SEL_ORDERNUMBER.Text = "" Then
            Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
            WW_FixvalueMasterSearch("", "NEWORDERNOGET", "", WW_GetValue)
            work.WF_SEL_ORDERNUMBER.Text = WW_GetValue(0)
            TxtOrderNo.Text = work.WF_SEL_ORDERNUMBER.Text
        Else
            TxtOrderNo.Text = work.WF_SEL_ORDERNUMBER.Text
        End If

        '荷主
        TxtShippersCode.Text = work.WF_SEL_SHIPPERSCODE.Text
        '荷受人
        TxtConsigneeCode.Text = work.WF_SEL_CONSIGNEECODE.Text
        '本線列車
        TxtTrainNo.Text = work.WF_SEL_TRAIN.Text
        '発駅
        TxtDepstationCode.Text = work.WF_SEL_DEPARTURESTATION.Text
        '着駅
        TxtArrstationCode.Text = work.WF_SEL_ARRIVALSTATION.Text

        '(予定)積込日
        TxtLoadingDate.Text = work.WF_SEL_LODDATE.Text
        '(予定)発日
        TxtDepDate.Text = work.WF_SEL_DEPDATE.Text
        '(予定)積車着日
        TxtArrDate.Text = work.WF_SEL_ARRDATE.Text
        '(予定)受入日
        TxtAccDate.Text = work.WF_SEL_ACCDATE.Text
        '(予定)空車着日
        TxtEmparrDate.Text = work.WF_SEL_EMPARRDATE.Text

        '(実績)積込日
        TxtActualLoadingDate.Text = work.WF_SEL_ACTUALLODDATE.Text
        '(実績)発日
        TxtActualDepDate.Text = work.WF_SEL_ACTUALDEPDATE.Text
        '(実績)積車着日
        TxtActualArrDate.Text = work.WF_SEL_ACTUALARRDATE.Text
        '(実績)受入日
        TxtActualAccDate.Text = work.WF_SEL_ACTUALACCDATE.Text
        '(実績)空車着日
        TxtActualEmparrDate.Text = work.WF_SEL_ACTUALEMPARRDATE.Text

        '○ 油種別タンク車数(車)データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_OILTANKCntGet(SQLcon)
        End Using

        '計上月
        If work.WF_SEL_KEIJYOYMD.Text <> "" Then
            Dim dt As DateTime = DateTime.ParseExact(work.WF_SEL_KEIJYOYMD.Text, "yyyy/MM/dd", Nothing)
            TxtBudgetMonth.Text = dt.Year.ToString() + "/" + dt.Month.ToString("00")
        Else
            TxtBudgetMonth.Text = work.WF_SEL_KEIJYOYMD.Text
        End If
        '売上合計金額(税抜)
        TxtTotalSales.Text = work.WF_SEL_SALSE.Text
        '支払合計金額(税抜)
        TxtTitalPayment.Text = work.WF_SEL_PAYMENT.Text
        '売上合計金額(税額)
        TxtTotalSales2.Text = work.WF_SEL_TOTALSALSE.Text
        '支払合計金額(税額)
        TxtTitalPayment2.Text = work.WF_SEL_TOTALPAYMENT.Text

        '● タブ「タンク車割当」
        '　■油種別タンク車数(車)
        'ハイオク(タンク車数)
        TxtHTank.Text = work.WF_SEL_HIGHOCTANE_TANKCAR.Text
        'レギュラー(タンク車数)
        TxtRTank.Text = work.WF_SEL_REGULAR_TANKCAR.Text
        '灯油(タンク車数)
        TxtTTank.Text = work.WF_SEL_KEROSENE_TANKCAR.Text
        '未添加灯油(タンク車数)
        TxtMTTank.Text = work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text
        '軽油(タンク車数)
        TxtKTank.Text = work.WF_SEL_DIESEL_TANKCAR.Text
        '3号軽油(タンク車数)
        TxtK3Tank.Text = work.WF_SEL_NUM3DIESEL_TANKCAR.Text
        '5号軽油(タンク車数)
        TxtK5Tank.Text = work.WF_SEL_NUM5DIESEL_TANKCAR.Text
        '10号軽油(タンク車数)
        TxtK10Tank.Text = work.WF_SEL_NUM10DIESEL_TANKCAR.Text
        'LSA(タンク車数)
        TxtLTank.Text = work.WF_SEL_LSA_TANKCAR.Text
        'A重油(タンク車数)
        TxtATank.Text = work.WF_SEL_AHEAVY_TANKCAR.Text

        '　■割当後　油種別タンク車数(車)
        'ハイオク(タンク車数)
        TxtHTank_w.Text = work.WF_SEL_HIGHOCTANECH_TANKCAR.Text
        'レギュラー(タンク車数)
        TxtRTank_w.Text = work.WF_SEL_REGULARCH_TANKCAR.Text
        '灯油(タンク車数)
        TxtTTank_w.Text = work.WF_SEL_KEROSENECH_TANKCAR.Text
        '未添加灯油(タンク車数)
        TxtMTTank_w.Text = work.WF_SEL_NOTADDED_KEROSENECH_TANKCAR.Text
        '軽油(タンク車数)
        TxtKTank_w.Text = work.WF_SEL_DIESELCH_TANKCAR.Text
        '3号軽油(タンク車数)
        TxtK3Tank_w.Text = work.WF_SEL_NUM3DIESELCH_TANKCAR.Text
        '5号軽油(タンク車数)
        TxtK5Tank_w.Text = work.WF_SEL_NUM5DIESELCH_TANKCAR.Text
        '10号軽油(タンク車数)
        TxtK10Tank_w.Text = work.WF_SEL_NUM10DIESELCH_TANKCAR.Text
        'LSA(タンク車数)
        TxtLTank_w.Text = work.WF_SEL_LSACH_TANKCAR.Text
        'A重油(タンク車数)
        TxtATank_w.Text = work.WF_SEL_AHEAVYCH_TANKCAR.Text

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", work.WF_SEL_UORG.Text, WF_UORG_TEXT.Text, WW_DUMMY)
        '荷主
        CODENAME_get("SHIPPERS", TxtShippersCode.Text, LblShippersName.Text, WW_DUMMY)
        '荷受人
        CODENAME_get("CONSIGNEE", TxtConsigneeCode.Text, LblConsigneeName.Text, WW_DUMMY)
        '発駅
        CODENAME_get("DEPSTATION", TxtDepstationCode.Text, LblDepstationName.Text, WW_DUMMY)
        '着駅
        CODENAME_get("ARRSTATION", TxtArrstationCode.Text, LblArrstationName.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        'If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.MENU Then
        '    '○ 画面表示データ取得
        '    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
        '        SQLcon.Open()       'DataBase接続

        '        MAPDataGet(SQLcon, 0)
        '    End Using
        'Else
        '    work.WF_SEL_CREATEFLG.Text = "1"
        '    work.WF_SEL_CREATELINKFLG.Text = "1"
        'End If

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            '作成フラグ(新規登録：1, 更新：2)
            work.WF_SEL_CREATEFLG.Text = "1"
            '作成フラグ(貨車連結未使用：1, 貨車連結使用：2)
            work.WF_SEL_CREATELINKFLG.Text = "1"

            '○ 画面レイアウト設定
            If Master.VIEWID = "" Then
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "SCREENLAYOUT", Master.MAPID, WW_GetValue)

                Master.VIEWID = WW_GetValue(0)
            End If
        End If

        '〇画面表示設定処理
        WW_ScreenEnabledSet()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, 0)
        End Using

        '貨車連結を使用する場合
        If work.WF_SEL_CREATELINKFLG.Text = "2" Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGetLink(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0003tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea1
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Vertical
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection, ByVal O_INSCNT As Integer)

        If IsNothing(OIT0003tbl) Then
            OIT0003tbl = New DataTable
        End If

        If OIT0003tbl.Columns.Count <> 0 Then
            OIT0003tbl.Columns.Clear()
        End If

        OIT0003tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String = ""
        Dim SQLTempTblStr As String = ""

        SQLTempTblStr =
                  " DELETE FROM OIL.TMP0001ORDER; " _
                & " INSERT INTO OIL.TMP0001ORDER "

        '新規登録ボタン押下
        If work.WF_SEL_CREATEFLG.Text = "1" Then
            SQLStr =
              " SELECT TOP (@P00)" _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , ''                                             AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , @P01                                           AS ORDERNO" _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS DETAILNO" _
            & " , @P12                                           AS SHIPPERSCODE" _
            & " , @P13                                           AS SHIPPERSNAME" _
            & " , @P14                                           AS BASECODE" _
            & " , @P15                                           AS BASENAME" _
            & " , @P16                                           AS CONSIGNEECODE" _
            & " , @P17                                           AS CONSIGNEENAME" _
            & " , ''                                             AS ORDERINFO" _
            & " , ''                                             AS ORDERINFONAME" _
            & " , ''                                             AS OILCODE" _
            & " , ''                                             AS OILNAME" _
            & " , ''                                             AS TANKQUOTA" _
            & " , ''                                             AS LINKNO" _
            & " , ''                                             AS LINKDETAILNO" _
            & " , ''                                             AS LINEORDER" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS LASTOILCODE" _
            & " , ''                                             AS LASTOILNAME" _
            & " , '0'                                            AS DELFLG" _
            & " FROM sys.all_objects "

            '" SELECT TOP (@P0)" _
            'SQLStr &=
            '      " ORDER BY" _
            '    & "    LINECNT"

            '明細データダブルクリック
        ElseIf work.WF_SEL_CREATEFLG.Text = "2" Then
            SQLStr =
                  " SELECT" _
                & "   0                                                  AS LINECNT" _
                & " , ''                                                 AS OPERATION" _
                & " , CAST(OIT0002.UPDTIMSTP AS bigint)                  AS TIMSTP" _
                & " , 1                                                  AS 'SELECT'" _
                & " , 0                                                  AS HIDDEN" _
                & " , ISNULL(RTRIM(OIT0003.ORDERNO), '')                 AS ORDERNO" _
                & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')                AS DETAILNO" _
                & " , ISNULL(RTRIM(OIT0003.SHIPPERSCODE), '')            AS SHIPPERSCODE" _
                & " , ISNULL(RTRIM(OIT0003.SHIPPERSNAME), '')            AS SHIPPERSNAME" _
                & " , ISNULL(RTRIM(OIT0002.BASECODE), '')                AS BASECODE" _
                & " , ISNULL(RTRIM(OIT0002.BASENAME), '')                AS BASENAME" _
                & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')           AS CONSIGNEECODE" _
                & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')           AS CONSIGNEENAME" _
                & " , ISNULL(RTRIM(OIT0002.ORDERINFO), '')               AS ORDERINFO" _
                & " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
                & "   WHEN '10' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '11' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '12' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   ELSE ISNULL(RTRIM(OIS0015_2.VALUE1), '')" _
                & "   END                                                           AS ORDERINFONAME" _
                & " , ISNULL(RTRIM(OIT0003.OILCODE), '')                 AS OILCODE" _
                & " , ISNULL(RTRIM(OIM0003_NOW.OILNAME), '')             AS OILNAME" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIT0003.TANKNO), '') <> '' THEN @P03" _
                & "   WHEN OIT0003.TANKNO <> '' " _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P04" _
                & "   WHEN OIT0003.TANKNO <> '' " _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P04" _
                & "   ELSE @P05" _
                & "   END                                                AS TANKQUOTA" _
                & " , ''                                                 AS LINKNO" _
                & " , ''                                                 AS LINKDETAILNO" _
                & " , ''                                                 AS LINEORDER" _
                & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                  AS TANKNO" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>'" _
                & "   END                                                           AS JRINSPECTIONALERT" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P09" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN @P10" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN @P11" _
                & "   END                                                           AS JRINSPECTIONALERTSTR" _
                & " , ISNULL(FORMAT(OIM0005.JRINSPECTIONDATE, 'yyyy/MM/dd'), NULL)    AS JRINSPECTIONDATE" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>'" _
                & "   END                                                           AS JRALLINSPECTIONALERT" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P09" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN @P10" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN @P11" _
                & "   END                                                           AS JRALLINSPECTIONALERTSTR" _
                & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), NULL) AS JRALLINSPECTIONDATE" _
                & " , ISNULL(RTRIM(OIT0005.LASTOILCODE), '')                        AS LASTOILCODE" _
                & " , ISNULL(RTRIM(OIM0003_PAST.OILNAME), '')                       AS LASTOILNAME" _
                & " , ISNULL(RTRIM(OIT0002.DELFLG), '')                             AS DELFLG" _
                & " FROM OIL.OIT0002_ORDER OIT0002 " _
                & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
                & "       OIT0002.ORDERNO = OIT0003.ORDERNO" _
                & "       AND OIT0003.DELFLG <> @P02" _
                & " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
                & "       OIT0003.TANKNO = OIT0005.TANKNUMBER" _
                & "       AND OIT0005.DELFLG <> @P02" _
                & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
                & "       OIT0003.TANKNO = OIM0005.TANKNUMBER" _
                & "       AND OIM0005.DELFLG <> @P02" _
                & " LEFT JOIN com.OIS0015_FIXVALUE OIS0015_2 ON " _
                & "        OIS0015_2.CLASS   = 'ORDERINFO' " _
                & "    AND OIS0015_2.KEYCODE = OIT0002.ORDERINFO " _
                & " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_NOW ON " _
                & "       OIT0002.OFFICECODE = OIM0003_NOW.OFFICECODE" _
                & "       AND OIT0002.SHIPPERSCODE = OIM0003_NOW.SHIPPERCODE" _
                & "       AND OIT0002.BASECODE = OIM0003_NOW.PLANTCODE" _
                & "       AND OIT0003.OILCODE = OIM0003_NOW.OILCODE" _
                & "       AND OIM0003_NOW.DELFLG <> @P02" _
                & " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_PAST ON " _
                & "       OIT0002.OFFICECODE = OIM0003_PAST.OFFICECODE" _
                & "       AND OIT0002.SHIPPERSCODE = OIM0003_PAST.SHIPPERCODE" _
                & "       AND OIT0002.BASECODE = OIM0003_PAST.PLANTCODE" _
                & "       AND OIT0005.LASTOILCODE = OIM0003_PAST.OILCODE" _
                & "       AND OIM0003_PAST.DELFLG <> @P02" _
                & " WHERE OIT0002.ORDERNO = @P01" _
                & " AND OIT0002.DELFLG <> @P02"

            'SQLStr &=
            '      " ORDER BY" _
            '    & "    OIT0002.ORDERNO"

        End If
        SQLTempTblStr &= SQLStr

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLTMPcmd As New SqlCommand(SQLTempTblStr, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.Int)          '明細数(新規作成)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 6)  'タンク車割当状況(割当)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 6)  'タンク車割当状況(不可)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 6)  'タンク車割当状況(未割当)

                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20)  '赤丸
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)  '黄丸
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)  '緑丸
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 10)  '荷主コード
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 40)  '荷主名
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 9)   '基地コード
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 40)  '基地名
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 10)  '荷受人コード
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 40)  '荷受人名

                PARA00.Value = O_INSCNT
                PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE
                PARA03.Value = CONST_TANKNO_STATUS_WARI
                PARA04.Value = CONST_TANKNO_STATUS_FUKA
                PARA05.Value = CONST_TANKNO_STATUS_MIWARI

                PARA09.Value = C_INSPECTIONALERT.ALERT_RED
                PARA10.Value = C_INSPECTIONALERT.ALERT_YELLOW
                PARA11.Value = C_INSPECTIONALERT.ALERT_GREEN
                PARA12.Value = work.WF_SEL_SHIPPERSCODE.Text
                PARA13.Value = work.WF_SEL_SHIPPERSNAME.Text
                PARA14.Value = work.WF_SEL_BASECODE.Text
                PARA15.Value = work.WF_SEL_BASENAME.Text
                PARA16.Value = work.WF_SEL_CONSIGNEECODE.Text
                PARA17.Value = work.WF_SEL_CONSIGNEENAME.Text

                'tmp作成用
                Dim PARATMP00 As SqlParameter = SQLTMPcmd.Parameters.Add("@P00", SqlDbType.Int)          '明細数(新規作成)
                Dim PARATMP01 As SqlParameter = SQLTMPcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARATMP02 As SqlParameter = SQLTMPcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARATMP03 As SqlParameter = SQLTMPcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 6)  'タンク車割当状況(割当)
                Dim PARATMP04 As SqlParameter = SQLTMPcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 6)  'タンク車割当状況(不可)
                Dim PARATMP05 As SqlParameter = SQLTMPcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 6)  'タンク車割当状況(未割当)

                Dim PARATMP09 As SqlParameter = SQLTMPcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20)  '赤丸
                Dim PARATMP10 As SqlParameter = SQLTMPcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)  '黄丸
                Dim PARATMP11 As SqlParameter = SQLTMPcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)  '緑丸
                Dim PARATMP12 As SqlParameter = SQLTMPcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 10)  '荷主コード
                Dim PARATMP13 As SqlParameter = SQLTMPcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 40)  '荷主名
                Dim PARATMP14 As SqlParameter = SQLTMPcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 9)   '基地コード
                Dim PARATMP15 As SqlParameter = SQLTMPcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 40)  '基地名
                Dim PARATMP16 As SqlParameter = SQLTMPcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 10)  '荷受人コード
                Dim PARATMP17 As SqlParameter = SQLTMPcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 40)  '荷受人名

                PARATMP00.Value = O_INSCNT
                PARATMP01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARATMP02.Value = C_DELETE_FLG.DELETE
                PARATMP03.Value = CONST_TANKNO_STATUS_WARI
                PARATMP04.Value = CONST_TANKNO_STATUS_FUKA
                PARATMP05.Value = CONST_TANKNO_STATUS_MIWARI

                PARATMP09.Value = C_INSPECTIONALERT.ALERT_RED
                PARATMP10.Value = C_INSPECTIONALERT.ALERT_YELLOW
                PARATMP11.Value = C_INSPECTIONALERT.ALERT_GREEN
                PARATMP12.Value = work.WF_SEL_SHIPPERSCODE.Text
                PARATMP13.Value = work.WF_SEL_SHIPPERSNAME.Text
                PARATMP14.Value = work.WF_SEL_BASECODE.Text
                PARATMP15.Value = work.WF_SEL_BASENAME.Text
                PARATMP16.Value = work.WF_SEL_CONSIGNEECODE.Text
                PARATMP17.Value = work.WF_SEL_CONSIGNEENAME.Text

                SQLTMPcmd.ExecuteNonQuery()

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    i += 1
                    OIT0003row("LINECNT") = i        'LINECNT

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 画面表示データ取得(貨車連結を使用)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGetLink(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0003tbl) Then
            OIT0003tbl = New DataTable
        End If

        If OIT0003tbl.Columns.Count <> 0 Then
            OIT0003tbl.Columns.Clear()
        End If

        OIT0003tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
              " SELECT" _
            & "   0                                                             AS LINECNT" _
            & " , ''                                                            AS OPERATION" _
            & " , CAST(OIT0004.UPDTIMSTP AS bigint)                             AS TIMSTP" _
            & " , 1                                                             AS 'SELECT'" _
            & " , 0                                                             AS HIDDEN" _
            & " , ISNULL(RTRIM(TMP0001.ORDERNO), '')                            AS ORDERNO" _
            & " , ISNULL(RTRIM(TMP0001.DETAILNO), '')                           AS DETAILNO" _
            & " , ISNULL(RTRIM(TMP0001.SHIPPERSCODE), '')                       AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(TMP0001.SHIPPERSNAME), '')                       AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(TMP0001.BASECODE), '')                           AS BASECODE" _
            & " , ISNULL(RTRIM(TMP0001.BASENAME), '')                           AS BASENAME" _
            & " , ISNULL(RTRIM(TMP0001.CONSIGNEECODE), '')                      AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(TMP0001.CONSIGNEENAME), '')                      AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINFO), '')                          AS ORDERINFO" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINFONAME), '')                      AS ORDERINFONAME" _
            & " , ISNULL(RTRIM(TMP0001.OILCODE), '')                            AS OILCODE" _
            & " , ISNULL(RTRIM(TMP0001.OILNAME), '')                            AS OILNAME" _
            & " , CASE" _
            & "   WHEN OIT0004.TANKNUMBER IS NULL AND TMP0001.OILNAME IS NULL THEN @P04" _
            & "   WHEN OIT0004.TANKNUMBER <> '' " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN OIT0004.TANKNUMBER <> '' " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN OIT0004.TANKNUMBER IS NOT NULL AND TMP0001.OILCODE IS NULL THEN @P04" _
            & "   WHEN OIT0004.TANKNUMBER IS NOT NULL AND TMP0001.OILCODE = OIT0004.PREOILCODE THEN @P06" _
            & "   END                                                           AS TANKQUOTA" _
            & " , ISNULL(RTRIM(OIT0004.LINKNO), '')                             AS LINKNO" _
            & " , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')                       AS LINKDETAILNO" _
            & " , ISNULL(RTRIM(OIT0004.LINEORDER), '')                          AS LINEORDER" _
            & " , ISNULL(RTRIM(OIT0004.TANKNUMBER), '')                         AS TANKNO" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>'" _
            & "   END                                                           AS JRINSPECTIONALERT" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P08" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN @P09" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN @P10" _
            & "   END                                                           AS JRINSPECTIONALERTSTR" _
            & " , ISNULL(FORMAT(OIM0005.JRINSPECTIONDATE, 'yyyy/MM/dd'), NULL)    AS JRINSPECTIONDATE" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>'" _
            & "   END                                                           AS JRALLINSPECTIONALERT" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P08" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN @P09" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN @P10" _
            & "   END                                                           AS JRALLINSPECTIONALERTSTR" _
            & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), NULL) AS JRALLINSPECTIONDATE" _
            & " , ISNULL(RTRIM(OIT0004.PREOILCODE), '')                         AS LASTOILCODE" _
            & " , ISNULL(RTRIM(OIM0003_PAST.OILNAME), '')                       AS LASTOILNAME" _
            & " , ISNULL(RTRIM(OIT0004.DELFLG), '')                             AS DELFLG" _
            & " FROM OIL.OIT0004_LINK OIT0004 " _
            & " LEFT JOIN OIL.TMP0001ORDER TMP0001 ON " _
            & "       OIT0004.TANKNUMBER = TMP0001.TANKNO " _
            & "       AND TMP0001.DELFLG <> @P03" _
            & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "       OIT0004.TANKNUMBER = OIM0005.TANKNUMBER" _
            & "       AND OIM0005.DELFLG <> @P03" _
            & " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_PAST ON " _
            & "       OIM0003_PAST.OFFICECODE = @P11" _
            & "       AND OIM0003_PAST.SHIPPERCODE = @P12" _
            & "       AND OIM0003_PAST.PLANTCODE  = @P13" _
            & "       AND OIT0004.PREOILCODE = OIM0003_PAST.OILCODE" _
            & "       AND OIM0003_PAST.DELFLG <> @P03" _
            & " WHERE OIT0004.LINKNO = @P01" _
            & " AND OIT0004.TRAINNO = @P02" _
            & " AND OIT0004.DELFLG <> @P03"
        '& "   WHEN TMP0001.TANKNO IS NOT NULL AND TMP0001.OILCODE <> OIT0004.PREOILCODE THEN '前回油種確認'" _

        SQLStr &=
              " " _
            & " UNION ALL " _
            & " SELECT" _
            & "   0                                                             AS LINECNT" _
            & " , ''                                                            AS OPERATION" _
            & " , 0                             AS TIMSTP" _
            & " , 1                                                             AS 'SELECT'" _
            & " , 0                                                             AS HIDDEN" _
            & " , ISNULL(RTRIM(TMP0001.ORDERNO), '')                            AS ORDERNO" _
            & " , ISNULL(RTRIM(TMP0001.DETAILNO), '')                           AS DETAILNO" _
            & " , ISNULL(RTRIM(TMP0001.SHIPPERSCODE), '')                       AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(TMP0001.SHIPPERSNAME), '')                       AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(TMP0001.BASECODE), '')                           AS BASECODE" _
            & " , ISNULL(RTRIM(TMP0001.BASENAME), '')                           AS BASENAME" _
            & " , ISNULL(RTRIM(TMP0001.CONSIGNEECODE), '')                      AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(TMP0001.CONSIGNEENAME), '')                      AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINFO), '')                          AS ORDERINFO" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINFONAME), '')                      AS ORDERINFONAME" _
            & " , ISNULL(RTRIM(TMP0001.OILCODE), '')                            AS OILCODE" _
            & " , ISNULL(RTRIM(TMP0001.OILNAME), '')                            AS OILNAME" _
            & " , @P07                                                          AS TANKQUOTA" _
            & " , ISNULL(RTRIM(OIT0004.LINKNO), '')                             AS LINKNO" _
            & " , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')                       AS LINKDETAILNO" _
            & " , ISNULL(RTRIM(OIT0004.LINEORDER), '')                          AS LINEORDER" _
            & " , ISNULL(RTRIM(TMP0001.TANKNO), '')                             AS TANKNO" _
            & " , ISNULL(RTRIM(TMP0001.JRINSPECTIONALERT), '')                  AS JRINSPECTIONALERT" _
            & " , ISNULL(RTRIM(TMP0001.JRINSPECTIONALERTSTR), '')               AS JRINSPECTIONALERTSTR" _
            & " , ISNULL(RTRIM(TMP0001.JRINSPECTIONDATE), '')                   AS JRINSPECTIONDATE" _
            & " , ISNULL(RTRIM(TMP0001.JRALLINSPECTIONALERT), '')               AS JRALLINSPECTIONALERT" _
            & " , ISNULL(RTRIM(TMP0001.JRALLINSPECTIONALERTSTR), '')            AS JRALLINSPECTIONALERTSTR" _
            & " , ISNULL(RTRIM(TMP0001.JRALLINSPECTIONDATE), '')                AS JRALLINSPECTIONDATE" _
            & " , ''                                                            AS LASTOILCODE" _
            & " , ''                                                            AS LASTOILNAME" _
            & " , ISNULL(RTRIM(TMP0001.DELFLG), '')                             AS DELFLG" _
            & " FROM OIL.TMP0001ORDER TMP0001 " _
            & " LEFT JOIN OIL.OIT0004_LINK OIT0004 ON " _
            & "       OIT0004.TANKNUMBER = TMP0001.TANKNO " _
            & "       AND OIT0004.LINKNO = @P01" _
            & "       AND OIT0004.TRAINNO = @P02" _
            & "       AND OIT0004.DELFLG <> @P03" _
            & " WHERE OIT0004.TANKNUMBER IS NULL"

        'SQLStr &=
        '      " ORDER BY" _
        '    & "    OIT0004.LINKNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 6)  'タンク車割当状況(残車)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 6)  'タンク車割当状況(不可)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 6)  'タンク車割当状況(割当)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 6)  'タンク車割当状況(未割当)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 20) '赤丸
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20) '黄丸
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20) '緑丸
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 6)  '営業所コード
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 10) '荷主コード
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 9)  '基地コード

                PARA01.Value = work.WF_SEL_LINK_LINKNO.Text
                PARA02.Value = work.WF_SEL_LINK_TRAIN.Text
                PARA03.Value = C_DELETE_FLG.DELETE
                PARA04.Value = CONST_TANKNO_STATUS_ZAN
                PARA05.Value = CONST_TANKNO_STATUS_FUKA
                PARA06.Value = CONST_TANKNO_STATUS_WARI
                PARA07.Value = CONST_TANKNO_STATUS_MIWARI
                PARA08.Value = C_INSPECTIONALERT.ALERT_RED
                PARA09.Value = C_INSPECTIONALERT.ALERT_YELLOW
                PARA10.Value = C_INSPECTIONALERT.ALERT_GREEN
                PARA11.Value = work.WF_SEL_ORDERSALESOFFICECODE.Text
                PARA12.Value = work.WF_SEL_SHIPPERSCODE.Text
                PARA13.Value = work.WF_SEL_BASECODE.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    i += 1
                    OIT0003row("LINECNT") = i        'LINECNT

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        '〇 選択されたタブの一覧を再表示
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            DisplayGrid_TAB1()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            DisplayGrid_TAB2()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            DisplayGrid_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            DisplayGrid_TAB4()

        End If

    End Sub

    ''' <summary>
    ''' 一覧再表示処理(タブ「タンク車割当」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid_TAB1()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            If OIT0003row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0003row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(OIT0003tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea1
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 一覧再表示処理(タブ「タンク車明細」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid_TAB2()

    End Sub

    ''' <summary>
    ''' 一覧再表示処理(タブ「入換・積込指示」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid_TAB3()

    End Sub

    ''' <summary>
    ''' 一覧再表示処理(タブ「費用入力」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid_TAB4()

    End Sub

    ''' <summary>
    ''' 油種数登録ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '着駅コードが未設定の場合
        '※一覧を作成するにあたり、基地コード・荷受人を取得するために、
        '　着駅コードは必須となるため
        If TxtArrstationCode.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "着駅", needsPopUp:=True)
            TxtArrstationCode.Focus()
            WW_CheckERR("着駅入力エラー。", C_MESSAGE_NO.PREREQUISITE_ERROR)
            Exit Sub
        End If




    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.MAPID = work.WF_SEL_MAPIDBACKUP.Text
        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then

                    '会社コード
                    Dim prmData As New Hashtable
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                    '運用部署
                    If WF_FIELD.Value = "WF_UORG" Then
                        prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    End If

                    '########################################
                    '受注パターン
                    If WF_FIELD.Value = "TxtOrderType" Then
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtOrderType.Text)
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtOrderType.Text)
                        End If
                    End If
                    '########################################

                    '荷主名
                    If WF_FIELD.Value = "TxtShippersCode" Then
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtShippersCode.Text)
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtShippersCode.Text)
                        End If
                    End If

                    '荷受人名
                    If WF_FIELD.Value = "TxtConsigneeCode" Then
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtConsigneeCode.Text)
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtConsigneeCode.Text)
                        End If
                    End If

                    '本線列車
                    If WF_FIELD.Value = "TxtTrainNo" Then
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtTrainNo.Text)
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtTrainNo.Text)
                        End If
                    End If

                    '発駅
                    If WF_FIELD.Value = "TxtDepstationCode" Then
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "1", TxtDepstationCode.Text)
                        Else
                            prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "1", TxtDepstationCode.Text)
                        End If
                    End If

                    '着駅
                    If WF_FIELD.Value = "TxtArrstationCode" Then
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "2", TxtArrstationCode.Text)
                        Else
                            prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "2", TxtArrstationCode.Text)
                        End If
                    End If

                    '油種
                    If WF_FIELD.Value = "OILNAME" Then
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, "")
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
                            'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                        End If
                    End If

                    'タンク車№
                    If WF_FIELD.Value = "TANKNO" Then
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, "")
                        Else
                            'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
                        End If
                    End If

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        '(予定)積込日
                        Case "TxtLoadingDate"
                            .WF_Calendar.Text = TxtLoadingDate.Text
                        '(予定)発日
                        Case "TxtDepDate"
                            .WF_Calendar.Text = TxtDepDate.Text
                        '(予定)積車着日
                        Case "TxtArrDate"
                            .WF_Calendar.Text = TxtArrDate.Text
                        '(予定)受入日
                        Case "TxtAccDate"
                            .WF_Calendar.Text = TxtAccDate.Text
                        '(予定)空車着日
                        Case "TxtEmparrDate"
                            .WF_Calendar.Text = TxtEmparrDate.Text
                        '(実績)積込日
                        Case "TxtActualLoadingDate"
                            .WF_Calendar.Text = TxtActualLoadingDate.Text
                        '(実績)発日
                        Case "TxtActualDepDate"
                            .WF_Calendar.Text = TxtActualDepDate.Text
                        '(実績)積車着日
                        Case "TxtActualArrDate"
                            .WF_Calendar.Text = TxtActualArrDate.Text
                        '(実績)受入日
                        Case "TxtActualAccDate"
                            .WF_Calendar.Text = TxtActualAccDate.Text
                        '(実績)空車着日
                        Case "TxtActualEmparrDate"
                            .WF_Calendar.Text = TxtActualEmparrDate.Text
                    End Select
                    .ActiveCalendar()

                End If
            End With

        End If
    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        'チェックボックス判定
        For i As Integer = 0 To OIT0003tbl.Rows.Count - 1
            If OIT0003tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If OIT0003tbl.Rows(i)("OPERATION") = "on" Then
                    OIT0003tbl.Rows(i)("OPERATION") = ""
                Else
                    OIT0003tbl.Rows(i)("OPERATION") = "on"
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '運用部署
            Case "WF_UORG"
                CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_RTN_SW)

            '荷主
            Case "TxtShippersCode"
                CODENAME_get("SHIPPERS", TxtShippersCode.Text, LblShippersName.Text, WW_RTN_SW)

            '荷受人
            Case "TxtConsigneeCode"
                CODENAME_get("CONSIGNEE", TxtConsigneeCode.Text, LblConsigneeName.Text, WW_RTN_SW)

            '本線列車
            Case "TxtTrainNo"
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}

                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    WW_FixvalueMasterSearch(Master.USER_ORG, "TRAINNUMBER", TxtTrainNo.Text, WW_GetValue)
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER", TxtTrainNo.Text, WW_GetValue)
                End If

                '指定された本線列車№で値が取得できない場合はエラー判定
                If WW_GetValue(0) = "" Then
                    WW_RTN_SW = C_MESSAGE_NO.OIL_TRAIN_MASTER_NOTFOUND
                Else
                    WW_RTN_SW = C_MESSAGE_NO.NORMAL
                End If

                '発駅
                TxtDepstationCode.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", TxtDepstationCode.Text, LblDepstationName.Text, WW_DUMMY)
                '着駅
                TxtArrstationCode.Text = WW_GetValue(2)
                CODENAME_get("ARRSTATION", TxtArrstationCode.Text, LblArrstationName.Text, WW_DUMMY)
                TxtTrainNo.Focus()

                '〇営業所配下情報を取得・設定
                WW_GetValue = {"", "", "", "", "", "", "", ""}

                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                End If

                '荷主
                TxtShippersCode.Text = WW_GetValue(0)
                LblShippersName.Text = WW_GetValue(1)
                '荷受人
                TxtConsigneeCode.Text = WW_GetValue(4)
                LblConsigneeName.Text = WW_GetValue(5)
                '受注パターン
                TxtOrderType.Text = WW_GetValue(7)

                '〇 (予定)の日付を設定
                TxtLoadingDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtDepDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtArrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtAccDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtEmparrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")

                work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
                work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
                work.WF_SEL_BASECODE.Text = WW_GetValue(2)
                work.WF_SEL_BASENAME.Text = WW_GetValue(3)
                work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
                work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)
                work.WF_SEL_PATTERNCODE.Text = WW_GetValue(6)
                work.WF_SEL_PATTERNNAME.Text = WW_GetValue(7)

            '発駅
            Case "TxtDepstationCode"
                CODENAME_get("DEPSTATION", TxtDepstationCode.Text, LblDepstationName.Text, WW_RTN_SW)

            '着駅
            Case "TxtArrstationCode"
                CODENAME_get("ARRSTATION", TxtArrstationCode.Text, LblArrstationName.Text, WW_RTN_SW)

        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Select Case WF_FIELD.Value
                Case "TxtShippersCode"
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
                Case "TxtConsigneeCode"
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
                Case "TxtTrainNo"
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
                Case "TxtDepstationCode"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "発駅")
                Case "TxtArrstationCode"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "着駅")
                Case Else
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
            End Select
        End If
    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '〇 選択されたタブ一覧の全解除を制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_ButtonALLSELECT_TAB1()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonALLSELECT_TAB2()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            WW_ButtonALLSELECT_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            WW_ButtonALLSELECT_TAB4()

        End If

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB1()
        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0003tbl.Rows.Count - 1
            If OIT0003tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0003tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「タンク車明細」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB2()

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB3()

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB4()

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '〇 選択されたタブ一覧の全解除を制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_ButtonSELECT_LIFTED_TAB1()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonSELECT_LIFTED_TAB2()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            WW_ButtonSELECT_LIFTED_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            WW_ButtonSELECT_LIFTED_TAB4()

        End If

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB1()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0003tbl.Rows.Count - 1
            If OIT0003tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0003tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「タンク車明細」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB2()

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB3()

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB4()

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_LIFTED_Click()

        '〇 選択されたタブ一覧の行削除を制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_ButtonLINE_LIFTED_TAB1()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonLINE_LIFTED_TAB2()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            WW_ButtonLINE_LIFTED_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            WW_ButtonLINE_LIFTED_TAB4()

        End If

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB1()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        '■■■ OIT0001tbl関連の受注・受注明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注明細・貨車連結表を一括論理削除
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0003_DETAIL         " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE ORDERNO     = @P01       " _
                    & "    AND DETAILNO    = @P02       " _
                    & "    AND DELFLG     <> '1'       ;"

            SQLStr &=
                    " UPDATE OIL.OIT0004_LINK           " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE LINKNO       = @P03      " _
                    & "    AND LINKDETAILNO = @P04      " _
                    & "    AND DELFLG      <> '1'      ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            For Each OIT0001UPDrow In OIT0003tbl.Rows
                If OIT0001UPDrow("OPERATION") = "on" Then
                    j += 1
                    OIT0001UPDrow("LINECNT") = j        'LINECNT
                    OIT0001UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0001UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0001UPDrow("ORDERNO")
                    PARA02.Value = OIT0001UPDrow("DETAILNO")
                    PARA03.Value = OIT0001UPDrow("LINKNO")
                    PARA04.Value = OIT0001UPDrow("LINKDETAILNO")

                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()
                Else
                    i += 1
                    OIT0001UPDrow("LINECNT") = i        'LINECNT
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB1 DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB1 DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理(タブ「タンク車明細」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB2()

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB3()

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB4()

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_ADD_Click()
        '〇 選択されたタブ一覧の行追加を制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_ButtonLINE_ADD_TAB1()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonLINE_ADD_TAB2()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            WW_ButtonLINE_ADD_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            WW_ButtonLINE_ADD_TAB4()

        End If

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB1()
        If IsNothing(OIT0003WKtbl) Then
            OIT0003WKtbl = New DataTable
        End If

        If OIT0003WKtbl.Columns.Count <> 0 Then
            OIT0003WKtbl.Columns.Clear()
        End If

        OIT0003WKtbl.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Dim SQLStrNum As String

        'If work.WF_SEL_ORDERNUMBER.Text = "" Then
        '○ 作成モード(１：新規登録, ２：更新)設定
        If work.WF_SEL_CREATEFLG.Text = "1" Then
            SQLStrNum =
            " SELECT " _
            & "  @P01   AS ORDERNO" _
            & ", '001'  AS DETAILNO"

        Else
            SQLStrNum =
            " SELECT " _
            & "  ISNULL(OIT0003.ORDERNO,'')                                     AS ORDERNO" _
            & ", ISNULL(FORMAT(CONVERT(INT, OIT0003.DETAILNO) + 1,'000'),'000') AS DETAILNO" _
            & " FROM (" _
            & "  SELECT OIT0003.ORDERNO" _
            & "       , OIT0003.DETAILNO" _
            & "       , ROW_NUMBER() OVER(PARTITION BY OIT0003.ORDERNO ORDER BY OIT0003.ORDERNO, OIT0003.DETAILNO DESC) RNUM" _
            & "  FROM OIL.OIT0003_DETAIL OIT0003" _
            & "  WHERE OIT0003.ORDERNO = @P01" _
            & " ) OIT0003 " _
            & " WHERE OIT0003.RNUM = 1"

        End If

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String =
              " SELECT TOP (1)" _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , @P01                                           AS ORDERNO" _
            & " , @P08                                           AS DETAILNO" _
            & " , @P02                                           AS SHIPPERSCODE" _
            & " , @P03                                           AS SHIPPERSNAME" _
            & " , @P04                                           AS BASECODE" _
            & " , @P05                                           AS BASENAME" _
            & " , @P06                                           AS CONSIGNEECODE" _
            & " , @P07                                           AS CONSIGNEENAME" _
            & " , ''                                             AS ORDERINFO" _
            & " , ''                                             AS ORDERINFONAME" _
            & " , ''                                             AS OILCODE" _
            & " , ''                                             AS OILNAME" _
            & " , ''                                             AS TANKQUOTA" _
            & " , ''                                             AS LINKNO" _
            & " , ''                                             AS LINKDETAILNO" _
            & " , ''                                             AS LINEORDER" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS LASTOILCODE" _
            & " , ''                                             AS LASTOILNAME" _
            & " , '0'                                            AS DELFLG" _
            & " FROM sys.all_objects "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)
                Dim PARANUM1 As SqlParameter = SQLcmdNum.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                PARANUM1.Value = work.WF_SEL_ORDERNUMBER.Text

                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0003WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 3)  '受注明細№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10) '荷主コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 40) '荷主名
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 9)  '基地コード
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 40) '基地名
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 10) '荷受人コード
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 40) '荷受人名

                Dim intDetailNo As Integer = 0
                For Each OIT0001WKrow As DataRow In OIT0003WKtbl.Rows
                    intDetailNo = OIT0001WKrow("DETAILNO")
                    PARA1.Value = OIT0001WKrow("ORDERNO")
                    PARA8.Value = OIT0001WKrow("DETAILNO")
                    PARA2.Value = work.WF_SEL_SHIPPERSCODE.Text
                    PARA3.Value = work.WF_SEL_SHIPPERSNAME.Text
                    PARA4.Value = work.WF_SEL_BASECODE.Text
                    PARA5.Value = work.WF_SEL_BASENAME.Text
                    PARA6.Value = work.WF_SEL_CONSIGNEECODE.Text
                    PARA7.Value = work.WF_SEL_CONSIGNEENAME.Text
                Next

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                For Each OIT0003row As DataRow In OIT0003tbl.Rows

                    '行追加データに既存の受注№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[受注№]を利用
                    If OIT0003row("LINECNT") = 0 Then
                        OIT0003row("DETAILNO") = intDetailNo.ToString("000")

                    ElseIf OIT0003row("DETAILNO") = intDetailNo.ToString("000") Then
                        intDetailNo += 1

                    End If

                    '削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    If OIT0003row("HIDDEN") = 1 Then
                        j += 1
                        OIT0003row("LINECNT") = j        'LINECNT
                    Else
                        i += 1
                        OIT0003row("LINECNT") = i        'LINECNT
                    End If

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB1 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB1 SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「タンク車明細」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB2()

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB3()

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB4()

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub

    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            rightview.SelectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()

        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = OIT0003tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea1.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "SHIPPERSNAME"      '(一覧)荷主
                If WW_ListValue <> "" Then
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "SHIPPERSMASTER_N", WW_ListValue, WW_GetValue)
                    updHeader.Item("SHIPPERSCODE") = WW_GetValue(0)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                Else
                    updHeader.Item("SHIPPERSCODE") = ""
                    updHeader.Item(WF_FIELD.Value) = ""
                End If

            Case "OILNAME"           '(一覧)油種
                '〇油種が設定されている場合
                If WW_ListValue <> "" Then
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN_N", WW_ListValue, WW_GetValue)
                    updHeader.Item("OILCODE") = WW_GetValue(0)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                Else
                    updHeader.Item("OILCODE") = ""
                    updHeader.Item(WF_FIELD.Value) = ""
                End If

                '〇 タンク車割当状況チェック
                WW_TANKQUOTACHK(updHeader)
                ''タンク車割当状況＝"割当"の場合
                'If updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI Then

                '    '油種が削除("")の場合
                '    If updHeader.Item("OILCODE") = "" Then
                '        'タンク車割当状況＝"残車"に設定
                '        updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN
                '    End If

                '    'タンク車割当状況＝"残車"の場合
                'ElseIf updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN Then
                '    '油種が設定された場合
                '    If updHeader.Item("OILCODE") <> "" Then
                '        'タンク車割当状況＝"割当"に設定
                '        updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI
                '    End If
                'End If

            Case "TANKNO"            '(一覧)タンク車№

                '入力が空の場合は、対象項目を空文字で設定する。
                If WW_ListValue = "" Then
                    'タンク車№
                    updHeader.Item("TANKNO") = ""
                    '前回油種
                    updHeader.Item("LASTOILCODE") = ""
                    updHeader.Item("LASTOILNAME") = ""
                    '交検日
                    updHeader.Item("JRINSPECTIONDATE") = ""
                    updHeader.Item("JRINSPECTIONALERT") = ""
                    updHeader.Item("JRINSPECTIONALERTSTR") = ""
                    '全検日
                    updHeader.Item("JRALLINSPECTIONDATE") = ""
                    updHeader.Item("JRALLINSPECTIONALERT") = ""
                    updHeader.Item("JRALLINSPECTIONALERTSTR") = ""
                    Exit Select
                End If

                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)

                'タンク車№
                updHeader.Item("TANKNO") = WW_ListValue

                '前回油種
                Dim WW_LASTOILNAME As String = ""
                updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                updHeader.Item("LASTOILNAME") = WW_LASTOILNAME

                'Dim WW_GetValue2() As String = {"", "", "", "", "", ""}
                'WW_FixvalueMasterSearch("", "PRODUCTPATTERN", WW_GetValue(1), WW_GetValue2)
                'updHeader.Item("LASTOILNAME") = WW_GetValue2(0)

                '交検日
                Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                Dim WW_JRINSPECTIONCNT As String
                updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(2)
                If WW_GetValue(2) <> "" Then
                    WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(2)))

                    Dim WW_JRINSPECTIONFLG As String
                    If WW_JRINSPECTIONCNT <= 3 Then
                        WW_JRINSPECTIONFLG = "1"
                    ElseIf WW_JRINSPECTIONCNT >= 4 And WW_JRINSPECTIONCNT <= 6 Then
                        WW_JRINSPECTIONFLG = "2"
                    Else
                        WW_JRINSPECTIONFLG = "3"
                    End If
                    Select Case WW_JRINSPECTIONFLG
                        Case "1"
                            updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                        Case "2"
                            updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                        Case "3"
                            updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                            updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                    End Select
                Else
                    updHeader.Item("JRINSPECTIONALERT") = ""
                    updHeader.Item("JRINSPECTIONALERTSTR") = ""
                End If

                '全検日
                Dim WW_JRALLINSPECTIONCNT As String
                updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(3)
                If WW_GetValue(3) <> "" Then
                    WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(3)))

                    Dim WW_JRALLINSPECTIONFLG As String
                    If WW_JRALLINSPECTIONCNT <= 3 Then
                        WW_JRALLINSPECTIONFLG = "1"
                    ElseIf WW_JRALLINSPECTIONCNT >= 4 And WW_JRALLINSPECTIONCNT <= 6 Then
                        WW_JRALLINSPECTIONFLG = "2"
                    Else
                        WW_JRALLINSPECTIONFLG = "3"
                    End If
                    Select Case WW_JRALLINSPECTIONFLG
                        Case "1"
                            updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                        Case "2"
                            updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                        Case "3"
                            updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                            updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                    End Select
                Else
                    updHeader.Item("JRALLINSPECTIONALERT") = ""
                    updHeader.Item("JRALLINSPECTIONALERTSTR") = ""
                End If

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' 詳細画面 初期設定(空明細作成 イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_INIT()
        Dim dataTable As DataTable = New DataTable
        '○詳細ヘッダーの設定
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        ''WF_CAMPCODE.ReadOnly = True
        ''WF_CAMPCODE.Style.Add("background-color", "rgb(213,208,181)")
        ''WF_SHARYOTYPE.ReadOnly = True
        ''WF_SHARYOTYPE.Style.Add("background-color", "rgb(213,208,181)")
        ''WF_TSHABAN.ReadOnly = True
        ''WF_TSHABAN.Style.Add("background-color", "rgb(213,208,181)")

        ''カラム情報をリピーター作成用に取得
        'Master.CreateEmptyTable(dataTable)
        'dataTable.Rows.Add(dataTable.NewRow())

        ''○ディテール01（タンク車割当）変数設定 
        'CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        'CS0052DetailView.PROFID = Master.PROF_VIEW
        'CS0052DetailView.MAPID = Master.MAPID
        'CS0052DetailView.VARI = Master.VIEWID
        'CS0052DetailView.TABID = "MANG"
        'CS0052DetailView.SRCDATA = dataTable
        'CS0052DetailView.REPEATER = WF_DViewRep1
        'CS0052DetailView.COLPREFIX = "WF_Rep1_"
        'CS0052DetailView.MaketDetailView()
        'If Not isNormal(CS0052DetailView.ERR) Then
        '    Exit Sub
        'End If

        ''○ディテール02（タンク車明細）変数設定 
        'CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        'CS0052DetailView.PROFID = Master.PROF_VIEW
        'CS0052DetailView.MAPID = Master.MAPID
        'CS0052DetailView.VARI = Master.VIEWID
        'CS0052DetailView.TABID = "SYAB"
        'CS0052DetailView.SRCDATA = dataTable
        'CS0052DetailView.REPEATER = WF_DViewRep2
        'CS0052DetailView.COLPREFIX = "WF_Rep2_"
        'CS0052DetailView.MaketDetailView()
        'If Not isNormal(CS0052DetailView.ERR) Then
        '    Exit Sub
        'End If

        ''○ディテール03（入換・積込指示）変数設定
        'CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        'CS0052DetailView.PROFID = Master.PROF_VIEW
        'CS0052DetailView.MAPID = Master.MAPID
        'CS0052DetailView.VARI = Master.VIEWID
        'CS0052DetailView.TABID = "FCTR"
        'CS0052DetailView.SRCDATA = dataTable
        'CS0052DetailView.REPEATER = WF_DViewRep3
        'CS0052DetailView.COLPREFIX = "WF_Rep3_"
        'CS0052DetailView.MaketDetailView()
        'If Not isNormal(CS0052DetailView.ERR) Then
        '    Exit Sub
        'End If

        ''○ディテール04（費用入力）変数設定
        'CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        'CS0052DetailView.PROFID = Master.PROF_VIEW
        'CS0052DetailView.MAPID = Master.MAPID
        'CS0052DetailView.VARI = Master.VIEWID
        'CS0052DetailView.TABID = "OTNK"
        'CS0052DetailView.SRCDATA = dataTable
        'CS0052DetailView.REPEATER = WF_DViewRep4
        'CS0052DetailView.COLPREFIX = "WF_Rep4_"
        'CS0052DetailView.MaketDetailView()
        'If Not isNormal(CS0052DetailView.ERR) Then
        '    Exit Sub
        'End If

        ''○ディテール01（管理）イベント設定 
        'Dim WW_FIELD As Label = Nothing
        'Dim WW_VALUE As TextBox = Nothing
        'Dim WW_FIELDNM As Label = Nothing
        'Dim WW_ATTR As String = ""

        'For tabindex As Integer = 1 To CONST_MAX_TABID
        '    Dim rep As Repeater = CType(WF_DetailMView.FindControl("WF_DViewRep" & tabindex), Repeater)
        '    For Each reitem As RepeaterItem In rep.Items
        '        'ダブルクリック時コード検索イベント追加
        '        If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label).Text <> "" Then
        '            WW_FIELD = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label)
        '            WW_VALUE = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_1"), TextBox)
        '            ATTR_get(WW_FIELD.Text, WW_ATTR)
        '            If WW_ATTR <> "" AndAlso Not WW_VALUE.ReadOnly Then
        '                WW_VALUE.Attributes.Remove("ondblclick")
        '                WW_VALUE.Attributes.Add("ondblclick", WW_ATTR)
        '                WW_FIELDNM = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELDNM_1"), Label)
        '                WW_FIELDNM.Attributes.Remove("style")
        '                WW_FIELDNM.Attributes.Add("style", "text-decoration: underline;")
        '            End If
        '        End If

        '        If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label).Text <> "" Then
        '            WW_FIELD = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label)
        '            WW_VALUE = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_2"), TextBox)
        '            ATTR_get(WW_FIELD.Text, WW_ATTR)
        '            If WW_ATTR <> "" AndAlso Not WW_VALUE.ReadOnly Then
        '                WW_VALUE.Attributes.Remove("ondblclick")
        '                WW_VALUE.Attributes.Add("ondblclick", WW_ATTR)
        '                WW_FIELDNM = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELDNM_2"), Label)
        '                WW_FIELDNM.Attributes.Remove("style")
        '                WW_FIELDNM.Attributes.Add("style", "text-decoration: underline;")
        '            End If
        '        End If

        '        If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label).Text <> "" Then
        '            WW_FIELD = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label)
        '            WW_VALUE = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_3"), TextBox)
        '            ATTR_get(WW_FIELD.Text, WW_ATTR)
        '            If WW_ATTR <> "" AndAlso Not WW_VALUE.ReadOnly Then
        '                WW_VALUE.Attributes.Remove("ondblclick")
        '                WW_VALUE.Attributes.Add("ondblclick", WW_ATTR)
        '                WW_FIELDNM = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELDNM_3"), Label)
        '                WW_FIELDNM.Attributes.Remove("style")
        '                WW_FIELDNM.Attributes.Add("style", "text-decoration: underline;")
        '            End If
        '        End If
        '    Next
        'Next

    End Sub

    ' *** 詳細画面-イベント文字取得
    Protected Sub ATTR_get(ByVal I_FIELD As String, ByRef O_ATTR As String)

        O_ATTR = ""
        Select Case I_FIELD
            Case "CAMPCODE"
                '会社コード
                O_ATTR = "REF_Field_DBclick('CAMPCODE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_COMPANY & "');"
            Case "DELFLG"
                '削除フラグ
                O_ATTR = "REF_Field_DBclick('DELFLG', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_DELFLG & "');"
        End Select

    End Sub

    ''' <summary>
    ''' タブ切替
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Detail_TABChange()

        Dim WW_DTABChange As Integer
        Try
            Integer.TryParse(WF_DTAB_CHANGE_NO.Value, WW_DTABChange)
        Catch ex As Exception
            WW_DTABChange = 0
        End Try

        WF_DetailMView.ActiveViewIndex = WW_DTABChange

        '初期値（書式）変更

        'タンク車割当
        WF_Dtab01.CssClass = ""
        'タンク車明細
        WF_Dtab02.CssClass = ""
        '入換・積込指示
        WF_Dtab03.CssClass = ""
        '費用入力
        WF_Dtab04.CssClass = ""

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                'タンク車割当
                WF_Dtab01.CssClass = "selected"
            Case 1
                'タンク車明細
                WF_Dtab02.CssClass = "selected"
            Case 2
                '入換・積込指示
                WF_Dtab03.CssClass = "selected"
            Case 3
                '費用入力
                WF_Dtab04.CssClass = "selected"
        End Select
    End Sub

    ''' <summary>
    ''' タブ指定時表示判定処理
    ''' </summary>
    Protected Sub TAB_DisplayCTRL()
        ''Const C_SHARYOTYPE_FRONT As String = "前"
        ''Const C_SHARYOTYPE_BACK As String = "後"
        ''Dim WW_RESULT As String = ""
        ''Dim WW_SHARYOTYPE2 As String = ""
        ''Dim WW_MANGOILTYPE As String = ""

        'WF_DViewRep1.Visible = False
        'WF_DViewRep2.Visible = False
        'WF_DViewRep3.Visible = False
        'WF_DViewRep4.Visible = False

        'Select Case WF_DetailMView.ActiveViewIndex
        '    Case 0
        '        WF_DViewRep1.Visible = True
        '    Case 1
        '        WF_DViewRep2.Visible = True
        '    Case 2
        '        WF_DViewRep3.Visible = True
        '    Case 3
        '        WF_DViewRep4.Visible = True
        'End Select

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        Dim prmData As New Hashtable

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

                Case "ORDERTYPE"        '受注パターン
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERTYPE"))

                Case "SHIPPERS"         '荷主
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SHIPPERSLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHIPPERS"))

                Case "CONSIGNEE"        '荷受人
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CONSIGNEE"))

                Case "DEPSTATION"       '発駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPSTATION"))

                Case "ARRSTATION"       '着駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ARRSTATION"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 油種別タンク車数、積込数量データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_OILTANKCntGet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0003WKtbl) Then
            OIT0003WKtbl = New DataTable
        End If

        If OIT0003WKtbl.Columns.Count <> 0 Then
            OIT0003WKtbl.Columns.Clear()
        End If

        OIT0003WKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
              " SELECT DISTINCT " _
            & "   0                                                  AS LINECNT" _
            & " , ''                                                 AS OPERATION" _
            & " , ''                                                 AS TIMSTP" _
            & " , 1                                                  AS 'SELECT'" _
            & " , 0                                                  AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0003.ORDERNO), '')                 AS ORDERNO" _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P10 THEN 1 ELSE 0 END) " _
            & "    OVER(Partition BY OIT0003.ORDERNO)                AS HTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P11 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS RTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P12 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS TTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P13 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS MTTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P14 OR OIT0003.OILCODE = @P15 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS KTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P16 OR OIT0003.OILCODE = @P17 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K3TANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P18 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K5TANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P19 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K10TANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P20 OR OIT0003.OILCODE = @P21 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS LTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P22 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS ATANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE <> '' THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS TOTAL " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P10 THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS HTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P11 THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS RTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P12 THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS TTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P13 THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS MTTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P14 OR OIT0003.OILCODE = @P15 THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS KTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P16 OR OIT0003.OILCODE = @P17 THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K3TANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P18 THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K5TANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P19 THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K10TANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P20 OR OIT0003.OILCODE = @P21 THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS LTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P22 THEN ISNULL(OIM0005.VOLUME,0)ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS ATANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE <> '' THEN ISNULL(OIM0005.VOLUME,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS TOTALCNT " _
            & " FROM OIL.OIT0003_DETAIL OIT0003 " _
            & "  LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "  OIT0003.TANKNO = OIM0005.TANKNUMBER " _
            & " WHERE OIT0003.ORDERNO = @P01" _
            & "   AND OIT0003.DELFLG <> @P02"

        'SQLStr &=
        '      " ORDER BY" _
        '    & "    OIT0003.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 4) '油種(ハイオク)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 4) '油種(レギュラー)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 4) '油種(灯油)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 4) '油種(未添加灯油)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 4) '油種(軽油)
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 4) '油種(軽油)
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 4) '油種(３号軽油)
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 4) '油種(３号軽油)
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 4) '油種(５号軽油)
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 4) '油種(１０号軽油)
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 4) '油種(ＬＳＡ)
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 4) '油種(ＬＳＡ)
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 4) '油種(Ａ重油)
                PARA10.Value = BaseDllConst.CONST_HTank
                PARA11.Value = BaseDllConst.CONST_RTank
                PARA12.Value = BaseDllConst.CONST_TTank
                PARA13.Value = BaseDllConst.CONST_MTTank
                PARA14.Value = BaseDllConst.CONST_KTank1
                PARA15.Value = BaseDllConst.CONST_KTank2
                PARA16.Value = BaseDllConst.CONST_K3Tank1
                PARA17.Value = BaseDllConst.CONST_K3Tank2
                PARA18.Value = BaseDllConst.CONST_K5Tank
                PARA19.Value = BaseDllConst.CONST_K10Tank
                PARA20.Value = BaseDllConst.CONST_LTank1
                PARA21.Value = BaseDllConst.CONST_LTank2
                PARA22.Value = BaseDllConst.CONST_ATank

                '■　初期化
                '〇 油種別タンク車数(車)
                TxtHTank_c.Text = "0"
                TxtRTank_c.Text = "0"
                TxtTTank_c.Text = "0"
                TxtMTTank_c.Text = "0"
                TxtKTank_c.Text = "0"
                TxtK3Tank_c.Text = "0"
                TxtK5Tank_c.Text = "0"
                TxtK10Tank_c.Text = "0"
                TxtLTank_c.Text = "0"
                TxtATank_c.Text = "0"
                TxtTotal_c.Text = "0"
                '〇 積込数量(kl)
                TxtHTank_c2.Text = "0"
                TxtRTank_c2.Text = "0"
                TxtTTank_c2.Text = "0"
                TxtMTTank_c2.Text = "0"
                TxtKTank_c2.Text = "0"
                TxtK3Tank_c2.Text = "0"
                TxtK5Tank_c2.Text = "0"
                TxtK10Tank_c2.Text = "0"
                TxtLTank_c2.Text = "0"
                TxtATank_c2.Text = "0"
                TxtTotalCnt_c2.Text = "0"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003WKrow As DataRow In OIT0003WKtbl.Rows
                    i += 1
                    OIT0003WKrow("LINECNT") = i        'LINECNT

                    '〇 油種別タンク車数(車)
                    TxtHTank_c.Text = OIT0003WKrow("HTANK")
                    TxtRTank_c.Text = OIT0003WKrow("RTANK")
                    TxtTTank_c.Text = OIT0003WKrow("TTANK")
                    TxtMTTank_c.Text = OIT0003WKrow("MTTANK")
                    TxtKTank_c.Text = OIT0003WKrow("KTANK")
                    TxtK3Tank_c.Text = OIT0003WKrow("K3TANK")
                    TxtK5Tank_c.Text = OIT0003WKrow("K5TANK")
                    TxtK10Tank_c.Text = OIT0003WKrow("K10TANK")
                    TxtLTank_c.Text = OIT0003WKrow("LTANK")
                    TxtATank_c.Text = OIT0003WKrow("ATANK")
                    TxtTotal_c.Text = OIT0003WKrow("TOTAL")
                    '〇 積込数量(kl)
                    TxtHTank_c2.Text = OIT0003WKrow("HTANKCNT")
                    TxtRTank_c2.Text = OIT0003WKrow("RTANKCNT")
                    TxtTTank_c2.Text = OIT0003WKrow("TTANKCNT")
                    TxtMTTank_c2.Text = OIT0003WKrow("MTTANKCNT")
                    TxtKTank_c2.Text = OIT0003WKrow("KTANKCNT")
                    TxtK3Tank_c2.Text = OIT0003WKrow("K3TANKCNT")
                    TxtK5Tank_c2.Text = OIT0003WKrow("K5TANKCNT")
                    TxtK10Tank_c2.Text = OIT0003WKrow("K10TANKCNT")
                    TxtLTank_c2.Text = OIT0003WKrow("LTANKCNT")
                    TxtATank_c2.Text = OIT0003WKrow("ATANKCNT")
                    TxtTotalCnt_c2.Text = OIT0003WKrow("TOTALCNT")
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()
        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()

            Case "WF_UORG"              '運用部署
                WF_UORG.Text = WW_SelectValue
                WF_UORG_TEXT.Text = WW_SelectText
                WF_UORG.Focus()

            '荷主
            Case "TxtShippersCode"
                TxtShippersCode.Text = WW_SelectValue
                LblShippersName.Text = WW_SelectText
                work.WF_SEL_SHIPPERSCODE.Text = WW_SelectValue
                work.WF_SEL_SHIPPERSNAME.Text = WW_SelectText
                TxtShippersCode.Focus()

            '荷受人
            Case "TxtConsigneeCode"
                TxtConsigneeCode.Text = WW_SelectValue
                LblConsigneeName.Text = WW_SelectText
                work.WF_SEL_CONSIGNEECODE.Text = WW_SelectValue
                work.WF_SEL_CONSIGNEENAME.Text = WW_SelectText
                TxtConsigneeCode.Focus()

            'Case "TxtOrderOffice"      '受注営業所
            '    '別の受注営業所が設定されて場合
            '    If TxtOrderOffice.Text <> WW_SelectText Then
            '        TxtOrderOffice.Text = WW_SelectText
            '        work.WF_SEL_SALESOFFICECODE.Text = WW_SelectValue
            '        work.WF_SEL_SALESOFFICE.Text = WW_SelectText

            '        '○ 本線列車, 発駅, 着駅のテキストボックスを初期化
            '        TxtHeadOfficeTrain.Text = ""
            '        TxtDepstation.Text = ""
            '        LblDepstationName.Text = ""
            '        TxtArrstation.Text = ""
            '        LblArrstationName.Text = ""

            '        '○ 油種別タンク車数(車)の件数を初期化
            '        TxtTotalTank.Text = "0"
            '        TxtHTank.Text = "0"
            '        TxtRTank.Text = "0"
            '        TxtTTank.Text = "0"
            '        TxtMTTank.Text = "0"
            '        TxtKTank.Text = "0"
            '        TxtK3Tank.Text = "0"
            '        TxtK5Tank.Text = "0"
            '        TxtK10Tank.Text = "0"
            '        TxtLTank.Text = "0"
            '        TxtATank.Text = "0"

            '        '〇営業所配下情報を取得・設定
            '        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstation.Text, WW_GetValue)
            '        work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
            '        work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
            '        work.WF_SEL_BASECODE.Text = WW_GetValue(2)
            '        work.WF_SEL_BASENAME.Text = WW_GetValue(3)
            '        work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
            '        work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)

            '        '○ 一覧の初期化画面表示データ取得
            '        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            '            SQLcon.Open()       'DataBase接続

            '            MAPDataGet(SQLcon, 0)
            '        End Using

            '        '○ 画面表示データ保存
            '        Master.SaveTable(OIT0001tbl)

            '    End If

            '    '新規作成の場合(油種別タンク車数のテキストボックスの入力を可とする。)
            '    If work.WF_SEL_CREATEFLG.Text = "1" Then
            '        WW_ScreenEnabledSet()
            '    End If
            '    TxtOrderOffice.Focus()

            Case "TxtTrainNo"   '本線列車
                '                TxtHeadOfficeTrain.Text = WW_SelectValue.Substring(0, 4)
                TxtTrainNo.Text = WW_SelectValue
                'WW_FixvalueMasterSearch("", "TRAINNUMBER", WW_SelectValue, WW_GetValue)

                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    WW_FixvalueMasterSearch(Master.USER_ORG + WF_SelectedIndex.Value, "TRAINNUMBER", WW_SelectValue, WW_GetValue, I_PARA01:=WF_SelectedIndex.Value)
                    'WW_FixvalueMasterSearch(Master.USER_ORG, "TRAINNUMBER", WW_SelectValue, WW_GetValue, I_PARA01:=WF_SelectedIndex.Value)
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER", WW_SelectValue, WW_GetValue)
                End If

                '発駅
                TxtDepstationCode.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", TxtDepstationCode.Text, LblDepstationName.Text, WW_DUMMY)
                '着駅
                TxtArrstationCode.Text = WW_GetValue(2)
                CODENAME_get("ARRSTATION", TxtArrstationCode.Text, LblArrstationName.Text, WW_DUMMY)
                TxtTrainNo.Focus()

                '〇営業所配下情報を取得・設定
                WW_GetValue = {"", "", "", "", "", "", "", ""}

                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                End If

                '荷主
                TxtShippersCode.Text = WW_GetValue(0)
                LblShippersName.Text = WW_GetValue(1)
                '荷受人
                TxtConsigneeCode.Text = WW_GetValue(4)
                LblConsigneeName.Text = WW_GetValue(5)
                '受注パターン
                TxtOrderType.Text = WW_GetValue(7)

                '〇 (予定)の日付を設定
                TxtLoadingDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtDepDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtArrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtAccDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtEmparrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")

                work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
                work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
                work.WF_SEL_BASECODE.Text = WW_GetValue(2)
                work.WF_SEL_BASENAME.Text = WW_GetValue(3)
                work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
                work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)
                work.WF_SEL_PATTERNCODE.Text = WW_GetValue(6)
                work.WF_SEL_PATTERNNAME.Text = WW_GetValue(7)

            Case "TxtDepstationCode"        '発駅
                TxtDepstationCode.Text = WW_SelectValue
                LblDepstationName.Text = WW_SelectText
                TxtDepstationCode.Focus()

            Case "TxtArrstationCode"        '着駅
                TxtArrstationCode.Text = WW_SelectValue
                LblArrstationName.Text = WW_SelectText
                TxtArrstationCode.Focus()

                '〇営業所配下情報を取得・設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                End If
                TxtShippersCode.Text = WW_GetValue(0)
                LblShippersName.Text = WW_GetValue(1)
                TxtConsigneeCode.Text = WW_GetValue(4)
                LblConsigneeName.Text = WW_GetValue(5)
                TxtOrderType.Text = WW_GetValue(7)

                work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
                work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
                work.WF_SEL_BASECODE.Text = WW_GetValue(2)
                work.WF_SEL_BASENAME.Text = WW_GetValue(3)
                work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
                work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)
                work.WF_SEL_PATTERNCODE.Text = WW_GetValue(6)
                work.WF_SEL_PATTERNNAME.Text = WW_GetValue(7)

            Case "TxtLoadingDate"       '(予定)積込日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtLoadingDate.Text = ""
                    Else
                        TxtLoadingDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtLoadingDate.Focus()
            Case "TxtDepDate"           '(予定)発日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtDepDate.Text = ""
                    Else
                        TxtDepDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtDepDate.Focus()
            Case "TxtArrDate"           '(予定)積車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtArrDate.Text = ""
                    Else
                        TxtArrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtArrDate.Focus()
            Case "TxtAccDate"           '(予定)受入日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtAccDate.Text = ""
                    Else
                        TxtAccDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtAccDate.Focus()

            Case "TxtEmparrDate"           '(予定)空車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtEmparrDate.Text = ""
                    Else
                        TxtEmparrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtEmparrDate.Focus()

            Case "TxtActualLoadingDate"       '(実績)積込日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtActualLoadingDate.Text = ""
                    Else
                        TxtActualLoadingDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtActualLoadingDate.Focus()
            Case "TxtActualDepDate"           '(実績)発日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtActualDepDate.Text = ""
                    Else
                        TxtActualDepDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtActualDepDate.Focus()
            Case "TxtActualArrDate"           '(実績)積車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtActualArrDate.Text = ""
                    Else
                        TxtActualArrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtActualArrDate.Focus()
            Case "TxtActualAccDate"           '(実績)受入日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtActualAccDate.Text = ""
                    Else
                        TxtActualAccDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtActualAccDate.Focus()

            Case "TxtActualEmparrDate"        '(実績)空車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtActualEmparrDate.Text = ""
                    Else
                        TxtActualEmparrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtActualEmparrDate.Focus()

                '(一覧)荷主, (一覧)油種, (一覧)タンク車№
            Case "SHIPPERSNAME", "OILNAME", "TANKNO"
                '○ LINECNT取得
                Dim WW_LINECNT As Integer = 0
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                '○ 設定項目取得
                Dim WW_SETTEXT As String = WW_SelectText
                Dim WW_SETVALUE As String = WW_SelectValue

                '○ 画面表示データ復元
                If Not Master.RecoverTable(OIT0003tbl) Then Exit Sub

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0003tbl.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                If IsNothing(updHeader) Then Exit Sub

                '〇 一覧項目へ設定
                '荷主名を一覧に設定
                If WF_FIELD.Value = "SHIPPERSNAME" Then
                    updHeader.Item("SHIPPERSCODE") = WW_SETVALUE
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                    '油種名を一覧に設定
                ElseIf WF_FIELD.Value = "OILNAME" Then
                    updHeader.Item("OILCODE") = WW_SETVALUE
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                    '〇 タンク車割当状況チェック
                    WW_TANKQUOTACHK(updHeader)

                    ''タンク車割当状況＝"割当"の場合
                    'If updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI Then

                    '    '油種が削除("")の場合
                    '    If updHeader.Item("OILCODE") = "" Then
                    '        'タンク車割当状況＝"残車"に設定
                    '        updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN
                    '    End If

                    '    'タンク車割当状況＝"残車"の場合
                    'ElseIf updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN Then
                    '    '油種が設定された場合
                    '    If updHeader.Item("OILCODE") <> "" Then
                    '        'タンク車割当状況＝"割当"に設定
                    '        updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI
                    '    End If
                    'End If

                    'タンク車№を一覧に設定
                ElseIf WF_FIELD.Value = "TANKNO" Then
                    'Dim WW_TANKNUMBER As String = WW_SETTEXT.Substring(0, 8).Replace("-", "")
                    Dim WW_TANKNUMBER As String = WW_SETVALUE
                    Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                    updHeader.Item(WF_FIELD.Value) = WW_TANKNUMBER

                    WW_FixvalueMasterSearch("", "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)

                    '前回油種
                    Dim WW_LASTOILNAME As String = ""
                    updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                    CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                    updHeader.Item("LASTOILNAME") = WW_LASTOILNAME

                    '交検日
                    Dim WW_JRINSPECTIONCNT As String
                    updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(2)
                    If WW_GetValue(2) <> "" Then
                        WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(2)))

                        Dim WW_JRINSPECTIONFLG As String
                        If WW_JRINSPECTIONCNT <= 3 Then
                            WW_JRINSPECTIONFLG = "1"
                        ElseIf WW_JRINSPECTIONCNT >= 4 And WW_JRINSPECTIONCNT <= 6 Then
                            WW_JRINSPECTIONFLG = "2"
                        Else
                            WW_JRINSPECTIONFLG = "3"
                        End If
                        Select Case WW_JRINSPECTIONFLG
                            Case "1"
                                updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            Case "2"
                                updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            Case "3"
                                updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                        End Select
                    Else
                        updHeader.Item("JRINSPECTIONALERT") = ""
                    End If

                    '全検日
                    Dim WW_JRALLINSPECTIONCNT As String
                    updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(3)
                    If WW_GetValue(3) <> "" Then
                        WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(3)))

                        Dim WW_JRALLINSPECTIONFLG As String
                        If WW_JRALLINSPECTIONCNT <= 3 Then
                            WW_JRALLINSPECTIONFLG = "1"
                        ElseIf WW_JRALLINSPECTIONCNT >= 4 And WW_JRALLINSPECTIONCNT <= 6 Then
                            WW_JRALLINSPECTIONFLG = "2"
                        Else
                            WW_JRALLINSPECTIONFLG = "3"
                        End If
                        Select Case WW_JRALLINSPECTIONFLG
                            Case "1"
                                updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            Case "2"
                                updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            Case "3"
                                updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                        End Select
                    Else
                        updHeader.Item("JRALLINSPECTIONALERT") = ""
                    End If

                End If
                'updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl) Then Exit Sub

        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()
        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"               '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_UORG"                   '運用部署
                WF_UORG.Focus()
            Case "TxtShippersCode"           '荷主
                TxtShippersCode.Focus()
            Case "TxtConsigneeCode"          '荷受人
                TxtConsigneeCode.Focus()
            Case "TxtTrainNo"                '本線列車
                TxtTrainNo.Focus()
            Case "TxtDepstationCode"         '発駅
                TxtDepstationCode.Focus()
            Case "TxtArrstationCode"         '着駅
                TxtArrstationCode.Focus()
            Case "TxtLoadingDate"            '(予定)積込日
                TxtLoadingDate.Focus()
            Case "TxtDepDate"                '(予定)発日
                TxtDepDate.Focus()
            Case "TxtArrDate"                '(予定)積車着日
                TxtArrDate.Focus()
            Case "TxtAccDate"                '(予定)受入日
                TxtAccDate.Focus()
            Case "TxtEmparrDate"             '(予定)空車着日
                TxtEmparrDate.Focus()
            Case "TxtActualLoadingDate"      '(実績)積込日
                TxtActualLoadingDate.Focus()
            Case "TxtActualDepDate"          '(実績)発日
                TxtActualDepDate.Focus()
            Case "TxtActualArrDate"          '(実績)積車着日
                TxtActualArrDate.Focus()
            Case "TxtActualAccDate"          '(実績)受入日
                TxtActualAccDate.Focus()
            Case "TxtActualEmparrDate"       '(実績)空車着日
                TxtActualEmparrDate.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' マスタ検索処理
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="O_VALUE"></param>
    Protected Sub WW_FixvalueMasterSearch(ByVal I_CODE As String,
                                          ByVal I_CLASS As String,
                                          ByVal I_KEYCODE As String,
                                          ByRef O_VALUE() As String,
                                          Optional ByVal I_PARA01 As String = Nothing)

        If IsNothing(OIT0003Fixvaltbl) Then
            OIT0003Fixvaltbl = New DataTable
        End If

        If OIT0003Fixvaltbl.Columns.Count <> 0 Then
            OIT0003Fixvaltbl.Columns.Clear()
        End If

        OIT0003Fixvaltbl.Clear()

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
               " SELECT" _
                & "   ISNULL(RTRIM(VIW0001.CAMPCODE), '   ') AS CAMPCODE" _
                & " , ISNULL(RTRIM(VIW0001.CLASS), '   ')    AS CLASS" _
                & " , ISNULL(RTRIM(VIW0001.KEYCODE), '   ')  AS KEYCODE" _
                & " , ISNULL(RTRIM(VIW0001.STYMD), '   ')    AS STYMD" _
                & " , ISNULL(RTRIM(VIW0001.ENDYMD), '   ')   AS ENDYMD" _
                & " , ISNULL(RTRIM(VIW0001.VALUE1), '   ')   AS VALUE1" _
                & " , ISNULL(RTRIM(VIW0001.VALUE2), '   ')   AS VALUE2" _
                & " , ISNULL(RTRIM(VIW0001.VALUE3), '   ')   AS VALUE3" _
                & " , ISNULL(RTRIM(VIW0001.VALUE4), '   ')   AS VALUE4" _
                & " , ISNULL(RTRIM(VIW0001.VALUE5), '   ')   AS VALUE5" _
                & " , ISNULL(RTRIM(VIW0001.VALUE6), '   ')   AS VALUE6" _
                & " , ISNULL(RTRIM(VIW0001.VALUE7), '   ')   AS VALUE7" _
                & " , ISNULL(RTRIM(VIW0001.VALUE8), '   ')   AS VALUE8" _
                & " , ISNULL(RTRIM(VIW0001.SYSTEMKEYFLG), '   ')   AS SYSTEMKEYFLG" _
                & " , ISNULL(RTRIM(VIW0001.DELFLG), '   ')   AS DELFLG" _
                & " FROM  OIL.VIW0001_FIXVALUE VIW0001" _
                & " WHERE VIW0001.CLASS = @P01" _
                & " AND VIW0001.DELFLG <> @P03"

            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            '会社コード
            If Not String.IsNullOrEmpty(I_CODE) Then
                SQLStr &= String.Format("    AND VIW0001.CAMPCODE = '{0}'", I_CODE)
            End If
            'マスターキー
            If Not String.IsNullOrEmpty(I_KEYCODE) Then
                SQLStr &= String.Format("    AND VIW0001.KEYCODE = '{0}'", I_KEYCODE)
            End If

            SQLStr &=
                  " ORDER BY" _
                & "    VIW0001.KEYCODE"

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                'Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)

                PARA01.Value = I_CLASS
                'PARA02.Value = I_KEYCODE
                PARA03.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003Fixvaltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003Fixvaltbl.Load(SQLdr)
                End Using

                If I_KEYCODE.Equals("") Then
                    Dim i As Integer = 0
                    For Each OIT0001WKrow As DataRow In OIT0003Fixvaltbl.Rows
                        O_VALUE(i) = OIT0001WKrow("KEYCODE")
                        i += 1
                    Next
                    '    '〇〇部、支店対応
                    'ElseIf I_PARA01 <> "" Then
                    '    For Each OIT0001WKrow As DataRow In OIT0003Fixvaltbl.Rows
                    '        If OIT0001WKrow("SYSTEMKEYFLG") = I_PARA01 Then
                    '            O_VALUE(0) = OIT0001WKrow("VALUE1")
                    '            O_VALUE(1) = OIT0001WKrow("VALUE2")
                    '            O_VALUE(2) = OIT0001WKrow("VALUE3")
                    '            O_VALUE(3) = OIT0001WKrow("VALUE4")
                    '            O_VALUE(4) = OIT0001WKrow("VALUE5")
                    '            O_VALUE(5) = OIT0001WKrow("VALUE6")
                    '            O_VALUE(6) = OIT0001WKrow("VALUE7")
                    '            O_VALUE(7) = OIT0001WKrow("VALUE8")
                    '        End If
                    '    Next
                Else
                    For Each OIT0001WKrow As DataRow In OIT0003Fixvaltbl.Rows
                        O_VALUE(0) = OIT0001WKrow("VALUE1")
                        O_VALUE(1) = OIT0001WKrow("VALUE2")
                        O_VALUE(2) = OIT0001WKrow("VALUE3")
                        O_VALUE(3) = OIT0001WKrow("VALUE4")
                        O_VALUE(4) = OIT0001WKrow("VALUE5")
                        O_VALUE(5) = OIT0001WKrow("VALUE6")
                        O_VALUE(6) = OIT0001WKrow("VALUE7")
                        O_VALUE(7) = OIT0001WKrow("VALUE8")
                    Next
                End If
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 画面表示設定処理
    ''' </summary>
    Protected Sub WW_ScreenEnabledSet()

        ''〇 タブの使用可否制御
        'If work.WF_SEL_ORDERSTATUS.Text = "100" Then
        '    WF_Dtab01.Enabled = True
        '    WF_Dtab02.Enabled = False
        '    WF_Dtab03.Enabled = False
        '    WF_Dtab04.Enabled = False

        'ElseIf work.WF_SEL_ORDERSTATUS.Text = "200" _
        '    OrElse work.WF_SEL_ORDERSTATUS.Text = "210" _
        '    OrElse work.WF_SEL_ORDERSTATUS.Text = "220" _
        '    OrElse work.WF_SEL_ORDERSTATUS.Text = "230" _
        '    OrElse work.WF_SEL_ORDERSTATUS.Text = "240" _
        '    OrElse work.WF_SEL_ORDERSTATUS.Text = "250" _
        '    OrElse work.WF_SEL_ORDERSTATUS.Text = "260" Then
        '    WF_Dtab01.Enabled = True
        '    WF_Dtab02.Enabled = False
        '    WF_Dtab03.Enabled = True
        '    WF_Dtab04.Enabled = False

        'Else
        '    WF_Dtab01.Enabled = False
        '    WF_Dtab02.Enabled = True
        '    WF_Dtab03.Enabled = False
        '    WF_Dtab04.Enabled = False

        'End If

        '○ 油種別タンク車数(車)、積込数量(kl)、計上月、売上金額、支払金額の表示・非表示制御
        '権限コードが更新の場合は表示設定
        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            pnlSummaryArea.Visible = True
        Else
            pnlSummaryArea.Visible = False
        End If

        '〇 (実績)の日付の入力可否制御
        '受注情報が以下の場合は、(実績)の日付の入力を制限
        '100:受注受付, 200:手配, 210:手配中（入換指示手配済）, 220:手配中（積込指示手配済）
        '230:手配中（託送指示手配済）, 240:手配中（入換指示未手配）, 250:手配中（積込指示未手配）
        '260;手配中（託送指示未手配）, 270:手配完了
        If work.WF_SEL_ORDERSTATUS.Text = "100" _
            OrElse work.WF_SEL_ORDERSTATUS.Text = "200" _
            OrElse work.WF_SEL_ORDERSTATUS.Text = "210" _
            OrElse work.WF_SEL_ORDERSTATUS.Text = "220" _
            OrElse work.WF_SEL_ORDERSTATUS.Text = "230" _
            OrElse work.WF_SEL_ORDERSTATUS.Text = "240" _
            OrElse work.WF_SEL_ORDERSTATUS.Text = "250" _
            OrElse work.WF_SEL_ORDERSTATUS.Text = "260" _
            OrElse work.WF_SEL_ORDERSTATUS.Text = "270" Then

            '(実績)積込日
            TxtActualLoadingDate.Enabled = False
            '(実績)発日
            TxtActualDepDate.Enabled = False
            '(実績)積車着日
            TxtActualArrDate.Enabled = False
            '(実績)受入日
            TxtActualAccDate.Enabled = False
            '(実績)空車着日
            TxtActualEmparrDate.Enabled = False
        Else
            '(実績)積込日
            TxtActualLoadingDate.Enabled = True
            '(実績)発日
            TxtActualDepDate.Enabled = True
            '(実績)積車着日
            TxtActualArrDate.Enabled = True
            '(実績)受入日
            TxtActualAccDate.Enabled = True
            '(実績)空車着日
            TxtActualEmparrDate.Enabled = True
        End If

    End Sub

    ''' <summary>
    ''' タンク車割当状況チェック
    ''' </summary>
    Protected Sub WW_TANKQUOTACHK(ByVal I_updHeader As DataRow)

        'タンク車割当状況＝"割当"の場合
        If I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI Then

            '油種が削除("")の場合
            If I_updHeader.Item("OILCODE") = "" Then
                'タンク車割当状況＝"残車"に設定
                I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN
            End If

            'タンク車割当状況＝"残車"の場合
        ElseIf I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN Then
            '油種が設定された場合
            If I_updHeader.Item("OILCODE") <> "" Then
                'タンク車割当状況＝"割当"に設定
                I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI
            End If
        End If

    End Sub


    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        WW_ERR_MES &= ControlChars.NewLine & "  --> オーダー№         =" & TxtOrderNo.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車           =" & TxtTrainNo.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 発駅               =" & TxtDepstationCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 着駅               =" & TxtArrstationCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)積込日       =" & TxtLoadingDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)発日         =" & TxtDepDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)積車着日     =" & TxtArrDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)受入日       =" & TxtAccDate.Text
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)空車着日     =" & TxtEmparrDate.Text

        rightview.SetErrorReport(WW_ERR_MES)

    End Sub

End Class
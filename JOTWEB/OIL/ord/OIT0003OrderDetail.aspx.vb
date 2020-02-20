'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0003OrderDetail
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0003tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0003tbl_tab1 As DataTable                            '一覧格納用テーブル(タブ１用)
    Private OIT0003tbl_tab2 As DataTable                            '一覧格納用テーブル(タブ２用)
    Private OIT0003tbl_tab3 As DataTable                            '一覧格納用テーブル(タブ３用)
    Private OIT0003tbl_tab4 As DataTable                            '一覧格納用テーブル(タブ４用)
    Private OIT0003INPtbl As DataTable                              'チェック用テーブル
    Private OIT0003UPDtbl As DataTable                              '更新用テーブル
    Private OIT0003WKtbl As DataTable                               '作業用テーブル
    Private OIT0003WK2tbl As DataTable                              '作業用2テーブル
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

    Private WW_RINKAIFLG As Boolean = False                         '臨海鉄道対象可否(TRUE：対象, FALSE:未対象)

    Private WW_SwapInput As String = "0"                            '入換指示入力(0:未 1:完了)
    Private WW_LoadingInput As String = "0"                         '積込指示入力(0:未 1:完了)

    Private WW_ORDERINFOFLG_10 As Boolean = False                   '受注情報セット可否(情報(10:積置))
    Private WW_ORDERINFOALERMFLG_80 As Boolean = False              '受注情報セット可否(警告(80:タンク車数オーバー))
    Private WW_ORDERINFOALERMFLG_82 As Boolean = False              '受注情報セット可否(警告(82:検査間近あり))

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0003tbl)
                    Master.RecoverTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
                    Master.RecoverTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
                    'Master.RecoverTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonDELIVERY"              '託送指示ボタン押下
                            WF_ButtonDELIVERY_Click()
                        Case "WF_ButtonINSERT"                '油種数登録ボタン押下
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
                        Case "WF_ButtonUPDATE_TAB1",          '更新ボタン押下
                             "WF_ButtonUPDATE_TAB2",
                             "WF_ButtonUPDATE_TAB3",
                             "WF_ButtonUPDATE_TAB4"
                            WF_ButtonUPDATE_Click()
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

            '○ 託送指示フラグ(0：未手配, 1：手配)設定
            '　・100:受注受付の状態では、非活性とする。
            If work.WF_SEL_DELIVERYFLG.Text = "1" _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 Then
                WF_DELIVERYFLG.Value = "1"
            Else
                WF_DELIVERYFLG.Value = "0"
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
        '〇 受注進行ステータスが"受注受付"の場合
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then
            WF_DTAB_CHANGE_NO.Value = "0"
            WF_DetailMView.ActiveViewIndex = WF_DTAB_CHANGE_NO.Value

            '〇 受注進行ステータスが下記内容へ変更された場合
            '   受注進行ステータス＝"200:手配中"
            '   受注進行ステータス＝"210:手配中(入換指示手配済)"
            '   受注進行ステータス＝"220:手配中(積込指示手配済)"
            '   受注進行ステータス＝"230:手配中(託送指示手配済)"
            '   受注進行ステータス＝"240:手配中(入換指示未手配)"
            '   受注進行ステータス＝"250:手配中(積込指示未手配)"
            '   受注進行ステータス＝"260:手配中(託送指示未手配)"
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_210 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_220 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_230 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_240 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_250 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 Then
            WF_DTAB_CHANGE_NO.Value = "1"
            WF_DetailMView.ActiveViewIndex = WF_DTAB_CHANGE_NO.Value

            '〇 (一覧)テキストボックスの制御(読取専用)
            WW_ListTextBoxReadControl()

            '〇 受注進行ステータスが"手配完了"へ変更された場合
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 Then
            WF_DTAB_CHANGE_NO.Value = "2"
            WF_DetailMView.ActiveViewIndex = WF_DTAB_CHANGE_NO.Value

            '〇 (一覧)テキストボックスの制御(読取専用)
            WW_ListTextBoxReadControl()

        Else
            WF_DTAB_CHANGE_NO.Value = "0"
            WF_DetailMView.ActiveViewIndex = WF_DTAB_CHANGE_NO.Value

        End If

        '〇 タブ切替
        WF_Detail_TABChange()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○ 遷移先(各タブ)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '受注営業所
        '作成(貨車連結用)フラグ(２：更新)　かつ、作成モード(１：新規)
        If work.WF_SEL_CREATELINKFLG.Text = "2" _
            AndAlso work.WF_SEL_CREATEFLG.Text = "1" Then
            TxtOrderOfficeCode.Text = work.WF_SEL_LINK_ORDERSALESOFFICE.Text
            CODENAME_get("SALESOFFICE", TxtOrderOfficeCode.Text, TxtOrderOffice.Text, WW_RTN_SW)

            work.WF_SEL_ORDERSALESOFFICE.Text = TxtOrderOffice.Text
            work.WF_SEL_ORDERSALESOFFICECODE.Text = TxtOrderOfficeCode.Text

            '作成モード(２：更新)
        ElseIf work.WF_SEL_CREATEFLG.Text = "2" Then
            TxtOrderOffice.Text = work.WF_SEL_ORDERSALESOFFICE.Text
            TxtOrderOfficeCode.Text = work.WF_SEL_ORDERSALESOFFICECODE.Text

            '作成モード(１：新規登録)
        Else
            TxtOrderOffice.Text = work.WF_SEL_SALESOFFICE.Text
            TxtOrderOfficeCode.Text = work.WF_SEL_SALESOFFICECODE.Text

        End If

        'ステータス
        If work.WF_SEL_ORDERSTATUSNM.Text = "" Then
            work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100
            CODENAME_get("ORDERSTATUS", BaseDllConst.CONST_ORDERSTATUS_100, work.WF_SEL_ORDERSTATUSNM.Text, WW_DUMMY)
        End If
        TxtOrderStatus.Text = work.WF_SEL_ORDERSTATUSNM.Text

        '情報
        If work.WF_SEL_STACKINGFLG.Text = "" Then
            work.WF_SEL_STACKINGFLG.Text = "2"
        End If

        '〇 積置可否フラグ(１：積置あり, ２：積置なし)
        If work.WF_SEL_STACKINGFLG.Text = "1" Then
            chkOrderInfo.Checked = True
        Else
            chkOrderInfo.Checked = False
        End If

        '受注パターン
        CODENAME_get("ORDERTYPE", work.WF_SEL_PATTERNCODE.Text, work.WF_SEL_PATTERNNAME.Text, WW_DUMMY)
        TxtOrderType.Text = work.WF_SEL_PATTERNNAME.Text

        'オーダー№
        If work.WF_SEL_ORDERNUMBER.Text = "" Then
            Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
            WW_FixvalueMasterSearch("", "NEWORDERNOGET", "", WW_GetValue)
            work.WF_SEL_ORDERNUMBER.Text = WW_GetValue(0)
            TxtOrderNo.Text = work.WF_SEL_ORDERNUMBER.Text
        Else
            TxtOrderNo.Text = work.WF_SEL_ORDERNUMBER.Text
        End If

        '〇 作成(貨車連結用)フラグ(２：更新)　かつ、作成モード(１：新規)
        If work.WF_SEL_CREATELINKFLG.Text = "2" _
            AndAlso work.WF_SEL_CREATEFLG.Text = "1" Then

            TxtTrainNo.Text = work.WF_SEL_LINK_TRAIN.Text
            TxtTrainName.Text = work.WF_SEL_LINK_TRAINNAME.Text

            '〇 貨車連結表のみで作成の場合、取得した列車名から各値を取得し設定する。
            WW_TRAINNUMBER_FIND(work.WF_SEL_LINK_TRAINNAME.Text)
        Else
            '本線列車
            TxtTrainNo.Text = work.WF_SEL_TRAIN.Text
            TxtTrainName.Text = work.WF_SEL_TRAINNAME.Text
            '荷主
            TxtShippersCode.Text = work.WF_SEL_SHIPPERSCODE.Text
            '荷受人
            TxtConsigneeCode.Text = work.WF_SEL_CONSIGNEECODE.Text
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
        End If

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

        If work.WF_SEL_CREATEFLG.Text = "2" Then
            '売上合計金額(税抜)
            TxtTotalSales.Text = work.WF_SEL_SALSE.Text
            '支払合計金額(税抜)
            TxtTitalPayment.Text = work.WF_SEL_PAYMENT.Text
            '売上合計金額(税額)
            TxtTotalSales2.Text = work.WF_SEL_TOTALSALSE.Text
            '支払合計金額(税額)
            TxtTitalPayment2.Text = work.WF_SEL_TOTALPAYMENT.Text
        Else
            '売上合計金額(税抜)
            TxtTotalSales.Text = "0"
            '支払合計金額(税抜)
            TxtTitalPayment.Text = "0"
            '売上合計金額(税額)
            TxtTotalSales2.Text = "0"
            '支払合計金額(税額)
            TxtTitalPayment2.Text = "0"
        End If

        '● タブ「タンク車割当」
        '　■油種別タンク車数(車)
        If work.WF_SEL_CREATEFLG.Text = "2" Then
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
        Else
            'ハイオク(タンク車数)
            TxtHTank.Text = "0"
            'レギュラー(タンク車数)
            TxtRTank.Text = "0"
            '灯油(タンク車数)
            TxtTTank.Text = "0"
            '未添加灯油(タンク車数)
            TxtMTTank.Text = "0"
            '軽油(タンク車数)
            TxtKTank.Text = "0"
            '3号軽油(タンク車数)
            TxtK3Tank.Text = "0"
            '5号軽油(タンク車数)
            TxtK5Tank.Text = "0"
            '10号軽油(タンク車数)
            TxtK10Tank.Text = "0"
            'LSA(タンク車数)
            TxtLTank.Text = "0"
            'A重油(タンク車数)
            TxtATank.Text = "0"
        End If


        '　■割当後　油種別タンク車数(車)
        If work.WF_SEL_CREATEFLG.Text = "2" Then
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
        Else
            'ハイオク(タンク車数)
            TxtHTank_w.Text = "0"
            'レギュラー(タンク車数)
            TxtRTank_w.Text = "0"
            '灯油(タンク車数)
            TxtTTank_w.Text = "0"
            '未添加灯油(タンク車数)
            TxtMTTank_w.Text = "0"
            '軽油(タンク車数)
            TxtKTank_w.Text = "0"
            '3号軽油(タンク車数)
            TxtK3Tank_w.Text = "0"
            '5号軽油(タンク車数)
            TxtK5Tank_w.Text = "0"
            '10号軽油(タンク車数)
            TxtK10Tank_w.Text = "0"
            'LSA(タンク車数)
            TxtLTank_w.Text = "0"
            'A重油(タンク車数)
            TxtATank_w.Text = "0"
        End If

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

        'メニュー画面からの遷移の場合
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

        '〇タブ「タンク車割当」表示用
        GridViewInitializeTab1()

        '〇タブ「入換・積込指示」表示用
        GridViewInitializeTab2()

        '〇タブ「タンク車明細」表示用
        GridViewInitializeTab3()


    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「タンク車割当」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab1()
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

                MAPDataGetLinkTab1(SQLcon)
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
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        WF_DetailMView.ActiveViewIndex = 0
        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「入換・積込指示」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab2()
        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab2(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0003tbl_tab2)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB2"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea2
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「タンク車明細」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab3()
        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab3(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0003tbl_tab3)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB3"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea3
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"

        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

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
            & " , ''                                             AS ORDERINGTYPE" _
            & " , ''                                             AS ORDERINGOILNAME" _
            & " , @P05                                           AS TANKQUOTA" _
            & " , ''                                             AS LINKNO" _
            & " , ''                                             AS LINKDETAILNO" _
            & " , ''                                             AS LINEORDER" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS MODEL" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS LASTOILCODE" _
            & " , ''                                             AS LASTOILNAME" _
            & " , ''                                             AS PREORDERINGTYPE" _
            & " , ''                                             AS PREORDERINGOILNAME" _
            & " , ''                                             AS CHANGETRAINNO" _
            & " , ''                                             AS CHANGETRAINNAME" _
            & " , ''                                             AS SECONDCONSIGNEECODE" _
            & " , ''                                             AS SECONDCONSIGNEENAME" _
            & " , ''                                             AS SECONDARRSTATION" _
            & " , ''                                             AS SECONDARRSTATIONNAME" _
            & " , ''                                             AS CANGERETSTATION" _
            & " , ''                                             AS CHANGEARRSTATIONNAME" _
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
                & " , ISNULL(RTRIM(OIT0003.ORDERINFO), '')               AS ORDERINFO" _
                & " , CASE ISNULL(RTRIM(OIT0003.ORDERINFO), '')" _
                & "   WHEN '10' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '11' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '12' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   ELSE ISNULL(RTRIM(OIS0015_2.VALUE1), '')" _
                & "   END                                                AS ORDERINFONAME" _
                & " , ISNULL(RTRIM(OIT0003.OILCODE), '')                 AS OILCODE" _
                & " , ISNULL(RTRIM(OIT0003.OILNAME), '')                 AS OILNAME" _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')            AS ORDERINGTYPE" _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '')         AS ORDERINGOILNAME" _
                & " , CASE" _
                & "   WHEN OIT0003.TANKNO <> '' " _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P04" _
                & "   WHEN OIT0003.TANKNO <> '' " _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P04" _
                & "   WHEN ISNULL(RTRIM(OIT0003.TANKNO), '') <> '' THEN @P03" _
                & "   ELSE @P05" _
                & "   END                                                AS TANKQUOTA" _
                & " , ''                                                 AS LINKNO" _
                & " , ''                                                 AS LINKDETAILNO" _
                & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')               AS LINEORDER" _
                & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                  AS TANKNO" _
                & " , ISNULL(RTRIM(OIM0005.MODEL), '')                   AS MODEL" _
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
                & " , ISNULL(RTRIM(OIT0005.LASTOILNAME), '')                        AS LASTOILNAME" _
                & " , ISNULL(RTRIM(OIT0005.PREORDERINGTYPE), '')                    AS PREORDERINGTYPE" _
                & " , ISNULL(RTRIM(OIT0005.PREORDERINGOILNAME), '')                 AS PREORDERINGOILNAME" _
                & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNO), '')                      AS CHANGETRAINNO" _
                & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNAME), '')                    AS CHANGETRAINNAME" _
                & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEECODE), '')                AS SECONDCONSIGNEECODE" _
                & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEENAME), '')                AS SECONDCONSIGNEENAME" _
                & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATION), '')                   AS SECONDARRSTATION" _
                & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATIONNAME), '')               AS SECONDARRSTATIONNAME" _
                & " , ISNULL(RTRIM(OIT0003.CANGERETSTATION), '')                    AS CANGERETSTATION" _
                & " , ISNULL(RTRIM(OIT0003.CHANGEARRSTATIONNAME), '')               AS CHANGEARRSTATIONNAME" _
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
                & "    AND OIS0015_2.KEYCODE = OIT0003.ORDERINFO " _
                & " WHERE OIT0002.ORDERNO = @P01" _
                & " AND OIT0002.DELFLG <> @P02"
            '& " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_NOW ON " _
            '& "       OIT0002.OFFICECODE = OIM0003_NOW.OFFICECODE" _
            '& "       AND OIT0002.SHIPPERSCODE = OIM0003_NOW.SHIPPERCODE" _
            '& "       AND OIT0002.BASECODE = OIM0003_NOW.PLANTCODE" _
            '& "       AND OIT0003.OILCODE = OIM0003_NOW.OILCODE" _
            '& "       AND OIM0003_NOW.DELFLG <> @P02" _
            '& " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_PAST ON " _
            '& "       OIT0002.OFFICECODE = OIM0003_PAST.OFFICECODE" _
            '& "       AND OIT0002.SHIPPERSCODE = OIM0003_PAST.SHIPPERCODE" _
            '& "       AND OIT0002.BASECODE = OIM0003_PAST.PLANTCODE" _
            '& "       AND OIT0005.LASTOILCODE = OIM0003_PAST.OILCODE" _
            '& "       AND OIM0003_PAST.DELFLG <> @P02" _

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
    Protected Sub MAPDataGetLinkTab1(ByVal SQLcon As SqlConnection)

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
            & " , ISNULL(RTRIM(TMP0001.ORDERINGTYPE), '')                       AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINGOILNAME), '')                    AS ORDERINGOILNAME" _
            & " , CASE" _
            & "   WHEN (OIT0004.TANKNUMBER IS NULL OR TMP0001.TANKNO IS NULL) " _
            & "    AND TMP0001.OILNAME IS NULL THEN @P04" _
            & "   WHEN (OIT0004.TANKNUMBER <> '' OR TMP0001.TANKNO <> '') " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN (OIT0004.TANKNUMBER <> '' OR TMP0001.TANKNO <> '') " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN (OIT0004.TANKNUMBER IS NOT NULL OR TMP0001.TANKNO IS NOT NULL) " _
            & "    AND TMP0001.OILCODE IS NULL THEN @P04" _
            & "   WHEN (OIT0004.TANKNUMBER IS NOT NULL OR TMP0001.TANKNO IS NOT NULL) THEN @P06" _
            & "   ELSE @P07" _
            & "   END                                                           AS TANKQUOTA" _
            & " , ISNULL(RTRIM(OIT0004.LINKNO), '')                             AS LINKNO" _
            & " , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')                       AS LINKDETAILNO" _
            & " , ISNULL(RTRIM(OIT0004.LINEORDER), TMP0001.LINEORDER)           AS LINEORDER" _
            & " , ISNULL(RTRIM(OIM0005.TANKNUMBER), '')                         AS TANKNO" _
            & " , ISNULL(RTRIM(OIM0005.MODEL), '')                              AS MODEL" _
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
            & " , ISNULL(RTRIM(OIT0004.PREOILCODE), OIT0005.LASTOILCODE)                AS LASTOILCODE" _
            & " , ISNULL(RTRIM(OIT0004.PREOILNAME), OIT0005.LASTOILNAME)                AS LASTOILNAME" _
            & " , ISNULL(RTRIM(OIT0004.PREORDERINGTYPE), OIT0005.PREORDERINGTYPE)       AS PREORDERINGTYPE" _
            & " , ISNULL(RTRIM(OIT0004.PREORDERINGOILNAME), OIT0005.PREORDERINGOILNAME) AS PREORDERINGOILNAME" _
            & " , ISNULL(RTRIM(TMP0001.CHANGETRAINNO), '')                      AS CHANGETRAINNO" _
            & " , ISNULL(RTRIM(TMP0001.CHANGETRAINNAME), '')                    AS CHANGETRAINNAME" _
            & " , ISNULL(RTRIM(TMP0001.SECONDCONSIGNEECODE), '')                AS SECONDCONSIGNEECODE" _
            & " , ISNULL(RTRIM(TMP0001.SECONDCONSIGNEENAME), '')                AS SECONDCONSIGNEENAME" _
            & " , ISNULL(RTRIM(TMP0001.SECONDARRSTATION), '')                   AS SECONDARRSTATION" _
            & " , ISNULL(RTRIM(TMP0001.SECONDARRSTATIONNAME), '')               AS SECONDARRSTATIONNAME" _
            & " , ISNULL(RTRIM(TMP0001.CANGERETSTATION), '')                    AS CANGERETSTATION" _
            & " , ISNULL(RTRIM(TMP0001.CHANGEARRSTATIONNAME), '')               AS CHANGEARRSTATIONNAME" _
            & " , ISNULL(RTRIM(TMP0001.DELFLG), '0')                            AS DELFLG" _
            & " FROM ( " _
            & "       SELECT  " _
            & "              TMP0001.* " _
            & "            , ROW_NUMBER() OVER ( " _
            & "                  PARTITION BY TMP0001.OILCODE, TMP0001.ORDERINGTYPE " _
            & "                  ORDER BY TMP0001.OILCODE, TMP0001.ORDERINGTYPE " _
            & "              ) RNUM " _
            & "       FROM OIL.TMP0001ORDER TMP0001 " _
            & " ) TMP0001 " _
            & " LEFT JOIN ( " _
            & "       SELECT " _
            & "              OIT0004.* " _
            & "            , ROW_NUMBER() OVER ( " _
            & "                  PARTITION BY OIT0004.PREOILCODE, OIT0004.PREORDERINGTYPE " _
            & "                  ORDER BY OIT0004.PREOILCODE, OIT0004.PREORDERINGTYPE " _
            & "              ) RNUM " _
            & "       FROM OIL.OIT0004_LINK OIT0004 " _
            & "       WHERE OIT0004.LINKNO = @P01" _
            & "       AND OIT0004.TRAINNO = @P02" _
            & "       AND OIT0004.DELFLG <> @P03" _
            & "       AND OIT0004.STATUS  = '1'" _
            & " ) OIT0004 ON " _
            & "       (OIT0004.TANKNUMBER = TMP0001.TANKNO " _
            & "        OR OIT0004.PREOILCODE + OIT0004.PREORDERINGTYPE = TMP0001.OILCODE + TMP0001.ORDERINGTYPE) " _
            & "       AND OIT0004.RNUM = TMP0001.RNUM " _
            & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "       (OIT0004.TANKNUMBER = OIM0005.TANKNUMBER " _
            & "        OR TMP0001.TANKNO  = OIM0005.TANKNUMBER) " _
            & "       AND OIM0005.DELFLG <> @P03" _
            & " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
            & "       (OIT0004.TANKNUMBER = OIT0005.TANKNUMBER " _
            & "        OR TMP0001.TANKNO  = OIT0005.TANKNUMBER) " _
            & "       AND OIT0005.DELFLG <> @P03"

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
            & " , ISNULL(RTRIM(TMP0001.ORDERINGTYPE), '')                       AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINGOILNAME), '')                    AS ORDERINGOILNAME" _
            & " , CASE" _
            & "   WHEN (OIT0004.TANKNUMBER IS NULL OR TMP0001.TANKNO IS NULL) " _
            & "    AND TMP0001.OILNAME IS NULL THEN @P04" _
            & "   WHEN (OIT0004.TANKNUMBER <> '' OR TMP0001.TANKNO <> '') " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN (OIT0004.TANKNUMBER <> '' OR TMP0001.TANKNO <> '') " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN (OIT0004.TANKNUMBER IS NOT NULL OR TMP0001.TANKNO IS NOT NULL) " _
            & "    AND TMP0001.OILCODE IS NULL THEN @P04" _
            & "   WHEN (OIT0004.TANKNUMBER IS NOT NULL OR TMP0001.TANKNO IS NOT NULL) THEN @P06" _
            & "   ELSE @P07" _
            & "   END                                                           AS TANKQUOTA" _
            & " , ISNULL(RTRIM(OIT0004.LINKNO), '')                             AS LINKNO" _
            & " , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')                       AS LINKDETAILNO" _
            & " , ISNULL(RTRIM(OIT0004.LINEORDER), TMP0001.LINEORDER)           AS LINEORDER" _
            & " , ISNULL(RTRIM(OIM0005.TANKNUMBER), '')                         AS TANKNO" _
            & " , ISNULL(RTRIM(OIM0005.MODEL), '')                              AS MODEL" _
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
            & " , ISNULL(RTRIM(OIT0004.PREOILCODE), OIT0005.LASTOILCODE)                AS LASTOILCODE" _
            & " , ISNULL(RTRIM(OIT0004.PREOILNAME), OIT0005.LASTOILNAME)                AS LASTOILNAME" _
            & " , ISNULL(RTRIM(OIT0004.PREORDERINGTYPE), OIT0005.PREORDERINGTYPE)       AS PREORDERINGTYPE" _
            & " , ISNULL(RTRIM(OIT0004.PREORDERINGOILNAME), OIT0005.PREORDERINGOILNAME) AS PREORDERINGOILNAME" _
            & " , ISNULL(RTRIM(TMP0001.CHANGETRAINNO), '')                      AS CHANGETRAINNO" _
            & " , ISNULL(RTRIM(TMP0001.CHANGETRAINNAME), '')                      AS CHANGETRAINNAME" _
            & " , ISNULL(RTRIM(TMP0001.SECONDCONSIGNEECODE), '')                AS SECONDCONSIGNEECODE" _
            & " , ISNULL(RTRIM(TMP0001.SECONDCONSIGNEENAME), '')                AS SECONDCONSIGNEENAME" _
            & " , ISNULL(RTRIM(TMP0001.SECONDARRSTATION), '')                   AS SECONDARRSTATION" _
            & " , ISNULL(RTRIM(TMP0001.SECONDARRSTATIONNAME), '')               AS SECONDARRSTATIONNAME" _
            & " , ISNULL(RTRIM(TMP0001.CANGERETSTATION), '')                    AS CANGERETSTATION" _
            & " , ISNULL(RTRIM(TMP0001.CHANGEARRSTATIONNAME), '')               AS CHANGEARRSTATIONNAME" _
            & " , ISNULL(RTRIM(TMP0001.DELFLG), '0')                            AS DELFLG" _
            & " FROM ( " _
            & "       SELECT " _
            & "              OIT0004.* " _
            & "            , ROW_NUMBER() OVER ( " _
            & "                  PARTITION BY OIT0004.PREOILCODE, OIT0004.PREORDERINGTYPE " _
            & "                  ORDER BY OIT0004.PREOILCODE, OIT0004.PREORDERINGTYPE " _
            & "              ) RNUM " _
            & "       FROM OIL.OIT0004_LINK OIT0004 " _
            & "       WHERE OIT0004.LINKNO = @P01" _
            & "       AND OIT0004.TRAINNO = @P02" _
            & "       AND OIT0004.DELFLG <> @P03" _
            & "       AND OIT0004.STATUS  = '1'" _
            & " ) OIT0004 " _
            & " LEFT JOIN ( " _
            & "       SELECT  " _
            & "              TMP0001.* " _
            & "            , ROW_NUMBER() OVER ( " _
            & "                  PARTITION BY TMP0001.OILCODE, TMP0001.ORDERINGTYPE " _
            & "                  ORDER BY TMP0001.OILCODE, TMP0001.ORDERINGTYPE " _
            & "              ) RNUM " _
            & "       FROM OIL.TMP0001ORDER TMP0001 " _
            & " ) TMP0001 ON " _
            & "       (OIT0004.TANKNUMBER = TMP0001.TANKNO " _
            & "        OR OIT0004.PREOILCODE + OIT0004.PREORDERINGTYPE = TMP0001.OILCODE + TMP0001.ORDERINGTYPE) " _
            & "       AND OIT0004.RNUM = TMP0001.RNUM " _
            & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "       (OIT0004.TANKNUMBER = OIM0005.TANKNUMBER " _
            & "        OR TMP0001.TANKNO  = OIM0005.TANKNUMBER) " _
            & "       AND OIM0005.DELFLG <> @P03 " _
            & " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
            & "       (OIT0004.TANKNUMBER = OIT0005.TANKNUMBER " _
            & "        OR TMP0001.TANKNO  = OIT0005.TANKNUMBER) " _
            & "       AND OIT0005.DELFLG <> @P03" _
            & " WHERE TMP0001.ORDERNO IS NULL "

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
                Dim intDETAILNO As Integer = 0
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    i += 1
                    OIT0003row("LINECNT") = i        'LINECNT

                    '受注明細№の退避
                    If OIT0003row("DETAILNO") <> "" Then
                        intDETAILNO = OIT0003row("DETAILNO")
                    Else
                        intDETAILNO += 1
                    End If

                    '受注No, 受注明細№の設定
                    If OIT0003row("ORDERNO") = "" Then
                        OIT0003row("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text
                        OIT0003row("DETAILNO") = intDETAILNO.ToString("000")
                        OIT0003row("SHIPPERSCODE") = work.WF_SEL_SHIPPERSCODE.Text
                        OIT0003row("SHIPPERSNAME") = work.WF_SEL_SHIPPERSNAME.Text
                        OIT0003row("BASECODE") = work.WF_SEL_BASECODE.Text
                        OIT0003row("BASENAME") = work.WF_SEL_BASENAME.Text
                        OIT0003row("CONSIGNEECODE") = work.WF_SEL_CONSIGNEECODE.Text
                        OIT0003row("CONSIGNEENAME") = work.WF_SEL_CONSIGNEENAME.Text
                    End If

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
    ''' 画面表示データ取得(タブ「入換・積込指示」一覧表示用)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGetTab2(ByVal SQLcon As SqlConnection)
        If IsNothing(OIT0003tbl_tab2) Then
            OIT0003tbl_tab2 = New DataTable
        End If

        If OIT0003tbl_tab2.Columns.Count <> 0 Then
            OIT0003tbl_tab2.Columns.Clear()
        End If

        OIT0003tbl_tab2.Clear()

        '○ 一覧表示用検索SQL
        '　一覧説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
            " SELECT" _
            & "   0                                                  AS LINECNT" _
            & " , ''                                                 AS OPERATION" _
            & " , 0                                                  AS TIMSTP" _
            & " , 1                                                  AS 'SELECT'" _
            & " , 0                                                  AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0003.ORDERNO), '')                 AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')                AS DETAILNO" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINFO), '')               AS ORDERINFO" _
            & " , ''                                                 AS ORDERINFONAME" _
            & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')               AS LINEORDER" _
            & " , ISNULL(RTRIM(OIT0003.LOADINGIRILINETRAINNO), '')   AS LOADINGIRILINETRAINNO" _
            & " , ISNULL(RTRIM(OIT0003.LOADINGIRILINETRAINNAME), '') AS LOADINGIRILINETRAINNAME" _
            & " , ISNULL(RTRIM(OIT0003.LOADINGIRILINEORDER), '')     AS LOADINGIRILINEORDER" _
            & " , ISNULL(RTRIM(OIT0003.LINE), '')                    AS LINE" _
            & " , ISNULL(RTRIM(OIT0003.FILLINGPOINT), '')            AS FILLINGPOINT" _
            & " , ISNULL(RTRIM(OIT0003.OILCODE), '')                 AS OILCODE" _
            & " , ISNULL(RTRIM(OIT0003.OILNAME), '')                 AS OILNAME" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')            AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '')         AS ORDERINGOILNAME" _
            & " , ISNULL(RTRIM(OIM0005.MODEL), '')                   AS MODEL" _
            & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                  AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0003.LOADINGOUTLETTRAINNO), '')    AS LOADINGOUTLETTRAINNO" _
            & " , ISNULL(RTRIM(OIT0003.LOADINGOUTLETTRAINNAME), '')  AS LOADINGOUTLETTRAINNAME" _
            & " , ISNULL(RTRIM(OIT0003.LOADINGOUTLETORDER), '')      AS LOADINGOUTLETORDER" _
            & " , ISNULL(RTRIM(OIT0003.DELFLG), '')                  AS DELFLG" _
            & " FROM OIL.OIT0003_DETAIL OIT0003 " _
            & " INNER JOIN OIL.OIT0002_ORDER OIT0002 ON " _
            & "       OIT0002.ORDERNO = OIT0003.ORDERNO " _
            & " INNER JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "       OIT0003.TANKNO = OIM0005.TANKNUMBER" _
            & "       AND OIM0005.DELFLG <> @P02" _
            & " WHERE OIT0003.ORDERNO = @P01" _
            & " AND OIT0003.DELFLG <> @P02"

        SQLStr &=
              " ORDER BY" _
            & "    RIGHT('00' + OIT0003.LINEORDER, 2) DESC"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003tbl_tab2.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl_tab2.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003tab2row As DataRow In OIT0003tbl_tab2.Rows
                    i += 1
                    OIT0003tab2row("LINECNT") = i        'LINECNT
                    CODENAME_get("ORDERINFO", OIT0003tab2row("ORDERINFO"), OIT0003tab2row("ORDERINFONAME"), WW_DUMMY)
                    'CODENAME_get("PRODUCTPATTERN", OIT0003tab2row("OILCODE"), OIT0003tab2row("OILNAME"), WW_DUMMY)
                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB2 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB2 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 画面表示データ取得(タブ「タンク車明細」一覧表示用)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGetTab3(ByVal SQLcon As SqlConnection)
        If IsNothing(OIT0003tbl_tab3) Then
            OIT0003tbl_tab3 = New DataTable
        End If

        If OIT0003tbl_tab3.Columns.Count <> 0 Then
            OIT0003tbl_tab3.Columns.Clear()
        End If

        OIT0003tbl_tab3.Clear()

        '○ 一覧表示用検索SQL
        '　一覧説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
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
                & " , ISNULL(RTRIM(OIT0003.ORDERINFO), '')               AS ORDERINFO" _
                & " , CASE ISNULL(RTRIM(OIT0003.ORDERINFO), '')" _
                & "   WHEN '10' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '11' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '12' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
                & "   ELSE ISNULL(RTRIM(OIS0015_2.VALUE1), '')" _
                & "   END                                                AS ORDERINFONAME" _
                & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')               AS LINEORDER" _
                & " , ISNULL(RTRIM(OIT0003.OILCODE), '')                 AS OILCODE" _
                & " , ISNULL(RTRIM(OIT0003.OILNAME), '')                 AS OILNAME" _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')            AS ORDERINGTYPE" _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '')         AS ORDERINGOILNAME" _
                & " , ISNULL(RTRIM(OIM0005.MODEL), '')                   AS MODEL" _
                & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                  AS TANKNO" _
                & " , ISNULL(RTRIM(OIT0002.TANKRINKNO), '')              AS LINKNO" _
                & " , ''                                                 AS LINKDETAILNO" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>'" _
                & "   END                                                           AS JRINSPECTIONALERT" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P03" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN @P04" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN @P05" _
                & "   END                                                           AS JRINSPECTIONALERTSTR" _
                & " , ISNULL(FORMAT(OIM0005.JRINSPECTIONDATE, 'yyyy/MM/dd'), NULL)  AS JRINSPECTIONDATE" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>'" _
                & "   END                                                           AS JRALLINSPECTIONALERT" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P03" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN @P04" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN @P05" _
                & "   END                                                           AS JRALLINSPECTIONALERTSTR" _
                & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), NULL) AS JRALLINSPECTIONDATE" _
                & " , ISNULL(RTRIM(OIT0003.CARSAMOUNT), '')                         AS CARSAMOUNT" _
                & " , ISNULL(RTRIM(OIT0003.JOINT), '')                              AS JOINT" _
                & " , ISNULL(FORMAT(OIT0003.ACTUALLODDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALLODDATE" _
                & " , ISNULL(FORMAT(OIT0003.ACTUALDEPDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALDEPDATE" _
                & " , ISNULL(FORMAT(OIT0003.ACTUALARRDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALARRDATE" _
                & " , ISNULL(FORMAT(OIT0003.ACTUALACCDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALACCDATE" _
                & " , ISNULL(FORMAT(OIT0003.ACTUALEMPARRDATE, 'yyyy/MM/dd'), NULL)  AS ACTUALEMPARRDATE" _
                & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNO), '')                      AS CHANGETRAINNO" _
                & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNAME), '')                    AS CHANGETRAINNAME" _
                & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEECODE), '')                AS SECONDCONSIGNEECODE" _
                & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEENAME), '')                AS SECONDCONSIGNEENAME" _
                & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATION), '')                   AS SECONDARRSTATION" _
                & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATIONNAME), '')               AS SECONDARRSTATIONNAME" _
                & " , ISNULL(RTRIM(OIT0003.CANGERETSTATION), '')                    AS CANGERETSTATION" _
                & " , ISNULL(RTRIM(OIT0003.CHANGEARRSTATIONNAME), '')               AS CHANGEARRSTATIONNAME" _
                & " , ISNULL(RTRIM(OIT0002.DELFLG), '')                             AS DELFLG" _
                & " FROM OIL.OIT0002_ORDER OIT0002 " _
                & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
                & "       OIT0002.ORDERNO = OIT0003.ORDERNO" _
                & "       AND OIT0003.DELFLG <> @P02" _
                & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
                & "       OIT0003.TANKNO = OIM0005.TANKNUMBER" _
                & "       AND OIM0005.DELFLG <> @P02" _
                & " LEFT JOIN com.OIS0015_FIXVALUE OIS0015_2 ON " _
                & "        OIS0015_2.CLASS   = 'ORDERINFO' " _
                & "    AND OIS0015_2.KEYCODE = OIT0003.ORDERINFO " _
                & " WHERE OIT0002.ORDERNO = @P01" _
                & " AND OIT0002.DELFLG <> @P02"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20) '赤丸
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20) '黄丸
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '緑丸

                '                PARA00.Value = O_INSCNT
                PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE
                PARA03.Value = C_INSPECTIONALERT.ALERT_RED
                PARA04.Value = C_INSPECTIONALERT.ALERT_YELLOW
                PARA05.Value = C_INSPECTIONALERT.ALERT_GREEN

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003tbl_tab3.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl_tab3.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    i += 1
                    OIT0003tab3row("LINECNT") = i        'LINECNT
                    CODENAME_get("ORDERINFO", OIT0003tab3row("ORDERINFO"), OIT0003tab3row("ORDERINFONAME"), WW_DUMMY)
                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB3 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB3 Select"
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
            '○ 画面表示データ復元
            'Master.RecoverTable(OIT0003tbl_tab1, work.WF_SEL_INPTAB1TBL.Text)
            DisplayGrid_TAB1()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            '○ 画面表示データ復元
            Master.RecoverTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

            DisplayGrid_TAB2()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            Master.RecoverTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

            DisplayGrid_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            Master.RecoverTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

            DisplayGrid_TAB4()

        End If

        '〇 画面表示設定処理
        WW_ScreenEnabledSet()

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

        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()
        'Dim divObj = DirectCast(pnlListArea1.FindControl(pnlListArea1.ID & "_DR"), Panel)
        'Dim tblObj = DirectCast(divObj.Controls(0), Table)
        'For Each rowitem As TableRow In tblObj.Rows
        '    For Each cellObj As TableCell In rowitem.Controls
        '        If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPPERSNAME") _
        '            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ORDERINGOILNAME") Then
        '            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
        '        End If
        '    Next
        'Next

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
    ''' 一覧再表示処理(タブ「入換・積込指示」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid_TAB2()
        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0003tab2row As DataRow In OIT0003tbl_tab2.Rows
            If OIT0003tab2row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0003tab2row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0003tbl_tab2)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB2"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea2
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

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
    Protected Sub DisplayGrid_TAB3()
        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
            If OIT0003tab3row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0003tab3row("SELECT") = WW_DataCNT

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
        Dim TBLview As DataView = New DataView(OIT0003tbl_tab3)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB3"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea3
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

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
    ''' 一覧再表示処理(タブ「費用入力」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid_TAB4()
        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0003tab4row As DataRow In OIT0003tbl_tab4.Rows
            If OIT0003tab4row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0003tab4row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0003tbl_tab4)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID + "TAB4"
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea4
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

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
    ''' 託送指示ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDELIVERY_Click()

        Dim strOrderStatus As String = ""

        '託送指示フラグを"1"(手配)にする。
        work.WF_SEL_DELIVERYFLG.Text = "1"

        '受注TBL更新
        WW_UpdateDeliveryFlg("1")

        '〇 受注進行ステータスの状態
        WW_ScreenOrderStatusSet(strOrderStatus)

        '受注進行ステータスに変更があった場合
        If strOrderStatus <> "" Then
            '〇(受注TBL)受注進行ステータス更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrderStatus(strOrderStatus)
                CODENAME_get("ORDERSTATUS", strOrderStatus, TxtOrderStatus.Text, WW_DUMMY)
                work.WF_SEL_ORDERSTATUS.Text = strOrderStatus
                work.WF_SEL_ORDERSTATUSNM.Text = TxtOrderStatus.Text
            End Using
        End If

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

        For Each OIT0003row As DataRow In OIT0003WKtbl.Rows
            If OIT0003row("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text Then
                OIT0003row("ORDERSTATUS") = strOrderStatus
                OIT0003row("ORDERSTATUSNAME") = TxtOrderStatus.Text
                OIT0003row("DELIVERYFLG") = "1"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

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

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, "5")
        End Using

        Dim i As Integer = 0
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            i += 1
            OIT0003row("LINEORDER") = i        '貨物駅入線順

        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

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

        '〇 受注営業所チェック
        '受注営業所が選択されていない場合は、他の検索(LEFTBOX)は表示させない制御をする
        '※受注営業所は他の検索するためのKEYとして使用するため
        If WF_FIELD.Value <> "TxtOrderOffice" AndAlso TxtOrderOffice.Text = "" Then
            Master.Output(C_MESSAGE_NO.OIL_ORDEROFFICE_UNSELECT, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            TxtArrstationCode.Focus()
            WW_CheckERR("受注営業所が未選択。", C_MESSAGE_NO.OIL_ORDEROFFICE_UNSELECT)
            WF_LeftboxOpen.Value = ""   'LeftBoxを表示させない
            TxtOrderOffice.Focus()
            Exit Sub
        End If

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
                    '受注営業所
                    If WF_FIELD.Value = "TxtOrderOffice" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            'prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtOrderOffice.Text)

                            '〇 画面(受注営業所).テキストボックスが未設定
                            'If work.WF_SEL_ORDERSALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtOrderOffice.Text)
                            'Else
                            '    prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, TxtOrderOffice.Text)
                            'End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtOrderOffice.Text)
                        End If
                    End If
                    '########################################

                    '受注パターン
                    If WF_FIELD.Value = "TxtOrderType" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtOrderType.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, TxtOrderType.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtOrderType.Text)
                        End If
                    End If

                    '荷主名
                    If WF_FIELD.Value = "TxtShippersCode" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtShippersCode.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, TxtShippersCode.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtShippersCode.Text)
                        End If
                    End If

                    '荷受人名
                    If WF_FIELD.Value = "TxtConsigneeCode" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtConsigneeCode.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, TxtConsigneeCode.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtConsigneeCode.Text)
                        End If
                    End If

                    '本線列車
                    If WF_FIELD.Value = "TxtTrainNo" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtTrainNo.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, TxtTrainNo.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtTrainNo.Text)
                        End If
                    End If

                    '発駅
                    If WF_FIELD.Value = "TxtDepstationCode" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "1", TxtDepstationCode.Text)
                            Else
                                prmData = work.CreateSTATIONPTParam(work.WF_SEL_ORDERSALESOFFICECODE.Text + "1", TxtDepstationCode.Text)
                            End If
                        Else
                            prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "1", TxtDepstationCode.Text)
                        End If
                    End If

                    '着駅
                    If WF_FIELD.Value = "TxtArrstationCode" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "2", TxtArrstationCode.Text)
                            Else
                                prmData = work.CreateSTATIONPTParam(work.WF_SEL_ORDERSALESOFFICECODE.Text + "2", TxtArrstationCode.Text)
                            End If
                        Else
                            prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "2", TxtArrstationCode.Text)
                        End If
                    End If

                    '(一覧)荷主名, (一覧)油種, (一覧)タンク車№, 
                    '(一覧)入線列車番号, (一覧)出線列車番号, (一覧)回線
                    If WF_FIELD.Value = "SHIPPERSNAME" _
                        OrElse WF_FIELD.Value = "OILNAME" _
                        OrElse WF_FIELD.Value = "ORDERINGOILNAME" _
                        OrElse WF_FIELD.Value = "TANKNO" _
                        OrElse WF_FIELD.Value = "LOADINGIRILINETRAINNO" _
                        OrElse WF_FIELD.Value = "LOADINGOUTLETTRAINNO" _
                        OrElse WF_FIELD.Value = "LINE" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, "")
                            Else
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, "")
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
                        End If

                        '### LeftBoxマルチ対応(20200217) START #####################################################
                        If WF_FIELD.Value = "TANKNO" Then
                            '↓暫定一覧対応 2020/02/13 グループ会社版を復活させ石油システムに合わない部分は直す
                            Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                            .SetTableList(enumVal, WW_DUMMY, prmData)
                            .ActiveTable()
                            Return
                            '↑暫定一覧対応 2020/02/13
                        End If
                        '### LeftBoxマルチ対応(20200217) END   #####################################################
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

                If TxtTrainNo.Text = "" Then
                    '発駅
                    TxtDepstationCode.Text = ""
                    LblDepstationName.Text = ""
                    '着駅
                    TxtArrstationCode.Text = ""
                    LblArrstationName.Text = ""
                    '荷主
                    TxtShippersCode.Text = ""
                    LblShippersName.Text = ""
                    '荷受人
                    TxtConsigneeCode.Text = ""
                    LblConsigneeName.Text = ""
                    '受注パターン
                    TxtOrderType.Text = ""

                    '〇 (予定)の日付を設定
                    TxtLoadingDate.Text = ""
                    TxtDepDate.Text = ""
                    TxtArrDate.Text = ""
                    TxtAccDate.Text = ""
                    TxtEmparrDate.Text = ""

                    work.WF_SEL_SHIPPERSCODE.Text = ""
                    work.WF_SEL_SHIPPERSNAME.Text = ""
                    work.WF_SEL_BASECODE.Text = ""
                    work.WF_SEL_BASENAME.Text = ""
                    work.WF_SEL_CONSIGNEECODE.Text = ""
                    work.WF_SEL_CONSIGNEENAME.Text = ""
                    work.WF_SEL_PATTERNCODE.Text = ""
                    work.WF_SEL_PATTERNNAME.Text = ""

                    Exit Select
                End If

                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}

                '〇 検索(営業所).テキストボックスが未設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    '〇 画面(受注営業所).テキストボックスが未設定
                    If TxtOrderOffice.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "TRAINNUMBER", TxtTrainNo.Text, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TRAINNUMBER", TxtTrainNo.Text, WW_GetValue)
                    End If
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

                '〇 (予定)の日付を設定
                TxtLoadingDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtDepDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtArrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtAccDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtEmparrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")

                ''〇 (予定)の日付を設定
                'TxtLoadingDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                'TxtDepDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(6))).ToString("yyyy/MM/dd")
                'TxtArrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                'TxtAccDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                'TxtEmparrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")

                ''〇 積置フラグ(積置列車:T, 非積置列車：N)
                'If WW_GetValue(12) = "T" Then
                '    chkOrderInfo.Checked = True
                '    work.WF_SEL_STACKINGFLG.Text = "1"
                'Else
                '    chkOrderInfo.Checked = False
                '    work.WF_SEL_STACKINGFLG.Text = "2"
                'End If

                '〇営業所配下情報を取得・設定
                WW_GetValue = {"", "", "", "", "", "", "", ""}

                '〇 検索(営業所).テキストボックスが未設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    '〇 画面(受注営業所).テキストボックスが未設定
                    If TxtOrderOffice.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                    End If
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

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonALLSELECT_TAB2()

            'タブ「タンク車明細」
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
    ''' 全選択ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB2()

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「タンク車明細」)
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

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonSELECT_LIFTED_TAB2()

            'タブ「タンク車明細」
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
    ''' 全解除ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB2()

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「タンク車明細」)
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

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonLINE_LIFTED_TAB2()

            'タブ「タンク車明細」
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

        Dim SelectChk As Boolean = False

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
            For Each OIT0003UPDrow In OIT0003tbl.Rows
                If OIT0003UPDrow("OPERATION") = "on" Then

                    If OIT0003UPDrow("LINECNT") < 9000 Then
                        SelectChk = True
                    End If

                    j += 1
                    OIT0003UPDrow("LINECNT") = j        'LINECNT
                    OIT0003UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0003UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0003UPDrow("ORDERNO")
                    PARA02.Value = OIT0003UPDrow("DETAILNO")
                    PARA03.Value = OIT0003UPDrow("LINKNO")
                    PARA04.Value = OIT0003UPDrow("LINKDETAILNO")

                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()
                Else
                    i += 1
                    OIT0003UPDrow("LINECNT") = i        'LINECNT
                End If
            Next

            '行削除が1件でも実施された場合
            If SelectChk = True Then
                '貨物駅入線順に入力している値をクリアする。
                For Each OIT0003UPDrow In OIT0003tbl.Rows
                    OIT0003UPDrow("LINEORDER") = ""
                Next
            End If

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
        If SelectChk = False Then
            Master.Output(C_MESSAGE_NO.OIL_DELLINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        Else
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB2()

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理(タブ「タンク車明細」)
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

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonLINE_ADD_TAB2()

            'タブ「タンク車明細」
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
            & " , ''                                             AS ORDERINGTYPE" _
            & " , ''                                             AS ORDERINGOILNAME" _
            & " , ''                                             AS TANKQUOTA" _
            & " , ''                                             AS LINKNO" _
            & " , ''                                             AS LINKDETAILNO" _
            & " , ''                                             AS LINEORDER" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS MODEL" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS LASTOILCODE" _
            & " , ''                                             AS LASTOILNAME" _
            & " , ''                                             AS PREORDERINGTYPE" _
            & " , ''                                             AS PREORDERINGOILNAME" _
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

                    ElseIf OIT0003row("DETAILNO") >= intDetailNo.ToString("000") Then
                        intDetailNo += 1

                    ElseIf OIT0003row("HIDDEN") = 1 Then
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
    ''' 行追加ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB2()

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「タンク車明細」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB3()

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB4()

    End Sub

    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '〇 選択されたタブ一覧の各更新ボタン押下時の制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            '割当確定ボタン押下時
            WW_ButtonUPDATE_TAB1()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            '明細更新ボタン押下時
            WW_ButtonUPDATE_TAB2()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            '入力内容登録ボタン押下時
            WW_ButtonUPDATE_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            'XXXボタン押下時
            WW_ButtonUPDATE_TAB4()

        End If

    End Sub

    ''' <summary>
    ''' 割当確定ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonUPDATE_TAB1()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_RESULT As String = ""

        '○ 関連チェック
        WW_Check(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '〇 日付妥当性チェック((予定)日付)
        WW_CheckPlanValidityDate(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '〇 前回油種と油種の整合性チェック
        WW_CheckLastOilConsistency(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Master.Output(C_MESSAGE_NO.OIL_LASTOIL_CONSISTENCY_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            Exit Sub
        End If

        '〇 高速列車対応タンク車チェック
        WW_CheckSpeedTrainTank(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '〇列車重複チェック(同一レコードがすでに登録済みかチェック)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_CheckTrainRepeat(WW_ERRCODE, SQLcon)
            If WW_ERRCODE = "ERR" Then
                Exit Sub
            End If
        End Using

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            '受注DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrder(SQLcon)
            End Using

            '受注明細DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrderDetail(SQLcon)
            End Using

            '(受注TBL)タンク車数更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_UpdateOrderTankCnt(SQLcon)
            End Using

            '受注(一覧)画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_OrderListTBLSet(SQLcon)
            End Using

        End If

        '### START ######################################################
        '★ GridView初期設定
        '○ 画面表示データ再取得(受注(明細)画面表示データ取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            work.WF_SEL_CREATEFLG.Text = 2
            MAPDataGet(SQLcon, 0)
        End Using

        '貨車連結を使用する場合
        If work.WF_SEL_CREATELINKFLG.Text = "2" Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                'DataBase接続
                SQLcon.Open()

                MAPDataGetLinkTab1(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPTAB1TBL.Text)

        '○ 画面表示データ再取得(タブ「入換・積込」表示データ取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab2(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

        '### END   ######################################################

        '〇 荷受人油種チェック
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim strSubMsg As String = ""
            WW_CheckConsigneeOil(WW_ERRCODE, SQLcon, strSubMsg)
            If WW_ERRCODE = "ERR" Then
                Master.Output(C_MESSAGE_NO.OIL_CONSIGNEE_OILCODE_NG, C_MESSAGE_TYPE.ERR, strSubMsg, needsPopUp:=True)
                Exit Sub
            End If
        End Using

        '〇 列車マスタ牽引車数チェック
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_CheckTrainCars(WW_ERRCODE, SQLcon)
            If WW_ERRCODE = "ERR" Then
                Exit Sub
            End If
        End Using

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '〇 受注ステータスが"受注手配"の場合
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then
            '〇(受注TBL)受注進行ステータス更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrderStatus(BaseDllConst.CONST_ORDERSTATUS_200)
                CODENAME_get("ORDERSTATUS", BaseDllConst.CONST_ORDERSTATUS_200, TxtOrderStatus.Text, WW_DUMMY)
                work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200
                work.WF_SEL_ORDERSTATUSNM.Text = TxtOrderStatus.Text

                '○ 画面表示データ復元
                Master.RecoverTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)
                For Each OIT0003WKrow As DataRow In OIT0003WKtbl.Rows
                    If OIT0003WKrow("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text Then
                        OIT0003WKrow("ORDERSTATUS") = work.WF_SEL_ORDERSTATUS.Text
                        OIT0003WKrow("ORDERSTATUSNAME") = work.WF_SEL_ORDERSTATUSNM.Text
                    End If
                Next
                '○ 画面表示データ保存
                Master.SaveTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

            End Using

            '〇 受注ステータスが"手配"へ変更された場合
            If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 Then
                WF_DTAB_CHANGE_NO.Value = "1"
                WF_Detail_TABChange()
            End If

        End If

    End Sub

    ''' <summary>
    ''' 入力内容登録ボタン押下時処理(タブ「入換・積込指示」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonUPDATE_TAB2()
        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_RESULT As String = ""
        Dim WW_Message As String = ""
        'WW_DeliveryInput = "0"                        '託送指示入力(0:未 1:完了)

        '五井営業所、甲子営業所、袖ヶ浦営業所、三重塩浜営業所の場合
        '積込列車番号の入力を可能とする。
        If work.WF_SEL_ORDERSALESOFFICECODE.Text = "011201" _
            OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = "011202" _
            OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = "011203" Then

            '臨海鉄道対象のため有効にする。
            WW_RINKAIFLG = True

        End If

        '● 関連チェック
        WW_CheckTab2(WW_ERRCODE, WW_Message)
        If WW_ERRCODE = "ERR" Then
            '入換指示入力(0:未 1:完了)
            WW_SwapInput = "0"
            WW_RESULT = WW_ERRCODE
            'Exit Sub
        End If

        '〇 受注明細DB更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UpdateOrderDetail_TAB2(SQLcon)
        End Using

        '● 積場スペックチェック
        WW_CheckLoadingSpecs(WW_ERRCODE, WW_Message)
        If WW_ERRCODE = "ERR" _
            OrElse WW_ERRCODE = "ERR1" _
            OrElse WW_ERRCODE = "ERR2" _
            OrElse WW_ERRCODE = "ERR3" Then

            '積込指示入力(0:未 1:完了)
            WW_LoadingInput = "0"

        End If

        '受注進行ステータス退避用
        Dim strOrderStatus As String = ""

        '### START ###############################################################################
        '臨海鉄道未対象の場合
        If WW_RINKAIFLG = False Then
            '〇 受注進行ステータスの状態
            Select Case work.WF_SEL_ORDERSTATUS.Text
                '受注進行ステータス＝"200:手配中"
                Case BaseDllConst.CONST_ORDERSTATUS_200
                    '積込指示入力＝"1:完了"の場合
                    If WW_LoadingInput = "1" Then
                        '手配完了
                        strOrderStatus = CONST_ORDERSTATUS_270
                        CODENAME_get("ORDERSTATUS", strOrderStatus, TxtOrderStatus.Text, WW_DUMMY)
                    End If
            End Select

            '臨海鉄道対象の場合
        Else
            '〇 受注進行ステータスの状態
            WW_ScreenOrderStatusSet(strOrderStatus)

        End If

        '受注進行ステータスに変更があった場合
        If strOrderStatus <> "" Then
            '〇(受注TBL)受注進行ステータス更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrderStatus(strOrderStatus)
                CODENAME_get("ORDERSTATUS", strOrderStatus, TxtOrderStatus.Text, WW_DUMMY)
                work.WF_SEL_ORDERSTATUS.Text = strOrderStatus
                work.WF_SEL_ORDERSTATUSNM.Text = TxtOrderStatus.Text

            End Using
        End If

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

        For Each OIT0003row As DataRow In OIT0003WKtbl.Rows
            If OIT0003row("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text Then
                OIT0003row("ORDERSTATUS") = strOrderStatus
                OIT0003row("ORDERSTATUSNAME") = TxtOrderStatus.Text
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

        '### END ###############################################################################

        If WW_RESULT = "ERR" Then
            WW_ERRCODE = WW_RESULT
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, WW_Message, needsPopUp:=True)
            Exit Sub
        ElseIf WW_ERRCODE = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, WW_Message, needsPopUp:=True)
            Exit Sub
        ElseIf WW_ERRCODE = "ERR1" Then
            Master.Output(C_MESSAGE_NO.OIL_FILLINGPOINT_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        ElseIf WW_ERRCODE = "ERR2" Then
            Master.Output(C_MESSAGE_NO.OIL_LOADINGSPECS_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        ElseIf WW_ERRCODE = "ERR3" Then
            Master.Output(C_MESSAGE_NO.OIL_LOADING_OIL_RECORD_OVER, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '〇 受注ステータスが"手配完了"へ変更された場合
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 Then
            WF_DTAB_CHANGE_NO.Value = "2"
            WF_Detail_TABChange()
        End If

    End Sub

    ''' <summary>
    ''' 明細更新ボタン押下時処理(タブ「タンク車明細」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonUPDATE_TAB3()

        '● 関連チェック
        WW_CheckTab3(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '● 日付妥当性チェック(実績(日付))
        WW_CheckActualValidityDate(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '貨車連結表DB追加・更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UpdateLink(SQLcon)
        End Using

        '〇 受注DB更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UpdateOrder_TAB3(SQLcon)
        End Using

        '〇 受注明細DB更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UpdateOrderDetail_TAB3(SQLcon)
        End Using

        '◎ 油種別タンク車数(車)データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_OILTANKCntGet(SQLcon)
        End Using

        '受注進行ステータス退避用
        Dim strOrderStatus As String = ""

        '### 受注進行ステータスチェック START ##############################################
        '受注進行ステータスの状態
        Select Case work.WF_SEL_ORDERSTATUS.Text
            '"270:手配完了"
            Case BaseDllConst.CONST_ORDERSTATUS_270
                '(一覧)数量の入力チェック
                '0(デフォルト値)以外が入力されていれば、入力していると判断
                Dim chkCarsAmount As Boolean = True
                Dim decCarsAmount As Decimal = 0
                For Each OIT0003Chktab3row As DataRow In OIT0003tbl_tab3.Rows
                    Try
                        decCarsAmount = Decimal.Parse(OIT0003Chktab3row("CARSAMOUNT"))
                    Catch ex As Exception
                        decCarsAmount = 0
                        OIT0003Chktab3row("CARSAMOUNT") = "0"
                    End Try
                    '(一覧)数値に0が1件でも存在したら、"False"(未入力)とする。
                    If decCarsAmount = 0 Then
                        chkCarsAmount = False
                    End If
                Next

                '(実績)積込日の入力が完了、かつ(一覧)数量の入力がすべて完了
                If TxtActualLoadingDate.Text <> "" AndAlso chkCarsAmount = True Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_300
                End If

                '(実績)積込日の入力が完了、かつ(一覧)数量の入力がすべて完了
                'かつ、(実績)積車着日の入力が完了
                If TxtActualLoadingDate.Text <> "" AndAlso chkCarsAmount = True _
                    AndAlso TxtActualArrDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_400
                End If

                '(実績)積込日の入力が完了、かつ(一覧)数量の入力がすべて完了
                'かつ、(実績)積車着日の入力が完了
                'かつ、(実績)空車着日の入力が完了
                If TxtActualLoadingDate.Text <> "" _
                    AndAlso TxtActualArrDate.Text <> "" _
                    AndAlso TxtActualEmparrDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_500
                End If

            '"300:受注確定"
            Case BaseDllConst.CONST_ORDERSTATUS_300
                '(実績)積車着日の入力が完了
                If TxtActualArrDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_400
                End If

                '(実績)積車着日の入力が完了
                'かつ、(実績)空車着日の入力が完了
                If TxtActualArrDate.Text <> "" _
                    AndAlso TxtActualEmparrDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_500
                End If

            '"400:受入確認中"
            Case BaseDllConst.CONST_ORDERSTATUS_400

                '(実績)空車着日の入力が完了
                If TxtActualEmparrDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_500
                End If

            '"500:検収中"
            Case BaseDllConst.CONST_ORDERSTATUS_500

        End Select

        '受注進行ステータスに変更があった場合
        If strOrderStatus <> "" Then
            '〇(受注TBL)受注進行ステータス更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrderStatus(strOrderStatus)
                CODENAME_get("ORDERSTATUS", strOrderStatus, TxtOrderStatus.Text, WW_DUMMY)
                work.WF_SEL_ORDERSTATUS.Text = strOrderStatus
                work.WF_SEL_ORDERSTATUSNM.Text = TxtOrderStatus.Text

            End Using

            '○ 画面表示データ復元
            Master.RecoverTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

            For Each OIT0003row As DataRow In OIT0003WKtbl.Rows
                If OIT0003row("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text Then
                    OIT0003row("ORDERSTATUS") = strOrderStatus
                    OIT0003row("ORDERSTATUSNAME") = TxtOrderStatus.Text
                End If
            Next

            '○ 画面表示データ保存
            Master.SaveTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)
        End If
        '### 受注進行ステータスチェック END   ##############################################

        '◎ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab3(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

    End Sub

    ''' <summary>
    ''' XXXボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonUPDATE_TAB4()

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

        Select Case WF_DetailMView.ActiveViewIndex
                'タンク車割当
            Case 0
                WW_ListChange_TAB1()

                '入換・積込指示
            Case 1
                WW_ListChange_TAB2()

                'タンク車明細
            Case 2
                WW_ListChange_TAB3()

                '費用入力
            Case 3

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' リスト変更時処理(タブ「タンク車割当」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListChange_TAB1()
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
                    updHeader.Item("OILCODE") = WW_GetValue(0).Substring(0, 4)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                Else
                    updHeader.Item("OILCODE") = ""
                    updHeader.Item(WF_FIELD.Value) = ""
                End If

                '〇 タンク車割当状況チェック
                WW_TANKQUOTACHK(WF_FIELD.Value, updHeader)
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

            Case "ORDERINGOILNAME"    '(一覧)油種(受発注用)
                '〇油種が設定されている場合
                If WW_ListValue <> "" Then
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN_SEG_N", WW_ListValue, WW_GetValue)
                    updHeader.Item("OILCODE") = WW_GetValue(0).Substring(0, 4)
                    updHeader.Item("OILNAME") = WW_GetValue(2)
                    updHeader.Item("ORDERINGTYPE") = WW_GetValue(3)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                Else
                    updHeader.Item("OILCODE") = ""
                    updHeader.Item("OILNAME") = ""
                    updHeader.Item("ORDERINGTYPE") = ""
                    updHeader.Item(WF_FIELD.Value) = ""
                End If

                '〇 タンク車割当状況チェック
                WW_TANKQUOTACHK(WF_FIELD.Value, updHeader)

            Case "TANKNO"            '(一覧)タンク車№

                '入力が空の場合は、対象項目を空文字で設定する。
                If WW_ListValue = "" Then
                    'タンク車№
                    updHeader.Item("TANKNO") = ""
                    '型式
                    updHeader.Item("MODEL") = ""
                    '前回油種
                    updHeader.Item("LASTOILCODE") = ""
                    updHeader.Item("LASTOILNAME") = ""
                    updHeader.Item("PREORDERINGTYPE") = ""
                    updHeader.Item("PREORDERINGOILNAME") = ""
                    '交検日
                    updHeader.Item("JRINSPECTIONDATE") = ""
                    updHeader.Item("JRINSPECTIONALERT") = ""
                    updHeader.Item("JRINSPECTIONALERTSTR") = ""
                    '全検日
                    updHeader.Item("JRALLINSPECTIONDATE") = ""
                    updHeader.Item("JRALLINSPECTIONALERT") = ""
                    updHeader.Item("JRALLINSPECTIONALERTSTR") = ""

                    '〇 タンク車割当状況チェック
                    WW_TANKQUOTACHK(WF_FIELD.Value, updHeader)

                    Exit Select
                End If

                'WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)
                '〇 検索(営業所).テキストボックスが未設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    '〇 画面(受注営業所).テキストボックスが未設定
                    If TxtOrderOffice.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "TANKNUMBER", WW_ListValue, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)
                    End If
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)
                End If

                'タンク車№
                updHeader.Item("TANKNO") = WW_ListValue
                '型式
                updHeader.Item("MODEL") = WW_GetValue(7)
                '####################################################
                '前回油種
                'Dim WW_LASTOILNAME As String = ""
                'updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                'CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                'updHeader.Item("LASTOILNAME") = WW_LASTOILNAME

                updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                updHeader.Item("LASTOILNAME") = WW_GetValue(4)
                updHeader.Item("PREORDERINGTYPE") = WW_GetValue(5)
                updHeader.Item("PREORDERINGOILNAME") = WW_GetValue(6)
                '####################################################

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

                '〇 タンク車割当状況チェック
                WW_TANKQUOTACHK(WF_FIELD.Value, updHeader)

            Case "LINEORDER"              '(一覧)貨物駅入線順
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "CHANGETRAINNO"          '(一覧)本線列車番号変更
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "SECONDARRSTATIONNAME"   '(一覧)第2着駅
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "SECONDCONSIGNEENAME"    '(一覧)第2荷受人
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "CHANGEARRSTATIONNAME"   '(一覧)空車着駅(変更)
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' リスト変更時処理(タブ「入換・積込指示」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListChange_TAB2()
        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = OIT0003tbl_tab2.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '〇 一覧の件数取得
        Dim intListCnt As Integer = OIT0003tbl_tab2.Rows.Count

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea2.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "LOADINGIRILINEORDER"      '(一覧)積込入線順
                updHeader.Item(WF_FIELD.Value) = WW_ListValue
                updHeader.Item("LOADINGOUTLETORDER") = (intListCnt - Integer.Parse(WW_ListValue) + 1)

            Case "FILLINGPOINT"             '(一覧)充填ポイント
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "LOADINGOUTLETORDER"       '(一覧)積込出線順
                updHeader.Item(WF_FIELD.Value) = WW_ListValue
                updHeader.Item("LOADINGIRILINEORDER") = (intListCnt - Integer.Parse(WW_ListValue) + 1)

            'Case "LOADINGIRILINETRAINNO"    '(一覧)積込入線列車番号
            '    updHeader.Item(WF_FIELD.Value) = WW_ListValue
            '    updHeader.Item("LOADINGIRILINETRAINNAME") = ""

            'Case "LOADINGOUTLETTRAINNO"     '(一覧)積込出線列車番号
            '    updHeader.Item(WF_FIELD.Value) = WW_ListValue
            '    updHeader.Item("LOADINGOUTLETTRAINNAME") = ""

            Case "LINE"                     '(一覧)回線を一覧に設定
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

                '〇営業所配下情報を取得・設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    '〇 画面(受注営業所).テキストボックスが未設定
                    If TxtOrderOffice.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "RINKAITRAIN_LINE", WW_ListValue, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "RINKAITRAIN_LINE", WW_ListValue, WW_GetValue)
                    End If
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "RINKAITRAIN_LINE", WW_ListValue, WW_GetValue)
                End If

                '入線列車番号
                updHeader.Item("LOADINGIRILINETRAINNO") = WW_GetValue(1)
                '入線列車名
                updHeader.Item("LOADINGIRILINETRAINNAME") = WW_GetValue(9)
                '出線列車番号
                updHeader.Item("LOADINGOUTLETTRAINNO") = WW_GetValue(6)
                '出線列車名
                updHeader.Item("LOADINGOUTLETTRAINNAME") = WW_GetValue(7)

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

    End Sub

    ''' <summary>
    ''' リスト変更時処理(タブ「タンク車明細」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListChange_TAB3()
        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = OIT0003tbl_tab3.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea3.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "CARSAMOUNT",           '(一覧)数量
                 "JOINT"                 '(一覧)ジョイント先
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "CHANGETRAINNO",        '(一覧)本線列車番号変更
                 "SECONDARRSTATIONNAME", '(一覧)第2着駅
                 "SECONDCONSIGNEENAME",  '(一覧)第2荷受人
                 "CHANGEARRSTATIONNAME"  '(一覧)空車着駅(変更)
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

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
        '入換・積込指示
        WF_Dtab02.CssClass = ""
        'タンク車明細
        WF_Dtab03.CssClass = ""
        '費用入力
        WF_Dtab04.CssClass = ""

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                'タンク車割当
                WF_Dtab01.CssClass = "selected"

            Case 1
                '入換・積込指示
                WF_Dtab02.CssClass = "selected"

                ''○ 画面表示データ取得
                'Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                '    SQLcon.Open()       'DataBase接続

                '    MAPDataGetTab2(SQLcon)
                'End Using

                ''○ 画面表示データ保存
                'Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

            Case 2
                'タンク車明細
                WF_Dtab03.CssClass = "selected"
            Case 3
                '費用入力
                WF_Dtab04.CssClass = "selected"
        End Select
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

                Case "ORDERSTATUS"      '受注進行ステータス
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERSTATUS"))

                Case "ORDERINFO"        '受注情報
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERINFO, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERINFO"))

                Case "SALESOFFICE"      '営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

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

                Case "PRODUCTPATTERN"   '油種

                    '〇 検索(営業所).テキストボックスが未設定
                    If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                        '〇 画面(受注営業所).テキストボックスが未設定
                        If TxtOrderOffice.Text = "" Then
                            leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USER_ORG, "PRODUCTPATTERN"))
                        Else
                            leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, "PRODUCTPATTERN"))
                        End If
                    Else
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN"))
                    End If

                Case "TANKNO"           'タンク車
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TANKNUMBER, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TANKNO"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            Select Case OIT0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        'WF_Sel_LINECNT.Text = ""            'LINECNT

    End Sub

#Region "タブ「タンク車割当」各テーブル追加・更新"

    ''' <summary>
    ''' 受注TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrder(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0002_ORDER" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0002_ORDER" _
            & "    SET" _
            & "        OFFICECODE        = @P04  , OFFICENAME     = @P05" _
            & "        , TRAINNO         = @P02  , TRAINNAME      = @P93, ORDERTYPE      = @P06" _
            & "        , SHIPPERSCODE    = @P07  , SHIPPERSNAME   = @P08" _
            & "        , BASECODE        = @P09  , BASENAME       = @P10" _
            & "        , CONSIGNEECODE   = @P11  , CONSIGNEENAME  = @P12" _
            & "        , DEPSTATION      = @P13  , DEPSTATIONNAME = @P14" _
            & "        , ARRSTATION      = @P15  , ARRSTATIONNAME = @P16" _
            & "        , ORDERINFO       = @P22  , STACKINGFLG    = @P92" _
            & "        , USEPROPRIETYFLG = @P23  , DELIVERYFLG    = @P94" _
            & "        , LODDATE         = @P24  , DEPDATE        = @P25" _
            & "        , ARRDATE         = @P26  , ACCDATE        = @P27" _
            & "        , EMPARRDATE      = @P28" _
            & "        , UPDYMD          = @P87  , UPDUSER        = @P88" _
            & "        , UPDTERMID       = @P89  , RECEIVEYMD     = @P90" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0002_ORDER" _
            & "        ( ORDERNO      , TRAINNO         , TRAINNAME      , ORDERYMD            , OFFICECODE , OFFICENAME" _
            & "        , ORDERTYPE    , SHIPPERSCODE    , SHIPPERSNAME   , BASECODE            , BASENAME" _
            & "        , CONSIGNEECODE, CONSIGNEENAME   , DEPSTATION     , DEPSTATIONNAME      , ARRSTATION , ARRSTATIONNAME" _
            & "        , RETSTATION   , RETSTATIONNAME  , CANGERETSTATION, CHANGEARRSTATIONNAME, ORDERSTATUS, ORDERINFO " _
            & "        , STACKINGFLG  , USEPROPRIETYFLG , DELIVERYFLG    , LODDATE             , DEPDATE    , ARRDATE" _
            & "        , ACCDATE      , EMPARRDATE      , ACTUALLODDATE  , ACTUALDEPDATE       , ACTUALARRDATE" _
            & "        , ACTUALACCDATE, ACTUALEMPARRDATE, RTANK          , HTANK               , TTANK" _
            & "        , MTTANK       , KTANK           , K3TANK         , K5TANK              , K10TANK" _
            & "        , LTANK        , ATANK           , OTHER1OTANK    , OTHER2OTANK         , OTHER3OTANK" _
            & "        , OTHER4OTANK  , OTHER5OTANK     , OTHER6OTANK    , OTHER7OTANK         , OTHER8OTANK" _
            & "        , OTHER9OTANK  , OTHER10OTANK    , TOTALTANK" _
            & "        , RTANKCH      , HTANKCH         , TTANKCH        , MTTANKCH            , KTANKCH" _
            & "        , K3TANKCH     , K5TANKCH        , K10TANKCH      , LTANKCH             , ATANKCH" _
            & "        , OTHER1OTANKCH, OTHER2OTANKCH   , OTHER3OTANKCH  , OTHER4OTANKCH       , OTHER5OTANKCH" _
            & "        , OTHER6OTANKCH, OTHER7OTANKCH   , OTHER8OTANKCH  , OTHER9OTANKCH       , OTHER10OTANKCH" _
            & "        , TOTALTANKCH" _
            & "        , TANKRINKNO   , KEIJYOYMD       , SALSE          , SALSETAX            , TOTALSALSE" _
            & "        , PAYMENT      , PAYMENTTAX      , TOTALPAYMENT   , DELFLG" _
            & "        , INITYMD      , INITUSER        , INITTERMID" _
            & "        , UPDYMD       , UPDUSER         , UPDTERMID      , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P93, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08, @P09, @P10" _
            & "        , @P11, @P12, @P13, @P14, @P15, @P16" _
            & "        , @P17, @P18, @P19, @P20, @P21, @P22" _
            & "        , @P92, @P23, @P94, @P24, @P25, @P26" _
            & "        , @P27, @P28, @P29, @P30, @P31" _
            & "        , @P32, @P33, @P34, @P35, @P36" _
            & "        , @P37, @P38, @P39, @P40, @P41" _
            & "        , @P42, @P43, @P44, @P45, @P46" _
            & "        , @P47, @P48, @P49, @P50, @P51" _
            & "        , @P52, @P53, @P54" _
            & "        , @P55, @P56, @P57, @P58, @P59" _
            & "        , @P60, @P61, @P62, @P63, @P64" _
            & "        , @P65, @P66, @P67, @P68, @P69" _
            & "        , @P70, @P71, @P72, @P73, @P74" _
            & "        , @P75" _
            & "        , @P76, @P91, @P77, @P78, @P79" _
            & "        , @P80, @P81, @P82, @P83" _
            & "        , @P84, @P85, @P86" _
            & "        , @P87, @P88, @P89, @P90) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    ORDERNO" _
            & "    , TRAINNO" _
            & "    , TRAINNAME" _
            & "    , ORDERYMD" _
            & "    , OFFICECODE" _
            & "    , OFFICENAME" _
            & "    , ORDERTYPE" _
            & "    , SHIPPERSCODE" _
            & "    , SHIPPERSNAME" _
            & "    , BASECODE" _
            & "    , BASENAME" _
            & "    , CONSIGNEECODE" _
            & "    , CONSIGNEENAME" _
            & "    , DEPSTATION" _
            & "    , DEPSTATIONNAME" _
            & "    , ARRSTATION" _
            & "    , ARRSTATIONNAME" _
            & "    , RETSTATION" _
            & "    , RETSTATIONNAME" _
            & "    , CANGERETSTATION" _
            & "    , CHANGEARRSTATIONNAME" _
            & "    , ORDERSTATUS" _
            & "    , ORDERINFO" _
            & "    , STACKINGFLG" _
            & "    , USEPROPRIETYFLG" _
            & "    , DELIVERYFLG" _
            & "    , LODDATE" _
            & "    , DEPDATE" _
            & "    , ARRDATE" _
            & "    , ACCDATE" _
            & "    , EMPARRDATE" _
            & "    , ACTUALLODDATE" _
            & "    , ACTUALDEPDATE" _
            & "    , ACTUALARRDATE" _
            & "    , ACTUALACCDATE" _
            & "    , ACTUALEMPARRDATE" _
            & "    , RTANK" _
            & "    , HTANK" _
            & "    , TTANK" _
            & "    , MTTANK" _
            & "    , KTANK" _
            & "    , K3TANK" _
            & "    , K5TANK" _
            & "    , K10TANK" _
            & "    , LTANK" _
            & "    , ATANK" _
            & "    , OTHER1OTANK" _
            & "    , OTHER2OTANK" _
            & "    , OTHER3OTANK" _
            & "    , OTHER4OTANK" _
            & "    , OTHER5OTANK" _
            & "    , OTHER6OTANK" _
            & "    , OTHER7OTANK" _
            & "    , OTHER8OTANK" _
            & "    , OTHER9OTANK" _
            & "    , OTHER10OTANK" _
            & "    , TOTALTANK" _
            & "    , RTANKCH" _
            & "    , HTANKCH" _
            & "    , TTANKCH" _
            & "    , MTTANKCH" _
            & "    , KTANKCH" _
            & "    , K3TANKCH" _
            & "    , K5TANKCH" _
            & "    , K10TANKCH" _
            & "    , LTANKCH" _
            & "    , ATANKCH" _
            & "    , OTHER1OTANKCH" _
            & "    , OTHER2OTANKCH" _
            & "    , OTHER3OTANKCH" _
            & "    , OTHER4OTANKCH" _
            & "    , OTHER5OTANKCH" _
            & "    , OTHER6OTANKCH" _
            & "    , OTHER7OTANKCH" _
            & "    , OTHER8OTANKCH" _
            & "    , OTHER9OTANKCH" _
            & "    , OTHER10OTANKCH" _
            & "    , TOTALTANKCH" _
            & "    , TANKRINKNO" _
            & "    , KEIJYOYMD" _
            & "    , SALSE" _
            & "    , SALSETAX" _
            & "    , TOTALSALSE" _
            & "    , PAYMENT" _
            & "    , PAYMENTTAX" _
            & "    , TOTALPAYMENT" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & " FROM" _
            & "    OIL.OIT0002_ORDER" _
            & " WHERE" _
            & "        ORDERNO      = @P01"
        '& "    , UPDTIMSTP" _

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARA93 As SqlParameter = SQLcmd.Parameters.Add("@P93", SqlDbType.NVarChar, 20) '本線列車名
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '受注登録日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '受注営業所名
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 7)  '受注パターン
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 10) '荷主コード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 40) '荷主名
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 9)  '基地コード
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 40) '基地名
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 10) '荷受人コード
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 40) '荷受人名
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 7)  '発駅コード
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 40) '発駅名
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 7)  '着駅コード
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 40) '着駅名
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 7)  '空車着駅コード
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 40) '空車着駅名
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 7)  '空車着駅コード(変更後)
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 40) '空車着駅名(変更後)
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 2)  '受注情報
                Dim PARA92 As SqlParameter = SQLcmd.Parameters.Add("@P92", SqlDbType.NVarChar, 1)  '積置可否フラグ
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)  '利用可否フラグ
                Dim PARA94 As SqlParameter = SQLcmd.Parameters.Add("@P94", SqlDbType.NVarChar, 1)  '託送指示フラグ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.Date)         '積込日（予定）
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Date)         '発日（予定）
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Date)         '積車着日（予定）
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.Date)         '受入日（予定）
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Date)         '空車着日（予定）
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.Date)         '積込日（実績）
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.Date)         '発日（実績）
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.Date)         '積車着日（実績）
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.Date)         '受入日（実績）
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.Date)         '空車着日（実績）
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.Int)          '車数（レギュラー）
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.Int)          '車数（ハイオク）
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.Int)          '車数（灯油）
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.Int)          '車数（未添加灯油）
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.Int)          '車数（軽油）
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.Int)          '車数（３号軽油）
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.Int)          '車数（５号軽油）
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.Int)          '車数（１０号軽油）
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", SqlDbType.Int)          '車数（LSA）
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", SqlDbType.Int)          '車数（A重油）
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", SqlDbType.Int)          '車数（その他１）
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", SqlDbType.Int)          '車数（その他２）
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", SqlDbType.Int)          '車数（その他３）
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", SqlDbType.Int)          '車数（その他４）
                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", SqlDbType.Int)          '車数（その他５）
                Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", SqlDbType.Int)          '車数（その他６）
                Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", SqlDbType.Int)          '車数（その他７）
                Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", SqlDbType.Int)          '車数（その他８）
                Dim PARA52 As SqlParameter = SQLcmd.Parameters.Add("@P52", SqlDbType.Int)          '車数（その他９）
                Dim PARA53 As SqlParameter = SQLcmd.Parameters.Add("@P53", SqlDbType.Int)          '車数（その他１０）
                Dim PARA54 As SqlParameter = SQLcmd.Parameters.Add("@P54", SqlDbType.Int)          '合計車数
                Dim PARA55 As SqlParameter = SQLcmd.Parameters.Add("@P55", SqlDbType.Int)          '変更後_車数（レギュラー）
                Dim PARA56 As SqlParameter = SQLcmd.Parameters.Add("@P56", SqlDbType.Int)          '変更後_車数（ハイオク）
                Dim PARA57 As SqlParameter = SQLcmd.Parameters.Add("@P57", SqlDbType.Int)          '変更後_車数（灯油）
                Dim PARA58 As SqlParameter = SQLcmd.Parameters.Add("@P58", SqlDbType.Int)          '変更後_車数（未添加灯油）
                Dim PARA59 As SqlParameter = SQLcmd.Parameters.Add("@P59", SqlDbType.Int)          '変更後_車数（軽油）
                Dim PARA60 As SqlParameter = SQLcmd.Parameters.Add("@P60", SqlDbType.Int)          '変更後_車数（３号軽油）
                Dim PARA61 As SqlParameter = SQLcmd.Parameters.Add("@P61", SqlDbType.Int)          '変更後_車数（５号軽油）
                Dim PARA62 As SqlParameter = SQLcmd.Parameters.Add("@P62", SqlDbType.Int)          '変更後_車数（１０号軽油）
                Dim PARA63 As SqlParameter = SQLcmd.Parameters.Add("@P63", SqlDbType.Int)          '変更後_車数（LSA）
                Dim PARA64 As SqlParameter = SQLcmd.Parameters.Add("@P64", SqlDbType.Int)          '変更後_車数（A重油）
                Dim PARA65 As SqlParameter = SQLcmd.Parameters.Add("@P65", SqlDbType.Int)          '変更後_車数（その他１）
                Dim PARA66 As SqlParameter = SQLcmd.Parameters.Add("@P66", SqlDbType.Int)          '変更後_車数（その他２）
                Dim PARA67 As SqlParameter = SQLcmd.Parameters.Add("@P67", SqlDbType.Int)          '変更後_車数（その他３）
                Dim PARA68 As SqlParameter = SQLcmd.Parameters.Add("@P68", SqlDbType.Int)          '変更後_車数（その他４）
                Dim PARA69 As SqlParameter = SQLcmd.Parameters.Add("@P69", SqlDbType.Int)          '変更後_車数（その他５）
                Dim PARA70 As SqlParameter = SQLcmd.Parameters.Add("@P70", SqlDbType.Int)          '変更後_車数（その他６）
                Dim PARA71 As SqlParameter = SQLcmd.Parameters.Add("@P71", SqlDbType.Int)          '変更後_車数（その他７）
                Dim PARA72 As SqlParameter = SQLcmd.Parameters.Add("@P72", SqlDbType.Int)          '変更後_車数（その他８）
                Dim PARA73 As SqlParameter = SQLcmd.Parameters.Add("@P73", SqlDbType.Int)          '変更後_車数（その他９）
                Dim PARA74 As SqlParameter = SQLcmd.Parameters.Add("@P74", SqlDbType.Int)          '変更後_車数（その他１０）
                Dim PARA75 As SqlParameter = SQLcmd.Parameters.Add("@P75", SqlDbType.Int)          '変更後_合計車数
                Dim PARA76 As SqlParameter = SQLcmd.Parameters.Add("@P76", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim PARA91 As SqlParameter = SQLcmd.Parameters.Add("@P91", SqlDbType.Date)         '計上日
                Dim PARA77 As SqlParameter = SQLcmd.Parameters.Add("@P77", SqlDbType.Int)          '売上金額
                Dim PARA78 As SqlParameter = SQLcmd.Parameters.Add("@P78", SqlDbType.Int)          '売上消費税額
                Dim PARA79 As SqlParameter = SQLcmd.Parameters.Add("@P79", SqlDbType.Int)          '売上合計金額
                Dim PARA80 As SqlParameter = SQLcmd.Parameters.Add("@P80", SqlDbType.Int)          '支払金額
                Dim PARA81 As SqlParameter = SQLcmd.Parameters.Add("@P81", SqlDbType.Int)          '支払消費税額
                Dim PARA82 As SqlParameter = SQLcmd.Parameters.Add("@P82", SqlDbType.Int)          '支払合計金額
                Dim PARA83 As SqlParameter = SQLcmd.Parameters.Add("@P83", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA84 As SqlParameter = SQLcmd.Parameters.Add("@P84", SqlDbType.DateTime)     '登録年月日
                Dim PARA85 As SqlParameter = SQLcmd.Parameters.Add("@P85", SqlDbType.NVarChar, 20) '登録ユーザーID
                Dim PARA86 As SqlParameter = SQLcmd.Parameters.Add("@P86", SqlDbType.NVarChar, 20) '登録端末
                Dim PARA87 As SqlParameter = SQLcmd.Parameters.Add("@P87", SqlDbType.DateTime)     '更新年月日
                Dim PARA88 As SqlParameter = SQLcmd.Parameters.Add("@P88", SqlDbType.NVarChar, 20) '更新ユーザーID
                Dim PARA89 As SqlParameter = SQLcmd.Parameters.Add("@P89", SqlDbType.NVarChar, 20) '更新端末
                Dim PARA90 As SqlParameter = SQLcmd.Parameters.Add("@P90", SqlDbType.DateTime)     '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№

                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    'If Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                    Dim WW_DATENOW As DateTime = Date.Now

                    'DB更新
                    PARA01.Value = work.WF_SEL_ORDERNUMBER.Text       '受注№
                    PARA02.Value = TxtTrainNo.Text                    '本線列車
                    PARA93.Value = TxtTrainName.Text                  '本線列車名
                    PARA03.Value = WW_DATENOW                         '受注登録日
                    PARA04.Value = TxtOrderOfficeCode.Text            '受注営業所コード
                    PARA05.Value = TxtOrderOffice.Text                '受注営業所名
                    PARA06.Value = work.WF_SEL_PATTERNCODE.Text       '受注パターン
                    PARA07.Value = work.WF_SEL_SHIPPERSCODE.Text      '荷主コード
                    PARA08.Value = work.WF_SEL_SHIPPERSNAME.Text      '荷主名
                    PARA09.Value = work.WF_SEL_BASECODE.Text          '基地コード
                    PARA10.Value = work.WF_SEL_BASENAME.Text          '基地名
                    PARA11.Value = work.WF_SEL_CONSIGNEECODE.Text     '荷受人コード
                    PARA12.Value = work.WF_SEL_CONSIGNEENAME.Text     '荷受人名
                    PARA13.Value = TxtDepstationCode.Text             '発駅コード
                    PARA14.Value = LblDepstationName.Text             '発駅名
                    PARA15.Value = TxtArrstationCode.Text             '着駅コード
                    PARA16.Value = LblArrstationName.Text             '着駅名
                    PARA17.Value = ""                                 '空車着駅コード
                    PARA18.Value = ""                                 '空車着駅名
                    PARA19.Value = ""                                 '空車着駅コード(変更後)
                    PARA20.Value = ""                                 '空車着駅名(変更後)
                    'PARA19.Value = OIT0003row("CANGERETSTATION")      '空車着駅コード(変更後)
                    'PARA20.Value = OIT0003row("CHANGEARRSTATIONNAME") '空車着駅名(変更後)
                    PARA21.Value = work.WF_SEL_ORDERSTATUS.Text       '受注進行ステータス

                    ''# 受注情報
                    ''交付アラートが「3日以内のタンク車」または「4日～6日のタンク車」の場合
                    'If OIT0003row("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW _
                    '    OrElse OIT0003row("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Then
                    '    WW_ORDERINFOALERMFLG_82 = True

                    '    '全検アラートが「3日以内のタンク車」または「4日～6日のタンク車」の場合
                    'ElseIf OIT0003row("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW _
                    '    OrElse OIT0003row("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Then
                    '    WW_ORDERINFOALERMFLG_82 = True

                    'End If

                    ''〇 交付アラート、または全検アラートが1件でも警告以上の場合
                    'If WW_ORDERINFOALERMFLG_82 = True Then
                    '    PARA22.Value = BaseDllConst.CONST_ORDERINFO_ALERT_82

                    '    '〇 上記以外
                    'ElseIf WW_ORDERINFOALERMFLG_82 = False Then
                    '    PARA22.Value = ""

                    'End If
                    PARA22.Value = ""

                    '〇 積込日 < 発日 の場合 
                    If WW_ORDERINFOFLG_10 = True Then
                        PARA92.Value = "1"                                '利用可否フラグ(1:積置あり)
                    Else
                        PARA92.Value = "2"                                '利用可否フラグ(2:積置なし)
                    End If

                    PARA23.Value = "1"                                    '利用可否フラグ(1:利用可能)
                    PARA94.Value = work.WF_SEL_DELIVERYFLG.Text           '託送指示フラグ(0:未手配, 1:手配)
                    PARA24.Value = TxtLoadingDate.Text                    '積込日（予定）
                    PARA25.Value = TxtDepDate.Text                        '発日（予定）
                    PARA26.Value = TxtArrDate.Text                        '積車着日（予定）
                    PARA27.Value = TxtAccDate.Text                        '受入日（予定）
                    '空車着日（予定）
                    If TxtEmparrDate.Text = "" Then
                        PARA28.Value = DBNull.Value
                    Else
                        PARA28.Value = TxtEmparrDate.Text
                    End If
                    'PARA24.Value = DateTime.Parse(TxtLoadingDate.Text)                '積込日（予定）
                    'PARA25.Value = DateTime.Parse(TxtDepDate.Text)                    '発日（予定）
                    'PARA26.Value = DateTime.Parse(TxtArrDate.Text)                    '積車着日（予定）
                    'PARA27.Value = DateTime.Parse(TxtAccDate.Text)                    '受入日（予定）
                    'PARA28.Value = DateTime.Parse(TxtEmparrDate.Text)                 '空車着日（予定）
                    '積込日（実績）
                    If TxtActualLoadingDate.Text = "" Then
                        PARA29.Value = DBNull.Value
                    Else
                        PARA29.Value = TxtActualLoadingDate.Text
                    End If
                    '発日（実績）
                    If TxtActualLoadingDate.Text = "" Then
                        PARA30.Value = DBNull.Value
                    Else
                        PARA30.Value = TxtActualDepDate.Text
                    End If
                    '積車着日（実績）
                    If TxtActualArrDate.Text = "" Then
                        PARA31.Value = DBNull.Value
                    Else
                        PARA31.Value = TxtActualArrDate.Text
                    End If
                    '受入日（実績）
                    If TxtActualAccDate.Text = "" Then
                        PARA32.Value = DBNull.Value
                    Else
                        PARA32.Value = TxtActualAccDate.Text
                    End If
                    '空車着日（実績）
                    If TxtActualEmparrDate.Text = "" Then
                        PARA33.Value = DBNull.Value
                    Else
                        PARA33.Value = TxtActualEmparrDate.Text
                    End If

                    PARA34.Value = TxtRTank.Text                      '車数（レギュラー）
                    PARA35.Value = TxtHTank.Text                      '車数（ハイオク）
                    PARA36.Value = TxtTTank.Text                      '車数（灯油）
                    PARA37.Value = TxtMTTank.Text                     '車数（未添加灯油）
                    PARA38.Value = TxtKTank.Text                      '車数（軽油）
                    PARA39.Value = TxtK3Tank.Text                     '車数（３号軽油）
                    PARA40.Value = TxtK5Tank.Text                     '車数（５号軽油）
                    PARA41.Value = TxtK10Tank.Text                    '車数（１０号軽油）
                    PARA42.Value = TxtLTank.Text                      '車数（LSA）
                    PARA43.Value = TxtATank.Text                      '車数（A重油）
                    'PARA34.Value = work.WF_SEL_REGULAR_TANKCAR.Text   '車数（レギュラー）
                    'PARA35.Value = work.WF_SEL_HIGHOCTANE_TANKCAR.Text '車数（ハイオク）
                    'PARA36.Value = work.WF_SEL_KEROSENE_TANKCAR.Text  '車数（灯油）
                    'PARA37.Value = work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text  '車数（未添加灯油）
                    'PARA38.Value = work.WF_SEL_DIESEL_TANKCAR.Text    '車数（軽油）
                    'PARA39.Value = work.WF_SEL_NUM3DIESEL_TANKCAR.Text  '車数（３号軽油）
                    'PARA40.Value = work.WF_SEL_NUM5DIESEL_TANKCAR.Text  '車数（５号軽油）
                    'PARA41.Value = work.WF_SEL_NUM10DIESEL_TANKCAR.Text '車数（１０号軽油）
                    'PARA42.Value = work.WF_SEL_LSA_TANKCAR.Text       '車数（LSA）
                    'PARA43.Value = work.WF_SEL_AHEAVY_TANKCAR.Text    '車数（A重油）
                    PARA44.Value = 0                                  '車数（その他１）
                    PARA45.Value = 0                                  '車数（その他２）
                    PARA46.Value = 0                                  '車数（その他３）
                    PARA47.Value = 0                                  '車数（その他４）
                    PARA48.Value = 0                                  '車数（その他５）
                    PARA49.Value = 0                                  '車数（その他６）
                    PARA50.Value = 0                                  '車数（その他７）
                    PARA51.Value = 0                                  '車数（その他８）
                    PARA52.Value = 0                                  '車数（その他９）
                    PARA53.Value = 0                                  '車数（その他１０）
                    '合計車数
                    work.WF_SEL_TANKCARTOTAL.Text = Integer.Parse(TxtRTank.Text) _
                                                    + Integer.Parse(TxtHTank.Text) _
                                                    + Integer.Parse(TxtTTank.Text) _
                                                    + Integer.Parse(TxtMTTank.Text) _
                                                    + Integer.Parse(TxtKTank.Text) _
                                                    + Integer.Parse(TxtK3Tank.Text) _
                                                    + Integer.Parse(TxtK5Tank.Text) _
                                                    + Integer.Parse(TxtK10Tank.Text) _
                                                    + Integer.Parse(TxtLTank.Text) _
                                                    + Integer.Parse(TxtATank.Text)
                    PARA54.Value = work.WF_SEL_TANKCARTOTAL.Text

                    PARA55.Value = Integer.Parse(TxtRTank_w.Text)                    '変更後_車数（レギュラー）
                    PARA56.Value = Integer.Parse(TxtHTank_w.Text)                    '変更後_車数（ハイオク）
                    PARA57.Value = Integer.Parse(TxtTTank_w.Text)                    '変更後_車数（灯油）
                    PARA58.Value = Integer.Parse(TxtMTTank_w.Text)                   '変更後_車数（未添加灯油）
                    PARA59.Value = Integer.Parse(TxtKTank_w.Text)                    '変更後_車数（軽油）
                    PARA60.Value = Integer.Parse(TxtK3Tank_w.Text)                   '変更後_車数（３号軽油）
                    PARA61.Value = Integer.Parse(TxtK5Tank_w.Text)                   '変更後_車数（５号軽油）
                    PARA62.Value = Integer.Parse(TxtK10Tank_w.Text)                  '変更後_車数（１０号軽油）
                    PARA63.Value = Integer.Parse(TxtLTank_w.Text)                    '変更後_車数（LSA）
                    PARA64.Value = Integer.Parse(TxtATank_w.Text)                    '変更後_車数（A重油）
                    PARA65.Value = 0                                  '変更後_車数（その他１）
                    PARA66.Value = 0                                  '変更後_車数（その他２）
                    PARA67.Value = 0                                  '変更後_車数（その他３）
                    PARA68.Value = 0                                  '変更後_車数（その他４）
                    PARA69.Value = 0                                  '変更後_車数（その他５）
                    PARA70.Value = 0                                  '変更後_車数（その他６）
                    PARA71.Value = 0                                  '変更後_車数（その他７）
                    PARA72.Value = 0                                  '変更後_車数（その他８）
                    PARA73.Value = 0                                  '変更後_車数（その他９）
                    PARA74.Value = 0                                  '変更後_車数（その他１０）
                    '変更後_合計車数
                    work.WF_SEL_TANKCARTOTALCH.Text = Integer.Parse(TxtRTank_w.Text) _
                                                    + Integer.Parse(TxtHTank_w.Text) _
                                                    + Integer.Parse(TxtTTank_w.Text) _
                                                    + Integer.Parse(TxtMTTank_w.Text) _
                                                    + Integer.Parse(TxtKTank_w.Text) _
                                                    + Integer.Parse(TxtK3Tank_w.Text) _
                                                    + Integer.Parse(TxtK5Tank_w.Text) _
                                                    + Integer.Parse(TxtK10Tank_w.Text) _
                                                    + Integer.Parse(TxtLTank_w.Text) _
                                                    + Integer.Parse(TxtATank_w.Text)
                    PARA75.Value = Integer.Parse(work.WF_SEL_TANKCARTOTALCH.Text)

                    PARA76.Value = work.WF_SEL_LINK_LINKNO.Text       '貨車連結順序表№
                    PARA91.Value = DBNull.Value                '計上日
                    'PARA91.Value = TxtBudgetMonth.Text                '計上日
                    PARA77.Value = 0                                  '売上金額
                    PARA78.Value = 0                                  '売上消費税額
                    PARA79.Value = 0                                  '売上合計金額
                    PARA80.Value = 0                                  '支払金額
                    PARA81.Value = 0                                  '支払消費税額
                    PARA82.Value = 0                                  '支払合計金額
                    'PARA77.Value = TxtTotalSales.Text                 '売上金額
                    'PARA78.Value = Integer.Parse(TxtTotalSales2.Text) - Integer.Parse(TxtTotalSales.Text) '売上消費税額
                    'PARA79.Value = TxtTotalSales2.Text                '売上合計金額
                    'PARA80.Value = TxtTitalPayment.Text               '支払金額
                    'PARA81.Value = Integer.Parse(TxtTitalPayment2.Text) - Integer.Parse(TxtTitalPayment.Text) '支払消費税額
                    'PARA82.Value = TxtTitalPayment2.Text              '支払合計金額
                    'PARA83.Value = OIT0003row("DELFLG")               '削除フラグ
                    PARA83.Value = "0"                                '削除フラグ
                    PARA84.Value = WW_DATENOW                         '登録年月日
                    PARA85.Value = Master.USERID                      '登録ユーザーID
                    PARA86.Value = Master.USERTERMID                  '登録端末
                    PARA87.Value = WW_DATENOW                         '更新年月日
                    PARA88.Value = Master.USERID                      '更新ユーザーID
                    PARA89.Value = Master.USERTERMID                  '更新端末
                    PARA90.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = work.WF_SEL_ORDERNUMBER.Text

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0003UPDtbl) Then
                            OIT0003UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0003UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0003UPDtbl.Clear()
                        OIT0003UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0003UPDrow As DataRow In OIT0003UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0003D"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0003UPDrow
                        CS0020JOURNAL.CS0020JOURNAL()
                        If Not isNormal(CS0020JOURNAL.ERR) Then
                            Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                            CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                            CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                            CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                            Exit Sub
                        End If
                    Next
                    'End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D UPDATE_INSERT_ORDER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D UPDATE_INSERT_ORDER"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注明細TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderDetail(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0003_DETAIL" _
            & "    WHERE" _
            & "        ORDERNO  = @P01" _
            & "   AND  DETAILNO = @P02" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0003_DETAIL" _
            & "    SET" _
            & "        LINEORDER             = @P33, TANKNO               = @P03" _
            & "        , ORDERINFO           = @P37, SHIPPERSCODE         = @P23, SHIPPERSNAME        = @P24" _
            & "        , OILCODE             = @P05, OILNAME              = @P34, ORDERINGTYPE        = @P35" _
            & "        , ORDERINGOILNAME     = @P36, RETURNDATETRAIN      = @P07, JOINT               = @P08" _
            & "        , CHANGETRAINNO       = @P26, CHANGETRAINNAME      = @P38" _
            & "        , SECONDCONSIGNEECODE = @P27, SECONDCONSIGNEENAME  = @P28" _
            & "        , SECONDARRSTATION    = @P29, SECONDARRSTATIONNAME = @P30" _
            & "        , CANGERETSTATION     = @P31, CHANGEARRSTATIONNAME = @P32" _
            & "        , SALSE               = @P09, SALSETAX             = @P10, TOTALSALSE   = @P11" _
            & "        , PAYMENT             = @P12, PAYMENTTAX           = @P13, TOTALPAYMENT = @P14" _
            & "        , UPDYMD              = @P19, UPDUSER              = @P20" _
            & "        , UPDTERMID           = @P21, RECEIVEYMD           = @P22" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & "        AND DETAILNO     = @P02" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0003_DETAIL" _
            & "        ( ORDERNO         , DETAILNO            , LINEORDER          , TANKNO             " _
            & "        , KAMOKU          , ORDERINFO           , SHIPPERSCODE       , SHIPPERSNAME" _
            & "        , OILCODE         , OILNAME             , ORDERINGTYPE       , ORDERINGOILNAME" _
            & "        , CARSNUMBER      , CARSAMOUNT          , RETURNDATETRAIN    , JOINT" _
            & "        , CHANGETRAINNO   , CHANGETRAINNAME     , SECONDCONSIGNEECODE, SECONDCONSIGNEENAME" _
            & "        , SECONDARRSTATION, SECONDARRSTATIONNAME, CANGERETSTATION    , CHANGEARRSTATIONNAME" _
            & "        , SALSE           , SALSETAX            , TOTALSALSE" _
            & "        , PAYMENT         , PAYMENTTAX          , TOTALPAYMENT" _
            & "        , DELFLG          , INITYMD             , INITUSER           , INITTERMID" _
            & "        , UPDYMD          , UPDUSER             , UPDTERMID          , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P33, @P03" _
            & "        , @P04, @P37, @P23, @P24" _
            & "        , @P05, @P34, @P35, @P36" _
            & "        , @P06, @P25, @P07, @P08" _
            & "        , @P26, @P38, @P27, @P28" _
            & "        , @P29, @P30, @P31, @P32" _
            & "        , @P09, @P10, @P11" _
            & "        , @P12, @P13, @P14" _
            & "        , @P15, @P16, @P17, @P18" _
            & "        , @P19, @P20, @P21, @P22) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    ORDERNO" _
            & "    , DETAILNO" _
            & "    , LINEORDER" _
            & "    , TANKNO" _
            & "    , KAMOKU" _
            & "    , ORDERINFO" _
            & "    , SHIPPERSCODE" _
            & "    , SHIPPERSNAME" _
            & "    , OILCODE" _
            & "    , OILNAME" _
            & "    , ORDERINGTYPE" _
            & "    , ORDERINGOILNAME" _
            & "    , CARSNUMBER" _
            & "    , CARSAMOUNT" _
            & "    , RETURNDATETRAIN" _
            & "    , JOINT" _
            & "    , CHANGETRAINNO" _
            & "    , CHANGETRAINNAME" _
            & "    , SECONDCONSIGNEECODE" _
            & "    , SECONDCONSIGNEENAME" _
            & "    , SECONDARRSTATION" _
            & "    , SECONDARRSTATIONNAME" _
            & "    , CANGERETSTATION" _
            & "    , CHANGEARRSTATIONNAME" _
            & "    , SALSE" _
            & "    , SALSETAX" _
            & "    , TOTALSALSE" _
            & "    , PAYMENT" _
            & "    , PAYMENTTAX" _
            & "    , TOTALPAYMENT" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & " FROM" _
            & "    OIL.OIT0003_DETAIL" _
            & " WHERE" _
            & "        ORDERNO  = @P01" _
            & "   AND  DETAILNO = @P02"
        '& "    , UPDTIMSTP" _

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11)  '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)   '受注明細№
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 2)   '貨物駅入線順
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 8)   'タンク車№
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 7)   '費用科目
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.NVarChar, 2)   '受注情報
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 10)  '荷主コード
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 10)  '荷主名
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 4)   '油種コード
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.NVarChar, 40)  '油種名
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.NVarChar, 2)   '油種区分(受発注用)
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.NVarChar, 40)  '油種名(受発注用)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Int)           '車数
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Int)           '数量
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.DateTime)      '返送日列車
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 200) 'ジョイント
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 4)   '本線列車（変更後）
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.NVarChar, 4)   '本線列車名（変更後）
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 10)  '第2荷受人コード
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.NVarChar, 40)  '第2荷受人名
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 7)   '第2着駅コード
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 40)  '第2着駅名
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.NVarChar, 7)   '空車着駅コード（変更後）
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 40)  '空車着駅名（変更後）
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.Int)           '売上金額
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.Int)           '売上消費税額
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.Int)           '売上合計金額
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Int)           '支払金額
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Int)           '支払消費税額
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.Int)           '支払合計金額
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 1)   '削除フラグ
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.DateTime)      '登録年月日
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)  '登録ユーザーID
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)  '登録端末
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.DateTime)      '更新年月日
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)  '更新ユーザーID
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 20)  '更新端末
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.DateTime)      '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 4) '受注№
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 3) '受注明細№

                For Each OIT0003row As DataRow In OIT0003tbl.Rows

                    Dim WW_DATENOW As DateTime = Date.Now

                    'DB更新
                    PARA01.Value = work.WF_SEL_ORDERNUMBER.Text       '受注№
                    PARA02.Value = OIT0003row("DETAILNO")             '受注明細№
                    PARA33.Value = OIT0003row("LINEORDER")            '貨物駅入線順
                    PARA03.Value = OIT0003row("TANKNO")               'タンク車№
                    PARA04.Value = ""                                 '費用科目

                    '# 受注情報
                    '交付アラートが「3日以内のタンク車」または「4日～6日のタンク車」の場合
                    If OIT0003row("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW _
                        OrElse OIT0003row("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Then
                        PARA37.Value = BaseDllConst.CONST_ORDERINFO_ALERT_82

                        '全検アラートが「3日以内のタンク車」または「4日～6日のタンク車」の場合
                    ElseIf OIT0003row("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW _
                        OrElse OIT0003row("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Then
                        PARA37.Value = BaseDllConst.CONST_ORDERINFO_ALERT_82
                    Else
                        PARA37.Value = ""
                    End If

                    PARA23.Value = OIT0003row("SHIPPERSCODE")         '荷主コード
                    PARA24.Value = OIT0003row("SHIPPERSNAME")         '荷主名
                    PARA05.Value = OIT0003row("OILCODE")              '油種コード
                    PARA34.Value = OIT0003row("OILNAME")              '油種名
                    PARA35.Value = OIT0003row("ORDERINGTYPE")         '油種区分(受発注用)
                    PARA36.Value = OIT0003row("ORDERINGOILNAME")      '油種名(受発注用)
                    PARA06.Value = "1"                                '車数
                    PARA25.Value = "0"                                '数量
                    PARA07.Value = DBNull.Value                       '返送日列車
                    PARA08.Value = DBNull.Value                       'ジョイント
                    PARA26.Value = OIT0003row("CHANGETRAINNO")        '本線列車（変更後）
                    PARA38.Value = OIT0003row("CHANGETRAINNAME")      '本線列車名（変更後）
                    PARA27.Value = OIT0003row("SECONDCONSIGNEECODE")  '第2荷受人コード
                    PARA28.Value = OIT0003row("SECONDCONSIGNEENAME")  '第2荷受人名
                    PARA29.Value = OIT0003row("SECONDARRSTATION")     '第2着駅コード
                    PARA30.Value = OIT0003row("SECONDARRSTATIONNAME") '第2着駅名
                    PARA31.Value = OIT0003row("CANGERETSTATION")      '空車着駅コード（変更後）
                    PARA32.Value = OIT0003row("CHANGEARRSTATIONNAME") '空車着駅名（変更後）
                    PARA09.Value = "0"                                '売上金額
                    PARA10.Value = "0"                                '売上消費税額
                    PARA11.Value = "0"                                '売上合計金額
                    PARA12.Value = "0"                                '支払金額
                    PARA13.Value = "0"                                '支払消費税額
                    PARA14.Value = "0"                                '支払合計金額
                    PARA15.Value = OIT0003row("DELFLG")               '削除フラグ
                    PARA16.Value = WW_DATENOW                         '登録年月日
                    PARA17.Value = Master.USERID                      '登録ユーザーID
                    PARA18.Value = Master.USERTERMID                  '登録端末
                    PARA19.Value = WW_DATENOW                         '更新年月日
                    PARA20.Value = Master.USERID                      '更新ユーザーID
                    PARA21.Value = Master.USERTERMID                  '更新端末
                    PARA22.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = work.WF_SEL_ORDERNUMBER.Text      '受注№
                    JPARA02.Value = OIT0003row("DETAILNO")            '受注明細№

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0003UPDtbl) Then
                            OIT0003UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0003UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0003UPDtbl.Clear()
                        OIT0003UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0003UPDrow As DataRow In OIT0003UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0003L"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0003UPDrow
                        CS0020JOURNAL.CS0020JOURNAL()
                        If Not isNormal(CS0020JOURNAL.ERR) Then
                            Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                            CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                            CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                            CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                            Exit Sub
                        End If
                    Next
                    'End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D UPDATE_INSERT_ORDERDETAIL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D UPDATE_INSERT_ORDERDETAIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 貨車連結表TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateLink(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0004_LINK" _
            & "    WHERE" _
            & "        LINKNO  = @P01" _
            & "   AND  LINKDETAILNO = @P02" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0004_LINK" _
            & "    SET" _
            & "        AVAILABLEYMD      = @P03" _
            & "        , STATUS          = @P04, INFO               = @P05, PREORDERNO = @P06" _
            & "        , TRAINNO         = @P07, TRAINNAME          = @P08, OFFICECODE = @P09" _
            & "        , DEPSTATION      = @P10, DEPSTATIONNAME     = @P11" _
            & "        , RETSTATION      = @P12, RETSTATIONNAME     = @P13" _
            & "        , EMPARRDATE      = @P14, ACTUALEMPARRDATE   = @P15" _
            & "        , LINETRAINNO     = @P16, LINEORDER          = @P17" _
            & "        , TANKNUMBER      = @P18, PREOILCODE         = @P19, PREOILNAME = @P20" _
            & "        , PREORDERINGTYPE = @P21, PREORDERINGOILNAME = @P22, DELFLG     = @P23" _
            & "        , INITYMD         = @P24, INITUSER           = @P25, INITTERMID = @P26" _
            & "        , UPDYMD          = @P27, UPDUSER            = @P28, UPDTERMID  = @P29" _
            & "        , RECEIVEYMD      = @P30" _
            & "    WHERE" _
            & "        LINKNO            = @P01" _
            & "        AND LINKDETAILNO  = @P02" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0004_LINK" _
            & "        ( LINKNO     , LINKDETAILNO  , AVAILABLEYMD    , STATUS " _
            & "        , INFO       , PREORDERNO    , TRAINNO         , TRAINNAME " _
            & "        , OFFICECODE , DEPSTATION    , DEPSTATIONNAME" _
            & "        , RETSTATION , RETSTATIONNAME, EMPARRDATE      , ACTUALEMPARRDATE " _
            & "        , LINETRAINNO, LINEORDER     , TANKNUMBER " _
            & "        , PREOILCODE , PREOILNAME    , PREORDERINGTYPE , PREORDERINGOILNAME" _
            & "        , DELFLG     , INITYMD       , INITUSER        , INITTERMID " _
            & "        , UPDYMD     , UPDUSER       , UPDTERMID       , RECEIVEYMD) " _
            & "    VALUES" _
            & "        ( @P01, @P02, @P03, @P04" _
            & "        , @P05, @P06, @P07, @P08" _
            & "        , @P09, @P10, @P11" _
            & "        , @P12, @P13, @P14, @P15" _
            & "        , @P16, @P17, @P18" _
            & "        , @P19, @P20, @P21, @P22" _
            & "        , @P23, @P24, @P25, @P26" _
            & "        , @P27, @P28, @P29, @P30) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    LINKNO" _
            & "    , LINKDETAILNO" _
            & "    , AVAILABLEYMD" _
            & "    , STATUS" _
            & "    , INFO" _
            & "    , PREORDERNO" _
            & "    , TRAINNO" _
            & "    , TRAINNAME" _
            & "    , OFFICECODE" _
            & "    , DEPSTATION" _
            & "    , DEPSTATIONNAME" _
            & "    , RETSTATION" _
            & "    , RETSTATIONNAME" _
            & "    , EMPARRDATE" _
            & "    , ACTUALEMPARRDATE" _
            & "    , LINETRAINNO" _
            & "    , LINEORDER" _
            & "    , TANKNUMBER" _
            & "    , PREOILCODE" _
            & "    , PREOILNAME" _
            & "    , PREORDERINGTYPE" _
            & "    , PREORDERINGOILNAME" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & " FROM" _
            & "    OIL.OIT0004_LINK" _
            & " WHERE" _
            & "        LINKNO       = @P01" _
            & "   AND  LINKDETAILNO = @P02"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '貨車連結順序表明細№
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '利用可能日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  'ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '情報
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 11) '前回オーダー№
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 20) '本線列車名
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 6)  '登録営業所コード
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 7)  '空車発駅コード
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 40) '空車発駅名
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 7)  '空車着駅コード
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 40) '空車着駅名
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.Date)         '空車着日（予定）
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.Date)         '空車着日（実績）
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 4)  '入線列車番号
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 2)  '入線順
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 8)  'タンク車№
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 4)  '前回油種
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 40) '前回油種名
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 2)  '前回油種区分(受発注用)
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 40) '前回油種名(受発注用)

                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.DateTime)     '登録年月日
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 20) '登録ユーザーID
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 20) '登録端末
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.DateTime)     '更新年月日
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.NVarChar, 20) '更新ユーザーID
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 20) '更新端末
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.DateTime)     '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '貨車連結順序表明細№

                Dim WW_DATENOW As DateTime = Date.Now
                Dim WW_GetValue() As String = {""}
                Dim iNewLineOrder As Integer = 1

                '新規で貨車連結順序表を作成する場合
                If work.WF_SEL_LINKNO_ORDER.Text = "" Then
                    '貨車連結順序表の新規№を取得
                    WW_FixvalueMasterSearch("ZZ", "NEWLINKNOGET", "", WW_GetValue)
                    work.WF_SEL_LINKNO_ORDER.Text = WW_GetValue(0)
                End If

                For Each OIT0003row As DataRow In OIT0003tbl_tab3.Select(Nothing, "LINEORDER DESC")

                    PARA01.Value = work.WF_SEL_LINKNO_ORDER.Text     '貨車連結順序表№
                    PARA02.Value = OIT0003row("DETAILNO")            '貨車連結順序表明細№
                    PARA03.Value = WW_DATENOW.AddDays(1)             '利用可能日

                    '(実績)発日が入力されている場合
                    If Me.TxtActualDepDate.Text <> "" Then
                        'ステータス(1:利用可, 2:利用不可)
                        PARA04.Value = "1"
                    Else
                        'ステータス(1:利用可, 2:利用不可)
                        PARA04.Value = "2"
                    End If

                    PARA05.Value = ""                                '情報
                    PARA06.Value = work.WF_SEL_ORDERNUMBER.Text      '前回オーダー№
                    PARA07.Value = Me.TxtTrainNo.Text                '本線列車
                    PARA08.Value = Me.TxtTrainName.Text              '本線列車名
                    PARA09.Value = Me.TxtOrderOfficeCode.Text        '登録営業所コード

                    '(一覧)空車着駅(変更)が設定されている場合
                    If OIT0003row("SECONDARRSTATION") <> "" Then
                        '空車発駅コード
                        PARA10.Value = OIT0003row("SECONDARRSTATION")       '←　(一覧)の内容を設定
                        '空車発駅名
                        PARA11.Value = OIT0003row("SECONDARRSTATIONNAME")   '←　(一覧)の内容を設定
                    Else
                        '空車発駅コード
                        PARA10.Value = Me.TxtArrstationCode.Text
                        '空車発駅名
                        PARA11.Value = Me.LblArrstationName.Text
                    End If

                    PARA12.Value = Me.TxtDepstationCode.Text         '空車着駅コード
                    PARA13.Value = Me.LblDepstationName.Text         '空車着駅名

                    '空車着日（予定）
                    If Me.TxtEmparrDate.Text <> "" Then
                        PARA14.Value = Me.TxtEmparrDate.Text
                    Else
                        PARA14.Value = DBNull.Value
                    End If

                    PARA15.Value = DBNull.Value                      '空車着日（実績）
                    PARA16.Value = ""         '入線列車番号
                    'PARA16.Value = OIT0003row("LINETRAINNO")         '入線列車番号
                    PARA17.Value = iNewLineOrder                      '入線順
                    'PARA17.Value = OIT0003row("LINEORDER")           '入線順

                    PARA18.Value = OIT0003row("TANKNO")              'タンク車№
                    PARA19.Value = OIT0003row("OILCODE")             '油種コード
                    PARA20.Value = OIT0003row("OILNAME")             '油種名
                    PARA21.Value = OIT0003row("ORDERINGTYPE")        '油種区分(受発注用)
                    PARA22.Value = OIT0003row("ORDERINGOILNAME")     '油種名(受発注用)

                    PARA23.Value = OIT0003row("DELFLG")              '削除フラグ
                    PARA24.Value = WW_DATENOW                        '登録年月日
                    PARA25.Value = Master.USERID                     '登録ユーザーID
                    PARA26.Value = Master.USERTERMID                 '登録端末
                    PARA27.Value = WW_DATENOW                        '更新年月日
                    PARA28.Value = Master.USERID                     '更新ユーザーID
                    PARA29.Value = Master.USERTERMID                 '更新端末
                    PARA30.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = work.WF_SEL_LINKNO_ORDER.Text    '貨車連結順序表№
                    JPARA02.Value = OIT0003row("DETAILNO")           '貨車連結順序表明細№

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0003UPDtbl) Then
                            OIT0003UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0003UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0003UPDtbl.Clear()
                        OIT0003UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0003UPDrow As DataRow In OIT0003UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0003D"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0003UPDrow
                        CS0020JOURNAL.CS0020JOURNAL()
                        If Not isNormal(CS0020JOURNAL.ERR) Then
                            Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                            CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                            CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                            CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                            Exit Sub
                        End If
                    Next

                    iNewLineOrder += 1
                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D UPDATE_INSERT_LINK")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D UPDATE_INSERT_LINK"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (受注TBL)タンク車数更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderTankCnt(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0003WKtbl) Then
            OIT0003WKtbl = New DataTable
        End If

        If OIT0003WKtbl.Columns.Count <> 0 Then
            OIT0003WKtbl.Columns.Clear()
        End If

        OIT0003WKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     受注明細テーブルから油種別タンク車数を取得する
        Dim SQLStr As String =
              " SELECT DISTINCT" _
            & "   ISNULL(RTRIM(OIT0003.ORDERNO), '') AS ORDERNO" _
            & " , ISNULL(RTRIM(OIM0003.OILCODE), '') AS OILCODE" _
            & " , ISNULL(RTRIM(OIM0003.OILNAME), '') AS OILNAME" _
            & " , CAST(SUM(1) OVER(PARTITION BY OIM0003.OILCODE ORDER BY OIM0003.OILCODE) AS int) AS CNT" _
            & " FROM OIL.OIT0003_DETAIL OIT0003 " _
            & "  INNER JOIN OIL.OIM0003_PRODUCT OIM0003 ON" _
            & "        OIM0003.OFFICECODE     = @P02 " _
            & "    AND OIM0003.OILCODE        = OIT0003.OILCODE" _
            & "    AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE" _
            & " WHERE OIT0003.ORDERNO = @P01" _
            & "   AND OIT0003.DELFLG <> @P03"

        '更新SQL文･･･受注TBLのタンク車数を更新
        Dim SQLUpStr As String =
                    " UPDATE OIL.OIT0002_ORDER           " _
                    & "    SET HTANKCH        = @P11,      " _
                    & "        RTANKCH        = @P12,      " _
                    & "        TTANKCH        = @P13,      " _
                    & "        MTTANKCH       = @P14,      " _
                    & "        KTANKCH        = @P15,      " _
                    & "        K3TANKCH       = @P16,      " _
                    & "        K5TANKCH       = @P17,      " _
                    & "        K10TANKCH      = @P18,      " _
                    & "        LTANKCH        = @P19,      " _
                    & "        ATANKCH        = @P20,      " _
                    & "        OTHER1OTANKCH  = @P21,      " _
                    & "        OTHER2OTANKCH  = @P22,      " _
                    & "        OTHER3OTANKCH  = @P23,      " _
                    & "        OTHER4OTANKCH  = @P24,      " _
                    & "        OTHER5OTANKCH  = @P25,      " _
                    & "        OTHER6OTANKCH  = @P26,      " _
                    & "        OTHER7OTANKCH  = @P27,      " _
                    & "        OTHER8OTANKCH  = @P28,      " _
                    & "        OTHER9OTANKCH  = @P29,      " _
                    & "        OTHER10OTANKCH = @P30,      " _
                    & "        TOTALTANKCH    = @P31,      " _
                    & "        UPDYMD         = @P32,      " _
                    & "        UPDUSER        = @P33,      " _
                    & "        UPDTERMID      = @P34,      " _
                    & "        RECEIVEYMD     = @P35,      " _
                    & "        ORDERINFO      = @P36       " _
                    & "  WHERE ORDERNO        = @P01       " _
                    & "    AND OFFICECODE     = @P02       " _
                    & "    AND DELFLG        <> @P03      ;"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLUpcmd As New SqlCommand(SQLUpStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA2.Value = TxtOrderOfficeCode.Text
                'PARA2.Value = work.WF_SEL_SALESOFFICECODE.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Dim PARAUP01 As SqlParameter = SQLUpcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARAUP02 As SqlParameter = SQLUpcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARAUP03 As SqlParameter = SQLUpcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARAUP11 As SqlParameter = SQLUpcmd.Parameters.Add("@P11", SqlDbType.Int)          '車数（ハイオク）
                Dim PARAUP12 As SqlParameter = SQLUpcmd.Parameters.Add("@P12", SqlDbType.Int)          '車数（レギュラー）
                Dim PARAUP13 As SqlParameter = SQLUpcmd.Parameters.Add("@P13", SqlDbType.Int)          '車数（灯油）
                Dim PARAUP14 As SqlParameter = SQLUpcmd.Parameters.Add("@P14", SqlDbType.Int)          '車数（未添加灯油）
                Dim PARAUP15 As SqlParameter = SQLUpcmd.Parameters.Add("@P15", SqlDbType.Int)          '車数（軽油）
                Dim PARAUP16 As SqlParameter = SQLUpcmd.Parameters.Add("@P16", SqlDbType.Int)          '車数（３号軽油）
                Dim PARAUP17 As SqlParameter = SQLUpcmd.Parameters.Add("@P17", SqlDbType.Int)          '車数（５号軽油）
                Dim PARAUP18 As SqlParameter = SQLUpcmd.Parameters.Add("@P18", SqlDbType.Int)          '車数（１０号軽油）
                Dim PARAUP19 As SqlParameter = SQLUpcmd.Parameters.Add("@P19", SqlDbType.Int)          '車数（LSA）
                Dim PARAUP20 As SqlParameter = SQLUpcmd.Parameters.Add("@P20", SqlDbType.Int)          '車数（A重油）
                Dim PARAUP21 As SqlParameter = SQLUpcmd.Parameters.Add("@P21", SqlDbType.Int)          '車数（その他１）
                Dim PARAUP22 As SqlParameter = SQLUpcmd.Parameters.Add("@P22", SqlDbType.Int)          '車数（その他２）
                Dim PARAUP23 As SqlParameter = SQLUpcmd.Parameters.Add("@P23", SqlDbType.Int)          '車数（その他３）
                Dim PARAUP24 As SqlParameter = SQLUpcmd.Parameters.Add("@P24", SqlDbType.Int)          '車数（その他４）
                Dim PARAUP25 As SqlParameter = SQLUpcmd.Parameters.Add("@P25", SqlDbType.Int)          '車数（その他５）
                Dim PARAUP26 As SqlParameter = SQLUpcmd.Parameters.Add("@P26", SqlDbType.Int)          '車数（その他６）
                Dim PARAUP27 As SqlParameter = SQLUpcmd.Parameters.Add("@P27", SqlDbType.Int)          '車数（その他７）
                Dim PARAUP28 As SqlParameter = SQLUpcmd.Parameters.Add("@P28", SqlDbType.Int)          '車数（その他８）
                Dim PARAUP29 As SqlParameter = SQLUpcmd.Parameters.Add("@P29", SqlDbType.Int)          '車数（その他９）
                Dim PARAUP30 As SqlParameter = SQLUpcmd.Parameters.Add("@P30", SqlDbType.Int)          '車数（その他１０）
                Dim PARAUP31 As SqlParameter = SQLUpcmd.Parameters.Add("@P31", SqlDbType.Int)          '合計車数
                Dim PARAUP32 As SqlParameter = SQLUpcmd.Parameters.Add("@P32", SqlDbType.DateTime)
                Dim PARAUP33 As SqlParameter = SQLUpcmd.Parameters.Add("@P33", SqlDbType.NVarChar)
                Dim PARAUP34 As SqlParameter = SQLUpcmd.Parameters.Add("@P34", SqlDbType.NVarChar)
                Dim PARAUP35 As SqlParameter = SQLUpcmd.Parameters.Add("@P35", SqlDbType.DateTime)
                Dim PARAUP36 As SqlParameter = SQLUpcmd.Parameters.Add("@P36", SqlDbType.NVarChar)     '受注情報
                PARAUP01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARAUP02.Value = TxtOrderOfficeCode.Text
                'PARAUP02.Value = work.WF_SEL_SALESOFFICECODE.Text
                PARAUP03.Value = C_DELETE_FLG.DELETE

                PARAUP11.Value = "0"
                PARAUP12.Value = "0"
                PARAUP13.Value = "0"
                PARAUP14.Value = "0"
                PARAUP15.Value = "0"
                PARAUP16.Value = "0"
                PARAUP17.Value = "0"
                PARAUP18.Value = "0"
                PARAUP19.Value = "0"
                PARAUP20.Value = "0"
                PARAUP21.Value = "0"
                PARAUP22.Value = "0"
                PARAUP23.Value = "0"
                PARAUP24.Value = "0"
                PARAUP25.Value = "0"
                PARAUP26.Value = "0"
                PARAUP27.Value = "0"
                PARAUP28.Value = "0"
                PARAUP29.Value = "0"
                PARAUP30.Value = "0"
                PARAUP31.Value = "0"
                PARAUP36.Value = ""

                '各タンク車件数を初期化
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

                TxtHTank_w.Text = "0"
                TxtRTank_w.Text = "0"
                TxtTTank_w.Text = "0"
                TxtMTTank_w.Text = "0"
                TxtKTank_w.Text = "0"
                TxtK3Tank_w.Text = "0"
                TxtK5Tank_w.Text = "0"
                TxtK10Tank_w.Text = "0"
                TxtLTank_w.Text = "0"
                TxtATank_w.Text = "0"
                'TxtTotalTank.Text = "0"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim iTotalTank = 0
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}

                '〇 検索(営業所).テキストボックスが未設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    '〇 画面(受注営業所).テキストボックスが未設定
                    If work.WF_SEL_ORDERSALESOFFICECODE.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "TRAINNUMBER_FIND", TxtTrainName.Text, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TRAINNUMBER_FIND", TxtTrainName.Text, WW_GetValue)
                    End If
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER_FIND", TxtTrainName.Text, WW_GetValue)
                End If
                'WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER", TxtTrainNo.Text, WW_GetValue)

                For Each OIT0003UPDrow As DataRow In OIT0003WKtbl.Rows

                    Select Case OIT0003UPDrow("OILCODE")
                        Case BaseDllConst.CONST_HTank
                            PARAUP11.Value = OIT0003UPDrow("CNT")
                            TxtHTank_c.Text = OIT0003UPDrow("CNT")
                            TxtHTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_RTank
                            PARAUP12.Value = OIT0003UPDrow("CNT")
                            TxtRTank_c.Text = OIT0003UPDrow("CNT")
                            TxtRTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_TTank
                            PARAUP13.Value = OIT0003UPDrow("CNT")
                            TxtTTank_c.Text = OIT0003UPDrow("CNT")
                            TxtTTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_MTTank
                            PARAUP14.Value = OIT0003UPDrow("CNT")
                            TxtMTTank_c.Text = OIT0003UPDrow("CNT")
                            TxtMTTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_KTank1, BaseDllConst.CONST_KTank2
                            PARAUP15.Value = OIT0003UPDrow("CNT")
                            TxtKTank_c.Text = OIT0003UPDrow("CNT")
                            TxtKTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_K3Tank1, BaseDllConst.CONST_K3Tank2
                            PARAUP16.Value = OIT0003UPDrow("CNT")
                            TxtK3Tank_c.Text = OIT0003UPDrow("CNT")
                            TxtK3Tank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_K5Tank
                            PARAUP17.Value = OIT0003UPDrow("CNT")
                            TxtK5Tank_c.Text = OIT0003UPDrow("CNT")
                            TxtK5Tank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_K10Tank
                            PARAUP18.Value = OIT0003UPDrow("CNT")
                            TxtK10Tank_c.Text = OIT0003UPDrow("CNT")
                            TxtK10Tank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_LTank1, BaseDllConst.CONST_LTank2
                            PARAUP19.Value = OIT0003UPDrow("CNT")
                            TxtLTank_c.Text = OIT0003UPDrow("CNT")
                            TxtLTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_ATank
                            PARAUP20.Value = OIT0003UPDrow("CNT")
                            TxtATank_c.Text = OIT0003UPDrow("CNT")
                            TxtATank_w.Text = OIT0003UPDrow("CNT")
                    End Select

                    i += OIT0003UPDrow("CNT")
                    TxtTotal_c.Text = i
                    iTotalTank = i
                    PARAUP31.Value = i
                    PARAUP32.Value = Date.Now
                    PARAUP33.Value = Master.USERID
                    PARAUP34.Value = Master.USERTERMID
                    PARAUP35.Value = C_DEFAULT_YMD

                    ''受付情報が「検査間近有」の場合は優先して設定
                    'If WW_ORDERINFOALERMFLG_82 = True Then
                    '    PARAUP36.Value = BaseDllConst.CONST_ORDERINFO_ALERT_82

                    'タンク車数が「最大牽引タンク車数」より大きい場合
                    If Integer.Parse(iTotalTank) > Integer.Parse(WW_GetValue(3)) Then
                        '80(タンク車数オーバー)を設定
                        PARAUP36.Value = BaseDllConst.CONST_ORDERINFO_ALERT_80

                    ElseIf Integer.Parse(iTotalTank) <= Integer.Parse(WW_GetValue(3)) Then
                        PARAUP36.Value = ""

                    End If

                    SQLUpcmd.ExecuteNonQuery()
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D ORDERTANKCNTSET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D ORDERTANKCNTSET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub
#End Region

#Region "タブ「入換・積込指示」各テーブル更新"
    ''' <summary>
    ''' 受注明細TBL更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderDetail_TAB2(ByVal SQLcon As SqlConnection)

        Try
            '更新SQL文･･･受注明細TBLの各項目をを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0003_DETAIL " _
                    & "    SET LINE                    = @P04, " _
                    & "        FILLINGPOINT            = @P05, " _
                    & "        LOADINGIRILINETRAINNO   = @P06, " _
                    & "        LOADINGIRILINETRAINNAME = @P07, " _
                    & "        LOADINGIRILINEORDER     = @P08, " _
                    & "        LOADINGOUTLETTRAINNO    = @P09, " _
                    & "        LOADINGOUTLETTRAINNAME  = @P10, " _
                    & "        LOADINGOUTLETORDER      = @P11, " _
                    & "        UPDYMD                  = @P12, " _
                    & "        UPDUSER                 = @P13, " _
                    & "        UPDTERMID               = @P14, " _
                    & "        RECEIVEYMD              = @P15  " _
                    & "  WHERE ORDERNO                 = @P01  " _
                    & "    AND DETAILNO                = @P02  " _
                    & "    AND DELFLG                 <> @P03; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  '受注№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '受注明細No
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)  '削除フラグ
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)  '回線
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)  '充填ポイント
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)  '積込入線列車番号
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)  '積込入線列車番号名
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)  '積込入線順
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)  '積込出線列車番号
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)  '積込出線列車番号名
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar)  '積込出線順

            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)  '更新年月日
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)  '更新ユーザーＩＤ
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar)  '更新端末
            Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.DateTime)  '集信日時

            For Each OIT0003tab2row As DataRow In OIT0003tbl_tab2.Rows
                PARA01.Value = OIT0003tab2row("ORDERNO")
                PARA02.Value = OIT0003tab2row("DETAILNO")
                PARA03.Value = C_DELETE_FLG.DELETE
                PARA04.Value = OIT0003tab2row("LINE")
                PARA05.Value = OIT0003tab2row("FILLINGPOINT")
                PARA06.Value = OIT0003tab2row("LOADINGIRILINETRAINNO")
                PARA07.Value = OIT0003tab2row("LOADINGIRILINETRAINNAME")
                PARA08.Value = OIT0003tab2row("LOADINGIRILINEORDER")
                PARA09.Value = OIT0003tab2row("LOADINGOUTLETTRAINNO")
                PARA10.Value = OIT0003tab2row("LOADINGOUTLETTRAINNAME")
                PARA11.Value = OIT0003tab2row("LOADINGOUTLETORDER")

                PARA12.Value = Date.Now
                PARA13.Value = Master.USERID
                PARA14.Value = Master.USERTERMID
                PARA15.Value = C_DEFAULT_YMD

                SQLcmd.ExecuteNonQuery()
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_ORDERDETAIL_TAB2 UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_ORDERDETAIL_TAB2 UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

#End Region

#Region "タブ「タンク車明細」各テーブル更新"
    ''' <summary>
    ''' 受注TBL更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrder_TAB3(ByVal SQLcon As SqlConnection)

        Try
            '更新SQL文･･･受注TBLの各項目をを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0002_ORDER " _
                    & "    SET ACTUALLODDATE    = @P03, " _
                    & "        ACTUALDEPDATE    = @P04, " _
                    & "        ACTUALARRDATE    = @P05, " _
                    & "        ACTUALACCDATE    = @P06, " _
                    & "        ACTUALEMPARRDATE = @P07, " _
                    & "        TANKRINKNOMADE   = @P12, " _
                    & "        UPDYMD           = @P08, " _
                    & "        UPDUSER          = @P09, " _
                    & "        UPDTERMID        = @P10, " _
                    & "        RECEIVEYMD       = @P11  " _
                    & "  WHERE ORDERNO          = @P01  " _
                    & "    AND DELFLG           <> @P02; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  '受注№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '削除フラグ
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)      '積込日（実績）
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)      '発日（実績）
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '積車着日（実績）
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)      '受入日（実績）
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)      '空車着日（実績）
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)  '更新年月日
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)  '更新ユーザーＩＤ
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)  '更新端末
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)  '集信日時
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)  '作成_貨車連結順序表№

            PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
            PARA02.Value = C_DELETE_FLG.DELETE

            If TxtActualLoadingDate.Text = "" Then
                PARA03.Value = DBNull.Value
            Else
                PARA03.Value = Date.Parse(TxtActualLoadingDate.Text)
            End If
            If TxtActualDepDate.Text = "" Then
                PARA04.Value = DBNull.Value
            Else
                PARA04.Value = Date.Parse(TxtActualDepDate.Text)
            End If
            If TxtActualArrDate.Text = "" Then
                PARA05.Value = DBNull.Value
            Else
                PARA05.Value = Date.Parse(TxtActualArrDate.Text)
            End If
            If TxtActualAccDate.Text = "" Then
                PARA06.Value = DBNull.Value
            Else
                PARA06.Value = Date.Parse(TxtActualAccDate.Text)
            End If
            If TxtActualEmparrDate.Text = "" Then
                PARA07.Value = DBNull.Value
            Else
                PARA07.Value = Date.Parse(TxtActualEmparrDate.Text)
            End If

            PARA08.Value = Date.Now
            PARA09.Value = Master.USERID
            PARA10.Value = Master.USERTERMID
            PARA11.Value = C_DEFAULT_YMD
            PARA12.Value = work.WF_SEL_LINKNO_ORDER.Text

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_ORDER_TAB3 UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_ORDER_TAB3 UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注明細TBL更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderDetail_TAB3(ByVal SQLcon As SqlConnection)

        Try
            '更新SQL文･･･受注明細TBLの各項目をを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0003_DETAIL " _
                    & "    SET CARSAMOUNT           = @P04, " _
                    & "        JOINT                = @P05, " _
                    & "        ACTUALLODDATE        = @P06, " _
                    & "        ACTUALDEPDATE        = @P07, " _
                    & "        ACTUALARRDATE        = @P08, " _
                    & "        ACTUALACCDATE        = @P09, " _
                    & "        ACTUALEMPARRDATE     = @P10, " _
                    & "        CHANGETRAINNO        = @P11, " _
                    & "        CHANGETRAINNAME      = @P22, " _
                    & "        SECONDCONSIGNEECODE  = @P12, " _
                    & "        SECONDCONSIGNEENAME  = @P13, " _
                    & "        SECONDARRSTATION     = @P14, " _
                    & "        SECONDARRSTATIONNAME = @P15, " _
                    & "        CANGERETSTATION      = @P16, " _
                    & "        CHANGEARRSTATIONNAME = @P17, " _
                    & "        UPDYMD               = @P18, " _
                    & "        UPDUSER              = @P19, " _
                    & "        UPDTERMID            = @P20, " _
                    & "        RECEIVEYMD           = @P21  " _
                    & "  WHERE ORDERNO              = @P01  " _
                    & "    AND DETAILNO             = @P02  " _
                    & "    AND DELFLG              <> @P03; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  '受注№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '受注明細No
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)  '削除フラグ
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Decimal)   '数量
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)  'ジョイント
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)      '積込日（実績）
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)      '発日（実績）
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Date)      '積車着日（実績）
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.Date)      '受入日（実績）
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.Date)      '空車着日（実績）
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar)  '本線列車番号(変更)
            Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar)  '本線列車名(変更)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)  '第2荷受人コード
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)  '第2荷受人名
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar)  '第2着駅コード
            Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar)  '第2着駅名
            Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar)  '空車着駅コード（変更後）
            Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.NVarChar)  '空車着駅名（変更後）

            Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.DateTime)  '更新年月日
            Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar)  '更新ユーザーＩＤ
            Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.NVarChar)  '更新端末
            Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.DateTime)  '集信日時

            For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                PARA01.Value = OIT0003tab3row("ORDERNO")
                PARA02.Value = OIT0003tab3row("DETAILNO")
                PARA03.Value = C_DELETE_FLG.DELETE
                Try
                    PARA04.Value = Decimal.Parse(OIT0003tab3row("CARSAMOUNT"))
                Catch ex As Exception
                    PARA04.Value = "0"
                End Try
                PARA05.Value = OIT0003tab3row("JOINT")

                If OIT0003tab3row("ACTUALLODDATE") = "" Then
                    PARA06.Value = DBNull.Value
                Else
                    PARA06.Value = OIT0003tab3row("ACTUALLODDATE")
                End If
                If OIT0003tab3row("ACTUALDEPDATE") = "" Then
                    PARA07.Value = DBNull.Value
                Else
                    PARA07.Value = OIT0003tab3row("ACTUALDEPDATE")
                End If
                If OIT0003tab3row("ACTUALARRDATE") = "" Then
                    PARA08.Value = DBNull.Value
                Else
                    PARA08.Value = OIT0003tab3row("ACTUALARRDATE")
                End If
                If OIT0003tab3row("ACTUALACCDATE") = "" Then
                    PARA09.Value = DBNull.Value
                Else
                    PARA09.Value = OIT0003tab3row("ACTUALACCDATE")
                End If
                If OIT0003tab3row("ACTUALEMPARRDATE") = "" Then
                    PARA10.Value = DBNull.Value
                Else
                    PARA10.Value = OIT0003tab3row("ACTUALEMPARRDATE")
                End If

                PARA11.Value = OIT0003tab3row("CHANGETRAINNO")
                PARA22.Value = OIT0003tab3row("CHANGETRAINNAME")
                PARA12.Value = OIT0003tab3row("SECONDCONSIGNEECODE")
                PARA13.Value = OIT0003tab3row("SECONDCONSIGNEENAME")
                PARA14.Value = OIT0003tab3row("SECONDARRSTATION")
                PARA15.Value = OIT0003tab3row("SECONDARRSTATIONNAME")
                PARA16.Value = OIT0003tab3row("CANGERETSTATION")
                PARA17.Value = OIT0003tab3row("CHANGEARRSTATIONNAME")

                PARA18.Value = Date.Now
                PARA19.Value = Master.USERID
                PARA20.Value = Master.USERTERMID
                PARA21.Value = C_DEFAULT_YMD

                SQLcmd.ExecuteNonQuery()
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_ORDERDETAIL_TAB3 UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_ORDERDETAIL_TAB3 UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

#End Region

    ''' <summary>
    ''' 受注(一覧)表示用
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_OrderListTBLSet(ByVal SQLcon As SqlConnection)

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
              " SELECT" _
            & "   0                                                  AS LINECNT" _
            & " , ''                                                 AS OPERATION" _
            & " , CAST(OIT0002.UPDTIMSTP AS bigint)                  AS TIMSTP" _
            & " , 1                                                  AS 'SELECT'" _
            & " , 0                                                  AS HIDDEN" _
            & " , ISNULL(FORMAT(OIT0002.ORDERYMD, 'yyyy/MM/dd'), '') AS ORDERYMD" _
            & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')              AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')              AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')             AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIS0015_1.VALUE1), '')                AS ORDERSTATUSNAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERINFO), '')               AS ORDERINFO" _
            & " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "   WHEN '10' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   WHEN '11' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   WHEN '12' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   ELSE ISNULL(RTRIM(OIS0015_2.VALUE1), '')" _
            & "   END                                                AS ORDERINFONAME" _
            & " , ISNULL(RTRIM(OIT0002.STACKINGFLG), '')   　        AS STACKINGFLG" _
            & " , ISNULL(RTRIM(OIT0002.USEPROPRIETYFLG), '')   　    AS USEPROPRIETYFLG" _
            & " , ISNULL(RTRIM(OIT0002.DELIVERYFLG), '')   　        AS DELIVERYFLG" _
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')   　            AS ORDERNO" _
            & " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "   ELSE ISNULL(RTRIM(OIT0002.TRAINNO), '')" _
            & "   END                                                AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')               AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERTYPE), '')               AS ORDERTYPE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')            AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')            AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0002.BASECODE), '')                AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0002.BASENAME), '')                AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')           AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')           AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')              AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')          AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')              AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')          AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.CANGERETSTATION), '')         AS CHANGERETSTATION" _
            & " , ISNULL(RTRIM(OIT0002.CHANGEARRSTATIONNAME), '')    AS CHANGEARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.RTANK), '')                   AS RTANK" _
            & " , ISNULL(RTRIM(OIT0002.HTANK), '')                   AS HTANK" _
            & " , ISNULL(RTRIM(OIT0002.TTANK), '')                   AS TTANK" _
            & " , ISNULL(RTRIM(OIT0002.MTTANK), '')                  AS MTTANK" _
            & " , ISNULL(RTRIM(OIT0002.KTANK), '')                   AS KTANK" _
            & " , ISNULL(RTRIM(OIT0002.K3TANK), '')                  AS K3TANK" _
            & " , ISNULL(RTRIM(OIT0002.K5TANK), '')                  AS K5TANK" _
            & " , ISNULL(RTRIM(OIT0002.K10TANK), '')                 AS K10TANK" _
            & " , ISNULL(RTRIM(OIT0002.LTANK), '')                   AS LTANK" _
            & " , ISNULL(RTRIM(OIT0002.ATANK), '')                   AS ATANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER1OTANK), '')             AS OTHER1OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER2OTANK), '')             AS OTHER2OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER3OTANK), '')             AS OTHER3OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER4OTANK), '')             AS OTHER4OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER5OTANK), '')             AS OTHER5OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER6OTANK), '')             AS OTHER6OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER7OTANK), '')             AS OTHER7OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER8OTANK), '')             AS OTHER8OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER9OTANK), '')             AS OTHER9OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER10OTANK), '')            AS OTHER10OTANK" _
            & " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TOTALTANK), '') + '</div>'" _
            & "   ELSE ISNULL(RTRIM(OIT0002.TOTALTANK), '')" _
            & "   END                                                AS TOTALTANK" _
            & " , ISNULL(RTRIM(OIT0002.RTANKCH), '')                 AS RTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.HTANKCH), '')                 AS HTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.TTANKCH), '')                 AS TTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.MTTANKCH), '')                AS MTTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.KTANKCH), '')                 AS KTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.K3TANKCH), '')                AS K3TANKCH" _
            & " , ISNULL(RTRIM(OIT0002.K5TANKCH), '')                AS K5TANKCH" _
            & " , ISNULL(RTRIM(OIT0002.K10TANKCH), '')               AS K10TANKCH" _
            & " , ISNULL(RTRIM(OIT0002.LTANKCH), '')                 AS LTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.ATANKCH), '')                 AS ATANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER1OTANKCH), '')           AS OTHER1OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER2OTANKCH), '')           AS OTHER2OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER3OTANKCH), '')           AS OTHER3OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER4OTANKCH), '')           AS OTHER4OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER5OTANKCH), '')           AS OTHER5OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER6OTANKCH), '')           AS OTHER6OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER7OTANKCH), '')           AS OTHER7OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER8OTANKCH), '')           AS OTHER8OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER9OTANKCH), '')           AS OTHER9OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER10OTANKCH), '')          AS OTHER10OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.TOTALTANKCH), '')             AS TOTALTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.TANKRINKNO), '')              AS TANKRINKNO" _
            & " , ISNULL(RTRIM(OIT0002.TANKRINKNOMADE), '')          AS TANKRINKNOMADE" _
            & " , ISNULL(FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd'), '')           AS LODDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALLODDATE, 'yyyy/MM/dd'), '')     AS ACTUALLODDATE" _
            & " , ISNULL(FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd'), '')           AS DEPDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALDEPDATE, 'yyyy/MM/dd'), '')     AS ACTUALDEPDATE" _
            & " , ISNULL(FORMAT(OIT0002.ARRDATE, 'yyyy/MM/dd'), '')           AS ARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALARRDATE, 'yyyy/MM/dd'), '')     AS ACTUALARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACCDATE, 'yyyy/MM/dd'), '')           AS ACCDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALACCDATE, 'yyyy/MM/dd'), '')     AS ACTUALACCDATE" _
            & " , ISNULL(FORMAT(OIT0002.EMPARRDATE, 'yyyy/MM/dd'), '')        AS EMPARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALEMPARRDATE, 'yyyy/MM/dd'), '')  AS ACTUALEMPARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.KEIJYOYMD, 'yyyy/MM/dd'), '')         AS KEIJYOYMD" _
            & " , ISNULL(RTRIM(OIT0002.SALSE), '')                   AS SALSE" _
            & " , ISNULL(RTRIM(OIT0002.SALSETAX), '')                AS SALSETAX" _
            & " , ISNULL(RTRIM(OIT0002.TOTALSALSE), '')              AS TOTALSALSE" _
            & " , ISNULL(RTRIM(OIT0002.PAYMENT), '')                 AS PAYMENT" _
            & " , ISNULL(RTRIM(OIT0002.PAYMENTTAX), '')              AS PAYMENTTAX" _
            & " , ISNULL(RTRIM(OIT0002.TOTALPAYMENT), '')            AS TOTALPAYMENT" _
            & " , ISNULL(RTRIM(OIT0002.DELFLG), '')                  AS DELFLG" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & "  INNER JOIN OIL.VIW0003_OFFICECHANGE VIW0003 ON " _
            & "        VIW0003.ORGCODE    = @P1 " _
            & "    AND VIW0003.OFFICECODE = OIT0002.OFFICECODE " _
            & "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_1 ON " _
            & "        OIS0015_1.CLASS   = 'ORDERSTATUS' " _
            & "    AND OIS0015_1.KEYCODE = OIT0002.ORDERSTATUS " _
            & "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_2 ON " _
            & "        OIS0015_2.CLASS   = 'ORDERINFO' " _
            & "    AND OIS0015_2.KEYCODE = OIT0002.ORDERINFO " _
            & " WHERE OIT0002.ORDERYMD   >= @P2" _
            & "   AND OIT0002.DELFLG     <> @P3"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '営業所
        If Not String.IsNullOrEmpty(work.WF_SEL_SALESOFFICECODE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.OFFICECODE = '{0}'", work.WF_SEL_SALESOFFICECODE.Text)
        End If
        '列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNUMBER.Text) Then
            SQLStr &= String.Format("    AND OIT0002.TRAINNO = '{0}'", work.WF_SEL_TRAINNUMBER.Text)
        End If
        '荷卸地(荷受人)
        If Not String.IsNullOrEmpty(work.WF_SEL_UNLOADINGCODE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.CONSIGNEECODE = '{0}'", work.WF_SEL_UNLOADINGCODE.Text)
        End If
        '状態(受注進行ステータス)
        If Not String.IsNullOrEmpty(work.WF_SEL_STATUSCODE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.ORDERSTATUS = '{0}'", work.WF_SEL_STATUSCODE.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIT0002.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 10) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)     '積込日(開始)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = Master.USER_ORG
                If work.WF_SEL_DATE.Text = "" Then
                    PARA2.Value = Date.Now
                Else
                    PARA2.Value = work.WF_SEL_DATE.Text
                End If
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Wkrow As DataRow In OIT0003WKtbl.Rows
                    i += 1
                    OIT0003Wkrow("LINECNT") = i        'LINECNT

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D ORDERLIST_SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D ORDERLIST_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

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
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P10 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS HTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P11 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS RTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P12 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS TTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P13 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS MTTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P14 OR OIT0003.OILCODE = @P15 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS KTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P16 OR OIT0003.OILCODE = @P17 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K3TANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P18 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K5TANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P19 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K10TANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P20 OR OIT0003.OILCODE = @P21 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS LTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P22 THEN ISNULL(OIT0003.CARSAMOUNT,0)ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS ATANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE <> '' THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
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

                    '[ヘッダー]
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

    ''' <summary>
    ''' (受注TBL)受注進行ステータス更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderStatus(ByVal I_Value As String)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLの受注進行ステータス、及び貨車連結順序表№を更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0002_ORDER " _
                    & "    SET ORDERSTATUS = @P03, " _
                    & "        TANKRINKNO  = @P04, " _
                    & "        UPDYMD      = @P11, " _
                    & "        UPDUSER     = @P12, " _
                    & "        UPDTERMID   = @P13, " _
                    & "        RECEIVEYMD  = @P14  " _
                    & "  WHERE ORDERNO     = @P01  " _
                    & "    AND DELFLG     <> @P02; "

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

            PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
            PARA02.Value = C_DELETE_FLG.DELETE
            PARA03.Value = I_Value
            PARA04.Value = work.WF_SEL_LINK_LINKNO.Text

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_ORDERSTATUS UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_ORDERSTATUS UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (受注TBL)託送指示フラグ更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateDeliveryFlg(ByVal I_Value As String)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLの託送指示フラグを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0002_ORDER " _
                    & "    SET DELIVERYFLG = @P03, " _
                    & "        UPDYMD      = @P11, " _
                    & "        UPDUSER     = @P12, " _
                    & "        UPDTERMID   = @P13, " _
                    & "        RECEIVEYMD  = @P14  " _
                    & "  WHERE ORDERNO     = @P01  " _
                    & "    AND DELFLG     <> @P02; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
            PARA02.Value = C_DELETE_FLG.DELETE
            PARA03.Value = I_Value

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_DELIVERYFLG UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_DELIVERYFLG UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '○ 選択内容を取得
        '### LeftBoxマルチ対応(20200217) START #####################################################
        If leftview.ActiveViewIdx = 2 Then
            '一覧表表示時
            Dim selectedLeftTableVal = leftview.GetLeftTableValue()
            WW_SelectValue = selectedLeftTableVal(LEFT_TABLE_SELECTED_KEY)
            WW_SelectText = selectedLeftTableVal("VALUE1")
            '### LeftBoxマルチ対応(20200217) END   #####################################################
        ElseIf leftview.WF_LeftListBox.SelectedIndex >= 0 Then
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

            '受注営業所
            Case "TxtOrderOffice"
                '別の受注営業所が設定された場合
                If TxtOrderOffice.Text <> WW_SelectText Then
                    TxtOrderOffice.Text = WW_SelectText
                    TxtOrderOfficeCode.Text = WW_SelectValue

                    'work.WF_SEL_SALESOFFICECODE.Text = WW_SelectValue
                    'work.WF_SEL_SALESOFFICE.Text = WW_SelectText
                    work.WF_SEL_ORDERSALESOFFICECODE.Text = WW_SelectValue
                    work.WF_SEL_ORDERSALESOFFICE.Text = WW_SelectText

                    '○ テキストボックスを初期化
                    '荷主
                    TxtShippersCode.Text = ""
                    LblShippersName.Text = ""
                    '荷受人
                    TxtConsigneeCode.Text = ""
                    LblConsigneeName.Text = ""
                    '本線列車
                    TxtTrainNo.Text = ""
                    '発駅
                    TxtDepstationCode.Text = ""
                    LblDepstationName.Text = ""
                    '着駅
                    TxtArrstationCode.Text = ""
                    LblArrstationName.Text = ""
                    '受注パターン
                    TxtOrderType.Text = ""
                    '(予定)日付
                    TxtLoadingDate.Text = ""
                    TxtDepDate.Text = ""
                    TxtArrDate.Text = ""
                    TxtAccDate.Text = ""
                    TxtEmparrDate.Text = ""

                    '○ 一覧の初期化画面表示データ取得
                    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()       'DataBase接続

                        '######################################################
                        '受注営業所を変更した時点で、新規登録と同様の扱いとする。
                        work.WF_SEL_CREATEFLG.Text = "1"
                        work.WF_SEL_CREATELINKFLG.Text = "1"
                        '######################################################
                        MAPDataGet(SQLcon, 0)
                    End Using

                    '○ 画面表示データ保存
                    Master.SaveTable(OIT0003tbl)

                End If
                TxtTrainNo.Focus()

            '本線列車
            Case "TxtTrainNo"
                '                TxtHeadOfficeTrain.Text = WW_SelectValue.Substring(0, 4)

                '〇 KeyCodeが重複し、名称(Value1)が異なる場合の取得術
                If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                    Dim selectedText = Me.Request.Form("commonLeftListSelectedText")
                    Dim selectedItem = leftview.WF_LeftListBox.Items.FindByText(selectedText)
                    WW_SelectValue = selectedItem.Value
                    WW_SelectText = selectedItem.Text
                End If

                TxtTrainNo.Text = WW_SelectValue
                TxtTrainName.Text = WW_SelectText
                'WW_FixvalueMasterSearch("", "TRAINNUMBER", WW_SelectValue, WW_GetValue)

                '〇 取得した列車名から各値を取得し設定する。
                WW_TRAINNUMBER_FIND(WW_SelectText)

            '発駅
            Case "TxtDepstationCode"
                TxtDepstationCode.Text = WW_SelectValue
                LblDepstationName.Text = WW_SelectText
                TxtDepstationCode.Focus()

            '着駅
            Case "TxtArrstationCode"
                TxtArrstationCode.Text = WW_SelectValue
                LblArrstationName.Text = WW_SelectText
                TxtArrstationCode.Focus()

                '〇営業所配下情報を取得・設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    '〇 画面(受注営業所).テキストボックスが未設定
                    If TxtOrderOffice.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
                    End If
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

            '(予定)積込日
            Case "TxtLoadingDate"
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

            '(予定)発日
            Case "TxtDepDate"
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

            '(予定)積車着日
            Case "TxtArrDate"
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

            '(予定)受入日
            Case "TxtAccDate"
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

            '(予定)空車着日
            Case "TxtEmparrDate"
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

            '(実績)積込日
            Case "TxtActualLoadingDate"
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

                '(実績)積込日に入力された日付を、(一覧)積込日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    OIT0003tab3row("ACTUALLODDATE") = TxtActualLoadingDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            '(実績)発日
            Case "TxtActualDepDate"
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

                '(実績)発日に入力された日付を、(一覧)発日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    OIT0003tab3row("ACTUALDEPDATE") = TxtActualDepDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            '(実績)積車着日
            Case "TxtActualArrDate"
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

                '(実績)積込着日に入力された日付を、(一覧)積込着日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    OIT0003tab3row("ACTUALARRDATE") = TxtActualArrDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            '(実績)受入日
            Case "TxtActualAccDate"
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

                '(実績)受入日に入力された日付を、(一覧)受入日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    OIT0003tab3row("ACTUALACCDATE") = TxtActualAccDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            '(実績)空車着日
            Case "TxtActualEmparrDate"
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

                '(実績)空車着日に入力された日付を、(一覧)空車着日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    OIT0003tab3row("ACTUALEMPARRDATE") = TxtActualEmparrDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            'タブ「タンク車割当」　　⇒　(一覧)荷主, (一覧)油種, (一覧)タンク車№
            'タブ「入換・積込指示」　⇒　(一覧)積込入線列車番号, (一覧)積込出線列車番号, (一覧)回線
            'タブ「タンク車明細」　　⇒　(一覧)(実績)積込日, (一覧)(実績)発日, (一覧)(実績)積車着日, (一覧)(実績)受入日, (一覧)(実績)空車着日
            '                            (一覧)第2着駅, (一覧)第2荷受人
            Case "SHIPPERSNAME", "OILNAME", "ORDERINGOILNAME", "TANKNO",
                 "LOADINGIRILINETRAINNO", "LOADINGOUTLETTRAINNO", "LINE",
                 "ACTUALLODDATE", "ACTUALDEPDATE", "ACTUALARRDATE", "ACTUALACCDATE", "ACTUALEMPARRDATE",
                 "SECONDARRSTATIONNAME", "SECONDCONSIGNEENAME"
                '○ LINECNT取得
                Dim WW_LINECNT As Integer = 0
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                '○ 設定項目取得
                Dim WW_SETTEXT As String = WW_SelectText
                Dim WW_SETVALUE As String = WW_SelectValue

                '各タブにより設定を制御
                Select Case WF_DetailMView.ActiveViewIndex
                '◆タンク車割当
                    Case 0
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
                            If WW_SETVALUE = "" Then
                                updHeader.Item("OILCODE") = ""
                                updHeader.Item(WF_FIELD.Value) = ""
                            Else
                                updHeader.Item("OILCODE") = WW_SETVALUE.Substring(0, 4)
                                updHeader.Item(WF_FIELD.Value) = WW_SETTEXT
                            End If

                            '〇 タンク車割当状況チェック
                            WW_TANKQUOTACHK(WF_FIELD.Value, updHeader)

                            '油種名(受発注用)を一覧に設定
                        ElseIf WF_FIELD.Value = "ORDERINGOILNAME" Then
                            If WW_SETVALUE = "" Then
                                updHeader.Item("OILCODE") = ""
                                updHeader.Item(WF_FIELD.Value) = ""
                                updHeader.Item("OILNAME") = ""
                                updHeader.Item("ORDERINGTYPE") = ""
                            Else
                                updHeader.Item("OILCODE") = WW_SETVALUE.Substring(0, 4)
                                updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                                '〇営業所配下情報を取得・設定
                                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                                    '〇 画面(受注営業所).テキストボックスが未設定
                                    If TxtOrderOffice.Text = "" Then
                                        WW_FixvalueMasterSearch(Master.USER_ORG, "PRODUCTPATTERN_SEG", WW_SETVALUE, WW_GetValue)
                                    Else
                                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "PRODUCTPATTERN_SEG", WW_SETVALUE, WW_GetValue)
                                    End If
                                Else
                                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN_SEG", WW_SETVALUE, WW_GetValue)
                                End If
                                updHeader.Item("OILNAME") = WW_GetValue(2)
                                updHeader.Item("ORDERINGTYPE") = WW_GetValue(1)
                            End If

                            '〇 タンク車割当状況チェック
                            WW_TANKQUOTACHK(WF_FIELD.Value, updHeader)

                            'タンク車№を一覧に設定
                        ElseIf WF_FIELD.Value = "TANKNO" Then
                            'Dim WW_TANKNUMBER As String = WW_SETTEXT.Substring(0, 8).Replace("-", "")
                            Dim WW_TANKNUMBER As String = WW_SETVALUE
                            Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                            updHeader.Item(WF_FIELD.Value) = WW_TANKNUMBER

                            'WW_FixvalueMasterSearch("", "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                            '〇 検索(営業所).テキストボックスが未設定
                            If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                                '〇 画面(受注営業所).テキストボックスが未設定
                                If TxtOrderOffice.Text = "" Then
                                    WW_FixvalueMasterSearch(Master.USER_ORG, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                                Else
                                    WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                                End If
                            Else
                                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                            End If

                            '型式
                            updHeader.Item("MODEL") = WW_GetValue(7)

                            '####################################################
                            '前回油種
                            'Dim WW_LASTOILNAME As String = ""
                            'updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                            'CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                            'updHeader.Item("LASTOILNAME") = WW_LASTOILNAME

                            updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                            updHeader.Item("LASTOILNAME") = WW_GetValue(4)
                            updHeader.Item("PREORDERINGTYPE") = WW_GetValue(5)
                            updHeader.Item("PREORDERINGOILNAME") = WW_GetValue(6)
                            '####################################################

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
                            End If

                            '〇 タンク車割当状況チェック
                            WW_TANKQUOTACHK(WF_FIELD.Value, updHeader)


                            '(一覧)第2着駅
                        ElseIf WF_FIELD.Value = "SECONDARRSTATIONNAME" Then
                            updHeader.Item("SECONDARRSTATION") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '(一覧)第2荷受人
                        ElseIf WF_FIELD.Value = "SECONDCONSIGNEENAME" Then
                            updHeader.Item("SECONDCONSIGNEECODE") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '    '積込入線列車番号を一覧に設定
                            'ElseIf WF_FIELD.Value = "LOADINGIRILINETRAINNO" Then
                            '    updHeader.Item("LOADINGIRILINETRAINNAME") = WW_SETTEXT
                            '    updHeader.Item(WF_FIELD.Value) = WW_SETVALUE

                            '    '積込出線列車番号を一覧に設定
                            'ElseIf WF_FIELD.Value = "LOADINGOUTLETTRAINNO" Then
                            '    updHeader.Item("LOADINGOUTLETTRAINNAME") = WW_SETTEXT
                            '    updHeader.Item(WF_FIELD.Value) = WW_SETVALUE

                        End If
                        'updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                        '○ 画面表示データ保存
                        If Not Master.SaveTable(OIT0003tbl) Then Exit Sub

                '◆入換・積込指示
                    Case 1
                        '○ 画面表示データ復元
                        If Not Master.RecoverTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text) Then Exit Sub

                        '○ 対象ヘッダー取得
                        Dim updHeader = OIT0003tbl_tab2.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                        If IsNothing(updHeader) Then Exit Sub

                        '〇 一覧項目へ設定
                        '積込入線列車番号を一覧に設定
                        If WF_FIELD.Value = "LOADINGIRILINETRAINNO" Then

                            '〇 KeyCodeが重複し、名称(Value1)が異なる場合の取得術
                            If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                                Dim selectedText = Me.Request.Form("commonLeftListSelectedText")
                                Dim selectedItem = leftview.WF_LeftListBox.Items.FindByText(selectedText)
                                WW_SelectValue = selectedItem.Value
                                WW_SelectText = selectedItem.Text
                            End If

                            '積込入線列車番号
                            updHeader.Item(WF_FIELD.Value) = WW_SelectValue
                            '積込入線列車名
                            updHeader.Item("LOADINGIRILINETRAINNAME") = WW_SelectText

                            '〇営業所配下情報を取得・設定
                            If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                                '〇 画面(受注営業所).テキストボックスが未設定
                                If TxtOrderOffice.Text = "" Then
                                    WW_FixvalueMasterSearch(Master.USER_ORG, "RINKAITRAIN_FIND_I", WW_SelectText, WW_GetValue)
                                Else
                                    WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "RINKAITRAIN_FIND_I", WW_SelectText, WW_GetValue)
                                End If
                            Else
                                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "RINKAITRAIN_FIND_I", WW_SelectText, WW_GetValue)
                            End If

                            '回線
                            updHeader.Item("LINE") = WW_GetValue(5)
                            '出線列車番号
                            updHeader.Item("LOADINGOUTLETTRAINNO") = WW_GetValue(6)
                            '出線列車名
                            updHeader.Item("LOADINGOUTLETTRAINNAME") = WW_GetValue(7)

                            '積込出線列車番号を一覧に設定
                        ElseIf WF_FIELD.Value = "LOADINGOUTLETTRAINNO" Then
                            '〇 KeyCodeが重複し、名称(Value1)が異なる場合の取得術
                            If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                                Dim selectedText = Me.Request.Form("commonLeftListSelectedText")
                                Dim selectedItem = leftview.WF_LeftListBox.Items.FindByText(selectedText)
                                WW_SelectValue = selectedItem.Value
                                WW_SelectText = selectedItem.Text
                            End If

                            '出線列車番号
                            updHeader.Item(WF_FIELD.Value) = WW_SelectValue
                            '出線列車名
                            updHeader.Item("LOADINGOUTLETTRAINNAME") = WW_SelectText

                            '〇営業所配下情報を取得・設定
                            If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                                '〇 画面(受注営業所).テキストボックスが未設定
                                If TxtOrderOffice.Text = "" Then
                                    WW_FixvalueMasterSearch(Master.USER_ORG, "RINKAITRAIN_FIND_O", WW_SelectText, WW_GetValue)
                                Else
                                    WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "RINKAITRAIN_FIND_O", WW_SelectText, WW_GetValue)
                                End If
                            Else
                                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "RINKAITRAIN_FIND_O", WW_SelectText, WW_GetValue)
                            End If

                            '回線
                            updHeader.Item("LINE") = WW_GetValue(5)
                            '入線列車番号
                            updHeader.Item("LOADINGIRILINETRAINNO") = WW_GetValue(6)
                            '入線列車名
                            updHeader.Item("LOADINGIRILINETRAINNAME") = WW_GetValue(7)

                            '回線を一覧に設定
                        ElseIf WF_FIELD.Value = "LINE" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE

                            '〇営業所配下情報を取得・設定
                            If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                                '〇 画面(受注営業所).テキストボックスが未設定
                                If TxtOrderOffice.Text = "" Then
                                    WW_FixvalueMasterSearch(Master.USER_ORG, "RINKAITRAIN_LINE", WW_SETVALUE, WW_GetValue)
                                Else
                                    WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "RINKAITRAIN_LINE", WW_SETVALUE, WW_GetValue)
                                End If
                            Else
                                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "RINKAITRAIN_LINE", WW_SETVALUE, WW_GetValue)
                            End If

                            '入線列車番号
                            updHeader.Item("LOADINGIRILINETRAINNO") = WW_GetValue(1)
                            '入線列車名
                            updHeader.Item("LOADINGIRILINETRAINNAME") = WW_GetValue(9)
                            '出線列車番号
                            updHeader.Item("LOADINGOUTLETTRAINNO") = WW_GetValue(6)
                            '出線列車名
                            updHeader.Item("LOADINGOUTLETTRAINNAME") = WW_GetValue(7)

                        End If

                        '○ 画面表示データ保存
                        If Not Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text) Then Exit Sub

                '◆タンク車明細
                    Case 2
                        '○ 画面表示データ復元
                        If Not Master.RecoverTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

                        '○ 対象ヘッダー取得
                        Dim updHeader = OIT0003tbl_tab3.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                        If IsNothing(updHeader) Then Exit Sub

                        '〇 一覧項目へ設定
                        '(一覧)(実績)積込日, (一覧)(実績)発日, (一覧)(実績)積車積込日, 
                        '(一覧)(実績)受入日, (一覧)(実績)空車着日を一覧に設定
                        If WF_FIELD.Value = "ACTUALLODDATE" _
                            OrElse WF_FIELD.Value = "ACTUALDEPDATE" _
                            OrElse WF_FIELD.Value = "ACTUALARRDATE" _
                            OrElse WF_FIELD.Value = "ACTUALACCDATE" _
                            OrElse WF_FIELD.Value = "ACTUALEMPARRDATE" Then

                            Dim WW_DATE As Date
                            Try
                                Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                                If WW_DATE < C_DEFAULT_YMD Then
                                    updHeader.Item(WF_FIELD.Value) = ""
                                Else
                                    updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text
                                End If
                            Catch ex As Exception
                            End Try

                            '(一覧)第2着駅
                        ElseIf WF_FIELD.Value = "SECONDARRSTATIONNAME" Then
                            updHeader.Item("SECONDARRSTATION") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '(一覧)第2荷受人
                        ElseIf WF_FIELD.Value = "SECONDCONSIGNEENAME" Then
                            updHeader.Item("SECONDCONSIGNEECODE") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                        End If

                        '○ 画面表示データ保存
                        If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

                '◆費用入力
                    Case 3
                        '○ 画面表示データ復元
                        If Not Master.RecoverTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text) Then Exit Sub

                        '○ 対象ヘッダー取得
                        Dim updHeader = OIT0003tbl_tab4.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                        If IsNothing(updHeader) Then Exit Sub

                        '### ここに書く #######

                        '○ 画面表示データ保存
                        If Not Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text) Then Exit Sub

                End Select

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
                & " , ISNULL(RTRIM(VIW0001.VALUE9), '   ')   AS VALUE9" _
                & " , ISNULL(RTRIM(VIW0001.VALUE10), '   ')   AS VALUE10" _
                & " , ISNULL(RTRIM(VIW0001.VALUE11), '   ')   AS VALUE11" _
                & " , ISNULL(RTRIM(VIW0001.VALUE12), '   ')   AS VALUE12" _
                & " , ISNULL(RTRIM(VIW0001.VALUE13), '   ')   AS VALUE13" _
                & " , ISNULL(RTRIM(VIW0001.VALUE14), '   ')   AS VALUE14" _
                & " , ISNULL(RTRIM(VIW0001.VALUE15), '   ')   AS VALUE15" _
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
                    For Each OIT0003WKrow As DataRow In OIT0003Fixvaltbl.Rows
                        O_VALUE(i) = OIT0003WKrow("KEYCODE")
                        i += 1
                    Next
                Else
                    For Each OIT0003WKrow As DataRow In OIT0003Fixvaltbl.Rows

                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0003WKrow("VALUE" & i.ToString())
                        Next
                        'O_VALUE(0) = OIT0003WKrow("VALUE1")
                        'O_VALUE(1) = OIT0003WKrow("VALUE2")
                        'O_VALUE(2) = OIT0003WKrow("VALUE3")
                        'O_VALUE(3) = OIT0003WKrow("VALUE4")
                        'O_VALUE(4) = OIT0003WKrow("VALUE5")
                        'O_VALUE(5) = OIT0003WKrow("VALUE6")
                        'O_VALUE(6) = OIT0003WKrow("VALUE7")
                        'O_VALUE(7) = OIT0003WKrow("VALUE8")
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
    ''' 画面表示設定処理(受注進行ステータス)
    ''' </summary>
    Protected Sub WW_ScreenOrderStatusSet(ByRef O_VALUE As String)

        '◆一度に設定をしない場合の対応
        '　受注進行ステータス＝"260:手配中(託送指示未手配)"
        '　託送指示フラグが"1"(手配)の場合
        If work.WF_SEL_ORDERSTATUS.Text = CONST_ORDERSTATUS_260 _
            AndAlso work.WF_SEL_DELIVERYFLG.Text = "1" Then
            '手配完了
            O_VALUE = CONST_ORDERSTATUS_270
            Exit Sub
        End If

        Select Case work.WF_SEL_ORDERSTATUS.Text
                '受注進行ステータス＝"200:手配中"
                '受注進行ステータス＝"210:手配中(入換指示手配済)"
                '受注進行ステータス＝"220:手配中(積込指示手配済)"
                '受注進行ステータス＝"230:手配中(託送指示手配済)"
                '受注進行ステータス＝"240:手配中(入換指示未手配)"
                '受注進行ステータス＝"250:手配中(積込指示未手配)"
                '受注進行ステータス＝"260:手配中(託送指示未手配)"
            Case BaseDllConst.CONST_ORDERSTATUS_200,
                 BaseDllConst.CONST_ORDERSTATUS_210,
                 BaseDllConst.CONST_ORDERSTATUS_220,
                 BaseDllConst.CONST_ORDERSTATUS_230,
                 BaseDllConst.CONST_ORDERSTATUS_240,
                 BaseDllConst.CONST_ORDERSTATUS_250,
                 BaseDllConst.CONST_ORDERSTATUS_260

                '入換指示入力＝"1:完了"
                'かつ、積込指示入力＝"1:完了"
                'かつ、託送指示入力＝"1:完了"の場合
                If WW_SwapInput = "1" AndAlso WW_LoadingInput = "1" AndAlso work.WF_SEL_DELIVERYFLG.Text = "1" Then
                    '手配完了
                    O_VALUE = CONST_ORDERSTATUS_270

                    '入換指示入力＝"1:完了"
                    'かつ、積込指示入力＝"0:未完了"
                    'かつ、託送指示入力＝"0:未完了"の場合
                ElseIf WW_SwapInput = "1" AndAlso WW_LoadingInput = "0" AndAlso work.WF_SEL_DELIVERYFLG.Text = "0" Then
                    '手配中(入換指示手配済)
                    O_VALUE = CONST_ORDERSTATUS_210

                    '入換指示入力＝"0:未完了"
                    'かつ、積込指示入力＝"1:完了"
                    'かつ、託送指示入力＝"0:未完了"の場合
                ElseIf WW_SwapInput = "0" AndAlso WW_LoadingInput = "1" AndAlso work.WF_SEL_DELIVERYFLG.Text = "0" Then
                    '手配中(積込指示手配済)
                    O_VALUE = CONST_ORDERSTATUS_220

                    '入換指示入力＝"0:未完了"
                    'かつ、積込指示入力＝"0:未完了"
                    'かつ、託送指示入力＝"1:完了"の場合
                ElseIf WW_SwapInput = "0" AndAlso WW_LoadingInput = "0" AndAlso work.WF_SEL_DELIVERYFLG.Text = "1" Then
                    '手配中(託送指示手配済)
                    O_VALUE = CONST_ORDERSTATUS_230

                    '入換指示入力＝"0:未完了"
                    'かつ、積込指示入力＝"1:完了"
                    'かつ、託送指示入力＝"1:完了"の場合
                ElseIf WW_SwapInput = "0" AndAlso WW_LoadingInput = "1" AndAlso work.WF_SEL_DELIVERYFLG.Text = "1" Then
                    '手配中(入換指示未手配)
                    O_VALUE = CONST_ORDERSTATUS_240

                    '入換指示入力＝"1:完了"
                    'かつ、積込指示入力＝"0:未完了"
                    'かつ、託送指示入力＝"1:完了"の場合
                ElseIf WW_SwapInput = "1" AndAlso WW_LoadingInput = "0" AndAlso work.WF_SEL_DELIVERYFLG.Text = "1" Then
                    '手配中(積込指示未手配)
                    O_VALUE = CONST_ORDERSTATUS_250

                    '入換指示入力＝"1:完了"
                    'かつ、積込指示入力＝"1:完了"
                    'かつ、託送指示入力＝"0:未完了"の場合
                ElseIf WW_SwapInput = "1" AndAlso WW_LoadingInput = "1" AndAlso work.WF_SEL_DELIVERYFLG.Text = "0" Then
                    '手配中(託送指示未手配)
                    O_VALUE = CONST_ORDERSTATUS_260

                End If
        End Select
    End Sub



    ''' <summary>
    ''' 画面表示設定処理
    ''' </summary>
    Protected Sub WW_ScreenEnabledSet()

        '〇 託送指示ボタン制御
        If work.WF_SEL_DELIVERYFLG.Text = "1" Then

        End If

        '○ 油種別タンク車数(車)、積込数量(kl)、計上月、売上金額、支払金額の表示・非表示制御
        '権限コードが更新の場合は表示設定
        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            pnlSummaryArea.Visible = True
        Else
            pnlSummaryArea.Visible = False
        End If

        '〇 タブの使用可否制御
        '受注受付の場合は、タブ「タンク車割当」のみ許可
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then
            WF_Dtab01.Enabled = True
            WF_Dtab02.Enabled = False
            WF_Dtab03.Enabled = False
            WF_Dtab04.Enabled = False
            pnlSummaryArea.Visible = False

            '200:手配, 210～270:手配中の場合は、タブ「タンク車割当」、タブ「入換指示・積込指示」を許可
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_210 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_220 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_230 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_240 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_250 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 Then
            WF_Dtab01.Enabled = True
            WF_Dtab02.Enabled = True
            WF_Dtab03.Enabled = False
            WF_Dtab04.Enabled = False
            pnlSummaryArea.Visible = False

            '上記以外は、タブ「タンク車明細」の許可
        Else
            WF_Dtab01.Enabled = False
            WF_Dtab02.Enabled = False
            WF_Dtab03.Enabled = True
            WF_Dtab04.Enabled = False
            pnlSummaryArea.Visible = True

        End If
        'WF_Dtab03.Enabled = True

        '〇 受注内容の制御
        '100:受注受付以外の場合は、受注内容(ヘッダーの内容)の変更を不可とする。
        If work.WF_SEL_ORDERSTATUS.Text <> BaseDllConst.CONST_ORDERSTATUS_100 Then
            '受注営業所
            TxtOrderOffice.Enabled = False
            '本線列車
            TxtTrainNo.Enabled = False
            '荷主
            TxtShippersCode.Enabled = False
            '荷受人
            TxtConsigneeCode.Enabled = False
            '発駅
            TxtDepstationCode.Enabled = False
            '着駅
            TxtArrstationCode.Enabled = False
            '(予定)積込日
            TxtLoadingDate.Enabled = False
            '(予定)発日
            TxtDepDate.Enabled = False
            '(予定)積車着日
            TxtArrDate.Enabled = False
            '(予定)受入日
            TxtAccDate.Enabled = False
            '(予定)空車着日
            TxtEmparrDate.Enabled = False
        Else
            '受注営業所
            TxtOrderOffice.Enabled = True
            '本線列車
            TxtTrainNo.Enabled = True
            '荷主
            TxtShippersCode.Enabled = True
            '荷受人
            TxtConsigneeCode.Enabled = True
            '発駅
            TxtDepstationCode.Enabled = True
            '着駅
            TxtArrstationCode.Enabled = True
            '(予定)積込日
            TxtLoadingDate.Enabled = True
            '(予定)発日
            TxtDepDate.Enabled = True
            '(予定)積車着日
            TxtArrDate.Enabled = True
            '(予定)受入日
            TxtAccDate.Enabled = True
            '(予定)空車着日
            TxtEmparrDate.Enabled = True
        End If

        '〇 (実績)の日付の入力可否制御
        '受注情報が以下の場合は、(実績)の日付の入力を制限
        '100:受注受付, 200:手配, 210:手配中（入換指示手配済）, 220:手配中（積込指示手配済）
        '230:手配中（託送指示手配済）, 240:手配中（入換指示未手配）, 250:手配中（積込指示未手配）
        '260:手配中（託送指示未手配）
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_210 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_220 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_230 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_240 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_250 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 Then

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

            '受注情報が「270:手配完了」の場合は、(実績)すべての日付の入力を制限
            '270:手配完了
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 Then
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

            '受注情報が「300:受注確定」の場合は、(実績)積込日の入力を制限
            '300:受注確定
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_300 Then
            '(実績)積込日
            TxtActualLoadingDate.Enabled = False
            '(実績)発日
            TxtActualDepDate.Enabled = True
            '(実績)積車着日
            TxtActualArrDate.Enabled = True
            '(実績)受入日
            TxtActualAccDate.Enabled = True
            '(実績)空車着日
            TxtActualEmparrDate.Enabled = True

            '受注情報が「400:受入確認中」の場合は、(実績)積車着日の入力を制限
            '400:受入確認中
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 Then
            '(実績)積込日
            TxtActualLoadingDate.Enabled = False
            '(実績)発日
            TxtActualDepDate.Enabled = False
            '(実績)積車着日
            TxtActualArrDate.Enabled = False
            '(実績)受入日
            TxtActualAccDate.Enabled = True
            '(実績)空車着日
            TxtActualEmparrDate.Enabled = True

            '受注情報が「500:検収中」の場合は、(実績)空車着日の入力を制限
            '500:検収中
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 Then
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

            '550:検収済
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 Then
            '600:費用確定
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 Then
            '700:経理未計上
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 Then
            '800:経理計上
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 Then
            '900:受注キャンセル
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_900 Then

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
    Protected Sub WW_TANKQUOTACHK(ByVal I_Value As String, ByVal I_updHeader As DataRow)

        '〇 (一覧)項目変更箇所特定
        Select Case I_Value
            Case "OILNAME", "ORDERINGOILNAME"
                '〇 現状のタンク車割当状況により判断
                Select Case I_updHeader.Item("TANKQUOTA")
                    '■タンク車割当状況＝"割当"の場合
                    Case CONST_TANKNO_STATUS_WARI
                        '油種が削除("")の場合
                        If I_updHeader.Item("OILCODE") = "" Then
                            'タンク車割当状況＝"残車"に設定
                            I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN
                        End If

                        '■タンク車割当状況＝"残車"の場合
                    Case CONST_TANKNO_STATUS_ZAN
                        '油種が設定された場合
                        If I_updHeader.Item("OILCODE") <> "" Then
                            'タンク車割当状況＝"割当"に設定
                            I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI
                        End If

                        '■タンク車割当状況＝"未割当"の場合
                    Case CONST_TANKNO_STATUS_MIWARI


                        '■タンク車割当状況＝"不可"の場合
                    Case CONST_TANKNO_STATUS_FUKA

                End Select

            Case "TANKNO"
                '〇 現状のタンク車割当状況により判断
                Select Case I_updHeader.Item("TANKQUOTA")
                    '■タンク車割当状況＝"割当"の場合
                    Case CONST_TANKNO_STATUS_WARI
                        'タンク車番号が設定された場合
                        If I_updHeader.Item("TANKNO") <> "" Then
                            '〇 指定されたタンク車の交検日、または全検日が近い場合
                            If I_updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED _
                                OrElse I_updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Then
                                'タンク車割当状況＝"不可"に設定
                                I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_FUKA
                            Else
                                '〇 油種が設定されているかチェック
                                If I_updHeader.Item("OILCODE") = "" Then
                                    'タンク車割当状況＝"残車"に設定
                                    I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN
                                Else
                                    'タンク車割当状況＝"割当"に設定
                                    I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI

                                End If

                            End If
                        Else
                            'タンク車割当状況＝"未割当"に設定
                            I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_MIWARI
                        End If

                        '■タンク車割当状況＝"残車"の場合
                    Case CONST_TANKNO_STATUS_ZAN
                        'タンク車番号が設定された場合
                        If I_updHeader.Item("TANKNO") <> "" Then
                            '〇 指定されたタンク車の交検日、または全検日が近い場合
                            If I_updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED _
                                OrElse I_updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Then
                                'タンク車割当状況＝"不可"に設定
                                I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_FUKA
                            Else
                                'タンク車割当状況＝"残車"に設定
                                I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN
                            End If
                        Else
                            'タンク車割当状況＝"未割当"に設定
                            I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_MIWARI

                        End If

                        '■タンク車割当状況＝"未割当"の場合
                    Case CONST_TANKNO_STATUS_MIWARI
                        'タンク車番号が設定された場合
                        If I_updHeader.Item("TANKNO") <> "" Then
                            '〇 指定されたタンク車の交検日、または全検日が近い場合
                            If I_updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED _
                                OrElse I_updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Then
                                'タンク車割当状況＝"不可"に設定
                                I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_FUKA
                            Else
                                '〇 油種が設定されているかチェック
                                If I_updHeader.Item("OILCODE") = "" Then
                                    'タンク車割当状況＝"残車"に設定
                                    I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN
                                Else
                                    'タンク車割当状況＝"割当"に設定
                                    I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI
                                End If

                            End If
                        Else
                            'タンク車割当状況＝"未割当"に設定
                            I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_MIWARI

                        End If

                        '■タンク車割当状況＝"不可"の場合
                    Case CONST_TANKNO_STATUS_FUKA
                        'タンク車番号が設定された場合
                        If I_updHeader.Item("TANKNO") <> "" Then
                            '〇 指定されたタンク車の交検日、または全検日が近い場合
                            If I_updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED _
                                OrElse I_updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Then
                                'タンク車割当状況＝"不可"に設定
                                I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_FUKA
                            Else
                                '〇 油種が設定されているかチェック
                                If I_updHeader.Item("OILCODE") = "" Then
                                    'タンク車割当状況＝"残車"に設定
                                    I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_ZAN
                                Else
                                    'タンク車割当状況＝"割当"に設定
                                    I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_WARI
                                End If
                            End If
                        Else
                            'タンク車割当状況＝"未割当"に設定
                            I_updHeader.Item("TANKQUOTA") = CONST_TANKNO_STATUS_MIWARI

                        End If

                End Select

        End Select

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '受注営業所
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", TxtOrderOfficeCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        'Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", work.WF_SEL_SALESOFFICECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SALESOFFICE", TxtOrderOfficeCode.Text, TxtOrderOffice.Text, WW_RTN_SW)
            'CODENAME_get("SALESOFFICE", work.WF_SEL_SALESOFFICECODE.Text, TxtOrderOffice.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "受注営業所 : " & TxtOrderOfficeCode.Text)
                TxtOrderOffice.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtOrderOffice.Focus()
            WW_CheckMES1 = "受注営業所入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '本線列車
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", TxtTrainNo.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "本線列車", needsPopUp:=True)
            TxtTrainNo.Focus()
            WW_CheckMES1 = "本線列車入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '荷主
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHIPPERSCODE", TxtShippersCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SHIPPERS", TxtShippersCode.Text, LblShippersName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "荷主 : " & TxtShippersCode.Text)
                TxtShippersCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtShippersCode.Focus()
            WW_CheckMES1 = "荷主入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '荷受人
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CONSIGNEECODE", TxtConsigneeCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CONSIGNEE", TxtConsigneeCode.Text, LblConsigneeName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "荷受人 : " & TxtConsigneeCode.Text)
                TxtConsigneeCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtConsigneeCode.Focus()
            WW_CheckMES1 = "荷受人入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '発駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", TxtDepstationCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("DEPSTATION", TxtDepstationCode.Text, LblDepstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "発駅 : " & TxtDepstationCode.Text)
                TxtDepstationCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発駅", needsPopUp:=True)
            TxtDepstationCode.Focus()
            WW_CheckMES1 = "発駅入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '着駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRSTATION", TxtArrstationCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("ARRSTATION", TxtArrstationCode.Text, LblArrstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "着駅 : " & TxtArrstationCode.Text)
                TxtArrstationCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "着駅", needsPopUp:=True)
            TxtArrstationCode.Focus()
            WW_CheckMES1 = "着駅入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積込日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LODDATE", TxtLoadingDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtLoadingDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else

            '年月日チェック
            WW_CheckDate(TxtLoadingDate.Text, "(予定)積込日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)積込日", needsPopUp:=True)
            TxtLoadingDate.Focus()
            WW_CheckMES1 = "積込日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)発日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPDATE", TxtDepDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtDepDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            '年月日チェック
            WW_CheckDate(TxtDepDate.Text, "(予定)発日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)発日", needsPopUp:=True)
            TxtDepDate.Focus()
            WW_CheckMES1 = "発日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積車着日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRDATE", TxtArrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtArrDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            '年月日チェック
            WW_CheckDate(TxtArrDate.Text, "(予定)積車着日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)積車着日", needsPopUp:=True)
            TxtArrDate.Focus()
            WW_CheckMES1 = "積車着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)受入日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACCDATE", TxtAccDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtAccDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            '年月日チェック
            WW_CheckDate(TxtAccDate.Text, "(予定)受入日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)受入日", needsPopUp:=True)
            TxtAccDate.Focus()
            WW_CheckMES1 = "受入日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)空車着日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EMPARRDATE", TxtEmparrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtEmparrDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            '年月日チェック
            WW_CheckDate(TxtEmparrDate.Text, "(予定)空車着日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)空車着日", needsPopUp:=True)
            TxtEmparrDate.Focus()
            WW_CheckMES1 = "空車着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(一覧)チェック(準備)
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            OIT0003row("ORDERINFO") = ""
            OIT0003row("ORDERINFONAME") = ""
        Next
        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        'タンク車Noでソートし、重複がないかチェックする。
        Dim OIT0003tbl_DUMMY As DataTable = OIT0003tbl.Copy
        Dim OIT0003tbl_dv As DataView = New DataView(OIT0003tbl_DUMMY)
        Dim chkTankNo As String = ""
        Dim chkLineOrder As String = ""
        OIT0003tbl_dv.Sort = "TANKNO"
        For Each drv As DataRowView In OIT0003tbl_dv
            If drv("HIDDEN") <> "1" AndAlso drv("TANKNO") <> "" AndAlso chkTankNo = drv("TANKNO") Then
                Master.Output(C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckMES1 = "タンク車№重複エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                O_RTN = "ERR"

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0003tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = drv("LINECNT"))
                updHeader.Item("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                CODENAME_get("ORDERINFO", updHeader.Item("ORDERINFO"), updHeader.Item("ORDERINFONAME"), WW_DUMMY)

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl)
                Exit Sub
            End If

            '行削除したデータの場合は退避しない。
            If drv("HIDDEN") <> "1" Then
                chkTankNo = drv("TANKNO")
            End If
        Next

        '貨物駅入線順でソートし、重複がないかチェックする。
        OIT0003tbl_dv.Sort = "LINEORDER"
        For Each drv As DataRowView In OIT0003tbl_dv
            If drv("HIDDEN") <> "1" AndAlso drv("LINEORDER") <> "" AndAlso chkLineOrder = drv("LINEORDER") Then
                Master.Output(C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckMES1 = "貨物駅入線順重複エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                O_RTN = "ERR"
                Exit Sub
            End If

            '行削除したデータの場合は退避しない。
            If drv("HIDDEN") <> "1" Then
                chkLineOrder = drv("LINEORDER")
            End If
        Next

        '(一覧)チェック
        For Each OIT0003row As DataRow In OIT0003tbl.Rows

            '(一覧)受注油種(空白チェック)
            If OIT0003row("ORDERINGOILNAME") = "" And OIT0003row("DELFLG") = "0" Then
                Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)受注油種", needsPopUp:=True)

                WW_CheckMES1 = "受注油種未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                Exit Sub
            End If

            '(一覧)貨物駅入線順(空白チェック)
            If OIT0003row("LINEORDER") = "" And OIT0003row("DELFLG") = "0" Then
                Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)貨物駅入線順", needsPopUp:=True)

                WW_CheckMES1 = "貨物駅入線順未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                Exit Sub
            End If

            '(一覧)タンク車割当状況(未割当チェック)
            If OIT0003row("TANKQUOTA") = CONST_TANKNO_STATUS_MIWARI And OIT0003row("DELFLG") = "0" Then
                Master.Output(C_MESSAGE_NO.OIL_TANKNO_MIWARIATE_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                WW_CheckMES1 = "タンク車No未割当エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_TANKNO_MIWARIATE_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                Exit Sub
            End If
        Next


        ''(一覧)タンク車No
        'For Each OIT0003row As DataRow In OIT0003tbl.Rows
        '    If OIT0003row("TANKNO").Equals("") And OIT0003row("DELFLG") = "0" Then
        '        Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR)

        '        WW_CheckMES1 = "タンク車No入力エラー。"
        '        WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '        WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
        '        O_RTN = "ERR"
        '        Exit Sub
        '    End If
        'Next

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' チェック処理(タブ「入換・積込指示」)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTab2(ByRef O_RTN As String, ByRef O_Msg As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '(一覧)チェック
        For Each OIT0003row As DataRow In OIT0003tbl_tab2.Rows

            '(一覧)積込入線列車番号(空白チェック)
            '※臨海鉄道対象の場合
            If WW_RINKAIFLG = True _
                And OIT0003row("LOADINGIRILINETRAINNO") = "" And OIT0003row("DELFLG") = "0" Then
                'Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)積込入線列車番号", needsPopUp:=True)

                WW_CheckMES1 = "積込入線列車番号未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                O_Msg = "(一覧)積込入線列車番号"
                Exit Sub
            End If

            '(一覧)積込入線順(空白チェック)
            '※臨海鉄道対象の場合
            If WW_RINKAIFLG = True _
                And OIT0003row("LOADINGIRILINEORDER") = "" And OIT0003row("DELFLG") = "0" Then
                'Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)積込入線順", needsPopUp:=True)

                WW_CheckMES1 = "積込入線順未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                O_Msg = "(一覧)積込入線順"
                Exit Sub
            End If

            ''(一覧)回線(空白チェック)
            'If OIT0003row("LINE") = "" And OIT0003row("DELFLG") = "0" Then
            '    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)回線", needsPopUp:=True)

            '    WW_CheckMES1 = "回線未設定エラー。"
            '    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            '    WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
            '    O_RTN = "ERR"
            '    Exit Sub
            'End If

            ''(一覧)充填ポイント(空白チェック)
            'If OIT0003row("FILLINGPOINT") = "" And OIT0003row("DELFLG") = "0" Then
            '    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)充填ポイント", needsPopUp:=True)

            '    WW_CheckMES1 = "充填ポイント未設定エラー。"
            '    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            '    WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
            '    O_RTN = "ERR"
            '    Exit Sub
            'End If

            '(一覧)出線列車番号(空白チェック)
            '※臨海鉄道対象の場合
            If WW_RINKAIFLG = True _
                And OIT0003row("LOADINGOUTLETTRAINNO") = "" And OIT0003row("DELFLG") = "0" Then
                'Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)出線列車番号", needsPopUp:=True)

                WW_CheckMES1 = "出線列車番号未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                O_Msg = "(一覧)出線列車番号"
                Exit Sub
            End If

            '(一覧)出線順(空白チェック)
            '※臨海鉄道対象の場合
            If WW_RINKAIFLG = True _
                And OIT0003row("LOADINGOUTLETORDER") = "" And OIT0003row("DELFLG") = "0" Then
                'Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)出線順", needsPopUp:=True)

                WW_CheckMES1 = "出線順未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                O_Msg = "(一覧)出線順"
                Exit Sub
            End If
        Next

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

        '入換指示⇒完了
        WW_SwapInput = "1"

    End Sub

    ''' <summary>
    ''' チェック処理(タブ「タンク車明細」)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTab3(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '〇 (実績)の日付は入力されていた場合チェックする。
        '(実績)積込日
        If TxtActualLoadingDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALLODDATE", TxtActualLoadingDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(TxtActualLoadingDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(TxtActualLoadingDate.Text, "(実績)積込日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)積込日", needsPopUp:=True)
                TxtActualLoadingDate.Focus()
                WW_CheckMES1 = "積込日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)発日
        If TxtActualDepDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALDEPDATE", TxtActualDepDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(TxtActualDepDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(TxtActualDepDate.Text, "(実績)発日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)発日", needsPopUp:=True)
                TxtActualDepDate.Focus()
                WW_CheckMES1 = "発日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)積車着日
        If TxtActualArrDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALARRDATE", TxtActualArrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(TxtActualArrDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(TxtActualArrDate.Text, "(実績)積車着日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)積車着日", needsPopUp:=True)
                TxtActualArrDate.Focus()
                WW_CheckMES1 = "積車着日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)受入日
        If TxtActualAccDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALACCDATE", TxtActualAccDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(TxtActualAccDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(TxtActualAccDate.Text, "(実績)受入日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)受入日", needsPopUp:=True)
                TxtActualAccDate.Focus()
                WW_CheckMES1 = "受入日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)空車着日
        If TxtActualEmparrDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALEMPARRDATE", TxtActualEmparrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(TxtActualEmparrDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(TxtActualEmparrDate.Text, "(実績)空車着日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)空車着日", needsPopUp:=True)
                TxtActualEmparrDate.Focus()
                WW_CheckMES1 = "空車着日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

    End Sub

    ''' <summary>
    ''' 年月日チェック
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <param name="I_DATENAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String)

        Try
            '年取得
            Dim chkLeapYear As String = I_DATE.Substring(0, 4)
            '月日を取得
            Dim getMMDD As String = I_DATE.Remove(0, I_DATE.IndexOf("/") + 1)
            '月取得
            Dim getMonth As String = getMMDD.Remove(getMMDD.IndexOf("/"))
            '日取得
            Dim getDay As String = getMMDD.Remove(0, getMMDD.IndexOf("/") + 1)

            '閏年の場合はその旨のメッセージを出力
            If Not DateTime.IsLeapYear(chkLeapYear) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                '月と日の範囲チェック
            ElseIf getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            End If
        Catch ex As Exception
            Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

    End Sub

    ''' <summary>
    ''' 年月日妥当性チェック((予定)日付)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckPlanValidityDate(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        WW_ORDERINFOFLG_10 = False
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim iresult As Integer

        '○ 過去日付チェック
        '例) iresult = dt1.Date.CompareTo(dt2.Date)
        '    iresultの意味
        '     0 : dt1とdt2は同じ日
        '    -1 : dt1はdt2より前の日
        '     1 : dt1はdt2より後の日
        '(予定)積込日 と　現在日付を比較
        iresult = Date.Parse(TxtLoadingDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)積込日", needsPopUp:=True)
            TxtLoadingDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)発日 と　現在日付を比較
        iresult = Date.Parse(TxtDepDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)発日", needsPopUp:=True)
            TxtDepDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積車着日 と　現在日付を比較
        iresult = Date.Parse(TxtArrDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)積車着日", needsPopUp:=True)
            TxtArrDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)受入日 と　現在日付を比較
        iresult = Date.Parse(TxtAccDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)受入日", needsPopUp:=True)
            TxtAccDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)空車着日 と　現在日付を比較
        iresult = Date.Parse(TxtEmparrDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)空車着日", needsPopUp:=True)
            TxtEmparrDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ 日付妥当性チェック
        '例) iresult = dt1.Date.CompareTo(dt2.Date)
        '    iresultの意味
        '     0 : dt1とdt2は同じ日
        '    -1 : dt1はdt2より前の日
        '     1 : dt1はdt2より後の日
        '(予定)積込日 と　(予定)発日を比較
        iresult = Date.Parse(TxtLoadingDate.Text).CompareTo(Date.Parse(TxtDepDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)積込日 > (予定)発日", needsPopUp:=True)
            TxtDepDate.Focus()
            WW_CheckMES1 = "(予定日)入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        ElseIf iresult = -1 Then    '(予定)積込日 < (予定)発日の場合
            WW_ORDERINFOFLG_10 = True
        End If

        '(予定)発日 と　(予定)積車着日を比較
        iresult = Date.Parse(TxtDepDate.Text).CompareTo(Date.Parse(TxtArrDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)発日 > (予定)積車着日", needsPopUp:=True)
            TxtArrDate.Focus()
            WW_CheckMES1 = "(予定日)入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積車着日 と　(予定)受入日を比較
        iresult = Date.Parse(TxtArrDate.Text).CompareTo(Date.Parse(TxtAccDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)積車着日 > (予定)受入日", needsPopUp:=True)
            TxtAccDate.Focus()
            WW_CheckMES1 = "(予定日)入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)受入日 と　(予定)空車着日を比較
        iresult = Date.Parse(TxtAccDate.Text).CompareTo(Date.Parse(TxtEmparrDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)受入日 > (予定)空車着日", needsPopUp:=True)
            TxtEmparrDate.Focus()
            WW_CheckMES1 = "(予定日)入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

    End Sub

    ''' <summary>
    ''' 年月日妥当性チェック((実績)日付)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckActualValidityDate(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim iresult As Integer

        '○ 過去日付チェック
        '例) iresult = dt1.Date.CompareTo(dt2.Date)
        '    iresultの意味
        '     0 : dt1とdt2は同じ日
        '    -1 : dt1はdt2より前の日
        '     1 : dt1はdt2より後の日
        '(実績)積込日 と　現在日付を比較
        If TxtActualLoadingDate.Text <> "" Then
            iresult = Date.Parse(TxtActualLoadingDate.Text).CompareTo(DateTime.Today)
            If iresult = -1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)積込日", needsPopUp:=True)
                TxtActualLoadingDate.Focus()
                WW_CheckMES1 = "(実績日)過去日付エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)発日 と　現在日付を比較
        If TxtActualDepDate.Text <> "" Then
            iresult = Date.Parse(TxtActualDepDate.Text).CompareTo(DateTime.Today)
            If iresult = -1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)発日", needsPopUp:=True)
                TxtActualDepDate.Focus()
                WW_CheckMES1 = "(実績日)過去日付エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)積車着日 と　現在日付を比較
        If TxtActualArrDate.Text <> "" Then
            iresult = Date.Parse(TxtActualArrDate.Text).CompareTo(DateTime.Today)
            If iresult = -1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)積車着日", needsPopUp:=True)
                TxtActualArrDate.Focus()
                WW_CheckMES1 = "(実績日)過去日付エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)受入日 と　現在日付を比較
        If TxtActualAccDate.Text <> "" Then
            iresult = Date.Parse(TxtActualAccDate.Text).CompareTo(DateTime.Today)
            If iresult = -1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)受入日", needsPopUp:=True)
                TxtActualAccDate.Focus()
                WW_CheckMES1 = "(実績日)過去日付エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)空車着日 と　現在日付を比較
        If TxtActualEmparrDate.Text <> "" Then
            iresult = Date.Parse(TxtActualEmparrDate.Text).CompareTo(DateTime.Today)
            If iresult = -1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)空車着日", needsPopUp:=True)
                TxtActualEmparrDate.Focus()
                WW_CheckMES1 = "(実績日)過去日付エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '○ 日付妥当性チェック
        '例) iresult = dt1.Date.CompareTo(dt2.Date)
        '    iresultの意味
        '     0 : dt1とdt2は同じ日
        '    -1 : dt1はdt2より前の日
        '     1 : dt1はdt2より後の日
        '(実績)積込日 と　(実績)発日を比較
        If TxtActualLoadingDate.Text <> "" AndAlso TxtActualDepDate.Text <> "" Then
            iresult = Date.Parse(TxtActualLoadingDate.Text).CompareTo(Date.Parse(TxtActualDepDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)積込日 > (実績)発日", needsPopUp:=True)
                TxtActualDepDate.Focus()
                WW_CheckMES1 = "(実績日)入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)発日 と　(実績)積車着日を比較
        If TxtActualDepDate.Text <> "" AndAlso TxtActualArrDate.Text <> "" Then
            iresult = Date.Parse(TxtActualDepDate.Text).CompareTo(Date.Parse(TxtActualArrDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)発日 > (実績)積車着日", needsPopUp:=True)
                TxtActualArrDate.Focus()
                WW_CheckMES1 = "(実績日)入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)積車着日 と　(実績)受入日を比較
        If TxtActualArrDate.Text <> "" AndAlso TxtActualAccDate.Text <> "" Then
            iresult = Date.Parse(TxtActualArrDate.Text).CompareTo(Date.Parse(TxtActualAccDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)積車着日 > (実績)受入日", needsPopUp:=True)
                TxtActualAccDate.Focus()
                WW_CheckMES1 = "(実績日)入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)受入日 と　(実績)空車着日を比較
        If TxtActualAccDate.Text <> "" AndAlso TxtActualEmparrDate.Text <> "" Then
            iresult = Date.Parse(TxtActualAccDate.Text).CompareTo(Date.Parse(TxtActualEmparrDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)受入日 > (実績)空車着日", needsPopUp:=True)
                TxtActualEmparrDate.Focus()
                WW_CheckMES1 = "(実績日)入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(一覧)日付有効性チェック
        'テキストボックスに入力している(実績)日付より過去の場合はアラートとする。
        For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
            '例) iresult = dt1.Date.CompareTo(dt2.Date)
            '    iresultの意味
            '     0 : dt1とdt2は同じ日
            '    -1 : dt1はdt2より前の日
            '     1 : dt1はdt2より後の日
            '〇 (実績)積込日 と　(一覧)積込日を比較
            If TxtActualLoadingDate.Text <> "" AndAlso OIT0003tab3row("ACTUALLODDATE") <> "" Then
                iresult = Date.Parse(TxtActualLoadingDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALLODDATE")))
                If iresult = 1 Then
                    OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_91
                    CODENAME_get("ORDERINFO", OIT0003tab3row("ORDERINFO"), OIT0003tab3row("ORDERINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "(実績)積込日で入力した日付より過去日のためエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Continue For
                Else
                    OIT0003tab3row("ORDERINFO") = ""
                    OIT0003tab3row("ORDERINFONAME") = ""
                End If
            End If

            '〇 (実績)発日 と　(一覧)発日を比較
            If TxtActualDepDate.Text <> "" AndAlso OIT0003tab3row("ACTUALDEPDATE") <> "" Then
                iresult = Date.Parse(TxtActualDepDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALDEPDATE")))
                If iresult = 1 Then
                    OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_92
                    CODENAME_get("ORDERINFO", OIT0003tab3row("ORDERINFO"), OIT0003tab3row("ORDERINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "(実績)発日で入力した日付より過去日のためエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Continue For
                Else
                    OIT0003tab3row("ORDERINFO") = ""
                    OIT0003tab3row("ORDERINFONAME") = ""
                End If
            End If

            '〇 (実績)積車着日 と　(一覧)積車着日を比較
            If TxtActualArrDate.Text <> "" AndAlso OIT0003tab3row("ACTUALARRDATE") <> "" Then
                iresult = Date.Parse(TxtActualArrDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALARRDATE")))
                If iresult = 1 Then
                    OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_93
                    CODENAME_get("ORDERINFO", OIT0003tab3row("ORDERINFO"), OIT0003tab3row("ORDERINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "(実績)積車着日で入力した日付より過去日のためエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Continue For
                Else
                    OIT0003tab3row("ORDERINFO") = ""
                    OIT0003tab3row("ORDERINFONAME") = ""
                End If
            End If

            '〇 (実績)受入日 と　(一覧)受入日を比較
            If TxtActualAccDate.Text <> "" AndAlso OIT0003tab3row("ACTUALACCDATE") <> "" Then
                iresult = Date.Parse(TxtActualAccDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALACCDATE")))
                If iresult = 1 Then
                    OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_94
                    CODENAME_get("ORDERINFO", OIT0003tab3row("ORDERINFO"), OIT0003tab3row("ORDERINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "(実績)受入日で入力した日付より過去日のためエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Continue For
                Else
                    OIT0003tab3row("ORDERINFO") = ""
                    OIT0003tab3row("ORDERINFONAME") = ""
                End If
            End If

            '〇 (実績)空車着日 と　(一覧)空車着日を比較
            If TxtActualEmparrDate.Text <> "" AndAlso OIT0003tab3row("ACTUALEMPARRDATE") <> "" Then
                iresult = Date.Parse(TxtActualEmparrDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALEMPARRDATE")))
                If iresult = 1 Then
                    OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_95
                    CODENAME_get("ORDERINFO", OIT0003tab3row("ORDERINFO"), OIT0003tab3row("ORDERINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "(実績)空車着日で入力した日付より過去日のためエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Continue For
                Else
                    OIT0003tab3row("ORDERINFO") = ""
                    OIT0003tab3row("ORDERINFONAME") = ""
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        '(一覧)日付有効性チェックがエラーの場合
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)日付 > (一覧)日付", needsPopUp:=True)
            Exit Sub
        End If


    End Sub

    ''' <summary>
    ''' 前回油種と油種の整合性チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckLastOilConsistency(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_GetValue = {"", "", "", "", "", "", "", ""}

        '前回油種と油種の整合性チェック
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            WW_GetValue = {"", "", "", "", "", "", "", ""}
            WW_FixvalueMasterSearch(OIT0003row("LASTOILCODE") + OIT0003row("PREORDERINGTYPE"), "LASTOILCONSISTENCY", OIT0003row("OILCODE") + OIT0003row("ORDERINGTYPE"), WW_GetValue)

            If WW_GetValue(2) = "1" Then
                'Master.Output(C_MESSAGE_NO.OIL_LASTOIL_CONSISTENCY_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_83
                CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                WW_CheckMES1 = "前回油種と油種の整合性エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_LASTOIL_CONSISTENCY_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                'Exit Sub
            Else
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = ""

            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' 高速列車対応タンク車チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckSpeedTrainTank(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_OfficeCode As String = ""
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '〇 検索(営業所).テキストボックスが未設定
        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
            '〇 画面(受注営業所).テキストボックスが未設定
            If work.WF_SEL_ORDERSALESOFFICECODE.Text = "" Then
                WW_OfficeCode = Master.USER_ORG
            Else
                WW_OfficeCode = work.WF_SEL_ORDERSALESOFFICECODE.Text
            End If
        Else
            WW_OfficeCode = work.WF_SEL_SALESOFFICECODE.Text
        End If
        WW_FixvalueMasterSearch(WW_OfficeCode, "TRAINNUMBER_FIND", TxtTrainName.Text, WW_GetValue)

        '高速列車対応タンク車チェック
        For Each OIT0003row As DataRow In OIT0003tbl.Rows

            '高速列車区分＝"1"(高速列車)、かつ型式<>"タキ1000"の場合はエラー
            If WW_GetValue(5) = "1" And OIT0003row("MODEL") <> "タキ1000" Then
                OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_84
                CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                Master.Output(C_MESSAGE_NO.OIL_SPEEDTRAINTANK_ERROR,
                              C_MESSAGE_TYPE.ERR,
                              OIT0003row("TANKNO") + "(" + OIT0003row("MODEL") + ")",
                              needsPopUp:=True)

                WW_CheckMES1 = "高速列車非対応タンク車エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_SPEEDTRAINTANK_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl)
                Exit Sub
            Else
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = ""

            End If
        Next

    End Sub

    ''' <summary>
    ''' 列車重複チェック(同一レコードがすでに登録済みかチェック)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTrainRepeat(ByRef O_RTN As String, ByVal SQLcon As SqlConnection)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        If IsNothing(OIT0003WK2tbl) Then
            OIT0003WK2tbl = New DataTable
        End If

        If OIT0003WK2tbl.Columns.Count <> 0 Then
            OIT0003WK2tbl.Columns.Clear()
        End If

        OIT0003WK2tbl.Clear()

        '○ チェックSQL
        '　説明
        '     登録された内容が受注TBLにすでに登録済みかチェックする

        Dim SQLStr As String =
              " SELECT " _
            & "   ISNULL(RTRIM(OIT0002.ORDERNO), '')        AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')        AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')      AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')     AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')     AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')   AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')   AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0002.BASECODE), '')       AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0002.BASENAME), '')       AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')  AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')  AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')     AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '') AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')     AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '') AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.LODDATE), '')        AS LODDATE" _
            & " , ISNULL(RTRIM(OIT0002.DEPDATE), '')        AS DEPDATE" _
            & " , ISNULL(RTRIM(OIT0002.ARRDATE), '')        AS ARRDATE" _
            & " , ISNULL(RTRIM(OIT0002.ACCDATE), '')        AS ACCDATE" _
            & " , ISNULL(RTRIM(OIT0002.EMPARRDATE), '')     AS EMPARRDATE" _
            & " FROM oil.OIT0002_ORDER OIT0002 " _
            & " WHERE OIT0002.USEPROPRIETYFLG = '1' " _
            & "   AND OIT0002.ORDERNO        <> @P01 " _
            & "   AND OIT0002.TRAINNO         = @P02 " _
            & "   AND OIT0002.DEPDATE         = @P03 " _
            & "   AND OIT0002.ORDERSTATUS    <> @P04 " _
            & "   AND OIT0002.DELFLG         <> @P05 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)発日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA2.Value = Me.TxtTrainNo.Text
                PARA3.Value = Me.TxtDepDate.Text
                PARA4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA5.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WK2tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WK2tbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0003CHKDrow As DataRow In OIT0003WK2tbl.Rows
                    Master.Output(C_MESSAGE_NO.OIL_ORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, OIT0003CHKDrow("ORDERNO"), needsPopUp:=True)

                    WW_CheckMES1 = "受注データ登録済みエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_ORDER_REPEAT_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Exit Sub
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D CHECK_TRAINREPEAT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D CHECK_TRAINREPEAT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 列車マスタ牽引車数チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTrainCars(ByRef O_RTN As String, ByVal SQLcon As SqlConnection)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        If IsNothing(OIT0003WKtbl) Then
            OIT0003WKtbl = New DataTable
        End If

        If OIT0003WKtbl.Columns.Count <> 0 Then
            OIT0003WKtbl.Columns.Clear()
        End If

        OIT0003WKtbl.Clear()

        '○ チェックSQL
        '　説明
        '     受注TBL, 受注明細TBLと列車マスタからMAX牽引車数の値を取得しチェックする

        'JR最終列車番号チェック用
        Dim SQLStr As String =
              " SELECT " _
            & "   'JR最終列車'                              AS CHECKNUMBER" _
            & " , ISNULL(RTRIM(VIW0006_JR3.JRTRAINNO3), '') AS JRTRAINNO" _
            & " , SUM(VIW0006_JR3.CNT)                      AS TRAINCARS" _
            & " , ISNULL(RTRIM(VIW0006_JR3.MAXTANK3), '')   AS MAXTANK" _
            & " FROM ( " _
            & "       SELECT VIW0006.* " _
            & "       FROM oil.VIW0006_TRAINCARSCHECK VIW0006 " _
            & "       WHERE VIW0006.ORDERNO = @P01 " _
            & "      ) VIW0006_BASE " _
            & " INNER JOIN OIL.VIW0006_TRAINCARSCHECK VIW0006_JR3 ON" _
            & "        VIW0006_BASE.JRTRAINNO3  = VIW0006_JR3.JRTRAINNO3 " _
            & "    AND VIW0006_BASE.MERGEDAY    = VIW0006_JR3.MERGEDAY" _
            & "    AND VIW0006_BASE.JRTRAINNO3 <> ''" _
            & " GROUP BY " _
            & "   VIW0006_JR3.JRTRAINNO3" _
            & " , VIW0006_JR3.MAXTANK3"

        'JR中継列車番号チェック用
        SQLStr &=
              " UNION ALL " _
            & " SELECT " _
            & "   'JR中継列車'                              AS CHECKNUMBER" _
            & " , ISNULL(RTRIM(VIW0006_JR2.JRTRAINNO2), '') AS JRTRAINNO" _
            & " , SUM(VIW0006_JR2.CNT)                      AS TRAINCARS" _
            & " , ISNULL(RTRIM(VIW0006_JR2.MAXTANK2), '')   AS MAXTANK" _
            & " FROM ( " _
            & "       SELECT VIW0006.* " _
            & "       FROM oil.VIW0006_TRAINCARSCHECK VIW0006 " _
            & "       WHERE VIW0006.ORDERNO = @P01 " _
            & "      ) VIW0006_BASE " _
            & " INNER JOIN OIL.VIW0006_TRAINCARSCHECK VIW0006_JR2 ON" _
            & "        VIW0006_BASE.JRTRAINNO2  = VIW0006_JR2.JRTRAINNO2 " _
            & "    AND VIW0006_BASE.MERGEDAY    = VIW0006_JR2.MERGEDAY" _
            & "    AND VIW0006_BASE.JRTRAINNO2 <> ''" _
            & " GROUP BY " _
            & "   VIW0006_JR2.JRTRAINNO2" _
            & " , VIW0006_JR2.MAXTANK2"

        'JR発列車番号チェック用
        SQLStr &=
              " UNION ALL " _
            & " SELECT " _
            & "   'JR発列車'                                AS CHECKNUMBER" _
            & " , ISNULL(RTRIM(VIW0006_JR1.JRTRAINNO1), '') AS JRTRAINNO" _
            & " , SUM(VIW0006_JR1.CNT)                      AS TRAINCARS" _
            & " , ISNULL(RTRIM(VIW0006_JR1.MAXTANK1), '')   AS MAXTANK" _
            & " FROM ( " _
            & "       SELECT VIW0006.* " _
            & "       FROM oil.VIW0006_TRAINCARSCHECK VIW0006 " _
            & "       WHERE VIW0006.ORDERNO = @P01 " _
            & "      ) VIW0006_BASE " _
            & " INNER JOIN OIL.VIW0006_TRAINCARSCHECK VIW0006_JR1 ON" _
            & "        VIW0006_BASE.JRTRAINNO1  = VIW0006_JR1.JRTRAINNO1 " _
            & "    AND VIW0006_BASE.MERGEDAY    = VIW0006_JR1.MERGEDAY" _
            & "    AND VIW0006_BASE.JRTRAINNO1 <> ''" _
            & " GROUP BY " _
            & "   VIW0006_JR1.JRTRAINNO1" _
            & " , VIW0006_JR1.MAXTANK1"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WKtbl.Load(SQLdr)
                End Using

                For Each OIT0003UPDrow As DataRow In OIT0003WKtbl.Rows
                    If OIT0003UPDrow("TRAINCARS") > OIT0003UPDrow("MAXTANK") Then
                        Master.Output(C_MESSAGE_NO.OIL_TRAINCARS_ERROR, C_MESSAGE_TYPE.ERR, OIT0003UPDrow("CHECKNUMBER"), needsPopUp:=True)

                        WW_CheckMES1 = "列車牽引車数オーバー。"
                        WW_CheckMES2 = C_MESSAGE_NO.OIL_TRAINCARS_ERROR
                        WW_CheckTRAINCARSERR(WW_CheckMES1, WW_CheckMES2, OIT0003UPDrow)
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D CHECK_TRAINCARS")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D CHECK_TRAINCARS"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 荷受人油種チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckConsigneeOil(ByRef O_RTN As String, ByVal SQLcon As SqlConnection, ByRef O_VALUE As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        If IsNothing(OIT0003WKtbl) Then
            OIT0003WKtbl = New DataTable
        End If

        If OIT0003WKtbl.Columns.Count <> 0 Then
            OIT0003WKtbl.Columns.Clear()
        End If

        OIT0003WKtbl.Clear()

        '○ チェックSQL
        '　説明
        '     タンク車割当で設定した油種について、下記油槽所が受入可能かチェックする。
        '     (JXTG北信油槽所, JXTG甲府油槽所, OT八王子)

        '荷受人油種チェック用
        Dim SQLStr As String =
              " SELECT " _
            & " ISNULL(RTRIM(VIW0007.CONSIGNEECODE), '')   AS CONSIGNEECODE " _
            & " , ISNULL(RTRIM(VIW0007.CONSIGNEENAME), '') AS CONSIGNEENAME " _
            & " , ISNULL(RTRIM(VIW0007.NG_OILCODE), '')    AS NG_OILCODE " _
            & " , ISNULL(RTRIM(VIW0007.NG_OILNAME), '')    AS NG_OILNAME " _
            & " FROM  OIL.VIW0007_CONSIGNEE_OILCHECK VIW0007 " _
            & " WHERE VIW0007.CONSIGNEECODE = @P01 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '荷受人コード
                PARA1.Value = work.WF_SEL_CONSIGNEECODE.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WKtbl.Load(SQLdr)
                End Using

                For Each OIT0003ChkDrow As DataRow In OIT0003WKtbl.Rows
                    For Each OIT0003row As DataRow In OIT0003tbl.Rows
                        If OIT0003ChkDrow("NG_OILCODE") = OIT0003row("OILCODE") + OIT0003row("ORDERINGTYPE") Then
                            'Master.Output(C_MESSAGE_NO.OIL_CONSIGNEE_OILCODE_NG, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                            OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_90
                            CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "荷受人(油槽所)受入油種NG。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_CONSIGNEE_OILCODE_NG
                            WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR"
                            O_VALUE = OIT0003ChkDrow("CONSIGNEENAME")
                            'Exit Sub
                        Else
                            OIT0003row("ORDERINFO") = ""
                            OIT0003row("ORDERINFONAME") = ""
                        End If
                    Next
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl)

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D CHECK_CONSIGNEEOIL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D CHECK_CONSIGNEEOIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 積場スペックチェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckLoadingSpecs(ByRef O_RTN As String, ByRef O_MSG As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_LoadingTotalCars As Integer = 0
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '(一覧)チェック(準備)
        For Each OIT0003row As DataRow In OIT0003tbl_tab2.Rows
            OIT0003row("ORDERINFO") = ""
            OIT0003row("ORDERINFONAME") = ""
        Next
        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

        For Each OIT0003row As DataRow In OIT0003tbl_tab2.Rows
            '(一覧)回線(空白チェック)
            If OIT0003row("LINE") = "" And OIT0003row("DELFLG") = "0" Then
                'Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)回線", needsPopUp:=True)

                WW_CheckMES1 = "回線未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                O_MSG = "(一覧)回線"
                Exit Sub
            End If

            '(一覧)充填ポイント(空白チェック)
            If OIT0003row("FILLINGPOINT") = "" And OIT0003row("DELFLG") = "0" Then
                'Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)充填ポイント", needsPopUp:=True)

                WW_CheckMES1 = "充填ポイント未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                O_MSG = "(一覧)充填ポイント"
                Exit Sub
            End If
        Next

        '〇 回線・充填ポイントでソートし、重複がないかチェックする。
        Dim OIT0003tbl_DUMMY As DataTable = OIT0003tbl_tab2.Copy
        Dim OIT0003tbl_dv As DataView = New DataView(OIT0003tbl_DUMMY)
        Dim chkFillingPoint As String = ""
        OIT0003tbl_dv.Sort = "LINE, FILLINGPOINT"
        For Each drv As DataRowView In OIT0003tbl_dv
            If drv("HIDDEN") <> "1" _
                AndAlso drv("FILLINGPOINT") <> "" _
                AndAlso chkFillingPoint = drv("LINE") + drv("FILLINGPOINT") Then
                'Master.Output(C_MESSAGE_NO.OIL_FILLINGPOINT_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckMES1 = "回線・充填ポイント重複エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_FILLINGPOINT_REPEAT_ERROR
                WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                O_RTN = "ERR1"

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0003tbl_tab2.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = drv("LINECNT"))
                updHeader.Item("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_86
                CODENAME_get("ORDERINFO", updHeader.Item("ORDERINFO"), updHeader.Item("ORDERINFONAME"), WW_DUMMY)

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
                'Exit Sub
            End If

            '行削除したデータの場合は退避しない。
            If drv("HIDDEN") <> "1" Then
                chkFillingPoint = drv("LINE") + drv("FILLINGPOINT")
            End If
        Next
        If O_RTN = "ERR1" Then Exit Sub

        '〇 積場スペックチェック
        For Each OIT0003tab2row As DataRow In OIT0003tbl_tab2.Rows
            WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            WW_FixvalueMasterSearch(work.WF_SEL_BASECODE.Text,
                                "LOADINGSPECS",
                                OIT0003tab2row("OILCODE") _
                                + OIT0003tab2row("ORDERINGTYPE") _
                                + OIT0003tab2row("FILLINGPOINT"),
                                WW_GetValue)

            If WW_GetValue(0) = "" Then
                'Master.Output(C_MESSAGE_NO.OIL_LOADINGSPECS_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                WW_CheckMES1 = "積場スペックエラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_LOADINGSPECS_ERROR
                WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003tab2row)
                O_RTN = "ERR2"

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0003tbl_tab2.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = OIT0003tab2row("LINECNT"))
                updHeader.Item("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_81
                CODENAME_get("ORDERINFO", updHeader.Item("ORDERINFO"), updHeader.Item("ORDERINFONAME"), WW_DUMMY)

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
                'Exit Sub
            Else
                WW_LoadingTotalCars = WW_GetValue(0)

            End If
        Next
        If O_RTN = "ERR2" Then Exit Sub

        '〇 積込可能件数チェック
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_CheckLoadingCnt(WW_ERRCODE, SQLcon)
            If WW_ERRCODE = "ERR3" Then Exit Sub
        End Using

        '積込指示⇒完了
        WW_LoadingInput = "1"

    End Sub

    ''' <summary>
    ''' 積込可能件数チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckLoadingCnt(ByRef O_RTN As String, ByVal SQLcon As SqlConnection)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        If IsNothing(OIT0003WKtbl) Then
            OIT0003WKtbl = New DataTable
        End If

        If OIT0003WKtbl.Columns.Count <> 0 Then
            OIT0003WKtbl.Columns.Clear()
        End If

        OIT0003WKtbl.Clear()

        '(一覧)チェック(準備)
        For Each OIT0003row As DataRow In OIT0003tbl_tab2.Rows
            OIT0003row("ORDERINFO") = ""
            OIT0003row("ORDERINFONAME") = ""
        Next
        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

        '○ チェックSQL
        '　説明
        '     受注TBL, 受注明細TBLと油種マスタから積込可能件数の値を取得しチェックする
        Dim SQLStr As String =
              " SELECT " _
            & "   ISNULL(RTRIM(MERGE_TBL.PLANTCODE), '')    AS PLANTCODE" _
            & " , ISNULL(RTRIM(MERGE_TBL.PLANTNAME), '')    AS PLANTNAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.LINE), '')         AS LINE" _
            & " , ISNULL(RTRIM(MERGE_TBL.BIGOILCODE), '')   AS BIGOILCODE" _
            & " , ISNULL(RTRIM(MERGE_TBL.CHECKOILCODE), '') AS CHECKOILCODE" _
            & " , ISNULL(RTRIM(MERGE_TBL.CHECKOILNAME), '') AS CHECKOILNAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.TANKCOUNT), '')    AS TANKCOUNT" _
            & " , ISNULL(RTRIM(OIM0014.BIGOILCODE), '')     AS CHK_BIGOILCODE" _
            & " , ISNULL(RTRIM(OIM0014.CHECKOILCODE), '')   AS CHK_CHECKOILCODE" _
            & " , ISNULL(RTRIM(OIM0014.TANKCOUNT), '')      AS CHK_TANKCOUNT" _
            & " , CASE WHEN MERGE_TBL.TANKCOUNT <= OIM0014.TANKCOUNT THEN 0 " _
            & "   ELSE 1 " _
            & "   END                                       AS JUDGE " _
            & " FROM ( "

        '基地コード毎の油種件数一覧
        SQLStr &=
              " SELECT " _
            & "   ISNULL(RTRIM(OIM0003.PLANTCODE), '')    AS PLANTCODE" _
            & " , ISNULL(RTRIM(OIM0009.PLANTNAME), '')    AS PLANTNAME" _
            & " , ISNULL(RTRIM(OIT0003.LINE), '')         AS LINE" _
            & " , ISNULL(RTRIM(OIM0003.BIGOILCODE), '')   AS BIGOILCODE" _
            & " , ISNULL(RTRIM(OIM0003.CHECKOILCODE), '') AS CHECKOILCODE" _
            & " , ISNULL(RTRIM(OIM0003.CHECKOILNAME), '') AS CHECKOILNAME" _
            & " , COUNT(1)                                AS TANKCOUNT" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON" _
            & "        OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "    AND OIT0003.DELFLG <> @P02" _
            & " INNER JOIN OIL.OIM0003_PRODUCT OIM0003 ON" _
            & "        OIM0003.OFFICECODE     = OIT0002.OFFICECODE " _
            & "    AND OIM0003.SHIPPERCODE    = OIT0002.SHIPPERSCODE" _
            & "    AND OIM0003.PLANTCODE      = OIT0002.BASECODE" _
            & "    AND OIM0003.OILCODE        = OIT0003.OILCODE" _
            & "    AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE" _
            & "    AND OIM0003.DELFLG        <> @P02" _
            & " INNER JOIN OIL.OIM0009_PLANT OIM0009 ON" _
            & "        OIM0009.PLANTCODE      = OIM0003.PLANTCODE " _
            & "    AND OIM0009.DELFLG        <> @P02" _
            & " WHERE OIT0002.ORDERNO = @P01" _
            & "    AND OIT0002.DELFLG <> @P02" _
            & " GROUP BY " _
            & "   OIM0003.PLANTCODE" _
            & " , OIM0009.PLANTNAME" _
            & " , OIT0003.LINE" _
            & " , OIM0003.BIGOILCODE" _
            & " , OIM0003.CHECKOILCODE" _
            & " , OIM0003.CHECKOILNAME"

        '基地コード毎の油種大分類件数一覧
        SQLStr &=
              " UNION ALL " _
            & " SELECT " _
            & "   ISNULL(RTRIM(OIM0003.PLANTCODE), '')    AS PLANTCODE" _
            & " , ISNULL(RTRIM(OIM0009.PLANTNAME), '')    AS PLANTNAME" _
            & " , ISNULL(RTRIM(OIT0003.LINE), '')         AS LINE" _
            & " , ISNULL(RTRIM(OIM0003.BIGOILCODE), '')   AS BIGOILCODE" _
            & " , 'ZZZZ'                                  AS CHECKOILCODE" _
            & " , ISNULL(RTRIM(OIM0003.BIGOILNAME), '')   AS CHECKOILNAME" _
            & " , COUNT(1)                                AS TANKCOUNT" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON" _
            & "        OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "    AND OIT0003.DELFLG <> @P02" _
            & " INNER JOIN OIL.OIM0003_PRODUCT OIM0003 ON" _
            & "        OIM0003.OFFICECODE     = OIT0002.OFFICECODE " _
            & "    AND OIM0003.SHIPPERCODE    = OIT0002.SHIPPERSCODE" _
            & "    AND OIM0003.PLANTCODE      = OIT0002.BASECODE" _
            & "    AND OIM0003.OILCODE        = OIT0003.OILCODE" _
            & "    AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE" _
            & "    AND OIM0003.DELFLG        <> @P02" _
            & " INNER JOIN OIL.OIM0009_PLANT OIM0009 ON" _
            & "        OIM0009.PLANTCODE      = OIM0003.PLANTCODE " _
            & "    AND OIM0009.DELFLG        <> @P02" _
            & " WHERE OIT0002.ORDERNO = @P01" _
            & "    AND OIT0002.DELFLG <> @P02" _
            & " GROUP BY " _
            & "   OIM0003.PLANTCODE" _
            & " , OIM0009.PLANTNAME" _
            & " , OIT0003.LINE" _
            & " , OIM0003.BIGOILCODE" _
            & " , OIM0003.BIGOILNAME"

        '基地コード毎の油種合計件数一覧
        SQLStr &=
              " UNION ALL " _
            & " SELECT " _
            & "   ISNULL(RTRIM(OIM0003.PLANTCODE), '')    AS PLANTCODE" _
            & " , ISNULL(RTRIM(OIM0009.PLANTNAME), '')    AS PLANTNAME" _
            & " , ISNULL(RTRIM(OIT0003.LINE), '')         AS LINE" _
            & " , ''                                      AS BIGOILCODE" _
            & " , ''                                      AS CHECKOILCODE" _
            & " , '合計'                                  AS CHECKOILNAME" _
            & " , COUNT(1)                                AS TANKCOUNT" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON" _
            & "        OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "    AND OIT0003.DELFLG <> @P02" _
            & " INNER JOIN OIL.OIM0003_PRODUCT OIM0003 ON" _
            & "        OIM0003.OFFICECODE     = OIT0002.OFFICECODE " _
            & "    AND OIM0003.SHIPPERCODE    = OIT0002.SHIPPERSCODE" _
            & "    AND OIM0003.PLANTCODE      = OIT0002.BASECODE" _
            & "    AND OIM0003.OILCODE        = OIT0003.OILCODE" _
            & "    AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE" _
            & "    AND OIM0003.DELFLG        <> @P02" _
            & " INNER JOIN OIL.OIM0009_PLANT OIM0009 ON" _
            & "        OIM0009.PLANTCODE      = OIM0003.PLANTCODE " _
            & "    AND OIM0009.DELFLG        <> @P02" _
            & " WHERE OIT0002.ORDERNO  = @P01" _
            & "    AND OIT0002.DELFLG <> @P02" _
            & " GROUP BY " _
            & "   OIM0003.PLANTCODE" _
            & " , OIM0009.PLANTNAME" _
            & " , OIT0003.LINE"

        SQLStr &=
              " ) MERGE_TBL " _
            & " LEFT JOIN ( " _
            & "      SELECT " _
            & "        OIM0014.PLANTCODE " _
            & "      , OIM0014.BIGOILCODE " _
            & "      , OIM0014.CHECKOILCODE " _
            & "      , OIM0014.TANKCOUNT " _
            & "      FROM OIL.OIM0014_LOADCALC OIM0014 " _
            & "      UNION ALL " _
            & "      SELECT " _
            & "        OIM0014_TOTAL.PLANTCODE " _
            & "      , '' AS BIGOILCODE " _
            & "      , '' AS CHECKOILCODE " _
            & "      , SUM(OIM0014_TOTAL.TANKCOUNT) AS TANKCOUNT " _
            & "      FROM OIL.OIM0014_LOADCALC OIM0014_TOTAL " _
            & "      WHERE OIM0014_TOTAL.CHECKOILCODE = 'ZZZZ' " _
            & "      GROUP BY " _
            & "        OIM0014_TOTAL.PLANTCODE " _
            & " ) OIM0014 ON" _
            & "     OIM0014.PLANTCODE = MERGE_TBL.PLANTCODE " _
            & " AND OIM0014.BIGOILCODE = MERGE_TBL.BIGOILCODE " _
            & " AND OIM0014.CHECKOILCODE = MERGE_TBL.CHECKOILCODE "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WKtbl.Load(SQLdr)
                End Using

                For Each OIT0003UPDrow As DataRow In OIT0003WKtbl.Rows
                    '"1"(車数オーバー)
                    If OIT0003UPDrow("JUDGE") = "1" Then

                        '○ 対象ヘッダー取得
                        Dim updHeader = OIT0003tbl_tab2.AsEnumerable.
                                          FirstOrDefault(Function(x) x.Item("LINECNT") = OIT0003UPDrow("LINECNT"))

                        Select Case OIT0003UPDrow("CHECKOILCODE")
                            '油種(白油・黒油)合計チェック
                            Case "ZZZZ"
                                WW_CheckMES1 = "積込可能(油種大分類毎)件数オーバー。"
                                WW_CheckMES2 = C_MESSAGE_NO.OIL_LOADING_OIL_RECORD_OVER

                                updHeader.Item("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_88
                                CODENAME_get("ORDERINFO", updHeader.Item("ORDERINFO"), updHeader.Item("ORDERINFONAME"), WW_DUMMY)

                                '○ 画面表示データ保存
                                Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

                            '油種合計チェック
                            Case ""
                                WW_CheckMES1 = "積込可能(油種合計)件数オーバー。"
                                WW_CheckMES2 = C_MESSAGE_NO.OIL_LOADING_OIL_RECORD_OVER

                                updHeader.Item("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_89
                                CODENAME_get("ORDERINFO", updHeader.Item("ORDERINFO"), updHeader.Item("ORDERINFONAME"), WW_DUMMY)

                                '○ 画面表示データ保存
                                Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

                                '油種(各種)合計チェック
                            Case Else
                                WW_CheckMES1 = "積込可能(油種毎)件数オーバー。"
                                WW_CheckMES2 = C_MESSAGE_NO.OIL_LOADING_OIL_RECORD_OVER

                                updHeader.Item("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_87
                                CODENAME_get("ORDERINFO", updHeader.Item("ORDERINFO"), updHeader.Item("ORDERINFONAME"), WW_DUMMY)

                                '○ 画面表示データ保存
                                Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
                        End Select

                        WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003UPDrow)
                        O_RTN = "ERR3"
                        Exit Sub
                    End If
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D CHECK_LOADINGCNT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D CHECK_LOADINGCNT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 本線列車に紐づく情報を取得
    ''' </summary>
    ''' <param name="I_Value"></param>
    ''' <remarks></remarks>
    Protected Sub WW_TRAINNUMBER_FIND(ByVal I_Value As String)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '〇 検索(営業所).テキストボックスが未設定
        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
            '〇 画面(受注営業所).テキストボックスが未設定
            If work.WF_SEL_ORDERSALESOFFICECODE.Text = "" Then
                WW_FixvalueMasterSearch(Master.USER_ORG, "TRAINNUMBER_FIND", I_Value, WW_GetValue)
            Else
                WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TRAINNUMBER_FIND", I_Value, WW_GetValue)
            End If
        Else
            WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER_FIND", I_Value, WW_GetValue)
        End If

        '発駅
        TxtDepstationCode.Text = WW_GetValue(1)
        work.WF_SEL_DEPARTURESTATION.Text = TxtDepstationCode.Text
        CODENAME_get("DEPSTATION", TxtDepstationCode.Text, LblDepstationName.Text, WW_DUMMY)
        '着駅
        TxtArrstationCode.Text = WW_GetValue(2)
        work.WF_SEL_ARRIVALSTATION.Text = TxtArrstationCode.Text
        CODENAME_get("ARRSTATION", TxtArrstationCode.Text, LblArrstationName.Text, WW_DUMMY)
        TxtTrainNo.Focus()

        '〇 (予定)の日付を設定
        TxtLoadingDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
        TxtDepDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(6))).ToString("yyyy/MM/dd")
        TxtArrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
        TxtAccDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
        TxtEmparrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(10)) + Integer.Parse(WW_GetValue(11))).ToString("yyyy/MM/dd")

        '〇 積置フラグ(積置列車:T, 非積置列車：N)
        If WW_GetValue(12) = "T" Then
            chkOrderInfo.Checked = True
            work.WF_SEL_STACKINGFLG.Text = "1"
        Else
            chkOrderInfo.Checked = False
            work.WF_SEL_STACKINGFLG.Text = "2"
        End If

        '〇営業所配下情報を取得・設定
        WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '〇 検索(営業所).テキストボックスが未設定
        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
            '〇 画面(受注営業所).テキストボックスが未設定
            If TxtOrderOffice.Text = "" Then
                WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
            Else
                WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstationCode.Text, WW_GetValue)
            End If
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

        work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
        work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
        work.WF_SEL_BASECODE.Text = WW_GetValue(2)
        work.WF_SEL_BASENAME.Text = WW_GetValue(3)
        work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
        work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)
        work.WF_SEL_PATTERNCODE.Text = WW_GetValue(6)
        work.WF_SEL_PATTERNNAME.Text = WW_GetValue(7)

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

    ''' <summary>
    ''' エラーレポート編集(一覧用(タブ「タンク車割当」))
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckListERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項番               =" & OIM0003row("LINECNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主               =" & OIM0003row("SHIPPERSNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注油種           =" & OIM0003row("OILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車番号       =" & OIM0003row("TANKNO")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' エラーレポート編集(一覧用(タブ「入換・積込指示」))
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckListTab2ERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項番               =" & OIM0003row("LINECNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 回線               =" & OIM0003row("LINE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 充填ポイント       =" & OIM0003row("FILLINGPOINT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注油種           =" & OIM0003row("ORDERINGOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車№         =" & OIM0003row("TANKNO")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' エラーレポート編集(列車牽引車数オーバー)
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTRAINCARSERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR列車             =" & OIM0003row("CHECKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 列車番号           =" & OIM0003row("JRTRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 車数               =" & OIM0003row("TRAINCARS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> MAX車数            =" & OIM0003row("MAXTANK")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControl()

        Select Case WF_DetailMView.ActiveViewIndex
           'タンク車割当
            Case 0
                '〇 (一覧)テキストボックスの制御(読取専用)
                Dim divObj = DirectCast(pnlListArea1.FindControl(pnlListArea1.ID & "_DR"), Panel)
                Dim tblObj = DirectCast(divObj.Controls(0), Table)

                '受注進行ステータスが"受注受付"の場合
                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then
                    For Each rowitem As TableRow In tblObj.Rows
                        For Each cellObj As TableCell In rowitem.Controls
                            If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPPERSNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ORDERINGOILNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDARRSTATIONNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDCONSIGNEENAME") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                            End If
                        Next
                    Next

                    '受注進行ステータス＝"200:手配中"
                    '受注進行ステータス＝"210:手配中(入換指示手配済)"
                    '受注進行ステータス＝"220:手配中(積込指示手配済)"
                    '受注進行ステータス＝"230:手配中(託送指示手配済)"
                    '受注進行ステータス＝"240:手配中(入換指示未手配)"
                    '受注進行ステータス＝"250:手配中(積込指示未手配)"
                    '受注進行ステータス＝"260:手配中(託送指示未手配)"
                ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_210 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_220 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_230 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_240 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_250 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 Then
                    For Each rowitem As TableRow In tblObj.Rows
                        For Each cellObj As TableCell In rowitem.Controls
                            If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPPERSNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ORDERINGOILNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDARRSTATIONNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDCONSIGNEENAME") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                            End If
                        Next
                    Next

                    '受注進行ステータスが"受注受付"以外の場合
                Else
                    For Each rowitem As TableRow In tblObj.Rows
                        For Each cellObj As TableCell In rowitem.Controls
                            If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPPERSNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ORDERINGOILNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "LINEORDER") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "TANKNO") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "CHANGETRAINNO") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDCONSIGNEENAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDARRSTATIONNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "CHANGEARRSTATIONNAME") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                            End If
                        Next
                    Next
                End If

            '入換・積込指示
            Case 1
                '〇 (一覧)テキストボックスの制御(読取専用)
                Dim divObj = DirectCast(pnlListArea2.FindControl(pnlListArea2.ID & "_DR"), Panel)
                Dim tblObj = DirectCast(divObj.Controls(0), Table)

                '〇 受注進行ステータスの状態
                Select Case work.WF_SEL_ORDERSTATUS.Text
                '受注進行ステータス＝"200:手配中"
                '受注進行ステータス＝"210:手配中(入換指示手配済)"
                '受注進行ステータス＝"220:手配中(積込指示手配済)"
                '受注進行ステータス＝"230:手配中(託送指示手配済)"
                '受注進行ステータス＝"240:手配中(入換指示未手配)"
                '受注進行ステータス＝"250:手配中(積込指示未手配)"
                '受注進行ステータス＝"260:手配中(託送指示未手配)"
                    Case BaseDllConst.CONST_ORDERSTATUS_200,
                         BaseDllConst.CONST_ORDERSTATUS_210,
                         BaseDllConst.CONST_ORDERSTATUS_220,
                         BaseDllConst.CONST_ORDERSTATUS_230,
                         BaseDllConst.CONST_ORDERSTATUS_240,
                         BaseDllConst.CONST_ORDERSTATUS_250,
                         BaseDllConst.CONST_ORDERSTATUS_260
                        '五井営業所、甲子営業所、袖ヶ浦営業所、三重塩浜営業所の場合
                        '積込列車番号の入力を可能とする。
                        If work.WF_SEL_ORDERSALESOFFICECODE.Text = "011201" _
                            OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = "011202" _
                            OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = "011203" Then

                            WW_RINKAIFLG = True

                            For Each rowitem As TableRow In tblObj.Rows
                                For Each cellObj As TableCell In rowitem.Controls
                                    If cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGIRILINETRAINNO") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGOUTLETTRAINNO") Then
                                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                    End If
                                Next
                            Next

                            '上記以外(仙台営業所、根岸営業所、四日市営業所)の場合
                            '積込列車番号の入力を不可とする。
                        Else
                            For Each rowitem As TableRow In tblObj.Rows
                                For Each cellObj As TableCell In rowitem.Controls
                                    If cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGIRILINETRAINNO") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGIRILINEORDER") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGOUTLETTRAINNO") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGOUTLETORDER") Then
                                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                    End If
                                Next
                            Next

                        End If

                    Case Else
                        For Each rowitem As TableRow In tblObj.Rows
                            For Each cellObj As TableCell In rowitem.Controls
                                If cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGIRILINETRAINNO") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGIRILINEORDER") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LINE") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "FILLINGPOINT") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGOUTLETTRAINNO") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGOUTLETORDER") Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                End If
                            Next
                        Next
                End Select

            'タンク車明細
            Case 2
                '〇 (一覧)テキストボックスの制御(読取専用)
                Dim divObj = DirectCast(pnlListArea3.FindControl(pnlListArea3.ID & "_DR"), Panel)
                Dim tblObj = DirectCast(divObj.Controls(0), Table)

                For Each rowitem As TableRow In tblObj.Rows
                    For Each cellObj As TableCell In rowitem.Controls
                        If cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALLODDATE") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALDEPDATE") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALARRDATE") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALACCDATE") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALEMPARRDATE") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDARRSTATIONNAME") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDCONSIGNEENAME") Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        End If
                    Next
                Next

                '費用入力
            Case 3


        End Select

    End Sub

    ''' <summary>
    ''' 各タブ用退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTAB1TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTAB1TBL.txt"
        work.WF_SEL_INPTAB2TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTAB2TBL.txt"
        work.WF_SEL_INPTAB3TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTAB3TBL.txt"
        work.WF_SEL_INPTAB4TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTAB4TBL.txt"

        '〇メニュー画面から遷移した場合の対応(一覧の保存場所を作成)
        If work.WF_SEL_INPTBL.Text = "" Then
            work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

        End If
    End Sub

#Region "ViewStateを圧縮 これをしないとViewStateが7万文字近くなり重くなる,実行すると9000文字"

    Protected Overrides Sub SavePageStateToPersistenceMedium(ByVal viewState As Object)
        Dim lofF As New LosFormatter
        Using sw As New IO.StringWriter
            lofF.Serialize(sw, viewState)
            Dim viewStateString = sw.ToString()
            Dim bytes = Convert.FromBase64String(viewStateString)
            bytes = CompressByte(bytes)
            ClientScript.RegisterHiddenField("__VSTATE", Convert.ToBase64String(bytes))
        End Using
    End Sub
    Protected Overrides Function LoadPageStateFromPersistenceMedium() As Object
        Dim viewState As String = Request.Form("__VSTATE")
        Dim bytes = Convert.FromBase64String(viewState)
        bytes = DeCompressByte(bytes)
        Dim lofF = New LosFormatter()
        Return lofF.Deserialize(Convert.ToBase64String(bytes))
    End Function
    ''' <summary>
    ''' ByteDetaを圧縮
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function CompressByte(data As Byte()) As Byte()
        Using ms As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress)
            ds.Write(data, 0, data.Length)
            ds.Close()
            Return ms.ToArray
        End Using
    End Function
    ''' <summary>
    ''' Byteデータを解凍
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function DeCompressByte(data As Byte()) As Byte()
        Using inpMs As New IO.MemoryStream(data),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            Return outMs.ToArray
        End Using

    End Function
#End Region

End Class
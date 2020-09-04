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
    Private OIT0003INPtbl_tab4 As DataTable                         'チェック用テーブル(タブ４用)
    Private OIT0003UPDtbl As DataTable                              '更新用テーブル
    Private OIT0003UPDtbl_tab4 As DataTable                         '更新用テーブル(タブ４用)
    Private OIT0003WKtbl As DataTable                               '作業用テーブル
    Private OIT0003WK2tbl As DataTable                              '作業用2テーブル(同一列車(同一発日)チェック用)
    Private OIT0003WK3tbl As DataTable                              '作業用3テーブル(同一列車(同一発日)タンク車チェック用)
    Private OIT0003WK4tbl As DataTable                              '作業用4テーブル(列車入線順重複チェック用)
    Private OIT0003WK5tbl As DataTable                              '作業用5テーブル(列車発送順重複チェック用)
    Private OIT0003WK6tbl As DataTable                              '作業用6テーブル(異なる列車(同一発日)チェック用)
    Private OIT0003WK7tbl As DataTable                              '作業用7テーブル(異なる列車(同一発日)タンク車チェック用)
    Private OIT0003WK8tbl As DataTable                              '作業用8テーブル(異なる列車(同一積込日)タンク車チェック用)
    Private OIT0003WK9tbl As DataTable                              '作業用9テーブル(他受注オーダーで積込日が同日チェック用)
    Private OIT0003WK10tbl As DataTable                             '作業用10テーブル(同一列車(同一積込日)タンク車チェック用)
    Private OIT0003WKtbl_tab4 As DataTable                          '作業用テーブル(タブ４用)
    Private OIT0003Fixvaltbl As DataTable                           '作業用テーブル(固定値マスタ取得用)
    Private OIT0003His1tbl As DataTable                             '履歴格納用テーブル(受注履歴)
    Private OIT0003His2tbl As DataTable                             '履歴格納用テーブル(受注明細履歴)
    Private OIT0003ReportDeliverytbl As DataTable                   '帳票用(託送指示)テーブル
    'Private OIT0003FIDtbl_tab1 As DataTable                         '検索用テーブル(タブ１用)
    'Private OIT0003FIDtbl_tab2 As DataTable                         '検索用テーブル(タブ２用)
    Private OIT0003FIDtbl_tab3 As DataTable                         '検索用1テーブル(タブ３用)
    Private OIT0003FID2tbl_tab3 As DataTable                        '検索用2テーブル(タブ３用)(受注TBLから情報を取得)
    'Private OIT0003FIDtbl_tab4 As DataTable                         '検索用テーブル(タブ４用)

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 7                  'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部タブID
    Private Const CONST_DETAIL_NEWLIST As String = "5"              '明細一覧(新規作成)

    'Private Const CONST_DSPROWCOUNT As Integer = 45                '１画面表示対象
    'Private Const CONST_SCROLLROWCOUNT As Integer = 10              'マウススクロール時の増分
    'Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID
    Private Const CONST_MAX_TABID As Integer = 4                    '詳細タブ数

    '〇タンク車割当状況
    Private Const CONST_TANKNO_STATUS_WARI As String = "割当"
    Private Const CONST_TANKNO_STATUS_MIWARI As String = "未割当"
    Private Const CONST_TANKNO_STATUS_FUKA As String = "不可"
    Private Const CONST_TANKNO_STATUS_ZAN As String = "残車"

    '◯交検・全件アラート表示用
    Private Const CONST_ALERT_STATUS_SAFE As String = "'<div class=""safe""></div>'"
    Private Const CONST_ALERT_STATUS_WARNING As String = "'<div class=""warning""></div>'"
    Private Const CONST_ALERT_STATUS_CAUTION As String = "'<div class=""caution""></div>'"

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

    Private WW_UPBUTTONFLG As String = "0"                          '更新用ボタンフラグ(1:割当確定, 2:入力内容登録, 3:明細更新, 4:訂正更新)

    Private WW_ORDERCNT As Integer = 0                              '受注TBLの件数を設定(0件の場合は貨車連結順序表のみと判断するため)

    Private WW_RINKAIFLG As Boolean = False                         '臨海鉄道対象可否(TRUE：対象, FALSE:未対象)
    Private WW_USEORDERFLG As Boolean = False                       '使用受注オーダー可否(TRUE：使用中, FALSE:未使用)
    Private WW_InitializeTAB3 As Boolean = False                    '

    Private WW_SHIPORDER As String = "0"                            '発送順のMAX値(タブ「タンク車割当」)

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
                    Master.RecoverTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

                    '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                    If CS0013ProfView.SetDispListTextBoxValues(OIT0003tbl, pnlListArea1) Then
                        Master.SaveTable(OIT0003tbl)
                    End If
                    If CS0013ProfView.SetDispListTextBoxValues(OIT0003tbl_tab2, pnlListArea2) Then
                        Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
                    End If
                    If CS0013ProfView.SetDispListTextBoxValues(OIT0003tbl_tab3, pnlListArea3) Then
                        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
                    End If
                    If CS0013ProfView.SetDispListTextBoxValues(OIT0003tbl_tab4, pnlListArea4) Then
                        Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)
                    End If
                    '◯ フラグ初期化
                    Me.WW_UPBUTTONFLG = "0"
                    Me.WW_USEORDERFLG = False
                    Me.WW_InitializeTAB3 = False
                    Me.WF_CheckBoxFLG.Value = "FALSE"
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonCONTACT"               '手配連絡ボタン押下
                            WF_ButtonCONTACT_Click()
                        Case "WF_ButtonRESULT"                '結果受理ボタン押下
                            WF_ButtonRESULT_Click()
                        Case "WF_ButtonDELIVERY"              '託送指示ボタン押下
                            WF_ButtonDELIVERY_Click()
                        Case "WF_ButtonINSERT"                '油種数登録ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonEND"                   '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"               'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_CheckBoxSELECT",
                             "WF_CheckBoxSELECTSTACKING",
                             "WF_CheckBoxSELECTFIRSTRETURN",
                             "WF_CheckBoxSELECTAFTERRETURN",
                             "WF_CheckBoxSELECTOTTRANSPORT"   'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click(WF_ButtonClick.Value)
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
                        Case "WF_EXCEL_UPLOAD"                'ファイルアップロード
                            'WF_FILEUPLOAD()
                        Case "WF_RadioButonClick"             '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"                  '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "WF_ListChange"                  'リスト変更
                            WF_ListChange()
                        Case "WF_DTAB_Click"                  '○DetailTab切替処理
                            WF_Detail_TABChange()
                        Case "btnChkLastOilConfirmYes"        '確認メッセージはいボタン押下(前回油種チェック)
                            '画面表示設定処理(受注進行ステータス)
                            WW_ScreenOrderStatusSet()
                        Case "btnChkLastOilConfirmNo"         '確認メッセージいいえボタン押下(前回油種チェック)

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

            '◯手配連絡フラグ(0：未連絡, 1：連絡)設定
            '　または、受注進行ステータスが100:受注受付, または310:手配完了以降のステータスに変更された場合
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
            '305:手配完了（託送未）
            If work.WF_SEL_CONTACTFLG.Text = "1" _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_900 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then

                '### 20200722 START 指摘票対応(全体(No116))  ##################
                '手配連絡ボタンを非活性
                WF_CONTACTFLG.Value = "1"
                ''### 20200710 START 指摘票対応(全体(No101))  ##################
                ''★臨海鉄道対応(手配連絡ボタン(1：連絡)済みも対象)
                'If WW_RINKAIFLG = True OrElse work.WF_SEL_CONTACTFLG.Text = "1" Then
                '    '手配連絡ボタンを非活性
                '    WF_CONTACTFLG.Value = "1"
                'Else
                '    '★臨海鉄道対象外の営業所の場合は、タブ「タンク車明細」で使用可能とする。
                '    If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
                '        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
                '        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                '        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                '        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
                '        '手配連絡ボタンを活性
                '        WF_CONTACTFLG.Value = "0"
                '    Else
                '        '手配連絡ボタンを非活性
                '        WF_CONTACTFLG.Value = "1"
                '    End If
                'End If
                ''### 20200710 END   指摘票対応(全体(No101))  ##################
                '### 20200722 END   指摘票対応(全体(No116))  ##################
            Else
                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_280 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_290 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_300 Then
                    '手配連絡ボタンを活性
                    WF_CONTACTFLG.Value = "0"

                    '★臨海鉄道対応(臨海鉄道でない営業所)
                    '(入換・積込の運用がないため、"200：手配"の場合に手配連絡ボタンを活性にする。)
                ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
                    AndAlso WW_RINKAIFLG = False Then
                    'AndAlso Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012402 Then
                    '手配連絡ボタンを活性
                    WF_CONTACTFLG.Value = "0"

                Else
                    '手配連絡ボタンを非活性
                    WF_CONTACTFLG.Value = "1"
                End If
            End If

            '◯結果受理フラグ(0：未受理, 1：受理)設定
            '　または、受注進行ステータスが100:受注受付, または310:手配完了以降のステータスに変更された場合
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
            '305:手配完了（託送未）
            If work.WF_SEL_RESULTFLG.Text = "1" _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_900 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then

                '### 20200722 START 指摘票対応(全体(No116))  ##################
                '結果受理ボタンを非活性
                WF_RESULTFLG.Value = "1"
                ''### 20200710 START 指摘票対応(全体(No101))  ##################
                ''★臨海鉄道対応(結果受理ボタン(1：受理)済みも対象)
                'If WW_RINKAIFLG = True OrElse work.WF_SEL_RESULTFLG.Text = "1" Then
                '    '結果受理ボタンを非活性
                '    WF_RESULTFLG.Value = "1"
                'Else
                '    '★臨海鉄道対象外の営業所の場合は、タブ「タンク車明細」で使用可能とする。
                '    If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
                '        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
                '        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                '        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                '        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then

                '        '手配連絡が"1"(連絡)の場合
                '        If work.WF_SEL_CONTACTFLG.Text = "1" Then
                '            '結果受理ボタンを活性
                '            WF_RESULTFLG.Value = "0"
                '        Else
                '            '結果受理ボタンを非活性
                '            WF_RESULTFLG.Value = "1"
                '        End If
                '    Else
                '        '結果受理ボタンを非活性
                '        WF_RESULTFLG.Value = "1"
                '    End If
                'End If
                ''### 20200710 END   指摘票対応(全体(No101))  ##################
                '### 20200722 END   指摘票対応(全体(No116))  ##################

            Else
                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_280 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_290 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_300 Then

                    '手配連絡が"1"(連絡)の場合
                    If work.WF_SEL_CONTACTFLG.Text = "1" Then
                        '結果受理ボタンを活性
                        WF_RESULTFLG.Value = "0"
                    Else
                        '結果受理ボタンを非活性
                        WF_RESULTFLG.Value = "1"
                    End If
                Else
                    '結果受理ボタンを非活性
                    WF_RESULTFLG.Value = "1"

                End If
            End If

            '◯託送指示フラグ(0：未手配, 1：手配)設定
            '　または、受注進行ステータスが100:受注受付, または310:手配完了以降のステータスに変更された場合
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
            '305:手配完了（託送未）
            If work.WF_SEL_DELIVERYFLG.Text = "1" _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_900 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then

                '### 20200710 START 指摘票対応(全体(No101))  ##################
                ''託送指示ボタンを非活性
                'WF_DELIVERYFLG.Value = "1"
                '★臨海鉄道対応(五井営業所、甲子営業所、袖ヶ浦営業所)
                If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011201 _
                    OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011202 _
                    OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011203 Then
                    '### 20200722 START 受注進行ステータスの制御を追加 #################################
                    ''託送指示ボタンを非活性
                    'WF_DELIVERYFLG.Value = "1"
                    '★受注進行ステータスが下記の場合
                    '　305:手配完了（託送未）
                    '　310:手配完了
                    If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 _
                        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 Then
                        '託送指示ボタンを活性
                        WF_DELIVERYFLG.Value = "0"
                    Else
                        '託送指示ボタンを非活性
                        WF_DELIVERYFLG.Value = "1"
                    End If
                    '### 20200722 END   受注進行ステータスの制御を追加 #################################

                    '★託送指示ボタン(1：手配)済み)
                ElseIf work.WF_SEL_DELIVERYFLG.Text = "1" Then
                    '### 20200722 START 受注進行ステータスの制御を追加 #################################
                    ''託送指示ボタンを非活性
                    'WF_DELIVERYFLG.Value = "1"
                    '★三重塩浜営業所の場合
                    '　205:手配中（千葉(根岸を除く)以外）
                    '　305:手配完了（託送未）
                    '　310:手配完了
                    If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012402 _
                        AndAlso (work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
                                 OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 _
                                 OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310) Then
                        '託送指示ボタンを活性
                        WF_DELIVERYFLG.Value = "0"

                        '### 20200902 START 四日市営業所も託送指示を許可 ###################################
                        '★四日市営業所の場合
                        '　205:手配中（千葉(根岸を除く)以外）
                        '　305:手配完了（託送未）
                        '　310:手配完了
                    ElseIf Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012401 _
                        AndAlso (work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
                                 OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 _
                                 OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310) Then
                        '託送指示ボタンを活性
                        WF_DELIVERYFLG.Value = "0"

                        '### 20200902 END   四日市営業所も託送指示を許可 ###################################
                    Else
                        '託送指示ボタンを非活性
                        WF_DELIVERYFLG.Value = "1"
                    End If
                    '### 20200722 END   受注進行ステータスの制御を追加 #################################

                Else
                    '★臨海鉄道対象外の営業所の場合は、タブ「タンク車明細」で使用可能とする。
                    '### 20200722 受注進行ステータスの制御を追加 #################################
                    '205:手配中（千葉(根岸を除く)以外）
                    If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
                        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
                        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 _
                        OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 Then
                        '三重塩浜営業所の場合
                        If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012402 Then
                            '託送指示ボタンを活性
                            WF_DELIVERYFLG.Value = "0"

                            '### 20200902 START 四日市営業所も託送指示を許可 ###################################
                            '四日市営業所の場合
                        ElseIf Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012401 Then
                            '託送指示ボタンを活性
                            WF_DELIVERYFLG.Value = "0"

                            '### 20200902 END   四日市営業所も託送指示を許可 ###################################

                        Else
                            '託送指示ボタンを非活性
                            WF_DELIVERYFLG.Value = "1"
                        End If
                        '### 20200722 START 受注進行ステータスの制御を追加 #################################
                    ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then
                        '託送指示ボタンを活性
                        WF_DELIVERYFLG.Value = "0"
                        '### 20200722 END   受注進行ステータスの制御を追加 #################################
                    Else
                        '託送指示ボタンを非活性
                        WF_DELIVERYFLG.Value = "1"
                    End If
                End If
                '### 20200710 END   指摘票対応(全体(No101))  ##################

            Else
                '★臨海鉄道対応(臨海鉄道である営業所)
                '　または、三重塩浜営業所の場合
                If WW_RINKAIFLG = True _
                    OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012402 Then
                    '託送指示ボタンを活性
                    WF_DELIVERYFLG.Value = "0"

                    '### 20200902 START 四日市営業所も託送指示を許可 ###################################
                ElseIf WW_RINKAIFLG = True _
                    OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012401 Then
                    '託送指示ボタンを活性
                    WF_DELIVERYFLG.Value = "0"
                    '### 20200902 END   四日市営業所も託送指示を許可 ###################################

                Else
                    '託送指示ボタンを非活性
                    WF_DELIVERYFLG.Value = "1"

                End If
            End If

            '◯受注進行ステータスが310:手配完了のステータスに変更された場合
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
            '305:手配完了（託送未）
            If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then
                '### 20200722 START 受注進行ステータスの制御を追加 #################################
                ''タブ「タンク車割当」, タブ「入換・積込指示」のボタンをすべて非活性
                'WF_MAPButtonControl.Value = "1"
                '★臨海鉄道対応(臨海鉄道である営業所)
                If WW_RINKAIFLG = True Then
                    'タブ「タンク車割当」, タブ「入換・積込指示」のボタンをすべて非活性
                    WF_MAPButtonControl.Value = "1"
                Else
                    'タブ「入換・積込指示」のボタンを非活性
                    WF_MAPButtonControl.Value = "2"
                End If
                '### 20200722 END   受注進行ステータスの制御を追加 #################################

                '◯受注進行ステータスが320:受注確定以降のステータスに変更された場合
            ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then

                'タブ「タンク車割当」, タブ「入換・積込指示」のボタンをすべて非活性
                WF_MAPButtonControl.Value = "1"

                '◯受注進行ステータスが500:検収中以降のステータスに変更された場合
            ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_900 Then

                'タブ「タンク車割当」, タブ「入換・積込指示」, タブ「タンク車明細」のボタンをすべて非活性
                WF_MAPButtonControl.Value = "3"

            Else
                '★臨海鉄道対応(臨海鉄道でない営業所)
                '(入換・積込の運用がないため、更新ボタンを活性にする。)
                If WW_RINKAIFLG = False Then
                    'タブ「入換・積込指示」のボタンを非活性
                    WF_MAPButtonControl.Value = "2"
                Else
                    WF_MAPButtonControl.Value = "0"
                End If

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

            '手配連絡フラグ(0：未連絡, 1：連絡)
            work.WF_SEL_CONTACTFLG.Text = "0"
            '結果受理フラグ(0：未受理, 1：受理)
            work.WF_SEL_RESULTFLG.Text = "0"
            '託送指示フラグ(0：未手配, 1:手配)
            work.WF_SEL_DELIVERYFLG.Text = "0"
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
        WF_ButtonInsertFLG.Value = "FALSE"
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
            '   受注進行ステータス＝"200:手配"
            '   受注進行ステータス＝"210:手配中(入換指示入力済)"
            '   受注進行ステータス＝"220:手配中(積込指示入力済)"
            '   受注進行ステータス＝"230:手配中(託送指示手配済)"
            '   受注進行ステータス＝"240:手配中(入換指示未入力)"
            '   受注進行ステータス＝"250:手配中(積込指示未入力)"
            '   受注進行ステータス＝"260:手配中(託送指示未手配)"
            '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
            '   受注進行ステータス＝"270:手配中(入換積込指示手配済)"
            '   受注進行ステータス＝"280:手配中(託送指示未手配)"入換積込手配連絡（手配・結果受理）
            '   受注進行ステータス＝"290:手配中(入換積込未連絡)"
            '   受注進行ステータス＝"300:手配中(入換積込未確認)"
            '### END   ##################################################################
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_210 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_220 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_230 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_240 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_250 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_280 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_290 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_300 Then
            WF_DTAB_CHANGE_NO.Value = "1"
            WF_DetailMView.ActiveViewIndex = WF_DTAB_CHANGE_NO.Value

            '〇 (一覧)テキストボックスの制御(読取専用)
            WW_ListTextBoxReadControl()

            '〇 受注進行ステータスが"310:手配完了"へ変更された場合
            '### ステータス追加(仮) #################################
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
            '305:手配完了（託送未）
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then
            WF_DTAB_CHANGE_NO.Value = "2"
            WF_DetailMView.ActiveViewIndex = WF_DTAB_CHANGE_NO.Value

            '〇 (一覧)テキストボックスの制御(読取専用)
            WW_ListTextBoxReadControl()

            '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
            If Me.WW_USEORDERFLG = True Then
                Master.Output(C_MESSAGE_NO.OIL_ORDERNO_WAR_MESSAGE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
            End If
            '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################

            '〇 受注進行ステータスが"500:検収中"へ変更された場合
            '### ステータス追加(仮) #################################
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 Then

            WF_DTAB_CHANGE_NO.Value = "3"
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
            Me.TxtOrderOfficeCode.Text = work.WF_SEL_LINK_ORDERSALESOFFICE.Text
            CODENAME_get("SALESOFFICE", Me.TxtOrderOfficeCode.Text, Me.TxtOrderOffice.Text, WW_RTN_SW)

            work.WF_SEL_ORDERSALESOFFICE.Text = Me.TxtOrderOffice.Text
            work.WF_SEL_ORDERSALESOFFICECODE.Text = Me.TxtOrderOfficeCode.Text

            '作成モード(２：更新)
        ElseIf work.WF_SEL_CREATEFLG.Text = "2" Then
            Me.TxtOrderOffice.Text = work.WF_SEL_ORDERSALESOFFICE.Text
            Me.TxtOrderOfficeCode.Text = work.WF_SEL_ORDERSALESOFFICECODE.Text

            '作成モード(１：新規登録)
        Else
            Me.TxtOrderOffice.Text = work.WF_SEL_SALESOFFICE.Text
            Me.TxtOrderOfficeCode.Text = work.WF_SEL_SALESOFFICECODE.Text

        End If

        'ステータス
        If work.WF_SEL_ORDERSTATUSNM.Text = "" Then
            work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100
            CODENAME_get("ORDERSTATUS", BaseDllConst.CONST_ORDERSTATUS_100, work.WF_SEL_ORDERSTATUSNM.Text, WW_DUMMY)
        End If
        Me.TxtOrderStatus.Text = work.WF_SEL_ORDERSTATUSNM.Text

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
        Me.TxtOrderType.Text = work.WF_SEL_PATTERNNAME.Text

        'オーダー№
        If work.WF_SEL_ORDERNUMBER.Text = "" Then
            Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
            WW_FixvalueMasterSearch("", "NEWORDERNOGET", "", WW_GetValue)
            work.WF_SEL_ORDERNUMBER.Text = WW_GetValue(0)
            Me.TxtOrderNo.Text = work.WF_SEL_ORDERNUMBER.Text
        Else
            Me.TxtOrderNo.Text = work.WF_SEL_ORDERNUMBER.Text
        End If

        '〇 作成(貨車連結用)フラグ(２：更新)　かつ、作成モード(１：新規)
        If work.WF_SEL_CREATELINKFLG.Text = "2" _
            AndAlso work.WF_SEL_CREATEFLG.Text = "1" Then

            Me.TxtTrainNo.Text = work.WF_SEL_LINK_TRAIN.Text
            Me.TxtTrainName.Text = work.WF_SEL_LINK_TRAINNAME.Text

            '〇 貨車連結表のみで作成の場合、取得した列車名から各値を取得し設定する。
            WW_TRAINNUMBER_FIND(work.WF_SEL_LINK_TRAINNAME.Text)
        Else
            '本線列車
            Me.TxtTrainNo.Text = work.WF_SEL_TRAIN.Text
            Me.TxtTrainName.Text = work.WF_SEL_TRAINNAME.Text
            'OT列車番号
            Me.TxtOTTrainNo.Text = work.WF_SEL_OTTRAIN.Text
            '荷主
            Me.TxtShippersCode.Text = work.WF_SEL_SHIPPERSCODE.Text
            '荷受人
            Me.TxtConsigneeCode.Text = work.WF_SEL_CONSIGNEECODE.Text
            '発駅
            Me.TxtDepstationCode.Text = work.WF_SEL_DEPARTURESTATION.Text
            '着駅
            Me.TxtArrstationCode.Text = work.WF_SEL_ARRIVALSTATION.Text
            '(予定)積込日
            Me.TxtLoadingDate.Text = work.WF_SEL_LODDATE.Text
            '(予定)発日
            Me.TxtDepDate.Text = work.WF_SEL_DEPDATE.Text
            '(予定)積車着日
            Me.TxtArrDate.Text = work.WF_SEL_ARRDATE.Text
            '(予定)受入日
            Me.TxtAccDate.Text = work.WF_SEL_ACCDATE.Text
            '(予定)空車着日
            Me.TxtEmparrDate.Text = work.WF_SEL_EMPARRDATE.Text
        End If

        '(実績)積込日
        Me.TxtActualLoadingDate.Text = work.WF_SEL_ACTUALLODDATE.Text
        '(実績)発日
        Me.TxtActualDepDate.Text = work.WF_SEL_ACTUALDEPDATE.Text
        '(実績)積車着日
        Me.TxtActualArrDate.Text = work.WF_SEL_ACTUALARRDATE.Text
        '(実績)受入日
        Me.TxtActualAccDate.Text = work.WF_SEL_ACTUALACCDATE.Text
        '(実績)空車着日
        Me.TxtActualEmparrDate.Text = work.WF_SEL_ACTUALEMPARRDATE.Text

        '○ 油種別タンク車数(車)データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_OILTANKCntGet(SQLcon)
        End Using

        '計上月
        If work.WF_SEL_KEIJYOYMD.Text <> "" Then
            Dim dt As DateTime = DateTime.ParseExact(work.WF_SEL_KEIJYOYMD.Text, "yyyy/MM/dd", Nothing)
            Me.TxtBudgetMonth.Text = dt.Year.ToString() + "/" + dt.Month.ToString("00")
        Else
            Me.TxtBudgetMonth.Text = work.WF_SEL_KEIJYOYMD.Text
        End If

        If work.WF_SEL_CREATEFLG.Text = "2" Then
            '売上合計金額(税抜)
            Me.TxtTotalSales.Text = work.WF_SEL_SALSE.Text
            '支払合計金額(税抜)
            Me.TxtTitalPayment.Text = work.WF_SEL_PAYMENT.Text
            '売上合計金額(税額)
            Me.TxtTotalSales2.Text = work.WF_SEL_TOTALSALSE.Text
            '支払合計金額(税額)
            Me.TxtTitalPayment2.Text = work.WF_SEL_TOTALPAYMENT.Text
        Else
            '売上合計金額(税抜)
            Me.TxtTotalSales.Text = "0"
            '支払合計金額(税抜)
            Me.TxtTitalPayment.Text = "0"
            '売上合計金額(税額)
            Me.TxtTotalSales2.Text = "0"
            '支払合計金額(税額)
            Me.TxtTitalPayment2.Text = "0"
        End If

        '● タブ「タンク車割当」
        '　■油種別タンク車数(車)
        If work.WF_SEL_CREATEFLG.Text = "2" Then
            'ハイオク(タンク車数)
            Me.TxtHTank.Text = work.WF_SEL_HIGHOCTANE_TANKCAR.Text
            'レギュラー(タンク車数)
            Me.TxtRTank.Text = work.WF_SEL_REGULAR_TANKCAR.Text
            '灯油(タンク車数)
            Me.TxtTTank.Text = work.WF_SEL_KEROSENE_TANKCAR.Text
            '未添加灯油(タンク車数)
            Me.TxtMTTank.Text = work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text
            '軽油(タンク車数)
            Me.TxtKTank.Text = work.WF_SEL_DIESEL_TANKCAR.Text
            '3号軽油(タンク車数)
            Me.TxtK3Tank.Text = work.WF_SEL_NUM3DIESEL_TANKCAR.Text
            '5号軽油(タンク車数)
            Me.TxtK5Tank.Text = work.WF_SEL_NUM5DIESEL_TANKCAR.Text
            '10号軽油(タンク車数)
            Me.TxtK10Tank.Text = work.WF_SEL_NUM10DIESEL_TANKCAR.Text
            'LSA(タンク車数)
            Me.TxtLTank.Text = work.WF_SEL_LSA_TANKCAR.Text
            'A重油(タンク車数)
            Me.TxtATank.Text = work.WF_SEL_AHEAVY_TANKCAR.Text
            'タンク車合計
            Me.TxtTotalCnt.Text = work.WF_SEL_TANKCARTOTAL.Text
        Else
            'ハイオク(タンク車数)
            Me.TxtHTank.Text = "0"
            'レギュラー(タンク車数)
            Me.TxtRTank.Text = "0"
            '灯油(タンク車数)
            Me.TxtTTank.Text = "0"
            '未添加灯油(タンク車数)
            Me.TxtMTTank.Text = "0"
            '軽油(タンク車数)
            Me.TxtKTank.Text = "0"
            '3号軽油(タンク車数)
            Me.TxtK3Tank.Text = "0"
            '5号軽油(タンク車数)
            Me.TxtK5Tank.Text = "0"
            '10号軽油(タンク車数)
            Me.TxtK10Tank.Text = "0"
            'LSA(タンク車数)
            Me.TxtLTank.Text = "0"
            'A重油(タンク車数)
            Me.TxtATank.Text = "0"
            'タンク車合計
            Me.TxtTotalCnt.Text = "0"
        End If

        '　■割当後　油種別タンク車数(車)
        If work.WF_SEL_CREATEFLG.Text = "2" Then
            'ハイオク(タンク車数)
            Me.TxtHTank_w.Text = work.WF_SEL_HIGHOCTANECH_TANKCAR.Text
            'レギュラー(タンク車数)
            Me.TxtRTank_w.Text = work.WF_SEL_REGULARCH_TANKCAR.Text
            '灯油(タンク車数)
            Me.TxtTTank_w.Text = work.WF_SEL_KEROSENECH_TANKCAR.Text
            '未添加灯油(タンク車数)
            Me.TxtMTTank_w.Text = work.WF_SEL_NOTADDED_KEROSENECH_TANKCAR.Text
            '軽油(タンク車数)
            Me.TxtKTank_w.Text = work.WF_SEL_DIESELCH_TANKCAR.Text
            '3号軽油(タンク車数)
            Me.TxtK3Tank_w.Text = work.WF_SEL_NUM3DIESELCH_TANKCAR.Text
            '5号軽油(タンク車数)
            Me.TxtK5Tank_w.Text = work.WF_SEL_NUM5DIESELCH_TANKCAR.Text
            '10号軽油(タンク車数)
            Me.TxtK10Tank_w.Text = work.WF_SEL_NUM10DIESELCH_TANKCAR.Text
            'LSA(タンク車数)
            Me.TxtLTank_w.Text = work.WF_SEL_LSACH_TANKCAR.Text
            'A重油(タンク車数)
            Me.TxtATank_w.Text = work.WF_SEL_AHEAVYCH_TANKCAR.Text
            'タンク車合計(割当)
            Me.TxtTotalCnt_w.Text = work.WF_SEL_TANKCARTOTALCH.Text
        Else
            'ハイオク(タンク車数)
            Me.TxtHTank_w.Text = "0"
            'レギュラー(タンク車数)
            Me.TxtRTank_w.Text = "0"
            '灯油(タンク車数)
            Me.TxtTTank_w.Text = "0"
            '未添加灯油(タンク車数)
            Me.TxtMTTank_w.Text = "0"
            '軽油(タンク車数)
            Me.TxtKTank_w.Text = "0"
            '3号軽油(タンク車数)
            Me.TxtK3Tank_w.Text = "0"
            '5号軽油(タンク車数)
            Me.TxtK5Tank_w.Text = "0"
            '10号軽油(タンク車数)
            Me.TxtK10Tank_w.Text = "0"
            'LSA(タンク車数)
            Me.TxtLTank_w.Text = "0"
            'A重油(タンク車数)
            Me.TxtATank_w.Text = "0"
            'タンク車合計(割当)
            Me.TxtTotalCnt_w.Text = "0"
        End If

        '車数を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtHTank_w.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtRTank_w.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTTank_w.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtMTTank_w.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKTank_w.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtK3Tank_w.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtK5Tank_w.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtK10Tank_w.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtLTank_w.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtATank_w.Attributes("onkeyPress") = "CheckNum()"

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", work.WF_SEL_UORG.Text, Me.WF_UORG_TEXT.Text, WW_DUMMY)
        '荷主
        CODENAME_get("SHIPPERS", Me.TxtShippersCode.Text, Me.LblShippersName.Text, WW_DUMMY)
        '荷受人
        CODENAME_get("CONSIGNEE", Me.TxtConsigneeCode.Text, Me.LblConsigneeName.Text, WW_DUMMY)
        '発駅
        CODENAME_get("DEPSTATION", Me.TxtDepstationCode.Text, Me.LblDepstationName.Text, WW_DUMMY)
        '着駅
        CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_DUMMY)

        '消費税の取得
        Dim WW_GetConsumptionTax() As String = {"", "", "", "", "", "", "", ""}
        WW_FixvalueMasterSearch("", "CONSUMPTIONTAX", "", WW_GetConsumptionTax)
        work.WF_SEL_CONSUMPTIONTAX.Text = WW_GetConsumptionTax(1)

        '輸送形態区分の取得
        If Me.TxtOrderOfficeCode.Text <> "" AndAlso Me.TxtArrstationCode.Text <> "" Then
            Dim WW_GetTrkKbn() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetTrkKbn)
            Me.TxtOrderTrkKbn.Text = WW_GetTrkKbn(8)
        Else
            Me.TxtOrderTrkKbn.Text = ""
        End If

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

        '〇タブ「費用入力」表示用
        GridViewInitializeTab4()

        '〇タンク車所在の更新
        WW_TankShozaiSet()

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

        '★受注オーダーが存在する場合
        If OIT0003tbl.Rows.Count <> 0 Then
            For Each OIT0003row As DataRow In OIT0003tbl.Rows
                If OIT0003row("TANKNO") = "" Then Continue For
                '★タンク車№に紐づく情報を取得
                WW_TANKNUMBER_FIND(OIT0003row, I_CMPCD:=work.WF_SEL_CAMPCODE.Text)
            Next
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

        WF_DetailMView.ActiveViewIndex = 1
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

        '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
        If work.WF_SEL_ORDERSTATUS.Text <= BaseDllConst.CONST_ORDERSTATUS_450 Then
            '◯受注№存在チェック
            WW_OrderNoExistChk()
        End If
        '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################

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

        '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
        For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
            '指定タンク車№が他の受注オーダーで使用中の場合は、(実績)日付を許可しない。
            If OIT0003tab3row("USEORDERNO") = "" Then
                Continue For
            ElseIf OIT0003tab3row("USEORDERNO") <> Me.TxtOrderNo.Text Then
                '使用受注オーダーを使用中に変更
                Me.WW_USEORDERFLG = True
            End If
        Next
        '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################

        '### 20200626 START (一覧)積置をチェックした場合の表示方法を変更 #################################
        '「一部積置」表示チェック
        '※積置ありの場合はそのまま表示
        If chkOrderInfo.Checked = False Then
            For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                If OIT0003tab3row("STACKINGFLG") = "on" Then
                    '(一覧)積置が１つでもチェックされた場合は、表示を「一部積置」に切り替える。
                    chkOrderInfo.Visible = False        'チェックボックス(積置なし・積置あり)を非表示
                    chkOrderDetailInfo.Visible = True   'チェックボックス(一部積置)表示
                    Exit For
                Else
                    chkOrderInfo.Visible = True         'チェックボックス(積置なし・積置あり)を表示
                    chkOrderDetailInfo.Visible = False  'チェックボックス(一部積置)非表示
                End If
            Next
        End If
        '### 20200626 END   (一覧)積置をチェックした場合の表示方法を変更 #################################

        WF_DetailMView.ActiveViewIndex = 2
        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

#Region "タブ「費用入力」関連処理"
    ''' <summary>
    ''' GridViewデータ設定(タブ「費用入力」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab4()
        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '勘定科目明細作成
            WW_InsertRequestAccountDetail(SQLcon)

            '費用入力一覧(勘定科目サマリー作成)
            MAPDataGetTab4(SQLcon)

            '費用入力一覧(勘定科目追加項目作成)
            MAPDataADDTab4(SQLcon)

        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0003tbl_tab4)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

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
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        WF_DetailMView.ActiveViewIndex = 3
        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 請求科目（明細）データ作成処理
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_InsertRequestAccountDetail(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0003INPtbl_tab4) Then
            OIT0003INPtbl_tab4 = New DataTable
        End If

        If OIT0003INPtbl_tab4.Columns.Count <> 0 Then
            OIT0003INPtbl_tab4.Columns.Clear()
        End If

        OIT0003INPtbl_tab4.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String = ""
        Dim SQLSelectStr As String = ""
        Dim SQLFromStr1 As String = ""
        Dim SQLFromStr2 As String = ""
        Dim SQLTempTblStr As String = ""

        '削除・追加用
        SQLTempTblStr =
          " DELETE FROM OIL.TMP0002RATE; " _
        & " INSERT INTO OIL.TMP0002RATE "

        '共通SELECT用
        SQLSelectStr =
          " SELECT" _
        & "   0                                                  AS LINECNT" _
        & " , ''                                                 AS OPERATION" _
        & " , ''                                                 AS TIMSTP" _
        & " , 1                                                  AS 'SELECT'" _
        & " , 0                                                  AS HIDDEN" _
        & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')                 AS ORDERNO" _
        & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')                AS DETAILNO" _
        & " , ISNULL(RTRIM(OIM0010.PATCODE), '')                 AS PATCODE" _
        & " , ISNULL(RTRIM(OIM0010.PATNAME), '')                 AS PATNAME" _
        & " , ISNULL(RTRIM(OIM0010.ACCOUNTCODE), '')             AS ACCOUNTCODE" _
        & " , ISNULL(RTRIM(VIW0012.ACCOUNTNAME), '')             AS ACCOUNTNAME" _
        & " , ISNULL(RTRIM(OIM0010.SEGMENTCODE), '')             AS SEGMENTCODE" _
        & " , ISNULL(RTRIM(VIW0012.SEGMENTNAME), '')             AS SEGMENTNAME" _
        & " , ISNULL(RTRIM(VIW0012.BREAKDOWNCODE), '')           AS BREAKDOWNCODE" _
        & " , ISNULL(RTRIM(VIW0012.BREAKDOWN), '')               AS BREAKDOWN" _
        & " , ISNULL(RTRIM(OIT0003.SHIPPERSCODE), '')            AS SHIPPERSCODE" _
        & " , ISNULL(RTRIM(OIT0003.SHIPPERSNAME), '')            AS SHIPPERSNAME" _
        & " , ISNULL(RTRIM(OIT0002.BASECODE), '')                AS BASECODE" _
        & " , ISNULL(RTRIM(OIT0002.BASENAME), '')                AS BASENAME" _
        & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')              AS OFFICECODE" _
        & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')              AS OFFICENAME" _
        & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')              AS DEPSTATION" _
        & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')          AS DEPSTATIONNAME" _
        & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')              AS ARRSTATION" _
        & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')          AS ARRSTATIONNAME" _
        & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')           AS CONSIGNEECODE" _
        & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')           AS CONSIGNEENAME" _
        & " , ISNULL(RTRIM(OIT0002.KEIJYOYMD), FORMAT(GETDATE(), 'yyyy/MM/dd'))             AS KEIJYOYMD" _
        & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')                 AS TRAINNO" _
        & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')               AS TRAINNAME" _
        & " , ISNULL(RTRIM(OIM0005.MODEL), '')                   AS MODEL" _
        & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                  AS TANKNO" _
        & " , ISNULL(RTRIM(OIT0003.CARSNUMBER), '')              AS CARSNUMBER" _
        & " , ISNULL(RTRIM(OIT0003.CARSAMOUNT), '')              AS CARSAMOUNT" _
        & " , ISNULL(RTRIM(OIM0005.LOAD), '')                    AS LOAD" _
        & " , ISNULL(RTRIM(OIT0003.OILCODE), '')                 AS OILCODE" _
        & " , ISNULL(RTRIM(OIT0003.OILNAME), '')                 AS OILNAME" _
        & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')            AS ORDERINGTYPE" _
        & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '')         AS ORDERINGOILNAME" _
        & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNO), '')           AS CHANGETRAINNO" _
        & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNAME), '')         AS CHANGETRAINNAME" _
        & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEECODE), '')     AS SECONDCONSIGNEECODE" _
        & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEENAME), '')     AS SECONDCONSIGNEENAME" _
        & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATION), '')        AS SECONDARRSTATION" _
        & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATIONNAME), '')    AS SECONDARRSTATIONNAME" _
        & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATION), '')        AS CHANGERETSTATION" _
        & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATIONNAME), '')    AS CHANGERETSTATIONNAME" _
        & " , ISNULL(RTRIM(VIW0012.TRKBN), '')                   AS TRKBN" _
        & " , ISNULL(RTRIM(VIW0012.TRKBNNAME), '')               AS TRKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.KIRO), '')                    AS KIRO" _
        & " , ISNULL(RTRIM(VIW0012.BRANCH), '')                  AS BRANCH"

        '共通FROM用1
        SQLFromStr1 =
          " FROM OIL.OIT0002_ORDER OIT0002 " _
        & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
        & "       OIT0003.ORDERNO = OIT0002.ORDERNO" _
        & "       AND OIT0003.DELFLG <> @P02" _
        & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
        & "       OIT0003.TANKNO = OIM0005.TANKNUMBER" _
        & "       AND OIM0005.DELFLG <> @P02" _
        & " LEFT JOIN OIL.OIM0010_PATTERN OIM0010 ON " _
        & "       OIM0010.PATCODE = OIT0002.ORDERTYPE" _
        & "       AND OIM0010.WORKCODE = '9'" _
        & "       AND OIM0010.DELFLG <> @P02"

        '共通FROM用2
        SQLFromStr2 =
          "       VIW0012.ACCOUNTCODE = OIM0010.ACCOUNTCODE" _
        & "       AND VIW0012.SEGMENTCODE = OIM0010.SEGMENTCODE" _
        & "       AND VIW0012.SHIPPERSCODE = OIT0003.SHIPPERSCODE" _
        & "       AND VIW0012.BASECODE = OIT0002.BASECODE" _
        & "       AND VIW0012.OFFICECODE = OIT0002.OFFICECODE" _
        & "       AND VIW0012.DEPSTATION = OIT0002.DEPSTATION" _
        & "       AND VIW0012.ARRSTATION = OIT0002.ARRSTATION" _
        & "       AND VIW0012.CONSIGNEECODE = OIT0002.CONSIGNEECODE" _
        & "       AND VIW0012.LOAD = OIM0005.LOAD"

        '★作成SQL
        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(タンク車使用料)
        '#############################################################################
        SQLStr =
            SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.TCCALCKBN), '')                  AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.TCCALCKBNNAME), '')              AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.TCCHARGE), '')                   AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ''                                                    AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.TCDISCOUNT1), '')                AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.TCDISCOUNT2), '')                AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.TCDISCOUNT3), '')                AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.TCAPPLYCHARGE), '')              AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.TCINVOICECODE), '')              AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.TCINVOICENAME), '')              AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.TCINVOICEDEPTNAME), '')          AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.TCPAYEECODE), '')                AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.TCPAYEENAME), '')                 AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.TCPAYEEDEPTNAME), '')            AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10101 VIW0012 ON " _
        & SQLFromStr2 _
        & "       And VIW0012.SENDAI_MORIOKA_FLAG =" _
        & "           Case WHEN OIT0002.BASECODE = '0401' AND OIT0002.CONSIGNEECODE = '51' THEN" _
        & "                Case WHEN OIT0003.OILCODE = '1001' OR OIT0003.OILCODE = '1101' THEN '1' ELSE '2' END" _
        & "           Else '0' END" _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(鉄道運賃)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.FARECALCKBN), '')                AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.FARECALCKBNNAME), '')            AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.SYOTEIHAZFARE), '')              AS CHARGE" _
        & " , ISNULL(RTRIM(VIW0012.HAZJRDISCOUNT), '')              AS JRDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HAZOTDISCOUNT), '')              AS OTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HAZJOTDISCOUNT), '')             AS JOTDISCOUNT" _
        & " , ''                                                    AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HAZDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.HAZDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ''                                                    AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.HAZFARE), '')                    AS APPLYCHARGE" _
        & " , ISNULL(RTRIM(VIW0012.RETURNFARE), '')                 AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.FAREINVOICECODE), '')            AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.FAREINVOICENAME), '')            AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.FAREINVOICEDEPTNAME), '')        AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.FAREPAYEECODE), '')              AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.FAREPAYEENAME), '')               AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.FAREPAYEEDEPTNAME), '')          AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10102_1 VIW0012 ON " _
        & SQLFromStr2 _
        & "       And VIW0012.SENDAI_MORIOKA_FLAG =" _
        & "           Case WHEN OIT0003.OILCODE = '1001' OR OIT0003.OILCODE = '1101' THEN '1' ELSE '2' END" _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.MOTCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.MOTCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.MOTCHARGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ''                                                    AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.MOTDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.MOTDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.MOTDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.MOTAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.MOTINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.MOTINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.MOTINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.MOTPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.MOTPAYEENAME), '')               AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.MOTPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10102_2 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(業務料)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.WRKCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.WRKCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.WRKCHARGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.WRKDISCOUNT), '')                AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.WRKDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.WRKDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.WRKDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.WRKAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.WRKINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.WRKINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.WRKINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.WRKPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.WRKPAYEENAME), '')                AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.WRKPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10103 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(取扱料)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.HNDCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.HNDCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.HNDCAHRGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HNDDISCOUNT), '')                AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HNDDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.HNDDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.HNDDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.HNDAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.HNDINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.HNDINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.HNDINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.HNDPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.HNDPAYEENAME), '')                AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.HNDPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10104 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(ＯＴ業務料)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.OTCALCKBN), '')                  AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.OTCALCKBNNAME), '')              AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.OTCAHRGE), '')                   AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.OTDISCOUNT), '')                 AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.OTDISCOUNT1), '')                AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.OTDISCOUNT2), '')                AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.OTDISCOUNT3), '')                AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.OTAPPLYCHARGE), '')              AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.OTINVOICECODE), '')              AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.OTINVOICENAME), '')              AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.OTINVOICEDEPTNAME), '')          AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.OTPAYEECODE), '')                AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.OTPAYEENAME), '')                 AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.OTPAYEEDEPTNAME), '')            AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10105 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(運賃手数料)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.FRTCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.FRTCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.FRTCAHRGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.FRTDISCOUNT), '')                AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.FRTDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.FRTDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.FRTDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.FRTAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.FRTINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.FRTINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.FRTINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.FRTPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.FRTPAYEENAME), '')                AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.FRTPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10106 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(委託作業費)
        '　セグメント(通運取扱その他)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.COMCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.COMCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.COMCAHRGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.COMDISCOUNT), '')                AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.COMDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.COMDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.COMDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.COMAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.COMINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.COMINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.COMINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.COMPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.COMPAYEENAME), '')                AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.COMPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_51020104_10106 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '削除・追加用にSELECT分を追加
        SQLTempTblStr &= SQLStr

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLTMPcmd As New SqlCommand(SQLTempTblStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                'tmp作成用
                Dim PARATMP01 As SqlParameter = SQLTMPcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARATMP02 As SqlParameter = SQLTMPcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARATMP01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARATMP02.Value = C_DELETE_FLG.DELETE

                SQLTMPcmd.ExecuteNonQuery()

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003INPtbl_tab4.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003INPtbl_tab4.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003row As DataRow In OIT0003INPtbl_tab4.Rows
                    i += 1
                    OIT0003row("LINECNT") = i        'LINECNT
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB4 InsertRequestAccountDetail")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB4 InsertRequestAccountDetail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub
#End Region

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
            & " , ''                                             AS SHIPORDER" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS TANKSTATUS" _
            & " , ''                                             AS LINEORDER" _
            & " , ''                                             AS MODEL" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS STACKINGFLG" _
            & " , ''                                             AS ACTUALLODDATE" _
            & " , ''                                             AS JOINTCODE" _
            & " , ''                                             AS JOINT" _
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
            & " , ''                                             AS CHANGERETSTATION" _
            & " , ''                                             AS CHANGERETSTATIONNAME" _
            & " , ''                                             AS USEORDERNO" _
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
                & " , ISNULL(RTRIM(OIT0003.SHIPORDER), '')               AS SHIPORDER" _
                & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                  AS TANKNO" _
                & " , ISNULL(RTRIM(OIT0005.TANKSTATUS), '')              AS TANKSTATUS" _
                & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')               AS LINEORDER" _
                & " , ISNULL(RTRIM(OIM0005.MODEL), '')                   AS MODEL" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
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
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
                & "   END                                                           AS JRALLINSPECTIONALERT" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P09" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN @P10" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN @P11" _
                & "   END                                                           AS JRALLINSPECTIONALERTSTR" _
                & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), NULL) AS JRALLINSPECTIONDATE" _
                & " , CASE ISNULL(RTRIM(OIT0003.STACKINGFLG), '')" _
                & "   WHEN '1' THEN 'on'" _
                & "   WHEN '2' THEN ''" _
                & "   ELSE ''" _
                & "   END                                                           AS STACKINGFLG" _
                & " , ISNULL(FORMAT(OIT0003.ACTUALLODDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALLODDATE" _
                & " , ISNULL(RTRIM(OIT0003.JOINTCODE), '')                          AS JOINTCODE" _
                & " , ISNULL(RTRIM(OIT0003.JOINT), '')                              AS JOINT" _
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
                & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATION), '')                   AS CHANGERETSTATION" _
                & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATIONNAME), '')               AS CHANGERETSTATIONNAME" _
                & " , ISNULL(RTRIM(OIT0005.USEORDERNO), '')                         AS USEORDERNO" _
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

            SQLStr &=
                  " ORDER BY" _
                & "    OIT0003.OILCODE"

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
                    '★ 作成した受注データ件数を取得
                    WW_ORDERCNT = OIT0003tbl.Rows.Count
                End Using

                Dim i As Integer = 0
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    i += 1
                    OIT0003row("LINECNT") = i        'LINECNT

                    '◯名称取得
                    '受注情報
                    If OIT0003row("ORDERINFONAME") = "" Then
                        CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)
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
            " SELECT DISTINCT " _
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
            & " , ISNULL(RTRIM(TMP0001.OILCODE), OIT0004.PREOILCODE)                 AS OILCODE" _
            & " , ISNULL(RTRIM(TMP0001.OILNAME), OIT0004.PREOILNAME)                 AS OILNAME" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINGTYPE), OIT0004.PREORDERINGTYPE)       AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINGOILNAME), OIT0004.PREORDERINGOILNAME) AS ORDERINGOILNAME" _
            & " , CASE" _
            & "   WHEN (OIT0004.TANKNUMBER IS NULL OR TMP0001.TANKNO IS NULL) " _
            & "    AND TMP0001.OILNAME IS NULL AND OIT0004.PREOILNAME IS NULL THEN @P04" _
            & "   WHEN (OIT0004.TANKNUMBER <> '' OR TMP0001.TANKNO <> '') " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN (OIT0004.TANKNUMBER <> '' OR TMP0001.TANKNO <> '') " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN (OIT0004.TANKNUMBER IS NOT NULL OR TMP0001.TANKNO IS NOT NULL) " _
            & "    AND TMP0001.OILCODE IS NULL AND OIT0004.PREOILCODE IS NULL THEN @P04" _
            & "   WHEN (OIT0004.TANKNUMBER IS NOT NULL OR TMP0001.TANKNO IS NOT NULL) " _
            & "    AND OIT0004.PREOILCODE IS NOT NULL THEN @P06" _
            & "   ELSE @P07" _
            & "   END                                                           AS TANKQUOTA" _
            & " , ISNULL(RTRIM(OIT0004.LINKNO), '')                             AS LINKNO" _
            & " , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')                       AS LINKDETAILNO" _
            & " , ISNULL(RTRIM(TMP0001.SHIPORDER), '')                          AS SHIPORDER" _
            & " , ISNULL(RTRIM(OIM0005.TANKNUMBER), '')                         AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0005.TANKSTATUS), '')                         AS TANKSTATUS" _
            & " , ISNULL(RTRIM(OIT0004.LINEORDER), TMP0001.LINEORDER)           AS LINEORDER" _
            & " , ISNULL(RTRIM(OIM0005.MODEL), '')                              AS MODEL" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
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
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
            & "   END                                                           AS JRALLINSPECTIONALERT" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P08" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN @P09" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN @P10" _
            & "   END                                                           AS JRALLINSPECTIONALERTSTR" _
            & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), NULL) AS JRALLINSPECTIONDATE" _
            & " , CASE ISNULL(RTRIM(TMP0001.STACKINGFLG), '')" _
            & "   WHEN '1' THEN 'on'" _
            & "   WHEN '2' THEN ''" _
            & "   ELSE ''" _
            & "   END                                                           AS STACKINGFLG" _
            & " , ISNULL(FORMAT(TMP0001.ACTUALLODDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALLODDATE" _
            & " , ISNULL(RTRIM(TMP0001.JOINTCODE), '')                          AS JOINTCODE" _
            & " , ISNULL(RTRIM(TMP0001.JOINT), '')                              AS JOINT" _
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
            & " , ISNULL(RTRIM(TMP0001.CHANGERETSTATION), '')                   AS CHANGERETSTATION" _
            & " , ISNULL(RTRIM(TMP0001.CHANGERETSTATIONNAME), '')               AS CHANGERETSTATIONNAME" _
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
            & " , ISNULL(RTRIM(TMP0001.OILCODE), OIT0004.PREOILCODE)                 AS OILCODE" _
            & " , ISNULL(RTRIM(TMP0001.OILNAME), OIT0004.PREOILNAME)                 AS OILNAME" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINGTYPE), OIT0004.PREORDERINGTYPE)       AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(TMP0001.ORDERINGOILNAME), OIT0004.PREORDERINGOILNAME) AS ORDERINGOILNAME" _
            & " , CASE" _
            & "   WHEN (OIT0004.TANKNUMBER IS NULL OR TMP0001.TANKNO IS NULL) " _
            & "    AND TMP0001.OILNAME IS NULL AND OIT0004.PREOILNAME IS NULL THEN @P04" _
            & "   WHEN (OIT0004.TANKNUMBER <> '' OR TMP0001.TANKNO <> '') " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN (OIT0004.TANKNUMBER <> '' OR TMP0001.TANKNO <> '') " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P05" _
            & "   WHEN (OIT0004.TANKNUMBER IS NOT NULL OR TMP0001.TANKNO IS NOT NULL) " _
            & "    AND TMP0001.OILCODE IS NULL AND OIT0004.PREOILCODE IS NULL THEN @P04" _
            & "   WHEN (OIT0004.TANKNUMBER IS NOT NULL OR TMP0001.TANKNO IS NOT NULL) " _
            & "    AND TMP0001.OILCODE IS NULL AND OIT0004.PREOILCODE IS NOT NULL THEN @P07" _
            & "   WHEN (OIT0004.TANKNUMBER IS NOT NULL OR TMP0001.TANKNO IS NOT NULL) " _
            & "    AND OIT0004.PREOILCODE IS NOT NULL THEN @P06" _
            & "   ELSE @P07" _
            & "   END                                                           AS TANKQUOTA" _
            & " , ISNULL(RTRIM(OIT0004.LINKNO), '')                             AS LINKNO" _
            & " , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')                       AS LINKDETAILNO" _
            & " , ISNULL(RTRIM(TMP0001.SHIPORDER), '')                          AS SHIPORDER" _
            & " , ISNULL(RTRIM(OIM0005.TANKNUMBER), '')                         AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0005.TANKSTATUS), '')                         AS TANKSTATUS" _
            & " , ISNULL(RTRIM(OIT0004.LINEORDER), TMP0001.LINEORDER)           AS LINEORDER" _
            & " , ISNULL(RTRIM(OIM0005.MODEL), '')                              AS MODEL" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
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
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
            & "   END                                                           AS JRALLINSPECTIONALERT" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P08" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN @P09" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN @P10" _
            & "   END                                                           AS JRALLINSPECTIONALERTSTR" _
            & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), NULL) AS JRALLINSPECTIONDATE" _
            & " , CASE ISNULL(RTRIM(TMP0001.STACKINGFLG), '')" _
            & "   WHEN '1' THEN 'on'" _
            & "   WHEN '2' THEN ''" _
            & "   ELSE ''" _
            & "   END                                                           AS STACKINGFLG" _
            & " , ISNULL(FORMAT(TMP0001.ACTUALLODDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALLODDATE" _
            & " , ISNULL(RTRIM(TMP0001.JOINTCODE), '')                          AS JOINTCODE" _
            & " , ISNULL(RTRIM(TMP0001.JOINT), '')                              AS JOINT" _
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
            & " , ISNULL(RTRIM(TMP0001.CHANGERETSTATION), '')                   AS CHANGERETSTATION" _
            & " , ISNULL(RTRIM(TMP0001.CHANGERETSTATIONNAME), '')               AS CHANGERETSTATIONNAME" _
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

        SQLStr &=
              " ORDER BY" _
            & "    ISNULL(RTRIM(TMP0001.OILCODE), OIT0004.PREOILCODE)"

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

                    '★受注データが0件の場合(貨車連結順序表のみ選択)
                    If WW_ORDERCNT = 0 Then
                        '"割当"を設定する
                        OIT0003row("TANKQUOTA") = CONST_TANKNO_STATUS_WARI
                    End If

                Next

                'KEY重複回避処理
                Dim strDETAILNO_BEFORE As String = "000"
                For Each OIT0003row As DataRow In OIT0003tbl.Rows

                    '1つ前と今回の受注明細№が同じか比較
                    If strDETAILNO_BEFORE = OIT0003row("DETAILNO") Then
                        intDETAILNO += 1
                        OIT0003row("DETAILNO") = intDETAILNO.ToString("000")
                    End If
                    strDETAILNO_BEFORE = OIT0003row("DETAILNO")
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
            & "    RIGHT('00' + OIT0003.LINEORDER, 2)"

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
                & " , ISNULL(RTRIM(OIT0003.SHIPORDER), '')               AS SHIPORDER" _
                & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')               AS LINEORDER" _
                & " , ISNULL(RTRIM(OIT0003.OILCODE), '')                 AS OILCODE" _
                & " , ISNULL(RTRIM(OIT0003.OILNAME), '')                 AS OILNAME" _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')            AS ORDERINGTYPE" _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '')         AS ORDERINGOILNAME" _
                & " , ISNULL(RTRIM(OIM0005.MODEL), '')                   AS MODEL" _
                & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                  AS TANKNO" _
                & " , ISNULL(RTRIM(OIT0005.TANKSTATUS), '')              AS TANKSTATUS" _
                & " , ISNULL(RTRIM(OIT0005.LOADINGKBN), '')              AS LOADINGKBN" _
                & " , ISNULL(RTRIM(OIT0005.TANKSITUATION), '')           AS TANKSITUATION" _
                & " , ISNULL(RTRIM(OIT0005.USEORDERNO), '')              AS USEORDERNO" _
                & " , ISNULL(RTRIM(OIT0003.STACKINGORDERNO), '')         AS STACKINGORDERNO" _
                & " , CASE ISNULL(RTRIM(OIT0003.STACKINGFLG), '')" _
                & "   WHEN '1' THEN 'on'" _
                & "   WHEN '2' THEN ''" _
                & "   ELSE ''" _
                & "   END                                                AS STACKINGFLG" _
                & " , CASE ISNULL(RTRIM(OIT0003.FIRSTRETURNFLG), '')" _
                & "   WHEN '1' THEN 'on'" _
                & "   WHEN '2' THEN ''" _
                & "   ELSE ''" _
                & "   END                                                AS FIRSTRETURNFLG" _
                & " , CASE ISNULL(RTRIM(OIT0003.AFTERRETURNFLG), '')" _
                & "   WHEN '1' THEN 'on'" _
                & "   WHEN '2' THEN ''" _
                & "   ELSE ''" _
                & "   END                                                AS AFTERRETURNFLG" _
                & " , CASE ISNULL(RTRIM(OIT0003.OTTRANSPORTFLG), '')" _
                & "   WHEN '1' THEN 'on'" _
                & "   WHEN '2' THEN ''" _
                & "   ELSE ''" _
                & "   END                                                AS OTTRANSPORTFLG" _
                & " , ISNULL(RTRIM(OIT0002.TANKLINKNO), '')              AS LINKNO" _
                & " , ''                                                 AS LINKDETAILNO" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
                & "   END                                                           AS JRINSPECTIONALERT" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN '" + C_INSPECTIONALERT.ALERT_RED + "'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN '" + C_INSPECTIONALERT.ALERT_YELLOW + "'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN '" + C_INSPECTIONALERT.ALERT_GREEN + "'" _
                & "   END                                                           AS JRINSPECTIONALERTSTR" _
                & " , ISNULL(FORMAT(OIM0005.JRINSPECTIONDATE, 'yyyy/MM/dd'), NULL)  AS JRINSPECTIONDATE" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
                & "   END                                                           AS JRALLINSPECTIONALERT" _
                & " , CASE" _
                & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN '" + C_INSPECTIONALERT.ALERT_RED + "'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN '" + C_INSPECTIONALERT.ALERT_YELLOW + "'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN '" + C_INSPECTIONALERT.ALERT_GREEN + "'" _
                & "   END                                                           AS JRALLINSPECTIONALERTSTR" _
                & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), NULL) AS JRALLINSPECTIONDATE" _
                & " , ISNULL(RTRIM(OIT0003.CARSAMOUNT), '')                         AS CARSAMOUNT" _
                & " , ISNULL(RTRIM(OIT0003.JOINTCODE), '')                          AS JOINTCODE" _
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
                & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATION), '')                   AS CHANGERETSTATION" _
                & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATIONNAME), '')               AS CHANGERETSTATIONNAME" _
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
                & " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
                & "       OIT0003.TANKNO = OIT0005.TANKNUMBER" _
                & "       AND OIT0005.DELFLG <> @P02" _
                & " WHERE OIT0002.ORDERNO = @P01" _
                & " AND OIT0002.DELFLG <> @P02"

        SQLStr &=
              " ORDER BY" _
            & "    RIGHT('00' + OIT0003.LINEORDER, 2)"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                'Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20) '赤丸
                'Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20) '黄丸
                'Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '緑丸

                'PARA00.Value = O_INSCNT
                PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE
                'PARA03.Value = C_INSPECTIONALERT.ALERT_RED
                'PARA04.Value = C_INSPECTIONALERT.ALERT_YELLOW
                'PARA05.Value = C_INSPECTIONALERT.ALERT_GREEN

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

                    '### 20200717 START((全体)No112対応) ######################################
                    '★輸送形態が"M"(請負OT混載)ではない場合
                    If Me.TxtOrderTrkKbn.Text <> BaseDllConst.CONST_TRKBN_M Then
                        'OT輸送可否フラグをすべて未チェックに変更
                        OIT0003tab3row("OTTRANSPORTFLG") = ""
                    End If
                    '### 20200717 END  ((全体)No112対応) ######################################
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

#Region "タブ「費用入力」一覧表示用"
    ''' <summary>
    ''' 画面表示データ取得(タブ「費用入力」一覧表示用)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGetTab4(ByVal SQLcon As SqlConnection)
        If IsNothing(OIT0003tbl_tab4) Then
            OIT0003tbl_tab4 = New DataTable
        End If

        If OIT0003tbl_tab4.Columns.Count <> 0 Then
            OIT0003tbl_tab4.Columns.Clear()
        End If

        OIT0003tbl_tab4.Clear()

        '○ 一覧表示用検索SQL
        '　一覧説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
                  " SELECT" _
                & "   0                                                  AS LINECNT" _
                & " , ''                                                 AS OPERATION" _
                & " , '0'                                                AS TIMSTP" _
                & " , 1                                                  AS 'SELECT'" _
                & " , 0                                                  AS HIDDEN" _
                & " , @P01                                               AS BILLINGNO" _
                & " , ''                                                 AS BILLINGDETAILNO" _
                & " , TMP0002.ORDERNO                                    AS ORDERNO" _
                & " , TMP0002.SHIPPERSCODE                               AS SHIPPERSCODE" _
                & " , TMP0002.SHIPPERSNAME                               AS SHIPPERSNAME" _
                & " , TMP0002.BASECODE                                   AS BASECODE" _
                & " , TMP0002.BASENAME                                   AS BASENAME" _
                & " , TMP0002.OFFICECODE                                 AS OFFICECODE" _
                & " , TMP0002.OFFICENAME                                 AS OFFICENAME" _
                & " , TMP0002.CONSIGNEECODE                              AS CONSIGNEECODE" _
                & " , TMP0002.CONSIGNEENAME                              AS CONSIGNEENAME" _
                & " , ''                                                 AS ORDERINFO" _
                & " , ''                                                 AS ORDERINFONAME" _
                & " , '1'                                                AS CALCACCOUNT" _
                & " , '1'                                                AS AKAKURO" _
                & " , FORMAT(TMP0002.KEIJYOYMD, 'yyyy/MM')               AS KEIJYOYM" _
                & " , TMP0002.KEIJYOYMD                                  AS KEIJYOYMD" _
                & " , TMP0002.ACCOUNTCODE + '　' + TMP0002.SEGMENTCODE   AS ACCSEGCODE" _
                & " , TMP0002.ACCOUNTNAME + '　' + TMP0002.SEGMENTNAME   AS ACCSEGNAME" _
                & " , TMP0002.ACCOUNTCODE                                AS ACCOUNTCODE" _
                & " , TMP0002.ACCOUNTNAME                                AS ACCOUNTNAME" _
                & " , TMP0002.SEGMENTCODE                                AS SEGMENTCODE" _
                & " , TMP0002.SEGMENTNAME                                AS SEGMENTNAME" _
                & " , TMP0002.BREAKDOWNCODE                              AS BREAKDOWNCODE" _
                & " , TMP0002.BREAKDOWN                                  AS BREAKDOWN" _
                & " , TMP0002.CALCKBN                                    AS CALCKBN" _
                & " , TMP0002.CALCKBNNAME                                AS CALCKBNNAME" _
                & " , REPLACE(CONVERT(VARCHAR,CAST(ROUND(CASE " _
                & "   WHEN TMP0002.CALCKBN = '1' THEN " _
                & "        SUM(TMP0002.CARSNUMBER) " _
                & "   WHEN TMP0002.CALCKBN = '2' THEN " _
                & "        SUM(TMP0002.CARSAMOUNT) " _
                & "   WHEN TMP0002.CALCKBN = '3' THEN " _
                & "        SUM(TMP0002.LOAD) " _
                & "   END, 3) AS MONEY), 1),'.00' , '') CARSAMOUNT" _
                & " , ''                                                 AS CARSAMOUNTNAME" _
                & " , '￥' " _
                & "  + REPLACE ( " _
                & "   CONVERT(VARCHAR, CAST(TMP0002.APPLYCHARGE AS MONEY), 1) " _
                & "   , '.00', '')                                       AS APPLYCHARGE" _
                & " , '￥' + REPLACE(CONVERT(VARCHAR,CAST(ROUND(CASE " _
                & "   WHEN TMP0002.CALCKBN = '1' THEN " _
                & "        SUM(TMP0002.CARSNUMBER * TMP0002.APPLYCHARGE) " _
                & "   WHEN TMP0002.CALCKBN = '2' THEN " _
                & "        SUM(TMP0002.CARSAMOUNT * TMP0002.APPLYCHARGE) " _
                & "   WHEN TMP0002.CALCKBN = '3' THEN " _
                & "        SUM(TMP0002.LOAD * TMP0002.APPLYCHARGE) " _
                & "   END, 3) AS MONEY), 1),'.00' , '') APPLYCHARGESUM" _
                & " , '￥' + REPLACE(CONVERT(VARCHAR,CAST(ROUND(CASE " _
                & "   WHEN TMP0002.CALCKBN = '1' THEN " _
                & "        SUM(TMP0002.CARSNUMBER * (TMP0002.APPLYCHARGE * @P02)) " _
                & "   WHEN TMP0002.CALCKBN = '2' THEN " _
                & "        SUM(TMP0002.CARSAMOUNT * (TMP0002.APPLYCHARGE * @P02)) " _
                & "   WHEN TMP0002.CALCKBN = '3' THEN " _
                & "        SUM(TMP0002.LOAD * (TMP0002.APPLYCHARGE * @P02)) " _
                & "   END, 3) AS MONEY), 1),'.00' , '') CONSUMPTIONTAX" _
                & " , TMP0002.INVOICECODE                                AS INVOICECODE" _
                & " , TMP0002.INVOICENAME                                AS INVOICENAME" _
                & " , TMP0002.INVOICEDEPTNAME                            AS INVOICEDEPTNAME" _
                & " , TMP0002.PAYEECODE                                  AS PAYEECODE" _
                & " , TMP0002.PAYEENAME                                  AS PAYEENAME" _
                & " , TMP0002.PAYEEDEPTNAME                              AS PAYEEDEPTNAME" _
                & " FROM OIL.TMP0002RATE TMP0002 "

        SQLStr &=
                " GROUP BY TMP0002.ORDERNO, TMP0002.SHIPPERSCODE, TMP0002.SHIPPERSNAME" _
              & " , TMP0002.BASECODE, TMP0002.BASENAME, TMP0002.OFFICECODE, TMP0002.OFFICENAME" _
              & " , TMP0002.CONSIGNEECODE, TMP0002.CONSIGNEENAME, TMP0002.KEIJYOYMD" _
              & " , TMP0002.ACCOUNTCODE, TMP0002.ACCOUNTNAME, TMP0002.SEGMENTCODE, TMP0002.SEGMENTNAME" _
              & " , TMP0002.BREAKDOWNCODE, TMP0002.BREAKDOWN, TMP0002.CALCKBN, TMP0002.CALCKBNNAME, TMP0002.APPLYCHARGE" _
              & " , TMP0002.INVOICECODE, TMP0002.INVOICENAME, TMP0002.INVOICEDEPTNAME" _
              & " , TMP0002.PAYEECODE, TMP0002.PAYEENAME, TMP0002.PAYEEDEPTNAME"

        SQLStr &=
              " ORDER BY TMP0002.ACCOUNTCODE, TMP0002.SEGMENTCODE"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '支払請求№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Decimal) '消費税

                PARA01.Value = work.WF_SEL_BILLINGNO.Text
                PARA02.Value = work.WF_SEL_CONSUMPTIONTAX.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003tbl_tab4.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl_tab4.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003tab4row As DataRow In OIT0003tbl_tab4.Rows
                    i += 1

                    'LINECNT
                    OIT0003tab4row("LINECNT") = i
                    '数量(単位を後ろに付ける)
                    OIT0003tab4row("CARSAMOUNTNAME") = OIT0003tab4row("CARSAMOUNT").ToString() + OIT0003tab4row("CALCKBNNAME") + " "

                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB4 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB4 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 画面表示データ取得(タブ「費用入力」一覧表示用(追加項目用))
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataADDTab4(ByVal SQLcon As SqlConnection)

        '○ 一覧表示用検索SQL
        '　一覧説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
                  " SELECT" _
                & "   0                                                  AS LINECNT" _
                & " , ''                                                 AS OPERATION" _
                & " , '0'                                                AS TIMSTP" _
                & " , 1                                                  AS 'SELECT'" _
                & " , 0                                                  AS HIDDEN" _
                & " , OIT0010.BILLINGNO                                  AS BILLINGNO" _
                & " , OIT0010.BILLINGDETAILNO                            AS BILLINGDETAILNO" _
                & " , OIT0010.ORDERNO                                    AS ORDERNO" _
                & " , ''                                                 AS SHIPPERSCODE" _
                & " , ''                                                 AS SHIPPERSNAME" _
                & " , ''                                                 AS BASECODE" _
                & " , ''                                                 AS BASENAME" _
                & " , ''                                                 AS OFFICECODE" _
                & " , ''                                                 AS OFFICENAME" _
                & " , ''                                                 AS CONSIGNEECODE" _
                & " , ''                                                 AS CONSIGNEENAME" _
                & " , ''                                                 AS ORDERINFO" _
                & " , ''                                                 AS ORDERINFONAME" _
                & " , OIT0010.CALCACCOUNT                                AS CALCACCOUNT" _
                & " , OIT0010.AKAKURO                                    AS AKAKURO" _
                & " , FORMAT(OIT0010.KEIJYOYMD, 'yyyy/MM')               AS KEIJYOYM" _
                & " , OIT0010.KEIJYOYMD                                  AS KEIJYOYMD" _
                & " , OIT0010.ACCOUNTCODE + '　' + OIT0010.SEGMENTCODE   AS ACCSEGCODE" _
                & " , OIT0010.ACCOUNTNAME + '　' + OIT0010.SEGMENTNAME   AS ACCSEGNAME" _
                & " , OIT0010.ACCOUNTCODE                                AS ACCOUNTCODE" _
                & " , OIT0010.ACCOUNTNAME                                AS ACCOUNTNAME" _
                & " , OIT0010.SEGMENTCODE                                AS SEGMENTCODE" _
                & " , OIT0010.SEGMENTNAME                                AS SEGMENTNAME" _
                & " , OIT0010.SEGMENTBRANCHCODE                          AS BREAKDOWNCODE" _
                & " , OIT0010.SEGMENTBRANCHNAME                          AS BREAKDOWN" _
                & " , OIT0010.ACCOUNTTYPE                                AS CALCKBN" _
                & " , OIT0010.ACCOUNTTYPENAME                            AS CALCKBNNAME" _
                & " , REPLACE ( " _
                & "   CONVERT(VARCHAR, CAST(ROUND(OIT0010.QUANTITY, 3) AS DECIMAL(12, 3)), 1) " _
                & "   , '.000', '')                                       AS CARSAMOUNT " _
                & " , ''                                                 AS CARSAMOUNTNAME" _
                & " , ''                                                 AS APPLYCHARGE" _
                & " , '￥'" _
                & "  + REPLACE( " _
                & "   CONVERT(VARCHAR, CAST(ROUND(OIT0010.AMOUNT, 3) AS MONEY), 1) " _
                & "   , '.00', '')                                       AS APPLYCHARGESUM" _
                & " , '￥' " _
                & "  + REPLACE ( " _
                & "   CONVERT(VARCHAR, CAST(ROUND(OIT0010.TAX, 3) AS MONEY), 1) " _
                & "   , '.00', '')                                       AS CONSUMPTIONTAX " _
                & " , OIT0010.INVOICECODE                                AS INVOICECODE" _
                & " , OIT0010.INVOICENAME                                AS INVOICENAME" _
                & " , OIT0010.INVOICEDEPTNAME                            AS INVOICEDEPTNAME" _
                & " , OIT0010.PAYEECODE                                  AS PAYEECODE" _
                & " , OIT0010.PAYEENAME                                  AS PAYEENAME" _
                & " , OIT0010.PAYEEDEPTNAME                              AS PAYEEDEPTNAME" _
                & " FROM OIL.OIT0010_ORDERBILLING OIT0010 " _
                & " WHERE " _
                & "     OIT0010.BILLINGNO   = @P01 " _
                & " AND OIT0010.CALCACCOUNT = '2' " _
                & " AND OIT0010.DELFLG      <> @P02;"

        '### 単価の表示部分を念のためコメント
        '& " , '￥' " _
        '& "  + REPLACE ( " _
        '& "   CONVERT(VARCHAR, CAST(ROUND(OIT0010.UNITPRICE, 3) AS MONEY), 1) " _
        '& "   , '.00', '')                                       AS APPLYCHARGE " _

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '支払請求№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA01.Value = work.WF_SEL_BILLINGNO.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl_tab4.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003tab4row As DataRow In OIT0003tbl_tab4.Rows

                    If OIT0003tab4row("LINECNT").ToString() <> "0" Then
                        i = OIT0003tab4row("LINECNT")
                    Else
                        i += 1
                        OIT0003tab4row("LINECNT") = i   'LINECNT
                    End If
                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB4 ADDSELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB4 AddSelect"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub
#End Region

    ''' <summary>
    ''' 各タブ(一覧)の再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ReDisplayTabList()

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
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPTAB1TBL.Text)

        '○ 画面表示データ再取得(タブ「入換・積込」表示データ取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab2(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

        '○ 画面表示データ再取得(タブ「タンク車明細」表示データ取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab3(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        '◎ タブ「費用入力」画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '勘定科目明細作成
            WW_InsertRequestAccountDetail(SQLcon)

            '費用入力一覧(勘定科目サマリー作成)
            MAPDataGetTab4(SQLcon)

            '費用入力一覧(勘定科目追加項目作成)
            MAPDataADDTab4(SQLcon)

        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

    End Sub

#Region "一覧再表示処理"
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

            '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
            If work.WF_SEL_ORDERSTATUS.Text <= BaseDllConst.CONST_ORDERSTATUS_450 Then
                '◯受注№存在チェック
                WW_OrderNoExistChk()
            End If
            '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################

            DisplayGrid_TAB3()

            '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
            '### 20200626 タブ「タンク車明細」を表示したタイミング ###########################################
            If Me.WW_USEORDERFLG = True AndAlso Me.WW_InitializeTAB3 = True Then
                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 _
                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 Then
                    '### 受注進行ステータスが検収中以降の場合は、メッセージを出力しない（何もしない) #########
                Else
                    Master.Output(C_MESSAGE_NO.OIL_ORDERNO_WAR_MESSAGE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                End If
            End If
            '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            Master.RecoverTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

            DisplayGrid_TAB4()

        End If

        '〇 画面表示設定処理
        WW_ScreenEnabledSet()

        '〇タンク車所在の更新
        WW_TankShozaiSet()

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

        '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
        For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
            '指定タンク車№が他の受注オーダーで使用中の場合は、(実績)日付を許可しない。
            If OIT0003tab3row("USEORDERNO") = "" Then
                Continue For
            ElseIf OIT0003tab3row("USEORDERNO") <> Me.TxtOrderNo.Text Then
                '使用受注オーダーを使用中に変更
                Me.WW_USEORDERFLG = True
            End If
        Next
        '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################

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
#End Region

    ''' <summary>
    ''' 手配連絡ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCONTACT_Click()

        Dim strOrderStatus As String = ""

        '手配連絡フラグを"1"(連絡)にする。
        work.WF_SEL_CONTACTFLG.Text = "1"

        '受注TBL更新
        WW_UpdateRelatedFlg("1", "CONTACTFLG")

        '〇 受注進行ステータスの状態を取得
        WW_ScreenOrderStatusSet(strOrderStatus)

        '〇 受注進行ステータスの変更分を反映
        WW_ScreenOrderStatusChgRef(strOrderStatus)

    End Sub

    ''' <summary>
    ''' 結果受理ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonRESULT_Click()

        Dim strOrderStatus As String = ""

        '結果受理フラグを"1"(結果受理)にする。
        work.WF_SEL_RESULTFLG.Text = "1"

        '受注TBL更新
        WW_UpdateRelatedFlg("1", "RESULTFLG")

        '〇 受注進行ステータスの状態を取得
        WW_ScreenOrderStatusSet(strOrderStatus)

        '〇 受注進行ステータスの変更分を反映
        WW_ScreenOrderStatusChgRef(strOrderStatus)

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
        WW_UpdateRelatedFlg("1", "DELIVERYFLG")

        '〇 受注進行ステータスの状態を取得
        WW_ScreenOrderStatusSet(strOrderStatus)

        '〇 受注進行ステータスの変更分を反映
        WW_ScreenOrderStatusChgRef(strOrderStatus)

        '★ 固定帳票(貨物運送状)
        Select Case Me.TxtOrderOfficeCode.Text
            '◯　四日市営業所, 三重塩浜営業所
            Case BaseDllConst.CONST_OFFICECODE_012401,
                 BaseDllConst.CONST_OFFICECODE_012402

                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelDeliveryDataGet(SQLcon, Me.TxtOrderOfficeCode.Text, lodDate:=Me.TxtLoadingDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_DELIVERYPLAN.xlsx", OIT0003ReportDeliverytbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintYokkaichiData("DELIVERYPLAN", Me.TxtLoadingDate.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

        End Select


    End Sub

    ''' <summary>
    ''' 帳票表示(託送指示)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelDeliveryDataGet(ByVal SQLcon As SqlConnection,
                                       ByVal officeCode As String,
                                       Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003ReportDeliverytbl) Then
            OIT0003ReportDeliverytbl = New DataTable
        End If

        If OIT0003ReportDeliverytbl.Columns.Count <> 0 Then
            OIT0003ReportDeliverytbl.Columns.Clear()
        End If

        OIT0003ReportDeliverytbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
        " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , ''                                             AS No" _
            & " , ''                                             AS AGREEMENTCODE" _
            & " , ''                                             AS DISCOUNTCODE" _
            & " , ''                                             AS ITEMCODE" _
            & " , ''                                             AS MODELCODE" _
            & " , OIT0002.OFFICECODE                             AS OFFICECODE" _
            & " , OIT0002.OFFICENAME                             AS OFFICENAME" _
            & " , OIT0003.SHIPPERSCODE                           AS SHIPPERSCODE" _
            & " , OIT0003.SHIPPERSNAME                           AS SHIPPERSNAME" _
            & " , OIT0002.BASECODE                               AS BASECODE" _
            & " , OIT0002.BASENAME                               AS BASENAME" _
            & " , OIT0002.CONSIGNEECODE                          AS CONSIGNEECODE" _
            & " , OIT0002.CONSIGNEENAME                          AS CONSIGNEENAME" _
            & " , OIT0002.TRAINNO                                AS TRAINNO" _
            & " , OIT0002.TRAINNAME                              AS TRAINNAME" _
            & " , OIT0002.DEPSTATION                             AS DEPSTATION" _
            & " , OIT0002.DEPSTATIONNAME                         AS DEPSTATIONNAME" _
            & " , OIT0002.ARRSTATION                             AS ARRSTATION" _
            & " , OIT0002.ARRSTATIONNAME                         AS ARRSTATIONNAME" _
            & " , OIT0003.TANKNO                                 AS TANKNO" _
            & " , ''                                             AS TRANSPORTLETTER" _
            & " , ''                                             AS ASSEMBLENO" _
            & " , ''                                             AS FARE" _
            & " , ''                                             AS RECEIPTSTAMP" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " WHERE OIT0002.LODDATE = @P03 " _
            & " AND OIT0002.OFFICECODE = @P01 " _
            & " AND OIT0002.DELFLG <> @P02 " _
            & " AND OIT0002.ORDERNO = @P04 "

        'SQLStr &=
        '        " ORDER BY" _
        '    & "    OIT0002.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 11) '受注No
                PARA01.Value = officeCode
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                PARA04.Value = Me.TxtOrderNo.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportDeliverytbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportDeliverytbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003ReportDeliverytbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D DELIVERY EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D DELIVERY EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportYokkaichitbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 油種数登録ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '着駅コードが未設定の場合
        '※一覧を作成するにあたり、基地コード・荷受人を取得するために、
        '　着駅コードは必須となるため
        If Me.TxtArrstationCode.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "着駅", needsPopUp:=True)
            Me.TxtArrstationCode.Focus()
            WW_CheckERR("着駅入力エラー。", C_MESSAGE_NO.PREREQUISITE_ERROR)
            Exit Sub
        End If

        '### 20200512-START 油種数登録制御追加 ###################################
        '★油種数の""(空文字)チェック
        If Me.TxtHTank_w.Text = "" Then Me.TxtHTank_w.Text = "0"        '車数（ハイオク）
        If Me.TxtRTank_w.Text = "" Then Me.TxtRTank_w.Text = "0"        '車数（レギュラー）
        If Me.TxtTTank_w.Text = "" Then Me.TxtTTank_w.Text = "0"        '車数（灯油）
        If Me.TxtMTTank_w.Text = "" Then Me.TxtMTTank_w.Text = "0"      '車数（未添加灯油）
        If Me.TxtKTank_w.Text = "" Then Me.TxtKTank_w.Text = "0"        '車数（軽油）
        If Me.TxtK3Tank_w.Text = "" Then Me.TxtK3Tank_w.Text = "0"      '車数（３号軽油）
        If Me.TxtK5Tank_w.Text = "" Then Me.TxtK5Tank_w.Text = "0"      '車数（５号軽油）
        If Me.TxtK10Tank_w.Text = "" Then Me.TxtK10Tank_w.Text = "0"    '車数（１０号軽油）
        If Me.TxtLTank_w.Text = "" Then Me.TxtLTank_w.Text = "0"        '車数（LSA）
        If Me.TxtATank_w.Text = "" Then Me.TxtATank_w.Text = "0"        '車数（A重油）

        'タンク車数の件数カウント用
        Dim intTankCnt As Integer = 0
        intTankCnt += Integer.Parse(Me.TxtHTank_w.Text)
        intTankCnt += Integer.Parse(Me.TxtRTank_w.Text)
        intTankCnt += Integer.Parse(Me.TxtTTank_w.Text)
        intTankCnt += Integer.Parse(Me.TxtMTTank_w.Text)
        intTankCnt += Integer.Parse(Me.TxtKTank_w.Text)
        intTankCnt += Integer.Parse(Me.TxtK3Tank_w.Text)
        intTankCnt += Integer.Parse(Me.TxtK5Tank_w.Text)
        intTankCnt += Integer.Parse(Me.TxtK10Tank_w.Text)
        intTankCnt += Integer.Parse(Me.TxtLTank_w.Text)
        intTankCnt += Integer.Parse(Me.TxtATank_w.Text)
        Me.TxtTotalCnt_w.Text = intTankCnt.ToString()

        '油種数が１つも入力されていない場合
        If Me.TxtTotalCnt_w.Text = "0" Then
            Master.Output(C_MESSAGE_NO.OIL_OILTANK_INPUT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Me.TxtHTank_w.Focus()

            '〇 油種数登録ボタンのチェックを無効(False)
            WF_ButtonInsertFLG.Value = "FALSE"

        Else
            '〇 油種数登録ボタンのチェックを有効(True)
            WF_ButtonInsertFLG.Value = "TRUE"

        End If
        '### 20200512-END   ######################################################

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            'MAPDataGet(SQLcon, CONST_DETAIL_NEWLIST)
            MAPDataGet(SQLcon, intTankCnt)
        End Using

        '### 20200512-START 油種数登録制御追加 ###################################
        '〇画面で設定された油種コードを取得
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        Dim arrTankCode(intTankCnt) As String
        Dim arrTankName(intTankCnt) As String
        Dim arrTankType(intTankCnt) As String
        Dim arrTankOrderName(intTankCnt) As String
        Dim z As Integer = 0

        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_HTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtHTank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_HTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_RTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtRTank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_RTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_TTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtTTank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_TTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_MTTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtMTTank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_MTTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_KTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtKTank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_KTank1
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_K3Tank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtK3Tank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_K3Tank1
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_K5Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtK5Tank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_K5Tank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_K10Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtK10Tank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_K10Tank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_LTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtLTank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_LTank1
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", BaseDllConst.CONST_ATank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtATank_w.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_ATank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        '### 20200512-END   ######################################################

        Dim j As Integer = 0
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            OIT0003row("OILCODE") = arrTankCode(j)              '油種コード
            OIT0003row("OILNAME") = arrTankName(j)              '油種名
            OIT0003row("ORDERINGTYPE") = arrTankType(j)         '油種区分(受発注用)
            OIT0003row("ORDERINGOILNAME") = arrTankOrderName(j) '油種名(受発注用)

            j += 1
            '◯列車マスタ(発送順区分)が対象(1:発送対象)の場合は値を設定
            '　※上記以外(2:発送対象外)については、入力しないため値は未入力。
            If work.WF_SEL_SHIPORDERCLASS.Text = "1" Then
                OIT0003row("SHIPORDER") = j    '発送順
            End If

            '◯袖ヶ浦営業所のみ貨物駅入線順の値を設定
            '　※上記以外の営業所については、入力しないため値は未入力。
            If Me.TxtOrderOfficeCode.Text = "011203" Then
                OIT0003row("LINEORDER") = j    '貨物駅入線順

            End If
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
        If WF_FIELD.Value <> "TxtOrderOffice" AndAlso Me.TxtOrderOffice.Text = "" Then
            Master.Output(C_MESSAGE_NO.OIL_ORDEROFFICE_UNSELECT, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Me.TxtArrstationCode.Focus()
            WW_CheckERR("受注営業所が未選択。", C_MESSAGE_NO.OIL_ORDEROFFICE_UNSELECT)
            WF_LeftboxOpen.Value = ""   'LeftBoxを表示させない
            Me.TxtOrderOffice.Focus()
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
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, Me.TxtOrderOffice.Text)
                            'Else
                            '    prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, TxtOrderOffice.Text)
                            'End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, Me.TxtOrderOffice.Text)
                        End If
                    End If
                    '########################################

                    '受注パターン
                    If WF_FIELD.Value = "TxtOrderType" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If Me.TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, Me.TxtOrderType.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, Me.TxtOrderType.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, Me.TxtOrderType.Text)
                        End If
                    End If

                    '荷主名
                    If WF_FIELD.Value = "TxtShippersCode" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If Me.TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, Me.TxtShippersCode.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, Me.TxtShippersCode.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, Me.TxtShippersCode.Text)
                        End If
                    End If

                    '荷受人名
                    If WF_FIELD.Value = "TxtConsigneeCode" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If Me.TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, Me.TxtConsigneeCode.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, Me.TxtConsigneeCode.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, Me.TxtConsigneeCode.Text)
                        End If
                    End If

                    '本線列車
                    If WF_FIELD.Value = "TxtTrainNo" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If Me.TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, Me.TxtTrainNo.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_ORDERSALESOFFICECODE.Text, Me.TxtTrainNo.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, Me.TxtTrainNo.Text)
                        End If
                    End If

                    '発駅
                    If WF_FIELD.Value = "TxtDepstationCode" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If Me.TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "1", Me.TxtDepstationCode.Text)
                            Else
                                prmData = work.CreateSTATIONPTParam(work.WF_SEL_ORDERSALESOFFICECODE.Text + "1", Me.TxtDepstationCode.Text)
                            End If
                        Else
                            prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "1", Me.TxtDepstationCode.Text)
                        End If
                    End If

                    '着駅
                    If WF_FIELD.Value = "TxtArrstationCode" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(受注営業所).テキストボックスが未設定
                            If Me.TxtOrderOffice.Text = "" Then
                                prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "2", Me.TxtArrstationCode.Text)
                            Else
                                prmData = work.CreateSTATIONPTParam(work.WF_SEL_ORDERSALESOFFICECODE.Text + "2", Me.TxtArrstationCode.Text)
                            End If
                        Else
                            prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "2", Me.TxtArrstationCode.Text)
                        End If
                    End If

                    '(一覧)荷主名, (一覧)油種, (一覧)タンク車№, 
                    '(一覧)入線列車番号, (一覧)出線列車番号, (一覧)回線, (一覧)充填ポイント
                    If WF_FIELD.Value = "SHIPPERSNAME" _
                        OrElse WF_FIELD.Value = "OILNAME" _
                        OrElse WF_FIELD.Value = "ORDERINGOILNAME" _
                        OrElse WF_FIELD.Value = "TANKNO" _
                        OrElse WF_FIELD.Value = "LOADINGIRILINETRAINNO" _
                        OrElse WF_FIELD.Value = "LOADINGOUTLETTRAINNO" _
                        OrElse WF_FIELD.Value = "LINE" _
                        OrElse WF_FIELD.Value = "FILLINGPOINT" Then
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

                        '(一覧)充填ポイント
                        If WF_FIELD.Value = "FILLINGPOINT" Then
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_BASECODE.Text, "")
                        End If

                        '### LeftBoxマルチ対応(20200217) START #####################################################
                        If WF_FIELD.Value = "TANKNO" _
                            OrElse WF_FIELD.Value = "FILLINGPOINT" Then

                            '↓暫定一覧対応 2020/02/13 グループ会社版を復活させ石油システムに合わない部分は直す
                            Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                            .SetTableList(enumVal, WW_DUMMY, prmData)
                            .ActiveTable()
                            Return
                            '↑暫定一覧対応 2020/02/13
                        End If
                        '### LeftBoxマルチ対応(20200217) END   #####################################################

                        '(一覧)ジョイント先
                    ElseIf WF_FIELD.Value = "JOINT" Then
                        '全表示のため設定をコメントにする。
                        'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")

                    End If

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        '(予定)積込日
                        Case "TxtLoadingDate"
                            .WF_Calendar.Text = Me.TxtLoadingDate.Text
                        '(予定)発日
                        Case "TxtDepDate"
                            .WF_Calendar.Text = Me.TxtDepDate.Text
                        '(予定)積車着日
                        Case "TxtArrDate"
                            .WF_Calendar.Text = Me.TxtArrDate.Text
                        '(予定)受入日
                        Case "TxtAccDate"
                            .WF_Calendar.Text = Me.TxtAccDate.Text
                        '(予定)空車着日
                        Case "TxtEmparrDate"
                            .WF_Calendar.Text = Me.TxtEmparrDate.Text
                        '(実績)積込日
                        Case "TxtActualLoadingDate"
                            .WF_Calendar.Text = Me.TxtActualLoadingDate.Text
                        '(実績)発日
                        Case "TxtActualDepDate"
                            .WF_Calendar.Text = Me.TxtActualDepDate.Text
                        '(実績)積車着日
                        Case "TxtActualArrDate"
                            .WF_Calendar.Text = Me.TxtActualArrDate.Text
                        '(実績)受入日
                        Case "TxtActualAccDate"
                            .WF_Calendar.Text = Me.TxtActualAccDate.Text
                        '(実績)空車着日
                        Case "TxtActualEmparrDate"
                            .WF_Calendar.Text = Me.TxtActualEmparrDate.Text

                        '(一覧)交検日
                        Case "JRINSPECTIONDATE"

                            '○ LINECNT取得
                            Dim WW_LINECNT As Integer = 0
                            If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                            '○ 対象ヘッダー取得
                            Dim updHeader = OIT0003tbl.AsEnumerable.
                                FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                            If IsNothing(updHeader) Then Exit Sub

                            .WF_Calendar.Text = updHeader.Item("JRINSPECTIONDATE")

                    End Select
                    .ActiveCalendar()

                End If
            End With

        End If
    End Sub

#Region "チェックボックス(選択)クリック処理"
    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click(ByVal chkFieldName As String)

        '〇 選択されたチェックボックスを制御
        Me.WF_CheckBoxFLG.Value = "TRUE"
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_CheckBoxSELECT_TAB1(chkFieldName)

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_CheckBoxSELECT_TAB2()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            WW_CheckBoxSELECT_TAB3(chkFieldName)

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            WW_CheckBoxSELECT_TAB4()

        End If

    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_CheckBoxSELECT_TAB1(ByVal chkFieldName As String)
        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        Select Case chkFieldName
            Case "WF_CheckBoxSELECTSTACKING"
                '◯ 受注営業所が"010402"(仙台新港営業所)以外の場合
                If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_010402 Then
                    Exit Select
                End If
                'チェックボックス判定
                For i As Integer = 0 To OIT0003tbl.Rows.Count - 1
                    If OIT0003tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If OIT0003tbl.Rows(i)("STACKINGFLG") = "on" Then
                            OIT0003tbl.Rows(i)("STACKINGFLG") = ""
                        Else
                            OIT0003tbl.Rows(i)("STACKINGFLG") = "on"
                        End If

                        '★チェックボックスのON,OFFチェック
                        If OIT0003tbl.Rows(i)("STACKINGFLG") = "on" Then
                            OIT0003tbl.Rows(i)("ACTUALLODDATE") = Date.Parse(Me.TxtDepDate.Text).AddDays(-1).ToString("yyyy/MM/dd")
                        Else
                            OIT0003tbl.Rows(i)("ACTUALLODDATE") = Me.TxtLoadingDate.Text
                        End If

                    End If
                Next

                '### 20200626 START (一覧)積置をチェックした場合の表示方法を変更 ###################
                '「一部積置」表示チェック
                '※積置ありの場合はそのまま表示(処理を抜ける)
                If chkOrderInfo.Checked = True Then Exit Select
                For i As Integer = 0 To OIT0003tbl.Rows.Count - 1
                    If OIT0003tbl.Rows(i)("STACKINGFLG") = "on" Then
                        '(一覧)積置が１つでもチェックされた場合は、表示を「一部積置」に切り替える。
                        chkOrderInfo.Visible = False        'チェックボックス(積置なし・積置あり)を非表示
                        chkOrderDetailInfo.Visible = True   'チェックボックス(一部積置)表示
                        Exit For
                    Else
                        chkOrderInfo.Visible = True         'チェックボックス(積置なし・積置あり)を表示
                        chkOrderDetailInfo.Visible = False  'チェックボックス(一部積置)非表示
                    End If
                Next
                '### 20200626 END   (一覧)積置をチェックした場合の表示方法を変更 ###################

            Case Else
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
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)
    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理(タブ「入換・積込指示」)
    ''' </summary>
    Protected Sub WW_CheckBoxSELECT_TAB2()

    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理(タブ「タンク車明細」)
    ''' </summary>
    Protected Sub WW_CheckBoxSELECT_TAB3(ByVal chkFieldName As String)

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        Select Case chkFieldName
            Case "WF_CheckBoxSELECTSTACKING"
                '◯ 受注営業所が"010402"(仙台新港営業所)以外の場合
                If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_010402 Then
                    Exit Select
                End If
                'チェックボックス判定
                For i As Integer = 0 To OIT0003tbl_tab3.Rows.Count - 1
                    If OIT0003tbl_tab3.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If OIT0003tbl_tab3.Rows(i)("STACKINGFLG") = "on" Then
                            OIT0003tbl_tab3.Rows(i)("STACKINGFLG") = ""
                        Else
                            OIT0003tbl_tab3.Rows(i)("STACKINGFLG") = "on"
                        End If

                        '★チェックボックスのON,OFFチェック
                        If OIT0003tbl_tab3.Rows(i)("STACKINGFLG") = "on" Then
                            OIT0003tbl_tab3.Rows(i)("ACTUALLODDATE") = Date.Parse(Me.TxtDepDate.Text).AddDays(-1).ToString("yyyy/MM/dd")
                        Else
                            OIT0003tbl_tab3.Rows(i)("ACTUALLODDATE") = Me.TxtLoadingDate.Text
                        End If

                    End If
                Next

                '### 20200626 START (一覧)積置をチェックした場合の表示方法を変更 ###################
                '「一部積置」表示チェック
                '※積置ありの場合はそのまま表示(処理を抜ける)
                If chkOrderInfo.Checked = True Then Exit Select
                For i As Integer = 0 To OIT0003tbl_tab3.Rows.Count - 1
                    If OIT0003tbl_tab3.Rows(i)("STACKINGFLG") = "on" Then
                        '(一覧)積置が１つでもチェックされた場合は、表示を「一部積置」に切り替える。
                        chkOrderInfo.Visible = False        'チェックボックス(積置なし・積置あり)を非表示
                        chkOrderDetailInfo.Visible = True   'チェックボックス(一部積置)表示
                        Exit For
                    Else
                        chkOrderInfo.Visible = True         'チェックボックス(積置なし・積置あり)を表示
                        chkOrderDetailInfo.Visible = False  'チェックボックス(一部積置)非表示
                    End If
                Next
                '### 20200626 END   (一覧)積置をチェックした場合の表示方法を変更 ###################
            Case "WF_CheckBoxSELECTFIRSTRETURN"
                '◯ 受注営業所が"011402"(根岸営業所)以外の場合
                If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_011402 _
                    OrElse (Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011402 _
                            AndAlso Me.TxtTrainNo.Text <> "81") Then
                    Exit Select
                End If
                'チェックボックス判定
                For i As Integer = 0 To OIT0003tbl_tab3.Rows.Count - 1
                    If OIT0003tbl_tab3.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If OIT0003tbl_tab3.Rows(i)("FIRSTRETURNFLG") = "on" Then
                            OIT0003tbl_tab3.Rows(i)("FIRSTRETURNFLG") = ""
                        Else
                            '### 20200702 START 指摘票対応(全体(No97))  ##################
                            'OIT0003tbl_tab3.Rows(i)("FIRSTRETURNFLG") = "on"
                            If OIT0003tbl_tab3.Rows(i)("AFTERRETURNFLG") = "on" Then
                                '★後返し(チェックボックス)にチェック有の場合
                                '　先返し(チェックボックス)にはチェックを未許可とする。
                            Else
                                OIT0003tbl_tab3.Rows(i)("FIRSTRETURNFLG") = "on"
                            End If
                            '### 20200702 END   指摘票対応(全体(No97))  ##################
                        End If
                    End If
                Next

            '    ### 20200622 START((全体)No87対応) ######################################
            Case "WF_CheckBoxSELECTAFTERRETURN"
                '◯ 受注営業所が"011402"(根岸営業所)以外の場合
                If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_011402 _
                    OrElse (Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011402 _
                            AndAlso Not (Me.TxtTrainNo.Text = "83" OrElse Me.TxtTrainNo.Text = "81")) Then
                    Exit Select
                End If
                'チェックボックス判定
                For i As Integer = 0 To OIT0003tbl_tab3.Rows.Count - 1
                    If OIT0003tbl_tab3.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If OIT0003tbl_tab3.Rows(i)("AFTERRETURNFLG") = "on" Then
                            OIT0003tbl_tab3.Rows(i)("AFTERRETURNFLG") = ""
                        Else
                            '### 20200702 START 指摘票対応(全体(No97))  ##################
                            'OIT0003tbl_tab3.Rows(i)("AFTERRETURNFLG") = "on"
                            If OIT0003tbl_tab3.Rows(i)("FIRSTRETURNFLG") = "on" Then
                                '★先返し(チェックボックス)にチェック有の場合
                                '　後返し(チェックボックス)にはチェックを未許可とする。
                            Else
                                OIT0003tbl_tab3.Rows(i)("AFTERRETURNFLG") = "on"
                            End If
                            '### 20200702 END   指摘票対応(全体(No97))  ##################
                        End If
                    End If
                Next
                '### 20200622 END  ((全体)No87対応) ######################################

            '    ### 20200717 START((全体)No112対応) ######################################
            Case "WF_CheckBoxSELECTOTTRANSPORT"
                '◯ 輸送形態区分が"M"(請負OT混載)以外の場合
                If Me.TxtOrderTrkKbn.Text <> BaseDllConst.CONST_TRKBN_M Then
                    Exit Select
                End If
                'チェックボックス判定
                For i As Integer = 0 To OIT0003tbl_tab3.Rows.Count - 1
                    If OIT0003tbl_tab3.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If OIT0003tbl_tab3.Rows(i)("OTTRANSPORTFLG") = "on" Then
                            OIT0003tbl_tab3.Rows(i)("OTTRANSPORTFLG") = ""
                        Else
                            OIT0003tbl_tab3.Rows(i)("OTTRANSPORTFLG") = "on"
                        End If
                    End If
                Next
                '### 20200717 END  ((全体)No112対応) ######################################
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_CheckBoxSELECT_TAB4()
        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        'チェックボックス判定
        For i As Integer = 0 To OIT0003tbl_tab4.Rows.Count - 1
            If OIT0003tbl_tab4.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If OIT0003tbl_tab4.Rows(i)("OPERATION") = "on" Then
                    OIT0003tbl_tab4.Rows(i)("OPERATION") = ""
                Else
                    OIT0003tbl_tab4.Rows(i)("OPERATION") = "on"
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)
    End Sub
#End Region

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", Me.WF_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '運用部署
            Case "WF_UORG"
                CODENAME_get("UORG", Me.WF_UORG.Text, Me.WF_UORG_TEXT.Text, WW_RTN_SW)

            '荷主
            Case "TxtShippersCode"
                CODENAME_get("SHIPPERS", Me.TxtShippersCode.Text, Me.LblShippersName.Text, WW_RTN_SW)

            '荷受人
            Case "TxtConsigneeCode"
                CODENAME_get("CONSIGNEE", Me.TxtConsigneeCode.Text, Me.LblConsigneeName.Text, WW_RTN_SW)

            '本線列車
            Case "TxtTrainNo"

                If Me.TxtTrainNo.Text = "" Then
                    '発駅
                    Me.TxtDepstationCode.Text = ""
                    Me.LblDepstationName.Text = ""
                    '着駅
                    Me.TxtArrstationCode.Text = ""
                    Me.LblArrstationName.Text = ""
                    '荷主
                    Me.TxtShippersCode.Text = ""
                    Me.LblShippersName.Text = ""
                    '荷受人
                    Me.TxtConsigneeCode.Text = ""
                    Me.LblConsigneeName.Text = ""
                    '受注パターン
                    Me.TxtOrderType.Text = ""

                    '〇 (予定)の日付を設定
                    Me.TxtLoadingDate.Text = ""
                    Me.TxtDepDate.Text = ""
                    Me.TxtArrDate.Text = ""
                    Me.TxtAccDate.Text = ""
                    Me.TxtEmparrDate.Text = ""

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
                    If Me.TxtOrderOffice.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "TRAINNUMBER", Me.TxtTrainNo.Text, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TRAINNUMBER", Me.TxtTrainNo.Text, WW_GetValue)
                    End If
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER", Me.TxtTrainNo.Text, WW_GetValue)
                End If

                '指定された本線列車№で値が取得できない場合はエラー判定
                If WW_GetValue(0) = "" Then
                    WW_RTN_SW = C_MESSAGE_NO.OIL_TRAIN_MASTER_NOTFOUND
                Else
                    WW_RTN_SW = C_MESSAGE_NO.NORMAL
                End If

                '発駅
                Me.TxtDepstationCode.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", Me.TxtDepstationCode.Text, Me.LblDepstationName.Text, WW_DUMMY)
                '着駅
                Me.TxtArrstationCode.Text = WW_GetValue(2)
                CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_DUMMY)
                Me.TxtTrainNo.Focus()

                '〇 (予定)の日付を設定
                Me.TxtLoadingDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                Me.TxtDepDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                Me.TxtArrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                Me.TxtAccDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                Me.TxtEmparrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")

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
                    If Me.TxtOrderOffice.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
                    End If
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
                End If

                '荷主
                Me.TxtShippersCode.Text = WW_GetValue(0)
                Me.LblShippersName.Text = WW_GetValue(1)
                '荷受人
                Me.TxtConsigneeCode.Text = WW_GetValue(4)
                Me.LblConsigneeName.Text = WW_GetValue(5)
                '受注パターン
                Me.TxtOrderType.Text = WW_GetValue(7)

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
                CODENAME_get("DEPSTATION", Me.TxtDepstationCode.Text, Me.LblDepstationName.Text, WW_RTN_SW)

            '着駅
            Case "TxtArrstationCode"
                CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_RTN_SW)

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

#Region "全選択ボタン押下時処理"
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
#End Region

#Region "全解除ボタン押下時処理"
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
#End Region

#Region "行削除ボタン押下時処理"
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

        '各タブ(一覧)の再表示処理
        ReDisplayTabList()

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB1()

        Dim SelectChk As Boolean = False
        Dim intTblCnt As Integer = 0

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        '■■■ OIT0003tbl関連の受注・受注明細を論理削除 ■■■

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

            '件数を取得
            intTblCnt = OIT0003tbl.Rows.Count

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

                    '★タンク車所在の更新(タンク車№を再度選択できるようにするため)
                    '引数１：所在地コード　⇒　変更なし(空白)
                    '引数２：タンク車状態　⇒　変更あり("3"(到着))
                    '引数３：積車区分　　　⇒　変更なし(空白)
                    '引数４：タンク車状況　⇒　変更あり("1"(残車))
                    'WW_UpdateTankShozai("", "3", "", I_TANKNO:=OIT0003UPDrow("TANKNO"), I_SITUATION:="1",
                    '                    I_AEMPARRDATE:=Me.TxtEmparrDate.Text, upActualEmparrDate:=True)
                    WW_UpdateTankShozai("", "3", "", I_TANKNO:=OIT0003UPDrow("TANKNO"), I_SITUATION:="1",
                                        I_AEMPARRDATE:="", upActualEmparrDate:=True)

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

            '### 20200609 START(内部No178) #################################################
            '一覧明細の件数を取得
            Dim cntTbl As Integer = OIT0003tbl.Select("DELFLG <> '1'").Count
            If cntTbl = 0 Then
                '★ 一覧明細がすべて削除(0件)になった場合は、すべてのステータスを初期値に戻す
                '◯ 受注TBLのステータス初期化
                WW_UpdateOrderStatus(BaseDllConst.CONST_ORDERSTATUS_100,
                                     InitializeFlg:=True)

                '◯ 画面定義変数の初期化
                '　★受注進行ステータス(100:受注受付)
                Me.TxtOrderStatus.Text = "受注受付"
                work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100
                '　★作成モード(１：新規登録, ２：更新)設定
                work.WF_SEL_CREATEFLG.Text = "1"
                '　★作成モード(１：貨車連結未使用, ２：貨車連結使用)設定
                work.WF_SEL_CREATELINKFLG.Text = "1"
                '　★手配連絡フラグ(0：未連絡, 1：連絡)設定
                work.WF_SEL_CONTACTFLG.Text = "0"
                '　★結果受理フラグ(0：未受理, 1：受理)設定
                work.WF_SEL_RESULTFLG.Text = "0"
                '　★託送指示フラグ(0：未手配, 1：手配)設定
                work.WF_SEL_DELIVERYFLG.Text = "0"
                '　★車数
                Me.TxtHTank.Text = "0"
                Me.TxtRTank.Text = "0"
                Me.TxtTTank.Text = "0"
                Me.TxtMTTank.Text = "0"
                Me.TxtKTank.Text = "0"
                Me.TxtK3Tank.Text = "0"
                Me.TxtK5Tank.Text = "0"
                Me.TxtK10Tank.Text = "0"
                Me.TxtLTank.Text = "0"
                Me.TxtATank.Text = "0"
                Me.TxtTotalCnt.Text = "0"
                '　★車数(割当後)
                Me.TxtHTank_w.Text = "0"
                Me.TxtRTank_w.Text = "0"
                Me.TxtTTank_w.Text = "0"
                Me.TxtMTTank_w.Text = "0"
                Me.TxtKTank_w.Text = "0"
                Me.TxtK3Tank_w.Text = "0"
                Me.TxtK5Tank_w.Text = "0"
                Me.TxtK10Tank_w.Text = "0"
                Me.TxtLTank_w.Text = "0"
                Me.TxtATank_w.Text = "0"
                Me.TxtTotalCnt_w.Text = "0"

                '〇 油種数登録ボタンのチェックを無効(False)
                WF_ButtonInsertFLG.Value = "FALSE"

            End If

            '(受注TBL)タンク車数更新
            WW_UpdateOrderTankCnt(SQLcon)

            '受注(一覧)表示用
            WW_OrderListTBLSet(SQLcon)
            '### 20200609 END  (内部No178) #################################################

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
        '一覧件数が０件の時の行削除の場合
        If intTblCnt = 0 Then
            Master.Output(C_MESSAGE_NO.OIL_DELDATA_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            '一覧件数が１件以上で未選択による行削除の場合
        ElseIf SelectChk = False Then
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

        Dim SelectChk As Boolean = False
        Dim intTblCnt As Integer = 0

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        '■■■ OIT0003tbl関連の受注・受注明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注費用を一括論理削除
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0010_ORDERBILLING  " _
                    & "    SET UPDYMD          = @P11, " _
                    & "        UPDUSER         = @P12, " _
                    & "        UPDTERMID       = @P13, " _
                    & "        RECEIVEYMD      = @P14, " _
                    & "        DELFLG          = '1'   " _
                    & "  WHERE BILLINGNO       = @P01  " _
                    & "    AND BILLINGDETAILNO = @P02  " _
                    & "    AND DELFLG          <> '1';"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            '件数を取得
            intTblCnt = OIT0003tbl_tab4.Rows.Count

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            For Each OIT0003UPDrow In OIT0003tbl_tab4.Rows
                If OIT0003UPDrow("OPERATION") = "on" Then

                    If OIT0003UPDrow("LINECNT") < 9000 Then
                        SelectChk = True
                    End If

                    j += 1
                    OIT0003UPDrow("LINECNT") = j        'LINECNT
                    'OIT0003UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0003UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0003UPDrow("BILLINGNO")
                    PARA02.Value = OIT0003UPDrow("BILLINGDETAILNO")

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

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB4 DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB4 DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        '○メッセージ表示
        '一覧件数が０件の時の行削除の場合
        If intTblCnt = 0 Then
            Master.Output(C_MESSAGE_NO.OIL_DELDATA_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            '一覧件数が１件以上で未選択による行削除の場合
        ElseIf SelectChk = False Then
            Master.Output(C_MESSAGE_NO.OIL_DELLINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

        Else
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

    End Sub
#End Region

#Region "行追加ボタン押下時処理"
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
        If work.WF_SEL_CREATEFLG.Text = "1" OrElse OIT0003tbl.Rows.Count = 0 Then
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
            & " , '未割当'                                       AS TANKQUOTA" _
            & " , ''                                             AS LINKNO" _
            & " , ''                                             AS LINKDETAILNO" _
            & " , ''                                             AS SHIPORDER" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS TANKSTATUS" _
            & " , ''                                             AS LINEORDER" _
            & " , ''                                             AS MODEL" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS STACKINGFLG" _
            & " , ''                                             AS ACTUALLODDATE" _
            & " , ''                                             AS JOINTCODE" _
            & " , ''                                             AS JOINT" _
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
            & " , ''                                             AS CHANGERETSTATION" _
            & " , ''                                             AS CHANGERETSTATIONNAME" _
            & " , ''                                             AS USEORDERNO" _
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
                For Each OIT0003WKrow As DataRow In OIT0003WKtbl.Rows
                    intDetailNo = OIT0003WKrow("DETAILNO")
                    PARA1.Value = OIT0003WKrow("ORDERNO")
                    PARA8.Value = OIT0003WKrow("DETAILNO")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB1 LINEADD")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB1 LINEADD"
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

        If IsNothing(OIT0003WKtbl_tab4) Then
            OIT0003WKtbl_tab4 = New DataTable
        End If

        If OIT0003WKtbl_tab4.Columns.Count <> 0 Then
            OIT0003WKtbl_tab4.Columns.Clear()
        End If

        OIT0003WKtbl_tab4.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String =
              " SELECT TOP (1)" _
            & "   0                                                  AS LINECNT" _
            & " , ''                                                 AS OPERATION" _
            & " , '0'                                                AS TIMSTP" _
            & " , 1                                                  AS 'SELECT'" _
            & " , 0                                                  AS HIDDEN" _
            & " , @P01                                               AS BILLINGNO" _
            & " , ''                                                 AS BILLINGDETAILNO" _
            & " , @P02                                               AS ORDERNO" _
            & " , ''                                                 AS SHIPPERSCODE" _
            & " , ''                                                 AS SHIPPERSNAME" _
            & " , ''                                                 AS BASECODE" _
            & " , ''                                                 AS BASENAME" _
            & " , @P03                                               AS OFFICECODE" _
            & " , @P04                                               AS OFFICENAME" _
            & " , ''                                                 AS CONSIGNEECODE" _
            & " , ''                                                 AS CONSIGNEENAME" _
            & " , ''                                                 AS ORDERINFO" _
            & " , ''                                                 AS ORDERINFONAME" _
            & " , '2'                                                AS CALCACCOUNT" _
            & " , '1'                                                AS AKAKURO" _
            & " , FORMAT(GETDATE(), 'yyyy/MM')                       AS KEIJYOYM" _
            & " , FORMAT(GETDATE(), 'yyyy/MM/dd')                    AS KEIJYOYMD" _
            & " , ''                                                 AS ACCSEGCODE" _
            & " , ''                                                 AS ACCSEGNAME" _
            & " , ''                                                 AS ACCOUNTCODE" _
            & " , ''                                                 AS ACCOUNTNAME" _
            & " , ''                                                 AS SEGMENTCODE" _
            & " , ''                                                 AS SEGMENTNAME" _
            & " , ''                                                 AS BREAKDOWNCODE" _
            & " , ''                                                 AS BREAKDOWN" _
            & " , '2'                                                AS CALCKBN" _
            & " , 0                                               AS CARSAMOUNT" _
            & " , ''                                                 AS CARSAMOUNTNAME" _
            & " , ''                                                 AS APPLYCHARGE" _
            & " , 0                                               AS APPLYCHARGESUM" _
            & " , 0                                               AS CONSUMPTIONTAX" _
            & " , ''                                                 AS INVOICECODE" _
            & " , ''                                                 AS INVOICENAME" _
            & " , ''                                                 AS INVOICEDEPTNAME" _
            & " , ''                                                 AS PAYEECODE" _
            & " , ''                                                 AS PAYEENAME" _
            & " , ''                                                 AS PAYEEDEPTNAME" _
            & " FROM sys.all_objects "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '支払請求№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 11) '受注№
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 6)  '営業所コード
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20) '営業所名

                PARA1.Value = work.WF_SEL_BILLINGNO.Text
                PARA2.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA3.Value = work.WF_SEL_ORDERSALESOFFICECODE.Text
                PARA4.Value = work.WF_SEL_ORDERSALESOFFICE.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl_tab4.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                Dim intDetailNo As Integer = 0
                For Each OIT0003row As DataRow In OIT0003tbl_tab4.Rows

                    '行追加データに既存の受注№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[受注№]を利用
                    If OIT0003row("BILLINGDETAILNO") = "" Then
                        intDetailNo += 1
                        OIT0003row("BILLINGDETAILNO") = intDetailNo.ToString("000")

                    Else
                        intDetailNo = OIT0003row("BILLINGDETAILNO")

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TAB4 LINEADD")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TAB4 LINEADD"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub
#End Region

    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '〇 選択されたタブ一覧の各更新ボタン押下時の制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            '割当確定ボタン押下時
            Me.WW_UPBUTTONFLG = "1"
            '### 20200812 START(指摘票(全体)No121) #########################################
            '★初期化
            work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100
            '手配連絡フラグ("0"(未連絡))
            work.WF_SEL_CONTACTFLG.Text = "0"
            '結果受理フラグ("0"(未受理))
            work.WF_SEL_RESULTFLG.Text = "0"
            '託送指示フラグ("0"(未手配))
            work.WF_SEL_DELIVERYFLG.Text = "0"
            '### 20200812 END  (指摘票(全体)No121) #########################################
            WW_ButtonUPDATE_TAB1()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            '入力内容登録ボタン押下時
            Me.WW_UPBUTTONFLG = "2"
            WW_ButtonUPDATE_TAB2()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            '明細更新ボタン押下時
            Me.WW_UPBUTTONFLG = "3"
            WW_ButtonUPDATE_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            '訂正更新ボタン押下時
            Me.WW_UPBUTTONFLG = "4"
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

        '〇新規登録時で油種数登録ボタンを押下しているかチェック
        If work.WF_SEL_CREATEFLG.Text = "1" _
            AndAlso WF_ButtonInsertFLG.Value = "FALSE" Then

            Master.Output(C_MESSAGE_NO.OIL_OILREGISTER_ORDER_NOTUSE, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '### 20200818 START (一覧)タンク車Noがすべて割当されてない場合は更新のみ実施 #####################
        If OIT0003tbl.Select("TANKNO = '' AND DELFLG = '0'").Count <> 0 Then
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

            '各タブ(一覧)の再表示処理
            ReDisplayTabList()

            Exit Sub
        End If
        '### 20200818 END   (一覧)タンク車Noがすべて割当されてない場合は更新のみ実施 #####################

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
        Dim blnOilCheck As Boolean = False
        WW_CheckLastOilConsistency(WW_ERRCODE)
        '前回黒油によるエラー
        If WW_ERRCODE = "ERR1" Then
            Master.Output(C_MESSAGE_NO.OIL_LASTOIL_CONSISTENCY_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            Exit Sub

            '前回揮発油,今回黒油、または灯軽油による警告
        ElseIf WW_ERRCODE = "ERR2" _
            AndAlso work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then
            blnOilCheck = True
            Master.Output(C_MESSAGE_NO.OIL_LASTVOLATILEOIL_BLACKLIGHTOIL_ERROR,
              C_MESSAGE_TYPE.QUES,
              needsPopUp:=True,
              messageBoxTitle:="",
              IsConfirm:=True,
              YesButtonId:="btnChkLastOilConfirmYes",
              needsConfirmNgToPostBack:=True,
              NoButtonId:="btnChkLastOilConfirmNo")
        End If

        '〇 タンク車状態チェック
        WW_CheckTankStatus(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '〇 高速列車対応タンク車チェック
        WW_CheckSpeedTrainTank(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

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
        End If

        '〇列車重複チェック(同一レコードがすでに登録済みかチェック)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_CheckTrainRepeat(WW_ERRCODE, SQLcon)
            If WW_ERRCODE = "ERR" Then

                '### 20200828 START エラー時のデータ削除を廃止 #########################
                ''★新規登録の場合のみ
                'If work.WF_SEL_CREATEFLG.Text = "1" Then
                '    '★チェックNGの場合は、登録されている受注TBL・受注明細TBLを削除する。
                '    WW_DeleteOrder(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                'End If
                '### 20200828 END   エラー時のデータ削除を廃止 #########################
                Exit Sub
            End If
        End Using

        '◯列車マスタ(発送順区分)が対象(1:発送対象)の場合チェックを実施
        '　※上記以外(2:発送対象外)については、入力しないためチェックは未実施。
        If work.WF_SEL_SHIPORDERCLASS.Text = "1" Then
            '列車発送順重複チェック(同じ列車(発日も一緒)で発送順がすでに登録済みかチェック)
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_CheckTrainShipRepeat(WW_ERRCODE, SQLcon)
                If WW_ERRCODE = "ERR" Then
                    Master.Output(C_MESSAGE_NO.OIL_SHIPORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    Exit Sub
                End If
            End Using
        End If

        '列車タンク車重複チェック(同一(異なる)列車(発日(積込日)も一緒)でタンク車がすでに登録済みかチェック)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_CheckTrainTankRepeat(WW_ERRCODE, SQLcon)
            If WW_ERRCODE = "ERR1" Then
                Master.Output(C_MESSAGE_NO.OIL_ORDER_DEPDATE_SAMETRAINTANKNO, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                '### 20200828 START エラー時のデータ削除を廃止 #########################
                ''★新規登録の場合のみ
                'If work.WF_SEL_CREATEFLG.Text = "1" Then
                '    '★チェックNGの場合は、登録されている受注TBL・受注明細TBLを削除する。
                '    WW_DeleteOrder(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                'End If
                '### 20200828 END   エラー時のデータ削除を廃止 #########################
                Exit Sub
            ElseIf WW_ERRCODE = "ERR2" Then
                Master.Output(C_MESSAGE_NO.OIL_ORDER_DEPDATE_DIFFTRAINTANKNO, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                '### 20200828 START エラー時のデータ削除を廃止 #########################
                ''★新規登録の場合のみ
                'If work.WF_SEL_CREATEFLG.Text = "1" Then
                '    '★チェックNGの場合は、登録されている受注TBL・受注明細TBLを削除する。
                '    WW_DeleteOrder(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                'End If
                '### 20200828 END   エラー時のデータ削除を廃止 #########################
                Exit Sub
            ElseIf WW_ERRCODE = "ERR3" Then
                Master.Output(C_MESSAGE_NO.OIL_ORDER_LODDATE_DIFFTRAINTANKNO, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                '### 20200828 START エラー時のデータ削除を廃止 #########################
                ''★新規登録の場合のみ
                'If work.WF_SEL_CREATEFLG.Text = "1" Then
                '    '★チェックNGの場合は、登録されている受注TBL・受注明細TBLを削除する。
                '    WW_DeleteOrder(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                'End If
                '### 20200828 END   エラー時のデータ削除を廃止 #########################
                Exit Sub
            ElseIf WW_ERRCODE = "ERR4" Then
                Master.Output(C_MESSAGE_NO.OIL_ORDER_LODDATE_SAMETRAINTANKNO, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                '### 20200828 START エラー時のデータ削除を廃止 #########################
                ''★新規登録の場合のみ
                'If work.WF_SEL_CREATEFLG.Text = "1" Then
                '    '★チェックNGの場合は、登録されている受注TBL・受注明細TBLを削除する。
                '    WW_DeleteOrder(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                'End If
                '### 20200828 END   エラー時のデータ削除を廃止 #########################
                Exit Sub
            End If
        End Using

        '◯袖ヶ浦営業所のみ貨物駅入線順のチェックを実施
        '　※上記以外の営業所については、入力しないためチェックは未実施。
        If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011203 Then
            '列車入線順重複チェック(同じ列車(発日も一緒)で入線順がすでに登録済みかチェック)
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_CheckTrainLineRepeat(WW_ERRCODE, SQLcon)
                If WW_ERRCODE = "ERR" Then
                    Master.Output(C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    Exit Sub
                End If
            End Using
        End If

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
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

        '### 20200622 START((全体)No81対応) ######################################
        '◯発送順(MAX値)と列車(油種)数のチェック
        If Integer.Parse(Me.TxtTotalCnt_w.Text) < Integer.Parse(WW_SHIPORDER) Then
            Master.Output(C_MESSAGE_NO.OIL_SHIPORDER_OILTOTAL_OVER, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If
        '### 20200622 END  ((全体)No81対応) ######################################

        '〇タンク車所在の更新
        WW_TankShozaiSet()

        '★ 各タブ(一覧)の再表示処理
        ReDisplayTabList()

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

        '画面表示設定処理(受注進行ステータス)
        '◯前回油種チェック時に警告データがない場合
        If blnOilCheck = False Then WW_ScreenOrderStatusSet()

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

        '五井営業所、甲子営業所、袖ヶ浦営業所の場合
        '積込列車番号の入力を可能とする。
        If work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011201 _
            OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011202 _
            OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then

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
        '★臨海鉄道未対象の場合
        '　入換・積込指示の入力は行わないためチェック対象外
        If WW_RINKAIFLG = False Then
            '積込指示入力(0:未 1:完了)
            WW_LoadingInput = "1"

            '★臨海鉄道対象の場合
        Else
            '### 20200616 START((全体)No74対応) ######################################
            '★袖ヶ浦営業所の場合、充填ポイント未設定のため未チェック
            If work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then
                '積込指示入力(0:未 1:完了)
                WW_LoadingInput = "1"
                '### 20200616 END  ((全体)No74対応) ######################################

            Else
                '積場スペックチェック
                WW_CheckLoadingSpecs(WW_ERRCODE, WW_Message)
                If WW_ERRCODE = "ERR" _
                OrElse WW_ERRCODE = "ERR1" _
                OrElse WW_ERRCODE = "ERR2" _
                OrElse WW_ERRCODE = "ERR3" Then
                    '積込指示入力(0:未 1:完了)
                    WW_LoadingInput = "0"

                End If
            End If
        End If

        '受注進行ステータス退避用
        Dim strOrderStatus As String = ""

        '### START ###############################################################################
        '★臨海鉄道未対象の場合
        If WW_RINKAIFLG = False Then
            '〇 受注進行ステータスの状態
            Select Case work.WF_SEL_ORDERSTATUS.Text
                '受注進行ステータス＝"200:手配"
                Case BaseDllConst.CONST_ORDERSTATUS_200
                    '積込指示入力＝"1:完了"の場合
                    If WW_LoadingInput = "1" Then
                        '310:手配完了
                        strOrderStatus = CONST_ORDERSTATUS_310
                        CODENAME_get("ORDERSTATUS", strOrderStatus, Me.TxtOrderStatus.Text, WW_DUMMY)
                    End If
            End Select

            '★臨海鉄道対象の場合
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
                CODENAME_get("ORDERSTATUS", strOrderStatus, Me.TxtOrderStatus.Text, WW_DUMMY)
                work.WF_SEL_ORDERSTATUS.Text = strOrderStatus
                work.WF_SEL_ORDERSTATUSNM.Text = Me.TxtOrderStatus.Text

            End Using

            '○ 画面表示データ復元
            Master.RecoverTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

            For Each OIT0003row As DataRow In OIT0003WKtbl.Rows
                If OIT0003row("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text Then
                    OIT0003row("ORDERSTATUS") = strOrderStatus
                    OIT0003row("ORDERSTATUSNAME") = Me.TxtOrderStatus.Text
                End If
            Next

            '○ 画面表示データ保存
            Master.SaveTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)
        End If

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
        Else
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

        '〇 受注ステータスが"310:手配完了"へ変更された場合
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 Then
            WF_DTAB_CHANGE_NO.Value = "2"
            WF_Detail_TABChange()

            '### START 受注履歴テーブルの追加(2020/03/26) #############
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_InsertOrderHistory(SQLcon)
            End Using
            '### END   ################################################

            '○メッセージ表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
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

        '### 20200819 START 受注明細画面からの貨車連結表自動作成を廃止 ##########
        ''貨車連結表DB追加・更新
        'Using SQLcon As SqlConnection = CS0050SESSION.getConnection
        '    SQLcon.Open()       'DataBase接続

        '    WW_UpdateLink(SQLcon)
        'End Using
        '### 20200819 END   受注明細画面からの貨車連結表自動作成を廃止 ##########

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

        '### 20200622 START((全体)No81対応) ######################################
        '◯発送順(MAX値)と列車(油種)数のチェック
        If Integer.Parse(Me.TxtTotal_c.Text) < Integer.Parse(WW_SHIPORDER) Then
            Master.Output(C_MESSAGE_NO.OIL_SHIPORDER_OILTOTAL_OVER, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                If OIT0003tab3row("SHIPORDER") = WW_SHIPORDER Then
                    OIT0003tab3row.Item("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_80
                    CODENAME_get("ORDERINFO", OIT0003tab3row.Item("ORDERINFO"), OIT0003tab3row.Item("ORDERINFONAME"), WW_DUMMY)
                End If
            Next
            Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

            Exit Sub
        Else
            For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                If OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_80 Then
                    OIT0003tab3row.Item("ORDERINFO") = ""
                    OIT0003tab3row.Item("ORDERINFONAME") = ""
                End If
            Next
            Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
        End If
        '### 20200622 END  ((全体)No81対応) ######################################

        '### 20200622 START((全体)No82対応) ######################################
        '◯列車マスタ(発送順区分)が対象(1:発送対象)の場合チェックを実施
        '　※上記以外(2:発送対象外)については、入力しないためチェックは未実施。
        If work.WF_SEL_SHIPORDERCLASS.Text = "1" Then
            '列車発送順重複チェック(同じ列車(発日も一緒)で発送順がすでに登録済みかチェック)
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_CheckTrainShipRepeat(WW_ERRCODE, SQLcon, dt:=OIT0003tbl_tab3)
                If WW_ERRCODE = "ERR" Then
                    Master.Output(C_MESSAGE_NO.OIL_SHIPORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    Exit Sub
                End If
            End Using
        End If
        '### 20200622 END  ((全体)No82対応) ######################################

        '受注進行ステータス退避用
        Dim strOrderStatus As String = ""

        '### 受注進行ステータスチェック START ##############################################
        '受注進行ステータスの状態
        Select Case work.WF_SEL_ORDERSTATUS.Text
            '"310:手配完了"
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '"205:手配中（千葉(根岸を除く)以外）"
            '"305:手配完了（託送未）"
            Case BaseDllConst.CONST_ORDERSTATUS_310,
                 BaseDllConst.CONST_ORDERSTATUS_205,
                 BaseDllConst.CONST_ORDERSTATUS_305

                '### 20200630 他の受注で同日の積込日を設定しているタンク車がないかチェック #####
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    WW_CheckSameLodDayOtherOrder(SQLcon, WW_ERRCODE)
                End Using
                If WW_ERRCODE = "ERR" Then
                    Master.Output(C_MESSAGE_NO.OIL_ORDER_LODDATE_DIFFTRAINTANKNO, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    Exit Sub
                End If

                '(実績)積込日の入力が未入力の場合
                If Me.TxtActualLoadingDate.Text = "" Then Exit Select

                '(一覧)数量の入力チェック
                '0(デフォルト値)以外が入力されていれば、入力していると判断
                Dim chkCarsAmount As Boolean = True
                Dim decCarsAmount As Decimal = 0
                Dim WW_CheckMES1 As String = ""
                Dim WW_CheckMES2 As String = ""
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

                        OIT0003Chktab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_96
                        CODENAME_get("ORDERINFO", OIT0003Chktab3row("ORDERINFO"), OIT0003Chktab3row("ORDERINFONAME"), WW_DUMMY)

                        WW_CheckMES1 = "タンク車の油種数量が0(kl)エラー。"
                        WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                        WW_CheckListTab3ERR(WW_CheckMES1, WW_CheckMES2, OIT0003Chktab3row)
                    Else
                        OIT0003Chktab3row("ORDERINFO") = ""
                        OIT0003Chktab3row("ORDERINFONAME") = ""

                    End If
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

                '(実績)積込日の入力が完了、かつ(一覧)数量の入力がすべて完了
                If Me.TxtActualLoadingDate.Text <> "" AndAlso chkCarsAmount = True Then

                    '◯ 他の受注で同日の積込日を設定しているデータ取得・更新処理
                    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()       'DataBase接続

                        WW_UpdateSameStackingOtherOrder(SQLcon)
                    End Using

                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_320

                    '### 油種数量(数値)が0(kl)の場合はエラー表示 ##################
                ElseIf chkCarsAmount = False Then
                    Master.Output(C_MESSAGE_NO.OIL_TANKNO_NUMBER_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    Exit Sub
                    '##############################################################
                End If

                '### ステータス追加(仮) #################################
                '(実績)積込日の入力が完了、かつ(一覧)数量の入力がすべて完了
                'かつ、(実績)発日の入力が完了(★)
                If Me.TxtActualLoadingDate.Text <> "" AndAlso chkCarsAmount = True _
                    AndAlso Me.TxtActualDepDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_350
                End If
                '########################################################

                '(実績)積込日の入力が完了、かつ(一覧)数量の入力がすべて完了
                'かつ、(実績)発日の入力が完了(★)
                'かつ、(実績)積車着日の入力が完了
                If Me.TxtActualLoadingDate.Text <> "" AndAlso chkCarsAmount = True _
                    AndAlso Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_400
                End If

                '### ステータス追加(仮) #################################
                '(実績)発日の入力が完了(★)
                'かつ、(実績)積車着日の入力が完了
                'かつ、(実績)受入日の入力が完了(★)
                If Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" _
                    AndAlso Me.TxtActualAccDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_450
                End If
                '########################################################

                '(実績)積込日の入力が完了、かつ(一覧)数量の入力がすべて完了
                'かつ、(実績)発日の入力が完了(★)
                'かつ、(実績)積車着日の入力が完了
                'かつ、(実績)受入日の入力が完了(★)
                'かつ、(実績)空車着日の入力が完了
                If Me.TxtActualLoadingDate.Text <> "" AndAlso chkCarsAmount = True _
                    AndAlso Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" _
                    AndAlso Me.TxtActualAccDate.Text <> "" _
                    AndAlso Me.TxtActualEmparrDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_500
                End If

            '"320:受注確定"
            Case BaseDllConst.CONST_ORDERSTATUS_320,
                 BaseDllConst.CONST_ORDERSTATUS_350

                '### ステータス追加(仮) #################################
                '(実績)発日の入力が完了(★)
                If Me.TxtActualDepDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_350
                End If
                '########################################################

                '(実績)発日の入力が完了(★)
                'かつ、(実績)積車着日の入力が完了
                If Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_400
                End If

                '### ステータス追加(仮) #################################
                '(実績)発日の入力が完了(★)
                'かつ、(実績)積車着日の入力が完了
                'かつ、(実績)受入日の入力が完了(★)
                If Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" _
                    AndAlso Me.TxtActualAccDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_450
                End If
                '########################################################

                '(実績)発日の入力が完了(★)
                'かつ、(実績)積車着日の入力が完了
                'かつ、(実績)受入日の入力が完了(★)
                'かつ、(実績)空車着日の入力が完了
                If Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" _
                    AndAlso Me.TxtActualAccDate.Text <> "" _
                    AndAlso Me.TxtActualEmparrDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_500
                End If

            '"400:受入確認中"
            Case BaseDllConst.CONST_ORDERSTATUS_400,
                 BaseDllConst.CONST_ORDERSTATUS_450

                '### ステータス追加(仮) #################################
                '(実績)受入日の入力が完了(★)
                If Me.TxtActualAccDate.Text <> "" Then
                    strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_450
                End If
                '########################################################

                '(実績)受入日の入力が完了(★)
                'かつ、(実績)空車着日の入力が完了
                If Me.TxtActualAccDate.Text <> "" _
                    AndAlso Me.TxtActualEmparrDate.Text <> "" Then
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
                CODENAME_get("ORDERSTATUS", strOrderStatus, Me.TxtOrderStatus.Text, WW_DUMMY)
                work.WF_SEL_ORDERSTATUS.Text = strOrderStatus
                work.WF_SEL_ORDERSTATUSNM.Text = Me.TxtOrderStatus.Text

            End Using

            '○ 画面表示データ復元
            Master.RecoverTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

            For Each OIT0003row As DataRow In OIT0003WKtbl.Rows
                If OIT0003row("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text Then
                    OIT0003row("ORDERSTATUS") = strOrderStatus
                    OIT0003row("ORDERSTATUSNAME") = Me.TxtOrderStatus.Text
                End If
            Next

            '○ 画面表示データ保存
            Master.SaveTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)
        End If
        '### 受注進行ステータスチェック END   ##############################################

        '受注(一覧)画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            WW_OrderListTBLSet(SQLcon)
        End Using

        '◎ タブ「タンク車明細」画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGetTab3(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        '◎ タブ「費用入力」画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '勘定科目明細作成
            WW_InsertRequestAccountDetail(SQLcon)

            '費用入力一覧(勘定科目サマリー作成)
            MAPDataGetTab4(SQLcon)

            '費用入力一覧(勘定科目追加項目作成)
            MAPDataADDTab4(SQLcon)

        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        '〇 受注ステータスが"500:検収中"へ変更された場合
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 Then
            WF_DTAB_CHANGE_NO.Value = "3"
            WF_Detail_TABChange()

            '○メッセージ表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

    End Sub

    ''' <summary>
    ''' 訂正更新ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonUPDATE_TAB4()

        '● 関連チェック
        WW_CheckTab4(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

        '受注費用DB追加・更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '受注費用(TBL)追加・更新
            WW_UpdateOrderBilling(SQLcon)

            '受注(TBL)更新
            WW_UpdateOrder_TAB4(SQLcon)

            '受注(一覧)画面表示データ取得
            WW_OrderListTBLSet(SQLcon)

        End Using

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

#Region "Excelアップロード"

    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '〇 選択されたタブ一覧のファイルアップロードの制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_ButtonFILEUPLOAD_TAB1()

            'タブ「入換・積込指示」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonFILEUPLOAD_TAB2()

            'タブ「タンク車明細」
        ElseIf WF_DetailMView.ActiveViewIndex = "2" Then
            WW_ButtonFILEUPLOAD_TAB3()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "3" Then
            WW_ButtonFILEUPLOAD_TAB4()

        End If

    End Sub

    ''' <summary>
    ''' ファイルアップロード時処理(タブ「タンク車割当」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonFILEUPLOAD_TAB1()
        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        'CS0023XLSUPLOAD.PROFID = Master.PROF_REPORT                 'ﾌﾟﾛﾌｧｲﾙID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD(I_PROFID:=Master.PROF_REPORT)
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.Output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
            Exit Sub
        End If

        '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
        Next

        Dim CS0023XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            CS0023XLSTBLrow.ItemArray = XLSTBLrow.ItemArray

            For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
                If IsDBNull(CS0023XLSTBLrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSTBLrow.Item(XLSTBLcol)) Then
                    CS0023XLSTBLrow.Item(XLSTBLcol) = ""
                End If
            Next

            XLSTBLrow.ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        Dim i As Integer = 0
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            OIT0003tbl.Rows(i)("SHIPORDER") = XLSTBLrow("SHIPORDER")
            OIT0003tbl.Rows(i)("TANKNO") = XLSTBLrow("TANKNO")
            OIT0003tbl.Rows(i)("LINEORDER") = XLSTBLrow("LINEORDER")

            WW_TANKNUMBER_FIND(OIT0003tbl.Rows(i))

            i += 1
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' ファイルアップロード時処理(タブ「入換・積込指示」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonFILEUPLOAD_TAB2()

    End Sub

    ''' <summary>
    ''' ファイルアップロード時処理(タブ「タンク車明細」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonFILEUPLOAD_TAB3()

    End Sub

    ''' <summary>
    ''' ファイルアップロード時処理(タブ「費用入力」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonFILEUPLOAD_TAB4()

    End Sub

    ''' <summary>
    ''' タンク車№に紐づく情報を取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_TANKNUMBER_FIND(ByRef OIT0003row As DataRow, Optional ByVal I_CMPCD As String = Nothing)
        Dim WW_TANKNUMBER As String = OIT0003row("TANKNO")
        Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '会社コードが指定されていた場合
        If Not String.IsNullOrEmpty(I_CMPCD) Then
            '指定された会社コードをKEYとする。
            WW_FixvalueMasterSearch(I_CMPCD, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
        Else
            '〇 検索(営業所).テキストボックスが未設定
            If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                '〇 画面(受注営業所).テキストボックスが未設定
                If Me.TxtOrderOffice.Text = "" Then
                    WW_FixvalueMasterSearch(Master.USER_ORG, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                End If
            Else
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
            End If
        End If

        '型式
        OIT0003row("MODEL") = WW_GetValue(7)

        '####################################################
        '前回油種
        OIT0003row("LASTOILCODE") = WW_GetValue(1)
        OIT0003row("LASTOILNAME") = WW_GetValue(4)
        OIT0003row("PREORDERINGTYPE") = WW_GetValue(5)
        OIT0003row("PREORDERINGOILNAME") = WW_GetValue(6)
        '####################################################

        '交検日
        Dim WW_JRINSPECTIONCNT As String
        OIT0003row("JRINSPECTIONDATE") = WW_GetValue(2)
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
                    OIT0003row("JRINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                    OIT0003row("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                Case "2"
                    OIT0003row("JRINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                    OIT0003row("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                Case "3"
                    OIT0003row("JRINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                    OIT0003row("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
            End Select
        Else
            OIT0003row("JRINSPECTIONALERT") = ""
        End If

        '全検日
        Dim WW_JRALLINSPECTIONCNT As String
        OIT0003row("JRALLINSPECTIONDATE") = WW_GetValue(3)
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
                    OIT0003row("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                    OIT0003row("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                Case "2"
                    OIT0003row("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                    OIT0003row("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                Case "3"
                    OIT0003row("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                    OIT0003row("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
            End Select
        Else
            OIT0003row("JRALLINSPECTIONALERT") = ""
        End If

        '〇 タンク車割当状況チェック
        WW_TANKQUOTACHK("TANKNO", OIT0003row)

        '### 20200701 START((全体)No96対応) ######################################
        '★指定したタンク車№が所属営業所以外の場合
        If WW_GetValue(13) <> Me.TxtOrderOfficeCode.Text Then
            '### 20200819 START 管轄支店コード(11001(OT本社))対応 ####################
            If WW_GetValue(14) <> BaseDllConst.CONST_BRANCHCODE_110001 Then
                OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_102
                CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)
            Else
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = ""
            End If
            '### 20200819 END   管轄支店コード(11001(OT本社))対応 ####################
        ElseIf OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_102 Then
            OIT0003row("ORDERINFO") = ""
            OIT0003row("ORDERINFONAME") = ""
        End If
        '### 20200701 END  ((全体)No96対応) ######################################

        '### 20200831 START タンク車の所在地コード確認 ###########################
        '★指定したタンク車№が、発駅以外の所在地の場合
        If WW_GetValue(15) <> Me.TxtDepstationCode.Text Then
            OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_101
            CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)
        ElseIf OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_101 Then
            OIT0003row("ORDERINFO") = ""
            OIT0003row("ORDERINFONAME") = ""
        End If
        '### 20200831 END   タンク車の所在地コード確認 ###########################

    End Sub

#Region "コメント"
    '''' <summary>
    '''' ファイルアップロード時処理
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub WF_FILEUPLOAD()

    '    '○ エラーレポート準備
    '    rightview.SetErrorReport("")

    '    '○ UPLOAD XLSデータ取得
    '    CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
    '    CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
    '    'CS0023XLSUPLOAD.PROFID = Master.PROF_REPORT                 'ﾌﾟﾛﾌｧｲﾙID
    '    CS0023XLSUPLOAD.CS0023XLSUPLOAD(I_PROFID:=Master.PROF_REPORT)
    '    If isNormal(CS0023XLSUPLOAD.ERR) Then
    '        If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
    '            Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
    '            Exit Sub
    '        End If
    '    Else
    '        Master.Output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
    '        Exit Sub
    '    End If

    '    '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
    '    Dim WW_COLUMNS As New List(Of String)
    '    For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
    '        WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
    '    Next

    '    Dim CS0023XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
    '    For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
    '        CS0023XLSTBLrow.ItemArray = XLSTBLrow.ItemArray

    '        For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
    '            If IsDBNull(CS0023XLSTBLrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSTBLrow.Item(XLSTBLcol)) Then
    '                CS0023XLSTBLrow.Item(XLSTBLcol) = ""
    '            End If
    '        Next

    '        XLSTBLrow.ItemArray = CS0023XLSTBLrow.ItemArray
    '    Next

    '    '○ XLSUPLOAD明細⇒INPtbl
    '    Master.CreateEmptyTable(OIT0003INPtbl)

    '    '★新規受注№の取得
    '    Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
    '    WW_FixvalueMasterSearch("", "NEWORDERNOGET", "", WW_GetValue)
    '    work.WF_SEL_ORDERNUMBER.Text = WW_GetValue(0)

    '    '★受注明細№
    '    Dim intDETAILNO As Integer = 1

    '    For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
    '        Dim OIT0003INProw As DataRow = OIT0003INPtbl.NewRow

    '        '○ 初期クリア
    '        For Each OIT0003INPcol As DataColumn In OIT0003INPtbl.Columns
    '            If IsDBNull(OIT0003INProw.Item(OIT0003INPcol)) OrElse IsNothing(OIT0003INProw.Item(OIT0003INPcol)) Then
    '                Select Case OIT0003INPcol.ColumnName
    '                    Case "LINECNT"
    '                        OIT0003INProw.Item(OIT0003INPcol) = 0
    '                    Case "OPERATION"
    '                        OIT0003INProw.Item(OIT0003INPcol) = C_LIST_OPERATION_CODE.NODATA
    '                    Case "TIMSTP"
    '                        OIT0003INProw.Item(OIT0003INPcol) = 0
    '                    Case "SELECT"
    '                        OIT0003INProw.Item(OIT0003INPcol) = 1
    '                    Case "HIDDEN"
    '                        OIT0003INProw.Item(OIT0003INPcol) = 0
    '                    Case Else
    '                        OIT0003INProw.Item(OIT0003INPcol) = ""
    '                End Select
    '            End If
    '        Next

    '        '○ 変更元情報をデフォルト設定
    '        If WW_COLUMNS.IndexOf("OILCODE") >= 0 AndAlso
    '            WW_COLUMNS.IndexOf("SHIPORDER") >= 0 AndAlso
    '            WW_COLUMNS.IndexOf("TANKNO") >= 0 AndAlso
    '            WW_COLUMNS.IndexOf("LINEORDER") >= 0 Then
    '            For Each OIT0003row As DataRow In OIT0003tbl.Rows
    '                If XLSTBLrow("OILCODE") = OIT0003row("OILCODE") AndAlso
    '                    XLSTBLrow("SHIPORDER") = OIT0003row("SHIPORDER") AndAlso
    '                    XLSTBLrow("TANKNO") = OIT0003row("TANKNO") AndAlso
    '                    XLSTBLrow("LINEORDER") = OIT0003row("LINEORDER") Then
    '                    OIT0003INProw.ItemArray = OIT0003row.ItemArray
    '                    Exit For
    '                End If
    '            Next
    '        End If

    '        '○ 項目セット
    '        Dim WW_GetFieldValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

    '        '受注営業所コード
    '        If WW_COLUMNS.IndexOf("OFFICECODE") >= 0 Then
    '            'OIT0003INProw("OFFICECODE") = XLSTBLrow("OFFICECODE")
    '            Me.TxtOrderOfficeCode.Text = XLSTBLrow("OFFICECODE")
    '            CODENAME_get("SALESOFFICE", Me.TxtOrderOfficeCode.Text, Me.TxtOrderOffice.Text, WW_RTN_SW)
    '        End If

    '        '本線列車名
    '        If WW_COLUMNS.IndexOf("TRAINNAME") >= 0 Then
    '            'OIT0003INProw("TRAINNAME") = XLSTBLrow("TRAINNAME")
    '            Me.TxtTrainName.Text = XLSTBLrow("TRAINNAME")
    '        End If

    '        '本線列車
    '        If WW_COLUMNS.IndexOf("TRAINNO") >= 0 Then
    '            'OIT0003INProw("TRAINNO") = XLSTBLrow("TRAINNO")
    '            Me.TxtTrainNo.Text = XLSTBLrow("TRAINNO")
    '        End If

    '        '受注№
    '        OIT0003INProw("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text

    '        '受注明細№
    '        OIT0003INProw("DETAILNO") = intDETAILNO.ToString("000")

    '        '### 油種情報取得 ##########################################################################################
    '        '油種コード
    '        If WW_COLUMNS.IndexOf("OILCODE") >= 0 Then
    '            OIT0003INProw("OILCODE") = XLSTBLrow("OILCODE")
    '        End If
    '        '配列初期化
    '        WW_GetFieldValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
    '        '油種情報を取得
    '        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", OIT0003INProw("OILCODE"), WW_GetFieldValue)
    '        '油種名
    '        OIT0003INProw("OILNAME") = WW_GetFieldValue(0)
    '        '油種区分
    '        OIT0003INProw("ORDERINGTYPE") = WW_GetFieldValue(1)
    '        '受注油種
    '        OIT0003INProw("ORDERINGOILNAME") = WW_GetFieldValue(2)
    '        '###########################################################################################################

    '        'タンク車割当状況

    '        '発送順
    '        If WW_COLUMNS.IndexOf("SHIPORDER") >= 0 Then
    '            OIT0003INProw("SHIPORDER") = XLSTBLrow("SHIPORDER")
    '        End If

    '        '### タンク車情報取得 ######################################################################################
    '        'タンク車№
    '        If WW_COLUMNS.IndexOf("TANKNO") >= 0 Then
    '            OIT0003INProw("TANKNO") = XLSTBLrow("TANKNO")
    '        End If
    '        '配列初期化
    '        WW_GetFieldValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
    '        WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "TANKNUMBER", OIT0003INProw("TANKNO"), WW_GetFieldValue)
    '        '形式
    '        OIT0003INProw("MODEL") = WW_GetFieldValue(7)
    '        '前回油種コード
    '        OIT0003INProw("LASTOILCODE") = WW_GetFieldValue(1)
    '        '前回油種名
    '        OIT0003INProw("LASTOILNAME") = WW_GetFieldValue(4)
    '        '前回油種区分
    '        OIT0003INProw("PREORDERINGTYPE") = WW_GetFieldValue(5)
    '        '前回受注油種
    '        OIT0003INProw("PREORDERINGOILNAME") = WW_GetFieldValue(6)
    '        '交検日
    '        Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
    '        Dim WW_JRINSPECTIONCNT As String
    '        OIT0003INProw("JRINSPECTIONDATE") = WW_GetFieldValue(2)
    '        If WW_GetFieldValue(2) <> "" Then
    '            WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetFieldValue(2)))

    '            Dim WW_JRINSPECTIONFLG As String
    '            If WW_JRINSPECTIONCNT <= 3 Then
    '                WW_JRINSPECTIONFLG = "1"
    '            ElseIf WW_JRINSPECTIONCNT >= 4 And WW_JRINSPECTIONCNT <= 6 Then
    '                WW_JRINSPECTIONFLG = "2"
    '            Else
    '                WW_JRINSPECTIONFLG = "3"
    '            End If
    '            Select Case WW_JRINSPECTIONFLG
    '                Case "1"
    '                    OIT0003INProw("JRINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
    '                    OIT0003INProw("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
    '                Case "2"
    '                    OIT0003INProw("JRINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
    '                    OIT0003INProw("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
    '                Case "3"
    '                    OIT0003INProw("JRINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
    '                    OIT0003INProw("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
    '            End Select
    '        Else
    '            OIT0003INProw("JRINSPECTIONALERT") = ""
    '        End If

    '        '全検日
    '        Dim WW_JRALLINSPECTIONCNT As String
    '        OIT0003INProw("JRALLINSPECTIONDATE") = WW_GetFieldValue(3)
    '        If WW_GetFieldValue(3) <> "" Then
    '            WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetFieldValue(3)))

    '            Dim WW_JRALLINSPECTIONFLG As String
    '            If WW_JRALLINSPECTIONCNT <= 3 Then
    '                WW_JRALLINSPECTIONFLG = "1"
    '            ElseIf WW_JRALLINSPECTIONCNT >= 4 And WW_JRALLINSPECTIONCNT <= 6 Then
    '                WW_JRALLINSPECTIONFLG = "2"
    '            Else
    '                WW_JRALLINSPECTIONFLG = "3"
    '            End If
    '            Select Case WW_JRALLINSPECTIONFLG
    '                Case "1"
    '                    OIT0003INProw("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
    '                    OIT0003INProw("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
    '                Case "2"
    '                    OIT0003INProw("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
    '                    OIT0003INProw("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
    '                Case "3"
    '                    OIT0003INProw("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
    '                    OIT0003INProw("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
    '            End Select
    '        Else
    '            OIT0003INProw("JRALLINSPECTIONALERT") = ""
    '        End If
    '        '###########################################################################################################

    '        '貨物駅入線順
    '        If WW_COLUMNS.IndexOf("LINEORDER") >= 0 Then
    '            OIT0003INProw("LINEORDER") = XLSTBLrow("LINEORDER")
    '        End If

    '        ''本線列車（変更後）
    '        'If WW_COLUMNS.IndexOf("CHANGETRAINNO") >= 0 Then
    '        '    OIT0003INProw("CHANGETRAINNO") = XLSTBLrow("CHANGETRAINNO")
    '        'End If

    '        ''本線列車名
    '        'If WW_COLUMNS.IndexOf("CHANGETRAINNAME") >= 0 Then
    '        '    OIT0003INProw("CHANGETRAINNAME") = XLSTBLrow("CHANGETRAINNAME")
    '        'End If

    '        ''第2荷受人コード
    '        'If WW_COLUMNS.IndexOf("SECONDCONSIGNEECODE") >= 0 Then
    '        '    OIT0003INProw("SECONDCONSIGNEECODE") = XLSTBLrow("SECONDCONSIGNEECODE")
    '        'End If

    '        ''第2荷受人
    '        'If WW_COLUMNS.IndexOf("SECONDCONSIGNEENAME") >= 0 Then
    '        '    OIT0003INProw("SECONDCONSIGNEENAME") = XLSTBLrow("SECONDCONSIGNEENAME")
    '        'End If

    '        ''第2着駅コード
    '        'If WW_COLUMNS.IndexOf("SECONDARRSTATION") >= 0 Then
    '        '    OIT0003INProw("SECONDARRSTATION") = XLSTBLrow("SECONDARRSTATION")
    '        'End If

    '        ''第2着駅
    '        'If WW_COLUMNS.IndexOf("SECONDARRSTATIONNAME") >= 0 Then
    '        '    OIT0003INProw("SECONDARRSTATIONNAME") = XLSTBLrow("SECONDARRSTATIONNAME")
    '        'End If

    '        ''空車着駅コード（変更後）
    '        'If WW_COLUMNS.IndexOf("CHANGERETSTATION") >= 0 Then
    '        '    OIT0003INProw("CHANGERETSTATION") = XLSTBLrow("CHANGERETSTATION")
    '        'End If

    '        ''空車着駅名（変更後）
    '        'If WW_COLUMNS.IndexOf("CHANGERETSTATIONNAME") >= 0 Then
    '        '    OIT0003INProw("CHANGERETSTATIONNAME") = XLSTBLrow("CHANGERETSTATIONNAME")
    '        'End If

    '        '削除フラグ
    '        If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
    '            OIT0003INProw("DELFLG") = XLSTBLrow("DELFLG")
    '        End If

    '        OIT0003INPtbl.Rows.Add(OIT0003INProw)
    '        intDETAILNO += 1
    '    Next

    '    '〇 取得した列車名から各値を取得し設定する。
    '    WW_TRAINNUMBER_FIND(Me.TxtTrainName.Text)

    '    '○ 項目チェック
    '    'INPTableCheck(WW_ERR_SW)

    '    '○ 入力値のテーブル反映
    '    OIT0003tbl_UPD()

    '    '○ 画面表示データ保存
    '    Master.SaveTable(OIT0003tbl)

    '    '○ メッセージ表示
    '    If isNormal(WW_ERR_SW) Then
    '        Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
    '    Else
    '        Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
    '    End If

    '    '○ Close
    '    CS0023XLSUPLOAD.TBLDATA.Dispose()
    '    CS0023XLSUPLOAD.TBLDATA.Clear()

    'End Sub

    '''' <summary>
    '''' OIT0003tbl更新
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub OIT0003tbl_UPD()

    '    '○ 画面状態設定
    '    For Each OIT0003row As DataRow In OIT0003tbl.Rows
    '        Select Case OIT0003row("OPERATION")
    '            Case C_LIST_OPERATION_CODE.NODATA
    '                OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
    '            Case C_LIST_OPERATION_CODE.NODISP
    '                OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
    '            Case C_LIST_OPERATION_CODE.SELECTED
    '                OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
    '            Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
    '                OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
    '            Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
    '                OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
    '        End Select
    '    Next

    '    '○ 追加変更判定
    '    For Each OIT0003INProw As DataRow In OIT0003INPtbl.Rows

    '        'エラーレコード読み飛ばし
    '        If OIT0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
    '            Continue For
    '        End If

    '        OIT0003INProw.Item("OPERATION") = CONST_INSERT

    '        'KEY項目が等しい時
    '        For Each OIT0003row As DataRow In OIT0003tbl.Rows
    '            If OIT0003row("ORDERNO") = OIT0003INProw("ORDERNO") AndAlso
    '                OIT0003row("DETAILNO") = OIT0003INProw("DETAILNO") Then
    '                'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
    '                If OIT0003row("DELFLG") = OIT0003INProw("DELFLG") AndAlso
    '                    OIT0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
    '                Else
    '                    'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
    '                    OIT0003INProw("OPERATION") = CONST_UPDATE
    '                    Exit For
    '                End If

    '                Exit For

    '            End If
    '        Next
    '    Next

    '    '○ 変更有無判定　&　入力値反映
    '    For Each OIT0003INProw As DataRow In OIT0003INPtbl.Rows
    '        Select Case OIT0003INProw("OPERATION")
    '            Case CONST_UPDATE
    '                TBL_UPDATE_SUB(OIT0003INProw)
    '            Case CONST_INSERT
    '                TBL_INSERT_SUB(OIT0003INProw)
    '            Case CONST_PATTERNERR
    '                '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
    '                TBL_INSERT_SUB(OIT0003INProw)
    '            Case C_LIST_OPERATION_CODE.ERRORED
    '                TBL_ERR_SUB(OIT0003INProw)
    '        End Select
    '    Next

    'End Sub

    '''' <summary>
    '''' 更新予定データの一覧更新時処理
    '''' </summary>
    '''' <param name="OIT0003INProw"></param>
    '''' <remarks></remarks>
    'Protected Sub TBL_UPDATE_SUB(ByRef OIT0003INProw As DataRow)

    '    For Each OIT0003row As DataRow In OIT0003tbl.Rows

    '        '同一レコードか判定
    '        If OIT0003INProw("ORDERNO") = OIT0003row("ORDERNO") AndAlso
    '            OIT0003INProw("DETAILNO") = OIT0003row("DETAILNO") Then
    '            '画面入力テーブル項目設定
    '            OIT0003INProw("LINECNT") = OIT0003row("LINECNT")
    '            OIT0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
    '            OIT0003INProw("TIMSTP") = OIT0003row("TIMSTP")
    '            OIT0003INProw("SELECT") = 1
    '            OIT0003INProw("HIDDEN") = 0

    '            '項目テーブル項目設定
    '            OIT0003row.ItemArray = OIT0003INProw.ItemArray
    '            Exit For
    '        End If
    '    Next

    'End Sub

    '''' <summary>
    '''' 追加予定データの一覧登録時処理
    '''' </summary>
    '''' <param name="OIT0003INProw"></param>
    '''' <remarks></remarks>
    'Protected Sub TBL_INSERT_SUB(ByRef OIT0003INProw As DataRow)

    '    '○ 項目テーブル項目設定
    '    Dim OIT0003row As DataRow = OIT0003tbl.NewRow
    '    OIT0003row.ItemArray = OIT0003INProw.ItemArray

    '    OIT0003row("LINECNT") = OIT0003tbl.Rows.Count + 1
    '    If OIT0003INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
    '        OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
    '    Else
    '        OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
    '    End If

    '    OIT0003row("TIMSTP") = "0"
    '    OIT0003row("SELECT") = 1
    '    OIT0003row("HIDDEN") = 0

    '    OIT0003tbl.Rows.Add(OIT0003row)

    'End Sub

    '''' <summary>
    '''' エラーデータの一覧登録時処理
    '''' </summary>
    '''' <param name="OIT0003INProw"></param>
    '''' <remarks></remarks>
    'Protected Sub TBL_ERR_SUB(ByRef OIT0003INProw As DataRow)

    '    For Each OIT0003row As DataRow In OIT0003tbl.Rows

    '        '同一レコードか判定
    '        If OIT0003INProw("ORDERNO") = OIT0003row("ORDERNO") AndAlso
    '           OIT0003INProw("DETAILNO") = OIT0003row("DETAILNO") Then
    '            '画面入力テーブル項目設定
    '            OIT0003INProw("LINECNT") = OIT0003row("LINECNT")
    '            OIT0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
    '            OIT0003INProw("TIMSTP") = OIT0003row("TIMSTP")
    '            OIT0003INProw("SELECT") = 1
    '            OIT0003INProw("HIDDEN") = 0

    '            '項目テーブル項目設定
    '            OIT0003row.ItemArray = OIT0003INProw.ItemArray
    '            Exit For
    '        End If
    '    Next

    'End Sub
#End Region

#End Region

#Region "リスト変更時処理"
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
                WW_ListChange_TAB4()

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

            Case "SHIPORDER"         '(一覧)発送順
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "TANKNO"            '(一覧)タンク車№

                '受注情報
                updHeader.Item("ORDERINFO") = ""
                updHeader.Item("ORDERINFONAME") = ""

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

                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)

                '設定されたタンク車Noを設定
                updHeader.Item("TANKNO") = WW_ListValue

                'タンク車№に紐づく情報を取得・設定
                WW_TANKNUMBER_FIND(updHeader, I_CMPCD:=work.WF_SEL_CAMPCODE.Text)

            Case "LINEORDER"              '(一覧)貨物駅入線順
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

                '(★サーバー側で設定しているため必要ないが念のため残す(20200302))
            Case "CHANGETRAINNO"          '(一覧)本線列車番号変更
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "SECONDARRSTATIONNAME"   '(一覧)第2着駅
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "SECONDCONSIGNEENAME"    '(一覧)第2荷受人
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

                '(★サーバー側で設定しているため必要ないが念のため残す(20200302))
            Case "CHANGERETSTATIONNAME"   '(一覧)空車着駅(変更)
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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "LOADINGIRILINEORDER"      '(一覧)積込入線順
                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)

                '入力された値が""(空文字)の場合
                If WW_ListValue = "" Then
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("LOADINGOUTLETORDER") = ""
                    '入力された値が0、または一覧の件数より大きい場合
                ElseIf Integer.Parse(WW_ListValue) = 0 _
                        OrElse Integer.Parse(WW_ListValue) > intListCnt Then
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("LOADINGOUTLETORDER") = ""
                Else
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("LOADINGOUTLETORDER") = (intListCnt - Integer.Parse(WW_ListValue) + 1)
                End If

            Case "FILLINGPOINT"             '(一覧)充填ポイント
                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)

                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "LOADINGOUTLETORDER"       '(一覧)積込出線順
                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)

                '入力された値が""(空文字)の場合
                If WW_ListValue = "" Then
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("LOADINGIRILINEORDER") = ""
                    '入力された値が0、または一覧の件数より大きい場合
                ElseIf Integer.Parse(WW_ListValue) = 0 _
                        OrElse Integer.Parse(WW_ListValue) > intListCnt Then
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("LOADINGIRILINEORDER") = ""
                Else
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("LOADINGIRILINEORDER") = (intListCnt - Integer.Parse(WW_ListValue) + 1)
                End If

            'Case "LOADINGIRILINETRAINNO"    '(一覧)積込入線列車番号
            '    updHeader.Item(WF_FIELD.Value) = WW_ListValue
            '    updHeader.Item("LOADINGIRILINETRAINNAME") = ""

            'Case "LOADINGOUTLETTRAINNO"     '(一覧)積込出線列車番号
            '    updHeader.Item(WF_FIELD.Value) = WW_ListValue
            '    updHeader.Item("LOADINGOUTLETTRAINNAME") = ""

            Case "LINE"                     '(一覧)回線を一覧に設定
                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

                '入力された値が""(空文字)の場合
                If WW_ListValue = "" Then
                    '入線列車番号
                    updHeader.Item("LOADINGIRILINETRAINNO") = ""
                    '入線列車名
                    updHeader.Item("LOADINGIRILINETRAINNAME") = ""
                    '出線列車番号
                    updHeader.Item("LOADINGOUTLETTRAINNO") = ""
                    '出線列車名
                    updHeader.Item("LOADINGOUTLETTRAINNAME") = ""
                    Exit Select
                End If

                '〇営業所配下情報を取得・設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    '〇 画面(受注営業所).テキストボックスが未設定
                    If Me.TxtOrderOffice.Text = "" Then
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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "CARSAMOUNT"            '(一覧)数量
                'updHeader.Item(WF_FIELD.Value) = WW_ListValue

                Dim regChkAmount As New Regex("^(?<seisu>(\d*))\.*(?<syosu>(\d*))$", RegexOptions.Singleline)
                Dim strSeisu As String  '整数部取得
                Dim strSyosu As String  '小数部取得

                Try
                    strSeisu = regChkAmount.Match(WW_ListValue).Result("${seisu}")
                    strSyosu = regChkAmount.Match(WW_ListValue).Result("${syosu}")
                    If strSyosu.Length > 0 _
                    OrElse strSeisu.Length <> 5 Then
                        'updHeader.Item(WF_FIELD.Value) = strSeisu.Substring(0, strSeisu.Length) & "." & "000"
                        updHeader.Item(WF_FIELD.Value) = "0.000"
                        Exit Select

                    End If

                    updHeader.Item(WF_FIELD.Value) = strSeisu.Substring(0, 2) & "." & strSeisu.Substring(2, 3)

                Catch ex As Exception
                    updHeader.Item(WF_FIELD.Value) = "0.000"
                    Exit Select

                End Try

            Case "JOINT"                 '(一覧)ジョイント先
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "CHANGETRAINNO",        '(一覧)本線列車番号変更
                 "SECONDARRSTATIONNAME", '(一覧)第2着駅
                 "SECONDCONSIGNEENAME",  '(一覧)第2荷受人
                 "CHANGERETSTATIONNAME"  '(一覧)空車着駅(変更)
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

    End Sub

    ''' <summary>
    ''' リスト変更時処理(タブ「費用入力」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListChange_TAB4()
        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = OIT0003tbl_tab4.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea4.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "APPLYCHARGESUM"       '(一覧)金額
                updHeader.Item(WF_FIELD.Value) = "￥" + String.Format("{0:#,0}", Integer.Parse(WW_ListValue))
                '税額(消費税)
                updHeader.Item("CONSUMPTIONTAX") = "￥" + String.Format("{0:#,0.00}", Integer.Parse(WW_ListValue) * work.WF_SEL_CONSUMPTIONTAX.Text)
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)

    End Sub
#End Region

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
            Case 2
                'タンク車明細
                WF_Dtab03.CssClass = "selected"
                Me.WW_InitializeTAB3 = True
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

                Case "STACKING"         '積置きフラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STACKING, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "STACKING"))

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
                        If Me.TxtOrderOffice.Text = "" Then
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
    ''' <param name="SQLcon">SQL接続文字</param>
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
            & "        ( ORDERNO      , TRAINNO         , TRAINNAME       , ORDERYMD            , OFFICECODE , OFFICENAME" _
            & "        , ORDERTYPE    , SHIPPERSCODE    , SHIPPERSNAME    , BASECODE            , BASENAME" _
            & "        , CONSIGNEECODE, CONSIGNEENAME   , DEPSTATION      , DEPSTATIONNAME      , ARRSTATION , ARRSTATIONNAME" _
            & "        , RETSTATION   , RETSTATIONNAME  , CHANGERETSTATION, CHANGERETSTATIONNAME, ORDERSTATUS, ORDERINFO " _
            & "        , EMPTYTURNFLG , STACKINGFLG     , USEPROPRIETYFLG , CONTACTFLG          , RESULTFLG  , DELIVERYFLG   , DELIVERYCOUNT" _
            & "        , LODDATE      , DEPDATE         , ARRDATE" _
            & "        , ACCDATE      , EMPARRDATE      , ACTUALLODDATE   , ACTUALDEPDATE       , ACTUALARRDATE" _
            & "        , ACTUALACCDATE, ACTUALEMPARRDATE, RTANK           , HTANK               , TTANK" _
            & "        , MTTANK       , KTANK           , K3TANK          , K5TANK              , K10TANK" _
            & "        , LTANK        , ATANK           , OTHER1OTANK     , OTHER2OTANK         , OTHER3OTANK" _
            & "        , OTHER4OTANK  , OTHER5OTANK     , OTHER6OTANK     , OTHER7OTANK         , OTHER8OTANK" _
            & "        , OTHER9OTANK  , OTHER10OTANK    , TOTALTANK" _
            & "        , RTANKCH      , HTANKCH         , TTANKCH         , MTTANKCH            , KTANKCH" _
            & "        , K3TANKCH     , K5TANKCH        , K10TANKCH       , LTANKCH             , ATANKCH" _
            & "        , OTHER1OTANKCH, OTHER2OTANKCH   , OTHER3OTANKCH   , OTHER4OTANKCH       , OTHER5OTANKCH" _
            & "        , OTHER6OTANKCH, OTHER7OTANKCH   , OTHER8OTANKCH   , OTHER9OTANKCH       , OTHER10OTANKCH" _
            & "        , TOTALTANKCH" _
            & "        , TANKLINKNO   , KEIJYOYMD       , SALSE           , SALSETAX            , TOTALSALSE" _
            & "        , PAYMENT      , PAYMENTTAX      , TOTALPAYMENT" _
            & "        , RECEIVECOUNT , OTSENDSTATUS    , RESERVEDSTATUS  , TAKUSOUSTATUS" _
            & "        , DELFLG       , INITYMD         , INITUSER        , INITTERMID" _
            & "        , UPDYMD       , UPDUSER         , UPDTERMID       , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P93, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08, @P09, @P10" _
            & "        , @P11, @P12, @P13, @P14, @P15, @P16" _
            & "        , @P17, @P18, @P19, @P20, @P21, @P22" _
            & "        , @P95, @P92, @P23, @P96, @P97, @P94, @P98" _
            & "        , @P24, @P25, @P26" _
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
            & "        , @P80, @P81, @P82" _
            & "        , @P99, @P100, @P101, @P102" _
            & "        , @P83, @P84, @P85, @P86" _
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
            & "    , CHANGERETSTATION" _
            & "    , CHANGERETSTATIONNAME" _
            & "    , ORDERSTATUS" _
            & "    , ORDERINFO" _
            & "    , EMPTYTURNFLG" _
            & "    , STACKINGFLG" _
            & "    , USEPROPRIETYFLG" _
            & "    , CONTACTFLG" _
            & "    , RESULTFLG" _
            & "    , DELIVERYFLG" _
            & "    , DELIVERYCOUNT" _
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
            & "    , TANKLINKNO" _
            & "    , KEIJYOYMD" _
            & "    , SALSE" _
            & "    , SALSETAX" _
            & "    , TOTALSALSE" _
            & "    , PAYMENT" _
            & "    , PAYMENTTAX" _
            & "    , TOTALPAYMENT" _
            & "    , RECEIVECOUNT" _
            & "    , OTSENDSTATUS" _
            & "    , RESERVEDSTATUS" _
            & "    , TAKUSOUSTATUS" _
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
                Dim PARA95 As SqlParameter = SQLcmd.Parameters.Add("@P95", SqlDbType.NVarChar, 1)  '空回日報可否フラグ
                Dim PARA92 As SqlParameter = SQLcmd.Parameters.Add("@P92", SqlDbType.NVarChar, 1)  '積置可否フラグ
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)  '利用可否フラグ
                Dim PARA96 As SqlParameter = SQLcmd.Parameters.Add("@P96", SqlDbType.NVarChar, 1)  '手配連絡フラグ
                Dim PARA97 As SqlParameter = SQLcmd.Parameters.Add("@P97", SqlDbType.NVarChar, 1)  '結果受理フラグ
                Dim PARA94 As SqlParameter = SQLcmd.Parameters.Add("@P94", SqlDbType.NVarChar, 1)  '託送指示フラグ
                Dim PARA98 As SqlParameter = SQLcmd.Parameters.Add("@P98", SqlDbType.Int)          '託送指示送信回数
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
                Dim PARA99 As SqlParameter = SQLcmd.Parameters.Add("@P99", SqlDbType.Int)          'OT空回日報受信回数
                Dim PARA100 As SqlParameter = SQLcmd.Parameters.Add("@P100", SqlDbType.NVarChar, 1)  'OT発送日報送信状況
                Dim PARA101 As SqlParameter = SQLcmd.Parameters.Add("@P101", SqlDbType.NVarChar, 1)  '出荷予約ダウンロード状況
                Dim PARA102 As SqlParameter = SQLcmd.Parameters.Add("@P102", SqlDbType.NVarChar, 1)  '託送状ダウンロード状況
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
                    PARA02.Value = Me.TxtTrainNo.Text                 '本線列車
                    PARA93.Value = Me.TxtTrainName.Text               '本線列車名
                    PARA03.Value = WW_DATENOW                         '受注登録日
                    PARA04.Value = Me.TxtOrderOfficeCode.Text         '受注営業所コード
                    PARA05.Value = Me.TxtOrderOffice.Text             '受注営業所名
                    PARA06.Value = work.WF_SEL_PATTERNCODE.Text       '受注パターン
                    PARA07.Value = work.WF_SEL_SHIPPERSCODE.Text      '荷主コード
                    PARA08.Value = work.WF_SEL_SHIPPERSNAME.Text      '荷主名
                    PARA09.Value = work.WF_SEL_BASECODE.Text          '基地コード
                    PARA10.Value = work.WF_SEL_BASENAME.Text          '基地名
                    PARA11.Value = work.WF_SEL_CONSIGNEECODE.Text     '荷受人コード
                    PARA12.Value = work.WF_SEL_CONSIGNEENAME.Text     '荷受人名
                    PARA13.Value = Me.TxtDepstationCode.Text          '発駅コード
                    PARA14.Value = Me.LblDepstationName.Text          '発駅名
                    PARA15.Value = Me.TxtArrstationCode.Text          '着駅コード
                    PARA16.Value = Me.LblArrstationName.Text          '着駅名
                    PARA17.Value = ""                                 '空車着駅コード
                    PARA18.Value = ""                                 '空車着駅名
                    PARA19.Value = ""                                 '空車着駅コード(変更後)
                    PARA20.Value = ""                                 '空車着駅名(変更後)
                    'PARA19.Value = OIT0003row("CHANGERETSTATION")     '空車着駅コード(変更後)
                    'PARA20.Value = OIT0003row("CHANGERETSTATIONNAME") '空車着駅名(変更後)
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

                    PARA95.Value = "0"                                    '空回日報可否フラグ(0:未作成)

                    '〇 積込日 < 発日 の場合 
                    If WW_ORDERINFOFLG_10 = True Then
                        PARA92.Value = "1"                                '積置可否フラグ(1:積置あり)
                    Else
                        PARA92.Value = "2"                                '積置可否フラグ(2:積置なし)
                    End If

                    PARA23.Value = "1"                                    '利用可否フラグ(1:利用可能)
                    PARA96.Value = work.WF_SEL_CONTACTFLG.Text            '手配連絡フラグ(0:未連絡)
                    PARA97.Value = work.WF_SEL_RESULTFLG.Text             '結果受理フラグ(0:未受理)
                    PARA94.Value = work.WF_SEL_DELIVERYFLG.Text           '託送指示フラグ(0:未手配, 1:手配)
                    PARA98.Value = "0"                                    '託送指示送信回数

                    PARA24.Value = Me.TxtLoadingDate.Text                 '積込日（予定）
                    PARA25.Value = Me.TxtDepDate.Text                     '発日（予定）
                    PARA26.Value = Me.TxtArrDate.Text                     '積車着日（予定）
                    PARA27.Value = Me.TxtAccDate.Text                     '受入日（予定）
                    '空車着日（予定）
                    If Me.TxtEmparrDate.Text = "" Then
                        PARA28.Value = DBNull.Value
                    Else
                        PARA28.Value = Me.TxtEmparrDate.Text
                    End If

                    '積込日（実績）
                    If Me.TxtActualLoadingDate.Text = "" Then
                        PARA29.Value = DBNull.Value
                    Else
                        PARA29.Value = Me.TxtActualLoadingDate.Text
                    End If
                    '発日（実績）
                    If Me.TxtActualLoadingDate.Text = "" Then
                        PARA30.Value = DBNull.Value
                    Else
                        PARA30.Value = Me.TxtActualDepDate.Text
                    End If
                    '積車着日（実績）
                    If Me.TxtActualArrDate.Text = "" Then
                        PARA31.Value = DBNull.Value
                    Else
                        PARA31.Value = Me.TxtActualArrDate.Text
                    End If
                    '受入日（実績）
                    If Me.TxtActualAccDate.Text = "" Then
                        PARA32.Value = DBNull.Value
                    Else
                        PARA32.Value = Me.TxtActualAccDate.Text
                    End If
                    '空車着日（実績）
                    If Me.TxtActualEmparrDate.Text = "" Then
                        PARA33.Value = DBNull.Value
                    Else
                        PARA33.Value = Me.TxtActualEmparrDate.Text
                    End If

                    PARA34.Value = Me.TxtRTank.Text                      '車数（レギュラー）
                    PARA35.Value = Me.TxtHTank.Text                      '車数（ハイオク）
                    PARA36.Value = Me.TxtTTank.Text                      '車数（灯油）
                    PARA37.Value = Me.TxtMTTank.Text                     '車数（未添加灯油）
                    PARA38.Value = Me.TxtKTank.Text                      '車数（軽油）
                    PARA39.Value = Me.TxtK3Tank.Text                     '車数（３号軽油）
                    PARA40.Value = Me.TxtK5Tank.Text                     '車数（５号軽油）
                    PARA41.Value = Me.TxtK10Tank.Text                    '車数（１０号軽油）
                    PARA42.Value = Me.TxtLTank.Text                      '車数（LSA）
                    PARA43.Value = Me.TxtATank.Text                      '車数（A重油）
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
                    work.WF_SEL_TANKCARTOTAL.Text = Integer.Parse(Me.TxtRTank.Text) _
                                                    + Integer.Parse(Me.TxtHTank.Text) _
                                                    + Integer.Parse(Me.TxtTTank.Text) _
                                                    + Integer.Parse(Me.TxtMTTank.Text) _
                                                    + Integer.Parse(Me.TxtKTank.Text) _
                                                    + Integer.Parse(Me.TxtK3Tank.Text) _
                                                    + Integer.Parse(Me.TxtK5Tank.Text) _
                                                    + Integer.Parse(Me.TxtK10Tank.Text) _
                                                    + Integer.Parse(Me.TxtLTank.Text) _
                                                    + Integer.Parse(Me.TxtATank.Text)
                    PARA54.Value = work.WF_SEL_TANKCARTOTAL.Text

                    PARA55.Value = Integer.Parse(Me.TxtRTank_w.Text)   '変更後_車数（レギュラー）
                    PARA56.Value = Integer.Parse(Me.TxtHTank_w.Text)   '変更後_車数（ハイオク）
                    PARA57.Value = Integer.Parse(Me.TxtTTank_w.Text)   '変更後_車数（灯油）
                    PARA58.Value = Integer.Parse(Me.TxtMTTank_w.Text)  '変更後_車数（未添加灯油）
                    PARA59.Value = Integer.Parse(Me.TxtKTank_w.Text)   '変更後_車数（軽油）
                    PARA60.Value = Integer.Parse(Me.TxtK3Tank_w.Text)  '変更後_車数（３号軽油）
                    PARA61.Value = Integer.Parse(Me.TxtK5Tank_w.Text)  '変更後_車数（５号軽油）
                    PARA62.Value = Integer.Parse(Me.TxtK10Tank_w.Text) '変更後_車数（１０号軽油）
                    PARA63.Value = Integer.Parse(Me.TxtLTank_w.Text)   '変更後_車数（LSA）
                    PARA64.Value = Integer.Parse(Me.TxtATank_w.Text)   '変更後_車数（A重油）
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
                    work.WF_SEL_TANKCARTOTALCH.Text = Integer.Parse(Me.TxtRTank_w.Text) _
                                                    + Integer.Parse(Me.TxtHTank_w.Text) _
                                                    + Integer.Parse(Me.TxtTTank_w.Text) _
                                                    + Integer.Parse(Me.TxtMTTank_w.Text) _
                                                    + Integer.Parse(Me.TxtKTank_w.Text) _
                                                    + Integer.Parse(Me.TxtK3Tank_w.Text) _
                                                    + Integer.Parse(Me.TxtK5Tank_w.Text) _
                                                    + Integer.Parse(Me.TxtK10Tank_w.Text) _
                                                    + Integer.Parse(Me.TxtLTank_w.Text) _
                                                    + Integer.Parse(Me.TxtATank_w.Text)
                    PARA75.Value = Integer.Parse(work.WF_SEL_TANKCARTOTALCH.Text)

                    PARA76.Value = work.WF_SEL_LINK_LINKNO.Text       '貨車連結順序表№
                    PARA91.Value = DBNull.Value                '計上日
                    PARA77.Value = 0                                  '売上金額
                    PARA78.Value = 0                                  '売上消費税額
                    PARA79.Value = 0                                  '売上合計金額
                    PARA80.Value = 0                                  '支払金額
                    PARA81.Value = 0                                  '支払消費税額
                    PARA82.Value = 0                                  '支払合計金額

                    PARA99.Value = "0"                                'OT空回日報受信回数
                    PARA100.Value = "0"                               'OT発送日報送信状況
                    PARA101.Value = "0"                               '出荷予約ダウンロード状況
                    PARA102.Value = "0"                               '託送状ダウンロード状況

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

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注明細TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderDetail(ByVal SQLcon As SqlConnection)

        '○ ＤＢ削除
        Dim SQLTempTblStr As String =
          " DELETE FROM OIL.OIT0003_DETAIL WHERE ORDERNO = @P01 AND DELFLG = '1'; " _

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
            & "        SHIPORDER             = @P40, LINEORDER            = @P33, TANKNO        = @P03" _
            & "        , ORDERINFO           = @P37, STACKINGFLG          = @P41, SHIPPERSCODE  = @P23, SHIPPERSNAME = @P24" _
            & "        , OILCODE             = @P05, OILNAME              = @P34, ORDERINGTYPE  = @P35" _
            & "        , ORDERINGOILNAME     = @P36, RETURNDATETRAIN      = @P07, JOINTCODE     = @P39, JOINT        = @P08" _
            & "        , CHANGETRAINNO       = @P26, CHANGETRAINNAME      = @P38" _
            & "        , SECONDCONSIGNEECODE = @P27, SECONDCONSIGNEENAME  = @P28" _
            & "        , SECONDARRSTATION    = @P29, SECONDARRSTATIONNAME = @P30" _
            & "        , CHANGERETSTATION    = @P31, CHANGERETSTATIONNAME = @P32" _
            & "        , LOADINGIRILINEORDER = @P43, LOADINGOUTLETORDER   = @P44, ACTUALLODDATE = @P47" _
            & "        , SALSE               = @P09, SALSETAX             = @P10, TOTALSALSE    = @P11" _
            & "        , PAYMENT             = @P12, PAYMENTTAX           = @P13, TOTALPAYMENT  = @P14" _
            & "        , UPDYMD              = @P19, UPDUSER              = @P20" _
            & "        , UPDTERMID           = @P21, RECEIVEYMD           = @P22" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & "        AND DETAILNO     = @P02" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0003_DETAIL" _
            & "        ( ORDERNO              , DETAILNO               , SHIPORDER          , LINEORDER           , TANKNO" _
            & "        , KAMOKU               , STACKINGFLG            , FIRSTRETURNFLG     , AFTERRETURNFLG      , OTTRANSPORTFLG" _
            & "        , ORDERINFO            , SHIPPERSCODE           , SHIPPERSNAME" _
            & "        , OILCODE              , OILNAME                , ORDERINGTYPE       , ORDERINGOILNAME" _
            & "        , CARSNUMBER           , CARSAMOUNT             , RETURNDATETRAIN    , JOINTCODE           , JOINT" _
            & "        , CHANGETRAINNO        , CHANGETRAINNAME        , SECONDCONSIGNEECODE, SECONDCONSIGNEENAME" _
            & "        , SECONDARRSTATION     , SECONDARRSTATIONNAME   , CHANGERETSTATION   , CHANGERETSTATIONNAME" _
            & "        , LOADINGIRILINEORDER  , LOADINGOUTLETORDER     , ACTUALLODDATE" _
            & "        , RESERVEDNO           , OTSENDCOUNT            , DLRESERVEDCOUNT    , DLTAKUSOUCOUNT" _
            & "        , SALSE                , SALSETAX               , TOTALSALSE" _
            & "        , PAYMENT              , PAYMENTTAX             , TOTALPAYMENT" _
            & "        , DELFLG               , INITYMD                , INITUSER           , INITTERMID" _
            & "        , UPDYMD               , UPDUSER                , UPDTERMID          , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P40, @P33, @P03" _
            & "        , @P04, @P41, @P42, @P45, @P46" _
            & "        , @P37, @P23, @P24" _
            & "        , @P05, @P34, @P35, @P36" _
            & "        , @P06, @P25, @P07, @P39, @P08" _
            & "        , @P26, @P38, @P27, @P28" _
            & "        , @P29, @P30, @P31, @P32" _
            & "        , @P43, @P44, @P47" _
            & "        , @P48, @P49, @P50, @P51" _
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
            & "    , SHIPORDER" _
            & "    , LINEORDER" _
            & "    , TANKNO" _
            & "    , KAMOKU" _
            & "    , STACKINGFLG" _
            & "    , FIRSTRETURNFLG" _
            & "    , AFTERRETURNFLG" _
            & "    , OTTRANSPORTFLG" _
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
            & "    , JOINTCODE" _
            & "    , JOINT" _
            & "    , CHANGETRAINNO" _
            & "    , CHANGETRAINNAME" _
            & "    , SECONDCONSIGNEECODE" _
            & "    , SECONDCONSIGNEENAME" _
            & "    , SECONDARRSTATION" _
            & "    , SECONDARRSTATIONNAME" _
            & "    , CHANGERETSTATION" _
            & "    , CHANGERETSTATIONNAME" _
            & "    , LOADINGIRILINEORDER" _
            & "    , LOADINGOUTLETORDER" _
            & "    , RESERVEDNO" _
            & "    , OTSENDCOUNT" _
            & "    , DLRESERVEDCOUNT" _
            & "    , DLTAKUSOUCOUNT" _
            & "    , ACTUALLODDATE" _
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
            Using SQLTMPcmd As New SqlCommand(SQLTempTblStr, SQLcon),
                  SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)

                '★削除作成用
                Dim PARATMP01 As SqlParameter = SQLTMPcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                PARATMP01.Value = work.WF_SEL_ORDERNUMBER.Text

                '　削除実行
                SQLTMPcmd.ExecuteNonQuery()

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11)  '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)   '受注明細№
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.NVarChar, 2)   '発送順
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 2)   '貨物駅入線順
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 8)   'タンク車№
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 7)   '費用科目
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.NVarChar)      '積置可否フラグ
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", SqlDbType.NVarChar)      '先返し可否フラグ
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", SqlDbType.NVarChar)      '後返し可否フラグ
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", SqlDbType.NVarChar)      'OT輸送可否フラグ
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
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.NVarChar)      'ジョイントコード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 200) 'ジョイント
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 4)   '本線列車（変更後）
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.NVarChar, 4)   '本線列車名（変更後）
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 10)  '第2荷受人コード
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.NVarChar, 40)  '第2荷受人名
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 7)   '第2着駅コード
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 40)  '第2着駅名
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.NVarChar, 7)   '空車着駅コード（変更後）
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 40)  '空車着駅名（変更後）
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", SqlDbType.NVarChar, 2)   '積込入線順
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", SqlDbType.NVarChar, 2)   '積込出線順
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", SqlDbType.Date)          '積込日（実績）

                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", SqlDbType.NVarChar, 3)   '予約番号
                Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", SqlDbType.Int)           'OT発送日報送信回数
                Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", SqlDbType.Int)           '出荷予約ダウンロード回数
                Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", SqlDbType.Int)           '託送状ダウンロード回数

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
                    '発送順(★全角⇒半角変換)
                    'PARA40.Value = OIT0003row("SHIPORDER")
                    PARA40.Value = StrConv(OIT0003row("SHIPORDER"), VbStrConv.Narrow)
                    '貨物駅入線順(★全角⇒半角変換)
                    'PARA33.Value = OIT0003row("LINEORDER")
                    PARA33.Value = StrConv(OIT0003row("LINEORDER"), VbStrConv.Narrow)
                    PARA03.Value = OIT0003row("TANKNO")               'タンク車№
                    PARA04.Value = ""                                 '費用科目

                    'PARA41.Value = "2"                                '積置可否フラグ(1:積置あり 2:積置なし)
                    '# 積置可否フラグ(1:積置あり 2:積置なし)
                    If OIT0003row("STACKINGFLG") = "on" Then
                        PARA41.Value = "1"
                    Else
                        PARA41.Value = "2"
                    End If
                    PARA42.Value = "2"                                '先返し可否フラグ(1:先返しあり 2:先返しなし)
                    PARA45.Value = "2"                                '後返し可否フラグ(1:後返しあり 2:後返しなし)
                    PARA46.Value = "2"                                'OT輸送可否フラグ(1:OT輸送あり 2:OT輸送なし)

                    '# 積込日(実績)
                    If OIT0003row("ACTUALLODDATE") = "" Then
                        PARA47.Value = DBNull.Value
                    Else
                        PARA47.Value = OIT0003row("ACTUALLODDATE")
                    End If

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
                        PARA37.Value = OIT0003row("ORDERINFO")
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
                    'PARA39.Value = DBNull.Value                       'ジョイントコード
                    PARA39.Value = OIT0003row("JOINTCODE")            'ジョイントコード
                    'PARA08.Value = DBNull.Value                       'ジョイント
                    PARA08.Value = OIT0003row("JOINT")                'ジョイント
                    PARA26.Value = OIT0003row("CHANGETRAINNO")        '本線列車（変更後）
                    PARA38.Value = OIT0003row("CHANGETRAINNAME")      '本線列車名（変更後）
                    PARA27.Value = OIT0003row("SECONDCONSIGNEECODE")  '第2荷受人コード
                    PARA28.Value = OIT0003row("SECONDCONSIGNEENAME")  '第2荷受人名
                    PARA29.Value = OIT0003row("SECONDARRSTATION")     '第2着駅コード
                    PARA30.Value = OIT0003row("SECONDARRSTATIONNAME") '第2着駅名
                    PARA31.Value = OIT0003row("CHANGERETSTATION")     '空車着駅コード（変更後）
                    PARA32.Value = OIT0003row("CHANGERETSTATIONNAME") '空車着駅名（変更後）

                    '### 20200616 START((全体)No74対応) ######################################
                    '積込入線順
                    '    ★袖ヶ浦の場合
                    If (Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011203) _
                        AndAlso OIT0003row("LINEORDER") <> "" Then
                        '貨物駅入線順を積込入線順に設定
                        PARA43.Value = OIT0003row("LINEORDER")
                        '積込出線順に(明細数 - 積込入線順 + 1)設定
                        PARA44.Value = (OIT0003tbl.Rows.Count - Integer.Parse(OIT0003row("LINEORDER"))) + 1
                        '    '★五井・甲子の場合
                        'ElseIf Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011201 _
                        '    OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011202 Then
                        '    '積込入線順を設定
                        '    PARA43.Value = OIT0003row("LOADINGIRILINEORDER")
                        '    '積込出線順を設定
                        '    PARA44.Value = OIT0003row("LOADINGOUTLETORDER")
                    Else
                        PARA43.Value = ""
                        PARA44.Value = ""
                    End If
                    '### 20200616 END  ((全体)No74対応) ######################################

                    PARA48.Value = ""                                 '予約番号
                    PARA49.Value = "0"                                'OT発送日報送信回数
                    PARA50.Value = "0"                                '出荷予約ダウンロード回数
                    PARA51.Value = "0"                                '託送状ダウンロード回数

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D UPDATE_INSERT_ORDERDETAIL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D UPDATE_INSERT_ORDERDETAIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 貨車連結表TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
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

                '(予定)空車着日(前日)を取得
                Dim dtEDbefore As Date = Date.Parse(Me.TxtEmparrDate.Text).AddDays(-1)

                For Each OIT0003row As DataRow In OIT0003tbl_tab3.Select(Nothing, "LINEORDER DESC")

                    PARA01.Value = work.WF_SEL_LINKNO_ORDER.Text     '貨車連結順序表№
                    PARA02.Value = OIT0003row("DETAILNO")            '貨車連結順序表明細№

                    '利用可能日
                    '(実績)発日が入力されている場合
                    If Me.TxtActualDepDate.Text <> "" Then
                        '◯ (予定)(発日) = (実績)発日
                        If Me.TxtDepDate.Text = Me.TxtActualDepDate.Text Then
                            '◯ (予定)(発日) = (予定)空車着日
                            If Me.TxtDepDate.Text = Me.TxtEmparrDate.Text Then
                                '(予定)空車着日の日付を設定する。
                                PARA03.Value = Me.TxtEmparrDate.Text

                            Else
                                '(予定)空車着日(前日)の日付を設定する。
                                PARA03.Value = dtEDbefore

                            End If
                        Else
                            '(予定)空車着日の日付を設定する。
                            PARA03.Value = Me.TxtEmparrDate.Text

                        End If
                    Else
                        PARA03.Value = WW_DATENOW.AddDays(1)
                    End If

                    '★(一覧)タンク車№がOT本社、または在日米軍のリース車かチェック
                    WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                    WW_FixvalueMasterSearch("ZZ", "TANKNO_OTCHECK", OIT0003row("TANKNO"), WW_GetValue)

                    'ステータス
                    '(実績)発日が入力されている場合、
                    'かつ(一覧)タンク車№がOT本社、または在日米軍のリース車ではない場合
                    If Me.TxtActualDepDate.Text <> "" AndAlso WW_GetValue(0) = "" Then
                        '◯ (予定)(発日) = (実績)発日
                        If Me.TxtDepDate.Text = Me.TxtActualDepDate.Text Then
                            '◯ (予定)空車着日(前日) = (実績)発日
                            '　 または、(予定)空車着日(当日) = (実績)発日
                            If dtEDbefore = Date.Parse(Me.TxtActualDepDate.Text) _
                            OrElse Date.Parse(Me.TxtEmparrDate.Text) = Date.Parse(Me.TxtActualDepDate.Text) Then
                                'ステータス(1:利用可, 2:利用不可)
                                PARA04.Value = "1"

                                '◯ (予定)空車着日(前日) = 当日
                                '　 または、(予定)空車着日(当日) = 当日
                            ElseIf dtEDbefore = Date.Today _
                                OrElse Date.Parse(Me.TxtEmparrDate.Text) = Date.Today Then
                                'ステータス(1:利用可, 2:利用不可)
                                PARA04.Value = "1"

                            Else
                                'ステータス(1:利用可, 2:利用不可)
                                PARA04.Value = "2"

                            End If
                        Else
                            'ステータス(1:利用可, 2:利用不可)
                            PARA04.Value = "2"

                        End If
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
                    'PARA17.Value = iNewLineOrder                      '入線順
                    PARA17.Value = OIT0003row("LINEORDER")           '入線順

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

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (受注TBL)タンク車数更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
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
                PARA2.Value = Me.TxtOrderOfficeCode.Text
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
                PARAUP02.Value = Me.TxtOrderOfficeCode.Text
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
                Me.TxtHTank_c.Text = "0"
                Me.TxtRTank_c.Text = "0"
                Me.TxtTTank_c.Text = "0"
                Me.TxtMTTank_c.Text = "0"
                Me.TxtKTank_c.Text = "0"
                Me.TxtK3Tank_c.Text = "0"
                Me.TxtK5Tank_c.Text = "0"
                Me.TxtK10Tank_c.Text = "0"
                Me.TxtLTank_c.Text = "0"
                Me.TxtATank_c.Text = "0"

                Me.TxtHTank_w.Text = "0"
                Me.TxtRTank_w.Text = "0"
                Me.TxtTTank_w.Text = "0"
                Me.TxtMTTank_w.Text = "0"
                Me.TxtKTank_w.Text = "0"
                Me.TxtK3Tank_w.Text = "0"
                Me.TxtK5Tank_w.Text = "0"
                Me.TxtK10Tank_w.Text = "0"
                Me.TxtLTank_w.Text = "0"
                Me.TxtATank_w.Text = "0"
                Me.TxtTotalCnt_w.Text = "0"

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
                        WW_FixvalueMasterSearch(Master.USER_ORG, "TRAINNUMBER_FIND", Me.TxtTrainName.Text, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TRAINNUMBER_FIND", Me.TxtTrainName.Text, WW_GetValue)
                    End If
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER_FIND", Me.TxtTrainName.Text, WW_GetValue)
                End If
                'WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER", TxtTrainNo.Text, WW_GetValue)

                For Each OIT0003UPDrow As DataRow In OIT0003WKtbl.Rows

                    Select Case OIT0003UPDrow("OILCODE")
                        Case BaseDllConst.CONST_HTank
                            PARAUP11.Value = OIT0003UPDrow("CNT")
                            Me.TxtHTank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtHTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_RTank
                            PARAUP12.Value = OIT0003UPDrow("CNT")
                            Me.TxtRTank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtRTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_TTank
                            PARAUP13.Value = OIT0003UPDrow("CNT")
                            Me.TxtTTank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtTTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_MTTank
                            PARAUP14.Value = OIT0003UPDrow("CNT")
                            Me.TxtMTTank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtMTTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_KTank1, BaseDllConst.CONST_KTank2
                            PARAUP15.Value = OIT0003UPDrow("CNT")
                            Me.TxtKTank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtKTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_K3Tank1, BaseDllConst.CONST_K3Tank2
                            PARAUP16.Value = OIT0003UPDrow("CNT")
                            Me.TxtK3Tank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtK3Tank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_K5Tank
                            PARAUP17.Value = OIT0003UPDrow("CNT")
                            Me.TxtK5Tank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtK5Tank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_K10Tank
                            PARAUP18.Value = OIT0003UPDrow("CNT")
                            Me.TxtK10Tank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtK10Tank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_LTank1, BaseDllConst.CONST_LTank2
                            PARAUP19.Value = OIT0003UPDrow("CNT")
                            Me.TxtLTank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtLTank_w.Text = OIT0003UPDrow("CNT")
                        Case BaseDllConst.CONST_ATank
                            PARAUP20.Value = OIT0003UPDrow("CNT")
                            Me.TxtATank_c.Text = OIT0003UPDrow("CNT")
                            Me.TxtATank_w.Text = OIT0003UPDrow("CNT")
                    End Select

                    i += OIT0003UPDrow("CNT")
                    Me.TxtTotal_c.Text = i
                    Me.TxtTotalCnt_w.Text = i
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

                '◯空回日報経由での作成ではない場合
                '　「車数」に「割当後の車数」と同様な件数を更新
                If work.WF_SEL_EMPTYTURNFLG.Text = "0" Then
                    WW_UpdateNotETDViaOrderTankCnt(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                End If

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
    ''' <param name="SQLcon">SQL接続</param>
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

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

#End Region

#Region "タブ「タンク車明細」各テーブル更新"
    ''' <summary>
    ''' 受注TBL更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
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
                    & "        TANKLINKNOMADE   = @P12, " _
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

            If Me.TxtActualLoadingDate.Text = "" Then
                PARA03.Value = DBNull.Value
            Else
                PARA03.Value = Date.Parse(Me.TxtActualLoadingDate.Text)
            End If
            If Me.TxtActualDepDate.Text = "" Then
                PARA04.Value = DBNull.Value
            Else
                PARA04.Value = Date.Parse(Me.TxtActualDepDate.Text)
            End If
            If Me.TxtActualArrDate.Text = "" Then
                PARA05.Value = DBNull.Value
            Else
                PARA05.Value = Date.Parse(Me.TxtActualArrDate.Text)
            End If
            If Me.TxtActualAccDate.Text = "" Then
                PARA06.Value = DBNull.Value
            Else
                PARA06.Value = Date.Parse(Me.TxtActualAccDate.Text)
            End If
            If Me.TxtActualEmparrDate.Text = "" Then
                PARA07.Value = DBNull.Value
            Else
                PARA07.Value = Date.Parse(Me.TxtActualEmparrDate.Text)
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
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderDetail_TAB3(ByVal SQLcon As SqlConnection)

        Try
            '更新SQL文･･･受注明細TBLの各項目をを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0003_DETAIL " _
                    & "    SET SHIPORDER            = @P27, " _
                    & "        CARSAMOUNT           = @P04, " _
                    & "        JOINTCODE            = @P23, " _
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
                    & "        CHANGERETSTATION     = @P16, " _
                    & "        CHANGERETSTATIONNAME = @P17, " _
                    & "        STACKINGFLG          = @P24, " _
                    & "        FIRSTRETURNFLG       = @P25, " _
                    & "        AFTERRETURNFLG       = @P26, " _
                    & "        OTTRANSPORTFLG       = @P28, " _
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
            Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar)  '発送順
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Decimal)   '数量
            Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar)  'ジョイントコード
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
            Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar)  '積置可否フラグ
            Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.NVarChar)  '先返し可否フラグ
            Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.NVarChar)  '後返し可否フラグ
            Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar)  'OT輸送可否フラグ

            Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.DateTime)  '更新年月日
            Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar)  '更新ユーザーＩＤ
            Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.NVarChar)  '更新端末
            Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.DateTime)  '集信日時

            For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                PARA01.Value = OIT0003tab3row("ORDERNO")
                PARA02.Value = OIT0003tab3row("DETAILNO")
                PARA03.Value = C_DELETE_FLG.DELETE
                '発送順(★全角⇒半角変換)
                'PARA27.Value = OIT0003tab3row("SHIPORDER")
                PARA27.Value = StrConv(OIT0003tab3row("SHIPORDER"), VbStrConv.Narrow)

                Try
                    PARA04.Value = Decimal.Parse(OIT0003tab3row("CARSAMOUNT"))
                Catch ex As Exception
                    PARA04.Value = "0"
                End Try
                PARA23.Value = OIT0003tab3row("JOINTCODE")
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
                PARA16.Value = OIT0003tab3row("CHANGERETSTATION")
                PARA17.Value = OIT0003tab3row("CHANGERETSTATIONNAME")

                '# 積置可否フラグ(1:積置あり 2:積置なし)
                If OIT0003tab3row("STACKINGFLG") = "on" Then
                    PARA24.Value = "1"
                Else
                    PARA24.Value = "2"
                End If
                '# 先返し可否フラグ(1:先返しあり 2:先返しなし)
                If OIT0003tab3row("FIRSTRETURNFLG") = "on" Then
                    PARA25.Value = "1"
                Else
                    PARA25.Value = "2"
                End If
                '### 20200622 START((全体)No87対応) ######################################
                '# 後返し可否フラグ(1:後返しあり 2:後返しなし)
                If OIT0003tab3row("AFTERRETURNFLG") = "on" Then
                    PARA26.Value = "1"
                Else
                    PARA26.Value = "2"
                End If
                '### 20200622 END  ((全体)No87対応) ######################################
                '### 20200717 START((全体)No112対応) #####################################
                '# OT輸送可否フラグ(1:OT輸送あり 2:OT輸送なし)
                If OIT0003tab3row("OTTRANSPORTFLG") = "on" Then
                    PARA28.Value = "1"
                Else
                    PARA28.Value = "2"
                End If
                '### 20200717 END  ((全体)No112対応) #####################################

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

#Region "タブ「費用入力」各テーブル更新"
    ''' <summary>
    ''' 受注費用TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderBilling(ByVal SQLcon As SqlConnection)

        '○ ＤＢ削除
        Dim SQLTempTblStr As String =
          " DELETE FROM OIL.OIT0010_ORDERBILLING WHERE BILLINGNO = @P01; " _

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0010_ORDERBILLING" _
            & "    WHERE" _
            & "        BILLINGNO          = @P01" _
            & "    AND BILLINGDETAILNO    = @P02" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0010_ORDERBILLING" _
            & "    SET" _
            & "        KEIJYOYMD           = @P03  , AKAKURO           = @P04" _
            & "        , ORDERNO           = @P05  , TANKNO            = @P06" _
            & "        , OFFICECODE        = @P07  , OFFICENAME        = @P08, CALCACCOUNT = @P09" _
            & "        , ACCOUNTCODE       = @P10  , ACCOUNTNAME       = @P11" _
            & "        , SEGMENTCODE       = @P12  , SEGMENTNAME       = @P13" _
            & "        , SEGMENTBRANCHCODE = @P14  , SEGMENTBRANCHNAME = @P15" _
            & "        , ACCOUNTTYPE       = @P16  , ACCOUNTTYPENAME   = @P17" _
            & "        , QUANTITY          = @P18" _
            & "        , UNITPRICE         = @P19  , AMOUNT            = @P20" _
            & "        , TAX               = @P21  , INVOICECODE       = @P22" _
            & "        , INVOICENAME       = @P23  , INVOICEDEPTNAME   = @P24" _
            & "        , PAYEECODE         = @P25  , PAYEENAME         = @P26" _
            & "        , PAYEEDEPTNAME     = @P27  , TEKIYOU           = @P28" _
            & "        , BIKOU             = @P29  , DELFLG            = @P30" _
            & "        , UPDYMD            = @P34  , UPDUSER           = @P35" _
            & "        , UPDTERMID         = @P36  , RECEIVEYMD        = @P37" _
            & "    WHERE" _
            & "        BILLINGNO       = @P01" _
            & "    AND BILLINGDETAILNO = @P02" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0010_ORDERBILLING" _
            & "        ( BILLINGNO        , BILLINGDETAILNO  , KEIJYOYMD      , AKAKURO        , ORDERNO" _
            & "        , TANKNO           , OFFICECODE       , OFFICENAME     , CALCACCOUNT" _
            & "        , ACCOUNTCODE      , ACCOUNTNAME      , SEGMENTCODE    , SEGMENTNAME" _
            & "        , SEGMENTBRANCHCODE, SEGMENTBRANCHNAME, ACCOUNTTYPE    , ACCOUNTTYPENAME" _
            & "        , QUANTITY         , UNITPRICE        , AMOUNT         , TAX" _
            & "        , INVOICECODE      , INVOICENAME      , INVOICEDEPTNAME" _
            & "        , PAYEECODE        , PAYEENAME        , PAYEEDEPTNAME" _
            & "        , TEKIYOU          , BIKOU            , DELFLG" _
            & "        , INITYMD          , INITUSER         , INITTERMID" _
            & "        , UPDYMD           , UPDUSER          , UPDTERMID      , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08, @P09" _
            & "        , @P10, @P11, @P12, @P13" _
            & "        , @P14, @P15, @P16, @P17" _
            & "        , @P18, @P19, @P20, @P21" _
            & "        , @P22, @P23, @P24" _
            & "        , @P25, @P26, @P27" _
            & "        , @P28, @P29, @P30" _
            & "        , @P31, @P32, @P33" _
            & "        , @P34, @P35, @P36, @P37) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    BILLINGNO" _
            & "    , BILLINGDETAILNO" _
            & "    , KEIJYOYMD" _
            & "    , AKAKURO" _
            & "    , ORDERNO" _
            & "    , TANKNO" _
            & "    , OFFICECODE" _
            & "    , OFFICENAME" _
            & "    , CALCACCOUNT" _
            & "    , ACCOUNTCODE" _
            & "    , ACCOUNTNAME" _
            & "    , SEGMENTCODE" _
            & "    , SEGMENTNAME" _
            & "    , SEGMENTBRANCHCODE" _
            & "    , SEGMENTBRANCHNAME" _
            & "    , ACCOUNTTYPE" _
            & "    , ACCOUNTTYPENAME" _
            & "    , QUANTITY" _
            & "    , UNITPRICE" _
            & "    , AMOUNT" _
            & "    , TAX" _
            & "    , INVOICECODE" _
            & "    , INVOICENAME" _
            & "    , INVOICEDEPTNAME" _
            & "    , PAYEECODE" _
            & "    , PAYEENAME" _
            & "    , PAYEEDEPTNAME" _
            & "    , TEKIYOU" _
            & "    , BIKOU" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & " FROM" _
            & "    OIL.OIT0010_ORDERBILLING" _
            & " WHERE" _
            & "        BILLINGNO       = @P01" _
            & "    AND BILLINGDETAILNO = @P02"

        Try
            Using SQLTMPcmd As New SqlCommand(SQLTempTblStr, SQLcon),
                  SQLcmd As New SqlCommand(SQLStr, SQLcon),
                  SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)

                '★削除作成用
                Dim PARATMP01 As SqlParameter = SQLTMPcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '支払請求№
                PARATMP01.Value = work.WF_SEL_BILLINGNO.Text

                '　削除実行
                SQLTMPcmd.ExecuteNonQuery()

                '★追加・更新用
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11)  '支払請求№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)   '支払請求明細№
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)          '計上年月日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)   '赤黒区分
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 11)  '受注№
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 8)   'タンク車№
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 6)   '受注営業所コード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 20)  '受注営業所名
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 1)   '計算科目
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 8)   '科目コード
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 40)  '科目名
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 5)   'セグメント
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 40)  'セグメント名
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 2)   'セグメント枝番
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 40)  'セグメント枝番名
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 2)   '科目区分
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 40)  '科目区分名
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.Decimal)       '数量
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Money)         '単価
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.Money)         '金額
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Money)         '税額
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 10)  '請求先コード
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 40)  '請求先名
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 40)  '請求先部門名
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 10)  '支払先コード
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 40)  '支払先名
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 40)  '支払先部門名
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.NVarChar)      '摘要
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar)      '備考
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 1)   '削除フラグ
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.DateTime)      '登録年月日
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 20)  '登録ユーザーＩＤ
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 20)  '登録端末
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.DateTime)      '更新年月日
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.NVarChar, 20)  '更新ユーザーＩＤ
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.NVarChar, 20)  '更新端末
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.DateTime)      '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '支払請求№
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '支払請求明細№

                Dim WW_DATENOW As DateTime = Date.Now

                '支払請求№チェック
                If work.WF_SEL_BILLINGNO.Text = "" Then
                    '★新規支払請求№の取得
                    Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
                    WW_FixvalueMasterSearch("", "NEWBILLINGNOGET", "", WW_GetValue)
                    work.WF_SEL_BILLINGNO.Text = WW_GetValue(0)
                End If

                Dim i As Integer = 0
                Dim reg As New Regex("[^0-9^.]")
                For Each OIT0003tab4row As DataRow In OIT0003tbl_tab4.Rows
                    i += 1
                    'DB更新
                    PARA01.Value = work.WF_SEL_BILLINGNO.Text          '支払請求№
                    PARA02.Value = i.ToString("000")                   '支払請求明細№
                    PARA03.Value = OIT0003tab4row("KEIJYOYMD")         '計上年月日
                    PARA04.Value = OIT0003tab4row("AKAKURO")           '赤黒区分
                    PARA05.Value = OIT0003tab4row("ORDERNO")           '受注№
                    PARA06.Value = ""                                  'タンク車№
                    PARA07.Value = OIT0003tab4row("OFFICECODE")        '受注営業所コード
                    PARA08.Value = OIT0003tab4row("OFFICENAME")        '受注営業所名
                    PARA09.Value = OIT0003tab4row("CALCACCOUNT")       '計算科目
                    PARA10.Value = OIT0003tab4row("ACCOUNTCODE")       '科目コード
                    PARA11.Value = OIT0003tab4row("ACCOUNTNAME")       '科目名
                    PARA12.Value = OIT0003tab4row("SEGMENTCODE")       'セグメント
                    PARA13.Value = OIT0003tab4row("SEGMENTNAME")       'セグメント名
                    PARA14.Value = OIT0003tab4row("BREAKDOWNCODE")     'セグメント枝番
                    PARA15.Value = OIT0003tab4row("BREAKDOWN")         'セグメント枝番名
                    PARA16.Value = OIT0003tab4row("CALCKBN")           '科目区分
                    PARA17.Value = OIT0003tab4row("CALCKBNNAME")       '科目区分名

                    PARA18.Value = reg.Replace(OIT0003tab4row("CARSAMOUNT"), "")        '数量
                    '単価
                    If OIT0003tab4row("APPLYCHARGE") = "" Then
                        OIT0003tab4row("APPLYCHARGE") = 0
                    Else
                        PARA19.Value = Replace(OIT0003tab4row("APPLYCHARGE"), "￥", "")
                    End If
                    PARA20.Value = Replace(OIT0003tab4row("APPLYCHARGESUM"), "￥", "")    '金額
                    PARA21.Value = Replace(OIT0003tab4row("CONSUMPTIONTAX"), "￥", "")    '税額
                    PARA22.Value = OIT0003tab4row("INVOICECODE")       '請求先コード
                    PARA23.Value = OIT0003tab4row("INVOICENAME")       '請求先名
                    PARA24.Value = OIT0003tab4row("INVOICEDEPTNAME")   '請求先部門名
                    PARA25.Value = OIT0003tab4row("PAYEECODE")         '支払先コード
                    PARA26.Value = OIT0003tab4row("PAYEENAME")         '支払先名
                    PARA27.Value = OIT0003tab4row("PAYEEDEPTNAME")     '支払先部門名
                    'PARA28.Value = OIT0003tab4row("TEKIYOU")           '摘要
                    'PARA29.Value = OIT0003tab4row("BIKOU")             '備考
                    PARA28.Value = ""             '摘要
                    PARA29.Value = ""             '備考

                    '削除フラグ
                    If OIT0003tab4row("HIDDEN") = "1" Then
                        PARA30.Value = "1"
                    Else
                        PARA30.Value = "0"
                    End If

                    PARA31.Value = WW_DATENOW                          '登録年月日
                    PARA32.Value = Master.USERID                       '登録ユーザーID
                    PARA33.Value = Master.USERTERMID                   '登録端末
                    PARA34.Value = WW_DATENOW                          '更新年月日
                    PARA35.Value = Master.USERID                       '更新ユーザーID
                    PARA36.Value = Master.USERTERMID                   '更新端末
                    PARA37.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()
                    OIT0003tab4row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = OIT0003tab4row("BILLINGNO")
                    JPARA02.Value = OIT0003tab4row("BILLINGDETAILNO")

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0003UPDtbl_tab4) Then
                            OIT0003UPDtbl_tab4 = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0003UPDtbl_tab4.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0003UPDtbl_tab4.Clear()
                        OIT0003UPDtbl_tab4.Load(SQLdr)
                    End Using

                    For Each OIT0003UPDrow As DataRow In OIT0003UPDtbl_tab4.Rows
                        CS0020JOURNAL.TABLENM = "OIT0003D_TAB4"
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
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D UPDATE_ORDERBILLING")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D UPDATE_ORDERBILLING"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 受注TBL更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrder_TAB4(ByVal SQLcon As SqlConnection)

        Try
            '更新SQL文･･･受注TBLの各項目をを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0002_ORDER " _
                    & "    SET BILLINGNO        = @P03, " _
                    & "        KEIJYOYMD        = @P04, " _
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
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)  '支払請求№
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)      '計上日
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)  '更新年月日
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)  '更新ユーザーＩＤ
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)  '更新端末
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)  '集信日時

            PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
            PARA02.Value = C_DELETE_FLG.DELETE
            PARA03.Value = work.WF_SEL_BILLINGNO.Text
            If work.WF_SEL_KEIJYOYMD.Text = "" Then
                PARA04.Value = Me.TxtActualLoadingDate.Text
            Else
                PARA04.Value = work.WF_SEL_KEIJYOYMD.Text
            End If

            PARA08.Value = Date.Now
            PARA09.Value = Master.USERID
            PARA10.Value = Master.USERTERMID
            PARA11.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_ORDER_TAB4 UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_ORDER_TAB4 UPDATE"
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
    ''' <param name="SQLcon">SQL接続</param>
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
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')   　            AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0002.EMPTYTURNFLG), '')   　       AS EMPTYTURNFLG" _
            & " , ISNULL(RTRIM(OIT0002.STACKINGFLG), '')   　        AS STACKINGFLG" _
            & " , ''                                                 AS STACKINGNAME" _
            & " , ISNULL(RTRIM(OIT0002.USEPROPRIETYFLG), '')   　    AS USEPROPRIETYFLG" _
            & " , ISNULL(RTRIM(OIT0002.CONTACTFLG), '')   　         AS CONTACTFLG" _
            & " , ISNULL(RTRIM(OIT0002.RESULTFLG), '')   　          AS RESULTFLG" _
            & " , ISNULL(RTRIM(OIT0002.DELIVERYFLG), '')   　        AS DELIVERYFLG" _
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
            & " , ISNULL(RTRIM(OIT0002.CHANGERETSTATION), '')        AS CHANGERETSTATION" _
            & " , ISNULL(RTRIM(OIT0002.CHANGERETSTATIONNAME), '')    AS CHANGERETSTATIONNAME" _
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
            & " , ISNULL(RTRIM(OIT0002.TANKLINKNO), '')              AS TANKLINKNO" _
            & " , ISNULL(RTRIM(OIT0002.TANKLINKNOMADE), '')          AS TANKLINKNOMADE" _
            & " , ISNULL(RTRIM(OIT0002.BILLINGNO), '')               AS BILLINGNO" _
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
            & " WHERE OIT0002.LODDATE    >= @P2" _
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

                    '◯名称取得
                    '積置きフラグ
                    CODENAME_get("STACKING", OIT0003Wkrow("STACKINGFLG"), OIT0003Wkrow("STACKINGNAME"), WW_RTN_SW)

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
                Me.TxtHTank_c.Text = "0"
                Me.TxtRTank_c.Text = "0"
                Me.TxtTTank_c.Text = "0"
                Me.TxtMTTank_c.Text = "0"
                Me.TxtKTank_c.Text = "0"
                Me.TxtK3Tank_c.Text = "0"
                Me.TxtK5Tank_c.Text = "0"
                Me.TxtK10Tank_c.Text = "0"
                Me.TxtLTank_c.Text = "0"
                Me.TxtATank_c.Text = "0"
                Me.TxtTotal_c.Text = "0"
                '〇 積込数量(kl)
                Me.TxtHTank_c2.Text = "0"
                Me.TxtRTank_c2.Text = "0"
                Me.TxtTTank_c2.Text = "0"
                Me.TxtMTTank_c2.Text = "0"
                Me.TxtKTank_c2.Text = "0"
                Me.TxtK3Tank_c2.Text = "0"
                Me.TxtK5Tank_c2.Text = "0"
                Me.TxtK10Tank_c2.Text = "0"
                Me.TxtLTank_c2.Text = "0"
                Me.TxtATank_c2.Text = "0"
                Me.TxtTotalCnt_c2.Text = "0"

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
                    Me.TxtHTank_c.Text = OIT0003WKrow("HTANK")
                    Me.TxtRTank_c.Text = OIT0003WKrow("RTANK")
                    Me.TxtTTank_c.Text = OIT0003WKrow("TTANK")
                    Me.TxtMTTank_c.Text = OIT0003WKrow("MTTANK")
                    Me.TxtKTank_c.Text = OIT0003WKrow("KTANK")
                    Me.TxtK3Tank_c.Text = OIT0003WKrow("K3TANK")
                    Me.TxtK5Tank_c.Text = OIT0003WKrow("K5TANK")
                    Me.TxtK10Tank_c.Text = OIT0003WKrow("K10TANK")
                    Me.TxtLTank_c.Text = OIT0003WKrow("LTANK")
                    Me.TxtATank_c.Text = OIT0003WKrow("ATANK")
                    Me.TxtTotal_c.Text = OIT0003WKrow("TOTAL")
                    '〇 積込数量(kl)
                    Me.TxtHTank_c2.Text = OIT0003WKrow("HTANKCNT")
                    Me.TxtRTank_c2.Text = OIT0003WKrow("RTANKCNT")
                    Me.TxtTTank_c2.Text = OIT0003WKrow("TTANKCNT")
                    Me.TxtMTTank_c2.Text = OIT0003WKrow("MTTANKCNT")
                    Me.TxtKTank_c2.Text = OIT0003WKrow("KTANKCNT")
                    Me.TxtK3Tank_c2.Text = OIT0003WKrow("K3TANKCNT")
                    Me.TxtK5Tank_c2.Text = OIT0003WKrow("K5TANKCNT")
                    Me.TxtK10Tank_c2.Text = OIT0003WKrow("K10TANKCNT")
                    Me.TxtLTank_c2.Text = OIT0003WKrow("LTANKCNT")
                    Me.TxtATank_c2.Text = OIT0003WKrow("ATANKCNT")
                    Me.TxtTotalCnt_c2.Text = OIT0003WKrow("TOTALCNT")

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
    Protected Sub WW_UpdateOrderStatus(ByVal I_Value As String,
                                       Optional ByVal InitializeFlg As Boolean = False,
                                       Optional ByVal ReuseFlg As Boolean = False)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLの受注進行ステータス、及び貨車連結順序表№を更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0002_ORDER " _
                    & "    SET ORDERSTATUS = @P03, " _
                    & "        TANKLINKNO  = @P04, "

            '### 20200609 START(内部No178) #################################################
            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            If InitializeFlg = True Then
                '空回日報可否フラグ
                ' 0：未作成, 1：作成(空回日報から作成), 2：作成(在庫管理から作成)
                SQLStr &= String.Format("        EMPTYTURNFLG = '{0}', ", "0")
                '積置可否フラグ
                ' 1：積置あり, 2：積置なし
                SQLStr &= String.Format("        STACKINGFLG  = '{0}', ", "2")
                '利用可否フラグ
                ' 1：利用可, 2：利用不可
                SQLStr &= String.Format("        USEPROPRIETYFLG = '{0}', ", "1")
                '手配連絡フラグ
                ' 0：未連絡, 1：連絡
                SQLStr &= String.Format("        CONTACTFLG = '{0}', ", "0")
                '結果受理フラグ
                ' 0：未受理, 1：受理
                SQLStr &= String.Format("        RESULTFLG = '{0}', ", "0")
                '託送指示フラグ
                ' 0：未手配, 1：手配
                SQLStr &= String.Format("        DELIVERYFLG = '{0}', ", "0")
                '車数
                SQLStr &= String.Format("        RTANK = '{0}', ", "0")
                SQLStr &= String.Format("        HTANK = '{0}', ", "0")
                SQLStr &= String.Format("        TTANK = '{0}', ", "0")
                SQLStr &= String.Format("        MTTANK = '{0}', ", "0")
                SQLStr &= String.Format("        KTANK = '{0}', ", "0")
                SQLStr &= String.Format("        K3TANK = '{0}', ", "0")
                SQLStr &= String.Format("        K5TANK = '{0}', ", "0")
                SQLStr &= String.Format("        K10TANK = '{0}', ", "0")
                SQLStr &= String.Format("        LTANK = '{0}', ", "0")
                SQLStr &= String.Format("        ATANK = '{0}', ", "0")
                SQLStr &= String.Format("        TOTALTANK = '{0}', ", "0")
                '変更後_車数
                SQLStr &= String.Format("        RTANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        HTANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        TTANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        MTTANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        KTANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        K3TANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        K5TANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        K10TANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        LTANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        ATANKCH = '{0}', ", "0")
                SQLStr &= String.Format("        TOTALTANKCH = '{0}', ", "0")

                ''貨車連結順序表№
                'SQLStr &= String.Format("        TANKLINKNO = '{0}', ", "")
                ''作成_貨車連結順序表№
                'SQLStr &= String.Format("        TANKLINKNOMADE = '{0}', ", "")
                ''支払請求№
                'SQLStr &= String.Format("        BILLINGNO = '{0}', ", "")
            End If
            '### 20200609 END  (内部No178) #################################################

            '### 20200812 START(指摘票(全体)No121) #########################################
            If ReuseFlg = True Then
                '手配連絡フラグ
                ' 0：未連絡, 1：連絡
                SQLStr &= String.Format("        CONTACTFLG = '{0}', ", "0")
                '結果受理フラグ
                ' 0：未受理, 1：受理
                SQLStr &= String.Format("        RESULTFLG = '{0}', ", "0")
                '託送指示フラグ
                ' 0：未手配, 1：手配
                SQLStr &= String.Format("        DELIVERYFLG = '{0}', ", "0")
            End If
            '### 20200812 END  (指摘票(全体)No121) #########################################

            SQLStr &=
                      "        UPDYMD      = @P11, " _
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

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (受注TBL)受注情報更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderInfo(ByVal SQLcon As SqlConnection, ByVal I_TYPE As String, ByVal OIT0003row As DataRow)

        Try
            'DataBase接続文字
            'Dim SQLcon = CS0050SESSION.getConnection
            'SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr As String = ""
            '更新SQL文･･･受注TBLの受注情報を更新
            If I_TYPE = "1" Then
                SQLStr =
                " UPDATE OIL.OIT0002_ORDER " _
                & "    SET ORDERINFO   = @P04, " _
                & "        UPDYMD      = @P11, " _
                & "        UPDUSER     = @P12, " _
                & "        UPDTERMID   = @P13, " _
                & "        RECEIVEYMD  = @P14  " _
                & "  WHERE ORDERNO     = @P01  " _
                & "    AND DELFLG     <> @P03; "

                '更新SQL文･･･受注明細TBLの受注情報を更新
            ElseIf I_TYPE = "2" Then
                SQLStr =
                " UPDATE OIL.OIT0003_DETAIL " _
                & "    SET ORDERINFO   = @P04, " _
                & "        UPDYMD      = @P11, " _
                & "        UPDUSER     = @P12, " _
                & "        UPDTERMID   = @P13, " _
                & "        RECEIVEYMD  = @P14  " _
                & "  WHERE ORDERNO     = @P01  " _
                & "    AND DETAILNO    = @P02  " _
                & "    AND DELFLG     <> @P03; "

            End If

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

            PARA01.Value = OIT0003row("ORDERNO")
            PARA02.Value = OIT0003row("DETAILNO")
            PARA03.Value = C_DELETE_FLG.DELETE
            PARA04.Value = OIT0003row("ORDERINFO")

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_ORDERINFO UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_ORDERINFO UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (タンク車マスタTBL)の内容を更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateTankMaster(ByVal I_TANKNO As String,
                                      Optional ByVal I_JRINSPECTIONDATE As String = Nothing)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･タンク車マスタTBL更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIM0005_TANK " _
                    & "    SET "

            '○ 更新内容が指定されていれば追加する
            '次回交検年月日(JR）
            If Not String.IsNullOrEmpty(I_JRINSPECTIONDATE) Then
                SQLStr &= String.Format("        JRINSPECTIONDATE = '{0}', ", I_JRINSPECTIONDATE)
            End If

            SQLStr &=
                      "        UPDYMD         = @P11, " _
                    & "        UPDUSER        = @P12, " _
                    & "        UPDTERMID      = @P13, " _
                    & "        RECEIVEYMD     = @P14  " _
                    & "  WHERE TANKNUMBER     = @P01  " _
                    & "    AND DELFLG        <> @P02; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  'タンク車№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '削除フラグ

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA01.Value = I_TANKNO
            PARA02.Value = C_DELETE_FLG.DELETE

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TANKMASTER UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TANKMASTER UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try
    End Sub

    ''' <summary>
    ''' (タンク車所在TBL)の内容を更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateTankShozai(ByVal I_LOCATION As String,
                                      ByVal I_STATUS As String,
                                      ByVal I_KBN As String,
                                      Optional ByVal I_SITUATION As String = Nothing,
                                      Optional ByVal I_TANKNO As String = Nothing,
                                      Optional ByVal I_ORDERNO As String = Nothing,
                                      Optional ByVal I_EMPARRDATE As String = Nothing,
                                      Optional ByVal I_AEMPARRDATE As String = Nothing,
                                      Optional ByVal upEmparrDate As Boolean = False,
                                      Optional ByVal upActualEmparrDate As Boolean = False,
                                      Optional ByVal upLastOilCode As Boolean = False)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･タンク車所在TBL更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0005_SHOZAI " _
                    & "    SET "

            '○ 更新内容が指定されていれば追加する
            '所在地コード
            If Not String.IsNullOrEmpty(I_LOCATION) Then
                SQLStr &= String.Format("        LOCATIONCODE = '{0}', ", I_LOCATION)
            End If
            'タンク車状態コード
            If Not String.IsNullOrEmpty(I_STATUS) Then
                SQLStr &= String.Format("        TANKSTATUS   = '{0}', ", I_STATUS)
            End If
            '積車区分
            If Not String.IsNullOrEmpty(I_KBN) Then
                SQLStr &= String.Format("        LOADINGKBN   = '{0}', ", I_KBN)
            End If
            'タンク車状況コード
            If Not String.IsNullOrEmpty(I_SITUATION) Then
                SQLStr &= String.Format("        TANKSITUATION = '{0}', ", I_SITUATION)
            End If

            '★空車着日（予定）が未設定の場合は、オーダー中の空車着日（予定）を設定
            If String.IsNullOrEmpty(I_EMPARRDATE) Then I_EMPARRDATE = Me.TxtEmparrDate.Text
            '空車着日（予定）
            If upEmparrDate = True Then
                SQLStr &= String.Format("        EMPARRDATE   = '{0}', ", I_EMPARRDATE)
                SQLStr &= String.Format("        ACTUALEMPARRDATE   = {0}, ", "NULL")
            End If

            '★空車着日（実績）が未設定の場合は、オーダー中の空車着日（実績）を設定
            If String.IsNullOrEmpty(I_AEMPARRDATE) Then I_AEMPARRDATE = Me.TxtActualEmparrDate.Text
            '★受注Noが未設定の場合は、オーダー中の受注№を設定
            If String.IsNullOrEmpty(I_ORDERNO) Then I_ORDERNO = Me.TxtOrderNo.Text
            '空車着日（実績）
            If upActualEmparrDate = True Then
                If I_AEMPARRDATE = "" Then
                    SQLStr &= "        ACTUALEMPARRDATE   = NULL, "
                Else
                    SQLStr &= String.Format("        ACTUALEMPARRDATE   = '{0}', ", I_AEMPARRDATE)
                End If
                '### 20200618 START 受注での使用をリセットする対応 #########################################
                SQLStr &= String.Format("        USEORDERNO         = '{0}', ", "")
                '### 20200618 END   受注での使用をリセットする対応 #########################################
            Else
                '### 20200618 START 受注での使用を設定する対応 #############################################
                SQLStr &= String.Format("        USEORDERNO         = '{0}', ", I_ORDERNO)
                '### 20200618 END   受注での使用を設定する対応 #############################################
            End If
            '前回油種
            If upLastOilCode = True Then
                SQLStr &=
                          "        LASTOILCODE        = @P03, " _
                        & "        LASTOILNAME        = @P04, " _
                        & "        PREORDERINGTYPE    = @P05, " _
                        & "        PREORDERINGOILNAME = @P06, "
            End If

            SQLStr &=
                      "        UPDYMD         = @P11, " _
                    & "        UPDUSER        = @P12, " _
                    & "        UPDTERMID      = @P13, " _
                    & "        RECEIVEYMD     = @P14  " _
                    & "  WHERE TANKNUMBER     = @P01  " _
                    & "    AND TANKSITUATION <> '3' " _
                    & "    AND DELFLG        <> @P02 "

            '### 20200618 START 受注での使用をリセットする対応 #########################################
            '空車着日（実績）
            If upActualEmparrDate = True Then
                'SQLStr &=
                '      "    AND ISNULL(USEORDERNO, '')    <> ''; "
                SQLStr &= String.Format("    AND USEORDERNO = '{0}';", I_ORDERNO)
            Else
                SQLStr &=
                      "    AND (ISNULL(USEORDERNO, '')     = '' "
                SQLStr &= String.Format(" OR USEORDERNO = '{0}');", I_ORDERNO)
            End If
            '### 20200618 END   受注での使用をリセットする対応 #########################################

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  'タンク車№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '削除フラグ

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA02.Value = C_DELETE_FLG.DELETE

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            If I_TANKNO = "" Then
                '### ★前回油種の更新 ###############################
                If upLastOilCode = True Then
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)  '前回油種コード
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)  '前回油種名
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)  '前回油種区分(受発注用)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)  '前回油種名(受発注用)

                    '(一覧)で設定しているタンク車をKEYに前回油種を更新
                    For Each OIT0003row As DataRow In OIT0003tbl.Rows
                        PARA01.Value = OIT0003row("TANKNO")
                        PARA03.Value = OIT0003row("OILCODE")
                        PARA04.Value = OIT0003row("OILNAME")
                        PARA05.Value = OIT0003row("ORDERINGTYPE")
                        PARA06.Value = OIT0003row("ORDERINGOILNAME")
                        SQLcmd.ExecuteNonQuery()
                    Next

                Else
                    '(一覧)で設定しているタンク車をKEYに更新
                    For Each OIT0003row As DataRow In OIT0003tbl.Rows
                        PARA01.Value = OIT0003row("TANKNO")
                        SQLcmd.ExecuteNonQuery()
                    Next
                End If
            Else
                '指定されたタンク車№をKEYに更新
                PARA01.Value = I_TANKNO
                SQLcmd.ExecuteNonQuery()
            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_TANKSHOZAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_TANKSHOZAI UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        'If WW_ERRCODE = C_MESSAGE_NO.NORMAL Then
        '    '○メッセージ表示
        '    Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        'End If

    End Sub

    ''' <summary>
    ''' (受注TBL)フラグ関連更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateRelatedFlg(ByVal I_Value As String, Optional ByVal I_PARA01 As String = Nothing)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLの各フラグを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0002_ORDER " _
                    & "    SET UPDYMD      = @P11, " _
                    & "        UPDUSER     = @P12, " _
                    & "        UPDTERMID   = @P13, " _
                    & "        RECEIVEYMD  = @P14, "

            SQLStr &= String.Format("        {0}   = @P03 ", I_PARA01)

            SQLStr &=
                    "  WHERE ORDERNO     = @P01  " _
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_" + I_PARA01 + "UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_" + I_PARA01 + "UPDATE"
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
    ''' (受注TBL)タンク車数更新(空回日報経由ではない場合)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateNotETDViaOrderTankCnt(ByVal SQLcon As SqlConnection, ByVal I_KEY As String)

        Try
            'DataBase接続文字
            'Dim SQLcon = CS0050SESSION.getConnection
            'SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr As String = ""
            '更新SQL文･･･受注TBLの受注情報を更新
            SQLStr =
                " UPDATE OIL.OIT0002_ORDER " _
                & "    SET HTANK        = @P11, " _
                & "        RTANK        = @P12, " _
                & "        TTANK        = @P13, " _
                & "        MTTANK       = @P14, " _
                & "        KTANK        = @P15, " _
                & "        K3TANK       = @P16, " _
                & "        K5TANK       = @P17, " _
                & "        K10TANK      = @P18, " _
                & "        LTANK        = @P19, " _
                & "        ATANK        = @P20, " _
                & "        OTHER1OTANK  = @P21, " _
                & "        OTHER2OTANK  = @P22, " _
                & "        OTHER3OTANK  = @P23, " _
                & "        OTHER4OTANK  = @P24, " _
                & "        OTHER5OTANK  = @P25, " _
                & "        OTHER6OTANK  = @P26, " _
                & "        OTHER7OTANK  = @P27, " _
                & "        OTHER8OTANK  = @P28, " _
                & "        OTHER9OTANK  = @P29, " _
                & "        OTHER10OTANK = @P30, " _
                & "        TOTALTANK    = @P31, " _
                & "        UPDYMD       = @P32, " _
                & "        UPDUSER      = @P33, " _
                & "        UPDTERMID    = @P34, " _
                & "        RECEIVEYMD   = @P35  " _
                & "  WHERE ORDERNO      = @P01 " _
                & "    AND DELFLG      <> @P02;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.Int)          '車数（ハイオク）
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Int)          '車数（レギュラー）
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Int)          '車数（灯油）
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.Int)          '車数（未添加灯油）
            Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.Int)          '車数（軽油）
            Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.Int)          '車数（３号軽油）
            Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.Int)          '車数（５号軽油）
            Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.Int)          '車数（１０号軽油）
            Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Int)          '車数（LSA）
            Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.Int)          '車数（A重油）
            Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Int)          '車数（その他１）
            Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Int)          '車数（その他２）
            Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.Int)          '車数（その他３）
            Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.Int)          '車数（その他４）
            Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Int)          '車数（その他５）
            Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Int)          '車数（その他６）
            Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.Int)          '車数（その他７）
            Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Int)          '車数（その他８）
            Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.Int)          '車数（その他９）
            Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.Int)          '車数（その他１０）
            Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.Int)          '合計車数
            Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.DateTime)
            Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.NVarChar)
            Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.NVarChar)
            Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.DateTime)

            PARA01.Value = I_KEY
            PARA02.Value = C_DELETE_FLG.DELETE
            'TBL更新
            PARA11.Value = Me.TxtHTank_c.Text
            PARA12.Value = Me.TxtRTank_c.Text
            PARA13.Value = Me.TxtTTank_c.Text
            PARA14.Value = Me.TxtMTTank_c.Text
            PARA15.Value = Me.TxtKTank_c.Text
            PARA16.Value = Me.TxtK3Tank_c.Text
            PARA17.Value = Me.TxtK5Tank_c.Text
            PARA18.Value = Me.TxtK10Tank_c.Text
            PARA19.Value = Me.TxtLTank_c.Text
            PARA20.Value = Me.TxtATank_c.Text
            PARA21.Value = "0"
            PARA22.Value = "0"
            PARA23.Value = "0"
            PARA24.Value = "0"
            PARA25.Value = "0"
            PARA26.Value = "0"
            PARA27.Value = "0"
            PARA28.Value = "0"
            PARA29.Value = "0"
            PARA30.Value = "0"
            PARA31.Value = Me.TxtTotal_c.Text
            PARA32.Value = Date.Now
            PARA33.Value = Master.USERID
            PARA34.Value = Master.USERTERMID
            PARA35.Value = C_DEFAULT_YMD

            '画面更新
            Me.TxtHTank.Text = Me.TxtHTank_c.Text
            Me.TxtRTank.Text = Me.TxtRTank_c.Text
            Me.TxtTTank.Text = Me.TxtTTank_c.Text
            Me.TxtMTTank.Text = Me.TxtMTTank_c.Text
            Me.TxtKTank.Text = Me.TxtKTank_c.Text
            Me.TxtK3Tank.Text = Me.TxtK3Tank_c.Text
            Me.TxtK5Tank.Text = Me.TxtK5Tank_c.Text
            Me.TxtK10Tank.Text = Me.TxtK10Tank_c.Text
            Me.TxtLTank.Text = Me.TxtLTank_c.Text
            Me.TxtATank.Text = Me.TxtATank_c.Text
            Me.TxtTotalCnt.Text = Me.TxtTotal_c.Text

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_NotETDViaORDERTANKCNT UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_NotETDViaORDERTANKCNT UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (受注明細TBL)他の受注で同日の積込日を設定しているデータ取得・更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateSameStackingOtherOrder(ByVal SQLcon As SqlConnection)

        '他受注オーダーで積込日が同日チェック用
        If IsNothing(OIT0003WK9tbl) Then
            OIT0003WK9tbl = New DataTable
        End If

        If OIT0003WK9tbl.Columns.Count <> 0 Then
            OIT0003WK9tbl.Columns.Clear()
        End If

        OIT0003WK9tbl.Clear()

        Try
            '○ チェックSQL
            '　説明
            '     登録された内容が受注TBLにすでに登録済みかチェックする
            Dim SQLStr As String =
                  " SELECT " _
                & "   ISNULL(RTRIM(OIT0002.ORDERNO), '')         AS ORDERNO" _
                & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')        AS DETAILNO" _
                & " , ISNULL(RTRIM(OIT0003.SHIPORDER), '')       AS SHIPORDER" _
                & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')         AS TRAINNO" _
                & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')       AS TRAINNAME" _
                & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')       AS LINEORDER" _
                & " , ISNULL(RTRIM(OIT0003.TANKNO), '')          AS TANKNO" _
                & " , ISNULL(RTRIM(OIT0003.OILCODE), '')         AS OILCODE" _
                & " , ISNULL(RTRIM(OIT0003.OILNAME), '')         AS OILNAME" _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')    AS ORDERINGTYPE" _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '') AS ORDERINGOILNAME" _
                & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')      AS OFFICECODE" _
                & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')      AS OFFICENAME" _
                & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')    AS SHIPPERSCODE" _
                & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')    AS SHIPPERSNAME" _
                & " , ISNULL(RTRIM(OIT0002.BASECODE), '')        AS BASECODE" _
                & " , ISNULL(RTRIM(OIT0002.BASENAME), '')        AS BASENAME" _
                & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')   AS CONSIGNEECODE" _
                & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')   AS CONSIGNEENAME" _
                & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')      AS DEPSTATION" _
                & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')  AS DEPSTATIONNAME" _
                & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')      AS ARRSTATION" _
                & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')  AS ARRSTATIONNAME" _
                & " , ISNULL(RTRIM(OIT0002.LODDATE), '')         AS LODDATE" _
                & " , ISNULL(RTRIM(OIT0002.DEPDATE), '')         AS DEPDATE" _
                & " , ISNULL(RTRIM(OIT0002.ARRDATE), '')         AS ARRDATE" _
                & " , ISNULL(RTRIM(OIT0002.ACCDATE), '')         AS ACCDATE" _
                & " , ISNULL(RTRIM(OIT0002.EMPARRDATE), '')      AS EMPARRDATE" _
                & " FROM oil.OIT0002_ORDER OIT0002 " _
                & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
                & "       OIT0003.ORDERNO = OIT0002.ORDERNO " _
                & "   AND OIT0003.STACKINGFLG = '1' " _
                & "   AND OIT0003.ACTUALLODDATE = @P03 " _
                & "   AND ISNULL(OIT0003.STACKINGORDERNO,'') = '' " _
                & "   AND OIT0003.DELFLG <> @P05" _
                & " WHERE OIT0002.ORDERNO <> @P01 " _
                & " AND OIT0002.OFFICECODE = @P02 " _
                & " AND (OIT0002.ORDERSTATUS <> @P04 AND OIT0002.ORDERSTATUS <= @P06)" _
                & " AND OIT0002.DELFLG <> @P05 "

            '○ チェックSQL
            '　説明
            '     登録された内容が受注TBLにすでに登録済みかチェックする
            Dim SQLUPStr As String =
                  " UPDATE oil.OIT0003_DETAIL " _
                & "  SET STACKINGORDERNO = @P01, " _
                & "      UPDYMD          = @P13, " _
                & "      UPDUSER         = @P14, " _
                & "      UPDTERMID       = @P15, " _
                & "      RECEIVEYMD      = @P16 " _
                & "  WHERE ORDERNO       = @P11 " _
                & "    AND DETAILNO      = @P12 " _
                & "    AND DELFLG       <> '1' ;"

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLUPcmd As New SqlCommand(SQLUPStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                'Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20) '本線列車名
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '(実績)積込日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス(900:受注キャンセル)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 3)  '受注進行ステータス(310:受注確定)
                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                'PARA2.Value = Me.TxtTrainName.Text
                PARA2.Value = Me.TxtOrderOfficeCode.Text
                PARA3.Value = Me.TxtActualLoadingDate.Text
                PARA4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA5.Value = C_DELETE_FLG.DELETE
                PARA6.Value = BaseDllConst.CONST_ORDERSTATUS_310

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WK9tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WK9tbl.Load(SQLdr)
                End Using

                '★更新用条件設定
                Dim PARAUP01 As SqlParameter = SQLUPcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№(更新元)
                Dim PARAUP11 As SqlParameter = SQLUPcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 11) '受注№(更新先)
                Dim PARAUP12 As SqlParameter = SQLUPcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 3)  '受注明細№(更新先)
                Dim PARAUP13 As SqlParameter = SQLUPcmd.Parameters.Add("@P13", System.Data.SqlDbType.DateTime)
                Dim PARAUP14 As SqlParameter = SQLUPcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar)
                Dim PARAUP15 As SqlParameter = SQLUPcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar)
                Dim PARAUP16 As SqlParameter = SQLUPcmd.Parameters.Add("@P16", System.Data.SqlDbType.DateTime)
                PARAUP01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARAUP13.Value = Date.Now
                PARAUP14.Value = Master.USERID
                PARAUP15.Value = Master.USERTERMID
                PARAUP16.Value = C_DEFAULT_YMD

                For Each OIT0003UPDrow In OIT0003WK9tbl.Rows
                    PARAUP11.Value = OIT0003UPDrow("ORDERNO")
                    PARAUP12.Value = OIT0003UPDrow("DETAILNO")

                    SQLUPcmd.ExecuteNonQuery()
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D_SameStackingOtherOrder UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D_SameStackingOtherOrder UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' (受注TBL/受注明細TBL)受注データ削除
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_DeleteOrder(ByVal SQLcon As SqlConnection, ByVal I_ORDERNO As String)

        '削除SQL文･･･受注TBL、及び受注明細TBLにおいて指定された受注Noを削除
        Dim SQLStr As String =
            " DELETE FROM OIL.OIT0002_ORDER WHERE ORDERNO = @P01; " _
            & " DELETE FROM OIL.OIT0003_DETAIL WHERE ORDERNO = @P01; "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11)  '受注№
                PARA01.Value = I_ORDERNO

                SQLcmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D DELETEORDER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D DELETEORDER"
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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

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
                Me.TxtShippersCode.Text = WW_SelectValue
                Me.LblShippersName.Text = WW_SelectText
                work.WF_SEL_SHIPPERSCODE.Text = WW_SelectValue
                work.WF_SEL_SHIPPERSNAME.Text = WW_SelectText
                Me.TxtShippersCode.Focus()

            '荷受人
            Case "TxtConsigneeCode"
                Me.TxtConsigneeCode.Text = WW_SelectValue
                Me.LblConsigneeName.Text = WW_SelectText
                work.WF_SEL_CONSIGNEECODE.Text = WW_SelectValue
                work.WF_SEL_CONSIGNEENAME.Text = WW_SelectText
                Me.TxtConsigneeCode.Focus()

            '受注営業所
            Case "TxtOrderOffice"
                '別の受注営業所が設定された場合
                If Me.TxtOrderOffice.Text <> WW_SelectText Then
                    Me.TxtOrderOffice.Text = WW_SelectText
                    Me.TxtOrderOfficeCode.Text = WW_SelectValue

                    'work.WF_SEL_SALESOFFICECODE.Text = WW_SelectValue
                    'work.WF_SEL_SALESOFFICE.Text = WW_SelectText
                    work.WF_SEL_ORDERSALESOFFICECODE.Text = WW_SelectValue
                    work.WF_SEL_ORDERSALESOFFICE.Text = WW_SelectText

                    '○ テキストボックスを初期化
                    WW_HedarItemsInitialize()

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
                Me.TxtTrainNo.Focus()

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

                '★再設定した列車が前回と違う場合
                If Me.TxtTrainName.Text <> WW_SelectText Then
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

                    '○ テキストボックスを初期化
                    WW_HedarItemsInitialize()

                    Me.TxtTrainNo.Text = WW_SelectValue
                    Me.TxtTrainName.Text = WW_SelectText
                    'WW_FixvalueMasterSearch("", "TRAINNUMBER", WW_SelectValue, WW_GetValue)

                    '★再設定した列車が""(空)の場合
                    If Me.TxtTrainName.Text = "" Then Exit Select

                    '〇 取得した列車名から各値を取得し設定する。
                    WW_TRAINNUMBER_FIND(WW_SelectText)
                End If

            '発駅
            Case "TxtDepstationCode"
                Me.TxtDepstationCode.Text = WW_SelectValue
                Me.LblDepstationName.Text = WW_SelectText
                Me.TxtDepstationCode.Focus()

            '着駅
            Case "TxtArrstationCode"
                Me.TxtArrstationCode.Text = WW_SelectValue
                Me.LblArrstationName.Text = WW_SelectText
                Me.TxtArrstationCode.Focus()

                '〇営業所配下情報を取得・設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    '〇 画面(受注営業所).テキストボックスが未設定
                    If Me.TxtOrderOffice.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
                    End If
                Else
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
                End If
                Me.TxtShippersCode.Text = WW_GetValue(0)
                Me.LblShippersName.Text = WW_GetValue(1)
                Me.TxtConsigneeCode.Text = WW_GetValue(4)
                Me.LblConsigneeName.Text = WW_GetValue(5)
                Me.TxtOrderType.Text = WW_GetValue(7)

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
                        Me.TxtLoadingDate.Text = ""
                    Else
                        Me.TxtLoadingDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtLoadingDate.Focus()

                '### 2020608 START ########################################################################################
                '◯ 列車(名称)から日数を取得
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "TRAINNUMBER_FIND", Me.TxtTrainName.Text, WW_GetValue)

                '〇 (予定)の日付を設定
                Me.TxtDepDate.Text = Date.Parse(Me.TxtLoadingDate.Text).AddDays(Integer.Parse(WW_GetValue(6))).ToString("yyyy/MM/dd")
                Me.TxtArrDate.Text = Date.Parse(Me.TxtLoadingDate.Text).AddDays(Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                Me.TxtAccDate.Text = Date.Parse(Me.TxtLoadingDate.Text).AddDays(Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                Me.TxtEmparrDate.Text = Date.Parse(Me.TxtLoadingDate.Text).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                '### 2020608 END   ########################################################################################

            '(予定)発日
            Case "TxtDepDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtDepDate.Text = ""
                    Else
                        Me.TxtDepDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtDepDate.Focus()

                '### 2020608 START ########################################################################################
                '◯ 列車(名称)から日数を取得
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "TRAINNUMBER_FIND", Me.TxtTrainName.Text, WW_GetValue)

                '〇 (予定)の日付を設定
                If Integer.Parse(WW_GetValue(6)) = 0 Then
                    Me.TxtArrDate.Text = Date.Parse(Me.TxtDepDate.Text).AddDays(Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                    Me.TxtAccDate.Text = Date.Parse(Me.TxtDepDate.Text).AddDays(Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtDepDate.Text).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                ElseIf Integer.Parse(WW_GetValue(6)) > 0 Then
                    Me.TxtArrDate.Text = Date.Parse(Me.TxtDepDate.Text).AddDays((-1 * Integer.Parse(WW_GetValue(6))) + Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                    Me.TxtAccDate.Text = Date.Parse(Me.TxtDepDate.Text).AddDays((-1 * Integer.Parse(WW_GetValue(6))) + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtDepDate.Text).AddDays((-1 * Integer.Parse(WW_GetValue(6))) + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                End If
                '### 2020608 END   ########################################################################################

            '(予定)積車着日
            Case "TxtArrDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtArrDate.Text = ""
                    Else
                        Me.TxtArrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtArrDate.Focus()

                '### 2020608 START ########################################################################################
                '◯ 列車(名称)から日数を取得
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "TRAINNUMBER_FIND", Me.TxtTrainName.Text, WW_GetValue)

                '〇 (予定)の日付を設定
                If Integer.Parse(WW_GetValue(8)) = 0 Then
                    Me.TxtAccDate.Text = Date.Parse(Me.TxtArrDate.Text).AddDays(Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtArrDate.Text).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                ElseIf Integer.Parse(WW_GetValue(8)) > 0 Then
                    Me.TxtAccDate.Text = Date.Parse(Me.TxtArrDate.Text).AddDays((-1 * Integer.Parse(WW_GetValue(8))) + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtArrDate.Text).AddDays((-1 * Integer.Parse(WW_GetValue(8))) + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                End If
                '### 2020608 END   ########################################################################################

            '(予定)受入日
            Case "TxtAccDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtAccDate.Text = ""
                    Else
                        Me.TxtAccDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtAccDate.Focus()

                '### 2020608 START ########################################################################################
                '◯ 列車(名称)から日数を取得
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "TRAINNUMBER_FIND", Me.TxtTrainName.Text, WW_GetValue)

                '〇 (予定)の日付を設定
                If Integer.Parse(WW_GetValue(9)) = 0 Then
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtAccDate.Text).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                ElseIf Integer.Parse(WW_GetValue(9)) > 0 Then
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtAccDate.Text).AddDays((-1 * Integer.Parse(WW_GetValue(9))) + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                End If
                '### 2020608 END   ########################################################################################

            '(予定)空車着日
            Case "TxtEmparrDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtEmparrDate.Text = ""
                    Else
                        Me.TxtEmparrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtEmparrDate.Focus()

            '(実績)積込日
            Case "TxtActualLoadingDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtActualLoadingDate.Text = ""
                    Else
                        Me.TxtActualLoadingDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtActualLoadingDate.Focus()

                '(実績)積込日に入力された日付を、(一覧)積込日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    If OIT0003tab3row("ACTUALLODDATE") <> "" Then Continue For
                    OIT0003tab3row("ACTUALLODDATE") = Me.TxtActualLoadingDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            '(実績)発日
            Case "TxtActualDepDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtActualDepDate.Text = ""
                    Else
                        Me.TxtActualDepDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtActualDepDate.Focus()

                '(実績)発日に入力された日付を、(一覧)発日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    OIT0003tab3row("ACTUALDEPDATE") = Me.TxtActualDepDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            '(実績)積車着日
            Case "TxtActualArrDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtActualArrDate.Text = ""
                    Else
                        Me.TxtActualArrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtActualArrDate.Focus()

                '(実績)積込着日に入力された日付を、(一覧)積込着日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    OIT0003tab3row("ACTUALARRDATE") = Me.TxtActualArrDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            '(実績)受入日
            Case "TxtActualAccDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtActualAccDate.Text = ""
                    Else
                        Me.TxtActualAccDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtActualAccDate.Focus()

                '(実績)受入日に入力された日付を、(一覧)受入日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    OIT0003tab3row("ACTUALACCDATE") = Me.TxtActualAccDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            '(実績)空車着日
            Case "TxtActualEmparrDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtActualEmparrDate.Text = ""
                    Else
                        Me.TxtActualEmparrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtActualEmparrDate.Focus()

                '(実績)空車着日に入力された日付を、(一覧)空車着日に反映させる。
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    OIT0003tab3row("ACTUALEMPARRDATE") = Me.TxtActualEmparrDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text) Then Exit Sub

            'タブ「タンク車割当」　　⇒　(一覧)荷主, (一覧)油種, (一覧)タンク車№, (一覧)交検日, 
            '                            (一覧)(実績)積込日, (一覧)ジョイント先
            'タブ「入換・積込指示」　⇒　(一覧)積込入線列車番号, (一覧)積込出線列車番号, (一覧)回線, (一覧)充填ポイント
            'タブ「タンク車明細」　　⇒　(一覧)(実績)積込日, (一覧)(実績)発日, (一覧)(実績)積車着日, (一覧)(実績)受入日, (一覧)(実績)空車着日,
            '                            (一覧)ジョイント先, (一覧)第2着駅, (一覧)第2荷受人
            'タブ「費用入力」　　　　⇒　(一覧)計上月, (一覧)科目コード, (一覧)請求先コード, (一覧)支払先コード
            Case "SHIPPERSNAME", "OILNAME", "ORDERINGOILNAME", "TANKNO", "JRINSPECTIONDATE",
                 "LOADINGIRILINETRAINNO", "LOADINGOUTLETTRAINNO", "LINE", "FILLINGPOINT",
                 "ACTUALLODDATE", "ACTUALDEPDATE", "ACTUALARRDATE", "ACTUALACCDATE", "ACTUALEMPARRDATE",
                 "JOINT", "SECONDARRSTATIONNAME", "SECONDCONSIGNEENAME",
                 "KEIJYOYM", "ACCSEGCODE", "INVOICECODE", "PAYEECODE"
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
                        '(一覧)荷主名
                        If WF_FIELD.Value = "SHIPPERSNAME" Then
                            updHeader.Item("SHIPPERSCODE") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '(一覧)油種名
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

                            '(一覧)油種名(受発注用)
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
                                    If Me.TxtOrderOffice.Text = "" Then
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

                            '(一覧)タンク車№
                        ElseIf WF_FIELD.Value = "TANKNO" Then

                            '設定されたタンク車Noを設定
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE

                            '受注情報を初期化
                            updHeader.Item("ORDERINFO") = ""
                            updHeader.Item("ORDERINFONAME") = ""

                            'タンク車№に紐づく情報を取得・設定
                            WW_TANKNUMBER_FIND(updHeader)

                            '(一覧)交検日
                        ElseIf WF_FIELD.Value = "JRINSPECTIONDATE" Then

                            Dim WW_DATE As Date
                            Try
                                Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                                If WW_DATE < C_DEFAULT_YMD Then
                                    updHeader.Item(WF_FIELD.Value) = ""
                                Else

                                    '■ 選択した日付が未設定,
                                    '   選択した日付が現状の交検日より過去の場合
                                    If leftview.WF_Calendar.Text = "" _
                                        OrElse updHeader.Item(WF_FIELD.Value) = "" _
                                        OrElse Date.Compare(Date.Parse(leftview.WF_Calendar.Text), Date.Parse(updHeader.Item(WF_FIELD.Value))) = -1 Then
                                        Master.Output(C_MESSAGE_NO.OIL_TANKNO_KOUKENBI_PAST_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                                        '■ 選択した日付が現状の交検日と同日の場合
                                    ElseIf Date.Compare(Date.Parse(leftview.WF_Calendar.Text), Date.Parse(updHeader.Item(WF_FIELD.Value))) = 0 Then
                                        updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text

                                    Else
                                        '(一覧)交検日に指定した日付を設定
                                        updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text
                                        Master.SaveTable(OIT0003tbl)
                                        'タンク車マスタの交検日を更新
                                        WW_UpdateTankMaster(updHeader.Item("TANKNO"), I_JRINSPECTIONDATE:=updHeader.Item(WF_FIELD.Value))
                                        'タンク車№に紐づく情報を取得・設定
                                        WW_TANKNUMBER_FIND(updHeader, I_CMPCD:=work.WF_SEL_CAMPCODE.Text)

                                    End If

                                End If
                            Catch ex As Exception
                            End Try

                            '(一覧)(実績)積込日を一覧に設定
                        ElseIf WF_FIELD.Value = "ACTUALLODDATE" Then
                            Dim WW_DATE As Date
                            Try
                                Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                                If WW_DATE < C_DEFAULT_YMD Then
                                    updHeader.Item(WF_FIELD.Value) = ""
                                Else
                                    updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text
                                End If

                                '(一覧)(実績)積込日の場合
                                If WF_FIELD.Value = "ACTUALLODDATE" Then
                                    '○ 過去日付チェック
                                    '例) iresult = dt1.Date.CompareTo(dt2.Date)
                                    '    iresultの意味
                                    '     0 : dt1とdt2は同じ日
                                    '    -1 : dt1はdt2より前の日
                                    '     1 : dt1はdt2より後の日
                                    '(予定)積込日 と　現在日付を比較
                                    Dim iresult As Integer = Date.Parse(leftview.WF_Calendar.Text).CompareTo(Date.Parse(Me.TxtDepDate.Text))
                                    '◯ (一覧)積込日＜(予定)発日
                                    If iresult = -1 Then
                                        '★積置(チェックボックスON)
                                        updHeader.Item("STACKINGFLG") = "on"
                                    Else
                                        '★積置(チェックボックスOFF)
                                        updHeader.Item("STACKINGFLG") = ""
                                    End If
                                End If

                            Catch ex As Exception
                            End Try

                            '(一覧)ジョイントを一覧に設定
                        ElseIf WF_FIELD.Value = "JOINT" Then
                            updHeader.Item("JOINTCODE") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '(一覧)第2着駅
                        ElseIf WF_FIELD.Value = "SECONDARRSTATIONNAME" Then
                            updHeader.Item("SECONDARRSTATION") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                            '(一覧)第2荷受人
                        ElseIf WF_FIELD.Value = "SECONDCONSIGNEENAME" Then
                            updHeader.Item("SECONDCONSIGNEECODE") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

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
                                If Me.TxtOrderOffice.Text = "" Then
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

                            '★表の1行目を入力した場合、2行目以降の値も同様に設定する。
                            If WW_LINECNT = 1 Then
                                For Each OIT0003row As DataRow In OIT0003tbl_tab2.Rows
                                    OIT0003row("LOADINGIRILINETRAINNO") = WW_SelectValue
                                    OIT0003row("LOADINGIRILINETRAINNAME") = WW_SelectText
                                    OIT0003row("LINE") = WW_GetValue(5)
                                    OIT0003row("LOADINGOUTLETTRAINNO") = WW_GetValue(6)
                                    OIT0003row("LOADINGOUTLETTRAINNAME") = WW_GetValue(7)
                                Next
                            End If

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
                                If Me.TxtOrderOffice.Text = "" Then
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
                                If Me.TxtOrderOffice.Text = "" Then
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

                            '充填ポイントを一覧に設定
                        ElseIf WF_FIELD.Value = "FILLINGPOINT" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE

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

                                '(一覧)(実績)積込日の場合
                                If WF_FIELD.Value = "ACTUALLODDATE" Then
                                    '○ 過去日付チェック
                                    '例) iresult = dt1.Date.CompareTo(dt2.Date)
                                    '    iresultの意味
                                    '     0 : dt1とdt2は同じ日
                                    '    -1 : dt1はdt2より前の日
                                    '     1 : dt1はdt2より後の日
                                    '(予定)積込日 と　現在日付を比較
                                    Dim iresult As Integer = Date.Parse(leftview.WF_Calendar.Text).CompareTo(Date.Parse(Me.TxtDepDate.Text))
                                    '◯ (一覧)積込日＜(予定)発日
                                    If iresult = -1 Then
                                        '★積置(チェックボックスON)
                                        updHeader.Item("STACKINGFLG") = "on"
                                    Else
                                        '★積置(チェックボックスOFF)
                                        updHeader.Item("STACKINGFLG") = ""
                                    End If
                                End If

                            Catch ex As Exception
                            End Try

                            '(一覧)ジョイントを一覧に設定
                        ElseIf WF_FIELD.Value = "JOINT" Then
                            updHeader.Item("JOINTCODE") = WW_SETVALUE
                            updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

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

                        '〇 一覧項目へ設定
                        '(一覧)計上月
                        If WF_FIELD.Value = "KEIJYOYM" Then
                            Dim WW_DATE As Date
                            Try
                                Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                                If WW_DATE < C_DEFAULT_YMD Then
                                    updHeader.Item(WF_FIELD.Value) = ""
                                    updHeader.Item("KEIJYOYMD") = ""
                                Else
                                    updHeader.Item(WF_FIELD.Value) = WW_DATE.ToString("yyyy/MM")
                                    updHeader.Item("KEIJYOYMD") = leftview.WF_Calendar.Text
                                End If
                            Catch ex As Exception
                            End Try

                            '(一覧)科目コード
                        ElseIf WF_FIELD.Value = "ACCSEGCODE" Then

                            '科目コード+セグメント+セグメント枝番号をKEYにして、科目コード等を取得
                            WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "ACCOUNTPATTERN", WW_SETVALUE, WW_GetValue)

                            updHeader.Item(WF_FIELD.Value) = WW_GetValue(1) + "　" + WW_GetValue(3)
                            updHeader.Item("ACCSEGNAME") = WW_GetValue(2) + "　" + WW_GetValue(4)
                            updHeader.Item("ACCOUNTCODE") = WW_GetValue(1)
                            updHeader.Item("ACCOUNTNAME") = WW_GetValue(2)
                            updHeader.Item("SEGMENTCODE") = WW_GetValue(3)
                            updHeader.Item("SEGMENTNAME") = WW_GetValue(4)
                            updHeader.Item("BREAKDOWNCODE") = WW_GetValue(5)
                            updHeader.Item("BREAKDOWN") = WW_GetValue(6)

                            '(一覧)請求先コード
                        ElseIf WF_FIELD.Value = "INVOICECODE" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                            updHeader.Item("INVOICENAME") = WW_SETTEXT

                            '(一覧)支払先コード
                        ElseIf WF_FIELD.Value = "PAYEECODE" Then
                            updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                            updHeader.Item("PAYEENAME") = WW_SETTEXT

                        End If
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
    ''' ヘッダー項目を初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_HedarItemsInitialize()

        '本線列車
        Me.TxtTrainNo.Text = ""
        Me.TxtTrainName.Text = ""
        'OT本線列車
        Me.TxtOTTrainNo.Text = ""
        Me.TxtOTTrainName.Text = ""
        '積置フラグ("2"(積置なし)を設定)
        work.WF_SEL_STACKINGFLG.Text = "2"
        chkOrderInfo.Checked = False
        '荷主
        Me.TxtShippersCode.Text = ""
        Me.LblShippersName.Text = ""
        '荷受人
        Me.TxtConsigneeCode.Text = ""
        Me.LblConsigneeName.Text = ""
        '発駅
        Me.TxtDepstationCode.Text = ""
        Me.LblDepstationName.Text = ""
        '着駅
        Me.TxtArrstationCode.Text = ""
        Me.LblArrstationName.Text = ""
        '受注パターン
        Me.TxtOrderType.Text = ""
        '(予定)日付
        Me.TxtLoadingDate.Text = ""
        Me.TxtDepDate.Text = ""
        Me.TxtArrDate.Text = ""
        Me.TxtAccDate.Text = ""
        Me.TxtEmparrDate.Text = ""
        '(割当後)タンク車割当
        Me.TxtHTank_w.Text = "0"
        Me.TxtRTank_w.Text = "0"
        Me.TxtTTank_w.Text = "0"
        Me.TxtMTTank_w.Text = "0"
        Me.TxtKTank_w.Text = "0"
        Me.TxtK3Tank_w.Text = "0"
        Me.TxtK5Tank_w.Text = "0"
        Me.TxtK10Tank_w.Text = "0"
        Me.TxtLTank_w.Text = "0"
        Me.TxtATank_w.Text = "0"
        Me.TxtTotalCnt_w.Text = "0"

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()
        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"               '会社コード
                Me.WF_CAMPCODE.Focus()
            Case "WF_UORG"                   '運用部署
                Me.WF_UORG.Focus()
            Case "TxtShippersCode"           '荷主
                Me.TxtShippersCode.Focus()
            Case "TxtConsigneeCode"          '荷受人
                Me.TxtConsigneeCode.Focus()
            Case "TxtTrainNo"                '本線列車
                Me.TxtTrainNo.Focus()
            Case "TxtDepstationCode"         '発駅
                Me.TxtDepstationCode.Focus()
            Case "TxtArrstationCode"         '着駅
                Me.TxtArrstationCode.Focus()
            Case "TxtLoadingDate"            '(予定)積込日
                Me.TxtLoadingDate.Focus()
            Case "TxtDepDate"                '(予定)発日
                Me.TxtDepDate.Focus()
            Case "TxtArrDate"                '(予定)積車着日
                Me.TxtArrDate.Focus()
            Case "TxtAccDate"                '(予定)受入日
                Me.TxtAccDate.Focus()
            Case "TxtEmparrDate"             '(予定)空車着日
                Me.TxtEmparrDate.Focus()
            Case "TxtActualLoadingDate"      '(実績)積込日
                Me.TxtActualLoadingDate.Focus()
            Case "TxtActualDepDate"          '(実績)発日
                Me.TxtActualDepDate.Focus()
            Case "TxtActualArrDate"          '(実績)積車着日
                Me.TxtActualArrDate.Focus()
            Case "TxtActualAccDate"          '(実績)受入日
                Me.TxtActualAccDate.Focus()
            Case "TxtActualEmparrDate"       '(実績)空車着日
                Me.TxtActualEmparrDate.Focus()
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
                & "   ISNULL(RTRIM(VIW0001.CAMPCODE), '')    AS CAMPCODE" _
                & " , ISNULL(RTRIM(VIW0001.CLASS), '')       AS CLASS" _
                & " , ISNULL(RTRIM(VIW0001.KEYCODE), '')     AS KEYCODE" _
                & " , ISNULL(RTRIM(VIW0001.STYMD), '')       AS STYMD" _
                & " , ISNULL(RTRIM(VIW0001.ENDYMD), '')      AS ENDYMD" _
                & " , ISNULL(RTRIM(VIW0001.VALUE1), '')      AS VALUE1" _
                & " , ISNULL(RTRIM(VIW0001.VALUE2), '')      AS VALUE2" _
                & " , ISNULL(RTRIM(VIW0001.VALUE3), '')      AS VALUE3" _
                & " , ISNULL(RTRIM(VIW0001.VALUE4), '')      AS VALUE4" _
                & " , ISNULL(RTRIM(VIW0001.VALUE5), '')      AS VALUE5" _
                & " , ISNULL(RTRIM(VIW0001.VALUE6), '')      AS VALUE6" _
                & " , ISNULL(RTRIM(VIW0001.VALUE7), '')      AS VALUE7" _
                & " , ISNULL(RTRIM(VIW0001.VALUE8), '')      AS VALUE8" _
                & " , ISNULL(RTRIM(VIW0001.VALUE9), '')      AS VALUE9" _
                & " , ISNULL(RTRIM(VIW0001.VALUE10), '')     AS VALUE10" _
                & " , ISNULL(RTRIM(VIW0001.VALUE11), '')     AS VALUE11" _
                & " , ISNULL(RTRIM(VIW0001.VALUE12), '')     AS VALUE12" _
                & " , ISNULL(RTRIM(VIW0001.VALUE13), '')     AS VALUE13" _
                & " , ISNULL(RTRIM(VIW0001.VALUE14), '')     AS VALUE14" _
                & " , ISNULL(RTRIM(VIW0001.VALUE15), '')     AS VALUE15" _
                & " , ISNULL(RTRIM(VIW0001.VALUE16), '')     AS VALUE16" _
                & " , ISNULL(RTRIM(VIW0001.VALUE17), '')     AS VALUE17" _
                & " , ISNULL(RTRIM(VIW0001.VALUE18), '')     AS VALUE18" _
                & " , ISNULL(RTRIM(VIW0001.VALUE19), '')     AS VALUE19" _
                & " , ISNULL(RTRIM(VIW0001.VALUE20), '')     AS VALUE20" _
                & " , ISNULL(RTRIM(VIW0001.SYSTEMKEYFLG), '')   AS SYSTEMKEYFLG" _
                & " , ISNULL(RTRIM(VIW0001.DELFLG), '')      AS DELFLG" _
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

                    If IsNothing(I_PARA01) Then
                        'Dim i As Integer = 0 '2020/3/23 三宅 Delete
                        For Each OIT0003WKrow As DataRow In OIT0003Fixvaltbl.Rows '(全抽出結果回るので要検討
                            'O_VALUE(i) = OIT0003WKrow("KEYCODE") 2020/3/23 三宅 全部KEYCODE(列車NO)が格納されてしまうので修正しました（問題なければこのコメント消してください)
                            For i = 1 To O_VALUE.Length
                                O_VALUE(i - 1) = OIT0003WKrow("VALUE" & i.ToString())
                            Next
                            'i += 1 '2020/3/23 三宅 Delete
                        Next

                    ElseIf I_PARA01 = "1" Then    '### 油種登録用の油種コードを取得 ###
                        Dim i As Integer = 0
                        For Each OIT0003WKrow As DataRow In OIT0003Fixvaltbl.Rows
                            O_VALUE(i) = Convert.ToString(OIT0003WKrow("KEYCODE"))
                            i += 1
                        Next
                    End If

                Else
                    For Each OIT0003WKrow As DataRow In OIT0003Fixvaltbl.Rows

                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0003WKrow("VALUE" & i.ToString())
                        Next
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
    Protected Sub WW_ScreenOrderStatusSet()
        '〇 受注ステータスが"受注手配"の場合
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then
            '〇(受注TBL)受注進行ステータス更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                '### 20200812 START(指摘票(全体)No121) #########################################
                'WW_UpdateOrderStatus(BaseDllConst.CONST_ORDERSTATUS_200)
                WW_UpdateOrderStatus(BaseDllConst.CONST_ORDERSTATUS_200, ReuseFlg:=True)
                '### 20200812 END  (指摘票(全体)No121) #########################################
                CODENAME_get("ORDERSTATUS", BaseDllConst.CONST_ORDERSTATUS_200, Me.TxtOrderStatus.Text, WW_DUMMY)
                work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200
                work.WF_SEL_ORDERSTATUSNM.Text = Me.TxtOrderStatus.Text

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
                Me.WW_UPBUTTONFLG = "1"

                '〇タンク車所在の更新
                WW_TankShozaiSet()

                '### 臨海鉄道対応 ####################################################################################
                '五井営業所、甲子営業所、袖ヶ浦営業所の場合
                '積込列車番号の入力を可能とする。
                If work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011201 _
                    OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011202 _
                    OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then

                    '臨海鉄道対象のため有効にする。
                    WW_RINKAIFLG = True

                End If

                '$$$ 20200710 START((全体)No101対応) $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                '$$$ 臨海鉄道未対象の営業所については、タブ「タンク車明細」で積込指示を実施するため処理を復活 $$$$$$$$
                ''### 入換・積込業者とのやり取りを実施する運用を追加したため下記の処理を廃止(2020/03/30) ##############
                '臨海鉄道未対象の営業所((東北支店、関東支店(根岸のみ)、中部支店))は、
                '入換・積込指示の業務がないため、受注進行ステータスを"手配完了"に変更し、タブ「タンク車明細」へ業務を移行する。
                ''※但し、「三重塩浜営業所」は託送指示のみ業務があるため除外する。
                ''If WW_RINKAIFLG = False _
                ''    AndAlso Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_012402 Then
                If WW_RINKAIFLG = False Then
                    '〇(受注TBL)受注進行ステータス更新
                    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()       'DataBase接続

                        '### 20200722 START 受注進行ステータスの制御を変更 #################################
                        'WW_UpdateOrderStatus(BaseDllConst.CONST_ORDERSTATUS_310)
                        'CODENAME_get("ORDERSTATUS", BaseDllConst.CONST_ORDERSTATUS_310, Me.TxtOrderStatus.Text, WW_DUMMY)
                        'work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310
                        'work.WF_SEL_ORDERSTATUSNM.Text = Me.TxtOrderStatus.Text
                        '★205:手配中（千葉(根岸を除く)以外）に更新
                        WW_UpdateOrderStatus(BaseDllConst.CONST_ORDERSTATUS_205)
                        CODENAME_get("ORDERSTATUS", BaseDllConst.CONST_ORDERSTATUS_205, Me.TxtOrderStatus.Text, WW_DUMMY)
                        work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205
                        work.WF_SEL_ORDERSTATUSNM.Text = Me.TxtOrderStatus.Text
                        '### 20200722 END   受注進行ステータスの制御を変更 #################################

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

                        '### START 受注履歴テーブルの追加(2020/03/26) #############
                        WW_InsertOrderHistory(SQLcon)
                        '### END   ################################################
                    End Using

                    WF_DTAB_CHANGE_NO.Value = "2"
                    WF_Detail_TABChange()

                    ''〇タンク車所在の更新
                    'WW_TankShozaiSet()
                End If
                ''#####################################################################################################
                '$$$ 20200710 END  ((全体)No101対応) $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                '#####################################################################################################

            End If

        End If
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
            '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
            '310:手配完了
            'O_VALUE = CONST_ORDERSTATUS_310

            '### 特に何もしない #########################################################

            '### END   ##################################################################
            'Exit Sub

            '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
            '★臨海鉄道ではない営業所対応
            '　営業所＝"仙台新港営業所", 営業所＝"根岸営業所", 営業所＝"四日市営業所"
            '　受注進行ステータス＝"200:手配"
        ElseIf (work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_010402 _
                OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011402 _
                OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_012401) _
                AndAlso (work.WF_SEL_ORDERSTATUS.Text = CONST_ORDERSTATUS_200 _
                         OrElse work.WF_SEL_ORDERSTATUS.Text = CONST_ORDERSTATUS_300) Then

            '　手配連絡フラグが"1"(連絡)、かつ結果受理が"0"(未受理)の場合
            If work.WF_SEL_CONTACTFLG.Text = "1" AndAlso work.WF_SEL_RESULTFLG.Text = "0" Then
                '300:手配中(入換積込未確認)
                O_VALUE = CONST_ORDERSTATUS_300

                '　手配連絡フラグが"1"(連絡)、かつ結果受理が"1"(受理)の場合
            ElseIf work.WF_SEL_CONTACTFLG.Text = "1" AndAlso work.WF_SEL_RESULTFLG.Text = "1" Then
                '310:手配完了
                O_VALUE = CONST_ORDERSTATUS_310

            End If
            Exit Sub
            '### END   ##################################################################

            '　営業所＝"三重塩浜営業所"
            '　受注進行ステータス＝"200:手配"
        ElseIf Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012402 _
            AndAlso work.WF_SEL_ORDERSTATUS.Text = CONST_ORDERSTATUS_200 Then
            '　託送指示フラグが"1"(手配)の場合
            If work.WF_SEL_DELIVERYFLG.Text = "1" Then
                '290:手配中(入換積込未連絡)
                O_VALUE = CONST_ORDERSTATUS_290

                '　手配連絡フラグが"1"(連絡)の場合
            ElseIf work.WF_SEL_CONTACTFLG.Text = "1" Then
                '270:手配中(入換積込指示手配済)
                O_VALUE = CONST_ORDERSTATUS_270

            End If

            Exit Sub

        End If

        '◯受注進行ステータスの状態によって値を更新
        '　入換・積込
        Select Case work.WF_SEL_ORDERSTATUS.Text
                '受注進行ステータス＝"200:手配"
                '受注進行ステータス＝"210:手配中(入換指示入力済)"
                '受注進行ステータス＝"220:手配中(積込指示入力済)"
                '受注進行ステータス＝"230:手配中(託送指示手配済)"
                '受注進行ステータス＝"240:手配中(入換指示未入力)"
                '受注進行ステータス＝"250:手配中(積込指示未入力)"
                '受注進行ステータス＝"260:手配中(託送指示未手配)"
            Case BaseDllConst.CONST_ORDERSTATUS_200,
                 BaseDllConst.CONST_ORDERSTATUS_210,
                 BaseDllConst.CONST_ORDERSTATUS_220,
                 BaseDllConst.CONST_ORDERSTATUS_230,
                 BaseDllConst.CONST_ORDERSTATUS_240,
                 BaseDllConst.CONST_ORDERSTATUS_250,
                 BaseDllConst.CONST_ORDERSTATUS_260

                '入換指示入力＝"1:完了"
                'かつ、積込指示入力＝"1:未完了"
                'かつ、託送指示入力＝"1:未手配"の場合
                If WW_SwapInput = "1" AndAlso WW_LoadingInput = "1" AndAlso work.WF_SEL_DELIVERYFLG.Text = "1" Then
                    '290:手配中(入換積込未連絡)
                    O_VALUE = CONST_ORDERSTATUS_290

                    '入換指示入力＝"1:完了"
                    'かつ、積込指示入力＝"0:未完了"
                    'かつ、託送指示入力＝"0:未手配"の場合
                ElseIf WW_SwapInput = "1" AndAlso WW_LoadingInput = "0" AndAlso work.WF_SEL_DELIVERYFLG.Text = "0" Then
                    '手配中(入換指示入力済)
                    O_VALUE = CONST_ORDERSTATUS_210

                    '入換指示入力＝"0:未完了"
                    'かつ、積込指示入力＝"1:完了"
                    'かつ、託送指示入力＝"0:未手配"の場合
                ElseIf WW_SwapInput = "0" AndAlso WW_LoadingInput = "1" AndAlso work.WF_SEL_DELIVERYFLG.Text = "0" Then
                    '手配中(積込指示入力済)
                    O_VALUE = CONST_ORDERSTATUS_220

                    '入換指示入力＝"0:未完了"
                    'かつ、積込指示入力＝"0:未完了"
                    'かつ、託送指示入力＝"1:手配"の場合
                ElseIf WW_SwapInput = "0" AndAlso WW_LoadingInput = "0" AndAlso work.WF_SEL_DELIVERYFLG.Text = "1" Then
                    '手配中(託送指示手配済)
                    O_VALUE = CONST_ORDERSTATUS_230

                    '入換指示入力＝"0:未完了"
                    'かつ、積込指示入力＝"1:完了"
                    'かつ、託送指示入力＝"1:手配"の場合
                ElseIf WW_SwapInput = "0" AndAlso WW_LoadingInput = "1" AndAlso work.WF_SEL_DELIVERYFLG.Text = "1" Then
                    '手配中(入換指示未入力)
                    O_VALUE = CONST_ORDERSTATUS_240

                    '入換指示入力＝"1:完了"
                    'かつ、積込指示入力＝"0:未完了"
                    'かつ、託送指示入力＝"1:手配"の場合
                ElseIf WW_SwapInput = "1" AndAlso WW_LoadingInput = "0" AndAlso work.WF_SEL_DELIVERYFLG.Text = "1" Then
                    '手配中(積込指示未入力)
                    O_VALUE = CONST_ORDERSTATUS_250

                    '入換指示入力＝"1:完了"
                    'かつ、積込指示入力＝"1:完了"
                    'かつ、託送指示入力＝"0:未手配"の場合
                ElseIf WW_SwapInput = "1" AndAlso WW_LoadingInput = "1" AndAlso work.WF_SEL_DELIVERYFLG.Text = "0" Then
                    '手配中(託送指示未手配)
                    O_VALUE = CONST_ORDERSTATUS_260

                End If
        End Select

        '◯受注進行ステータスの状態によって値を更新
        '　入換・積込後の業者連絡
        Select Case work.WF_SEL_ORDERSTATUS.Text
                '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
                '受注進行ステータス＝"260:手配中(託送指示未手配)"
                '受注進行ステータス＝"270:手配中(入換積込指示手配済)"
                '受注進行ステータス＝"280:手配中(託送指示未手配)"入換積込手配連絡（手配・結果受理）
                '受注進行ステータス＝"290:手配中(入換積込未連絡)"
                '受注進行ステータス＝"300:手配中(入換積込未確認)"
            Case BaseDllConst.CONST_ORDERSTATUS_260,
                         BaseDllConst.CONST_ORDERSTATUS_270,
                         BaseDllConst.CONST_ORDERSTATUS_280,
                         BaseDllConst.CONST_ORDERSTATUS_290,
                         BaseDllConst.CONST_ORDERSTATUS_300

                '託送指示入力＝"1:手配"
                'かつ、手配連絡＝"1:連絡"
                'かつ、結果受理＝"1:受理"の場合
                If work.WF_SEL_DELIVERYFLG.Text = "1" _
                            AndAlso work.WF_SEL_CONTACTFLG.Text = "1" _
                            AndAlso work.WF_SEL_RESULTFLG.Text = "1" Then
                    '310:手配完了
                    O_VALUE = CONST_ORDERSTATUS_310

                    '託送指示入力＝"0:未手配"
                    'かつ、手配連絡＝"1:連絡"
                    'かつ、結果受理＝"0:未受理"の場合
                ElseIf work.WF_SEL_DELIVERYFLG.Text = "0" _
                            AndAlso work.WF_SEL_CONTACTFLG.Text = "1" _
                            AndAlso work.WF_SEL_RESULTFLG.Text = "0" Then
                    '270:手配中(入換積込指示手配済)
                    O_VALUE = CONST_ORDERSTATUS_270

                    '託送指示入力＝"0:未手配"
                    'かつ、手配連絡＝"1:連絡"
                    'かつ、結果受理＝"1:受理"の場合
                ElseIf work.WF_SEL_DELIVERYFLG.Text = "0" _
                            AndAlso work.WF_SEL_CONTACTFLG.Text = "1" _
                            AndAlso work.WF_SEL_RESULTFLG.Text = "1" Then
                    '### 20200722 START 受注進行ステータスの制御を変更 #################################
                    ''280:手配中(託送指示未手配)入換積込手配連絡（手配・結果受理）
                    'O_VALUE = CONST_ORDERSTATUS_280
                    '305:手配完了(託送未)
                    O_VALUE = CONST_ORDERSTATUS_305
                    '### 20200722 END   受注進行ステータスの制御を変更 #################################

                    '託送指示入力＝"1:手配"
                    'かつ、手配連絡＝"0:未連絡"
                    'かつ、結果受理＝"0:未受理"の場合
                ElseIf work.WF_SEL_DELIVERYFLG.Text = "1" _
                            AndAlso work.WF_SEL_CONTACTFLG.Text = "0" _
                            AndAlso work.WF_SEL_RESULTFLG.Text = "0" Then
                    '290:手配中(入換積込未連絡)
                    O_VALUE = CONST_ORDERSTATUS_290

                    '託送指示入力＝"1:手配"
                    'かつ、手配連絡＝"1:連絡"
                    'かつ、結果受理＝"0:未受理"の場合
                ElseIf work.WF_SEL_DELIVERYFLG.Text = "1" _
                            AndAlso work.WF_SEL_CONTACTFLG.Text = "1" _
                            AndAlso work.WF_SEL_RESULTFLG.Text = "0" Then
                    '300:手配中(入換積込未確認)
                    O_VALUE = CONST_ORDERSTATUS_300

                End If
                '### END   ##################################################################

                '### 20200722 START 受注進行ステータスの制御を追加 #################################
                '受注進行ステータス＝"305:手配完了（託送未）"
                '受注進行ステータス＝"205:手配中（千葉(根岸を除く)以外）"
            Case BaseDllConst.CONST_ORDERSTATUS_305,
                 BaseDllConst.CONST_ORDERSTATUS_205
                '310:手配完了
                O_VALUE = CONST_ORDERSTATUS_310
                '### 20200722 END   受注進行ステータスの制御を追加 #################################

        End Select
    End Sub

    ''' <summary>
    ''' 画面表示設定処理(受注進行ステータス(変更分を反映))
    ''' </summary>
    Protected Sub WW_ScreenOrderStatusChgRef(ByVal O_VALUE As String)

        '受注進行ステータスに変更があった場合
        If O_VALUE <> "" Then
            '〇(受注TBL)受注進行ステータス更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrderStatus(O_VALUE)
                CODENAME_get("ORDERSTATUS", O_VALUE, Me.TxtOrderStatus.Text, WW_DUMMY)
                work.WF_SEL_ORDERSTATUS.Text = O_VALUE
                work.WF_SEL_ORDERSTATUSNM.Text = Me.TxtOrderStatus.Text

            End Using

            '○ 画面表示データ復元
            Master.RecoverTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)

            For Each OIT0003row As DataRow In OIT0003WKtbl.Rows
                If OIT0003row("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text Then
                    OIT0003row("ORDERSTATUS") = O_VALUE
                    OIT0003row("ORDERSTATUSNAME") = Me.TxtOrderStatus.Text
                End If
            Next

            '○ 画面表示データ保存
            Master.SaveTable(OIT0003WKtbl, work.WF_SEL_INPTBL.Text)
        End If

        '〇 受注ステータスが"310:手配完了"へ変更された場合
        '### 20200722 受注進行ステータスの制御を追加 #################################
        '205:手配中（千葉(根岸を除く)以外）
        '305:手配完了（託送未）
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then
            WF_DTAB_CHANGE_NO.Value = "2"
            WF_Detail_TABChange()

            '### START 受注履歴テーブルの追加(2020/03/26) #############
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_InsertOrderHistory(SQLcon)
            End Using
            '### END   ################################################

            '○メッセージ表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

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

            '200:手配, 210～300:手配中の場合は、タブ「タンク車割当」、タブ「入換指示・積込指示」を許可
            '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
            '   受注進行ステータス＝"270:手配中(入換積込指示手配済)"
            '   受注進行ステータス＝"280:手配中(託送指示未手配)"入換積込手配連絡（手配・結果受理）
            '   受注進行ステータス＝"290:手配中(入換積込未連絡)"
            '   受注進行ステータス＝"300:手配中(入換積込未確認)"
            '### END   ##################################################################
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_210 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_220 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_230 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_240 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_250 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_280 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_290 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_300 Then
            WF_Dtab01.Enabled = True
            WF_Dtab02.Enabled = True
            WF_Dtab03.Enabled = False
            WF_Dtab04.Enabled = False
            pnlSummaryArea.Visible = False

            '310:手配完了の場合は、タブ「タンク車明細」を許可
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
            '305:手配完了（託送未）
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then
            WF_Dtab01.Enabled = True
            WF_Dtab02.Enabled = True
            WF_Dtab03.Enabled = True
            WF_Dtab04.Enabled = False
            pnlSummaryArea.Visible = True

            '### 20200811 START 千葉(臨海)以外の営業所は、タブ「入換指示・積込指示」を許可しない ##########
            If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_010402 _
                OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011402 _
                OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012401 _
                OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012402 Then
                WF_Dtab02.Enabled = False
            End If
            '### 20200811 END   千葉(臨海)以外の営業所は、タブ「入換指示・積込指示」を許可しない ##########

            '上記以外は、タブ「費用入力」の許可
        Else
            WF_Dtab01.Enabled = True
            WF_Dtab02.Enabled = True
            WF_Dtab03.Enabled = True
            WF_Dtab04.Enabled = True
            pnlSummaryArea.Visible = True

            '### 20200811 START 千葉(臨海)以外の営業所は、タブ「入換指示・積込指示」を許可しない ##########
            If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_010402 _
                OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011402 _
                OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012401 _
                OrElse Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_012402 Then
                WF_Dtab02.Enabled = False
            End If
            '### 20200811 END   千葉(臨海)以外の営業所は、タブ「入換指示・積込指示」を許可しない ##########
        End If

        '〇 受注内容の制御
        '100:受注受付以外の場合は、受注内容(ヘッダーの内容)の変更を不可とする。
        If work.WF_SEL_ORDERSTATUS.Text <> BaseDllConst.CONST_ORDERSTATUS_100 Then
            '受注営業所
            Me.TxtOrderOffice.Enabled = False
            '本線列車
            Me.TxtTrainNo.Enabled = False
            '荷主
            Me.TxtShippersCode.Enabled = False
            '荷受人
            Me.TxtConsigneeCode.Enabled = False
            '発駅
            Me.TxtDepstationCode.Enabled = False
            '着駅
            Me.TxtArrstationCode.Enabled = False
            '(予定)積込日
            Me.TxtLoadingDate.Enabled = False
            '(予定)発日
            Me.TxtDepDate.Enabled = False
            '(予定)積車着日
            Me.TxtArrDate.Enabled = False
            '(予定)受入日
            Me.TxtAccDate.Enabled = False
            '(予定)空車着日
            Me.TxtEmparrDate.Enabled = False

            '### 20200512-START 油種数登録制御追加 ###################################
            Me.TxtHTank_w.Enabled = False
            Me.TxtRTank_w.Enabled = False
            Me.TxtTTank_w.Enabled = False
            Me.TxtMTTank_w.Enabled = False
            Me.TxtKTank_w.Enabled = False
            Me.TxtK3Tank_w.Enabled = False
            Me.TxtK5Tank_w.Enabled = False
            Me.TxtK10Tank_w.Enabled = False
            Me.TxtLTank_w.Enabled = False
            Me.TxtATank_w.Enabled = False
            '### 20200512-END   ######################################################
        Else
            '受注営業所
            Me.TxtOrderOffice.Enabled = True
            '本線列車
            Me.TxtTrainNo.Enabled = True
            '荷主
            Me.TxtShippersCode.Enabled = True
            '荷受人
            Me.TxtConsigneeCode.Enabled = True
            '発駅
            Me.TxtDepstationCode.Enabled = True
            '着駅
            Me.TxtArrstationCode.Enabled = True

            '### 20200623 START((全体)No76対応) ######################################
            '◯本線列車名が未設定の場合
            '　(日付の自動設定を行うには、本線列車名が必要なため制御をかける)
            '### 20200623 END  ((全体)No76対応) ######################################
            If Me.TxtTrainName.Text = "" Then
                '(予定)積込日
                Me.TxtLoadingDate.Enabled = False
                '(予定)発日
                Me.TxtDepDate.Enabled = False
                '(予定)積車着日
                Me.TxtArrDate.Enabled = False
                '(予定)受入日
                Me.TxtAccDate.Enabled = False
                '(予定)空車着日
                Me.TxtEmparrDate.Enabled = False
            Else
                '(予定)積込日
                Me.TxtLoadingDate.Enabled = True
                '(予定)発日
                Me.TxtDepDate.Enabled = True
                '(予定)積車着日
                Me.TxtArrDate.Enabled = True
                '(予定)受入日
                Me.TxtAccDate.Enabled = True
                '(予定)空車着日
                Me.TxtEmparrDate.Enabled = True
            End If

            '### 20200512-START 油種数登録制御追加 ###################################
            '★新規受注作成時のみ
            If work.WF_SEL_CREATEFLG.Text = "1" _
                AndAlso work.WF_SEL_CREATELINKFLG.Text = "1" Then
                '画面表示(油種数)設定処理
                WW_ScreenOilEnabledSet()
            End If
            '### 20200512-END   ######################################################

        End If

        '〇 (実績)の日付の入力可否制御
        '受注情報が以下の場合は、(実績)の日付の入力を制限
        '100:受注受付, 200:手配, 210:手配中（入換指示入力済）, 220:手配中（積込指示入力済）
        '230:手配中（託送指示手配済）, 240:手配中（入換指示未入力）, 250:手配中（積込指示未入力）
        '260:手配中（託送指示未手配）
        '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
        '270:手配中(入換積込指示手配済), 280:手配中(託送指示未手配)入換積込手配連絡（手配・結果受理）
        '290:手配中(入換積込未連絡), 300:手配中(入換積込未確認)
        '### END   ##################################################################
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_210 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_220 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_230 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_240 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_250 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_280 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_290 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_300 Then

            '(実績)積込日
            Me.TxtActualLoadingDate.Enabled = False
            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = False
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = False
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = False

            '受注情報が「310:手配完了」の場合は、(実績)すべての日付の入力を解放
            '310:手配完了
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
            '305:手配完了（託送未）
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then
            '(実績)積込日
            Me.TxtActualLoadingDate.Enabled = True
            '(実績)発日
            Me.TxtActualDepDate.Enabled = True
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = True
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = True
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True

            '受注情報が「320:受注確定」の場合は、(実績)積込日の入力を制限
            '320:受注確定
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 Then
            '(実績)積込日
            Me.TxtActualLoadingDate.Enabled = False
            '(実績)発日
            Me.TxtActualDepDate.Enabled = True
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = True
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = True
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True

            '### ステータス追加(仮) #################################
            '受注情報が「350:受注確定」の場合は、(実績)発日の入力を制限
            '350:受注確定((実績)発日入力済み)
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 Then
            '(実績)積込日
            Me.TxtActualLoadingDate.Enabled = False
            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = True
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = True
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True
            '########################################################

            '受注情報が「400:受入確認中」の場合は、(実績)積車着日の入力を制限
            '400:受入確認中
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 Then
            '(実績)積込日
            Me.TxtActualLoadingDate.Enabled = False
            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = False
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = True
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True

            '### ステータス追加(仮) #################################
            '受注情報が「450:受入確認中」の場合は、(実績)積車着日の入力を制限
            '450:受入確認中((実績)受入日入力済み)
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
            '(実績)積込日
            Me.TxtActualLoadingDate.Enabled = False
            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = False
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = False
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True
            '########################################################

            '受注情報が「500:検収中」の場合は、(実績)空車着日の入力を制限
            '500:検収中
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 Then
            '(実績)積込日
            Me.TxtActualLoadingDate.Enabled = False
            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = False
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = False
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = False


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
            Me.TxtActualLoadingDate.Enabled = True
            '(実績)発日
            Me.TxtActualDepDate.Enabled = True
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = True
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = True
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True
        End If

    End Sub

    ''' <summary>
    ''' 画面表示(油種数)設定処理
    ''' </summary>
    Protected Sub WW_ScreenOilEnabledSet()

        '〇各営業者で管理している油種を取得
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        If Me.TxtOrderOfficeCode.Text = "" Then
            WW_FixvalueMasterSearch(Master.USER_ORG, "PRODUCTPATTERN", "", WW_GetValue, I_PARA01:="1")
        Else
            WW_FixvalueMasterSearch(Me.TxtOrderOfficeCode.Text, "PRODUCTPATTERN", "", WW_GetValue, I_PARA01:="1")
        End If

        '〇初期化
        'ハイオク
        Me.TxtHTank_w.Enabled = False
        'レギュラー
        Me.TxtRTank_w.Enabled = False
        '灯油
        Me.TxtTTank_w.Enabled = False
        '未添加灯油
        Me.TxtMTTank_w.Enabled = False
        '軽油
        Me.TxtKTank_w.Enabled = False
        '３号軽油
        Me.TxtK3Tank_w.Enabled = False
        '軽油５
        Me.TxtK5Tank_w.Enabled = False
        '軽油１０
        Me.TxtK10Tank_w.Enabled = False
        'ＬＳＡ
        Me.TxtLTank_w.Enabled = False
        'Ａ重油
        Me.TxtATank_w.Enabled = False

        For i As Integer = 0 To WW_GetValue.Length - 1
            Select Case WW_GetValue(i)
                    'ハイオク
                Case BaseDllConst.CONST_HTank
                    Me.TxtHTank_w.Enabled = True
                    'レギュラー
                Case BaseDllConst.CONST_RTank
                    Me.TxtRTank_w.Enabled = True
                    '灯油
                Case BaseDllConst.CONST_TTank
                    '### 20200615 START((全体)No73対応) ##########################################
                    '★根岸営業所の場合
                    If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011402 Then
                        '★JXTG北信油槽所, 及びJXTG甲府油槽所の場合
                        If Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_10 _
                            OrElse Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_20 Then
                            '入力を未許可にする。
                            Me.TxtTTank_w.Enabled = False
                        Else
                            Me.TxtTTank_w.Enabled = True
                        End If
                    Else
                        Me.TxtTTank_w.Enabled = True
                    End If
                    '### 20200615 END  ((全体)No73対応) ##########################################
                    '未添加灯油
                Case BaseDllConst.CONST_MTTank
                    '★根岸営業所の場合
                    If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011402 Then
                        '### 20200615 START((全体)No73対応) ##########################################
                        '★JXTG北信油槽所, 及びJXTG甲府油槽所の場合
                        If Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_10 _
                            OrElse Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_20 Then
                            '入力を許可する。
                            Me.TxtMTTank_w.Enabled = True
                        Else
                            Me.TxtMTTank_w.Enabled = False
                        End If
                        '### 20200615 END  ((全体)No73対応) ##########################################
                    Else
                        Me.TxtMTTank_w.Enabled = True
                    End If
                    '軽油
                Case BaseDllConst.CONST_KTank1, BaseDllConst.CONST_KTank2
                    Me.TxtKTank_w.Enabled = True
                    '３号軽油
                Case BaseDllConst.CONST_K3Tank1, BaseDllConst.CONST_K3Tank2
                    Me.TxtK3Tank_w.Enabled = True
                    '軽油５
                Case BaseDllConst.CONST_K5Tank
                    Me.TxtK5Tank_w.Enabled = True
                    '軽油１０
                Case BaseDllConst.CONST_K10Tank
                    Me.TxtK10Tank_w.Enabled = True
                    'ＬＳＡ
                Case BaseDllConst.CONST_LTank1, BaseDllConst.CONST_LTank2
                    '### 20200706 START((全体)No100対応) ##########################################
                    'Me.TxtLTank_w.Enabled = True
                    '★OT八王子の場合
                    If Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_55 Then
                        Me.TxtLTank_w.Enabled = False
                        Me.TxtLTank_w.Text = 0
                    Else
                        Me.TxtLTank_w.Enabled = True
                    End If
                    '### 20200706 END  ((全体)No100対応) ##########################################
                    'Ａ重油
                Case BaseDllConst.CONST_ATank
                    '### 20200706 START((全体)No100対応) ##########################################
                    'Me.TxtATank_w.Enabled = True
                    '★OT八王子の場合
                    If Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_55 Then
                        Me.TxtATank_w.Enabled = False
                        Me.TxtATank_w.Text = 0
                    Else
                        Me.TxtATank_w.Enabled = True
                    End If
                    '### 20200706 END  ((全体)No100対応) ##########################################
            End Select
        Next
    End Sub

    ''' <summary>
    ''' タンク車所在設定処理
    ''' </summary>
    Protected Sub WW_TankShozaiSet()

        '〇タンク車所在の更新
        '受注進行ステータスが以下の場合
        '100:受注受付
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then

            '★割当確定ボタン押下時に更新
            If Me.WW_UPBUTTONFLG = "1" AndAlso isNormal(WW_ERRCODE) Then
                '★タンク車所在の更新
                '引数１：所在地コード　⇒　変更なし(空白)
                '引数２：タンク車状態　⇒　変更あり("1"(発送))
                '引数３：積車区分　　　⇒　変更なし(空白)
                '引数４：(予定)空車着日⇒　更新対象(画面項目)
                WW_UpdateTankShozai("", "1", "", upEmparrDate:=True)
            End If

            '受注進行ステータスが以下の場合
            '200:手配
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 Then

            '★割当確定ボタン押下時に更新
            If Me.WW_UPBUTTONFLG = "1" AndAlso isNormal(WW_ERRCODE) Then
                '★タンク車所在の更新
                '引数１：所在地コード　⇒　変更なし(空白)
                '引数２：タンク車状態　⇒　変更あり("1"(発送))
                '引数３：積車区分　　　⇒　変更なし(空白)
                '引数４：(予定)空車着日⇒　更新対象(画面項目)
                WW_UpdateTankShozai("", "1", "", upEmparrDate:=True)
            End If

            '受注進行ステータスが以下の場合
            '210:手配中（入換指示入力済）, 220:手配中（積込指示入力済）
            '230:手配中（託送指示手配済）, 240:手配中（入換指示未入力）, 250:手配中（積込指示未入力）
            '260:手配中（託送指示未手配）
            '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
            '270:手配中(入換積込指示手配済), 280:手配中(託送指示未手配)入換積込手配連絡（手配・結果受理）
            '290:手配中(入換積込未連絡), 300:手配中(入換積込未確認)
            '### END   ##################################################################
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_210 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_220 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_230 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_240 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_250 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_280 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_290 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_300 Then

            '### 特になし ###############################################################

            '受注進行ステータスが「310:手配完了」の場合
            '310:手配完了
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '305:手配完了（託送未）
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then

            '★割当確定ボタン押下時に更新
            If Me.WW_UPBUTTONFLG = "1" AndAlso isNormal(WW_ERRCODE) Then
                '★タンク車所在の更新
                '引数１：所在地コード　⇒　変更なし(空白)
                '引数２：タンク車状態　⇒　変更あり("1"(発送))
                '引数３：積車区分　　　⇒　変更なし(空白)
                '引数４：(予定)空車着日⇒　更新対象(画面項目)
                WW_UpdateTankShozai("", "1", "", upEmparrDate:=True)
            End If

            '受注進行ステータスが「320:受注確定」の場合
            '320:受注確定
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 Then

            '★明細更新ボタン押下時に更新
            If Me.WW_UPBUTTONFLG = "3" AndAlso isNormal(WW_ERRCODE) Then
                '引数１：所在地コード　⇒　変更なし(空白)
                '引数２：タンク車状態　⇒　変更なし(空白)
                '引数３：積車区分　　　⇒　変更あり("F"(積車))
                '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                WW_UpdateTankShozai("", "1", "F", I_SITUATION:="2", upLastOilCode:=True)

                '(実績)発日の入力が完了
                If Me.TxtActualDepDate.Text <> "" Then
                    '★タンク車所在の更新
                    '引数１：所在地コード　⇒　変更あり(着駅)
                    '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
                    '引数３：積車区分　　　⇒　変更なし(空白)
                    '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                    '### 20200828 START 前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                    '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                    'WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "2", "F", I_SITUATION:="2")
                    WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "2", "F", I_SITUATION:="2", upLastOilCode:=True)
                    '### 20200828 END   前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                End If
            End If

            '### ステータス追加(仮) #################################
            '受注進行ステータスが「300:受注確定」の場合
            '350:受注確定
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 Then

            '★明細更新ボタン押下時に更新
            If Me.WW_UPBUTTONFLG = "3" AndAlso isNormal(WW_ERRCODE) Then
                '引数１：所在地コード　⇒　変更あり(着駅)
                '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
                '引数３：積車区分　　　⇒　変更あり("F"(積車))
                '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                '### 20200828 START 前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                'WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "2", "F", I_SITUATION:="2")
                WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "2", "F", I_SITUATION:="2", upLastOilCode:=True)
                '### 20200828 END   前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
            End If
            '########################################################

            '受注進行ステータスが「400:受入確認中」の場合
            '400:受入確認中
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 Then

            '★明細更新ボタン押下時に更新
            If Me.WW_UPBUTTONFLG = "3" AndAlso isNormal(WW_ERRCODE) Then
                '★タンク車所在の更新
                '引数１：所在地コード　⇒　変更あり(着駅)
                '引数２：タンク車状態　⇒　変更あり("3"(到着))
                '引数３：積車区分　　　⇒　変更あり("F"(積車))
                '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                '### 20200828 START 前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                'WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "3", "F", I_SITUATION:="2")
                WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "3", "F", I_SITUATION:="2", upLastOilCode:=True)
                '### 20200828 END   前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 

                '(実績)受入日の入力が完了
                If Me.TxtActualAccDate.Text <> "" Then
                    '★タンク車所在の更新
                    '引数１：所在地コード　⇒　変更あり(着駅)
                    '引数２：タンク車状態　⇒　変更あり("3"(到着))
                    '引数３：積車区分　　　⇒　変更あり("E"(空車))
                    '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                    '### 20200828 START 前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                    '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                    'WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "3", "E", I_SITUATION:="2")
                    WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "3", "E", I_SITUATION:="2", upLastOilCode:=True)
                    '### 20200828 END   前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                End If
            End If

            '### ステータス追加(仮) #################################
            '受注進行ステータスが「450:受入確認中」の場合
            '450:受入確認中
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then

            '★明細更新ボタン押下時に更新
            If Me.WW_UPBUTTONFLG = "3" AndAlso isNormal(WW_ERRCODE) Then
                '★タンク車所在の更新
                '引数１：所在地コード　⇒　変更あり(着駅)
                '引数２：タンク車状態　⇒　変更あり("3"(到着))
                '引数３：積車区分　　　⇒　変更あり("E"(空車))
                '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                '### 20200828 START 前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                'WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "3", "E", I_SITUATION:="2")
                WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "3", "E", I_SITUATION:="2", upLastOilCode:=True)
                '### 20200828 END   前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
            End If
            '########################################################

            '受注進行ステータスが「500:検収中」の場合
            '500:検収中
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 Then

            '★明細更新ボタン押下時に更新
            If Me.WW_UPBUTTONFLG = "3" AndAlso isNormal(WW_ERRCODE) Then
                '割り当てたタンク車のチェック
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

                    '★(一覧)タンク車NoがOT本社、または在日米軍のリース車かチェック
                    WW_FixvalueMasterSearch("ZZ", "TANKNO_OTCHECK", OIT0003row("TANKNO"), WW_GetValue)

                    'タンク車がOT本社、または在日米軍のリース車の場合
                    If WW_GetValue(0) <> "" Then

                        '### 特に何もしない ####################################

                        ''★タンク車所在の更新(### 所在地はそのまま更新しない###)
                        ''引数１：所在地コード　⇒　変更なし(空白)
                        ''引数２：タンク車状態　⇒　変更あり("3"(到着))
                        ''引数３：積車区分　　　⇒　変更あり("E"(空車))
                        'WW_UpdateTankShozai("", "3", "E")

                    Else
                        '★タンク車所在の更新
                        '引数１：所在地コード　⇒　変更あり(発駅)
                        '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
                        '引数３：積車区分　　　⇒　変更あり("E"(空車))
                        '引数４：タンク車状況　⇒　変更あり("1"(残車))
                        '### 20200828 START 前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                        '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                        'WW_UpdateTankShozai(Me.TxtDepstationCode.Text, "2", "E", I_SITUATION:="1", upActualEmparrDate:=True)
                        WW_UpdateTankShozai(Me.TxtDepstationCode.Text, "2", "E", I_SITUATION:="1", upActualEmparrDate:=True, upLastOilCode:=True)
                        '### 20200828 END   前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 

                        '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
                        '受注オーダーしているタンク車の存在確認
                        WW_FindOrderTank()
                        '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################

                    End If
                Next
            End If

            '550:検収済
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 Then
            '### 特になし ###############################################################

            '600:費用確定
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 Then
            '### 特になし ###############################################################

            '700:経理未計上
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 Then
            '### 特になし ###############################################################

            '800:経理計上
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 Then
            '### 特になし ###############################################################

            '900:受注キャンセル
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_900 Then
            '### 特になし ###############################################################

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

                        '### 特に何もしない ###############

                        '■タンク車割当状況＝"不可"の場合
                    Case CONST_TANKNO_STATUS_FUKA

                        '### 特に何もしない ###############

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
    ''' 受注№存在チェック(オーダーしたタンク車№が他のオーダーで使用していないか)
    ''' </summary>
    Protected Sub WW_OrderNoExistChk()
        '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
        For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows

            If OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_101 Then
                OIT0003tab3row("ORDERINFO") = ""
                OIT0003tab3row("ORDERINFONAME") = ""
            End If

            '指定タンク車№が他の受注オーダーで使用中の場合は、(実績)日付を許可しない。
            If OIT0003tab3row("USEORDERNO") = "" Then
                Continue For
            ElseIf OIT0003tab3row("USEORDERNO") <> Me.TxtOrderNo.Text Then
                '(実績)積込日
                Me.TxtActualLoadingDate.Enabled = False
                '(実績)発日
                Me.TxtActualDepDate.Enabled = False
                '(実績)積車着日
                Me.TxtActualArrDate.Enabled = False
                '(実績)受入日
                Me.TxtActualAccDate.Enabled = False
                '(実績)空車着日
                Me.TxtActualEmparrDate.Enabled = False

                '★他の受注オーダー情報を取得
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続
                    WW_SelectOrder(SQLcon,
                                   I_ORDERNO:=OIT0003tab3row("USEORDERNO"),
                                   O_dtORDER:=OIT0003FID2tbl_tab3,
                                   I_TANKNO:=OIT0003tab3row("TANKNO"))
                End Using

                Dim sOrderInfo As String = "利用中 "
                sOrderInfo &= OIT0003FID2tbl_tab3.Rows(0)("TRAINNO_NM") + " "
                sOrderInfo &= Date.Parse(OIT0003FID2tbl_tab3.Rows(0)("DEPDATE")).ToString("MM/dd") + "発分"

                OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_101
                'CODENAME_get("ORDERINFO", OIT0003tab3row("ORDERINFO"), OIT0003tab3row("ORDERINFONAME"), WW_DUMMY)
                OIT0003tab3row("ORDERINFONAME") = sOrderInfo

            End If
        Next
        Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)
        '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
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
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", Me.TxtOrderOfficeCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        'Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", work.WF_SEL_SALESOFFICECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SALESOFFICE", Me.TxtOrderOfficeCode.Text, Me.TxtOrderOffice.Text, WW_RTN_SW)
            'CODENAME_get("SALESOFFICE", work.WF_SEL_SALESOFFICECODE.Text, TxtOrderOffice.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "受注営業所 : " & Me.TxtOrderOfficeCode.Text)
                Me.TxtOrderOffice.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            Me.TxtOrderOffice.Focus()
            WW_CheckMES1 = "受注営業所入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '本線列車
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", Me.TxtTrainNo.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "本線列車", needsPopUp:=True)
            Me.TxtTrainNo.Focus()
            WW_CheckMES1 = "本線列車入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '荷主
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHIPPERSCODE", Me.TxtShippersCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SHIPPERS", Me.TxtShippersCode.Text, Me.LblShippersName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "荷主 : " & Me.TxtShippersCode.Text)
                Me.TxtShippersCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            Me.TxtShippersCode.Focus()
            WW_CheckMES1 = "荷主入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '荷受人
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CONSIGNEECODE", Me.TxtConsigneeCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CONSIGNEE", Me.TxtConsigneeCode.Text, Me.LblConsigneeName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "荷受人 : " & Me.TxtConsigneeCode.Text)
                Me.TxtConsigneeCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            Me.TxtConsigneeCode.Focus()
            WW_CheckMES1 = "荷受人入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '発駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", Me.TxtDepstationCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("DEPSTATION", Me.TxtDepstationCode.Text, Me.LblDepstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "発駅 : " & Me.TxtDepstationCode.Text)
                Me.TxtDepstationCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発駅", needsPopUp:=True)
            Me.TxtDepstationCode.Focus()
            WW_CheckMES1 = "発駅入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '着駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRSTATION", Me.TxtArrstationCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "着駅 : " & Me.TxtArrstationCode.Text)
                Me.TxtArrstationCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "着駅", needsPopUp:=True)
            Me.TxtArrstationCode.Focus()
            WW_CheckMES1 = "着駅入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積込日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LODDATE", Me.TxtLoadingDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(Me.TxtLoadingDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else

            '年月日チェック
            WW_CheckDate(Me.TxtLoadingDate.Text, "(予定)積込日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)積込日", needsPopUp:=True)
            Me.TxtLoadingDate.Focus()
            WW_CheckMES1 = "積込日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)発日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPDATE", Me.TxtDepDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(Me.TxtDepDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            '年月日チェック
            WW_CheckDate(Me.TxtDepDate.Text, "(予定)発日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)発日", needsPopUp:=True)
            Me.TxtDepDate.Focus()
            WW_CheckMES1 = "発日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積車着日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRDATE", Me.TxtArrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(Me.TxtArrDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            '年月日チェック
            WW_CheckDate(Me.TxtArrDate.Text, "(予定)積車着日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)積車着日", needsPopUp:=True)
            Me.TxtArrDate.Focus()
            WW_CheckMES1 = "積車着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)受入日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACCDATE", Me.TxtAccDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(Me.TxtAccDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            '年月日チェック
            WW_CheckDate(Me.TxtAccDate.Text, "(予定)受入日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)受入日", needsPopUp:=True)
            Me.TxtAccDate.Focus()
            WW_CheckMES1 = "受入日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)空車着日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EMPARRDATE", Me.TxtEmparrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(Me.TxtEmparrDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            '年月日チェック
            WW_CheckDate(Me.TxtEmparrDate.Text, "(予定)空車着日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)空車着日", needsPopUp:=True)
            Me.TxtEmparrDate.Focus()
            WW_CheckMES1 = "空車着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(一覧)チェック(準備)
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            If OIT0003row("ORDERINFO") <> BaseDllConst.CONST_ORDERINFO_ALERT_102 Then
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = ""
            End If
        Next
        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        'タンク車Noでソートし、重複がないかチェックする。
        Dim OIT0003tbl_DUMMY As DataTable = OIT0003tbl.Copy
        'OIT0003tbl_DUMMY.Columns.Add("TANKNO_SORT", GetType(Integer), "Convert(TANKNO, 'System.Int32')")
        'OIT0003tbl_DUMMY.Columns.Add("SHIPORDER_SORT", GetType(Integer), "Convert(SHIPORDER, 'System.Int32')")
        'OIT0003tbl_DUMMY.Columns.Add("LINEORDER_SORT", GetType(Integer), "Convert(LINEORDER, 'System.Int32')")
        OIT0003tbl_DUMMY.Columns.Add("TANKNO_SORT", GetType(Integer))
        For Each OIT0003row As DataRow In OIT0003tbl_DUMMY.Rows
            Try
                OIT0003row("TANKNO_SORT") = OIT0003row("TANKNO")
            Catch ex As Exception
                OIT0003row("TANKNO_SORT") = 0
            End Try
        Next
        OIT0003tbl_DUMMY.Columns.Add("SHIPORDER_SORT", GetType(Integer))
        For Each OIT0003row As DataRow In OIT0003tbl_DUMMY.Rows
            Try
                OIT0003row("SHIPORDER_SORT") = OIT0003row("SHIPORDER")
            Catch ex As Exception
                OIT0003row("SHIPORDER_SORT") = 0
            End Try
        Next
        OIT0003tbl_DUMMY.Columns.Add("LINEORDER_SORT", GetType(Integer))
        For Each OIT0003row As DataRow In OIT0003tbl_DUMMY.Rows
            Try
                OIT0003row("LINEORDER_SORT") = OIT0003row("LINEORDER")
            Catch ex As Exception
                OIT0003row("LINEORDER_SORT") = 0
            End Try
        Next

        Dim OIT0003tbl_dv As DataView = New DataView(OIT0003tbl_DUMMY)
        Dim chkTankNo As String = ""
        Dim chkShipOrder As String = ""
        Dim chkLineOrder As String = ""
        'OIT0003tbl_dv.Sort = "TANKNO"
        OIT0003tbl_dv.Sort = "TANKNO_SORT"
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

        '◯列車マスタ(発送順区分)が対象(1:発送対象)の場合チェックを実施
        '　※上記以外(2:発送対象外)については、入力しないためチェックは未実施。
        WW_SHIPORDER = "0"
        If work.WF_SEL_SHIPORDERCLASS.Text = "1" Then
            '### START 2020/03/26 発送順を追加したため合わせてチェックを追加 ######################################
            '発送順でソートし、重複がないかチェックする。
            'OIT0003tbl_dv.Sort = "SHIPORDER"
            OIT0003tbl_dv.Sort = "SHIPORDER_SORT"
            For Each drv As DataRowView In OIT0003tbl_dv

                '### 20200902 START 発送順チェック追加("0"の場合はエラーとする) ###################################
                If drv("HIDDEN") <> "1" AndAlso drv("SHIPORDER") = "0" Then
                    Master.Output(C_MESSAGE_NO.OIL_SHIPORDER_ZERO_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    WW_CheckMES1 = "発送順設定値0エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_SHIPORDER_ZERO_ERROR
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '### 20200902 END   発送順チェック追加("0"の場合はエラーとする) ###################################

                If drv("HIDDEN") <> "1" AndAlso drv("SHIPORDER") <> "" AndAlso chkShipOrder = drv("SHIPORDER") Then
                    Master.Output(C_MESSAGE_NO.OIL_SHIPORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    WW_CheckMES1 = "発送順重複エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_SHIPORDER_REPEAT_ERROR
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                    O_RTN = "ERR"
                    Exit Sub
                End If

                '行削除したデータの場合は退避しない。
                If drv("HIDDEN") <> "1" Then
                    chkShipOrder = drv("SHIPORDER")
                End If
            Next
            '### END  #############################################################################################
            WW_SHIPORDER = StrConv(chkShipOrder, VbStrConv.Narrow)
        End If

        '◯袖ヶ浦営業所のみ貨物駅入線順のチェックを実施
        '　※上記以外の営業所については、入力しないためチェックは未実施。
        If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011203 Then
            '貨物駅入線順でソートし、重複がないかチェックする。
            'OIT0003tbl_dv.Sort = "LINEORDER"
            OIT0003tbl_dv.Sort = "LINEORDER_SORT"
            For Each drv As DataRowView In OIT0003tbl_dv
                '### 20200902 START 入線順チェック追加("0"の場合はエラーとする) ###################################
                If drv("HIDDEN") <> "1" AndAlso drv("LINEORDER") = "0" Then
                    Master.Output(C_MESSAGE_NO.OIL_LINEORDER_ZERO_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    WW_CheckMES1 = "入線順設定値0エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_LINEORDER_ZERO_ERROR
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '### 20200902 END   入線順チェック追加("0"の場合はエラーとする) ###################################

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
        End If

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

            '◯列車マスタ(発送順区分)が対象(1:発送対象)の場合チェックを実施
            '　※上記以外(2:発送対象外)については、入力しないためチェックは未実施。
            If work.WF_SEL_SHIPORDERCLASS.Text = "1" Then
                '### START 2020/03/26 発送順を追加したため合わせてチェックを追加 ######################################
                '(一覧)発送順(空白チェック)
                If OIT0003row("SHIPORDER") = "" And OIT0003row("DELFLG") = "0" Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)発送順", needsPopUp:=True)

                    WW_CheckMES1 = "発送順未設定エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '### END  #############################################################################################

                '★数値(大文字)で設定されている場合は、数値(小文字)に変換する。
                OIT0003row("SHIPORDER") = StrConv(OIT0003row("SHIPORDER"), VbStrConv.Narrow)

            End If

            '◯袖ヶ浦営業所のみ貨物駅入線順のチェックを実施
            '　※上記以外の営業所については、入力しないためチェックは未実施。
            If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011203 Then
                '(一覧)貨物駅入線順(空白チェック)
                If OIT0003row("LINEORDER") = "" And OIT0003row("DELFLG") = "0" Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)貨物駅入線順", needsPopUp:=True)

                    WW_CheckMES1 = "貨物駅入線順未設定エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                    O_RTN = "ERR"
                    Exit Sub
                End If

                '★数値(大文字)で設定されている場合は、数値(小文字)に変換する。
                OIT0003row("SHIPORDER") = StrConv(OIT0003row("LINEORDER"), VbStrConv.Narrow)
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

            '### 20200701 START((全体)No96対応) ######################################
            '★指定したタンク車№が所属営業所以外の場合
            If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_102 _
                AndAlso OIT0003row("DELFLG") = "0" Then
                Master.Output(C_MESSAGE_NO.OIL_ORDER_NOT_BELONGOFFICE_TANKNO_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                WW_CheckMES1 = "タンク車No所属営業所以外。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_ORDER_NOT_BELONGOFFICE_TANKNO_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                Exit Sub
            End If
            '### 20200701 END  ((全体)No96対応) ######################################

            '### 20200831 START タンク車の所在地コード確認 ###########################
            '★指定したタンク車№が所在地以外の場合
            If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_101 _
                AndAlso OIT0003row("DELFLG") = "0" Then
                Master.Output(C_MESSAGE_NO.OIL_ORDER_NOT_LOCATION_TANKNO_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                WW_CheckMES1 = "タンク車No所在地以外。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_ORDER_NOT_LOCATION_TANKNO_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
                Exit Sub
            End If
            '### 20200831 END   タンク車の所在地コード確認 ###########################

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

            '### 20200616 START((全体)No74対応) ######################################
            '★袖ヶ浦営業所の場合
            If work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then
                '(一覧)回線(空白チェック)
                If OIT0003row("LINE") = "" And OIT0003row("DELFLG") = "0" Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)回線", needsPopUp:=True)

                    WW_CheckMES1 = "回線未設定エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    WW_CheckListTab2ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
            '### 20200616 END  ((全体)No74対応) ######################################

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
        If Me.TxtActualLoadingDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALLODDATE", Me.TxtActualLoadingDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(Me.TxtActualLoadingDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(Me.TxtActualLoadingDate.Text, "(実績)積込日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)積込日", needsPopUp:=True)
                Me.TxtActualLoadingDate.Focus()
                WW_CheckMES1 = "積込日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)発日
        If Me.TxtActualDepDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALDEPDATE", Me.TxtActualDepDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(Me.TxtActualDepDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(Me.TxtActualDepDate.Text, "(実績)発日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)発日", needsPopUp:=True)
                Me.TxtActualDepDate.Focus()
                WW_CheckMES1 = "発日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)積車着日
        If Me.TxtActualArrDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALARRDATE", Me.TxtActualArrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(Me.TxtActualArrDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(Me.TxtActualArrDate.Text, "(実績)積車着日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)積車着日", needsPopUp:=True)
                Me.TxtActualArrDate.Focus()
                WW_CheckMES1 = "積車着日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)受入日
        If Me.TxtActualAccDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALACCDATE", Me.TxtActualAccDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(Me.TxtActualAccDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(Me.TxtActualAccDate.Text, "(実績)受入日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)受入日", needsPopUp:=True)
                Me.TxtActualAccDate.Focus()
                WW_CheckMES1 = "受入日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)空車着日
        If Me.TxtActualEmparrDate.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACTUALEMPARRDATE", Me.TxtActualEmparrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(Me.TxtActualEmparrDate.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else

                '年月日チェック
                WW_CheckDate(Me.TxtActualEmparrDate.Text, "(実績)空車着日", WW_CS0024FCHECKERR)
                'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(実績)空車着日", needsPopUp:=True)
                Me.TxtActualEmparrDate.Focus()
                WW_CheckMES1 = "空車着日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '### 20200622 START((全体)No82対応) ######################################
        '発送順でソートし、重複がないかチェックする。
        Dim OIT0003tbltab3_DUMMY As DataTable = OIT0003tbl_tab3.Copy
        'OIT0003tbltab3_DUMMY.Columns.Add("SHIPORDER_SORT", GetType(Integer), "Convert(SHIPORDER, 'System.Int32')")
        OIT0003tbltab3_DUMMY.Columns.Add("SHIPORDER_SORT", GetType(Integer))
        For Each OIT0003row As DataRow In OIT0003tbltab3_DUMMY.Rows
            Try
                OIT0003row("SHIPORDER_SORT") = OIT0003row("SHIPORDER")
            Catch ex As Exception
                OIT0003row("SHIPORDER_SORT") = 0
            End Try
        Next
        Dim OIT0003tbltab3_dv As DataView = New DataView(OIT0003tbltab3_DUMMY)
        Dim chkShipOrder As String = ""

        '◯列車マスタ(発送順区分)が対象(1:発送対象)の場合チェックを実施
        '　※上記以外(2:発送対象外)については、入力しないためチェックは未実施。
        WW_SHIPORDER = "0"
        If work.WF_SEL_SHIPORDERCLASS.Text = "1" Then
            '発送順でソートし、重複がないかチェックする。
            OIT0003tbltab3_dv.Sort = "SHIPORDER_SORT"
            For Each drv As DataRowView In OIT0003tbltab3_dv

                '### 20200902 START 発送順チェック追加("0"の場合はエラーとする) ###################################
                If drv("HIDDEN") <> "1" AndAlso drv("SHIPORDER") = "0" Then
                    Master.Output(C_MESSAGE_NO.OIL_SHIPORDER_ZERO_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    WW_CheckMES1 = "発送順設定値0エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_SHIPORDER_ZERO_ERROR
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                    O_RTN = "ERR"
                    Exit Sub
                End If
                '### 20200902 END   発送順チェック追加("0"の場合はエラーとする) ###################################

                If drv("HIDDEN") <> "1" AndAlso drv("SHIPORDER") <> "" AndAlso chkShipOrder = drv("SHIPORDER") Then
                    Master.Output(C_MESSAGE_NO.OIL_SHIPORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                    WW_CheckMES1 = "発送順重複エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_SHIPORDER_REPEAT_ERROR
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                    O_RTN = "ERR"
                    Exit Sub
                End If

                '行削除したデータの場合は退避しない。
                If drv("HIDDEN") <> "1" Then
                    chkShipOrder = drv("SHIPORDER")
                End If
            Next
            WW_SHIPORDER = StrConv(chkShipOrder, VbStrConv.Narrow)
        End If

        '(一覧)チェック
        For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
            '◯列車マスタ(発送順区分)が対象(1:発送対象)の場合チェックを実施
            '　※上記以外(2:発送対象外)については、入力しないためチェックは未実施。
            If work.WF_SEL_SHIPORDERCLASS.Text = "1" Then
                '(一覧)発送順(空白チェック)
                If OIT0003tab3row("SHIPORDER") = "" And OIT0003tab3row("DELFLG") = "0" Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)発送順", needsPopUp:=True)

                    WW_CheckMES1 = "発送順未設定エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    WW_CheckListTab3ERR(WW_CheckMES1, WW_CheckMES2, OIT0003tab3row)
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Next
        '### 20200622 END  ((全体)No82対応) ######################################

    End Sub

    ''' <summary>
    ''' チェック処理(タブ「費用入力」)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTab4(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '(一覧)チェック
        '◯　計上年月日
        For Each OIT0003row As DataRow In OIT0003tbl_tab4.Rows
            If OIT0003row("CALCACCOUNT") = "1" Or OIT0003row("HIDDEN") <> "0" Then Continue For

            Try
                Date.TryParse(OIT0003row("KEIJYOYMD"), WW_STYMD)
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = ""
            Catch ex As Exception
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = "計上年月未入力"

                WW_CheckMES1 = "計上年月日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab4ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
            End Try
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)計上年月", needsPopUp:=True)
            Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)
            Exit Sub
        End If

        '◯　科目コード
        For Each OIT0003row As DataRow In OIT0003tbl_tab4.Rows
            If OIT0003row("ACCSEGCODE") = "" And OIT0003row("HIDDEN") = "0" And OIT0003row("CALCACCOUNT") <> "1" Then
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = "科目コード未入力"

                WW_CheckMES1 = "科目コード設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab4ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
            Else
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = ""
            End If
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)科目コード", needsPopUp:=True)
            Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)
            Exit Sub
        End If

        '◯　金額
        For Each OIT0003row As DataRow In OIT0003tbl_tab4.Rows
            If OIT0003row("APPLYCHARGESUM") = 0 And OIT0003row("HIDDEN") = "0" And OIT0003row("CALCACCOUNT") <> "1" Then
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = "金額未入力"

                WW_CheckMES1 = "金額設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab4ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
            Else
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = ""
            End If
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)金額", needsPopUp:=True)
            Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)
            Exit Sub
        End If

        '◯　請求先コード
        For Each OIT0003row As DataRow In OIT0003tbl_tab4.Rows
            If OIT0003row("INVOICECODE") = "" And OIT0003row("HIDDEN") = "0" And OIT0003row("CALCACCOUNT") <> "1" Then
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = "請求先コード未入力"

                WW_CheckMES1 = "請求先コード設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab4ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
            Else
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = ""
            End If
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)請求先コード", needsPopUp:=True)
            Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)
            Exit Sub
        End If

        '◯　支払先コード
        For Each OIT0003row As DataRow In OIT0003tbl_tab4.Rows
            If OIT0003row("PAYEECODE") = "" And OIT0003row("HIDDEN") = "0" And OIT0003row("CALCACCOUNT") <> "1" Then
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = "支払先コード未入力"

                WW_CheckMES1 = "支払先コード設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListTab4ERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR"
            Else
                OIT0003row("ORDERINFO") = ""
                OIT0003row("ORDERINFONAME") = ""
            End If
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)支払先コード", needsPopUp:=True)
            Master.SaveTable(OIT0003tbl_tab4, work.WF_SEL_INPTAB4TBL.Text)
            Exit Sub
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
        iresult = Date.Parse(Me.TxtLoadingDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)積込日", needsPopUp:=True)
            Me.TxtLoadingDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)発日 と　現在日付を比較
        iresult = Date.Parse(Me.TxtDepDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)発日", needsPopUp:=True)
            Me.TxtDepDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積車着日 と　現在日付を比較
        iresult = Date.Parse(Me.TxtArrDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)積車着日", needsPopUp:=True)
            Me.TxtArrDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)受入日 と　現在日付を比較
        iresult = Date.Parse(Me.TxtAccDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)受入日", needsPopUp:=True)
            Me.TxtAccDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)空車着日 と　現在日付を比較
        iresult = Date.Parse(Me.TxtEmparrDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)空車着日", needsPopUp:=True)
            Me.TxtEmparrDate.Focus()
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
        iresult = Date.Parse(Me.TxtLoadingDate.Text).CompareTo(Date.Parse(Me.TxtDepDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)積込日 > (予定)発日", needsPopUp:=True)
            Me.TxtDepDate.Focus()
            WW_CheckMES1 = "(予定日)入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        ElseIf iresult = -1 Then    '(予定)積込日 < (予定)発日の場合
            WW_ORDERINFOFLG_10 = True
            chkOrderInfo.Checked = True
        End If

        '(予定)発日 と　(予定)積車着日を比較
        iresult = Date.Parse(Me.TxtDepDate.Text).CompareTo(Date.Parse(Me.TxtArrDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)発日 > (予定)積車着日", needsPopUp:=True)
            Me.TxtArrDate.Focus()
            WW_CheckMES1 = "(予定日)入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積車着日 と　(予定)受入日を比較
        iresult = Date.Parse(Me.TxtArrDate.Text).CompareTo(Date.Parse(Me.TxtAccDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)積車着日 > (予定)受入日", needsPopUp:=True)
            Me.TxtAccDate.Focus()
            WW_CheckMES1 = "(予定日)入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)受入日 と　(予定)空車着日を比較
        iresult = Date.Parse(Me.TxtAccDate.Text).CompareTo(Date.Parse(Me.TxtEmparrDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)受入日 > (予定)空車着日", needsPopUp:=True)
            Me.TxtEmparrDate.Focus()
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
        '受注進行ステータスが"310:手配完了"の場合
        '### 20200722 受注進行ステータスの制御を追加 #################################
        '205:手配中（千葉(根岸を除く)以外）
        '305:手配完了（託送未）
        If Me.TxtActualLoadingDate.Text <> "" _
            AndAlso (work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
                    OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305) Then
            iresult = Date.Parse(Me.TxtActualLoadingDate.Text).CompareTo(DateTime.Today)
            'If iresult = -1 Then
            '    Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)積込日", needsPopUp:=True)
            '    Me.TxtActualLoadingDate.Focus()
            '    WW_CheckMES1 = "(実績日)過去日付エラー。"
            '    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            '    Exit Sub
            'End If
        End If

        '(実績)発日 と　現在日付を比較
        '受注進行ステータスが"320:受注確定"の場合
        If Me.TxtActualDepDate.Text <> "" _
            AndAlso work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 Then
            iresult = Date.Parse(Me.TxtActualDepDate.Text).CompareTo(DateTime.Today)
            'If iresult = -1 Then
            '    Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)発日", needsPopUp:=True)
            '    Me.TxtActualDepDate.Focus()
            '    WW_CheckMES1 = "(実績日)過去日付エラー。"
            '    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            '    Exit Sub
            'End If
        End If

        '(実績)積車着日 と　現在日付を比較
        '### ステータス追加(仮) #################################
        '受注進行ステータスが"350:受注確定((実績)発日設定済み)"の場合
        If Me.TxtActualArrDate.Text <> "" _
            AndAlso work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 Then
            iresult = Date.Parse(Me.TxtActualArrDate.Text).CompareTo(DateTime.Today)
            'If iresult = -1 Then
            '    Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)積車着日", needsPopUp:=True)
            '    Me.TxtActualArrDate.Focus()
            '    WW_CheckMES1 = "(実績日)過去日付エラー。"
            '    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            '    Exit Sub
            'End If
        End If

        '(実績)受入日 と　現在日付を比較
        '受注進行ステータスが"400:受入確認中"の場合
        If Me.TxtActualAccDate.Text <> "" _
           AndAlso work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 Then
            iresult = Date.Parse(Me.TxtActualAccDate.Text).CompareTo(DateTime.Today)
            'If iresult = -1 Then
            '    Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)受入日", needsPopUp:=True)
            '    Me.TxtActualAccDate.Focus()
            '    WW_CheckMES1 = "(実績日)過去日付エラー。"
            '    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            '    Exit Sub
            'End If
        End If

        '(実績)空車着日 と　現在日付を比較
        '### ステータス追加(仮) #################################
        '受注進行ステータスが"450:受入確認中((実績)受入日設定済み)"の場合
        If Me.TxtActualEmparrDate.Text <> "" _
            AndAlso work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
            iresult = Date.Parse(Me.TxtActualEmparrDate.Text).CompareTo(DateTime.Today)
            'If iresult = -1 Then
            '    Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(実績)空車着日", needsPopUp:=True)
            '    Me.TxtActualEmparrDate.Focus()
            '    WW_CheckMES1 = "(実績日)過去日付エラー。"
            '    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    O_RTN = "ERR"
            '    Exit Sub
            'End If
        End If

        '○ 日付妥当性チェック
        '例) iresult = dt1.Date.CompareTo(dt2.Date)
        '    iresultの意味
        '     0 : dt1とdt2は同じ日
        '    -1 : dt1はdt2より前の日
        '     1 : dt1はdt2より後の日
        '(実績)積込日 と　(実績)発日を比較
        If Me.TxtActualLoadingDate.Text <> "" AndAlso Me.TxtActualDepDate.Text <> "" Then
            iresult = Date.Parse(Me.TxtActualLoadingDate.Text).CompareTo(Date.Parse(Me.TxtActualDepDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)積込日 > (実績)発日", needsPopUp:=True)
                Me.TxtActualDepDate.Focus()
                WW_CheckMES1 = "(実績日)入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)発日 と　(実績)積車着日を比較
        If Me.TxtActualDepDate.Text <> "" AndAlso Me.TxtActualArrDate.Text <> "" Then
            iresult = Date.Parse(Me.TxtActualDepDate.Text).CompareTo(Date.Parse(Me.TxtActualArrDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)発日 > (実績)積車着日", needsPopUp:=True)
                Me.TxtActualArrDate.Focus()
                WW_CheckMES1 = "(実績日)入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)積車着日 と　(実績)受入日を比較
        If Me.TxtActualArrDate.Text <> "" AndAlso Me.TxtActualAccDate.Text <> "" Then
            iresult = Date.Parse(Me.TxtActualArrDate.Text).CompareTo(Date.Parse(Me.TxtActualAccDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)積車着日 > (実績)受入日", needsPopUp:=True)
                Me.TxtActualAccDate.Focus()
                WW_CheckMES1 = "(実績日)入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)受入日 と　(実績)空車着日を比較
        If Me.TxtActualAccDate.Text <> "" AndAlso Me.TxtActualEmparrDate.Text <> "" Then
            iresult = Date.Parse(Me.TxtActualAccDate.Text).CompareTo(Date.Parse(Me.TxtActualEmparrDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)受入日 > (実績)空車着日", needsPopUp:=True)
                Me.TxtActualEmparrDate.Focus()
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
            '### 20200626 条件追加(積置フラグ)が未チェックの場合 ########################################################
            If Me.TxtActualLoadingDate.Text <> "" AndAlso OIT0003tab3row("ACTUALLODDATE") <> "" _
                AndAlso OIT0003tab3row("STACKINGFLG") = "" Then
                iresult = Date.Parse(Me.TxtActualLoadingDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALLODDATE")))
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
            If Me.TxtActualDepDate.Text <> "" AndAlso OIT0003tab3row("ACTUALDEPDATE") <> "" Then
                iresult = Date.Parse(Me.TxtActualDepDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALDEPDATE")))
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
            If Me.TxtActualArrDate.Text <> "" AndAlso OIT0003tab3row("ACTUALARRDATE") <> "" Then
                iresult = Date.Parse(Me.TxtActualArrDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALARRDATE")))
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
            If Me.TxtActualAccDate.Text <> "" AndAlso OIT0003tab3row("ACTUALACCDATE") <> "" Then
                iresult = Date.Parse(Me.TxtActualAccDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALACCDATE")))
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
            If Me.TxtActualEmparrDate.Text <> "" AndAlso OIT0003tab3row("ACTUALEMPARRDATE") <> "" Then
                iresult = Date.Parse(Me.TxtActualEmparrDate.Text).CompareTo(Date.Parse(OIT0003tab3row("ACTUALEMPARRDATE")))
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

            '前回黒油
            If WW_GetValue(2) = "1" AndAlso OIT0003row("DELFLG") = "0" Then
                OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_99
                CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                WW_CheckMES1 = "前回油種と油種の整合性エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_LASTOIL_CONSISTENCY_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                O_RTN = "ERR1"
                'Exit Sub

                '前回揮発油
            ElseIf (WW_GetValue(2) = "2" OrElse WW_GetValue(2) = "3") AndAlso OIT0003row("DELFLG") = "0" Then
                OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_98
                CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                WW_CheckMES1 = "前回油種と油種の整合性エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_LASTVOLATILEOIL_BLACKLIGHTOIL_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)

                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    '受注明細TBLの受注情報を更新
                    WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)
                End Using

                If O_RTN <> "ERR1" Then O_RTN = "ERR2"
            Else
                If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_99 _
                    OrElse OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_98 Then
                    OIT0003row("ORDERINFO") = ""
                    OIT0003row("ORDERINFONAME") = ""
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' タンク車状態チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTankStatus(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_OfficeCode As String = ""
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

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

        'タンク車状態チェック
        '(1:発送　2:到着予定　3:到着　4:交検　5:全検　6:修理　7:疎開留置)
        For Each OIT0003row As DataRow In OIT0003tbl.Rows

            If OIT0003row("DELFLG") = "0" AndAlso OIT0003row("TANKSTATUS") = "" Then
                'タンク車情報を取得
                WW_FixvalueMasterSearch("01", "TANKNUMBER", OIT0003row("TANKNO"), WW_GetValue)

                '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
                '使用受注№が設定されている場合
                If WW_GetValue(12) <> "" Then
                    '次のレコードに進む（SKIPする）
                    Continue For
                End If
                '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################

                'タンク車状態
                Select Case WW_GetValue(11)
                        'タンク車状態が"2"(到着予定), "3"(到着)の場合
                    Case "2", "3"
                        If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_101 Then
                            OIT0003row("ORDERINFO") = ""
                            OIT0003row("ORDERINFONAME") = ""
                        End If
                        'タンク車状態が"2"(到着予定), "3"(到着)以外の場合
                    Case Else
                        OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_101
                        CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                        Master.Output(C_MESSAGE_NO.OIL_TANKSTATUS_ERROR,
                              C_MESSAGE_TYPE.ERR,
                              "(" + OIT0003row("TANKNO") + ")" + WW_GetValue(9),
                              needsPopUp:=True)

                        WW_CheckMES1 = "タンク車状態未到着エラー。"
                        WW_CheckMES2 = C_MESSAGE_NO.OIL_TANKSTATUS_ERROR
                        WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                        O_RTN = "ERR"

                        '○ 画面表示データ保存
                        Master.SaveTable(OIT0003tbl)
                        Exit Sub
                End Select
            End If
        Next

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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

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
        WW_FixvalueMasterSearch(WW_OfficeCode, "TRAINNUMBER_FIND", Me.TxtTrainName.Text, WW_GetValue)

        '高速列車対応タンク車チェック
        For Each OIT0003row As DataRow In OIT0003tbl.Rows

            '高速列車区分＝"1"(高速列車)、かつ型式<>"タキ1000"の場合はエラー
            If WW_GetValue(5) = "1" AndAlso OIT0003row("MODEL") <> "タキ1000" AndAlso OIT0003row("DELFLG") = "0" Then
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
                If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_84 Then
                    OIT0003row("ORDERINFO") = ""
                    OIT0003row("ORDERINFONAME") = ""
                End If
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

        '同一列車チェック用
        If IsNothing(OIT0003WK2tbl) Then
            OIT0003WK2tbl = New DataTable
        End If

        If OIT0003WK2tbl.Columns.Count <> 0 Then
            OIT0003WK2tbl.Columns.Clear()
        End If

        OIT0003WK2tbl.Clear()

        '異なる列車チェック用
        If IsNothing(OIT0003WK6tbl) Then
            OIT0003WK6tbl = New DataTable
        End If

        If OIT0003WK6tbl.Columns.Count <> 0 Then
            OIT0003WK6tbl.Columns.Clear()
        End If

        OIT0003WK6tbl.Clear()

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
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')    AS ORDERSTATUS" _
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
            & "   AND OIT0002.DEPDATE         = @P03 " _
            & "   AND OIT0002.ORDERSTATUS    <> @P04 " _
            & "   AND OIT0002.STACKINGFLG     = @P05 " _
            & "   AND OIT0002.DELFLG         <> @P07 "

        '### 20200620 START((全体)No79対応) ######################################
        Dim SQLDiffTrainStr As String =
              SQLStr _
            & "   AND OIT0002.TRAINNO        <> @P02 " _
            & "   AND OIT0002.OFFICECODE      = @P08 "
        '### 20200620 END  ((全体)No79対応) ######################################

        SQLStr &=
              "   AND OIT0002.TRAINNO         = @P02 " _
            & "   AND OIT0002.CONSIGNEECODE   = @P06 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLDiffTraincmd As New SqlCommand(SQLDiffTrainStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)発日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '積置可否フラグ
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 10) '荷受人コード
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA2.Value = Me.TxtTrainNo.Text
                PARA3.Value = Me.TxtDepDate.Text
                PARA4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA5.Value = work.WF_SEL_STACKINGFLG.Text
                PARA6.Value = Me.TxtConsigneeCode.Text
                PARA7.Value = C_DELETE_FLG.DELETE

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

                    '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                    If OIT0003CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                    Master.Output(C_MESSAGE_NO.OIL_ORDER_DEPDATE_SAMETRAIN, C_MESSAGE_TYPE.ERR, OIT0003CHKDrow("ORDERNO"), needsPopUp:=True)

                    WW_CheckMES1 = "受注データ登録済みエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_ORDER_DEPDATE_SAMETRAIN
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Exit Sub
                Next

                '### 20200620 START((全体)No79対応) ######################################
                Dim PARADF1 As SqlParameter = SQLDiffTraincmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARADF2 As SqlParameter = SQLDiffTraincmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARADF3 As SqlParameter = SQLDiffTraincmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)発日
                Dim PARADF4 As SqlParameter = SQLDiffTraincmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARADF5 As SqlParameter = SQLDiffTraincmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '積置可否フラグ
                Dim PARADF7 As SqlParameter = SQLDiffTraincmd.Parameters.Add("@P07", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARADF8 As SqlParameter = SQLDiffTraincmd.Parameters.Add("@P08", SqlDbType.NVarChar, 6)  '受注営業所コード
                PARADF1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARADF2.Value = Me.TxtTrainNo.Text
                PARADF3.Value = Me.TxtDepDate.Text
                PARADF4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARADF5.Value = work.WF_SEL_STACKINGFLG.Text
                PARADF7.Value = C_DELETE_FLG.DELETE
                PARADF8.Value = Me.TxtOrderOfficeCode.Text

                Using SQLdr As SqlDataReader = SQLDiffTraincmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WK6tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WK6tbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                'For Each OIT0003CHKDrow As DataRow In OIT0003WK6tbl.Rows
                '    Master.Output(C_MESSAGE_NO.OIL_ORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, OIT0003CHKDrow("ORDERNO"), needsPopUp:=True)

                '    WW_CheckMES1 = "受注データ登録済みエラー。"
                '    WW_CheckMES2 = C_MESSAGE_NO.OIL_ORDER_REPEAT_ERROR
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                '    Exit Sub
                'Next
                '### 20200620 END  ((全体)No79対応) ######################################

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

        'Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 列車発送順重複チェック(同じ列車(発日も一緒)で発送順がすでに登録済みかチェック)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTrainShipRepeat(ByRef O_RTN As String, ByVal SQLcon As SqlConnection, Optional ByVal dt As DataTable = Nothing)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        If IsNothing(OIT0003WK5tbl) Then
            OIT0003WK5tbl = New DataTable
        End If

        If OIT0003WK5tbl.Columns.Count <> 0 Then
            OIT0003WK5tbl.Columns.Clear()
        End If

        OIT0003WK5tbl.Clear()

        '★データテーブルの指定がない場合は、タンク車割当で使用しているデータテーブルを指定
        If IsNothing(dt) Then
            dt = OIT0003tbl.Copy
        End If

        '○ チェックSQL
        '　説明
        '     登録された内容が受注TBLにすでに登録済みかチェックする

        Dim SQLStr As String =
              " SELECT " _
            & "   ISNULL(RTRIM(OIT0002.ORDERNO), '')         AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')        AS DETAILNO" _
            & " , ISNULL(RTRIM(OIT0003.SHIPORDER), '')       AS SHIPORDER" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')         AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')       AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')       AS LINEORDER" _
            & " , ISNULL(RTRIM(OIT0003.TANKNO), '')          AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0003.OILCODE), '')         AS OILCODE" _
            & " , ISNULL(RTRIM(OIT0003.OILNAME), '')         AS OILNAME" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')    AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '') AS ORDERINGOILNAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')     AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')      AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')      AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')    AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')    AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0002.BASECODE), '')        AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0002.BASENAME), '')        AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')   AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')   AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')      AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')  AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')      AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')  AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.LODDATE), '')         AS LODDATE" _
            & " , ISNULL(RTRIM(OIT0002.DEPDATE), '')         AS DEPDATE" _
            & " , ISNULL(RTRIM(OIT0002.ARRDATE), '')         AS ARRDATE" _
            & " , ISNULL(RTRIM(OIT0002.ACCDATE), '')         AS ACCDATE" _
            & " , ISNULL(RTRIM(OIT0002.EMPARRDATE), '')      AS EMPARRDATE" _
            & " FROM oil.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "       OIT0003.ORDERNO         = OIT0002.ORDERNO " _
            & "   AND OIT0003.SHIPORDER       IN (''"

        '一覧に設定している発送順を条件に設定
        For Each OIT0003row As DataRow In dt.Rows
            SQLStr &= ", '" & OIT0003row("SHIPORDER") & "' "
        Next

        SQLStr &=
              "                                  )" _
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
                        OIT0003WK5tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WK5tbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0003row As DataRow In dt.Rows
                    For Each OIT0003CHKDrow As DataRow In OIT0003WK5tbl.Rows

                        '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                        If OIT0003CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                        If OIT0003CHKDrow("SHIPORDER") = OIT0003row("SHIPORDER") Then
                            OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_100
                            CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "発送順(同一の列車番号)重複。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_SHIPORDER_REPEAT_ERROR
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR"

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            Exit For
                        Else
                            If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_100 Then
                                OIT0003row("ORDERINFO") = ""
                                OIT0003row("ORDERINFONAME") = ""
                            End If
                        End If
                    Next
                Next
                If O_RTN = "ERR" Then Exit Sub

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D CHECK_TRAINSHIPREPEAT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D CHECK_TRAINSHIPREPEAT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 列車タンク車重複チェック(同一(異なる)列車(発日(積込日)も一緒)でタンク車がすでに登録済みかチェック)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTrainTankRepeat(ByRef O_RTN As String, ByVal SQLcon As SqlConnection)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '同一列車チェック用
        If IsNothing(OIT0003WK3tbl) Then
            OIT0003WK3tbl = New DataTable
        End If

        If OIT0003WK3tbl.Columns.Count <> 0 Then
            OIT0003WK3tbl.Columns.Clear()
        End If

        OIT0003WK3tbl.Clear()

        '異なる列車チェック用(同一発日)
        If IsNothing(OIT0003WK7tbl) Then
            OIT0003WK7tbl = New DataTable
        End If

        If OIT0003WK7tbl.Columns.Count <> 0 Then
            OIT0003WK7tbl.Columns.Clear()
        End If

        OIT0003WK7tbl.Clear()

        '異なる列車チェック用(同一積込日)
        If IsNothing(OIT0003WK8tbl) Then
            OIT0003WK8tbl = New DataTable
        End If

        If OIT0003WK8tbl.Columns.Count <> 0 Then
            OIT0003WK8tbl.Columns.Clear()
        End If

        OIT0003WK8tbl.Clear()

        '同一列車チェック用(同一積込日)
        If IsNothing(OIT0003WK10tbl) Then
            OIT0003WK10tbl = New DataTable
        End If

        If OIT0003WK10tbl.Columns.Count <> 0 Then
            OIT0003WK10tbl.Columns.Clear()
        End If

        OIT0003WK10tbl.Clear()

        '○ チェックSQL
        '　説明
        '     登録された内容が受注TBLにすでに登録済みかチェックする

        Dim SQLStr As String =
              " SELECT " _
            & "   ISNULL(RTRIM(OIT0002.ORDERNO), '')         AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')        AS DETAILNO" _
            & " , ISNULL(RTRIM(OIT0003.SHIPORDER), '')       AS SHIPORDER" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')         AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')       AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')       AS LINEORDER" _
            & " , ISNULL(RTRIM(OIT0003.TANKNO), '')          AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0003.OILCODE), '')         AS OILCODE" _
            & " , ISNULL(RTRIM(OIT0003.OILNAME), '')         AS OILNAME" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')    AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '') AS ORDERINGOILNAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')     AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')      AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')      AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')    AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')    AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0002.BASECODE), '')        AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0002.BASENAME), '')        AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')   AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')   AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')      AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')  AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')      AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')  AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.LODDATE), '')         AS LODDATE" _
            & " , ISNULL(RTRIM(OIT0002.DEPDATE), '')         AS DEPDATE" _
            & " , ISNULL(RTRIM(OIT0002.ARRDATE), '')         AS ARRDATE" _
            & " , ISNULL(RTRIM(OIT0002.ACCDATE), '')         AS ACCDATE" _
            & " , ISNULL(RTRIM(OIT0002.EMPARRDATE), '')      AS EMPARRDATE" _
            & " FROM oil.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "       OIT0003.ORDERNO         = OIT0002.ORDERNO " _
            & "   AND OIT0003.TANKNO          IN (''"

        '一覧に設定しているタンク車を条件に設定
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            SQLStr &= ", '" & OIT0003row("TANKNO") & "' "
        Next

        '### 20200620 START((全体)No79対応)異なる列車で同一積込日の場合###########
        '### 20200827 START 積置を考慮した妥当性チェック対応 ######################
        'Dim SQLDiffLODTrainStr As String =
        '      SQLStr _
        '    & "                                  )" _
        '    & " WHERE OIT0002.USEPROPRIETYFLG = '1' " _
        '    & "   AND OIT0002.ORDERNO        <> @P01 " _
        '    & "   AND OIT0002.OFFICECODE      = @P06 " _
        '    & "   AND OIT0002.LODDATE         = @P03 " _
        '    & "   AND OIT0002.ORDERSTATUS    <> @P04 " _
        '    & "   AND OIT0002.DELFLG         <> @P05 " _
        '    & "   AND OIT0002.TRAINNO        <> @P02 "

        '★積置チェックパターン
        '　１．　①積込日　＝　②積込日(明細)　ＯＲ　①積込日　＝　②積込日
        '　２．（①積込日　＞　②積込日　　　　ＯＲ　①積込日　＞　②積込日(明細)）　ＡＮＤ　①発日　＜　②発日
        '　３．　①積込日　＞　②積込日(明細)　ＡＮＤ　①発日　＜　②発日
        '　４．（①積込日　＞　②積込日　　　　ＯＲ　①積込日　＞　②積込日(明細)）　ＡＮＤ　①発日　＞　②発日
        '　５．　①積込日　＞　②積込日(明細)　ＡＮＤ　①発日　＞　②発日
        '　６．（①積込日　＜　②積込日　　　　ＯＲ　①積込日　＜　②積込日(明細)）　ＡＮＤ　①発日　＞　②発日
        '　７．　①積込日　＜　②積込日(明細)　ＡＮＤ　①発日　＞　②発日
        Dim SQLDiffLODTrainStr As String =
              SQLStr _
            & "                                  )" _
            & " WHERE OIT0002.USEPROPRIETYFLG = '1' " _
            & "   AND OIT0002.ORDERNO        <> @P01 " _
            & "   AND OIT0002.OFFICECODE      = @P06 " _
            & "   AND ( " _
            & "          (OIT0002.LODDATE = @P03 OR OIT0002.LODDATE = @P08) " _
            & "       OR (OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE = @P08) " _
            & "       OR ((OIT0002.LODDATE > @P03 OR OIT0002.LODDATE > @P08) AND OIT0002.DEPDATE < @P07) " _
            & "       OR ((OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE > @P08) AND OIT0002.DEPDATE < @P07) " _
            & "       OR ((OIT0002.LODDATE > @P03 OR OIT0002.LODDATE > @P08) AND OIT0002.DEPDATE > @P07) " _
            & "       OR ((OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE > @P08) AND OIT0002.DEPDATE > @P07) " _
            & "       OR ((OIT0002.LODDATE < @P03 OR OIT0002.LODDATE < @P08) AND OIT0002.DEPDATE > @P07) " _
            & "       OR ((OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE < @P08) AND OIT0002.DEPDATE > @P07) " _
            & "       ) " _
            & "   AND OIT0002.ORDERSTATUS    <> @P04 " _
            & "   AND OIT0002.DELFLG         <> @P05 " _
            & "   AND OIT0002.TRAINNO        <> @P02 "
        '& "          (OIT0002.LODDATE = @P03 OR (OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE = @P08)) " _
        '### 20200827 END   積置を考慮した妥当性チェック対応 ######################
        '### 20200620 END  ((全体)No79対応)異なる列車で同一積込日の場合###########

        '### 20200805 START((全体)No117対応)異なる列車で同一積込日の場合###########
        '### 20200827 START 積置を考慮した妥当性チェック対応 ######################
        'Dim SQLSameLODTrainStr As String =
        '      SQLStr _
        '    & "                                  )" _
        '    & " WHERE OIT0002.USEPROPRIETYFLG = '1' " _
        '    & "   AND OIT0002.ORDERNO        <> @P01 " _
        '    & "   AND OIT0002.OFFICECODE      = @P06 " _
        '    & "   AND OIT0002.LODDATE         = @P03 " _
        '    & "   AND OIT0002.ORDERSTATUS    <> @P04 " _
        '    & "   AND OIT0002.DELFLG         <> @P05 " _
        '    & "   AND OIT0002.TRAINNO         = @P02 "

        '★積置チェックパターン
        '　上記と同様
        Dim SQLSameLODTrainStr As String =
              SQLStr _
            & "                                  )" _
            & " WHERE OIT0002.USEPROPRIETYFLG = '1' " _
            & "   AND OIT0002.ORDERNO        <> @P01 " _
            & "   AND OIT0002.OFFICECODE      = @P06 " _
            & "   AND ( " _
            & "          (OIT0002.LODDATE = @P03 OR OIT0002.LODDATE = @P08) " _
            & "       OR (OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE = @P08) " _
            & "       OR ((OIT0002.LODDATE > @P03 OR OIT0002.LODDATE > @P08) AND OIT0002.DEPDATE < @P07) " _
            & "       OR ((OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE > @P08) AND OIT0002.DEPDATE < @P07) " _
            & "       OR ((OIT0002.LODDATE > @P03 OR OIT0002.LODDATE > @P08) AND OIT0002.DEPDATE > @P07) " _
            & "       OR ((OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE > @P08) AND OIT0002.DEPDATE > @P07) " _
            & "       OR ((OIT0002.LODDATE < @P03 OR OIT0002.LODDATE < @P08) AND OIT0002.DEPDATE > @P07) " _
            & "       OR ((OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE < @P08) AND OIT0002.DEPDATE > @P07) " _
            & "       ) " _
            & "   AND OIT0002.ORDERSTATUS    <> @P04 " _
            & "   AND OIT0002.DELFLG         <> @P05 " _
            & "   AND OIT0002.TRAINNO         = @P02 "
        '& "          (OIT0002.LODDATE = @P03 OR (OIT0003.STACKINGFLG = '1' AND OIT0003.ACTUALLODDATE = @P08)) " _
        '### 20200827 END   積置を考慮した妥当性チェック対応 ######################
        '### 20200805 END  ((全体)No117対応)異なる列車で同一積込日の場合###########

        SQLStr &=
              "                                  )" _
            & " WHERE OIT0002.USEPROPRIETYFLG = '1' " _
            & "   AND OIT0002.ORDERNO        <> @P01 " _
            & "   AND OIT0002.OFFICECODE      = @P06 " _
            & "   AND OIT0002.DEPDATE         = @P03 " _
            & "   AND OIT0002.ORDERSTATUS    <> @P04 " _
            & "   AND OIT0002.DELFLG         <> @P05 "

        '### 20200620 START((全体)No79対応)異なる列車で同一発日の場合#############
        Dim SQLDiffDEPTrainStr As String =
              SQLStr _
            & "   AND OIT0002.TRAINNO        <> @P02 "
        '### 20200620 END  ((全体)No79対応)異なる列車で同一発日の場合#############

        SQLStr &=
              "   AND OIT0002.TRAINNO         = @P02 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon),
                  SQLDiffDEPTraincmd As New SqlCommand(SQLDiffDEPTrainStr, SQLcon),
                  SQLDiffLODTraincmd As New SqlCommand(SQLDiffLODTrainStr, SQLcon),
                  SQLSameLODTraincmd As New SqlCommand(SQLSameLODTrainStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)発日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 6)  '受注営業所
                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA2.Value = Me.TxtTrainNo.Text
                PARA3.Value = Me.TxtDepDate.Text
                PARA4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA5.Value = C_DELETE_FLG.DELETE
                PARA6.Value = Me.TxtOrderOfficeCode.Text
                'PARA6.Value = work.WF_SEL_SALESOFFICECODE.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    If OIT0003WK3tbl.Columns.Count = 0 Then
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIT0003WK3tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    Else
                        OIT0003WK3tbl.Clear()
                    End If

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WK3tbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    '★行削除したデータはSKIPする。
                    If OIT0003row("DELFLG") = "1" Then Continue For

                    '★受注情報を初期化(タンク車重複の場合のみ)
                    If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                        OIT0003row("ORDERINFO") = ""
                        OIT0003row("ORDERINFONAME") = ""

                        '受注明細TBLの受注情報を更新
                        WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                    End If

                    For Each OIT0003CHKDrow As DataRow In OIT0003WK3tbl.Rows

                        '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                        If OIT0003CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                        '### 20200903 START 受注進行ステータスが500：検収中以降はチェック不要とする ############
                        If OIT0003CHKDrow("ORDERSTATUS") >= BaseDllConst.CONST_ORDERSTATUS_500 Then
                            OIT0003row("ORDERINFO") = ""
                            OIT0003row("ORDERINFONAME") = ""

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            Continue For
                        End If
                        '### 20200903 END   受注進行ステータスが500：検収中以降はチェック不要とする ############

                        If OIT0003CHKDrow("TANKNO") = OIT0003row("TANKNO") Then
                            OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                            CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "タンク車№(同一の列車番号)重複。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR1"

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            '★タンク車所在の更新
                            '引数１：所在地コード　⇒　変更あり(発駅)
                            '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
                            '引数３：積車区分　　　⇒　変更あり("E"(空車))
                            '引数４：タンク車状況　⇒　変更あり("1"(残車))
                            '※１つでも重複があった場合は、すべてのタンク車の状態を元に戻す
                            'WW_UpdateTankShozai("", "2", "E", I_SITUATION:="1", upActualEmparrDate:=True)
                            ''WW_UpdateTankShozai("", "2", "E", I_TANKNO:=OIT0003row("TANKNO"), I_SITUATION:="1", upActualEmparrDate:=True)

                            Exit For
                            'Else
                            '    If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                            '        OIT0003row("ORDERINFO") = ""
                            '        OIT0003row("ORDERINFONAME") = ""
                            '    End If
                        End If
                    Next
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl)

                If O_RTN = "ERR1" Then Exit Sub

                '### 20200620 START((全体)No79対応)異なる列車で同一発日の場合#############
                Dim PARADF1 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARADF2 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARADF3 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)発日
                Dim PARADF4 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARADF5 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARADF6 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P06", SqlDbType.NVarChar, 6)  '受注営業所
                PARADF1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARADF2.Value = Me.TxtTrainNo.Text
                PARADF3.Value = Me.TxtDepDate.Text
                PARADF4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARADF5.Value = C_DELETE_FLG.DELETE
                PARADF6.Value = Me.TxtOrderOfficeCode.Text
                'PARADF6.Value = work.WF_SEL_SALESOFFICECODE.Text

                Using SQLdr As SqlDataReader = SQLDiffDEPTraincmd.ExecuteReader()

                    If OIT0003WK7tbl.Columns.Count = 0 Then
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIT0003WK7tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    Else
                        OIT0003WK7tbl.Clear()
                    End If

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WK7tbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    '★行削除したデータはSKIPする。
                    If OIT0003row("DELFLG") = "1" Then Continue For

                    '★受注情報を初期化(タンク車重複の場合のみ)
                    If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                        OIT0003row("ORDERINFO") = ""
                        OIT0003row("ORDERINFONAME") = ""

                        '受注明細TBLの受注情報を更新
                        WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                    End If

                    For Each OIT0003CHKDrow As DataRow In OIT0003WK7tbl.Rows

                        '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                        If OIT0003CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                        '### 20200903 START 受注進行ステータスが500：検収中以降はチェック不要とする ############
                        If OIT0003CHKDrow("ORDERSTATUS") >= BaseDllConst.CONST_ORDERSTATUS_500 Then
                            OIT0003row("ORDERINFO") = ""
                            OIT0003row("ORDERINFONAME") = ""

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            Continue For
                        End If
                        '### 20200903 END   受注進行ステータスが500：検収中以降はチェック不要とする ############

                        If OIT0003CHKDrow("TANKNO") = OIT0003row("TANKNO") Then
                            OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                            CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "タンク車№(異なる列車番号)重複。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR2"

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            '★タンク車所在の更新
                            '引数１：所在地コード　⇒　変更あり(発駅)
                            '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
                            '引数３：積車区分　　　⇒　変更あり("E"(空車))
                            '引数４：タンク車状況　⇒　変更あり("1"(残車))
                            '※１つでも重複があった場合は、すべてのタンク車の状態を元に戻す
                            'WW_UpdateTankShozai("", "2", "E", I_SITUATION:="1", upActualEmparrDate:=True)
                            ''WW_UpdateTankShozai("", "2", "E", I_TANKNO:=OIT0003row("TANKNO"), I_SITUATION:="1", upActualEmparrDate:=True)

                            Exit For
                            'Else
                            '    If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                            '        OIT0003row("ORDERINFO") = ""
                            '        OIT0003row("ORDERINFONAME") = ""
                            '    End If
                        End If
                    Next
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl)

                If O_RTN = "ERR2" Then Exit Sub
                '### 20200620 END  ((全体)No79対応)異なる列車で同一発日の場合#############

                '### 20200620 START((全体)No79対応)異なる列車で同一積込日の場合###########
                Dim PARALDF1 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARALDF2 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARALDF3 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)積込日
                Dim PARALDF4 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARALDF5 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARALDF6 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P06", SqlDbType.NVarChar, 6)  '受注営業所
                Dim PARALDF7 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P07", SqlDbType.Date)         '(予定)発日
                Dim PARALDF8 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P08", SqlDbType.Date)         '(実績)積込日(明細)
                PARALDF1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARALDF2.Value = Me.TxtTrainNo.Text
                PARALDF3.Value = Me.TxtLoadingDate.Text
                PARALDF4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARALDF5.Value = C_DELETE_FLG.DELETE
                PARALDF6.Value = Me.TxtOrderOfficeCode.Text
                PARALDF7.Value = Me.TxtDepDate.Text

                'Using SQLdr As SqlDataReader = SQLDiffLODTraincmd.ExecuteReader()
                '    '○ フィールド名とフィールドの型を取得
                '    For index As Integer = 0 To SQLdr.FieldCount - 1
                '        OIT0003WK8tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                '    Next

                '    '○ テーブル検索結果をテーブル格納
                '    OIT0003WK8tbl.Load(SQLdr)
                'End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0003row As DataRow In OIT0003tbl.Rows

                    '★行削除したデータはSKIPする。
                    If OIT0003row("DELFLG") = "1" Then Continue For

                    If OIT0003row("ACTUALLODDATE") <> "" Then
                        PARALDF8.Value = OIT0003row("ACTUALLODDATE")
                    Else
                        PARALDF8.Value = DBNull.Value
                    End If

                    Using SQLdr As SqlDataReader = SQLDiffLODTraincmd.ExecuteReader()

                        If OIT0003WK8tbl.Columns.Count = 0 Then
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0003WK8tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        Else
                            OIT0003WK8tbl.Clear()
                        End If

                        '○ テーブル検索結果をテーブル格納
                        OIT0003WK8tbl.Load(SQLdr)

                    End Using

                    '★受注情報を初期化(タンク車重複の場合のみ)
                    If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                        OIT0003row("ORDERINFO") = ""
                        OIT0003row("ORDERINFONAME") = ""

                        '受注明細TBLの受注情報を更新
                        WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                    End If

                    For Each OIT0003CHKDrow As DataRow In OIT0003WK8tbl.Rows

                        '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                        If OIT0003CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                        '### 20200903 START 受注進行ステータスが500：検収中以降はチェック不要とする ############
                        If OIT0003CHKDrow("ORDERSTATUS") >= BaseDllConst.CONST_ORDERSTATUS_500 Then
                            OIT0003row("ORDERINFO") = ""
                            OIT0003row("ORDERINFONAME") = ""

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            Continue For
                        End If
                        '### 20200903 END   受注進行ステータスが500：検収中以降はチェック不要とする ############

                        If OIT0003CHKDrow("TANKNO") = OIT0003row("TANKNO") Then
                            OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                            CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "タンク車№(異なる列車番号)重複。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR3"

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            '★タンク車所在の更新
                            '引数１：所在地コード　⇒　変更あり(発駅)
                            '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
                            '引数３：積車区分　　　⇒　変更あり("E"(空車))
                            '引数４：タンク車状況　⇒　変更あり("1"(残車))
                            '※１つでも重複があった場合は、すべてのタンク車の状態を元に戻す
                            'WW_UpdateTankShozai("", "2", "E", I_SITUATION:="1", upActualEmparrDate:=True)
                            ''WW_UpdateTankShozai("", "2", "E", I_TANKNO:=OIT0003row("TANKNO"), I_SITUATION:="1", upActualEmparrDate:=True)

                            Exit For
                            'Else
                            '    If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                            '        OIT0003row("ORDERINFO") = ""
                            '        OIT0003row("ORDERINFONAME") = ""
                            '    End If
                        End If
                    Next
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl)

                If O_RTN = "ERR3" Then Exit Sub
                '### 20200620 END  ((全体)No79対応)異なる列車で同一積込日の場合###########

                '### 20200805 START((全体)No117対応)異なる列車で同一積込日の場合###########
                Dim PARALSM1 As SqlParameter = SQLSameLODTraincmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARALSM2 As SqlParameter = SQLSameLODTraincmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARALSM3 As SqlParameter = SQLSameLODTraincmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)積込日
                Dim PARALSM4 As SqlParameter = SQLSameLODTraincmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARALSM5 As SqlParameter = SQLSameLODTraincmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARALSM6 As SqlParameter = SQLSameLODTraincmd.Parameters.Add("@P06", SqlDbType.NVarChar, 6)  '受注営業所
                Dim PARALSM7 As SqlParameter = SQLSameLODTraincmd.Parameters.Add("@P07", SqlDbType.Date)         '(予定)発日
                Dim PARALSM8 As SqlParameter = SQLSameLODTraincmd.Parameters.Add("@P08", SqlDbType.Date)         '(実績)積込日(明細)
                PARALSM1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARALSM2.Value = Me.TxtTrainNo.Text
                PARALSM3.Value = Me.TxtLoadingDate.Text
                PARALSM4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARALSM5.Value = C_DELETE_FLG.DELETE
                PARALSM6.Value = Me.TxtOrderOfficeCode.Text
                PARALSM7.Value = Me.TxtDepDate.Text

                'Using SQLdr As SqlDataReader = SQLSameLODTraincmd.ExecuteReader()
                '    '○ フィールド名とフィールドの型を取得
                '    For index As Integer = 0 To SQLdr.FieldCount - 1
                '        OIT0003WK10tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                '    Next

                '    '○ テーブル検索結果をテーブル格納
                '    OIT0003WK10tbl.Load(SQLdr)
                'End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0003row As DataRow In OIT0003tbl.Rows

                    '★行削除したデータはSKIPする。
                    If OIT0003row("DELFLG") = "1" Then Continue For

                    If OIT0003row("ACTUALLODDATE") <> "" Then
                        PARALSM8.Value = OIT0003row("ACTUALLODDATE")
                    Else
                        PARALSM8.Value = DBNull.Value
                    End If

                    Using SQLdr As SqlDataReader = SQLSameLODTraincmd.ExecuteReader()

                        If OIT0003WK10tbl.Columns.Count = 0 Then
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0003WK10tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        Else
                            OIT0003WK10tbl.Clear()
                        End If

                        '○ テーブル検索結果をテーブル格納
                        OIT0003WK10tbl.Load(SQLdr)

                    End Using

                    '★受注情報を初期化(タンク車重複の場合のみ)
                    If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                        OIT0003row("ORDERINFO") = ""
                        OIT0003row("ORDERINFONAME") = ""

                        '受注明細TBLの受注情報を更新
                        WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                    End If

                    For Each OIT0003CHKDrow As DataRow In OIT0003WK10tbl.Rows

                        '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                        If OIT0003CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                        '### 20200903 START 受注進行ステータスが500：検収中以降はチェック不要とする ############
                        If OIT0003CHKDrow("ORDERSTATUS") >= BaseDllConst.CONST_ORDERSTATUS_500 Then
                            OIT0003row("ORDERINFO") = ""
                            OIT0003row("ORDERINFONAME") = ""

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            Continue For
                        End If
                        '### 20200903 END   受注進行ステータスが500：検収中以降はチェック不要とする ############

                        If OIT0003CHKDrow("TANKNO") = OIT0003row("TANKNO") Then
                            OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                            CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "タンク車№(同一の列車番号)重複。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR4"

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            '★タンク車所在の更新
                            '引数１：所在地コード　⇒　変更あり(発駅)
                            '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
                            '引数３：積車区分　　　⇒　変更あり("E"(空車))
                            '引数４：タンク車状況　⇒　変更あり("1"(残車))
                            '※１つでも重複があった場合は、すべてのタンク車の状態を元に戻す
                            'WW_UpdateTankShozai("", "2", "E", I_SITUATION:="1", upActualEmparrDate:=True)
                            ''WW_UpdateTankShozai("", "2", "E", I_TANKNO:=OIT0003row("TANKNO"), I_SITUATION:="1", upActualEmparrDate:=True)

                            Exit For
                            'Else
                            '    If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                            '        OIT0003row("ORDERINFO") = ""
                            '        OIT0003row("ORDERINFONAME") = ""
                            '    End If
                        End If
                    Next
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl)

                If O_RTN = "ERR4" Then Exit Sub

                '### 20200805 END  ((全体)No117対応)異なる列車で同一積込日の場合###########

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D CHECK_TRAINTANKREPEAT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D CHECK_TRAINTANKREPEAT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 列車入線順重複チェック(同じ列車(発日も一緒)で入線順がすでに登録済みかチェック)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTrainLineRepeat(ByRef O_RTN As String, ByVal SQLcon As SqlConnection)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        If IsNothing(OIT0003WK4tbl) Then
            OIT0003WK4tbl = New DataTable
        End If

        If OIT0003WK4tbl.Columns.Count <> 0 Then
            OIT0003WK4tbl.Columns.Clear()
        End If

        OIT0003WK4tbl.Clear()

        '○ チェックSQL
        '　説明
        '     登録された内容が受注TBLにすでに登録済みかチェックする

        Dim SQLStr As String =
              " SELECT " _
            & "   ISNULL(RTRIM(OIT0002.ORDERNO), '')         AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')        AS DETAILNO" _
            & " , ISNULL(RTRIM(OIT0003.SHIPORDER), '')       AS SHIPORDER" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')         AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')       AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')       AS LINEORDER" _
            & " , ISNULL(RTRIM(OIT0003.TANKNO), '')          AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0003.OILCODE), '')         AS OILCODE" _
            & " , ISNULL(RTRIM(OIT0003.OILNAME), '')         AS OILNAME" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')    AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '') AS ORDERINGOILNAME" _
            & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')      AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')      AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')    AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')    AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0002.BASECODE), '')        AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0002.BASENAME), '')        AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')   AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')   AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')      AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')  AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')      AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')  AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.LODDATE), '')         AS LODDATE" _
            & " , ISNULL(RTRIM(OIT0002.DEPDATE), '')         AS DEPDATE" _
            & " , ISNULL(RTRIM(OIT0002.ARRDATE), '')         AS ARRDATE" _
            & " , ISNULL(RTRIM(OIT0002.ACCDATE), '')         AS ACCDATE" _
            & " , ISNULL(RTRIM(OIT0002.EMPARRDATE), '')      AS EMPARRDATE" _
            & " FROM oil.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "       OIT0003.ORDERNO         = OIT0002.ORDERNO " _
            & "   AND OIT0003.LINEORDER       IN (''"

        '一覧に設定しているタンク車を条件に設定
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            SQLStr &= ", '" & OIT0003row("LINEORDER") & "' "
        Next

        SQLStr &=
              "                                  )" _
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
                        OIT0003WK4tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WK4tbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    For Each OIT0003CHKDrow As DataRow In OIT0003WK4tbl.Rows
                        If OIT0003CHKDrow("LINEORDER") = OIT0003row("LINEORDER") Then
                            OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_97
                            CODENAME_get("ORDERINFO", OIT0003row("ORDERINFO"), OIT0003row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "入線順(同一の列車番号)重複。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR"

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                            Exit For
                        Else
                            If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_97 Then
                                OIT0003row("ORDERINFO") = ""
                                OIT0003row("ORDERINFONAME") = ""
                            End If
                        End If
                    Next
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0003tbl)

                If O_RTN = "ERR" Then Exit Sub

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D CHECK_TRAINLINEREPEAT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D CHECK_TRAINLINEREPEAT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

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
        '### 20200701 指摘票対応(全体(No98)) 積込日、発日も条件に追加 #############
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
            & "    AND VIW0006_BASE.LODDATE = VIW0006_JR3.LODDATE" _
            & "    AND VIW0006_BASE.DEPDATE = VIW0006_JR3.DEPDATE" _
            & " GROUP BY " _
            & "   VIW0006_JR3.JRTRAINNO3" _
            & " , VIW0006_JR3.MAXTANK3"

        'JR中継列車番号チェック用
        '### 20200701 指摘票対応(全体(No98)) 積込日、発日も条件に追加 #############
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
            & "    AND VIW0006_BASE.LODDATE = VIW0006_JR2.LODDATE" _
            & "    AND VIW0006_BASE.DEPDATE = VIW0006_JR2.DEPDATE" _
            & " GROUP BY " _
            & "   VIW0006_JR2.JRTRAINNO2" _
            & " , VIW0006_JR2.MAXTANK2"

        'JR発列車番号チェック用
        '### 20200701 指摘票対応(全体(No98)) 積込日、発日も条件に追加 #############
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
            & "    AND VIW0006_BASE.LODDATE = VIW0006_JR1.LODDATE" _
            & "    AND VIW0006_BASE.DEPDATE = VIW0006_JR1.DEPDATE" _
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

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0003row)

                        Else
                            If OIT0003row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_90 Then
                                OIT0003row("ORDERINFO") = ""
                                OIT0003row("ORDERINFONAME") = ""
                            End If
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

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

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
            WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
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
    ''' 他の受注で同日の積込日を設定しているタンク車がないかチェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckSameLodDayOtherOrder(ByVal SQLcon As SqlConnection, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        '異なる列車チェック用(同一積込日)
        If IsNothing(OIT0003WK8tbl) Then
            OIT0003WK8tbl = New DataTable
        End If

        If OIT0003WK8tbl.Columns.Count <> 0 Then
            OIT0003WK8tbl.Columns.Clear()
        End If

        OIT0003WK8tbl.Clear()

        '○ チェックSQL
        '　説明
        '     登録された内容が受注TBLにすでに登録済みかチェックする

        Dim SQLStr As String =
              " SELECT " _
            & "   ISNULL(RTRIM(OIT0002.ORDERNO), '')         AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')        AS DETAILNO" _
            & " , ISNULL(RTRIM(OIT0003.SHIPORDER), '')       AS SHIPORDER" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')         AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')       AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0003.LINEORDER), '')       AS LINEORDER" _
            & " , ISNULL(RTRIM(OIT0003.TANKNO), '')          AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0003.OILCODE), '')         AS OILCODE" _
            & " , ISNULL(RTRIM(OIT0003.OILNAME), '')         AS OILNAME" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')    AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '') AS ORDERINGOILNAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')     AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')      AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')      AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')    AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')    AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0002.BASECODE), '')        AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0002.BASENAME), '')        AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')   AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')   AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')      AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')  AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')      AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')  AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.LODDATE), '')         AS LODDATE" _
            & " , ISNULL(RTRIM(OIT0002.DEPDATE), '')         AS DEPDATE" _
            & " , ISNULL(RTRIM(OIT0002.ARRDATE), '')         AS ARRDATE" _
            & " , ISNULL(RTRIM(OIT0002.ACCDATE), '')         AS ACCDATE" _
            & " , ISNULL(RTRIM(OIT0002.EMPARRDATE), '')      AS EMPARRDATE" _
            & " FROM ( "

        '①現受注オーダー情報取得
        SQLStr &=
              "     SELECT " _
            & "       OIT0002.ORDERNO " _
            & "     , OIT0003.DETAILNO " _
            & "     , OIT0002.ORDERSTATUS " _
            & "     , OIT0002.OFFICECODE " _
            & "     , OIT0002.OFFICENAME " _
            & "     , OIT0002.SHIPPERSCODE " _
            & "     , OIT0002.SHIPPERSNAME " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.BASENAME " _
            & "     , OIT0002.CONSIGNEECODE " _
            & "     , OIT0002.CONSIGNEENAME " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.TANKNO " _
            & "     , OIT0002.LODDATE " _
            & "     , OIT0002.DEPDATE " _
            & "     , OIT0002.ARRDATE " _
            & "     , OIT0002.ACCDATE " _
            & "     , OIT0002.EMPARRDATE " _
            & "     , OIT0003.ACTUALLODDATE " _
            & "     , OIT0003.ACTUALDEPDATE " _
            & "     , OIT0003.ACTUALARRDATE " _
            & "     , OIT0003.ACTUALACCDATE " _
            & "     , OIT0003.ACTUALEMPARRDATE " _
            & "     FROM oil.OIT0002_ORDER OIT0002 " _
            & "     INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "           OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "       AND OIT0003.TANKNO <> '' " _
            & "       AND OIT0003.ACTUALLODDATE <> '' " _
            & "       AND OIT0003.DELFLG <> @P02 " _
            & "     WHERE OIT0002.ORDERNO = @P01 " _
            & " ) PRESENTORDER "

        '①の情報の受注オーダー以外で下記の内容が同一のデータ
        '　・受注営業所が同一
        '　・積込日が同一((予定)積置日と①.(実績)積置日)
        '　・タンク車Noが同一
        SQLStr &=
              " INNER JOIN oil.OIT0002_ORDER OIT0002 ON " _
            & "       OIT0002.ORDERNO <> PRESENTORDER.ORDERNO " _
            & "   AND OIT0002.OFFICECODE = PRESENTORDER.OFFICECODE" _
            & "   AND OIT0002.LODDATE = PRESENTORDER.ACTUALLODDATE" _
            & "   AND OIT0002.DELFLG <> @P02" _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "       OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "   AND OIT0003.TANKNO = PRESENTORDER.TANKNO" _
            & "   AND OIT0003.DELFLG <> @P02"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003WK8tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WK8tbl.Load(SQLdr)
                End Using

            End Using

            For Each OIT0003CHKrow As DataRow In OIT0003WK8tbl.Rows
                For Each OIT0003tab3row As DataRow In OIT0003tbl_tab3.Rows
                    If OIT0003tab3row("TANKNO") = OIT0003CHKrow("TANKNO") Then
                        OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                        CODENAME_get("ORDERINFO", OIT0003tab3row("ORDERINFO"), OIT0003tab3row("ORDERINFONAME"), WW_DUMMY)
                        O_RTN = "ERR"
                    Else
                        If OIT0003tab3row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                            OIT0003tab3row("ORDERINFO") = ""
                            OIT0003tab3row("ORDERINFONAME") = ""
                        End If
                    End If
                Next
            Next

            Master.SaveTable(OIT0003tbl_tab3, work.WF_SEL_INPTAB3TBL.Text)

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D CheckSameLodDayOtherOrder")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D CheckSameLodDayOtherOrder"
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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

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

        '積置列車
        Me.TxtOTTrainNo.Text = WW_GetValue(14)
        Me.TxtOTTrainName.Text = ""

        '積置可否フラグ
        '(積置列車:T, 非積置列車：N)
        If WW_GetValue(12) = "T" Then
            '"1"(積置あり)を設定
            work.WF_SEL_STACKINGFLG.Text = "1"
            chkOrderInfo.Checked = True
        ElseIf WW_GetValue(12) = "N" Then
            '"2"(積置なし)を設定
            work.WF_SEL_STACKINGFLG.Text = "2"
            chkOrderInfo.Checked = False
        Else
            work.WF_SEL_STACKINGFLG.Text = "2"
            chkOrderInfo.Checked = False
        End If

        '発送順区分
        work.WF_SEL_SHIPORDERCLASS.Text = WW_GetValue(13)

        '発駅
        Me.TxtDepstationCode.Text = WW_GetValue(1)
        work.WF_SEL_DEPARTURESTATION.Text = Me.TxtDepstationCode.Text
        CODENAME_get("DEPSTATION", Me.TxtDepstationCode.Text, Me.LblDepstationName.Text, WW_DUMMY)
        '着駅
        Me.TxtArrstationCode.Text = WW_GetValue(2)
        work.WF_SEL_ARRIVALSTATION.Text = Me.TxtArrstationCode.Text
        CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_DUMMY)
        TxtTrainNo.Focus()

        '〇 (予定)の日付を設定
        Me.TxtLoadingDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
        Me.TxtDepDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(6))).ToString("yyyy/MM/dd")
        Me.TxtArrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
        Me.TxtAccDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
        Me.TxtEmparrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(10)) + Integer.Parse(WW_GetValue(11))).ToString("yyyy/MM/dd")

        '〇 積置フラグ(積置列車:T, 非積置列車：N)
        If WW_GetValue(12) = "T" Then
            chkOrderInfo.Checked = True
            work.WF_SEL_STACKINGFLG.Text = "1"
        Else
            chkOrderInfo.Checked = False
            work.WF_SEL_STACKINGFLG.Text = "2"
        End If

        '〇営業所配下情報を取得・設定
        WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '〇 検索(営業所).テキストボックスが未設定
        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
            '〇 画面(受注営業所).テキストボックスが未設定
            If Me.TxtOrderOffice.Text = "" Then
                WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
            Else
                WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
            End If
        Else
            WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
        End If

        '荷主
        Me.TxtShippersCode.Text = WW_GetValue(0)
        Me.LblShippersName.Text = WW_GetValue(1)
        '荷受人
        Me.TxtConsigneeCode.Text = WW_GetValue(4)
        Me.LblConsigneeName.Text = WW_GetValue(5)
        '受注パターン
        Me.TxtOrderType.Text = WW_GetValue(7)
        '輸送形態区分
        Me.TxtOrderTrkKbn.Text = WW_GetValue(8)

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

        WW_ERR_MES &= ControlChars.NewLine & "  --> オーダー№         =" & Me.TxtOrderNo.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車           =" & Me.TxtTrainNo.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 発駅               =" & Me.TxtDepstationCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 着駅               =" & Me.TxtArrstationCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)積込日       =" & Me.TxtLoadingDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)発日         =" & Me.TxtDepDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)積車着日     =" & Me.TxtArrDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)受入日       =" & Me.TxtAccDate.Text
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)空車着日     =" & Me.TxtEmparrDate.Text

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
    ''' エラーレポート編集(一覧用(タブ「タンク車明細」))
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckListTab3ERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項番               =" & OIM0003row("LINECNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注№             =" & OIM0003row("ORDERNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注油種           =" & OIM0003row("ORDERINGOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車№         =" & OIM0003row("TANKNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 数量(kl)           =" & OIM0003row("CARSAMOUNT")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' エラーレポート編集(一覧用(タブ「費用入力」))
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckListTab4ERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項番               =" & OIM0003row("LINECNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 計上年月           =" & OIM0003row("KEIJYOYM") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目コード         =" & OIM0003row("ACCSEGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目名　　         =" & OIM0003row("ACCSEGNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 金額　　　         =" & OIM0003row("APPLYCHARGESUM") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 請求先コード       =" & OIM0003row("INVOICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 請求先名　　       =" & OIM0003row("INVOICENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 支払先コード       =" & OIM0003row("PAYEECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 支払先名　　　     =" & OIM0003row("PAYEENAME")
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
    ''' 受注オーダーしているタンク車の存在確認
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_FindOrderTank()

        '○ 受注登録しているタンク車№を検索
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_ORDERTANK_FIND(WW_ERRCODE, SQLcon)
        End Using
        If WW_ERRCODE = "ERR" Then Exit Sub

        '◯ 受注登録しているタンク車が存在した場合
        If OIT0003FIDtbl_tab3.Rows.Count <> 0 Then

            '存在したタンク車№だけ、タンク車所在の更新を実施
            For Each OIT0003FIDrow As DataRow In OIT0003FIDtbl_tab3.Rows
                '〇タンク車所在の更新
                '受注進行ステータスが以下の場合
                Select Case OIT0003FIDrow("ORDERSTATUS")
                    '100:受注受付
                    Case BaseDllConst.CONST_ORDERSTATUS_100
                        '### 特になし #########################

                    '200:手配
                    '210:手配中（入換指示入力済）, 220:手配中（積込指示入力済）
                    '230:手配中（託送指示手配済）, 240:手配中（入換指示未入力）, 250:手配中（積込指示未入力）
                    '260:手配中（託送指示未手配）
                    '270:手配中(入換積込指示手配済), 280:手配中(託送指示未手配)入換積込手配連絡（手配・結果受理）
                    '290:手配中(入換積込未連絡), 300:手配中(入換積込未確認)
                    '310:手配完了
                    '### 20200722 受注進行ステータスの制御を追加 #################################
                    '205:手配中（千葉(根岸を除く)以外）
                    '305:手配完了（託送未）
                    Case BaseDllConst.CONST_ORDERSTATUS_200,
                         BaseDllConst.CONST_ORDERSTATUS_210,
                         BaseDllConst.CONST_ORDERSTATUS_220,
                         BaseDllConst.CONST_ORDERSTATUS_230,
                         BaseDllConst.CONST_ORDERSTATUS_240,
                         BaseDllConst.CONST_ORDERSTATUS_250,
                         BaseDllConst.CONST_ORDERSTATUS_260,
                         BaseDllConst.CONST_ORDERSTATUS_270,
                         BaseDllConst.CONST_ORDERSTATUS_280,
                         BaseDllConst.CONST_ORDERSTATUS_290,
                         BaseDllConst.CONST_ORDERSTATUS_300,
                         BaseDllConst.CONST_ORDERSTATUS_310,
                         BaseDllConst.CONST_ORDERSTATUS_205,
                         BaseDllConst.CONST_ORDERSTATUS_305

                        '★タンク車所在の更新
                        '引数１：所在地コード　⇒　変更なし(空白)
                        '引数２：タンク車状態　⇒　変更あり("1"(発送))
                        '引数３：積車区分　　　⇒　変更なし(空白)
                        '引数４：(予定)空車着日⇒　更新対象(画面項目)
                        WW_UpdateTankShozai("", "1", "", upEmparrDate:=True,
                                            I_TANKNO:=OIT0003FIDrow("TANKNO"), I_EMPARRDATE:=OIT0003FIDrow("EMPARRDATE"), I_ORDERNO:=OIT0003FIDrow("ORDERNO"))

                    '320:受注確定((実績)積込日設定済み)
                    Case BaseDllConst.CONST_ORDERSTATUS_320

                        '引数１：所在地コード　⇒　変更なし(空白)
                        '引数２：タンク車状態　⇒　変更あり("1"(発送))
                        '引数３：積車区分　　　⇒　変更あり("F"(積車))
                        '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                        '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                        WW_UpdateTankShozai("", "1", "F", I_SITUATION:="2", upLastOilCode:=True,
                                            I_TANKNO:=OIT0003FIDrow("TANKNO"), I_ORDERNO:=OIT0003FIDrow("ORDERNO"))

                    '350:受注確定((実績)発日設定済み)
                    Case BaseDllConst.CONST_ORDERSTATUS_350

                        '★タンク車所在の更新
                        '引数１：所在地コード　⇒　変更あり(着駅)
                        '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
                        '引数３：積車区分　　　⇒　変更あり("F"(積車))
                        '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                        '### 20200828 START 前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                        '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                        'WW_UpdateTankShozai(OIT0003FIDrow("ARRSTATION"), "2", "F", I_SITUATION:="2",
                        '                    I_TANKNO:=OIT0003FIDrow("TANKNO"), I_ORDERNO:=OIT0003FIDrow("ORDERNO"))
                        WW_UpdateTankShozai(OIT0003FIDrow("ARRSTATION"), "2", "F", I_SITUATION:="2",
                                            I_TANKNO:=OIT0003FIDrow("TANKNO"), I_ORDERNO:=OIT0003FIDrow("ORDERNO"), upLastOilCode:=True)
                        '### 20200828 END   前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 

                    '400:受入確認中((実績)積車着日設定済み)
                    Case BaseDllConst.CONST_ORDERSTATUS_400

                        '★タンク車所在の更新
                        '引数１：所在地コード　⇒　変更あり(着駅)
                        '引数２：タンク車状態　⇒　変更あり("3"(到着))
                        '引数３：積車区分　　　⇒　変更あり("F"(積車))
                        '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                        '### 20200828 START 前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                        '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                        'WW_UpdateTankShozai(OIT0003FIDrow("ARRSTATION"), "3", "F", I_SITUATION:="2",
                        '                    I_TANKNO:=OIT0003FIDrow("TANKNO"), I_ORDERNO:=OIT0003FIDrow("ORDERNO"))
                        WW_UpdateTankShozai(OIT0003FIDrow("ARRSTATION"), "3", "F", I_SITUATION:="2",
                                            I_TANKNO:=OIT0003FIDrow("TANKNO"), I_ORDERNO:=OIT0003FIDrow("ORDERNO"), upLastOilCode:=True)
                        '### 20200828 END   前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 

                    '450:受入確認中((実績)受入日設定済み)
                    Case BaseDllConst.CONST_ORDERSTATUS_450

                        '★タンク車所在の更新
                        '引数１：所在地コード　⇒　変更あり(着駅)
                        '引数２：タンク車状態　⇒　変更あり("3"(到着))
                        '引数３：積車区分　　　⇒　変更あり("E"(空車))
                        '引数４：タンク車状況　⇒　変更あり("2"(輸送中))
                        '### 20200828 START 前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 
                        '引数５：前回油種　　　⇒　変更あり(油種⇒前回油種に更新)
                        'WW_UpdateTankShozai(OIT0003FIDrow("ARRSTATION"), "3", "E", I_SITUATION:="2",
                        '                    I_TANKNO:=OIT0003FIDrow("TANKNO"), I_ORDERNO:=OIT0003FIDrow("ORDERNO"))
                        WW_UpdateTankShozai(OIT0003FIDrow("ARRSTATION"), "3", "E", I_SITUATION:="2",
                                            I_TANKNO:=OIT0003FIDrow("TANKNO"), I_ORDERNO:=OIT0003FIDrow("ORDERNO"), upLastOilCode:=True)
                        '### 20200828 END   前回油種の更新追加(積置日＋発日以降の同時設定対応) ######## 

                End Select
            Next
        Else
            '### 特になし #########################
        End If

    End Sub

    ''' <summary>
    ''' 受注TBL登録検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Protected Sub WW_SelectOrder(ByVal SQLcon As SqlConnection,
                                 ByVal I_ORDERNO As String,
                                 ByRef O_dtORDER As DataTable,
                                 Optional I_OFFICECODE As String = Nothing,
                                 Optional I_TANKNO As String = Nothing)

        If IsNothing(O_dtORDER) Then
            O_dtORDER = New DataTable
        End If

        If O_dtORDER.Columns.Count <> 0 Then
            O_dtORDER.Columns.Clear()
        End If

        O_dtORDER.Clear()

        '○ 検索SQL
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
            " SELECT" _
            & "   OIT0002.ORDERNO                 AS ORDERNO" _
            & " , OIT0003.DETAILNO                AS DETAILNO" _
            & " , OIT0002.TRAINNO                 AS TRAINNO" _
            & " , OIT0002.TRAINNO + 'レ'          AS TRAINNO_NM" _
            & " , OIT0002.TRAINNAME               AS TRAINNAME" _
            & " , OIT0002.ORDERYMD                AS ORDERYMD" _
            & " , OIT0002.OFFICECODE              AS OFFICECODE" _
            & " , OIT0002.OFFICENAME              AS OFFICENAME" _
            & " , OIT0002.ORDERTYPE               AS ORDERTYPE" _
            & " , OIT0002.SHIPPERSCODE            AS SHIPPERSCODE" _
            & " , OIT0002.SHIPPERSNAME            AS SHIPPERSNAME" _
            & " , OIT0002.BASECODE                AS BASECODE" _
            & " , OIT0002.BASENAME                AS BASENAME" _
            & " , OIT0002.CONSIGNEECODE           AS CONSIGNEECODE" _
            & " , OIT0002.CONSIGNEENAME           AS CONSIGNEENAME" _
            & " , OIT0002.DEPSTATION              AS DEPSTATION" _
            & " , OIT0002.DEPSTATIONNAME          AS DEPSTATIONNAME" _
            & " , OIT0002.ARRSTATION              AS ARRSTATION" _
            & " , OIT0002.ARRSTATIONNAME          AS ARRSTATIONNAME" _
            & " , OIT0002.RETSTATION              AS RETSTATION" _
            & " , OIT0002.RETSTATIONNAME          AS RETSTATIONNAME" _
            & " , OIT0002.CHANGERETSTATION        AS CHANGERETSTATION" _
            & " , OIT0002.CHANGERETSTATIONNAME    AS CHANGERETSTATIONNAME" _
            & " , OIT0002.ORDERSTATUS             AS ORDERSTATUS" _
            & " , OIT0002.ORDERINFO               AS ORDERINFO" _
            & " , OIT0002.EMPTYTURNFLG            AS EMPTYTURNFLG" _
            & " , OIT0002.STACKINGFLG             AS STACKINGFLG" _
            & " , OIT0002.USEPROPRIETYFLG         AS USEPROPRIETYFLG" _
            & " , OIT0002.CONTACTFLG              AS CONTACTFLG" _
            & " , OIT0002.RESULTFLG               AS RESULTFLG" _
            & " , OIT0002.DELIVERYFLG             AS DELIVERYFLG" _
            & " , OIT0002.LODDATE                 AS LODDATE" _
            & " , OIT0002.DEPDATE                 AS DEPDATE" _
            & " , OIT0002.ARRDATE                 AS ARRDATE" _
            & " , OIT0002.ACCDATE                 AS ACCDATE" _
            & " , OIT0002.EMPARRDATE              AS EMPARRDATE" _
            & " , OIT0002.ACTUALLODDATE           AS ACTUALLODDATE" _
            & " , OIT0002.ACTUALDEPDATE           AS ACTUALDEPDATE" _
            & " , OIT0002.ACTUALARRDATE           AS ACTUALARRDATE" _
            & " , OIT0002.ACTUALACCDATE           AS ACTUALACCDATE" _
            & " , OIT0002.ACTUALEMPARRDATE        AS ACTUALEMPARRDATE" _
            & " , OIT0002.RTANK                   AS RTANK" _
            & " , OIT0002.HTANK                   AS HTANK" _
            & " , OIT0002.TTANK                   AS TTANK" _
            & " , OIT0002.MTTANK                  AS MTTANK" _
            & " , OIT0002.KTANK                   AS KTANK" _
            & " , OIT0002.K3TANK                  AS K3TANK" _
            & " , OIT0002.K5TANK                  AS K5TANK" _
            & " , OIT0002.K10TANK                 AS K10TANK" _
            & " , OIT0002.LTANK                   AS LTANK" _
            & " , OIT0002.ATANK                   AS ATANK" _
            & " , OIT0002.OTHER1OTANK             AS OTHER1OTANK" _
            & " , OIT0002.OTHER2OTANK             AS OTHER2OTANK" _
            & " , OIT0002.OTHER3OTANK             AS OTHER3OTANK" _
            & " , OIT0002.OTHER4OTANK             AS OTHER4OTANK" _
            & " , OIT0002.OTHER5OTANK             AS OTHER5OTANK" _
            & " , OIT0002.OTHER6OTANK             AS OTHER6OTANK" _
            & " , OIT0002.OTHER7OTANK             AS OTHER7OTANK" _
            & " , OIT0002.OTHER8OTANK             AS OTHER8OTANK" _
            & " , OIT0002.OTHER9OTANK             AS OTHER9OTANK" _
            & " , OIT0002.OTHER10OTANK            AS OTHER10OTANK" _
            & " , OIT0002.TOTALTANK               AS TOTALTANK" _
            & " , OIT0002.RTANKCH                 AS RTANKCH" _
            & " , OIT0002.HTANKCH                 AS HTANKCH" _
            & " , OIT0002.TTANKCH                 AS TTANKCH" _
            & " , OIT0002.MTTANKCH                AS MTTANKCH" _
            & " , OIT0002.KTANKCH                 AS KTANKCH" _
            & " , OIT0002.K3TANKCH                AS K3TANKCH" _
            & " , OIT0002.K5TANKCH                AS K5TANKCH" _
            & " , OIT0002.K10TANKCH               AS K10TANKCH" _
            & " , OIT0002.LTANKCH                 AS LTANKCH" _
            & " , OIT0002.ATANKCH                 AS ATANKCH" _
            & " , OIT0002.OTHER1OTANKCH           AS OTHER1OTANKCH" _
            & " , OIT0002.OTHER2OTANKCH           AS OTHER2OTANKCH" _
            & " , OIT0002.OTHER3OTANKCH           AS OTHER3OTANKCH" _
            & " , OIT0002.OTHER4OTANKCH           AS OTHER4OTANKCH" _
            & " , OIT0002.OTHER5OTANKCH           AS OTHER5OTANKCH" _
            & " , OIT0002.OTHER6OTANKCH           AS OTHER6OTANKCH" _
            & " , OIT0002.OTHER7OTANKCH           AS OTHER7OTANKCH" _
            & " , OIT0002.OTHER8OTANKCH           AS OTHER8OTANKCH" _
            & " , OIT0002.OTHER9OTANKCH           AS OTHER9OTANKCH" _
            & " , OIT0002.OTHER10OTANKCH          AS OTHER10OTANKCH" _
            & " , OIT0002.TOTALTANKCH             AS TOTALTANKCH" _
            & " , OIT0002.TANKLINKNO              AS TANKLINKNO" _
            & " , OIT0002.TANKLINKNOMADE          AS TANKLINKNOMADE" _
            & " , OIT0002.BILLINGNO               AS BILLINGNO" _
            & " , OIT0002.KEIJYOYMD               AS KEIJYOYMD" _
            & " , OIT0002.SALSE                   AS SALSE" _
            & " , OIT0002.SALSETAX                AS SALSETAX" _
            & " , OIT0002.TOTALSALSE              AS TOTALSALSE" _
            & " , OIT0002.PAYMENT                 AS PAYMENT" _
            & " , OIT0002.PAYMENTTAX              AS PAYMENTTAX" _
            & " , OIT0002.TOTALPAYMENT            AS TOTALPAYMENT" _
            & " , OIT0002.OTFILENAME              AS OTFILENAME" _
            & " , OIT0002.RECEIVECOUNT            AS RECEIVECOUNT" _
            & " , OIT0003.SHIPORDER               AS SHIPORDER" _
            & " , OIT0003.LINEORDER               AS LINEORDER" _
            & " , OIT0003.TANKNO                  AS TANKNO" _
            & " , OIT0003.KAMOKU                  AS KAMOKU" _
            & " , OIT0003.STACKINGORDERNO         AS STACKINGORDERNO" _
            & " , OIT0003.STACKINGFLG             AS DETAIL_STACKINGFLG" _
            & " , OIT0003.FIRSTRETURNFLG          AS FIRSTRETURNFLG" _
            & " , OIT0003.AFTERRETURNFLG          AS AFTERRETURNFLG" _
            & " , OIT0003.OTTRANSPORTFLG          AS OTTRANSPORTFLG" _
            & " , OIT0003.ORDERINFO               AS DETAIL_ORDERINFO" _
            & " , OIT0003.SHIPPERSCODE            AS DETAIL_SHIPPERSCODE" _
            & " , OIT0003.SHIPPERSNAME            AS DETAIL_SHIPPERSNAME" _
            & " , OIT0003.OILCODE                 AS OILCODE" _
            & " , OIT0003.OILNAME                 AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE            AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME         AS ORDERINGOILNAME" _
            & " , OIT0003.CARSNUMBER              AS CARSNUMBER" _
            & " , OIT0003.CARSAMOUNT              AS CARSAMOUNT" _
            & " , OIT0003.RETURNDATETRAIN         AS RETURNDATETRAIN" _
            & " , OIT0003.JOINTCODE               AS JOINTCODE" _
            & " , OIT0003.JOINT                   AS JOINT" _
            & " , OIT0003.REMARK                  AS REMARK" _
            & " , OIT0003.CHANGETRAINNO           AS CHANGETRAINNO" _
            & " , OIT0003.CHANGETRAINNAME         AS CHANGETRAINNAME" _
            & " , OIT0003.SECONDCONSIGNEECODE     AS SECONDCONSIGNEECODE" _
            & " , OIT0003.SECONDCONSIGNEENAME     AS SECONDCONSIGNEENAME" _
            & " , OIT0003.SECONDARRSTATION        AS SECONDARRSTATION" _
            & " , OIT0003.SECONDARRSTATIONNAME    AS SECONDARRSTATIONNAME" _
            & " , OIT0003.CHANGERETSTATION        AS DETAIL_CHANGERETSTATION" _
            & " , OIT0003.CHANGERETSTATIONNAME    AS DETAIL_CHANGERETSTATIONNAME" _
            & " , OIT0003.LINE                    AS LINE" _
            & " , OIT0003.FILLINGPOINT            AS FILLINGPOINT" _
            & " , OIT0003.LOADINGIRILINETRAINNO   AS LOADINGIRILINETRAINNO" _
            & " , OIT0003.LOADINGIRILINETRAINNAME AS LOADINGIRILINETRAINNAME" _
            & " , OIT0003.LOADINGIRILINEORDER     AS LOADINGIRILINEORDER" _
            & " , OIT0003.LOADINGOUTLETTRAINNO    AS LOADINGOUTLETTRAINNO" _
            & " , OIT0003.LOADINGOUTLETTRAINNAME  AS LOADINGOUTLETTRAINNAME" _
            & " , OIT0003.LOADINGOUTLETORDER      AS LOADINGOUTLETORDER" _
            & " , OIT0003.ACTUALLODDATE           AS DETAIL_ACTUALLODDATE" _
            & " , OIT0003.ACTUALDEPDATE           AS DETAIL_ACTUALDEPDATE" _
            & " , OIT0003.ACTUALARRDATE           AS DETAIL_ACTUALARRDATE" _
            & " , OIT0003.ACTUALACCDATE           AS DETAIL_ACTUALACCDATE" _
            & " , OIT0003.ACTUALEMPARRDATE        AS DETAIL_ACTUALEMPARRDATE" _
            & " , OIT0003.SALSE                   AS DETAIL_SALSE" _
            & " , OIT0003.SALSETAX                AS DETAIL_SALSETAX" _
            & " , OIT0003.TOTALSALSE              AS DETAIL_TOTALSALSE" _
            & " , OIT0003.PAYMENT                 AS DETAIL_PAYMENT" _
            & " , OIT0003.PAYMENTTAX              AS DETAIL_PAYMENTTAX" _
            & " , OIT0003.TOTALPAYMENT            AS DETAIL_TOTALPAYMENT" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO "

        '○ 検索条件が指定されていれば追加する
        'タンク車№
        If Not String.IsNullOrEmpty(I_TANKNO) Then
            SQLStr &= String.Format(" AND OIT0003.TANKNO = '{0}' ", I_TANKNO)
        End If
        '削除フラグ
        SQLStr &= String.Format(" AND OIT0003.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        '受注No
        SQLStr &= String.Format(" WHERE OIT0002.ORDERNO = '{0}' ", I_ORDERNO)
        '受注営業所コード
        If Not String.IsNullOrEmpty(I_OFFICECODE) Then
            SQLStr &= String.Format(" AND OIT0002.OFFICECODE = '{0}' ", I_OFFICECODE)
        End If
        '受注進行ステータス
        SQLStr &= String.Format(" AND OIT0002.ORDERSTATUS <> '{0}' ", BaseDllConst.CONST_ORDERSTATUS_900)
        '削除フラグ
        SQLStr &= String.Format(" AND OIT0002.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtORDER.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtORDER.Load(SQLdr)
                End Using

                'Dim i As Integer = 0
                'For Each O_dtORDERrow As DataRow In O_dtORDER.Rows
                '    i += 1
                '    O_dtORDERrow("LINECNT") = i        'LINECNT
                'Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D SELECT_ORDER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D SELECT_ORDER"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注登録しているタンク車№を検索
    ''' </summary>
    ''' <param name="sqlCon"></param>
    Private Sub WW_ORDERTANK_FIND(ByRef O_RTN As String, ByVal SQLcon As SqlConnection)

        O_RTN = C_MESSAGE_NO.NORMAL

        If IsNothing(OIT0003FIDtbl_tab3) Then
            OIT0003FIDtbl_tab3 = New DataTable
        End If

        If OIT0003FIDtbl_tab3.Columns.Count <> 0 Then
            OIT0003FIDtbl_tab3.Columns.Clear()
        End If

        OIT0003FIDtbl_tab3.Clear()

        '○ 検索SQL
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
            " SELECT" _
            & "   ISNULL(RTRIM(MERGE_TBL.ORDERNO), '')                  AS ORDERNO" _
            & " , ISNULL(RTRIM(MERGE_TBL.ORDERSTATUS), '')              AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(MERGE_TBL.OFFICECODE), '')               AS OFFICECODE" _
            & " , ISNULL(RTRIM(MERGE_TBL.OFFICENAME), '')               AS OFFICENAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.SHIPPERSCODE), '')             AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(MERGE_TBL.SHIPPERSNAME), '')             AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.BASECODE), '')                 AS BASECODE" _
            & " , ISNULL(RTRIM(MERGE_TBL.BASENAME), '')                 AS BASENAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.CONSIGNEECODE), '')            AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(MERGE_TBL.CONSIGNEENAME), '')            AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.TANKNO), '')                   AS TANKNO" _
            & " , ISNULL(RTRIM(MERGE_TBL.OILCODE), '')                  AS OILCODE" _
            & " , ISNULL(RTRIM(MERGE_TBL.OILNAME), '')                  AS OILNAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.ORDERINGTYPE), '')             AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(MERGE_TBL.ORDERINGOILNAME), '')          AS ORDERINGOILNAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.DEPSTATION), '')               AS DEPSTATION" _
            & " , ISNULL(RTRIM(MERGE_TBL.DEPSTATIONNAME), '')           AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.ARRSTATION), '')               AS ARRSTATION" _
            & " , ISNULL(RTRIM(MERGE_TBL.ARRSTATIONNAME), '')           AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(MERGE_TBL.LODDATE), '')                  AS LODDATE" _
            & " , ISNULL(RTRIM(MERGE_TBL.DEPDATE), '')                  AS DEPDATE" _
            & " , ISNULL(RTRIM(MERGE_TBL.ARRDATE), '')                  AS ARRDATE" _
            & " , ISNULL(RTRIM(MERGE_TBL.ACCDATE), '')                  AS ACCDATE" _
            & " , ISNULL(RTRIM(MERGE_TBL.EMPARRDATE), '')               AS EMPARRDATE" _
            & " FROM ( " _
            & "     SELECT " _
            & "           OIT0003.* " _
            & "         , ROW_NUMBER() OVER(PARTITION BY OIT0003.TANKNO ORDER BY OIT0003.LODDATE) RNUM "

        '①受注テーブルと受注明細テーブルの情報を取得(利用可能のデータ)
        SQLStr &=
              "     FROM ( " _
            & "         SELECT " _
            & "               OIT0002.ORDERNO " _
            & "             , OIT0002.ORDERSTATUS " _
            & "             , OIT0002.OFFICECODE " _
            & "             , OIT0002.OFFICENAME " _
            & "             , OIT0003.SHIPPERSCODE " _
            & "             , OIT0003.SHIPPERSNAME " _
            & "             , OIT0002.BASECODE " _
            & "             , OIT0002.BASENAME " _
            & "             , OIT0002.CONSIGNEECODE " _
            & "             , OIT0002.CONSIGNEENAME " _
            & "             , OIT0002.DEPSTATION " _
            & "             , OIT0002.DEPSTATIONNAME " _
            & "             , OIT0002.ARRSTATION " _
            & "             , OIT0002.ARRSTATIONNAME " _
            & "             , OIT0002.LODDATE " _
            & "             , OIT0002.DEPDATE " _
            & "             , OIT0002.ARRDATE " _
            & "             , OIT0002.ACCDATE " _
            & "             , OIT0002.EMPARRDATE " _
            & "             , OIT0003.TANKNO " _
            & "             , OIT0003.OILCODE " _
            & "             , OIT0003.OILNAME " _
            & "             , OIT0003.ORDERINGTYPE " _
            & "             , OIT0003.ORDERINGOILNAME " _
            & "         FROM OIL.OIT0002_ORDER OIT0002 " _
            & "         INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "               OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "               AND OIT0003.DELFLG <> @P02" _
            & "         WHERE OIT0002.USEPROPRIETYFLG = '1' " _
            & "         AND OIT0002.DELFLG <> @P02" _
            & "         AND OIT0002.ORDERSTATUS <> @P03" _
            & "     ) OIT0003"

        '②現在指定している受注オーダーの情報を取得
        SQLStr &=
              "     INNER JOIN ( " _
            & "         SELECT " _
            & "               OIT0002.ORDERNO " _
            & "             , OIT0002.ORDERSTATUS " _
            & "             , OIT0002.OFFICECODE " _
            & "             , OIT0002.OFFICENAME " _
            & "             , OIT0003.SHIPPERSCODE " _
            & "             , OIT0003.SHIPPERSNAME " _
            & "             , OIT0002.BASECODE " _
            & "             , OIT0002.BASENAME " _
            & "             , OIT0002.CONSIGNEECODE " _
            & "             , OIT0002.CONSIGNEENAME " _
            & "             , OIT0002.DEPSTATION " _
            & "             , OIT0002.DEPSTATIONNAME " _
            & "             , OIT0002.ARRSTATION " _
            & "             , OIT0002.ARRSTATIONNAME " _
            & "             , OIT0002.LODDATE " _
            & "             , OIT0002.DEPDATE " _
            & "             , OIT0002.ARRDATE " _
            & "             , OIT0002.ACCDATE " _
            & "             , OIT0002.EMPARRDATE " _
            & "             , OIT0003.TANKNO " _
            & "             , OIT0003.OILCODE " _
            & "             , OIT0003.OILNAME " _
            & "             , OIT0003.ORDERINGTYPE " _
            & "             , OIT0003.ORDERINGOILNAME " _
            & "         FROM OIL.OIT0002_ORDER OIT0002 " _
            & "         INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "               OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "               AND OIT0003.ORDERNO = @P01 " _
            & "               AND OIT0003.DELFLG <> @P02" _
            & "     ) OIT0003_GEN ON "

        '②の受注オーダー以外でタンク車Noを使用している受注オーダーがないか、①のデータから検索
        SQLStr &=
              "           OIT0003_GEN.TANKNO = OIT0003.TANKNO " _
            & "           AND OIT0003_GEN.DEPDATE < OIT0003.DEPDATE " _
            & "     WHERE OIT0003.ORDERNO <> @P01" _
            & "     AND OIT0003.TANKNO IN ("

        '一覧に設定しているタンク車№を条件に設定
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            If OIT0003row("LINECNT") = 1 Then
                SQLStr &= "'" & OIT0003row("TANKNO") & "' "
            Else
                SQLStr &= ", '" & OIT0003row("TANKNO") & "' "
            End If
        Next

        '※一致したデータが複数ある場合、一番(予定)発日が過去の日を取得
        SQLStr &=
             "      )" _
            & " ) MERGE_TBL" _
            & " WHERE MERGE_TBL.RNUM = 1"

        'SQLStr &=
        '      " ORDER BY" _
        '    & "    MERGE_TBL.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 3)  '受注進行ステータス
                PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE
                PARA03.Value = BaseDllConst.CONST_ORDERSTATUS_900

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003FIDtbl_tab3.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003FIDtbl_tab3.Load(SQLdr)
                End Using

                'Dim i As Integer = 0
                'For Each OIT0003FIDtab3row As DataRow In OIT0003FIDtbl_tab3.Rows
                '    i += 1
                '    OIT0003FIDtab3row("LINECNT") = i        'LINECNT
                'Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D FIND_ORDERTANK")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D FIND_ORDERTANK"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
                WW_ListTextBoxReadControlTab1()

            '入換・積込指示
            Case 1
                '〇 (一覧)テキストボックスの制御(読取専用)
                WW_ListTextBoxReadControlTab2()

            'タンク車明細
            Case 2
                '〇 (一覧)テキストボックスの制御(読取専用)
                WW_ListTextBoxReadControlTab3()

                '費用入力
            Case 3
                '〇 (一覧)テキストボックスの制御(読取専用)
                WW_ListTextBoxReadControlTab4()

        End Select

    End Sub

    ''' <summary>
    ''' タブ(タンク車割当)
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControlTab1()
        '〇 (一覧)テキストボックスの制御(読取専用)
        Dim divObj = DirectCast(pnlListArea1.FindControl(pnlListArea1.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)
        '　ループ内の対象データROW(これでXXX項目の値をとれるかと）
        Dim loopdr As DataRow = Nothing
        '　データテーブルの行Index
        Dim rowIdx As Integer = 0
        '### ★積置（チェックボックス）を非活性にするための準備 ################
        Dim chkObjST As CheckBox = Nothing
        Dim chkObjIdWOSTcnt As String = "chk" & pnlListArea1.ID & "STACKINGFLG"
        Dim chkObjSTId As String
        '#######################################################################

        '受注進行ステータスが"受注受付"の場合
        '※但し、受注営業所が"011203"(袖ヶ浦営業所)以外の場合は、貨物駅入線順を読取専用(入力不可)とする。
        '※但し、受注営業所が"010402"(仙台新港営業所)以外の場合は、積込日を読取専用(入力不可)とする。
        '※但し、発送順区分が"2"(発送対象外)の場合は、発送順を読取専用(入力不可)とする。
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then
            For Each rowitem As TableRow In tblObj.Rows
                '### ★積置選択（チェックボックス）を非活性にする ######################
                loopdr = CS0013ProfView.SRCDATA.Rows(rowIdx)
                chkObjSTId = chkObjIdWOSTcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjST = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjST = DirectCast(cellObj.FindControl(chkObjSTId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjST IsNot Nothing Then
                        Exit For
                    End If
                Next
                '◯ 受注営業所が"010402"(仙台新港営業所)以外の場合
                If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_010402 Then
                    chkObjST.Enabled = False
                End If
                '###################################################################

                For Each cellObj As TableCell In rowitem.Controls
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPPERSNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ORDERINGOILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "JRINSPECTIONDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "JOINT") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDARRSTATIONNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDCONSIGNEENAME") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALLODDATE") Then
                        If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_010402 Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        End If
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "LINEORDER") _
                        AndAlso Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_011203 Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPORDER") _
                        AndAlso work.WF_SEL_SHIPORDERCLASS.Text = "2" Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    End If
                Next
                rowIdx += 1
            Next

            '受注進行ステータス＝"200:手配"
            '受注進行ステータス＝"210:手配中(入換指示入力済)"
            '受注進行ステータス＝"220:手配中(積込指示入力済)"
            '受注進行ステータス＝"230:手配中(託送指示手配済)"
            '受注進行ステータス＝"240:手配中(入換指示未入力)"
            '受注進行ステータス＝"250:手配中(積込指示未入力)"
            '受注進行ステータス＝"260:手配中(託送指示未手配)"
            '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
            '受注進行ステータス＝"270:手配中(入換積込指示手配済)"
            '受注進行ステータス＝"280:手配中(託送指示未手配)"入換積込手配連絡（手配・結果受理）
            '受注進行ステータス＝"290:手配中(入換積込未連絡)"
            '受注進行ステータス＝"300:手配中(入換積込未確認)"
            '### END   ##################################################################
            '受注進行ステータス＝"310:手配完了"
            '※但し、受注営業所が"011203"(袖ヶ浦営業所)以外の場合は、貨物駅入線順を読取専用(入力不可)とする。
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
            '305:手配完了（託送未）
        ElseIf work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_200 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_210 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_220 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_230 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_240 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_250 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_260 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_270 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_280 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_290 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_300 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_310 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_205 _
            OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_305 Then

            Dim chkTankNocnt As String = "txt" & pnlListArea1.ID & "TANKNO"
            Dim chkTankNo As String = ""
            Dim chkJRInspectionDatecnt As String = "txt" & pnlListArea1.ID & "JRINSPECTIONDATE"
            Dim chkJRInspectionDate As String = ""

            For Each rowitem As TableRow In tblObj.Rows
                '### ★積置選択（チェックボックス）を非活性にする ##################
                loopdr = CS0013ProfView.SRCDATA.Rows(rowIdx)
                chkObjSTId = chkObjIdWOSTcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjST = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjST = DirectCast(cellObj.FindControl(chkObjSTId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjST IsNot Nothing Then
                        Exit For
                    End If
                Next
                '◯ 受注営業所が"010402"(仙台新港営業所)以外の場合
                If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_010402 Then
                    chkObjST.Enabled = False
                End If
                '###################################################################

                For Each cellObj As TableCell In rowitem.Controls
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPPERSNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ORDERINGOILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "JRINSPECTIONDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "JOINT") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDARRSTATIONNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDCONSIGNEENAME") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALLODDATE") Then
                        If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_010402 Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        End If
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "LINEORDER") _
                        AndAlso Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_011203 Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPORDER") _
                        AndAlso work.WF_SEL_SHIPORDERCLASS.Text = "2" Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    End If
                Next

                '★タンク車の状態が「発送」の場合は、入力を許可しない。
                '★同受注Noとタンク車所在で管理している使用受注№が同じ場合(2020/08/11追加)
                '画面表示行が存在している場合
                If OIT0003tbl.Rows.Count <> 0 Then
                    loopdr = OIT0003tbl.Rows(rowIdx)
                    If loopdr("TANKSTATUS") = "1" AndAlso loopdr("DELFLG") = "0" _
                        AndAlso loopdr("ORDERNO") = loopdr("USEORDERNO") Then
                        '◯ タンク車№
                        chkTankNo = chkTankNocnt & Convert.ToString(loopdr("LINECNT"))
                        For Each cellObj As TableCell In rowitem.Controls
                            'コントロールが見つかったら脱出
                            If cellObj.Text.Contains(chkTankNo) Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                Exit For
                            End If
                        Next
                        '◯ 交検日
                        chkJRInspectionDate = chkJRInspectionDatecnt & Convert.ToString(loopdr("LINECNT"))
                        For Each cellObj As TableCell In rowitem.Controls
                            'コントロールが見つかったら脱出
                            If cellObj.Text.Contains(chkJRInspectionDate) Then
                                cellObj.Text = cellObj.Text.Replace(" readonly='readonly' class='iconOnly'>", " readonly='readonly'>")
                                Exit For
                            End If
                        Next
                    End If
                End If
                rowIdx += 1
            Next

            '受注進行ステータスが"310：手配完了"以降のステータスの場合
        Else
            '### ★選択（チェックボックス）を非活性にするための準備 ################
            Dim chkObj As CheckBox = Nothing
            '　LINECNTを除いたチェックボックスID
            Dim chkObjIdWOLincnt As String = "chk" & pnlListArea1.ID & "OPERATION"
            '　LINECNTを含むチェックボックスID
            Dim chkObjId As String
            'Dim chkObjType As String
            '#######################################################################

            For Each rowitem As TableRow In tblObj.Rows
                '### ★選択（チェックボックス）を非活性にする ######################
                loopdr = CS0013ProfView.SRCDATA.Rows(rowIdx)
                chkObjId = chkObjIdWOLincnt & Convert.ToString(loopdr("LINECNT"))
                'chkObjType = Convert.ToString(loopdr("CALCACCOUNT"))
                chkObj = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObj = DirectCast(cellObj.FindControl(chkObjId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObj IsNot Nothing Then
                        Exit For
                    End If
                Next
                chkObj.Enabled = False
                '###################################################################

                '### ★積置選択（チェックボックス）を非活性にする ##################
                loopdr = CS0013ProfView.SRCDATA.Rows(rowIdx)
                chkObjSTId = chkObjIdWOSTcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjST = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjST = DirectCast(cellObj.FindControl(chkObjSTId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjST IsNot Nothing Then
                        Exit For
                    End If
                Next
                '◯ 受注営業所が"010402"(仙台新港営業所)以外の場合
                If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_010402 Then
                    chkObjST.Enabled = False
                End If
                '###################################################################

                For Each cellObj As TableCell In rowitem.Controls
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPPERSNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ORDERINGOILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPORDER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "TANKNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "LINEORDER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "JRINSPECTIONDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALLODDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "JOINT") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "CHANGETRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDCONSIGNEENAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SECONDARRSTATIONNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "CHANGERETSTATIONNAME") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    End If
                Next
                rowIdx += 1
            Next
        End If
    End Sub

    ''' <summary>
    ''' タブ(入換・積込指示)
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControlTab2()
        '〇 (一覧)テキストボックスの制御(読取専用)
        Dim divObj = DirectCast(pnlListArea2.FindControl(pnlListArea2.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)

        '〇 受注進行ステータスの状態
        Select Case work.WF_SEL_ORDERSTATUS.Text
                '受注進行ステータス＝"200:手配"
                '受注進行ステータス＝"210:手配中(入換指示入力済)"
                '受注進行ステータス＝"220:手配中(積込指示入力済)"
                '受注進行ステータス＝"230:手配中(託送指示手配済)"
                '受注進行ステータス＝"240:手配中(入換指示未入力)"
                '受注進行ステータス＝"250:手配中(積込指示未入力)"
                '受注進行ステータス＝"260:手配中(託送指示未手配)"
                '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
                '受注進行ステータス＝"270:手配中(入換積込指示手配済)"
                '受注進行ステータス＝"280:手配中(託送指示未手配)"入換積込手配連絡（手配・結果受理）
                '受注進行ステータス＝"290:手配中(入換積込未連絡)"
                '受注進行ステータス＝"300:手配中(入換積込未確認)"
                '### END   ##################################################################
            Case BaseDllConst.CONST_ORDERSTATUS_200,
                 BaseDllConst.CONST_ORDERSTATUS_210,
                 BaseDllConst.CONST_ORDERSTATUS_220,
                 BaseDllConst.CONST_ORDERSTATUS_230,
                 BaseDllConst.CONST_ORDERSTATUS_240,
                 BaseDllConst.CONST_ORDERSTATUS_250,
                 BaseDllConst.CONST_ORDERSTATUS_260,
                 BaseDllConst.CONST_ORDERSTATUS_270,
                 BaseDllConst.CONST_ORDERSTATUS_280,
                 BaseDllConst.CONST_ORDERSTATUS_290,
                 BaseDllConst.CONST_ORDERSTATUS_300
                '五井営業所、甲子営業所、袖ヶ浦営業所の場合
                '積込列車番号の入力を可能とする。
                If work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011201 _
                    OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011202 _
                    OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then

                    WW_RINKAIFLG = True

                    For Each rowitem As TableRow In tblObj.Rows
                        For Each cellObj As TableCell In rowitem.Controls
                            If cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGIRILINETRAINNO") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "LOADINGOUTLETTRAINNO") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                            End If
                            '### 20200616 START((全体)No74対応) ######################################
                            '★袖ヶ浦営業所の場合、充填ポイントを入力不可とする。
                            If work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 _
                                AndAlso cellObj.Text.Contains("input id=""txt" & pnlListArea2.ID & "FILLINGPOINT") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                            End If
                            '### 20200616 END  ((全体)No74対応) ######################################
                        Next
                    Next

                    '上記以外(仙台営業所、根岸営業所、四日市営業所、三重塩浜営業所)の場合
                    '積込列車番号の入力を不可とする。
                    '充填ポイントの入力を不可とする。(20200221石油部打合せ内容反映)
                Else
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
    End Sub

    ''' <summary>
    ''' タブ(タンク車明細)
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControlTab3()
        '〇 (一覧)テキストボックスの制御(読取専用)
        Dim divObj = DirectCast(pnlListArea3.FindControl(pnlListArea3.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)
        Dim chkObjST As CheckBox = Nothing
        Dim chkObjFR As CheckBox = Nothing
        Dim chkObjAF As CheckBox = Nothing
        Dim chkObjOT As CheckBox = Nothing
        'LINECNTを除いたチェックボックスID
        Dim chkObjIdWOSTcnt As String = "chk" & pnlListArea3.ID & "STACKINGFLG"
        Dim chkObjIdWOFRcnt As String = "chk" & pnlListArea3.ID & "FIRSTRETURNFLG"
        Dim chkObjIdWOAFcnt As String = "chk" & pnlListArea3.ID & "AFTERRETURNFLG"
        Dim chkObjIdWOOTcnt As String = "chk" & pnlListArea3.ID & "OTTRANSPORTFLG"
        'LINECNTを含むチェックボックスID
        Dim chkObjSTId As String
        Dim chkObjFRId As String
        Dim chkObjAFId As String
        Dim chkObjOTId As String
        Dim chkObjType As String = ""
        'ループ内の対象データROW(これで計算区分の値をとれるかと）
        Dim loopdr As DataRow = Nothing
        'データテーブルの行Index
        Dim rowIdx As Integer = 0

        '〇 受注進行ステータスの状態
        Select Case work.WF_SEL_ORDERSTATUS.Text
            '### 20200722 受注進行ステータスの制御を追加 #################################
            '205:手配中（千葉(根岸を除く)以外）
            '305:手配完了（託送未）
            Case BaseDllConst.CONST_ORDERSTATUS_310,
                 BaseDllConst.CONST_ORDERSTATUS_320,
                 BaseDllConst.CONST_ORDERSTATUS_350,
                 BaseDllConst.CONST_ORDERSTATUS_400,
                 BaseDllConst.CONST_ORDERSTATUS_450,
                 BaseDllConst.CONST_ORDERSTATUS_205,
                 BaseDllConst.CONST_ORDERSTATUS_305

                For Each rowitem As TableRow In tblObj.Rows
                    '画面表示行が存在している場合
                    If OIT0003tbl_tab3.Rows.Count <> 0 Then
                        loopdr = OIT0003tbl_tab3.Rows(rowIdx)
                        chkObjSTId = chkObjIdWOSTcnt & Convert.ToString(loopdr("LINECNT"))
                        chkObjFRId = chkObjIdWOFRcnt & Convert.ToString(loopdr("LINECNT"))
                        chkObjAFId = chkObjIdWOAFcnt & Convert.ToString(loopdr("LINECNT"))
                        chkObjOTId = chkObjIdWOOTcnt & Convert.ToString(loopdr("LINECNT"))
                        chkObjType = Convert.ToString(loopdr("STACKINGORDERNO"))
                        '下のループより先に見つけなければいけないかもしれないので
                        '冗長ですがこちらでループ
                        chkObjST = Nothing
                        For Each cellObj As TableCell In rowitem.Controls
                            chkObjST = DirectCast(cellObj.FindControl(chkObjSTId), CheckBox)
                            'コントロールが見つかったら脱出
                            If chkObjST IsNot Nothing Then
                                Exit For
                            End If
                        Next
                        chkObjFR = Nothing
                        For Each cellObj As TableCell In rowitem.Controls
                            chkObjFR = DirectCast(cellObj.FindControl(chkObjFRId), CheckBox)
                            'コントロールが見つかったら脱出
                            If chkObjFR IsNot Nothing Then
                                Exit For
                            End If
                        Next
                        '### 20200622 START((全体)No87対応) ######################################
                        chkObjAF = Nothing
                        For Each cellObj As TableCell In rowitem.Controls
                            chkObjAF = DirectCast(cellObj.FindControl(chkObjAFId), CheckBox)
                            'コントロールが見つかったら脱出
                            If chkObjAF IsNot Nothing Then
                                Exit For
                            End If
                        Next
                        '### 20200622 END  ((全体)No87対応) ######################################
                        '### 20200717 START((全体)No112対応) ######################################
                        chkObjOT = Nothing
                        For Each cellObj As TableCell In rowitem.Controls
                            chkObjOT = DirectCast(cellObj.FindControl(chkObjOTId), CheckBox)
                            'コントロールが見つかったら脱出
                            If chkObjOT IsNot Nothing Then
                                Exit For
                            End If
                        Next
                        '### 20200717 END  ((全体)No112対応) ######################################

                        '◯ 受注営業所が"010402"(仙台新港営業所)以外の場合
                        '### 20200626 積置受注№が設定されている場合(条件追加) #######################
                        If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_010402 _
                            OrElse chkObjType <> "" Then
                            '積込可否フラグ(チェックボックス)を非活性
                            chkObjST.Enabled = False
                        End If
                        '◯ 受注営業所が"011402"(根岸営業所)以外の場合
                        '### 20200618 すでに指定したタンク車№が他の受注で使用されている場合の対応 ### 
                        'Me.WW_USEORDERFLG(TRUE:使用中, FALSE:未使用)
                        '### 20200626 積置受注№が設定されている場合(条件追加) #######################
                        If Me.TxtOrderOfficeCode.Text <> BaseDllConst.CONST_OFFICECODE_011402 _
                            OrElse Me.WW_USEORDERFLG = True _
                            OrElse chkObjType <> "" Then
                            '先返し可否フラグ(チェックボックス)を非活性
                            chkObjFR.Enabled = False
                            '後返し可否フラグ(チェックボックス)を非活性
                            chkObjAF.Enabled = False

                            '### 20200622 START((全体)No87対応) ######################################
                        ElseIf Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_011402 Then
                            If Me.TxtTrainNo.Text = "81" Then
                                '先返し可否フラグ(チェックボックス)を活性
                                chkObjFR.Enabled = True
                                '後返し可否フラグ(チェックボックス)を非活性
                                chkObjAF.Enabled = True
                            ElseIf Me.TxtTrainNo.Text = "83" Then
                                '先返し可否フラグ(チェックボックス)を非活性
                                chkObjFR.Enabled = False
                                '後返し可否フラグ(チェックボックス)を活性
                                chkObjAF.Enabled = True
                            Else
                                '先返し可否フラグ(チェックボックス)を非活性
                                chkObjFR.Enabled = False
                                '後返し可否フラグ(チェックボックス)を非活性
                                chkObjAF.Enabled = False
                            End If
                            '### 20200622 END  ((全体)No87対応) ######################################
                        End If

                        '### 20200717 START((全体)No112対応) ######################################
                        If Me.TxtOrderTrkKbn.Text <> BaseDllConst.CONST_TRKBN_M Then
                            'OT輸送可否フラグ(チェックボックス)を非活性
                            chkObjOT.Enabled = False
                        End If
                        '### 20200717 END  ((全体)No112対応) ######################################
                    End If

                    For Each cellObj As TableCell In rowitem.Controls
                        '### 20200626 積置受注№が設定されている場合(条件追加) #######################
                        '★積置受注№が設定されている場合は、その行すべて入力不可とする。
                        '　(他の受注オーダーにて積込済みのため)
                        If chkObjType <> "" Then

                            '### 20200814 START((全体)No123対応) ######################################
                            '別オーダーの積置きまでロックされる為、数量が入力出来なくなるので
                            '数量だけは入力出来るようにする

                            'If cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "JOINT") _
                            '    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SHIPORDER") _
                            '    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CARSAMOUNT") _
                            '    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALLODDATE") _
                            '    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGETRAINNO") _
                            '    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDARRSTATIONNAME") _
                            '    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDCONSIGNEENAME") _
                            '    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGERETSTATIONNAME") Then
                            '    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")

                            If cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "JOINT") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SHIPORDER") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALLODDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGETRAINNO") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDARRSTATIONNAME") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDCONSIGNEENAME") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGERETSTATIONNAME") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")


                                '### 20200814 END  ((全体)No123対応) ######################################



                                '★(実績)発日
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALDEPDATE") Then
                                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                Else
                                    If Me.WW_USEORDERFLG = True Then
                                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                    Else
                                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                    End If
                                End If

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                                '★(実績)積車着日
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALARRDATE") Then
                                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                Else
                                    If Me.WW_USEORDERFLG = True Then
                                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                    Else
                                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                    End If
                                End If

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                                '★(実績)受入日
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALACCDATE") Then
                                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                Else
                                    If Me.WW_USEORDERFLG = True Then
                                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                    Else
                                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                    End If
                                End If

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                                '★(実績)空車着日
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALEMPARRDATE") Then
                                If Me.WW_USEORDERFLG = True Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                Else
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                End If

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                            End If

                            '    ### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 ######## 
                            '    Me.WW_USEORDERFLG(TRUE:使用中, FALSE:未使用)
                        ElseIf Me.WW_USEORDERFLG = True Then
                            '◯ 受注営業所が"010402"(仙台新港営業所)の場合
                            If Me.TxtOrderOfficeCode.Text = BaseDllConst.CONST_OFFICECODE_010402 Then
                                If cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "JOINT") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALDEPDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALARRDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALACCDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALEMPARRDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGETRAINNO") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDARRSTATIONNAME") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDCONSIGNEENAME") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGERETSTATIONNAME") Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SHIPORDER") _
                                    AndAlso work.WF_SEL_SHIPORDERCLASS.Text = "2" Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALLODDATE") Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                End If
                            Else
                                If cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "JOINT") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CARSAMOUNT") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALLODDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALDEPDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALARRDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALACCDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALEMPARRDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGETRAINNO") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDARRSTATIONNAME") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDCONSIGNEENAME") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGERETSTATIONNAME") Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SHIPORDER") _
                                    AndAlso work.WF_SEL_SHIPORDERCLASS.Text = "2" Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                End If
                            End If
                            '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 ######## 
                        Else
                            If cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "JOINT") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDARRSTATIONNAME") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDCONSIGNEENAME") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SHIPORDER") _
                                AndAlso work.WF_SEL_SHIPORDERCLASS.Text = "2" Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")

                                '### 20200622 START((全体)No82対応) ######################################
                                '★発送順
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SHIPORDER") _
                                AndAlso (work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                                         OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                                         OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450) Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                '### 20200622 END  ((全体)No82対応) ######################################

                                '★数量(kl)
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CARSAMOUNT") _
                                AndAlso (work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
                                         OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                                         OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                                         OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450) Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                                '★(実績)積込日
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALLODDATE") Then
                                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
                                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                Else
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                End If

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                                '★(実績)発日
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALDEPDATE") Then
                                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
                                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                Else
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                End If

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                                '★(実績)積車着日
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALARRDATE") Then
                                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
                                OrElse work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                Else
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                End If

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                                '★(実績)受入日
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALACCDATE") Then
                                If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                                Else
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                End If

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                                '★(実績)空車着日
                            ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALEMPARRDATE") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")

                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False

                            End If
                        End If
                    Next
                    rowIdx += 1
                Next

            Case Else
                For Each rowitem As TableRow In tblObj.Rows
                    '画面表示行が存在している場合
                    If OIT0003tbl_tab3.Rows.Count <> 0 Then
                        loopdr = OIT0003tbl_tab3.Rows(rowIdx)
                        chkObjSTId = chkObjIdWOSTcnt & Convert.ToString(loopdr("LINECNT"))
                        chkObjFRId = chkObjIdWOFRcnt & Convert.ToString(loopdr("LINECNT"))
                        chkObjAFId = chkObjIdWOAFcnt & Convert.ToString(loopdr("LINECNT"))
                        chkObjOTId = chkObjIdWOOTcnt & Convert.ToString(loopdr("LINECNT"))
                        '下のループより先に見つけなければいけないかもしれないので
                        '冗長ですがこちらでループ
                        chkObjST = Nothing
                        For Each cellObj As TableCell In rowitem.Controls
                            chkObjST = DirectCast(cellObj.FindControl(chkObjSTId), CheckBox)
                            'コントロールが見つかったら脱出
                            If chkObjST IsNot Nothing Then
                                Exit For
                            End If
                        Next
                        chkObjFR = Nothing
                        For Each cellObj As TableCell In rowitem.Controls
                            chkObjFR = DirectCast(cellObj.FindControl(chkObjFRId), CheckBox)
                            'コントロールが見つかったら脱出
                            If chkObjFR IsNot Nothing Then
                                Exit For
                            End If
                        Next
                        '### 20200622 START((全体)No87対応) ######################################
                        chkObjAF = Nothing
                        For Each cellObj As TableCell In rowitem.Controls
                            chkObjAF = DirectCast(cellObj.FindControl(chkObjAFId), CheckBox)
                            'コントロールが見つかったら脱出
                            If chkObjAF IsNot Nothing Then
                                Exit For
                            End If
                        Next
                        '### 20200622 END  ((全体)No87対応) ######################################
                        '### 20200717 START((全体)No112対応) ######################################
                        chkObjOT = Nothing
                        For Each cellObj As TableCell In rowitem.Controls
                            chkObjOT = DirectCast(cellObj.FindControl(chkObjOTId), CheckBox)
                            'コントロールが見つかったら脱出
                            If chkObjOT IsNot Nothing Then
                                Exit For
                            End If
                        Next
                        '### 20200717 END  ((全体)No112対応) ######################################
                        '積込可否フラグ(チェックボックス)を非活性
                        chkObjST.Enabled = False
                        '先返し可否フラグ(チェックボックス)を非活性
                        chkObjFR.Enabled = False
                        '後返し可否フラグ(チェックボックス)を非活性
                        chkObjAF.Enabled = False
                        'OT輸送可否フラグ(チェックボックス)を非活性
                        chkObjOT.Enabled = False
                    End If
                    For Each cellObj As TableCell In rowitem.Controls
                        If cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "JOINT") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SHIPORDER") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CARSAMOUNT") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALLODDATE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALDEPDATE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALARRDATE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALACCDATE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "ACTUALEMPARRDATE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGETRAINNO") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDARRSTATIONNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "SECONDCONSIGNEENAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea3.ID & "CHANGERETSTATIONNAME") Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        End If
                    Next
                    rowIdx += 1
                Next
        End Select
    End Sub

    ''' <summary>
    ''' タブ(費用入力)
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControlTab4()
        Dim divObj = DirectCast(pnlListArea4.FindControl(pnlListArea4.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)
        Dim chkObj As CheckBox = Nothing
        'LINECNTを除いたチェックボックスID
        Dim chkObjIdWOLincnt As String = "chk" & pnlListArea4.ID & "OPERATION"
        'LINECNTを含むチェックボックスID
        Dim chkObjId As String
        Dim chkObjType As String
        'ループ内の対象データROW(これで計算区分の値をとれるかと）
        Dim loopdr As DataRow = Nothing
        'データテーブルの行Index
        Dim rowIdx As Integer = 0

        For Each rowitem As TableRow In tblObj.Rows
            '画面表示行が存在している場合
            If OIT0003tbl_tab4.Rows.Count <> 0 Then
                loopdr = OIT0003tbl_tab4.Rows(rowIdx)
                chkObjId = chkObjIdWOLincnt & Convert.ToString(loopdr("LINECNT"))
                chkObjType = Convert.ToString(loopdr("CALCACCOUNT"))
                '下のループより先に見つけなければいけないかもしれないので
                '冗長ですがこちらでループ
                chkObj = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObj = DirectCast(cellObj.FindControl(chkObjId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObj IsNot Nothing Then
                        Exit For
                    End If
                Next

                '★自動計算科目は入力は不可
                If chkObjType = "1" Then
                    'チェック時処理(もちろん↓のセルループにも入れてOK）
                    chkObj.Enabled = False
                    For Each cellObj As TableCell In rowitem.Controls
                        If cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "KEIJYOYM") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "ACCSEGCODE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "BREAKDOWN") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "APPLYCHARGESUM") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "INVOICECODE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "INVOICEDEPTNAME") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "PAYEECODE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "PAYEEDEPTNAME") Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        End If
                    Next
                    '★追加科目は入力可能
                Else
                    chkObj.Enabled = True
                    For Each cellObj As TableCell In rowitem.Controls
                        If cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "KEIJYOYM") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "ACCSEGCODE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "INVOICECODE") _
                            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea4.ID & "PAYEECODE") Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        End If
                    Next
                End If
            End If

            rowIdx += 1
        Next
    End Sub

    ''' <summary>
    ''' 受注履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    Private Sub WW_InsertOrderHistory(ByVal SQLcon As SqlConnection)
        Dim WW_GetHistoryNo() As String = {""}
        WW_FixvalueMasterSearch("", "NEWHISTORYNOGET", "", WW_GetHistoryNo)

        '◯受注履歴テーブル格納用
        If IsNothing(OIT0003His1tbl) Then
            OIT0003His1tbl = New DataTable
        End If

        If OIT0003His1tbl.Columns.Count <> 0 Then
            OIT0003His1tbl.Columns.Clear()
        End If
        OIT0003His1tbl.Clear()

        '◯受注明細履歴テーブル格納用
        If IsNothing(OIT0003His2tbl) Then
            OIT0003His2tbl = New DataTable
        End If

        If OIT0003His2tbl.Columns.Count <> 0 Then
            OIT0003His2tbl.Columns.Clear()
        End If
        OIT0003His2tbl.Clear()

        '○ 受注TBL検索SQL
        Dim SQLOrderStr As String =
            "SELECT " _
            & String.Format("   '{0}' AS HISTORYNO", WW_GetHistoryNo(0)) _
            & String.Format(" , '{0}' AS MAPID", Me.Title) _
            & " , OIT0002.*" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & String.Format(" WHERE OIT0002.ORDERNO = '{0}'", work.WF_SEL_ORDERNUMBER.Text)

        '○ 受注明細TBL検索SQL
        Dim SQLOrderDetailStr As String =
            "SELECT " _
            & String.Format("   '{0}' AS HISTORYNO", WW_GetHistoryNo(0)) _
            & String.Format(" , '{0}' AS MAPID", Me.Title) _
            & " , OIT0003.*" _
            & " FROM OIL.OIT0003_DETAIL OIT0003 " _
            & String.Format(" WHERE OIT0003.ORDERNO = '{0}'", work.WF_SEL_ORDERNUMBER.Text)

        Try
            Using SQLcmd As New SqlCommand(SQLOrderStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003His1tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003His1tbl.Load(SQLdr)
                End Using
            End Using

            Using SQLcmd As New SqlCommand(SQLOrderDetailStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003His2tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003His2tbl.Load(SQLdr)
                End Using
            End Using

            Using tran = SQLcon.BeginTransaction
                '■受注履歴テーブル
                EntryHistory.InsertOrderHistory(SQLcon, tran, OIT0003His1tbl.Rows(0))

                '■受注明細履歴テーブル
                For Each OIT0001His2rowtbl In OIT0003His2tbl.Rows
                    EntryHistory.InsertOrderDetailHistory(SQLcon, tran, OIT0001His2rowtbl)
                Next

                'トランザクションコミット
                tran.Commit()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D ORDERHISTORY")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D ORDERHISTORY"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

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
'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0006OutOfServiceDetail
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0006tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0006tbl_tab1 As DataTable                            '一覧格納用テーブル(タブ１用)
    Private OIT0006tbl_tab2 As DataTable                            '一覧格納用テーブル(タブ２用)
    Private OIT0006tbl_tab3 As DataTable                            '一覧格納用テーブル(タブ３用)
    Private OIT0006tbl_tab4 As DataTable                            '一覧格納用テーブル(タブ４用)
    Private OIT0006INPtbl As DataTable                              'チェック用テーブル
    Private OIT0006UPDtbl As DataTable                              '更新用テーブル
    Private OIT0006WKtbl As DataTable                               '作業用テーブル
    Private OIT0006WK2tbl As DataTable                              '作業用2テーブル
    Private OIT0006WK3tbl As DataTable                              '作業用3テーブル
    Private OIT0006WK4tbl As DataTable                              '作業用4テーブル
    Private OIT0006WK5tbl As DataTable                              '作業用4テーブル
    Private OIT0006Fixvaltbl As DataTable                           '作業用テーブル(固定値マスタ取得用)
    Private OIT0006His1tbl As DataTable                             '履歴格納用テーブル
    Private OIT0006His2tbl As DataTable                             '履歴格納用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 99                '1画面表示用
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
    Private WW_TAB1_SW As String = ""
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
                    Master.RecoverTable(OIT0006tbl)
                    'Master.RecoverTable(OIT0006tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

                    '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                    If CS0013ProfView.SetDispListTextBoxValues(OIT0006tbl, pnlListArea1) Then
                        Master.SaveTable(OIT0006tbl)
                    End If
                    'If CS0013ProfView.SetDispListTextBoxValues(OIT0006tbl_tab2, pnlListArea2) Then
                    '    Master.SaveTable(OIT0006tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)
                    'End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonDELIVERY"              '託送指示ボタン押下
                            WF_ButtonDELIVERY_Click()
                        Case "WF_ButtonINSERT"                '明細を作るボタン押下
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
                             "WF_ButtonALLSELECT_TAB2"
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED_TAB1",   '選択解除ボタン押下
                             "WF_ButtonSELECT_LIFTED_TAB2"
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonLINE_LIFTED_TAB1",     '行削除ボタン押下
                             "WF_ButtonLINE_LIFTED_TAB2"
                            WF_ButtonLINE_LIFTED_Click()
                        Case "WF_ButtonLINE_ADD_TAB1",        '行追加ボタン押下
                             "WF_ButtonLINE_ADD_TAB2"
                            WF_ButtonLINE_ADD_Click()
                        Case "WF_ButtonUPDATE_TAB1",          '更新ボタン押下
                             "WF_ButtonUPDATE_TAB2"
                            WW_TAB1_SW = "1"
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonUPDATE_MEISAI_TAB1"    '明細更新ボタン押下
                            WW_TAB1_SW = "2"
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
                            'Case "btnChkLastOilConfirmYes"        '確認メッセージはいボタン押下(前回油種チェック)
                            '    '画面表示設定処理(受注進行ステータス)
                            '    WW_ScreenOrderStatusSet()
                            'Case "btnChkLastOilConfirmNo"         '確認メッセージいいえボタン押下(前回油種チェック)
                            '    '### 特になし ###########
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

            '◯託送指示フラグ(0：未手配, 1：手配)設定
            '　または、回送進行ステータスが100:回送受付, または250:手配完了以降のステータスに変更された場合
            If work.WF_SEL_DELIVERYFLG.Text = "1" _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_100 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_250 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_300 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_350 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_400 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_450 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_500 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_550 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_600 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_700 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_800 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_900 Then

                '託送指示ボタンを非活性
                WF_DELIVERYFLG.Value = "1"

            Else
                '託送指示ボタンを活性
                WF_DELIVERYFLG.Value = "0"

            End If

            '◯回送進行ステータスが250:手配完了以降のステータスに変更された場合
            If work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_250 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_300 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_350 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_400 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_450 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_500 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_550 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_600 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_700 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_800 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_900 Then

                'タブ「タンク車割当」のボタンをすべて非活性
                WF_MAPButtonControl.Value = "1"

            End If

        Finally
            '○ 格納Table Close
            If Not IsNothing(OIT0006tbl) Then
                OIT0006tbl.Clear()
                OIT0006tbl.Dispose()
                OIT0006tbl = Nothing
            End If

            If Not IsNothing(OIT0006INPtbl) Then
                OIT0006INPtbl.Clear()
                OIT0006INPtbl.Dispose()
                OIT0006INPtbl = Nothing
            End If

            If Not IsNothing(OIT0006UPDtbl) Then
                OIT0006UPDtbl.Clear()
                OIT0006UPDtbl.Dispose()
                OIT0006UPDtbl = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0006WRKINC.MAPIDD
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.MENU Then
            'Master.MAPID = OIT0006WRKINC.MAPIDD
            work.WF_SEL_MAPIDBACKUP.Text = OIT0006WRKINC.MAPIDD
        Else
            'Master.MAPID = OIT0006WRKINC.MAPIDD + "MAIN"
            work.WF_SEL_MAPIDBACKUP.Text = OIT0006WRKINC.MAPIDD + "MAIN"
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
        '〇 回送進行ステータスが"100:回送受付"～"500：検収中"の場合
        If work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_100 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_200 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_210 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_250 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_300 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_350 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_400 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_450 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_500 Then

            WF_DTAB_CHANGE_NO.Value = "0"
            WF_DetailMView.ActiveViewIndex = WF_DTAB_CHANGE_NO.Value

            ' 回送進行ステータスが"100:回送受付"以外の場合
            If work.WF_SEL_KAISOUSTATUS.Text <> BaseDllConst.CONST_KAISOUSTATUS_100 Then
                '〇 (一覧)テキストボックスの制御(読取専用)
                WW_ListTextBoxReadControl()

            End If

        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_550 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_600 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_700 _
                OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_800 Then

            WF_DTAB_CHANGE_NO.Value = "1"
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

        '■オーダー№
        If work.WF_SEL_KAISOUNUMBER.Text = "" Then
            Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
            WW_FixvalueMasterSearch("", "NEWKAISOUNOGET", "", WW_GetValue)
            work.WF_SEL_KAISOUNUMBER.Text = WW_GetValue(0)
            Me.TxtKaisouOrderNo.Text = work.WF_SEL_KAISOUNUMBER.Text
        Else
            Me.TxtKaisouOrderNo.Text = work.WF_SEL_KAISOUNUMBER.Text
        End If

        '■ステータス
        If work.WF_SEL_KAISOUSTATUSNM.Text = "" Then
            work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_100
            CODENAME_get("KAISOUSTATUS", BaseDllConst.CONST_KAISOUSTATUS_100, work.WF_SEL_KAISOUSTATUSNM.Text, WW_DUMMY)
        End If
        Me.TxtKaisouStatus.Text = work.WF_SEL_KAISOUSTATUSNM.Text

        '■回送営業所
        '作成モード(２：更新)
        If work.WF_SEL_CREATEFLG.Text = "2" Then
            Me.TxtKaisouOrderOffice.Text = work.WF_SEL_KAISOUSALESOFFICE.Text
            Me.TxtKaisouOrderOfficeCode.Text = work.WF_SEL_KAISOUSALESOFFICECODE.Text

            '作成モード(１：新規登録)
        Else
            Me.TxtKaisouOrderOffice.Text = work.WF_SEL_SALESOFFICE.Text
            Me.TxtKaisouOrderOfficeCode.Text = work.WF_SEL_SALESOFFICECODE.Text

        End If

        '■本線列車
        Me.TxtTrainNo.Text = work.WF_SEL_TRAIN.Text
        Me.TxtTrainName.Text = work.WF_SEL_TRAINNAME.Text

        '■目的
        Me.TxtObjective.Text = work.WF_SEL_OBJECTIVECODE.Text
        Me.LblObjective.Text = work.WF_SEL_OBJECTIVENAME.Text

        '■タンク車数
        Me.TxtTankCnt.Text = work.WF_SEL_TANKCARTOTAL.Text

        '■回送パターン
        Me.TxtKaisouTypeCode.Text = work.WF_SEL_PATTERNCODE.Text
        CODENAME_get("KAISOUPATTERN", work.WF_SEL_PATTERNCODE.Text, work.WF_SEL_PATTERNNAME.Text, WW_DUMMY)
        Me.TxtKaisouType.Text = work.WF_SEL_PATTERNNAME.Text

        '■運賃フラグ(1:片道 2:往復)
        If work.WF_SEL_FAREFLG.Text = "1" Then
            Me.ChkSelect.Checked = True
        Else
            Me.ChkSelect.Checked = False
        End If

        '■発駅
        Me.TxtDepstationCode.Text = work.WF_SEL_DEPARTURESTATION.Text
        '■着駅
        Me.TxtArrstationCode.Text = work.WF_SEL_ARRIVALSTATION.Text

        '■(予定)発日
        Me.TxtDepDate.Text = work.WF_SEL_DEPDATE.Text
        '■(予定)積車着日
        Me.TxtArrDate.Text = work.WF_SEL_ARRDATE.Text
        '■(予定)受入日
        Me.TxtAccDate.Text = work.WF_SEL_ACCDATE.Text
        '■(予定)空車着日
        Me.TxtEmparrDate.Text = work.WF_SEL_EMPARRDATE.Text

        '■(実績)発日
        Me.TxtActualDepDate.Text = work.WF_SEL_ACTUALDEPDATE.Text
        '■(実績)積車着日
        Me.TxtActualArrDate.Text = work.WF_SEL_ACTUALARRDATE.Text
        '■(実績)受入日
        Me.TxtActualAccDate.Text = work.WF_SEL_ACTUALACCDATE.Text
        '■(実績)空車着日
        Me.TxtActualEmparrDate.Text = work.WF_SEL_ACTUALEMPARRDATE.Text

        '本線列車・目的・タンク車数・発駅・着駅を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtTrainNo.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtObjective.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTankCnt.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtDepstationCode.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtArrstationCode.Attributes("onkeyPress") = "CheckNum()"

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", work.WF_SEL_UORG.Text, Me.WF_UORG_TEXT.Text, WW_DUMMY)
        '発駅
        CODENAME_get("DEPSTATION", Me.TxtDepstationCode.Text, Me.LblDepstationName.Text, WW_DUMMY)
        '着駅
        CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_DUMMY)

    End Sub

#Region "GridViewデータ設定"
    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        'メニュー画面からの遷移の場合
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            '作成フラグ(新規登録：1, 更新：2)
            work.WF_SEL_CREATEFLG.Text = "1"

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

        '〇タブ「費用入力」表示用
        GridViewInitializeTab2()

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

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0006tbl)

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
    ''' GridViewデータ設定(タブ「費用入力」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab2()

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection, ByVal O_INSCNT As Integer)

        If IsNothing(OIT0006tbl) Then
            OIT0006tbl = New DataTable
        End If

        If OIT0006tbl.Columns.Count <> 0 Then
            OIT0006tbl.Columns.Clear()
        End If

        OIT0006tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを回送テーブルから取得する
        Dim SQLStr As String = ""
        Dim SQLTempTblStr As String = ""

        '新規登録ボタン押下
        If work.WF_SEL_CREATEFLG.Text = "1" Then
            SQLStr =
              " SELECT TOP (@P00)" _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , ''                                             AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , @P01                                           AS KAISOUNO" _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS DETAILNO" _
            & " , @P12                                           AS SHIPPERSCODE" _
            & " , @P13                                           AS SHIPPERSNAME" _
            & " , @P14                                           AS BASECODE" _
            & " , @P15                                           AS BASENAME" _
            & " , @P16                                           AS CONSIGNEECODE" _
            & " , @P17                                           AS CONSIGNEENAME" _
            & " , ''                                             AS KAISOUINFO" _
            & " , ''                                             AS KAISOUINFONAME" _
            & " , ''                                             AS ORDERNO" _
            & " , ''                                             AS SHIPORDER" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS ACTUALDEPDATE" _
            & " , ''                                             AS ACTUALARRDATE" _
            & " , ''                                             AS ACTUALACCDATE" _
            & " , ''                                             AS ACTUALEMPARRDATE" _
            & " , ''                                             AS REMARK" _
            & " , '0'                                            AS DELFLG" _
            & " FROM sys.all_objects "

            '明細データダブルクリック
        ElseIf work.WF_SEL_CREATEFLG.Text = "2" Then
            SQLStr =
                  " SELECT" _
                & "   0                                                  AS LINECNT" _
                & " , ''                                                 AS OPERATION" _
                & " , CAST(OIT0006.UPDTIMSTP AS bigint)                  AS TIMSTP" _
                & " , 1                                                  AS 'SELECT'" _
                & " , 0                                                  AS HIDDEN" _
                & " , ISNULL(RTRIM(OIT0006.KAISOUNO), '')                AS KAISOUNO" _
                & " , ISNULL(RTRIM(OIT0007.DETAILNO), '')                AS DETAILNO" _
                & " , ISNULL(RTRIM(OIT0006.SHIPPERSCODE), '')            AS SHIPPERSCODE" _
                & " , ISNULL(RTRIM(OIT0006.SHIPPERSNAME), '')            AS SHIPPERSNAME" _
                & " , ISNULL(RTRIM(OIT0006.BASECODE), '')                AS BASECODE" _
                & " , ISNULL(RTRIM(OIT0006.BASENAME), '')                AS BASENAME" _
                & " , ISNULL(RTRIM(OIT0006.CONSIGNEECODE), '')           AS CONSIGNEECODE" _
                & " , ISNULL(RTRIM(OIT0006.CONSIGNEENAME), '')           AS CONSIGNEENAME" _
                & " , ISNULL(RTRIM(OIT0007.KAISOUINFO), '')              AS KAISOUINFO" _
                & " , ''                                                 AS KAISOUINFONAME" _
                & " , ISNULL(RTRIM(OIT0006.ORDERNO), '')                 AS ORDERNO" _
                & " , ISNULL(RTRIM(OIT0007.SHIPORDER), '')               AS SHIPORDER" _
                & " , ISNULL(RTRIM(OIT0007.TANKNO), '')                  AS TANKNO" _
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
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN '" + C_INSPECTIONALERT.ALERT_RED + "'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
                & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN '" + C_INSPECTIONALERT.ALERT_YELLOW + "'" _
                & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN '" + C_INSPECTIONALERT.ALERT_GREEN + "'" _
                & "   END                                                           AS JRALLINSPECTIONALERTSTR" _
                & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), NULL) AS JRALLINSPECTIONDATE" _
                & " , ISNULL(FORMAT(OIT0007.ACTUALDEPDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALDEPDATE" _
                & " , ISNULL(FORMAT(OIT0007.ACTUALARRDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALARRDATE" _
                & " , ISNULL(FORMAT(OIT0007.ACTUALACCDATE, 'yyyy/MM/dd'), NULL)     AS ACTUALACCDATE" _
                & " , ISNULL(FORMAT(OIT0007.ACTUALEMPARRDATE, 'yyyy/MM/dd'), NULL)  AS ACTUALEMPARRDATE" _
                & " , ISNULL(RTRIM(OIT0007.REMARK), '')                             AS REMARK" _
                & " , ISNULL(RTRIM(OIT0006.DELFLG), '')                             AS DELFLG" _
                & " FROM OIL.OIT0006_KAISOU OIT0006 " _
                & " INNER JOIN OIL.OIT0007_KAISOUDETAIL OIT0007 ON " _
                & "       OIT0007.KAISOUNO = OIT0006.KAISOUNO" _
                & "       AND OIT0007.DELFLG <> @P02" _
                & " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
                & "       OIT0007.TANKNO = OIT0005.TANKNUMBER" _
                & "       AND OIT0005.DELFLG <> @P02" _
                & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
                & "       OIT0007.TANKNO = OIM0005.TANKNUMBER" _
                & "       AND OIM0005.DELFLG <> @P02" _
                & " LEFT JOIN com.OIS0015_FIXVALUE OIS0015_2 ON " _
                & "       OIS0015_2.CLASS   = 'KAISOUINFO' " _
                & "       AND OIS0015_2.KEYCODE = OIT0007.KAISOUINFO " _
                & " WHERE OIT0006.KAISOUNO = @P01" _
                & " AND OIT0006.DELFLG <> @P02"

        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLTMPcmd As New SqlCommand(SQLTempTblStr, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.Int)          '明細数(新規作成)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '回送№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ

                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 10)  '荷主コード
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 40)  '荷主名
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 9)   '基地コード
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 40)  '基地名
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 10)  '荷受人コード
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 40)  '荷受人名

                PARA00.Value = O_INSCNT
                PARA01.Value = work.WF_SEL_KAISOUNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                PARA12.Value = work.WF_SEL_SHIPPERSCODE.Text
                PARA13.Value = work.WF_SEL_SHIPPERSNAME.Text
                PARA14.Value = work.WF_SEL_BASECODE.Text
                PARA15.Value = work.WF_SEL_BASENAME.Text
                PARA16.Value = work.WF_SEL_CONSIGNEECODE.Text
                PARA17.Value = work.WF_SEL_CONSIGNEENAME.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0006tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0006row As DataRow In OIT0006tbl.Rows
                    i += 1
                    OIT0006row("LINECNT") = i        'LINECNT

                    '◯名称取得
                    '回送情報
                    If OIT0006row("KAISOUINFONAME") = "" Then
                        CODENAME_get("KAISOUINFO", OIT0006row("KAISOUINFO"), OIT0006row("KAISOUINFONAME"), WW_DUMMY)
                        If OIT0006row("KAISOUINFONAME") = "" Then
                            CODENAME_get("ORDERINFO", OIT0006row("KAISOUINFO"), OIT0006row("KAISOUINFONAME"), WW_DUMMY)
                        End If
                    End If

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

#End Region

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
            'Master.RecoverTable(OIT0006tbl_tab1, work.WF_SEL_INPTAB1TBL.Text)
            DisplayGrid_TAB1()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            '○ 画面表示データ復元
            Master.RecoverTable(OIT0006tbl_tab2, work.WF_SEL_INPTAB2TBL.Text)

            DisplayGrid_TAB2()

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
        For Each OIT0006row As DataRow In OIT0006tbl.Rows
            If OIT0006row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0006row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0006tbl)

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
    Protected Sub DisplayGrid_TAB2()
        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0006tab2row As DataRow In OIT0006tbl_tab2.Rows
            If OIT0006tab2row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0006tab2row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0006tbl_tab2)

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
#End Region

    ''' <summary>
    ''' 託送指示ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDELIVERY_Click()

        '託送指示フラグを"1"(手配)にする。
        work.WF_SEL_DELIVERYFLG.Text = "1"

        '回送TBL更新
        WW_UpdateRelatedFlg("1", "DELIVERYFLG")

        '〇 回送進行ステータスの状態を取得
        WW_ScreenKaisouStatusSet()

    End Sub

    ''' <summary>
    ''' 明細を作るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        ''着駅コードが未設定の場合
        ''※一覧を作成するにあたり、基地コード・荷受人を取得するために、
        ''　着駅コードは必須となるため
        'If Me.TxtArrstationCode.Text = "" Then
        '    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "着駅", needsPopUp:=True)
        '    Me.TxtArrstationCode.Focus()
        '    WW_CheckERR("着駅入力エラー。", C_MESSAGE_NO.PREREQUISITE_ERROR)
        '    Exit Sub
        'End If

        ''(予定)発日が未設定の場合
        'If Me.TxtDepDate.Text = "" Then
        '    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)発日", needsPopUp:=True)
        '    Me.TxtArrstationCode.Focus()
        '    WW_CheckERR("(予定)発日入力エラー。", C_MESSAGE_NO.PREREQUISITE_ERROR)
        '    Exit Sub
        'End If

        Dim WW_RESULT As String = ""
        WW_ERRCODE = C_MESSAGE_NO.NORMAL

        '○ 関連チェック
        WW_Check(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, Me.TxtTankCnt.Text)
        End Using

        Dim i As Integer = 0
        For Each OIT0006row As DataRow In OIT0006tbl.Rows
            i += 1
            OIT0006row("SHIPORDER") = i        '発送順

        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

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

        '〇 回送登録営業所チェック
        '回送登録営業所が選択されていない場合は、他の検索(LEFTBOX)は表示させない制御をする
        '※回送登録営業所は他の検索するためのKEYとして使用するため
        If WF_FIELD.Value <> "TxtKaisouOrderOffice" AndAlso Me.TxtKaisouOrderOffice.Text = "" Then
            Master.Output(C_MESSAGE_NO.OIL_KAISOUOFFICE_UNSELECT, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Me.TxtArrstationCode.Focus()
            WW_CheckERR("回送登録営業所が未選択。", C_MESSAGE_NO.OIL_KAISOUOFFICE_UNSELECT)
            WF_LeftboxOpen.Value = ""   'LeftBoxを表示させない
            Me.TxtKaisouOrderOffice.Focus()
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

                    '回送登録営業所
                    If WF_FIELD.Value = "TxtKaisouOrderOffice" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, Me.TxtKaisouOrderOffice.Text)
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, Me.TxtKaisouOrderOffice.Text)
                        End If
                    End If

                    '本線列車
                    If WF_FIELD.Value = "TxtTrainNo" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(回送登録営業所).テキストボックスが未設定
                            If Me.TxtKaisouOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, Me.TxtTrainNo.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(Me.TxtKaisouOrderOfficeCode.Text, Me.TxtTrainNo.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, Me.TxtTrainNo.Text)
                        End If
                    End If

                    '回送パターン
                    If WF_FIELD.Value = "TxtKaisouType" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(回送登録営業所).テキストボックスが未設定
                            If Me.TxtKaisouOrderOffice.Text = "" Then
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, Me.TxtKaisouType.Text)
                            Else
                                prmData = work.CreateSALESOFFICEParam(Me.TxtKaisouOrderOfficeCode.Text, Me.TxtKaisouType.Text)
                            End If
                        Else
                            prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, Me.TxtKaisouType.Text)
                        End If
                    End If

                    '発駅
                    If WF_FIELD.Value = "TxtDepstationCode" Then

                        '### 営業所に関係なくすべての発駅を出力 ########################################################
                        prmData = work.CreateSTATIONPTParam(work.WF_SEL_CAMPCODE.Text + "1", Me.TxtDepstationCode.Text)

                        ''〇 検索(営業所).テキストボックスが未設定
                        'If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                        '    '〇 画面(回送登録営業所).テキストボックスが未設定
                        '    If Me.TxtKaisouOrderOffice.Text = "" Then
                        '        prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "1", Me.TxtDepstationCode.Text)
                        '    Else
                        '        prmData = work.CreateSTATIONPTParam(Me.TxtKaisouOrderOfficeCode.Text + "1", Me.TxtDepstationCode.Text)
                        '    End If
                        'Else
                        '    prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "1", Me.TxtDepstationCode.Text)
                        'End If
                    End If

                    '着駅
                    If WF_FIELD.Value = "TxtArrstationCode" Then

                        '### 営業所に関係なくすべての着駅を出力 ########################################################
                        prmData = work.CreateSTATIONPTParam(work.WF_SEL_CAMPCODE.Text + "2", Me.TxtDepstationCode.Text)

                        ''〇 検索(営業所).テキストボックスが未設定
                        'If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                        '    '〇 画面(回送登録営業所).テキストボックスが未設定
                        '    If Me.TxtKaisouOrderOffice.Text = "" Then
                        '        prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "2", Me.TxtArrstationCode.Text)
                        '    Else
                        '        prmData = work.CreateSTATIONPTParam(Me.TxtKaisouOrderOfficeCode.Text + "2", Me.TxtArrstationCode.Text)
                        '    End If
                        'Else
                        '    prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "2", Me.TxtArrstationCode.Text)
                        'End If
                    End If

                    '(一覧)タンク車№, 
                    If WF_FIELD.Value = "TANKNO" Then
                        ''〇 検索(営業所).テキストボックスが未設定
                        'If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                        '    '〇 画面(回送登録営業所).テキストボックスが未設定
                        '    If Me.TxtKaisouOrderOffice.Text = "" Then
                        '        prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, "")
                        '    Else
                        '        prmData = work.CreateSALESOFFICEParam(Me.TxtKaisouOrderOfficeCode.Text, "")
                        '    End If
                        'Else
                        '    prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
                        'End If

                        prmData = work.CreateSALESOFFICEParam(Me.TxtDepstationCode.Text, "")

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
                        '(予定)発日
                        Case "TxtDepDate"
                            .WF_Calendar.Text = Me.TxtDepDate.Text
                        '(予定)着日
                        Case "TxtArrDate"
                            .WF_Calendar.Text = Me.TxtArrDate.Text
                        '(予定)受入日
                        Case "TxtAccDate"
                            .WF_Calendar.Text = Me.TxtAccDate.Text
                        '(予定)空車着日
                        Case "TxtEmparrDate"
                            .WF_Calendar.Text = Me.TxtEmparrDate.Text
                        '(実績)発日
                        Case "TxtActualDepDate"
                            .WF_Calendar.Text = Me.TxtActualDepDate.Text
                        '(実績)着日
                        Case "TxtActualArrDate"
                            .WF_Calendar.Text = Me.TxtActualArrDate.Text
                        '(実績)受入日
                        Case "TxtActualAccDate"
                            .WF_Calendar.Text = Me.TxtActualAccDate.Text
                        '(実績)空車着日
                        Case "TxtActualEmparrDate"
                            .WF_Calendar.Text = Me.TxtActualEmparrDate.Text
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
        Master.RecoverTable(OIT0006tbl)

        'チェックボックス判定
        For i As Integer = 0 To OIT0006tbl.Rows.Count - 1
            If OIT0006tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If OIT0006tbl.Rows(i)("OPERATION") = "on" Then
                    OIT0006tbl.Rows(i)("OPERATION") = ""
                Else
                    OIT0006tbl.Rows(i)("OPERATION") = "on"
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

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
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '運用部署
            Case "WF_UORG"
                CODENAME_get("UORG", WF_UORG.Text, Me.WF_UORG_TEXT.Text, WW_RTN_SW)

            '本線列車
            Case "TxtTrainNo"

                If Me.TxtTrainNo.Text = "" Then
                    ''発駅
                    'Me.TxtDepstationCode.Text = ""
                    'Me.LblDepstationName.Text = ""
                    ''着駅
                    'Me.TxtArrstationCode.Text = ""
                    'Me.LblArrstationName.Text = ""

                    ''〇 (予定)の日付を設定
                    'Me.TxtDepDate.Text = ""
                    'Me.TxtArrDate.Text = ""
                    'Me.TxtAccDate.Text = ""
                    'Me.TxtEmparrDate.Text = ""

                    'work.WF_SEL_SHIPPERSCODE.Text = ""
                    'work.WF_SEL_SHIPPERSNAME.Text = ""
                    'work.WF_SEL_BASECODE.Text = ""
                    'work.WF_SEL_BASENAME.Text = ""
                    'work.WF_SEL_CONSIGNEECODE.Text = ""
                    'work.WF_SEL_CONSIGNEENAME.Text = ""
                    'work.WF_SEL_PATTERNCODE.Text = ""
                    'work.WF_SEL_PATTERNNAME.Text = ""

                    Exit Select
                End If

                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}

                '〇 検索(営業所).テキストボックスが未設定
                If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                    '〇 画面(回送登録営業所).テキストボックスが未設定
                    If Me.TxtKaisouOrderOffice.Text = "" Then
                        WW_FixvalueMasterSearch(Master.USER_ORG, "TRAINNUMBER", Me.TxtTrainNo.Text, WW_GetValue)
                    Else
                        WW_FixvalueMasterSearch(Me.TxtKaisouOrderOfficeCode.Text, "TRAINNUMBER", Me.TxtTrainNo.Text, WW_GetValue)
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

                ''発駅
                'Me.TxtDepstationCode.Text = WW_GetValue(1)
                'CODENAME_get("DEPSTATION", Me.TxtDepstationCode.Text, Me.LblDepstationName.Text, WW_DUMMY)
                ''着駅
                'Me.TxtArrstationCode.Text = WW_GetValue(2)
                'CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_DUMMY)
                'Me.TxtTrainNo.Focus()

                ''〇 (予定)の日付を設定
                'Me.TxtDepDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                'Me.TxtArrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                'Me.TxtAccDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                'Me.TxtEmparrDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")

                ''〇営業所配下情報を取得・設定
                'WW_GetValue = {"", "", "", "", "", "", "", ""}

                ''〇 検索(営業所).テキストボックスが未設定
                'If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                '    '〇 画面(回送登録営業所).テキストボックスが未設定
                '    If Me.TxtKaisouOrderOffice.Text = "" Then
                '        WW_FixvalueMasterSearch(Master.USER_ORG, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
                '    Else
                '        WW_FixvalueMasterSearch(Me.TxtKaisouOrderOfficeCode.Text, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
                '    End If
                'Else
                '    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", Me.TxtArrstationCode.Text, WW_GetValue)
                'End If

                'work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
                'work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
                'work.WF_SEL_BASECODE.Text = WW_GetValue(2)
                'work.WF_SEL_BASENAME.Text = WW_GetValue(3)
                'work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
                'work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)
                'work.WF_SEL_PATTERNCODE.Text = WW_GetValue(6)
                'work.WF_SEL_PATTERNNAME.Text = WW_GetValue(7)

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

#Region "LeftBox関連操作"
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

            '回送登録営業所
            Case "TxtKaisouOrderOffice"
                '別の回送登録営業所が設定された場合
                If Me.TxtKaisouOrderOffice.Text <> WW_SelectText Then
                    Me.TxtKaisouOrderOffice.Text = WW_SelectText
                    Me.TxtKaisouOrderOfficeCode.Text = WW_SelectValue

                    'work.WF_SEL_SALESOFFICECODE.Text = WW_SelectValue
                    'work.WF_SEL_SALESOFFICE.Text = WW_SelectText
                    work.WF_SEL_KAISOUSALESOFFICECODE.Text = WW_SelectValue
                    work.WF_SEL_KAISOUSALESOFFICE.Text = WW_SelectText

                    '○ テキストボックスを初期化
                    '回送パターン
                    Me.TxtKaisouType.Text = ""
                    '本線列車
                    Me.TxtTrainNo.Text = ""
                    'タンク車数
                    Me.TxtTankCnt.Text = "0"
                    '発駅
                    Me.TxtDepstationCode.Text = ""
                    Me.LblDepstationName.Text = ""
                    '着駅
                    Me.TxtArrstationCode.Text = ""
                    Me.LblArrstationName.Text = ""
                    '(予定)日付
                    Me.TxtDepDate.Text = ""
                    Me.TxtArrDate.Text = ""
                    Me.TxtAccDate.Text = ""
                    Me.TxtEmparrDate.Text = ""

                    '○ 一覧の初期化画面表示データ取得
                    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()       'DataBase接続

                        '######################################################
                        '回送登録営業所を変更した時点で、新規登録と同様の扱いとする。
                        work.WF_SEL_CREATEFLG.Text = "1"
                        '######################################################
                        MAPDataGet(SQLcon, 0)
                    End Using

                    '○ 画面表示データ保存
                    Master.SaveTable(OIT0006tbl)

                End If
                Me.TxtTrainNo.Focus()

            '本線列車
            Case "TxtTrainNo"
                Me.TxtTrainNo.Text = WW_SelectValue
                Me.TxtTrainName.Text = WW_SelectText
                Me.TxtTrainNo.Focus()

            '回送パターン
            Case "TxtKaisouType"
                Me.TxtKaisouTypeCode.Text = WW_SelectValue
                Me.TxtKaisouType.Text = WW_SelectText
                Me.TxtKaisouType.Focus()

                '回送パターンの情報を取得
                WW_FixvalueMasterSearch(Me.WF_CAMPCODE.Text, "KAISOUPATTERN", Me.TxtKaisouTypeCode.Text, WW_GetValue)

                '目的の設定
                Me.TxtObjective.Text = WW_GetValue(2)
                Me.LblObjective.Text = WW_GetValue(3)

                '★下記の回送パターンの場合は着駅(浮島町)を設定する。
                '　01:修理-JOT負担発払
                '　02:修理-JOT負担着払
                '　03:修理-他社負担
                '　04:ＭＣ-JOT負担発払
                '　05:ＭＣ-JOT負担着払
                '　06:ＭＣ-他社負担
                If Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_01 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_02 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_03 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_04 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_05 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_06 Then

                    '着駅(浮島町)
                    Me.TxtArrstationCode.Text = "450704"
                    CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_DUMMY)
                Else
                    Me.TxtArrstationCode.Text = ""
                    Me.LblArrstationName.Text = ""
                End If

                '★下記の回送パターンの場合は片道(チェックボックス)を設定する。
                '　09:疎開留置-JOT負担発払
                '　10:疎開留置-JOT負担着払
                '　11:疎開留置-他社負担
                '　12:移動-JOT負担発払
                '　13:移動-JOT負担着払
                '　14:移動-他社負担
                If Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_09 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_10 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_11 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_12 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_13 _
                    OrElse Me.TxtKaisouType.Text = BaseDllConst.CONST_KAISOUPATTERN_14 Then

                    '片道(チェックボックス)をON
                    Me.ChkSelect.Checked = True
                Else
                    '片道(チェックボックス)をOFF
                    Me.ChkSelect.Checked = False
                End If

            '目的
            Case "TxtObjective"
                Me.TxtObjective.Text = WW_SelectValue
                Me.LblObjective.Text = WW_SelectText
                Me.TxtObjective.Focus()

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

            '(予定)着日
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
                For Each OIT0006tab1row As DataRow In OIT0006tbl.Rows
                    OIT0006tab1row("ACTUALDEPDATE") = Me.TxtActualDepDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0006tbl) Then Exit Sub

            '(実績)着日
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
                For Each OIT0006tab1row As DataRow In OIT0006tbl.Rows
                    OIT0006tab1row("ACTUALARRDATE") = Me.TxtActualArrDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0006tbl) Then Exit Sub

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
                For Each OIT0006tab1row As DataRow In OIT0006tbl.Rows
                    OIT0006tab1row("ACTUALACCDATE") = Me.TxtActualAccDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0006tbl) Then Exit Sub

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
                For Each OIT0006tab1row As DataRow In OIT0006tbl.Rows
                    OIT0006tab1row("ACTUALEMPARRDATE") = Me.TxtActualEmparrDate.Text
                Next
                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0006tbl) Then Exit Sub

            'タブ「タンク車割当」 ⇒　(一覧)タンク車№
            '                   　    (一覧)(実績)発日, (一覧)(実績)積車着日, (一覧)(実績)受入日, (一覧)(実績)空車着日
            Case "TANKNO",
                 "ACTUALDEPDATE", "ACTUALARRDATE", "ACTUALACCDATE", "ACTUALEMPARRDATE"
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
                        If Not Master.RecoverTable(OIT0006tbl) Then Exit Sub

                        '○ 対象ヘッダー取得
                        Dim updHeader = OIT0006tbl.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                        If IsNothing(updHeader) Then Exit Sub

                        '〇 一覧項目へ設定
                        'タンク車№を一覧に設定
                        If WF_FIELD.Value = "TANKNO" Then
                            Dim WW_TANKNUMBER As String = WW_SETVALUE
                            Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                            updHeader.Item(WF_FIELD.Value) = WW_TANKNUMBER

                            ''〇 検索(営業所).テキストボックスが未設定
                            'If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '    '〇 画面(回送登録営業所).テキストボックスが未設定
                            '    If Me.TxtKaisouOrderOffice.Text = "" Then
                            '        WW_FixvalueMasterSearch(Master.USER_ORG, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                            '    Else
                            '        WW_FixvalueMasterSearch(Me.TxtKaisouOrderOfficeCode.Text, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                            '    End If
                            'Else
                            '    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                            'End If

                            WW_FixvalueMasterSearch(Me.TxtDepstationCode.Text, "TANKNUMBER_KAISOU", WW_TANKNUMBER, WW_GetValue)

                            '交検日
                            Dim WW_JRINSPECTIONCNT As String
                            'updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(2)
                            updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(11)
                            If WW_GetValue(11) <> "" Then
                                WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(11)))

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
                                        updHeader.Item("JRINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                                        updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                                    Case "2"
                                        updHeader.Item("JRINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                                        updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                                    Case "3"
                                        updHeader.Item("JRINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                                        updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                                End Select
                            Else
                                updHeader.Item("JRINSPECTIONALERT") = ""
                            End If

                            '全検日
                            Dim WW_JRALLINSPECTIONCNT As String
                            'updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(3)
                            updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(12)
                            If WW_GetValue(12) <> "" Then
                                WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(12)))

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
                                        updHeader.Item("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                                        updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                                    Case "2"
                                        updHeader.Item("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                                        updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                                    Case "3"
                                        updHeader.Item("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                                        updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                                End Select
                            Else
                                updHeader.Item("JRALLINSPECTIONALERT") = ""
                            End If

                            '〇 タンク車割当状況チェック
                            Dim chkTankSts As String = "0"
                            WW_TANKQUOTACHK(WF_FIELD.Value, updHeader, WW_GetValue, chkTankSts)

                            '戻り値が"2"(異常終了)の場合
                            If chkTankSts = "2" Then
                                '★設定した値を初期化する
                                '　タンク車№
                                updHeader.Item("TANKNO") = ""
                                '　交検日
                                updHeader.Item("JRINSPECTIONDATE") = ""
                                updHeader.Item("JRINSPECTIONALERTSTR") = ""
                                updHeader.Item("JRINSPECTIONALERT") = ""
                                '　全検日
                                updHeader.Item("JRALLINSPECTIONDATE") = ""
                                updHeader.Item("JRALLINSPECTIONALERTSTR") = ""
                                updHeader.Item("JRALLINSPECTIONALERT") = ""
                            End If

                            '(一覧)(実績)発日, 　(一覧)(実績)着日, 
                            '(一覧)(実績)受入日, (一覧)(実績)空車着日を一覧に設定
                        ElseIf WF_FIELD.Value = "ACTUALDEPDATE" _
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

                        End If

                        '○ 画面表示データ保存
                        If Not Master.SaveTable(OIT0006tbl) Then Exit Sub

                        '◆費用入力
                    Case 1

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
            Case "TxtTrainNo"                '本線列車
                Me.TxtTrainNo.Focus()
            Case "TxtObjective"              '目的
                Me.TxtObjective.Focus()
            Case "TxtDepstationCode"         '発駅
                Me.TxtDepstationCode.Focus()
            Case "TxtArrstationCode"         '着駅
                Me.TxtArrstationCode.Focus()
            Case "TxtDepDate"                '(予定)発日
                Me.TxtDepDate.Focus()
            Case "TxtArrDate"                '(予定)積車着日
                Me.TxtArrDate.Focus()
            Case "TxtAccDate"                '(予定)受入日
                Me.TxtAccDate.Focus()
            Case "TxtEmparrDate"             '(予定)空車着日
                Me.TxtEmparrDate.Focus()
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
#End Region

#Region "全選択ボタン押下時処理"
    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '〇 選択されたタブ一覧の全解除を制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then
            WW_ButtonALLSELECT_TAB1()

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonALLSELECT_TAB2()

        End If

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB1()
        '○ 画面表示データ復元
        Master.RecoverTable(OIT0006tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0006tbl.Rows.Count - 1
            If OIT0006tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0006tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonALLSELECT_TAB2()

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

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonSELECT_LIFTED_TAB2()

        End If

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB1()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0006tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0006tbl.Rows.Count - 1
            If OIT0006tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0006tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonSELECT_LIFTED_TAB2()

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

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonLINE_LIFTED_TAB2()

        End If

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB1()

        Dim SelectChk As Boolean = False
        Dim intTblCnt As Integer = 0

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0006tbl)

        '■■■ OIT0006tbl関連の回送明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･回送明細を一括論理削除
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0007_KAISOUDETAIL   " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE KAISOUNO    = @P01       " _
                    & "    AND DETAILNO    = @P02       " _
                    & "    AND DELFLG     <> '1'       ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            '件数を取得
            intTblCnt = OIT0006tbl.Rows.Count

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            For Each OIT0006UPDrow In OIT0006tbl.Rows
                If OIT0006UPDrow("OPERATION") = "on" Then

                    If OIT0006UPDrow("LINECNT") < 9000 Then
                        SelectChk = True
                    End If

                    j += 1
                    OIT0006UPDrow("LINECNT") = j        'LINECNT
                    OIT0006UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0006UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0006UPDrow("KAISOUNO")
                    PARA02.Value = OIT0006UPDrow("DETAILNO")

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
                    WW_UpdateTankShozai("", "3", "", I_TANKNO:=OIT0006UPDrow("TANKNO"), I_SITUATION:="1")

                Else
                    i += 1
                    OIT0006UPDrow("LINECNT") = i        'LINECNT
                End If
            Next

            '行削除が1件でも実施された場合
            If SelectChk = True Then
                '発送順に入力している値をクリアする。
                For Each OIT0006UPDrow In OIT0006tbl.Rows
                    OIT0006UPDrow("SHIPORDER") = ""
                Next
            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D_TAB1 DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D_TAB1 DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

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
    ''' 行削除ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB2()

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

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            WW_ButtonLINE_ADD_TAB2()

        End If

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB1()
        If IsNothing(OIT0006WKtbl) Then
            OIT0006WKtbl = New DataTable
        End If

        If OIT0006WKtbl.Columns.Count <> 0 Then
            OIT0006WKtbl.Columns.Clear()
        End If

        OIT0006WKtbl.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Dim SQLStrNum As String

        '○ 作成モード(１：新規登録, ２：更新)設定
        If work.WF_SEL_CREATEFLG.Text = "1" OrElse OIT0006tbl.Rows.Count = 0 Then
            SQLStrNum =
            " SELECT " _
            & "  @P01   AS KAISOUNO" _
            & ", '001'  AS DETAILNO"

        Else
            SQLStrNum =
            " SELECT " _
            & "  ISNULL(OIT0007.KAISOUNO,'')                                    AS KAISOUNO" _
            & ", ISNULL(FORMAT(CONVERT(INT, OIT0007.DETAILNO) + 1,'000'),'000') AS DETAILNO" _
            & " FROM (" _
            & "  SELECT OIT0007.KAISOUNO" _
            & "       , OIT0007.DETAILNO" _
            & "       , ROW_NUMBER() OVER(PARTITION BY OIT0007.KAISOUNO ORDER BY OIT0007.KAISOUNO, OIT0007.DETAILNO DESC) RNUM" _
            & "  FROM OIL.OIT0007_KAISOUDETAIL OIT0007" _
            & "  WHERE OIT0007.KAISOUNO = @P01" _
            & " ) OIT0007 " _
            & " WHERE OIT0007.RNUM = 1"

        End If

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String
        SQLStr =
              " SELECT TOP (1)" _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , @P01                                           AS KAISOUNO" _
            & " , @P08                                           AS DETAILNO" _
            & " , @P02                                           AS SHIPPERSCODE" _
            & " , @P03                                           AS SHIPPERSNAME" _
            & " , @P04                                           AS BASECODE" _
            & " , @P05                                           AS BASENAME" _
            & " , @P06                                           AS CONSIGNEECODE" _
            & " , @P07                                           AS CONSIGNEENAME" _
            & " , ''                                             AS KAISOUINFO" _
            & " , ''                                             AS KAISOUINFONAME" _
            & " , ''                                             AS ORDERNO" _
            & " , ''                                             AS SHIPORDER" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS ACTUALDEPDATE" _
            & " , ''                                             AS ACTUALARRDATE" _
            & " , ''                                             AS ACTUALACCDATE" _
            & " , ''                                             AS ACTUALEMPARRDATE" _
            & " , ''                                             AS REMARK" _
            & " , '0'                                            AS DELFLG" _
            & " FROM sys.all_objects "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)
                Dim PARANUM1 As SqlParameter = SQLcmdNum.Parameters.Add("@P01", SqlDbType.NVarChar) '回送№
                PARANUM1.Value = work.WF_SEL_KAISOUNUMBER.Text

                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0006WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar) '回送№
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar) '回送明細№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar) '荷主コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar) '荷主名
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar) '基地コード
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar) '基地名
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar) '荷受人コード
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar) '荷受人名

                Dim intDetailNo As Integer = 0
                For Each OIT0006WKrow As DataRow In OIT0006WKtbl.Rows
                    intDetailNo = OIT0006WKrow("DETAILNO")

                    PARA1.Value = OIT0006WKrow("KAISOUNO")
                    PARA8.Value = OIT0006WKrow("DETAILNO")
                    PARA2.Value = work.WF_SEL_SHIPPERSCODE.Text
                    PARA3.Value = work.WF_SEL_SHIPPERSNAME.Text
                    PARA4.Value = work.WF_SEL_BASECODE.Text
                    PARA5.Value = work.WF_SEL_BASENAME.Text
                    PARA6.Value = work.WF_SEL_CONSIGNEECODE.Text
                    PARA7.Value = work.WF_SEL_CONSIGNEENAME.Text
                Next

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    OIT0006tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                For Each OIT0006row As DataRow In OIT0006tbl.Rows

                    '行追加データに既存の回送№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[回送№]を利用
                    If OIT0006row("LINECNT") = 0 Then
                        OIT0006row("DETAILNO") = intDetailNo.ToString("000")

                    ElseIf OIT0006row("DETAILNO") >= intDetailNo.ToString("000") Then
                        intDetailNo += 1

                    ElseIf OIT0006row("HIDDEN") = 1 Then
                        intDetailNo += 1

                    End If

                    '削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    If OIT0006row("HIDDEN") = 1 Then
                        j += 1
                        OIT0006row("LINECNT") = j        'LINECNT
                    Else
                        i += 1
                        OIT0006row("LINECNT") = i        'LINECNT
                    End If

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D_TAB1 LINEADD")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D_TAB1 LINEADD"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_ADD_TAB2()

    End Sub
#End Region

#Region "更新ボタン押下時処理"
    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '〇 選択されたタブ一覧の各更新ボタン押下時の制御
        'タブ「タンク車割当」
        If WF_DetailMView.ActiveViewIndex = "0" Then

            If WW_TAB1_SW = "1" Then
                '割当確定ボタン押下時
                WW_ButtonUPDATE_TAB1()

            ElseIf WW_TAB1_SW = "2" Then
                '明細更新ボタン押下時
                WW_ButtonUPDATE_MEISAI_TAB1()

            End If

            'タブ「費用入力」
        ElseIf WF_DetailMView.ActiveViewIndex = "1" Then
            'XXXボタン押下時
            WW_ButtonUPDATE_TAB2()

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
        WW_ERRCODE = C_MESSAGE_NO.NORMAL

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

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            '回送DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateKaisou(SQLcon)
            End Using

            '回送明細DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateKaisouDetail(SQLcon)
            End Using

        End If

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            '回送(一覧)画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_KaisouListTBLSet(SQLcon)
            End Using

        End If

        '★ GridView初期設定
        '○ 画面表示データ再取得(回送(明細)画面表示データ取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            work.WF_SEL_CREATEFLG.Text = 2
            MAPDataGet(SQLcon, 0)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)
        Master.SaveTable(OIT0006tbl, work.WF_SEL_INPTAB1TBL.Text)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '画面表示設定処理(回送進行ステータス)
        WW_ScreenKaisouStatusSet()

    End Sub

    ''' <summary>
    ''' 明細更新ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonUPDATE_MEISAI_TAB1()

        '● 関連チェック
        WW_CheckMeisai(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '● 日付妥当性チェック(実績(日付))
        WW_CheckActualValidityDate(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '〇 回送DB更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UpdateKaisouMeisai(SQLcon)
        End Using

        '〇 回送明細DB更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UpdateKaisouDetailMeisai(SQLcon)
        End Using

        '回送進行ステータス退避用
        Dim strKaisouStatus As String = ""

        '回送進行ステータスの状態
        Select Case work.WF_SEL_KAISOUSTATUS.Text
            '"250:手配完了"
            '"300:回送確定"
            '"350:回送確定(発日入力済み)"
            Case BaseDllConst.CONST_KAISOUSTATUS_250,
                 BaseDllConst.CONST_KAISOUSTATUS_300,
                 BaseDllConst.CONST_KAISOUSTATUS_350

                '(実績)発日の入力が完了(★)
                If Me.TxtActualDepDate.Text <> "" Then
                    strKaisouStatus = BaseDllConst.CONST_KAISOUSTATUS_350
                End If

                '(実績)発日の入力が完了(★)
                'かつ、(実績)着日の入力が完了
                If Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" Then
                    strKaisouStatus = BaseDllConst.CONST_KAISOUSTATUS_400
                End If

                '(実績)発日の入力が完了(★)
                'かつ、(実績)着日の入力が完了
                'かつ、(実績)受入日の入力が完了(★)
                If Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" _
                    AndAlso Me.TxtActualAccDate.Text <> "" Then
                    strKaisouStatus = BaseDllConst.CONST_KAISOUSTATUS_450
                End If

                '(実績)発日の入力が完了(★)
                'かつ、(実績)着日の入力が完了
                'かつ、(実績)受入日の入力が完了(★)
                'かつ、(実績)空車着日の入力が完了
                If Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" _
                    AndAlso Me.TxtActualAccDate.Text <> "" _
                    AndAlso Me.TxtActualEmparrDate.Text <> "" Then
                    strKaisouStatus = BaseDllConst.CONST_KAISOUSTATUS_500

                    '### 目的が"24:疎開留置", "25:移動"の場合は、受入日の入力を省略する ###########
                ElseIf Me.TxtActualDepDate.Text <> "" _
                    AndAlso Me.TxtActualArrDate.Text <> "" _
                    AndAlso (Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
                                OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25) _
                    AndAlso Me.TxtActualEmparrDate.Text <> "" Then
                    strKaisouStatus = BaseDllConst.CONST_KAISOUSTATUS_500
                    '############################################################################

                End If

            '"400:受入確認中"
            Case BaseDllConst.CONST_KAISOUSTATUS_400,
                 BaseDllConst.CONST_KAISOUSTATUS_450

                '(実績)受入日の入力が完了(★)
                If Me.TxtActualAccDate.Text <> "" Then
                    strKaisouStatus = BaseDllConst.CONST_KAISOUSTATUS_450
                End If

                '(実績)受入日の入力が完了(★)
                'かつ、(実績)空車着日の入力が完了
                If Me.TxtActualAccDate.Text <> "" _
                    AndAlso Me.TxtActualEmparrDate.Text <> "" Then
                    strKaisouStatus = BaseDllConst.CONST_KAISOUSTATUS_500

                    '### 目的が"5:疎開留置", "6:移動"の場合は、受入日の入力を省略する ###########
                ElseIf (Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
                                OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25) _
                    AndAlso Me.TxtActualEmparrDate.Text <> "" Then
                    strKaisouStatus = BaseDllConst.CONST_KAISOUSTATUS_500
                    '############################################################################

                End If

            '"500:検収中"
            Case BaseDllConst.CONST_KAISOUSTATUS_500

                '### 特に何もしない ################

        End Select

        '回送進行ステータスに変更があった場合
        If strKaisouStatus <> "" Then
            WW_ScreenKaisouStatusChgRef(strKaisouStatus)

        End If

        '◎ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, 0)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

    End Sub

    ''' <summary>
    ''' XXXボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ButtonUPDATE_TAB2()

    End Sub
#End Region

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

                '費用入力
            Case 1
                WW_ListChange_TAB2()

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

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
        Dim updHeader = OIT0006tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea1.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "TANKNO"            '(一覧)タンク車№
                '入力が空の場合は、対象項目を空文字で設定する。
                If WW_ListValue = "" Then
                    'タンク車№
                    updHeader.Item("TANKNO") = ""
                    ''型式
                    'updHeader.Item("MODEL") = ""
                    '交検日
                    updHeader.Item("JRINSPECTIONDATE") = ""
                    updHeader.Item("JRINSPECTIONALERT") = ""
                    updHeader.Item("JRINSPECTIONALERTSTR") = ""
                    '全検日
                    updHeader.Item("JRALLINSPECTIONDATE") = ""
                    updHeader.Item("JRALLINSPECTIONALERT") = ""
                    updHeader.Item("JRALLINSPECTIONALERTSTR") = ""

                    '〇 タンク車割当状況チェック
                    'WW_TANKQUOTACHK(WF_FIELD.Value, updHeader)

                    Exit Select
                End If

                ''〇 検索(営業所).テキストボックスが未設定
                'If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                '    '〇 画面(受注営業所).テキストボックスが未設定
                '    If Me.TxtKaisouOrderOffice.Text = "" Then
                '        WW_FixvalueMasterSearch(Master.USER_ORG, "TANKNUMBER", WW_ListValue, WW_GetValue)
                '    Else
                '        WW_FixvalueMasterSearch(Me.TxtKaisouOrderOfficeCode.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)
                '    End If
                'Else
                '    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)
                'End If

                WW_FixvalueMasterSearch(Me.TxtDepstationCode.Text, "TANKNUMBER_KAISOU", WW_ListValue, WW_GetValue)

                'タンク車№
                updHeader.Item("TANKNO") = WW_ListValue
                ''型式
                'updHeader.Item("MODEL") = WW_GetValue(7)

                '交検日
                Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                Dim WW_JRINSPECTIONCNT As String
                'updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(2)
                updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(11)
                If WW_GetValue(11) <> "" Then
                    WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(11)))

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
                            updHeader.Item("JRINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                            updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                        Case "2"
                            updHeader.Item("JRINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                            updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                        Case "3"
                            updHeader.Item("JRINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                            updHeader.Item("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                    End Select
                Else
                    updHeader.Item("JRINSPECTIONALERT") = ""
                    updHeader.Item("JRINSPECTIONALERTSTR") = ""
                End If

                '全検日
                Dim WW_JRALLINSPECTIONCNT As String
                'updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(3)
                updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(12)
                If WW_GetValue(12) <> "" Then
                    WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(12)))

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
                            updHeader.Item("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                            updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                        Case "2"
                            updHeader.Item("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                            updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                        Case "3"
                            updHeader.Item("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                            updHeader.Item("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                    End Select
                Else
                    updHeader.Item("JRALLINSPECTIONALERT") = ""
                    updHeader.Item("JRALLINSPECTIONALERTSTR") = ""
                End If

                '〇 タンク車割当状況チェック
                Dim chkTankSts As String = "0"
                WW_TANKQUOTACHK(WF_FIELD.Value, updHeader, WW_GetValue, chkTankSts)

                '戻り値が"2"(異常終了)の場合
                If chkTankSts = "2" Then
                    '★設定した値を初期化する
                    '　タンク車№
                    updHeader.Item("TANKNO") = ""
                    '　交検日
                    updHeader.Item("JRINSPECTIONDATE") = ""
                    updHeader.Item("JRINSPECTIONALERTSTR") = ""
                    updHeader.Item("JRINSPECTIONALERT") = ""
                    '　全検日
                    updHeader.Item("JRALLINSPECTIONDATE") = ""
                    updHeader.Item("JRALLINSPECTIONALERTSTR") = ""
                    updHeader.Item("JRALLINSPECTIONALERT") = ""
                End If

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

    End Sub

    ''' <summary>
    ''' リスト変更時処理(タブ「費用入力」)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListChange_TAB2()

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
        '費用入力
        WF_Dtab02.CssClass = ""

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                'タンク車割当
                WF_Dtab01.CssClass = "selected"

            Case 1
                '費用入力
                WF_Dtab02.CssClass = "selected"

        End Select
    End Sub

#Region "タブ「タンク車割当」各テーブル追加・更新"
    ''' <summary>
    ''' 回送TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateKaisou(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0006_KAISOU" _
            & "    WHERE" _
            & "        KAISOUNO        = @P01" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0006_KAISOU" _
            & "    SET" _
            & "        KAISOUTYPE      = @P02  , TRAINNO         = @P03, TRAINNAME     = @P04" _
            & "        , KAISOUYMD     = @P05  , OFFICECODE      = @P06, OFFICENAME    = @P07" _
            & "        , SHIPPERSCODE  = @P08  , SHIPPERSNAME    = @P09, BASECODE      = @P10, BASENAME         = @P11" _
            & "        , CONSIGNEECODE = @P12  , CONSIGNEENAME   = @P13, DEPSTATION    = @P14, DEPSTATIONNAME   = @P15" _
            & "        , ARRSTATION    = @P16  , ARRSTATIONNAME  = @P17, OBJECTIVECODE = @P18" _
            & "        , KAISOUSTATUS  = @P19  , KAISOUINFO      = @P20" _
            & "        , FAREFLG       = @P21  , USEPROPRIETYFLG = @P22, DELIVERYFLG   = @P48" _
            & "        , DEPDATE       = @P23  , ARRDATE         = @P24, ACCDATE       = @P25, EMPARRDATE       = @P26" _
            & "        , ACTUALDEPDATE = @P27  , ACTUALARRDATE   = @P28, ACTUALACCDATE = @P29, ACTUALEMPARRDATE = @P30" _
            & "        , TOTALTANK     = @P31  , ORDERNO         = @P32, KEIJYOYMD     = @P33" _
            & "        , SALSE         = @P34  , SALSETAX        = @P35, TOTALSALSE    = @P36" _
            & "        , PAYMENT       = @P37  , PAYMENTTAX      = @P38, TOTALPAYMENT  = @P39, DELFLG           = @P40" _
            & "        , INITYMD       = @P41  , INITUSER        = @P42, INITTERMID    = @P43" _
            & "        , UPDYMD        = @P44  , UPDUSER         = @P45, UPDTERMID     = @P46, RECEIVEYMD       = @P47" _
            & "    WHERE" _
            & "        KAISOUNO          = @P01" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0006_KAISOU" _
            & "        ( KAISOUNO,      KAISOUTYPE     , TRAINNO      , TRAINNAME" _
            & "        , KAISOUYMD,     OFFICECODE     , OFFICENAME" _
            & "        , SHIPPERSCODE , SHIPPERSNAME   , BASECODE     , BASENAME" _
            & "        , CONSIGNEECODE, CONSIGNEENAME  , DEPSTATION   , DEPSTATIONNAME" _
            & "        , ARRSTATION   , ARRSTATIONNAME , OBJECTIVECODE" _
            & "        , KAISOUSTATUS , KAISOUINFO     , FAREFLG      , USEPROPRIETYFLG , DELIVERYFLG" _
            & "        , DEPDATE,       ARRDATE        , ACCDATE      , EMPARRDATE" _
            & "        , ACTUALDEPDATE, ACTUALARRDATE  , ACTUALACCDATE, ACTUALEMPARRDATE" _
            & "        , TOTALTANK,     ORDERNO        , KEIJYOYMD" _
            & "        , SALSE,         SALSETAX       , TOTALSALSE" _
            & "        , PAYMENT,       PAYMENTTAX     , TOTALPAYMENT , DELFLG" _
            & "        , INITYMD,       INITUSER       , INITTERMID" _
            & "        , UPDYMD,        UPDUSER        , UPDTERMID    , RECEIVEYMD )" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P03, @P04" _
            & "        , @P05, @P06, @P07" _
            & "        , @P08, @P09, @P10, @P11" _
            & "        , @P12, @P13, @P14, @P15" _
            & "        , @P16, @P17, @P18" _
            & "        , @P19, @P20, @P21, @P22, @P48" _
            & "        , @P23, @P24, @P25, @P26" _
            & "        , @P27, @P28, @P29, @P30" _
            & "        , @P31, @P32, @P33" _
            & "        , @P34, @P35, @P36" _
            & "        , @P37, @P38, @P39, @P40" _
            & "        , @P41, @P42, @P43" _
            & "        , @P44, @P45, @P46, @P47);" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    KAISOUNO" _
            & "    , KAISOUTYPE" _
            & "    , TRAINNO" _
            & "    , TRAINNAME" _
            & "    , KAISOUYMD" _
            & "    , OFFICECODE" _
            & "    , OFFICENAME" _
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
            & "    , OBJECTIVECODE" _
            & "    , KAISOUSTATUS" _
            & "    , KAISOUINFO" _
            & "    , FAREFLG" _
            & "    , USEPROPRIETYFLG" _
            & "    , DELIVERYFLG" _
            & "    , DEPDATE" _
            & "    , ARRDATE" _
            & "    , ACCDATE" _
            & "    , EMPARRDATE" _
            & "    , ACTUALDEPDATE" _
            & "    , ACTUALARRDATE" _
            & "    , ACTUALACCDATE" _
            & "    , ACTUALEMPARRDATE" _
            & "    , TOTALTANK" _
            & "    , ORDERNO" _
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
            & "    OIL.OIT0006_KAISOU" _
            & " WHERE" _
            & "        KAISOUNO      = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar) '回送№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar) '回送パターン
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar) '本線列車
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar) '本線列車名
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)     '回送登録日
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar) '営業所コード
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar) '営業所名
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar) '荷主コード
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar) '荷主名
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar) '基地コード
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar) '基地名
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar) '荷受人コード
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar) '荷受人名
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar) '発駅コード
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar) '発駅名
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar) '着駅コード
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar) '着駅名
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar) '目的
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar) '回送進行ステータス
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar) '回送情報
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar) '運賃フラグ
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar) '利用可否フラグ
                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", SqlDbType.NVarChar) '託送指示フラグ
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.Date)     '発日（予定）
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.Date)     '着日（予定）
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Date)     '受入日（予定）
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Date)     '空車着日（予定）
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.Date)     '発日（実績）
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Date)     '着日（実績）
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.Date)     '受入日（実績）
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.Date)     '空車着日（実績）
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.Int)      '合計車数
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar) '受注№
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.Date)     '計上日
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.Int)      '売上金額
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.Int)      '売上消費税額
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.Int)      '売上合計金額
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.Int)      '支払金額
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.Int)      '支払消費税額
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.Int)      '支払合計金額
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.NVarChar) '削除フラグ
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.DateTime) '登録年月日
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", SqlDbType.NVarChar) '登録ユーザーＩＤ
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", SqlDbType.NVarChar) '登録端末
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", SqlDbType.DateTime) '更新年月日
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", SqlDbType.NVarChar) '更新ユーザーＩＤ
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", SqlDbType.NVarChar) '更新端末
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", SqlDbType.DateTime) '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar) '回送№

                Dim WW_DATENOW As DateTime = Date.Now

                PARA01.Value = work.WF_SEL_KAISOUNUMBER.Text      '回送№
                PARA02.Value = Me.TxtKaisouTypeCode.Text          'パターンコード
                PARA03.Value = Me.TxtTrainNo.Text                 '本線列車
                PARA04.Value = Me.TxtTrainName.Text               '本線列車名
                PARA05.Value = WW_DATENOW                         '回送登録日

                PARA06.Value = Me.TxtKaisouOrderOfficeCode.Text   '回送登録営業所コード
                PARA07.Value = Me.TxtKaisouOrderOffice.Text       '回送登録営業所名
                PARA08.Value = work.WF_SEL_SHIPPERSCODE.Text      '荷主コード
                PARA09.Value = work.WF_SEL_SHIPPERSNAME.Text      '荷主名
                PARA10.Value = work.WF_SEL_BASECODE.Text          '基地コード
                PARA11.Value = work.WF_SEL_BASENAME.Text          '基地名
                PARA12.Value = work.WF_SEL_CONSIGNEECODE.Text     '荷受人コード
                PARA13.Value = work.WF_SEL_CONSIGNEENAME.Text     '荷受人名

                PARA14.Value = Me.TxtDepstationCode.Text          '発駅コード
                PARA15.Value = Me.LblDepstationName.Text          '発駅名
                PARA16.Value = Me.TxtArrstationCode.Text          '着駅コード
                PARA17.Value = Me.LblArrstationName.Text          '着駅名

                PARA18.Value = Me.TxtObjective.Text               '目的
                PARA19.Value = work.WF_SEL_KAISOUSTATUS.Text      '回送進行ステータス
                PARA20.Value = work.WF_SEL_INFORMATION.Text       '回送情報

                '運賃フラグ(1:片道 2:往復)
                If Me.ChkSelect.Checked = True Then
                    PARA21.Value = "1"
                Else
                    PARA21.Value = "2"

                End If

                PARA22.Value = "1"                                '利用可否フラグ(1:利用可 2:利用不可)
                PARA48.Value = work.WF_SEL_DELIVERYFLG.Text       '託送指示フラグ(0:未手配 1:手配)

                PARA23.Value = Me.TxtDepDate.Text                 '発日（予定）
                PARA24.Value = Me.TxtArrDate.Text                 '着日（予定）

                '### 目的が"24:疎開留置", "25:移動"の場合は、受入日はNULLを設定 ###########
                '受入日（予定）
                If Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
                    OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25 Then
                    PARA25.Value = DBNull.Value
                    '########################################################################
                Else
                    PARA25.Value = Me.TxtAccDate.Text
                End If
                PARA26.Value = Me.TxtEmparrDate.Text              '空車着日（予定）

                PARA27.Value = DBNull.Value                       '発日（実績）
                PARA28.Value = DBNull.Value                       '着日（実績）
                PARA29.Value = DBNull.Value                       '受入日（実績）
                PARA30.Value = DBNull.Value                       '空車着日（実績）

                'PARA31.Value = Me.TxtTankCnt.Text                 'タンク車数
                PARA32.Value = work.WF_SEL_ORDERNUMBER.Text       '受注№

                PARA33.Value = DBNull.Value                       '計上日
                PARA34.Value = 0                                  '売上金額
                PARA35.Value = 0                                  '売上消費税額
                PARA36.Value = 0                                  '売上合計金額
                PARA37.Value = 0                                  '支払金額
                PARA38.Value = 0                                  '支払消費税額
                PARA39.Value = 0                                  '支払合計金額

                PARA40.Value = C_DELETE_FLG.ALIVE                 '削除フラグ(0:有効 1:無効)
                PARA41.Value = WW_DATENOW                         '登録年月日
                PARA42.Value = Master.USERID                      '登録ユーザーID
                PARA43.Value = Master.USERTERMID                  '登録端末
                PARA44.Value = WW_DATENOW                         '更新年月日
                PARA45.Value = Master.USERID                      '更新ユーザーID
                PARA46.Value = Master.USERTERMID                  '更新端末
                PARA47.Value = C_DEFAULT_YMD

                For Each OIT0006row As DataRow In OIT0006tbl.Rows

                    'DB更新
                    PARA31.Value = OIT0006tbl.Select("DELFLG = '0'").Count          'タンク車数
                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = work.WF_SEL_KAISOUNUMBER.Text

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0006UPDtbl) Then
                            OIT0006UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0006UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0006UPDtbl.Clear()
                        OIT0006UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0006UPDrow As DataRow In OIT0006UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0006D"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0006UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D UPDATE_INSERT_KAISOU")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D UPDATE_INSERT_KAISOU"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 回送明細TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateKaisouDetail(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0007_KAISOUDETAIL" _
            & "    WHERE" _
            & "        KAISOUNO        = @P01" _
            & "   AND  DETAILNO        = @P02" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0007_KAISOUDETAIL" _
            & "    SET" _
            & "        SHIPORDER       = @P03  , TANKNO         = @P04, KAMOKU        = @P05" _
            & "        , KAISOUINFO    = @P06  , CARSNUMBER     = @P07, REMARK        = @P08" _
            & "        , ACTUALDEPDATE = @P09  , ACTUALARRDATE  = @P10, ACTUALACCDATE = @P11, ACTUALEMPARRDATE = @P12" _
            & "        , SALSE         = @P13  , SALSETAX       = @P14, TOTALSALSE    = @P15" _
            & "        , PAYMENT       = @P16  , PAYMENTTAX     = @P17, TOTALPAYMENT  = @P18" _
            & "        , INITYMD       = @P20  , INITUSER       = @P21, INITTERMID    = @P22" _
            & "        , UPDYMD        = @P23  , UPDUSER        = @P24, UPDTERMID     = @P25, RECEIVEYMD       = @P26" _
            & "    WHERE" _
            & "        KAISOUNO        = @P01" _
            & "        AND DETAILNO    = @P02" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0007_KAISOUDETAIL" _
            & "        ( KAISOUNO     , DETAILNO     , SHIPORDER    , TANKNO          , KAMOKU " _
            & "        , KAISOUINFO   , CARSNUMBER   , REMARK" _
            & "        , ACTUALDEPDATE, ACTUALARRDATE, ACTUALACCDATE, ACTUALEMPARRDATE" _
            & "        , SALSE        , SALSETAX     , TOTALSALSE" _
            & "        , PAYMENT      , PAYMENTTAX   , TOTALPAYMENT , DELFLG" _
            & "        , INITYMD      , INITUSER     , INITTERMID" _
            & "        , UPDYMD       , UPDUSER      , UPDTERMID    , RECEIVEYMD )" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08" _
            & "        , @P09, @P10, @P11, @P12" _
            & "        , @P13, @P14, @P15" _
            & "        , @P16, @P17, @P18, @P19" _
            & "        , @P20, @P21, @P22" _
            & "        , @P23, @P24, @P25, @P26);" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    KAISOUNO" _
            & "    , DETAILNO" _
            & "    , SHIPORDER" _
            & "    , TANKNO" _
            & "    , KAMOKU" _
            & "    , KAISOUINFO" _
            & "    , CARSNUMBER" _
            & "    , REMARK" _
            & "    , ACTUALDEPDATE" _
            & "    , ACTUALARRDATE" _
            & "    , ACTUALACCDATE" _
            & "    , ACTUALEMPARRDATE" _
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
            & "    OIL.OIT0007_KAISOUDETAIL" _
            & " WHERE" _
            & "        KAISOUNO      = @P01" _
            & "   AND  DETAILNO      = @P02"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)      '回送№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar)      '回送明細№
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar)      '発送順

                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar)      'タンク車№
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar)      '費用科目
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar)      '回送情報
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar)      '車数
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar)      '備考

                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.Date)          '発日（実績）
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.Date)          '着日（実績）
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.Date)          '受入日（実績）
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Date)          '空車着日（実績）

                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Int)           '売上金額
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.Int)           '売上消費税額
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.Int)           '売上合計金額
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.Int)           '支払金額
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.Int)           '支払消費税額
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.Int)           '支払合計金額
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar)      '削除フラグ
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.DateTime)      '登録年月日
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar)      '登録ユーザーID
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar)      '登録端末
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.DateTime)      '更新年月日
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar)      '更新ユーザーID
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar)      '更新端末
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.DateTime)      '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar)  '回送№
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar)  '回送明細№

                Dim WW_DATENOW As DateTime = Date.Now

                PARA01.Value = work.WF_SEL_KAISOUNUMBER.Text      '回送№
                PARA05.Value = DBNull.Value                       '費用科目
                PARA07.Value = 1                                  '車数

                PARA09.Value = DBNull.Value                       '発日（実績）
                PARA10.Value = DBNull.Value                       '積車着日（実績）
                PARA11.Value = DBNull.Value                       '受入日（実績）
                PARA12.Value = DBNull.Value                       '空車着日（実績）

                PARA13.Value = 0                                  '売上金額
                PARA14.Value = 0                                  '売上消費税額
                PARA15.Value = 0                                  '売上合計金額
                PARA16.Value = 0                                  '支払金額
                PARA17.Value = 0                                  '支払消費税額
                PARA18.Value = 0                                  '支払合計金額

                PARA20.Value = WW_DATENOW                         '登録年月日
                PARA21.Value = Master.USERID                      '登録ユーザーID
                PARA22.Value = Master.USERTERMID                  '登録端末
                PARA23.Value = WW_DATENOW                         '更新年月日
                PARA24.Value = Master.USERID                      '更新ユーザーID
                PARA25.Value = Master.USERTERMID                  '更新端末
                PARA26.Value = C_DEFAULT_YMD

                '更新ジャーナル出力
                JPARA01.Value = work.WF_SEL_KAISOUNUMBER.Text     '回送№

                For Each OIT0006row As DataRow In OIT0006tbl.Rows
                    'DB更新
                    PARA02.Value = OIT0006row("DETAILNO")         '回送明細№
                    PARA03.Value = OIT0006row("SHIPORDER")        '発送順
                    PARA04.Value = OIT0006row("TANKNO")           'タンク車№
                    PARA06.Value = OIT0006row("KAISOUINFO")       '回送情報
                    PARA08.Value = OIT0006row("REMARK")           '備考
                    PARA19.Value = OIT0006row("DELFLG")           '削除フラグ(0:有効 1:無効)

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA02.Value = OIT0006row("DETAILNO")            '回送明細№

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0006UPDtbl) Then
                            OIT0006UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0006UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0006UPDtbl.Clear()
                        OIT0006UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0006UPDrow As DataRow In OIT0006UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0006D"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0006UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D UPDATE_INSERT_KAISOUDETAIL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D UPDATE_INSERT_KAISOUDETAIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 回送TBL更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateKaisouMeisai(ByVal SQLcon As SqlConnection)

        Try
            '更新SQL文･･･回送TBLの各項目をを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0006_KAISOU " _
                    & "    SET ACTUALDEPDATE    = @P04, " _
                    & "        ACTUALARRDATE    = @P05, " _
                    & "        ACTUALACCDATE    = @P06, " _
                    & "        ACTUALEMPARRDATE = @P07, " _
                    & "        UPDYMD           = @P08, " _
                    & "        UPDUSER          = @P09, " _
                    & "        UPDTERMID        = @P10, " _
                    & "        RECEIVEYMD       = @P11  " _
                    & "  WHERE KAISOUNO         = @P01  " _
                    & "    AND DELFLG           <> @P02; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  '回送№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '削除フラグ
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)      '発日（実績）
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '着日（実績）
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)      '受入日（実績）
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)      '空車着日（実績）
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)  '更新年月日
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)  '更新ユーザーＩＤ
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)  '更新端末
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)  '集信日時

            PARA01.Value = work.WF_SEL_KAISOUNUMBER.Text
            PARA02.Value = C_DELETE_FLG.DELETE

            '発日（実績）
            If Me.TxtActualDepDate.Text = "" Then
                PARA04.Value = DBNull.Value
            Else
                PARA04.Value = Date.Parse(Me.TxtActualDepDate.Text)
            End If

            '着日（実績）
            If Me.TxtActualArrDate.Text = "" Then
                PARA05.Value = DBNull.Value
            Else
                PARA05.Value = Date.Parse(Me.TxtActualArrDate.Text)
            End If

            '受入日（実績）
            If Me.TxtActualAccDate.Text = "" Then
                PARA06.Value = DBNull.Value
            Else
                PARA06.Value = Date.Parse(Me.TxtActualAccDate.Text)
            End If

            '空車着日（実績）
            If Me.TxtActualEmparrDate.Text = "" Then
                PARA07.Value = DBNull.Value
            Else
                PARA07.Value = Date.Parse(Me.TxtActualEmparrDate.Text)
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D_KAISOU_MEISAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D_KAISOU_MEISAI UPDATE"
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
    ''' 回送明細TBL更新
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateKaisouDetailMeisai(ByVal SQLcon As SqlConnection)

        Try
            '更新SQL文･･･回送明細TBLの各項目をを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0007_KAISOUDETAIL " _
                    & "    SET REMARK               = @P12, " _
                    & "        ACTUALDEPDATE        = @P04, " _
                    & "        ACTUALARRDATE        = @P05, " _
                    & "        ACTUALACCDATE        = @P06, " _
                    & "        ACTUALEMPARRDATE     = @P07, " _
                    & "        UPDYMD               = @P08, " _
                    & "        UPDUSER              = @P09, " _
                    & "        UPDTERMID            = @P10, " _
                    & "        RECEIVEYMD           = @P11  " _
                    & "  WHERE KAISOUNO             = @P01  " _
                    & "    AND DETAILNO             = @P02  " _
                    & "    AND DELFLG              <> @P03; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  '回送№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '回送明細No
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)  '削除フラグ
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)  '備考
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)      '発日（実績）
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '着日（実績）
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)      '受入日（実績）
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)      '空車着日（実績）

            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)  '更新年月日
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)  '更新ユーザーＩＤ
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)  '更新端末
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)  '集信日時

            For Each OIT0006row As DataRow In OIT0006tbl.Rows
                PARA01.Value = OIT0006row("KAISOUNO")
                PARA02.Value = OIT0006row("DETAILNO")
                PARA03.Value = C_DELETE_FLG.DELETE

                '記事欄
                PARA12.Value = OIT0006row("REMARK")

                '発日（実績）
                If OIT0006row("ACTUALDEPDATE") = "" Then
                    PARA04.Value = DBNull.Value
                Else
                    PARA04.Value = OIT0006row("ACTUALDEPDATE")
                End If

                '着日（実績）
                If OIT0006row("ACTUALARRDATE") = "" Then
                    PARA05.Value = DBNull.Value
                Else
                    PARA05.Value = OIT0006row("ACTUALARRDATE")
                End If

                '受入日（実績）
                If OIT0006row("ACTUALACCDATE") = "" Then
                    PARA06.Value = DBNull.Value
                Else
                    PARA06.Value = OIT0006row("ACTUALACCDATE")
                End If

                '空車着日（実績）
                If OIT0006row("ACTUALEMPARRDATE") = "" Then
                    PARA07.Value = DBNull.Value
                Else
                    PARA07.Value = OIT0006row("ACTUALEMPARRDATE")
                End If

                PARA08.Value = Date.Now
                PARA09.Value = Master.USERID
                PARA10.Value = Master.USERTERMID
                PARA11.Value = C_DEFAULT_YMD

                SQLcmd.ExecuteNonQuery()
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D_KAISOUDETAIL_MEISAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D_KAISOUDETAIL_MEISAI UPDATE"
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
    ''' 回送(一覧)画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Protected Sub WW_KaisouListTBLSet(ByVal SQLcon As SqlConnection)
        If IsNothing(OIT0006WKtbl) Then
            OIT0006WKtbl = New DataTable
        End If

        If OIT0006WKtbl.Columns.Count <> 0 Then
            OIT0006WKtbl.Columns.Clear()
        End If

        OIT0006WKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                                   AS LINECNT" _
            & " , ''                                                  AS OPERATION" _
            & " , CAST(OIT0006.UPDTIMSTP AS bigint)                   AS TIMSTP" _
            & " , 1                                                   AS 'SELECT'" _
            & " , 0                                                   AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0006.KAISOUNO), '')   　            AS KAISOUNO" _
            & " , ISNULL(RTRIM(OIT0006.KAISOUTYPE), '')   　          AS KAISOUTYPE" _
            & " , ISNULL(RTRIM(OIT0006.TRAINNO), '')                  AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0006.TRAINNAME), '')                AS TRAINNAME" _
            & " , ISNULL(FORMAT(OIT0006.KAISOUYMD, 'yyyy/MM/dd'), '') AS KAISOUYMD" _
            & " , ISNULL(RTRIM(OIT0006.OFFICECODE), '')               AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0006.OFFICENAME), '')               AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0006.SHIPPERSCODE), '')             AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0006.SHIPPERSNAME), '')             AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0006.BASECODE), '')                 AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0006.BASENAME), '')                 AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0006.CONSIGNEECODE), '')            AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0006.CONSIGNEENAME), '')            AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0006.DEPSTATION), '')               AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0006.DEPSTATIONNAME), '')           AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0006.ARRSTATION), '')               AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0006.ARRSTATIONNAME), '')           AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0006.OBJECTIVECODE), '')            AS OBJECTIVECODE" _
            & " , ''                                                  AS OBJECTIVENAME" _
            & " , ISNULL(RTRIM(OIT0006.KAISOUSTATUS), '')             AS KAISOUSTATUS" _
            & " , ISNULL(RTRIM(OIS0015_1.VALUE1), '')                 AS KAISOUSTATUSNAME" _
            & " , ISNULL(RTRIM(OIT0006.KAISOUINFO), '')               AS KAISOUINFO" _
            & " , ISNULL(RTRIM(OIS0015_2.VALUE1), '')                 AS KAISOUINFONAME" _
            & " , ISNULL(RTRIM(OIT0006.FAREFLG), '')   　             AS FAREFLG" _
            & " , ISNULL(RTRIM(OIT0006.USEPROPRIETYFLG), '')   　     AS USEPROPRIETYFLG" _
            & " , ISNULL(RTRIM(OIT0006.DELIVERYFLG), '')   　         AS DELIVERYFLG" _
            & " , ISNULL(FORMAT(OIT0006.DEPDATE, 'yyyy/MM/dd'), '')           AS DEPDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACTUALDEPDATE, 'yyyy/MM/dd'), '')     AS ACTUALDEPDATE" _
            & " , ISNULL(FORMAT(OIT0006.ARRDATE, 'yyyy/MM/dd'), '')           AS ARRDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACTUALARRDATE, 'yyyy/MM/dd'), '')     AS ACTUALARRDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACCDATE, 'yyyy/MM/dd'), '')           AS ACCDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACTUALACCDATE, 'yyyy/MM/dd'), '')     AS ACTUALACCDATE" _
            & " , ISNULL(FORMAT(OIT0006.EMPARRDATE, 'yyyy/MM/dd'), '')        AS EMPARRDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACTUALEMPARRDATE, 'yyyy/MM/dd'), '')  AS ACTUALEMPARRDATE" _
            & " , ISNULL(RTRIM(OIT0006.TOTALTANK), '')   　           AS TOTALTANK" _
            & " , ISNULL(RTRIM(OIT0006.ORDERNO), '')                  AS ORDERNO" _
            & " , ISNULL(FORMAT(OIT0006.KEIJYOYMD, 'yyyy/MM/dd'), '')         AS KEIJYOYMD" _
            & " , ISNULL(RTRIM(OIT0006.SALSE), '')                   AS SALSE" _
            & " , ISNULL(RTRIM(OIT0006.SALSETAX), '')                AS SALSETAX" _
            & " , ISNULL(RTRIM(OIT0006.TOTALSALSE), '')              AS TOTALSALSE" _
            & " , ISNULL(RTRIM(OIT0006.PAYMENT), '')                 AS PAYMENT" _
            & " , ISNULL(RTRIM(OIT0006.PAYMENTTAX), '')              AS PAYMENTTAX" _
            & " , ISNULL(RTRIM(OIT0006.TOTALPAYMENT), '')            AS TOTALPAYMENT" _
            & " , ISNULL(RTRIM(OIT0006.DELFLG), '')                  AS DELFLG" _
            & " FROM OIL.OIT0006_KAISOU OIT0006 " _
            & "  INNER JOIN OIL.VIW0003_OFFICECHANGE VIW0003 ON " _
            & "        VIW0003.ORGCODE    = @P1 " _
            & "    AND VIW0003.OFFICECODE = OIT0006.OFFICECODE " _
            & "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_1 ON " _
            & "        OIS0015_1.CLASS   = 'KAISOUSTATUS' " _
            & "    AND OIS0015_1.KEYCODE = OIT0006.KAISOUSTATUS " _
            & "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_2 ON " _
            & "        OIS0015_2.CLASS   = 'KAISOUINFO' " _
            & "    AND OIS0015_2.KEYCODE = OIT0006.KAISOUINFO " _
            & " WHERE OIT0006.DELFLG     <> @P3" _
            & "   AND OIT0006.DEPDATE    >= @P2"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '営業所
        If Not String.IsNullOrEmpty(work.WF_SEL_SALESOFFICECODEMAP.Text) Then
            SQLStr &= String.Format("    AND OIT0006.OFFICECODE = '{0}'", work.WF_SEL_SALESOFFICECODEMAP.Text)
        End If
        '列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNUMBER.Text) Then
            SQLStr &= String.Format("    AND OIT0006.TRAINNO = '{0}'", work.WF_SEL_TRAINNUMBER.Text)
        End If
        '状態(回送進行ステータス)
        If Not String.IsNullOrEmpty(work.WF_SEL_STATUSCODE.Text) Then
            SQLStr &= String.Format("    AND OIT0006.KAISOUSTATUS = '{0}'", work.WF_SEL_STATUSCODE.Text)
        End If
        '目的
        If Not String.IsNullOrEmpty(work.WF_SEL_OBJECTIVECODEMAP.Text) Then
            SQLStr &= String.Format("    AND OIT0006.OBJECTIVECODE = '{0}'", work.WF_SEL_OBJECTIVECODEMAP.Text)
        End If
        '着駅
        If Not String.IsNullOrEmpty(work.WF_SEL_ARRIVALSTATIONMAP.Text) Then
            SQLStr &= String.Format("    AND OIT0006.ARRSTATION = '{0}'", work.WF_SEL_ARRIVALSTATIONMAP.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIT0006.KAISOUNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar)     '組織コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)     '年月日(開始)
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
                        OIT0006WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0006row As DataRow In OIT0006WKtbl.Rows
                    i += 1
                    OIT0006row("LINECNT") = i        'LINECNT

                    '◯名称取得
                    '目的
                    CODENAME_get("OBJECTIVECODE", OIT0006row("OBJECTIVECODE"), OIT0006row("OBJECTIVENAME"), WW_RTN_SW)

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006WKtbl, work.WF_SEL_INPTBL.Text)

    End Sub

#End Region

#Region "関連チェック"
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
        '回送登録営業所
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", Me.TxtKaisouOrderOfficeCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SALESOFFICE", Me.TxtKaisouOrderOfficeCode.Text, Me.TxtKaisouOrderOffice.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "回送登録営業所 : " & Me.TxtKaisouOrderOfficeCode.Text)
                Me.TxtKaisouOrderOffice.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            Me.TxtKaisouOrderOffice.Focus()
            WW_CheckMES1 = "回送営業所入力エラー。"
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

        '目的
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OBJECTIVECODE", Me.TxtObjective.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "目的", needsPopUp:=True)
            Me.TxtObjective.Focus()
            WW_CheckMES1 = "目的入力エラー。"
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

        '(予定)着日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRDATE", Me.TxtArrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(Me.TxtArrDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            '年月日チェック
            WW_CheckDate(Me.TxtArrDate.Text, "(予定)着日", WW_CS0024FCHECKERR)
            'Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "(予定)着日", needsPopUp:=True)
            Me.TxtArrDate.Focus()
            WW_CheckMES1 = "着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '### 目的が"24:疎開留置", "25:移動"の場合は、受入日のチェックを実施しない ###########
        If Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
            OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25 Then

            '### 特に何もしない ##########################################

        Else
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
        End If
        '##################################################################################

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

        ''(一覧)チェック(準備)
        'For Each OIT0006row As DataRow In OIT0006tbl.Rows
        '    OIT0006row("KAISOUINFO") = ""
        '    OIT0006row("KAISOUINFONAME") = ""
        'Next
        ''○ 画面表示データ保存
        'Master.SaveTable(OIT0006tbl)

        '◯ 発送順でソートし、重複がないかチェックする。
        Dim OIT0006tbl_DUMMY As DataTable = OIT0006tbl.Copy
        Dim OIT0006tbl_dv As DataView = New DataView(OIT0006tbl_DUMMY)
        Dim chkShipOrder As String = ""
        Dim chkTankNo As String = ""

        OIT0006tbl_dv.Sort = "SHIPORDER"
        For Each drv As DataRowView In OIT0006tbl_dv
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

        'タンク車Noでソートし、重複がないかチェックする。
        OIT0006tbl_dv.Sort = "TANKNO"
        For Each drv As DataRowView In OIT0006tbl_dv

            '○ 対象ヘッダー取得
            Dim updHeader = OIT0006tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = drv("LINECNT"))

            If drv("HIDDEN") <> "1" AndAlso drv("TANKNO") <> "" AndAlso chkTankNo = drv("TANKNO") Then
                Master.Output(C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckMES1 = "タンク車№重複エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                O_RTN = "ERR"

                updHeader.Item("KAISOUINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                CODENAME_get("KAISOUINFO", updHeader.Item("KAISOUINFO"), updHeader.Item("KAISOUINFONAME"), WW_DUMMY)

                '○ 画面表示データ保存
                Master.SaveTable(OIT0006tbl)
                Exit Sub
            ElseIf updHeader.Item("KAISOUINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then

                updHeader.Item("KAISOUINFO") = ""
                updHeader.Item("KAISOUINFONAME") = ""

                '○ 画面表示データ保存
                Master.SaveTable(OIT0006tbl)

            End If

            '行削除したデータの場合は退避しない。
            If drv("HIDDEN") <> "1" Then
                chkTankNo = drv("TANKNO")
            End If
        Next

        '(一覧)チェック
        For Each OIT0006row As DataRow In OIT0006tbl.Rows

            '(一覧)発送順(空白チェック)
            If OIT0006row("SHIPORDER") = "" And OIT0006row("DELFLG") = "0" Then
                Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)発送順", needsPopUp:=True)

                WW_CheckMES1 = "発送順未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0006row)
                O_RTN = "ERR"
                Exit Sub
            End If

            '(一覧)タンク車No(空白チェック)
            If OIT0006row("TANKNO") = "" And OIT0006row("DELFLG") = "0" Then
                Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)タンク車No", needsPopUp:=True)

                WW_CheckMES1 = "タンク車No未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0006row)
                O_RTN = "ERR"
                Exit Sub
            End If

        Next

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' チェック処理(明細更新ボタン押下時)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckMeisai(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '〇 (実績)の日付は入力されていた場合チェックする。
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

    End Sub

    ''' <summary>
    ''' 年月日妥当性チェック((予定)日付)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckPlanValidityDate(ByRef O_RTN As String)

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

        '(予定)着日 と　現在日付を比較
        iresult = Date.Parse(Me.TxtArrDate.Text).CompareTo(DateTime.Today)
        If iresult = -1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)着日", needsPopUp:=True)
            Me.TxtArrDate.Focus()
            WW_CheckMES1 = "(予定日)過去日付エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '### 目的が"24:疎開留置", "25:移動"の場合は、受入日のチェックを実施しない ###########
        If Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
            OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25 Then

            '### 特に何もしない ##########################################

        Else
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
        End If
        '##################################################################################

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
        '(予定)発日 と　(予定)着日を比較
        iresult = Date.Parse(Me.TxtDepDate.Text).CompareTo(Date.Parse(Me.TxtArrDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)発日 > (予定)着日", needsPopUp:=True)
            Me.TxtArrDate.Focus()
            WW_CheckMES1 = "(予定日)入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If


        '### 目的が"24:疎開留置", "25:移動"の場合は、受入日のチェックを実施しない ###########
        If Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
            OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25 Then

            '(予定)着日 と　(予定)空車着日を比較
            iresult = Date.Parse(Me.TxtArrDate.Text).CompareTo(Date.Parse(Me.TxtEmparrDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)着日 > (予定)空車着日", needsPopUp:=True)
                Me.TxtEmparrDate.Focus()
                WW_CheckMES1 = "(予定日)入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
            '##################################################################################

        Else
            '(予定)着日 と　(予定)受入日を比較
            iresult = Date.Parse(Me.TxtArrDate.Text).CompareTo(Date.Parse(Me.TxtAccDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)着日 > (予定)受入日", needsPopUp:=True)
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
        '(実績)発日 と　現在日付を比較
        '受注進行ステータスが"300:受注確定"の場合
        If Me.TxtActualDepDate.Text <> "" _
            AndAlso work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_300 Then
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
            AndAlso work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_350 Then
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
           AndAlso work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_400 Then
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
            AndAlso work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_450 Then
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
        '(実績)発日 と　(実績)着日を比較
        If Me.TxtActualDepDate.Text <> "" AndAlso Me.TxtActualArrDate.Text <> "" Then
            iresult = Date.Parse(Me.TxtActualDepDate.Text).CompareTo(Date.Parse(Me.TxtActualArrDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)発日 > (実績)着日", needsPopUp:=True)
                Me.TxtActualArrDate.Focus()
                WW_CheckMES1 = "(実績日)入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)着日 と　(実績)受入日を比較
        If Me.TxtActualArrDate.Text <> "" AndAlso Me.TxtActualAccDate.Text <> "" Then
            iresult = Date.Parse(Me.TxtActualArrDate.Text).CompareTo(Date.Parse(Me.TxtActualAccDate.Text))
            If iresult = 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)着日 > (実績)受入日", needsPopUp:=True)
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
        For Each OIT0006row As DataRow In OIT0006tbl.Rows
            '例) iresult = dt1.Date.CompareTo(dt2.Date)
            '    iresultの意味
            '     0 : dt1とdt2は同じ日
            '    -1 : dt1はdt2より前の日
            '     1 : dt1はdt2より後の日
            '〇 (実績)発日 と　(一覧)発日を比較
            If Me.TxtActualDepDate.Text <> "" AndAlso OIT0006row("ACTUALDEPDATE") <> "" Then
                iresult = Date.Parse(Me.TxtActualDepDate.Text).CompareTo(Date.Parse(OIT0006row("ACTUALDEPDATE")))
                If iresult = 1 Then
                    OIT0006row("KAISOUINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_92
                    CODENAME_get("KAISOUINFO", OIT0006row("KAISOUINFO"), OIT0006row("KAISOUINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "(実績)発日で入力した日付より過去日のためエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Continue For
                Else
                    OIT0006row("KAISOUINFO") = ""
                    OIT0006row("KAISOUINFONAME") = ""
                End If
            End If

            '〇 (実績)積車着日 と　(一覧)積車着日を比較
            If Me.TxtActualArrDate.Text <> "" AndAlso OIT0006row("ACTUALARRDATE") <> "" Then
                iresult = Date.Parse(Me.TxtActualArrDate.Text).CompareTo(Date.Parse(OIT0006row("ACTUALARRDATE")))
                If iresult = 1 Then
                    OIT0006row("KAISOUINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_93
                    CODENAME_get("KAISOUINFO", OIT0006row("KAISOUINFO"), OIT0006row("KAISOUINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "(実績)積車着日で入力した日付より過去日のためエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Continue For
                Else
                    OIT0006row("KAISOUINFO") = ""
                    OIT0006row("KAISOUINFONAME") = ""
                End If
            End If

            '〇 (実績)受入日 と　(一覧)受入日を比較
            If Me.TxtActualAccDate.Text <> "" AndAlso OIT0006row("ACTUALACCDATE") <> "" Then
                iresult = Date.Parse(Me.TxtActualAccDate.Text).CompareTo(Date.Parse(OIT0006row("ACTUALACCDATE")))
                If iresult = 1 Then
                    OIT0006row("KAISOUINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_94
                    CODENAME_get("KAISOUINFO", OIT0006row("KAISOUINFO"), OIT0006row("KAISOUINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "(実績)受入日で入力した日付より過去日のためエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Continue For
                Else
                    OIT0006row("KAISOUINFO") = ""
                    OIT0006row("KAISOUINFONAME") = ""
                End If
            End If

            '〇 (実績)空車着日 と　(一覧)空車着日を比較
            If Me.TxtActualEmparrDate.Text <> "" AndAlso OIT0006row("ACTUALEMPARRDATE") <> "" Then
                iresult = Date.Parse(Me.TxtActualEmparrDate.Text).CompareTo(Date.Parse(OIT0006row("ACTUALEMPARRDATE")))
                If iresult = 1 Then
                    OIT0006row("KAISOUINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_95
                    CODENAME_get("KAISOUINFO", OIT0006row("KAISOUINFO"), OIT0006row("KAISOUINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "(実績)空車着日で入力した日付より過去日のためエラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Continue For
                Else
                    OIT0006row("KAISOUINFO") = ""
                    OIT0006row("KAISOUINFONAME") = ""
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

        '(一覧)日付有効性チェックがエラーの場合
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(実績)日付 > (一覧)日付", needsPopUp:=True)
            Exit Sub
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

        WW_ERR_MES &= ControlChars.NewLine & "  --> オーダー№         =" & Me.TxtKaisouOrderNo.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> ステータス         =" & Me.TxtKaisouStatus.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車           =" & Me.TxtTrainNo.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 発駅               =" & Me.TxtDepstationCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 着駅               =" & Me.TxtArrstationCode.Text & " , "
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
    ''' <param name="OIM0006row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckListERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0006row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0006row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項番               =" & OIM0006row("LINECNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発送順             =" & OIM0006row("SHIPORDER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車番号       =" & OIM0006row("TANKNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> (実績)発日         =" & OIM0006row("ACTUALDEPDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> (実績)着日         =" & OIM0006row("ACTUALARRDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> (実績)受入日       =" & OIM0006row("ACTUALACCDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> (実績)空車着日     =" & OIM0006row("ACTUALEMPARRDATE")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

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
    ''' タンク車割当状況チェック
    ''' </summary>
    Protected Sub WW_TANKQUOTACHK(ByVal I_Value As String,
                                  ByVal I_updHeader As DataRow,
                                  ByVal O_VALUE() As String,
                                  ByRef O_chkSts As String)
        '戻り値初期化
        O_chkSts = "0"

        '〇 (一覧)項目変更箇所特定
        Select Case I_Value
            Case "TANKNO"
                'タンク車(所在地)
                Dim chkTankLocation As String = O_VALUE(1)
                'タンク車(管轄支店)
                Dim chkTankBranch As String = O_VALUE(3)
                'タンク車(所属営業所)
                Dim chkTankOffice As String = O_VALUE(5)
                'タンク車(タンク車状態)
                Dim chkTankStatus As String = O_VALUE(7)
                'タンク車(積車区分)
                Dim chkLoading As String = O_VALUE(9)

                '★受注データ(受注進行ステータス)確認
                Dim strOrderStatus As String = ""
                WW_CheckOrderTable(I_updHeader, strOrderStatus)

                '受注進行ステータスの状況にて判断
                Select Case strOrderStatus
                    Case BaseDllConst.CONST_ORDERSTATUS_100

                        '### 特に何もしない ################

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
                         BaseDllConst.CONST_ORDERSTATUS_310

                        Master.Output(C_MESSAGE_NO.OIL_TANKNO_SHIPPING_USE, C_MESSAGE_TYPE.ERR, I_updHeader("TANKNO"), needsPopUp:=True)

                        '異常終了
                        O_chkSts = "2"

                        '輸送中と判断(下記ステータス)
                        '320:受注確定, 350:受注確定(発日入力済み), 400:受入確認中
                    Case BaseDllConst.CONST_ORDERSTATUS_320,
                         BaseDllConst.CONST_ORDERSTATUS_350,
                         BaseDllConst.CONST_ORDERSTATUS_400

                        '400:受入確認中の場合は警告メッセージ(割当はできる)
                        If strOrderStatus = BaseDllConst.CONST_ORDERSTATUS_400 Then

                            '回送明細TBLの回送情報を更新
                            WW_UpdateKaisouInfo("2", I_updHeader)
                            I_updHeader("KAISOUINFO") = BaseDllConst.CONST_ORDERINFO_10
                            CODENAME_get("ORDERINFO", BaseDllConst.CONST_ORDERINFO_10, I_updHeader("KAISOUINFONAME"), WW_DUMMY)
                            Master.Output(C_MESSAGE_NO.OIL_TANKNO_LOADING_USE, C_MESSAGE_TYPE.WAR, I_updHeader("TANKNO"), needsPopUp:=True)

                            '警告終了
                            O_chkSts = "1"
                        Else
                            Master.Output(C_MESSAGE_NO.OIL_TANKNO_SHIPPING_USE, C_MESSAGE_TYPE.ERR, I_updHeader("TANKNO"), needsPopUp:=True)

                            '異常終了
                            O_chkSts = "2"
                        End If

                    Case BaseDllConst.CONST_ORDERSTATUS_450,
                         BaseDllConst.CONST_ORDERSTATUS_500,
                         BaseDllConst.CONST_ORDERSTATUS_550,
                         BaseDllConst.CONST_ORDERSTATUS_600,
                         BaseDllConst.CONST_ORDERSTATUS_700,
                         BaseDllConst.CONST_ORDERSTATUS_800

                        '### 特に何もしない ################

                    Case Else

                        '### 特に何もしない ################

                End Select
        End Select

    End Sub

    ''' <summary>
    ''' 受注(TBL)受注進行ステータス取得(回送にて使用するタンク車の状態)
    ''' </summary>
    Protected Sub WW_CheckOrderTable(ByVal I_updHeader As DataRow,
                                     ByRef O_STATUS As String)

        '受注進行ステータス初期化
        O_STATUS = ""

        If IsNothing(OIT0006WKtbl) Then
            OIT0006WKtbl = New DataTable
        End If

        If OIT0006WKtbl.Columns.Count <> 0 Then
            OIT0006WKtbl.Columns.Clear()
        End If

        OIT0006WKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                                    AS LINECNT" _
            & " , ''                                                   AS OPERATION" _
            & " , CAST(OIT0002.UPDTIMSTP AS bigint)                    AS TIMSTP" _
            & " , 1                                                    AS 'SELECT'" _
            & " , 0                                                    AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')   　              AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')   　             AS DETAILNO" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')   　          AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                    AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')                AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')                AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')                AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')            AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')                AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')            AS ARRSTATIONNAME" _
            & " , ISNULL(FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd'), '')    AS LODDATE" _
            & " , ISNULL(FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd'), '')    AS DEPDATE" _
            & " , ISNULL(FORMAT(OIT0002.ARRDATE, 'yyyy/MM/dd'), '')    AS ARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACCDATE, 'yyyy/MM/dd'), '')    AS ACCDATE" _
            & " , ISNULL(FORMAT(OIT0002.EMPARRDATE, 'yyyy/MM/dd'), '') AS EMPARRDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALLODDATE, 'yyyy/MM/dd'), '')    AS ACTUALLODDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALDEPDATE, 'yyyy/MM/dd'), '')    AS ACTUALDEPDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALARRDATE, 'yyyy/MM/dd'), '')    AS ACTUALARRDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALACCDATE, 'yyyy/MM/dd'), '')    AS ACTUALACCDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALEMPARRDATE, 'yyyy/MM/dd'), '') AS ACTUALEMPARRDATE" _
            & " , ISNULL(RTRIM(OIT0002.DELFLG), '')                    AS DELFLG" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & "  INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "        OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "    AND OIT0003.TANKNO  = @P01 " _
            & "    AND OIT0003.DELFLG <> @P03 " _
            & " WHERE OIT0002.DEPDATE          = @P02" _
            & "    AND OIT0002.USEPROPRIETYFLG = '1'" _
            & "    AND OIT0002.DELFLG          <> @P03"

        SQLStr &=
              " ORDER BY" _
            & "    OIT0003.TANKNO, OIT0002.LODDATE, OIT0002.DEPDATE"

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)     'タンク車№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.DateTime)     '(予定)発日
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = I_updHeader("TANKNO")
                PARA2.Value = Me.TxtDepDate.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0006WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0006row As DataRow In OIT0006WKtbl.Rows
                    i += 1
                    OIT0006row("LINECNT") = i        'LINECNT

                    O_STATUS = OIT0006row("ORDERSTATUS")

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D CHECK_ORDERTABLE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D CHECK_ORDERTABLE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

#End Region

    ''' <summary>
    ''' 画面表示設定処理
    ''' </summary>
    Protected Sub WW_ScreenEnabledSet()

        '〇 タブの使用可否制御
        '100:回送受付～500:検収中の場合は、タブ「タンク車割当」のみ許可
        If work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_100 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_200 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_210 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_250 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_300 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_350 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_400 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_450 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_500 Then
            WF_Dtab01.Enabled = True
            WF_Dtab02.Enabled = False

            '上記以外は、タブ「費用入力」の許可
        Else
            WF_Dtab01.Enabled = True
            WF_Dtab02.Enabled = False

        End If

        '〇 回送内容の制御
        '100:回送受付以外の場合は、回送内容(ヘッダーの内容)の変更を不可とする。
        If work.WF_SEL_KAISOUSTATUS.Text <> BaseDllConst.CONST_KAISOUSTATUS_100 Then
            '回送登録営業所
            Me.TxtKaisouOrderOffice.Enabled = False
            '本線列車
            Me.TxtTrainNo.Enabled = False
            '目的
            Me.TxtObjective.Enabled = False
            'タンク車数
            Me.TxtTankCnt.Enabled = False
            '回送パターン
            Me.TxtKaisouType.Enabled = False
            '運賃フラグ
            Me.ChkSelect.Enabled = False
            '発駅
            Me.TxtDepstationCode.Enabled = False
            '着駅
            Me.TxtArrstationCode.Enabled = False

            '(予定)発日
            Me.TxtDepDate.Enabled = False
            '(予定)積車着日
            Me.TxtArrDate.Enabled = False
            '(予定)受入日
            Me.TxtAccDate.Enabled = False
            '(予定)空車着日
            Me.TxtEmparrDate.Enabled = False
        Else
            '回送登録営業所
            Me.TxtKaisouOrderOffice.Enabled = True
            '本線列車
            Me.TxtTrainNo.Enabled = True
            '目的
            Me.TxtObjective.Enabled = True
            'タンク車数
            Me.TxtTankCnt.Enabled = True
            '回送パターン
            Me.TxtKaisouType.Enabled = True
            '運賃フラグ
            Me.ChkSelect.Enabled = True
            '発駅
            Me.TxtDepstationCode.Enabled = True
            '着駅
            Me.TxtArrstationCode.Enabled = True

            '(予定)発日
            Me.TxtDepDate.Enabled = True
            '(予定)積車着日
            Me.TxtArrDate.Enabled = True

            '### 目的が"24:疎開留置", "25:移動"の場合は、受入日の入力を省略する ###########
            If Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
                OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25 Then
                '(予定)受入日
                Me.TxtAccDate.Enabled = False
                Me.TxtAccDate.Text = ""
            Else
                '(予定)受入日
                Me.TxtAccDate.Enabled = True
            End If
            '############################################################################

            '(予定)空車着日
            Me.TxtEmparrDate.Enabled = True
        End If

        '〇 (実績)の日付の入力可否制御
        '回送情報が以下の場合は、(実績)の日付の入力を制限
        '100:回送受付, 200:手配, 210:手配中
        If work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_100 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_200 _
            OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_210 Then

            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = False
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = False
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = False

            '受注情報が「250:手配完了」の場合は、(実績)すべての日付の入力を解放
            '250:手配完了
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_250 Then
            '(実績)発日
            Me.TxtActualDepDate.Enabled = True
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = True

            '### 目的が"24:疎開留置", "25:移動"の場合は、受入日の入力を省略する ###########
            If Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
                OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25 Then
                '(実績)受入日
                Me.TxtActualAccDate.Enabled = False
            Else
                '(実績)受入日
                Me.TxtActualAccDate.Enabled = True
            End If
            '############################################################################
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True

            '### 積込日の概念がないため削除 ################################################
            '    '回送情報が「300:回送確定」の場合は、(実績)積込日の入力を制限
            '    '300:回送確定
            'ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_300 Then
            '    '(実績)発日
            '    Me.TxtActualDepDate.Enabled = True
            '    '(実績)積車着日
            '    Me.TxtActualArrDate.Enabled = True
            '    '(実績)受入日
            '    Me.TxtActualAccDate.Enabled = True
            '    '(実績)空車着日
            '    Me.TxtActualEmparrDate.Enabled = True
            '###############################################################################

            '回送情報が「350:回送確定」の場合は、(実績)発日の入力を制限
            '350:回送確定((実績)発日入力済み)
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_350 Then
            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = True

            '### 目的が"24:疎開留置", "25:移動"の場合は、受入日の入力を省略する ###########
            If Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
                OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25 Then
                '(実績)受入日
                Me.TxtActualAccDate.Enabled = False
            Else
                '(実績)受入日
                Me.TxtActualAccDate.Enabled = True
            End If
            '############################################################################

            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True

            '回送情報が「400:受入確認中」の場合は、(実績)着日の入力を制限
            '400:受入確認中
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_400 Then
            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)積車着日
            Me.TxtActualArrDate.Enabled = False

            '### 目的が"24:疎開留置", "25:移動"の場合は、受入日の入力を省略する ###########
            If Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
                OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25 Then
                '(実績)受入日
                Me.TxtActualAccDate.Enabled = False
            Else
                '(実績)受入日
                Me.TxtActualAccDate.Enabled = True
            End If
            '############################################################################

            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True

            '回送情報が「450:受入確認中」の場合は、(実績)受入日の入力を制限
            '450:受入確認中((実績)受入日入力済み)
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_450 Then
            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)着日
            Me.TxtActualArrDate.Enabled = False
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = False
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True

            '回送情報が「500:検収中」の場合は、(実績)空車着日の入力を制限
            '500:検収中
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_500 Then
            '(実績)発日
            Me.TxtActualDepDate.Enabled = False
            '(実績)着日
            Me.TxtActualArrDate.Enabled = False
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = False
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = False

            '550:検収済
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_550 Then
            '600:費用確定
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_600 Then
            '700:経理未計上
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_700 Then
            '800:経理計上
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_800 Then
            '900:受注キャンセル
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_900 Then

        Else
            '(実績)発日
            Me.TxtActualDepDate.Enabled = True
            '(実績)着日
            Me.TxtActualArrDate.Enabled = True
            '(実績)受入日
            Me.TxtActualAccDate.Enabled = True
            '(実績)空車着日
            Me.TxtActualEmparrDate.Enabled = True

        End If

    End Sub

    ''' <summary>
    ''' 画面表示設定処理(回送進行ステータス)
    ''' </summary>
    Protected Sub WW_ScreenKaisouStatusSet()

        '〇 回送ステータスが"100:回送受付"の場合
        If work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_100 Then

            '画面表示設定処理(回送進行ステータス(変更分を反映))
            WW_ScreenKaisouStatusChgRef(BaseDllConst.CONST_KAISOUSTATUS_200)

            '〇 回送ステータスが"200:手配"へ変更された場合
            If work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_200 Then
                'WF_DTAB_CHANGE_NO.Value = "1"
                'WF_Detail_TABChange()

                '〇タンク車所在の更新
                WW_TankShozaiSet()

            End If

            '〇 回送ステータスが"200:手配"の場合
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_200 Then

            '託送指示入力＝"1:手配"
            If work.WF_SEL_DELIVERYFLG.Text = "1" Then

                '画面表示設定処理(回送進行ステータス(変更分を反映))
                WW_ScreenKaisouStatusChgRef(BaseDllConst.CONST_KAISOUSTATUS_250)

            End If
        End If

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

                '回送進行ステータスが"100:回送受付"
                '回送進行ステータスが"200:手配"
                '回送進行ステータスが"210:手配中"
                If work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_100 _
                    OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_200 _
                    OrElse work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_210 Then
                    For Each rowitem As TableRow In tblObj.Rows
                        For Each cellObj As TableCell In rowitem.Controls
                            If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALDEPDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALARRDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALACCDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALEMPARRDATE") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                            End If
                        Next
                    Next

                    '受注進行ステータスが"250：手配完了"以降のステータスの場合
                Else
                    '### ★選択（チェックボックス）を非活性にするための準備 ################
                    Dim chkObj As CheckBox = Nothing
                    '　LINECNTを除いたチェックボックスID
                    Dim chkObjIdWOLincnt As String = "chk" & pnlListArea1.ID & "OPERATION"
                    '　LINECNTを含むチェックボックスID
                    Dim chkObjId As String
                    'Dim chkObjType As String
                    '　ループ内の対象データROW(これでXXX項目の値をとれるかと）
                    Dim loopdr As DataRow = Nothing
                    '　データテーブルの行Index
                    Dim rowIdx As Integer = 0
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

                        For Each cellObj As TableCell In rowitem.Controls

                            If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "SHIPORDER") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "TANKNO") Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                            End If

                            '### 目的が"24:疎開留置", "25:移動"の場合は、受入日の入力を省略する ###########
                            If Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_24 _
                                OrElse Me.TxtObjective.Text = BaseDllConst.CONST_OBJECTCODE_25 Then

                                '(実績)発日, (実績)着日, (実績)空車着日を入力可能とする。
                                If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALDEPDATE") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALARRDATE") _
                                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALEMPARRDATE") Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")

                                    '(実績)受入日は入力不可とする。
                                ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALACCDATE") Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")

                                End If
                                '############################################################################

                            Else
                                '(実績)のすべてを入力可能とする。
                                If cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALDEPDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALARRDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALACCDATE") _
                                OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea1.ID & "ACTUALEMPARRDATE") Then
                                    cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                                End If

                            End If

                        Next
                        rowIdx += 1
                    Next
                End If

            '費用入力
            Case 1
                '〇 (一覧)テキストボックスの制御(読取専用)
                Dim divObj = DirectCast(pnlListArea2.FindControl(pnlListArea2.ID & "_DR"), Panel)
                Dim tblObj = DirectCast(divObj.Controls(0), Table)

                '〇 回送進行ステータスの状態
                Select Case work.WF_SEL_KAISOUSTATUS.Text
                '回送進行ステータス＝"200:手配中"
                '回送進行ステータス＝"210:手配中(入換指示入力済)"
                '回送進行ステータス＝"220:手配中(積込指示入力済)"
                '回送進行ステータス＝"230:手配中(託送指示手配済)"
                '回送進行ステータス＝"240:手配中(入換指示未入力)"
                '回送進行ステータス＝"250:手配中(積込指示未入力)"
                '回送進行ステータス＝"260:手配中(託送指示未手配)"
                '### START (20200330)入換・積込業者との進捗管理を実施する運用追加対応 #######
                '回送進行ステータス＝"270:手配中(入換積込指示手配済)"
                '回送進行ステータス＝"280:手配中(託送指示未手配)"入換積込手配連絡（手配・結果受理）
                '回送進行ステータス＝"290:手配中(入換積込未連絡)"
                '回送進行ステータス＝"300:手配中(入換積込未確認)"
                '### END   ##################################################################
                    Case BaseDllConst.CONST_KAISOUSTATUS_200,
                         BaseDllConst.CONST_KAISOUSTATUS_210,
                         BaseDllConst.CONST_KAISOUSTATUS_250,
                         BaseDllConst.CONST_KAISOUSTATUS_300

                    Case Else

                End Select

        End Select

    End Sub

    ''' <summary>
    ''' タンク車所在設定処理
    ''' </summary>
    Protected Sub WW_TankShozaiSet()

        '〇タンク車所在の更新
        '回送進行ステータスが以下の場合
        '100:回送受付
        If work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_100 Then

            '### 特になし ###############################################################

            '回送進行ステータスが以下の場合
            '200:手配
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_200 Then

            '★タンク車所在の更新
            '引数１：所在地コード　⇒　変更なし(空白)
            '引数２：タンク車状態　⇒　変更あり("1"(発送))
            '引数３：積車区分　　　⇒　変更なし(空白)
            '引数４：(予定)空車着日⇒　更新対象(画面項目)
            '引数５：タンク車状況　⇒　変更あり("3"(回送中))
            WW_UpdateTankShozai("", "1", "", upEmparrDate:=True, I_SITUATION:="3")

            '回送進行ステータスが以下の場合
            '210:手配中
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_210 Then

            '### 特になし ###############################################################

            '回送進行ステータスが「250:手配完了」の場合
            '250:手配完了
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_250 Then

            '### 特になし ###############################################################

            '回送進行ステータスが「300:回送確定」の場合
            '300:回送確定
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_300 Then

            '### 特になし ###############################################################

            '(実績)発日の入力が完了
            If Me.TxtActualDepDate.Text <> "" Then
                '★タンク車所在の更新
                '引数１：所在地コード　⇒　変更あり(着駅)
                '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
                '引数３：積車区分　　　⇒　変更なし(空白)
                WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "2", "")
            End If

            '回送進行ステータスが「350:回送確定」の場合
            '350:回送確定
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_350 Then
            '引数１：所在地コード　⇒　変更あり(着駅)
            '引数２：タンク車状態　⇒　変更あり("2"(到着予定))
            '引数３：積車区分　　　⇒　変更なし(空白)
            WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "2", "")

            '回送進行ステータスが「400:受入確認中」の場合
            '400:受入確認中
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_400 Then

            '### 特になし ###############################################################

            '(実績)受入日の入力が完了
            If Me.TxtActualAccDate.Text <> "" Then
                '★タンク車所在の更新
                '引数１：所在地コード　⇒　変更あり(着駅)
                '引数２：タンク車状態　⇒　変更あり("3"(到着))
                '引数３：積車区分　　　⇒　変更なし(空白)
                WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "3", "")
            End If

            '回送進行ステータスが「450:受入確認中」の場合
            '450:受入確認中
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_450 Then
            '★タンク車所在の更新
            '引数１：所在地コード　⇒　変更あり(着駅)
            '引数２：タンク車状態　⇒　変更あり("3"(到着))
            '引数３：積車区分　　　⇒　変更なし(空白)
            WW_UpdateTankShozai(Me.TxtArrstationCode.Text, "3", "")

            '回送進行ステータスが「500:検収中」の場合
            '500:検収中
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_500 Then

            '割り当てたタンク車のチェック
            Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            For Each OIT0006row As DataRow In OIT0006tbl.Rows
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

                '★(一覧)タンク車NoがOT本社、または在日米軍のリース車かチェック
                WW_FixvalueMasterSearch("ZZ", "TANKNO_OTCHECK", OIT0006row("TANKNO"), WW_GetValue)

                'タンク車がOT本社、または在日米軍のリース車の場合
                If WW_GetValue(0) <> "" Then

                    '### 特に何もしない ####################################

                Else

                    '回送画面の目的が"5:疎開留置"の場合
                    If Me.TxtObjective.Text = "5" Then

                        Dim strOfficeCode As String = ""

                        Select Case Me.TxtKaisouOrderOfficeCode.Text
                            Case BaseDllConst.CONST_OFFICECODE_010402
                                '東北支店
                                strOfficeCode = BaseDllConst.CONST_OFFICECODE_010401

                            Case BaseDllConst.CONST_OFFICECODE_011201,
                                 BaseDllConst.CONST_OFFICECODE_011202,
                                 BaseDllConst.CONST_OFFICECODE_011203,
                                 BaseDllConst.CONST_OFFICECODE_011402
                                '関東支店
                                strOfficeCode = BaseDllConst.CONST_OFFICECODE_011401

                            Case BaseDllConst.CONST_OFFICECODE_012401,
                                 BaseDllConst.CONST_OFFICECODE_012402
                                '中部支店
                                strOfficeCode = BaseDllConst.CONST_OFFICECODE_012301
                        End Select

                        '★タンク車所在の更新
                        '引数１：所在地コード　　　⇒　変更なし(空白)
                        '引数２：タンク車状態　　　⇒　変更あり("3"(到着))
                        '引数３：積車区分　　　　　⇒　変更なし(空白)
                        '引数４：所属営業所コード　⇒　変更なし(支店)
                        '引数５：タンク車№　　　　⇒　指定あり
                        WW_UpdateTankShozai("", "3", "", I_OFFICE:=strOfficeCode, I_TANKNO:=OIT0006row("TANKNO"))

                    Else
                        '★タンク車所在の更新
                        '引数１：所在地コード　　　⇒　変更あり(発駅)
                        '引数２：タンク車状態　　　⇒　変更あり("3"(到着))
                        '引数３：積車区分　　　　　⇒　変更なし(空白)
                        '引数４：タンク車№　　　　⇒　指定あり
                        '引数５：タンク車状況　　　⇒　変更あり("1"(残車))
                        WW_UpdateTankShozai(Me.TxtDepstationCode.Text, "3", "", I_TANKNO:=OIT0006row("TANKNO"), I_SITUATION:="1")

                    End If


                End If
            Next

            '550:検収済
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_550 Then
            '### 特になし ###############################################################

            '600:費用確定
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_600 Then
            '### 特になし ###############################################################

            '700:経理未計上
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_700 Then
            '### 特になし ###############################################################

            '800:経理計上
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_800 Then
            '### 特になし ###############################################################

            '900:回送キャンセル
        ElseIf work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_900 Then
            '### 特になし ###############################################################

        End If

    End Sub

    ''' <summary>
    ''' (回送TBL)回送進行ステータス更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateKaisouStatus(ByVal I_Value As String)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･回送TBLの回送進行ステータスを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0006_KAISOU " _
                    & "    SET KAISOUSTATUS = @P03, " _
                    & "        UPDYMD       = @P11, " _
                    & "        UPDUSER      = @P12, " _
                    & "        UPDTERMID    = @P13, " _
                    & "        RECEIVEYMD   = @P14  " _
                    & "  WHERE KAISOUNO     = @P01  " _
                    & "    AND DELFLG      <> @P02; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA01.Value = work.WF_SEL_KAISOUNUMBER.Text
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D_KAISOUSTATUS UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D_KAISOUSTATUS UPDATE"
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
    ''' (回送TBL)回送情報更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateKaisouInfo(ByVal I_TYPE As String, ByVal OIT0006row As DataRow)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr As String = ""
            '更新SQL文･･･回送TBLの回送情報を更新
            If I_TYPE = "1" Then
                SQLStr =
                " UPDATE OIL.OIT0006_KAISOU " _
                & "    SET KAISOUINFO  = @P04, " _
                & "        UPDYMD      = @P11, " _
                & "        UPDUSER     = @P12, " _
                & "        UPDTERMID   = @P13, " _
                & "        RECEIVEYMD  = @P14  " _
                & "  WHERE KAISOUNO    = @P01  " _
                & "    AND DELFLG     <> @P03; "

                '更新SQL文･･･回送明細TBLの回送情報を更新
            ElseIf I_TYPE = "2" Then
                SQLStr =
                " UPDATE OIL.OIT0007_KAISOUDETAIL " _
                & "    SET KAISOUINFO  = @P04, " _
                & "        UPDYMD      = @P11, " _
                & "        UPDUSER     = @P12, " _
                & "        UPDTERMID   = @P13, " _
                & "        RECEIVEYMD  = @P14  " _
                & "  WHERE KAISOUNO    = @P01  " _
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

            PARA01.Value = OIT0006row("KAISOUNO")
            PARA02.Value = OIT0006row("DETAILNO")
            PARA03.Value = C_DELETE_FLG.DELETE
            PARA04.Value = OIT0006row("KAISOUINFO")

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D_KAISOUINFO UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D_KAISOUINFO UPDATE"
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
    ''' (タンク車所在TBL)所在地の内容を更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateTankShozai(ByVal I_LOCATION As String,
                                      ByVal I_STATUS As String,
                                      ByVal I_KBN As String,
                                      Optional ByVal I_OFFICE As String = Nothing,
                                      Optional ByVal I_SITUATION As String = Nothing,
                                      Optional ByVal I_TANKNO As String = Nothing,
                                      Optional ByVal upEmparrDate As Boolean = False,
                                      Optional ByVal upActualEmparrDate As Boolean = False)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLの託送指示フラグを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0005_SHOZAI " _
                    & "    SET "

            '○ 更新内容が指定されていれば追加する
            '所属営業所コード
            If Not String.IsNullOrEmpty(I_LOCATION) Then
                SQLStr &= String.Format("        OFFICECODE   = '{0}', ", I_OFFICE)
            End If
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
            '空車着日（予定）
            If upEmparrDate = True Then
                SQLStr &= String.Format("        EMPARRDATE   = '{0}', ", Me.TxtEmparrDate.Text)
                SQLStr &= String.Format("        ACTUALEMPARRDATE   = {0}, ", "NULL")
            End If
            '空車着日（実績）
            If upActualEmparrDate = True Then
                SQLStr &= String.Format("        ACTUALEMPARRDATE   = '{0}', ", Me.TxtActualEmparrDate.Text)
            End If

            SQLStr &=
                      "        UPDYMD       = @P11, " _
                    & "        UPDUSER      = @P12, " _
                    & "        UPDTERMID    = @P13, " _
                    & "        RECEIVEYMD   = @P14  " _
                    & "  WHERE TANKNUMBER   = @P01  " _
                    & "    AND DELFLG      <> @P02; "

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
                '(一覧)で設定しているタンク車をKEYに更新
                For Each OIT0006row As DataRow In OIT0006tbl.Rows
                    PARA01.Value = OIT0006row("TANKNO")
                    SQLcmd.ExecuteNonQuery()
                Next
            Else
                '指定されたタンク車№をKEYに更新
                PARA01.Value = I_TANKNO
                SQLcmd.ExecuteNonQuery()

            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D_TANKSHOZAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D_TANKSHOZAI UPDATE"
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
    ''' (回送TBL)フラグ関連更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateRelatedFlg(ByVal I_Value As String, Optional ByVal I_PARA01 As String = Nothing)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･回送TBLの各フラグを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0006_KAISOU " _
                    & "    SET UPDYMD      = @P11, " _
                    & "        UPDUSER     = @P12, " _
                    & "        UPDTERMID   = @P13, " _
                    & "        RECEIVEYMD  = @P14, "

            SQLStr &= String.Format("        {0}   = @P03 ", I_PARA01)

            SQLStr &=
                    "  WHERE KAISOUNO     = @P01  " _
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

            PARA01.Value = work.WF_SEL_KAISOUNUMBER.Text
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D_" + I_PARA01 + "UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D_" + I_PARA01 + "UPDATE"
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
    ''' 画面表示設定処理(回送進行ステータス(変更分を反映))
    ''' </summary>
    Protected Sub WW_ScreenKaisouStatusChgRef(ByVal O_VALUE As String)

        '回送進行ステータスに変更があった場合
        If O_VALUE <> "" Then
            '〇(回送TBL)回送進行ステータス更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateKaisouStatus(O_VALUE)
                CODENAME_get("KAISOUSTATUS", O_VALUE, Me.TxtKaisouStatus.Text, WW_DUMMY)
                work.WF_SEL_KAISOUSTATUS.Text = O_VALUE
                work.WF_SEL_KAISOUSTATUSNM.Text = Me.TxtKaisouStatus.Text

            End Using

            '○ 画面表示データ復元
            Master.RecoverTable(OIT0006WKtbl, work.WF_SEL_INPTBL.Text)

            For Each OIT0006row As DataRow In OIT0006WKtbl.Rows
                If OIT0006row("KAISOUNO") = work.WF_SEL_KAISOUNUMBER.Text Then
                    OIT0006row("KAISOUSTATUS") = O_VALUE
                    OIT0006row("KAISOUSTATUSNAME") = Me.TxtKaisouStatus.Text
                End If
            Next

            '○ 画面表示データ保存
            Master.SaveTable(OIT0006WKtbl, work.WF_SEL_INPTBL.Text)
        End If

        '〇 受注ステータスが"250:手配完了"へ変更された場合
        If work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_250 Then
            'WF_DTAB_CHANGE_NO.Value = "2"
            'WF_Detail_TABChange()

            '### START 回送履歴テーブルの追加(2020/03/26) #############
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_InsertKaisouHistory(SQLcon)
            End Using
            '### END   ################################################

            '○メッセージ表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

    End Sub

    ''' <summary>
    ''' 回送履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon">SQL接続</param>
    Private Sub WW_InsertKaisouHistory(ByVal SQLcon As SqlConnection)
        Dim WW_GetHistoryNo() As String = {""}
        WW_FixvalueMasterSearch("", "NEWHISTORYNOGET", "", WW_GetHistoryNo)

        '◯回送履歴テーブル格納用
        If IsNothing(OIT0006His1tbl) Then
            OIT0006His1tbl = New DataTable
        End If

        If OIT0006His1tbl.Columns.Count <> 0 Then
            OIT0006His1tbl.Columns.Clear()
        End If
        OIT0006His1tbl.Clear()

        '◯回送明細履歴テーブル格納用
        If IsNothing(OIT0006His2tbl) Then
            OIT0006His2tbl = New DataTable
        End If

        If OIT0006His2tbl.Columns.Count <> 0 Then
            OIT0006His2tbl.Columns.Clear()
        End If
        OIT0006His2tbl.Clear()

        '○ 回送TBL検索SQL
        Dim SQLOrderStr As String =
            "SELECT " _
            & String.Format("   '{0}' AS HISTORYNO", WW_GetHistoryNo(0)) _
            & String.Format(" , '{0}' AS MAPID", Me.Title) _
            & " , OIT0006.*" _
            & " FROM OIL.OIT0006_KAISOU OIT0006 " _
            & String.Format(" WHERE OIT0006.KAISOUNO = '{0}'", work.WF_SEL_KAISOUNUMBER.Text)

        '○ 回送明細TBL検索SQL
        Dim SQLOrderDetailStr As String =
            "SELECT " _
            & String.Format("   '{0}' AS HISTORYNO", WW_GetHistoryNo(0)) _
            & String.Format(" , '{0}' AS MAPID", Me.Title) _
            & " , OIT0007.*" _
            & " FROM OIL.OIT0007_KAISOUDETAIL OIT0007 " _
            & String.Format(" WHERE OIT0007.KAISOUNO = '{0}'", work.WF_SEL_KAISOUNUMBER.Text)

        Try
            Using SQLcmd As New SqlCommand(SQLOrderStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0006His1tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006His1tbl.Load(SQLdr)
                End Using
            End Using

            Using SQLcmd As New SqlCommand(SQLOrderDetailStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0006His2tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006His2tbl.Load(SQLdr)
                End Using
            End Using

            Using tran = SQLcon.BeginTransaction
                '■回送履歴テーブル
                EntryHistory.InsertKaisouHistory(SQLcon, tran, OIT0006His1tbl.Rows(0))

                '■回送明細履歴テーブル
                For Each OIT0001His2rowtbl In OIT0006His2tbl.Rows
                    EntryHistory.InsertKaisouDetailHistory(SQLcon, tran, OIT0001His2rowtbl)
                Next

                'トランザクションコミット
                tran.Commit()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D KAISOUHISTORY")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D KAISOUHISTORY"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

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

        If IsNothing(OIT0006Fixvaltbl) Then
            OIT0006Fixvaltbl = New DataTable
        End If

        If OIT0006Fixvaltbl.Columns.Count <> 0 Then
            OIT0006Fixvaltbl.Columns.Clear()
        End If

        OIT0006Fixvaltbl.Clear()

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
                        OIT0006Fixvaltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006Fixvaltbl.Load(SQLdr)
                End Using

                If I_KEYCODE.Equals("") Then
                    'Dim i As Integer = 0 '2020/3/23 三宅 Delete
                    For Each OIT0006WKrow As DataRow In OIT0006Fixvaltbl.Rows '(全抽出結果回るので要検討
                        'O_VALUE(i) = OIT0006WKrow("KEYCODE") 2020/3/23 三宅 全部KEYCODE(列車NO)が格納されてしまうので修正しました（問題なければこのコメント消してください)
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0006WKrow("VALUE" & i.ToString())
                        Next
                        'i += 1 '2020/3/23 三宅 Delete
                    Next
                Else
                    For Each OIT0006WKrow As DataRow In OIT0006Fixvaltbl.Rows

                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0006WKrow("VALUE" & i.ToString())
                        Next
                    Next
                End If
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
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

                Case "ORDERINFO"        '受注情報
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERINFO, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERINFO"))

                Case "KAISOUSTATUS"     '回送進行ステータス
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUSTATUS, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "KAISOUSTATUS"))

                Case "KAISOUINFO"       '回送情報
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUINFO, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "KAISOUINFO"))

                Case "SALESOFFICE"      '営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "KAISOUPATTERN"    '回送パターン
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_KAISOUSALESOFFICECODE.Text, "KAISOUPATTERN"))

                Case "DEPSTATION"       '発駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPSTATION"))

                Case "ARRSTATION"       '着駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ARRSTATION"))

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
        For Each OIT0006row As DataRow In OIT0006tbl.Rows
            Select Case OIT0006row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

        'WF_Sel_LINECNT.Text = ""            'LINECNT

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
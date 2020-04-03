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
                    'DisplayGrid()
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
        'GridViewInitialize()

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


        '### 画面に設定する内容を書く ##################################


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
        'WW_ScreenEnabledSet()

        '〇タブ「タンク車割当」表示用
        GridViewInitializeTab1()

        '〇タブ「費用入力」表示用
        GridViewInitializeTab2()

        ''〇タンク車所在の更新
        'WW_TankShozaiSet()

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(タブ「タンク車割当」表示用)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitializeTab1()
        ''○ 画面表示データ取得
        'Using SQLcon As SqlConnection = CS0050SESSION.getConnection
        '    SQLcon.Open()       'DataBase接続

        '    MAPDataGet(SQLcon, 0)
        'End Using

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
        'WW_ListTextBoxReadControl()

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
        'WW_ScreenEnabledSet()

        '〇タンク車所在の更新
        'WW_TankShozaiSet()

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
        'WW_ListTextBoxReadControl()

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
        'WW_ListTextBoxReadControl()

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
    ''' XXXXXボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

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
    ''' 明細を作るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDETAIL_Click()

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
            'WW_CheckERR("回送登録営業所が未選択。", C_MESSAGE_NO.OIL_KAISOUOFFICE_UNSELECT)
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
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(回送登録営業所).テキストボックスが未設定
                            If Me.TxtKaisouOrderOffice.Text = "" Then
                                prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "1", Me.TxtDepstationCode.Text)
                            Else
                                prmData = work.CreateSTATIONPTParam(Me.TxtKaisouOrderOfficeCode.Text + "1", Me.TxtDepstationCode.Text)
                            End If
                        Else
                            prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "1", Me.TxtDepstationCode.Text)
                        End If
                    End If

                    '着駅
                    If WF_FIELD.Value = "TxtArrstationCode" Then
                        '〇 検索(営業所).テキストボックスが未設定
                        If work.WF_SEL_SALESOFFICECODE.Text = "" Then
                            '〇 画面(回送登録営業所).テキストボックスが未設定
                            If Me.TxtKaisouOrderOffice.Text = "" Then
                                prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "2", Me.TxtArrstationCode.Text)
                            Else
                                prmData = work.CreateSTATIONPTParam(Me.TxtKaisouOrderOfficeCode.Text + "2", Me.TxtArrstationCode.Text)
                            End If
                        Else
                            prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "2", Me.TxtArrstationCode.Text)
                        End If
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
                    'work.WF_SEL_ORDERSALESOFFICECODE.Text = WW_SelectValue
                    'work.WF_SEL_ORDERSALESOFFICE.Text = WW_SelectText

                    '○ テキストボックスを初期化
                    '回送パターン
                    Me.TxtKaisouType.Text = ""
                    '本線列車
                    Me.TxtTrainNo.Text = ""
                    'タンク車数
                    Me.TxtTankCnt.Text = ""
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
                        'MAPDataGet(SQLcon, 0)
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

            '発駅
            Case "TxtDepstationCode"
                Me.TxtDepstationCode.Text = WW_SelectValue
                Me.LblDepstationName.Text = WW_SelectText
                Me.TxtDepstationCode.Focus()

            '着駅
            Case "TxtDepstationCode"
                Me.TxtDepstationCode.Text = WW_SelectValue
                Me.LblDepstationName.Text = WW_SelectText
                Me.TxtDepstationCode.Focus()


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
    ''' 行削除ボタン押下時処理(タブ「費用入力」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB2()

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理(タブ「タンク車割当」)
    ''' </summary>
    Protected Sub WW_ButtonLINE_LIFTED_TAB1()

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
            '割当確定ボタン押下時
            WW_ButtonUPDATE_TAB1()

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

                Case "KAISOUSTATUS"     '回送進行ステータス
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUSTATUS, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "KAISOUSTATUS"))

                Case "KAISOUINFO"       '回送情報
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUINFO, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "KAISOUINFO"))

                Case "SALESOFFICE"      '営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "KAISOUTYPE"       '回送パターン
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "KAISOUTYPE"))

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
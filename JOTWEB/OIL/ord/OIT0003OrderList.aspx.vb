'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 受注一覧画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0003OrderList
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0003tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0003INPtbl As DataTable                              'チェック用テーブル
    Private OIT0003UPDtbl As DataTable                              '更新用テーブル
    Private OIT0003EXLUPtbl As DataTable                            'EXCELアップロード用
    Private OIT0003EXLDELtbl As DataTable                           'EXCELアップロード(削除)用
    Private OIT0003EXLINStbl As DataTable                           'EXCELアップロード(追加(回線別積込取込(日新)TBL))用
    Private OIT0003EXLCHKtbl As DataTable                           'EXCELアップロード(チェック)用
    Private OIT0003EXLODRtbl As DataTable                           'EXCELアップロード(受注データ取込(日新)TBL))用
    Private OIT0003EXLODRALLtbl As DataTable                        'EXCELアップロード(受注データ取込(日新)TBL))用
    Private OIT0003WKtbl As DataTable                               '作業用テーブル
    Private OIT0003Fixvaltbl As DataTable                           '作業用テーブル(固定値マスタ取得用)
    Private OIT0003His1tbl As DataTable                             '履歴格納用テーブル
    Private OIT0003His2tbl As DataTable                             '履歴格納用テーブル
    Private OIT0003Reporttbl As DataTable                           '帳票用テーブル
    Private OIT0003ReportDeliverytbl As DataTable                   '帳票用(託送指示)テーブル
    Private OIT0003ReportSendaitbl As DataTable                     '帳票用(仙台新港)テーブル
    Private OIT0003ReportGoitbl As DataTable                        '帳票用(五井)テーブル
    Private OIT0003ReportKinoenetbl As DataTable                    '帳票用(甲子)テーブル
    Private OIT0003ReportSodegauratbl As DataTable                  '帳票用(袖ヶ浦)テーブル
    Private OIT0003ReportMieShiohamatbl As DataTable                '帳票用(三重塩浜)テーブル
    Private OIT0003ReportNegishitbl As DataTable                    '帳票用(根岸)テーブル
    Private OIT0003ReportPlanFrame As DataTable                     '帳票用(計画枠用)テーブル
    Private OIT0003ReportReserveAmount As DataTable                 '帳票用(予約数量枠用)テーブル
    Private OIT0003ReportOilDuration As DataTable                   '帳票用(油種期間枠用)テーブル
    Private OIT0003CsvDeliverytbl As DataTable                      'CSV用(託送指示)テーブル
    Private OIT0003ItemGettbl As DataTable                          '値取得用テーブル
    Private OIT0003CsvActualLoadtbl As DataTable                    'CSV用(積込実績)テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー

    '◯ 条件用
    Private Const CONST_SODE_TRAIN_8877 As String = "8877"          '袖ヶ浦営業所(列車番号：8877)
    Private Const CONST_SODE_TRAIN_8883 As String = "8883"          '袖ヶ浦営業所(列車番号：8883)
    Private Const CONST_SODE_TRAIN_5461 As String = "5461"          '袖ヶ浦営業所(列車番号：5461)
    Private Const CONST_SODE_TRAIN_5972 As String = "5972"          '袖ヶ浦営業所(列車番号：5972)
    Private Const CONST_SODE_TRAIN_9672 As String = "9672"          '袖ヶ浦営業所(列車番号：9672)
    Private Const CONST_SHIPPERSCODE_0122700010 As String = "0122700010"   '出光昭和シェル(荷主コード : 0122700010)

    '○ 帳票用
    Private Const CONST_RPT_LOADPLAN As String = "LOADPLAN"                     '積込予定(共通)
    Private Const CONST_RPT_LOADPLAN_KINOENE As String = "KINOENE_LOADPLAN"     '積込予定(甲子)
    Private Const CONST_RPT_LOADPLAN_NEGISHI As String = "NEGISHI_LOADPLAN"     '積込予定(根岸)
    Private Const CONST_RPT_OTLOADPLAN As String = "OTLOADPLAN"                 'OT積込予定(共通)
    Private Const CONST_RPT_SHIPPLAN As String = "SHIPPLAN"                     '出荷予定
    Private Const CONST_RPT_LINEPLAN As String = "LINEPLAN"                     '入線予定(袖ヶ浦)
    Private Const CONST_RPT_DELIVERYPLAN As String = "DELIVERYPLAN"             '託送指示
    Private Const CONST_RPT_KUUKAI_SODEGAURA As String = "KUUKAI_SODEGAURA"     '空回日報(袖ヶ浦)
    Private Const CONST_RPT_FILLINGPOINT As String = "FILLINGPOINT"             '充填ポイント表
    Private Const CONST_RPT_TANKDISPATCH As String = "TANKDISPATCH"             'タンク車発送実績
    Private Const CONST_RPT_TANKDISPATCH_30 As String = "TANKDISPATCH_30"       'タンク車発送実績(コウショウ高崎)
    Private Const CONST_RPT_TANKDISPATCH_40 As String = "TANKDISPATCH_40"       'タンク車発送実績(JONET松本)
    Private Const CONST_RPT_TANKDISPATCH_51 As String = "TANKDISPATCH_51"       'タンク車発送実績(OT盛岡)
    Private Const CONST_RPT_TANKDISPATCH_54 As String = "TANKDISPATCH_54"       'タンク車発送実績(OT高崎/構内取り)
    Private Const CONST_RPT_ACTUALSHIP As String = "ACTUALSHIP"                 '発送実績
    Private Const CONST_RPT_CONCATORDER As String = "CONCATORDER"               '連結順序表
    Private Const CONST_RPT_SHIPCONTACT As String = "SHIPCONTACT"               'タンク車出荷連絡書
    Private Const CONST_CSV_ACTUALLOAD_10 As String = "ACTUALLOAD_10"           '積込実績(北信)
    Private Const CONST_CSV_ACTUALLOAD_20 As String = "ACTUALLOAD_20"           '積込実績(甲府)

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private RSSQL As New ReportSignSQL                              '帳票表示用SQL取得
    Private CMNPTS As New CmnParts                                  '共通関数

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    'サブ用リターンコード
    Private WW_ORDERSTATUS As String = ""                           '受注進行ステータス

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0003tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_CheckBoxSELECT"        'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonORDER_CANCEL"    'キャンセルボタン押下
                            WF_ButtonORDER_CANCEL_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonDetailDownload"  '明細ダウンロードボタン押下
                            WF_ButtonDetailDownload_Click()
                        Case "WF_ButtonINSERT"          '受注新規作成ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonLinkINSERT"      '貨車連結選択ボタン押下
                            WF_ButtonLinkINSERT_Click()
                        Case "WF_ButtonOTLinkageINSERT" 'OT連携選択ボタン押下
                            WF_ButtonOTLinkageINSERT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "btnCommonConfirmOk"       '確認メッセージ
                            WW_UpdateOrderStatusCancel()
                        Case "tileSalesOffice"          '帳票ポップアップ(営業所(チェックボックス)選択)
                            WF_TyohyoSalesOfficeSelect()
                            WF_ReportSelect()
                        Case "tileReport"               '帳票ポップアップ(帳票(チェックボックス)選択)
                            WF_ReportSelect()
                        Case "WF_ButtonOkCommonPopUp"   '帳票ポップアップ(ダウンロードボタン押下)
                            WF_TyohyoDownloadClick()
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

            '◯ ボタン(表示・非表示)設定(各営業所にて出し分けをするため)
            WF_BUTTONofficecode.Value = Master.USER_ORG
            Select Case Master.USER_ORG
                '★情報システム部/石油部
                Case BaseDllConst.CONST_OFFICECODE_010006,
                     BaseDllConst.CONST_OFFICECODE_010007
                    WF_BUTTONpermitcode.Value = "0"
                '★東北支店/仙台新港
                Case BaseDllConst.CONST_OFFICECODE_010401,
                     BaseDllConst.CONST_OFFICECODE_010402
                    WF_BUTTONpermitcode.Value = "1"
                '★関東支店/五井/甲子/袖ヶ浦/根岸
                Case BaseDllConst.CONST_OFFICECODE_011401,
                     BaseDllConst.CONST_OFFICECODE_011201,
                     BaseDllConst.CONST_OFFICECODE_011202,
                     BaseDllConst.CONST_OFFICECODE_011203,
                     BaseDllConst.CONST_OFFICECODE_011402
                    WF_BUTTONpermitcode.Value = "2"
                '★中部支店/四日市/三重塩浜
                Case BaseDllConst.CONST_OFFICECODE_012301,
                     BaseDllConst.CONST_OFFICECODE_012401,
                     BaseDllConst.CONST_OFFICECODE_012402
                    WF_BUTTONpermitcode.Value = "3"
            End Select

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
        Master.MAPID = OIT0003WRKINC.MAPIDL
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

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003D Then
            Master.RecoverTable(OIT0003tbl, work.WF_SEL_INPTBL.Text)
        End If

        '### 帳票ポップアップの設定 ###################################################################
        '〇仮置き
        'Dim paramData As Hashtable = work.CreateSALESOFFICEParam(Master.USER_ORG, dummyTxtSalesOffice)
        Dim paramData As Hashtable = work.CreateFIXParam(Master.USER_ORG)
        Me.tileSalesOffice.ListBoxClassification = LIST_BOX_CLASSIFICATION.LC_SALESOFFICE
        Me.tileSalesOffice.ParamData = paramData
        Me.tileSalesOffice.LeftObj = leftview
        Me.tileSalesOffice.SelectionMode = ListSelectionMode.Single
        Me.tileSalesOffice.NeedsPostbackAfterSelect = True
        Me.tileSalesOffice.SetTileValues()

        'ラジオボタンを非表示にする。
        Me.rbDeliveryBtn.Visible = False
        Me.rbDeliveryCSVBtn.Visible = False
        Me.rbShipBtn.Visible = False
        Me.rbLoadBtn.Visible = False
        '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
        Me.rbOTLoadBtn.Visible = False
        '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
        Me.rbLineBtn.Visible = False
        Me.rbKinoeneLoadBtn.Visible = False
        Me.rbNegishiLoadBtn.Visible = False
        Me.rbActualLoad10Btn.Visible = False
        Me.rbActualLoad20Btn.Visible = False
        '### 20201106 START 指摘票No195(空回日報対応) #################################################
        Me.rbKuukaiBtn.Visible = False
        '### 20201106 END   指摘票No195(空回日報対応) #################################################
        Me.rbFillingPointBtn.Visible = False
        Me.rbTankDispatchBtn.Visible = False
        Me.rbTankDispatch30Btn.Visible = False
        Me.rbTankDispatch40Btn.Visible = False
        Me.rbTankDispatch54Btn.Visible = False
        Me.rbActualShipBtn.Visible = False
        Me.rbConcatOederBtn.Visible = False
        Me.rbShipContactBtn.Visible = False

        '(帳票)積込日に翌日を設定
        Me.txtReportLodDate.Text = Format(Now.AddDays(1), "yyyy/MM/dd")

        Me.divRTrainNo.Visible = False
        Me.divTrainNo.Visible = False
        Me.divEndMonthChk.Visible = False

        ''帳票のポップアップを閉じる
        'Master.HideCustomPopUp()
        '##############################################################################################

        ''○ 名称設定処理
        'CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_SEL_CAMPNAME.Text, WW_DUMMY)             '会社コード
        'CODENAME_get("UORG", work.WF_SEL_UORG.Text, WF_SELUORG_TEXT.Text, WW_DUMMY)                     '運用部署

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIT0003D Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
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
        CS0013ProfView.TBLOBJ = pnlListArea
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
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

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
            & " , ISNULL(RTRIM(OIT0002.EMPTYTURNFLG), '')   　       AS EMPTYTURNFLG" _
            & " , ISNULL(RTRIM(OIT0002.STACKINGFLG), '')   　        AS STACKINGFLG" _
            & " , ''                                                 AS STACKINGNAME" _
            & " , ISNULL(RTRIM(OIT0002.USEPROPRIETYFLG), '')   　    AS USEPROPRIETYFLG" _
            & " , ISNULL(RTRIM(OIT0002.CONTACTFLG), '')   　         AS CONTACTFLG" _
            & " , ISNULL(RTRIM(OIT0002.RESULTFLG), '')   　          AS RESULTFLG" _
            & " , ISNULL(RTRIM(OIT0002.DELIVERYFLG), '')   　        AS DELIVERYFLG" _
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')   　            AS ORDERNO" _
            & " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "   ELSE ISNULL(RTRIM(OIT0002.TRAINNO), '')" _
            & "   END                                                AS TRAINNO" _
            & " , CASE " _
            & "   WHEN OIT0002.OFFICECODE = '" + BaseDllConst.CONST_OFFICECODE_011201 + "' AND OIM0007.TSUMI = 'T' THEN" _
            & "       CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "       WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIM0007.OTTRAINNO), '') + '</div>'" _
            & "       WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIM0007.OTTRAINNO), '') + '</div>'" _
            & "       ELSE ISNULL(RTRIM(OIM0007.OTTRAINNO), '')" _
            & "       END" _
            & "   ELSE" _
            & "       CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "       WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "       WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "       ELSE ISNULL(RTRIM(OIT0002.TRAINNO), '')" _
            & "       END" _
            & "   END                                                AS OTTRAINNO" _
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
            & " , ISNULL(RTRIM(OIT0002.HTANK), '')                   AS HTANK" _
            & " , ISNULL(RTRIM(OIT0002.RTANK), '')                   AS RTANK" _
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
            & " , ISNULL(RTRIM(OIM0007.OTFLG), '')                   AS OTFLG" _
            & " , ISNULL(RTRIM(OIT0002.OTSENDSTATUS), '')            AS OTSENDSTATUS" _
            & " , CASE" _
            & "   WHEN OIM0007.OTFLG = '1' THEN ISNULL(RTRIM(OIS0015_3.VALUE1), '') " _
            & "   ELSE '対象外'" _
            & "   END                                                AS OTSENDSTATUSNAME" _
            & " , ISNULL(RTRIM(OIT0002.RESERVEDSTATUS), '')          AS RESERVEDSTATUS" _
            & " , CASE" _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_4.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_011201) _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_4.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_011202) _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_4.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_011203) _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_4.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_011402) _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_4.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_012401) _
            & "   ELSE '対象外'" _
            & "   END                                                AS RESERVEDSTATUSNAME" _
            & " , ISNULL(RTRIM(OIT0002.TAKUSOUSTATUS), '')           AS TAKUSOUSTATUS" _
            & " , CASE" _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_5.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_011201) _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_5.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_011202) _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_5.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_011203) _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_5.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_012401) _
            & String.Format("   WHEN OIT0002.OFFICECODE = '{0}' THEN ISNULL(RTRIM(OIS0015_5.VALUE1), '') ", BaseDllConst.CONST_OFFICECODE_012402) _
            & "   ELSE '対象外'" _
            & "   END                                                AS TAKUSOUSTATUSNAME" _
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
            & "  LEFT JOIN oil.OIM0007_TRAIN OIM0007 ON " _
            & "        OIM0007.OFFICECODE = OIT0002.OFFICECODE " _
            & "    AND OIM0007.TRAINNAME = OIT0002.TRAINNAME " _
            & "    AND OIM0007.DEFAULTKBN = 'def' "

        '### 20210405 START 受注一覧のソート順対応 #########################################
        SQLStr &=
              "  LEFT JOIN oil.OIM0029_CONVERT OIM0029 ON " _
            & "        OIM0029.CLASS = 'ORDERLIST_SORT' " _
            & "    AND OIM0029.KEYCODE01 = OIT0002.OFFICECODE " _
            & "    AND OIM0029.KEYCODE04 = OIT0002.TRAINNAME "
        '### 20210405 END   受注一覧のソート順対応 #########################################

        '### 20210409 START OT発送日報送信状況順対応 #######################################
        SQLStr &=
              "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_3 ON " _
            & "        OIS0015_3.CLASS   = 'OTSENDSTATUS' " _
            & "    AND OIS0015_3.KEYCODE = OIT0002.OTSENDSTATUS " _
        '### 20210409 END   OT発送日報送信状況順対応 #######################################
        '### 20210421 START 出荷予約対応 ###################################################
        SQLStr &=
              "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_4 ON " _
            & "        OIS0015_4.CLASS   = 'RESERVEDSTATUS' " _
            & "    AND OIS0015_4.KEYCODE = OIT0002.RESERVEDSTATUS " _
        '### 20210421 END   出荷予約対応 ###################################################
        '### 20210421 START 託送状対応 #####################################################
        SQLStr &=
              "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_5 ON " _
            & "        OIS0015_5.CLASS   = 'TAKUSOUSTATUS' " _
            & "    AND OIS0015_5.KEYCODE = ISNULL(OIT0002.TAKUSOUSTATUS,'0') " _
        '### 20210421 END   託送状対応 #####################################################

        SQLStr &=
              " WHERE OIT0002.DELFLG     <> @P3" _
            & "   AND OIT0002.LODDATE    >= @P2"

        '20210322(条件変更：五井営業所の場合、OT列車番号を表示)
        '& " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
        '& "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
        '& "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
        '& "   ELSE ISNULL(RTRIM(OIT0002.TRAINNO), '')" _
        '& "   END                                                AS TRAINNO" _

        '20200225(条件変更：登録年月日⇒(予定)積込日に変更)
        '& " WHERE OIT0002.ORDERYMD   >= @P2" _

        '& " , ISNULL(RTRIM(OIS0015_2.VALUE1), '')                AS ORDERINFONAME" _
        '& " , ISNULL(RTRIM(OIT0002.TRAINNO), '')                 AS TRAINNO" _
        '& " , ISNULL(RTRIM(OIT0002.TOTALTANK), '')               AS TOTALTANK" _


        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '営業所
        If Not String.IsNullOrEmpty(work.WF_SEL_SALESOFFICECODE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.OFFICECODE = '{0}'", work.WF_SEL_SALESOFFICECODE.Text)
        End If
        '積込日To('20210413(条件追加：(予定)積込日To))
        If Not String.IsNullOrEmpty(work.WF_SEL_DATE_TO.Text) Then
            SQLStr &= String.Format("    AND OIT0002.LODDATE <= '{0}'", work.WF_SEL_DATE_TO.Text)
        End If
        '発日('20200225(条件追加：(予定)発日))
        If Not String.IsNullOrEmpty(work.WF_SEL_SEARCH_DEPDATE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.DEPDATE >= '{0}'", work.WF_SEL_SEARCH_DEPDATE.Text)
        End If
        '列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNUMBER.Text) Then
            SQLStr &= String.Format("    AND OIT0002.TRAINNO = '{0}'", Integer.Parse(work.WF_SEL_TRAINNUMBER.Text))
        End If
        '荷卸地(荷受人)
        If Not String.IsNullOrEmpty(work.WF_SEL_UNLOADINGCODE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.CONSIGNEECODE = '{0}'", work.WF_SEL_UNLOADINGCODE.Text)
        End If
        '状態(受注進行ステータス)
        If Not String.IsNullOrEmpty(work.WF_SEL_STATUSCODE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.ORDERSTATUS = '{0}'", work.WF_SEL_STATUSCODE.Text)
        Else
            SQLStr &= String.Format("    AND OIT0002.ORDERSTATUS < '{0}'", BaseDllConst.CONST_ORDERSTATUS_500)
        End If

        '### 20201126 START 指摘票対応(No233)全体 ################################
        '受注キャンセルフラグ
        If work.WF_SEL_ORDERCANCELFLG.Text = "0" Then
            SQLStr &= String.Format("    AND OIT0002.ORDERSTATUS <> '{0}'", BaseDllConst.CONST_ORDERSTATUS_900)
        End If
        '### 20201126 END   指摘票対応(No233)全体 ################################

        '### 20210405 START 受注一覧のソート順対応 #########################################
        SQLStr &=
              " ORDER BY" _
            & "    OIT0002.OFFICECODE" _
            & " ,  OIT0002.LODDATE" _
            & " ,  OIM0029.KEYCODE05" _
            & " ,  OIM0029.KEYCODE06" _
            & " ,  OIT0002.ORDERNO"
        'SQLStr &=
        '      " ORDER BY" _
        '    & "    OIT0002.ORDERNO"
        '### 20210405 END   受注一覧のソート順対応 #########################################

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 10) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)     '積込日(開始)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = Master.USER_ORG
                PARA2.Value = work.WF_SEL_DATE.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim OTSENDSTATUSNAME As String() = {"未送信", "済", "一部送信済", "再送信済"}
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    i += 1
                    OIT0003row("LINECNT") = i        'LINECNT

                    '◯名称取得
                    '積置きフラグ
                    CODENAME_get("STACKING", OIT0003row("STACKINGFLG"), OIT0003row("STACKINGNAME"), WW_RTN_SW)

                    '### 20210712 START 指摘票No522(OT発送状況対応) ###############################################
                    '★OT発送状況(未送信)の場合は赤文字にする。
                    If Convert.ToString(OIT0003row("OTSENDSTATUSNAME")) = OTSENDSTATUSNAME(0) Then
                        OIT0003row("OTSENDSTATUSNAME") = String.Format("<div class=""caution_letter"">{0}</div>", OTSENDSTATUSNAME(0))
                    End If
                    '### 20210712 END   指摘票No522(OT発送状況対応) ###############################################

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

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
                If OIT0003tbl.Rows(i)("OPERATION") = "" Then
                    If (OIT0003tbl.Rows(i)("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_900 _
                             OrElse OIT0003tbl.Rows(i)("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_500 _
                             OrElse OIT0003tbl.Rows(i)("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_550 _
                             OrElse OIT0003tbl.Rows(i)("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_600 _
                             OrElse OIT0003tbl.Rows(i)("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_700 _
                             OrElse OIT0003tbl.Rows(i)("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_800) Then
                        OIT0003tbl.Rows(i)("OPERATION") = ""

                    Else
                        OIT0003tbl.Rows(i)("OPERATION") = "on"

                    End If
                Else
                    OIT0003tbl.Rows(i)("OPERATION") = ""
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0003tbl.Rows.Count - 1
            If OIT0003tbl.Rows(i)("HIDDEN") = "0" AndAlso OIT0003tbl.Rows(i)("ORDERSTATUS") <> BaseDllConst.CONST_ORDERSTATUS_900 Then
                OIT0003tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

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
    ''' キャンセルボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonORDER_CANCEL_Click()

        Dim SelectChk As Boolean = False
        Dim intTblCnt As Integer = 0

        '件数を取得
        intTblCnt = OIT0003tbl.Rows.Count

        '行が選択されているかチェック
        For Each OIT0003UPDrow In OIT0003tbl.Rows
            If OIT0003UPDrow("OPERATION") = "on" Then
                If OIT0003UPDrow("ORDERSTATUS") <> BaseDllConst.CONST_ORDERSTATUS_900 Then
                    SelectChk = True
                End If
            End If
        Next

        '○メッセージ表示
        '一覧件数が０件の時のキャンセルの場合
        If intTblCnt = 0 Then
            Master.Output(C_MESSAGE_NO.OIL_CANCELDATA_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub

            '一覧件数が１件以上で未選択によるキャンセルの場合
        ElseIf SelectChk = False Then
            Master.Output(C_MESSAGE_NO.OIL_CANCELLINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '◯確認メッセージ(受注キャンセルの確認)
        Master.Output(C_MESSAGE_NO.OIL_CONFIRM_CANCEL_ORDER,
                      C_MESSAGE_TYPE.QUES,
                      needsPopUp:=True,
                      messageBoxTitle:="",
                      IsConfirm:=True)

    End Sub

#Region "帳票処理"

    ''' <summary>
    ''' 帳票(明細情報)出力
    ''' </summary>
    Protected Sub WF_ButtonDetailDownload_Click()

        Dim SelectChk As Boolean = False
        Dim intTblCnt As Integer = 0

        '件数を取得
        intTblCnt = OIT0003tbl.Rows.Count

        '行が選択されているかチェック
        For Each OIT0003UPDrow In OIT0003tbl.Rows
            If OIT0003UPDrow("OPERATION") = "on" Then
                SelectChk = True
            End If
        Next

        '○メッセージ表示
        '一覧件数が０件の時の場合
        If intTblCnt = 0 Then
            Master.Output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub

            '一覧件数が１件以上で未選択の場合
        ElseIf SelectChk = False Then
            Master.Output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '******************************
        '帳票表示データ取得処理
        '******************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            ExcelOrderDetailDataGet(SQLcon)
        End Using

        '******************************
        '帳票作成処理の実行
        '******************************
        Dim url As String
        Try
            url = OIT0003CustomMultiReport.CreateOrderDetail(Master.MAPID, OIT0003Reporttbl)
        Catch ex As Exception
            Throw
        End Try

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = url
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    Protected Sub ExcelOrderDetailDataGet(SQLcon As SqlConnection)

        If IsNothing(OIT0003Reporttbl) Then
            OIT0003Reporttbl = New DataTable
        End If

        If OIT0003Reporttbl.Columns.Count <> 0 Then
            OIT0003Reporttbl.Columns.Clear()
        End If

        OIT0003Reporttbl.Clear()

        '画面上選択されたORDERNO一覧を生成
        Dim qcheckedRow = (From dr As DataRow In OIT0003tbl Where Convert.ToString(dr("OPERATION")) <> "" Select Convert.ToString(dr("ORDERNO")))
        '未選択ならば全て選択を想定
        If qcheckedRow.Any = False Then
            qcheckedRow = (From dr As DataRow In OIT0003tbl Select Convert.ToString(dr("ORDERNO")))
        End If
        Dim selectedOrderNo As List(Of String) = qcheckedRow.ToList
        Dim selectedOrderNoInStat As String = String.Join(",", (From odrNo In selectedOrderNo Select "'" & odrNo & "'"))

        '○ 取得SQL
        '　 説明　：　受注明細ダウンロード取得用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "     ISNULL(RTRIM(OIT0002.ORDERNO), '')                      AS ORDERNO " _
            & "   , ISNULL(RTRIM(OIT0003.DETAILNO), '')                     AS DETAILNO " _
            & "   , ISNULL(RTRIM(OIT0002.OFFICECODE), '')                   AS OFFICECODE " _
            & "   , ISNULL(RTRIM(OIT0002.TRAINNO), '')                      AS TRAINNO " _
            & "   , ISNULL(RTRIM(OIT0002.ARRSTATION), '')                   AS ARRSTATION " _
            & "   , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')               AS ARRSTATIONNAME " _
            & "   , ISNULL(FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd'), '')       AS LODDATE " _
            & "   , ISNULL(FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd'), '')       AS DEPDATE " _
            & "   , ISNULL(RTRIM(OIT0003.SHIPORDER), '')                    AS SHIPORDER " _
            & "   , CASE ISNULL(OIM0005.MODEL, '') " _
            & "     WHEN 'タキ1000' THEN '1000-' + " _
            & "   RIGHT ('0000' + ISNULL(RTRIM(OIT0003.TANKNO), ''), 4) " _
            & "   ELSE ISNULL(RTRIM(OIT0003.TANKNO), '') " _
            & "   END                                                       AS TANKNO " _
            & "   , ISNULL(RTRIM(OIT0003.OILCODE), '')                      AS OILCODE " _
            & "   , ISNULL(RTRIM(OIT0003.OILNAME), '')                      AS OILNAME " _
            & "   , ISNULL(RTRIM(OIT0003.CARSAMOUNT), '')                   AS CARSAMOUNT " _
            & "   , ISNULL(RTRIM(OIT0003.SHIPPERSCODE), '')                 AS SHIPPERSCODE " _
            & "   , ISNULL(RTRIM(OIT0003.SHIPPERSNAME), '')                 AS SHIPPERSNAME " _
            & "   , ISNULL(RTRIM(OIT0003.JOINTCODE), '')                    AS JOINTCODE " _
            & "   , ISNULL(RTRIM(OIT0003.JOINT), '')                        AS JOINT " _
            & "   , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEECODE), '')          AS SECONDCONSIGNEECODE " _
            & "   , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEENAME), '')          AS SECONDCONSIGNEENAME " _
            & "   , CASE ISNULL(RTRIM(OIT0003.STACKINGFLG), '') " _
            & "   WHEN '1' THEN 'レ' " _
            & "   WHEN '2' THEN '' " _
            & "   ELSE '' " _
            & "   END                                                       AS STACKINGFLG " _
            & "   , CASE ISNULL(RTRIM(OIT0003.FIRSTRETURNFLG), '') " _
            & "   WHEN '1' THEN 'レ' " _
            & "   WHEN '2' THEN '' " _
            & "   ELSE '' " _
            & "   END                                                       AS FIRSTRETURNFLG " _
            & "   , CASE ISNULL(RTRIM(OIT0003.AFTERRETURNFLG), '') " _
            & "   WHEN '1' THEN 'レ' " _
            & "   WHEN '2' THEN '' " _
            & "   ELSE '' " _
            & "   END                                                       AS AFTERRETURNFLG " _
            & "   , CASE ISNULL(RTRIM(OIT0003.OTTRANSPORTFLG), '') " _
            & "   WHEN '1' THEN 'レ' " _
            & "   WHEN '2' THEN '' " _
            & "   ELSE '' " _
            & "   END                                                       AS OTTRANSPORTFLG " _
            & "   , CASE ISNULL(RTRIM(OIT0003.UPGRADEFLG), '') " _
            & "   WHEN '0' THEN '' " _
            & "   WHEN '1' THEN 'レ' " _
            & "   WHEN '2' THEN '' " _
            & "   ELSE '' " _
            & "   END                                                       AS UPGRADEFLG " _
            & "   , CASE ISNULL(RTRIM(OIT0003.UPGRADEFLG), '') " _
            & "   WHEN '0' THEN 'レ' " _
            & "   WHEN '1' THEN '' " _
            & "   WHEN '2' THEN '' " _
            & "   ELSE '' " _
            & "   END                                                       AS DOWNGRADEFLG " _
            & "   , ISNULL(FORMAT(OIT0003.ACTUALLODDATE, 'yyyy/MM/dd'), '') AS ACTUALLODDATE " _
            & "   , ISNULL(FORMAT(OIT0003.ACTUALDEPDATE, 'yyyy/MM/dd'), '') AS ACTUALDEPDATE " _
            & "   , ISNULL(FORMAT(OIT0003.ACTUALARRDATE, 'yyyy/MM/dd'), '') AS ACTUALARRDATE " _
            & "   , ISNULL(FORMAT(OIT0003.ACTUALACCDATE, 'yyyy/MM/dd'), '') AS ACTUALACCDATE " _
            & "   , ISNULL( " _
            & "   FORMAT(OIT0003.ACTUALEMPARRDATE, 'yyyy/MM/dd') " _
            & "   , '' " _
            & " )                                                           AS ACTUALEMPARRDATE " _
            & "   , ISNULL(RTRIM(OIT0003.LOADINGIRILINETRAINNO), '')        AS LOADINGIRILINETRAINNO " _
            & "   , ISNULL(RTRIM(OIT0003.LOADINGIRILINEORDER), '')          AS LOADINGIRILINEORDER " _
            & "   , ISNULL(RTRIM(OIT0003.LINE), '')                         AS LINE " _
            & "   , ISNULL(RTRIM(OIT0003.FILLINGPOINT), '')                 AS FILLINGPOINT " _
            & "   , ISNULL(RTRIM(OIM0024.PRIORITYNO), '')                   AS PRIORITYNO " _
            & " FROM " _
            & "   [oil].OIT0002_ORDER OIT0002 " _
            & "   INNER JOIN [oil].OIT0003_DETAIL OIT0003 " _
            & "     ON OIT0002.ORDERNO = OIT0003.ORDERNO " _
            & "     AND OIT0003.DELFLG = @DELFLG " _
            & "   LEFT JOIN [oil].OIM0024_PRIORITY OIM0024 " _
            & "     ON OIM0024.OFFICECODE = OIT0002.OFFICECODE " _
            & "     AND OIM0024.OILCODE = OIT0003.OILCODE " _
            & "     AND OIM0024.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & "     AND OIM0024.DELFLG = @DELFLG " _
            & "   LEFT JOIN [oil].OIM0005_TANK OIM0005 " _
            & "     ON OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & "     AND OIM0005.DELFLG = @DELFLG " _
            & " WHERE " _
            & "   OIT0002.DELFLG = @DELFLG "

        If Not String.IsNullOrEmpty(selectedOrderNoInStat) Then
            SQLStr &= String.Format("   AND OIT0002.ORDERNO IN ({0}) ", selectedOrderNoInStat)
        End If

        SQLStr &=
              " ORDER BY " _
            & "   OIT0002.ORDERNO " _
            & "   , OIT0002.TRAINNO " _
            & "   , OIT0002.LODDATE " _
            & "   , OIT0002.DEPDATE "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1) '受注営業所コード
                PARA01.Value = BaseDllConst.C_DELETE_FLG.ALIVE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003Reporttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003Reporttbl.Load(SQLdr)
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LNEGISHI PLANFRAME_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LNEGISHI PLANFRAME_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportPlanFrame)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        Select Case WF_ButtonClick.Value
            Case "WF_ButtonCSV"             'ダウンロードボタン押下

                ''ダウンロードボタン(積込予定)押下
                'Case "WF_ButtonSendaiLOADCSV"

                '    '******************************
                '    '帳票表示データ取得処理
                '    '******************************
                '    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                '        SQLcon.Open()       'DataBase接続

                '        ExcelLoadCommonDataGet(SQLcon, BaseDllConst.CONST_OFFICECODE_010402)
                '    End Using

                '    '******************************
                '    '帳票作成処理の実行
                '    '******************************
                '    Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_LOADPLAN.xlsx", OIT0003Reporttbl)
                '        Dim url As String
                '        Try
                '            url = repCbj.CreateExcelPrintData(BaseDllConst.CONST_OFFICECODE_010402)
                '        Catch ex As Exception
                '            Return
                '        End Try
                '        '○ 別画面でExcelを表示
                '        WF_PrintURL.Value = url
                '        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                '    End Using

                ''ダウンロードボタン(根岸(出荷予定))押下
                ''ダウンロードボタン(根岸(積込予定))押下
                'Case "WF_ButtonNegishiSHIPCSV",
                '     "WF_ButtonNegishiLOADCSV"

                '    '******************************
                '    '帳票表示データ取得処理
                '    '******************************
                '    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                '        SQLcon.Open()       'DataBase接続

                '        ExcelNegishiDataGet(SQLcon, WF_ButtonClick.Value)
                '    End Using

                '    '******************************
                '    '帳票作成処理の実行
                '    '******************************
                '    Select Case WF_ButtonClick.Value
                '        'ダウンロードボタン(根岸(出荷予定))押下
                '        Case "WF_ButtonNegishiSHIPCSV"
                '            Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_NEGISHI_SHIPPLAN.xlsx", OIT0003ReportNegishitbl)
                '                Dim url As String
                '                Try
                '                    url = repCbj.CreateExcelPrintNegishiData("SHIPPLAN", Now.AddDays(1).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP")))
                '                Catch ex As Exception
                '                    Return
                '                End Try
                '                '○ 別画面でExcelを表示
                '                WF_PrintURL.Value = url
                '                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                '            End Using

                '        'ダウンロードボタン(根岸(積込予定))押下
                '        Case "WF_ButtonNegishiLOADCSV"
                '            Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_NEGISHI_LOADPLAN.xlsx", OIT0003ReportNegishitbl)
                '                Dim url As String
                '                Try
                '                    url = repCbj.CreateExcelPrintNegishiData("LOADPLAN", Now.AddDays(1).ToString("yyyy/MM/dd", New Globalization.CultureInfo("ja-JP")))
                '                Catch ex As Exception
                '                    Return
                '                End Try
                '                '○ 別画面でExcelを表示
                '                WF_PrintURL.Value = url
                '                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                '            End Using

                '    End Select
        End Select
    End Sub
#End Region

    ''' <summary>
    ''' 受注新規作成ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        work.WF_SEL_LINECNT.Text = ""
        '登録日
        work.WF_SEL_REGISTRATIONDATE.Text = ""
        '受注営業所(名)
        work.WF_SEL_ORDERSALESOFFICE.Text = ""
        '受注営業所(コード)
        work.WF_SEL_ORDERSALESOFFICECODE.Text = ""
        '受注進行ステータス(名)
        'work.WF_SEL_ORDERSTATUSNM.Text = "受注受付"
        CODENAME_get("ORDERSTATUS", BaseDllConst.CONST_ORDERSTATUS_100, work.WF_SEL_ORDERSTATUSNM.Text, WW_DUMMY)
        '受注進行ステータス(コード)
        work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100
        '受注情報(名)
        work.WF_SEL_INFORMATIONNM.Text = ""
        '受注情報(コード)
        work.WF_SEL_INFORMATION.Text = ""
        '発送日報送信状況フラグ
        work.WF_SEL_OTSENDSTATUS_FLG.Text = ""
        '発送日報送信状況(名)
        work.WF_SEL_OTSENDSTATUSNM.Text = ""
        '発送日報送信状況(コード)
        work.WF_SEL_OTSENDSTATUS.Text = ""
        '空回日報可否フラグ(0：未作成, 1:作成)
        work.WF_SEL_EMPTYTURNFLG.Text = "0"
        '積置可否フラグ(１：積置あり, ２：積置なし)
        work.WF_SEL_STACKINGFLG.Text = "2"
        '利用可否フラグ(１：利用可, ２：利用不可)
        work.WF_SEL_USEPROPRIETYFLG.Text = "1"
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = ""
        '本線列車
        work.WF_SEL_TRAIN.Text = ""
        '本線列車名
        work.WF_SEL_TRAINNAME.Text = ""
        'OT本線列車
        work.WF_SEL_OTTRAIN.Text = ""
        '荷主(名)
        work.WF_SEL_SHIPPERSNAME.Text = ""
        '荷主(コード)
        work.WF_SEL_SHIPPERSCODE.Text = ""
        '荷受人(名)
        work.WF_SEL_CONSIGNEENAME.Text = ""
        '荷受人(コード)
        work.WF_SEL_CONSIGNEECODE.Text = ""
        'パターンコード(名)
        work.WF_SEL_PATTERNNAME.Text = ""
        'パターンコード
        work.WF_SEL_PATTERNCODE.Text = ""
        '発駅(名)
        work.WF_SEL_DEPARTURESTATIONNM.Text = ""
        '発駅(コード)
        work.WF_SEL_DEPARTURESTATION.Text = ""
        '着駅(名)
        work.WF_SEL_ARRIVALSTATIONNM.Text = ""
        '着駅(コード)
        work.WF_SEL_ARRIVALSTATION.Text = ""
        '戻着駅(名)
        work.WF_SEL_CHANGERETSTATIONNM.Text = ""
        '戻着駅(コード)
        work.WF_SEL_CHANGERETSTATION.Text = ""

        '車数（レギュラー）
        work.WF_SEL_REGULAR_TANKCAR.Text = "0"
        '車数（ハイオク）
        work.WF_SEL_HIGHOCTANE_TANKCAR.Text = "0"
        '車数（灯油）
        work.WF_SEL_KEROSENE_TANKCAR.Text = "0"
        '車数（未添加灯油）
        work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text = "0"
        '車数（軽油）
        work.WF_SEL_DIESEL_TANKCAR.Text = "0"
        '車数（３号軽油）
        work.WF_SEL_NUM3DIESEL_TANKCAR.Text = "0"
        '車数（５号軽油）
        work.WF_SEL_NUM5DIESEL_TANKCAR.Text = "0"
        '車数（１０号軽油）
        work.WF_SEL_NUM10DIESEL_TANKCAR.Text = "0"
        '車数（LSA）
        work.WF_SEL_LSA_TANKCAR.Text = "0"
        '車数（A重油）
        work.WF_SEL_AHEAVY_TANKCAR.Text = "0"
        '合計車数
        work.WF_SEL_TANKCARTOTAL.Text = "0"

        '積込日(予定)
        work.WF_SEL_LODDATE.Text = ""
        '発日(予定)
        work.WF_SEL_DEPDATE.Text = ""
        '着日(予定)
        work.WF_SEL_ARRDATE.Text = ""
        '受入日(予定)
        work.WF_SEL_ACCDATE.Text = ""
        '空車着日(予定)
        work.WF_SEL_EMPARRDATE.Text = ""
        '積込日(実績)
        work.WF_SEL_ACTUALLODDATE.Text = ""
        '発日(実績)
        work.WF_SEL_ACTUALDEPDATE.Text = ""
        '着日(実績)
        work.WF_SEL_ACTUALARRDATE.Text = ""
        '受入日(実績)
        work.WF_SEL_ACTUALACCDATE.Text = ""
        '空車着日(実績)
        work.WF_SEL_ACTUALEMPARRDATE.Text = ""
        '貨車連結順序表№
        work.WF_SEL_LINKNO.Text = ""

        '支払請求№
        work.WF_SEL_BILLINGNO.Text = ""
        '計上年月日
        work.WF_SEL_KEIJYOYMD.Text = ""
        '売上金額
        work.WF_SEL_SALSE.Text = "0"
        '売上消費税額
        work.WF_SEL_SALSETAX.Text = "0"
        '売上合計金額
        work.WF_SEL_TOTALSALSE.Text = "0"
        '支払金額
        work.WF_SEL_PAYMENT.Text = "0"
        '支払消費税額
        work.WF_SEL_PAYMENTTAX.Text = "0"
        '支払合計金額
        work.WF_SEL_TOTALPAYMENT.Text = "0"

        '削除フラグ
        work.WF_SEL_DELFLG.Text = "0"
        '作成フラグ(1：新規登録, 2：更新)
        work.WF_SEL_CREATEFLG.Text = "1"
        '作成フラグ(1：貨車連結未使用, 2：貨車連結使用)
        work.WF_SEL_CREATELINKFLG.Text = "1"
        '手配連絡フラグ(0：未連絡, 1：連絡)
        work.WF_SEL_CONTACTFLG.Text = "0"
        '結果受理フラグ(0：未受理, 1：受理)
        work.WF_SEL_RESULTFLG.Text = "0"
        '託送指示フラグ(0：未手配, 1:手配)
        work.WF_SEL_DELIVERYFLG.Text = "0"
        '発送順区分
        work.WF_SEL_SHIPORDERCLASS.Text = ""
        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)

    End Sub

    ''' <summary>
    ''' 貨車連結選択ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLinkINSERT_Click()

        '選択行
        work.WF_SEL_LINECNT.Text = ""
        '登録日
        'work.WF_SEL_REGISTRATIONDATE.Text = DateTime.Now.ToString("d")
        work.WF_SEL_REGISTRATIONDATE.Text = work.WF_SEL_DATE.Text
        '受注営業所(名)
        work.WF_SEL_ORDERSALESOFFICE.Text = ""
        '受注営業所(コード)
        work.WF_SEL_ORDERSALESOFFICECODE.Text = ""
        '受注進行ステータス(名)
        'work.WF_SEL_ORDERSTATUSNM.Text = "受注受付"
        CODENAME_get("ORDERSTATUS", BaseDllConst.CONST_ORDERSTATUS_100, work.WF_SEL_ORDERSTATUSNM.Text, WW_DUMMY)
        '受注進行ステータス(コード)
        work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100
        '受注情報(名)
        work.WF_SEL_INFORMATIONNM.Text = ""
        '受注情報(コード)
        work.WF_SEL_INFORMATION.Text = ""
        '空回日報可否フラグ(0：未作成, 1:作成)
        work.WF_SEL_EMPTYTURNFLG.Text = "0"
        '積置可否フラグ(１：積置あり, ２：積置なし)
        work.WF_SEL_STACKINGFLG.Text = "2"
        '利用可否フラグ(１：利用可, ２：利用不可)
        work.WF_SEL_USEPROPRIETYFLG.Text = "1"
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = ""
        '本線列車
        work.WF_SEL_TRAIN.Text = ""
        '本線列車名
        work.WF_SEL_TRAINNAME.Text = ""
        'OT本線列車
        work.WF_SEL_OTTRAIN.Text = ""
        '荷主(名)
        work.WF_SEL_SHIPPERSNAME.Text = ""
        '荷主(コード)
        work.WF_SEL_SHIPPERSCODE.Text = ""
        '荷受人(名)
        work.WF_SEL_CONSIGNEENAME.Text = ""
        '荷受人(コード)
        work.WF_SEL_CONSIGNEECODE.Text = ""
        '発駅(名)
        work.WF_SEL_DEPARTURESTATIONNM.Text = ""
        '発駅(コード)
        work.WF_SEL_DEPARTURESTATION.Text = ""
        '着駅(名)
        work.WF_SEL_ARRIVALSTATIONNM.Text = ""
        '着駅(コード)
        work.WF_SEL_ARRIVALSTATION.Text = ""
        '戻着駅(名)
        work.WF_SEL_CHANGERETSTATIONNM.Text = ""
        '戻着駅(コード)
        work.WF_SEL_CHANGERETSTATION.Text = ""

        '車数（レギュラー）
        work.WF_SEL_REGULAR_TANKCAR.Text = "0"
        '車数（ハイオク）
        work.WF_SEL_HIGHOCTANE_TANKCAR.Text = "0"
        '車数（灯油）
        work.WF_SEL_KEROSENE_TANKCAR.Text = "0"
        '車数（未添加灯油）
        work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text = "0"
        '車数（軽油）
        work.WF_SEL_DIESEL_TANKCAR.Text = "0"
        '車数（３号軽油）
        work.WF_SEL_NUM3DIESEL_TANKCAR.Text = "0"
        '車数（５号軽油）
        work.WF_SEL_NUM5DIESEL_TANKCAR.Text = "0"
        '車数（１０号軽油）
        work.WF_SEL_NUM10DIESEL_TANKCAR.Text = "0"
        '車数（LSA）
        work.WF_SEL_LSA_TANKCAR.Text = "0"
        '車数（A重油）
        work.WF_SEL_AHEAVY_TANKCAR.Text = "0"
        '合計車数
        work.WF_SEL_TANKCARTOTAL.Text = "0"

        '積込日(予定)
        work.WF_SEL_LODDATE.Text = ""
        '発日(予定)
        work.WF_SEL_DEPDATE.Text = ""
        '着日(予定)
        work.WF_SEL_ARRDATE.Text = ""
        '受入日(予定)
        work.WF_SEL_ACCDATE.Text = ""
        '空車着日(予定)
        work.WF_SEL_EMPARRDATE.Text = ""
        '積込日(実績)
        work.WF_SEL_ACTUALLODDATE.Text = ""
        '発日(実績)
        work.WF_SEL_ACTUALDEPDATE.Text = ""
        '着日(実績)
        work.WF_SEL_ACTUALARRDATE.Text = ""
        '受入日(実績)
        work.WF_SEL_ACTUALACCDATE.Text = ""
        '空車着日(実績)
        work.WF_SEL_ACTUALEMPARRDATE.Text = ""
        '貨車連結順序表№
        work.WF_SEL_LINKNO.Text = ""

        '支払請求№
        work.WF_SEL_BILLINGNO.Text = ""
        '計上年月日
        work.WF_SEL_KEIJYOYMD.Text = ""
        '売上金額
        work.WF_SEL_SALSE.Text = "0"
        '売上消費税額
        work.WF_SEL_SALSETAX.Text = "0"
        '売上合計金額
        work.WF_SEL_TOTALSALSE.Text = "0"
        '支払金額
        work.WF_SEL_PAYMENT.Text = "0"
        '支払消費税額
        work.WF_SEL_PAYMENTTAX.Text = "0"
        '支払合計金額
        work.WF_SEL_TOTALPAYMENT.Text = "0"

        '削除フラグ
        work.WF_SEL_DELFLG.Text = "0"
        '作成フラグ(1：新規登録, 2：更新)
        work.WF_SEL_CREATEFLG.Text = "1"
        '作成フラグ(1：貨車連結未使用, 2：貨車連結使用)
        work.WF_SEL_CREATELINKFLG.Text = "2"
        '手配連絡フラグ(0：未連絡, 1：連絡)
        work.WF_SEL_CONTACTFLG.Text = "0"
        '結果受理フラグ(0：未受理, 1：受理)
        work.WF_SEL_RESULTFLG.Text = "0"
        '託送指示フラグ(0：未手配, 1:手配)
        work.WF_SEL_DELIVERYFLG.Text = "0"
        '発送順区分
        work.WF_SEL_SHIPORDERCLASS.Text = ""

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text + "1")

    End Sub

    ''' <summary>
    ''' OT連携選択ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOTLinkageINSERT_Click()

        '○ 次ページ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text + "2")

    End Sub


    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

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

                    '○ 選択内容を画面項目へセット
                    Select Case WF_FIELD.Value
                        '列車番号(臨海)
                        Case "txtReportRTrainNo"
                            prmData = work.CreateFIXParam(work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)
                        '列車番号
                        Case "txtReportTrainNo"

                            ' 営業所指定
                            prmData = work.CreateFIXParam(work.WF_SEL_TH_ORDERSALESOFFICECODE.Text, "TRAINNUMBER_REPCNV")

                            Select Case work.WF_SEL_TH_ORDERSALESOFFICECODE.Text
                                Case CONST_OFFICECODE_010402
                                    '■仙台新港
                                    If Me.rbTankDispatchBtn.Checked = True Then
                                        '○タンク車発送実績の場合
                                        '発駅指定：仙台
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = " AND VALUE2 = '243202' "
                                        '着駅指定：[OT盛岡51] 2018(盛岡タ)
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND VALUE3 = '2018' "
                                    End If

                                Case CONST_OFFICECODE_011203
                                    '■袖ヶ浦
                                    If Me.rbActualShipBtn.Checked = True Then
                                        '○出荷実績の場合
                                        '発駅：袖ヶ浦
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = " AND VALUE2 = '434108' "
                                        '着駅：倉賀野（高崎）南松本の列車を表示
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND VALUE3 IN ('5141', '4113') "
                                        '対象の列車
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND KEYCODE IN ('5461', '9672', '5972') "
                                    ElseIf Me.rbTankDispatch30Btn.Checked = True Or Me.rbTankDispatch54Btn.Checked = True Then
                                        '○タンク車発送実績の場合
                                        '発駅：袖ヶ浦
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = " AND VALUE2 = '434108' "
                                        '着駅：倉賀野（高崎）南松本の列車を表示
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND VALUE3 = '4113' "
                                        '対象の列車
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND KEYCODE IN ('8877', '8883') "
                                    ElseIf Me.rbTankDispatch40Btn.Checked = True Then
                                        '○タンク車発送実績の場合
                                        '発駅：袖ヶ浦
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = " AND VALUE2 = '434108' "
                                        '着駅：倉賀野（高崎）南松本の列車を表示
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND VALUE3 = '5141' "
                                        '対象の列車
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND KEYCODE IN ('5461', '9672', '5972') "
                                    ElseIf Me.rbConcatOederBtn.Checked = True Then
                                        '○連結順序表の場合
                                        '発駅：袖ヶ浦
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = " AND VALUE2 = '434108' "
                                        '着駅：倉賀野（高崎）南松本の列車を表示
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND VALUE3 IN ('5141', '4113') "
                                        '対象の列車
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND KEYCODE IN ('5461', '9672', '5972')  "
                                    End If

                                Case CONST_OFFICECODE_012402
                                    '■三重塩浜
                                    If Me.rbTankDispatchBtn.Checked = True Then
                                        '○タンク車発送実績の場合
                                        '発駅：三重塩浜
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = " AND VALUE2 = '5512' "
                                        '着駅：南松本の列車を表示
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND VALUE3 = '5141' "
                                    ElseIf Me.rbActualShipBtn.Checked = True Then
                                        '○出荷実績の場合
                                        '発駅：三重塩浜
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = " AND VALUE2 = '5512' "
                                        '着駅：南松本の列車を表示
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND VALUE3 = '5141' "
                                    ElseIf Me.rbConcatOederBtn.Checked = True Then
                                        '○連結順序表の場合
                                        '発駅：三重塩浜
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = " AND VALUE2 = '5512' "
                                        '着駅：南松本の列車を表示
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND VALUE3 = '5141' "
                                    ElseIf Me.rbShipContactBtn.Checked = True Then
                                        '○連結順序表の場合
                                        '発駅：三重塩浜
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = " AND VALUE2 = '5512' "
                                        '着駅：南松本の列車を表示
                                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) += " AND VALUE3 = '5141' "
                                    End If

                                Case Else
                                    '表示制限なし
                            End Select
                    End Select
                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        '(帳票ポップアップ)積込日
                        Case "txtReportLodDate"
                            .WF_Calendar.Text = Me.txtReportLodDate.Text
                    End Select
                    .ActiveCalendar()
                End If
            End With
        End If
    End Sub

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_LINECNT As Integer = 0
        Dim WW_FIELD_OBJ As Object = Nothing
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '〇 受注進行ステータスが"900(受注キャンセル)"の場合は何もしない
        WW_ORDERSTATUS = OIT0003tbl.Rows(WW_LINECNT)("ORDERSTATUS")
        If WW_ORDERSTATUS = BaseDllConst.CONST_ORDERSTATUS_900 Then
            Master.Output(C_MESSAGE_NO.OIL_CANCEL_ENTRY_ORDER, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '選択行
        work.WF_SEL_LINECNT.Text = OIT0003tbl.Rows(WW_LINECNT)("LINECNT")
        '登録日
        work.WF_SEL_REGISTRATIONDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERYMD")
        '受注営業所(名)
        work.WF_SEL_ORDERSALESOFFICE.Text = OIT0003tbl.Rows(WW_LINECNT)("OFFICENAME")
        '受注営業所(コード)
        work.WF_SEL_ORDERSALESOFFICECODE.Text = OIT0003tbl.Rows(WW_LINECNT)("OFFICECODE")
        '受注進行ステータス(名)
        work.WF_SEL_ORDERSTATUSNM.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERSTATUSNAME")
        '受注進行ステータス(コード)
        work.WF_SEL_ORDERSTATUS.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERSTATUS")
        '受注情報(名)
        'work.WF_SEL_INFORMATIONNM.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERINFONAME")
        work.WF_SEL_INFORMATIONNM.Text = Regex.Replace(OIT0003tbl.Rows(WW_LINECNT)("ORDERINFONAME"), "<[^>]*?>", "")
        '受注情報(コード)
        work.WF_SEL_INFORMATION.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERINFO")
        '発送日報送信状況フラグ
        work.WF_SEL_OTSENDSTATUS_FLG.Text = OIT0003tbl.Rows(WW_LINECNT)("OTFLG")
        '発送日報送信状況(名)
        work.WF_SEL_OTSENDSTATUSNM.Text = OIT0003tbl.Rows(WW_LINECNT)("OTSENDSTATUSNAME")
        '発送日報送信状況(コード)
        work.WF_SEL_OTSENDSTATUS.Text = OIT0003tbl.Rows(WW_LINECNT)("OTSENDSTATUS")
        '空回日報可否フラグ
        work.WF_SEL_EMPTYTURNFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("EMPTYTURNFLG")
        '積置可否フラグ
        work.WF_SEL_STACKINGFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("STACKINGFLG")
        '利用可否フラグ
        work.WF_SEL_USEPROPRIETYFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("USEPROPRIETYFLG")
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERNO")
        '本線列車
        'work.WF_SEL_TRAIN.Text = OIT0003tbl.Rows(WW_LINECNT)("TRAINNO")
        work.WF_SEL_TRAIN.Text = Regex.Replace(OIT0003tbl.Rows(WW_LINECNT)("TRAINNO"), "<[^>]*?>", "")
        '本線列車名
        work.WF_SEL_TRAINNAME.Text = OIT0003tbl.Rows(WW_LINECNT)("TRAINNAME")
        '受注パターン
        work.WF_SEL_PATTERNCODE.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERTYPE")
        '受注パターン(名)
        work.WF_SEL_PATTERNNAME.Text = ""
        '荷主(名)
        work.WF_SEL_SHIPPERSNAME.Text = OIT0003tbl.Rows(WW_LINECNT)("SHIPPERSNAME")
        '荷主(コード)
        work.WF_SEL_SHIPPERSCODE.Text = OIT0003tbl.Rows(WW_LINECNT)("SHIPPERSCODE")
        '基地(名)
        work.WF_SEL_BASENAME.Text = OIT0003tbl.Rows(WW_LINECNT)("BASENAME")
        '基地(コード)
        work.WF_SEL_BASECODE.Text = OIT0003tbl.Rows(WW_LINECNT)("BASECODE")
        '荷受人(名)
        work.WF_SEL_CONSIGNEENAME.Text = OIT0003tbl.Rows(WW_LINECNT)("CONSIGNEENAME")
        '荷受人(コード)
        work.WF_SEL_CONSIGNEECODE.Text = OIT0003tbl.Rows(WW_LINECNT)("CONSIGNEECODE")
        '発駅(名)
        work.WF_SEL_DEPARTURESTATIONNM.Text = OIT0003tbl.Rows(WW_LINECNT)("DEPSTATIONNAME")
        '発駅(コード)
        work.WF_SEL_DEPARTURESTATION.Text = OIT0003tbl.Rows(WW_LINECNT)("DEPSTATION")
        '着駅(名)
        work.WF_SEL_ARRIVALSTATIONNM.Text = OIT0003tbl.Rows(WW_LINECNT)("ARRSTATIONNAME")
        '着駅(コード)
        work.WF_SEL_ARRIVALSTATION.Text = OIT0003tbl.Rows(WW_LINECNT)("ARRSTATION")
        '戻着駅(名)
        work.WF_SEL_CHANGERETSTATIONNM.Text = OIT0003tbl.Rows(WW_LINECNT)("CHANGERETSTATIONNAME")
        '戻着駅(コード)
        work.WF_SEL_CHANGERETSTATION.Text = OIT0003tbl.Rows(WW_LINECNT)("CHANGERETSTATION")

        '車数（レギュラー）
        work.WF_SEL_REGULAR_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("RTANK")
        '車数（ハイオク）
        work.WF_SEL_HIGHOCTANE_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("HTANK")
        '車数（灯油）
        work.WF_SEL_KEROSENE_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("TTANK")
        '車数（未添加灯油）
        work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("MTTANK")
        '車数（軽油）
        work.WF_SEL_DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("KTANK")
        '車数（３号軽油）
        work.WF_SEL_NUM3DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K3TANK")
        '車数（５号軽油）
        work.WF_SEL_NUM5DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K5TANK")
        '車数（１０号軽油）
        work.WF_SEL_NUM10DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K10TANK")
        '車数（LSA）
        work.WF_SEL_LSA_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("LTANK")
        '車数（A重油）
        work.WF_SEL_AHEAVY_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("ATANK")
        '合計車数
        'work.WF_SEL_TANKCARTOTAL.Text = OIT0003tbl.Rows(WW_LINECNT)("TOTALTANK")
        work.WF_SEL_TANKCARTOTAL.Text = Regex.Replace(OIT0003tbl.Rows(WW_LINECNT)("TOTALTANK"), "<[^>]*?>", "")

        '車数（レギュラー）割当
        work.WF_SEL_REGULARCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("RTANKCH")
        '車数（ハイオク）割当
        work.WF_SEL_HIGHOCTANECH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("HTANKCH")
        '車数（灯油）割当
        work.WF_SEL_KEROSENECH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("TTANKCH")
        '車数（未添加灯油）割当
        work.WF_SEL_NOTADDED_KEROSENECH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("MTTANKCH")
        '車数（軽油）割当
        work.WF_SEL_DIESELCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("KTANKCH")
        '車数（３号軽油）割当
        work.WF_SEL_NUM3DIESELCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K3TANKCH")
        '車数（５号軽油）割当
        work.WF_SEL_NUM5DIESELCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K5TANKCH")
        '車数（１０号軽油）割当
        work.WF_SEL_NUM10DIESELCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K10TANKCH")
        '車数（LSA）割当
        work.WF_SEL_LSACH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("LTANKCH")
        '車数（A重油）割当
        work.WF_SEL_AHEAVYCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("ATANKCH")
        '合計車数（割当）
        work.WF_SEL_TANKCARTOTALCH.Text = OIT0003tbl.Rows(WW_LINECNT)("TOTALTANKCH")

        '積込日(予定)
        work.WF_SEL_LODDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("LODDATE")
        '発日(予定)
        work.WF_SEL_DEPDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("DEPDATE")
        '着日(予定)
        work.WF_SEL_ARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ARRDATE")
        '受入日(予定)
        work.WF_SEL_ACCDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACCDATE")
        '空車着日(予定)
        work.WF_SEL_EMPARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("EMPARRDATE")
        '積込日(実績)
        work.WF_SEL_ACTUALLODDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALLODDATE")
        '発日(実績)
        work.WF_SEL_ACTUALDEPDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALDEPDATE")
        '着日(実績)
        work.WF_SEL_ACTUALARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALARRDATE")
        '受入日(実績)
        work.WF_SEL_ACTUALACCDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALACCDATE")
        '空車着日(実績)
        work.WF_SEL_ACTUALEMPARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALEMPARRDATE")
        '貨車連結順序表№
        work.WF_SEL_LINKNO.Text = OIT0003tbl.Rows(WW_LINECNT)("TANKLINKNO")
        '作成用_貨車連結順序表№
        work.WF_SEL_LINKNO_ORDER.Text = OIT0003tbl.Rows(WW_LINECNT)("TANKLINKNOMADE")

        '支払請求№
        work.WF_SEL_BILLINGNO.Text = OIT0003tbl.Rows(WW_LINECNT)("BILLINGNO")
        '計上年月日
        work.WF_SEL_KEIJYOYMD.Text = OIT0003tbl.Rows(WW_LINECNT)("KEIJYOYMD")
        '売上金額
        work.WF_SEL_SALSE.Text = OIT0003tbl.Rows(WW_LINECNT)("SALSE")
        '売上消費税額
        work.WF_SEL_SALSETAX.Text = OIT0003tbl.Rows(WW_LINECNT)("SALSETAX")
        '売上合計金額
        work.WF_SEL_TOTALSALSE.Text = OIT0003tbl.Rows(WW_LINECNT)("TOTALSALSE")
        '支払金額
        work.WF_SEL_PAYMENT.Text = OIT0003tbl.Rows(WW_LINECNT)("PAYMENT")
        '支払消費税額
        work.WF_SEL_PAYMENTTAX.Text = OIT0003tbl.Rows(WW_LINECNT)("PAYMENTTAX")
        '支払合計金額
        work.WF_SEL_TOTALPAYMENT.Text = OIT0003tbl.Rows(WW_LINECNT)("TOTALPAYMENT")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("DELFLG")
        '作成フラグ(1：新規登録, 2：更新)
        work.WF_SEL_CREATEFLG.Text = "2"
        '作成フラグ(1：貨車連結未使用, 2：貨車連結使用)
        work.WF_SEL_CREATELINKFLG.Text = "1"
        '手配連絡フラグ(0：未連絡, 1：連絡)
        work.WF_SEL_CONTACTFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("CONTACTFLG")
        '結果受理フラグ(0：未受理, 1：受理)
        work.WF_SEL_RESULTFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("RESULTFLG")
        '託送指示フラグ(0：未手配, 1:手配)
        work.WF_SEL_DELIVERYFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("DELIVERYFLG")

        '★列車マスタから情報を取得
        WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        'WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TRAINNUMBER_FIND", work.WF_SEL_TRAINNAME.Text, WW_GetValue)
        WW_FixvalueMasterSearch(work.WF_SEL_ORDERSALESOFFICECODE.Text, "TRAINNUMBER_FIND", work.WF_SEL_TRAIN.Text + work.WF_SEL_ARRIVALSTATION.Text, WW_GetValue)
        '発送順区分
        work.WF_SEL_SHIPORDERCLASS.Text = WW_GetValue(13)
        'OT本線列車
        work.WF_SEL_OTTRAIN.Text = WW_GetValue(14)

        '○ 状態をクリア
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            Select Case OIT0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select

        Next

        '○ 選択明細の状態を設定
        Select Case OIT0003tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPTBL.Text)

        '### 20200806 START 受注一覧画面からの受注貨車連結割当画面への遷移を廃止 ###########################
        '### ★貨車連結順序表画面にて受注明細のデータを作成できるように変更したため(指摘票(全体)No115) #####
        ''受注進行ステータス(コード)
        ''〇受注進行ステータスが"100:受注受付"の場合
        'If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then
        '    If work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011201 _
        '        OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011202 _
        '        OrElse work.WF_SEL_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then
        '        '受注貨車連結割当画面ページへ遷移
        '        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text + "1")
        '    Else
        '        '受注明細画面ページへ遷移
        '        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)
        '    End If
        'Else
        '    '受注明細画面ページへ遷移
        '    Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)
        'End If

        '受注明細画面ページへ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)

        '### 20200806 END   受注一覧画面からの受注貨車連結割当画面への遷移を廃止 ###########################

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub

#Region "Excelアップロード"
    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()
        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '★ファイル判別フラグ
        Dim useFlg As String = ""

        Try
            '○ UPLOAD XLSデータ取得
            CS0023XLSUPLOAD.CS0023XLSUPLOAD_NEGISHI_LOADPLAN(OIT0003EXLUPtbl)
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End Try

        ''○発日を取得する(回線別予定表に設定されている場合はそのままの日付を採用)
        'Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        'For Each OIT0003EXLUProw In OIT0003EXLUPtbl.Select("TRAINNO<>'' AND DEPDATE=''")
        '    WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        '    WW_FixvalueMasterSearch(I_CODE:=BaseDllConst.CONST_OFFICECODE_011402,
        '                            I_CLASS:="TRAINNUMBER",
        '                            I_KEYCODE:=OIT0003EXLUProw("TRAINNO"),
        '                            O_VALUE:=WW_GetValue)
        '    OIT0003EXLUProw("DEPDATE") = Date.Parse(OIT0003EXLUProw("LODDATE")).AddDays(WW_GetValue(6)).ToString("yyyy/MM/dd")
        'Next

        '◯回線別積込取込(日新)TBL削除処理(再アップロード対応)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_DELETE_NLINELOAD(SQLcon)
        End Using

        '◯回線別積込取込(日新)TBL追加処理
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_INSERT_NLINELOAD(SQLcon)
        End Using

        '◯回線別積込取込(日新)TBL更新処理(JOT油種反映)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UPDATE_NLINELOAD(SQLcon)
        End Using

        '○受注データと回線別積込データとの紐づけ処理
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_LINKING_ORDER(SQLcon, WW_ERRCODE)
            If WW_ERRCODE = "ERR" Then
                Exit Sub
            End If
        End Using

        Dim Msg As String = ""
        Dim lodDate As Date = OIT0003EXLODRALLtbl.Rows(0)("LODDATE")
        Msg = lodDate.ToString("MM月dd日")
        Msg &= String.Format("車号データ {0}件取込完了", OIT0003EXLODRALLtbl.Rows.Count)
        Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.INF, I_PARA01:=Msg, needsPopUp:=True)

    End Sub

    ''' <summary>
    ''' 回線別積込取込(日新)TBL削除処理(再アップロード対応)
    ''' </summary>
    ''' <param name="SQLcon">接続オブジェクト</param>
    Protected Sub WW_DELETE_NLINELOAD(ByVal SQLcon As SqlConnection)
        '再アップロード時の削除データ取得用
        If IsNothing(OIT0003EXLDELtbl) Then
            OIT0003EXLDELtbl = New DataTable
        End If

        If OIT0003EXLDELtbl.Columns.Count <> 0 Then
            OIT0003EXLDELtbl.Columns.Clear()
        End If

        OIT0003EXLDELtbl.Clear()

        '○ ＤＢ削除
        Dim SQLDelNLineLoadTblStr As String =
          " DELETE FROM OIL.OIT0012_NLINELOAD WHERE FILENAME = @P01 AND LODDATE = @P02 AND DELFLG = '0'; "

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを回線別積込取込(日新)テーブルから取得する
        Dim SQLStr As String =
              " SELECT " _
            & "      ISNULL(RTRIM(OIT0012.FILENAME), '')         AS FILENAME " _
            & "    , ISNULL(RTRIM(OIT0012.REGISTRATIONDATE), '') AS REGISTRATIONDATE " _
            & "    , ISNULL(RTRIM(OIT0012.LODDATE), '')          AS LODDATE " _
            & "    , ISNULL(RTRIM(OIT0012.LINE), '')             AS LINE " _
            & "    , ISNULL(RTRIM(OIT0012.ARRSTATION), '')       AS ARRSTATION " _
            & "    , ISNULL(RTRIM(OIT0012.TRAINNO), '')          AS TRAINNO " _
            & " FROM oil.OIT0012_NLINELOAD OIT0012 " _
            & " WHERE " _
            & "     OIT0012.FILENAME = @P01 " _
            & " AND OIT0012.LODDATE  = @P02 " _
            & " AND OIT0012.DELFLG  <> @P03 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon),
                  SQLDel1cmd As New SqlCommand(SQLDelNLineLoadTblStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)      'ファイル名
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)          '積置日
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)   '削除フラグ

                PARA01.Value = OIT0003EXLUPtbl.Rows(0)("FILENAME")
                PARA02.Value = OIT0003EXLUPtbl.Rows(0)("LODDATE")
                PARA03.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003EXLDELtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003EXLDELtbl.Load(SQLdr)
                End Using

                '★削除対象データが存在した場合
                If OIT0003EXLDELtbl.Rows.Count <> 0 Then
                    '★削除実行(回線別積込取込(日新)テーブル)
                    Dim PARADELRL01 As SqlParameter = SQLDel1cmd.Parameters.Add("@P01", SqlDbType.NVarChar) 'ファイル名
                    Dim PARADELRL02 As SqlParameter = SQLDel1cmd.Parameters.Add("@P02", SqlDbType.NVarChar) '積置日
                    PARADELRL01.Value = OIT0003EXLDELtbl.Rows(0)("FILENAME")
                    PARADELRL02.Value = OIT0003EXLDELtbl.Rows(0)("LODDATE")
                    SQLDel1cmd.ExecuteNonQuery()
                    SQLDel1cmd.Dispose()
                End If

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_NLINELOAD_DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_NLINELOAD_DELETE"
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
    ''' 回線別積込取込(日新)TBL追加処理
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="sqlCon">接続オブジェクト</param>
    Protected Sub WW_INSERT_NLINELOAD(ByVal SQLcon As SqlConnection, Optional ByVal useFlg As String = Nothing)

        '追加SQL文･･･回線別積込取込(日新)TBL
        Dim SQLNLineLoadStr As String =
              " INSERT INTO OIL.OIT0012_NLINELOAD " _
            & " ( FILENAME  , REGISTRATIONDATE, LODDATE       , LINE" _
            & " , ARRSTATION, TRAINNO         , POINT         , OIL" _
            & " , TANKNO    , TRAINNODETAIL   , LOADINGTRAINNO, LOADINGTANKNO" _
            & " , DELFLG    , INITYMD         , INITUSER      , INITTERMID" _
            & " , UPDYMD    , UPDUSER         , UPDTERMID     , RECEIVEYMD)"

        SQLNLineLoadStr &=
              " VALUES" _
            & " ( @FILENAME  , @REGISTRATIONDATE, @LODDATE       , @LINE" _
            & " , @ARRSTATION, @TRAINNO         , @POINT         , @OIL" _
            & " , @TANKNO    , @TRAINNODETAIL   , @LOADINGTRAINNO, @LOADINGTANKNO" _
            & " , @DELFLG    , @INITYMD         , @INITUSER      , @INITTERMID" _
            & " , @UPDYMD    , @UPDUSER         , @UPDTERMID     , @RECEIVEYMD);"

        Try
            Using SQLNLineLoadcmd As New SqlCommand(SQLNLineLoadStr, SQLcon)

                Dim WW_DATENOW As DateTime = Date.Now
                Dim FILENAME As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@FILENAME", SqlDbType.NVarChar)                 'ファイル名(EXCEL)
                Dim REGISTRATIONDATE As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@REGISTRATIONDATE", SqlDbType.Date) '登録年月日(EXCEL)
                Dim LODDATE As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@LODDATE", SqlDbType.Date)                   '積込日(EXCEL)
                Dim LINE As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@LINE", SqlDbType.NVarChar)                         '回線(EXCEL)
                Dim ARRSTATION As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@ARRSTATION", SqlDbType.NVarChar)             '着駅(EXCEL)
                Dim TRAINNO As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar)                   '列車(EXCEL)
                Dim POINT As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@POINT", SqlDbType.NVarChar)                       'ポイント(EXCEL)
                Dim OIL As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@OIL", SqlDbType.NVarChar)                           '油種(EXCEL)
                Dim TANKNO As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar)                     'タンク車№(EXCEL)
                Dim TRAINNODETAIL As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@TRAINNODETAIL", SqlDbType.NVarChar)       '列車(EXCEL)
                Dim LOADINGTRAINNO As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@LOADINGTRAINNO", SqlDbType.NVarChar)     '列車(受注用)
                Dim LOADINGTANKNO As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@LOADINGTANKNO", SqlDbType.NVarChar)       'タンク車№(受注用)

                Dim DELFLG As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)         '削除フラグ
                Dim INITYMD As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@INITYMD", SqlDbType.DateTime)       '登録年月日
                Dim INITUSER As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@INITUSER", SqlDbType.NVarChar)     '登録ユーザーＩＤ
                Dim INITTERMID As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@INITTERMID", SqlDbType.NVarChar) '登録端末
                Dim UPDYMD As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)         '更新年月日
                Dim UPDUSER As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar)       '更新ユーザーＩＤ
                Dim UPDTERMID As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar)   '更新端末
                Dim RECEIVEYMD As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime) '集信日時

                Dim rg As New Regex("/")
                For Each OIT0003EXLUProw As DataRow In OIT0003EXLUPtbl.Select("LINE_HEADER<>''")
                    'ファイル名(EXCEL)
                    FILENAME.Value = OIT0003EXLUProw("FILENAME")
                    '登録年月日(EXCEL)
                    REGISTRATIONDATE.Value = OIT0003EXLUProw("DATERECEIVEYMD")
                    '積込日(EXCEL)
                    LODDATE.Value = OIT0003EXLUProw("LODDATE")
                    '回線(EXCEL)
                    LINE.Value = OIT0003EXLUProw("LINE_HEADER")
                    '着駅(EXCEL)
                    ARRSTATION.Value = OIT0003EXLUProw("ARRSTATION_HEADER")
                    '列車(EXCEL)
                    'TRAINNO.Value = OIT0003EXLUProw("TRAINNO_HEADER")
                    TRAINNO.Value = rg.Replace(OIT0003EXLUProw("TRAINNO_HEADER"), "")
                    'ポイント(EXCEL)
                    POINT.Value = OIT0003EXLUProw("POINT")
                    '油種(EXCEL)
                    OIL.Value = OIT0003EXLUProw("OIL_DETAIL")
                    'タンク車№(EXCEL)
                    TANKNO.Value = OIT0003EXLUProw("TANKNO_DETAIL")
                    '列車(EXCEL)
                    TRAINNODETAIL.Value = OIT0003EXLUProw("TRAINNO_DETAIL")
                    '列車(受注用)
                    LOADINGTRAINNO.Value = OIT0003EXLUProw("TRAINNO")
                    'タンク車№(受注用)
                    LOADINGTANKNO.Value = OIT0003EXLUProw("TANKNO")

                    '削除フラグ
                    DELFLG.Value = C_DELETE_FLG.ALIVE
                    '登録年月日
                    INITYMD.Value = Date.Now
                    '登録ユーザーＩＤ
                    INITUSER.Value = Master.USERID
                    '登録端末
                    INITTERMID.Value = Master.USERTERMID
                    '更新年月日
                    UPDYMD.Value = Date.Now
                    '更新ユーザーＩＤ
                    UPDUSER.Value = Master.USERID
                    '更新端末
                    UPDTERMID.Value = Master.USERTERMID
                    '集信日時
                    RECEIVEYMD.Value = C_DEFAULT_YMD

                    SQLNLineLoadcmd.CommandTimeout = 300
                    SQLNLineLoadcmd.ExecuteNonQuery()
                Next
                'CLOSE
                SQLNLineLoadcmd.Dispose()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_NLINELOAD_INSERT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_NLINELOAD_INSERT"
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
    ''' 回線別積込取込(日新)TBL更新処理(JOT油種に変換)
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Protected Sub WW_UPDATE_NLINELOAD(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0003EXLCHKtbl) Then
            OIT0003EXLCHKtbl = New DataTable
        End If

        If OIT0003EXLCHKtbl.Columns.Count <> 0 Then
            OIT0003EXLCHKtbl.Columns.Clear()
        End If

        OIT0003EXLCHKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLChkStr As String =
              " SELECT " _
            & "    OIM0029.value04   AS NISSHIN_OIL " _
            & " ,  OIM0029.KEYCODE05 AS OILCODE " _
            & " ,  OIM0029.KEYCODE06 AS OILNAME " _
            & " ,  OIM0029.KEYCODE07 AS OILKANA " _
            & " ,  OIM0029.KEYCODE08 AS SEGMENTOILCODE " _
            & " ,  OIM0029.KEYCODE09 AS SEGMENTOILNAME " _
            & " FROM OIL.OIM0029_CONVERT OIM0029 " _
            & " WHERE OIM0029.CLASS     = @P01" _
            & "   AND OIM0029.KEYCODE01 = @P02" _
            & "   AND OIM0029.DELFLG   <> @P03"
        Try
            Using SQLChkcmd As New SqlCommand(SQLChkStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLChkcmd.Parameters.Add("@P01", SqlDbType.NVarChar)     '分類
                Dim PARA02 As SqlParameter = SQLChkcmd.Parameters.Add("@P02", SqlDbType.NVarChar)     '受注営業所
                Dim PARA03 As SqlParameter = SQLChkcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA01.Value = "NISSHIN_OILMASTER"
                PARA02.Value = BaseDllConst.CONST_OFFICECODE_011402
                PARA03.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLChkcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003EXLCHKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003EXLCHKtbl.Load(SQLdr)
                End Using
                'CLOSE
                SQLChkcmd.Dispose()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_NLINELOAD_CHECK")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_NLINELOAD_CHECK"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        Try
            '更新SQL文･･･回線別積込取込(日新)TBLの各項目をを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0012_NLINELOAD " _
                    & "    SET LOADINGOILCODE          = @P05, " _
                    & "        LOADINGOILNAME          = @P06, " _
                    & "        LOADINGORDERINGTYPE     = @P07, " _
                    & "        LOADINGORDERINGOILNAME  = @P08, " _
                    & "        UPDYMD                  = @P09, " _
                    & "        UPDUSER                 = @P10, " _
                    & "        UPDTERMID               = @P11, " _
                    & "        RECEIVEYMD              = @P12  " _
                    & " WHERE " _
                    & "     FILENAME = @P01 " _
                    & " AND LODDATE  = @P02 " _
                    & " AND OIL      = @P03 " _
                    & " AND DELFLG  <> @P04 "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)              'ファイル名(EXCEL)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)                  '積込日(EXCEL)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar)              '油種(EXCEL)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)           '削除フラグ
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar)              '油種コード(JOT)
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar)              '油種名(JOT)
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar)              '油種コード(受発注用)(JOT)
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar)              '油種名(受発注用)(JOT)
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.DateTime)  '更新年月日
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)  '更新ユーザーＩＤ
            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar)  '更新端末
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)  '集信日時

            PARA01.Value = OIT0003EXLUPtbl.Rows(0)("FILENAME")
            PARA02.Value = OIT0003EXLUPtbl.Rows(0)("LODDATE")
            PARA04.Value = C_DELETE_FLG.DELETE
            PARA09.Value = Date.Now
            PARA10.Value = Master.USERID
            PARA11.Value = Master.USERTERMID
            PARA12.Value = C_DEFAULT_YMD

            For Each OIT0003CHKrow In OIT0003EXLCHKtbl.Rows
                PARA03.Value = OIT0003CHKrow("NISSHIN_OIL")
                PARA05.Value = OIT0003CHKrow("OILCODE")
                PARA06.Value = OIT0003CHKrow("OILNAME")
                PARA07.Value = OIT0003CHKrow("SEGMENTOILCODE")
                PARA08.Value = OIT0003CHKrow("SEGMENTOILNAME")

                SQLcmd.ExecuteNonQuery()
            Next

            'CLOSE
            SQLcmd.Dispose()

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_NLINELOAD_UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_NLINELOAD_UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        Try
            '追加SQL文･･･回線別積込取込(日新)WORK
            Dim SQLNLineLoadStr As String =
                  " DELETE FROM OIL.TMP0006_NLINELOAD; " _
                & " INSERT INTO OIL.TMP0006_NLINELOAD " _
                & " ( FILENAME      , REGISTRATIONDATE, LODDATE            , LINE" _
                & " , ARRSTATION    , TRAINNO         , POINT              , OIL" _
                & " , TANKNO        , TRAINNODETAIL   , LOADINGTRAINNO     , LOADINGTANKNO" _
                & " , LOADINGOILCODE, LOADINGOILNAME  , LOADINGORDERINGTYPE, LOADINGORDERINGOILNAME" _
                & " , DELFLG        , INITYMD         , INITUSER           , INITTERMID" _
                & " , UPDYMD        , UPDUSER         , UPDTERMID          , RECEIVEYMD)"

            SQLNLineLoadStr &=
                  " SELECT" _
                & "   FILENAME      , REGISTRATIONDATE, LODDATE            , LINE" _
                & " , ARRSTATION    , TRAINNO         , POINT              , OIL" _
                & " , TANKNO        , TRAINNODETAIL   , LOADINGTRAINNO     , LOADINGTANKNO" _
                & " , LOADINGOILCODE, LOADINGOILNAME  , LOADINGORDERINGTYPE, LOADINGORDERINGOILNAME" _
                & " , DELFLG        , INITYMD         , INITUSER           , INITTERMID" _
                & " , UPDYMD        , UPDUSER         , UPDTERMID          , RECEIVEYMD" _
                & " FROM OIL.OIT0012_NLINELOAD OIT0012" _
                & " WHERE " _
                & "     OIT0012.FILENAME = @P01" _
                & " AND OIT0012.LODDATE  = @P02"

            Using SQLNLineLoadcmd As New SqlCommand(SQLNLineLoadStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@P01", SqlDbType.NVarChar)      'ファイル名
                Dim PARA02 As SqlParameter = SQLNLineLoadcmd.Parameters.Add("@P02", SqlDbType.Date)          '積置日

                PARA01.Value = OIT0003EXLUPtbl.Rows(0)("FILENAME")
                PARA02.Value = OIT0003EXLUPtbl.Rows(0)("LODDATE")

                SQLNLineLoadcmd.CommandTimeout = 300
                SQLNLineLoadcmd.ExecuteNonQuery()

                'CLOSE
                SQLNLineLoadcmd.Dispose()
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_NLINELOAD_WORK_INSERT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_NLINELOAD_WORK_INSERT"
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
    ''' 受注データと回線別積込(日新)データとの紐づけ処理
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Protected Sub WW_LINKING_ORDER(ByVal SQLcon As SqlConnection, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        If IsNothing(OIT0003EXLINStbl) Then
            OIT0003EXLINStbl = New DataTable
        End If

        If OIT0003EXLINStbl.Columns.Count <> 0 Then
            OIT0003EXLINStbl.Columns.Clear()
        End If

        OIT0003EXLINStbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを回線別積込取込(日新)テーブルから取得する
        Dim SQLTankStr As String =
              " SELECT " _
            & "    FILENAME               AS FILENAME " _
            & " ,  REGISTRATIONDATE       AS REGISTRATIONDATE " _
            & " ,  LODDATE                AS LODDATE " _
            & " ,  LINE                   AS LINE " _
            & " ,  ARRSTATION             AS ARRSTATION " _
            & " ,  TRAINNO                AS TRAINNO " _
            & " ,  POINT                  AS POINT " _
            & " ,  OIL                    AS OIL " _
            & " ,  TANKNO                 AS TANKNO " _
            & " ,  TRAINNODETAIL          AS TRAINNODETAIL " _
            & " ,  LOADINGTRAINNO         AS LOADINGTRAINNO " _
            & " ,  LOADINGTANKNO          AS LOADINGTANKNO " _
            & " ,  LOADINGOILCODE         AS LOADINGOILCODE " _
            & " ,  LOADINGOILNAME         AS LOADINGOILNAME " _
            & " ,  LOADINGORDERINGTYPE    AS LOADINGORDERINGTYPE " _
            & " ,  LOADINGORDERINGOILNAME AS LOADINGORDERINGOILNAME " _
            & " ,  '0'                    AS USEFLAG " _
            & " FROM OIL.TMP0006_NLINELOAD TMP0006 " _
            & " WHERE TMP0006.LOADINGTRAINNO <> '' " _
            & " ORDER BY " _
            & "       TMP0006.LOADINGTRAINNO" _
            & "     , TMP0006.TRAINNODETAIL" _
            & "     , TMP0006.LINE" _
            & "     , TMP0006.POINT "

        Try
            Using SQLTankcmd As New SqlCommand(SQLTankStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLTankcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003EXLINStbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003EXLINStbl.Load(SQLdr)
                End Using
                'CLOSE
                SQLTankcmd.Dispose()
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_GET_NLINELOAD")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_GET_NLINELOAD"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        If IsNothing(OIT0003EXLODRtbl) Then
            OIT0003EXLODRtbl = New DataTable
        End If

        If OIT0003EXLODRtbl.Columns.Count <> 0 Then
            OIT0003EXLODRtbl.Columns.Clear()
        End If

        OIT0003EXLODRtbl.Clear()

        If IsNothing(OIT0003EXLODRALLtbl) Then
            OIT0003EXLODRALLtbl = New DataTable
        End If

        If OIT0003EXLODRALLtbl.Columns.Count <> 0 Then
            OIT0003EXLODRALLtbl.Columns.Clear()
        End If

        OIT0003EXLODRALLtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
              " SELECT " _
            & "    OIT0002.ORDERNO         AS ORDERNO " _
            & " ,  OIT0003.DETAILNO        AS DETAILNO " _
            & " ,  OIT0002.TRAINNO         AS TRAINNO " _
            & " ,  OIT0002.TRAINNAME       AS TRAINNAME " _
            & " ,  OIT0002.LODDATE         AS LODDATE " _
            & " ,  OIT0002.DEPDATE         AS DEPDATE " _
            & " ,  OIT0003.SHIPORDER       AS SHIPORDER " _
            & " ,  OIT0003.TANKNO          AS TANKNO " _
            & " ,  OIT0003.OILCODE         AS OILCODE " _
            & " ,  OIT0003.OILNAME         AS OILNAME " _
            & " ,  OIT0003.ORDERINGTYPE    AS ORDERINGTYPE " _
            & " ,  OIT0003.ORDERINGOILNAME AS ORDERINGOILNAME " _
            & " ,  CASE " _
            & "      WHEN ISNULL(OIT0003.SECONDCONSIGNEECODE, '') <> '' " _
            & "        THEN OIT0003.SECONDCONSIGNEECODE " _
            & "      ELSE OIT0002.CONSIGNEECODE " _
            & "    END                     AS CONSIGNEECODE " _
            & " ,  '0'                     AS USEFLAG " _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & " AND OIT0003.DELFLG <> @DELFLG " _
            & " WHERE OIT0002.OFFICECODE   = @OFFICECODE" _
            & "   AND OIT0002.TRAINNO      = @TRAINNO" _
            & "   AND OIT0002.LODDATE      = @LODDATE" _
            & "   AND OIT0002.ORDERSTATUS <> @ORDERSTATUS" _
            & "   AND OIT0002.DELFLG      <> @DELFLG"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar)     '受注営業所
                Dim P_TRAINNO As SqlParameter = SQLcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar)           '本線列車№
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.Date)               '積込日(予定)
                Dim P_ORDERSTATUS As SqlParameter = SQLcmd.Parameters.Add("@ORDERSTATUS", SqlDbType.NVarChar)   '受注進行ステータス
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)          '削除フラグ
                P_OFFICECODE.Value = BaseDllConst.CONST_OFFICECODE_011402
                P_ORDERSTATUS.Value = BaseDllConst.CONST_ORDERSTATUS_900
                P_DELFLG.Value = C_DELETE_FLG.DELETE


                '回線別積込取込(日新)の【タンク車No】を受注データに紐づけする。
                Dim strTrainNo As String = ""
                Dim shipOderAsc As Integer = 0
                Dim shipOderDesc As Integer = 0
                For Each OIT0003INSrow In OIT0003EXLINStbl.Rows

                    '初回、または列車№が変更されたら、受注データを再度取得する。
                    If strTrainNo = "" _
                        OrElse (strTrainNo <> "" _
                                AndAlso strTrainNo <> Convert.ToString(OIT0003INSrow("LOADINGTRAINNO"))) Then

                        '★★★紐づけした受注データを保存(チェック用)★★★
                        For Each OIT0003ODRrow In OIT0003EXLODRtbl.Rows
                            OIT0003EXLODRALLtbl.ImportRow(OIT0003ODRrow)
                        Next

                        P_TRAINNO.Value = OIT0003INSrow("LOADINGTRAINNO")
                        P_LODDATE.Value = OIT0003INSrow("LODDATE")

                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                            If OIT0003EXLODRtbl.Columns.Count = 0 Then
                                '○ フィールド名とフィールドの型を取得
                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIT0003EXLODRtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                    OIT0003EXLODRALLtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIT0003EXLODRtbl.Clear()

                            '○ テーブル検索結果をテーブル格納
                            OIT0003EXLODRtbl.Load(SQLdr)
                        End Using

                        '発送順をリセット
                        shipOderAsc = 1
                        shipOderDesc = OIT0003EXLODRtbl.AsEnumerable.
                            Where(Function(x As DataRow) x("USEFLAG") = "0").
                            Where(Function(x As DataRow) x("TRAINNO") = OIT0003INSrow("LOADINGTRAINNO")).Count
                    End If

                    '★受注データの油種と回線別積込取込(日新)の油種が一致したらタンク車№を設定
                    For Each OIT0003ODRrow In OIT0003EXLODRtbl.Select("USEFLAG = '0'")
                        If OIT0003ODRrow("OILCODE") = OIT0003INSrow("LOADINGOILCODE") _
                            AndAlso OIT0003ODRrow("ORDERINGTYPE") = OIT0003INSrow("LOADINGORDERINGTYPE") Then
                            OIT0003ODRrow("TANKNO") = OIT0003INSrow("LOADINGTANKNO")
                            OIT0003ODRrow("USEFLAG") = "1"
                            OIT0003INSrow("USEFLAG") = "1"
                            If (OIT0003ODRrow("CONSIGNEECODE") = "10" OrElse OIT0003ODRrow("CONSIGNEECODE") = "20") Then
                                '北信と甲府は発送順を昇順にする。その他は降順。
                                OIT0003ODRrow("SHIPORDER") = shipOderAsc.ToString
                                shipOderAsc += 1
                            Else
                                OIT0003ODRrow("SHIPORDER") = shipOderDesc.ToString
                                shipOderDesc -= 1
                            End If
                            Exit For

                            '★★未添加灯油は北信と甲府。OTは逆に未添加灯油はなく灯油。
                            '　　そのため、タンク車№を紐づける際にはこの内容を加味し設定する必要あり。
                        ElseIf (OIT0003ODRrow("OILCODE") = BaseDllConst.CONST_MTTank _
                                    AndAlso OIT0003INSrow("LOADINGOILCODE") = BaseDllConst.CONST_TTank) _
                                OrElse (OIT0003ODRrow("OILCODE") = BaseDllConst.CONST_TTank _
                                    AndAlso OIT0003INSrow("LOADINGOILCODE") = BaseDllConst.CONST_MTTank) Then
                            OIT0003ODRrow("TANKNO") = OIT0003INSrow("LOADINGTANKNO")
                            OIT0003ODRrow("USEFLAG") = "1"
                            OIT0003INSrow("USEFLAG") = "1"
                            If (OIT0003ODRrow("CONSIGNEECODE") = "10" OrElse OIT0003ODRrow("CONSIGNEECODE") = "20") Then
                                '北信と甲府は発送順を昇順にする。その他は降順。
                                OIT0003ODRrow("SHIPORDER") = shipOderAsc.ToString
                                shipOderAsc += 1
                            Else
                                OIT0003ODRrow("SHIPORDER") = shipOderDesc.ToString
                                shipOderDesc -= 1
                            End If
                            Exit For
                        End If
                    Next


                    '本線列車№を保存(比較用)
                    strTrainNo = OIT0003INSrow("LOADINGTRAINNO")
                Next

                '★★★紐づけした受注データを保存(チェック用)★★★
                For Each OIT0003ODRrow In OIT0003EXLODRtbl.Rows
                    OIT0003EXLODRALLtbl.ImportRow(OIT0003ODRrow)
                Next

                '○日新回線別予定表チェック
                Dim WW_ERRCOCE As String = ""
                WW_CHECK_NEGISHI_LOADPLAN(OIT0003EXLINStbl, OIT0003EXLODRALLtbl, WW_ERRCOCE)
                If WW_ERRCOCE = "ERR" Then
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    '○(受注明細TBL)タンク車№更新
                    If OIT0003EXLODRALLtbl.Rows.Count <> 0 Then WW_UpdateOrderTankNo(SQLcon, OIT0003EXLODRALLtbl)
                End If

                'CLOSE
                SQLcmd.Dispose()
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_LINKING_ORDER")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_LINKING_ORDER"
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
    ''' 日新回線別予定表チェック
    ''' </summary>
    ''' <param name="I_DT1">タンク車回線別積込チェック用</param>
    ''' <param name="I_DT2">受注データチェック用</param>
    ''' <param name="O_RTN">戻り値</param>
    ''' <remarks></remarks>
    Protected Sub WW_CHECK_NEGISHI_LOADPLAN(ByVal I_DT1 As DataTable, ByVal I_DT2 As DataTable, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_ErrorMES As String = ""

        '★受注の油種数と比べて回線別積込予定表で割当てしている油種数が多い
        For Each I_DT1row In I_DT1.Select("USEFLAG = '0'")
            WW_ErrorMES = Convert.ToString(I_DT1row("LOADINGTRAINNO")) + "列車"
            WW_ErrorMES &= "　" + Convert.ToString(I_DT1row("LOADINGORDERINGOILNAME"))
            Master.Output(C_MESSAGE_NO.OIL_UPLOAD_ERR_NEGISHI_LOAD_OILOVER_MESSAGE, C_MESSAGE_TYPE.ERR,
                          I_PARA01:=WW_ErrorMES, needsPopUp:=True)

            O_RTN = "ERR"
            Exit Sub
        Next

        '★受注の油種数と比べて回線別積込予定表で割当てしている油種数が少ない
        For Each I_DT2row In I_DT2.Select("USEFLAG = '0'")
            WW_ErrorMES = Convert.ToString(I_DT2row("TRAINNO")) + "列車"
            WW_ErrorMES &= "　" + Convert.ToString(I_DT2row("ORDERINGOILNAME"))
            Master.Output(C_MESSAGE_NO.OIL_UPLOAD_ERR_NEGISHI_LOAD_OILLESS_MESSAGE, C_MESSAGE_TYPE.ERR,
                          I_PARA01:=WW_ErrorMES, needsPopUp:=True)

            O_RTN = "ERR"
            Exit Sub
        Next

    End Sub
#End Region

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
            '(帳票ポップアップ)積込日
            Case "txtReportLodDate"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.txtReportLodDate.Text = ""
                    Else
                        Me.txtReportLodDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.txtReportLodDate.Focus()
            '(帳票ポップアップ)列車番号(臨海)
            Case "txtReportRTrainNo"
                Me.txtReportRTrainNo.Text = WW_SelectValue
                Me.txtReportRTrainNo.Focus()
            '(帳票ポップアップ)列車番号
            Case "txtReportTrainNo"
                Me.txtReportTrainNo.Text = WW_SelectValue
                Me.txtReportTrainNo.Focus()
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
            Case "txtReportLodDate"          '(帳票)積込日
                Me.txtReportLodDate.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
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
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

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
        CS0013ProfView.TBLOBJ = pnlListArea
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
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String,
                               Optional ByVal I_OFFICECODE As String = Nothing)

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

                Case "CTRAINNUMBER"     '列車番号(在線)
                    If Not IsNothing(I_OFFICECODE) Then
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CTRAINNUMBER, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(I_OFFICECODE, "CTRAINNUMBER_FIND"))
                    End If

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' (受注TBL)受注進行ステータス(受注キャンセル)更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderStatusCancel()

        Dim StatusChk As Boolean = False

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        '■■■ OIT0003tbl関連の受注TBLの「受注進行ステータス」を「900:受注キャンセル」に更新 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0002_ORDER       " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        ORDERSTATUS = @P15       " _
                    & "  WHERE ORDERNO     = @P01       " _
                    & "    AND DELFLG     <> '1'       ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            '受注キャンセルする情報取得用
            Dim strOrderSts As String = ""          '受注進行ステータス
            Dim strDepstation As String = ""        '発駅コード
            Dim strArrstation As String = ""        '着駅コード
            Dim strLinkNoMade As String = ""        '作成_貨車連結順序表№

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)
            Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar)

            '選択されている行の受注進行ステータスを「900:受注キャンセル」に更新
            For Each OIT0003UPDrow In OIT0003tbl.Rows
                If OIT0003UPDrow("OPERATION") = "on" Then
                    PARA01.Value = OIT0003UPDrow("ORDERNO")
                    work.WF_SEL_ORDERNUMBER.Text = OIT0003UPDrow("ORDERNO")
                    strOrderSts = OIT0003UPDrow("ORDERSTATUS")
                    strDepstation = OIT0003UPDrow("DEPSTATION")
                    strArrstation = OIT0003UPDrow("ARRSTATION")
                    strLinkNoMade = OIT0003UPDrow("TANKLINKNOMADE")

                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD
                    PARA15.Value = BaseDllConst.CONST_ORDERSTATUS_900

                    OIT0003UPDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_900
                    CODENAME_get("ORDERSTATUS", OIT0003UPDrow("ORDERSTATUS"), OIT0003UPDrow("ORDERSTATUSNAME"), WW_DUMMY)

                    SQLcmd.ExecuteNonQuery()

                    '### START 受注履歴テーブルの追加(2020/03/26) #############
                    WW_InsertOrderHistory(SQLcon)
                    '### END   ################################################

                    '### START 受注キャンセル時のタンク車所在の更新処理を追加(2020/03/31) ###############################
                    For Each OIT0003His2tblrow In OIT0003His2tbl.Rows
                        Select Case strOrderSts
                            'Case BaseDllConst.CONST_ORDERSTATUS_100

                            '    '### 何もしない####################

                            '200:手配　～　310：手配完了
                            Case BaseDllConst.CONST_ORDERSTATUS_100,
                                 BaseDllConst.CONST_ORDERSTATUS_200,
                                 BaseDllConst.CONST_ORDERSTATUS_205,
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
                                 BaseDllConst.CONST_ORDERSTATUS_305,
                                 BaseDllConst.CONST_ORDERSTATUS_310,
                                 BaseDllConst.CONST_ORDERSTATUS_320
                                '★タンク車所在の更新(タンク車№を再度選択できるようにするため)
                                '引数１：所在地コード　⇒　変更なし(空白)
                                '引数２：タンク車状態　⇒　変更あり("3"(到着))
                                '引数３：積車区分　　　⇒　変更なし(空白)
                                '引数４：タンク車状況　⇒　変更あり("1"(残車))
                                'WW_UpdateTankShozai("", "3", "", I_ORDERNO:=OIT0003His2tblrow("ORDERNO"),
                                '                    I_TANKNO:=OIT0003His2tblrow("TANKNO"), I_SITUATION:="1",
                                '                    I_ActualEmparrDate:=Now.ToString("yyyy/MM/dd"), upActualEmparrDate:=True)
                                WW_UpdateTankShozai("", "3", "", I_ORDERNO:=OIT0003His2tblrow("ORDERNO"),
                                                    I_TANKNO:=OIT0003His2tblrow("TANKNO"), I_SITUATION:="1",
                                                    I_ActualEmparrDate:="", upActualEmparrDate:=True)

                            '350：受注確定
                            Case BaseDllConst.CONST_ORDERSTATUS_350
                                StatusChk = True

                                '★タンク車所在の更新(タンク車№を再度選択できるようにするため)
                                '引数１：所在地コード　⇒　変更あり(発駅)
                                '引数２：タンク車状態　⇒　変更あり("3"(到着))
                                '引数３：積車区分　　　⇒　変更なし(空白)
                                WW_UpdateTankShozai(strDepstation, "3", "", I_ORDERNO:=OIT0003His2tblrow("ORDERNO"),
                                                    I_TANKNO:=OIT0003His2tblrow("TANKNO"),
                                                    I_ActualEmparrDate:=Now.ToString("yyyy/MM/dd"), upActualEmparrDate:=True)

                            '400：受入確認中, 450:受入確認中(受入日入力)
                            Case BaseDllConst.CONST_ORDERSTATUS_400,
                                 BaseDllConst.CONST_ORDERSTATUS_450

                                '### 何もしない####################

                            '※"500：輸送完了"のステータス以降についてはキャンセルができない仕様だが
                            '　条件は追加しておく
                            Case BaseDllConst.CONST_ORDERSTATUS_500,
                                 BaseDllConst.CONST_ORDERSTATUS_550,
                                 BaseDllConst.CONST_ORDERSTATUS_600,
                                 BaseDllConst.CONST_ORDERSTATUS_700,
                                 BaseDllConst.CONST_ORDERSTATUS_800,
                                 BaseDllConst.CONST_ORDERSTATUS_900

                                '### 何もしない####################

                        End Select
                    Next
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            '受注進行ステータスの状態によって、貨車連結順序表を利用不可にする。
            Select Case strOrderSts
                Case BaseDllConst.CONST_ORDERSTATUS_350,
                     BaseDllConst.CONST_ORDERSTATUS_400,
                     BaseDllConst.CONST_ORDERSTATUS_450

                    WW_UpdateLink(strLinkNoMade, "2")

                Case BaseDllConst.CONST_ORDERSTATUS_100,
                     BaseDllConst.CONST_ORDERSTATUS_200,
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
                     BaseDllConst.CONST_ORDERSTATUS_320,
                     BaseDllConst.CONST_ORDERSTATUS_500,
                     BaseDllConst.CONST_ORDERSTATUS_550,
                     BaseDllConst.CONST_ORDERSTATUS_600,
                     BaseDllConst.CONST_ORDERSTATUS_700,
                     BaseDllConst.CONST_ORDERSTATUS_800,
                     BaseDllConst.CONST_ORDERSTATUS_900

                    '### 何もしない####################

            End Select
            '### END  ###########################################################################################

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        '○メッセージ表示
        If StatusChk = True Then
            Master.Output(C_MESSAGE_NO.OIL_TANKNO_INFO_MESSAGE, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        Else
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

    End Sub

    ''' <summary>
    ''' (受注明細TBL)タンク車№更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderTankNo(ByVal SQLcon As SqlConnection, ByVal OIT0003tbl As DataTable)

        Try
            'DataBase接続文字
            'Dim SQLcon = CS0050SESSION.getConnection
            'SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr As String =
                " UPDATE OIL.OIT0003_DETAIL " _
                & "    SET TANKNO      = @TANKNO, " _
                & "        SHIPORDER   = @SHIPORDER, " _
                & "        UPDYMD      = @UPDYMD, " _
                & "        UPDUSER     = @UPDUSER, " _
                & "        UPDTERMID   = @UPDTERMID, " _
                & "        RECEIVEYMD  = @RECEIVEYMD  " _
                & "  WHERE ORDERNO     = @ORDERNO  " _
                & "    AND DETAILNO    = @DETAILNO  " _
                & "    AND DELFLG     <> @DELFLG; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", System.Data.SqlDbType.NVarChar)
            Dim P_DETAILNO As SqlParameter = SQLcmd.Parameters.Add("@DETAILNO", System.Data.SqlDbType.NVarChar)
            Dim P_TANKNO As SqlParameter = SQLcmd.Parameters.Add("@TANKNO", System.Data.SqlDbType.NVarChar)
            Dim P_SHIPORDER As SqlParameter = SQLcmd.Parameters.Add("@SHIPORDER", System.Data.SqlDbType.NVarChar)
            Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
            P_DELFLG.Value = C_DELETE_FLG.DELETE
            P_UPDYMD.Value = Date.Now
            P_UPDUSER.Value = Master.USERID
            P_UPDTERMID.Value = Master.USERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            For Each OIT0003ODRrow In OIT0003tbl.Rows
                P_ORDERNO.Value = OIT0003ODRrow("ORDERNO")
                P_DETAILNO.Value = OIT0003ODRrow("DETAILNO")
                P_TANKNO.Value = OIT0003ODRrow("TANKNO")
                P_SHIPORDER.Value = OIT0003ODRrow("SHIPORDER")

                SQLcmd.ExecuteNonQuery()
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_TANKNO UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_TANKNO UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

#Region "帳票ポップアップ"
    ''' <summary>
    ''' 帳票ポップアップ(営業所(チェックボックス)選択)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_TyohyoSalesOfficeSelect()

        '選択したチェックボックス(営業所)の名称を取得
        work.WF_SEL_TH_ORDERSALESOFFICENAME.Text = tileSalesOffice.GetSelectedSingleText()
        '選択したチェックボックス(営業所)のコードを取得
        work.WF_SEL_TH_ORDERSALESOFFICECODE.Text = tileSalesOffice.GetSelectedSingleValue()

        '★初期化
        '託送指示(ラジオボタン)
        Me.rbDeliveryBtn.Checked = False
        '積込指示(ラジオボタン)
        Me.rbLoadBtn.Checked = False
        '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
        'OT積込指示(ラジオボタン)
        Me.rbOTLoadBtn.Checked = False
        '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
        '出荷予定(ラジオボタン)
        Me.rbShipBtn.Checked = False
        '入線方(ラジオボタン)
        Me.rbLineBtn.Checked = False
        '積込予定(根岸)(ラジオボタン)
        Me.rbNegishiLoadBtn.Checked = False
        '積込実績(北信)(ラジオボタン)
        Me.rbActualLoad10Btn.Checked = False
        '積込実績(甲府)(ラジオボタン)
        Me.rbActualLoad20Btn.Checked = False
        '積込予定(甲子)(ラジオボタン)
        Me.rbKinoeneLoadBtn.Checked = False
        '### 20201106 START 指摘票No195(空回日報対応) #################################################
        Me.rbKuukaiBtn.Checked = False
        '### 20201106 END   指摘票No195(空回日報対応) #################################################
        Me.rbFillingPointBtn.Checked = False
        'タンク車発送実績
        Me.rbTankDispatchBtn.Checked = False
        'タンク車発送実績(コウショウ高崎)
        Me.rbTankDispatch30Btn.Checked = False
        'タンク車発送実績(ＪＯＮＥＴ松本)
        Me.rbTankDispatch40Btn.Checked = False
        'タンク車発送実績(構内取り)
        Me.rbTankDispatch54Btn.Checked = False
        '出荷実績
        Me.rbActualShipBtn.Checked = False
        '連結順序表
        Me.rbConcatOederBtn.Checked = False
        'タンク車出荷連絡書
        Me.rbShipContactBtn.Checked = False

        '託送指示(ラジオボタン)を非表示
        Me.rbDeliveryBtn.Visible = False
        '託送指示(CSV)(ラジオボタン)を非表示
        Me.rbDeliveryCSVBtn.Visible = False
        '積込指示(ラジオボタン)を非表示
        Me.rbLoadBtn.Visible = False
        '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
        'OT積込指示(ラジオボタン)
        Me.rbOTLoadBtn.Visible = False
        '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
        '出荷予定(ラジオボタン)を非表示
        Me.rbShipBtn.Visible = False
        '入線方(ラジオボタン)を非表示
        Me.rbLineBtn.Visible = False
        '積込予定(根岸)(ラジオボタン)を非表示
        Me.rbNegishiLoadBtn.Visible = False
        '積込実績(北信)(ラジオボタン)
        Me.rbActualLoad10Btn.Visible = False
        '積込実績(甲府)(ラジオボタン)
        Me.rbActualLoad20Btn.Visible = False
        '積込予定(甲子)(ラジオボタン)を非表示
        Me.rbKinoeneLoadBtn.Visible = False
        '### 20201106 START 指摘票No195(空回日報対応) #################################################
        Me.rbKuukaiBtn.Visible = False
        '### 20201106 END   指摘票No195(空回日報対応) #################################################
        Me.rbFillingPointBtn.Visible = False
        'タンク車発送実績
        Me.rbTankDispatchBtn.Visible = False
        'タンク車発送実績(コウショウ高崎)
        Me.rbTankDispatch30Btn.Visible = False
        'タンク車発送実績(ＪＯＮＥＴ松本)
        Me.rbTankDispatch40Btn.Visible = False
        'タンク車発送実績(構内取り)
        Me.rbTankDispatch54Btn.Visible = False
        '出荷実績
        Me.rbActualShipBtn.Visible = False
        '連結順序表
        Me.rbConcatOederBtn.Visible = False
        'タンク車出荷連絡書
        Me.rbShipContactBtn.Visible = False

        '列車番号(臨海)(テキストボックス)
        Me.txtReportRTrainNo.Text = ""
        '列車番号(テキストボックス)
        Me.txtReportTrainNo.Text = ""

        Select Case work.WF_SEL_TH_ORDERSALESOFFICECODE.Text
            '◯仙台新港営業所
            Case BaseDllConst.CONST_OFFICECODE_010402
                '積込指示(ラジオボタン)を表示
                Me.rbLoadBtn.Visible = True
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                'OT積込指示(ラジオボタン)を表示
                Me.rbOTLoadBtn.Visible = True
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
                'タンク車発送実績(ラジオボタン)を表示
                Me.rbTankDispatchBtn.Visible = True

            '◯五井営業所
            Case BaseDllConst.CONST_OFFICECODE_011201
                '託送指示(CSV)(ラジオボタン)を表示
                'Me.rbDeliveryCSVBtn.Visible = True
                '積込指示(ラジオボタン)を表示
                Me.rbLoadBtn.Visible = True
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                ''OT積込指示(ラジオボタン)を表示
                'Me.rbOTLoadBtn.Visible = True
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
                '出荷予定(ラジオボタン)を表示
                Me.rbShipBtn.Visible = True
                '充填ポイント表(ラジオボタン)を表示
                Me.rbFillingPointBtn.Visible = True

            '◯甲子営業所
            Case BaseDllConst.CONST_OFFICECODE_011202
                '託送指示(CSV)(ラジオボタン)を表示
                'Me.rbDeliveryCSVBtn.Visible = True
                '積込指示(ラジオボタン)を表示
                Me.rbLoadBtn.Visible = True
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                ''OT積込指示(ラジオボタン)を表示
                'Me.rbOTLoadBtn.Visible = True
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
                '積込予定(甲子)(ラジオボタン)を表示
                Me.rbKinoeneLoadBtn.Visible = True
                '出荷予定(ラジオボタン)を表示
                Me.rbShipBtn.Visible = True

            '◯袖ヶ浦営業所
            Case BaseDllConst.CONST_OFFICECODE_011203
                '託送指示(CSV)(ラジオボタン)を表示
                'Me.rbDeliveryCSVBtn.Visible = True
                '積込指示(ラジオボタン)を表示
                Me.rbLoadBtn.Visible = True
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                ''OT積込指示(ラジオボタン)を表示
                'Me.rbOTLoadBtn.Visible = True
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
                '入線方(ラジオボタン)を表示
                Me.rbLineBtn.Visible = True
                '### 20201106 START 指摘票No195(空回日報対応) #################################################
                '空回日報(ラジオボタン)を表示
                Me.rbKuukaiBtn.Visible = True
                '### 20201106 END   指摘票No195(空回日報対応) #################################################
                'タンク車発送実績(コウショウ高崎)(ラジオボタン)を表示
                Me.rbTankDispatch30Btn.Visible = True
                'タンク車発送実績(ＪＯＮＥＴ松本)(ラジオボタン)を表示
                Me.rbTankDispatch40Btn.Visible = True
                'タンク車発送実績(構内取り)(ラジオボタン)を表示
                Me.rbTankDispatch54Btn.Visible = True
                '出荷実績(ラジオボタン)を表示
                'Me.rbActualShipBtn.Visible = True
                '連結順序表(ラジオボタン)を表示
                Me.rbConcatOederBtn.Visible = True
                '出荷予定(ラジオボタン)を表示
                Me.rbShipBtn.Visible = True

            '◯根岸営業所
            Case BaseDllConst.CONST_OFFICECODE_011402
                '### 20210210 START 関東支店の要望にて廃止 ####################################################
                ''積込指示(ラジオボタン)を表示
                'Me.rbLoadBtn.Visible = True
                '### 20210210 END   関東支店の要望にて廃止 ####################################################
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                ''OT積込指示(ラジオボタン)を表示
                'Me.rbOTLoadBtn.Visible = True
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
                '出荷予定(ラジオボタン)を表示
                Me.rbShipBtn.Visible = True
                '積込予定(根岸)(ラジオボタン)を表示
                Me.rbNegishiLoadBtn.Visible = True
                '積込実績(北信)(ラジオボタン)を表示
                Me.rbActualLoad10Btn.Visible = True
                '積込実績(甲府)(ラジオボタン)を表示
                Me.rbActualLoad20Btn.Visible = True

            '◯四日市営業所
            Case BaseDllConst.CONST_OFFICECODE_012401
                '託送指示(ラジオボタン)を表示
                'Me.rbDeliveryBtn.Visible = True
                '積込指示(ラジオボタン)を表示
                Me.rbLoadBtn.Visible = True
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                ''OT積込指示(ラジオボタン)を表示
                'Me.rbOTLoadBtn.Visible = True
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
                '出荷予定(ラジオボタン)を表示
                Me.rbShipBtn.Visible = True

            '◯三重塩浜営業所
            Case BaseDllConst.CONST_OFFICECODE_012402
                '託送指示(ラジオボタン)を表示
                'Me.rbDeliveryBtn.Visible = True
                '積込指示(ラジオボタン)を表示
                Me.rbLoadBtn.Visible = True
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                ''OT積込指示(ラジオボタン)を表示
                'Me.rbOTLoadBtn.Visible = True
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################
                '出荷実績
                Me.rbActualShipBtn.Visible = True
                '連結順序表
                Me.rbConcatOederBtn.Visible = True
                'タンク車出荷連絡書
                Me.rbShipContactBtn.Visible = True
                '出荷予定(ラジオボタン)を表示
                Me.rbShipBtn.Visible = True

            Case Else
                '### すべてのラジオボタン非表示のまま ##########

        End Select

    End Sub

    Protected Sub WF_ReportSelect()

        '選択したチェックボックス(営業所)の名称を取得
        work.WF_SEL_TH_ORDERSALESOFFICENAME.Text = tileSalesOffice.GetSelectedSingleText()
        '選択したチェックボックス(営業所)のコードを取得
        work.WF_SEL_TH_ORDERSALESOFFICECODE.Text = tileSalesOffice.GetSelectedSingleValue()

        '初期化
        '○入線方 関連
        Me.divRTrainNo.Visible = False
        Me.txtReportRTrainNo.Text = ""
        Me.ChkSameTimeLineChk.Checked = False
        '○タンク車発送実績 関連
        Me.divTrainNo.Visible = False
        Me.txtReportTrainNo.Text = ""
        '○OT積込指示書 関連
        Me.divEndMonthChk.Visible = False
        Me.ChkEndMonthChk.Checked = False

        Select Case True
            Case Me.rbDeliveryBtn.Checked           '託送指示
                '何もしない
            Case Me.rbDeliveryCSVBtn.Checked        '託送指示(CSV)
                '何もしない
            Case Me.rbShipBtn.Checked               '出荷予定
                '何もしない
            Case Me.rbFillingPointBtn.Checked       '充填ポイント表
                '何もしない
            Case Me.rbKinoeneLoadBtn.Checked        '回線別指示書<br>(甲子)
                '何もしない
            Case Me.rbNegishiLoadBtn.Checked        '回線別(根岸)
                '何もしない
            Case Me.rbActualLoad10Btn.Checked       '積込実績(北信)
                '何もしない
            Case Me.rbActualLoad20Btn.Checked       '積込実績(甲府)
                '何もしない
            Case Me.rbLineBtn.Checked               '入線方
                Me.divRTrainNo.Visible = True
            Case Me.rbKuukaiBtn.Checked             '空回日報
                '何もしない
            Case Me.rbLoadBtn.Checked               '積込指示書
                '何もしない
            Case Me.rbOTLoadBtn.Checked             'OT積込指示
                Me.divEndMonthChk.Visible = True
            Case Me.rbTankDispatchBtn.Checked       'タンク車発送実績
                Me.divTrainNo.Visible = True
            Case Me.rbTankDispatch30Btn.Checked     'タンク車発送実績<br>(コウショウ高崎)
                Me.divTrainNo.Visible = True
                Me.txtReportTrainNo.Text = CONST_SODE_TRAIN_8877
            Case Me.rbTankDispatch40Btn.Checked     'タンク車発送実績<br>(ＪＯＮＥＴ松本)
                Me.divTrainNo.Visible = True
                Me.txtReportTrainNo.Text = CONST_SODE_TRAIN_5972
            Case Me.rbTankDispatch54Btn.Checked     'タンク車発送実績<br>(構内取り)
                Me.divTrainNo.Visible = True
                Me.txtReportTrainNo.Text = CONST_SODE_TRAIN_8877
            Case Me.rbActualShipBtn.Checked         '出荷実績
                Me.divTrainNo.Visible = True
            Case Me.rbConcatOederBtn.Checked        '連結順序表
                Me.divTrainNo.Visible = True
                If work.WF_SEL_TH_ORDERSALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then
                    '袖ヶ浦のみ初期値を設定
                    Me.txtReportTrainNo.Text = CONST_SODE_TRAIN_5972
                End If
            Case Me.rbShipContactBtn.Checked           'タンク車出荷連絡書
                Me.divTrainNo.Visible = True
        End Select

    End Sub

    ''' <summary>
    ''' 帳票ポップアップ(ダウンロードボタン押下)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_TyohyoDownloadClick()

        '◯ 積込日(空白)チェック
        If Me.txtReportLodDate.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "積込日が未設定です。", needsPopUp:=True)
            Exit Sub
        End If

        '◯ 営業所(未選択)チェック
        If work.WF_SEL_TH_ORDERSALESOFFICECODE.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "営業所が未選択です。", needsPopUp:=True)
            Exit Sub
        End If

        '◯ 帳票(未選択)チェック
        If Me.rbDeliveryBtn.Checked = False _
            AndAlso Me.rbDeliveryCSVBtn.Checked = False _
            AndAlso Me.rbLoadBtn.Checked = False _
            AndAlso Me.rbOTLoadBtn.Checked = False _
            AndAlso Me.rbShipBtn.Checked = False _
            AndAlso Me.rbLineBtn.Checked = False _
            AndAlso Me.rbNegishiLoadBtn.Checked = False _
            AndAlso Me.rbActualLoad10Btn.Checked = False _
            AndAlso Me.rbActualLoad20Btn.Checked = False _
            AndAlso Me.rbKinoeneLoadBtn.Checked = False _
            AndAlso Me.rbKuukaiBtn.Checked = False _
            AndAlso Me.rbFillingPointBtn.Checked = False _
            AndAlso Me.rbTankDispatchBtn.Checked = False _
            AndAlso Me.rbTankDispatch30Btn.Checked = False _
            AndAlso Me.rbTankDispatch40Btn.Checked = False _
            AndAlso Me.rbTankDispatch54Btn.Checked = False _
            AndAlso Me.rbActualShipBtn.Checked = False _
            AndAlso Me.rbConcatOederBtn.Checked = False _
            AndAlso Me.rbShipContactBtn.Checked = False Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "帳票が未選択です。", needsPopUp:=True)
            Exit Sub
        End If

        '◯ 帳票(入線予定)選択時チェック
        If Me.rbLineBtn.Checked = True AndAlso Me.txtReportRTrainNo.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "列車番号(臨海)が未設定です。", needsPopUp:=True)
            Exit Sub
        End If

        '○ 帳票(タンク車発送実績)選択時チェック
        If (Me.rbTankDispatchBtn.Checked = True _
            Or Me.rbTankDispatch30Btn.Checked = True _
            Or Me.rbTankDispatch40Btn.Checked = True _
            Or Me.rbTankDispatch54Btn.Checked = True) _
            AndAlso Me.txtReportTrainNo.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "列車番号が未設定です。", needsPopUp:=True)
            Exit Sub
        End If

        '○ 帳票(発送実績)選択時チェック
        If Me.rbActualShipBtn.Checked = True AndAlso Me.txtReportTrainNo.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "列車番号が未設定です。", needsPopUp:=True)
            Exit Sub
        End If

        '○ 帳票(連結順序表)選択時チェック
        If Me.rbConcatOederBtn.Checked = True AndAlso Me.txtReportTrainNo.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "列車番号が未設定です。", needsPopUp:=True)
            Exit Sub
        End If

        '○ 帳票(タンク車出荷連絡書)選択時チェック
        If Me.rbShipContactBtn.Checked = True AndAlso Me.txtReportTrainNo.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "列車番号が未設定です。", needsPopUp:=True)
            Exit Sub
        End If

        '★ 各営業所の固定帳票
        Select Case work.WF_SEL_TH_ORDERSALESOFFICECODE.Text
            '◯ 仙台新港営業所
            Case BaseDllConst.CONST_OFFICECODE_010402
                If Me.rbLoadBtn.Checked = True Then             '■積込指示を選択
                    '★ 固定帳票(積込予定(共通))作成処理
                    WW_TyohyoLoadCommonCreate(CONST_RPT_LOADPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbOTLoadBtn.Checked = True Then       '■OT積込指示を選択
                    '★ 固定帳票(OT積込予定(共通))作成処理
                    WW_TyohyoLoadCommonCreate(CONST_RPT_OTLOADPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbTankDispatchBtn.Checked = True Then '■タンク車発送実績を選択
                    '★ 固定帳票(タンク車発送実績(仙台新港))作成処理
                    WW_TyohyoSendaiCreate(CONST_RPT_TANKDISPATCH, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                End If

            '◯ 五井営業所
            Case BaseDllConst.CONST_OFFICECODE_011201
                If Me.rbDeliveryCSVBtn.Checked = True Then      '■託送指示(CSV)を選択
                    '◆ CSV(託送指示)作成処理
                    WW_CsvDeliveryCreate(work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbLoadBtn.Checked = True Then         '■積込指示を選択
                    '★ 固定帳票(積込予定(共通))作成処理
                    WW_TyohyoGoiCreate(CONST_RPT_LOADPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbOTLoadBtn.Checked = True Then       '■OT積込指示を選択
                    '### ここにOT積込処理を追加 #########

                ElseIf Me.rbShipBtn.Checked = True Then         '■出荷予定を選択
                    '☆ 固定帳票(出荷予定(五井))作成処理
                    WW_TyohyoGoiCreate(CONST_RPT_SHIPPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbFillingPointBtn.Checked = True Then '■充填ポイント表を選択
                    '☆ 固定帳票(充填ポイント表(五井))作成処理
                    WW_TyohyoGoiCreate(CONST_RPT_FILLINGPOINT, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                End If

            '◯ 甲子営業所
            Case BaseDllConst.CONST_OFFICECODE_011202
                If Me.rbDeliveryCSVBtn.Checked = True Then      '■託送指示(CSV)を選択
                    '◆ CSV(託送指示)作成処理
                    WW_CsvDeliveryCreate(work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbLoadBtn.Checked = True Then         '■積込指示を選択
                    '★ 固定帳票(積込予定(共通))作成処理
                    WW_TyohyoKinoeneCreate(CONST_RPT_LOADPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbOTLoadBtn.Checked = True Then       '■OT積込指示を選択
                    '### ここにOT積込処理を追加 #########

                ElseIf Me.rbShipBtn.Checked = True Then         '■出荷予定を選択
                    '☆ 固定帳票(出荷予定(甲子))作成処理
                    WW_TyohyoKinoeneCreate(CONST_RPT_SHIPPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbKinoeneLoadBtn.Checked = True Then  '■積込予定(甲子)を選択
                    '☆ 固定帳票(積込予定(甲子))作成処理
                    WW_TyohyoKinoeneCreate(CONST_RPT_LOADPLAN_KINOENE, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                End If

            '◯ 袖ヶ浦営業所
            Case BaseDllConst.CONST_OFFICECODE_011203
                If Me.rbDeliveryCSVBtn.Checked = True Then      '■託送指示(CSV)を選択
                    '◆ CSV(託送指示)作成処理
                    WW_CsvDeliveryCreate(work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbLoadBtn.Checked = True Then         '■積込指示を選択
                    '★ 固定帳票(積込予定(共通))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_LOADPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbOTLoadBtn.Checked = True Then       '■OT積込指示を選択
                    '### ここにOT積込処理を追加 #########

                ElseIf Me.rbShipBtn.Checked = True Then         '■出荷予定を選択
                    '☆ 固定帳票(出荷予定(袖ケ浦))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_SHIPPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbLineBtn.Checked = True Then         '■入線方を選択
                    '☆ 固定帳票(入線予定(袖ヶ浦))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_LINEPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbKuukaiBtn.Checked = True Then       '■空回日報を選択
                    '### 20201106 START 指摘票No195(空回日報対応) #################################################
                    '☆ 固定帳票(空回日報(袖ヶ浦))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_KUUKAI_SODEGAURA, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)
                    '### 20201106 END   指摘票No195(空回日報対応) #################################################

                ElseIf Me.rbTankDispatchBtn.Checked = True Then '■タンク車発送実績を選択
                    '★ 固定帳票(タンク車発送実績(袖ヶ浦))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_TANKDISPATCH, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbTankDispatch30Btn.Checked = True Then '■タンク車発送実績(コウショウ高崎)を選択
                    '★ 固定帳票(タンク車発送実績(袖ヶ浦))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_TANKDISPATCH_30, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbTankDispatch40Btn.Checked = True Then '■タンク車発送実績(ＪＯＮＥＴ松本)を選択
                    '★ 固定帳票(タンク車発送実績(袖ヶ浦))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_TANKDISPATCH_40, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbTankDispatch54Btn.Checked = True Then '■タンク車発送実績(構内取り)を選択
                    '★ 固定帳票(タンク車発送実績(袖ヶ浦))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_TANKDISPATCH_54, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbActualShipBtn.Checked = True Then   '■出荷実績を選択
                    '★ 固定帳票(出荷実績(袖ヶ浦))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_ACTUALSHIP, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbConcatOederBtn.Checked = True Then   '■連結順序表を選択
                    '★ 固定帳票(連結順序表(袖ヶ浦))作成処理
                    WW_TyohyoSodegauraCreate(CONST_RPT_CONCATORDER, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                End If

            '◯ 根岸営業所
            Case BaseDllConst.CONST_OFFICECODE_011402
                If Me.rbLoadBtn.Checked = True Then             '■積込指示を選択
                    '★ 固定帳票(積込予定(共通))作成処理
                    WW_TyohyoNegishiCreate(CONST_RPT_LOADPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbOTLoadBtn.Checked = True Then       '■OT積込指示を選択
                    '### ここにOT積込処理を追加 #########

                ElseIf Me.rbShipBtn.Checked = True Then         '■出荷予定を選択
                    '☆ 固定帳票(出荷予定(根岸))作成処理
                    WW_TyohyoNegishiCreate(CONST_RPT_SHIPPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbNegishiLoadBtn.Checked = True Then  '■積込予定(根岸)を選択
                    '☆ 固定帳票(積込予定(根岸))作成処理
                    WW_TyohyoNegishiCreate(CONST_RPT_LOADPLAN_NEGISHI, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbActualLoad10Btn.Checked = True Then  '■積込実績(北信)を選択
                    '☆ CSV(積込実績(北信))作成処理
                    WW_TyohyoNegishiCreate(CONST_CSV_ACTUALLOAD_10, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbActualLoad20Btn.Checked = True Then  '■積込実績(甲府)を選択
                    '☆ CSV(積込実績(甲府))作成処理
                    WW_TyohyoNegishiCreate(CONST_CSV_ACTUALLOAD_20, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                End If

            '◯ 四日市営業所
            Case BaseDllConst.CONST_OFFICECODE_012401
                If Me.rbDeliveryBtn.Checked = True Then         '■託送指示を選択
                    '☆ 固定帳票(託送指示)作成処理
                    WW_TyohyoYokkaichiCreate(CONST_RPT_DELIVERYPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbShipBtn.Checked = True Then         '■出荷予定を選択
                    '★ 固定帳票(出荷予定(四日市))作成処理
                    WW_TyohyoYokkaichiCreate(CONST_RPT_SHIPPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbLoadBtn.Checked = True Then         '■積込指示を選択
                    '★ 固定帳票(積込予定(共通))作成処理
                    WW_TyohyoYokkaichiCreate(CONST_RPT_LOADPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbOTLoadBtn.Checked = True Then       '■OT積込指示を選択
                    '### ここにOT積込処理を追加 #########

                End If

            '◯ 三重塩浜営業所
            Case BaseDllConst.CONST_OFFICECODE_012402
                If Me.rbDeliveryBtn.Checked = True Then         '■託送指示を選択
                    '☆ 固定帳票(託送指示)作成処理
                    WW_TyohyoMieShiohamaCreate(CONST_RPT_DELIVERYPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbShipBtn.Checked = True Then         '■出荷予定を選択
                    '★ 固定帳票(出荷予定(三重塩浜))作成処理
                    WW_TyohyoMieShiohamaCreate(CONST_RPT_SHIPPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbLoadBtn.Checked = True Then         '■積込指示を選択
                    '★ 固定帳票(積込予定(共通))作成処理
                    WW_TyohyoMieShiohamaCreate(CONST_RPT_LOADPLAN, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbOTLoadBtn.Checked = True Then       '■OT積込指示を選択
                    '### ここにOT積込処理を追加 #########

                ElseIf Me.rbActualShipBtn.Checked = True Then   '■出荷実績を選択
                    '★ 固定帳票(出荷実績(三重塩浜))作成処理
                    WW_TyohyoMieShiohamaCreate(CONST_RPT_ACTUALSHIP, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbConcatOederBtn.Checked = True Then  '■連結順序表を選択
                    '★ 固定帳票(連結順序表(三重塩浜))作成処理
                    WW_TyohyoMieShiohamaCreate(CONST_RPT_CONCATORDER, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                ElseIf Me.rbShipContactBtn.Checked = True Then  '■タンク車出荷連絡書を選択
                    '★ 固定帳票(タンク車出荷連絡書)作成処理
                    WW_TyohyoMieShiohamaCreate(CONST_RPT_SHIPCONTACT, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                End If
        End Select

    End Sub

#Region "CSV"
    ''' <summary>
    ''' CSV(託送指示)作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_CsvDeliveryCreate(ByVal officeCode As String)

        '******************************
        'CSVデータ取得処理
        '******************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            CsvDeliveryDataGet(SQLcon, officeCode, lodDate:=Me.txtReportLodDate.Text)
        End Using

        '******************************
        'CSV作成処理の実行
        '******************************
        Using repCbj = New CsvCreate(OIT0003CsvDeliverytbl)
            Dim url As String
            Try
                url = repCbj.ConvertDataTableToCsv(False, blnFrame:=True, blnSeparate:=True)
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        End Using

    End Sub

    ''' <summary>
    ''' CSV(託送指示)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub CsvDeliveryDataGet(ByVal SQLcon As SqlConnection,
                                     ByVal OFFICECDE As String,
                                     Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003CsvDeliverytbl) Then
            OIT0003CsvDeliverytbl = New DataTable
        End If

        If OIT0003CsvDeliverytbl.Columns.Count <> 0 Then
            OIT0003CsvDeliverytbl.Columns.Clear()
        End If

        OIT0003CsvDeliverytbl.Clear()

        ''○ 取得SQL
        ''　 説明　：　帳票表示用SQL
        'Dim SQLStr As String =
        '      " SELECT " _
        '    & "   0                                              AS LINECNT" _
        '    & " , ''                                             AS OPERATION" _
        '    & " , '0'                                            AS TIMSTP" _
        '    & " , 1                                              AS 'SELECT'" _
        '    & " , 0                                              AS HIDDEN" _
        '    & " , OIT0002.OFFICECODE                             AS OFFICECODE" _
        '    & " , OIT0002.OFFICENAME                             AS OFFICENAME" _
        '    & " , OIT0002.BASECODE                               AS BASECODE" _
        '    & " , OIT0002.BASENAME                               AS BASENAME" _
        '    & " , OIT0003.SHIPPERSCODE                           AS SHIPPERSCODE" _
        '    & " , OIT0003.SHIPPERSNAME                           AS SHIPPERSNAME" _
        '    & " , OIT0002.ARRSTATION                             AS ARRSTATION" _
        '    & " , OIT0002.ARRSTATIONNAME                         AS ARRSTATIONNAME" _
        '    & " , OIT0002.CONSIGNEECODE                          AS CONSIGNEECODE" _
        '    & " , OIT0002.CONSIGNEENAME                          AS CONSIGNEENAME" _
        '    & " , ''                                             AS LODPOINT" _
        '    & " , OIT0003.OILCODE                                AS OILCODE" _
        '    & " , OIT0003.OILNAME                                AS OILNAME" _
        '    & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
        '    & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
        '    & " , OIM0005.MODEL                                  AS MODEL" _
        '    & " , OIM0005.TANKNUMBER                             AS TANKNUMBER" _
        '    & " , OIM0005.JRINSPECTIONDATE                       AS JRINSPECTIONDATE" _
        '    & " , OIM0021.RESERVEDQUANTITY                       AS RESERVEAMOUNT" _
        '    & " , CASE " _
        '    & "   WHEN OIT0002.STACKINGFLG = '1' THEN '積置' " _
        '    & "   ELSE '' " _
        '    & "   END                                            AS STACKING" _
        '    & " , OIT0002.TRAINNO                                AS TRAINNO" _
        '    & " , OIT0002.TRAINNAME                              AS TRAINNAME" _
        '    & " , OIT0002.TOTALTANKCH                            AS TOTALTANK" _
        '    & " , OIT0002.LODDATE                                AS LODDATE" _
        '    & " , OIT0002.DEPDATE                                AS DEPDATE" _
        '    & " , OIT0002.ARRDATE                                AS ARRDATE" _
        '    & " , OIT0002.ACCDATE                                AS ACCDATE" _
        '    & " FROM OIL.OIT0002_ORDER OIT0002 " _
        '    & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
        '    & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
        '    & " AND OIT0003.DELFLG <> @P02 " _
        '    & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
        '    & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
        '    & " AND OIM0005.DELFLG <> @P02 " _
        '    & " LEFT JOIN OIL.OIM0021_LOADRESERVE OIM0021 ON " _
        '    & "     OIM0021.OFFICECODE = OIT0002.OFFICECODE " _
        '    & " AND OIM0021.MODEL = OIM0005.MODEL " _
        '    & " AND OIM0021.LOAD = OIM0005.LOAD " _
        '    & " AND OIM0021.OILCODE = OIT0003.OILCODE " _
        '    & " AND OIM0021.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
        '    & " AND OIM0021.FROMYMD <= FORMAT(GETDATE(),'yyyy/MM/dd') " _
        '    & " AND OIM0021.TOYMD >= FORMAT(GETDATE(),'yyyy/MM/dd') " _
        '    & " AND OIM0021.DELFLG <> @P02 " _
        '    & " WHERE OIT0002.OFFICECODE = @P01 " _
        '    & "   AND OIT0002.DELFLG <> @P02 " _
        '    & "   AND OIT0002.LODDATE = @P03 "

        'SQLStr &=
        '      " ORDER BY" _
        '    & "    OIT0002.BASECODE" _
        '    & "  , OIT0003.OILCODE"

        Try
            'Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
            '    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
            '    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
            '    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
            '    PARA01.Value = OFFICECDE
            '    PARA02.Value = C_DELETE_FLG.DELETE
            '    If Not String.IsNullOrEmpty(lodDate) Then
            '        PARA03.Value = lodDate
            '    Else
            '        PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
            '    End If

            '    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
            '        '○ フィールド名とフィールドの型を取得
            '        For index As Integer = 0 To SQLdr.FieldCount - 1
            '            OIT0003CsvDeliverytbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
            '        Next

            '        '○ テーブル検索結果をテーブル格納
            '        OIT0003CsvDeliverytbl.Load(SQLdr)
            '    End Using

            '    Dim i As Integer = 0
            '    For Each OIT0003Csvrow As DataRow In OIT0003CsvDeliverytbl.Rows
            '        i += 1
            '        OIT0003Csvrow("LINECNT") = i        'LINECNT

            '    Next

            'End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L CSV_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L CSV_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003CsvDeliverytbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub
#End Region

#Region "固定帳票"
    ''' <summary>
    ''' 固定帳票(積込指示書(共通))作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_TyohyoLoadCommonCreate(ByVal tyohyoType As String,
                                             ByVal officeCode As String)

        '******************************
        '帳票表示データ取得処理
        '******************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            ExcelLoadCommonDataGet(SQLcon, tyohyoType, officeCode, lodDate:=Me.txtReportLodDate.Text)
        End Using

        '******************************
        '帳票作成処理の実行
        '******************************
        Select Case tyohyoType
            '固定帳票(積込予定(共通))作成処理
            Case CONST_RPT_LOADPLAN
                Dim fileName As String = "_LOADPLAN.xlsx"
                If officeCode = BaseDllConst.CONST_OFFICECODE_012401 Then fileName = "_YOKKAICHI_LOADPLAN.xlsx"
                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & fileName, OIT0003Reporttbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintData(tyohyoType, officeCode, lodDate:=Me.txtReportLodDate.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            '固定帳票(OT積込予定(共通))作成処理
            Case CONST_RPT_OTLOADPLAN
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_OTLOADPLAN.xlsx", OIT0003Reporttbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintData(tyohyoType, officeCode, lodDate:=Me.txtReportLodDate.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################

        End Select
    End Sub

    ''' <summary>
    ''' 固定帳票(仙台新港)作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_TyohyoSendaiCreate(ByVal tyohyoType As String,
                                        ByVal officeCode As String)

        '******************************
        '帳票作成処理の実行
        '******************************
        Select Case tyohyoType
            'ダウンロードボタン(タンク車発送実績(仙台新港))押下
            Case CONST_RPT_TANKDISPATCH
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelTankDispatchDataGet(SQLcon, officeCode, Me.txtReportLodDate.Text, {Me.txtReportTrainNo.Text}, BaseDllConst.CONST_CONSIGNEECODE_51, Nothing)
                End Using

                '帳票作成
                Dim url As String =
                OIT0003CustomMultiReport.CreateTankDispatch(Master.MAPID, officeCode, OIT0003Reporttbl, txtReportLodDate.Text, BaseDllConst.CONST_CONSIGNEECODE_51, txtReportTrainNo.Text)

                '○ 別画面でExcelを表示
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        End Select

    End Sub

    ''' <summary>
    ''' 固定帳票(五井営業所)作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_TyohyoGoiCreate(ByVal tyohyoType As String,
                                             ByVal officeCode As String)

        '******************************
        '帳票作成処理の実行
        '******************************
        Select Case tyohyoType
            'ダウンロードボタン(積込予定)押下
            Case CONST_RPT_LOADPLAN
                '★ 固定帳票(積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

            'ダウンロードボタン(OT積込指示)押下
            Case CONST_RPT_OTLOADPLAN
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                '★ 固定帳票(OT積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################

            'ダウンロードボタン(出荷予定(五井))押下
            Case CONST_RPT_SHIPPLAN
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelGoiDataGet(SQLcon, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_GOI_SHIPPLAN.xlsx", OIT0003ReportGoitbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintGoiData("SHIPPLAN", Me.txtReportLodDate.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(充填ポイント表(五井))押下
            Case CONST_RPT_FILLINGPOINT
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelGoiFillingPointDataGet(SQLcon, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_GOI_FILLINGPOINT.xlsx", OIT0003ReportGoitbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintGoiData(CONST_RPT_FILLINGPOINT, Me.txtReportLodDate.Text)
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
    ''' 固定帳票(甲子営業所)作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_TyohyoKinoeneCreate(ByVal tyohyoType As String,
                                             ByVal officeCode As String)

        '******************************
        '帳票作成処理の実行
        '******************************
        Select Case tyohyoType
            'ダウンロードボタン(積込指示)押下
            Case CONST_RPT_LOADPLAN
                '★ 固定帳票(積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

            'ダウンロードボタン(OT積込指示)押下
            Case CONST_RPT_OTLOADPLAN
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                '★ 固定帳票(OT積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################

            'ダウンロードボタン(出荷予定(甲子))押下
            Case CONST_RPT_SHIPPLAN
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelKinoeneShipPlanDataGet(SQLcon, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_KINOENE_SHIPPLAN.xlsx", OIT0003ReportKinoenetbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintKinoeneData(CONST_RPT_SHIPPLAN, Me.txtReportLodDate.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

                'ダウンロードボタン(積込予定(甲子))押下
            Case CONST_RPT_LOADPLAN_KINOENE
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelKinoeneDataGet(SQLcon, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_KINOENE_LOADPLAN.xlsx", OIT0003ReportKinoenetbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintKinoeneData("KINOENE_LOADPLAN", Me.txtReportLodDate.Text)
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
    ''' 固定帳票(袖ヶ浦営業所)作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_TyohyoSodegauraCreate(ByVal tyohyoType As String,
                                           ByVal officeCode As String)

        '******************************
        '帳票作成処理の実行
        '******************************
        Select Case tyohyoType
            'ダウンロードボタン(積込指示)押下
            Case CONST_RPT_LOADPLAN
                '★ 固定帳票(積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

            'ダウンロードボタン(OT積込指示)押下
            Case CONST_RPT_OTLOADPLAN
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                '★ 固定帳票(OT積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################

            'ダウンロードボタン(出荷予定(袖ヶ浦))押下
            Case CONST_RPT_SHIPPLAN
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelSodegauraShipPlanDataGet(SQLcon, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_SODEGAURA_SHIPPLAN.xlsx", OIT0003ReportSodegauratbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintSodegauraData(CONST_RPT_SHIPPLAN, Me.txtReportLodDate.Text, Me.txtReportRTrainNo.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(入線方(袖ヶ浦))押下
            Case CONST_RPT_LINEPLAN
                '******************************
                '帳票表示データ取得処理
                '******************************
                Dim sArrStation As String = ""
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelSodegauraLinePlanDataGet(SQLcon, sArrStation, lodDate:=Me.txtReportLodDate.Text, rTrainNo:=Me.txtReportRTrainNo.Text)
                End Using
                '使用する帳票の確認
                Dim tyohyoName As String = ""
                Dim repRTrainNo As String = Me.txtReportRTrainNo.Text
                '★同時入線(チェックボックス)が押下されている場合
                If Me.ChkSameTimeLineChk.Checked = True Then
                    '◯ファイル名(袖ヶ浦同時入線専用入線方)
                    tyohyoName = "_SODEGAURA_LINEPLAN"

                    Dim dtOrderNo As String = ""
                    For Each OIT0001row As DataRow In OIT0003ReportSodegauratbl.Select(Nothing, "ORDERNO")
                        '★受注TBL(同時入線フラグ)を"1"(同時入線)に更新
                        If dtOrderNo <> Convert.ToString(OIT0001row("ORDERNO")) Then
                            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                                SQLcon.Open()       'DataBase接続
                                CMNPTS.UpdateOrderCRT(SQLcon, Convert.ToString(OIT0001row("ORDERNO")), Master, "1", I_PARA:="SAMETIMELINEFLG")
                            End Using
                        End If
                        dtOrderNo = Convert.ToString(OIT0001row("ORDERNO"))
                    Next

                Else
                    If Me.txtReportRTrainNo.Text = BaseDllConst.CONST_RTRAIN_I02_401_011203 Then
                        '◯ファイル名(袖ヶ浦401レ専用入線方)
                        tyohyoName = "_SODEGAURA_LINEPLAN_401"
                        '★着駅が"5141"(南松本)の場合は、フォーマット(南松本向け)にする。
                        If sArrStation = "5141" Then
                            tyohyoName = "_SODEGAURA_LINEPLAN_501"
                            repRTrainNo = BaseDllConst.CONST_RTRAIN_I01_501_011203
                        End If
                    Else
                        '◯ファイル名(袖ヶ浦501レ専用入線方)
                        tyohyoName = "_SODEGAURA_LINEPLAN_501"
                        '★着駅が"4113"(倉賀野)の場合は、フォーマット(倉賀野向け)にする。
                        If sArrStation = "4113" Then
                            tyohyoName = "_SODEGAURA_LINEPLAN_401"
                            repRTrainNo = BaseDllConst.CONST_RTRAIN_I02_401_011203
                        End If
                    End If

                    Dim dtOrderNo As String = ""
                    For Each OIT0001row As DataRow In OIT0003ReportSodegauratbl.Select(Nothing, "ORDERNO")
                        '★受注TBL(同時入線フラグ)を"0"(通常)に更新
                        If dtOrderNo <> Convert.ToString(OIT0001row("ORDERNO")) Then
                            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                                SQLcon.Open()       'DataBase接続
                                CMNPTS.UpdateOrderCRT(SQLcon, Convert.ToString(OIT0001row("ORDERNO")), Master, "0", I_PARA:="SAMETIMELINEFLG")
                            End Using
                        End If
                        dtOrderNo = Convert.ToString(OIT0001row("ORDERNO"))
                    Next
                End If
                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & tyohyoName & ".xlsx", OIT0003ReportSodegauratbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintSodegauraData(CONST_RPT_LINEPLAN, Me.txtReportLodDate.Text, repRTrainNo, Me.ChkSameTimeLineChk.Checked)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(空回日報(袖ヶ浦))押下
            Case CONST_RPT_KUUKAI_SODEGAURA
                '### 20201106 START 指摘票No195(空回日報対応) #################################################
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelSodegauraKuukaiDataGet(SQLcon, lodDate:=Me.txtReportLodDate.Text, rTrainNo:=Me.txtReportRTrainNo.Text)
                End Using

                'Using repCbj = New OIT0001CustomReport("OIT0001D", "OIT0001D" & "_SODEGAURA.xlsx", OIT0003ReportSodegauratbl)
                Using repCbj = New OIT0001CustomReport(Master.MAPID, Master.MAPID & "_SODEGAURA_KUUKAI.xlsx", OIT0003ReportSodegauratbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintData(officeCode, repPtn:=CONST_RPT_KUUKAI_SODEGAURA, lodDate:=Me.txtReportLodDate.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using
                '### 20201106 END   指摘票No195(空回日報対応) #################################################

            'ダウンロードボタン(タンク車発送実績(袖ヶ浦))押下
            Case CONST_RPT_TANKDISPATCH,
                 CONST_RPT_TANKDISPATCH_30,    'コウショウ高崎
                 CONST_RPT_TANKDISPATCH_54     'OT高崎（構内取り）

                '列車設定
                Dim trainNoList As New List(Of String)
                If Me.txtReportTrainNo.Text = CONST_SODE_TRAIN_8877 OrElse Me.txtReportTrainNo.Text = CONST_SODE_TRAIN_8883 Then
                    '8877及び8883は集計して出力する
                    trainNoList.AddRange({CONST_SODE_TRAIN_8877, CONST_SODE_TRAIN_8883})
                Else
                    trainNoList.Add(Me.txtReportTrainNo.Text)
                End If

                '油層所毎設定
                Dim consigneeCode As String = Nothing
                Dim secondConsigneeCode As String = Nothing
                Select Case tyohyoType
                    Case CONST_RPT_TANKDISPATCH_30
                        'コウショウ高崎
                        consigneeCode = BaseDllConst.CONST_CONSIGNEECODE_30
                    Case CONST_RPT_TANKDISPATCH_54
                        'OT高崎(構内取り)
                        secondConsigneeCode = BaseDllConst.CONST_CONSIGNEECODE_54
                End Select

                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続
                    ExcelTankDispatchDataGet(SQLcon, officeCode, Me.txtReportLodDate.Text, trainNoList.ToArray(), consigneeCode, secondConsigneeCode)
                End Using

                '出力時の表示油層所を設定
                If Not String.IsNullOrEmpty(secondConsigneeCode) Then
                    consigneeCode = secondConsigneeCode
                End If

                '出力時の列車番号を設定
                Dim outputTrainNo As String = Me.txtReportTrainNo.Text
                If OIT0003Reporttbl IsNot Nothing AndAlso OIT0003Reporttbl.Rows.Count > 0 AndAlso trainNoList.Count = 2 Then
                    Dim query = OIT0003Reporttbl.AsEnumerable()

                    '列車番号毎に入線番号を取得
                    Dim dicIrisen As New Dictionary(Of String, String())
                    For Each trainNo As String In trainNoList
                        dicIrisen.Add(trainNo, query.Where(Function(r) r("TRAINNO").ToString() = trainNo).Select(Function(r) r("LOADINGIRILINETRAINNO").ToString()).Distinct().ToArray())
                    Next

                    '表示優先列車番号を取得
                    Dim viewIrisen As New List(Of String)
                    Dim anyTrain As Boolean = False
                    For i As Integer = 0 To dicIrisen.Count - 1 Step 1
                        If anyTrain Then Exit For
                        For j As Integer = 0 To dicIrisen.Count - 1 Step 1
                            If i = j Then Continue For
                            '入線番号の積集合をとる
                            Dim intersectIrisen = dicIrisen.ElementAtOrDefault(i).Value.Intersect(dicIrisen.ElementAtOrDefault(j).Value)
                            If intersectIrisen.Any() Then
                                If Not anyTrain Then
                                    outputTrainNo = dicIrisen.ElementAtOrDefault(i).Key
                                    anyTrain = True
                                End If
                                viewIrisen.AddRange(intersectIrisen)
                            End If
                        Next
                    Next
                    viewIrisen = viewIrisen.Distinct().ToList()
                    If Not anyTrain Then
                        outputTrainNo = dicIrisen.Where(Function(d) d.Value.Any()).FirstOrDefault().Key
                        viewIrisen = dicIrisen.Item(outputTrainNo).ToList()
                    End If

                    '表示優先列車番号以外かつ同一の入線番号を持たないデータを出力対象外とする
                    query.Where(Function(r)
                                    Return outputTrainNo <> r("TRAINNO").ToString() AndAlso Not viewIrisen.Contains(r("LOADINGIRILINETRAINNO").ToString())
                                End Function).
                                ToList().ForEach(Sub(r) r.Delete())

                End If

                '帳票作成
                Dim url As String =
                    OIT0003CustomMultiReport.CreateTankDispatch(Master.MAPID, officeCode, OIT0003Reporttbl, txtReportLodDate.Text, consigneeCode, outputTrainNo)

                '○ 別画面でExcelを表示
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            'ダウンロードボタン(出荷実績(袖ヶ浦)、タンク車発送実績(袖ヶ浦))押下
            Case CONST_RPT_ACTUALSHIP,
                 CONST_RPT_TANKDISPATCH_40    'ＪＯＮＥＴ松本
                '******************************
                '帳票表示データ取得処理
                '******************************
                Dim cvtTrainNo As String = ""
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelActualShipDataGet(SQLcon, officeCode, Me.txtReportLodDate.Text, Me.txtReportTrainNo.Text, cvtTrainNo)
                End Using

                '帳票作成
                Dim url As String =
                OIT0003CustomMultiReport.CreateActualShip(Master.MAPID, officeCode, OIT0003Reporttbl, Me.txtReportLodDate.Text, cvtTrainNo)
                'OIT0003CustomMultiReport.CreateActualShip(Master.MAPID, officeCode, OIT0003Reporttbl, Me.txtReportLodDate.Text, Me.txtReportTrainNo.Text)

                '○ 別画面でExcelを表示
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            'ダウンロードボタン(連結順序表(袖ヶ浦))押下
            Case CONST_RPT_CONCATORDER
                '******************************
                '帳票表示データ取得処理
                '******************************
                Dim cvtTrainNo As String = ""
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelConcatOrderDataGet(SQLcon, officeCode, Me.txtReportLodDate.Text, Me.txtReportTrainNo.Text, cvtTrainNo)
                End Using

                '帳票作成
                Dim url As String =
                OIT0003CustomMultiReport.CreateContactOrder(Master.MAPID, officeCode, OIT0003Reporttbl, Me.txtReportLodDate.Text, cvtTrainNo)
                'OIT0003CustomMultiReport.CreateContactOrder(Master.MAPID, officeCode, OIT0003Reporttbl, Me.txtReportLodDate.Text, Me.txtReportTrainNo.Text)

                '○ 別画面でExcelを表示
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        End Select

    End Sub


    ''' <summary>
    ''' 固定帳票(根岸営業所)作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_TyohyoNegishiCreate(ByVal tyohyoType As String,
                                             ByVal officeCode As String)

        '【出荷予定(根岸)】 または【積込予定(根岸)】の場合
        If tyohyoType = CONST_RPT_SHIPPLAN OrElse tyohyoType = CONST_RPT_LOADPLAN_NEGISHI Then
            '******************************
            '帳票表示データ取得処理
            '******************************
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                ExcelNegishiDataGet(SQLcon, tyohyoType, lodDate:=Me.txtReportLodDate.Text)
            End Using
        End If

        '******************************
        '帳票作成処理の実行
        '******************************
        Select Case tyohyoType
            'ダウンロードボタン(積込予定)押下
            Case CONST_RPT_LOADPLAN
                '★ 固定帳票(積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

            'ダウンロードボタン(OT積込指示)押下
            Case CONST_RPT_OTLOADPLAN
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                '★ 固定帳票(OT積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################

            'ダウンロードボタン(出荷予定(根岸))押下
            Case CONST_RPT_SHIPPLAN
                '### 20201111 START 指摘票No193(全体)対応 #####################################################
                '******************************
                '帳票表示(計画枠)データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelPlanFrameDataGet(SQLcon)
                End Using
                '### 20201111 END   指摘票No193(全体)対応 #####################################################

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_NEGISHI_SHIPPLAN.xlsx", OIT0003ReportNegishitbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintNegishiData("SHIPPLAN", Me.txtReportLodDate.Text,
                                                                 dtFT:=OIT0003ItemGettbl,
                                                                 dtPF:=OIT0003ReportPlanFrame)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(積込予定(根岸))押下
            Case CONST_RPT_LOADPLAN_NEGISHI
                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_NEGISHI_LOADPLAN.xlsx", OIT0003ReportNegishitbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintNegishiData("LOADPLAN", Me.txtReportLodDate.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(積込実績(北信、甲府))押下
            Case CONST_CSV_ACTUALLOAD_10,
                 CONST_CSV_ACTUALLOAD_20
                '******************************
                '積込実績データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    CsvActualLoadDataGet(SQLcon, tyohyoType, officeCode, Me.txtReportLodDate.Text)
                End Using
                '******************************
                'CSV作成処理の実行
                '******************************
                Dim OTFileName As String = "油槽所B.CSV"
                Using repCbj = New CsvCreate(OIT0003CsvActualLoadtbl, I_FileName:=OTFileName, I_Enc:="Shift_JIS")
                    Dim url As String
                    Try
                        url = repCbj.ConvertDataTableToCsv(writeHeader:=False, strOfficeCode:="", blnFrame:=False, blnSeparate:=True, blnNewline:=True)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ CSVをダウンロード
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

        End Select

    End Sub

    ''' <summary>
    ''' 固定帳票(四日市営業所)作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_TyohyoYokkaichiCreate(ByVal tyohyoType As String,
                                             ByVal officeCode As String)

        '******************************
        '帳票作成処理の実行
        '******************************
        Select Case tyohyoType
            'ダウンロードボタン(託送指示)押下
            Case CONST_RPT_DELIVERYPLAN
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelDeliveryDataGet(SQLcon, BaseDllConst.CONST_OFFICECODE_012401, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_DELIVERYPLAN.xlsx", OIT0003ReportDeliverytbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintYokkaichiData("DELIVERYPLAN", Me.txtReportLodDate.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(出荷予定(四日市))押下
            Case CONST_RPT_SHIPPLAN
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelCentralOfficeShipPlanDataGet(SQLcon, BaseDllConst.CONST_OFFICECODE_012401, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_YOKKAICHI_SHIPPLAN.xlsx", OIT0003ReportMieShiohamatbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintYokkaichiData(CONST_RPT_SHIPPLAN, Me.txtReportLodDate.Text, dt:=OIT0003ReportOilDuration)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(積込指示)押下
            Case CONST_RPT_LOADPLAN
                '★ 固定帳票(積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

            'ダウンロードボタン(OT積込指示)押下
            Case CONST_RPT_OTLOADPLAN
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                '★ 固定帳票(OT積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################

        End Select
    End Sub

    ''' <summary>
    ''' 固定帳票(三重塩浜営業所)作成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_TyohyoMieShiohamaCreate(ByVal tyohyoType As String,
                                             ByVal officeCode As String)

        '******************************
        '帳票作成処理の実行
        '******************************
        Select Case tyohyoType
            'ダウンロードボタン(託送指示)押下
            Case CONST_RPT_DELIVERYPLAN
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelDeliveryDataGet(SQLcon, BaseDllConst.CONST_OFFICECODE_012402, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_DELIVERYPLAN.xlsx", OIT0003ReportDeliverytbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintMieShiohamaData("DELIVERYPLAN", Me.txtReportLodDate.Text)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(出荷予定(三重塩浜))押下
            Case CONST_RPT_SHIPPLAN
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelCentralOfficeShipPlanDataGet(SQLcon, BaseDllConst.CONST_OFFICECODE_012402, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_MIESHIOHAMA_SHIPPLAN.xlsx", OIT0003ReportMieShiohamatbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintMieShiohamaData(CONST_RPT_SHIPPLAN, Me.txtReportLodDate.Text, dt:=OIT0003ReportOilDuration)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(積込指示)押下
            Case CONST_RPT_LOADPLAN
                '### 20210224 START 共通帳票から個別帳票へ対応 ###############################################
                ''★ 固定帳票(積込予定(共通))作成処理  止める！！
                ''WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)

                '******************************
                '帳票表示(予約数量枠)データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelReserveAmountDataGet(SQLcon, Me.txtReportLodDate.Text)
                End Using

                '★ 固定帳票(積込予定（三重塩浜))作成処理
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelLoadPlanDataGet(SQLcon, BaseDllConst.CONST_OFFICECODE_012402, lodDate:=Me.txtReportLodDate.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_MIESHIOHAMA_LOADPLAN.xlsx", OIT0003Reporttbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintMieShiohamaData(CONST_RPT_LOADPLAN, Me.txtReportLodDate.Text, dt:=OIT0003ReportReserveAmount)
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using
                '### 20210224 END 共通帳票から個別帳票へ対応 ###############################################

            'ダウンロードボタン(OT積込指示)押下
            Case CONST_RPT_OTLOADPLAN
                '### 20201014 START 指摘票No168(OT積込指示対応) ###############################################
                '★ 固定帳票(OT積込予定(共通))作成処理
                WW_TyohyoLoadCommonCreate(tyohyoType, work.WF_SEL_TH_ORDERSALESOFFICECODE.Text)
                '### 20201014 END   指摘票No168(OT積込指示対応) ###############################################

            'ダウンロードボタン(出荷実績(三重塩浜))押下
            Case CONST_RPT_ACTUALSHIP
                '******************************
                '帳票表示データ取得処理
                '******************************
                Dim cvtTrainNo As String = ""
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelActualShipDataGet(SQLcon, officeCode, Me.txtReportLodDate.Text, Me.txtReportTrainNo.Text, cvtTrainNo)
                End Using

                '帳票作成
                Dim url As String =
                OIT0003CustomMultiReport.CreateActualShip(Master.MAPID, officeCode, OIT0003Reporttbl, Me.txtReportLodDate.Text, cvtTrainNo)
                'OIT0003CustomMultiReport.CreateActualShip(Master.MAPID, officeCode, OIT0003Reporttbl, Me.txtReportLodDate.Text, Me.txtReportTrainNo.Text)

                '○ 別画面でExcelを表示
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            'ダウンロードボタン(連結順序表(三重塩浜))押下
            Case CONST_RPT_CONCATORDER
                '******************************
                '帳票表示データ取得処理
                '******************************
                Dim cvtTrainNo As String = ""
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelConcatOrderDataGet(SQLcon, officeCode, Me.txtReportLodDate.Text, Me.txtReportTrainNo.Text, cvtTrainNo)
                End Using

                '帳票作成
                Dim url As String =
                OIT0003CustomMultiReport.CreateContactOrder(Master.MAPID, officeCode, OIT0003Reporttbl, Me.txtReportLodDate.Text, cvtTrainNo)
                'OIT0003CustomMultiReport.CreateContactOrder(Master.MAPID, officeCode, OIT0003Reporttbl, Me.txtReportLodDate.Text, Me.txtReportTrainNo.Text)

                '○ 別画面でExcelを表示
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

            'ダウンロードボタン(タンク車出荷連絡書)押下
            Case CONST_RPT_SHIPCONTACT
                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelShipContactDataGet(SQLcon, officeCode, Me.txtReportLodDate.Text, Me.txtReportTrainNo.Text)
                End Using

                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_MIESHIOHAMA_SHIPCONTACT.xlsx", OIT0003Reporttbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintMieShiohamaData(CONST_RPT_SHIPCONTACT, Me.txtReportLodDate.Text)
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
    ''' 帳票表示(積込指示書(共通))データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelLoadCommonDataGet(ByVal SQLcon As SqlConnection,
                                         ByVal tyohyoType As String,
                                         ByVal OFFICECDE As String,
                                         Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003Reporttbl) Then
            OIT0003Reporttbl = New DataTable
        End If

        If OIT0003Reporttbl.Columns.Count <> 0 Then
            OIT0003Reporttbl.Columns.Clear()
        End If

        OIT0003Reporttbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        '### 20200818 START SQLの入換を実施 ####################################################
        '★共通SQL
        Dim SQLStrCmn As String =
              " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , OIT0002.OFFICECODE                             AS OFFICECODE" _
            & " , OIT0002.OFFICENAME                             AS OFFICENAME" _
            & " , OIT0002.BASECODE                               AS BASECODE" _
            & " , OIT0002.BASENAME                               AS BASENAME" _
            & " , OIT0003.SHIPPERSCODE                           AS SHIPPERSCODE" _
            & " , OIT0003.SHIPPERSNAME                           AS SHIPPERSNAME" _
            & " , OIT0002.ARRSTATION                             AS ARRSTATION" _
            & " , OIT0002.ARRSTATIONNAME                         AS ARRSTATIONNAME" _
            & " , OIT0002.CONSIGNEECODE                          AS CONSIGNEECODE" _
            & " , OIT0002.CONSIGNEENAME                          AS CONSIGNEENAME" _
            & " , ''                                             AS LODPOINT" _
            & " , OIM0003_OIL.MIDDLEOILCODE                      AS MIDDLEOILCODE" _
            & " , OIM0003_OIL.MIDDLEOILNAME                      AS MIDDLEOILNAME" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIT0003.OILNAME                                AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
            & " , OIM0003_LASTOIL.MIDDLEOILCODE                  AS LASTMIDDLEOILCODE" _
            & " , OIM0003_LASTOIL.MIDDLEOILNAME                  AS LASTMIDDLEOILNAME" _
            & " , OIT0005.LASTOILCODE                            AS LASTOILCODE" _
            & " , OIT0005.LASTOILNAME                            AS LASTOILNAME" _
            & " , OIT0005.PREORDERINGTYPE                        AS PREORDERINGTYPE" _
            & " , OIT0005.PREORDERINGOILNAME                     AS PREORDERINGOILNAME" _
            & " , OIT0003.SHIPORDER                              AS SHIPORDER" _
            & " , OIT0003.LINEORDER                              AS LINEORDER" _
            & " , OIM0005.MODEL                                  AS MODEL" _
            & " , OIM0005.LOAD                                   AS LOAD" _
            & " , OIM0005.LOAD_SORT                              AS LOAD_SORT" _
            & " , CONVERT(int, OIM0005.TANKNUMBER)               AS TANKNUMBER" _
            & " , OIM0005.JRINSPECTIONDATE                       AS JRINSPECTIONDATE" _
            & " , OIM0021.RESERVEDQUANTITY                       AS RESERVEAMOUNT" _
            & " , OIT0003.JOINT                                  AS JOINT" _
            & "	, CASE " _
            & "   WHEN OIT0003.STACKINGFLG ='1' " _
            & "	      THEN '積　置' " _
            & "	      ELSE '' " _
            & "   END                                            AS STACKING " _
            & "	, CASE " _
            & "   WHEN OIT0003.INSPECTIONFLG ='1' " _
            & "	      THEN '交　検' " _
            & "	      ELSE '' " _
            & "   END                                            AS INSPECTION " _
            & "	, CASE " _
            & "   WHEN OIT0003.DETENTIONFLG ='1' " _
            & "	      THEN '荷卸後休車' " _
            & "	      ELSE '' " _
            & "   END                                            AS DETENTION " _
            & "	, CASE " _
            & "   WHEN OIM0003_LASTOIL.MIDDLEOILCODE ='2' AND OIM0003_OIL.MIDDLEOILCODE = '1' " _
            & "	      THEN '格上' " _
            & "	      ELSE '' " _
            & "   END                                            AS UPGRADE "

        '### 20201209 START OT積込指示書(翌月発送対応) #########################
        If tyohyoType <> "OTLOADPLAN" Then
            SQLStrCmn &=
                  " , ''                                             AS NEXTMONTH"
        Else
            If ChkEndMonthChk.Checked = False Then
                SQLStrCmn &=
                      " , ''                                             AS NEXTMONTH"
            Else
                SQLStrCmn &=
                      " , CASE" _
                    & "   WHEN CONVERT(VARCHAR (6), OIT0002.DEPDATE, 112) = @P05 THEN FORMAT(OIT0002.DEPDATE, 'MM/dd') + '発送分' " _
                    & "   ELSE ''" _
                    & "   END                                            AS NEXTMONTH"
            End If
        End If
        '### 20201209 END   OT積込指示書(翌月発送対応) #########################

        SQLStrCmn &=
              " , OIT0003.REMARK                                 AS REMARK" _
            & " , OIT0002.TRAINNO                                AS TRAINNO" _
            & " , OIT0002.TRAINNAME                              AS TRAINNAME" _
            & " , SUBSTRING(OIT0002.TRAINNAME,1,CHARINDEX('-',OIT0002.TRAINNAME)-1) AS OTTRAINNO" _
            & " , OIT0003.OTTRANSPORTFLG                         AS OTTRANSPORTFLG" _
            & " , OIT0002.TOTALTANKCH                            AS TOTALTANK"

        '★積置フラグ無し用SQL
        Dim SQLStrNashi As String =
              SQLStrCmn _
            & " , ISNULL(OIT0003.ACTUALLODDATE, OIT0002.LODDATE) AS LODDATE"

        '★積置フラグ有り用SQL
        Dim SQLStrAri As String =
              SQLStrCmn _
            & " , OIT0003.ACTUALLODDATE                          AS LODDATE"

        SQLStrCmn =
              " , OIT0002.DEPDATE                                AS DEPDATE" _
            & " , OIT0002.ARRDATE                                AS ARRDATE" _
            & " , OIT0002.ACCDATE                                AS ACCDATE" _
            & " , OIM0024.PRIORITYNO                             AS PRIORITYNO" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & " AND OIT0003.TANKNO <> '' " _
            & " AND OIT0003.DELFLG <> @P02 "

        '★積置フラグ無し用SQL
        If tyohyoType <> "OTLOADPLAN" Then
            '◯積込指示書の場合
            SQLStrNashi &=
              SQLStrCmn _
            & " AND (OIT0003.STACKINGFLG <> '1' OR OIT0003.STACKINGFLG IS NULL) "
        Else
            '◯OT積込指示書の場合
            SQLStrNashi &= SQLStrCmn
        End If

        '★積置フラグ有り用SQL
        SQLStrAri &=
                  SQLStrCmn _
                & " AND OIT0003.STACKINGFLG = '1' "

        Dim dtShipClose As DataTable = New DataTable
        Dim chkShipCloseFlg As String = "0"
        '○チェックボックス(当月積込、翌月発分を含める)未選択
        If ChkEndMonthChk.Checked = False Then
            SQLStrAri &=
              " AND OIT0003.ACTUALLODDATE = @P03 "
        Else
            '○出荷休業日マスタに設定されているかチェック
            If CMNPTS.ChkShipClose(SQLcon, BaseDllConst.CONST_OFFICECODE_010402, lodDate, "1", dtShipClose) = True Then
                chkShipCloseFlg = "1"
                SQLStrAri &=
                  String.Format(" AND OIT0003.ACTUALLODDATE = '{0}' ", Format(Date.Parse(lodDate).AddDays(-1), "yyyy/MM/dd"))
            ElseIf CMNPTS.ChkShipClose(SQLcon, BaseDllConst.CONST_OFFICECODE_010402, lodDate, "2", dtShipClose) = True Then
                chkShipCloseFlg = "2"
                '    SQLStrAri &=
                '      String.Format(" AND OIT0003.ACTUALLODDATE = '{0}' ", Format(Date.Parse(lodDate).AddDays(1), "yyyy/MM/dd"))
            Else
                SQLStrAri &=
                  " AND OIT0003.ACTUALLODDATE = @P03 "
            End If

        End If

        '### 20210511 START 甲子営業所対応(荷重のソート順) #############################################
        SQLStrCmn =
              " LEFT JOIN (SELECT " _
            & "                 OIM0005.* " _
            & "               , CASE " _
            & "                 WHEN OIM0005.LOAD = 43.0 THEN 1 " _
            & "                 WHEN OIM0005.LOAD = 44.0 THEN 3 " _
            & "                 WHEN OIM0005.LOAD = 45.0 THEN 2 " _
            & "                 END AS LOAD_SORT " _
            & "            FROM OIL.OIM0005_TANK OIM0005) OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & " AND OIM0005.DELFLG <> @P02 "
        'SQLStrCmn =
        '      " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
        '    & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
        '    & " AND OIM0005.DELFLG <> @P02 "
        '### 20210511 END   甲子営業所対応(荷重のソート順) #############################################

        SQLStrCmn &=
              " LEFT JOIN OIL.OIM0021_LOADRESERVE OIM0021 ON " _
            & "     OIM0021.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0021.MODEL = OIM0005.MODEL " _
            & " AND OIM0021.LOAD = OIM0005.LOAD " _
            & " AND OIM0021.OILCODE = OIT0003.OILCODE " _
            & " AND OIM0021.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & " AND OIM0021.FROMYMD <= FORMAT(GETDATE(),'yyyy/MM/dd') " _
            & " AND OIM0021.TOYMD >= FORMAT(GETDATE(),'yyyy/MM/dd') " _
            & " AND OIM0021.DELFLG <> @P02 "

        '### 20201013 START 油種中分類、及び前回油種取得するための条件を追加 ###########################
        SQLStrCmn &=
              " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
            & "     OIT0005.TANKNUMBER = OIT0003.TANKNO " _
            & " AND OIT0005.DELFLG <> @P02 " _
            & " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_OIL ON " _
            & "     OIM0003_OIL.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0003_OIL.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
            & " AND OIM0003_OIL.PLANTCODE = OIT0002.BASECODE " _
            & " AND OIM0003_OIL.OILCODE = OIT0003.OILCODE " _
            & " AND OIM0003_OIL.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & " AND OIM0003_OIL.DELFLG <> @P02 " _
            & " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_LASTOIL ON " _
            & "     OIM0003_LASTOIL.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0003_LASTOIL.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
            & " AND OIM0003_LASTOIL.PLANTCODE = OIT0002.BASECODE " _
            & " AND OIM0003_LASTOIL.OILCODE = OIT0005.LASTOILCODE " _
            & " AND OIM0003_LASTOIL.SEGMENTOILCODE = OIT0005.PREORDERINGTYPE " _
            & " AND OIM0003_LASTOIL.DELFLG <> @P02 "
        '### 20201013 END   油種中分類、及び前回油種取得するための条件を追加 ###########################

        '### 20200902 START 積込優先油種マスタを条件に追加(油種の優先をこのマスタで制御) ###############
        SQLStrCmn &=
              " LEFT JOIN oil.OIM0024_PRIORITY OIM0024 ON " _
            & "     OIM0024.OFFICECODE = @P01 " _
            & " AND OIM0024.OILCODE = OIT0003.OILCODE " _
            & " AND OIM0024.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & " AND OIM0024.DELFLG <> @P02 "
        '### 20200902 END   積込優先油種マスタを条件に追加(油種の優先をこのマスタで制御) ###############

        SQLStrCmn &=
              " WHERE OIT0002.OFFICECODE = @P01 " _
            & "   AND OIT0002.DELFLG <> @P02 " _
            & "   AND OIT0002.ORDERSTATUS <= @P04 " _

        '★積置フラグ無し用SQL
        If tyohyoType <> "OTLOADPLAN" Then
            '◯積込指示書の場合
            SQLStrNashi &= SQLStrCmn _
            & "   AND OIT0002.LODDATE = @P03 "
        Else
            '◯OT積込指示書の場合(発日基準)
            SQLStrNashi &= SQLStrCmn _
            & "   AND OIT0002.DEPDATE = @P03 "
        End If

        '★積置フラグ有り用SQL
        If tyohyoType <> "OTLOADPLAN" Then
            '◯積込指示書の場合
            SQLStrAri &= SQLStrCmn
        Else
            '◯OT積込指示書の場合(発日基準)
            If ChkEndMonthChk.Checked = False Then
                SQLStrAri &= SQLStrCmn _
                & "   AND OIT0002.DEPDATE = @P03 "
            Else
                '### 20201109 START 『(チェックボックス)当月積込、翌月発分を含める』がチェックされた場合 ##########
                SQLStrAri &= SQLStrCmn _
                & "   AND CONVERT(VARCHAR(6), OIT0002.DEPDATE, 112) = @P05 "
                '### 20201109 END   『(チェックボックス)当月積込、翌月発分を含める』がチェックされた場合 ##########

                '★出荷休業日マスタと一致した場合(月末休業日)
                If dtShipClose.Rows.Count <> 0 Then
                    If chkShipCloseFlg = "2" Then
                        SQLStrAri &= "AND OIT0002.TRAINNO NOT IN ("
                    Else
                        SQLStrAri &= "AND OIT0002.TRAINNO IN ("
                    End If
                    For Each dtShipCloserow As DataRow In dtShipClose.Rows
                        If dtShipCloserow("LINECNT") = "1" Then
                            SQLStrAri &= "'" + Convert.ToString(dtShipCloserow("TRAINNO")) + "'"
                        Else
                            SQLStrAri &= ", '" + Convert.ToString(dtShipCloserow("TRAINNO")) + "'"
                        End If
                    Next
                    SQLStrAri &= ")"
                End If

            End If
        End If

        'Dim SQLStr As String =
        '      " SELECT " _
        '    & "   0                                              AS LINECNT" _
        '    & " , ''                                             AS OPERATION" _
        '    & " , '0'                                            AS TIMSTP" _
        '    & " , 1                                              AS 'SELECT'" _
        '    & " , 0                                              AS HIDDEN" _
        '    & " , OIT0002.OFFICECODE                             AS OFFICECODE" _
        '    & " , OIT0002.OFFICENAME                             AS OFFICENAME" _
        '    & " , OIT0002.BASECODE                               AS BASECODE" _
        '    & " , OIT0002.BASENAME                               AS BASENAME" _
        '    & " , OIT0003.SHIPPERSCODE                           AS SHIPPERSCODE" _
        '    & " , OIT0003.SHIPPERSNAME                           AS SHIPPERSNAME" _
        '    & " , OIT0002.ARRSTATION                             AS ARRSTATION" _
        '    & " , OIT0002.ARRSTATIONNAME                         AS ARRSTATIONNAME" _
        '    & " , OIT0002.CONSIGNEECODE                          AS CONSIGNEECODE" _
        '    & " , OIT0002.CONSIGNEENAME                          AS CONSIGNEENAME" _
        '    & " , ''                                             AS LODPOINT" _
        '    & " , OIT0003.OILCODE                                AS OILCODE" _
        '    & " , OIT0003.OILNAME                                AS OILNAME" _
        '    & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
        '    & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
        '    & " , OIM0005.MODEL                                  AS MODEL" _
        '    & " , OIM0005.TANKNUMBER                             AS TANKNUMBER" _
        '    & " , OIM0005.JRINSPECTIONDATE                       AS JRINSPECTIONDATE" _
        '    & " , OIM0021.RESERVEDQUANTITY                       AS RESERVEAMOUNT" _
        '    & "	, CASE " _
        '    & "   WHEN OIT0003.STACKINGFLG ='1' AND OIT0003.ACTUALLODDATE IS NOT NULL " _
        '    & "	      THEN '積置' " _
        '    & "	      ELSE CASE " _
        '    & "	           WHEN OIT0002.STACKINGFLG = '1' " _
        '    & "	     	      THEN '積置' " _
        '    & "	   		      ELSE '' " _
        '    & "            END " _
        '    & "   END  AS STACKING " _
        '    & " , OIT0002.TRAINNO                                AS TRAINNO" _
        '    & " , OIT0002.TRAINNAME                              AS TRAINNAME" _
        '    & " , OIT0002.TOTALTANKCH                            AS TOTALTANK" _
        '    & " , CASE " _
        '    & "   WHEN OIT0003.STACKINGFLG ='1' AND OIT0003.ACTUALLODDATE IS NOT NULL" _
        '    & "	      THEN OIT0003.ACTUALLODDATE " _
        '    & "	      ELSE OIT0002_OTHER.LODDATE " _
        '    & "	  END AS LODDATE" _
        '    & " , OIT0002_OTHER.DEPDATE                          AS DEPDATE" _
        '    & " , OIT0002_OTHER.ARRDATE                          AS ARRDATE" _
        '    & " , OIT0002_OTHER.ACCDATE                          AS ACCDATE" _
        '    & " FROM OIL.OIT0002_ORDER OIT0002 " _
        '    & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
        '    & "     (OIT0003.ORDERNO = OIT0002.ORDERNO " _
        '    & "      OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
        '    & " AND OIT0003.DELFLG <> @P02 " _
        '    & " AND ((OIT0002.LODDATE = @P03 AND ISNULL(OIT0003.ACTUALLODDATE,'') = '') " _
        '    & "      OR OIT0003.ACTUALLODDATE = @P03) " _
        '    & " LEFT JOIN OIL.OIT0002_ORDER OIT0002_OTHER ON " _
        '    & "     OIT0002_OTHER.ORDERNO = OIT0003.ORDERNO " _
        '    & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
        '    & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
        '    & " AND OIM0005.DELFLG <> @P02 " _
        '    & " LEFT JOIN OIL.OIM0021_LOADRESERVE OIM0021 ON " _
        '    & "     OIM0021.OFFICECODE = OIT0002.OFFICECODE " _
        '    & " AND OIM0021.MODEL = OIM0005.MODEL " _
        '    & " AND OIM0021.LOAD = OIM0005.LOAD " _
        '    & " AND OIM0021.OILCODE = OIT0003.OILCODE " _
        '    & " AND OIM0021.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
        '    & " AND OIM0021.FROMYMD <= FORMAT(GETDATE(),'yyyy/MM/dd') " _
        '    & " AND OIM0021.TOYMD >= FORMAT(GETDATE(),'yyyy/MM/dd') " _
        '    & " AND OIM0021.DELFLG <> @P02 " _
        '    & " WHERE OIT0002.OFFICECODE = @P01 " _
        '    & "   AND OIT0002.DELFLG <> @P02 " _
        '    & "   AND OIT0002.ORDERSTATUS <= @P04 "
        '### 20200818 END   SQLの入換を実施 ####################################################

        '営業所における順序の設定
        Select Case OFFICECDE
            Case BaseDllConst.CONST_OFFICECODE_011202
                '★甲子営業所の場合
                SQLStrAri &=
                      " ORDER BY" _
                    & "    OIT0002.TRAINNO" _
                    & "  , STACKING" _
                    & "  , OIT0003.SHIPPERSCODE" _
                    & "  , OIM0024.PRIORITYNO" _
                    & "  , OIM0005.LOAD_SORT" _
                    & "  , CONVERT(int, OIM0005.TANKNUMBER)"

            Case BaseDllConst.CONST_OFFICECODE_011203
                '★袖ヶ浦営業所の場合
                SQLStrAri &=
                      " ORDER BY" _
                    & "    OIT0002.CONSIGNEECODE DESC" _
                    & "  , OIT0003.SHIPPERSCODE" _
                    & "  , OIM0024.PRIORITYNO" _
                    & "  , OIT0002.TRAINNO" _
                    & "  , STACKING"

            Case BaseDllConst.CONST_OFFICECODE_011201
                '★五井営業所の場合
                SQLStrAri &=
                      " ORDER BY" _
                    & "    OIT0002.TRAINNO" _
                    & "  , OIT0003.OTTRANSPORTFLG" _
                    & "  , STACKING" _
                    & "  , OIT0003.SHIPPERSCODE" _
                    & "  , OIM0024.PRIORITYNO"

            Case BaseDllConst.CONST_OFFICECODE_010402
                '★仙台新港営業所の場合
                If tyohyoType = CONST_RPT_OTLOADPLAN Then
                    '### 20201209 START OT積込指示書(翌月発送対応) #########################
                    SQLStrAri &=
                      " ORDER BY" _
                    & "    OIT0002.TRAINNO" _
                    & "  , OIT0003.SHIPPERSCODE" _
                    & "  , STACKING" _
                    & "  , DEPDATE" _
                    & "  , LODDATE DESC" _
                    & "  , OIM0024.PRIORITYNO"
                    '### 20201209 END   OT積込指示書(翌月発送対応) #########################
                Else
                    SQLStrAri &=
                      " ORDER BY" _
                    & "    OIT0002.TRAINNO" _
                    & "  , STACKING" _
                    & "  , DEPDATE" _
                    & "  , OIT0003.SHIPPERSCODE" _
                    & "  , OIM0024.PRIORITYNO" _
                    & "  , TANKNUMBER"
                End If

            Case Else
                ''★上記以外の営業所
                ''### 20201209 START OT積込指示書(翌月発送対応) #########################
                'If tyohyoType = CONST_RPT_OTLOADPLAN Then
                '    SQLStrAri &=
                '      " ORDER BY" _
                '    & "    OIT0002.TRAINNO" _
                '    & "  , OIT0003.SHIPPERSCODE" _
                '    & "  , STACKING" _
                '    & "  , DEPDATE" _
                '    & "  , LODDATE DESC" _
                '    & "  , OIM0024.PRIORITYNO"
                'Else
                SQLStrAri &=
                  " ORDER BY" _
                & "    OIT0002.TRAINNO" _
                & "  , STACKING" _
                & "  , OIT0003.SHIPPERSCODE" _
                & "  , OIM0024.PRIORITYNO"
                '& "  , OIT0003.OILCODE" _
                'End If
                ''### 20201209 END   OT積込指示書(翌月発送対応) #########################
        End Select

        '◯積置フラグ無し用SQLと積置フラグ有り用SQLを結合
        SQLStrNashi &=
              " UNION ALL" _
            & SQLStrAri

        Try
            Using SQLcmd As New SqlCommand(SQLStrNashi, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar)     '発日(翌月)
                PARA01.Value = OFFICECDE
                PARA02.Value = C_DELETE_FLG.DELETE
                'PARA03.Value = "2020/5/29"
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                    PARA05.Value = Format(Date.Parse(lodDate).AddMonths(1), "yyyyMM")
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                    PARA05.Value = Format(Now.AddMonths(1), "yyyyMM")
                End If
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_310

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003Reporttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003Reporttbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003Reporttbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT

                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003Reporttbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(充填ポイント表)(五井営業所)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelGoiFillingPointDataGet(ByVal SQLcon As SqlConnection,
                                      Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003ReportGoitbl) Then
            OIT0003ReportGoitbl = New DataTable
        End If

        If OIT0003ReportGoitbl.Columns.Count <> 0 Then
            OIT0003ReportGoitbl.Columns.Clear()
        End If

        OIT0003ReportGoitbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd')          AS LODDATE" _
            & " , OIT0002.LOADINGIRILINETRAINNO                  AS LOADINGIRILINETRAINNO" _
            & " , OIT0002.TOTALCNT                               AS TOTALCNT" _
            & " , VIW0013.LINECNT                                AS LINE" _
            & " , VIW0013.LOADINGPOINT                           AS LOADINGPOINT" _
            & " , OIT0002.OILCODE                                AS OILCODE" _
            & " , OIT0002.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIM0005.MODEL                                  AS MODEL" _
            & " , OIT0002.TANKNO                                 AS TANKNO" _
            & " , OIT0002.ARRSTATION                             AS ARRSTATION" _
            & " , OIT0002.ARRSTATIONNAME                         AS ARRSTATIONNAME" _
            & " , OIT0002.TRAINNO                                AS TRAINNO" _
            & " , OIT0002.TRAINNAME                              AS TRAINNAME" _
            & " , OIT0002.RETURNDATETRAIN                        AS RETURNDATETRAIN" _
            & " , OIM0003.SHIPPINGGATE                           AS SHIPPINGGATE" _
            & " FROM oil.VIW0013_OILFOR_GOI_FILLINGPOINT VIW0013 "

        SQLStr &=
              " LEFT JOIN (" _
            & "        SELECT " _
            & "          OIT0002.ORDERNO" _
            & "        , OIT0002.OFFICECODE" _
            & "        , OIT0002.LODDATE" _
            & "        , OIT0002.TOTALTANKCH" _
            & "        , OIT0002.ARRSTATION" _
            & "        , OIT0002.ARRSTATIONNAME" _
            & "        , OIT0002.TRAINNO" _
            & "        , OIT0002.TRAINNAME" _
            & "        , OIT0003.LOADINGIRILINETRAINNO" _
            & "        , OIT0003.LINE" _
            & "        , OIT0003.FILLINGPOINT" _
            & "        , OIT0003.OILCODE" _
            & "        , OIT0003.ORDERINGTYPE" _
            & "        , OIT0003.TANKNO" _
            & "        , OIT0003.RETURNDATETRAIN" _
            & "        , SUM(1) OVER (PARTITION BY OIT0003.LOADINGIRILINETRAINNO ORDER BY OIT0003.LOADINGIRILINETRAINNO) AS TOTALCNT" _
            & "        FROM oil.OIT0002_ORDER OIT0002 " _
            & "        INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "              OIT0003.ORDERNO      = OIT0002.ORDERNO " _
            & "          AND OIT0003.LOADINGIRILINETRAINNO <> '' " _
            & "          AND OIT0003.DELFLG      <> @DELFLG " _
            & "        WHERE OIT0002.OFFICECODE   = @OFFICECODE " _
            & "          AND OIT0002.LODDATE      = @LODDATE " _
            & "          AND OIT0002.ORDERSTATUS <> @ORDERSTATUS " _
            & "          AND OIT0002.DELFLG      <> @DELFLG" _
            & " ) OIT0002 ON " _
            & "     OIT0002.LOADINGIRILINETRAINNO = VIW0013.TRAINNO " _
            & " AND OIT0002.LINE = VIW0013.LINECNT " _
            & " AND OIT0002.FILLINGPOINT = VIW0013.LOADINGPOINT  "

        SQLStr &=
              " LEFT JOIN oil.OIM0005_TANK OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0002.TANKNO " _
            & " AND OIM0005.DELFLG      <> @DELFLG "

        SQLStr &=
              " LEFT JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "     OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0003.OILCODE    = OIT0002.OILCODE " _
            & " AND OIM0003.SEGMENTOILCODE = OIT0002.ORDERINGTYPE " _
            & " AND OIM0003.DELFLG    <> @DELFLG "

        SQLStr &=
              " ORDER BY" _
            & "    VIW0013.TRAINNO" _
            & "  , VIW0013.LINECNT" _
            & "  , VIW0013.LOADINGPOINT"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 20)     '受注営業所コード
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)              '削除フラグ
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.Date)                   '積込日
                Dim P_ORDERSTATUS As SqlParameter = SQLcmd.Parameters.Add("@ORDERSTATUS", SqlDbType.NVarChar, 3)    '受注進行ステータス
                P_OFFICECODE.Value = BaseDllConst.CONST_OFFICECODE_011201
                P_DELFLG.Value = C_DELETE_FLG.DELETE
                P_ORDERSTATUS.Value = BaseDllConst.CONST_ORDERSTATUS_900
                If Not String.IsNullOrEmpty(lodDate) Then
                    P_LODDATE.Value = lodDate
                Else
                    P_LODDATE.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportGoitbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportGoitbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                'Dim j As Integer = 1
                'Dim svTrainName As String = ""
                For Each OIT0003Reprow As DataRow In OIT0003ReportGoitbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT

                    ''★列車番号が変わったら回線を増加し設定
                    'If svTrainName <> "" _
                    '            AndAlso svTrainName <> Convert.ToString(OIT0003Reprow("LOADINGIRILINETRAINNO")) Then
                    '    j += 1
                    '    OIT0003Reprow("LINE") = j
                    'Else
                    '    OIT0003Reprow("LINE") = j
                    'End If
                    'svTrainName = Convert.ToString(OIT0003Reprow("LOADINGIRILINETRAINNO"))
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LGOI EXCEL_FILLINGPOINTDATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LGOI EXCEL_FILLINGPOINTDATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportGoitbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' 帳票表示(五井営業所)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelGoiDataGet(ByVal SQLcon As SqlConnection,
                                      Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003ReportGoitbl) Then
            OIT0003ReportGoitbl = New DataTable
        End If

        If OIT0003ReportGoitbl.Columns.Count <> 0 Then
            OIT0003ReportGoitbl.Columns.Clear()
        End If

        OIT0003ReportGoitbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , VIW0013.No                                     AS No" _
            & " , VIW0013.ZAIKOSORT                              AS ZAIKOSORT" _
            & " , VIW0013.TRAINNAME                              AS TRAINNAME" _
            & " , VIW0013.TRAINNO                                AS TRAINNO" _
            & " , VIW0013.OTTRAINNO                              AS OTTRAINNO" _
            & " , VIW0013.DEPSTATION                         　  AS DEPSTATION" _
            & " , VIW0013.ARRSTATION                             AS ARRSTATION" _
            & " , VIW0013.JRTRAINNO1                             AS JRTRAINNO1" _
            & " , VIW0013.TSUMI                                  AS TSUMI" _
            & " , OIT0002.OILCODE                                AS OILCODE" _
            & " , OIT0002.OILNAME                                AS OILNAME" _
            & " , OIT0002.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0002.ORDERINGOILNAME                        AS ORDERINGOILNAME"

        '### 20201002 START 変換マスタに移行したため修正 ########################
        SQLStr &=
              " , OIM0029.VALUE01                                AS REPORTOILCODE" _
            & " , OIM0029.VALUE02                                AS REPORTOILNAME"
        'SQLStr &=
        '      " , TMP0005.REPORTOILCODE                          AS REPORTOILCODE" _
        '    & " , TMP0005.REPORTOILNAME                          AS REPORTOILNAME"
        '### 20201002 END   変換マスタに移行したため修正 ########################

        SQLStr &=
              " , OIT0002.CNT                                    AS CNT" _
            & " , OIT0002.LODDATE                                AS LODDATE" _
            & " , OIT0002.DEPDATE                                AS DEPDATE" _
            & " , OIT0002.ARRDATE                                AS ARRDATE" _
            & " , OIT0002.ACCDATE                                AS ACCDATE" _
            & " FROM oil.VIW0013_OILFOR_GOI_SHIP VIW0013 "

        '★受注データより油種数を取得
        SQLStr &=
              " LEFT JOIN ( " _
            & "     SELECT " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & "     , COUNT(1) AS CNT " _
            & "     FROM oil.OIT0002_ORDER OIT0002 " _
            & "     INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "         OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "     AND OIT0003.DELFLG <> @P02 " _
            & "     AND OIT0003.OTTRANSPORTFLG IN (@P04,@P07) " _
            & "     WHERE " _
            & "         OIT0002.OFFICECODE = @P01 " _
            & "     AND OIT0002.DELFLG <> @P02 " _
            & "     AND OIT0002.LODDATE = @P03 " _
            & "     AND OIT0002.ORDERSTATUS <> @P06 " _
            & "     GROUP BY " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & " ) OIT0002 ON " _
            & "     OIT0002.TRAINNAME = VIW0013.TRAINNAME "

        'SQLStr &=
        '      " LEFT JOIN OIL.OIT0002_ORDER OIT0002 ON " _
        '    & "     OIT0002.OFFICECODE = @P01 " _
        '    & " AND OIT0002.DELFLG <> @P02 " _
        '    & " AND OIT0002.ORDERSTATUS <> @P06 " _
        '    & " AND OIT0002.TRAINNO = VIW0013.TRAINNO " _
        '    & " AND OIT0002.STACKINGFLG = VIW0013.TSUMI " _
        '    & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
        '    & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
        '    & " AND OIT0003.DELFLG <> @P02 " _
        '    & " AND OIT0003.OTTRANSPORTFLG = @P04 "

        '### 20201002 START 変換マスタに移行したため修正 ########################
        '★変換用油種コードと紐づけ
        SQLStr &=
              " LEFT JOIN oil.OIM0029_CONVERT OIM0029 ON " _
            & "     OIM0029.CLASS = 'RINKAI_OILMASTER' " _
            & " AND OIM0029.KEYCODE01 = OIT0002.OFFICECODE  " _
            & " AND OIM0029.KEYCODE02 = OIT0002.SHIPPERSCODE " _
            & " AND OIM0029.KEYCODE03 = OIT0002.BASECODE " _
            & " AND OIM0029.KEYCODE04 = '1' " _
            & " AND OIM0029.KEYCODE05 = OIT0002.OILCODE " _
            & " AND OIM0029.KEYCODE08 = OIT0002.ORDERINGTYPE "
        'SQLStr &=
        '      " LEFT JOIN oil.TMP0005OILMASTER TMP0005 ON " _
        '    & "     TMP0005.OFFICECODE = OIT0002.OFFICECODE  " _
        '    & " AND TMP0005.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
        '    & " AND TMP0005.PLANTCODE = OIT0002.BASECODE " _
        '    & " AND TMP0005.OILNo = '1' " _
        '    & " AND TMP0005.OILCODE = OIT0002.OILCODE " _
        '    & " AND TMP0005.SEGMENTOILCODE = OIT0002.ORDERINGTYPE "
        '### 20201002 END   変換マスタに移行したため修正 ########################

        Dim SQLStrADDOther As String =
              SQLStr _
            & " WHERE VIW0013.No IN ('2','3','4','5') " _
            & " ORDER BY" _
            & "    VIW0013.No" _
            & "  , VIW0013.ZAIKOSORT"

        '請負用データ取得用(倉賀野)
        Dim SQLStrADD As String =
              SQLStr _
            & " WHERE VIW0013.No = @P05 " _
            & " ORDER BY" _
            & "    VIW0013.No" _
            & "  , VIW0013.ZAIKOSORT"

        'OT用(倉賀野)
        SQLStr &=
              " WHERE VIW0013.No = @P05 " _
            & " ORDER BY" _
            & "    VIW0013.No" _
            & "  , VIW0013.ZAIKOSORT"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLADDcmd As New SqlCommand(SQLStrADD, SQLcon),
                  SQLADDOTHcmd As New SqlCommand(SQLStrADDOther, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  'OT輸送可否フラグ
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '五井帳票No
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 1)  'OT輸送可否フラグ
                PARA01.Value = BaseDllConst.CONST_OFFICECODE_011201
                PARA02.Value = C_DELETE_FLG.DELETE
                PARA06.Value = BaseDllConst.CONST_ORDERSTATUS_900
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                'OT輸送可否("1"(OT輸送あり))
                PARA04.Value = "1"
                PARA07.Value = "1"
                PARA05.Value = "1"
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportGoitbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportGoitbl.Load(SQLdr)
                End Using

                Dim PARAADDOTH01 As SqlParameter = SQLADDOTHcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARAADDOTH02 As SqlParameter = SQLADDOTHcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARAADDOTH03 As SqlParameter = SQLADDOTHcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARAADDOTH04 As SqlParameter = SQLADDOTHcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  'OT輸送可否フラグ
                'Dim PARAADDOTH05 As SqlParameter = SQLADDOTHcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '五井帳票No
                Dim PARAADDOTH06 As SqlParameter = SQLADDOTHcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARAADDOTH07 As SqlParameter = SQLADDOTHcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 1)  'OT輸送可否フラグ
                PARAADDOTH01.Value = BaseDllConst.CONST_OFFICECODE_011201
                PARAADDOTH02.Value = C_DELETE_FLG.DELETE
                PARAADDOTH06.Value = BaseDllConst.CONST_ORDERSTATUS_900
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARAADDOTH03.Value = lodDate
                Else
                    PARAADDOTH03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If

                'OT輸送可否("2"(OT輸送なし))
                PARAADDOTH04.Value = "1"
                PARAADDOTH07.Value = "2"
                'PARAADDOTH05.Value = "6"
                Using SQLdr As SqlDataReader = SQLADDOTHcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportGoitbl.Load(SQLdr)
                End Using

                Dim PARAADD01 As SqlParameter = SQLADDcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARAADD02 As SqlParameter = SQLADDcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARAADD03 As SqlParameter = SQLADDcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARAADD04 As SqlParameter = SQLADDcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  'OT輸送可否フラグ
                Dim PARAADD05 As SqlParameter = SQLADDcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '五井帳票No
                Dim PARAADD06 As SqlParameter = SQLADDcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARAADD07 As SqlParameter = SQLADDcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 1)  'OT輸送可否フラグ
                PARAADD01.Value = BaseDllConst.CONST_OFFICECODE_011201
                PARAADD02.Value = C_DELETE_FLG.DELETE
                PARAADD06.Value = BaseDllConst.CONST_ORDERSTATUS_900
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARAADD03.Value = lodDate
                Else
                    PARAADD03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If

                'OT輸送可否("2"(OT輸送なし))
                PARAADD04.Value = "2"
                PARAADD07.Value = "2"
                PARAADD05.Value = "6"
                Using SQLdr As SqlDataReader = SQLADDcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportGoitbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003ReportGoitbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT

                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LGOI EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LGOI EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportGoitbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(出荷予定（甲子）)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelKinoeneShipPlanDataGet(ByVal SQLcon As SqlConnection,
                                      Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003ReportKinoenetbl) Then
            OIT0003ReportKinoenetbl = New DataTable
        End If

        If OIT0003ReportKinoenetbl.Columns.Count <> 0 Then
            OIT0003ReportKinoenetbl.Columns.Clear()
        End If

        OIT0003ReportKinoenetbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , OIM0029.KEYCODE01                              AS OFFICECODE" _
            & " , OIM0029.KEYCODE02                              AS SEQ" _
            & " , OIM0029.KEYCODE03                              AS TRAINNO" _
            & " , OIM0029.VALUE03                                AS TRAINNAME" _
            & " , OIM0029.KEYCODE04                              AS CONSIGNEECODE" _
            & " , OIM0029.VALUE04                       　       AS CONSIGNEENAME" _
            & " , OIM0029.KEYCODE05                              AS SHIPPERSCODE" _
            & " , OIM0029.VALUE05                       　       AS SHIPPERSNAME" _
            & " , OIM0007.OTTRAINNO                              AS OTTRAINNO" _
            & " , OIT0002.OILCODE                                AS OILCODE" _
            & " , OIT0002.OILNAME                                AS OILNAME" _
            & " , OIT0002.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0002.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
            & " , OIT0002.CNT                                    AS CNT" _
            & " , OIT0002.LODDATE                                AS LODDATE" _
            & " , OIT0002.DEPDATE                                AS DEPDATE" _
            & " , OIT0002.ARRDATE                                AS ARRDATE" _
            & " , OIT0002.ACCDATE                                AS ACCDATE" _
            & " , OIM0030.ORDERFROMDATE                          AS ORDERFROMDATE" _
            & " , OIM0030.ORDERTODATE                            AS ORDERTODATE" _
            & " FROM oil.OIM0029_CONVERT OIM0029 "

        '★列車マスタよりOT列車番号を取得
        SQLStr &=
              " LEFT JOIN oil.OIM0007_TRAIN OIM0007 " _
            & " ON  OIM0007.OFFICECODE = OIM0029.KEYCODE01 " _
            & " AND OIM0007.TRAINNO = OIM0029.KEYCODE03 " _
            & " AND OIM0029.DELFLG <>  @P02 "

        '★受注データより油種数を取得
        SQLStr &=
              " LEFT JOIN ( " _
            & "     SELECT " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0002.CONSIGNEECODE " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & "     , COUNT(1) AS CNT " _
            & "     FROM oil.OIT0002_ORDER OIT0002 " _
            & "     INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "         OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "     AND OIT0003.DELFLG <> @P02 " _
            & "     WHERE " _
            & "         OIT0002.OFFICECODE = @P01 " _
            & "     AND OIT0002.DELFLG <> @P02 " _
            & "     AND OIT0002.LODDATE = @P03 " _
            & "     AND OIT0002.ORDERSTATUS <> @P04 " _
            & "     GROUP BY " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0002.CONSIGNEECODE " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & " ) OIT0002 " _
            & " ON  OIT0002.TRAINNO      = OIM0029.KEYCODE03 " _
            & " AND OIT0002.CONSIGNEECODE = OIM0029.KEYCODE04 " _
            & " AND OIT0002.SHIPPERSCODE = OIM0029.KEYCODE05 "

        '★品種出荷期限マスタより経由３号(ADO3TCH)を取得
        SQLStr &=
              " LEFT JOIN oil.OIM0030_OILTERM OIM0030 " _
            & " ON  OIM0030.OFFICECODE = OIM0029.KEYCODE01 " _
            & " AND OIM0030.OILCODE = '1404' " _
            & " AND OIM0030.SEGMENTOILCODE = 'D' " _
            & " AND OIM0030.DELFLG <> @P02 "

        SQLStr &=
              " WHERE OIM0029.CLASS = @P05 " _
            & " AND   OIM0029.KEYCODE01 = @P01 " _
            & " AND   OIM0029.DELFLG <> @P02 " _
            & " ORDER BY" _
            & "    OIM0029.KEYCODE02 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '分類
                PARA01.Value = BaseDllConst.CONST_OFFICECODE_011202
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA05.Value = "SHIP_PLAN"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportKinoenetbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportKinoenetbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003ReportKinoenetbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT

                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LKINOENE EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LKINOENE EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportKinoenetbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(甲子営業所)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelKinoeneDataGet(ByVal SQLcon As SqlConnection,
                                        Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003ReportKinoenetbl) Then
            OIT0003ReportKinoenetbl = New DataTable
        End If

        If OIT0003ReportKinoenetbl.Columns.Count <> 0 Then
            OIT0003ReportKinoenetbl.Columns.Clear()
        End If

        OIT0003ReportKinoenetbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "   VIW0013.JRTRAINNO1                             AS TRAINNO" _
            & " , VIW0013.LOADINGPOINT                           AS LOADINGPOINT" _
            & " , VIW0013.RINKAITRAINNO                          AS RINKAITRAINNO" _
            & " , VIW0013.RINKAITRAINNAME                        AS RINKAITRAINNAME" _
            & " , VIW0013.SPOTNO                                 AS SPOTNO" _
            & " , ORDERINFOTBL.TANKNO                            AS TANKNO" _
            & " , ORDERINFOTBL.MODEL                             AS MODEL"

        '### 20201007 START JXTG千葉車番を設定 ##################################################################
        SQLStr &=
              " , CASE" _
            & "   WHEN ORDERINFOTBL.MODEL = '" & BaseDllConst.CONST_MODEL_1000 & "' THEN ORDERINFOTBL.JXTGTANKNUMBER2" _
            & "   ELSE ORDERINFOTBL.TANKNO" _
            & "   END                                            AS SYARYONUMBER"
        'SQLStr &=
        '      " , CASE" _
        '    & "   WHEN ORDERINFOTBL.MODEL = '" & BaseDllConst.CONST_MODEL_1000 & "' THEN FORMAT(CONVERT(int,ORDERINFOTBL.TANKNO),'1000000')" _
        '    & "   ELSE ORDERINFOTBL.TANKNO" _
        '    & "   END                                            AS SYARYONUMBER"
        '### 20201007 END   JXTG千葉車番を設定 ##################################################################

        '### 20201021 START 指摘票対応(No185)全体 ###############################################################
        SQLStr &=
              " , CASE" _
            & "   WHEN ORDERINFOTBL.LENGTHFLG = 'S' THEN '★'" _
            & "   ELSE ''" _
            & "   END                                            AS ATTENTION"
        '### 20201021 END   指摘票対応(No185)全体 ###############################################################

        SQLStr &=
              " , ORDERINFOTBL.REPORTOILNAME                     AS REPORTOILNAME" _
            & " , ORDERINFOTBL.RESERVEDQUANTITY                  AS RESERVEDQUANTITY" _
            & " , CASE " _
            & "   WHEN ORDERINFOTBL.TANKNO <> '' THEN VIW0013.DELIVERYFIRST " _
            & "   ELSE '' " _
            & "   END                                            AS DELIVERYFIRST" _
            & " FROM oil.VIW0013_OILFOR_KINOENE_LOAD VIW0013 " _
            & "  INNER JOIN ( "

        '受注データ(KEY(甲子営業所、積込日))を取得し
        'タンク車マスタ(型式)、積込予約マスタ(予約数量)を設定
        SQLStr &=
              "      SELECT " _
            & "        OIT0002.TRAINNO " _
            & "      , OIT0002.TRAINNAME " _
            & "      , OIT0003.TANKNO " _
            & "      , OIM0005.MODEL " _
            & "      , OIM0005.LOAD " _
            & "      , OIM0005.LENGTHFLG " _
            & "      , OIM0005.JXTGTANKNUMBER2 " _
            & "      , OIT0003.OILCODE " _
            & "      , OIT0003.OILNAME " _
            & "      , OIT0003.ORDERINGTYPE " _
            & "      , OIT0003.ORDERINGOILNAME " _
            & "      , OIT0003.LINE " _
            & "      , OIT0003.FILLINGPOINT " _
            & "      , OIT0003.LOADINGIRILINETRAINNO " _
            & "      , OIT0003.LOADINGIRILINETRAINNAME " _
            & "      , OIT0003.LOADINGIRILINEORDER " _
            & "      , OIM0021.RESERVEDQUANTITY "

        '### 20201002 START 変換マスタに移行したため修正 ########################
        SQLStr &=
              "      , OIM0029.VALUE02 AS REPORTOILNAME "
        'SQLStr &=
        '      "      , TMP0005.REPORTOILNAME "
        '### 20201002 END   変換マスタに移行したため修正 ########################

        SQLStr &=
              "      FROM oil.OIT0002_ORDER OIT0002 " _
            & "       INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "           OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "       AND OIT0003.DELFLG <> @P02 " _
            & "       INNER JOIN oil.OIM0005_TANK OIM0005 ON " _
            & "           OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & "       AND OIM0005.DELFLG <> @P02 "

        '### 20201002 START 変換マスタに移行したため修正 ########################
        '★変換用油種コードと紐づけ
        SQLStr &=
              "       INNER JOIN oil.OIM0029_CONVERT OIM0029 ON " _
            & "           OIM0029.CLASS = 'RINKAI_OILMASTER' " _
            & "       AND OIM0029.KEYCODE01 = OIT0002.OFFICECODE " _
            & "       AND OIM0029.KEYCODE02 = OIT0003.SHIPPERSCODE " _
            & "       AND OIM0029.KEYCODE03 = OIT0002.BASECODE " _
            & "       AND OIM0029.KEYCODE04 = '1' " _
            & "       AND OIM0029.KEYCODE05 = OIT0003.OILCODE " _
            & "       AND OIM0029.KEYCODE08 = OIT0003.ORDERINGTYPE "
        'SQLStr &=
        '      "       INNER JOIN oil.TMP0005OILMASTER TMP0005 ON " _
        '    & "           TMP0005.OFFICECODE = OIT0002.OFFICECODE " _
        '    & "       AND TMP0005.SHIPPERCODE = OIT0003.SHIPPERSCODE " _
        '    & "       AND TMP0005.PLANTCODE = OIT0002.BASECODE " _
        '    & "       AND TMP0005.OILNo = '1' " _
        '    & "       AND TMP0005.OILCODE = OIT0003.OILCODE " _
        '    & "       AND TMP0005.SEGMENTOILCODE = OIT0003.ORDERINGTYPE "
        '### 20201002 END   変換マスタに移行したため修正 ########################

        SQLStr &=
              "       INNER JOIN oil.OIM0021_LOADRESERVE OIM0021 ON " _
            & "           OIM0021.OFFICECODE = OIT0002.OFFICECODE " _
            & "       AND OIM0021.MODEL = OIM0005.MODEL " _
            & "       AND OIM0021.LOAD = OIM0005.LOAD " _
            & "       AND OIM0021.OILCODE = OIT0003.OILCODE " _
            & "       AND OIM0021.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & "       AND OIM0021.FROMYMD <= FORMAT(GETDATE(),'yyyy/MM/dd') " _
            & "       AND OIM0021.TOYMD >= FORMAT(GETDATE(),'yyyy/MM/dd') " _
            & "       AND OIM0021.DELFLG <> @P02 " _
            & "      WHERE OIT0002.OFFICECODE = @P01 " _
            & "      AND OIT0002.LODDATE = @P03 " _
            & "      AND OIT0002.ORDERSTATUS <> @P04 " _
            & "  ) ORDERINFOTBL ON " _
            & "      VIW0013.RINKAITRAINNAME = ORDERINFOTBL.LOADINGIRILINETRAINNAME " _
            & "  AND VIW0013.SPOTNO = ORDERINFOTBL.FILLINGPOINT "

        SQLStr &=
              " ORDER BY" _
            & "    VIW0013.LOADINGPOINT " _
            & " ,  VIW0013.SPOTNO "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar)     '受注進行ステータス
                PARA01.Value = BaseDllConst.CONST_OFFICECODE_011202
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_900

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportKinoenetbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportKinoenetbl.Load(SQLdr)
                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LKINOENE EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LKINOENE EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportKinoenetbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(空回日報(袖ヶ浦))データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelSodegauraKuukaiDataGet(ByVal SQLcon As SqlConnection,
                                        Optional ByVal lodDate As String = Nothing,
                                        Optional ByVal rTrainNo As String = Nothing)

        If IsNothing(OIT0003ReportSodegauratbl) Then
            OIT0003ReportSodegauratbl = New DataTable
        End If

        If OIT0003ReportSodegauratbl.Columns.Count <> 0 Then
            OIT0003ReportSodegauratbl.Columns.Clear()
        End If

        OIT0003ReportSodegauratbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String = RSSQL.EmptyTurnDairy("OIT0003")

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                'Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)      '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)   '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 6)   '受注営業所コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.Date)          '積込日(予定)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 3)    '受注進行ステータス
                'PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE
                PARA03.Value = BaseDllConst.CONST_OFFICECODE_011203
                PARA04.Value = lodDate
                PARA05.Value = BaseDllConst.CONST_ORDERSTATUS_900

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportSodegauratbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportSodegauratbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim tblcnt As Integer = 0           '列車毎の件数を保持
                Dim tblFirstcnt As Integer = 0      '列車毎の件数(サマリー)を保持
                Dim tblMaxLinecnt As Integer = 0    '列車毎の入線順のMAX値を保持
                Dim strTrainNosave As String = ""
                Dim strRTrainNamesave As String = ""
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                For Each OIT0003Reprow As DataRow In OIT0003ReportSodegauratbl.Rows

                    '### 20210514 START 同時入線取得用 ##########################################################
                    '列車Noが前回と違う場合(または初回)
                    If strTrainNosave = "" Then
                        '★列車の合計を設定
                        tblcnt = OIT0003ReportSodegauratbl.Select("TRAINNO='" + Convert.ToString(OIT0003Reprow("TRAINNO")) + "'").Count
                        Try
                            'tblMaxLinecnt = Integer.Parse(OIT0003ReportSodegauratbl.Compute("MAX(LINEORDER)", "TRAINNO='" + Convert.ToString(OIT0003Reprow("TRAINNO")) + "'").ToString())
                            Dim maxLine As Integer = 0
                            For Each OIT0003row As DataRow In OIT0003ReportSodegauratbl.Select("TRAINNO='" + Convert.ToString(OIT0003Reprow("TRAINNO")) + "'")
                                If maxLine < Integer.Parse(OIT0003row("LINEORDER")) Then
                                    maxLine = Integer.Parse(OIT0003row("LINEORDER"))
                                End If
                            Next
                            tblMaxLinecnt = maxLine
                        Catch ex As Exception
                        End Try
                        tblFirstcnt = tblcnt
                        If tblMaxLinecnt <> tblFirstcnt Then
                            '○列車のMAX値(入線順)を設定
                            tblFirstcnt = tblMaxLinecnt
                        End If
                    ElseIf strTrainNosave <> OIT0003Reprow("TRAINNO") Then
                        '★列車の合計を設定
                        tblcnt = OIT0003ReportSodegauratbl.Select("TRAINNO='" + Convert.ToString(OIT0003Reprow("TRAINNO")) + "'").Count
                        Try
                            'tblMaxLinecnt = Integer.Parse(OIT0003ReportSodegauratbl.Compute("MAX(LINEORDER)", "TRAINNO='" + Convert.ToString(OIT0003Reprow("TRAINNO")) + "'").ToString())
                            Dim maxLine As Integer = 0
                            For Each OIT0003row As DataRow In OIT0003ReportSodegauratbl.Select("TRAINNO='" + Convert.ToString(OIT0003Reprow("TRAINNO")) + "'")
                                If maxLine < Integer.Parse(OIT0003row("LINEORDER")) Then
                                    maxLine = Integer.Parse(OIT0003row("LINEORDER"))
                                End If
                            Next
                            tblMaxLinecnt = maxLine
                        Catch ex As Exception
                        End Try
                        '★列車の合計(サマリー)を設定
                        tblFirstcnt += tblcnt
                        If tblMaxLinecnt <> tblFirstcnt Then
                            '○同時入線ではない場合は、列車の合計を設定
                            tblFirstcnt = tblcnt
                        End If
                    End If
                    Try
                        'OT順位を降順で設定
                        OIT0003Reprow("OTRANK") = (tblFirstcnt - Integer.Parse(OIT0003Reprow("LINEORDER"))) + 1
                    Catch ex As Exception
                        OIT0003Reprow("OTRANK") = ""
                    End Try
                    'tblcnt -= 1
                    '### 20210514 END   同時入線取得用 ##########################################################

                    If strTrainNosave <> "" _
                        AndAlso strTrainNosave <> Convert.ToString(OIT0003Reprow("TRAINNO")) Then
                        i = 1
                        OIT0003Reprow("LINECNT") = i        'LINECNT
                    Else
                        i += 1
                        OIT0003Reprow("LINECNT") = i        'LINECNT
                    End If
                    'O_officeCode = Convert.ToString(OIT0003Reprow("OFFICECODE"))

                    If OIT0003Reprow("RETURNDATETRAINNO").ToString() <> "" Then
                        If OIT0003Reprow("OFFICECODE") = BaseDllConst.CONST_OFFICECODE_011203 Then
                            '### 20201216 START 指摘票対応(No266)全体 ##################################################
                            '返送列車
                            WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                            WW_FixvalueMasterSearch(BaseDllConst.CONST_OFFICECODE_011203, "CTRAINNUMBER_FIND", OIT0003Reprow("RETURNDATETRAINNO").ToString(), WW_GetValue)
                            If WW_GetValue(3) = "" Then
                                OIT0003Reprow("RETURNDATETRAIN") = OIT0003Reprow("RETURNDATETRAINNO")
                            Else
                                OIT0003Reprow("RETURNDATETRAIN") = WW_GetValue(3)
                            End If
                            '### 20201216 END   指摘票対応(No266)全体 ##################################################
                        Else
                            '返送列車
                            CODENAME_get("CTRAINNUMBER", OIT0003Reprow("RETURNDATETRAINNO").ToString(), strRTrainNamesave, WW_DUMMY, I_OFFICECODE:=OIT0003Reprow("OFFICECODE"))
                            OIT0003Reprow("RETURNDATETRAIN") = strRTrainNamesave
                            If OIT0003Reprow("RETURNDATETRAIN").ToString() = "" Then OIT0003Reprow("RETURNDATETRAIN") = OIT0003Reprow("RETURNDATETRAINNO")
                        End If
                    End If
                    strTrainNosave = Convert.ToString(OIT0003Reprow("TRAINNO"))
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L EXCEL_SODEGAURA_KUUKAI_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L EXCEL_SODEGAURA_KUUKAI_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 帳票表示(出荷予定（袖ヶ浦）)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelSodegauraShipPlanDataGet(ByVal SQLcon As SqlConnection,
                                      Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003ReportSodegauratbl) Then
            OIT0003ReportSodegauratbl = New DataTable
        End If

        If OIT0003ReportSodegauratbl.Columns.Count <> 0 Then
            OIT0003ReportSodegauratbl.Columns.Clear()
        End If

        OIT0003ReportSodegauratbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , OIM0029.KEYCODE01                              AS OFFICECODE" _
            & " , OIM0029.KEYCODE02                              AS SEQ" _
            & " , OIM0029.KEYCODE03                              AS TRAINNO" _
            & " , OIM0029.VALUE03                                AS TRAINNAME" _
            & " , OIM0029.KEYCODE04                              AS CONSIGNEECODE" _
            & " , OIM0029.VALUE04                       　       AS CONSIGNEENAME" _
            & " , OIM0029.KEYCODE05                              AS SHIPPERSCODE" _
            & " , OIM0029.VALUE05                       　       AS SHIPPERSNAME" _
            & " , OIM0007.OTTRAINNO                              AS OTTRAINNO" _
            & " , OIT0002.OILCODE                                AS OILCODE" _
            & " , OIT0002.OILNAME                                AS OILNAME" _
            & " , OIT0002.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0002.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
            & " , OIT0002.CNT                                    AS CNT" _
            & " , OIT0002.LODDATE                                AS LODDATE" _
            & " , OIT0002.DEPDATE                                AS DEPDATE" _
            & " , OIT0002.ARRDATE                                AS ARRDATE" _
            & " , OIT0002.ACCDATE                                AS ACCDATE" _
            & " , OIM0030.ORDERFROMDATE                          AS ORDERFROMDATE" _
            & " , OIM0030.ORDERTODATE                            AS ORDERTODATE" _
            & " FROM oil.OIM0029_CONVERT OIM0029 "

        '★列車マスタよりOT列車番号を取得
        SQLStr &=
              " LEFT JOIN oil.OIM0007_TRAIN OIM0007 " _
            & " ON  OIM0007.OFFICECODE = OIM0029.KEYCODE01 " _
            & " AND OIM0007.TRAINNO = OIM0029.KEYCODE03 " _
            & " AND OIM0029.DELFLG <>  @P02 "

        '★受注データより油種数を取得（荷受人向け（第２荷受人が空白）の場合）
        SQLStr &=
              " LEFT JOIN ( " _
            & "     SELECT " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0002.CONSIGNEECODE" _
            & "     , OIT0002.CONSIGNEENAME" _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & "     , COUNT(1) AS CNT " _
            & "     FROM oil.OIT0002_ORDER OIT0002 " _
            & "     INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "         OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "     AND OIT0003.SECONDCONSIGNEECODE = '' " _
            & "     AND OIT0003.DELFLG <> @P02 " _
            & "     WHERE " _
            & "         OIT0002.OFFICECODE = @P01 " _
            & "     AND OIT0002.DELFLG <> @P02 " _
            & "     AND OIT0002.LODDATE = @P03 " _
            & "     AND OIT0002.ORDERSTATUS <> @P04 " _
            & "     GROUP BY " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0002.CONSIGNEECODE" _
            & "     , OIT0002.CONSIGNEENAME" _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE"

        '★受注データより油種数を取得（第２荷受人向けの場合）
        SQLStr &=
              "     UNION ALL " _
            & "     SELECT " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0003.SECONDCONSIGNEECODE AS CONSIGNEECODE" _
            & "     , OIT0003.SECONDCONSIGNEENAME AS CONSIGNEENAME" _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & "     , COUNT(1) AS CNT " _
            & "     FROM oil.OIT0002_ORDER OIT0002 " _
            & "     INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "         OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "     AND OIT0003.SECONDCONSIGNEECODE <> '' " _
            & "     AND OIT0003.DELFLG <> @P02 " _
            & "     WHERE " _
            & "         OIT0002.OFFICECODE = @P01 " _
            & "     AND OIT0002.DELFLG <> @P02 " _
            & "     AND OIT0002.LODDATE = @P03 " _
            & "     AND OIT0002.ORDERSTATUS <> @P04 " _
            & "     GROUP BY " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0003.SECONDCONSIGNEECODE" _
            & "     , OIT0003.SECONDCONSIGNEENAME" _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & " ) OIT0002 " _
            & " ON  OIT0002.TRAINNO      = OIM0029.KEYCODE03 " _
            & " AND OIT0002.SHIPPERSCODE = OIM0029.KEYCODE05 " _
            & " AND OIT0002.CONSIGNEECODE = OIM0029.KEYCODE04 "

        '★品種出荷期限マスタよりLTA出荷期間を取得
        SQLStr &=
              " LEFT JOIN oil.OIM0030_OILTERM OIM0030 " _
            & " ON  OIM0030.OFFICECODE = OIM0029.KEYCODE01 " _
            & " AND OIM0030.CONSIGNEECODE = OIM0029.KEYCODE04 " _
            & " AND OIM0030.OILCODE = '2101' " _
            & " AND OIM0030.SEGMENTOILCODE = 'C' " _
            & " AND OIM0030.DELFLG <> @P02 "

        SQLStr &=
              " WHERE OIM0029.CLASS = @P05 " _
            & " AND   OIM0029.KEYCODE01 = @P01 " _
            & " AND   OIM0029.DELFLG <> @P02 " _
            & " ORDER BY" _
            & "    OIM0029.KEYCODE02 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '分類
                PARA01.Value = BaseDllConst.CONST_OFFICECODE_011203
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA05.Value = "SHIP_PLAN"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportSodegauratbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportSodegauratbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003ReportSodegauratbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT

                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LSODEGAURA EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LSODEGAURA EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportSodegauratbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(入線方(袖ヶ浦))データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelSodegauraLinePlanDataGet(ByVal SQLcon As SqlConnection, ByRef ArrStation As String,
                                        Optional ByVal lodDate As String = Nothing,
                                        Optional ByVal rTrainNo As String = Nothing)

        If IsNothing(OIT0003ReportSodegauratbl) Then
            OIT0003ReportSodegauratbl = New DataTable
        End If

        If OIT0003ReportSodegauratbl.Columns.Count <> 0 Then
            OIT0003ReportSodegauratbl.Columns.Clear()
        End If

        OIT0003ReportSodegauratbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "   OIT0002.ORDERNO                                AS ORDERNO" _
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
            & " , OIM0007.OTTRAINNO                              AS OTTRAINNO" _
            & " , OIM0007.JRTRAINNO1                             AS JRTRAINNO1" _
            & " , OIM0007.JRTRAINNO2                             AS JRTRAINNO2" _
            & " , OIM0007.JRTRAINNO3                             AS JRTRAINNO3" _
            & " , CASE " _
            & "   WHEN SUM(1) OVER(PARTITION BY OIT0002.TRAINNO) <= 5 THEN 1 " _
            & "   WHEN SUM(1) OVER(PARTITION BY OIT0002.TRAINNO) >= 6 " _
            & "        AND SUM(1) OVER(PARTITION BY OIT0002.TRAINNO) <= 10 THEN 2 " _
            & "   WHEN SUM(1) OVER(PARTITION BY OIT0002.TRAINNO) >= 11 " _
            & "        AND SUM(1) OVER(PARTITION BY OIT0002.TRAINNO) <= 15 THEN 3 " _
            & "   WHEN SUM(1) OVER(PARTITION BY OIT0002.TRAINNO) >= 16 " _
            & "        AND SUM(1) OVER(PARTITION BY OIT0002.TRAINNO) <= 20 THEN 4 " _
            & "   END                                            AS TRAINSUM " _
            & " , OIT0002.LODDATE                                AS LODDATE" _
            & " , OIT0002.DEPDATE                                AS DEPDATE" _
            & " , OIT0002.ARRDATE                                AS ARRDATE" _
            & " , OIT0002.ACCDATE                                AS ACCDATE" _
            & " , OIT0002.EMPARRDATE                             AS EMPARRDATE" _
            & " , OIT0003.ACTUALLODDATE                          AS ACTUALLODDATE" _
            & " , OIT0003.ACTUALDEPDATE                          AS ACTUALDEPDATE" _
            & " , OIT0003.ACTUALARRDATE                          AS ACTUALARRDATE" _
            & " , OIT0003.ACTUALACCDATE                          AS ACTUALACCDATE" _
            & " , OIT0003.ACTUALEMPARRDATE                       AS ACTUALEMPARRDATE" _
            & " , ROW_NUMBER() OVER(ORDER BY OIM0007.OTFLG DESC, " _
            & "                              OIM0007.ZAIKOSORT, " _
            & "                              RIGHT ('00' + CASE OIT0003.LOADINGIRILINEORDER WHEN '' THEN OIT0003.LINEORDER ELSE OIT0003.LOADINGIRILINEORDER END, 2)) AS NYUSENNO" _
            & " , ROW_NUMBER() OVER(PARTITION BY OIM0007.OTFLG, OIM0007.ZAIKOSORT " _
            & "                     ORDER BY OIM0007.OTFLG DESC, " _
            & "                              OIM0007.ZAIKOSORT, " _
            & "                              RIGHT ('00' + CASE OIT0003.LOADINGIRILINEORDER WHEN '' THEN OIT0003.LINEORDER ELSE OIT0003.LOADINGIRILINEORDER END, 2)) AS TRAINNO_SORT" _
            & " , ''                                             AS OTRANK" _
            & " , CASE" _
            & "   WHEN OIT0003.LOADINGIRILINEORDER = '' THEN OIT0003.LINEORDER" _
            & "   ELSE OIT0003.LOADINGIRILINEORDER" _
            & "   END                                            AS LOADINGIRILINEORDER" _
            & " , OIT0003.LOADINGIRILINETRAINNO                  AS LOADINGIRILINETRAINNO" _
            & " , OIT0003.LOADINGIRILINETRAINNAME                AS LOADINGIRILINETRAINNAME" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIT0003.OILNAME                                AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME"
        '& " , OIT0003.LOADINGIRILINEORDER                    AS LOADINGIRILINEORDER" _

        '### 20201002 START 変換マスタに移行したため修正 ########################
        SQLStr &=
              " , OIM0029.VALUE02                                AS REPORTOILNAME"
        'SQLStr &=
        '      " , TMP0005.REPORTOILNAME                          AS REPORTOILNAME"
        '### 20201002 END   変換マスタに移行したため修正 ########################

        SQLStr &=
              " , CASE" _
            & "   WHEN OIM0005.MODEL = '" & BaseDllConst.CONST_MODEL_1000 & "' THEN '1-' + OIT0003.TANKNO" _
            & "   ELSE OIT0003.TANKNO" _
            & "   END                                            AS CARSNUMBER" _
            & " , OIT0003.TANKNO                                 AS TANKNO" _
            & " , OIM0005.MODEL                                  AS MODEL" _
            & " , ''                                             AS NYUUKA" _
            & " , OIT0003.LOADINGOUTLETORDER                     AS LOADINGOUTLETORDER" _
            & " , OIT0003.LOADINGOUTLETTRAINNO                   AS LOADINGOUTLETTRAINNO" _
            & " , OIT0003.LOADINGOUTLETTRAINNAME                 AS LOADINGOUTLETTRAINNAME" _
            & " , OIT0003.DELFLG                                 AS DELFLG" _
            & " FROM oil.OIT0002_ORDER OIT0002 " _
            & "  INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "      OIT0003.ORDERNO = OIT0002.ORDERNO "

        SQLStr &=
              "  AND OIT0003.LOADINGIRILINETRAINNO = @P04 "

        'If Not String.IsNullOrEmpty(rTrainNo) Then
        '    '### 「積込入線列車番号」の条件は未設定 ##########
        'Else
        '    SQLStr &=
        '      "  AND OIT0003.LOADINGIRILINETRAINNO = @P04 "
        'End If

        SQLStr &=
              "  AND OIT0003.DELFLG <> @P02 " _
            & "  LEFT JOIN oil.OIM0005_TANK OIM0005 ON " _
            & "      OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & "  AND OIM0005.DELFLG <> @P02 " _
            & "  LEFT JOIN oil.OIM0007_TRAIN OIM0007 ON " _
            & "      OIM0007.OFFICECODE = OIT0002.OFFICECODE " _
            & "  AND OIM0007.TRAINNAME = OIT0002.TRAINNAME " _
            & "  AND OIM0007.DELFLG <> @P02 "

        '### 20201002 START 変換マスタに移行したため修正 ########################
        '★変換用油種コードと紐づけ
        SQLStr &=
              "  LEFT JOIN oil.OIM0029_CONVERT OIM0029 ON " _
            & "      OIM0029.CLASS = 'RINKAI_OILMASTER' " _
            & "  AND OIM0029.KEYCODE01 =OIT0002.OFFICECODE " _
            & "  AND OIM0029.KEYCODE02 = OIT0003.SHIPPERSCODE " _
            & "  AND OIM0029.KEYCODE03 = OIT0002.BASECODE " _
            & "  AND OIM0029.KEYCODE04 = '1' " _
            & "  AND OIM0029.KEYCODE05 = OIT0003.OILCODE " _
            & "  AND OIM0029.KEYCODE08 = OIT0003.ORDERINGTYPE "
        'SQLStr &=
        '      "  LEFT JOIN oil.TMP0005OILMASTER TMP0005 ON " _
        '    & "      TMP0005.OFFICECODE =OIT0002.OFFICECODE " _
        '    & "  AND TMP0005.SHIPPERCODE = OIT0003.SHIPPERSCODE " _
        '    & "  AND TMP0005.PLANTCODE = OIT0002.BASECODE " _
        '    & "  AND TMP0005.OILNo = '1' " _
        '    & "  AND TMP0005.OILCODE = OIT0003.OILCODE " _
        '    & "  AND TMP0005.SEGMENTOILCODE = OIT0003.ORDERINGTYPE "
        '### 20201002 END   変換マスタに移行したため修正 ########################

        SQLStr &=
              " WHERE OIT0002.OFFICECODE = @P01 " _
            & " AND OIT0002.LODDATE = @P03 " _
            & " AND OIT0002.ORDERSTATUS <> @P05 "

        SQLStr &=
              " ORDER BY" _
            & "    OIM0007.OTFLG DESC " _
            & " ,  OIM0007.ZAIKOSORT " _
            & " ,  RIGHT('00' + OIT0003.LOADINGIRILINEORDER, 2) "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 4)  '積込入線列車番号
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 3)  '受注進行ステータス
                PARA01.Value = BaseDllConst.CONST_OFFICECODE_011203
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                PARA04.Value = rTrainNo
                PARA05.Value = BaseDllConst.CONST_ORDERSTATUS_900

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportSodegauratbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportSodegauratbl.Load(SQLdr)
                End Using

                Dim tblCnt As Integer = OIT0003ReportSodegauratbl.Rows.Count
                For Each OIT0003Reprow As DataRow In OIT0003ReportSodegauratbl.Select("LOADINGOUTLETORDER=''")
                    Try
                        '積込出線順を自動設定(積込入線順の値の逆値を設定する)
                        OIT0003Reprow("LOADINGOUTLETORDER") = (tblCnt - Integer.Parse(OIT0003Reprow("LOADINGIRILINEORDER")) + 1)
                    Catch ex As Exception
                        OIT0003Reprow("LOADINGOUTLETORDER") = ""
                    End Try
                Next

                Dim strTrainNo As String = ""
                For Each OIT0003Reprow As DataRow In OIT0003ReportSodegauratbl.Rows

                    '列車Noが前回と違う場合(または初回)
                    If strTrainNo = "" OrElse strTrainNo <> OIT0003Reprow("TRAINNO") Then
                        '★列車の合計を設定
                        tblCnt = OIT0003ReportSodegauratbl.Select("TRAINNO='" + Convert.ToString(OIT0003Reprow("TRAINNO")) + "'").Count
                    End If

                    'OT順位を降順で設定
                    OIT0003Reprow("OTRANK") = tblCnt
                    tblCnt -= 1

                    '列車番号を設定(比較用)
                    strTrainNo = OIT0003Reprow("TRAINNO")

                    '★着駅コードを設定
                    ArrStation = OIT0003Reprow("ARRSTATION")

                Next

                strTrainNo = ""
                Dim strNyuuka As String = "(入)"
                Dim LineCnt As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003ReportSodegauratbl.Rows

                    '列車Noが前回と違う場合(または初回)
                    If strTrainNo = "" OrElse strTrainNo <> OIT0003Reprow("TRAINNO") Then
                        '★列車の合計を設定
                        LineCnt = OIT0003Reprow("TRAINSUM")
                    End If

                    '★列車Noによって、上から設定/下から設定を切り分ける
                    Select Case OIT0003Reprow("TRAINNO")
                        '列車№:8877, 8883, 9672は下から
                        Case CONST_SODE_TRAIN_8877, CONST_SODE_TRAIN_8883, CONST_SODE_TRAIN_9672
                            If ChkSameTimeLineChk.Checked = True Then
                                If OIT0003Reprow("OTRANK") = LineCnt Then
                                    OIT0003Reprow("NYUUKA") = strNyuuka
                                    LineCnt -= 1
                                End If
                            Else
                                If OIT0003Reprow("LOADINGOUTLETORDER") <= LineCnt Then
                                    OIT0003Reprow("NYUUKA") = strNyuuka
                                    'LineCnt -= 1
                                End If
                            End If
                        '列車№:5461(JR:5972)は上から
                        Case CONST_SODE_TRAIN_5461
                            If ChkSameTimeLineChk.Checked = True Then
                                If OIT0003Reprow("TRAINNO_SORT") <= LineCnt Then
                                    OIT0003Reprow("NYUUKA") = strNyuuka
                                End If
                            Else
                                If OIT0003Reprow("LOADINGIRILINEORDER") <= LineCnt Then
                                    OIT0003Reprow("NYUUKA") = strNyuuka
                                End If
                            End If
                    End Select

                    '列車番号を設定(比較用)
                    strTrainNo = OIT0003Reprow("TRAINNO")
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LSODEGAURA EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LSODEGAURA EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportSodegauratbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(予約数量枠)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelReserveAmountDataGet(ByVal SQLcon As SqlConnection, ByVal I_LODDATE As String)

        If IsNothing(OIT0003ReportReserveAmount) Then
            OIT0003ReportReserveAmount = New DataTable
        End If

        If OIT0003ReportReserveAmount.Columns.Count <> 0 Then
            OIT0003ReportReserveAmount.Columns.Clear()
        End If

        OIT0003ReportReserveAmount.Clear()

        '○ 取得SQL
        '　 説明　：　積込指示書(三重塩浜営業所(予約数量枠))取得用SQL
        Dim SQLStr As String =
            " SELECT " _
            & "   OIM0021.OFFICECODE                             AS OFFICECODE" _
            & " , OIM0021.MODEL                                  AS MODEL" _
            & " , OIM0021.LOAD                                   AS LOAD" _
            & " , OIM0021.OILCODE                                AS OILCODE" _
            & " , OIM0021.SEGMENTOILCODE                         AS SEGMENTOILCODE" _
            & " , OIM0021.RESERVEDQUANTITY                       AS RESERVEDQUANTITY" _
            & " , '0'                                            AS DELFLG" _
            & " FROM oil.OIM0021_LOADRESERVE OIM0021 " _
            & " WHERE " _
            & "     OIM0021.OFFICECODE = @P01 " _
            & " AND OIM0021.LOAD       = 45 " _
            & " AND OIM0021.FROMYMD   <= @P02 " _
            & " AND OIM0021.TOYMD     >= @P02 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar)    '積込日
                PARA01.Value = BaseDllConst.CONST_OFFICECODE_012402
                PARA02.Value = I_LODDATE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportReserveAmount.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportReserveAmount.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LMIESHIOHAMA RESERVEAMOUNT_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LMIESHIOHAMA RESERVEAMOUNT_DATAGET"
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
    ''' 帳票表示(計画枠)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelPlanFrameDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0003ReportPlanFrame) Then
            OIT0003ReportPlanFrame = New DataTable
        End If

        If OIT0003ReportPlanFrame.Columns.Count <> 0 Then
            OIT0003ReportPlanFrame.Columns.Clear()
        End If

        OIT0003ReportPlanFrame.Clear()

        '○ 取得SQL
        '　 説明　：　出荷予定表(計画枠)取得用SQL
        Dim SQLStr As String =
            " SELECT " _
            & "   OIM0003.OFFICECODE                             AS OFFICECODE" _
            & " , OIM0003.SHIPPERCODE                            AS SHIPPERCODE" _
            & " , OIM0003.OILCODE                                AS OILCODE" _
            & " , OIM0003.OILNAME                                AS OILNAME" _
            & " , OIM0003.SEGMENTOILCODE                         AS SEGMENTOILCODE" _
            & " , OIM0003.SEGMENTOILNAME                         AS SEGMENTOILNAME" _
            & " , OIM0003.OTOILCODE                              AS OTOILCODE" _
            & " , OIM0003.OTOILNAME                              AS OTOILNAME" _
            & " , OIM0003.AVERAGELOADAMOUNT                      AS AVERAGELOADAMOUNT" _
            & " , OIM0003.SHIPPINGPLAN                           AS SHIPPINGPLAN" _
            & " , '0'                                            AS DELFLG" _
            & " FROM oil.OIM0003_PRODUCT OIM0003 " _
            & " WHERE " _
            & "     OIM0003.OFFICECODE = @P01 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6) '受注営業所コード
                PARA01.Value = BaseDllConst.CONST_OFFICECODE_011402

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportPlanFrame.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportPlanFrame.Load(SQLdr)
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LNEGISHI PLANFRAME_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LNEGISHI PLANFRAME_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportPlanFrame)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' CSV出力(積込実績(北信、甲府))データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CsvActualLoadDataGet(ByVal SQLcon As SqlConnection,
                                         ByVal tyohyoType As String,
                                         ByVal officeCode As String,
                                         ByVal lodDate As String)

        If IsNothing(OIT0003CsvActualLoadtbl) Then
            OIT0003CsvActualLoadtbl = New DataTable
        End If

        If OIT0003CsvActualLoadtbl.Columns.Count <> 0 Then
            OIT0003CsvActualLoadtbl.Columns.Clear()
        End If

        OIT0003CsvActualLoadtbl.Clear()

        '○ 取得SQL
        '　 説明　：　CSV用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "   MONTH(OIT0002.LODDATE)                         AS LODDATE_MM" _
            & " , DAY(OIT0002.LODDATE)                           AS LODDATE_DD" _
            & " , OIT0002.TRAINNO                                AS TRAINNO" _
            & " , OIT0003.SHIPORDER                              AS SHIPORDER" _
            & " , CASE  " _
            & "        WHEN OIM0005.MODEL = 'タキ1000' THEN 1000000 + CONVERT(INT, OIM0005.TANKNUMBER) " _
            & "        ELSE OIM0005.TANKNUMBER " _
            & "   END AS TANKNO " _
            & " , OIM0029.VALUE01                                AS OILKANA" _
            & " , OIM0029.VALUE02                                AS OILOUTCODE" _
            & " , CONVERT(INT, OIT0003.CARSAMOUNT * 1000)        AS CARSAMOUNT" _
            & " , CASE OIT0002.CONSIGNEECODE " _
            & String.Format("        WHEN '{0}' THEN '北信（油）'", BaseDllConst.CONST_CONSIGNEECODE_10) _
            & String.Format("        WHEN '{0}' THEN '甲府（油）'", BaseDllConst.CONST_CONSIGNEECODE_20) _
            & "        ELSE '' " _
            & "   END AS CONSIGNEENAME" _
            & " FROM oil.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "         OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "     And OIT0003.DELFLG <> @P02 " _

        '★タンク車マスタより車番を取得
        SQLStr &=
              " LEFT JOIN oil.OIM0005_TANK OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & " And OIM0005.DELFLG    <> @P02 "

        '★変換マスタより油種変換を取得
        SQLStr &=
              " LEFT JOIN oil.OIM0029_CONVERT OIM0029 " _
            & " ON  OIM0029.CLASS     = @P05 " _
            & " And OIM0029.KEYCODE01 =  @P01 " _
            & " And OIM0029.KEYCODE02 =  OIT0003.OILCODE " _
            & " And OIM0029.KEYCODE04 =  ORDERINGTYPE " _
            & " AND OIM0029.DELFLG   <>  @P02 "

        SQLStr &=
              " WHERE OIT0002.OFFICECODE    = @P01 " _
            & " AND   OIT0002.CONSIGNEECODE = @P06 " _
            & " AND   OIT0002.LODDATE       = @P03 " _
            & " AND   OIT0002.ORDERSTATUS  <> @P04 " _
            & " AND   OIT0002.DELFLG       <> @P02 " _
            & " ORDER BY" _
            & "    OIT0002.TRAINNO " _
            & "  , CONVERT(INT, OIT0003.SHIPORDER) "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '分類
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 10) '荷受人コード
                PARA01.Value = officeCode
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA05.Value = "LOAD_OILCODE"
                Select Case tyohyoType
                    Case CONST_CSV_ACTUALLOAD_10
                        PARA06.Value = BaseDllConst.CONST_CONSIGNEECODE_10
                    Case CONST_CSV_ACTUALLOAD_20
                        PARA06.Value = BaseDllConst.CONST_CONSIGNEECODE_20
                End Select

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003CsvActualLoadtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003CsvActualLoadtbl.Load(SQLdr)
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LNEGISHI CSV_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LNEGISHI CSV_DATAGET"
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
    ''' 帳票表示(根岸営業所)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelNegishiDataGet(ByVal SQLcon As SqlConnection,
                                      ByVal type As String,
                                      Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003WKtbl) Then
            OIT0003WKtbl = New DataTable
        End If

        If OIT0003WKtbl.Columns.Count <> 0 Then
            OIT0003WKtbl.Columns.Clear()
        End If

        OIT0003WKtbl.Clear()

        '○ 取得SQL
        '　 説明　：　充填ポイント取得用SQL
        Dim SQLLDP As String =
            " SELECT " _
            & "   OIM0003.OFFICECODE                             AS OFFICECODE" _
            & " , OIM0003.SHIPPERCODE                            AS SHIPPERCODE" _
            & " , OIM0013.PLANTCODE                              AS PLANTCODE" _
            & " , OIM0013.MAXLINECNT                             AS MAXLINECNT" _
            & " , OIM0013.LOADINGPOINT                           AS LOADINGPOINT" _
            & " , OIM0013.OILCODE                                AS OILCODE" _
            & " , OIM0003.OILNAME                                AS OILNAME" _
            & " , OIM0013.SEGMENTOILCODE                         AS SEGMENTOILCODE" _
            & " , OIM0003.SEGMENTOILNAME                         AS SEGMENTOILNAME" _
            & " , OIM0003.OTOILCODE                              AS OTOILCODE" _
            & " , OIM0003.OTOILNAME                              AS OTOILNAME" _
            & " , OIM0024.PRIORITYNO                             AS PRIORITYNO" _
            & " , OIM0013.ORDERPOINT                             AS ORDERPOINT" _
            & " , '0'                                            AS DELFLG" _
            & " FROM oil.OIM0013_LOAD OIM0013 " _
            & " INNER JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "     OIM0003.OFFICECODE = @P01 " _
            & " AND OIM0003.PLANTCODE = OIM0013.PLANTCODE " _
            & " AND OIM0003.OILCODE = OIM0013.OILCODE " _
            & " LEFT JOIN oil.OIM0024_PRIORITY OIM0024 ON " _
            & "     OIM0024.OFFICECODE = OIM0003.OFFICECODE " _
            & " AND OIM0024.OILCODE = OIM0013.OILCODE " _
            & " AND OIM0024.SEGMENTOILCODE = OIM0013.SEGMENTOILCODE " _
            & " AND OIM0024.DELFLG <> @P02 "

        SQLLDP &=
                " ORDER BY" _
            & "    OIM0024.PRIORITYNO " _
            & "  , OIM0013.ORDERPOINT " _
            & "  , OIM0013.LOADINGPOINT " _
            & "  , OIM0013.OILCODE"

        If IsNothing(OIT0003ReportNegishitbl) Then
            OIT0003ReportNegishitbl = New DataTable
        End If

        If OIT0003ReportNegishitbl.Columns.Count <> 0 Then
            OIT0003ReportNegishitbl.Columns.Clear()
        End If

        OIT0003ReportNegishitbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
        " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , VIW0013.No                                     AS No" _
            & " , VIW0013.ZAIKOSORT                              AS ZAIKOSORT" _
            & " , ROW_NUMBER() OVER(PARTITION BY VIW0013.No ORDER BY VIW0013.No, VIW0013.ZAIKOSORT, VIW0013.JRTRAINNO1) AS ZAIKOSORT_KAI" _
            & " , VIW0013.TRAINNAME                              AS TRAINNAME" _
            & " , VIW0013.TRAINNO                                AS TRAINNO" _
            & " , VIW0013.JRTRAINNO1                             AS JRTRAINNO1" _
            & " , VIW0013.TSUMI                                  AS TSUMI" _
            & " , ''                                             AS FILLINGPOINT" _
            & " , ROW_NUMBER() OVER(PARTITION BY VIW0013.No, VIW0013.ZAIKOSORT ORDER BY VIW0013.No, VIW0013.ZAIKOSORT, VIW0013.JRTRAINNO1) AS FILLINGPOINT_KAI" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIM0003.OILNAME                                AS OILNAME" _
            & " , OIM0003.OILKANA                                AS OILKANA" _
            & " , ISNULL(CASE " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_HTank + "' THEN OIT0002.HTANKCH " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_RTank + "' THEN OIT0002.RTANKCH " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_TTank + "' THEN OIT0002.TTANKCH " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_MTTank + "' THEN OIT0002.MTTANKCH " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_KTank1 + "' THEN OIT0002.KTANKCH " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_K3Tank1 + "' THEN OIT0002.K3TANKCH " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_ATank + "' THEN OIT0002.ATANKCH " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_LTank1 + "' THEN OIT0002.LTANKCH " _
            & "   END, 0)                                        AS TOTALTANK" _
            & " , OIM0024.PRIORITYNO                             AS PRIORITYNO"

        '★予備枠用SQLセット
        Dim SQLStrYobi As String = SQLStr & " , ISNULL(OIT0002.RNUM, 2) AS RNUM"

        SQLStr &= " , ISNULL(OIT0002.RNUM, 1) AS RNUM"

        '& "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_HTank + "' THEN IIF(OIT0002.HTANKCH <> 0, OIT0002.HTANKCH, OIT0002.HTANK) " _
        '& "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_RTank + "' THEN IIF(OIT0002.RTANKCH <> 0, OIT0002.RTANKCH, OIT0002.RTANK) " _
        '& "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_TTank + "' THEN IIF(OIT0002.TTANKCH <> 0, OIT0002.TTANKCH, OIT0002.TTANK) " _
        '& "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_MTTank + "' THEN IIF(OIT0002.MTTANKCH <> 0, OIT0002.MTTANKCH, OIT0002.MTTANK) " _
        '& "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_KTank1 + "' THEN IIF(OIT0002.KTANKCH <> 0, OIT0002.KTANKCH, OIT0002.KTANK) " _
        '& "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_K3Tank1 + "' THEN IIF(OIT0002.K3TANKCH <> 0, OIT0002.K3TANKCH, OIT0002.K3TANK) " _
        '& "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_ATank + "' THEN IIF(OIT0002.ATANKCH <> 0, OIT0002.ATANKCH, OIT0002.ATANK) " _
        '& "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_LTank1 + "' THEN IIF(OIT0002.LTANKCH <> 0, OIT0002.LTANKCH, OIT0002.LTANK) " _
        '& "   END, 0)                                        AS TOTALTANK"

        '★帳票の種類によって(枠)を変更
        If type = "NEGISHI_LOADPLAN" Then   '★積込予定(根岸)
            SQLStr &=
              " FROM oil.VIW0013_OILFOR_NEGISHI_LOAD VIW0013 "
            SQLStrYobi &=
              " FROM oil.VIW0013_OILFOR_NEGISHI_LOAD_SUB VIW0013 "

        ElseIf type = "SHIPPLAN" Then       '★出荷予定(根岸)
            SQLStr &=
              " FROM oil.VIW0013_OILFOR_NEGISHI_SHIP VIW0013 "
            SQLStrYobi &=
              " FROM oil.VIW0013_OILFOR_NEGISHI_SHIP_SUB VIW0013 "
        End If

        '★共通SQL
        Dim SQLStrCmn As String =
              " LEFT JOIN ( " _
            & "     SELECT OIT0002.*" _
            & "          , ROW_NUMBER() OVER(PARTITION BY OIT0002.TRAINNO, OIT0002.LODDATE " _
            & "                              ORDER BY OIT0002.LODDATE, OIT0002.DEPDATE) RNUM " _
            & "     FROM OIL.OIT0002_ORDER OIT0002 " _
            & "     WHERE OIT0002.LODDATE = @P03 " _
            & "       AND OIT0002.OFFICECODE = @P01 " _
            & "       AND OIT0002.DELFLG <> @P02 " _
            & "       AND OIT0002.ORDERSTATUS <> @P04 " _
            & " ) OIT0002 ON "
        '" LEFT JOIN OIL.OIT0002_ORDER OIT0002 ON "

        SQLStrCmn &=
              "     OIT0002.LODDATE = @P03 " _
            & " AND OIT0002.TRAINNO = VIW0013.TRAINNO " _
            & " AND OIT0002.OFFICECODE = @P01 " _
            & " AND OIT0002.DELFLG <> @P02 " _
            & " AND OIT0002.ORDERSTATUS <> @P04 " _
            & " AND ISNULL(OIT0002.RNUM, @P05) = @P05 " _
            & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " LEFT JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "     OIM0003.OFFICECODE = @P01 " _
            & " AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & " AND OIM0003.DELFLG <> @P02 "

        '### 20200710 START 積込優先油種マスタを条件に追加(油種の優先をこのマスタで制御) ###############
        SQLStrCmn &=
              " LEFT JOIN oil.OIM0024_PRIORITY OIM0024 ON " _
            & "     OIM0024.OFFICECODE = @P01 " _
            & " AND OIM0024.OILCODE = OIT0003.OILCODE " _
            & " AND OIM0024.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & " AND OIM0024.DELFLG <> @P02 "
        '### 20200710 END   積込優先油種マスタを条件に追加(油種の優先をこのマスタで制御) ###############

        SQLStrCmn &=
                " ORDER BY" _
            & "    VIW0013.No" _
            & "  , VIW0013.ZAIKOSORT" _
            & "  , VIW0013.JRTRAINNO1" _
            & "  , OIM0024.PRIORITYNO"
        '& "  , TOTALTANK　DESC"

        '### 20201020 START 指摘票対応(No174)全体 ##################################################
        SQLStr &= SQLStrCmn
        SQLStrYobi &= SQLStrCmn
        '### 20201020 END   指摘票対応(No174)全体 ##################################################

        '### 20201105 START 指摘票対応(No191) ####################################################################
        If IsNothing(OIT0003ItemGettbl) Then
            OIT0003ItemGettbl = New DataTable
        End If

        If OIT0003ItemGettbl.Columns.Count <> 0 Then
            OIT0003ItemGettbl.Columns.Clear()
        End If

        OIT0003ItemGettbl.Clear()

        '★値取得SQL(3号軽油のFROM, TOを取得(帳票に反映するため))
        '### 20201126 START 指摘票対応(No230) ####################################################################
        Dim SQLGetStr As String =
              " SELECT " _
            & "   OIM0030.OILCODE           AS OILCODE" _
            & " , OIM0030.SEGMENTOILCODE    AS SEGMENTOILCODE" _
            & " , OIM0030.CONSIGNEECODE     AS CONSIGNEECODE" _
            & " , OIM0030.ORDERFROMDATE     AS ORDERFROMDATE" _
            & " , OIM0030.ORDERTODATE       AS ORDERTODATE" _
            & " FROM oil.OIM0030_OILTERM OIM0030 " _
            & " WHERE OIM0030.OFFICECODE = @P01 " _
            & "   AND OIM0030.OILCODE = @P02 "
        'Dim SQLGetStr As String =
        '      " SELECT " _
        '    & "   OIM0003.OILCODE           AS OILCODE" _
        '    & " , OIM0003.OILNAME           AS OILNAME" _
        '    & " , OIM0003.SEGMENTOILCODE    AS SEGMENTOILCODE" _
        '    & " , OIM0003.SEGMENTOILNAME    AS SEGMENTOILNAME" _
        '    & " , OIM0003.ORDERFROMDATE     AS ORDERFROMDATE" _
        '    & " , OIM0003.ORDERTODATE       AS ORDERTODATE" _
        '    & " FROM oil.OIM0003_PRODUCT OIM0003 " _
        '    & " WHERE OIM0003.OFFICECODE = @P01 " _
        '    & "   AND OIM0003.OILCODE = @P02 "
        '### 20201126 END   指摘票対応(No230) ####################################################################
        '### 20201105 END   指摘票対応(No191) ####################################################################

#Region "コメントアウト"
        ' ### START 在庫管理(シミュレーション)の設定が前提の場合 #########################
        '" SELECT " _
        '    & "   0                                              AS LINECNT" _
        '    & " , ''                                             AS OPERATION" _
        '    & " , '0'                                            AS TIMSTP" _
        '    & " , 1                                              AS 'SELECT'" _
        '    & " , 0                                              AS HIDDEN" _
        '    & " , VIW0013.No                                     AS No" _
        '    & " , VIW0013.ZAIKOSORT                              AS ZAIKOSORT" _
        '    & " , VIW0013.TRAINNAME                              AS TRAINNAME" _
        '    & " , VIW0013.TRAINNO                                AS TRAINNO" _
        '    & " , VIW0013.JRTRAINNO1                             AS JRTRAINNO1" _
        '    & " , VIW0013.TSUMI                                  AS TSUMI" _
        '    & " , ''                                             AS FILLINGPOINT" _
        '    & " , VIW0013_OIL.OILCODE                            AS OILCODE" _
        '    & " , OIM0003.OILNAME                                AS OILNAME" _
        '    & " , ISNULL(VIW0013_OIL.TOTALTANK,0)                AS TOTALTANK" _
        '    & " FROM oil.VIW0013_OILFOR_NEGISHI_SHIP VIW0013 " _
        '    & " LEFT JOIN oil.VIW0013_OILSTOCK_NEGISHI_SHIP VIW0013_OIL ON " _
        '    & "     VIW0013_OIL.STOCKYMD = FORMAT(GETDATE(), 'yyyy/MM/dd') " _
        '    & " AND VIW0013_OIL.TRAINNO = VIW0013.TRAINNO " _
        '    & " LEFT JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
        '    & "     OIM0003.OFFICECODE = @P01 " _
        '    & " AND OIM0003.OILCODE = VIW0013_OIL.OILCODE " _
        '    & " AND OIM0003.DELFLG <> @P02 "

        'SQLStr &=
        '        " ORDER BY" _
        '    & "    VIW0013.No" _
        '    & "  , VIW0013.ZAIKOSORT" _
        '    & "  , VIW0013_OIL.OILNUMBER"
        ' ### END   ######################################################################

        ' ### START 充填ポイント設定が前提の場合 #########################################
        '" SELECT " _
        '    & "   0                                              AS LINECNT" _
        '    & " , ''                                             AS OPERATION" _
        '    & " , '0'                                            AS TIMSTP" _
        '    & " , 1                                              AS 'SELECT'" _
        '    & " , 0                                              AS HIDDEN" _
        '    & " , VIW0013.No                                     AS No" _
        '    & " , VIW0013.ZAIKOSORT                              AS ZAIKOSORT" _
        '    & " , VIW0013.TRAINNAME                              AS TRAINNAME" _
        '    & " , VIW0013.TRAINNO                                AS TRAINNO" _
        '    & " , VIW0013.JRTRAINNO1                             AS JRTRAINNO1" _
        '    & " , VIW0013.TSUMI                                  AS TSUMI" _
        '    & " , OIT0003.FILLINGPOINT                           AS FILLINGPOINT" _
        '    & " , OIT0003.OILCODE                                AS OILCODE" _
        '    & " , OIT0003.OILNAME                                AS OILNAME" _
        '    & " , OIT0002.TOTALTANK                              AS TOTALTANK" _
        '    & " FROM oil.VIW0013_OILFOR_NEGISHI_SHIP VIW0013 " _
        '    & " LEFT JOIN OIL.OIT0002_ORDER OIT0002 ON " _
        '    & "     OIT0002.LODDATE = FORMAT(GETDATE(), 'yyyy/MM/dd') " _
        '    & " AND OIT0002.TRAINNO = VIW0013.TRAINNO " _
        '    & " AND OIT0002.OFFICECODE = @P01 " _
        '    & " AND OIT0002.DELFLG <> @P02 " _
        '    & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
        '    & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
        '    & " AND OIT0003.DELFLG <> @P02 "

        'SQLStr &=
        '        " ORDER BY" _
        '    & "    VIW0013.No" _
        '    & "  , VIW0013.ZAIKOSORT" _
        '    & "  , OIT0003.FILLINGPOINT"
        ' ### END   ######################################################################
#End Region

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon),
                  SQLYobicmd As New SqlCommand(SQLStrYobi, SQLcon),
                  SQLLDPcmd As New SqlCommand(SQLLDP, SQLcon),
                  SQLGetcmd As New SqlCommand(SQLGetStr, SQLcon)
                Dim PARALDP01 As SqlParameter = SQLLDPcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARALDP02 As SqlParameter = SQLLDPcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARALDP01.Value = BaseDllConst.CONST_OFFICECODE_011402
                PARALDP02.Value = C_DELETE_FLG.DELETE

                Using SQLLDPdr As SqlDataReader = SQLLDPcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLLDPdr.FieldCount - 1
                        OIT0003WKtbl.Columns.Add(SQLLDPdr.GetName(index), SQLLDPdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WKtbl.Load(SQLLDPdr)
                End Using

                '### 20201105 START 指摘票対応(No191) ####################################################################
                Dim PARAGET01 As SqlParameter = SQLGetcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARAGET02 As SqlParameter = SQLGetcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '油種コード
                PARAGET01.Value = BaseDllConst.CONST_OFFICECODE_011402
                PARAGET02.Value = BaseDllConst.CONST_K3Tank1

                Using SQLGETdr As SqlDataReader = SQLGetcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLGETdr.FieldCount - 1
                        OIT0003ItemGettbl.Columns.Add(SQLGETdr.GetName(index), SQLGETdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ItemGettbl.Load(SQLGETdr)
                End Using
                '### 20201105 END   指摘票対応(No191) ####################################################################

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar)     '受注進行ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar)     '同日積込日データ優先１取得
                PARA01.Value = BaseDllConst.CONST_OFFICECODE_011402
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA05.Value = 1

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportNegishitbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportNegishitbl.Load(SQLdr)
                End Using

                '### 20201020 START 指摘票対応(No174)全体 ##################################################
                Dim PARAY01 As SqlParameter = SQLYobicmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARAY02 As SqlParameter = SQLYobicmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARAY03 As SqlParameter = SQLYobicmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARAY04 As SqlParameter = SQLYobicmd.Parameters.Add("@P04", SqlDbType.NVarChar)     '受注進行ステータス
                Dim PARAY05 As SqlParameter = SQLYobicmd.Parameters.Add("@P05", SqlDbType.NVarChar)     '同日積込日データ優先２取得
                PARAY01.Value = BaseDllConst.CONST_OFFICECODE_011402
                PARAY02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARAY03.Value = lodDate
                Else
                    PARAY03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                PARAY04.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARAY05.Value = 2
                Using SQLdr As SqlDataReader = SQLYobicmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportNegishitbl.Load(SQLdr)
                End Using
                '### 20201020 END   指摘票対応(No174)全体 ##################################################

                Dim i As Integer = 0
                Dim strTrainNo As String = ""
                Dim strOilCode As String = ""

                'OIT0003ReportNegishitbl.AsEnumerable().
                '        Where(Function(r) r.Item("OILCODE") IsNot DBNull.Value).
                '        GroupBy(Function(r) Tuple.Create(r.Item("TRAINNO"), r.Item("OILCODE"), r.Item("PRIORITYNO"))).
                '        Select(Function(g) New With {
                '            .trainNo = g.Key.Item1,
                '            .oilCode = g.Key.Item2,
                '            .priorityNo = g.Key.Item3,
                '            .rows = g.Select(Function(r) r)
                '                   }).
                '        ToList().ForEach(
                '        Sub(g)

                '            '★列車Noが変更になったら、充填ポイントの内容を初期化
                '            If strTrainNo <> "" AndAlso strTrainNo <> g.trainNo Then
                '                For Each OIT0003Wkrow As DataRow In OIT0003WKtbl.Rows
                '                    OIT0003Wkrow("DELFLG") = "0"
                '                Next
                '            End If

                '            g.rows.ToList.ForEach(
                '            Sub(r)

                '                '現油種の空きレコード
                '                Dim thisFreePoints = OIT0003WKtbl.AsEnumerable().
                '                Where(Function(wr) wr("DELFLG") <> "1" AndAlso wr("OILCODE") = g.oilCode).
                '                Select(Function(wr) CInt(wr("LOADINGPOINT"))).Distinct().ToList()

                '                '優先する空きレコード
                '                Dim priorityPoints = OIT0003WKtbl.AsEnumerable().
                '                Where(Function(wr)
                '                          Return wr("DELFLG") <> "1" _
                '                          AndAlso wr("PRIORITYNO") < g.priorityNo _
                '                          AndAlso thisFreePoints.Exists(Function(x) x = CInt(wr("LOADINGPOINT")))
                '                      End Function).
                '                Select(Function(wr) CInt(wr("LOADINGPOINT"))).Distinct().ToList()

                '                '★表示用の油種コードと充填ポイントで設定している油種コードを比較
                '                For Each OIT0003Wkrow As DataRow In OIT0003WKtbl.Rows
                '                    If OIT0003Wkrow("DELFLG") = "1" Then Continue For
                '                    If OIT0003Wkrow("OILCODE") <> r("OILCODE") Then Continue For

                '                    If thisFreePoints.Any() AndAlso
                '                    Not thisFreePoints.Exists(Function(x) x = OIT0003Wkrow("LOADINGPOINT")) Then
                '                        Continue For
                '                    End If

                '                    If priorityPoints.Any() AndAlso
                '                    Not priorityPoints.Exists(Function(x) x = OIT0003Wkrow("LOADINGPOINT")) Then
                '                        Continue For
                '                    End If

                '                    '★充填ポイントの設定
                '                    r("FILLINGPOINT") = OIT0003Wkrow("LOADINGPOINT")
                '                    OIT0003Wkrow("DELFLG") = "1"

                '                    '★設定した充填ポイントはすべて使用済みにする
                '                    For Each OIT0003Wk2row As DataRow In OIT0003WKtbl.Rows
                '                        If OIT0003Wk2row("LOADINGPOINT") = OIT0003Wkrow("LOADINGPOINT") Then
                '                            OIT0003Wk2row("DELFLG") = "1"
                '                        End If
                '                    Next
                '                    Exit For
                '                Next
                '            End Sub)

                '            '★列車No退避
                '            strTrainNo = g.trainNo
                '            strOilCode = g.oilCode

                '        End Sub)

                'For Each OIT0003Reprow As DataRow In OIT0003ReportNegishitbl.Rows
                '    i += 1
                '    OIT0003Reprow("LINECNT") = i        'LINECNT
                'Next

                For Each OIT0003Reprow As DataRow In OIT0003ReportNegishitbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT

                    '★油種コードが未設定（NULL）の場合は次のデータへ遷移
                    If OIT0003Reprow("OILCODE") Is DBNull.Value Then
                        '★列車No退避
                        strTrainNo = OIT0003Reprow("TRAINNO")
                        Continue For
                    End If

                    '★列車Noが変更になったら、充填ポイントの内容を初期化
                    If strTrainNo <> "" AndAlso strTrainNo <> OIT0003Reprow("TRAINNO") Then
                        For Each OIT0003Wkrow As DataRow In OIT0003WKtbl.Rows
                            OIT0003Wkrow("DELFLG") = "0"
                        Next
                    End If

                    '★表示用の油種コードと充填ポイントで設定している油種コードを比較
                    For Each OIT0003Wkrow As DataRow In OIT0003WKtbl.Rows
                        '### 20210401 START 油種を優先順に上から設定するに変更 ##########################
                        '①充填ポイントで一致する(油種順に設定)
                        '②油種で一致する(テンプレートに設定された油種で設定)
                        '### 20210401 END   油種を優先順に上から設定するに変更 ##########################
                        'If OIT0003Wkrow("DELFLG") <> "1" _
                        'AndAlso OIT0003Wkrow("OILCODE") = OIT0003Reprow("OILCODE") Then
                        If OIT0003Wkrow("DELFLG") <> "1" _
                            AndAlso OIT0003Wkrow("LOADINGPOINT") = OIT0003Reprow("FILLINGPOINT_KAI") Then
                            OIT0003Reprow("FILLINGPOINT") = OIT0003Wkrow("LOADINGPOINT")
                            OIT0003Wkrow("DELFLG") = "1"

                            '★設定した充填ポイントはすべて使用済みにする
                            For Each OIT0003Wk2row As DataRow In OIT0003WKtbl.Rows
                                If OIT0003Wk2row("LOADINGPOINT") = OIT0003Wkrow("LOADINGPOINT") Then
                                    OIT0003Wk2row("DELFLG") = "1"
                                End If
                            Next
                            Exit For
                        End If
                    Next

                    '★列車No退避
                    strTrainNo = OIT0003Reprow("TRAINNO")
                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LNEGISHI EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LNEGISHI EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportNegishitbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
            & " AND OIT0002.DELFLG <> @P02 "

        'SQLStr &=
        '        " ORDER BY" _
        '    & "    OIT0002.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                PARA01.Value = officeCode
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LDELIVERY EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LDELIVERY EXCEL_DATAGET"
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
    ''' 帳票表示(出荷予定（四日市営業所, 三重塩浜営業所）)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelCentralOfficeShipPlanDataGet(ByVal SQLcon As SqlConnection,
                                                  ByVal I_OFFICECODE As String,
                                                  Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003ReportMieShiohamatbl) Then
            OIT0003ReportMieShiohamatbl = New DataTable
        End If

        If OIT0003ReportMieShiohamatbl.Columns.Count <> 0 Then
            OIT0003ReportMieShiohamatbl.Columns.Clear()
        End If

        OIT0003ReportMieShiohamatbl.Clear()

        If IsNothing(OIT0003ReportOilDuration) Then
            OIT0003ReportOilDuration = New DataTable
        End If

        If OIT0003ReportOilDuration.Columns.Count <> 0 Then
            OIT0003ReportOilDuration.Columns.Clear()
        End If

        OIT0003ReportOilDuration.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , OIM0029.KEYCODE01                              AS OFFICECODE" _
            & " , OIM0029.KEYCODE02                              AS SEQ" _
            & " , OIM0029.KEYCODE03                              AS TRAINNO" _
            & " , OIM0029.VALUE03                                AS TRAINNAME" _
            & " , OIM0029.KEYCODE04                              AS CONSIGNEECODE" _
            & " , OIM0029.VALUE04                       　       AS CONSIGNEENAME" _
            & " , OIM0029.KEYCODE05                              AS SHIPPERSCODE" _
            & " , OIM0029.VALUE05                       　       AS SHIPPERSNAME" _
            & " , OIM0007.OTTRAINNO                              AS OTTRAINNO" _
            & " , OIT0002.OILCODE                                AS OILCODE" _
            & " , OIT0002.OILNAME                                AS OILNAME" _
            & " , OIT0002.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0002.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
            & " , OIT0002.CNT                                    AS CNT" _
            & " , OIT0002.LODDATE                                AS LODDATE" _
            & " , OIT0002.DEPDATE                                AS DEPDATE" _
            & " , OIT0002.ARRDATE                                AS ARRDATE" _
            & " , OIT0002.ACCDATE                                AS ACCDATE" _
            & " FROM oil.OIM0029_CONVERT OIM0029 "
        '& " , OIM0030.ORDERFROMDATE                          AS ORDERFROMDATE" _
        '& " , OIM0030.ORDERTODATE                            AS ORDERTODATE" _

        '★列車マスタよりOT列車番号を取得
        SQLStr &=
              " LEFT JOIN oil.OIM0007_TRAIN OIM0007 " _
            & " ON  OIM0007.OFFICECODE = OIM0029.KEYCODE01 " _
            & " AND OIM0007.TRAINNO = OIM0029.KEYCODE03 " _
            & " AND OIM0007.TRAINNAME = OIM0029.VALUE03 " _
            & " AND OIM0029.DELFLG <>  @P02 "

        '★受注データより油種数を取得（荷受人向け（第２荷受人が空白）の場合）
        SQLStr &=
              " LEFT JOIN ( " _
            & "     SELECT " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0002.CONSIGNEECODE" _
            & "     , OIT0002.CONSIGNEENAME" _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & "     , COUNT(1) AS CNT " _
            & "     FROM oil.OIT0002_ORDER OIT0002 " _
            & "     INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "         OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "     AND OIT0003.SECONDCONSIGNEECODE = '' " _
            & "     AND OIT0003.DELFLG <> @P02 " _
            & "     WHERE " _
            & "         OIT0002.OFFICECODE = @P01 " _
            & "     AND OIT0002.DELFLG <> @P02 " _
            & "     AND OIT0002.LODDATE = @P03 " _
            & "     AND OIT0002.ORDERSTATUS <> @P04 " _
            & "     GROUP BY " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0002.CONSIGNEECODE" _
            & "     , OIT0002.CONSIGNEENAME" _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE"

        '★受注データより油種数を取得（第２荷受人向けの場合）
        SQLStr &=
              "     UNION ALL " _
            & "     SELECT " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0003.SECONDCONSIGNEECODE AS CONSIGNEECODE" _
            & "     , OIT0003.SECONDCONSIGNEENAME AS CONSIGNEENAME" _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & "     , COUNT(1) AS CNT " _
            & "     FROM oil.OIT0002_ORDER OIT0002 " _
            & "     INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "         OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "     AND OIT0003.SECONDCONSIGNEECODE <> '' " _
            & "     AND OIT0003.DELFLG <> @P02 " _
            & "     WHERE " _
            & "         OIT0002.OFFICECODE = @P01 " _
            & "     AND OIT0002.DELFLG <> @P02 " _
            & "     AND OIT0002.LODDATE = @P03 " _
            & "     AND OIT0002.ORDERSTATUS <> @P04 " _
            & "     GROUP BY " _
            & "       OIT0002.OFFICECODE " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.STACKINGFLG " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.OTTRANSPORTFLG " _
            & "     , OIT0003.SECONDCONSIGNEECODE" _
            & "     , OIT0003.SECONDCONSIGNEENAME" _
            & "     , OIT0002.LODDATE" _
            & "     , OIT0002.DEPDATE" _
            & "     , OIT0002.ARRDATE" _
            & "     , OIT0002.ACCDATE" _
            & " ) OIT0002 " _
            & " ON  OIT0002.TRAINNO      = OIM0029.KEYCODE03 " _
            & " AND OIT0002.TRAINNAME    = OIM0029.VALUE03 " _
            & " AND OIT0002.SHIPPERSCODE = OIM0029.KEYCODE05 " _
            & " AND OIT0002.CONSIGNEECODE = OIM0029.KEYCODE04 "

        ''★品種出荷期限マスタより３号軽油(寒冷軽油)出荷期間を取得
        'SQLStr &=
        '      " LEFT JOIN oil.OIM0030_OILTERM OIM0030 " _
        '    & " ON  OIM0030.OFFICECODE = OIM0029.KEYCODE01 " _
        '    & " AND OIM0030.CONSIGNEECODE = OIM0029.KEYCODE04 " _
        '    & " AND OIM0030.OILCODE = OIT0002.OILCODE " _
        '    & " AND OIM0030.SEGMENTOILCODE = OIT0002.ORDERINGTYPE " _
        '    & " AND OIM0030.DELFLG <> @P02 "
        ''& " AND OIM0030.OILCODE = '1404' " _
        ''& " AND OIM0030.SEGMENTOILCODE = 'E' " _

        SQLStr &=
              " WHERE OIM0029.CLASS = @P05 " _
            & " AND   OIM0029.KEYCODE01 = @P01 " _
            & " AND   OIM0029.DELFLG <> @P02 " _
            & " ORDER BY" _
            & "    OIM0029.KEYCODE02 "


        '★品種出荷期限マスタより出荷期間を取得
        Dim SQLOilDurationStr As String =
              " SELECT " _
            & "   OIM0030.OFFICECODE     " _
            & " , OIM0030.SHIPPERCODE    " _
            & " , OIM0030.PLANTCODE      " _
            & " , OIM0030.OILCODE        " _
            & " , OIM0030.SEGMENTOILCODE " _
            & " , OIM0030.CONSIGNEECODE  " _
            & " , OIM0030.ORDERFROMDATE  " _
            & " , OIM0030.ORDERTODATE    " _
            & " FROM oil.OIM0030_OILTERM OIM0030" _
            & " WHERE OIM0030.OFFICECODE = @P01 " _
            & " AND OIM0030.DELFLG <> @P02 " _
            & " ORDER BY " _
            & "   OIM0030.CONSIGNEECODE " _
            & " , OIM0030.OILCODE" _
            & " , OIM0030.SEGMENTOILCODE "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLOilDuracmd As New SqlCommand(SQLOilDurationStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '分類
                PARA01.Value = I_OFFICECODE
                'PARA01.Value = BaseDllConst.CONST_OFFICECODE_012402
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA05.Value = "SHIP_PLAN"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportMieShiohamatbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportMieShiohamatbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003ReportMieShiohamatbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT

                Next

                '★品種出荷期限マスタより出荷期間を取得
                Dim PARAOilDure01 As SqlParameter = SQLOilDuracmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARAOilDure02 As SqlParameter = SQLOilDuracmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARAOilDure01.Value = I_OFFICECODE
                'PARAOilDure01.Value = BaseDllConst.CONST_OFFICECODE_012402
                PARAOilDure02.Value = C_DELETE_FLG.DELETE
                Using SQLdr As SqlDataReader = SQLOilDuracmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportOilDuration.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportOilDuration.Load(SQLdr)
                End Using

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LMIESHIOHAMA EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LMIESHIOHAMA EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportMieShiohamatbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(積込指示（三重塩浜）)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelLoadPlanDataGet(ByVal SQLcon As SqlConnection,
                                       ByVal officeCode As String,
                                       Optional ByVal lodDate As String = Nothing)

        If IsNothing(OIT0003Reporttbl) Then
            OIT0003Reporttbl = New DataTable
        End If

        If OIT0003Reporttbl.Columns.Count <> 0 Then
            OIT0003Reporttbl.Columns.Clear()
        End If

        OIT0003Reporttbl.Clear()

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
            & " , OIT0002.OFFICECODE                             AS OFFICECODE" _
            & " , OIT0002.OFFICENAME                             AS OFFICENAME" _
            & " , OIT0003.SHIPPERSCODE                           AS SHIPPERSCODE" _
            & " , OIT0003.SHIPPERSNAME                           AS SHIPPERSNAME" _
            & " , OIT0002.CONSIGNEECODE                          AS CONSIGNEECODE" _
            & " , OIT0002.CONSIGNEENAME                          AS CONSIGNEENAME" _
            & " , OIT0002.TRAINNO                                AS TRAINNO" _
            & " , OIT0002.TRAINNAME                              AS TRAINNAME" _
            & " , OIT0002.ARRSTATION                             AS ARRSTATION" _
            & " , OIT0002.ARRSTATIONNAME                         AS ARRSTATIONNAME" _
            & " , OIT0003.TANKNO                                 AS TANKNO" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , ISNULL(OIM0029.VALUE01,OIT0003.OILNAME)        AS OILNAME" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " LEFT JOIN OIL.OIM0029_CONVERT OIM0029 ON " _
            & "     OIM0029.CLASS = @P05 " _
            & " AND OIM0029.KEYCODE01 = @P01 " _
            & " AND OIM0029.KEYCODE02 = OIT0003.OILCODE " _
            & " AND OIM0029.KEYCODE04 = OIT0003.ORDERINGTYPE " _
            & " AND OIM0029.DELFLG <> @P02 " _
            & " WHERE OIT0002.LODDATE = @P03 " _
            & " AND OIT0002.OFFICECODE = @P01 " _
            & " AND OIT0002.ORDERSTATUS <= @P04 " _
            & " AND OIT0002.DELFLG <> @P02 "

        SQLStr &=
                " ORDER BY" _
            & "    OIT0002.TRAINNO" _
            & "   ,OIT0003.OILCODE" _
            & "   ,OIT0003.ORDERINGTYPE"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '分類
                PARA01.Value = officeCode
                PARA02.Value = C_DELETE_FLG.DELETE
                If Not String.IsNullOrEmpty(lodDate) Then
                    PARA03.Value = lodDate
                Else
                    PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                End If
                'PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_310
                PARA05.Value = "LOADPLAN_OILCODE"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003Reporttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003Reporttbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003Reporttbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003LLOADPLAN EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003LLOADPLAN EXCEL_DATAGET"
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
    ''' 帳票表示(タンク車発送実績)データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ExcelTankDispatchDataGet(ByVal SQLcon As SqlConnection,
                                           ByVal officeCode As String,
                                           ByVal lodDate As String,
                                           ByVal trainNo As String(),
                                           ByVal consigneeCode As String,
                                           ByVal secondConsigneeCode As String)

        If IsNothing(OIT0003Reporttbl) Then
            OIT0003Reporttbl = New DataTable
        End If

        If OIT0003Reporttbl.Columns.Count <> 0 Then
            OIT0003Reporttbl.Columns.Clear()
        End If

        OIT0003Reporttbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "     0                                          AS LINECNT " _
            & "   , ''                                         AS OPERATION " _
            & "   , '0'                                        AS TIMSTP " _
            & "   , 1                                          AS 'SELECT' " _
            & "   , 0                                          AS HIDDEN " _
            & "   , OIT0002.OFFICECODE                         AS OFFICECODE " _
            & "   , OIT0003.ACTUALLODDATE                      AS ACTUALLODDATE " _
            & "   , OIT0002.TRAINNO                            AS TRAINNO " _
            & "   , OIM0007.DEPSTATION                         AS DEPSTATION " _
            & "   , OIM0007.ARRSTATION                         AS ARRSTATION " _
            & "   , ISNULL( " _
            & "     TANKDISPATCH.SHIPPEROILCODE " _
            & "     , ISNULL(OIM0003.SHIPPEROILCODE, '') " _
            & "   )                                            AS OILCODE " _
            & "   , ISNULL(TANKDISPATCH.SHIPPEROILCODESEQ, '') AS OILCODESEQ " _
            & "   , ISNULL(OIT0002.CONSIGNEECODE, '')          AS CONSIGNEECODE " _
            & "   , ISNULL(OIT0003.SECONDCONSIGNEECODE, '')    AS SECONDCONSIGNEECODE " _
            & "   , ISNULL(OIT0003.CARSAMOUNT, '0.000')        AS CARSAMOUNT " _
            & "   , ISNULL(OIM0005.SAPSHELLTANKNUMBER, '')     AS TANKNUMBER " _
            & "   , ISNULL(OIT0003.LOADINGIRILINETRAINNO, '')  AS LOADINGIRILINETRAINNO " _
            & " FROM " _
            & "   OIL.OIT0002_ORDER OIT0002 " _
            & "   INNER JOIN OIL.OIT0003_DETAIL OIT0003 " _
            & "     ON OIT0002.ORDERNO = OIT0003.ORDERNO " _
            & "   INNER JOIN OIL.OIM0007_TRAIN OIM0007 " _
            & "     ON OIT0002.OFFICECODE = OIM0007.OFFICECODE " _
            & "     AND OIT0002.TRAINNO = OIM0007.TRAINNO " _
            & "   INNER JOIN OIL.OIM0005_TANK OIM0005 " _
            & "     ON OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & "   INNER JOIN ( " _
            & "     SELECT " _
            & "         OIM0003.OFFICECODE                           AS OFFICECODE " _
            & "       , OIM0003.SHIPPERCODE                          AS SHIPPERCODE " _
            & "       , OIM0003.PLANTCODE                            AS PLANTCODE " _
            & "       , OIM0003.OILCODE                              AS OILCODE " _
            & "       , OIM0003.SEGMENTOILCODE                       AS SEGMENTOILCODE " _
            & "       , " _
            & "       LEFT (OIM0003.SHIPPEROILCODE + '000000000', 9) AS SHIPPEROILCODE " _
            & "       , OIM0003.SHIPPEROILNAME                       AS SHIPPEROILNAME " _
            & "     FROM " _
            & "       OIL.OIM0003_PRODUCT OIM0003 " _
            & "     WHERE " _
            & "       OIM0003.DELFLG <> @DELFLG " _
            & "   ) OIM0003 " _
            & "     ON OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & "     AND OIM0003.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
            & "     AND OIM0003.PLANTCODE = OIT0002.BASECODE " _
            & "     AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & "     AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & "   LEFT JOIN ( " _
            & "     SELECT " _
            & "         OIM0029.KEYCODE01 AS OFFICECODE " _
            & "       , OIM0029.KEYCODE02 AS OILCODE " _
            & "       , OIM0029.KEYCODE03 AS SEGMENTOILCODE " _
            & "       , OIM0029.VALUE01   AS SHIPPEROILCODE " _
            & "       , OIM0029.VALUE02   AS SHIPPEROILNAME " _
            & "       , OIM0029.VALUE03   AS SHIPPEROILCODESEQ " _
            & "     FROM " _
            & "       OIL.OIM0029_CONVERT OIM0029 " _
            & "     WHERE " _
            & "       OIM0029.CLASS = 'TANKDISPATCH_OILCODE' " _
            & "       AND OIM0029.DELFLG <> @DELFLG " _
            & "   ) TANKDISPATCH " _
            & "     ON TANKDISPATCH.OFFICECODE = OIM0003.OFFICECODE " _
            & "     AND TANKDISPATCH.OILCODE = OIM0003.OILCODE " _
            & "     AND TANKDISPATCH.SEGMENTOILCODE = OIM0003.SEGMENTOILCODE " _
            & " WHERE " _
            & "   OIT0002.OFFICECODE = @OFFICECODE " _
            & "   AND OIT0003.SHIPPERSCODE = @SHIPPERSCODE " _
            & "   AND OIT0002.ORDERSTATUS <> @ORDERSTATUS " _
            & "   AND OIT0002.DELFLG <> @DELFLG " _
            & "   AND OIT0002.LODDATE = @LODDATE " _
            & "   AND OIT0003.DELFLG <> @DELFLG " _
            & "   AND OIM0005.DELFLG <> @DELFLG "

        SQLStr &= String.Format("   AND OIT0002.TRAINNO IN ('{0}') ", String.Join("','", trainNo))

        If Not String.IsNullOrWhiteSpace(consigneeCode) Then
            SQLStr &= "   AND OIT0002.CONSIGNEECODE = @CONSIGNEECODE "
        End If

        If Not String.IsNullOrWhiteSpace(secondConsigneeCode) Then
            SQLStr &= "   AND OIT0003.SECONDCONSIGNEECODE = @SECONDCONSIGNEECODE "
        End If

        SQLStr &= " ORDER BY " _
            & "   OIT0002.OFFICECODE " _
            & "   , OIT0002.ACTUALLODDATE " _
            & "   , TANKDISPATCH.SHIPPEROILCODESEQ " _
            & "   , OIM0003.SHIPPEROILCODE " _
            & "   , OIM0005.MODEL " _
            & "   , OIM0005.TANKNUMBER " _
            & "   , OIT0003.CARSAMOUNT "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 20)     '受注営業所コード
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.Date)                   '積込日
                'Dim P_TRAINNO As SqlParameter = SQLcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar, 4)            '列車番号
                Dim P_SHIPPERSCODE As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSCODE", SqlDbType.NVarChar, 10) '荷主コード
                Dim P_ORDERSTATUS As SqlParameter = SQLcmd.Parameters.Add("@ORDERSTATUS", SqlDbType.NVarChar, 3)    '受注進行ステータス
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)              '削除フラグ
                P_OFFICECODE.Value = officeCode
                P_LODDATE.Value = lodDate
                'P_TRAINNO.Value = trainNo
                P_SHIPPERSCODE.Value = BaseDllConst.CONST_SHIPPERCODE_0122700010
                'P_SHIPPERSCODE.Value = CONST_SHIPPERSCODE_0122700010
                P_ORDERSTATUS.Value = BaseDllConst.CONST_ORDERSTATUS_900
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                If Not String.IsNullOrWhiteSpace(consigneeCode) Then
                    Dim P_CONSIGNEECODE As SqlParameter = SQLcmd.Parameters.Add("@CONSIGNEECODE", SqlDbType.NVarChar, 10)               '荷受人コード
                    P_CONSIGNEECODE.Value = consigneeCode
                End If

                If Not String.IsNullOrWhiteSpace(secondConsigneeCode) Then
                    Dim P_SECONDCONSIGNEECODE As SqlParameter = SQLcmd.Parameters.Add("@SECONDCONSIGNEECODE", SqlDbType.NVarChar, 10)   '第二荷受人コード
                    P_SECONDCONSIGNEECODE.Value = secondConsigneeCode
                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003Reporttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003Reporttbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003Reporttbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L EXCEL_TANKDISPATCHDATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L EXCEL_TANKDISPATCHDATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportSendaitbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(出荷実績(共通))データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ExcelActualShipDataGet(ByVal SQLcon As SqlConnection,
                                         ByVal officeCode As String,
                                         ByVal lodDate As String,
                                         ByVal trainNo As String,
                                         ByRef trainNoCvt As String)

        If IsNothing(OIT0003Reporttbl) Then
            OIT0003Reporttbl = New DataTable
        End If

        If OIT0003Reporttbl.Columns.Count <> 0 Then
            OIT0003Reporttbl.Columns.Clear()
        End If

        OIT0003Reporttbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "     0                                                   AS LINECNT " _
            & "   , ''                                                  AS OPERATION " _
            & "   , '0'                                                 AS TIMSTP " _
            & "   , 1                                                   AS 'SELECT' " _
            & "   , 0                                                   AS HIDDEN " _
            & "   , OIT0002.OFFICECODE                                  AS OFFICECODE " _
            & "   , OIT0003.ACTUALLODDATE                               AS ACTUALLODDATE " _
            & "   , CASE OIT0002.TRAINNO " _
            & "     WHEN TRACNV.KEYCODE02 THEN TRACNV.VALUE01 " _
            & "     ELSE OIT0002.TRAINNO " _
            & "     END                                                 AS TRAINNO " _
            & "   , OIM0007.DEPSTATION                                  AS DEPSTATION " _
            & "   , OIM0007.ARRSTATION                                  AS ARRSTATION " _
            & "   , OIT0003.OILCODE                                     AS OILCODE " _
            & "   , OIT0003.ORDERINGTYPE                                AS ORDERINGTYPE " _
            & "   , FORMAT(ISNULL(CARSAMOUNT * 1000, '00000'), '00000') AS CARSAMOUNT " _
            & "   , ISNULL(OIM0005.MODEL, '')                           AS MODEL " _
            & "   , ISNULL(OIM0005.TANKNUMBER, '')                      AS TANKNUMBER " _
            & "   , ''                                                  AS TANKNO " _
            & "   , OIT0002.LODDATE                                     AS LODDATE" _
            & "   , OIT0002.DEPDATE                                     AS DEPDATE" _
            & " FROM " _
            & "   OIL.OIT0002_ORDER OIT0002 " _
            & "   INNER JOIN OIL.OIT0003_DETAIL OIT0003 " _
            & "     ON OIT0002.ORDERNO = OIT0003.ORDERNO " _
            & "   INNER JOIN OIL.OIM0007_TRAIN OIM0007 " _
            & "     ON OIT0002.OFFICECODE = OIM0007.OFFICECODE " _
            & "     AND OIT0002.TRAINNO = OIM0007.TRAINNO " _
            & "   INNER JOIN OIL.OIM0005_TANK OIM0005 " _
            & "     ON OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & "   INNER JOIN ( " _
            & "     SELECT " _
            & "         OIM0003.OFFICECODE                           AS OFFICECODE " _
            & "       , OIM0003.SHIPPERCODE                          AS SHIPPERCODE " _
            & "       , OIM0003.PLANTCODE                            AS PLANTCODE " _
            & "       , OIM0003.OILCODE                              AS OILCODE " _
            & "       , OIM0003.SEGMENTOILCODE                       AS SEGMENTOILCODE " _
            & "       , " _
            & "       LEFT (OIM0003.SHIPPEROILCODE + '000000000', 9) AS SHIPPEROILCODE " _
            & "       , OIM0003.SHIPPEROILNAME                       AS SHIPPEROILNAME " _
            & "     FROM " _
            & "       OIL.OIM0003_PRODUCT OIM0003 " _
            & "     WHERE " _
            & "       OIM0003.DELFLG <> @DELFLG " _
            & "   ) OIM0003 " _
            & "     ON OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & "     AND OIM0003.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
            & "     AND OIM0003.PLANTCODE = OIT0002.BASECODE " _
            & "     AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & "     AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & "   LEFT JOIN ( " _
            & "     SELECT " _
            & "         OIS0015.KEYCODE AS SHIPPEROILCODE " _
            & "       , OIS0015.VALUE1  AS SHIPPEROILNAME " _
            & "       , OIS0015.VALUE2  AS SHIPPEROILCODESEQ " _
            & "     FROM " _
            & "       COM.OIS0015_FIXVALUE OIS0015 " _
            & "     WHERE " _
            & "       OIS0015.CAMPCODE = @OFFICECODE " _
            & "       AND OIS0015.CLASS = 'ACTUALSHIP_OILCODE' " _
            & "       AND OIS0015.DELFLG <> @DELFLG " _
            & "   ) ACTUALSHIP " _
            & "     ON OIM0003.SHIPPEROILCODE = ACTUALSHIP.SHIPPEROILCODE " _
            & "   LEFT JOIN OIL.OIM0029_CONVERT TRACNV " _
            & "     ON TRACNV.CLASS = 'ORDERREPORTTRAINNO' " _
            & "     AND TRACNV.DELFLG <> '1' " _
            & "     AND OIT0002.OFFICECODE = TRACNV.KEYCODE01 " _
            & "     AND OIT0002.TRAINNO = TRACNV.KEYCODE02 " _
            & " WHERE " _
            & "   OIT0002.OFFICECODE = @OFFICECODE " _
            & "   AND CASE OIT0002.TRAINNO " _
            & "     WHEN TRACNV.KEYCODE02 THEN TRACNV.VALUE01 " _
            & "     ELSE OIT0002.TRAINNO " _
            & "     END = @TRAINNO " _
            & "   AND OIT0003.SHIPPERSCODE = @SHIPPERSCODE " _
            & "   AND OIT0002.ORDERSTATUS <> @ORDERSTATUS " _
            & "   AND OIT0002.DELFLG <> @DELFLG " _
            & "   AND OIT0002.LODDATE = @LODDATE " _
            & "   AND OIT0003.DELFLG <> @DELFLG " _
            & "   AND OIM0005.DELFLG <> @DELFLG " _
            & "   AND ( " _
            & "     ( " _
            & "       ( " _
            & "         OIT0002.CONSIGNEECODE = '40' " _
            & "         AND ISNULL(OIT0003.SECONDCONSIGNEECODE, '') = '' " _
            & "       ) " _
            & "       OR OIT0003.SECONDCONSIGNEECODE = '40' " _
            & "     ) OR (" _
            & "       ( " _
            & "         OIT0002.CONSIGNEECODE = '70' " _
            & "         AND ISNULL(OIT0003.SECONDCONSIGNEECODE, '') = '' " _
            & "       ) " _
            & "       OR OIT0003.SECONDCONSIGNEECODE = '70' " _
            & "     )" _
            & "   ) " _
            & " ORDER BY " _
            & "   OIT0002.OFFICECODE " _
            & "   , OIT0002.ACTUALLODDATE " _
            & "   , CASE OIT0002.TRAINNO " _
            & "     WHEN TRACNV.KEYCODE02 THEN TRACNV.VALUE01 " _
            & "     ELSE OIT0002.TRAINNO " _
            & "     END " _
            & "   , ACTUALSHIP.SHIPPEROILCODESEQ "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 20)     '受注営業所コード
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.Date)                   '積込日
                Dim P_TRAINNO As SqlParameter = SQLcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar, 4)            '列車番号
                Dim P_SHIPPERSCODE As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSCODE", SqlDbType.NVarChar, 10) '荷主コード
                Dim P_ORDERSTATUS As SqlParameter = SQLcmd.Parameters.Add("@ORDERSTATUS", SqlDbType.NVarChar, 3)    '受注進行ステータス
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)              '削除フラグ
                P_OFFICECODE.Value = officeCode
                P_LODDATE.Value = lodDate
                P_TRAINNO.Value = trainNo
                P_SHIPPERSCODE.Value = BaseDllConst.CONST_SHIPPERCODE_0122700010
                'P_SHIPPERSCODE.Value = CONST_SHIPPERSCODE_0122700010
                P_ORDERSTATUS.Value = BaseDllConst.CONST_ORDERSTATUS_900
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003Reporttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003Reporttbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003Reporttbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT

                    Dim tankNoMaxDigits As Integer = 7

                    Dim tankNo As Decimal = 0
                    Dim tankNumber As Decimal = 0
                    Dim modelL As String = ""
                    Dim modelR As Decimal = 0
                    'JOT車番
                    tankNumber = Decimal.Parse(OIT0003Reprow("TANKNUMBER").ToString())
                    '形式
                    Dim re As New Regex("^(?<modelL>\w*?)(?<modelR>\d*?)$")
                    Dim m As Match = re.Match(OIT0003Reprow("MODEL").ToString())
                    While m.Success
                        modelL = StrConv(m.Groups("modelL").Value, VbStrConv.Wide)
                        modelR = Decimal.Parse(m.Groups("modelR").Value)
                        m = m.NextMatch()
                    End While

                    Select Case modelR
                        Case 1000
                            Dim modelRMaxDigits As Integer = modelR.ToString().Length
                            Dim tankNoDigits As Integer = tankNumber.ToString().Length

                            If tankNoDigits >= 1 AndAlso tankNoDigits < modelRMaxDigits Then
                                tankNo = modelR * CDec(10 ^ tankNoDigits) + tankNumber
                            ElseIf tankNoDigits >= modelRMaxDigits AndAlso tankNoDigits < tankNoMaxDigits Then
                                tankNo = modelR * CDec(10 ^ (tankNoMaxDigits - modelRMaxDigits)) + tankNumber
                            End If
                        Case 43000, 243000
                            tankNo = tankNumber
                    End Select

                    OIT0003Reprow("TANKNO") = tankNo.ToString().PadLeft(7, "0"c)

                    '★営業所別列車No変換処理
                    WW_CvtOfficeTrainNo(officeCode, OIT0003Reprow, trainNoCvt)

                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L EXCEL_ACTUALSHIPDATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L EXCEL_ACTUALSHIPDATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportSendaitbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(連結順序表(共通))データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ExcelConcatOrderDataGet(ByVal SQLcon As SqlConnection,
                                          ByVal officeCode As String,
                                          ByVal lodDate As String,
                                          ByVal trainNo As String,
                                          ByRef trainNoCvt As String)

        If IsNothing(OIT0003Reporttbl) Then
            OIT0003Reporttbl = New DataTable
        End If

        If OIT0003Reporttbl.Columns.Count <> 0 Then
            OIT0003Reporttbl.Columns.Clear()
        End If

        OIT0003Reporttbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "     0                              AS LINECNT " _
            & "   , ''                             AS OPERATION " _
            & "   , '0'                            AS TIMSTP " _
            & "   , 1                              AS 'SELECT' " _
            & "   , 0                              AS HIDDEN " _
            & "   , OIT0002.OFFICECODE             AS OFFICECODE " _
            & "   , OIT0003.ACTUALLODDATE          AS ACTUALLODDATE " _
            & "   , CASE OIT0002.TRAINNO " _
            & "     WHEN TRACNV.KEYCODE02 THEN TRACNV.VALUE01 " _
            & "     ELSE OIT0002.TRAINNO " _
            & "     END                            AS TRAINNO " _
            & "   , OIM0007.DEPSTATION             AS DEPSTATION " _
            & "   , OIM0007.ARRSTATION             AS ARRSTATION " _
            & "   , OIT0003.OILCODE                AS OILCODE " _
            & "   , OIT0003.ORDERINGTYPE           AS ORDERINGTYPE " _
            & "   , ISNULL(OIT0003.SHIPORDER, '')  AS SHIPORDER " _
            & "   , ISNULL(OIM0005.MODEL, '')      AS MODEL " _
            & "   , ISNULL(OIM0005.TANKNUMBER, '') AS TANKNUMBER " _
            & "   , ''                             AS TANKNO " _
            & "   , OIT0002.LODDATE                AS LODDATE" _
            & "   , OIT0002.DEPDATE                AS DEPDATE" _
            & " FROM " _
            & "   OIL.OIT0002_ORDER OIT0002 " _
            & "   INNER JOIN OIL.OIT0003_DETAIL OIT0003 " _
            & "     ON OIT0002.ORDERNO = OIT0003.ORDERNO " _
            & "   INNER JOIN OIL.OIM0007_TRAIN OIM0007 " _
            & "     ON OIT0002.OFFICECODE = OIM0007.OFFICECODE " _
            & "     AND OIT0002.TRAINNO = OIM0007.TRAINNO " _
            & "   INNER JOIN OIL.OIM0005_TANK OIM0005 " _
            & "     ON OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & "   INNER JOIN ( " _
            & "     SELECT " _
            & "         OIM0003.OFFICECODE                           AS OFFICECODE " _
            & "       , OIM0003.SHIPPERCODE                          AS SHIPPERCODE " _
            & "       , OIM0003.PLANTCODE                            AS PLANTCODE " _
            & "       , OIM0003.OILCODE                              AS OILCODE " _
            & "       , OIM0003.SEGMENTOILCODE                       AS SEGMENTOILCODE " _
            & "       , " _
            & "       LEFT (OIM0003.SHIPPEROILCODE + '000000000', 9) AS SHIPPEROILCODE " _
            & "       , OIM0003.SHIPPEROILNAME                       AS SHIPPEROILNAME " _
            & "     FROM " _
            & "       OIL.OIM0003_PRODUCT OIM0003 " _
            & "     WHERE " _
            & "       OIM0003.DELFLG <> @DELFLG " _
            & "   ) OIM0003 " _
            & "     ON OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & "     AND OIM0003.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
            & "     AND OIM0003.PLANTCODE = OIT0002.BASECODE " _
            & "     AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & "     AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & "   LEFT JOIN ( " _
            & "     SELECT " _
            & "         OIS0015.KEYCODE AS SHIPPEROILCODE " _
            & "       , OIS0015.VALUE1  AS SHIPPEROILNAME " _
            & "       , OIS0015.VALUE2  AS SHIPPEROILCODESEQ " _
            & "     FROM " _
            & "       COM.OIS0015_FIXVALUE OIS0015 " _
            & "     WHERE " _
            & "       OIS0015.CAMPCODE = @OFFICECODE " _
            & "       AND OIS0015.CLASS = 'ACTUALSHIP_OILCODE' " _
            & "       AND OIS0015.DELFLG <> @DELFLG " _
            & "   ) ACTUALSHIP " _
            & "     ON OIM0003.SHIPPEROILCODE = ACTUALSHIP.SHIPPEROILCODE " _
            & "   LEFT JOIN OIL.OIM0029_CONVERT TRACNV " _
            & "     ON TRACNV.CLASS = 'ORDERREPORTTRAINNO' " _
            & "     AND TRACNV.DELFLG <> '1' " _
            & "     AND OIT0002.OFFICECODE = TRACNV.KEYCODE01 " _
            & "     AND OIT0002.TRAINNO = TRACNV.KEYCODE02 " _
            & " WHERE " _
            & "   OIT0002.OFFICECODE = @OFFICECODE " _
            & "   AND CASE OIT0002.TRAINNO " _
            & "     WHEN TRACNV.KEYCODE02 THEN TRACNV.VALUE01 " _
            & "     ELSE OIT0002.TRAINNO " _
            & "     END = @TRAINNO " _
            & "   AND OIT0003.SHIPPERSCODE = @SHIPPERSCODE " _
            & "   AND OIT0002.ORDERSTATUS <> @ORDERSTATUS " _
            & "   AND OIT0002.DELFLG <> @DELFLG " _
            & "   AND OIT0002.LODDATE = @LODDATE " _
            & "   AND OIT0003.DELFLG <> @DELFLG " _
            & "   AND OIM0005.DELFLG <> @DELFLG " _
            & " ORDER BY " _
            & "   OIT0002.OFFICECODE " _
            & "   , OIT0002.ACTUALLODDATE " _
            & "   , CASE OIT0002.TRAINNO " _
            & "     WHEN TRACNV.KEYCODE02 THEN TRACNV.VALUE01 " _
            & "     ELSE OIT0002.TRAINNO " _
            & "     END " _
            & "   , CONVERT(INT, ISNULL(OIT0003.SHIPORDER, 99)) " _
            & "   , OIM0005.MODEL " _
            & "   , OIM0005.TANKNUMBER "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 20)     '受注営業所コード
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.Date)                   '積込日
                Dim P_TRAINNO As SqlParameter = SQLcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar, 4)            '列車番号
                Dim P_SHIPPERSCODE As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSCODE", SqlDbType.NVarChar, 10) '荷主コード
                Dim P_ORDERSTATUS As SqlParameter = SQLcmd.Parameters.Add("@ORDERSTATUS", SqlDbType.NVarChar, 3)    '受注進行ステータス
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)              '削除フラグ
                P_OFFICECODE.Value = officeCode
                P_LODDATE.Value = lodDate
                P_TRAINNO.Value = trainNo
                P_SHIPPERSCODE.Value = BaseDllConst.CONST_SHIPPERCODE_0122700010
                'P_SHIPPERSCODE.Value = CONST_SHIPPERSCODE_0122700010
                P_ORDERSTATUS.Value = BaseDllConst.CONST_ORDERSTATUS_900
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003Reporttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003Reporttbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003Reporttbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT

                    Dim tankNoMaxDigits As Integer = 7

                    Dim tankNo As Decimal = 0
                    Dim tankNumber As Decimal = 0
                    Dim modelL As String = ""
                    Dim modelR As Decimal = 0
                    'JOT車番
                    tankNumber = Decimal.Parse(OIT0003Reprow("TANKNUMBER").ToString())
                    '形式
                    Dim re As New Regex("^(?<modelL>\w*?)(?<modelR>\d*?)$")
                    Dim m As Match = re.Match(OIT0003Reprow("MODEL").ToString())
                    While m.Success
                        modelL = StrConv(m.Groups("modelL").Value, VbStrConv.Wide)
                        modelR = Decimal.Parse(m.Groups("modelR").Value)
                        m = m.NextMatch()
                    End While

                    Select Case modelR
                        Case 1000
                            Dim modelRMaxDigits As Integer = modelR.ToString().Length
                            Dim tankNoDigits As Integer = tankNumber.ToString().Length

                            If tankNoDigits >= 1 AndAlso tankNoDigits < modelRMaxDigits Then
                                tankNo = modelR * CDec(10 ^ tankNoDigits) + tankNumber
                            ElseIf tankNoDigits >= modelRMaxDigits AndAlso tankNoDigits < tankNoMaxDigits Then
                                tankNo = modelR * CDec(10 ^ (tankNoMaxDigits - modelRMaxDigits)) + tankNumber
                            End If
                        Case 43000, 243000
                            tankNo = tankNumber
                    End Select

                    OIT0003Reprow("TANKNO") = tankNo.ToString().PadLeft(7, "0"c)

                    '★営業所別列車No変換処理
                    WW_CvtOfficeTrainNo(officeCode, OIT0003Reprow, trainNoCvt)

                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L EXCEL_CONTACTORDERDATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L EXCEL_CONTACTORDERDATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportSendaitbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 帳票表示(タンク車連出荷連絡書)データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ExcelShipContactDataGet(ByVal SQLcon As SqlConnection,
                                       ByVal officeCode As String,
                                       ByVal depDate As String,
                                       ByVal trainNo As String)

        If IsNothing(OIT0003Reporttbl) Then
            OIT0003Reporttbl = New DataTable
        End If

        If OIT0003Reporttbl.Columns.Count <> 0 Then
            OIT0003Reporttbl.Columns.Clear()
        End If

        OIT0003Reporttbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
              " SELECT " _
            & "       0                                          AS LINECNT " _
            & "     , ''                                         AS OPERATION " _
            & "     , '0'                                        AS TIMSTP " _
            & "     , 1                                          AS 'SELECT' " _
            & "     , 0                                          AS HIDDEN " _
            & "     , OIT0002.OFFICECODE                         AS OFFICECODE " _
            & "     , OIT0002.DEPDATE                            AS DEPDATE " _
            & "     , OIT0002.TRAINNO                            AS TRAINNO " _
            & "     , ISNULL(OIM0007.JRTRAINNO3, '')             AS JRTRAINNO3 " _
            & "     , ISNULL(OIM0029.VALUE01, '')                AS TRAINNO_TUMIOKI " _
            & "     , CASE WHEN OIT0002.DEPDATE = OIT0002.LODDATE " _
            & "            THEN '0' " _
            & "            ELSE '1' " _
            & "       END                                        as TUMIOKIFLG " _
            & "     , OIT0002.CONSIGNEECODE                      AS CONSIGNEECODE " _
            & "     , OIT0002.CONSIGNEENAME                      AS CONSIGNEENAME " _
            & "     , OIT0003.TANKNO                             AS TANKNO " _
            & "     , ISNULL(OIM0005.MODEL, '')                  AS MODEL " _
            & "     , OIT0003.OILCODE                            AS OILCODE " _
            & "     , OIT0003.ORDERINGTYPE                       AS ORDERINGTYPE " _
            & "     , ISNULL(OIM0003.REPORTOILNAME, '')          AS REPORTOILNAME " _
            & "     , OIT0003.CARSAMOUNT                         AS CARSAMOUNT " _
            & "     , OIM0005.JRINSPECTIONDATE                   AS JRINSPECTIONDATE " _
            & " FROM " _
            & "     OIL.OIT0002_ORDER OIT0002 " _
            & "     INNER JOIN OIL.OIT0003_DETAIL OIT0003 " _
            & "         ON OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "        AND OIT0003.DELFLG <> @DELFLG " _
            & "     INNER JOIN OIL.OIM0007_TRAIN OIM0007 " _
            & "         ON OIM0007.OFFICECODE = OIT0002.OFFICECODE " _
            & "        AND OIM0007.TRAINNO = OIT0002.TRAINNO " _
            & "        AND OIM0007.ARRSTATION = OIT0002.ARRSTATION " _
            & "        AND OIM0007.DELFLG <> @DELFLG " _
            & "     INNER JOIN OIL.OIM0005_TANK OIM0005 " _
            & "         ON OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & "        AND OIM0005.DELFLG <> @DELFLG " _
            & "     INNER JOIN OIL.OIM0003_PRODUCT OIM0003 " _
            & "         ON OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & "         AND OIM0003.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
            & "         AND OIM0003.PLANTCODE = OIT0002.BASECODE " _
            & "         AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & "         AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & "         AND OIM0003.DELFLG <> @DELFLG " _
            & "     LEFT JOIN OIL.OIM0029_CONVERT OIM0029 " _
            & "         ON OIM0029.CLASS = 'SHIPCONTACT_TRAINNO' " _
            & "        AND OIM0029.KEYCODE01 = OIT0002.OFFICECODE " _
            & "        AND OIM0029.KEYCODE02 = OIT0002.TRAINNO " _
            & "        AND OIM0029.DELFLG <> @DELFLG " _
            & " WHERE " _
            & "     OIT0002.OFFICECODE = @OFFICECODE " _
            & "     AND OIT0002.TRAINNO = @TRAINNO " _
            & "     AND OIT0002.ARRSTATION = '5141' " _
            & "     AND OIT0002.ORDERSTATUS <> @ORDERSTATUS " _
            & "     AND OIT0002.DEPDATE = @DEPDATE " _
            & "     AND OIT0002.DELFLG <> @DELFLG " _
            & " ORDER BY " _
            & "     OIT0002.OFFICECODE " _
            & "     , OIT0003.DETAILNO " _
            & "     , OIT0003.TANKNO "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 20)     '受注営業所コード
                Dim P_DEPDATE As SqlParameter = SQLcmd.Parameters.Add("@DEPDATE", SqlDbType.Date)                   '発日（予定）
                Dim P_TRAINNO As SqlParameter = SQLcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar, 4)            '列車番号
                Dim P_ORDERSTATUS As SqlParameter = SQLcmd.Parameters.Add("@ORDERSTATUS", SqlDbType.NVarChar, 3)    '受注進行ステータス
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)              '削除フラグ
                P_OFFICECODE.Value = officeCode
                P_DEPDATE.Value = depDate
                P_TRAINNO.Value = trainNo
                P_ORDERSTATUS.Value = BaseDllConst.CONST_ORDERSTATUS_900
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003Reporttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003Reporttbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Reprow As DataRow In OIT0003Reporttbl.Rows
                    i += 1
                    OIT0003Reprow("LINECNT") = i        'LINECNT
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L EXCEL_TANKSHIPDATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L EXCEL_TANKSHIPDATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003ReportSendaitbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


#End Region

#End Region

    ''' <summary>
    ''' 受注履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon">SQL接続</param>
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
    ''' (貨車連結順序表TBL)の内容を更新
    ''' </summary>
    ''' <param name="I_LINKNO">貨車連結順序表№</param>
    ''' <param name="I_STATUS">利用可否(1:利用可, 2:利用不可)</param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateLink(ByVal I_LINKNO As String,
                                ByVal I_STATUS As String,
                                Optional ByVal I_LINKDETAILNO As String = Nothing,
                                Optional ByVal I_TANKNO As String = Nothing)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･貨車連結順序表TBLのステータスを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0004_LINK " _
                    & "    SET "

            '○ 更新内容が指定されていれば追加する
            'ステータス
            If Not String.IsNullOrEmpty(I_STATUS) Then
                SQLStr &= String.Format("        STATUS = '{0}', ", I_STATUS)
            End If

            SQLStr &=
                      "        UPDYMD       = @P11, " _
                    & "        UPDUSER      = @P12, " _
                    & "        UPDTERMID    = @P13, " _
                    & "        RECEIVEYMD   = @P14  " _
                    & "  WHERE LINKNO       = @P01  "

            '○ 追加条件が指定されていれば追加する
            '貨車連結順序表明細№
            If Not String.IsNullOrEmpty(I_LINKDETAILNO) Then
                SQLStr &= String.Format("    AND LINKDETAILNO = '{0}', ", I_LINKDETAILNO)
            End If

            'タンク車№
            If Not String.IsNullOrEmpty(I_TANKNO) Then
                SQLStr &= String.Format("    AND TANKNUMBER   = '{0}', ", I_TANKNO)
            End If

            SQLStr &= "    AND DELFLG      <> @P02; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  '貨車連結順序表№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '削除フラグ

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA01.Value = I_LINKNO
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_LINK UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_LINK UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' (タンク車所在TBL)所在地の内容を更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateTankShozai(ByVal I_LOCATION As String,
                                      ByVal I_STATUS As String,
                                      ByVal I_KBN As String,
                                      Optional ByVal I_SITUATION As String = Nothing,
                                      Optional ByVal I_TANKNO As String = Nothing,
                                      Optional ByVal I_ORDERNO As String = Nothing,
                                      Optional ByVal upEmparrDate As Boolean = False,
                                      Optional ByVal I_EmparrDate As String = Nothing,
                                      Optional ByVal upActualEmparrDate As Boolean = False,
                                      Optional ByVal I_ActualEmparrDate As String = Nothing)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLの託送指示フラグを更新
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
            '空車着日（予定）
            If upEmparrDate = True Then
                SQLStr &= String.Format("        EMPARRDATE   = '{0}', ", I_EmparrDate)
                SQLStr &= String.Format("        ACTUALEMPARRDATE   = {0}, ", "NULL")
            End If
            '空車着日（実績）
            If upActualEmparrDate = True Then
                'SQLStr &= String.Format("        ACTUALEMPARRDATE   = '{0}', ", I_ActualEmparrDate)
                If I_ActualEmparrDate = "" Then
                    SQLStr &= "        ACTUALEMPARRDATE   = NULL, "
                Else
                    SQLStr &= String.Format("        ACTUALEMPARRDATE   = '{0}', ", I_ActualEmparrDate)
                End If

                '### 20200618 START 受注での使用をリセットする対応 #########################################
                SQLStr &= String.Format("        USEORDERNO         = '{0}', ", "")
                '### 20200618 END   受注での使用をリセットする対応 #########################################
            End If

            SQLStr &=
                      "        UPDYMD       = @P11, " _
                    & "        UPDUSER      = @P12, " _
                    & "        UPDTERMID    = @P13, " _
                    & "        RECEIVEYMD   = @P14  " _
                    & "  WHERE TANKNUMBER   = @P01  " _
                    & "    AND DELFLG      <> @P02  "

            '### 20200618 START 受注での使用をリセットする対応 #########################################
            If I_ORDERNO <> "" Then
                SQLStr &=
                      "    AND (ISNULL(USEORDERNO, '')     = '' "
                SQLStr &= String.Format(" OR USEORDERNO = '{0}') ", I_ORDERNO)
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

            If String.IsNullOrEmpty(I_TANKNO) Then
                ''(一覧)で設定しているタンク車をKEYに更新
                'For Each OIT0003row As DataRow In OIT0003tbl.Rows
                '    PARA01.Value = OIT0003row("TANKNO")
                '    SQLcmd.ExecuteNonQuery()
                'Next
            Else
                '指定されたタンク車№をKEYに更新
                PARA01.Value = I_TANKNO
                SQLcmd.ExecuteNonQuery()

            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L_TANKSHOZAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L_TANKSHOZAI UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' 営業所別列車No変換処理
    ''' </summary>
    Protected Sub WW_CvtOfficeTrainNo(ByVal officeCode As String,
                                      ByVal OIT0003Reprow As DataRow,
                                      ByRef trainNoCvt As String)
        If officeCode = BaseDllConst.CONST_OFFICECODE_011203 Then
            Select Case OIT0003Reprow("TRAINNO")
                Case "9672"
                    OIT0003Reprow("TRAINNO") = "5461"

                    '2021/05/06 s.igusa ADD ST
                Case "5972"
                    OIT0003Reprow("TRAINNO") = "5461"
                    '2021/05/06 s.igusa ADD ED
            End Select
        ElseIf officeCode = BaseDllConst.CONST_OFFICECODE_012402 Then
            Select Case OIT0003Reprow("TRAINNO")
                Case "5282"
                    If OIT0003Reprow("LODDATE") = OIT0003Reprow("DEPDATE") Then
                        OIT0003Reprow("TRAINNO") = "5875"
                    Else
                        OIT0003Reprow("TRAINNO") = "6883"
                    End If
                Case "8072"
                    If OIT0003Reprow("LODDATE") = OIT0003Reprow("DEPDATE") Then
                        OIT0003Reprow("TRAINNO") = "8081"
                    Else
                        OIT0003Reprow("TRAINNO") = "9081"
                    End If
            End Select
        End If
        trainNoCvt = OIT0003Reprow("TRAINNO")
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
            SqlConnection.ClearPool(SQLcon)

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
                        OIT0003Fixvaltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003Fixvaltbl.Load(SQLdr)
                End Using

                If I_KEYCODE.Equals("") Then
                    'Dim i As Integer = 0 '2020/3/23 三宅 Delete
                    For Each OIT0003WKrow As DataRow In OIT0003Fixvaltbl.Rows '(全抽出結果回るので要検討
                        'O_VALUE(i) = OIT0003WKrow("KEYCODE") 2020/3/23 三宅 全部KEYCODE(列車NO)が格納されてしまうので修正しました（問題なければこのコメント消してください)
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0003WKrow("VALUE" & i.ToString())
                        Next
                        'i += 1 '2020/3/23 三宅 Delete
                    Next
                Else
                    For Each OIT0003WKrow As DataRow In OIT0003Fixvaltbl.Rows
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0003WKrow("VALUE" & i.ToString())
                        Next
                    Next
                End If

                'CLOSE
                SQLcmd.Dispose()

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
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub
End Class
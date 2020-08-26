'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0001EmptyTurnDairyDetail
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0001tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0001INPtbl As DataTable                              'チェック用テーブル
    Private OIT0001UPDtbl As DataTable                              '更新用テーブル
    Private OIT0001WKtbl As DataTable                               '作業用テーブル
    Private OIT0001WK2tbl As DataTable                              '作業用テーブル
    Private OIT0001WK3tbl As DataTable                              '作業用テーブル(同一列車(同一発日)タンク車チェック用)
    Private OIT0001WK4tbl As DataTable                              '作業用テーブル(異なる列車(同一発日)タンク車チェック用)
    Private OIT0001WK5tbl As DataTable                              '作業用テーブル(異なる列車(同一積込日)タンク車チェック用)
    Private OIT0001WK6tbl As DataTable                              '作業用テーブル(他オーダー情報取得(同オーダーの同一積込日取得用))
    Private OIT0001WK7tbl As DataTable                              '作業用テーブル(他オーダー情報取得(同オーダーの同一発日取得用))
    Private OIT0001Fixvaltbl As DataTable                           '作業用テーブル(固定値マスタ取得用)
    Private OIT0001His1tbl As DataTable                             '履歴格納用テーブル
    Private OIT0001His2tbl As DataTable                             '履歴格納用テーブル
    Private OIT0001Reporttbl As DataTable                           '帳票用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 7                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

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

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    'サブ用リターンコード

    Private WW_ORDERINFOFLG_10 As Boolean = False                   '受注情報セット可否(情報(10:積置))
    Private WW_ORDERINFOALERMFLG_80 As Boolean = False              '受注情報セット可否(警告(80:タンク車数オーバー))
    Private WW_ORDERINFOALERMFLG_82 As Boolean = False              '受注情報セット可否(警告(82:検査間近あり))

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0001tbl)
                    If CS0013ProfView.SetDispListTextBoxValues(OIT0001tbl, pnlListArea) Then
                        Master.SaveTable(OIT0001tbl)
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '油種数登録ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_CheckBoxSELECT",
                             "WF_CheckBoxSELECTSTACKING"    'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click(WF_ButtonClick.Value)
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonLINE_LIFTED"     '行削除ボタン押下
                            WF_ButtonLINE_LIFTED_Click()
                        Case "WF_ButtonLINE_ADD"        '行追加ボタン押下
                            WF_ButtonLINE_ADD_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonUPDATE"          '空回日報確定ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
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
        Finally
            '○ 格納Table Close
            If Not IsNothing(OIT0001tbl) Then
                OIT0001tbl.Clear()
                OIT0001tbl.Dispose()
                OIT0001tbl = Nothing
            End If

            If Not IsNothing(OIT0001INPtbl) Then
                OIT0001INPtbl.Clear()
                OIT0001INPtbl.Dispose()
                OIT0001INPtbl = Nothing
            End If

            If Not IsNothing(OIT0001UPDtbl) Then
                OIT0001UPDtbl.Clear()
                OIT0001UPDtbl.Dispose()
                OIT0001UPDtbl = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0001WRKINC.MAPIDD
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
        WF_OrderStatusFLG.Value = "FALSE"
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

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '受注営業所
        'Me.TxtOrderOffice.Text = work.WF_SEL_ORDERSALESOFFICE.Text
        '本線列車
        Me.TxtHeadOfficeTrain.Text = work.WF_SEL_TRAIN.Text
        Me.TxtHeadOfficeTrainName.Text = work.WF_SEL_TRAINNAME.Text
        '発駅
        Me.TxtDepstation.Text = work.WF_SEL_DEPARTURESTATION.Text
        '着駅
        Me.TxtArrstation.Text = work.WF_SEL_ARRIVALSTATION.Text
        '(予定)積込日
        Me.TxtLoadingDate.Text = work.WF_SEL_LOADINGDATE.Text
        '(予定)発日
        Me.TxtDepDate.Text = work.WF_SEL_LOADINGCAR_DEPARTUREDATE.Text
        '(予定)積車着日
        Me.TxtArrDate.Text = work.WF_SEL_LOADINGCAR_ARRIVALDATE.Text
        '(予定)受入日
        Me.TxtAccDate.Text = work.WF_SEL_RECEIPTDATE.Text
        '(予定)空車着日
        Me.TxtEmparrDate.Text = work.WF_SEL_EMPARRDATE.Text

        '合計車数
        Me.TxtTotalTank.Text = work.WF_SEL_TANKCARTOTAL.Text
        '車数（レギュラー）
        Me.TxtRTank.Text = work.WF_SEL_REGULAR_TANKCAR.Text
        '車数（ハイオク）
        Me.TxtHTank.Text = work.WF_SEL_HIGHOCTANE_TANKCAR.Text
        '車数（灯油）
        Me.TxtTTank.Text = work.WF_SEL_KEROSENE_TANKCAR.Text
        '車数（未添加灯油）
        Me.TxtMTTank.Text = work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text
        '車数（軽油）
        Me.TxtKTank.Text = work.WF_SEL_DIESEL_TANKCAR.Text
        '車数（３号軽油）
        Me.TxtK3Tank.Text = work.WF_SEL_NUM3DIESEL_TANKCAR.Text
        '車数（５号軽油）
        Me.TxtK5Tank.Text = work.WF_SEL_NUM5DIESEL_TANKCAR.Text
        '車数（１０号軽油）
        Me.TxtK10Tank.Text = work.WF_SEL_NUM10DIESEL_TANKCAR.Text
        '車数（LSA）
        Me.TxtLTank.Text = work.WF_SEL_LSA_TANKCAR.Text
        '車数（A重油）
        Me.TxtATank.Text = work.WF_SEL_AHEAVY_TANKCAR.Text

        '本線列車・発駅・着駅を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtHeadOfficeTrain.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtDepstation.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtArrstation.Attributes("onkeyPress") = "CheckNum()"
        '車数を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtHTank.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtRTank.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTTank.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtMTTank.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKTank.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtK3Tank.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtK5Tank.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtK10Tank.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtLTank.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtATank.Attributes("onkeyPress") = "CheckNum()"

        '新規作成の場合(油種別タンク車数のテキストボックスの入力を可とする。)
        If work.WF_SEL_CREATEFLG.Text = "1" Then

            '◯ 画面表示設定
            WW_ScreenEnabledSet()

            '新規データの作成については、受注営業所は読取専用とする。
            Me.TxtOrderOffice.ReadOnly = True
        Else

            '既存データの修正については、受注営業所は入力不可とする。
            Me.TxtOrderOffice.Enabled = False
        End If

        '〇営業所配下情報を取得・設定
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", Me.TxtArrstation.Text, WW_GetValue)
        work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
        work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
        work.WF_SEL_BASECODE.Text = WW_GetValue(2)
        work.WF_SEL_BASENAME.Text = WW_GetValue(3)
        work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
        work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)
        work.WF_SEL_PATTERNCODE.Text = WW_GetValue(6)
        work.WF_SEL_PATTERNNAME.Text = WW_GetValue(7)

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", work.WF_SEL_UORG.Text, Me.WF_UORG_TEXT.Text, WW_DUMMY)
        '受注営業所
        CODENAME_get("SALESOFFICE", work.WF_SEL_SALESOFFICECODE.Text, Me.TxtOrderOffice.Text, WW_DUMMY)
        work.WF_SEL_SALESOFFICE.Text = Me.TxtOrderOffice.Text
        '発駅
        CODENAME_get("DEPSTATION", Me.TxtDepstation.Text, Me.LblDepstationName.Text, WW_DUMMY)
        '着駅
        CODENAME_get("ARRSTATION", Me.TxtArrstation.Text, Me.LblArrstationName.Text, WW_DUMMY)

        '### 20200812 START 指摘票対応(No120)全体 ############################################
        '受注進行ステータスが「320：受注確定」以降の場合
        If work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_900 Then

            '本線列車
            Me.TxtHeadOfficeTrain.Enabled = False
            '発駅
            Me.TxtDepstation.Enabled = False
            '着駅
            Me.TxtArrstation.Enabled = False
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
            '受注進行ステータス制御フラグ
            WF_OrderStatusFLG.Value = "TRUE"
        End If
        '### 20200812 END   指摘票対応(No120)全体 ############################################

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, 0)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0001tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.Both).ToString
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○(一覧)テキストボックスの制御(読取専用)
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

        If IsNothing(OIT0001tbl) Then
            OIT0001tbl = New DataTable
        End If

        If OIT0001tbl.Columns.Count <> 0 Then
            OIT0001tbl.Columns.Clear()
        End If

        OIT0001tbl.Clear()

        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

        '○ 取得SQL
        '　検索説明　：　受注№の連番を決める
        Dim SQLStrNum As String =
        " SELECT " _
            & " ISNULL(FORMAT(MAX(SUBSTRING(OIT0002.ORDERNO, 10, 2)) + 1,'00'),'01') AS ORDERNO_NUM" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " WHERE SUBSTRING(OIT0002.ORDERNO, 2, 8) = FORMAT(GETDATE(),'yyyyMMdd')"

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注、受注明細等のマスタから取得する
        Dim SQLStr As String = ""

        '新規登録ボタン押下
        If work.WF_SEL_CREATEFLG.Text = "1" Then

            SQLStr =
              " SELECT TOP (@P0)" _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , ''                                             AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , FORMAT(GETDATE(),'yyyy/MM/dd')                 AS ORDERYMD" _
            & " , @P12                                           AS ORDERTYPE" _
            & " , @P13                                           AS ORDERTYPENAME" _
            & " , ''                                             AS ORDERINFO" _
            & " , ''                                             AS ORDERINFONAME" _
            & " , @P3                                            AS SHIPPERSCODE" _
            & " , @P4                                            AS SHIPPERSNAME" _
            & " , @P5                                            AS BASECODE" _
            & " , @P6                                            AS BASENAME" _
            & " , @P7                                            AS CONSIGNEECODE" _
            & " , @P8                                            AS CONSIGNEENAME" _
            & " , ''                                             AS OILCODE" _
            & " , ''                                             AS OILNAME" _
            & " , ''                                             AS ORDERINGTYPE" _
            & " , ''                                             AS ORDERINGOILNAME" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS TANKSTATUS" _
            & " , ''                                             AS LASTOILCODE" _
            & " , ''                                             AS LASTOILNAME" _
            & " , ''                                             AS PREORDERINGTYPE" _
            & " , ''                                             AS PREORDERINGOILNAME" _
            & " , ''                                             AS STACKINGORDERNO" _
            & " , ''                                             AS STACKINGFLG" _
            & " , ''                                             AS FIRSTRETURNFLG" _
            & " , ''                                             AS AFTERRETURNFLG" _
            & " , ''                                             AS OTTRANSPORTFLG" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS ACTUALLODDATE" _
            & " , ''                                             AS ACTUALDEPDATE" _
            & " , ''                                             AS ACTUALARRDATE" _
            & " , ''                                             AS ACTUALACCDATE" _
            & " , ''                                             AS ACTUALEMPARRDATE" _
            & " , ''                                             AS RETURNDATETRAIN" _
            & " , ''                                             AS JOINTCODE" _
            & " , ''                                             AS JOINT" _
            & " , ''                                             AS REMARK" _
            & " , '0'                                            AS DELFLG"

            '### 20200609 START #######################################################
            If work.WF_SEL_ORDERNUMBER.Text = "" Then
                SQLStr &=
                  " , 'O' + FORMAT(GETDATE(),'yyyyMMdd') + @P1       AS ORDERNO"
            Else
                SQLStr &=
                  " , @P1                                            AS ORDERNO"
            End If
            '### 20200609 END   #######################################################

            SQLStr &=
              " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS DETAILNO" _
            & " , ''                                             AS KAMOKU" _
            & " , ''                                             AS ORDERSTATUS" _
            & " , ''                                             AS USEORDERNO" _
            & " FROM sys.all_objects "

            SQLStr &=
                      " ORDER BY" _
                    & "    LINECNT"

            '明細データダブルクリック
        ElseIf work.WF_SEL_CREATEFLG.Text = "2" Then
            SQLStr =
              " SELECT" _
            & "   0                                                  AS LINECNT" _
            & " , ''                                                 AS OPERATION" _
            & " , CAST(OIT0002.UPDTIMSTP AS bigint)                  AS TIMSTP" _
            & " , 1                                                  AS 'SELECT'" _
            & " , 0                                                  AS HIDDEN" _
            & " , ISNULL(FORMAT(OIT0002.ORDERYMD, 'yyyy/MM/dd'), '') AS ORDERYMD" _
            & " , ISNULL(RTRIM(OIT0002.ORDERTYPE), '')               AS ORDERTYPE" _
            & " , ''                                                 AS ORDERTYPENAME" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINFO), '')               AS ORDERINFO" _
            & " , ''                                                 AS ORDERINFONAME" _
            & " , ISNULL(RTRIM(OIT0003.SHIPPERSCODE), '')            AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0003.SHIPPERSNAME), '')            AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0002.BASECODE), '')                AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0002.BASENAME), '')                AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')           AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')           AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0003.OILCODE), '')                 AS OILCODE" _
            & " , ISNULL(RTRIM(OIT0003.OILNAME), '')                 AS OILNAME" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')            AS ORDERINGTYPE" _
            & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '')         AS ORDERINGOILNAME" _
            & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                  AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0005.TANKSTATUS), '')              AS TANKSTATUS" _
            & " , ISNULL(RTRIM(OIT0005.LASTOILCODE), '')             AS LASTOILCODE" _
            & " , ISNULL(RTRIM(OIT0005.LASTOILNAME), '')             AS LASTOILNAME" _
            & " , ISNULL(RTRIM(OIT0005.PREORDERINGTYPE), '')         AS PREORDERINGTYPE" _
            & " , ISNULL(RTRIM(OIT0005.PREORDERINGOILNAME), '')      AS PREORDERINGOILNAME" _
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
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
            & "   END                                                                      AS JRINSPECTIONALERT" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P9" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN @P10" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN @P11" _
            & "   END                                                                      AS JRINSPECTIONALERTSTR" _
            & " , ISNULL(FORMAT(OIM0005.JRINSPECTIONDATE, 'yyyy/MM/dd'), '')               AS JRINSPECTIONDATE" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN " + CONST_ALERT_STATUS_CAUTION _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN " + CONST_ALERT_STATUS_WARNING _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN " + CONST_ALERT_STATUS_SAFE _
            & "   END                                                                      AS JRALLINSPECTIONALERT" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P9" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN @P10" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN @P11" _
            & "   END                                                                      AS JRALLINSPECTIONALERTSTR" _
            & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), '')            AS JRALLINSPECTIONDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALLODDATE, 'yyyy/MM/dd'), '')           AS ACTUALLODDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALDEPDATE, 'yyyy/MM/dd'), '')           AS ACTUALDEPDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALARRDATE, 'yyyy/MM/dd'), '')           AS ACTUALARRDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALACCDATE, 'yyyy/MM/dd'), '')           AS ACTUALACCDATE" _
            & " , ISNULL(FORMAT(OIT0003.ACTUALEMPARRDATE, 'yyyy/MM/dd'), '')        AS ACTUALEMPARRDATE" _
            & " , ISNULL(RTRIM(OIT0003.RETURNDATETRAIN), '')         AS RETURNDATETRAIN" _
            & " , ISNULL(RTRIM(OIT0003.JOINTCODE), '')               AS JOINTCODE" _
            & " , ISNULL(RTRIM(OIT0003.JOINT), '')                   AS JOINT" _
            & " , ISNULL(RTRIM(OIT0003.REMARK), '')                  AS REMARK" _
            & " , ISNULL(RTRIM(OIT0002.DELFLG), '')                  AS DELFLG" _
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')                 AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')                AS DETAILNO" _
            & " , ISNULL(RTRIM(OIT0003.KAMOKU), '')                  AS KAMOKU" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')             AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIT0005.USEORDERNO), '')              AS USEORDERNO" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "       OIT0002.ORDERNO = OIT0003.ORDERNO" _
            & "       AND OIT0003.DELFLG <> @P2" _
            & " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
            & "       OIT0003.TANKNO = OIT0005.TANKNUMBER" _
            & "       AND OIT0005.DELFLG <> @P2" _
            & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "       OIT0003.TANKNO = OIM0005.TANKNUMBER" _
            & "       AND OIM0005.DELFLG <> @P2" _
            & " WHERE OIT0002.ORDERNO = @P1" _
            & " AND OIT0002.DELFLG <> @P2"
            '& " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_NOW ON " _
            '& "       OIT0002.OFFICECODE = OIM0003_NOW.OFFICECODE" _
            '& "       AND OIT0002.SHIPPERSCODE = OIM0003_NOW.SHIPPERCODE" _
            '& "       AND OIT0002.BASECODE = OIM0003_NOW.PLANTCODE" _
            '& "       AND OIT0003.OILCODE = OIM0003_NOW.OILCODE" _
            '& "       AND OIM0003_NOW.DELFLG <> @P2" _
            '& " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_PAST ON " _
            '& "       OIT0002.OFFICECODE = OIM0003_PAST.OFFICECODE" _
            '& "       AND OIT0002.SHIPPERSCODE = OIM0003_PAST.SHIPPERCODE" _
            '& "       AND OIT0002.BASECODE = OIM0003_PAST.PLANTCODE" _
            '& "       AND OIT0005.LASTOILCODE = OIM0003_PAST.OILCODE" _
            '& "       AND OIM0003_PAST.DELFLG <> @P2" _

            SQLStr &=
                  " ORDER BY" _
                & "    OIT0002.ORDERYMD" _
                & "    , OIT0002.SHIPPERSCODE" _
                & "    , OIT0003.DETAILNO" _
                & "    , OIT0003.OILCODE" _
                & "    , OIT0003.ORDERINGTYPE" _
                & "    , OIT0003.TANKNO"
        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)

                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdrNum)
                End Using

                With SQLcmd.Parameters
                    .Add("@P0", SqlDbType.Int).Value = O_INSCNT                                  '明細数(新規作成)
                    .Add("@P3", SqlDbType.NVarChar, 10).Value = work.WF_SEL_SHIPPERSCODE.Text    '荷主コード
                    .Add("@P4", SqlDbType.NVarChar, 40).Value = work.WF_SEL_SHIPPERSNAME.Text    '荷主名
                    .Add("@P5", SqlDbType.NVarChar, 9).Value = work.WF_SEL_BASECODE.Text         '基地コード
                    .Add("@P6", SqlDbType.NVarChar, 40).Value = work.WF_SEL_BASENAME.Text        '基地名
                    .Add("@P7", SqlDbType.NVarChar, 10).Value = work.WF_SEL_CONSIGNEECODE.Text   '荷受人コード
                    .Add("@P8", SqlDbType.NVarChar, 40).Value = work.WF_SEL_CONSIGNEENAME.Text   '荷受人名
                    .Add("@P9", SqlDbType.NVarChar, 20).Value = C_INSPECTIONALERT.ALERT_RED      '赤丸
                    .Add("@P10", SqlDbType.NVarChar, 20).Value = C_INSPECTIONALERT.ALERT_YELLOW  '黄丸
                    .Add("@P11", SqlDbType.NVarChar, 20).Value = C_INSPECTIONALERT.ALERT_GREEN   '緑丸
                    .Add("@P12", SqlDbType.NVarChar, 9).Value = work.WF_SEL_PATTERNCODE.Text     '受注パターン
                    .Add("@P13", SqlDbType.NVarChar, 100).Value = work.WF_SEL_PATTERNNAME.Text   '受注パターン名
                End With

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)  '削除フラグ

                '新規登録の場合
                If work.WF_SEL_CREATEFLG.Text = "1" Then
                    For Each OIT0001WKrow As DataRow In OIT0001WKtbl.Rows
                        If work.WF_SEL_ORDERNUMBER.Text = "" Then
                            PARA1.Value = OIT0001WKrow("ORDERNO_NUM")
                        Else
                            PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                        End If
                        PARA2.Value = C_DELETE_FLG.ALIVE
                    Next

                    '既存更新の場合
                ElseIf work.WF_SEL_CREATEFLG.Text = "2" Then
                    PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                    PARA2.Value = C_DELETE_FLG.DELETE

                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    If i = 0 Then
                        work.WF_SEL_ORDERNUMBER.Text = Convert.ToString(OIT0001row("ORDERNO"))
                    End If
                    i += 1
                    OIT0001row("LINECNT") = i        'LINECNT

                    '受注情報
                    CODENAME_get("ORDERINFO", OIT0001row("ORDERINFO"), OIT0001row("ORDERINFONAME"), WW_DUMMY)

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D Select"
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

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0001row As DataRow In OIT0001tbl.Rows
            If Convert.ToString(OIT0001row("HIDDEN")) = "0" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0001row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0001tbl)

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
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.Both).ToString
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '◯ 画面表示設定処理
        WW_ScreenEnabledSet()

        '○(一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = Convert.ToString(TBLview.Item(0)("SELECT"))
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 油種数登録ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '着駅コードが未設定の場合
        '※一覧を作成するにあたり、基地コード・荷受人を取得するために、
        '　着駅コードは必須となるため
        If TxtArrstation.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "着駅", needsPopUp:=True)
            TxtArrstation.Focus()
            WW_CheckERR("着駅入力エラー。", C_MESSAGE_NO.PREREQUISITE_ERROR)
            Exit Sub
        End If

        '〇営業所配下情報を取得・設定
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstation.Text, WW_GetValue)
        work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
        work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
        work.WF_SEL_BASECODE.Text = WW_GetValue(2)
        work.WF_SEL_BASENAME.Text = WW_GetValue(3)
        work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
        work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)
        work.WF_SEL_PATTERNCODE.Text = WW_GetValue(6)
        work.WF_SEL_PATTERNNAME.Text = WW_GetValue(7)

        'タンク車数の件数カウント用
        Dim intTankCnt As Integer = 0
        intTankCnt += Integer.Parse(TxtHTank.Text)
        intTankCnt += Integer.Parse(TxtRTank.Text)
        intTankCnt += Integer.Parse(TxtTTank.Text)
        intTankCnt += Integer.Parse(TxtMTTank.Text)
        intTankCnt += Integer.Parse(TxtKTank.Text)
        intTankCnt += Integer.Parse(TxtK3Tank.Text)
        intTankCnt += Integer.Parse(TxtK5Tank.Text)
        intTankCnt += Integer.Parse(TxtK10Tank.Text)
        intTankCnt += Integer.Parse(TxtLTank.Text)
        intTankCnt += Integer.Parse(TxtATank.Text)
        TxtTotalTank.Text = intTankCnt.ToString()

        '油種数が１つも入力されていない場合
        If TxtTotalTank.Text = "0" Then
            Master.Output(C_MESSAGE_NO.OIL_OILTANK_INPUT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            TxtHTank.Focus()

            '〇 油種数登録ボタンのチェックを無効(False)
            WF_ButtonInsertFLG.Value = "FALSE"

        Else
            '〇 油種数登録ボタンのチェックを有効(True)
            WF_ButtonInsertFLG.Value = "TRUE"

        End If

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, intTankCnt)
        End Using

        '〇画面で設定された油種コードを取得
        WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        Dim arrTankCode(intTankCnt) As String
        Dim arrTankName(intTankCnt) As String
        Dim arrTankType(intTankCnt) As String
        Dim arrTankOrderName(intTankCnt) As String
        Dim z As Integer = 0

        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_HTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtHTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_HTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_RTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtRTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_RTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_TTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtTTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_TTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_MTTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtMTTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_MTTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_KTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtKTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_KTank1
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_K3Tank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtK3Tank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_K3Tank1
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_K5Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtK5Tank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_K5Tank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_K10Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtK10Tank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_K10Tank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_LTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtLTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_LTank1
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_ATank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtATank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_ATank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next

        '〇取得した油種情報をTBLに設定
        z = 0
        For Each OIT0001row As DataRow In OIT0001tbl.Rows
            OIT0001row("OILCODE") = arrTankCode(z)
            OIT0001row("OILNAME") = arrTankName(z)
            OIT0001row("ORDERINGTYPE") = arrTankType(z)
            OIT0001row("ORDERINGOILNAME") = arrTankOrderName(z)
            z += 1
        Next

        ''〇 1件以上の登録があった場合
        'If intTankCnt <> 0 Then
        '    '作成フラグを"2"(更新)に切換え
        '    work.WF_SEL_CREATEFLG.Text = "2"
        'End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

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
                WF_LeftMViewChange.Value = Integer.Parse(WF_LeftMViewChange.Value).ToString
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                If CInt(WF_LeftMViewChange.Value) <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)

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
                        prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtOrderOffice.Text)
                    End If
                    '########################################

                    '本線列車
                    If WF_FIELD.Value = "TxtHeadOfficeTrain" Then
                        'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, TxtHeadOfficeTrain.Text + work.WF_SEL_UORG.Text)
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtHeadOfficeTrain.Text)
                    End If

                    '発駅
                    If WF_FIELD.Value = "TxtDepstation" Then
                        'prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text, TxtDepstation.Text)
                        prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "1", TxtDepstation.Text)
                    End If

                    '着駅
                    If WF_FIELD.Value = "TxtArrstation" Then
                        'prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text, TxtArrstation.Text)
                        prmData = work.CreateSTATIONPTParam(work.WF_SEL_SALESOFFICECODE.Text + "2", TxtArrstation.Text)
                    End If

                    '荷主名
                    If WF_FIELD.Value = "SHIPPERSNAME" Then
                        'prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, "")
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
                    End If

                    '油種
                    If WF_FIELD.Value = "OILNAME" _
                        OrElse WF_FIELD.Value = "ORDERINGOILNAME" Then
                        'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
                    End If
                    'タンク車№
                    If WF_FIELD.Value = "TANKNO" Then
                        'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
                        '↓暫定一覧対応 2020/02/13 グループ会社版を復活させ石油システムに合わない部分は直す
                        .SetTableList(enumVal, WW_DUMMY, prmData)
                        .ActiveTable()
                        Return
                        '↑暫定一覧対応 2020/02/13
                    End If
                    'ジョイント
                    If WF_FIELD.Value = "JOINT" Then
                        '全表示のため設定をコメントにする。
                        'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
                    End If

                    .SetListBox(enumVal, WW_DUMMY, prmData)
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
                            ''返送日列車
                            'Case "RETURNDATETRAIN"
                            '    .WF_Calendar.Text = Date.Now.ToString("yyyy/MM/dd")
                    End Select
                    .ActiveCalendar()

                End If
            End With

        End If
    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click(ByVal chkFieldName As String)

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        Select Case chkFieldName
            Case "WF_CheckBoxSELECTSTACKING"
                '◯ 受注営業所が"010402"(仙台新港営業所)以外の場合
                If work.WF_SEL_SALESOFFICECODE.Text <> BaseDllConst.CONST_OFFICECODE_010402 Then
                    Exit Select
                End If

                'チェックボックス判定
                For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
                    If OIT0001tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If OIT0001tbl.Rows(i)("STACKINGFLG") = "on" Then
                            OIT0001tbl.Rows(i)("STACKINGFLG") = ""
                        Else
                            OIT0001tbl.Rows(i)("STACKINGFLG") = "on"
                        End If

                        '★チェックボックスのON,OFFチェック
                        If OIT0001tbl.Rows(i)("STACKINGFLG") = "on" Then
                            OIT0001tbl.Rows(i)("ACTUALLODDATE") = Date.Parse(Me.TxtDepDate.Text).AddDays(-1).ToString("yyyy/MM/dd")
                        Else
                            OIT0001tbl.Rows(i)("ACTUALLODDATE") = Me.TxtLoadingDate.Text
                        End If

                    End If
                Next

            Case Else
                'チェックボックス判定
                For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
                    If Convert.ToString(OIT0001tbl.Rows(i)("LINECNT")) = WF_SelectedIndex.Value Then
                        If Convert.ToString(OIT0001tbl.Rows(i)("OPERATION")) = "on" Then
                            OIT0001tbl.Rows(i)("OPERATION") = ""
                        Else
                            OIT0001tbl.Rows(i)("OPERATION") = "on"
                        End If
                    End If
                Next
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

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
            '本線列車
            Case "TxtHeadOfficeTrain"
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                'WW_FixvalueMasterSearch("", "TRAINNUMBER", TxtHeadOfficeTrain.Text, WW_GetValue)
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER", TxtHeadOfficeTrain.Text, WW_GetValue)

                '指定された本線列車№で値が取得できない場合はエラー判定
                If WW_GetValue(0) = "" Then
                    WW_RTN_SW = C_MESSAGE_NO.OIL_TRAIN_MASTER_NOTFOUND
                Else
                    WW_RTN_SW = C_MESSAGE_NO.NORMAL
                End If

                '発駅
                TxtDepstation.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
                '着駅
                TxtArrstation.Text = WW_GetValue(2)
                CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_DUMMY)

                '〇 (予定)の日付を設定
                TxtLoadingDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtDepDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(6))).ToString("yyyy/MM/dd")
                TxtArrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                TxtAccDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                TxtEmparrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")

                TxtHeadOfficeTrain.Focus()
            '発駅
            Case "TxtDepstation"
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_RTN_SW)
            '着駅
            Case "TxtArrstation"
                CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_RTN_SW)

        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Select Case WF_FIELD.Value
                Case "TxtHeadOfficeTrain"
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
                Case "TxtDepstation"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "発駅")
                Case "TxtArrstation"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "着駅")
                Case Else
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
            End Select
        End If
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
        If leftview.ActiveViewIdx = 2 Then
            '一覧表表示時
            Dim selectedLeftTableVal = leftview.GetLeftTableValue()
            WW_SelectValue = selectedLeftTableVal(LEFT_TABLE_SELECTED_KEY)
            Dim selectedTblKey As String = "VALUE1"
            If selectedLeftTableVal.ContainsKey(selectedTblKey) = False Then
                selectedTblKey = "VALUE8"
            End If
            WW_SelectText = selectedLeftTableVal(selectedTblKey) '他のフィールド名でも取ること可能一旦VALUE1で
        ElseIf leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex.ToString
            WW_SelectValue = leftview.WF_LeftListBox.Items(CInt(WF_SelectedIndex.Value)).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(CInt(WF_SelectedIndex.Value)).Text

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

            Case "TxtOrderOffice"      '受注営業所
                '別の受注営業所が設定されて場合
                If TxtOrderOffice.Text <> WW_SelectText Then
                    TxtOrderOffice.Text = WW_SelectText
                    work.WF_SEL_SALESOFFICECODE.Text = WW_SelectValue
                    work.WF_SEL_SALESOFFICE.Text = WW_SelectText

                    '○ 本線列車, 発駅, 着駅のテキストボックスを初期化
                    TxtHeadOfficeTrain.Text = ""
                    TxtHeadOfficeTrainName.Text = ""
                    TxtDepstation.Text = ""
                    LblDepstationName.Text = ""
                    TxtArrstation.Text = ""
                    LblArrstationName.Text = ""

                    '○ 油種別タンク車数(車)の件数を初期化
                    TxtTotalTank.Text = "0"
                    TxtHTank.Text = "0"
                    TxtRTank.Text = "0"
                    TxtTTank.Text = "0"
                    TxtMTTank.Text = "0"
                    TxtKTank.Text = "0"
                    TxtK3Tank.Text = "0"
                    TxtK5Tank.Text = "0"
                    TxtK10Tank.Text = "0"
                    TxtLTank.Text = "0"
                    TxtATank.Text = "0"

                    '〇営業所配下情報を取得・設定
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstation.Text, WW_GetValue)
                    work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
                    work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
                    work.WF_SEL_BASECODE.Text = WW_GetValue(2)
                    work.WF_SEL_BASENAME.Text = WW_GetValue(3)
                    work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
                    work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)

                    '○ 一覧の初期化画面表示データ取得
                    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()       'DataBase接続

                        MAPDataGet(SQLcon, 0)
                    End Using

                    '○ 画面表示データ保存
                    Master.SaveTable(OIT0001tbl)

                End If

                '◯ 画面表示設定処理
                WW_ScreenEnabledSet()
                TxtOrderOffice.Focus()

            Case "TxtHeadOfficeTrain"   '本線列車
                '                TxtHeadOfficeTrain.Text = WW_SelectValue.Substring(0, 4)

                If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                    Dim selectedText = Me.Request.Form("commonLeftListSelectedText")
                    Dim selectedItem = leftview.WF_LeftListBox.Items.FindByText(selectedText)
                    WW_SelectValue = selectedItem.Value
                    WW_SelectText = selectedItem.Text
                End If

                TxtHeadOfficeTrain.Text = WW_SelectValue
                TxtHeadOfficeTrainName.Text = WW_SelectText
                If TxtHeadOfficeTrainName.Text = "" Then
                    '発駅
                    TxtDepstation.Text = ""
                    LblDepstationName.Text = ""
                    '着駅
                    TxtArrstation.Text = ""
                    LblArrstationName.Text = ""
                    '(予定)日付
                    TxtLoadingDate.Text = ""
                    TxtDepDate.Text = ""
                    TxtArrDate.Text = ""
                    TxtAccDate.Text = ""
                    TxtEmparrDate.Text = ""
                    '営業所配下情報
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

                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER_FIND", WW_SelectText, WW_GetValue)
                'WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER", WW_SelectValue, WW_GetValue)

                '積置可否フラグ
                '(積置列車:T, 非積置列車：N)
                If WW_GetValue(12) = "T" Then
                    '"1"(積置あり)を設定
                    work.WF_SEL_STACKINGFLG.Text = "1"
                    WW_ORDERINFOFLG_10 = True
                ElseIf WW_GetValue(12) = "N" Then
                    '"1"(積置なし)を設定
                    work.WF_SEL_STACKINGFLG.Text = "2"
                    WW_ORDERINFOFLG_10 = False
                Else
                    work.WF_SEL_STACKINGFLG.Text = "2"
                    WW_ORDERINFOFLG_10 = False
                End If

                '発駅
                TxtDepstation.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
                '着駅
                TxtArrstation.Text = WW_GetValue(2)
                CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_DUMMY)
                TxtHeadOfficeTrain.Focus()

                '〇 (予定)の日付を設定
                TxtLoadingDate.Text = Now.AddDays(1).ToString("yyyy/MM/dd")
                TxtDepDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(6))).ToString("yyyy/MM/dd")
                TxtArrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                TxtAccDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                TxtEmparrDate.Text = Now.AddDays(1 + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")

                '〇営業所配下情報を取得・設定
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstation.Text, WW_GetValue)
                work.WF_SEL_SHIPPERSCODE.Text = WW_GetValue(0)
                work.WF_SEL_SHIPPERSNAME.Text = WW_GetValue(1)
                work.WF_SEL_BASECODE.Text = WW_GetValue(2)
                work.WF_SEL_BASENAME.Text = WW_GetValue(3)
                work.WF_SEL_CONSIGNEECODE.Text = WW_GetValue(4)
                work.WF_SEL_CONSIGNEENAME.Text = WW_GetValue(5)
                work.WF_SEL_PATTERNCODE.Text = WW_GetValue(6)
                work.WF_SEL_PATTERNNAME.Text = WW_GetValue(7)

            Case "TxtDepstation"        '発駅
                TxtDepstation.Text = WW_SelectValue
                LblDepstationName.Text = WW_SelectText
                TxtDepstation.Focus()

            Case "TxtArrstation"        '着駅
                TxtArrstation.Text = WW_SelectValue
                LblArrstationName.Text = WW_SelectText
                TxtArrstation.Focus()

                '〇営業所配下情報を取得・設定
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PATTERNMASTER", TxtArrstation.Text, WW_GetValue)
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
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        TxtLoadingDate.Text = ""
                    Else
                        TxtLoadingDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtLoadingDate.Focus()

                '### 2020608 START ########################################################################################
                '◯ 列車(名称)から日数を取得
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER_FIND", Me.TxtHeadOfficeTrainName.Text, WW_GetValue)

                '〇 (予定)の日付を設定
                Me.TxtDepDate.Text = Date.Parse(Me.TxtLoadingDate.Text).AddDays(Integer.Parse(WW_GetValue(6))).ToString("yyyy/MM/dd")
                Me.TxtArrDate.Text = Date.Parse(Me.TxtLoadingDate.Text).AddDays(Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                Me.TxtAccDate.Text = Date.Parse(Me.TxtLoadingDate.Text).AddDays(Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                Me.TxtEmparrDate.Text = Date.Parse(Me.TxtLoadingDate.Text).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                '### 2020608 END   ########################################################################################

            Case "TxtDepDate"           '(予定)発日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        TxtDepDate.Text = ""
                    Else
                        TxtDepDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtDepDate.Focus()

                '### 2020608 START ########################################################################################
                '◯ 列車(名称)から日数を取得
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER_FIND", Me.TxtHeadOfficeTrainName.Text, WW_GetValue)

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

            Case "TxtArrDate"           '(予定)積車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        TxtArrDate.Text = ""
                    Else
                        TxtArrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtArrDate.Focus()

                '### 2020608 START ########################################################################################
                '◯ 列車(名称)から日数を取得
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER_FIND", Me.TxtHeadOfficeTrainName.Text, WW_GetValue)

                '〇 (予定)の日付を設定
                If Integer.Parse(WW_GetValue(8)) = 0 Then
                    Me.TxtAccDate.Text = Date.Parse(Me.TxtArrDate.Text).AddDays(Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtArrDate.Text).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                ElseIf Integer.Parse(WW_GetValue(8)) > 0 Then
                    Me.TxtAccDate.Text = Date.Parse(Me.TxtArrDate.Text).AddDays((-1 * Integer.Parse(WW_GetValue(8))) + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtArrDate.Text).AddDays((-1 * Integer.Parse(WW_GetValue(8))) + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                End If
                '### 2020608 END   ########################################################################################

            Case "TxtAccDate"           '(予定)受入日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        TxtAccDate.Text = ""
                    Else
                        TxtAccDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtAccDate.Focus()

                '### 2020608 START ########################################################################################
                '◯ 列車(名称)から日数を取得
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER_FIND", Me.TxtHeadOfficeTrainName.Text, WW_GetValue)

                '〇 (予定)の日付を設定
                If Integer.Parse(WW_GetValue(9)) = 0 Then
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtAccDate.Text).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                ElseIf Integer.Parse(WW_GetValue(9)) > 0 Then
                    Me.TxtEmparrDate.Text = Date.Parse(Me.TxtAccDate.Text).AddDays((-1 * Integer.Parse(WW_GetValue(9))) + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                End If
                '### 2020608 END   ########################################################################################

            Case "TxtEmparrDate"           '(予定)空車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        TxtEmparrDate.Text = ""
                    Else
                        TxtEmparrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtEmparrDate.Focus()

                '(一覧)荷主, (一覧)油種, (一覧)タンク車№, (一覧)ジョイント, (一覧)返送日列車
            Case "SHIPPERSNAME", "OILNAME", "ORDERINGOILNAME", "TANKNO", "JOINT", "RETURNDATETRAIN"
                '○ LINECNT取得
                Dim WW_LINECNT As Integer = 0
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                '○ 設定項目取得
                Dim WW_SETTEXT As String = WW_SelectText
                Dim WW_SETVALUE As String = WW_SelectValue

                '○ 画面表示データ復元
                If Not Master.RecoverTable(OIT0001tbl) Then Exit Sub

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0001tbl.AsEnumerable.
                            FirstOrDefault(Function(x) CInt(x.Item("LINECNT")) = WW_LINECNT)
                If IsNothing(updHeader) Then Exit Sub

                '〇 一覧項目へ設定
                '(一覧)荷主名を一覧に設定
                If WF_FIELD.Value = "SHIPPERSNAME" Then
                    updHeader.Item("SHIPPERSCODE") = WW_SETVALUE
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                    '(一覧)油種名を一覧に設定
                ElseIf WF_FIELD.Value = "OILNAME" Then
                    updHeader.Item("OILCODE") = WW_SETVALUE
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                    '(一覧)油種名(受発注用)を一覧に設定
                ElseIf WF_FIELD.Value = "ORDERINGOILNAME" Then
                    If WW_SETVALUE = "" Then
                        updHeader.Item("OILCODE") = ""
                        updHeader.Item(WF_FIELD.Value) = ""
                        updHeader.Item("OILNAME") = ""
                        updHeader.Item("ORDERINGTYPE") = ""
                    Else
                        updHeader.Item("OILCODE") = WW_SETVALUE.Substring(0, 4)
                        updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN_SEG", WW_SETVALUE, WW_GetValue)
                        updHeader.Item("OILNAME") = WW_GetValue(2)
                        updHeader.Item("ORDERINGTYPE") = WW_GetValue(1)
                    End If

                    '(一覧)タンク車№を一覧に設定
                ElseIf WF_FIELD.Value = "TANKNO" Then
                    'Dim WW_TANKNUMBER As String = WW_SETTEXT.Substring(0, 8).Replace("-", "")
                    Dim WW_TANKNUMBER As String = WW_SETVALUE
                    Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                    updHeader.Item(WF_FIELD.Value) = WW_TANKNUMBER
                    '### 20200819 START タンク車Noが変更されたらタンク車Noステータスを初期化 #####
                    updHeader.Item("TANKSTATUS") = ""
                    '### 20200819 END   タンク車Noが変更されたらタンク車Noステータスを初期化 #####

                    'WW_FixvalueMasterSearch("", "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)

                    '(一覧)前回油種
                    Dim WW_LASTOILNAME As String = ""
                    updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                    updHeader.Item("LASTOILNAME") = WW_GetValue(4)
                    updHeader.Item("PREORDERINGTYPE") = WW_GetValue(5)
                    updHeader.Item("PREORDERINGOILNAME") = WW_GetValue(6)

                    ''CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                    ''updHeader.Item("LASTOILNAME") = WW_LASTOILNAME

                    'WW_GetValue = {"", "", "", "", "", "", "", ""}
                    'WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", updHeader.Item("LASTOILCODE"), WW_GetValue)
                    'updHeader.Item("LASTOILNAME") = WW_GetValue(0)

                    '(一覧)交検日
                    Dim WW_JRINSPECTIONCNT As String
                    updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(2)
                    If WW_GetValue(2) <> "" Then
                        WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(2))).ToString

                        Dim WW_JRINSPECTIONFLG As String
                        If CInt(WW_JRINSPECTIONCNT) <= 3 Then
                            WW_JRINSPECTIONFLG = "1"
                        ElseIf CInt(WW_JRINSPECTIONCNT) >= 4 AndAlso CInt(WW_JRINSPECTIONCNT) <= 6 Then
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

                    '(一覧)全検日
                    Dim WW_JRALLINSPECTIONCNT As String
                    updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(3)
                    If WW_GetValue(3) <> "" Then
                        WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(3))).ToString

                        Dim WW_JRALLINSPECTIONFLG As String
                        If CInt(WW_JRALLINSPECTIONCNT) <= 3 Then
                            WW_JRALLINSPECTIONFLG = "1"
                        ElseIf CInt(WW_JRALLINSPECTIONCNT) >= 4 AndAlso CInt(WW_JRALLINSPECTIONCNT) <= 6 Then
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

                    '(一覧)ジョイントを一覧に設定
                ElseIf WF_FIELD.Value = "JOINT" Then
                    updHeader.Item("JOINTCODE") = WW_SETVALUE
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                    '(一覧)返送日列車を一覧に設定
                ElseIf WF_FIELD.Value = "RETURNDATETRAIN" Then
                    'Dim WW_DATE As Date
                    'Try
                    '    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    '    If WW_DATE < C_DEFAULT_YMD Then
                    '        updHeader.Item(WF_FIELD.Value) = ""
                    '    Else
                    '        updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text
                    '    End If
                    'Catch ex As Exception
                    'End Try

                End If
                'updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0001tbl) Then Exit Sub

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
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_UORG"              '運用部署
                WF_UORG.Focus()
            Case "TxtHeadOfficeTrain"   '本線列車
                TxtHeadOfficeTrain.Focus()
            Case "TxtDepstation"        '発駅
                TxtDepstation.Focus()
            Case "TxtArrstation"        '着駅
                TxtArrstation.Focus()
            Case "TxtLoadingDate"       '(予定)積込日
                TxtLoadingDate.Focus()
            Case "TxtDepDate"           '(予定)発日
                TxtDepDate.Focus()
            Case "TxtArrDate"           '(予定)積車着日
                TxtArrDate.Focus()
            Case "TxtAccDate"           '(予定)受入日
                TxtAccDate.Focus()
            Case "TxtEmparrDate"        '(予定)空車着日
                TxtEmparrDate.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
            If Convert.ToString(OIT0001tbl.Rows(i)("HIDDEN")) = "0" Then
                OIT0001tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
            If Convert.ToString(OIT0001tbl.Rows(i)("HIDDEN")) = "0" Then
                OIT0001tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_LIFTED_Click()

        Dim SelectChk As Boolean = False

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '■■■ OIT0001tbl関連の受注・受注明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注明細を一括論理削除
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0003_DETAIL       " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE ORDERNO     = @P01       " _
                    & "    AND DETAILNO    = @P02       " _
                    & "    AND DELFLG     <> '1'       ;"

            'Dim SQLStr As String =
            '        " UPDATE OIL.OIT0003_DETAIL       " _
            '        & "    SET UPDYMD      = @P11,      " _
            '        & "        UPDUSER     = @P12,      " _
            '        & "        UPDTERMID   = @P13,      " _
            '        & "        RECEIVEYMD  = @P14,      " _
            '        & "        DELFLG      = '1'        " _
            '        & "  WHERE ORDERNO     = @P01       " _
            '        & "    AND DETAILNO    = @P02       " _
            '        & "    AND TANKNO      = @P03       " _
            '        & "    AND KAMOKU      = @P04       " _
            '        & "    AND DELFLG     <> '1'       ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            'Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
            'Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            For Each OIT0001UPDrow As DataRow In OIT0001tbl.Rows
                If Convert.ToString(OIT0001UPDrow("OPERATION")) = "on" Then

                    If OIT0001UPDrow("LINECNT") < 9000 Then
                        SelectChk = True
                    End If

                    j += 1
                    OIT0001UPDrow("LINECNT") = j        'LINECNT
                    OIT0001UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0001UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0001UPDrow("ORDERNO")
                    PARA02.Value = OIT0001UPDrow("DETAILNO")
                    'PARA03.Value = OIT0001UPDrow("TANKNO")
                    'PARA04.Value = OIT0001UPDrow("KAMOKU")

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

            '### 20200609 START(内部No178) #################################################
            '一覧明細の件数を取得
            Dim cntTbl As Integer = OIT0001tbl.Select("DELFLG <> '1'").Count
            If cntTbl = 0 Then
                '★ 一覧明細がすべて削除(0件)になった場合は、すべてのステータスを初期値に戻す
                '◯ 受注TBLのステータス初期化
                WW_UpdateOrderStatus(BaseDllConst.CONST_ORDERSTATUS_100,
                                     InitializeFlg:=True)

                '◯ 画面定義変数の初期化
                '　★作成モード(１：新規登録, ２：更新)設定
                work.WF_SEL_CREATEFLG.Text = "1"
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
                Me.TxtTotalTank.Text = "0"

            End If

            '(受注TBL)タンク車数更新
            WW_UpdateOrderTankCnt(SQLcon)

            '空回日報(一覧)画面表示データ取得
            WW_OrderListTBLSet(SQLcon)
            '### 20200609 END  (内部No178) #################################################

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○メッセージ表示
        If SelectChk = False Then
            Master.Output(C_MESSAGE_NO.OIL_DELLINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        Else
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_ADD_Click()

        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Dim SQLStrNum As String

        If work.WF_SEL_ORDERNUMBER.Text = "" Then
            SQLStrNum =
            " SELECT " _
            & "  ISNULL(SUBSTRING(OIT0003.ORDERNO,1,9) + CONVERT(varchar,FORMAT(OIT0003.num,'00')), DUAL.ORDERNO) AS ORDERNO" _
            & ", '001'                                     AS DETAILNO" _
            & " FROM (" _
            & "  SELECT 'O' + FORMAT(GETDATE(),'yyyyMMdd') + '01' AS ORDERNO" _
            & " ) DUAL " _
            & " LEFT JOIN (" _
            & "  SELECT OIT0003.ORDERNO" _
            & "  ,  CONVERT(int,SUBSTRING(OIT0003.ORDERNO,10,2)) + 1 AS num" _
            & "  ,  ROW_NUMBER() OVER(ORDER BY OIT0003.ORDERNO DESC) AS RNUM" _
            & "  FROM OIL.OIT0003_DETAIL OIT0003" _
            & "  WHERE SUBSTRING(OIT0003.ORDERNO,2,8) = FORMAT(GETDATE(),'yyyyMMdd')" _
            & " ) OIT0003 ON " _
            & "   SUBSTRING(OIT0003.ORDERNO,2,8) = SUBSTRING(DUAL.ORDERNO,2,8) " _
            & "   AND ISNULL(OIT0003.RNUM, 1) = 1"
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
        'SQLStrNum =
        '" SELECT " _
        '    & "  MAX(OIT0003_1.ORDERNO)                                      AS ORDERNO" _
        '    & ", FORMAT(MAX(SUBSTRING(OIT0003_1.ORDERNO, 10, 2)) + 1, '00')  AS ORDERNO_NUM" _
        '    & ", FORMAT(MAX(ISNULL(OIT0003_2.DETAILNO, '000')) + 1, '000')   AS DETAILNO_NUM" _
        '    & " FROM (" _
        '    & "  SELECT ISNULL(MAX(OIT0003.ORDERNO),'O' + FORMAT(GETDATE(),'yyyyMMdd') + '00') AS ORDERNO" _
        '    & "  FROM   OIL.OIT0003_DETAIL OIT0003" _
        '    & "  WHERE  SUBSTRING(OIT0003.ORDERNO, 2, 8) = FORMAT(GETDATE(),'yyyyMMdd')" _
        '    & " ) OIT0003_1 " _
        '    & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003_2 ON" _
        '    & " OIT0003_1.ORDERNO = OIT0003_2.ORDERNO"

        '" SELECT " _
        '    & " ISNULL(FORMAT(MAX(SUBSTRING(OIT0002.ORDERNO, 10, 2)) + 1,'00'),'01') AS ORDERNO" _
        '    & " FROM OIL.OIT0002_ORDER OIT0002 " _
        '    & " WHERE SUBSTRING(OIT0002.ORDERNO, 2, 8) = FORMAT(GETDATE(),'yyyyMMdd')"

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String =
        " SELECT TOP (1)" _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , FORMAT(GETDATE(),'yyyy/MM/dd')                 AS ORDERYMD" _
            & " , @P12                                           AS ORDERTYPE" _
            & " , @P13                                           AS ORDERTYPENAME" _
            & " , ''                                             AS ORDERINFO" _
            & " , ''                                             AS ORDERINFONAME" _
            & " , @P02                                           AS SHIPPERSCODE" _
            & " , @P03                                           AS SHIPPERSNAME" _
            & " , @P04                                           AS BASECODE" _
            & " , @P05                                           AS BASENAME" _
            & " , @P06                                           AS CONSIGNEECODE" _
            & " , @P07                                           AS CONSIGNEENAME" _
            & " , ''                                             AS OILCODE" _
            & " , ''                                             AS OILNAME" _
            & " , ''                                             AS ORDERINGTYPE" _
            & " , ''                                             AS ORDERINGOILNAME" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS TANKSTATUS" _
            & " , ''                                             AS LASTOILCODE" _
            & " , ''                                             AS LASTOILNAME" _
            & " , ''                                             AS PREORDERINGTYPE" _
            & " , ''                                             AS PREORDERINGOILNAME" _
            & " , ''                                             AS STACKINGORDERNO" _
            & " , ''                                             AS STACKINGFLG" _
            & " , ''                                             AS FIRSTRETURNFLG" _
            & " , ''                                             AS AFTERRETURNFLG" _
            & " , ''                                             AS OTTRANSPORTFLG" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS ACTUALLODDATE" _
            & " , ''                                             AS ACTUALDEPDATE" _
            & " , ''                                             AS ACTUALARRDATE" _
            & " , ''                                             AS ACTUALACCDATE" _
            & " , ''                                             AS ACTUALEMPARRDATE" _
            & " , ''                                             AS RETURNDATETRAIN" _
            & " , ''                                             AS JOINTCODE" _
            & " , ''                                             AS JOINT" _
            & " , ''                                             AS REMARK" _
            & " , '0'                                            AS DELFLG" _
            & " , @P01                                           AS ORDERNO" _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS DETAILNO" _
            & " , ''                                             AS KAMOKU" _
            & " , ''                                             AS ORDERSTATUS" _
            & " , ''                                             AS USEORDERNO" _
            & " FROM sys.all_objects "
        SQLStr &=
                  " ORDER BY" _
                & "    LINECNT"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)
                Dim PARANUM1 As SqlParameter = SQLcmdNum.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                PARANUM1.Value = work.WF_SEL_ORDERNUMBER.Text

                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10) '荷主コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 40) '荷主名
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 9)  '基地コード
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 40) '基地名
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 10) '荷受人コード
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 40) '荷受人名

                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 9) '受注パターン
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 100) '受注パターン名

                Dim strOrderNo As String = ""
                Dim intDetailNo As Integer = 0
                For Each OIT0001WKrow As DataRow In OIT0001WKtbl.Rows
                    strOrderNo = Convert.ToString(OIT0001WKrow("ORDERNO"))
                    intDetailNo = CInt(OIT0001WKrow("DETAILNO"))
                    PARA1.Value = strOrderNo
                    PARA2.Value = work.WF_SEL_SHIPPERSCODE.Text
                    PARA3.Value = work.WF_SEL_SHIPPERSNAME.Text
                    PARA4.Value = work.WF_SEL_BASECODE.Text
                    PARA5.Value = work.WF_SEL_BASENAME.Text
                    PARA6.Value = work.WF_SEL_CONSIGNEECODE.Text
                    PARA7.Value = work.WF_SEL_CONSIGNEENAME.Text
                    PARA12.Value = work.WF_SEL_PATTERNCODE.Text
                    PARA13.Value = work.WF_SEL_PATTERNNAME.Text
                Next

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    ''○ フィールド名とフィールドの型を取得
                    'For index As Integer = 0 To SQLdr.FieldCount - 1
                    '    OIT0001WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    'Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                'Dim intDetailNo As Integer = 0
                'Dim strOrderNoBak As String = ""
                For Each OIT0001row As DataRow In OIT0001tbl.Rows

                    '行追加データに既存の受注№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[受注№]を利用
                    If CInt(OIT0001row("LINECNT")) = 0 Then
                        If work.WF_SEL_CREATEFLG.Text = "1" Then
                            OIT0001row("ORDERNO") = strOrderNo
                            OIT0001row("DETAILNO") = intDetailNo.ToString("000")
                        Else
                            OIT0001row("ORDERNO") = work.WF_SEL_ORDERNUMBER.Text
                            OIT0001row("DETAILNO") = intDetailNo.ToString("000")
                        End If
                        intDetailNo += 1
                    End If

                    '削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    If Convert.ToString(OIT0001row("HIDDEN")) = "1" Then
                        j += 1
                        OIT0001row("LINECNT") = j        'LINECNT
                    Else
                        i += 1
                        OIT0001row("LINECNT") = i        'LINECNT
                    End If
                    'strOrderNoBak = OIT0001row("ORDERNO")
                    If CInt(OIT0001row("DETAILNO")) >= intDetailNo Then
                        intDetailNo += 1
                    ElseIf Convert.ToString(OIT0001row("HIDDEN")) = "1" Then
                        intDetailNo += 1
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

#Region "帳票処理"
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        '******************************
        '帳票表示データ取得処理
        '******************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            ExcelDataGet(SQLcon)
        End Using

        '******************************
        '帳票作成処理の実行
        '******************************
        Using repCbj = New OIT0001CustomReport(Master.MAPID, Master.MAPID & ".xlsx", OIT0001Reporttbl)
            Dim url As String
            Try
                url = repCbj.CreateExcelPrintData
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        End Using

        ''### 共通帳票処理をコメント ##################################################################
        ''○ 帳票出力
        'CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        'CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        'CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        'CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        'CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        'CS0030REPORT.TBLDATA = OIT0001tbl                       'データ参照  Table
        'CS0030REPORT.CS0030REPORT()
        'If Not isNormal(CS0030REPORT.ERR) Then
        '    If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
        '        Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
        '    Else
        '        Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
        '    End If
        '    Exit Sub
        'End If

        ''○ 別画面でExcelを表示
        'WF_PrintURL.Value = CS0030REPORT.URL
        'ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        ''#############################################################################################

    End Sub

    ''' <summary>
    ''' 帳票表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0001Reporttbl) Then
            OIT0001Reporttbl = New DataTable
        End If

        If OIT0001Reporttbl.Columns.Count <> 0 Then
            OIT0001Reporttbl.Columns.Clear()
        End If

        OIT0001Reporttbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
        " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , OIT0002_OTHER.OFFICECODE                       AS OFFICECODE" _
            & " , OIT0002_OTHER.OFFICENAME                       AS OFFICENAME" _
            & " , OIT0002_OTHER.TRAINNO                          AS TRAINNO" _
            & " , OIT0002_OTHER.TRAINNAME                        AS TRAINNAME" _
            & " , OIT0003.SHIPPERSCODE                           AS SHIPPERSCODE" _
            & " , OIT0003.SHIPPERSNAME                           AS SHIPPERSNAME" _
            & " , OIT0002_OTHER.BASECODE                         AS BASECODE" _
            & " , OIT0002_OTHER.BASENAME                         AS BASENAME" _
            & " , OIT0002_OTHER.CONSIGNEECODE                    AS CONSIGNEECODE" _
            & " , OIT0002_OTHER.CONSIGNEENAME                    AS CONSIGNEENAME" _
            & " , OIT0002_OTHER.DEPSTATION                       AS DEPSTATION" _
            & " , OIT0002_OTHER.DEPSTATIONNAME                   AS DEPSTATIONNAME" _
            & " , OIT0002_OTHER.ARRSTATION                       AS ARRSTATION" _
            & " , OIT0002_OTHER.ARRSTATIONNAME                   AS ARRSTATIONNAME" _
            & " , OIT0002_OTHER.LODDATE                          AS LODDATE" _
            & " , OIT0002_OTHER.DEPDATE                          AS DEPDATE" _
            & " , OIT0002_OTHER.ARRDATE                          AS ARRDATE" _
            & " , OIT0002_OTHER.ACCDATE                          AS ACCDATE" _
            & " , OIT0002.EMPARRDATE                             AS EMPARRDATE" _
            & " , OIT0003.ACTUALLODDATE                          AS ACTUALLODDATE" _
            & " , OIT0003.ACTUALDEPDATE                          AS ACTUALDEPDATE" _
            & " , OIT0003.ACTUALARRDATE                          AS ACTUALARRDATE" _
            & " , OIT0003.ACTUALACCDATE                          AS ACTUALACCDATE" _
            & " , OIT0003.ACTUALEMPARRDATE                       AS ACTUALEMPARRDATE" _
            & " , OIM0005.MODEL                                  AS MODEL" _
            & " , OIT0003.TANKNO                                 AS TANKNO" _
            & " , OIT0003.CARSNUMBER                             AS CARSNUMBER" _
            & " , OIT0003.CARSAMOUNT                             AS CARSAMOUNT" _
            & " , OIM0005.LOAD                                   AS LOAD" _
            & " , OIM0005.OWNERCODE                              AS OWNERCODE" _
            & " , OIM0005.OWNERNAME                              AS OWNERNAME" _
            & " , OIM0005.LEASECODE                              AS LEASECODE" _
            & " , OIM0005.LEASENAME                              AS LEASENAME" _
            & " , OIM0005.JRINSPECTIONDATE                       AS JRINSPECTIONDATE" _
            & " , OIM0005.JRALLINSPECTIONDATE                    AS JRALLINSPECTIONDATE" _
            & " , OIT0003.RETURNDATETRAIN                        AS RETURNDATETRAIN" _
            & " , OIT0003.JOINTCODE                              AS JOINTCODE" _
            & " , OIT0003.JOINT                                  AS JOINT" _
            & " , OIT0003.REMARK                                 AS REMARK" _
            & " , OIM0003.BIGOILCODE                             AS BIGOILCODE" _
            & " , OIM0003.BIGOILNAME                             AS BIGOILNAME" _
            & " , OIM0003.MIDDLEOILCODE                          AS MIDDLEOILCODE" _
            & " , OIM0003.MIDDLEOILNAME                          AS MIDDLEOILNAME" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIT0003.OILNAME                                AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
            & " , OIM0003.OTOILCODE                              AS OTOILCODE" _
            & " , OIM0003.OTOILNAME                              AS OTOILNAME" _
            & " , OIM0003.SHIPPEROILCODE                         AS SHIPPEROILCODE" _
            & " , OIM0003.SHIPPEROILNAME                         AS SHIPPEROILNAME" _
            & " , OIM0003.CHECKOILCODE                           AS CHECKOILCODE" _
            & " , OIM0003.CHECKOILNAME                           AS CHECKOILNAME" _
            & " , OIT0005.LASTOILCODE                            AS LASTOILCODE" _
            & " , OIT0005.LASTOILNAME                            AS LASTOILNAME" _
            & " , OIT0005.PREORDERINGTYPE                        AS PREORDERINGTYPE" _
            & " , OIT0005.PREORDERINGOILNAME                     AS PREORDERINGOILNAME" _
            & " , OTOILCT.OTOILCODE                              AS OTOILCTCODE" _
            & " , OTOILCT.CNT                                    AS OTOILCTCNT" _
            & " FROM oil.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "     (OIT0003.ORDERNO = OIT0002.ORDERNO OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " LEFT JOIN OIL.OIT0002_ORDER OIT0002_OTHER ON " _
            & "     OIT0002_OTHER.ORDERNO = OIT0003.ORDERNO " _
            & " LEFT JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "     OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & " AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & " AND OIM0003.DELFLG <> @P02 " _
            & " LEFT JOIN oil.OIM0005_TANK OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & " AND OIM0005.DELFLG <> @P02 " _
            & " LEFT JOIN oil.OIT0005_SHOZAI OIT0005 ON " _
            & "     OIT0005.TANKNUMBER = OIT0003.TANKNO " _
            & " AND OIT0005.DELFLG <> @P02 "

        SQLStr &=
              " LEFT JOIN ( " _
            & "   SELECT " _
            & "         OIT0002.ORDERNO " _
            & "       , OIT0003.SHIPPERSCODE " _
            & "       , OIT0003.SHIPPERSNAME " _
            & "       , OIM0003.OTOILCODE " _
            & "       , OIM0003.OTOILNAME " _
            & "       , COUNT(1) AS CNT " _
            & "   FROM oil.OIT0002_ORDER OIT0002 " _
            & "   INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "       (OIT0003.ORDERNO = OIT0002.ORDERNO OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
            & "   AND OIT0003.DELFLG <> @P02 " _
            & "   INNER JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "       OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & "   AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & "   AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & "   AND OIM0003.DELFLG <> @P02 " _
            & "   WHERE OIT0002.ORDERNO = @P01 " _
            & "   GROUP BY " _
            & "         OIT0002.ORDERNO " _
            & "       , OIT0003.SHIPPERSCODE " _
            & "       , OIT0003.SHIPPERSNAME " _
            & "       , OIM0003.OTOILCODE " _
            & "       , OIM0003.OTOILNAME " _
            & " ) OTOILCT ON " _
            & "     OTOILCT.SHIPPERSCODE = OIT0003.SHIPPERSCODE " _
            & " AND OTOILCT.OTOILCODE = OIM0003.OTOILCODE "

        SQLStr &=
              " WHERE OIT0002.ORDERNO = @P01 " _
            & " AND OIT0002.DELFLG <> @P02 "

        SQLStr &=
                " ORDER BY" _
            & "    OIT0003.SHIPPERSCODE" _
            & "  , OIT0002.DEPSTATION" _
            & "  , OIM0003.OTOILCODE" _
            & "  , OIT0003.TANKNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11)  '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001Reporttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001Reporttbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0001Reprow As DataRow In OIT0001Reporttbl.Rows
                    i += 1
                    OIT0001Reprow("LINECNT") = i        'LINECNT
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0001Reporttbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

#End Region

    ''' <summary>
    ''' 空回日報確定ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_RESULT As String = ""

        '〇新規登録時で油種数登録ボタンを押下しているかチェック
        If work.WF_SEL_CREATEFLG.Text = "1" _
            AndAlso WF_ButtonInsertFLG.Value = "FALSE" Then

            Master.Output(C_MESSAGE_NO.OIL_OILREGISTER_NOTUSE, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '○関連チェック
        WW_Check(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '〇日付妥当性チェック
        WW_CheckValidityDate(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '### 20200819 START 一度チェックは外したが要望により復活 ###############################################
        '### 20200623 空回日報の登録では、タンク車所在の更新は行わないのでタンク車所在のチェックは実施しない ###
        '〇 タンク車状態チェック
        WW_CheckTankStatus(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If
        '### 20200819 END   一度チェックは外したが要望により復活 ###############################################

        '〇前回油種と油種の整合性チェック
        WW_CheckLastOilConsistency(WW_ERRCODE)
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
                '★チェックNGの場合は、登録されている受注TBL・受注明細TBLを削除する。
                WW_DeleteOrder(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                Exit Sub
            End If
        End Using

        '列車タンク車重複チェック(同じ列車(発日も一緒)でタンク車がすでに登録済みかチェック)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_CheckTrainTankRepeat(WW_ERRCODE, SQLcon)
            If WW_ERRCODE = "ERR1" Then
                Master.Output(C_MESSAGE_NO.OIL_ORDER_DEPDATE_SAMETRAINTANKNO, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                '★チェックNGの場合は、登録されている受注TBL・受注明細TBLを削除する。
                WW_DeleteOrder(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                Exit Sub
            ElseIf WW_ERRCODE = "ERR2" Then
                Master.Output(C_MESSAGE_NO.OIL_ORDER_DEPDATE_DIFFTRAINTANKNO, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                '★チェックNGの場合は、登録されている受注TBL・受注明細TBLを削除する。
                WW_DeleteOrder(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                Exit Sub
            ElseIf WW_ERRCODE = "ERR3" Then
                Master.Output(C_MESSAGE_NO.OIL_ORDER_LODDATE_DIFFTRAINTANKNO, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                '★チェックNGの場合は、登録されている受注TBL・受注明細TBLを削除する。
                WW_DeleteOrder(SQLcon, work.WF_SEL_ORDERNUMBER.Text)
                Exit Sub
            End If
        End Using

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            ''受注DB追加・更新
            'Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            '    SQLcon.Open()       'DataBase接続

            '    WW_UpdateOrder(SQLcon)
            'End Using

            ''受注明細DB追加・更新
            'Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            '    SQLcon.Open()       'DataBase接続

            '    WW_UpdateOrderDetail(SQLcon)
            'End Using

            '◯新規作成(空回日報から作成)したデータの場合
            If work.WF_SEL_EMPTYTURNFLG.Text = "1" Then
                '(受注TBL)タンク車数更新
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続
                    WW_UpdateOrderTankCnt(SQLcon)
                End Using

            End If

            '空回日報(一覧)画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_OrderListTBLSet(SQLcon)
            End Using

            '### START 受注履歴テーブルの追加(2020/03/26) #############
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_InsertOrderHistory(SQLcon)
            End Using
            '### END   ################################################

        End If

        '○ GridView初期設定
        '○ 画面表示データ再取得(空回日報(明細)画面表示データ取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            work.WF_SEL_CREATEFLG.Text = "2"
            MAPDataGet(SQLcon, 0)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub

    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
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

        '○ XLSUPLOAD明細⇒INPtbl
        Master.CreateEmptyTable(OIT0001INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIT0001INProw As DataRow = OIT0001INPtbl.NewRow

            '○ 初期クリア
            For Each OIT0001INPcol As DataColumn In OIT0001INPtbl.Columns
                If IsDBNull(OIT0001INProw.Item(OIT0001INPcol)) OrElse IsNothing(OIT0001INProw.Item(OIT0001INPcol)) Then
                    Select Case OIT0001INPcol.ColumnName
                        Case "LINECNT"
                            OIT0001INProw.Item(OIT0001INPcol) = 0
                        Case "OPERATION"
                            OIT0001INProw.Item(OIT0001INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            OIT0001INProw.Item(OIT0001INPcol) = 0
                        Case "SELECT"
                            OIT0001INProw.Item(OIT0001INPcol) = 1
                        Case "HIDDEN"
                            OIT0001INProw.Item(OIT0001INPcol) = 0
                        Case Else
                            OIT0001INProw.Item(OIT0001INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("ORDERYMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("SHIPPERSNAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OILNAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TANKNO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LASTOILNAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRINSPECTIONALERT") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRINSPECTIONDATE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRALLINSPECTIONALERT") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRALLINSPECTIONDATE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("RETURNDATETRAIN") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JOINT") >= 0 AndAlso
                WW_COLUMNS.IndexOf("REMARK") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DELFLG") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ORDERNO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DETAILNO") >= 0 Then
                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    If XLSTBLrow("ORDERYMD").Equals(OIT0001row("ORDERYMD")) AndAlso
                        XLSTBLrow("SHIPPERSNAME").Equals(OIT0001row("SHIPPERSNAME")) AndAlso
                        XLSTBLrow("OILNAME").Equals(OIT0001row("OILNAME")) AndAlso
                        XLSTBLrow("TANKNO").Equals(OIT0001row("TANKNO")) AndAlso
                        XLSTBLrow("LASTOILNAME").Equals(OIT0001row("LASTOILNAME")) AndAlso
                        XLSTBLrow("JRINSPECTIONALERT").Equals(OIT0001row("JRINSPECTIONALERT")) AndAlso
                        XLSTBLrow("JRINSPECTIONDATE").Equals(OIT0001row("JRINSPECTIONDATE")) AndAlso
                        XLSTBLrow("JRALLINSPECTIONALERT").Equals(OIT0001row("JRALLINSPECTIONALERT")) AndAlso
                        XLSTBLrow("JRALLINSPECTIONDATE").Equals(OIT0001row("JRALLINSPECTIONDATE")) AndAlso
                        XLSTBLrow("RETURNDATETRAIN").Equals(OIT0001row("RETURNDATETRAIN")) AndAlso
                        XLSTBLrow("JOINT").Equals(OIT0001row("JOINT")) AndAlso
                        XLSTBLrow("REMARK").Equals(OIT0001row("REMARK")) AndAlso
                        XLSTBLrow("DELFLG").Equals(OIT0001row("DELFLG")) AndAlso
                        XLSTBLrow("ORDERNO").Equals(OIT0001row("ORDERNO")) AndAlso
                        XLSTBLrow("DETAILNO").Equals(OIT0001row("DETAILNO")) Then
                        OIT0001INProw.ItemArray = OIT0001row.ItemArray
                        Exit For
                    End If
                Next
            End If

            Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

            '○ 項目セット
            ''会社コード
            'OIM0004INProw.Item("CAMPCODE") = work.WF_SEL_CAMPCODE.Text

            ''運用部署
            'OIM0004INProw.Item("UORG") = work.WF_SEL_UORG.Text

            '受注登録日
            If WW_COLUMNS.IndexOf("ORDERYMD") >= 0 Then
                OIT0001INProw("ORDERYMD") = XLSTBLrow("ORDERYMD")
            End If

            '荷主名
            If WW_COLUMNS.IndexOf("SHIPPERSNAME") >= 0 Then
                OIT0001INProw("SHIPPERSNAME") = XLSTBLrow("SHIPPERSNAME")
            End If

            '油種名
            If WW_COLUMNS.IndexOf("OILNAME") >= 0 Then
                OIT0001INProw("OILNAME") = XLSTBLrow("OILNAME")
                Dim oilName As String = Convert.ToString(OIT0001INProw("OILNAME"))
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN_N", oilName, WW_GetValue)
                OIT0001INProw("OILCODE") = WW_GetValue(0)
            End If

            'タンク車№
            If WW_COLUMNS.IndexOf("TANKNO") >= 0 Then
                OIT0001INProw("TANKNO") = XLSTBLrow("TANKNO")

                '●タンク車№から対象データを自動で設定
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", Convert.ToString(OIT0001INProw("TANKNO")), WW_GetValue)
                'WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", OIT0001INProw("TANKNO"), WW_GetValue)
                OIT0001INProw("LASTOILCODE") = WW_GetValue(1)

                '前回油種名(前回油種コードから油種名を取得し設定)
                'WW_GetValue = {"", "", "", "", "", "", "", ""}
                'WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", OIT0001INProw("LASTOILCODE"), WW_GetValue)
                'OIT0001INProw("LASTOILNAME") = WW_GetValue(0)
                OIT0001INProw("LASTOILNAME") = WW_GetValue(4)
                OIT0001INProw("PREORDERINGTYPE") = WW_GetValue(5)
                OIT0001INProw("PREORDERINGOILNAME") = WW_GetValue(6)

                '交検日
                OIT0001INProw("JRINSPECTIONDATE") = WW_GetValue(2)

                '交付アラート
                If WW_GetValue(2) <> "" Then
                    Dim WW_JRINSPECTIONCNT As String = DateDiff(DateInterval.Day, Date.Parse(Now.ToString("yyyy/MM/dd")), Date.Parse(WW_GetValue(2))).ToString
                    Dim WW_JRINSPECTIONFLG As String
                    If CInt(WW_JRINSPECTIONCNT) <= 3 Then
                        WW_JRINSPECTIONFLG = "1"
                    ElseIf CInt(WW_JRINSPECTIONCNT) >= 4 AndAlso CInt(WW_JRINSPECTIONCNT) <= 6 Then
                        WW_JRINSPECTIONFLG = "2"
                    Else
                        WW_JRINSPECTIONFLG = "3"
                    End If
                    Select Case WW_JRINSPECTIONFLG
                        Case "1"
                            OIT0001INProw("JRINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                        Case "2"
                            OIT0001INProw("JRINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                        Case "3"
                            OIT0001INProw("JRINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                    End Select
                Else
                    OIT0001INProw("JRINSPECTIONALERT") = ""
                End If

                '全検日
                OIT0001INProw("JRALLINSPECTIONDATE") = WW_GetValue(3)

                '全検アラート
                If WW_GetValue(3) <> "" Then
                    Dim WW_JRALLINSPECTIONCNT As String = DateDiff(DateInterval.Day, Date.Parse(Now.ToString("yyyy/MM/dd")), Date.Parse(WW_GetValue(3))).ToString
                    Dim WW_JRALLINSPECTIONFLG As String
                    If CInt(WW_JRALLINSPECTIONCNT) <= 3 Then
                        WW_JRALLINSPECTIONFLG = "1"
                    ElseIf CInt(WW_JRALLINSPECTIONCNT) >= 4 AndAlso CInt(WW_JRALLINSPECTIONCNT) <= 6 Then
                        WW_JRALLINSPECTIONFLG = "2"
                    Else
                        WW_JRALLINSPECTIONFLG = "3"
                    End If
                    Select Case WW_JRALLINSPECTIONFLG
                        Case "1"
                            OIT0001INProw("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                        Case "2"
                            OIT0001INProw("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                        Case "3"
                            OIT0001INProw("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                    End Select
                Else
                    OIT0001INProw("JRALLINSPECTIONALERT") = ""
                End If

            End If

            '返送日列車
            If WW_COLUMNS.IndexOf("RETURNDATETRAIN") >= 0 Then
                OIT0001INProw("RETURNDATETRAIN") = XLSTBLrow("RETURNDATETRAIN")
            End If

            'ジョイント
            If WW_COLUMNS.IndexOf("JOINT") >= 0 Then
                OIT0001INProw("JOINT") = XLSTBLrow("JOINT")
            End If

            '記事欄
            If WW_COLUMNS.IndexOf("REMARK") >= 0 Then
                OIT0001INProw("REMARK") = XLSTBLrow("REMARK")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIT0001INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '受注№
            If WW_COLUMNS.IndexOf("ORDERNO") >= 0 Then
                OIT0001INProw("ORDERNO") = XLSTBLrow("ORDERNO")
            End If

            '受注明細№
            If WW_COLUMNS.IndexOf("DETAILNO") >= 0 Then
                OIT0001INProw("DETAILNO") = XLSTBLrow("DETAILNO")
            End If

            '○ 名称取得
            'CODENAME_get("TORICODES", OIM0004INProw("TORICODES"), OIM0004INProw("TORINAMES"), WW_DUMMY)           '取引先名称(出荷先)
            'CODENAME_get("SHUKABASHO", OIM0004INProw("SHUKABASHO"), OIM0004INProw("SHUKABASHONAMES"), WW_DUMMY)   '出荷場所名称

            'CODENAME_get("TORICODET", OIM0004INProw("TORICODET"), OIM0004INProw("TORINAMET"), WW_DUMMY)           '取引先名称(届先)
            'CODENAME_get("TODOKECODE", OIM0004INProw("TODOKECODE"), OIM0004INProw("TODOKENAME"), WW_DUMMY)        '届先名称

            OIT0001INPtbl.Rows.Add(OIT0001INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIT0001tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub

    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                WF_RightViewChange.Value = Integer.Parse(WF_RightViewChange.Value).ToString
            Catch ex As Exception
                Exit Sub
            End Try
            Dim enumVal = DirectCast([Enum].ToObject(GetType(GRIS0004RightBox.RIGHT_TAB_INDEX), CInt(WF_RightViewChange.Value)), GRIS0004RightBox.RIGHT_TAB_INDEX)
            rightview.SelectIndex(enumVal)
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
        Dim updHeader = OIT0001tbl.AsEnumerable.
                    FirstOrDefault(Function(x) CInt(x.Item("LINECNT")) = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "TxtOrderOffice"    '受注営業所

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
                If WW_ListValue <> "" Then
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN_N", WW_ListValue, WW_GetValue)
                    updHeader.Item("OILCODE") = WW_GetValue(0)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                Else
                    updHeader.Item("OILCODE") = ""
                    updHeader.Item(WF_FIELD.Value) = ""
                End If

            Case "ORDERINGOILNAME"   '(一覧)油種(受発注用)
                If WW_ListValue <> "" Then
                    WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN_SEG_N", WW_ListValue, WW_GetValue)
                    updHeader.Item("OILCODE") = WW_GetValue(0)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    updHeader.Item("OILNAME") = WW_GetValue(2)
                    updHeader.Item("ORDERINGTYPE") = WW_GetValue(1)

                Else
                    updHeader.Item("OILCODE") = ""
                    updHeader.Item(WF_FIELD.Value) = ""
                    updHeader.Item("OILNAME") = ""
                    updHeader.Item("ORDERINGTYPE") = ""
                End If

            Case "TANKNO"            '(一覧)タンク車№

                '入力が空の場合は、対象項目を空文字で設定する。
                If WW_ListValue = "" Then
                    'タンク車№
                    updHeader.Item("TANKNO") = ""
                    '### 20200819 START タンク車Noが変更されたらタンク車Noステータスを初期化 #####
                    updHeader.Item("TANKSTATUS") = ""
                    '### 20200819 END   タンク車Noが変更されたらタンク車Noステータスを初期化 #####
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
                    Exit Select
                End If

                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)

                '### 2020/06/17 START #####################################################################################
                '    タンク車Noを手入力した場合はすべてのタンク車Noの情報を取得したいため、会社コードにて取得するように変更
                'WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)
                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)
                '### 2020/06/17 END   #####################################################################################

                'タンク車№
                updHeader.Item("TANKNO") = WW_ListValue
                '### 20200819 START タンク車Noが変更されたらタンク車Noステータスを初期化 #####
                updHeader.Item("TANKSTATUS") = ""
                '### 20200819 END   タンク車Noが変更されたらタンク車Noステータスを初期化 #####

                '前回油種
                Dim WW_LASTOILNAME As String = ""
                updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                updHeader.Item("LASTOILNAME") = WW_GetValue(4)
                updHeader.Item("PREORDERINGTYPE") = WW_GetValue(5)
                updHeader.Item("PREORDERINGOILNAME") = WW_GetValue(6)
                ''CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                ''updHeader.Item("LASTOILNAME") = WW_LASTOILNAME

                'WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                'WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", WW_GetValue(1), WW_GetValue)
                ''WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN_SEG", WW_GetValue(1) + WW_GetValue(4), WW_GetValue)
                'updHeader.Item("LASTOILNAME") = WW_GetValue(0)

                '交検日
                Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                Dim WW_JRINSPECTIONCNT As String
                updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(2)
                If WW_GetValue(2) <> "" Then
                    WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(2))).ToString

                    Dim WW_JRINSPECTIONFLG As String
                    If CInt(WW_JRINSPECTIONCNT) <= 3 Then
                        WW_JRINSPECTIONFLG = "1"
                    ElseIf CInt(WW_JRINSPECTIONCNT) >= 4 AndAlso CInt(WW_JRINSPECTIONCNT) <= 6 Then
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
                updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(3)
                If WW_GetValue(3) <> "" Then
                    WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(3))).ToString

                    Dim WW_JRALLINSPECTIONFLG As String
                    If CInt(WW_JRALLINSPECTIONCNT) <= 3 Then
                        WW_JRALLINSPECTIONFLG = "1"
                    ElseIf CInt(WW_JRALLINSPECTIONCNT) >= 4 AndAlso CInt(WW_JRALLINSPECTIONCNT) <= 6 Then
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

                '(★サーバー側で設定しているため必要ないが念のため残す(20200302))
            Case "RETURNDATETRAIN"   '(一覧)返送日列車
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "JOINT"             '(一覧)ジョイント
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

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
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", work.WF_SEL_SALESOFFICECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SALESOFFICE", work.WF_SEL_SALESOFFICECODE.Text, TxtOrderOffice.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "受注営業所 : " & work.WF_SEL_SALESOFFICECODE.Text)
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
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", TxtHeadOfficeTrain.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "本線列車", needsPopUp:=True)
            TxtHeadOfficeTrain.Focus()
            WW_CheckMES1 = "本線列車入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '発駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", TxtDepstation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "発駅 : " & TxtDepstation.Text)
                TxtDepstation.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発駅", needsPopUp:=True)
            TxtDepstation.Focus()
            WW_CheckMES1 = "発駅入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '着駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRSTATION", TxtArrstation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "着駅 : " & TxtArrstation.Text)
                TxtArrstation.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "着駅", needsPopUp:=True)
            TxtArrstation.Focus()
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
                WW_STYMD = CDate(C_DEFAULT_YMD)
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
                WW_STYMD = CDate(C_DEFAULT_YMD)
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
                WW_STYMD = CDate(C_DEFAULT_YMD)
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
                WW_STYMD = CDate(C_DEFAULT_YMD)
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
                WW_STYMD = CDate(C_DEFAULT_YMD)
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

        '(一覧)タンク車No(重複チェック)
        Dim OIT0001tbl_DUMMY As DataTable = OIT0001tbl.Copy
        Dim OIT0001tbl_dv As DataView = New DataView(OIT0001tbl_DUMMY)
        Dim chkTankNo As String = ""

        'タンク車Noでソートし、重複がないかチェックする。
        OIT0001tbl_dv.Sort = "TANKNO"
        For Each drv As DataRowView In OIT0001tbl_dv
            If Convert.ToString(drv("HIDDEN")) <> "1" AndAlso Convert.ToString(drv("TANKNO")) <> "" AndAlso chkTankNo = Convert.ToString(drv("TANKNO")) Then
                Master.Output(C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckMES1 = "タンク車№重複エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                O_RTN = "ERR"

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0001tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = drv("LINECNT"))
                updHeader.Item("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                CODENAME_get("ORDERINFO", updHeader.Item("ORDERINFO"), updHeader.Item("ORDERINFONAME"), WW_DUMMY)

                '○ 画面表示データ保存
                Master.SaveTable(OIT0001tbl)
                Exit Sub
            End If

            '行削除したデータの場合は退避しない。
            If Convert.ToString(drv("HIDDEN")) <> "1" Then
                chkTankNo = Convert.ToString(drv("TANKNO"))
            End If
        Next

        '(一覧)チェック
        For Each OIT0001row As DataRow In OIT0001tbl.Rows

            '(一覧)受注油種(空白チェック)
            If OIT0001row("ORDERINGOILNAME") = "" And OIT0001row("DELFLG") = "0" Then
                Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)受注油種", needsPopUp:=True)

                WW_CheckMES1 = "受注油種未設定エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0001row)
                O_RTN = "ERR"
                Exit Sub
            End If
        Next

        ''(一覧)タンク車No
        'For Each OIT0001row As DataRow In OIT0001tbl.Rows
        '    If OIT0001row("TANKNO").Equals("") And OIT0001row("DELFLG") = "0" Then
        '        Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR)

        '        WW_CheckMES1 = "タンク車No入力エラー。"
        '        WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '        WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0001row)
        '        O_RTN = "ERR"
        '        Exit Sub
        '    End If
        'Next

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

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
            If Not DateTime.IsLeapYear(CInt(chkLeapYear)) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                '月と日の範囲チェック
            ElseIf CInt(getMonth) >= 13 OrElse CInt(getDay) >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            End If
        Catch ex As Exception
            Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

    End Sub

    ''' <summary>
    ''' 年月日妥当性チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckValidityDate(ByRef O_RTN As String)

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
            TxtLoadingDate.Focus()
            WW_CheckMES1 = "(予定日)入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        ElseIf iresult = -1 Then    '(予定)積込日 < (予定)発日の場合
            WW_ORDERINFOFLG_10 = True
            work.WF_SEL_STACKINGFLG.Text = "1"
        End If

        '(予定)発日 と　(予定)積車着日を比較
        iresult = Date.Parse(TxtDepDate.Text).CompareTo(Date.Parse(TxtArrDate.Text))
        If iresult = 1 Then
            Master.Output(C_MESSAGE_NO.OIL_DATE_VALIDITY_ERROR, C_MESSAGE_TYPE.ERR, "(予定)発日 > (予定)積車着日", needsPopUp:=True)
            TxtLoadingDate.Focus()
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
            TxtLoadingDate.Focus()
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
        Dim WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '前回油種と油種の整合性チェック
        For Each OIT0001row As DataRow In OIT0001tbl.Rows

            'タンク車№が未設定の場合はチェックはしない。
            If Convert.ToString(OIT0001row("TANKNO")) = "" Then
                Continue For
            End If

            WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            WW_FixvalueMasterSearch(Convert.ToString(OIT0001row("LASTOILCODE")) + Convert.ToString(OIT0001row("PREORDERINGTYPE")), "LASTOILCONSISTENCY", Convert.ToString(OIT0001row("OILCODE")) + Convert.ToString(OIT0001row("ORDERINGTYPE")), WW_GetValue)

            If WW_GetValue(2) = "1" Then
                Master.Output(C_MESSAGE_NO.OIL_LASTOIL_CONSISTENCY_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                WW_CheckMES1 = "前回油種と油種の整合性エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_LASTOIL_CONSISTENCY_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0001row)
                O_RTN = "ERR"
                Exit Sub
            End If
        Next

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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '〇 検索(営業所).テキストボックスが未設定
        WW_OfficeCode = work.WF_SEL_SALESOFFICECODE.Text

        'タンク車状態チェック
        '(1:発送　2:到着予定　3:到着　4:交検　5:全検　6:修理　7:疎開留置)
        For Each OIT0001row As DataRow In OIT0001tbl.Rows

            If OIT0001row("TANKNO") <> "" AndAlso OIT0001row("DELFLG") = "0" AndAlso OIT0001row("TANKSTATUS") = "" Then
                'タンク車情報を取得
                WW_FixvalueMasterSearch("01", "TANKNUMBER", OIT0001row("TANKNO"), WW_GetValue)

                '### 20200618 START すでに指定したタンク車№が他の受注で使用されている場合の対応 #################
                '使用受注№が設定されている場合
                If WW_GetValue(12) <> "" Then

                    '### 20200820 START タンク車Noの積込日が同一で設定されていないかを確認する対応 ###############
                    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()       'DataBase接続

                        '★タンク車Noの積込日重複チェック
                        WW_CheckTankNoLodDateRepeat(O_RTN, SQLcon, OIT0001row)
                    End Using
                    If O_RTN = "ERR" Then Exit Sub
                    '### 20200820 END   タンク車Noの積込日が同一で設定されていないかを確認する対応 ###############

                    '次のレコードに進む（SKIPする）
                    Continue For
                End If
                '### 20200618 END   すでに指定したタンク車№が他の受注で使用されている場合の対応 #################

                'タンク車状態
                Select Case WW_GetValue(11)
                        'タンク車状態が"2"(到着予定), "3"(到着)の場合
                    Case "2", "3"
                        If OIT0001row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_101 Then
                            OIT0001row("ORDERINFO") = ""
                            OIT0001row("ORDERINFONAME") = ""
                        End If
                        'タンク車状態が"2"(到着予定), "3"(到着)以外の場合
                    Case Else
                        OIT0001row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_101
                        CODENAME_get("ORDERINFO", OIT0001row("ORDERINFO"), OIT0001row("ORDERINFONAME"), WW_DUMMY)

                        Master.Output(C_MESSAGE_NO.OIL_TANKSTATUS_ERROR,
                              C_MESSAGE_TYPE.ERR,
                              "(" + OIT0001row("TANKNO") + ")" + WW_GetValue(9),
                              needsPopUp:=True)

                        WW_CheckMES1 = "タンク車状態未到着エラー。"
                        WW_CheckMES2 = C_MESSAGE_NO.OIL_TANKSTATUS_ERROR
                        WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0001row)
                        O_RTN = "ERR"

                        '○ 画面表示データ保存
                        Master.SaveTable(OIT0001tbl)
                        Exit Sub
                End Select
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
        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

        '異なる列車チェック用
        If IsNothing(OIT0001WK2tbl) Then
            OIT0001WK2tbl = New DataTable
        End If

        If OIT0001WK2tbl.Columns.Count <> 0 Then
            OIT0001WK2tbl.Columns.Clear()
        End If

        OIT0001WK2tbl.Clear()

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
                PARA2.Value = Me.TxtHeadOfficeTrain.Text
                PARA3.Value = Me.TxtDepDate.Text
                PARA4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA5.Value = work.WF_SEL_STACKINGFLG.Text
                PARA6.Value = work.WF_SEL_CONSIGNEECODE.Text
                PARA7.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0001CHKDrow As DataRow In OIT0001WKtbl.Rows

                    '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                    If OIT0001CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                    Master.Output(C_MESSAGE_NO.OIL_ORDER_DEPDATE_SAMETRAIN, C_MESSAGE_TYPE.ERR, OIT0001CHKDrow("ORDERNO"), needsPopUp:=True)

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
                PARADF2.Value = Me.TxtHeadOfficeTrain.Text
                PARADF3.Value = Me.TxtDepDate.Text
                PARADF4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARADF5.Value = work.WF_SEL_STACKINGFLG.Text
                PARADF7.Value = C_DELETE_FLG.DELETE
                PARADF8.Value = work.WF_SEL_SALESOFFICECODE.Text

                Using SQLdr As SqlDataReader = SQLDiffTraincmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WK2tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WK2tbl.Load(SQLdr)
                End Using

                ''〇1件でも存在したら、登録済みエラーとして終了。
                'For Each OIT0001CHKDrow As DataRow In OIT0001WK2tbl.Rows
                '    Master.Output(C_MESSAGE_NO.OIL_ORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, OIT0001CHKDrow("ORDERNO"), needsPopUp:=True)

                '    WW_CheckMES1 = "受注データ登録済みエラー。"
                '    WW_CheckMES2 = C_MESSAGE_NO.OIL_ORDER_REPEAT_ERROR
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                '    O_RTN = "ERR"
                '    Exit Sub
                'Next
                '### 20200620 END  ((全体)No79対応) ######################################

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D CHECK_TRAINREPEAT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D CHECK_TRAINREPEAT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 列車タンク車重複チェック(同一(異なる)列車(発日も一緒)でタンク車がすでに登録済みかチェック)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTrainTankRepeat(ByRef O_RTN As String, ByVal SQLcon As SqlConnection)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '同一列車チェック用
        If IsNothing(OIT0001WK3tbl) Then
            OIT0001WK3tbl = New DataTable
        End If

        If OIT0001WK3tbl.Columns.Count <> 0 Then
            OIT0001WK3tbl.Columns.Clear()
        End If

        OIT0001WK3tbl.Clear()

        '異なる列車チェック用(同一発日)
        If IsNothing(OIT0001WK4tbl) Then
            OIT0001WK4tbl = New DataTable
        End If

        If OIT0001WK4tbl.Columns.Count <> 0 Then
            OIT0001WK4tbl.Columns.Clear()
        End If

        OIT0001WK4tbl.Clear()

        '異なる列車チェック用(同一積込日)
        If IsNothing(OIT0001WK5tbl) Then
            OIT0001WK5tbl = New DataTable
        End If

        If OIT0001WK5tbl.Columns.Count <> 0 Then
            OIT0001WK5tbl.Columns.Clear()
        End If

        OIT0001WK5tbl.Clear()

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
            & "       OIT0003.ORDERNO         = OIT0002.ORDERNO "

        If OIT0001tbl.Select("TANKNO <> ''").Count <> 0 Then
            SQLStr &= "   AND OIT0003.TANKNO          IN ("
        End If

        '一覧に設定しているタンク車を条件に設定
        Dim i As Integer = 0
        For Each OIT0001row As DataRow In OIT0001tbl.Rows
            If OIT0001row("TANKNO") <> "" Then
                If i = 0 Then
                    SQLStr &= "'" & OIT0001row("TANKNO") & "' "
                Else
                    SQLStr &= ", '" & OIT0001row("TANKNO") & "' "
                End If
                i += 1
            End If
        Next

        '### 20200620 START((全体)No79対応)異なる列車で同一積込日の場合###########
        If OIT0001tbl.Select("TANKNO <> ''").Count <> 0 Then
            SQLStr &= "                                  )"
        End If
        Dim SQLDiffLODTrainStr As String =
              SQLStr _
            & " WHERE OIT0002.USEPROPRIETYFLG = '1' " _
            & "   AND OIT0002.ORDERNO        <> @P01 " _
            & "   AND OIT0002.OFFICECODE      = @P06 " _
            & "   AND OIT0002.LODDATE         = @P03 " _
            & "   AND OIT0002.ORDERSTATUS    <> @P04 " _
            & "   AND OIT0002.DELFLG         <> @P05 " _
            & "   AND OIT0002.TRAINNO        <> @P02 "
        '### 20200620 END  ((全体)No79対応)異なる列車で同一積込日の場合###########

        SQLStr &=
              " WHERE OIT0002.USEPROPRIETYFLG = '1' " _
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
                  SQLDiffLODTraincmd As New SqlCommand(SQLDiffLODTrainStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)発日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 6)  '受注営業所
                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA2.Value = Me.TxtHeadOfficeTrain.Text
                PARA3.Value = Me.TxtDepDate.Text
                PARA4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARA5.Value = C_DELETE_FLG.DELETE
                PARA6.Value = work.WF_SEL_SALESOFFICECODE.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WK3tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WK3tbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    '★行削除したデータはSKIPする。
                    If OIT0001row("DELFLG") = "1" Then Continue For
                    For Each OIT0001CHKDrow As DataRow In OIT0001WK3tbl.Rows

                        '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                        If OIT0001CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                        If OIT0001CHKDrow("TANKNO") = OIT0001row("TANKNO") _
                            AndAlso OIT0001row("TANKNO") <> "" Then

                            OIT0001row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                            CODENAME_get("ORDERINFO", OIT0001row("ORDERINFO"), OIT0001row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "タンク車№(同一の列車番号)重複。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR1"

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0001row)

                            Exit For
                        Else
                            If OIT0001row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                                OIT0001row("ORDERINFO") = ""
                                OIT0001row("ORDERINFONAME") = ""
                            End If
                        End If
                    Next
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0001tbl)

                If O_RTN = "ERR1" Then Exit Sub

                '### 20200620 START((全体)No79対応)異なる列車で同一発日の場合#############
                Dim PARADF1 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARADF2 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARADF3 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)発日
                Dim PARADF4 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARADF5 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARADF6 As SqlParameter = SQLDiffDEPTraincmd.Parameters.Add("@P06", SqlDbType.NVarChar, 6)  '受注営業所
                PARADF1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARADF2.Value = Me.TxtHeadOfficeTrain.Text
                PARADF3.Value = Me.TxtDepDate.Text
                PARADF4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARADF5.Value = C_DELETE_FLG.DELETE
                PARADF6.Value = work.WF_SEL_SALESOFFICECODE.Text

                Using SQLdr As SqlDataReader = SQLDiffDEPTraincmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WK4tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WK4tbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    '★行削除したデータはSKIPする。
                    If OIT0001row("DELFLG") = "1" Then Continue For
                    For Each OIT0001CHKDrow As DataRow In OIT0001WK4tbl.Rows

                        '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                        If OIT0001CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                        If OIT0001CHKDrow("TANKNO") = OIT0001row("TANKNO") _
                            AndAlso OIT0001row("TANKNO") <> "" Then
                            OIT0001row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                            CODENAME_get("ORDERINFO", OIT0001row("ORDERINFO"), OIT0001row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "タンク車№(同一の列車番号)重複。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR2"

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0001row)

                            Exit For
                        Else
                            If OIT0001row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                                OIT0001row("ORDERINFO") = ""
                                OIT0001row("ORDERINFONAME") = ""
                            End If
                        End If
                    Next
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0001tbl)

                If O_RTN = "ERR2" Then Exit Sub
                '### 20200620 END  ((全体)No79対応)異なる列車で同一発日の場合#############

                '### 20200620 START((全体)No79対応)異なる列車で同一積込日の場合###########
                Dim PARALDF1 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARALDF2 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARALDF3 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P03", SqlDbType.Date)         '(予定)積込日
                Dim PARALDF4 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARALDF5 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARALDF6 As SqlParameter = SQLDiffLODTraincmd.Parameters.Add("@P06", SqlDbType.NVarChar, 6)  '受注営業所
                PARALDF1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARALDF2.Value = Me.TxtHeadOfficeTrain.Text
                PARALDF3.Value = Me.TxtLoadingDate.Text
                PARALDF4.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARALDF5.Value = C_DELETE_FLG.DELETE
                PARALDF6.Value = work.WF_SEL_SALESOFFICECODE.Text

                Using SQLdr As SqlDataReader = SQLDiffLODTraincmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WK5tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WK5tbl.Load(SQLdr)
                End Using

                '〇1件でも存在したら、登録済みエラーとして終了。
                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    For Each OIT0001CHKDrow As DataRow In OIT0001WK5tbl.Rows

                        '★存在したデータがまだ「100:受注受付」の場合は、割当前なのでSKIPする。
                        If OIT0001CHKDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_100 Then Continue For

                        If OIT0001CHKDrow("TANKNO") = OIT0001row("TANKNO") Then
                            OIT0001row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                            CODENAME_get("ORDERINFO", OIT0001row("ORDERINFO"), OIT0001row("ORDERINFONAME"), WW_DUMMY)

                            WW_CheckMES1 = "タンク車№(異なる列車番号)重複。"
                            WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0003row)
                            O_RTN = "ERR3"

                            '受注明細TBLの受注情報を更新
                            WW_UpdateOrderInfo(SQLcon, "2", OIT0001row)

                            Exit For
                        Else
                            If OIT0001row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                                OIT0001row("ORDERINFO") = ""
                                OIT0001row("ORDERINFONAME") = ""
                            End If
                        End If
                    Next
                Next

                '○ 画面表示データ保存
                Master.SaveTable(OIT0001tbl)

                If O_RTN = "ERR3" Then Exit Sub
                '### 20200620 END  ((全体)No79対応)異なる列車で同一積込日の場合###########
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D CHECK_TRAINTANKREPEAT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D CHECK_TRAINTANKREPEAT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' タンク車Noの積込日重複チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckTankNoLodDateRepeat(ByRef O_RTN As String,
                                              ByVal SQLcon As SqlConnection,
                                              ByVal I_DR As DataRow)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '他オーダー情報取得(同オーダーの同一積込日取得用)
        If IsNothing(OIT0001WK6tbl) Then
            OIT0001WK6tbl = New DataTable
        End If

        If OIT0001WK6tbl.Columns.Count <> 0 Then
            OIT0001WK6tbl.Columns.Clear()
        End If

        OIT0001WK6tbl.Clear()

        '他オーダー情報取得(同オーダーの同一発日取得用)
        If IsNothing(OIT0001WK7tbl) Then
            OIT0001WK7tbl = New DataTable
        End If

        If OIT0001WK7tbl.Columns.Count <> 0 Then
            OIT0001WK7tbl.Columns.Clear()
        End If

        OIT0001WK7tbl.Clear()

        '○ チェックSQL
        '　説明
        '     登録された内容が受注TBLにすでに登録済みかチェックする

        Dim SQLLODStr As String =
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
            & " , ISNULL(RTRIM(OIT0003.ACTUALLODDATE), RTRIM(OIT0002.LODDATE)) AS LODDATE" _
            & " , ISNULL(RTRIM(OIT0002.DEPDATE), '')         AS DEPDATE" _
            & " , ISNULL(RTRIM(OIT0002.ARRDATE), '')         AS ARRDATE" _
            & " , ISNULL(RTRIM(OIT0002.ACCDATE), '')         AS ACCDATE" _
            & " , ISNULL(RTRIM(OIT0002.EMPARRDATE), '')      AS EMPARRDATE" _
            & " FROM oil.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "       OIT0003.ORDERNO  = OIT0002.ORDERNO " _
            & "   AND OIT0003.TANKNO   = @P03" _
            & "   AND OIT0003.DELFLG  <> @P04"

        Dim SQLDEPStr As String =
              SQLLODStr _
            & " WHERE OIT0002.DEPDATE      = @P01 " _
            & "   AND OIT0002.ORDERNO     <> @P05 " _
            & "   AND OIT0002.ORDERSTATUS <> @P02 "


        SQLLODStr &=
              " WHERE ISNULL(OIT0003.ACTUALLODDATE, OIT0002.LODDATE) = @P01 " _
            & "   AND OIT0002.ORDERNO     <> @P05 " _
            & "   AND OIT0002.ORDERSTATUS <> @P02 "

        Try
            Using SQLLODcmd As New SqlCommand(SQLLODStr, SQLcon),
                  SQLDEPcmd As New SqlCommand(SQLDEPStr, SQLcon)
                Dim PARALOD1 As SqlParameter = SQLLODcmd.Parameters.Add("@P01", SqlDbType.Date)         '積込日
                Dim PARALOD2 As SqlParameter = SQLLODcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARALOD3 As SqlParameter = SQLLODcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 8)  'タンク車№
                Dim PARALOD4 As SqlParameter = SQLLODcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARALOD5 As SqlParameter = SQLLODcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 11) '受注No
                PARALOD1.Value = Me.TxtLoadingDate.Text
                PARALOD2.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARALOD3.Value = I_DR("TANKNO")
                PARALOD4.Value = C_DELETE_FLG.DELETE
                PARALOD5.Value = I_DR("ORDERNO")

                Using SQLdr As SqlDataReader = SQLLODcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WK6tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WK6tbl.Load(SQLdr)
                End Using

                If OIT0001WK6tbl.Rows.Count <> 0 Then
                    I_DR("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                    CODENAME_get("ORDERINFO", I_DR("ORDERINFO"), I_DR("ORDERINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "タンク車№(同一の積込日)重複。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, I_DR)
                    O_RTN = "ERR"

                    Master.Output(C_MESSAGE_NO.OIL_TANKNO_LOADDATE_USE, C_MESSAGE_TYPE.ERR, OIT0001WK6tbl.Rows(0)("ORDERNO"), needsPopUp:=True)

                Else
                    If I_DR("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                        I_DR("ORDERINFO") = ""
                        I_DR("ORDERINFONAME") = ""
                    End If
                End If

                '○ 画面表示データ保存
                Master.SaveTable(OIT0001tbl)

                If O_RTN = "ERR" Then Exit Sub


                Dim PARADEP1 As SqlParameter = SQLDEPcmd.Parameters.Add("@P01", SqlDbType.Date)         '発日
                Dim PARADEP2 As SqlParameter = SQLDEPcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARADEP3 As SqlParameter = SQLDEPcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 8)  'タンク車№
                Dim PARADEP4 As SqlParameter = SQLDEPcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARADEP5 As SqlParameter = SQLDEPcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 11) '受注No
                PARADEP1.Value = Me.TxtDepDate.Text
                PARADEP2.Value = BaseDllConst.CONST_ORDERSTATUS_900
                PARADEP3.Value = I_DR("TANKNO")
                PARADEP4.Value = C_DELETE_FLG.DELETE
                PARADEP5.Value = I_DR("ORDERNO")

                Using SQLdr As SqlDataReader = SQLDEPcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WK7tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WK7tbl.Load(SQLdr)
                End Using

                If OIT0001WK7tbl.Rows.Count <> 0 Then
                    I_DR("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85
                    CODENAME_get("ORDERINFO", I_DR("ORDERINFO"), I_DR("ORDERINFONAME"), WW_DUMMY)

                    WW_CheckMES1 = "タンク車№(同一の発日)重複。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, I_DR)
                    O_RTN = "ERR"

                    Master.Output(C_MESSAGE_NO.OIL_TANKNO_LOADDATE_USE, C_MESSAGE_TYPE.ERR, OIT0001WK7tbl.Rows(0)("ORDERNO"), needsPopUp:=True)

                Else
                    If I_DR("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_85 Then
                        I_DR("ORDERINFO") = ""
                        I_DR("ORDERINFONAME") = ""
                    End If
                End If

                '○ 画面表示データ保存
                Master.SaveTable(OIT0001tbl)

                If O_RTN = "ERR" Then Exit Sub

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D CHECK_TANKNO_LODDATE_REPEAT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D CHECK_TANKNO_LODDATE_REPEAT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

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

        WW_ERR_MES &= ControlChars.NewLine & "  --> 受注営業所         =" & TxtOrderOffice.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車           =" & TxtHeadOfficeTrain.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 発駅               =" & TxtDepstation.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 着駅               =" & TxtArrstation.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)積込日       =" & TxtLoadingDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)発日         =" & TxtDepDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)積車着日     =" & TxtArrDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)受入日       =" & TxtAccDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)空車着日     =" & TxtEmparrDate.Text

        rightview.SetErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' エラーレポート編集(一覧用)
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0001row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckListERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0001row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0001row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項番               =" & Convert.ToString(OIM0001row("LINECNT")) & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 登録日             =" & Convert.ToString(OIM0001row("ORDERYMD")) & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主               =" & Convert.ToString(OIM0001row("SHIPPERSNAME")) & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種               =" & Convert.ToString(OIM0001row("OILNAME")) & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車番号       =" & Convert.ToString(OIM0001row("TANKNO"))
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' マスタ検索処理
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="O_VALUE"></param>
    Protected Sub WW_FixvalueMasterSearch(ByVal I_CODE As String, ByVal I_CLASS As String, ByVal I_KEYCODE As String, ByRef O_VALUE() As String)

        If IsNothing(OIT0001Fixvaltbl) Then
            OIT0001Fixvaltbl = New DataTable
        End If

        If OIT0001Fixvaltbl.Columns.Count <> 0 Then
            OIT0001Fixvaltbl.Columns.Clear()
        End If

        OIT0001Fixvaltbl.Clear()

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
               " SELECT" _
                & "   ISNULL(RTRIM(VIW0001.CAMPCODE), '') AS CAMPCODE" _
                & " , ISNULL(RTRIM(VIW0001.CLASS), '')    AS CLASS" _
                & " , ISNULL(RTRIM(VIW0001.KEYCODE), '')  AS KEYCODE" _
                & " , ISNULL(RTRIM(VIW0001.STYMD), '')    AS STYMD" _
                & " , ISNULL(RTRIM(VIW0001.ENDYMD), '')   AS ENDYMD" _
                & " , ISNULL(RTRIM(VIW0001.VALUE1), '')   AS VALUE1" _
                & " , ISNULL(RTRIM(VIW0001.VALUE2), '')   AS VALUE2" _
                & " , ISNULL(RTRIM(VIW0001.VALUE3), '')   AS VALUE3" _
                & " , ISNULL(RTRIM(VIW0001.VALUE4), '')   AS VALUE4" _
                & " , ISNULL(RTRIM(VIW0001.VALUE5), '')   AS VALUE5" _
                & " , ISNULL(RTRIM(VIW0001.VALUE6), '')   AS VALUE6" _
                & " , ISNULL(RTRIM(VIW0001.VALUE7), '')   AS VALUE7" _
                & " , ISNULL(RTRIM(VIW0001.VALUE8), '')   AS VALUE8" _
                & " , ISNULL(RTRIM(VIW0001.VALUE9), '')   AS VALUE9" _
                & " , ISNULL(RTRIM(VIW0001.VALUE10), '')  AS VALUE10" _
                & " , ISNULL(RTRIM(VIW0001.VALUE11), '')  AS VALUE11" _
                & " , ISNULL(RTRIM(VIW0001.VALUE12), '')  AS VALUE12" _
                & " , ISNULL(RTRIM(VIW0001.VALUE13), '')  AS VALUE13" _
                & " , ISNULL(RTRIM(VIW0001.VALUE14), '')  AS VALUE14" _
                & " , ISNULL(RTRIM(VIW0001.VALUE15), '')  AS VALUE15" _
                & " , ISNULL(RTRIM(VIW0001.DELFLG), '')   AS DELFLG" _
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
                        OIT0001Fixvaltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001Fixvaltbl.Load(SQLdr)
                End Using

                If I_KEYCODE.Equals("") Then
                    Dim i As Integer = 0
                    For Each OIT0001WKrow As DataRow In OIT0001Fixvaltbl.Rows
                        O_VALUE(i) = Convert.ToString(OIT0001WKrow("KEYCODE"))
                        i += 1
                    Next
                Else
                    For Each OIT0001WKrow As DataRow In OIT0001Fixvaltbl.Rows
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = Convert.ToString(OIT0001WKrow("VALUE" & i.ToString()))
                        Next
                        'O_VALUE(0) = OIT0001WKrow("VALUE1")
                        'O_VALUE(1) = OIT0001WKrow("VALUE2")
                        'O_VALUE(2) = OIT0001WKrow("VALUE3")
                        'O_VALUE(3) = OIT0001WKrow("VALUE4")
                        'O_VALUE(4) = OIT0001WKrow("VALUE5")
                        'O_VALUE(5) = OIT0001WKrow("VALUE6")
                        'O_VALUE(6) = OIT0001WKrow("VALUE7")
                        'O_VALUE(7) = OIT0001WKrow("VALUE8")
                    Next
                End If
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D MASTER_SELECT"
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

        '### 20200623 START((全体)No76対応) ######################################
        '◯本線列車名が未設定の場合
        '　(日付の自動設定を行うには、本線列車名が必要なため制御をかける)
        If Me.TxtHeadOfficeTrainName.Text = "" Then
            '★(予定)日付を非活性にする。
            Me.TxtLoadingDate.Enabled = False
            Me.TxtDepDate.Enabled = False
            Me.TxtArrDate.Enabled = False
            Me.TxtAccDate.Enabled = False
            Me.TxtEmparrDate.Enabled = False
        Else
            '★(予定)日付を活性にする。
            Me.TxtLoadingDate.Enabled = True
            Me.TxtDepDate.Enabled = True
            Me.TxtArrDate.Enabled = True
            Me.TxtAccDate.Enabled = True
            Me.TxtEmparrDate.Enabled = True
        End If
        '### 20200623 END  ((全体)No76対応) ######################################

        '〇初期化
        'ハイオク
        TxtHTank.Enabled = False
        'レギュラー
        TxtRTank.Enabled = False
        '灯油
        TxtTTank.Enabled = False
        '未添加灯油
        TxtMTTank.Enabled = False
        '軽油
        TxtKTank.Enabled = False
        '３号軽油
        TxtK3Tank.Enabled = False
        '軽油５
        TxtK5Tank.Enabled = False
        '軽油１０
        TxtK10Tank.Enabled = False
        'ＬＳＡ
        TxtLTank.Enabled = False
        'Ａ重油
        TxtATank.Enabled = False

        '更新モードの場合は、油種の非活性をだけ行い処理を抜ける。
        If work.WF_SEL_CREATEFLG.Text = "2" Then Exit Sub

        '〇各営業者で管理している油種を取得
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN", "", WW_GetValue)

        For i As Integer = 0 To WW_GetValue.Length - 1
            Select Case WW_GetValue(i)
                    'ハイオク
                Case BaseDllConst.CONST_HTank
                    TxtHTank.Enabled = True
                    'レギュラー
                Case BaseDllConst.CONST_RTank
                    TxtRTank.Enabled = True
                    '灯油
                Case BaseDllConst.CONST_TTank
                    '### 2020/06/15 START ########################################################
                    '★根岸営業所の場合
                    If work.WF_SEL_SALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011402 Then
                        '★JXTG北信油槽所, 及びJXTG甲府油槽所の場合
                        If work.WF_SEL_CONSIGNEECODE.Text = BaseDllConst.CONST_CONSIGNEECODE_10 _
                            OrElse work.WF_SEL_CONSIGNEECODE.Text = BaseDllConst.CONST_CONSIGNEECODE_20 Then
                            '入力を未許可にする。
                            Me.TxtTTank.Enabled = False
                        Else
                            Me.TxtTTank.Enabled = True
                        End If
                    Else
                        TxtTTank.Enabled = True
                    End If
                    '### 2020/06/15 END   ########################################################
                    '未添加灯油
                Case BaseDllConst.CONST_MTTank
                    '★根岸営業所の場合
                    If work.WF_SEL_SALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011402 Then
                        '### 2020/06/15 START ########################################################
                        '★JXTG北信油槽所, 及びJXTG甲府油槽所の場合
                        If work.WF_SEL_CONSIGNEECODE.Text = BaseDllConst.CONST_CONSIGNEECODE_10 _
                            OrElse work.WF_SEL_CONSIGNEECODE.Text = BaseDllConst.CONST_CONSIGNEECODE_20 Then
                            '入力を許可する。
                            Me.TxtMTTank.Enabled = True
                        Else
                            Me.TxtMTTank.Enabled = False
                        End If
                        '### 2020/06/15 END   ########################################################
                    Else
                        TxtMTTank.Enabled = True
                    End If
                    '軽油
                Case BaseDllConst.CONST_KTank1, BaseDllConst.CONST_KTank2
                    TxtKTank.Enabled = True
                    '３号軽油
                Case BaseDllConst.CONST_K3Tank1, BaseDllConst.CONST_K3Tank2
                    TxtK3Tank.Enabled = True
                    '軽油５
                Case BaseDllConst.CONST_K5Tank
                    TxtK5Tank.Enabled = True
                    '軽油１０
                Case BaseDllConst.CONST_K10Tank
                    TxtK10Tank.Enabled = True
                    'ＬＳＡ
                Case BaseDllConst.CONST_LTank1, BaseDllConst.CONST_LTank2
                    '### 20200706 START((全体)No100対応) ##########################################
                    'TxtLTank.Enabled = True
                    '★OT八王子の場合
                    If work.WF_SEL_CONSIGNEECODE.Text = BaseDllConst.CONST_CONSIGNEECODE_55 Then
                        Me.TxtLTank.Enabled = False
                        Me.TxtLTank.Text = 0
                    Else
                        Me.TxtLTank.Enabled = True
                    End If
                    '### 20200706 END  ((全体)No100対応) ##########################################
                    'Ａ重油
                Case BaseDllConst.CONST_ATank
                    '### 20200706 START((全体)No100対応) ##########################################
                    'TxtATank.Enabled = True
                    '★OT八王子の場合
                    If work.WF_SEL_CONSIGNEECODE.Text = BaseDllConst.CONST_CONSIGNEECODE_55 Then
                        Me.TxtATank.Enabled = False
                        Me.TxtATank.Text = 0
                    Else
                        Me.TxtATank.Enabled = True
                    End If
                    '### 20200706 END  ((全体)No100対応) ##########################################
            End Select
        Next
    End Sub


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
            & "        OFFICECODE      = @P04    , OFFICENAME     = @P05" _
            & "        , TRAINNO       = @P02    , TRAINNAME      = @P93, ORDERTYPE  = @P06" _
            & "        , SHIPPERSCODE  = @P07    , SHIPPERSNAME   = @P08" _
            & "        , BASECODE      = @P09    , BASENAME       = @P10" _
            & "        , CONSIGNEECODE = @P11    , CONSIGNEENAME  = @P12" _
            & "        , DEPSTATION    = @P13    , DEPSTATIONNAME = @P14" _
            & "        , ARRSTATION    = @P15    , ARRSTATIONNAME = @P16" _
            & "        , ORDERINFO     = @P22    , STACKINGFLG    = @P92" _
            & "        , USEPROPRIETYFLG = @P23  , DELIVERYFLG    = @P94" _
            & "        , LODDATE       = @P24    , DEPDATE        = @P25" _
            & "        , ARRDATE       = @P26    , ACCDATE        = @P27, EMPARRDATE = @P28" _
            & "        , UPDYMD        = @P87    , UPDUSER        = @P88" _
            & "        , UPDTERMID     = @P89    , RECEIVEYMD     = @P90" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0002_ORDER" _
            & "        ( ORDERNO      , TRAINNO         , TRAINNAME       , ORDERYMD            , OFFICECODE , OFFICENAME" _
            & "        , ORDERTYPE    , SHIPPERSCODE    , SHIPPERSNAME    , BASECODE            , BASENAME" _
            & "        , CONSIGNEECODE, CONSIGNEENAME   , DEPSTATION      , DEPSTATIONNAME      , ARRSTATION , ARRSTATIONNAME" _
            & "        , RETSTATION   , RETSTATIONNAME  , CHANGERETSTATION, CHANGERETSTATIONNAME, ORDERSTATUS, ORDERINFO    " _
            & "        , EMPTYTURNFLG , STACKINGFLG     , USEPROPRIETYFLG , CONTACTFLG          , RESULTFLG  , DELIVERYFLG" _
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
            & "        , PAYMENT      , PAYMENTTAX      , TOTALPAYMENT    , DELFLG" _
            & "        , INITYMD      , INITUSER        , INITTERMID" _
            & "        , UPDYMD       , UPDUSER         , UPDTERMID       , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P93, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08, @P09, @P10" _
            & "        , @P11, @P12, @P13, @P14, @P15, @P16" _
            & "        , @P17, @P18, @P19, @P20, @P21, @P22" _
            & "        , @P95, @P92, @P23, @P96, @P97, @P94" _
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
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIT0002_ORDER" _
            & " WHERE" _
            & "        ORDERNO      = @P01"

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

                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    'If Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                    Dim WW_DATENOW As DateTime = Date.Now

                    'DB更新
                    PARA01.Value = work.WF_SEL_ORDERNUMBER.Text       '受注№
                    'PARA01.Value = OIT0001row("ORDERNO")              '受注№
                    PARA02.Value = Me.TxtHeadOfficeTrain.Text         '本線列車
                    PARA93.Value = Me.TxtHeadOfficeTrainName.Text     '本線列車名
                    PARA03.Value = OIT0001row("ORDERYMD")             '受注登録日
                    PARA04.Value = work.WF_SEL_SALESOFFICECODE.Text   '受注営業所コード
                    PARA05.Value = work.WF_SEL_SALESOFFICE.Text       '受注営業所名
                    PARA06.Value = work.WF_SEL_PATTERNCODE.Text       '受注パターン
                    PARA07.Value = work.WF_SEL_SHIPPERSCODE.Text      '荷主コード
                    PARA08.Value = work.WF_SEL_SHIPPERSNAME.Text      '荷主名
                    PARA09.Value = work.WF_SEL_BASECODE.Text          '基地コード
                    PARA10.Value = work.WF_SEL_BASENAME.Text          '基地名
                    PARA11.Value = work.WF_SEL_CONSIGNEECODE.Text     '荷受人コード
                    PARA12.Value = work.WF_SEL_CONSIGNEENAME.Text     '荷受人名
                    PARA13.Value = TxtDepstation.Text                 '発駅コード
                    PARA14.Value = LblDepstationName.Text             '発駅名
                    PARA15.Value = TxtArrstation.Text                 '着駅コード
                    PARA16.Value = LblArrstationName.Text             '着駅名
                    PARA17.Value = ""                                 '空車着駅コード
                    PARA18.Value = ""                                 '空車着駅名
                    PARA19.Value = ""                                 '空車着駅コード(変更後)
                    PARA20.Value = ""                                 '空車着駅名(変更後)

                    ''#受注進行ステータス
                    'If OIT0001row("ORDERSTATUS") = "" Then
                    '    '受注進行ステータス(100:受注受付)
                    '    PARA21.Value = "100"
                    'Else
                    '    PARA21.Value = OIT0001row("ORDERSTATUS")
                    'End If

                    '受注進行ステータス(100:受注受付)
                    PARA21.Value = "100"

                    '# 受注情報
                    '交付アラートが「3日以内のタンク車」または「4日～6日のタンク車」の場合
                    If OIT0001row("JRINSPECTIONALERTSTR").Equals(C_INSPECTIONALERT.ALERT_YELLOW) _
                        OrElse OIT0001row("JRINSPECTIONALERTSTR").Equals(C_INSPECTIONALERT.ALERT_RED) Then
                        WW_ORDERINFOALERMFLG_82 = True

                        '全検アラートが「3日以内のタンク車」または「4日～6日のタンク車」の場合
                    ElseIf OIT0001row("JRALLINSPECTIONALERTSTR").Equals(C_INSPECTIONALERT.ALERT_YELLOW) _
                        OrElse OIT0001row("JRALLINSPECTIONALERTSTR").Equals(C_INSPECTIONALERT.ALERT_RED) Then
                        WW_ORDERINFOALERMFLG_82 = True

                    End If

                    '〇 交付アラート、または全検アラートが1件でも警告以上の場合
                    If WW_ORDERINFOALERMFLG_82 = True Then
                        PARA22.Value = BaseDllConst.CONST_ORDERINFO_ALERT_82

                        '    '〇 積込日 < 発日 の場合 
                        'ElseIf WW_ORDERINFOFLG_10 = True AndAlso WW_ORDERINFOALERMFLG_82 = False Then
                        '    PARA22.Value = BaseDllConst.CONST_ORDERINFO_10

                        '〇 上記以外
                    ElseIf WW_ORDERINFOALERMFLG_82 = False Then
                        PARA22.Value = ""

                    End If

                    PARA95.Value = "1"                                '空回日報可否フラグ(1:作成)

                    '〇 積込日 < 発日 の場合 
                    If WW_ORDERINFOFLG_10 = True Then
                        PARA92.Value = "1"                                '積置可否フラグ(1:積置あり)
                    Else
                        PARA92.Value = "2"                                '積置可否フラグ(2:積置なし)
                    End If

                    PARA23.Value = "1"                                '利用可否フラグ(1:利用可能)
                    PARA96.Value = "0"                                '手配連絡フラグ(0:未連絡)
                    PARA97.Value = "0"                                '結果受理フラグ(0:未受理)
                    PARA94.Value = "0"                                '託送指示フラグ(0:未手配)
                    PARA24.Value = TxtLoadingDate.Text                '積込日（予定）
                    PARA25.Value = TxtDepDate.Text                    '発日（予定）
                    PARA26.Value = TxtArrDate.Text                    '積車着日（予定）
                    PARA27.Value = TxtAccDate.Text                    '受入日（予定）
                    PARA28.Value = TxtEmparrDate.Text                 '空車着日（予定）
                    PARA29.Value = DBNull.Value                       '積込日（実績）
                    PARA30.Value = DBNull.Value                       '発日（実績）
                    PARA31.Value = DBNull.Value                       '積車着日（実績）
                    PARA32.Value = DBNull.Value                       '受入日（実績）
                    PARA33.Value = DBNull.Value                       '空車着日（実績）
                    PARA34.Value = "0"                                '車数（レギュラー）
                    PARA35.Value = "0"                                '車数（ハイオク）
                    PARA36.Value = "0"                                '車数（灯油）
                    PARA37.Value = "0"                                '車数（未添加灯油）
                    PARA38.Value = "0"                                '車数（軽油）
                    PARA39.Value = "0"                                '車数（３号軽油）
                    PARA40.Value = "0"                                '車数（５号軽油）
                    PARA41.Value = "0"                                '車数（１０号軽油）
                    PARA42.Value = "0"                                '車数（LSA）
                    PARA43.Value = "0"                                '車数（A重油）
                    PARA44.Value = "0"                                '車数（その他１）
                    PARA45.Value = "0"                                '車数（その他２）
                    PARA46.Value = "0"                                '車数（その他３）
                    PARA47.Value = "0"                                '車数（その他４）
                    PARA48.Value = "0"                                '車数（その他５）
                    PARA49.Value = "0"                                '車数（その他６）
                    PARA50.Value = "0"                                '車数（その他７）
                    PARA51.Value = "0"                                '車数（その他８）
                    PARA52.Value = "0"                                '車数（その他９）
                    PARA53.Value = "0"                                '車数（その他１０）
                    PARA54.Value = "0"                                '合計車数
                    PARA55.Value = "0"                                '変更後_車数（レギュラー）
                    PARA56.Value = "0"                                '変更後_車数（ハイオク）
                    PARA57.Value = "0"                                '変更後_車数（灯油）
                    PARA58.Value = "0"                                '変更後_車数（未添加灯油）
                    PARA59.Value = "0"                                '変更後_車数（軽油）
                    PARA60.Value = "0"                                '変更後_車数（３号軽油）
                    PARA61.Value = "0"                                '変更後_車数（５号軽油）
                    PARA62.Value = "0"                                '変更後_車数（１０号軽油）
                    PARA63.Value = "0"                                '変更後_車数（LSA）
                    PARA64.Value = "0"                                '変更後_車数（A重油）
                    PARA65.Value = "0"                                '変更後_車数（その他１）
                    PARA66.Value = "0"                                '変更後_車数（その他２）
                    PARA67.Value = "0"                                '変更後_車数（その他３）
                    PARA68.Value = "0"                                '変更後_車数（その他４）
                    PARA69.Value = "0"                                '変更後_車数（その他５）
                    PARA70.Value = "0"                                '変更後_車数（その他６）
                    PARA71.Value = "0"                                '変更後_車数（その他７）
                    PARA72.Value = "0"                                '変更後_車数（その他８）
                    PARA73.Value = "0"                                '変更後_車数（その他９）
                    PARA74.Value = "0"                                '変更後_車数（その他１０）
                    PARA75.Value = "0"                                '変更後_合計車数
                    PARA76.Value = ""                                 '貨車連結順序表№
                    PARA91.Value = DBNull.Value                       '計上日
                    PARA77.Value = "0"                                '売上金額
                    PARA78.Value = "0"                                '売上消費税額
                    PARA79.Value = "0"                                '売上合計金額
                    PARA80.Value = "0"                                '支払金額
                    PARA81.Value = "0"                                '支払消費税額
                    PARA82.Value = "0"                                '支払合計金額
                    PARA83.Value = OIT0001row("DELFLG")               '削除フラグ
                    PARA84.Value = WW_DATENOW                         '登録年月日
                    PARA85.Value = Master.USERID                      '登録ユーザーID
                    PARA86.Value = Master.USERTERMID                  '登録端末
                    PARA87.Value = WW_DATENOW                         '更新年月日
                    PARA88.Value = Master.USERID                      '更新ユーザーID
                    PARA89.Value = Master.USERTERMID                  '更新端末
                    PARA90.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = work.WF_SEL_ORDERNUMBER.Text

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0001UPDtbl) Then
                            OIT0001UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0001UPDtbl.Clear()
                        OIT0001UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0001UPDrow As DataRow In OIT0001UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0001L"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0001UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D UPDATE_INSERT_ORDER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D UPDATE_INSERT_ORDER"
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
            & "        TANKNO            = @P03, STACKINGFLG  = @P40" _
            & "        , SHIPPERSCODE    = @P23, SHIPPERSNAME = @P24" _
            & "        , OILCODE         = @P05, OILNAME      = @P35, ORDERINGTYPE = @P36, ORDERINGOILNAME = @P37" _
            & "        , RETURNDATETRAIN = @P07, JOINTCODE    = @P39, JOINT        = @P08, REMARK       = @P38" _
            & "        , ACTUALLODDATE   = @P41" _
            & "        , UPDYMD          = @P19, UPDUSER      = @P20" _
            & "        , UPDTERMID       = @P21, RECEIVEYMD   = @P22" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & "        AND DETAILNO     = @P02" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0003_DETAIL" _
            & "        ( ORDERNO         , DETAILNO" _
            & "        , TANKNO          , KAMOKU              , STACKINGFLG        , ORDERINFO" _
            & "        , SHIPPERSCODE    , SHIPPERSNAME        , OILCODE            , OILNAME" _
            & "        , ORDERINGTYPE    , ORDERINGOILNAME     , CARSNUMBER         , CARSAMOUNT          " _
            & "        , RETURNDATETRAIN , JOINTCODE           , JOINT" _
            & "        , REMARK          , CHANGETRAINNO       , SECONDCONSIGNEECODE, SECONDCONSIGNEENAME" _
            & "        , SECONDARRSTATION, SECONDARRSTATIONNAME, CHANGERETSTATION   , CHANGERETSTATIONNAME" _
            & "        , ACTUALLODDATE   , SALSE               , SALSETAX" _
            & "        , TOTALSALSE      , PAYMENT             , PAYMENTTAX         , TOTALPAYMENT" _
            & "        , DELFLG          , INITYMD             , INITUSER           , INITTERMID" _
            & "        , UPDYMD          , UPDUSER             , UPDTERMID          , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02" _
            & "        , @P03, @P04, @P40, @P34" _
            & "        , @P23, @P24, @P05, @P35" _
            & "        , @P36, @P37, @P06, @P25" _
            & "        , @P07, @P39, @P08" _
            & "        , @P38, @P26, @P27, @P28" _
            & "        , @P29, @P30, @P31, @P32" _
            & "        , @P41, @P09, @P10" _
            & "        , @P11, @P12, @P13, @P14" _
            & "        , @P15, @P16, @P17, @P18" _
            & "        , @P19, @P20, @P21, @P22) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '& "   AND  TANKNO   = @P03" _
        '& "   AND  KAMOKU   = @P04" _
        '& "        AND TANKNO       = @P03" _
        '& "        AND KAMOKU       = @P04" _

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    ORDERNO" _
            & "    , DETAILNO" _
            & "    , TANKNO" _
            & "    , KAMOKU" _
            & "    , STACKINGFLG" _
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
            & "    , REMARK" _
            & "    , CHANGETRAINNO" _
            & "    , SECONDCONSIGNEECODE" _
            & "    , SECONDCONSIGNEENAME" _
            & "    , SECONDARRSTATION" _
            & "    , SECONDARRSTATIONNAME" _
            & "    , CHANGERETSTATION" _
            & "    , CHANGERETSTATIONNAME" _
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
            & "    , UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIT0003_DETAIL" _
            & " WHERE" _
            & "        ORDERNO  = @P01" _
            & "   AND  DETAILNO = @P02"
        '& "   AND  TANKNO   = @P03" _
        '& "   AND  KAMOKU   = @P04"

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
                'Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 2)   '入線順
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 8)   'タンク車№
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 7)   '費用科目
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.NVarChar, 1)   '積置可否フラグ
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.NVarChar, 2)   '受注情報
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 10)  '荷主コード
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 10)  '荷主名
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 4)   '油種コード
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.NVarChar, 40)  '油種名
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.NVarChar, 2)   '油種区分(受発注用)
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.NVarChar, 40)  '油種名(受発注用)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Int)           '車数
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Int)           '数量
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 4)   '返送日列車
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.NVarChar, 40)  'ジョイントコード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 200) 'ジョイント
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.NVarChar)      '記事欄
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 4)   '本線列車（変更後）
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 10)  '第2荷受人コード
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.NVarChar, 40)  '第2荷受人名
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 7)   '第2着駅コード
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 40)  '第2着駅名
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.NVarChar, 7)   '空車着駅コード（変更後）
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 40)  '空車着駅名（変更後）
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.Date)          '積込日（実績）
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
                'Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 8) 'タンク車№
                'Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 7) '費用科目

                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    'If Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                    Dim WW_DATENOW As DateTime = Date.Now

                    'DB更新
                    PARA01.Value = work.WF_SEL_ORDERNUMBER.Text       '受注№
                    'PARA01.Value = OIT0001row("ORDERNO")             '受注№
                    PARA02.Value = OIT0001row("DETAILNO")             '受注明細№
                    'PARA33.Value = ""                                 '入線順
                    PARA03.Value = OIT0001row("TANKNO")               'タンク車№
                    PARA04.Value = OIT0001row("KAMOKU")               '費用科目

                    '# 積置可否フラグ(1:積置あり 2:積置なし)
                    If OIT0001row("STACKINGFLG") = "on" Then
                        PARA40.Value = "1"
                    Else
                        PARA40.Value = "2"
                    End If

                    PARA34.Value = ""                                 '受注情報
                    PARA23.Value = OIT0001row("SHIPPERSCODE")         '荷主コード
                    PARA24.Value = OIT0001row("SHIPPERSNAME")         '荷主名
                    PARA05.Value = OIT0001row("OILCODE")              '油種コード
                    PARA35.Value = OIT0001row("OILNAME")              '油種名
                    PARA36.Value = OIT0001row("ORDERINGTYPE")         '油種区分(受発注用)
                    PARA37.Value = OIT0001row("ORDERINGOILNAME")      '油種名(受発注用)
                    PARA06.Value = "1"                                '車数
                    PARA25.Value = "0"                                '数量
                    If Convert.ToString(OIT0001row("RETURNDATETRAIN")) <> "" Then
                        PARA07.Value = OIT0001row("RETURNDATETRAIN")  '返送日列車
                    Else
                        PARA07.Value = DBNull.Value
                    End If
                    PARA39.Value = OIT0001row("JOINTCODE")            'ジョイントコード
                    PARA08.Value = OIT0001row("JOINT")                'ジョイント
                    PARA38.Value = OIT0001row("REMARK")               '記事欄
                    PARA26.Value = ""                                 '本線列車（変更後）
                    PARA27.Value = ""                                 '第2荷受人コード
                    PARA28.Value = ""                                 '第2荷受人名
                    PARA29.Value = ""                                 '第2着駅コード
                    PARA30.Value = ""                                 '第2着駅名
                    PARA31.Value = ""                                 '空車着駅コード（変更後）
                    PARA32.Value = ""                                 '空車着駅名（変更後）

                    '積込日(実績)
                    If OIT0001row("ACTUALLODDATE") = "" Then
                        PARA41.Value = DBNull.Value
                    Else
                        PARA41.Value = OIT0001row("ACTUALLODDATE")
                    End If

                    PARA09.Value = "0"                                '売上金額
                    PARA10.Value = "0"                                '売上消費税額
                    PARA11.Value = "0"                                '売上合計金額
                    PARA12.Value = "0"                                '支払金額
                    PARA13.Value = "0"                                '支払消費税額
                    PARA14.Value = "0"                                '支払合計金額
                    PARA15.Value = OIT0001row("DELFLG")               '削除フラグ
                    PARA16.Value = WW_DATENOW                         '登録年月日
                    PARA17.Value = Master.USERID                      '登録ユーザーID
                    PARA18.Value = Master.USERTERMID                  '登録端末
                    PARA19.Value = WW_DATENOW                         '更新年月日
                    PARA20.Value = Master.USERID                      '更新ユーザーID
                    PARA21.Value = Master.USERTERMID                  '更新端末
                    PARA22.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                    JPARA01.Value = OIT0001row("ORDERNO")             '受注№
                    JPARA02.Value = OIT0001row("DETAILNO")            '受注明細№
                    'JPARA03.Value = OIT0001row("TANKNO")              'タンク車№
                    'JPARA04.Value = OIT0001row("KAMOKU")              '費用科目

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0001UPDtbl) Then
                            OIT0001UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0001UPDtbl.Clear()
                        OIT0001UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0001UPDrow As DataRow In OIT0001UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0001L"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0001UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D UPDATE_INSERT_ORDERDETAIL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D UPDATE_INSERT_ORDERDETAIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (受注TBL)受注進行ステータス更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderStatus(ByVal I_Value As String,
                                       Optional ByVal InitializeFlg As Boolean = False)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLの受注進行ステータス、及び貨車連結順序表№を更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0002_ORDER " _
                    & "    SET ORDERSTATUS = @P03, "
            '& "        TANKLINKNO  = @P04, "

            '### 20200609 START(内部No178) #################################################
            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            If InitializeFlg = True Then
                '空回日報可否フラグ
                ' 0：未作成, 1：作成(空回日報から作成), 2：作成(在庫管理から作成)
                SQLStr &= String.Format("        EMPTYTURNFLG = '{0}', ", "1")
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
            'Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA01.Value = work.WF_SEL_ORDERNUMBER.Text
            PARA02.Value = C_DELETE_FLG.DELETE
            PARA03.Value = I_Value
            'PARA04.Value = work.WF_SEL_LINK_LINKNO.Text

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D_ORDERSTATUS UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D_ORDERSTATUS UPDATE"
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
    Protected Sub WW_UpdateOrderInfo(ByVal SQLcon As SqlConnection, ByVal I_TYPE As String, ByVal OIT0001row As DataRow)

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

            PARA01.Value = OIT0001row("ORDERNO")
            PARA02.Value = OIT0001row("DETAILNO")
            PARA03.Value = C_DELETE_FLG.DELETE
            PARA04.Value = OIT0001row("ORDERINFO")

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D_ORDERINFO UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D_ORDERINFO UPDATE"
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
    ''' (受注TBL)タンク車数更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderTankCnt(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

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

        'Dim SQLStr As String =
        '      " SELECT" _
        '    & "  VIEW_TANKCNT.ORDERNO   AS ORDERNO" _
        '    & " , VIEW_TANKCNT.OILCODE  AS OILCODE" _
        '    & " , VIEW_TANKCNT.OILNAME  AS OILNAME" _
        '    & " , VIEW_TANKCNT.TANKTYPE AS TANKTYPE" _
        '    & " , COUNT(1)              AS CNT" _
        '    & " FROM (" _
        '    & "  SELECT " _
        '    & "  ISNULL(RTRIM(OIT0003.ORDERNO), '')   AS ORDERNO" _
        '    & "  , ISNULL(RTRIM(OIM0003.OILCODE), '') AS OILCODE" _
        '    & "  , ISNULL(RTRIM(OIM0003.OILNAME), '') AS OILNAME" _
        '    & "  , CASE" _
        '    & "    WHEN OIM0003.OILCODE = '1001' THEN 'HTANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1101' THEN 'RTANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1301' THEN 'TTANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1302' THEN 'MTTANK'" _
        '    & "    WHEN OIM0003.OILCODE IN ('1401','1406') THEN 'KTANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1402' THEN 'K5TANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1403' THEN 'K10TANK'" _
        '    & "    WHEN OIM0003.OILCODE IN ('1404','1405') THEN 'K3TANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1501' THEN ''" _
        '    & "    WHEN OIM0003.OILCODE = '2101' THEN 'ATANK'" _
        '    & "    WHEN OIM0003.OILCODE IN ('2201','2202') THEN 'LTANK'" _
        '    & "    END TANKTYPE" _
        '    & "  FROM OIL.OIT0003_DETAIL OIT0003 " _
        '    & "  INNER JOIN OIL.OIM0003_PRODUCT OIM0003 ON " _
        '    & "        OIM0003.OFFICECODE = @P02 " _
        '    & "    AND OIM0003.OILCODE    = OIT0003.OILCODE " _
        '    & "  WHERE OIT0003.ORDERNO = @P01" _
        '    & "    AND OIT0003.DELFLG <> @P03" _
        '    & " ) VIEW_TANKCNT" _
        '    & " GROUP BY VIEW_TANKCNT.ORDERNO, VIEW_TANKCNT.OILCODE, VIEW_TANKCNT.OILNAME, VIEW_TANKCNT.TANKTYPE"

        '更新SQL文･･･受注TBLのタンク車数を更新
        Dim SQLUpStr As String =
                    " UPDATE OIL.OIT0002_ORDER           " _
                    & "    SET HTANK        = @P11,      " _
                    & "        RTANK        = @P12,      " _
                    & "        TTANK        = @P13,      " _
                    & "        MTTANK       = @P14,      " _
                    & "        KTANK        = @P15,      " _
                    & "        K3TANK       = @P16,      " _
                    & "        K5TANK       = @P17,      " _
                    & "        K10TANK      = @P18,      " _
                    & "        LTANK        = @P19,      " _
                    & "        ATANK        = @P20,      " _
                    & "        OTHER1OTANK  = @P21,      " _
                    & "        OTHER2OTANK  = @P22,      " _
                    & "        OTHER3OTANK  = @P23,      " _
                    & "        OTHER4OTANK  = @P24,      " _
                    & "        OTHER5OTANK  = @P25,      " _
                    & "        OTHER6OTANK  = @P26,      " _
                    & "        OTHER7OTANK  = @P27,      " _
                    & "        OTHER8OTANK  = @P28,      " _
                    & "        OTHER9OTANK  = @P29,      " _
                    & "        OTHER10OTANK = @P30,      " _
                    & "        TOTALTANK    = @P31,      " _
                    & "        HTANKCH        = @P37,      " _
                    & "        RTANKCH        = @P38,      " _
                    & "        TTANKCH        = @P39,      " _
                    & "        MTTANKCH       = @P40,      " _
                    & "        KTANKCH        = @P41,      " _
                    & "        K3TANKCH       = @P42,      " _
                    & "        K5TANKCH       = @P43,      " _
                    & "        K10TANKCH      = @P44,      " _
                    & "        LTANKCH        = @P45,      " _
                    & "        ATANKCH        = @P46,      " _
                    & "        OTHER1OTANKCH  = @P47,      " _
                    & "        OTHER2OTANKCH  = @P48,      " _
                    & "        OTHER3OTANKCH  = @P49,      " _
                    & "        OTHER4OTANKCH  = @P50,      " _
                    & "        OTHER5OTANKCH  = @P51,      " _
                    & "        OTHER6OTANKCH  = @P52,      " _
                    & "        OTHER7OTANKCH  = @P53,      " _
                    & "        OTHER8OTANKCH  = @P54,      " _
                    & "        OTHER9OTANKCH  = @P55,      " _
                    & "        OTHER10OTANKCH = @P56,      " _
                    & "        TOTALTANKCH    = @P57,      " _
                    & "        UPDYMD       = @P32,      " _
                    & "        UPDUSER      = @P33,      " _
                    & "        UPDTERMID    = @P34,      " _
                    & "        RECEIVEYMD   = @P35,      " _
                    & "        ORDERINFO    = @P36       " _
                    & "  WHERE ORDERNO      = @P01       " _
                    & "    AND OFFICECODE   = @P02       " _
                    & "    AND DELFLG      <> @P03      ;"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLUpcmd As New SqlCommand(SQLUpStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA2.Value = work.WF_SEL_SALESOFFICECODE.Text
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
                '### 20200812 START 変更後の車数にも同様に件数を設定 ####################################################
                Dim PARAUP37 As SqlParameter = SQLUpcmd.Parameters.Add("@P37", SqlDbType.Int)          '変更後_車数（ハイオク）
                Dim PARAUP38 As SqlParameter = SQLUpcmd.Parameters.Add("@P38", SqlDbType.Int)          '変更後_車数（レギュラー）
                Dim PARAUP39 As SqlParameter = SQLUpcmd.Parameters.Add("@P39", SqlDbType.Int)          '変更後_車数（灯油）
                Dim PARAUP40 As SqlParameter = SQLUpcmd.Parameters.Add("@P40", SqlDbType.Int)          '変更後_車数（未添加灯油）
                Dim PARAUP41 As SqlParameter = SQLUpcmd.Parameters.Add("@P41", SqlDbType.Int)          '変更後_車数（軽油）
                Dim PARAUP42 As SqlParameter = SQLUpcmd.Parameters.Add("@P42", SqlDbType.Int)          '変更後_車数（３号軽油）
                Dim PARAUP43 As SqlParameter = SQLUpcmd.Parameters.Add("@P43", SqlDbType.Int)          '変更後_車数（５号軽油）
                Dim PARAUP44 As SqlParameter = SQLUpcmd.Parameters.Add("@P44", SqlDbType.Int)          '変更後_車数（１０号軽油）
                Dim PARAUP45 As SqlParameter = SQLUpcmd.Parameters.Add("@P45", SqlDbType.Int)          '変更後_車数（LSA）
                Dim PARAUP46 As SqlParameter = SQLUpcmd.Parameters.Add("@P46", SqlDbType.Int)          '変更後_車数（A重油）
                Dim PARAUP47 As SqlParameter = SQLUpcmd.Parameters.Add("@P47", SqlDbType.Int)          '変更後_車数（その他１）
                Dim PARAUP48 As SqlParameter = SQLUpcmd.Parameters.Add("@P48", SqlDbType.Int)          '変更後_車数（その他２）
                Dim PARAUP49 As SqlParameter = SQLUpcmd.Parameters.Add("@P49", SqlDbType.Int)          '変更後_車数（その他３）
                Dim PARAUP50 As SqlParameter = SQLUpcmd.Parameters.Add("@P50", SqlDbType.Int)          '変更後_車数（その他４）
                Dim PARAUP51 As SqlParameter = SQLUpcmd.Parameters.Add("@P51", SqlDbType.Int)          '変更後_車数（その他５）
                Dim PARAUP52 As SqlParameter = SQLUpcmd.Parameters.Add("@P52", SqlDbType.Int)          '変更後_車数（その他６）
                Dim PARAUP53 As SqlParameter = SQLUpcmd.Parameters.Add("@P53", SqlDbType.Int)          '変更後_車数（その他７）
                Dim PARAUP54 As SqlParameter = SQLUpcmd.Parameters.Add("@P54", SqlDbType.Int)          '変更後_車数（その他８）
                Dim PARAUP55 As SqlParameter = SQLUpcmd.Parameters.Add("@P55", SqlDbType.Int)          '変更後_車数（その他９）
                Dim PARAUP56 As SqlParameter = SQLUpcmd.Parameters.Add("@P56", SqlDbType.Int)          '変更後_車数（その他１０）
                Dim PARAUP57 As SqlParameter = SQLUpcmd.Parameters.Add("@P57", SqlDbType.Int)          '変更後_合計車数
                '### 20200812 END   変更後の車数にも同様に件数を設定 ####################################################
                Dim PARAUP32 As SqlParameter = SQLUpcmd.Parameters.Add("@P32", SqlDbType.DateTime)
                Dim PARAUP33 As SqlParameter = SQLUpcmd.Parameters.Add("@P33", SqlDbType.NVarChar)
                Dim PARAUP34 As SqlParameter = SQLUpcmd.Parameters.Add("@P34", SqlDbType.NVarChar)
                Dim PARAUP35 As SqlParameter = SQLUpcmd.Parameters.Add("@P35", SqlDbType.DateTime)
                Dim PARAUP36 As SqlParameter = SQLUpcmd.Parameters.Add("@P36", SqlDbType.NVarChar)     '受注情報
                PARAUP01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARAUP02.Value = work.WF_SEL_SALESOFFICECODE.Text
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
                '### 20200812 START 変更後の車数にも同様に件数を設定 ####################################################
                PARAUP37.Value = "0"
                PARAUP38.Value = "0"
                PARAUP39.Value = "0"
                PARAUP40.Value = "0"
                PARAUP41.Value = "0"
                PARAUP42.Value = "0"
                PARAUP43.Value = "0"
                PARAUP44.Value = "0"
                PARAUP45.Value = "0"
                PARAUP46.Value = "0"
                PARAUP47.Value = "0"
                PARAUP48.Value = "0"
                PARAUP49.Value = "0"
                PARAUP50.Value = "0"
                PARAUP51.Value = "0"
                PARAUP52.Value = "0"
                PARAUP53.Value = "0"
                PARAUP54.Value = "0"
                PARAUP55.Value = "0"
                PARAUP56.Value = "0"
                PARAUP57.Value = "0"
                '### 20200812 END   変更後の車数にも同様に件数を設定 ####################################################

                '各タンク車件数を初期化
                TxtHTank.Text = "0"
                TxtRTank.Text = "0"
                TxtTTank.Text = "0"
                TxtMTTank.Text = "0"
                TxtKTank.Text = "0"
                TxtK3Tank.Text = "0"
                TxtK5Tank.Text = "0"
                TxtK10Tank.Text = "0"
                TxtLTank.Text = "0"
                TxtATank.Text = "0"
                TxtTotalTank.Text = "0"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "TRAINNUMBER", TxtHeadOfficeTrain.Text, WW_GetValue)
                Dim cntVal As Integer = 0
                For Each OIT0001UPDrow As DataRow In OIT0001WKtbl.Rows
                    cntVal = CInt(OIT0001UPDrow("CNT"))
                    Select Case Convert.ToString(OIT0001UPDrow("OILCODE"))
                        Case BaseDllConst.CONST_HTank
                            PARAUP11.Value = OIT0001UPDrow("CNT")
                            PARAUP37.Value = OIT0001UPDrow("CNT")
                            TxtHTank.Text = cntVal.ToString
                        Case BaseDllConst.CONST_RTank
                            PARAUP12.Value = OIT0001UPDrow("CNT")
                            PARAUP38.Value = OIT0001UPDrow("CNT")
                            TxtRTank.Text = cntVal.ToString
                        Case BaseDllConst.CONST_TTank
                            PARAUP13.Value = OIT0001UPDrow("CNT")
                            PARAUP39.Value = OIT0001UPDrow("CNT")
                            TxtTTank.Text = cntVal.ToString
                        Case BaseDllConst.CONST_MTTank
                            PARAUP14.Value = OIT0001UPDrow("CNT")
                            PARAUP40.Value = OIT0001UPDrow("CNT")
                            TxtMTTank.Text = cntVal.ToString
                        Case BaseDllConst.CONST_KTank1, BaseDllConst.CONST_KTank2
                            PARAUP15.Value = OIT0001UPDrow("CNT")
                            PARAUP41.Value = OIT0001UPDrow("CNT")
                            TxtKTank.Text = cntVal.ToString
                        Case BaseDllConst.CONST_K3Tank1, BaseDllConst.CONST_K3Tank2
                            PARAUP16.Value = OIT0001UPDrow("CNT")
                            PARAUP42.Value = OIT0001UPDrow("CNT")
                            TxtK3Tank.Text = cntVal.ToString
                        Case BaseDllConst.CONST_K5Tank
                            PARAUP17.Value = OIT0001UPDrow("CNT")
                            PARAUP43.Value = OIT0001UPDrow("CNT")
                            TxtK5Tank.Text = cntVal.ToString
                        Case BaseDllConst.CONST_K10Tank
                            PARAUP18.Value = OIT0001UPDrow("CNT")
                            PARAUP44.Value = OIT0001UPDrow("CNT")
                            TxtK10Tank.Text = cntVal.ToString
                        Case BaseDllConst.CONST_LTank1, BaseDllConst.CONST_LTank2
                            PARAUP19.Value = OIT0001UPDrow("CNT")
                            PARAUP45.Value = OIT0001UPDrow("CNT")
                            TxtLTank.Text = cntVal.ToString
                        Case BaseDllConst.CONST_ATank
                            PARAUP20.Value = OIT0001UPDrow("CNT")
                            PARAUP46.Value = OIT0001UPDrow("CNT")
                            TxtATank.Text = cntVal.ToString
                    End Select

                    i += cntVal
                    TxtTotalTank.Text = i.ToString
                    PARAUP31.Value = i
                    '### 20200812 START 変更後の車数にも同様に件数を設定 ####################################################
                    PARAUP57.Value = i
                    '### 20200812 END   変更後の車数にも同様に件数を設定 ####################################################
                    PARAUP32.Value = Date.Now
                    PARAUP33.Value = Master.USERID
                    PARAUP34.Value = Master.USERTERMID
                    PARAUP35.Value = C_DEFAULT_YMD

                    '受付情報が「検査間近有」の場合は優先して設定
                    If WW_ORDERINFOALERMFLG_82 = True Then
                        PARAUP36.Value = BaseDllConst.CONST_ORDERINFO_ALERT_82

                        'タンク車数が「最大牽引タンク車数」より大きい場合
                    ElseIf Integer.Parse(TxtTotalTank.Text) > Integer.Parse(WW_GetValue(3)) Then
                        '80(タンク車数オーバー)を設定
                        PARAUP36.Value = BaseDllConst.CONST_ORDERINFO_ALERT_80

                    ElseIf Integer.Parse(TxtTotalTank.Text) <= Integer.Parse(WW_GetValue(3)) Then
                        PARAUP36.Value = ""

                    End If

                    SQLUpcmd.ExecuteNonQuery()
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D ORDERTANKCNTSET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D ORDERTANKCNTSET"
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D DELETEORDER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D DELETEORDER"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 空回日報(一覧)表示用
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_OrderListTBLSet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

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
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')   　            AS ORDERNO" _
            & " , ISNULL(FORMAT(OIT0002.ORDERYMD, 'yyyy/MM/dd'), '') AS ORDERYMD" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')             AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')             AS ORDERSTATUSNAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERINFO), '')               AS ORDERINFO" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')              AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.EMPTYTURNFLG), '')            AS EMPTYTURNFLG" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')                 AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')               AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')              AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')          AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')              AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')          AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.STACKINGFLG), '')             AS STACKINGFLG" _
            & " , ISNULL(FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd'), '')  AS LODDATE" _
            & " , ISNULL(FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd'), '')  AS DEPDATE" _
            & " , ISNULL(FORMAT(OIT0002.ARRDATE, 'yyyy/MM/dd'), '')  AS ARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACCDATE, 'yyyy/MM/dd'), '')  AS ACCDATE" _
            & " , ISNULL(FORMAT(OIT0002.EMPARRDATE, 'yyyy/MM/dd'), '')  AS EMPARRDATE" _
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
            & " , ISNULL(RTRIM(OIT0002.TOTALTANK), '')               AS TOTALTANK" _
            & " , ISNULL(RTRIM(OIT0002.TANKLINKNOMADE), '')          AS TANKLINKNOMADE" _
            & " , ISNULL(RTRIM(OIT0002.DELFLG), '')                  AS DELFLG" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " WHERE OIT0002.OFFICECODE   = @P1" _
            & "   AND OIT0002.LODDATE      >= @P2" _
            & "   AND OIT0002.DELFLG       <> @P3" _
            & "   AND OIT0002.EMPTYTURNFLG <> '2'"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNUMBER.Text) Then
            SQLStr &= String.Format("    AND OIT0002.TRAINNO = '{0}'", work.WF_SEL_TRAINNUMBER.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIT0002.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)     '積込日(開始)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = work.WF_SEL_SALESOFFICECODE.Text
                PARA2.Value = work.WF_SEL_LOADING.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim retVal As String = ""
                For Each OIT0001row As DataRow In OIT0001WKtbl.Rows
                    i += 1
                    OIT0001row("LINECNT") = i        'LINECNT

                    '受注進行ステータス
                    retVal = Convert.ToString(OIT0001row("ORDERSTATUS"))
                    CODENAME_get("ORDERSTATUS", Convert.ToString(OIT0001row("ORDERSTATUS")), retVal, WW_DUMMY)
                    OIT0001row("ORDERSTATUSNAME") = retVal
                    '受注情報
                    retVal = Convert.ToString(OIT0001row("ORDERINFO"))
                    CODENAME_get("ORDERINFO", Convert.ToString(OIT0001row("ORDERINFO")), retVal, WW_DUMMY)
                    OIT0001row("ORDERINFO") = retVal
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D ORDERLISTSET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D OrderListSet"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001WKtbl, work.WF_SEL_INPTBL.Text)

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIT0001row As DataRow In OIT0001tbl.Rows
            Select Case Convert.ToString(OIT0001row("OPERATION"))
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        'WF_Sel_LINECNT.Text = ""            'LINECNT

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 画面操作権限チェック
        '権限チェック(操作者がデータ内USERの更新権限があるかチェック
        '　※権限判定時点：現在
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now.ToString("yyyy/MM/dd")
        CS0025AUTHORget.ENDYMD = Date.Now.ToString("yyyy/MM/dd")
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・更新できないレコード(ユーザ更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        Dim ioVal As String = ""
        For Each OIT0001INProw As DataRow In OIT0001INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            ioVal = Convert.ToString(OIT0001INProw("DELFLG"))
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", ioVal, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            OIT0001INProw("DELFLG") = ioVal
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", Convert.ToString(OIT0001INProw("DELFLG")), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種(バリデーションチェック)
            ioVal = Convert.ToString(OIT0001INProw("OILCODE"))
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OILCODE", ioVal, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            OIT0001INProw("OILCODE") = ioVal
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "油種入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'タンク車(バリデーションチェック)
            ioVal = Convert.ToString(OIT0001INProw("TANKNO"))
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNO", ioVal, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            OIT0001INProw("TANKNO") = ioVal
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "タンク車入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If Convert.ToString(OIT0001INProw("OPERATION")) <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIT0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIT0001INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIT0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' OIT0001tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIT0001tbl_UPD()

        '○ 画面状態設定
        For Each OIT0001row As DataRow In OIT0001tbl.Rows
            Select Case Convert.ToString(OIT0001row("OPERATION"))
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIT0001INProw As DataRow In OIT0001INPtbl.Rows

            'エラーレコード読み飛ばし
            If Convert.ToString(OIT0001INProw("OPERATION")) <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIT0001INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIT0001row As DataRow In OIT0001tbl.Rows
                If OIT0001row("ORDERNO").Equals(OIT0001INProw("ORDERNO")) AndAlso
                    OIT0001row("DETAILNO").Equals(OIT0001INProw("DETAILNO")) Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIT0001row("DELFLG").Equals(OIT0001INProw("DELFLG")) AndAlso
                        OIT0001INProw("OPERATION").Equals(C_LIST_OPERATION_CODE.NODATA) Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIT0001INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIT0001INProw As DataRow In OIT0001INPtbl.Rows
            Select Case Convert.ToString(OIT0001INProw("OPERATION"))
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIT0001INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIT0001INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIT0001INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIT0001INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIT0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIT0001INProw As DataRow)

        For Each OIT0001row As DataRow In OIT0001tbl.Rows

            '同一レコードか判定
            If OIT0001INProw("ORDERNO").Equals(OIT0001row("ORDERNO")) AndAlso
                OIT0001INProw("DETAILNO").Equals(OIT0001row("DETAILNO")) Then
                '画面入力テーブル項目設定
                OIT0001INProw("LINECNT") = OIT0001row("LINECNT")
                OIT0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIT0001INProw("TIMSTP") = OIT0001row("TIMSTP")
                OIT0001INProw("SELECT") = 1
                OIT0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIT0001row.ItemArray = OIT0001INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIT0001INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIT0001row As DataRow = OIT0001tbl.NewRow
        OIT0001row.ItemArray = OIT0001INProw.ItemArray

        OIT0001row("LINECNT") = OIT0001tbl.Rows.Count + 1
        If OIT0001INProw.Item("OPERATION").Equals(C_LIST_OPERATION_CODE.UPDATING) Then
            OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIT0001row("TIMSTP") = "0"
        OIT0001row("SELECT") = 1
        OIT0001row("HIDDEN") = 0

        OIT0001tbl.Rows.Add(OIT0001row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIT0001INProw As DataRow)

        For Each OIT0001row As DataRow In OIT0001tbl.Rows

            '同一レコードか判定
            If OIT0001INProw("ORDERNO").Equals(OIT0001row("ORDERNO")) AndAlso
               OIT0001INProw("DETAILNO").Equals(OIT0001row("DETAILNO")) Then
                '画面入力テーブル項目設定
                OIT0001INProw("LINECNT") = OIT0001row("LINECNT")
                OIT0001INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIT0001INProw("TIMSTP") = OIT0001row("TIMSTP")
                OIT0001INProw("SELECT") = 1
                OIT0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIT0001row.ItemArray = OIT0001INProw.ItemArray
                Exit For
            End If
        Next

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

                Case "SALESOFFICE"      '営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "DEPSTATION"       '発駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPSTATION"))

                Case "ARRSTATION"       '着駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ARRSTATION"))

                Case "PRODUCTPATTERN"   '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN"))

                Case "ORDERSTATUS"      '受注進行ステータス
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERSTATUS"))
                Case "ORDERINFO"        '受注情報
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERINFO, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERINFO"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 受注履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    Private Sub WW_InsertOrderHistory(ByVal SQLcon As SqlConnection)
        Dim WW_GetHistoryNo() As String = {""}
        WW_FixvalueMasterSearch("", "NEWHISTORYNOGET", "", WW_GetHistoryNo)

        '◯受注履歴テーブル格納用
        If IsNothing(OIT0001His1tbl) Then
            OIT0001His1tbl = New DataTable
        End If

        If OIT0001His1tbl.Columns.Count <> 0 Then
            OIT0001His1tbl.Columns.Clear()
        End If
        OIT0001His1tbl.Clear()

        '◯受注明細履歴テーブル格納用
        If IsNothing(OIT0001His2tbl) Then
            OIT0001His2tbl = New DataTable
        End If

        If OIT0001His2tbl.Columns.Count <> 0 Then
            OIT0001His2tbl.Columns.Clear()
        End If
        OIT0001His2tbl.Clear()

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
                        OIT0001His1tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001His1tbl.Load(SQLdr)
                End Using
            End Using

            Using SQLcmd As New SqlCommand(SQLOrderDetailStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001His2tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001His2tbl.Load(SQLdr)
                End Using
            End Using

            Using tran = SQLcon.BeginTransaction
                '■受注履歴テーブル
                EntryHistory.InsertOrderHistory(SQLcon, tran, OIT0001His1tbl.Rows(0))

                '■受注明細履歴テーブル
                For Each OIT0001His2rowtbl In OIT0001His2tbl.Rows
                    EntryHistory.InsertOrderDetailHistory(SQLcon, tran, OIT0001His2rowtbl)
                Next

                'トランザクションコミット
                tran.Commit()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D ORDERHISTORY")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D ORDERHISTORY"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControl()
        '〇 (一覧)テキストボックスの制御(読取専用)
        Dim divObj = DirectCast(pnlListArea.FindControl(pnlListArea.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)
        Dim chkObjST As CheckBox = Nothing
        'LINECNTを除いたチェックボックスID
        Dim chkObjIdWOSTcnt As String = "chk" & pnlListArea.ID & "STACKINGFLG"
        'LINECNTを含むチェックボックスID
        Dim chkObjSTId As String
        Dim chkObjType As String = ""
        '　ループ内の対象データROW(これでXXX項目の値をとれるかと）
        Dim loopdr As DataRow = Nothing
        '　データテーブルの行Index
        Dim rowIdx As Integer = 0

        '### 20200812 START 指摘票対応(No120)全体 ############################################
        'For Each rowitem As TableRow In tblObj.Rows
        '    For Each cellObj As TableCell In rowitem.Controls
        '        If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "SHIPPERSNAME") _
        '            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "OILNAME") _
        '            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDERINGOILNAME") _
        '            OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "JOINT") Then
        '            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
        '        End If
        '    Next
        'Next
        '受注進行ステータスが「320：受注確定」以降の場合
        If work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_320 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_350 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_400 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_450 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_500 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_550 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_600 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_700 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_800 _
            OrElse work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_900 Then
            For Each rowitem As TableRow In tblObj.Rows
                For Each cellObj As TableCell In rowitem.Controls
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "SHIPPERSNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "OILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDERINGOILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "TANKNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ACTUALLODDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "RETURNDATETRAIN") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "JOINT") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "REMARK") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    End If
                Next

                '★積込(チェックボックス)の制御
                If OIT0001tbl.Rows.Count <> 0 Then
                    loopdr = OIT0001tbl.Rows(rowIdx)
                    chkObjSTId = chkObjIdWOSTcnt & Convert.ToString(loopdr("LINECNT"))
                    chkObjST = Nothing
                    For Each cellObj As TableCell In rowitem.Controls
                        chkObjST = DirectCast(cellObj.FindControl(chkObjSTId), CheckBox)
                        'コントロールが見つかったら脱出
                        If chkObjST IsNot Nothing Then
                            '積込可否フラグ(チェックボックス)を非活性
                            chkObjST.Enabled = False
                            Exit For
                        End If
                    Next
                End If
                rowIdx += 1
            Next
        Else
            For Each rowitem As TableRow In tblObj.Rows
                For Each cellObj As TableCell In rowitem.Controls
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "SHIPPERSNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "OILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDERINGOILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "JOINT") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")

                        '★受注営業所が仙台新港営業所以外の場合、(一覧)積込日(実績)の入力を許可しない
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ACTUALLODDATE") _
                    AndAlso work.WF_SEL_SALESOFFICECODE.Text <> BaseDllConst.CONST_OFFICECODE_010402 Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    End If
                Next

                '★タンク車の状態が「発送」の場合は、入力を許可しない。
                '★同受注Noとタンク車所在で管理している使用受注№が同じ場合(2020/08/11追加)
                '画面表示行が存在している場合
                Dim chkTankNocnt As String = "txt" & pnlListArea.ID & "TANKNO"
                Dim chkTankNo As String = ""
                If OIT0001tbl.Rows.Count <> 0 Then
                    loopdr = OIT0001tbl.Rows(rowIdx)
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
                    End If

                    '★積込(チェックボックス)の制御
                    '　受注営業所が仙台新港営業所以外の場合は許可しない
                    '　他オーダーで積込受注№が設定されている場合も許可しない
                    chkObjSTId = chkObjIdWOSTcnt & Convert.ToString(loopdr("LINECNT"))
                    chkObjType = Convert.ToString(loopdr("STACKINGORDERNO"))
                    chkObjST = Nothing
                    For Each cellObj As TableCell In rowitem.Controls
                        chkObjST = DirectCast(cellObj.FindControl(chkObjSTId), CheckBox)
                        'コントロールが見つかったら脱出
                        If chkObjST IsNot Nothing Then
                            '◯ 受注営業所が"010402"(仙台新港営業所)以外の場合
                            '### 20200626 積置受注№が設定されている場合(条件追加) #######################
                            If work.WF_SEL_SALESOFFICECODE.Text <> BaseDllConst.CONST_OFFICECODE_010402 _
                            OrElse chkObjType <> "" Then
                                '積込可否フラグ(チェックボックス)を非活性
                                chkObjST.Enabled = False
                            End If
                            Exit For
                        End If
                    Next

                    For Each cellObj As TableCell In rowitem.Controls
                        '★受注営業所が仙台新港営業所の場合、(一覧)積込日(実績)の入力を許可する
                        If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ACTUALLODDATE") _
                            AndAlso work.WF_SEL_SALESOFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_010402 Then
                            '★積置受注№が設定されている場合は、入力不可とする。
                            '　(他の受注オーダーにて積込済みのため)
                            If chkObjType <> "" Then
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                            Else
                                cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                            End If

                        End If
                    Next

                End If
                rowIdx += 1
            Next
        End If
        '### 20200812 END   指摘票対応(No120)全体 ############################################
    End Sub

End Class
''************************************************************
' 貨車連結順序表詳細画面
' 作成日  :2020/07/27
' 更新日  :2020/07/27
' 作成者  :森川
' 更新車  :森川
'
' 修正履歴:新規作成
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 貨車連結順序表詳細
''' </summary>
''' <remarks></remarks>
Public Class OIT0002LinkDetail
    Inherits Page

    '○ 検索結果格納Table
    Private OIT0002tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0002INPtbl As DataTable                              'チェック用テーブル
    Private OIT0002UPDtbl As DataTable                              '更新用テーブル
    Private OIT0002WKtbl As DataTable                               '作業用テーブル
    Private OIT0002GETtbl As DataTable                              '取得用テーブル
    Private OIT0002Reporttbl As DataTable                           '帳票用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_INIT_ROWS As Integer = 5                    '新規登録時初期行数
    Private Const CONST_SCROLLCOUNT As Integer = 7                  'マウススクロール時稼働行数
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

    Private WW_ORDERINFOALERMFLG_80 As Boolean = False                   '受注情報セット可否(警告(80:タンク車数オーバー))
    Private WW_ORDERINFOALERM_80 As String = "80"                        '受注情報(80:タンク車数オーバー)用格納
    Private WW_ORDERINFOALERMNAME_80 As String = "タンク車数オーバー"

    Private WW_ORDERINFOALERMFLG_82 As Boolean = False                   '受注情報セット可否(警告(82:検査間近あり))
    Private WW_ORDERINFOALERM_82 As String = "82"                        '受注情報(82:検査間近あり)用格納
    Private WW_ORDERINFOALERMNAME_82 As String = "検査間近あり"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0002tbl)

                    '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                    If CS0013ProfView.SetDispListTextBoxValues(OIT0002tbl, pnlListArea) Then
                        Master.SaveTable(OIT0002tbl)
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonRegister"        '登録ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_CheckBoxSELECT",
                             "WF_CheckBoxSELECTINSPECTION",
                             "WF_CheckBoxSELECTOTTRANSPORT"   'チェックボックス(選択)クリック
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
                        Case "WF_ButtonUPDATE"          '明細更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            'WF_FILEUPLOAD()
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
                If WF_PANELFLG.Value <> "1" Then
                    WF_PANELFLG.Value = "2"
                End If
            Else
                WF_CREATEFLG.Value = "2"
                If WF_PANELFLG.Value <> "1" Then
                    WF_PANELFLG.Value = "1"
                End If
            End If
        Finally
            '○ 格納Table Close
            If Not IsNothing(OIT0002tbl) Then
                OIT0002tbl.Clear()
                OIT0002tbl.Dispose()
                OIT0002tbl = Nothing
            End If

            If Not IsNothing(OIT0002INPtbl) Then
                OIT0002INPtbl.Clear()
                OIT0002INPtbl.Dispose()
                OIT0002INPtbl = Nothing
            End If

            If Not IsNothing(OIT0002UPDtbl) Then
                OIT0002UPDtbl.Clear()
                OIT0002UPDtbl.Dispose()
                OIT0002UPDtbl = Nothing
            End If

            WF_UPDERRFLG.Value = "0"
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0002WRKINC.MAPIDD
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

        '〇 (一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '登録営業所
        Me.TxtOrderOffice.Text = work.WF_SEL_OFFICENAME.Text
        '返送列車
        Me.TxtBTrainNo.Text = work.WF_SEL_BTRAINNO.Text
        Me.TxtBTrainName.Text = work.WF_SEL_BTRAINNAME.Text
        If work.WF_SEL_BTRAINNO.Text = work.WF_SEL_BTRAINNAME.Text Then
            Me.LblBTrainName.Text = work.WF_SEL_BTRAINNAME.Text + "レ"
        Else
            Me.LblBTrainName.Text = work.WF_SEL_BTRAINNAME.Text
        End If

        '空車着日（予定）
        Me.txtEmparrDate.Text = work.WF_SEL_EMPARRDATE.Text
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

        '返送列車を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtBTrainNo.Attributes("onkeyPress") = "CheckNum()"

        '新規作成ではない場合
        If work.WF_SEL_CREATEFLG.Text <> "1" Then
            '既存データの修正については、登録営業所は入力不可とする。
            Me.TxtOrderOffice.Enabled = False
            Me.TxtBTrainNo.Enabled = False
            Me.txtEmparrDate.Enabled = False
        End If

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("ORG", work.WF_SEL_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)
        '登録営業所
        CODENAME_get("SALESOFFICE", work.WF_SEL_OFFICECODE.Text, TxtOrderOffice.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '### 20200618 START 油種数の入力許可に伴う対応 #########################
        '〇画面表示設定処理
        WW_ScreenEnabledSet()
        '### 20200618 END   油種数の入力許可に伴う対応 #########################

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, 0)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0002tbl)

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
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection, ByVal O_INSCNT As Integer)

        If IsNothing(OIT0002tbl) Then
            OIT0002tbl = New DataTable
        End If

        If OIT0002tbl.Columns.Count <> 0 Then
            OIT0002tbl.Columns.Clear()
        End If

        OIT0002tbl.Clear()

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

        '○ 取得SQL
        '　検索説明　：　貨車連結順序表№の連番を決める
        Dim SQLStrNum As String =
        " SELECT " _
            & " ISNULL(FORMAT(MAX(SUBSTRING(OIT0004.LINKNO, 10, 2)) + 1,'00'),'01') AS LINKNO_NUM " _
            & " FROM OIL.OIT0004_LINK OIT0004 " _
            & " WHERE SUBSTRING(OIT0004.LINKNO, 2, 8) = FORMAT(GETDATE(),'yyyyMMdd') "

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注、受注明細等のマスタから取得する
        Dim SQLStr As String = ""

        '登録ボタン押下
        If work.WF_SEL_CREATEFLG.Text = 1 Then

            SQLStr =
                  " SELECT TOP (@P01)" _
                & "   0                                             AS LINECNT " _
                & " , ''                                            AS OPERATION " _
                & " , ''                                            AS UPDTIMSTP " _
                & " , 1                                             AS 'SELECT' " _
                & " , 0                                             AS HIDDEN " _
                & " , @P02                                          AS RLINKNO " _
                & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS RLINKDETAILNO " _
                & " , ''                                            AS LINKNO " _
                & " , @P10                                          AS REGISTRATIONDATE " _
                & " , ''                                            AS TRAINNO " _
                & " , ''                                            AS MODEL " _
                & " , ''                                            AS TANKNUMBER " _
                & " , @P03                                          AS OFFICECODE " _
                & " , @P04                                          AS OFFICENAME " _
                & " , ''                                            AS PATTERNCODE " _
                & " , ''                                            AS PATTERNNAME " _
                & " , ''                                            AS SHIPPERSCODE " _
                & " , ''                                            AS SHIPPERSNAME " _
                & " , ''                                            AS BASECODE " _
                & " , ''                                            AS BASENAME " _
                & " , ''                                            AS CONSIGNEECODE " _
                & " , ''                                            AS CONSIGNEENAME " _
                & " , ''                                            AS ORDERINFO " _
                & " , ''                                            AS ORDERINFONAME " _
                & " , @P05                                          AS DEPSTATION " _
                & " , @P06                                          AS DEPSTATIONNAME " _
                & " , @P07                                          AS RETSTATION " _
                & " , @P08                                          AS RETSTATIONNAME " _
                & " , ''                                            AS EMPARRDATE " _
                & " , ''                                            AS PREOILCODE " _
                & " , ''                                            AS PREOILNAME " _
                & " , ''                                            AS PREORDERINGTYPE " _
                & " , ''                                            AS PREORDERINGOILNAME " _
                & " , ''                                            AS ARTICLENAME " _
                & " , ''                                            AS CONVERSIONAMOUNT " _
                & " , ''                                            AS ARTICLE " _
                & " , ''                                            AS ARTICLETRAINNO " _
                & " , ''                                            AS ARTICLEOILNAME " _
                & " , ''                                            AS CURRENTCARTOTAL " _
                & " , ''                                            AS EXTEND " _
                & " , ''                                            AS CONVERSIONTOTAL " _
                & " , ''                                            AS LOADINGIRILINEORDER " _
                & " , ''                                            AS INSPECTIONFLG" _
                & " , ''                                            AS OILCODE " _
                & " , ''                                            AS OILNAME " _
                & " , ''                                            AS ORDERINGTYPE " _
                & " , ''                                            AS ORDERINGOILNAME " _
                & " , ''                                            AS FILLINGPOINT " _
                & " , ''                                            AS LINE " _
                & " , ''                                            AS LOADINGIRILINETRAINNO " _
                & " , ''                                            AS LOADINGIRILINETRAINNAME " _
                & " , ''                                            AS LOADINGOUTLETTRAINNO " _
                & " , ''                                            AS LOADINGOUTLETTRAINNAME " _
                & " , ''                                            AS LOADINGOUTLETORDER " _
                & " , ''                                            AS ORDERNO " _
                & " , ''                                            AS DETAILNO " _
                & " , ''                                            AS LOADINGTRAINNO " _
                & " , ''                                            AS LOADINGTRAINNAME " _
                & " , ''                                            AS LOADINGDEPSTATION " _
                & " , ''                                            AS LOADINGDEPSTATIONNAME " _
                & " , ''                                            AS LOADINGRETSTATION " _
                & " , ''                                            AS LOADINGRETSTATIONNAME " _
                & " , ''                                            AS ORDERTRKBN " _
                & " , ''                                            AS OTTRANSPORTFLG " _
                & " , ''                                            AS LOADINGLODDATE " _
                & " , ''                                            AS LOADINGDEPDATE " _
                & " , ''                                            AS LOADINGARRDATE " _
                & " , ''                                            AS LOADINGACCDATE " _
                & " , ''                                            AS LOADINGEMPARRDATE " _
                & " , '0'                                           AS DELFLG " _
                & " FROM sys.all_objects "

            SQLStr &=
                  " ORDER BY " _
                & "    LINECNT "

            '明細データダブルクリック
        ElseIf work.WF_SEL_CREATEFLG.Text = 2 Then
            SQLStr =
                  " SELECT " _
                & "   0                                             AS LINECNT " _
                & " , ''                                            AS OPERATION " _
                & " , CAST(OIT0011.UPDTIMSTP AS bigint)             AS UPDTIMSTP " _
                & " , 1                                             AS 'SELECT' " _
                & " , 0                                             AS HIDDEN " _
                & " , ISNULL(RTRIM(OIT0011.RLINKNO), '')            AS RLINKNO " _
                & " , ISNULL(RTRIM(OIT0011.RLINKDETAILNO), '')      AS RLINKDETAILNO " _
                & " , ISNULL(RTRIM(OIT0011.LINKNO), '')             AS LINKNO " _
                & " , ISNULL(RTRIM(OIT0011.REGISTRATIONDATE), '')   AS REGISTRATIONDATE " _
                & " , ISNULL(RTRIM(OIT0011.TRAINNO), '')            AS TRAINNO " _
                & " , ISNULL(RTRIM(OIT0011.TRUCKSYMBOL), '')        AS MODEL " _
                & " , ISNULL(RTRIM(OIT0011.TRUCKNO), '')            AS TANKNUMBER " _
                & " , ISNULL(RTRIM(OIT0002.OFFICECODE)," _
                & "          ISNULL(RTRIM(OIT0004.OFFICECODE), '')) AS OFFICECODE " _
                & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')         AS OFFICENAME " _
                & " , ISNULL(RTRIM(OIT0002.ORDERTYPE), '')          AS PATTERNCODE " _
                & " , ''                                            AS PATTERNNAME " _
                & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')       AS SHIPPERSCODE " _
                & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')       AS SHIPPERSNAME " _
                & " , ISNULL(RTRIM(OIT0002.BASECODE), '')           AS BASECODE " _
                & " , ISNULL(RTRIM(OIT0002.BASENAME), '')           AS BASENAME " _
                & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')      AS CONSIGNEECODE " _
                & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')      AS CONSIGNEENAME " _
                & " , ISNULL(RTRIM(OIT0004.INFO), '')               AS ORDERINFO " _
                & " , ''                                            AS ORDERINFONAME " _
                & " , ISNULL(RTRIM(OIT0004.DEPSTATION), '')         AS DEPSTATION " _
                & " , ISNULL(RTRIM(OIT0004.DEPSTATIONNAME), '')     AS DEPSTATIONNAME " _
                & " , ISNULL(RTRIM(OIT0004.RETSTATION), '')         AS RETSTATION " _
                & " , ISNULL(RTRIM(OIT0004.RETSTATIONNAME), '')     AS RETSTATIONNAME " _
                & " , ISNULL(RTRIM(OIT0004.EMPARRDATE), '')         AS EMPARRDATE " _
                & " , ISNULL(RTRIM(OIT0004.PREOILCODE), '')         AS PREOILCODE " _
                & " , ISNULL(RTRIM(OIT0004.PREOILNAME), '')         AS PREOILNAME " _
                & " , ISNULL(RTRIM(OIT0004.PREORDERINGTYPE), '')    AS PREORDERINGTYPE " _
                & " , ISNULL(RTRIM(OIT0004.PREORDERINGOILNAME), '') AS PREORDERINGOILNAME " _
                & " , ISNULL(RTRIM(OIT0011.ARTICLENAME), '')        AS ARTICLENAME " _
                & " , ISNULL(RTRIM(OIT0011.CONVERSIONAMOUNT), '')   AS CONVERSIONAMOUNT " _
                & " , ISNULL(RTRIM(OIT0011.ARTICLE), '')            AS ARTICLE " _
                & " , ISNULL(RTRIM(OIT0011.ARTICLETRAINNO), '')     AS ARTICLETRAINNO " _
                & " , ISNULL(RTRIM(OIT0011.ARTICLEOILNAME), '')     AS ARTICLEOILNAME " _
                & " , ISNULL(RTRIM(OIT0011.CURRENTCARTOTAL), '')    AS CURRENTCARTOTAL " _
                & " , ISNULL(RTRIM(OIT0011.EXTEND), '')             AS EXTEND " _
                & " , ISNULL(RTRIM(OIT0011.CONVERSIONTOTAL), '')    AS CONVERSIONTOTAL " _
                & " , ISNULL(RTRIM(OIT0003.LOADINGIRILINEORDER)," _
                & "          RTRIM(OIT0011.SERIALNUMBER))           AS LOADINGIRILINEORDER " _
                & " , CASE ISNULL(RTRIM(OIT0005.TANKSITUATION), '')" _
                & "   WHEN @P11 THEN 'on'" _
                & "   ELSE ''" _
                & "   END                                           AS INSPECTIONFLG" _
                & " , ISNULL(RTRIM(OIT0003.OILCODE), '')            AS OILCODE " _
                & " , ISNULL(RTRIM(OIT0003.OILNAME), '')            AS OILNAME " _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')       AS ORDERINGTYPE " _
                & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '')    AS ORDERINGOILNAME " _
                & " , ISNULL(RTRIM(OIT0003.FILLINGPOINT), '')       AS FILLINGPOINT " _
                & " , ISNULL(RTRIM(OIT0003.LINE), '')               AS LINE " _
                & " , ISNULL(RTRIM(OIT0003.LOADINGIRILINETRAINNO)," _
                & "          RTRIM(OIT0004.LINETRAINNO))            AS LOADINGIRILINETRAINNO  " _
                & " , ISNULL(RTRIM(OIT0003.LOADINGIRILINETRAINNAME), '') AS LOADINGIRILINETRAINNAME " _
                & " , ISNULL(RTRIM(OIT0003.LOADINGOUTLETTRAINNO), '')    AS LOADINGOUTLETTRAINNO " _
                & " , ISNULL(RTRIM(OIT0003.LOADINGOUTLETTRAINNAME), '')  AS LOADINGOUTLETTRAINNAME " _
                & " , ISNULL(RTRIM(OIT0003.LOADINGOUTLETORDER), '')      AS LOADINGOUTLETORDER " _
                & " , ISNULL(RTRIM(OIT0011.ORDERNO), '')            AS ORDERNO " _
                & " , ISNULL(RTRIM(OIT0011.DETAILNO), '')           AS DETAILNO " _
                & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')            AS LOADINGTRAINNO " _
                & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')          AS LOADINGTRAINNAME " _
                & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')         AS LOADINGDEPSTATION " _
                & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')     AS LOADINGDEPSTATIONNAME " _
                & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')         AS LOADINGRETSTATION " _
                & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')     AS LOADINGRETSTATIONNAME " _
                & " , ''                                            AS ORDERTRKBN " _
                & " , CASE ISNULL(RTRIM(OIT0003.OTTRANSPORTFLG), '')" _
                & "   WHEN '1' THEN 'on'" _
                & "   WHEN '2' THEN ''" _
                & "   ELSE ''" _
                & "   END                                           AS OTTRANSPORTFLG" _
                & " , ISNULL(FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd'), '') AS LOADINGLODDATE" _
                & " , ISNULL(FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd'), '') AS LOADINGDEPDATE" _
                & " , ISNULL(FORMAT(OIT0002.ARRDATE, 'yyyy/MM/dd'), '') AS LOADINGARRDATE" _
                & " , ISNULL(FORMAT(OIT0002.ACCDATE, 'yyyy/MM/dd'), '') AS LOADINGACCDATE" _
                & " , ISNULL(FORMAT(OIT0002.EMPARRDATE, 'yyyy/MM/dd'), '') AS LOADINGEMPARRDATE" _
                & " , ISNULL(RTRIM(OIT0004.DELFLG), '')             AS DELFLG " _
                & " FROM OIL.OIT0011_RLINK OIT0011 " _
                & " LEFT JOIN OIL.OIT0004_LINK OIT0004 ON " _
                & "     OIT0004.LINKNO       = OIT0011.LINKNO" _
                & " AND OIT0004.LINKDETAILNO = OIT0011.RLINKDETAILNO " _
                & " AND OIT0004.STATUS       = '1' " _
                & " AND OIT0004.DELFLG      <> @P09 " _
                & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
                & "     OIT0011.TRUCKNO = OIM0005.TANKNUMBER " _
                & " AND OIM0005.DELFLG <> @P09 "

            '### 20201021 START 指摘票対応(No183)全体 #############################################
            SQLStr &=
                  " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
                & "     OIT0011.TRUCKNO = OIT0005.TANKNUMBER " _
                & " AND OIT0005.TANKSITUATION = @P11 " _
                & " AND OIT0005.DELFLG <> @P09 "
            '### 20201021 END   指摘票対応(No183)全体 #############################################

            SQLStr &=
                  " LEFT JOIN OIL.OIT0002_ORDER OIT0002 ON " _
                & "     OIT0002.ORDERNO = OIT0011.ORDERNO " _
                & " AND OIT0002.DELFLG <> @P09 " _
                & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
                & "     OIT0003.ORDERNO = OIT0011.ORDERNO " _
                & " AND OIT0003.DETAILNO = OIT0011.DETAILNO " _
                & " AND OIT0003.DELFLG <> @P09 " _
                & " WHERE OIT0011.RLINKNO = @P02" _
                & " AND OIT0011.DELFLG <> @P09 " _
                & " AND ISNULL(OIT0011.TRUCKSYMBOL, '') <> '' "

            SQLStr &=
                  " ORDER BY " _
                & "    OIT0011.RLINKDETAILNO"
        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)
                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0002WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.Int)          '貨車連結(臨海)順序表明細数(新規作成)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 11) '貨車連結(臨海)順序表№
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20) '受注営業所名
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 7)  '発駅コード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 40) '発駅名
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 7)  '着駅コード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 40)  '着駅名
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.Date)         '登録年月日
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar)     'タンク車状況コード

                PARA01.Value = O_INSCNT
                If work.WF_SEL_RLINKNO.Text <> "" Then
                    PARA02.Value = work.WF_SEL_RLINKNO.Text
                Else
                    '★新規の場合は、『貨車連結(臨海)順序表№』を取得して設定
                    Dim WW_GetNumber() As String = {"", "", "", "", "", ""}
                    FixvalueMasterSearch("ZZ", "NEWRLINKNOGET", "", WW_GetNumber)

                    work.WF_SEL_RLINKNO.Text = WW_GetNumber(0)
                    PARA02.Value = work.WF_SEL_RLINKNO.Text

                End If
                PARA03.Value = work.WF_SEL_OFFICECODE.Text
                PARA04.Value = work.WF_SEL_OFFICENAME.Text
                PARA05.Value = work.WF_SEL_DEPSTATION.Text
                PARA06.Value = work.WF_SEL_DEPSTATIONNAME.Text
                PARA07.Value = work.WF_SEL_RETSTATION.Text
                PARA08.Value = work.WF_SEL_RETSTATIONNAME.Text
                PARA09.Value = C_DELETE_FLG.DELETE
                PARA10.Value = Now.ToString("yyyy/MM/dd")
                PARA11.Value = BaseDllConst.CONST_TANKSITUATION_13

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    If i = 0 Then work.WF_SEL_LINKNO.Text = OIT0002row("LINKNO")
                    i += 1
                    OIT0002row("LINECNT") = i        'LINECNT

                    '登録営業所
                    If OIT0002row("OFFICECODE") <> "" Then
                        CODENAME_get("SALESOFFICE", OIT0002row("OFFICECODE"), OIT0002row("OFFICENAME"), WW_DUMMY)

                        '積込後着駅
                        If OIT0002row("LOADINGRETSTATION") <> "" Then
                            '★営業所関連情報(輸送形態区分)取得
                            FixvalueMasterSearch(OIT0002row("OFFICECODE"),
                                                 "PATTERNMASTER",
                                                 OIT0002row("LOADINGRETSTATION"),
                                                 WW_GetValue)
                            OIT0002row("ORDERTRKBN") = WW_GetValue(8)
                        End If

                    End If
                    '受注情報
                    CODENAME_get("ORDERINFO", OIT0002row("ORDERINFO"), OIT0002row("ORDERINFONAME"), WW_DUMMY)

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D Select"
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
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            If OIT0002row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0002row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0002tbl)

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

        '### 20200618 START 油種数の入力許可に伴う対応 #########################
        '〇画面表示設定処理
        WW_ScreenEnabledSet()
        '### 20200618 END   油種数の入力許可に伴う対応 #########################

        ''### 20200618 START 貨車連結順序表の登録時はタンク車所在の更新をしない整理に変更 #########################
        ''〇タンク車所在の更新
        'WW_TankShozaiSet()
        ''### 20200618 END   貨車連結順序表の登録時はタンク車所在の更新をしない整理に変更 #########################

    End Sub

    ''' <summary>
    ''' 登録ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '### 20200713 START 登録時のチェックは不要なため削除 ####################################################
        ''○関連チェック
        'WW_Check(WW_ERRCODE)
        'If WW_ERRCODE = "ERR" Then
        '    Exit Sub
        'End If
        '### 20200713 END   登録時のチェックは不要なため削除 ####################################################

        ''### 20200618 START 空車着日を画面から削除したため合わせてチェックも削除 #################################
        ''〇日付妥当性チェック
        'WW_CheckValidityDate(WW_ERRCODE)
        'If WW_ERRCODE = "ERR" Then
        '    Exit Sub
        'End If
        ''### 20200618 END   空車着日を画面から削除したため合わせてチェックも削除 #################################

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("ORG", work.WF_SEL_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)
        '登録営業所
        CODENAME_get("SALESOFFICE", work.WF_SEL_OFFICECODE.Text, TxtOrderOffice.Text, WW_DUMMY)
        ''空車発駅
        'CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
        ''空車着駅
        'CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_DUMMY)

        '### 20200710-START 油種数登録制御追加 ###################################
        '★油種数の""(空文字)チェック
        If Me.TxtHTank.Text = "" Then Me.TxtHTank.Text = "0"        '車数（ハイオク）
        If Me.TxtRTank.Text = "" Then Me.TxtRTank.Text = "0"        '車数（レギュラー）
        If Me.TxtTTank.Text = "" Then Me.TxtTTank.Text = "0"        '車数（灯油）
        If Me.TxtMTTank.Text = "" Then Me.TxtMTTank.Text = "0"      '車数（未添加灯油）
        If Me.TxtKTank.Text = "" Then Me.TxtKTank.Text = "0"        '車数（軽油）
        If Me.TxtK3Tank.Text = "" Then Me.TxtK3Tank.Text = "0"      '車数（３号軽油）
        If Me.TxtK5Tank.Text = "" Then Me.TxtK5Tank.Text = "0"      '車数（５号軽油）
        If Me.TxtK10Tank.Text = "" Then Me.TxtK10Tank.Text = "0"    '車数（１０号軽油）
        If Me.TxtLTank.Text = "" Then Me.TxtLTank.Text = "0"        '車数（LSA）
        If Me.TxtATank.Text = "" Then Me.TxtATank.Text = "0"        '車数（A重油）

        'タンク車数の件数カウント用
        Dim intTankCnt As Integer = 0
        intTankCnt += Integer.Parse(Me.TxtHTank.Text)
        intTankCnt += Integer.Parse(Me.TxtRTank.Text)
        intTankCnt += Integer.Parse(Me.TxtTTank.Text)
        intTankCnt += Integer.Parse(Me.TxtMTTank.Text)
        intTankCnt += Integer.Parse(Me.TxtKTank.Text)
        intTankCnt += Integer.Parse(Me.TxtK3Tank.Text)
        intTankCnt += Integer.Parse(Me.TxtK5Tank.Text)
        intTankCnt += Integer.Parse(Me.TxtK10Tank.Text)
        intTankCnt += Integer.Parse(Me.TxtLTank.Text)
        intTankCnt += Integer.Parse(Me.TxtATank.Text)
        Me.TxtTotalTank.Text = intTankCnt.ToString()

        '油種数が１つも入力されていない場合
        If Me.TxtTotalTank.Text = "0" Then
            Master.Output(C_MESSAGE_NO.OIL_OILTANK_INPUT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Me.TxtHTank.Focus()

            '〇 登録ボタンのチェックを無効(False)
            WF_ButtonInsertFLG.Value = "FALSE"
            Exit Sub
        Else
            '〇 登録ボタンのチェックを有効(True)
            WF_ButtonInsertFLG.Value = "TRUE"

        End If
        '### 20200710-END   油種数登録制御追加 ###################################

        'パネルロックを解除
        work.WF_SEL_PANEL.Value = "1"

        WF_PANELFLG.Value = "1"

        '○ GridView初期設定
        '○ 画面表示データ再取得(貨車連結表(明細)画面表示データ取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '### 20200710-START 油種数登録制御追加 ###################################
            'MAPDataGet(SQLcon, 0)
            MAPDataGet(SQLcon, Integer.Parse(Me.TxtTotalTank.Text))
            '### 20200710-END   油種数登録制御追加 ###################################
        End Using

        '### 20200512-START 油種数登録制御追加 ##################################################################
        '〇画面で設定された油種コードを取得
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        Dim arrTankCode(intTankCnt) As String
        Dim arrTankName(intTankCnt) As String
        Dim arrTankType(intTankCnt) As String
        Dim arrTankOrderName(intTankCnt) As String
        Dim z As Integer = 0

        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_HTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtHTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_HTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_RTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtRTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_RTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_TTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtTTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_TTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_MTTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtMTTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_MTTank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_KTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtKTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_KTank1
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_K3Tank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtK3Tank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_K3Tank1
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_K5Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtK5Tank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_K5Tank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_K10Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtK10Tank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_K10Tank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_LTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtLTank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_LTank1
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", BaseDllConst.CONST_ATank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Me.TxtATank.Text) - 1
            arrTankCode(z) = BaseDllConst.CONST_ATank
            arrTankName(z) = WW_GetValue(0)
            arrTankType(z) = WW_GetValue(1)
            arrTankOrderName(z) = WW_GetValue(2)
            z += 1
        Next
        '### 20200512-END   油種数登録制御追加 ##################################################################

        '### 指摘票内部(No170)対象の営業所のみチェックをするように変更(20200407) ################################
        Dim j As Integer = 0
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            OIT0002row("OILCODE") = arrTankCode(j)              '油種コード
            OIT0002row("OILNAME") = arrTankName(j)              '油種名
            OIT0002row("ORDERINGTYPE") = arrTankType(j)         '油種区分(受発注用)
            OIT0002row("ORDERINGOILNAME") = arrTankOrderName(j) '油種名(受発注用)
            'OIT0002row("PREOILCODE") = arrTankCode(j)              '油種コード
            'OIT0002row("PREOILNAME") = arrTankName(j)              '油種名
            'OIT0002row("PREORDERINGTYPE") = arrTankType(j)         '油種区分(受発注用)
            'OIT0002row("PREORDERINGOILNAME") = arrTankOrderName(j) '油種名(受発注用)

            j += 1
            '営業所が"011201(五井営業所)", "011202(甲子営業所)", "011203(袖ヶ浦営業所)"が対象
            If work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011201 _
                OrElse work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011202 _
                OrElse work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then
                OIT0002row("LOADINGIRILINEORDER") = j        '入線順
            End If
        Next
        '########################################################################################################

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            '(予定)空車着日
                            Case "txtEmparrDate"
                                .WF_Calendar.Text = Me.txtEmparrDate.Text
                        End Select
                        .ActiveCalendar()
                    Case Else   '以外
                        '会社コード
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            '運用部署
                            Case "WF_ORG"
                                prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)

                            '登録営業所
                            Case "TxtOrderOffice"
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtOrderOffice.Text)

                            '返送列車
                            Case "TxtBTrainNo"
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_OFFICECODE.Text, TxtBTrainNo.Text)

                            '(一覧)タンク車№
                            Case "TANKNUMBER"
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_OFFICECODE.Text, "")
                                '### LeftBoxマルチ対応(20200217) START #####################################################
                                '↓暫定一覧対応 2020/02/13 グループ会社版を復活させ石油システムに合わない部分は直す
                                Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                                .SetTableList(enumVal, WW_DUMMY, prmData)
                                .ActiveTable()
                                Return
                                '↑暫定一覧対応 2020/02/13
                                '### LeftBoxマルチ対応(20200217) END   #####################################################
                            '(一覧)油種
                            Case "ORDERINGOILNAME"
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_OFFICECODE.Text, "")

                            '(一覧)充填ポイント
                            '(一覧)入線列車番号, (一覧)出線列車番号, 
                            '(一覧)積込後本線列車
                            Case "FILLINGPOINT",
                                 "LOADINGIRILINETRAINNO", "LOADINGOUTLETTRAINNO",
                                 "LOADINGTRAINNO"
                                '○ LINECNT取得
                                Dim WW_LINECNT As Integer = 0
                                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                                '○ 対象ヘッダー取得
                                Dim updHeader = OIT0002tbl.AsEnumerable.
                                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                                If IsNothing(updHeader) Then Exit Sub

                                '◯ (一覧)充填ポイント
                                If WF_FIELD.Value = "FILLINGPOINT" Then
                                    prmData = work.CreateSALESOFFICEParam(updHeader.Item("BASECODE"), "")
                                    '↓暫定一覧対応 2020/02/13 グループ会社版を復活させ石油システムに合わない部分は直す
                                    Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                                    .SetTableList(enumVal, WW_DUMMY, prmData)
                                    .ActiveTable()
                                    Return
                                    '↑暫定一覧対応 2020/02/13
                                Else
                                    prmData.Item(C_PARAMETERS.LP_COMPANY) = updHeader.Item("OFFICECODE")
                                End If

                        End Select
                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select
            End With
        End If
    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click(ByVal chkFieldName As String)

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0002tbl)

        Select Case chkFieldName
            Case "WF_CheckBoxSELECTOTTRANSPORT"
                'チェックボックス判定
                For i As Integer = 0 To OIT0002tbl.Rows.Count - 1

                    '◯ 輸送形態区分が"M"(請負OT混載)以外の場合
                    If OIT0002tbl.Rows(i)("ORDERTRKBN") <> BaseDllConst.CONST_TRKBN_M Then
                        Continue For
                    End If

                    If OIT0002tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If OIT0002tbl.Rows(i)("OTTRANSPORTFLG") = "on" Then
                            OIT0002tbl.Rows(i)("OTTRANSPORTFLG") = ""
                        Else
                            OIT0002tbl.Rows(i)("OTTRANSPORTFLG") = "on"
                        End If
                    End If
                Next
                '### 20201021 START 指摘票対応(No183)全体 #############################################
            Case "WF_CheckBoxSELECTINSPECTION"
                'チェックボックス判定
                For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
                    If OIT0002tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If OIT0002tbl.Rows(i)("INSPECTIONFLG") = "on" Then
                            OIT0002tbl.Rows(i)("INSPECTIONFLG") = ""
                        Else
                            OIT0002tbl.Rows(i)("INSPECTIONFLG") = "on"
                        End If
                    End If
                Next
                '### 20201021 END   指摘票対応(No183)全体 #############################################
            Case Else
                'チェックボックス判定
                For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
                    If OIT0002tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                        If OIT0002tbl.Rows(i)("OPERATION") = "on" Then
                            OIT0002tbl.Rows(i)("OPERATION") = ""
                        Else
                            OIT0002tbl.Rows(i)("OPERATION") = "on"
                        End If
                    End If
                Next
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", TxtOrderOffice.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '登録営業所
            Case "TxtOrderOffice"
                CODENAME_get("ORG", TxtOrderOffice.Text, WF_ORG_TEXT.Text, WW_RTN_SW)
            '返送列車
            Case "TxtBTrainNo"
                '★列車№(返送)から情報を取得
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text,
                                     "BTRAINNUMBER",
                                     Me.TxtBTrainNo.Text,
                                     WW_GetValue)

                '◯ 空車発駅
                work.WF_SEL_DEPSTATION.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", work.WF_SEL_DEPSTATION.Text, work.WF_SEL_DEPSTATIONNAME.Text, WW_RTN_SW)

                '◯ 空車着駅
                work.WF_SEL_RETSTATION.Text = WW_GetValue(2)
                CODENAME_get("RETSTATION", work.WF_SEL_RETSTATION.Text, work.WF_SEL_RETSTATIONNAME.Text, WW_RTN_SW)

                '〇 空車着日
                Dim iNextUseday As Integer
                Try
                    iNextUseday = Integer.Parse(WW_GetValue(6))
                Catch ex As Exception
                    iNextUseday = 0
                End Try
                Me.txtEmparrDate.Text = Now.AddDays(1 + iNextUseday).ToString("yyyy/MM/dd")

                Me.TxtBTrainNo.Focus()
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Select Case WF_FIELD.Value
                Case "TxtHeadOfficeTrain"
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR, "登録営業所")
                Case "AvailableYMD"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "利用可能日")
                Case "TxtDepstation"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "空車発駅")
                Case "TxtRetstation"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "空車着駅")
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

            Case "WF_ORG"              '運用部署
                WF_ORG.Text = WW_SelectValue
                WF_ORG_TEXT.Text = WW_SelectText
                WF_ORG.Focus()

            Case "TxtOrderOffice"      '登録営業所
                '別の登録営業所が設定された場合
                If TxtOrderOffice.Text <> WW_SelectText Then
                    TxtOrderOffice.Text = WW_SelectText
                    work.WF_SEL_OFFICECODE.Text = WW_SelectValue
                    work.WF_SEL_OFFICENAME.Text = WW_SelectText

                    '返送列車のテキストボックスを初期化
                    Me.TxtBTrainNo.Text = ""
                    Me.TxtBTrainName.Text = ""

                    '○ 油種別タンク車数(車)の件数を初期化
                    Me.TxtTotalTank.Text = "0"
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

                    '○ 一覧の初期化画面表示データ取得
                    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()       'DataBase接続

                        '######################################################
                        '受注営業所を変更した時点で、新規登録と同様の扱いとする。
                        work.WF_SEL_CREATEFLG.Text = "1"
                        work.WF_SEL_PANEL.Value = ""
                        '######################################################
                        MAPDataGet(SQLcon, 0)
                    End Using

                    '○ 画面表示データ保存
                    Master.SaveTable(OIT0002tbl)

                End If

                Me.TxtOrderOffice.Focus()

            Case "TxtBTrainNo"   '返送列車

                If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                    Dim selectedText = Me.Request.Form("commonLeftListSelectedText")
                    Dim selectedItem = leftview.WF_LeftListBox.Items.FindByText(selectedText)
                    WW_SelectValue = selectedItem.Value
                    WW_SelectText = selectedItem.Text
                End If

                If WW_SelectText = "" Then
                    '◯ 返送列車
                    Me.TxtBTrainNo.Text = ""
                    Me.TxtBTrainName.Text = ""
                    Me.LblBTrainName.Text = ""

                    '◯ 空車発駅
                    work.WF_SEL_DEPSTATION.Text = ""
                    work.WF_SEL_DEPSTATIONNAME.Text = ""

                    '◯ 空車着駅
                    work.WF_SEL_RETSTATION.Text = ""
                    work.WF_SEL_RETSTATIONNAME.Text = ""

                    Exit Select
                End If

                Me.TxtBTrainNo.Text = WW_SelectValue
                Me.TxtBTrainName.Text = WW_SelectText
                Me.LblBTrainName.Text = WW_SelectText
                Me.TxtBTrainNo.Focus()

                '★列車名(返送)から情報を取得
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text,
                                     "BTRAINNUMBER_FIND",
                                     Me.TxtBTrainName.Text,
                                     WW_GetValue)

                '◯情報が取得できない場合
                If WW_GetValue(1) = "" Then
                    '★列車名(在線)から情報を取得
                    FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text,
                                     "CTRAINNUMBER_FIND",
                                     Me.TxtBTrainName.Text,
                                     WW_GetValue)
                End If

                '◯ 空車発駅
                work.WF_SEL_DEPSTATION.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", work.WF_SEL_DEPSTATION.Text, work.WF_SEL_DEPSTATIONNAME.Text, WW_RTN_SW)

                '◯ 空車着駅
                work.WF_SEL_RETSTATION.Text = WW_GetValue(2)
                CODENAME_get("RETSTATION", work.WF_SEL_RETSTATION.Text, work.WF_SEL_RETSTATIONNAME.Text, WW_RTN_SW)

                '〇 空車着日
                Dim iNextUseday As Integer
                Try
                    iNextUseday = Integer.Parse(WW_GetValue(6))
                Catch ex As Exception
                    iNextUseday = 0
                End Try
                Me.txtEmparrDate.Text = Now.AddDays(1 + iNextUseday).ToString("yyyy/MM/dd")
                work.WF_SEL_EMPARRDATE.Text = Me.txtEmparrDate.Text

            Case "txtEmparrDate"       '空車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.txtEmparrDate.Text = ""
                    Else
                        Me.txtEmparrDate.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                work.WF_SEL_EMPARRDATE.Text = Me.txtEmparrDate.Text
                Me.txtEmparrDate.Focus()

            '(一覧)タンク車№, (一覧)油種名(受発注用), 
            '(一覧)充填ポイント, (一覧)積込入線列車番号, (一覧)積込出線列車番号
            '(一覧)積込後本線列車, (一覧)積込後本線列車積込予定日, (一覧)積込後本線列車発予定日
            Case "TANKNUMBER", "ORDERINGOILNAME",
                 "FILLINGPOINT", "LOADINGIRILINETRAINNO", "LOADINGOUTLETTRAINNO",
                 "LOADINGTRAINNO", "LOADINGLODDATE", "LOADINGDEPDATE"
                '○ LINECNT取得
                Dim WW_LINECNT As Integer = 0
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                '○ 設定項目取得
                Dim WW_SETTEXT As String = WW_SelectText
                Dim WW_SETVALUE As String = WW_SelectValue

                '○ 画面表示データ復元
                If Not Master.RecoverTable(OIT0002tbl) Then Exit Sub

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0002tbl.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                If IsNothing(updHeader) Then Exit Sub

                '〇 一覧項目へ設定
                'タンク車№を一覧に設定
                If WF_FIELD.Value = "TANKNUMBER" Then
                    Dim WW_TANKNUMBER As String = WW_SETVALUE
                    Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                    updHeader.Item(WF_FIELD.Value) = WW_TANKNUMBER

                    FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TANKNUMBERLINK", WW_TANKNUMBER, WW_GetValue)

                    '型式
                    updHeader.Item("MODEL") = WW_GetValue(0)

                    '前回油種
                    Dim WW_LASTOILNAME As String = ""
                    updHeader.Item("PREOILCODE") = WW_GetValue(1)
                    updHeader.Item("PREOILNAME") = WW_GetValue(4)
                    updHeader.Item("PREORDERINGTYPE") = WW_GetValue(5)
                    updHeader.Item("PREORDERINGOILNAME") = WW_GetValue(6)

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

                        WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN_SEG", WW_SETVALUE, WW_GetValue)
                        updHeader.Item("OILNAME") = WW_GetValue(2)
                        updHeader.Item("ORDERINGTYPE") = WW_GetValue(1)
                    End If

                    '(一覧)充填ポイント
                ElseIf WF_FIELD.Value = "FILLINGPOINT" Then
                    updHeader.Item(WF_FIELD.Value) = WW_SETVALUE

                    '(一覧)積込入線列車番号
                ElseIf WF_FIELD.Value = "LOADINGIRILINETRAINNO" Then
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
                    FixvalueMasterSearch(updHeader.Item("OFFICECODE"), "RINKAITRAIN_FIND_I", WW_SelectText, WW_GetValue)

                    '回線
                    updHeader.Item("LINE") = WW_GetValue(5)
                    '出線列車番号
                    updHeader.Item("LOADINGOUTLETTRAINNO") = WW_GetValue(6)
                    '出線列車名
                    updHeader.Item("LOADINGOUTLETTRAINNAME") = WW_GetValue(7)

                    ''★表の1行目を入力した場合、2行目以降の値も同様に設定する。
                    'If WW_LINECNT = 1 Then
                    '    For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    '        OIT0002row("LOADINGIRILINETRAINNO") = WW_SelectValue
                    '        OIT0002row("LOADINGIRILINETRAINNAME") = WW_SelectText
                    '        OIT0002row("LINE") = WW_GetValue(5)
                    '        OIT0002row("LOADINGOUTLETTRAINNO") = WW_GetValue(6)
                    '        OIT0002row("LOADINGOUTLETTRAINNAME") = WW_GetValue(7)
                    '    Next
                    'End If

                    '(一覧)積込出線列車番号
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
                    FixvalueMasterSearch(updHeader.Item("OFFICECODE"), "RINKAITRAIN_FIND_O", WW_SelectText, WW_GetValue)

                    '回線
                    updHeader.Item("LINE") = WW_GetValue(5)
                    '入線列車番号
                    updHeader.Item("LOADINGIRILINETRAINNO") = WW_GetValue(6)
                    '入線列車名
                    updHeader.Item("LOADINGIRILINETRAINNAME") = WW_GetValue(7)

                    '(一覧)積込後本線列車
                ElseIf WF_FIELD.Value = "LOADINGTRAINNO" Then
                    If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                        Dim selectedText = Me.Request.Form("commonLeftListSelectedText")
                        Dim selectedItem = leftview.WF_LeftListBox.Items.FindByText(selectedText)
                        WW_SelectValue = selectedItem.Value
                        WW_SelectText = selectedItem.Text
                    End If

                    If WW_SelectValue = "" Then
                        '◯ 積込後本線列車
                        updHeader.Item(WF_FIELD.Value) = ""
                        updHeader.Item("LOADINGTRAINNAME") = ""

                        '◯ 積込後発駅
                        updHeader.Item("LOADINGDEPSTATION") = ""
                        updHeader.Item("LOADINGDEPSTATIONNAME") = ""

                        '◯ 積込後着駅
                        updHeader.Item("LOADINGRETSTATION") = ""
                        updHeader.Item("LOADINGRETSTATIONNAME") = ""

                        '◯ 積込後(予定)日付を設定
                        updHeader.Item("LOADINGLODDATE") = ""
                        updHeader.Item("LOADINGDEPDATE") = ""
                        updHeader.Item("LOADINGARRDATE") = ""
                        updHeader.Item("LOADINGACCDATE") = ""
                        updHeader.Item("LOADINGEMPARRDATE") = ""

                        '荷主
                        updHeader.Item("SHIPPERSCODE") = ""
                        updHeader.Item("SHIPPERSNAME") = ""
                        '基地
                        updHeader.Item("BASECODE") = ""
                        updHeader.Item("BASENAME") = ""
                        '荷受人
                        updHeader.Item("CONSIGNEECODE") = ""
                        updHeader.Item("CONSIGNEENAME") = ""
                        '受注パターン
                        updHeader.Item("PATTERNCODE") = ""
                        updHeader.Item("PATTERNNAME") = ""
                        '輸送形態区分
                        updHeader.Item("ORDERTRKBN") = ""

                        '○ 画面表示データ保存
                        Master.SaveTable(OIT0002tbl)

                        Exit Select
                    End If

                    updHeader.Item(WF_FIELD.Value) = WW_SelectValue
                    updHeader.Item("LOADINGTRAINNAME") = WW_SelectText
                    'updHeader.Item(WF_FIELD.Value) = WW_SETVALUE
                    'updHeader.Item("LOADINGTRAINNAME") = WW_SETTEXT

                    '★列車名(本線)から情報を取得
                    FixvalueMasterSearch(updHeader.Item("OFFICECODE"),
                                         "TRAINNUMBER_FIND",
                                         WW_SelectText,
                                         WW_GetValue)

                    '◯ 積込後発駅
                    updHeader.Item("LOADINGDEPSTATION") = WW_GetValue(1)
                    CODENAME_get("DEPSTATION", WW_GetValue(1), updHeader.Item("LOADINGDEPSTATIONNAME"), WW_RTN_SW, I_OFFICECODE:=updHeader.Item("OFFICECODE"))

                    '◯ 積込後着駅
                    updHeader.Item("LOADINGRETSTATION") = WW_GetValue(2)
                    CODENAME_get("RETSTATION", WW_GetValue(2), updHeader.Item("LOADINGRETSTATIONNAME"), WW_RTN_SW, I_OFFICECODE:=updHeader.Item("OFFICECODE"))

                    '〇 積込後(予定)日付を設定
                    updHeader.Item("LOADINGLODDATE") = Now.AddDays(1).ToString("yyyy/MM/dd")
                    updHeader.Item("LOADINGDEPDATE") = Now.AddDays(1 + Integer.Parse(WW_GetValue(6))).ToString("yyyy/MM/dd")
                    updHeader.Item("LOADINGARRDATE") = Now.AddDays(1 + Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                    updHeader.Item("LOADINGACCDATE") = Now.AddDays(1 + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                    updHeader.Item("LOADINGEMPARRDATE") = Now.AddDays(1 + Integer.Parse(WW_GetValue(10)) + Integer.Parse(WW_GetValue(11))).ToString("yyyy/MM/dd")

                    '★営業所関連情報(荷主、基地、荷受人、受注パターン、輸送形態区分)取得
                    WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                    FixvalueMasterSearch(updHeader.Item("OFFICECODE"),
                                         "PATTERNMASTER",
                                         updHeader.Item("LOADINGRETSTATION"),
                                         WW_GetValue)

                    '荷主
                    updHeader.Item("SHIPPERSCODE") = WW_GetValue(0)
                    updHeader.Item("SHIPPERSNAME") = WW_GetValue(1)
                    '基地
                    updHeader.Item("BASECODE") = WW_GetValue(2)
                    updHeader.Item("BASENAME") = WW_GetValue(3)
                    '荷受人
                    updHeader.Item("CONSIGNEECODE") = WW_GetValue(4)
                    updHeader.Item("CONSIGNEENAME") = WW_GetValue(5)
                    '受注パターン
                    updHeader.Item("PATTERNCODE") = WW_GetValue(6)
                    updHeader.Item("PATTERNNAME") = WW_GetValue(7)
                    '輸送形態区分
                    updHeader.Item("ORDERTRKBN") = WW_GetValue(8)

                    '(一覧)積込後本線列車積込予定日
                ElseIf WF_FIELD.Value = "LOADINGLODDATE" Then
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            updHeader.Item("LOADINGLODDATE") = ""
                        Else
                            updHeader.Item("LOADINGLODDATE") = leftview.WF_Calendar.Text
                        End If
                    Catch ex As Exception
                    End Try
                    'updHeader.Item("LOADINGLODDATE").Focus()

                    '◯ 列車(名称)から日数を取得
                    WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                    FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TRAINNUMBER_FIND", updHeader.Item("LOADINGTRAINNAME"), WW_GetValue)

                    '〇 (予定)の日付を設定
                    updHeader.Item("LOADINGDEPDATE") = Date.Parse(updHeader.Item("LOADINGLODDATE")).AddDays(Integer.Parse(WW_GetValue(6))).ToString("yyyy/MM/dd")
                    updHeader.Item("LOADINGARRDATE") = Date.Parse(updHeader.Item("LOADINGLODDATE")).AddDays(Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                    updHeader.Item("LOADINGACCDATE") = Date.Parse(updHeader.Item("LOADINGLODDATE")).AddDays(Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                    updHeader.Item("LOADINGEMPARRDATE") = Date.Parse(updHeader.Item("LOADINGLODDATE")).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")

                    '(一覧)積込後本線列車発予定日
                ElseIf WF_FIELD.Value = "LOADINGDEPDATE" Then
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            updHeader.Item("LOADINGDEPDATE") = ""
                        Else
                            updHeader.Item("LOADINGDEPDATE") = leftview.WF_Calendar.Text
                        End If
                    Catch ex As Exception
                    End Try
                    'updHeader.Item("LOADINGDEPDATE").Focus()

                    '◯ 列車(名称)から日数を取得
                    WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                    FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TRAINNUMBER_FIND", updHeader.Item("LOADINGTRAINNAME"), WW_GetValue)

                    '〇 (予定)の日付を設定
                    If Integer.Parse(WW_GetValue(6)) = 0 Then
                        updHeader.Item("LOADINGARRDATE") = Date.Parse(updHeader.Item("LOADINGDEPDATE")).AddDays(Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                        updHeader.Item("LOADINGACCDATE") = Date.Parse(updHeader.Item("LOADINGDEPDATE")).AddDays(Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                        updHeader.Item("LOADINGEMPARRDATE") = Date.Parse(updHeader.Item("LOADINGDEPDATE")).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                    ElseIf Integer.Parse(WW_GetValue(6)) > 0 Then
                        updHeader.Item("LOADINGARRDATE") = Date.Parse(updHeader.Item("LOADINGDEPDATE")).AddDays((-1 * Integer.Parse(WW_GetValue(6))) + Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                        updHeader.Item("LOADINGACCDATE") = Date.Parse(updHeader.Item("LOADINGDEPDATE")).AddDays((-1 * Integer.Parse(WW_GetValue(6))) + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                        updHeader.Item("LOADINGEMPARRDATE") = Date.Parse(updHeader.Item("LOADINGDEPDATE")).AddDays((-1 * Integer.Parse(WW_GetValue(6))) + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                    End If

                End If

                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0002tbl) Then Exit Sub

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
            Case "WF_ORG"               '運用部署
                WF_ORG.Focus()
            Case "TxtBTrainNo"          '返送列車
                Me.TxtBTrainNo.Focus()
            Case "txtEmparrDate"        '空車着日
                Me.txtEmparrDate.Focus()
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
        Master.RecoverTable(OIT0002tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
            If OIT0002tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0002tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0002tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
            If OIT0002tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0002tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0002tbl)

        '■■■ OIT0002tbl関連の受注・受注明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･貨車連結表(臨海)TBLと貨車連結表TBLを一括論理削除
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0011_RLINK " _
                    & "    SET UPDYMD        = @P11, " _
                    & "        UPDUSER       = @P12, " _
                    & "        UPDTERMID     = @P13, " _
                    & "        RECEIVEYMD    = @P14, " _
                    & "        DELFLG        = @P04 " _
                    & "  WHERE RLINKNO       = @P01 " _
                    & "    AND RLINKDETAILNO = @P02 " _
                    & "    AND DELFLG       <> @P04; "

            SQLStr &=
                    " UPDATE OIL.OIT0004_LINK " _
                    & "    SET UPDYMD       = @P11, " _
                    & "        UPDUSER      = @P12, " _
                    & "        UPDTERMID    = @P13, " _
                    & "        RECEIVEYMD   = @P14, " _
                    & "        DELFLG       = @P04 " _
                    & "  WHERE LINKNO       = @P03 " _
                    & "    AND LINKDETAILNO = @P02 " _
                    & "    AND DELFLG      <> @P04; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)      ' 貨車連結(臨海)順序表№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar)      ' 貨車連結(臨海)順序表明細№
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar)      ' 貨車連結順序表№
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)   ' 削除フラグ

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)      ' 更新年月日
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar)      ' 更新ユーザーＩＤ
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar)      ' 更新端末
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)      ' 集信日時

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            For Each OIT0002UPDrow In OIT0002tbl.Rows
                If OIT0002UPDrow("OPERATION") = "on" Then
                    j += 1
                    OIT0002UPDrow("LINECNT") = j        'LINECNT
                    OIT0002UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0002UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0002UPDrow("RLINKNO")
                    PARA02.Value = OIT0002UPDrow("RLINKDETAILNO")
                    PARA03.Value = OIT0002UPDrow("LINKNO")
                    PARA04.Value = C_DELETE_FLG.DELETE

                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()
                Else
                    i += 1
                    OIT0002UPDrow("LINECNT") = i        'LINECNT
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_ADD_Click()

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        '○ 作成モード(１：新規登録, ２：更新)設定
        Dim SQLStrNum As String
        If work.WF_SEL_CREATEFLG.Text = "1" OrElse OIT0002tbl.Rows.Count = 0 Then
            SQLStrNum =
            " SELECT " _
            & "  @P01   AS RLINKNO" _
            & ", '001'  AS RLINKDETAILNO"

        Else
            SQLStrNum =
            " SELECT " _
            & "  ISNULL(OIT0011.RLINKNO,'')                                          AS RLINKNO" _
            & ", ISNULL(FORMAT(CONVERT(INT, OIT0011.RLINKDETAILNO) + 1,'000'),'000') AS RLINKDETAILNO" _
            & " FROM (" _
            & "  SELECT OIT0011.RLINKNO" _
            & "       , OIT0011.RLINKDETAILNO" _
            & "       , ROW_NUMBER() OVER(PARTITION BY OIT0011.RLINKNO ORDER BY OIT0011.RLINKNO, OIT0011.RLINKDETAILNO DESC) RNUM" _
            & "  FROM OIL.OIT0011_RLINK OIT0011" _
            & "  WHERE OIT0011.RLINKNO = @P01" _
            & " ) OIT0011 " _
            & " WHERE OIT0011.RNUM = 1"

        End If

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String =
              " SELECT TOP (1)" _
            & "   0                                             AS LINECNT " _
            & " , ''                                            AS OPERATION " _
            & " , '0'                                            AS UPDTIMSTP " _
            & " , 1                                             AS 'SELECT' " _
            & " , 0                                             AS HIDDEN " _
            & " , @P01                                          AS RLINKNO " _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS RLINKDETAILNO " _
            & " , @P02                                          AS LINKNO " _
            & " , ''                                            AS REGISTRATIONDATE " _
            & " , ''                                            AS TRAINNO " _
            & " , ''                                            AS MODEL " _
            & " , ''                                            AS TANKNUMBER " _
            & " , @P03                                          AS OFFICECODE " _
            & " , ''                                            AS OFFICENAME " _
            & " , ''                                            AS PATTERNCODE " _
            & " , ''                                            AS PATTERNNAME " _
            & " , ''                                            AS SHIPPERSCODE " _
            & " , ''                                            AS SHIPPERSNAME " _
            & " , ''                                            AS BASECODE " _
            & " , ''                                            AS BASENAME " _
            & " , ''                                            AS CONSIGNEECODE " _
            & " , ''                                            AS CONSIGNEENAME " _
            & " , ''                                            AS ORDERINFO " _
            & " , ''                                            AS ORDERINFONAME " _
            & " , @P04                                          AS DEPSTATION " _
            & " , @P05                                          AS DEPSTATIONNAME " _
            & " , @P06                                          AS RETSTATION " _
            & " , @P07                                          AS RETSTATIONNAME " _
            & " , @P08                                          AS EMPARRDATE " _
            & " , ''                                            AS PREOILCODE " _
            & " , ''                                            AS PREOILNAME " _
            & " , ''                                            AS PREORDERINGTYPE " _
            & " , ''                                            AS PREORDERINGOILNAME " _
            & " , ''                                            AS ARTICLENAME " _
            & " , ''                                            AS CONVERSIONAMOUNT " _
            & " , ''                                            AS ARTICLE " _
            & " , ''                                            AS ARTICLETRAINNO " _
            & " , ''                                            AS ARTICLEOILNAME " _
            & " , ''                                            AS CURRENTCARTOTAL " _
            & " , ''                                            AS EXTEND " _
            & " , ''                                            AS CONVERSIONTOTAL " _
            & " , ''                                            AS LOADINGIRILINEORDER " _
            & " , ''                                            AS INSPECTIONFLG" _
            & " , ''                                            AS OILCODE " _
            & " , ''                                            AS OILNAME " _
            & " , ''                                            AS ORDERINGTYPE " _
            & " , ''                                            AS ORDERINGOILNAME " _
            & " , ''                                            AS FILLINGPOINT " _
            & " , ''                                            AS LINE " _
            & " , ''                                            AS LOADINGIRILINETRAINNO " _
            & " , ''                                            AS LOADINGIRILINETRAINNAME " _
            & " , ''                                            AS LOADINGOUTLETTRAINNO " _
            & " , ''                                            AS LOADINGOUTLETTRAINNAME " _
            & " , ''                                            AS LOADINGOUTLETORDER " _
            & " , ''                                            AS ORDERNO " _
            & " , ''                                            AS DETAILNO " _
            & " , ''                                            AS LOADINGTRAINNO " _
            & " , ''                                            AS LOADINGTRAINNAME " _
            & " , ''                                            AS LOADINGDEPSTATION " _
            & " , ''                                            AS LOADINGDEPSTATIONNAME " _
            & " , ''                                            AS LOADINGRETSTATION " _
            & " , ''                                            AS LOADINGRETSTATIONNAME " _
            & " , ''                                            AS ORDERTRKBN " _
            & " , ''                                            AS OTTRANSPORTFLG " _
            & " , ''                                            AS LOADINGLODDATE " _
            & " , ''                                            AS LOADINGDEPDATE " _
            & " , ''                                            AS LOADINGARRDATE " _
            & " , ''                                            AS LOADINGACCDATE " _
            & " , ''                                            AS LOADINGEMPARRDATE " _
            & " , '0'                                           AS DELFLG " _
            & " FROM sys.all_objects "

        SQLStr &=
                  " ORDER BY " _
                & "    LINECNT "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)
                Dim PARANUM1 As SqlParameter = SQLcmdNum.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結順序表№
                PARANUM1.Value = work.WF_SEL_RLINKNO.Text

                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0002WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11)  '貨車連結(臨海)順序表№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 11)  '貨車連結順序表№
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 6)   '登録営業所コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 7)   '空車発駅コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 40)  '空車発駅名
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 7)   '空車着駅コード
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 40)  '空車着駅名
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.Date)          '空車着日

                PARA01.Value = work.WF_SEL_RLINKNO.Text
                PARA02.Value = work.WF_SEL_LINKNO.Text
                PARA03.Value = work.WF_SEL_OFFICECODE.Text
                PARA04.Value = work.WF_SEL_DEPSTATION.Text
                PARA05.Value = work.WF_SEL_DEPSTATIONNAME.Text
                PARA06.Value = work.WF_SEL_RETSTATION.Text
                PARA07.Value = work.WF_SEL_RETSTATIONNAME.Text
                PARA08.Value = work.WF_SEL_EMPARRDATE.Text

                Dim intDetailNo As Integer = 0
                For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                    intDetailNo = OIT0002WKrow("RLINKDETAILNO")
                    'PARA01.Value = OIT0002WKrow("RLINKNO")
                Next

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    OIT0002tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    '行追加データに既存の貨車連結(臨海)順序表明細№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[貨車連結(臨海)順序表明細№]を利用
                    If OIT0002row("LINECNT") = 0 Then
                        OIT0002row("RLINKDETAILNO") = intDetailNo.ToString("000")

                    ElseIf OIT0002row("RLINKDETAILNO") >= intDetailNo.ToString("000") Then
                        intDetailNo += 1

                    ElseIf OIT0002row("HIDDEN") = 1 Then
                        intDetailNo += 1

                    End If

                    '削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    If OIT0002row("HIDDEN") = 1 Then
                        j += 1
                        OIT0002row("LINECNT") = j        'LINECNT
                    Else
                        i += 1
                        OIT0002row("LINECNT") = i        'LINECNT
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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
        Using repCbj = New OIT0002CustomReport(Master.MAPID, Master.MAPID & ".xlsx", OIT0002Reporttbl)
            Dim url As String
            Try
                url = repCbj.CreateExcelPrintData(work.WF_SEL_OFFICENAME.Text)
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
        'CS0030REPORT.TBLDATA = OIT0002tbl                       'データ参照  Table
        'CS0030REPORT.CS0030REPORT()
        'If Not isNormal(CS0030REPORT.ERR) Then
        '    If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
        '        Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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

        If IsNothing(OIT0002Reporttbl) Then
            OIT0002Reporttbl = New DataTable
        End If

        If OIT0002Reporttbl.Columns.Count <> 0 Then
            OIT0002Reporttbl.Columns.Clear()
        End If

        OIT0002Reporttbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String =
        " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , OIT0011.TRAINNO                                AS TRAINNO" _
            & " , OIT0011.CONVENTIONAL                           AS CONVENTIONAL" _
            & " , OIT0011.CONVENTIONALTIME                       AS CONVENTIONALTIME" _
            & " , OIT0011.AGOBEHINDFLG                           AS AGOBEHINDFLG" _
            & " , OIT0011.REGISTRATIONDATE                       AS REGISTRATIONDATE" _
            & " , OIT0011.SERIALNUMBER                           AS SERIALNUMBER" _
            & " , OIT0011.TRUCKSYMBOL                            AS TRUCKSYMBOL" _
            & " , OIT0011.TRUCKNO                                AS TRUCKNO" _
            & " , OIT0011.DEPSTATIONNAME                         AS DEPSTATIONNAME" _
            & " , OIT0011.ARRSTATIONNAME                         AS ARRSTATIONNAME" _
            & " , OIT0011.ARTICLENAME                            AS ARTICLENAME" _
            & " , ISNULL(OIT0011.INSPECTIONDATE, OIM0005.JRINSPECTIONDATE) AS INSPECTIONDATE" _
            & " , OIT0011.CONVERSIONAMOUNT                       AS CONVERSIONAMOUNT" _
            & " , OIT0011.ARTICLE                                AS ARTICLE" _
            & " , OIT0011.CURRENTCARTOTAL                        AS CURRENTCARTOTAL" _
            & " , OIT0011.EXTEND                                 AS EXTEND" _
            & " , OIT0011.CONVERSIONTOTAL                        AS CONVERSIONTOTAL" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIT0003.OILNAME                                AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME"

        '### 20201002 START 変換マスタに移行したため修正 ########################
        SQLStr &=
              " , OIM0029.VALUE02                                AS REPORTOILNAME" _
            & " , OIM0029.VALUE05                                AS RINKAIOILKANA" _
            & " , OIM0029.VALUE06                                AS RINKAISEGMENTOILNAME"
        'SQLStr &=
        '      " , TMP0005.REPORTOILNAME                          AS REPORTOILNAME" _
        '    & " , TMP0005.RINKAIOILKANA                          AS RINKAIOILKANA" _
        '    & " , TMP0005.RINKAISEGMENTOILNAME                   AS RINKAISEGMENTOILNAME"
        '### 20201002 END   変換マスタに移行したため修正 ########################

        SQLStr &=
              " , OIT0003.FILLINGPOINT                           AS FILLINGPOINT" _
            & " , OIT0003.LINE                                   AS LINE" _
            & " , OIT0003.LOADINGIRILINETRAINNO                  AS LOADINGIRILINETRAINNO" _
            & " , OIT0002.ARRSTATIONNAME                         AS LOADINGARRSTATIONNAME" _
            & " , OIT0002.TRAINNO                                AS ORDERTRAINNO " _
            & " , FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd')          AS ORDERLODDATE " _
            & " , FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd')          AS ORDERDEPDATE " _
            & " , OIT0002.ORDERNO                                AS ORDERNO " _
            & " , OIT0003.DETAILNO                               AS DETAILNO " _
            & " , ''                                             AS ORDERTRKBN " _
            & " , OIT0003.OTTRANSPORTFLG                         AS OTTRANSPORTFLG " _
            & " FROM oil.OIT0011_RLINK OIT0011 " _
            & " LEFT JOIN oil.OIT0002_ORDER OIT0002 ON " _
            & "     OIT0002.ORDERNO = OIT0011.ORDERNO " _
            & " AND OIT0002.DELFLG <> @DELFLG " _
            & " LEFT JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0011.ORDERNO " _
            & " AND OIT0003.DETAILNO = OIT0011.DETAILNO " _
            & " AND OIT0003.DELFLG <> @DELFLG "

        '### 20201002 START 変換マスタに移行したため修正 ########################
        SQLStr &=
              " LEFT JOIN oil.OIM0029_CONVERT OIM0029 ON " _
            & "     OIM0029.KEYCODE01 = OIT0002.OFFICECODE " _
            & " AND OIM0029.KEYCODE04 = '1' " _
            & " AND OIM0029.KEYCODE05 = OIT0003.OILCODE " _
            & " AND OIM0029.KEYCODE08 = OIT0003.ORDERINGTYPE "
        'SQLStr &=
        '      " LEFT JOIN oil.TMP0005OILMASTER TMP0005 ON " _
        '    & "     TMP0005.OFFICECODE = OIT0002.OFFICECODE " _
        '    & " AND TMP0005.OILNo = '1' " _
        '    & " AND TMP0005.OILCODE = OIT0003.OILCODE " _
        '    & " AND TMP0005.SEGMENTOILCODE = OIT0003.ORDERINGTYPE "
        '### 20201002 END   変換マスタに移行したため修正 ########################

        SQLStr &=
              " LEFT JOIN oil.OIM0005_TANK OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0011.TRUCKNO " _
            & " AND OIM0005.DELFLG <> @DELFLG " _
            & " WHERE OIT0011.RLINKNO = @RLINKNO "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_RLINKNO As SqlParameter = SQLcmd.Parameters.Add("@RLINKNO", SqlDbType.NVarChar, 11)  '貨車連結(臨海)順序表№
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)     '削除フラグ
                P_RLINKNO.Value = work.WF_SEL_RLINKNO.Text
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002Reporttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002Reporttbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0002Reprow As DataRow In OIT0002Reporttbl.Rows
                    i += 1
                    OIT0002Reprow("LINECNT") = i        'LINECNT

                    '◯受注Noが未設定の場合はSKIP
                    If OIT0002Reprow("ORDERNO").ToString() = "" Then Continue For
                    For Each OIT0002tblrow As DataRow In OIT0002tbl.Rows
                        '◯受注Noが未設定の場合はSKIP
                        If OIT0002tblrow("ORDERNO").ToString() = "" Then Continue For

                        '★受注No＋受注明細Noと一致した場合
                        If OIT0002tblrow("ORDERNO") + OIT0002tblrow("DETAILNO") _
                            = OIT0002Reprow("ORDERNO") + OIT0002Reprow("DETAILNO") Then

                            '★輸送形態を設定
                            OIT0002Reprow("ORDERTRKBN") = OIT0002tblrow("ORDERTRKBN")
                            Exit For
                        End If
                    Next
                Next

                '### 20200925 START 帳票(Excel)の計算式にて対応のため廃止 ########################################
                ''### 20200916 START 指摘票対応(No142)全体 ########################################################
                ''★甲子営業所対応(位置(充填ポイント)に、回転(回線)+位置(充填ポイント)を再設定)
                'For Each OIT0002Reprow As DataRow In OIT0002Reporttbl.Rows
                '    If work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011202 Then
                '        Try
                '            OIT0002Reprow("FILLINGPOINT") = OIT0002Reprow("LINE") + OIT0002Reprow("FILLINGPOINT")
                '        Catch ex As Exception
                '            OIT0002Reprow("FILLINGPOINT") = ""
                '        End Try
                '    End If
                'Next
                ''### 20200916 END   指摘票対応(No142)全体 ########################################################
                '### 20200925 END   帳票(Excel)の計算式にて対応のため廃止 ########################################
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D EXCEL_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D EXCEL_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0002Reporttbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

#End Region

    ''' <summary>
    ''' 明細更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_RESULT As String = ""

        '〇新規登録時で登録ボタンを押下しているかチェック
        If work.WF_SEL_CREATEFLG.Text = "1" _
            AndAlso WF_ButtonInsertFLG.Value = "FALSE" Then

            Master.Output(C_MESSAGE_NO.OIL_OILREGISTER_ORDER_NOTUSE, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '○関連チェック
        WW_Check(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Master.SaveTable(OIT0002tbl)
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
        ElseIf WW_ERRCODE = "ERR2" Then
            blnOilCheck = True
            Master.Output(C_MESSAGE_NO.OIL_LASTVOLATILEOIL_BLACKLIGHTOIL_ALERT,
              C_MESSAGE_TYPE.QUES,
              needsPopUp:=True,
              messageBoxTitle:="")
            'IsConfirm:=True,
            'YesButtonId:="btnChkLastOilConfirmYes",
            'needsConfirmNgToPostBack:=True,
            'NoButtonId:="btnChkLastOilConfirmNo")
        End If

        '★受注No取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_GetOrderNo(SQLcon)
        End Using

        '○ 画面表示データ一時保存
        Dim OIT0002Tmptbl As DataTable = OIT0002tbl.Copy
        Master.SaveTable(OIT0002Tmptbl)

        '○ 同一レコードチェック
        'If isNormal(WW_ERRCODE) AndAlso blnOilCheck = False Then
        '貨車連結表(臨海)DB追加・更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UpdateRLINK(SQLcon)
        End Using

        '貨車連結表DB追加・更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WF_UPDERRFLG.Value = "0"

            WW_UpdateLINK(SQLcon)
        End Using

        '受注明細DB追加・更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UpdateORDERDETAIL(SQLcon)
        End Using

        '受注DB追加・更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_UpdateORDER(SQLcon)
        End Using

        '### 20201021 START 指摘票対応(No183)全体 #############################################
        'タンク車所在(交検)更新
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            If OIT0002row("INSPECTIONFLG") = "on" Then
                '(タンク車所在TBL)の内容を更新
                '引数１：タンク車状態　⇒　変更なし
                '引数２：積車区分　　　⇒　変更なし
                '引数３：タンク車状況　⇒　変更あり("13"(交検中))
                WW_UpdateTankShozai(Nothing, Nothing, Nothing,
                                    I_TANKNO:=OIT0002row("TANKNUMBER"),
                                    I_SITUATION:=BaseDllConst.CONST_TANKSITUATION_13)
            Else
                '(タンク車所在TBL)の内容を更新
                '引数１：タンク車状態　⇒　変更なし
                '引数２：積車区分　　　⇒　変更なし
                '引数３：タンク車状況　⇒　変更あり("01"(残車))
                '※タンク車状況が"13"(交検中)の場合のみ"01"(残車)へ更新
                WW_UpdateTankShozai(Nothing, Nothing, Nothing,
                                    I_TANKNO:=OIT0002row("TANKNUMBER"),
                                    I_SITUATION:=BaseDllConst.CONST_TANKSITUATION_01,
                                    I_CONDITION:="TANKSITUATION",
                                    I_CONDITION_VAL:=BaseDllConst.CONST_TANKSITUATION_13)
            End If
        Next
        '### 20201021 END   指摘票対応(No183)全体 #############################################

        If WF_UPDERRFLG.Value <> "1" Then
            '貨車連結表(一覧)画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_LinkListTBLSet(SQLcon)
            End Using
        End If
        'End If

        '○ GridView初期設定
        '○ 画面表示データ再取得(貨車連結表(明細)画面表示データ取得)
        If WF_UPDERRFLG.Value <> "1" Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon, 0)
            End Using
        End If

        If WF_UPDERRFLG.Value = "1" Then
            '○ 画面表示一時データ復元
            OIT0002tbl = OIT0002Tmptbl.Copy
            '○ 一時格納Table Close
            If Not IsNothing(OIT0002tbl) Then
                OIT0002Tmptbl.Clear()
                OIT0002Tmptbl.Dispose()
                OIT0002Tmptbl = Nothing
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) AndAlso blnOilCheck = False Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
        Master.CreateEmptyTable(OIT0002INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIT0002INProw As DataRow = OIT0002INPtbl.NewRow

            '○ 初期クリア
            For Each OIT0002INPcol As DataColumn In OIT0002INPtbl.Columns
                If IsDBNull(OIT0002INProw.Item(OIT0002INPcol)) OrElse IsNothing(OIT0002INProw.Item(OIT0002INPcol)) Then
                    Select Case OIT0002INPcol.ColumnName
                        Case "LINECNT"
                            OIT0002INProw.Item(OIT0002INPcol) = 0
                        Case "OPERATION"
                            OIT0002INProw.Item(OIT0002INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "UPDTIMSTP"
                            OIT0002INProw.Item(OIT0002INPcol) = 0
                        Case "SELECT"
                            OIT0002INProw.Item(OIT0002INPcol) = 1
                        Case "HIDDEN"
                            OIT0002INProw.Item(OIT0002INPcol) = 0
                        Case Else
                            OIT0002INProw.Item(OIT0002INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("LINETRAINNO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LINEORDER") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 AndAlso
                WW_COLUMNS.IndexOf("PREOILCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DEPSTATIONNAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("RETSTATIONNAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRINSPECTIONALERTSTR") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRINSPECTIONDATE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRALLINSPECTIONALERTSTR") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRALLINSPECTIONDATE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("AVAILABLEYMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DELFLG") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LINKNO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LINKDETAILNO") >= 0 Then
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    If XLSTBLrow("LINETRAINNO") = OIT0002row("LINETRAINNO") AndAlso
                        XLSTBLrow("LINEORDER") = OIT0002row("LINEORDER") AndAlso
                        XLSTBLrow("TANKNUMBER") = OIT0002row("TANKNUMBER") AndAlso
                        XLSTBLrow("PREOILCODE") = OIT0002row("PREOILCODE") AndAlso
                        XLSTBLrow("DEPSTATIONNAME") = OIT0002row("DEPSTATIONNAME") AndAlso
                        XLSTBLrow("RETSTATIONNAME") = OIT0002row("RETSTATIONNAME") AndAlso
                        XLSTBLrow("JRINSPECTIONALERTSTR") = OIT0002row("JRINSPECTIONALERTSTR") AndAlso
                        XLSTBLrow("JRINSPECTIONDATE") = OIT0002row("JRINSPECTIONDATE") AndAlso
                        XLSTBLrow("JRALLINSPECTIONALERTSTR") = OIT0002row("JRALLINSPECTIONALERTSTR") AndAlso
                        XLSTBLrow("JRALLINSPECTIONDATE") = OIT0002row("JRALLINSPECTIONDATE") AndAlso
                        XLSTBLrow("AVAILABLEYMD") = OIT0002row("AVAILABLEYMD") AndAlso
                        XLSTBLrow("DELFLG") = OIT0002row("DELFLG") AndAlso
                        XLSTBLrow("LINKNO") = OIT0002row("LINKNO") AndAlso
                        XLSTBLrow("LINKDETAILNO") = OIT0002row("LINKDETAILNO") Then
                        OIT0002INProw.ItemArray = OIT0002row.ItemArray
                        Exit For
                    End If
                Next
            End If

            Dim WW_GetValue() As String = {"", "", "", "", ""}

            '○ 項目セット
            '入線列車番号
            If WW_COLUMNS.IndexOf("LINETRAINNO") >= 0 Then
                OIT0002INProw("LINETRAINNO") = XLSTBLrow("LINETRAINNO")
            End If

            '入線順
            If WW_COLUMNS.IndexOf("LOADINGIRILINEORDER") >= 0 Then
                OIT0002INProw("LOADINGIRILINEORDER") = XLSTBLrow("LOADINGIRILINEORDER")
            End If

            'タンク車№
            If WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 Then
                OIT0002INProw("TANKNUMBER") = XLSTBLrow("TANKNUMBER")

                '●タンク車№から対象データを自動で設定
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TANKNUMBER", OIT0002INProw("TANKNUMBER"), WW_GetValue)
                'FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", OIT0002INProw("TANKNUMBER"), WW_GetValue)
                OIT0002INProw("PREOILCODE") = WW_GetValue(1)

                '交検日
                OIT0002INProw("JRINSPECTIONDATE") = WW_GetValue(2)

                ''交検日アラート
                'If WW_GetValue(2) <> "" Then
                '    Dim WW_JRINSPECTIONCNT As String = DateDiff(DateInterval.Day, Date.Parse(Now.ToString("yyyy/MM/dd")), Date.Parse(WW_GetValue(2)))
                '    Dim WW_JRINSPECTIONFLG As String
                '    If WW_JRINSPECTIONCNT <= 3 Then
                '        WW_JRINSPECTIONFLG = "1"
                '    ElseIf WW_JRINSPECTIONCNT >= 4 And WW_JRINSPECTIONCNT <= 6 Then
                '        WW_JRINSPECTIONFLG = "2"
                '    Else
                '        WW_JRINSPECTIONFLG = "3"
                '    End If
                '    Select Case WW_JRINSPECTIONFLG
                '        Case "1"
                '            OIT0002INProw("JRINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                '            OIT0002INProw("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                '        Case "2"
                '            OIT0002INProw("JRINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                '            OIT0002INProw("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                '        Case "3"
                '            OIT0002INProw("JRINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                '            OIT0002INProw("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                '    End Select
                'Else
                '    OIT0002INProw("JRINSPECTIONALERT") = ""
                'End If

                ''全検日
                'OIT0002INProw("JRALLINSPECTIONDATE") = WW_GetValue(3)

                ''全検日アラート
                'If WW_GetValue(3) <> "" Then
                '    Dim WW_JRALLINSPECTIONCNT As String = DateDiff(DateInterval.Day, Date.Parse(Now.ToString("yyyy/MM/dd")), Date.Parse(WW_GetValue(3)))
                '    Dim WW_JRALLINSPECTIONFLG As String
                '    If WW_JRALLINSPECTIONCNT <= 3 Then
                '        WW_JRALLINSPECTIONFLG = "1"
                '    ElseIf WW_JRALLINSPECTIONCNT >= 4 And WW_JRALLINSPECTIONCNT <= 6 Then
                '        WW_JRALLINSPECTIONFLG = "2"
                '    Else
                '        WW_JRALLINSPECTIONFLG = "3"
                '    End If
                '    Select Case WW_JRALLINSPECTIONFLG
                '        Case "1"
                '            OIT0002INProw("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_CAUTION.Replace("'", "")
                '            OIT0002INProw("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                '        Case "2"
                '            OIT0002INProw("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_WARNING.Replace("'", "")
                '            OIT0002INProw("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                '        Case "3"
                '            OIT0002INProw("JRALLINSPECTIONALERT") = CONST_ALERT_STATUS_SAFE.Replace("'", "")
                '            OIT0002INProw("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                '    End Select
                'Else
                '    OIT0002INProw("JRALLINSPECTIONALERT") = ""
                'End If

                '前回油種名(前回油種コードから油種名を取得し設定)
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", OIT0002INProw("PREOILCODE"), WW_GetValue)
                OIT0002INProw("PREOILNAME") = WW_GetValue(0)
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIT0002INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIT0002INProw("DELFLG") = "0"
            End If

            '貨車連結順序表№
            If WW_COLUMNS.IndexOf("LINKNO") >= 0 Then
                OIT0002INProw("LINKNO") = XLSTBLrow("LINKNO")
            End If

            '貨車連結順序表明細№
            If WW_COLUMNS.IndexOf("LINKDETAILNO") >= 0 Then
                OIT0002INProw("LINKDETAILNO") = XLSTBLrow("LINKDETAILNO")
            End If

            OIT0002INPtbl.Rows.Add(OIT0002INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIT0002tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
        Dim updHeader = OIT0002tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            '(★サーバー側で設定しているため必要ないが念のため残す(20200302))
            Case "LINETRAINNO"          '入線番号
                updHeader.Item("LINETRAINNO") = WW_ListValue

            '(★サーバー側で設定しているため必要ないが念のため残す(20200302))
            Case "LOADINGIRILINEORDER"            '入線順序
                updHeader.Item("LOADINGIRILINEORDER") = WW_ListValue

            Case "TANKNUMBER"           '(一覧)タンク車№
                If WW_ListValue <> "" Then
                    FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TANKNUMBERLINK", WW_ListValue, WW_GetValue)

                    'タンク車№
                    updHeader.Item("TANKNUMBER") = WW_ListValue

                    '型式
                    updHeader.Item("MODEL") = WW_GetValue(0)

                    '前回油種
                    Dim WW_LASTOILNAME As String = ""
                    updHeader.Item("PREOILCODE") = WW_GetValue(1)
                    updHeader.Item("PREOILNAME") = WW_GetValue(4)
                    updHeader.Item("PREORDERINGTYPE") = WW_GetValue(5)
                    updHeader.Item("PREORDERINGOILNAME") = WW_GetValue(6)

                Else
                    'タンク車№が空の場合
                    updHeader.Item("TANKNUMBER") = WW_ListValue
                    updHeader.Item("PREOILCODE") = WW_ListValue
                    updHeader.Item("PREOILNAME") = WW_ListValue
                    updHeader.Item("PREORDERINGTYPE") = WW_ListValue
                    updHeader.Item("PREORDERINGOILNAME") = WW_ListValue
                End If

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
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "RINKAITRAIN_LINE", WW_ListValue, WW_GetValue)

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
        Master.SaveTable(OIT0002tbl)

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
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim dateErrFlag As String = ""

        '○ 単項目チェック
        '登録営業所
        If TxtOrderOffice.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "登録営業所", needsPopUp:=True)
            TxtOrderOffice.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", TxtOrderOffice.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SALESOFFICE", work.WF_SEL_OFFICECODE.Text, TxtOrderOffice.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "登録営業所 : " & work.WF_SEL_OFFICECODE.Text, needsPopUp:=True)
                TxtOrderOffice.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="登録営業所", needsPopUp:=True)
            TxtOrderOffice.Focus()
            WW_CheckMES1 = "登録営業所入力エラー。"
            WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '返送列車
        If Me.TxtBTrainNo.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "返送列車", needsPopUp:=True)
            Me.TxtBTrainNo.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", Me.TxtBTrainNo.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="返送列車", needsPopUp:=True)
            Me.TxtBTrainNo.Focus()
            WW_CheckMES1 = "返送列車入力エラー。"
            WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '空車着日
        If Me.txtEmparrDate.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "空車着日", needsPopUp:=True)
            Me.txtEmparrDate.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        '年月日チェック
        WW_CheckDate(Me.txtEmparrDate.Text, "空車着日", WW_CS0024FCHECKERR, dateErrFlag)
        If dateErrFlag = "1" Then
            Me.txtEmparrDate.Focus()
            WW_CheckMES1 = "空車着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            O_RTN = "ERR"
            Exit Sub
        Else
            Me.txtEmparrDate.Text = CDate(Me.txtEmparrDate.Text).ToString("yyyy/MM/dd")
        End If
        '日付過去チェック
        If Me.txtEmparrDate.Text <> "" Then
            Dim WW_DATE_ED As Date
            Try
                Date.TryParse(Me.txtEmparrDate.Text, WW_DATE_ED)

                If WW_DATE_ED < Today Then
                    Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="空車着日", needsPopUp:=True)
                    Me.txtEmparrDate.Focus()
                    WW_CheckMES1 = "空車着日入力エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                End If
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, Me.txtEmparrDate.Text)
                Me.txtEmparrDate.Focus()
                O_RTN = "ERR"
                Exit Sub
            End Try
        End If

        ''○ 一覧チェック
        ''(一覧)タンク車No(重複チェック)
        'Dim OIT0002tbl_DUMMY As DataTable = OIT0002tbl.Copy
        'OIT0002tbl_DUMMY.Columns.Add("TANKNUMBER_SORT", GetType(Integer))
        'For Each OIT0002row As DataRow In OIT0002tbl_DUMMY.Rows
        '    Try
        '        OIT0002row("TANKNUMBER_SORT") = OIT0002row("TANKNUMBER")
        '    Catch ex As Exception
        '        OIT0002row("TANKNUMBER_SORT") = 0
        '    End Try
        'Next

        'OIT0002tbl_DUMMY.Columns.Add("LOADINGIRILINEORDER_SORT", GetType(Integer))
        'For Each OIT0002row As DataRow In OIT0002tbl_DUMMY.Rows
        '    Try
        '        OIT0002row("LOADINGIRILINEORDER_SORT") = OIT0002row("LOADINGIRILINEORDER")
        '    Catch ex As Exception
        '        OIT0002row("LOADINGIRILINEORDER_SORT") = 0
        '    End Try
        'Next

        'OIT0002tbl_DUMMY.Columns.Add("LOADINGOUTLETORDER_SORT", GetType(Integer))
        'For Each OIT0002row As DataRow In OIT0002tbl_DUMMY.Rows
        '    Try
        '        OIT0002row("LOADINGOUTLETORDER_SORT") = OIT0002row("LOADINGOUTLETORDER")
        '    Catch ex As Exception
        '        OIT0002row("LOADINGOUTLETORDER_SORT") = 0
        '    End Try
        'Next

        'Dim OIT0002tbl_dv As DataView = New DataView(OIT0002tbl_DUMMY)
        'Dim chkTankNo As String = ""
        'Dim chkLineOrder As String = ""
        'Dim chkTrainName As String = ""

        ''タンク車Noでソートし、重複がないかチェックする。
        ''OIT0002tbl_dv.Sort = "TANKNUMBER"
        'OIT0002tbl_dv.Sort = "TANKNUMBER_SORT"
        'For Each drv As DataRowView In OIT0002tbl_dv
        '    If drv("HIDDEN") <> "1" AndAlso drv("TANKNUMBER") <> "" AndAlso chkTankNo = drv("TANKNUMBER") Then
        '        Master.Output(C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        '        WW_CheckMES1 = "タンク車№重複エラー。"
        '        WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
        '        WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
        '        O_RTN = "ERR"
        '        Exit Sub
        '    End If
        '    chkTankNo = drv("TANKNUMBER")
        'Next

        ''### 20200407 START 指摘票内部(No170)対象の営業所のみチェックをするように変更 #########################
        ''営業所が"011201(五井営業所)", "011202(甲子営業所)", "011203(袖ヶ浦営業所)"が対象
        'If work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011201 _
        '    OrElse work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011202 _
        '    OrElse work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then

        '    '(一覧)入線順序でソートし、重複がないかチェックする。
        '    'OIT0002tbl_dv.Sort = "LOADINGIRILINEORDER"
        '    OIT0002tbl_dv.Sort = "LOADINGIRILINEORDER_SORT,LOADINGTRAINNAME"
        '    For Each drv As DataRowView In OIT0002tbl_dv
        '        If drv("HIDDEN") <> "1" AndAlso drv("LOADINGIRILINEORDER") <> "" _
        '            AndAlso chkLineOrder = drv("LOADINGIRILINEORDER") _
        '            AndAlso chkTrainName = drv("LOADINGTRAINNAME") Then
        '            Master.Output(C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        '            WW_CheckMES1 = "入線順序重複エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR
        '            WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If
        '        chkLineOrder = drv("LOADINGIRILINEORDER")
        '        chkTrainName = drv("LOADINGTRAINNAME")
        '    Next

        '    '(一覧)出線順序でソートし、重複がないかチェックする。
        '    chkLineOrder = ""
        '    chkTrainName = ""
        '    'OIT0002tbl_dv.Sort = "LOADINGOUTLETORDER"
        '    OIT0002tbl_dv.Sort = "LOADINGOUTLETORDER_SORT,LOADINGTRAINNAME"
        '    For Each drv As DataRowView In OIT0002tbl_dv
        '        If drv("HIDDEN") <> "1" AndAlso drv("LOADINGOUTLETORDER") <> "" _
        '            AndAlso chkLineOrder = drv("LOADINGOUTLETORDER") _
        '            AndAlso chkTrainName = drv("LOADINGTRAINNAME") Then
        '            Master.Output(C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        '            WW_CheckMES1 = "出線順序重複エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR
        '            WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If
        '        chkLineOrder = drv("LOADINGOUTLETORDER")
        '        chkTrainName = drv("LOADINGTRAINNAME")
        '    Next

        'End If
        '### 20200407 END   指摘票内部(No170)対象の営業所のみチェックをするように変更 #########################

        ''★(一覧)空白チェック
        ''営業所が"011201(五井営業所)", "011202(甲子営業所)", "011203(袖ヶ浦営業所)"が対象
        'If work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011201 _
        '    OrElse work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011202 _
        '    OrElse work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 Then

        '    '★タンク車№
        '    For Each OIT0002row As DataRow In OIT0002tbl.Rows
        '        '必須項目が全部空白の行はスキップする
        '        If Trim(OIT0002row("TANKNUMBER")) = "" Then
        '            Continue For
        '        End If
        '        '(一覧)タンク車番号(空白チェック)
        '        If OIT0002row("TANKNUMBER") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)タンク車番号", needsPopUp:=True)

        '            WW_CheckMES1 = "タンク車番号未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If
        '    Next

        '    '★積込列車(入線・出線)
        '    For Each OIT0002row As DataRow In OIT0002tbl.Rows
        '        '必須項目が全部空白の行はスキップする
        '        If Trim(OIT0002row("LOADINGIRILINEORDER")) = "" _
        '            AndAlso Trim(OIT0002row("LOADINGIRILINETRAINNO")) = "" _
        '            AndAlso Trim(OIT0002row("LOADINGOUTLETORDER")) = "" _
        '            AndAlso Trim(OIT0002row("LOADINGOUTLETTRAINNO")) = "" _
        '            AndAlso Trim(OIT0002row("FILLINGPOINT")) = "" _
        '            AndAlso Trim(OIT0002row("LINE")) = "" Then
        '            Continue For
        '        End If

        '        '(一覧)入線列車番号(空白チェック)
        '        If OIT0002row("LOADINGIRILINETRAINNO") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)入線列車番号", needsPopUp:=True)

        '            WW_CheckMES1 = "入線列車番号未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)入線順序(空白チェック)
        '        If OIT0002row("LOADINGIRILINEORDER") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)入線順序", needsPopUp:=True)

        '            WW_CheckMES1 = "入線順序未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)出線列車番号(空白チェック)
        '        If OIT0002row("LOADINGOUTLETTRAINNO") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)出線列車番号", needsPopUp:=True)

        '            WW_CheckMES1 = "出線列車番号未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)出線順序(空白チェック)
        '        If OIT0002row("LOADINGOUTLETORDER") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)出線順序", needsPopUp:=True)

        '            WW_CheckMES1 = "出線順序未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)位置(充填ポイント)(空白チェック)
        '        If OIT0002row("FILLINGPOINT") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)位置(充填ポイント)", needsPopUp:=True)

        '            WW_CheckMES1 = "位置(充填ポイント)未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)回線(空白チェック)
        '        If OIT0002row("LINE") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)回線", needsPopUp:=True)

        '            WW_CheckMES1 = "回線未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '    Next

        '    '★受注登録項目
        '    For Each OIT0002row As DataRow In OIT0002tbl.Rows
        '        '必須項目が全部空白の行はスキップする
        '        If Trim(OIT0002row("ORDERINGOILNAME")) = "" _
        '            AndAlso Trim(OIT0002row("LOADINGTRAINNO")) = "" _
        '            AndAlso Trim(OIT0002row("LOADINGDEPSTATIONNAME")) = "" _
        '            AndAlso Trim(OIT0002row("LOADINGRETSTATIONNAME")) = "" _
        '            AndAlso Trim(OIT0002row("LOADINGLODDATE")) = "" _
        '            AndAlso Trim(OIT0002row("LOADINGDEPDATE")) = "" Then
        '            Continue For
        '        End If

        '        '(一覧)積込油種(空白チェック)
        '        If OIT0002row("ORDERINGOILNAME") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)積込油種", needsPopUp:=True)

        '            WW_CheckMES1 = "積込油種未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)積込後本線列車(空白チェック)
        '        If OIT0002row("LOADINGTRAINNO") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)積込後本線列車", needsPopUp:=True)

        '            WW_CheckMES1 = "積込後本線列車未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)積込後発駅(空白チェック)
        '        If OIT0002row("LOADINGDEPSTATIONNAME") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)積込後発駅", needsPopUp:=True)

        '            WW_CheckMES1 = "積込後発駅未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)積込後着駅(空白チェック)
        '        If OIT0002row("LOADINGRETSTATIONNAME") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)積込後着駅", needsPopUp:=True)

        '            WW_CheckMES1 = "積込後着駅未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)積込後本線列車積込予定日(空白チェック)
        '        If OIT0002row("LOADINGLODDATE") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)積込後本線列車積込予定日", needsPopUp:=True)

        '            WW_CheckMES1 = "積込後本線列車積込予定日未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If

        '        '(一覧)積込後本線列車発予定日(空白チェック)
        '        If OIT0002row("LOADINGDEPDATE") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)積込後本線列車発予定日", needsPopUp:=True)

        '            WW_CheckMES1 = "積込後本線列車発予定日未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If
        '    Next

        'Else
        '    For Each OIT0002row As DataRow In OIT0002tbl.Rows
        '        '(一覧)タンク車番号(空白チェック)
        '        If OIT0002row("TANKNUMBER") = "" And OIT0002row("DELFLG") = "0" Then
        '            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(一覧)タンク車番号", needsPopUp:=True)

        '            WW_CheckMES1 = "タンク車番号未設定エラー。"
        '            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
        '            'WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If
        '    Next
        'End If

        '(一覧)タンク車番号マスタチェック
        Dim strTankName As String = ""
        Dim cvTruckSymbol As String
        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '★コンテナの場合はタンク車番号チェックは未実施
            cvTruckSymbol = StrConv(OIT0002row("MODEL"), Microsoft.VisualBasic.VbStrConv.Wide, &H411)
            '    ### 20201022 START コタキ(OTタンク車)のため除外しない対応 ########
            'If cvTruckSymbol.Substring(0, 1) = "コ" _
            '    OrElse cvTruckSymbol.Substring(0, 1) = "チ" Then
            If cvTruckSymbol.Substring(0, 1) = "チ" Then
                '### 20201022 END   コタキ(OTタンク車)のため除外しない対応 ########
                Continue For
            End If

            '存在チェック
            CODENAME_get("TANKNO", OIT0002row("TANKNUMBER"), strTankName, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "タンク車番号 : " & OIT0002row("TANKNUMBER"), needsPopUp:=True)
                O_RTN = "ERR"
                Exit Sub
            End If
        Next

        '(一覧)積込後本線列車チェック
        '★受注登録するうえで、積込後本線列車番号が未設定の場合はエラーとする。
        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '下記(一覧)項目が設定されているか確認
            '(一覧)積込油種, (一覧)位置, (一覧)回線, (一覧)入線列車, (一覧)出線列車
            If OIT0002row("ORDERINGOILNAME") <> "" _
                OrElse OIT0002row("FILLINGPOINT") <> "" _
                OrElse OIT0002row("LINE") <> "" _
                OrElse OIT0002row("LOADINGIRILINETRAINNO") <> "" _
                OrElse OIT0002row("LOADINGOUTLETTRAINNO") <> "" Then

                '★積込後本線列車が未設定の場合
                If OIT0002row("LOADINGTRAINNO") = "" Then
                    OIT0002row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_103
                    CODENAME_get("ORDERINFO", OIT0002row("ORDERINFO"), OIT0002row("ORDERINFONAME"), WW_DUMMY)

                    'Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    '    SQLcon.Open()       'DataBase接続

                    '    '貨車連結表TBLの情報を更新
                    '    WW_UpdateLinkInfo(SQLcon, OIT0002row)
                    'End Using

                    O_RTN = "ERR"
                End If
            Else
                If OIT0002row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_103 Then
                    OIT0002row("ORDERINFO") = ""
                    OIT0002row("ORDERINFONAME") = ""
                End If
            End If
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.OIL_ORDER_NO_CHECKED_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 年月日妥当性チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckValidityDate(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        'Dim iresult As Integer
        'Dim decChkDay As Decimal

        ''○ 過去日付チェック
        ''例) iresult = dt1.Date.CompareTo(dt2.Date)
        ''    iresultの意味
        ''     0 : dt1とdt2は同じ日
        ''    -1 : dt1はdt2より前の日
        ''     1 : dt1はdt2より後の日
        ''(予定)空車着日 と　利用可能日を比較
        'iresult = Date.Parse(txtEmparrDate.Text).CompareTo(Date.Parse(AvailableYMD.Text))
        'If iresult = 1 Then
        '    decChkDay = (Date.Parse(txtEmparrDate.Text) - Date.Parse(AvailableYMD.Text)).TotalDays
        '    '(予定)空車着日 と　利用可能日の日数を取得し判断
        '    '1 : (予定)空車着日が利用可能日の翌日の日付
        '    '2 : (予定)空車着日が利用可能日の翌々日の日付
        '    '※2以上の日数は未来日としてエラーの位置づけとする。
        '    If decChkDay > 1 Then
        '        Master.Output(C_MESSAGE_NO.OIL_DATE_AVAILABLEDATE_ERROR_Y, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
        '        txtEmparrDate.Focus()
        '        WW_CheckMES1 = "(予定)空車着日"
        '        WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_AVAILABLEDATE_ERROR_Y
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
        '        O_RTN = "ERR"
        '        Exit Sub
        '    End If
        'End If

        ''(実績)空車着日 と　利用可能日を比較
        'If TxtActEmpDate.Text <> "" Then
        '    iresult = Date.Parse(TxtActEmpDate.Text).CompareTo(Date.Parse(AvailableYMD.Text))
        '    If iresult = 1 Then
        '        decChkDay = (Date.Parse(TxtActEmpDate.Text) - Date.Parse(AvailableYMD.Text)).TotalDays
        '        '(実績)空車着日 と　利用可能日の日数を取得し判断
        '        '1 : (実績)空車着日が利用可能日の翌日の日付
        '        '2 : (実績)空車着日が利用可能日の翌々日の日付
        '        '※2以上の日数は未来日としてエラーの位置づけとする。
        '        If decChkDay > 1 Then
        '            Master.Output(C_MESSAGE_NO.OIL_DATE_AVAILABLEDATE_ERROR_J, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
        '            TxtActEmpDate.Focus()
        '            WW_CheckMES1 = "(実績)空車着日"
        '            WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_AVAILABLEDATE_ERROR_J
        '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If
        '    End If
        'End If

    End Sub

    ''' <summary>
    ''' 年月日チェック
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <param name="I_DATENAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String, ByRef dateErrFlag As String)

        dateErrFlag = "1"
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
                'Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                'エラーなし
                dateErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

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
        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '★積込油種が未設定の場合はSKIP(次レコード)する。
            If OIT0002row("OILCODE") = "" Then Continue For

            WW_GetValue = {"", "", "", "", "", "", "", ""}
            FixvalueMasterSearch(OIT0002row("PREOILCODE") + OIT0002row("PREORDERINGTYPE"), "LASTOILCONSISTENCY", OIT0002row("OILCODE") + OIT0002row("ORDERINGTYPE"), WW_GetValue)

            '前回黒油
            If WW_GetValue(2) = "1" AndAlso OIT0002row("DELFLG") = "0" Then
                OIT0002row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_99
                CODENAME_get("ORDERINFO", OIT0002row("ORDERINFO"), OIT0002row("ORDERINFONAME"), WW_DUMMY)

                WW_CheckMES1 = "前回油種と油種の整合性エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_LASTOIL_CONSISTENCY_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)
                O_RTN = "ERR1"
                'Exit Sub

                '前回揮発油
            ElseIf (WW_GetValue(2) = "2" OrElse WW_GetValue(2) = "3") AndAlso OIT0002row("DELFLG") = "0" Then
                OIT0002row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_98
                CODENAME_get("ORDERINFO", OIT0002row("ORDERINFO"), OIT0002row("ORDERINFONAME"), WW_DUMMY)

                WW_CheckMES1 = "前回油種と油種の整合性エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_LASTVOLATILEOIL_BLACKLIGHTOIL_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002row)

                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    '貨車連結表TBLの情報を更新
                    WW_UpdateLinkInfo(SQLcon, OIT0002row)
                End Using

                If O_RTN <> "ERR1" Then O_RTN = "ERR2"
            Else
                If OIT0002row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_99 _
                    OrElse OIT0002row("ORDERINFO") = BaseDllConst.CONST_ORDERINFO_ALERT_98 Then
                    OIT0002row("ORDERINFO") = ""
                    OIT0002row("ORDERINFONAME") = ""
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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

        WW_ERR_MES &= ControlChars.NewLine & "  --> 登録営業所         =" & Me.TxtOrderOffice.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 返送列車           =" & Me.TxtBTrainNo.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着日           =" & Me.txtEmparrDate.Text

        rightview.SetErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' エラーレポート編集(一覧用)
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIT0002row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckListERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIT0002row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIT0002row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項番               =" & OIT0002row("LINECNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線列車番号       =" & OIT0002row("LOADINGIRILINETRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線順             =" & OIT0002row("LOADINGIRILINEORDER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車番号       =" & OIT0002row("TANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 前回油種　　       =" & OIT0002row("PREOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発駅       　　　　=" & OIT0002row("DEPSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 着駅   　    　　　=" & OIT0002row("RETSTATION") & " , "
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
    Protected Sub FixvalueMasterSearch(ByVal I_CODE As String,
                                       ByVal I_CLASS As String,
                                       ByVal I_KEYCODE As String,
                                       ByRef O_VALUE() As String,
                                       Optional ByVal I_PARA01 As String = Nothing)

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

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
                & " , ISNULL(RTRIM(VIW0001.VALUE16), '')  AS VALUE16" _
                & " , ISNULL(RTRIM(VIW0001.VALUE17), '')  AS VALUE17" _
                & " , ISNULL(RTRIM(VIW0001.VALUE18), '')  AS VALUE18" _
                & " , ISNULL(RTRIM(VIW0001.VALUE19), '')  AS VALUE19" _
                & " , ISNULL(RTRIM(VIW0001.VALUE20), '')  AS VALUE20" _
                & " , ISNULL(RTRIM(VIW0001.DELFLG), '')   AS DELFLG" _
                & " FROM  OIL.VIW0001_FIXVALUE VIW0001" _
                & " WHERE VIW0001.CLASS = @P01" _
                & " AND VIW0001.DELFLG <> @P02"

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
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)

                PARA01.Value = I_CLASS
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdr)
                End Using

                If I_KEYCODE.Equals("") Then
                    'Dim i As Integer = 0
                    'For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                    '    O_VALUE(i) = OIT0002WKrow("KEYCODE")
                    '    i += 1
                    'Next

                    If IsNothing(I_PARA01) Then
                        'Dim i As Integer = 0 '2020/3/23 三宅 Delete
                        For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows '(全抽出結果回るので要検討
                            'O_VALUE(i) = OIT0003WKrow("KEYCODE") 2020/3/23 三宅 全部KEYCODE(列車NO)が格納されてしまうので修正しました（問題なければこのコメント消してください)
                            For i = 1 To O_VALUE.Length
                                O_VALUE(i - 1) = OIT0002WKrow("VALUE" & i.ToString())
                            Next
                            'i += 1 '2020/3/23 三宅 Delete
                        Next

                    ElseIf I_PARA01 = "1" Then    '### 油種登録用の油種コードを取得 ###
                        Dim i As Integer = 0
                        For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                            O_VALUE(i) = Convert.ToString(OIT0002WKrow("KEYCODE"))
                            i += 1
                        Next
                    End If

                Else
                    For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0002WKrow("VALUE" & i.ToString())
                        Next
                    Next
                End If
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 受注TBLから受注Noを取得(受注TBLに未存在の場合は新規で受注Noを設定)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_GetOrderNo(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002GETtbl) Then
            OIT0002GETtbl = New DataTable
        End If

        If OIT0002GETtbl.Columns.Count <> 0 Then
            OIT0002GETtbl.Columns.Clear()
        End If

        OIT0002GETtbl.Clear()

        Dim SQLStr As String =
              " SELECT" _
            & "   OIT0002.ORDERNO                                           AS ORDERNO" _
            & " , MAX(OIT0003.DETAILNO)                                     AS DETAILNO" _
            & " , SUM(CASE WHEN OIT0003.OILCODE <> '' Then 1 Else 0 End)    AS TOTALTANK"

        '油種(ハイオク)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS HTANK ", BaseDllConst.CONST_HTank)
        '油種(レギュラー)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS RTANK ", BaseDllConst.CONST_RTank)
        '油種(灯油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS TTANK ", BaseDllConst.CONST_TTank)
        '油種(未添加灯油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS MTTANK ", BaseDllConst.CONST_MTTank)
        '油種(軽油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS KTANK ", BaseDllConst.CONST_KTank1)
        '油種(３号軽油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS K3TANK ", BaseDllConst.CONST_K3Tank1)
        '油種(５号軽油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS K5TANK ", BaseDllConst.CONST_K5Tank)
        '油種(１０号軽油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS K10TANK ", BaseDllConst.CONST_K10Tank)
        '油種(ＬＳＡ)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS LTANK ", BaseDllConst.CONST_LTank1)
        '油種(Ａ重油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS ATANK ", BaseDllConst.CONST_ATank)

        SQLStr &=
              " FROM OIL.OIT0002_ORDER OIT0002" _
            & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003 ON" _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO" _
            & " AND OIT0003.DELFLG <> @DELFLG" _
            & " WHERE " _
            & "     OIT0002.OFFICECODE = @OFFICECODE" _
            & " AND OIT0002.TRAINNAME  = @TRAINNAME" _
            & " AND OIT0002.LODDATE    = @LODDATE" _
            & " AND OIT0002.DEPDATE    = @DEPDATE" _
            & " AND OIT0002.DELFLG    <> @DELFLG"

        SQLStr &=
              " GROUP BY OIT0002.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim P_TRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@TRAINNAME", SqlDbType.NVarChar, 40)   '本線列車名
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.Date)               '積込日(予定)
                Dim P_DEPDATE As SqlParameter = SQLcmd.Parameters.Add("@DEPDATE", SqlDbType.Date)               '発日(予定)
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)          '削除フラグ

                P_DELFLG.Value = C_DELETE_FLG.DELETE

                '受注№取得
                Dim WW_GetValue() As String = {"", "", "", "", "", ""}
                FixvalueMasterSearch("ZZ", "NEWORDERNOGET", "", WW_GetValue)
                Dim sOrderNo As String = WW_GetValue(0)

                '退避用
                Dim sOrderContent() As String = {"", "", "", "", "", ""}
                Dim iNum As Integer

                For Each OIT0002row As DataRow In OIT0002tbl.Select("LOADINGTRAINNAME <> ''", "LOADINGTRAINNAME, ORDERNO, DETAILNO")

                    '★すでに受注Noが設定されているデータはSKIP
                    If OIT0002row("ORDERNO") <> "" Then Continue For

                    '同じオーダーの場合
                    If sOrderContent(2) = OIT0002row("OFFICECODE") _
                       AndAlso sOrderContent(3) = OIT0002row("LOADINGTRAINNAME") _
                       AndAlso sOrderContent(4) = OIT0002row("LOADINGLODDATE") _
                       AndAlso sOrderContent(5) = OIT0002row("LOADINGDEPDATE") Then

                        OIT0002row("ORDERNO") = sOrderContent(0)
                        iNum = Integer.Parse(sOrderContent(1)) + 1
                        OIT0002row("DETAILNO") = iNum.ToString("000")

                    Else
                        P_OFFICECODE.Value = OIT0002row("OFFICECODE")
                        P_TRAINNAME.Value = OIT0002row("LOADINGTRAINNAME")
                        P_LODDATE.Value = OIT0002row("LOADINGLODDATE")
                        P_DEPDATE.Value = OIT0002row("LOADINGDEPDATE")
                        P_DELFLG.Value = C_DELETE_FLG.DELETE

                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            If OIT0002GETtbl.Columns.Count = 0 Then
                                '○ フィールド名とフィールドの型を取得
                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIT0002GETtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIT0002GETtbl.Clear()

                            '○ テーブル検索結果をテーブル格納
                            OIT0002GETtbl.Load(SQLdr)

                        End Using

                        '★受注TBLに存在しない場合
                        If OIT0002GETtbl.Rows.Count = 0 Then
                            OIT0002row("ORDERNO") = sOrderNo
                            OIT0002row("DETAILNO") = "001"

                            '次回用に受注Noをカウント
                            iNum = Integer.Parse(sOrderNo.Substring(9, 2)) + 1
                            sOrderNo = sOrderNo.Substring(0, 9) + iNum.ToString("00")
                        Else
                            '存在する場合は、設定されている受注Noを設定
                            OIT0002row("ORDERNO") = OIT0002GETtbl.Rows(0)("ORDERNO")
                            iNum = Integer.Parse(OIT0002GETtbl.Rows(0)("DETAILNO")) + 1
                            OIT0002row("DETAILNO") = iNum.ToString("000")

                        End If

                        'sOrderContent(0) = OIT0002row("ORDERNO")
                        'sOrderContent(1) = OIT0002row("DETAILNO")
                        'sOrderContent(2) = OIT0002row("OFFICECODE")
                        'sOrderContent(3) = OIT0002row("LOADINGTRAINNAME")
                        'sOrderContent(4) = OIT0002row("LOADINGLODDATE")
                        'sOrderContent(5) = OIT0002row("LOADINGDEPDATE")

                    End If
                    sOrderContent(0) = OIT0002row("ORDERNO")
                    sOrderContent(1) = OIT0002row("DETAILNO")
                    sOrderContent(2) = OIT0002row("OFFICECODE")
                    sOrderContent(3) = OIT0002row("LOADINGTRAINNAME")
                    sOrderContent(4) = OIT0002row("LOADINGLODDATE")
                    sOrderContent(5) = OIT0002row("LOADINGDEPDATE")
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D GET_ORDERNO", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D GET_ORDERNO"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 貨車連結表(臨海)TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateRLINK(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002UPDtbl) Then
            OIT0002UPDtbl = New DataTable
        End If

        If OIT0002UPDtbl.Columns.Count <> 0 Then
            OIT0002UPDtbl.Columns.Clear()
        End If

        OIT0002UPDtbl.Clear()

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0011_RLINK" _
            & "    WHERE" _
            & "        RLINKNO          = @P01 " _
            & "   AND  RLINKDETAILNO    = @P02 " _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0011_RLINK" _
            & "    SET" _
            & "          TRAINNO         = @P06     , SERIALNUMBER       = @P07, TRUCKSYMBOL      = @P08" _
            & "        , TRUCKNO         = @P09     , DEPSTATIONNAME     = @P10, ARRSTATIONNAME   = @P11" _
            & "        , ARTICLENAME     = @P12     , LINKNO             = @P20 " _
            & "        , ORDERNO         = @P21     , DETAILNO           = @P22 " _
            & "        , UPDYMD          = @P27     , UPDUSER            = @P28" _
            & "        , UPDTERMID       = @P29     , RECEIVEYMD         = @P30" _
            & "    WHERE" _
            & "        RLINKNO           = @P01 " _
            & "        AND RLINKDETAILNO = @P02 " _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0011_RLINK" _
            & "        ( RLINKNO       , RLINKDETAILNO " _
            & "        , FILENAME      , AGOBEHINDFLG    , REGISTRATIONDATE " _
            & "        , TRAINNO       , SERIALNUMBER    , TRUCKSYMBOL " _
            & "        , TRUCKNO       , DEPSTATIONNAME  , ARRSTATIONNAME " _
            & "        , ARTICLENAME   , CONVERSIONAMOUNT, ARTICLE " _
            & "        , ARTICLETRAINNO, ARTICLEOILNAME  , CURRENTCARTOTAL " _
            & "        , EXTEND        , CONVERSIONTOTAL , LINKNO " _
            & "        , ORDERNO       , DETAILNO " _
            & "        , DELFLG        , INITYMD         , INITUSER      , INITTERMID " _
            & "        , UPDYMD        , UPDUSER         , UPDTERMID     , RECEIVEYMD) " _
            & "    VALUES" _
            & "        ( @P01, @P02" _
            & "        , @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08" _
            & "        , @P09, @P10, @P11" _
            & "        , @P12, @P13, @P14" _
            & "        , @P15, @P16, @P17" _
            & "        , @P18, @P19, @P20" _
            & "        , @P21, @P22" _
            & "        , @P23, @P24, @P25, @P26" _
            & "        , @P27, @P28, @P29, @P30) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "      RLINKNO" _
            & "    , RLINKDETAILNO" _
            & "    , FILENAME" _
            & "    , AGOBEHINDFLG" _
            & "    , REGISTRATIONDATE" _
            & "    , TRAINNO" _
            & "    , SERIALNUMBER" _
            & "    , TRUCKSYMBOL" _
            & "    , TRUCKNO" _
            & "    , DEPSTATIONNAME" _
            & "    , ARRSTATIONNAME" _
            & "    , ARTICLENAME" _
            & "    , CONVERSIONAMOUNT" _
            & "    , ARTICLE" _
            & "    , ARTICLETRAINNO" _
            & "    , ARTICLEOILNAME" _
            & "    , CURRENTCARTOTAL" _
            & "    , EXTEND" _
            & "    , CONVERSIONTOTAL" _
            & "    , LINKNO" _
            & "    , ORDERNO" _
            & "    , DETAILNO" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & " FROM" _
            & "    OIL.OIT0011_RLINK" _
            & " WHERE" _
            & "     RLINKNO       = @P01" _
            & " AND RLINKDETAILNO = @P02"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結(臨海)順序表№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '貨車連結(臨海)順序表明細№
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20) 'ファイル名
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  '前後フラグ
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)         '登録年月日
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 4)  '列車
                'Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Int)          '通番
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 2)  '通番
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 20) '貨車(記号及び符号)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 8)  '貨車(番号)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 40) '発駅
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 40) '着駅
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 10) '品名
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Decimal)      '換算数量
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20) '記事
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 4)  '列車(記事)
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 40) '油種名(記事)
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.Decimal)      '現車合計
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.Decimal)      '延長
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Decimal)      '換算合計
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 11) '受注№
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 3)  '受注明細№

                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.DateTime)     '登録年月日
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 20) '登録ユーザーID
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 20) '登録端末
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.DateTime)     '更新年月日
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.NVarChar, 20) '更新ユーザーID
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 20) '更新端末
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.DateTime)     '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結(臨海)順序表№
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '貨車連結(臨海)順序表明細№

                Dim WW_DATENOW As DateTime = Date.Now

                '固定値設定
                PARA03.Value = ""                                 'ファイル名
                PARA04.Value = ""                                 '前後フラグ
                PARA06.Value = Me.TxtBTrainNo.Text                '列車
                PARA24.Value = WW_DATENOW                         '登録年月日
                PARA25.Value = Master.USERID                      '登録ユーザーID
                PARA26.Value = Master.USERTERMID                  '登録端末
                PARA27.Value = WW_DATENOW                         '更新年月日
                PARA28.Value = Master.USERID                      '更新ユーザーID
                PARA29.Value = Master.USERTERMID                  '更新端末
                PARA30.Value = C_DEFAULT_YMD

                '着駅名(保存用)
                Dim strRetstationName As String = ""

                '貨車連結順序表No取得
                Dim WW_GetValue() As String = {"", "", "", "", "", ""}
                FixvalueMasterSearch("ZZ", "NEWLINKNOGET", "", WW_GetValue)
                Dim sLinkNo As String = WW_GetValue(0)

                For Each OIT0002row As DataRow In OIT0002tbl.Select(Nothing, "RETSTATIONNAME, DEPSTATIONNAME")
                    PARA01.Value = OIT0002row("RLINKNO")          '貨車連結(臨海)順序表№
                    PARA02.Value = OIT0002row("RLINKDETAILNO")    '貨車連結(臨海)順序表明細№

                    '登録年月日
                    If OIT0002row("REGISTRATIONDATE") <> "" Then
                        PARA05.Value = OIT0002row("REGISTRATIONDATE")
                    Else
                        PARA05.Value = WW_DATENOW.ToString("yyyy/MM/dd")
                    End If

                    PARA07.Value = OIT0002row("LOADINGIRILINEORDER")    '通番
                    PARA08.Value = OIT0002row("MODEL")            '貨車(記号及び符号)
                    PARA09.Value = OIT0002row("TANKNUMBER")       '貨車(番号)
                    PARA10.Value = OIT0002row("DEPSTATIONNAME")   '発駅
                    PARA11.Value = OIT0002row("RETSTATIONNAME")   '着駅
                    PARA12.Value = OIT0002row("ARTICLENAME")      '品名

                    '換算数量
                    If OIT0002row("CONVERSIONAMOUNT") <> "" Then
                        PARA13.Value = OIT0002row("CONVERSIONAMOUNT")
                    Else
                        PARA13.Value = DBNull.Value
                    End If
                    '記事
                    If OIT0002row("ARTICLE") <> "" Then
                        PARA14.Value = OIT0002row("ARTICLE")
                    Else
                        PARA14.Value = ""
                    End If
                    '列車(記事)
                    If OIT0002row("ARTICLETRAINNO") <> "" Then
                        PARA15.Value = OIT0002row("ARTICLETRAINNO")
                    Else
                        PARA15.Value = ""
                    End If
                    '油種名(記事)
                    If OIT0002row("ARTICLEOILNAME") <> "" Then
                        PARA16.Value = OIT0002row("ARTICLEOILNAME")
                    Else
                        PARA16.Value = ""
                    End If
                    '現車合計
                    If OIT0002row("CURRENTCARTOTAL") <> "" Then
                        PARA17.Value = OIT0002row("CURRENTCARTOTAL")
                    Else
                        PARA17.Value = DBNull.Value
                    End If
                    '延長
                    If OIT0002row("EXTEND") <> "" Then
                        PARA18.Value = OIT0002row("EXTEND")
                    Else
                        PARA18.Value = DBNull.Value
                    End If
                    '換算合計
                    If OIT0002row("CONVERSIONTOTAL") <> "" Then
                        PARA19.Value = OIT0002row("CONVERSIONTOTAL")
                    Else
                        PARA19.Value = DBNull.Value
                    End If

                    '貨車連結順序表№
                    If OIT0002row("LINKNO") <> "" Then
                        PARA20.Value = OIT0002row("LINKNO")           '貨車連結順序表№

                        '★貨車連結順序表№が未設定の場合
                    Else
                        If strRetstationName <> "" _
                        AndAlso strRetstationName <> OIT0002row("RETSTATIONNAME") Then
                            Dim sLinkNoBak1 As String = sLinkNo
                            Dim iLinkNoBak1 As Integer
                            sLinkNo = sLinkNoBak1.Substring(0, 9)
                            iLinkNoBak1 = Integer.Parse(sLinkNoBak1.Substring(9, 2)) + 1
                            sLinkNo &= iLinkNoBak1.ToString("00")
                        End If
                        PARA20.Value = sLinkNo
                        OIT0002row("LINKNO") = sLinkNo
                    End If
                    '★着駅名を保存
                    strRetstationName = OIT0002row("RETSTATIONNAME")

                    PARA21.Value = OIT0002row("ORDERNO")          '受注№
                    PARA22.Value = OIT0002row("DETAILNO")         '受注明細№
                    PARA23.Value = OIT0002row("DELFLG")           '削除フラグ

                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    '更新ジャーナル出力
                    JPARA01.Value = OIT0002row("RLINKNO")
                    JPARA02.Value = OIT0002row("RLINKDETAILNO")

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0002UPDtbl) Then
                            OIT0002UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0002UPDtbl.Clear()
                        OIT0002UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0002UPDrow As DataRow In OIT0002UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0002D"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D UPDATE_INSERT_RLINK", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D UPDATE_INSERT_RLINK"
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
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateLINK(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002UPDtbl) Then
            OIT0002UPDtbl = New DataTable
        End If

        If OIT0002UPDtbl.Columns.Count <> 0 Then
            OIT0002UPDtbl.Columns.Clear()
        End If

        OIT0002UPDtbl.Clear()

        '○ ＤＢ更新
        Dim CNT_Total As Long = "0"         '合計
        Dim LNG_TxtHTank As Long = "0"      '油種(ハイオク)
        Dim LNG_TxtRTank As Long = "0"      '油種(レギュラー)
        Dim LNG_TxtTTank As Long = "0"      '油種(灯油)
        Dim LNG_TxtMTTank As Long = "0"     '油種(未添加灯油)
        Dim LNG_TxtKTank1 As Long = "0"     '油種(軽油)
        'Dim LNG_TxtKTank2 As Long = "0"
        Dim LNG_TxtK3Tank1 As Long = "0"    '３号軽油
        'Dim LNG_TxtK3Tank2 As Long = "0"
        Dim LNG_TxtK5Tank As Long = "0"     '５号軽油
        Dim LNG_TxtK10Tank As Long = "0"    '１０号軽油
        Dim LNG_TxtLTank1 As Long = "0"     'ＬＳＡ
        'Dim LNG_TxtLTank2 As Long = "0"
        Dim LNG_TxtATank As Long = "0"      'Ａ重油

        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0004_LINK" _
            & "    WHERE" _
            & "        LINKNO          = @P01 " _
            & "   AND  LINKDETAILNO    = @P02 " _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0004_LINK" _
            & "    SET" _
            & "          AVAILABLEYMD    = @P03 , INFO               = @P05" _
            & "        , TRAINNO         = @P07 , TRAINNAME          = @P08, OFFICECODE       = @P09" _
            & "        , DEPSTATION      = @P10 , DEPSTATIONNAME     = @P11" _
            & "        , RETSTATION      = @P12 , RETSTATIONNAME     = @P13" _
            & "        , EMPARRDATE      = @P14 , ACTUALEMPARRDATE   = @P15" _
            & "        , LINETRAINNO     = @P16 , LINEORDER          = @P17" _
            & "        , TANKNUMBER      = @P18" _
            & "        , PREOILCODE      = @P19 , PREOILNAME         = @P20" _
            & "        , PREORDERINGTYPE = @P21 , PREORDERINGOILNAME = @P22" _
            & "        , UPDYMD          = @P27 , UPDUSER            = @P28" _
            & "        , UPDTERMID       = @P29 , RECEIVEYMD         = @P30" _
            & "    WHERE" _
            & "        LINKNO            = @P01 " _
            & "        AND  LINKDETAILNO = @P02 " _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0004_LINK" _
            & "        ( LINKNO    , LINKDETAILNO  , AVAILABLEYMD, STATUS          , INFO " _
            & "        , PREORDERNO, TRAINNO       , TRAINNAME   , OFFICECODE      , DEPSTATION     , DEPSTATIONNAME " _
            & "        , RETSTATION, RETSTATIONNAME, EMPARRDATE  , ACTUALEMPARRDATE, LINETRAINNO " _
            & "        , LINEORDER , TANKNUMBER    , PREOILCODE  , PREOILNAME      , PREORDERINGTYPE, PREORDERINGOILNAME " _
            & "        , DELFLG    , INITYMD       , INITUSER    , INITTERMID " _
            & "        , UPDYMD    , UPDUSER       , UPDTERMID   , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08, @P09, @P10, @P11" _
            & "        , @P12, @P13, @P14, @P15, @P16" _
            & "        , @P17, @P18, @P19, @P20, @P21, @P22" _
            & "        , @P23, @P24, @P25, @P26" _
            & "        , @P27, @P28, @P29, @P30) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "      LINKNO" _
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
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 7)  '本線列車
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

                ''先にアラームの確認を行う
                'Dim WW_GetValue() As String = {"", "", "", "", "", ""}
                'FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TRAINNUMBER", TxtBTrainNo.Text, WW_GetValue)
                'Dim info As String = ""
                ''### 20200706 START 列車番号が列車マスタに未存在の場合の対応 ####################################
                'Dim iMaxtank As Integer
                'Try
                '    iMaxtank = Integer.Parse(WW_GetValue(3))
                'Catch ex As Exception
                '    iMaxtank = 99
                'End Try
                ''タンク車数が「最大牽引タンク車数」より大きい場合
                'If Integer.Parse(Me.TxtTotalTank.Text) > iMaxtank Then
                '    '80(タンク車数オーバー)を設定
                '    info = WW_ORDERINFOALERM_80
                'End If
                ''### 20200706 END   列車番号が列車マスタに未存在の場合の対応 ####################################

                Dim WW_DATENOW As DateTime = Date.Now
                For Each OIT0002row As DataRow In OIT0002tbl.Rows

                    '★石油輸送での営業所ではない（コンテナなどの登録データ）ものは除外
                    If OIT0002row("OFFICECODE") = "" Then Continue For

                    '◯ DB更新
                    '貨車連結順序表№
                    PARA01.Value = OIT0002row("LINKNO")
                    'If OIT0002row("LINKNO") <> "" Then
                    '    PARA01.Value = OIT0002row("LINKNO")
                    'Else
                    '    '★新規の場合は、『貨車連結順序表№』を取得して設定
                    '    Dim WW_GetValue() As String = {"", "", "", "", "", ""}
                    '    FixvalueMasterSearch("ZZ", "NEWLINKNOGET", "", WW_GetValue)

                    '    work.WF_SEL_LINKNO.Text = WW_GetValue(0)
                    '    PARA01.Value = work.WF_SEL_LINKNO.Text
                    'End If
                    '貨車連結順序表明細№
                    PARA02.Value = OIT0002row("RLINKDETAILNO")
                    '利用可能日
                    If OIT0002row("REGISTRATIONDATE").ToString() = "" Then
                        PARA03.Value = DBNull.Value
                    Else
                        PARA03.Value = RTrim(CDate(OIT0002row("REGISTRATIONDATE")).ToString("yyyy/MM/dd"))
                    End If
                    'ステータス
                    PARA04.Value = "1"
                    '情報
                    PARA05.Value = OIT0002row("ORDERINFO")             '情報
                    '前回オーダー№
                    PARA06.Value = ""
                    PARA07.Value = Me.TxtBTrainNo.Text                 '返送列車
                    PARA08.Value = Me.TxtBTrainName.Text               '返送列車名
                    PARA09.Value = OIT0002row("OFFICECODE")            '登録営業所コード
                    PARA10.Value = OIT0002row("DEPSTATION")            '空車発駅（着駅）コード
                    PARA11.Value = OIT0002row("DEPSTATIONNAME")        '空車発駅（着駅）名
                    PARA12.Value = OIT0002row("RETSTATION")            '空車着駅（発駅）コード
                    PARA13.Value = OIT0002row("RETSTATIONNAME")        '空車着駅（発駅）名
                    '空車着日(予定)
                    If work.WF_SEL_EMPARRDATE.Text = "" Then
                        PARA14.Value = DBNull.Value
                    Else
                        PARA14.Value = work.WF_SEL_EMPARRDATE.Text
                    End If
                    '空車着日(実績)
                    PARA15.Value = DBNull.Value
                    PARA16.Value = OIT0002row("LOADINGIRILINETRAINNO") '入線列車番号
                    PARA17.Value = OIT0002row("LOADINGIRILINEORDER")  '入線順
                    PARA18.Value = OIT0002row("TANKNUMBER")           'タンク車№
                    PARA19.Value = OIT0002row("PREOILCODE")           '前回油種　
                    PARA20.Value = OIT0002row("PREOILNAME")           '前回油種名　
                    PARA21.Value = OIT0002row("PREORDERINGTYPE")      '前回油種区分(受発注用)　
                    PARA22.Value = OIT0002row("PREORDERINGOILNAME")   '前回油種名(受発注用)
                    '★油種毎にカウント
                    Select Case PARA19.Value
                        Case BaseDllConst.CONST_HTank                 '油種(ハイオク)
                            LNG_TxtHTank += 1
                            CNT_Total += 1
                        Case BaseDllConst.CONST_RTank                 '油種(レギュラー)
                            LNG_TxtRTank += 1
                            CNT_Total += 1
                        Case BaseDllConst.CONST_TTank                 '油種(灯油)
                            LNG_TxtTTank += 1
                            CNT_Total += 1
                        Case BaseDllConst.CONST_MTTank                '油種(未添加灯油)
                            LNG_TxtMTTank += 1
                            CNT_Total += 1
                        Case BaseDllConst.CONST_KTank1                '油種(軽油)
                            LNG_TxtKTank1 += 1
                            CNT_Total += 1
                        'Case BaseDllConst.CONST_KTank2
                        'LNG_TxtKTank2 += 1
                        'CNT_Total += 1
                        Case BaseDllConst.CONST_K3Tank1               '３号軽油
                            LNG_TxtK3Tank1 += 1
                            CNT_Total += 1
                        'Case BaseDllConst.CONST_K3Tank2
                        'LNG_TxtK3Tank2 += 1
                        'CNT_Total += 1
                        Case BaseDllConst.CONST_K5Tank                '５号軽油
                            LNG_TxtK5Tank += 1
                            CNT_Total += 1
                        Case BaseDllConst.CONST_K10Tank               '１０号軽油
                            LNG_TxtK10Tank += 1
                            CNT_Total += 1
                        Case BaseDllConst.CONST_LTank1                'ＬＳＡ
                            LNG_TxtLTank1 += 1
                            CNT_Total += 1
                        'Case BaseDllConst.CONST_LTank2
                        'LNG_TxtLTank2 += 1
                        'CNT_Total += 1
                        Case BaseDllConst.CONST_ATank                 'Ａ重油
                            LNG_TxtATank += 1
                            CNT_Total += 1
                    End Select
                    PARA23.Value = OIT0002row("DELFLG")               '削除フラグ
                    PARA24.Value = WW_DATENOW                         '登録年月日
                    PARA25.Value = Master.USERID                      '登録ユーザーID
                    PARA26.Value = Master.USERTERMID                  '登録端末
                    PARA27.Value = WW_DATENOW                         '更新年月日
                    PARA28.Value = Master.USERID                      '更新ユーザーID
                    PARA29.Value = Master.USERTERMID                  '更新端末
                    PARA30.Value = C_DEFAULT_YMD

                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    '更新ジャーナル出力
                    JPARA01.Value = OIT0002row("LINKNO")
                    JPARA02.Value = OIT0002row("RLINKDETAILNO")

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0002UPDtbl) Then
                            OIT0002UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0002UPDtbl.Clear()
                        OIT0002UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0002UPDrow As DataRow In OIT0002UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0002D"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0002UPDrow
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

                '★画面の油種数に合計値を設定
                'タンク車合計
                Me.TxtTotalTank.Text = CNT_Total
                '油種(ハイオク)
                Me.TxtHTank.Text = LNG_TxtHTank
                '油種(レギュラー)
                Me.TxtRTank.Text = LNG_TxtRTank
                '油種(灯油)
                Me.TxtTTank.Text = LNG_TxtTTank
                '油種(未添加灯油)
                Me.TxtMTTank.Text = LNG_TxtMTTank
                '油種(軽油)
                Me.TxtKTank.Text = LNG_TxtKTank1
                'Case CONST_TxtKTank2
                '    WF_SEL_HIGHOCTANE_TANKCAR.Text = LNG_TxtKTank2 + 1
                '３号軽油
                Me.TxtK3Tank.Text = LNG_TxtK3Tank1
                'Case CONST_TxtK3Tank2
                '    Me.TxtK3Tank2.Text = LNG_TxtK3Tank2 + 1
                '５号軽油
                Me.TxtK5Tank.Text = LNG_TxtK5Tank
                '１０号軽油
                Me.TxtK10Tank.Text = LNG_TxtK10Tank
                'ＬＳＡ
                Me.TxtLTank.Text = LNG_TxtLTank1
                'Case CONST_TxtLTank2
                '    Me.TxtLTank2.Text = LNG_TxtLTank2 + 1
                'Ａ重油
                Me.TxtATank.Text = LNG_TxtATank

                work.WF_SEL_CREATEFLG.Text = 2 'エラーが発生しなかった場合、更新モードに切り替える
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D UPDATE_INSERT_LINK", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D UPDATE_INSERT_LINK"
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
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateORDERDETAIL(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002UPDtbl) Then
            OIT0002UPDtbl = New DataTable
        End If

        If OIT0002UPDtbl.Columns.Count <> 0 Then
            OIT0002UPDtbl.Columns.Clear()
        End If

        OIT0002UPDtbl.Clear()

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
            & "        ORDERNO  = @ORDERNO" _
            & "   AND  DETAILNO = @DETAILNO" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0003_DETAIL" _
            & "    SET" _
            & "        LINEORDER               = @LINEORDER            , TANKNO                  = @TANKNO" _
            & "        , STACKINGFLG           = @STACKINGFLG          , OTTRANSPORTFLG          = @OTTRANSPORTFLG" _
            & "        , SHIPPERSCODE          = @SHIPPERSCODE         , SHIPPERSNAME            = @SHIPPERSNAME" _
            & "        , OILCODE               = @OILCODE              , OILNAME                 = @OILNAME" _
            & "        , ORDERINGTYPE          = @ORDERINGTYPE         , ORDERINGOILNAME         = @ORDERINGOILNAME" _
            & "        , LINE                  = @LINE                 , FILLINGPOINT            = @FILLINGPOINT" _
            & "        , LOADINGIRILINETRAINNO = @LOADINGIRILINETRAINNO, LOADINGIRILINETRAINNAME = @LOADINGIRILINETRAINNAME" _
            & "        , LOADINGIRILINEORDER   = @LOADINGIRILINEORDER" _
            & "        , LOADINGOUTLETTRAINNO  = @LOADINGOUTLETTRAINNO , LOADINGOUTLETTRAINNAME  = @LOADINGOUTLETTRAINNAME" _
            & "        , LOADINGOUTLETORDER    = @LOADINGOUTLETORDER" _
            & "        , DELFLG                = @DELFLG" _
            & "        , UPDYMD                = @UPDYMD               , UPDUSER                 = @UPDUSER" _
            & "        , UPDTERMID             = @UPDTERMID            , RECEIVEYMD              = @RECEIVEYMD" _
            & "    WHERE" _
            & "        ORDERNO          = @ORDERNO" _
            & "        AND DETAILNO     = @DETAILNO" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0003_DETAIL" _
            & "        ( ORDERNO              , DETAILNO               , LINEORDER          , TANKNO" _
            & "        , STACKINGFLG          , WHOLESALEFLG           , INSPECTIONFLG      , DETENTIONFLG" _
            & "        , FIRSTRETURNFLG       , AFTERRETURNFLG         , OTTRANSPORTFLG" _
            & "        , ORDERINFO            , SHIPPERSCODE           , SHIPPERSNAME" _
            & "        , OILCODE              , OILNAME                , ORDERINGTYPE       , ORDERINGOILNAME" _
            & "        , CARSNUMBER           , CARSAMOUNT             , RETURNDATETRAIN" _
            & "        , LINE                 , FILLINGPOINT" _
            & "        , LOADINGIRILINETRAINNO, LOADINGIRILINETRAINNAME, LOADINGIRILINEORDER" _
            & "        , LOADINGOUTLETTRAINNO , LOADINGOUTLETTRAINNAME , LOADINGOUTLETORDER" _
            & "        , RESERVEDNO           , OTSENDCOUNT            , DLRESERVEDCOUNT    , DLTAKUSOUCOUNT" _
            & "        , SALSE                , SALSETAX               , TOTALSALSE" _
            & "        , PAYMENT              , PAYMENTTAX             , TOTALPAYMENT" _
            & "        , DELFLG               , INITYMD                , INITUSER           , INITTERMID" _
            & "        , UPDYMD               , UPDUSER                , UPDTERMID          , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @ORDERNO              , @DETAILNO               , @LINEORDER          , @TANKNO" _
            & "        , @STACKINGFLG          , @WHOLESALEFLG           , @INSPECTIONFLG      , @DETENTIONFLG" _
            & "        , @FIRSTRETURNFLG       , @AFTERRETURNFLG         , @OTTRANSPORTFLG" _
            & "        , @ORDERINFO            , @SHIPPERSCODE           , @SHIPPERSNAME" _
            & "        , @OILCODE              , @OILNAME                , @ORDERINGTYPE       , @ORDERINGOILNAME" _
            & "        , @CARSNUMBER           , @CARSAMOUNT             , @RETURNDATETRAIN" _
            & "        , @LINE                 , @FILLINGPOINT" _
            & "        , @LOADINGIRILINETRAINNO, @LOADINGIRILINETRAINNAME, @LOADINGIRILINEORDER" _
            & "        , @LOADINGOUTLETTRAINNO , @LOADINGOUTLETTRAINNAME , @LOADINGOUTLETORDER" _
            & "        , @RESERVEDNO           , @OTSENDCOUNT            , @DLRESERVEDCOUNT    , @DLTAKUSOUCOUNT" _
            & "        , @SALSE                , @SALSETAX               , @TOTALSALSE" _
            & "        , @PAYMENT              , @PAYMENTTAX             , @TOTALPAYMENT" _
            & "        , @DELFLG               , @INITYMD                , @INITUSER           , @INITTERMID" _
            & "        , @UPDYMD               , @UPDUSER                , @UPDTERMID          , @RECEIVEYMD) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    ORDERNO" _
            & "    , DETAILNO" _
            & "    , LINEORDER" _
            & "    , TANKNO" _
            & "    , STACKINGFLG" _
            & "    , WHOLESALEFLG" _
            & "    , INSPECTIONFLG" _
            & "    , DETENTIONFLG" _
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
            & "    , LINE" _
            & "    , FILLINGPOINT" _
            & "    , LOADINGIRILINETRAINNO" _
            & "    , LOADINGIRILINETRAINNAME" _
            & "    , LOADINGIRILINEORDER" _
            & "    , LOADINGOUTLETTRAINNO" _
            & "    , LOADINGOUTLETTRAINNAME" _
            & "    , LOADINGOUTLETORDER" _
            & "    , RESERVEDNO" _
            & "    , OTSENDCOUNT" _
            & "    , DLRESERVEDCOUNT" _
            & "    , DLTAKUSOUCOUNT" _
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
            & "        ORDERNO  = @ORDERNO" _
            & "   AND  DETAILNO = @DETAILNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 11)           '受注№
                Dim P_DETAILNO As SqlParameter = SQLcmd.Parameters.Add("@DETAILNO", SqlDbType.NVarChar, 3)          '受注明細№
                Dim P_LINEORDER As SqlParameter = SQLcmd.Parameters.Add("@LINEORDER", SqlDbType.NVarChar, 2)        '貨物駅入線順
                Dim P_TANKNO As SqlParameter = SQLcmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar, 8)              'タンク車№
                Dim P_STACKINGFLG As SqlParameter = SQLcmd.Parameters.Add("@STACKINGFLG", SqlDbType.NVarChar)       '積置可否フラグ
                Dim P_WHOLESALEFLG As SqlParameter = SQLcmd.Parameters.Add("@WHOLESALEFLG", SqlDbType.NVarChar)     '未卸可否フラグ
                Dim P_INSPECTIONFLG As SqlParameter = SQLcmd.Parameters.Add("@INSPECTIONFLG", SqlDbType.NVarChar)   '交検可否フラグ
                Dim P_DETENTIONFLG As SqlParameter = SQLcmd.Parameters.Add("@DETENTIONFLG", SqlDbType.NVarChar)     '留置可否フラグ
                Dim P_FIRSTRETURNFLG As SqlParameter = SQLcmd.Parameters.Add("@FIRSTRETURNFLG", SqlDbType.NVarChar) '先返し可否フラグ
                Dim P_AFTERRETURNFLG As SqlParameter = SQLcmd.Parameters.Add("@AFTERRETURNFLG", SqlDbType.NVarChar) '後返し可否フラグ
                Dim P_OTTRANSPORTFLG As SqlParameter = SQLcmd.Parameters.Add("@OTTRANSPORTFLG", SqlDbType.NVarChar) 'OT輸送可否フラグ
                Dim P_ORDERINFO As SqlParameter = SQLcmd.Parameters.Add("@ORDERINFO", SqlDbType.NVarChar, 2)        '受注情報
                Dim P_SHIPPERSCODE As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSCODE", SqlDbType.NVarChar, 10) '荷主コード
                Dim P_SHIPPERSNAME As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSNAME", SqlDbType.NVarChar, 10) '荷主名
                Dim P_OILCODE As SqlParameter = SQLcmd.Parameters.Add("@OILCODE", SqlDbType.NVarChar, 4)            '油種コード
                Dim P_OILNAME As SqlParameter = SQLcmd.Parameters.Add("@OILNAME", SqlDbType.NVarChar, 40)           '油種名
                Dim P_ORDERINGTYPE As SqlParameter = SQLcmd.Parameters.Add("@ORDERINGTYPE", SqlDbType.NVarChar, 2)  '油種区分(受発注用)
                Dim P_ORDERINGOILNAME As SqlParameter = SQLcmd.Parameters.Add("@ORDERINGOILNAME", SqlDbType.NVarChar, 40)  '油種名(受発注用)
                Dim P_CARSNUMBER As SqlParameter = SQLcmd.Parameters.Add("@CARSNUMBER", SqlDbType.Int)              '車数
                Dim P_CARSAMOUNT As SqlParameter = SQLcmd.Parameters.Add("@CARSAMOUNT", SqlDbType.Int)              '数量
                '### 20200928 START 指摘票対応(全体(No149)) ###############################################################
                Dim P_RETURNDATETRAIN As SqlParameter = SQLcmd.Parameters.Add("@RETURNDATETRAIN", SqlDbType.NVarChar, 4)                  '返送日列車
                '### 20200928 END   指摘票対応(全体(No149)) ###############################################################
                Dim P_LINE As SqlParameter = SQLcmd.Parameters.Add("@LINE", SqlDbType.NVarChar, 2)                  '回線
                Dim P_FILLINGPOINT As SqlParameter = SQLcmd.Parameters.Add("@FILLINGPOINT", SqlDbType.NVarChar, 2)  '充填ポイント
                Dim P_LOADINGIRILINETRAINNO As SqlParameter = SQLcmd.Parameters.Add("@LOADINGIRILINETRAINNO", SqlDbType.NVarChar, 4)      '積込入線列車番号
                Dim P_LOADINGIRILINETRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@LOADINGIRILINETRAINNAME", SqlDbType.NVarChar, 40) '積込入線列車番号名
                Dim P_LOADINGIRILINEORDER As SqlParameter = SQLcmd.Parameters.Add("@LOADINGIRILINEORDER", SqlDbType.NVarChar, 2)          '積込入線順
                Dim P_LOADINGOUTLETTRAINNO As SqlParameter = SQLcmd.Parameters.Add("@LOADINGOUTLETTRAINNO", SqlDbType.NVarChar, 4)        '積込出線列車番号
                Dim P_LOADINGOUTLETTRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@LOADINGOUTLETTRAINNAME", SqlDbType.NVarChar, 40)   '積込出線列車番号名
                Dim P_LOADINGOUTLETORDER As SqlParameter = SQLcmd.Parameters.Add("@LOADINGOUTLETORDER", SqlDbType.NVarChar, 2)            '積込出線順
                Dim P_RESERVEDNO As SqlParameter = SQLcmd.Parameters.Add("@RESERVEDNO", SqlDbType.NVarChar, 11)     '予約番号
                Dim P_OTSENDCOUNT As SqlParameter = SQLcmd.Parameters.Add("@OTSENDCOUNT", SqlDbType.Int)            'OT発送日報送信回数
                Dim P_DLRESERVEDCOUNT As SqlParameter = SQLcmd.Parameters.Add("@DLRESERVEDCOUNT", SqlDbType.Int)    '出荷予約ダウンロード回数
                Dim P_DLTAKUSOUCOUNT As SqlParameter = SQLcmd.Parameters.Add("@DLTAKUSOUCOUNT", SqlDbType.Int)      '託送状ダウンロード回数
                Dim P_SALSE As SqlParameter = SQLcmd.Parameters.Add("@SALSE", SqlDbType.Int)                        '売上金額
                Dim P_SALSETAX As SqlParameter = SQLcmd.Parameters.Add("@SALSETAX", SqlDbType.Int)                  '売上消費税額
                Dim P_TOTALSALSE As SqlParameter = SQLcmd.Parameters.Add("@TOTALSALSE", SqlDbType.Int)              '売上合計金額
                Dim P_PAYMENT As SqlParameter = SQLcmd.Parameters.Add("@PAYMENT", SqlDbType.Int)                    '支払金額
                Dim P_PAYMENTTAX As SqlParameter = SQLcmd.Parameters.Add("@PAYMENTTAX", SqlDbType.Int)              '支払消費税額
                Dim P_TOTALPAYMENT As SqlParameter = SQLcmd.Parameters.Add("@TOTALPAYMENT", SqlDbType.Int)          '支払合計金額
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)              '削除フラグ
                Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", SqlDbType.DateTime)               '登録年月日
                Dim P_INITUSER As SqlParameter = SQLcmd.Parameters.Add("@INITUSER", SqlDbType.NVarChar, 20)         '登録ユーザーID
                Dim P_INITTERMID As SqlParameter = SQLcmd.Parameters.Add("@INITTERMID", SqlDbType.NVarChar, 20)     '登録端末
                Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)                 '更新年月日
                Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)           '更新ユーザーID
                Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar, 20)       '更新端末
                Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)         '集信日時

                Dim JP_ORDERNO As SqlParameter = SQLcmdJnl.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 4)   '受注№
                Dim JP_DETAILNO As SqlParameter = SQLcmdJnl.Parameters.Add("@DETAILNO", SqlDbType.NVarChar, 3) '受注明細№

                Dim WW_DATENOW As DateTime = Date.Now
                For Each OIT0002row As DataRow In OIT0002tbl.Select("ORDERNO <> ''", "ORDERNO, DETAILNO")

                    P_ORDERNO.Value = OIT0002row("ORDERNO")                 '受注№
                    P_DETAILNO.Value = OIT0002row("DETAILNO")               '受注明細№

                    P_LINEORDER.Value = ""               '貨物駅入線順
                    'P_LINEORDER.Value = OIT0002row("LINECNT")               '貨物駅入線順
                    P_TANKNO.Value = OIT0002row("TANKNUMBER")               'タンク車№
                    P_STACKINGFLG.Value = "2"                               '積置可否フラグ
                    P_WHOLESALEFLG.Value = "2"                              '未卸可否フラグ
                    P_INSPECTIONFLG.Value = "2"                             '交検可否フラグ
                    P_DETENTIONFLG.Value = "2"                              '留置可否フラグ
                    P_FIRSTRETURNFLG.Value = "2"                            '先返し可否フラグ
                    P_AFTERRETURNFLG.Value = "2"                            '後返し可否フラグ
                    '# OT輸送可否フラグ(1:OT輸送あり 2:OT輸送なし)
                    'P_OTTRANSPORTFLG.Value = "2"                            'OT輸送可否フラグ
                    If OIT0002row("OTTRANSPORTFLG") = "on" Then
                        P_OTTRANSPORTFLG.Value = "1"
                    Else
                        P_OTTRANSPORTFLG.Value = "2"
                    End If

                    P_ORDERINFO.Value = ""                                  '受注情報
                    P_SHIPPERSCODE.Value = OIT0002row("SHIPPERSCODE")       '荷主コード
                    P_SHIPPERSNAME.Value = OIT0002row("SHIPPERSNAME")       '荷主名

                    P_OILCODE.Value = OIT0002row("OILCODE")                 '油種コード
                    P_OILNAME.Value = OIT0002row("OILNAME")                 '油種名
                    P_ORDERINGTYPE.Value = OIT0002row("ORDERINGTYPE")       '油種区分(受発注用)
                    P_ORDERINGOILNAME.Value = OIT0002row("ORDERINGOILNAME") '油種名(受発注用)
                    P_CARSNUMBER.Value = 1                                  '車数
                    P_CARSAMOUNT.Value = 0                                  '数量

                    '### 20200928 START 指摘票対応(全体(No149)) #######################################
                    P_RETURNDATETRAIN.Value = Me.TxtBTrainNo.Text           '返送日列車
                    '### 20200928 START 指摘票対応(全体(No149)) #######################################
                    P_FILLINGPOINT.Value = OIT0002row("FILLINGPOINT")       '充填ポイント
                    P_LINE.Value = OIT0002row("LINE")                       '回線
                    P_LOADINGIRILINETRAINNO.Value = OIT0002row("LOADINGIRILINETRAINNO")     '積込入線列車番号
                    P_LOADINGIRILINETRAINNAME.Value = OIT0002row("LOADINGIRILINETRAINNAME") '積込入線列車番号名
                    P_LOADINGIRILINEORDER.Value = ""         '積込入線順
                    'P_LOADINGIRILINEORDER.Value = OIT0002row("LOADINGIRILINEORDER")         '積込入線順
                    P_LOADINGOUTLETTRAINNO.Value = OIT0002row("LOADINGOUTLETTRAINNO")       '積込出線列車番号
                    P_LOADINGOUTLETTRAINNAME.Value = OIT0002row("LOADINGOUTLETTRAINNAME")   '積込出線列車番号名
                    P_LOADINGOUTLETORDER.Value = ""           '積込出線順
                    'P_LOADINGOUTLETORDER.Value = OIT0002row("LOADINGOUTLETORDER")           '積込出線順

                    ''貨物駅入線順を積込入線順に設定
                    'P_LOADINGIRILINEORDER.Value = OIT0002row("LINEORDER")
                    ''積込出線順に(明細数 - 積込入線順 + 1)設定
                    'P_LOADINGOUTLETORDER.Value = (OIT0002tbl.Rows.Count - Integer.Parse(OIT0002row("LINEORDER"))) + 1

                    P_RESERVEDNO.Value = ""                                 '予約番号
                    P_OTSENDCOUNT.Value = "0"                               'OT発送日報送信回数
                    P_DLRESERVEDCOUNT.Value = "0"                           '出荷予約ダウンロード回数
                    P_DLTAKUSOUCOUNT.Value = "0"                            '託送状ダウンロード回数

                    P_SALSE.Value = "0"                                     '売上金額
                    P_SALSETAX.Value = "0"                                  '売上消費税額
                    P_TOTALSALSE.Value = "0"                                '売上合計金額
                    P_PAYMENT.Value = "0"                                   '支払金額
                    P_PAYMENTTAX.Value = "0"                                '支払消費税額
                    P_TOTALPAYMENT.Value = "0"                              '支払合計金額
                    P_DELFLG.Value = OIT0002row("DELFLG")                   '削除フラグ
                    P_INITYMD.Value = WW_DATENOW                            '登録年月日
                    P_INITUSER.Value = Master.USERID                        '登録ユーザーID
                    P_INITTERMID.Value = Master.USERTERMID                  '登録端末
                    P_UPDYMD.Value = WW_DATENOW                             '更新年月日
                    P_UPDUSER.Value = Master.USERID                         '更新ユーザーID
                    P_UPDTERMID.Value = Master.USERTERMID                   '更新端末
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    JP_ORDERNO.Value = OIT0002row("ORDERNO")                 '受注№
                    JP_DETAILNO.Value = OIT0002row("DETAILNO")               '受注明細№

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0002UPDtbl) Then
                            OIT0002UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0002UPDtbl.Clear()
                        OIT0002UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0002UPDrow As DataRow In OIT0002UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0002D"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D UPDATE_INSERT_ORDERDETAIL", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D UPDATE_INSERT_ORDERDETAIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateORDER(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002UPDtbl) Then
            OIT0002UPDtbl = New DataTable
        End If

        If OIT0002UPDtbl.Columns.Count <> 0 Then
            OIT0002UPDtbl.Columns.Clear()
        End If

        OIT0002UPDtbl.Clear()

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
            & "        ORDERNO          = @ORDERNO" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0002_ORDER" _
            & "    SET" _
            & "        OFFICECODE        = @OFFICECODE   , OFFICENAME     = @OFFICENAME" _
            & "        , TRAINNO         = @TRAINNO      , TRAINNAME      = @TRAINNAME" _
            & "        , ORDERTYPE       = @ORDERTYPE" _
            & "        , SHIPPERSCODE    = @SHIPPERSCODE , SHIPPERSNAME   = @SHIPPERSNAME" _
            & "        , BASECODE        = @BASECODE     , BASENAME       = @BASENAME" _
            & "        , CONSIGNEECODE   = @CONSIGNEECODE, CONSIGNEENAME  = @CONSIGNEENAME" _
            & "        , DEPSTATION      = @DEPSTATION   , DEPSTATIONNAME = @DEPSTATIONNAME" _
            & "        , ARRSTATION      = @ARRSTATION   , ARRSTATIONNAME = @ARRSTATIONNAME" _
            & "        , ORDERINFO       = @ORDERINFO    , STACKINGFLG    = @STACKINGFLG" _
            & "        , LODDATE         = @LODDATE      , DEPDATE        = @DEPDATE" _
            & "        , ARRDATE         = @ARRDATE      , ACCDATE        = @ACCDATE" _
            & "        , EMPARRDATE      = @EMPARRDATE" _
            & "        , UPDYMD          = @UPDYMD       , UPDUSER        = @UPDUSER" _
            & "        , UPDTERMID       = @UPDTERMID    , RECEIVEYMD     = @RECEIVEYMD" _
            & "    WHERE" _
            & "        ORDERNO          = @ORDERNO" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0002_ORDER" _
            & "        ( ORDERNO      , TRAINNO         , TRAINNAME       , ORDERYMD            , OFFICECODE , OFFICENAME" _
            & "        , ORDERTYPE    , SHIPPERSCODE    , SHIPPERSNAME    , BASECODE            , BASENAME" _
            & "        , CONSIGNEECODE, CONSIGNEENAME   , DEPSTATION      , DEPSTATIONNAME      , ARRSTATION , ARRSTATIONNAME" _
            & "        , ORDERSTATUS  , ORDERINFO " _
            & "        , EMPTYTURNFLG , STACKINGFLG     , USEPROPRIETYFLG , CONTACTFLG          , RESULTFLG  , DELIVERYFLG   , DELIVERYCOUNT" _
            & "        , LODDATE      , DEPDATE         , ARRDATE         , ACCDATE             , EMPARRDATE " _
            & "        , RTANK        , HTANK           , TTANK           , MTTANK " _
            & "        , KTANK        , K3TANK          , K5TANK          , K10TANK" _
            & "        , LTANK        , ATANK           , OTHER1OTANK     , OTHER2OTANK         , OTHER3OTANK" _
            & "        , OTHER4OTANK  , OTHER5OTANK     , OTHER6OTANK     , OTHER7OTANK         , OTHER8OTANK" _
            & "        , OTHER9OTANK  , OTHER10OTANK    , TOTALTANK" _
            & "        , RTANKCH      , HTANKCH         , TTANKCH         , MTTANKCH            , KTANKCH" _
            & "        , K3TANKCH     , K5TANKCH        , K10TANKCH       , LTANKCH             , ATANKCH" _
            & "        , OTHER1OTANKCH, OTHER2OTANKCH   , OTHER3OTANKCH   , OTHER4OTANKCH       , OTHER5OTANKCH" _
            & "        , OTHER6OTANKCH, OTHER7OTANKCH   , OTHER8OTANKCH   , OTHER9OTANKCH       , OTHER10OTANKCH" _
            & "        , TOTALTANKCH  , KEIJYOYMD       , SALSE           , SALSETAX            , TOTALSALSE" _
            & "        , PAYMENT      , PAYMENTTAX      , TOTALPAYMENT" _
            & "        , RECEIVECOUNT , OTSENDSTATUS    , RESERVEDSTATUS  , TAKUSOUSTATUS" _
            & "        , BTRAINNO     , BTRAINNAME" _
            & "        , DELFLG       , INITYMD         , INITUSER        , INITTERMID" _
            & "        , UPDYMD       , UPDUSER         , UPDTERMID       , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @ORDERNO      , @TRAINNO      , @TRAINNAME      , @ORDERYMD      , @OFFICECODE, @OFFICENAME" _
            & "        , @ORDERTYPE    , @SHIPPERSCODE , @SHIPPERSNAME   , @BASECODE      , @BASENAME" _
            & "        , @CONSIGNEECODE, @CONSIGNEENAME, @DEPSTATION     , @DEPSTATIONNAME, @ARRSTATION, @ARRSTATIONNAME" _
            & "        , @ORDERSTATUS  , @ORDERINFO" _
            & "        , @EMPTYTURNFLG , @STACKINGFLG  , @USEPROPRIETYFLG, @CONTACTFLG    , @RESULTFLG , @DELIVERYFLG   , @DELIVERYCOUNT" _
            & "        , @LODDATE      , @DEPDATE      , @ARRDATE        , @ACCDATE       , @EMPARRDATE" _
            & "        , @RTANK        , @HTANK        , @TTANK          , @MTTANK" _
            & "        , @KTANK        , @K3TANK       , @K5TANK         , @K10TANK" _
            & "        , @LTANK        , @ATANK        , @OTHER1OTANK    , @OTHER2OTANK   , @OTHER3OTANK" _
            & "        , @OTHER4OTANK  , @OTHER5OTANK  , @OTHER6OTANK    , @OTHER7OTANK   , @OTHER8OTANK" _
            & "        , @OTHER9OTANK  , @OTHER10OTANK , @TOTALTANK" _
            & "        , @RTANKCH      , @HTANKCH      , @TTANKCH        , @MTTANKCH      , @KTANKCH" _
            & "        , @K3TANKCH     , @K5TANKCH     , @K10TANKCH      , @LTANKCH       , @ATANKCH" _
            & "        , @OTHER1OTANKCH, @OTHER2OTANKCH, @OTHER3OTANKCH  , @OTHER4OTANKCH , @OTHER5OTANKCH" _
            & "        , @OTHER6OTANKCH, @OTHER7OTANKCH, @OTHER8OTANKCH  , @OTHER9OTANKCH , @OTHER10OTANKCH" _
            & "        , @TOTALTANKCH  , @KEIJYOYMD    , @SALSE          , @SALSETAX      , @TOTALSALSE" _
            & "        , @PAYMENT      , @PAYMENTTAX   , @TOTALPAYMENT" _
            & "        , @RECEIVECOUNT , @OTSENDSTATUS , @RESERVEDSTATUS , @TAKUSOUSTATUS" _
            & "        , @BTRAINNO     , @BTRAINNAME" _
            & "        , @DELFLG       , @INITYMD      , @INITUSER       , @INITTERMID" _
            & "        , @UPDYMD       , @UPDUSER      , @UPDTERMID      , @RECEIVEYMD) ;" _
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
            & "    , BTRAINNO" _
            & "    , BTRAINNAME" _
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
            & "        ORDERNO = @ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 11) '受注№
                Dim P_TRAINNO As SqlParameter = SQLcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar, 4)  '本線列車
                Dim P_TRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@TRAINNAME", SqlDbType.NVarChar, 20) '本線列車名
                Dim P_ORDERYMD As SqlParameter = SQLcmd.Parameters.Add("@ORDERYMD", SqlDbType.Date)         '受注登録日
                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim P_OFFICENAME As SqlParameter = SQLcmd.Parameters.Add("@OFFICENAME", SqlDbType.NVarChar, 20) '受注営業所名
                Dim P_ORDERTYPE As SqlParameter = SQLcmd.Parameters.Add("@ORDERTYPE", SqlDbType.NVarChar, 7)  '受注パターン
                Dim P_SHIPPERSCODE As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSCODE", SqlDbType.NVarChar, 10) '荷主コード
                Dim P_SHIPPERSNAME As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSNAME", SqlDbType.NVarChar, 40) '荷主名
                Dim P_BASECODE As SqlParameter = SQLcmd.Parameters.Add("@BASECODE", SqlDbType.NVarChar, 9)  '基地コード
                Dim P_BASENAME As SqlParameter = SQLcmd.Parameters.Add("@BASENAME", SqlDbType.NVarChar, 40) '基地名
                Dim P_CONSIGNEECODE As SqlParameter = SQLcmd.Parameters.Add("@CONSIGNEECODE", SqlDbType.NVarChar, 10) '荷受人コード
                Dim P_CONSIGNEENAME As SqlParameter = SQLcmd.Parameters.Add("@CONSIGNEENAME", SqlDbType.NVarChar, 40) '荷受人名
                Dim P_DEPSTATION As SqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", SqlDbType.NVarChar, 7)  '発駅コード
                Dim P_DEPSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@DEPSTATIONNAME", SqlDbType.NVarChar, 40) '発駅名
                Dim P_ARRSTATION As SqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", SqlDbType.NVarChar, 7)  '着駅コード
                Dim P_ARRSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@ARRSTATIONNAME", SqlDbType.NVarChar, 40) '着駅名
                Dim P_ORDERSTATUS As SqlParameter = SQLcmd.Parameters.Add("@ORDERSTATUS", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim P_ORDERINFO As SqlParameter = SQLcmd.Parameters.Add("@ORDERINFO", SqlDbType.NVarChar, 2)  '受注情報
                Dim P_EMPTYTURNFLG As SqlParameter = SQLcmd.Parameters.Add("@EMPTYTURNFLG", SqlDbType.NVarChar, 1)  '空回日報可否フラグ
                Dim P_STACKINGFLG As SqlParameter = SQLcmd.Parameters.Add("@STACKINGFLG", SqlDbType.NVarChar, 1)  '積置可否フラグ
                Dim P_USEPROPRIETYFLG As SqlParameter = SQLcmd.Parameters.Add("@USEPROPRIETYFLG", SqlDbType.NVarChar, 1)  '利用可否フラグ
                Dim P_CONTACTFLG As SqlParameter = SQLcmd.Parameters.Add("@CONTACTFLG", SqlDbType.NVarChar, 1)  '手配連絡フラグ
                Dim P_RESULTFLG As SqlParameter = SQLcmd.Parameters.Add("@RESULTFLG", SqlDbType.NVarChar, 1)    '結果受理フラグ
                Dim P_DELIVERYFLG As SqlParameter = SQLcmd.Parameters.Add("@DELIVERYFLG", SqlDbType.NVarChar, 1) '託送指示フラグ
                Dim P_DELIVERYCOUNT As SqlParameter = SQLcmd.Parameters.Add("@DELIVERYCOUNT", SqlDbType.Int)     '託送指示送信回数
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.Date)               '積込日（予定）
                Dim P_DEPDATE As SqlParameter = SQLcmd.Parameters.Add("@DEPDATE", SqlDbType.Date)               '発日（予定）
                Dim P_ARRDATE As SqlParameter = SQLcmd.Parameters.Add("@ARRDATE", SqlDbType.Date)               '積車着日（予定）
                Dim P_ACCDATE As SqlParameter = SQLcmd.Parameters.Add("@ACCDATE", SqlDbType.Date)               '受入日（予定）
                Dim P_EMPARRDATE As SqlParameter = SQLcmd.Parameters.Add("@EMPARRDATE", SqlDbType.Date)         '空車着日（予定）
                Dim P_RTANK As SqlParameter = SQLcmd.Parameters.Add("@RTANK", SqlDbType.Int)                    '車数（レギュラー）
                Dim P_HTANK As SqlParameter = SQLcmd.Parameters.Add("@HTANK", SqlDbType.Int)                    '車数（ハイオク）
                Dim P_TTANK As SqlParameter = SQLcmd.Parameters.Add("@TTANK", SqlDbType.Int)                    '車数（灯油）
                Dim P_MTTANK As SqlParameter = SQLcmd.Parameters.Add("@MTTANK", SqlDbType.Int)                  '車数（未添加灯油）
                Dim P_KTANK As SqlParameter = SQLcmd.Parameters.Add("@KTANK", SqlDbType.Int)                    '車数（軽油）
                Dim P_K3TANK As SqlParameter = SQLcmd.Parameters.Add("@K3TANK", SqlDbType.Int)                  '車数（３号軽油）
                Dim P_K5TANK As SqlParameter = SQLcmd.Parameters.Add("@K5TANK", SqlDbType.Int)                  '車数（５号軽油）
                Dim P_K10TANK As SqlParameter = SQLcmd.Parameters.Add("@K10TANK", SqlDbType.Int)                '車数（１０号軽油）
                Dim P_LTANK As SqlParameter = SQLcmd.Parameters.Add("@LTANK", SqlDbType.Int)                    '車数（LSA）
                Dim P_ATANK As SqlParameter = SQLcmd.Parameters.Add("@ATANK", SqlDbType.Int)                    '車数（A重油）
                Dim P_OTHER1OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER1OTANK", SqlDbType.Int)        '車数（その他１）
                Dim P_OTHER2OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER2OTANK", SqlDbType.Int)        '車数（その他２）
                Dim P_OTHER3OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER3OTANK", SqlDbType.Int)        '車数（その他３）
                Dim P_OTHER4OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER4OTANK", SqlDbType.Int)        '車数（その他４）
                Dim P_OTHER5OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER5OTANK", SqlDbType.Int)        '車数（その他５）
                Dim P_OTHER6OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER6OTANK", SqlDbType.Int)        '車数（その他６）
                Dim P_OTHER7OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER7OTANK", SqlDbType.Int)        '車数（その他７）
                Dim P_OTHER8OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER8OTANK", SqlDbType.Int)        '車数（その他８）
                Dim P_OTHER9OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER9OTANK", SqlDbType.Int)        '車数（その他９）
                Dim P_OTHER10OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER10OTANK", SqlDbType.Int)      '車数（その他１０）
                Dim P_TOTALTANK As SqlParameter = SQLcmd.Parameters.Add("@TOTALTANK", SqlDbType.Int)            '合計車数
                Dim P_RTANKCH As SqlParameter = SQLcmd.Parameters.Add("@RTANKCH", SqlDbType.Int)                '変更後_車数（レギュラー）
                Dim P_HTANKCH As SqlParameter = SQLcmd.Parameters.Add("@HTANKCH", SqlDbType.Int)                '変更後_車数（ハイオク）
                Dim P_TTANKCH As SqlParameter = SQLcmd.Parameters.Add("@TTANKCH", SqlDbType.Int)                '変更後_車数（灯油）
                Dim P_MTTANKCH As SqlParameter = SQLcmd.Parameters.Add("@MTTANKCH", SqlDbType.Int)              '変更後_車数（未添加灯油）
                Dim P_KTANKCH As SqlParameter = SQLcmd.Parameters.Add("@KTANKCH", SqlDbType.Int)                '変更後_車数（軽油）
                Dim P_K3TANKCH As SqlParameter = SQLcmd.Parameters.Add("@K3TANKCH", SqlDbType.Int)              '変更後_車数（３号軽油）
                Dim P_K5TANKCH As SqlParameter = SQLcmd.Parameters.Add("@K5TANKCH", SqlDbType.Int)              '変更後_車数（５号軽油）
                Dim P_K10TANKCH As SqlParameter = SQLcmd.Parameters.Add("@K10TANKCH", SqlDbType.Int)            '変更後_車数（１０号軽油）
                Dim P_LTANKCH As SqlParameter = SQLcmd.Parameters.Add("@LTANKCH", SqlDbType.Int)                '変更後_車数（LSA）
                Dim P_ATANKCH As SqlParameter = SQLcmd.Parameters.Add("@ATANKCH", SqlDbType.Int)                '変更後_車数（A重油）
                Dim P_OTHER1OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER1OTANKCH", SqlDbType.Int)    '変更後_車数（その他１）
                Dim P_OTHER2OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER2OTANKCH", SqlDbType.Int)    '変更後_車数（その他２）
                Dim P_OTHER3OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER3OTANKCH", SqlDbType.Int)    '変更後_車数（その他３）
                Dim P_OTHER4OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER4OTANKCH", SqlDbType.Int)    '変更後_車数（その他４）
                Dim P_OTHER5OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER5OTANKCH", SqlDbType.Int)    '変更後_車数（その他５）
                Dim P_OTHER6OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER6OTANKCH", SqlDbType.Int)    '変更後_車数（その他６）
                Dim P_OTHER7OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER7OTANKCH", SqlDbType.Int)    '変更後_車数（その他７）
                Dim P_OTHER8OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER8OTANKCH", SqlDbType.Int)    '変更後_車数（その他８）
                Dim P_OTHER9OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER9OTANKCH", SqlDbType.Int)    '変更後_車数（その他９）
                Dim P_OTHER10OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER10OTANKCH", SqlDbType.Int)  '変更後_車数（その他１０）
                Dim P_TOTALTANKCH As SqlParameter = SQLcmd.Parameters.Add("@TOTALTANKCH", SqlDbType.Int)        '変更後_合計車数
                Dim P_KEIJYOYMD As SqlParameter = SQLcmd.Parameters.Add("@KEIJYOYMD", SqlDbType.Date)           '計上日
                Dim P_SALSE As SqlParameter = SQLcmd.Parameters.Add("@SALSE", SqlDbType.Int)                    '売上金額
                Dim P_SALSETAX As SqlParameter = SQLcmd.Parameters.Add("@SALSETAX", SqlDbType.Int)              '売上消費税額
                Dim P_TOTALSALSE As SqlParameter = SQLcmd.Parameters.Add("@TOTALSALSE", SqlDbType.Int)          '売上合計金額
                Dim P_PAYMENT As SqlParameter = SQLcmd.Parameters.Add("@PAYMENT", SqlDbType.Int)                '支払金額
                Dim P_PAYMENTTAX As SqlParameter = SQLcmd.Parameters.Add("@PAYMENTTAX", SqlDbType.Int)          '支払消費税額
                Dim P_TOTALPAYMENT As SqlParameter = SQLcmd.Parameters.Add("@TOTALPAYMENT", SqlDbType.Int)      '支払合計金額
                Dim P_RECEIVECOUNT As SqlParameter = SQLcmd.Parameters.Add("@RECEIVECOUNT", SqlDbType.Int)             'OT空回日報受信回数
                Dim P_OTSENDSTATUS As SqlParameter = SQLcmd.Parameters.Add("@OTSENDSTATUS", SqlDbType.NVarChar, 1)     'OT発送日報送信状況
                Dim P_RESERVEDSTATUS As SqlParameter = SQLcmd.Parameters.Add("@RESERVEDSTATUS", SqlDbType.NVarChar, 1) '出荷予約ダウンロード状況
                Dim P_TAKUSOUSTATUS As SqlParameter = SQLcmd.Parameters.Add("@TAKUSOUSTATUS", SqlDbType.NVarChar, 1)   '託送状ダウンロード状況
                '### 20200928 START 指摘票対応(全体(No149)) ###############################################################
                Dim P_BTRAINNO As SqlParameter = SQLcmd.Parameters.Add("@BTRAINNO", SqlDbType.NVarChar, 4)      '返送列車
                Dim P_BTRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@BTRAINNAME", SqlDbType.NVarChar, 20) '返送列車名
                '### 20200928 END   指摘票対応(全体(No149)) ###############################################################
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)          '削除フラグ
                Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", SqlDbType.DateTime)           '登録年月日
                Dim P_INITUSER As SqlParameter = SQLcmd.Parameters.Add("@INITUSER", SqlDbType.NVarChar, 20)     '登録ユーザーID
                Dim P_INITTERMID As SqlParameter = SQLcmd.Parameters.Add("@INITTERMID", SqlDbType.NVarChar, 20) '登録端末
                Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)             '更新年月日
                Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)       '更新ユーザーID
                Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar, 20)   '更新端末
                Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)     '集信日時

                Dim JP_ORDERNO As SqlParameter = SQLcmdJnl.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 11)   '受注№

                Dim WW_DATENOW As DateTime = Date.Now
                Dim iresult As Integer
                Dim strOilCnt() As String

                For Each OIT0002row As DataRow In OIT0002tbl.Rows

                    '★受注№が未設定の場合は次レコード
                    If OIT0002row("ORDERNO") = "" Then Continue For

                    'DB更新
                    P_ORDERNO.Value = OIT0002row("ORDERNO")                       '受注№
                    P_TRAINNO.Value = OIT0002row("LOADINGTRAINNO")                '本線列車
                    P_TRAINNAME.Value = OIT0002row("LOADINGTRAINNAME")            '本線列車名
                    P_ORDERYMD.Value = WW_DATENOW                                 '受注登録日
                    P_OFFICECODE.Value = OIT0002row("OFFICECODE")                 '受注営業所コード
                    P_OFFICENAME.Value = OIT0002row("OFFICENAME")                 '受注営業所名
                    P_ORDERTYPE.Value = OIT0002row("PATTERNCODE")                 '受注パターン
                    P_SHIPPERSCODE.Value = OIT0002row("SHIPPERSCODE")             '荷主コード
                    P_SHIPPERSNAME.Value = OIT0002row("SHIPPERSNAME")             '荷主名
                    P_BASECODE.Value = OIT0002row("BASECODE")                     '基地コード
                    P_BASENAME.Value = OIT0002row("BASENAME")                     '基地名
                    P_CONSIGNEECODE.Value = OIT0002row("CONSIGNEECODE")           '荷受人コード
                    P_CONSIGNEENAME.Value = OIT0002row("CONSIGNEENAME")           '荷受人名
                    P_DEPSTATION.Value = OIT0002row("LOADINGDEPSTATION")          '発駅コード
                    P_DEPSTATIONNAME.Value = OIT0002row("LOADINGDEPSTATIONNAME")  '発駅名
                    P_ARRSTATION.Value = OIT0002row("LOADINGRETSTATION")          '着駅コード
                    P_ARRSTATIONNAME.Value = OIT0002row("LOADINGRETSTATIONNAME")  '着駅名

                    P_ORDERSTATUS.Value = BaseDllConst.CONST_ORDERSTATUS_100      '受注進行ステータス
                    P_ORDERINFO.Value = ""                                        '受注情報
                    P_EMPTYTURNFLG.Value = "3"                                    '空回日報可否フラグ(3:作成(貨車連結表から作成))

                    '〇 積込日 < 発日 の場合 
                    iresult = Date.Parse(OIT0002row("LOADINGLODDATE")).CompareTo(Date.Parse(OIT0002row("LOADINGDEPDATE")))
                    '例) iresult = dt1.Date.CompareTo(dt2.Date)
                    '    iresultの意味
                    '     0 : dt1とdt2は同じ日
                    '    -1 : dt1はdt2より前の日
                    '     1 : dt1はdt2より後の日
                    If iresult = -1 Then
                        P_STACKINGFLG.Value = "1"                         '積置可否フラグ(1:積置あり)
                    Else
                        P_STACKINGFLG.Value = "2"                         '積置可否フラグ(2:積置なし)
                    End If

                    P_USEPROPRIETYFLG.Value = "1"                         '利用可否フラグ(1:利用可能)
                    P_CONTACTFLG.Value = "0"                              '手配連絡フラグ(0:未連絡)
                    P_RESULTFLG.Value = "0"                               '結果受理フラグ(0:未受理)
                    P_DELIVERYFLG.Value = "0"                             '託送指示フラグ(0:未手配, 1:手配)
                    P_DELIVERYCOUNT.Value = "0"                           '託送指示送信回数
                    '積込日（予定）
                    If OIT0002row("LOADINGLODDATE") <> "" Then
                        P_LODDATE.Value = OIT0002row("LOADINGLODDATE")
                    Else
                        P_LODDATE.Value = DBNull.Value
                    End If
                    '発日（予定）
                    If OIT0002row("LOADINGDEPDATE") <> "" Then
                        P_DEPDATE.Value = OIT0002row("LOADINGDEPDATE")
                    Else
                        P_DEPDATE.Value = DBNull.Value
                    End If
                    '積車着日（予定）
                    If OIT0002row("LOADINGARRDATE") <> "" Then
                        P_ARRDATE.Value = OIT0002row("LOADINGARRDATE")
                    Else
                        P_ARRDATE.Value = DBNull.Value
                    End If
                    '受入日（予定）
                    If OIT0002row("LOADINGACCDATE") <> "" Then
                        P_ACCDATE.Value = OIT0002row("LOADINGACCDATE")
                    Else
                        P_ACCDATE.Value = DBNull.Value
                    End If
                    '空車着日（予定）
                    If OIT0002row("LOADINGEMPARRDATE") <> "" Then
                        P_EMPARRDATE.Value = OIT0002row("LOADINGEMPARRDATE")
                    Else
                        P_EMPARRDATE.Value = DBNull.Value
                    End If

                    '★受注登録用油種数カウント用
                    strOilCnt = {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"}
                    '油種別タンク車数、積込数量データ取得
                    WW_OILTANKCntGet(SQLcon, P_ORDERNO.Value, strOilCnt)

                    P_HTANK.Value = strOilCnt(1)                        '車数（ハイオク）
                    P_RTANK.Value = strOilCnt(2)                        '車数（レギュラー）
                    P_TTANK.Value = strOilCnt(3)                        '車数（灯油）
                    P_MTTANK.Value = strOilCnt(4)                       '車数（未添加灯油）
                    P_KTANK.Value = strOilCnt(5)                        '車数（軽油）
                    P_K3TANK.Value = strOilCnt(6)                       '車数（３号軽油）
                    P_K5TANK.Value = strOilCnt(7)                       '車数（５号軽油）
                    P_K10TANK.Value = strOilCnt(8)                      '車数（１０号軽油）
                    P_LTANK.Value = strOilCnt(9)                        '車数（LSA）
                    P_ATANK.Value = strOilCnt(10)                       '車数（A重油）

                    P_OTHER1OTANK.Value = 0                             '車数（その他１）
                    P_OTHER2OTANK.Value = 0                             '車数（その他２）
                    P_OTHER3OTANK.Value = 0                             '車数（その他３）
                    P_OTHER4OTANK.Value = 0                             '車数（その他４）
                    P_OTHER5OTANK.Value = 0                             '車数（その他５）
                    P_OTHER6OTANK.Value = 0                             '車数（その他６）
                    P_OTHER7OTANK.Value = 0                             '車数（その他７）
                    P_OTHER8OTANK.Value = 0                             '車数（その他８）
                    P_OTHER9OTANK.Value = 0                             '車数（その他９）
                    P_OTHER10OTANK.Value = 0                            '車数（その他１０）
                    P_TOTALTANK.Value = strOilCnt(0)                    '合計車数

                    P_HTANKCH.Value = strOilCnt(1)                      '変更後_車数（ハイオク）
                    P_RTANKCH.Value = strOilCnt(2)                      '変更後_車数（レギュラー）
                    P_TTANKCH.Value = strOilCnt(3)                      '変更後_車数（灯油）
                    P_MTTANKCH.Value = strOilCnt(4)                     '変更後_車数（未添加灯油）
                    P_KTANKCH.Value = strOilCnt(5)                      '変更後_車数（軽油）
                    P_K3TANKCH.Value = strOilCnt(6)                     '変更後_車数（３号軽油）
                    P_K5TANKCH.Value = strOilCnt(7)                     '変更後_車数（５号軽油）
                    P_K10TANKCH.Value = strOilCnt(8)                    '変更後_車数（１０号軽油）
                    P_LTANKCH.Value = strOilCnt(9)                      '変更後_車数（LSA）
                    P_ATANKCH.Value = strOilCnt(10)                     '変更後_車数（A重油）
                    P_OTHER1OTANKCH.Value = 0                           '変更後_車数（その他１）
                    P_OTHER2OTANKCH.Value = 0                           '変更後_車数（その他２）
                    P_OTHER3OTANKCH.Value = 0                           '変更後_車数（その他３）
                    P_OTHER4OTANKCH.Value = 0                           '変更後_車数（その他４）
                    P_OTHER5OTANKCH.Value = 0                           '変更後_車数（その他５）
                    P_OTHER6OTANKCH.Value = 0                           '変更後_車数（その他６）
                    P_OTHER7OTANKCH.Value = 0                           '変更後_車数（その他７）
                    P_OTHER8OTANKCH.Value = 0                           '変更後_車数（その他８）
                    P_OTHER9OTANKCH.Value = 0                           '変更後_車数（その他９）
                    P_OTHER10OTANKCH.Value = 0                          '変更後_車数（その他１０）
                    P_TOTALTANKCH.Value = strOilCnt(0)                  '変更後_合計車数

                    P_KEIJYOYMD.Value = DBNull.Value                    '計上日
                    P_SALSE.Value = 0                                   '売上金額
                    P_SALSETAX.Value = 0                                '売上消費税額
                    P_TOTALSALSE.Value = 0                              '売上合計金額
                    P_PAYMENT.Value = 0                                 '支払金額
                    P_PAYMENTTAX.Value = 0                              '支払消費税額
                    P_TOTALPAYMENT.Value = 0                            '支払合計金額

                    P_RECEIVECOUNT.Value = 0                            'OT空回日報受信回数
                    P_OTSENDSTATUS.Value = "0"                          'OT発送日報送信状況
                    P_RESERVEDSTATUS.Value = "0"                        '出荷予約ダウンロード状況
                    P_TAKUSOUSTATUS.Value = "0"                         '託送状ダウンロード状況
                    '### 20200928 START 指摘票対応(全体(No149)) #################################
                    P_BTRAINNO.Value = Me.TxtBTrainNo.Text
                    P_BTRAINNAME.Value = Me.LblBTrainName.Text
                    '### 20200928 START 指摘票対応(全体(No149)) #################################

                    P_DELFLG.Value = "0"                                '削除フラグ
                    P_INITYMD.Value = WW_DATENOW                        '登録年月日
                    P_INITUSER.Value = Master.USERID                    '登録ユーザーID
                    P_INITTERMID.Value = Master.USERTERMID              '登録端末
                    P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                    P_UPDUSER.Value = Master.USERID                     '更新ユーザーID
                    P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JP_ORDERNO.Value = OIT0002row("ORDERNO")

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0002UPDtbl) Then
                            OIT0002UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0002UPDtbl.Clear()
                        OIT0002UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0002UPDrow As DataRow In OIT0002UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0002D"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D UPDATE_INSERT_ORDER", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D UPDATE_INSERT_ORDER"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 油種別タンク車数、積込数量データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_OILTANKCntGet(ByVal SQLcon As SqlConnection, ByVal OrderNo As String, ByRef OilCnt() As String)

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

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
                PARA01.Value = OrderNo
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
                OilCnt(0) = "0"             'タンク車数合計
                OilCnt(1) = "0"             '油種(ハイオク)
                OilCnt(2) = "0"             '油種(レギュラー)
                OilCnt(3) = "0"             '油種(灯油)
                OilCnt(4) = "0"             '油種(未添加灯油)
                OilCnt(5) = "0"             '油種(軽油)
                OilCnt(6) = "0"             '油種(３号軽油)
                OilCnt(7) = "0"             '油種(５号軽油)
                OilCnt(8) = "0"             '油種(１０号軽油)
                OilCnt(9) = "0"             '油種(ＬＳＡ)
                OilCnt(10) = "0"            '油種(Ａ重油)
                ''〇 積込数量(kl)
                'Me.TxtHTank_c2.Text = "0"
                'Me.TxtRTank_c2.Text = "0"
                'Me.TxtTTank_c2.Text = "0"
                'Me.TxtMTTank_c2.Text = "0"
                'Me.TxtKTank_c2.Text = "0"
                'Me.TxtK3Tank_c2.Text = "0"
                'Me.TxtK5Tank_c2.Text = "0"
                'Me.TxtK10Tank_c2.Text = "0"
                'Me.TxtLTank_c2.Text = "0"
                'Me.TxtATank_c2.Text = "0"
                'Me.TxtTotalCnt_c2.Text = "0"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                    i += 1
                    OIT0002WKrow("LINECNT") = i        'LINECNT

                    '[ヘッダー]
                    '〇 油種別タンク車数(車)
                    OilCnt(0) = OIT0002WKrow("TOTAL")             'タンク車数合計
                    OilCnt(1) = OIT0002WKrow("HTANK")             '油種(ハイオク)
                    OilCnt(2) = OIT0002WKrow("RTANK")             '油種(レギュラー)
                    OilCnt(3) = OIT0002WKrow("TTANK")             '油種(灯油)
                    OilCnt(4) = OIT0002WKrow("MTTANK")            '油種(未添加灯油)
                    OilCnt(5) = OIT0002WKrow("KTANK")             '油種(軽油)
                    OilCnt(6) = OIT0002WKrow("K3TANK")            '油種(３号軽油)
                    OilCnt(7) = OIT0002WKrow("K5TANK")            '油種(５号軽油)
                    OilCnt(8) = OIT0002WKrow("K10TANK")           '油種(１０号軽油)
                    OilCnt(9) = OIT0002WKrow("LTANK")             '油種(ＬＳＡ)
                    OilCnt(10) = OIT0002WKrow("ATANK")            '油種(Ａ重油)

                    ''〇 積込数量(kl)
                    'Me.TxtHTank_c2.Text = OIT0002WKrow("HTANKCNT")
                    'Me.TxtRTank_c2.Text = OIT0002WKrow("RTANKCNT")
                    'Me.TxtTTank_c2.Text = OIT0002WKrow("TTANKCNT")
                    'Me.TxtMTTank_c2.Text = OIT0002WKrow("MTTANKCNT")
                    'Me.TxtKTank_c2.Text = OIT0002WKrow("KTANKCNT")
                    'Me.TxtK3Tank_c2.Text = OIT0002WKrow("K3TANKCNT")
                    'Me.TxtK5Tank_c2.Text = OIT0002WKrow("K5TANKCNT")
                    'Me.TxtK10Tank_c2.Text = OIT0002WKrow("K10TANKCNT")
                    'Me.TxtLTank_c2.Text = OIT0002WKrow("LTANKCNT")
                    'Me.TxtATank_c2.Text = OIT0002WKrow("ATANKCNT")
                    'Me.TxtTotalCnt_c2.Text = OIT0002WKrow("TOTALCNT")

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' (貨車連結表TBL)情報更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateLinkInfo(ByVal SQLcon As SqlConnection, ByVal OIT0002row As DataRow)

        Try
            '更新SQL文･･･貨車連結表TBLの情報を更新
            Dim SQLStr As String = ""
            SQLStr =
                " UPDATE OIL.OIT0004_LINK " _
                & "    SET INFO          = @INFO, " _
                & "        UPDYMD        = @UPDYMD, " _
                & "        UPDUSER       = @UPDUSER, " _
                & "        UPDTERMID     = @UPDTERMID, " _
                & "        RECEIVEYMD    = @RECEIVEYMD  " _
                & "  WHERE LINKNO        = @LINKNO " _
                & "    AND LINKDETAILNO  = @LINKDETAILNO " _
                & "    AND DELFLG       <> @DELFLG "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim P_LINKNO As SqlParameter = SQLcmd.Parameters.Add("@LINKNO", System.Data.SqlDbType.NVarChar)
            Dim P_LINKDETAILNO As SqlParameter = SQLcmd.Parameters.Add("@LINKDETAILNO", System.Data.SqlDbType.NVarChar)
            Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
            Dim P_INFO As SqlParameter = SQLcmd.Parameters.Add("@INFO", System.Data.SqlDbType.NVarChar)

            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            P_LINKNO.Value = OIT0002row("LINKNO")
            P_LINKDETAILNO.Value = OIT0002row("RLINKDETAILNO")
            P_DELFLG.Value = C_DELETE_FLG.DELETE
            P_INFO.Value = OIT0002row("ORDERINFO")

            P_UPDYMD.Value = Date.Now
            P_UPDUSER.Value = Master.USERID
            P_UPDTERMID.Value = Master.USERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D_LINKINFO UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D_LINKINFO UPDATE"
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
    ''' 貨車連結順序表(一覧)表示用
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_LinkListTBLSet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを貨車連結順序表テーブルから取得する
        Dim SQLStr As String =
                " SELECT " _
            & "    0                                                             AS LINECNT " _
            & "    , ''                                                          AS OPERATION " _
            & "    , 1                                                           AS 'SELECT' " _
            & "    , 0                                                           AS HIDDEN " _
            & "    , ISNULL(RTRIM(OIT0011.RLINKNO), '')                          AS RLINKNO " _
            & "    , ISNULL(RTRIM(OIT0011.LINKNO), '')                           AS LINKNO " _
            & "    , ''                                                          AS INFO " _
            & "    , ''                                                          AS ORDERINFONAME " _
            & "    , ISNULL(RTRIM(OIT0004.TRAINNO), '')                          AS TRAINNO " _
            & "    , ISNULL(RTRIM(OIT0004.TRAINNAME), '')                        AS TRAINNAME " _
            & "    , ISNULL(RTRIM(OIT0004.OFFICECODE), '')                       AS OFFICECODE " _
            & "    , ''                                                          AS OFFICENAME " _
            & "    , ISNULL(FORMAT(OIT0004.EMPARRDATE, 'yyyy/MM/dd'), NULL)      AS EMPARRDATE " _
            & "    , ISNULL(RTRIM(OIT0004.DEPSTATION), '')                       AS DEPSTATION " _
            & "    , ISNULL(RTRIM(OIT0004.DEPSTATIONNAME), '')                   AS DEPSTATIONNAME " _
            & "    , ISNULL(RTRIM(OIT0004.RETSTATION), '')                       AS RETSTATION " _
            & "    , ISNULL(RTRIM(OIT0004.RETSTATIONNAME), '')                   AS RETSTATIONNAME " _
            & "	   , COUNT(1)                                                    AS TOTALTANK "

        '油種(ハイオク)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS HTANK ", BaseDllConst.CONST_HTank)
        '油種(レギュラー)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS RTANK ", BaseDllConst.CONST_RTank)
        '油種(灯油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS TTANK ", BaseDllConst.CONST_TTank)
        '油種(未添加灯油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS MTTANK ", BaseDllConst.CONST_MTTank)
        '油種(軽油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS KTANK ", BaseDllConst.CONST_KTank1)
        '油種(３号軽油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS K3TANK ", BaseDllConst.CONST_K3Tank1)
        '油種(５号軽油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS K5TANK ", BaseDllConst.CONST_K5Tank)
        '油種(１０号軽油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS K10TANK ", BaseDllConst.CONST_K10Tank)
        '油種(ＬＳＡ)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS LTANK ", BaseDllConst.CONST_LTank1)
        '油種(Ａ重油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS ATANK ", BaseDllConst.CONST_ATank)

        SQLStr &=
              " FROM oil.OIT0011_RLINK OIT0011 " _
            & " INNER JOIN oil.OIT0004_LINK OIT0004 ON " _
            & "     OIT0004.LINKNO       = OIT0011.LINKNO " _
            & " AND OIT0004.LINKDETAILNO = OIT0011.RLINKDETAILNO " _
            & " AND OIT0004.STATUS       = '1' "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '返送列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_SEARCH_BTRAINNO.Text) Then
            SQLStr &= String.Format(" AND OIT0004.TRAINNO      = '{0}'", work.WF_SEL_SEARCH_BTRAINNO.Text)
        End If

        SQLStr &=
              " AND OIT0004.EMPARRDATE  >= @P01 " _
            & " AND OIT0004.DELFLG      <> @P02 " _
            & " WHERE ISNULL(OIT0011.TRUCKSYMBOL,'') <> '' "

        SQLStr &=
              " GROUP BY " _
            & "      OIT0011.RLINKNO " _
            & "	    ,OIT0011.LINKNO " _
            & "	    ,OIT0004.TRAINNO " _
            & "	    ,OIT0004.TRAINNAME " _
            & "	    ,OIT0004.OFFICECODE " _
            & "	    ,OIT0004.EMPARRDATE " _
            & "	    ,OIT0004.DEPSTATION " _
            & "	    ,OIT0004.DEPSTATIONNAME " _
            & "	    ,OIT0004.RETSTATION " _
            & "	    ,OIT0004.RETSTATIONNAME "

        SQLStr &=
              " ORDER BY " _
            & "      OIT0004.TRAINNO "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.Date)                '空車着日
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA01.Value = work.WF_SEL_SEARCH_EMPARRDATE.Text
                PARA02.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                    i += 1
                    OIT0002WKrow("LINECNT") = i        'LINECNT

                    '◯名称取得
                    '受注営業所
                    CODENAME_get("SALESOFFICE", OIT0002WKrow("OFFICECODE"), OIT0002WKrow("OFFICENAME"), WW_DUMMY)                               '会社コード
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D ORDERLISTSET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D OrderListSet"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002WKtbl, work.WF_SEL_INPTBL.Text)

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            Select Case OIT0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
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
        For Each OIT0002INProw As DataRow In OIT0002INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIT0002INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIT0002INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PREOILCODE", OIT0002INProw("PREOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "油種入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'タンク車(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", OIT0002INProw("TANKNUMBER"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "タンク車入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If OIT0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIT0002INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIT0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' OIT0002tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIT0002tbl_UPD()

        '○ 画面状態設定
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            Select Case OIT0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIT0002INProw As DataRow In OIT0002INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIT0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIT0002INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIT0002row As DataRow In OIT0002tbl.Rows
                If OIT0002row("LINKNO") = OIT0002INProw("LINKNO") AndAlso
                    OIT0002row("LINKDETAILNO") = OIT0002INProw("LINKDETAILNO") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIT0002row("DELFLG") = OIT0002INProw("DELFLG") AndAlso
                        OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIT0002INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIT0002INProw As DataRow In OIT0002INPtbl.Rows
            Select Case OIT0002INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIT0002INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIT0002INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIT0002INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIT0002INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIT0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIT0002INProw As DataRow)

        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '同一レコードか判定
            If OIT0002INProw("LINKNO") = OIT0002row("LINKNO") AndAlso
                OIT0002INProw("LINKDETAILNO") = OIT0002row("LINKDETAILNO") Then
                '画面入力テーブル項目設定
                OIT0002INProw("LINECNT") = OIT0002row("LINECNT")
                OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIT0002INProw("UPDTIMSTP") = OIT0002row("UPDTIMSTP")
                OIT0002INProw("SELECT") = 1
                OIT0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIT0002row.ItemArray = OIT0002INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIT0002INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIT0002row As DataRow = OIT0002tbl.NewRow
        OIT0002row.ItemArray = OIT0002INProw.ItemArray

        OIT0002row("LINECNT") = OIT0002tbl.Rows.Count + 1
        If OIT0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIT0002row("UPDTIMSTP") = "0"
        OIT0002row("SELECT") = 1
        OIT0002row("HIDDEN") = 0

        OIT0002tbl.Rows.Add(OIT0002row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIT0002INProw As DataRow)

        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '同一レコードか判定
            If OIT0002INProw("LINKNO") = OIT0002row("LINKNO") AndAlso
               OIT0002INProw("LINKDETAILNO") = OIT0002row("LINKDETAILNO") Then
                '画面入力テーブル項目設定
                OIT0002INProw("LINECNT") = OIT0002row("LINECNT")
                OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIT0002INProw("UPDTIMSTP") = OIT0002row("UPDTIMSTP")
                OIT0002INProw("SELECT") = 1
                OIT0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIT0002row.ItemArray = OIT0002INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControl()

        '〇 (一覧)テキストボックスの制御(読取専用)
        Dim divObj = DirectCast(pnlListArea.FindControl(pnlListArea.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)
        '### OT輸送フラグ用 START ##########################################################
        Dim chkObjOT As CheckBox = Nothing
        Dim chkObjIdWOOTcnt As String = "chk" & pnlListArea.ID & "OTTRANSPORTFLG"
        Dim chkObjOTId As String
        '### OT輸送フラグ用 END   ##########################################################
        Dim loopdr As DataRow = Nothing
        Dim rowIdx As Integer = 0
        Dim cvTruckSymbol As String = ""
        Dim cvTruckSymbolSub As String = ""
        Dim trkKbn As String = ""

        For Each rowitem As TableRow In tblObj.Rows

            loopdr = OIT0002tbl.Rows(rowIdx)
            cvTruckSymbol = StrConv(loopdr("MODEL"), Microsoft.VisualBasic.VbStrConv.Wide, &H411)
            Try
                cvTruckSymbolSub = cvTruckSymbol.Substring(0, 1)
            Catch ex As Exception
                cvTruckSymbolSub = ""
            End Try

            For Each cellObj As TableCell In rowitem.Controls
                '★コンテナの場合は入力制限する。
                '    ### 20201022 START コタキ(OTタンク車)のため除外しない対応 ########
                'If (cvTruckSymbolSub = "コ" OrElse cvTruckSymbolSub = "チ") Then
                If (cvTruckSymbolSub = "チ") Then
                    '### 20201022 END   コタキ(OTタンク車)のため除外しない対応 ########
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "TANKNUMBER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDERINGOILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LINE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "FILLINGPOINT") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGIRILINEORDER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGIRILINETRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGOUTLETORDER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGOUTLETTRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGTRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGLODDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGDEPDATE") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    End If

                    '★(一覧)の営業所が受注営業所コード(テキストボックス)と不一致の場合は入力制限する。
                ElseIf loopdr("OFFICECODE") <> work.WF_SEL_OFFICECODE.Text Then
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "TANKNUMBER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDERINGOILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LINE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "FILLINGPOINT") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGIRILINEORDER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGIRILINETRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGOUTLETORDER") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGOUTLETTRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGTRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGLODDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGDEPDATE") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                    End If

                Else
                    '(一覧)積込油種, (一覧)入線列車, (一覧)出線列車, 
                    '(一覧)積込後本線列車, (一覧)積込後本線列車積込予定日, (一覧)積込後本線列車発予定日
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDERINGOILNAME") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGIRILINETRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGOUTLETTRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGTRAINNO") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGLODDATE") _
                    OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "LOADINGDEPDATE") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")

                        '★袖ヶ浦営業所の場合は、位置(充填ポイント)を入力制限する。
                    ElseIf work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011203 _
                    AndAlso cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "FILLINGPOINT") Then
                        cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")

                    End If
                End If
            Next

            '★輸送形態区分
            trkKbn = loopdr("ORDERTRKBN")
            chkObjOTId = chkObjIdWOOTcnt & Convert.ToString(loopdr("LINECNT"))
            chkObjOT = Nothing
            For Each cellObj As TableCell In rowitem.Controls
                chkObjOT = DirectCast(cellObj.FindControl(chkObjOTId), CheckBox)
                'コントロールが見つかったら脱出
                If chkObjOT IsNot Nothing Then
                    Exit For
                End If
            Next

            'コントロールが見つかっていたら活性・非活性を実施
            If chkObjOT IsNot Nothing Then
                'M:請負OT混載の場合
                If trkKbn = BaseDllConst.CONST_TRKBN_M Then
                    'OT輸送(チェックボックス)を活性
                    chkObjOT.Enabled = True

                    'M:請負OT混載以外
                Else
                    'OT輸送(チェックボックス)を非活性
                    chkObjOT.Enabled = False
                End If
            End If

            rowIdx += 1
        Next

    End Sub

    ''' <summary>
    ''' 画面表示設定処理
    ''' </summary>
    Protected Sub WW_ScreenEnabledSet()

        '★新規受注作成の場合
        If work.WF_SEL_CREATEFLG.Text = "1" Then
            '画面表示(油種数)設定処理
            WW_ScreenOilEnabledSet()
        Else
            Me.TxtHTank.Enabled = False
            Me.TxtRTank.Enabled = False
            Me.TxtTTank.Enabled = False
            Me.TxtMTTank.Enabled = False
            Me.TxtKTank.Enabled = False
            Me.TxtK3Tank.Enabled = False
            Me.TxtK5Tank.Enabled = False
            Me.TxtK10Tank.Enabled = False
            Me.TxtLTank.Enabled = False
            Me.TxtATank.Enabled = False
        End If

    End Sub

    ''' <summary>
    ''' 画面表示(油種数)設定処理
    ''' </summary>
    Protected Sub WW_ScreenOilEnabledSet()

        '〇各営業者で管理している油種を取得
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        If work.WF_SEL_OFFICECODE.Text = "" Then
            FixvalueMasterSearch(Master.USER_ORG, "PRODUCTPATTERN", "", WW_GetValue, I_PARA01:="1")
        Else
            FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", "", WW_GetValue, I_PARA01:="1")
        End If

        '〇初期化
        'ハイオク
        Me.TxtHTank.Enabled = False
        'レギュラー
        Me.TxtRTank.Enabled = False
        '灯油
        Me.TxtTTank.Enabled = False
        '未添加灯油
        Me.TxtMTTank.Enabled = False
        '軽油
        Me.TxtKTank.Enabled = False
        '３号軽油
        Me.TxtK3Tank.Enabled = False
        '軽油５
        Me.TxtK5Tank.Enabled = False
        '軽油１０
        Me.TxtK10Tank.Enabled = False
        'ＬＳＡ
        Me.TxtLTank.Enabled = False
        'Ａ重油
        Me.TxtATank.Enabled = False

        For i As Integer = 0 To WW_GetValue.Length - 1
            Select Case WW_GetValue(i)
                    'ハイオク
                Case BaseDllConst.CONST_HTank
                    Me.TxtHTank.Enabled = True
                    'レギュラー
                Case BaseDllConst.CONST_RTank
                    Me.TxtRTank.Enabled = True
                    '灯油
                Case BaseDllConst.CONST_TTank
                    Me.TxtTTank.Enabled = True
                    ''### 20200615 START((全体)No73対応) ##########################################
                    ''★根岸営業所の場合
                    'If work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011402 Then
                    '    '★JXTG北信油槽所, 及びJXTG甲府油槽所の場合
                    '    If Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_10 _
                    '        OrElse Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_20 Then
                    '        '入力を未許可にする。
                    '        Me.TxtTTank.Enabled = False
                    '    Else
                    '        Me.TxtTTank.Enabled = True
                    '    End If
                    'Else
                    '    Me.TxtTTank.Enabled = True
                    'End If
                    ''### 20200615 END  ((全体)No73対応) ##########################################
                    '未添加灯油
                Case BaseDllConst.CONST_MTTank
                    Me.TxtMTTank.Enabled = True
                    ''★根岸営業所の場合
                    'If work.WF_SEL_OFFICECODE.Text = BaseDllConst.CONST_OFFICECODE_011402 Then
                    '    '### 20200615 START((全体)No73対応) ##########################################
                    '    '★JXTG北信油槽所, 及びJXTG甲府油槽所の場合
                    '    If Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_10 _
                    '        OrElse Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_20 Then
                    '        '入力を許可する。
                    '        Me.TxtMTTank.Enabled = True
                    '    Else
                    '        Me.TxtMTTank.Enabled = False
                    '    End If
                    '    '### 20200615 END  ((全体)No73対応) ##########################################
                    'Else
                    '    Me.TxtMTTank.Enabled = True
                    'End If
                    '軽油
                Case BaseDllConst.CONST_KTank1, BaseDllConst.CONST_KTank2
                    Me.TxtKTank.Enabled = True
                    '３号軽油
                Case BaseDllConst.CONST_K3Tank1, BaseDllConst.CONST_K3Tank2
                    Me.TxtK3Tank.Enabled = True
                    '軽油５
                Case BaseDllConst.CONST_K5Tank
                    Me.TxtK5Tank.Enabled = True
                    '軽油１０
                Case BaseDllConst.CONST_K10Tank
                    Me.TxtK10Tank.Enabled = True
                    'ＬＳＡ
                Case BaseDllConst.CONST_LTank1, BaseDllConst.CONST_LTank2
                    Me.TxtLTank.Enabled = True
                    ''### 20200706 START((全体)No100対応) ##########################################
                    ''★OT八王子の場合
                    'If Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_55 Then
                    '    Me.TxtLTank.Enabled = False
                    '    Me.TxtLTank.Text = 0
                    'Else
                    '    Me.TxtLTank.Enabled = True
                    'End If
                    ''### 20200706 END  ((全体)No100対応) ##########################################
                    'Ａ重油
                Case BaseDllConst.CONST_ATank
                    Me.TxtATank.Enabled = True
                    ''### 20200706 START((全体)No100対応) ##########################################
                    ''★OT八王子の場合
                    'If Me.TxtConsigneeCode.Text = BaseDllConst.CONST_CONSIGNEECODE_55 Then
                    '    Me.TxtATank.Enabled = False
                    '    Me.TxtATank.Text = 0
                    'Else
                    '    Me.TxtATank.Enabled = True
                    'End If
                    ''### 20200706 END  ((全体)No100対応) ##########################################
            End Select
        Next
    End Sub

    ''' <summary>
    ''' タンク車所在設定処理
    ''' </summary>
    Protected Sub WW_TankShozaiSet()

        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            If OIT0002row("TANKNUMBER") = "" Then
                Continue For
            End If

            '★(一覧)タンク車番号がOT本社、または在日米軍のリース車かチェック
            FixvalueMasterSearch("ZZ", "TANKNO_OTCHECK", OIT0002row("TANKNUMBER"), WW_GetValue)

            'タンク車がOT本社、または在日米軍のリース車の場合
            'タンク車所在の所在地を空車着駅(発駅)に更新する。
            If WW_GetValue(0) <> "" Then
                '★タンク車所在の更新
                '引数１：所在地コード　⇒　変更あり(空車着駅（発駅）)
                '引数２：タンク車状態　⇒　変更あり("3"(到着))
                '引数３：積車区分　　　⇒　変更なし(空白)
                '引数４：タンク車№　　⇒　指定あり
                'WW_UpdateTankShozai(Me.TxtRetstation.Text, "3", "", I_TANKNO:=OIT0002row("TANKNUMBER"))
            Else
                '★タンク車所在の更新
                '引数１：所在地コード　⇒　変更なし(空白)
                '引数２：タンク車状態　⇒　変更あり("3"(到着))
                '引数３：積車区分　　　⇒　変更なし(空白)
                '引数４：所属営業所コード　⇒　変更あり(登録営業所)
                '引数５：タンク車№　　　　⇒　指定あり
                WW_UpdateTankShozai("", "3", "", I_OFFICE:=work.WF_SEL_OFFICECODE.Text, I_TANKNO:=OIT0002row("TANKNUMBER"))

            End If

        Next
    End Sub

    ''' <summary>
    ''' (タンク車所在TBL)所在地の内容を更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateTankShozai(ByVal I_LOCATION As String,
                                      ByVal I_STATUS As String,
                                      ByVal I_KBN As String,
                                      Optional ByVal I_OFFICE As String = Nothing,
                                      Optional ByVal I_TANKNO As String = Nothing,
                                      Optional ByVal I_SITUATION As String = Nothing,
                                      Optional ByVal upEmparrDate As Boolean = False,
                                      Optional ByVal upActualEmparrDate As Boolean = False,
                                      Optional ByVal I_CONDITION As String = Nothing,
                                      Optional ByVal I_CONDITION_VAL As String = Nothing)

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
            If Not String.IsNullOrEmpty(I_OFFICE) Then
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
            ''空車着日（予定）
            'If upEmparrDate = True Then
            '    SQLStr &= String.Format("        EMPARRDATE   = '{0}', ", Me.TxtEmparrDate.Text)
            '    SQLStr &= String.Format("        ACTUALEMPARRDATE   = {0}, ", "NULL")
            'End If
            ''空車着日（実績）
            'If upActualEmparrDate = True Then
            '    SQLStr &= String.Format("        ACTUALEMPARRDATE   = '{0}', ", Me.TxtActualEmparrDate.Text)
            'End If

            SQLStr &=
                      "        UPDYMD       = @P11, " _
                    & "        UPDUSER      = @P12, " _
                    & "        UPDTERMID    = @P13, " _
                    & "        RECEIVEYMD   = @P14  " _
                    & "  WHERE TANKNUMBER   = @P01  " _
                    & "    AND DELFLG      <> @P02  "

            '◯条件付加
            If Not String.IsNullOrEmpty(I_CONDITION) AndAlso Not String.IsNullOrEmpty(I_CONDITION_VAL) Then
                Select Case I_CONDITION
                    Case "TANKSITUATION"
                        SQLStr &= "    AND TANKSITUATION = '" & I_CONDITION_VAL & "'"
                End Select
            End If

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
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    PARA01.Value = OIT0002row("TANKNUMBER")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D_TANKSHOZAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D_TANKSHOZAI UPDATE"
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
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String,
                               Optional I_OFFICECODE As String = Nothing)

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

                Case "ORG"             '運用部署
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

                Case "SALESOFFICE"      '登録営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "ORDERINFO"        '受注情報
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERINFO, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERINFO"))

                Case "USEPROPRIETY"     '利用可否フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "USEPROPRIETY"))

                Case "DEPSTATION"       '積込後発駅
                    'leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_OFFICECODE.Text + "2", "DEPSTATION"))
                    If IsNothing(I_OFFICECODE) Then
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_OFFICECODE.Text, "DEPSTATION"))
                    Else
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(I_OFFICECODE, "DEPSTATION"))
                    End If

                Case "RETSTATION"       '積込後着駅
                    'leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_OFFICECODE.Text + "1", "RETSTATION"))
                    If IsNothing(I_OFFICECODE) Then
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_OFFICECODE.Text, "RETSTATION"))
                    Else
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(I_OFFICECODE, "RETSTATION"))
                    End If

                Case "PRODUCTPATTERN"   '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN"))

                Case "TANKNO"           'タンク車
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TANKNUMBER, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TANKNO"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
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
    Private OIT0003WKtbl As DataTable                               '作業用テーブル
    Private OIT0003Fixvaltbl As DataTable                           '作業用テーブル(固定値マスタ取得用)
    Private OIT0003His1tbl As DataTable                             '履歴格納用テーブル
    Private OIT0003His2tbl As DataTable                             '履歴格納用テーブル
    Private OIT0003Reporttbl As DataTable                           '帳票用テーブル
    Private OIT0003ReportNegishitbl As DataTable                    '帳票用(根岸)テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

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
                        Case "WF_ButtonORDER_CANCEL"     'キャンセルボタン押下
                            WF_ButtonORDER_CANCEL_Click()
                        Case "WF_ButtonCSV",
                             "WF_ButtonSendaiLOADCSV",
                             "WF_ButtonNegishiSHIPCSV",
                             "WF_ButtonNegishiLOADCSV"  'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonINSERT"          '受注新規作成ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonLinkINSERT"      '貨車連結選択ボタン押下
                            WF_ButtonLinkINSERT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
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
                        Case "btnCommonConfirmOk"       '確認メッセージ
                            WW_UpdateOrderStatusCancel()
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

        'RadioButton1.Visible = False

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
            & " WHERE OIT0002.DELFLG     <> @P3" _
            & "   AND OIT0002.LODDATE    >= @P2"

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
        '発日('20200225(条件追加：(予定)発日))
        If Not String.IsNullOrEmpty(work.WF_SEL_SEARCH_DEPDATE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.DEPDATE >= '{0}'", work.WF_SEL_SEARCH_DEPDATE.Text)
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
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    i += 1
                    OIT0003row("LINECNT") = i        'LINECNT

                    '◯名称取得
                    '積置きフラグ
                    CODENAME_get("STACKING", OIT0003row("STACKINGFLG"), OIT0003row("STACKINGNAME"), WW_RTN_SW)

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
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        Select Case WF_ButtonClick.Value
            Case "WF_ButtonCSV"             'ダウンロードボタン押下

            'ダウンロードボタン(積込予定)押下
            Case "WF_ButtonSendaiLOADCSV"

                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelCommonDataGet(SQLcon, BaseDllConst.CONST_OFFICECODE_010402)
                End Using

                '******************************
                '帳票作成処理の実行
                '******************************
                Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_LOADPLAN.xlsx", OIT0003Reporttbl)
                    Dim url As String
                    Try
                        url = repCbj.CreateExcelPrintData()
                    Catch ex As Exception
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using

            'ダウンロードボタン(根岸(出荷予定))押下
            'ダウンロードボタン(根岸(積込予定))押下
            Case "WF_ButtonNegishiSHIPCSV",
                 "WF_ButtonNegishiLOADCSV"

                '******************************
                '帳票表示データ取得処理
                '******************************
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続

                    ExcelNegishiDataGet(SQLcon)
                End Using

                '******************************
                '帳票作成処理の実行
                '******************************
                Select Case WF_ButtonClick.Value
                    'ダウンロードボタン(根岸(出荷予定))押下
                    Case "WF_ButtonNegishiSHIPCSV"
                        Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_NEGISHI_SHIPPLAN.xlsx", OIT0003ReportNegishitbl)
                            Dim url As String
                            Try
                                url = repCbj.CreateExcelPrintNegishiData("SHIPPLAN")
                            Catch ex As Exception
                                Return
                            End Try
                            '○ 別画面でExcelを表示
                            WF_PrintURL.Value = url
                            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                        End Using

                    'ダウンロードボタン(根岸(積込予定))押下
                    Case "WF_ButtonNegishiLOADCSV"
                        Using repCbj = New OIT0003CustomReport(Master.MAPID, Master.MAPID & "_NEGISHI_LOADPLAN.xlsx", OIT0003ReportNegishitbl)
                            Dim url As String
                            Try
                                url = repCbj.CreateExcelPrintNegishiData("LOADPLAN")
                            Catch ex As Exception
                                Return
                            End Try
                            '○ 別画面でExcelを表示
                            WF_PrintURL.Value = url
                            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                        End Using

                End Select
        End Select
    End Sub

    ''' <summary>
    ''' 帳票表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelCommonDataGet(ByVal SQLcon As SqlConnection, ByVal OFFICECDE As String)
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
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIT0003.OILNAME                                AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
            & " , OIM0005.MODEL                                  AS MODEL" _
            & " , OIM0005.TANKNUMBER                             AS TANKNUMBER" _
            & " , OIM0005.JRINSPECTIONDATE                       AS JRINSPECTIONDATE" _
            & " , ''                                             AS RESERVEAMOUNT" _
            & " , CASE " _
            & "   WHEN OIT0002.STACKINGFLG = '1' THEN '積置' " _
            & "   ELSE '' " _
            & "   END                                            AS STACKING" _
            & " , OIT0002.TRAINNO                                AS TRAINNO" _
            & " , OIT0002.TRAINNAME                              AS TRAINNAME" _
            & " , OIT0002.TOTALTANKCH                            AS TOTALTANK" _
            & " , OIT0002.LODDATE                                AS LODDATE" _
            & " , OIT0002.DEPDATE                                AS DEPDATE" _
            & " , OIT0002.ARRDATE                                AS ARRDATE" _
            & " , OIT0002.ACCDATE                                AS ACCDATE" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & " AND OIM0005.DELFLG <> @P02 " _
            & " WHERE OIT0002.OFFICECODE = @P01 " _
            & "   AND OIT0002.DELFLG <> @P02 " _
            & "   AND OIT0002.LODDATE = @P03 "

        '& " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
        '& "     OIT0005.TANKNUMBER = OIT0003.TANKNO " _
        '& " AND OIT0005.DELFLG <> @P02 " _

        SQLStr &=
              " ORDER BY" _
            & "    OIT0002.BASECODE" _
            & "  , OIT0003.OILCODE"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                PARA01.Value = OFFICECDE
                PARA02.Value = C_DELETE_FLG.DELETE
                'PARA03.Value = "2020/5/29"
                PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")

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
    ''' 帳票表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelNegishiDataGet(ByVal SQLcon As SqlConnection)

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
            & " , '0'                                            AS DELFLG" _
            & " FROM oil.OIM0013_LOAD OIM0013 " _
            & " INNER JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "     OIM0003.OFFICECODE = @P01 " _
            & " AND OIM0003.PLANTCODE = OIM0013.PLANTCODE " _
            & " AND OIM0003.OILCODE = OIM0013.OILCODE "

        SQLLDP &=
                " ORDER BY" _
            & "    OIM0013.LOADINGPOINT " _
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
            & " , VIW0013.TRAINNAME                              AS TRAINNAME" _
            & " , VIW0013.TRAINNO                                AS TRAINNO" _
            & " , VIW0013.JRTRAINNO1                             AS JRTRAINNO1" _
            & " , VIW0013.TSUMI                                  AS TSUMI" _
            & " , ''                                             AS FILLINGPOINT" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIM0003.OILNAME                                AS OILNAME" _
            & " , OIM0003.OILKANA                                AS OILKANA" _
            & " , ISNULL(CASE " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_HTank + "' THEN OIT0002.HTANK " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_RTank + "' THEN OIT0002.RTANK " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_TTank + "' THEN OIT0002.TTANK " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_MTTank + "' THEN OIT0002.MTTANK " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_KTank1 + "' THEN OIT0002.KTANK " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_K3Tank1 + "' THEN OIT0002.K3TANK " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_ATank + "' THEN OIT0002.ATANK " _
            & "   WHEN OIT0003.OILCODE = '" + BaseDllConst.CONST_LTank1 + "' THEN OIT0002.LTANK " _
            & "   END, 0)                                        AS TOTALTANK" _
            & " FROM oil.VIW0013_OILFOR_NEGISHI_SHIP VIW0013 " _
            & " LEFT JOIN OIL.OIT0002_ORDER OIT0002 ON " _
            & "     OIT0002.LODDATE = @P03 " _
            & " AND OIT0002.TRAINNO = VIW0013.TRAINNO " _
            & " AND OIT0002.OFFICECODE = @P01 " _
            & " AND OIT0002.DELFLG <> @P02 " _
            & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " LEFT JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "     OIM0003.OFFICECODE = @P01 " _
            & " AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & " AND OIM0003.DELFLG <> @P02 "

        SQLStr &=
                " ORDER BY" _
            & "    VIW0013.No" _
            & "  , VIW0013.ZAIKOSORT" _
            & "  , TOTALTANK"

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
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLLDPcmd As New SqlCommand(SQLLDP, SQLcon)
                Dim PARALDP01 As SqlParameter = SQLLDPcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                PARALDP01.Value = BaseDllConst.CONST_OFFICECODE_011402

                Using SQLLDPdr As SqlDataReader = SQLLDPcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLLDPdr.FieldCount - 1
                        OIT0003WKtbl.Columns.Add(SQLLDPdr.GetName(index), SQLLDPdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003WKtbl.Load(SQLLDPdr)
                End Using

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                PARA01.Value = BaseDllConst.CONST_OFFICECODE_011402
                PARA02.Value = C_DELETE_FLG.DELETE
                'PARA03.Value = "2020/5/25"
                PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003ReportNegishitbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003ReportNegishitbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim strTrainNo As String = ""
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
                        If OIT0003Wkrow("DELFLG") <> "1" _
                            AndAlso OIT0003Wkrow("OILCODE") = OIT0003Reprow("OILCODE") Then
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
        work.WF_SEL_ORDERSTATUSNM.Text = "受注受付"
        '受注進行ステータス(コード)
        work.WF_SEL_ORDERSTATUS.Text = "100"
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
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

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

        '受注進行ステータス(コード)
        '〇受注進行ステータスが"100:受注受付"の場合
        If work.WF_SEL_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100 Then
            '受注貨車連結割当画面ページへ遷移
            Master.TransitionPage(work.WF_SEL_CAMPCODE.Text + "1")
        Else
            '受注明細画面ページへ遷移
            Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)
        End If

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

    End Sub
#End Region

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
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            '### START 受注履歴テーブルの追加(2020/03/26) #############
            WW_InsertOrderHistory(SQLcon)
            '### END   ################################################

            '### START 受注キャンセル時のタンク車所在の更新処理を追加(2020/03/31) ###############################
            For Each OIT0003His2tblrow In OIT0003His2tbl.Rows
                Select Case strOrderSts
                    Case BaseDllConst.CONST_ORDERSTATUS_100

                        '### 何もしない####################

                    '200:手配　～　310：手配完了
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
                         BaseDllConst.CONST_ORDERSTATUS_320
                        '★タンク車所在の更新(タンク車№を再度選択できるようにするため)
                        '引数１：所在地コード　⇒　変更なし(空白)
                        '引数２：タンク車状態　⇒　変更あり("3"(到着))
                        '引数３：積車区分　　　⇒　変更なし(空白)
                        '引数４：タンク車状況　⇒　変更あり("1"(残車))
                        WW_UpdateTankShozai("", "3", "", I_TANKNO:=OIT0003His2tblrow("TANKNO"), I_SITUATION:="1")

                    '350：受注確定
                    Case BaseDllConst.CONST_ORDERSTATUS_350
                        '★タンク車所在の更新(タンク車№を再度選択できるようにするため)
                        '引数１：所在地コード　⇒　変更あり(発駅)
                        '引数２：タンク車状態　⇒　変更あり("3"(到着))
                        '引数３：積車区分　　　⇒　変更なし(空白)
                        WW_UpdateTankShozai(strDepstation, "3", "", I_TANKNO:=OIT0003His2tblrow("TANKNO"))

                    '400：受入確認中, 450:受入確認中(受入日入力)
                    Case BaseDllConst.CONST_ORDERSTATUS_400,
                         BaseDllConst.CONST_ORDERSTATUS_450

                        '### 何もしない####################

                    '※"500：検収中"のステータス以降についてはキャンセルができない仕様だが
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
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

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
                SQLStr &= String.Format("        ACTUALEMPARRDATE   = '{0}', ", I_ActualEmparrDate)
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
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    PARA01.Value = OIT0003row("TANKNO")
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
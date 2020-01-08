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

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 7                  'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部タブID

    'Private Const CONST_DSPROWCOUNT As Integer = 45                '１画面表示対象
    'Private Const CONST_SCROLLROWCOUNT As Integer = 10              'マウススクロール時の増分
    'Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID
    Private Const CONST_MAX_TABID As Integer = 4                    '詳細タブ数

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
                        'Case "WF_ButtonINSERT"          '登録ボタン押下
                        '    WF_ButtonINSERT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        'Case "WF_Field_DBClick"         'フィールドダブルクリック
                        '    WF_FIELD_DBClick()
                        'Case "WF_CheckBoxSELECT"        'チェックボックス(選択)クリック
                        '    WF_CheckBoxSELECT_Click()
                        'Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                        '    WF_FIELD_Change()
                        'Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                        '    WF_ButtonSel_Click()
                        'Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                        '    WF_ButtonCan_Click()
                        'Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                        '    WF_ButtonSel_Click()
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        'Case "WF_ButtonLINE_LIFTED"     '行削除ボタン押下
                        '    WF_ButtonLINE_LIFTED_Click()
                        'Case "WF_ButtonLINE_ADD"        '行追加ボタン押下
                        '    WF_ButtonLINE_ADD_Click()
                        'Case "WF_ButtonCSV"             'ダウンロードボタン押下
                        '    WF_ButtonDownload_Click()
                        'Case "WF_ButtonUPDATE"          '明細更新ボタン押下
                        '    WF_ButtonUPDATE_Click()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        'Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                        '    WF_FILEUPLOAD()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                            'Case "WF_ListChange"            'リスト変更
                            '    WF_ListChange()
                        Case "WF_DTAB_Click" '○DetailTab切替処理
                            WF_Detail_TABChange()
                            TAB_DisplayCTRL()
                    End Select

                    ''○ 一覧再表示処理
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
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.MENU Then
            Master.MAPID = OIT0003WRKINC.MAPIDD
        Else
            Master.MAPID = OIT0003WRKINC.MAPIDD + "MAIN"
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

        ''○ GridView初期設定
        'GridViewInitialize()

        '○ 詳細-画面初期設定
        Repeater_INIT()
        WF_DTAB_CHANGE_NO.Value = "0"

        '〇 タブ切替
        WF_Detail_TABChange()

        '〇 タブ指定時表示判定処理
        TAB_DisplayCTRL()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        'ステータス
        TxtOrderStatus.Text = work.WF_SEL_ORDERSTATUSNM.Text
        '情報
        TxtOrderInfo.Text = work.WF_SEL_INFORMATIONNM.Text
        '###################################################
        '受注パターン
        TxtOrderType.Text = ""
        '###################################################
        'オーダー№
        TxtOrderNo.Text = work.WF_SEL_ORDERNUMBER.Text
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
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')   　            AS ORDERNO" _
            & " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "   ELSE ISNULL(RTRIM(OIT0002.TRAINNO), '')" _
            & "   END                                                AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')            AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')            AS SHIPPERSNAME" _
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

        '& " , ISNULL(RTRIM(OIS0015_2.VALUE1), '')                AS ORDERINFONAME" _
        '& " , ISNULL(RTRIM(OIT0002.TRAINNO), '')                 AS TRAINNO" _
        '& " , ISNULL(RTRIM(OIT0002.TOTALTANK), '')               AS TOTALTANK" _


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
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

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
        WF_Dtab01.Style.Remove("color")
        WF_Dtab01.Style.Add("color", "black")
        WF_Dtab01.Style.Remove("background-color")
        WF_Dtab01.Style.Add("background-color", "rgb(211, 211, 211)")
        WF_Dtab01.Style.Remove("border")
        WF_Dtab01.Style.Add("border", "1px solid black")
        WF_Dtab01.Style.Remove("font-weight")
        WF_Dtab01.Style.Add("font-weight", "normal")

        'タンク車明細
        WF_Dtab02.Style.Remove("color")
        WF_Dtab02.Style.Add("color", "black")
        WF_Dtab02.Style.Remove("background-color")
        WF_Dtab02.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab02.Style.Remove("border")
        WF_Dtab02.Style.Add("border", "1px solid black")
        WF_Dtab02.Style.Remove("font-weight")
        WF_Dtab02.Style.Add("font-weight", "normal")

        '入換・積込指示
        WF_Dtab03.Style.Remove("color")
        WF_Dtab03.Style.Add("color", "black")
        WF_Dtab03.Style.Remove("background-color")
        WF_Dtab03.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab03.Style.Remove("border")
        WF_Dtab03.Style.Add("border", "1px solid black")
        WF_Dtab03.Style.Remove("font-weight")
        WF_Dtab03.Style.Add("font-weight", "normal")

        '費用入力
        WF_Dtab04.Style.Remove("color")
        WF_Dtab04.Style.Add("color", "black")
        WF_Dtab04.Style.Remove("background-color")
        WF_Dtab04.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab04.Style.Remove("border")
        WF_Dtab04.Style.Add("border", "1px solid black")
        WF_Dtab04.Style.Remove("font-weight")
        WF_Dtab04.Style.Add("font-weight", "normal")

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                'タンク車割当
                WF_Dtab01.Style.Remove("color")
                WF_Dtab01.Style.Add("color", "black")
                WF_Dtab01.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(220, 230, 240)")
                WF_Dtab01.Style.Remove("border")
                WF_Dtab01.Style.Add("border", "1px solid blue")
                WF_Dtab01.Style.Remove("font-weight")
                WF_Dtab01.Style.Add("font-weight", "bold")
            Case 1
                'タンク車明細
                WF_Dtab02.Style.Remove("color")
                WF_Dtab02.Style.Add("color", "black")
                WF_Dtab02.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(220, 230, 240)")
                WF_Dtab02.Style.Remove("border")
                WF_Dtab02.Style.Add("border", "1px solid blue")
                WF_Dtab02.Style.Remove("font-weight")
                WF_Dtab02.Style.Add("font-weight", "bold")
            Case 2
                '入換・積込指示
                WF_Dtab03.Style.Remove("color")
                WF_Dtab03.Style.Add("color", "black")
                WF_Dtab03.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(220, 230, 240)")
                WF_Dtab03.Style.Remove("border")
                WF_Dtab03.Style.Add("border", "1px solid blue")
                WF_Dtab03.Style.Remove("font-weight")
                WF_Dtab03.Style.Add("font-weight", "bold")
            Case 3
                '費用入力
                WF_Dtab04.Style.Remove("color")
                WF_Dtab04.Style.Add("color", "black")
                WF_Dtab04.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(220, 230, 240)")
                WF_Dtab04.Style.Remove("border")
                WF_Dtab04.Style.Add("border", "1px solid blue")
                WF_Dtab04.Style.Remove("font-weight")
                WF_Dtab04.Style.Add("font-weight", "bold")
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

End Class
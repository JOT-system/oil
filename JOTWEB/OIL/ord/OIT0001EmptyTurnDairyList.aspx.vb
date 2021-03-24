'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox


''' <summary>
''' 空回日報一覧画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0001EmptyTurnDairyList
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0001tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0001INPtbl As DataTable                              'チェック用テーブル
    Private OIT0001UPDtbl As DataTable                              '更新用テーブル
    Private OIT0001WKtbl As DataTable                               '作業用テーブル
    Private OIT0001Fixvaltbl As DataTable                           '作業用テーブル
    Private OIT0001His1tbl As DataTable                             '履歴格納用テーブル
    Private OIT0001His2tbl As DataTable                             '履歴格納用テーブル
    Private OIT0001Detailtbl As DataTable                           '受注明細TBL取込用テーブル
    Private OIT0001OTOrdertbl As DataTable                          'OT空回日報(OT受注TBL)取込用テーブル
    Private OIT0001OTDetailtbl As DataTable                         'OT空回日報(OT受注明細TBL)取込用テーブル
    Private OIT0001CHKOrdertbl As DataTable                         '受注TBLチェック用テーブル
    Private OIT0001CMPOrdertbl As DataTable                         '受注TBL, OT受注TBL比較用テーブル
    Private OIT0001ReportOTComparetbl As DataTable                  '帳票(受注TBL, OT受注TBL)比較結果テーブル

    '帳票用
    Private Const CONST_RPT_OTCOMPARE As String = "OTCOMPARE"       '空回日報(受注TBL, OT受注TBL)比較

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
    'Private CS0013ProfView As New CS0013ProfView_TEST                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private RSSQL As New ReportSignSQL                              '帳票表示用SQL取得

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
                    Master.RecoverTable(OIT0001tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_CheckBoxSELECT"        'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonLINE_LIFTED"     '行削除ボタン押下
                            WF_ButtonLINE_LIFTED_Click()
                        Case "WF_ButtonOTCOMPARE"       'OT比較結果ボタン押下
                            WF_ButtonOTCOMPARE_Click()
                        Case "WF_ButtonOTINSERT"        '空回日報取込ボタン押下
                            WF_ButtonOTINSERT_Click()
                        Case "WF_ButtonINSERT"          '新規登録ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
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
        Master.MAPID = OIT0001WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0001S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0001D Then
            Master.RecoverTable(OIT0001tbl, work.WF_SEL_INPTBL.Text)
        End If

        '### 20210216 START 指摘票対応(No347)全体 #################################
        '○ OT空回日報が連携されているかチェック
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_GetOTOrderData(SQLcon)
        End Using
        '○ OT空回日報が連携されている場合
        If OIT0001OTOrdertbl.Rows.Count <> 0 Then
            '★空回日報取込ボタンを有効
            Me.WF_OTReceiveFLG.Value = "TRUE"
        Else
            '★空回日報取込ボタンを無効
            Me.WF_OTReceiveFLG.Value = "FALSE"
        End If
        '### 20210216 END   指摘票対応(No347)全体 #################################

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
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIT0001D Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

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
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        'CS0013ProfView.SCROLLTYPE = CS0013ProfView_TEST.SCROLLTYPE_ENUM.Both
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

        If IsNothing(OIT0001tbl) Then
            OIT0001tbl = New DataTable
        End If

        If OIT0001tbl.Columns.Count <> 0 Then
            OIT0001tbl.Columns.Clear()
        End If

        OIT0001tbl.Clear()

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
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')             AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')             AS ORDERSTATUSNAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERINFO), '')               AS ORDERINFO" _
            & " , ISNULL(RTRIM(OIT0016.CMPRESULTSCODE), '')          AS CMPRESULTSCODE" _
            & " , CASE " _
            & "   WHEN OIT0016.CMPRESULTSCODE = '1' THEN '不一致'" _
            & "   ELSE '' END                                        AS CMPRESULTSNAME" _
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
            & " , ISNULL(RTRIM(OIT0002.TOTALTANK), '')               AS TOTALTANK" _
            & " , ISNULL(RTRIM(OIT0002.TANKLINKNOMADE), '')          AS TANKLINKNOMADE" _
            & " , ISNULL(FORMAT(OIT0002.ORDERYMD, 'yyyy/MM/dd'), '') AS ORDERYMD" _
            & " , ISNULL(RTRIM(OIT0002.DELFLG), '')                  AS DELFLG" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " LEFT JOIN OIL.OIT0016_OTORDER OIT0016 ON " _
            & " OIT0016.ORDERNO = OIT0002.ORDERNO " _
            & " WHERE OIT0002.OFFICECODE   = @P1" _
            & "   AND OIT0002.LODDATE      >= @P2" _
            & "   AND OIT0002.DELFLG       <> @P3" _
            & "   AND OIT0002.ORDERSTATUS  <> @P4" _
            & "   AND OIT0002.EMPTYTURNFLG <> '2'"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNUMBER.Text) Then
            SQLStr &= String.Format("    AND OIT0002.TRAINNO = '{0}'", work.WF_SEL_TRAINNUMBER.Text)
        End If

        SQLStr &= String.Format("    AND OIT0002.ORDERSTATUS < '{0}'", BaseDllConst.CONST_ORDERSTATUS_500)

        SQLStr &=
              " ORDER BY" _
            & "    OIT0002.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 10) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)     '積込日(開始)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 3)  '受注進行ステータス

                PARA1.Value = work.WF_SEL_SALESOFFICECODE.Text
                PARA2.Value = work.WF_SEL_LOADINGDATE.Text
                PARA3.Value = C_DELETE_FLG.DELETE
                PARA4.Value = BaseDllConst.CONST_ORDERSTATUS_900

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
                    i += 1
                    OIT0001row("LINECNT") = i        'LINECNT

                    '受注進行ステータス
                    CODENAME_get("ORDERSTATUS", OIT0001row("ORDERSTATUS"), OIT0001row("ORDERSTATUSNAME"), WW_DUMMY)
                    '受注情報
                    CODENAME_get("ORDERINFO", OIT0001row("ORDERINFO"), OIT0001row("ORDERINFO"), WW_DUMMY)
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L Select"
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
            If OIT0001row("HIDDEN") = 0 Then
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
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        'CS0013ProfView.SCROLLTYPE = CS0013ProfView_TEST.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '### 20210216 START 指摘票対応(No347)全体 #################################
        '○ OT空回日報が連携されているかチェック
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_GetOTOrderData(SQLcon)
        End Using
        '○ OT空回日報が連携されている場合
        If OIT0001OTOrdertbl.Rows.Count <> 0 Then
            '★空回日報取込ボタンを有効
            Me.WF_OTReceiveFLG.Value = "TRUE"
        Else
            '★空回日報取込ボタンを無効
            Me.WF_OTReceiveFLG.Value = "FALSE"
        End If
        '### 20210216 END   指摘票対応(No347)全体 #################################

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
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        'チェックボックス判定
        For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
            If OIT0001tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If OIT0001tbl.Rows(i)("OPERATION") = "on" Then
                    OIT0001tbl.Rows(i)("OPERATION") = ""
                Else
                    OIT0001tbl.Rows(i)("OPERATION") = "on"
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
            If OIT0001tbl.Rows(i)("HIDDEN") = "0" Then
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
            If OIT0001tbl.Rows(i)("HIDDEN") = "0" Then
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
        Dim intTblCnt As Integer = 0
        Dim StatusChk As Boolean = False

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '■■■ OIT0001tbl関連の受注・受注明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注・受注明細を一括論理削除
            Dim SQLStr As String =
                      " UPDATE OIL.OIT0002_ORDER        " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE ORDERNO     = @P01       " _
                    & "    AND DELFLG     <> '1'       ;" _
                    & " UPDATE OIL.OIT0003_DETAIL       " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE ORDERNO     = @P01       " _
                    & "    AND DELFLG     <> '1'       ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            '受注削除する情報取得用
            Dim strOrderSts As String = ""          '受注進行ステータス
            Dim strDepstation As String = ""        '発駅コード
            Dim strArrstation As String = ""        '着駅コード
            Dim strLinkNoMade As String = ""        '作成_貨車連結順序表№

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            intTblCnt = OIT0001tbl.Rows.Count
            For Each OIT0001UPDrow In OIT0001tbl.Rows
                If OIT0001UPDrow("OPERATION") = "on" Then

                    If OIT0001UPDrow("LINECNT") < 9000 Then
                        SelectChk = True
                    End If
                    j += 1
                    OIT0001UPDrow("LINECNT") = j        'LINECNT
                    OIT0001UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0001UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0001UPDrow("ORDERNO")
                    work.WF_SEL_ORDERNUMBER.Text = OIT0001UPDrow("ORDERNO")
                    strOrderSts = OIT0001UPDrow("ORDERSTATUS")
                    strDepstation = OIT0001UPDrow("DEPSTATION")
                    strArrstation = OIT0001UPDrow("ARRSTATION")
                    strLinkNoMade = OIT0001UPDrow("TANKLINKNOMADE")

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

            '### START 受注履歴テーブルの追加(2020/06/08) #############
            WW_InsertOrderHistory(SQLcon)
            '### END   ################################################

            '### START 受注削除時のタンク車所在の更新処理を追加(2020/06/08) ###############################
            For Each OIT0001His2tblrow In OIT0001His2tbl.Rows
                Select Case strOrderSts
                    Case BaseDllConst.CONST_ORDERSTATUS_100

                        '### 何もしない####################

                    '200:手配　～　310：手配完了
                    Case BaseDllConst.CONST_ORDERSTATUS_200,
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
                        'WW_UpdateTankShozai("", "3", "", I_ORDERNO:=OIT0001His2tblrow("ORDERNO"),
                        '                    I_TANKNO:=OIT0001His2tblrow("TANKNO"), I_SITUATION:="1",
                        '                    I_ActualEmparrDate:=Now.ToString("yyyy/MM/dd"), upActualEmparrDate:=True)
                        WW_UpdateTankShozai("", "3", "", I_ORDERNO:=OIT0001His2tblrow("ORDERNO"),
                                            I_TANKNO:=OIT0001His2tblrow("TANKNO"), I_SITUATION:="1",
                                            I_ActualEmparrDate:="", upActualEmparrDate:=True)

                    '350：受注確定
                    Case BaseDllConst.CONST_ORDERSTATUS_350
                        StatusChk = True

                        '★タンク車所在の更新(タンク車№を再度選択できるようにするため)
                        '引数１：所在地コード　⇒　変更あり(発駅)
                        '引数２：タンク車状態　⇒　変更あり("3"(到着))
                        '引数３：積車区分　　　⇒　変更なし(空白)
                        'WW_UpdateTankShozai(strDepstation, "3", "", I_ORDERNO:=OIT0001His2tblrow("ORDERNO"),
                        '                    I_TANKNO:=OIT0001His2tblrow("TANKNO"),
                        '                    I_ActualEmparrDate:=Now.ToString("yyyy/MM/dd"), upActualEmparrDate:=True)
                        WW_UpdateTankShozai(strDepstation, "3", "", I_ORDERNO:=OIT0001His2tblrow("ORDERNO"),
                                            I_TANKNO:=OIT0001His2tblrow("TANKNO"),
                                            I_ActualEmparrDate:="", upActualEmparrDate:=True)

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○メッセージ表示
        '一覧件数が０件の時の行削除の場合
        If intTblCnt = 0 Then
            Master.Output(C_MESSAGE_NO.OIL_DELDATA_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            '一覧件数が１件以上で未選択による行削除の場合
        ElseIf SelectChk = False Then
            Master.Output(C_MESSAGE_NO.OIL_DELLINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

        ElseIf StatusChk = True Then
            Master.Output(C_MESSAGE_NO.OIL_TANKNO_INFO_MESSAGE, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

        Else
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

    End Sub

    ''' <summary>
    ''' OT比較結果ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOTCOMPARE_Click()

        '    ★一覧の表示が0件の場合
        If OIT0001tbl.Rows.Count = 0 Then
            Master.Output(C_MESSAGE_NO.OIL_OTCOMPAREDATA_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub

            '★一覧件数が１件以上で未選択の場合
        ElseIf OIT0001tbl.Select("OPERATION='on'").Count = 0 Then
            Master.Output(C_MESSAGE_NO.OIL_OTCOMPARELINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '******************************
        '帳票表示データ取得処理
        '******************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            ExcelOTCompareDataGet(SQLcon)
        End Using

        Using repCbj = New OIT0001CustomReport(Master.MAPID, Master.MAPID & "_OTCOMPARE.xlsx", OIT0001ReportOTComparetbl)
            Dim url As String
            Try
                url = repCbj.CreateExcelPrintData(work.WF_SEL_SALESOFFICECODE.Text, repPtn:=CONST_RPT_OTCOMPARE)
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        End Using

    End Sub

    ''' <summary>
    ''' 帳票表示(OT比較結果)データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelOTCompareDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0001ReportOTComparetbl) Then
            OIT0001ReportOTComparetbl = New DataTable
        End If

        If OIT0001ReportOTComparetbl.Columns.Count <> 0 Then
            OIT0001ReportOTComparetbl.Columns.Clear()
        End If

        OIT0001ReportOTComparetbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String = RSSQL.EmptyTurnDairyOTComparePrint("OIT0001")

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 11)       '受注№
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)          '削除フラグ
                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 6)  '受注営業所コード
                P_DELFLG.Value = C_DELETE_FLG.DELETE
                P_OFFICECODE.Value = work.WF_SEL_SALESOFFICECODE.Text

                '○一覧で選択された受注NoをKEYに取得
                For Each OIT0001row As DataRow In OIT0001tbl.Select("OPERATION='on'")
                    P_ORDERNO.Value = OIT0001row("ORDERNO")
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If OIT0001ReportOTComparetbl.Columns.Count = 0 Then
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0001ReportOTComparetbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        '○ テーブル検索結果をテーブル格納
                        OIT0001ReportOTComparetbl.Load(SQLdr)
                    End Using
                Next

                Dim i As Integer = 0
                Dim strTrainNosave As String = ""
                For Each OIT0001Reprow As DataRow In OIT0001ReportOTComparetbl.Rows
                    If strTrainNosave <> "" _
                        AndAlso strTrainNosave <> Convert.ToString(OIT0001Reprow("TRAINNO")) Then
                        i = 1
                        OIT0001Reprow("LINECNT") = i        'LINECNT
                    Else
                        i += 1
                        OIT0001Reprow("LINECNT") = i        'LINECNT
                    End If
                    strTrainNosave = Convert.ToString(OIT0001Reprow("TRAINNO"))
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L EXCEL_OTCOMPARE_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L EXCEL_OTCOMPARE_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub


    ''' <summary>
    ''' 空回日報取込ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOTINSERT_Click()
        '○ OT受注データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_GetOTOrderData(SQLcon)
        End Using

        '○ OT受注データ⇒受注データに追加
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '★OT受注TBL⇒受注TBLへ追加(すでに登録済みの場合は追加しない)
            WW_InsertOrder(SQLcon)
            '★OT受注明細TBL⇒受注明細TBLへ追加(すでに登録済みの場合は追加しない)
            WW_InsertOrderDetail(SQLcon)
            '★取込したOT受注TBLの削除フラグを"1"(無効)更新
            WW_UpdateOTOrderStatus(SQLcon, I_ITEM:="DELFLG", I_VALUE:=C_DELETE_FLG.DELETE)

        End Using

        '○ 受注TBLとOT受注TBLデータの比較
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '★受注TBLとOT受注TBL比較
            WW_CompareOrderToOTOrder(SQLcon)
        End Using

        '○ 画面表示データ取得(一覧再取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

    End Sub

    ''' <summary>
    ''' 新規登録ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        work.WF_SEL_LINECNT.Text = ""
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = ""
        '登録日
        work.WF_SEL_REGISTRATIONDATE.Text = ""
        '受注進行ステータス
        work.WF_SEL_STATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100
        '受注情報
        work.WF_SEL_INFORMATION.Text = ""
        '受注営業所名
        work.WF_SEL_ORDERSALESOFFICE.Text = ""
        '空回日報可否フラグ(0：未作成, 1:作成)
        work.WF_SEL_EMPTYTURNFLG.Text = "1"
        '積置可否フラグ(1：積置あり, 2:積置なし)
        work.WF_SEL_STACKINGFLG.Text = "2"

        '本線列車
        work.WF_SEL_TRAIN.Text = ""
        work.WF_SEL_TRAINNAME.Text = ""
        '発駅
        work.WF_SEL_DEPARTURESTATION.Text = ""
        '着駅
        work.WF_SEL_ARRIVALSTATION.Text = ""

        '積込日
        work.WF_SEL_LOADINGDATE.Text = ""
        '発日
        work.WF_SEL_LOADINGCAR_DEPARTUREDATE.Text = ""
        '着日
        work.WF_SEL_LOADINGCAR_ARRIVALDATE.Text = ""
        '受入日
        work.WF_SEL_RECEIPTDATE.Text = ""
        '空車着日
        work.WF_SEL_EMPARRDATE.Text = ""

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

        '削除フラグ
        work.WF_SEL_DELFLG.Text = "0"
        '作成フラグ(新規登録：1, 更新：2)
        work.WF_SEL_CREATEFLG.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = OIT0001tbl                       'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

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

        '選択行
        work.WF_SEL_LINECNT.Text = OIT0001tbl.Rows(WW_LINECNT)("LINECNT")
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = OIT0001tbl.Rows(WW_LINECNT)("ORDERNO")
        '登録日
        work.WF_SEL_REGISTRATIONDATE.Text = OIT0001tbl.Rows(WW_LINECNT)("ORDERYMD")
        '受注進行ステータス
        work.WF_SEL_STATUS.Text = OIT0001tbl.Rows(WW_LINECNT)("ORDERSTATUS")
        '受注情報
        work.WF_SEL_INFORMATION.Text = OIT0001tbl.Rows(WW_LINECNT)("ORDERINFO")
        '受注営業所名
        work.WF_SEL_ORDERSALESOFFICE.Text = OIT0001tbl.Rows(WW_LINECNT)("OFFICENAME")
        '空回日報可否フラグ(0：未作成, 1:作成)
        work.WF_SEL_EMPTYTURNFLG.Text = OIT0001tbl.Rows(WW_LINECNT)("EMPTYTURNFLG")
        '積置可否フラグ(1：積置あり, 2:積置なし)
        work.WF_SEL_STACKINGFLG.Text = OIT0001tbl.Rows(WW_LINECNT)("STACKINGFLG")

        '本線列車
        work.WF_SEL_TRAIN.Text = OIT0001tbl.Rows(WW_LINECNT)("TRAINNO")
        work.WF_SEL_TRAINNAME.Text = OIT0001tbl.Rows(WW_LINECNT)("TRAINNAME")
        '発駅
        work.WF_SEL_DEPARTURESTATION.Text = OIT0001tbl.Rows(WW_LINECNT)("DEPSTATION")
        '着駅
        work.WF_SEL_ARRIVALSTATION.Text = OIT0001tbl.Rows(WW_LINECNT)("ARRSTATION")

        '積込日
        work.WF_SEL_LOADINGDATE.Text = OIT0001tbl.Rows(WW_LINECNT)("LODDATE")
        '発日
        work.WF_SEL_LOADINGCAR_DEPARTUREDATE.Text = OIT0001tbl.Rows(WW_LINECNT)("DEPDATE")
        '着日
        work.WF_SEL_LOADINGCAR_ARRIVALDATE.Text = OIT0001tbl.Rows(WW_LINECNT)("ARRDATE")
        '受入日
        work.WF_SEL_RECEIPTDATE.Text = OIT0001tbl.Rows(WW_LINECNT)("ACCDATE")
        '空車着日
        work.WF_SEL_EMPARRDATE.Text = OIT0001tbl.Rows(WW_LINECNT)("EMPARRDATE")

        '車数（レギュラー）
        work.WF_SEL_REGULAR_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("RTANK")
        '車数（ハイオク）
        work.WF_SEL_HIGHOCTANE_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("HTANK")
        '車数（灯油）
        work.WF_SEL_KEROSENE_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("TTANK")
        '車数（未添加灯油）
        work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("MTTANK")
        '車数（軽油）
        work.WF_SEL_DIESEL_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("KTANK")
        '車数（３号軽油）
        work.WF_SEL_NUM3DIESEL_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("K3TANK")
        '車数（５号軽油）
        work.WF_SEL_NUM5DIESEL_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("K5TANK")
        '車数（１０号軽油）
        work.WF_SEL_NUM10DIESEL_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("K10TANK")
        '車数（LSA）
        work.WF_SEL_LSA_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("LTANK")
        '車数（A重油）
        work.WF_SEL_AHEAVY_TANKCAR.Text = OIT0001tbl.Rows(WW_LINECNT)("ATANK")
        '合計車数
        work.WF_SEL_TANKCARTOTAL.Text = OIT0001tbl.Rows(WW_LINECNT)("TOTALTANK")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = OIT0001tbl.Rows(WW_LINECNT)("DELFLG")
        '作成フラグ(新規登録：1, 更新：2)
        work.WF_SEL_CREATEFLG.Text = "2"

        '○ 状態をクリア
        For Each OIT0001row As DataRow In OIT0001tbl.Rows
            Select Case OIT0001row("OPERATION")
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

        '○ 選択明細の状態を設定
        Select Case OIT0001tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIT0001tbl, work.WF_SEL_INPTBL.Text)

        '登録画面ページへ遷移
        Master.TransitionPage()

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L_LINK UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L_LINK UPDATE"
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

            If I_TANKNO = "" Then
                '(一覧)で設定しているタンク車をKEYに更新
                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    PARA01.Value = OIT0001row("TANKNO")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L_TANKSHOZAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L_TANKSHOZAI UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' OT空回日報取得(OT受注TBL)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_GetOTOrderData(ByVal SQLcon As SqlConnection)
        '○OT空回日報取込用(OT受注TBL)
        If IsNothing(OIT0001OTOrdertbl) Then
            OIT0001OTOrdertbl = New DataTable
        End If

        If OIT0001OTOrdertbl.Columns.Count <> 0 Then
            OIT0001OTOrdertbl.Columns.Clear()
        End If

        OIT0001OTOrdertbl.Clear()

        '○OT空回日報取込用(OT受注明細TBL)
        If IsNothing(OIT0001OTDetailtbl) Then
            OIT0001OTDetailtbl = New DataTable
        End If

        If OIT0001OTDetailtbl.Columns.Count <> 0 Then
            OIT0001OTDetailtbl.Columns.Clear()
        End If

        OIT0001OTDetailtbl.Clear()

        '○受注明細TBL取込用
        If IsNothing(OIT0001Detailtbl) Then
            OIT0001Detailtbl = New DataTable
        End If

        If OIT0001Detailtbl.Columns.Count <> 0 Then
            OIT0001Detailtbl.Columns.Clear()
        End If

        OIT0001Detailtbl.Clear()

        '○受注TBLチェック用
        If IsNothing(OIT0001CHKOrdertbl) Then
            OIT0001CHKOrdertbl = New DataTable
        End If

        If OIT0001CHKOrdertbl.Columns.Count <> 0 Then
            OIT0001CHKOrdertbl.Columns.Clear()
        End If

        OIT0001CHKOrdertbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをOT受注データを取得する
        Dim SQLOTOrderStr As String =
              " SELECT" _
            & "   '0' AS ORDERFLAG" _
            & " , OIT0016.CMPRESULTSCODE" _
            & " , OIT0016.CMPRESULTSNAME" _
            & " , OIT0016.ORDERNO" _
            & " , OIT0016.TRAINNO" _
            & " , OIT0016.TRAINNAME" _
            & " , OIT0016.ORDERYMD" _
            & " , OIT0016.OFFICECODE" _
            & " , OIT0016.OFFICENAME" _
            & " , OIT0016.ORDERTYPE" _
            & " , OIT0016.SHIPPERSCODE" _
            & " , OIT0016.SHIPPERSNAME" _
            & " , OIT0016.BASECODE" _
            & " , OIT0016.BASENAME" _
            & " , OIT0016.CONSIGNEECODE" _
            & " , OIT0016.CONSIGNEENAME" _
            & " , OIT0016.DEPSTATION" _
            & " , OIT0016.DEPSTATIONNAME" _
            & " , OIT0016.ARRSTATION" _
            & " , OIT0016.ARRSTATIONNAME" _
            & " , OIT0016.RETSTATION" _
            & " , OIT0016.RETSTATIONNAME" _
            & " , OIT0016.CHANGERETSTATION" _
            & " , OIT0016.CHANGERETSTATIONNAME" _
            & " , OIT0016.ORDERSTATUS" _
            & " , OIT0016.ORDERINFO" _
            & " , OIT0016.EMPTYTURNFLG" _
            & " , OIT0016.STACKINGFLG" _
            & " , OIT0016.USEPROPRIETYFLG" _
            & " , OIT0016.CONTACTFLG" _
            & " , OIT0016.RESULTFLG" _
            & " , OIT0016.DELIVERYFLG" _
            & " , OIT0016.DELIVERYCOUNT" _
            & " , OIT0016.LODDATE" _
            & " , OIT0016.DEPDATE" _
            & " , OIT0016.ARRDATE" _
            & " , OIT0016.ACCDATE" _
            & " , OIT0016.EMPARRDATE" _
            & " , OIT0016.ACTUALLODDATE" _
            & " , OIT0016.ACTUALDEPDATE" _
            & " , OIT0016.ACTUALARRDATE" _
            & " , OIT0016.ACTUALACCDATE" _
            & " , OIT0016.ACTUALEMPARRDATE" _
            & " , OIT0016.RTANK" _
            & " , OIT0016.HTANK" _
            & " , OIT0016.TTANK" _
            & " , OIT0016.MTTANK" _
            & " , OIT0016.KTANK" _
            & " , OIT0016.K3TANK" _
            & " , OIT0016.K5TANK" _
            & " , OIT0016.K10TANK" _
            & " , OIT0016.LTANK" _
            & " , OIT0016.ATANK" _
            & " , OIT0016.OTHER1OTANK" _
            & " , OIT0016.OTHER2OTANK" _
            & " , OIT0016.OTHER3OTANK" _
            & " , OIT0016.OTHER4OTANK" _
            & " , OIT0016.OTHER5OTANK" _
            & " , OIT0016.OTHER6OTANK" _
            & " , OIT0016.OTHER7OTANK" _
            & " , OIT0016.OTHER8OTANK" _
            & " , OIT0016.OTHER9OTANK" _
            & " , OIT0016.OTHER10OTANK" _
            & " , OIT0016.TOTALTANK" _
            & " , OIT0016.RTANKCH" _
            & " , OIT0016.HTANKCH" _
            & " , OIT0016.TTANKCH" _
            & " , OIT0016.MTTANKCH" _
            & " , OIT0016.KTANKCH" _
            & " , OIT0016.K3TANKCH" _
            & " , OIT0016.K5TANKCH" _
            & " , OIT0016.K10TANKCH" _
            & " , OIT0016.LTANKCH" _
            & " , OIT0016.ATANKCH" _
            & " , OIT0016.OTHER1OTANKCH" _
            & " , OIT0016.OTHER2OTANKCH" _
            & " , OIT0016.OTHER3OTANKCH" _
            & " , OIT0016.OTHER4OTANKCH" _
            & " , OIT0016.OTHER5OTANKCH" _
            & " , OIT0016.OTHER6OTANKCH" _
            & " , OIT0016.OTHER7OTANKCH" _
            & " , OIT0016.OTHER8OTANKCH" _
            & " , OIT0016.OTHER9OTANKCH" _
            & " , OIT0016.OTHER10OTANKCH" _
            & " , OIT0016.TOTALTANKCH" _
            & " , OIT0016.TANKLINKNO" _
            & " , OIT0016.TANKLINKNOMADE" _
            & " , OIT0016.BILLINGNO" _
            & " , OIT0016.KEIJYOYMD" _
            & " , OIT0016.SALSE" _
            & " , OIT0016.SALSETAX" _
            & " , OIT0016.TOTALSALSE" _
            & " , OIT0016.PAYMENT" _
            & " , OIT0016.PAYMENTTAX" _
            & " , OIT0016.TOTALPAYMENT" _
            & " , OIT0016.OTFILENAME" _
            & " , OIT0016.RECEIVECOUNT" _
            & " , OIT0016.OTSENDSTATUS" _
            & " , OIT0016.RESERVEDSTATUS" _
            & " , OIT0016.TAKUSOUSTATUS" _
            & " , OIT0016.BTRAINNO" _
            & " , OIT0016.BTRAINNAME" _
            & " , OIT0016.ANASYORIFLG" _
            & " , OIT0016.DELFLG" _
            & " , OIT0016.INITYMD" _
            & " , OIT0016.INITUSER" _
            & " , OIT0016.INITTERMID" _
            & " , OIT0016.UPDYMD" _
            & " , OIT0016.UPDUSER" _
            & " , OIT0016.UPDTERMID" _
            & " , OIT0016.RECEIVEYMD" _
            & " FROM OIL.OIT0016_OTORDER OIT0016" _
            & " WHERE " _
            & "     OIT0016.OFFICECODE = @OFFICECODE" _
            & " AND OIT0016.ORDERYMD   = @ORDERYMD" _
            & " AND OIT0016.DELFLG    <> @DELFLG"

        '　検索説明
        '     条件指定に従い該当データをOT受注明細データを取得する
        Dim SQLOTDetailStr As String =
              " SELECT" _
            & "   '0' AS ORDERFLAG" _
            & " , OIT0017.ORDERNO" _
            & " , OIT0017.DETAILNO" _
            & " , OIT0017.SHIPORDER" _
            & " , OIT0017.LINEORDER" _
            & " , OIT0017.TANKNO" _
            & " , OIT0017.KAMOKU" _
            & " , OIT0017.STACKINGORDERNO" _
            & " , OIT0017.STACKINGFLG" _
            & " , OIT0017.WHOLESALEFLG" _
            & " , OIT0017.INSPECTIONFLG" _
            & " , OIT0017.DETENTIONFLG" _
            & " , OIT0017.FIRSTRETURNFLG" _
            & " , OIT0017.AFTERRETURNFLG" _
            & " , OIT0017.OTTRANSPORTFLG" _
            & " , OIT0017.UPGRADEFLG" _
            & " , OIT0017.ORDERINFO" _
            & " , OIT0017.SHIPPERSCODE" _
            & " , OIT0017.SHIPPERSNAME" _
            & " , OIT0017.OILCODE" _
            & " , OIT0017.OILNAME" _
            & " , OIT0017.ORDERINGTYPE" _
            & " , OIT0017.ORDERINGOILNAME" _
            & " , OIT0017.CARSNUMBER" _
            & " , OIT0017.CARSAMOUNT" _
            & " , OIT0017.RETURNDATETRAIN" _
            & " , OIT0017.JOINTCODE" _
            & " , OIT0017.JOINT" _
            & " , OIT0017.REMARK" _
            & " , OIT0017.CHANGETRAINNO" _
            & " , OIT0017.CHANGETRAINNAME" _
            & " , OIT0017.SECONDCONSIGNEECODE" _
            & " , OIT0017.SECONDCONSIGNEENAME" _
            & " , OIT0017.SECONDARRSTATION" _
            & " , OIT0017.SECONDARRSTATIONNAME" _
            & " , OIT0017.CHANGERETSTATION" _
            & " , OIT0017.CHANGERETSTATIONNAME" _
            & " , OIT0017.LINE" _
            & " , OIT0017.FILLINGPOINT" _
            & " , OIT0017.LOADINGIRILINETRAINNO" _
            & " , OIT0017.LOADINGIRILINETRAINNAME" _
            & " , OIT0017.LOADINGIRILINEORDER" _
            & " , OIT0017.LOADINGOUTLETTRAINNO" _
            & " , OIT0017.LOADINGOUTLETTRAINNAME" _
            & " , OIT0017.LOADINGOUTLETORDER" _
            & " , OIT0017.ACTUALLODDATE" _
            & " , OIT0017.ACTUALDEPDATE" _
            & " , OIT0017.ACTUALARRDATE" _
            & " , OIT0017.ACTUALACCDATE" _
            & " , OIT0017.ACTUALEMPARRDATE" _
            & " , OIT0017.RESERVEDNO" _
            & " , OIT0017.GYONO" _
            & " , OIT0017.OTSENDCOUNT" _
            & " , OIT0017.DLRESERVEDCOUNT" _
            & " , OIT0017.DLTAKUSOUCOUNT" _
            & " , OIT0017.SALSE" _
            & " , OIT0017.SALSETAX" _
            & " , OIT0017.TOTALSALSE" _
            & " , OIT0017.PAYMENT" _
            & " , OIT0017.PAYMENTTAX" _
            & " , OIT0017.TOTALPAYMENT" _
            & " , OIT0017.ANASYORIFLG" _
            & " , OIT0017.VOLSYORIFLG" _
            & " , OIT0017.DELFLG" _
            & " , OIT0017.INITYMD" _
            & " , OIT0017.INITUSER" _
            & " , OIT0017.INITTERMID" _
            & " , OIT0017.UPDYMD" _
            & " , OIT0017.UPDUSER" _
            & " , OIT0017.UPDTERMID" _
            & " , OIT0017.RECEIVEYMD" _
            & " FROM OIL.OIT0017_OTDETAIL OIT0017" _
            & " INNER JOIN OIL.OIT0016_OTORDER OIT0016 ON" _
            & "     OIT0016.OFFICECODE = @OFFICECODE" _
            & " AND OIT0016.ORDERYMD   = @ORDERYMD" _
            & " AND OIT0016.ORDERNO    = OIT0017.ORDERNO" _
            & " AND OIT0016.DELFLG    <> @DELFLG" _
            & " WHERE " _
            & "     OIT0017.DELFLG    <> @DELFLG"

        '　検索説明
        '     条件指定に従い該当データを受注明細データを取得する
        Dim SQLDetailStr As String =
              " SELECT" _
            & "   OIT0003.ORDERNO" _
            & " , OIT0003.DETAILNO" _
            & " , OIT0003.SHIPORDER" _
            & " , OIT0003.LINEORDER" _
            & " , OIT0003.TANKNO" _
            & " , OIT0003.KAMOKU" _
            & " , OIT0003.STACKINGORDERNO" _
            & " , OIT0003.STACKINGFLG" _
            & " , OIT0003.WHOLESALEFLG" _
            & " , OIT0003.INSPECTIONFLG" _
            & " , OIT0003.DETENTIONFLG" _
            & " , OIT0003.FIRSTRETURNFLG" _
            & " , OIT0003.AFTERRETURNFLG" _
            & " , OIT0003.OTTRANSPORTFLG" _
            & " , OIT0003.UPGRADEFLG" _
            & " , OIT0003.ORDERINFO" _
            & " , OIT0003.SHIPPERSCODE" _
            & " , OIT0003.SHIPPERSNAME" _
            & " , OIT0003.OILCODE" _
            & " , OIT0003.OILNAME" _
            & " , OIT0003.ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME" _
            & " , OIT0003.CARSNUMBER" _
            & " , OIT0003.CARSAMOUNT" _
            & " , OIT0003.RETURNDATETRAIN" _
            & " , OIT0003.JOINTCODE" _
            & " , OIT0003.JOINT" _
            & " , OIT0003.REMARK" _
            & " , OIT0003.CHANGETRAINNO" _
            & " , OIT0003.CHANGETRAINNAME" _
            & " , OIT0003.SECONDCONSIGNEECODE" _
            & " , OIT0003.SECONDCONSIGNEENAME" _
            & " , OIT0003.SECONDARRSTATION" _
            & " , OIT0003.SECONDARRSTATIONNAME" _
            & " , OIT0003.CHANGERETSTATION" _
            & " , OIT0003.CHANGERETSTATIONNAME" _
            & " , OIT0003.LINE" _
            & " , OIT0003.FILLINGPOINT" _
            & " , OIT0003.LOADINGIRILINETRAINNO" _
            & " , OIT0003.LOADINGIRILINETRAINNAME" _
            & " , OIT0003.LOADINGIRILINEORDER" _
            & " , OIT0003.LOADINGOUTLETTRAINNO" _
            & " , OIT0003.LOADINGOUTLETTRAINNAME" _
            & " , OIT0003.LOADINGOUTLETORDER" _
            & " , OIT0003.ACTUALLODDATE" _
            & " , OIT0003.ACTUALDEPDATE" _
            & " , OIT0003.ACTUALARRDATE" _
            & " , OIT0003.ACTUALACCDATE" _
            & " , OIT0003.ACTUALEMPARRDATE" _
            & " , OIT0003.RESERVEDNO" _
            & " , OIT0003.GYONO" _
            & " , OIT0003.OTSENDCOUNT" _
            & " , OIT0003.DLRESERVEDCOUNT" _
            & " , OIT0003.DLTAKUSOUCOUNT" _
            & " , OIT0003.SALSE" _
            & " , OIT0003.SALSETAX" _
            & " , OIT0003.TOTALSALSE" _
            & " , OIT0003.PAYMENT" _
            & " , OIT0003.PAYMENTTAX" _
            & " , OIT0003.TOTALPAYMENT" _
            & " , OIT0003.ANASYORIFLG" _
            & " , OIT0003.VOLSYORIFLG" _
            & " , OIT0003.DELFLG" _
            & " , OIT0003.INITYMD" _
            & " , OIT0003.INITUSER" _
            & " , OIT0003.INITTERMID" _
            & " , OIT0003.UPDYMD" _
            & " , OIT0003.UPDUSER" _
            & " , OIT0003.UPDTERMID" _
            & " , OIT0003.RECEIVEYMD" _
            & " FROM OIL.OIT0003_DETAIL OIT0003" _
            & " WHERE " _
            & "     OIT0003.ORDERNO    = @ORDERNO" _
            & " AND OIT0003.DELFLG    <> @DELFLG"

        '★受注TBL存在チェック用
        Dim SQLChkOrderStr As String =
              " SELECT" _
            & "   OIT0002.ORDERNO" _
            & " FROM OIL.OIT0002_ORDER OIT0002" _
            & " WHERE OIT0002.ORDERNO = @ORDERNO"

        Try
            Using SQLOTOrdercmd As New SqlCommand(SQLOTOrderStr, SQLcon),
                  SQLOTDetailcmd As New SqlCommand(SQLOTDetailStr, SQLcon),
                  SQLDetailcmd As New SqlCommand(SQLDetailStr, SQLcon),
                  SQLChkOrdercmd As New SqlCommand(SQLChkOrderStr, SQLcon)
                '★OT受注TBLからデータを取得
                With SQLOTOrdercmd.Parameters
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = work.WF_SEL_SALESOFFICECODE.Text    '営業所コード
                    .Add("@ORDERYMD", SqlDbType.Date).Value = Now.ToString("yyyy/MM/dd")                '登録日
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.DELETE                     '削除フラグ
                End With

                Using SQLdr As SqlDataReader = SQLOTOrdercmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001OTOrdertbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001OTOrdertbl.Load(SQLdr)
                End Using

                Dim P_ORDERNO As SqlParameter = SQLChkOrdercmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar) '受注№
                '受信した空回日報が受注TBLに登録済みかチェック
                For Each OIT0001OTrow As DataRow In OIT0001OTOrdertbl.Rows
                    P_ORDERNO.Value = OIT0001OTrow("ORDERNO")

                    Using SQLdr As SqlDataReader = SQLChkOrdercmd.ExecuteReader()
                        If OIT0001CHKOrdertbl.Columns.Count = 0 Then
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0001CHKOrdertbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If
                        '○ テーブル検索結果クリア
                        OIT0001CHKOrdertbl.Clear()
                        '○ テーブル検索結果をテーブル格納
                        OIT0001CHKOrdertbl.Load(SQLdr)
                    End Using

                    '★受注TBLに存在した場合
                    If OIT0001CHKOrdertbl.Rows.Count <> 0 Then
                        '受注TBL"1"(存在)に設定
                        OIT0001OTrow("ORDERFLAG") = "1"
                        'OT受注TBLの取込フラグを"1"(取込済み)に更新
                        WW_UpdateOTOrderStatus(SQLcon, OIT0001row:=OIT0001OTrow, I_ITEM:="IMPORTFLG", I_VALUE:="1")

                    End If

                Next

                '★OT受注明細TBLからデータを取得
                With SQLOTDetailcmd.Parameters
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = work.WF_SEL_SALESOFFICECODE.Text    '営業所コード
                    .Add("@ORDERYMD", SqlDbType.Date).Value = Now.ToString("yyyy/MM/dd")                '登録日
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.DELETE                     '削除フラグ
                End With

                Using SQLdr As SqlDataReader = SQLOTDetailcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001OTDetailtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001OTDetailtbl.Load(SQLdr)
                End Using

                '★受注TBLに存在した受注NoをOT受注明細の受注にも設定する。
                For Each OIT0001OTOrderrow As DataRow In OIT0001OTOrdertbl.Select("ORDERFLAG='1'")
                    For Each OIT0001OTDetailrow As DataRow In OIT0001OTDetailtbl.Rows
                        If OIT0001OTOrderrow("ORDERNO") = OIT0001OTOrderrow("ORDERNO") Then
                            OIT0001OTDetailrow("ORDERFLAG") = "1"
                        End If
                    Next
                Next

                '★受注明細TBLからデータを取得
                Dim P_DELFLG As SqlParameter = SQLDetailcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar) '削除フラグ
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                P_ORDERNO = SQLDetailcmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar) '受注№
                For Each OIT0001OTrow As DataRow In OIT0001OTOrdertbl.Rows
                    P_ORDERNO.Value = OIT0001OTrow("ORDERNO")
                    Using SQLdr As SqlDataReader = SQLDetailcmd.ExecuteReader()
                        If OIT0001Detailtbl.Columns.Count = 0 Then
                            '○ フィールド名とフィールドの型を取得
                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0001Detailtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If
                        '○ テーブル検索結果をテーブル格納
                        OIT0001Detailtbl.Load(SQLdr)
                    End Using
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L GetOTOrderData")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L GetOTOrderData"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' OT受注TBL⇒受注TBLへ追加
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_InsertOrder(ByVal SQLcon As SqlConnection)
        Try
            '追加SQL文･･･受注TBL追加
            Dim SQLStr As String =
                  " INSERT INTO OIL.OIT0002_ORDER " _
                & " ( ORDERNO         , TRAINNO             , TRAINNAME      , ORDERYMD      , OFFICECODE      , OFFICENAME" _
                & " , ORDERTYPE       , SHIPPERSCODE        , SHIPPERSNAME   , BASECODE      , BASENAME" _
                & " , CONSIGNEECODE   , CONSIGNEENAME       , DEPSTATION     , DEPSTATIONNAME" _
                & " , ARRSTATION      , ARRSTATIONNAME      , RETSTATION     , RETSTATIONNAME" _
                & " , CHANGERETSTATION, CHANGERETSTATIONNAME, ORDERSTATUS    , ORDERINFO" _
                & " , EMPTYTURNFLG    , STACKINGFLG         , USEPROPRIETYFLG, CONTACTFLG    , RESULTFLG" _
                & " , DELIVERYFLG     , DELIVERYCOUNT" _
                & " , LODDATE         , DEPDATE             , ARRDATE        , ACCDATE       , EMPARRDATE" _
                & " , ACTUALLODDATE   , ACTUALDEPDATE       , ACTUALARRDATE  , ACTUALACCDATE , ACTUALEMPARRDATE" _
                & " , RTANK           , HTANK               , TTANK          , MTTANK        , KTANK" _
                & " , K3TANK          , K5TANK              , K10TANK        , LTANK         , ATANK" _
                & " , OTHER1OTANK     , OTHER2OTANK         , OTHER3OTANK    , OTHER4OTANK   , OTHER5OTANK" _
                & " , OTHER6OTANK     , OTHER7OTANK         , OTHER8OTANK    , OTHER9OTANK   , OTHER10OTANK    , TOTALTANK" _
                & " , RTANKCH         , HTANKCH             , TTANKCH        , MTTANKCH      , KTANKCH         , K3TANKCH" _
                & " , K5TANKCH        , K10TANKCH           , LTANKCH        , ATANKCH" _
                & " , OTHER1OTANKCH   , OTHER2OTANKCH       , OTHER3OTANKCH  , OTHER4OTANKCH , OTHER5OTANKCH" _
                & " , OTHER6OTANKCH   , OTHER7OTANKCH       , OTHER8OTANKCH  , OTHER9OTANKCH , OTHER10OTANKCH  , TOTALTANKCH" _
                & " , TANKLINKNO      , TANKLINKNOMADE      , BILLINGNO      , KEIJYOYMD" _
                & " , SALSE           , SALSETAX            , TOTALSALSE" _
                & " , PAYMENT         , PAYMENTTAX          , TOTALPAYMENT" _
                & " , OTFILENAME      , RECEIVECOUNT        , OTSENDSTATUS   , RESERVEDSTATUS" _
                & " , TAKUSOUSTATUS   , BTRAINNO            , BTRAINNAME     , ANASYORIFLG   , DELFLG" _
                & " , INITYMD         , INITUSER            , INITTERMID" _
                & " , UPDYMD          , UPDUSER             , UPDTERMID      , RECEIVEYMD)"

            SQLStr &=
                  " VALUES" _
                & " ( @ORDERNO         , @TRAINNO             , @TRAINNAME      , @ORDERYMD      , @OFFICECODE      , @OFFICENAME" _
                & " , @ORDERTYPE       , @SHIPPERSCODE        , @SHIPPERSNAME   , @BASECODE      , @BASENAME" _
                & " , @CONSIGNEECODE   , @CONSIGNEENAME       , @DEPSTATION     , @DEPSTATIONNAME" _
                & " , @ARRSTATION      , @ARRSTATIONNAME      , @RETSTATION     , @RETSTATIONNAME" _
                & " , @CHANGERETSTATION, @CHANGERETSTATIONNAME, @ORDERSTATUS    , @ORDERINFO" _
                & " , @EMPTYTURNFLG    , @STACKINGFLG         , @USEPROPRIETYFLG, @CONTACTFLG    , @RESULTFLG" _
                & " , @DELIVERYFLG     , @DELIVERYCOUNT" _
                & " , @LODDATE         , @DEPDATE             , @ARRDATE        , @ACCDATE       , @EMPARRDATE" _
                & " , @ACTUALLODDATE   , @ACTUALDEPDATE       , @ACTUALARRDATE  , @ACTUALACCDATE , @ACTUALEMPARRDATE" _
                & " , @RTANK           , @HTANK               , @TTANK          , @MTTANK        , @KTANK" _
                & " , @K3TANK          , @K5TANK              , @K10TANK        , @LTANK         , @ATANK" _
                & " , @OTHER1OTANK     , @OTHER2OTANK         , @OTHER3OTANK    , @OTHER4OTANK   , @OTHER5OTANK" _
                & " , @OTHER6OTANK     , @OTHER7OTANK         , @OTHER8OTANK    , @OTHER9OTANK   , @OTHER10OTANK    , @TOTALTANK" _
                & " , @RTANKCH         , @HTANKCH             , @TTANKCH        , @MTTANKCH      , @KTANKCH         , @K3TANKCH" _
                & " , @K5TANKCH        , @K10TANKCH           , @LTANKCH        , @ATANKCH" _
                & " , @OTHER1OTANKCH   , @OTHER2OTANKCH       , @OTHER3OTANKCH  , @OTHER4OTANKCH , @OTHER5OTANKCH" _
                & " , @OTHER6OTANKCH   , @OTHER7OTANKCH       , @OTHER8OTANKCH  , @OTHER9OTANKCH , @OTHER10OTANKCH  , @TOTALTANKCH" _
                & " , @TANKLINKNO      , @TANKLINKNOMADE      , @BILLINGNO      , @KEIJYOYMD" _
                & " , @SALSE           , @SALSETAX            , @TOTALSALSE" _
                & " , @PAYMENT         , @PAYMENTTAX          , @TOTALPAYMENT" _
                & " , @OTFILENAME      , @RECEIVECOUNT        , @OTSENDSTATUS   , @RESERVEDSTATUS" _
                & " , @TAKUSOUSTATUS   , @BTRAINNO            , @BTRAINNAME     , @ANASYORIFLG   , @DELFLG" _
                & " , @INITYMD         , @INITUSER            , @INITTERMID" _
                & " , @UPDYMD          , @UPDUSER             , @UPDTERMID      , @RECEIVEYMD)"

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar)                               '-- 受注№
                Dim P_TRAINNO As SqlParameter = SQLcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar)                               '-- 本線列車
                Dim P_TRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@TRAINNAME", SqlDbType.NVarChar)                           '-- 本線列車名
                Dim P_ORDERYMD As SqlParameter = SQLcmd.Parameters.Add("@ORDERYMD", SqlDbType.NVarChar)                             '-- 受注登録日
                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar)                         '-- 受注営業所コード
                Dim P_OFFICENAME As SqlParameter = SQLcmd.Parameters.Add("@OFFICENAME", SqlDbType.NVarChar)                         '-- 受注営業所名
                Dim P_ORDERTYPE As SqlParameter = SQLcmd.Parameters.Add("@ORDERTYPE", SqlDbType.NVarChar)                           '-- 受注パターン
                Dim P_SHIPPERSCODE As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSCODE", SqlDbType.NVarChar)                     '-- 荷主コード
                Dim P_SHIPPERSNAME As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSNAME", SqlDbType.NVarChar)                     '-- 荷主名
                Dim P_BASECODE As SqlParameter = SQLcmd.Parameters.Add("@BASECODE", SqlDbType.NVarChar)                             '-- 基地コード
                Dim P_BASENAME As SqlParameter = SQLcmd.Parameters.Add("@BASENAME", SqlDbType.NVarChar)                             '-- 基地名
                Dim P_CONSIGNEECODE As SqlParameter = SQLcmd.Parameters.Add("@CONSIGNEECODE", SqlDbType.NVarChar)                   '-- 荷受人コード
                Dim P_CONSIGNEENAME As SqlParameter = SQLcmd.Parameters.Add("@CONSIGNEENAME", SqlDbType.NVarChar)                   '-- 荷受人名
                Dim P_DEPSTATION As SqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", SqlDbType.NVarChar)                         '-- 発駅コード
                Dim P_DEPSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@DEPSTATIONNAME", SqlDbType.NVarChar)                 '-- 発駅名
                Dim P_ARRSTATION As SqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", SqlDbType.NVarChar)                         '-- 着駅コード
                Dim P_ARRSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@ARRSTATIONNAME", SqlDbType.NVarChar)                 '-- 着駅名
                Dim P_RETSTATION As SqlParameter = SQLcmd.Parameters.Add("@RETSTATION", SqlDbType.NVarChar)                         '-- 空車着駅コード
                Dim P_RETSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@RETSTATIONNAME", SqlDbType.NVarChar)                 '-- 空車着駅名
                Dim P_CHANGERETSTATION As SqlParameter = SQLcmd.Parameters.Add("@CHANGERETSTATION", SqlDbType.NVarChar)             '-- 空車着駅コード（変更後）
                Dim P_CHANGERETSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@CHANGERETSTATIONNAME", SqlDbType.NVarChar)     '-- 空車着駅名（変更後）
                Dim P_ORDERSTATUS As SqlParameter = SQLcmd.Parameters.Add("@ORDERSTATUS", SqlDbType.NVarChar)                       '-- 受注進行ステータス
                Dim P_ORDERINFO As SqlParameter = SQLcmd.Parameters.Add("@ORDERINFO", SqlDbType.NVarChar)                           '-- 受注情報
                Dim P_EMPTYTURNFLG As SqlParameter = SQLcmd.Parameters.Add("@EMPTYTURNFLG", SqlDbType.NVarChar)                     '-- 空回日報可否フラグ
                Dim P_STACKINGFLG As SqlParameter = SQLcmd.Parameters.Add("@STACKINGFLG", SqlDbType.NVarChar)                       '-- 積置可否フラグ
                Dim P_USEPROPRIETYFLG As SqlParameter = SQLcmd.Parameters.Add("@USEPROPRIETYFLG", SqlDbType.NVarChar)               '-- 利用可否フラグ
                Dim P_CONTACTFLG As SqlParameter = SQLcmd.Parameters.Add("@CONTACTFLG", SqlDbType.NVarChar)                         '-- 手配連絡フラグ
                Dim P_RESULTFLG As SqlParameter = SQLcmd.Parameters.Add("@RESULTFLG", SqlDbType.NVarChar)                           '-- 結果受理フラグ
                Dim P_DELIVERYFLG As SqlParameter = SQLcmd.Parameters.Add("@DELIVERYFLG", SqlDbType.NVarChar)                       '-- 託送指示フラグ
                Dim P_DELIVERYCOUNT As SqlParameter = SQLcmd.Parameters.Add("@DELIVERYCOUNT", SqlDbType.NVarChar)                   '-- 託送指示送信回数
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.NVarChar)                               '-- 積込日（予定）
                Dim P_DEPDATE As SqlParameter = SQLcmd.Parameters.Add("@DEPDATE", SqlDbType.NVarChar)                               '-- 発日（予定）
                Dim P_ARRDATE As SqlParameter = SQLcmd.Parameters.Add("@ARRDATE", SqlDbType.NVarChar)                               '-- 積車着日（予定）
                Dim P_ACCDATE As SqlParameter = SQLcmd.Parameters.Add("@ACCDATE", SqlDbType.NVarChar)                               '-- 受入日（予定）
                Dim P_EMPARRDATE As SqlParameter = SQLcmd.Parameters.Add("@EMPARRDATE", SqlDbType.NVarChar)                         '-- 空車着日（予定）
                Dim P_ACTUALLODDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALLODDATE", SqlDbType.NVarChar)                   '-- 積込日（実績）
                Dim P_ACTUALDEPDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALDEPDATE", SqlDbType.NVarChar)                   '-- 発日（実績）
                Dim P_ACTUALARRDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALARRDATE", SqlDbType.NVarChar)                   '-- 積車着日（実績）
                Dim P_ACTUALACCDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALACCDATE", SqlDbType.NVarChar)                   '-- 受入日（実績）
                Dim P_ACTUALEMPARRDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALEMPARRDATE", SqlDbType.NVarChar)             '-- 空車着日（実績）
                Dim P_RTANK As SqlParameter = SQLcmd.Parameters.Add("@RTANK", SqlDbType.NVarChar)                                   '-- 車数（レギュラー）
                Dim P_HTANK As SqlParameter = SQLcmd.Parameters.Add("@HTANK", SqlDbType.NVarChar)                                   '-- 車数（ハイオク）
                Dim P_TTANK As SqlParameter = SQLcmd.Parameters.Add("@TTANK", SqlDbType.NVarChar)                                   '-- 車数（灯油）
                Dim P_MTTANK As SqlParameter = SQLcmd.Parameters.Add("@MTTANK", SqlDbType.NVarChar)                                 '-- 車数（未添加灯油）
                Dim P_KTANK As SqlParameter = SQLcmd.Parameters.Add("@KTANK", SqlDbType.NVarChar)                                   '-- 車数（軽油）
                Dim P_K3TANK As SqlParameter = SQLcmd.Parameters.Add("@K3TANK", SqlDbType.NVarChar)                                 '-- 車数（３号軽油）
                Dim P_K5TANK As SqlParameter = SQLcmd.Parameters.Add("@K5TANK", SqlDbType.NVarChar)                                 '-- 車数（５号軽油）
                Dim P_K10TANK As SqlParameter = SQLcmd.Parameters.Add("@K10TANK", SqlDbType.NVarChar)                               '-- 車数（１０号軽油）
                Dim P_LTANK As SqlParameter = SQLcmd.Parameters.Add("@LTANK", SqlDbType.NVarChar)                                   '-- 車数（LSA）
                Dim P_ATANK As SqlParameter = SQLcmd.Parameters.Add("@ATANK", SqlDbType.NVarChar)                                   '-- 車数（A重油）
                Dim P_OTHER1OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER1OTANK", SqlDbType.NVarChar)                       '-- 車数（その他１）
                Dim P_OTHER2OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER2OTANK", SqlDbType.NVarChar)                       '-- 車数（その他２）
                Dim P_OTHER3OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER3OTANK", SqlDbType.NVarChar)                       '-- 車数（その他３）
                Dim P_OTHER4OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER4OTANK", SqlDbType.NVarChar)                       '-- 車数（その他４）
                Dim P_OTHER5OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER5OTANK", SqlDbType.NVarChar)                       '-- 車数（その他５）
                Dim P_OTHER6OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER6OTANK", SqlDbType.NVarChar)                       '-- 車数（その他６）
                Dim P_OTHER7OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER7OTANK", SqlDbType.NVarChar)                       '-- 車数（その他７）
                Dim P_OTHER8OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER8OTANK", SqlDbType.NVarChar)                       '-- 車数（その他８）
                Dim P_OTHER9OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER9OTANK", SqlDbType.NVarChar)                       '-- 車数（その他９）
                Dim P_OTHER10OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER10OTANK", SqlDbType.NVarChar)                     '-- 車数（その他１０）
                Dim P_TOTALTANK As SqlParameter = SQLcmd.Parameters.Add("@TOTALTANK", SqlDbType.NVarChar)                           '-- 合計車数
                Dim P_RTANKCH As SqlParameter = SQLcmd.Parameters.Add("@RTANKCH", SqlDbType.NVarChar)                               '-- 変更後_車数（レギュラー）
                Dim P_HTANKCH As SqlParameter = SQLcmd.Parameters.Add("@HTANKCH", SqlDbType.NVarChar)                               '-- 変更後_車数（ハイオク）
                Dim P_TTANKCH As SqlParameter = SQLcmd.Parameters.Add("@TTANKCH", SqlDbType.NVarChar)                               '-- 変更後_車数（灯油）
                Dim P_MTTANKCH As SqlParameter = SQLcmd.Parameters.Add("@MTTANKCH", SqlDbType.NVarChar)                             '-- 変更後_車数（未添加灯油）
                Dim P_KTANKCH As SqlParameter = SQLcmd.Parameters.Add("@KTANKCH", SqlDbType.NVarChar)                               '-- 変更後_車数（軽油）
                Dim P_K3TANKCH As SqlParameter = SQLcmd.Parameters.Add("@K3TANKCH", SqlDbType.NVarChar)                             '-- 変更後_車数（３号軽油）
                Dim P_K5TANKCH As SqlParameter = SQLcmd.Parameters.Add("@K5TANKCH", SqlDbType.NVarChar)                             '-- 変更後_車数（５号軽油）
                Dim P_K10TANKCH As SqlParameter = SQLcmd.Parameters.Add("@K10TANKCH", SqlDbType.NVarChar)                           '-- 変更後_車数（１０号軽油）
                Dim P_LTANKCH As SqlParameter = SQLcmd.Parameters.Add("@LTANKCH", SqlDbType.NVarChar)                               '-- 変更後_車数（LSA）
                Dim P_ATANKCH As SqlParameter = SQLcmd.Parameters.Add("@ATANKCH", SqlDbType.NVarChar)                               '-- 変更後_車数（A重油）
                Dim P_OTHER1OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER1OTANKCH", SqlDbType.NVarChar)                   '-- 変更後_車数（その他１）
                Dim P_OTHER2OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER2OTANKCH", SqlDbType.NVarChar)                   '-- 変更後_車数（その他２）
                Dim P_OTHER3OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER3OTANKCH", SqlDbType.NVarChar)                   '-- 変更後_車数（その他３）
                Dim P_OTHER4OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER4OTANKCH", SqlDbType.NVarChar)                   '-- 変更後_車数（その他４）
                Dim P_OTHER5OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER5OTANKCH", SqlDbType.NVarChar)                   '-- 変更後_車数（その他５）
                Dim P_OTHER6OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER6OTANKCH", SqlDbType.NVarChar)                   '-- 変更後_車数（その他６）
                Dim P_OTHER7OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER7OTANKCH", SqlDbType.NVarChar)                   '-- 変更後_車数（その他７）
                Dim P_OTHER8OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER8OTANKCH", SqlDbType.NVarChar)                   '-- 変更後_車数（その他８）
                Dim P_OTHER9OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER9OTANKCH", SqlDbType.NVarChar)                   '-- 変更後_車数（その他９）
                Dim P_OTHER10OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER10OTANKCH", SqlDbType.NVarChar)                 '-- 変更後_車数（その他１０）
                Dim P_TOTALTANKCH As SqlParameter = SQLcmd.Parameters.Add("@TOTALTANKCH", SqlDbType.NVarChar)                       '-- 変更後_合計車数
                Dim P_TANKLINKNO As SqlParameter = SQLcmd.Parameters.Add("@TANKLINKNO", SqlDbType.NVarChar)                         '-- 貨車連結順序表№
                Dim P_TANKLINKNOMADE As SqlParameter = SQLcmd.Parameters.Add("@TANKLINKNOMADE", SqlDbType.NVarChar)                 '-- 作成_貨車連結順序表№
                Dim P_BILLINGNO As SqlParameter = SQLcmd.Parameters.Add("@BILLINGNO", SqlDbType.NVarChar)                           '-- 支払請求№
                Dim P_KEIJYOYMD As SqlParameter = SQLcmd.Parameters.Add("@KEIJYOYMD", SqlDbType.NVarChar)                           '-- 計上日
                Dim P_SALSE As SqlParameter = SQLcmd.Parameters.Add("@SALSE", SqlDbType.NVarChar)                                   '-- 売上金額
                Dim P_SALSETAX As SqlParameter = SQLcmd.Parameters.Add("@SALSETAX", SqlDbType.NVarChar)                             '-- 売上消費税額
                Dim P_TOTALSALSE As SqlParameter = SQLcmd.Parameters.Add("@TOTALSALSE", SqlDbType.NVarChar)                         '-- 売上合計金額
                Dim P_PAYMENT As SqlParameter = SQLcmd.Parameters.Add("@PAYMENT", SqlDbType.NVarChar)                               '-- 支払金額
                Dim P_PAYMENTTAX As SqlParameter = SQLcmd.Parameters.Add("@PAYMENTTAX", SqlDbType.NVarChar)                         '-- 支払消費税額
                Dim P_TOTALPAYMENT As SqlParameter = SQLcmd.Parameters.Add("@TOTALPAYMENT", SqlDbType.NVarChar)                     '-- 支払合計金額
                Dim P_OTFILENAME As SqlParameter = SQLcmd.Parameters.Add("@OTFILENAME", SqlDbType.NVarChar)                         '-- OTファイル名
                Dim P_RECEIVECOUNT As SqlParameter = SQLcmd.Parameters.Add("@RECEIVECOUNT", SqlDbType.NVarChar)                     '-- OT空回日報受信回数
                Dim P_OTSENDSTATUS As SqlParameter = SQLcmd.Parameters.Add("@OTSENDSTATUS", SqlDbType.NVarChar)                     '-- OT発送日報送信状況
                Dim P_RESERVEDSTATUS As SqlParameter = SQLcmd.Parameters.Add("@RESERVEDSTATUS", SqlDbType.NVarChar)                 '-- 出荷予約ダウンロード状況
                Dim P_TAKUSOUSTATUS As SqlParameter = SQLcmd.Parameters.Add("@TAKUSOUSTATUS", SqlDbType.NVarChar)                   '-- 託送状ダウンロード状況
                Dim P_BTRAINNO As SqlParameter = SQLcmd.Parameters.Add("@BTRAINNO", SqlDbType.NVarChar)                             '-- 返送列車
                Dim P_BTRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@BTRAINNAME", SqlDbType.NVarChar)                         '-- 返送列車名
                Dim P_ANASYORIFLG As SqlParameter = SQLcmd.Parameters.Add("@ANASYORIFLG", SqlDbType.NVarChar)                       '-- 分析テーブル処理フラグ
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)                                 '-- 削除フラグ
                Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", SqlDbType.NVarChar)                               '-- 登録年月日
                Dim P_INITUSER As SqlParameter = SQLcmd.Parameters.Add("@INITUSER", SqlDbType.NVarChar)                             '-- 登録ユーザーＩＤ
                Dim P_INITTERMID As SqlParameter = SQLcmd.Parameters.Add("@INITTERMID", SqlDbType.NVarChar)                         '-- 登録端末
                Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", SqlDbType.NVarChar)                                 '-- 更新年月日
                Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar)                               '-- 更新ユーザーＩＤ
                Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar)                           '-- 更新端末
                Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", SqlDbType.NVarChar)                         '-- 集信日時

                For Each OIT0001OTrow As DataRow In OIT0001OTOrdertbl.Select("ORDERFLAG='0'")
                    P_ORDERNO.Value = OIT0001OTrow("ORDERNO")
                    P_TRAINNO.Value = OIT0001OTrow("TRAINNO")
                    P_TRAINNAME.Value = OIT0001OTrow("TRAINNAME")
                    P_ORDERYMD.Value = OIT0001OTrow("ORDERYMD")
                    P_OFFICECODE.Value = OIT0001OTrow("OFFICECODE")
                    P_OFFICENAME.Value = OIT0001OTrow("OFFICENAME")
                    P_ORDERTYPE.Value = OIT0001OTrow("ORDERTYPE")
                    P_SHIPPERSCODE.Value = OIT0001OTrow("SHIPPERSCODE")
                    P_SHIPPERSNAME.Value = OIT0001OTrow("SHIPPERSNAME")
                    P_BASECODE.Value = OIT0001OTrow("BASECODE")
                    P_BASENAME.Value = OIT0001OTrow("BASENAME")
                    P_CONSIGNEECODE.Value = OIT0001OTrow("CONSIGNEECODE")
                    P_CONSIGNEENAME.Value = OIT0001OTrow("CONSIGNEENAME")
                    P_DEPSTATION.Value = OIT0001OTrow("DEPSTATION")
                    P_DEPSTATIONNAME.Value = OIT0001OTrow("DEPSTATIONNAME")
                    P_ARRSTATION.Value = OIT0001OTrow("ARRSTATION")
                    P_ARRSTATIONNAME.Value = OIT0001OTrow("ARRSTATIONNAME")
                    P_RETSTATION.Value = OIT0001OTrow("RETSTATION")
                    P_RETSTATIONNAME.Value = OIT0001OTrow("RETSTATIONNAME")
                    P_CHANGERETSTATION.Value = OIT0001OTrow("CHANGERETSTATION")
                    P_CHANGERETSTATIONNAME.Value = OIT0001OTrow("CHANGERETSTATIONNAME")
                    P_ORDERSTATUS.Value = OIT0001OTrow("ORDERSTATUS")
                    P_ORDERINFO.Value = OIT0001OTrow("ORDERINFO")
                    P_EMPTYTURNFLG.Value = OIT0001OTrow("EMPTYTURNFLG")
                    P_STACKINGFLG.Value = OIT0001OTrow("STACKINGFLG")
                    P_USEPROPRIETYFLG.Value = OIT0001OTrow("USEPROPRIETYFLG")
                    P_CONTACTFLG.Value = OIT0001OTrow("CONTACTFLG")
                    P_RESULTFLG.Value = OIT0001OTrow("RESULTFLG")
                    P_DELIVERYFLG.Value = OIT0001OTrow("DELIVERYFLG")
                    P_DELIVERYCOUNT.Value = OIT0001OTrow("DELIVERYCOUNT")
                    P_LODDATE.Value = OIT0001OTrow("LODDATE")
                    P_DEPDATE.Value = OIT0001OTrow("DEPDATE")
                    P_ARRDATE.Value = OIT0001OTrow("ARRDATE")
                    P_ACCDATE.Value = OIT0001OTrow("ACCDATE")
                    P_EMPARRDATE.Value = OIT0001OTrow("EMPARRDATE")
                    P_ACTUALLODDATE.Value = OIT0001OTrow("ACTUALLODDATE")
                    P_ACTUALDEPDATE.Value = OIT0001OTrow("ACTUALDEPDATE")
                    P_ACTUALARRDATE.Value = OIT0001OTrow("ACTUALARRDATE")
                    P_ACTUALACCDATE.Value = OIT0001OTrow("ACTUALACCDATE")
                    P_ACTUALEMPARRDATE.Value = OIT0001OTrow("ACTUALEMPARRDATE")
                    P_RTANK.Value = OIT0001OTrow("RTANK")
                    P_HTANK.Value = OIT0001OTrow("HTANK")
                    P_TTANK.Value = OIT0001OTrow("TTANK")
                    P_MTTANK.Value = OIT0001OTrow("MTTANK")
                    P_KTANK.Value = OIT0001OTrow("KTANK")
                    P_K3TANK.Value = OIT0001OTrow("K3TANK")
                    P_K5TANK.Value = OIT0001OTrow("K5TANK")
                    P_K10TANK.Value = OIT0001OTrow("K10TANK")
                    P_LTANK.Value = OIT0001OTrow("LTANK")
                    P_ATANK.Value = OIT0001OTrow("ATANK")
                    P_OTHER1OTANK.Value = OIT0001OTrow("OTHER1OTANK")
                    P_OTHER2OTANK.Value = OIT0001OTrow("OTHER2OTANK")
                    P_OTHER3OTANK.Value = OIT0001OTrow("OTHER3OTANK")
                    P_OTHER4OTANK.Value = OIT0001OTrow("OTHER4OTANK")
                    P_OTHER5OTANK.Value = OIT0001OTrow("OTHER5OTANK")
                    P_OTHER6OTANK.Value = OIT0001OTrow("OTHER6OTANK")
                    P_OTHER7OTANK.Value = OIT0001OTrow("OTHER7OTANK")
                    P_OTHER8OTANK.Value = OIT0001OTrow("OTHER8OTANK")
                    P_OTHER9OTANK.Value = OIT0001OTrow("OTHER9OTANK")
                    P_OTHER10OTANK.Value = OIT0001OTrow("OTHER10OTANK")
                    P_TOTALTANK.Value = OIT0001OTrow("TOTALTANK")
                    P_RTANKCH.Value = OIT0001OTrow("RTANKCH")
                    P_HTANKCH.Value = OIT0001OTrow("HTANKCH")
                    P_TTANKCH.Value = OIT0001OTrow("TTANKCH")
                    P_MTTANKCH.Value = OIT0001OTrow("MTTANKCH")
                    P_KTANKCH.Value = OIT0001OTrow("KTANKCH")
                    P_K3TANKCH.Value = OIT0001OTrow("K3TANKCH")
                    P_K5TANKCH.Value = OIT0001OTrow("K5TANKCH")
                    P_K10TANKCH.Value = OIT0001OTrow("K10TANKCH")
                    P_LTANKCH.Value = OIT0001OTrow("LTANKCH")
                    P_ATANKCH.Value = OIT0001OTrow("ATANKCH")
                    P_OTHER1OTANKCH.Value = OIT0001OTrow("OTHER1OTANKCH")
                    P_OTHER2OTANKCH.Value = OIT0001OTrow("OTHER2OTANKCH")
                    P_OTHER3OTANKCH.Value = OIT0001OTrow("OTHER3OTANKCH")
                    P_OTHER4OTANKCH.Value = OIT0001OTrow("OTHER4OTANKCH")
                    P_OTHER5OTANKCH.Value = OIT0001OTrow("OTHER5OTANKCH")
                    P_OTHER6OTANKCH.Value = OIT0001OTrow("OTHER6OTANKCH")
                    P_OTHER7OTANKCH.Value = OIT0001OTrow("OTHER7OTANKCH")
                    P_OTHER8OTANKCH.Value = OIT0001OTrow("OTHER8OTANKCH")
                    P_OTHER9OTANKCH.Value = OIT0001OTrow("OTHER9OTANKCH")
                    P_OTHER10OTANKCH.Value = OIT0001OTrow("OTHER10OTANKCH")
                    P_TOTALTANKCH.Value = OIT0001OTrow("TOTALTANKCH")
                    P_TANKLINKNO.Value = OIT0001OTrow("TANKLINKNO")
                    P_TANKLINKNOMADE.Value = OIT0001OTrow("TANKLINKNOMADE")
                    P_BILLINGNO.Value = OIT0001OTrow("BILLINGNO")
                    P_KEIJYOYMD.Value = OIT0001OTrow("KEIJYOYMD")
                    P_SALSE.Value = OIT0001OTrow("SALSE")
                    P_SALSETAX.Value = OIT0001OTrow("SALSETAX")
                    P_TOTALSALSE.Value = OIT0001OTrow("TOTALSALSE")
                    P_PAYMENT.Value = OIT0001OTrow("PAYMENT")
                    P_PAYMENTTAX.Value = OIT0001OTrow("PAYMENTTAX")
                    P_TOTALPAYMENT.Value = OIT0001OTrow("TOTALPAYMENT")
                    P_OTFILENAME.Value = OIT0001OTrow("OTFILENAME")
                    P_RECEIVECOUNT.Value = OIT0001OTrow("RECEIVECOUNT")
                    P_OTSENDSTATUS.Value = OIT0001OTrow("OTSENDSTATUS")
                    P_RESERVEDSTATUS.Value = OIT0001OTrow("RESERVEDSTATUS")
                    P_TAKUSOUSTATUS.Value = OIT0001OTrow("TAKUSOUSTATUS")
                    P_BTRAINNO.Value = OIT0001OTrow("BTRAINNO")
                    P_BTRAINNAME.Value = OIT0001OTrow("BTRAINNAME")
                    P_ANASYORIFLG.Value = OIT0001OTrow("ANASYORIFLG")
                    P_DELFLG.Value = OIT0001OTrow("DELFLG")
                    P_INITYMD.Value = OIT0001OTrow("INITYMD")
                    P_INITUSER.Value = OIT0001OTrow("INITUSER")
                    P_INITTERMID.Value = OIT0001OTrow("INITTERMID")
                    P_UPDYMD.Value = OIT0001OTrow("UPDYMD")
                    P_UPDUSER.Value = OIT0001OTrow("UPDUSER")
                    P_UPDTERMID.Value = OIT0001OTrow("UPDTERMID")
                    P_RECEIVEYMD.Value = OIT0001OTrow("RECEIVEYMD")

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L_InsertOrder")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L_InsertOrder"
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
    ''' OT受注明細TBL⇒受注明細TBLへ追加
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_InsertOrderDetail(ByVal SQLcon As SqlConnection)
        Try
            '追加SQL文･･･受注明細TBL追加
            Dim SQLStr As String =
                  " INSERT INTO OIL.OIT0003_DETAIL " _
                & " ( ORDERNO              , DETAILNO               , SHIPORDER       , LINEORDER" _
                & " , TANKNO               , KAMOKU                 , STACKINGORDERNO , STACKINGFLG" _
                & " , WHOLESALEFLG         , INSPECTIONFLG          , DETENTIONFLG" _
                & " , FIRSTRETURNFLG       , AFTERRETURNFLG         , OTTRANSPORTFLG  , UPGRADEFLG" _
                & " , ORDERINFO            , SHIPPERSCODE           , SHIPPERSNAME" _
                & " , OILCODE              , OILNAME                , ORDERINGTYPE    , ORDERINGOILNAME" _
                & " , CARSNUMBER           , CARSAMOUNT             , RETURNDATETRAIN" _
                & " , JOINTCODE            , JOINT                  , REMARK          , CHANGETRAINNO      , CHANGETRAINNAME" _
                & " , SECONDCONSIGNEECODE  , SECONDCONSIGNEENAME    , SECONDARRSTATION, SECONDARRSTATIONNAME" _
                & " , CHANGERETSTATION     , CHANGERETSTATIONNAME   , LINE            , FILLINGPOINT" _
                & " , LOADINGIRILINETRAINNO, LOADINGIRILINETRAINNAME, LOADINGIRILINEORDER" _
                & " , LOADINGOUTLETTRAINNO , LOADINGOUTLETTRAINNAME , LOADINGOUTLETORDER" _
                & " , ACTUALLODDATE        , ACTUALDEPDATE          , ACTUALARRDATE   , ACTUALACCDATE      , ACTUALEMPARRDATE" _
                & " , RESERVEDNO           , GYONO                  , OTSENDCOUNT     , DLRESERVEDCOUNT    , DLTAKUSOUCOUNT" _
                & " , SALSE                , SALSETAX               , TOTALSALSE      , PAYMENT            , PAYMENTTAX      , TOTALPAYMENT" _
                & " , ANASYORIFLG          , VOLSYORIFLG            , DELFLG" _
                & " , INITYMD              , INITUSER               , INITTERMID" _
                & " , UPDYMD               , UPDUSER                , UPDTERMID       , RECEIVEYMD)"

            SQLStr &=
                  " VALUES" _
                & " ( @ORDERNO              , @DETAILNO               , @SHIPORDER       , @LINEORDER" _
                & " , @TANKNO               , @KAMOKU                 , @STACKINGORDERNO , @STACKINGFLG" _
                & " , @WHOLESALEFLG         , @INSPECTIONFLG          , @DETENTIONFLG" _
                & " , @FIRSTRETURNFLG       , @AFTERRETURNFLG         , @OTTRANSPORTFLG  , @UPGRADEFLG" _
                & " , @ORDERINFO            , @SHIPPERSCODE           , @SHIPPERSNAME" _
                & " , @OILCODE              , @OILNAME                , @ORDERINGTYPE    , @ORDERINGOILNAME" _
                & " , @CARSNUMBER           , @CARSAMOUNT             , @RETURNDATETRAIN" _
                & " , @JOINTCODE            , @JOINT                  , @REMARK          , @CHANGETRAINNO      , @CHANGETRAINNAME" _
                & " , @SECONDCONSIGNEECODE  , @SECONDCONSIGNEENAME    , @SECONDARRSTATION, @SECONDARRSTATIONNAME" _
                & " , @CHANGERETSTATION     , @CHANGERETSTATIONNAME   , @LINE            , @FILLINGPOINT" _
                & " , @LOADINGIRILINETRAINNO, @LOADINGIRILINETRAINNAME, @LOADINGIRILINEORDER" _
                & " , @LOADINGOUTLETTRAINNO , @LOADINGOUTLETTRAINNAME , @LOADINGOUTLETORDER" _
                & " , @ACTUALLODDATE        , @ACTUALDEPDATE          , @ACTUALARRDATE   , @ACTUALACCDATE      , @ACTUALEMPARRDATE" _
                & " , @RESERVEDNO           , @GYONO                  , @OTSENDCOUNT     , @DLRESERVEDCOUNT    , @DLTAKUSOUCOUNT" _
                & " , @SALSE                , @SALSETAX               , @TOTALSALSE      , @PAYMENT            , @PAYMENTTAX      , @TOTALPAYMENT" _
                & " , @ANASYORIFLG          , @VOLSYORIFLG            , @DELFLG" _
                & " , @INITYMD              , @INITUSER               , @INITTERMID" _
                & " , @UPDYMD               , @UPDUSER                , @UPDTERMID       , @RECEIVEYMD)"

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar)                                   '-- 受注№
                Dim P_DETAILNO As SqlParameter = SQLcmd.Parameters.Add("@DETAILNO", SqlDbType.NVarChar)                                 '-- 受注明細№
                Dim P_SHIPORDER As SqlParameter = SQLcmd.Parameters.Add("@SHIPORDER", SqlDbType.NVarChar)                               '-- 発送順
                Dim P_LINEORDER As SqlParameter = SQLcmd.Parameters.Add("@LINEORDER", SqlDbType.NVarChar)                               '-- 入線順
                Dim P_TANKNO As SqlParameter = SQLcmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar)                                     '-- タンク車№
                Dim P_KAMOKU As SqlParameter = SQLcmd.Parameters.Add("@KAMOKU", SqlDbType.NVarChar)                                     '-- 費用科目
                Dim P_STACKINGORDERNO As SqlParameter = SQLcmd.Parameters.Add("@STACKINGORDERNO", SqlDbType.NVarChar)                   '-- 積置受注№
                Dim P_STACKINGFLG As SqlParameter = SQLcmd.Parameters.Add("@STACKINGFLG", SqlDbType.NVarChar)                           '-- 積置可否フラグ
                Dim P_WHOLESALEFLG As SqlParameter = SQLcmd.Parameters.Add("@WHOLESALEFLG", SqlDbType.NVarChar)                         '-- 未卸可否フラグ
                Dim P_INSPECTIONFLG As SqlParameter = SQLcmd.Parameters.Add("@INSPECTIONFLG", SqlDbType.NVarChar)                       '-- 交検可否フラグ
                Dim P_DETENTIONFLG As SqlParameter = SQLcmd.Parameters.Add("@DETENTIONFLG", SqlDbType.NVarChar)                         '-- 留置可否フラグ
                Dim P_FIRSTRETURNFLG As SqlParameter = SQLcmd.Parameters.Add("@FIRSTRETURNFLG", SqlDbType.NVarChar)                     '-- 先返し可否フラグ
                Dim P_AFTERRETURNFLG As SqlParameter = SQLcmd.Parameters.Add("@AFTERRETURNFLG", SqlDbType.NVarChar)                     '-- 後返し可否フラグ
                Dim P_OTTRANSPORTFLG As SqlParameter = SQLcmd.Parameters.Add("@OTTRANSPORTFLG", SqlDbType.NVarChar)                     '-- OT輸送可否フラグ
                Dim P_UPGRADEFLG As SqlParameter = SQLcmd.Parameters.Add("@UPGRADEFLG", SqlDbType.NVarChar)                             '-- 格上(格下)可否フラグ
                Dim P_ORDERINFO As SqlParameter = SQLcmd.Parameters.Add("@ORDERINFO", SqlDbType.NVarChar)                               '-- 受注情報
                Dim P_SHIPPERSCODE As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSCODE", SqlDbType.NVarChar)                         '-- 荷主コード
                Dim P_SHIPPERSNAME As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSNAME", SqlDbType.NVarChar)                         '-- 荷主名
                Dim P_OILCODE As SqlParameter = SQLcmd.Parameters.Add("@OILCODE", SqlDbType.NVarChar)                                   '-- 油種コード
                Dim P_OILNAME As SqlParameter = SQLcmd.Parameters.Add("@OILNAME", SqlDbType.NVarChar)                                   '-- 油種名
                Dim P_ORDERINGTYPE As SqlParameter = SQLcmd.Parameters.Add("@ORDERINGTYPE", SqlDbType.NVarChar)                         '-- 油種区分(受発注用)
                Dim P_ORDERINGOILNAME As SqlParameter = SQLcmd.Parameters.Add("@ORDERINGOILNAME", SqlDbType.NVarChar)                   '-- 油種名(受発注用)
                Dim P_CARSNUMBER As SqlParameter = SQLcmd.Parameters.Add("@CARSNUMBER", SqlDbType.NVarChar)                             '-- 車数
                Dim P_CARSAMOUNT As SqlParameter = SQLcmd.Parameters.Add("@CARSAMOUNT", SqlDbType.NVarChar)                             '-- 数量
                Dim P_RETURNDATETRAIN As SqlParameter = SQLcmd.Parameters.Add("@RETURNDATETRAIN", SqlDbType.NVarChar)                   '-- 返送日列車
                Dim P_JOINTCODE As SqlParameter = SQLcmd.Parameters.Add("@JOINTCODE", SqlDbType.NVarChar)                               '-- ジョイントコード
                Dim P_JOINT As SqlParameter = SQLcmd.Parameters.Add("@JOINT", SqlDbType.NVarChar)                                       '-- ジョイント
                Dim P_REMARK As SqlParameter = SQLcmd.Parameters.Add("@REMARK", SqlDbType.NVarChar)                                     '-- 備考
                Dim P_CHANGETRAINNO As SqlParameter = SQLcmd.Parameters.Add("@CHANGETRAINNO", SqlDbType.NVarChar)                       '-- 本線列車（変更後）
                Dim P_CHANGETRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@CHANGETRAINNAME", SqlDbType.NVarChar)                   '-- 本線列車名（変更後）
                Dim P_SECONDCONSIGNEECODE As SqlParameter = SQLcmd.Parameters.Add("@SECONDCONSIGNEECODE", SqlDbType.NVarChar)           '-- 第2荷受人コード
                Dim P_SECONDCONSIGNEENAME As SqlParameter = SQLcmd.Parameters.Add("@SECONDCONSIGNEENAME", SqlDbType.NVarChar)           '-- 第2荷受人名
                Dim P_SECONDARRSTATION As SqlParameter = SQLcmd.Parameters.Add("@SECONDARRSTATION", SqlDbType.NVarChar)                 '-- 第2着駅コード
                Dim P_SECONDARRSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@SECONDARRSTATIONNAME", SqlDbType.NVarChar)         '-- 第2着駅名
                Dim P_CHANGERETSTATION As SqlParameter = SQLcmd.Parameters.Add("@CHANGERETSTATION", SqlDbType.NVarChar)                 '-- 空車着駅コード（変更後）
                Dim P_CHANGERETSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@CHANGERETSTATIONNAME", SqlDbType.NVarChar)         '-- 空車着駅名（変更後）
                Dim P_LINE As SqlParameter = SQLcmd.Parameters.Add("@LINE", SqlDbType.NVarChar)                                         '-- 回線
                Dim P_FILLINGPOINT As SqlParameter = SQLcmd.Parameters.Add("@FILLINGPOINT", SqlDbType.NVarChar)                         '-- 充填ポイント
                Dim P_LOADINGIRILINETRAINNO As SqlParameter = SQLcmd.Parameters.Add("@LOADINGIRILINETRAINNO", SqlDbType.NVarChar)       '-- 積込入線列車番号
                Dim P_LOADINGIRILINETRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@LOADINGIRILINETRAINNAME", SqlDbType.NVarChar)   '-- 積込入線列車番号名
                Dim P_LOADINGIRILINEORDER As SqlParameter = SQLcmd.Parameters.Add("@LOADINGIRILINEORDER", SqlDbType.NVarChar)           '-- 積込入線順
                Dim P_LOADINGOUTLETTRAINNO As SqlParameter = SQLcmd.Parameters.Add("@LOADINGOUTLETTRAINNO", SqlDbType.NVarChar)         '-- 積込出線列車番号
                Dim P_LOADINGOUTLETTRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@LOADINGOUTLETTRAINNAME", SqlDbType.NVarChar)     '-- 積込出線列車番号名
                Dim P_LOADINGOUTLETORDER As SqlParameter = SQLcmd.Parameters.Add("@LOADINGOUTLETORDER", SqlDbType.NVarChar)             '-- 積込出線順
                Dim P_ACTUALLODDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALLODDATE", SqlDbType.NVarChar)                       '-- 積込日（実績）
                Dim P_ACTUALDEPDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALDEPDATE", SqlDbType.NVarChar)                       '-- 発日（実績）
                Dim P_ACTUALARRDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALARRDATE", SqlDbType.NVarChar)                       '-- 積車着日（実績）
                Dim P_ACTUALACCDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALACCDATE", SqlDbType.NVarChar)                       '-- 受入日（実績）
                Dim P_ACTUALEMPARRDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALEMPARRDATE", SqlDbType.NVarChar)                 '-- 空車着日（実績）
                Dim P_RESERVEDNO As SqlParameter = SQLcmd.Parameters.Add("@RESERVEDNO", SqlDbType.NVarChar)                             '-- 予約番号
                Dim P_GYONO As SqlParameter = SQLcmd.Parameters.Add("@GYONO", SqlDbType.NVarChar)                                       '-- 行番号
                Dim P_OTSENDCOUNT As SqlParameter = SQLcmd.Parameters.Add("@OTSENDCOUNT", SqlDbType.NVarChar)                           '-- OT発送日報送信回数
                Dim P_DLRESERVEDCOUNT As SqlParameter = SQLcmd.Parameters.Add("@DLRESERVEDCOUNT", SqlDbType.NVarChar)                   '-- 出荷予約ダウンロード回数
                Dim P_DLTAKUSOUCOUNT As SqlParameter = SQLcmd.Parameters.Add("@DLTAKUSOUCOUNT", SqlDbType.NVarChar)                     '-- 託送状ダウンロード回数
                Dim P_SALSE As SqlParameter = SQLcmd.Parameters.Add("@SALSE", SqlDbType.NVarChar)                                       '-- 売上金額
                Dim P_SALSETAX As SqlParameter = SQLcmd.Parameters.Add("@SALSETAX", SqlDbType.NVarChar)                                 '-- 売上消費税額
                Dim P_TOTALSALSE As SqlParameter = SQLcmd.Parameters.Add("@TOTALSALSE", SqlDbType.NVarChar)                             '-- 売上合計金額
                Dim P_PAYMENT As SqlParameter = SQLcmd.Parameters.Add("@PAYMENT", SqlDbType.NVarChar)                                   '-- 支払金額
                Dim P_PAYMENTTAX As SqlParameter = SQLcmd.Parameters.Add("@PAYMENTTAX", SqlDbType.NVarChar)                             '-- 支払消費税額
                Dim P_TOTALPAYMENT As SqlParameter = SQLcmd.Parameters.Add("@TOTALPAYMENT", SqlDbType.NVarChar)                         '-- 支払合計金額
                Dim P_ANASYORIFLG As SqlParameter = SQLcmd.Parameters.Add("@ANASYORIFLG", SqlDbType.NVarChar)                           '-- 分析テーブル処理フラグ
                Dim P_VOLSYORIFLG As SqlParameter = SQLcmd.Parameters.Add("@VOLSYORIFLG", SqlDbType.NVarChar)                           '-- 月間輸送量処理フラグ
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)                                     '-- 削除フラグ
                Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", SqlDbType.NVarChar)                                   '-- 登録年月日
                Dim P_INITUSER As SqlParameter = SQLcmd.Parameters.Add("@INITUSER", SqlDbType.NVarChar)                                 '-- 登録ユーザーＩＤ
                Dim P_INITTERMID As SqlParameter = SQLcmd.Parameters.Add("@INITTERMID", SqlDbType.NVarChar)                             '-- 登録端末
                Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", SqlDbType.NVarChar)                                     '-- 更新年月日
                Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar)                                   '-- 更新ユーザーＩＤ
                Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar)                               '-- 更新端末
                Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", SqlDbType.NVarChar)                             '-- 集信日時

                For Each OIT0001OTrow As DataRow In OIT0001OTDetailtbl.Select("ORDERFLAG='0'")
                    P_ORDERNO.Value = OIT0001OTrow("ORDERNO")
                    P_DETAILNO.Value = OIT0001OTrow("DETAILNO")
                    P_SHIPORDER.Value = OIT0001OTrow("SHIPORDER")
                    P_LINEORDER.Value = OIT0001OTrow("LINEORDER")
                    P_TANKNO.Value = OIT0001OTrow("TANKNO")
                    P_KAMOKU.Value = OIT0001OTrow("KAMOKU")
                    P_STACKINGORDERNO.Value = OIT0001OTrow("STACKINGORDERNO")
                    P_STACKINGFLG.Value = OIT0001OTrow("STACKINGFLG")
                    P_WHOLESALEFLG.Value = OIT0001OTrow("WHOLESALEFLG")
                    P_INSPECTIONFLG.Value = OIT0001OTrow("INSPECTIONFLG")
                    P_DETENTIONFLG.Value = OIT0001OTrow("DETENTIONFLG")
                    P_FIRSTRETURNFLG.Value = OIT0001OTrow("FIRSTRETURNFLG")
                    P_AFTERRETURNFLG.Value = OIT0001OTrow("AFTERRETURNFLG")
                    P_OTTRANSPORTFLG.Value = OIT0001OTrow("OTTRANSPORTFLG")
                    P_UPGRADEFLG.Value = OIT0001OTrow("UPGRADEFLG")
                    P_ORDERINFO.Value = OIT0001OTrow("ORDERINFO")
                    P_SHIPPERSCODE.Value = OIT0001OTrow("SHIPPERSCODE")
                    P_SHIPPERSNAME.Value = OIT0001OTrow("SHIPPERSNAME")
                    P_OILCODE.Value = OIT0001OTrow("OILCODE")
                    P_OILNAME.Value = OIT0001OTrow("OILNAME")
                    P_ORDERINGTYPE.Value = OIT0001OTrow("ORDERINGTYPE")
                    P_ORDERINGOILNAME.Value = OIT0001OTrow("ORDERINGOILNAME")
                    P_CARSNUMBER.Value = OIT0001OTrow("CARSNUMBER")
                    P_CARSAMOUNT.Value = OIT0001OTrow("CARSAMOUNT")
                    P_RETURNDATETRAIN.Value = OIT0001OTrow("RETURNDATETRAIN")
                    P_JOINTCODE.Value = OIT0001OTrow("JOINTCODE")
                    P_JOINT.Value = OIT0001OTrow("JOINT")
                    P_REMARK.Value = OIT0001OTrow("REMARK")
                    P_CHANGETRAINNO.Value = OIT0001OTrow("CHANGETRAINNO")
                    P_CHANGETRAINNAME.Value = OIT0001OTrow("CHANGETRAINNAME")
                    P_SECONDCONSIGNEECODE.Value = OIT0001OTrow("SECONDCONSIGNEECODE")
                    P_SECONDCONSIGNEENAME.Value = OIT0001OTrow("SECONDCONSIGNEENAME")
                    P_SECONDARRSTATION.Value = OIT0001OTrow("SECONDARRSTATION")
                    P_SECONDARRSTATIONNAME.Value = OIT0001OTrow("SECONDARRSTATIONNAME")
                    P_CHANGERETSTATION.Value = OIT0001OTrow("CHANGERETSTATION")
                    P_CHANGERETSTATIONNAME.Value = OIT0001OTrow("CHANGERETSTATIONNAME")
                    P_LINE.Value = OIT0001OTrow("LINE")
                    P_FILLINGPOINT.Value = OIT0001OTrow("FILLINGPOINT")
                    P_LOADINGIRILINETRAINNO.Value = OIT0001OTrow("LOADINGIRILINETRAINNO")
                    P_LOADINGIRILINETRAINNAME.Value = OIT0001OTrow("LOADINGIRILINETRAINNAME")
                    P_LOADINGIRILINEORDER.Value = OIT0001OTrow("LOADINGIRILINEORDER")
                    P_LOADINGOUTLETTRAINNO.Value = OIT0001OTrow("LOADINGOUTLETTRAINNO")
                    P_LOADINGOUTLETTRAINNAME.Value = OIT0001OTrow("LOADINGOUTLETTRAINNAME")
                    P_LOADINGOUTLETORDER.Value = OIT0001OTrow("LOADINGOUTLETORDER")
                    P_ACTUALLODDATE.Value = OIT0001OTrow("ACTUALLODDATE")
                    P_ACTUALDEPDATE.Value = OIT0001OTrow("ACTUALDEPDATE")
                    P_ACTUALARRDATE.Value = OIT0001OTrow("ACTUALARRDATE")
                    P_ACTUALACCDATE.Value = OIT0001OTrow("ACTUALACCDATE")
                    P_ACTUALEMPARRDATE.Value = OIT0001OTrow("ACTUALEMPARRDATE")
                    P_RESERVEDNO.Value = OIT0001OTrow("RESERVEDNO")
                    P_GYONO.Value = OIT0001OTrow("GYONO")
                    P_OTSENDCOUNT.Value = OIT0001OTrow("OTSENDCOUNT")
                    P_DLRESERVEDCOUNT.Value = OIT0001OTrow("DLRESERVEDCOUNT")
                    P_DLTAKUSOUCOUNT.Value = OIT0001OTrow("DLTAKUSOUCOUNT")
                    P_SALSE.Value = OIT0001OTrow("SALSE")
                    P_SALSETAX.Value = OIT0001OTrow("SALSETAX")
                    P_TOTALSALSE.Value = OIT0001OTrow("TOTALSALSE")
                    P_PAYMENT.Value = OIT0001OTrow("PAYMENT")
                    P_PAYMENTTAX.Value = OIT0001OTrow("PAYMENTTAX")
                    P_TOTALPAYMENT.Value = OIT0001OTrow("TOTALPAYMENT")
                    P_ANASYORIFLG.Value = OIT0001OTrow("ANASYORIFLG")
                    P_VOLSYORIFLG.Value = OIT0001OTrow("VOLSYORIFLG")
                    P_DELFLG.Value = OIT0001OTrow("DELFLG")
                    P_INITYMD.Value = OIT0001OTrow("INITYMD")
                    P_INITUSER.Value = OIT0001OTrow("INITUSER")
                    P_INITTERMID.Value = OIT0001OTrow("INITTERMID")
                    P_UPDYMD.Value = OIT0001OTrow("UPDYMD")
                    P_UPDUSER.Value = OIT0001OTrow("UPDUSER")
                    P_UPDTERMID.Value = OIT0001OTrow("UPDTERMID")
                    P_RECEIVEYMD.Value = OIT0001OTrow("RECEIVEYMD")

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L_InsertOrderDetail")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L_InsertOrderDetail"
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
    ''' 受注TBLステータス更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderStatus(ByVal SQLcon As SqlConnection,
                                         Optional I_ITEM As String = Nothing,
                                         Optional I_VALUE As String = Nothing)
        'Try
        '    '更新SQL文･･･OT受注TBLのステータスを更新
        '    Dim SQLStr As String =
        '              " UPDATE OIL.OIT0002_OTORDER " _
        '            & "    SET " _
        '            & String.Format("        {0}  = '{1}', ", I_ITEM, I_VALUE)

        '    SQLStr &=
        '              "        UPDYMD      = @UPDYMD, " _
        '            & "        UPDUSER     = @UPDUSER, " _
        '            & "        UPDTERMID   = @UPDTERMID, " _
        '            & "        RECEIVEYMD  = @RECEIVEYMD  " _
        '            & "  WHERE ORDERNO     = @ORDERNO  " _
        '            & "    AND DELFLG     <> @DELFLG; "

        '    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        '    SQLcmd.CommandTimeout = 300
        '    Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", System.Data.SqlDbType.NVarChar)
        '    Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)

        '    Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
        '    Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
        '    Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
        '    Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

        '    P_DELFLG.Value = C_DELETE_FLG.DELETE
        '    P_UPDYMD.Value = Date.Now
        '    P_UPDUSER.Value = Master.USERID
        '    P_UPDTERMID.Value = Master.USERTERMID
        '    P_RECEIVEYMD.Value = C_DEFAULT_YMD

        '    For Each OIT0001OTrow As DataRow In OIT0001OTOrdertbl.Rows
        '        P_ORDERNO.Value = OIT0001OTrow("ORDERNO")

        '        SQLcmd.ExecuteNonQuery()
        '    Next

        '    'CLOSE
        '    SQLcmd.Dispose()
        '    SQLcmd = Nothing

        'Catch ex As Exception
        '    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L_ORDERSTATUS UPDATE")
        '    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
        '    CS0011LOGWrite.INFPOSI = "DB:OIT0001L_ORDERSTATUS UPDATE"
        '    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
        '    CS0011LOGWrite.TEXT = ex.ToString()
        '    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
        '    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        '    Exit Sub

        'End Try

        ''○メッセージ表示
        ''Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' OT受注TBLステータス更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOTOrderStatus(ByVal SQLcon As SqlConnection,
                                         Optional OIT0001row As DataRow = Nothing,
                                         Optional I_ITEM As String = Nothing,
                                         Optional I_VALUE As String = Nothing)
        Try
            '更新SQL文･･･OT受注TBLのステータスを更新
            Dim SQLStr As String =
                      " UPDATE OIL.OIT0016_OTORDER " _
                    & "    SET " _
                    & String.Format("        {0}  = '{1}', ", I_ITEM, I_VALUE)

            SQLStr &=
                      "        UPDYMD      = @UPDYMD, " _
                    & "        UPDUSER     = @UPDUSER, " _
                    & "        UPDTERMID   = @UPDTERMID, " _
                    & "        RECEIVEYMD  = @RECEIVEYMD  " _
                    & "  WHERE ORDERNO     = @ORDERNO  "
            '& "    AND DELFLG     <> @DELFLG; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300
            Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", System.Data.SqlDbType.NVarChar)
            'Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)

            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            'P_DELFLG.Value = C_DELETE_FLG.DELETE
            P_UPDYMD.Value = Date.Now
            P_UPDUSER.Value = Master.USERID
            P_UPDTERMID.Value = Master.USERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            If IsNothing(OIT0001row) Then
                For Each OIT0001OTrow As DataRow In OIT0001OTOrdertbl.Rows
                    P_ORDERNO.Value = OIT0001OTrow("ORDERNO")
                    SQLcmd.ExecuteNonQuery()
                Next

            Else
                P_ORDERNO.Value = OIT0001row("ORDERNO")
                SQLcmd.ExecuteNonQuery()
            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L_OTORDERSTATUS UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L_OTORDERSTATUS UPDATE"
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
    ''' 受注TBLとOT受注TBL比較
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_CompareOrderToOTOrder(ByVal SQLcon As SqlConnection)

        Dim inOrder As String = ""

        If IsNothing(OIT0001CMPOrdertbl) Then
            OIT0001CMPOrdertbl = New DataTable
        End If

        If OIT0001CMPOrdertbl.Columns.Count <> 0 Then
            OIT0001CMPOrdertbl.Columns.Clear()
        End If

        OIT0001CMPOrdertbl.Clear()

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As String = RSSQL.EmptyTurnDairyOTCompare(OIT0001OTOrdertbl, inOrder)

        '削除・追加用
        Dim SQLTempTblStr As String =
          " DELETE FROM OIL.OIT0020_OTCOMPARE " _
        & " WHERE KEYCODE3 = @OFFICECODE " _
        & String.Format("   AND KEYCODE1 IN {0}; ", inOrder) _
        & " INSERT INTO OIL.OIT0020_OTCOMPARE " _
        & " (KEYCODE1, KEYCODE2, KEYCODE3, COMPAREINFOCD, COMPAREINFONM, " _
        & "  ORDERNO, DETAILNO, ORDERSTATUS, TRAINNO, TRAINNAME, ORDERYMD, " _
        & "  OFFICECODE, OFFICENAME, SHIPPERSCODE, SHIPPERSNAME, BASECODE, BASENAME, " _
        & "  CONSIGNEECODE, CONSIGNEENAME, DEPSTATION, DEPSTATIONNAME, " _
        & "  ARRSTATION, ARRSTATIONNAME, SHIPORDER, LINEORDER, TANKNO, " _
        & "  OILCODE, OILNAME, ORDERINGTYPE, ORDERINGOILNAME, CARSNUMBER, CARSAMOUNT, RETURNDATETRAIN, " _
        & "  JOINTCODE, JOINT, REMARK, SECONDCONSIGNEECODE, SECONDCONSIGNEENAME, " _
        & "  LODDATE, DEPDATE, ARRDATE, ACCDATE, EMPARRDATE, BTRAINNO, IMPORTFLG, " _
        & "  OT_ORDERNO, OT_DETAILNO, OT_ORDERSTATUS, OT_TRAINNO, OT_TRAINNAME, OT_ORDERYMD, " _
        & "  OT_OFFICECODE, OT_OFFICENAME, OT_SHIPPERSCODE, OT_SHIPPERSNAME, OT_BASECODE, OT_BASENAME, " _
        & "  OT_CONSIGNEECODE, OT_CONSIGNEENAME, OT_DEPSTATION, OT_DEPSTATIONNAME, " _
        & "  OT_ARRSTATION, OT_ARRSTATIONNAME, OT_SHIPORDER, OT_LINEORDER, OT_TANKNO, " _
        & "  OT_OILCODE, OT_OILNAME, OT_ORDERINGTYPE, OT_ORDERINGOILNAME, OT_CARSNUMBER, OT_CARSAMOUNT, OT_RETURNDATETRAIN, " _
        & "  OT_JOINTCODE, OT_JOINT, OT_REMARK, OT_SECONDCONSIGNEECODE, OT_SECONDCONSIGNEENAME, " _
        & "  OT_LODDATE, OT_DEPDATE, OT_ARRDATE, OT_ACCDATE, OT_EMPARRDATE, OT_BTRAINNO, " _
        & "  DELFLG, INITYMD, INITUSER, INITTERMID, UPDYMD, UPDUSER, UPDTERMID, RECEIVEYMD) "
        'Dim SQLTempTblStr As String =
        '  " DELETE FROM OIL.TMP0009_OTRECEIVECOMPARE " _
        '& " WHERE KEYCODE2 = @OFFICECODE " _
        '& String.Format("   AND KEYCODE1 IN {0}; ", inOrder) _
        '& " INSERT INTO OIL.TMP0009_OTRECEIVECOMPARE "

        '削除・追加用にSELECT分を追加
        SQLTempTblStr &= SQLStr

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLTMPcmd As New SqlCommand(SQLTempTblStr, SQLcon)
                'tmp作成用
                Dim PT_OFFICECODE As SqlParameter = SQLTMPcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar) '営業所コード
                Dim PT_ORDERYMD As SqlParameter = SQLTMPcmd.Parameters.Add("@ORDERYMD", SqlDbType.Date)         '登録日(当日)
                Dim PT_INITYMD As SqlParameter = SQLTMPcmd.Parameters.Add("@INITYMD", System.Data.SqlDbType.DateTime)
                Dim PT_INITUSER As SqlParameter = SQLTMPcmd.Parameters.Add("@INITUSER", System.Data.SqlDbType.NVarChar)
                Dim PT_INITTERMID As SqlParameter = SQLTMPcmd.Parameters.Add("@INITTERMID", System.Data.SqlDbType.NVarChar)
                Dim PT_UPDYMD As SqlParameter = SQLTMPcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
                Dim PT_UPDUSER As SqlParameter = SQLTMPcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
                Dim PT_UPDTERMID As SqlParameter = SQLTMPcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
                Dim PT_RECEIVEYMD As SqlParameter = SQLTMPcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                PT_OFFICECODE.Value = work.WF_SEL_SALESOFFICECODE.Text
                PT_ORDERYMD.Value = Now.ToString("yyyy/MM/dd")
                PT_INITYMD.Value = Date.Now
                PT_INITUSER.Value = Master.USERID
                PT_INITTERMID.Value = Master.USERTERMID
                PT_UPDYMD.Value = Date.Now
                PT_UPDUSER.Value = Master.USERID
                PT_UPDTERMID.Value = Master.USERTERMID
                PT_RECEIVEYMD.Value = C_DEFAULT_YMD
                SQLTMPcmd.ExecuteNonQuery()

                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar) '営業所コード
                Dim P_ORDERYMD As SqlParameter = SQLcmd.Parameters.Add("@ORDERYMD", SqlDbType.Date)         '登録日(当日)
                Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", System.Data.SqlDbType.DateTime)
                Dim P_INITUSER As SqlParameter = SQLcmd.Parameters.Add("@INITUSER", System.Data.SqlDbType.NVarChar)
                Dim P_INITTERMID As SqlParameter = SQLcmd.Parameters.Add("@INITTERMID", System.Data.SqlDbType.NVarChar)
                Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
                Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
                Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
                Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)
                'Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)     '削除フラグ
                P_OFFICECODE.Value = work.WF_SEL_SALESOFFICECODE.Text
                P_ORDERYMD.Value = Now.ToString("yyyy/MM/dd")
                P_INITYMD.Value = Date.Now
                P_INITUSER.Value = Master.USERID
                P_INITTERMID.Value = Master.USERTERMID
                P_UPDYMD.Value = Date.Now
                P_UPDUSER.Value = Master.USERID
                P_UPDTERMID.Value = Master.USERTERMID
                P_RECEIVEYMD.Value = C_DEFAULT_YMD
                'P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001CMPOrdertbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001CMPOrdertbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim svOrderNo As String = ""
                '★差異がある場合(受注TBLとOT受注TBLの比較)
                For Each OIT0001Cmprow As DataRow In OIT0001CMPOrdertbl.Select("COMPAREINFOCD<>'1'")
                    svOrderNo = Convert.ToString(OIT0001Cmprow("ORDERNO"))
                    OIT0001Cmprow("ORDERNO") = OIT0001Cmprow("KEYCODE1")
                    '★OT受注TBLの比較結果を"不一致"とする。
                    WW_UpdateOTOrderStatus(SQLcon, OIT0001Cmprow, I_ITEM:="CMPRESULTSCODE", I_VALUE:="1")
                    OIT0001Cmprow("ORDERNO") = svOrderNo
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L_ORDERtoOTORDER COMPARE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L_ORDERtoOTORDER COMPARE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************
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

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
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
            SqlConnection.ClearPool(SQLcon)

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001L ORDERHISTORY")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001L ORDERHISTORY"
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
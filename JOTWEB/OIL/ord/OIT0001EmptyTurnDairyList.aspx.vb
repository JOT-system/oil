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
            & " WHERE OIT0002.OFFICECODE   = @P1" _
            & "   AND OIT0002.LODDATE      >= @P2" _
            & "   AND OIT0002.DELFLG       <> @P3" _
            & "   AND OIT0002.ORDERSTATUS  <> @P4" _
            & "   AND OIT0002.EMPTYTURNFLG <> '2'"
        '& "   AND OIT0002.TRAINNO    = @P4"

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
                    & "    AND DELFLG      <> @P02 "

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
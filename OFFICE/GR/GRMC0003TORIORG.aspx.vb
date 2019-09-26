Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 取引先部署マスタ入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMC0003TORIORG
    Inherits Page

    '○ 検索結果格納Table
    Private MC0003tbl As DataTable                          '一覧格納用テーブル
    Private MC0003INPtbl As DataTable                       'チェック用テーブル
    Private MC0003UPDtbl As DataTable                       '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45        '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10         'マウススクロール時稼働行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL              '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD          'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget          '権限チェック(マスタチェック)
    Private CS0026TBLSORT As New CS0026TBLSORT              '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                '帳票出力
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    If Master.RecoverTable(MC0003tbl) Then
                        '○ 画面の情報反映
                        WF_TableChange()
                    Else
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
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
            If Not IsNothing(MC0003tbl) Then
                MC0003tbl.Clear()
                MC0003tbl.Dispose()
                MC0003tbl = Nothing
            End If

            If Not IsNothing(MC0003INPtbl) Then
                MC0003INPtbl.Clear()
                MC0003INPtbl.Dispose()
                MC0003INPtbl = Nothing
            End If

            If Not IsNothing(MC0003UPDtbl) Then
                MC0003UPDtbl.Clear()
                MC0003UPDtbl.Dispose()
                MC0003UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRMC0003WRKINC.MAPID

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()
        rightview.ResetIndex()

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ 右ボックスへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MC0003S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

            '運用部署
            WF_UORG.Text = work.WF_SEL_UORG.Text
            CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_DUMMY)
        End If

        '○ ヘルプボタン非表示
        Master.dispHelp = False

        '○ ファイルドロップ有無
        Master.eventDrop = True

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(MC0003tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(MC0003tbl)

        TBLview.RowFilter = "LINECNT >= 1"

        '○ 一覧表示データ編集(性能対策)
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
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

        If IsNothing(MC0003tbl) Then
            MC0003tbl = New DataTable
        End If

        If MC0003tbl.Columns.Count <> 0 Then
            MC0003tbl.Columns.Clear()
        End If

        MC0003tbl.Clear()

        '○ 検索SQL
        Dim SQLStr As String =
              " SELECT" _
            & "    0                                           AS LINECNT" _
            & "    , ''                                        AS OPERATION" _
            & "    , CAST(ISNULL(MC03.UPDTIMSTP, 0) AS bigint) AS TIMSTP" _
            & "    , 1                                         AS 'SELECT'" _
            & "    , 0                                         AS HIDDEN" _
            & "    , ISNULL(RTRIM(S006.CAMPCODE), '')          AS CAMPCODE" _
            & "    , ''                                        AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(S006.CODE), '')              AS UORG" _
            & "    , ''                                        AS UORGNAMES" _
            & "    , ISNULL(RTRIM(MC02.TORICODE), '')          AS TORICODE" _
            & "    , ISNULL(RTRIM(MC02.NAMES), '')             AS TORINAMES" _
            & "    , ISNULL(RTRIM(MC02.NAMESK), '')            AS TORIKANA" _
            & "    , ISNULL(RTRIM(MC03.STORICODE), '')         AS STORICODE" _
            & "    , ''                                        AS STORINAMES" _
            & "    , ISNULL(RTRIM(MC03.TORITYPE01), '')        AS TORITYPE01" _
            & "    , ''                                        AS TORITYPE01NAMES" _
            & "    , ISNULL(RTRIM(MC03.TORITYPE02), '')        AS TORITYPE02" _
            & "    , ''                                        AS TORITYPE02NAMES" _
            & "    , ISNULL(RTRIM(MC03.TORITYPE03), '')        AS TORITYPE03" _
            & "    , ''                                        AS TORITYPE03NAMES" _
            & "    , ISNULL(RTRIM(MC03.TORITYPE04), '')        AS TORITYPE04" _
            & "    , ''                                        AS TORITYPE04NAMES" _
            & "    , ISNULL(RTRIM(MC03.TORITYPE05), '')        AS TORITYPE05" _
            & "    , ''                                        AS TORITYPE05NAMES" _
            & "    , ISNULL(RTRIM(MC03.YTORICODE), '')         AS YTORICODE" _
            & "    , ISNULL(RTRIM(MC03.KTORICODE), '')         AS KTORICODE" _
            & "    , ISNULL(RTRIM(MC03.SEQ), '')               AS SEQ" _
            & "    , ISNULL(MC03.SEQ, 999999)                  AS SORTSEQ" _
            & "    , ISNULL(RTRIM(MC03.DELFLG), '1')           AS DELFLG" _
            & "    , CASE WHEN MC03.UORG IS NULL THEN '02'" _
            & "                                  ELSE '01' END AS ORGUSE" _
            & " FROM" _
            & "    MC002_TORIHIKISAKI MC02" _
            & "    INNER JOIN S0006_ROLE S006" _
            & "        ON  S006.CAMPCODE = @P1" _
            & "        AND S006.OBJECT   = @P3" _
            & "        AND S006.ROLE     = @P4" _
            & "        AND S006.CODE     = @P2" _
            & "        AND S006.STYMD   <= @P5" _
            & "        AND S006.ENDYMD  >= @P5" _
            & "        AND S006.DELFLG  <> @P7" _
            & "    LEFT JOIN MC003_TORIORG MC03" _
            & "        ON  MC03.CAMPCODE = S006.CAMPCODE" _
            & "        AND MC03.TORICODE = MC02.TORICODE" _
            & "        AND MC03.UORG     = S006.CODE" _
            & "        AND MC03.DELFLG  <> @P7" _
            & " WHERE" _
            & "    MC02.CAMPCODE    = @P1" _
            & "    AND MC02.STYMD  <= @P5" _
            & "    AND MC02.ENDYMD >= @P6" _
            & "    AND MC02.DELFLG <> @P7" _
            & " ORDER BY" _
            & "    S006.CAMPCODE" _
            & "    , MC02.TORICODE"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '運用部署
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        'オブジェクト
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)        'ロール
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '現在日付
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)                '現在日付-1月初日
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = work.WF_SEL_UORG.Text
                PARA3.Value = C_ROLE_VARIANT.USER_ORG
                PARA4.Value = Master.ROLE_ORG
                PARA5.Value = Date.Now
                PARA6.Value = Convert.ToDateTime(Date.Now.AddMonths(-1).ToString("yyyy/MM") & "/01")
                PARA7.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        MC0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    MC0003tbl.Load(SQLdr)
                End Using

                '○ テーブル並び替え
                CS0026TBLSORT.TABLE = MC0003tbl
                CS0026TBLSORT.SORTING = "CAMPCODE, SORTSEQ, TORICODE"
                CS0026TBLSORT.FILTER = ""
                CS0026TBLSORT.sort(MC0003tbl)

                Dim i As Integer = 0
                For Each MC0003row As DataRow In MC0003tbl.Rows
                    i += 1
                    MC0003row("LINECNT") = i        'LINECNT

                    '名称取得
                    CODENAME_get("CAMPCODE", MC0003row("CAMPCODE"), MC0003row("CAMPNAMES"), WW_DUMMY)                   '会社コード
                    CODENAME_get("UORG", MC0003row("UORG"), MC0003row("UORGNAMES"), WW_DUMMY)                           '運用部署
                    CODENAME_get("STORICODE", MC0003row("STORICODE"), MC0003row("STORINAMES"), WW_DUMMY)                '請求先
                    CODENAME_get("TORITYPE01", MC0003row("TORITYPE01"), MC0003row("TORITYPE01NAMES"), WW_DUMMY)         '取引タイプ01
                    CODENAME_get("TORITYPE02", MC0003row("TORITYPE02"), MC0003row("TORITYPE02NAMES"), WW_DUMMY)         '取引タイプ02
                    CODENAME_get("TORITYPE03", MC0003row("TORITYPE03"), MC0003row("TORITYPE03NAMES"), WW_DUMMY)         '取引タイプ03
                    CODENAME_get("TORITYPE04", MC0003row("TORITYPE04"), MC0003row("TORITYPE04NAMES"), WW_DUMMY)         '取引タイプ04
                    CODENAME_get("TORITYPE05", MC0003row("TORITYPE05"), MC0003row("TORITYPE05NAMES"), WW_DUMMY)         '取引タイプ05
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC003_TORIORG SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MC003_TORIORG Select"
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
        For Each MC0003row As DataRow In MC0003tbl.Rows
            If MC0003row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                MC0003row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(MC0003tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = C_DEFAULT_DATAKEY
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
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
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをtblへ退避
        DetailBoxToMC0003tbl()

        '○ 項目チェック
        TableCheck(WW_ERR_SW)

        If isNormal(WW_ERR_SW) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                '取引先部署マスタ更新
                UpdateToriORGMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MC0003tbl)

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMC0003tbl()

        For i As Integer = 0 To MC0003tbl.Rows.Count - 1

            '使用有無
            MC0003tbl.Rows(i)("ORGUSE") = Convert.ToString(Request.Form("ctl00$contents1$rblORGUSEORGUSE" & (i + 1)))
            Select Case MC0003tbl.Rows(i)("ORGUSE")
                Case "01"       '使用
                    If MC0003tbl.Rows(i)("DELFLG") <> C_DELETE_FLG.ALIVE Then
                        MC0003tbl.Rows(i)("DELFLG") = C_DELETE_FLG.ALIVE
                        MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                Case "02"       '未使用
                    If MC0003tbl.Rows(i)("DELFLG") <> C_DELETE_FLG.DELETE Then
                        MC0003tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
                        MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
            End Select

            '請求先
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "STORICODE" & (i + 1))) AndAlso
                MC0003tbl.Rows(i)("STORICODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "STORICODE" & (i + 1))) Then
                MC0003tbl.Rows(i)("STORICODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "STORICODE" & (i + 1)))
                CODENAME_get("STORICODE", MC0003tbl(i)("STORICODE"), MC0003tbl(i)("STORINAMES"), WW_DUMMY)
                MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0003tbl.Rows(i)("STORICODE"))

            '取引タイプ01
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE01" & (i + 1))) AndAlso
                MC0003tbl.Rows(i)("TORITYPE01") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE01" & (i + 1))) Then
                MC0003tbl.Rows(i)("TORITYPE01") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE01" & (i + 1)))
                CODENAME_get("TORITYPE01", MC0003tbl(i)("TORITYPE01"), MC0003tbl(i)("TORITYPE01NAMES"), WW_DUMMY)
                MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0003tbl.Rows(i)("TORITYPE01"))

            '取引タイプ02
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE02" & (i + 1))) AndAlso
                MC0003tbl.Rows(i)("TORITYPE02") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE02" & (i + 1))) Then
                MC0003tbl.Rows(i)("TORITYPE02") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE02" & (i + 1)))
                CODENAME_get("TORITYPE02", MC0003tbl(i)("TORITYPE02"), MC0003tbl(i)("TORITYPE02NAMES"), WW_DUMMY)
                MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0003tbl.Rows(i)("TORITYPE02"))

            '取引タイプ03
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE03" & (i + 1))) AndAlso
                MC0003tbl.Rows(i)("TORITYPE03") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE03" & (i + 1))) Then
                MC0003tbl.Rows(i)("TORITYPE03") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE03" & (i + 1)))
                CODENAME_get("TORITYPE03", MC0003tbl(i)("TORITYPE03"), MC0003tbl(i)("TORITYPE03NAMES"), WW_DUMMY)
                MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0003tbl.Rows(i)("TORITYPE03"))

            '取引タイプ04
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE04" & (i + 1))) AndAlso
                MC0003tbl.Rows(i)("TORITYPE04") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE04" & (i + 1))) Then
                MC0003tbl.Rows(i)("TORITYPE04") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE04" & (i + 1)))
                CODENAME_get("TORITYPE04", MC0003tbl(i)("TORITYPE04"), MC0003tbl(i)("TORITYPE04NAMES"), WW_DUMMY)
                MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0003tbl.Rows(i)("TORITYPE04"))

            '取引タイプ05
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE05" & (i + 1))) AndAlso
                MC0003tbl.Rows(i)("TORITYPE05") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE05" & (i + 1))) Then
                MC0003tbl.Rows(i)("TORITYPE05") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE05" & (i + 1)))
                CODENAME_get("TORITYPE05", MC0003tbl(i)("TORITYPE05"), MC0003tbl(i)("TORITYPE05NAMES"), WW_DUMMY)
                MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0003tbl.Rows(i)("TORITYPE05"))

            '矢崎車端用取引先コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "YTORICODE" & (i + 1))) AndAlso
                MC0003tbl.Rows(i)("YTORICODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YTORICODE" & (i + 1))) Then
                MC0003tbl.Rows(i)("YTORICODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YTORICODE" & (i + 1)))
                MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0003tbl.Rows(i)("YTORICODE"))

            '光英車端用取引先コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "KTORICODE" & (i + 1))) AndAlso
                MC0003tbl.Rows(i)("KTORICODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KTORICODE" & (i + 1))) Then
                MC0003tbl.Rows(i)("KTORICODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KTORICODE" & (i + 1)))
                MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0003tbl.Rows(i)("KTORICODE"))

            'SEQ
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) AndAlso
                MC0003tbl.Rows(i)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) Then
                MC0003tbl.Rows(i)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1)))
                MC0003tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0003tbl.Rows(i)("SEQ"))
        Next

    End Sub

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub TableCheck(ByRef O_RTN As String)

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
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each MC0003row As DataRow In MC0003tbl.Rows

            '変更していない明細は飛ばす
            If MC0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                Continue For
            End If

            WW_LINE_ERR = ""

            '会社コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MC0003row("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MC0003row("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用部署
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UORG", MC0003row("UORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("UORG", MC0003row("UORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '権限チェック
                CS0025AUTHORget.USERID = CS0050SESSION.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                CS0025AUTHORget.CODE = MC0003row("UORG")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(ユーザ部署更新権限なし)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit Sub
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '請求先
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STORICODE", MC0003row("STORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("STORICODE", MC0003row("STORICODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(請求先エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(請求先エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ01
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE01", MC0003row("TORITYPE01"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE01", MC0003row("TORITYPE01"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ01エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ01エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ02
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE02", MC0003row("TORITYPE02"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE02", MC0003row("TORITYPE02"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ02エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ02エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ03
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE03", MC0003row("TORITYPE03"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE03", MC0003row("TORITYPE03"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ03エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ03エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ04
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE04", MC0003row("TORITYPE04"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE04", MC0003row("TORITYPE04"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ04エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ04エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ05
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE05", MC0003row("TORITYPE05"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE05", MC0003row("TORITYPE05"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ05エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ05エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '矢崎車端用取引先コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "YTORICODE", MC0003row("YTORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(矢崎車端用取引先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '光英車端用取引先コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "KTORICODE", MC0003row("KTORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(光英車端用取引先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'SEQ
            WW_TEXT = MC0003row("SEQ")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEQ", MC0003row("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" AndAlso MC0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    MC0003row("SEQ") = ""
                Else
                    Try
                        MC0003row("SEQ") = Format(CInt(MC0003row("SEQ")), "#0")
                    Catch ex As Exception
                        MC0003row("SEQ") = "0"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR <> "" Then
                MC0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 取引先部署マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateToriORGMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        MC003_TORIORG" _
            & "    WHERE" _
            & "        TORICODE     = @P1" _
            & "        AND CAMPCODE = @P2" _
            & "        AND UORG     = @P3 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE MC003_TORIORG" _
            & "    SET" _
            & "        TORITYPE01   = @P4     , TORITYPE02 = @P5" _
            & "        , TORITYPE03 = @P6     , TORITYPE04 = @P7" _
            & "        , TORITYPE05 = @P8     , STORICODE  = @P9" _
            & "        , SEQ        = @P10    , YTORICODE  = @P11" _
            & "        , KTORICODE  = @P12    , DELFLG     = @P13" _
            & "        , UPDYMD     = @P15    , UPDUSER    = @P16" _
            & "        , UPDTERMID  = @P17    , RECEIVEYMD = @P18" _
            & "    WHERE" _
            & "        TORICODE     = @P1" _
            & "        AND CAMPCODE = @P2" _
            & "        AND UORG     = @P3 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO MC003_TORIORG" _
            & "        (TORICODE       , CAMPCODE" _
            & "        , UORG          , TORITYPE01" _
            & "        , TORITYPE02    , TORITYPE03" _
            & "        , TORITYPE04    , TORITYPE05" _
            & "        , STORICODE     , SEQ" _
            & "        , YTORICODE     , KTORICODE" _
            & "        , DELFLG        , INITYMD" _
            & "        , UPDYMD        , UPDUSER" _
            & "        , UPDTERMID     , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2" _
            & "        , @P3     , @P4" _
            & "        , @P5     , @P6" _
            & "        , @P7     , @P8" _
            & "        , @P9     , @P10" _
            & "        , @P11    , @P12" _
            & "        , @P13    , @P14" _
            & "        , @P15    , @P16" _
            & "        , @P17    , @P18) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    TORICODE" _
            & "    , CAMPCODE" _
            & "    , UORG" _
            & "    , TORITYPE01" _
            & "    , TORITYPE02" _
            & "    , TORITYPE03" _
            & "    , TORITYPE04" _
            & "    , TORITYPE05" _
            & "    , STORICODE" _
            & "    , SEQ" _
            & "    , YTORICODE" _
            & "    , KTORICODE" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) TIMSTP" _
            & " FROM" _
            & "    MC003_TORIORG" _
            & " WHERE" _
            & "    TORICODE     = @P1" _
            & "    AND CAMPCODE = @P2" _
            & "    AND UORG     = @P3"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '取引先コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)            '運用部署
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 3)             '取引タイプ01
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 3)             '取引タイプ02
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 3)             '取引タイプ03
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 3)             '取引タイプ04
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 3)             '取引タイプ05
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)            '請求取引先コード
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.Int)                   '表示順番
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 6)           '矢崎車端用取引先コード
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 6)           '光英車端用取引先コード
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)              '登録年月日
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.DateTime)              '更新年月日
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '取引先コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        '運用部署

                For Each MC0003row As DataRow In MC0003tbl.Rows
                    If Trim(MC0003row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING Then

                        '新規分で削除のレコードは作成しない
                        If MC0003row("TIMSTP") = 0 AndAlso MC0003row("DELFLG") = C_DELETE_FLG.DELETE Then
                            MC0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Continue For
                        End If

                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = MC0003row("TORICODE")
                        PARA2.Value = MC0003row("CAMPCODE")
                        PARA3.Value = MC0003row("UORG")
                        PARA4.Value = MC0003row("TORITYPE01")
                        PARA5.Value = MC0003row("TORITYPE02")
                        PARA6.Value = MC0003row("TORITYPE03")
                        PARA7.Value = MC0003row("TORITYPE04")
                        PARA8.Value = MC0003row("TORITYPE05")
                        PARA9.Value = MC0003row("STORICODE")
                        PARA10.Value = MC0003row("SEQ")
                        PARA11.Value = MC0003row("YTORICODE")
                        PARA12.Value = MC0003row("KTORICODE")
                        PARA13.Value = MC0003row("DELFLG")
                        PARA14.Value = WW_DATENOW
                        PARA15.Value = WW_DATENOW
                        PARA16.Value = Master.USERID
                        PARA17.Value = Master.USERTERMID
                        PARA18.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        MC0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = MC0003row("TORICODE")
                        JPARA2.Value = MC0003row("CAMPCODE")
                        JPARA3.Value = MC0003row("UORG")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(MC0003UPDtbl) Then
                                MC0003UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    MC0003UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            MC0003UPDtbl.Clear()
                            MC0003UPDtbl.Load(SQLdr)
                        End Using

                        For Each MC0003UPDrow As DataRow In MC0003UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "MC003_TORIORG"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MC0003UPDrow
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
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC003_TORIORG UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MC003_TORIORG UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        CS0030REPORT.TBLDATA = MC0003tbl                        'データ参照  Table
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
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

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
        Master.CreateEmptyTable(MC0003INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim MC0003INProw As DataRow = MC0003INPtbl.NewRow

            '○ 初期クリア
            For Each MC0003INPcol As DataColumn In MC0003INPtbl.Columns
                If IsDBNull(MC0003INProw.Item(MC0003INPcol)) OrElse IsNothing(MC0003INProw.Item(MC0003INPcol)) Then
                    Select Case MC0003INPcol.ColumnName
                        Case "LINECNT"
                            MC0003INProw.Item(MC0003INPcol) = 0
                        Case "OPERATION"
                            MC0003INProw.Item(MC0003INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MC0003INProw.Item(MC0003INPcol) = 0
                        Case "SELECT"
                            MC0003INProw.Item(MC0003INPcol) = 1
                        Case "HIDDEN"
                            MC0003INProw.Item(MC0003INPcol) = 0
                        Case "SORTSEQ"
                            MC0003INProw.Item(MC0003INPcol) = 0
                        Case Else
                            MC0003INProw.Item(MC0003INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("UORG") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                For Each MC0003row As DataRow In MC0003tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MC0003row("CAMPCODE") AndAlso
                        XLSTBLrow("UORG") = MC0003row("UORG") AndAlso
                        XLSTBLrow("TORICODE") = MC0003row("TORICODE") Then
                        MC0003INProw.ItemArray = MC0003row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MC0003INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '運用部署
            If WW_COLUMNS.IndexOf("UORG") >= 0 Then
                MC0003INProw("UORG") = XLSTBLrow("UORG")
            End If

            '取引先コード
            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                MC0003INProw("TORICODE") = XLSTBLrow("TORICODE")
            End If

            '請求先
            If WW_COLUMNS.IndexOf("STORICODE") >= 0 Then
                MC0003INProw("STORICODE") = XLSTBLrow("STORICODE")
            End If

            '取引タイプ01
            If WW_COLUMNS.IndexOf("TORITYPE01") >= 0 Then
                MC0003INProw("TORITYPE01") = XLSTBLrow("TORITYPE01")
            End If

            '取引タイプ02
            If WW_COLUMNS.IndexOf("TORITYPE02") >= 0 Then
                MC0003INProw("TORITYPE02") = XLSTBLrow("TORITYPE02")
            End If

            '取引タイプ03
            If WW_COLUMNS.IndexOf("TORITYPE03") >= 0 Then
                MC0003INProw("TORITYPE03") = XLSTBLrow("TORITYPE03")
            End If

            '取引タイプ04
            If WW_COLUMNS.IndexOf("TORITYPE04") >= 0 Then
                MC0003INProw("TORITYPE04") = XLSTBLrow("TORITYPE04")
            End If

            '取引タイプ05
            If WW_COLUMNS.IndexOf("TORITYPE05") >= 0 Then
                MC0003INProw("TORITYPE05") = XLSTBLrow("TORITYPE05")
            End If

            '矢崎車端用取引先コード
            If WW_COLUMNS.IndexOf("YTORICODE") >= 0 Then
                MC0003INProw("YTORICODE") = XLSTBLrow("YTORICODE")
            End If

            '光英車端用取引先コード
            If WW_COLUMNS.IndexOf("KTORICODE") >= 0 Then
                MC0003INProw("KTORICODE") = XLSTBLrow("KTORICODE")
            End If

            '表示順番
            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                MC0003INProw("SEQ") = XLSTBLrow("SEQ")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MC0003INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '名称取得
            CODENAME_get("STORICODE", MC0003INProw("STORICODE"), MC0003INProw("STORINAMES"), WW_DUMMY)              '請求先
            CODENAME_get("TORITYPE01", MC0003INProw("TORITYPE01"), MC0003INProw("TORITYPE01NAMES"), WW_DUMMY)       '取引タイプ01
            CODENAME_get("TORITYPE02", MC0003INProw("TORITYPE02"), MC0003INProw("TORITYPE02NAMES"), WW_DUMMY)       '取引タイプ02
            CODENAME_get("TORITYPE03", MC0003INProw("TORITYPE03"), MC0003INProw("TORITYPE03NAMES"), WW_DUMMY)       '取引タイプ03
            CODENAME_get("TORITYPE04", MC0003INProw("TORITYPE04"), MC0003INProw("TORITYPE04NAMES"), WW_DUMMY)       '取引タイプ04
            CODENAME_get("TORITYPE05", MC0003INProw("TORITYPE05"), MC0003INProw("TORITYPE05NAMES"), WW_DUMMY)       '取引タイプ05

            MC0003INPtbl.Rows.Add(MC0003INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        MC0003tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(MC0003tbl)

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

        '○ 単項目チェック
        For Each MC0003INProw As DataRow In MC0003INPtbl.Rows

            WW_LINE_ERR = ""

            '会社コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MC0003INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MC0003INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_CAMPCODE.Text <> MC0003INProw("CAMPCODE") Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "検索条件の会社コードと一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用部署
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UORG", MC0003INProw("UORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("UORG", MC0003INProw("UORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_CAMPCODE.Text <> MC0003INProw("CAMPCODE") Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "検索条件の運用部署と一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORICODE", MC0003INProw("TORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORICODE", MC0003INProw("TORICODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '請求先
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STORICODE", MC0003INProw("STORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("STORICODE", MC0003INProw("STORICODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(請求先エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(請求先エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ01
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE01", MC0003INProw("TORITYPE01"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE01", MC0003INProw("TORITYPE01"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ01エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ01エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ02
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE02", MC0003INProw("TORITYPE02"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE02", MC0003INProw("TORITYPE02"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ02エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ02エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ03
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE03", MC0003INProw("TORITYPE03"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE03", MC0003INProw("TORITYPE03"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ03エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ03エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ04
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE04", MC0003INProw("TORITYPE04"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE04", MC0003INProw("TORITYPE04"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ04エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ04エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引タイプ05
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORITYPE05", MC0003INProw("TORITYPE05"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORITYPE05", MC0003INProw("TORITYPE05"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引タイプ05エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引タイプ05エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '矢崎車端用取引先コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "YTORICODE", MC0003INProw("YTORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(矢崎車端用取引先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '光英車端用取引先コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "KTORICODE", MC0003INProw("KTORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(光英車端用取引先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'SEQ
            WW_TEXT = MC0003INProw("SEQ")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEQ", MC0003INProw("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MC0003INProw("SEQ") = ""
                Else
                    Try
                        MC0003INProw("SEQ") = Format(CInt(MC0003INProw("SEQ")), "#0")
                    Catch ex As Exception
                        MC0003INProw("SEQ") = "0"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(表示順番)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", MC0003INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", MC0003INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If MC0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MC0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MC0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If

        Next

    End Sub

    ''' <summary>
    ''' MC0003tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MC0003tbl_UPD()

        '○ 追加変更判定
        For Each MC0003INProw As DataRow In MC0003INPtbl.Rows

            'エラーレコード読み飛ばし
            If MC0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            MC0003INProw("OPERATION") = "Insert"

            'KEY項目が等しい
            For Each MC0003row As DataRow In MC0003tbl.Rows
                If MC0003row("CAMPCODE") = MC0003INProw("CAMPCODE") AndAlso
                    MC0003row("UORG") = MC0003INProw("UORG") AndAlso
                    MC0003row("TORICODE") = MC0003INProw("TORICODE") Then

                    '変更無は操作無
                    If MC0003row("STORICODE") = MC0003INProw("STORICODE") AndAlso
                        MC0003row("TORITYPE01") = MC0003INProw("TORITYPE01") AndAlso
                        MC0003row("TORITYPE02") = MC0003INProw("TORITYPE02") AndAlso
                        MC0003row("TORITYPE03") = MC0003INProw("TORITYPE03") AndAlso
                        MC0003row("TORITYPE04") = MC0003INProw("TORITYPE04") AndAlso
                        MC0003row("TORITYPE05") = MC0003INProw("TORITYPE05") AndAlso
                        MC0003row("YTORICODE") = MC0003INProw("YTORICODE") AndAlso
                        MC0003row("KTORICODE") = MC0003INProw("KTORICODE") AndAlso
                        MC0003row("SEQ") = MC0003INProw("SEQ") AndAlso
                        MC0003row("DELFLG") = MC0003INProw("DELFLG") Then
                        MC0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    MC0003INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each MC0003INProw As DataRow In MC0003INPtbl.Rows
            Select Case MC0003INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(MC0003INProw)
                Case "Insert"
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="MC0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef MC0003INProw As DataRow)

        For Each MC0003row As DataRow In MC0003tbl.Rows

            '同一レコード
            If MC0003INProw("CAMPCODE") = MC0003row("CAMPCODE") AndAlso
                MC0003INProw("UORG") = MC0003row("UORG") AndAlso
                MC0003INProw("TORICODE") = MC0003row("TORICODE") Then

                '画面入力テーブル項目設定
                MC0003INProw("LINECNT") = MC0003row("LINECNT")
                MC0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MC0003INProw("TIMSTP") = MC0003row("TIMSTP")
                MC0003INProw("SELECT") = 1
                MC0003INProw("HIDDEN") = 0

                '使用有無判定
                If MC0003INProw("DELFLG") = C_DELETE_FLG.ALIVE Then
                    MC0003INProw("ORGUSE") = "01"       '使用
                Else
                    MC0003INProw("ORGUSE") = "02"       '未使用
                End If

                '項目テーブル項目設定
                MC0003row.ItemArray = MC0003INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 一覧変更情報取込処理
    ''' </summary>
    Protected Sub WF_TableChange()

        For Each MC0003row As DataRow In MC0003tbl.Rows
            WF_SelectedIndex.Value = CStr(MC0003row("LINECNT"))
            WF_ListChange(False)
        Next

        '○ 画面表示データ保存
        Master.SaveTable(MC0003tbl)

    End Sub

    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()

        WF_ListChange(True)

    End Sub
    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <param name="isSaving">更新保存可否</param>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange(ByVal isSaving As Boolean)

        Dim WW_LINECNT As Integer = 0

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_SelectedIndex.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○ 変更チェック
        '使用有無
        If MC0003tbl.Rows(WW_LINECNT)("ORGUSE") <> Convert.ToString(Request.Form("ctl00$contents1$rblORGUSEORGUSE" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("ORGUSE") = Convert.ToString(Request.Form("ctl00$contents1$rblORGUSEORGUSE" & WF_SelectedIndex.Value))
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '請求先
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "STORICODE" & WF_SelectedIndex.Value)) AndAlso
            MC0003tbl.Rows(WW_LINECNT)("STORICODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "STORICODE" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("STORICODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "STORICODE" & WF_SelectedIndex.Value))
            CODENAME_get("STORICODE", MC0003tbl(WW_LINECNT)("STORICODE"), MC0003tbl(WW_LINECNT)("STORINAMES"), WW_DUMMY)
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '取引タイプ01
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE01" & WF_SelectedIndex.Value)) AndAlso
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE01") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE01" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE01") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE01" & WF_SelectedIndex.Value))
            CODENAME_get("TORITYPE01", MC0003tbl(WW_LINECNT)("TORITYPE01"), MC0003tbl(WW_LINECNT)("TORITYPE01NAMES"), WW_DUMMY)
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '取引タイプ02
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE02" & WF_SelectedIndex.Value)) AndAlso
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE02") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE02" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE02") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE02" & WF_SelectedIndex.Value))
            CODENAME_get("TORITYPE02", MC0003tbl(WW_LINECNT)("TORITYPE02"), MC0003tbl(WW_LINECNT)("TORITYPE02NAMES"), WW_DUMMY)
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '取引タイプ03
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE03" & WF_SelectedIndex.Value)) AndAlso
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE03") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE03" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE03") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE03" & WF_SelectedIndex.Value))
            CODENAME_get("TORITYPE03", MC0003tbl(WW_LINECNT)("TORITYPE03"), MC0003tbl(WW_LINECNT)("TORITYPE03NAMES"), WW_DUMMY)
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '取引タイプ04
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE04" & WF_SelectedIndex.Value)) AndAlso
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE04") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE04" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE04") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE04" & WF_SelectedIndex.Value))
            CODENAME_get("TORITYPE04", MC0003tbl(WW_LINECNT)("TORITYPE04"), MC0003tbl(WW_LINECNT)("TORITYPE04NAMES"), WW_DUMMY)
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '取引タイプ05
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TORITYPE05" & WF_SelectedIndex.Value)) AndAlso
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE05") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE05" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("TORITYPE05") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TORITYPE05" & WF_SelectedIndex.Value))
            CODENAME_get("TORITYPE05", MC0003tbl(WW_LINECNT)("TORITYPE05"), MC0003tbl(WW_LINECNT)("TORITYPE05NAMES"), WW_DUMMY)
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '矢崎車端用取引先コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "YTORICODE" & WF_SelectedIndex.Value)) AndAlso
            MC0003tbl.Rows(WW_LINECNT)("YTORICODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YTORICODE" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("YTORICODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YTORICODE" & WF_SelectedIndex.Value))
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '光英車端用取引先コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "KTORICODE" & WF_SelectedIndex.Value)) AndAlso
            MC0003tbl.Rows(WW_LINECNT)("KTORICODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KTORICODE" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("KTORICODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KTORICODE" & WF_SelectedIndex.Value))
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        'SEQ
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) AndAlso
            MC0003tbl.Rows(WW_LINECNT)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) Then
            MC0003tbl.Rows(WW_LINECNT)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value))
            MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '○ 画面表示データ保存
        If isSaving Then Master.SaveTable(MC0003tbl)

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
                Dim prmData As New Hashtable
                prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                Select Case WF_FIELD.Value
                    Case "STORICODE"        '請求先
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "STORICODE"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                    Case "TORITYPE01"       '取引タイプ01
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE01"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                    Case "TORITYPE02"       '取引タイプ02
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE02"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                    Case "TORITYPE03"       '取引タイプ03
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE03"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                    Case "TORITYPE04"       '取引タイプ04
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE04"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                    Case "TORITYPE05"       '取引タイプ05
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE05"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            End With
        End If

    End Sub


    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_LINECNT As Integer = 0
        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.GetActiveValue) Then
            WW_SelectValue = leftview.GetActiveValue(0)
            WW_SelectText = leftview.GetActiveValue(1)
        End If

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_SelectLine.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "STORICODE"        '請求先
                If MC0003tbl.Rows(WW_LINECNT)("STORICODE") <> WW_SelectValue Then
                    MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MC0003tbl.Rows(WW_LINECNT)("STORICODE") = WW_SelectValue
                MC0003tbl.Rows(WW_LINECNT)("STORINAMES") = WW_SelectText

            Case "TORITYPE01"       '取引タイプ01
                If MC0003tbl.Rows(WW_LINECNT)("TORITYPE01") <> WW_SelectValue Then
                    MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE01") = WW_SelectValue
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE01NAMES") = WW_SelectText

            Case "TORITYPE02"       '取引タイプ02
                If MC0003tbl.Rows(WW_LINECNT)("TORITYPE02") <> WW_SelectValue Then
                    MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE02") = WW_SelectValue
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE02NAMES") = WW_SelectText

            Case "TORITYPE03"       '取引タイプ03
                If MC0003tbl.Rows(WW_LINECNT)("TORITYPE03") <> WW_SelectValue Then
                    MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE03") = WW_SelectValue
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE03NAMES") = WW_SelectText

            Case "TORITYPE04"       '取引タイプ04
                If MC0003tbl.Rows(WW_LINECNT)("TORITYPE04") <> WW_SelectValue Then
                    MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE04") = WW_SelectValue
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE04NAMES") = WW_SelectText

            Case "TORITYPE05"       '取引タイプ05
                If MC0003tbl.Rows(WW_LINECNT)("TORITYPE05") <> WW_SelectValue Then
                    MC0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE05") = WW_SelectValue
                MC0003tbl.Rows(WW_LINECNT)("TORITYPE05NAMES") = WW_SelectText
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(MC0003tbl)

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "STORICODE"        '請求先
            Case "TORITYPE01"       '取引タイプ01
            Case "TORITYPE02"       '取引タイプ02
            Case "TORITYPE03"       '取引タイプ03
            Case "TORITYPE04"       '取引タイプ04
            Case "TORITYPE05"       '取引タイプ05
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="MC0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal MC0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(MC0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社       =" & MC0003row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用部署   =" & MC0003row("UORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先     =" & MC0003row("TORICODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先名称 =" & MC0003row("TORINAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順番   =" & MC0003row("SEQ")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORICODE"         '取引先
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STORICODE"        '請求先
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "STORICODE"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORITYPE01"       '取引タイプ01
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE01"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORITYPE02"       '取引タイプ02
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE02"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORITYPE03"       '取引タイプ03
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE03"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORITYPE04"       '取引タイプ04
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE04"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORITYPE05"       '取引タイプ05
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "TORITYPE05"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

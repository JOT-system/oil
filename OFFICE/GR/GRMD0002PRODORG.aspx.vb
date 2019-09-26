Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 品名部署マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMD0002PRODORG
    Inherits Page

    '○ 検索結果格納Table
    Private MD0002tbl As DataTable                          '一覧格納用テーブル
    Private MD0002INPtbl As DataTable                       'チェック用テーブル
    Private MD0002UPDtbl As DataTable                       '更新用テーブル

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
                    If Master.RecoverTable(MD0002tbl) Then
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
            If Not IsNothing(MD0002tbl) Then
                MD0002tbl.Clear()
                MD0002tbl.Dispose()
                MD0002tbl = Nothing
            End If

            If Not IsNothing(MD0002INPtbl) Then
                MD0002INPtbl.Clear()
                MD0002INPtbl.Dispose()
                MD0002INPtbl = Nothing
            End If

            If Not IsNothing(MD0002UPDtbl) Then
                MD0002UPDtbl.Clear()
                MD0002UPDtbl.Dispose()
                MD0002UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRMD0002WRKINC.MAPID

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MD0002S Then
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
        Master.SaveTable(MD0002tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(MD0002tbl)

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

        If IsNothing(MD0002tbl) Then
            MD0002tbl = New DataTable
        End If

        If MD0002tbl.Columns.Count <> 0 Then
            MD0002tbl.Columns.Clear()
        End If

        MD0002tbl.Clear()

        '○ 検索SQL
        Dim SQLStr As String =
              " SELECT" _
            & "    0                                               AS LINECNT" _
            & "    , ''                                            AS OPERATION" _
            & "    , CAST(ISNULL(MD02.UPDTIMSTP, 0) AS bigint)     AS TIMSTP" _
            & "    , 1                                             AS 'SELECT'" _
            & "    , 0                                             AS HIDDEN" _
            & "    , ISNULL(RTRIM(S006.CAMPCODE), '')              AS CAMPCODE" _
            & "    , ''                                            AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(S006.CODE), '')                  AS UORG" _
            & "    , ''                                            AS UORGNAMES" _
            & "    , ISNULL(RTRIM(MD01.PRODUCTCODE), '')           AS PRODUCTCODE" _
            & "    , ISNULL(RTRIM(MD01.OILTYPE), '')               AS OILTYPE" _
            & "    , ''                                            AS OILTYPENAMES" _
            & "    , ISNULL(RTRIM(MD01.PRODUCT1), '')              AS PRODUCT1" _
            & "    , ''                                            AS PRODUCT1NAMES" _
            & "    , ISNULL(RTRIM(MD01.PRODUCT2), '')              AS PRODUCT2" _
            & "    , ISNULL(RTRIM(MD01.NAMES), '')                 AS PRODUCT2NAMES" _
            & "    , ISNULL(RTRIM(MD01.STANI), '')                 AS STANI" _
            & "    , ''                                            AS STANINAMES" _
            & "    , ISNULL(RTRIM(MD02.HTANI), '')                 AS HTANI" _
            & "    , ''                                            AS HTANINAMES" _
            & "    , ISNULL(RTRIM(MD02.YPRODUCT), '')              AS YPRODUCT" _
            & "    , ISNULL(RTRIM(MD02.KPRODUCT), '')              AS KPRODUCT" _
            & "    , ISNULL(RTRIM(MD02.JSRPRODUCT), '')            AS JSRPRODUCT" _
            & "    , ISNULL(RTRIM(MD02.SEQ), '')                   AS SEQ" _
            & "    , ISNULL(MD02.SEQ, 99999)                       AS SORTSEQ" _
            & "    , ISNULL(FORMAT(MD01.STYMD, 'yyyy/MM/dd'), '')  AS STYMD" _
            & "    , ISNULL(FORMAT(MD01.ENDYMD, 'yyyy/MM/dd'), '') AS ENDYMD" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE01), '')            AS PRODTYPE01" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE02), '')            AS PRODTYPE02" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE03), '')            AS PRODTYPE03" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE04), '')            AS PRODTYPE04" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE05), '')            AS PRODTYPE05" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE06), '')            AS PRODTYPE06" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE07), '')            AS PRODTYPE07" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE08), '')            AS PRODTYPE08" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE09), '')            AS PRODTYPE09" _
            & "    , ISNULL(RTRIM(MD02.PRODTYPE10), '')            AS PRODTYPE10" _
            & "    , ISNULL(RTRIM(MD02.UNLOADADDTANKA), '0')       AS UNLOADADDTANKA" _
            & "    , ISNULL(RTRIM(MD02.LOADINGTANKA), '0')         AS LOADINGTANKA" _
            & "    , ISNULL(RTRIM(MD02.DELFLG), '1')               AS DELFLG" _
            & "    , CASE WHEN MD02.UORG IS NULL THEN '02'" _
            & "                                  ELSE '01' END     AS ORGUSE" _
            & " FROM" _
            & "    MD001_PRODUCT MD01" _
            & "    INNER JOIN S0006_ROLE S006" _
            & "        ON  S006.CAMPCODE    = @P1" _
            & "        AND S006.CODE        = @P2" _
            & "        AND S006.OBJECT      = @P3" _
            & "        AND S006.ROLE        = @P4" _
            & "        AND S006.STYMD      <= @P5" _
            & "        AND S006.ENDYMD     >= @P5" _
            & "        AND S006.DELFLG     <> @P7" _
            & "    LEFT JOIN MD002_PRODORG MD02" _
            & "        ON  MD02.CAMPCODE    = S006.CAMPCODE" _
            & "        AND MD02.UORG        = S006.CODE" _
            & "        AND MD02.PRODUCTCODE = MD01.PRODUCTCODE" _
            & "        AND MD02.STYMD      <= @P5" _
            & "        AND MD02.ENDYMD     >= @P5" _
            & "        AND MD02.DELFLG     <> @P7" _
            & " WHERE" _
            & "    MD01.CAMPCODE    = @P1" _
            & "    AND MD01.STYMD  <= @P5" _
            & "    AND MD01.ENDYMD >= @P6" _
            & "    AND MD01.DELFLG <> @P7" _
            & " ORDER BY" _
            & "    MD01.PRODUCTCODE"

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
                        MD0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    MD0002tbl.Load(SQLdr)
                End Using

                '○ テーブル並び替え
                CS0026TBLSORT.TABLE = MD0002tbl
                CS0026TBLSORT.SORTING = "SORTSEQ, PRODUCTCODE"
                CS0026TBLSORT.FILTER = ""
                CS0026TBLSORT.sort(MD0002tbl)

                Dim i As Integer = 0
                For Each MD0002row As DataRow In MD0002tbl.Rows
                    i += 1
                    MD0002row("LINECNT") = i        'LINECNT

                    '名称取得
                    CODENAME_get("CAMPCODE", MD0002row("CAMPCODE"), MD0002row("CAMPNAMES"), WW_DUMMY)                                   '会社コード
                    CODENAME_get("UORG", MD0002row("UORG"), MD0002row("UORGNAMES"), WW_DUMMY)                                           '運用部署
                    CODENAME_get("OILTYPE", MD0002row("OILTYPE"), MD0002row("OILTYPENAMES"), WW_DUMMY)                                  '油種
                    CODENAME_get("PRODUCT1", MD0002row("PRODUCT1"), MD0002row("PRODUCT1NAMES"), WW_DUMMY, MD0002row("OILTYPE"))         '品名１
                    CODENAME_get("STANI", MD0002row("STANI"), MD0002row("STANINAMES"), WW_DUMMY)                                        '請求単位
                    CODENAME_get("HTANI", MD0002row("HTANI"), MD0002row("HTANINAMES"), WW_DUMMY)                                        '配送単位
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MD002_PRODORG SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MD002_PRODORG Select"
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
        For Each MD0002row As DataRow In MD0002tbl.Rows
            If MD0002row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                MD0002row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(MD0002tbl)

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
        DetailBoxToMD0002tbl()

        '○ 項目チェック
        TableCheck(WW_ERR_SW)

        If isNormal(WW_ERR_SW) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                '品名部署マスタ更新
                UpdateProductORGMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MD0002tbl)

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMD0002tbl()

        For i As Integer = 0 To MD0002tbl.Rows.Count - 1

            '使用有無
            MD0002tbl.Rows(i)("ORGUSE") = Convert.ToString(Request.Form("ctl00$contents1$rblORGUSEORGUSE" & (i + 1)))
            Select Case MD0002tbl.Rows(i)("ORGUSE")
                Case "01"       '使用
                    If MD0002tbl.Rows(i)("DELFLG") <> C_DELETE_FLG.ALIVE Then
                        MD0002tbl.Rows(i)("DELFLG") = C_DELETE_FLG.ALIVE
                        MD0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                Case "02"       '未使用
                    If MD0002tbl.Rows(i)("DELFLG") <> C_DELETE_FLG.DELETE Then
                        MD0002tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
                        MD0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
            End Select

            '配送単位
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "HTANI" & (i + 1))) AndAlso
                MD0002tbl.Rows(i)("HTANI") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "HTANI" & (i + 1))) Then
                MD0002tbl.Rows(i)("HTANI") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "HTANI" & (i + 1)))
                CODENAME_get("HTANI", MD0002tbl(i)("HTANI"), MD0002tbl(i)("HTANINAMES"), WW_DUMMY)
                MD0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MD0002tbl.Rows(i)("HTANI"))

            '矢崎車端用品名コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "YPRODUCT" & (i + 1))) AndAlso
                MD0002tbl.Rows(i)("YPRODUCT") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YPRODUCT" & (i + 1))) Then
                MD0002tbl.Rows(i)("YPRODUCT") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YPRODUCT" & (i + 1)))
                MD0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MD0002tbl.Rows(i)("YPRODUCT"))

            '光英車端用品名コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "KPRODUCT" & (i + 1))) AndAlso
                MD0002tbl.Rows(i)("KPRODUCT") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KPRODUCT" & (i + 1))) Then
                MD0002tbl.Rows(i)("KPRODUCT") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KPRODUCT" & (i + 1)))
                MD0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MD0002tbl.Rows(i)("KPRODUCT"))

            'JSR品名コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "JSRPRODUCT" & (i + 1))) AndAlso
                MD0002tbl.Rows(i)("JSRPRODUCT") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRPRODUCT" & (i + 1))) Then
                MD0002tbl.Rows(i)("JSRPRODUCT") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRPRODUCT" & (i + 1)))
                MD0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MD0002tbl.Rows(i)("JSRPRODUCT"))

            '荷卸時加算単価
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "UNLOADADDTANKA" & (i + 1))) AndAlso
                MD0002tbl.Rows(i)("UNLOADADDTANKA") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "UNLOADADDTANKA" & (i + 1))) Then
                MD0002tbl.Rows(i)("UNLOADADDTANKA") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "UNLOADADDTANKA" & (i + 1)))
                MD0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MD0002tbl.Rows(i)("UNLOADADDTANKA"))

            '積込単価
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "LOADINGTANKA" & (i + 1))) AndAlso
                MD0002tbl.Rows(i)("LOADINGTANKA") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "LOADINGTANKA" & (i + 1))) Then
                MD0002tbl.Rows(i)("LOADINGTANKA") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "LOADINGTANKA" & (i + 1)))
                MD0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MD0002tbl.Rows(i)("LOADINGTANKA"))

            'SEQ
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) AndAlso
                MD0002tbl.Rows(i)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) Then
                MD0002tbl.Rows(i)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1)))
                MD0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MD0002tbl.Rows(i)("SEQ"))
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
        For Each MD0002row As DataRow In MD0002tbl.Rows

            '変更していない明細は飛ばす
            If MD0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                Continue For
            End If

            WW_LINE_ERR = ""

            '会社コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MD0002row("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MD0002row("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用部署
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UORG", MD0002row("UORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("UORG", MD0002row("UORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '権限チェック
                CS0025AUTHORget.USERID = CS0050SESSION.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                CS0025AUTHORget.CODE = MD0002row("UORG")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(ユーザ部署更新権限なし)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit Sub
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '配送単位
            If IsNothing(MD0002row("HTANI")) Then
                MD0002row("HTANI") = MD0002row("STANI")
            End If

            WW_TEXT = MD0002row("HTANI")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "HTANI", MD0002row("HTANI"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MD0002row("HTANI") = ""
                Else
                    '存在チェック
                    CODENAME_get("HTANI", MD0002row("HTANI"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(配送単位エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(配送単位エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '矢崎車端用品名コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "YPRODUCT", MD0002row("YPRODUCT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(矢崎車端用品名コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '光英車端用品名コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "KPRODUCT", MD0002row("KPRODUCT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(光英車端用品名コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JSR品名コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JSRPRODUCT", MD0002row("JSRPRODUCT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JSR品名コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷卸時加算単価
            WW_TEXT = MD0002row("UNLOADADDTANKA")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UNLOADADDTANKA", MD0002row("UNLOADADDTANKA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" OrElse WW_TEXT = "0" Then
                    MD0002row("UNLOADADDTANKA") = "0"
                Else
                    Try
                        MD0002row("UNLOADADDTANKA") = Format(CInt(MD0002row("UNLOADADDTANKA")), "#0")
                    Catch ex As Exception
                        MD0002row("UNLOADADDTANKA") = "0"
                    End Try

                    '存在チェック
                    CODENAME_get("UNLOADADDTANKA", MD0002row("UNLOADADDTANKA"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(荷卸時加算単価エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷卸時加算単価エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込単価
            WW_TEXT = MD0002row("LOADINGTANKA")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LOADINGTANKA", MD0002row("LOADINGTANKA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" OrElse WW_TEXT = "0" Then
                    MD0002row("LOADINGTANKA") = "0"
                Else
                    Try
                        MD0002row("LOADINGTANKA") = Format(CInt(MD0002row("LOADINGTANKA")), "#0")
                    Catch ex As Exception
                        MD0002row("LOADINGTANKA") = "0"
                    End Try

                    '存在チェック
                    CODENAME_get("LOADINGTANKA", MD0002row("LOADINGTANKA"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(積込単価エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(積込単価エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'SEQ
            WW_TEXT = MD0002row("SEQ")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEQ", MD0002row("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" AndAlso MD0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    MD0002row("SEQ") = ""
                Else
                    Try
                        MD0002row("SEQ") = Format(CInt(MD0002row("SEQ")), "#0")
                    Catch ex As Exception
                        MD0002row("SEQ") = "0"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR <> "" Then
                MD0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 品名部署マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateProductORGMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        MD002_PRODORG" _
            & "    WHERE" _
            & "        CAMPCODE        = @P1" _
            & "        AND UORG        = @P2" _
            & "        AND PRODUCTCODE = @P3" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE MD002_PRODORG" _
            & "    SET" _
            & "        SEQ            = @P4     , STYMD          = @P5" _
            & "        , ENDYMD       = @P6     , HTANI          = @P17" _
            & "        , YPRODUCT     = @P18    , KPRODUCT       = @P19" _
            & "        , JSRPRODUCT   = @P20    , UNLOADADDTANKA = @P21" _
            & "        , LOADINGTANKA = @P22    , DELFLG         = @P23" _
            & "        , UPDYMD       = @P25    , UPDUSER        = @P26" _
            & "        , UPDTERMID    = @P27    , RECEIVEYMD     = @P28" _
            & "    WHERE" _
            & "        CAMPCODE        = @P1" _
            & "        AND UORG        = @P2" _
            & "        AND PRODUCTCODE = @P3" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO MD002_PRODORG" _
            & "        (CAMPCODE           , UORG" _
            & "        , PRODUCTCODE       , SEQ" _
            & "        , STYMD             , ENDYMD" _
            & "        , PRODTYPE01        , PRODTYPE02" _
            & "        , PRODTYPE03        , PRODTYPE04" _
            & "        , PRODTYPE05        , PRODTYPE06" _
            & "        , PRODTYPE07        , PRODTYPE08" _
            & "        , PRODTYPE09        , PRODTYPE10" _
            & "        , HTANI             , YPRODUCT" _
            & "        , KPRODUCT          , JSRPRODUCT" _
            & "        , UNLOADADDTANKA    , LOADINGTANKA" _
            & "        , DELFLG            , INITYMD" _
            & "        , UPDYMD            , UPDUSER" _
            & "        , UPDTERMID         , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2" _
            & "        , @P3     , @P4" _
            & "        , @P5     , @P6" _
            & "        , @P7     , @P8" _
            & "        , @P9     , @P10" _
            & "        , @P11    , @P12" _
            & "        , @P13    , @P14" _
            & "        , @P15    , @P16" _
            & "        , @P17    , @P18" _
            & "        , @P19    , @P20" _
            & "        , @P21    , @P22" _
            & "        , @P23    , @P24" _
            & "        , @P25    , @P26" _
            & "        , @P27    , @P28) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , UORG" _
            & "    , PRODUCTCODE" _
            & "    , SEQ" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , PRODTYPE01" _
            & "    , PRODTYPE02" _
            & "    , PRODTYPE03" _
            & "    , PRODTYPE04" _
            & "    , PRODTYPE05" _
            & "    , PRODTYPE06" _
            & "    , PRODTYPE07" _
            & "    , PRODTYPE08" _
            & "    , PRODTYPE09" _
            & "    , PRODTYPE10" _
            & "    , HTANI" _
            & "    , YPRODUCT" _
            & "    , KPRODUCT" _
            & "    , JSRPRODUCT" _
            & "    , UNLOADADDTANKA" _
            & "    , LOADINGTANKA" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    MD002_PRODORG" _
            & " WHERE" _
            & "    CAMPCODE        = @P1" _
            & "    AND UORG        = @P2" _
            & "    AND PRODUCTCODE = @P3"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            '運用部署
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 30)            '品名コード
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Int)                     '表示順番
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                    '開始年月日
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)                    '終了年月日
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 3)             '品目タイプ01
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 3)             '品目タイプ02
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 3)             '品目タイプ03
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 3)           '品目タイプ04
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 3)           '品目タイプ05
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 3)           '品目タイプ06
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 3)           '品目タイプ07
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 3)           '品目タイプ08
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 3)           '品目タイプ09
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 3)           '品目タイプ10
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 10)          '配送単位
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 5)           '矢崎車端用品名
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 10)          '光英車端用品名
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)          'JSR品名コード
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Int)                   '荷卸時加算単価
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Int)                   '積込単価
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.DateTime)              '登録年月日
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.DateTime)              '更新年月日
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '運用部署
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 30)        '品名コード

                For Each MD0002row As DataRow In MD0002tbl.Rows
                    If Trim(MD0002row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING Then

                        '新規分で削除のレコードは作成しない
                        If MD0002row("TIMSTP") = 0 AndAlso MD0002row("DELFLG") = C_DELETE_FLG.DELETE Then
                            MD0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Continue For
                        End If

                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = MD0002row("CAMPCODE")
                        PARA2.Value = MD0002row("UORG")
                        PARA3.Value = MD0002row("PRODUCTCODE")
                        PARA4.Value = MD0002row("SEQ")
                        PARA5.Value = MD0002row("STYMD")
                        PARA6.Value = MD0002row("ENDYMD")
                        PARA7.Value = MD0002row("PRODTYPE01")
                        PARA8.Value = MD0002row("PRODTYPE02")
                        PARA9.Value = MD0002row("PRODTYPE03")
                        PARA10.Value = MD0002row("PRODTYPE04")
                        PARA11.Value = MD0002row("PRODTYPE05")
                        PARA12.Value = MD0002row("PRODTYPE06")
                        PARA13.Value = MD0002row("PRODTYPE07")
                        PARA14.Value = MD0002row("PRODTYPE08")
                        PARA15.Value = MD0002row("PRODTYPE09")
                        PARA16.Value = MD0002row("PRODTYPE10")
                        PARA17.Value = MD0002row("HTANI")
                        PARA18.Value = MD0002row("YPRODUCT")
                        PARA19.Value = MD0002row("KPRODUCT")
                        PARA20.Value = MD0002row("JSRPRODUCT")
                        PARA21.Value = MD0002row("UNLOADADDTANKA")
                        PARA22.Value = MD0002row("LOADINGTANKA")
                        PARA23.Value = MD0002row("DELFLG")
                        PARA24.Value = WW_DATENOW
                        PARA25.Value = WW_DATENOW
                        PARA26.Value = Master.USERID
                        PARA27.Value = Master.USERTERMID
                        PARA28.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        MD0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = MD0002row("CAMPCODE")
                        JPARA2.Value = MD0002row("UORG")
                        JPARA3.Value = MD0002row("PRODUCTCODE")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(MD0002UPDtbl) Then
                                MD0002UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    MD0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            MD0002UPDtbl.Clear()
                            MD0002UPDtbl.Load(SQLdr)
                        End Using

                        For Each MD0002UPDrow As DataRow In MD0002UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "MD002_PRODORG"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MD0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MD002_PRODORG UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MD002_PRODORG UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = MD0002tbl                        'データ参照  Table
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
        Master.CreateEmptyTable(MD0002INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim MD0002INProw As DataRow = MD0002INPtbl.NewRow

            '○ 初期クリア
            For Each MD0002INPcol As DataColumn In MD0002INPtbl.Columns
                If IsDBNull(MD0002INProw.Item(MD0002INPcol)) OrElse IsNothing(MD0002INProw.Item(MD0002INPcol)) Then
                    Select Case MD0002INPcol.ColumnName
                        Case "LINECNT"
                            MD0002INProw.Item(MD0002INPcol) = 0
                        Case "OPERATION"
                            MD0002INProw.Item(MD0002INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MD0002INProw.Item(MD0002INPcol) = 0
                        Case "SELECT"
                            MD0002INProw.Item(MD0002INPcol) = 1
                        Case "HIDDEN"
                            MD0002INProw.Item(MD0002INPcol) = 0
                        Case "SORTSEQ"
                            MD0002INProw.Item(MD0002INPcol) = 0
                        Case "UNLOADADDTANKA", "LOADINGTANKA"
                            MD0002INProw.Item(MD0002INPcol) = "0"
                        Case Else
                            MD0002INProw.Item(MD0002INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("UORG") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OILTYPE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("PRODUCT1") >= 0 AndAlso
                WW_COLUMNS.IndexOf("PRODUCT2") >= 0 Then
                For Each MD0002row As DataRow In MD0002tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MD0002row("CAMPCODE") AndAlso
                        XLSTBLrow("UORG") = MD0002row("UORG") AndAlso
                        XLSTBLrow("OILTYPE") = MD0002row("OILTYPE") AndAlso
                        XLSTBLrow("PRODUCT1") = MD0002row("PRODUCT1") AndAlso
                        XLSTBLrow("PRODUCT2") = MD0002row("PRODUCT2") Then
                        MD0002INProw.ItemArray = MD0002row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MD0002INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '運用部署
            If WW_COLUMNS.IndexOf("UORG") >= 0 Then
                MD0002INProw("UORG") = XLSTBLrow("UORG")
            End If

            '油種
            If WW_COLUMNS.IndexOf("OILTYPE") >= 0 Then
                MD0002INProw("OILTYPE") = XLSTBLrow("OILTYPE")
            End If

            '品名１
            If WW_COLUMNS.IndexOf("PRODUCT1") >= 0 Then
                MD0002INProw("PRODUCT1") = XLSTBLrow("PRODUCT1")
            End If

            '品名２
            If WW_COLUMNS.IndexOf("PRODUCT2") >= 0 Then
                MD0002INProw("PRODUCT2") = XLSTBLrow("PRODUCT2")
            End If

            '表示順番
            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                MD0002INProw("SEQ") = XLSTBLrow("SEQ")
            End If

            '配送単位
            If WW_COLUMNS.IndexOf("HTANI") >= 0 Then
                MD0002INProw("HTANI") = XLSTBLrow("HTANI")
            End If

            '矢崎車端用品名コード
            If WW_COLUMNS.IndexOf("YPRODUCT") >= 0 Then
                MD0002INProw("YPRODUCT") = XLSTBLrow("YPRODUCT")
            End If

            '光英車端用品名コード
            If WW_COLUMNS.IndexOf("KPRODUCT") >= 0 Then
                MD0002INProw("KPRODUCT") = XLSTBLrow("KPRODUCT")
            End If

            'JSR品名コード
            If WW_COLUMNS.IndexOf("JSRPRODUCT") >= 0 Then
                MD0002INProw("JSRPRODUCT") = XLSTBLrow("JSRPRODUCT")
            End If

            '荷卸時加算単価
            If WW_COLUMNS.IndexOf("UNLOADADDTANKA") >= 0 Then
                MD0002INProw("UNLOADADDTANKA") = XLSTBLrow("UNLOADADDTANKA")
            End If

            '積込単価
            If WW_COLUMNS.IndexOf("LOADINGTANKA") >= 0 Then
                MD0002INProw("LOADINGTANKA") = XLSTBLrow("LOADINGTANKA")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MD0002INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            MD0002INProw("STYMD") = "2000/01/01"
            MD0002INProw("ENDYMD") = "2099/12/31"

            '名称取得
            CODENAME_get("HTANI", MD0002INProw("HTANI"), MD0002INProw("HTANINAMES"), WW_DUMMY)          '配送単位

            MD0002INPtbl.Rows.Add(MD0002INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        MD0002tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(MD0002tbl)

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
        For Each MD0002INProw As DataRow In MD0002INPtbl.Rows

            WW_LINE_ERR = ""

            '会社コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MD0002INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MD0002INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_CAMPCODE.Text <> MD0002INProw("CAMPCODE") Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "検索条件の会社コードと一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用部署
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UORG", MD0002INProw("UORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("UORG", MD0002INProw("UORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_CAMPCODE.Text <> MD0002INProw("CAMPCODE") Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "検索条件の運用部署と一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OILTYPE", MD0002INProw("OILTYPE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("OILTYPE", MD0002INProw("OILTYPE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '品名１
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", MD0002INProw("PRODUCT1"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("PRODUCT1", MD0002INProw("PRODUCT1"), WW_DUMMY, WW_RTN_SW, MD0002INProw("OILTYPE"))
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(品名１エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(品名１エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '品名２
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", MD0002INProw("PRODUCT2"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(品名２エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '品名
            MD0002INProw("PRODUCTCODE") = String.Format("{0}{1}{2}{3}", MD0002INProw("CAMPCODE"), MD0002INProw("OILTYPE"), MD0002INProw("PRODUCT1"), MD0002INProw("PRODUCT2"))
            CODENAME_get("PRODUCT", MD0002INProw("PRODUCTCODE"), WW_DUMMY, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(品名エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '配送単位
            If IsNothing(MD0002INProw("HTANI")) Then
                MD0002INProw("HTANI") = MD0002INProw("STANI")
            End If

            WW_TEXT = MD0002INProw("HTANI")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "HTANI", MD0002INProw("HTANI"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MD0002INProw("HTANI") = ""
                Else
                    '存在チェック
                    CODENAME_get("HTANI", MD0002INProw("HTANI"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(配送単位エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(配送単位エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '矢崎車端用品名コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "YPRODUCT", MD0002INProw("YPRODUCT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(矢崎車端用品名コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '光英車端用品名コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "KPRODUCT", MD0002INProw("KPRODUCT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(光英車端用品名コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JSR品名コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JSRPRODUCT", MD0002INProw("JSRPRODUCT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JSR品名コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷卸時加算単価
            WW_TEXT = MD0002INProw("UNLOADADDTANKA")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UNLOADADDTANKA", MD0002INProw("UNLOADADDTANKA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" OrElse WW_TEXT = "0" Then
                    MD0002INProw("UNLOADADDTANKA") = "0"
                Else
                    Try
                        MD0002INProw("UNLOADADDTANKA") = Format(CInt(MD0002INProw("UNLOADADDTANKA")), "#0")
                    Catch ex As Exception
                        MD0002INProw("UNLOADADDTANKA") = "0"
                    End Try

                    '存在チェック
                    CODENAME_get("UNLOADADDTANKA", MD0002INProw("UNLOADADDTANKA"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(荷卸時加算単価エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷卸時加算単価エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込単価
            WW_TEXT = MD0002INProw("LOADINGTANKA")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LOADINGTANKA", MD0002INProw("LOADINGTANKA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" OrElse WW_TEXT = "0" Then
                    MD0002INProw("LOADINGTANKA") = "0"
                Else
                    Try
                        MD0002INProw("LOADINGTANKA") = Format(CInt(MD0002INProw("LOADINGTANKA")), "#0")
                    Catch ex As Exception
                        MD0002INProw("LOADINGTANKA") = "0"
                    End Try

                    '存在チェック
                    CODENAME_get("LOADINGTANKA", MD0002INProw("LOADINGTANKA"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(積込単価エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(積込単価エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'SEQ
            WW_TEXT = MD0002INProw("SEQ")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEQ", MD0002INProw("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MD0002INProw("SEQ") = ""
                Else
                    Try
                        MD0002INProw("SEQ") = Format(CInt(MD0002INProw("SEQ")), "#0")
                    Catch ex As Exception
                        MD0002INProw("SEQ") = "0"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", MD0002INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", MD0002INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MD0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If MD0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MD0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MD0002INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If

        Next

    End Sub

    ''' <summary>
    ''' MD0002tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MD0002tbl_UPD()

        '○ 追加変更判定
        For Each MD0002INProw As DataRow In MD0002INPtbl.Rows

            'エラーレコード読み飛ばし
            If MD0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            MD0002INProw("OPERATION") = "Insert"

            'KEY項目が等しい
            For Each MD0002row As DataRow In MD0002tbl.Rows
                If MD0002row("CAMPCODE") = MD0002INProw("CAMPCODE") AndAlso
                    MD0002row("UORG") = MD0002INProw("UORG") AndAlso
                    ((MD0002row("PRODUCTCODE") = MD0002INProw("PRODUCTCODE")) OrElse
                    (MD0002row("OILTYPE") = MD0002INProw("OILTYPE") AndAlso
                    MD0002row("PRODUCT1") = MD0002INProw("PRODUCT1") AndAlso
                    MD0002row("PRODUCT2") = MD0002INProw("PRODUCT2"))) Then

                    '変更無は操作無
                    If MD0002row("SEQ") = MD0002INProw("SEQ") AndAlso
                        MD0002row("HTANI") = MD0002INProw("HTANI") AndAlso
                        MD0002row("YPRODUCT") = MD0002INProw("YPRODUCT") AndAlso
                        MD0002row("KPRODUCT") = MD0002INProw("KPRODUCT") AndAlso
                        MD0002row("JSRPRODUCT") = MD0002INProw("JSRPRODUCT") AndAlso
                        MD0002row("UNLOADADDTANKA") = MD0002INProw("UNLOADADDTANKA") AndAlso
                        MD0002row("LOADINGTANKA") = MD0002INProw("LOADINGTANKA") AndAlso
                        MD0002row("DELFLG") = MD0002INProw("DELFLG") Then
                        MD0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    MD0002INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each MD0002INProw As DataRow In MD0002INPtbl.Rows
            Select Case MD0002INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(MD0002INProw)
                Case "Insert"
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="MD0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef MD0002INProw As DataRow)

        For Each MD0002row As DataRow In MD0002tbl.Rows

            '同一レコード
            If MD0002INProw("CAMPCODE") = MD0002row("CAMPCODE") AndAlso
                MD0002INProw("UORG") = MD0002row("UORG") AndAlso
                ((MD0002INProw("PRODUCTCODE") = MD0002row("PRODUCTCODE")) OrElse
                (MD0002INProw("OILTYPE") = MD0002row("OILTYPE") AndAlso
                MD0002INProw("PRODUCT1") = MD0002row("PRODUCT1") AndAlso
                MD0002INProw("PRODUCT2") = MD0002row("PRODUCT2"))) Then

                '画面入力テーブル項目設定
                MD0002INProw("LINECNT") = MD0002row("LINECNT")
                MD0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MD0002INProw("TIMSTP") = MD0002row("TIMSTP")
                MD0002INProw("SELECT") = 1
                MD0002INProw("HIDDEN") = 0

                '使用有無判定
                If MD0002INProw("DELFLG") = C_DELETE_FLG.ALIVE Then
                    MD0002INProw("ORGUSE") = "01"       '使用
                Else
                    MD0002INProw("ORGUSE") = "02"       '未使用
                End If

                '項目テーブル項目設定
                MD0002row.ItemArray = MD0002INProw.ItemArray
                Exit For
            End If
        Next

    End Sub
    ''' <summary>
    ''' 一覧変更情報取込処理
    ''' </summary>
    Protected Sub WF_TableChange()

        For Each row As DataRow In MD0002tbl.Rows
            WF_SelectedIndex.Value = CStr(row("LINECNT"))
            WF_ListChange(False)
        Next
        '○ 画面表示データ保存
        Master.SaveTable(MD0002tbl)
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
        If MD0002tbl.Rows(WW_LINECNT)("ORGUSE") <> Convert.ToString(Request.Form("ctl00$contents1$rblORGUSEORGUSE" & WF_SelectedIndex.Value)) Then
            MD0002tbl.Rows(WW_LINECNT)("ORGUSE") = Convert.ToString(Request.Form("ctl00$contents1$rblORGUSEORGUSE" & WF_SelectedIndex.Value))
            MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '配送単位
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "HTANI" & WF_SelectedIndex.Value)) AndAlso
            MD0002tbl.Rows(WW_LINECNT)("HTANI") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "HTANI" & WF_SelectedIndex.Value)) Then
            MD0002tbl.Rows(WW_LINECNT)("HTANI") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "HTANI" & WF_SelectedIndex.Value))
            CODENAME_get("HTANI", MD0002tbl(WW_LINECNT)("HTANI"), MD0002tbl(WW_LINECNT)("HTANINAMES"), WW_DUMMY)
            MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '矢崎車端用品名コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "YPRODUCT" & WF_SelectedIndex.Value)) AndAlso
            MD0002tbl.Rows(WW_LINECNT)("YPRODUCT") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YPRODUCT" & WF_SelectedIndex.Value)) Then
            MD0002tbl.Rows(WW_LINECNT)("YPRODUCT") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YPRODUCT" & WF_SelectedIndex.Value))
            MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '光英車端用品名コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "KPRODUCT" & WF_SelectedIndex.Value)) AndAlso
            MD0002tbl.Rows(WW_LINECNT)("KPRODUCT") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KPRODUCT" & WF_SelectedIndex.Value)) Then
            MD0002tbl.Rows(WW_LINECNT)("KPRODUCT") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KPRODUCT" & WF_SelectedIndex.Value))
            MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        'JSR品名コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "JSRPRODUCT" & WF_SelectedIndex.Value)) AndAlso
            MD0002tbl.Rows(WW_LINECNT)("JSRPRODUCT") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRPRODUCT" & WF_SelectedIndex.Value)) Then
            MD0002tbl.Rows(WW_LINECNT)("JSRPRODUCT") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRPRODUCT" & WF_SelectedIndex.Value))
            MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '荷卸時加算単価
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "UNLOADADDTANKA" & WF_SelectedIndex.Value)) AndAlso
            MD0002tbl.Rows(WW_LINECNT)("UNLOADADDTANKA") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "UNLOADADDTANKA" & WF_SelectedIndex.Value)) Then
            MD0002tbl.Rows(WW_LINECNT)("UNLOADADDTANKA") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "UNLOADADDTANKA" & WF_SelectedIndex.Value))
            MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '積込単価
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "LOADINGTANKA" & WF_SelectedIndex.Value)) AndAlso
            MD0002tbl.Rows(WW_LINECNT)("LOADINGTANKA") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "LOADINGTANKA" & WF_SelectedIndex.Value)) Then
            MD0002tbl.Rows(WW_LINECNT)("LOADINGTANKA") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "LOADINGTANKA" & WF_SelectedIndex.Value))
            MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        'SEQ
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) AndAlso
            MD0002tbl.Rows(WW_LINECNT)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) Then
            MD0002tbl.Rows(WW_LINECNT)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value))
            MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '○ 画面表示データ保存
        If isSaving Then Master.SaveTable(MD0002tbl)

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
                    Case "HTANI"                '配送単位
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "HTANI"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                    Case "UNLOADADDTANKA"       '荷卸時加算単価
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "UNLOADADDTANKA"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                    Case "LOADINGTANKA"         '積込単価
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "LOADINGTANKA"
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
            Case "HTANI"                '配送単位
                If MD0002tbl.Rows(WW_LINECNT)("HTANI") <> WW_SelectValue Then
                    MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MD0002tbl.Rows(WW_LINECNT)("HTANI") = WW_SelectValue
                MD0002tbl.Rows(WW_LINECNT)("HTANINAMES") = WW_SelectText

            Case "UNLOADADDTANKA"       '荷卸時加算単価
                If MD0002tbl.Rows(WW_LINECNT)("UNLOADADDTANKA") <> WW_SelectValue Then
                    MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MD0002tbl.Rows(WW_LINECNT)("UNLOADADDTANKA") = WW_SelectValue

            Case "LOADINGTANKA"         '積込単価
                If MD0002tbl.Rows(WW_LINECNT)("LOADINGTANKA") <> WW_SelectValue Then
                    MD0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MD0002tbl.Rows(WW_LINECNT)("LOADINGTANKA") = WW_SelectValue
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(MD0002tbl)

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
            Case "HTANI"                '配送単位
            Case "UNLOADADDTANKA"       '荷卸時加算単価
            Case "LOADINGTANKA"         '積込単価
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
    ''' <param name="MD0002row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal MD0002row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(MD0002row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社               =" & MD0002row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用部署           =" & MD0002row("UORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種               =" & MD0002row("OILTYPE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 品名１             =" & MD0002row("PRODUCT1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 品名２             =" & MD0002row("PRODUCT2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 品名               =" & MD0002row("PRODUCTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 配送単位           =" & MD0002row("HTANI") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 矢崎車端用品名ｺｰﾄﾞ =" & MD0002row("YPRODUCT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 光英車端用品名ｺｰﾄﾞ =" & MD0002row("KPRODUCT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順番           =" & MD0002row("SEQ")
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
    ''' <param name="I_OILTYPE"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal I_OILTYPE As String = "")

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
                Case "CAMPCODE"             '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UORG"                 '運用部署
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OILTYPE"              '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PRODUCT1"             '品名１
                    prmData = work.CreateProduct1Param(work.WF_SEL_CAMPCODE.Text, I_OILTYPE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PRODUCT"              '品名
                    prmData = work.CreateProductParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STANI"                '請求単位
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "STANI"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HTANI"                '配送単位
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "HTANI"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UNLOADADDTANKA"       '荷卸時加算単価
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "UNLOADADDTANKA"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LOADINGTANKA"         '積込単価
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "LOADINGTANKA"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"               '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

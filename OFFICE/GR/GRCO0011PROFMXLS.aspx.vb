Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 帳票入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRCO0011PROFMXLS
    Inherits Page

    '○ 検索結果格納Table
    Private CO0011tbl As DataTable                          '一覧格納用テーブル
    Private CO0011INPtbl As DataTable                       'チェック用テーブル
    Private CO0011UPDtbl As DataTable                       '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45        '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10         'マウススクロール時稼働行数
    Private Const CONST_EXCEL_MAXROW As Integer = 10        'エクセル最大行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL              '更新ジャーナル出力
    Private CS0025AUTHORget As New CS0025AUTHORget          '権限チェック(マスタチェック)
    Private CS0026TBLSORT As New CS0026TBLSORT              '表示画面情報ソート
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
                    If Not Master.RecoverTable(CO0011tbl, WF_XMLsaveF.Value) Then Exit Sub

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
                        Case "WF_BACK"                  '戻るボタン押下
                            WF_BACK_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_INSERT"                '挿入ボタン押下
                            WF_INSERT_Click()
                        Case "WF_DELETE"                '削除ボタン押下
                            WF_DELETE_Click()
                        Case "WF_ExcelDragStart"        'マウスドラッグ
                            WF_Excel_DragStart()
                        Case "WF_ExcelMouseUp"          'マウスアップ
                            WF_EXCEL_MouseUp()
                        Case "WF_ExcelChange"           'エクセル切替
                            WF_EXCEL_Change()
                        Case "WF_ExcelHeadDBClick"      'エクセル項目ヘッダーダブルクリック
                            WF_EXCEL_HEAD_DBClick()
                        Case "WF_ExcelDBClick"          'エクセル項目ダブルクリック
                            WF_EXCEL_DBClick()
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
            If Not IsNothing(CO0011tbl) Then
                CO0011tbl.Clear()
                CO0011tbl.Dispose()
                CO0011tbl = Nothing
            End If

            If Not IsNothing(CO0011INPtbl) Then
                CO0011INPtbl.Clear()
                CO0011INPtbl.Dispose()
                CO0011INPtbl = Nothing
            End If

            If Not IsNothing(CO0011UPDtbl) Then
                CO0011UPDtbl.Clear()
                CO0011UPDtbl.Dispose()
                CO0011UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRCO0011WRKINC.MAPID

        WF_SELMAPID.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.activeListBox()
        rightview.resetindex()

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

        '○ 詳細画面初期設定
        DetailInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.CO0011S Then
            'Grid情報保存先のファイル名
            WF_XMLsaveF.Value = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
                Master.USERID & "-" & Master.MAPID & "-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

            WF_XMLsaveF_INP.Value = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
                Master.USERID & "-" & Master.MAPID & "INP-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

            '会社コード表示
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        End If

        '○ ヘルプボタン非表示
        Master.dispHelp = False

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
        Master.SaveTable(CO0011tbl, WF_XMLsaveF.Value)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(CO0011tbl)

        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H'" _
            & " and LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
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

        If IsNothing(CO0011tbl) Then
            CO0011tbl = New DataTable
        End If

        If CO0011tbl.Columns.Count <> 0 Then
            CO0011tbl.Columns.Clear()
        End If

        CO0011tbl.Clear()

        '○ 検索SQL文
        '  検索説明
        '    ログインユーザーのPROFIDおよびデフォルトPROFIDのTBL(S0026_PROFMXLS)を取得
        '     また、FIXVALUEに登録した画面IDのみ更新可能とする
        '     画面表示は、参照可能および更新ユーザに関連するTBLデータとなるため
        '     ※データの権限について(参考)
        '     権限チェックは、表追加のタイミングで行う。
        '      (チェック内容)
        '        ① 操作USERは、TBL入力データ(USER)の更新権限をもっているか。
        '        ② TBL入力データ(USER)は、TBL入力データ(MAP)の参照および更新権限をもっているか。
        '        ③ TBL入力データ(USER)は、TBL入力データ(CAMPCODE)の参照および更新権限をもっているか。
        '  注意事項  日付について
        '    権限判断はすべてDateNow。グループコード、名称取得は全てDateNow。表追加時の①はDateNow。
        '    但し、表追加時の②および③は、TBL入力有効期限。

        Dim SQLStr As String =
              " SELECT" _
            & "    0                                               AS LINECNT" _
            & "    , ''                                            AS OPERATION" _
            & "    , CAST(S026.UPDTIMSTP AS BIGINT)                AS TIMSTP" _
            & "    , 1                                             AS 'SELECT'" _
            & "    , 0                                             AS HIDDEN" _
            & "    , ISNULL(RTRIM(S026.CAMPCODE), '')              AS CAMPCODE" _
            & "    , ''                                            AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(S026.PROFID), '')                AS PROFID" _
            & "    , ISNULL(RTRIM(S026.MAPID), '')                 AS MAPID" _
            & "    , ''                                            AS MAPNAMES" _
            & "    , ISNULL(RTRIM(S026.REPORTID), '')              AS REPORTID" _
            & "    , ISNULL(RTRIM(S026.TITLEKBN), '')              AS TITLEKBN" _
            & "    , ISNULL(RTRIM(S026.FIELD), '')                 AS FIELD" _
            & "    , ISNULL(FORMAT(S026.STYMD, 'yyyy/MM/dd'), '')  AS STYMD" _
            & "    , ISNULL(FORMAT(S026.ENDYMD, 'yyyy/MM/dd'), '') AS ENDYMD" _
            & "    , ISNULL(RTRIM(S026.FIELDNAMES), '')            AS FIELDNAMES" _
            & "    , ISNULL(RTRIM(S026.POSISTART), '0')            AS POSISTART" _
            & "    , ISNULL(S026.POSIROW, 0)                       AS POSIROW" _
            & "    , ISNULL(S026.POSICOL, 0)                       AS POSICOL" _
            & "    , ISNULL(S026.WIDTH, 0)                         AS WIDTH" _
            & "    , ISNULL(RTRIM(S026.EXCELFILE), '')             AS EXCELFILE" _
            & "    , ISNULL(RTRIM(S026.STRUCTCODE), '')            AS STRUCTCODE" _
            & "    , ISNULL(S026.SORTORDER, 0)                     AS SORTORDER" _
            & "    , ISNULL(RTRIM(S026.EFFECT), '')                AS EFFECT" _
            & "    , ISNULL(RTRIM(S026.FORMATTYPE), '')            AS FORMATTYPE" _
            & "    , ISNULL(RTRIM(S026.DELFLG), '')                AS DELFLG" _
            & "    , ''                                            AS DELFLGNAMES" _
            & " FROM" _
            & "    S0026_PROFMXLS S026" _
            & "    INNER JOIN MC001_FIXVALUE MC01" _
            & "        ON  MC01.CAMPCODE = @P1" _
            & "        AND MC01.CLASS    = @P3" _
            & "        AND MC01.KEYCODE  = S026.MAPID" _
            & "        AND MC01.STYMD   <= @P6" _
            & "        AND MC01.ENDYMD  >= @P6" _
            & "        AND MC01.DELFLG  <> @P7" _
            & " WHERE" _
            & "    S026.CAMPCODE    = @P1" _
            & "    AND S026.STYMD  <= @P4" _
            & "    AND S026.ENDYMD >= @P5" _
            & "    AND S026.DELFLG <> @P7"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '機能選択
        If work.WF_SEL_FUNCSEL.Text = GRCO0011WRKINC.C_LIST_FUNSEL_DEFAULT.VISIBLE Then
            SQLStr &= "    AND S026.PROFID IN ('" & C_DEFAULT_DATAKEY & "', @P2)"
        ElseIf work.WF_SEL_FUNCSEL.Text = GRCO0011WRKINC.C_LIST_FUNSEL_DEFAULT.INVISIBLE Then
            SQLStr &= "    AND S026.PROFID = @P2 "
        End If
        '画面ID(From)
        If Not String.IsNullOrEmpty(work.WF_SEL_MAPIDF.Text) Then
            SQLStr &= String.Format("    AND S026.MAPID  >= '{0}'", work.WF_SEL_MAPIDF.Text)
        End If
        '画面ID(To)
        If Not String.IsNullOrEmpty(work.WF_SEL_MAPIDT.Text) Then
            SQLStr &= String.Format("    AND S026.MAPID  <= '{0}'", work.WF_SEL_MAPIDT.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    S026.CAMPCODE" _
            & "    , S026.PROFID" _
            & "    , S026.MAPID" _
            & "    , S026.REPORTID" _
            & "    , S026.TITLEKBN"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'プロフID
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        '分類
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '有効年月日(To)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '有効年月日(From)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)                '現在日付
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.PROF_REPORT
                PARA3.Value = "CO0010_CO0011_MAPID"
                PARA4.Value = work.WF_SEL_ENDYMD.Text
                PARA5.Value = work.WF_SEL_STYMD.Text
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        CO0011tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    CO0011tbl.Load(SQLdr)
                End Using

                For Each CO0011row As DataRow In CO0011tbl.Rows
                    '名称取得
                    CODENAME_get("CAMPCODE", CO0011row("CAMPCODE"), CO0011row("CAMPNAMES"), WW_DUMMY)       '会社コード
                    CODENAME_get("MAPID", CO0011row("MAPID"), CO0011row("MAPNAMES"), WW_DUMMY)              '画面ID
                    CODENAME_get("DELFLG", CO0011row("DELFLG"), CO0011row("DELFLGNAMES"), WW_DUMMY)         '削除
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0026_PROFMXLS SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0026_PROFMXLS Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データソート
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TABLE = CO0011tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = "TITLEKBN = 'H'"
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            CO0011tbl = CS0026TBLSORT.TABLE
        End If

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        If WF_DISP.Value = "detailbox" Then
            ExcelFormatDisplay()
            Exit Sub
        End If

        '○ 表示対象行カウント(絞り込み対象)
        '   ※ 絞込 (Cell(4) : 0=表示対象, 1=非表示対象)
        For Each CO0011row As DataRow In CO0011tbl.Rows
            If CO0011row("HIDDEN") = 0 AndAlso CO0011row("TITLEKBN") = "H" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                CO0011row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(CO0011tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H'" _
            & " and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
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
    ''' Excel表示一覧設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ExcelFormatDisplay()

        If Not Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        Dim WW_TITLEKBN As String = ""
        Dim EXCELtbl As New DataTable

        If WF_TITLEKBN_TITLE.Checked Then
            WW_TITLEKBN = "T"
        End If
        If WF_TITLEKBN_ITEM.Checked Then
            WW_TITLEKBN = "I"
        End If

        '○ 項目選択リスト作成
        CS0026TBLSORT.TABLE = CO0011INPtbl
        CS0026TBLSORT.SORTING = "FIELD"
        CS0026TBLSORT.FILTER = "TITLEKBN <> 'H'" _
            & " and ((POSIROW = 0 and POSICOL = 0) or (EFFECT = 'N'))"
        CS0026TBLSORT.sort(EXCELtbl)

        '○ ListBoxはDragイベント未対応のため、Tableを使用し疑似ListBoxを作成する
        For Each EXCELrow As DataRow In EXCELtbl.Rows
            Dim ListRow = New TableRow With {.ViewStateMode = ViewStateMode.Disabled}
            Dim ListCell = New TableCell With {.ViewStateMode = ViewStateMode.Disabled}
            ListCell.Text = EXCELrow("FIELDNAMES")
            ListCell.ID = "WF_Rep_" & EXCELrow("FIELD")

            ListCell.Attributes.Add("draggable", "true")
            ListCell.Attributes.Add("onmousedown", "ExcelMouseDown(0, 0, '" & EXCELrow("FIELD") & "');")
            ListCell.Attributes.Add("ondragstart", "ExcelDragStart('" & EXCELrow("FIELD") & "');")

            If EXCELrow("FIELD") = WF_EXCEL_SELECT.Value Then
                ListCell.Style.Add("background-color", "#1E90FF")
                ListCell.Style.Add("color", "#FFFFFF")
            End If

            ListRow.Cells.Add(ListCell)
            WF_EXCEL_LIST.Rows.Add(ListRow)
        Next

        '○ 画面に表示する分のデータを抽出する
        CS0026TBLSORT.TABLE = CO0011INPtbl
        CS0026TBLSORT.SORTING = "POSIROW, POSICOL"
        CS0026TBLSORT.FILTER = "TITLEKBN = '" & WW_TITLEKBN & "' and EFFECT = 'Y'"
        CS0026TBLSORT.sort(EXCELtbl)

        '○ 画面表示枠へ実データをセット
        Try
            '○ ヘッダー部作成
            Dim HeadRow = New TableHeaderRow With {.ViewStateMode = ViewStateMode.Disabled}
            For col As Integer = 1 To CO0011INPtbl.Rows.Count + 10              '列
                Dim HeadCell = New TableHeaderCell With {.ViewStateMode = ViewStateMode.Disabled}

                HeadCell.Text = col.ToString()
                HeadCell.Attributes.Remove("ondblclick")
                HeadCell.Attributes.Add("ondblclick", "ExcelHeadDBClick(" & col & ");")
                HeadRow.Cells.Add(HeadCell)
            Next

            WF_EXCEL.Rows.Add(HeadRow)

            '○ 明細部作成
            For row As Integer = 1 To CONST_EXCEL_MAXROW                        '行
                Dim DetailRow = New TableRow With {.ViewStateMode = ViewStateMode.Disabled}

                For col As Integer = 1 To CO0011INPtbl.Rows.Count + 10          '列
                    Dim DetailCell = New TableCell With {.ViewStateMode = ViewStateMode.Disabled}
                    Dim cellTextBox = New TextBox With {.ReadOnly = True}

                    cellTextBox.ID = "WF_Rep_" & row.ToString() & "_" & col.ToString()

                    'ダブルクリックイベント追加
                    cellTextBox.Attributes.Add("ondblclick", "ExcelDBClick(" & row & ", " & col & ");")

                    'マウスダウン、マウスアップイベント追加
                    cellTextBox.Attributes.Add("draggable", "true")
                    cellTextBox.Attributes.Add("onmousedown", "ExcelMouseDown(" & row & ", " & col & ", '');")
                    cellTextBox.Attributes.Add("ondragstart", "ExcelDragStart('');")
                    cellTextBox.Attributes.Add("onmouseup", "ExcelMouseUp(" & row & ", " & col & ");")

                    '項目セット
                    For Each EXCELrow As DataRow In EXCELtbl.Rows
                        If EXCELrow("POSICOL") = col AndAlso
                            EXCELrow("POSIROW") = row Then
                            cellTextBox.Text = EXCELrow("FIELDNAMES")
                            Exit For
                        End If
                    Next

                    DetailCell.Controls.Add(cellTextBox)
                    DetailRow.Cells.Add(DetailCell)
                Next

                WF_EXCEL.Rows.Add(DetailRow)
            Next

            '○ 背景色設定
            Dim WW_ITEM_NAME = "WF_Rep_" & WF_EXCEL_ROW.Value & "_" & WF_EXCEL_COL.Value
            CType(WF_EXCEL.FindControl(WW_ITEM_NAME), TextBox).Style.Add("background-color", "rgb(220, 230, 240)")
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.CAST_FORMAT_ERROR_EX, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Finally
            EXCELtbl.Clear()
            EXCELtbl.Dispose()
            EXCELtbl = Nothing
        End Try

    End Sub


    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.eraseCharToIgnore(WF_SELMAPID.Text)

        '○ 名称取得
        CODENAME_get("MAPID", WF_SELMAPID.Text, WF_SELMAPID_TEXT.Text, WW_DUMMY)

        '○ 絞り込み操作(GridView明細Hidden設定)
        For Each CO0011row As DataRow In CO0011tbl.Rows

            '一度非表示にする
            CO0011row("HIDDEN") = 1

            Dim WW_HANTEI As Boolean = True

            '画面IDによる絞込判定
            If WF_SELMAPID.Text <> "" AndAlso
                WF_SELMAPID.Text <> CO0011row("MAPID") Then
                WW_HANTEI = False
            End If

            '画面(GridView)のHIDDENに結果格納
            If WW_HANTEI Then
                CO0011row("HIDDEN") = 0
            End If
        Next

        '○ 画面先頭を表示
        WF_GridPosition.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(CO0011tbl, WF_XMLsaveF.Value)

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_SELMAPID.Focus()
        WF_DISP.Value = "headerbox"

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ 関連チェック
        RelatedCheck(WW_ERR_SW)

        If isNormal(WW_ERR_SW) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'プロファイルマスタ(帳票)更新
                UpdateProfileMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0011tbl, WF_XMLsaveF.Value)

        '○ 詳細画面クリア
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        WF_SELMAPID.Focus()
        WF_DISP.Value = "headerbox"

    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTN As String)

        '○ 初期値設定
        O_RTN = C_MESSAGE_NO.NORMAL
        rightview.setErrorReport("")

        Dim WW_LINE_ERR As String = ""
        Dim WW_CheckMES As String = ""
        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Dim WW_DATE_ST2 As Date
        Dim WW_DATE_END2 As Date

        '○ 日付重複チェック
        For Each CO0011row As DataRow In CO0011tbl.Rows

            '読み飛ばし
            If (CO0011row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                CO0011row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                CO0011row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                CO0011row("STYMD") = "" Then
                Continue For
            End If

            WW_LINE_ERR = ""

            'チェック
            For Each CO0011chk As DataRow In CO0011tbl.Rows

                '同一KEY以外は読み飛ばし
                If CO0011row("CAMPCODE") <> CO0011chk("CAMPCODE") OrElse
                    CO0011row("PROFID") <> CO0011chk("PROFID") OrElse
                    CO0011row("MAPID") <> CO0011chk("MAPID") OrElse
                    CO0011row("REPORTID") <> CO0011chk("REPORTID") OrElse
                    CO0011row("FIELD") <> CO0011chk("FIELD") OrElse
                    CO0011chk("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If CO0011row("STYMD") = CO0011chk("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(CO0011row("STYMD"), WW_DATE_ST)
                    Date.TryParse(CO0011row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(CO0011chk("STYMD"), WW_DATE_ST2)
                    Date.TryParse(CO0011chk("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                    Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End Try

                '開始日チェック
                If WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", CO0011row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If

                '終了日チェック
                If WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", CO0011row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If
            Next

            If WW_LINE_ERR = "" Then
                CO0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' プロファイルマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateProfileMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        S0026_PROFMXLS" _
            & "    WHERE" _
            & "        CAMPCODE     = @P1" _
            & "        AND PROFID   = @P2" _
            & "        AND MAPID    = @P3" _
            & "        AND REPORTID = @P4" _
            & "        AND FIELD    = @P6" _
            & "        AND STYMD    = @P7 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE S0026_PROFMXLS" _
            & "    SET" _
            & "        TITLEKBN     = @P5     , ENDYMD     = @P8" _
            & "        , FIELDNAMES = @P9     , POSISTART  = @P10" _
            & "        , POSIROW    = @P11    , POSICOL    = @P12" _
            & "        , WIDTH      = @P13    , EXCELFILE  = @P14" _
            & "        , STRUCTCODE = @P15    , SORTORDER  = @P16" _
            & "        , EFFECT     = @P17    , FORMATTYPE = @P18" _
            & "        , DELFLG     = @P19    , UPDYMD     = @P21" _
            & "        , UPDUSER    = @P22    , UPDTERMID  = @P23" _
            & "        , RECEIVEYMD = @P24" _
            & "    WHERE" _
            & "        CAMPCODE     = @P1" _
            & "        AND PROFID   = @P2" _
            & "        AND MAPID    = @P3" _
            & "        AND REPORTID = @P4" _
            & "        AND FIELD    = @P6" _
            & "        AND STYMD    = @P7 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO S0026_PROFMXLS" _
            & "        (CAMPCODE      , PROFID       , MAPID" _
            & "        , REPORTID     , TITLEKBN     , FIELD" _
            & "        , STYMD        , ENDYMD       , FIELDNAMES" _
            & "        , POSISTART    , POSIROW      , POSICOL" _
            & "        , WIDTH        , EXCELFILE    , STRUCTCODE" _
            & "        , SORTORDER    , EFFECT       , FORMATTYPE" _
            & "        , DELFLG       , INITYMD      , UPDYMD" _
            & "        , UPDUSER      , UPDTERMID    , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2     , @P3" _
            & "        , @P4     , @P5     , @P6" _
            & "        , @P7     , @P8     , @P9" _
            & "        , @P10    , @P11    , @P12" _
            & "        , @P13    , @P14    , @P15" _
            & "        , @P16    , @P17    , @P18" _
            & "        , @P19    , @P20    , @P21" _
            & "        , @P22    , @P23    , @P24) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , PROFID" _
            & "    , MAPID" _
            & "    , REPORTID" _
            & "    , TITLEKBN" _
            & "    , FIELD" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , FIELDNAMES" _
            & "    , POSISTART" _
            & "    , POSIROW" _
            & "    , POSICOL" _
            & "    , WIDTH" _
            & "    , EXCELFILE" _
            & "    , STRUCTCODE" _
            & "    , SORTORDER" _
            & "    , EFFECT" _
            & "    , FORMATTYPE" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) TIMSTP" _
            & " FROM" _
            & "    S0026_PROFMXLS" _
            & " WHERE" _
            & "    CAMPCODE     = @P1" _
            & "    AND PROFID   = @P2" _
            & "    AND MAPID    = @P3" _
            & "    AND REPORTID = @P4" _
            & "    AND FIELD    = @P5" _
            & "    AND STYMD    = @P6"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            'プロファイルID
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 50)            '画面ID
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 50)            'レポートID
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)            'タイトル区分
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 50)            '項目
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                    '開始年月日
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Date)                    '終了年月日
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)            '項目名称(短)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.Int)                   '明細行開始位置
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.Int)                   '行位置
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Int)                   '列位置
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Int)                   '横幅
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 50)          'テンプレート（書式）Excel名
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 50)          '構造コード
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.Int)                   '並び順
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 1)           '表示有無
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)          'フォーマットタイプ
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.DateTime)              '登録年月日
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.DateTime)              '更新年月日
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'プロファイルID
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 50)        '画面ID
                Dim JPARA4 As SqlParameter = SQLcmdJnl.Parameters.Add("@P4", SqlDbType.NVarChar, 50)        'レポートID
                Dim JPARA5 As SqlParameter = SQLcmdJnl.Parameters.Add("@P5", SqlDbType.NVarChar, 50)        '項目
                Dim JPARA6 As SqlParameter = SQLcmdJnl.Parameters.Add("@P6", SqlDbType.Date)                '開始年月日

                For Each CO0011row As DataRow In CO0011tbl.Rows
                    If Trim(CO0011row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(CO0011row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = CO0011row("CAMPCODE")
                        PARA2.Value = CO0011row("PROFID")
                        PARA3.Value = CO0011row("MAPID")
                        PARA4.Value = CO0011row("REPORTID")
                        PARA5.Value = CO0011row("TITLEKBN")
                        PARA6.Value = CO0011row("FIELD")
                        PARA7.Value = CO0011row("STYMD")
                        PARA8.Value = CO0011row("ENDYMD")
                        PARA9.Value = CO0011row("FIELDNAMES")
                        PARA10.Value = CO0011row("POSISTART")
                        PARA11.Value = CO0011row("POSIROW")
                        PARA12.Value = CO0011row("POSICOL")
                        PARA13.Value = CO0011row("WIDTH")
                        PARA14.Value = CO0011row("EXCELFILE")
                        PARA15.Value = CO0011row("STRUCTCODE")
                        PARA16.Value = CO0011row("SORTORDER")
                        PARA17.Value = CO0011row("EFFECT")
                        PARA18.Value = CO0011row("FORMATTYPE")
                        PARA19.Value = CO0011row("DELFLG")
                        PARA20.Value = WW_DATENOW
                        PARA21.Value = WW_DATENOW
                        PARA22.Value = Master.USERID
                        PARA23.Value = Master.USERTERMID
                        PARA24.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = CO0011row("CAMPCODE")
                        JPARA2.Value = CO0011row("PROFID")
                        JPARA3.Value = CO0011row("MAPID")
                        JPARA4.Value = CO0011row("REPORTID")
                        JPARA5.Value = CO0011row("FIELD")
                        JPARA6.Value = CO0011row("STYMD")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(CO0011UPDtbl) Then
                                CO0011UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    CO0011UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            CO0011UPDtbl.Clear()
                            CO0011UPDtbl.Load(SQLdr)
                        End Using

                        For Each CO0011UPDrow As DataRow In CO0011UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "S0026_PROFMXLS"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = CO0011UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

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
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0026_PROFMXLS UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0026_PROFMXLS UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub


    ''' <summary>
    ''' 先頭頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"
        WF_SELMAPID.Focus()
        WF_DISP.Value = "headerbox"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(CO0011tbl)
        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H'"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        WF_SELMAPID.Focus()
        WF_DISP.Value = "headerbox"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_LINECNT As Integer = 0

        Master.CreateEmptyTable(CO0011INPtbl, WF_XMLsaveF.Value)

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try

        For i As Integer = 0 To CO0011tbl.Rows.Count - 1
            If CO0011tbl.Rows(i)("LINECNT") = WW_LINECNT Then
                WW_LINECNT = i
                Exit For
            End If
        Next

        '選択行
        WF_Sel_LINECNT.Text = CO0011tbl.Rows(WW_LINECNT)("LINECNT")

        '会社コード
        WF_CAMPCODE.Text = CO0011tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        'プロフID
        WF_PROFID.Text = CO0011tbl.Rows(WW_LINECNT)("PROFID")

        '画面ID
        WF_MAPID.Text = CO0011tbl.Rows(WW_LINECNT)("MAPID")
        CODENAME_get("MAPID", WF_MAPID.Text, WF_MAPID_TEXT.Text, WW_DUMMY)

        '帳票ID
        WF_REPORTID.Text = CO0011tbl.Rows(WW_LINECNT)("REPORTID")

        '帳票名称
        WF_REPORTNAMES.Text = CO0011tbl.Rows(WW_LINECNT)("FIELDNAMES")

        '有効年月日
        WF_STYMD.Text = CO0011tbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = CO0011tbl.Rows(WW_LINECNT)("ENDYMD")

        'EXCELファイル名
        WF_EXCELFILE.Text = CO0011tbl.Rows(WW_LINECNT)("EXCELFILE")

        '明細開始行
        WF_POSISTART.Text = CO0011tbl.Rows(WW_LINECNT)("POSISTART")

        '削除
        WF_DELFLG.Text = CO0011tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        'Excel切替
        WF_TITLEKBN_TITLE.Checked = True
        WF_TITLEKBN_ITEM.Checked = False

        '行列追加削除
        WF_ROW.Checked = True
        WF_COL.Checked = False

        CS0026TBLSORT.TABLE = CO0011tbl
        CS0026TBLSORT.SORTING = "TITLEKBN, POSIROW, POSICOL, FIELD"
        CS0026TBLSORT.FILTER = "CAMPCODE = '" & WF_CAMPCODE.Text & "'" _
            & " and PROFID = '" & WF_PROFID.Text & "'" _
            & " and MAPID = '" & WF_MAPID.Text & "'" _
            & " and REPORTID = '" & WF_REPORTID.Text & "'" _
            & " and STYMD = '" & WF_STYMD.Text & "'"
        CS0026TBLSORT.sort(CO0011INPtbl)
        Master.SaveTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        '○ 1行1列目選択
        WF_EXCEL_SELECT.Value = ""
        WF_EXCEL_ROW.Value = 1
        WF_EXCEL_COL.Value = 1
        WW_ExcelSelectDetail()

        WF_List_Top.Value = 0
        WF_Scroll_Left.Value = 0
        WF_Scroll_Top.Value = 0

        '○ 状態をクリア
        For Each CO0011row As DataRow In CO0011tbl.Rows
            Select Case CO0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case CO0011tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                CO0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                CO0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                CO0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                CO0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                CO0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(CO0011tbl, WF_XMLsaveF.Value)

        WF_REPORTID.Focus()
        WF_DISP.Value = "detailbox"
        WF_GridDBclick.Text = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

        WF_SELMAPID.Focus()
        WF_DISP.Value = "headerbox"

    End Sub


    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_PROFID.Text)            'プロフID
        Master.eraseCharToIgnore(WF_MAPID.Text)             '画面ID
        Master.eraseCharToIgnore(WF_REPORTID.Text)          'レポートID
        Master.eraseCharToIgnore(WF_REPORTNAMES.Text)       'レポート名称
        Master.eraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.eraseCharToIgnore(WF_EXCELFILE.Text)         'EXCELファイル名
        Master.eraseCharToIgnore(WF_POSISTART.Text)         '明細開始行
        Master.eraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        '○ ヘッダー情報をテーブルに反映
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            CO0011INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            CO0011INProw("PROFID") = WF_PROFID.Text
            CO0011INProw("MAPID") = WF_MAPID.Text
            CO0011INProw("REPORTID") = WF_REPORTID.Text

            If CO0011INProw("TITLEKBN") = "H" Then
                CO0011INProw("FIELDNAMES") = WF_REPORTNAMES.Text
                CO0011INProw("EXCELFILE") = WF_EXCELFILE.Text
            End If

            CO0011INProw("STYMD") = WF_STYMD.Text
            If WF_ENDYMD.Text = "" Then
                CO0011INProw("ENDYMD") = WF_STYMD.Text
            Else
                CO0011INProw("ENDYMD") = WF_ENDYMD.Text
            End If

            CO0011INProw("POSISTART") = WF_POSISTART.Text

            If WF_DELFLG.Text = "" Then
                CO0011INProw("DELFLG") = C_DELETE_FLG.ALIVE
            Else
                CO0011INProw("DELFLG") = WF_DELFLG.Text
            End If
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            CO0011tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0011tbl, WF_XMLsaveF.Value)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()

            Master.output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            WF_SELMAPID.Focus()
            WF_DISP.Value = "headerbox"
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
            WF_REPORTID.Focus()
            WF_DISP.Value = "detailbox"
        End If

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        WF_TITLEKBN_TITLE.Checked = True
        WF_TITLEKBN_ITEM.Checked = False

        WF_ROW.Checked = True
        WF_COL.Checked = False

        WF_List_Top.Value = 0
        WF_Scroll_Left.Value = 0
        WF_Scroll_Top.Value = 0

        '○ 明細項目クリア
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_REPORTID.Text = ""
        WF_REPORTNAMES.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_EXCELFILE.Text = ""
        WF_POSISTART.Text = ""
        WF_DELFLG.Text = C_DELETE_FLG.ALIVE
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        WF_EXCEL_SELECT.Value = ""
        WF_EXCEL_ROW.Value = 1
        WF_EXCEL_COL.Value = 1
        WF_FIELD_EXCEL.Value = ""

        WF_POSIROW.Text = "0"
        WF_POSICOL.Text = "0"
        WF_WIDTH.Text = "0"
        WF_SORT.Text = "0"
        WF_FIELDNAMES.Text = ""

        '○ CO0011INP項目クリア
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            If CO0011INProw("TITLEKBN") = "H" Then
                Continue For
            End If

            CO0011INProw("TITLEKBN") = "I"
            CO0011INProw("POSIROW") = 0
            CO0011INProw("POSICOL") = 0
            CO0011INProw("WIDTH") = 0
            CO0011INProw("STRUCTCODE") = ""
            CO0011INProw("SORTORDER") = 0
            CO0011INProw("EFFECT") = "N"
            CO0011INProw("FORMATTYPE") = ""
        Next

        Master.SaveTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        WW_ExcelSelectDetail()

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_REPORTID.Focus()
        WF_DISP.Value = "detailbox"

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_BACK_Click()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_SELMAPID.Focus()
        WF_DISP.Value = "headerbox"
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each CO0011row As DataRow In CO0011tbl.Rows
            Select Case CO0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(CO0011tbl, WF_XMLsaveF.Value)

        WF_Sel_LINECNT.Text = ""                            'LINECNT
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_PROFID.Text = ""                                 'プロフID
        WF_MAPID.Text = ""                                  '画面ID
        WF_MAPID_TEXT.Text = ""                             '画面名
        WF_REPORTID.Text = ""                               '帳票ID
        WF_REPORTNAMES.Text = ""                            '帳票名
        WF_STYMD.Text = ""                                  '有効年月日(From)
        WF_ENDYMD.Text = ""                                 '有効年月日(To)
        WF_EXCELFILE.Text = ""                              'EXCELファイル名
        WF_POSISTART.Text = ""                              '明細開始行
        WF_DELFLG.Text = ""                                 '削除
        WF_DELFLG_TEXT.Text = ""                            '削除名称

        WF_EXCEL_SELECT.Value = ""                          '項目選択リスト

        '○ 詳細画面初期設定
        DetailInitialize()

    End Sub

    ''' <summary>
    ''' 詳細画面-初期設定 (空明細作成 ＆ イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailInitialize()

        WF_List_Top.Value = 0
        WF_Scroll_Left.Value = 0
        WF_Scroll_Top.Value = 0

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
                            Case "WF_STYMD"         '有効年月日(From)
                                .WF_Calendar.Text = WF_STYMD.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = WF_ENDYMD.Text
                        End Select
                        .activeCalendar()

                    Case Else
                        '以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメーターを変える
                        Select Case WF_FIELD.Value
                            Case "WF_SELMAPID"          '画面ID
                                prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "CO0010_CO0011_MAPID"
                        End Select

                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
                End Select
            End With
        End If

    End Sub


    ''' <summary>
    ''' 挿入ボタンクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_INSERT_Click()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        Dim WW_TITLEKBN As String = ""
        Dim EXCELtbl As New DataTable

        If Not Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        If WF_TITLEKBN_TITLE.Checked Then
            WW_TITLEKBN = "T"
        End If
        If WF_TITLEKBN_ITEM.Checked Then
            WW_TITLEKBN = "I"
        End If

        '○ 選択箇所の最大の行 or 列が埋まっている場合エラー
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            If CO0011INProw("TITLEKBN") = WW_TITLEKBN AndAlso
                CO0011INProw("EFFECT") = "Y" Then
                If WF_ROW.Checked AndAlso
                    CO0011INProw("POSIROW") >= CONST_EXCEL_MAXROW AndAlso
                    CO0011INProw("POSICOL") = WF_EXCEL_COL.Value Then
                    rightview.AddErrorReport("・Viewイメージに項目を挿入する事が出来ません。")
                    Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End If

                If WF_COL.Checked AndAlso
                    CO0011INProw("POSIROW") = WF_EXCEL_ROW.Value AndAlso
                    CO0011INProw("POSICOL") >= CO0011INPtbl.Rows.Count + 10 Then
                    rightview.AddErrorReport("・Viewイメージに項目を挿入する事が出来ません。")
                    Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End If
            End If
        Next

        '○ 項目ずらし
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            If CO0011INProw("TITLEKBN") = WW_TITLEKBN AndAlso
                CO0011INProw("EFFECT") = "Y" Then
                '選択箇所以降の行を下に移動する
                If WF_ROW.Checked AndAlso
                    CO0011INProw("POSIROW") >= WF_EXCEL_ROW.Value AndAlso
                    CO0011INProw("POSICOL") = WF_EXCEL_COL.Value Then
                    CO0011INProw("POSIROW") = CO0011INProw("POSIROW") + 1
                End If

                '選択箇所以降の列を右に移動する
                If WF_COL.Checked AndAlso
                    CO0011INProw("POSIROW") = WF_EXCEL_ROW.Value AndAlso
                    CO0011INProw("POSICOL") >= WF_EXCEL_COL.Value Then
                    CO0011INProw("POSICOL") = CO0011INProw("POSICOL") + 1
                End If
            End If
        Next

        Master.SaveTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        WW_ExcelSelectDetail()

        WF_DISP.Value = "detailbox"
        WF_EXCEL_SELECT.Value = ""

        EXCELtbl.Clear()
        EXCELtbl.Dispose()
        EXCELtbl = Nothing

    End Sub


    ''' <summary>
    ''' 削除ボタンクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_DELETE_Click()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        Dim WW_TITLEKBN As String = ""

        If Not Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        Master.eraseCharToIgnore(WF_FIELDNAMES.Text)

        If WF_TITLEKBN_TITLE.Checked Then
            WW_TITLEKBN = "T"
        End If
        If WF_TITLEKBN_ITEM.Checked Then
            WW_TITLEKBN = "I"
        End If

        '○ 選択箇所削除
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            If CO0011INProw("TITLEKBN") = WW_TITLEKBN AndAlso
                CO0011INProw("EFFECT") = "Y" Then
                '選択箇所を削除する
                If CO0011INProw("POSIROW") = WF_EXCEL_ROW.Value AndAlso
                    CO0011INProw("POSICOL") = WF_EXCEL_COL.Value Then
                    CO0011INProw("TITLEKBN") = "I"
                    CO0011INProw("FIELDNAMES") = WF_FIELDNAMES.Text
                    CO0011INProw("POSIROW") = 0
                    CO0011INProw("POSICOL") = 0
                    CO0011INProw("WIDTH") = 0
                    CO0011INProw("STRUCTCODE") = ""
                    CO0011INProw("SORTORDER") = 0
                    CO0011INProw("EFFECT") = "N"
                    CO0011INProw("FORMATTYPE") = ""
                End If

                '選択箇所以降の行を上に詰める
                If WF_ROW.Checked AndAlso
                    CO0011INProw("POSIROW") >= WF_EXCEL_ROW.Value AndAlso
                    CO0011INProw("POSICOL") = WF_EXCEL_COL.Value Then
                    CO0011INProw("POSIROW") = CO0011INProw("POSIROW") - 1
                End If

                '選択箇所以降の列を左に詰める
                If WF_COL.Checked AndAlso
                    CO0011INProw("POSIROW") = WF_EXCEL_ROW.Value AndAlso
                    CO0011INProw("POSICOL") >= WF_EXCEL_COL.Value Then
                    CO0011INProw("POSICOL") = CO0011INProw("POSICOL") - 1
                End If
            End If
        Next

        Master.SaveTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        WW_ExcelSelectDetail()

        WF_DISP.Value = "detailbox"
        WF_EXCEL_SELECT.Value = ""

    End Sub


    ''' <summary>
    ''' マウス移動開始時処理
    ''' </summary>
    Protected Sub WF_Excel_DragStart()

        If Not Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        WF_EXCEL_END_ROW.Value = String.Empty
        WF_EXCEL_END_COL.Value = String.Empty

        If String.IsNullOrEmpty(WF_EXCEL_START_ROW.Value) OrElse
            String.IsNullOrEmpty(WF_EXCEL_START_COL.Value) Then
            WF_EXCEL_START_ROW.Value = String.Empty
            WF_EXCEL_START_COL.Value = String.Empty
            Exit Sub
        End If

        If WF_EXCEL_START_ROW.Value = "0" OrElse
            WF_EXCEL_START_COL.Value = "0" Then
            Exit Sub
        End If

        WF_EXCEL_ROW.Value = WF_EXCEL_START_ROW.Value
        WF_EXCEL_COL.Value = WF_EXCEL_START_COL.Value
        WW_ExcelSelectDetail()

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"

    End Sub


    ''' <summary>
    ''' エクセル項目移動(マウスによる移動)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_EXCEL_MouseUp()

        If Not Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        If String.IsNullOrEmpty(WF_EXCEL_START_ROW.Value) OrElse
            String.IsNullOrEmpty(WF_EXCEL_START_COL.Value) OrElse
            String.IsNullOrEmpty(WF_EXCEL_END_ROW.Value) OrElse
            String.IsNullOrEmpty(WF_EXCEL_END_COL.Value) Then
            WF_EXCEL_START_ROW.Value = ""
            WF_EXCEL_START_COL.Value = ""
            WF_EXCEL_END_ROW.Value = ""
            WF_EXCEL_END_COL.Value = ""
            Exit Sub
        End If

        If WF_EXCEL_START_ROW.Value = WF_EXCEL_END_ROW.Value AndAlso
            WF_EXCEL_START_COL.Value = WF_EXCEL_END_COL.Value Then
            WF_EXCEL_START_ROW.Value = ""
            WF_EXCEL_START_COL.Value = ""
            WF_EXCEL_END_ROW.Value = ""
            WF_EXCEL_END_COL.Value = ""
            Exit Sub
        End If

        Dim WW_ERR As String = ""
        Dim WW_TITLEKBN As String = ""

        If WF_TITLEKBN_TITLE.Checked Then
            WW_TITLEKBN = "T"
        End If
        If WF_TITLEKBN_ITEM.Checked Then
            WW_TITLEKBN = "I"
        End If

        If WF_EXCEL_START_ROW.Value = "0" AndAlso
            WF_EXCEL_START_COL.Value = "0" Then
            If WF_EXCEL_SELECT.Value = "" Then
                rightview.addErrorReport("・選択可能項目を選択 or Excelイメージの選択 が必要です。")
                Master.output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        End If

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        '○ 移動前と移動先の項目を取得
        Dim CO0011STRrow As DataRow = Nothing
        Dim CO0011ENDrow As DataRow = Nothing

        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            '移動前の項目を取得
            If CO0011INProw("POSIROW") = WF_EXCEL_START_ROW.Value AndAlso
                CO0011INProw("POSICOL") = WF_EXCEL_START_COL.Value Then
                If WF_EXCEL_START_ROW.Value = "0" AndAlso WF_EXCEL_START_COL.Value = "0" Then
                    If CO0011INProw("TITLEKBN") = "I" AndAlso
                        CO0011INProw("EFFECT") = "N" AndAlso
                        CO0011INProw("FIELD") = WF_EXCEL_SELECT.Value Then
                        CO0011STRrow = CO0011INPtbl.NewRow
                        CO0011STRrow.ItemArray = CO0011INProw.ItemArray
                    End If
                Else
                    If CO0011INProw("TITLEKBN") = WW_TITLEKBN AndAlso
                        CO0011INProw("EFFECT") = "Y" Then
                        CO0011STRrow = CO0011INPtbl.NewRow
                        CO0011STRrow.ItemArray = CO0011INProw.ItemArray
                    End If
                End If
            End If

            '移動先の項目を取得
            If WF_EXCEL_END_ROW.Value <> "0" AndAlso WF_EXCEL_END_COL.Value <> "0" AndAlso
                CO0011INProw("TITLEKBN") = WW_TITLEKBN AndAlso
                CO0011INProw("EFFECT") = "Y" AndAlso
                CO0011INProw("POSIROW") = WF_EXCEL_END_ROW.Value AndAlso
                CO0011INProw("POSICOL") = WF_EXCEL_END_COL.Value Then
                CO0011ENDrow = CO0011INPtbl.NewRow
                CO0011ENDrow.ItemArray = CO0011INProw.ItemArray
            End If
        Next

        '○ 移動前の座標に移動先の座標をセット
        If Not IsNothing(CO0011STRrow) Then
            CO0011STRrow("POSIROW") = WF_EXCEL_END_ROW.Value
            CO0011STRrow("POSICOL") = WF_EXCEL_END_COL.Value

            If WF_EXCEL_START_ROW.Value = "0" AndAlso WF_EXCEL_START_COL.Value = "0" Then
                CO0011STRrow("TITLEKBN") = WW_TITLEKBN
                CO0011STRrow("EFFECT") = "Y"
                CO0011STRrow("WIDTH") = 10
            End If

            If WF_EXCEL_END_ROW.Value = "0" AndAlso WF_EXCEL_END_COL.Value = "0" Then
                CO0011STRrow("TITLEKBN") = "I"
                CO0011STRrow("POSIROW") = 0
                CO0011STRrow("POSICOL") = 0
                CO0011STRrow("WIDTH") = 0
                CO0011STRrow("STRUCTCODE") = ""
                CO0011STRrow("SORTORDER") = 0
                CO0011STRrow("EFFECT") = "N"
                CO0011STRrow("FORMATTYPE") = ""
            End If
        End If

        '○ 移動先の座標に移動前の座標をセット
        If Not IsNothing(CO0011ENDrow) Then
            CO0011ENDrow("POSIROW") = WF_EXCEL_START_ROW.Value
            CO0011ENDrow("POSICOL") = WF_EXCEL_START_COL.Value

            If WF_EXCEL_START_ROW.Value = "0" AndAlso WF_EXCEL_START_COL.Value = "0" Then
                CO0011ENDrow("TITLEKBN") = "I"
                CO0011ENDrow("POSIROW") = 0
                CO0011ENDrow("POSICOL") = 0
                CO0011ENDrow("WIDTH") = 0
                CO0011ENDrow("STRUCTCODE") = ""
                CO0011ENDrow("SORTORDER") = 0
                CO0011ENDrow("EFFECT") = "N"
                CO0011ENDrow("FORMATTYPE") = ""
            End If
        End If

        '○ テーブルに保存する
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            '移動前の項目を保存
            If Not IsNothing(CO0011STRrow) AndAlso
                CO0011INProw("FIELD") = CO0011STRrow("FIELD") Then
                CO0011INProw.ItemArray = CO0011STRrow.ItemArray
            End If

            '移動先の項目を保存
            If Not IsNothing(CO0011ENDrow) AndAlso
                CO0011INProw("FIELD") = CO0011ENDrow("FIELD") Then
                CO0011INProw.ItemArray = CO0011ENDrow.ItemArray
            End If
        Next

        Master.SaveTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        WF_EXCEL_SELECT.Value = ""
        WF_EXCEL_ROW.Value = If(WF_EXCEL_END_ROW.Value = "0", WF_EXCEL_START_ROW.Value, WF_EXCEL_END_ROW.Value)
        WF_EXCEL_COL.Value = If(WF_EXCEL_END_COL.Value = "0", WF_EXCEL_START_COL.Value, WF_EXCEL_END_COL.Value)
        WW_ExcelSelectDetail()

        WF_List_Top.Value = 0

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"
        WF_EXCEL_START_ROW.Value = ""
        WF_EXCEL_START_COL.Value = ""
        WF_EXCEL_END_ROW.Value = ""
        WF_EXCEL_END_COL.Value = ""

    End Sub


    ''' <summary>
    ''' エクセル切替
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_EXCEL_Change()

        Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        Master.SaveTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        WF_EXCEL_SELECT.Value = ""
        WF_EXCEL_ROW.Value = 1
        WF_EXCEL_COL.Value = 1
        WW_ExcelSelectDetail()

        WF_List_Top.Value = 0
        WF_Scroll_Left.Value = 0
        WF_Scroll_Top.Value = 0

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"

    End Sub


    ''' <summary>
    ''' エクセル項目ヘッダーダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_EXCEL_HEAD_DBClick()

        Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        Dim WW_TITLEKBN As String = ""

        If WF_TITLEKBN_TITLE.Checked Then
            WW_TITLEKBN = "T"
        End If
        If WF_TITLEKBN_ITEM.Checked Then
            WW_TITLEKBN = "I"
        End If

        '○ ダブルクリックした列と以降の明細編集
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            If CO0011INProw("TITLEKBN") = WW_TITLEKBN AndAlso
                CO0011INProw("EFFECT") = "Y" Then
                '同じ列の場合、未使用にする
                If CO0011INProw("POSICOL") = WF_DELCOL.Value Then
                    CO0011INProw("TITLEKBN") = "I"
                    CO0011INProw("POSIROW") = 0
                    CO0011INProw("POSICOL") = 0
                    CO0011INProw("WIDTH") = 0
                    CO0011INProw("STRUCTCODE") = ""
                    CO0011INProw("SORTORDER") = 0
                    CO0011INProw("EFFECT") = "N"
                    CO0011INProw("FORMATTYPE") = ""
                End If

                '以降の場合1列左に移動
                If CO0011INProw("POSICOL") > WF_DELCOL.Value Then
                    CO0011INProw("POSICOL") = CO0011INProw("POSICOL") - 1
                End If
            End If
        Next

        Master.SaveTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        WF_EXCEL_SELECT.Value = ""
        WW_ExcelSelectDetail()

        WF_List_Top.Value = 0

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"

    End Sub


    ''' <summary>
    ''' エクセル項目ダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_EXCEL_DBClick()

        Master.RecoverTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        Master.SaveTable(CO0011INPtbl, WF_XMLsaveF_INP.Value)

        WW_ExcelSelectDetail()

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"

    End Sub


    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValue = leftview.getActiveValue(0)
            WW_SelectText = leftview.getActiveValue(1)
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_SELMAPID"          '画面ID
                WF_SELMAPID.Text = WW_SelectValue
                WF_SELMAPID_TEXT.Text = WW_SelectText
                WF_SELMAPID.Focus()

            Case "WF_STYMD"             '有効年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    WF_STYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"            '有効年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    WF_ENDYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                WF_ENDYMD.Focus()

            Case "WF_DELFLG"            '削除
                WF_DELFLG.Text = WW_SelectValue
                WF_DELFLG_TEXT.Text = WW_SelectText
                WF_DELFLG.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
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
            Case "WF_SELMAPID"          '画面ID
                WF_SELMAPID.Focus()
            Case "WF_STYMD"             '有効年月日(From)
                WF_STYMD.Focus()
            Case "WF_ENDYMD"            '有効年月日(To)
                WF_ENDYMD.Focus()
            Case "WF_DELFLG"            '削除
                WF_DELFLG.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
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

            rightview.selectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-テーブル反映
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub RepeaterUpdate()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        Dim WW_TEXT As String = ""
        Dim WW_LINE_ERR As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        Master.eraseCharToIgnore(WF_WIDTH.Text)
        Master.eraseCharToIgnore(WF_SORT.Text)
        Master.eraseCharToIgnore(WF_FIELDNAMES.Text)

        If Not String.IsNullOrEmpty(WF_FIELD_EXCEL.Value) Then
            '幅
            WW_TEXT = WF_WIDTH.Text
            If WW_TEXT = "" Then
                WF_WIDTH.Text = "10"
            Else
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "WIDTH", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・エラー(幅不正)の為、初期値10を設定しました。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                    WF_WIDTH.Text = "10"
                    WW_LINE_ERR = "ERR"
                End If
            End If

            'ソート
            WW_TEXT = WF_SORT.Text
            If WW_TEXT = "" Then
                WF_SORT.Text = "0"
            Else
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SORTORDER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・エラー(ソート不正)の為、初期値0を設定しました。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                    WF_SORT.Text = "0"
                    WW_LINE_ERR = "ERR"
                End If
            End If

            If WW_LINE_ERR = "ERR" Then
                Master.output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
            End If

            '○ 選択項目詳細の位置より項目を取得し、該当するCO0011INPtblへ選択項目詳細内容を反映
            For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
                If CO0011INProw("FIELD") = WF_FIELD_EXCEL.Value Then
                    CO0011INProw("WIDTH") = WF_WIDTH.Text
                    CO0011INProw("SORTORDER") = WF_SORT.Text
                    CO0011INProw("FIELDNAMES") = WF_FIELDNAMES.Text
                    Exit For
                End If
            Next
        End If

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

        '○ ヘッダー単項目チェック
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0011INProw("TITLEKBN") <> "H" Then
                Continue For
            End If

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", CO0011INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", CO0011INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'プロフID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "PROFID", CO0011INProw("PROFID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If CO0011INProw("PROFID") <> C_DEFAULT_DATAKEY AndAlso
                    CO0011INProw("PROFID") <> Master.PROF_REPORT Then
                    WW_CheckMES1 = "・更新できないレコード(プロフIDエラー)です。"
                    WW_CheckMES2 = "ログインユーザーのプロフIDと異なります。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(プロフIDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ユーザープロフID、Defaultはエラー
            If Master.PROF_REPORT = C_DEFAULT_DATAKEY Then
                WW_CheckMES1 = "・更新できないレコード(プロフID='" & C_DEFAULT_DATAKEY & "')です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面ID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MAPID", CO0011INProw("MAPID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("MAPID", CO0011INProw("MAPID"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(画面IDエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(画面IDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'レポートID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "REPORTID", CO0011INProw("REPORTID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(レポートIDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'レポート名称
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "REPORTNAMES", CO0011INProw("FIELDNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(レポート名称エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", CO0011INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", CO0011INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '日付大小チェック
            If CO0011INProw("STYMD") > CO0011INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(有効開始日＞有効終了日)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '範囲チェック
            If work.WF_SEL_STYMD.Text > CO0011INProw("STYMD") AndAlso
                work.WF_SEL_STYMD.Text > CO0011INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            If work.WF_SEL_ENDYMD.Text < CO0011INProw("STYMD") AndAlso
                work.WF_SEL_ENDYMD.Text < CO0011INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'EXCELファイル名
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "EXCELFILE", CO0011INProw("EXCELFILE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(EXCELファイル名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '明細開始行
            WW_TEXT = CO0011INProw("POSISTART")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "POSISTART", CO0011INProw("POSISTART"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0011INProw("POSISTART") = 0
                Else
                    Try
                        CO0011INProw("POSISTART") = Format(CInt(CO0011INProw("POSISTART")), "#0")
                    Catch ex As Exception
                        CO0011INProw("POSISTART") = 0
                    End Try
                End If
                If CO0011INProw("POSISTART") = 0 Then
                    WW_CheckMES1 = "・更新できないレコード(明細開始行がゼロ)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(明細開始行エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除フラグ
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", CO0011INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", CO0011INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                CO0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0011INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
            Exit For
        Next

        '○ 明細単項目チェック
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0011INProw("TITLEKBN") <> "T" AndAlso
                CO0011INProw("TITLEKBN") <> "I" Then
                Continue For
            End If

            '行位置
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "POSIROW", CO0011INProw("POSIROW"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(行位置エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '列位置
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "POSICOL", CO0011INProw("POSICOL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(列位置エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '入力不足
            If CO0011INProw("FIELD") = "REPORTID" AndAlso
                (CO0011INProw("POSIROW") = 0 OrElse CO0011INProw("POSICOL") = 0 OrElse CO0011INProw("EFFECT") = "N") Then
                WW_CheckMES1 = "・更新できないレコード(レポートID未設定)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If CO0011INProw("POSIROW") = 0 AndAlso CO0011INProw("POSICOL") <> 0 Then
                WW_CheckMES1 = "・更新できないレコード(列≠0、行＝0)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If CO0011INProw("POSIROW") <> 0 AndAlso CO0011INProw("POSICOL") = 0 Then
                WW_CheckMES1 = "・更新できないレコード(列＝0、行≠0)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '幅
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "WIDTH", CO0011INProw("WIDTH"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(幅エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ソート
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SORTORDER", CO0011INProw("SORTORDER"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(ソートエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '項目名称
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "FIELDNAMES", CO0011INProw("FIELDNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(項目名称エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '表示有無
            WW_TEXT = CO0011INProw("EFFECT")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "EFFECT", CO0011INProw("EFFECT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0011INProw("EFFECT") = "N"
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(表示有無エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '構造
            If CO0011INProw("STRUCTCODE") <> "" Then
                WW_CheckMES1 = "・更新できないレコード(列見出定義指定不可)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                CO0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0011INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        '○ 重複チェック
        For i As Integer = 0 To CO0011INPtbl.Rows.Count - 1
            If CO0011INPtbl.Rows(i)("POSIROW") <> 0 AndAlso CO0011INPtbl.Rows(i)("POSICOL") <> 0 Then
                For j As Integer = i + 1 To CO0011INPtbl.Rows.Count - 1
                    If CO0011INPtbl.Rows(i)("TITLEKBN") = CO0011INPtbl.Rows(j)("TITLEKBN") AndAlso
                        CO0011INPtbl.Rows(i)("POSIROW") = CO0011INPtbl.Rows(j)("POSIROW") AndAlso
                        CO0011INPtbl.Rows(i)("POSICOL") = CO0011INPtbl.Rows(j)("POSICOL") Then
                        WW_CheckMES1 = "・更新できないレコード(列行重複)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0011INPtbl.Rows(i))
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Next
            End If

            If WW_LINE_ERR = "" Then
                CO0011INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0011INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="CO0011row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal CO0011row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(CO0011row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード   =" & CO0011row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> プロフＩＤ   =" & CO0011row("PROFID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 画面ＩＤ     =" & CO0011row("MAPID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> レポートＩＤ =" & CO0011row("REPORTID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タイトル区分 =" & CO0011row("TITLEKBN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項目         =" & CO0011row("FIELD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日   =" & CO0011row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日   =" & CO0011row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除         =" & CO0011row("DELFLG")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' CO0011tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CO0011tbl_UPD()

        '○ 画面状態設定
        For Each CO0011row As DataRow In CO0011tbl.Rows
            Select Case CO0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 件数が膨大なため、会社と画面IDで絞る
        Dim CO0011FILtbl As New DataTable
        CS0026TBLSORT.TABLE = CO0011tbl
        CS0026TBLSORT.SORTING = ""
        CS0026TBLSORT.FILTER = "CAMPCODE = '" & WF_CAMPCODE.Text & "'" _
            & " and MAPID = '" & WF_MAPID.Text & "'"
        CS0026TBLSORT.sort(CO0011FILtbl)

        '○ 追加変更判定
        Dim WW_UPDAT As Boolean = False
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows

            'エラーレコード読み飛ばし
            If CO0011INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            CO0011INProw("OPERATION") = "Insert"

            'KEY項目が等しい(ENDYMD以外のKEYが同じ)
            For Each CO0011row As DataRow In CO0011FILtbl.Rows
                If CO0011row("CAMPCODE") = CO0011INProw("CAMPCODE") AndAlso
                    CO0011row("PROFID") = Master.PROF_REPORT AndAlso
                    CO0011row("MAPID") = CO0011INProw("MAPID") AndAlso
                    CO0011row("REPORTID") = CO0011INProw("REPORTID") AndAlso
                    CO0011row("FIELD") = CO0011INProw("FIELD") AndAlso
                    CO0011row("STYMD") = CO0011INProw("STYMD") Then

                    '変更無は操作無
                    If CO0011row("TITLEKBN") = CO0011INProw("TITLEKBN") AndAlso
                        CO0011row("ENDYMD") = CO0011INProw("ENDYMD") AndAlso
                        CO0011row("FIELDNAMES") = CO0011INProw("FIELDNAMES") AndAlso
                        CO0011row("POSISTART") = CO0011INProw("POSISTART") AndAlso
                        CO0011row("POSIROW") = CO0011INProw("POSIROW") AndAlso
                        CO0011row("POSICOL") = CO0011INProw("POSICOL") AndAlso
                        CO0011row("WIDTH") = CO0011INProw("WIDTH") AndAlso
                        CO0011row("EXCELFILE") = CO0011INProw("EXCELFILE") AndAlso
                        CO0011row("STRUCTCODE") = CO0011INProw("STRUCTCODE") AndAlso
                        CO0011row("SORTORDER") = CO0011INProw("SORTORDER") AndAlso
                        CO0011row("EFFECT") = CO0011INProw("EFFECT") AndAlso
                        CO0011row("FORMATTYPE") = CO0011INProw("FORMATTYPE") AndAlso
                        CO0011row("DELFLG") = CO0011INProw("DELFLG") Then
                        CO0011INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    CO0011INProw("OPERATION") = "Update"
                    WW_UPDAT = True
                    Exit For
                End If
            Next
        Next

        CO0011FILtbl.Clear()
        CO0011FILtbl.Dispose()
        CO0011FILtbl = Nothing

        '○ 更新レコードが存在する場合、ヘッダー区分も更新対象にする
        If WW_UPDAT Then
            For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
                If CO0011INProw("TITLEKBN") = "H" Then
                    CO0011INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        End If

        '○ 変更有無判定　&　入力値反映
        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            Select Case CO0011INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(CO0011INProw)
                Case "Insert"
                    TBL_INSERT_SUB(CO0011INProw)
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="CO0011INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByVal CO0011INProw As DataRow)

        For Each CO0011row As DataRow In CO0011tbl.Rows

            '同一(ENDYMD以外が同一KEY)レコード
            If CO0011row("CAMPCODE") = CO0011INProw("CAMPCODE") AndAlso
                CO0011row("PROFID") = Master.PROF_REPORT AndAlso
                CO0011row("MAPID") = CO0011INProw("MAPID") AndAlso
                CO0011row("REPORTID") = CO0011INProw("REPORTID") AndAlso
                CO0011row("FIELD") = CO0011INProw("FIELD") AndAlso
                CO0011row("STYMD") = CO0011INProw("STYMD") Then

                '画面入力テーブル項目設定
                CO0011INProw("LINECNT") = CO0011row("LINECNT")
                CO0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                CO0011INProw("TIMSTP") = CO0011row("TIMSTP")
                CO0011INProw("SELECT") = 1
                CO0011INProw("HIDDEN") = 0

                CO0011INProw("PROFID") = Master.PROF_REPORT

                '項目テーブル項目設定
                CO0011row.ItemArray = CO0011INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="CO0011INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByVal CO0011INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim CO0011row As DataRow = CO0011tbl.NewRow
        CO0011row.ItemArray = CO0011INProw.ItemArray

        '○ 最大項番数を取得
        Dim TBLview As DataView = New DataView(CO0011tbl)
        TBLview.RowFilter = "TITLEKBN = 'H'"

        If CO0011INProw("TITLEKBN") = "H" Then
            CO0011row("LINECNT") = TBLview.Count + 1
        Else
            CO0011row("LINECNT") = 0
        End If

        CO0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        CO0011row("TIMSTP") = "0"
        CO0011row("SELECT") = 1
        CO0011row("HIDDEN") = 0

        CO0011row("PROFID") = Master.PROF_REPORT

        CO0011tbl.Rows.Add(CO0011row)

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ''' <summary>
    ''' 項目変更時編集設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ExcelSelectDetail()

        '○ 座標セット
        WF_POSIROW.Text = WF_EXCEL_ROW.Value
        WF_POSICOL.Text = WF_EXCEL_COL.Value

        '○ 項目セット
        Dim WW_TITLEKBN As String = ""

        If WF_TITLEKBN_TITLE.Checked Then
            WW_TITLEKBN = "T"
        End If
        If WF_TITLEKBN_ITEM.Checked Then
            WW_TITLEKBN = "I"
        End If

        WF_FIELD_EXCEL.Value = ""
        WF_WIDTH.Text = "0"
        WF_SORT.Text = "0"
        WF_FIELDNAMES.Text = ""

        For Each CO0011INProw As DataRow In CO0011INPtbl.Rows
            If CO0011INProw("TITLEKBN") = WW_TITLEKBN AndAlso
                CO0011INProw("EFFECT") = "Y" AndAlso
                CO0011INProw("POSIROW") = WF_EXCEL_ROW.Value AndAlso
                CO0011INProw("POSICOL") = WF_EXCEL_COL.Value Then
                WF_FIELD_EXCEL.Value = CO0011INProw("FIELD")
                WF_WIDTH.Text = CO0011INProw("WIDTH")
                WF_SORT.Text = CO0011INProw("SORTORDER")
                WF_FIELDNAMES.Text = CO0011INProw("FIELDNAMES")
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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MAPID"            '画面ID
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "CO0010_CO0011_MAPID"
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

Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 画面表示項目入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRCO0010PROFMVIEW
    Inherits Page

    '○ 検索結果格納Table
    Private CO0010tbl As DataTable                                  '一覧格納用テーブル
    Private CO0010INPtbl As DataTable                               'チェック用テーブル
    Private CO0010UPDtbl As DataTable                               '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10                 'マウススクロール時稼働行数
    Private Const CONST_VIEW_HEAD_MAXROW As Integer = 1             '最大行数(ヘッダー)
    Private Const CONST_VIEW_DETAIL_MAXROW As Integer = 20          '最大行数(明細)

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0026TBLSORT As New CS0026TBLSORT                      '表示画面情報ソート
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

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
                    If Not Master.RecoverTable(CO0010tbl, WF_XMLsaveF.Value) Then
                        Exit Sub
                    End If

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
                        Case "WF_ViewDragStart"         'マウスドラッグ
                            WF_View_DragStart()
                        Case "WF_ViewMouseUp"           'マウスアップ
                            WF_VIEW_MouseUp()
                        Case "WF_ViewChange"            'ビュー切替
                            WF_VIEW_Change()
                        Case "WF_TABChange"             'タブ項目変更
                            WF_TAB_Change()
                        Case "WF_ViewHeadDBClick"       'ビュー項目ヘッダーダブルクリック
                            WF_VIEW_HEAD_DBClick()
                        Case "WF_ViewDBClick"           'ビュー項目ダブルクリック
                            WF_VIEW_DBClick()
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
            If Not IsNothing(CO0010tbl) Then
                CO0010tbl.Clear()
                CO0010tbl.Dispose()
                CO0010tbl = Nothing
            End If

            If Not IsNothing(CO0010INPtbl) Then
                CO0010INPtbl.Clear()
                CO0010INPtbl.Dispose()
                CO0010INPtbl = Nothing
            End If

            If Not IsNothing(CO0010UPDtbl) Then
                CO0010UPDtbl.Clear()
                CO0010UPDtbl.Dispose()
                CO0010UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRCO0010WRKINC.MAPID

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.CO0010S Then
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
        Master.SaveTable(CO0010tbl, WF_XMLsaveF.Value)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(CO0010tbl)

        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H'" _
            & " and LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
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

        If IsNothing(CO0010tbl) Then
            CO0010tbl = New DataTable
        End If

        If CO0010tbl.Columns.Count <> 0 Then
            CO0010tbl.Columns.Clear()
        End If

        CO0010tbl.Clear()

        '○ 検索SQL文
        '  検索説明
        '    ログインユーザーのPROFIDおよびデフォルトPROFIDのTBL(S0025_PROFMVIEW)を取得
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
            & "    , CAST(S025.UPDTIMSTP AS BIGINT)                AS TIMSTP" _
            & "    , 1                                             AS 'SELECT'" _
            & "    , 0                                             AS HIDDEN" _
            & "    , ISNULL(RTRIM(S025.CAMPCODE), '')              AS CAMPCODE" _
            & "    , ''                                            AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(S025.PROFID), '')                AS PROFID" _
            & "    , ISNULL(RTRIM(S025.MAPID), '')                 AS MAPID" _
            & "    , ''                                            AS MAPNAMES" _
            & "    , ISNULL(RTRIM(S025.VARIANT), '')               AS VARIANT" _
            & "    , ISNULL(RTRIM(S025.HDKBN), '')                 AS HDKBN" _
            & "    , ISNULL(RTRIM(S025.TITLEKBN), '')              AS TITLEKBN" _
            & "    , ISNULL(RTRIM(S025.TABID), '')                 AS TABID" _
            & "    , ISNULL(S025.POSIROW, 0)                       AS POSIROW" _
            & "    , ISNULL(S025.POSICOL, 0)                       AS POSICOL" _
            & "    , ISNULL(RTRIM(S025.FIELD), '')                 AS FIELD" _
            & "    , ISNULL(FORMAT(S025.STYMD, 'yyyy/MM/dd'), '')  AS STYMD" _
            & "    , ISNULL(FORMAT(S025.ENDYMD, 'yyyy/MM/dd'), '') AS ENDYMD" _
            & "    , ISNULL(RTRIM(S025.FIELDNAMES), '')            AS FIELDNAMES" _
            & "    , ISNULL(RTRIM(S025.FIELDNAMEL), '')            AS FIELDNAMEL" _
            & "    , ISNULL(RTRIM(S025.PREFIX), '')                AS PREFIX" _
            & "    , ISNULL(RTRIM(S025.SUFFIX), '')                AS SUFFIX" _
            & "    , ISNULL(S025.LENGTH, 0)                        AS LENGTH" _
            & "    , ISNULL(RTRIM(S025.ALIGN), '')                 AS ALIGN" _
            & "    , ISNULL(S025.SORTORDER, 0)                     AS SORTORDER" _
            & "    , ISNULL(RTRIM(S025.SORTKBN), '')               AS SORTKBN" _
            & "    , ISNULL(RTRIM(S025.EFFECT), '')                AS EFFECT" _
            & "    , ISNULL(S025.WIDTH, 0)                         AS WIDTH" _
            & "    , ISNULL(RTRIM(S025.OBJECTTYPE), '')            AS OBJECTTYPE" _
            & "    , ISNULL(RTRIM(S025.FORMATTYPE), '')            AS FORMATTYPE" _
            & "    , ISNULL(RTRIM(S025.FORMATVALUE), '')           AS FORMATVALUE" _
            & "    , ISNULL(RTRIM(S025.FIXCOL), '')                AS FIXCOL" _
            & "    , ISNULL(RTRIM(S025.REQUIRED), '')              AS REQUIRED" _
            & "    , ISNULL(RTRIM(S025.COLORSET), '')              AS COLORSET" _
            & "    , ISNULL(RTRIM(S025.ADDEVENT1), '')             AS ADDEVENT1" _
            & "    , ISNULL(RTRIM(S025.ADDFUNC1), '')              AS ADDFUNC1" _
            & "    , ISNULL(RTRIM(S025.ADDEVENT2), '')             AS ADDEVENT2" _
            & "    , ISNULL(RTRIM(S025.ADDFUNC2), '')              AS ADDFUNC2" _
            & "    , ISNULL(RTRIM(S025.ADDEVENT3), '')             AS ADDEVENT3" _
            & "    , ISNULL(RTRIM(S025.ADDFUNC3), '')              AS ADDFUNC3" _
            & "    , ISNULL(RTRIM(S025.ADDEVENT4), '')             AS ADDEVENT4" _
            & "    , ISNULL(RTRIM(S025.ADDFUNC4), '')              AS ADDFUNC4" _
            & "    , ISNULL(RTRIM(S025.ADDEVENT5), '')             AS ADDEVENT5" _
            & "    , ISNULL(RTRIM(S025.ADDFUNC5), '')              AS ADDFUNC5" _
            & "    , ISNULL(RTRIM(S025.DELFLG), '')                AS DELFLG" _
            & "    , ''                                            AS DELFLGNAMES" _
            & " FROM" _
            & "    S0025_PROFMVIEW S025" _
            & "    INNER JOIN MC001_FIXVALUE MC01" _
            & "        ON  MC01.CAMPCODE = @P1" _
            & "        AND MC01.CLASS    = @P3" _
            & "        AND MC01.KEYCODE  = S025.MAPID" _
            & "        AND MC01.STYMD   <= @P6" _
            & "        AND MC01.ENDYMD  >= @P6" _
            & "        AND MC01.DELFLG  <> @P7" _
            & " WHERE" _
            & "    S025.CAMPCODE    = @P1" _
            & "    AND S025.STYMD  <= @P4" _
            & "    AND S025.ENDYMD >= @P5" _
            & "    AND S025.DELFLG <> @P7"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '機能選択
        If work.WF_SEL_FUNCSEL.Text = GRCO0010WRKINC.C_LIST_FUNSEL_DEFAULT.VISIBLE Then
            SQLStr &= "    AND S025.PROFID IN ('" & C_DEFAULT_DATAKEY & "', @P2)"
        ElseIf work.WF_SEL_FUNCSEL.Text = GRCO0010WRKINC.C_LIST_FUNSEL_DEFAULT.INVISIBLE Then
            SQLStr &= "    AND S025.PROFID = @P2 "
        End If
        '画面ID(From)
        If Not String.IsNullOrEmpty(work.WF_SEL_MAPIDF.Text) Then
            SQLStr &= String.Format("    AND S025.MAPID  >= '{0}'", work.WF_SEL_MAPIDF.Text)
        End If
        '画面ID(To)
        If Not String.IsNullOrEmpty(work.WF_SEL_MAPIDT.Text) Then
            SQLStr &= String.Format("    AND S025.MAPID  <= '{0}'", work.WF_SEL_MAPIDT.Text)
        End If

        SQLStr &=
              " ORDER BY             " _
            & "    S025.CAMPCODE     " _
            & "    , S025.PROFID     " _
            & "    , S025.MAPID      " _
            & "    , S025.VARIANT    " _
            & "    , S025.HDKBN DESC " _
            & "    , S025.TITLEKBN   "

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
                PARA2.Value = Master.PROF_VIEW
                PARA3.Value = "CO0010_CO0011_MAPID"
                PARA4.Value = work.WF_SEL_ENDYMD.Text
                PARA5.Value = work.WF_SEL_STYMD.Text
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        CO0010tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    CO0010tbl.Load(SQLdr)
                End Using

                For Each CO0010row As DataRow In CO0010tbl.Rows
                    '名称取得
                    CODENAME_get("CAMPCODE", CO0010row("CAMPCODE"), CO0010row("CAMPNAMES"), WW_DUMMY)           '会社コード
                    CODENAME_get("MAPID", CO0010row("MAPID"), CO0010row("MAPNAMES"), WW_DUMMY)                  '画面ID
                    CODENAME_get("DELFLG", CO0010row("DELFLG"), CO0010row("DELFLGNAMES"), WW_DUMMY)             '削除
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0025_PROFMVIEW SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0025_PROFMVIEW Select"
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
        CS0026TBLSORT.TABLE = CO0010tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = "TITLEKBN = 'H'"
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            CO0010tbl = CS0026TBLSORT.TABLE
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
            ViewFormatDisplay()
            Exit Sub
        End If

        '○ 表示対象行カウント(絞り込み対象)
        '   ※ 絞込 (Cell(4) : 0=表示対象, 1=非表示対象)
        For Each CO0010row As DataRow In CO0010tbl.Rows
            If CO0010row("HIDDEN") = 0 AndAlso CO0010row("TITLEKBN") = "H" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                CO0010row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(CO0010tbl)

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
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
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
    ''' View表示一覧設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ViewFormatDisplay()

        If Not Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        Dim WW_HDKBN As String = ""
        Dim WW_TABID As String = ""
        Dim WW_MAXROW As Integer = 0
        Dim WW_MAXCOL As Integer = 0
        Dim VIEWtbl As New DataTable

        If WF_HDKBN_HEAD.Checked Then
            WW_HDKBN = "H"
            WW_TABID = ""
        End If
        If WF_HDKBN_DETAIL.Checked Then
            WW_HDKBN = "D"
            WW_TABID = WF_TABselect.SelectedValue
        End If

        '○ 項目選択リスト作成
        CS0026TBLSORT.TABLE = CO0010INPtbl
        CS0026TBLSORT.SORTING = "FIELD"
        CS0026TBLSORT.FILTER = "TITLEKBN <> 'H'" _
            & " and HDKBN = '" & WW_HDKBN & "'" _
            & " and TABID = '" & WW_TABID & "'" _
            & " and POSIROW = 0 and POSICOL = 0"
        CS0026TBLSORT.sort(VIEWtbl)

        '○ ListBoxはDragイベント未対応のため、Tableを使用し疑似ListBoxを作成する
        For Each VIEWrow As DataRow In VIEWtbl.Rows
            Dim ListRow = New TableRow With {.ViewStateMode = ViewStateMode.Disabled}
            Dim ListCell = New TableCell With {.ViewStateMode = ViewStateMode.Disabled}
            ListCell.Text = VIEWrow("FIELDNAMES")
            ListCell.ID = "WF_Rep_" & VIEWrow("FIELD")

            ListCell.Attributes.Add("draggable", "true")
            ListCell.Attributes.Add("onmousedown", "ViewMouseDown(0, 0, '" & VIEWrow("FIELD") & "');")
            ListCell.Attributes.Add("ondragstart", "ViewDragStart('" & VIEWrow("FIELD") & "');")

            If VIEWrow("FIELD") = WF_VIEW_SELECT.Value Then
                ListCell.Style.Add("background-color", "#1E90FF")
                ListCell.Style.Add("color", "#FFFFFF")
            End If

            ListRow.Cells.Add(ListCell)
            WF_VIEW_LIST.Rows.Add(ListRow)
        Next

        '○ 画面に表示する分のデータを抽出する
        CS0026TBLSORT.TABLE = CO0010INPtbl
        CS0026TBLSORT.SORTING = "POSIROW, POSICOL"
        CS0026TBLSORT.FILTER = "TITLEKBN <> 'H'" _
            & " and HDKBN = '" & WW_HDKBN & "'" _
            & " and TABID = '" & WW_TABID & "'"
        CS0026TBLSORT.sort(VIEWtbl)

        If WW_HDKBN = "H" Then
            WF_View_TAB.Visible = False
            WF_TABselect.Visible = False

            WW_MAXROW = CONST_VIEW_HEAD_MAXROW
            WW_MAXCOL = VIEWtbl.Rows.Count + 10

            WF_FIXCOL.Enabled = True           '固定列
            WF_FIXCOL.Style.Remove("background-color")
            WF_FIXCOL.Style.Add("background-color", "rgb(255, 255, 170)")
            WF_EFFECT.Enabled = False          '入力有無
            WF_EFFECT.Style.Remove("background-color")
            WF_EFFECT.Style.Add("background-color", "lightgray")
        End If

        If WW_HDKBN = "D" Then
            WF_View_TAB.Visible = True
            WF_TABselect.Visible = True

            WW_MAXROW = CONST_VIEW_DETAIL_MAXROW
            WW_MAXCOL = 3

            '明細行6列判定
            CODENAME_get("COL6", WF_MAPID.Text, WW_DUMMY, WW_RTN_SW)
            If isNormal(WW_RTN_SW) Then
                WW_MAXCOL = 6
            End If

            WF_FIXCOL.Enabled = False          '固定列
            WF_FIXCOL.Style.Remove("background-color")
            WF_FIXCOL.Style.Add("background-color", "lightgray")
            WF_EFFECT.Enabled = True           '入力有無
            WF_EFFECT.Style.Remove("background-color")
            WF_EFFECT.Style.Add("background-color", "rgb(255, 255, 170)")
        End If

        '○ 画面表示枠へ実データをセット
        Try
            '○ ヘッダー部作成
            Dim HeadRow = New TableHeaderRow With {.ViewStateMode = ViewStateMode.Disabled}
            For col As Integer = 1 To WW_MAXCOL             '列
                Dim HeadCell = New TableHeaderCell With {.ViewStateMode = ViewStateMode.Disabled}

                HeadCell.Text = col.ToString()
                HeadCell.Attributes.Remove("ondblclick")
                HeadCell.Attributes.Add("ondblclick", "ViewHeadDBClick(" & col & ");")
                HeadRow.Cells.Add(HeadCell)
            Next

            WF_VIEW.Rows.Add(HeadRow)

            '○ 明細部作成
            For row As Integer = 1 To WW_MAXROW             '行
                Dim DetailRow = New TableRow With {.ViewStateMode = ViewStateMode.Disabled}

                For col As Integer = 1 To WW_MAXCOL         '列
                    Dim DetailCell = New TableCell With {.ViewStateMode = ViewStateMode.Disabled}
                    Dim cellTextBox = New TextBox With {.ReadOnly = True}

                    cellTextBox.ID = "WF_Rep_" & col.ToString() & "_" & row.ToString()

                    'ダブルクリックイベント追加
                    cellTextBox.Attributes.Add("ondblclick", "ViewDBClick(" & col & ", " & row & ");")

                    'マウスダウン、マウスアップイベント追加
                    cellTextBox.Attributes.Add("draggable", "true")
                    cellTextBox.Attributes.Add("onmousedown", "ViewMouseDown(" & col & ", " & row & ", '');")
                    cellTextBox.Attributes.Add("ondragstart", "ViewDragStart('');")
                    cellTextBox.Attributes.Add("onmouseup", "ViewMouseUp(" & col & ", " & row & ");")

                    '項目セット
                    For Each VIEWrow As DataRow In VIEWtbl.Rows
                        If VIEWrow("POSIROW") = If(WW_HDKBN = "D", row, 0) AndAlso
                            VIEWrow("POSICOL") = col Then
                            cellTextBox.Text = VIEWrow("FIELDNAMES")
                            Exit For
                        End If
                    Next

                    DetailCell.Controls.Add(cellTextBox)
                    DetailRow.Cells.Add(DetailCell)
                Next

                WF_VIEW.Rows.Add(DetailRow)
            Next

            '○ 背景色設定
            Dim WW_ITEM_NAME = "WF_Rep_" & WF_VIEW_COL.Value & "_" & WF_VIEW_ROW.Value
            CType(WF_VIEW.FindControl(WW_ITEM_NAME), TextBox).Style.Add("background-color", "rgb(220, 230, 240)")
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CAST_FORMAT_ERROR_EX, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Finally
            VIEWtbl.Clear()
            VIEWtbl.Dispose()
            VIEWtbl = Nothing
        End Try

    End Sub


    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.EraseCharToIgnore(WF_SELMAPID.Text)

        '○ 名称取得
        CODENAME_get("MAPID", WF_SELMAPID.Text, WF_SELMAPID_TEXT.Text, WW_DUMMY)

        '○ 絞り込み操作(GridView明細Hidden設定)
        For Each CO0010row As DataRow In CO0010tbl.Rows

            '一度非表示にする
            CO0010row("HIDDEN") = 1

            Dim WW_HANTEI As Boolean = True

            '画面IDによる絞込判定
            If WF_SELMAPID.Text <> "" AndAlso
                WF_SELMAPID.Text <> CO0010row("MAPID") Then
                WW_HANTEI = False
            End If

            '画面(GridView)のHIDDENに結果格納
            If WW_HANTEI Then
                CO0010row("HIDDEN") = 0
            End If
        Next

        '○ 画面先頭を表示
        WF_GridPosition.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(CO0010tbl, WF_XMLsaveF.Value)

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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

                'プロファイルマスタ(ビュー)更新
                UpdateProfileMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0010tbl, WF_XMLsaveF.Value)

        '○ 詳細画面クリア
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
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
        For Each CO0010row As DataRow In CO0010tbl.Rows

            '読み飛ばし
            If (CO0010row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                CO0010row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                CO0010row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                CO0010row("STYMD") = "" Then
                Continue For
            End If

            WW_LINE_ERR = ""

            'チェック
            For Each CO0010chk As DataRow In CO0010tbl.Rows

                '同一KEY以外は読み飛ばし
                If CO0010row("CAMPCODE") <> CO0010chk("CAMPCODE") OrElse
                    CO0010row("PROFID") <> CO0010chk("PROFID") OrElse
                    CO0010row("MAPID") <> CO0010chk("MAPID") OrElse
                    CO0010row("VARIANT") <> CO0010chk("VARIANT") OrElse
                    CO0010row("HDKBN") <> CO0010chk("HDKBN") OrElse
                    CO0010row("FIELD") <> CO0010chk("FIELD") OrElse
                    CO0010chk("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If CO0010row("STYMD") = CO0010chk("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(CO0010row("STYMD"), WW_DATE_ST)
                    Date.TryParse(CO0010row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(CO0010chk("STYMD"), WW_DATE_ST2)
                    Date.TryParse(CO0010chk("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End Try

                '開始日チェック
                If WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", CO0010row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If

                '終了日チェック
                If WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", CO0010row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If
            Next

            If WW_LINE_ERR = "" Then
                CO0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0010row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
            & "        S0025_PROFMVIEW" _
            & "    WHERE" _
            & "        CAMPCODE    = @P1" _
            & "        AND PROFID  = @P2" _
            & "        AND MAPID   = @P3" _
            & "        AND VARIANT = @P4" _
            & "        AND HDKBN   = @P5" _
            & "        AND FIELD   = @P10" _
            & "        AND STYMD   = @P11 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE S0025_PROFMVIEW" _
            & "    SET" _
            & "        TITLEKBN     = @P6     , TABID       = @P7" _
            & "        , POSIROW    = @P8     , POSICOL     = @P9" _
            & "        , ENDYMD     = @P12    , FIELDNAMES  = @P13" _
            & "        , FIELDNAMEL = @P14    , PREFIX      = @P15" _
            & "        , SUFFIX     = @P16    , LENGTH      = @P17" _
            & "        , ALIGN      = @P18    , SORTORDER   = @P19" _
            & "        , SORTKBN    = @P20    , EFFECT      = @P21" _
            & "        , WIDTH      = @P22    , OBJECTTYPE  = @P23" _
            & "        , FORMATTYPE = @P24    , FORMATVALUE = @P25" _
            & "        , FIXCOL     = @P26    , REQUIRED    = @P27" _
            & "        , COLORSET   = @P28    , ADDEVENT1   = @P29" _
            & "        , ADDFUNC1   = @P30    , ADDEVENT2   = @P31" _
            & "        , ADDFUNC2   = @P32    , ADDEVENT3   = @P33" _
            & "        , ADDFUNC3   = @P34    , ADDEVENT4   = @P35" _
            & "        , ADDFUNC4   = @P36    , ADDEVENT5   = @P37" _
            & "        , ADDFUNC5   = @P38    , DELFLG      = @P39" _
            & "        , UPDYMD     = @P41    , UPDUSER     = @P42" _
            & "        , UPDTERMID  = @P43    , RECEIVEYMD  = @P44" _
            & "    WHERE" _
            & "        CAMPCODE    = @P1" _
            & "        AND PROFID  = @P2" _
            & "        AND MAPID   = @P3" _
            & "        AND VARIANT = @P4" _
            & "        AND HDKBN   = @P5" _
            & "        AND FIELD   = @P10" _
            & "        AND STYMD   = @P11 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO S0025_PROFMVIEW" _
            & "        (CAMPCODE        , PROFID        , MAPID" _
            & "        , VARIANT        , HDKBN         , TITLEKBN" _
            & "        , TABID          , POSIROW       , POSICOL" _
            & "        , FIELD          , STYMD         , ENDYMD" _
            & "        , FIELDNAMES     , FIELDNAMEL    , PREFIX" _
            & "        , SUFFIX         , LENGTH        , ALIGN" _
            & "        , SORTORDER      , SORTKBN       , EFFECT" _
            & "        , WIDTH          , OBJECTTYPE    , FORMATTYPE" _
            & "        , FORMATVALUE    , FIXCOL        , REQUIRED" _
            & "        , COLORSET       , ADDEVENT1     , ADDFUNC1" _
            & "        , ADDEVENT2      , ADDFUNC2      , ADDEVENT3" _
            & "        , ADDFUNC3       , ADDEVENT4     , ADDFUNC4" _
            & "        , ADDEVENT5      , ADDFUNC5      , DELFLG" _
            & "        , INITYMD        , UPDYMD        , UPDUSER" _
            & "        , UPDTERMID      , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2     , @P3" _
            & "        , @P4     , @P5     , @P6" _
            & "        , @P7     , @P8     , @P9" _
            & "        , @P10    , @P11    , @P12" _
            & "        , @P13    , @P14    , @P15" _
            & "        , @P16    , @P17    , @P18" _
            & "        , @P19    , @P20    , @P21" _
            & "        , @P22    , @P23    , @P24" _
            & "        , @P25    , @P26    , @P27" _
            & "        , @P28    , @P29    , @P30" _
            & "        , @P31    , @P32    , @P33" _
            & "        , @P34    , @P35    , @P36" _
            & "        , @P37    , @P38    , @P39" _
            & "        , @P40    , @P41    , @P42" _
            & "        , @P43    , @P44) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , PROFID" _
            & "    , MAPID" _
            & "    , VARIANT" _
            & "    , HDKBN" _
            & "    , TITLEKBN" _
            & "    , TABID" _
            & "    , POSIROW" _
            & "    , POSICOL" _
            & "    , FIELD" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , FIELDNAMES" _
            & "    , FIELDNAMEL" _
            & "    , PREFIX" _
            & "    , SUFFIX" _
            & "    , LENGTH" _
            & "    , ALIGN" _
            & "    , SORTORDER" _
            & "    , SORTKBN" _
            & "    , EFFECT" _
            & "    , WIDTH" _
            & "    , OBJECTTYPE" _
            & "    , FORMATTYPE" _
            & "    , FORMATVALUE" _
            & "    , FIXCOL" _
            & "    , REQUIRED" _
            & "    , COLORSET" _
            & "    , ADDEVENT1" _
            & "    , ADDFUNC1" _
            & "    , ADDEVENT2" _
            & "    , ADDFUNC2" _
            & "    , ADDEVENT3" _
            & "    , ADDFUNC3" _
            & "    , ADDEVENT4" _
            & "    , ADDFUNC4" _
            & "    , ADDEVENT5" _
            & "    , ADDFUNC5" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    S0025_PROFMVIEW" _
            & " WHERE" _
            & "    CAMPCODE    = @P1" _
            & "    AND PROFID  = @P2" _
            & "    AND MAPID   = @P3" _
            & "    AND VARIANT = @P4" _
            & "    AND HDKBN   = @P5" _
            & "    AND FIELD   = @P6" _
            & "    AND STYMD   = @P7"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            'プロファイルID
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 50)            '画面ID
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 50)            '変数
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)             'ヘッダー・ディテイル区分
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)             'タイトル区分
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 20)            'タブID
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Int)                     '行位置
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.Int)                     '列位置
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 50)          '項目
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.Date)                  '開始年月日
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Date)                  '終了年月日
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)          '項目名称（短）
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 50)          '項目名称（長）
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 50)          '接頭句
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 50)          '接尾句
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.Int)                   '入力可能数
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)          '文字揃え
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Int)                   '並び順
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 10)          '昇降区分
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 1)           '表示有無
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Float)                 '横幅
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)           'オブジェクトタイプ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 20)          'フォーマットタイプ
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 20)          'フォーマット書式
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 1)           '固定列
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 1)           '入力必須
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.NVarChar, 1)           '色設定
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 20)          '追加イベント１
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 20)          '追加ファンクション１
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.NVarChar, 20)          '追加イベント２
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 20)          '追加ファンクション２
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 20)          '追加イベント３
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.NVarChar, 20)          '追加ファンクション３
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.NVarChar, 20)          '追加イベント４
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.NVarChar, 20)          '追加ファンクション４
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.NVarChar, 20)          '追加イベント５
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.NVarChar, 20)          '追加ファンクション５
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.DateTime)              '登録年月日
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.DateTime)              '更新年月日
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'プロファイルID
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 50)        '画面ID
                Dim JPARA4 As SqlParameter = SQLcmdJnl.Parameters.Add("@P4", SqlDbType.NVarChar, 50)        '変数
                Dim JPARA5 As SqlParameter = SQLcmdJnl.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         'ヘッダー・ディテイル区分
                Dim JPARA6 As SqlParameter = SQLcmdJnl.Parameters.Add("@P6", SqlDbType.NVarChar, 50)        '項目
                Dim JPARA7 As SqlParameter = SQLcmdJnl.Parameters.Add("@P7", SqlDbType.Date)                '開始年月日

                For Each CO0010row As DataRow In CO0010tbl.Rows
                    If Trim(CO0010row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(CO0010row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = CO0010row("CAMPCODE")
                        PARA2.Value = CO0010row("PROFID")
                        PARA3.Value = CO0010row("MAPID")
                        PARA4.Value = CO0010row("VARIANT")
                        PARA5.Value = CO0010row("HDKBN")
                        PARA6.Value = CO0010row("TITLEKBN")
                        PARA7.Value = CO0010row("TABID")
                        PARA8.Value = CO0010row("POSIROW")
                        PARA9.Value = CO0010row("POSICOL")
                        PARA10.Value = CO0010row("FIELD")
                        PARA11.Value = CO0010row("STYMD")
                        PARA12.Value = CO0010row("ENDYMD")
                        PARA13.Value = CO0010row("FIELDNAMES")
                        PARA14.Value = CO0010row("FIELDNAMEL")
                        PARA15.Value = CO0010row("PREFIX")
                        PARA16.Value = CO0010row("SUFFIX")
                        PARA17.Value = CO0010row("LENGTH")
                        PARA18.Value = CO0010row("ALIGN")
                        PARA19.Value = CO0010row("SORTORDER")
                        PARA20.Value = CO0010row("SORTKBN")
                        PARA21.Value = CO0010row("EFFECT")
                        PARA22.Value = CO0010row("WIDTH")
                        PARA23.Value = CO0010row("OBJECTTYPE")
                        PARA24.Value = CO0010row("FORMATTYPE")
                        PARA25.Value = CO0010row("FORMATVALUE")
                        PARA26.Value = CO0010row("FIXCOL")
                        PARA27.Value = CO0010row("REQUIRED")
                        PARA28.Value = CO0010row("COLORSET")
                        PARA29.Value = CO0010row("ADDEVENT1")
                        PARA30.Value = CO0010row("ADDFUNC1")
                        PARA31.Value = CO0010row("ADDEVENT2")
                        PARA32.Value = CO0010row("ADDFUNC2")
                        PARA33.Value = CO0010row("ADDEVENT3")
                        PARA34.Value = CO0010row("ADDFUNC3")
                        PARA35.Value = CO0010row("ADDEVENT4")
                        PARA36.Value = CO0010row("ADDFUNC4")
                        PARA37.Value = CO0010row("ADDEVENT5")
                        PARA38.Value = CO0010row("ADDFUNC5")
                        PARA39.Value = CO0010row("DELFLG")
                        PARA40.Value = WW_DATENOW
                        PARA41.Value = WW_DATENOW
                        PARA42.Value = Master.USERID
                        PARA43.Value = Master.USERTERMID
                        PARA44.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = CO0010row("CAMPCODE")
                        JPARA2.Value = CO0010row("PROFID")
                        JPARA3.Value = CO0010row("MAPID")
                        JPARA4.Value = CO0010row("VARIANT")
                        JPARA5.Value = CO0010row("HDKBN")
                        JPARA6.Value = CO0010row("FIELD")
                        JPARA7.Value = CO0010row("STYMD")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(CO0010UPDtbl) Then
                                CO0010UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    CO0010UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            CO0010UPDtbl.Clear()
                            CO0010UPDtbl.Load(SQLdr)
                        End Using

                        For Each CO0010UPDrow As DataRow In CO0010UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "S0025_PROFMVIEW"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = CO0010UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0025_PROFMVIEW UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0025_PROFMVIEW UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

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
        Dim TBLview As New DataView(CO0010tbl)
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

        Master.CreateEmptyTable(CO0010INPtbl, WF_XMLsaveF.Value)

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try

        For i As Integer = 0 To CO0010tbl.Rows.Count - 1
            If CO0010tbl.Rows(i)("LINECNT") = WW_LINECNT Then
                WW_LINECNT = i
                Exit For
            End If
        Next

        '選択行
        WF_Sel_LINECNT.Text = CO0010tbl.Rows(WW_LINECNT)("LINECNT")

        '会社コード
        WF_CAMPCODE.Text = CO0010tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        'プロフID
        WF_PROFID.Text = CO0010tbl.Rows(WW_LINECNT)("PROFID")

        '画面ID
        WF_MAPID.Text = CO0010tbl.Rows(WW_LINECNT)("MAPID")
        CODENAME_get("MAPID", WF_MAPID.Text, WF_MAPID_TEXT.Text, WW_DUMMY)

        '画面識別ｺｰﾄﾞ(変数)
        WF_VARIANT.Text = CO0010tbl.Rows(WW_LINECNT)("VARIANT")

        '画面識別名(変数)
        WF_VARIANTNAMES.Text = CO0010tbl.Rows(WW_LINECNT)("FIELDNAMES")

        '有効年月日
        WF_STYMD.Text = CO0010tbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = CO0010tbl.Rows(WW_LINECNT)("ENDYMD")

        '削除
        WF_DELFLG.Text = CO0010tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        'View切替
        WF_HDKBN_HEAD.Checked = True
        WF_HDKBN_DETAIL.Checked = False

        '行列追加削除
        WF_ROW.Checked = True
        WF_COL.Checked = False

        WF_HDKBN.Value = "H"
        WF_TABID.Value = ""

        CS0026TBLSORT.TABLE = CO0010tbl
        CS0026TBLSORT.SORTING = "TITLEKBN, HDKBN DESC, TABID, POSIROW, POSICOL, FIELD"
        CS0026TBLSORT.FILTER = "CAMPCODE = '" & WF_CAMPCODE.Text & "'" _
            & " and PROFID = '" & WF_PROFID.Text & "'" _
            & " and MAPID = '" & WF_MAPID.Text & "'" _
            & " and VARIANT = '" & WF_VARIANT.Text & "'" _
            & " and STYMD = '" & WF_STYMD.Text & "'"
        CS0026TBLSORT.sort(CO0010INPtbl)
        Master.SaveTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        '○ タブリスト作成
        WF_TABselect.Items.Clear()
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            If CO0010INProw("HDKBN") = "D" AndAlso
                WF_TABselect.Items.IndexOf(New ListItem(CO0010INProw("TABID"), CO0010INProw("TABID"))) < 0 Then
                WF_TABselect.Items.Add(New ListItem(CO0010INProw("TABID"), CO0010INProw("TABID")))
                WF_TABselect.SelectedIndex = 0
            End If
        Next

        '○ 1行1列目選択
        WF_VIEW_SELECT.Value = ""
        WF_VIEW_ROW.Value = 1
        WF_VIEW_COL.Value = 1
        WW_ViewSelectDetail()

        WF_List_Top.Value = 0
        WF_Scroll_Left.Value = 0
        WF_Scroll_Top.Value = 0

        '○ 状態をクリア
        For Each CO0010row As DataRow In CO0010tbl.Rows
            Select Case CO0010row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case CO0010tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                CO0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                CO0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                CO0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                CO0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                CO0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(CO0010tbl, WF_XMLsaveF.Value)

        WF_VARIANT.Focus()
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

        Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)              '会社コード
        Master.EraseCharToIgnore(WF_PROFID.Text)                'プロフID
        Master.EraseCharToIgnore(WF_MAPID.Text)                 '画面ID
        Master.EraseCharToIgnore(WF_VARIANT.Text)               '画面識別ｺｰﾄﾞ(変数)
        Master.EraseCharToIgnore(WF_VARIANTNAMES.Text)          '画面識別名(変数)
        Master.EraseCharToIgnore(WF_STYMD.Text)                 '開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)                '終了年月日
        Master.EraseCharToIgnore(WF_DELFLG.Text)                '削除フラグ

        '○ ヘッダー情報をテーブルに反映
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            CO0010INProw("CAMPCODE") = WF_CAMPCODE.Text
            CO0010INProw("PROFID") = WF_PROFID.Text
            CO0010INProw("MAPID") = WF_MAPID.Text
            CO0010INProw("VARIANT") = WF_VARIANT.Text

            If CO0010INProw("TITLEKBN") = "H" Then
                CO0010INProw("FIELDNAMES") = WF_VARIANTNAMES.Text
                CO0010INProw("FIELDNAMEL") = WF_VARIANTNAMES.Text
            End If

            CO0010INProw("STYMD") = WF_STYMD.Text
            If WF_ENDYMD.Text = "" Then
                CO0010INProw("ENDYMD") = WF_STYMD.Text
            Else
                CO0010INProw("ENDYMD") = WF_ENDYMD.Text
            End If

            If WF_DELFLG.Text = "" Then
                CO0010INProw("DELFLG") = C_DELETE_FLG.ALIVE
            Else
                CO0010INProw("DELFLG") = WF_DELFLG.Text
            End If
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            CO0010tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0010tbl, WF_XMLsaveF.Value)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()

            Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            WF_SELMAPID.Focus()
            WF_DISP.Value = "headerbox"
        Else
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
            WF_VARIANT.Focus()
            WF_DISP.Value = "detailbox"
        End If

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        Dim WW_HDKBN As String = ""
        Dim WW_TABID As String = ""

        If WF_HDKBN_HEAD.Checked Then
            WW_HDKBN = "H"
            WW_TABID = ""
        End If
        If WF_HDKBN_DETAIL.Checked Then
            WW_HDKBN = "D"
            WW_TABID = WF_TABselect.SelectedValue
        End If

        WF_ROW.Checked = True
        WF_COL.Checked = False

        WF_List_Top.Value = 0
        WF_Scroll_Left.Value = 0
        WF_Scroll_Top.Value = 0

        '○ 明細項目クリア
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_VARIANT.Text = ""
        WF_VARIANTNAMES.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG.Text = C_DELETE_FLG.ALIVE
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        WF_VIEW_SELECT.Value = ""
        WF_VIEW_ROW.Value = 1
        WF_VIEW_COL.Value = 1
        WF_FIELD_VIEW.Value = ""

        WF_WIDTH.Text = "0"
        WF_ALIGN.Text = "left"
        CODENAME_get("ALIGN", WF_ALIGN.Text, WF_ALIGN_TEXT.Text, WW_DUMMY)
        WF_SORT.Text = "0"
        WF_SORTKBN.Text = ""
        WF_SORTKBN_TEXT.Text = ""
        WF_FIELDNAMES.Text = ""
        WF_LENGTH.Text = "0"
        WF_FIXCOL.Text = ""
        WF_FIXCOL_TEXT.Text = ""
        WF_EFFECT.Text = "N"
        CODENAME_get("EFFECT", WF_EFFECT.Text, WF_EFFECT_TEXT.Text, WW_DUMMY)

        '○ CO0010INP項目クリア
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            If CO0010INProw("TITLEKBN") = "H" Then
                Continue For
            End If

            If CO0010INProw("HDKBN") <> WW_HDKBN OrElse
                CO0010INProw("TABID") <> WW_TABID Then
                Continue For
            End If

            CO0010INProw("POSIROW") = 0
            CO0010INProw("POSICOL") = 0
            CO0010INProw("LENGTH") = 0
            CO0010INProw("ALIGN") = "left"
            CO0010INProw("SORTORDER") = 0
            CO0010INProw("SORTKBN") = ""
            CO0010INProw("EFFECT") = "N"
            CO0010INProw("WIDTH") = 0
            CO0010INProw("FIXCOL") = ""
        Next

        Master.SaveTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        WW_ViewSelectDetail()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_VARIANT.Focus()
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
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        For Each CO0010row As DataRow In CO0010tbl.Rows
            Select Case CO0010row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(CO0010tbl, WF_XMLsaveF.Value)

        WF_Sel_LINECNT.Text = ""                            'LINECNT
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_PROFID.Text = ""                                 'プロフID
        WF_MAPID.Text = ""                                  '画面ID
        WF_MAPID_TEXT.Text = ""                             '画面名
        WF_VARIANT.Text = ""                                '画面識別ｺｰﾄﾞ(変数)
        WF_VARIANTNAMES.Text = ""                           '画面識別名(変数)
        WF_STYMD.Text = ""                                  '有効年月日(From)
        WF_ENDYMD.Text = ""                                 '有効年月日(To)
        WF_DELFLG.Text = ""                                 '削除
        WF_DELFLG_TEXT.Text = ""                            '削除名称

        WF_VIEW_SELECT.Value = ""                           '項目選択リスト
        WF_TABselect.Items.Clear()                          'タブ選択リスト

        WF_HDKBN.Value = ""
        WF_TABID.Value = ""

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
                            Case "WF_ALIGN"             '文字揃え
                                prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "ALIGN"
                            Case "WF_SORTKBN"           '昇降区分
                                prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SORTKBN"
                            Case "WF_FIXCOL"            '固定列
                                prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "FIXCOL"
                            Case "WF_EFFECT"            '入力有無
                                prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "EFFECT"
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

        Dim WW_HDKBN As String = ""
        Dim WW_TABID As String = ""
        Dim WW_MAXROW As Integer = 0
        Dim WW_MAXCOL As Integer = 0
        Dim VIEWtbl As New DataTable

        If Not Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        If WF_HDKBN_HEAD.Checked Then
            WW_HDKBN = "H"
            WW_TABID = ""
        End If
        If WF_HDKBN_DETAIL.Checked Then
            WW_HDKBN = "D"
            WW_TABID = WF_TABselect.SelectedValue
        End If

        '○ 項目選択リスト作成
        CS0026TBLSORT.TABLE = CO0010INPtbl
        CS0026TBLSORT.SORTING = "POSIROW, POSICOL"
        CS0026TBLSORT.FILTER = "TITLEKBN <> 'H'" _
            & " and HDKBN = '" & WW_HDKBN & "'" _
            & " and TABID = '" & WW_TABID & "'"
        CS0026TBLSORT.sort(VIEWtbl)

        If WW_HDKBN = "H" Then
            WW_MAXROW = 0
            WW_MAXCOL = VIEWtbl.Rows.Count + 10
        End If
        If WW_HDKBN = "D" Then
            WW_MAXROW = CONST_VIEW_DETAIL_MAXROW
            WW_MAXCOL = 3

            '明細6列判定
            CODENAME_get("COL6", WF_MAPID.Text, WW_DUMMY, WW_RTN_SW)
            If isNormal(WW_RTN_SW) Then
                WW_MAXCOL = 6
            End If
        End If

        VIEWtbl.Clear()
        VIEWtbl.Dispose()
        VIEWtbl = Nothing

        '○ 選択箇所の最大の行 or 列が埋まっている場合エラー
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            If CO0010INProw("TITLEKBN") <> "H" AndAlso
                CO0010INProw("HDKBN") = WW_HDKBN AndAlso
                CO0010INProw("TABID") = WW_TABID Then
                If WF_ROW.Checked AndAlso
                    CO0010INProw("POSIROW") >= WW_MAXROW AndAlso
                    CO0010INProw("POSICOL") = WF_VIEW_COL.Value Then
                    rightview.addErrorReport("・Viewイメージに項目を挿入する事が出来ません。")
                    Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End If

                If WF_COL.Checked AndAlso
                    CO0010INProw("POSIROW") = If(WW_HDKBN = "D", WF_VIEW_ROW.Value, 0) AndAlso
                    CO0010INProw("POSICOL") >= WW_MAXCOL Then
                    rightview.addErrorReport("・Viewイメージに項目を挿入する事が出来ません。")
                    Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End If
            End If
        Next

        '○ 項目ずらし
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            If CO0010INProw("TITLEKBN") <> "H" AndAlso
                CO0010INProw("HDKBN") = WW_HDKBN AndAlso
                CO0010INProw("TABID") = WW_TABID Then
                '選択箇所以降の行を下に移動する
                If WF_ROW.Checked AndAlso
                    CO0010INProw("POSIROW") >= If(WW_HDKBN = "D", WF_VIEW_ROW.Value, 0) AndAlso
                    CO0010INProw("POSICOL") = WF_VIEW_COL.Value Then
                    CO0010INProw("POSIROW") = CO0010INProw("POSIROW") + 1
                End If

                '選択箇所以降の列を右に移動する
                If WF_COL.Checked AndAlso
                    CO0010INProw("POSIROW") = If(WW_HDKBN = "D", WF_VIEW_ROW.Value, 0) AndAlso
                    CO0010INProw("POSICOL") >= WF_VIEW_COL.Value Then
                    CO0010INProw("POSICOL") = CO0010INProw("POSICOL") + 1
                End If
            End If
        Next

        Master.SaveTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        WW_ViewSelectDetail()

        WF_DISP.Value = "detailbox"
        WF_VIEW_SELECT.Value = ""

    End Sub


    ''' <summary>
    ''' 削除ボタンクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_DELETE_Click()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        Dim WW_HDKBN As String = ""
        Dim WW_TABID As String = ""

        If Not Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        Master.EraseCharToIgnore(WF_FIELDNAMES.Text)

        If WF_HDKBN_HEAD.Checked Then
            WW_HDKBN = "H"
            WW_TABID = ""
        End If
        If WF_HDKBN_DETAIL.Checked Then
            WW_HDKBN = "D"
            WW_TABID = WF_TABselect.SelectedValue
        End If

        '○ 選択箇所削除
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            If CO0010INProw("TITLEKBN") <> "H" AndAlso
                CO0010INProw("HDKBN") = WW_HDKBN AndAlso
                CO0010INProw("TABID") = WW_TABID Then
                '選択箇所を削除する
                If CO0010INProw("POSIROW") = If(WW_HDKBN = "D", WF_VIEW_ROW.Value, 0) AndAlso
                    CO0010INProw("POSICOL") = WF_VIEW_COL.Value Then
                    CO0010INProw("POSIROW") = 0
                    CO0010INProw("POSICOL") = 0
                    CO0010INProw("FIELDNAMES") = WF_FIELDNAMES.Text
                    CO0010INProw("ALIGN") = "left"
                    CO0010INProw("SORTORDER") = 0
                    CO0010INProw("SORTKBN") = ""
                    CO0010INProw("EFFECT") = "N"
                    CO0010INProw("FIXCOL") = ""
                End If

                '選択箇所以降の行を上に詰める
                If WF_ROW.Checked Then
                    If CO0010INProw("POSIROW") > If(WW_HDKBN = "D", WF_VIEW_ROW.Value, 0) AndAlso
                        CO0010INProw("POSICOL") = WF_VIEW_COL.Value Then
                        CO0010INProw("POSIROW") = CO0010INProw("POSIROW") - 1
                    End If
                End If

                '選択箇所以降の列を左に詰める
                If WF_COL.Checked Then
                    If CO0010INProw("POSIROW") = If(WW_HDKBN = "D", WF_VIEW_ROW.Value, 0) AndAlso
                        CO0010INProw("POSICOL") > WF_VIEW_COL.Value Then
                        CO0010INProw("POSICOL") = CO0010INProw("POSICOL") - 1
                    End If
                End If
            End If
        Next

        Master.SaveTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        WW_ViewSelectDetail()

        WF_DISP.Value = "detailbox"
        WF_VIEW_SELECT.Value = ""

    End Sub


    ''' <summary>
    ''' マウス移動開始時処理
    ''' </summary>
    Protected Sub WF_View_DragStart()

        If Not Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        WF_VIEW_END_ROW.Value = ""
        WF_VIEW_END_COL.Value = ""

        If String.IsNullOrEmpty(WF_VIEW_START_ROW.Value) OrElse
            String.IsNullOrEmpty(WF_VIEW_START_COL.Value) Then
            WF_VIEW_START_ROW.Value = ""
            WF_VIEW_START_COL.Value = ""
            Exit Sub
        End If

        If WF_VIEW_START_ROW.Value = "0" OrElse
            WF_VIEW_START_COL.Value = "0" Then
            Exit Sub
        End If

        WF_VIEW_ROW.Value = WF_VIEW_START_ROW.Value
        WF_VIEW_COL.Value = WF_VIEW_START_COL.Value
        WW_ViewSelectDetail()

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"

    End Sub


    ''' <summary>
    ''' ビュー項目移動(マウスによる移動)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_VIEW_MouseUp()

        If Not Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value) Then
            Exit Sub
        End If

        If String.IsNullOrEmpty(WF_VIEW_START_ROW.Value) OrElse
            String.IsNullOrEmpty(WF_VIEW_START_COL.Value) OrElse
            String.IsNullOrEmpty(WF_VIEW_END_ROW.Value) OrElse
            String.IsNullOrEmpty(WF_VIEW_END_COL.Value) Then
            WF_VIEW_START_ROW.Value = ""
            WF_VIEW_START_COL.Value = ""
            WF_VIEW_END_ROW.Value = ""
            WF_VIEW_END_COL.Value = ""
            Exit Sub
        End If

        If WF_VIEW_START_ROW.Value = WF_VIEW_END_ROW.Value AndAlso
            WF_VIEW_START_COL.Value = WF_VIEW_END_COL.Value Then
            WF_VIEW_START_ROW.Value = ""
            WF_VIEW_START_COL.Value = ""
            WF_VIEW_END_ROW.Value = ""
            WF_VIEW_END_COL.Value = ""
            Exit Sub
        End If

        Dim WW_ERR As String = ""
        Dim WW_HDKBN As String = ""
        Dim WW_TABID As String = ""

        If WF_HDKBN_HEAD.Checked Then
            WW_HDKBN = "H"
            WW_TABID = ""
        End If
        If WF_HDKBN_DETAIL.Checked Then
            WW_HDKBN = "D"
            WW_TABID = WF_TABselect.SelectedValue
        End If

        If WF_VIEW_START_ROW.Value = "0" OrElse
            WF_VIEW_START_COL.Value = "0" Then
            If WF_VIEW_SELECT.Value = "" Then
                rightview.addErrorReport("・選択可能項目を選択 or Viewイメージの選択 が必要です。")
                Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        End If

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        '○ 移動前と移動先の項目を取得
        Dim CO0010STRrow As DataRow = Nothing
        Dim CO0010ENDrow As DataRow = Nothing

        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            If CO0010INProw("TITLEKBN") <> "H" AndAlso
                CO0010INProw("HDKBN") = WW_HDKBN AndAlso
                CO0010INProw("TABID") = WW_TABID Then
                '移動前の項目を取得
                If CO0010INProw("POSIROW") = If(WW_HDKBN = "D", WF_VIEW_START_ROW.Value, 0) AndAlso
                    CO0010INProw("POSICOL") = WF_VIEW_START_COL.Value Then
                    If WF_VIEW_START_ROW.Value = "0" AndAlso WF_VIEW_START_COL.Value = "0" Then
                        If CO0010INProw("FIELD") = WF_VIEW_SELECT.Value Then
                            CO0010STRrow = CO0010INPtbl.NewRow
                            CO0010STRrow.ItemArray = CO0010INProw.ItemArray
                        End If
                    Else
                        CO0010STRrow = CO0010INPtbl.NewRow
                        CO0010STRrow.ItemArray = CO0010INProw.ItemArray
                    End If
                End If

                '移動先の項目を取得
                If WF_VIEW_END_ROW.Value <> "0" AndAlso WF_VIEW_END_COL.Value <> "0" AndAlso
                    CO0010INProw("POSIROW") = If(WW_HDKBN = "D", WF_VIEW_END_ROW.Value, 0) AndAlso
                    CO0010INProw("POSICOL") = WF_VIEW_END_COL.Value Then
                    CO0010ENDrow = CO0010INPtbl.NewRow
                    CO0010ENDrow.ItemArray = CO0010INProw.ItemArray
                End If
            End If
        Next

        '○ 移動前の座標に移動先の座標をセット
        If Not IsNothing(CO0010STRrow) Then
            CO0010STRrow("POSIROW") = If(WW_HDKBN = "D", WF_VIEW_END_ROW.Value, 0)
            CO0010STRrow("POSICOL") = WF_VIEW_END_COL.Value

            If WF_VIEW_START_ROW.Value = "0" AndAlso WF_VIEW_START_COL.Value = "0" Then
                CO0010STRrow("EFFECT") = "Y"
            End If

            If WF_VIEW_END_ROW.Value = "0" AndAlso WF_VIEW_END_COL.Value = "0" Then
                CO0010STRrow("ALIGN") = "left"
                CO0010STRrow("SORTORDER") = 0
                CO0010STRrow("SORTKBN") = ""
                CO0010STRrow("EFFECT") = "N"
                CO0010STRrow("FIXCOL") = ""
            End If
        End If

        '○ 移動先の座標に移動前の座標をセット
        If Not IsNothing(CO0010ENDrow) Then
            CO0010ENDrow("POSIROW") = If(WW_HDKBN = "D", WF_VIEW_START_ROW.Value, 0)
            CO0010ENDrow("POSICOL") = WF_VIEW_START_COL.Value

            If WF_VIEW_START_ROW.Value = "0" AndAlso WF_VIEW_START_COL.Value = "0" Then
                CO0010ENDrow("ALIGN") = "left"
                CO0010ENDrow("SORTORDER") = 0
                CO0010ENDrow("SORTKBN") = ""
                CO0010ENDrow("EFFECT") = "N"
                CO0010ENDrow("FIXCOL") = ""
            End If
        End If

        '○ テーブルに保存する
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            If CO0010INProw("TITLEKBN") <> "H" AndAlso
                CO0010INProw("HDKBN") = WW_HDKBN AndAlso
                CO0010INProw("TABID") = WW_TABID Then
                '移動前の項目を保存
                If Not IsNothing(CO0010STRrow) AndAlso
                    CO0010INProw("FIELD") = CO0010STRrow("FIELD") Then
                    CO0010INProw.ItemArray = CO0010STRrow.ItemArray
                End If

                '移動先の項目を保存
                If Not IsNothing(CO0010ENDrow) AndAlso
                    CO0010INProw("FIELD") = CO0010ENDrow("FIELD") Then
                    CO0010INProw.ItemArray = CO0010ENDrow.ItemArray
                End If
            End If
        Next

        Master.SaveTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        WF_VIEW_SELECT.Value = ""
        WF_VIEW_ROW.Value = If(WF_VIEW_END_ROW.Value = "0", WF_VIEW_START_ROW.Value, WF_VIEW_END_ROW.Value)
        WF_VIEW_COL.Value = If(WF_VIEW_END_COL.Value = "0", WF_VIEW_START_COL.Value, WF_VIEW_END_COL.Value)
        WW_ViewSelectDetail()

        WF_List_Top.Value = 0

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"
        WF_VIEW_START_ROW.Value = ""
        WF_VIEW_START_COL.Value = ""
        WF_VIEW_END_ROW.Value = ""
        WF_VIEW_END_COL.Value = ""

    End Sub


    ''' <summary>
    ''' ビュー切替
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_VIEW_Change()

        Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        Master.SaveTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        WF_VIEW_SELECT.Value = ""
        WF_VIEW_ROW.Value = 1
        WF_VIEW_COL.Value = 1
        WW_ViewSelectDetail()

        If WF_HDKBN_HEAD.Checked Then
            WF_HDKBN.Value = "H"
            WF_TABID.Value = ""
        End If
        If WF_HDKBN_DETAIL.Checked Then
            WF_HDKBN.Value = "D"
            WF_TABID.Value = WF_TABselect.SelectedValue
        End If

        WF_List_Top.Value = 0
        WF_Scroll_Left.Value = 0
        WF_Scroll_Top.Value = 0

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"
        WF_VIEW_SELECT.Value = ""

    End Sub


    ''' <summary>
    ''' タブ項目変更処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_TAB_Change()

        Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        Master.SaveTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        WF_VIEW_SELECT.Value = ""
        WF_VIEW_ROW.Value = 1
        WF_VIEW_COL.Value = 1
        WW_ViewSelectDetail()

        WF_TABID.Value = WF_TABselect.SelectedValue

        WF_List_Top.Value = 0
        WF_Scroll_Left.Value = 0
        WF_Scroll_Top.Value = 0

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"
        WF_VIEW_SELECT.Value = ""

    End Sub


    ''' <summary>
    ''' ビュー項目ヘッダーダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_VIEW_HEAD_DBClick()

        Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        Dim WW_HDKBN As String = ""
        Dim WW_TABID As String = ""

        If WF_HDKBN_HEAD.Checked Then
            WW_HDKBN = "H"
            WW_TABID = ""
        End If
        If WF_HDKBN_DETAIL.Checked Then
            WW_HDKBN = "D"
            WW_TABID = WF_TABselect.SelectedValue
        End If

        '○ ダブルクリックした列と以降の明細編集
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            If CO0010INProw("TITLEKBN") <> "H" AndAlso
                CO0010INProw("HDKBN") = WW_HDKBN AndAlso
                CO0010INProw("TABID") = WW_TABID AndAlso
                CO0010INProw("EFFECT") = "Y" Then
                '同じ列の場合、未使用にする
                If CO0010INProw("POSICOL") = WF_DELCOL.Value Then
                    CO0010INProw("POSIROW") = 0
                    CO0010INProw("POSICOL") = 0
                    CO0010INProw("ALIGN") = "left"
                    CO0010INProw("SORTORDER") = 0
                    CO0010INProw("SORTKBN") = ""
                    CO0010INProw("EFFECT") = "N"
                    CO0010INProw("FIXCOL") = ""
                End If

                '以降の場合1列左に移動
                If CO0010INProw("POSICOL") > WF_DELCOL.Value Then
                    CO0010INProw("POSICOL") = CO0010INProw("POSICOL") - 1
                End If
            End If
        Next

        Master.SaveTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        WF_VIEW_SELECT.Value = ""
        WW_ViewSelectDetail()

        WF_List_Top.Value = 0

        WF_WIDTH.Focus()
        WF_DISP.Value = "detailbox"

    End Sub


    ''' <summary>
    ''' ビュー項目ダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_VIEW_DBClick()

        Master.RecoverTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        '○ 現在の表示をテーブルに反映
        RepeaterUpdate()

        Master.SaveTable(CO0010INPtbl, WF_XMLsaveF_INP.Value)

        WW_ViewSelectDetail()

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

            Case "WF_ALIGN"             '文字揃え
                WF_ALIGN.Text = WW_SelectValue
                WF_ALIGN_TEXT.Text = WW_SelectText
                WF_ALIGN.Focus()

            Case "WF_SORTKBN"           '昇降区分
                WF_SORTKBN.Text = WW_SelectValue
                WF_SORTKBN_TEXT.Text = WW_SelectText
                WF_SORTKBN.Focus()

            Case "WF_FIXCOL"            '固定列
                If WF_HDKBN_HEAD.Checked Then
                    WF_FIXCOL.Text = WW_SelectValue
                    WF_FIXCOL_TEXT.Text = WW_SelectText
                    WF_FIXCOL.Focus()
                End If

            Case "WF_EFFECT"            '入力有無
                If WF_HDKBN_DETAIL.Checked Then
                    WF_EFFECT.Text = WW_SelectValue
                    WF_EFFECT_TEXT.Text = WW_SelectText
                    WF_EFFECT.Focus()
                End If
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_LeftboxOpen.Value = ""

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
            Case "WF_ALIGN"             '文字揃え
                WF_ALIGN.Focus()
            Case "WF_SORTKBN"           '昇降区分
                WF_SORTKBN.Focus()
            Case "WF_FIXCOL"            '固定列
                WF_FIXCOL.Focus()
            Case "WF_EFFECT"            '入力有無
                WF_EFFECT.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftMViewChange.Value = ""
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
        Master.eraseCharToIgnore(WF_ALIGN.Text)
        Master.eraseCharToIgnore(WF_SORT.Text)
        Master.eraseCharToIgnore(WF_SORTKBN.Text)
        Master.eraseCharToIgnore(WF_FIELDNAMES.Text)
        Master.eraseCharToIgnore(WF_LENGTH.Text)
        Master.eraseCharToIgnore(WF_FIXCOL.Text)
        Master.eraseCharToIgnore(WF_EFFECT.Text)

        If Not String.IsNullOrEmpty(WF_FIELD_VIEW.Value) Then
            '幅
            WW_TEXT = WF_WIDTH.Text
            If WW_TEXT = "" Then
                WF_WIDTH.Text = "0"
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

            '文字揃え
            WW_TEXT = WF_ALIGN.Text
            If WW_TEXT = "" Then
                WF_ALIGN.Text = "left"
            Else
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ALIGN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("ALIGN", WF_ALIGN.Text, WW_DUMMY, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        WW_CheckMES1 = "・エラー(文字揃え不正)の為、初期値leftを設定しました。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                        WF_ALIGN.Text = "left"
                        WW_LINE_ERR = "ERR"
                    End If
                Else
                    WW_CheckMES1 = "・エラー(文字揃え不正)の為、初期値leftを設定しました。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                    WF_ALIGN.Text = "left"
                    WW_LINE_ERR = "ERR"
                End If
            End If
            CODENAME_get("ALIGN", WF_ALIGN.Text, WF_ALIGN_TEXT.Text, WW_ERR_SW)

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

            '昇降区分
            WW_TEXT = WF_SORTKBN.Text
            If WW_TEXT = "" Then
                WF_SORTKBN.Text = ""
            Else
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SORTKBN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("SORTKBN", WF_SORTKBN.Text, WW_DUMMY, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        WW_CheckMES1 = "・エラー(昇降区分不正)の為、初期値ブランクを設定しました。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                        WF_SORTKBN.Text = ""
                        WW_LINE_ERR = "ERR"
                    End If
                Else
                    WW_CheckMES1 = "・エラー(昇降区分不正)の為、初期値ブランクを設定しました。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                    WF_SORTKBN.Text = ""
                    WW_LINE_ERR = "ERR"
                End If
            End If
            CODENAME_get("SORTKBN", WF_SORTKBN.Text, WF_SORTKBN_TEXT.Text, WW_ERR_SW)

            '入力可能数
            WW_TEXT = WF_LENGTH.Text
            If WW_TEXT = "" Then
                WF_LENGTH.Text = "0"
            Else
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LENGTH", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・エラー(入力可能数不正)の為、初期値10を設定しました。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                    WF_LENGTH.Text = "10"
                    WW_LINE_ERR = "ERR"
                End If
            End If

            '固定列
            WW_TEXT = WF_FIXCOL.Text
            If WF_HDKBN_DETAIL.Checked OrElse WW_TEXT = "" Then
                WF_FIXCOL.Text = ""
            Else
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "FIXCOL", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("FIXCOL", WF_FIXCOL.Text, WW_DUMMY, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        WW_CheckMES1 = "・エラー(固定列不正)の為、初期値ブランクを設定しました。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                        WF_FIXCOL.Text = ""
                        WW_LINE_ERR = "ERR"
                    End If
                Else
                    WW_CheckMES1 = "・エラー(固定列不正)の為、初期値ブランクを設定しました。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                    WF_FIXCOL.Text = ""
                    WW_LINE_ERR = "ERR"
                End If
            End If
            CODENAME_get("FIXCOL", WF_FIXCOL.Text, WF_FIXCOL_TEXT.Text, WW_ERR_SW)

            '入力有無
            WW_TEXT = WF_EFFECT.Text
            If WF_HDKBN_HEAD.Checked Then
                WF_EFFECT.Text = "Y"
            Else
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "EFFECT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("EFFECT", WF_EFFECT.Text, WW_DUMMY, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        WW_CheckMES1 = "・エラー(入力有無不正)の為、初期値'Y'を設定しました。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                        WF_EFFECT.Text = "Y"
                        WW_LINE_ERR = "ERR"
                    End If
                Else
                    WW_CheckMES1 = "・エラー(入力有無不正)の為、初期値'Y'を設定しました。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                    WF_EFFECT.Text = "Y"
                    WW_LINE_ERR = "ERR"
                End If
            End If
            CODENAME_get("EFFECT", WF_EFFECT.Text, WF_EFFECT_TEXT.Text, WW_ERR_SW)

            If WW_LINE_ERR = "ERR" Then
                Master.output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
            End If

            '○ 選択項目詳細の位置より項目を取得し、該当するCO0010INPtblへ選択項目詳細内容を反映
            For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
                If CO0010INProw("TITLEKBN") <> "H" AndAlso
                    CO0010INProw("HDKBN") = WF_HDKBN.Value AndAlso
                    CO0010INProw("TABID") = WF_TABID.Value AndAlso
                    CO0010INProw("FIELD") = WF_FIELD_VIEW.Value Then
                    CO0010INProw("FIELDNAMES") = WF_FIELDNAMES.Text
                    CO0010INProw("LENGTH") = WF_LENGTH.Text
                    CO0010INProw("ALIGN") = WF_ALIGN.Text
                    CO0010INProw("SORTORDER") = WF_SORT.Text
                    CO0010INProw("SORTKBN") = WF_SORTKBN.Text
                    CO0010INProw("EFFECT") = WF_EFFECT.Text
                    CO0010INProw("WIDTH") = WF_WIDTH.Text
                    CO0010INProw("FIXCOL") = WF_FIXCOL.Text
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
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0010INProw("TITLEKBN") <> "H" Then
                Continue For
            End If

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", CO0010INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", CO0010INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'プロフID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "PROFID", CO0010INProw("PROFID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If CO0010INProw("PROFID") <> C_DEFAULT_DATAKEY AndAlso
                    CO0010INProw("PROFID") <> Master.PROF_VIEW Then
                    WW_CheckMES1 = "・更新できないレコード(プロフIDエラー)です。"
                    WW_CheckMES2 = "ログインユーザーのプロフIDと異なります。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(プロフIDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ユーザープロフID、Defaultはエラー
            If Master.PROF_VIEW = C_DEFAULT_DATAKEY Then
                WW_CheckMES1 = "・更新できないレコード(プロフID='" & C_DEFAULT_DATAKEY & "')です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面ID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MAPID", CO0010INProw("MAPID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("MAPID", CO0010INProw("MAPID"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(画面IDエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(画面IDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面識別ｺｰﾄﾞ(変数)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VARIANT", CO0010INProw("VARIANT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(画面識別ｺｰﾄﾞエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面識別名(変数)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "FIELDNAMES", CO0010INProw("FIELDNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(画面識別名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", CO0010INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", CO0010INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '日付大小チェック
            If CO0010INProw("STYMD") > CO0010INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(有効開始日＞有効終了日)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '範囲チェック
            If work.WF_SEL_STYMD.Text > CO0010INProw("STYMD") AndAlso
                work.WF_SEL_STYMD.Text > CO0010INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            If work.WF_SEL_ENDYMD.Text < CO0010INProw("STYMD") AndAlso
                work.WF_SEL_ENDYMD.Text < CO0010INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除フラグ
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", CO0010INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", CO0010INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                CO0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0010INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
            Exit For
        Next

        '○ 明細単項目チェック
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0010INProw("TITLEKBN") = "H" Then
                Continue For
            End If

            '行位置
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "POSIROW", CO0010INProw("POSIROW"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(行位置エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '列位置
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "POSICOL", CO0010INProw("POSICOL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(列位置エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '入力不足
            If CO0010INProw("TABID") = "D" AndAlso CO0010INProw("POSIROW") = 0 AndAlso CO0010INProw("POSICOL") <> 0 Then
                WW_CheckMES1 = "・更新できないレコード(列＝0、行≠0)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If CO0010INProw("POSIROW") <> 0 AndAlso CO0010INProw("POSICOL") = 0 Then
                WW_CheckMES1 = "・更新できないレコード(列≠0、行＝0)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '幅
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "WIDTH", CO0010INProw("WIDTH"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(幅エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '文字揃え
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ALIGN", CO0010INProw("ALIGN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("ALIGN", CO0010INProw("ALIGN"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(文字揃えエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(文字揃えエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ソート
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SORTORDER", CO0010INProw("SORTORDER"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(ソートエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '昇降区分
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SORTKBN", CO0010INProw("SORTKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("SORTKBN", CO0010INProw("SORTKBN"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(昇降区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(昇降区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '項目名称
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "FIELDNAMES", CO0010INProw("FIELDNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(項目名称エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '入力可能数
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LENGTH", CO0010INProw("LENGTH"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(入力可能数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '固定列
            WW_TEXT = CO0010INProw("FIXCOL")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "FIXCOL", CO0010INProw("FIXCOL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0010INProw("FIXCOL") = ""
                Else
                    '存在チェック
                    CODENAME_get("FIXCOL", CO0010INProw("FIXCOL"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(固定列エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(固定列エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '表示有無
            WW_TEXT = CO0010INProw("EFFECT")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "EFFECT", CO0010INProw("EFFECT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0010INProw("EFFECT") = "N"
                Else
                    '存在チェック
                    CODENAME_get("EFFECT", CO0010INProw("EFFECT"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(表示有無エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(表示有無エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                CO0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0010INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        '○ 重複チェック
        For i As Integer = 0 To CO0010INPtbl.Rows.Count - 1
            If (CO0010INPtbl.Rows(i)("HDKBN") = "H" AndAlso CO0010INPtbl.Rows(i)("POSICOL") <> 0) OrElse
                (CO0010INPtbl.Rows(i)("HDKBN") = "D" AndAlso CO0010INPtbl.Rows(i)("POSIROW") <> 0 AndAlso CO0010INPtbl.Rows(i)("POSICOL") <> 0) Then
                For j As Integer = i + 1 To CO0010INPtbl.Rows.Count - 1
                    If CO0010INPtbl.Rows(i)("HDKBN") = CO0010INPtbl.Rows(j)("HDKBN") AndAlso
                        CO0010INPtbl.Rows(i)("TABID") = CO0010INPtbl.Rows(j)("TABID") AndAlso
                        CO0010INPtbl.Rows(i)("POSIROW") = CO0010INPtbl.Rows(j)("POSIROW") AndAlso
                        CO0010INPtbl.Rows(i)("POSICOL") = CO0010INPtbl.Rows(j)("POSICOL") Then
                        WW_CheckMES1 = "・更新できないレコード(列行重複)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0010INPtbl.Rows(i))
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Next
            End If

            If WW_LINE_ERR = "" Then
                CO0010INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0010INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="CO0010row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal CO0010row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(CO0010row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード =" & CO0010row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> プロフＩＤ =" & CO0010row("PROFID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 画面ＩＤ   =" & CO0010row("MAPID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 変数       =" & CO0010row("VARIANT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> ＨＤ区分   =" & CO0010row("HDKBN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タブID     =" & CO0010row("TABID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項目       =" & CO0010row("FIELD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日 =" & CO0010row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日 =" & CO0010row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除       =" & CO0010row("DELFLG")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' CO0010tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CO0010tbl_UPD()

        '○ 画面状態設定
        For Each CO0010row As DataRow In CO0010tbl.Rows
            Select Case CO0010row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0010row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 件数が膨大なため、会社と画面IDで絞る
        Dim CO0010FILtbl As New DataTable
        CS0026TBLSORT.TABLE = CO0010tbl
        CS0026TBLSORT.SORTING = ""
        CS0026TBLSORT.FILTER = "CAMPCODE = '" & WF_CAMPCODE.Text & "'" _
            & " and MAPID = '" & WF_MAPID.Text & "'"
        CS0026TBLSORT.sort(CO0010FILtbl)

        '○ 追加変更判定
        Dim WW_UPDAT As Boolean = False
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows

            'エラーレコード読み飛ばし
            If CO0010INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            CO0010INProw("OPERATION") = "Insert"

            'KEY項目が等しい(ENDYMD以外のKEYが同じ)
            For Each CO0010row As DataRow In CO0010FILtbl.Rows
                If CO0010row("CAMPCODE") = CO0010INProw("CAMPCODE") AndAlso
                    CO0010row("PROFID") = Master.PROF_VIEW AndAlso
                    CO0010row("MAPID") = CO0010INProw("MAPID") AndAlso
                    CO0010row("VARIANT") = CO0010INProw("VARIANT") AndAlso
                    CO0010row("HDKBN") = CO0010INProw("HDKBN") AndAlso
                    CO0010row("FIELD") = CO0010INProw("FIELD") AndAlso
                    CO0010row("STYMD") = CO0010INProw("STYMD") Then

                    '変更無は操作無
                    If CO0010row("TITLEKBN") = CO0010INProw("TITLEKBN") AndAlso
                        CO0010row("TABID") = CO0010INProw("TABID") AndAlso
                        CO0010row("POSIROW") = CO0010INProw("POSIROW") AndAlso
                        CO0010row("POSICOL") = CO0010INProw("POSICOL") AndAlso
                        CO0010row("ENDYMD") = CO0010INProw("ENDYMD") AndAlso
                        CO0010row("FIELDNAMES") = CO0010INProw("FIELDNAMES") AndAlso
                        CO0010row("FIELDNAMEL") = CO0010INProw("FIELDNAMEL") AndAlso
                        CO0010row("PREFIX") = CO0010INProw("PREFIX") AndAlso
                        CO0010row("SUFFIX") = CO0010INProw("SUFFIX") AndAlso
                        CO0010row("LENGTH") = CO0010INProw("LENGTH") AndAlso
                        CO0010row("ALIGN") = CO0010INProw("ALIGN") AndAlso
                        CO0010row("SORTORDER") = CO0010INProw("SORTORDER") AndAlso
                        CO0010row("SORTKBN") = CO0010INProw("SORTKBN") AndAlso
                        CO0010row("EFFECT") = CO0010INProw("EFFECT") AndAlso
                        CO0010row("WIDTH") = CO0010INProw("WIDTH") AndAlso
                        CO0010row("OBJECTTYPE") = CO0010INProw("OBJECTTYPE") AndAlso
                        CO0010row("FORMATTYPE") = CO0010INProw("FORMATTYPE") AndAlso
                        CO0010row("FORMATVALUE") = CO0010INProw("FORMATVALUE") AndAlso
                        CO0010row("FIXCOL") = CO0010INProw("FIXCOL") AndAlso
                        CO0010row("REQUIRED") = CO0010INProw("REQUIRED") AndAlso
                        CO0010row("COLORSET") = CO0010INProw("COLORSET") AndAlso
                        CO0010row("ADDEVENT1") = CO0010INProw("ADDEVENT1") AndAlso
                        CO0010row("ADDFUNC1") = CO0010INProw("ADDFUNC1") AndAlso
                        CO0010row("ADDEVENT2") = CO0010INProw("ADDEVENT2") AndAlso
                        CO0010row("ADDFUNC2") = CO0010INProw("ADDFUNC2") AndAlso
                        CO0010row("ADDEVENT3") = CO0010INProw("ADDEVENT3") AndAlso
                        CO0010row("ADDFUNC3") = CO0010INProw("ADDFUNC3") AndAlso
                        CO0010row("ADDEVENT4") = CO0010INProw("ADDEVENT4") AndAlso
                        CO0010row("ADDFUNC4") = CO0010INProw("ADDFUNC4") AndAlso
                        CO0010row("ADDEVENT5") = CO0010INProw("ADDEVENT5") AndAlso
                        CO0010row("ADDFUNC5") = CO0010INProw("ADDFUNC5") AndAlso
                        CO0010row("DELFLG") = CO0010INProw("DELFLG") Then
                        CO0010INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    CO0010INProw("OPERATION") = "Update"
                    WW_UPDAT = True
                    Exit For
                End If
            Next
        Next

        CO0010FILtbl.Clear()
        CO0010FILtbl.Dispose()
        CO0010FILtbl = Nothing

        '○ 更新レコードが存在する場合、ヘッダー区分も更新対象にする
        If WW_UPDAT Then
            For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
                If CO0010INProw("TITLEKBN") = "H" Then
                    CO0010INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        End If

        '○ 変更有無判定　&　入力値反映
        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            Select Case CO0010INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(CO0010INProw)
                Case "Insert"
                    TBL_INSERT_SUB(CO0010INProw)
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="CO0010INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByVal CO0010INProw As DataRow)

        For Each CO0010row As DataRow In CO0010tbl.Rows

            '同一(ENDYMD以外が同一KEY)レコード
            If CO0010row("CAMPCODE") = CO0010INProw("CAMPCODE") AndAlso
                CO0010row("PROFID") = Master.PROF_VIEW AndAlso
                CO0010row("MAPID") = CO0010INProw("MAPID") AndAlso
                CO0010row("VARIANT") = CO0010INProw("VARIANT") AndAlso
                CO0010row("HDKBN") = CO0010INProw("HDKBN") AndAlso
                CO0010row("TABID") = CO0010INProw("TABID") AndAlso
                CO0010row("FIELD") = CO0010INProw("FIELD") AndAlso
                CO0010row("STYMD") = CO0010INProw("STYMD") Then

                '画面入力テーブル項目設定
                CO0010INProw("LINECNT") = CO0010row("LINECNT")
                CO0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                CO0010INProw("TIMSTP") = CO0010row("TIMSTP")
                CO0010INProw("SELECT") = 1
                CO0010INProw("HIDDEN") = 0

                CO0010INProw("PROFID") = Master.PROF_VIEW

                '項目テーブル項目設定
                CO0010row.ItemArray = CO0010INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="CO0010INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByVal CO0010INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim CO0010row As DataRow = CO0010tbl.NewRow
        CO0010row.ItemArray = CO0010INProw.ItemArray

        '○ 最大項番数を取得
        Dim TBLview As DataView = New DataView(CO0010tbl)
        TBLview.RowFilter = "TITLEKBN = 'H'"

        If CO0010INProw("TITLEKBN") = "H" Then
            CO0010row("LINECNT") = TBLview.Count + 1
        Else
            CO0010row("LINECNT") = 0
        End If

        CO0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        CO0010row("TIMSTP") = "0"
        CO0010row("SELECT") = 1
        CO0010row("HIDDEN") = 0

        CO0010row("PROFID") = Master.PROF_VIEW

        CO0010tbl.Rows.Add(CO0010row)

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ''' <summary>
    ''' 選択項目編集部設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ViewSelectDetail()

        '○ 項目セット
        Dim WW_HDKBN As String = ""
        Dim WW_TABID As String = ""

        If WF_HDKBN_HEAD.Checked Then
            WW_HDKBN = "H"
            WW_TABID = ""
        End If
        If WF_HDKBN_DETAIL.Checked Then
            WW_HDKBN = "D"
            WW_TABID = WF_TABselect.SelectedValue
        End If

        WF_FIELD_VIEW.Value = ""
        WF_WIDTH.Text = "0"
        WF_ALIGN.Text = "left"
        WF_SORT.Text = "0"
        WF_SORTKBN.Text = ""
        WF_FIELDNAMES.Text = ""
        WF_LENGTH.Text = "0"
        WF_FIXCOL.Text = ""
        WF_EFFECT.Text = "N"

        For Each CO0010INProw As DataRow In CO0010INPtbl.Rows
            If CO0010INProw("TITLEKBN") <> "H" AndAlso
                CO0010INProw("HDKBN") = WW_HDKBN AndAlso
                CO0010INProw("TABID") = WW_TABID AndAlso
                CO0010INProw("POSIROW") = If(WW_HDKBN = "D", WF_VIEW_ROW.Value, 0) AndAlso
                CO0010INProw("POSICOL") = WF_VIEW_COL.Value Then
                WF_FIELD_VIEW.Value = CO0010INProw("FIELD")
                WF_WIDTH.Text = CO0010INProw("WIDTH")
                WF_ALIGN.Text = CO0010INProw("ALIGN")
                WF_SORT.Text = CO0010INProw("SORTORDER")
                WF_SORTKBN.Text = CO0010INProw("SORTKBN")
                WF_FIELDNAMES.Text = CO0010INProw("FIELDNAMES")
                WF_LENGTH.Text = CO0010INProw("LENGTH")
                WF_FIXCOL.Text = CO0010INProw("FIXCOL")
                WF_EFFECT.Text = CO0010INProw("EFFECT")
                Exit For
            End If
        Next

        '名称取得
        CODENAME_get("ALIGN", WF_ALIGN.Text, WF_ALIGN_TEXT.Text, WW_DUMMY)              '文字揃え
        CODENAME_get("SORTKBN", WF_SORTKBN.Text, WF_SORTKBN_TEXT.Text, WW_DUMMY)        '昇降区分
        CODENAME_get("FIXCOL", WF_FIXCOL.Text, WF_FIXCOL_TEXT.Text, WW_DUMMY)           '固定列
        CODENAME_get("EFFECT", WF_EFFECT.Text, WF_EFFECT_TEXT.Text, WW_DUMMY)           '入力有無

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
                Case "ALIGN"            '文字揃え
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "ALIGN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SORTKBN"          '昇降区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SORTKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "FIXCOL"          '固定列
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "FIXCOL"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "EFFECT"          '入力有無
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "EFFECT"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "COL6"             '明細画面6列
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "CO0010_MAPID"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

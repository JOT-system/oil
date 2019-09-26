Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 所定労働時間登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMB0004WORKINGH
    Inherits Page

    '○ 検索結果格納Table
    Private MB0004tbl As DataTable                              '一覧格納用テーブル
    Private MB0004INPtbl As DataTable                           'チェック用テーブル
    Private MB0004UPDtbl As DataTable                           '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45            '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10             'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '明細部ID

    '○ 共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '文字編集
    Private CS0011LOGWrite As New CS0011LOGWrite                'ログ出力
    Private CS0013ProfView As New CS0013ProfView                'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                  '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD              'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget              '権限チェック(マスタチェック)
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                    '帳票出力
    Private CS0050SESSION As New CS0050SESSION                  'セッション情報操作処理
    Private CS0052DetailView As New CS0052DetailView            'リピーター用Tableオブジェクト展開

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
                    If Not Master.RecoverTable(MB0004tbl) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                            WF_ButtonPrint_Click()
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
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
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
            If Not IsNothing(MB0004tbl) Then
                MB0004tbl.Clear()
                MB0004tbl.Dispose()
                MB0004tbl = Nothing
            End If

            If Not IsNothing(MB0004INPtbl) Then
                MB0004INPtbl.Clear()
                MB0004INPtbl.Dispose()
                MB0004INPtbl = Nothing
            End If

            If Not IsNothing(MB0004UPDtbl) Then
                MB0004UPDtbl.Clear()
                MB0004UPDtbl.Dispose()
                MB0004UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRMB0004WRKINC.MAPID

        WF_HORG.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MB0004S Then
            'Grid情報保存先のファイル名
            Master.createXMLSaveFile()

            '会社コード表示
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
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
        Master.SaveTable(MB0004tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(MB0004tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

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

        If IsNothing(MB0004tbl) Then
            MB0004tbl = New DataTable
        End If

        If MB0004tbl.Columns.Count <> 0 Then
            MB0004tbl.Columns.Clear()
        End If

        MB0004tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     所定労働時間マスタの有効なものを取得する
        '　注意事項　日付について
        '　　権限判断はすべてDateNow。グループコード、名称取得は全てDateNow。表追加時の①はDateNow。
        '　　但し、表追加時の②および③は、TBL入力有効期限。

        Dim SQLStr As String =
              " SELECT" _
            & "    0                                               AS LINECNT" _
            & "    , ''                                            AS OPERATION" _
            & "    , CAST(MB04.UPDTIMSTP AS bigint)                AS TIMSTP" _
            & "    , 1                                             AS 'SELECT'" _
            & "    , 0                                             AS HIDDEN" _
            & "    , ISNULL(RTRIM(MB04.CAMPCODE), '')              AS CAMPCODE" _
            & "    , ''                                            AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(MB04.HORG), '')                  AS HORG" _
            & "    , ''                                            AS HORGNAMES" _
            & "    , ISNULL(RTRIM(MB04.STAFFKBN), '')              AS STAFFKBN" _
            & "    , ''                                            AS STAFFKBNNAMES" _
            & "    , ISNULL(FORMAT(MB04.STYMD, 'yyyy/MM/dd'), '')  AS STYMD" _
            & "    , ISNULL(FORMAT(MB04.ENDYMD, 'yyyy/MM/dd'), '') AS ENDYMD" _
            & "    , ISNULL(CONVERT(char(5), MB04.WORKINGH), '')   AS WORKINGH" _
            & "    , ISNULL(RTRIM(MB04.WORKINGN), '0')             AS WORKINGN" _
            & "    , ISNULL(RTRIM(MB04.DELFLG), '')                AS DELFLG" _
            & " FROM" _
            & "    MB004_WORKINGH MB04" _
            & "    INNER JOIN S0006_ROLE S006" _
            & "        ON  S006.CAMPCODE    = @P1" _
            & "        AND S006.OBJECT      = @P4" _
            & "        AND S006.ROLE        = @P5" _
            & "        AND S006.CODE        = MB04.HORG" _
            & "        AND S006.PERMITCODE >= @P6" _
            & "        AND S006.STYMD      <= @P7" _
            & "        AND S006.ENDYMD     >= @P7" _
            & "        AND S006.DELFLG     <> @P8" _
            & " WHERE" _
            & "    MB04.CAMPCODE    = @P1" _
            & "    AND MB04.STYMD  <= @P2" _
            & "    AND MB04.ENDYMD >= @P3" _
            & "    AND MB04.DELFLG <> @P8"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '配属部署
        If Not String.IsNullOrEmpty(work.WF_SEL_HORG.Text) Then
            SQLStr &= String.Format("    AND MB04.HORG    = '{0}'", work.WF_SEL_HORG.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "      MB04.HORG" _
            & "    , MB04.STYMD" _
            & "    , MB04.ENDYMD" _
            & "    , MB04.STAFFKBN"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '有効年月日(To)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '有効年月日(From)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)        'オブジェクト
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)        'ロール
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Int)                 'パーミット
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                '現在日付
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = work.WF_SEL_ENDYMD.Text
                PARA3.Value = work.WF_SEL_STYMD.Text
                PARA4.Value = C_ROLE_VARIANT.USER_ORG
                PARA5.Value = Master.ROLE_ORG
                PARA6.Value = C_PERMISSION.REFERLANCE
                PARA7.Value = Date.Now
                PARA8.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        MB0004tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    MB0004tbl.Load(SQLdr)
                End Using

                For Each MB0004row As DataRow In MB0004tbl.Rows
                    '名称取得
                    CODENAME_get("CAMPCODE", MB0004row("CAMPCODE"), MB0004row("CAMPNAMES"), WW_DUMMY)           '会社コード
                    CODENAME_get("HORG", MB0004row("HORG"), MB0004row("HORGNAMES"), WW_DUMMY)                   '配属部署
                    CODENAME_get("STAFFKBN", MB0004row("STAFFKBN"), MB0004row("STAFFKBNNAMES"), WW_DUMMY)       '職務区分
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB004_WORKINGH SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MB004_WORKINGH Select"
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
        CS0026TBLSORT.TABLE = MB0004tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            MB0004tbl = CS0026TBLSORT.TABLE
        End If

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        '   ※ 絞込 (Cell(4) : 0=表示対象, 1=非表示対象)
        For Each MB0004row As DataRow In MB0004tbl.Rows
            If MB0004row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                MB0004row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(MB0004tbl)

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
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.eraseCharToIgnore(WF_SELHORG.Text)
        Master.eraseCharToIgnore(WF_SELSTAFFKBN.Text)

        '○ 名称取得
        CODENAME_get("HORG", WF_SELHORG.Text, WF_SELHORG_TEXT.Text, WW_DUMMY)                   '配属部署
        CODENAME_get("STAFFKBN", WF_SELSTAFFKBN.Text, WF_SELSTAFFKBN_TEXT.Text, WW_DUMMY)       '職務区分

        '○ 絞り込み操作(GridView明細Hidden設定)
        For Each MB0004row As DataRow In MB0004tbl.Rows

            '一度非表示にする
            MB0004row("HIDDEN") = 1

            Dim WW_HANTEI As Boolean = True

            '配属部署による絞込判定
            If WF_SELHORG.Text <> "" AndAlso
                WF_SELHORG.Text <> MB0004row("HORG") Then
                WW_HANTEI = False
            End If

            '職務区分による絞込判定
            If WF_SELSTAFFKBN.Text <> "" AndAlso
                WF_SELSTAFFKBN.Text <> MB0004row("STAFFKBN") Then
                WW_HANTEI = False
            End If

            '画面(GridView)のHIDDENに結果格納
            If WW_HANTEI Then
                MB0004row("HIDDEN") = 0
            End If
        Next

        '○ 画面先頭を表示
        WF_GridPosition.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(MB0004tbl)

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_HORG.Focus()

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

                '所定労働時間マスタ更新
                UpdateWorkingHours(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MB0004tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        WF_HORG.Focus()

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
        For Each MB0004row As DataRow In MB0004tbl.Rows

            '読み飛ばし
            If (MB0004row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                MB0004row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                MB0004row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                MB0004row("STYMD") = "" Then
                Continue For
            End If

            WW_LINE_ERR = ""

            'チェック
            For Each MB0004chk As DataRow In MB0004tbl.Rows

                '同一KEY以外は読み飛ばし
                If MB0004row("CAMPCODE") <> MB0004chk("CAMPCODE") OrElse
                    MB0004row("HORG") <> MB0004chk("HORG") OrElse
                    MB0004row("STAFFKBN") <> MB0004chk("STAFFKBN") OrElse
                    MB0004chk("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If MB0004row("STYMD") = MB0004chk("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(MB0004row("STYMD"), WW_DATE_ST)
                    Date.TryParse(MB0004row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(MB0004chk("STYMD"), WW_DATE_ST2)
                    Date.TryParse(MB0004chk("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                    Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End Try

                '開始日チェック
                If WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", MB0004row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If

                '終了日チェック
                If WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", MB0004row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = ""
                    Exit For
                End If
            Next

            If WW_LINE_ERR = "" Then
                MB0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                MB0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 従業員マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateWorkingHours(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        MB004_WORKINGH" _
            & "    WHERE" _
            & "        CAMPCODE     = @P1" _
            & "        AND HORG     = @P2" _
            & "        AND STAFFKBN = @P3" _
            & "        AND STYMD    = @P4 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE MB004_WORKINGH" _
            & "    SET" _
            & "        ENDYMD      = @P5     , WORKINGH   = @P6" _
            & "        , WORKINGN  = @P7     , DELFLG     = @P8" _
            & "        , UPDYMD    = @P10    , UPDUSER    = @P11" _
            & "        , UPDTERMID = @P12    , RECEIVEYMD = @P13" _
            & "    WHERE" _
            & "        CAMPCODE     = @P1" _
            & "        AND HORG     = @P2" _
            & "        AND STAFFKBN = @P3" _
            & "        AND STYMD    = @P4 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO MB004_WORKINGH" _
            & "        (CAMPCODE     , HORG" _
            & "        , STAFFKBN    , STYMD" _
            & "        , ENDYMD      , WORKINGH" _
            & "        , WORKINGN    , DELFLG" _
            & "        , INITYMD     , UPDYMD" _
            & "        , UPDUSER     , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2" _
            & "        , @P3     , @P4" _
            & "        , @P5     , @P6" _
            & "        , @P7     , @P8" _
            & "        , @P9     , @P10" _
            & "        , @P11    , @P12" _
            & "        , @P13) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , HORG" _
            & "    , STAFFKBN" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , CONVERT(char(8), WORKINGH) AS WORKINGH" _
            & "    , WORKINGN" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint)  AS TIMSTP" _
            & " FROM" _
            & "    MB004_WORKINGH" _
            & " WHERE" _
            & "    CAMPCODE     = @P1" _
            & "    AND HORG     = @P2" _
            & "    AND STAFFKBN = @P3" _
            & "    AND STYMD    = @P4"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            '配属部署
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 5)             '職務区分
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                    '開始年月日
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                    '終了年月日
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Time)                    '所定労働時間
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Int)                     '所定労働日数
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 1)             '削除フラグ
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.DateTime)                '登録年月日
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)              '更新年月日
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '配属部署
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 5)         '職務区分
                Dim JPARA4 As SqlParameter = SQLcmdJnl.Parameters.Add("@P4", SqlDbType.Date)                '開始年月日

                For Each MB0004row As DataRow In MB0004tbl.Rows
                    If Trim(MB0004row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(MB0004row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = MB0004row("CAMPCODE")
                        PARA2.Value = MB0004row("HORG")
                        PARA3.Value = MB0004row("STAFFKBN")
                        PARA4.Value = MB0004row("STYMD")
                        PARA5.Value = MB0004row("ENDYMD")
                        PARA6.Value = MB0004row("WORKINGH")
                        PARA7.Value = MB0004row("WORKINGN")
                        PARA8.Value = MB0004row("DELFLG")
                        PARA9.Value = WW_DATENOW
                        PARA10.Value = WW_DATENOW
                        PARA11.Value = Master.USERID
                        PARA12.Value = Master.USERTERMID
                        PARA13.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = MB0004row("CAMPCODE")
                        JPARA2.Value = MB0004row("HORG")
                        JPARA3.Value = MB0004row("STAFFKBN")
                        JPARA4.Value = MB0004row("STYMD")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(MB0004UPDtbl) Then
                                MB0004UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    MB0004UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            MB0004UPDtbl.Clear()
                            MB0004UPDtbl.Load(SQLdr)
                        End Using

                        For Each MB0004UPDrow As DataRow In MB0004UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "MB004_WORKINGH"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MB0004UPDrow
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
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB004_WORKINGH UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MB004_WORKINGH UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = MB0004tbl                        'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        WF_HORG.Focus()

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPrint_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = MB0004tbl                        'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

        WF_HORG.Focus()

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
        WF_HORG.Focus()

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(MB0004tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        WF_HORG.Focus()

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
        WF_Sel_LINECNT.Text = MB0004tbl.Rows(WW_LINECNT)("LINECNT")

        '会社コード
        WF_CAMPCODE.Text = MB0004tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        '配属部署
        WF_HORG.Text = MB0004tbl.Rows(WW_LINECNT)("HORG")
        CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_DUMMY)

        '職務区分
        WF_STAFFKBN.Text = MB0004tbl.Rows(WW_LINECNT)("STAFFKBN")
        CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_DUMMY)

        '有効年月日
        WF_STYMD.Text = MB0004tbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = MB0004tbl.Rows(WW_LINECNT)("ENDYMD")

        '削除
        WF_DELFLG.Text = MB0004tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '○ Repeater設定
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            WW_FIELD_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label)
            If WW_FIELD_OBJ.text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FIELD_OBJ.text, MB0004tbl.Rows(WW_LINECNT)(WW_FIELD_OBJ.text))
                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_VALUE
                '名称取得
                CODENAME_get(WW_FIELD_OBJ.text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_TEXT
            End If

            '中
            WW_FIELD_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label)
            If WW_FIELD_OBJ.text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FIELD_OBJ.text, MB0004tbl.Rows(WW_LINECNT)(WW_FIELD_OBJ.text))
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_VALUE
                '名称取得
                CODENAME_get(WW_FIELD_OBJ.text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_TEXT
            End If

            '右
            WW_FIELD_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label)
            If WW_FIELD_OBJ.text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FIELD_OBJ.text, MB0004tbl.Rows(WW_LINECNT)(WW_FIELD_OBJ.text))
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_VALUE
                '名称取得
                CODENAME_get(WW_FIELD_OBJ.text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
            End If
        Next

        '○ 状態をクリア
        For Each MB0004row As DataRow In MB0004tbl.Rows
            Select Case MB0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case MB0004tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                MB0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                MB0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                MB0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                MB0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                MB0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(MB0004tbl)

        WF_HORG.Focus()
        WF_GridDBclick.Text = ""

    End Sub

    ''' <summary>
    ''' GridView値設定
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function REP_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String

        REP_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "SEQ"
                Try
                    REP_ITEM_FORMAT = Format(CInt(I_VALUE), "0")
                Catch ex As Exception
                End Try
        End Select

    End Function


    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

        WF_HORG.Focus()

    End Sub


    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
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
        Master.CreateEmptyTable(MB0004INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim MB0004INProw As DataRow = MB0004INPtbl.NewRow

            '○ 初期クリア
            For Each MB0004INPcol As DataColumn In MB0004INPtbl.Columns
                If IsDBNull(MB0004INProw.Item(MB0004INPcol)) OrElse IsNothing(MB0004INProw.Item(MB0004INPcol)) Then
                    Select Case MB0004INPcol.ColumnName
                        Case "LINECNT"
                            MB0004INProw.Item(MB0004INPcol) = 0
                        Case "OPERATION"
                            MB0004INProw.Item(MB0004INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MB0004INProw.Item(MB0004INPcol) = 0
                        Case "SELECT"
                            MB0004INProw.Item(MB0004INPcol) = 1
                        Case "HIDDEN"
                            MB0004INProw.Item(MB0004INPcol) = 0
                        Case Else
                            MB0004INProw.Item(MB0004INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("HORG") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STAFFKBN") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                For Each MB0004row As DataRow In MB0004tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MB0004row("CAMPCODE") AndAlso
                        XLSTBLrow("HORG") = MB0004row("HORG") AndAlso
                        XLSTBLrow("STAFFKBN") = MB0004row("STAFFKBN") AndAlso
                        XLSTBLrow("STYMD") = MB0004row("STYMD") Then
                        MB0004INProw.ItemArray = MB0004row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MB0004INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '配属部署
            If WW_COLUMNS.IndexOf("HORG") >= 0 Then
                MB0004INProw("HORG") = XLSTBLrow("HORG")
            End If

            '職務区分
            If WW_COLUMNS.IndexOf("STAFFKBN") >= 0 Then
                MB0004INProw("STAFFKBN") = XLSTBLrow("STAFFKBN")
            End If

            '開始年月日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        MB0004INProw("STYMD") = ""
                    Else
                        MB0004INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    MB0004INProw("STYMD") = ""
                End Try
            End If

            '終了年月日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        MB0004INProw("ENDYMD") = ""
                    Else
                        MB0004INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    MB0004INProw("ENDYMD") = ""
                End Try
            End If

            '所定労働時間
            If WW_COLUMNS.IndexOf("WORKINGH") >= 0 Then
                MB0004INProw("WORKINGH") = XLSTBLrow("WORKINGH")
            End If

            '所定労働日数
            If WW_COLUMNS.IndexOf("WORKINGN") >= 0 Then
                MB0004INProw("WORKINGN") = XLSTBLrow("WORKINGN")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MB0004INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '○ 名称取得
            CODENAME_get("CAMPCODE", MB0004INProw("CAMPCODE"), MB0004INProw("CAMPNAMES"), WW_DUMMY)             '会社コード
            CODENAME_get("HORG", MB0004INProw("HORG"), MB0004INProw("HORGNAMES"), WW_DUMMY)                     '配属部署
            CODENAME_get("STAFFKBN", MB0004INProw("STAFFKBN"), MB0004INProw("STAFFKBNNAMES"), WW_DUMMY)         '職務区分

            MB0004INPtbl.Rows.Add(MB0004INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        MB0004tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(MB0004tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

        WF_HORG.Focus()

    End Sub


    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToMB0004INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            MB0004tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MB0004tbl)

        '○ 詳細画面初期化
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        WF_HORG.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMB0004INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_HORG.Text)              '配属部署
        Master.eraseCharToIgnore(WF_STAFFKBN.Text)          '職務区分
        Master.eraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.eraseCharToIgnore(WF_DELFLG.Text)            '削除

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_HORG.Text) AndAlso
            String.IsNullOrEmpty(WF_STAFFKBN.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(MB0004INPtbl)
        Dim MB0004INProw As DataRow = MB0004INPtbl.NewRow

        '○ 初期クリア
        For Each MB0004INPcol As DataColumn In MB0004INPtbl.Columns
            If IsDBNull(MB0004INProw.Item(MB0004INPcol)) OrElse IsNothing(MB0004INProw.Item(MB0004INPcol)) Then
                Select Case MB0004INPcol.ColumnName
                    Case "LINECNT"
                        MB0004INProw.Item(MB0004INPcol) = 0
                    Case "OPERATION"
                        MB0004INProw.Item(MB0004INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        MB0004INProw.Item(MB0004INPcol) = 0
                    Case "SELECT"
                        MB0004INProw.Item(MB0004INPcol) = 1
                    Case "HIDDEN"
                        MB0004INProw.Item(MB0004INPcol) = 0
                    Case Else
                        MB0004INProw.Item(MB0004INPcol) = ""
                End Select
            End If
        Next

        '○ LINECNT取得
        If WF_Sel_LINECNT.Text = "" Then
            MB0004INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, MB0004INProw("LINECNT"))
            Catch ex As Exception
                MB0004INProw("LINECNT") = 0
            End Try
        End If

        MB0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        MB0004INProw("TIMSTP") = 0
        MB0004INProw("SELECT") = 1
        MB0004INProw("HIDDEN") = 0

        MB0004INProw("CAMPCODE") = WF_CAMPCODE.Text         '会社コード
        MB0004INProw("HORG") = WF_HORG.Text                 '配属部署
        MB0004INProw("STAFFKBN") = WF_STAFFKBN.Text         '職務区分
        MB0004INProw("STYMD") = WF_STYMD.Text               '有効年月日(From)
        MB0004INProw("ENDYMD") = WF_ENDYMD.Text             '有効年月日(To)
        MB0004INProw("DELFLG") = WF_DELFLG.Text             '削除

        MB0004INProw("CAMPNAMES") = ""                      '会社名称
        MB0004INProw("HORGNAMES") = ""                      '配属部署名
        MB0004INProw("STAFFKBNNAMES") = ""                  '職務区分名
        MB0004INProw("WORKINGH") = ""                       '所定労働時間
        MB0004INProw("WORKINGN") = ""                       '所定労働日数

        '○ Detail設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MB0004INProw(CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '中
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MB0004INProw(CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MB0004INProw(CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
            End If
        Next

        '○ 名称取得
        CODENAME_get("CAMPCODE", MB0004INProw("CAMPCODE"), MB0004INProw("CAMPNAMES"), WW_DUMMY)             '会社コード
        CODENAME_get("HORG", MB0004INProw("HORG"), MB0004INProw("HORGNAMES"), WW_DUMMY)                     '配属部署
        CODENAME_get("STAFFKBN", MB0004INProw("STAFFKBN"), MB0004INProw("STAFFKBNNAMES"), WW_DUMMY)         '職務区分

        '○ チェック用テーブルに登録する
        MB0004INPtbl.Rows.Add(MB0004INProw)

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_HORG.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each MB0004row As DataRow In MB0004tbl.Rows
            Select Case MB0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(MB0004tbl)

        WF_Sel_LINECNT.Text = ""                            'LINECNT
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_HORG.Text = ""                                   '配属部署
        WF_HORG_TEXT.Text = ""                              '配属部署名
        WF_STAFFKBN.Text = ""                               '職務区分
        WF_STAFFKBN_TEXT.Text = ""                          '職務区分名
        WF_STYMD.Text = ""                                  '有効年月日(From)
        WF_ENDYMD.Text = ""                                 '有効年月日(To)
        WF_DELFLG.Text = ""                                 '削除
        WF_DELFLG_TEXT.Text = ""                            '削除名称

        '○ 詳細画面初期設定
        DetailInitialize()

    End Sub

    ''' <summary>
    ''' 詳細画面-初期設定 (空明細作成 ＆ イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailInitialize()

        Dim WW_DataTable As DataTable = New DataTable
        Dim WW_RepField As Label = Nothing
        Dim WW_RepValue As TextBox = Nothing
        Dim WW_RepName As Label = Nothing
        Dim WW_RepAttr As String = ""

        Try
            '○ カラム情報をリピーター作成用に取得
            Master.CreateEmptyTable(WW_DataTable)
            WW_DataTable.Rows.Add(WW_DataTable.NewRow())

            '○ リピーター作成
            CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0052DetailView.PROFID = Master.PROF_VIEW
            CS0052DetailView.MAPID = Master.MAPID
            CS0052DetailView.VARI = Master.VIEWID
            CS0052DetailView.TABID = CONST_DETAIL_TABID
            CS0052DetailView.SRCDATA = WW_DataTable
            CS0052DetailView.REPEATER = WF_DViewRep1
            CS0052DetailView.COLPREFIX = "WF_Rep1_"
            CS0052DetailView.MaketDetailView()
            If Not isNormal(CS0052DetailView.ERR) Then
                Master.output(CS0052DetailView.ERR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If

            WF_DetailMView.ActiveViewIndex = 0

            '○ ダブルクリック時検索イベント追加
            For row As Integer = 0 To CS0052DetailView.ROWMAX - 1
                For col As Integer = 1 To CS0052DetailView.COLMAX
                    WW_RepField = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), Label)

                    If WW_RepField.Text <> "" Then
                        WW_RepValue = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_VALUE_" & col), TextBox)
                        ATTR_get(WW_RepField.Text, WW_RepAttr)

                        If WW_RepAttr <> "" AndAlso Not WW_RepValue.ReadOnly Then
                            WW_RepValue.Attributes.Remove("ondblclick")
                            WW_RepValue.Attributes.Add("ondblclick", WW_RepAttr)
                            WW_RepName = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELDNM_" & col), Label)
                            WW_RepName.Attributes.Remove("style")
                            WW_RepName.Attributes.Add("style", "text-decoration:underline;")
                        End If
                    End If
                Next col
            Next row

            WF_DViewRep1.Visible = True
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
        Finally
            WW_DataTable.Dispose()
            WW_DataTable = Nothing
        End Try

    End Sub

    ''' <summary>
    ''' 詳細画面-イベント文字取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="O_ATTR"></param>
    ''' <remarks></remarks>
    Protected Sub ATTR_get(ByVal I_FIELD As String, ByRef O_ATTR As String)

        O_ATTR = ""

        Select Case I_FIELD
            Case ""
        End Select

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

            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WW_FIELD
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
                        Select Case WW_FIELD
                            Case "WF_SELHORG", "WF_HORG"        '配属部署
                                prmData = work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text)
                        End Select

                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
                End Select
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

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValue = leftview.getActiveValue(0)
            WW_SelectText = leftview.getActiveValue(1)
        End If

        '○ 選択内容を画面項目へセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "WF_SELHORG"           '配属部署
                    WF_SELHORG.Text = WW_SelectValue
                    WF_SELHORG_TEXT.Text = WW_SelectText
                    WF_SELHORG.Focus()

                Case "WF_SELSTAFFKBN"       '職務区分
                    WF_SELSTAFFKBN.Text = WW_SelectValue
                    WF_SELSTAFFKBN_TEXT.Text = WW_SelectText
                    WF_SELSTAFFKBN.Focus()

                Case "WF_HORG"              '配属部署
                    WF_HORG.Text = WW_SelectValue
                    WF_HORG_TEXT.Text = WW_SelectText
                    WF_HORG.Focus()

                Case "WF_STAFFKBN"          '職務区分
                    WF_STAFFKBN.Text = WW_SelectValue
                    WF_STAFFKBN_TEXT.Text = WW_SelectText

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

        Else
            '○ 明細部設定
            For Each reitem As RepeaterItem In WF_DViewRep1.Items
                '左
                If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_SelectText
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '中
                If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_SelectText
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Focus()
                    Exit For
                End If

                '右
                If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_SelectText
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Focus()
                    Exit For
                End If
            Next
        End If

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
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "WF_SELHORG"           '配属部署
                    WF_SELHORG.Focus()
                Case "WF_SELSTAFFKBN"       '職務区分
                    WF_SELSTAFFKBN.Focus()
                Case "WF_HORG"              '配属部署
                    WF_HORG.Focus()
                Case "WF_STAFFKBN"          '職務区分
                    WF_STAFFKBN.Focus()
                Case "WF_STYMD"             '有効年月日(From)
                    WF_STYMD.Focus()
                Case "WF_ENDYMD"            '有効年月日(To)
                    WF_ENDYMD.Focus()
                Case "WF_DELFLG"            '削除
                    WF_DELFLG.Focus()
            End Select

        Else
            '○ 明細部設定
            For Each reitem As RepeaterItem In WF_DViewRep1.Items
                '左
                If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '中
                If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Focus()
                    Exit For
                End If

                '右
                If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Focus()
                    Exit For
                End If
            Next
        End If

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
            WW_CheckMES1 = "・更新できないレコード(ユーザに画面更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_COMP
        CS0025AUTHORget.CODE = work.WF_SEL_CAMPCODE.Text
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・更新できないレコード(ユーザに会社更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ ユーザー組織取得
        Dim WW_ORGLEVEL As String = ""
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_ORGLEVEL = ORGLEVELget(SQLcon)
        End Using

        '○ 単項目チェック
        For Each MB0004INProw As DataRow In MB0004INPtbl.Rows

            WW_LINE_ERR = ""

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MB0004INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MB0004INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '配属部署
            WW_TEXT = MB0004INProw("HORG")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "HORG", MB0004INProw("HORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MB0004INProw("HORG") = ""
                Else
                    '存在チェック
                    CODENAME_get("HORG", MB0004INProw("HORG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(配属部署エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If

                    '権限チェック
                    CS0025AUTHORget.USERID = CS0050SESSION.USERID
                    CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                    CS0025AUTHORget.CODE = MB0004INProw("HORG")
                    CS0025AUTHORget.STYMD = Date.Now
                    CS0025AUTHORget.ENDYMD = Date.Now
                    CS0025AUTHORget.CS0025AUTHORget()
                    If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                    Else
                        WW_CheckMES1 = "・更新できないレコード(配属部署権限無)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(配属部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '職務区分
            WW_TEXT = MB0004INProw("STAFFKBN")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STAFFKBN", MB0004INProw("STAFFKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MB0004INProw("STAFFKBN") = ""
                Else
                    '存在チェック
                    CODENAME_get("STAFFKBN", MB0004INProw("STAFFKBN"), WW_DUMMY, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        '支店、営業所レベルの場合、正社員(職務区分から)は変更不可
                        If WW_ORGLEVEL = "00100" AndAlso Len(MB0004INProw("STAFFKBN")) >= 4 Then
                            Dim WW_CODE As String = MB0004INProw("STAFFKBN").ToString().Substring(2, 2)
                            If ((WW_CODE >= "10" AndAlso WW_CODE <= "30") OrElse WW_CODE = "41") AndAlso
                                MB0004INProw("STAFFKBN").ToString().Substring(1, 3) <> "430" Then
                                WW_CheckMES1 = "・エラーが存在します。(正社員・契約社員の変更はできません。)"
                                WW_CheckMES2 = ""
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                                WW_LINE_ERR = "ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(職務区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(職務区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", MB0004INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", MB0004INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '日付大小チェック
            If MB0004INProw("STYMD") > MB0004INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(有効開始日＞有効終了日)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '範囲チェック
            If work.WF_SEL_STYMD.Text > MB0004INProw("STYMD") AndAlso
                work.WF_SEL_STYMD.Text > MB0004INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            If work.WF_SEL_ENDYMD.Text < MB0004INProw("STYMD") AndAlso
                work.WF_SEL_ENDYMD.Text < MB0004INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除フラグ
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", MB0004INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", MB0004INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '所定労働時間
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "WORKINGH", MB0004INProw("WORKINGH"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "WORKINGH", MB0004INProw("WORKINGH"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    Try
                        Dim WW_DATE As DateTime = Date.Parse(MB0004INProw("WORKINGH"))
                        MB0004INProw("WORKINGH") = WW_DATE.ToString("HH:mm")
                    Catch ex As Exception
                        MB0004INProw("WORKINGH") = "00:00"
                    End Try
                Else
                    WW_CheckMES1 = "・更新できないレコード(所定労働時間エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(所定労働時間エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '所定労働日数
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "WORKINGN", MB0004INProw("WORKINGN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "WORKINGN", MB0004INProw("WORKINGN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    Try
                        Dim WW_INT As Integer = MB0004INProw("WORKINGN")
                        MB0004INProw("WORKINGN") = WW_INT.ToString()
                    Catch ex As Exception
                        MB0004INProw("WORKINGN") = ""
                    End Try
                Else
                    WW_CheckMES1 = "・更新できないレコード(所定労働日数エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(所定労働日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If MB0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MB0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MB0004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 組織レベル取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function ORGLEVELget(ByVal SQLcon As SqlConnection) As String

        ORGLEVELget = ""

        Dim SQLStr As String =
              " SELECT" _
            & "    RTRIM(M02B.ORGLEVEL) AS ORGLEVEL" _
            & " FROM" _
            & "    S0004_USER S004" _
            & "    INNER JOIN M0002_ORG M02A" _
            & "        ON  M02A.CAMPCODE = @P1" _
            & "        AND M02A.ORGCODE  = S004.ORG" _
            & "        AND M02A.STYMD   <= @P3" _
            & "        AND M02A.ENDYMD  >= @P3" _
            & "        AND M02A.DELFLG  <> @P4" _
            & "    INNER JOIN M0002_ORG M02B" _
            & "        ON  M02A.CAMPCODE = @P1" _
            & "        AND M02A.MORGCODE = M02B.ORGCODE" _
            & "        AND M02A.STYMD   <= @P3" _
            & "        AND M02A.ENDYMD  >= @P3" _
            & "        AND M02A.DELFLG  <> @P4" _
            & " WHERE" _
            & "    S004.USERID      = @P2" _
            & "    AND S004.STYMD  <= @P3" _
            & "    AND S004.ENDYMD >= @P3" _
            & "    AND S004.DELFLG <> @P4"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NChar, 20)       '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NChar, 20)       'ユーザーID
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)            '現在日付
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NChar, 1)        '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = CS0050SESSION.USERID
                PARA3.Value = Date.Now
                PARA4.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        ORGLEVELget = SQLdr("ORGLEVEL")
                        Exit While
                    End While
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0004_USER, M0002_ORG SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "S0004_USER, M0002_ORG SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
        End Try

    End Function

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="MB0004row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal MB0004row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(MB0004row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社         =" & MB0004row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 配属部署     =" & MB0004row("HORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 職務区分     =" & MB0004row("STAFFKBN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日   =" & MB0004row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日   =" & MB0004row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 所定労働時間 =" & MB0004row("WORKINGH") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 所定労働日数 =" & MB0004row("WORKINGN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除         =" & MB0004row("DELFLG")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' MB0004tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MB0004tbl_UPD()

        '○ 画面状態設定
        For Each MB0004row As DataRow In MB0004tbl.Rows
            Select Case MB0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MB0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each MB0004INProw As DataRow In MB0004INPtbl.Rows

            'エラーレコード読み飛ばし
            If MB0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            MB0004INProw("OPERATION") = "Insert"

            'KEY項目が等しい(ENDYMD以外のKEYが同じ)
            For Each MB0004row As DataRow In MB0004tbl.Rows
                If MB0004row("CAMPCODE") = MB0004INProw("CAMPCODE") AndAlso
                    MB0004row("HORG") = MB0004INProw("HORG") AndAlso
                    MB0004row("STAFFKBN") = MB0004INProw("STAFFKBN") AndAlso
                    MB0004row("STYMD") = MB0004INProw("STYMD") Then

                    '変更無は操作無
                    If MB0004row("ENDYMD") = MB0004INProw("ENDYMD") AndAlso
                        MB0004row("WORKINGH") = MB0004INProw("WORKINGH") AndAlso
                        MB0004row("WORKINGN") = MB0004INProw("WORKINGN") AndAlso
                        MB0004row("DELFLG") = MB0004INProw("DELFLG") Then
                        MB0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    MB0004INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each MB0004INProw As DataRow In MB0004INPtbl.Rows
            Select Case MB0004INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(MB0004INProw)
                Case "Insert"
                    TBL_INSERT_SUB(MB0004INProw)
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="MB0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef MB0004INProw As DataRow)

        For Each MB0004row As DataRow In MB0004tbl.Rows

            '同一(ENDYMD以外が同一KEY)レコード
            If MB0004row("CAMPCODE") = MB0004INProw("CAMPCODE") AndAlso
                MB0004row("HORG") = MB0004INProw("HORG") AndAlso
                MB0004row("STAFFKBN") = MB0004INProw("STAFFKBN") AndAlso
                MB0004row("STYMD") = MB0004INProw("STYMD") Then

                '画面入力テーブル項目設定
                MB0004INProw("LINECNT") = MB0004row("LINECNT")
                MB0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MB0004INProw("TIMSTP") = MB0004row("TIMSTP")
                MB0004INProw("SELECT") = 1
                MB0004INProw("HIDDEN") = 0

                '項目テーブル項目設定
                MB0004row.ItemArray = MB0004INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="MB0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef MB0004INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim MB0004row As DataRow = MB0004tbl.NewRow
        MB0004row.ItemArray = MB0004INProw.ItemArray

        MB0004row("LINECNT") = MB0004tbl.Rows.Count + 1
        MB0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        MB0004row("TIMSTP") = "0"
        MB0004row("SELECT") = 1
        MB0004row("HIDDEN") = 0

        MB0004tbl.Rows.Add(MB0004row)

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
                Case "HORG"             '配属部署
                    prmData = work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"         '職務区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFKBN, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

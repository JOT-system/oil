Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 変数入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRCO0007PROFMVARI
    Inherits Page

    '○ 検索結果格納Table
    Private CO0007tbl As DataTable                          '一覧格納用テーブル
    Private CO0007INPtbl As DataTable                       'チェック用テーブル
    Private CO0007UPDtbl As DataTable                       '更新用テーブル

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
                    If Not Master.RecoverTable(CO0007tbl) Then
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
            If Not IsNothing(CO0007tbl) Then
                CO0007tbl.Clear()
                CO0007tbl.Dispose()
                CO0007tbl = Nothing
            End If

            If Not IsNothing(CO0007INPtbl) Then
                CO0007INPtbl.Clear()
                CO0007INPtbl.Dispose()
                CO0007INPtbl = Nothing
            End If

            If Not IsNothing(CO0007UPDtbl) Then
                CO0007UPDtbl.Clear()
                CO0007UPDtbl.Dispose()
                CO0007UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRCO0007WRKINC.MAPID

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.CO0007S Then
            'Grid情報保存先のファイル名
            Master.createXMLSaveFile()

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
        Master.SaveTable(CO0007tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(CO0007tbl)

        TBLview.RowFilter = "TITLEKBN = 'H' and LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

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

        If IsNothing(CO0007tbl) Then
            CO0007tbl = New DataTable
        End If

        If CO0007tbl.Columns.Count <> 0 Then
            CO0007tbl.Columns.Clear()
        End If

        CO0007tbl.Clear()

        '○ 検索SQL文
        '  検索説明
        '    メンテナンス可能USERのPROFIDおよびデフォルトPROFIDのTBL(S0023_PROFMVARI)を取得
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
            & "    0                                          AS LINECNT" _
            & "    , ''                                       AS OPERATION" _
            & "    , CAST(UPDTIMSTP AS BIGINT)                AS TIMSTP" _
            & "    , 1                                        AS 'SELECT'" _
            & "    , 0                                        AS HIDDEN" _
            & "    , ISNULL(RTRIM(CAMPCODE), '')              AS CAMPCODE" _
            & "    , ''                                       AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(PROFID), '')                AS PROFID" _
            & "    , ISNULL(RTRIM(MAPID), '')                 AS MAPID" _
            & "    , ''                                       AS MAPNAMES" _
            & "    , ISNULL(RTRIM(VARIANT), '')               AS VARIANT" _
            & "    , ISNULL(RTRIM(TITLEKBN), '')              AS TITLEKBN" _
            & "    , ISNULL(SEQ, 0)                           AS SEQ" _
            & "    , ISNULL(RTRIM(FIELD), '')                 AS FIELD" _
            & "    , ISNULL(FORMAT(STYMD, 'yyyy/MM/dd'), '')  AS STYMD" _
            & "    , ISNULL(FORMAT(ENDYMD, 'yyyy/MM/dd'), '') AS ENDYMD" _
            & "    , ISNULL(RTRIM(VARIANTNAMES), '')          AS VARIANTNAMES" _
            & "    , ISNULL(RTRIM(TITLENAMES), '')            AS TITLENAMES" _
            & "    , ISNULL(RTRIM(VALUETYPE), '')             AS VALUETYPE" _
            & "    , ''                                       AS VALUETYPENAMES" _
            & "    , ISNULL(RTRIM(VALUE), '')                 AS VALUE" _
            & "    , ISNULL(RTRIM(VALUEADDYY), '0')           AS VALUEADDYY" _
            & "    , ISNULL(RTRIM(VALUEADDMM), '0')           AS VALUEADDMM" _
            & "    , ISNULL(RTRIM(VALUEADDDD), '0')           AS VALUEADDDD" _
            & "    , ISNULL(RTRIM(DELFLG), '')                AS DELFLG" _
            & "    , ''                                       AS DELFLGNAMES" _
            & " FROM" _
            & "    S0023_PROFMVARI" _
            & " WHERE" _
            & "    CAMPCODE    = @P1" _
            & "    AND PROFID IN ('" & C_DEFAULT_DATAKEY & "', @P2)" _
            & "    AND STYMD  <= @P3" _
            & "    AND ENDYMD >= @P4" _
            & "    AND DELFLG <> @P5"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '画面ID(From)
        If Not String.IsNullOrEmpty(work.WF_SEL_MAPIDF.Text) Then
            SQLStr &= String.Format("    AND MAPID  >= '{0}'", work.WF_SEL_MAPIDF.Text)
        End If
        '画面ID(To)
        If Not String.IsNullOrEmpty(work.WF_SEL_MAPIDT.Text) Then
            SQLStr &= String.Format("    AND MAPID  <= '{0}'", work.WF_SEL_MAPIDT.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    CAMPCODE" _
            & "    , PROFID" _
            & "    , MAPID" _
            & "    , VARIANT" _
            & "    , TITLEKBN" _
            & "    , STYMD" _
            & "    , SEQ" _
            & "    , FIELD"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'プロフID
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '有効年月日(To)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '有効年月日(From)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.PROF_VIEW
                PARA3.Value = work.WF_SEL_ENDYMD.Text
                PARA4.Value = work.WF_SEL_STYMD.Text
                PARA5.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        CO0007tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    CO0007tbl.Load(SQLdr)
                End Using

                For Each CO0007row As DataRow In CO0007tbl.Rows
                    '名称取得
                    CODENAME_get("CAMPCODE", CO0007row("CAMPCODE"), CO0007row("CAMPNAMES"), WW_DUMMY)               '会社コード
                    CODENAME_get("MAPID", CO0007row("MAPID"), CO0007row("MAPNAMES"), WW_DUMMY)                      '画面ID
                    CODENAME_get("VALUETYPE", CO0007row("VALUETYPE"), CO0007row("VALUETYPENAMES"), WW_DUMMY)        '値タイプ
                    CODENAME_get("DELFLG", CO0007row("DELFLG"), CO0007row("DELFLGNAMES"), WW_DUMMY)                 '削除
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0023_PROFMVARI SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0023_PROFMVARI Select"
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
        CS0026TBLSORT.TABLE = CO0007tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = "TITLEKBN = 'H'"
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            CO0007tbl = CS0026TBLSORT.TABLE
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
            Exit Sub
        End If

        '○ 表示対象行カウント(絞り込み対象)
        '   ※ 絞込 (Cell(4) : 0=表示対象, 1=非表示対象)
        For Each CO0007row As DataRow In CO0007tbl.Rows
            If CO0007row("HIDDEN") = 0 AndAlso CO0007row("TITLEKBN") = "H" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                CO0007row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(CO0007tbl)

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
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.eraseCharToIgnore(WF_SELMAPID.Text)

        '○ 名称取得
        CODENAME_get("MAPID", WF_SELMAPID.Text, WF_SELMAPID_TEXT.Text, WW_DUMMY)

        '○ 絞り込み操作(GridView明細Hidden設定)
        For Each CO0007row As DataRow In CO0007tbl.Rows

            '一度非表示にする
            CO0007row("HIDDEN") = 1

            Dim WW_HANTEI As Boolean = True

            '画面IDによる絞込判定
            If WF_SELMAPID.Text <> "" AndAlso
                WF_SELMAPID.Text <> CO0007row("MAPID") Then
                WW_HANTEI = False
            End If

            '画面(GridView)のHIDDENに結果格納
            If WW_HANTEI Then
                CO0007row("HIDDEN") = 0
            End If
        Next

        '○ 画面先頭を表示
        WF_GridPosition.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(CO0007tbl)

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

                'プロファイルマスタ(変数)更新
                UpdateProfileMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0007tbl)

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
        For Each CO0007row As DataRow In CO0007tbl.Rows

            '読み飛ばし
            If (CO0007row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                CO0007row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                CO0007row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                CO0007row("STYMD") = "" Then
                Continue For
            End If

            WW_LINE_ERR = ""

            'チェック
            For Each CO0007chk As DataRow In CO0007tbl.Rows

                '同一KEY以外は読み飛ばし
                If CO0007row("CAMPCODE") <> CO0007chk("CAMPCODE") OrElse
                    CO0007row("PROFID") <> CO0007chk("PROFID") OrElse
                    CO0007row("MAPID") <> CO0007chk("MAPID") OrElse
                    CO0007row("VARIANT") <> CO0007chk("VARIANT") OrElse
                    CO0007row("TITLEKBN") <> CO0007chk("TITLEKBN") OrElse
                    CO0007row("FIELD") <> CO0007chk("FIELD") OrElse
                    CO0007chk("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If CO0007row("STYMD") = CO0007chk("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(CO0007row("STYMD"), WW_DATE_ST)
                    Date.TryParse(CO0007row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(CO0007chk("STYMD"), WW_DATE_ST2)
                    Date.TryParse(CO0007chk("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                    Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End Try

                '開始日チェック
                If WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", CO0007row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If

                '終了日チェック
                If WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", CO0007row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If
            Next

            If WW_LINE_ERR = "" Then
                CO0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
            & "        S0023_PROFMVARI" _
            & "    WHERE" _
            & "        CAMPCODE     = @P1" _
            & "        AND PROFID   = @P2" _
            & "        AND MAPID    = @P3" _
            & "        AND VARIANT  = @P4" _
            & "        AND TITLEKBN = @P5" _
            & "        AND FIELD    = @P7" _
            & "        AND STYMD    = @P8 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE S0023_PROFMVARI" _
            & "    SET" _
            & "        SEQ            = @P6     , ENDYMD     = @P9" _
            & "        , VARIANTNAMES = @P10    , TITLENAMES = @P11" _
            & "        , VALUETYPE    = @P12    , VALUE      = @P13" _
            & "        , VALUEADDYY   = @P14    , VALUEADDMM = @P15" _
            & "        , VALUEADDDD   = @P16    , DELFLG     = @P17" _
            & "        , UPDYMD       = @P19    , UPDUSER    = @P20" _
            & "        , UPDTERMID    = @P21    , RECEIVEYMD = @P22" _
            & "    WHERE" _
            & "        CAMPCODE     = @P1" _
            & "        AND PROFID   = @P2" _
            & "        AND MAPID    = @P3" _
            & "        AND VARIANT  = @P4" _
            & "        AND TITLEKBN = @P5" _
            & "        AND FIELD    = @P7" _
            & "        AND STYMD    = @P8 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO S0023_PROFMVARI" _
            & "        (CAMPCODE       , PROFID" _
            & "        , MAPID         , VARIANT" _
            & "        , TITLEKBN      , SEQ" _
            & "        , FIELD         , STYMD" _
            & "        , ENDYMD        , VARIANTNAMES" _
            & "        , TITLENAMES    , VALUETYPE" _
            & "        , VALUE         , VALUEADDYY" _
            & "        , VALUEADDMM    , VALUEADDDD" _
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
            & "        , @P17    , @P18" _
            & "        , @P19    , @P20" _
            & "        , @P21    , @P22) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , PROFID" _
            & "    , MAPID" _
            & "    , VARIANT" _
            & "    , TITLEKBN" _
            & "    , SEQ" _
            & "    , FIELD" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , VARIANTNAMES" _
            & "    , TITLENAMES" _
            & "    , VALUETYPE" _
            & "    , VALUE" _
            & "    , VALUEADDYY" _
            & "    , VALUEADDMM" _
            & "    , VALUEADDDD" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    S0023_PROFMVARI" _
            & " WHERE" _
            & "    CAMPCODE     = @P1" _
            & "    AND PROFID   = @P2" _
            & "    AND MAPID    = @P3" _
            & "    AND VARIANT  = @P4" _
            & "    AND TITLEKBN = @P5" _
            & "    AND FIELD    = @P6" _
            & "    AND STYMD    = @P7"

        Try
            'DB更新
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            'プロファイルID
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 50)            '画面ID
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 50)            '変数
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)             'タイトル区分
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Int)                     '表示順番
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 50)            '項目
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Date)                    '開始年月日
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.Date)                    '終了年月日
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)          '変数名称
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          'タイトル名称
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 10)          '値タイプ
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 50)          '値
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.Int)                   '値加算年
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.Int)                   '値加算月
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.Int)                   '値加算日
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.DateTime)              '登録年月日
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.DateTime)              '更新年月日
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'プロファイルID
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 50)        '画面ID
                Dim JPARA4 As SqlParameter = SQLcmdJnl.Parameters.Add("@P4", SqlDbType.NVarChar, 50)        '変数
                Dim JPARA5 As SqlParameter = SQLcmdJnl.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         'タイトル区分
                Dim JPARA6 As SqlParameter = SQLcmdJnl.Parameters.Add("@P6", SqlDbType.NVarChar, 50)        '項目
                Dim JPARA7 As SqlParameter = SQLcmdJnl.Parameters.Add("@P7", SqlDbType.Date)                '開始年月日

                For Each CO0007row As DataRow In CO0007tbl.Rows
                    If Trim(CO0007row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(CO0007row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = CO0007row("CAMPCODE")
                        PARA2.Value = CO0007row("PROFID")
                        PARA3.Value = CO0007row("MAPID")
                        PARA4.Value = CO0007row("VARIANT")
                        PARA5.Value = CO0007row("TITLEKBN")
                        PARA6.Value = CO0007row("SEQ")
                        PARA7.Value = CO0007row("FIELD")
                        PARA8.Value = CO0007row("STYMD")
                        PARA9.Value = CO0007row("ENDYMD")
                        PARA10.Value = CO0007row("VARIANTNAMES")
                        PARA11.Value = CO0007row("TITLENAMES")
                        PARA12.Value = CO0007row("VALUETYPE")
                        PARA13.Value = CO0007row("VALUE")
                        PARA14.Value = CO0007row("VALUEADDYY")
                        PARA15.Value = CO0007row("VALUEADDMM")
                        PARA16.Value = CO0007row("VALUEADDDD")
                        PARA17.Value = CO0007row("DELFLG")
                        PARA18.Value = WW_DATENOW
                        PARA19.Value = WW_DATENOW
                        PARA20.Value = Master.USERID
                        PARA21.Value = Master.USERTERMID
                        PARA22.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = CO0007row("CAMPCODE")
                        JPARA2.Value = CO0007row("PROFID")
                        JPARA3.Value = CO0007row("MAPID")
                        JPARA4.Value = CO0007row("VARIANT")
                        JPARA5.Value = CO0007row("TITLEKBN")
                        JPARA6.Value = CO0007row("FIELD")
                        JPARA7.Value = CO0007row("STYMD")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(CO0007UPDtbl) Then
                                CO0007UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    CO0007UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            CO0007UPDtbl.Clear()
                            CO0007UPDtbl.Load(SQLdr)
                        End Using

                        For Each CO0007UPDrow As DataRow In CO0007UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "S0023_PROFMVARI"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = CO0007UPDrow
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
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0023_PROFMVARI UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0023_PROFMVARI UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = CO0007tbl                        'データ参照  Table
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
        CS0030REPORT.TBLDATA = CO0007tbl                        'データ参照Table
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
        Dim TBLview As New DataView(CO0007tbl)
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
        Dim WW_FIELD_OBJ As Object = Nothing
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try

        For i As Integer = 0 To CO0007tbl.Rows.Count - 1
            If CO0007tbl.Rows(i)("LINECNT") = WW_LINECNT Then
                WW_LINECNT = i
                Exit For
            End If
        Next

        '選択行
        WF_Sel_LINECNT.Text = CO0007tbl.Rows(WW_LINECNT)("LINECNT")

        '会社コード
        WF_CAMPCODE.Text = CO0007tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        'プロフID
        WF_PROFID.Text = CO0007tbl.Rows(WW_LINECNT)("PROFID")

        '画面ID
        WF_MAPID.Text = CO0007tbl.Rows(WW_LINECNT)("MAPID")
        CODENAME_get("MAPID", WF_MAPID.Text, WF_MAPID_TEXT.Text, WW_DUMMY)

        '変数・名称
        WF_VARIANT.Text = CO0007tbl.Rows(WW_LINECNT)("VARIANT")
        WF_VARIANTNAMES.Text = CO0007tbl.Rows(WW_LINECNT)("VARIANTNAMES")

        '有効年月日
        WF_STYMD.Text = CO0007tbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = CO0007tbl.Rows(WW_LINECNT)("ENDYMD")

        '削除
        WF_DELFLG.Text = CO0007tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        Dim CO0007VARItbl As New DataTable
        CS0026TBLSORT.TABLE = CO0007tbl
        CS0026TBLSORT.SORTING = "SEQ, FIELD"
        CS0026TBLSORT.FILTER = "CAMPCODE = '" & WF_CAMPCODE.Text & "'" _
            & " and PROFID = '" & WF_PROFID.Text & "'" _
            & " and MAPID = '" & WF_MAPID.Text & "'" _
            & " and VARIANT = '" & WF_VARIANT.Text & "'" _
            & " and STYMD = '" & WF_STYMD.Text & "'" _
            & " and TITLEKBN = 'I'"
        CS0026TBLSORT.sort(CO0007VARItbl)

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = True
        WF_Repeater.DataSource = CO0007VARItbl
        WF_Repeater.DataBind()

        '○ 明細作成
        For i As Integer = 0 To WF_Repeater.Items.Count - 1
            Dim CO0007VARIrow As DataRow = CO0007VARItbl.Rows(i)
            Dim reitem As RepeaterItem = WF_Repeater.Items(i)

            '項番
            CType(reitem.FindControl("WF_Rep_SEQ"), Label).Text = CO0007VARIrow("SEQ")

            '項目(名称)
            CType(reitem.FindControl("WF_Rep_TITLENAMES"), Label).Text = CO0007VARIrow("TITLENAMES")

            '項目(記号名)
            CType(reitem.FindControl("WF_Rep_FIELD"), Label).Text = CO0007VARIrow("FIELD")

            '値タイプ
            CType(reitem.FindControl("WF_Rep_VALUETYPE"), TextBox).Text = CO0007VARIrow("VALUETYPE")
            CType(reitem.FindControl("WF_Rep_VALUETYPE"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_VALUETYPE"), TextBox).Attributes.Add("ondblclick", "REF_Field_DBclick('" & i & "', 'WF_Rep_VALUETYPE', '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');")

            '値
            CType(reitem.FindControl("WF_Rep_VALUE"), TextBox).Text = CO0007VARIrow("VALUE")

            '値加算(年)
            Try
                Dim WW_NUM As Integer
                Integer.TryParse(CO0007VARIrow("VALUEADDYY"), WW_NUM)
                CType(reitem.FindControl("WF_Rep_VALUEADDYY"), TextBox).Text = WW_NUM.ToString()
            Catch ex As Exception
                CType(reitem.FindControl("WF_Rep_VALUEADDYY"), TextBox).Text = "0"
            End Try

            '値加算(月)
            Try
                Dim WW_NUM As Integer
                Integer.TryParse(CO0007VARIrow("VALUEADDMM"), WW_NUM)
                CType(reitem.FindControl("WF_Rep_VALUEADDMM"), TextBox).Text = WW_NUM.ToString()
            Catch ex As Exception
                CType(reitem.FindControl("WF_Rep_VALUEADDMM"), TextBox).Text = "0"
            End Try

            '値加算(日)
            Try
                Dim WW_NUM As Integer
                Integer.TryParse(CO0007VARIrow("VALUEADDDD"), WW_NUM)
                CType(reitem.FindControl("WF_Rep_VALUEADDDD"), TextBox).Text = WW_NUM.ToString()
            Catch ex As Exception
                CType(reitem.FindControl("WF_Rep_VALUEADDDD"), TextBox).Text = "0"
            End Try
        Next

        CO0007VARItbl.Clear()
        CO0007VARItbl.Dispose()
        CO0007VARItbl = Nothing

        '○ 状態をクリア
        For Each CO0007row As DataRow In CO0007tbl.Rows
            Select Case CO0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case CO0007tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                CO0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                CO0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                CO0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                CO0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                CO0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(CO0007tbl)

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


    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks>アップロードは出来ないが記載</remarks>
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
        Master.CreateEmptyTable(CO0007INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim CO0007INProw As DataRow = CO0007INPtbl.NewRow

            '○ 初期クリア
            For Each CO0007INPcol As DataColumn In CO0007INPtbl.Columns
                If IsDBNull(CO0007INProw.Item(CO0007INPcol)) OrElse IsNothing(CO0007INProw.Item(CO0007INPcol)) Then
                    Select Case CO0007INPcol.ColumnName
                        Case "LINECNT"
                            CO0007INProw.Item(CO0007INPcol) = 0
                        Case "OPERATION"
                            CO0007INProw.Item(CO0007INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            CO0007INProw.Item(CO0007INPcol) = 0
                        Case "SELECT"
                            CO0007INProw.Item(CO0007INPcol) = 1
                        Case "HIDDEN"
                            CO0007INProw.Item(CO0007INPcol) = 0
                        Case "SEQ"
                            CO0007INProw.Item(CO0007INPcol) = 0
                        Case Else
                            CO0007INProw.Item(CO0007INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("PROFID") >= 0 AndAlso
                WW_COLUMNS.IndexOf("MAPID") >= 0 AndAlso
                WW_COLUMNS.IndexOf("VARIANT") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TITLEKBN") >= 0 AndAlso
                WW_COLUMNS.IndexOf("FIELD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                For Each CO0007row As DataRow In CO0007tbl.Rows
                    If XLSTBLrow("CAMPCODE") = CO0007row("CAMPCODE") AndAlso
                        XLSTBLrow("PROFID") = CO0007row("PROFID") AndAlso
                        XLSTBLrow("MAPID") = CO0007row("MAPID") AndAlso
                        XLSTBLrow("VARIANT") = CO0007row("VARIANT") AndAlso
                        XLSTBLrow("TITLEKBN") = CO0007row("TITLEKBN") AndAlso
                        XLSTBLrow("FIELD") = CO0007row("FIELD") AndAlso
                        XLSTBLrow("STYMD") = CO0007row("STYMD") Then
                        CO0007INProw.ItemArray = CO0007row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                CO0007INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            'プロファイルID
            If WW_COLUMNS.IndexOf("PROFID") >= 0 Then
                CO0007INProw("PROFID") = XLSTBLrow("PROFID")
            End If

            '画面ID
            If WW_COLUMNS.IndexOf("MAPID") >= 0 Then
                CO0007INProw("MAPID") = XLSTBLrow("MAPID")
            End If

            '変数
            If WW_COLUMNS.IndexOf("VARIANT") >= 0 Then
                CO0007INProw("VARIANT") = XLSTBLrow("VARIANT")
            End If

            'タイトル区分
            If WW_COLUMNS.IndexOf("TITLEKBN") >= 0 Then
                CO0007INProw("TITLEKBN") = XLSTBLrow("TITLEKBN")
            End If

            '表示順番
            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                CO0007INProw("SEQ") = XLSTBLrow("SEQ")
            End If

            '項目
            If WW_COLUMNS.IndexOf("FIELD") >= 0 Then
                CO0007INProw("FIELD") = XLSTBLrow("FIELD")
            End If

            '開始年月日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        CO0007INProw("STYMD") = ""
                    Else
                        CO0007INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    CO0007INProw("STYMD") = ""
                End Try
            End If

            '終了年月日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        CO0007INProw("ENDYMD") = ""
                    Else
                        CO0007INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    CO0007INProw("ENDYMD") = ""
                End Try
            End If

            '変数名称
            If WW_COLUMNS.IndexOf("VARIANTNAMES") >= 0 Then
                CO0007INProw("VARIANTNAMES") = XLSTBLrow("VARIANTNAMES")
            End If

            'タイトル名称
            If WW_COLUMNS.IndexOf("TITLENAMES") >= 0 Then
                CO0007INProw("TITLENAMES") = XLSTBLrow("TITLENAMES")
            End If

            '値タイプ
            If WW_COLUMNS.IndexOf("VALUETYPE") >= 0 Then
                CO0007INProw("VALUETYPE") = XLSTBLrow("VALUETYPE")
            End If

            '値
            If WW_COLUMNS.IndexOf("VALUE") >= 0 Then
                CO0007INProw("VALUE") = XLSTBLrow("VALUE")
            End If

            '値加算年
            If WW_COLUMNS.IndexOf("VALUEADDYY") >= 0 Then
                CO0007INProw("VALUEADDYY") = XLSTBLrow("VALUEADDYY")
            End If

            '値加算月
            If WW_COLUMNS.IndexOf("VALUEADDMM") >= 0 Then
                CO0007INProw("VALUEADDMM") = XLSTBLrow("VALUEADDMM")
            End If

            '値加算日
            If WW_COLUMNS.IndexOf("VALUEADDDD") >= 0 Then
                CO0007INProw("VALUEADDDD") = XLSTBLrow("VALUEADDDD")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                CO0007INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '○ 名称取得
            CODENAME_get("CAMPCODE", CO0007INProw("CAMPCODE"), CO0007INProw("CAMPNAMES"), WW_DUMMY)                 '会社コード
            CODENAME_get("MAPID", CO0007INProw("MAPID"), CO0007INProw("MAPIDNAMES"), WW_DUMMY)                      '画面ID
            CODENAME_get("VALUETYPE", CO0007INProw("VALUETYPE"), CO0007INProw("VALUETYPENAMES"), WW_DUMMY)          '値タイプ
            CODENAME_get("DELFLG", CO0007INProw("DELFLG"), CO0007INProw("DELFLGNAMES"), WW_DUMMY)                   '削除フラグ

            CO0007INPtbl.Rows.Add(CO0007INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        CO0007tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(CO0007tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

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
        DetailBoxToCO0007INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            CO0007tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0007tbl)

        '○ 詳細画面初期化
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            WF_SELMAPID.Focus()
            WF_DISP.Value = "headerbox"
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            WF_DISP.Value = "detailbox"
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToCO0007INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)              '会社コード
        Master.eraseCharToIgnore(WF_PROFID.Text)                'プロフID
        Master.eraseCharToIgnore(WF_MAPID.Text)                 '画面ID
        Master.eraseCharToIgnore(WF_VARIANT.Text)               '変数
        Master.eraseCharToIgnore(WF_VARIANTNAMES.Text)          '変数名称
        Master.eraseCharToIgnore(WF_STYMD.Text)                 '開始年月日
        Master.eraseCharToIgnore(WF_ENDYMD.Text)                '終了年月日
        Master.eraseCharToIgnore(WF_DELFLG.Text)                '削除フラグ

        Master.CreateEmptyTable(CO0007INPtbl)

        '○ ヘッダー分作成
        Dim CO0007INProw As DataRow = CO0007INPtbl.NewRow
        '○ 初期クリア
        For Each CO0007INPcol As DataColumn In CO0007INPtbl.Columns
            If IsDBNull(CO0007INProw.Item(CO0007INPcol)) OrElse IsNothing(CO0007INProw.Item(CO0007INPcol)) Then
                Select Case CO0007INPcol.ColumnName
                    Case "LINECNT"
                        CO0007INProw.Item(CO0007INPcol) = 0
                    Case "OPERATION"
                        CO0007INProw.Item(CO0007INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        CO0007INProw.Item(CO0007INPcol) = 0
                    Case "SELECT"
                        CO0007INProw.Item(CO0007INPcol) = 1
                    Case "HIDDEN"
                        CO0007INProw.Item(CO0007INPcol) = 0
                    Case "SEQ"
                        CO0007INProw.Item(CO0007INPcol) = 0
                    Case Else
                        CO0007INProw.Item(CO0007INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        Try
            Integer.TryParse(WF_Sel_LINECNT.Text, CO0007INProw("LINECNT"))
        Catch ex As Exception
            CO0007INProw("LINECNT") = 0
        End Try

        CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        CO0007INProw("TIMSTP") = 0
        CO0007INProw("SELECT") = 1
        CO0007INProw("HIDDEN") = 0

        CO0007INProw("CAMPCODE") = WF_CAMPCODE.Text                 '会社コード
        CO0007INProw("CAMPNAMES") = ""                              '会社名称
        CO0007INProw("PROFID") = WF_PROFID.Text                     'プロファイルID
        CO0007INProw("MAPID") = WF_MAPID.Text                       '画面ID
        CO0007INProw("MAPNAMES") = ""                               '画面名称
        CO0007INProw("VARIANT") = WF_VARIANT.Text                   '変数
        CO0007INProw("TITLEKBN") = "H"                              'タイトル区分
        CO0007INProw("SEQ") = 0                                     '表示順番
        CO0007INProw("FIELD") = C_DEFAULT_DATAKEY                   '項目
        CO0007INProw("STYMD") = WF_STYMD.Text                       '開始年月日
        CO0007INProw("ENDYMD") = WF_ENDYMD.Text                     '終了年月日
        CO0007INProw("VARIANTNAMES") = WF_VARIANTNAMES.Text         '変数名称
        CO0007INProw("TITLENAMES") = ""                             'タイトル名称
        CO0007INProw("VALUETYPE") = ""                              '値タイプ
        CO0007INProw("VALUETYPENAMES") = ""                         '値タイプ名称
        CO0007INProw("VALUE") = ""                                  '値
        CO0007INProw("VALUEADDYY") = "0"                            '値加算年
        CO0007INProw("VALUEADDMM") = "0"                            '値加算月
        CO0007INProw("VALUEADDDD") = "0"                            '値加算日
        CO0007INProw("DELFLG") = WF_DELFLG.Text                     '削除フラグ
        CO0007INProw("DELFLGNAMES") = ""                            '削除フラグ名称

        '○ 名称取得
        CODENAME_get("CAMPCODE", CO0007INProw("CAMPCODE"), CO0007INProw("CAMPNAMES"), WW_DUMMY)                 '会社コード
        CODENAME_get("MAPID", CO0007INProw("MAPID"), CO0007INProw("MAPNAMES"), WW_DUMMY)                        '画面ID
        CODENAME_get("VALUETYPE", CO0007INProw("VALUETYPE"), CO0007INProw("VALUETYPENAMES"), WW_DUMMY)          '値タイプ
        CODENAME_get("DELFLG", CO0007INProw("DELFLG"), CO0007INProw("DELFLGNAMES"), WW_DUMMY)                   '削除

        '○ チェック用テーブルに登録する
        CO0007INPtbl.Rows.Add(CO0007INProw)

        '○ 明細分作成
        For Each reitem As RepeaterItem In WF_Repeater.Items
            CO0007INProw = CO0007INPtbl.NewRow

            Master.eraseCharToIgnore(CType(reitem.FindControl("WF_Rep_VALUETYPE"), TextBox).Text)           '値タイプ
            Master.eraseCharToIgnore(CType(reitem.FindControl("WF_Rep_VALUE"), TextBox).Text)               '値
            Master.eraseCharToIgnore(CType(reitem.FindControl("WF_Rep_VALUEADDYY"), TextBox).Text)          '値加算年
            Master.eraseCharToIgnore(CType(reitem.FindControl("WF_Rep_VALUEADDMM"), TextBox).Text)          '値加算月
            Master.eraseCharToIgnore(CType(reitem.FindControl("WF_Rep_VALUEADDDD"), TextBox).Text)          '値加算日

            '○ 初期クリア
            For Each CO0007INPcol As DataColumn In CO0007INPtbl.Columns
                If IsDBNull(CO0007INProw.Item(CO0007INPcol)) OrElse IsNothing(CO0007INProw.Item(CO0007INPcol)) Then
                    Select Case CO0007INPcol.ColumnName
                        Case "LINECNT"
                            CO0007INProw.Item(CO0007INPcol) = 0
                        Case "OPERATION"
                            CO0007INProw.Item(CO0007INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            CO0007INProw.Item(CO0007INPcol) = 0
                        Case "SELECT"
                            CO0007INProw.Item(CO0007INPcol) = 1
                        Case "HIDDEN"
                            CO0007INProw.Item(CO0007INPcol) = 0
                        Case "SEQ"
                            CO0007INProw.Item(CO0007INPcol) = 0
                        Case Else
                            CO0007INProw.Item(CO0007INPcol) = ""
                    End Select
                End If
            Next

            'LINECNT
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, CO0007INProw("LINECNT"))
            Catch ex As Exception
                CO0007INProw("LINECNT") = 0
            End Try

            CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            CO0007INProw("TIMSTP") = 0
            CO0007INProw("SELECT") = 1
            CO0007INProw("HIDDEN") = 0

            CO0007INProw("CAMPCODE") = WF_CAMPCODE.Text                                                     '会社コード
            CO0007INProw("CAMPNAMES") = ""                                                                  '会社名称
            CO0007INProw("PROFID") = WF_PROFID.Text                                                         'プロファイルID
            CO0007INProw("MAPID") = WF_MAPID.Text                                                           '画面ID
            CO0007INProw("MAPNAMES") = ""                                                                   '画面名称
            CO0007INProw("VARIANT") = WF_VARIANT.Text                                                       '変数
            CO0007INProw("TITLEKBN") = "I"                                                                  'タイトル区分
            CO0007INProw("SEQ") = CType(reitem.FindControl("WF_Rep_SEQ"), Label).Text                       '表示順番
            CO0007INProw("FIELD") = CType(reitem.FindControl("WF_Rep_FIELD"), Label).Text                   '項目
            CO0007INProw("STYMD") = WF_STYMD.Text                                                           '開始年月日
            CO0007INProw("ENDYMD") = WF_ENDYMD.Text                                                         '終了年月日
            CO0007INProw("VARIANTNAMES") = WF_VARIANTNAMES.Text                                             '変数名称
            CO0007INProw("TITLENAMES") = CType(reitem.FindControl("WF_Rep_TITLENAMES"), Label).Text         'タイトル名称
            CO0007INProw("VALUETYPE") = CType(reitem.FindControl("WF_Rep_VALUETYPE"), TextBox).Text         '値タイプ
            CO0007INProw("VALUETYPENAMES") = ""                                                             '値タイプ名称
            CO0007INProw("VALUE") = CType(reitem.FindControl("WF_Rep_VALUE"), TextBox).Text                 '値
            CO0007INProw("VALUEADDYY") = CType(reitem.FindControl("WF_Rep_VALUEADDYY"), TextBox).Text       '値加算年
            CO0007INProw("VALUEADDMM") = CType(reitem.FindControl("WF_Rep_VALUEADDMM"), TextBox).Text       '値加算月
            CO0007INProw("VALUEADDDD") = CType(reitem.FindControl("WF_Rep_VALUEADDDD"), TextBox).Text       '値加算日
            CO0007INProw("DELFLG") = WF_DELFLG.Text                                                         '削除フラグ
            CO0007INProw("DELFLGNAMES") = ""                                                                '削除フラグ名称

            '○ 名称取得
            CODENAME_get("CAMPCODE", CO0007INProw("CAMPCODE"), CO0007INProw("CAMPNAMES"), WW_DUMMY)               '会社コード
            CODENAME_get("MAPID", CO0007INProw("MAPID"), CO0007INProw("MAPNAMES"), WW_DUMMY)                      '画面ID
            CODENAME_get("VALUETYPE", CO0007INProw("VALUETYPE"), CO0007INProw("VALUETYPENAMES"), WW_DUMMY)        '値タイプ
            CODENAME_get("DELFLG", CO0007INProw("DELFLG"), CO0007INProw("DELFLGNAMES"), WW_DUMMY)                 '削除

            '○ チェック用テーブルに登録する
            CO0007INPtbl.Rows.Add(CO0007INProw)
        Next

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
        For Each CO0007row As DataRow In CO0007tbl.Rows
            Select Case CO0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(CO0007tbl)

        WF_Sel_LINECNT.Text = ""                            'LINECNT
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_PROFID.Text = ""                                 'プロファイルID
        WF_MAPID.Text = ""                                  '画面ID
        WF_MAPID_TEXT.Text = ""                             '画面名称
        WF_VARIANT.Text = ""                                '変数
        WF_VARIANTNAMES.Text = ""                           '変数名称
        WF_STYMD.Text = ""                                  '開始年月日
        WF_ENDYMD.Text = ""                                 '終了年月日
        WF_DELFLG.Text = ""                                 '削除フラグ
        WF_DELFLG_TEXT.Text = ""                            '削除フラグ名称

        '○ 詳細画面初期設定
        DetailInitialize()

    End Sub

    ''' <summary>
    ''' 詳細画面-初期設定 (空明細作成 ＆ イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailInitialize()

        Master.CreateEmptyTable(CO0007INPtbl)

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = False
        WF_Repeater.DataSource = CO0007INPtbl
        WF_Repeater.DataBind()

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
                            Case "WF_SELMAPID"              '画面ID
                                prmData = work.CreateMAPIDParam(work.WF_SEL_CAMPCODE.Text)
                            Case "WF_Rep_VALUETYPE"         '値タイプ
                                prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "VALUETYPE"
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
        Select Case WF_FIELD.Value
            Case "WF_SELMAPID"              '画面ID
                WF_SELMAPID.Text = WW_SelectValue
                WF_SELMAPID_TEXT.Text = WW_SelectText
                WF_SELMAPID.Focus()

            Case "WF_STYMD"                 '有効年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    WF_STYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"                '有効年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    WF_ENDYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                WF_ENDYMD.Focus()

            Case "WF_DELFLG"                '削除
                WF_DELFLG.Text = WW_SelectValue
                WF_DELFLG_TEXT.Text = WW_SelectText
                WF_DELFLG.Focus()

            Case "WF_Rep_VALUETYPE"         '値タイプ
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_VALUETYPE"), TextBox).Text = WW_SelectValue
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_VALUETYPE"), TextBox).Focus()
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
            Case "WF_SELMAPID"              '画面ID
                WF_SELMAPID.Focus()
            Case "WF_STYMD"                 '有効年月日(From)
                WF_STYMD.Focus()
            Case "WF_ENDYMD"                '有効年月日(To)
                WF_ENDYMD.Focus()
            Case "WF_DELFLG"                '削除
                WF_DELFLG.Focus()
            Case "WF_Rep_VALUETYPE"         '値タイプ
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_VALUETYPE"), TextBox).Focus()
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
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_CNT1 As Integer = 0
        Dim WW_CNT2 As Integer = 0
        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim VARItbl As New DataTable
        Dim CHKtbl As New DataTable

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

        '○ 事前準備(重複レコード削除)
        Do Until WW_CNT1 > CO0007INPtbl.Rows.Count - 1
            WW_CNT2 = WW_CNT1 + 1

            Do Until WW_CNT2 > CO0007INPtbl.Rows.Count - 1
                'KEY重複は削除
                If CO0007INPtbl.Rows(WW_CNT1)("CAMPCODE") = CO0007INPtbl.Rows(WW_CNT2)("CAMPCODE") AndAlso
                    CO0007INPtbl.Rows(WW_CNT1)("PROFID") = CO0007INPtbl.Rows(WW_CNT2)("PROFID") AndAlso
                    CO0007INPtbl.Rows(WW_CNT1)("MAPID") = CO0007INPtbl.Rows(WW_CNT2)("MAPID") AndAlso
                    CO0007INPtbl.Rows(WW_CNT1)("VARIANT") = CO0007INPtbl.Rows(WW_CNT2)("VARIANT") AndAlso
                    CO0007INPtbl.Rows(WW_CNT1)("TITLEKBN") = CO0007INPtbl.Rows(WW_CNT2)("TITLEKBN") AndAlso
                    CO0007INPtbl.Rows(WW_CNT1)("FIELD") = CO0007INPtbl.Rows(WW_CNT2)("FIELD") AndAlso
                    CO0007INPtbl.Rows(WW_CNT1)("STYMD") = CO0007INPtbl.Rows(WW_CNT2)("STYMD") AndAlso
                    CO0007INPtbl.Rows(WW_CNT1)("ENDYMD") = CO0007INPtbl.Rows(WW_CNT2)("ENDYMD") Then
                    CO0007INPtbl.Rows(WW_CNT2).Delete()
                Else
                    WW_CNT2 = WW_CNT2 + 1
                End If
            Loop
            WW_CNT1 = WW_CNT1 + 1
        Loop

        '○ プロフIDと変数がDefaultのデータを取得する
        CS0026TBLSORT.TABLE = CO0007tbl
        CS0026TBLSORT.SORTING = "CAMPCODE, MAPID, TITLEKBN, SEQ, FIELD"
        CS0026TBLSORT.FILTER = "PROFID = '" & C_DEFAULT_DATAKEY & "'" _
            & " and VARIANT = '" & C_DEFAULT_DATAKEY & "'"
        CS0026TBLSORT.sort(VARItbl)

        '○ ヘッダー単項目チェック
        For Each CO0007INProw As DataRow In CO0007INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0007INProw("TITLEKBN") <> "H" Then
                Continue For
            End If

            CS0026TBLSORT.TABLE = VARItbl
            CS0026TBLSORT.SORTING = "TITLEKBN, SEQ, FIELD"
            CS0026TBLSORT.FILTER = "CAMPCODE = '" & CO0007INProw("CAMPCODE") & "'" _
                & " and MAPID = '" & CO0007INProw("MAPID") & "'"
            CS0026TBLSORT.sort(CHKtbl)

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", CO0007INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", CO0007INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'プロフID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "PROFID", CO0007INProw("PROFID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If CO0007INProw("PROFID") <> C_DEFAULT_DATAKEY AndAlso
                    CO0007INProw("PROFID") <> Master.PROF_VIEW Then
                    WW_CheckMES1 = "・更新できないレコード(プロフIDエラー)です。"
                    WW_CheckMES2 = "ログインユーザーのプロフIDと異なります。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(プロフIDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ユーザープロフID、Defaultはエラー
            If Master.PROF_VIEW = C_DEFAULT_DATAKEY Then
                WW_CheckMES1 = "・更新できないレコード(プロフID='" & C_DEFAULT_DATAKEY & "')です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面ID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MAPID", CO0007INProw("MAPID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("MAPID", CO0007INProw("MAPID"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) OrElse CHKtbl.Rows.Count = 0 Then
                    WW_CheckMES1 = "・更新できないレコード(画面IDエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(画面IDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '変数
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VARIANT", CO0007INProw("VARIANT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(変数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '変数名称
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VARIANTNAMES", CO0007INProw("VARIANTNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(変数名称エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", CO0007INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", CO0007INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '日付大小チェック
            If CO0007INProw("STYMD") > CO0007INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(有効開始日＞有効終了日)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '範囲チェック
            If work.WF_SEL_STYMD.Text > CO0007INProw("STYMD") AndAlso
                work.WF_SEL_STYMD.Text > CO0007INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            If work.WF_SEL_ENDYMD.Text < CO0007INProw("STYMD") AndAlso
                work.WF_SEL_ENDYMD.Text < CO0007INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除フラグ
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", CO0007INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", CO0007INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        '○ 明細単項目チェック
        For Each CO0007INProw As DataRow In CO0007INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0007INProw("TITLEKBN") = "H" Then
                Continue For
            End If

            CS0026TBLSORT.TABLE = VARItbl
            CS0026TBLSORT.SORTING = ""
            CS0026TBLSORT.FILTER = "CAMPCODE = '" & CO0007INProw("CAMPCODE") & "'" _
                & " and MAPID = '" & CO0007INProw("MAPID") & "'" _
                & " and FIELD = '" & CO0007INProw("FIELD") & "'"
            CS0026TBLSORT.sort(CHKtbl)

            '項目
            If CHKtbl.Rows.Count = 0 Then
                WW_CheckMES1 = "・更新できないレコード(項目エラー)です。"
                WW_CheckMES2 = C_DEFAULT_DATAKEY & "データに存在しません。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '値タイプ
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VALUETYPE", CO0007INProw("VALUETYPE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("VALUETYPE", CO0007INProw("VALUETYPE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(値タイプエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(値タイプエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '値
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VALUE", CO0007INProw("VALUE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Select Case CO0007INProw("VALUETYPE")
                    Case "DATEFIX"          '固定日付
                        '必須チェック
                        If CO0007INProw("VALUE") = "" Then
                            WW_CheckMES1 = "・更新できないレコード(DATEFIXの値未入力エラー)です。"
                            WW_CheckMES2 = WW_CS0024FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If

                        '日付チェック
                        Try
                            Dim WW_DATE As Date
                            If Date.TryParse(CO0007INProw("VALUE"), WW_DATE) Then
                                If WW_DATE < C_DEFAULT_YMD Then
                                    WW_CheckMES1 = "・更新できないレコード(DATEFIXの値が日付形式ではないエラー)です。"
                                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                                    WW_LINE_ERR = "ERR"
                                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                                End If
                            Else
                                WW_CheckMES1 = "・更新できないレコード(DATEFIXの値が日付形式ではないエラー)です。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                                WW_LINE_ERR = "ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Catch ex As Exception
                            WW_CheckMES1 = "・更新できないレコード(DATEFIXの値が日付形式ではないエラー)です。"
                            WW_CheckMES2 = WW_CS0024FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End Try

                    Case "DATENOW"          '現在日付
                        '入力チェック
                        If CO0007INProw("VALUE") <> "" Then
                            WW_CheckMES1 = "・更新できないレコード(DATENOWは値不要)です。"
                            WW_CheckMES2 = WW_CS0024FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If

                    Case "DATES"            '現在日付(月初)
                        '入力チェック
                        If CO0007INProw("VALUE") <> "" Then
                            WW_CheckMES1 = "・更新できないレコード(DATESは値不要)です。"
                            WW_CheckMES2 = WW_CS0024FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If

                    Case "FIX"              '固定値
                End Select
            Else
                WW_CheckMES1 = "・更新できないレコード(値エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '値加算年
            WW_TEXT = CO0007INProw("VALUEADDYY")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VALUEADDYY", CO0007INProw("VALUEADDYY"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0007INProw("VALUEADDYY") = 0
                Else
                    Try
                        CO0007INProw("VALUEADDYY") = Format(CInt(CO0007INProw("VALUEADDYY")), "#0")
                    Catch ex As Exception
                        CO0007INProw("VALUEADDYY") = 0
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(値加算年エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '値加算月
            WW_TEXT = CO0007INProw("VALUEADDMM")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VALUEADDMM", CO0007INProw("VALUEADDMM"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0007INProw("VALUEADDMM") = 0
                Else
                    Try
                        CO0007INProw("VALUEADDMM") = Format(CInt(CO0007INProw("VALUEADDMM")), "#0")
                    Catch ex As Exception
                        CO0007INProw("VALUEADDMM") = 0
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(値加算月エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '値加算日
            WW_TEXT = CO0007INProw("VALUEADDDD")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VALUEADDDD", CO0007INProw("VALUEADDDD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0007INProw("VALUEADDDD") = 0
                Else
                    Try
                        CO0007INProw("VALUEADDDD") = Format(CInt(CO0007INProw("VALUEADDDD")), "#0")
                    Catch ex As Exception
                        CO0007INProw("VALUEADDDD") = 0
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(値加算日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        '○ Defaultデータ、タイトル区分存在チェック(Hレコードが無ければエラー)
        For Each CO0007INProw As DataRow In CO0007INPtbl.Rows
            If CO0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            Dim CNTtbl As New DataTable
            CS0026TBLSORT.TABLE = VARItbl
            CS0026TBLSORT.SORTING = ""
            CS0026TBLSORT.FILTER = "CAMPCODE = '" & CO0007INProw("CAMPCODE") & "'" _
                & " and MAPID = '" & CO0007INProw("MAPID") & "'" _
                & " and TITLEKBN = 'H'"
            CS0026TBLSORT.sort(CNTtbl)

            If CNTtbl.Rows.Count = 0 Then
                WW_CheckMES1 = "・更新できないレコード(" & C_DEFAULT_DATAKEY & " タイトル区分(H)なし)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0007INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        VARItbl.Clear()
        VARItbl.Dispose()
        VARItbl = Nothing

        CHKtbl.Clear()
        CHKtbl.Dispose()
        CHKtbl = Nothing

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="CO0007row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal CO0007row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(CO0007row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード   =" & CO0007row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> プロフＩＤ   =" & CO0007row("PROFID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 画面ＩＤ     =" & CO0007row("MAPID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 変数         =" & CO0007row("VARIANT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 変数名称     =" & CO0007row("VARIANTNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日   =" & CO0007row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日   =" & CO0007row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タイトル区分 =" & CO0007row("TITLEKBN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項目         =" & CO0007row("FIELD")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' CO0007tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CO0007tbl_UPD()

        Dim CO0007FILtbl As New DataTable

        '○ 画面状態設定
        For Each CO0007row As DataRow In CO0007tbl.Rows
            Select Case CO0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ Defaultに存在し、更新データに存在しない項目を補正する
        CS0026TBLSORT.TABLE = CO0007tbl
        CS0026TBLSORT.SORTING = ""
        CS0026TBLSORT.FILTER = "CAMPCODE = '" & CO0007INPtbl.Rows(0)("CAMPCODE") & "'" _
            & " and PROFID = '" & C_DEFAULT_DATAKEY & "'" _
            & " and MAPID = '" & CO0007INPtbl.Rows(0)("MAPID") & "'" _
            & " and VARIANT = '" & C_DEFAULT_DATAKEY & "'"
        CS0026TBLSORT.sort(CO0007FILtbl)

        For Each CO0007FILrow As DataRow In CO0007FILtbl.Rows
            If CO0007FILrow("TITLEKBN") = "H" Then
                Continue For
            End If

            Dim WW_EXIST As Boolean = False

            '項目が存在するか探す
            For Each CO0007INProw As DataRow In CO0007INPtbl.Rows
                If CO0007INProw("TITLEKBN") = "H" Then
                    Continue For
                End If

                If CO0007FILrow("FIELD") = CO0007INProw("FIELD") Then
                    WW_EXIST = True
                    Exit For
                End If
            Next

            '存在しない場合、Defaultの情報を更新用テーブルに追加
            If Not WW_EXIST Then
                Dim CO0007INProw As DataRow = CO0007INPtbl.NewRow
                CO0007INProw.ItemArray = CO0007FILrow.ItemArray

                CO0007INProw("LINECNT") = 0
                CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                CO0007INProw("TIMSTP") = 0
                CO0007INProw("SELECT") = 1
                CO0007INProw("HIDDEN") = 0

                CO0007INProw("PROFID") = Master.PROF_VIEW
                CO0007INProw("VARIANT") = CO0007INPtbl.Rows(0)("VARIANT")
                CO0007INProw("STYMD") = CO0007INPtbl.Rows(0)("STYMD")
                CO0007INProw("ENDYMD") = CO0007INPtbl.Rows(0)("ENDYMD")
                CO0007INProw("DELFLG") = CO0007INPtbl.Rows(0)("DELFLG")

                CO0007INPtbl.Rows.Add(CO0007INProw)
            End If
        Next

        '件数が膨大なため、会社と画面IDで絞る
        CS0026TBLSORT.TABLE = CO0007tbl
        CS0026TBLSORT.SORTING = ""
        CS0026TBLSORT.FILTER = "CAMPCODE = '" & CO0007INPtbl.Rows(0)("CAMPCODE") & "'" _
            & " and MAPID = '" & CO0007INPtbl.Rows(0)("MAPID") & "'"
        CS0026TBLSORT.sort(CO0007FILtbl)

        '○ 追加変更判定
        Dim WW_UPDAT As Boolean = False
        For Each CO0007INProw As DataRow In CO0007INPtbl.Rows

            'エラーレコード読み飛ばし
            If CO0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            CO0007INProw("OPERATION") = "Insert"

            'KEY項目が等しい(ENDYMD以外のKEYが同じ)
            For Each CO0007row As DataRow In CO0007FILtbl.Rows
                If CO0007row("CAMPCODE") = CO0007INProw("CAMPCODE") AndAlso
                    CO0007row("PROFID") = Master.PROF_VIEW AndAlso
                    CO0007row("MAPID") = CO0007INProw("MAPID") AndAlso
                    CO0007row("VARIANT") = CO0007INProw("VARIANT") AndAlso
                    CO0007row("TITLEKBN") = CO0007INProw("TITLEKBN") AndAlso
                    CO0007row("FIELD") = CO0007INProw("FIELD") AndAlso
                    CO0007row("STYMD") = CO0007INProw("STYMD") Then

                    '変更無は操作無
                    If CO0007row("SEQ") = CO0007INProw("SEQ") AndAlso
                        CO0007row("ENDYMD") = CO0007INProw("ENDYMD") AndAlso
                        CO0007row("VARIANTNAMES") = CO0007INProw("VARIANTNAMES") AndAlso
                        CO0007row("TITLENAMES") = CO0007INProw("TITLENAMES") AndAlso
                        CO0007row("VALUETYPE") = CO0007INProw("VALUETYPE") AndAlso
                        CO0007row("VALUE") = CO0007INProw("VALUE") AndAlso
                        CO0007row("VALUEADDYY") = CO0007INProw("VALUEADDYY") AndAlso
                        CO0007row("VALUEADDMM") = CO0007INProw("VALUEADDMM") AndAlso
                        CO0007row("VALUEADDDD") = CO0007INProw("VALUEADDDD") AndAlso
                        CO0007row("DELFLG") = CO0007INProw("DELFLG") Then
                        CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    CO0007INProw("OPERATION") = "Update"
                    WW_UPDAT = True
                    Exit For
                End If
            Next
        Next

        CO0007FILtbl.Clear()
        CO0007FILtbl.Dispose()
        CO0007FILtbl = Nothing

        '○ 更新レコードが存在する場合、ヘッダー区分も更新対象にする
        If WW_UPDAT Then
            For Each CO0007INProw As DataRow In CO0007INPtbl.Rows
                If CO0007INProw("TITLEKBN") = "H" Then
                    CO0007INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        End If

        '○ 変更有無判定　&　入力値反映
        For Each CO0007INProw As DataRow In CO0007INPtbl.Rows
            Select Case CO0007INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(CO0007INProw)
                Case "Insert"
                    TBL_INSERT_SUB(CO0007INProw)
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="CO0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef CO0007INProw As DataRow)

        For Each CO0007row As DataRow In CO0007tbl.Rows

            '同一(ENDYMD以外が同一KEY)レコード
            If CO0007row("CAMPCODE") = CO0007INProw("CAMPCODE") AndAlso
                CO0007row("PROFID") = Master.PROF_VIEW AndAlso
                CO0007row("MAPID") = CO0007INProw("MAPID") AndAlso
                CO0007row("VARIANT") = CO0007INProw("VARIANT") AndAlso
                CO0007row("TITLEKBN") = CO0007INProw("TITLEKBN") AndAlso
                CO0007row("FIELD") = CO0007INProw("FIELD") AndAlso
                CO0007row("STYMD") = CO0007INProw("STYMD") Then

                '画面入力テーブル項目設定
                CO0007INProw("LINECNT") = CO0007row("LINECNT")
                CO0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                CO0007INProw("TIMSTP") = CO0007row("TIMSTP")
                CO0007INProw("SELECT") = 1
                CO0007INProw("HIDDEN") = 0

                CO0007INProw("PROFID") = Master.PROF_VIEW

                '項目テーブル項目設定
                CO0007row.ItemArray = CO0007INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="CO0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef CO0007INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim CO0007row As DataRow = CO0007tbl.NewRow
        CO0007row.ItemArray = CO0007INProw.ItemArray

        '○ 最大項番数を取得
        Dim TBLview As DataView = New DataView(CO0007tbl)
        TBLview.RowFilter = "TITLEKBN = 'H'"

        If CO0007INProw("TITLEKBN") = "H" Then
            CO0007row("LINECNT") = TBLview.Count + 1
        Else
            CO0007row("LINECNT") = 0
        End If

        CO0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        CO0007row("TIMSTP") = "0"
        CO0007row("SELECT") = 1
        CO0007row("HIDDEN") = 0

        CO0007row("PROFID") = Master.PROF_VIEW

        CO0007tbl.Rows.Add(CO0007row)

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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MAPID"            '画面ID
                    prmData = work.CreateMAPIDParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "VALUETYPE"        '値タイプ
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "VALUETYPE"
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

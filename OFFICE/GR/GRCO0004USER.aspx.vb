Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' ユーザIDマスタ入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRCO0004USER
    Inherits Page

    '○ 検索結果格納Table
    Private CO0004tbl As DataTable                              '一覧格納用テーブル
    Private CO0004INPtbl As DataTable                           'チェック用テーブル
    Private CO0004UPDtbl As DataTable                           '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45            '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10             'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '明細部ID
    Private Const CONST_APPROVAL_CNT As Integer = 2             '承認権限数

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
                    If Not Master.RecoverTable(CO0004tbl) Then
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
            If Not IsNothing(CO0004tbl) Then
                CO0004tbl.Clear()
                CO0004tbl.Dispose()
                CO0004tbl = Nothing
            End If

            If Not IsNothing(CO0004INPtbl) Then
                CO0004INPtbl.Clear()
                CO0004INPtbl.Dispose()
                CO0004INPtbl = Nothing
            End If

            If Not IsNothing(CO0004UPDtbl) Then
                CO0004UPDtbl.Clear()
                CO0004UPDtbl.Dispose()
                CO0004UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRCO0004WRKINC.MAPID

        WF_USERID.Focus()
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.CO0004S Then
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
        Master.SaveTable(CO0004tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(CO0004tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

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

        If IsNothing(CO0004tbl) Then
            CO0004tbl = New DataTable
        End If

        If CO0004tbl.Columns.Count <> 0 Then
            CO0004tbl.Columns.Clear()
        End If

        CO0004tbl.Clear()

        '○ 検索SQL(メンテナンスは総務のみなので、権限は利用しない)
        Dim SQLStr As String =
              " SELECT" _
            & "    0                                                   AS LINECNT" _
            & "    , ''                                                AS OPERATION" _
            & "    , CAST(S004.UPDTIMSTP AS BIGINT)                    AS TIMSTP" _
            & "    , 1                                                 AS 'SELECT'" _
            & "    , 0                                                 AS HIDDEN" _
            & "    , ISNULL(RTRIM(S004.USERID), '')                    AS USERID" _
            & "    , ISNULL(FORMAT(S004.STYMD, 'yyyy/MM/dd'), '')      AS STYMD" _
            & "    , ISNULL(FORMAT(S004.ENDYMD, 'yyyy/MM/dd'), '')     AS ENDYMD" _
            & "    , ISNULL(RTRIM(S004.CAMPCODE), '')                  AS CAMPCODE" _
            & "    , ''                                                AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(S004.ORG), '')                       AS ORG" _
            & "    , ''                                                AS ORGNAMES" _
            & "    , ISNULL(RTRIM(S004.STAFFCODE), '')                 AS STAFFCODE" _
            & "    , ISNULL(RTRIM(S004.STAFFNAMES), '')                AS STAFFNAMES" _
            & "    , ISNULL(RTRIM(S004.STAFFNAMEL), '')                AS STAFFNAMEL" _
            & "    , ISNULL(RTRIM(S004.CAMPROLE), '')                  AS CAMPROLE" _
            & "    , ISNULL(RTRIM(S004.MAPROLE), '')                   AS MAPROLE" _
            & "    , ISNULL(RTRIM(S004.ORGROLE), '')                   AS ORGROLE" _
            & "    , ISNULL(RTRIM(S004.VIEWPROFID), '')                AS VIEWPROFID" _
            & "    , ISNULL(RTRIM(S004.RPRTPROFID), '')                AS RPRTPROFID" _
            & "    , ISNULL(RTRIM(S004.MAPID), '')                     AS MAPID" _
            & "    , ISNULL(RTRIM(S004.VARIANT), '')                   AS VARIANT" _
            & "    , ISNULL(RTRIM(S014.PASSWORD), '')                  AS PASSWORD" _
            & "    , ISNULL(RTRIM(S014.MISSCNT), '')                   AS MISSCNT" _
            & "    , ISNULL(FORMAT(S014.PASSENDYMD, 'yyyy/MM/dd'), '') AS PASSENDYMD" _
            & "    , ISNULL(RTRIM(S051.ROLE), '')                      AS ROLEAPPROVAL1" _
            & "    , ''                                                AS ROLEAPPROVAL1TYPE" _
            & "    , ISNULL(RTRIM(S052.ROLE), '')                      AS ROLEAPPROVAL2" _
            & "    , ''                                                AS ROLEAPPROVAL2TYPE" _
            & "    , ISNULL(RTRIM(S004.DELFLG), '')                    AS DELFLG" _
            & " FROM" _
            & "    S0004_USER S004" _
            & "    INNER JOIN S0014_USERPASS S014" _
            & "        ON  S014.USERID   = S004.USERID" _
            & "        AND S014.DELFLG  <> @P6" _
            & "    LEFT JOIN S0005_AUTHOR S051" _
            & "        ON  S051.USERID   = S004.USERID" _
            & "        AND S051.CAMPCODE = S004.CAMPCODE" _
            & "        AND S051.OBJECT   = @P2" _
            & "        AND S051.STYMD    = S004.STYMD" _
            & "        AND S051.DELFLG  <> @P6" _
            & "    LEFT JOIN S0005_AUTHOR S052" _
            & "        ON  S052.USERID   = S004.USERID" _
            & "        AND S052.CAMPCODE = S004.CAMPCODE" _
            & "        AND S052.OBJECT   = @P3" _
            & "        AND S052.STYMD    = S004.STYMD" _
            & "        AND S052.DELFLG  <> @P6" _
            & " WHERE" _
            & "    S004.CAMPCODE    = @P1" _
            & "    AND S004.STYMD  <= @P4" _
            & "    AND S004.ENDYMD >= @P5" _
            & "    AND S004.DELFLG <> @P6"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '所属部署
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
            SQLStr &= String.Format("    AND S004.ORG     = '{0}'", work.WF_SEL_ORG.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    S004.ORG" _
            & "    , S004.USERID"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '第一承認
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        '最終承認
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '有効年月日(To)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '有効年月日(From)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = "APPROVAL1"
                PARA3.Value = "APPROVAL2"
                PARA4.Value = work.WF_SEL_ENDYMD.Text
                PARA5.Value = work.WF_SEL_STYMD.Text
                PARA6.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        CO0004tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    CO0004tbl.Load(SQLdr)
                End Using

                For Each CO0004row As DataRow In CO0004tbl.Rows
                    '名称取得
                    CODENAME_get("CAMPCODE", CO0004row("CAMPCODE"), CO0004row("CAMPNAMES"), WW_DUMMY)                               '会社コード
                    CODENAME_get("ORG", CO0004row("ORG"), CO0004row("ORGNAMES"), WW_DUMMY)                                          '所属部署
                    CODENAME_get("ROLEAPPROVAL1", CO0004row("ROLEAPPROVAL1"), CO0004row("ROLEAPPROVAL1TYPE"), WW_DUMMY, "2")        '権限(第一承認)
                    CODENAME_get("ROLEAPPROVAL2", CO0004row("ROLEAPPROVAL2"), CO0004row("ROLEAPPROVAL2TYPE"), WW_DUMMY, "2")        '権限(最終承認)
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0004_USER SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0004_USER Select"
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
        CS0026TBLSORT.TABLE = CO0004tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            CO0004tbl = CS0026TBLSORT.TABLE
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
        For Each CO0004row As DataRow In CO0004tbl.Rows
            If CO0004row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                CO0004row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(CO0004tbl)

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
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.EraseCharToIgnore(WF_SELUSERID.Text)
        Master.EraseCharToIgnore(WF_SELSTAFFCODE.Text)

        '○ 名称取得
        CODENAME_get("STAFFCODE", WF_SELSTAFFCODE.Text, WF_SELSTAFFCODE_TEXT.Text, WW_DUMMY)

        '○ 絞り込み操作(GridView明細Hidden設定)
        For Each CO0004row As DataRow In CO0004tbl.Rows

            '一度非表示にする
            CO0004row("HIDDEN") = 1

            Dim WW_HANTEI As Boolean = True

            'ユーザーIDによる絞込判定
            If WF_SELUSERID.Text <> "" AndAlso
                WF_SELUSERID.Text <> CO0004row("USERID") Then
                WW_HANTEI = False
            End If

            '従業員による絞込判定
            If WF_SELSTAFFCODE.Text <> "" AndAlso
                WF_SELSTAFFCODE.Text <> CO0004row("STAFFCODE") Then
                WW_HANTEI = False
            End If

            '画面(GridView)のHIDDENに結果格納
            If WW_HANTEI Then
                CO0004row("HIDDEN") = 0
            End If
        Next

        '○ 画面先頭を表示
        WF_GridPosition.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(CO0004tbl)

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_USERID.Focus()

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

                'ユーザマスタ更新
                UpdateUserMaster(SQLcon)
            End Using
        End If

        '○ 削除データ除外
        CS0026TBLSORT.TABLE = CO0004tbl
        CS0026TBLSORT.SORTING = ""
        CS0026TBLSORT.FILTER = "DELFLG <> '" & C_DELETE_FLG.DELETE & "'"
        CS0026TBLSORT.sort(CO0004tbl)

        '○ 画面表示データ再ソート
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TABLE = CO0004tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            CO0004tbl = CS0026TBLSORT.TABLE
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0004tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        WF_USERID.Focus()

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
        For Each CO0004row As DataRow In CO0004tbl.Rows

            '読み飛ばし
            If (CO0004row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                CO0004row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                CO0004row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                CO0004row("STYMD") = "" Then
                Continue For
            End If

            WW_LINE_ERR = ""

            'チェック
            For Each CO0004chk As DataRow In CO0004tbl.Rows

                '同一KEY以外は読み飛ばし
                If CO0004row("CAMPCODE") <> CO0004chk("CAMPCODE") OrElse
                    CO0004row("USERID") <> CO0004chk("USERID") OrElse
                    CO0004chk("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If CO0004row("STYMD") = CO0004chk("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(CO0004row("STYMD"), WW_DATE_ST)
                    Date.TryParse(CO0004row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(CO0004chk("STYMD"), WW_DATE_ST2)
                    Date.TryParse(CO0004chk("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End Try

                '開始日チェック
                If WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", CO0004row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If

                '終了日チェック
                If WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", CO0004row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If

                '日付連続性チェック
                If WW_DATE_END.AddDays(1) <> WW_DATE_ST2 Then
                    WW_CheckMES = "・エラー(開始、終了年月日が連続していません)。"
                    WW_CheckERR(WW_CheckMES, "", CO0004row)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If
            Next

            If WW_LINE_ERR = "" Then
                CO0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' ユーザマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateUserMaster(ByVal SQLcon As SqlConnection)

        Dim SQLcmd As New SqlCommand()
        Dim SQLtrn As SqlTransaction = Nothing

        Try
            For Each CO0004row As DataRow In CO0004tbl.Rows
                If Trim(CO0004row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                    Trim(CO0004row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then

                    '削除は更新しない
                    If CO0004row("DELFLG") = C_DELETE_FLG.DELETE AndAlso CO0004row("TIMSTP") = 0 Then
                        CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        For Each CO0004ope As DataRow In CO0004tbl.Rows
                            If CO0004row("CAMPCODE") = CO0004ope("CAMPCODE") AndAlso
                                CO0004row("USERID") = CO0004ope("USERID") AndAlso
                                CO0004row("STYMD") = CO0004ope("STYMD") Then
                                CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            End If
                        Next

                        Continue For
                    End If

                    'DB更新
                    Dim WW_DATENOW As DateTime = Date.Now
                    SQLtrn = SQLcon.BeginTransaction()

                    'ユーザーマスタ更新
                    ExecuteUser(CO0004row, WW_DATENOW, SQLcon, SQLcmd, SQLtrn)

                    'ユーザーパスワードマスタ更新SQL
                    '  有効期間を持たないため複数期間のうち、ある期間が削除されるとパスワードテーブルが削除され
                    '  一覧表示できないため削除はしない
                    If CO0004row("DELFLG") <> C_DELETE_FLG.DELETE Then
                        ExecuteUserPass(CO0004row, WW_DATENOW, SQLcon, SQLcmd, SQLtrn)
                    End If

                    '承認権限
                    For i As Integer = 1 To CONST_APPROVAL_CNT
                        Dim WW_OBJECT As String = "APPROVAL" & i
                        Dim WW_FIELD As String = "ROLEAPPROVAL" & i
                        Dim WW_TYPE As String = "ROLEAPPROVAL" & i & "TYPE"
                        Dim WW_ORG As DataTable = New DataTable

                        '一度削除する
                        DeleteApproval(CO0004row, WW_OBJECT, i, WW_DATENOW, SQLcon, SQLcmd, SQLtrn)

                        '権限承認が入力されている時
                        If CO0004row(WW_FIELD) <> "" Then
                            '残業承認ロールを取得する
                            GetApprovalROLE(WW_OBJECT, CO0004row(WW_FIELD), WW_ORG)

                            '権限マスタ更新
                            ExecuteAuthor(CO0004row, WW_OBJECT, WW_FIELD, WW_DATENOW, SQLcon, SQLcmd, SQLtrn)
                            '承認設定マスタ更新
                            ExecuteApproval(CO0004row, WW_FIELD, WW_TYPE, WW_ORG, i, WW_DATENOW, SQLcon, SQLcmd, SQLtrn)
                        End If
                    Next

                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    SQLtrn.Commit()

                    '更新ログ発行
                    ExecuteJournal(CO0004row, SQLcon, SQLcmd)
                End If
            Next
        Catch ex As Exception
            If Not IsNothing(SQLtrn) Then
                SQLtrn.Rollback()
            End If

            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0004_USER UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0004_USER UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        Finally
            If Not IsNothing(SQLtrn) Then
                SQLtrn.Dispose()
                SQLtrn = Nothing
            End If

            SQLcmd.Dispose()
            SQLcmd = Nothing
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' ユーザーマスター更新
    ''' </summary>
    ''' <param name="CO0004row"></param>
    ''' <param name="WW_DATENOW"></param>
    ''' <param name="SQLcon"></param>
    ''' <param name="SQLcmd"></param>
    ''' <param name="SQLtrn"></param>
    ''' <remarks></remarks>
    Protected Sub ExecuteUser(ByVal CO0004row As DataRow, ByVal WW_DATENOW As DateTime,
                              ByVal SQLcon As SqlConnection, ByVal SQLcmd As SqlCommand, ByRef SQLtrn As SqlTransaction)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        S0004_USER" _
            & "    WHERE" _
            & "        USERID       = @P1" _
            & "        AND STYMD    = @P2" _
            & "        AND CAMPCODE = @P4 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE S0004_USER" _
            & "    SET" _
            & "        ENDYMD       = @P3     , ORG        = @P5" _
            & "        , STAFFCODE  = @P6     , STAFFNAMES = @P7" _
            & "        , STAFFNAMEL = @P8     , CAMPROLE   = @P9" _
            & "        , MAPROLE    = @P10    , ORGROLE    = @P11" _
            & "        , VIEWPROFID = @P12    , RPRTPROFID = @P13" _
            & "        , MAPID      = @P14    , VARIANT    = @P15" _
            & "        , DELFLG     = @P16    , UPDYMD     = @P18" _
            & "        , UPDUSER    = @P19    , UPDTERMID  = @P20" _
            & "        , RECEIVEYMD = @P21" _
            & "    WHERE" _
            & "        USERID       = @P1" _
            & "        AND STYMD    = @P2" _
            & "        AND CAMPCODE = @P4 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO S0004_USER" _
            & "        (USERID         , STYMD" _
            & "        , ENDYMD        , CAMPCODE" _
            & "        , ORG           , STAFFCODE" _
            & "        , STAFFNAMES    , STAFFNAMEL" _
            & "        , CAMPROLE      , MAPROLE" _
            & "        , ORGROLE       , VIEWPROFID" _
            & "        , RPRTPROFID    , MAPID" _
            & "        , VARIANT       , DELFLG" _
            & "        , INITYMD       , UPDYMD" _
            & "        , UPDUSER       , UPDTERMID" _
            & "        , RECEIVEYMD)" _
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
            & "        , @P21) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        SQLcmd = New SqlCommand(SQLStr, SQLcon, SQLtrn)

        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)           'ユーザID
        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                   '開始年月日
        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                   '終了年月日
        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)           '所属会社
        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)           '所属組織
        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)           '社員コード
        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 20)           '社員名(短)
        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 50)           '社員名(長)
        Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)           '会社権限
        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)          '更新権限
        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          '部署権限
        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)          '画面プロファイルID
        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)          '帳票プロファイルID
        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 50)          '画面ID
        Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 50)          '変数
        Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 1)           '削除フラグ
        Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.DateTime)              '登録年月日
        Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.DateTime)              '更新年月日
        Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 20)          '更新ユーザーID
        Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 30)          '更新端末
        Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.DateTime)              '集信日時

        PARA01.Value = CO0004row("USERID")
        PARA02.Value = CO0004row("STYMD")
        PARA03.Value = CO0004row("ENDYMD")
        PARA04.Value = CO0004row("CAMPCODE")
        PARA05.Value = CO0004row("ORG")
        PARA06.Value = CO0004row("STAFFCODE")
        PARA07.Value = CO0004row("STAFFNAMES")
        PARA08.Value = CO0004row("STAFFNAMEL")
        PARA09.Value = CO0004row("CAMPROLE")
        PARA10.Value = CO0004row("MAPROLE")
        PARA11.Value = CO0004row("ORGROLE")
        PARA12.Value = CO0004row("VIEWPROFID")
        PARA13.Value = CO0004row("RPRTPROFID")
        PARA14.Value = CO0004row("MAPID")
        PARA15.Value = CO0004row("VARIANT")
        PARA16.Value = CO0004row("DELFLG")
        PARA17.Value = WW_DATENOW
        PARA18.Value = WW_DATENOW
        PARA19.Value = Master.USERID
        PARA20.Value = Master.USERTERMID
        PARA21.Value = C_DEFAULT_YMD

        SQLcmd.CommandTimeout = 300
        SQLcmd.ExecuteNonQuery()

    End Sub

    ''' <summary>
    ''' ユーザーパスワードマスター更新
    ''' </summary>
    ''' <param name="CO0004row"></param>
    ''' <param name="WW_DATENOW"></param>
    ''' <param name="SQLcon"></param>
    ''' <param name="SQLcmd"></param>
    ''' <param name="SQLtrn"></param>
    ''' <remarks></remarks>
    Protected Sub ExecuteUserPass(ByVal CO0004row As DataRow, ByVal WW_DATENOW As DateTime,
                                  ByVal SQLcon As SqlConnection, ByVal SQLcmd As SqlCommand, ByRef SQLtrn As SqlTransaction)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        S0014_USERPASS" _
            & "    WHERE" _
            & "        USERID = @P1 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE S0014_USERPASS" _
            & "    SET" _
            & "        PASSWORD     = @P2    , MISSCNT    = @P3" _
            & "        , PASSENDYMD = @P4    , DELFLG     = @P5" _
            & "        , UPDYMD     = @P7    , UPDUSER    = @P8" _
            & "        , UPDTERMID  = @P9    , RECEIVEYMD = @P10" _
            & "    WHERE" _
            & "        USERID = @P1 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO S0014_USERPASS" _
            & "        (USERID        , PASSWORD" _
            & "        , MISSCNT      , PASSENDYMD" _
            & "        , DELFLG       , INITYMD" _
            & "        , UPDYMD       , UPDUSER" _
            & "        , UPDTERMID    , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1     , @P2" _
            & "        , @P3    , @P4" _
            & "        , @P5    , @P6" _
            & "        , @P7    , @P8" _
            & "        , @P9    , @P10) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        SQLcmd = New SqlCommand(SQLStr, SQLcon, SQLtrn)

        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)       'ユーザID
        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 30)       'パスワード
        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Int)                '誤り回数
        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)               'パスワード有効期限
        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)        '削除フラグ
        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.DateTime)           '登録年月日
        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.DateTime)           '更新年月日
        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 20)       '更新ユーザーID
        Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 30)       '更新端末
        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)          '集信日時

        PARA01.Value = CO0004row("USERID")
        PARA02.Value = CO0004row("PASSWORD")
        PARA03.Value = CO0004row("MISSCNT")
        PARA04.Value = CO0004row("PASSENDYMD")
        PARA05.Value = CO0004row("DELFLG")
        PARA06.Value = WW_DATENOW
        PARA07.Value = WW_DATENOW
        PARA08.Value = Master.USERID
        PARA09.Value = Master.USERTERMID
        PARA10.Value = C_DEFAULT_YMD

        SQLcmd.CommandTimeout = 300
        SQLcmd.ExecuteNonQuery()

    End Sub

    ''' <summary>
    ''' 権限、承認設定マスタ削除
    ''' </summary>
    ''' <param name="CO0004row"></param>
    ''' <param name="I_OBJECT"></param>
    ''' <param name="I_APPROVALNUM"></param>
    ''' <param name="WW_DATENOW"></param>
    ''' <param name="SQLcon"></param>
    ''' <param name="SQLcmd"></param>
    ''' <param name="SQLtrn"></param>
    ''' <remarks></remarks>
    Protected Sub DeleteApproval(ByVal CO0004row As DataRow, ByVal I_OBJECT As String, ByVal I_APPROVALNUM As Integer, ByVal WW_DATENOW As DateTime,
                                 ByVal SQLcon As SqlConnection, ByVal SQLcmd As SqlCommand, ByRef SQLtrn As SqlTransaction)

        '○ 権限マスタ削除
        Dim SQLStr As String =
              " UPDATE S0005_AUTHOR" _
            & " SET" _
            & "    DELFLG       = @P5" _
            & "    , UPDYMD     = @P6" _
            & "    , UPDUSER    = @P7" _
            & "    , UPDTERMID  = @P8" _
            & "    , RECEIVEYMD = @P9" _
            & " WHERE" _
            & "    USERID       = @P1" _
            & "    AND CAMPCODE = @P2" _
            & "    AND OBJECT   = @P3" _
            & "    AND STYMD    = @P4"

        SQLcmd = New SqlCommand(SQLStr, SQLcon, SQLtrn)

        Dim PARA101 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)          'ユーザID
        Dim PARA102 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)          '会社コード
        Dim PARA103 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)          'オブジェクト
        Dim PARA104 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                  '開始年月日
        Dim PARA105 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)           '削除フラグ
        Dim PARA106 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.DateTime)              '更新年月日
        Dim PARA107 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 20)          '更新ユーザーID
        Dim PARA108 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 30)          '更新端末
        Dim PARA109 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.DateTime)              '集信日時

        PARA101.Value = CO0004row("USERID")
        PARA102.Value = CO0004row("CAMPCODE")
        PARA103.Value = I_OBJECT
        PARA104.Value = CO0004row("STYMD")
        PARA105.Value = C_DELETE_FLG.DELETE
        PARA106.Value = WW_DATENOW
        PARA107.Value = Master.USERID
        PARA108.Value = Master.USERTERMID
        PARA109.Value = C_DEFAULT_YMD

        SQLcmd.CommandTimeout = 300
        SQLcmd.ExecuteNonQuery()


        '○ 承認設定マスタ削除
        SQLStr =
              " UPDATE S0022_APPROVAL" _
            & " SET" _
            & "    DELFLG       = @P7" _
            & "    , UPDYMD     = @P8" _
            & "    , UPDUSER    = @P9" _
            & "    , UPDTERMID  = @P10" _
            & "    , RECEIVEYMD = @P11" _
            & " WHERE" _
            & "    CAMPCODE      = @P1" _
            & "    AND MAPID     = @P2" _
            & "    AND EVENTCODE = @P3" _
            & "    AND STEP      = @P4" _
            & "    AND STAFFCODE = @P5" _
            & "    AND STYMD     = @P6"

        SQLcmd = New SqlCommand(SQLStr, SQLcon, SQLtrn)

        Dim PARA201 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)          '会社コード
        Dim PARA202 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)          '画面ID
        Dim PARA203 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)          'イベントコード
        Dim PARA204 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)          '承認ステップ
        Dim PARA205 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)          '社員コード
        Dim PARA206 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)                  '有効開始日
        Dim PARA207 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 1)           '削除フラグ
        Dim PARA208 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.DateTime)              '更新年月日
        Dim PARA209 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)          '更新ユーザーID
        Dim PARA210 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 30)         '更新端末
        Dim PARA211 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)             '集信日時

        PARA201.Value = CO0004row("CAMPCODE")
        PARA202.Value = "T00009"
        PARA203.Value = "残業申請"
        If I_APPROVALNUM = 1 Then
            PARA204.Value = "01"
        Else
            PARA204.Value = "02"
        End If
        PARA205.Value = CO0004row("STAFFCODE")
        PARA206.Value = CO0004row("STYMD")
        PARA207.Value = C_DELETE_FLG.DELETE
        PARA208.Value = WW_DATENOW
        PARA209.Value = Master.USERID
        PARA210.Value = Master.USERTERMID
        PARA211.Value = C_DEFAULT_YMD

        SQLcmd.CommandTimeout = 300
        SQLcmd.ExecuteNonQuery()

    End Sub

    ''' <summary>
    ''' 残業承認ロール取得
    ''' </summary>
    ''' <param name="I_OBJECT"></param>
    ''' <param name="I_ROLE"></param>
    ''' <param name="O_TABLE"></param>
    ''' <remarks></remarks>
    Protected Sub GetApprovalROLE(ByVal I_OBJECT As String, ByVal I_ROLE As String, ByRef O_TABLE As DataTable)

        '○ 承認ロール取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLStr As String =
                  " SELECT" _
                & "    RTRIM(CODE) AS ORG" _
                & " FROM" _
                & "    S0006_ROLE" _
                & " WHERE" _
                & "    CAMPCODE    = @P1" _
                & "    AND OBJECT  = @P2" _
                & "    AND ROLE    = @P3" _
                & "    AND STYMD  <= @P4" _
                & "    AND ENDYMD >= @P4" _
                & "    AND DELFLG <> @P5"

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)       '会社コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)       'オブジェクト
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)       'ロール
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)               '開始年月日
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)        '削除フラグ

                PARA01.Value = work.WF_SEL_CAMPCODE.Text
                PARA02.Value = I_OBJECT
                PARA03.Value = I_ROLE
                PARA04.Value = Date.Now
                PARA05.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    'フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_TABLE.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    'テーブル検索結果をテーブル格納
                    O_TABLE.Load(SQLdr)
                End Using
            End Using
        End Using

    End Sub

    ''' <summary>
    ''' 権限マスター更新
    ''' </summary>
    ''' <param name="CO0004row"></param>
    ''' <param name="I_OBJECT"></param>
    ''' <param name="I_FIELD"></param>
    ''' <param name="WW_DATENOW"></param>
    ''' <param name="SQLcon"></param>
    ''' <param name="SQLcmd"></param>
    ''' <param name="SQLtrn"></param>
    ''' <remarks></remarks>
    Protected Sub ExecuteAuthor(ByVal CO0004row As DataRow, ByVal I_OBJECT As String, ByVal I_FIELD As String, ByVal WW_DATENOW As DateTime,
                                  ByVal SQLcon As SqlConnection, ByVal SQLcmd As SqlCommand, ByRef SQLtrn As SqlTransaction)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        S0005_AUTHOR" _
            & "    WHERE" _
            & "        USERID       = @P1" _
            & "        AND CAMPCODE = @P2" _
            & "        AND OBJECT   = @P3" _
            & "        AND ROLE     = @P4" _
            & "        AND STYMD    = @P6 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE S0005_AUTHOR" _
            & "    SET" _
            & "        ENDYMD      = @P7     , ROLENAMES  = @P8" _
            & "        , ROLENAMEL = @P9     , DELFLG     = @P10" _
            & "        , UPDYMD    = @P12    , UPDUSER    = @P13" _
            & "        , UPDTERMID = @P14    , RECEIVEYMD = @P15" _
            & "    WHERE" _
            & "        USERID       = @P1" _
            & "        AND CAMPCODE = @P2" _
            & "        AND OBJECT   = @P3" _
            & "        AND ROLE     = @P4" _
            & "        AND STYMD    = @P6 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO S0005_AUTHOR" _
            & "        (USERID        , CAMPCODE" _
            & "        , OBJECT       , ROLE" _
            & "        , SEQ          , STYMD" _
            & "        , ENDYMD       , ROLENAMES" _
            & "        , ROLENAMEL    , DELFLG" _
            & "        , INITYMD      , UPDYMD" _
            & "        , UPDUSER      , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2" _
            & "        , @P3     , @P4" _
            & "        , @P5     , @P6" _
            & "        , @P7     , @P8" _
            & "        , @P9     , @P10" _
            & "        , @P11    , @P12" _
            & "        , @P13    , @P14" _
            & "        , @P15) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        SQLcmd = New SqlCommand(SQLStr, SQLcon, SQLtrn)

        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)           'ユーザID
        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)           '会社コード
        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)           'オブジェクト
        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)           'ロール
        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Int)                    '表示順番
        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)                   '開始年月日
        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                   '終了年月日
        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 20)           'ロール名称(短)
        Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 50)           'ロール名称(長)
        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 1)           '削除フラグ
        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)              '登録年月日
        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.DateTime)              '更新年月日
        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)          '更新ユーザーID
        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 30)          '更新端末
        Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.DateTime)              '集信日時

        PARA01.Value = CO0004row("USERID")
        PARA02.Value = CO0004row("CAMPCODE")
        PARA03.Value = I_OBJECT
        PARA04.Value = CO0004row(I_FIELD)
        PARA05.Value = 1
        PARA06.Value = CO0004row("STYMD")
        PARA07.Value = CO0004row("ENDYMD")
        PARA08.Value = CO0004row("STAFFNAMES")
        PARA09.Value = CO0004row("ORGNAMES") & "_" & CO0004row("STAFFNAMES")
        PARA10.Value = CO0004row("DELFLG")
        PARA11.Value = WW_DATENOW
        PARA12.Value = WW_DATENOW
        PARA13.Value = Master.USERID
        PARA14.Value = Master.USERTERMID
        PARA15.Value = C_DEFAULT_YMD

        SQLcmd.CommandTimeout = 300
        SQLcmd.ExecuteNonQuery()

    End Sub

    ''' <summary>
    ''' 承認設定マスタ更新
    ''' </summary>
    ''' <param name="CO0004row"></param>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_TYPE"></param>
    ''' <param name="I_ORG"></param>
    ''' <param name="I_APPROVALNUM"></param>
    ''' <param name="WW_DATENOW"></param>
    ''' <param name="SQLcon"></param>
    ''' <param name="SQLcmd"></param>
    ''' <param name="SQLtrn"></param>
    ''' <remarks></remarks>
    Protected Sub ExecuteApproval(ByVal CO0004row As DataRow, ByVal I_FIELD As String, ByVal I_TYPE As String, ByVal I_ORG As DataTable, ByVal I_APPROVALNUM As Integer, ByVal WW_DATENOW As DateTime,
                                  ByVal SQLcon As SqlConnection, ByVal SQLcmd As SqlCommand, ByRef SQLtrn As SqlTransaction)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        S0022_APPROVAL" _
            & "    WHERE" _
            & "        CAMPCODE      = @P1" _
            & "        AND MAPID     = @P2" _
            & "        AND EVENTCODE = @P3" _
            & "        AND SUBCODE   = @P4" _
            & "        AND STEP      = @P5" _
            & "        AND STAFFCODE = @P6" _
            & "        AND STYMD     = @P7 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE S0022_APPROVAL" _
            & "    SET" _
            & "        ENDYMD      = @P8     , APPROVALTYPE = @P9" _
            & "        , MAILVALID = @P10    , MAILID       = @P11" _
            & "        , REMARK    = @P12    , DELFLG       = @P13" _
            & "        , UPDYMD    = @P15    , UPDUSER      = @P16" _
            & "        , UPDTERMID = @P17    , RECEIVEYMD   = @P18" _
            & "    WHERE" _
            & "        CAMPCODE      = @P1" _
            & "        AND MAPID     = @P2" _
            & "        AND EVENTCODE = @P3" _
            & "        AND SUBCODE   = @P4" _
            & "        AND STEP      = @P5" _
            & "        AND STAFFCODE = @P6" _
            & "        AND STYMD     = @P7 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO S0022_APPROVAL" _
            & "        (CAMPCODE         , MAPID" _
            & "        , EVENTCODE       , SUBCODE" _
            & "        , STEP            , STAFFCODE" _
            & "        , STYMD           , ENDYMD" _
            & "        , APPROVALTYPE    , MAILVALID" _
            & "        , MAILID          , REMARK" _
            & "        , DELFLG          , INITYMD" _
            & "        , UPDYMD          , UPDUSER" _
            & "        , UPDTERMID       , RECEIVEYMD)" _
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

        SQLcmd = New SqlCommand(SQLStr, SQLcon, SQLtrn)

        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)           '会社コード
        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)           '画面ID
        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)           'イベントコード
        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)           'サブコード
        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)           '承認ステップ
        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)           '社員コード
        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                   '有効開始日
        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Date)                   '有効終了日
        Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)           '承認区分
        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 1)           'メール有無
        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          'メール内容ID
        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 50)          '備考
        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 1)           '削除フラグ
        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)              '登録年月日
        Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.DateTime)              '更新年月日
        Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 20)          '更新ユーザID
        Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 30)          '更新端末
        Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.DateTime)              '集信日時

        For Each ORGrow As DataRow In I_ORG.Rows
            PARA01.Value = CO0004row("CAMPCODE")
            PARA02.Value = "T00009"
            PARA03.Value = "残業申請"
            PARA04.Value = ORGrow("ORG")
            If I_APPROVALNUM = 1 Then
                PARA05.Value = "01"
            Else
                PARA05.Value = "02"
            End If
            PARA06.Value = CO0004row("STAFFCODE")
            PARA07.Value = CO0004row("STYMD")
            PARA08.Value = CO0004row("ENDYMD")
            PARA09.Value = CO0004row(I_TYPE)
            PARA10.Value = ""
            PARA11.Value = ""
            PARA12.Value = CO0004row("ORGNAMES") & "_" & CO0004row("STAFFNAMES")
            PARA13.Value = CO0004row("DELFLG")
            PARA14.Value = WW_DATENOW
            PARA15.Value = WW_DATENOW
            PARA16.Value = Master.USERID
            PARA17.Value = Master.USERTERMID
            PARA18.Value = C_DEFAULT_YMD

            SQLcmd.CommandTimeout = 300
            SQLcmd.ExecuteNonQuery()
        Next

    End Sub

    ''' <summary>
    ''' 更新ジャーナル出力
    ''' </summary>
    ''' <param name="CO0004row"></param>
    ''' <param name="SQLcon"></param>
    ''' <param name="SQLcmd"></param>
    ''' <remarks></remarks>
    Protected Sub ExecuteJournal(ByVal CO0004row As DataRow, ByVal SQLcon As SqlConnection, ByVal SQLcmd As SqlCommand)

        '○ ユーザーマスタ
        Dim SQLJnl As String =
              " SELECT" _
            & "    USERID" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , CAMPCODE" _
            & "    , ORG" _
            & "    , STAFFCODE" _
            & "    , STAFFNAMES" _
            & "    , STAFFNAMEL" _
            & "    , CAMPROLE" _
            & "    , MAPROLE" _
            & "    , ORGROLE" _
            & "    , VIEWPROFID" _
            & "    , RPRTPROFID" _
            & "    , MAPID" _
            & "    , VARIANT" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    S0004_USER" _
            & " WHERE" _
            & "    USERID       = @P1" _
            & "    AND STYMD    = @P2" _
            & "    AND CAMPCODE = @P3"

        SQLcmd = New SqlCommand(SQLJnl, SQLcon)

        Dim JPARA101 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)         'ユーザID
        Dim JPARA102 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                 '開始年月日
        Dim JPARA103 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)         '所属会社

        JPARA101.Value = CO0004row("USERID")
        JPARA102.Value = CO0004row("STYMD")
        JPARA103.Value = CO0004row("CAMPCODE")

        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
            CO0004UPDtbl = New DataTable
            For index As Integer = 0 To SQLdr.FieldCount - 1
                CO0004UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
            Next

            CO0004UPDtbl.Clear()
            CO0004UPDtbl.Load(SQLdr)

            For Each CO0004UPDrow As DataRow In CO0004UPDtbl.Rows
                CS0020JOURNAL.TABLENM = "S0004_USER"
                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                CS0020JOURNAL.ROW = CO0004UPDrow
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
        End Using


        '○ ユーザーパスワードマスタ
        If CO0004row("DELFLG") <> C_DELETE_FLG.DELETE Then
            SQLJnl =
                  " SELECT" _
                & "    USERID" _
                & "    , PASSWORD" _
                & "    , MISSCNT" _
                & "    , PASSENDYMD" _
                & "    , DELFLG" _
                & "    , INITYMD" _
                & "    , UPDYMD" _
                & "    , UPDUSER" _
                & "    , UPDTERMID" _
                & "    , RECEIVEYMD" _
                & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
                & " FROM" _
                & "    S0014_USERPASS" _
                & " WHERE" _
                & "    USERID = @P1"

            SQLcmd = New SqlCommand(SQLJnl, SQLcon)

            Dim JPARA201 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)         'ユーザID

            JPARA201.Value = CO0004row("USERID")

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                CO0004UPDtbl = New DataTable
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    CO0004UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                CO0004UPDtbl.Clear()
                CO0004UPDtbl.Load(SQLdr)

                For Each CO0004UPDrow As DataRow In CO0004UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "S0014_USERPASS"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = CO0004UPDrow
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
            End Using
        End If


        '○ 権限マスタ
        SQLJnl =
              " SELECT" _
            & "    USERID" _
            & "    , CAMPCODE" _
            & "    , OBJECT" _
            & "    , ROLE" _
            & "    , SEQ" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , ROLENAMES" _
            & "    , ROLENAMEL" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    S0005_AUTHOR" _
            & " WHERE" _
            & "    USERID       = @P1" _
            & "    AND CAMPCODE = @P2" _
            & "    AND OBJECT  IN (@P3, @P4)" _
            & "    AND STYMD    = @P5"

        SQLcmd = New SqlCommand(SQLJnl, SQLcon)

        Dim JPARA301 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)         'ユーザID
        Dim JPARA302 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)         '会社コード
        Dim JPARA303 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)         'オブジェクト
        Dim JPARA304 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)         'オブジェクト
        Dim JPARA305 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                 '開始年月日

        JPARA301.Value = CO0004row("USERID")
        JPARA302.Value = CO0004row("CAMPCODE")
        JPARA303.Value = "APPROVAL1"
        JPARA304.Value = "APPROVAL2"
        JPARA305.Value = CO0004row("STYMD")

        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
            CO0004UPDtbl = New DataTable
            For index As Integer = 0 To SQLdr.FieldCount - 1
                CO0004UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
            Next

            CO0004UPDtbl.Clear()
            CO0004UPDtbl.Load(SQLdr)

            For Each CO0004UPDrow As DataRow In CO0004UPDtbl.Rows
                CS0020JOURNAL.TABLENM = "S0005_AUTHOR"
                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                CS0020JOURNAL.ROW = CO0004UPDrow
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
        End Using


        '○ 承認設定マスタ
        SQLJnl =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , MAPID" _
            & "    , EVENTCODE" _
            & "    , SUBCODE" _
            & "    , STEP" _
            & "    , STAFFCODE" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , APPROVALTYPE" _
            & "    , MAILVALID" _
            & "    , MAILID" _
            & "    , REMARK" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    S0022_APPROVAL" _
            & " WHERE" _
            & "    CAMPCODE      = @P1" _
            & "    AND MAPID     = @P2" _
            & "    AND EVENTCODE = @P3" _
            & "    AND STEP     IN (@P4, @P5)" _
            & "    AND STAFFCODE = @P6" _
            & "    AND STYMD     = @P7"

        SQLcmd = New SqlCommand(SQLJnl, SQLcon)

        Dim JPARA401 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)         '会社コード
        Dim JPARA402 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)         '画面ID
        Dim JPARA403 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)         'イベントコード
        Dim JPARA404 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)         '承認ステップ
        Dim JPARA405 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)         '承認ステップ
        Dim JPARA406 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)         '社員コード
        Dim JPARA407 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                 '有効開始日

        JPARA401.Value = CO0004row("CAMPCODE")
        JPARA402.Value = "T00009"
        JPARA403.Value = "残業申請"
        JPARA404.Value = "01"
        JPARA405.Value = "02"
        JPARA406.Value = CO0004row("STAFFCODE")
        JPARA407.Value = CO0004row("STYMD")

        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
            CO0004UPDtbl = New DataTable
            For index As Integer = 0 To SQLdr.FieldCount - 1
                CO0004UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
            Next

            CO0004UPDtbl.Clear()
            CO0004UPDtbl.Load(SQLdr)

            For Each CO0004UPDrow As DataRow In CO0004UPDtbl.Rows
                CS0020JOURNAL.TABLENM = "S0022_APPROVAL"
                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                CS0020JOURNAL.ROW = CO0004UPDrow
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
        End Using

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
        CS0030REPORT.TBLDATA = CO0004tbl                        'データ参照  Table
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

        WF_USERID.Focus()

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
        CS0030REPORT.TBLDATA = CO0004tbl                        'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

        WF_USERID.Focus()

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
        WF_USERID.Focus()

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(CO0004tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        WF_USERID.Focus()

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
        WF_Sel_LINECNT.Text = CO0004tbl.Rows(WW_LINECNT)("LINECNT")

        '会社コード
        WF_CAMPCODE.Text = CO0004tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        'ユーザーID
        WF_USERID.Text = CO0004tbl.Rows(WW_LINECNT)("USERID")

        '有効年月日
        WF_STYMD.Text = CO0004tbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = CO0004tbl.Rows(WW_LINECNT)("ENDYMD")

        '削除
        WF_DELFLG.Text = CO0004tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '○ Repeater設定
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            WW_FIELD_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label)
            If WW_FIELD_OBJ.text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FIELD_OBJ.text, CO0004tbl.Rows(WW_LINECNT)(WW_FIELD_OBJ.text))
                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_VALUE
                '名称取得
                CODENAME_get(WW_FIELD_OBJ.text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_TEXT
            End If

            '中
            WW_FIELD_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label)
            If WW_FIELD_OBJ.text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FIELD_OBJ.text, CO0004tbl.Rows(WW_LINECNT)(WW_FIELD_OBJ.text))
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_VALUE
                '名称取得
                CODENAME_get(WW_FIELD_OBJ.text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_TEXT
            End If

            '右
            WW_FIELD_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label)
            If WW_FIELD_OBJ.text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FIELD_OBJ.text, CO0004tbl.Rows(WW_LINECNT)(WW_FIELD_OBJ.text))
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_VALUE
                '名称取得
                CODENAME_get(WW_FIELD_OBJ.text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
            End If
        Next

        '○ 状態をクリア
        For Each CO0004row As DataRow In CO0004tbl.Rows
            Select Case CO0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case CO0004tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                CO0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                CO0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                CO0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                CO0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                CO0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(CO0004tbl)

        WF_USERID.Focus()
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

        WF_USERID.Focus()

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
        Master.CreateEmptyTable(CO0004INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim CO0004INProw As DataRow = CO0004INPtbl.NewRow

            '○ 初期クリア
            For Each CO0004INPcol As DataColumn In CO0004INPtbl.Columns
                If IsDBNull(CO0004INProw.Item(CO0004INPcol)) OrElse IsNothing(CO0004INProw.Item(CO0004INPcol)) Then
                    Select Case CO0004INPcol.ColumnName
                        Case "LINECNT"
                            CO0004INProw.Item(CO0004INPcol) = 0
                        Case "OPERATION"
                            CO0004INProw.Item(CO0004INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            CO0004INProw.Item(CO0004INPcol) = 0
                        Case "SELECT"
                            CO0004INProw.Item(CO0004INPcol) = 1
                        Case "HIDDEN"
                            CO0004INProw.Item(CO0004INPcol) = 0
                        Case Else
                            CO0004INProw.Item(CO0004INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("USERID") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                For Each CO0004row As DataRow In CO0004tbl.Rows
                    If XLSTBLrow("CAMPCODE") = CO0004row("CAMPCODE") AndAlso
                        XLSTBLrow("USERID") = CO0004row("USERID") AndAlso
                        XLSTBLrow("STYMD") = CO0004row("STYMD") Then
                        CO0004INProw.ItemArray = CO0004row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '所属会社
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                CO0004INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            'ユーザーID
            If WW_COLUMNS.IndexOf("USERID") >= 0 Then
                CO0004INProw("USERID") = XLSTBLrow("USERID")
            End If

            '開始年月日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        CO0004INProw("STYMD") = ""
                    Else
                        CO0004INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    CO0004INProw("STYMD") = ""
                End Try
            End If

            '終了年月日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        CO0004INProw("ENDYMD") = ""
                    Else
                        CO0004INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    CO0004INProw("ENDYMD") = ""
                End Try
            End If

            '所属組織
            If WW_COLUMNS.IndexOf("ORG") >= 0 Then
                CO0004INProw("ORG") = XLSTBLrow("ORG")
            End If

            '社員コード
            If WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                CO0004INProw("STAFFCODE") = XLSTBLrow("STAFFCODE")
            End If

            '社員名(短)
            If WW_COLUMNS.IndexOf("STAFFNAMES") >= 0 Then
                CO0004INProw("STAFFNAMES") = XLSTBLrow("STAFFNAMES")
            End If

            '社員名(長)
            If WW_COLUMNS.IndexOf("STAFFNAMEL") >= 0 Then
                CO0004INProw("STAFFNAMEL") = XLSTBLrow("STAFFNAMEL")
            End If

            '画面ID
            If WW_COLUMNS.IndexOf("MAPID") >= 0 Then
                CO0004INProw("MAPID") = XLSTBLrow("MAPID")
            End If

            '変数
            If WW_COLUMNS.IndexOf("VARIANT") >= 0 Then
                CO0004INProw("VARIANT") = XLSTBLrow("VARIANT")
            End If

            'パスワード
            If WW_COLUMNS.IndexOf("PASSWORD") >= 0 Then
                CO0004INProw("PASSWORD") = XLSTBLrow("PASSWORD")
            End If

            '誤り回数
            If WW_COLUMNS.IndexOf("MISSCNT") >= 0 Then
                CO0004INProw("MISSCNT") = XLSTBLrow("MISSCNT")
            End If

            'パスワード有効期限
            If WW_COLUMNS.IndexOf("PASSENDYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("PASSENDYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        CO0004INProw("PASSENDYMD") = ""
                    Else
                        CO0004INProw("PASSENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    CO0004INProw("PASSENDYMD") = ""
                End Try
            End If

            '会社権限
            If WW_COLUMNS.IndexOf("CAMPROLE") >= 0 Then
                CO0004INProw("CAMPROLE") = XLSTBLrow("CAMPROLE")
            End If

            '更新権限
            If WW_COLUMNS.IndexOf("MAPROLE") >= 0 Then
                CO0004INProw("MAPROLE") = XLSTBLrow("MAPROLE")
            End If

            '部署権限
            If WW_COLUMNS.IndexOf("ORGROLE") >= 0 Then
                CO0004INProw("ORGROLE") = XLSTBLrow("ORGROLE")
            End If

            '画面プロファイルID
            If WW_COLUMNS.IndexOf("VIEWPROFID") >= 0 Then
                CO0004INProw("VIEWPROFID") = XLSTBLrow("VIEWPROFID")
            End If

            '帳票プロファイルID
            If WW_COLUMNS.IndexOf("RPRTPROFID") >= 0 Then
                CO0004INProw("RPRTPROFID") = XLSTBLrow("RPRTPROFID")
            End If

            '第１承認
            If WW_COLUMNS.IndexOf("ROLEAPPROVAL1") >= 0 Then
                CO0004INProw("ROLEAPPROVAL1") = XLSTBLrow("ROLEAPPROVAL1")
            End If

            '最終承認
            If WW_COLUMNS.IndexOf("ROLEAPPROVAL2") >= 0 Then
                CO0004INProw("ROLEAPPROVAL2") = XLSTBLrow("ROLEAPPROVAL2")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                CO0004INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '名称取得
            CODENAME_get("CAMPCODE", CO0004INProw("CAMPCODE"), CO0004INProw("CAMPNAMES"), WW_DUMMY)                                 '会社コード
            CODENAME_get("ORG", CO0004INProw("ORG"), CO0004INProw("ORGNAMES"), WW_DUMMY)                                            '所属部署
            CODENAME_get("ROLEAPPROVAL1", CO0004INProw("ROLEAPPROVAL1"), CO0004INProw("ROLEAPPROVAL1TYPE"), WW_DUMMY, "2")          '権限(第一承認)
            CODENAME_get("ROLEAPPROVAL2", CO0004INProw("ROLEAPPROVAL2"), CO0004INProw("ROLEAPPROVAL2TYPE"), WW_DUMMY, "2")          '権限(最終承認)

            CO0004INPtbl.Rows.Add(CO0004INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        CO0004tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(CO0004tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

        WF_USERID.Focus()

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
        DetailBoxToCO0004INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            CO0004tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0004tbl)

        '○ 詳細画面初期化
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        WF_USERID.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToCO0004INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)      '会社コード
        Master.EraseCharToIgnore(WF_USERID.Text)        'ユーザID
        Master.EraseCharToIgnore(WF_STYMD.Text)         '有効年月日(From)
        Master.EraseCharToIgnore(WF_ENDYMD.Text)        '有効年月日(To)
        Master.EraseCharToIgnore(WF_DELFLG.Text)        '削除

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_USERID.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(CO0004INPtbl)
        Dim CO0004INProw As DataRow = CO0004INPtbl.NewRow

        '○ 初期クリア
        For Each CO0004INPcol As DataColumn In CO0004INPtbl.Columns
            If IsDBNull(CO0004INProw.Item(CO0004INPcol)) OrElse IsNothing(CO0004INProw.Item(CO0004INPcol)) Then
                Select Case CO0004INPcol.ColumnName
                    Case "LINECNT"
                        CO0004INProw.Item(CO0004INPcol) = 0
                    Case "OPERATION"
                        CO0004INProw.Item(CO0004INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        CO0004INProw.Item(CO0004INPcol) = 0
                    Case "SELECT"
                        CO0004INProw.Item(CO0004INPcol) = 1
                    Case "HIDDEN"
                        CO0004INProw.Item(CO0004INPcol) = 0
                    Case Else
                        CO0004INProw.Item(CO0004INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            CO0004INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, CO0004INProw("LINECNT"))
            Catch ex As Exception
                CO0004INProw("LINECNT") = 0
            End Try
        End If

        CO0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        CO0004INProw("TIMSTP") = 0
        CO0004INProw("SELECT") = 1
        CO0004INProw("HIDDEN") = 0

        CO0004INProw("CAMPCODE") = WF_CAMPCODE.Text         '会社コード
        CO0004INProw("USERID") = WF_USERID.Text             'ユーザID
        CO0004INProw("STYMD") = WF_STYMD.Text               '有効年月日(From)
        CO0004INProw("ENDYMD") = WF_ENDYMD.Text             '有効年月日(To)
        CO0004INProw("DELFLG") = WF_DELFLG.Text             '削除

        CO0004INProw("CAMPNAMES") = ""                      '会社名称
        CO0004INProw("ORG") = ""                            '所属組織
        CO0004INProw("ORGNAMES") = ""                       '所属組織名
        CO0004INProw("STAFFCODE") = ""                      '社員コード
        CO0004INProw("STAFFNAMES") = ""                     '社員名(短)
        CO0004INProw("STAFFNAMEL") = ""                     '社員名(長)
        CO0004INProw("MAPID") = ""                          '画面ＩＤ
        CO0004INProw("VARIANT") = ""                        '変数
        CO0004INProw("PASSWORD") = ""                       'パスワード
        CO0004INProw("MISSCNT") = ""                        '誤り回数
        CO0004INProw("PASSENDYMD") = ""                     'パスワード有効期限
        CO0004INProw("CAMPROLE") = ""                       '会社権限
        CO0004INProw("MAPROLE") = ""                        '更新権限
        CO0004INProw("ORGROLE") = ""                        '部署権限
        CO0004INProw("VIEWPROFID") = ""                     '画面プロファイルID
        CO0004INProw("RPRTPROFID") = ""                     '帳票プロファイルID
        CO0004INProw("ROLEAPPROVAL1") = ""                  '第一承認
        CO0004INProw("ROLEAPPROVAL1TYPE") = ""              '第一承認タイプ
        CO0004INProw("ROLEAPPROVAL2") = ""                  '最終承認
        CO0004INProw("ROLEAPPROVAL2TYPE") = ""              '最終承認タイプ

        '○ Detail設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                CO0004INProw(CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '中
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                CO0004INProw(CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                CO0004INProw(CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
            End If
        Next

        '○ 名称取得
        CODENAME_get("CAMPCODE", CO0004INProw("CAMPCODE"), CO0004INProw("CAMPNAMES"), WW_DUMMY)                                 '会社コード
        CODENAME_get("ORG", CO0004INProw("ORG"), CO0004INProw("ORGNAMES"), WW_DUMMY)                                            '所属部署
        CODENAME_get("ROLEAPPROVAL1", CO0004INProw("ROLEAPPROVAL1"), CO0004INProw("ROLEAPPROVAL1TYPE"), WW_DUMMY, "2")          '権限(第一承認)
        CODENAME_get("ROLEAPPROVAL2", CO0004INProw("ROLEAPPROVAL2"), CO0004INProw("ROLEAPPROVAL2TYPE"), WW_DUMMY, "2")          '権限(最終承認)

        '○ チェック用テーブルに登録する
        CO0004INPtbl.Rows.Add(CO0004INProw)

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_USERID.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each CO0004row As DataRow In CO0004tbl.Rows
            Select Case CO0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(CO0004tbl)

        WF_Sel_LINECNT.Text = ""                            'LINECNT
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_USERID.Text = ""                                 'ユーザID
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
            Case "STAFFCODE"            '従業員
                O_ATTR = "REF_Field_DBclick('STAFFCODE', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_STAFFCODE & "');"
            Case "ORG"                  '所属部署
                O_ATTR = "REF_Field_DBclick('ORG', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_ORG & "');"
            Case "PASSENDYMD"           '有効期限
                O_ATTR = "REF_Field_DBclick('PASSENDYMD', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "CAMPROLE"             '会社権限
                O_ATTR = "REF_Field_DBclick('CAMPROLE', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & "');"
            Case "MAPROLE"              '更新権限
                O_ATTR = "REF_Field_DBclick('MAPROLE', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & "');"
            Case "ORGROLE"              '部署権限
                O_ATTR = "REF_Field_DBclick('ORGROLE', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & "');"
            Case "VIEWPROFID"           '画面プロファイルID
                O_ATTR = "REF_Field_DBclick('VIEWPROFID', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & "');"
            Case "RPRTPROFID"           '帳票プロファイルID
                O_ATTR = "REF_Field_DBclick('RPRTPROFID', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & "');"
            Case "ROLEAPPROVAL1"        '権限(第１承認)
                O_ATTR = "REF_Field_DBclick('ROLEAPPROVAL1', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & "');"
            Case "ROLEAPPROVAL2"        '権限(最終承認)
                O_ATTR = "REF_Field_DBclick('ROLEAPPROVAL2', 'WF_Rep_FIELD', '" & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & "');"
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
                    Case LIST_BOX_CLASSIFICATION.LC_STAFFCODE
                        '従業員
                        WF_LeftboxOpen.Value = "STAFFTABLEOpen"
                        Dim prmData = work.CreateStaffCodeParam(WF_CAMPCODE.Text)
                        .seTTableList(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeTable()

                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WW_FIELD
                            Case "WF_STYMD"         '有効年月日(From)
                                .WF_Calendar.Text = WF_STYMD.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = WF_ENDYMD.Text
                            Case "PASSENDYMD"       'パスワード有効期限
                                .WF_Calendar.Text = ""
                                For Each reitem As RepeaterItem In WF_DViewRep1.Items
                                    For i As Integer = 1 To 3
                                        If CType(reitem.FindControl("WF_Rep1_FIELD_" & i), Label).Text = "PASSENDYMD" Then
                                            .WF_Calendar.Text = CType(reitem.FindControl("WF_Rep1_VALUE_" & i), TextBox).Text
                                            Exit For
                                        End If
                                    Next

                                    If .WF_Calendar.Text <> "" Then
                                        Exit For
                                    End If
                                Next
                        End Select
                        .activeCalendar()

                    Case Else
                        '以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメーターを変える
                        Select Case WW_FIELD
                            Case "ORG"                  '所属部署
                                prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)
                            Case "CAMPROLE"             '会社権限
                                prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_CAMP", "1")
                            Case "MAPROLE"              '更新権限
                                prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_MAP", "1")
                            Case "ORGROLE"              '部署権限
                                prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_ORG", "1")
                            Case "VIEWPROFID"           '画面プロファイルID
                                prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_VIEWPROFID", "1")
                            Case "RPRTPROFID"           '帳票プロファイルID
                                prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_RPRTPROFID", "1")
                            Case "ROLEAPPROVAL1"        '権限(第１承認)
                                prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_APPROVAL", "1")
                            Case "ROLEAPPROVAL2"        '権限(最終承認)
                                prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_APPROVAL2", "1")
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
                Case "WF_SELSTAFFCODE"          '従業員
                    WF_SELSTAFFCODE.Text = Mid(WW_SelectValue, InStr(WW_SelectValue, "=") + 1, Len(WW_SelectValue))
                    WF_SELSTAFFCODE_TEXT.Text = Mid(WW_SelectText, InStr(WW_SelectText, "=") + 1, Len(WW_SelectText))
                    WF_SELSTAFFCODE.Focus()

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
            End Select

        Else
            '○ 明細部設定
            Select Case WF_FIELD_REP.Value
                Case "STAFFCODE"
                    WW_SelectValue = Mid(WW_SelectValue, InStr(WW_SelectValue, "=") + 1, Len(WW_SelectValue))
                    WW_SelectText = Mid(WW_SelectText, InStr(WW_SelectText, "=") + 1, Len(WW_SelectText))
                    GetStaffData(WW_SelectValue)

                Case "PASSENDYMD"
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(WW_SelectValue, WW_DATE)
                        WW_SelectValue = WW_DATE.ToString("yyyy/MM/dd")
                    Catch ex As Exception
                    End Try
            End Select

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
    ''' 従業員情報取得
    ''' </summary>
    ''' <param name="I_STAFF"></param>
    ''' <remarks></remarks>
    Protected Sub GetStaffData(ByVal I_STAFF As String)

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                Dim SQLStr As String =
                      " SELECT" _
                    & "    ISNULL(RTRIM(STAFFCODE), '')    AS STAFFCODE" _
                    & "    , ISNULL(RTRIM(STAFFNAMES), '') AS STAFFNAMES" _
                    & "    , ISNULL(RTRIM(STAFFNAMEL), '') AS STAFFNAMEL" _
                    & "    , ISNULL(RTRIM(HORG), '')       AS HORG" _
                    & " FROM" _
                    & "    MB001_STAFF" _
                    & " WHERE" _
                    & "    CAMPCODE      = @P1" _
                    & "    AND STAFFCODE = @P2" _
                    & "    AND STYMD    <= @P3" _
                    & "    AND ENDYMD   >= @P3" _
                    & "    AND DELFLG   <> @P4"

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '従業員コード
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '現在日付
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1)         '削除フラグ

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = I_STAFF
                    PARA3.Value = Date.Now
                    PARA4.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.Read Then
                            RepeaterValueSet("STAFFNAMES", SQLdr("STAFFNAMES"), "")
                            RepeaterValueSet("STAFFNAMEL", SQLdr("STAFFNAMEL"), "")
                            Dim WW_ORGNAMES As String = ""
                            CODENAME_get("ORG", SQLdr("HORG"), WW_ORGNAMES, WW_DUMMY)
                            RepeaterValueSet("ORG", SQLdr("HORG"), WW_ORGNAMES)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try

    End Sub

    ''' <summary>
    ''' 左ボックス選択時に伴う項目内容変更
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="I_TEXT"></param>
    ''' <remarks></remarks>
    Protected Sub RepeaterValueSet(ByVal I_FIELD As String, ByVal I_VALUE As String, ByVal I_TEXT As String)

        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text = I_FIELD Then
                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = I_VALUE
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = I_TEXT
                Exit For
            End If

            '中
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text = I_FIELD Then
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = I_VALUE
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = I_TEXT
                Exit For
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text = I_FIELD Then
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = I_VALUE
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = I_TEXT
                Exit For
            End If
        Next

    End Sub


    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "WF_SELSTAFFCODE"          '従業員
                    WF_SELSTAFFCODE.Focus()
                Case "WF_STYMD"                 '有効年月日(From)
                    WF_STYMD.Focus()
                Case "WF_ENDYMD"                '有効年月日(To)
                    WF_ENDYMD.Focus()
                Case "WF_DELFLG"                '削除
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
            WW_CheckMES1 = "・更新できないレコード(ユーザ更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each CO0004INProw As DataRow In CO0004INPtbl.Rows

            WW_LINE_ERR = ""

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", CO0004INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", CO0004INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '権限チェック
                CS0025AUTHORget.USERID = CS0050SESSION.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_COMP
                CS0025AUTHORget.CODE = CO0004INProw("CAMPCODE")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(ユーザ会社権限なし)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit Sub
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ユーザーID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "USERID", CO0004INProw("USERID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(ユーザIDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", CO0004INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", CO0004INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '日付大小チェック
            If CO0004INProw("STYMD") > CO0004INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(有効開始日＞有効終了日)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '範囲チェック
            If work.WF_SEL_STYMD.Text > CO0004INProw("STYMD") AndAlso
                work.WF_SEL_STYMD.Text > CO0004INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            If work.WF_SEL_ENDYMD.Text < CO0004INProw("STYMD") AndAlso
                work.WF_SEL_ENDYMD.Text < CO0004INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除フラグ
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", CO0004INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", CO0004INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '従業員コード
            WW_TEXT = CO0004INProw("STAFFCODE")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", CO0004INProw("STAFFCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0004INProw("STAFFCODE") = ""
                Else
                    '存在チェック
                    CODENAME_get("STAFFCODE", CO0004INProw("STAFFCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '従業員名称(短)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STAFFNAMES", CO0004INProw("STAFFNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(従業員名称(短)エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '従業員名称(長)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STAFFNAMEL", CO0004INProw("STAFFNAMEL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(従業員名称(長)エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '所属部署
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ORG", CO0004INProw("ORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("ORG", CO0004INProw("ORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(所属部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '権限チェック
                CS0025AUTHORget.USERID = CS0050SESSION.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                CS0025AUTHORget.CODE = CO0004INProw("ORG")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(ユーザ部署権限なし)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit Sub
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(所属部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面ID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MAPID", CO0004INProw("MAPID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(画面IDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '変数
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VARIANT", CO0004INProw("VARIANT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(変数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'パスワード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "PASSWORD", CO0004INProw("PASSWORD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(パスワードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '誤回数
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MISSCNT", CO0004INProw("MISSCNT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(誤回数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '有効期限
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "PASSENDYMD", CO0004INProw("PASSENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効期限エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '会社権限
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPROLE", CO0004INProw("CAMPROLE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                CODENAME_get("CAMPROLE", CO0004INProw("CAMPROLE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社権限エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社権限エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '更新権限
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MAPROLE", CO0004INProw("MAPROLE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                CODENAME_get("MAPROLE", CO0004INProw("MAPROLE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(更新権限エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(更新権限エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '部署権限
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ORGROLE", CO0004INProw("ORGROLE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                CODENAME_get("ORGROLE", CO0004INProw("ORGROLE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(部署権限エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(部署権限エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面プロファイルID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "VIEWPROFID", CO0004INProw("VIEWPROFID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If CO0004INProw("VIEWPROFID") <> C_DEFAULT_DATAKEY Then
                    CODENAME_get("VIEWPROFID", CO0004INProw("VIEWPROFID"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(画面プロファイルIDエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(画面プロファイルIDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '帳票プロファイルID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "RPRTPROFID", CO0004INProw("RPRTPROFID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If CO0004INProw("RPRTPROFID") <> C_DEFAULT_DATAKEY Then
                    CODENAME_get("RPRTPROFID", CO0004INProw("RPRTPROFID"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(帳票プロファイルIDエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(帳票プロファイルIDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '権限(第１承認)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ROLEAPPROVAL", CO0004INProw("ROLEAPPROVAL1"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                CODENAME_get("ROLEAPPROVAL1", CO0004INProw("ROLEAPPROVAL1"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(権限(第１承認)エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(権限(第１承認)エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '権限(最終承認)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ROLEAPPROVAL", CO0004INProw("ROLEAPPROVAL2"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                CODENAME_get("ROLEAPPROVAL2", CO0004INProw("ROLEAPPROVAL2"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(権限(最終承認)エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(権限(最終承認)エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '関連チェック
            If CO0004INProw("ROLEAPPROVAL1") <> "" AndAlso CO0004INProw("ROLEAPPROVAL2") <> "" Then
                WW_CheckMES1 = "・更新できないレコード(権限(第１、最終承認)エラー)です。"
                WW_CheckMES2 = "承認権限は、第１か最終のいずれかです。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If CO0004INProw("STAFFCODE") = "" Then
                If CO0004INProw("ROLEAPPROVAL1") <> "" AndAlso Not CO0004INProw("ROLEAPPROVAL1") Like "*自動*" Then
                    WW_CheckMES1 = "・更新できないレコード(権限(第１承認)エラー)です。"
                    WW_CheckMES2 = "承認権限は、従業員コードが必須です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                If CO0004INProw("ROLEAPPROVAL2") <> "" AndAlso Not CO0004INProw("ROLEAPPROVAL2") Like "*自動*" Then
                    WW_CheckMES1 = "・更新できないレコード(権限(最終承認)エラー)です。"
                    WW_CheckMES2 = "承認権限は、従業員コードが必須です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If CO0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    CO0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                CO0004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="CO0004row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal CO0004row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(CO0004row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社       =" & CO0004row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> ユーザID   =" & CO0004row("USERID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日 =" & CO0004row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日 =" & CO0004row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除       =" & CO0004row("DELFLG")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' CO0004tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CO0004tbl_UPD()

        '○ 画面状態設定
        For Each CO0004row As DataRow In CO0004tbl.Rows
            Select Case CO0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each CO0004INProw As DataRow In CO0004INPtbl.Rows

            'エラーレコード読み飛ばし
            If CO0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            CO0004INProw("OPERATION") = "Insert"

            'KEY項目が等しい(ENDYMD以外のKEYが同じ)
            For Each CO0004row As DataRow In CO0004tbl.Rows
                If CO0004row("CAMPCODE") = CO0004INProw("CAMPCODE") AndAlso
                    CO0004row("USERID") = CO0004INProw("USERID") AndAlso
                    CO0004row("STYMD") = CO0004INProw("STYMD") Then

                    '変更無は操作無
                    If CO0004row("ENDYMD") = CO0004INProw("ENDYMD") AndAlso
                        CO0004row("STAFFCODE") = CO0004INProw("STAFFCODE") AndAlso
                        CO0004row("STAFFNAMES") = CO0004INProw("STAFFNAMES") AndAlso
                        CO0004row("STAFFNAMEL") = CO0004INProw("STAFFNAMEL") AndAlso
                        CO0004row("ORG") = CO0004INProw("ORG") AndAlso
                        CO0004row("MAPID") = CO0004INProw("MAPID") AndAlso
                        CO0004row("VARIANT") = CO0004INProw("VARIANT") AndAlso
                        CO0004row("PASSWORD") = CO0004INProw("PASSWORD") AndAlso
                        CO0004row("MISSCNT") = CO0004INProw("MISSCNT") AndAlso
                        CO0004row("PASSENDYMD") = CO0004INProw("PASSENDYMD") AndAlso
                        CO0004row("CAMPROLE") = CO0004INProw("CAMPROLE") AndAlso
                        CO0004row("MAPROLE") = CO0004INProw("MAPROLE") AndAlso
                        CO0004row("ORGROLE") = CO0004INProw("ORGROLE") AndAlso
                        CO0004row("VIEWPROFID") = CO0004INProw("VIEWPROFID") AndAlso
                        CO0004row("RPRTPROFID") = CO0004INProw("RPRTPROFID") AndAlso
                        CO0004row("ROLEAPPROVAL1") = CO0004INProw("ROLEAPPROVAL1") AndAlso
                        CO0004row("ROLEAPPROVAL2") = CO0004INProw("ROLEAPPROVAL2") AndAlso
                        CO0004row("DELFLG") = CO0004INProw("DELFLG") Then
                        CO0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    CO0004INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each CO0004INProw As DataRow In CO0004INPtbl.Rows
            Select Case CO0004INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(CO0004INProw)
                Case "Insert"
                    TBL_INSERT_SUB(CO0004INProw)
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="CO0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef CO0004INProw As DataRow)

        For Each CO0004row As DataRow In CO0004tbl.Rows

            '同一(ENDYMD以外が同一KEY)レコード
            If CO0004INProw("CAMPCODE") = CO0004row("CAMPCODE") AndAlso
                CO0004INProw("USERID") = CO0004row("USERID") AndAlso
                CO0004INProw("STYMD") = CO0004row("STYMD") Then

                '画面入力テーブル項目設定
                CO0004INProw("LINECNT") = CO0004row("LINECNT")
                CO0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                CO0004INProw("TIMSTP") = CO0004row("TIMSTP")
                CO0004INProw("SELECT") = 1
                CO0004INProw("HIDDEN") = 0

                '項目テーブル項目設定
                CO0004row.ItemArray = CO0004INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="CO0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef CO0004INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim CO0004row As DataRow = CO0004tbl.NewRow
        CO0004row.ItemArray = CO0004INProw.ItemArray

        CO0004row("LINECNT") = CO0004tbl.Rows.Count + 1
        CO0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        CO0004row("TIMSTP") = "0"
        CO0004row("SELECT") = 1
        CO0004row("HIDDEN") = 0

        CO0004tbl.Rows.Add(CO0004row)

    End Sub


    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <param name="I_SUBCODE"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal I_SUBCODE As String = "1")

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
                Case "STAFFCODE"            '従業員コード
                    prmData = work.CreateStaffCodeParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"                  '所属部署
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "CAMPROLE"             '会社権限
                    prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_CAMP", I_SUBCODE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MAPROLE"              '更新権限
                    prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_MAP", I_SUBCODE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORGROLE"              '部署権限
                    prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_ORG", I_SUBCODE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "VIEWPROFID"           '画面プロファイルID
                    prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_VIEWPROFID", I_SUBCODE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "RPRTPROFID"           '帳票プロファイルID
                    prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_RPRTPROFID", I_SUBCODE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ROLEAPPROVAL1"        '権限(第１承認)
                    prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_APPROVAL", I_SUBCODE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ROLEAPPROVAL2"        '権限(最終承認)
                    prmData = work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "CO0004_APPROVAL2", I_SUBCODE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"               '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

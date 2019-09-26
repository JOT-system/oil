Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 従業員部署マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMB0002STAFFORG
    Inherits Page

    '○ 検索結果格納Table
    Private MB0002tbl As DataTable                          '一覧格納用テーブル
    Private MB0002INPtbl As DataTable                       'チェック用テーブル
    Private MB0002UPDtbl As DataTable                       '更新用テーブル

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
                    If Not Master.RecoverTable(MB0002tbl) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click()
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
            If Not IsNothing(MB0002tbl) Then
                MB0002tbl.Clear()
                MB0002tbl.Dispose()
                MB0002tbl = Nothing
            End If

            If Not IsNothing(MB0002INPtbl) Then
                MB0002INPtbl.Clear()
                MB0002INPtbl.Dispose()
                MB0002INPtbl = Nothing
            End If

            If Not IsNothing(MB0002UPDtbl) Then
                MB0002UPDtbl.Clear()
                MB0002UPDtbl.Dispose()
                MB0002UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRMB0002WRKINC.MAPID

        WF_SELCAMPCODE.Focus()
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

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MB0002S Then
            'Grid情報保存先のファイル名
            Master.createXMLSaveFile()

            '作業部署
            WF_SORG.Text = work.WF_SEL_SORG.Text
            CODENAME_get("SORG", WF_SORG.Text, WF_SORG_TEXT.Text, WW_DUMMY)
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
        Master.SaveTable(MB0002tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(MB0002tbl)

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

        If IsNothing(MB0002tbl) Then
            MB0002tbl = New DataTable
        End If

        If MB0002tbl.Columns.Count <> 0 Then
            MB0002tbl.Columns.Clear()
        End If

        MB0002tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを従業員マスタから取得する
        '　注意事項　日付について
        '　　権限判断はすべてDateNow。グループコード、名称取得は全てDateNow。表追加時の①はDateNow。
        '　　但し、表追加時の②および③は、TBL入力有効期限。
        '　　ログインユーザにて設定可能な従業員かどうかの判定は取得後に行う
        '　　検索条件で指定された従業員グループの判定は取得後に行う

        Dim SQLStr As String =
              " SELECT" _
            & "    0                                           AS LINECNT" _
            & "    , ''                                        AS OPERATION" _
            & "    , CAST(ISNULL(MB02.UPDTIMSTP, 0) AS BIGINT) AS TIMSTP" _
            & "    , 1                                         AS 'SELECT'" _
            & "    , 0                                         AS HIDDEN" _
            & "    , ISNULL(RTRIM(MB02.CAMPCODE), '')          AS CAMPCODE" _
            & "    , ''                                        AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(MB02.SORG), '')              AS SORG" _
            & "    , ''                                        AS SORGNAMES" _
            & "    , ISNULL(MB02.SEQ, 999999)                  AS SORTSEQ" _
            & "    , ISNULL(RTRIM(MB02.SEQ), '')               AS SEQ" _
            & "    , ISNULL(RTRIM(MB01.STAFFCODE), '')         AS STAFFCODE" _
            & "    , ISNULL(RTRIM(MB01.STAFFNAMES), '')        AS STAFFNAMES" _
            & "    , ISNULL(RTRIM(MB01.STAFFKBN), '')          AS STAFFKBN" _
            & "    , ''                                        AS STAFFKBNNAMES" _
            & "    , ISNULL(RTRIM(MB01.CAMPCODE), '')          AS STAFFCAMP" _
            & "    , ''                                        AS STAFFCAMPNAMES" _
            & "    , ISNULL(RTRIM(MB01.MORG), '')              AS MORG" _
            & "    , ''                                        AS MORGNAMES" _
            & "    , ISNULL(RTRIM(MB01.HORG), '')              AS HORG" _
            & "    , ''                                        AS HORGNAMES" _
            & "    , ISNULL(RTRIM(MB02.JSRSTAFFCODE), '')      AS JSRSTAFFCODE" _
            & "    , ISNULL(RTRIM(MB02.DELFLG), '')            AS DELFLG" _
            & "    , ''                                        AS DELFLGNAMES" _
            & " FROM" _
            & "    MB001_STAFF MB01" _
            & "    INNER JOIN M0002_ORG M002" _
            & "        ON  M002.CAMPCODE  = MB01.CAMPCODE" _
            & "        AND M002.ORGCODE   = MB01.HORG" _
            & "        AND M002.ORGLEVEL IN ('00010', '00001')" _
            & "        AND M002.STYMD    <= @P3" _
            & "        AND M002.ENDYMD   >= @P3" _
            & "        AND M002.DELFLG   <> @P5" _
            & "    LEFT JOIN MB002_STAFFORG MB02" _
            & "        ON  MB02.CAMPCODE  = @P1" _
            & "        AND MB02.STAFFCODE = MB01.STAFFCODE" _
            & "        AND MB02.SORG      = @P2" _
            & "        AND MB02.DELFLG   <> @P5" _
            & " WHERE" _
            & "    MB01.STYMD            = (" _
            & "        SELECT" _
            & "            MAX(STYMD)" _
            & "        FROM" _
            & "            MB001_STAFF" _
            & "        WHERE" _
            & "            CAMPCODE      = MB01.CAMPCODE" _
            & "            AND STAFFCODE = MB01.STAFFCODE" _
            & "            AND STYMD    <= @P3" _
            & "            AND ENDYMD   >= @P4" _
            & "            AND DELFLG   <> @P5" _
            & "    )" _
            & "    AND MB01.ENDYMD      >= @P3" _
            & "    AND MB01.DELFLG      <> @P5"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '配属部署
        If Not String.IsNullOrEmpty(work.WF_SEL_HORG.Text) Then
            SQLStr &= String.Format("    AND MB01.HORG        = '{0}'", work.WF_SEL_HORG.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    MB01.CAMPCODE" _
            & "    , MB01.STAFFCODE"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '作業部署
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '現在日付
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '前月初日
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         '削除フラグ

                Dim WW_DATE_END As Date = Date.Now
                Dim WW_DATE_ST As Date = Convert.ToDateTime(WW_DATE_END.AddMonths(-1).ToString("yyyy/MM") & "/01")

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = work.WF_SEL_SORG.Text
                PARA3.Value = WW_DATE_END
                PARA4.Value = WW_DATE_ST
                PARA5.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        MB0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    MB0002tbl.Load(SQLdr)
                End Using

                '○ テーブル並び替え
                CS0026TBLSORT.TABLE = MB0002tbl

                '○ 条件画面で自社従業員を選択していた場合結果を絞る
                If work.WF_SEL_SELECT.Text = "0" Then
                    CS0026TBLSORT.FILTER = "STAFFCAMP = '" & work.WF_SEL_CAMPCODE.Text & "'"
                End If
                CS0026TBLSORT.SORTING = "SORTSEQ, STAFFCAMP, MORG, HORG, STAFFCODE"
                CS0026TBLSORT.sort(MB0002tbl)

                Dim i As Integer = 0
                For Each MB0002row As DataRow In MB0002tbl.Rows
                    i += 1
                    MB0002row("LINECNT") = i

                    MB0002row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                    MB0002row("SORG") = work.WF_SEL_SORG.Text

                    '名称取得
                    CODENAME_get("CAMPCODE", MB0002row("CAMPCODE"), MB0002row("CAMPNAMES"), WW_DUMMY)                                   '会社コード
                    CODENAME_get("SORG", MB0002row("SORG"), MB0002row("SORGNAMES"), WW_DUMMY)                                           '作業部署
                    CODENAME_get("STAFFKBN", MB0002row("STAFFKBN"), MB0002row("STAFFKBNNAMES"), WW_DUMMY, MB0002row("STAFFCAMP"))       '職務区分
                    CODENAME_get("CAMPALL", MB0002row("STAFFCAMP"), MB0002row("STAFFCAMPNAMES"), WW_DUMMY)                              '従業員会社
                    CODENAME_get("ORG", MB0002row("MORG"), MB0002row("MORGNAMES"), WW_DUMMY, MB0002row("STAFFCAMP"))                    '管理部署
                    CODENAME_get("ORG", MB0002row("HORG"), MB0002row("HORGNAMES"), WW_DUMMY, MB0002row("STAFFCAMP"))                    '配属部署
                    CODENAME_get("DELFLG", MB0002row("DELFLG"), MB0002row("DELFLGNAMES"), WW_DUMMY)                                     '削除フラグ
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB002_STAFFORG SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MB002_STAFFORG Select"
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
        For Each MB0002row As DataRow In MB0002tbl.Rows
            If MB0002row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                MB0002row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(MB0002tbl)

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

        WF_SELCAMPCODE.Focus()

    End Sub


    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.eraseCharToIgnore(WF_SELCAMPCODE.Text)
        Master.eraseCharToIgnore(WF_SELMORG.Text)

        '○ 名称取得
        CODENAME_get("CAMPALL", WF_SELCAMPCODE.Text, WF_SELCAMPCODE_TEXT.Text, WW_DUMMY)        '従業員会社
        CODENAME_get("ORG", WF_SELMORG.Text, WF_SELMORG_TEXT.Text, WW_DUMMY)                    '管理部署

        '○ 絞り込み操作(GridView明細Hidden設定)
        For Each MB0002row As DataRow In MB0002tbl.Rows

            '一度非表示にする
            MB0002row("HIDDEN") = 1

            Dim WW_HANTEI As Boolean = True

            '会社コードよる絞込判定
            If WF_SELCAMPCODE.Text <> "" AndAlso
                WF_SELCAMPCODE.Text <> MB0002row("STAFFCAMP") Then
                WW_HANTEI = False
            End If

            '管理部署による絞込判定
            If WF_SELMORG.Text <> "" AndAlso
                WF_SELMORG.Text <> MB0002row("MORG") Then
                WW_HANTEI = False
            End If

            '画面(GridView)のHIDDENに結果格納
            If WW_HANTEI Then
                MB0002row("HIDDEN") = 0
            End If
        Next

        '○ 画面先頭を表示
        WF_GridPosition.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(MB0002tbl)

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ DetailBoxをtblへ退避
        DetailBoxToMB0002tbl()

        '○ 項目チェック
        TableCheck(WW_ERR_SW)

        If isNormal(WW_ERR_SW) Then
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                '従業員作業部署マスタ登録更新
                UpdateStaffOrgMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MB0002tbl)

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMB0002tbl()

        For i As Integer = 0 To MB0002tbl.Rows.Count - 1
            'SEQ
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) AndAlso
                MB0002tbl.Rows(i)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) Then
                MB0002tbl.Rows(i)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1)))
                MB0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MB0002tbl.Rows(i)("SEQ"))

            'JSR従業員コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "JSRSTAFFCODE" & (i + 1))) AndAlso
                MB0002tbl.Rows(i)("JSRSTAFFCODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRSTAFFCODE" & (i + 1))) Then
                MB0002tbl.Rows(i)("JSRSTAFFCODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRSTAFFCODE" & (i + 1)))
                MB0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MB0002tbl.Rows(i)("JSRSTAFFCODE"))

            '削除
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DELFLG" & (i + 1))) AndAlso
                MB0002tbl.Rows(i)("DELFLG") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DELFLG" & (i + 1))) Then
                MB0002tbl.Rows(i)("DELFLG") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DELFLG" & (i + 1)))
                CODENAME_get("DELFLG", MB0002tbl.Rows(i)("DELFLG"), MB0002tbl.Rows(i)("DELFLGNAMES"), WW_DUMMY)
                MB0002tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MB0002tbl.Rows(i)("DELFLG"))
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
        For Each MB0002row As DataRow In MB0002tbl.Rows

            '変更していない明細は飛ばす
            If MB0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                Continue For
            End If

            WW_LINE_ERR = ""

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MB0002row("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MB0002row("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '作業部署
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SORG", MB0002row("SORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("SORG", MB0002row("SORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(作業部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '権限チェック
                CS0025AUTHORget.USERID = CS0050SESSION.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                CS0025AUTHORget.CODE = MB0002row("SORG")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(ユーザ部署更新権限なし)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit Sub
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(作業部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'SEQ
            WW_TEXT = MB0002row("SEQ")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SEQ", MB0002row("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" AndAlso MB0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    MB0002row("SEQ") = ""
                Else
                    Try
                        MB0002row("SEQ") = Format(CInt(MB0002row("SEQ")), "#0")
                    Catch ex As Exception
                        MB0002row("SEQ") = "0"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JSR従業員コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "JSRSTAFFCODE", MB0002row("JSRSTAFFCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JSR従業員コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", MB0002row("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", MB0002row("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR <> "" Then
                MB0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 従業員作業部署マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateStaffOrgMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        MB002_STAFFORG" _
            & "    WHERE" _
            & "        CAMPCODE      = @P1" _
            & "        AND STAFFCODE = @P2" _
            & "        AND SORG      = @P3 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE MB002_STAFFORG" _
            & "    SET" _
            & "        SEQ          = @P4    , JSRSTAFFCODE = @P5" _
            & "        , DELFLG     = @P6    , UPDYMD       = @P8" _
            & "        , UPDUSER    = @P9    , UPDTERMID    = @P10" _
            & "        , RECEIVEYMD = @P11" _
            & "    WHERE" _
            & "        CAMPCODE      = @P1" _
            & "        AND STAFFCODE = @P2" _
            & "        AND SORG      = @P3 ;" _
            & " IF (@@FETCH_STATUS <> 0) " _
            & "    INSERT INTO MB002_STAFFORG" _
            & "        (CAMPCODE         , STAFFCODE" _
            & "        , SORG            , SEQ" _
            & "        , JSRSTAFFCODE    , DELFLG" _
            & "        , INITYMD         , UPDYMD" _
            & "        , UPDUSER         , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1     , @P2" _
            & "        , @P3    , @P4" _
            & "        , @P5    , @P6" _
            & "        , @P7    , @P8" _
            & "        , @P9    , @P10" _
            & "        , @P11) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , STAFFCODE" _
            & "    , SORG" _
            & "    , SEQ" _
            & "    , JSRSTAFFCODE" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    MB002_STAFFORG" _
            & " WHERE" _
            & "    CAMPCODE      = @P1" _
            & "    And STAFFCODE = @P2" _
            & "    And SORG      = @P3"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            '従業員コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)            '作業部署
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Int)                     '表示順番
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)            'JSR従業員コード
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)             '削除フラグ
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.DateTime)                '登録年月日
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.DateTime)                '更新年月日
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)            '更新ユーザーID
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '従業員コード
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        '作業部署

                For Each MB0002row As DataRow In MB0002tbl.Rows
                    If Trim(MB0002row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING Then

                        '新規分で削除のレコードは作成しない
                        If MB0002row("TIMSTP") = 0 AndAlso MB0002row("DELFLG") = C_DELETE_FLG.DELETE Then
                            MB0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Continue For
                        End If

                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = MB0002row("CAMPCODE")
                        PARA2.Value = MB0002row("STAFFCODE")
                        PARA3.Value = MB0002row("SORG")
                        PARA4.Value = MB0002row("SEQ")
                        PARA5.Value = MB0002row("JSRSTAFFCODE")
                        PARA6.Value = MB0002row("DELFLG")
                        PARA7.Value = WW_DATENOW
                        PARA8.Value = WW_DATENOW
                        PARA9.Value = Master.USERID
                        PARA10.Value = Master.USERTERMID
                        PARA11.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        MB0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = MB0002row("CAMPCODE")
                        JPARA2.Value = MB0002row("STAFFCODE")
                        JPARA3.Value = MB0002row("SORG")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(MB0002UPDtbl) Then
                                MB0002UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    MB0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            MB0002UPDtbl.Clear()
                            MB0002UPDtbl.Load(SQLdr)
                        End Using

                        For Each MB0002UPDrow As DataRow In MB0002UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "MB002_STAFFORG"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MB0002UPDrow
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
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB002_STAFFORG UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                                 'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MB002_STAFFORG UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                     'ログ出力
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
        CS0030REPORT.TBLDATA = MB0002tbl                        'データ参照  Table
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
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

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
        Master.CreateEmptyTable(MB0002INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows

            Dim MB0002INProw As DataRow = MB0002INPtbl.NewRow

            '○ 初期クリア
            For Each MB0002INPcol As DataColumn In MB0002INPtbl.Columns
                If IsDBNull(MB0002INProw.Item(MB0002INPcol)) OrElse IsNothing(MB0002INProw.Item(MB0002INPcol)) Then
                    Select Case MB0002INPcol.ColumnName
                        Case "LINECNT"
                            MB0002INProw.Item(MB0002INPcol) = 0
                        Case "OPERATION"
                            MB0002INProw.Item(MB0002INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MB0002INProw.Item(MB0002INPcol) = 0
                        Case "SELECT"
                            MB0002INProw.Item(MB0002INPcol) = 1
                        Case "HIDDEN"
                            MB0002INProw.Item(MB0002INPcol) = 0
                        Case "SORTSEQ"
                            MB0002INProw.Item(MB0002INPcol) = 0
                        Case Else
                            MB0002INProw.Item(MB0002INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("STAFFCAMP") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                For Each MB0002row As DataRow In MB0002tbl.Rows
                    If XLSTBLrow("STAFFCAMP") = MB0002row("STAFFCAMP") AndAlso
                        XLSTBLrow("STAFFCODE") = MB0002row("STAFFCODE") Then
                        MB0002INProw.ItemArray = MB0002row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MB0002INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '作業部署
            If WW_COLUMNS.IndexOf("SORG") >= 0 Then
                MB0002INProw("SORG") = XLSTBLrow("SORG")
            End If

            '従業員
            If WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                MB0002INProw("STAFFCODE") = XLSTBLrow("STAFFCODE")
            End If

            '従業員会社コード
            If WW_COLUMNS.IndexOf("STAFFCAMP") >= 0 Then
                MB0002INProw("STAFFCAMP") = XLSTBLrow("STAFFCAMP")
            End If

            'SEQ
            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                MB0002INProw("SEQ") = XLSTBLrow("SEQ")
            End If

            'JSR従業員コード
            If WW_COLUMNS.IndexOf("JSRSTAFFCODE") >= 0 Then
                MB0002INProw("JSRSTAFFCODE") = XLSTBLrow("JSRSTAFFCODE")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MB0002INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '名称取得
            CODENAME_get("DELFLG", MB0002INProw("DELFLG"), MB0002INProw("DELFLGNAMES"), WW_DUMMY)       '削除

            MB0002INPtbl.Rows.Add(MB0002INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        MB0002tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(MB0002tbl)

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
        For Each MB0002INProw As DataRow In MB0002INPtbl.Rows

            WW_LINE_ERR = ""

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MB0002INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MB0002INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_CAMPCODE.Text <> MB0002INProw("CAMPCODE") Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "検索条件の会社コードと一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '作業部署
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SORG", MB0002INProw("SORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("SORG", MB0002INProw("SORG"), MB0002INProw("SORGNAMES"), WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(作業部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_SORG.Text <> MB0002INProw("SORG") Then
                    WW_CheckMES1 = "・更新できないレコード(作業部署エラー)です。"
                    WW_CheckMES2 = "検索条件の作業部署と一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(作業部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '従業員会社
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MB0002INProw("STAFFCAMP"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '自社従業員のみ選択している場合は他社の会社コードの場合エラーとする
                If work.WF_SEL_SELECT.Text = "0" AndAlso
                    work.WF_SEL_CAMPCODE.Text <> MB0002INProw("STAFFCAMP") Then
                    WW_CheckMES1 = "・更新できないレコード(従業員会社コードエラー)です。"
                    WW_CheckMES2 = "自社従業員のみ登録可能です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '存在チェック
                CODENAME_get("CAMPALL", MB0002INProw("STAFFCAMP"), MB0002INProw("STAFFCAMPNAMES"), WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(従業員会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(従業員会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '従業員コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", MB0002INProw("STAFFCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("STAFFCODE", MB0002INProw("STAFFCODE"), MB0002INProw("STAFFNAMES"), WW_RTN_SW, MB0002INProw("STAFFCAMP"))
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'SEQ
            If MB0002INProw("DELFLG") <> C_DELETE_FLG.DELETE Then
                WW_TEXT = MB0002INProw("SEQ")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SEQ", MB0002INProw("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" Then
                        MB0002INProw("SEQ") = ""
                    Else
                        Try
                            MB0002INProw("SEQ") = Format(CInt(MB0002INProw("SEQ")), "#0")
                        Catch ex As Exception
                            MB0002INProw("SEQ") = "0"
                        End Try
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'JSR従業員コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "JSRSTAFFCODE", MB0002INProw("JSRSTAFFCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JSR従業員コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除
            WW_TEXT = MB0002INProw("DELFLG")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", MB0002INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MB0002INProw("DELFLG") = ""
                Else
                    '存在チェック
                    CODENAME_get("DELFLG", MB0002INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'その他名称取得
            '職務区分
            CODENAME_get("STAFFKBN", MB0002INProw("STAFFKBN"), WW_TEXT, WW_RTN_SW, MB0002INProw("STAFFCAMP"))
            If isNormal(WW_RTN_SW) Then
                MB0002INProw("STAFFKBNNAMES") = WW_TEXT
            End If

            '管理部署
            CODENAME_get("ORG", MB0002INProw("MORG"), WW_TEXT, WW_RTN_SW, MB0002INProw("STAFFCAMP"))
            If isNormal(WW_RTN_SW) Then
                MB0002INProw("MORGNAMES") = WW_TEXT
            End If

            '配属部署
            CODENAME_get("ORG", MB0002INProw("HORG"), WW_TEXT, WW_RTN_SW, MB0002INProw("STAFFCAMP"))
            If isNormal(WW_RTN_SW) Then
                MB0002INProw("HORGNAMES") = WW_TEXT
            End If

            If WW_LINE_ERR = "" Then
                If MB0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MB0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MB0002INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' MB0002tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MB0002tbl_UPD()

        '○ 追加変更判定
        For Each MB0002INProw As DataRow In MB0002INPtbl.Rows

            'エラーレコード読み飛ばし
            If MB0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            MB0002INProw("OPERATION") = "Insert"

            'KEY項目が等しい
            For Each MB0002row As DataRow In MB0002tbl.Rows
                If MB0002row("CAMPCODE") = MB0002INProw("CAMPCODE") AndAlso
                    MB0002row("SORG") = MB0002INProw("SORG") AndAlso
                    MB0002row("STAFFCAMP") = MB0002INProw("STAFFCAMP") AndAlso
                    MB0002row("STAFFCODE") = MB0002INProw("STAFFCODE") Then

                    '変更無は操作無
                    If MB0002row("SEQ") = MB0002INProw("SEQ") AndAlso
                        MB0002row("JSRSTAFFCODE") = MB0002INProw("JSRSTAFFCODE") AndAlso
                        MB0002row("DELFLG") = MB0002INProw("DELFLG") Then
                        MB0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    MB0002INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each MB0002INProw As DataRow In MB0002INPtbl.Rows
            Select Case MB0002INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(MB0002INProw)
                Case "Insert"
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="MB0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef MB0002INProw As DataRow)

        For Each MB0002row As DataRow In MB0002tbl.Rows

            '同一レコード
            If MB0002INProw("CAMPCODE") = MB0002row("CAMPCODE") AndAlso
                MB0002INProw("SORG") = MB0002row("SORG") AndAlso
                MB0002INProw("STAFFCAMP") = MB0002row("STAFFCAMP") AndAlso
                MB0002INProw("STAFFCODE") = MB0002row("STAFFCODE") Then

                '画面入力テーブル項目設定
                MB0002INProw("LINECNT") = MB0002row("LINECNT")
                MB0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MB0002INProw("TIMSTP") = MB0002row("TIMSTP")
                MB0002INProw("SELECT") = 1
                MB0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                MB0002row.ItemArray = MB0002INProw.ItemArray
                Exit For
            End If
        Next

    End Sub


    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()

        Dim WW_LINECNT As Integer = 0

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_SelectedIndex.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○ 変更チェック
        'SEQ
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) AndAlso
            MB0002tbl.Rows(WW_LINECNT)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) Then
            MB0002tbl.Rows(WW_LINECNT)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value))
            MB0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        'JSR従業員コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "JSRSTAFFCODE" & WF_SelectedIndex.Value)) AndAlso
            MB0002tbl.Rows(WW_LINECNT)("JSRSTAFFCODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRSTAFFCODE" & WF_SelectedIndex.Value)) Then
            MB0002tbl.Rows(WW_LINECNT)("JSRSTAFFCODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRSTAFFCODE" & WF_SelectedIndex.Value))
            MB0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '削除
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DELFLG" & WF_SelectedIndex.Value)) AndAlso
            MB0002tbl.Rows(WW_LINECNT)("DELFLG") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DELFLG" & WF_SelectedIndex.Value)) Then
            MB0002tbl.Rows(WW_LINECNT)("DELFLG") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DELFLG" & WF_SelectedIndex.Value))
            CODENAME_get("DELFLG", MB0002tbl.Rows(WW_LINECNT)("DELFLG"), MB0002tbl.Rows(WW_LINECNT)("DELFLGNAMES"), WW_DUMMY)
            MB0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MB0002tbl)

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
                    Case "WF_SELCAMPCODE"       '会社コード
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                    Case "WF_SELMORG"           '管理部署
                        prmData = work.CreateMORGParam(work.WF_SEL_CAMPCODE.Text)
                    Case "DELFLG"               '削除
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_DELFLG
                End Select

                .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .activeListBox()
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
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValue = leftview.getActiveValue(0)
            WW_SelectText = leftview.getActiveValue(1)
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
            Case "WF_SELCAMPCODE"       '会社コード
                WF_SELCAMPCODE.Text = WW_SelectValue
                WF_SELCAMPCODE_TEXT.Text = WW_SelectText
                WF_SELCAMPCODE.Focus()

            Case "WF_SELMORG"           '管理部署
                WF_SELMORG.Text = WW_SelectValue
                WF_SELMORG_TEXT.Text = WW_SelectText
                WF_SELMORG.Focus()

            Case "DELFLG"               '削除
                If MB0002tbl.Rows(WW_LINECNT)("DELFLG") <> WW_SelectValue Then
                    MB0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MB0002tbl.Rows(WW_LINECNT)("DELFLG") = WW_SelectValue
                MB0002tbl.Rows(WW_LINECNT)("DELFLGNAMES") = WW_SelectText
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(MB0002tbl)

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
            Case "WF_SELCAMPCODE"       '会社コード
                WF_SELCAMPCODE.Focus()
            Case "WF_SELMORG"           '管理部署
                WF_SELMORG.Focus()
            Case "DELFLG"               '削除
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
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="MB0002row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal MB0002row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(MB0002row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社       =" & MB0002row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 作業部署   =" & MB0002row("SORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> SEQ        =" & MB0002row("SEQ") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 従業員会社 =" & MB0002row("STAFFCAMP") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 従業員     =" & MB0002row("STAFFCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 配属部署   =" & MB0002row("HORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除       =" & MB0002row("DELFLG")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <param name="I_COMP"></param>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal I_COMP As String = "")

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Dim WW_CAMPCODE As String = work.WF_SEL_CAMPCODE.Text
        If I_COMP <> "" Then
            WW_CAMPCODE = I_COMP
        End If

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = WW_CAMPCODE

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "CAMPALL"          '会社コード(全会社)
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SORG"             '作業部署
                    prmData = work.CreateSORGParam(WW_CAMPCODE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"         '職務区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFKBN, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"              '部署
                    prmData = work.CreateORGParam(WW_CAMPCODE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFCODE"        '従業員コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

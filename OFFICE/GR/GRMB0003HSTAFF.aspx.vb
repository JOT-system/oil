Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 乗務員休日予定登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMB0003HSTAFF
    Inherits Page

    '○ 検索結果格納Table
    Private MB0003tbl As DataTable                          '一覧格納用テーブル
    Private MB0003INPtbl As DataTable                       'チェック用テーブル
    Private MB0003UPDtbl As DataTable                       '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45        '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10         'マウススクロール時稼働行数
    Private Const CONST_WORK As String = "0"                '出社
    Private Const CONST_REST As String = "1"                '休み

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
                    If Not Master.RecoverTable(MB0003tbl) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
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
            If Not IsNothing(MB0003tbl) Then
                MB0003tbl.Clear()
                MB0003tbl.Dispose()
                MB0003tbl = Nothing
            End If

            If Not IsNothing(MB0003INPtbl) Then
                MB0003INPtbl.Clear()
                MB0003INPtbl.Dispose()
                MB0003INPtbl = Nothing
            End If

            If Not IsNothing(MB0003UPDtbl) Then
                MB0003UPDtbl.Clear()
                MB0003UPDtbl.Dispose()
                MB0003UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRMB0003WRKINC.MAPID

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MB0003S Then
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
        Master.SaveTable(MB0003tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(MB0003tbl)

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

        If IsNothing(MB0003tbl) Then
            MB0003tbl = New DataTable
        End If

        If MB0003tbl.Columns.Count <> 0 Then
            MB0003tbl.Columns.Clear()
        End If

        MB0003tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '           従業員マスタに一致する従業員作業部署マスタから表示順を取得
        '           固定値マスタから職務区分を取得
        '　注意事項　日付について
        '　　権限判断はすべてDateNow。グループコード、名称取得は全てDateNow。表追加時の①はDateNow。
        '　　但し、表追加時の②および③は、TBL入力有効期限。

        Dim SQLStr As String =
              " SELECT" _
            & "    0                                                   AS LINECNT" _
            & "    , ''                                                AS OPERATION" _
            & "    , CAST(ISNULL(MB03.UPDTIMSTP, 0) AS bigint)         AS TIMSTP" _
            & "    , 1                                                 AS 'SELECT'" _
            & "    , 0                                                 AS HIDDEN" _
            & "    , 'I'                                               AS TITLEKBN" _
            & "    , ISNULL(RTRIM(MB01.CAMPCODE), '')                  AS CAMPCODE" _
            & "    , ''                                                AS CAMPNAMES" _
            & "    , ISNULL(FORMAT(MB05.WORKINGYMD, 'yyyy/MM/dd'), '') AS YMD" _
            & "    , ISNULL(RTRIM(MB05.WORKINGKBN), '')                AS WORKINGKBN" _
            & "    , ISNULL(RTRIM(MB02.SORG), '')                      AS SORG" _
            & "    , ''                                                AS SORGNAMES" _
            & "    , ISNULL(MB02.SEQ, 999999)                          AS SEQ" _
            & "    , ISNULL(RTRIM(MB01.STAFFCODE), '')                 AS STAFFCODE" _
            & "    , ''                                                AS STAFFNAMES" _
            & "    , ISNULL(RTRIM(MB01.STAFFKBN), '')                  AS STAFFKBN" _
            & "    , ''                                                AS STAFFKBNNAMES" _
            & "    , ISNULL(RTRIM(MB03.HOLIDAYKBN), '')                AS HOLIDAYKBN" _
            & "    , ''                                                AS STATUS" _
            & " FROM" _
            & "    MB005_CALENDAR MB05" _
            & "    INNER JOIN MB001_STAFF MB01" _
            & "        ON  MB01.CAMPCODE  = @P1" _
            & "        AND MB01.STYMD    <= MB05.WORKINGYMD" _
            & "        AND MB01.ENDYMD   >= MB05.WORKINGYMD" _
            & "        AND MB01.DELFLG   <> @P5" _
            & "    INNER JOIN MC001_FIXVALUE MC01" _
            & "        ON  MC01.CAMPCODE  = @P1" _
            & "        AND MC01.CLASS     = 'STAFFKBN'" _
            & "        AND MC01.KEYCODE   = MB01.STAFFKBN" _
            & "        AND MC01.STYMD    <= MB05.WORKINGYMD" _
            & "        AND MC01.ENDYMD   >= MB05.WORKINGYMD" _
            & "        AND MC01.VALUE2    = '1'" _
            & "        AND MC01.DELFLG   <> @P5" _
            & "    INNER JOIN MB002_STAFFORG MB02" _
            & "        ON  MB02.CAMPCODE  = @P1" _
            & "        AND MB02.STAFFCODE = MB01.STAFFCODE" _
            & "        AND MB02.SORG      = @P4" _
            & "        AND MB02.DELFLG   <> @P5" _
            & "    LEFT JOIN MB003_HSTAFF MB03" _
            & "        ON MB03.CAMPCODE   = @P1" _
            & "        AND MB03.STAFFCODE = MB01.STAFFCODE" _
            & "        AND MB03.YMD       = MB05.WORKINGYMD" _
            & "        AND MB03.DELFLG   <> @P5" _
            & " WHERE" _
            & "    MB05.CAMPCODE        = @P1" _
            & "    AND MB05.WORKINGYMD >= @P2" _
            & "    AND MB05.WORKINGYMD <= @P3" _
            & "    AND MB05.DELFLG     <> @P5"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '職務区分
        If Not String.IsNullOrEmpty(work.WF_SEL_STAFFKBN.Text) Then
            SQLStr &= String.Format("    AND MB01.STAFFKBN    = '{0}'", work.WF_SEL_STAFFKBN.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    YMD" _
            & "    , SORG" _
            & "    , SEQ" _
            & "    , STAFFCODE"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '月初
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '月末
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)        '作業部署
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         '削除フラグ

                Dim WW_DATE_ST As Date
                Dim WW_DATE_END As Date

                Try
                    Date.TryParse(work.WF_SEL_YYMM.Text & "/01", WW_DATE_ST)
                Catch ex As Exception
                    WW_DATE_ST = Date.Now.ToString("yyyy/MM") & "/01"
                End Try

                WW_DATE_END = WW_DATE_ST.AddMonths(1).AddDays(-1)

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = WW_DATE_ST
                PARA3.Value = WW_DATE_END
                PARA4.Value = work.WF_SEL_SORG.Text
                PARA5.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        MB0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    MB0003tbl.Load(SQLdr)
                End Using

                Dim WW_SAVEKEY As String = ""
                For Each MB0003row As DataRow In MB0003tbl.Rows
                    'ヘッダーを設定
                    Dim WW_KEY As String = MB0003row("YMD") & "," & MB0003row("SORG")
                    If WW_SAVEKEY <> WW_KEY Then
                        MB0003row("TITLEKBN") = "H"
                        WW_SAVEKEY = WW_KEY
                    End If

                    '休日区分設定
                    If String.IsNullOrEmpty(MB0003row("HOLIDAYKBN")) Then
                        MB0003row("STATUS") = "未登録"

                        If MB0003row("WORKINGKBN") = "0" Then
                            MB0003row("HOLIDAYKBN") = CONST_WORK        '0:出社
                        Else
                            MB0003row("HOLIDAYKBN") = CONST_REST        '1:休み
                        End If
                    End If

                    '名称取得
                    CODENAME_get("CAMPCODE", MB0003row("CAMPCODE"), MB0003row("CAMPNAMES"), WW_DUMMY)           '会社コード
                    CODENAME_get("SORG", MB0003row("SORG"), MB0003row("SORGNAMES"), WW_DUMMY)                   '作業部署
                    CODENAME_get("STAFFCODE", MB0003row("STAFFCODE"), MB0003row("STAFFNAMES"), WW_DUMMY)        '従業員
                    CODENAME_get("STAFFKBN", MB0003row("STAFFKBN"), MB0003row("STAFFKBNNAMES"), WW_DUMMY)       '職務区分
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB003_HSTAFF SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MB003_HSTAFF Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データソート
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TABLE = MB0003tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = "TITLEKBN = 'H'"
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            MB0003tbl = CS0026TBLSORT.TABLE
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
        For Each MB0003row As DataRow In MB0003tbl.Rows
            If MB0003row("HIDDEN") = 0 AndAlso MB0003row("TITLEKBN") = "H" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                MB0003row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(MB0003tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H' and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

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
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '乗務員マスタ更新
            UpdateHStaffMaster(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(MB0003tbl)

        '○ 詳細画面クリア
        DetailBoxClear()

    End Sub

    ''' <summary>
    ''' 乗務員マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateHStaffMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        MB003_HSTAFF" _
            & "    WHERE" _
            & "        CAMPCODE      = @P1" _
            & "        AND STAFFCODE = @P2" _
            & "        AND YMD       = @P3 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE MB003_HSTAFF" _
            & "    SET" _
            & "        HOLIDAYKBN   = @P4" _
            & "        , UPDYMD     = @P11" _
            & "        , UPDUSER    = @P12" _
            & "        , UPDTERMID  = @P13" _
            & "        , RECEIVEYMD = @P14" _
            & "    WHERE" _
            & "        CAMPCODE      = @P1" _
            & "        AND STAFFCODE = @P2" _
            & "        AND YMD       = @P3" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO MB003_HSTAFF" _
            & "        (CAMPCODE      , STAFFCODE" _
            & "        , YMD          , HOLIDAYKBN" _
            & "        , ORVERTIME    , DISTANCE" _
            & "        , UNLOADCNT    , DANGCNT" _
            & "        , DELFLG       , INITYMD" _
            & "        , UPDYMD       , UPDUSER" _
            & "        , UPDTERMID    , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2" _
            & "        , @P3     , @P4" _
            & "        , @P5     , @P6" _
            & "        , @P7     , @P8" _
            & "        , @P9     , @P10" _
            & "        , @P11    , @P12" _
            & "        , @P13    , @P14) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , STAFFCODE" _
            & "    , YMD" _
            & "    , HOLIDAYKBN" _
            & "    , CONVERT(char(8), ORVERTIME) AS ORVERTIME" _
            & "    , DISTANCE" _
            & "    , UNLOADCNT" _
            & "    , DANGCNT" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint)   AS TIMSTP" _
            & " FROM" _
            & "    MB003_HSTAFF" _
            & " WHERE" _
            & "    CAMPCODE      = @P1" _
            & "    AND STAFFCODE = @P2" _
            & "    AND YMD       = @P3"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            '乗務員コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                    '年月日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1)             '休暇区分
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Time)                    '残業時間
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Int)                     '走行距離
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Int)                     '荷卸回数
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Int)                     '危険物回数
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 1)             '削除フラグ
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)              '登録年月日
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)              '更新年月日
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)          '更新ユーザＩＤ
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '乗務員コード
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.Date)                '年月日

                For Each MB0003row As DataRow In MB0003tbl.Rows
                    If Trim(MB0003row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(MB0003row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As Date = Date.Now

                        'ＤＢ更新
                        PARA1.Value = MB0003row("CAMPCODE")
                        PARA2.Value = MB0003row("STAFFCODE")
                        PARA3.Value = MB0003row("YMD")
                        PARA4.Value = MB0003row("HOLIDAYKBN")
                        PARA5.Value = "00:00"
                        PARA6.Value = 0
                        PARA7.Value = 0
                        PARA8.Value = 0
                        PARA9.Value = C_DELETE_FLG.ALIVE
                        PARA10.Value = WW_DATENOW
                        PARA11.Value = WW_DATENOW
                        PARA12.Value = Master.USERID
                        PARA13.Value = Master.USERTERMID
                        PARA14.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        MB0003row("STATUS") = ""

                        '更新ジャーナル出力
                        JPARA1.Value = MB0003row("CAMPCODE")
                        JPARA2.Value = MB0003row("STAFFCODE")
                        JPARA3.Value = MB0003row("YMD")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(MB0003UPDtbl) Then
                                MB0003UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    MB0003UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            MB0003UPDtbl.Clear()
                            MB0003UPDtbl.Load(SQLdr)
                        End Using

                        For Each MB0003UPDrow As DataRow In MB0003UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "MB003_HSTAFF"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MB0003UPDrow
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
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB003_HSTAFF UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MB003_HSTAFF UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = MB0003tbl                        'データ参照  Table
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
        CS0030REPORT.TBLDATA = MB0003tbl                        'データ参照Table
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

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(MB0003tbl)
        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H'"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

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
        Dim WW_TEXT As String = ""
        Dim WW_TABLE As New DataTable

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try

        For i As Integer = 0 To MB0003tbl.Rows.Count - 1
            If MB0003tbl.Rows(i)("LINECNT") = WW_LINECNT Then
                WW_LINECNT = i
                Exit For
            End If
        Next

        '○ 選択行
        WF_Sel_LINECNT.Text = MB0003tbl.Rows(WW_LINECNT)("LINECNT")

        '会社コード
        WF_CAMPCODE.Text = MB0003tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        '年月日
        Try
            Dim WW_DATE As Date
            Date.TryParse(MB0003tbl.Rows(WW_LINECNT)("YMD"), WW_DATE)
            WF_YMD.Text = WW_DATE.ToString("yyyy/MM/dd")
            WF_WEEK.Text = "(" & WW_DATE.ToString("ddd") & ")"
        Catch ex As Exception
            WF_YMD.Text = MB0003tbl.Rows(WW_LINECNT)("YMD")
            WF_WEEK.Text = ""
        End Try

        CS0026TBLSORT.TABLE = MB0003tbl
        CS0026TBLSORT.SORTING = "SEQ, STAFFCODE"
        CS0026TBLSORT.FILTER = "YMD = '" & WF_YMD.Text & "'"
        CS0026TBLSORT.sort(WW_TABLE)

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = True
        WF_Repeater.DataSource = WW_TABLE
        WF_Repeater.DataBind()

        '○ 明細作成
        For i As Integer = 0 To WF_Repeater.Items.Count - 1
            '従業員
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_STAFFCODE"), Label).Text = WW_TABLE.Rows(i)("STAFFCODE")
            CODENAME_get("STAFFCODE", WW_TABLE.Rows(i)("STAFFCODE"), WW_TEXT, WW_DUMMY)
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_STAFFCODE_TEXT"), Label).Text = WW_TEXT

            '職務区分
            CODENAME_get("STAFFKBN", WW_TABLE.Rows(i)("STAFFKBN"), WW_TEXT, WW_DUMMY)
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_STAFFKBN_TEXT"), Label).Text = WW_TEXT

            '勤務状況(休日区分)
            If WW_TABLE.Rows(i)("HOLIDAYKBN") = CONST_WORK Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW1"), CheckBox).Checked = True          '出社
            Else
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW2"), CheckBox).Checked = True          '休み
            End If
        Next

        '○ 状態をクリア
        For Each MB0003row As DataRow In MB0003tbl.Rows
            Select Case MB0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case MB0003tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                MB0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                MB0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                MB0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                MB0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                MB0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(MB0003tbl)

        WF_GridDBclick.Text = ""

        WW_TABLE.Clear()
        WW_TABLE.Dispose()
        WW_TABLE = Nothing

    End Sub


    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

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
        Master.CreateEmptyTable(MB0003INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim MB0003INProw As DataRow = MB0003INPtbl.NewRow

            '○ 初期クリア
            For Each MB0003INPcol As DataColumn In MB0003INPtbl.Columns
                If IsDBNull(MB0003INProw.Item(MB0003INPcol)) OrElse IsNothing(MB0003INProw.Item(MB0003INPcol)) Then
                    Select Case MB0003INPcol.ColumnName
                        Case "LINECNT"
                            MB0003INProw.Item(MB0003INPcol) = 0
                        Case "OPERATION"
                            MB0003INProw.Item(MB0003INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MB0003INProw.Item(MB0003INPcol) = 0
                        Case "SELECT"
                            MB0003INProw.Item(MB0003INPcol) = 1
                        Case "HIDDEN"
                            MB0003INProw.Item(MB0003INPcol) = 0
                        Case "SEQ"
                            MB0003INProw.Item(MB0003INPcol) = 0
                        Case Else
                            MB0003INProw.Item(MB0003INPcol) = ""
                    End Select
                End If
            Next

            MB0003INProw("TITLEKBN") = "I"

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("YMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                For Each MB0003row As DataRow In MB0003tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MB0003row("CAMPCODE") AndAlso
                        XLSTBLrow("YMD") = MB0003row("YMD") AndAlso
                        XLSTBLrow("STAFFCODE") = MB0003row("STAFFCODE") Then
                        MB0003INProw.ItemArray = MB0003row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MB0003INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '開始年月日
            If WW_COLUMNS.IndexOf("YMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("YMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        MB0003INProw("YMD") = ""
                    Else
                        MB0003INProw("YMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    MB0003INProw("YMD") = ""
                End Try
            End If

            '従業員コード
            If WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                MB0003INProw("STAFFCODE") = XLSTBLrow("STAFFCODE")
            End If

            '勤務状況
            If WW_COLUMNS.IndexOf("HOLIDAYKBN") >= 0 Then
                MB0003INProw("HOLIDAYKBN") = XLSTBLrow("HOLIDAYKBN")
            End If

            MB0003INPtbl.Rows.Add(MB0003INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        MB0003tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(MB0003tbl)

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
        DetailBoxToMB0003INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            MB0003tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MB0003tbl)

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

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMB0003INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_YMD.Text)               '年月日

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If (String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_YMD.Text)) OrElse
            WF_Repeater.Items.Count = 0 Then
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

        Master.CreateEmptyTable(MB0003INPtbl)

        For Each reitem As RepeaterItem In WF_Repeater.Items
            Dim MB0003INProw As DataRow = MB0003INPtbl.NewRow

            '○ 初期クリア
            For Each MB0003INPcol As DataColumn In MB0003INPtbl.Columns
                If IsDBNull(MB0003INProw.Item(MB0003INPcol)) OrElse IsNothing(MB0003INProw.Item(MB0003INPcol)) Then
                    Select Case MB0003INPcol.ColumnName
                        Case "LINECNT"
                            MB0003INProw.Item(MB0003INPcol) = 0
                        Case "OPERATION"
                            MB0003INProw.Item(MB0003INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MB0003INProw.Item(MB0003INPcol) = 0
                        Case "SELECT"
                            MB0003INProw.Item(MB0003INPcol) = 1
                        Case "HIDDEN"
                            MB0003INProw.Item(MB0003INPcol) = 0
                        Case "SEQ"
                            MB0003INProw.Item(MB0003INPcol) = 0
                        Case Else
                            MB0003INProw.Item(MB0003INPcol) = ""
                    End Select
                End If
            Next

            'LINECNT
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, MB0003INProw("LINECNT"))
            Catch ex As Exception
                MB0003INProw("LINECNT") = 0
            End Try

            MB0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            MB0003INProw("TIMSTP") = 0
            MB0003INProw("SELECT") = 1
            MB0003INProw("HIDDEN") = 0

            MB0003INProw("CAMPCODE") = WF_CAMPCODE.Text         '会社コード
            MB0003INProw("YMD") = WF_YMD.Text                   '年月日

            MB0003INProw("STAFFCODE") = CType(reitem.FindControl("WF_Rep_STAFFCODE"), Label).Text       '従業員

            '勤務状況
            If CType(reitem.FindControl("WF_Rep_SW1"), CheckBox).Checked Then
                MB0003INProw("HOLIDAYKBN") = CONST_WORK         '出社
            Else
                MB0003INProw("HOLIDAYKBN") = CONST_REST         '休み
            End If

            MB0003INProw("WORKINGKBN") = ""             '休日区分
            MB0003INProw("SEQ") = 0                     '表示順
            MB0003INProw("CAMPNAMES") = ""              '会社名称
            MB0003INProw("SORG") = ""                   '作業部署
            MB0003INProw("SORGNAMES") = ""              '作業部署名
            MB0003INProw("STAFFNAMES") = ""             '従業員名
            MB0003INProw("STAFFKBN") = ""               '職務区分
            MB0003INProw("STAFFKBNNAMES") = ""          '職務区分名
            MB0003INProw("STATUS") = ""                 '登録状況

            '○ 名称取得
            CODENAME_get("CAMPCODE", MB0003INProw("CAMPCODE"), MB0003INProw("CAMPNAMES"), WW_DUMMY)             '会社コード
            CODENAME_get("STAFFCODE", MB0003INProw("STAFFCODE"), MB0003INProw("STAFFNAMES"), WW_DUMMY)          '従業員
            CODENAME_get("STAFFKBN", MB0003INProw("STAFFKBN"), MB0003INProw("STAFFKBNNAMES"), WW_DUMMY)         '職務区分

            '○ チェック用テーブルに登録する
            MB0003INPtbl.Rows.Add(MB0003INProw)
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

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each MB0003row As DataRow In MB0003tbl.Rows
            Select Case MB0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(MB0003tbl)

        WF_Sel_LINECNT.Text = ""                            'LINECNT
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_YMD.Text = ""                                    '年月日
        WF_WEEK.Text = ""                                   '曜日

        '○ 詳細画面初期設定
        DetailInitialize()

    End Sub

    ''' <summary>
    ''' 詳細画面-初期設定 (空明細作成 ＆ イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailInitialize()

        Master.CreateEmptyTable(MB0003INPtbl)

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = False
        WF_Repeater.DataSource = MB0003INPtbl
        WF_Repeater.DataBind()

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

        '○ キー重複レコード削除
        Do Until WW_CNT1 > (MB0003INPtbl.Rows.Count - 1)
            WW_CNT2 = WW_CNT1 + 1

            Do Until WW_CNT2 > (MB0003INPtbl.Rows.Count - 1)
                'KEY重複データを探す
                If MB0003INPtbl.Rows(WW_CNT1)("CAMPCODE") = MB0003INPtbl(WW_CNT2)("CAMPCODE") AndAlso
                    MB0003INPtbl.Rows(WW_CNT1)("YMD") = MB0003INPtbl(WW_CNT2)("YMD") AndAlso
                    MB0003INPtbl.Rows(WW_CNT1)("STAFFCODE") = MB0003INPtbl(WW_CNT2)("STAFFCODE") Then
                    MB0003INPtbl.Rows(WW_CNT2).Delete()
                Else
                    WW_CNT2 += 1
                End If
            Loop
            WW_CNT1 += 1
        Loop

        '○ 単項目チェック
        For Each MB0003INProw As DataRow In MB0003INPtbl.Rows

            WW_LINE_ERR = ""

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MB0003INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MB0003INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "YMD", MB0003INProw("YMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(年月日)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '範囲チェック
            Dim WW_DATE As Date
            Dim WW_DATE_ST As Date
            Dim WW_DATE_END As Date
            Try
                Date.TryParse(MB0003INProw("YMD"), WW_DATE)
                Date.TryParse(work.WF_SEL_YYMM.Text & "/01", WW_DATE_ST)
                WW_DATE_END = WW_DATE_ST.AddMonths(1)
                WW_DATE_END = WW_DATE_END.AddDays(-1)
                If WW_DATE_ST > WW_DATE OrElse WW_DATE_END < WW_DATE Then
                    WW_CheckMES1 = "・更新できないレコード(日付が範囲外)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Catch ex As Exception
            End Try

            '従業員
            WW_TEXT = MB0003INProw("STAFFCODE")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", MB0003INProw("STAFFCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MB0003INProw("STAFFCODE") = ""
                Else
                    CODENAME_get("STAFFCODE", MB0003INProw("STAFFCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(乗務員)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(乗務員)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '勤務状況
            If MB0003INProw("HOLIDAYKBN") = CONST_WORK OrElse MB0003INProw("HOLIDAYKBN") = CONST_REST Then
            Else
                WW_CheckMES1 = "・更新できないレコード(休日区分)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If MB0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MB0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MB0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="MB0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal MB0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(MB0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社     =" & MB0003row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 年月日   =" & MB0003row("YMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 乗務員   =" & MB0003row("STAFFCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 勤務状況 =" & MB0003row("HOLIDAYKBN")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' MB0003tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MB0003tbl_UPD()

        '○ 画面状態設定
        For Each MB0003row As DataRow In MB0003tbl.Rows
            Select Case MB0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each MB0003INProw As DataRow In MB0003INPtbl.Rows

            'エラーレコード読み飛ばし
            If MB0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            'KEY項目が等しい
            For Each MB0003row As DataRow In MB0003tbl.Rows
                If MB0003row("CAMPCODE") = MB0003INProw("CAMPCODE") AndAlso
                    MB0003row("YMD") = MB0003INProw("YMD") AndAlso
                    MB0003row("STAFFCODE") = MB0003INProw("STAFFCODE") Then

                    If MB0003row("HOLIDAYKBN") <> MB0003INProw("HOLIDAYKBN") Then
                        MB0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        MB0003row("HOLIDAYKBN") = MB0003INProw("HOLIDAYKBN")
                    End If

                    Exit For
                End If
            Next
        Next

        '○ タイトル明細ステータス更新
        For Each MB0003INProw As DataRow In MB0003tbl.Rows
            If MB0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            For Each MB0003row As DataRow In MB0003tbl.Rows
                If MB0003row("YMD") = MB0003INProw("YMD") Then
                    MB0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Next
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
                Case "STAFFCODE"        '従業員コード
                    prmData = work.CreateStaffCodeParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SORG"             '作業部署
                    prmData = work.CreateSORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"         '職務区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFKBN, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

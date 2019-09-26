Imports System.Drawing
Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' カレンダー（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMB0005CALENDAR
    Inherits Page

    '○ 検索結果格納Table
    Private MB0005tbl As DataTable                          '一覧格納用テーブル
    Private MB0005INPtbl As DataTable                       'チェック用テーブル
    Private MB0005UPDtbl As DataTable                       '更新用テーブル

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
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
                    If Not Master.RecoverTable(MB0005tbl) Then
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
                        Case "WF_Repeater_Change"       'リピーター変更
                            WF_Repeater_Change()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                    End Select
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
            If Not IsNothing(MB0005tbl) Then
                MB0005tbl.Clear()
                MB0005tbl.Dispose()
                MB0005tbl = Nothing
            End If

            If Not IsNothing(MB0005INPtbl) Then
                MB0005INPtbl.Clear()
                MB0005INPtbl.Dispose()
                MB0005INPtbl = Nothing
            End If

            If Not IsNothing(MB0005UPDtbl) Then
                MB0005UPDtbl.Clear()
                MB0005UPDtbl.Dispose()
                MB0005UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRMB0005WRKINC.MAPID

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.resetindex()

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ 右ボックスへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ Repeater初期設定
        RepeaterInitialize()

        '○ 日付ツリー作成
        TreeViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MB0005S Then
            'Grid情報保存先のファイル名
            Master.createXMLSaveFile()
        End If

        '○ ヘルプボタン非表示
        Master.dispHelp = False

        '○ ファイルドロップ有無
        Master.eventDrop = True

    End Sub

    ''' <summary>
    ''' Repeaterデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub RepeaterInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(MB0005tbl)

        '○ 一覧表示データ作成
        WF_DISP_DATE.Value = work.WF_SEL_STYYMM.Text & "/01"
        MapDisplay(WF_DISP_DATE.Value)

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(MB0005tbl) Then
            MB0005tbl = New DataTable
        End If

        If MB0005tbl.Columns.Count <> 0 Then
            MB0005tbl.Columns.Clear()
        End If

        MB0005tbl.Clear()

        '○ 条件指定(年月)編集
        Dim WW_DATE As Date
        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Try
            Date.TryParse(work.WF_SEL_STYYMM.Text & "/01", WW_DATE_ST)
            Date.TryParse(work.WF_SEL_ENDYYMM.Text & "/01", WW_DATE_END)

            WW_DATE_END = WW_DATE_END.AddMonths(1)
            WW_DATE_END = WW_DATE_END.AddDays(-1)
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End Try

        Try
            '○ テンポラリーテーブル(年月日)の全てを取得する
            Dim SQLstr As String = "CREATE TABLE #myTemp(YMD DATE NOT NULL)"
            Using SQLcmd As New SqlCommand(SQLstr, SQLcon)
                SQLcmd.ExecuteNonQuery()
            End Using

            '○ 年月日追加
            SQLstr = "INSERT INTO #myTemp (YMD) VALUES (@P1)"

            Using SQLcmd As New SqlCommand(SQLstr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.Date)

                WW_DATE = WW_DATE_ST
                Do
                    PARA1.Value = WW_DATE

                    SQLcmd.ExecuteNonQuery()
                    WW_DATE = WW_DATE.AddDays(1)
                Loop Until WW_DATE_END < WW_DATE
            End Using

            '○ カレンダー検索SQL文
            SQLstr =
                  " SELECT" _
                & "    0                                            AS LINECNT" _
                & "    , ''                                         AS OPERATION" _
                & "    , CAST(ISNULL(MB05.UPDTIMSTP, 0) AS bigint)  AS TIMSTP" _
                & "    , ISNULL(RTRIM(MB05.CAMPCODE), '')           AS CAMPCODE" _
                & "    , ''                                         AS CAMPNAMES" _
                & "    , ISNULL(FORMAT(TEMP.YMD, 'yyyy/MM/dd'), '') AS WORKINGYMD" _
                & "    , ''                                         AS WORKINGWEEK" _
                & "    , ISNULL(RTRIM(MB05.WORKINGTEXT), '')        AS WORKINGTEXT" _
                & "    , ISNULL(RTRIM(MB05.WORKINGKBN), '0')        AS WORKINGKBN" _
                & " FROM" _
                & "    #myTemp TEMP" _
                & "    LEFT JOIN MB005_CALENDAR MB05" _
                & "        ON  MB05.CAMPCODE   = @P1" _
                & "        AND MB05.WORKINGYMD = TEMP.YMD" _
                & "        AND MB05.DELFLG    <> @P2" _
                & " ORDER BY" _
                & "    TEMP.YMD"

            Using SQLcmd As New SqlCommand(SQLstr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)         '削除

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        MB0005tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    MB0005tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each MB0005row As DataRow In MB0005tbl.Rows
                    i += 1
                    MB0005row("LINECNT") = i

                    '日付、曜日取得    
                    Try
                        Date.TryParse(MB0005row("WORKINGYMD"), WW_DATE)
                        MB0005row("WORKINGWEEK") = CInt(WW_DATE.DayOfWeek).ToString("0")
                    Catch ex As Exception
                        MB0005row("WORKINGWEEK") = ""
                    End Try

                    '未登録分テキスト設定
                    If MB0005row("TIMSTP") = 0 Then
                        MB0005row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                        MB0005row("WORKINGKBN") = "0"

                        If MB0005row("WORKINGWEEK") = "0" Then
                            MB0005row("WORKINGTEXT") = "法定休日"
                            MB0005row("WORKINGKBN") = "1"
                        End If

                        '祝祭日(固定日)判定
                        If WW_DATE.Month = 1 AndAlso WW_DATE.Day = 1 Then
                            '元日(1月1日)
                            MB0005row("WORKINGTEXT") = "元日"
                            MB0005row("WORKINGKBN") = "2"
                        End If
                        If WW_DATE.Month = 1 AndAlso WW_DATE.Day = 2 Then
                            '法定外(1月2日)
                            MB0005row("WORKINGTEXT") = "法定外休日"
                            MB0005row("WORKINGKBN") = "2"
                        End If
                        If WW_DATE.Month = 1 AndAlso WW_DATE.Day = 3 Then
                            '法定外(1月3日)
                            MB0005row("WORKINGTEXT") = "法定外休日"
                            MB0005row("WORKINGKBN") = "2"
                        End If
                        If WW_DATE.Month = 2 AndAlso WW_DATE.Day = 11 Then
                            '建国記念日(2月11日)
                            MB0005row("WORKINGTEXT") = "建国記念日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Year >= 2020 AndAlso WW_DATE.Month = 2 AndAlso WW_DATE.Day = 23 Then
                            '天皇誕生日(2月23日)
                            MB0005row("WORKINGTEXT") = "天皇誕生日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Month = 4 AndAlso WW_DATE.Day = 29 Then
                            '昭和の日(4月29日)
                            MB0005row("WORKINGTEXT") = "昭和の日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Month = 5 AndAlso WW_DATE.Day = 3 Then
                            '憲法記念日(5月3日)
                            MB0005row("WORKINGTEXT") = "憲法記念日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Month = 5 AndAlso WW_DATE.Day = 4 Then
                            'みどりの日(5月4日)
                            MB0005row("WORKINGTEXT") = "みどりの日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Month = 5 AndAlso WW_DATE.Day = 5 Then
                            'こどもの日(5月5日)
                            MB0005row("WORKINGTEXT") = "こどもの日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Month = 8 AndAlso WW_DATE.Day = 11 Then
                            '山の日(8月11日)
                            MB0005row("WORKINGTEXT") = "山の日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Month = 11 AndAlso WW_DATE.Day = 3 Then
                            '文化の日(11月3日)
                            MB0005row("WORKINGTEXT") = "文化の日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Month = 11 AndAlso WW_DATE.Day = 23 Then
                            '勤労感謝の日(11月23日)
                            MB0005row("WORKINGTEXT") = "勤労感謝の日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Year <= 2018 AndAlso WW_DATE.Month = 12 AndAlso WW_DATE.Day = 23 Then
                            '天皇誕生日(12月23日)
                            MB0005row("WORKINGTEXT") = "天皇誕生日"
                            If MB0005row("WORKINGWEEK") <> "0" Then
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If
                        If WW_DATE.Month = 12 AndAlso WW_DATE.Day = 31 AndAlso MB0005row("WORKINGWEEK") <> "0" Then
                            '法定外(12月31日)
                            MB0005row("WORKINGTEXT") = "法定外休日"
                            MB0005row("WORKINGKBN") = "2"
                        End If

                        '祝祭日(固定日)振替判定
                        If MB0005row("WORKINGWEEK") = "1" Then
                            If WW_DATE.AddDays(-1).Month = 1 AndAlso WW_DATE.AddDays(-1).Day = 1 Then
                                '元日(1月1日)
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.AddDays(-1).Month = 2 AndAlso WW_DATE.AddDays(-1).Day = 11 Then
                                '建国記念日(2月11日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.Year >= 2020 AndAlso WW_DATE.AddDays(-1).Month = 2 AndAlso WW_DATE.AddDays(-1).Day = 23 Then
                                '天皇誕生日(2月23日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.AddDays(-1).Month = 4 AndAlso WW_DATE.AddDays(-1).Day = 29 Then
                                '昭和の日(4月29日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.AddDays(-1).Month = 5 AndAlso WW_DATE.AddDays(-1).Day = 3 Then
                                '憲法記念日(5月3日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.AddDays(-1).Month = 5 AndAlso WW_DATE.AddDays(-1).Day = 4 Then
                                'みどりの日(5月4日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.AddDays(-1).Month = 5 AndAlso WW_DATE.AddDays(-1).Day = 5 Then
                                'こどもの日(5月5日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.AddDays(-1).Month = 8 AndAlso WW_DATE.AddDays(-1).Day = 11 Then
                                '山の日(8月11日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.AddDays(-1).Month = 11 AndAlso WW_DATE.AddDays(-1).Day = 3 Then
                                '文化の日(11月3日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.AddDays(-1).Month = 11 AndAlso WW_DATE.AddDays(-1).Day = 23 Then
                                '勤労感謝の日(11月23日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.Year <= 2018 AndAlso WW_DATE.AddDays(-1).Month = 12 AndAlso WW_DATE.AddDays(-1).Day = 23 Then
                                '天皇誕生日(12月23日)
                                MB0005row("WORKINGTEXT") = "振替休日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If

                        '祝祭日(変動日)判定
                        If MB0005row("WORKINGWEEK") = "1" Then
                            'その日は年間の第何週目か判定
                            Dim WW_WEEKCNT1 As Integer = DatePart("WW", WW_DATE)
                            'その月の1日は年間の第何週目か判定
                            Dim WW_WEEKCNT2 As Integer = DatePart("WW", CDate(WW_DATE.ToString("yyyy/MM") & "/01")) - 1
                            'その日の曜日が1日の曜日よりも前の曜日なら -1。以降の曜日なら 0
                            Dim WW_WEEKCNT3 As Integer = Weekday(WW_DATE) < Weekday(CDate(WW_DATE.ToString("yyyy/MM") & "/01"))
                            '月単位の第何週目か算出
                            Dim WW_WEEKCNT As Integer = WW_WEEKCNT1 - WW_WEEKCNT2 + WW_WEEKCNT3

                            If WW_DATE.Month = 1 AndAlso WW_WEEKCNT = 2 Then
                                '成人の日(1月の第2月曜日)
                                MB0005row("WORKINGTEXT") = "成人の日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.Month = 7 AndAlso WW_WEEKCNT = 3 Then
                                '海の日(7月の第3月曜日)
                                MB0005row("WORKINGTEXT") = "海の日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.Month = 9 AndAlso WW_WEEKCNT = 3 Then
                                '敬老の日(9月の第3月曜日)
                                MB0005row("WORKINGTEXT") = "敬老の日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                            If WW_DATE.Month = 10 AndAlso WW_WEEKCNT = 2 Then
                                '体育の日(10月の第2月曜日)
                                MB0005row("WORKINGTEXT") = "体育の日"
                                MB0005row("WORKINGKBN") = "2"
                            End If
                        End If

                        '春分の日・秋分の日は未サポート
                    End If

                    '名称取得
                    CODENAME_get("CAMPCODE", MB0005row("CAMPCODE"), MB0005row("CAMPNAMES"), WW_DUMMY)       '会社コード
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB005_CALENDAR SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MB005_CALENDAR Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 日付ツリー作成
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub TreeViewInitialize()

        Dim WW_ParentNode As New TreeNode       '親ノード
        Dim WW_Node As New TreeNode             '子ノード

        WF_DATE_TREE.Nodes.Clear()

        '○ 従属ノード設定
        For Each MB0005row As DataRow In MB0005tbl.Rows
            '年追加
            If WW_ParentNode.Value <> Mid(MB0005row("WORKINGYMD"), 1, 4) Then
                WW_Node = New TreeNode
                WW_Node.Text = Mid(MB0005row("WORKINGYMD"), 1, 4) & "年"
                WW_Node.Value = Mid(MB0005row("WORKINGYMD"), 1, 4)
                WF_DATE_TREE.Nodes.Add(WW_Node)
                WW_ParentNode = WW_Node
            End If

            '年月追加
            If WW_Node.Value <> Mid(MB0005row("WORKINGYMD"), 1, 7) Then
                WW_Node = New TreeNode
                WW_Node.Text = Mid(MB0005row("WORKINGYMD"), 1, 4) & "年" & Mid(MB0005row("WORKINGYMD"), 6, 2) & "月"
                If MB0005row("TIMSTP") = 0 Then
                    WW_Node.Text &= " (未登録)"
                End If
                WW_Node.Value = Mid(MB0005row("WORKINGYMD"), 1, 7)
                WW_ParentNode.ChildNodes.Add(WW_Node)
            End If
        Next

        '○ ツリー内のノードを全て開く
        WF_DATE_TREE.ExpandAll()

    End Sub

    ''' <summary>
    ''' 一覧表示データ作成
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <remarks></remarks>
    Protected Sub MapDisplay(ByVal I_DATE As String)

        Dim WW_DATE As Date
        Dim WW_DATE_ST As String
        Dim WW_DATE_END As String
        Dim WW_TABLE As New DataTable

        Date.TryParse(I_DATE, WW_DATE)

        WW_DATE_ST = WW_DATE.ToString("yyyy/MM/dd")
        WW_DATE_END = WW_DATE.ToString("yyyy/MM") & "/" & WW_DATE.AddMonths(1).AddDays(-1).ToString("dd")

        CS0026TBLSORT.TABLE = MB0005tbl
        CS0026TBLSORT.SORTING = "WORKINGYMD"
        CS0026TBLSORT.FILTER = "WORKINGYMD >= '" & WW_DATE_ST & "' and WORKINGYMD <= '" & WW_DATE_END & "'"
        CS0026TBLSORT.sort(WW_TABLE)

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = True
        WF_Repeater.DataSource = WW_TABLE
        WF_Repeater.DataBind()

        WF_YYMM_L.Text = WW_DATE.ToString("yyyy") & "年" & WW_DATE.ToString("MM") & "月"

        For i As Integer = 0 To WF_Repeater.Items.Count - 1
            Try
                Date.TryParse(WW_TABLE.Rows(i)("WORKINGYMD"), WW_DATE)
            Catch ex As Exception
            End Try

            '日付
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_DATE"), Label).Text = WW_DATE.ToString("yyyy/MM/dd")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_DAY"), Label).Text = WW_DATE.ToString("dd") & "日"

            '曜日
            Dim WW_WEEK As String = ""
            CODENAME_get("WORKINGWEEK", WW_TABLE.Rows(i)("WORKINGWEEK"), WW_WEEK, WW_DUMMY)
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_WEEK"), Label).Text = WW_WEEK

            Select Case WW_TABLE.Rows(i)("WORKINGWEEK")
                Case "0"        '日曜日
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_WEEK"), Label).ForeColor = Color.Red
                Case "6"        '土曜日
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_WEEK"), Label).ForeColor = Color.Blue
                Case Else       '月曜日～金曜日
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_WEEK"), Label).ForeColor = Color.Black
            End Select

            If WW_TABLE.Rows(i)("WORKINGTEXT") <> "" Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_WEEK"), Label).ForeColor = Color.Red
            End If

            'テキスト
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_TEXT"), TextBox).Text = WW_TABLE.Rows(i)("WORKINGTEXT")

            '勤務状況の切替
            Select Case WW_TABLE.Rows(i)("WORKINGKBN")
                Case "0"        '平日
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW1"), CheckBox).Checked = True          '平日
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW2"), CheckBox).Checked = False         '法定
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW3"), CheckBox).Checked = False         '法定外
                Case "1"        '法定
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW1"), CheckBox).Checked = False         '平日
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW2"), CheckBox).Checked = True          '法定
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW3"), CheckBox).Checked = False         '法定外
                Case "2"        '法定外
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW1"), CheckBox).Checked = False         '平日
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW2"), CheckBox).Checked = False         '法定
                    CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW3"), CheckBox).Checked = True          '法定外
            End Select

            'イベント追加
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_TEXT"), TextBox).Attributes.Remove("Onchange")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW1"), CheckBox).Attributes.Remove("Onchange")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW2"), CheckBox).Attributes.Remove("Onchange")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW3"), CheckBox).Attributes.Remove("Onchange")

            CType(WF_Repeater.Items(i).FindControl("WF_Rep_TEXT"), TextBox).Attributes.Add("Onchange", "RepChange(" & i & ")")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW1"), CheckBox).Attributes.Add("Onchange", "RepChange(" & i & ")")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW2"), CheckBox).Attributes.Add("Onchange", "RepChange(" & i & ")")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SW3"), CheckBox).Attributes.Add("Onchange", "RepChange(" & i & ")")
        Next

        WW_TABLE.Clear()
        WW_TABLE.Dispose()
        WW_TABLE = Nothing

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ 関連チェック
        RelatedCheck(WW_ERR_SW)

        If isNormal(WW_ERR_SW) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'カレンダーマスタ登録更新
                UpdateCalendarMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MB0005tbl)

        '○ 一覧表示データ作成
        MapDisplay(WF_DISP_DATE.Value)

        '○ 日付ツリー作成
        TreeViewInitialize()

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        For Each MB0005row As DataRow In MB0005tbl.Rows
            Master.eraseCharToIgnore(MB0005row("WORKINGTEXT"))

            If MB0005row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso MB0005row("TIMSTP") <> 0 Then
                Continue For
            End If

            'テキスト入力チェック(法定・法定外の時)
            If MB0005row("WORKINGKBN") <> "0" AndAlso
                MB0005row("WORKINGTEXT") = "" Then
                WW_CheckMES1 = "・更新できないレコード(法定・法定外区分)です。"
                WW_CheckMES2 = "休日の場合、テキスト欄を入力してください。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0005row)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '日曜日チェック
            If MB0005row("WORKINGWEEK") = "0" AndAlso
                MB0005row("WORKINGKBN") <> "1" Then
                WW_CheckMES1 = "・更新できないレコード(法定・法定外区分)です。"
                WW_CheckMES2 = "日曜日は、法定休日を選択してください。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0005row)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '日曜日以外チェック
            If MB0005row("WORKINGWEEK") <> "0" AndAlso
                MB0005row("WORKINGKBN") = "1" Then
                WW_CheckMES1 = "・更新できないレコード(法定・法定外区分)です。"
                WW_CheckMES2 = "法定休日は選択出来ません。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0005row)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Next

    End Sub

    ''' <summary>
    ''' カレンダーマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateCalendarMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        MB005_CALENDAR" _
            & "    WHERE" _
            & "        CAMPCODE       = @P1" _
            & "        AND WORKINGYMD = @P2" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE MB005_CALENDAR" _
            & "    SET" _
            & "        WORKINGWEEK  = @P3     , WORKINGTEXT = @P4" _
            & "        , WORKINGKBN = @P5     , DELFLG      = @P6" _
            & "        , UPDYMD     = @P8     , UPDUSER     = @P9" _
            & "        , UPDTERMID  = @P10    , RECEIVEYMD  = @P11" _
            & "    WHERE" _
            & "        CAMPCODE       = @P1" _
            & "        AND WORKINGYMD = @P2" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO MB005_CALENDAR" _
            & "        (CAMPCODE        , WORKINGYMD" _
            & "        , WORKINGWEEK    , WORKINGTEXT" _
            & "        , WORKINGKBN     , DELFLG" _
            & "        , INITYMD        , UPDYMD" _
            & "        , UPDUSER        , UPDTERMID" _
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
            & "    , WORKINGYMD" _
            & "    , WORKINGWEEK" _
            & "    , WORKINGTEXT" _
            & "    , WORKINGKBN" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    MB005_CALENDAR" _
            & " WHERE" _
            & "    CAMPCODE       = @P1" _
            & "    AND WORKINGYMD = @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                    '営業年月日
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 2)             '営業日曜日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)            '営業日テキスト
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 2)             '営業日区分
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)             '削除フラグ
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.DateTime)                '登録年月日
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.DateTime)                '更新年月日
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)            '更新ユーザーID
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.Date)                '営業年月日

                For Each MB0005row As DataRow In MB0005tbl.Rows
                    If Trim(MB0005row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse MB0005row("TIMSTP") = 0 Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = MB0005row("CAMPCODE")
                        PARA2.Value = MB0005row("WORKINGYMD")
                        PARA3.Value = MB0005row("WORKINGWEEK")
                        PARA4.Value = MB0005row("WORKINGTEXT")
                        PARA5.Value = MB0005row("WORKINGKBN")
                        PARA6.Value = C_DELETE_FLG.ALIVE
                        PARA7.Value = WW_DATENOW
                        PARA8.Value = WW_DATENOW
                        PARA9.Value = Master.USERID
                        PARA10.Value = Master.USERTERMID
                        PARA11.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        MB0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = MB0005row("CAMPCODE")
                        JPARA2.Value = MB0005row("WORKINGYMD")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(MB0005UPDtbl) Then
                                MB0005UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    MB0005UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            MB0005UPDtbl.Clear()
                            MB0005UPDtbl.Load(SQLdr)
                        End Using

                        For Each MB0005UPDrow As DataRow In MB0005UPDtbl.Rows
                            MB0005row("TIMSTP") = MB0005UPDrow("TIMSTP")

                            CS0020JOURNAL.TABLENM = "MB005_CALENDAR"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MB0005UPDrow
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
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB005_CALENDAR UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MB005_CALENDAR UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = MB0005tbl                        'データ参照  Table
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
        CS0030REPORT.TBLDATA = MB0005tbl                        'データ参照Table
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
    ''' リピーター変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Repeater_Change()

        Dim WW_NUM As Integer = 0

        If Not String.IsNullOrEmpty(WF_FIELD_REP.Value) Then
            Try
                Integer.TryParse(WF_FIELD_REP.Value, WW_NUM)
            Catch ex As Exception
                Exit Sub
            End Try

            '○ 更新判定
            For Each MB0005row As DataRow In MB0005tbl.Rows
                If MB0005row("WORKINGYMD") = CType(WF_Repeater.Items(WW_NUM).FindControl("WF_Rep_DATE"), Label).Text Then
                    'テキスト
                    If MB0005row("WORKINGTEXT") <> CType(WF_Repeater.Items(WW_NUM).FindControl("WF_Rep_TEXT"), TextBox).Text Then
                        MB0005row("WORKINGTEXT") = CType(WF_Repeater.Items(WW_NUM).FindControl("WF_Rep_TEXT"), TextBox).Text
                        MB0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        Exit For
                    End If

                    '勤務状況の切替
                    If CType(WF_Repeater.Items(WW_NUM).FindControl("WF_Rep_SW1"), CheckBox).Checked AndAlso
                        MB0005row("WORKINGKBN") <> "0" Then
                        MB0005row("WORKINGKBN") = "0"
                        MB0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        Exit For
                    End If
                    If CType(WF_Repeater.Items(WW_NUM).FindControl("WF_Rep_SW2"), CheckBox).Checked AndAlso
                        MB0005row("WORKINGKBN") <> "1" Then
                        MB0005row("WORKINGKBN") = "1"
                        MB0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        Exit For
                    End If
                    If CType(WF_Repeater.Items(WW_NUM).FindControl("WF_Rep_SW3"), CheckBox).Checked AndAlso
                        MB0005row("WORKINGKBN") <> "2" Then
                        MB0005row("WORKINGKBN") = "2"
                        MB0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        Exit For
                    End If
                End If
            Next
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MB0005tbl)

        WF_FIELD_REP.Value = ""

    End Sub


    ''' <summary>
    ''' 日付ツリー押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_DATE_TREE_Click(sender As Object, e As EventArgs) Handles WF_DATE_TREE.SelectedNodeChanged

        '○ 年指定は処理対象外
        If Len(WF_DATE_TREE.SelectedNode.Value) > 4 Then
            Try
                '画面表示データ復元
                If Not Master.RecoverTable(MB0005tbl) Then
                    Exit Sub
                End If

                WF_DISP_DATE.Value = WF_DATE_TREE.SelectedNode.Value & "/01"
                MapDisplay(WF_DISP_DATE.Value)
            Finally
                If Not IsNothing(MB0005tbl) Then
                    MB0005tbl.Clear()
                    MB0005tbl.Dispose()
                    MB0005tbl = Nothing
                End If
            End Try
        End If

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
        Master.CreateEmptyTable(MB0005INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim MB0005INProw As DataRow = MB0005INPtbl.NewRow

            '○ 初期クリア
            For Each MB0005INPcol As DataColumn In MB0005INPtbl.Columns
                If IsDBNull(MB0005INProw.Item(MB0005INPcol)) OrElse IsNothing(MB0005INProw.Item(MB0005INPcol)) Then
                    Select Case MB0005INPcol.ColumnName
                        Case "LINECNT"
                            MB0005INProw.Item(MB0005INPcol) = 0
                        Case "OPERATION"
                            MB0005INProw.Item(MB0005INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MB0005INProw.Item(MB0005INPcol) = 0
                        Case Else
                            MB0005INProw.Item(MB0005INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("WORKINGYMD") >= 0 Then
                For Each MB0005row As DataRow In MB0005tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MB0005row("CAMPCODE") AndAlso
                        XLSTBLrow("WORKINGYMD") = MB0005row("WORKINGYMD") Then
                        MB0005INProw.ItemArray = MB0005row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MB0005INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '年月日
            If WW_COLUMNS.IndexOf("WORKINGYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(XLSTBLrow("WORKINGYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        MB0005INProw("WORKINGYMD") = ""
                    Else
                        MB0005INProw("WORKINGYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    MB0005INProw("WORKINGYMD") = ""
                End Try
            End If

            'テキスト
            If WW_COLUMNS.IndexOf("WORKINGTEXT") >= 0 Then
                MB0005INProw("WORKINGTEXT") = XLSTBLrow("WORKINGTEXT")
            End If

            '法定休日区分
            If WW_COLUMNS.IndexOf("WORKINGKBN") >= 0 Then
                MB0005INProw("WORKINGKBN") = XLSTBLrow("WORKINGKBN")
            End If

            '名称取得
            CODENAME_get("CAMPCODE", MB0005INProw("CAMPCODE"), MB0005INProw("CAMPNAMES"), WW_DUMMY)         '会社コード

            MB0005INPtbl.Rows.Add(MB0005INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        GRMB0005tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(MB0005tbl)

        '○ 一覧表示データ作成
        MapDisplay(WF_DISP_DATE.Value)

        '○ 日付ツリー作成
        TreeViewInitialize()

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

        '○ 開始、終了日付取得
        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Try
            Date.TryParse(work.WF_SEL_STYYMM.Text & "/01", WW_DATE_ST)
            Date.TryParse(work.WF_SEL_ENDYYMM.Text & "/01", WW_DATE_END)
            WW_DATE_END = WW_DATE_END.AddMonths(1).AddDays(-1)
        Catch ex As Exception

        End Try

        '○ 単項目チェック
        For Each MB0005INProw As DataRow In MB0005INPtbl.Rows

            WW_LINE_ERR = ""

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MB0005INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MB0005INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '年月日
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "WORKINGYMD", MB0005INProw("WORKINGYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(年月日)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '範囲チェック
            Dim WW_DATE As Date
            Try
                Date.TryParse(MB0005INProw("WORKINGYMD"), WW_DATE)
                If WW_DATE_ST > WW_DATE OrElse WW_DATE_END < WW_DATE Then
                    WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Catch ex As Exception
                MB0005INProw("WORKINGYMD") = ""
            End Try

            '法定休日区分
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "WORKINGKBN", MB0005INProw("WORKINGKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(法定休日区分)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MB0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If MB0005INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MB0005INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MB0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="MB0005row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal MB0005row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(MB0005row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社         =" & MB0005row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 日付         =" & MB0005row("WORKINGYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 法定休日区分 =" & MB0005row("WORKINGKBN")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' GRMB0005tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GRMB0005tbl_UPD()

        '○ 画面状態設定
        For Each MB0005row As DataRow In MB0005tbl.Rows
            Select Case MB0005row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MB0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MB0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            End Select
        Next

        '○ 追加変更判定
        For Each MB0005INProw As DataRow In MB0005INPtbl.Rows

            'エラーレコード読み飛ばし
            If MB0005INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            MB0005INProw("OPERATION") = "Insert"

            'KEY項目が等しい(ENDYMD以外のKEYが同じ)
            For Each MB0005row As DataRow In MB0005tbl.Rows
                If MB0005row("CAMPCODE") = MB0005INProw("CAMPCODE") AndAlso
                    MB0005row("WORKINGYMD") = MB0005INProw("WORKINGYMD") Then

                    '変更無は操作無
                    If MB0005row("WORKINGTEXT") = MB0005INProw("WORKINGTEXT") AndAlso
                        MB0005row("WORKINGKBN") = MB0005INProw("WORKINGKBN") Then
                        MB0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    MB0005INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each MB0005INProw As DataRow In MB0005INPtbl.Rows
            Select Case MB0005INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(MB0005INProw)
                Case "Insert"
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="MB0005INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef MB0005INProw As DataRow)

        For Each MB0005row As DataRow In MB0005tbl.Rows

            '同一(ENDYMD以外が同一KEY)レコード
            If MB0005INProw("CAMPCODE") = MB0005row("CAMPCODE") AndAlso
                MB0005INProw("WORKINGYMD") = MB0005row("WORKINGYMD") Then

                '画面入力テーブル項目設定
                MB0005INProw("LINECNT") = MB0005row("LINECNT")
                MB0005INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MB0005INProw("TIMSTP") = MB0005row("TIMSTP")

                '項目テーブル項目設定
                MB0005row.ItemArray = MB0005INProw.ItemArray
                Exit For
            End If
        Next

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
                Case "CAMPCODE"             '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "WORKINGWEEK"          '曜日
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "WORKINGWEEK"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

Imports System.IO
Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox

Public Class GRT00011ACTUALQTY
    Inherits Page

    '共通宣言
    ''' <summary>
    ''' ログ出力クラス
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    ''' <summary>
    ''' 権限チェック用クラス
    ''' </summary>
    Private CS0012AUTHORorg As New CS0012AUTHORorg                  '権限チェック(APサーバチェックあり)
    ''' <summary>
    ''' 一覧表示用クラス
    ''' </summary>
    Private CS0013ProfView As New CS0013ProfView                    'ユーザプロファイル（GridView）設定
    ''' <summary>
    ''' DB更新時のジャーナル登録用クラス
    ''' </summary>
    Private CS0020JOURNAL As New CS0020JOURNAL                      'Journal Out
    ''' <summary>
    ''' 帳票クラス
    ''' </summary>
    Private CS0023XLSTBL As New CS0023XLSUPLOAD                     'UPLOAD_XLSデータ取得
    ''' <summary>
    ''' 帳票出力
    ''' </summary>
    Private CS0030REPORTtbl As New CS0030REPORT                     '帳票出力(入力：TBL)
    ''' <summary>
    ''' 入力情報ジャーナル出力
    ''' </summary>
    Private CS0044L1INSERT As New CS0044L1INSERT                    'ジャーナル
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション情報
    ''' <summary>
    ''' T3コントロール取得
    ''' </summary>
    Private GS0029T3CNTLget As New GS0029T3CNTLget                  'T3コントロール
    ''' <summary>
    ''' 固定値マスタ検索
    ''' </summary>
    Private GS0007FIXVALUElst As New GS0007FIXVALUElst              'Leftボックス用固定値リスト取得
    ''' <summary>
    ''' テーブルソート
    ''' </summary>
    Private CS0026TBLSORT As New CS0026TBLSORT
    ''' <summary>
    ''' 勤怠共通クラス
    ''' </summary>
    Private T0007COM As New GRT0007COM                              '勤怠共通
    ''' <summary>
    ''' 日報共通クラス
    ''' </summary>
    Private T0005COM As New GRT0005COM                              '日報共通
    'テーブル更新
    Private MA002UPDATE As New GRMA002UPDATE                        '車両台帳更新
    Private MB003UPDATE As New GRMB003UPDATE                        '従業員（配送時間）更新
    Private MC006UPDATE As New GRMC006UPDATE                        '届先更新
    Private T0004UPDATE As New GRT0004UPDATE                        '配送受注、荷主受注ＤＢ更新
    Private T0005UPDATE As New GRT0005UPDATE                        '日報ＤＢ更新
    Private TA001UPDATE As New GRTA001UPDATE                        '車両稼働状況更新
    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    Private WW_RTN_SW As String                                     '
    Private WW_DUMMY As String                                      '

    Private T0011tbl As DataTable                                   '日報テーブル（GridView用）
    Private T0011INPtbl As DataTable                                '日報テーブル（取込用）
    Private T0011WKtbl As DataTable                                 '日報テーブル（ワーク）
    Private T0011WEEKtbl As DataTable                               '日報テーブル（一週間前）

    Private WW_ERRLISTCNT As Integer                                'エラーリスト件数               

    Private WW_ERRLIST_ALL As List(Of String)                       'インポート全体のエラー
    Private WW_ERRLIST As List(Of String)                           'インポート中の１セット分のエラー

    Private Const CONST_DSPROWCOUNT As Integer = 50                 '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 20              'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID


    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '■■■ 各ボタン押下処理 ■■■
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"                             '絞り込みボタン押下時処理
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"
                            WF_Print_Click("XLSX")
                        Case "WF_ButtonPrint"
                            WF_Print_Click("pdf")
                        Case "WF_ButtonFIRST"
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonEND"
                            WF_ButtonEND_Click()
                        Case "WF_ButtonSel"
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"
                            WF_ButtonCan_Click()
                        Case "WF_Field_DBClick"
                            WF_Field_DBClick()
                        Case "WF_ListboxDBclick"
                            WF_Listbox_DBClick()
                        Case "WF_RadioButonClick"
                            WF_RadioButon_Click()
                        Case "WF_MEMOChange"
                            WF_MEMO_Change()
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
                        Case "WF_MouseWheelDown"
                            WF_GRID_ScroleDown()
                        Case "WF_MouseWheelUp"
                            WF_GRID_ScroleUp()
                        Case "WF_EXCEL_UPLOAD"
                            UPLOAD_EXCEL()
                    End Select
                    '○一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '〇初期化処理
                Initialize()
            End If
        Finally

            If Not IsNothing(T0011tbl) Then
                T0011tbl.Dispose()
                T0011tbl = Nothing
            End If
            If Not IsNothing(T0011INPtbl) Then
                T0011INPtbl.Dispose()
                T0011INPtbl = Nothing
            End If
            If Not IsNothing(T0011WKtbl) Then
                T0011WKtbl.Dispose()
                T0011WKtbl = Nothing
            End If

            If Not IsNothing(T0011WEEKtbl) Then
                T0011WEEKtbl.Dispose()
                T0011WEEKtbl = Nothing
            End If

        End Try
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Initialize()
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
        'メッセージクリア
        WF_FIELD.Value = ""
        WF_STAFFCODE.Focus()
        '〇画面遷移処理
        MAPrefelence(O_RTN)
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップON
        Master.eventDrop = True

        '■■■ 選択情報　設定処理 ■■■
        '〇右Boxへの値設定
        rightview.MAPID_MEMO = Master.MAPID
        rightview.MAPID_REPORT = GRT00011WRKINC.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '〇通常検索
        GRID_INITset()

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(T0011tbl)
            Dim WW_STPOS As Integer = Val(work.WF_T5I_GridPosition.Text)
            Dim WW_ENDPOS As Integer = Val(work.WF_T5I_GridPosition.Text) + CONST_DSPROWCOUNT
            TBLview.RowFilter = "LINECNT >= " & WW_STPOS & " and LINECNT <= " & WW_ENDPOS
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRT00011WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.LEVENT = "Onchange"
            CS0013ProfView.LFUNC = "ListChange"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.CS0013ProfView()
        End Using
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If
    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        If IsNothing(T0011tbl) Then
            '○画面表示データ復元
            If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        End If
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To T0011tbl.Rows.Count - 1
            If T0011tbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                T0011tbl.Rows(i)("SELECT") = WW_DataCNT
            End If
        Next

        '○表示Linecnt取得
        If work.WF_T5I_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(work.WF_T5I_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLROWCOUNT) <= WW_DataCNT Then
                WW_GridPosition = WW_GridPosition + CONST_SCROLLROWCOUNT
            End If
        End If

        '表示開始_位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLROWCOUNT) > 0 Then
                WW_GridPosition = WW_GridPosition - CONST_SCROLLROWCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○画面（GridView）表示
        Dim WW_TBLview As DataView = New DataView(T0011tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
        '一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013PROFview.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRT00011WRKINC.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013PROFview.SRCDATA = WW_TBLview.ToTable
        CS0013PROFview.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
        CS0013PROFview.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            work.WF_T5I_GridPosition.Text = "1"
        Else
            work.WF_T5I_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If
        WF_STAFFCODE.Focus()

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    'イベント処理
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○入力値チェック
        Dim WW_CONVERT As String = ""
        Dim WW_TEXT As String = ""
        '乗務員
        CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WW_TEXT, WW_DUMMY)
        WF_STAFFCODE_TEXT.Text = WW_TEXT

        '○テーブルデータ 復元（絞込みボタン押下の時のみ）
        If WF_ButtonClick.Value Like "WF_ButtonExtract*" Then
            '〇データリカバリ
            If IsNothing(T0011tbl) Then
                If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
            End If
            '○データリカバリ（一週間前データ）
            If IsNothing(T0011WEEKtbl) Then
                If Not Master.RecoverTable(T0011WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
            End If
        End If

        '○絞り込み操作（GridView明細Hidden設定）
        For Each T0011row As DataRow In T0011tbl.Select("HDKBN='D' and WORKKBN='B3' ", "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ")
            If T0011row("SELECT") = 1 Then
                T0011row("HIDDEN") = 1

                '従業員・日報　絞込判定
                If (WF_STAFFCODE.Text = "") AndAlso (WF_YMD.Text = "") Then
                    T0011row("HIDDEN") = 0
                End If

                If (WF_STAFFCODE.Text <> "") AndAlso (WF_YMD.Text = "") Then
                    If T0011row("STAFFCODE") Like WF_STAFFCODE.Text & "*" Then
                        T0011row("HIDDEN") = 0
                    End If
                End If

                If (WF_STAFFCODE.Text = "") AndAlso (WF_YMD.Text <> "") Then
                    If Not IsDate(WF_YMD.Text) Then WF_YMD.Text = C_DEFAULT_YMD

                    If T0011row("YMD") = CDate(WF_YMD.Text).ToString("yyyy/MM/dd") Then T0011row("HIDDEN") = 0
                End If

                If (WF_STAFFCODE.Text <> "") AndAlso (WF_YMD.Text <> "") Then
                    If Not IsDate(WF_YMD.Text) Then WF_YMD.Text = C_DEFAULT_YMD

                    If T0011row("STAFFCODE") Like WF_STAFFCODE.Text & "*" AndAlso
                       T0011row("YMD") = CDate(WF_YMD.Text).ToString("yyyy/MM/dd") Then
                        T0011row("HIDDEN") = 0
                    End If
                End If
            End If
        Next

        If WF_ButtonClick.Value = "WF_ButtonExtract" Then
            work.WF_T5I_GridPosition.Text = "1"
        End If

        '○GridViewデータをテーブルに保存（絞込みボタン押下の時のみ）
        If WF_ButtonClick.Value Like "WF_ButtonExtract*" Then
            '〇データ保存
            If Not Master.SaveTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
            If Not Master.SaveTable(T0011WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
        End If

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        '○カーソル設定
        WF_FIELD.Value = "WF_STAFFCODE"
        WF_STAFFCODE.Focus()

    End Sub

    ''' <summary>
    ''' 更新ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

        rightview.setErrorReport("")
        If IsNothing(T0011tbl) Then
            If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        End If
        '○データリカバリ（一週間前データ）
        If IsNothing(T0011WEEKtbl) Then
            If Not Master.RecoverTable(T0011WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
        End If
        '〇ヘッダーの反映
        T0011_HeadToDetail(T0011tbl)

        '重複チェック
        Dim WW_MSG As String = String.Empty
        T0005COM.CheckDuplicateDataT0005(T0011tbl, WW_MSG, WW_RTN)
        If Not isNormal(WW_RTN) Then
            rightview.addErrorReport("内部処理エラー")
            rightview.addErrorReport(ControlChars.NewLine & WW_MSG)

            CS0011LOGWRITE.INFSUBCLASS = "T0005_DuplCheck"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0005_DuplCheck"                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = WW_MSG
            CS0011LOGWRITE.MESSAGENO = WW_RTN
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力
            Master.output(WW_RTN, C_MESSAGE_TYPE.ABORT, "T0005_DuplCheck")
            Exit Sub
        End If

        '--------------------------------------------------------------------
        'ＤＢ更新
        '--------------------------------------------------------------------
        'DataBase接続文字
        Using SQLcon As SqlConnection = CS0050Session.getConnection
            'トランザクション
            Dim SQLtrn As SqlClient.SqlTransaction = Nothing

            SQLcon.Open() 'DataBase接続(Open)
            'トランザクション開始
            SQLtrn = Nothing

            '〇車両台帳更新（走行キロ）
            MA002UPDATE.SQLcon = SQLcon
            MA002UPDATE.SQLtrn = SQLtrn
            MA002UPDATE.T0005tbl = T0011tbl
            MA002UPDATE.UPDUSERID = Master.USERID
            MA002UPDATE.UPDTERMID = Master.USERTERMID
            MA002UPDATE.Update()
            If Not isNormal(MA002UPDATE.ERR) Then
                Master.output(MA002UPDATE.ERR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If

            '〇車両稼働状況更新
            TA001UPDATE.SQLcon = SQLcon
            TA001UPDATE.SQLtrn = SQLtrn
            TA001UPDATE.T0005tbl = T0011tbl
            TA001UPDATE.UPDUSERID = Master.USERID
            TA001UPDATE.UPDTERMID = Master.USERTERMID
            TA001UPDATE.Update()
            If Not isNormal(TA001UPDATE.ERR) Then
                Master.output(TA001UPDATE.ERR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If

            ''〇従業員マスタ（配送用）更新（配送受注用）
            'MB003UPDATE.SQLcon = SQLcon
            'MB003UPDATE.SQLtrn = SQLtrn
            'MB003UPDATE.SORG = work.WF_SEL_UORG.Text
            'MB003UPDATE.UPDUSERID = Master.USERID
            'MB003UPDATE.UPDTERMID = Master.USERTERMID
            'MB003UPDATE.T0005tbl = T0011tbl
            'MB003UPDATE.Update()
            'If Not isNormal(MB003UPDATE.ERR) Then
            '    Master.output(MB003UPDATE.ERR, C_MESSAGE_TYPE.ABORT, "例外発生")
            '    Exit Sub
            'End If


            '〇配送受注、荷主受注更新
            Dim WW_T0004tbl As DataTable = New DataTable
            T0004UPDATE.SQLcon = SQLcon
            T0004UPDATE.SQLtrn = SQLtrn
            T0004UPDATE.T0005tbl = T0011tbl
            T0004UPDATE.UPDUSERID = Master.USERID
            T0004UPDATE.UPDTERMID = Master.USERTERMID
            T0004UPDATE.ListBoxGSHABAN = work.createSHABANLists(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
            T0004UPDATE.Update()
            If isNormal(T0004UPDATE.ERR) AndAlso isNormal(O_RTN) Then
                T0011tbl = T0004UPDATE.rtnTbl
            Else
                Master.output(T0004UPDATE.ERR, C_MESSAGE_TYPE.ABORT, "例外発生")
                Exit Sub
            End If

            '〇日報ＤＢ更新

            '統計DB出力用項目設定
            CS0026TBLSORT.TABLE = T0011tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            '削除データの退避
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            Dim WW_T0011DELtbl As DataTable = CS0026TBLSORT.sort()
            '有効データのみ
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            Dim WW_T0011SELtbl As DataTable = CS0026TBLSORT.sort()
            '有効データ＋１週間前
            WW_T0011SELtbl.Merge(T0011WEEKtbl)

            '〇トリップ判定・回送判定・出荷日内荷積荷卸回数判定
            T0005COM.ReEditT0005(WW_T0011SELtbl, work.WF_SEL_CAMPCODE.Text, WW_RTN)
            '有効データと１週間前データの分離
            CS0026TBLSORT.TABLE = WW_T0011SELtbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "YMD >= #" & work.WF_SEL_STYMD.Text & "#"
            T0011tbl = CS0026TBLSORT.sort()

            '有効レコード＋削除レコード（元に戻す）
            T0011tbl.Merge(WW_T0011DELtbl)

            Dim WW_DATE As Date = Date.Now
            '〇T0005更新処理
            T0005UPDATE.SQLcon = SQLcon
            T0005UPDATE.SQLtrn = SQLtrn
            T0005UPDATE.T0005tbl = T0011tbl
            T0005UPDATE.ENTRYDATE = WW_DATE
            T0005UPDATE.UPDUSERID = Master.USERID
            T0005UPDATE.UPDTERMID = Master.USERTERMID
            T0005UPDATE.Update()
            If isNormal(T0005UPDATE.ERR) Then
                T0011tbl = T0005UPDATE.T0005tbl
            Else
                Master.output(T0005UPDATE.ERR, C_MESSAGE_TYPE.ABORT, "例外発生")
                Exit Sub
            End If
            '〇不要テーブルデータ除去
            If Not IsNothing(WW_T0011DELtbl) Then
                WW_T0011DELtbl.Dispose()
                WW_T0011DELtbl = Nothing
            End If
            If Not IsNothing(WW_T0011SELtbl) Then
                WW_T0011SELtbl.Dispose()
                WW_T0011SELtbl = Nothing
            End If
            '〇統計ＤＢ更新
            Dim L00001tbl = New DataTable
            CS0044L1INSERT.CS0044L1ColmnsAdd(L00001tbl)

            '有効データのみ
            CS0026TBLSORT.TABLE = T0011tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            Dim WW_T0011LSELtbl As DataTable = CS0026TBLSORT.sort()

            '削除データ（削除処理）
            Dim WW_DATENOW As DateTime = Date.Now
            '日報ＤＢ更新
            Dim SQLStr As String =
                        "UPDATE L0001_TOKEI " _
                      & "SET DELFLG         = '1'  " _
                      & "  , UPDYMD         = @P05 " _
                      & "  , UPDUSER        = @P06 " _
                      & "  , UPDTERMID      = @P07 " _
                      & "  , RECEIVEYMD     = @P08 " _
                      & "WHERE CAMPCODE     = @P01 " _
                      & "  and DENTYPE      = @P02 " _
                      & "  and NACSHUKODATE = @P03 " _
                      & "  and KEYSTAFFCODE = @P04 " _
                      & "  and DELFLG      <> '1'  "

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.DateTime)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)

                For Each T0011row As DataRow In WW_T0011LSELtbl.Rows
                    '〇更新対象レコードは統計情報を一度削除する（ヘッダーを用いて削除）
                    If T0011row("HDKBN") = "H" AndAlso T0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then

                        Try
                            PARA01.Value = work.WF_SEL_CAMPCODE.Text
                            PARA02.Value = "T05"
                            PARA03.Value = T0011row("YMD")
                            PARA04.Value = T0011row("STAFFCODE")
                            PARA05.Value = WW_DATENOW
                            PARA06.Value = Master.USERID
                            PARA07.Value = Master.USERTERMID
                            PARA08.Value = C_DEFAULT_YMD

                            SQLcmd.ExecuteNonQuery()
                        Catch ex As Exception
                            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_TOKEI")

                            CS0011LOGWRITE.INFSUBCLASS = "L0001_Delete"                 'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "DB:UPDATE L0001_TOKEI"            '
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWRITE.TEXT = ex.ToString()
                            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                            Exit Sub

                        End Try

                    End If
                Next
            End Using
            '〇 L00001統計ＤＢ編集
            T0005COM.EditL00001(WW_T0011LSELtbl, L00001tbl, WW_RTN)
            '〇 L00001統計ＤＢサマリー
            T0005COM.SumL00001(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, Master.USERID, L00001tbl, WW_RTN)

            WW_DATENOW = Date.Now
            For Each L00001row As DataRow In L00001tbl.Rows
                L00001row("INITYMD") = WW_DATENOW '登録年月日
                L00001row("UPDYMD") = WW_DATENOW  '更新年月日
                L00001row("UPDUSER") = CS0050Session.USERID   '更新ユーザＩＤ
                L00001row("UPDTERMID") = CS0050Session.TERMID    '更新端末
                L00001row("RECEIVEYMD") = C_DEFAULT_YMD   '集信日時
            Next

            '統計DB出力
            CS0044L1INSERT.SQLCON = SQLcon
            CS0044L1INSERT.CS0044L1Insert(L00001tbl)
            If Not isNormal(CS0044L1INSERT.ERR) Then
                Master.output(CS0044L1INSERT.ERR, C_MESSAGE_TYPE.ABORT, "CS0044L1INSERT")
                Exit Sub
            End If

            If Not IsNothing(WW_T0011LSELtbl) Then
                WW_T0011LSELtbl.Dispose()
                WW_T0011LSELtbl = Nothing
            End If
            If Not IsNothing(L00001tbl) Then
                L00001tbl.Dispose()
                L00001tbl = Nothing
            End If

            '検索SQL文
            Dim SQLStrTime As String =
                 "SELECT TIMSTP = cast(A.UPDTIMSTP  as bigint) " _
                & "     ,ENTRYDATE       					   " _
                & " FROM T0005_NIPPO AS A					   " _
                & " WHERE A.CAMPCODE         = @P01            " _
                & "  and  A.SHIPORG          = @P02            " _
                & "  and  A.TERMKBN          = @P03            " _
                & "  and  A.YMD              = @P04            " _
                & "  and  A.STAFFCODE        = @P05            " _
                & "  and  A.SEQ              = @P06            " _
                & "  and  A.NIPPONO          = @P07            " _
                & "  and  A.DELFLG          <> '1'             "

            Using SQLcmdTime As New SqlCommand(SQLStrTime, SQLcon, SQLtrn)

                Dim PARAT1 As SqlParameter = SQLcmdTime.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT2 As SqlParameter = SQLcmdTime.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT3 As SqlParameter = SQLcmdTime.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT4 As SqlParameter = SQLcmdTime.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                Dim PARAT5 As SqlParameter = SQLcmdTime.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT6 As SqlParameter = SQLcmdTime.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
                Dim PARAT7 As SqlParameter = SQLcmdTime.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)
                'タイムスタンプの取得
                For Each WW_row As DataRow In T0011tbl.Rows
                    Try
                        If WW_row("HDKBN") = "D" AndAlso
                           WW_row("SELECT") = "1" AndAlso
                           WW_row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then

                            '○関連受注指定
                            PARAT1.Value = WW_row("CAMPCODE")
                            PARAT2.Value = WW_row("SHIPORG")
                            PARAT3.Value = WW_row("TERMKBN")
                            PARAT4.Value = WW_row("YMD")
                            PARAT5.Value = WW_row("STAFFCODE")
                            PARAT6.Value = WW_row("SEQ")
                            PARAT7.Value = WW_row("NIPPONO")

                            '■SQL実行
                            Using SQLdr As SqlDataReader = SQLcmdTime.ExecuteReader()

                                While SQLdr.Read
                                    WW_row("TIMSTP") = SQLdr("TIMSTP")
                                    WW_row("ENTRYDATE") = SQLdr("ENTRYDATE")
                                End While

                            End Using
                        End If
                    Catch ex As Exception
                        CS0011LOGWRITE.INFSUBCLASS = "CS0047T5_Select"              'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:SELECT T0005_NIPPO"            '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try
                Next
            End Using
            'タイムスタンプをヘッダに反映
            CS0026TBLSORT.TABLE = T0011tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            T0011tbl = CS0026TBLSORT.sort()

            For i As Integer = T0011tbl.Rows.Count - 1 To 0 Step -1
                Dim T0011row As DataRow = T0011tbl.Rows(i)
                If T0011row("SELECT") = "0" Then
                    T0011row.Delete()
                Else
                    If T0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                        T0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        If T0011row("HDKBN") = "H" Then
                            Dim WW_row As DataRow = T0011tbl.Rows(i + 1)
                            T0011row("TIMSTP") = WW_row("TIMSTP")
                        End If
                    End If
                End If
            Next

            '○Close
            If Not IsNothing(WW_T0004tbl) Then
                WW_T0004tbl.Dispose()
                WW_T0004tbl = Nothing
            End If
        End Using
        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0011WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

        '絞込みボタン処理（GridViewの表示）を行う
        WF_ButtonExtract_Click()
    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理
    ''' </summary>
    ''' <param name="OutType"></param>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click(ByVal OutType As String)

        'テーブルデータ 復元
        If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

        Using WW_TBLview As DataView = New DataView(T0011tbl)
            WW_TBLview.Sort = "CAMPCODE, SHIPORG, TERMKBN, YMD, STAFFCODE, SEQ"
            WW_TBLview.RowFilter = "HDKBN='D' and SELECT = '1' and HIDDEN='0' "
            Using WW_TBL As DataTable = WW_TBLview.ToTable

                '帳票出力dll Interface
                CS0030REPORTtbl.CAMPCODE = work.WF_SEL_CAMPCODE.Text
                CS0030REPORTtbl.PROFID = Master.PROF_REPORT
                CS0030REPORTtbl.MAPID = GRT00011WRKINC.MAPID                   'PARAM01:画面ID
                CS0030REPORTtbl.REPORTID = rightview.getReportId               'PARAM02:帳票ID
                CS0030REPORTtbl.FILEtyp = OutType                              'PARAM03:出力ファイル形式
                CS0030REPORTtbl.TBLDATA = WW_TBL                               'PARAM04:データ参照tabledata
                CS0030REPORTtbl.CS0030REPORT()

                If Not isNormal(CS0030REPORTtbl.ERR) Then
                    Master.output(CS0030REPORTtbl.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
                    Exit Sub
                End If

                '別画面でPDFを表示
                WF_PrintURL.Value = CS0030REPORTtbl.URL
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
            End Using
        End Using
    End Sub
    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '画面遷移実行
        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 先頭頁ボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()
        '○データリカバリ 
        If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '先頭頁に移動
        work.WF_T5I_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()
        '○データリカバリ 
        If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○対象データ件数取得
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(T0011tbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '最終頁に移動
        If WW_TBLview.Count Mod 10 = 0 Then
            work.WF_T5I_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            work.WF_T5I_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If

    End Sub
    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '〇LeftBox処理（フィールドダブルクリック時）
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    Dim prmData As Hashtable = work.createFIXParam(work.WF_SEL_CAMPCODE.Text)
                    Select Case WF_LeftMViewChange.Value
                        Case LIST_BOX_CLASSIFICATION.LC_STAFFCODE
                            prmData = work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)

                    End Select
                    .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .activeListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_YMD"
                            .WF_Calendar.Text = WF_YMD.Text

                    End Select
                    .activeCalendar()
                End If
            End With
        End If
    End Sub
    ''' <summary>
    ''' 左リストボックスダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Listbox_DBClick()
        WF_ButtonSel_Click()

    End Sub
    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '〇RightBox処理（ラジオボタン選択）
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
    ''' メモ欄変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MEMO_Change()
        '〇RightBox処理（右Boxメモ変更時）
        rightview.MAPID = Master.MAPID
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleDown()
        '○画面表示データ復元
        If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○データリカバリ
        If Not Master.RecoverTable(T0011WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleUp()
        '○画面表示データ復元
        If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○データリカバリ
        If Not Master.RecoverTable(T0011WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

    End Sub

    ''' <summary>
    ''' leftBOX選択ボタン処理(ListBox値 ---> detailbox)　
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue() As String

        WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
        WW_SelectValue = leftview.getActiveValue

        Select Case WF_FIELD.Value
            Case "WF_STAFFCODE"
                '乗務員 
                WF_STAFFCODE_TEXT.Text = WW_SelectValue(1)
                WF_STAFFCODE.Text = WW_SelectValue(0)
                WF_STAFFCODE.Focus()
            Case "WF_YMD"
                '出庫日 
                WF_YMD.Text = WW_SelectValue(0)
                WF_YMD.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
    End Sub

    ''' <summary>
    ''' leftBOXキャンセルボタン処理　
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_STAFFCODE"
                '従業員コード　 
                WF_STAFFCODE.Focus()
            Case "WF_YMD"
                '出庫日　 
                WF_YMD.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

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
        Catch ex As Exception
            Exit Sub
        End Try
        '○画面表示データ復元
        If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

        '○ 変更チェック
        For Each T0011row As DataRow In T0011tbl.Rows

            If T0011row("LINECNT") <> WF_SelectedIndex.Value Then Continue For
            '数量
            For QTYCNT As Integer = 1 To 8
                If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SURYO" & QTYCNT & WF_SelectedIndex.Value)) AndAlso
                  T0011row("SURYO" & QTYCNT) <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SURYO" & QTYCNT & WF_SelectedIndex.Value)) Then
                    T0011row("SURYO" & QTYCNT) = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SURYO" & QTYCNT & WF_SelectedIndex.Value))
                    T0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If

            Next
        Next
        ' 〇チェック処理
        T0011tbl_CheckOrder(T0011tbl, WW_RTN_SW)
        '〇ヘッダーの再作成
        T0011_CreHead(T0011tbl)
        '○ 画面表示データ保存
        If Not Master.SaveTable(T0011tbl) Then Exit Sub
    End Sub

    ''' <summary>
    ''' GridView用データ取得        ★済
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GRID_INITset()

        Dim WW_CONVERT As String = ""

        '■■■ 画面表示用データ取得 ■■■
        Dim WW_SORT As String = ""
        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0026TBLSORT.MAPID = Master.MAPID
            CS0026TBLSORT.PROFID = Master.PROF_VIEW
            CS0026TBLSORT.TAB = ""
            CS0026TBLSORT.VARI = Master.VIEWID
            CS0026TBLSORT.TABLE = T0011tbl
            CS0026TBLSORT.getSorting()
            WW_SORT = CS0026TBLSORT.SORTING
            '■テーブル検索結果をテーブル退避
            '日報DB更新用テーブル

            T0005COM.AddColumnT0005tbl(T0011tbl)

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                '　検索説明
                '　　Step1：操作USERが、メンテナンス可能なUSERを取得
                '　　　　　　※権限ではUSER、MAPで行う必要があるが、絞り込み効率を勘案し、最初にUSERで処理を限定
                '　　Step2：メンテナンス可能USERおよびデフォルトUSERのTBL(S0007_UPROFVARI)を取得
                '　　        画面表示は、参照可能および更新ユーザに関連するTBLデータとなる
                '　　　　　　※権限について（参考）　権限チャックは、表追加のタイミングで行う。
                '　　　　　　　　チェック内容
                '　　　　　　　　①操作USERは、TBL入力データ(USER)の更新権限をもっているか。
                '　　　　　　　　②TBL入力データ(USER)は、TBL入力データ(MAP)の参照および更新権限をもっているか。
                '　　　　　　　　③TBL入力データ(USER)は、TBL入力データ(CAMPCODE)の参照および更新権限をもっているか。
                '　　Step3：関連するグループコードを取得(操作USERに依存)
                '　　Step4：関連する名称を取得(TBL入力データ(USER)に依存)
                '　注意事項　日付について
                '　　権限判断はすべてDateNow。グループコード、名称取得は全てDateNow。表追加時の①はDateNow。
                '　　但し、表追加時の②および③は、TBL入力有効期限。

                Dim SQLStr As String =
                     "SELECT 0                                  as      LINECNT           , " _
                   & "       ''                                 as      OPERATION         , " _
                   & "       TIMSTP = cast(A.UPDTIMSTP as bigint)                         , " _
                   & "       0                                  as      'SELECT'          , " _
                   & "       1                                  as      HIDDEN            , " _
                   & "       ''                                 as      ORDERUMU          , " _
                   & "       '0'                                as      EXTRACTCNT        , " _
                   & "       'OFF'                              as      CTRL              , " _
                   & "       ''                                 as      TWOMANTRIP        , " _
                   & "       isnull(rtrim(A.CAMPCODE),'')       as      CAMPCODE          , " _
                   & "       isnull(rtrim(M1.NAMES),'')         as      CAMPNAMES         , " _
                   & "       isnull(rtrim(A.SHIPORG),'')        as      SHIPORG           , " _
                   & "       isnull(rtrim(M2.NAMES),'')         as      SHIPORGNAMES      , " _
                   & "       isnull(rtrim(A.TERMKBN),'')        as      TERMKBN           , " _
                   & "       isnull(rtrim(F1.VALUE1),'')        as      TERMKBNNAMES      , " _
                   & "       isnull(rtrim(A.YMD),'')            as      YMD               , " _
                   & "       isnull(rtrim(A.NIPPONO),'')        as      NIPPONO           , " _
                   & "       isnull(rtrim(A.WORKKBN),'')        as      WORKKBN           , " _
                   & "       isnull(rtrim(F2.VALUE1),'')        as      WORKKBNNAMES      , " _
                   & "       isnull(A.SEQ,'0')                  as      SEQ               , " _
                   & "       isnull(rtrim(A.STAFFCODE),'')      as      STAFFCODE         , " _
                   & "       isnull(rtrim(A.ENTRYDATE),'')      as      ENTRYDATE         , " _
                   & "       isnull(rtrim(B.STAFFNAMES),'')     as STAFFNAMES        , " _
                   & "       isnull(rtrim(A.SUBSTAFFCODE),'')   as SUBSTAFFCODE      , " _
                   & "       isnull(rtrim(B2.STAFFNAMES),'')    as SUBSTAFFNAMES     , " _
                   & "       isnull(rtrim(A.CREWKBN),'')        as CREWKBN           , " _
                   & "       isnull(rtrim(F3.VALUE1),'')        as CREWKBNNAMES      , " _
                   & "       isnull(rtrim(A.GSHABAN),'')        as GSHABAN           , " _
                   & "       ''                                 as GSHABANLICNPLTNO  , " _
                   & "       isnull(rtrim(A.STDATE),'')         as STDATE , " _
                   & "       isnull(rtrim(A.STTIME),'')         as STTIME , " _
                   & "       isnull(rtrim(A.ENDDATE),'')        as ENDDATE , " _
                   & "       isnull(rtrim(A.ENDTIME),'')        as ENDTIME , " _
                   & "       isnull(rtrim(A.WORKTIME),'')       as WORKTIME , " _
                   & "       isnull(rtrim(A.MOVETIME),'')       as MOVETIME , " _
                   & "       isnull(rtrim(A.ACTTIME),'')        as ACTTIME , " _
                   & "       isnull(A.PRATE,'0')                as PRATE , " _
                   & "       isnull(A.CASH,'0')                 as CASH , " _
                   & "       isnull(A.TICKET,'0')               as TICKET , " _
                   & "       isnull(A.ETC,'0')                  as ETC , " _
                   & "       isnull(A.TOTALTOLL,'0')            as TOTALTOLL , " _
                   & "       isnull(A.STMATER,'0')              as STMATER , " _
                   & "       isnull(A.ENDMATER,'0')             as ENDMATER , " _
                   & "       isnull(A.RUIDISTANCE,'0')          as RUIDISTANCE , " _
                   & "       isnull(A.SOUDISTANCE,'0')          as SOUDISTANCE , " _
                   & "       isnull(A.JIDISTANCE,'0')           as JIDISTANCE , " _
                   & "       isnull(A.KUDISTANCE,'0')           as KUDISTANCE , " _
                   & "       isnull(A.IPPDISTANCE,'0')          as IPPDISTANCE , " _
                   & "       isnull(A.KOSDISTANCE,'0')          as KOSDISTANCE , " _
                   & "       isnull(A.IPPJIDISTANCE,'0')        as IPPJIDISTANCE , " _
                   & "       isnull(A.IPPKUDISTANCE,'0')        as IPPKUDISTANCE , " _
                   & "       isnull(A.KOSJIDISTANCE,'0')        as KOSJIDISTANCE , " _
                   & "       isnull(A.KOSKUDISTANCE,'0')        as KOSKUDISTANCE , " _
                   & "       isnull(A.KYUYU,'0')                as KYUYU , " _
                   & "       isnull(rtrim(A.TORICODE),'')       as TORICODE , " _
                   & "       isnull(rtrim(MC2.NAMES),'')        as TORINAMES , " _
                   & "       isnull(rtrim(A.SHUKABASHO),'')     as SHUKABASHO , " _
                   & "       isnull(rtrim(MC62.NAMES),'')       as SHUKABASHONAMES , " _
                   & "       isnull(rtrim(A.SHUKADATE),'')      as SHUKADATE , " _
                   & "       isnull(rtrim(A.TODOKECODE),'')     as TODOKECODE , " _
                   & "       isnull(rtrim(MC6.NAMES),'')        as TODOKENAMES , " _
                   & "       isnull(rtrim(A.TODOKEDATE),'')     as TODOKEDATE , " _
                   & "       isnull(rtrim(A.OILTYPE1),'')       as OILTYPE1 , " _
                   & "       isnull(rtrim(A.PRODUCT11),'')      as PRODUCT11 , " _
                   & "       isnull(rtrim(A.PRODUCT21),'')      as PRODUCT21 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE1),'')   as PRODUCTCODE1 ," _
                   & "       ''                                 as PRODUCT1NAMES , " _
                   & "       isnull(A.SURYO1,'0')               as SURYO1 , " _
                   & "       isnull(rtrim(A.STANI1),'')         as STANI1 , " _
                   & "       isnull(rtrim(F41.VALUE1),'')       as STANI1NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE2),'')       as OILTYPE2 , " _
                   & "       isnull(rtrim(A.PRODUCT12),'')      as PRODUCT12 , " _
                   & "       isnull(rtrim(A.PRODUCT22),'')      as PRODUCT22 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE2),'')   as PRODUCTCODE2 ," _
                   & "       ''                                 as PRODUCT2NAMES , " _
                   & "       isnull(A.SURYO2,'0')               as SURYO2 , " _
                   & "       isnull(rtrim(A.STANI2),'')         as STANI2 , " _
                   & "       isnull(rtrim(F42.VALUE1),'')       as STANI2NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE3),'')       as OILTYPE3 , " _
                   & "       isnull(rtrim(A.PRODUCT13),'')      as PRODUCT13 , " _
                   & "       isnull(rtrim(A.PRODUCT23),'')      as PRODUCT23 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE3),'')   as PRODUCTCODE3 ," _
                   & "       ''                                 as PRODUCT3NAMES , " _
                   & "       isnull(A.SURYO3,'0')               as SURYO3 , " _
                   & "       isnull(rtrim(A.STANI3),'')         as STANI3 , " _
                   & "       isnull(rtrim(F43.VALUE1),'')       as STANI3NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE4),'')       as OILTYPE4 , " _
                   & "       isnull(rtrim(A.PRODUCT14),'')      as PRODUCT14 , " _
                   & "       isnull(rtrim(A.PRODUCT24),'')      as PRODUCT24 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE4),'')   as PRODUCTCODE4 ," _
                   & "       ''                                 as PRODUCT4NAMES , " _
                   & "       isnull(A.SURYO4,'0')               as SURYO4 , " _
                   & "       isnull(rtrim(A.STANI4),'')         as STANI4 , " _
                   & "       isnull(rtrim(F44.VALUE1),'')       as STANI4NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE5),'')       as OILTYPE5 , " _
                   & "       isnull(rtrim(A.PRODUCT15),'')      as PRODUCT15 , " _
                   & "       isnull(rtrim(A.PRODUCT25),'')      as PRODUCT25 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE5),'')   as PRODUCTCODE5 ," _
                   & "       ''                                 as PRODUCT5NAMES , " _
                   & "       isnull(A.SURYO5,'0')               as SURYO5 , " _
                   & "       isnull(rtrim(A.STANI5),'')         as STANI5 , " _
                   & "       isnull(rtrim(F45.VALUE1),'')       as STANI5NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE6),'')       as OILTYPE6 , " _
                   & "       isnull(rtrim(A.PRODUCT16),'')      as PRODUCT16 , " _
                   & "       isnull(rtrim(A.PRODUCT26),'')      as PRODUCT26 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE6),'')   as PRODUCTCODE6 ," _
                   & "       ''                                 as PRODUCT6NAMES , " _
                   & "       isnull(A.SURYO6,'0')               as SURYO6 , " _
                   & "       isnull(rtrim(A.STANI6),'')         as STANI6 , " _
                   & "       isnull(rtrim(F46.VALUE1),'')       as STANI6NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE7),'')       as OILTYPE7 , " _
                   & "       isnull(rtrim(A.PRODUCT17),'')      as PRODUCT17 , " _
                   & "       isnull(rtrim(A.PRODUCT27),'')      as PRODUCT27 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE7),'')   as PRODUCTCODE7 ," _
                   & "       ''                                 as PRODUCT7NAMES , " _
                   & "       isnull(A.SURYO7,'0')               as SURYO7 , " _
                   & "       isnull(rtrim(A.STANI7),'')         as STANI7 , " _
                   & "       isnull(rtrim(F47.VALUE1),'')       as STANI7NAMES , " _
                   & "       isnull(rtrim(A.OILTYPE8),'')       as OILTYPE8 , " _
                   & "       isnull(rtrim(A.PRODUCT18),'')      as PRODUCT18 , " _
                   & "       isnull(rtrim(A.PRODUCT28),'')      as PRODUCT28 , " _
                   & "       isnull(rtrim(A.PRODUCTCODE8),'')   as PRODUCTCODE8 ," _
                   & "       ''                                 as PRODUCT8NAMES , " _
                   & "       isnull(A.SURYO8,'0')               as SURYO8 , " _
                   & "       isnull(rtrim(A.STANI8),'')         as STANI8 , " _
                   & "       isnull(rtrim(F48.VALUE1),'')       as STANI8NAMES , " _
                   & "       isnull(A.TOTALSURYO,'0')           as TOTALSURYO , " _
                   & "       isnull(rtrim(A.ORDERNO),'')        as ORDERNO , " _
                   & "       isnull(rtrim(A.DETAILNO),'')       as DETAILNO , " _
                   & "       isnull(rtrim(A.TRIPNO),'')         as TRIPNO , " _
                   & "       isnull(rtrim(A.DROPNO),'')         as DROPNO , " _
                   & "       isnull(rtrim(A.JISSKIKBN),'')      as JISSKIKBN , " _
                   & "       ''                                 as JISSKIKBNNAMES , " _
                   & "       isnull(rtrim(A.URIKBN),'')         as URIKBN , " _
                   & "       isnull(rtrim(F6.VALUE1),'')        as URIKBNNAMES , " _
                   & "       isnull(rtrim(A.TUMIOKIKBN),'')     as TUMIOKIKBN , " _
                   & "       isnull(rtrim(F5.VALUE1),'')        as TUMIOKIKBNNAMES , " _
                   & "       isnull(rtrim(A.STORICODE),'')      as STORICODE , " _
                   & "       isnull(rtrim(MC22.NAMES),'')       as STORICODENAMES , " _
                   & "       isnull(rtrim(A.CONTCHASSIS),'')    as CONTCHASSIS , " _
                   & "       ''                                 as CONTCHASSISLICNPLTNO , " _
                   & "       isnull(rtrim(A.SHARYOTYPEF),'')    as SHARYOTYPEF , " _
                   & "       isnull(rtrim(A.TSHABANF),'')       as TSHABANF , " _
                   & "       isnull(rtrim(A.SHARYOTYPEB),'')    as SHARYOTYPEB , " _
                   & "       isnull(rtrim(A.TSHABANB),'')       as TSHABANB , " _
                   & "       isnull(rtrim(A.SHARYOTYPEB2),'')   as SHARYOTYPEB2 , " _
                   & "       isnull(rtrim(A.TSHABANB2),'')      as TSHABANB2 , " _
                   & "       isnull(rtrim(A.TAXKBN),'')         as TAXKBN , " _
                   & "       isnull(rtrim(F7.VALUE1),'')        as TAXKBNNAMES , " _
                   & "       isnull(rtrim(A.LATITUDE),'')       as LATITUDE , " _
                   & "       isnull(rtrim(A.LONGITUDE),'')      as LONGITUDE , " _
                   & "       isnull(rtrim(A.L1HAISOGROUP),'')   as wHaisoGroup , " _
                   & "       isnull(rtrim(A.DELFLG),'0')        as DELFLG , " _
                   & "       ''                                 as HOLIDAYKBN , " _
                   & "       ''                                 as TORITYPE01 , " _
                   & "       ''                                 as TORITYPE02 , " _
                   & "       ''                                 as TORITYPE03 , " _
                   & "       ''                                 as TORITYPE04 , " _
                   & "       ''                                 as TORITYPE05 , " _
                   & "       ''                                 as SUPPLIERKBN , " _
                   & "       ''                                 as SUPPLIER , " _
                   & "       ''                                 as MANGOILTYPE , " _
                   & "       ''                                 as MANGMORG1 , " _
                   & "       ''                                 as MANGSORG1 , " _
                   & "       ''                                 as MANGUORG1 , " _
                   & "       ''                                 as BASELEASE1 , " _
                   & "       ''                                 as MANGMORG2 , " _
                   & "       ''                                 as MANGSORG2 , " _
                   & "       ''                                 as MANGUORG2 , " _
                   & "       ''                                 as BASELEASE2 , " _
                   & "       ''                                 as MANGMORG3 , " _
                   & "       ''                                 as MANGSORG3 , " _
                   & "       ''                                 as MANGUORG3 , " _
                   & "       ''                                 as BASELEASE3 , " _
                   & "       ''                                 as STAFFKBN , " _
                   & "       ''                                 as MORG , " _
                   & "       ''                                 as HORG , " _
                   & "       ''                                 as SUBSTAFFKBN , " _
                   & "       ''                                 as SUBMORG , " _
                   & "       ''                                 as SUBHORG , " _
                   & "       ''                                 as ORDERORG  " _
                   & " FROM      T0005_NIPPO A " _
                   & " LEFT JOIN MB001_STAFF B " _
                   & "   ON    B.CAMPCODE    = A.CAMPCODE " _
                   & "   and   B.STAFFCODE   = A.STAFFCODE " _
                   & "   and   B.STYMD      <= A.YMD " _
                   & "   and   B.ENDYMD     >= A.YMD " _
                   & "   and   B.STYMD       = ( " _
                   & "    SELECT MAX(STYMD)  " _
                   & "    FROM     MB001_STAFF    B2 " _
                   & "    WHERE B2.CAMPCODE = A.CAMPCODE and B2.STAFFCODE = A.STAFFCODE and B2.STYMD <= A.YMD and B2.ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                   & "   and   B.DELFLG     <> '1' " _
                   & " LEFT JOIN MB001_STAFF B2 " _
                   & "   ON    B2.CAMPCODE    = @P1 " _
                   & "   and   B2.STAFFCODE   = A.SUBSTAFFCODE " _
                   & "   and   B2.STYMD      <= A.YMD " _
                   & "   and   B2.ENDYMD     >= A.YMD " _
                   & "   and   B2.STYMD       = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @P1 and STAFFCODE = A.SUBSTAFFCODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                   & "   and   B2.DELFLG     <> '1' " _
                   & " LEFT JOIN M0001_CAMP M1 " _
                   & "   ON    M1.CAMPCODE    = @P1 " _
                   & "   and   M1.STYMD      <= A.YMD " _
                   & "   and   M1.ENDYMD     >= A.YMD " _
                   & "   and   M1.STYMD       = (SELECT MAX(STYMD) FROM M0001_CAMP WHERE CAMPCODE = @P1 and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   M1.DELFLG     <> '1' " _
                   & " LEFT JOIN M0002_ORG M2 " _
                   & "   ON    M2.CAMPCODE    = @P1 " _
                   & "   and   M2.ORGCODE     = A.SHIPORG " _
                   & "   and   M2.STYMD      <= A.YMD " _
                   & "   and   M2.ENDYMD     >= A.YMD " _
                   & "   and   M2.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = @P1 and ORGCODE = A.SHIPORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   M2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC002_TORIHIKISAKI MC2 " _
                   & "   ON    MC2.TORICODE    = A.TORICODE " _
                   & "   and   MC2.CAMPCODE    = @P1 " _
                   & "   and   MC2.STYMD      <= A.YMD " _
                   & "   and   MC2.ENDYMD     >= A.YMD " _
                   & "   and   MC2.STYMD       = (SELECT MAX(STYMD) FROM MC002_TORIHIKISAKI WHERE TORICODE = A.TORICODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC002_TORIHIKISAKI MC22 " _
                   & "   ON    MC22.TORICODE    = A.STORICODE " _
                   & "   and   MC22.CAMPCODE    = @P1 " _
                   & "   and   MC22.STYMD      <= A.YMD " _
                   & "   and   MC22.ENDYMD     >= A.YMD " _
                   & "   and   MC22.STYMD       = (SELECT MAX(STYMD) FROM MC002_TORIHIKISAKI WHERE TORICODE = A.STORICODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC22.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6 " _
                   & "   ON    MC6.CAMPCODE    = A.CAMPCODE " _
                   & "   and   MC6.TORICODE    = A.TORICODE " _
                   & "   and   MC6.TODOKECODE  = A.TODOKECODE " _
                   & "   and   MC6.CLASS       = '1' " _
                   & "   and   MC6.STYMD      <= A.YMD " _
                   & "   and   MC6.ENDYMD     >= A.YMD " _
                   & "   and   MC6.STYMD       = (SELECT MAX(STYMD) FROM MC006_TODOKESAKI WHERE CAMPCODE = A.CAMPCODE and TORICODE = A.TORICODE and TODOKECODE = A.TODOKECODE and CLASS = '1' and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC6.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC62 " _
                   & "   ON    MC62.CAMPCODE    = A.CAMPCODE " _
                   & "   and   MC62.TODOKECODE  = A.SHUKABASHO " _
                   & "   and   MC62.CLASS       = '2' " _
                   & "   and   MC62.STYMD      <= A.YMD " _
                   & "   and   MC62.ENDYMD     >= A.YMD " _
                   & "   and   MC62.STYMD       = (SELECT MAX(STYMD) FROM MC006_TODOKESAKI WHERE CAMPCODE = A.CAMPCODE and TODOKECODE = A.SHUKABASHO and CLASS = '2' and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   MC62.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F1 " _
                   & "   ON    F1.CAMPCODE    = @P1 " _
                   & "   and   F1.CLASS       = 'TERMKBN' " _
                   & "   and   F1.KEYCODE     = A.TERMKBN " _
                   & "   and   F1.STYMD      <= A.YMD " _
                   & "   and   F1.ENDYMD     >= A.YMD " _
                   & "   and   F1.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F2 " _
                   & "   ON    F2.CAMPCODE    = @P1 " _
                   & "   and   F2.CLASS       = 'WORKKBN' " _
                   & "   and   F2.KEYCODE     = A.WORKKBN " _
                   & "   and   F2.STYMD      <= A.YMD " _
                   & "   and   F2.ENDYMD     >= A.YMD " _
                   & "   and   F2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F3 " _
                   & "   ON    F3.CAMPCODE    = @P1 " _
                   & "   and   F3.CLASS       = 'CREWKBN' " _
                   & "   and   F3.KEYCODE     = A.CREWKBN " _
                   & "   and   F3.STYMD      <= A.YMD " _
                   & "   and   F3.ENDYMD     >= A.YMD " _
                   & "   and   F3.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F41 " _
                   & "   ON    F41.CAMPCODE    = @P1 " _
                   & "   and   F41.CLASS       = 'STANI' " _
                   & "   and   F41.KEYCODE     = A.STANI1 " _
                   & "   and   F41.STYMD      <= A.YMD " _
                   & "   and   F41.ENDYMD     >= A.YMD " _
                   & "   and   F41.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F42 " _
                   & "   ON    F42.CAMPCODE    = @P1 " _
                   & "   and   F42.CLASS       = 'STANI' " _
                   & "   and   F42.KEYCODE     = A.STANI2 " _
                   & "   and   F42.STYMD      <= A.YMD " _
                   & "   and   F42.ENDYMD     >= A.YMD " _
                   & "   and   F42.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F43 " _
                   & "   ON    F43.CAMPCODE    = @P1 " _
                   & "   and   F43.CLASS       = 'STANI' " _
                   & "   and   F43.KEYCODE     = A.STANI3 " _
                   & "   and   F43.STYMD      <= A.YMD " _
                   & "   and   F43.ENDYMD     >= A.YMD " _
                   & "   and   F43.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F44 " _
                   & "   ON    F44.CAMPCODE    = @P1 " _
                   & "   and   F44.CLASS       = 'STANI' " _
                   & "   and   F44.KEYCODE     = A.STANI4 " _
                   & "   and   F44.STYMD      <= A.YMD " _
                   & "   and   F44.ENDYMD     >= A.YMD " _
                   & "   and   F44.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F45 " _
                   & "   ON    F45.CAMPCODE    = @P1 " _
                   & "   and   F45.CLASS       = 'STANI' " _
                   & "   and   F45.KEYCODE     = A.STANI5 " _
                   & "   and   F45.STYMD      <= A.YMD " _
                   & "   and   F45.ENDYMD     >= A.YMD " _
                   & "   and   F45.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F46 " _
                   & "   ON    F46.CAMPCODE    = @P1 " _
                   & "   and   F46.CLASS       = 'STANI' " _
                   & "   and   F46.KEYCODE     = A.STANI6 " _
                   & "   and   F46.STYMD      <= A.YMD " _
                   & "   and   F46.ENDYMD     >= A.YMD " _
                   & "   and   F46.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F47 " _
                   & "   ON    F47.CAMPCODE    = @P1 " _
                   & "   and   F47.CLASS       = 'STANI' " _
                   & "   and   F47.KEYCODE     = A.STANI7 " _
                   & "   and   F47.STYMD      <= A.YMD " _
                   & "   and   F47.ENDYMD     >= A.YMD " _
                   & "   and   F47.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F48 " _
                   & "   ON    F48.CAMPCODE    = @P1 " _
                   & "   and   F48.CLASS       = 'STANI' " _
                   & "   and   F48.KEYCODE     = A.STANI8 " _
                   & "   and   F48.STYMD      <= A.YMD " _
                   & "   and   F48.ENDYMD     >= A.YMD " _
                   & "   and   F48.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F5 " _
                   & "   ON    F5.CAMPCODE    = @P1 " _
                   & "   and   F5.CLASS       = 'TUMIOKIKBN' " _
                   & "   and   F5.KEYCODE     = A.TUMIOKIKBN " _
                   & "   and   F5.STYMD      <= A.YMD " _
                   & "   and   F5.ENDYMD     >= A.YMD " _
                   & "   and   F5.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F6 " _
                   & "   ON    F6.CAMPCODE    = @P1 " _
                   & "   and   F6.CLASS       = 'URIKBN' " _
                   & "   and   F6.KEYCODE     = A.URIKBN " _
                   & "   and   F6.STYMD      <= A.YMD " _
                   & "   and   F6.ENDYMD     >= A.YMD " _
                   & "   and   F6.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F7 " _
                   & "   ON    F7.CAMPCODE    = @P1 " _
                   & "   and   F7.CLASS       = 'TAXKBN' " _
                   & "   and   F7.KEYCODE     = A.TAXKBN " _
                   & "   and   F7.STYMD      <= A.YMD " _
                   & "   and   F7.ENDYMD     >= A.YMD " _
                   & "   and   F7.DELFLG     <> '1' " _
                   & " WHERE   " _
                   & "         A.CAMPCODE    = @P1 " _
                   & "   and   A.SHIPORG     = @P2 " _
                   & "   and   A.YMD        <= @P4 " _
                   & "   and   A.YMD        >= @P3 " _
                   & "   and   A.DELFLG     <> '1' "

                Dim SQLWhere As String = ""
                If Not String.IsNullOrEmpty(work.WF_SEL_STAFFCODE.Text) Then
                    SQLWhere = SQLWhere & " and A.STAFFCODE = '" & Trim(work.WF_SEL_STAFFCODE.Text) & "' "
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_STAFFNAME.Text) Then
                    SQLWhere = SQLWhere & " and B.STAFFNAMES like '%" & Trim(work.WF_SEL_STAFFNAME.Text) & "%' "
                End If
                If WW_SORT = "" OrElse String.IsNullOrEmpty(WW_SORT) Then
                    WW_SORT = "ORDER BY A.YMD , A.STAFFCODE , A.STDATE , A.STTIME"
                Else
                    WW_SORT = "ORDER BY " & WW_SORT
                End If

                Dim SQLStr2 As String = SQLStr & SQLWhere & WW_SORT
                Using SQLcmd As New SqlCommand(SQLStr2, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_UORG.Text
                    PARA3.Value = work.WF_SEL_STYMD.Text
                    PARA4.Value = work.WF_SEL_ENDYMD.Text
                    PARA5.Value = Date.Now
                    SQLcmd.CommandTimeout = 300
                    '----------------------------
                    '画面指定の開始日付～終了日付を取得
                    '----------------------------
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '〇データをテーブルに設定
                        T0011tbl.Load(SQLdr)
                        If T0011tbl.Rows.Count > 65000 Then
                            'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
                            Master.output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ABORT)
                            T0011tbl.Clear()
                            Exit Sub
                        End If

                    End Using

                End Using
                    '----------------------------
                    '一週間前の日報を取得
                    '----------------------------
                    WW_SORT = "ORDER BY A.YMD , A.STAFFCODE , A.STDATE , A.STTIME"

                SQLStr2 = SQLStr & WW_SORT
                Using SQLcmd2 As New SqlCommand(SQLStr2, SQLcon)
                    Dim PARA21 As SqlParameter = SQLcmd2.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA22 As SqlParameter = SQLcmd2.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA23 As SqlParameter = SQLcmd2.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA24 As SqlParameter = SQLcmd2.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    Dim PARA25 As SqlParameter = SQLcmd2.Parameters.Add("@P5", System.Data.SqlDbType.Date)

                    Dim WW_date As Date = Date.Parse(work.WF_SEL_STYMD.Text)
                    ' 一週間前
                    Dim WW_Fdate As String = WW_date.AddDays(-7).ToString("yyyy/MM/dd")
                    Dim WW_Tdate As String = WW_date.AddDays(-1).ToString("yyyy/MM/dd")

                    PARA21.Value = work.WF_SEL_CAMPCODE.Text
                    PARA22.Value = work.WF_SEL_UORG.Text
                    PARA23.Value = WW_Fdate
                    PARA24.Value = WW_Tdate
                    PARA25.Value = Date.Now
                    SQLcmd2.CommandTimeout = 300
                    Using SQLdr2 As SqlDataReader = SQLcmd2.ExecuteReader()

                        '■テーブル検索結果をテーブル退避
                        '日報DB更新用テーブル
                        T0005COM.AddColumnT0005tbl(T0011WEEKtbl)

                        T0011WEEKtbl.Load(SQLdr2)

                        '一週間前～開始日付－１日をマージ
                        T0011tbl.Merge(T0011WEEKtbl)
                    End Using

                    Using WW_T0011tbl As DataTable = T0011tbl.Clone
                        For i As Integer = 0 To T0011tbl.Rows.Count - 1
                            Dim T0011row As DataRow = WW_T0011tbl.NewRow
                            T0011row.ItemArray = T0011tbl.Rows(i).ItemArray

                            If IsDate(T0011row("YMD")) Then
                                T0011row("YMD") = CDate(T0011row("YMD")).ToString("yyyy/MM/dd")
                            Else
                                T0011row("YMD") = C_DEFAULT_YMD
                            End If

                            T0011row("SELECT") = "1"      '対象データ
                            T0011row("HIDDEN") = "1"      '非表示

                            T0011row("HDKBN") = "D"       'ヘッダ、明細区分
                            If IsDate(T0011row("SHUKADATE")) Then
                                T0011row("SHUKADATE") = CDate(T0011row("SHUKADATE")).ToString("yyyy/MM/dd")
                            End If
                            If IsDate(T0011row("TODOKEDATE")) Then
                                T0011row("TODOKEDATE") = CDate(T0011row("TODOKEDATE")).ToString("yyyy/MM/dd")
                            End If
                            T0011row("SEQ") = CInt(T0011row("SEQ")).ToString("000")
                            If IsDate(T0011row("STDATE")) Then
                                T0011row("STDATE") = CDate(T0011row("STDATE")).ToString("yyyy/MM/dd")
                            Else
                                T0011row("STDATE") = C_DEFAULT_YMD
                            End If
                            If IsDate(T0011row("STTIME")) Then
                                T0011row("STTIME") = CDate(T0011row("STTIME")).ToString("HH:mm")
                            Else
                                T0011row("STTIME") = "00:00"
                            End If
                            If IsDate(T0011row("ENDDATE")) Then
                                T0011row("ENDDATE") = CDate(T0011row("ENDDATE")).ToString("yyyy/MM/dd")
                            Else
                                T0011row("ENDDATE") = C_DEFAULT_YMD
                            End If
                            If IsDate(T0011row("STTIME")) Then
                                T0011row("ENDTIME") = CDate(T0011row("ENDTIME")).ToString("HH:mm")
                            Else
                                T0011row("ENDTIME") = "00:00"
                            End If
                            T0011row("WORKTIME") = T0005COM.MinutestoHHMM(T0011row("WORKTIME"))
                            T0011row("MOVETIME") = T0005COM.MinutestoHHMM(T0011row("MOVETIME"))
                            T0011row("ACTTIME") = T0005COM.MinutestoHHMM(T0011row("ACTTIME"))
                            T0011row("PRATE") = CInt(T0011row("PRATE")).ToString("#,0")

                            T0011row("CASH") = CInt(T0011row("CASH")).ToString("#,0")
                            T0011row("TICKET") = CInt(T0011row("TICKET")).ToString("#,0")
                            T0011row("ETC") = CInt(T0011row("ETC")).ToString("#,0")
                            T0011row("TOTALTOLL") = CInt(T0011row("TOTALTOLL")).ToString("#,0")
                            T0011row("STMATER") = Val(T0011row("STMATER")).ToString("#,0.00")
                            T0011row("ENDMATER") = Val(T0011row("ENDMATER")).ToString("#,0.00")
                            T0011row("RUIDISTANCE") = Val(T0011row("RUIDISTANCE")).ToString("#,0.00")
                            T0011row("SOUDISTANCE") = Val(T0011row("SOUDISTANCE")).ToString("#,0.00")
                            T0011row("JIDISTANCE") = Val(T0011row("JIDISTANCE")).ToString("#,0.00")
                            T0011row("KUDISTANCE") = Val(T0011row("KUDISTANCE")).ToString("#,0.00")
                            T0011row("IPPDISTANCE") = Val(T0011row("IPPDISTANCE")).ToString("#,0.00")
                            T0011row("KOSDISTANCE") = Val(T0011row("KOSDISTANCE")).ToString("#,0.00")
                            T0011row("IPPJIDISTANCE") = Val(T0011row("IPPJIDISTANCE")).ToString("#,0.00")
                            T0011row("IPPKUDISTANCE") = Val(T0011row("IPPKUDISTANCE")).ToString("#,0.00")
                            T0011row("KOSJIDISTANCE") = Val(T0011row("KOSJIDISTANCE")).ToString("#,0.00")
                            T0011row("KOSKUDISTANCE") = Val(T0011row("KOSKUDISTANCE")).ToString("#,0.00")
                            T0011row("KYUYU") = Val(T0011row("KYUYU")).ToString("#,0.00")
                            T0011row("SURYO1") = Val(T0011row("SURYO1")).ToString("#,0.000")
                            T0011row("SURYO2") = Val(T0011row("SURYO2")).ToString("#,0.000")
                            T0011row("SURYO3") = Val(T0011row("SURYO3")).ToString("#,0.000")
                            T0011row("SURYO4") = Val(T0011row("SURYO4")).ToString("#,0.000")
                            T0011row("SURYO5") = Val(T0011row("SURYO5")).ToString("#,0.000")
                            T0011row("SURYO6") = Val(T0011row("SURYO6")).ToString("#,0.000")
                            T0011row("SURYO7") = Val(T0011row("SURYO7")).ToString("#,0.000")
                            T0011row("SURYO8") = Val(T0011row("SURYO8")).ToString("#,0.000")
                            T0011row("TOTALSURYO") = Val(T0011row("TOTALSURYO")).ToString("#,0.000")

                            Dim WW_PRODUCT As String = ""
                            WW_PRODUCT = T0011row("PRODUCTCODE1")
                            If WW_PRODUCT <> "" Then
                                T0011row("PRODUCT1NAMES") = ""
                                CODENAME_get("PRODUCT2", WW_PRODUCT, T0011row("PRODUCT1NAMES"), WW_DUMMY)
                            End If
                            WW_PRODUCT = T0011row("PRODUCTCODE2")
                            If WW_PRODUCT <> "" Then
                                T0011row("PRODUCT2NAMES") = ""
                                CODENAME_get("PRODUCT2", WW_PRODUCT, T0011row("PRODUCT2NAMES"), WW_DUMMY)
                            End If
                            WW_PRODUCT = T0011row("PRODUCTCODE3")
                            If WW_PRODUCT <> "" Then
                                T0011row("PRODUCT3NAMES") = ""
                                CODENAME_get("PRODUCT2", WW_PRODUCT, T0011row("PRODUCT3NAMES"), WW_DUMMY)
                            End If
                            WW_PRODUCT = T0011row("PRODUCTCODE4")
                            If WW_PRODUCT <> "" Then
                                T0011row("PRODUCT4NAMES") = ""
                                CODENAME_get("PRODUCT2", WW_PRODUCT, T0011row("PRODUCT4NAMES"), WW_DUMMY)
                            End If
                            WW_PRODUCT = T0011row("PRODUCTCODE5")
                            If WW_PRODUCT <> "" Then
                                T0011row("PRODUCT5NAMES") = ""
                                CODENAME_get("PRODUCT2", WW_PRODUCT, T0011row("PRODUCT5NAMES"), WW_DUMMY)
                            End If
                            WW_PRODUCT = T0011row("PRODUCTCODE6")
                            If WW_PRODUCT <> "" Then
                                T0011row("PRODUCT6NAMES") = ""
                                CODENAME_get("PRODUCT2", WW_PRODUCT, T0011row("PRODUCT6NAMES"), WW_DUMMY)
                            End If
                            WW_PRODUCT = T0011row("PRODUCTCODE7")
                            If WW_PRODUCT <> "" Then
                                T0011row("PRODUCT7NAMES") = ""
                                CODENAME_get("PRODUCT2", WW_PRODUCT, T0011row("PRODUCT7NAMES"), WW_DUMMY)
                            End If
                            WW_PRODUCT = T0011row("PRODUCTCODE8")
                            If WW_PRODUCT <> "" Then
                                T0011row("PRODUCT8NAMES") = ""
                                CODENAME_get("PRODUCT2", WW_PRODUCT, T0011row("PRODUCT8NAMES"), WW_DUMMY)
                            End If
                            WW_T0011tbl.Rows.Add(T0011row)
                        Next

                        T0011tbl = WW_T0011tbl.Copy

                    End Using

                End Using
            End Using

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0005_NIPPO SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
        '--------------------------------------------
        '連番の設定
        '--------------------------------------------
        '○絞り込み操作（GridView明細Hidden設定）
        Dim WW_LINECNT As Integer = 0
        For Each T0011row As DataRow In T0011tbl.Select("HDKBN='D' and WORKKBN='B3' ")
            If T0011row("SELECT") = 1 Then
                WW_LINECNT = WW_LINECNT + 1
                T0011row("LINECNT") = WW_LINECNT
                T0011row("HIDDEN") = "0"
            End If
        Next
        '--------------------------------------------
        'ヘッダレコード作成
        '--------------------------------------------
        Dim WW_Filter As String = ""

        '一週間前データを分離
        CS0026TBLSORT.TABLE = T0011tbl
        CS0026TBLSORT.FILTER = "YMD < #" & work.WF_SEL_STYMD.Text & "#"
        CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        T0011WEEKtbl = CS0026TBLSORT.sort()
        '指定日データを分離
        CS0026TBLSORT.FILTER = "YMD >=  #" & work.WF_SEL_STYMD.Text & "#"
        T0011tbl = CS0026TBLSORT.sort()
        'ヘッダ作成
        T0011_CreHead_new(T0011WEEKtbl)
        'ヘッダ作成
        T0011_CreHead_new(T0011tbl)

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○GridViewデータをテーブルに保存（一週間前データ）
        If Not Master.SaveTable(T0011WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

        '絞込みボタン処理（GridViewの表示）を行う
        WF_ButtonExtract_Click()

    End Sub

    ''' <summary>
    ''' ヘッダー情報の繁栄
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    Protected Sub T0011_HeadToDetail(ByRef IO_TBL As DataTable)
        '出庫日、乗務員でグループ化しキーテーブル作成
        CS0026TBLSORT.TABLE = IO_TBL
        CS0026TBLSORT.FILTER = "HDKBN = 'H' and SELECT = '1'"
        CS0026TBLSORT.SORTING = "YMD, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        Dim WW_T0011_HEAD As DataTable = CS0026TBLSORT.sort()
        Dim WW_UPDTBL As DataTable = IO_TBL.Clone

        For Each WW_T0011_HEADROW As DataRow In WW_T0011_HEAD.Rows
            If WW_T0011_HEADROW("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then Continue For
            For Each WW_SELROW As DataRow In IO_TBL.Rows
                'ヘッダー情報と同じ明細があった場合
                If WW_T0011_HEADROW("YMD") = WW_SELROW("YMD") AndAlso
                   WW_T0011_HEADROW("STAFFCODE") = WW_SELROW("STAFFCODE") AndAlso
                   WW_SELROW("DELFLG") <> C_DELETE_FLG.DELETE Then

                    If WW_T0011_HEADROW("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_UPDROW As DataRow = WW_UPDTBL.NewRow
                        WW_UPDROW.ItemArray = WW_SELROW.ItemArray
                        '既存行の削除
                        WW_SELROW("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        WW_SELROW("TIMSTP") = WW_T0011_HEADROW("TIMSTP")
                        WW_SELROW("DELFLG") = C_DELETE_FLG.DELETE
                        WW_SELROW("SELECT") = "0"
                        WW_SELROW("LINECNT") = 0
                        WW_SELROW("HIDDEN") = "1"
                        '新規行の追加
                        WW_UPDROW("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        WW_UPDROW("TIMSTP") = "0"
                        WW_UPDROW("DELFLG") = C_DELETE_FLG.ALIVE
                        WW_UPDTBL.Rows.Add(WW_UPDROW)
                    ElseIf WW_T0011_HEADROW("OPERATION") = C_LIST_OPERATION_CODE.ERRORED AndAlso
                           WW_SELROW("OPERATION") <> C_LIST_OPERATION_CODE.NODATA Then
                        '既存行の更新
                        WW_SELROW("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

                    End If

                End If
            Next
        Next
        '〇追加情報をマージ
        IO_TBL.Merge(WW_UPDTBL)
        CS0026TBLSORT.TABLE = IO_TBL
        CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        CS0026TBLSORT.FILTER = ""
        IO_TBL = CS0026TBLSORT.sort
    End Sub


    ''' <summary>
    ''' ヘッダーレコード作成★×
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Protected Sub T0011_CreHead(ByRef IO_TBL As DataTable)

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_Cols As String() = {"YMD", "STAFFCODE"}
        Dim WW_T0011tbl As DataTable = IO_TBL.Clone
        Dim WW_TBLview As DataView
        Dim WW_T0011row As DataRow

        Try
            '更新元（削除）データをキープ
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            CS0026TBLSORT.SORTING = "SELECT"
            Dim WW_T0011DELtbl As DataTable = CS0026TBLSORT.sort()

            '出庫日、乗務員でグループ化しキーテーブル作成
            CS0026TBLSORT.FILTER = "HDKBN = 'D' and SELECT = '1'"
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            Dim WW_T0011SELtbl As DataTable = CS0026TBLSORT.sort()
            WW_TBLview = New DataView(WW_T0011SELtbl)
            '抽出後のテーブルに置き換える（ヘッダなし、明細のみ）
            IO_TBL = WW_T0011SELtbl.Copy()
            'キーテーブル作成
            Dim WW_KEYtbl As DataTable = WW_TBLview.ToTable(True, WW_Cols)

            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                Dim WW_FIRST As Boolean = False
                Dim WW_TOTALTOLL As Decimal = 0                             '通行料合計
                Dim WW_SOUDISTANCE As Decimal = 0                           '走行距離
                Dim WW_JIDISTANCE As Decimal = 0                            '実車距離
                Dim WW_KUDISTANCE As Decimal = 0                            '空車距離
                Dim WW_IPPDISTANCE As Decimal = 0                           '一般走行距離
                Dim WW_KOSDISTANCE As Decimal = 0                           '高速走行距離
                Dim WW_IPPJIDISTANCE As Decimal = 0                         '一般・実車距離
                Dim WW_IPPKUDISTANCE As Decimal = 0                         '一般・空車距離
                Dim WW_KOSJIDISTANCE As Decimal = 0                         '高速・実車距離
                Dim WW_KOSKUDISTANCE As Decimal = 0                         '高速・空車距離
                Dim WW_KYUYU As Decimal = 0                                 '給油
                Dim WW_STORICODE As String = ""                             '請求取引先コード
                Dim WW_CONTCHASSIS As String = ""                           'コンテナシャーシ
                Dim WW_OPE_UPD As Boolean = False
                Dim WW_OPE_ERR As Boolean = False
                Dim WW_ALIVE_FLG As Boolean = False

                '初期化
                WW_T0011row = WW_T0011tbl.NewRow
                T0005COM.InitialT5INPRow(WW_T0011row)
                WW_T0011row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                WW_T0011row("SHIPORG") = work.WF_SEL_UORG.Text

                For i As Integer = WW_IDX To WW_T0011SELtbl.Rows.Count - 1
                    Dim WW_SELrow As DataRow = WW_T0011SELtbl.Rows(i)
                    If WW_KEYrow("YMD") = WW_SELrow("YMD") AndAlso
                       WW_KEYrow("STAFFCODE") = WW_SELrow("STAFFCODE") Then
                        If WW_SELrow("DELFLG") = C_DELETE_FLG.ALIVE Then
                            If Not WW_FIRST Then
                                WW_FIRST = True
                                '先頭レコードより開始日、開始時間を取得
                                WW_T0011row("STDATE") = WW_SELrow("STDATE")
                                WW_T0011row("STTIME") = WW_SELrow("STTIME")
                                WW_T0011row("TERMKBN") = WW_SELrow("TERMKBN")
                                WW_T0011row("CREWKBN") = WW_SELrow("CREWKBN")
                                WW_T0011row("SUBSTAFFCODE") = WW_SELrow("SUBSTAFFCODE")
                                WW_T0011row("JISSKIKBN") = WW_SELrow("JISSKIKBN")
                            End If

                            '最終レコードの終了日、終了時間を取得
                            WW_T0011row("ENDDATE") = WW_SELrow("ENDDATE")
                            WW_T0011row("ENDTIME") = WW_SELrow("ENDTIME")

                            If WW_SELrow("WORKKBN") = "F3" Then
                                WW_TOTALTOLL = WW_TOTALTOLL + Val(WW_SELrow("TOTALTOLL").replace(",", ""))
                                WW_KYUYU = WW_KYUYU + Val(WW_SELrow("KYUYU").replace(",", ""))
                                WW_SOUDISTANCE = WW_SOUDISTANCE + Val(WW_SELrow("SOUDISTANCE").replace(",", ""))
                                WW_JIDISTANCE = WW_JIDISTANCE + Val(WW_SELrow("JIDISTANCE").replace(",", ""))
                                WW_KUDISTANCE = WW_KUDISTANCE + Val(WW_SELrow("KUDISTANCE").replace(",", ""))
                                WW_IPPDISTANCE = WW_IPPDISTANCE + Val(WW_SELrow("IPPDISTANCE").replace(",", ""))
                                WW_KOSDISTANCE = WW_KOSDISTANCE + Val(WW_SELrow("KOSDISTANCE").replace(",", ""))
                                WW_IPPJIDISTANCE = WW_IPPJIDISTANCE + Val(WW_SELrow("IPPJIDISTANCE").replace(",", ""))
                                WW_IPPKUDISTANCE = WW_IPPKUDISTANCE + Val(WW_SELrow("IPPKUDISTANCE").replace(",", ""))
                                WW_KOSJIDISTANCE = WW_KOSJIDISTANCE + Val(WW_SELrow("KOSJIDISTANCE").replace(",", ""))
                                WW_KOSKUDISTANCE = WW_KOSKUDISTANCE + Val(WW_SELrow("KOSKUDISTANCE").replace(",", ""))
                            End If

                            'タイムスタンプがゼロ以外が存在する場合、ヘッダにもとりあえずタイムスタンプ設定
                            'ヘッダで、ＤＢ登録済のデータか、初取込データ（新規を含む）かを判断できるようにする
                            If WW_SELrow("TIMSTP") <> "0" Then
                                WW_T0011row("TIMSTP") = WW_SELrow("TIMSTP")
                            End If
                        End If

                        If WW_SELrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                            WW_OPE_UPD = True
                        End If
                        If WW_SELrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                            WW_OPE_ERR = True
                        End If
                        If WW_SELrow("DELFLG") = C_DELETE_FLG.ALIVE Then
                            WW_ALIVE_FLG = True
                        End If
                    Else
                        WW_IDX = i
                        Exit For
                    End If
                Next

                If WW_OPE_ERR Then
                    WW_T0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                ElseIf WW_OPE_UPD Then
                    WW_T0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Else
                    WW_T0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                End If
                WW_T0011row("LINECNT") = "0"
                WW_T0011row("YMD") = WW_KEYrow("YMD")
                WW_T0011row("STAFFCODE") = WW_KEYrow("STAFFCODE")
                WW_T0011row("SELECT") = "1"
                WW_T0011row("HIDDEN") = "1"
                WW_T0011row("HDKBN") = "H"
                WW_T0011row("SEQ") = "001"
                If WW_ALIVE_FLG Then
                    WW_T0011row("DELFLG") = C_DELETE_FLG.ALIVE
                Else
                    WW_T0011row("DELFLG") = C_DELETE_FLG.DELETE
                End If
                Dim WW_WORKTIME As Integer = 0

                '作業時間
                WW_WORKTIME = DateDiff("n",
                                      WW_T0011row("STDATE") + " " + WW_T0011row("STTIME"),
                                      WW_T0011row("ENDDATE") + " " + WW_T0011row("ENDTIME")
                                     )
                WW_T0011row("WORKTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
                WW_T0011row("ACTTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
                WW_T0011row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                WW_T0011row("KYUYU") = Val(WW_KYUYU).ToString("#,0.00")
                WW_T0011row("TOTALTOLL") = Val(WW_TOTALTOLL).ToString("#,0")

                WW_T0011row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                WW_T0011row("JIDISTANCE") = Val(WW_JIDISTANCE).ToString("#,0.00")
                WW_T0011row("KUDISTANCE") = Val(WW_KUDISTANCE).ToString("#,0.00")
                WW_T0011row("IPPDISTANCE") = Val(WW_IPPDISTANCE).ToString("#,0.00")
                WW_T0011row("KOSDISTANCE") = Val(WW_KOSDISTANCE).ToString("#,0.00")
                WW_T0011row("IPPJIDISTANCE") = Val(WW_IPPJIDISTANCE).ToString("#,0.00")
                WW_T0011row("IPPKUDISTANCE") = Val(WW_IPPKUDISTANCE).ToString("#,0.00")
                WW_T0011row("KOSJIDISTANCE") = Val(WW_KOSJIDISTANCE).ToString("#,0.00")
                WW_T0011row("KOSKUDISTANCE") = Val(WW_KOSKUDISTANCE).ToString("#,0.00")

                WW_T0011row("CAMPNAMES") = ""
                CODENAME_get("CAMPCODE", WW_T0011row("CAMPCODE"), WW_T0011row("CAMPNAMES"), WW_RTN)
                WW_T0011row("SHIPORGNAMES") = ""
                CODENAME_get("SHIPORG", WW_T0011row("SHIPORG"), WW_T0011row("SHIPORGNAMES"), WW_RTN)
                WW_T0011row("TERMKBNNAMES") = ""
                CODENAME_get("TERMKBN", WW_T0011row("TERMKBN"), WW_T0011row("TERMKBNNAMES"), WW_RTN)
                WW_T0011row("STAFFNAMES") = ""
                CODENAME_get("STAFFCODE", WW_T0011row("STAFFCODE"), WW_T0011row("STAFFNAMES"), WW_RTN)
                WW_T0011row("SUBSTAFFNAMES") = ""
                CODENAME_get("STAFFCODE", WW_T0011row("SUBSTAFFCODE"), WW_T0011row("SUBSTAFFNAMES"), WW_RTN)
                WW_T0011row("CREWKBNNAMES") = ""
                CODENAME_get("CREWKBN", WW_T0011row("CREWKBN"), WW_T0011row("CREWKBNNAMES"), WW_RTN)
                WW_T0011row("JISSKIKBNNAMES") = ""
                CODENAME_get("JISSKIKBN", WW_T0011row("JISSKIKBN"), WW_T0011row("JISSKIKBNNAMES"), WW_RTN)

                WW_T0011tbl.Rows.Add(WW_T0011row)
            Next

            'ヘッダのマージ
            IO_TBL.Merge(WW_T0011tbl)

            '更新元（削除）データの戻し
            IO_TBL.Merge(WW_T0011DELtbl)

            'ソート
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.FILTER = ""
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            IO_TBL = CS0026TBLSORT.sort()

            WW_KEYtbl.Dispose()
            WW_KEYtbl = Nothing
            WW_TBLview.Dispose()
            WW_TBLview = Nothing
            WW_T0011DELtbl.Dispose()
            WW_T0011DELtbl = Nothing
            WW_T0011SELtbl.Dispose()
            WW_T0011SELtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0011_CreHead"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' ヘッダレコード作成new     ★済
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Protected Sub T0011_CreHead_new(ByRef IO_TBL As DataTable)

        Dim WW_IDX As Integer = 0
        Dim WW_WORKTIME As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_T0011tbl As DataTable = IO_TBL.Clone
        Dim WW_T0011DELtbl As DataTable
        Dim WW_T0011SELtbl As DataTable
        Dim WW_T0011row As DataRow

        Try
            '更新元（削除）データをキープ
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.SORTING = "SELECT"
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            WW_T0011DELtbl = CS0026TBLSORT.sort()

            '出庫日、乗務員でグループ化しキーテーブル作成
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "HDKBN = 'D' and SELECT = '1'"
            WW_T0011SELtbl = CS0026TBLSORT.sort()

            '抽出後のテーブルに置き換える（ヘッダなし、明細のみ）
            IO_TBL = WW_T0011SELtbl.Copy()

            'BreakKey
            Dim WW_YMD As String = ""
            Dim WW_STAFFCODE As String = ""

            Dim WW_FIRST As String = "OFF"
            Dim WW_FIRST2 As String = "OFF"
            Dim WW_OPE_UPD As String = "OFF"
            Dim WW_OPE_ERR As String = "OFF"
            Dim WW_DEL_FLG As String = "OFF"

            Dim WW_TOTALTOLL As Decimal = 0                             '通行料合計
            Dim WW_SOUDISTANCE As Decimal = 0                           '走行距離
            Dim WW_JIDISTANCE As Decimal = 0                            '実車距離
            Dim WW_KUDISTANCE As Decimal = 0                            '空車距離
            Dim WW_IPPDISTANCE As Decimal = 0                           '一般走行距離
            Dim WW_KOSDISTANCE As Decimal = 0                           '高速走行距離
            Dim WW_IPPJIDISTANCE As Decimal = 0                         '一般・実車距離
            Dim WW_IPPKUDISTANCE As Decimal = 0                         '一般・空車距離
            Dim WW_KOSJIDISTANCE As Decimal = 0                         '高速・実車距離
            Dim WW_KOSKUDISTANCE As Decimal = 0                         '高速・空車距離
            Dim WW_KYUYU As Decimal = 0                                 '給油
            Dim WW_STORICODE As String = ""                             '請求取引先コード
            Dim WW_CONTCHASSIS As String = ""                           'コンテナシャーシ

            '初期化
            WW_T0011row = WW_T0011tbl.NewRow
            T0005COM.InitialT5INPRow(WW_T0011row)
            WW_T0011row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            WW_T0011row("SHIPORG") = work.WF_SEL_UORG.Text

            Dim wIO_TBL As DataRow = IO_TBL.NewRow
            For i As Integer = 0 To IO_TBL.Rows.Count - 1
                wIO_TBL = IO_TBL.Rows(i)

                If wIO_TBL("DELFLG") <> C_DELETE_FLG.DELETE Then

                    '先頭レコード
                    If wIO_TBL("YMD") <> WW_YMD OrElse wIO_TBL("STAFFCODE") <> WW_STAFFCODE Then

                        'KeyBreak処理
                        If WW_YMD <> "" Then
                            WW_T0011row("LINECNT") = "0"
                            WW_T0011tbl.Rows.Add(WW_T0011row)
                        End If

                        '初期化レコード準備
                        WW_FIRST = "OFF"
                        WW_FIRST2 = "OFF"
                        WW_OPE_UPD = "OFF"
                        WW_OPE_ERR = "OFF"
                        WW_DEL_FLG = "OFF"

                        WW_TOTALTOLL = 0                             '通行料合計
                        WW_SOUDISTANCE = 0                           '走行距離
                        WW_JIDISTANCE = 0                            '実車距離
                        WW_KUDISTANCE = 0                            '空車距離
                        WW_IPPDISTANCE = 0                           '一般走行距離
                        WW_KOSDISTANCE = 0                           '高速走行距離
                        WW_IPPJIDISTANCE = 0                         '一般・実車距離
                        WW_IPPKUDISTANCE = 0                         '一般・空車距離
                        WW_KOSJIDISTANCE = 0                         '高速・実車距離
                        WW_KOSKUDISTANCE = 0                         '高速・空車距離
                        WW_KYUYU = 0                                 '給油
                        WW_STORICODE = ""                             '請求取引先コード
                        WW_CONTCHASSIS = ""                           'コンテナシャーシ

                        WW_T0011row = WW_T0011tbl.NewRow
                        T0005COM.InitialT5INPRow(WW_T0011row)

                        WW_YMD = wIO_TBL("YMD")
                        WW_STAFFCODE = wIO_TBL("STAFFCODE")

                        'ヘッダー項目
                        WW_T0011row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                        WW_T0011row("YMD") = wIO_TBL("YMD")
                        WW_T0011row("SHIPORG") = work.WF_SEL_UORG.Text
                        WW_T0011row("STAFFCODE") = wIO_TBL("STAFFCODE")
                        WW_T0011row("LINECNT") = 0
                        WW_T0011row("SELECT") = "1"
                        WW_T0011row("HIDDEN") = "1"
                        WW_T0011row("HDKBN") = "H"
                        WW_T0011row("SEQ") = "001"
                        '開始日、開始時間を取得
                        WW_T0011row("STDATE") = wIO_TBL("STDATE")
                        WW_T0011row("STTIME") = wIO_TBL("STTIME")
                        WW_T0011row("TERMKBN") = wIO_TBL("TERMKBN")
                        WW_T0011row("CREWKBN") = wIO_TBL("CREWKBN")
                        WW_T0011row("SUBSTAFFCODE") = wIO_TBL("SUBSTAFFCODE")
                        WW_T0011row("JISSKIKBN") = wIO_TBL("JISSKIKBN")
                    End If

                    '終了日、終了時間
                    WW_T0011row("ENDDATE") = wIO_TBL("ENDDATE")
                    WW_T0011row("ENDTIME") = wIO_TBL("ENDTIME")

                    '出庫
                    If wIO_TBL("WORKKBN") = "F1" Then
                        WW_T0011row("STMATER") = wIO_TBL("STMATER")
                    End If

                    '帰庫
                    If wIO_TBL("WORKKBN") = "F3" Then
                        WW_T0011row("ENDMATER") = wIO_TBL("ENDMATER")
                        WW_T0011row("RUIDISTANCE") = wIO_TBL("RUIDISTANCE")
                    End If

                    If wIO_TBL("WORKKBN") = "F3" Then
                        WW_TOTALTOLL = WW_TOTALTOLL + Val(wIO_TBL("TOTALTOLL").replace(",", ""))
                        WW_KYUYU = WW_KYUYU + Val(wIO_TBL("KYUYU").replace(",", ""))
                        WW_SOUDISTANCE = WW_SOUDISTANCE + Val(wIO_TBL("SOUDISTANCE").replace(",", ""))
                        WW_JIDISTANCE = WW_JIDISTANCE + Val(wIO_TBL("JIDISTANCE").replace(",", ""))
                        WW_KUDISTANCE = WW_KUDISTANCE + Val(wIO_TBL("KUDISTANCE").replace(",", ""))
                        WW_IPPDISTANCE = WW_IPPDISTANCE + Val(wIO_TBL("IPPDISTANCE").replace(",", ""))
                        WW_KOSDISTANCE = WW_KOSDISTANCE + Val(wIO_TBL("KOSDISTANCE").replace(",", ""))
                        WW_IPPJIDISTANCE = WW_IPPJIDISTANCE + Val(wIO_TBL("IPPJIDISTANCE").replace(",", ""))
                        WW_IPPKUDISTANCE = WW_IPPKUDISTANCE + Val(wIO_TBL("IPPKUDISTANCE").replace(",", ""))
                        WW_KOSJIDISTANCE = WW_KOSJIDISTANCE + Val(wIO_TBL("KOSJIDISTANCE").replace(",", ""))
                        WW_KOSKUDISTANCE = WW_KOSKUDISTANCE + Val(wIO_TBL("KOSKUDISTANCE").replace(",", ""))
                    End If
                    WW_WORKTIME = DateDiff("n",
                                          WW_T0011row("STDATE") + " " + WW_T0011row("STTIME"),
                                          WW_T0011row("ENDDATE") + " " + WW_T0011row("ENDTIME")
                                         )
                    WW_T0011row("WORKTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
                    WW_T0011row("ACTTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
                    WW_T0011row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                    WW_T0011row("KYUYU") = Val(WW_KYUYU).ToString("#,0.00")
                    WW_T0011row("TOTALTOLL") = Val(WW_TOTALTOLL).ToString("#,0")

                    WW_T0011row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                    WW_T0011row("JIDISTANCE") = Val(WW_JIDISTANCE).ToString("#,0.00")
                    WW_T0011row("KUDISTANCE") = Val(WW_KUDISTANCE).ToString("#,0.00")
                    WW_T0011row("IPPDISTANCE") = Val(WW_IPPDISTANCE).ToString("#,0.00")
                    WW_T0011row("KOSDISTANCE") = Val(WW_KOSDISTANCE).ToString("#,0.00")
                    WW_T0011row("IPPJIDISTANCE") = Val(WW_IPPJIDISTANCE).ToString("#,0.00")
                    WW_T0011row("IPPKUDISTANCE") = Val(WW_IPPKUDISTANCE).ToString("#,0.00")
                    WW_T0011row("KOSJIDISTANCE") = Val(WW_KOSJIDISTANCE).ToString("#,0.00")
                    WW_T0011row("KOSKUDISTANCE") = Val(WW_KOSKUDISTANCE).ToString("#,0.00")

                    'タイムスタンプがゼロ以外が存在する場合、ヘッダにもとりあえずタイムスタンプ設定
                    'ヘッダで、ＤＢ登録済のデータか、初取込データ（新規を含む）かを判断できるようにする
                    If wIO_TBL("TIMSTP") <> "0" Then
                        WW_T0011row("TIMSTP") = wIO_TBL("TIMSTP")
                    End If

                    If wIO_TBL("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                        WW_OPE_UPD = "ON"
                    End If
                    If wIO_TBL("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                        WW_OPE_ERR = "ON"
                    End If
                    If wIO_TBL("DELFLG") = C_DELETE_FLG.ALIVE Then
                        WW_DEL_FLG = "ON"
                    End If

                    If WW_OPE_ERR = "ON" Then
                        WW_T0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    ElseIf WW_OPE_UPD = "ON" Then
                        WW_T0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    Else
                        WW_T0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    End If
                    If WW_DEL_FLG = "ON" Then
                        WW_T0011row("DELFLG") = C_DELETE_FLG.ALIVE
                    Else
                        WW_T0011row("DELFLG") = C_DELETE_FLG.DELETE
                    End If

                    '名称
                    WW_T0011row("CAMPNAMES") = ""
                    CODENAME_get("CAMPCODE", WW_T0011row("CAMPCODE"), WW_T0011row("CAMPNAMES"), WW_RTN)
                    WW_T0011row("SHIPORGNAMES") = ""
                    CODENAME_get("SHIPORG", WW_T0011row("SHIPORG"), WW_T0011row("SHIPORGNAMES"), WW_RTN)
                    WW_T0011row("TERMKBNNAMES") = ""
                    CODENAME_get("TERMKBN", WW_T0011row("TERMKBN"), WW_T0011row("TERMKBNNAMES"), WW_RTN)
                    WW_T0011row("STAFFNAMES") = ""
                    CODENAME_get("STAFFCODE", WW_T0011row("STAFFCODE"), WW_T0011row("STAFFNAMES"), WW_RTN)
                    WW_T0011row("SUBSTAFFNAMES") = ""
                    CODENAME_get("STAFFCODE", WW_T0011row("SUBSTAFFCODE"), WW_T0011row("SUBSTAFFNAMES"), WW_RTN)
                    WW_T0011row("CREWKBNNAMES") = ""
                    CODENAME_get("CREWKBN", WW_T0011row("CREWKBN"), WW_T0011row("CREWKBNNAMES"), WW_RTN)
                    WW_T0011row("JISSKIKBNNAMES") = ""
                    CODENAME_get("JISSKIKBN", WW_T0011row("JISSKIKBN"), WW_T0011row("JISSKIKBNNAMES"), WW_RTN)
                End If
            Next

            'ヘッダのマージ
            IO_TBL.Merge(WW_T0011tbl)

            '更新元（削除）データの戻し
            IO_TBL.Merge(WW_T0011DELtbl)

            'ソート
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            IO_TBL = CS0026TBLSORT.sort()


            WW_T0011DELtbl.Dispose()
            WW_T0011DELtbl = Nothing
            WW_T0011SELtbl.Dispose()
            WW_T0011SELtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0011_CreHead"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub


    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="WW_LINEerr"></param>
    ''' <param name="T0011INProw"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub ERRMSG_write(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByRef WW_LINEerr As String, ByRef T0011INProw As DataRow, ByVal I_ERRCD As String)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 項番        =@L" & T0011INProw("YMD") & T0011INProw("STAFFCODE") & "L@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日      =" & T0011INProw("YMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員コード=" & T0011INProw("STAFFCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員      =" & T0011INProw("STAFFNAMES") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 作業区分    =" & T0011INProw("WORKKBNNAMES") & "  "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日報№      =" & T0011INProw("NIPPONO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細行番号  =" & T0011INProw("SEQ") & " , "
        ErrMsgSet(WW_ERR_MES)
        If WW_LINEerr <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            WW_LINEerr = I_ERRCD
        End If

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    'EXCEL取込み処理
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        T0005COM.AddColumnT0005tbl(T0011INPtbl)

        '■■■ UPLOAD_XLSデータ取得 ■■■ 
        CS0023XLSTBL.MAPID = Master.MAPID
        CS0023XLSTBL.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSTBL.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSTBL.ERR) Then
            If CS0023XLSTBL.TBLDATA.Rows.Count = 0 Then
                Master.output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR, "例外発生")

                Exit Sub
            End If
        Else
            Master.output(CS0023XLSTBL.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSTBL")

            Exit Sub
        End If

        'EXCELデータの初期化（DBNullを撲滅）
        Dim CS0023XLSTBLrow As DataRow = CS0023XLSTBL.TBLDATA.NewRow
        For i As Integer = 0 To CS0023XLSTBL.TBLDATA.Rows.Count - 1
            CS0023XLSTBLrow.ItemArray = CS0023XLSTBL.TBLDATA.Rows(i).ItemArray

            For j As Integer = 0 To CS0023XLSTBL.TBLDATA.Columns.Count - 1
                If IsDBNull(CS0023XLSTBLrow.Item(j)) OrElse IsNothing(CS0023XLSTBLrow.Item(j)) Then
                    CS0023XLSTBLrow.Item(j) = ""
                End If
            Next
            CS0023XLSTBL.TBLDATA.Rows(i).ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        '○CS0023XLSTBL.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For i As Integer = 0 To CS0023XLSTBL.TBLDATA.Columns.Count - 1
            WW_COLUMNS.Add(CS0023XLSTBL.TBLDATA.Columns.Item(i).ColumnName.ToString)
        Next

        '■■■ エラーレポート準備 ■■■
        Dim WW_RTN As String = ""
        Dim WW_DATE As Date

        '○ 初期処理
        rightview.setErrorReport("")

        '○T0011INPtblカラム設定
        T0005COM.AddColumnT0005tbl(T0011INPtbl)

        Dim WW_TEXT As String = ""
        Dim WW_VALUE As String = ""

        '■■■ Excelデータ毎にチェック＆更新 ■■■
        For i As Integer = 0 To CS0023XLSTBL.TBLDATA.Rows.Count - 1

            '○XLSTBL明細⇒T0011INProw
            Dim T0011INProw As DataRow = T0011INPtbl.NewRow
            '○初期クリア
            T0005COM.InitialT5INPRow(T0011INProw)

            T0011INProw("LINECNT") = 0
            T0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            T0011INProw("TIMSTP") = "0"
            T0011INProw("SELECT") = 1
            T0011INProw("HIDDEN") = 1

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                T0011INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            Else
                T0011INProw("CAMPCODE") = CS0023XLSTBL.TBLDATA.Rows(i)("CAMPCODE").PadLeft(2, "0"c)
                '名称付与
                WW_TEXT = ""
                CODENAME_get("CAMPCODE", T0011INProw("CAMPCODE"), WW_TEXT, WW_RTN)
                T0011INProw("CAMPNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("SHIPORG") < 0 Then
                T0011INProw("SHIPORG") = work.WF_SEL_UORG.Text
                WW_TEXT = ""
                CODENAME_get("SHIPORG", T0011INProw("SHIPORG"), WW_TEXT, WW_RTN)
                T0011INProw("SHIPORGNAMES") = WW_TEXT
            Else
                T0011INProw("SHIPORG") = CS0023XLSTBL.TBLDATA.Rows(i)("SHIPORG")
                '名称付与
                WW_TEXT = ""
                CODENAME_get("SHIPORG", T0011INProw("SHIPORG"), WW_TEXT, WW_RTN)
                T0011INProw("SHIPORGNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("TERMKBN") < 0 Then
                T0011INProw("TERMKBN") = GRT00011WRKINC.TERM_TYPE.HAND
            Else
                T0011INProw("TERMKBN") = CS0023XLSTBL.TBLDATA.Rows(i)("TERMKBN")
                CODENAME_get("TERMKBN", T0011INProw("TERMKBN"), WW_TEXT, WW_RTN)
                T0011INProw("TERMKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("YMD") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("YMD")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("YMD")
                    T0011INProw("YMD") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0011INProw("YMD") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("NIPPONO") >= 0 Then
                If IsDBNull(CS0023XLSTBL.TBLDATA.Rows(i)("NIPPONO")) Then
                    T0011INProw("NIPPONO") = ""
                Else
                    T0011INProw("NIPPONO") = CS0023XLSTBL.TBLDATA.Rows(i)("NIPPONO")
                End If
            End If

            T0011INProw("HDKBN") = "D"

            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                If IsDBNull(CS0023XLSTBL.TBLDATA.Rows(i)("SEQ")) Then
                    T0011INProw("SEQ") = ""
                Else
                    T0011INProw("SEQ") = CS0023XLSTBL.TBLDATA.Rows(i)("SEQ")
                End If
            End If

            If WW_COLUMNS.IndexOf("WORKKBN") >= 0 Then
                T0011INProw("WORKKBN") = CS0023XLSTBL.TBLDATA.Rows(i)("WORKKBN")
                '名称付与
                CODENAME_get("WORKKBN", T0011INProw("WORKKBN"), WW_TEXT, WW_RTN)
                T0011INProw("WORKKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                T0011INProw("STAFFCODE") = CS0023XLSTBL.TBLDATA.Rows(i)("STAFFCODE")
                '名称付与
                CODENAME_get("STAFFCODE", T0011INProw("STAFFCODE"), WW_TEXT, WW_RTN)
                T0011INProw("STAFFNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("SUBSTAFFCODE") >= 0 Then
                T0011INProw("SUBSTAFFCODE") = CS0023XLSTBL.TBLDATA.Rows(i)("SUBSTAFFCODE")
                '名称付与
                CODENAME_get("STAFFCODE", T0011INProw("SUBSTAFFCODE"), WW_TEXT, WW_RTN)
                T0011INProw("SUBSTAFFNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("CREWKBN") >= 0 Then
                T0011INProw("CREWKBN") = CS0023XLSTBL.TBLDATA.Rows(i)("CREWKBN")
                '名称付与
                CODENAME_get("CREWKBN", T0011INProw("CREWKBN"), WW_TEXT, WW_RTN)
                T0011INProw("CREWKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("GSHABAN") >= 0 Then
                T0011INProw("GSHABAN") = CS0023XLSTBL.TBLDATA.Rows(i)("GSHABAN")
            End If

            If WW_COLUMNS.IndexOf("STDATE") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("STDATE")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("STDATE")
                    T0011INProw("STDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0011INProw("STDATE") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("STTIME") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("STTIME")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("STTIME")
                    T0011INProw("STTIME") = WW_DATE.ToString("HH:mm")
                Else
                    T0011INProw("STTIME") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDDATE") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("ENDDATE")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("ENDDATE")
                    T0011INProw("ENDDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0011INProw("ENDDATE") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDTIME") >= 0 Then
                If IsDate(CS0023XLSTBL.TBLDATA.Rows(i)("ENDTIME")) Then
                    WW_DATE = CS0023XLSTBL.TBLDATA.Rows(i)("ENDTIME")
                    T0011INProw("ENDTIME") = WW_DATE.ToString("HH:mm")
                Else
                    T0011INProw("ENDTIME") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("WORKTIME") < 0 Then
                T0011INProw("WORKTIME") = "00:00"
            Else
                T0011INProw("WORKTIME") = CS0023XLSTBL.TBLDATA.Rows(i)("WORKTIME")
            End If

            If WW_COLUMNS.IndexOf("MOVETIME") < 0 Then
                T0011INProw("MOVETIME") = "00:00"
            Else
                T0011INProw("MOVETIME") = CS0023XLSTBL.TBLDATA.Rows(i)("MOVETIME")
            End If

            If WW_COLUMNS.IndexOf("ACTTIME") < 0 Then
                T0011INProw("ACTTIME") = "00:00"
            Else
                T0011INProw("ACTTIME") = CS0023XLSTBL.TBLDATA.Rows(i)("ACTTIME")
            End If

            If WW_COLUMNS.IndexOf("PRATE") < 0 Then
                T0011INProw("PRATE") = "0"
            Else
                T0011INProw("PRATE") = CS0023XLSTBL.TBLDATA.Rows(i)("PRATE")
            End If

            If WW_COLUMNS.IndexOf("CASH") < 0 Then
                T0011INProw("CASH") = "0"
            Else
                T0011INProw("CASH") = CS0023XLSTBL.TBLDATA.Rows(i)("CASH")
            End If

            If WW_COLUMNS.IndexOf("TICKET") < 0 Then
                T0011INProw("TICKET") = "0"
            Else
                T0011INProw("TICKET") = CS0023XLSTBL.TBLDATA.Rows(i)("TICKET")
            End If

            If WW_COLUMNS.IndexOf("ETC") < 0 Then
                T0011INProw("ETC") = "0"
            Else
                T0011INProw("ETC") = CS0023XLSTBL.TBLDATA.Rows(i)("ETC")
            End If

            If WW_COLUMNS.IndexOf("TOTALTOLL") < 0 Then
                T0011INProw("TOTALTOLL") = "0"
            Else
                T0011INProw("TOTALTOLL") = CS0023XLSTBL.TBLDATA.Rows(i)("TOTALTOLL")
            End If

            If WW_COLUMNS.IndexOf("STMATER") < 0 Then
                T0011INProw("STMATER") = "0.00"
            Else
                T0011INProw("STMATER") = CS0023XLSTBL.TBLDATA.Rows(i)("STMATER")
            End If

            If WW_COLUMNS.IndexOf("ENDMATER") < 0 Then
                T0011INProw("ENDMATER") = "0.00"
            Else
                T0011INProw("ENDMATER") = CS0023XLSTBL.TBLDATA.Rows(i)("ENDMATER")
            End If

            If WW_COLUMNS.IndexOf("RUIDISTANCE") < 0 Then
                T0011INProw("RUIDISTANCE") = "0.00"
            Else
                T0011INProw("RUIDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("RUIDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("SOUDISTANCE") < 0 Then
                T0011INProw("SOUDISTANCE") = "0.00"
            Else
                T0011INProw("SOUDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("SOUDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("JIDISTANCE") < 0 Then
                T0011INProw("JIDISTANCE") = "0.00"
            Else
                T0011INProw("JIDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("JIDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KUDISTANCE") < 0 Then
                T0011INProw("KUDISTANCE") = "0.00"
            Else
                T0011INProw("KUDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("KUDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("IPPDISTANCE") < 0 Then
                T0011INProw("IPPDISTANCE") = "0.00"
            Else
                T0011INProw("IPPDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("IPPDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KOSDISTANCE") < 0 Then
                T0011INProw("KOSDISTANCE") = "0.00"
            Else
                T0011INProw("KOSDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("KOSDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("IPPJIDISTANCE") < 0 Then
                T0011INProw("IPPJIDISTANCE") = "0.00"
            Else
                T0011INProw("IPPJIDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("IPPJIDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("IPPKUDISTANCE") < 0 Then
                T0011INProw("IPPKUDISTANCE") = "0.00"
            Else
                T0011INProw("IPPKUDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("IPPKUDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KOSJIDISTANCE") < 0 Then
                T0011INProw("KOSJIDISTANCE") = "0.00"
            Else
                T0011INProw("KOSJIDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("KOSJIDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KOSKUDISTANCE") < 0 Then
                T0011INProw("KOSKUDISTANCE") = "0.00"
            Else
                T0011INProw("KOSKUDISTANCE") = CS0023XLSTBL.TBLDATA.Rows(i)("KOSKUDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KYUYU") < 0 Then
                T0011INProw("KYUYU") = "0.00"
            Else
                T0011INProw("KYUYU") = CS0023XLSTBL.TBLDATA.Rows(i)("KYUYU")
            End If

            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                T0011INProw("TORICODE") = CS0023XLSTBL.TBLDATA.Rows(i)("TORICODE")
                '名称付与
                CODENAME_get("TORICODE", T0011INProw("TORICODE"), WW_TEXT, WW_RTN)
                T0011INProw("TORINAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("SHUKABASHO") >= 0 Then
                T0011INProw("SHUKABASHO") = CS0023XLSTBL.TBLDATA.Rows(i)("SHUKABASHO")
                '名称付与
                CODENAME_get("SHUKABASHO", T0011INProw("SHUKABASHO"), WW_TEXT, WW_RTN)
                T0011INProw("SHUKABASHONAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("SHUKADATE") >= 0 Then
                T0011INProw("SHUKADATE") = CS0023XLSTBL.TBLDATA.Rows(i)("SHUKADATE")
            End If

            If WW_COLUMNS.IndexOf("TODOKECODE") >= 0 Then
                T0011INProw("TODOKECODE") = CS0023XLSTBL.TBLDATA.Rows(i)("TODOKECODE")
                '名称付与
                CODENAME_get("TODOKECODEY", T0011INProw("TODOKECODE"), WW_TEXT, WW_RTN)
                T0011INProw("TODOKENAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("TODOKEDATE") >= 0 Then
                T0011INProw("TODOKEDATE") = CS0023XLSTBL.TBLDATA.Rows(i)("TODOKEDATE")
            End If

            '品名１～８、数量１～８、請求単位１～８
            For WW_SEQ As Integer = 1 To 8
                Dim WW_OILTYPE As String = "OILTYPE" & WW_SEQ.ToString("0")
                Dim WW_PRODUCT1 As String = "PRODUCT1" & WW_SEQ.ToString("0")
                Dim WW_PRODUCT2 As String = "PRODUCT2" & WW_SEQ.ToString("0")
                Dim WW_PRODUCTCODE As String = "PRODUCTCODE" & WW_SEQ.ToString("0")
                Dim WW_PRODUCTNAMES As String = "PRODUCT" & WW_SEQ.ToString("0") & "NAMES"
                Dim WW_SURYO As String = "SURYO" & WW_SEQ.ToString("0")
                Dim WW_STANI As String = "STANI" & WW_SEQ.ToString("0")

                If WW_COLUMNS.IndexOf(WW_OILTYPE) >= 0 Then
                    T0011INProw(WW_OILTYPE) = CS0023XLSTBL.TBLDATA.Rows(i)(WW_OILTYPE)
                End If

                If WW_COLUMNS.IndexOf(WW_PRODUCT1) >= 0 Then
                    T0011INProw(WW_PRODUCT1) = CS0023XLSTBL.TBLDATA.Rows(i)(WW_PRODUCT1)
                End If

                If WW_COLUMNS.IndexOf(WW_PRODUCT2) >= 0 Then
                    T0011INProw(WW_PRODUCT2) = CS0023XLSTBL.TBLDATA.Rows(i)(WW_PRODUCT2)
                End If

                If WW_COLUMNS.IndexOf(WW_PRODUCTCODE) >= 0 Then
                    T0011INProw(WW_PRODUCTCODE) = CS0023XLSTBL.TBLDATA.Rows(i)(WW_PRODUCTCODE)
                    '名称付与
                    Dim WW_PRODUCT As String = T0011INProw(WW_PRODUCTCODE)
                    CODENAME_get("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN)
                    T0011INProw(WW_PRODUCTNAMES) = WW_TEXT
                End If

                If WW_COLUMNS.IndexOf(WW_SURYO) < 0 Then
                    T0011INProw(WW_SURYO) = "0.000"
                Else
                    T0011INProw(WW_SURYO) = CS0023XLSTBL.TBLDATA.Rows(i)(WW_SURYO)
                End If

                If WW_COLUMNS.IndexOf(WW_STANI) >= 0 Then
                    T0011INProw(WW_STANI) = CS0023XLSTBL.TBLDATA.Rows(i)(WW_STANI)
                End If
            Next

            If WW_COLUMNS.IndexOf("TOTALSURYO") < 0 Then
                T0011INProw("TOTALSURYO") = "0.000"
            Else
                T0011INProw("TOTALSURYO") = CS0023XLSTBL.TBLDATA.Rows(i)("TOTALSURYO")
            End If

            If WW_COLUMNS.IndexOf("TUMIOKIKBN") >= 0 Then
                T0011INProw("TUMIOKIKBN") = CS0023XLSTBL.TBLDATA.Rows(i)("TUMIOKIKBN")
                '名称付与
                CODENAME_get("TUMIOKIKBN", T0011INProw("TUMIOKIKBN"), WW_TEXT, WW_RTN)
                T0011INProw("TUMIOKIKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("URIKBN") >= 0 Then
                T0011INProw("URIKBN") = CS0023XLSTBL.TBLDATA.Rows(i)("URIKBN")
                '名称付与
                CODENAME_get("URIKBN", T0011INProw("URIKBN"), WW_TEXT, WW_RTN)
                T0011INProw("URIKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("ORDERNO") >= 0 Then
                T0011INProw("ORDERNO") = CS0023XLSTBL.TBLDATA.Rows(i)("ORDERNO")
            End If

            If WW_COLUMNS.IndexOf("DETAILNO") >= 0 Then
                T0011INProw("DETAILNO") = CS0023XLSTBL.TBLDATA.Rows(i)("DETAILNO")
            End If

            If WW_COLUMNS.IndexOf("TRIPNO") >= 0 Then
                T0011INProw("TRIPNO") = CS0023XLSTBL.TBLDATA.Rows(i)("TRIPNO")
            End If

            If WW_COLUMNS.IndexOf("DROPNO") >= 0 Then
                T0011INProw("DROPNO") = CS0023XLSTBL.TBLDATA.Rows(i)("DROPNO")
            End If

            If WW_COLUMNS.IndexOf("TAXKBN") < 0 Then
                T0011INProw("TAXKBN") = "0"
            Else
                T0011INProw("TAXKBN") = CS0023XLSTBL.TBLDATA.Rows(i)("TAXKBN")
            End If

            If WW_COLUMNS.IndexOf("STORICODE") >= 0 Then
                T0011INProw("STORICODE") = CS0023XLSTBL.TBLDATA.Rows(i)("STORICODE")
            End If

            If WW_COLUMNS.IndexOf("CONTCHASSIS") >= 0 Then
                T0011INProw("CONTCHASSIS") = CS0023XLSTBL.TBLDATA.Rows(i)("CONTCHASSIS")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEF") >= 0 Then
                T0011INProw("SHARYOTYPEF") = CS0023XLSTBL.TBLDATA.Rows(i)("SHARYOTYPEF")
            End If

            If WW_COLUMNS.IndexOf("TSHABANF") >= 0 Then
                T0011INProw("TSHABANF") = CS0023XLSTBL.TBLDATA.Rows(i)("TSHABANF")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB") >= 0 Then
                T0011INProw("SHARYOTYPEB") = CS0023XLSTBL.TBLDATA.Rows(i)("SHARYOTYPEB")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB") >= 0 Then
                T0011INProw("TSHABANB") = CS0023XLSTBL.TBLDATA.Rows(i)("TSHABANB")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB2") >= 0 Then
                T0011INProw("SHARYOTYPEB2") = CS0023XLSTBL.TBLDATA.Rows(i)("SHARYOTYPEB2")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB2") >= 0 Then
                T0011INProw("TSHABANB2") = CS0023XLSTBL.TBLDATA.Rows(i)("TSHABANB2")
            End If

            If WW_COLUMNS.IndexOf("JISSKIKBN") < 0 Then
                T0011INProw("JISSKIKBN") = "0"
            Else
                T0011INProw("JISSKIKBN") = CS0023XLSTBL.TBLDATA.Rows(i)("JISSKIKBN")
            End If

            If WW_COLUMNS.IndexOf("DELFLG") < 0 Then
                T0011INProw("DELFLG") = C_DELETE_FLG.ALIVE
            Else
                T0011INProw("DELFLG") = CS0023XLSTBL.TBLDATA.Rows(i)("DELFLG")
            End If

            T0011INPtbl.Rows.Add(T0011INProw)

        Next

        '■■■ GridView更新 ■■■
        Grid_UpdateExcel(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        '○メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        '■■■ 画面終了 ■■■

        'Close
        CS0023XLSTBL.TBLDATA.Dispose()
        CS0023XLSTBL.TBLDATA.Clear()

        'カーソル設定
        WF_FIELD.Value = "WF_STAFFCODE"
        WF_STAFFCODE.Focus()

    End Sub

    ''' <summary>
    '''  TODO:GridViewの更新（Excel）
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub Grid_UpdateExcel(ByRef O_RTN As String)

        Dim WW_UMU As Integer = 0

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '○テーブルデータ 復元（GridView）
            'テーブルデータ 復元(TEXTファイルより復元)
            If Not Master.RecoverTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then
                O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                Exit Sub
            End If
            '○データリカバリ（一週間前データ）
            If Not Master.RecoverTable(T0011WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then
                O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                Exit Sub
            End If

            CS0026TBLSORT.TABLE = T0011tbl
            CS0026TBLSORT.FILTER = ""
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            T0011tbl = CS0026TBLSORT.sort()
            '-----------------------------------------------------------------------------------
            '差分データ（取込）とT0011tbl（GridView）を比較し、該当データが存在すれば上書き
            '存在しない場合はスルー
            '-----------------------------------------------------------------------------------
            For Each WW_INPRow As DataRow In T0011INPtbl.Rows
                For Each WW_Row As DataRow In T0011tbl.Rows
                    '出庫日・乗務員・トリップ・ドロップ・車番
                    If WW_Row("YMD") = WW_INPRow("YMD") AndAlso
                       WW_Row("STAFFCODE") = WW_INPRow("STAFFCODE") AndAlso
                       WW_Row("GSHABAN") = WW_INPRow("GSHABAN") AndAlso
                       WW_Row("TRIPNO") = WW_INPRow("TRIPNO") AndAlso
                       WW_Row("DROPNO") = WW_INPRow("DROPNO") AndAlso
                       WW_Row("SELECT") = "1" Then
                        '数量の反映
                        '品名１～８、数量１～８、請求単位１～８
                        For WW_IDX As Integer = 1 To 8
                            Dim WW_PRODUCTCODE As String = "PRODUCTCODE" & WW_IDX.ToString("0")
                            Dim WW_SURYO As String = "SURYO" & WW_IDX.ToString("0")

                            If Not String.IsNullOrEmpty(WW_INPRow(WW_PRODUCTCODE)) AndAlso
                                WW_Row(WW_SURYO) <> WW_INPRow(WW_SURYO) Then
                                WW_Row(WW_SURYO) = WW_INPRow(WW_SURYO)
                                WW_Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            End If
                        Next
                    End If
                Next

            Next

            '------------------------------------------------------------
            '■出庫日、従業員 単位
            '  取込んだ日報番号毎のヘッダを出庫日、従業員毎に集約し直す
            '------------------------------------------------------------
            T0011_CreHead(T0011tbl)

            '------------------------------------------------------------
            '■マージ後のチェック
            '------------------------------------------------------------
            T0011tbl_CheckOrderExcel(T0011tbl, O_RTN)
            If Not isNormal(O_RTN) Then
                Exit Sub
            End If

            Dim WW_SEQ As Integer = 0
            Dim WW_LINECNT As Integer = 0

            '行番号の採番
            For i As Integer = 0 To T0011tbl.Rows.Count - 1
                If T0011tbl.Rows(i)("SELECT") = "1" Then
                    If T0011tbl.Rows(i)("HDKBN") = "H" Then
                        WW_SEQ = 1
                        T0011tbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")
                        T0011tbl.Rows(i)("LINECNT") = 0
                        T0011tbl.Rows(i)("SELECT") = "1"
                        T0011tbl.Rows(i)("HIDDEN") = "1"
                    Else
                        T0011tbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")
                        WW_SEQ = WW_SEQ + 1

                        T0011tbl.Rows(i)("LINECNT") = 0
                        T0011tbl.Rows(i)("SELECT") = "1"
                        T0011tbl.Rows(i)("HIDDEN") = "1"
                        If T0011tbl.Rows(i)("WORKKBN") = "B3" Then
                            WW_LINECNT = WW_LINECNT + 1
                            T0011tbl.Rows(i)("LINECNT") = WW_LINECNT
                            T0011tbl.Rows(i)("HIDDEN") = "0"

                        End If
                    End If
                End If
            Next

            '明細にエラーがある場合、ヘッダにエラーを設定する
            Dim WW_HeadIdx As Integer = 0
            Dim WW_ERR_FLG As Boolean = False
            For i As Integer = 0 To T0011tbl.Rows.Count - 1
                If T0011tbl.Rows(i)("HDKBN") = "H" Then
                    WW_ERR_FLG = False
                    WW_HeadIdx = i
                End If
                '次のヘッダまで
                For j As Integer = i + 1 To T0011tbl.Rows.Count - 1
                    If T0011tbl.Rows(j)("HDKBN") = "H" Then
                        i = j - 1
                        Exit For
                    End If
                    If T0011tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then WW_ERR_FLG = True
                Next
                If WW_ERR_FLG Then T0011tbl.Rows(WW_HeadIdx)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

            Next

            CS0026TBLSORT.TABLE = T0011tbl
            CS0026TBLSORT.FILTER = "WORKKBN = 'B3'"
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, TRIPNO, DROPNO"
            Dim WW_T0011tbl As DataTable = CS0026TBLSORT.sort()

            For i As Integer = 0 To WW_T0011tbl.Rows.Count - 1
                Dim WW_ERRWORD As String = ""
                WW_ERRWORD = rightview.getErrorReport.Replace("@L" & WW_T0011tbl(i)("YMD") & WW_T0011tbl(i)("STAFFCODE") & "L@", WW_T0011tbl(i)("LINECNT"))
                rightview.setErrorReport(WW_ERRWORD)
            Next

            '○GridViewデータをテーブルに保存
            If Master.SaveTable(T0011tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

            '○GridViewデータをテーブルに保存（一週間前データ）
            If Master.SaveTable(T0011WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

            '絞込みボタン処理（GridViewの表示）を行う
            WF_ButtonExtract_Click()


        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "Grid_UpdateExcel")
            CS0011LOGWRITE.INFSUBCLASS = "Grid_Update"                  'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:INSERT T0005_NIPPO"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    '共通処理処理
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' 配送受注チェック
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub T0011tbl_CheckOrder(ByRef IO_TBL As DataTable, ByRef O_RTN As String)

        Dim WW_LINEerr As String = C_MESSAGE_NO.NORMAL
        Dim WW_SaveIdx As Integer = 0
        Dim IO_TBLrow As DataRow
        Dim WW_CONVERT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CHECKREPORT As String = String.Empty

        O_RTN = C_MESSAGE_NO.NORMAL
        Using S0013tbl As New DataTable
            For i As Integer = 0 To IO_TBL.Rows.Count - 1
                IO_TBLrow = IO_TBL.Rows(i)
                WW_LINEerr = C_MESSAGE_NO.NORMAL

                '画面表示対象外データ（更新前データ）は、読み飛ばし
                If IO_TBLrow("SELECT") = "0" Then Continue For
                'ヘッダー行は一時保存
                If IO_TBLrow("HDKBN") = "H" Then WW_SaveIdx = i


                '配送受注ＤＢの存在チェック
                '荷卸
                If IO_TBLrow("WORKKBN") = "B3" Then
                    '・キー項目(数量：SURYO)
                    For idx As Integer = 1 To 8
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", IO_TBLrow("SURYO" & idx), O_RTN, WW_CHECKREPORT, S0013tbl)
                        If isNormal(O_RTN) Then
                            IO_TBLrow("SURYO" & idx) = Format(Val(IO_TBLrow("SURYO" & idx)), "#,0.000")
                            If Val(IO_TBLrow("SURYO" & idx)) < 0 Then
                                'エラーレポート編集
                                ERRMSG_write("・更新できないレコード(数量" & idx & "エラー)です。", IO_TBLrow("SURYO" & idx), WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                            End If
                        Else
                            'エラーレポート編集
                            ERRMSG_write("・更新できないレコード(数量" & idx & "エラー)です。", WW_CHECKREPORT, WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                            IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                        End If
                    Next
                    '荷卸
                    If Val(IO_TBLrow("SURYO1")) = 0 AndAlso
                           Val(IO_TBLrow("SURYO2")) = 0 AndAlso
                           Val(IO_TBLrow("SURYO3")) = 0 AndAlso
                           Val(IO_TBLrow("SURYO4")) = 0 AndAlso
                           Val(IO_TBLrow("SURYO5")) = 0 AndAlso
                           Val(IO_TBLrow("SURYO6")) = 0 AndAlso
                           Val(IO_TBLrow("SURYO7")) = 0 AndAlso
                           Val(IO_TBLrow("SURYO8")) = 0 Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(数量エラー)です。"
                        WW_CheckMES2 = "荷卸数量未入力（）"
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                End If
            Next
        End Using
    End Sub

    ''' <summary>
    ''' 配送受注チェック
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub T0011tbl_CheckOrderExcel(ByRef IO_TBL As DataTable, ByRef O_RTN As String)

        Dim WW_SaveIdx As Integer = 0
        Dim iTblrow As DataRow
        Dim WW_CHECKREPORT As String = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        Using S0013tbl As New DataTable
            For i As Integer = 0 To IO_TBL.Rows.Count - 1
                iTblrow = IO_TBL.Rows(i)

                '画面表示対象外データ（更新前データ）は、読み飛ばし
                If iTblrow("SELECT") = "0" Then Continue For
                'ヘッダー行は一時保存
                If iTblrow("HDKBN") = "H" Then WW_SaveIdx = i

                '・キー項目(数量：SURYO)
                For idx As Integer = 1 To 8
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", iTblrow("SURYO" & idx), O_RTN, WW_CHECKREPORT, S0013tbl)
                    If isNormal(O_RTN) Then
                        iTblrow("SURYO" & idx) = Format(Val(iTblrow("SURYO" & idx)), "#,0.000")
                        If Val(iTblrow("SURYO" & idx)) < 0 Then
                            'エラーレポート編集
                            ERRMSG_write("・更新できないレコード(数量" & idx & "エラー)です。", iTblrow("SURYO" & idx), C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, iTblrow, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                            iTblrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                        End If
                    Else
                        'エラーレポート編集
                        ERRMSG_write("・更新できないレコード(数量" & idx & "エラー)です。", WW_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, iTblrow, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                        iTblrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                Next
                '配送受注ＤＢの存在チェック
                '荷卸
                If Val(iTblrow("SURYO1")) = 0 AndAlso
                           Val(iTblrow("SURYO2")) = 0 AndAlso
                           Val(iTblrow("SURYO3")) = 0 AndAlso
                           Val(iTblrow("SURYO4")) = 0 AndAlso
                           Val(iTblrow("SURYO5")) = 0 AndAlso
                           Val(iTblrow("SURYO6")) = 0 AndAlso
                           Val(iTblrow("SURYO7")) = 0 AndAlso
                           Val(iTblrow("SURYO8")) = 0 Then
                        'エラーレポート編集
                        ERRMSG_write("・更新できないレコード(数量エラー)です。", "荷卸数量未入力（）", i, iTblrow, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        iTblrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                Next
        End Using
    End Sub

    ''' <summary>
    ''' 名称設定処理   LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String,
                               ByRef I_VALUE As String,
                               ByRef O_TEXT As String,
                               ByRef O_RTN As String)

        '○名称取得
        O_TEXT = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        If Not String.IsNullOrEmpty(I_VALUE) Then
            Select Case I_FIELD
                Case "WORKKBN"
                    '作業区分名称 "WORKKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "WORKKBN"))
                Case "DELFLG"
                    '削除フラグ　DELFLG
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "TORICODEY"
                    '取引先名称（矢崎・変換）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.creatYazakiShipperList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "TORICODEK"
                    '取引先名称（光英・変換）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.creatKoeiShipperList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "TORICODE"
                    '取引先名称（マスタ）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.createCustomerParam(work.WF_SEL_CAMPCODE.Text))

                Case "TODOKECODEY"
                    '届先名（矢崎）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createYazakiConsigneeList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "1", O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "TODOKECODE"
                    '届先名（マスタ）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "", "1"))

                Case "SHUKABASHOY"
                    '出荷場所名称（矢崎）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createYazakiConsigneeList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "2", O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "SHUKABASHO"
                    '出荷場所名称（マスタ）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "", "2"))

                Case "PRODUCT2Y"
                    '品名（矢崎）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.creatYazakiProdList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "PRODUCT2K"
                    '品名（光英）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.creatKoeiProdList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "PRODUCT2G2"
                    '品名（光英ENEX） 品名２コードより品名を取得
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createProduct2Lists(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "PRODUCT2KE"
                    '品名（光英ENEX） 9桁コードより品名を取得
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createProductLists(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, work.WF_SEL_CAMPCODE.Text & I_VALUE, O_TEXT, O_RTN, param)

                Case "PRODUCT2"
                    '品名（マスタ）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createProductLists(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "STAFFCODE"
                    '乗務員名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text))

                Case "CAMPCODE"
                    '会社名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)

                Case "SHIPORG"
                    '出荷部署名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createShipORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.REFERLANCE))

                Case "TERMKBN"
                    '端末区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "TERMKBN"))
                Case "JISSKIKBN"
                    '実績登録区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "JISSKIKBN"))
                Case "CREWKBN"
                    '乗務区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "CREWKBN"))
                Case "TUMIOKIKBN"
                    '積置区分名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "TUMIOKIKBN"))

                Case "GSHABANSHATANY"
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createYSHABANList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "GSHABANSHATANK"
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createKSHABANList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "GSHABAN"
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createTSHABANList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "MANGSHAFUKU"
                    '車腹
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createSHABAN2OILList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "MANGOILTYPE"
                    '油種
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createSHABAN2OILList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "URIKBN"
                    '売上計上基準名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "URIKBN"))

                Case "STANI"
                    '請求単位 PRODUCTCODE 2 TANNI
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.createProduct2ClassList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "STANINAMES"
                    '請求単位
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "STANI"))

                Case "TAXKBN"
                    '税区分名称 TAXKBN
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "TAXKBN"))

                Case "SUISOKBN"
                    '水素区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, work.createHydrogenParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN))

            End Select
        End If

    End Sub


    '★★★★★★★★★★★★★★★★★★★★★
    'データ操作
    '★★★★★★★★★★★★★★★★★★★★★



    ''' <summary>
    ''' 条件抽出画面情報退避
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub MAPrefelence(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00011S Then                                                    '条件画面からの画面遷移
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()

            work.WF_T5I_LINECNT.Text = ""
            work.WF_T5I_GridPosition.Text = "1"
            work.WF_T5I_STAFFCODE.Text = ""
            work.WF_T5I_YMD.Text = ""
            work.WF_T5_ERRMSG.Text = ""

            '○T0011tbl情報保存先のファイル名
            work.WF_SEL_XMLsaveF.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
                                    CS0050Session.USERID & "-T00011-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"
            work.WF_SEL_XMLsaveF9.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
                                    CS0050Session.USERID & "-T000119-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"
        End If

        '勤怠締テーブル取得
        Dim WW_LIMITFLG As String = "0"
        T0007COM.T00008get(work.WF_SEL_CAMPCODE.Text,
                           work.WF_SEL_UORG.Text,
                           CDate(work.WF_SEL_STYMD.Text).ToString("yyyy/MM"),
                           WW_LIMITFLG,
                           WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End If

        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            If WW_LIMITFLG = "0" Then
                '対象月の締前は更新ＯＫ
                WF_MAPpermitcode.Value = "TRUE"

                '自分の部署と選択した配属部署が同一なら更新可能
                If work.WF_SEL_UORG.Text = work.WF_SEL_PERMIT_ORG.Text Then
                    WF_MAPpermitcode.Value = "TRUE"
                Else
                    WF_MAPpermitcode.Value = "FALSE"
                End If
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If
        Else
            WF_MAPpermitcode.Value = "FALSE"
        End If

    End Sub
    ''' <summary>
    ''' エラーメッセージ編集
    ''' </summary>
    ''' <param name="I_MSG"></param>
    ''' <remarks></remarks>
    Private Sub ErrMsgSet(ByVal I_MSG As String)

        If WW_ERRLISTCNT <= 4000 Then
            rightview.addErrorReport(ControlChars.NewLine & I_MSG)

            WW_ERRLISTCNT += I_MSG.Length - I_MSG.Replace(vbCr, "").Length + 1

            If WW_ERRLISTCNT > 4000 Then
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "※エラーが4000行超のため出力を停止しました。"
                rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
            End If

        End If
    End Sub
End Class
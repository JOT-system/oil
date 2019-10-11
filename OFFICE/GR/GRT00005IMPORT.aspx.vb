Imports System.IO
Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox

Public Class GRT00005IMPORT
    Inherits Page

    '共通宣言
    ''' <summary>
    ''' 固定値マスタ検索
    ''' </summary>
    Private GS0007FIXVALUElst As New GS0007FIXVALUElst              'Leftボックス用固定値リスト取得
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
    ''' テーブルソート
    ''' </summary>
    Private CS0026TBLSORT As New CS0026TBLSORT
    ''' <summary>
    ''' T3コントロール取得
    ''' </summary>
    Private GS0029T3CNTLget As New GS0029T3CNTLget                  'T3コントロール
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

    'CSV検索結果格納ds
    Private YNIPPOtbl As DataTable                                  '矢崎 日報CSV格納用テーブル
    Private YHAISOtbl As DataTable                                  '矢崎 配送CSV格納用テーブル
    Private YKYUYUtbl As DataTable                                  '矢崎 給油CSV格納用テーブル
    Private KSYASAItbl As DataTable                                 '光英 車載CSV格納用テーブル
    Private T0005tbl As DataTable                                   '日報テーブル（GridView用）
    Private T0005INPtbl As DataTable                                '日報テーブル（取込用）
    Private T0005WKtbl As DataTable                                 '日報テーブル（ワーク）
    Private T0005WEEKtbl As DataTable                               '日報テーブル（一週間前）
    Private T0005PARMtbl As DataTable                               '条件選択画面パラメータ（保存用）
    Private S0013tbl As DataTable                                   'データフィールド
    Private ML002tbl As DataTable                                   '勘定科目判定テーブル

    Private WW_ERRLISTCNT As Integer                                'エラーリスト件数               

    Private WW_ERRLIST_ALL As List(Of String)                       'インポート全体のエラー
    Private WW_ERRLIST As List(Of String)                           'インポート中の１セット分のエラー
    '伝票番号
    Private DENNO As Integer = 0                                    '伝票番号

    Private Const CONST_DSPROWCOUNT As Integer = 40                 '１画面表示対象
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
                        Case "WF_ButtonSAVE"                            '■ 一時保存ボタンクリック時処理
                            WF_ButtonSAVE_Click()
                        Case "WF_ButtonEND"                             '■ 終了ボタン押下時処理
                            WF_ButtonEND_Click()
                        Case "WF_ButtonExtract"                         '■ 絞り込みボタンクリック時処理
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonNEW"                             '■ 新規ボタンクリック時処理
                            WF_ButtonNEW_Click()
                        Case "WF_ButtonUPDATE"                          '■ 更新ボタンクリック時処理
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"                             '■ ダウンロードボタンクリック時処理
                            WF_Print_Click("XLSX")
                        Case "WF_ButtonPrint"                           '■ 印刷ボタンクリック時処理
                            WF_Print_Click("pdf")
                        Case "WF_ButtonFIRST"                           '■ 最始行ボタンクリック時処理
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"                            '■ 最終行ボタンクリック時処理
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonSel"                             '■ 左ボックス 選択ボタン押下時処理
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                             '■ 左ボックス キャンセルボタン押下時処理
                            WF_ButtonCan_Click()
                        Case "WF_Field_DBClick"                         '■ 入力領域ダブルクリック時処理
                            WF_Field_DBClick()
                        Case "WF_ListboxDBclick"                        '■ 左ボックス 一覧ダブルクリック時処理
                            WF_Listbox_DBClick()
                        Case "WF_RadioButonClick"                       '■ 右ボックス　ラジオボタン選択時処理
                            WF_RadioButon_Click()
                        Case "WF_MEMOChange"                            '■ 右ボックス　メモ欄保存処理
                            WF_MEMO_Change()
                        Case "WF_GridDBclick"                           '■ 一覧ダブルクリック時処理
                            WF_Grid_DBclick()
                        Case "WF_MouseWheelDown"                        '■ 一覧　前頁遷移処理
                            WF_GRID_ScroleDown()
                        Case "WF_MouseWheelUp"                          '■ 一覧　次頁遷移処理
                            WF_GRID_ScroleUp()
                        Case "WF_UPLOAD_EXCEL"                          '■ 一覧　ファイルアップロード時処理（EXCEL）
                            UPLOAD_EXCEL()
                        Case "WF_UPLOAD_KOUEI"                          '■ 一覧　ファイルアップロード時処理（JXレガシー）
                            UPLOAD_KOUEI(GRT00005WRKINC.TERM_TYPE.JX, WW_DUMMY)
                        Case "WF_UPLOAD_JX_KOUEI"                       '■ 一覧　ファイルアップロード時処理（JX）
                            UPLOAD_KOUEI(GRT00005WRKINC.TERM_TYPE.JX, WW_DUMMY)
                        Case "WF_UPLOAD_TG_KOUEI"                       '■ 一覧　ファイルアップロード時処理（TG）
                            UPLOAD_KOUEI(GRT00005WRKINC.TERM_TYPE.TG, WW_DUMMY)
                        Case "WF_UPLOAD_EX_KOUEI"                       '■ 一覧　ファイルアップロード時処理（ENEXレガシー）
                            UPLOAD_KOUEI(GRT00005WRKINC.TERM_TYPE.JOT, WW_DUMMY)
                        Case "WF_UPLOAD_JOT_KOUEI"                      '■ 一覧　ファイルアップロード時処理（JOT）
                            UPLOAD_KOUEI(GRT00005WRKINC.TERM_TYPE.JOT, WW_DUMMY)
                        Case "WF_UPLOAD_COSMO_KOUEI"                    '■ 一覧　ファイルアップロード時処理（COSMO）
                            UPLOAD_KOUEI(GRT00005WRKINC.TERM_TYPE.COSMO, WW_DUMMY)
                        Case "WF_UPLOAD_YAZAKI"                         '■ 一覧　ファイルアップロード時処理（YAZAKI）
                            UPLOAD_YAZAKI(WW_DUMMY)
                        Case "WF_ButtonDownload"                        '■ 光英受信ボタン押下時処理
                            Download_Click()
                    End Select
                    '○一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '〇初期化処理
                Initialize()
            End If
        Finally
            If Not IsNothing(YNIPPOtbl) Then
                YNIPPOtbl.Dispose()
                YNIPPOtbl = Nothing
            End If
            If Not IsNothing(YHAISOtbl) Then
                YHAISOtbl.Dispose()
                YHAISOtbl = Nothing
            End If
            If Not IsNothing(YKYUYUtbl) Then
                YKYUYUtbl.Dispose()
                YKYUYUtbl = Nothing
            End If
            If Not IsNothing(KSYASAItbl) Then
                KSYASAItbl.Dispose()
                KSYASAItbl = Nothing
            End If

            If Not IsNothing(T0005tbl) Then
                T0005tbl.Dispose()
                T0005tbl = Nothing
            End If
            If Not IsNothing(T0005INPtbl) Then
                T0005INPtbl.Dispose()
                T0005INPtbl = Nothing
            End If
            If Not IsNothing(T0005PARMtbl) Then
                T0005PARMtbl.Dispose()
                T0005PARMtbl = Nothing
            End If
            If Not IsNothing(T0005WKtbl) Then
                T0005WKtbl.Dispose()
                T0005WKtbl = Nothing
            End If

            If Not IsNothing(T0005WEEKtbl) Then
                T0005WEEKtbl.Dispose()
                T0005WEEKtbl = Nothing
            End If

            If Not IsNothing(S0013tbl) Then
                S0013tbl.Dispose()
                S0013tbl = Nothing
            End If

            If Not IsNothing(ML002tbl) Then
                ML002tbl.Dispose()
                ML002tbl = Nothing
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
        MapRefelence(O_RTN)
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップON
        Master.eventDrop = True

        '光英読込中ファイル一覧クリア
        WF_KoueiLoadFile.Items.Clear()

        '■■■ 選択情報　設定処理 ■■■
        '〇右Boxへの値設定
        rightview.MAPID_MEMO = Master.MAPID
        rightview.MAPID_REPORT = GRT00005WRKINC.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)
        '〇表示データの取得
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00005S Then
            '検索画面から遷移した場合
            If work.WF_SEL_BUTTON.Text = GRT00005WRKINC.LC_BTN_TYPE.BTN_RESTART Then
                '〇一時保存情報を反映
                If Not Master.RecoverTable(T0005tbl, work.WF_T5_XMLsaveTmp.Text) Then Exit Sub
                '〇一時保存情報を反映
                If Not Master.RecoverTable(T0005WEEKtbl, work.WF_T5_XMLsaveTmp9.Text) Then Exit Sub
                '〇データを保存する
                If Not Master.SaveTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
                '〇データを保存する(一週間前データ）
                If Not Master.SaveTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
                '絞込みボタン処理（GridViewの表示）を行う
                WF_ButtonExtract_Click()
            Else
                '〇通常検索
                GetGridData()
            End If

            '一覧表示データ編集（性能対策）
            Using TBLview As DataView = New DataView(T0005tbl)
                Dim WW_STPOS As Integer = Val(work.WF_T5I_GridPosition.Text)
                Dim WW_ENDPOS As Integer = Val(work.WF_T5I_GridPosition.Text) + CONST_DSPROWCOUNT
                TBLview.RowFilter = "LINECNT >= " & WW_STPOS & " and LINECNT <= " & WW_ENDPOS
                CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
                CS0013ProfView.PROFID = Master.PROF_VIEW
                CS0013ProfView.MAPID = GRT00005WRKINC.MAPIDI
                CS0013ProfView.VARI = Master.VIEWID
                CS0013ProfView.SRCDATA = TBLview.ToTable
                CS0013ProfView.TBLOBJ = pnlListArea
                CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
                CS0013ProfView.LEVENT = "ondblclick"
                CS0013ProfView.LFUNC = "ListDbClick"
                CS0013ProfView.TITLEOPT = True
                CS0013ProfView.CS0013ProfView()
            End Using
            If Not isNormal(CS0013ProfView.ERR) Then
                Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
                Exit Sub
            End If

        Else
            '日報訂正画面から遷移した場合
            '保存しておいた、GridViewの表示開始位置、絞込み条件の乗務員、日報番号を設定し直す
            WF_STAFFCODE.Text = work.WF_T5I_STAFFCODE.Text
            WF_YMD.Text = work.WF_T5I_YMD.Text
            For Each w In work.WF_KoueiLoadFile.Items
                WF_KoueiLoadFile.Items.Add(w)
            Next
            '保存しておいたエラーメッセージを表示
            rightview.setErrorReport(Replace(work.WF_T5_ERRMSG.Text, "\n", vbCrLf))
            WF_ButtonClick.Value = "WF_ButtonExtract2"
            WF_ButtonExtract_Click()
            WF_ButtonClick.Value = String.Empty
            '○一覧再表示処理
            DisplayGrid()
        End If

        '光英受信ボタン非表示設定
        Dim T5Com = New GRT0005COM
        If Not T5Com.IsKoueiAvailableOrg(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, GRT00005WRKINC.C_KOUEI_CLASS_CODE, WW_ERRCODE) Then
            WF_IsHideKoueiButton.Value = "1"
        End If
        T5Com = Nothing

        '〇テンポラリの削除
        work.DeleteTmpFiles()

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        If IsNothing(T0005tbl) Then
            '○画面表示データ復元
            If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        End If
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To T0005tbl.Rows.Count - 1
            If T0005tbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                T0005tbl.Rows(i)("SELECT") = WW_DataCNT
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
        Using WW_TBLview As DataView = New DataView(T0005tbl)
            'ソート
            WW_TBLview.Sort = "LINECNT"
            WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
            '一覧作成
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013PROFview.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRT00005WRKINC.MAPIDI
            CS0013PROFview.VARI = Master.VIEWID
            CS0013PROFview.SRCDATA = WW_TBLview.ToTable
            CS0013PROFview.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013PROFview.LEVENT = "ondblclick"
            CS0013PROFview.LFUNC = "ListDbClick"
            CS0013PROFview.TITLEOPT = True
            CS0013PROFview.CS0013ProfView()

            '○クリア
            If WW_TBLview.Count = 0 Then
                work.WF_T5I_GridPosition.Text = "1"
            Else
                work.WF_T5I_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
            End If
        End Using
        WF_STAFFCODE.Focus()

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    'イベント処理
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' 一時保存ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSAVE_Click()
        '画面表示を取得
        If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '一時保存ファイルに出力
        If Not Master.SaveTable(T0005tbl, work.WF_T5_XMLsaveTmp.Text) Then Exit Sub
        '一週間前データを取得
        If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
        '一時保存ファイルに出力
        If Not Master.SaveTable(T0005WEEKtbl, work.WF_T5_XMLsaveTmp9.Text) Then Exit Sub
        '検索条件を一時保存ファイルに出力
        T0005PARMtbl = work.createParamTable()

        Dim WW_T0005PARMrow As DataRow = T0005PARMtbl.NewRow
        WW_T0005PARMrow("LINECNT") = 1
        WW_T0005PARMrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        WW_T0005PARMrow("TIMSTP") = 0
        WW_T0005PARMrow("SELECT") = 1
        WW_T0005PARMrow("HIDDEN") = 0
        '会社コード　
        WW_T0005PARMrow("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
        '出庫日　
        WW_T0005PARMrow("STYMD") = work.WF_SEL_STYMD.Text
        WW_T0005PARMrow("ENDYMD") = work.WF_SEL_ENDYMD.Text
        '運用部署
        WW_T0005PARMrow("UORG") = work.WF_SEL_UORG.Text
        '従業員コード
        WW_T0005PARMrow("STAFFCODE") = work.WF_SEL_STAFFCODE.Text
        WW_T0005PARMrow("STAFFNAME") = work.WF_SEL_STAFFNAME.Text
        '日報確認年月
        WW_T0005PARMrow("IMPYM") = work.WF_SEL_IMPYM.Text
        T0005PARMtbl.Rows.Add(WW_T0005PARMrow)
        '一時保存ファイルに出力
        If Not Master.SaveTable(T0005PARMtbl, work.WF_SEL_XMLsavePARM.Text) Then Exit Sub

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_FIELD.Value = "WF_STAFFCODE"
        WF_STAFFCODE.Focus()

    End Sub

    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○入力値チェック
        Dim WW_CONVERT As String = ""
        Dim WW_TEXT As String = ""
        '乗務員
        CodeToName("STAFFCODE", WF_STAFFCODE.Text, WW_TEXT, WW_DUMMY)
        WF_STAFFCODE_TEXT.Text = WW_TEXT

        '○テーブルデータ 復元（絞込みボタン押下の時のみ）
        If WF_ButtonClick.Value Like "WF_ButtonExtract*" Then
            '〇データリカバリ
            If IsNothing(T0005tbl) Then
                If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
            End If
            '○データリカバリ（一週間前データ）
            If IsNothing(T0005WEEKtbl) Then
                If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
            End If
        End If

        '○絞り込み操作（GridView明細Hidden設定）
        For Each T0005row As DataRow In T0005tbl.Select("HDKBN='H'", "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ")
            If T0005row("SELECT") = 1 Then
                T0005row("HIDDEN") = 1

                '従業員・日報　絞込判定
                If (WF_STAFFCODE.Text = "") AndAlso (WF_YMD.Text = "") Then
                    T0005row("HIDDEN") = 0
                End If

                If (WF_STAFFCODE.Text <> "") AndAlso (WF_YMD.Text = "") Then
                    If T0005row("STAFFCODE") Like WF_STAFFCODE.Text & "*" Then
                        T0005row("HIDDEN") = 0
                    End If
                End If

                If (WF_STAFFCODE.Text = "") AndAlso (WF_YMD.Text <> "") Then
                    If Not IsDate(WF_YMD.Text) Then WF_YMD.Text = C_DEFAULT_YMD

                    If T0005row("YMD") = CDate(WF_YMD.Text).ToString("yyyy/MM/dd") Then T0005row("HIDDEN") = 0
                End If

                If (WF_STAFFCODE.Text <> "") AndAlso (WF_YMD.Text <> "") Then
                    If Not IsDate(WF_YMD.Text) Then WF_YMD.Text = C_DEFAULT_YMD

                    If T0005row("STAFFCODE") Like WF_STAFFCODE.Text & "*" AndAlso
                       T0005row("YMD") = CDate(WF_YMD.Text).ToString("yyyy/MM/dd") Then
                        T0005row("HIDDEN") = 0
                    End If
                End If
            End If
        Next

        If WF_ButtonClick.Value = "WF_ButtonExtract" Then
            work.WF_T5I_GridPosition.Text = "1"
            work.WF_T5I_STAFFCODE.Text = WF_STAFFCODE.Text
            work.WF_T5I_YMD.Text = WF_YMD.Text
        End If

        '○GridViewデータをテーブルに保存（絞込みボタン押下の時のみ）
        If WF_ButtonClick.Value Like "WF_ButtonExtract*" Then
            '〇データ保存
            If Not Master.SaveTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
            If Not Master.SaveTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
        End If

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        '○カーソル設定
        WF_FIELD.Value = "WF_STAFFCODE"
        WF_STAFFCODE.Focus()

    End Sub

    ''' <summary>
    ''' 新規ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonNEW_Click()

        Dim WW_CNT As Integer = 0

        '〇データリカバリ
        If IsNothing(T0005tbl) Then
            If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        End If
        '○データリカバリ（一週間前データ）
        If IsNothing(T0005WEEKtbl) Then
            If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
        End If
        '〇選択済のヘッダーデータ件数を取得
        For Each T0005row As DataRow In T0005tbl.Rows
            If T0005row("SELECT") = "1" AndAlso T0005row("HDKBN") = "H" Then WW_CNT = WW_CNT + 1
        Next

        'Grid表示位置（先頭行）
        '次画面から戻したい情報
        work.WF_T5I_LINECNT.Text = WW_CNT + 1
        'メッセージ
        work.WF_T5_ERRMSG.Text = Replace(rightview.getErrorReport(), vbCrLf, "\n")

        '出庫年月日　
        work.WF_T5_YMD.Text = String.Empty
        '従業員コード
        work.WF_T5_STAFFCODE.Text = String.Empty
        '従業員名
        work.WF_T5_STAFFNAME.Text = String.Empty
        '押下ボタン
        work.WF_SEL_BUTTON.Text = GRT00005WRKINC.LC_BTN_TYPE.BTN_NEW
        'パス
        work.WF_SEL_XMLsaveF2.Text = String.Empty
        '呼出元MAPID　
        work.WF_T5_FROMMAPID.Text = Master.MAPID

        '★★★ 画面遷移先URL取得 ★★★

        '画面遷移実行
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 更新ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

        rightview.setErrorReport("")
        If IsNothing(T0005tbl) Then
            If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        End If
        '○データリカバリ（一週間前データ）
        If IsNothing(T0005WEEKtbl) Then
            If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
        End If

        '重複チェック
        Dim WW_MSG As String = String.Empty
        T0005COM.CheckDuplicateDataT0005(T0005tbl, WW_MSG, WW_RTN)
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
            MA002UPDATE.T0005tbl = T0005tbl
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
            TA001UPDATE.T0005tbl = T0005tbl
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
            'MB003UPDATE.T0005tbl = T0005tbl
            'MB003UPDATE.Update()
            'If Not isNormal(MB003UPDATE.ERR) Then
            '    Master.output(MB003UPDATE.ERR, C_MESSAGE_TYPE.ABORT, "例外発生")
            '    Exit Sub
            'End If


            '〇配送受注、荷主受注更新
            T0004UPDATE.SQLcon = SQLcon
            T0004UPDATE.SQLtrn = SQLtrn
            T0004UPDATE.T0005tbl = T0005tbl
            T0004UPDATE.UPDUSERID = Master.USERID
            T0004UPDATE.UPDTERMID = Master.USERTERMID
            T0004UPDATE.ListBoxGSHABAN = work.createSHABANLists(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
            T0004UPDATE.Update()
            If isNormal(T0004UPDATE.ERR) AndAlso isNormal(O_RTN) Then
                T0005tbl = T0004UPDATE.rtnTbl
            Else
                Master.output(T0004UPDATE.ERR, C_MESSAGE_TYPE.ABORT, "例外発生")
                Exit Sub
            End If

            '〇日報ＤＢ更新

            '統計DB出力用項目設定
            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            '削除データの退避
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            Dim WW_T0005DELtbl As DataTable = CS0026TBLSORT.sort()
            '有効データのみ
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            Dim WW_T0005SELtbl As DataTable = CS0026TBLSORT.sort()
            '有効データ＋１週間前
            WW_T0005SELtbl.Merge(T0005WEEKtbl)

            '〇トリップ判定・回送判定・出荷日内荷積荷卸回数判定
            T0005COM.ReEditT0005(WW_T0005SELtbl, work.WF_SEL_CAMPCODE.Text, WW_RTN)
            '有効データと１週間前データの分離
            CS0026TBLSORT.TABLE = WW_T0005SELtbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "YMD >= #" & work.WF_SEL_STYMD.Text & "#"
            T0005tbl = CS0026TBLSORT.sort()

            '有効レコード＋削除レコード（元に戻す）
            T0005tbl.Merge(WW_T0005DELtbl)

            Dim WW_DATE As Date = Date.Now
            '〇T0005更新処理
            'SQLtrn = SQLcon.BeginTransaction
            T0005UPDATE.SQLcon = SQLcon
            T0005UPDATE.SQLtrn = SQLtrn
            T0005UPDATE.T0005tbl = T0005tbl
            T0005UPDATE.ENTRYDATE = WW_DATE
            T0005UPDATE.UPDUSERID = Master.USERID
            T0005UPDATE.UPDTERMID = Master.USERTERMID
            T0005UPDATE.KOUEIFILES = WF_KoueiLoadFile
            T0005UPDATE.Update()
            If isNormal(T0005UPDATE.ERR) Then
                T0005tbl = T0005UPDATE.T0005tbl
                'SQLtrn.Commit()
            Else
                Master.output(T0005UPDATE.ERR, C_MESSAGE_TYPE.ABORT, "例外発生")
                'SQLtrn.Rollback()
                Exit Sub
            End If
            'SQLtrn = Nothing

            '〇不要テーブルデータ除去
            If Not IsNothing(WW_T0005DELtbl) Then
                WW_T0005DELtbl.Dispose()
                WW_T0005DELtbl = Nothing
            End If
            If Not IsNothing(WW_T0005SELtbl) Then
                WW_T0005SELtbl.Dispose()
                WW_T0005SELtbl = Nothing
            End If
            '〇統計ＤＢ更新
            Dim L00001tbl = New DataTable
            CS0044L1INSERT.CS0044L1ColmnsAdd(L00001tbl)

            '有効データのみ
            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            Dim WW_T0005LSELtbl As DataTable = CS0026TBLSORT.sort()

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

                For Each T0005row As DataRow In WW_T0005LSELtbl.Rows
                    '〇更新対象レコードは統計情報を一度削除する（ヘッダーを用いて削除）
                    If T0005row("HDKBN") = "H" AndAlso T0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then

                        Try
                            PARA01.Value = work.WF_SEL_CAMPCODE.Text
                            PARA02.Value = "T05"
                            PARA03.Value = T0005row("YMD")
                            PARA04.Value = T0005row("STAFFCODE")
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
            T0005COM.EditL00001(WW_T0005LSELtbl, L00001tbl, WW_RTN)
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

            If Not IsNothing(WW_T0005LSELtbl) Then
                WW_T0005LSELtbl.Dispose()
                WW_T0005LSELtbl = Nothing
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
                For Each WW_row As DataRow In T0005tbl.Rows
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
            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            T0005tbl = CS0026TBLSORT.sort()

            For i As Integer = T0005tbl.Rows.Count - 1 To 0 Step -1
                Dim T0005row As DataRow = T0005tbl.Rows(i)
                If T0005row("SELECT") = "0" Then
                    T0005row.Delete()
                Else
                    If T0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                        T0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        If T0005row("HDKBN") = "H" Then
                            Dim WW_row As DataRow = T0005tbl.Rows(i + 1)
                            T0005row("TIMSTP") = WW_row("TIMSTP")
                        End If
                    End If
                End If
            Next

        End Using

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

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
        If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

        Using WW_TBLview As DataView = New DataView(T0005tbl)
            WW_TBLview.Sort = "CAMPCODE, SHIPORG, TERMKBN, YMD, STAFFCODE, SEQ"
            WW_TBLview.RowFilter = "HDKBN='D' and SELECT = '1'"
            Using WW_TBL As DataTable = WW_TBLview.ToTable

                '帳票出力dll Interface
                CS0030REPORTtbl.CAMPCODE = work.WF_SEL_CAMPCODE.Text
                CS0030REPORTtbl.PROFID = Master.PROF_REPORT
                CS0030REPORTtbl.MAPID = GRT00005WRKINC.MAPID                   'PARAM01:画面ID
                CS0030REPORTtbl.REPORTID = rightview.getReportId               'PARAM02:帳票ID
                CS0030REPORTtbl.FILEtyp = OutType                              'PARAM03:出力ファイル形式
                CS0030REPORTtbl.TBLDATA = WW_TBL                               'PARAM04:データ参照tabledata
                CS0030REPORTtbl.CS0030REPORT()

                If Not isNormal(CS0030REPORTtbl.ERR) Then
                    Master.output(CS0030REPORTtbl.ERR, C_MESSAGE_TYPE.ABORT, "CS0022REPORT")
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
        If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '先頭頁に移動
        work.WF_T5I_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()
        '○データリカバリ 
        If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○対象データ件数取得
        Using WW_TBLview As DataView = New DataView(T0005tbl)
            WW_TBLview.RowFilter = "HIDDEN= '0'"

            '最終頁に移動
            If WW_TBLview.Count Mod 10 = 0 Then
                work.WF_T5I_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
            Else
                work.WF_T5I_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
            End If
        End Using
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
                    Dim prmData As Hashtable = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text)
                    Select Case WF_LeftMViewChange.Value
                        Case LIST_BOX_CLASSIFICATION.LC_STAFFCODE
                            prmData = work.CreateSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text)

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
        If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○データリカバリ
        If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleUp()
        '○画面表示データ復元
        If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○データリカバリ
        If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

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
    ''' GridViewダブルクリック処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBclick()

        Dim WW_LINECNT As Integer

        '○処理準備
        '○画面表示データ復元(TEXTファイルより復元)
        If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○データリカバリ（一週間前データ）
        If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

        'LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try
        work.WF_T5I_LINECNT.Text = WW_LINECNT
        'Grid表示位置（先頭行）
        '次画面から戻したい情報
        work.WF_T5I_GridPosition.Text = work.WF_T5I_GridPosition.Text

        'メッセージ
        work.WF_T5_ERRMSG.Text = Replace(rightview.getErrorReport, vbCrLf, "\n")

        '■■■ Grid内容(T0005tbl)よりセッション変数編集 ■■■
        For Each WW_T0005row As DataRow In T0005tbl.Rows
            If WW_T0005row("SELECT") = "1" AndAlso WW_T0005row("LINECNT") = WW_LINECNT Then
                '会社コード　
                work.WF_SEL_CAMPCODE.Text = WW_T0005row("CAMPCODE")
                '出庫年月日　
                work.WF_T5_YMD.Text = WW_T0005row("YMD")
                '従業員コード
                work.WF_T5_STAFFCODE.Text = WW_T0005row("STAFFCODE")
                '従業員名
                work.WF_T5_STAFFNAME.Text = WW_T0005row("STAFFNAMES")

                Exit For
            End If
        Next

        '押下ボタン
        work.WF_SEL_BUTTON.Text = GRT00005WRKINC.LC_BTN_TYPE.BTN_NOSELECT

        '呼出元MAPID　
        work.WF_T5_FROMMAPID.Text = Master.MAPID

        '光英受信ファイル
        work.WF_KoueiLoadFile.Items.Clear()
        For Each w In WF_KoueiLoadFile.Items
            work.WF_KoueiLoadFile.Items.Add(w)
        Next

        '★★★ 画面遷移先URL取得 ★★★
        Master.transitionPage()

    End Sub

    ''' <summary>
    ''' GridView用データ取得        ★済
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GetGridData()

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
            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.getSorting()
            WW_SORT = CS0026TBLSORT.SORTING
            '■テーブル検索結果をテーブル退避
            '日報DB更新用テーブル

            T0005COM.AddColumnT0005tbl(T0005tbl)

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
                If work.WF_SEL_STAFFCODE.Text <> Nothing Then
                    SQLWhere = SQLWhere & " and A.STAFFCODE = '" & Trim(work.WF_SEL_STAFFCODE.Text) & "' "
                End If
                If work.WF_SEL_STAFFNAME.Text <> Nothing Then
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
                        T0005tbl.Load(SQLdr)
                        If T0005tbl.Rows.Count > 65000 Then
                            'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
                            Master.Output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ABORT)
                            'Close
                            T0005tbl.Clear()
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
                        T0005COM.AddColumnT0005tbl(T0005WEEKtbl)

                        T0005WEEKtbl.Load(SQLdr2)

                        '一週間前～開始日付－１日をマージ
                        T0005tbl.Merge(T0005WEEKtbl)
                    End Using
                    Dim WW_T0005tbl As DataTable = T0005tbl.Clone
                    For i As Integer = 0 To T0005tbl.Rows.Count - 1
                        Dim T0005row As DataRow = WW_T0005tbl.NewRow
                        T0005row.ItemArray = T0005tbl.Rows(i).ItemArray

                        If IsDate(T0005row("YMD")) Then
                            T0005row("YMD") = CDate(T0005row("YMD")).ToString("yyyy/MM/dd")
                        Else
                            T0005row("YMD") = C_DEFAULT_YMD
                        End If

                        T0005row("SELECT") = "1"      '対象データ
                        T0005row("HIDDEN") = "1"      '非表示

                        T0005row("HDKBN") = "D"       'ヘッダ、明細区分
                        If IsDate(T0005row("SHUKADATE")) Then
                            T0005row("SHUKADATE") = CDate(T0005row("SHUKADATE")).ToString("yyyy/MM/dd")
                        End If
                        If IsDate(T0005row("TODOKEDATE")) Then
                            T0005row("TODOKEDATE") = CDate(T0005row("TODOKEDATE")).ToString("yyyy/MM/dd")
                        End If
                        T0005row("SEQ") = CInt(T0005row("SEQ")).ToString("000")
                        If IsDate(T0005row("STDATE")) Then
                            T0005row("STDATE") = CDate(T0005row("STDATE")).ToString("yyyy/MM/dd")
                        Else
                            T0005row("STDATE") = C_DEFAULT_YMD
                        End If
                        If IsDate(T0005row("STTIME")) Then
                            T0005row("STTIME") = CDate(T0005row("STTIME")).ToString("HH:mm")
                        Else
                            T0005row("STTIME") = "00:00"
                        End If
                        If IsDate(T0005row("ENDDATE")) Then
                            T0005row("ENDDATE") = CDate(T0005row("ENDDATE")).ToString("yyyy/MM/dd")
                        Else
                            T0005row("ENDDATE") = C_DEFAULT_YMD
                        End If
                        If IsDate(T0005row("STTIME")) Then
                            T0005row("ENDTIME") = CDate(T0005row("ENDTIME")).ToString("HH:mm")
                        Else
                            T0005row("ENDTIME") = "00:00"
                        End If
                        T0005row("WORKTIME") = T0005COM.MinutestoHHMM(T0005row("WORKTIME"))
                        T0005row("MOVETIME") = T0005COM.MinutestoHHMM(T0005row("MOVETIME"))
                        T0005row("ACTTIME") = T0005COM.MinutestoHHMM(T0005row("ACTTIME"))
                        T0005row("PRATE") = CInt(T0005row("PRATE")).ToString("#,0")

                        T0005row("CASH") = CInt(T0005row("CASH")).ToString("#,0")
                        T0005row("TICKET") = CInt(T0005row("TICKET")).ToString("#,0")
                        T0005row("ETC") = CInt(T0005row("ETC")).ToString("#,0")
                        T0005row("TOTALTOLL") = CInt(T0005row("TOTALTOLL")).ToString("#,0")
                        T0005row("STMATER") = Val(T0005row("STMATER")).ToString("#,0.00")
                        T0005row("ENDMATER") = Val(T0005row("ENDMATER")).ToString("#,0.00")
                        T0005row("RUIDISTANCE") = Val(T0005row("RUIDISTANCE")).ToString("#,0.00")
                        T0005row("SOUDISTANCE") = Val(T0005row("SOUDISTANCE")).ToString("#,0.00")
                        T0005row("JIDISTANCE") = Val(T0005row("JIDISTANCE")).ToString("#,0.00")
                        T0005row("KUDISTANCE") = Val(T0005row("KUDISTANCE")).ToString("#,0.00")
                        T0005row("IPPDISTANCE") = Val(T0005row("IPPDISTANCE")).ToString("#,0.00")
                        T0005row("KOSDISTANCE") = Val(T0005row("KOSDISTANCE")).ToString("#,0.00")
                        T0005row("IPPJIDISTANCE") = Val(T0005row("IPPJIDISTANCE")).ToString("#,0.00")
                        T0005row("IPPKUDISTANCE") = Val(T0005row("IPPKUDISTANCE")).ToString("#,0.00")
                        T0005row("KOSJIDISTANCE") = Val(T0005row("KOSJIDISTANCE")).ToString("#,0.00")
                        T0005row("KOSKUDISTANCE") = Val(T0005row("KOSKUDISTANCE")).ToString("#,0.00")
                        T0005row("KYUYU") = Val(T0005row("KYUYU")).ToString("#,0.00")
                        T0005row("SURYO1") = Val(T0005row("SURYO1")).ToString("#,0.000")
                        T0005row("SURYO2") = Val(T0005row("SURYO2")).ToString("#,0.000")
                        T0005row("SURYO3") = Val(T0005row("SURYO3")).ToString("#,0.000")
                        T0005row("SURYO4") = Val(T0005row("SURYO4")).ToString("#,0.000")
                        T0005row("SURYO5") = Val(T0005row("SURYO5")).ToString("#,0.000")
                        T0005row("SURYO6") = Val(T0005row("SURYO6")).ToString("#,0.000")
                        T0005row("SURYO7") = Val(T0005row("SURYO7")).ToString("#,0.000")
                        T0005row("SURYO8") = Val(T0005row("SURYO8")).ToString("#,0.000")
                        T0005row("TOTALSURYO") = Val(T0005row("TOTALSURYO")).ToString("#,0.000")

                        Dim WW_PRODUCT As String = ""
                        WW_PRODUCT = T0005row("PRODUCTCODE1")
                        If WW_PRODUCT <> "" Then
                            T0005row("PRODUCT1NAMES") = ""
                            CodeToName("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT1NAMES"), WW_DUMMY)
                        End If
                        WW_PRODUCT = T0005row("PRODUCTCODE2")
                        If WW_PRODUCT <> "" Then
                            T0005row("PRODUCT2NAMES") = ""
                            CodeToName("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT2NAMES"), WW_DUMMY)
                        End If
                        WW_PRODUCT = T0005row("PRODUCTCODE3")
                        If WW_PRODUCT <> "" Then
                            T0005row("PRODUCT3NAMES") = ""
                            CodeToName("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT3NAMES"), WW_DUMMY)
                        End If
                        WW_PRODUCT = T0005row("PRODUCTCODE4")
                        If WW_PRODUCT <> "" Then
                            T0005row("PRODUCT4NAMES") = ""
                            CodeToName("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT4NAMES"), WW_DUMMY)
                        End If
                        WW_PRODUCT = T0005row("PRODUCTCODE5")
                        If WW_PRODUCT <> "" Then
                            T0005row("PRODUCT5NAMES") = ""
                            CodeToName("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT5NAMES"), WW_DUMMY)
                        End If
                        WW_PRODUCT = T0005row("PRODUCTCODE6")
                        If WW_PRODUCT <> "" Then
                            T0005row("PRODUCT6NAMES") = ""
                            CodeToName("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT6NAMES"), WW_DUMMY)
                        End If
                        WW_PRODUCT = T0005row("PRODUCTCODE7")
                        If WW_PRODUCT <> "" Then
                            T0005row("PRODUCT7NAMES") = ""
                            CodeToName("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT7NAMES"), WW_DUMMY)
                        End If
                        WW_PRODUCT = T0005row("PRODUCTCODE8")
                        If WW_PRODUCT <> "" Then
                            T0005row("PRODUCT8NAMES") = ""
                            CodeToName("PRODUCT2", WW_PRODUCT, T0005row("PRODUCT8NAMES"), WW_DUMMY)
                        End If
                        WW_T0005tbl.Rows.Add(T0005row)
                    Next

                    T0005tbl = WW_T0005tbl.Copy

                    WW_T0005tbl.Dispose()
                    WW_T0005tbl = Nothing

                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0005_NIPPO SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '--------------------------------------------
        'ヘッダレコード作成
        '--------------------------------------------
        Dim WW_Filter As String = ""

        '一週間前データを分離
        CS0026TBLSORT.TABLE = T0005tbl
        CS0026TBLSORT.FILTER = "YMD < #" & work.WF_SEL_STYMD.Text & "#"
        CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        T0005WEEKtbl = CS0026TBLSORT.sort()
        '指定日データを分離
        CS0026TBLSORT.FILTER = "YMD >=  #" & work.WF_SEL_STYMD.Text & "#"
        T0005tbl = CS0026TBLSORT.sort()
        'ヘッダ作成
        CreateT0005HeaderNew(T0005WEEKtbl)
        'ヘッダ作成
        CreateT0005HeaderNew(T0005tbl)

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○GridViewデータをテーブルに保存（一週間前データ）
        If Not Master.SaveTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

        '絞込みボタン処理（GridViewの表示）を行う
        WF_ButtonExtract_Click()

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    '車単取込み処理
    '★★★★★★★★★★★★★★★★★★★★★
#Region "<<YAZAKI>>"
    ''' <summary>
    ''' CSV取込（矢崎）処理    
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_YAZAKI(ByRef O_RTN As String)

        'CSVファイル名
        Dim WW_FileName1 As String = ""
        Dim WW_FileName2 As String = ""
        Dim WW_FileName3 As String = ""

        O_RTN = C_MESSAGE_NO.NORMAL
        rightview.setErrorReport("")

        '-----------------------------------------------
        'CSVファイル取得
        '-----------------------------------------------
        Try

            For Each tempFile As String In System.IO.Directory.GetFiles(CS0050Session.UPLOAD_PATH & "\" & "UPLOAD_TMP" & "\" & CS0050Session.USERID, "*.*")
                ' ファイルパスからファイル名を取得
                If tempFile.ToLower Like "*日報.csv" Then
                    WW_FileName1 = tempFile
                End If
                If tempFile.ToLower Like "*配送.csv" Then
                    WW_FileName2 = tempFile
                End If
                If tempFile.ToLower Like "*給油.csv" Then
                    WW_FileName3 = tempFile
                End If
            Next
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Master.output(O_RTN, C_MESSAGE_TYPE.ERR, "矢崎 csv read")
            CS0011LOGWRITE.INFSUBCLASS = "UPLOAD_YAZAKI"                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "矢崎 csv read"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR                                      '
            CS0011LOGWRITE.TEXT = ex.ToString
            CS0011LOGWRITE.MESSAGENO = O_RTN
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        If WW_FileName1 = "" OrElse WW_FileName2 = "" OrElse WW_FileName3 = "" Then
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Master.output(O_RTN, C_MESSAGE_TYPE.ERR, "矢崎 csv read")

            CS0011LOGWRITE.INFSUBCLASS = "UPLOAD_YAZAKI"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "矢崎 csv read"                    '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR                                 '
            CS0011LOGWRITE.TEXT = "ファイル名：「" & WW_FileName1 & "」" & "「" & WW_FileName2 & "」" & "「" & WW_FileName3 & "」が必要です"
            CS0011LOGWRITE.MESSAGENO = O_RTN
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '-----------------------------------------------
        '●CSV読み込み（データテーブルに格納）
        '-----------------------------------------------
        '日報CSV格納テーブル
        AddCsvColumn(YNIPPOtbl, 82)
        '配送CSV格納テーブル
        AddCsvColumn(YHAISOtbl, 65)
        '給油CSV格納テーブル
        AddCsvColumn(YKYUYUtbl, 18)
        '日報DB更新用テーブル
        T0005COM.AddColumnT0005tbl(T0005INPtbl)
        T0005WKtbl = T0005INPtbl.Clone

        '日報CSV
        ReadCsvFile(WW_FileName1, YNIPPOtbl, O_RTN)
        If Not isNormal(O_RTN) Then Exit Sub
        '配送CSV
        ReadCsvFile(WW_FileName2, YHAISOtbl, O_RTN)
        If Not isNormal(O_RTN) Then Exit Sub
        '給油CSV
        ReadCsvFile(WW_FileName3, YKYUYUtbl, O_RTN)
        If Not isNormal(O_RTN) Then Exit Sub

        '-----------------------------------------------
        '■ヘッダレコード編集（日報CSVより作成）
        '-----------------------------------------------
        WW_ERRLIST = New List(Of String)
        WW_ERRLIST_ALL = New List(Of String)
        WW_ERRLISTCNT = 0

        CreateT0005tblHeaderForYazaki(O_RTN)
        If Not isNormal(O_RTN) Then Exit Sub


        T0005WKtbl = T0005INPtbl.Copy
        '-----------------------------------------------
        '■明細コード編集（配送CSVより作成）
        '-----------------------------------------------
        CreateT0005tblDetailForYazaki(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            O_RTN = WW_ERRCODE
            Exit Sub
        End If

        '-----------------------------------------------
        '■T0005tbl再編集
        '-----------------------------------------------
        '不要なデータ（作業区分）を除いたため明細行番号の再符番する
        EditT0005Tbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            O_RTN = WW_ERRCODE
            Exit Sub
        End If

        '-----------------------------------------------
        '■項目変換（T0005INPtblより）
        '-----------------------------------------------
        ConvT0005tblData(T0005INPtbl, WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            O_RTN = WW_ERRCODE
            Exit Sub
        End If

        '-----------------------------------------------
        '■２マンの再編集（T0005INPtblより）
        '-----------------------------------------------
        EditTwoManRecordForYazaki(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            O_RTN = WW_ERRCODE
            Exit Sub
        End If

        '-----------------------------------------------
        '■関連チェック（T0005INPtblより）
        '-----------------------------------------------
        For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
            Dim T0005INProw As DataRow = T0005INPtbl.Rows(i)
            If T0005INProw("HDKBN") = "H" Then
                Continue For
            End If

            CheckT0005INPRow(T0005INProw, WW_ERRCODE)
        Next

        '-----------------------------------------------
        '■表更新
        '-----------------------------------------------
        UpdateGridData(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            O_RTN = WW_ERRCODE
            Exit Sub
        End If

        '○メッセージ表示
        If WW_ERRLIST_ALL.Count > 0 Then
            If WW_ERRLIST_ALL.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ElseIf WW_ERRLIST_ALL.IndexOf(C_MESSAGE_NO.BOX_ERROR_EXIST) >= 0 Then
                O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            Else
                O_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST
            End If
        End If
        If O_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST Then
            Master.Output(O_RTN, C_MESSAGE_TYPE.WAR)
        Else
            If Not isNormal(O_RTN) Then
                Master.Output(O_RTN, C_MESSAGE_TYPE.ERR)
            End If
        End If

    End Sub

    ''' <summary>
    ''' 日報ＤＢ（ヘッダ）編集（矢崎）
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CreateT0005tblHeaderForYazaki(ByRef O_RTN As String)

        Dim WW_TWOMAN As String = "OFF"
        Dim WW_SUBSTAFF As String = ""

        O_RTN = C_MESSAGE_NO.NORMAL

        Try
            S0013tbl = New DataTable

            Dim O_MESSAGE_NO As String = String.Empty
            Dim O_CHECKREPORT As String = String.Empty
            Dim O_VALUE As String = String.Empty

            For Each YNIPPOrow As DataRow In YNIPPOtbl.Rows
                'ヘッダ行のスキップ
                If YNIPPOrow("FIELD1") Like "*日報*" Then Continue For

                WW_ERRLIST.Clear()
                WW_ERRLIST_ALL.Clear()

                Dim T0005INProw As DataRow = T0005INPtbl.NewRow
                T0005COM.InitialT5INPRow(T0005INProw)

                '端末区分（1:矢崎）
                T0005INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                T0005INProw("TERMKBN") = GRT00005WRKINC.TERM_TYPE.YAZAKI
                T0005INProw("SHIPORG") = work.WF_SEL_UORG.Text

                '運行日
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "YMD", YNIPPOrow("FIELD2"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    If Not String.IsNullOrEmpty(O_VALUE) Then
                        T0005INProw("YMD") = O_VALUE
                        If CDate(T0005INProw("YMD")) < CDate(work.WF_SEL_STYMD.Text) OrElse
                           CDate(T0005INProw("YMD")) > CDate(work.WF_SEL_ENDYMD.Text) Then
                            OutputErrorMessageForYazaki(YNIPPOrow, "運行日", "運行日が範囲対象外です。", C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                        End If
                    Else
                        OutputErrorMessageForYazaki(YNIPPOrow, "運行日", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    OutputErrorMessageForYazaki(YNIPPOrow, "運行日", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '日報番号
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NIPPONO", YNIPPOrow("FIELD1"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("NIPPONO") = O_VALUE
                Else
                    OutputErrorMessageForYazaki(YNIPPOrow, "日報番号", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                'ヘッダー・明細区分
                T0005INProw("HDKBN") = "H"

                '作業区分
                T0005INProw("WORKKBN") = ""

                '明細行番号
                T0005INProw("SEQ") = "001"

                '乗務員
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", Mid(YNIPPOrow("FIELD3"), 4, 5), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("STAFFCODE") = O_VALUE
                Else
                    OutputErrorMessageForYazaki(YNIPPOrow, "乗務員コード", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '乗務区分（1:正乗務）
                T0005INProw("CREWKBN") = "1"

                '副乗務員(1)コード
                WW_TWOMAN = "OFF"
                If YNIPPOrow("FIELD68") <> "" Then
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", Mid(YNIPPOrow("FIELD68"), 4, 5), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        WW_SUBSTAFF = O_VALUE
                        T0005INProw("SUBSTAFFCODE") = O_VALUE
                    Else
                        T0005INProw("SUBSTAFFCODE") = Mid(YNIPPOrow("FIELD68"), 4, 5)
                        OutputErrorMessageForYazaki(YNIPPOrow, "副乗務員(1)", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                    WW_TWOMAN = "ON"
                Else
                    T0005INProw("SUBSTAFFCODE") = ""
                End If

                '----------------------------------------
                '以降は、エラーデータをとして取り込む
                '----------------------------------------
                '車両コード
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "GSHABAN", YNIPPOrow("FIELD5"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("GSHABAN") = O_VALUE
                Else
                    T0005INProw("GSHABAN") = YNIPPOrow("FIELD5")
                    OutputErrorMessageForYazaki(YNIPPOrow, "車両コード", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '出庫日付
                Dim WW_CHK_OK As Integer = 0
                '①必須・項目属性チェック
                If YNIPPOrow("FIELD74") = "" OrElse YNIPPOrow("FIELD74") = "99:99" Then
                    T0005INProw("CTRL") = "OFF"
                    '出庫日付
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STDATE", YNIPPOrow("FIELD32"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        T0005INProw("STDATE") = O_VALUE
                        WW_CHK_OK += 1
                    Else
                        T0005INProw("STDATE") = YNIPPOrow("FIELD32")
                        OutputErrorMessageForYazaki(YNIPPOrow, "出勤日付", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If

                    '出庫時刻
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", YNIPPOrow("FIELD33"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        If O_VALUE <> "" Then
                            T0005INProw("STTIME") = CDate(O_VALUE).ToString("HH:mm")
                            WW_CHK_OK += 1
                        Else
                            T0005INProw("STTIME") = YNIPPOrow("FIELD33")
                            OutputErrorMessageForYazaki(YNIPPOrow, "出勤時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        End If
                    Else
                        T0005INProw("STTIME") = YNIPPOrow("FIELD33")
                        OutputErrorMessageForYazaki(YNIPPOrow, "出勤時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                Else
                    T0005INProw("CTRL") = "ON"
                    Dim WW_YMDHS() As String = YNIPPOrow("FIELD74").Split(" ")
                    '出庫日付
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STDATE", WW_YMDHS(0), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        T0005INProw("STDATE") = O_VALUE
                        WW_CHK_OK += 1
                    Else
                        T0005INProw("STDATE") = WW_YMDHS(0)
                        OutputErrorMessageForYazaki(YNIPPOrow, "出勤日付", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If

                    '出庫時刻
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", WW_YMDHS(1), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        If O_VALUE <> "" Then
                            T0005INProw("STTIME") = CDate(O_VALUE).ToString("HH:mm")
                            WW_CHK_OK += 1
                        Else
                            T0005INProw("STTIME") = WW_YMDHS(1)
                            OutputErrorMessageForYazaki(YNIPPOrow, "出勤時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        End If
                    Else
                        T0005INProw("STTIME") = WW_YMDHS(1)
                        OutputErrorMessageForYazaki(YNIPPOrow, "出勤時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If

                End If

                '退社日付
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDDATE", YNIPPOrow("FIELD35"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("ENDDATE") = O_VALUE
                    WW_CHK_OK += 1
                Else
                    T0005INProw("ENDDATE") = YNIPPOrow("FIELD35")
                    OutputErrorMessageForYazaki(YNIPPOrow, "退社日付", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '退社時刻
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", YNIPPOrow("FIELD36"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    If Not String.IsNullOrEmpty(O_VALUE) Then
                        T0005INProw("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
                        WW_CHK_OK += 1
                    Else
                        T0005INProw("ENDTIME") = YNIPPOrow("FIELD36")
                        OutputErrorMessageForYazaki(YNIPPOrow, "退社時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                Else
                    T0005INProw("ENDTIME") = YNIPPOrow("FIELD36")
                    OutputErrorMessageForYazaki(YNIPPOrow, "退社時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '稼働時間
                Dim WW_MIN As Integer = 0
                If WW_CHK_OK = 4 Then
                    WW_MIN = DateDiff("n", T0005INProw("STDATE") + " " + T0005INProw("STTIME"), T0005INProw("ENDDATE") + " " + T0005INProw("ENDTIME"))
                End If
                T0005INProw("WORKTIME") = T0005COM.MinutestoHHMM(WW_MIN)

                '走行時間
                T0005INProw("MOVETIME") = "00:00"

                '稼働時間
                T0005INProw("ACTTIME") = T0005COM.MinutestoHHMM(WW_MIN)

                '通行料・プレート
                T0005INProw("PRATE") = 0

                '通行料・現金
                T0005INProw("CASH") = 0

                '通行料・チケット
                T0005INProw("TICKET") = 0

                '通行料・ＥＴＣ
                T0005INProw("ETC") = 0

                '通行料合計
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOTALTOLL", YNIPPOrow("FIELD82"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("TOTALTOLL") = CInt(O_VALUE).ToString("#,0")
                Else
                    T0005INProw("TOTALTOLL") = YNIPPOrow("FIELD82")
                    OutputErrorMessageForYazaki(YNIPPOrow, "通行料合計", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '出庫メータ
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STMATER", YNIPPOrow("FIELD19"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("STMATER") = Val(O_VALUE).ToString("#,0.00")
                Else
                    T0005INProw("STMATER") = YNIPPOrow("FIELD19")
                    OutputErrorMessageForYazaki(YNIPPOrow, "出庫メータ", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '帰庫メータ
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDMATER", YNIPPOrow("FIELD20"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("ENDMATER") = Val(O_VALUE).ToString("#,0.00")
                Else
                    T0005INProw("ENDMATER") = YNIPPOrow("FIELD20")
                    OutputErrorMessageForYazaki(YNIPPOrow, "帰庫メータ", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '累積走行距離
                T0005INProw("RUIDISTANCE") = "0.00"

                '走行距離
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SOUDISTANCE", YNIPPOrow("FIELD21"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("SOUDISTANCE") = Val(O_VALUE).ToString("#,0.00")
                Else
                    T0005INProw("SOUDISTANCE") = YNIPPOrow("FIELD21")
                    OutputErrorMessageForYazaki(YNIPPOrow, "走行距離", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                Dim WW_OK22 As Integer = 0
                Dim WW_OK23 As Integer = 0
                Dim WW_OK24 As Integer = 0
                Dim WW_OK25 As Integer = 0

                '一般・実車走行距離
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "IPPJIDISTANCE", YNIPPOrow("FIELD22"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("IPPJIDISTANCE") = Val(O_VALUE).ToString("#,0.00")
                    WW_OK22 = 1
                Else
                    T0005INProw("IPPJIDISTANCE") = YNIPPOrow("FIELD22")
                    OutputErrorMessageForYazaki(YNIPPOrow, "一般・実車走行距離", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '一般・空車走行距離
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "IPPKUDISTANCE", YNIPPOrow("FIELD23"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("IPPKUDISTANCE") = Val(O_VALUE).ToString("#,0.00")
                    WW_OK23 = 1
                Else
                    T0005INProw("IPPKUDISTANCE") = YNIPPOrow("FIELD23")
                    OutputErrorMessageForYazaki(YNIPPOrow, "一般・空車走行距離", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '高速・実車走行距離
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOSJIDISTANCE", YNIPPOrow("FIELD24"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("KOSJIDISTANCE") = Val(O_VALUE).ToString("#,0.00")
                    WW_OK24 = 1
                Else
                    T0005INProw("KOSJIDISTANCE") = YNIPPOrow("FIELD24")
                    OutputErrorMessageForYazaki(YNIPPOrow, "高速・実車走行距離", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '高速・空車走行距離
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOSKUDISTANCE", YNIPPOrow("FIELD25"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("KOSKUDISTANCE") = Val(O_VALUE).ToString("#,0.00")
                    WW_OK25 = 1
                Else
                    T0005INProw("KOSKUDISTANCE") = YNIPPOrow("FIELD25")
                    OutputErrorMessageForYazaki(YNIPPOrow, "高速・空車走行距離", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                If WW_OK22 = 1 AndAlso WW_OK24 = 1 Then
                    T0005INProw("JIDISTANCE") = (Val(YNIPPOrow("FIELD22")) + Val(YNIPPOrow("FIELD24"))).ToString("#,0.00")
                End If
                If WW_OK23 = 1 AndAlso WW_OK25 = 1 Then
                    T0005INProw("KUDISTANCE") = (Val(YNIPPOrow("FIELD23")) + Val(YNIPPOrow("FIELD25"))).ToString("#,0.00")
                End If
                If WW_OK22 = 1 AndAlso WW_OK23 = 1 Then
                    T0005INProw("IPPDISTANCE") = (Val(YNIPPOrow("FIELD22")) + Val(YNIPPOrow("FIELD23"))).ToString("#,0.00")
                End If
                If WW_OK24 = 1 AndAlso WW_OK25 = 1 Then
                    T0005INProw("KOSDISTANCE") = (Val(YNIPPOrow("FIELD24")) + Val(YNIPPOrow("FIELD25"))).ToString("#,0.00")
                End If

                '給油量
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KYUYU", YNIPPOrow("FIELD76"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("KYUYU") = Val(O_VALUE).ToString("#,0.00")
                    WW_OK25 = 1
                Else
                    T0005INProw("KYUYU") = YNIPPOrow("FIELD76")
                    OutputErrorMessageForYazaki(YNIPPOrow, "給油量:（給油区分なし）計", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '削除フラグ
                T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE

                If WW_ERRLIST.Count > 0 Then
                    If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                        Continue For
                    Else
                        'エラーフラグ
                        T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                Else
                    'エラーフラグ
                    T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                End If
                T0005INPtbl.Rows.Add(T0005INProw)

            Next

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
            CS0011LOGWRITE.INFSUBCLASS = "T0005tbl_Head_Yazaki"         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 日報ＤＢ（明細）編集（矢崎）
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CreateT0005tblDetailForYazaki(ByRef O_RTN As String)

        Dim WW_TWOMAN As String = "OFF"
        Dim WW_SUBSTAFF As String = ""
        Dim WW_MIN As Integer = 0
        Dim WW_ACT As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_NIPPONO As String = ""
        Dim WW_YMD As String = ""
        Dim WW_FIRST As String = "OFF"

        S0013tbl = New DataTable

        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Dim O_VALUE As String = String.Empty
            Dim O_MESSAGE_NO As String = C_MESSAGE_NO.NORMAL
            Dim O_CHECKREPORT As String = String.Empty
            '配送CSVより日報ＤＢを編集する
            For i As Integer = 0 To YHAISOtbl.Rows.Count - 1
                Dim YHAISOrow As DataRow = YHAISOtbl.Rows(i)

                WW_ERRLIST = New List(Of String)
                WW_ERRLIST_ALL = New List(Of String)
                If YHAISOrow("FIELD1") Like "*日報*" OrElse YHAISOrow("FIELD8") Like "*宵積*" Then Continue For

                WW_ERRLIST.Clear()
                WW_ERRLIST_ALL.Clear()

                Dim T0005INProw As DataRow = T0005INPtbl.NewRow
                T0005COM.InitialT5INPRow(T0005INProw)
                '端末区分（1:矢崎）
                T0005INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                T0005INProw("TERMKBN") = GRT00005WRKINC.TERM_TYPE.YAZAKI
                T0005INProw("SHIPORG") = work.WF_SEL_UORG.Text

                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "YMD", YHAISOrow("FIELD16"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    If O_VALUE <> "" Then
                        T0005INProw("YMD") = O_VALUE
                        WW_YMD = O_VALUE
                        If CDate(T0005INProw("YMD")) < CDate(work.WF_SEL_STYMD.Text) OrElse
                           CDate(T0005INProw("YMD")) > CDate(work.WF_SEL_ENDYMD.Text) Then
                            OutputErrorMessageForYazaki(YHAISOrow, "運行日", "運行日が範囲対象外です。", C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                        End If
                    Else
                        OutputErrorMessageForYazaki(YHAISOrow, "運行日", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    OutputErrorMessageForYazaki(YHAISOrow, "運行日", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '日報番号
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NIPPONO", YHAISOrow("FIELD1"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("NIPPONO") = O_VALUE

                    For Each WkRow As DataRow In T0005WKtbl.Rows
                        If WkRow("NIPPONO") = T0005INProw("NIPPONO") AndAlso
                            WkRow("HDKBN") = "H" Then
                            T0005INProw("YMD") = WkRow("YMD")
                            Exit For
                        End If
                    Next
                Else
                    OutputErrorMessageForYazaki(YHAISOrow, "日報番号", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '乗務員
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", Mid(YHAISOrow("FIELD2"), 4, 5), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("STAFFCODE") = O_VALUE
                Else
                    OutputErrorMessageForYazaki(YHAISOrow, "乗務員コード", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                'ヘッダー・明細区分
                T0005INProw("HDKBN") = "D"

                '作業区分
                Dim WW_WORKKBN = ConvertYazakiWorkKbn(YHAISOrow("FIELD8"))
                If WW_WORKKBN = "" Then OutputErrorMessageForYazaki(YHAISOrow, "着地作業", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                T0005INProw("WORKKBN") = WW_WORKKBN

                '明細行番号
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SEQ", YHAISOrow("FIELD6"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("SEQ") = O_VALUE
                Else
                    OutputErrorMessageForYazaki(YHAISOrow, "明細行番号", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '乗務区分（1:正乗務）
                T0005INProw("CREWKBN") = "1"

                '車両コード
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "GSHABAN", YHAISOrow("FIELD4"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("GSHABAN") = O_VALUE
                Else
                    T0005INProw("GSHABAN") = YHAISOrow("FIELD4")
                    OutputErrorMessageForYazaki(YHAISOrow, "車両コード", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '着日付
                Dim WW_CHK_OK As Integer = 0
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STDATE", YHAISOrow("FIELD10"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("STDATE") = O_VALUE
                    WW_CHK_OK += 1
                Else
                    T0005INProw("STDATE") = YHAISOrow("FIELD10")
                    OutputErrorMessageForYazaki(YHAISOrow, "着日付", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '着時刻
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", YHAISOrow("FIELD11"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    If O_VALUE <> "" Then
                        T0005INProw("STTIME") = CDate(O_VALUE).ToString("HH:mm")
                        WW_CHK_OK += 1
                    Else
                        T0005INProw("STTIME") = YHAISOrow("FIELD11")
                        OutputErrorMessageForYazaki(YHAISOrow, "着時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                Else
                    T0005INProw("STTIME") = YHAISOrow("FIELD11")
                    OutputErrorMessageForYazaki(YHAISOrow, "着時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '発日付
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDDATE", YHAISOrow("FIELD16"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("ENDDATE") = O_VALUE
                    WW_CHK_OK += 1
                Else
                    T0005INProw("ENDDATE") = YHAISOrow("FIELD16")
                    OutputErrorMessageForYazaki(YHAISOrow, "着日付", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '発時刻
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", YHAISOrow("FIELD17"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    If Not String.IsNullOrEmpty(O_VALUE) Then
                        T0005INProw("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
                        WW_CHK_OK += 1
                    Else
                        T0005INProw("ENDTIME") = YHAISOrow("FIELD17")
                        OutputErrorMessageForYazaki(YHAISOrow, "発時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                Else
                    T0005INProw("ENDTIME") = YHAISOrow("FIELD17")
                    OutputErrorMessageForYazaki(YHAISOrow, "発時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If


                WW_ACT = 0
                '稼働時間
                If WW_CHK_OK = 4 Then
                    Dim WW_WORKTIME As Integer = T0005COM.HHMMtoMinutes(YHAISOrow("FIELD18"))
                    Dim WW_DATE As Date = CDate(T0005INProw("STDATE") + " " + T0005INProw("STTIME"))
                    WW_DATE = WW_DATE.AddMinutes(WW_WORKTIME)
                    T0005INProw("ENDDATE") = WW_DATE.ToString("yyyy/MM/dd")
                    T0005INProw("ENDTIME") = WW_DATE.ToString("HH:mm")

                    WW_MIN = DateDiff("n", T0005INProw("STDATE") + " " + T0005INProw("STTIME"), T0005INProw("ENDDATE") + " " + T0005INProw("ENDTIME"))
                    WW_ACT += WW_MIN
                End If
                T0005INProw("WORKTIME") = T0005COM.MinutestoHHMM(WW_MIN)

                If WW_NIPPONO <> T0005INProw("NIPPONO") Then
                    T0005INProw("MOVETIME") = "00:00"
                Else
                    WW_MIN = DateDiff("n", YHAISOtbl.Rows(i - 1)("FIELD15"), YHAISOrow("FIELD9"))
                    T0005INProw("MOVETIME") = T0005COM.MinutestoHHMM(WW_MIN)
                    WW_ACT += WW_MIN
                End If

                '稼働時間
                T0005INProw("ACTTIME") = T0005COM.MinutestoHHMM(WW_ACT)

                '通行料・プレート
                T0005INProw("PRATE") = 0

                '通行料・現金
                T0005INProw("CASH") = 0

                '通行料・チケット
                T0005INProw("TICKET") = 0

                '通行料・ＥＴＣ
                T0005INProw("ETC") = 0

                '緯度
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "LATITUDE", YHAISOrow("FIELD24"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("LATITUDE") = O_VALUE
                Else
                    T0005INProw("LATITUDE") = YHAISOrow("FIELD24")
                    OutputErrorMessageForYazaki(YHAISOrow, "緯度", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '経度
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "LONGITUDE", YHAISOrow("FIELD25"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("LONGITUDE") = O_VALUE
                Else
                    T0005INProw("LONGITUDE") = YHAISOrow("FIELD25")
                    OutputErrorMessageForYazaki(YHAISOrow, "経度", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
                '累積走行距離
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RUIDISTANCE", YHAISOrow("FIELD56"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("RUIDISTANCE") = Val(O_VALUE).ToString("#,0.00")
                Else
                    T0005INProw("RUIDISTANCE") = YHAISOrow("FIELD56")
                    OutputErrorMessageForYazaki(YHAISOrow, "累積走行距離", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '走行距離
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SOUDISTANCE", YHAISOrow("FIELD32"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("SOUDISTANCE") = Val(O_VALUE).ToString("#,0.00")
                Else
                    T0005INProw("SOUDISTANCE") = YHAISOrow("FIELD32")
                    OutputErrorMessageForYazaki(YHAISOrow, "走行距離", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '実車距離
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JIDISTANCE", YHAISOrow("FIELD33"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("JIDISTANCE") = Val(O_VALUE).ToString("#,0.00")
                Else
                    T0005INProw("JIDISTANCE") = YHAISOrow("FIELD33")
                    OutputErrorMessageForYazaki(YHAISOrow, "実車距離", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '空車距離
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KUDISTANCE", YHAISOrow("FIELD34"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("KUDISTANCE") = Val(O_VALUE).ToString("#,0.00")
                Else
                    T0005INProw("KUDISTANCE") = YHAISOrow("FIELD34")
                    OutputErrorMessageForYazaki(YHAISOrow, "空車距離", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '荷主、出荷場所
                If WW_WORKKBN = "B2" Then  '荷積
                    T0005INProw("TORICODE") = ""
                    If YHAISOrow("FIELD20") <> "" Then
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", YHAISOrow("FIELD20"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            T0005INProw("SHUKABASHO") = O_VALUE
                        Else
                            T0005INProw("SHUKABASHO") = YHAISOrow("FIELD20")
                            OutputErrorMessageForYazaki(YHAISOrow, "集配先", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        End If
                    End If
                End If

                '届先
                If WW_WORKKBN = "B3" Then  '荷卸
                    If YHAISOrow("FIELD22") = "" Then
                        T0005INProw("TORICODE") = ""
                        T0005INProw("SHUKABASHO") = ""
                        '宵積or荷積、着地日、日報番号
                        For j As Integer = i - 1 To 0 Step -1
                            If YHAISOtbl.Rows(j)("FIELD1") = YHAISOrow("FIELD1") Then
                                If YHAISOtbl.Rows(j)("FIELD8") Like "*宵積*" OrElse
                                   YHAISOtbl.Rows(j)("FIELD8") Like "*荷積*" Then
                                    If YHAISOtbl.Rows(j)("FIELD22") <> "" Then
                                        T0005INProw("TORICODE") = YHAISOtbl.Rows(j)("FIELD22")
                                    End If
                                    If YHAISOtbl.Rows(j)("FIELD20") <> "" Then
                                        T0005INProw("SHUKABASHO") = YHAISOtbl.Rows(j)("FIELD20")
                                    End If
                                    Exit For
                                End If
                            Else
                                Exit For
                            End If
                        Next
                    End If
                    If YHAISOrow("FIELD20") <> "" Then
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", YHAISOrow("FIELD20"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            T0005INProw("TODOKECODE") = O_VALUE
                        Else
                            T0005INProw("TODOKECODE") = YHAISOrow("FIELD20")
                            OutputErrorMessageForYazaki(YHAISOrow, "届先", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        End If
                    End If
                End If

                '品名１
                If YHAISOrow("FIELD35") <> "" Then
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", YHAISOrow("FIELD35"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        T0005INProw("PRODUCTCODE1") = O_VALUE
                    Else
                        T0005INProw("PRODUCTCODE1") = YHAISOrow("FIELD35")
                        OutputErrorMessageForYazaki(YHAISOrow, "品名コード", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                End If

                '数量１
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", YHAISOrow("FIELD37"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("SURYO1") = Val(O_VALUE).ToString("#,0.000")
                Else
                    T0005INProw("SURYO1") = YHAISOrow("FIELD37")
                    OutputErrorMessageForYazaki(YHAISOrow, "数量", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '品名２
                If YHAISOrow("FIELD43") <> "" Then
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", YHAISOrow("FIELD43"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        T0005INProw("PRODUCTCODE2") = O_VALUE
                    Else
                        T0005INProw("PRODUCTCODE2") = YHAISOrow("FIELD43")
                        OutputErrorMessageForYazaki(YHAISOrow, "品名コード2", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                End If

                '数量２
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", YHAISOrow("FIELD45"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("SURYO2") = Val(O_VALUE).ToString("#,0.000")
                Else
                    T0005INProw("SURYO2") = YHAISOrow("FIELD45")
                    OutputErrorMessageForYazaki(YHAISOrow, "数量2", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '品名３
                If YHAISOrow("FIELD47") <> "" Then
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", YHAISOrow("FIELD47"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        T0005INProw("PRODUCTCODE3") = O_VALUE
                    Else
                        T0005INProw("PRODUCTCODE3") = YHAISOrow("FIELD47")
                        OutputErrorMessageForYazaki(YHAISOrow, "品名コード3", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                End If

                '数量３
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", YHAISOrow("FIELD49"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("SURYO3") = Val(O_VALUE).ToString("#,0.000")
                Else
                    T0005INProw("SURYO3") = YHAISOrow("FIELD49")
                    OutputErrorMessageForYazaki(YHAISOrow, "数量3", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '品名４
                If YHAISOrow("FIELD58") <> "" Then
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", YHAISOrow("FIELD58"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        T0005INProw("PRODUCTCODE4") = O_VALUE
                    Else
                        T0005INProw("PRODUCTCODE4") = YHAISOrow("FIELD58")
                        OutputErrorMessageForYazaki(YHAISOrow, "品名コード4", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                End If

                '数量４
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", YHAISOrow("FIELD60"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("SURYO4") = Val(O_VALUE).ToString("#,0.000")
                Else
                    T0005INProw("SURYO4") = YHAISOrow("FIELD60")
                    OutputErrorMessageForYazaki(YHAISOrow, "数量4", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '品名５
                If YHAISOrow("FIELD62") <> "" Then
                    '①必須・項目属性チェック
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", YHAISOrow("FIELD62"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        T0005INProw("PRODUCTCODE5") = O_VALUE
                    Else
                        T0005INProw("PRODUCTCODE5") = YHAISOrow("FIELD62")
                        OutputErrorMessageForYazaki(YHAISOrow, "品名コード5", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                End If

                '数量５
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", YHAISOrow("FIELD64"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("SURYO5") = Val(O_VALUE).ToString("#,0.000")
                Else
                    T0005INProw("SURYO5") = YHAISOrow("FIELD64")
                    OutputErrorMessageForYazaki(YHAISOrow, "数量5", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '全数量
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", YHAISOrow("FIELD51"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("TOTALSURYO") = Val(O_VALUE).ToString("#,0.000")
                Else
                    T0005INProw("TOTALSURYO") = YHAISOrow("FIELD51")
                    OutputErrorMessageForYazaki(YHAISOrow, "全数量", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If

                '積置区分
                T0005INProw("TUMIOKIKBN") = ""

                '受注番号
                'Get_OrderNo(YHAISOtbl, )
                T0005INProw("ORDERNO") = ""
                T0005INProw("DETAILNO") = ""
                T0005INProw("TRIPNO") = ""
                T0005INProw("DROPNO") = ""

                '削除フラグ
                T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE

                If WW_ERRLIST.Count > 0 Then
                    If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                        Continue For
                    Else
                        'エラーフラグ
                        T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                Else
                    'エラーフラグ
                    T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                End If
                T0005INPtbl.Rows.Add(T0005INProw)
                '〇F1（出庫）が存在する場合、A1（出社）を作成する
                If WW_WORKKBN = "F1" Then
                    Dim WW_T5INProw As DataRow = T0005INPtbl.NewRow
                    WW_T5INProw.ItemArray = T0005INProw.ItemArray
                    For j As Integer = 0 To T0005WKtbl.Rows.Count - 1
                        If T0005WKtbl.Rows(j)("YMD") = WW_T5INProw("YMD") AndAlso
                           T0005WKtbl.Rows(j)("STAFFCODE") = WW_T5INProw("STAFFCODE") AndAlso
                           T0005WKtbl.Rows(j)("NIPPONO") = WW_T5INProw("NIPPONO") AndAlso
                           T0005WKtbl.Rows(j)("HDKBN") = "H" Then
                            WW_T5INProw("STDATE") = T0005WKtbl.Rows(j)("STDATE")
                            WW_T5INProw("STTIME") = T0005WKtbl.Rows(j)("STTIME")
                            WW_T5INProw("ENDDATE") = T0005WKtbl.Rows(j)("STDATE")
                            WW_T5INProw("ENDTIME") = T0005WKtbl.Rows(j)("STTIME")
                            WW_T5INProw("CTRL") = T0005WKtbl.Rows(j)("CTRL")
                            Exit For
                        End If
                    Next
                    WW_T5INProw("WORKKBN") = "A1"
                    WW_T5INProw("SEQ") = "000"
                    WW_T5INProw("WORKTIME") = "00:00"
                    WW_T5INProw("MOVETIME") = "00:00"
                    WW_T5INProw("ACTTIME") = "00:00"
                    WW_T5INProw("RUIDISTANCE") = "0.00"
                    WW_T5INProw("SOUDISTANCE") = "0.00"
                    WW_T5INProw("JIDISTANCE") = "0.00"
                    WW_T5INProw("KUDISTANCE") = "0.00"
                    T0005INPtbl.Rows.Add(WW_T5INProw)
                End If
                '〇F1（帰庫）が存在する場合、Z1（退社）を作成する
                If WW_WORKKBN = "F3" Then
                    Dim WW_T5INProw As DataRow = T0005INPtbl.NewRow
                    WW_T5INProw.ItemArray = T0005INProw.ItemArray
                    For j As Integer = 0 To T0005WKtbl.Rows.Count - 1
                        If T0005WKtbl.Rows(j)("YMD") = WW_T5INProw("YMD") AndAlso
                            T0005WKtbl.Rows(j)("STAFFCODE") = WW_T5INProw("STAFFCODE") AndAlso
                            T0005WKtbl.Rows(j)("NIPPONO") = WW_T5INProw("NIPPONO") AndAlso
                            T0005WKtbl.Rows(j)("HDKBN") = "H" Then
                            WW_T5INProw("STDATE") = T0005WKtbl.Rows(j)("ENDDATE")
                            WW_T5INProw("STTIME") = T0005WKtbl.Rows(j)("ENDTIME")
                            WW_T5INProw("ENDDATE") = T0005WKtbl.Rows(j)("ENDDATE")
                            WW_T5INProw("ENDTIME") = T0005WKtbl.Rows(j)("ENDTIME")
                            Exit For
                        End If
                    Next
                    WW_T5INProw("WORKKBN") = "Z1"
                    WW_T5INProw("SEQ") = "999"
                    WW_T5INProw("WORKTIME") = "00:00"
                    WW_T5INProw("MOVETIME") = "00:00"
                    WW_T5INProw("ACTTIME") = "00:00"
                    WW_T5INProw("RUIDISTANCE") = "0.00"
                    WW_T5INProw("SOUDISTANCE") = "0.00"
                    WW_T5INProw("JIDISTANCE") = "0.00"
                    WW_T5INProw("KUDISTANCE") = "0.00"
                    T0005INPtbl.Rows.Add(WW_T5INProw)
                End If

                WW_NIPPONO = T0005INProw("NIPPONO")
            Next

            '副乗務員(1)コードの設定、日報.csvより取得
            'ソート（日報番号、運行日、乗務員）
            CS0026TBLSORT.TABLE = T0005INPtbl
            CS0026TBLSORT.FILTER = ""
            CS0026TBLSORT.SORTING = "NIPPONO, YMD, STAFFCODE, HDKBN DESC, SEQ"
            T0005INPtbl = CS0026TBLSORT.sort()

            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                Dim WW_T0005INProw As DataRow = T0005INPtbl.Rows(i)
                If WW_T0005INProw("HDKBN") = "H" AndAlso
                    WW_T0005INProw("SUBSTAFFCODE") <> "" Then

                    For j As Integer = i + 1 To T0005INPtbl.Rows.Count - 1
                        Dim WW_T0005INProwj As DataRow = T0005INPtbl.Rows(j)
                        If WW_T0005INProwj("NIPPONO") = WW_T0005INProw("NIPPONO") Then
                            WW_T0005INProwj("SUBSTAFFCODE") = WW_T0005INProw("SUBSTAFFCODE")
                        Else
                            i = j - 1
                            Exit For
                        End If
                    Next
                End If
            Next

            '出庫、帰庫レコードに合計値をばらす
            Dim T0005HEADrow As DataRow = Nothing
            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                Dim WW_T0005INProw As DataRow = T0005INPtbl.Rows(i)
                If WW_T0005INProw("HDKBN") = "H" Then
                    T0005HEADrow = WW_T0005INProw
                End If
                If WW_T0005INProw("YMD") = T0005HEADrow("YMD") AndAlso
                   WW_T0005INProw("STAFFCODE") = T0005HEADrow("STAFFCODE") AndAlso
                   WW_T0005INProw("NIPPONO") = T0005HEADrow("NIPPONO") Then

                    If WW_T0005INProw("WORKKBN") = "F1" Then
                        WW_T0005INProw("STMATER") = T0005HEADrow("STMATER")
                    ElseIf WW_T0005INProw("WORKKBN") = "F3" Then
                        '通行料合計
                        WW_T0005INProw("TOTALTOLL") = T0005HEADrow("TOTALTOLL")
                        '帰庫メータ
                        WW_T0005INProw("ENDMATER") = T0005HEADrow("ENDMATER")
                        '給油
                        WW_T0005INProw("KYUYU") = T0005HEADrow("KYUYU")
                        '一般走行距離
                        WW_T0005INProw("IPPDISTANCE") = T0005HEADrow("IPPDISTANCE")
                        '高速走行距離
                        WW_T0005INProw("KOSDISTANCE") = T0005HEADrow("KOSDISTANCE")
                        '一般・実車距離
                        WW_T0005INProw("IPPJIDISTANCE") = T0005HEADrow("IPPJIDISTANCE")
                        '一般・空車距離
                        WW_T0005INProw("IPPKUDISTANCE") = T0005HEADrow("IPPKUDISTANCE")
                        '高速・実車距離
                        WW_T0005INProw("KOSJIDISTANCE") = T0005HEADrow("KOSJIDISTANCE")
                        '高速・空車距離
                        WW_T0005INProw("KOSKUDISTANCE") = T0005HEADrow("KOSKUDISTANCE")
                    End If
                End If
            Next

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
            CS0011LOGWRITE.INFSUBCLASS = "T0005tbl_Detail_Yazaki"       'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' エラーメッセージ編集（矢崎）
    ''' </summary>
    ''' <param name="I_ROW">エラー対象行</param>
    ''' <param name="I_FIELDNAME">フィールド名</param>
    ''' <param name="I_MESSAGE">出力メッセージ</param>
    ''' <param name="I_ERRORCODE">エラーコード</param>
    ''' <remarks></remarks>
    Sub OutputErrorMessageForYazaki(ByVal I_ROW As DataRow, ByVal I_FIELDNAME As String, I_MESSAGE As String, ByVal I_ERRORCODE As String)
        'エラーレポート編集
        Dim WW_ERR_MES As String = ""

        Select Case I_ERRORCODE
            Case C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_ERR_MES = "更新できないレコード(" & I_FIELDNAME & ")です。"
            Case C_MESSAGE_NO.BOX_ERROR_EXIST
                WW_ERR_MES = "・エラーが存在します。(" & I_FIELDNAME & "エラー)"
        End Select

        If I_ROW.Table.TableName = "YNIPPOtbl" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日報.csv  , "
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE & " , "
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 運行日    =" & I_ROW("FIELD2") & " , "
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日報番号  =" & I_ROW("FIELD1") & " , "
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員    =" & I_ROW("FIELD3") & "   "
        Else
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 配送.csv  , "
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE & " , "
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 着日付      =" & I_ROW("FIELD10") & " , "
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日報№      =" & I_ROW("FIELD1") & " , "
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員コード=" & I_ROW("FIELD2") & " , "
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細行番号  =" & I_ROW("FIELD6") & " , "
        End If
        SetErrorMessage(WW_ERR_MES)

        WW_ERRLIST_ALL.Add(I_ERRORCODE)
        WW_ERRLIST.Add(I_ERRORCODE)

    End Sub
    ''' <summary>
    ''' T0005tbl編集（矢崎のみ） ２マンの再編集（トリップ毎に存在をチェックし、存在しない場合の日報を削除
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub EditTwoManRecordForYazaki(ByRef O_RTN As String)

        Dim WW_Cols As String() = {"YMD", "NIPPONO", "STAFFCODE"}
        Dim WW_KEYtbl As DataTable
        Dim WW_T0005tbl As DataTable = T0005INPtbl.Clone
        Dim WW_T0005INPtbl As DataTable = T0005INPtbl.Clone
        Dim WW_TWOMANtbl As DataTable = T0005INPtbl.Clone
        Dim WW_TBLview As DataView
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_YMD As String = ""
        Dim WW_NIPPONO As String = ""
        Dim WW_STAFFCODE As String = ""

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '２マン対象データ抽出
            WW_TBLview = New DataView(T0005INPtbl)
            WW_TBLview.Sort = "YMD, NIPPONO, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_TBLview.RowFilter = "HDKBN = 'D'"

            WW_T0005INPtbl = WW_TBLview.ToTable

            '出庫日、乗務員でグループ化しキーテーブル作成
            WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)

            '+++++++++++++++++++++++++++++++++
            '出庫日、乗務員毎に処理
            Dim WW_IDX As Integer = 0
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                WW_T0005tbl.Clear()
                WW_TWOMANtbl.Clear()
                For i As Integer = WW_IDX To WW_T0005INPtbl.Rows.Count - 1
                    If WW_KEYrow("YMD") = WW_T0005INPtbl.Rows(i)("YMD") AndAlso
                        WW_KEYrow("NIPPONO") = WW_T0005INPtbl.Rows(i)("NIPPONO") AndAlso
                        WW_KEYrow("STAFFCODE") = WW_T0005INPtbl.Rows(i)("STAFFCODE") Then
                        Dim WW_Row As DataRow = WW_T0005tbl.NewRow
                        WW_Row.ItemArray = WW_T0005INPtbl.Rows(i).ItemArray
                        WW_T0005tbl.Rows.Add(WW_Row)
                    Else
                        WW_IDX = i
                        Exit For
                    End If
                Next

                '配送受注を検索し、副乗務員の存在をチェック
                Dim WW_SUBSTAFFCODE As String = ""
                Dim WW_SUBSTAFFCODE_SV As String = ""
                Dim WW_SUBSTAFFCODE2 As String = ""
                Dim WW_B2cnt As Integer = 0
                Dim WW_B3First As String = "OFF"
                Dim WW_STtrip As Integer = 0
                Dim WW_ENDtrip As Integer = 0
                Dim WW_STtrip2 As Integer = 0
                Dim WW_ENDtrip2 As Integer = 0
                For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                    Dim WW_T5row As DataRow = WW_T0005tbl.Rows(i)
                    If i = 0 AndAlso WW_T5row("SUBSTAFFCODE") <> "" Then
                        WW_STtrip2 = WW_T5row("TWOMANTRIP")
                    End If
                    If WW_T5row("WORKKBN") = "B3" Then
                        'B3（荷卸）が先頭（B2（積置）より前）の場合、B3のトリップでT4を検索する
                        If WW_B2cnt = 0 Then
                            WW_B3First = "ON"
                            WW_STtrip = WW_T5row("TWOMANTRIP")
                        End If
                    End If

                    If WW_T5row("WORKKBN") = "B2" Then
                        WW_B2cnt += 1
                        CheckTwoManCode(WW_T5row, WW_B3First, WW_SUBSTAFFCODE, WW_RTN)
                        If WW_RTN = C_MESSAGE_NO.DB_ERROR Then
                            O_RTN = WW_RTN
                            Exit Sub
                        End If

                        If WW_SUBSTAFFCODE <> "" Then
                            WW_SUBSTAFFCODE_SV = WW_SUBSTAFFCODE
                            If WW_B2cnt = 1 Then
                                If WW_B3First = "OFF" Then
                                    WW_STtrip = WW_T5row("TWOMANTRIP")
                                End If
                            End If
                            WW_ENDtrip = WW_T5row("TWOMANTRIP")
                        Else
                            WW_T5row("ORDERUMU") = "無"
                        End If
                    End If

                    If WW_T5row("SUBSTAFFCODE") <> "" Then
                        WW_ENDtrip2 = WW_T5row("TWOMANTRIP")
                        WW_SUBSTAFFCODE2 = WW_T5row("SUBSTAFFCODE")
                    End If
                Next

                Dim WW_TWOMAN As Boolean = False
                If WW_STtrip <> 0 AndAlso WW_ENDtrip <> 0 Then
                    WW_TWOMAN = True
                ElseIf WW_STtrip2 <> 0 AndAlso WW_ENDtrip2 <> 0 Then
                    WW_SUBSTAFFCODE_SV = WW_SUBSTAFFCODE2
                    WW_STtrip = WW_STtrip2
                    WW_ENDtrip = WW_ENDtrip2
                    WW_TWOMAN = True
                End If


                '切り出し
                If WW_TWOMAN Then
                    For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                        Dim WW_T5row As DataRow = WW_T0005tbl.Rows(i)

                        If WW_STtrip <= WW_T5row("TWOMANTRIP") AndAlso WW_T5row("TWOMANTRIP") <= WW_ENDtrip Then
                            Dim TWOMANrow As DataRow = WW_TWOMANtbl.NewRow
                            TWOMANrow.ItemArray = WW_T5row.ItemArray

                            '２マンレコード編集
                            TWOMANrow("STAFFCODE") = WW_SUBSTAFFCODE_SV
                            TWOMANrow("STAFFNAMES") = ""
                            CodeToName("STAFFCODE", TWOMANrow("STAFFCODE"), TWOMANrow("STAFFNAMES"), WW_RTN)
                            TWOMANrow("SUBSTAFFCODE") = ""
                            TWOMANrow("SUBSTAFFNAMES") = ""
                            TWOMANrow("CREWKBN") = "2"
                            TWOMANrow("CREWKBNNAMES") = ""
                            CodeToName("CREWKBN", WW_T0005tbl.Rows(i)("CREWKBN"), WW_T0005tbl.Rows(i)("CREWKBNNAMES"), WW_RTN)


                            WW_TWOMANtbl.Rows.Add(TWOMANrow)
                        End If
                    Next
                End If

                Dim WW_F1cnt As Integer = 0
                Dim WW_F3cnt As Integer = 0
                For i As Integer = 0 To WW_TWOMANtbl.Rows.Count - 1
                    If WW_TWOMANtbl.Rows(i)("WORKKBN") = "F1" Then
                        WW_F1cnt += 1
                    End If
                    If WW_TWOMANtbl.Rows(i)("WORKKBN") = "F3" Then
                        WW_F3cnt += 1
                    End If
                Next

                Dim WW_WORKtbl As DataTable = WW_TWOMANtbl.Clone
                WW_WORKtbl.Clear()
                If WW_F1cnt = 0 Then
                    For i As Integer = 0 To WW_TWOMANtbl.Rows.Count - 1
                        Dim WW_T5row As DataRow = WW_TWOMANtbl.Rows(i)
                        Dim TWOMANrow As DataRow = WW_WORKtbl.NewRow
                        '出庫か帰庫がない
                        T0005COM.InitialT5INPRow(TWOMANrow)
                        TWOMANrow("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                        TWOMANrow("SHIPORG") = work.WF_SEL_UORG.Text
                        '開始日時、前のレコードの終了日時
                        TWOMANrow("STDATE") = WW_T5row("STDATE")
                        TWOMANrow("STTIME") = WW_T5row("STTIME")
                        TWOMANrow("ENDTIME") = WW_T5row("STTIME")
                        '終了日時、後ろレコードの開始日時
                        TWOMANrow("ENDDATE") = WW_T5row("STDATE")

                        'その他の項目は、現在のレコードをコピーする
                        TWOMANrow("YMD") = WW_T5row("YMD")
                        TWOMANrow("GSHABAN") = WW_T5row("GSHABAN")
                        TWOMANrow("NIPPONO") = WW_T5row("NIPPONO")
                        TWOMANrow("STAFFCODE") = WW_SUBSTAFFCODE_SV
                        TWOMANrow("SUBSTAFFCODE") = ""
                        TWOMANrow("CREWKBN") = WW_T5row("CREWKBN")
                        TWOMANrow("TERMKBN") = WW_T5row("TERMKBN")
                        TWOMANrow("HDKBN") = "D"
                        TWOMANrow("WORKKBN") = "F1"
                        TWOMANrow("SEQ") = "000" '仮SEQ

                        TWOMANrow("CAMPNAMES") = ""
                        CodeToName("CAMPCODE", TWOMANrow("CAMPCODE"), TWOMANrow("CAMPNAMES"), WW_RTN)
                        TWOMANrow("SHIPORGNAMES") = ""
                        CodeToName("SHIPORG", TWOMANrow("SHIPORG"), TWOMANrow("SHIPORGNAMES"), WW_RTN)
                        TWOMANrow("TERMKBNNAMES") = ""
                        CodeToName("TERMKBN", TWOMANrow("TERMKBN"), TWOMANrow("TERMKBNNAMES"), WW_RTN)
                        TWOMANrow("WORKKBNNAMES") = ""
                        CodeToName("WORKKBN", TWOMANrow("WORKKBN"), TWOMANrow("WORKKBNNAMES"), WW_RTN)
                        TWOMANrow("STAFFNAMES") = ""
                        CodeToName("STAFFCODE", TWOMANrow("STAFFCODE"), TWOMANrow("STAFFNAMES"), WW_RTN)
                        TWOMANrow("CREWKBNNAMES") = ""
                        CodeToName("CREWKBN", TWOMANrow("CREWKBN"), TWOMANrow("CREWKBNNAMES"), WW_RTN)
                        WW_WORKtbl.Rows.Add(TWOMANrow)

                        Dim TWOMANrow2 As DataRow = WW_WORKtbl.NewRow
                        TWOMANrow2.ItemArray = TWOMANrow.ItemArray
                        TWOMANrow2("WORKKBN") = "A1"
                        TWOMANrow2("WORKKBNNAMES") = ""
                        CodeToName("WORKKBN", TWOMANrow2("WORKKBN"), TWOMANrow2("WORKKBNNAMES"), WW_RTN)

                        WW_WORKtbl.Rows.Add(TWOMANrow2)

                        Exit For
                    Next
                    WW_TWOMANtbl.Merge(WW_WORKtbl)
                End If

                If WW_F3cnt = 0 Then
                    For i As Integer = WW_TWOMANtbl.Rows.Count - 1 To 0 Step -1
                        Dim WW_T5row As DataRow = WW_TWOMANtbl.Rows(i)
                        Dim TWOMANrow As DataRow = WW_WORKtbl.NewRow
                        '出庫か帰庫がない
                        T0005COM.InitialT5INPRow(TWOMANrow)
                        TWOMANrow("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                        TWOMANrow("SHIPORG") = work.WF_SEL_UORG.Text
                        '開始日時、前のレコードの終了日時
                        TWOMANrow("STDATE") = WW_T5row("ENDDATE")
                        TWOMANrow("STTIME") = WW_T5row("ENDTIME")
                        TWOMANrow("ENDTIME") = WW_T5row("ENDTIME")
                        '終了日時、後ろレコードの開始日時
                        TWOMANrow("ENDDATE") = WW_T5row("ENDDATE")

                        'その他の項目は、現在のレコードをコピーする
                        TWOMANrow("YMD") = WW_T5row("YMD")
                        TWOMANrow("GSHABAN") = WW_T5row("GSHABAN")
                        TWOMANrow("NIPPONO") = WW_T5row("NIPPONO")
                        TWOMANrow("STAFFCODE") = WW_SUBSTAFFCODE_SV
                        TWOMANrow("SUBSTAFFCODE") = ""
                        TWOMANrow("CREWKBN") = WW_T5row("CREWKBN")
                        TWOMANrow("TERMKBN") = WW_T5row("TERMKBN")
                        TWOMANrow("HDKBN") = "D"
                        TWOMANrow("WORKKBN") = "F3"
                        TWOMANrow("SEQ") = "999" '仮SEQ

                        TWOMANrow("CAMPNAMES") = ""
                        CodeToName("CAMPCODE", TWOMANrow("CAMPCODE"), TWOMANrow("CAMPNAMES"), WW_RTN)
                        TWOMANrow("SHIPORGNAMES") = ""
                        CodeToName("SHIPORG", TWOMANrow("SHIPORG"), TWOMANrow("SHIPORGNAMES"), WW_RTN)
                        TWOMANrow("TERMKBNNAMES") = ""
                        CodeToName("TERMKBN", TWOMANrow("TERMKBN"), TWOMANrow("TERMKBNNAMES"), WW_RTN)
                        TWOMANrow("WORKKBNNAMES") = ""
                        CodeToName("WORKKBN", TWOMANrow("WORKKBN"), TWOMANrow("WORKKBNNAMES"), WW_RTN)
                        TWOMANrow("STAFFNAMES") = ""
                        CodeToName("STAFFCODE", TWOMANrow("STAFFCODE"), TWOMANrow("STAFFNAMES"), WW_RTN)
                        TWOMANrow("CREWKBNNAMES") = ""
                        CodeToName("CREWKBN", TWOMANrow("CREWKBN"), TWOMANrow("CREWKBNNAMES"), WW_RTN)
                        WW_WORKtbl.Rows.Add(TWOMANrow)

                        Dim TWOMANrow2 As DataRow = WW_WORKtbl.NewRow
                        TWOMANrow2.ItemArray = TWOMANrow.ItemArray
                        TWOMANrow2("WORKKBN") = "Z1"
                        TWOMANrow2("WORKKBNNAMES") = ""
                        CodeToName("WORKKBN", TWOMANrow2("WORKKBN"), TWOMANrow2("WORKKBNNAMES"), WW_RTN)

                        WW_WORKtbl.Rows.Add(TWOMANrow2)

                        Exit For
                    Next

                    WW_TWOMANtbl.Merge(WW_WORKtbl)
                End If

                '２マンレコードの追加
                T0005INPtbl.Merge(WW_TWOMANtbl)

            Next

            '------------------------------------------------------------
            '■出庫日、従業員 単位
            '  出庫日、従業員毎に集約し直す
            '------------------------------------------------------------
            CreateT0005Header(T0005INPtbl)

            '---------------------------------------------------
            '■出庫日、従業員 単位
            '  明細行番号（並び順）の振り直し
            '---------------------------------------------------
            Dim WW_SEQ As Integer = 1

            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                '行番号の採番
                If T0005INPtbl.Rows(i)("HDKBN") = "H" Then
                    WW_SEQ = 1
                    T0005INPtbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")
                    Continue For
                End If
                T0005INPtbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")
                WW_SEQ = WW_SEQ + 1
            Next

            WW_KEYtbl.Dispose()
            WW_KEYtbl = Nothing
            WW_TBLview.Dispose()
            WW_TBLview = Nothing
            WW_T0005tbl.Dispose()
            WW_T0005tbl = Nothing
            WW_T0005INPtbl.Dispose()
            WW_T0005INPtbl = Nothing
            WW_TWOMANtbl.Dispose()
            WW_TWOMANtbl = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
            CS0011LOGWRITE.INFSUBCLASS = "T0005tbl_Edit"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    ''' 矢崎の作業区分を光英用作業区分に変換する
    ''' </summary>
    ''' <param name="I_WORKKBN"></param>
    ''' <returns>作業区分</returns>
    ''' <remarks></remarks>
    Protected Function ConvertYazakiWorkKbn(ByVal I_WORKKBN As String) As String
        Dim WW_WorkKbn As String
        '矢崎の場合
        Select Case I_WORKKBN
            Case "出庫"
                WW_WorkKbn = "F1"
            Case "点検"
                WW_WorkKbn = "B4"
            Case "荷積"
                WW_WorkKbn = "B2"
            Case "荷卸"
                WW_WorkKbn = "B3"
            Case "待機"
                WW_WorkKbn = "BA"
            Case "他作業"
                WW_WorkKbn = "BX"
            Case "休憩", "休息"
                WW_WorkKbn = "BB"
            Case "帰庫"
                WW_WorkKbn = "F3"
            Case "配送"
                WW_WorkKbn = "G1"
            Case Else
                WW_WorkKbn = ""
        End Select

        Return WW_WorkKbn
    End Function

#End Region

#Region "<<KOUEI>>"
    ''' <summary>
    ''' CSV取込（光英）処理      
    ''' </summary>
    ''' <param name="I_MODE"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_KOUEI(ByVal I_MODE As String, ByRef O_RTN As String)
        UPLOAD_KOUEI(I_MODE, CS0050Session.UPLOAD_PATH & "\" & "UPLOAD_TMP" & "\" & CS0050Session.USERID, O_RTN)
    End Sub
    ''' <summary>
    ''' CSV取込（光英）処理      
    ''' </summary>
    ''' <param name="I_MODE"></param>
    ''' <param name="I_FILEDIR">CSVファイルの保存箇所</param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_KOUEI(ByVal I_MODE As String, ByVal I_FILEDIR As String, ByRef O_RTN As String)

        'CSVファイル名
        Dim WW_FileName1 As String = String.Empty
        Dim WW_FileName2 As String = String.Empty
        Dim WW_FILEDATE As String = String.Empty
        Dim WW_JX_OLD_MODE As Boolean = False
        Dim WW_OLD_MODE As Boolean = False
        Dim WW_SYASAI_PREFIX As String() = Nothing
        '■■■ チェック処理 ■■■
        O_RTN = C_MESSAGE_NO.NORMAL
        rightview.SetErrorReport("")

        '-----------------------------------------------
        'CSVファイル取得
        '-----------------------------------------------
        If I_MODE = GRT00005WRKINC.TERM_TYPE.JOT Then
            WW_SYASAI_PREFIX = New String() {"jot_jotsyasai"}
        ElseIf I_MODE = GRT00005WRKINC.TERM_TYPE.JX Then
            WW_SYASAI_PREFIX = New String() {"jx_jotsyasai"}
        ElseIf I_MODE = GRT00005WRKINC.TERM_TYPE.TG Then
            WW_SYASAI_PREFIX = New String() {"tg_jotsyasai"}
        ElseIf I_MODE = GRT00005WRKINC.TERM_TYPE.COSMO Then
            WW_SYASAI_PREFIX = New String() {"cosmo_jotsyasai"}
        End If
        For Each WW_PREFIX As String In WW_SYASAI_PREFIX
            WW_JX_OLD_MODE = False
            '〇ディレクトリの確認
            If String.IsNullOrEmpty(I_FILEDIR) Then
                Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If
            Dim WW_Files As String() = System.IO.Directory.GetFiles(I_FILEDIR, WW_PREFIX.ToString & "_*.csv")
            If WW_Files.Count = 0 Then
                WW_Files = System.IO.Directory.GetFiles(I_FILEDIR, "*syasai.*")
                WW_OLD_MODE = True
            End If
            For Each tempFile As String In WW_Files
                If WW_OLD_MODE = False Then
                    WW_FileName1 = tempFile
                End If

                'Try
                'Dim fp As New FileInfo(tempFile)
                ''Dim tmpDate As String = If(fp.Name.Length > 21, fp.Name.Substring(fp.Name.LastIndexOf(".") - 14, 14), "-")
                'Dim tmpDate As String = ""
                'Dim tmp() As String = fp.Name.Split("_")
                'If fp.Name.Length > 21 Then
                '    tmpDate = tmp(2)
                'End If
                '' ファイルパスからファイル名を取得
                'If fp.Name.ToLower.StartsWith(WW_PREFIX) AndAlso WW_FILEDATE < tmpDate Then
                '    WW_FILEDATE = tmpDate
                '    WW_FileName1 = tempFile
                'End If
                'Catch ex As Exception
                '    O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                '    Master.Output(O_RTN, C_MESSAGE_TYPE.ERR, "光英 csv read")
                '    CS0011LOGWRITE.INFSUBCLASS = "UPLOAD_KOUEI"                    'SUBクラス名
                '    CS0011LOGWRITE.INFPOSI = "光英 csv read"                       '
                '    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                '    CS0011LOGWRITE.TEXT = ex.ToString
                '    CS0011LOGWRITE.MESSAGENO = O_RTN
                '    CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
                '    Exit Sub
                'End Try
                '〇JX対象は旧仕様の読み込みも行う
                If I_MODE = GRT00005WRKINC.TERM_TYPE.JX AndAlso String.IsNullOrEmpty(WW_FileName1) Then
                    Try
                        For Each tempFile2 As String In System.IO.Directory.GetFiles(I_FILEDIR, "*.*")
                            ' ファイルパスからファイル名を取得
                            If tempFile2.ToLower Like "*syasai.csv" Then
                                WW_FileName1 = tempFile2
                            End If
                            If tempFile2.ToLower Like "*yotei.csv" Then
                                WW_FileName2 = tempFile2
                            End If
                        Next
                        WW_JX_OLD_MODE = True
                    Catch ex As Exception
                        O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
                        Master.Output(O_RTN, C_MESSAGE_TYPE.ERR, "光英 csv read")
                        CS0011LOGWRITE.INFSUBCLASS = "UPLOAD_KOUEI"                    'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "光英 csv read"                       '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                        CS0011LOGWRITE.TEXT = ex.ToString
                        CS0011LOGWRITE.MESSAGENO = O_RTN
                        CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
                        Exit Sub
                    End Try
                ElseIf I_MODE = GRT00005WRKINC.TERM_TYPE.JOT AndAlso String.IsNullOrEmpty(WW_FileName1) Then
                    Try
                        For Each tempFile2 As String In System.IO.Directory.GetFiles(I_FILEDIR, "*.*")
                            ' ファイルパスからファイル名を取得
                            If tempFile2.ToLower Like "*exsyasai.csv" Then
                                WW_FileName1 = tempFile2
                            End If
                        Next
                        'WW_JX_OLD_MODE = True
                    Catch ex As Exception
                        O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
                        Master.Output(O_RTN, C_MESSAGE_TYPE.ERR, "光英 csv read")
                        CS0011LOGWRITE.INFSUBCLASS = "UPLOAD_KOUEI"                    'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "光英 csv read"                       '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                        CS0011LOGWRITE.TEXT = ex.ToString
                        CS0011LOGWRITE.MESSAGENO = O_RTN
                        CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
                        Exit Sub
                    End Try
                End If

                If String.IsNullOrEmpty(WW_FileName1) Then
                    O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
                    Master.Output(O_RTN, C_MESSAGE_TYPE.ERR, "光英 csv read")
                    CS0011LOGWRITE.INFSUBCLASS = "UPLOAD_KOUEI"                 'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "光英(ENEX) csv read"                    '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                    CS0011LOGWRITE.TEXT = "ファイル名：「" & WW_SYASAI_PREFIX.ToString & "」が必要です"
                    CS0011LOGWRITE.MESSAGENO = O_RTN
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End If
                '-----------------------------------------------
                '●CSV読み込み（データテーブルに格納）
                '-----------------------------------------------
                '車載CSV格納テーブル
                AddCsvColumn(KSYASAItbl, 53)

                '日報DB更新用テーブル
                T0005COM.AddColumnT0005tbl(T0005INPtbl)
                AddColumnForT0005WKTbl()

                '車載CSV
                ReadCsvFile(WW_FileName1, KSYASAItbl, WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                '-----------------------------------------------
                '■ヘッダ（キー項目）レコード編集（車載CSVより作成）
                '-----------------------------------------------
                WW_ERRLIST = New List(Of String)
                WW_ERRLIST_ALL = New List(Of String)
                WW_ERRLISTCNT = 0

                CreateT0005tblHeaderForKouei(I_MODE, WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                '-----------------------------------------------
                '■明細コード編集（車載CSVより作成）
                '-----------------------------------------------
                For Each T0005WKrow As DataRow In T0005WKtbl.Rows
                    CreateT0005tblDetailForKouei(I_MODE, WW_JX_OLD_MODE, T0005WKrow("STROW"), T0005WKrow("ENDROW"), WW_ERRCODE)
                    If Not isNormal(WW_ERRCODE) Then
                        O_RTN = WW_ERRCODE
                        Exit Sub
                    End If
                Next

                '-----------------------------------------------
                '■T0005tbl再編集（）
                '-----------------------------------------------
                '不要なデータ（作業区分）を除いたため明細行番号の再符番する
                EditT0005Tbl(WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                '-----------------------------------------------
                '■出荷場所、届先ＤＢ編集
                '-----------------------------------------------
                EditMC006tbl(I_MODE, WW_JX_OLD_MODE, WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                '-----------------------------------------------
                '■項目変換（T0005INPtblより）
                '-----------------------------------------------
                ConvT0005tblData(T0005INPtbl, WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                '-----------------------------------------------
                '■２マンの再編集（T0005INPtblより）
                '-----------------------------------------------
                EditTwoManForKouei(T0005WKtbl, WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                '-----------------------------------------------
                '■関連チェック（T0005INPtblより）
                '-----------------------------------------------
                For Each T0005INProw As DataRow In T0005INPtbl.Rows
                    If T0005INProw("HDKBN") = "H" Then
                        Continue For
                    End If

                    CheckT0005INPRow(T0005INProw, WW_ERRCODE)
                Next

                '-----------------------------------------------
                '■表更新
                '-----------------------------------------------
                UpdateGridData(WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                WF_KoueiLoadFile.Items.Add(New ListItem(WW_FileName1))
            Next
        Next

        '○メッセージ表示
        If WW_ERRLIST_ALL.Count > 0 Then
            If WW_ERRLIST_ALL.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ElseIf WW_ERRLIST_ALL.IndexOf(C_MESSAGE_NO.BOX_ERROR_EXIST) >= 0 Then
                O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            Else
                O_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST
            End If
            Exit Sub
        End If
        If O_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST Then
            Master.Output(O_RTN, C_MESSAGE_TYPE.WAR)
            Exit Sub
        End If

        If Not isNormal(O_RTN) Then Master.Output(O_RTN, C_MESSAGE_TYPE.ERR)
    End Sub
    ''' <summary>
    ''' 日報ＤＢ（ヘッダ）キー項目編集（光英）
    ''' </summary>
    ''' <param name="I_MODE">登録モード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CreateT0005tblHeaderForKouei(ByVal I_MODE As String, ByRef O_RTN As String)

        'ブレイクキー
        Dim WW_YMD As String = ""
        Dim WW_YYYYMMDD As String = ""
        Dim WW_NIPPONO As String = ""
        Dim WW_STAFFCODE As String = ""
        Dim WW_STAFFCODE2 As String = ""
        Dim WW_OLD_YMD As String = ""
        Dim WW_OLD_YYYYMMDD As String = ""
        Dim WW_OLD_NIPPONO As String = ""
        Dim WW_OLD_STAFFCODE As String = ""
        Dim WW_STROW As Integer = 0
        Dim WW_ENDROW As Integer = 0
        Dim WW_STROW2 As Integer = 0
        Dim WW_ENDROW2 As Integer = 0
        Dim WW_2ManFirst As Integer = 0
        Dim WW_STDATE2 As String = ""
        Dim WW_STTIME2 As String = ""
        Dim WW_ENDDATE2 As String = ""
        Dim WW_ENDTIME2 As String = ""
        Dim WW_TIME As String = ""
        Dim WW_ROW As DataRow = T0005WKtbl.NewRow
        Dim WW_ENDYMD As String = ""
        Dim WW_X1 As Integer = 0

        O_RTN = C_MESSAGE_NO.NORMAL

        Try
            S0013tbl = New DataTable
            Dim I_VALUE As String = String.Empty
            Dim O_VALUE As String = String.Empty
            Dim O_MESSAGE_NO As String = C_MESSAGE_NO.NORMAL
            Dim O_CHECKREPORT As String = String.Empty

            For i As Integer = 0 To KSYASAItbl.Rows.Count - 1
                Dim KSYASAIrow As DataRow = KSYASAItbl.Rows(i)
                WW_ERRLIST = New List(Of String)
                WW_ERRLIST_ALL = New List(Of String)

                If i = 0 Then
                    WW_2ManFirst = 0
                    WW_OLD_YMD = KSYASAIrow("FIELD4")
                    WW_OLD_NIPPONO = KSYASAIrow("FIELD10")
                    Select Case I_MODE
                        Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                            I_VALUE = Mid(KSYASAIrow("FIELD6"), 1, 5)
                        Case GRT00005WRKINC.TERM_TYPE.JOT
                            I_VALUE = KSYASAIrow("FIELD6")
                        Case GRT00005WRKINC.TERM_TYPE.COSMO
                            'TODO
                            I_VALUE = KSYASAIrow("FIELD6")
                    End Select
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then WW_OLD_STAFFCODE = O_VALUE
                End If

                WW_ROW = T0005WKtbl.NewRow
                WW_ERRLIST_ALL.Clear()
                WW_ERRLIST.Clear()

                '稼働日
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "YMD", Mid(KSYASAIrow("FIELD4"), 1, 4) & "/" & Mid(KSYASAIrow("FIELD4"), 5, 2) & "/" & Mid(KSYASAIrow("FIELD4"), 7, 2), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    If Not String.IsNullOrEmpty(O_VALUE) Then
                        WW_YMD = KSYASAIrow("FIELD4")
                        WW_YYYYMMDD = O_VALUE
                        If CDate(O_VALUE) < CDate(work.WF_SEL_STYMD.Text) OrElse
                           CDate(O_VALUE) > CDate(work.WF_SEL_ENDYMD.Text) Then
                            OutputErrorMessageByKouei(KSYASAIrow, "稼働日", "稼働日が範囲対象外です。", C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                        End If
                    Else
                        OutputErrorMessageByKouei(KSYASAIrow, "稼働日", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                    End If
                Else
                    OutputErrorMessageByKouei(KSYASAIrow, "稼働日", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                End If

                '日報番号
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NIPPONO", KSYASAIrow("FIELD10"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    WW_NIPPONO = KSYASAIrow("FIELD10")
                Else
                    OutputErrorMessageByKouei(KSYASAIrow, "日報番号", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                End If

                '乗務員
                '①必須・項目属性チェック
                Select Case I_MODE
                    Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                        I_VALUE = Mid(KSYASAIrow("FIELD6"), 1, 5)
                    Case GRT00005WRKINC.TERM_TYPE.JOT
                        I_VALUE = KSYASAIrow("FIELD6")
                        'I_VALUE = Mid(KSYASAIrow("FIELD6"), 1, 5)
                    Case GRT00005WRKINC.TERM_TYPE.COSMO
                        'TODO
                        I_VALUE = KSYASAIrow("FIELD6")
                End Select
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    WW_STAFFCODE = O_VALUE
                Else
                    OutputErrorMessageByKouei(KSYASAIrow, "乗務員", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                End If

                '終了日取得
                If KSYASAIrow("FIELD13") = "F1" Then
                    WW_ENDYMD = Mid(KSYASAIrow("FIELD32"), 1, 4) & "/" & Mid(KSYASAIrow("FIELD32"), 5, 2) & "/" & Mid(KSYASAIrow("FIELD32"), 7, 2)
                End If

                '２マン開始または、副乗務員に入力有（先頭）
                If KSYASAIrow("FIELD13") = "F7" OrElse (KSYASAIrow("FIELD18") <> "0" AndAlso WW_2ManFirst = 0) Then
                    WW_STROW2 = i
                    WW_2ManFirst = 1

                    '①必須・項目属性チェック
                    Select Case I_MODE
                        Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                            I_VALUE = Mid(KSYASAIrow("FIELD18"), 1, KSYASAIrow("FIELD18").length)
                        Case GRT00005WRKINC.TERM_TYPE.JOT
                            I_VALUE = KSYASAIrow("FIELD18")
                            'I_VALUE = Mid(KSYASAIrow("FIELD18"), 1, 5)
                        Case GRT00005WRKINC.TERM_TYPE.COSMO
                            'TODO
                            I_VALUE = KSYASAIrow("FIELD18")
                    End Select
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        WW_STAFFCODE2 = O_VALUE
                    Else
                        '乗務員＝副乗務員（自分が副乗務員の場合、エラー　→　乗務員＝副乗務員でデータ作成）
                        If WW_STAFFCODE = O_VALUE Then
                            OutputErrorMessageByKouei(KSYASAIrow, "２マン社員コード", "乗務員と副乗務員が同じため副乗務員の日報は作成しません", C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If
                        WW_STROW2 = 0
                        WW_STAFFCODE2 = ""
                    End If


                    '情報１（始業日）
                    '①必須・項目属性チェック
                    If KSYASAIrow("FIELD13") = "F7" Then
                        I_VALUE = Mid(KSYASAIrow("FIELD31"), 1, 4) & "/" & Mid(KSYASAIrow("FIELD31"), 5, 2) & "/" & Mid(KSYASAIrow("FIELD31"), 7, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STDATE", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            WW_STDATE2 = O_VALUE
                        Else
                            WW_STROW2 = 0
                            WW_STDATE2 = ""
                            OutputErrorMessageByKouei(KSYASAIrow, "情報１（２マン開始日）", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If
                    Else
                        WW_STDATE2 = WW_YYYYMMDD
                    End If

                    '開始時刻
                    '①必須・項目属性チェック
                    WW_TIME = KSYASAIrow("FIELD27").PadLeft(4, "0")
                    I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        If O_VALUE <> "" Then
                            WW_STTIME2 = CDate(O_VALUE).ToString("HH:mm")
                        Else
                            WW_STROW2 = 0
                            WW_STTIME2 = ""
                            OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If
                    Else
                        WW_STROW2 = 0
                        WW_STTIME2 = ""
                        OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                    End If
                End If
                '日跨り　X1
                If KSYASAIrow("FIELD13") = "X1" Then WW_X1 += 1

                If KSYASAIrow("FIELD13") = "F8" OrElse KSYASAIrow("FIELD18") <> "0" Then
                    'F7でエラーがなければ
                    If WW_STROW2 <> 0 Then
                        WW_ENDROW2 = i

                        '情報１（終業日）
                        '①必須・項目属性チェック
                        If KSYASAIrow("FIELD13") = "F8" Then
                            I_VALUE = Mid(KSYASAIrow("FIELD32"), 1, 4) & "/" & Mid(KSYASAIrow("FIELD32"), 5, 2) & "/" & Mid(KSYASAIrow("FIELD32"), 7, 2)
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDDATE", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                WW_ENDDATE2 = O_VALUE
                            Else
                                WW_ENDROW2 = 0
                                WW_ENDDATE2 = ""
                                OutputErrorMessageByKouei(KSYASAIrow, "情報２（２マン終了日）", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            If WW_X1 > 0 Then
                                WW_ENDDATE2 = WW_ENDYMD
                            Else
                                WW_ENDDATE2 = WW_YYYYMMDD
                            End If
                        End If

                        '開始時刻
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD28").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If O_VALUE <> "" Then
                                WW_ENDTIME2 = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                WW_ENDROW2 = 0
                                WW_ENDTIME2 = ""
                                OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            WW_ENDROW2 = 0
                            WW_ENDTIME2 = ""
                            OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If
                    End If
                End If

                'キー項目にエラーがなければ、ヘッダーレコードを作成する
                If WW_ERRLIST.Count = 0 Then
                    If WW_OLD_YMD = WW_YMD AndAlso
                       WW_OLD_NIPPONO = WW_NIPPONO AndAlso
                       WW_OLD_STAFFCODE = WW_STAFFCODE Then
                        WW_ENDROW = i
                    Else
                        'ヘッダーレコードの作成
                        WW_ROW("YMD") = WW_OLD_YYYYMMDD
                        WW_ROW("NIPPONO") = WW_OLD_NIPPONO
                        WW_ROW("STAFFCODE") = WW_OLD_STAFFCODE
                        WW_ROW("STROW") = WW_STROW
                        WW_ROW("ENDROW") = WW_ENDROW
                        WW_ROW("STAFFCODE2") = WW_STAFFCODE2
                        WW_ROW("STROW2") = WW_STROW2
                        WW_ROW("ENDROW2") = WW_ENDROW2
                        WW_ROW("STDATE2") = WW_STDATE2
                        WW_ROW("STTIME2") = WW_STTIME2
                        WW_ROW("ENDDATE2") = WW_ENDDATE2
                        WW_ROW("ENDTIME2") = WW_ENDTIME2
                        T0005WKtbl.Rows.Add(WW_ROW)

                        WW_STROW = i
                        WW_ENDROW = 0
                        WW_STAFFCODE2 = ""
                        WW_STROW2 = 0
                        WW_STDATE2 = ""
                        WW_STTIME2 = ""
                        WW_ENDROW2 = 0
                        WW_ENDDATE2 = ""
                        WW_ENDTIME2 = ""
                        WW_2ManFirst = 0

                        WW_ENDYMD = ""
                        WW_X1 = 0

                    End If
                    WW_OLD_YMD = WW_YMD
                    WW_OLD_YYYYMMDD = WW_YYYYMMDD
                    WW_OLD_NIPPONO = WW_NIPPONO
                    WW_OLD_STAFFCODE = WW_STAFFCODE
                End If
            Next

            If KSYASAItbl.Rows.Count > 0 AndAlso WW_ERRLIST.Count = 0 Then
                'ヘッダーレコードの作成
                WW_ROW("YMD") = WW_OLD_YYYYMMDD
                WW_ROW("NIPPONO") = WW_OLD_NIPPONO
                WW_ROW("STAFFCODE") = WW_OLD_STAFFCODE
                WW_ROW("STROW") = WW_STROW
                WW_ROW("ENDROW") = WW_ENDROW
                WW_ROW("STAFFCODE2") = WW_STAFFCODE2
                WW_ROW("STROW2") = WW_STROW2
                WW_ROW("ENDROW2") = WW_ENDROW2
                WW_ROW("STDATE2") = WW_STDATE2
                WW_ROW("STTIME2") = WW_STTIME2
                WW_ROW("ENDDATE2") = WW_ENDDATE2
                WW_ROW("ENDTIME2") = WW_ENDTIME2
                T0005WKtbl.Rows.Add(WW_ROW)
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
            CS0011LOGWRITE.INFSUBCLASS = "T0005tbl_KeyItem_Kouei"          'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub

        End Try
    End Sub

    ''' <summary>
    ''' 日報ＤＢ（明細）編集（光英）
    ''' </summary>
    ''' <param name="I_MODE">登録モード</param>
    ''' <param name="I_STSEQ">開始順</param>
    ''' <param name="I_ENDSEQ">終了順</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CreateT0005tblDetailForKouei(ByVal I_MODE As String,
                                        ByVal I_LEGACY_MODE As Boolean,
                                        ByVal I_STSEQ As Integer,
                                        ByVal I_ENDSEQ As Integer,
                                        ByRef O_RTN As String)

        Dim WW_WRITE_FLG As Boolean = False                          '出力フラグ
        Dim WW_CHGYMD_FLG As Boolean = False                         '日付跨ぎフラグ（X1レコードより）
        Dim WW_TWOMAN_FLG As Boolean = False                         '２マンフラグ

        Dim WW_OLD_YMD As String = ""
        Dim WW_OLD_NIPPONO As String = ""

        Dim WW_MOVETIME As String = 0

        Dim WW_SOUDISTANCE As Decimal = 0                           '走行距離
        Dim WW_JIDISTANCE As Decimal = 0                            '実車距離
        Dim WW_KUDISTANCE As Decimal = 0                            '空車距離
        Dim WW_IPPDISTANCE As Decimal = 0                           '一般走行距離
        Dim WW_KOSDISTANCE As Decimal = 0                           '高速走行距離
        Dim WW_IPPJIDISTANCE As Decimal = 0                         '一般・実車距離
        Dim WW_IPPKUDISTANCE As Decimal = 0                         '一般・空車距離
        Dim WW_KOSJIDISTANCE As Decimal = 0                         '高速・実車距離
        Dim WW_KOSKUDISTANCE As Decimal = 0                         '高速・空車距離
        Dim WW_SOUDISTANCE_T As Decimal = 0                         '走行距離ヘッダ用
        Dim WW_JIDISTANCE_T As Decimal = 0                          '実車距離ヘッダ用
        Dim WW_KUDISTANCE_T As Decimal = 0                          '空車距離ヘッダ用
        Dim WW_IPPDISTANCE_T As Decimal = 0                         '一般走行距離ヘッダ用
        Dim WW_KOSDISTANCE_T As Decimal = 0                         '高速走行距離ヘッダ用
        Dim WW_IPPJIDISTANCE_T As Decimal = 0                       '一般・実車距離ヘッダ用
        Dim WW_IPPKUDISTANCE_T As Decimal = 0                       '一般・空車距離ヘッダ用
        Dim WW_KOSJIDISTANCE_T As Decimal = 0                       '高速・実車距離ヘッダ用
        Dim WW_KOSKUDISTANCE_T As Decimal = 0                       '高速・空車距離ヘッダ用
        Dim WW_PRATE As Integer = 0                                 '通行料・プレート
        Dim WW_CASH As Integer = 0                                  '通行料・現金
        Dim WW_TICKET As Integer = 0                                '通行料・回数券
        Dim WW_ETC As Integer = 0                                   '通行料・ETC
        Dim WW_TOTALTOLL As Integer = 0                             '通行料・合計
        Dim WW_KYUYU As Decimal = 0                                 '給油
        Dim WW_STYMD As String = ""                                 '始業日
        Dim WW_ENDYMD As String = ""                                '就業日

        Dim WW_CNT As Integer = 0

        Dim WW_STTIME As String = ""
        Dim WW_ENDTIME As String = ""
        Dim WW_WORKTIME As String = "0"
        Dim WW_STMATER As String = "0"
        Dim WW_ENDMATER As String = "0"
        Dim WW_YMD As String = ""
        Dim WW_NIPPONO As String = ""
        Dim WW_STAFFCODE As String = ""
        Dim WW_SUBSTAFFCODE As String = ""
        Dim WW_GSHABAN As String = ""
        Dim WW_TIME As String = ""
        Dim WW_FIRST As String = "OFF"

        S0013tbl = New DataTable

        O_RTN = C_MESSAGE_NO.NORMAL

        WW_ERRLIST = New List(Of String)
        WW_ERRLIST_ALL = New List(Of String)
        Try
            WW_TWOMAN_FLG = False
            WW_ERRLIST_ALL.Clear()
            WW_ERRLIST.Clear()

            Dim I_VALUE As String = String.Empty
            Dim O_VALUE As String = String.Empty
            Dim O_MESSAGE_NO As String = C_MESSAGE_NO.NORMAL
            Dim O_CHECKREPORT As String = String.Empty

            For i As Integer = I_STSEQ To I_ENDSEQ
                Dim KSYASAIrow As DataRow = KSYASAItbl.Rows(i)

                '明細行用編集用
                Dim T0005INProw As DataRow = T0005INPtbl.NewRow
                T0005COM.InitialT5INPRow(T0005INProw)
                T0005INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                T0005INProw("TERMKBN") = I_MODE
                T0005INProw("SHIPORG") = work.WF_SEL_UORG.Text

                '稼働日
                '①必須・項目属性チェック
                I_VALUE = Mid(KSYASAIrow("FIELD4"), 1, 4) & "/" & Mid(KSYASAIrow("FIELD4"), 5, 2) & "/" & Mid(KSYASAIrow("FIELD4"), 7, 2)
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "YMD", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    If Not String.IsNullOrEmpty(O_VALUE) Then
                        T0005INProw("YMD") = O_VALUE
                        WW_YMD = O_VALUE
                        '稼働日が条件の開始日と終了日の範囲外の場合エラー
                        If CDate(WW_YMD) < CDate(work.WF_SEL_STYMD.Text) OrElse
                           CDate(WW_YMD) > CDate(work.WF_SEL_ENDYMD.Text) Then
                            OutputErrorMessageByKouei(KSYASAIrow, "稼働日", "稼働日が範囲対象外です。", C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                        End If
                    Else
                        OutputErrorMessageByKouei(KSYASAIrow, "稼働日", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                    End If
                Else
                    OutputErrorMessageByKouei(KSYASAIrow, "稼働日", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                End If

                '日報番号
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NIPPONO", KSYASAIrow("FIELD10"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("NIPPONO") = O_VALUE
                    WW_NIPPONO = O_VALUE
                Else
                    OutputErrorMessageByKouei(KSYASAIrow, "日報№", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                End If

                '社員コード
                '①必須・項目属性チェック
                Select Case I_MODE
                    Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                        I_VALUE = Mid(KSYASAIrow("FIELD6"), 1, 5)
                    Case GRT00005WRKINC.TERM_TYPE.JOT
                        I_VALUE = KSYASAIrow("FIELD6")
                        'I_VALUE.VALUE = Mid(KSYASAIrow("FIELD6"), 1, 5)
                    Case GRT00005WRKINC.TERM_TYPE.COSMO
                        'TODO
                        I_VALUE = KSYASAIrow("FIELD6")
                End Select
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("STAFFCODE") = O_VALUE
                    WW_STAFFCODE = O_VALUE
                Else
                    OutputErrorMessageByKouei(KSYASAIrow, "社員コード", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                End If

                '作業区分
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKKBN", KSYASAIrow("FIELD13"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("WORKKBN") = O_VALUE
                Else
                    OutputErrorMessageByKouei(KSYASAIrow, "作業区分", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                End If

                T0005INProw("HDKBN") = "D"

                '車載ＳＥＱ
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SEQ", KSYASAIrow("FIELD8"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("SEQ") = O_VALUE
                Else
                    T0005INProw("SEQ") = KSYASAIrow("FIELD8")
                    OutputErrorMessageByKouei(KSYASAIrow, "車載ＳＥＱ", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                End If

                'オドメータ
                '①必須・項目属性チェック
                If IsDBNull(KSYASAIrow("FIELD51")) Then KSYASAIrow("FIELD51") = String.Empty
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RUIDISTANCE", KSYASAIrow("FIELD51"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("RUIDISTANCE") = O_VALUE
                Else
                    T0005INProw("RUIDISTANCE") = 0
                End If

                '緯度
                '①必須・項目属性チェック
                If IsDBNull(KSYASAIrow("FIELD52")) Then KSYASAIrow("FIELD52") = String.Empty
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "LATITUDE", KSYASAIrow("FIELD52"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("LATITUDE") = O_VALUE
                Else
                    T0005INProw("LATITUDE") = ""
                End If

                '経度
                '①必須・項目属性チェック
                If IsDBNull(KSYASAIrow("FIELD53")) Then KSYASAIrow("FIELD53") = String.Empty
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "LONGITUDE", KSYASAIrow("FIELD53"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("LONGITUDE") = O_VALUE
                Else
                    T0005INProw("LONGITUDE") = ""
                End If

                '２マン社員コード
                If KSYASAIrow("FIELD18") = "0" Then
                    T0005INProw("SUBSTAFFCODE") = ""
                Else
                    '①必須・項目属性チェック
                    Select Case I_MODE
                        Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                            I_VALUE = Mid(KSYASAIrow("FIELD18"), 1, KSYASAIrow("FIELD18").length)
                        Case GRT00005WRKINC.TERM_TYPE.JOT
                            I_VALUE = KSYASAIrow("FIELD18")
                            'CS0036FCHECK.VALUE = Mid(KSYASAIrow("FIELD18"), 1, 5)
                        Case GRT00005WRKINC.TERM_TYPE.COSMO
                            'TODO
                            I_VALUE = KSYASAIrow("FIELD18")
                    End Select
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                    If isNormal(O_MESSAGE_NO) Then
                        '乗務員＝副乗務員（自分が副乗務員の場合、副乗務員クリア）
                        If WW_STAFFCODE = O_VALUE Then
                            T0005INProw("SUBSTAFFCODE") = ""
                            WW_SUBSTAFFCODE = ""
                        Else
                            T0005INProw("SUBSTAFFCODE") = O_VALUE
                            WW_SUBSTAFFCODE = O_VALUE
                        End If
                    Else
                        T0005INProw("SUBSTAFFCODE") = KSYASAIrow("FIELD18")
                        OutputErrorMessageByKouei(KSYASAIrow, "２マン社員コード", O_CHECKREPORT, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, I_MODE)
                    End If
                End If

                T0005INProw("CREWKBN") = "1"

                '車両コード
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "GSHABAN", KSYASAIrow("FIELD21"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If isNormal(O_MESSAGE_NO) Then
                    T0005INProw("GSHABAN") = O_VALUE
                    WW_GSHABAN = O_VALUE
                Else
                    T0005INProw("GSHABAN") = KSYASAIrow("FIELD21")
                    OutputErrorMessageByKouei(KSYASAIrow, "車両コード", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                End If

                Dim WW_F3F8 As String = "OFF"
                Select Case KSYASAIrow("FIELD13")
                    Case "F1", "F7"
                        '作業区分  F1：始業  -> A1 始業
                        '          F7：終業
                        If KSYASAIrow("FIELD13") = "F1" Then
                            '作業区分（F1）：始業・終業
                            T0005INProw("CTRL") = "OFF"
                            '始業レコード作成
                            T0005INProw("WORKKBN") = "A1" '始業
                            '情報１（始業日）
                            '①必須・項目属性チェック
                            I_VALUE = Mid(KSYASAIrow("FIELD31"), 1, 4) & "/" & Mid(KSYASAIrow("FIELD31"), 5, 2) & "/" & Mid(KSYASAIrow("FIELD31"), 7, 2)
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STDATE", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                WW_STYMD = O_VALUE
                            Else
                                WW_STYMD = KSYASAIrow("FIELD31")
                                OutputErrorMessageByKouei(KSYASAIrow, "情報１（始業日）", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If

                            '情報２（終業日）
                            '①必須・項目属性チェック
                            I_VALUE = Mid(KSYASAIrow("FIELD32"), 1, 4) & "/" & Mid(KSYASAIrow("FIELD32"), 5, 2) & "/" & Mid(KSYASAIrow("FIELD32"), 7, 2)
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDDATE", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                WW_ENDYMD = O_VALUE
                            Else
                                WW_ENDYMD = KSYASAIrow("FIELD32")
                                OutputErrorMessageByKouei(KSYASAIrow, "情報２（終業日）", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If

                            T0005INProw("STDATE") = WW_STYMD
                            T0005INProw("ENDDATE") = WW_STYMD

                            '開始時刻
                            '①必須・項目属性チェック
                            WW_TIME = KSYASAIrow("FIELD27").PadLeft(4, "0")
                            I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                If Not String.IsNullOrEmpty(O_VALUE) Then
                                    T0005INProw("STTIME") = CDate(O_VALUE).ToString("HH:mm")
                                    T0005INProw("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
                                Else
                                    T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                                    T0005INProw("ENDTIME") = KSYASAIrow("FIELD27")
                                    OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                End If
                            Else
                                T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                                T0005INProw("ENDTIME") = KSYASAIrow("FIELD27")
                                OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If

                            '終了時刻
                            '①必須・項目属性チェック
                            WW_TIME = KSYASAIrow("FIELD28").PadLeft(4, "0")
                            I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                If Not String.IsNullOrEmpty(O_VALUE) Then
                                    WW_ENDTIME = CDate(O_VALUE).ToString("HH:mm")
                                Else
                                    OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                End If
                            Else
                                OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If

                            WW_WORKTIME = 0
                            '情報３（稼働時間）
                            '①必須・項目属性チェック
                            If InStr(KSYASAIrow("FIELD33"), "-") > 0 Then
                                '開始時刻＞終了時刻（＝稼働時間がマイナス）の場合、開始時刻＞終了時刻エラーを表現するため意図的に終了時間を設定
                                '本来、A1（始業レコード）は開始時刻＝終了時刻で作り出すが上記の通り設定することでエラーとする（これで詳細画面でもエラーが表現できる）
                                T0005INProw("ENDTIME") = WW_ENDTIME
                                OutputErrorMessageByKouei(KSYASAIrow, "情報３（稼働時間）", "マイナス値です。(" & KSYASAIrow("FIELD33") & ")", C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            Else
                                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKTIME", KSYASAIrow("FIELD33"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                                If isNormal(O_MESSAGE_NO) Then
                                    WW_WORKTIME = O_VALUE
                                Else
                                    OutputErrorMessageByKouei(KSYASAIrow, "情報３（稼働時間）", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                End If
                            End If

                            If WW_ERRLIST.Count > 0 Then
                                If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                                    WW_ERRLIST = New List(Of String)
                                    Continue For
                                Else
                                    'エラーフラグ
                                    T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                End If
                            Else
                                'エラーフラグ
                                T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            End If

                            T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE
                            T0005INPtbl.Rows.Add(T0005INProw)
                            '終業レコード作成
                            Dim T0005INProw2 As DataRow = T0005INPtbl.NewRow
                            T0005INProw2.ItemArray = T0005INProw.ItemArray
                            T0005INProw2("WORKKBN") = "Z1" '終業
                            T0005INProw2("STDATE") = WW_ENDYMD
                            T0005INProw2("ENDDATE") = WW_ENDYMD
                            T0005INProw2("STTIME") = WW_ENDTIME
                            T0005INProw2("ENDTIME") = WW_ENDTIME

                            T0005INPtbl.Rows.Add(T0005INProw2)
                            WW_ERRLIST = New List(Of String)

                            '■ヘッダーレコード編集
                            WW_STTIME = T0005INProw("STTIME")

                            WW_SOUDISTANCE_T += WW_SOUDISTANCE
                            WW_JIDISTANCE_T += WW_JIDISTANCE
                            WW_KUDISTANCE_T += WW_KUDISTANCE
                            WW_IPPDISTANCE_T += WW_IPPDISTANCE
                            WW_KOSDISTANCE_T += WW_KOSDISTANCE
                            WW_IPPJIDISTANCE_T += WW_IPPJIDISTANCE
                            WW_IPPKUDISTANCE_T += WW_IPPKUDISTANCE
                            WW_KOSJIDISTANCE_T += WW_KOSJIDISTANCE
                            WW_KOSKUDISTANCE_T += WW_KOSKUDISTANCE

                            WW_MOVETIME = 0
                            WW_SOUDISTANCE = 0
                            WW_JIDISTANCE = 0
                            WW_KUDISTANCE = 0
                            WW_IPPDISTANCE = 0
                            WW_KOSDISTANCE = 0
                            WW_IPPJIDISTANCE = 0
                            WW_IPPKUDISTANCE = 0
                            WW_KOSJIDISTANCE = 0
                            WW_KOSKUDISTANCE = 0
                        End If

                    Case "F3"
                        '作業区分（F3）：出庫・帰庫　-> F1 出庫 + F3 帰庫

                        T0005INProw("WORKKBN") = "F1" '出庫
                        T0005INProw("STDATE") = WW_STYMD
                        T0005INProw("ENDDATE") = WW_STYMD

                        '開始時刻
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD27").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If Not String.IsNullOrEmpty(O_VALUE) Then
                                T0005INProw("STTIME") = CDate(O_VALUE).ToString("HH:mm")
                                T0005INProw("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                                T0005INProw("ENDTIME") = KSYASAIrow("FIELD27")
                                OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                            T0005INProw("ENDTIME") = KSYASAIrow("FIELD27")
                            OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                        Dim WW_ENDTIME2 As String = ""
                        '終了時刻
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD28").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If Not String.IsNullOrEmpty(O_VALUE) Then
                                WW_ENDTIME2 = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                        If WW_ERRLIST.Count > 0 Then
                            If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                                WW_ERRLIST = New List(Of String)
                                Continue For
                            Else
                                'エラーフラグ
                                T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                            End If
                        Else
                            'エラーフラグ
                            T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        End If

                        T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE
                        T0005INPtbl.Rows.Add(T0005INProw)

                        '〇帰庫レコード作成
                        Dim T0005INProw2 As DataRow = T0005INPtbl.NewRow
                        T0005INProw2.ItemArray = T0005INProw.ItemArray
                        T0005INProw2("WORKKBN") = "F3" '帰庫
                        If T0005INProw("STTIME") < WW_ENDTIME2 Then
                            T0005INProw2("STDATE") = WW_STYMD
                            T0005INProw2("ENDDATE") = WW_STYMD
                        Else
                            T0005INProw2("STDATE") = WW_ENDYMD
                            T0005INProw2("ENDDATE") = WW_ENDYMD
                        End If
                        T0005INProw2("STTIME") = WW_ENDTIME2
                        T0005INProw2("ENDTIME") = WW_ENDTIME2

                        T0005INPtbl.Rows.Add(T0005INProw2)

                        WW_ERRLIST = New List(Of String)

                        WW_SOUDISTANCE_T += WW_SOUDISTANCE
                        WW_JIDISTANCE_T += WW_JIDISTANCE
                        WW_KUDISTANCE_T += WW_KUDISTANCE
                        WW_IPPDISTANCE_T += WW_IPPDISTANCE
                        WW_KOSDISTANCE_T += WW_KOSDISTANCE
                        WW_IPPJIDISTANCE_T += WW_IPPJIDISTANCE
                        WW_IPPKUDISTANCE_T += WW_IPPKUDISTANCE
                        WW_KOSJIDISTANCE_T += WW_KOSJIDISTANCE
                        WW_KOSKUDISTANCE_T += WW_KOSKUDISTANCE

                        WW_MOVETIME = 0
                        WW_SOUDISTANCE = 0
                        WW_JIDISTANCE = 0
                        WW_KUDISTANCE = 0
                        WW_IPPDISTANCE = 0
                        WW_KOSDISTANCE = 0
                        WW_IPPJIDISTANCE = 0
                        WW_IPPKUDISTANCE = 0
                        WW_KOSJIDISTANCE = 0
                        WW_KOSKUDISTANCE = 0

                    Case "B4", "B8", "B9", "BA", "BX", "B5", "BB", "BC"
                        '作業区分（B4）：点検
                        '作業区分（B8）：荷卸準備
                        '作業区分（B9）：他手待
                        '作業区分（BA）：待機
                        '作業区分（BX）：他作業
                        '作業区分（B5）：洗車
                        '作業区分（BB,BC）：休憩　-> BB 休憩
                        If KSYASAIrow("FIELD13") = "BC" Then T0005INProw("WORKKBN") = "BB"

                        '開始時刻
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD27").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If Not String.IsNullOrEmpty(O_VALUE) Then
                                T0005INProw("STTIME") = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                                OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                            OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                        '終了時刻
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD28").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If Not String.IsNullOrEmpty(O_VALUE) Then
                                T0005INProw("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                T0005INProw("ENDTIME") = KSYASAIrow("FIELD28")
                                OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            T0005INProw("ENDTIME") = KSYASAIrow("FIELD28")
                            OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                        If WW_CHGYMD_FLG Then
                            T0005INProw("STDATE") = WW_ENDYMD
                            T0005INProw("ENDDATE") = WW_ENDYMD
                        Else
                            T0005INProw("STDATE") = WW_STYMD
                            '日跨り対応
                            If T0005INProw("STTIME") > T0005INProw("ENDTIME") Then
                                T0005INProw("ENDDATE") = WW_ENDYMD
                            Else
                                T0005INProw("ENDDATE") = WW_STYMD
                            End If
                        End If

                        Dim WW_ACT As Integer = 0
                        '情報３（作業時間）
                        '①必須・項目属性チェック
                        If InStr(KSYASAIrow("FIELD33"), "-") > 0 Then
                            T0005INProw("WORKTIME") = KSYASAIrow("FIELD33")
                            OutputErrorMessageByKouei(KSYASAIrow, "作業時間", "マイナス値です。(" & KSYASAIrow("FIELD33") & ")", C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        Else
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKTIME", KSYASAIrow("FIELD33"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                T0005INProw("WORKTIME") = T0005COM.MinutestoHHMM(O_VALUE)
                                WW_ACT += O_VALUE
                            Else
                                T0005INProw("WORKTIME") = KSYASAIrow("FIELD33")
                                OutputErrorMessageByKouei(KSYASAIrow, "作業時間", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        End If

                        '"C1", "C2", "C3", "C4", "C5"で集計、集約した結果を設定
                        T0005INProw("MOVETIME") = T0005COM.MinutestoHHMM(WW_MOVETIME)
                        WW_ACT += WW_MOVETIME
                        T0005INProw("ACTTIME") = T0005COM.MinutestoHHMM(WW_ACT)
                        T0005INProw("SOUDISTANCE") = WW_SOUDISTANCE.ToString("#,0.00")
                        T0005INProw("JIDISTANCE") = WW_JIDISTANCE.ToString("#,0.00")
                        T0005INProw("KUDISTANCE") = WW_KUDISTANCE.ToString("#,0.00")
                        T0005INProw("IPPDISTANCE") = WW_IPPDISTANCE.ToString("#,0.00")
                        T0005INProw("KOSDISTANCE") = WW_KOSDISTANCE.ToString("#,0.00")
                        T0005INProw("IPPJIDISTANCE") = WW_IPPJIDISTANCE.ToString("#,0.00")
                        T0005INProw("IPPKUDISTANCE") = WW_IPPKUDISTANCE.ToString("#,0.00")
                        T0005INProw("KOSJIDISTANCE") = WW_KOSJIDISTANCE.ToString("#,0.00")
                        T0005INProw("KOSKUDISTANCE") = WW_KOSKUDISTANCE.ToString("#,0.00")
                        '明細レコード追加
                        WW_WRITE_FLG = True

                        WW_SOUDISTANCE_T += WW_SOUDISTANCE
                        WW_JIDISTANCE_T += WW_JIDISTANCE
                        WW_KUDISTANCE_T += WW_KUDISTANCE
                        WW_IPPDISTANCE_T += WW_IPPDISTANCE
                        WW_KOSDISTANCE_T += WW_KOSDISTANCE
                        WW_IPPJIDISTANCE_T += WW_IPPJIDISTANCE
                        WW_IPPKUDISTANCE_T += WW_IPPKUDISTANCE
                        WW_KOSJIDISTANCE_T += WW_KOSJIDISTANCE
                        WW_KOSKUDISTANCE_T += WW_KOSKUDISTANCE
                        WW_MOVETIME = 0
                        WW_SOUDISTANCE = 0
                        WW_JIDISTANCE = 0
                        WW_KUDISTANCE = 0
                        WW_IPPDISTANCE = 0
                        WW_KOSDISTANCE = 0
                        WW_IPPJIDISTANCE = 0
                        WW_IPPKUDISTANCE = 0
                        WW_KOSJIDISTANCE = 0
                        WW_KOSKUDISTANCE = 0

                    Case "B2"
                        '作業区分（B2）：積込作業
                        '開始時刻　27
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD27").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If Not String.IsNullOrEmpty(O_VALUE) Then
                                T0005INProw("STTIME") = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                                OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                            OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                        '終了時刻 28
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD28").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If Not String.IsNullOrEmpty(O_VALUE) Then
                                T0005INProw("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                T0005INProw("ENDTIME") = KSYASAIrow("FIELD28")
                                OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            T0005INProw("ENDTIME") = KSYASAIrow("FIELD28")
                            OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                        If WW_CHGYMD_FLG Then
                            T0005INProw("STDATE") = WW_ENDYMD
                            T0005INProw("ENDDATE") = WW_ENDYMD
                        Else
                            T0005INProw("STDATE") = WW_STYMD
                            '日跨り対応
                            If T0005INProw("STTIME") > T0005INProw("ENDTIME") Then
                                T0005INProw("ENDDATE") = WW_ENDYMD
                            Else
                                T0005INProw("ENDDATE") = WW_STYMD
                            End If
                        End If

                        Dim WW_ACT As Integer = 0
                        '情報３（作業時間）
                        '①必須・項目属性チェック
                        If InStr(KSYASAIrow("FIELD33"), "-") > 0 Then
                            T0005INProw("WORKTIME") = KSYASAIrow("FIELD33")
                            OutputErrorMessageByKouei(KSYASAIrow, "作業時間", "マイナス値です。(" & KSYASAIrow("FIELD33") & ")", C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        Else
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKTIME", KSYASAIrow("FIELD33"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                T0005INProw("WORKTIME") = T0005COM.MinutestoHHMM(O_VALUE)
                                WW_ACT += O_VALUE
                            Else
                                T0005INProw("WORKTIME") = KSYASAIrow("FIELD33")
                                OutputErrorMessageByKouei(KSYASAIrow, "作業時間", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        End If
                        'E2レコードを検索　
                        '  （稼働日  4,日報№ 10,トリップ 16）し、作業区分 13 が"E2"（荷卸数量）が存在した場合、取引先、出荷場所 35を取得、設定する
                        Using WW_SEL As DataTable = KSYASAItbl.Clone
                            For j As Integer = i + 1 To KSYASAItbl.Rows.Count - 1
                                Dim WW_KSYASAIrow As DataRow = KSYASAItbl.Rows(j)
                                If WW_KSYASAIrow("FIELD4") = KSYASAIrow("FIELD4") AndAlso
                                   WW_KSYASAIrow("FIELD10") = KSYASAIrow("FIELD10") Then

                                    If WW_KSYASAIrow("FIELD16") = KSYASAIrow("FIELD16") AndAlso
                                       WW_KSYASAIrow("FIELD13") = "E2" Then
                                        Dim WW_row As DataRow = WW_SEL.NewRow
                                        WW_row.ItemArray = WW_KSYASAIrow.ItemArray
                                        WW_SEL.Rows.Add(WW_row)
                                    End If
                                Else
                                    Exit For
                                End If
                            Next
                            If WW_SEL.Rows.Count > 0 Then
                                '取引先コードにブランク設定
                                T0005INProw("TORICODE") = String.Empty
                                '〇出荷地が設定されている場合
                                If Not String.IsNullOrEmpty(WW_SEL.Rows(0)("FIELD35")) Then
                                    '出荷地コード
                                    '①必須・項目属性チェック
                                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", WW_SEL.Rows(0)("FIELD35"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                                    If isNormal(O_MESSAGE_NO) Then
                                        T0005INProw("SHUKABASHO") = O_VALUE.PadLeft(4, "0")
                                    Else
                                        T0005INProw("SHUKABASHO") = WW_SEL.Rows(0)("FIELD35")
                                        OutputErrorMessageByKouei(WW_SEL.Rows(0), "出荷地コード", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                    End If
                                End If
                            End If
                        End Using
                        'トリップ
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TRIPNO", KSYASAIrow("FIELD16"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            T0005INProw("TRIPNO") = O_VALUE
                        Else
                            OutputErrorMessageByKouei(KSYASAIrow, "トリップ", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            T0005INProw("TRIPNO") = "000"
                        End If


                        '"C1", "C2", "C3", "C4", "C5"で集計、集約した結果を設定
                        T0005INProw("MOVETIME") = T0005COM.MinutestoHHMM(WW_MOVETIME)
                        WW_ACT += WW_MOVETIME
                        T0005INProw("ACTTIME") = T0005COM.MinutestoHHMM(WW_ACT)
                        T0005INProw("SOUDISTANCE") = WW_SOUDISTANCE.ToString("#,0.00")
                        T0005INProw("JIDISTANCE") = WW_JIDISTANCE.ToString("#,0.00")
                        T0005INProw("KUDISTANCE") = WW_KUDISTANCE.ToString("#,0.00")
                        T0005INProw("IPPDISTANCE") = WW_IPPDISTANCE.ToString("#,0.00")
                        T0005INProw("KOSDISTANCE") = WW_KOSDISTANCE.ToString("#,0.00")
                        T0005INProw("IPPJIDISTANCE") = WW_IPPJIDISTANCE.ToString("#,0.00")
                        T0005INProw("IPPKUDISTANCE") = WW_IPPKUDISTANCE.ToString("#,0.00")
                        T0005INProw("KOSJIDISTANCE") = WW_KOSJIDISTANCE.ToString("#,0.00")
                        T0005INProw("KOSKUDISTANCE") = WW_KOSKUDISTANCE.ToString("#,0.00")
                        '明細レコード追加
                        WW_WRITE_FLG = True

                        WW_SOUDISTANCE_T += WW_SOUDISTANCE
                        WW_JIDISTANCE_T += WW_JIDISTANCE
                        WW_KUDISTANCE_T += WW_KUDISTANCE
                        WW_IPPDISTANCE_T += WW_IPPDISTANCE
                        WW_KOSDISTANCE_T += WW_KOSDISTANCE
                        WW_IPPJIDISTANCE_T += WW_IPPJIDISTANCE
                        WW_IPPKUDISTANCE_T += WW_IPPKUDISTANCE
                        WW_KOSJIDISTANCE_T += WW_KOSJIDISTANCE
                        WW_KOSKUDISTANCE_T += WW_KOSKUDISTANCE
                        WW_MOVETIME = 0
                        WW_SOUDISTANCE = 0
                        WW_JIDISTANCE = 0
                        WW_KUDISTANCE = 0
                        WW_IPPDISTANCE = 0
                        WW_KOSDISTANCE = 0
                        WW_IPPJIDISTANCE = 0
                        WW_IPPKUDISTANCE = 0
                        WW_KOSJIDISTANCE = 0
                        WW_KOSKUDISTANCE = 0
                    Case "B3"
                        '作業区分（B3）：荷卸作業
                        '開始時刻
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD27").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If Not String.IsNullOrEmpty(O_VALUE) Then
                                T0005INProw("STTIME") = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                                OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                            OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                        '終了時刻
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD28").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If Not String.IsNullOrEmpty(O_VALUE) Then
                                T0005INProw("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                T0005INProw("ENDTIME") = KSYASAIrow("FIELD28")
                                OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        Else
                            T0005INProw("ENDTIME") = KSYASAIrow("FIELD28")
                            OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                        If WW_CHGYMD_FLG Then
                            T0005INProw("STDATE") = WW_ENDYMD
                            T0005INProw("ENDDATE") = WW_ENDYMD
                        Else
                            T0005INProw("STDATE") = WW_STYMD
                            '日跨り対応
                            If T0005INProw("STTIME") > T0005INProw("ENDTIME") Then
                                T0005INProw("ENDDATE") = WW_ENDYMD
                            Else
                                T0005INProw("ENDDATE") = WW_STYMD
                            End If
                        End If

                        Dim WW_ACT As Integer = 0
                        '情報３（作業時間）
                        '①必須・項目属性チェック
                        If InStr(KSYASAIrow("FIELD33"), "-") > 0 Then
                            T0005INProw("WORKTIME") = KSYASAIrow("FIELD33")
                            OutputErrorMessageByKouei(KSYASAIrow, "作業時間", "マイナス値です。(" & KSYASAIrow("FIELD33") & ")", C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        Else
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKTIME", KSYASAIrow("FIELD33"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                T0005INProw("WORKTIME") = T0005COM.MinutestoHHMM(O_VALUE)
                                WW_ACT += O_VALUE
                            Else
                                T0005INProw("WORKTIME") = KSYASAIrow("FIELD33")
                                OutputErrorMessageByKouei(KSYASAIrow, "作業時間", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                        End If

                        'トリップ
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TRIPNO", KSYASAIrow("FIELD16"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            T0005INProw("TRIPNO") = O_VALUE
                        Else
                            OutputErrorMessageByKouei(KSYASAIrow, "トリップ", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            T0005INProw("TRIPNO") = "000"
                        End If

                        '荷卸作業に該当するE2（配送数量）レコード（複数あり）を検索する。稼働日、日報№、終了時刻が同一のもの
                        Dim WW_SEL As DataTable = KSYASAItbl.Clone
                        For j As Integer = i + 1 To KSYASAItbl.Rows.Count - 1
                            Dim WW_KSYASAIrow As DataRow = KSYASAItbl.Rows(j)
                            If WW_KSYASAIrow("FIELD4") = KSYASAIrow("FIELD4") AndAlso
                                WW_KSYASAIrow("FIELD10") = KSYASAIrow("FIELD10") AndAlso
                                Mid(WW_KSYASAIrow("FIELD13"), 1, 1) = "E" Then
                                If WW_KSYASAIrow("FIELD13") = "E2" Then
                                    Dim WW_row As DataRow = WW_SEL.NewRow
                                    WW_row.ItemArray = WW_KSYASAIrow.ItemArray
                                    WW_SEL.Rows.Add(WW_row)
                                End If
                            Else
                                '日跨り
                                If WW_KSYASAIrow("FIELD13") = "X1" Then
                                    WW_CHGYMD_FLG = True
                                    Continue For
                                End If
                                '荷卸作業中に日跨りが発生した場合、B3が分割される（B3（～0;00）→X1→B3（0:00～））
                                If WW_KSYASAIrow("FIELD13") = "B3" Then
                                    T0005INProw("ENDDATE") = WW_ENDYMD
                                    WW_TIME = WW_KSYASAIrow("FIELD28").PadLeft(4, "0")
                                    I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                                    If isNormal(O_MESSAGE_NO) Then
                                        If Not String.IsNullOrEmpty(O_VALUE) Then
                                            T0005INProw("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
                                        Else
                                            T0005INProw("ENDTIME") = WW_KSYASAIrow("FIELD28")
                                            OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                        End If
                                    Else
                                        T0005INProw("ENDTIME") = WW_KSYASAIrow("FIELD28")
                                        OutputErrorMessageByKouei(KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                    End If
                                    i = j
                                    Continue For
                                End If

                                Exit For
                            End If
                        Next

                        If WW_SEL.Rows.Count > 0 Then
                            'E2からトリップ取得
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TRIPNO", WW_SEL.Rows(0)("FIELD16"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then T0005INProw("TRIPNO") = O_VALUE

                            If WW_SEL.Rows(0)("FIELD34") <> "" Then
                                '荷主コード 34
                                '①必須・項目属性チェック
                                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TORICODE", WW_SEL.Rows(0)("FIELD34"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                                If isNormal(O_MESSAGE_NO) Then
                                    T0005INProw("TORICODE") = O_VALUE
                                Else
                                    T0005INProw("TORICODE") = WW_SEL.Rows(0)("FIELD34")
                                    OutputErrorMessageByKouei(WW_SEL.Rows(0), "荷主コード", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                End If
                            End If
                            If WW_SEL.Rows(0)("FIELD35") <> "" Then
                                '出荷地コード 35
                                '①必須・項目属性チェック
                                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", WW_SEL.Rows(0)("FIELD35"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                                If isNormal(O_MESSAGE_NO) Then
                                    T0005INProw("SHUKABASHO") = O_VALUE.PadLeft(4, "0")
                                Else
                                    T0005INProw("SHUKABASHO") = WW_SEL.Rows(0)("FIELD35")
                                    OutputErrorMessageByKouei(WW_SEL.Rows(0), "出荷地コード", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                End If
                            End If
                            If WW_SEL.Rows(0)("FIELD36") <> "" Then
                                '届先コード 36,届先枝番 37
                                '①必須・項目属性チェック
                                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", WW_SEL.Rows(0)("FIELD36"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                                If isNormal(O_MESSAGE_NO) Then
                                    Select Case I_MODE
                                        Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                                            If I_LEGACY_MODE Then
                                                T0005INProw("TODOKECODE") = O_VALUE.PadLeft(6, "0") & WW_SEL.Rows(0)("FIELD37").ToString().PadLeft(3, "0")
                                            Else
                                                T0005INProw("TODOKECODE") = O_VALUE.PadLeft(9, "0")
                                            End If
                                        Case GRT00005WRKINC.TERM_TYPE.JOT
                                            T0005INProw("TODOKECODE") = O_VALUE
                                        Case GRT00005WRKINC.TERM_TYPE.COSMO
                                            T0005INProw("TODOKECODE") = O_VALUE.PadLeft(11, "0")
                                    End Select

                                Else
                                    T0005INProw("TODOKECODE") = WW_SEL.Rows(0)("FIELD36")
                                    OutputErrorMessageByKouei(WW_SEL.Rows(0), "届先コード", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                End If
                            End If
                        End If
                        '品名毎のE2レコードを１レコードに集約し、B3レコードを作成する
                        For j As Integer = 0 To WW_SEL.Rows.Count - 1
                            Dim WW_SELrow As DataRow = WW_SEL.Rows(j)

                            '品名コード 38
                            '①必須・項目属性チェック
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", WW_SELrow("FIELD38"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                WW_SELrow("FIELD38") = O_VALUE
                            Else
                                OutputErrorMessageByKouei(WW_SELrow, "品名コード", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If

                            '数量
                            '①必須・項目属性チェック
                            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", WW_SELrow("FIELD30"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                            If isNormal(O_MESSAGE_NO) Then
                                WW_SELrow("FIELD30") = Val(O_VALUE).ToString("#,0.000")
                            Else
                                OutputErrorMessageByKouei(WW_SELrow, "数量", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            End If
                            Select Case j
                                Case 0
                                    T0005INProw("PRODUCTCODE1") = WW_SELrow("FIELD38")
                                    T0005INProw("SURYO1") = WW_SELrow("FIELD30")
                                Case 1
                                    T0005INProw("PRODUCTCODE2") = WW_SELrow("FIELD38")
                                    T0005INProw("SURYO2") = WW_SELrow("FIELD30")
                                Case 2
                                    T0005INProw("PRODUCTCODE3") = WW_SELrow("FIELD38")
                                    T0005INProw("SURYO3") = WW_SELrow("FIELD30")
                                Case 3
                                    T0005INProw("PRODUCTCODE4") = WW_SELrow("FIELD38")
                                    T0005INProw("SURYO4") = WW_SELrow("FIELD30")
                                Case 4
                                    T0005INProw("PRODUCTCODE5") = WW_SELrow("FIELD38")
                                    T0005INProw("SURYO5") = WW_SELrow("FIELD30")
                                Case 5
                                    T0005INProw("PRODUCTCODE6") = WW_SELrow("FIELD38")
                                    T0005INProw("SURYO6") = WW_SELrow("FIELD30")
                                Case 6
                                    T0005INProw("PRODUCTCODE7") = WW_SELrow("FIELD38")
                                    T0005INProw("SURYO7") = WW_SELrow("FIELD30")
                                Case 7
                                    T0005INProw("PRODUCTCODE8") = WW_SELrow("FIELD38")
                                    T0005INProw("SURYO8") = WW_SELrow("FIELD30")
                            End Select
                        Next

                        Dim WW_SURYO As Decimal = 0
                        WW_SURYO = Val(T0005INProw("SURYO1")) +
                                   Val(T0005INProw("SURYO2")) +
                                   Val(T0005INProw("SURYO3")) +
                                   Val(T0005INProw("SURYO4")) +
                                   Val(T0005INProw("SURYO5")) +
                                   Val(T0005INProw("SURYO6")) +
                                   Val(T0005INProw("SURYO7")) +
                                   Val(T0005INProw("SURYO8"))
                        T0005INProw("TOTALSURYO") = WW_SURYO.ToString("#,0.000")
                        '"C1", "C2", "C3", "C4", "C5"で集計、集約した結果を設定
                        T0005INProw("MOVETIME") = T0005COM.MinutestoHHMM(WW_MOVETIME)
                        WW_ACT += WW_MOVETIME
                        T0005INProw("ACTTIME") = T0005COM.MinutestoHHMM(WW_ACT)
                        T0005INProw("SOUDISTANCE") = WW_SOUDISTANCE.ToString("#,0.00")
                        T0005INProw("JIDISTANCE") = WW_JIDISTANCE.ToString("#,0.00")
                        T0005INProw("KUDISTANCE") = WW_KUDISTANCE.ToString("#,0.00")
                        T0005INProw("IPPDISTANCE") = WW_IPPDISTANCE.ToString("#,0.00")
                        T0005INProw("KOSDISTANCE") = WW_KOSDISTANCE.ToString("#,0.00")
                        T0005INProw("IPPJIDISTANCE") = WW_IPPJIDISTANCE.ToString("#,0.00")
                        T0005INProw("IPPKUDISTANCE") = WW_IPPKUDISTANCE.ToString("#,0.00")
                        T0005INProw("KOSJIDISTANCE") = WW_KOSJIDISTANCE.ToString("#,0.00")
                        T0005INProw("KOSKUDISTANCE") = WW_KOSKUDISTANCE.ToString("#,0.00")
                        '明細レコード追加
                        WW_WRITE_FLG = True

                        WW_SOUDISTANCE_T += WW_SOUDISTANCE
                        WW_JIDISTANCE_T += WW_JIDISTANCE
                        WW_KUDISTANCE_T += WW_KUDISTANCE
                        WW_IPPDISTANCE_T += WW_IPPDISTANCE
                        WW_KOSDISTANCE_T += WW_KOSDISTANCE
                        WW_IPPJIDISTANCE_T += WW_IPPJIDISTANCE
                        WW_IPPKUDISTANCE_T += WW_IPPKUDISTANCE
                        WW_KOSJIDISTANCE_T += WW_KOSJIDISTANCE
                        WW_KOSKUDISTANCE_T += WW_KOSKUDISTANCE
                        WW_MOVETIME = 0
                        WW_SOUDISTANCE = 0
                        WW_JIDISTANCE = 0
                        WW_KUDISTANCE = 0
                        WW_IPPDISTANCE = 0
                        WW_KOSDISTANCE = 0
                        WW_IPPJIDISTANCE = 0
                        WW_IPPKUDISTANCE = 0
                        WW_KOSJIDISTANCE = 0
                        WW_KOSKUDISTANCE = 0

                        WW_SEL.Dispose()
                        WW_SEL = Nothing
                    Case "C1", "C2", "C3", "C4", "C5"
                        '作業区分（C1）: 実車・運転
                        '作業区分（C2）: 実車・積地移動
                        '作業区分（C3）: 実車・卸地移動
                        '作業区分（C4）: 空車・通常
                        '作業区分（C5）: 空車・出庫～積地
                        '上記区分は、次の作業区分のレコードに距離、走行時間を格納する。複数行存在するため集計、集約を行う
                        '　走行時間 ＝ 終了日時－開始日時
                        '　　　　　　　※開始日時は、先頭レコード
                        '　　　　　　　　終了日時は、最終レコード（１レコードの場合は、開始日時と同じレコードの終了日時）
                        '　各距離は、先頭～最終レコードの合計

                        '現在の作業区分をキープ
                        Dim WW_WORKKBN As String = KSYASAIrow("FIELD13")

                        '開始日時の決定
                        If WW_CHGYMD_FLG Then
                            T0005INProw("STDATE") = WW_ENDYMD
                        Else
                            T0005INProw("STDATE") = WW_STYMD
                        End If

                        Dim WW_NGCNT As Integer = 0
                        '開始時刻
                        '①必須・項目属性チェック
                        WW_TIME = KSYASAIrow("FIELD27").PadLeft(4, "0")
                        I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            If Not String.IsNullOrEmpty(O_VALUE) Then
                                T0005INProw("STTIME") = CDate(O_VALUE).ToString("HH:mm")
                            Else
                                WW_NGCNT += 1
                                T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                                OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                'エラーレポート編集
                            End If
                        Else
                            WW_NGCNT += 1
                            T0005INProw("STTIME") = KSYASAIrow("FIELD27")
                            OutputErrorMessageByKouei(KSYASAIrow, "開始時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                            'エラーレポート編集
                        End If

                        For j As Integer = i To KSYASAItbl.Rows.Count - 1
                            Dim WW_KSYASAIrow As DataRow = KSYASAItbl.Rows(j)
                            If WW_KSYASAIrow("FIELD13") = WW_WORKKBN Then
                                '終了時刻
                                '①必須・項目属性チェック
                                WW_TIME = WW_KSYASAIrow("FIELD28").PadLeft(4, "0")
                                I_VALUE = Mid(WW_TIME, 1, 2) & ":" & Mid(WW_TIME, 3, 2)
                                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                                If isNormal(O_MESSAGE_NO) Then
                                    If Not String.IsNullOrEmpty(O_VALUE) Then
                                        T0005INProw("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
                                    Else
                                        WW_NGCNT += 1
                                        T0005INProw("ENDTIME") = WW_KSYASAIrow("FIELD28")
                                        OutputErrorMessageByKouei(WW_KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                    End If
                                Else
                                    WW_NGCNT += 1
                                    T0005INProw("ENDTIME") = WW_KSYASAIrow("FIELD28")
                                    OutputErrorMessageByKouei(WW_KSYASAIrow, "終了時刻", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                End If

                                '終了日時の決定
                                If WW_CHGYMD_FLG Then
                                    T0005INProw("ENDDATE") = WW_ENDYMD
                                Else
                                    '日跨り対応
                                    If T0005INProw("STTIME") > T0005INProw("ENDTIME") Then
                                        T0005INProw("ENDDATE") = WW_ENDYMD
                                    Else
                                        T0005INProw("ENDDATE") = WW_STYMD
                                    End If
                                End If

                                Dim WW_DISTANCE As Integer = 0

                                '距離の集計
                                Select Case WW_KSYASAIrow("FIELD13")
                                    Case "C1", "C2", "C3"
                                        '情報１（距離）
                                        '①必須・項目属性チェック
                                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SOUDISTANCE", WW_KSYASAIrow("FIELD31"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                                        If isNormal(O_MESSAGE_NO) Then
                                            WW_DISTANCE = O_VALUE
                                        Else
                                            WW_DISTANCE = 0
                                            OutputErrorMessageByKouei(WW_KSYASAIrow, "情報１（距離）", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                        End If

                                        If WW_KSYASAIrow("FIELD42") = "1" Then
                                            WW_KOSJIDISTANCE = WW_KOSJIDISTANCE + WW_DISTANCE
                                        Else
                                            WW_IPPJIDISTANCE = WW_IPPJIDISTANCE + WW_DISTANCE
                                        End If
                                        WW_JIDISTANCE = WW_JIDISTANCE + WW_DISTANCE

                                    Case "C4", "C5"
                                        '情報２（距離）
                                        '①必須・項目属性チェック
                                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SOUDISTANCE", WW_KSYASAIrow("FIELD32"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                                        If isNormal(O_MESSAGE_NO) Then
                                            WW_DISTANCE = O_VALUE
                                        Else
                                            WW_DISTANCE = 0
                                            OutputErrorMessageByKouei(WW_KSYASAIrow, "情報２（距離）", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                                        End If

                                        If WW_KSYASAIrow("FIELD42") = "1" Then
                                            WW_KOSKUDISTANCE = WW_KOSKUDISTANCE + WW_DISTANCE
                                        Else
                                            WW_IPPKUDISTANCE = WW_IPPKUDISTANCE + WW_DISTANCE
                                        End If
                                        WW_KUDISTANCE = WW_KUDISTANCE + WW_DISTANCE
                                End Select
                                WW_SOUDISTANCE = WW_SOUDISTANCE + WW_DISTANCE

                                If WW_KSYASAIrow("FIELD42") = "1" Then     '（高速・実車、空車）高速区分='1'
                                    WW_KOSDISTANCE = WW_KOSDISTANCE + WW_DISTANCE

                                ElseIf WW_KSYASAIrow("FIELD42") = "0" Then '（一般・実車、空車）高速区分='0'
                                    WW_IPPDISTANCE = WW_IPPDISTANCE + WW_DISTANCE
                                End If
                            Else
                                'ループインデックスを進める（C1）を処理済のため読み飛ばす
                                i = j - 1
                                Exit For
                            End If
                            If KSYASAItbl.Rows.Count - 1 <= j Then i = KSYASAItbl.Rows.Count - 1
                        Next
                        If WW_NGCNT = 0 Then
                            Dim WW_STDATE As Date = CDate(T0005INProw("STDATE") & " " & T0005INProw("STTIME"))
                            Dim WW_ENDDATE As Date = CDate(T0005INProw("ENDDATE") & " " & T0005INProw("ENDTIME"))
                            WW_MOVETIME = DateDiff("n", WW_STDATE, WW_ENDDATE)
                        Else
                            WW_MOVETIME = 0
                        End If
                        '---------------------------------------
                        'ヘッダーレコード編集用　（開始）
                        '---------------------------------------
                    Case "E3"
                        '作業区分（E3）：通行料・プレート
                        '情報１（通行料・プレート）
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOTALTOLL", KSYASAIrow("FIELD31"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            WW_PRATE = WW_PRATE + O_VALUE
                            WW_TOTALTOLL = WW_TOTALTOLL + O_VALUE
                        Else
                            OutputErrorMessageByKouei(KSYASAIrow, "通通行料・プレート", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If
                    Case "E4"
                        '作業区分（E4）：通行料・現金
                        '情報１（通行料・現金）
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOTALTOLL", KSYASAIrow("FIELD31"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            WW_CASH = WW_CASH + O_VALUE
                            WW_TOTALTOLL = WW_TOTALTOLL + O_VALUE
                        Else
                            OutputErrorMessageByKouei(KSYASAIrow, "通行料・現金", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If
                    Case "E5"
                        '作業区分（E5）：通行料・回数券
                        '情報１（通行料・回数券）
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOTALTOLL", KSYASAIrow("FIELD31"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            WW_TICKET = WW_TICKET + O_VALUE
                            WW_TOTALTOLL = WW_TOTALTOLL + O_VALUE
                        Else
                            OutputErrorMessageByKouei(KSYASAIrow, "通行料・現金", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If
                    Case "E8"
                        '作業区分（E8）：通行料・ETC
                        '情報１（通行料・ETC）
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOTALTOLL", KSYASAIrow("FIELD31"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            WW_ETC = WW_ETC + O_VALUE
                            WW_TOTALTOLL = WW_TOTALTOLL + O_VALUE
                        Else
                            OutputErrorMessageByKouei(KSYASAIrow, "通行料・ETC", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                    Case "A3"
                        '作業区分（A3）：走行メータ
                        '情報１（走行メータ）
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STMATER", KSYASAIrow("FIELD31"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            WW_STMATER = O_VALUE
                        Else
                            WW_STMATER = KSYASAIrow("FIELD31")
                            OutputErrorMessageByKouei(KSYASAIrow, "情報１（走行メータ）", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                        '情報２（走行メータ）
                        '①必須・項目属性チェック
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDMATER", KSYASAIrow("FIELD32"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            WW_ENDMATER = O_VALUE
                        Else
                            WW_ENDMATER = KSYASAIrow("FIELD32")
                            OutputErrorMessageByKouei(KSYASAIrow, "情報２（走行メータ）", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If

                    Case "E6"
                        '作業区分（E6）：軽油
                        '軽油
                        '①必須・項目属性チェック
                        Select Case I_MODE
                            Case GRT00005WRKINC.TERM_TYPE.JOT
                                If InStr(KSYASAIrow("FIELD30"), ".") = 0 Then
                                    I_VALUE = (Val(KSYASAIrow("FIELD30")) / 10).ToString("#.00")
                                Else
                                    I_VALUE = Mid(KSYASAIrow("FIELD30"), 1, KSYASAIrow("FIELD30").length - 1)
                                End If
                            Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                                If I_LEGACY_MODE = True Then
                                    '旧（１０で割らない）
                                    If InStr(KSYASAIrow("FIELD30"), ".") = 0 Then
                                        I_VALUE = Mid(KSYASAIrow("FIELD30"), 1, KSYASAIrow("FIELD30").length)
                                    Else
                                        I_VALUE = Mid(KSYASAIrow("FIELD30"), 1, KSYASAIrow("FIELD30").length - 1)
                                    End If
                                Else
                                    '新（１０で割る）
                                    If InStr(KSYASAIrow("FIELD30"), ".") = 0 Then
                                        I_VALUE = (Val(KSYASAIrow("FIELD30")) / 10).ToString("#.00")
                                    Else
                                        I_VALUE = Mid(KSYASAIrow("FIELD30"), 1, KSYASAIrow("FIELD30").length - 1)
                                    End If
                                End If
                            Case GRT00005WRKINC.TERM_TYPE.COSMO
                                'TODO
                                If InStr(KSYASAIrow("FIELD30"), ".") = 0 Then
                                    I_VALUE = (Val(KSYASAIrow("FIELD30")) / 10).ToString("#.00")
                                Else
                                    I_VALUE = Mid(KSYASAIrow("FIELD30"), 1, KSYASAIrow("FIELD30").length - 1)
                                End If
                        End Select
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KYUYU", I_VALUE, O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                        If isNormal(O_MESSAGE_NO) Then
                            WW_KYUYU = WW_KYUYU + O_VALUE
                        Else
                            OutputErrorMessageByKouei(KSYASAIrow, "軽油", O_CHECKREPORT, C_MESSAGE_NO.BOX_ERROR_EXIST, I_MODE)
                        End If
                        '---------------------------------------
                        'ヘッダーレコード編集用　（終了）
                        '---------------------------------------

                    Case "X1"
                        '日付変更（日付跨り）
                        WW_CHGYMD_FLG = True
                End Select

                '明細データ出力判定＆出力
                If WW_WRITE_FLG Then
                    If WW_ERRLIST.Count > 0 Then
                        If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                            WW_ERRLIST = New List(Of String)
                            Continue For
                        Else
                            'エラーフラグ
                            T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                        End If
                    Else
                        'エラーフラグ
                        T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    End If

                    T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE
                    T0005INPtbl.Rows.Add(T0005INProw)
                    WW_WRITE_FLG = False
                    WW_ERRLIST = New List(Of String)

                End If
            Next

            Dim WW_LAST As Boolean = False
            If WW_MOVETIME = 0 AndAlso
                WW_SOUDISTANCE = 0 AndAlso
                WW_JIDISTANCE = 0 AndAlso
                WW_KUDISTANCE = 0 AndAlso
                WW_IPPDISTANCE = 0 AndAlso
                WW_KOSDISTANCE = 0 AndAlso
                WW_IPPJIDISTANCE = 0 AndAlso
                WW_IPPKUDISTANCE = 0 AndAlso
                WW_KOSJIDISTANCE = 0 AndAlso
                WW_KOSKUDISTANCE = 0 Then
            Else
                WW_SOUDISTANCE_T += WW_SOUDISTANCE
                WW_JIDISTANCE_T += WW_JIDISTANCE
                WW_KUDISTANCE_T += WW_KUDISTANCE
                WW_IPPDISTANCE_T += WW_IPPDISTANCE
                WW_KOSDISTANCE_T += WW_KOSDISTANCE
                WW_IPPJIDISTANCE_T += WW_IPPJIDISTANCE
                WW_IPPKUDISTANCE_T += WW_IPPKUDISTANCE
                WW_KOSJIDISTANCE_T += WW_KOSJIDISTANCE
                WW_KOSKUDISTANCE_T += WW_KOSKUDISTANCE
                WW_LAST = True
            End If
            'ヘッダーレコード出力
            Dim T0005HINProw As DataRow = T0005INPtbl.NewRow
            T0005COM.InitialT5INPRow(T0005HINProw)
            T0005HINProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            T0005HINProw("TERMKBN") = I_MODE
            T0005HINProw("YMD") = WW_YMD
            T0005HINProw("SHIPORG") = work.WF_SEL_UORG.Text
            T0005HINProw("NIPPONO") = WW_NIPPONO
            T0005HINProw("HDKBN") = "H"
            T0005HINProw("WORKKBN") = ""
            T0005HINProw("SEQ") = "001"
            T0005HINProw("CREWKBN") = "1"
            T0005HINProw("STAFFCODE") = WW_STAFFCODE
            T0005HINProw("SUBSTAFFCODE") = WW_SUBSTAFFCODE
            T0005HINProw("STDATE") = WW_STYMD
            T0005HINProw("ENDDATE") = WW_ENDYMD
            T0005HINProw("STTIME") = WW_STTIME
            T0005HINProw("ENDTIME") = WW_ENDTIME
            T0005HINProw("WORKTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
            T0005HINProw("ACTTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
            T0005HINProw("GSHABAN") = WW_GSHABAN
            T0005HINProw("STMATER") = Val(WW_STMATER).ToString("#,0.00")
            T0005HINProw("ENDMATER") = Val(WW_ENDMATER).ToString("#,0.00")
            T0005HINProw("PRATE") = WW_PRATE.ToString("#,0")
            T0005HINProw("CASH") = WW_CASH.ToString("#,0")
            T0005HINProw("TICKET") = WW_TICKET.ToString("#,0")
            T0005HINProw("ETC") = WW_ETC.ToString("#,0")
            T0005HINProw("TOTALTOLL") = WW_TOTALTOLL.ToString("#,0")
            T0005HINProw("SOUDISTANCE") = WW_SOUDISTANCE_T.ToString("#,0.00")
            T0005HINProw("JIDISTANCE") = WW_JIDISTANCE_T.ToString("#,0.00")
            T0005HINProw("KUDISTANCE") = WW_KUDISTANCE_T.ToString("#,0.00")
            T0005HINProw("IPPDISTANCE") = WW_IPPDISTANCE_T.ToString("#,0.00")
            T0005HINProw("KOSDISTANCE") = WW_KOSDISTANCE_T.ToString("#,0.00")
            T0005HINProw("IPPJIDISTANCE") = WW_IPPJIDISTANCE_T.ToString("#,0.00")
            T0005HINProw("IPPKUDISTANCE") = WW_IPPKUDISTANCE_T.ToString("#,0.00")
            T0005HINProw("KOSJIDISTANCE") = WW_KOSJIDISTANCE_T.ToString("#,0.00")
            T0005HINProw("KOSKUDISTANCE") = WW_KOSKUDISTANCE_T.ToString("#,0.00")
            T0005HINProw("KYUYU") = WW_KYUYU.ToString("#,0.00")
            T0005HINProw("DELFLG") = C_DELETE_FLG.ALIVE
            '〇行数分繰り返して設定作業を行う
            For Each WW_row As DataRow In T0005INPtbl.Rows
                If WW_row("WORKKBN") = "F3" AndAlso
                   WW_row("YMD") = T0005HINProw("YMD") AndAlso
                   WW_row("NIPPONO") = T0005HINProw("NIPPONO") Then

                    WW_row("KYUYU") = WW_KYUYU.ToString("#,0.00")
                    WW_row("PRATE") = WW_PRATE.ToString("#,0")
                    WW_row("CASH") = WW_CASH.ToString("#,0")
                    WW_row("TICKET") = WW_TICKET.ToString("#,0")
                    WW_row("ETC") = WW_ETC.ToString("#,0")
                    WW_row("TOTALTOLL") = WW_TOTALTOLL.ToString("#,0")
                    If WW_LAST Then
                        WW_row("SOUDISTANCE") = WW_SOUDISTANCE_T.ToString("#,0.00")
                        WW_row("JIDISTANCE") = WW_JIDISTANCE_T.ToString("#,0.00")
                        WW_row("KUDISTANCE") = WW_KUDISTANCE_T.ToString("#,0.00")
                        WW_row("IPPDISTANCE") = WW_IPPDISTANCE_T.ToString("#,0.00")
                        WW_row("KOSDISTANCE") = WW_KOSDISTANCE_T.ToString("#,0.00")
                        WW_row("IPPJIDISTANCE") = WW_IPPJIDISTANCE_T.ToString("#,0.00")
                        WW_row("IPPKUDISTANCE") = WW_IPPKUDISTANCE_T.ToString("#,0.00")
                        WW_row("KOSJIDISTANCE") = WW_KOSJIDISTANCE_T.ToString("#,0.00")
                        WW_row("KOSKUDISTANCE") = WW_KOSKUDISTANCE_T.ToString("#,0.00")
                    End If
                    Exit For
                End If
            Next

            If WW_ERRLIST_ALL.Count > 0 Then
                'エラーフラグ
                T0005HINProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                T0005INPtbl.Rows.Add(T0005HINProw)
            Else
                'エラーフラグ
                T0005HINProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                T0005INPtbl.Rows.Add(T0005HINProw)
            End If

            WW_ERRLIST = New List(Of String)
            WW_ERRLIST_ALL = New List(Of String)

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
            CS0011LOGWRITE.INFSUBCLASS = "T0005tbl_Detail_Kouei"           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    '''  T0005tbl編集（JX光英のみ） ２マンの再編集（トリップ毎に存在をチェックし、存在しない場合の日報を削除
    ''' </summary>
    ''' <param name="I_TBL">対象テーブル</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub EditTwoManForKouei(ByVal I_TBL As DataTable, ByRef O_RTN As String)

        Dim WW_T0005tbl As DataTable = T0005INPtbl.Clone
        Dim WW_T0005INPtbl As DataTable = T0005INPtbl.Clone
        Dim WW_TWOMANtbl As DataTable = T0005INPtbl.Clone
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_YMD As String = ""
        Dim WW_NIPPONO As String = ""
        Dim WW_STAFFCODE As String = ""

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '２マン対象データ抽出
            CS0026TBLSORT.TABLE = I_TBL
            CS0026TBLSORT.SORTING = "YMD, NIPPONO, STAFFCODE"
            CS0026TBLSORT.FILTER = "STAFFCODE2 <> ''"
            I_TBL = CS0026TBLSORT.sort()

            For Each WW_KEYrow As DataRow In I_TBL.Rows
                For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                    Dim WW_T5INProw As DataRow = T0005INPtbl.Rows(i)
                    If WW_KEYrow("NIPPONO") = WW_T5INProw("NIPPONO") AndAlso
                       WW_KEYrow("STAFFCODE") = WW_T5INProw("STAFFCODE") Then
                        Dim WW_T5row As DataRow = WW_T0005INPtbl.NewRow
                        WW_T5row.ItemArray = WW_T5INProw.ItemArray
                        WW_T0005INPtbl.Rows.Add(WW_T5row)
                    End If
                Next
            Next

            Dim WW_IDX As Integer = 0
            For Each WW_KEYrow As DataRow In I_TBL.Rows
                WW_T0005tbl.Clear()
                WW_TWOMANtbl.Clear()

                For i As Integer = WW_IDX To WW_T0005INPtbl.Rows.Count - 1
                    Dim WW_T5INProw As DataRow = WW_T0005INPtbl.Rows(i)
                    If WW_KEYrow("NIPPONO") = WW_T5INProw("NIPPONO") AndAlso
                       WW_KEYrow("STAFFCODE") = WW_T5INProw("STAFFCODE") Then

                        Dim WW_Row As DataRow = WW_T0005tbl.NewRow
                        WW_Row.ItemArray = WW_T5INProw.ItemArray
                        WW_T0005tbl.Rows.Add(WW_Row)
                    Else
                        WW_IDX = i
                        Exit For
                    End If
                Next

                '配送受注を検索し、副乗務員の存在をチェック
                Dim WW_STtrip As String = ""
                Dim WW_ENDtrip As String = ""
                '２マン開始終了が出庫、帰庫より大きい（基本ありえない）

                '開始日の開始位置
                For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                    Dim WW_T5row As DataRow = WW_T0005tbl.Rows(i)
                    If WW_T5row("HDKBN") = "H" Then
                        Continue For
                    End If
                    If CDate(WW_KEYrow("STDATE2") & " " & WW_KEYrow("STTIME2")) >= CDate(WW_T5row("STDATE") & " " & WW_T5row("STTIME")) AndAlso
                       CDate(WW_KEYrow("STDATE2") & " " & WW_KEYrow("STTIME2")) <= CDate(WW_T5row("ENDDATE") & " " & WW_T5row("ENDTIME")) Then
                        WW_STtrip = WW_T5row("TWOMANTRIP")
                        Exit For
                    End If
                Next

                '終了日の開始位置
                For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                    Dim WW_T5row As DataRow = WW_T0005tbl.Rows(i)
                    If WW_T5row("HDKBN") = "H" Then
                        Continue For
                    End If
                    If CDate(WW_KEYrow("ENDDATE2") & " " & WW_KEYrow("ENDTIME2")) >= CDate(WW_T5row("STDATE") & " " & WW_T5row("STTIME")) AndAlso
                       CDate(WW_KEYrow("ENDDATE2") & " " & WW_KEYrow("ENDTIME2")) <= CDate(WW_T5row("ENDDATE") & " " & WW_T5row("ENDTIME")) Then
                        WW_ENDtrip = WW_T5row("TWOMANTRIP")
                        Exit For
                    End If
                Next

                '切り出し
                For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                    Dim WW_T5row As DataRow = WW_T0005tbl.Rows(i)
                    If WW_T5row("HDKBN") = "H" Then
                        Continue For
                    End If

                    If WW_STtrip <= WW_T5row("TWOMANTRIP") AndAlso WW_T5row("TWOMANTRIP") <= WW_ENDtrip Then
                        Dim TWOMANrow As DataRow = WW_TWOMANtbl.NewRow
                        TWOMANrow.ItemArray = WW_T5row.ItemArray

                        TWOMANrow("STAFFCODE") = WW_KEYrow("STAFFCODE2")
                        TWOMANrow("STAFFNAMES") = ""
                        CodeToName("STAFFCODE", TWOMANrow("STAFFCODE"), TWOMANrow("STAFFNAMES"), WW_RTN)
                        TWOMANrow("SUBSTAFFCODE") = ""
                        TWOMANrow("SUBSTAFFNAMES") = ""
                        TWOMANrow("CREWKBN") = "2"
                        TWOMANrow("CREWKBNNAMES") = ""
                        CodeToName("CREWKBN", WW_T0005tbl.Rows(i)("CREWKBN"), WW_T0005tbl.Rows(i)("CREWKBNNAMES"), WW_RTN)

                        WW_TWOMANtbl.Rows.Add(TWOMANrow)
                    End If
                Next

                Dim WW_F1cnt As Integer = 0
                Dim WW_F3cnt As Integer = 0
                For i As Integer = 0 To WW_TWOMANtbl.Rows.Count - 1
                    If WW_TWOMANtbl.Rows(i)("WORKKBN") = "F1" Then
                        WW_F1cnt += 1
                    End If
                    If WW_TWOMANtbl.Rows(i)("WORKKBN") = "F3" Then
                        WW_F3cnt += 1
                    End If
                Next

                Dim WW_WORKtbl As DataTable = WW_TWOMANtbl.Clone
                WW_WORKtbl.Clear()
                '出庫が１件もない場合、先頭行に造成する
                If WW_F1cnt = 0 Then
                    Dim WW_T5row As DataRow = WW_TWOMANtbl.Rows(0)
                    Dim TWOMANrow As DataRow = WW_WORKtbl.NewRow
                    '出庫か帰庫がない
                    T0005COM.InitialT5INPRow(TWOMANrow)
                    TWOMANrow("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                    TWOMANrow("SHIPORG") = work.WF_SEL_UORG.Text
                    '開始日時、前のレコードの終了日時
                    TWOMANrow("STDATE") = WW_T5row("STDATE")
                    TWOMANrow("STTIME") = WW_T5row("STTIME")
                    TWOMANrow("ENDTIME") = WW_T5row("STTIME")
                    '終了日時、後ろレコードの開始日時
                    TWOMANrow("ENDDATE") = WW_T5row("STDATE")

                    'その他の項目は、現在のレコードをコピーする
                    TWOMANrow("YMD") = WW_T5row("YMD")
                    TWOMANrow("GSHABAN") = WW_T5row("GSHABAN")
                    TWOMANrow("NIPPONO") = WW_T5row("NIPPONO")
                    TWOMANrow("STAFFCODE") = WW_KEYrow("STAFFCODE2")
                    TWOMANrow("SUBSTAFFCODE") = ""
                    TWOMANrow("CREWKBN") = WW_T5row("CREWKBN")
                    TWOMANrow("TERMKBN") = WW_T5row("TERMKBN")
                    TWOMANrow("HDKBN") = "D"
                    TWOMANrow("WORKKBN") = "F1"
                    TWOMANrow("SEQ") = "000" '仮SEQ

                    TWOMANrow("CAMPNAMES") = ""
                    CodeToName("CAMPCODE", TWOMANrow("CAMPCODE"), TWOMANrow("CAMPNAMES"), WW_RTN)
                    TWOMANrow("SHIPORGNAMES") = ""
                    CodeToName("SHIPORG", TWOMANrow("SHIPORG"), TWOMANrow("SHIPORGNAMES"), WW_RTN)
                    TWOMANrow("TERMKBNNAMES") = ""
                    CodeToName("TERMKBN", TWOMANrow("TERMKBN"), TWOMANrow("TERMKBNNAMES"), WW_RTN)
                    TWOMANrow("WORKKBNNAMES") = ""
                    CodeToName("WORKKBN", TWOMANrow("WORKKBN"), TWOMANrow("WORKKBNNAMES"), WW_RTN)
                    TWOMANrow("STAFFNAMES") = ""
                    CodeToName("STAFFCODE", TWOMANrow("STAFFCODE"), TWOMANrow("STAFFNAMES"), WW_RTN)
                    TWOMANrow("CREWKBNNAMES") = ""
                    CodeToName("CREWKBN", TWOMANrow("CREWKBN"), TWOMANrow("CREWKBNNAMES"), WW_RTN)
                    WW_WORKtbl.Rows.Add(TWOMANrow)

                    Dim TWOMANrow2 As DataRow = WW_WORKtbl.NewRow
                    TWOMANrow2.ItemArray = TWOMANrow.ItemArray
                    TWOMANrow2("WORKKBN") = "A1"
                    TWOMANrow2("WORKKBNNAMES") = ""
                    CodeToName("WORKKBN", TWOMANrow2("WORKKBN"), TWOMANrow2("WORKKBNNAMES"), WW_RTN)
                    WW_WORKtbl.Rows.Add(TWOMANrow2)
                    WW_TWOMANtbl.Merge(WW_WORKtbl)
                End If
                '帰庫が１件もない場合、最終行に造成する
                If WW_F3cnt = 0 Then
                    Dim WW_T5row As DataRow = WW_TWOMANtbl.Rows(WW_TWOMANtbl.Rows.Count - 1)
                    Dim TWOMANrow As DataRow = WW_WORKtbl.NewRow
                    '出庫か帰庫がない
                    T0005COM.InitialT5INPRow(TWOMANrow)
                    TWOMANrow("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                    TWOMANrow("SHIPORG") = work.WF_SEL_UORG.Text
                    '開始日時、前のレコードの終了日時
                    TWOMANrow("STDATE") = WW_T5row("ENDDATE")
                    TWOMANrow("STTIME") = WW_T5row("ENDTIME")
                    TWOMANrow("ENDTIME") = WW_T5row("ENDTIME")
                    '終了日時、後ろレコードの開始日時
                    TWOMANrow("ENDDATE") = WW_T5row("ENDDATE")

                    'その他の項目は、現在のレコードをコピーする
                    TWOMANrow("YMD") = WW_T5row("YMD")
                    TWOMANrow("GSHABAN") = WW_T5row("GSHABAN")
                    TWOMANrow("NIPPONO") = WW_T5row("NIPPONO")
                    TWOMANrow("STAFFCODE") = WW_KEYrow("STAFFCODE2")
                    TWOMANrow("SUBSTAFFCODE") = ""
                    TWOMANrow("CREWKBN") = WW_T5row("CREWKBN")
                    TWOMANrow("TERMKBN") = WW_T5row("TERMKBN")
                    TWOMANrow("HDKBN") = "D"
                    TWOMANrow("WORKKBN") = "F3"
                    TWOMANrow("SEQ") = "999" '仮SEQ

                    TWOMANrow("CAMPNAMES") = ""
                    CodeToName("CAMPCODE", TWOMANrow("CAMPCODE"), TWOMANrow("CAMPNAMES"), WW_RTN)
                    TWOMANrow("SHIPORGNAMES") = ""
                    CodeToName("SHIPORG", TWOMANrow("SHIPORG"), TWOMANrow("SHIPORGNAMES"), WW_RTN)
                    TWOMANrow("TERMKBNNAMES") = ""
                    CodeToName("TERMKBN", TWOMANrow("TERMKBN"), TWOMANrow("TERMKBNNAMES"), WW_RTN)
                    TWOMANrow("WORKKBNNAMES") = ""
                    CodeToName("WORKKBN", TWOMANrow("WORKKBN"), TWOMANrow("WORKKBNNAMES"), WW_RTN)
                    TWOMANrow("STAFFNAMES") = ""
                    CodeToName("STAFFCODE", TWOMANrow("STAFFCODE"), TWOMANrow("STAFFNAMES"), WW_RTN)
                    TWOMANrow("CREWKBNNAMES") = ""
                    CodeToName("CREWKBN", TWOMANrow("CREWKBN"), TWOMANrow("CREWKBNNAMES"), WW_RTN)
                    WW_WORKtbl.Rows.Add(TWOMANrow)

                    Dim TWOMANrow2 As DataRow = WW_WORKtbl.NewRow
                    TWOMANrow2.ItemArray = TWOMANrow.ItemArray
                    TWOMANrow2("WORKKBN") = "Z1"
                    TWOMANrow2("WORKKBNNAMES") = ""
                    CodeToName("WORKKBN", TWOMANrow2("WORKKBN"), TWOMANrow2("WORKKBNNAMES"), WW_RTN)
                    WW_WORKtbl.Rows.Add(TWOMANrow2)

                    WW_TWOMANtbl.Merge(WW_WORKtbl)

                End If

                '２マンレコードの追加
                T0005INPtbl.Merge(WW_TWOMANtbl)

            Next

            '------------------------------------------------------------
            '■出庫日、従業員 単位
            '  出庫日、従業員毎に集約し直す
            '------------------------------------------------------------
            CreateT0005Header(T0005INPtbl)

            '---------------------------------------------------
            '■出庫日、従業員 単位
            '  明細行番号（並び順）の振り直し
            '---------------------------------------------------
            CS0026TBLSORT.TABLE = T0005INPtbl
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            T0005INPtbl = CS0026TBLSORT.sort()

            Dim WW_SEQ As Integer = 1

            For Each row As DataRow In T0005INPtbl.Rows
                '行番号の採番
                If row("HDKBN") = "H" Then
                    WW_SEQ = 1
                    row("SEQ") = WW_SEQ.ToString("000")
                    Continue For
                End If
                row("SEQ") = WW_SEQ.ToString("000")
                WW_SEQ = WW_SEQ + 1
            Next

            WW_T0005tbl.Dispose()
            WW_T0005tbl = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
            CS0011LOGWRITE.INFSUBCLASS = "T0005tbl_Edit"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' エラーメッセージ編集（光英）
    ''' </summary>
    ''' <param name="I_ROW">エラー対象行</param>
    ''' <param name="I_FIELDNM">フィールド名</param>
    ''' <param name="I_MSG">メッセージ</param>
    ''' <param name="I_ERRCD">エラーコード</param>
    ''' <param name="I_MODE">登録モード</param>
    ''' <remarks></remarks>
    Sub OutputErrorMessageByKouei(ByVal I_ROW As DataRow, ByVal I_FIELDNM As String, ByVal I_MSG As String, ByVal I_ERRCD As String, ByVal I_MODE As String)
        'エラーレポート編集
        Dim WW_ERR_MES As String = ""
        Select Case I_ERRCD
            Case C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_ERR_MES = "更新できないレコード(" & I_FIELDNM & ")です。"
            Case C_MESSAGE_NO.BOX_ERROR_EXIST
                WW_ERR_MES = "・エラーが存在します。(" & I_FIELDNM & "エラー)"
        End Select
        Select Case I_MODE
            Case GRT00005WRKINC.TERM_TYPE.JX
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> jx_jotsyasai.csv  , "
            Case GRT00005WRKINC.TERM_TYPE.TG
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> tg_jotsyasai.csv  , "
            Case GRT00005WRKINC.TERM_TYPE.JOT
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> jot_jotsyasai.csv  , "
            Case GRT00005WRKINC.TERM_TYPE.COSMO
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> cosmo_jotsyasai.csv  , "
        End Select
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MSG & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 稼働日    =" & I_ROW("FIELD4") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日報№    =" & I_ROW("FIELD10") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 社員コード=" & I_ROW("FIELD6") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車載ＳＥＱ=" & I_ROW("FIELD8") & "  "
        SetErrorMessage(WW_ERR_MES)

        WW_ERRLIST_ALL.Add(I_ERRCD)
        WW_ERRLIST.Add(I_ERRCD)
    End Sub

#End Region

    ''' <summary>
    ''' 光英受信ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Download_Click()
        Const C_DIR_KOUEI As String = "KOUEI"                    '光英連携ディレクトリ名(LOCAL)
        Const C_DIR_KOUEI_RESULT As String = "result"            '光英連携日報ディレクトリ名
        Const C_DIR_KOUEI_SPLIT As String = "sprit"              '光英連携日報分割後ディレクトリ名
        Const C_KOUEI_RESULT_FILE_SERCH As String = "*_jotsyasai_*.csv"   '光英連携日報ファイルSearchPattern

        rightview.SetErrorReport("")
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
        Dim sm As New CS0050SESSION

        '受信ファイルリスト
        Dim dicFileList As New Dictionary(Of String, List(Of FileInfo))
        Dim dicFileSplitList As New Dictionary(Of String, List(Of FileInfo))
        Dim recFileList As New List(Of String)
        '光英ファイルFTP受信
        work.GetKoueiFile(work.WF_SEL_UORG.Text, dicFileList, O_RTN)
        If Not isNormal(O_RTN) Then
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR, "光英ファイル受信")
            Exit Sub
        End If

        If dicFileList.Count > 0 Then
            '光英ファイルを日付、日報番号で分割
            For Each type In dicFileList
                For i As Integer = 0 To type.Value.Count - 1

                    '①受信ファイル読み込み
                    Dim dir = New DirectoryInfo(type.Value(0).DirectoryName & "\" & C_DIR_KOUEI_SPLIT)
                    If dir.Exists Then
                    Else
                        '分割ファイル格納フォルダー作成
                        dir.Create()
                    End If

                    Dim WW_Text As New FileIO.TextFieldParser(type.Value(i).FullName, System.Text.Encoding.GetEncoding(932))
                    '②キーブレイク（日付、日報番号）
                    Dim WW_First As String = "OFF"
                    Dim OLD_YMD As String = ""
                    Dim OLD_NIPPONO As String = ""
                    Dim SAVEstr As New System.Text.StringBuilder()
                    Dim fileName() As String = type.Value(i).Name.Split(".")
                    Dim outFileName As String = ""

                    While Not WW_Text.EndOfData
                        'CSVファイルのフィールドを読み込みます。
                        Dim recode As String = WW_Text.ReadLine
                        Dim fields As String() = recode.Split(",")
                        If WW_First = "OFF" Then
                            OLD_YMD = fields(3)
                            OLD_NIPPONO = fields(9)
                            WW_First = "ON"
                        End If
                        If fields(3) <> OLD_YMD OrElse fields(9) <> OLD_NIPPONO Then
                            '④ファイル名変更（日付、日報番号）
                            outFileName = dir.FullName & "\" & fileName(0) & "_" & OLD_YMD & "_" & OLD_NIPPONO & "." & fileName(1)
                            '⑤ファイル出力（日付、日報番号）
                            Dim SaveF As New System.IO.StreamWriter(outFileName, False, System.Text.Encoding.GetEncoding("unicode"))
                            SaveF.Write(SAVEstr)
                            SaveF.Close()
                            SaveF.Dispose()
                            SAVEstr.Clear()
                        End If
                        '③出力（日付、日報番号）
                        SAVEstr.Append(recode)
                        SAVEstr.Append(vbCrLf)
                        OLD_YMD = fields(3)
                        OLD_NIPPONO = fields(9)
                    End While

                    If OLD_YMD <> "" Then
                        '④ファイル名変更（日付、日報番号）
                        outFileName = dir.FullName & "\" & fileName(0) & "_" & OLD_YMD & "_" & OLD_NIPPONO & "." & fileName(1)
                        '⑤ファイル出力（日付、日報番号）
                        Dim SaveF As New System.IO.StreamWriter(outFileName, False, System.Text.Encoding.GetEncoding("unicode"))
                        SaveF.Write(SAVEstr)
                        SaveF.Close()
                        SaveF.Dispose()
                    End If

                    'ファイルを解放します。
                    WW_Text.Close()

                    Dim f = New FileInfo(type.Value(i).FullName)
                    If f.Exists Then
                        '光英連携が安定稼働するまでは論理削除
                        Dim bakFileName As New FileInfo(f.FullName & ".used")
                        If bakFileName.Exists Then
                            bakFileName.Delete()
                        End If
                        f.MoveTo(bakFileName.FullName)
                    End If
                Next
            Next
        End If

        '分割済みローカル光英ファイル取得
        Dim koueiPath As String = Path.Combine(sm.UPLOAD_PATH, C_DIR_KOUEI, work.WF_SEL_UORG.Text, C_DIR_KOUEI_RESULT, C_DIR_KOUEI_SPLIT)
        Dim localDirSprit = New DirectoryInfo(koueiPath)
        If localDirSprit.Exists Then
            Dim localFilesSprit = localDirSprit.GetFiles(C_KOUEI_RESULT_FILE_SERCH)

            If localFilesSprit.Count > 0 Then

                For Each file In localFilesSprit
                    'ファイル名の1項目目はタイプ（区切り文字:"_"）
                    Dim wk = file.Name.Split("_")
                    Dim filetype = wk(0)
                    'タイプ別ファイル一覧作成
                    If Not dicFileSplitList.ContainsKey(filetype) Then
                        dicFileSplitList.Add(filetype, New List(Of FileInfo))
                    End If
                    dicFileSplitList(filetype).Add(file)
                Next

                '光英ファイルが存在する場合は取込
                'For Each type In dicFileSplitList
                For Each type In dicFileSplitList
                    Dim mode = GRT00005WRKINC.FILE_SUFFIX.Suffix2TermType(type.Key)
                    UPLOAD_KOUEI(mode, type.Value(0).DirectoryName, O_RTN)
                Next
            Else
                '○メッセージ表示
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.INF, )
            End If
        Else
            '○メッセージ表示
            Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.INF, )
        End If

    End Sub

    ''' <summary>
    ''' CSVファイル読み込み
    ''' </summary>
    ''' <param name="I_FILENAME">CSVファイル名</param>
    ''' <param name="O_TBLNAME">テーブルデータ</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub ReadCsvFile(ByVal I_FILENAME As String, ByRef O_TBLNAME As DataTable, ByRef O_RTN As String)

        Try
            O_RTN = C_MESSAGE_NO.NORMAL
            'Shift JISで読み込みます。
            Dim WW_Text As New FileIO.TextFieldParser(I_FILENAME, System.Text.Encoding.GetEncoding(932))

            'フィールドが文字で区切られている設定を行います。
            '（初期値がDelimited）
            WW_Text.TextFieldType = FileIO.FieldType.Delimited

            '区切り文字を「,（カンマ）」に設定します。
            WW_Text.Delimiters = New String() {","}

            'フィールドを"で囲み、改行文字、区切り文字を含めることが 'できるかを設定します。
            '（初期値がtrue）
            WW_Text.HasFieldsEnclosedInQuotes = True

            'フィールドの前後からスペースを削除する設定を行います。
            '（初期値がtrue）
            WW_Text.TrimWhiteSpace = True

            Dim WW_Cnt As Integer = 0
            While Not WW_Text.EndOfData
                'CSVファイルのフィールドを読み込みます。
                Dim fields As String() = WW_Text.ReadFields()
                Dim dr As DataRow = O_TBLNAME.NewRow()
                Dim wk_fields As String() = New String(O_TBLNAME.Columns.Count - 1) {}
                If fields.Count <= O_TBLNAME.Columns.Count Then
                    WW_Cnt = fields.Count
                ElseIf fields.Count > O_TBLNAME.Columns.Count Then
                    WW_Cnt = O_TBLNAME.Columns.Count
                End If
                Array.Copy(fields, 0, wk_fields, 0, WW_Cnt)
                dr.ItemArray = wk_fields
                'dr.ItemArray = fields
                O_TBLNAME.Rows.Add(dr)
            End While

            'ファイルを解放します。
            WW_Text.Close()

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT)
            CS0011LOGWRITE.INFSUBCLASS = "ReadCsvFile"                     'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.FILE_IO_ERROR
            Exit Sub
        End Try

    End Sub


    ''' <summary>
    ''' ヘッダーレコード作成★×
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Protected Sub CreateT0005Header(ByRef IO_TBL As DataTable)

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_Cols As String() = {"YMD", "STAFFCODE"}
        Dim WW_T0005tbl As DataTable = IO_TBL.Clone
        Dim WW_TBLview As DataView
        Dim WW_T0005row As DataRow

        Try
            '更新元（削除）データをキープ
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            CS0026TBLSORT.SORTING = "SELECT"
            Dim WW_T0005DELtbl As DataTable = CS0026TBLSORT.sort()

            '出庫日、乗務員でグループ化しキーテーブル作成
            CS0026TBLSORT.FILTER = "HDKBN = 'D' and SELECT = '1'"
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            Dim WW_T0005SELtbl As DataTable = CS0026TBLSORT.sort()
            WW_TBLview = New DataView(WW_T0005SELtbl)
            '抽出後のテーブルに置き換える（ヘッダなし、明細のみ）
            IO_TBL = WW_T0005SELtbl.Copy()
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
                WW_T0005row = WW_T0005tbl.NewRow
                T0005COM.InitialT5INPRow(WW_T0005row)
                WW_T0005row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                WW_T0005row("SHIPORG") = work.WF_SEL_UORG.Text

                For i As Integer = WW_IDX To WW_T0005SELtbl.Rows.Count - 1
                    Dim WW_SELrow As DataRow = WW_T0005SELtbl.Rows(i)
                    If WW_KEYrow("YMD") = WW_SELrow("YMD") AndAlso
                       WW_KEYrow("STAFFCODE") = WW_SELrow("STAFFCODE") Then
                        If WW_SELrow("DELFLG") = C_DELETE_FLG.ALIVE Then
                            If Not WW_FIRST Then
                                WW_FIRST = True
                                '先頭レコードより開始日、開始時間を取得
                                WW_T0005row("STDATE") = WW_SELrow("STDATE")
                                WW_T0005row("STTIME") = WW_SELrow("STTIME")
                                WW_T0005row("TERMKBN") = WW_SELrow("TERMKBN")
                                WW_T0005row("CREWKBN") = WW_SELrow("CREWKBN")
                                WW_T0005row("SUBSTAFFCODE") = WW_SELrow("SUBSTAFFCODE")
                                WW_T0005row("JISSKIKBN") = WW_SELrow("JISSKIKBN")
                            End If

                            '最終レコードの終了日、終了時間を取得
                            WW_T0005row("ENDDATE") = WW_SELrow("ENDDATE")
                            WW_T0005row("ENDTIME") = WW_SELrow("ENDTIME")

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
                                WW_T0005row("TIMSTP") = WW_SELrow("TIMSTP")
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
                    WW_T0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                ElseIf WW_OPE_UPD Then
                    WW_T0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Else
                    WW_T0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                End If
                WW_T0005row("YMD") = WW_KEYrow("YMD")
                WW_T0005row("STAFFCODE") = WW_KEYrow("STAFFCODE")
                WW_T0005row("SELECT") = "1"
                WW_T0005row("HIDDEN") = "0"
                WW_T0005row("HDKBN") = "H"
                WW_T0005row("SEQ") = "001"
                If WW_ALIVE_FLG Then
                    WW_T0005row("DELFLG") = C_DELETE_FLG.ALIVE
                Else
                    WW_T0005row("DELFLG") = C_DELETE_FLG.DELETE
                End If
                Dim WW_WORKTIME As Integer = 0

                '作業時間
                WW_WORKTIME = DateDiff("n",
                                      WW_T0005row("STDATE") + " " + WW_T0005row("STTIME"),
                                      WW_T0005row("ENDDATE") + " " + WW_T0005row("ENDTIME")
                                     )
                WW_T0005row("WORKTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
                WW_T0005row("ACTTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
                WW_T0005row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                WW_T0005row("KYUYU") = Val(WW_KYUYU).ToString("#,0.00")
                WW_T0005row("TOTALTOLL") = Val(WW_TOTALTOLL).ToString("#,0")

                WW_T0005row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                WW_T0005row("JIDISTANCE") = Val(WW_JIDISTANCE).ToString("#,0.00")
                WW_T0005row("KUDISTANCE") = Val(WW_KUDISTANCE).ToString("#,0.00")
                WW_T0005row("IPPDISTANCE") = Val(WW_IPPDISTANCE).ToString("#,0.00")
                WW_T0005row("KOSDISTANCE") = Val(WW_KOSDISTANCE).ToString("#,0.00")
                WW_T0005row("IPPJIDISTANCE") = Val(WW_IPPJIDISTANCE).ToString("#,0.00")
                WW_T0005row("IPPKUDISTANCE") = Val(WW_IPPKUDISTANCE).ToString("#,0.00")
                WW_T0005row("KOSJIDISTANCE") = Val(WW_KOSJIDISTANCE).ToString("#,0.00")
                WW_T0005row("KOSKUDISTANCE") = Val(WW_KOSKUDISTANCE).ToString("#,0.00")

                WW_T0005row("CAMPNAMES") = ""
                CodeToName("CAMPCODE", WW_T0005row("CAMPCODE"), WW_T0005row("CAMPNAMES"), WW_RTN)
                WW_T0005row("SHIPORGNAMES") = ""
                CodeToName("SHIPORG", WW_T0005row("SHIPORG"), WW_T0005row("SHIPORGNAMES"), WW_RTN)
                WW_T0005row("TERMKBNNAMES") = ""
                CodeToName("TERMKBN", WW_T0005row("TERMKBN"), WW_T0005row("TERMKBNNAMES"), WW_RTN)
                WW_T0005row("STAFFNAMES") = ""
                CodeToName("STAFFCODE", WW_T0005row("STAFFCODE"), WW_T0005row("STAFFNAMES"), WW_RTN)
                WW_T0005row("SUBSTAFFNAMES") = ""
                CodeToName("STAFFCODE", WW_T0005row("SUBSTAFFCODE"), WW_T0005row("SUBSTAFFNAMES"), WW_RTN)
                WW_T0005row("CREWKBNNAMES") = ""
                CodeToName("CREWKBN", WW_T0005row("CREWKBN"), WW_T0005row("CREWKBNNAMES"), WW_RTN)
                WW_T0005row("JISSKIKBNNAMES") = ""
                CodeToName("JISSKIKBN", WW_T0005row("JISSKIKBN"), WW_T0005row("JISSKIKBNNAMES"), WW_RTN)

                WW_LINECNT = WW_LINECNT + 1
                WW_T0005row("LINECNT") = WW_LINECNT
                WW_T0005tbl.Rows.Add(WW_T0005row)
            Next

            'ヘッダのマージ
            IO_TBL.Merge(WW_T0005tbl)

            '更新元（削除）データの戻し
            IO_TBL.Merge(WW_T0005DELtbl)

            'ソート
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.FILTER = ""
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            IO_TBL = CS0026TBLSORT.sort()

            WW_KEYtbl.Dispose()
            WW_KEYtbl = Nothing
            WW_TBLview.Dispose()
            WW_TBLview = Nothing
            WW_T0005DELtbl.Dispose()
            WW_T0005DELtbl = Nothing
            WW_T0005SELtbl.Dispose()
            WW_T0005SELtbl = Nothing
            WW_T0005tbl.Dispose()
            WW_T0005tbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CreateT0005Header"                'SUBクラス名
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
    Protected Sub CreateT0005HeaderNew(ByRef IO_TBL As DataTable)

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_WORKTIME As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_T0005tbl As DataTable = IO_TBL.Clone
        Dim WW_T0005DELtbl As DataTable
        Dim WW_T0005SELtbl As DataTable
        Dim WW_T0005row As DataRow

        Try
            '更新元（削除）データをキープ
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.SORTING = "SELECT"
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            WW_T0005DELtbl = CS0026TBLSORT.sort()

            '出庫日、乗務員でグループ化しキーテーブル作成
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "HDKBN = 'D' and SELECT = '1'"
            WW_T0005SELtbl = CS0026TBLSORT.sort()

            '抽出後のテーブルに置き換える（ヘッダなし、明細のみ）
            IO_TBL = WW_T0005SELtbl.Copy()

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
            WW_T0005row = WW_T0005tbl.NewRow
            T0005COM.InitialT5INPRow(WW_T0005row)
            WW_T0005row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            WW_T0005row("SHIPORG") = work.WF_SEL_UORG.Text

            Dim wIO_TBL As DataRow = IO_TBL.NewRow
            For i As Integer = 0 To IO_TBL.Rows.Count - 1
                wIO_TBL = IO_TBL.Rows(i)

                If wIO_TBL("DELFLG") <> C_DELETE_FLG.DELETE Then

                    '先頭レコード
                    If wIO_TBL("YMD") <> WW_YMD OrElse wIO_TBL("STAFFCODE") <> WW_STAFFCODE Then

                        'KeyBreak処理
                        If WW_YMD <> "" Then
                            WW_LINECNT = WW_LINECNT + 1
                            WW_T0005row("LINECNT") = WW_LINECNT
                            WW_T0005tbl.Rows.Add(WW_T0005row)
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

                        WW_T0005row = WW_T0005tbl.NewRow
                        T0005COM.InitialT5INPRow(WW_T0005row)

                        WW_YMD = wIO_TBL("YMD")
                        WW_STAFFCODE = wIO_TBL("STAFFCODE")

                        'ヘッダー項目
                        WW_T0005row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                        WW_T0005row("YMD") = wIO_TBL("YMD")
                        WW_T0005row("SHIPORG") = work.WF_SEL_UORG.Text
                        WW_T0005row("STAFFCODE") = wIO_TBL("STAFFCODE")
                        WW_T0005row("SELECT") = "1"
                        WW_T0005row("HIDDEN") = "0"
                        WW_T0005row("HDKBN") = "H"
                        WW_T0005row("SEQ") = "001"
                        '開始日、開始時間を取得
                        WW_T0005row("STDATE") = wIO_TBL("STDATE")
                        WW_T0005row("STTIME") = wIO_TBL("STTIME")
                        WW_T0005row("TERMKBN") = wIO_TBL("TERMKBN")
                        WW_T0005row("CREWKBN") = wIO_TBL("CREWKBN")
                        WW_T0005row("SUBSTAFFCODE") = wIO_TBL("SUBSTAFFCODE")
                        WW_T0005row("JISSKIKBN") = wIO_TBL("JISSKIKBN")
                    End If

                    '終了日、終了時間
                    WW_T0005row("ENDDATE") = wIO_TBL("ENDDATE")
                    WW_T0005row("ENDTIME") = wIO_TBL("ENDTIME")

                    '出庫
                    If wIO_TBL("WORKKBN") = "F1" Then
                        WW_T0005row("STMATER") = wIO_TBL("STMATER")
                    End If

                    '帰庫
                    If wIO_TBL("WORKKBN") = "F3" Then
                        WW_T0005row("ENDMATER") = wIO_TBL("ENDMATER")
                        WW_T0005row("RUIDISTANCE") = wIO_TBL("RUIDISTANCE")
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
                                          WW_T0005row("STDATE") + " " + WW_T0005row("STTIME"),
                                          WW_T0005row("ENDDATE") + " " + WW_T0005row("ENDTIME")
                                         )
                    WW_T0005row("WORKTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
                    WW_T0005row("ACTTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
                    WW_T0005row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                    WW_T0005row("KYUYU") = Val(WW_KYUYU).ToString("#,0.00")
                    WW_T0005row("TOTALTOLL") = Val(WW_TOTALTOLL).ToString("#,0")

                    WW_T0005row("SOUDISTANCE") = Val(WW_SOUDISTANCE).ToString("#,0.00")
                    WW_T0005row("JIDISTANCE") = Val(WW_JIDISTANCE).ToString("#,0.00")
                    WW_T0005row("KUDISTANCE") = Val(WW_KUDISTANCE).ToString("#,0.00")
                    WW_T0005row("IPPDISTANCE") = Val(WW_IPPDISTANCE).ToString("#,0.00")
                    WW_T0005row("KOSDISTANCE") = Val(WW_KOSDISTANCE).ToString("#,0.00")
                    WW_T0005row("IPPJIDISTANCE") = Val(WW_IPPJIDISTANCE).ToString("#,0.00")
                    WW_T0005row("IPPKUDISTANCE") = Val(WW_IPPKUDISTANCE).ToString("#,0.00")
                    WW_T0005row("KOSJIDISTANCE") = Val(WW_KOSJIDISTANCE).ToString("#,0.00")
                    WW_T0005row("KOSKUDISTANCE") = Val(WW_KOSKUDISTANCE).ToString("#,0.00")

                    'タイムスタンプがゼロ以外が存在する場合、ヘッダにもとりあえずタイムスタンプ設定
                    'ヘッダで、ＤＢ登録済のデータか、初取込データ（新規を含む）かを判断できるようにする
                    If wIO_TBL("TIMSTP") <> "0" Then
                        WW_T0005row("TIMSTP") = wIO_TBL("TIMSTP")
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
                        WW_T0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    ElseIf WW_OPE_UPD = "ON" Then
                        WW_T0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    Else
                        WW_T0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    End If
                    If WW_DEL_FLG = "ON" Then
                        WW_T0005row("DELFLG") = C_DELETE_FLG.ALIVE
                    Else
                        WW_T0005row("DELFLG") = C_DELETE_FLG.DELETE
                    End If

                    '名称
                    WW_T0005row("CAMPNAMES") = ""
                    CodeToName("CAMPCODE", WW_T0005row("CAMPCODE"), WW_T0005row("CAMPNAMES"), WW_RTN)
                    WW_T0005row("SHIPORGNAMES") = ""
                    CodeToName("SHIPORG", WW_T0005row("SHIPORG"), WW_T0005row("SHIPORGNAMES"), WW_RTN)
                    WW_T0005row("TERMKBNNAMES") = ""
                    CodeToName("TERMKBN", WW_T0005row("TERMKBN"), WW_T0005row("TERMKBNNAMES"), WW_RTN)
                    WW_T0005row("STAFFNAMES") = ""
                    CodeToName("STAFFCODE", WW_T0005row("STAFFCODE"), WW_T0005row("STAFFNAMES"), WW_RTN)
                    WW_T0005row("SUBSTAFFNAMES") = ""
                    CodeToName("STAFFCODE", WW_T0005row("SUBSTAFFCODE"), WW_T0005row("SUBSTAFFNAMES"), WW_RTN)
                    WW_T0005row("CREWKBNNAMES") = ""
                    CodeToName("CREWKBN", WW_T0005row("CREWKBN"), WW_T0005row("CREWKBNNAMES"), WW_RTN)
                    WW_T0005row("JISSKIKBNNAMES") = ""
                    CodeToName("JISSKIKBN", WW_T0005row("JISSKIKBN"), WW_T0005row("JISSKIKBNNAMES"), WW_RTN)
                End If

                If i = (IO_TBL.Rows.Count - 1) Then
                    WW_LINECNT = WW_LINECNT + 1
                    WW_T0005row("LINECNT") = WW_LINECNT
                    WW_T0005tbl.Rows.Add(WW_T0005row)
                End If
            Next

            'ヘッダのマージ
            IO_TBL.Merge(WW_T0005tbl)

            '更新元（削除）データの戻し
            IO_TBL.Merge(WW_T0005DELtbl)

            'ソート
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            IO_TBL = CS0026TBLSORT.sort()


            WW_T0005DELtbl.Dispose()
            WW_T0005DELtbl = Nothing
            WW_T0005SELtbl.Dispose()
            WW_T0005SELtbl = Nothing
            WW_T0005tbl.Dispose()
            WW_T0005tbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CreateT0005Header"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    '''  ダミーヘッダレコード作成
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Protected Sub CreateT0005HeaderDummy(ByRef IO_TBL As DataTable)

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_Cols As String() = {"YMD", "STAFFCODE"}
        Dim WW_KEYtbl As DataTable
        Dim WW_T0005tbl As DataTable = IO_TBL.Clone
        Dim WW_T0005SELtbl As DataTable
        Dim WW_TBLview As DataView

        Try
            '出庫日、乗務員でグループ化しキーテーブル作成
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "HDKBN = 'D' and SELECT = '0'"
            WW_T0005SELtbl = CS0026TBLSORT.sort()

            '抽出後のテーブルに置き換える（ヘッダなし、明細のみ）
            IO_TBL = WW_T0005SELtbl.Copy()

            'キーテーブル作成
            WW_TBLview = New DataView(WW_T0005SELtbl)
            WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)
            WW_TBLview.Dispose()
            WW_TBLview = Nothing
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                Dim WW_FIRST As Boolean = False
                Dim WW_OPE_UPD As Boolean = False
                Dim WW_OPE_ERR As Boolean = False

                '初期化
                Dim WW_T0005row As DataRow = WW_T0005tbl.NewRow
                T0005COM.InitialT5INPRow(WW_T0005row)
                WW_T0005row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                WW_T0005row("SHIPORG") = work.WF_SEL_UORG.Text

                For i As Integer = WW_IDX To WW_T0005SELtbl.Rows.Count - 1
                    Dim WW_row As DataRow = WW_T0005SELtbl.Rows(i)
                    If WW_KEYrow("YMD") = WW_row("YMD") AndAlso
                       WW_KEYrow("STAFFCODE") = WW_row("STAFFCODE") Then
                        If Not WW_FIRST Then
                            WW_FIRST = True
                            '先頭レコードより開始日、開始時間を取得
                            WW_T0005row("STDATE") = WW_row("STDATE")
                            WW_T0005row("STTIME") = WW_row("STTIME")
                            WW_T0005row("TERMKBN") = WW_row("TERMKBN")
                            WW_T0005row("CREWKBN") = WW_row("CREWKBN")
                            WW_T0005row("SUBSTAFFCODE") = WW_row("SUBSTAFFCODE")
                            WW_T0005row("JISSKIKBN") = WW_row("JISSKIKBN")
                        End If

                        '最終レコードの終了日、終了時間を取得
                        WW_T0005row("ENDDATE") = WW_row("ENDDATE")
                        WW_T0005row("ENDTIME") = WW_row("ENDTIME")

                        'タイムスタンプがゼロ以外が存在する場合、ヘッダにもとりあえずタイムスタンプ設定
                        'ヘッダで、ＤＢ登録済のデータか、初取込データ（新規を含む）かを判断できるようにする
                        If WW_row("TIMSTP") <> "0" Then
                            WW_T0005row("TIMSTP") = WW_row("TIMSTP")
                        End If

                        If WW_row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                            WW_OPE_UPD = True
                        ElseIf WW_row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                            WW_OPE_ERR = True
                        End If

                    Else
                        WW_IDX = i
                        Exit For
                    End If
                Next

                If WW_OPE_ERR Then
                    WW_T0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                ElseIf WW_OPE_UPD Then
                    WW_T0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Else
                    WW_T0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                End If
                WW_T0005row("YMD") = WW_KEYrow("YMD")
                WW_T0005row("STAFFCODE") = WW_KEYrow("STAFFCODE")
                WW_T0005row("LINECNT") = 0
                WW_T0005row("SELECT") = "0"
                WW_T0005row("HIDDEN") = "1"
                WW_T0005row("HDKBN") = "H"
                WW_T0005row("SEQ") = "001"
                WW_T0005row("DELFLG") = C_DELETE_FLG.DELETE
                WW_T0005row("CAMPNAMES") = ""
                CodeToName("CAMPCODE", WW_T0005row("CAMPCODE"), WW_T0005row("CAMPNAMES"), WW_RTN)
                WW_T0005row("SHIPORGNAMES") = ""
                CodeToName("SHIPORG", WW_T0005row("SHIPORG"), WW_T0005row("SHIPORGNAMES"), WW_RTN)
                WW_T0005row("TERMKBNNAMES") = ""
                CodeToName("TERMKBN", WW_T0005row("TERMKBN"), WW_T0005row("TERMKBNNAMES"), WW_RTN)
                WW_T0005row("STAFFNAMES") = ""
                CodeToName("STAFFCODE", WW_T0005row("STAFFCODE"), WW_T0005row("STAFFNAMES"), WW_RTN)
                WW_T0005row("SUBSTAFFNAMES") = ""
                CodeToName("STAFFCODE", WW_T0005row("SUBSTAFFCODE"), WW_T0005row("SUBSTAFFNAMES"), WW_RTN)
                WW_T0005row("CREWKBNNAMES") = ""
                CodeToName("CREWKBN", WW_T0005row("CREWKBN"), WW_T0005row("CREWKBNNAMES"), WW_RTN)
                WW_T0005row("JISSKIKBNNAMES") = ""
                CodeToName("JISSKIKBN", WW_T0005row("JISSKIKBN"), WW_T0005row("JISSKIKBNNAMES"), WW_RTN)

                WW_T0005tbl.Rows.Add(WW_T0005row)
            Next

            'ヘッダのマージ
            IO_TBL.Merge(WW_T0005tbl)

            'ソート
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            IO_TBL = CS0026TBLSORT.sort()

            WW_KEYtbl.Dispose()
            WW_KEYtbl = Nothing
            WW_T0005SELtbl.Dispose()
            WW_T0005SELtbl = Nothing
            WW_T0005tbl.Dispose()
            WW_T0005tbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CreateT0005HeaderDummy"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' T0005tblチェック
    ''' </summary>
    ''' <param name="T0005INPtbl"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub ConvT0005tblData(ByRef T0005INPtbl As DataTable, ByRef O_RTN As String)

        Dim WW_TEXT As String = ""
        Dim WW_CONVERT As String = ""
        Dim WW_KEYWORD As String = ""

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_T0005INPtbl As DataTable = T0005INPtbl.Clone
        For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
            Dim T0005INProw As DataRow = WW_T0005INPtbl.NewRow
            T0005INProw.ItemArray = T0005INPtbl.Rows(i).ItemArray

            '作業区分名
            CodeToName("WORKKBN", T0005INProw("WORKKBN"), WW_TEXT, WW_DUMMY)
            T0005INProw("WORKKBNNAMES") = WW_TEXT

            '■マスタチェック
            '○会社コード
            CodeToName("CAMPCODE", T0005INProw("CAMPCODE"), WW_TEXT, WW_DUMMY)
            T0005INProw("CAMPNAMES") = WW_TEXT

            '○出荷部署
            CodeToName("SHIPORG", T0005INProw("SHIPORG"), WW_TEXT, WW_DUMMY)
            T0005INProw("SHIPORGNAMES") = WW_TEXT

            '○従業員
            CodeToName("STAFFCODE", T0005INProw("STAFFCODE"), WW_TEXT, WW_DUMMY)
            T0005INProw("STAFFNAMES") = WW_TEXT

            '○業務車番
            If T0005INProw("GSHABAN") <> "" Then
                Select Case T0005INProw("TERMKBN")
                    Case GRT00005WRKINC.TERM_TYPE.YAZAKI
                        WW_KEYWORD = "GSHABANSHATANY"
                    Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG, GRT00005WRKINC.TERM_TYPE.COSMO
                        WW_KEYWORD = "GSHABANSHATANK"
                    Case Else
                        WW_KEYWORD = "GSHABAN"
                End Select
                CodeToCode(WW_KEYWORD, T0005INProw("GSHABAN"), WW_CONVERT, WW_DUMMY)
                T0005INProw("GSHABAN") = WW_CONVERT
            End If

            Dim WW_T4GET As String = "OFF"
            If T0005INProw("HDKBN") = "D" Then
                '○統一車番を取得
                CodeToName("GSHABAN", T0005INProw("GSHABAN"), WW_TEXT, WW_DUMMY)
                Dim WW_SPLIT() As String = WW_TEXT.Split(" ")
                If WW_SPLIT.Length >= 6 Then
                    T0005INProw("SHARYOTYPEF") = WW_SPLIT(0)
                    T0005INProw("TSHABANF") = WW_SPLIT(1)
                    T0005INProw("SHARYOTYPEB") = WW_SPLIT(2)
                    T0005INProw("TSHABANB") = WW_SPLIT(3)
                    T0005INProw("SHARYOTYPEB2") = WW_SPLIT(4)
                    T0005INProw("TSHABANB2") = WW_SPLIT(5)
                End If

                '◆荷積or荷卸の場合のみチェック
                If T0005INProw("WORKKBN") = "B2" OrElse T0005INProw("WORKKBN") = "B3" Then

                    '○取引先
                    Select Case T0005INProw("TERMKBN")
                        Case GRT00005WRKINC.TERM_TYPE.YAZAKI
                            WW_KEYWORD = "TORICODEY"
                        Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG, GRT00005WRKINC.TERM_TYPE.COSMO
                            WW_KEYWORD = "TORICODEK"
                        Case Else
                            WW_KEYWORD = "TORICODE"
                    End Select
                    CodeToCode(WW_KEYWORD, T0005INProw("TORICODE"), WW_CONVERT, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        T0005INProw("TORICODE") = WW_CONVERT
                    End If
                    CodeToName("TORICODE", T0005INProw("TORICODE"), WW_TEXT, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        T0005INProw("TORINAMES") = WW_TEXT
                    End If

                    '荷積の取引先不要
                    If T0005INProw("WORKKBN") = "B2" Then
                        T0005INProw("TORICODE") = ""
                        T0005INProw("TORINAMES") = ""
                    End If

                    '○出荷場所
                    Select Case T0005INProw("TERMKBN")
                        Case GRT00005WRKINC.TERM_TYPE.YAZAKI
                            WW_KEYWORD = "SHUKABASHOY"
                        Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                            WW_KEYWORD = "SHUKABASHO"
                            T0005INProw("SHUKABASHO") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & T0005INProw("SHUKABASHO")
                        Case GRT00005WRKINC.TERM_TYPE.COSMO
                            WW_KEYWORD = "SHUKABASHO"
                            T0005INProw("SHUKABASHO") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO & T0005INProw("SHUKABASHO")
                        Case Else
                            WW_KEYWORD = "SHUKABASHO"
                    End Select
                    CodeToCode(WW_KEYWORD, T0005INProw("SHUKABASHO"), WW_CONVERT, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        T0005INProw("SHUKABASHO") = WW_CONVERT
                    End If
                    CodeToName(WW_KEYWORD, T0005INProw("SHUKABASHO"), WW_TEXT, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        T0005INProw("SHUKABASHONAMES") = WW_TEXT
                    End If
                End If

                '◆荷卸の場合のみチェック
                If T0005INProw("WORKKBN") = "B3" Then
                    '○税区分を取得
                    T0005INProw("TAXKBN") = "0"
                    CodeToName("TAXKBN", T0005INProw("TAXKBN"), WW_TEXT, WW_DUMMY)
                    T0005INProw("TAXKBNNAMES") = WW_TEXT

                    '○届先
                    Select Case T0005INProw("TERMKBN")
                        Case GRT00005WRKINC.TERM_TYPE.YAZAKI
                            WW_KEYWORD = "TODOKECODEY"
                        Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                            WW_KEYWORD = "TODOKECODE"
                            T0005INProw("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & T0005INProw("TODOKECODE")
                        Case GRT00005WRKINC.TERM_TYPE.COSMO
                            WW_KEYWORD = "TODOKECODE"
                            T0005INProw("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO & T0005INProw("TODOKECODE")

                        Case Else
                            WW_KEYWORD = "TODOKECODE"
                    End Select
                    CodeToCode(WW_KEYWORD, T0005INProw("TODOKECODE"), WW_CONVERT, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        T0005INProw("TODOKECODE") = WW_CONVERT
                    End If
                    CodeToName(WW_KEYWORD, T0005INProw("TODOKECODE"), WW_TEXT, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        T0005INProw("TODOKECODE") = WW_CONVERT
                        T0005INProw("TODOKENAMES") = WW_TEXT

                        If T0005INProw("TORICODE") = "" Then
                            '届先マスタより取引先を取得
                            Dim WW_RTN As String = ""
                            GetToriCodeForMC006(T0005INProw("TODOKECODE"), T0005INProw("YMD"), T0005INProw("TORICODE"), WW_RTN)
                            If isNormal(WW_RTN) Then
                                CodeToName("TORICODE", T0005INProw("TORICODE"), WW_TEXT, WW_DUMMY)
                                T0005INProw("TORINAMES") = WW_TEXT
                            Else
                                Exit Sub
                            End If
                        End If

                    End If

                    '○品名１～８
                    For WW_SEQ As Integer = 1 To 8
                        Dim WW_OILTYPE As String = "OILTYPE" & WW_SEQ.ToString("0")
                        Dim WW_PRODUCT1 As String = "PRODUCT1" & WW_SEQ.ToString("0")
                        Dim WW_PRODUCT2 As String = "PRODUCT2" & WW_SEQ.ToString("0")
                        Dim WW_PRODUCTCODE As String = "PRODUCTCODE" & WW_SEQ.ToString("0")
                        Dim WW_PRODUCTNAMES As String = "PRODUCT" & WW_SEQ.ToString("0") & "NAMES"
                        If T0005INProw(WW_PRODUCTCODE) <> "" Then
                            Select Case T0005INProw("TERMKBN")
                                Case GRT00005WRKINC.TERM_TYPE.YAZAKI
                                    WW_KEYWORD = "PRODUCT2Y"
                                Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG, GRT00005WRKINC.TERM_TYPE.COSMO
                                    WW_KEYWORD = "PRODUCT2K"
                                Case GRT00005WRKINC.TERM_TYPE.JOT
                                    WW_KEYWORD = "PRODUCT2KE"
                                Case Else
                                    WW_KEYWORD = "PRODUCT2"
                            End Select
                            CodeToCode(WW_KEYWORD, T0005INProw(WW_PRODUCTCODE), WW_CONVERT, WW_RTN_SW)
                            CodeToName(WW_KEYWORD, T0005INProw(WW_PRODUCTCODE), WW_TEXT, WW_RTN_SW)

                            If isNormal(WW_RTN_SW) Then
                                T0005INProw(WW_OILTYPE) = WW_CONVERT.Substring(2, 2)
                                T0005INProw(WW_PRODUCT1) = WW_CONVERT.Substring(4, 2)
                                T0005INProw(WW_PRODUCT2) = WW_CONVERT.Substring(6)
                                T0005INProw(WW_PRODUCTCODE) = WW_CONVERT
                                T0005INProw(WW_PRODUCTNAMES) = WW_TEXT
                            End If
                        End If
                    Next
                End If
            End If

            'コンテナシャーシ取得
            If T0005INProw("WORKKBN") = "B3" OrElse T0005INProw("WORKKBN") = "B2" Then
                Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
                GetChassisForT0004Tbl(T0005INProw, WW_RTN)
                If Not isNormal(WW_RTN) Then
                    T0005INProw("CONTCHASSIS") = ""
                End If
            End If

            If T0005INProw("WORKKBN") = "B2" Then
                T0005INProw("SHUKADATE") = T0005INProw("STDATE")
            End If

            'B3（荷卸）の出荷日が空白の場合、直前のB2（荷積）の出庫日（＝出荷日）を取得する
            If T0005INProw("WORKKBN") = "B3" Then
                If T0005INProw("SHUKADATE") = "" Then
                    For j As Integer = i To 0 Step -1
                        Dim WW_T0005INProw As DataRow = T0005INPtbl.Rows(j)
                        If WW_T0005INProw("YMD") = T0005INProw("YMD") AndAlso
                           WW_T0005INProw("STAFFCODE") = T0005INProw("STAFFCODE") AndAlso
                           WW_T0005INProw("GSHABAN") = T0005INProw("GSHABAN") AndAlso
                           WW_T0005INProw("WORKKBN") = "B2" Then
                            T0005INProw("SHUKADATE") = WW_T0005INProw("SHUKADATE")
                            Exit For
                        End If
                    Next
                End If
            End If

            '作業区分名
            CodeToName("WORKKBN", T0005INProw("WORKKBN"), WW_TEXT, WW_DUMMY)
            T0005INProw("WORKKBNNAMES") = WW_TEXT

            '端末区分名
            CodeToName("TERMKBN", T0005INProw("TERMKBN"), WW_TEXT, WW_DUMMY)
            T0005INProw("TERMKBNNAMES") = WW_TEXT

            '乗務区分名
            CodeToName("CREWKBN", T0005INProw("CREWKBN"), WW_TEXT, WW_DUMMY)
            T0005INProw("CREWKBNNAMES") = WW_TEXT

            SetUriKbn(T0005INProw, O_RTN)

            CodeToName("URIKBN", T0005INProw("URIKBN"), WW_TEXT, WW_DUMMY)
            T0005INProw("URIKBNNAMES") = WW_TEXT

            WW_T0005INPtbl.Rows.Add(T0005INProw)
        Next

        T0005INPtbl = WW_T0005INPtbl.Copy

        '------------------------------------
        '未設定項目の救済準備
        '------------------------------------
        '○テーブルデータ 復元（一週間前データ）
        If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

        '入力データ＋一週間前データをマージ
        T0005INPtbl.Merge(T0005WEEKtbl)

        '入力データ＋１週間前
        CS0026TBLSORT.TABLE = T0005INPtbl
        CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        CS0026TBLSORT.FILTER = ""
        T0005INPtbl = CS0026TBLSORT.sort()

        'トリップ判定・回送判定・出荷日内荷積荷卸回数判定
        T0005COM.ReEditT0005(T0005INPtbl, WW_DUMMY)

        '入力データ、１週間前の分離
        CS0026TBLSORT.TABLE = T0005INPtbl
        CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        CS0026TBLSORT.FILTER = "YMD < #" & work.WF_SEL_STYMD.Text & "#"
        T0005WEEKtbl = CS0026TBLSORT.sort()
        CS0026TBLSORT.FILTER = "YMD >= #" & work.WF_SEL_STYMD.Text & "#"
        T0005INPtbl = CS0026TBLSORT.sort()

        WW_T0005INPtbl.Clear()
        For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
            Dim T0005INProw As DataRow = WW_T0005INPtbl.NewRow
            T0005INProw.ItemArray = T0005INPtbl.Rows(i).ItemArray
            '------------------------------------
            '未設定項目の救済
            '------------------------------------
            If T0005INProw("WORKKBN") = "B2" Then
                '出荷場所
                If T0005INProw("SHUKABASHO") = "" Then
                    T0005INProw("SHUKABASHO") = T0005INProw("wSHUKABASHO")
                    If T0005INProw("SHUKABASHO") = "" Then
                        '積置の場合、出荷場所が設定されないため受注DB（積置）を検索し、救済
                        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
                        GetShukaBasho(T0005INProw, WW_RTN)
                        If Not isNormal(WW_RTN) Then
                            T0005INProw("SHUKABASHO") = ""
                        End If
                        'NJSの場合、最終的に届先部署マスタを検索し救済
                        If T0005INProw("SHUKABASHO") = "" And T0005INProw("CAMPCODE") = GRT00005WRKINC.C_COMP_NJS Then
                            GetShukaBashoNJS(T0005INProw, WW_RTN)
                            If Not isNormal(WW_RTN) Then
                                T0005INProw("SHUKABASHO") = ""
                            End If
                        End If
                    End If
                End If

                '出荷日
                If T0005INProw("SHUKADATE") = "" Then
                    T0005INProw("SHUKADATE") = T0005INProw("wSHUKADATE")
                End If

            End If

            If T0005INProw("WORKKBN") = "B3" Then
                '取引先
                If T0005INProw("TORICODE") = "" Then
                    T0005INProw("TORICODE") = T0005INProw("wTORICODE")
                End If

                '請求取引先
                If T0005INProw("STORICODE") = "" Then
                    T0005INProw("STORICODE") = T0005INProw("wSTORICODE")
                End If

                '出荷場所
                If T0005INProw("SHUKABASHO") = "" Then
                    T0005INProw("SHUKABASHO") = T0005INProw("wSHUKABASHO")
                End If
                'NJSの場合、最終的に届先部署マスタを検索し救済
                Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
                If T0005INProw("SHUKABASHO") = "" And T0005INProw("CAMPCODE") = GRT00005WRKINC.C_COMP_NJS Then
                    GetShukaBashoNJS(T0005INProw, WW_RTN)
                    If Not isNormal(WW_RTN) Then
                        T0005INProw("SHUKABASHO") = ""
                    End If
                End If

                '届先
                If T0005INProw("TODOKECODE") = "" Then
                    T0005INProw("TODOKECODE") = T0005INProw("wTODOKECODE")
                End If

                '出荷日
                If T0005INProw("SHUKADATE") = "" Then
                    T0005INProw("SHUKADATE") = T0005INProw("wSHUKADATE")
                End If

                '届日
                If T0005INProw("TODOKEDATE") = "" Then
                    T0005INProw("TODOKEDATE") = T0005INProw("wTODOKEDATE")
                End If

            End If
            WW_T0005INPtbl.Rows.Add(T0005INProw)
        Next

        T0005INPtbl = WW_T0005INPtbl.Copy

        WW_T0005INPtbl.Dispose()
        WW_T0005INPtbl = Nothing
    End Sub

    ''' <summary>
    ''' GridViewの更新
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub UpdateGridData(ByRef O_RTN As String)

        Dim WW_UMU As Integer = 0

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '○テーブルデータ 復元（GridView）
            If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then
                O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                Exit Sub
            End If

            '○テーブルデータ 復元（一週間前データ）
            If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then
                O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                Exit Sub
            End If

            '入力データから日付、乗務員のキー情報（集約）を作成
            Dim WW_Cols As String() = {"YMD", "STAFFCODE", "NIPPONO"}
            Dim WW_KEYtbl As DataTable
            Dim WW_TBLview As DataView

            WW_TBLview = New DataView(T0005INPtbl)
            WW_TBLview.Sort = "YMD, STAFFCODE, NIPPONO"
            WW_TBLview.RowFilter = "HDKBN = 'D'"
            WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)
            WW_TBLview.Dispose()
            WW_TBLview = Nothing

            'GridView（日報ＤＢから取得）したデータより更新対象データを抽出
            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, NIPPONO, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "SELECT = '1' and HDKBN = 'D'"
            Dim WW_T0005tbl As DataTable = CS0026TBLSORT.sort()
            CS0026TBLSORT.FILTER = "SELECT = '0' "
            T0005tbl = CS0026TBLSORT.sort()

            '-----------------------------------------------------------------------------------
            '日付、乗務員単位に、入力データ（キー）とGridViewをマッチングし削除、追加する
            '-----------------------------------------------------------------------------------
            Dim WW_IDX As Integer = 0
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                For i As Integer = WW_IDX To WW_T0005tbl.Rows.Count - 1
                    Dim T0005row As DataRow = WW_T0005tbl.Rows(i)
                    If T0005row("YMD") & T0005row("STAFFCODE") & T0005row("NIPPONO") > WW_KEYrow("YMD") & WW_KEYrow("STAFFCODE") & WW_KEYrow("NIPPONO") Then
                        WW_IDX = i
                        Exit For
                    End If
                    'マッチしたら現在のレコードを削除対象とする（後で、入力データを追加するため）
                    If T0005row("YMD") = WW_KEYrow("YMD") AndAlso
                       T0005row("STAFFCODE") = WW_KEYrow("STAFFCODE") AndAlso
                       (T0005row("NIPPONO") = WW_KEYrow("NIPPONO") OrElse T0005row("NIPPONO") = "") Then
                        T0005row("LINECNT") = "0"
                        T0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        T0005row("SELECT") = 0
                        T0005row("HIDDEN") = 1
                        T0005row("DELFLG") = C_DELETE_FLG.DELETE
                    End If
                Next
            Next

            '対象外データをマージ
            T0005tbl.Merge(WW_T0005tbl)

            '日報ＤＢに未登録（タイムスタンプなし）のデータは物理削除
            For i As Integer = T0005tbl.Rows.Count - 1 To 0 Step -1
                Dim WW_T0005row As DataRow = T0005tbl.Rows(i)
                If WW_T0005row("TIMSTP") = 0 AndAlso
                   WW_T0005row("DELFLG") = C_DELETE_FLG.DELETE Then
                    WW_T0005row.Delete()
                End If
            Next

            '入力データのマージ
            T0005tbl.Merge(T0005INPtbl)

            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            T0005tbl = CS0026TBLSORT.sort()
            '------------------------------------------------------------
            '■出庫日、従業員 単位
            '  取込んだ日報番号毎のヘッダを出庫日、従業員毎に集約し直す
            '------------------------------------------------------------
            CreateT0005Header(T0005tbl)

            '削除データにヘッダを付加する（DB更新処理で必要なため）
            WW_T0005tbl = T0005tbl.Copy
            CreateT0005HeaderDummy(WW_T0005tbl)

            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "SELECT"
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            T0005tbl = CS0026TBLSORT.sort()

            T0005tbl.Merge(WW_T0005tbl)

            '------------------------------------------------------------
            '■関連チェックのチェック
            '------------------------------------------------------------
            Dim WW_Cols2 As String() = {"YMD", "STAFFCODE"}
            Dim WW_TBLview2 As DataView
            Dim WW_T0005SVtbl As DataTable = T0005tbl.Clone
            Dim WW_T0005WKtbl As DataTable = T0005tbl.Clone
            Dim WW_T0005INPtbl As DataTable = T0005tbl.Clone

            '削除データ抽出保存
            WW_TBLview2 = New DataView(T0005tbl)
            WW_TBLview2.Sort = "YMD, STAFFCODE"
            WW_TBLview2.RowFilter = "SELECT = '0'"
            WW_T0005SVtbl = WW_TBLview2.ToTable
            'チェック対象データ抽出（キー）
            WW_TBLview = New DataView(T0005tbl)
            WW_TBLview.Sort = "YMD, STAFFCODE"
            WW_TBLview.RowFilter = "SELECT = '1'"
            WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols2)
            '従業員、タイトル区分（ヘッダ、明細）、開始時刻の順番で処理する
            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            WW_T0005tbl = CS0026TBLSORT.sort()

            'ヘッダレコードが存在する場合、チェックを行う
            WW_IDX = 0
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                WW_T0005INPtbl.Clear()
                '同一の日報、従業員を抽出（ワークテーブルに保存）
                For i As Integer = WW_IDX To WW_T0005tbl.Rows.Count - 1
                    Dim T0005row As DataRow = WW_T0005tbl.Rows(i)
                    If T0005row("YMD") & T0005row("STAFFCODE") > WW_KEYrow("YMD") & WW_KEYrow("STAFFCODE") Then
                        WW_IDX = i
                        Exit For
                    End If
                    If WW_KEYrow("YMD") = T0005row("YMD") AndAlso
                       WW_KEYrow("STAFFCODE") = T0005row("STAFFCODE") Then
                        Dim T0005INProw As DataRow = WW_T0005INPtbl.NewRow
                        T0005INProw.ItemArray = T0005row.ItemArray
                        WW_T0005INPtbl.Rows.Add(T0005INProw)
                    End If
                Next

                '同一の日報、従業員毎にチェックを行う
                WW_ERRLIST = New List(Of String)

                CheckAllT0005tbl(WW_T0005INPtbl, O_RTN)

                WW_T0005WKtbl.Merge(WW_T0005INPtbl)

            Next

            T0005tbl.Clear()
            T0005tbl.Merge(WW_T0005WKtbl)
            T0005tbl.Merge(WW_T0005SVtbl)

            WW_T0005INPtbl.Dispose()
            WW_T0005INPtbl = Nothing
            WW_T0005WKtbl.Dispose()
            WW_T0005WKtbl = Nothing
            WW_T0005SVtbl.Dispose()
            WW_T0005SVtbl = Nothing
            WW_TBLview.Dispose()
            WW_TBLview = Nothing
            WW_TBLview2.Dispose()
            WW_TBLview2 = Nothing
            WW_KEYtbl.Dispose()
            WW_KEYtbl = Nothing

            '------------------------------------------------------------
            '■マージ後のチェック
            '------------------------------------------------------------
            CheckOrderData(T0005tbl, O_RTN)
            If Not isNormal(O_RTN) Then Exit Sub

            Dim WW_SEQ As Integer = 0
            Dim WW_LINECNT As Integer = 0

            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            T0005tbl = CS0026TBLSORT.sort()

            '行番号の採番
            For Each WW_T0005row As DataRow In T0005tbl.Rows
                If WW_T0005row("SELECT") = "1" Then
                    If WW_T0005row("HDKBN") = "H" Then
                        WW_SEQ = 1
                        WW_T0005row("SEQ") = WW_SEQ.ToString("000")

                        WW_LINECNT = WW_LINECNT + 1
                        WW_T0005row("LINECNT") = WW_LINECNT
                        WW_T0005row("SELECT") = "1"
                        WW_T0005row("HIDDEN") = "0"
                    Else
                        WW_T0005row("SEQ") = WW_SEQ.ToString("000")
                        WW_SEQ = WW_SEQ + 1

                        WW_T0005row("LINECNT") = 0
                        WW_T0005row("SELECT") = "1"
                        WW_T0005row("HIDDEN") = "1"
                    End If
                End If
            Next

            '明細にエラーがある場合、ヘッダにエラーを設定する
            Dim WW_HeadIdx As Integer = 0
            Dim WW_ERR_FLG As Boolean = False
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                Dim WW_T0005row As DataRow = T0005tbl.Rows(i)
                If WW_T0005row("HDKBN") = "H" Then
                    WW_ERR_FLG = False
                    WW_HeadIdx = i
                End If
                '次のヘッダまで
                For j As Integer = i + 1 To T0005tbl.Rows.Count - 1
                    Dim WW_T0005rowj As DataRow = T0005tbl.Rows(j)
                    If WW_T0005rowj("HDKBN") = "H" Then
                        i = j - 1
                        Exit For
                    End If
                    If WW_T0005rowj("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                        WW_ERR_FLG = True
                        Exit For
                    End If
                Next
                If WW_ERR_FLG = True Then
                    T0005tbl.Rows(WW_HeadIdx)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            Next

            'ヘッダが更新の場合、明細に更新を設定する
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                Dim WW_T0005row As DataRow = T0005tbl.Rows(i)
                If WW_T0005row("SELECT") = "1" Then
                    If WW_T0005row("HDKBN") = "H" Then
                        '次のヘッダまで
                        For j As Integer = i + 1 To T0005tbl.Rows.Count - 1
                            Dim WW_T0005rowj As DataRow = T0005tbl.Rows(j)
                            If WW_T0005rowj("HDKBN") = "H" Then
                                i = j - 1
                                Exit For
                            End If
                            WW_T0005rowj("OPERATION") = WW_T0005row("OPERATION")
                        Next
                    End If
                End If
            Next

            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN"
            CS0026TBLSORT.FILTER = "HDKBN = 'H' and SELECT = '1'"
            Dim WW_T0005ERRtbl As DataTable = CS0026TBLSORT.sort()

            'エラー行番号表示のためのキー設定（出庫日、乗務員）
            For Each WW_T0005row As DataRow In WW_T0005ERRtbl.Rows
                Dim WW_ERRWORD As String = ""
                WW_ERRWORD = rightview.GetErrorReport.Replace("@L" & WW_T0005row("YMD") & WW_T0005row("STAFFCODE") & "L@", WW_T0005row("LINECNT"))
                rightview.SetErrorReport(WW_ERRWORD)
            Next
            WW_T0005ERRtbl.Dispose()
            WW_T0005ERRtbl = Nothing

            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            T0005tbl = CS0026TBLSORT.sort()

            '○GridViewデータをテーブルに保存
            If Not Master.SaveTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
            '○GridViewデータをテーブルに保存（一週間前データ）
            If Not Master.SaveTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
            '絞込みボタン処理（GridViewの表示）を行う
            WF_ButtonExtract_Click()


        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "Grid_Update")
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

    ''' <summary>
    '''  T0005tbl編集（明細行番号の振り直し及び、取引先、出荷場所、届先の編集）
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub EditT0005Tbl(ByRef O_RTN As String)

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '---------------------------------------------------
            '■日報単位
            '  積置判定＆トリップ、ドロップ設定
            '---------------------------------------------------
            Dim WW_F1 As Integer = 0            '出庫
            Dim WW_B2 As Integer = 0            '積荷
            Dim WW_B3 As Integer = 0            '荷卸
            Dim WW_B2POS As Integer = 0         '積荷
            Dim WW_B3POS As Integer = 0         '荷卸
            Dim WW_TWOMANTRIP As Integer = 0
            Dim WW_TRIP As Integer = 0
            Dim WW_DROP As Integer = 0
            Dim WW_HeadIdx As Integer = 0
            Dim WW_RUIDISTANCE As Double = 0
            Dim WW_DATE As Date = Nothing
            Dim WW_YMD As String = ""
            Dim WW_CONVERT As String = ""
            Dim WW_LINEerr As String = ""
            Dim WW_TEXT As String = ""

            CS0026TBLSORT.TABLE = T0005INPtbl
            CS0026TBLSORT.SORTING = "YMD, NIPPONO, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            T0005INPtbl = CS0026TBLSORT.sort()


            '-----------------------------------------------
            '○業務車番変換と水素車の判定
            '　※水素車はエラー、電源車ならＯＫ
            Dim WW_T0005INPtbl As DataTable = T0005INPtbl.Clone
            Dim T0005INProw As DataRow = Nothing
            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                T0005INProw = WW_T0005INPtbl.NewRow
                T0005INProw.ItemArray = T0005INPtbl.Rows(i).ItemArray

                Dim WW_KEY As String = ""
                Select Case T0005INProw("TERMKBN")
                    Case GRT00005WRKINC.TERM_TYPE.YAZAKI
                        WW_KEY = "GSHABANSHATANY"
                    Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG, GRT00005WRKINC.TERM_TYPE.COSMO
                        WW_KEY = "GSHABANSHATANK"
                    Case Else
                        WW_KEY = "GSHABAN"
                End Select

                CodeToCode(WW_KEY, T0005INProw("GSHABAN"), WW_CONVERT, WW_RTN_SW)
                If isNormal(WW_RTN_SW) Then
                    Dim WW_WORK As String = ""
                    CodeToName("SUISOKBN", WW_CONVERT, WW_TEXT, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        '存在したら水素車
                        Dim WW_ERR_MES1 As String = ""
                        Dim WW_ERR_MES2 As String = ""
                        WW_ERR_MES1 = "・エラーが存在します。(水素車エラー)"
                        WW_ERR_MES2 = "水素車の日報は取り込めません。(" & T0005INProw("GSHABAN") & ") "
                        OutputErrorMessage(WW_ERR_MES1, WW_ERR_MES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                        T0005INProw("OPERATION") = "水素車"
                    End If
                End If
                WW_T0005INPtbl.Rows.Add(T0005INProw)
            Next
            T0005INPtbl = WW_T0005INPtbl.Copy

            WW_T0005INPtbl.Clear()

            '水素車を除外する
            CS0026TBLSORT.TABLE = T0005INPtbl
            CS0026TBLSORT.SORTING = "YMD, NIPPONO, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "OPERATION <> '水素車'"
            T0005INPtbl = CS0026TBLSORT.sort()

            Dim KANSAIBKIN As New ListBox
            Dim BKIN As New ListBox

            GS0007FIXVALUElst.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            GS0007FIXVALUElst.CLAS = "T00005_KANSAIBKIN"
            GS0007FIXVALUElst.LISTBOX1 = KANSAIBKIN
            GS0007FIXVALUElst.GS0007FIXVALUElst()
            If isNormal(GS0007FIXVALUElst.ERR) Then
                KANSAIBKIN = GS0007FIXVALUElst.LISTBOX1
            Else
                Master.Output(GS0007FIXVALUElst.ERR, C_MESSAGE_TYPE.ABORT)
                O_RTN = GS0007FIXVALUElst.ERR
                Exit Sub
            End If

            '○　B勤務開始ListBox設定              
            GS0007FIXVALUElst.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            GS0007FIXVALUElst.CLAS = "T00005_BKIN"
            GS0007FIXVALUElst.LISTBOX1 = BKIN
            GS0007FIXVALUElst.GS0007FIXVALUElst()
            If isNormal(GS0007FIXVALUElst.ERR) Then
                BKIN = GS0007FIXVALUElst.LISTBOX1
            Else
                Master.Output(GS0007FIXVALUElst.ERR, C_MESSAGE_TYPE.ABORT)
                O_RTN = GS0007FIXVALUElst.ERR
                Exit Sub
            End If


            Dim WW_T0005INProw As DataRow = Nothing
            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                WW_T0005INProw = WW_T0005INPtbl.NewRow
                WW_T0005INProw.ItemArray = T0005INPtbl.Rows(i).ItemArray
                If WW_T0005INProw("HDKBN") = "H" Then
                    'ヘッダは、日報毎
                    '〇指定の時間以降は翌日扱いにする。
                    '  指定の時間が未設定の場合は処理対象外とする
                    Dim WW_BKINST As String = String.Empty
                    '22時00分以降は翌日の日報とする（関西以外）
                    If KANSAIBKIN.Items.Count > 0 AndAlso Not IsNothing(KANSAIBKIN.Items.FindByValue(WW_T0005INProw("SHIPORG"))) Then
                        '関西（ENEX)は別の指定時間（十九時半予定）での処理を行う
                        WW_BKINST = KANSAIBKIN.Items.FindByValue(WW_T0005INProw("SHIPORG")).Text
                    Else
                        If BKIN.Items.Count > 0 Then WW_BKINST = BKIN.Items(0).Text
                    End If
                    If Not String.IsNullOrEmpty(WW_BKINST) AndAlso WW_T0005INProw("STTIME") >= WW_BKINST Then
                        WW_DATE = CDate(WW_T0005INProw("YMD"))
                        WW_T0005INProw("YMD") = WW_DATE.AddDays(1).ToString("yyyy/MM/dd")
                        WW_YMD = WW_T0005INProw("YMD")
                    Else
                        WW_YMD = WW_T0005INProw("YMD")
                    End If

                    WW_HeadIdx = i
                    WW_TWOMANTRIP = 1
                    WW_TRIP = 0
                    WW_DROP = 0
                    WW_F1 = 0
                    WW_B2 = 0
                    WW_B3 = 0
                    WW_B2POS = 0
                    WW_B3POS = 0
                    WW_RUIDISTANCE = 0
                    Dim WW_T0005INProwj As DataRow = Nothing
                    For j As Integer = i + 1 To T0005INPtbl.Rows.Count - 1
                        WW_T0005INProwj = WW_T0005INPtbl.NewRow
                        WW_T0005INProwj.ItemArray = T0005INPtbl.Rows(j).ItemArray
                        '次のヘッダ（別の出庫日、乗務員、日報）が現れるまで
                        If WW_T0005INProwj("HDKBN") = "H" Then
                            i = j - 1
                            Exit For
                        End If

                        '19時30分以降は翌日の日報とする（ヘッダの日付を設定）
                        WW_T0005INProwj("YMD") = WW_YMD

                        '--------------------------
                        '開始、終了メータ設定
                        '--------------------------
                        If WW_T0005INProwj("WORKKBN") = "F1" Then
                            WW_F1 += 1
                            WW_T0005INProwj("STMATER") = WW_T0005INProw("STMATER")
                            If Val(WW_T0005INProwj("RUIDISTANCE")) = 0 Then
                                WW_T0005INProwj("RUIDISTANCE") = WW_T0005INProw("STMATER")
                                WW_RUIDISTANCE = Val(WW_T0005INProw("STMATER").replace(",", ""))
                            End If

                            If WW_F1 > 1 Then
                                '出庫が出現するたびに（複数回の場合の考慮）２マン用トリップをクリアする
                                WW_TWOMANTRIP = 1

                                '矢崎の場合、出庫が出現するたびに（複数回の場合の考慮）トリップをクリアする
                                If WW_T0005INProwj("TERMKBN") = GRT00005WRKINC.TERM_TYPE.YAZAKI Then
                                    WW_TRIP = 0
                                End If
                            End If
                        End If
                        If WW_T0005INProwj("WORKKBN") = "F3" Then
                            WW_T0005INProwj("ENDMATER") = WW_T0005INProw("ENDMATER")
                            If Val(WW_T0005INProwj("RUIDISTANCE")) = 0 Then
                                WW_T0005INProwj("RUIDISTANCE") = WW_T0005INProw("ENDMATER")
                            End If
                            WW_T0005INProwj("PRATE") = WW_T0005INProw("PRATE")
                            WW_T0005INProwj("CASH") = WW_T0005INProw("CASH")
                            WW_T0005INProwj("TICKET") = WW_T0005INProw("TICKET")
                            WW_T0005INProwj("ETC") = WW_T0005INProw("ETC")
                            WW_T0005INProwj("TOTALTOLL") = WW_T0005INProw("TOTALTOLL")
                            WW_T0005INProwj("SOUDISTANCE") = WW_T0005INProw("SOUDISTANCE")
                            WW_T0005INProwj("JIDISTANCE") = WW_T0005INProw("JIDISTANCE")
                            WW_T0005INProwj("KUDISTANCE") = WW_T0005INProw("KUDISTANCE")
                            WW_T0005INProwj("IPPDISTANCE") = WW_T0005INProw("IPPDISTANCE")
                            WW_T0005INProwj("KOSDISTANCE") = WW_T0005INProw("KOSDISTANCE")
                            WW_T0005INProwj("IPPJIDISTANCE") = WW_T0005INProw("IPPJIDISTANCE")
                            WW_T0005INProwj("IPPKUDISTANCE") = WW_T0005INProw("IPPKUDISTANCE")
                            WW_T0005INProwj("KOSJIDISTANCE") = WW_T0005INProw("KOSJIDISTANCE")
                            WW_T0005INProwj("KOSKUDISTANCE") = WW_T0005INProw("KOSKUDISTANCE")
                        End If
                        If WW_T0005INProwj("WORKKBN") = "B3" Then
                            WW_T0005INProwj("TODOKEDATE") = WW_T0005INProwj("STDATE")
                        End If

                        If WW_T0005INProwj("WORKKBN") = "B2" Then
                            WW_T0005INProwj("SHUKADATE") = WW_T0005INProwj("STDATE")
                        End If
                        '--------------------------
                        '積置判定
                        '--------------------------
                        '荷卸
                        If WW_T0005INProwj("WORKKBN") = "B3" Then
                            WW_B3 = j
                        End If
                        '積荷
                        If WW_T0005INProwj("WORKKBN") = "B2" Then
                            WW_B2 = j
                        End If

                        '---------------------------------------------------------------------
                        '日付、日報番号毎にトリップ＆ドロップ設定
                        ' ※車端取込み時は日付、日報番号毎にH（ヘッダ）が作成されている
                        '---------------------------------------------------------------------
                        '光英の場合、車端のトリップを使う、ドロップは付番
                        If WW_T0005INProwj("TERMKBN") = GRT00005WRKINC.TERM_TYPE.JX OrElse
                           WW_T0005INProwj("TERMKBN") = GRT00005WRKINC.TERM_TYPE.TG OrElse
                           WW_T0005INProwj("TERMKBN") = GRT00005WRKINC.TERM_TYPE.COSMO OrElse
                           WW_T0005INProwj("TERMKBN") = GRT00005WRKINC.TERM_TYPE.JOT Then
                            If WW_T0005INProwj("WORKKBN") = "B2" Then
                                WW_TRIP = WW_T0005INProwj("TRIPNO")
                                WW_DROP = 0
                            End If

                            If WW_T0005INProwj("WORKKBN") = "B3" Then
                                If WW_TRIP = 0 Then
                                    WW_TRIP = WW_T0005INProwj("TRIPNO")
                                End If
                                If WW_TRIP = Val(WW_T0005INProwj("TRIPNO")) Then
                                    WW_DROP = WW_DROP + 1
                                    WW_T0005INProwj("DROPNO") = WW_DROP.ToString("000")
                                Else
                                    WW_DROP = 1
                                    WW_T0005INProwj("DROPNO") = WW_DROP.ToString("000")
                                End If
                            End If
                        End If

                        '------------------------------------
                        '累積走行距離を設定
                        '------------------------------------
                        If Val(WW_T0005INProwj("RUIDISTANCE")) = 0 Then
                            WW_RUIDISTANCE += Val(WW_T0005INProwj("SOUDISTANCE").replace(",", ""))
                            WW_T0005INProwj("RUIDISTANCE") = WW_RUIDISTANCE.ToString("#,0.00")
                        End If

                        '------------------------------------
                        '２マン用トリップ単位を設定
                        '------------------------------------
                        WW_T0005INProwj("TWOMANTRIP") = WW_TWOMANTRIP

                        If WW_T0005INProwj("WORKKBN") = "B2" Then
                            WW_B2POS = j '最後のB2のポジション

                            '荷積の前に荷卸があったら次のトリップ
                            If WW_B2POS > WW_B3POS Then
                                If WW_B3POS = 0 Then
                                    '初めての荷積は次の荷積まで同一トリップ（加算しない）
                                Else
                                    WW_TWOMANTRIP += 1
                                    WW_T0005INProwj("TWOMANTRIP") = WW_TWOMANTRIP
                                End If
                            End If
                        End If

                        If WW_T0005INProwj("WORKKBN") = "B3" Then
                            WW_B3POS = j '最初のB3のポジション
                        End If
                        WW_T0005INPtbl.Rows.Add(WW_T0005INProwj)
                    Next

                End If

            Next

            T0005INPtbl = WW_T0005INPtbl.Copy

            WW_T0005INPtbl.Clear()

            Dim OLD_SHIPORG As String = ""
            Dim OLD_YMD As String = ""
            Dim OLD_GSHABAN As String = ""

            CS0026TBLSORT.TABLE = T0005INPtbl
            CS0026TBLSORT.SORTING = "SHIPORG, YMD, GSHABAN, STDATE, STTIME, ENDDATE, ENDTIME"
            CS0026TBLSORT.FILTER = ""
            T0005INPtbl = CS0026TBLSORT.sort()

            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                WW_T0005INProw = WW_T0005INPtbl.NewRow
                WW_T0005INProw.ItemArray = T0005INPtbl.Rows(i).ItemArray

                '矢崎の場合、トリップ、ドロップを付番
                If WW_T0005INProw("TERMKBN") = GRT00005WRKINC.TERM_TYPE.YAZAKI AndAlso WW_T0005INProw("HDKBN") = "D" Then
                    If WW_T0005INProw("SHIPORG") = OLD_SHIPORG AndAlso
                       WW_T0005INProw("YMD") = OLD_YMD AndAlso
                       WW_T0005INProw("GSHABAN") = OLD_GSHABAN Then
                    Else
                        WW_TRIP = 0
                        WW_DROP = 0
                    End If

                    If WW_T0005INProw("WORKKBN") = "B2" Then
                        WW_TRIP = WW_TRIP + 1
                        WW_T0005INProw("TRIPNO") = WW_TRIP.ToString("000")
                        WW_T0005INProw("DROPNO") = ""
                        WW_DROP = 0
                    End If

                    If WW_T0005INProw("WORKKBN") = "B3" Then
                        If WW_TRIP = 0 Then
                            WW_TRIP = WW_TRIP + 1
                        End If
                        WW_DROP = WW_DROP + 1
                        WW_T0005INProw("TRIPNO") = WW_TRIP.ToString("000")
                        WW_T0005INProw("DROPNO") = WW_DROP.ToString("000")
                    End If

                    OLD_SHIPORG = WW_T0005INProw("SHIPORG")
                    OLD_YMD = WW_T0005INProw("YMD")
                    OLD_GSHABAN = WW_T0005INProw("GSHABAN")
                End If
                WW_T0005INPtbl.Rows.Add(WW_T0005INProw)
            Next

            T0005INPtbl = WW_T0005INPtbl.Copy

            WW_T0005INPtbl.Clear()

            '-----------------------------------------------------------------------
            '出庫（F1)＝出社（A1）で５時前の出庫の場合、出社（A1）を５時とする
            '出庫（F1)<>出社（A1）の場合は、５分単位のチェックを行う
            '-----------------------------------------------------------------------
            Dim WW_A1cnt As Integer = 0
            Dim WW_NIPPONO As String = ""
            Dim WW_STAFFCODE As String = ""
            Dim WW_STTIME As String = ""
            WW_YMD = ""

            CS0026TBLSORT.TABLE = T0005INPtbl
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, WORKKBN DESC, STDATE, STTIME"
            CS0026TBLSORT.FILTER = "WORKKBN = 'A1' or WORKKBN = 'F1'"
            Dim WW_A1F1tbl As DataTable = CS0026TBLSORT.sort()
            CS0026TBLSORT.FILTER = "WORKKBN <> 'A1' and WORKKBN <> 'F1'"
            T0005INPtbl = CS0026TBLSORT.sort()

            If work.WF_SEL_CAMPCODE.Text = GRT00005WRKINC.C_COMP_ENEX Then
                '---------------------------------------------------------------------
                'ENEXの場合のみ
                '---------------------------------------------------------------------

                '〇ENEXの規定始業時間
                Const START_TIME As String = "05:00"
                For Each WW_A1F1row As DataRow In WW_A1F1tbl.Rows
                    If WW_A1F1row("WORKKBN") = "F1" Then
                        WW_STTIME = WW_A1F1row("STTIME")
                        WW_A1cnt = 0
                    End If

                    If WW_A1F1row("WORKKBN") = "A1" Then
                        '日付と社員コードが一致したらインクリメント
                        If WW_YMD = WW_A1F1row("YMD") AndAlso
                           WW_STAFFCODE = WW_A1F1row("STAFFCODE") Then WW_A1cnt += 1

                        '先頭のA1のみ対象とする（複数あり）
                        If WW_A1cnt = 1 Then
                            If WW_YMD = WW_A1F1row("YMD") AndAlso
                               WW_STAFFCODE = WW_A1F1row("STAFFCODE") Then

                                If WW_A1F1row("CTRL") = "OFF" Then

                                    Select Case WW_A1F1row("TERMKBN")
                                        '矢崎・手入力・光英（JOT）は、Ｔ４を取得
                                        Case GRT00005WRKINC.TERM_TYPE.YAZAKI, GRT00005WRKINC.TERM_TYPE.JOT, GRT00005WRKINC.TERM_TYPE.HAND
                                            WW_A1F1row("CTRL") = "ON1"
                                            '救済（T4から取得してみる）
                                            Dim WW_T4STTIME As String = String.Empty
                                            GetSTTimeForT0004(WW_A1F1row, WW_T4STTIME, O_RTN)
                                            If Not isNormal(O_RTN) Then Exit Sub

                                            If Not String.IsNullOrEmpty(WW_T4STTIME) Then
                                                'Ｔ４の指定時刻を設定
                                                WW_A1F1row("STTIME") = WW_T4STTIME
                                                WW_A1F1row("ENDTIME") = WW_T4STTIME
                                            ElseIf WW_STTIME < START_TIME Then
                                                'Ｔ４が見つからなくて、出庫が５時前の場合は始業を５時とする
                                                '５時前の場合、５時を設定しフラグOFF（５分単位チェックの判定で使用する）
                                                WW_A1F1row("STTIME") = START_TIME
                                                WW_A1F1row("ENDTIME") = START_TIME
                                                WW_A1F1row("CTRL") = "OFF"
                                            End If
                                        Case Else  '光英（JX,TG,COSMO）
                                            '出社指定時刻の指定なしの場合（光英の場合は、常に指定なし（OFF）としているが
                                            '始業・終業レコードに出社指定時刻が設定されているはず)
                                            If WW_STTIME = WW_A1F1row("STTIME") Then
                                                If WW_STTIME < START_TIME Then
                                                    '５時前の場合、５時を設定しフラグOFF（５分単位チェックの判定で使用する）
                                                    WW_A1F1row("STTIME") = START_TIME
                                                    WW_A1F1row("ENDTIME") = START_TIME
                                                    WW_A1F1row("CTRL") = "OFF"
                                                Else
                                                    '５時以降はそのまま、フラグをON（５分単位チェックの判定で使用する）
                                                    WW_A1F1row("CTRL") = "ON2"
                                                End If
                                            Else
                                                '出庫日<>始業→５分単位チェック
                                                WW_A1F1row("CTRL") = "ON1"
                                            End If
                                    End Select
                                Else
                                    '出社指定時刻の指定ありの場合、チェックしない（5分単位になっているため）
                                    WW_A1F1row("CTRL") = "OFF"
                                End If
                            End If
                        Else
                            '途中のA1は、チェックしない（出庫→帰庫→出庫→帰庫の場合、A1が複数発生する）
                            WW_A1F1row("CTRL") = "OFF"
                        End If
                    End If

                    WW_YMD = WW_A1F1row("YMD")
                    WW_NIPPONO = WW_A1F1row("NIPPONO")
                    WW_STAFFCODE = WW_A1F1row("STAFFCODE")
                Next
            ElseIf work.WF_SEL_CAMPCODE.Text = GRT00005WRKINC.C_COMP_NJS Then
                '---------------------------------------------------------------------
                'NJSの場合のみ
                '---------------------------------------------------------------------
                For Each WW_A1F1row As DataRow In WW_A1F1tbl.Rows

                    If WW_A1F1row("WORKKBN") = "F1" Then
                        WW_STTIME = WW_A1F1row("STTIME")
                        WW_A1cnt = 0
                    End If

                    If WW_A1F1row("WORKKBN") = "A1" Then
                        If WW_YMD = WW_A1F1row("YMD") AndAlso
                           WW_STAFFCODE = WW_A1F1row("STAFFCODE") Then WW_A1cnt += 1

                        '先頭のA1のみ対象とする（複数あり）
                        If WW_A1cnt = 1 Then
                            If WW_YMD = WW_A1F1row("YMD") AndAlso
                               WW_STAFFCODE = WW_A1F1row("STAFFCODE") Then

                                If WW_A1F1row("CTRL") = "OFF" Then
                                    Select Case WW_A1F1row("TERMKBN")
                                        '矢崎・手作業・光英（JOT)は、Ｔ４を取得
                                        Case GRT00005WRKINC.TERM_TYPE.JOT, GRT00005WRKINC.TERM_TYPE.YAZAKI, GRT00005WRKINC.TERM_TYPE.HAND
                                            WW_A1F1row("CTRL") = "OFF"
                                            '救済（T4から取得してみる）
                                            Dim WW_T4STTIME As String = ""
                                            GetSTTimeForT0004(WW_A1F1row, WW_T4STTIME, O_RTN)
                                            If Not isNormal(O_RTN) Then Exit Sub

                                            If WW_T4STTIME <> "" Then
                                                'Ｔ４の指定時刻を設定
                                                WW_A1F1row("STTIME") = WW_T4STTIME
                                                WW_A1F1row("ENDTIME") = WW_T4STTIME
                                            End If
                                    End Select
                                Else
                                    '出社指定時刻の指定ありの場合、チェックしない（5分単位になっているため）
                                    WW_A1F1row("CTRL") = "OFF"
                                End If
                            End If
                        Else
                            '途中のA1は、チェックしない（出庫→帰庫→出庫→帰庫の場合、A1が複数発生する）
                            WW_A1F1row("CTRL") = "OFF"
                        End If
                    End If

                    WW_YMD = WW_A1F1row("YMD")
                    WW_NIPPONO = WW_A1F1row("NIPPONO")
                    WW_STAFFCODE = WW_A1F1row("STAFFCODE")
                Next
            End If

            T0005INPtbl.Merge(WW_A1F1tbl)
            WW_A1F1tbl.Dispose()
            WW_A1F1tbl = Nothing

            '------------------------------------------------------------
            '■出庫日、従業員 単位
            '  取込んだ日報番号毎のヘッダを出庫日、従業員毎に集約し直す
            '------------------------------------------------------------
            'ヘッダを捨て、明細からヘッダを作り直す
            CreateT0005Header(T0005INPtbl)

            '---------------------------------------------------
            '■出庫日、従業員 単位
            '  明細行番号（並び順）の振り直し
            '---------------------------------------------------
            Dim WW_SEQ As Integer = 1

            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                '行番号の採番
                If T0005INPtbl.Rows(i)("HDKBN") = "H" Then
                    WW_SEQ = 1
                    T0005INPtbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")
                    Continue For
                End If
                T0005INPtbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")
                WW_SEQ = WW_SEQ + 1
            Next

            WW_T0005INPtbl.Dispose()
            WW_T0005INPtbl = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
            CS0011LOGWRITE.INFSUBCLASS = "T0005tbl_Edit"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try
    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    'EXCEL取込み処理
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        T0005COM.AddColumnT0005tbl(T0005INPtbl)

        '■■■ UPLOAD_XLSデータ取得 ■■■   ☆☆☆ 2015/4/30追加
        'CS0023XLSTBL.MAPID = Master.MAPID
        CS0023XLSTBL.MAPID = GRT00005WRKINC.MAPID
        CS0023XLSTBL.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSTBL.CS0023XLSUPLOAD(String.Empty, Master.PROF_REPORT)
        If isNormal(CS0023XLSTBL.ERR) Then
            If CS0023XLSTBL.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR, "例外発生")

                Exit Sub
            End If
        Else
            Master.Output(CS0023XLSTBL.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSTBL")

            Exit Sub
        End If

        'EXCELデータの初期化（DBNullを撲滅）
        Dim CS0023XLSTBLrow As DataRow = CS0023XLSTBL.TBLDATA.NewRow
        For Each XLSRow As DataRow In CS0023XLSTBL.TBLDATA.Rows
            CS0023XLSTBLrow.ItemArray = XLSRow.ItemArray

            For j As Integer = 0 To CS0023XLSTBL.TBLDATA.Columns.Count - 1
                If IsDBNull(CS0023XLSTBLrow.Item(j)) OrElse IsNothing(CS0023XLSTBLrow.Item(j)) Then
                    CS0023XLSTBLrow.Item(j) = ""
                End If
            Next
            XLSRow.ItemArray = CS0023XLSTBLrow.ItemArray
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
        rightview.SetErrorReport("")

        '○T0005INPtblカラム設定
        T0005COM.AddColumnT0005tbl(T0005INPtbl)

        Dim WW_TEXT As String = ""
        Dim WW_VALUE As String = ""

        '■■■ Excelデータ毎にチェック＆更新 ■■■
        For Each XLSRow As DataRow In CS0023XLSTBL.TBLDATA.Rows

            '○XLSTBL明細⇒T0005INProw
            Dim T0005INProw As DataRow = T0005INPtbl.NewRow
            '○初期クリア
            T0005COM.InitialT5INPRow(T0005INProw)

            T0005INProw("LINECNT") = 0
            T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            T0005INProw("TIMSTP") = "0"
            T0005INProw("SELECT") = 1
            T0005INProw("HIDDEN") = 1

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                T0005INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            Else
                T0005INProw("CAMPCODE") = XLSRow("CAMPCODE").PadLeft(2, "0"c)
                '名称付与
                WW_TEXT = ""
                CodeToName("CAMPCODE", T0005INProw("CAMPCODE"), WW_TEXT, WW_RTN)
                T0005INProw("CAMPNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("SHIPORG") < 0 Then
                T0005INProw("SHIPORG") = work.WF_SEL_UORG.Text
                WW_TEXT = ""
                CodeToName("SHIPORG", T0005INProw("SHIPORG"), WW_TEXT, WW_RTN)
                T0005INProw("SHIPORGNAMES") = WW_TEXT
            Else
                T0005INProw("SHIPORG") = XLSRow("SHIPORG")
                '名称付与
                WW_TEXT = ""
                CodeToName("SHIPORG", T0005INProw("SHIPORG"), WW_TEXT, WW_RTN)
                T0005INProw("SHIPORGNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("TERMKBN") < 0 Then
                T0005INProw("TERMKBN") = GRT00005WRKINC.TERM_TYPE.HAND
            Else
                T0005INProw("TERMKBN") = XLSRow("TERMKBN")
                CodeToName("TERMKBN", T0005INProw("TERMKBN"), WW_TEXT, WW_RTN)
                T0005INProw("TERMKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("YMD") >= 0 Then
                If IsDate(XLSRow("YMD")) Then
                    WW_DATE = XLSRow("YMD")
                    T0005INProw("YMD") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0005INProw("YMD") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("NIPPONO") >= 0 Then
                If IsDBNull(XLSRow("NIPPONO")) Then
                    T0005INProw("NIPPONO") = ""
                Else
                    T0005INProw("NIPPONO") = XLSRow("NIPPONO")
                End If
            End If

            T0005INProw("HDKBN") = "D"

            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                If IsDBNull(XLSRow("SEQ")) Then
                    T0005INProw("SEQ") = ""
                Else
                    T0005INProw("SEQ") = XLSRow("SEQ")
                End If
            End If

            If WW_COLUMNS.IndexOf("WORKKBN") >= 0 Then
                T0005INProw("WORKKBN") = XLSRow("WORKKBN")
                '名称付与
                CodeToName("WORKKBN", T0005INProw("WORKKBN"), WW_TEXT, WW_RTN)
                T0005INProw("WORKKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                T0005INProw("STAFFCODE") = XLSRow("STAFFCODE")
                '名称付与
                CodeToName("STAFFCODE", T0005INProw("STAFFCODE"), WW_TEXT, WW_RTN)
                T0005INProw("STAFFNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("SUBSTAFFCODE") >= 0 Then
                T0005INProw("SUBSTAFFCODE") = XLSRow("SUBSTAFFCODE")
                '名称付与
                CodeToName("STAFFCODE", T0005INProw("SUBSTAFFCODE"), WW_TEXT, WW_RTN)
                T0005INProw("SUBSTAFFNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("CREWKBN") >= 0 Then
                T0005INProw("CREWKBN") = XLSRow("CREWKBN")
                '名称付与
                CodeToName("CREWKBN", T0005INProw("CREWKBN"), WW_TEXT, WW_RTN)
                T0005INProw("CREWKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("GSHABAN") >= 0 Then
                T0005INProw("GSHABAN") = XLSRow("GSHABAN")
            End If

            If WW_COLUMNS.IndexOf("STDATE") >= 0 Then
                If IsDate(XLSRow("STDATE")) Then
                    WW_DATE = XLSRow("STDATE")
                    T0005INProw("STDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0005INProw("STDATE") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("STTIME") >= 0 Then
                If IsDate(XLSRow("STTIME")) Then
                    WW_DATE = XLSRow("STTIME")
                    T0005INProw("STTIME") = WW_DATE.ToString("HH:mm")
                Else
                    T0005INProw("STTIME") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDDATE") >= 0 Then
                If IsDate(XLSRow("ENDDATE")) Then
                    WW_DATE = XLSRow("ENDDATE")
                    T0005INProw("ENDDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0005INProw("ENDDATE") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDTIME") >= 0 Then
                If IsDate(XLSRow("ENDTIME")) Then
                    WW_DATE = XLSRow("ENDTIME")
                    T0005INProw("ENDTIME") = WW_DATE.ToString("HH:mm")
                Else
                    T0005INProw("ENDTIME") = ""
                End If
            End If

            If WW_COLUMNS.IndexOf("WORKTIME") < 0 Then
                T0005INProw("WORKTIME") = "00:00"
            Else
                T0005INProw("WORKTIME") = XLSRow("WORKTIME")
            End If

            If WW_COLUMNS.IndexOf("MOVETIME") < 0 Then
                T0005INProw("MOVETIME") = "00:00"
            Else
                T0005INProw("MOVETIME") = XLSRow("MOVETIME")
            End If

            If WW_COLUMNS.IndexOf("ACTTIME") < 0 Then
                T0005INProw("ACTTIME") = "00:00"
            Else
                T0005INProw("ACTTIME") = XLSRow("ACTTIME")
            End If

            If WW_COLUMNS.IndexOf("PRATE") < 0 Then
                T0005INProw("PRATE") = "0"
            Else
                T0005INProw("PRATE") = XLSRow("PRATE")
            End If

            If WW_COLUMNS.IndexOf("CASH") < 0 Then
                T0005INProw("CASH") = "0"
            Else
                T0005INProw("CASH") = XLSRow("CASH")
            End If

            If WW_COLUMNS.IndexOf("TICKET") < 0 Then
                T0005INProw("TICKET") = "0"
            Else
                T0005INProw("TICKET") = XLSRow("TICKET")
            End If

            If WW_COLUMNS.IndexOf("ETC") < 0 Then
                T0005INProw("ETC") = "0"
            Else
                T0005INProw("ETC") = XLSRow("ETC")
            End If

            If WW_COLUMNS.IndexOf("TOTALTOLL") < 0 Then
                T0005INProw("TOTALTOLL") = "0"
            Else
                T0005INProw("TOTALTOLL") = XLSRow("TOTALTOLL")
            End If

            If WW_COLUMNS.IndexOf("STMATER") < 0 Then
                T0005INProw("STMATER") = "0.00"
            Else
                T0005INProw("STMATER") = XLSRow("STMATER")
            End If

            If WW_COLUMNS.IndexOf("ENDMATER") < 0 Then
                T0005INProw("ENDMATER") = "0.00"
            Else
                T0005INProw("ENDMATER") = XLSRow("ENDMATER")
            End If

            If WW_COLUMNS.IndexOf("RUIDISTANCE") < 0 Then
                T0005INProw("RUIDISTANCE") = "0.00"
            Else
                T0005INProw("RUIDISTANCE") = XLSRow("RUIDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("SOUDISTANCE") < 0 Then
                T0005INProw("SOUDISTANCE") = "0.00"
            Else
                T0005INProw("SOUDISTANCE") = XLSRow("SOUDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("JIDISTANCE") < 0 Then
                T0005INProw("JIDISTANCE") = "0.00"
            Else
                T0005INProw("JIDISTANCE") = XLSRow("JIDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KUDISTANCE") < 0 Then
                T0005INProw("KUDISTANCE") = "0.00"
            Else
                T0005INProw("KUDISTANCE") = XLSRow("KUDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("IPPDISTANCE") < 0 Then
                T0005INProw("IPPDISTANCE") = "0.00"
            Else
                T0005INProw("IPPDISTANCE") = XLSRow("IPPDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KOSDISTANCE") < 0 Then
                T0005INProw("KOSDISTANCE") = "0.00"
            Else
                T0005INProw("KOSDISTANCE") = XLSRow("KOSDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("IPPJIDISTANCE") < 0 Then
                T0005INProw("IPPJIDISTANCE") = "0.00"
            Else
                T0005INProw("IPPJIDISTANCE") = XLSRow("IPPJIDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("IPPKUDISTANCE") < 0 Then
                T0005INProw("IPPKUDISTANCE") = "0.00"
            Else
                T0005INProw("IPPKUDISTANCE") = XLSRow("IPPKUDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KOSJIDISTANCE") < 0 Then
                T0005INProw("KOSJIDISTANCE") = "0.00"
            Else
                T0005INProw("KOSJIDISTANCE") = XLSRow("KOSJIDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KOSKUDISTANCE") < 0 Then
                T0005INProw("KOSKUDISTANCE") = "0.00"
            Else
                T0005INProw("KOSKUDISTANCE") = XLSRow("KOSKUDISTANCE")
            End If

            If WW_COLUMNS.IndexOf("KYUYU") < 0 Then
                T0005INProw("KYUYU") = "0.00"
            Else
                T0005INProw("KYUYU") = XLSRow("KYUYU")
            End If

            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                T0005INProw("TORICODE") = XLSRow("TORICODE")
                '名称付与
                CodeToName("TORICODE", T0005INProw("TORICODE"), WW_TEXT, WW_RTN)
                T0005INProw("TORINAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("SHUKABASHO") >= 0 Then
                T0005INProw("SHUKABASHO") = XLSRow("SHUKABASHO")
                '名称付与
                CodeToName("SHUKABASHO", T0005INProw("SHUKABASHO"), WW_TEXT, WW_RTN)
                T0005INProw("SHUKABASHONAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("SHUKADATE") >= 0 Then
                T0005INProw("SHUKADATE") = XLSRow("SHUKADATE")
            End If

            If WW_COLUMNS.IndexOf("TODOKECODE") >= 0 Then
                T0005INProw("TODOKECODE") = XLSRow("TODOKECODE")
                '名称付与
                CodeToName("TODOKECODEY", T0005INProw("TODOKECODE"), WW_TEXT, WW_RTN)
                T0005INProw("TODOKENAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("TODOKEDATE") >= 0 Then
                T0005INProw("TODOKEDATE") = XLSRow("TODOKEDATE")
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
                    T0005INProw(WW_OILTYPE) = XLSRow(WW_OILTYPE)
                End If

                If WW_COLUMNS.IndexOf(WW_PRODUCT1) >= 0 Then
                    T0005INProw(WW_PRODUCT1) = XLSRow(WW_PRODUCT1)
                End If

                If WW_COLUMNS.IndexOf(WW_PRODUCT2) >= 0 Then
                    T0005INProw(WW_PRODUCT2) = XLSRow(WW_PRODUCT2)
                End If

                If WW_COLUMNS.IndexOf(WW_PRODUCTCODE) >= 0 Then
                    T0005INProw(WW_PRODUCTCODE) = XLSRow(WW_PRODUCTCODE)
                    '名称付与
                    Dim WW_PRODUCT As String = T0005INProw(WW_PRODUCTCODE)
                    CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN)
                    T0005INProw(WW_PRODUCTNAMES) = WW_TEXT
                Else
                    If T0005INProw(WW_OILTYPE) <> "" AndAlso T0005INProw(WW_PRODUCT1) <> "" AndAlso T0005INProw(WW_PRODUCT2) Then
                        T0005INProw(WW_PRODUCTCODE) = T0005INProw("CAMPCODE") &
                                                      T0005INProw(WW_OILTYPE) &
                                                      T0005INProw(WW_PRODUCT1) &
                                                      T0005INProw(WW_PRODUCT2)
                        '名称付与
                        Dim WW_PRODUCT As String = T0005INProw(WW_PRODUCTCODE)
                        CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN)
                        T0005INProw(WW_PRODUCTNAMES) = WW_TEXT
                    End If
                End If

                If WW_COLUMNS.IndexOf(WW_SURYO) < 0 Then
                    T0005INProw(WW_SURYO) = "0.000"
                Else
                    T0005INProw(WW_SURYO) = XLSRow(WW_SURYO)
                End If

                If WW_COLUMNS.IndexOf(WW_STANI) >= 0 Then
                    T0005INProw(WW_STANI) = XLSRow(WW_STANI)
                End If
            Next


            If WW_COLUMNS.IndexOf("TOTALSURYO") < 0 Then
                T0005INProw("TOTALSURYO") = "0.000"
            Else
                T0005INProw("TOTALSURYO") = XLSRow("TOTALSURYO")
            End If

            If WW_COLUMNS.IndexOf("TUMIOKIKBN") >= 0 Then
                T0005INProw("TUMIOKIKBN") = XLSRow("TUMIOKIKBN")
                '名称付与
                CodeToName("TUMIOKIKBN", T0005INProw("TUMIOKIKBN"), WW_TEXT, WW_RTN)
                T0005INProw("TUMIOKIKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("URIKBN") >= 0 Then
                T0005INProw("URIKBN") = XLSRow("URIKBN")
                '名称付与
                CodeToName("URIKBN", T0005INProw("URIKBN"), WW_TEXT, WW_RTN)
                T0005INProw("URIKBNNAMES") = WW_TEXT
            End If

            If WW_COLUMNS.IndexOf("ORDERNO") >= 0 Then
                T0005INProw("ORDERNO") = XLSRow("ORDERNO")
            End If

            If WW_COLUMNS.IndexOf("DETAILNO") >= 0 Then
                T0005INProw("DETAILNO") = XLSRow("DETAILNO")
            End If

            If WW_COLUMNS.IndexOf("TRIPNO") >= 0 Then
                T0005INProw("TRIPNO") = XLSRow("TRIPNO")
            End If

            If WW_COLUMNS.IndexOf("DROPNO") >= 0 Then
                T0005INProw("DROPNO") = XLSRow("DROPNO")
            End If

            If WW_COLUMNS.IndexOf("TAXKBN") < 0 Then
                T0005INProw("TAXKBN") = "0"
            Else
                T0005INProw("TAXKBN") = XLSRow("TAXKBN")
            End If

            If WW_COLUMNS.IndexOf("STORICODE") >= 0 Then
                T0005INProw("STORICODE") = XLSRow("STORICODE")
            End If

            If WW_COLUMNS.IndexOf("CONTCHASSIS") >= 0 Then
                T0005INProw("CONTCHASSIS") = XLSRow("CONTCHASSIS")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEF") >= 0 Then
                T0005INProw("SHARYOTYPEF") = XLSRow("SHARYOTYPEF")
            End If

            If WW_COLUMNS.IndexOf("TSHABANF") >= 0 Then
                T0005INProw("TSHABANF") = XLSRow("TSHABANF")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB") >= 0 Then
                T0005INProw("SHARYOTYPEB") = XLSRow("SHARYOTYPEB")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB") >= 0 Then
                T0005INProw("TSHABANB") = XLSRow("TSHABANB")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB2") >= 0 Then
                T0005INProw("SHARYOTYPEB2") = XLSRow("SHARYOTYPEB2")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB2") >= 0 Then
                T0005INProw("TSHABANB2") = XLSRow("TSHABANB2")
            End If

            If WW_COLUMNS.IndexOf("JISSKIKBN") < 0 Then
                T0005INProw("JISSKIKBN") = "0"
            Else
                T0005INProw("JISSKIKBN") = XLSRow("JISSKIKBN")
            End If

            If WW_COLUMNS.IndexOf("DELFLG") < 0 Then
                T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE
            Else
                T0005INProw("DELFLG") = XLSRow("DELFLG")
            End If

            T0005INPtbl.Rows.Add(T0005INProw)

        Next

        CS0026TBLSORT.TABLE = T0005INPtbl
        CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        CS0026TBLSORT.FILTER = ""
        T0005INPtbl = CS0026TBLSORT.sort()

        'ヘッダレコード作成 ＆ 項番(LineCnt)設定
        Dim WW_ERR As String = C_MESSAGE_NO.NORMAL
        Dim WW_LINECNT As Integer = 0
        Dim WW_SOUDISTANCE As Decimal = 0
        Dim WW_IDX As Integer = 0
        Dim WW_FIRST As String = "OFF"
        Dim WW_Cols As String() = {"YMD", "STAFFCODE"}
        Dim WW_KEYtbl As DataTable
        Dim WW_T0005tbl As DataTable = T0005INPtbl.Clone
        Dim WW_TBLview As DataView
        Dim WW_RESULT As String = C_MESSAGE_NO.NORMAL
        Dim WW_T0005INPtbl As DataTable = T0005INPtbl.Clone

        'キーテーブル作成
        WW_TBLview = New DataView(T0005INPtbl)
        WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)
        WW_TBLview.Dispose()
        WW_TBLview = Nothing

        WW_T0005tbl = T0005INPtbl.Clone
        'ヘッダレコードが存在する場合、チェックを行う
        For j As Integer = 0 To WW_KEYtbl.Rows.Count - 1
            WW_T0005tbl.Clear()
            '同一の日報、従業員を抽出（ワークテーブルに保存）
            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                If WW_KEYtbl.Rows(j)("YMD") = T0005INPtbl.Rows(i)("YMD") AndAlso
                   WW_KEYtbl.Rows(j)("STAFFCODE") = T0005INPtbl.Rows(i)("STAFFCODE") Then
                    Dim T0005INProw As DataRow = WW_T0005tbl.NewRow
                    T0005INProw.ItemArray = T0005INPtbl.Rows(i).ItemArray
                    WW_T0005tbl.Rows.Add(T0005INProw)
                End If
            Next

            WW_ERRLIST_ALL = New List(Of String)
            For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                WW_ERRLIST = New List(Of String)
                Dim T0005INProw As DataRow = WW_T0005tbl.Rows(i)
                CheckT0005INPRow(T0005INProw, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
                    If Not isNormal(WW_ERRCODE) Then
                        T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                End If
                If WW_ERR <= WW_ERRCODE Then
                    WW_ERR = WW_ERRCODE
                End If
            Next

            If WW_ERRLIST_ALL.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) < 0 Then
                WW_T0005INPtbl.Merge(WW_T0005tbl)
            End If
        Next

        T0005INPtbl = WW_T0005INPtbl.Copy

        'キーテーブル作成
        WW_TBLview = New DataView(T0005INPtbl)
        WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)
        WW_TBLview.Dispose()
        WW_TBLview = Nothing

        'ヘッダレコード作成
        CreateT0005Header(T0005INPtbl)

        WW_T0005INPtbl = T0005INPtbl.Copy

        T0005INPtbl.Clear()

        '従業員、タイトル区分（ヘッダ、明細）、開始時刻の順番で処理する
        CS0026TBLSORT.TABLE = T0005INPtbl
        CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        CS0026TBLSORT.FILTER = ""
        T0005INPtbl = CS0026TBLSORT.sort()

        WW_T0005tbl = T0005INPtbl.Clone
        'ヘッダレコードが存在する場合、チェックを行う
        For j As Integer = 0 To WW_KEYtbl.Rows.Count - 1
            WW_T0005tbl.Clear()
            '同一の日報、従業員を抽出（ワークテーブルに保存）
            For i As Integer = 0 To WW_T0005INPtbl.Rows.Count - 1
                If WW_KEYtbl.Rows(j)("YMD") = WW_T0005INPtbl.Rows(i)("YMD") AndAlso
                   WW_KEYtbl.Rows(j)("STAFFCODE") = WW_T0005INPtbl.Rows(i)("STAFFCODE") Then
                    Dim T0005INProw As DataRow = WW_T0005tbl.NewRow
                    T0005INProw.ItemArray = WW_T0005INPtbl.Rows(i).ItemArray
                    WW_T0005tbl.Rows.Add(T0005INProw)
                End If
            Next

            '同一の日報、従業員毎にチェックを行う
            WW_ERRLIST = New List(Of String)

            CheckAllT0005tbl(WW_T0005tbl, WW_RTN)

            T0005INPtbl.Merge(WW_T0005tbl)

            If isNormal(WW_RTN) OrElse WW_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST Then
            Else
                If WW_ERR <= WW_RTN Then
                    WW_ERR = WW_RTN
                End If
            End If
        Next

        '■■■ GridView更新 ■■■
        UpdateGridDataOnExcel(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then Exit Sub

        '○メッセージ表示
        If isNormal(WW_ERR) AndAlso isNormal(WW_RESULT) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERR, C_MESSAGE_TYPE.ERR)
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
    ''' T0005INProwチェック
    ''' </summary>
    ''' <param name="I_ROW">チェック対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckT0005INPRow(ByVal I_ROW As DataRow, ByRef O_RTN As String)

        '○インターフェイス初期値設定
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_CONVERT As String = String.Empty
        Dim WW_RESULT As String = String.Empty
        Dim WW_TEXT As String = String.Empty
        Dim WW_DATE_ERR As String = "OFF"
        Dim WW_LINEerr As String = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = String.Empty
        Dim WW_CheckMES2 As String = String.Empty
        Dim O_VALUE As String = String.Empty
        Dim O_MESSAGE_NO As String = C_MESSAGE_NO.NORMAL
        Dim O_CHECKREPORT As String = String.Empty

        S0013tbl = New DataTable

        WW_ERRLIST = New List(Of String)

        '■■■ 単項目チェック(ヘッダー情報) ■■■

        '-------------------------------------------------------------------------------
        '全レコード必須
        '-------------------------------------------------------------------------------

        '権限チェック（更新権限）
        '　　出荷部署
        '・キー項目(出荷部署：SHIPORG)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHIPORG", I_ROW("SHIPORG"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("SHIPORG") = O_VALUE
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(出荷部署エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        End If

        CS0012AUTHORorg.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0012AUTHORorg.ORGCODE = I_ROW("SHIPORG")
        CS0012AUTHORorg.STYMD = Date.Now
        CS0012AUTHORorg.ENDYMD = Date.Now
        CS0012AUTHORorg.CS0012AUTHORorg()
        If isNormal(CS0012AUTHORorg.ERR) AndAlso CS0012AUTHORorg.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(権限無)です。"
            OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        End If

        '・キー項目(会社コード：CAMPCODE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", I_ROW("CAMPCODE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("CAMPCODE") = O_VALUE
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        End If

        '・キー項目(日報年月日：YMD)
        '①必須・項目属性チェック
        Dim WW_YMDERR As String = "OFF"
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "YMD", I_ROW("YMD"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("YMD") = O_VALUE
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(日報年月日エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            WW_YMDERR = "ON"
        End If

        '・キー項目(乗務員：STAFFCODE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", I_ROW("STAFFCODE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("STAFFCODE") = O_VALUE
            '②LeftBox存在チェック
            If I_ROW("STAFFCODE") <> "" Then
                CodeToName("STAFFCODE", I_ROW("STAFFCODE"), WW_TEXT, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(乗務員エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("STAFFCODE") & ")"
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
                I_ROW("STAFFNAMES") = WW_TEXT
                '乗務員＝副乗務員はエラー
                If I_ROW("STAFFCODE") = I_ROW("SUBSTAFFCODE") Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(乗務員エラー)です。"
                    WW_CheckMES2 = "乗務員と副乗務員が同じ (" & I_ROW("STAFFCODE") & "," & I_ROW("SUBSTAFFCODE") & ") "
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(乗務員エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(作業区分：WORKKBN)
        '①必須・項目属性チェック
        If I_ROW("HDKBN") = "D" Then
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKKBN", I_ROW("WORKKBN"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("WORKKBN") = O_VALUE      '頭０埋め
                '②LeftBox存在チェック
                If I_ROW("WORKKBN") <> "" Then
                    CodeToName("WORKKBN", I_ROW("WORKKBN"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(作業区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("WORKKBN") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(作業区分エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
        End If

        '・キー項目(開始日：STDATE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STDATE", I_ROW("STDATE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("STDATE") = O_VALUE
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(開始日エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            WW_DATE_ERR = "ON"
        End If

        '・キー項目(開始時刻：STTIME)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", I_ROW("STTIME"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            If Not String.IsNullOrEmpty(O_VALUE) Then
                I_ROW("STTIME") = CDate(O_VALUE).ToString("HH:mm")
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(開始時刻エラー)です。"
                WW_CheckMES2 = O_VALUE
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                WW_DATE_ERR = "ON"
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(開始時刻エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            WW_DATE_ERR = "ON"
        End If

        '・キー項目(終了日：ENDDATE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDDATE", I_ROW("ENDDATE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("ENDDATE") = O_VALUE
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(終了日エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            WW_DATE_ERR = "ON"
        End If

        '・キー項目(終了時刻：ENDTIME)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", I_ROW("ENDTIME"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            If Not String.IsNullOrEmpty(O_VALUE) Then
                I_ROW("ENDTIME") = CDate(O_VALUE).ToString("HH:mm")
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(終了時刻エラー)です。"
                WW_CheckMES2 = O_VALUE
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                WW_DATE_ERR = "ON"
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(終了時刻エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            WW_DATE_ERR = "ON"
        End If

        '・キー項目(明細行番号：SEQ)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SEQ", I_ROW("SEQ"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("SEQ") = O_VALUE
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(明細行番号エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(車端区分：TERMKBN)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TERMKBN", I_ROW("TERMKBN"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("TERMKBN") = O_VALUE
            '②LeftBox存在チェック
            If I_ROW("TERMKBN") <> "" Then
                CodeToName("TERMKBN", I_ROW("TERMKBN"), WW_TEXT, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(車端区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("TERMKBN") & ") "
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード((車端区分エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        If I_ROW("WORKKBN") = "F1" Then
            '・キー項目(日報番号：NIPPONO)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NIPPONO", I_ROW("NIPPONO"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("NIPPONO") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(日報番号エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(業務車番：GSHABAN)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "GSHABAN", I_ROW("GSHABAN"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("GSHABAN") = O_VALUE
                '②LeftBox存在チェック
                If I_ROW("GSHABAN") <> "" Then
                    CodeToName("GSHABAN", I_ROW("GSHABAN"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(業務車番エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("GSHABAN") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(業務車番エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(統一車番（前）（上）：SHARYOTYPEF)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE", I_ROW("SHARYOTYPEF"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SHARYOTYPEF") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(統一車番（前）（上）エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(統一車番（前）（下）：TSHABANF)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TSHABAN", I_ROW("TSHABANF"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("TSHABANF") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(統一車番（前）（下）エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(統一車番（後）（上）：SHARYOTYPEB)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE", I_ROW("SHARYOTYPEB"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SHARYOTYPEB") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(統一車番（後）（上）エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(統一車番（後）（下）：TSHABANB)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TSHABAN", I_ROW("TSHABANB"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("TSHABANB") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(統一車番（後）（下）エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(統一車番（後）（上）2：SHARYOTYPEB2)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE", I_ROW("SHARYOTYPEB2"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SHARYOTYPEB2") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(統一車番（後）（上）2エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(統一車番（後）（下）2：TSHABANB2)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TSHABAN", I_ROW("TSHABANB2"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("TSHABANB2") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(統一車番（後）（下）2エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(乗務区分：CREWKBN)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CREWKBN", I_ROW("CREWKBN"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("CREWKBN") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(乗務区分エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '■容器検査期限、車検期限チェック（八戸、大井川、水島のみ）
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
            Dim WW_HPRSINSNYMDF As String = ""
            Dim WW_HPRSINSNYMDB As String = ""
            Dim WW_HPRSINSNYMDB2 As String = ""
            Dim WW_LICNYMDF As String = ""
            Dim WW_LICNYMDB As String = ""
            Dim WW_LICNYMDB2 As String = ""
            Dim WW_HPR As Boolean = False
            Dim WW_LICNPLTNOF As String = ""
            Dim WW_LICNPLTNOB As String = ""
            Dim WW_LICNPLTNOB2 As String = ""
            If WW_YMDERR = "OFF" AndAlso I_ROW("YMD") <> "" Then

                If T0005COM.IsInspectionOrg(work.WF_SEL_CAMPCODE.Text, I_ROW("SHIPORG"), WW_RTN) Then

                    If IsNothing(work.CreateTSHABANList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, WW_RTN).Items.FindByValue(I_ROW("GSHABAN"))) Then
                        If work.CreateSHABAN2OILList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, WW_RTN).Items.FindByValue(I_ROW("GSHABAN")).Text = "02" Then
                            Dim sublist As ListBox = work.GetShabanSubTable(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)(I_ROW("GSHABAN"))
                            WW_HPRSINSNYMDF = sublist.Items.FindByText("HPRSINSNYMDF").Value.Replace("-", "/")
                            WW_HPRSINSNYMDB = sublist.Items.FindByText("HPRSINSNYMDB").Value.Replace("-", "/")
                            WW_HPRSINSNYMDB2 = sublist.Items.FindByText("HPRSINSNYMDB2").Value.Replace("-", "/")
                            WW_LICNYMDF = sublist.Items.FindByText("LICNYMDF").Value.Replace("-", "/")
                            WW_LICNYMDB = sublist.Items.FindByText("LICNYMDB").Value.Replace("-", "/")
                            WW_LICNYMDB2 = sublist.Items.FindByText("LICNYMDB2").Value.Replace("-", "/")
                            WW_LICNPLTNOF = sublist.Items.FindByText("LICNPLTNOF").Value
                            WW_LICNPLTNOB = sublist.Items.FindByText("LICNPLTNOB").Value
                            WW_LICNPLTNOB2 = sublist.Items.FindByText("LICNPLTNOB2").Value
                            WW_HPR = True
                        End If
                    End If

                    '高圧のみ
                    If WW_HPR Then

                        '容器検査年月日チェック（２カ月前から警告、４日前はエラー）
                        '車検年月日チェック（１カ月前から警告、４日前はエラー）
                        '------ 車両前 -------------------------------------------------------------------------
                        '車検チェック
                        If I_ROW("SHARYOTYPEF") = "A" OrElse
                           I_ROW("SHARYOTYPEF") = "C" OrElse
                           I_ROW("SHARYOTYPEF") = "D" Then
                            If IsDate(WW_LICNYMDF) Then
                                Dim WW_days As String = DateDiff("d", I_ROW("YMD"), CDate(WW_LICNYMDF))
                                If CDate(WW_LICNYMDF) < I_ROW("YMD") Then
                                    '車検切れ
                                    WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOF & " " & I_ROW("SHARYOTYPEF") & I_ROW("TSHABANF") & " " & WW_LICNYMDF & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_LICNYMDF).AddDays(-4) < I_ROW("YMD") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & I_ROW("SHARYOTYPEF") & I_ROW("TSHABANF") & " " & WW_LICNYMDF & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_LICNYMDF).AddMonths(-1) < I_ROW("YMD") Then
                                    '1カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & I_ROW("SHARYOTYPEF") & I_ROW("TSHABANF") & " " & WW_LICNYMDF & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOF & " " & I_ROW("SHARYOTYPEF") & I_ROW("TSHABANF") & ")"
                                OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                            End If
                        End If

                        '容器チェック
                        If I_ROW("SHARYOTYPEF") = "B" OrElse
                           I_ROW("SHARYOTYPEF") = "D" Then
                            If IsDate(WW_HPRSINSNYMDF) Then
                                Dim WW_days As String = DateDiff("d", I_ROW("YMD"), CDate(WW_HPRSINSNYMDF))
                                If CDate(WW_HPRSINSNYMDF) < I_ROW("YMD") Then
                                    '容器検査切れ
                                    WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOF & " " & I_ROW("SHARYOTYPEF") & I_ROW("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_HPRSINSNYMDF).AddDays(-4) < I_ROW("YMD") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & I_ROW("SHARYOTYPEF") & I_ROW("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_HPRSINSNYMDF).AddMonths(-2) < I_ROW("YMD") Then
                                    '2カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & I_ROW("SHARYOTYPEF") & I_ROW("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOF & " " & I_ROW("SHARYOTYPEF") & I_ROW("TSHABANF") & ")"
                                OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                            End If

                        End If

                        '------ 車両後 -------------------------------------------------------------------------
                        '車検チェック
                        If I_ROW("SHARYOTYPEB") = "A" OrElse
                           I_ROW("SHARYOTYPEB") = "C" OrElse
                           I_ROW("SHARYOTYPEB") = "D" Then
                            If IsDate(WW_LICNYMDB) Then
                                Dim WW_days As String = DateDiff("d", I_ROW("YMD"), CDate(WW_LICNYMDB))
                                If CDate(WW_LICNYMDB) < I_ROW("YMD") Then
                                    '車検切れ
                                    WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB & " " & I_ROW("SHARYOTYPEB") & I_ROW("TSHABANB") & " " & WW_LICNYMDB & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_LICNYMDB).AddDays(-4) < I_ROW("YMD") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & I_ROW("SHARYOTYPEB") & I_ROW("TSHABANB") & " " & WW_LICNYMDB & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_LICNYMDB).AddMonths(-1) < I_ROW("YMD") Then
                                    '1カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & I_ROW("SHARYOTYPEB") & I_ROW("TSHABANB") & " " & WW_LICNYMDB & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB & " " & I_ROW("SHARYOTYPEB") & I_ROW("TSHABANB") & ")"
                                OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                            End If
                        End If

                        '容器チェック
                        If I_ROW("SHARYOTYPEB") = "B" OrElse
                           I_ROW("SHARYOTYPEB") = "D" Then
                            If IsDate(WW_HPRSINSNYMDB) Then
                                Dim WW_days As String = DateDiff("d", I_ROW("YMD"), CDate(WW_HPRSINSNYMDB))
                                If CDate(WW_HPRSINSNYMDB) < I_ROW("YMD") Then
                                    '容器検査切れ
                                    WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB & " " & I_ROW("SHARYOTYPEB") & I_ROW("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_HPRSINSNYMDB).AddDays(-4) < I_ROW("YMD") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & I_ROW("SHARYOTYPEB") & I_ROW("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_HPRSINSNYMDB).AddMonths(-2) < I_ROW("YMD") Then
                                    '2カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & I_ROW("SHARYOTYPEB") & I_ROW("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB & " " & I_ROW("SHARYOTYPEB") & I_ROW("TSHABANB") & ")"
                                OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                            End If

                        End If

                        '------ 車両後２ -------------------------------------------------------------------------
                        '車検チェック
                        If I_ROW("SHARYOTYPEB2") = "A" OrElse
                           I_ROW("SHARYOTYPEB2") = "C" OrElse
                           I_ROW("SHARYOTYPEB2") = "D" Then
                            If IsDate(WW_LICNYMDB2) Then
                                Dim WW_days As String = DateDiff("d", I_ROW("YMD"), CDate(WW_LICNYMDB2))
                                If CDate(WW_LICNYMDB2) < I_ROW("YMD") Then
                                    '車検切れ
                                    WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB2 & " " & I_ROW("SHARYOTYPEB2") & I_ROW("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_LICNYMDB2).AddDays(-4) < I_ROW("YMD") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & I_ROW("SHARYOTYPEB2") & I_ROW("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_LICNYMDB2).AddMonths(-1) < I_ROW("YMD") Then
                                    '1カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & I_ROW("SHARYOTYPEB2") & I_ROW("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB2 & " " & I_ROW("SHARYOTYPEB2") & I_ROW("TSHABANB2") & ")"
                                OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                            End If
                        End If

                        '容器チェック
                        If I_ROW("SHARYOTYPEB2") = "B" OrElse
                           I_ROW("SHARYOTYPEB2") = "D" Then
                            If IsDate(WW_HPRSINSNYMDB2) Then
                                Dim WW_days As String = DateDiff("d", I_ROW("YMD"), CDate(WW_HPRSINSNYMDB2))
                                If CDate(WW_HPRSINSNYMDB2) < I_ROW("YMD") Then
                                    '容器検査切れ
                                    WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB2 & " " & I_ROW("SHARYOTYPEB2") & I_ROW("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_HPRSINSNYMDB2).AddDays(-4) < I_ROW("YMD") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & I_ROW("SHARYOTYPEB2") & I_ROW("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                ElseIf CDate(WW_HPRSINSNYMDB2).AddMonths(-2) < I_ROW("YMD") Then
                                    '2カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & I_ROW("SHARYOTYPEB2") & I_ROW("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB2 & " " & I_ROW("SHARYOTYPEB2") & I_ROW("TSHABANB2") & ")"
                                OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                            End If

                        End If
                    End If
                End If
            End If

        End If

        '-------------------------------------------------------------------------------
        '荷積、荷卸しの場合
        '-------------------------------------------------------------------------------
        If I_ROW("WORKKBN") = "B2" OrElse
           I_ROW("WORKKBN") = "B3" Then
            '・キー項目(出荷場所：SHUKABASHO)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", I_ROW("SHUKABASHO"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                '②LeftBox存在チェック
                If I_ROW("SHUKABASHO") <> "" Then
                    CodeToName("SHUKABASHO", I_ROW("SHUKABASHO"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(出荷場所エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("SHUKABASHO") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                    I_ROW("SHUKABASHONAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(出荷場所エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(出荷日：SHUKADATE)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKADATE", I_ROW("SHUKADATE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If Not isNormal(O_MESSAGE_NO) Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(出荷日エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(コンテナシャーシ：CONTCHASSIS)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CONTCHASSIS", I_ROW("CONTCHASSIS"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("CONTCHASSIS") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(コンテナシャーシエラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(トリップ：TRIPNO)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TRIPNO", I_ROW("TRIPNO"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("TRIPNO") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トリップエラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

        End If

        '----------------------
        '配送作業、配送ボタン（NJS専用ボタン）の場合の場合
        '----------------------
        If I_ROW("WORKKBN") = "BY" OrElse I_ROW("WORKKBN") = "G1" Then
            '★今は、配送作業の場合、口だけ開けてノーチェック（2017/9/13現在）
            '　入力があれば、マスターチェックのみ行う
            '②LeftBox存在チェック
            If I_ROW("TORICODE") <> "" Then
                CodeToName("TORICODE", I_ROW("TORICODE"), WW_TEXT, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("TORICODE") & ") "
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
                I_ROW("TORINAMES") = WW_TEXT
            End If
        End If

        '----------------------
        '荷卸しの場合
        '----------------------
        Dim WW_TORI_FLG As String = ""
        Dim WW_PROD_FLG As String = ""
        If I_ROW("WORKKBN") = "B3" Then
            '・キー項目(取引先：TORICODE)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TORICODE", I_ROW("TORICODE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                '②LeftBox存在チェック
                If I_ROW("TORICODE") <> "" Then
                    CodeToName("TORICODE", I_ROW("TORICODE"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("TORICODE") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    Else
                        WW_TORI_FLG = "OK"
                    End If
                    I_ROW("TORINAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(請求取引先：STORICODE)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STORICODE", I_ROW("STORICODE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If Not isNormal(O_MESSAGE_NO) Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(請求取引先エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If


            '・キー項目(届先：TODOKECODE)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", I_ROW("TODOKECODE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                '②LeftBox存在チェック
                If I_ROW("TODOKECODE") <> "" Then
                    CodeToName("TODOKECODE", I_ROW("TODOKECODE"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(届先エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("TODOKECODE") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                    I_ROW("TODOKENAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(届先エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(届日：TODOKEDATE)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKEDATE", I_ROW("TODOKEDATE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If Not isNormal(O_MESSAGE_NO) Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(届日エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(ドロップ：DROPNO)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DROPNO", I_ROW("DROPNO"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("DROPNO") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(ドロップエラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(油種１：OILTYPE1)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", I_ROW("OILTYPE1"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("OILTYPE1") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(油種１エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名１１：PRODUCT11)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", I_ROW("PRODUCT11"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT11") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名１１エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名２１：PRODUCT21)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", I_ROW("PRODUCT21"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT21") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名２１エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名コード１：PRODUCTCODE1)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", I_ROW("PRODUCTCODE1"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCTCODE1") = O_VALUE
                '②LeftBox存在チェック
                If Not String.IsNullOrEmpty(I_ROW("PRODUCTCODE1")) Then
                    Dim WW_PRODUCT As String = I_ROW("PRODUCTCODE1")
                    CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名１エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("PRODUCTCODE1") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    Else
                        WW_PROD_FLG = "OK"
                    End If
                    I_ROW("PRODUCT1NAMES") = WW_TEXT
                    CodeToName("STANI", WW_PRODUCT, WW_TEXT, WW_DUMMY)
                    I_ROW("STANI1") = WW_TEXT
                    CodeToName("STANINAMES", I_ROW("STANI1"), WW_TEXT, WW_DUMMY)
                    I_ROW("STANI1NAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名コード１エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(油種２：OILTYPE2)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", I_ROW("OILTYPE2"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("OILTYPE2") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(油種２エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名１２：PRODUCT12)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", I_ROW("PRODUCT12"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT12") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名１２エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名２２：PRODUCT22)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", I_ROW("PRODUCT22"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT22") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名２２エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名コード２：PRODUCTCODE2)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", I_ROW("PRODUCTCODE2"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCTCODE2") = O_VALUE
                '②LeftBox存在チェック
                If Not String.IsNullOrEmpty(I_ROW("PRODUCTCODE2")) Then
                    Dim WW_PRODUCT As String = I_ROW("PRODUCTCODE2")
                    CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名コード２エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("PRODUCTCODE2") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    Else
                        WW_PROD_FLG = "OK"
                    End If
                    I_ROW("PRODUCT2NAMES") = WW_TEXT
                    CodeToName("STANI", WW_PRODUCT, WW_TEXT, WW_DUMMY)
                    I_ROW("STANI2") = WW_TEXT
                    CodeToName("STANINAMES", I_ROW("STANI2"), WW_TEXT, WW_DUMMY)
                    I_ROW("STANI2NAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名コード２エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(油種３：OILTYPE3)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", I_ROW("OILTYPE3"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("OILTYPE3") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(油種３エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名１３：PRODUCT13)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", I_ROW("PRODUCT13"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT13") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名１３エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名２３：PRODUCT23)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", I_ROW("PRODUCT23"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT23") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名２３エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名コード３：PRODUCTCODE3)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", I_ROW("PRODUCTCODE3"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCTCODE3") = O_VALUE
                '②LeftBox存在チェック
                If Not String.IsNullOrEmpty(I_ROW("PRODUCTCODE3")) Then
                    Dim WW_PRODUCT As String = I_ROW("PRODUCTCODE3")
                    CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名コード３エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("PRODUCTCODE3") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    Else
                        WW_PROD_FLG = "OK"
                    End If
                    I_ROW("PRODUCT3NAMES") = WW_TEXT
                    CodeToName("STANI", WW_PRODUCT, WW_TEXT, WW_DUMMY)
                    I_ROW("STANI3") = WW_TEXT
                    CodeToName("STANINAMES", I_ROW("STANI3"), WW_TEXT, WW_DUMMY)
                    I_ROW("STANI3NAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名コード３エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(油種４：OILTYPE4)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", I_ROW("OILTYPE4"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("OILTYPE4") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(油種４エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名１４：PRODUCT14)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", I_ROW("PRODUCT14"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT14") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名１４エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名２４：PRODUCT24)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", I_ROW("PRODUCT24"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT24") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名２４エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名コード４：PRODUCTCODE4)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", I_ROW("PRODUCTCODE4"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCTCODE4") = O_VALUE
                '②LeftBox存在チェック
                If Not String.IsNullOrEmpty(I_ROW("PRODUCTCODE4")) Then
                    Dim WW_PRODUCT As String = I_ROW("PRODUCTCODE4")
                    CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名コード４エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("PRODUCTCODE4") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    Else
                        WW_PROD_FLG = "OK"
                    End If
                    I_ROW("PRODUCT4NAMES") = WW_TEXT
                    CodeToName("STANI", WW_PRODUCT, WW_TEXT, WW_DUMMY)
                    I_ROW("STANI4") = WW_TEXT
                    CodeToName("STANINAMES", I_ROW("STANI4"), WW_TEXT, WW_DUMMY)
                    I_ROW("STANI4NAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名コード４エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(油種５：OILTYPE5)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", I_ROW("OILTYPE5"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("OILTYPE5") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(油種５エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名１５：PRODUCT15)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", I_ROW("PRODUCT15"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT15") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名１５エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名２５：PRODUCT25)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", I_ROW("PRODUCT25"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT25") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名２５エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名コード５：PRODUCTCODE5)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", I_ROW("PRODUCTCODE5"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCTCODE5") = O_VALUE
                '②LeftBox存在チェック
                If Not String.IsNullOrEmpty(I_ROW("PRODUCTCODE5")) Then
                    Dim WW_PRODUCT As String = I_ROW("PRODUCTCODE5")
                    CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名コード５エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("PRODUCTCODE5") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    Else
                        WW_PROD_FLG = "OK"
                    End If
                    I_ROW("PRODUCT5NAMES") = WW_TEXT
                    CodeToName("STANI", WW_PRODUCT, WW_TEXT, WW_DUMMY)
                    I_ROW("STANI5") = WW_TEXT
                    CodeToName("STANINAMES", I_ROW("STANI5"), WW_TEXT, WW_DUMMY)
                    I_ROW("STANI5NAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名コード５エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(油種６：OILTYPE6)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", I_ROW("OILTYPE6"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("OILTYPE6") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(油種６エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名１６：PRODUCT16)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", I_ROW("PRODUCT16"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT16") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名１６エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名２６：PRODUCT26)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", I_ROW("PRODUCT26"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT26") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名２６エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名コード６：PRODUCTCODE6)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", I_ROW("PRODUCTCODE6"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCTCODE6") = O_VALUE
                '②LeftBox存在チェック
                If Not String.IsNullOrEmpty(I_ROW("PRODUCTCODE6")) Then
                    Dim WW_PRODUCT As String = I_ROW("PRODUCTCODE6")
                    CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名コード６エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("PRODUCTCODE6") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    Else
                        WW_PROD_FLG = "OK"
                    End If
                    I_ROW("PRODUCT6NAMES") = WW_TEXT
                    CodeToName("STANI", WW_PRODUCT, WW_TEXT, WW_DUMMY)
                    I_ROW("STANI6") = WW_TEXT
                    CodeToName("STANINAMES", I_ROW("STANI6"), WW_TEXT, WW_DUMMY)
                    I_ROW("STANI6NAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名コード６エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(油種７：OILTYPE7)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", I_ROW("OILTYPE7"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("OILTYPE7") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(油種７エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名１７：PRODUCT17)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", I_ROW("PRODUCT17"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT17") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名１７エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名２７：PRODUCT27)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", I_ROW("PRODUCT27"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT27") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名２７エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名コード７：PRODUCTCODE7)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", I_ROW("PRODUCTCODE7"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCTCODE7") = O_VALUE
                '②LeftBox存在チェック
                If Not String.IsNullOrEmpty(I_ROW("PRODUCTCODE7")) Then
                    Dim WW_PRODUCT As String = I_ROW("PRODUCTCODE7")
                    CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名コード７エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("PRODUCTCODE7") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    Else
                        WW_PROD_FLG = "OK"
                    End If
                    I_ROW("PRODUCT7NAMES") = WW_TEXT
                    CodeToName("STANI", WW_PRODUCT, WW_TEXT, WW_DUMMY)
                    I_ROW("STANI7") = WW_TEXT
                    CodeToName("STANINAMES", I_ROW("STANI7"), WW_TEXT, WW_DUMMY)
                    I_ROW("STANI7NAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名コード７エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(油種８：OILTYPE8)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", I_ROW("OILTYPE8"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("OILTYPE8") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(油種８エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名１８：PRODUCT18)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", I_ROW("PRODUCT18"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT18") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名１８エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名２８：PRODUCT28)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", I_ROW("PRODUCT28"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCT28") = O_VALUE
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名２８エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '・キー項目(品名コード８：PRODUCTCODE8)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", I_ROW("PRODUCTCODE8"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("PRODUCTCODE8") = O_VALUE
                '②LeftBox存在チェック
                If Not String.IsNullOrEmpty(I_ROW("PRODUCTCODE8")) Then
                    Dim WW_PRODUCT As String = I_ROW("PRODUCTCODE8")
                    CodeToName("PRODUCT2", WW_PRODUCT, WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名コード８エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("PRODUCTCODE8") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    Else
                        WW_PROD_FLG = "OK"
                    End If
                    I_ROW("PRODUCT8NAMES") = WW_TEXT
                    CodeToName("STANI", WW_PRODUCT, WW_TEXT, WW_DUMMY)
                    I_ROW("STANI8") = WW_TEXT
                    CodeToName("STANINAMES", I_ROW("STANI8"), WW_TEXT, WW_DUMMY)
                    I_ROW("STANI8NAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名コード８エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(数量１：SURYO1)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", I_ROW("SURYO1"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SURYO1") = Format(Val(O_VALUE), "#,0.000")

                If Val(O_VALUE) < 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量１エラー)です。"
                    WW_CheckMES2 = O_VALUE
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(数量１エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(数量２：SURYO2)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", I_ROW("SURYO2"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SURYO2") = Format(Val(O_VALUE), "#,0.000")

                If Val(O_VALUE) < 0 Then
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_CheckMES1 = "・更新できないレコード(数量２エラー)です。"
                    WW_CheckMES2 = O_VALUE
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_CheckMES1 = "・更新できないレコード(数量２エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(数量３：SURYO3)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", I_ROW("SURYO3"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SURYO3") = Format(Val(O_VALUE), "#,0.000")

                If Val(O_VALUE) < 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量３エラー)です。"
                    WW_CheckMES2 = O_VALUE
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(数量３エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(数量４：SURYO4)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", I_ROW("SURYO4"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SURYO4") = Format(Val(O_VALUE), "#,0.000")

                If Val(O_VALUE) < 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量４エラー)です。"
                    WW_CheckMES2 = O_VALUE
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(数量４エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(数量５：SURYO5)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", I_ROW("SURYO5"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SURYO5") = Format(Val(O_VALUE), "#,0.000")

                If Val(O_VALUE) < 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量５エラー)です。"
                    WW_CheckMES2 = O_VALUE
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(数量５エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(数量６：SURYO6)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", I_ROW("SURYO6"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SURYO6") = Format(Val(O_VALUE), "#,0.000")

                If Val(O_VALUE) < 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量６エラー)です。"
                    WW_CheckMES2 = O_VALUE
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(数量６エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(数量７：SURYO7)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", I_ROW("SURYO7"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SURYO7") = Format(Val(O_VALUE), "#,0.000")

                If Val(O_VALUE) < 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量７エラー)です。"
                    WW_CheckMES2 = O_VALUE
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(数量７エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(数量８：SURYO8)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", I_ROW("SURYO8"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("SURYO8") = Format(Val(O_VALUE), "#,0.000")

                If Val(O_VALUE) < 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量８エラー)です。"
                    WW_CheckMES2 = O_VALUE
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(数量８エラー)です。"
                OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            '・キー項目(税区分：TAXKBN)
            '①必須・項目属性チェック
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TAXKBN", I_ROW("TAXKBN"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
            If isNormal(O_MESSAGE_NO) Then
                I_ROW("TAXKBN") = O_VALUE
                '②LeftBox存在チェック
                If I_ROW("TAXKBN") <> "" Then
                    CodeToName("TAXKBN", I_ROW("TAXKBN"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(税区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("TAXKBN") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                    I_ROW("TAXKBNNAMES") = WW_TEXT
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(税区分エラー)です。"
                WW_CheckMES2 = O_CHECKREPORT
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If


            '②LeftBox存在チェック
            If I_ROW("URIKBN") <> "" Then
                '・キー項目(売上計上基準：URIKBN)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "URIKBN", I_ROW("URIKBN"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
                If Not isNormal(O_MESSAGE_NO) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(売上計上基準エラー)です。"
                    WW_CheckMES2 = O_CHECKREPORT
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
                CodeToName("URIKBN", I_ROW("URIKBN"), WW_TEXT, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(売上計上基準エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("URIKBN") & ") "
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
                I_ROW("URIKBNNAMES") = WW_TEXT
            Else
                If WW_TORI_FLG = "OK" AndAlso WW_PROD_FLG = "OK" Then
                    SetUriKbn(I_ROW, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集0
                        WW_CheckMES1 = "・更新できないレコード(売上計上基準エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("TORICODE") & ") "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                End If
            End If

        End If

        '・キー項目(開始メータ：STMATER)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STMATER", I_ROW("STMATER"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("STMATER") = Format(Val(O_VALUE), "#,0.00")

            If Val(O_VALUE) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(開始メータエラー)です。"
                WW_CheckMES2 = O_VALUE
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(開始メータエラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(終了メータ：ENDMATER)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDMATER", I_ROW("ENDMATER"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("ENDMATER") = Format(Val(O_VALUE), "#,0.00")

            If Val(O_VALUE) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(終了メータエラー)です。"
                WW_CheckMES2 = O_VALUE
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(終了メータエラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If


        '・キー項目(走行距離：SOUDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SOUDISTANCE", I_ROW("SOUDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("SOUDISTANCE") = Format(CInt(O_VALUE), "#,0.00")

            If Val(O_VALUE) > 700 AndAlso I_ROW("WORKKBN") <> "F3" AndAlso I_ROW("WORKKBN") <> "G1" Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(走行距離エラー)です。"
                WW_CheckMES2 = "走行距離が700キロを超過しています(" & O_VALUE & ") "
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            If Val(O_VALUE) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(走行距離エラー)です。"
                WW_CheckMES2 = O_VALUE
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(走行距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(累積走行距離：RUIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RUIDISTANCE", I_ROW("RUIDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("RUIDISTANCE") = Format(CInt(O_VALUE), "#,0.00")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(累積走行距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(実車距離：JIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JIDISTANCE", I_ROW("JIDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("JIDISTANCE") = Format(Val(O_VALUE), "#,0.00")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(実車距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(空車距離：KUDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KUDISTANCE", I_ROW("KUDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("KUDISTANCE") = Format(Val(O_VALUE), "#,0.00")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(空車距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(一般距離：IPPDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "IPPDISTANCE", I_ROW("IPPDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("IPPDISTANCE") = Format(Val(O_VALUE), "#,0.00")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(一般距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(高速距離：KOSDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOSDISTANCE", I_ROW("KOSDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("KOSDISTANCE") = Format(Val(O_VALUE), "#,0.00")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(高速距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(一般・実車距離：IPPJIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "IPPJIDISTANCE", I_ROW("IPPJIDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("IPPJIDISTANCE") = Format(Val(O_VALUE), "#,0.00")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(一般・実車距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(一般・空車距離：IPPJIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "IPPKUDISTANCE", I_ROW("IPPKUDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("IPPKUDISTANCE") = Format(Val(O_VALUE), "#,0.00")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(一般・空車距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(高速・実車距離：KOSJIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOSJIDISTANCE", I_ROW("KOSJIDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("KOSJIDISTANCE") = Format(Val(O_VALUE), "#,0.00")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(高速・実車距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(高速・空車距離：KOSKUDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOSKUDISTANCE", I_ROW("KOSKUDISTANCE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("KOSKUDISTANCE") = Format(Val(O_VALUE), "#,0.00")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(高速・空車距離エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(通行料・現金：CASH)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CASH", I_ROW("CASH"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("CASH") = Format(Val(O_VALUE), "#,0")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料・現金エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(通行料・ETC：ETC)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ETC", I_ROW("ETC"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("ETC") = Format(Val(O_VALUE), "#,0")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通通行料・ETCエラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(通行料・回数券：TICKET)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TICKET", I_ROW("TICKET"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("TICKET") = Format(Val(O_VALUE), "#,0")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料・回数券エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(通行料・プレート：PRATE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRATE", I_ROW("PRATE"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("PRATE") = Format(Val(O_VALUE), "#,0")
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料・プレートエラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(通行料：TOTALTOLL)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOTALTOLL", I_ROW("TOTALTOLL"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("TOTALTOLL") = Format(CInt(O_VALUE), "#,0")

            If Val(O_VALUE) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(通行料エラー)です。"
                WW_CheckMES2 = O_VALUE
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(給油：KYUYU)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KYUYU", I_ROW("KYUYU"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("KYUYU") = Format(Val(O_VALUE), "#,0.00")

            If I_ROW("WORKKBN") = "F3" Then
                If Val(O_VALUE) > 500 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(給油エラー)です。"
                    WW_CheckMES2 = "500ℓ超です(" & O_VALUE & ") "
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            End If
            If Val(O_VALUE) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(給油エラー)です。"
                WW_CheckMES2 = O_VALUE
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(給油エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '・キー項目(積置区分：TUMIOKIKBN)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TUMIOKIKBN", I_ROW("TUMIOKIKBN"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("TUMIOKIKBN") = O_VALUE
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(積置区分エラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If
        '・キー項目(削除フラグ：DELFLG)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DELFLG", I_ROW("DELFLG"), O_MESSAGE_NO, O_CHECKREPORT, O_VALUE, S0013tbl)
        If isNormal(O_MESSAGE_NO) Then
            I_ROW("DELFLG") = O_VALUE
            '②LeftBox存在チェック
            If I_ROW("DELFLG") <> "" Then
                CodeToName("DELFLG", I_ROW("DELFLG"), WW_TEXT, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(削除エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。(" & I_ROW("DELFLG") & ") "
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード((削除フラグエラー)です。"
            WW_CheckMES2 = O_CHECKREPORT
            OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
        End If

        '■■■ 関連チェック■■■

        '日付大小比較チェック
        If I_ROW("STDATE") <> "" AndAlso I_ROW("STTIME") <> "" AndAlso I_ROW("ENDDATE") <> "" AndAlso I_ROW("ENDTIME") <> "" Then
            If I_ROW("STDATE") > I_ROW("ENDDATE") Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(開始日付 ＞ 終了日付)です。"
                OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If

            If WW_DATE_ERR = "OFF" Then
                If CDate(I_ROW("STDATE") & " " & I_ROW("STTIME")) >
                   CDate(I_ROW("ENDDATE") & " " & I_ROW("ENDTIME")) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(開始時刻 ＞ 終了時刻)です。"
                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                End If
            End If
        End If

        '荷卸
        If I_ROW("WORKKBN") = "B3" Then
            If I_ROW("TODOKEDATE") = "" Then
                WW_CheckMES1 = "・更新できないレコード(届日エラー)です。"
                WW_CheckMES2 = "届日未入力"
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            Else
                If I_ROW("WORKKBN") = "B3" Then
                    If I_ROW("TODOKEDATE") < I_ROW("YMD") Then
                        WW_CheckMES1 = "・更新できないレコード(届日エラー)です。"
                        WW_CheckMES2 = "出庫日 ＞ 届日です  "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If
                End If
                If I_ROW("WORKKBN") = "B2" AndAlso I_ROW("TUMIOKIKBN") = "1" Then
                    If I_ROW("TODOKEDATE") <= I_ROW("YMD") Then
                        WW_CheckMES1 = "・更新できないレコード(届日エラー)です。"
                        WW_CheckMES2 = "積置は、翌日以降を入力 "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    End If

                End If
            End If

            If I_ROW("PRODUCTCODE1") = "" AndAlso
               I_ROW("PRODUCTCODE2") = "" AndAlso
               I_ROW("PRODUCTCODE3") = "" AndAlso
               I_ROW("PRODUCTCODE4") = "" AndAlso
               I_ROW("PRODUCTCODE5") = "" AndAlso
               I_ROW("PRODUCTCODE6") = "" AndAlso
               I_ROW("PRODUCTCODE7") = "" AndAlso
               I_ROW("PRODUCTCODE8") = "" Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(品名エラー)です。"
                WW_CheckMES2 = "品名未入力（） "
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
            '荷卸
            If Val(I_ROW("SURYO1")) = 0 AndAlso
                Val(I_ROW("SURYO2")) = 0 AndAlso
                Val(I_ROW("SURYO3")) = 0 AndAlso
                Val(I_ROW("SURYO4")) = 0 AndAlso
                Val(I_ROW("SURYO5")) = 0 AndAlso
                Val(I_ROW("SURYO6")) = 0 AndAlso
                Val(I_ROW("SURYO7")) = 0 AndAlso
                Val(I_ROW("SURYO8")) = 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(数量エラー)です。"
                WW_CheckMES2 = "荷卸数量未入力（） "
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, I_ROW, C_MESSAGE_NO.BOX_ERROR_EXIST)
            End If
        End If

        If WW_ERRLIST.Count > 0 Then
            If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                I_ROW("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            ElseIf WW_ERRLIST.IndexOf(C_MESSAGE_NO.BOX_ERROR_EXIST) >= 0 Then
                O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
                I_ROW("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            Else
                O_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST
                I_ROW("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
        Else
            I_ROW("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

    End Sub

    ''' <summary>
    '''  GridViewの更新（Excel）
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateGridDataOnExcel(ByRef O_RTN As String)

        Dim WW_UMU As Integer = 0

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '○テーブルデータ 復元（GridView）
            'テーブルデータ 復元(TEXTファイルより復元)
            Master.XMLsaveF = work.WF_SEL_XMLsaveF.Text
            If Not Master.RecoverTable(T0005tbl) Then
                O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                Exit Sub
            End If
            '○データリカバリ（一週間前データ）
            Master.XMLsaveF = work.WF_SEL_XMLsaveF9.Text
            If Not Master.RecoverTable(T0005WEEKtbl) Then
                O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                Exit Sub
            End If

            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.FILTER = ""
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            T0005tbl = CS0026TBLSORT.sort()
            '-----------------------------------------------------------------------------------
            '差分データ（取込）とT0005tbl（GridView）を比較し、該当データが存在すれば上書き
            '-----------------------------------------------------------------------------------
            Dim WW_IDX As Integer = 0
            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                If T0005INPtbl.Rows(i)("HDKBN") = "H" Then
                    Dim WW_TIMSTP As Integer = 0
                    For j As Integer = 0 To T0005tbl.Rows.Count - 1
                        If T0005tbl.Rows(j)("YMD") = T0005INPtbl.Rows(i)("YMD") AndAlso
                           T0005tbl.Rows(j)("STAFFCODE") = T0005INPtbl.Rows(i)("STAFFCODE") AndAlso
                           T0005tbl.Rows(j)("SELECT") = "1" AndAlso
                           T0005tbl.Rows(j)("HDKBN") = "H" Then
                            WW_TIMSTP = T0005tbl.Rows(j)("TIMSTP")
                            Exit For
                        End If
                    Next
                    If WW_TIMSTP = 0 Then
                        For j As Integer = T0005tbl.Rows.Count - 1 To 0 Step -1
                            If T0005tbl.Rows(j)("YMD") = T0005INPtbl.Rows(i)("YMD") AndAlso
                               T0005tbl.Rows(j)("STAFFCODE") = T0005INPtbl.Rows(i)("STAFFCODE") AndAlso
                               T0005tbl.Rows(j)("SELECT") = "1" Then
                                '前回データ削除
                                T0005tbl.Rows(j).Delete()
                            End If
                        Next
                    Else
                        For Each UpdRow As DataRow In T0005tbl.Rows
                            If UpdRow("YMD") = T0005INPtbl.Rows(i)("YMD") AndAlso
                               UpdRow("STAFFCODE") = T0005INPtbl.Rows(i)("STAFFCODE") AndAlso
                               UpdRow("SELECT") = "1" Then
                                '前回データ削除
                                UpdRow("LINECNT") = "0"
                                UpdRow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                                UpdRow("SELECT") = "0"
                                UpdRow("HIDDEN") = "1"
                                UpdRow("DELFLG") = C_DELETE_FLG.DELETE
                            End If
                        Next
                    End If
                End If
            Next

            T0005tbl.Merge(T0005INPtbl)

            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.FILTER = ""
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            T0005tbl = CS0026TBLSORT.sort()

            '------------------------------------------------------------
            '■出庫日、従業員 単位
            '  取込んだ日報番号毎のヘッダを出庫日、従業員毎に集約し直す
            '------------------------------------------------------------
            CreateT0005Header(T0005tbl)

            '------------------------------------------------------------
            '■マージ後のチェック
            '------------------------------------------------------------
            CheckOrderOnExcel(T0005tbl, O_RTN)
            If Not isNormal(O_RTN) Then Exit Sub

            Dim WW_SEQ As Integer = 0
            Dim WW_LINECNT As Integer = 0

            '行番号の採番
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                If T0005tbl.Rows(i)("SELECT") = "1" Then
                    If T0005tbl.Rows(i)("HDKBN") = "H" Then
                        WW_SEQ = 1
                        T0005tbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")

                        WW_LINECNT = WW_LINECNT + 1
                        T0005tbl.Rows(i)("LINECNT") = WW_LINECNT
                        T0005tbl.Rows(i)("SELECT") = "1"
                        T0005tbl.Rows(i)("HIDDEN") = "0"
                    Else
                        T0005tbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")
                        WW_SEQ = WW_SEQ + 1

                        T0005tbl.Rows(i)("LINECNT") = 0
                        T0005tbl.Rows(i)("SELECT") = "1"
                        T0005tbl.Rows(i)("HIDDEN") = "1"
                    End If
                End If
            Next

            '明細にエラーがある場合、ヘッダにエラーを設定する
            Dim WW_HeadIdx As Integer = 0
            Dim WW_ERR_FLG As Boolean = False
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                If T0005tbl.Rows(i)("HDKBN") = "H" Then
                    WW_ERR_FLG = False
                    WW_HeadIdx = i
                End If
                '次のヘッダまで
                For j As Integer = i + 1 To T0005tbl.Rows.Count - 1
                    If T0005tbl.Rows(j)("HDKBN") = "H" Then
                        i = j - 1
                        Exit For
                    End If
                    If T0005tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then WW_ERR_FLG = True
                Next
                If WW_ERR_FLG Then T0005tbl.Rows(WW_HeadIdx)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

            Next

            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                If T0005tbl.Rows(i)("SELECT") = "1" Then
                    If T0005tbl.Rows(i)("HDKBN") = "H" Then
                        '次のヘッダまで
                        For j As Integer = i + 1 To T0005tbl.Rows.Count - 1
                            If T0005tbl.Rows(j)("HDKBN") = "H" Then
                                i = j - 1
                                Exit For
                            End If
                            T0005tbl.Rows(j)("OPERATION") = T0005tbl.Rows(i)("OPERATION")
                        Next
                    End If
                End If
            Next

            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.FILTER = "HDKBN = 'H'"
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN"
            Dim WW_T0005tbl As DataTable = CS0026TBLSORT.sort()

            For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                Dim WW_ERRWORD As String = ""
                WW_ERRWORD = rightview.GetErrorReport.Replace("@L" & WW_T0005tbl(i)("YMD") & WW_T0005tbl(i)("STAFFCODE") & "L@", WW_T0005tbl(i)("LINECNT"))
                rightview.SetErrorReport(WW_ERRWORD)
            Next

            '○GridViewデータをテーブルに保存
            If Master.SaveTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

            '○GridViewデータをテーブルに保存（一週間前データ）
            If Master.SaveTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

            '絞込みボタン処理（GridViewの表示）を行う
            WF_ButtonExtract_Click()


        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "Grid_UpdateExcel")
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

    ''' <summary>
    ''' T0005tbl全体関連チェック
    ''' </summary>
    ''' <param name="IO_TBL">チェック対象</param>
    ''' <param name="O_ERRCD">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckAllT0005tbl(ByRef IO_TBL As DataTable, ByRef O_ERRCD As String)
        Dim WW_WORKTIME As Integer = 0
        Dim WW_MOVETIME As Integer = 0

        Dim WW_STDATE As DateTime
        Dim WW_ENDDATE As DateTime
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '---------------------------------------------------
        '■明細行番号（並び順）の振り直し()
        '---------------------------------------------------
        Dim WW_SEQ As Integer = 0
        Dim WW_ERRFLG As Boolean = False
        Dim WW_B2CNT As Integer = 0
        Dim WW_B3CNT As Integer = 0
        Dim WW_F1CNT As Integer = 0
        Dim WW_F3CNT As Integer = 0
        Dim WW_A1CNT As Integer = 0
        Dim WW_Z1CNT As Integer = 0
        Dim WW_LINEerr As String = C_MESSAGE_NO.NORMAL

        O_ERRCD = C_MESSAGE_NO.NORMAL

        '始業、終業を除く（退避）
        CS0026TBLSORT.TABLE = IO_TBL
        CS0026TBLSORT.FILTER = "HDKBN = 'H' or (WORKKBN = 'A1' or WORKKBN = 'G1' or WORKKBN = 'Z1')"
        CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        Dim WW_A1Z1tbl As DataTable = CS0026TBLSORT.sort()

        CS0026TBLSORT.FILTER = "HDKBN = 'D' and WORKKBN <> 'A1' and WORKKBN <> 'G1' and WORKKBN <> 'Z1'"
        IO_TBL = CS0026TBLSORT.sort()

        'ヘッダーレコードは、1レコード目（0番）、明細レコードは、2レコード目（1番～最後）が時系列であること前提
        WW_SEQ = 0
        WW_ERRFLG = False
        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            Dim IO_TBLrow As DataRow = IO_TBL.Rows(i)

            WW_LINEerr = C_MESSAGE_NO.NORMAL

            Select Case IO_TBLrow("WORKKBN")
                Case "B2"
                    WW_B2CNT += 1
                Case "B3"
                    WW_B3CNT += 1
                Case "F1"
                    WW_F1CNT += 1
                Case "F3"
                    WW_F3CNT += 1
            End Select

            '前後の日時大小関係

            WW_STDATE = IO_TBL.Rows(i)("STDATE") + " " + IO_TBL.Rows(i)("STTIME")
            WW_ENDDATE = IO_TBL.Rows(i)("ENDDATE") + " " + IO_TBL.Rows(i)("ENDTIME")
            If WW_ENDDATE < WW_STDATE Then
                'エラーレポート編集
                WW_CheckMES1 = "・エラーが存在します。(開始日付＞終了日付エラー)"
                OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.BOX_ERROR_EXIST)
                IO_TBL.Rows(i - 1)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                O_ERRCD = C_MESSAGE_NO.BOX_ERROR_EXIST
            End If

            If i >= 1 Then

                WW_ENDDATE = IO_TBL.Rows(i - 1)("ENDDATE") + " " + IO_TBL.Rows(i - 1)("ENDTIME")
                WW_STDATE = IO_TBLrow("STDATE") + " " + IO_TBLrow("STTIME")
                If WW_ENDDATE > WW_STDATE Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・エラーが存在します。(終了日付＞開始日付エラー)"
                    OutputErrorMessage(WW_CheckMES1, "", WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.BOX_ERROR_EXIST)
                    IO_TBL.Rows(i - 1)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    O_ERRCD = C_MESSAGE_NO.BOX_ERROR_EXIST
                End If

                '移動時間
                WW_MOVETIME = DateDiff("n",
                                      IO_TBL.Rows(i - 1)("ENDDATE") + " " + IO_TBL.Rows(i - 1)("ENDTIME"),
                                      IO_TBLrow("STDATE") + " " + IO_TBLrow("STTIME")
                                     )
                IO_TBLrow("MOVETIME") = T0005COM.MinutestoHHMM(WW_MOVETIME)

            End If

            '作業時間
            WW_WORKTIME = DateDiff("n",
                                  IO_TBLrow("STDATE") + " " + IO_TBLrow("STTIME"),
                                  IO_TBLrow("ENDDATE") + " " + IO_TBLrow("ENDTIME")
                                 )
            IO_TBLrow("WORKTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
            '稼働時間
            Dim WW_ACT As Integer = WW_WORKTIME + WW_MOVETIME
            IO_TBLrow("ACTTIME") = T0005COM.MinutestoHHMM(WW_ACT)

            If IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then WW_ERRFLG = True
        Next

        WW_LINEerr = C_MESSAGE_NO.NORMAL

        If IO_TBL.Rows.Count > 0 Then
            'ヘッダレコード編集
            If WW_ERRFLG Then IO_TBL.Rows(0)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            ''稼働時間計算
            WW_STDATE = IO_TBL.Rows(0)("STDATE") & " " & IO_TBL.Rows(0)("STTIME")
            WW_ENDDATE = IO_TBL.Rows(IO_TBL.Rows.Count - 1)("ENDDATE") & " " & IO_TBL.Rows(IO_TBL.Rows.Count - 1)("ENDTIME")
            WW_WORKTIME = DateDiff("n", WW_STDATE, WW_ENDDATE)

            '48時間（2日）以上の勤務はエラー（最終レコードをエラーとする）
            If WW_WORKTIME > 2880 Then
                'エラーレポート編集
                WW_CheckMES1 = "・エラーが存在します。(稼働時間エラー)"
                WW_CheckMES2 = "稼働時間が４８時間を超過しています。  "
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBL.Rows(0), C_MESSAGE_NO.BOX_ERROR_EXIST)
                IO_TBL.Rows(IO_TBL.Rows.Count - 1)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                O_ERRCD = C_MESSAGE_NO.BOX_ERROR_EXIST
            End If
        End If


        For i As Integer = 0 To WW_A1Z1tbl.Rows.Count - 1
            Dim IO_TBLrow As DataRow = WW_A1Z1tbl.Rows(i)
            If IO_TBLrow("WORKKBN") = "A1" AndAlso IO_TBLrow("CTRL") <> "OFF" Then
                If IsNothing(S0013tbl) Then S0013tbl = New DataTable
                If Master.ExistCheckTable(work.WF_SEL_CAMPCODE.Text, "STTIMEA1", S0013tbl) Then
                    Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
                    Dim WW_CHECKREPORT As String = String.Empty
                    Dim WW_OVALUE As String = String.Empty
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIMEA1", IO_TBLrow("STTIME"), WW_RTN, WW_CHECKREPORT, WW_OVALUE, S0013tbl)
                    If Not isNormal(WW_RTN) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・エラーが存在します。(開始時刻エラー)です。"
                        OutputErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                        O_ERRCD = C_MESSAGE_NO.BOX_ERROR_EXIST
                    End If
                End If

                If IO_TBLrow("CTRL") = "ON2" Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・警告が存在します。(出庫時刻＝始業時刻)です。"
                    WW_CheckMES2 = "始業時刻が正しいか確認してください。(" & IO_TBLrow("STTIME") & ") "
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                    IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    O_ERRCD = C_MESSAGE_NO.WORNING_RECORD_EXIST
                End If
            End If

            Select Case IO_TBLrow("WORKKBN")
                Case "A1"
                    WW_A1CNT += 1
                Case "Z1"
                    WW_Z1CNT += 1
            End Select
        Next

        If IO_TBL.Rows.Count > 0 Then
            If WW_B2CNT > 0 OrElse WW_B3CNT > 0 Then
                If WW_F1CNT = 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・エラーが存在します。(作業区分エラー)です。"
                    WW_CheckMES2 = "出庫（F1）が存在しません"
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBL.Rows(0), C_MESSAGE_NO.BOX_ERROR_EXIST)
                    IO_TBL.Rows(0)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    O_ERRCD = C_MESSAGE_NO.BOX_ERROR_EXIST
                End If

                If WW_F3CNT = 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・エラーが存在します。(作業区分エラー)です。"
                    WW_CheckMES2 = "帰庫（F3）が存在しません"
                    OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBL.Rows(0), C_MESSAGE_NO.BOX_ERROR_EXIST)
                    IO_TBL.Rows(0)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    O_ERRCD = C_MESSAGE_NO.BOX_ERROR_EXIST
                End If
            End If

            If WW_A1CNT = 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・エラーが存在します。(作業区分エラー)です。"
                WW_CheckMES2 = "始業（A1）が存在しません"
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBL.Rows(0), C_MESSAGE_NO.BOX_ERROR_EXIST)
                IO_TBL.Rows(0)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                O_ERRCD = C_MESSAGE_NO.BOX_ERROR_EXIST
            End If

            If WW_Z1CNT = 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・エラーが存在します。(作業区分エラー)です。"
                WW_CheckMES2 = "終業（Z1）が存在しません"
                OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBL.Rows(0), C_MESSAGE_NO.BOX_ERROR_EXIST)
                IO_TBL.Rows(0)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                O_ERRCD = C_MESSAGE_NO.BOX_ERROR_EXIST
            End If
        End If

        IO_TBL.Merge(WW_A1Z1tbl)

        CS0026TBLSORT.TABLE = IO_TBL
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
        IO_TBL = CS0026TBLSORT.sort()

        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            Dim IO_TBLrow As DataRow = IO_TBL.Rows(i)
            WW_SEQ = WW_SEQ + 1
            IO_TBLrow("SEQ") = WW_SEQ.ToString("000")
            IO_TBLrow("LINECNT") = WW_SEQ
        Next

        WW_A1Z1tbl.Clear()
        WW_A1Z1tbl = Nothing

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    '共通処理処理
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' 配送受注チェック
    ''' </summary>
    ''' <param name="IO_TBL">チェック対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckOrderData(ByRef IO_TBL As DataTable, ByRef O_RTN As String)

        Dim WW_LINEerr As String = C_MESSAGE_NO.NORMAL
        Dim WW_SaveIdx As Integer = 0
        Dim IO_TBLrow As DataRow
        Dim WW_CONVERT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        O_RTN = C_MESSAGE_NO.NORMAL

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

                SetOrderNoFromT0004(IO_TBLrow, WW_ERRCODE)
                'エラー時
                If Not isNormal(WW_ERRCODE) Then
                    If O_RTN = WW_ERRCODE Then Exit Sub

                    If IO_TBLrow("TODOKEDATE") = "" Then
                        WW_CheckMES1 = "・更新できないレコード(届日エラー)です。"
                        WW_CheckMES2 = "届日未入力  "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                    If IO_TBLrow("TODOKECODE") = "" Then
                        WW_CheckMES1 = "・更新できないレコード(届先エラー)です。"
                        WW_CheckMES2 = "届先未入力 "
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If

                    If IO_TBLrow("PRODUCTCODE1") = "" AndAlso
                       IO_TBLrow("PRODUCTCODE2") = "" AndAlso
                       IO_TBLrow("PRODUCTCODE3") = "" AndAlso
                       IO_TBLrow("PRODUCTCODE4") = "" AndAlso
                       IO_TBLrow("PRODUCTCODE5") = "" AndAlso
                       IO_TBLrow("PRODUCTCODE6") = "" AndAlso
                       IO_TBLrow("PRODUCTCODE7") = "" AndAlso
                       IO_TBLrow("PRODUCTCODE8") = "" Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名エラー)です。"
                        WW_CheckMES2 = "品名未入力（）"
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
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
                        OutputErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, IO_TBLrow, C_MESSAGE_NO.BOX_ERROR_EXIST)
                        IO_TBLrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                End If
                '届先
                IO_TBLrow("TODOKENAMES") = ""
                CodeToName("TODOKECODE", IO_TBLrow("TODOKECODE"), IO_TBLrow("TODOKENAMES"), WW_DUMMY)

                If IO_TBLrow("ORDERORG") = "" Then
                    IO_TBLrow("ORDERORG") = IO_TBLrow("SHIPORG")
                End If

                Dim WW_PRODUCT As String = ""
                WW_PRODUCT = IO_TBLrow("PRODUCTCODE1")
                IO_TBLrow("STANI1") = ""
                CodeToName("STANI", WW_PRODUCT, IO_TBLrow("STANI1"), WW_DUMMY)
                IO_TBLrow("STANI1NAMES") = ""
                CodeToName("STANINAMES", IO_TBLrow("STANI1"), IO_TBLrow("STANI1NAMES"), WW_DUMMY)
                WW_PRODUCT = IO_TBLrow("PRODUCTCODE2")
                IO_TBLrow("STANI2") = ""
                CodeToName("STANI", WW_PRODUCT, IO_TBLrow("STANI2"), WW_DUMMY)
                IO_TBLrow("STANI2NAMES") = ""
                CodeToName("STANINAMES", IO_TBLrow("STANI2"), IO_TBLrow("STANI2NAMES"), WW_DUMMY)
                WW_PRODUCT = IO_TBLrow("PRODUCTCODE3")
                IO_TBLrow("STANI3") = ""
                CodeToName("STANI", WW_PRODUCT, IO_TBLrow("STANI3"), WW_DUMMY)
                IO_TBLrow("STANI3NAMES") = ""
                CodeToName("STANINAMES", IO_TBLrow("STANI3"), IO_TBLrow("STANI3NAMES"), WW_DUMMY)
                WW_PRODUCT = IO_TBLrow("PRODUCTCODE4")
                IO_TBLrow("STANI4") = ""
                CodeToName("STANI", WW_PRODUCT, IO_TBLrow("STANI4"), WW_DUMMY)
                IO_TBLrow("STANI4NAMES") = ""
                CodeToName("STANINAMES", IO_TBLrow("STANI4"), IO_TBLrow("STANI4NAMES"), WW_DUMMY)
                WW_PRODUCT = IO_TBLrow("PRODUCTCODE5")
                IO_TBLrow("STANI5") = ""
                CodeToName("STANI", WW_PRODUCT, IO_TBLrow("STANI5"), WW_DUMMY)
                IO_TBLrow("STANI5NAMES") = ""
                CodeToName("STANINAMES", IO_TBLrow("STANI5"), IO_TBLrow("STANI5NAMES"), WW_DUMMY)
                WW_PRODUCT = IO_TBLrow("PRODUCTCODE6")
                IO_TBLrow("STANI6") = ""
                CodeToName("STANI", WW_PRODUCT, IO_TBLrow("STANI6"), WW_DUMMY)
                IO_TBLrow("STANI6NAMES") = ""
                CodeToName("STANINAMES", IO_TBLrow("STANI6"), IO_TBLrow("STANI6NAMES"), WW_DUMMY)
                WW_PRODUCT = IO_TBLrow("PRODUCTCODE7")
                IO_TBLrow("STANI7") = ""
                CodeToName("STANI", WW_PRODUCT, IO_TBLrow("STANI7"), WW_DUMMY)
                IO_TBLrow("STANI7NAMES") = ""
                CodeToName("STANINAMES", IO_TBLrow("STANI7"), IO_TBLrow("STANI7NAMES"), WW_DUMMY)
                WW_PRODUCT = IO_TBLrow("PRODUCTCODE8")
                IO_TBLrow("STANI8") = ""
                CodeToName("STANI", WW_PRODUCT, IO_TBLrow("STANI8"), WW_DUMMY)
                IO_TBLrow("STANI8NAMES") = ""
                CodeToName("STANINAMES", IO_TBLrow("STANI8"), IO_TBLrow("STANI8NAMES"), WW_DUMMY)

                If IO_TBLrow("TAXKBN") = "" Then
                    IO_TBLrow("TAXKBN") = "0"
                End If
                IO_TBLrow("TAXKBNNAMES") = ""
                CodeToName("TAXKBN", IO_TBLrow("TAXKBN"), IO_TBLrow("TAXKBNNAMES"), WW_DUMMY)

                '荷積に受注番号、統一車番を設定
                For j As Integer = i - 1 To 0 Step -1
                    Dim WW_row As DataRow = IO_TBL.Rows(j)
                    If WW_row("YMD") = IO_TBLrow("YMD") AndAlso
                       WW_row("STAFFCODE") = IO_TBLrow("STAFFCODE") AndAlso
                       WW_row("NIPPONO") = IO_TBLrow("NIPPONO") Then
                        If WW_row("WORKKBN") = "B2" Then
                            WW_row("SHARYOTYPEF") = IO_TBLrow("SHARYOTYPEF")
                            WW_row("TSHABANF") = IO_TBLrow("TSHABANF")
                            WW_row("SHARYOTYPEB") = IO_TBLrow("SHARYOTYPEB")
                            WW_row("TSHABANB") = IO_TBLrow("TSHABANB")
                            WW_row("SHARYOTYPEB2") = IO_TBLrow("SHARYOTYPEB2")
                            WW_row("TSHABANB2") = IO_TBLrow("TSHABANB2")
                            Exit For
                        End If
                    Else
                        Exit For
                    End If
                Next
            End If

            '明細行に無があれば、ヘッダー行をエラーとする
            If IO_TBLrow("ORDERUMU") = "無" Then
                IO_TBLrow("ORDERUMU") = ""
                IO_TBL.Rows(WW_SaveIdx)("ORDERUMU") = "無"
            End If
        Next

    End Sub

    ''' <summary>
    ''' 配送受注チェック（EXCEL用）
    ''' </summary>
    ''' <param name="IO_TBL">チェック対象</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckOrderOnExcel(ByRef IO_TBL As DataTable, ByRef O_RTN As String)

        Dim WW_SaveIdx As Integer = 0
        Dim iTblrow As DataRow

        O_RTN = C_MESSAGE_NO.NORMAL

        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            iTblrow = IO_TBL.Rows(i)

            '画面表示対象外データ（更新前データ）は、読み飛ばし
            If iTblrow("SELECT") = "0" Then Continue For
            'ヘッダー行は一時保存
            If iTblrow("HDKBN") = "H" Then WW_SaveIdx = i

            '配送受注ＤＢの存在チェック
            '荷卸
            If iTblrow("WORKKBN") = "B3" Then
                SetShabanCodeFromT0004(iTblrow, WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    'エラーレポート編集
                    If WW_ERRCODE = C_MESSAGE_NO.DB_ERROR Then
                        O_RTN = WW_ERRCODE
                        Exit Sub
                    End If
                End If

                '荷積に受注番号を設定
                For j As Integer = i - 1 To 0 Step -1
                    If IO_TBL.Rows(j)("YMD") = iTblrow("YMD") AndAlso
                       IO_TBL.Rows(j)("STAFFCODE") = iTblrow("STAFFCODE") AndAlso
                       IO_TBL.Rows(j)("NIPPONO") = iTblrow("NIPPONO") Then
                        If IO_TBL.Rows(j)("WORKKBN") = "B2" Then
                            IO_TBL.Rows(j)("ORDERNO") = iTblrow("ORDERNO")
                            IO_TBL.Rows(j)("TRIPNO") = iTblrow("TRIPNO")
                            Exit For
                        End If
                    Else
                        Exit For
                    End If
                Next
            End If

            '明細行に無があれば、ヘッダー行をエラーとする
            If iTblrow("ORDERUMU") = "無" Then
                iTblrow("ORDERUMU") = ""
                IO_TBL.Rows(WW_SaveIdx)("ORDERUMU") = "無"
            End If
        Next

    End Sub

    ''' <summary>
    '''  売上区分取得設定
    ''' </summary>
    ''' <param name="IO_ROW">対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub SetUriKbn(ByRef IO_ROW As DataRow, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        If IO_ROW("WORKKBN") = "B3" Then
            GS0029T3CNTLget.CAMPCODE = IO_ROW("CAMPCODE")
            GS0029T3CNTLget.TORICODE = IO_ROW("TORICODE")
            GS0029T3CNTLget.OILTYPE = IO_ROW("OILTYPE1")
            GS0029T3CNTLget.ORDERORG = IO_ROW("SHIPORG")
            GS0029T3CNTLget.KIJUNDATE = IO_ROW("YMD")
            GS0029T3CNTLget.GS0029T3CNTLget()
            If isNormal(GS0029T3CNTLget.ERR) Then
                IO_ROW("URIKBN") = GS0029T3CNTLget.URIKBN
            Else
                O_RTN = GS0029T3CNTLget.ERR
                WW_ERRLIST_ALL.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                WW_ERRLIST.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                IO_ROW("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        End If

    End Sub

    ''' <summary>
    ''' 配送受注ＤＢを検索し、受注番号と車両情報を取得する
    ''' </summary>
    ''' <param name="IO_ROW">対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub SetOrderNoFromT0004(ByRef IO_ROW As DataRow, ByRef O_RTN As String)

        Dim SQLStr As String = ""
        Dim SQLWhere As String = ""
        Dim SQLSort As String = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        Try
            Using WW_T0004tbl As New DataTable

                WW_T0004tbl.Columns.Add("ORDERNO", GetType(String))
                WW_T0004tbl.Columns.Add("SHARYOTYPEF", GetType(String))
                WW_T0004tbl.Columns.Add("TSHABANF", GetType(String))
                WW_T0004tbl.Columns.Add("SHARYOTYPEB", GetType(String))
                WW_T0004tbl.Columns.Add("TSHABANB", GetType(String))
                WW_T0004tbl.Columns.Add("SHARYOTYPEB2", GetType(String))
                WW_T0004tbl.Columns.Add("TSHABANB2", GetType(String))
                WW_T0004tbl.Columns.Add("TAXKBN", GetType(String))
                WW_T0004tbl.Columns.Add("ORDERORG", GetType(String))

                'DataBase接続文字
                Using SQLcon As SqlConnection = CS0050Session.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    SQLStr =
                               " SELECT isnull(rtrim(ORDERNO), '')     as ORDERNO " _
                             & "       ,isnull(rtrim(SHARYOTYPEF),'')  as SHARYOTYPEF " _
                             & "       ,isnull(rtrim(TSHABANF),'')     as TSHABANF " _
                             & "       ,isnull(rtrim(SHARYOTYPEB),'')  as SHARYOTYPEB " _
                             & "       ,isnull(rtrim(TSHABANB),'')     as TSHABANB " _
                             & "       ,isnull(rtrim(SHARYOTYPEB2),'') as SHARYOTYPEB2 " _
                             & "       ,isnull(rtrim(TSHABANB2),'')    as TSHABANB2 " _
                             & "       ,isnull(rtrim(TAXKBN),'')       as TAXKBN " _
                             & "       ,isnull(rtrim(ORDERORG),'')     as ORDERORG " _
                             & "   FROM T0004_HORDER " _
                             & "     WHERE    CAMPCODE        = @P01 " _
                             & "       and    SHIPORG         = @P02 " _
                             & "       and    TRIPNO          = @P03 " _
                             & "       and    DROPNO          = @P04 " _
                             & "       and    GSHABAN         = @P05 " _
                             & "       and    SHUKODATE       = @P06 " _
                             & "       and    DELFLG         <> '1'  " _
                             & " ORDER BY TRIPNO, DROPNO, SEQ"

                    SQLStr = SQLStr & SQLWhere & SQLSort

                    Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)

                        PARA1.Value = IO_ROW("CAMPCODE")
                        PARA2.Value = IO_ROW("SHIPORG")
                        PARA3.Value = IO_ROW("TRIPNO")
                        PARA4.Value = IO_ROW("DROPNO")
                        PARA5.Value = IO_ROW("GSHABAN")
                        PARA6.Value = IO_ROW("YMD")

                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            WW_T0004tbl.Load(SQLdr)

                            If WW_T0004tbl.Rows.Count > 0 Then
                                IO_ROW("ORDERNO") = WW_T0004tbl.Rows(0)("ORDERNO")
                                IO_ROW("SHARYOTYPEF") = WW_T0004tbl.Rows(0)("SHARYOTYPEF")
                                IO_ROW("TSHABANF") = WW_T0004tbl.Rows(0)("TSHABANF")
                                IO_ROW("SHARYOTYPEB") = WW_T0004tbl.Rows(0)("SHARYOTYPEB")
                                IO_ROW("TSHABANB") = WW_T0004tbl.Rows(0)("TSHABANB")
                                IO_ROW("SHARYOTYPEB2") = WW_T0004tbl.Rows(0)("SHARYOTYPEB2")
                                IO_ROW("TSHABANB2") = WW_T0004tbl.Rows(0)("TSHABANB2")
                                IO_ROW("TAXKBN") = WW_T0004tbl.Rows(0)("TAXKBN")
                                IO_ROW("ORDERORG") = WW_T0004tbl.Rows(0)("ORDERORG")
                            Else
                                IO_ROW("ORDERNO") = C_LIST_OPERATION_CODE.NODATA
                            End If
                        End Using
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER Select")
            CS0011LOGWRITE.INFSUBCLASS = "SetOrderNoFromT0004"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 配送受注ＤＢを検索し、車両情報を取得する         ★済　（意味不明　T4有無表示のみ）
    ''' </summary>
    ''' <param name="IO_ROW">対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub SetShabanCodeFromT0004(ByRef IO_ROW As DataRow, ByRef O_RTN As String)

        Dim SQLStr As String = ""
        Dim SQLWhere As String = ""
        Dim SQLSort As String = ""

        O_RTN = C_MESSAGE_NO.NORMAL

        Try
            Using WW_T0004tbl As New DataTable

                WW_T0004tbl.Columns.Add("SHARYOTYPEF", GetType(String))
                WW_T0004tbl.Columns.Add("TSHABANF", GetType(String))
                WW_T0004tbl.Columns.Add("SHARYOTYPEB", GetType(String))
                WW_T0004tbl.Columns.Add("TSHABANB", GetType(String))
                WW_T0004tbl.Columns.Add("SHARYOTYPEB2", GetType(String))
                WW_T0004tbl.Columns.Add("TSHABANB2", GetType(String))
                WW_T0004tbl.Columns.Add("ORDERORG", GetType(String))
                WW_T0004tbl.Columns.Add("CHKFLG", GetType(String))

                'DataBase接続文字
                Using SQLcon As SqlConnection = CS0050Session.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    SQLStr =
                               " SELECT isnull(rtrim(SHARYOTYPEF),'')  as SHARYOTYPEF " _
                             & "       ,isnull(rtrim(TSHABANF),'')     as TSHABANF " _
                             & "       ,isnull(rtrim(SHARYOTYPEB),'')  as SHARYOTYPEB " _
                             & "       ,isnull(rtrim(TSHABANB),'')     as TSHABANB " _
                             & "       ,isnull(rtrim(SHARYOTYPEB2),'') as SHARYOTYPEB2 " _
                             & "       ,isnull(rtrim(TSHABANB2),'')    as TSHABANB2 " _
                             & "       ,isnull(rtrim(ORDERORG),'')     as ORDERORG " _
                             & "       ,0                              as CHKFLG " _
                             & "   FROM T0004_HORDER " _
                             & "     WHERE    CAMPCODE        = @P01 " _
                             & "       and    SHIPORG         = @P02 " _
                             & "       and    TRIPNO          = @P03 " _
                             & "       and    DROPNO          = @P04 " _
                             & "       and    GSHABAN         = @P05 " _
                             & "       and    SHUKODATE       = @P06 " _
                             & "       and    DELFLG         <> '1'  " _
                             & " ORDER BY ORDERNO, DETAILNO, TRIPNO, DROPNO, SEQ"

                    SQLStr = SQLStr & SQLWhere & SQLSort

                    Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)

                        PARA1.Value = IO_ROW("CAMPCODE")
                        PARA2.Value = IO_ROW("SHIPORG")
                        PARA3.Value = IO_ROW("TRIPNO")
                        PARA4.Value = IO_ROW("DROPNO")
                        PARA5.Value = IO_ROW("GSHABAN")
                        PARA6.Value = IO_ROW("YMD")

                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            WW_T0004tbl.Load(SQLdr)

                            If WW_T0004tbl.Rows.Count > 0 Then
                                IO_ROW("SHARYOTYPEF") = WW_T0004tbl.Rows(0)("SHARYOTYPEF")
                                IO_ROW("TSHABANF") = WW_T0004tbl.Rows(0)("TSHABANF")
                                IO_ROW("SHARYOTYPEB") = WW_T0004tbl.Rows(0)("SHARYOTYPEB")
                                IO_ROW("TSHABANB") = WW_T0004tbl.Rows(0)("TSHABANB")
                                IO_ROW("SHARYOTYPEB2") = WW_T0004tbl.Rows(0)("SHARYOTYPEB2")
                                IO_ROW("TSHABANB2") = WW_T0004tbl.Rows(0)("TSHABANB2")
                                IO_ROW("ORDERORG") = WW_T0004tbl.Rows(0)("ORDERORG")
                            End If
                        End Using
                    End Using
                End Using
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "SetShabanCodeFromT0004"                  'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 配送受注ＤＢを検索し副搭乗員を取得する
    ''' </summary>
    ''' <param name="I_ROW">検索対象行</param>
    ''' <param name="I_B3_FIRST">B3初回フラグ</param>
    ''' <param name="O_STAFFCODE">副乗務員コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckTwoManCode(ByVal I_ROW As DataRow, ByVal I_B3_FIRST As String, ByRef O_STAFFCODE As String, ByRef O_RTN As String)

        Dim SQLStr As String = ""
        Dim SQLWhere As String = ""
        Dim SQLSort As String = ""

        O_RTN = C_MESSAGE_NO.NORMAL

        Try

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                SQLStr =
                           " SELECT isnull(rtrim(SUBSTAFFCODE),'') as SUBSTAFFCODE " _
                         & "   FROM T0004_HORDER " _
                         & "     WHERE    CAMPCODE        = @P01 " _
                         & "       and    SHIPORG         = @P02 " _
                         & "       and    SHUKODATE       = @P03 " _
                         & "       and    GSHABAN         = @P04 " _
                         & "       and    TRIPNO          = @P05 " _
                         & "       and    STAFFCODE       = @P06 " _
                         & "       and    DELFLG         <> '1'  "

                SQLStr = SQLStr & SQLWhere & SQLSort

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = I_ROW("CAMPCODE")
                    PARA2.Value = I_ROW("SHIPORG")
                    PARA3.Value = I_ROW("YMD")
                    PARA4.Value = I_ROW("GSHABAN")
                    If I_B3_FIRST = "ON" Then
                        'B3が先の場合、トリップ№－１で検索
                        PARA5.Value = (Val(I_ROW("TRIPNO")) - 1).ToString("000")
                    Else
                        PARA5.Value = I_ROW("TRIPNO")
                    End If
                    PARA6.Value = I_ROW("STAFFCODE")

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        O_STAFFCODE = String.Empty

                        If SQLdr.Read Then
                            O_STAFFCODE = SQLdr("SUBSTAFFCODE")
                        End If

                    End Using
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CheckTwoManCode"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 配送受注ＤＢより開始日を取得
    ''' </summary>
    ''' <param name="I_ROW">検索条件行</param>
    ''' <param name="O_STTIME">開始日</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub GetSTTimeForT0004(ByVal I_ROW As DataRow, ByRef O_STTIME As String, ByRef O_RTN As String)

        Dim SQLStr As String = ""
        Dim SQLWhere As String = ""
        Dim SQLSort As String = ""

        O_RTN = C_MESSAGE_NO.NORMAL

        Try

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                SQLStr =
                           " SELECT DISTINCT isnull(rtrim(STTIME),'') as STTIME " _
                         & "   FROM T0004_HORDER                                " _
                         & "     WHERE    CAMPCODE        = @P01                " _
                         & "       and    SHIPORG         = @P02                " _
                         & "       and    SHUKODATE       = @P03                " _
                         & "       and    GSHABAN         = @P04                " _
                         & "       and    DELFLG         <> '1'                 "

                SQLStr = SQLStr & SQLWhere & SQLSort

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = I_ROW("CAMPCODE")
                    PARA2.Value = I_ROW("SHIPORG")
                    PARA3.Value = I_ROW("YMD")
                    PARA4.Value = I_ROW("GSHABAN")

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        O_STTIME = ""

                        If SQLdr.Read Then
                            O_STTIME = SQLdr("STTIME")
                        End If

                    End Using

                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GetSTTimeForT0004"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 配送受注ＤＢを検索し、コンテナシャーシ取得（救済処置）
    ''' </summary>
    ''' <param name="IO_ROW">検索対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub GetChassisForT0004Tbl(ByRef IO_ROW As DataRow, ByRef O_RTN As String)

        Dim SQLStr As String = ""
        Dim SQLWhere As String = ""
        Dim SQLSort As String = ""
        O_RTN = C_MESSAGE_NO.DLL_IF_ERROR

        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                SQLStr =
                           " SELECT isnull(rtrim(CONTCHASSIS),'')   as CONTCHASSIS " _
                         & "       ,isnull(rtrim(TRIPNO),'')        as TRIPNO " _
                         & "   FROM T0004_HORDER "

                '会社、出荷部署、基準日、車番、従業員、油種
                SQLWhere =
                                   "     WHERE    CAMPCODE        = @P01 " _
                                 & "       and    SHIPORG         = @P02 " _
                                 & "       and    SHUKODATE       = @P03 " _
                                 & "       and    GSHABAN         = @P04 " _
                                 & "       and    STAFFCODE       = @P05 " _
                                 & "       and    DELFLG         <> '1'  "

                SQLSort = "ORDER BY TRIPNO,DROPNO,SEQ"

                SQLStr = SQLStr & SQLWhere & SQLSort

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = IO_ROW("CAMPCODE")
                    PARA2.Value = IO_ROW("SHIPORG")
                    PARA3.Value = IO_ROW("YMD") '初期値
                    If IO_ROW("WORKKBN") = "B3" Then
                        PARA3.Value = IO_ROW("TODOKEDATE")
                    ElseIf IO_ROW("WORKKBN") = "B2" Then
                        PARA3.Value = IO_ROW("YMD")
                    End If
                    PARA4.Value = IO_ROW("GSHABAN")
                    PARA5.Value = IO_ROW("STAFFCODE")

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        '同一トリップの場合、コンテナシャーシ取得
                        If SQLdr("TRIPNO") = IO_ROW("TRIPNO") Then
                            '○出力編集
                            IO_ROW("CONTCHASSIS") = SQLdr("CONTCHASSIS")
                            O_RTN = C_MESSAGE_NO.NORMAL
                            Exit While
                        End If
                    End While

                    If SQLdr.HasRows = False Then
                        O_RTN = C_MESSAGE_NO.DLL_IF_ERROR
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER Select")
            CS0011LOGWRITE.INFSUBCLASS = "GetChassisForT0004Tbl"                  'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' コード変換
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="I_VALUE">変換前コード値</param>
    ''' <param name="O_CONV">変換後コード値</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CodeToCode(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_CONV As String, ByRef O_RTN As String)
        O_CONV = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        If Not String.IsNullOrEmpty(I_VALUE) Then
            O_CONV = I_VALUE

            Select Case I_FIELD
                Case "TORICODEY"
                    '取引先（矢崎・変換）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateShepperY2G(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_CONV, O_RTN, param)

                Case "TORICODEK"
                    '取引先（光英・変換）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateShepperK2G(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_CONV, O_RTN, param)

                Case "TORICODE"
                    '取引先（マスタ）
                    O_CONV = I_VALUE

                Case "TODOKECODEY"
                    '届先（矢崎）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateConsigneeY2G(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "1", O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_CONV, O_RTN, param)

                Case "TODOKECODE"
                    '届先（マスタ）
                    O_CONV = I_VALUE

                Case "SHUKABASHOY"
                    '出荷場所名称（矢崎）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateConsigneeY2G(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "2", O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_CONV, O_RTN, param)

                Case "SHUKABASHO"
                    '出荷場所（マスタ）
                    O_CONV = I_VALUE

                Case "PRODUCT2Y"
                    '品名（矢崎）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateProdY2G(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_CONV, O_RTN, param)

                Case "PRODUCT2K"
                    '品名（光英）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateProdK2G(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_CONV, O_RTN, param)

                Case "PRODUCT2G2"
                    '品名（光英ENEX）品名２から品名コードを取得する
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateProdP2G(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_CONV, O_RTN, param)

                Case "PRODUCT2KE"
                    '品名（光英ENEX）９桁コードから品名コード作成
                    O_CONV = work.WF_SEL_CAMPCODE.Text & I_VALUE

                Case "PRODUCT2"
                    '品名（マスタ）
                    O_CONV = I_VALUE

                Case "GSHABANSHATANY"
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateSHABANY2G(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_CONV, O_RTN, param)

                Case "GSHABANSHATANK"
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateSHABANK2G(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_CONV, O_RTN, param)

                Case "GSHABAN"
                    O_CONV = I_VALUE
            End Select
        End If
    End Sub

    ''' <summary>
    ''' 名称設定処理   LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="I_VALUE">コード</param>
    ''' <param name="O_TEXT">名称</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CodeToName(ByVal I_FIELD As String,
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
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "WORKKBN"))
                Case "DELFLG"
                    '削除フラグ　DELFLG
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "TORICODEY"
                    '取引先名称（矢崎・変換）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateYazakiShipperList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "TORICODEK"
                    '取引先名称（光英・変換）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateKoeiShipperList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "TORICODE"
                    '取引先名称（マスタ）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateCustomerParam(work.WF_SEL_CAMPCODE.Text))

                Case "TODOKECODEY"
                    '届先名（矢崎）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateYazakiConsigneeList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "1", O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "TODOKECODE"
                    '届先名（マスタ）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "", "1"))

                Case "SHUKABASHOY"
                    '出荷場所名称（矢崎）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateYazakiConsigneeList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "2", O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "SHUKABASHO"
                    '出荷場所名称（マスタ）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "", "2"))

                Case "PRODUCT2Y"
                    '品名（矢崎）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateYazakiProdList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "PRODUCT2K"
                    '品名（光英）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateKoeiProdList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "PRODUCT2G2"
                    '品名（光英ENEX） 品名２コードより品名を取得
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateProduct2Lists(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "PRODUCT2KE"
                    '品名（光英ENEX） 9桁コードより品名を取得
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateProductLists(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, work.WF_SEL_CAMPCODE.Text & I_VALUE, O_TEXT, O_RTN, param)

                Case "PRODUCT2"
                    '品名（マスタ）
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateProductLists(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "STAFFCODE"
                    '乗務員名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.CreateSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text))

                Case "CAMPCODE"
                    '会社名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)

                Case "SHIPORG"
                    '出荷部署名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateShipORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.REFERLANCE))

                Case "TERMKBN"
                    '端末区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TERMKBN"))
                Case "JISSKIKBN"
                    '実績登録区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "JISSKIKBN"))
                Case "CREWKBN"
                    '乗務区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CREWKBN"))
                Case "TUMIOKIKBN"
                    '積置区分名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TUMIOKIKBN"))

                Case "GSHABANSHATANY"
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateYSHABANList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "GSHABANSHATANK"
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateKSHABANList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "GSHABAN"
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateTSHABANList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "MANGSHAFUKU"
                    '車腹
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateSHABAN2OILList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "MANGOILTYPE"
                    '油種
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateSHABAN2OILList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "URIKBN"
                    '売上計上基準名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "URIKBN"))

                Case "STANI"
                    '請求単位 PRODUCTCODE 2 TANNI
                    Dim param As New Hashtable
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    param.Item(GRIS0005LeftBox.C_PARAMETERS.LP_LIST) = work.CreateProduct2ClassList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text, O_RTN)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, param)

                Case "STANINAMES"
                    '請求単位
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "STANI"))

                Case "TAXKBN"
                    '税区分名称 TAXKBN
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TAXKBN"))

                Case "SUISOKBN"
                    '水素区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, work.CreateHydrogenParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN))

            End Select
        End If

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    'テーブル定義（カラム定義）
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    ''' テンポラリtblカラム設定
    ''' </summary>
    ''' <param name="IO_TBL">登録対象テーブル</param>
    ''' <param name="I_COLUMNCOUNT">カラム数</param>
    ''' <remarks></remarks>
    Protected Sub AddCsvColumn(ByRef IO_TBL As DataTable, ByVal I_COLUMNCOUNT As Integer)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then
            IO_TBL.Columns.Clear()
        End If

        'テンポラリDB項目作成
        IO_TBL.Clear()
        For i As Integer = 1 To I_COLUMNCOUNT
            IO_TBL.Columns.Add("FIELD" & i, GetType(String))         '項目フィールド
        Next

    End Sub

    ''' <summary>
    ''' ワークテーブル用カラム設定(光英用）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub AddColumnForT0005WKTbl()

        If IsNothing(T0005WKtbl) Then
            T0005WKtbl = New DataTable
        End If
        If T0005WKtbl.Columns.Count = 0 Then
        Else
            T0005WKtbl.Columns.Clear()
        End If

        'T0005DB項目作成
        T0005WKtbl.Clear()
        T0005WKtbl.Columns.Add("YMD", GetType(String))
        T0005WKtbl.Columns.Add("NIPPONO", GetType(String))
        T0005WKtbl.Columns.Add("STAFFCODE", GetType(String))
        T0005WKtbl.Columns.Add("STROW", GetType(String))
        T0005WKtbl.Columns.Add("ENDROW", GetType(Integer))
        T0005WKtbl.Columns.Add("STAFFCODE2", GetType(String))
        T0005WKtbl.Columns.Add("STROW2", GetType(Integer))
        T0005WKtbl.Columns.Add("ENDROW2", GetType(Integer))
        T0005WKtbl.Columns.Add("STDATE2", GetType(String))
        T0005WKtbl.Columns.Add("STTIME2", GetType(String))
        T0005WKtbl.Columns.Add("ENDDATE2", GetType(String))
        T0005WKtbl.Columns.Add("ENDTIME2", GetType(String))

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    'データ操作
    '★★★★★★★★★★★★★★★★★★★★★

    ''' <summary>
    '''  配送受注ＤＢを検索し、出荷場所を取得する（救済処置）
    ''' </summary>
    ''' <param name="IO_ROW"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub GetShukaBasho(ByRef IO_ROW As DataRow, ByRef O_RTN As String)

        Dim SQLStr As String = ""
        Dim SQLWhere As String = ""
        Dim SQLSort As String = ""
        O_RTN = C_MESSAGE_NO.DLL_IF_ERROR

        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                SQLStr =
                           " SELECT isnull(rtrim(SHUKABASHO),'')   as SHUKABASHO " _
                         & "   FROM T0004_HORDER "

                '会社、出荷部署、基準日、車番、従業員、油種
                SQLWhere =
                                   "     WHERE    CAMPCODE        = @P01 " _
                                 & "       and    SHIPORG         = @P02 " _
                                 & "       and    SHUKODATE       = @P03 " _
                                 & "       and    SHUKODATE       = SHUKADATE " _
                                 & "       and    GSHABAN         = @P04 " _
                                 & "       and    TUMIOKIKBN      = '1' " _
                                 & "       and    STAFFCODE       = @P05 " _
                                 & "       and    DELFLG         <> '1'  "

                SQLSort = "ORDER BY TRIPNO,DROPNO,SEQ"

                SQLStr = SQLStr & SQLWhere & SQLSort

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = IO_ROW("CAMPCODE")
                    PARA2.Value = IO_ROW("SHIPORG")
                    PARA3.Value = IO_ROW("YMD")
                    PARA4.Value = IO_ROW("GSHABAN")
                    PARA5.Value = IO_ROW("STAFFCODE")

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        If SQLdr.Read Then
                            '○出力編集
                            IO_ROW("SHUKABASHO") = SQLdr("SHUKABASHO")
                            O_RTN = C_MESSAGE_NO.NORMAL
                        End If

                        If SQLdr.HasRows = False Then
                            O_RTN = C_MESSAGE_NO.DLL_IF_ERROR
                        End If

                    End Using

                End Using

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER Select")
            CS0011LOGWRITE.INFSUBCLASS = "Get_SHUKABASHO"               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    '''  NJSの場合、届先部署マスタに登録されている出荷場所を取得する（最終救済処置）
    ''' </summary>
    ''' <param name="IO_ROW"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub GetShukaBashoNJS(ByRef IO_ROW As DataRow, ByRef O_RTN As String)

        Dim SQLStr As String = ""
        Dim SQLWhere As String = ""
        O_RTN = C_MESSAGE_NO.DLL_IF_ERROR

        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                SQLStr =
                           " SELECT isnull(rtrim(SHUKABASHO),'')   as SHUKABASHO " _
                         & "   FROM MC007_TODKORG " _
                         & "     WHERE    CAMPCODE     = @P01 " _
                         & "       and    TORICODE     = @P02 " _
                         & "       and    TODOKECODE   = @P03 " _
                         & "       and    UORG         = @P04 " _
                         & "       and    DELFLG      <> '1'  "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)

                    PARA1.Value = IO_ROW("CAMPCODE")
                    PARA2.Value = IO_ROW("wTORICODE")
                    PARA3.Value = IO_ROW("wTODOKECODE")
                    PARA4.Value = IO_ROW("SHIPORG")

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        If SQLdr.Read Then
                            '○出力編集
                            IO_ROW("SHUKABASHO") = SQLdr("SHUKABASHO")
                            O_RTN = C_MESSAGE_NO.NORMAL
                        End If

                        If SQLdr.HasRows = False Then
                            O_RTN = C_MESSAGE_NO.DLL_IF_ERROR
                        End If

                    End Using

                End Using

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC007_TODKORG Select")
            CS0011LOGWRITE.INFSUBCLASS = "Get_SHUKABASHO_NJS"           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC007_TODKORG Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    '''  MC006tbl（届先マスタ）編集
    ''' </summary>
    ''' <param name="I_MODE">登録モード</param>
    ''' <param name="I_LEGACY_MODE">レガシーモード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub EditMC006tbl(ByVal I_MODE As String, ByVal I_LEGACY_MODE As Boolean, ByRef O_RTN As String)


        Using SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open()
            'トランザクション
            Dim SQLtrn As SqlClient.SqlTransaction = Nothing

            O_RTN = C_MESSAGE_NO.NORMAL

            'トランザクション開始
            'SQLtrn = SQLcon.BeginTransaction

            MC006UPDATE.SQLcon = SQLcon
            MC006UPDATE.SQLtrn = SQLtrn
            MC006UPDATE.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            MC006UPDATE.UORG = work.WF_SEL_UORG.Text
            MC006UPDATE.UPDUSERID = Master.USERID
            MC006UPDATE.UPDTERMID = Master.USERTERMID
            MC006UPDATE.Update(I_MODE, I_LEGACY_MODE, KSYASAItbl)
            If Not isNormal(MC006UPDATE.ERR) Then
                Master.Output(MC006UPDATE.ERR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If
        End Using
    End Sub

    ''' <summary>
    ''' 日報ＤＢ取得
    ''' </summary>
    ''' <param name="I_ROW">条件対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks>未使用</remarks>
    Protected Sub GetTimeStampForT0005Tbl(ByVal I_ROW As DataRow, ByRef O_RTN As String)

        'オブジェクト内容検索
        Try
            Dim SQLStr As String = String.Empty
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                SQLStr =
                     "SELECT TIMSTP = cast(A.UPDTIMSTP  as bigint) " _
                    & " FROM T0005_NIPPO AS A					   " _
                    & " WHERE A.CAMPCODE         = @P01            " _
                    & "  and  A.SHIPORG          = @P02            " _
                    & "  and  A.TERMKBN          = @P03            " _
                    & "  and  A.YMD              = @P04            " _
                    & "  and  A.STAFFCODE        = @P05            " _
                    & "  and  A.DELFLG          <> '1'             "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
                    '○関連受注指定
                    PARA01.Value = I_ROW("CAMPCODE")
                    PARA02.Value = I_ROW("SHIPORG")
                    PARA03.Value = I_ROW("TERMKBN")
                    PARA04.Value = I_ROW("YMD")
                    PARA05.Value = I_ROW("STAFFCODE")

                    '■SQL実行
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        Dim WW_FIND As Boolean = False
                        While SQLdr.Read
                            If I_ROW("TIMSTP") = SQLdr("TIMSTP") Then
                                WW_FIND = True
                            End If
                        End While
                        If WW_FIND Then
                            O_RTN = C_MESSAGE_NO.NORMAL
                        Else
                            O_RTN = C_MESSAGE_NO.DLL_IF_ERROR
                        End If

                    End Using
                End Using
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0005_Select"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0005_NIPPO SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try
    End Sub

    ''' <summary>
    ''' T00004tbl登録処理
    ''' </summary>
    ''' <param name="I_SQLCON">DB接続情報</param>
    ''' <param name="I_SQLTTRN">トランザクション情報</param>
    ''' <param name="I_TBL">対象情報</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks>未使用</remarks>
    Protected Sub InsertT0004Tbl(ByRef I_SQLCON As SqlConnection, ByRef I_SQLTTRN As SqlTransaction,
                                    ByRef I_TBL As DataTable, ByRef O_RTN As String)

        Dim WW_DATENOW As Date = Date.Now
        Dim WW_SORTstr As String = ""
        Dim WW_FILLstr As String = ""

        Dim WW_TORICODE As String = ""
        Dim WW_OILTYPE As String = ""
        Dim WW_SHUKADATE As String = ""
        Dim WW_ORDERORG As String = ""
        Dim WW_SHIPORG As String = ""

        '■■■ T0004tblより配送受注追加 ■■■
        Try
            '〇配送受注DB登録
            Dim SQLStr As String =
                       " INSERT INTO T0004_HORDER                   " _
                     & "             (CAMPCODE,                     " _
                     & "              TERMORG,                      " _
                     & "              ORDERNO,                      " _
                     & "              DETAILNO,                     " _
                     & "              TRIPNO,                       " _
                     & "              DROPNO,                       " _
                     & "              SEQ,                          " _
                     & "              ENTRYDATE,                    " _
                     & "              TORICODE,                     " _
                     & "              OILTYPE,                      " _
                     & "              STORICODE,                    " _
                     & "              ORDERORG,                     " _
                     & "              SHUKODATE,                    " _
                     & "              KIKODATE,                     " _
                     & "              SHUKADATE,                    " _
                     & "              TUMIOKIKBN,                   " _
                     & "              URIKBN,                       " _
                     & "              STATUS,                       " _
                     & "              SHIPORG,                      " _
                     & "              SHUKABASHO,                   " _
                     & "              INTIME,                       " _
                     & "              OUTTIME,                      " _
                     & "              SHUKADENNO,                   " _
                     & "              TUMISEQ,                      " _
                     & "              TUMIBA,                       " _
                     & "              GATE,                         " _
                     & "              GSHABAN,                      " _
                     & "              RYOME,                        " _
                     & "              CONTCHASSIS,                  " _
                     & "              SHAFUKU,                      " _
                     & "              STAFFCODE,                    " _
                     & "              SUBSTAFFCODE,                 " _
                     & "              STTIME,                       " _
                     & "              TORIORDERNO,                  " _
                     & "              TODOKEDATE,                   " _
                     & "              TODOKETIME,                   " _
                     & "              TODOKECODE,                   " _
                     & "              PRODUCT1,                     " _
                     & "              PRODUCT2,                     " _
                     & "              PRATIO,                       " _
                     & "              SMELLKBN,                     " _
                     & "              CONTNO,                       " _
                     & "              SURYO,                        " _
                     & "              DAISU,                        " _
                     & "              JSURYO,                       " _
                     & "              JDAISU,                       " _
                     & "              REMARKS1,                     " _
                     & "              REMARKS2,                     " _
                     & "              REMARKS3,                     " _
                     & "              REMARKS4,                     " _
                     & "              REMARKS5,                     " _
                     & "              REMARKS6,                     " _
                     & "              DELFLG,                       " _
                     & "              INITYMD,                      " _
                     & "              UPDYMD,                       " _
                     & "              UPDUSER,                      " _
                     & "              UPDTERMID,                    " _
                     & "              RECEIVEYMD,                   " _
                     & "              KIJUNDATE,                    " _
                     & "              SHARYOTYPEF,                  " _
                     & "              TSHABANF,                     " _
                     & "              SHARYOTYPEB,                  " _
                     & "              TSHABANB,                     " _
                     & "              SHARYOTYPEB2,                 " _
                     & "              TSHABANB2,                    " _
                     & "              HTANI,                        " _
                     & "              STANI,                        " _
                     & "              TAXKBN)                       " _
                     & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,     " _
                     & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,     " _
                     & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,     " _
                     & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39,@P40,     " _
                     & "              @P41,@P42,@P43,@P44,@P45,@P46,@P47,@P48,@P49,@P50,     " _
                     & "              @P51,@P52,@P53,@P54,@P55,@P56,@P57,@P58,@P59,@P60,     " _
                     & "              @P61,@P62,@P63,@P64,@P65,@P66,@P67,@P68);         "

            Using SQLcmd As New SqlCommand(SQLStr, I_SQLCON, I_SQLTTRN)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 15)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 2)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 14)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.DateTime)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.DateTime)
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.Int)
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.Decimal)
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.DateTime)
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.NVarChar, 10)
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", System.Data.SqlDbType.Decimal)
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", System.Data.SqlDbType.Decimal)
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", System.Data.SqlDbType.Int)
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", System.Data.SqlDbType.Decimal)
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", System.Data.SqlDbType.Int)
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA52 As SqlParameter = SQLcmd.Parameters.Add("@P52", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA53 As SqlParameter = SQLcmd.Parameters.Add("@P53", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA54 As SqlParameter = SQLcmd.Parameters.Add("@P54", System.Data.SqlDbType.DateTime)
                Dim PARA55 As SqlParameter = SQLcmd.Parameters.Add("@P55", System.Data.SqlDbType.DateTime)
                Dim PARA56 As SqlParameter = SQLcmd.Parameters.Add("@P56", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA57 As SqlParameter = SQLcmd.Parameters.Add("@P57", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA58 As SqlParameter = SQLcmd.Parameters.Add("@P58", System.Data.SqlDbType.DateTime)
                Dim PARA59 As SqlParameter = SQLcmd.Parameters.Add("@P59", System.Data.SqlDbType.DateTime)
                Dim PARA60 As SqlParameter = SQLcmd.Parameters.Add("@P60", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA61 As SqlParameter = SQLcmd.Parameters.Add("@P61", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA62 As SqlParameter = SQLcmd.Parameters.Add("@P62", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA63 As SqlParameter = SQLcmd.Parameters.Add("@P63", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA64 As SqlParameter = SQLcmd.Parameters.Add("@P64", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA65 As SqlParameter = SQLcmd.Parameters.Add("@P65", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA66 As SqlParameter = SQLcmd.Parameters.Add("@P66", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA67 As SqlParameter = SQLcmd.Parameters.Add("@P67", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA68 As SqlParameter = SQLcmd.Parameters.Add("@P68", System.Data.SqlDbType.NVarChar, 20)

                '対象情報分繰り返し
                For Each WW_T0004UPDrow As DataRow In I_TBL.Rows

                    PARA01.Value = WW_T0004UPDrow("CAMPCODE")                           '会社コード(CAMPCODE)
                    PARA02.Value = WW_T0004UPDrow("TERMORG")                            '端末設置部署(TERMORG)
                    PARA03.Value = WW_T0004UPDrow("ORDERNO").PadLeft(7, "0")            '受注番号(ORDERNO)
                    PARA04.Value = WW_T0004UPDrow("DETAILNO").PadLeft(3, "0")           '明細№(DETAILNO)
                    PARA05.Value = WW_T0004UPDrow("TRIPNO").PadLeft(3, "0")             'トリップ(TRIPNO)
                    PARA06.Value = WW_T0004UPDrow("DROPNO").PadLeft(3, "0")             'ドロップ(DROPNO)
                    PARA07.Value = WW_T0004UPDrow("SEQ").PadLeft(2, "0")                '枝番(SEQ)
                    PARA08.Value = WW_DATENOW.ToString("yyyyMMddHHmmss")                'エントリー日時(ENTRYDATE)
                    PARA09.Value = WW_T0004UPDrow("TORICODE")                           '取引先コード(TORICODE)
                    PARA10.Value = WW_T0004UPDrow("OILTYPE")                            '油種(OILTYPE)
                    PARA11.Value = WW_T0004UPDrow("STORICODE")                          '請求取引先コード(STORICODE)
                    PARA12.Value = WW_T0004UPDrow("ORDERORG")                           '受注受付部署(ORDERORG)
                    If WW_T0004UPDrow("SHUKODATE") = "" Then                            '出庫日(SHUKODATE)
                        PARA13.Value = C_DEFAULT_YMD
                    Else
                        PARA13.Value = RTrim(WW_T0004UPDrow("SHUKODATE"))
                    End If
                    If WW_T0004UPDrow("KIKODATE") = "" Then                             '帰庫日(KIKODATE)
                        PARA14.Value = C_DEFAULT_YMD
                    Else
                        PARA14.Value = RTrim(WW_T0004UPDrow("KIKODATE"))
                    End If
                    If WW_T0004UPDrow("SHUKADATE") = "" Then                            '出荷日(SHUKADATE)
                        PARA15.Value = C_DEFAULT_YMD
                    Else
                        PARA15.Value = RTrim(WW_T0004UPDrow("SHUKADATE"))
                    End If
                    PARA16.Value = WW_T0004UPDrow("TUMIOKIKBN")                         '積置区分(TUMIOKIKBN)
                    PARA17.Value = WW_T0004UPDrow("URIKBN")                             '売上計上基準(URIKBN)
                    PARA18.Value = WW_T0004UPDrow("STATUS")                             '状態(STATUS)
                    PARA19.Value = WW_T0004UPDrow("SHIPORG")                            '出荷部署(SHIPORG)
                    PARA20.Value = WW_T0004UPDrow("SHUKABASHO")                         '出荷場所(SHUKABASHO)
                    PARA21.Value = WW_T0004UPDrow("INTIME")                             '時間指定（入構）(INTIME)
                    PARA22.Value = WW_T0004UPDrow("OUTTIME")                            '時間指定（出構）(OUTTIME)
                    PARA23.Value = WW_T0004UPDrow("SHUKADENNO")                         '出荷伝票番号(SHUKADENNO)
                    If String.IsNullOrWhiteSpace(RTrim(WW_T0004UPDrow("TUMISEQ"))) Then '積順(TUMISEQ)
                        PARA24.Value = 0
                    Else
                        PARA24.Value = WW_T0004UPDrow("TUMISEQ")
                    End If
                    PARA25.Value = WW_T0004UPDrow("TUMIBA")                             '積場(TUMIBA)
                    PARA26.Value = WW_T0004UPDrow("GATE")                               'ゲート(GATE)
                    PARA27.Value = WW_T0004UPDrow("GSHABAN")                            '業務車番(GSHABAN)
                    PARA28.Value = WW_T0004UPDrow("RYOME")                              '両目(RYOME)
                    PARA29.Value = WW_T0004UPDrow("CONTCHASSIS")                        'コンテナシャーシ(CONTCHASSIS)
                    If String.IsNullOrWhiteSpace(RTrim(WW_T0004UPDrow("SHAFUKU"))) Then '車腹（積載量）(SHAFUKU)
                        PARA30.Value = 0.0
                    Else
                        PARA30.Value = CType(WW_T0004UPDrow("SHAFUKU"), Double)
                    End If
                    PARA31.Value = WW_T0004UPDrow("STAFFCODE")                          '乗務員コード(STAFFCODE)
                    PARA32.Value = WW_T0004UPDrow("SUBSTAFFCODE")                       '副乗務員コード(SUBSTAFFCODE)
                    PARA33.Value = WW_T0004UPDrow("STTIME")                             '出勤時間(STTIME)
                    PARA34.Value = ""                                                 '荷主受注番号(TORIORDERNO)
                    If RTrim(WW_T0004UPDrow("TODOKEDATE")) = "" Then                    '届日(TODOKEDATE)
                        PARA35.Value = C_DEFAULT_YMD
                    Else
                        PARA35.Value = RTrim(WW_T0004UPDrow("TODOKEDATE"))
                    End If
                    PARA36.Value = WW_T0004UPDrow("TODOKETIME")                         '時間指定（配送）(TODOKETIME)
                    PARA37.Value = WW_T0004UPDrow("TODOKECODE")                         '届先コード(TODOKECODE)
                    PARA38.Value = WW_T0004UPDrow("PRODUCT1")                           '品名１(PRODUCT1)
                    PARA39.Value = WW_T0004UPDrow("PRODUCT2")                           '品名２(PRODUCT2)
                    If String.IsNullOrWhiteSpace(RTrim(WW_T0004UPDrow("PRATIO"))) Then  'Ｐ比率(PRATIO)
                        PARA40.Value = 0.0
                    Else
                        PARA40.Value = CType(WW_T0004UPDrow("PRATIO"), Double)
                    End If
                    PARA41.Value = WW_T0004UPDrow("SMELLKBN")                           '臭有無(SMELLKBN)
                    PARA42.Value = WW_T0004UPDrow("CONTNO")                             'コンテナ番号(CONTNO)
                    If String.IsNullOrWhiteSpace(RTrim(WW_T0004UPDrow("SURYO"))) Then   '数量(SURYO)
                        PARA43.Value = 0.0
                    Else
                        PARA43.Value = CType(WW_T0004UPDrow("SURYO"), Double)
                    End If
                    If String.IsNullOrWhiteSpace(RTrim(WW_T0004UPDrow("DAISU"))) Then   '台数(DAISU)
                        PARA44.Value = 0
                    Else
                        PARA44.Value = CType(WW_T0004UPDrow("DAISU"), Double)
                    End If
                    If String.IsNullOrWhiteSpace(RTrim(WW_T0004UPDrow("JSURYO"))) Then  '数量(JSURYO)
                        PARA45.Value = 0.0
                    Else
                        PARA45.Value = CType(WW_T0004UPDrow("JSURYO"), Double)          '配送実績数量(JSURYO)
                    End If
                    If String.IsNullOrWhiteSpace(RTrim(WW_T0004UPDrow("JDAISU"))) Then  '台数(JDAISU)
                        PARA46.Value = 0
                    Else
                        PARA46.Value = CType(WW_T0004UPDrow("DAISU"), Double)
                    End If
                    PARA47.Value = WW_T0004UPDrow("REMARKS1")                           '備考１(REMARKS1)
                    PARA48.Value = WW_T0004UPDrow("REMARKS2")                           '備考２(REMARKS2)
                    PARA49.Value = WW_T0004UPDrow("REMARKS3")                           '備考３(REMARKS3)
                    PARA50.Value = WW_T0004UPDrow("REMARKS4")                           '備考４(REMARKS4)
                    PARA51.Value = WW_T0004UPDrow("REMARKS5")                           '備考５(REMARKS5)
                    PARA52.Value = WW_T0004UPDrow("REMARKS6")                           '備考６(REMARKS6)
                    PARA53.Value = WW_T0004UPDrow("DELFLG")                             '削除フラグ(DELFLG)
                    PARA54.Value = WW_DATENOW                                           '登録年月日(INITYMD)
                    PARA55.Value = WW_DATENOW                                           '更新年月日(UPDYMD)
                    PARA56.Value = CS0050Session.USERID                                 '更新ユーザＩＤ(UPDUSER)
                    PARA57.Value = CS0050Session.TERMID                                 '更新端末(UPDTERMID)
                    PARA58.Value = C_DEFAULT_YMD                                        '集信日時(RECEIVEYMD)
                    '基準日＝出荷日 7/11
                    If WW_T0004UPDrow("KIJUNDATE") = "" Then                            '基準日(KIJUNDATE)
                        PARA59.Value = C_DEFAULT_YMD
                    Else
                        PARA59.Value = RTrim(WW_T0004UPDrow("KIJUNDATE"))
                    End If
                    PARA60.Value = WW_T0004UPDrow("SHARYOTYPEF")                        '統一車番(SHARYOTYPEF)
                    PARA61.Value = WW_T0004UPDrow("TSHABANF")                           '統一車番(TSHABANF)
                    PARA62.Value = WW_T0004UPDrow("SHARYOTYPEB")                        '統一車番(SHARYOTYPEB)
                    PARA63.Value = WW_T0004UPDrow("TSHABANB")                           '統一車番(TSHABANB)
                    PARA64.Value = WW_T0004UPDrow("SHARYOTYPEB2")                       '統一車番(SHARYOTYPEB2)
                    PARA65.Value = WW_T0004UPDrow("TSHABANB2")                          '統一車番(TSHABANB2)
                    PARA66.Value = WW_T0004UPDrow("HTANI")                              '配送単位(HTANI)
                    PARA67.Value = WW_T0004UPDrow("STANI")                              '配送実績単位(STANI)
                    PARA68.Value = WW_T0004UPDrow("TAXKBN")                             '税区分(TAXKBN)

                    SQLcmd.ExecuteNonQuery()
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER INSERT"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try
    End Sub

    ''' <summary>
    ''' 車両 管理、設置部署 , 荷主　取得  
    ''' </summary>
    ''' <param name="I_UORG"></param>
    ''' <param name="I_GSHABAN"></param>
    ''' <param name="I_YMD"></param>
    ''' <param name="O_MORG"></param>
    ''' <param name="O_SORG"></param>
    ''' <param name="O_SUPPL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks>未使用</remarks>
    Protected Sub GetOrgCodeForMA001(ByVal I_UORG As String,
                               ByVal I_GSHABAN As String,
                               ByVal I_YMD As String,
                               ByRef O_MORG As String,
                               ByRef O_SORG As String,
                               ByRef O_SUPPL As String,
                               ByRef O_RTN As String)
        Try
            O_RTN = C_MESSAGE_NO.NORMAL
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)
                Dim SQLStr As String =
                           " SELECT rtrim(B.MANGMORG) as MANGMORG , rtrim(A.MANGUORG) as MANGUORG, rtrim(A.MANGSUPPL) as MANGSUPPL " _
                         & "     FROM MA006_SHABANORG A " _
                         & "     INNER JOIN MA002_SHARYOA B " _
                         & "       ON    B.CAMPCODE    = A.CAMPCODE " _
                         & "       and   B.SHARYOTYPE  = A.SHARYOTYPEF " _
                         & "       and   B.TSHABAN     = A.TSHABANF " _
                         & "       and   B.STYMD      <= @P05 " _
                         & "       and   B.ENDYMD     >= @P04 " _
                         & "       and   B.DELFLG     <> '1' " _
                         & "     WHERE   A.CAMPCODE    = @P01 " _
                         & "       and   A.MANGUORG    = @P02 " _
                         & "       and   A.GSHABAN     = @P03 " _
                         & "       and   A.DELFLG     <> '1' "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = I_UORG
                    PARA3.Value = I_GSHABAN
                    PARA4.Value = I_YMD
                    PARA5.Value = I_YMD
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        If SQLdr.Read Then
                            O_MORG = SQLdr("MANGMORG")
                            O_SORG = SQLdr("MANGUORG")
                            O_SUPPL = SQLdr("MANGSUPPL")
                        End If

                    End Using
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA006_SHABANORG SELECT")

            CS0011LOGWRITE.INFSUBCLASS = "MA001_GetOrg"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA006_SHABANORG Select"        '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 従業員 管理、作業部署 取得 
    ''' </summary>
    ''' <param name="I_STAFFCODE"></param>
    ''' <param name="I_YMD"></param>
    ''' <param name="O_HORG"></param>
    ''' <param name="O_SORG"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks>未使用</remarks>
    Protected Sub GetOrgCodeForMB001(ByVal I_STAFFCODE As String,
                               ByVal I_YMD As String,
                               ByRef O_HORG As String,
                               ByRef O_SORG As String,
                               ByRef O_RTN As String)
        Try
            O_RTN = C_MESSAGE_NO.NORMAL
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)
                Dim SQLStr As String =
                           " SELECT                                     " _
                         & "               rtrim(A.HORG)     as HORG ,  " _
                         & "               rtrim(B.SORG)     as SORG    " _
                         & "     FROM       MB001_STAFF        A        " _
                         & "     INNER JOIN MB002_STAFFORG     B        " _
                         & "       ON    B.CAMPCODE    = A.CAMPCODE     " _
                         & "       and   B.STAFFCODE   = A.STAFFCODE    " _
                         & "       and   B.DELFLG     <> '1'            " _
                         & "     WHERE   A.CAMPCODE    = @P01           " _
                         & "       and   A.STAFFCODE   = @P02           " _
                         & "       and   A.STYMD      <= @P04           " _
                         & "       and   A.ENDYMD     >= @P03           " _
                         & "       and   A.DELFLG     <> '1'            "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = I_STAFFCODE
                    PARA3.Value = I_YMD
                    PARA4.Value = I_YMD
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        If SQLdr.Read Then
                            O_HORG = SQLdr("HORG")
                            O_SORG = SQLdr("SORG")
                        End If

                    End Using

                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB001_STAFF SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MB001_GetOrg"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 届先の取引先コードを取得
    ''' </summary>
    ''' <param name="I_TODOKECODE"></param>
    ''' <param name="I_YMD"></param>
    ''' <param name="O_TORICODE"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub GetToriCodeForMC006(ByVal I_TODOKECODE As String,
                               ByVal I_YMD As String,
                               ByRef O_TORICODE As String,
                               ByRef O_RTN As String)
        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)
                Dim SQLStr As String =
                           " SELECT rtrim(A.TORICODE) as TORICODE " _
                         & "     FROM MC006_TODOKESAKI A " _
                         & "     WHERE   A.CAMPCODE    = @P01 " _
                         & "       and   A.TODOKECODE  = @P02 " _
                         & "       and   A.STYMD      <= @P04 " _
                         & "       and   A.ENDYMD     >= @P03 " _
                         & "       and   A.DELFLG     <> '1' "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = I_TODOKECODE
                    PARA3.Value = I_YMD
                    PARA4.Value = I_YMD
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        Dim WW_TORICODE As String = ""
                        If SQLdr.Read Then
                            WW_TORICODE = SQLdr("TORICODE")
                        End If

                        O_TORICODE = WW_TORICODE
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC006_TODOKESAKI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MC006_GetTORI"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 条件抽出画面情報退避
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub MapRefelence(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00005S Then                                                    '条件画面からの画面遷移
            '○Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

            work.WF_T5I_LINECNT.Text = ""
            work.WF_T5I_GridPosition.Text = "1"
            work.WF_T5I_STAFFCODE.Text = ""
            work.WF_T5I_YMD.Text = ""
            work.WF_T5_ERRMSG.Text = ""

            '○T0005tbl情報保存先のファイル名
            work.WF_SEL_XMLsaveF.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
                                    CS0050Session.USERID & "-T00005I-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"
            work.WF_SEL_XMLsaveF9.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
                                    CS0050Session.USERID & "-T00005I9-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"
            '■■■ 選択画面の入力初期値設定 ■■■
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00005 Then
            '勤怠個別画面から遷移した場合
            '保存しておいた、GridViewの表示開始位置、絞込み条件の乗務員、日付を設定し直す
            WF_STAFFCODE.Text = work.WF_T5I_STAFFCODE.Text
            WF_YMD.Text = work.WF_T5I_YMD.Text

        End If

        '勤怠締テーブル取得
        Dim WW_LIMITFLG As String = "0"
        T0007COM.T00008get(work.WF_SEL_CAMPCODE.Text,
                           work.WF_SEL_UORG.Text,
                           CDate(work.WF_SEL_STYMD.Text).ToString("yyyy/MM"),
                           WW_LIMITFLG,
                           WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End If

        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            If WW_LIMITFLG = "0" Then
                '対象月の締前は更新ＯＫ
                WF_MAPpermitcode.Value = "TRUE"

                ''自分の部署と選択した配属部署が同一なら更新可能
                'If work.WF_SEL_UORG.Text = work.WF_SEL_PERMIT_ORG.Text Then
                '    WF_MAPpermitcode.Value = "TRUE"
                'Else
                '    WF_MAPpermitcode.Value = "FALSE"
                'End If
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If
        Else
            WF_MAPpermitcode.Value = "FALSE"
        End If

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="WW_LINEerr"></param>
    ''' <param name="T0005INProw"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub OutputErrorMessage(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByRef WW_LINEerr As String, ByRef T0005INProw As DataRow, ByVal I_ERRCD As String)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 項番        =@L" & T0005INProw("YMD") & T0005INProw("STAFFCODE") & "L@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日      =" & T0005INProw("YMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員コード=" & T0005INProw("STAFFCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員      =" & T0005INProw("STAFFNAMES") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 作業区分    =" & T0005INProw("WORKKBNNAMES") & "  "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日報№      =" & T0005INProw("NIPPONO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細行番号  =" & T0005INProw("SEQ") & " , "
        SetErrorMessage(WW_ERR_MES)
        WW_ERRLIST_ALL.Add(I_ERRCD)
        WW_ERRLIST.Add(I_ERRCD)
        If WW_LINEerr <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            WW_LINEerr = I_ERRCD
        End If

    End Sub

    ''' <summary>
    ''' エラーメッセージ編集
    ''' </summary>
    ''' <param name="I_MSG"></param>
    ''' <remarks></remarks>
    Sub SetErrorMessage(ByVal I_MSG As String)

        If WW_ERRLISTCNT <= 4000 Then
            rightview.AddErrorReport(ControlChars.NewLine & I_MSG)

            WW_ERRLISTCNT += I_MSG.Length - I_MSG.Replace(vbCr, "").Length + 1

            If WW_ERRLISTCNT > 4000 Then
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "※エラーが4000行超のため出力を停止しました。"
                rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
            End If

        End If
    End Sub
End Class